using MyOrderProjectAPI.Data;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.Tests
{
    public static class TestDatabaseSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Veri zaten varsa tekrar eklemeyi önlemek iyi bir uygulamadır
            if (context.Categories.Any())
            {
                return; // Veri zaten eklenmiş, işlemi sonlandır
            }

            // --- KATEGORİLER ---
            context.Categories.AddRange(
                new Category { Id = 1, Name = "İçecekler", RecordStatus = true, CreatedDate = DateTime.Now },
                new Category { Id = 2, Name = "Yiyecekler", RecordStatus = true, CreatedDate = DateTime.Now }
            );

            // --- ÜRÜNLER ---
            context.Products.AddRange(
                new Product { Name = "Temel Tes Ürünü", Price = 10, RecordStatus = true, CreatedDate = DateTime.Now, CategoryId = 1, StockQuantity = 45 },
                new Product { Name = "İkinci Ürün", Price = 40, RecordStatus = true, CreatedDate = DateTime.Now, CategoryId = 2, StockQuantity = 70 }
            );

            // --- MASALAR ---
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
            context.Tables.AddRange(toBeAddedTableList);

            context.SaveChanges();
        }


        public static OrderCreateDTO GenerateDummyOrders(int tableId, List<OrderItemDTO> orderItemsDTO)
        {
            return new OrderCreateDTO
            {
                TableId = tableId,
                Items = orderItemsDTO
            };
        }
    }
}
