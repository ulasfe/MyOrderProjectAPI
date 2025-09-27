using Microsoft.AspNetCore.Mvc;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;
using MyOrderProjectAPI.Services;
using System.ComponentModel.DataAnnotations;

namespace MyOrderProjectAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        //Tüm aktif siparişleri görüntüleme işlemi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetailsDTO>>> GetActiveOrders()
        {
            var orders = await _orderService.GetActiveOrdersAsync();
            return Ok(orders); // 200 OK
        }

       //Tek bir sipariş getirme işlemi
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailsDTO>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound(); // 404 Not Found
            }

            return Ok(order); // 200 OK
        }

       //Yeni sipariş oluşturma
        [HttpPost]
        public async Task<ActionResult<OrderDetailsDTO>> CreateOrder([FromBody] OrderCreateDTO orderDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 
            }

            try
            {
                var newOrder = await _orderService.CreateOrderAsync(orderDTO);

                // Başarılı oluşturma
                return CreatedAtAction(nameof(GetOrder), new { id = newOrder.Id }, newOrder);
            }
            catch (InvalidOperationException ex)
            {
                // Masa bulunamadı, stok yetersiz veya masa dolu hatası
                return Conflict(new { message = ex.Message }); // 409 
            }
            catch (Exception ex)
            {
                // Diğer beklenmedik hatalar
                return StatusCode(500, new { message = "Sipariş oluşturulurken beklenmedik bir hata oluştu.", error = ex.Message });
            }
        }

       
        //Siparişe ürün ekleme
        [HttpPut("{id}/items")]
        public async Task<IActionResult> AddItems(int id, [FromBody] List<OrderItemDTO> items)
        {
            if (items == null || !items.Any())
            {
                return BadRequest("Eklenmek istenen ürünler boş olamaz.");
            }

            try
            {
                var updatedOrder = await _orderService.AddItemsToOrderAsync(id, items);

                if (updatedOrder == null)
                {
                    return NotFound();
                }

                return Ok(updatedOrder);// 200
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        
        //sipariş iptal işlemi
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var success = await _orderService.CancelOrderAsync(id);

            if (!success)
            {
                return NotFound(); // 404 Sipariş yoksa veya zaten kapalıysa
            }

            return NoContent(); // 204
        }

        //Ödeme işlemi
        [HttpPost("{id}/pay")]
        public async Task<IActionResult> ProcessPayment(int id, [FromBody] PaymentRequestDTO paymentDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _orderService.ProcessPaymentAsync(id, paymentDTO.Amount, paymentDTO.PaymentMethod);

            if (!success)
            {
                return NotFound(); // 404 
            }

            return NoContent(); // 204 
        }
    }
    public class PaymentRequestDTO
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ödeme miktarı 0'dan büyük olmalıdır.")]
        public decimal Amount { get; set; }

        [Required]
        public paymentMethod PaymentMethod { get; set; } = paymentMethod.Nakit;
    }
}
