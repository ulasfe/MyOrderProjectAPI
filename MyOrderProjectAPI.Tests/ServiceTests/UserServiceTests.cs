using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;
using MyOrderProjectAPI.Services;
using MyOrderProjectAPI.Tests.Base;
using MyOrderProjectAPI.Helpers; // Hashing Helper'ı kullanmak için
using System.Threading.Tasks;
using Xunit;

namespace MyOrderProjectAPI.Tests.ServiceTests
{
    // Testlerin çalışması için BaseTest'in DummyData'sında 
    // en az bir 'testUser' (örneğin: Username="testuser", Password="Password123", Role=Garson)
    // oluşturulduğunu varsayıyoruz.

    public class UserServiceTests : BaseTest
    {
        private readonly UserService _userService;
        private readonly string _testUsername = "testuser";
        private readonly string _testPassword = "Password123";

        public UserServiceTests() : base()
        {
            _userService = new UserService(_context);
            // Test verisinin BaseTest'te yüklendiği varsayılır.

            // Eğer BaseTest'iniz user verisi oluşturmuyorsa, buraya bir kullanıcı ekleyin:
            if (!_context.Users.Any())
            {
                SecurityHelper.CreatePasswordHash(_testPassword, out byte[] hash, out byte[] salt);
                _context.Users.Add(new User
                {
                    Id = 1,
                    Username = _testUsername,
                    FullName = "Test Kullanıcı",
                    Role = UserRole.Garson,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    RecordStatus = true
                });
                _context.SaveChanges();
            }
        }

        // --- 1. AUTHENTICATE TESTLERİ ---

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnUser_WhenCredentialsAreCorrect()
        {
            // Act
            var user = await _userService.AuthenticateUserAsync(_testUsername, _testPassword);

            // Assert (Pozitif)
            user.Should().NotBeNull();
            user!.Username.Should().Be(_testUsername);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Act
            var user = await _userService.AuthenticateUserAsync(_testUsername, "yanlisSifre456");

            // Assert (Negatif)
            user.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenUsernameDoesNotExist()
        {
            // Act
            var user = await _userService.AuthenticateUserAsync("nonexistentuser", _testPassword);

            // Assert (Null Kontrolü)
            user.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenUserIsSoftDeleted()
        {
            // Arrange: Kullanıcıyı soft delete yapalım
            await _userService.SoftDeleteUserAsync(1);

            // Act: Silinmiş kullanıcı ile giriş yapmayı dene
            var user = await _userService.AuthenticateUserAsync(_testUsername, _testPassword);

            // Assert: Soft delete edilen kullanıcı giriş yapamamalı (null dönmeli)
            user.Should().BeNull();

            // Cleanup: Diğer testleri etkilememesi için geri yükle
            await _userService.RestoreUserAsync(1);
        }

        // --- 2. REGISTER TESTLERİ ---

        [Fact]
        public async Task RegisterUserAsync_ShouldCreateUser_WhenUsernameIsUnique()
        {
            // Arrange
            var newUsername = "newgarson";
            var registerDTO = new RegisterDTO
            {
                Username = newUsername,
                Password = "SecurePass123",
                FullName = "Yeni Garson",
                Role = UserRole.Garson
            };

            // Act (Pozitif)
            var newUser = await _userService.RegisterUserAsync(registerDTO);

            // Assert
            newUser.Should().NotBeNull();
            newUser!.Username.Should().Be(newUsername);

            // Hashing Kontrolü: Hash ve Salt oluşturulmuş mu?
            newUser.PasswordHash.Should().NotBeEmpty();
            newUser.PasswordSalt.Should().NotBeEmpty();

            // Kayıt kontrolü: Veritabanında doğru mu?
            var dbUser = await _context.Users.FindAsync(newUser.Id);
            dbUser.Should().NotBeNull();

            // Şifrenin doğru hashlenip hashlenmediğini doğrulama
            SecurityHelper.VerifyPasswordHash(registerDTO.Password, dbUser!.PasswordHash, dbUser.PasswordSalt)
                          .Should().BeTrue();
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnNull_WhenUsernameIsAlreadyTaken()
        {
            // Arrange: Mevcut kullanıcı adını kullan
            var registerDTO = new RegisterDTO
            {
                Username = _testUsername, // Zaten DB'de var
                Password = "AnyPassword",
                FullName = "Çakışan İsim",
                Role = UserRole.Mutfak
            };

            // Act (Negatif / Null Kontrolü)
            var newUser = await _userService.RegisterUserAsync(registerDTO);

            // Assert
            newUser.Should().BeNull();
        }

        // --- 3. SOFT DELETE TESTLERİ ---

        [Fact]
        public async Task SoftDeleteUserAsync_ShouldReturnTrueAndSetRecordStatusFalse_WhenUserExists()
        {
            // Arrange: Yeni bir kullanıcı oluştur (Id 2)
            var newId = (await _userService.RegisterUserAsync(new RegisterDTO { Username = "toDelete", Password = "p", FullName = "N", Role = UserRole.Garson }))!.Id;

            // Act (Pozitif)
            var success = await _userService.SoftDeleteUserAsync(newId);

            // Assert
            success.Should().BeTrue();

            // Veritabanı kontrolü
            var deletedUser = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == newId);
            deletedUser.Should().NotBeNull();
            deletedUser!.RecordStatus.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteUserAsync_ShouldReturnFalse_WhenUserIsNotFound()
        {
            // Act (Null Kontrolü)
            var success = await _userService.SoftDeleteUserAsync(999);

            // Assert
            success.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteUserAsync_ShouldReturnFalse_WhenUserIsAlreadyDeleted()
        {
            // Arrange
            await _userService.SoftDeleteUserAsync(1); // Önce sil

            // Act (Negatif)
            var success = await _userService.SoftDeleteUserAsync(1); // Tekrar silmeyi dene

            // Assert
            success.Should().BeFalse();

            // Cleanup
            await _userService.RestoreUserAsync(1);
        }

        // --- 4. RESTORE TESTLERİ ---

        [Fact]
        public async Task RestoreUserAsync_ShouldReturnTrueAndSetRecordStatusTrue_WhenUserIsDeleted()
        {
            // Arrange
            await _userService.SoftDeleteUserAsync(1); // Sil
            (await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == 1))!.RecordStatus.Should().BeFalse(); // Silindiğini onayla

            // Act (Pozitif)
            var success = await _userService.RestoreUserAsync(1);

            // Assert
            success.Should().BeTrue();

            // Veritabanı kontrolü
            var restoredUser = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == 1);
            restoredUser.Should().NotBeNull();
            restoredUser!.RecordStatus.Should().BeTrue();
        }

        [Fact]
        public async Task RestoreUserAsync_ShouldReturnFalse_WhenUserIsNotFound()
        {
            // Act (Null Kontrolü)
            var success = await _userService.RestoreUserAsync(999);

            // Assert
            success.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreUserAsync_ShouldReturnFalse_WhenUserIsAlreadyActive()
        {
            // Act (Negatif)
            var success = await _userService.RestoreUserAsync(1); // Zaten aktif olanı geri yüklemeyi dene

            // Assert
            success.Should().BeFalse();
        }
    }
}