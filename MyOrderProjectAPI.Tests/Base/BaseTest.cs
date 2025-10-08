using global::MyOrderProjectAPI.Data;
using global::MyOrderProjectAPI.DTOs;
using global::MyOrderProjectAPI.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MyOrderProjectAPI.Tests.Base
{
    public class BaseTest : IDisposable
    {
        protected readonly ApplicationDbContext _context;
        private readonly SqliteConnection _connection;

        public BaseTest()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Database.EnsureCreated();

            SeedData();
        }

        private void SeedData()
        {
            _context.Categories.AddRange(
                new Category { Id = 1, Name = "İçecekler", RecordStatus = true, CreatedDate = DateTime.Now },
                new Category { Id = 2, Name = "Yiyecekler", RecordStatus = true, CreatedDate = DateTime.Now }
            );

            _context.Products.AddRange(
                new Product { Name = "Temel Tes Ürünü", Price = 10, RecordStatus = true, CreatedDate = DateTime.Now, CategoryId = 1, StockQuantity = 45 },
                new Product { Name = "İkinci Ürün", Price = 40, RecordStatus = true, CreatedDate = DateTime.Now, CategoryId = 2, StockQuantity = 70 }
            );

            List<Table> toBeAddedTableList = new List<Table>();
            for (int i = 1; i <= 15; i++)
            {
                toBeAddedTableList.Add(
                    new Table
                    {
                        TableNumber = "A" + i.ToString(),
                        CreatedDate = DateTime.Now,
                        Status = Status.Boş,
                        RecordStatus = true
                    });
            }
            _context.Tables.AddRange(toBeAddedTableList);

            _context.SaveChanges();
        }

        protected OrderCreateDTO GenerateDummyOrders(int tableId, List<OrderItemDTO> orderItemsDTO)
        {
            return new OrderCreateDTO
            {
                TableId = tableId,
                Items = orderItemsDTO
            };
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Close();
            _connection.Dispose();
        }
    }
}