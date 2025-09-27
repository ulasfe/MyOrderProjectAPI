using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.Data;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDetailDTO>> GetAllProductsAsync()
        {
            //Include ile Category verisini de çek.
            return await _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductDetailDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();
        }

        public async Task<ProductDetailDTO?> GetProductByIdAsync(int id)
        {
            var productDetaidDTO = await _context.Products
                .IgnoreQueryFilters()
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new ProductDetailDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .FirstOrDefaultAsync();

            return productDetaidDTO;
        }

        public async Task<ProductDetailDTO> CreateProductAsync(ProductCreateUpdateDTO productDTO)
        {
            var product = new Product
            {
                Name = productDTO.Name,
                Price = productDTO.Price,
                StockQuantity = productDTO.StockQuantity,
                CategoryId = productDTO.CategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var createdProduct = await GetProductByIdAsync(product.Id);
            return createdProduct!; // null olmamalı
        }



        public async Task<bool> UpdateProductAsync(int id, ProductCreateUpdateDTO productDTO)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return false;
            }

            product.Name = productDTO.Name;
            product.Price = productDTO.Price;
            product.StockQuantity = productDTO.StockQuantity;
            product.CategoryId = productDTO.CategoryId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);

            if (product == null || !product.RecordStatus)
            {
                return false; // Ürün zaten yok
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreProductAsync(int id)
        {
            var product = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);

            if (product == null || product.RecordStatus)
            {
                return false; // Ürün zaten yok
            }
            product.RecordStatus = true;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
