using MyOrderProjectAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace MyOrderProjectAPI.DTOs
{
    public class PaymentRequestDTO
    { 
        public decimal Amount { get; set; } 
        public paymentMethod PaymentMethod { get; set; } = paymentMethod.Nakit;
    }
}
