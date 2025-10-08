using MyOrderProjectAPI.Commons;

namespace MyOrderProjectAPI.Models
{
    public class Category : ISoftDelete
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Product> Products { get; set; } = new List<Product>();

        // ISoftDelete Arayüz Alanları
        public bool RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
