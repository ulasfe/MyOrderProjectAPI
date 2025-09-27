using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;
using MyOrderProjectAPI.Services;
using MyOrderProjectAPI.Tests.Base;
using MyOrderProjectAPI.Tests.Base.MyOrderProjectAPI.Tests.Base;
using System.Threading.Tasks;
using Xunit;

namespace MyOrderProjectAPI.Tests.ServiceTests
{
    public class OrderServiceTests : BaseTest
    {
        private readonly OrderService _orderService;

        public OrderServiceTests() : base()
        {
            _orderService = new OrderService(_context);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldReturnOrderDetailDTO()
        {
            var dummyOrderItems = new List<OrderItemDTO>
            {
                new OrderItemDTO { ProductId = 1, Quantity = 2 },
                new OrderItemDTO { ProductId = 2, Quantity = 1 }
            };

            var newOrder = GenerateDummyOrders(1, dummyOrderItems);
            var orderTable = await _context.Tables.FindAsync(1);
            orderTable.Should().NotBeNull();
            var detailDTO = await _orderService.CreateOrderAsync(newOrder);

            detailDTO.Should().NotBeNull();
            detailDTO.TotalAmount.Should().Be(60);
            detailDTO.TableNumber.Should().Be(orderTable.TableNumber);
        }


        [Fact]
        public async Task CancelOrderAsync_ShouldReturnTrue()
        {
            int orderIdToDelete = 1;
            await CreateOrderAsync_ShouldReturnOrderDetailDTO();
            var result = await _orderService.CancelOrderAsync(orderIdToDelete);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AddItemsToOrderAsync_ShoudlReturnOrderDetailDTO()
        {
            int orderIdToModify = 1;
            await CreateOrderAsync_ShouldReturnOrderDetailDTO(); //Burada Toplam tutat 60 olmalı halihazırda 
            var newItems = new List<OrderItemDTO>{
                new OrderItemDTO { ProductId = 1 , Quantity = 3}, // Ütün fiyatı 10 toplam tutara 10 * 3 = 30 daha eklenmeli
                new OrderItemDTO { ProductId = 2 , Quantity = 2}, // Ürün fiyatı 40 toplam tutara 40 * 2 = 80 daha eklenmeli
            };

            var orderDetailsDTO = await _orderService.AddItemsToOrderAsync(orderIdToModify, newItems);

            orderDetailsDTO.Should().NotBeNull();
            orderDetailsDTO.Status.Should().Be(orderStatus.Acik);
            orderDetailsDTO.TotalAmount.Should().Be(170); // 60(önceden) +  30 + 80 = 170 olmalı

        }

        [Fact]
        public async Task GetActiveOrdersAsync_ShouldReturnOrderDetailsDTOList()
        {
            var activeOrders = await _orderService.GetActiveOrdersAsync();
            activeOrders.Count().Should().Be(_context.Orders.Count());
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldReturnTrue()
        {
            await AddItemsToOrderAsync_ShoudlReturnOrderDetailDTO();
            var susccessPayment = await _orderService.ProcessPaymentAsync(1, 170, paymentMethod.KrediKarti);
            susccessPayment.Should().BeTrue();
        }
    }
}