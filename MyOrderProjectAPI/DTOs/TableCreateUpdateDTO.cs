using MyOrderProjectAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace MyOrderProjectAPI.DTOs
{
    public class TableCreateUpdateDTO
    {
        [Required(ErrorMessage = "Masa numarası zorunludur.")]
        [StringLength(10, ErrorMessage = "Masa numarası 10 karakterden uzun olamaz.")]
        public string TableNumber { get; set; } = string.Empty;

        // Yeni masa oluşturulurken varsayılan olarak "Boş" olacağı için Status zorunlu değil.
        public Status Status { get; set; } = Status.Boş;
        public DateTime? ModifyDate { get; set; }
    }
}
