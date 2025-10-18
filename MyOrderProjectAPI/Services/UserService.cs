using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyOrderProjectAPI.Data;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Extensions;
using MyOrderProjectAPI.Helpers;
using MyOrderProjectAPI.Models;
using MyOrderProjectAPI.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    public UserService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<User?> AuthenticateUserAsync(string username, string password)
    {
        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Username == username);

        // 1. Kullanıcı bulunamadıysa veya aktif değilse
        if (user == null || !user.RecordStatus)
            return null;

        // 2. Şifre Doğrulaması (HASH Kontrolü)
        // Kullanıcının girdiği şifreyi, veritabanındaki Hash ve Salt ile karşılaştır.
        bool isPasswordCorrect = SecurityHelper.VerifyPasswordHash(
            password,
            user.PasswordHash,
            user.PasswordSalt
        );

        if (!isPasswordCorrect)
        {
            return null; // Şifre yanlış
        }

        // 3. Doğrulama başarılıysa kullanıcıyı döndür.
        return user;
    }

    public async Task<User?> RegisterUserAsync(RegisterDTO registerDTO)
    {
        // 1. Kullanıcı Adı Benzersizlik Kontrolü
        bool isUsernameTaken = await _context.Users
            .AnyAsync(u => u.Username == registerDTO.Username);

        if (isUsernameTaken)
        {
            // Kullanıcı zaten mevcut, Controller'a null döndürerek Conflict (409) yapmasını işaret et.
            return null;
        }

        // 2. Şifreyi Hashle ve Salt oluştur
        SecurityHelper.CreatePasswordHash(
            registerDTO.Password,
            out byte[] passwordHash,
            out byte[] passwordSalt
        );

        // 3. Yeni Kullanıcı Nesnesini Oluştur
        var user = new User
        {
            Username = registerDTO.Username,
            FullName = registerDTO.FullName,
            Role = registerDTO.Role,
            PasswordHash = passwordHash, // Hashlenmiş şifreyi kaydet
            PasswordSalt = passwordSalt, // Salt'ı kaydet
            RecordStatus = true,
            CreatedDate = DateTime.Now
        };

        // 4. Veritabanına Ekle ve Kaydet
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> SoftDeleteUserAsync(int userId)
    {
        // Kullanıcıyı aktif/pasif gözetmeksizin bul
        var user = await _context.Users
                                 .IgnoreQueryFilters() // Kayıt filtreleniyorsa bu gereklidir
                                 .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return false; // Kullanıcı bulunamadı
        }

        // Zaten silinmişse tekrar silme
        if (user.RecordStatus == false)
        {
            return false;
        }

        user.RecordStatus = false;
        _context.Users.Update(user); // Entity Framework'te değişikliği işaretle
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestoreUserAsync(int userId)
    {
        // Kullanıcıyı pasif kayıtlar dahil bul
        var user = await _context.Users
                                 .IgnoreQueryFilters() // Silinmiş kayıtları da çek
                                 .FirstOrDefaultAsync(u => u.Id == userId);


        
        if (user == null)
        {
            return false; // Kullanıcı bulunamadı
        }

        // Zaten aktifse geri yükleme
        if (user.RecordStatus == true)
        {
            return false;
        }

        _context.Restore(user);
        await _context.SaveChangesAsync();

        return true;
    }

    // ----------------------------------------------------------------------
    // YARDIMCI METOT: JWT TOKEN ÜRETME MANTIĞI
    // ----------------------------------------------------------------------
    public AuthResultDTO GenerateJwtToken(string username, string role)
    {
        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username), // Unique ID veya Username
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Jeton ID
                new Claim(ClaimTypes.Role,role) // Rolleri Claim olarak ekleme
            };

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