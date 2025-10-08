using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace MyOrderProjectAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Sipariş Numarası için benzersiz bir değer.
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString();

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public orderStatus Status { get; set; } = orderStatus.Acik;

        public decimal TotalAmount { get; set; }

        //Foreign key
        public int TableId { get; set; }

        public Table Table { get; set; } = null!;

        // Sipariş detayları
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        //Yapılmış olan ödemeler (varsa)
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum orderStatus
    {
        Acik,
        Hazirlaniyor,
        Kapali,
        Iptal
    }
}
