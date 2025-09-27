namespace MyOrderProjectAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        //Sipariş anundaki ürün birim değeri.
        public decimal UnitPriceAtTimeOfOrder { get; set; }

        //Foreign Key
        public int OrderId { get; set; }

        //Foreign Key
        public int ProductId { get; set; }

        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
