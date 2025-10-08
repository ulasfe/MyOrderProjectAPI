using FluentAssertions;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;
using MyOrderProjectAPI.Services;
using MyOrderProjectAPI.Tests.Base;

namespace MyOrderProjectAPI.Tests.ServiceTests
{
    public class OrderServiceTests : BaseTest
    {
        private readonly OrderService _orderService;

        public OrderServiceTests() : base()
        {
            _orderService = new OrderService(_context);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnNull_ForNonExistentId()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _orderService.GetOrderByIdAsync(nonExistentId);

            // Assert: Sipariş bulunamadığında null dönmeli.
            result.Should().BeNull();
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldReturnFalse_WhenOrderNotFound()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _orderService.ProcessPaymentAsync(nonExistentId, 10, paymentMethod.Nakit);

            // Assert: Sipariş bulunamadığı için false dönmeli.
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldReturnFalse_WhenOrderNotFound()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _orderService.CancelOrderAsync(nonExistentId);

            // Assert: Sipariş bulunamadığı için false dönmeli.
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AddItemsToOrderAsync_ShouldThrowInvalidOperationException_WhenOrderNotFound()
        {
            // Arrange
            int nonExistentId = 999;
            var newItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } };

            // Act & Assert: Service kodunuz burada InvalidOperationException fırlatıyor.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.AddItemsToOrderAsync(nonExistentId, newItems)
            );
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldReturnOrderDetailDTO()
        {
            var dummyOrderItems = new List<OrderItemDTO>
            {
                new OrderItemDTO { ProductId = 1, Quantity = 2 },
                new OrderItemDTO { ProductId = 2, Quantity = 1 }
            };

            var newOrder = GenerateDummyOrders(1, dummyOrderItems);
            var orderTable = await _context.Tables.FindAsync(1);
            orderTable.Should().NotBeNull();
            var detailDTO = await _orderService.CreateOrderAsync(newOrder);

            detailDTO.Should().NotBeNull();
            detailDTO.TotalAmount.Should().Be(60);
            detailDTO.TableNumber.Should().Be(orderTable.TableNumber);
        }


        [Fact]
        public async Task CancelOrderAsync_ShouldReturnTrue()
        {
            int orderIdToDelete = 1;
            await CreateOrderAsync_ShouldReturnOrderDetailDTO();
            var result = await _orderService.CancelOrderAsync(orderIdToDelete);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AddItemsToOrderAsync_ShoudlReturnOrderDetailDTO()
        {
            int orderIdToModify = 1;
            await CreateOrderAsync_ShouldReturnOrderDetailDTO(); //Burada Toplam tutat 60 olmalı halihazırda 
            var newItems = new List<OrderItemDTO>{
                new OrderItemDTO { ProductId = 1 , Quantity = 3}, // Ütün fiyatı 10 toplam tutara 10 * 3 = 30 daha eklenmeli
                new OrderItemDTO { ProductId = 2 , Quantity = 2}, // Ürün fiyatı 40 toplam tutara 40 * 2 = 80 daha eklenmeli
            };

            var orderDetailsDTO = await _orderService.AddItemsToOrderAsync(orderIdToModify, newItems);

            orderDetailsDTO.Should().NotBeNull();
            orderDetailsDTO.Status.Should().Be(orderStatus.Acik);
            orderDetailsDTO.TotalAmount.Should().Be(170); // 60(önceden) +  30 + 80 = 170 olmalı

        }

        [Fact]
        public async Task GetActiveOrdersAsync_ShouldReturnOrderDetailsDTOList()
        {
            var activeOrders = await _orderService.GetActiveOrdersAsync();
            activeOrders.Count().Should().Be(_context.Orders.Count());
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldReturnTrue()
        {
            await AddItemsToOrderAsync_ShoudlReturnOrderDetailDTO();
            var susccessPayment = await _orderService.ProcessPaymentAsync(1, 170, paymentMethod.KrediKarti);
            susccessPayment.Should().BeTrue();
        }
        [Fact]
        public async Task CreateOrderAsync_ShouldThrowException_WhenItemsListIsEmpty()
        {

            var emptyItemsDto = new OrderCreateDTO
            {
                TableId = 1,
                Items = new List<OrderItemDTO>()
            };

            await Assert.ThrowsAsync<ArgumentException>(
                () => _orderService.CreateOrderAsync(emptyItemsDto)
            );
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowInvalidOperationException_WhenTableNotFound()
        {
            // Arrange: Mevcut olmayan bir masa ID'si (örn: 99)
            var nonExistentOrder = GenerateDummyOrders(99, new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } });

            // Act & Assert: "Masa bulunamadı." hatası bekleriz.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(nonExistentOrder)
            );
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowInvalidOperationException_WhenTableIsAlreadyDolu()
        {
            // Arrange: Masanın durumunu manuel olarak "Dolu" yap
            var fullTableId = 3;
            var table = await _context.Tables.FindAsync(fullTableId);
            table.Should().NotBeNull();
            table.Status = Status.Dolu;
            await _context.SaveChangesAsync();

            var orderDTO = GenerateDummyOrders(fullTableId, new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } });

            // Act & Assert: "Masa zaten dolu veya aktif bir siparişe sahip." hatası bekleriz.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(orderDTO)
            );
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowInvalidOperationException_WhenProductNotFound()
        {
            // Arrange: Var olmayan ProductId (örn: 999) kullanıyoruz
            var orderDTO = GenerateDummyOrders(1, new List<OrderItemDTO> { new OrderItemDTO { ProductId = 999, Quantity = 1 } });

            // Act & Assert: "Ürün ID 999 bulunamadı." hatası bekleriz.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(orderDTO)
            );
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowInvalidOperationException_WhenInsufficientStock()
        {
            // Arrange: Ürün 1'in stoğu 45. 46 adet sipariş etmeye çalışıyoruz.
            var orderDTO = GenerateDummyOrders(1, new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 46 } });
            var product = await _context.Products.FindAsync(1);

            // Act & Assert: Yeterli stok yok hatası bekleriz.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(orderDTO)
            );
        }
          
        [Fact]
        public async Task ProcessPaymentAsync_ShouldReturnFalse_WhenOrderIsAlreadyClosed()
        {
            // Arrange: Önce bir sipariş oluştur, sonra Kapalı yap
            var orderDTO = await CreateAndReturnDummyOrderAsync(2);
            var order = await _context.Orders.FindAsync(orderDTO.Id);
            order.Status = orderStatus.Kapali;
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.ProcessPaymentAsync(order.Id, 10, paymentMethod.Nakit);

            // Assert: Kapalı siparişe ödeme yapılmamalı
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldReturnFalse_WhenOrderIsAlreadyCanceled()
        {
            // Arrange: Önce bir sipariş oluştur, sonra İptal yap
            var orderDTO = await CreateAndReturnDummyOrderAsync(4);
            var order = await _context.Orders.FindAsync(orderDTO.Id);
            order.Status = orderStatus.Iptal;
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.ProcessPaymentAsync(order.Id, 10, paymentMethod.Nakit);

            // Assert: İptal edilmiş siparişe ödeme yapılmamalı
            result.Should().BeFalse();
        }

        /// <summary>
        /// Testler için temel, aktif bir sipariş oluşturur ve DTO olarak döndürür.
        /// Bu metot, Service'de DEĞİL, sadece Test Sınıfı içinde kullanılır.
        /// </summary>
        private async Task<OrderDetailsDTO> CreateAndReturnDummyOrderAsync(int tableId)
        {
            var dummyOrderItems = new List<OrderItemDTO>
            {
                new OrderItemDTO { ProductId = 1, Quantity = 2 },
                new OrderItemDTO { ProductId = 2, Quantity = 1 }
            };

            var newOrder = GenerateDummyOrders(tableId, dummyOrderItems);

            return await _orderService.CreateOrderAsync(newOrder);
        }
         

        [Fact]
        public async Task CancelOrderAsync_ShouldReturnFalse_WhenOrderIsAlreadyCanceled()
        {
            // Arrange: Önce sipariş oluştur, sonra iptal et
            var orderDTO = await CreateAndReturnDummyOrderAsync(5);
            await _orderService.CancelOrderAsync(orderDTO.Id); // İlk iptal

            // Act: İkinci kez iptal etmeye çalış
            var result = await _orderService.CancelOrderAsync(orderDTO.Id);

            // Assert: Zaten iptal edilmiş bir sipariş için false dönmeli
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CancelOrderAsync_ShouldReturnFalse_WhenOrderIsAlreadyClosed()
        {
            // Arrange: Siparişi oluştur, ödemesini yapıp kapat
            var orderDTO = await CreateAndReturnDummyOrderAsync(6);
            await _orderService.ProcessPaymentAsync(orderDTO.Id, orderDTO.TotalAmount, paymentMethod.Nakit);

            // Act
            var result = await _orderService.CancelOrderAsync(orderDTO.Id);

            // Assert: Kapalı bir sipariş iptal edilemez
            result.Should().BeFalse();
        }

        [Fact]
        public async Task AddItemsToOrderAsync_ShouldThrowInvalidOperationException_WhenTableStatusIsNotDolu()
        {
            // Arrange: Sipariş oluştur ama masanın durumunu manuel olarak "Boş" yap
            var orderDTO = await CreateAndReturnDummyOrderAsync(7);
            var table = await _context.Tables.FindAsync(orderDTO.TableId);
            table.Status = Status.Boş; // Hata yaratmak için manuel değiştirme
            await _context.SaveChangesAsync();

            var newItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } };

            // Act & Assert
            // Servis kodunuzun Table'ı FindAsync ile çektiği için Table navigasyon propertysi null olabilir.
            // Ancak, hata mesajınızın fırladığı senaryoyu test ediyoruz.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.AddItemsToOrderAsync(orderDTO.Id, newItems)
            );
        }

        [Fact]
        public async Task AddItemsToOrderAsync_ShouldThrowInvalidOperationException_WhenNewProductNotFound()
        {
            // Arrange: Geçerli bir sipariş oluştur
            var orderDTO = await CreateAndReturnDummyOrderAsync(8);

            // Act & Assert: Yeni eklenen ürün bulunamadı hatası bekleriz.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.AddItemsToOrderAsync(orderDTO.Id, new List<OrderItemDTO> { new OrderItemDTO { ProductId = 999, Quantity = 1 } })
            );
        }

        [Fact]
        public async Task AddItemsToOrderAsync_ShouldThrowInvalidOperationException_WhenInsufficientStockForNewItem()
        {
            // Arrange: Geçerli bir sipariş oluştur
            var orderDTO = await CreateAndReturnDummyOrderAsync(9);

            // Act & Assert: Ürün 1'in stoğu 45'ti. İlk siparişte 2 düşmüştü (43 kaldı). 44 tane daha istersek hata fırlamalı.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.AddItemsToOrderAsync(orderDTO.Id, new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 44 } })
            );
        }

        [Fact]
        public async Task RetrieveOrderAsync_ShouldReturnTrueAndRestoreState_WhenOrderIsCanceled()
        {
            // Arrange: 
            // 1. Bir sipariş oluştur (Masa 10). Stok düşer (Ürün 1: 45 -> 43, Ürün 2: 70 -> 69).
            var orderDTO = await CreateAndReturnDummyOrderAsync(10);
            int orderId = orderDTO.Id;

            // 2. Siparişi iptal et. Stoklar geri gelir (Ürün 1: 43 -> 45, Ürün 2: 69 -> 70) ve Masa 10 Boş olur.
            await _orderService.CancelOrderAsync(orderId);

            // Stok ve Masa durumu kontrolü:
            var product1StockBeforeRestore = (await _context.Products.FindAsync(1)).StockQuantity;
            product1StockBeforeRestore.Should().Be(45); // İptal edildiği için tam stok beklenir.
            (await _context.Tables.FindAsync(10)).Status.Should().Be(Status.Boş);

            // Act
            var result = await _orderService.RetrieveOrderAsync(orderId);

            // Assert
            result.Should().BeTrue();

            // 1. Durum Kontrolü: Sipariş tekrar Açık olmalı.
            var retrievedOrder = await _context.Orders.FindAsync(orderId);
            retrievedOrder.Status.Should().Be(orderStatus.Acik);

            // 2. Masa Kontrolü: Masa tekrar Dolu olmalı.
            (await _context.Tables.FindAsync(10)).Status.Should().Be(Status.Dolu);

            // 3. Stok Kontrolü: Stoklar tekrar düşülmeli (45 -> 43, 70 -> 69).
            (await _context.Products.FindAsync(1)).StockQuantity.Should().Be(43);
        }

        [Fact]
        public async Task RetrieveOrderAsync_ShouldThrowInvalidOperationException_WhenInsufficientStockToRestore()
        {
            // Arrange: 
            // 1. Yeterli stok kalmayacak şekilde manuel ayar yap.
            var product1 = await _context.Products.FindAsync(1);
            var originalStock = product1.StockQuantity; // Örn: 45

            // 2. Bir sipariş oluştur (2 adet Ürün 1 sipariş edilsin) (Stok 43 olur).
            var orderDTO = await CreateAndReturnDummyOrderAsync(11);
            int orderId = orderDTO.Id;

            // 3. Siparişi iptal et (Stok 45 olur).
            await _orderService.CancelOrderAsync(orderId);

            // 4. Stok Yetersizliği simülasyonu: Başka bir işlemle stoğu düşür. 
            // Sipariş 2 adet ürün istiyordu. Stok 45'ti. 44 adet daha düşersek 1 kalır.
            product1.StockQuantity -= 44;
            await _context.SaveChangesAsync();

            // Mevcut stok: 1
            (await _context.Products.FindAsync(1)).StockQuantity.Should().Be(1);

            // Act & Assert: Geri yükleme 2 adet ürün düşmek isteyecek. (1 - 2 = -1) Hata fırlamalı.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.RetrieveOrderAsync(orderId)
            );

            // Kontrol: İşlem geri alınmalı, sipariş durumu hala Iptal kalmalı.
            (await _context.Orders.FindAsync(orderId)).Status.Should().Be(orderStatus.Iptal);
        }

        [Fact]
        public async Task RetrieveOrderAsync_ShouldReturnFalse_WhenOrderIsAlreadyActive()
        {
            // Arrange: Zaten Açık bir sipariş oluştur (Masa 12).
            var orderDTO = await CreateAndReturnDummyOrderAsync(12);

            // Act
            var result = await _orderService.RetrieveOrderAsync(orderDTO.Id);

            // Assert: Zaten Açık olduğu için false dönmeli.
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RetrieveOrderAsync_ShouldReturnFalse_WhenOrderIsClosed()
        {
            // Arrange: Siparişi oluştur ve tamamen ödeme yaparak Kapalı duruma getir (Masa 13).
            var orderDTO = await CreateAndReturnDummyOrderAsync(13);
            await _orderService.ProcessPaymentAsync(orderDTO.Id, orderDTO.TotalAmount, paymentMethod.Nakit);

            // Act
            var result = await _orderService.RetrieveOrderAsync(orderDTO.Id);

            // Assert: Kapalı sipariş geri yüklenemez.
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RetrieveOrderAsync_ShouldReturnFalse_WhenOrderNotFound()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _orderService.RetrieveOrderAsync(nonExistentId);

            // Assert: Sipariş bulunamadığında false dönmeli.
            result.Should().BeFalse();
        }
    }
}