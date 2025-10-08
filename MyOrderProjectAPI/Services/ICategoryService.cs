using MyOrderProjectAPI.DTOs;

namespace MyOrderProjectAPI.Services
{
    public interface ICategoryService
    {

        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO?> GetCategoryByIdAsync(int id);

        Task<CategoryDTO> CreateCategoryAsync(CategoryCreateDTO categoryDTO);
        Task<bool> SoftDeleteCategoryAsync(int id);
        Task<bool> RestoreCategoryAsync(int id);
    }
}
