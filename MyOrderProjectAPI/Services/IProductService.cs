using MyOrderProjectAPI.DTOs;

namespace MyOrderProjectAPI.Services
{
    public interface IProductService
    {
        // CRUD Operasyonları 
        Task<IEnumerable<ProductDetailDTO>> GetAllProductsAsync();

        // ID'ye göre tek bir ürünü DTO olarak getir
        Task<ProductDetailDTO?> GetProductByIdAsync(int id);

        // Yeni bir ürün oluştur ve oluşturulan ürünü DTO olarak döndür
        Task<ProductDetailDTO> CreateProductAsync(ProductCreateUpdateDTO productDTO);

        // Ürünü güncelle
        Task<bool> UpdateProductAsync(int id, ProductCreateUpdateDTO productDTO);

        // Ürünü sil
        Task<bool> DeleteProductAsync(int id);

        // Silinmiş kaydı geri getir
        Task<bool> RestoreProductAsync(int id);
    }
}
