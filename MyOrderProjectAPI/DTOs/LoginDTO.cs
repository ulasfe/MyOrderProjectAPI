namespace MyOrderProjectAPI.DTOs
{
    public class LoginDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // 2. Başarılı Login Sonucu Döndürülecek Bilgiler
    public class AuthResultDTO
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string Username { get; set; } = string.Empty;
        // İsteğe bağlı: Rol listesi vb.
    }
}
