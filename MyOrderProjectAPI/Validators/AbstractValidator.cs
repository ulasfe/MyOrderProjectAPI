using FluentValidation;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.Validators
{
    public class CategoryCreateValidator : AbstractValidator<CategoryCreateDTO>
    {
        public CategoryCreateValidator()
        {
            // Kategori Adı (Name) alanı için zorunluluk kuralı:
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Kategori adı boş bırakılamaz.") // Boş olamaz
                .MaximumLength(100).WithMessage("Kategori adı 100 karakteri geçemez."); // Maksimum uzunluk

            // İsteğe bağlı: Eğer DTO'da Fiyat gibi bir alan olsaydı:
            // RuleFor(c => c.Price)
            //     .GreaterThan(0).WithMessage("Fiyat sıfırdan büyük olmalıdır.");
        }
    }
    public class ProductCreateValidator : AbstractValidator<ProductCreateUpdateDTO>
    {
        public ProductCreateValidator()
        {
            // Ürün Adı Kontrolü
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Ürün adı boş bırakılamaz.")
                .MaximumLength(150).WithMessage("Ürün adı 150 karakteri geçemez.");

            // Fiyat Kontrolü
            RuleFor(p => p.Price)
                .NotNull().WithMessage("Fiyat alanı zorunludur.")
                .GreaterThan(0).WithMessage("Fiyat sıfırdan büyük olmalıdır.");

            // Kategori ID Kontrolü
            RuleFor(p => p.CategoryId)
                .NotNull().WithMessage("Kategori ID zorunludur.")
                .GreaterThan(0).WithMessage("Geçerli bir Kategori ID belirtilmelidir.");
        }
    }
    public class OrderCreateValidator : AbstractValidator<OrderCreateDTO>
    {
        public OrderCreateValidator()
        {
            // Masa ID (TableId) Kontrolü
            // [Required] yerine burada doğrulama yapılıyor.
            RuleFor(order => order.TableId)
                .GreaterThan(0).WithMessage("Masa ID (TableId) zorunludur ve geçerli bir değer olmalıdır.");

            // Sipariş Detayları (Items) Kontrolü
            RuleFor(order => order.Items)
                // Listenin kendisinin null veya boş olmamasını sağlar (En az bir item olmalı)
                .NotEmpty().WithMessage("Siparişin detayları (Items) zorunludur ve boş olamaz.");

            // 🔥 Listenin İçindeki Her Bir Elemanı Doğrulama (Nested Validation)
            // Listenin içindeki her OrderItemDTO'yu OrderItemValidator ile doğrula.
            RuleForEach(order => order.Items).SetValidator(new OrderItemValidator());
        }
    }
    public class OrderItemValidator : AbstractValidator<OrderItemDTO>
    {
        public OrderItemValidator()
        {
            // 1. Ürün ID (ProductId) Kontrolü
            // [Required] ve 0'dan büyük olma kuralını uyguluyor.
            RuleFor(item => item.ProductId)
                .GreaterThan(0).WithMessage("Ürün ID (ProductId) zorunludur ve geçerli bir değer olmalıdır.");

            // 2. Miktar (Quantity) Kontrolü
            // [Required] ve [Range(1, 100)] kuralını uyguluyor.
            RuleFor(item => item.Quantity)
                .NotEmpty().WithMessage("Miktar (Quantity) zorunludur.") // NotEmpty, int'ler için 0'a eşit olmamasını sağlar
                .InclusiveBetween(1, 100).WithMessage("Miktar en az 1, en fazla 100 olabilir.");
        }
    }
    public class TableCreateUpdateValidator : AbstractValidator<TableCreateUpdateDTO>
    {
        public TableCreateUpdateValidator()
        {
            // 1. Masa Numarası (TableNumber) Kontrolü
            // [Required] ve [StringLength(10)] kurallarını uyguluyor.
            RuleFor(t => t.TableNumber)
                .NotEmpty().WithMessage("Masa numarası zorunludur ve boş bırakılamaz.")
                .MaximumLength(10).WithMessage("Masa numarası 10 karakterden uzun olamaz.");

            // 2. Status Kontrolü (Opsiyonel ama iyi uygulama)
            // Eğer Status, Enum ise ve geçerli bir enum değeri olmasını istiyorsak:
            // RuleFor(t => t.Status)
            //     .IsInEnum().WithMessage("Geçersiz masa durumu (Status) değeri.");

            // 3. ModifyDate Kontrolü (Opsiyonel)
            // Eğer güncelleme DTO'sunda bu alan varsa ve null olmaması gerekiyorsa:
            // RuleFor(t => t.ModifyDate)
            //     .NotNull().When(t => t.ModifyDate.HasValue).WithMessage("Güncelleme tarihi boş olamaz.");
        }
    }
    public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterDTOValidator()
        {
            // 1. Username (Kullanıcı Adı) Kontrolü
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Kullanıcı Adı boş bırakılamaz.")
                .Length(3, 50).WithMessage("Kullanıcı Adı 3 ile 50 karakter arasında olmalıdır.");

            // 2. Password (Şifre) Kontrolü
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş bırakılamaz.")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")
                // Gelişmiş güvenlik için eklenebilecek ek kontroller:
                // .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
                // .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");
                .MaximumLength(100).WithMessage("Şifre en fazla 100 karakter olabilir.");

            // 3. FullName (Ad Soyad) Kontrolü
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad Soyad alanı boş bırakılamaz.")
                .MaximumLength(100).WithMessage("Ad Soyad en fazla 100 karakter olabilir.");

            // 4. Role (Rol) Kontrolü
            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Geçersiz Kullanıcı Rolü belirtildi.")
                // Opsiyonel: Admin rolünün doğrudan kayıt esnasında atanmasını engellemek için
                .NotEqual(UserRole.Admin).WithMessage("Admin rolü kayıt sırasında atanamaz.");
        }
    }
}
