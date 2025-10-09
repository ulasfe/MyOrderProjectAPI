using MyOrderProjectAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace MyOrderProjectAPI.DTOs
{
    public class RegisterDTO
    { 
        public string Username { get; set; } = string.Empty;
         
        public string Password { get; set; } = string.Empty;
         
        public string FullName { get; set; } = string.Empty;
         
        public UserRole Role { get; set; }
    }
}
