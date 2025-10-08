using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.DTOs
{
    public class TableCreateUpdateDTO
    {
        public string TableNumber { get; set; } = string.Empty;

        // Yeni masa oluşturulurken varsayılan olarak "Boş" olacağı için Status zorunlu değil.
        public Status Status { get; set; } = Status.Boş;
        public DateTime? ModifyDate { get; set; }
    }
}
