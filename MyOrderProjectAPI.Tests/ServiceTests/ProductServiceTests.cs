using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Services;
using MyOrderProjectAPI.Tests.Base;

namespace MyOrderProjectAPI.Tests.ServiceTests
{
    public class ProductServiceTests : BaseTest
    {
        private readonly ProductService _productService;

        public ProductServiceTests() : base()
        {
            _productService = new ProductService(_context);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnNull_ForNonExistentId()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _productService.GetProductByIdAsync(nonExistentId);

            // Assert: Ürün bulunamadığında null dönmeli.
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldReturnFalse_WhenProductNotFound()
        {
            // Arrange
            int nonExistentId = 999;
            var updateDto = new ProductCreateUpdateDTO { Name = "Geçersiz Ürün", CategoryId = 1 };

            // Act
            var result = await _productService.UpdateProductAsync(nonExistentId, updateDto);

            // Assert: Güncellenecek ürün yoksa false dönmeli.
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteProductAsync_ShouldReturnFalse_WhenProductNotFound()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _productService.DeleteProductAsync(nonExistentId);

            // Assert: Silinecek ürün yoksa false dönmeli.
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreProductAsync_ShouldReturnFalse_WhenProductNotFound()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _productService.RestoreProductAsync(nonExistentId);

            // Assert: Geri yüklenecek ürün yoksa false dönmeli.
            result.Should().BeFalse();
        }
        [Fact]
        public async Task CreateProductAsync_ShouldReturn()
        {
            var productCRUDDto = new ProductCreateUpdateDTO
            {
                Name = "TestÜrünü",
                Price = 10,
                StockQuantity = 10,
                CategoryId = 1
            };

            var productDetailDTO = await _productService.CreateProductAsync(productCRUDDto);

            productCRUDDto.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateProductAsync_ShouldThrowDbUpdateException_OnForeignKeyViolation()
        {
            // Arrange: DbContext'in SaveChangesAsync metodunu, dış anahtar (FK) hatası fırlatacak şekilde mock'la
            var invalidDto = new ProductCreateUpdateDTO
            {
                Name = "Yeni Ürün",
                Price = 100,
                StockQuantity = 10,
                CategoryId = 999 // Var olmayan kategori ID'si
            };

            // Act & Assert
            // Servis katmanının bu veritabanı hatasını Controller'a iletmesini bekleriz.
            await Assert.ThrowsAsync<DbUpdateException>(
                () => _productService.CreateProductAsync(invalidDto)
            );
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _productService.GetProductByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductIsSoftDeleted()
        {
            // Arrange
            // BaseTest'te Product 1 (Id=1) aktif olarak eklenmiştir.
            int productId = 1;

            // Ürünü sil
            await _productService.DeleteProductAsync(productId);

            // Act: Normal Get metodu sadece RecordStatus=True olanları çekmeli
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert: Soft Delete yapıldığı için null dönmelidir.
            result.Should().BeNull();
        }


        [Fact]
        public async Task UpdateProductAsync_ShouldReturnFalse_WhenProductToUpdateNotFound()
        {
            // Arrange
            int nonExistentId = 999;
            var updateDto = new ProductCreateUpdateDTO { Name = "Yeni İsim", CategoryId = 1 };

            // Act
            var result = await _productService.UpdateProductAsync(nonExistentId, updateDto);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateProductAsync_ShouldThrowDbUpdateException_OnForeignKeyViolation()
        {
            // Arrange
            int existingId = 1;
            var invalidDto = new ProductCreateUpdateDTO
            {
                Name = "FK Hata Ürünü",
                Price = 100,
                StockQuantity = 10,
                CategoryId = 999 // Var olmayan kategori ID'si
            };

            // Act & Assert
            // Product 1'i var olmayan bir kategoriye güncellemeye çalışıyoruz.
            await Assert.ThrowsAsync<DbUpdateException>(
                () => _productService.UpdateProductAsync(existingId, invalidDto)
            );
        }
        [Fact]
        public async Task DeleteProductAsync_ShouldReturnFalse_WhenProductIsAlreadyDeleted()
        {
            // Arrange
            int productId = 1;

            // Önce silme işlemini yap
            await _productService.DeleteProductAsync(productId);

            // Act: İkinci kez silmeye çalış
            var result = await _productService.DeleteProductAsync(productId);

            // Assert: Zaten silinmiş (RecordStatus=False) olduğu için false dönmeli.
            result.Should().BeFalse();
        }


        [Fact]
        public async Task SoftDeleteAsync_ValidId_ShouldSetRecordStatusToFalse()
        {
            int productIdToDelete = 1;

            var result = await _productService.DeleteProductAsync(productIdToDelete);


            result.Should().BeTrue();

            var deletedProduct = await _context.Products
                                               .IgnoreQueryFilters()
                                               .FirstOrDefaultAsync(p => p.Id == productIdToDelete);

            deletedProduct.Should().NotBeNull();
            deletedProduct.RecordStatus.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_InvalidId_ShouldReturnNotFoundError()
        {

            int invalidId = 999;

            var result = await _productService.DeleteProductAsync(invalidId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreAsync_Product_ShouldReturnTrue()
        {
            await SoftDeleteAsync_ValidId_ShouldSetRecordStatusToFalse();

            var success = await _productService.RestoreProductAsync(1);

            success.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Product_ShouldReturnTrue()
        {
            var product = await _productService.GetProductByIdAsync(1);

            product.Should().NotBeNull();

            var toBeUpdateProduct = new ProductCreateUpdateDTO
            {
                Name = "Güncellenen Ürün Adı",
                Price = 100,
                StockQuantity = 4,
                CategoryId = product.CategoryId
            };

            var updatedProduct = await _productService.UpdateProductAsync(product.Id, toBeUpdateProduct);

            updatedProduct.Should().BeTrue();

            product = await _productService.GetProductByIdAsync(1);
            product.Should().NotBeNull();

            product.Name.Should().Be("Güncellenen Ürün Adı");
            product.Price.Should().Be(100);
            product.StockQuantity.Should().Be(4);

        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            var allProducts = await _productService.GetAllProductsAsync();

            allProducts.Should().HaveCount(_context.Products.Where(k => k.RecordStatus).Count());
        }

        [Fact]
        public async Task RestoreProductAsync_ShouldReturnFalse_WhenProductDoesNotExist()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _productService.RestoreProductAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreProductAsync_ShouldReturnFalse_WhenProductIsAlreadyActive()
        {
            // Arrange
            // BaseTest'ten gelen Product 1 (Id=1) zaten aktif (RecordStatus=True)
            int activeId = 1;

            // Act
            var result = await _productService.RestoreProductAsync(activeId);

            // Assert: Zaten aktif olduğu için false dönmeli (hiçbir değişiklik yapılmadı).
            result.Should().BeFalse();
        }
    }
}