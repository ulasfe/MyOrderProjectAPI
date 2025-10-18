using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq; // SingleOrDefault için gerekli
using MyOrderProjectAPI.Data;
using Microsoft.Extensions.Configuration; // Varsayılan DbContext adınızı buraya göre ayarlayın
// using MyOrderProjectAPI.Models; // Seed data kullanacaksanız gereklidir.

namespace MyOrderProjectAPI.Tests.Base
{
    // <TProgram>, genellikle API projenizin ana sınıfı olan 'Program' sınıfını temsil eder.
   
    public class CustomWebApplicationFactory<TEntryPoint>
      : WebApplicationFactory<TEntryPoint> where TEntryPoint: class
    {
        // NOT: Varsayılan olarak DbContext'inizin adının MyOrderDbContext olduğunu varsaydık.
        // Projenizdeki gerçek DbContext adıyla (örneğin ApplicationDbContext) değiştirin.
        private static readonly string InMemoryDbName = "TestInMemoryDb_" + Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    // Her test seti için benzersiz bir ad kullanmak, testlerin birbirinden etkilenmesini önler.
                    options.UseInMemoryDatabase(InMemoryDbName);
                });

                // 3. (İsteğe bağlı) Test Verisi (Seed Data) Ekleme
                // Bu kısım, her entegrasyon testi setinin çalıştırılmasından önce temiz bir DB olmasını sağlar.
                using (var scope = services.BuildServiceProvider().CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;

                    // DB Context'i servisten alın.
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>(); // DbContext adınızı kontrol edin!

                    // Veritabanının oluşturulduğundan emin olun.
                    db.Database.EnsureCreated();

                    // TODO: Buraya, entegrasyon testlerinin ihtiyacı olan temel verileri (örneğin test kullanıcısı, test ürünü) ekleyin ve kaydedin.

                    // Örnek:
                    // if (!db.Products.Any())
                    // {
                    //     db.Products.Add(new Product { Id = 1, Name = "Test Ürünü", Price = 50 });
                    //     db.SaveChanges();
                    // }

                    TestDatabaseSeeder.Seed(db);
                    var productCount = db.Products.Count();
                    if (productCount == 0)
                    {
                        // Console.WriteLine veya bir Debug breakpoint ile buranın tetiklenip tetiklenmediğini kontrol edin.
                        throw new InvalidOperationException("Seed işlemi başarısız oldu, Products tablosu boş.");
                    }

                }
            });

            // Ortamı "IntegrationTest" olarak ayarlayın. Bu, test ortamına özel appsettings.IntegrationTest.json gibi ayarların yüklenmesini sağlar.
            builder.UseEnvironment("IntegrationTest");
        }
    }
}