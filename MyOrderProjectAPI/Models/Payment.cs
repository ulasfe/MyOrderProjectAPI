namespace MyOrderProjectAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public paymentMethod PaymentMethod { get; set; } = paymentMethod.Nakit;

        //Foreign Key
        public int OrderId { get; set; }

        // Ödemeye ait sipariş bilgisi.
        public Order Order { get; set; } = null!;
    }

    public enum paymentMethod
    {
        Null,
        Nakit,
        KrediKarti,
        Online
    }
}
