using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.Services;
using MyOrderProjectAPI.Models;
using MyOrderProjectAPI.Tests.Base;
using MyOrderProjectAPI.Tests.Base.MyOrderProjectAPI.Tests.Base;
using System.Threading.Tasks;
using Xunit;
using MyOrderProjectAPI.DTOs;

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

            var updatedProduct = await _productService.UpdateProductAsync(product.Id,toBeUpdateProduct);

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
    }
}