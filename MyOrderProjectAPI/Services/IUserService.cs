using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Verilen kullanıcı adı ve şifreye göre kullanıcıyı doğrular.
        /// </summary>
        Task<User?> AuthenticateUserAsync(string username, string password);

        Task<User?> RegisterUserAsync(RegisterDTO registerDTO);
        /// <summary>
        /// Kullanıcının RecordStatus'unu false yaparak kaydı siler. (Soft Delete)
        /// </summary>
        Task<bool> SoftDeleteUserAsync(int userId);

        /// <summary>
        /// Kullanıcının RecordStatus'unu true yaparak kaydı geri yükler.
        /// </summary>
        Task<bool> RestoreUserAsync(int userId);
        /// <summary>
        /// Kullanıcı için Jwt jetonu oluşturulmasını sağlar
        /// </summary>
        AuthResultDTO GenerateJwtToken(string username, string role);
    }
}
