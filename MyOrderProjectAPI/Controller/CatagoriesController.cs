using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Services;

namespace MyOrderProjectAPI.Controller
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

       
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoryDTO>> PostCategory(CategoryCreateDTO categoryDTO)
        {
            // DÜZELTME: Try-catch bloğu kaldırıldı.
            // ArgumentException, InvalidOperationException ve diğer tüm hatalar
            // artık Global Exception Handler'a fırlatılacak.
            var createdCategoryDTO = await _categoryService.CreateCategoryAsync(categoryDTO);

            return CreatedAtAction(nameof(GetCategory), new { id = createdCategoryDTO.Id }, createdCategoryDTO);
        }

       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {

            var categoryDTO = await _categoryService.GetCategoryByIdAsync(id);

            if (categoryDTO == null)
            {
                return NotFound();
            }

            return Ok(categoryDTO);
        }

       
        [HttpPost("{id}/restore")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreCategory(int id)
        {
            var result = await _categoryService.RestoreCategoryAsync(id);

            if (!result)
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null) return NotFound(new { message = $"ID'si {id} olan kategori bulunamadı." });

                return BadRequest(new { message = $"ID'si {id} olan kategori zaten aktif durumda." });
            }

            return NoContent();
        }

       
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.SoftDeleteCategoryAsync(id);

            if (!result)
            {
                // SoftDeleteCategoryAsync false döndürüyorsa ya bulunamamıştır ya da zaten silinmiştir.
                // Bu durumda (basit tutmak için) sadece NotFound dönelim ve detay hata mesajı serviste kalsın.
                return NotFound(new { message = $"ID'si {id} olan kategori bulunamadı veya zaten silinmiş." });
            }

            return NoContent();
        }
    }
}