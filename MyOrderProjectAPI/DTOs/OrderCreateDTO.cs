using System.ComponentModel.DataAnnotations;

namespace MyOrderProjectAPI.DTOs
{
    public class OrderCreateDTO
    {
        [Required(ErrorMessage = "Masa ID zorunludur.")]
        public int TableId { get; set; }

        [Required(ErrorMessage = "Sipariş detayları zorunludur.")]
        public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
    }
}
