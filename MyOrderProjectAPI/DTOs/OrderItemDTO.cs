using System.ComponentModel.DataAnnotations;

namespace MyOrderProjectAPI.DTOs
{
    public class OrderItemDTO
    {
        [Required(ErrorMessage = "Ürün ID zorunludur.")]
        public int ProductId { get; set; }

        //Geçici olarak miktar 100 ile sınırlandırıldı
        [Required(ErrorMessage = "Miktar zorunludur.")]
        [Range(1, 100, ErrorMessage = "Miktar en az 1 olmalıdır.")]
        public int Quantity { get; set; }
    }
}
