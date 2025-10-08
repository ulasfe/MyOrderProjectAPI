using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.Data;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderDetailsDTO>> GetActiveOrdersAsync()
        {
            // Yalnızca Kapalı olmayan siparişleri filtrele.
            return await _context.Orders
                .Where(o => o.Status != orderStatus.Kapali && o.Status != orderStatus.Iptal)
                //Bağlı olan bütün verileri çek.
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Select(o => MapToOrderDetailsDTO(o))
                .ToListAsync();
        }

        public async Task<OrderDetailsDTO?> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            return MapToOrderDetailsDTO(order);
        }
        public async Task<OrderDetailsDTO> CreateOrderAsync(OrderCreateDTO orderDTO)
        {
            //Masa Durumu Kontrolü işlemi
            var table = await _context.Tables.FindAsync(orderDTO.TableId);
            if (table == null || !table.RecordStatus) throw new InvalidOperationException("Masa bulunamadı.");

            if (table.Status == Status.Dolu)
            {
                // MAsa doluysa hata mesajı dön.
                throw new InvalidOperationException("Masa zaten dolu veya aktif bir siparişe sahip.");
            }

            //Masa müsaitse yeni sipariş açma işlemi.
            var order = new Order
            {
                TableId = orderDTO.TableId,
                Status = orderStatus.Acik,
                OrderDate = DateTime.UtcNow
            };
            _context.Orders.Add(order);

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            //Sipariş itemlerini oluştur ve stokları kontrol etme işlemi.
            if (orderDTO.Items is null || orderDTO.Items.Count == 0)
                throw new ArgumentException("Sipariş kalemleri olmadan sipariş oluşturamazsınız");
            foreach (var itemDTO in orderDTO.Items)
            {
                var product = await _context.Products.FindAsync(itemDTO.ProductId);
                if (product == null) throw new InvalidOperationException($"Ürün ID {itemDTO.ProductId} bulunamadı.");

                if (product.StockQuantity < itemDTO.Quantity)
                    throw new InvalidOperationException($"Ürün {product.Name} için yeterli stok yok.");

                //Birim fiyatı al.
                var unitPrice = product.Price;

                var orderItem = new OrderItem
                {
                    ProductId = itemDTO.ProductId,
                    Quantity = itemDTO.Quantity,
                    UnitPriceAtTimeOfOrder = unitPrice,
                    OrderId = order.Id
                };
                orderItems.Add(orderItem);

                //O an'daki mevcut toplam ödeme bilgisi
                totalAmount += unitPrice * itemDTO.Quantity;

                // Siparişe ekleme sonrası ürünğ stoktan düş.
                product.StockQuantity -= itemDTO.Quantity;
            }

            order.OrderItems = orderItems;
            order.TotalAmount = totalAmount;

            //Sistemde ilgili masanın durumunu dolu olarak değiştir.
            table.Status = Status.Dolu;

            await _context.SaveChangesAsync();

            return await GetOrderByIdAsync(order.Id) ?? throw new Exception("Sipariş oluşturulamadı.");
        }


        public async Task<bool> ProcessPaymentAsync(int orderId, decimal amount, paymentMethod method)
        {
            var order = await _context.Orders
                .Include(o => o.Payments)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.Status == orderStatus.Kapali || order.Status == orderStatus.Iptal) return false;

            var payment = new Payment
            {
                OrderId = orderId,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = method
            };
            _context.Payments.Add(payment);

            //Siparişin toplam ödenen miktarını hesapla 1 den fazla ödeme alabilmek için.
            var totalPaid = order.Payments.Sum(p => p.Amount);

            // Ödenen miktar o an'daki toplan tutardan büyük ya da eşitse siparişi kapat.
            if (totalPaid >= order.TotalAmount)
            {
                order.Status = orderStatus.Kapali;
                //Ödeme sonrası masanın durumunu tekrar boş olarak güncelle
                if (order.Table != null)
                {
                    order.Table.Status = Status.Boş;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.Status == orderStatus.Iptal || order.Status == orderStatus.Kapali) return false;

            //Siparişin durumunu iptal olarak değiştir
            order.Status = orderStatus.Iptal;

            //Sipariş için girilmiş olan ürünlerin stok değerlerini eski haline getir.
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }

            // İptal olan siparişe bağlı olan masanın durumunu boş olarak değiştir
            if (order.Table != null)
            {
                order.Table.Status = Status.Boş;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OrderDetailsDTO?> AddItemsToOrderAsync(int orderId, List<OrderItemDTO> newItems)
        {

            var orderDTO = await _context.Orders.FindAsync(orderId);
            if (orderDTO == null) throw new InvalidOperationException("Sipariş bulunamadı.");

            if (orderDTO.Table.Status is not Status.Dolu) throw new InvalidOperationException($"{orderId} id numaralı siparişin masa durumu dolu olması gerekirken {orderDTO.Table.Status.ToString()} olarak görünüyor.");

            decimal totalAmount = orderDTO.TotalAmount;

            foreach (var newItem in newItems)
            {
                var produdt = await _context.Products.FindAsync(newItem.ProductId);
                if (produdt == null) throw new InvalidOperationException($"Ürün ID {newItem.ProductId} bulunamadı.");

                if (produdt.StockQuantity < newItem.Quantity)
                    throw new InvalidOperationException($"Ürün {produdt.Name} için yeterli stok yok.");

                //Birim fiyatı al.
                var unitPrice = produdt.Price;

                var orderItem = new OrderItem
                {
                    ProductId = newItem.ProductId,
                    Quantity = newItem.Quantity,
                    UnitPriceAtTimeOfOrder = unitPrice,
                    OrderId = orderDTO.Id
                };
                orderDTO.OrderItems.Add(orderItem);

                //O an'daki mevcut toplam ödeme bilgisi
                totalAmount += unitPrice * newItem.Quantity;

                // Siparişe ekleme sonrası ürünğ stoktan düş.
                produdt.StockQuantity -= newItem.Quantity;
            }

            orderDTO.TotalAmount = totalAmount;
            _context.SaveChanges();
            return await GetOrderByIdAsync(orderId) ?? throw new Exception("Ürünler eklenirken bir hata oluştu.");
        }

        public async Task<bool> RetrieveOrderAsync(int orderId)
        {
            try
            {

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Table)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                // 1. Sipariş bulunamadıysa veya zaten Açık/Kapalı ise geri yüklenemez.
                if (order == null || order.Status != orderStatus.Iptal)
                {
                    // Eğer zaten Açık veya Kapalı ise bir işlem yapmayız, ya da NotFound/Conflict fırlatılabilir.
                    // Controller'a false dönelim ve Controller 404/409'u yönetsin.
                    return false;
                }

                var originalStatus = order.Status;

                // 2. Siparişin durumunu Açık olarak değiştir.
                order.Status = orderStatus.Acik;

                // 3. Stokları tekrar düş (Çünkü iptalde eklenmişti).
                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        // Stok kontrolü yapılması kritik! Yeterli stok yoksa exception fırlatılmalıdır.
                        if (product.StockQuantity < item.Quantity)
                        {
                            order.Status = originalStatus;
                            // Stok yetersizliği durumunda işlemi geri al, logla ve hata fırlat.
                            throw new InvalidOperationException($"Sipariş geri yüklenirken, Ürün {product.Name} için yeterli stok ({item.Quantity}) bulunamadı. Geri yükleme iptal edildi.");
                        }
                        product.StockQuantity -= item.Quantity;
                    }
                }

                // 4. Masanın durumunu tekrar Dolu olarak değiştir (Eğer Boş'sa).
                if (order.Table != null && order.Table.Status == Status.Boş)
                {
                    order.Table.Status = Status.Dolu;
                }

                await _context.SaveChangesAsync();
                return true;

            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("Sipariş geri alınırken bir hata oluştu!");
            }
        }

        /// <summary>
        /// Yardımcı ampleme işlemi için kullanlılır
        /// </summary>
        /// <param name="order">Model alır</param>
        /// <returns>Data Transfer Object döndürür.</returns>
        private static OrderDetailsDTO MapToOrderDetailsDTO(Order order)
        {
            return new OrderDetailsDTO
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TableId = order.TableId,
                TableNumber = order.Table?.TableNumber ?? "Silinmiş Masa",
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemDetailDTO
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "Silinmiş Ürün",
                    Quantity = oi.Quantity,
                    UnitPriceAtTimeOfOrder = oi.UnitPriceAtTimeOfOrder
                }).ToList(),
                Payments = order.Payments.Select(p => new PaymentDetailDTO
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod
                }).ToList()
            };
        }
    }
}
