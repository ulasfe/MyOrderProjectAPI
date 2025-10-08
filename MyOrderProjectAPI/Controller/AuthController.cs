using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyOrderProjectAPI.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyOrderProjectAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        // NOT: Gerçek projede ApplicationDbContext yerine UserService kullanılmalıdır!
        // private readonly IUserService _userService; 

        public AuthController(IConfiguration configuration /*, IUserService userService */)
        {
            _configuration = configuration;
            // _userService = userService;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResultDTO>> Login([FromBody] LoginDTO credentials)
        {
            // ----------------------------------------------------------------------
            // ADIM 1: Kullanıcı Kimlik Doğrulaması (Gerçek DB Kontrolü)
            // ----------------------------------------------------------------------

            // Gerçek uygulamada, kullanıcı adı ve şifrenin DB'de doğrulanması gerekir.
            // ÖRNEK BASİT KONTROL:
            if (credentials.Username != "testuser" || credentials.Password != "password")
            {
                // Eğer kimlik doğrulama başarısız olursa 401 döndürülür
                return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı." });
            }

            // ----------------------------------------------------------------------
            // ADIM 2: Token'ı Oluşturma ve Döndürme
            // ----------------------------------------------------------------------

            var tokenResult = GenerateJwtToken(credentials.Username, new List<string> { "User" });

            return Ok(tokenResult);
        }

        // ----------------------------------------------------------------------
        // YARDIMCI METOT: JWT TOKEN ÜRETME MANTIĞI
        // ----------------------------------------------------------------------
        private AuthResultDTO GenerateJwtToken(string username, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username), // Unique ID veya Username
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Jeton ID
            };

            // Rolleri Claim olarak ekleme
            roles.ForEach(role => claims.Add(new Claim(ClaimTypes.Role, role)));

            // appsettings.json'dan gizli anahtarı alıyoruz
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Token'ın bitiş süresini 1 saat olarak ayarlıyoruz
            var expiryTime = DateTime.Now.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expiryTime,
                signingCredentials: credentials);

            return new AuthResultDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiryTime,
                Username = username
            };
        }
    }
}
