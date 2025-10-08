using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.Data;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Extensions;
using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            return await _context.Categories.
                Include(c => c.Products)
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<CategoryDTO?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null) return null;

            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<CategoryDTO> CreateCategoryAsync(CategoryCreateDTO categoryDTO)
        {
            if (string.IsNullOrWhiteSpace(categoryDTO.Name))
            {
                throw new ArgumentException("Kategori adı boş olamaz.");
            }

            var category = new Category
            {
                Name = categoryDTO.Name
            };

            // Aynı isimde başka bir kayıt var mı kontrolü.
            var exists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
            if (exists)
            {
                throw new InvalidOperationException("Bu kategori zaten mevcut.");
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name
            };
        }
        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                                         .IgnoreQueryFilters()
                                         .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return false; // 404 Not Found yerine false dön
            }
            else if (category.RecordStatus == false)
            {
                // Zaten silinmişse, hata fırlatmak yerine (Controller'da BadRequest'e dönecek) false dönebiliriz.
                // Veya InvalidOperationException fırlatıp bunu Controller'da yakala.
                return false;
            }

            // Gerçek silme yerine soft deletion işlemi yapılır (EF Core'daki Remove Soft Delete'i tetikler)
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RestoreCategoryAsync(int id)
        {
            var category = await _context.Categories
                                         .IgnoreQueryFilters()
                                         .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return false; // 404 Not Found yerine false döner
            }

            if (category.RecordStatus == true)
            {
                // Zaten aktifse
                return false; // Controller'da BadRequest'e dönecek
            }

            // Restore işlemini tetikler
            _context.Restore(category);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
