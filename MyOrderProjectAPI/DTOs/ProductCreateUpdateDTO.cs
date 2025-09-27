using System.ComponentModel.DataAnnotations;

namespace MyOrderProjectAPI.DTOs
{
    public class ProductCreateUpdateDTO
    {
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(200, ErrorMessage = "Ürün adı 200 karakterden uzun olamaz.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, 9999.99, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Kategori ID zorunludur.")]
        public int CategoryId { get; set; }
    }
}
