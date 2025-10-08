namespace MyOrderProjectAPI.DTOs
{
    public class OrderCreateDTO
    {

        public int TableId { get; set; }
        public List<OrderItemDTO> Items { get; set; } = new List<OrderItemDTO>();
    }
}
