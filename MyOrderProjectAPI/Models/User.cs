using MyOrderProjectAPI.Commons;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyOrderProjectAPI.Models
{
    public enum UserRole
    {
        Admin,
        Garson,
        Mutfak,
        Kasiyer,
        Test
    }

    public class User : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public byte[] PasswordHash { get; set; } = new byte[0];

        [Required]
        public byte[] PasswordSalt { get; set; } = new byte[0];

        [Required]
        public UserRole Role { get; set; }

        public bool RecordStatus { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifyDate { get; set; }
    }
}