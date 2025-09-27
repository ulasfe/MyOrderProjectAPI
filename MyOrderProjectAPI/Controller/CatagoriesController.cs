using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.Data;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Extensions;
using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.Controller
{

    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoryDTO>> PostCategory(CategoryCreateDTO categoryDTO)
        {
            if (string.IsNullOrWhiteSpace(categoryDTO.Name))
            {
                return BadRequest(new { message = "Kategori adı boş olamaz." });
            }
            var category = new Category
            {
                Name = categoryDTO.Name
            };

            //Aynı isimde başka bir kayıt var mı kontrol et.
            var exists = await _context.Categories
                                       .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
            if (exists)
            {
                return Conflict(new { message = "Bu kategori zaten mevcut." });
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var createdCategoryDTO = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name
            };

            return CreatedAtAction(nameof(GetCategory), new { id = createdCategoryDTO.Id }, createdCategoryDTO);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            return await _context.Categories
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            } 

            var categoryDTO = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name
            };

            return categoryDTO;
        }

        [HttpPost("{id}/restore")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreCategory(int id)
        {
            //Kaydı bulmak için Global Query Filter'ı yoksaymamız gerekiyor
            //çünkü Global Query Filter ile RecordStatus durumu aktif olmayanlar (Soft Deletion işlemi ile silinen kategoriler) çekilemeyecek.
            var category = await _context.Categories
                                         .IgnoreQueryFilters()
                                         .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = $"ID'si {id} olan kategori bulunamadı." });
            }

            if (category.RecordStatus == true)
            {
                return BadRequest(new { message = $"ID'si {id} olan kategori zaten aktif durumda." });
            }

            _context.Restore(category);
            await _context.SaveChangesAsync();

            return NoContent(); //204 
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);



            if (category == null)
            {
                return NotFound(new { message = $"ID'si {id} olan kategori bulunamadı." });
            }
            else if (category.RecordStatus == false)
            {
                return BadRequest(new { message = $"ID'si {id} olan {category.Name} adlı kategori zaten {category.ModifyDate} tarihinde silinmiş." });
            }

            //Gerçek silme yerine soft deletion işlemi yapılacak.
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent(); // 204
        }
    }
}
