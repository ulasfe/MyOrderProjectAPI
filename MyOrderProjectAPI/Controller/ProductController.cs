using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Services;

namespace MyOrderProjectAPI.Controller
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        // DI
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDetailDTO>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products); // 200 
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDetailDTO>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product is null)
            {
                return NotFound(); // 404
            }

            return Ok(product); // 200 
        }


        [HttpPost]
        public async Task<ActionResult<ProductDetailDTO>> PostProduct([FromBody] ProductCreateUpdateDTO productDTO)
        {
            //gerekli alanların gelip gelmediğini kontrol etme işlemi.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400
            }

            // DÜZELTME: Try-catch bloğu kaldırıldı.
            // Oluşabilecek tüm hatalar (iş mantığı veya beklenmedik hatalar)
            // Global Exception Handler'a fırlatılacaktır.
            var newProduct = await _productService.CreateProductAsync(productDTO);

            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, newProduct); // 201 ve URI değeri 
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, [FromBody] ProductCreateUpdateDTO productDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400
            }

            var success = await _productService.UpdateProductAsync(id, productDTO);

            if (!success)
            {
                return NotFound(); // 404 
            }

            return NoContent(); // 204 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _productService.DeleteProductAsync(id);

            if (!success)
            {
                return NotFound(); // 404
            }

            return NoContent(); // 204
        }

        [HttpPost("{id}/restore")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreProduct(int id)
        {
            var success = await _productService.RestoreProductAsync(id);

            if (!success)
            {
                return NotFound(new { message = $"ID'si {id} olan ürün bulunamadı, ya da önceden silinmiş." });
            }

            return NoContent(); // 204.
        }
    }
}