using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Tests.Base;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

// Test projenizin namespace'i
namespace MyOrderProjectAPI.Tests.Integration
{
    // IntegrationTestsBase, CustomWebApplicationFactory'yi kullanır ve HttpClient'ı sağlar
    public class OrderControllerTests : IntegrationTestsBase
    {
        private const string BaseUrl = "/api/orders";

        // Constructor: Base sınıfın client ve configuration ayarlarını yükler
        public OrderControllerTests(CustomWebApplicationFactory<APIEntryPontForTests> factory)
            : base(factory)
        {
        }

        #region GET - Active Orders (Tüm Aktif Siparişler)

        [Fact]
        public async Task GetActiveOrders_ShouldReturn200AndActiveOrders_WhenAuthorized()
        {
            var response = await _client.GetAsync(BaseUrl);
            // Assert
            response.EnsureSuccessStatusCode(); // 200 OK
            var orders = await response.Content.ReadFromJsonAsync<IEnumerable<OrderDetailsDTO>>();

            Assert.NotNull(orders);
            // Varsayım: Seed data'da en az 1 aktif sipariş var (örn: OrderId = 101)
            Assert.True(orders.Any());
        }

        [Fact]
        public async Task GetActiveOrders_ShouldReturn401_WhenUnauthorized()
        {
            // Act
            var response = await _client.GetAsync(BaseUrl); // Tokersiz istek

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        #endregion

        #region GET - Single Order (Tekil Sipariş)

        [Fact]
        public async Task GetOrder_ShouldReturn200AndOrder_WhenOrderExists()
        {
            const int existingOrderId = 101; // Seed Data'dan gelen geçerli sipariş ID'si

            // Act
            var response = await _client.GetAsync($"{BaseUrl}/{existingOrderId}");

            // Assert
            response.EnsureSuccessStatusCode(); // 200 OK
            var order = await response.Content.ReadFromJsonAsync<OrderDetailsDTO>();
            Assert.NotNull(order);
            Assert.Equal(existingOrderId, order.Id);
        }

        [Fact]
        public async Task GetOrder_ShouldReturn404_WhenOrderDoesNotExist()
        {
            // Arrange 
            const int nonExistingOrderId = 999;

            // Act
            var response = await _client.GetAsync($"{BaseUrl}/{nonExistingOrderId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST - Create Order (Yeni Sipariş Oluşturma)

        [Fact]
        public async Task CreateOrder_ShouldReturn201Created_WhenDataIsValid()
        {
            // Arrange

            var validTableId = 1; // Seed Data'dan gelen geçerli masa ID'si
            var validProductId = 1; // Seed Data'dan gelen geçerli ürün ID'si

            var createDto = new OrderCreateDTO
            {
                TableId = validTableId,
                Items = new List<OrderItemDTO>
                {
                    new OrderItemDTO { ProductId = validProductId, Quantity = 2 }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(createDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync(BaseUrl, content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode); // 201 Created
            var newOrder = await response.Content.ReadFromJsonAsync<OrderDetailsDTO>();

            Assert.NotNull(newOrder);
            Assert.True(newOrder.Id > 0);
            Assert.Equal(validTableId, newOrder.TableId);

            // Response Header'da doğru Location URI'si döndüğünü kontrol et
            //Assert.Contains(nameof(GetOrder), response.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task CreateOrder_ShouldReturn400BadRequest_WhenTableIdIsInvalid()
        {
            // Arrange

            var invalidTableId = 999; // Geçersiz Masa ID'si
            var validProductId = 1;

            var createDto = new OrderCreateDTO
            {
                TableId = invalidTableId,
                Items = new List<OrderItemDTO>
                {
                    new OrderItemDTO { ProductId = validProductId, Quantity = 1 }
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(createDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync(BaseUrl, content);

            // Assert
            // Burada ModelState.IsValid geçecek ancak IOrderService (iş mantığı) NotFound fırlatmalıdır.
            // Global Exception Handler'ınız bunu 400 Bad Request veya 404 NotFound'a çevirecektir.
            // Global Exception Handler'ınızın InvalidOperationException'ı 400'e çevirdiğini varsayıyoruz.
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region PUT - Add Items (Siparişe Ürün Ekleme)

        [Fact]
        public async Task AddItems_ShouldReturn200Ok_WhenItemsAreAddedSuccessfully()
        {
            // Arrange

            const int existingOrderId = 101;
            var newProductId = 1; // Eklenecek yeni ürün ID'si

            var itemsToAdd = new List<OrderItemDTO>
            {
                new OrderItemDTO { ProductId = newProductId, Quantity = 3 }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(itemsToAdd),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"{BaseUrl}/{existingOrderId}/items", content);

            // Assert
            response.EnsureSuccessStatusCode(); // 200 OK
            var updatedOrder = await response.Content.ReadFromJsonAsync<OrderDetailsDTO>();

            Assert.NotNull(updatedOrder);
            // Siparişin toplam ürün sayısının arttığını kontrol et (Seed verisine göre)
            // (Örn: Eğer Seed'de 1 ürün varsa, şimdi 2 olmalı)
            Assert.True(updatedOrder.Items.Count > 1);
        }

        [Fact]
        public async Task AddItems_ShouldReturn400BadRequest_WhenItemsListIsEmpty()
        {
            // Arrange

            const int existingOrderId = 101;

            var itemsToAdd = new List<OrderItemDTO>(); // Boş liste

            var content = new StringContent(
                JsonSerializer.Serialize(itemsToAdd),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"{BaseUrl}/{existingOrderId}/items", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("Eklenmek istenen ürünler boş olamaz.", errorContent);
        }

        [Fact]
        public async Task AddItems_ShouldReturn404NotFound_WhenOrderDoesNotExist()
        {
            // Arrange

            const int nonExistingOrderId = 999;
            var itemsToAdd = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } };

            var content = new StringContent(
                JsonSerializer.Serialize(itemsToAdd),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PutAsync($"{BaseUrl}/{nonExistingOrderId}/items", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST - Cancel Order (Sipariş İptal)

        [Fact]
        public async Task CancelOrder_ShouldReturn204NoContent_WhenCancellationIsSuccessful()
        {
            // Arrange

            // Varsayım: 102 ID'li sipariş seed data'da aktiftir.
            const int cancellableOrderId = 102;

            // Act
            var response = await _client.PostAsync($"{BaseUrl}/{cancellableOrderId}/cancel", null);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // 204 NoContent
        }

        [Fact]
        public async Task CancelOrder_ShouldReturn404NotFound_WhenOrderIsAlreadyCancelledOrDoesNotExist()
        {
            // Arrange

            // Varsayım: 103 ID'li sipariş seed data'da zaten iptal edilmiş/kapatılmış.
            const int nonCancellableOrderId = 103;

            // Act
            var response = await _client.PostAsync($"{BaseUrl}/{nonCancellableOrderId}/cancel", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        #endregion

        #region POST - Process Payment (Ödeme İşlemi)

        [Fact]
        public async Task ProcessPayment_ShouldReturn204NoContent_WhenPaymentIsSuccessful()
        {
            // Arrange

            const int payableOrderId = 101; // Aktif ve ödenebilir sipariş

            var paymentDto = new PaymentRequestDTO
            {
                Amount = 50.00M, // Doğru ödeme tutarı (Seed data'dan gelen siparişin toplam tutarı)
                PaymentMethod = Models.paymentMethod.KrediKarti
            };

            var content = new StringContent(
                JsonSerializer.Serialize(paymentDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync($"{BaseUrl}/{payableOrderId}/pay", content);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode); // 204 NoContent
        }

        [Fact]
        public async Task ProcessPayment_ShouldReturn404NotFound_WhenOrderDoesNotExist()
        {
            // Arrange

            const int nonExistingOrderId = 999;
            var paymentDto = new PaymentRequestDTO { Amount = 10.00M, PaymentMethod = Models.paymentMethod.Nakit };

            var content = new StringContent(
                JsonSerializer.Serialize(paymentDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync($"{BaseUrl}/{nonExistingOrderId}/pay", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Negatif Senaryo: Ödeme tutarı Model Doğrulamasını geçemezse (Örn: Amount 0 veya negatif)
        [Fact]
        public async Task ProcessPayment_ShouldReturn400BadRequest_WhenAmountIsInvalid()
        {
            // Arrange

            const int payableOrderId = 101;

            var paymentDto = new PaymentRequestDTO
            {
                Amount = 0, // Geçersiz Tutar (Model Doğrulama Hatası varsayımı)
                PaymentMethod = Models.paymentMethod.Nakit
            };

            var content = new StringContent(
                JsonSerializer.Serialize(paymentDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync($"{BaseUrl}/{payableOrderId}/pay", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion
    }
}