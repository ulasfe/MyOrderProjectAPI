using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MyOrderProjectAPI; // API Projenizin ana namespace'i
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Tests.Base;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Xunit;

// Artık bu sınıf sadece HTTP (Entegrasyon) testlerine odaklanmıştır.
public abstract class IntegrationTestsBase : IClassFixture<CustomWebApplicationFactory<APIEntryPontForTests>>
{
    protected readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<APIEntryPontForTests> _factory;
    protected readonly IConfiguration _configuration;

    // Yapıcı metot, zorunlu olarak factory'i alır.
    public IntegrationTestsBase(CustomWebApplicationFactory<APIEntryPontForTests> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _configuration = BuildConfiguration();
        AddJwtHeader(GetValidAdminToken());
    }

    private IConfiguration BuildConfiguration()
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(currentDirectory)
            .AddJsonFile("appsettings.IntegrationTest.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        return configuration;
    }

    protected void AddJwtHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Admin rolüne sahip geçerli bir JWT tokeni oluşturur.
    /// </summary>
    protected string GetValidAdminToken()
    {
        return GenerateJwtToken("testadmin@test.com", "Admin");
    }

    /// <summary>
    /// Standart kullanıcı rolüne sahip geçerli bir JWT tokeni oluşturur.
    /// </summary>
    protected string GetValidUserToken()
    {
        return GenerateJwtToken("testuser@test.com", "User");
    }

    /// <summary>
    /// Belirtilen kullanıcı adı ve rol ile JWT tokeni oluşturmanın temel mantığı.
    /// </summary>
    private string GenerateJwtToken(string username, string role)
    {
        // 1. Token Ayarlarını Configuration'dan Çekme
        // Bu değerler, API projenizin appsettings.json dosyasındaki JWT ayarlarıyla eşleşmelidir.
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = jwtSettings["SecurityKey"]; // Örneğin, "JwtSettings": { "SecurityKey": "SüperGizliAnahtar..." }

        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("JwtSettings:SecurityKey değeri test yapılandırmasında eksik.");
        }

        // 2. Claim'leri (İddialar/Veriler) Oluşturma
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role) // BURASI KRİTİK: Rolü ekliyoruz
        };

        // 3. Güvenlik Anahtarını Hazırlama
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // 4. Token'ı Tanımlama
        var token = new JwtSecurityToken(
            issuer: jwtSettings["ValidIssuer"],
            audience: jwtSettings["ValidAudience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30), // Test için kısa bir süre yeterli
            signingCredentials: credentials);

        // 5. Token'ı String olarak Döndürme
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ... diğer HTTP veya yetkilendirme helper metotları ...
}

// **ÖRNEK KULLANIM:**
// public class OrderControllerIntegrationTests : IntegrationTestsBase
// {
//     public OrderControllerIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }
//     
//     // Constructor'da factory zorunluluğu VAR.
//     // Testler, _client üzerinden HTTP isteği gönderir.
// }