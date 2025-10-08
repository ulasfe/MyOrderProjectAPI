namespace MyOrderProjectAPI.DTOs
{
    public class ProductCreateUpdateDTO
    {
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
    }
}
