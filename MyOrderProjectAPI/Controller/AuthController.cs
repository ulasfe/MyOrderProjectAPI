using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyOrderProjectAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        // NOT: Gerçek projede ApplicationDbContext yerine UserService kullanılmalıdır!
        // private readonly IUserService _userService; 

        public AuthController( IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResultDTO>> Login([FromBody] LoginDTO credentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400
            }
            // GÜVENLİK DÜZELTİLDİ: Artık manuel kontrol YOK, servis kullanılıyor
            var user = await _userService.AuthenticateUserAsync(credentials.Username, credentials.Password);

            if (user == null)
            {
                // Kimlik doğrulama başarısız (Kullanıcı adı veya şifre yanlış)
                return Unauthorized(new { message = "Kullanıcı adı veya şifre yanlış." }); // 401
            }

            var tokenResult = _userService.GenerateJwtToken(credentials.Username, user.Role.ToString());

            return Ok(tokenResult);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Geçersiz Veri
            }

            try
            {
                var newUser = await _userService.RegisterUserAsync(registerDTO);

                if (newUser == null)
                {
                    // Kullanıcı adı zaten sistemde mevcut
                    return Conflict(new { message = $"Kullanıcı adı '{registerDTO.Username}' zaten sistemde mevcut." }); // 409 Conflict
                }

                // Başarılı Kayıt: 201 Created
                // Güvenlik nedeniyle şifre ve hash bilgileri döndürülmez.
                return StatusCode(201, new
                {
                    Message = "Kullanıcı başarıyla oluşturuldu.",
                    Username = newUser.Username,
                    Role = newUser.Role.ToString()
                });
            }
            catch (Exception ex)
            {
                // Beklenmedik veritabanı veya sunucu hataları
                return StatusCode(500, new { message = "Kayıt işlemi sırasında beklenmedik bir hata oluştu.", error = ex.Message });
            }
        }

        

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userService.SoftDeleteUserAsync(id);

            if (!success)
            {
                // Kullanıcı ya bulunamadı (404) ya da zaten silinmişti.
                return NotFound($"ID {id} numaralı kullanıcı bulunamadı veya zaten silinmiş."); // 404
            }

            return NoContent(); // 204 Başarılı Silme (İçerik Yok)
        }

        // Kullanıcıyı Geri Yükleme (Restore)
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreUser(int id)
        {
            var success = await _userService.RestoreUserAsync(id);

            if (!success)
            {
                // Kullanıcı ya bulunamadı (404) ya da zaten aktif durumdaydı.
                return NotFound($"ID {id} numaralı kullanıcı bulunamadı veya zaten aktif."); // 404
            }

            return NoContent(); // 204 Başarılı Geri Yükleme
        }
    }
}
