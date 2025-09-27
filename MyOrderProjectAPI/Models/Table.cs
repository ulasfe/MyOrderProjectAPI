using MyOrderProjectAPI.Commons;

namespace MyOrderProjectAPI.Models
{
    public class Table : ISoftDelete
    {
        public int Id { get; set; }
        public string TableNumber { get; set; } = string.Empty;

        public Status Status { get; set; } = Status.Boş;

        // Masaya ait siparişler
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        // ISoftDelete Arayüz Alanları
        public bool RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }

    public enum Status
    {
        Boş,
        Dolu,
        Rezerve
    }
}
