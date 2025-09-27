using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.DTOs
{
    public class OrderDetailsDTO
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty; 
        public orderStatus Status { get; set; } =  orderStatus.Kapali;
        public decimal TotalAmount { get; set; }
        public List<OrderItemDetailDTO> Items { get; set; } = new List<OrderItemDetailDTO>();
        public List<PaymentDetailDTO> Payments { get; set; } = new List<PaymentDetailDTO>();
    }
}

public class OrderItemDetailDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceAtTimeOfOrder { get; set; }
    public decimal TotalPrice => Quantity * UnitPriceAtTimeOfOrder;
}

public class PaymentDetailDTO
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public paymentMethod PaymentMethod { get; set; } = paymentMethod.Null;
}