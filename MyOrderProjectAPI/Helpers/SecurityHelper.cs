using System.Security.Cryptography;
using System.Text;

namespace MyOrderProjectAPI.Helpers
{
    public static class SecurityHelper
    {
        // Şifrenin hash'lenmesini sağlar.
        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Şifre boş olamaz.", nameof(password));

            // HMACSHA512 kullanarak salt oluşturulur.
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        // Kullanıcının girdiği şifreyi, veritabanındaki hash ile karşılaştırır.
        public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Şifre boş olamaz.", nameof(password));
            if (storedHash.Length != 64) throw new ArgumentException("Geçersiz hash uzunluğu (Beklenen: 64 byte).");
            if (storedSalt.Length != 128) throw new ArgumentException("Geçersiz salt uzunluğu (Beklenen: 128 byte).");

            // Kayıtlı salt (tuz) ile aynı algoritma kullanılır.
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Hesaplanan hash ile kayıtlı hash'i byte byte karşılaştır.
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}