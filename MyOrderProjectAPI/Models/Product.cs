using MyOrderProjectAPI.Commons;

namespace MyOrderProjectAPI.Models
{
    public class Product : ISoftDelete
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; } //Birim fiyat
        public int StockQuantity { get; set; }

        //Foreign Key
        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // ISoftDelete Arayüz Alanları
        public bool RecordStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}
