using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.Services
{
    public interface IOrderService
    {
        // Tüm siparişleri listele
        Task<IEnumerable<OrderDetailsDTO>> GetActiveOrdersAsync();

        // ID değerine göre sipariş detayını getir
        Task<OrderDetailsDTO?> GetOrderByIdAsync(int id);

        // Yeni bir sipariş oluştur
        Task<OrderDetailsDTO> CreateOrderAsync(OrderCreateDTO orderDTO);

        // Mevcut siparişe yeni ürünler ekle
        Task<OrderDetailsDTO?> AddItemsToOrderAsync(int orderId, List<OrderItemDTO> newItems);

        // Siparişi iptal et
        Task<bool> CancelOrderAsync(int orderId);

        // Ödeme işlemini tamamla
        Task<bool> ProcessPaymentAsync(int orderId, decimal amount, paymentMethod method);
    }
}
