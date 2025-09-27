using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyOrderProjectAPI.Tests.Base
{
    using global::MyOrderProjectAPI.Data;
    using global::MyOrderProjectAPI.DTOs;
    using global::MyOrderProjectAPI.Models;
    using Microsoft.EntityFrameworkCore;

    namespace MyOrderProjectAPI.Tests.Base
    {
        public class BaseTest : IDisposable
        {
            protected readonly ApplicationDbContext _context;

            public BaseTest()
            {
                //Her test için yeni ve benzersiz bir InMemory veritabanı 
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                _context = new ApplicationDbContext(options);

                SeedData();
            }

            private void SeedData()
            {
                _context.Categories.AddRange(
                    new Category { Id = 1, Name = "İçecekler", RecordStatus = true, CreatedDate = DateTime.Now },
                    new Category { Id = 2, Name = "Yiyecekler", RecordStatus = true, CreatedDate = DateTime.Now }
                );

                _context.Products.AddRange(
                    new Product { Name = "Temel Tes Ürünü", Price = 10, RecordStatus = true, CreatedDate = DateTime.Now, CategoryId = 1 , StockQuantity = 45},
                    new Product { Name = "İkinci Ürün", Price = 40, RecordStatus = true, CreatedDate = DateTime.Now, CategoryId = 2 , StockQuantity = 70 }
                );

                List<Table> toBeAddedTableList = new List<Table>();

                // Masa numaralarının A1,A2,A3..... Şeklinde oluşması için for döngüsü kullan
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
                // Her test bittiğinde inmemory veritabanını temizle 
                // Bazı case'ler için bu method devre dışı bırakılabilir.
                _context.Database.EnsureDeleted();
                _context.Dispose();
            }
        }
    }
}
