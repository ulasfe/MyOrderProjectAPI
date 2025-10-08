using MyOrderProjectAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace MyOrderProjectAPI.DTOs
{
    public class PaymentRequestDTO
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ödeme miktarı 0'dan büyük olmalıdır.")]
        public decimal Amount { get; set; }

        [Required]
        public paymentMethod PaymentMethod { get; set; } = paymentMethod.Nakit;
    }
}
