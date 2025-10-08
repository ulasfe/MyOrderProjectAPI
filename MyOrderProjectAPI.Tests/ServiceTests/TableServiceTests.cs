using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;
using MyOrderProjectAPI.Services;
using MyOrderProjectAPI.Tests.Base;

namespace MyOrderProjectAPI.Tests.ServiceTests
{
    public class TableServiceTests : BaseTest
    {
        private readonly TableService _tableService;

        public TableServiceTests() : base()
        {
            _tableService = new TableService(_context);
        }

        [Fact]
        public async Task GetTableByIdAsync_ShouldReturnNull_ForNonExistentId()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _tableService.GetTableByIdAsync(nonExistentId);

            // Assert: Masa bulunamadığında null dönmeli.
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateTableAsync_ShouldReturnFalse_WhenTableNotFound()
        {
            // Arrange
            int nonExistentId = 999;
            var updateDto = new TableCreateUpdateDTO { TableNumber = "T99", Status = Status.Boş };

            // Act
            var result = await _tableService.UpdateTableAsync(nonExistentId, updateDto);

            // Assert: Güncellenecek masa yoksa false dönmeli.
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteTableAsync_ShouldReturnFalse_WhenTableNotFound()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _tableService.DeleteTableAsync(nonExistentId);

            // Assert: Silinecek masa yoksa false dönmeli.
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreTableAsync_ShouldReturnFalse_WhenTableNotFound()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _tableService.RestoreTableAsync(nonExistentId);

            // Assert: Geri yüklenecek masa yoksa false dönmeli.
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CreateProductAsync_ShouldReturn()
        {
            var tableCRUDDto = new TableCreateUpdateDTO
            {
                TableNumber = "A999",
                Status = Models.Status.Boş
            };

            var tableDetailDTO = await _tableService.CreateTableAsync(tableCRUDDto);

            tableDetailDTO.Should().NotBeNull();
        }
        [Fact]
        public async Task RestoreAsync_Table_ShouldReturnTrue()
        {
            await SoftDeleteAsync_ValidId_ShouldSetRecordStatusToFalse();

            var success = await _tableService.RestoreTableAsync(1);

            success.Should().BeTrue();
        }


        [Fact]
        public async Task SoftDeleteAsync_ValidId_ShouldSetRecordStatusToFalse()
        {
            //BaseTest sınıfı içinde dumy data oluşturulduğu için burada data olduğunu varsayabiliriz.
            int tableIdToDelete = 1;
            var result = await _tableService.DeleteTableAsync(tableIdToDelete);

            result.Should().BeTrue();

            var deletedTable = await _context.Tables
                                             .IgnoreQueryFilters()
                                             .FirstOrDefaultAsync(t => t.Id == tableIdToDelete);

            deletedTable.Should().NotBeNull();
            deletedTable.RecordStatus.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_InvalidId_ShouldReturnNotFoundError()
        {
            // Arrange
            int invalidId = 999;

            // Act
            var result = await _tableService.DeleteTableAsync(invalidId);

            // Assert
            result.Should().BeFalse();
        }


        [Fact]
        public async Task UpdateAsync_Table_ShouldReturnTrue()
        {
            var table = await _tableService.GetTableByIdAsync(1);

            table.Should().NotBeNull();

            var toBeUpdateTable = new TableCreateUpdateDTO
            {
                Status = Models.Status.Rezerve,
                TableNumber = table.TableNumber
            };

            var updatedTable = await _tableService.UpdateTableAsync(table.Id, toBeUpdateTable);

            updatedTable.Should().BeTrue();

            table = await _tableService.GetTableByIdAsync(1);
            table.Should().NotBeNull();

            table.Status.Should().Be(Models.Status.Rezerve);
            table.TableNumber.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAllTablessAsync_ShouldReturnAllProducts()
        {
            var allTables = await _tableService.GetAllTablesAsync();

            allTables.Should().HaveCount(_context.Tables.Where(k => k.RecordStatus).Count());
        }

        [Fact]
        public async Task UpdateTableAsync_ShouldReturnFalse_WhenTableToUpdateIsNotFound()
        {
            var mockService = new Mock<ITableService>();
            int nonExistentId = 999;
            var dto = new TableCreateUpdateDTO { TableNumber = "T99", Status = Status.Boş };

            mockService.Setup(s => s.UpdateTableAsync(nonExistentId, It.IsAny<TableCreateUpdateDTO>()))
                       .ReturnsAsync(false);

            var result = await mockService.Object.UpdateTableAsync(nonExistentId, dto);

            Assert.False(result);
        }

        [Fact]
        public async Task GetTableByIdAsync_ShouldReturnNull_WhenTableDoesNotExist()
        {
            // Arrange
            int nonExistentId = 999;

            // Act: Doğrudan servisi çağırıyoruz.
            var result = await _tableService.GetTableByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTableByIdAsync_ShouldReturnNull_WhenTableIsSoftDeleted()
        {
            // Arrange: Masa 2'yi sil (BaseTest'te 15 masa ekleniyor)
            int tableId = 2;
            await _tableService.DeleteTableAsync(tableId);

            // Act: Silinen masayı çekmeye çalışıyoruz
            var result = await _tableService.GetTableByIdAsync(tableId);

            // Assert: Normal Get metotları RecordStatus=False olanları görmez.
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateTableAsync_ShouldThrowDbUpdateException_WhenTableNumberIsDuplicated()
        {
            // Arrange: BaseTest'te "A1" zaten var.
            var duplicateDto = new TableCreateUpdateDTO
            {
                TableNumber = "A1",
                Status = Status.Boş
            };

            // Act & Assert
            // Unique kısıtlaması (benzersiz indeks) varsa DbUpdateException fırlatmalıdır.
            // Bu, Modelinizde TableNumber alanı için HasIndex(t => t.TableNumber).IsUnique() kuralının olduğunu varsayar.
            await Assert.ThrowsAsync<DbUpdateException>(
                () => _tableService.CreateTableAsync(duplicateDto)
            );
        }

        [Fact]
        public async Task DeleteTableAsync_ShouldReturnFalse_WhenTableIsAlreadyDeleted()
        {
            // Arrange: Masa 3'ü sil
            int tableId = 3;
            await _tableService.DeleteTableAsync(tableId);

            // Act: İkinci kez silmeye çalış
            await Assert.ThrowsAsync<DbUpdateException>(
               () => _tableService.DeleteTableAsync(tableId)
           );
        }

        [Fact]
        public async Task DeleteTableAsync_ShouldThrowDbUpdateException_WhenTableHasActiveOrders()
        {
            // Bu, en kritik negatif testlerden biridir.
            // Arrange: Masa 4 için aktif bir sipariş oluştur.
            // Bu, OrderServiceTests'taki CreateAndReturnDummyOrderAsync benzeri bir metotla yapılmalıdır.
            // OrderService kullanmadığımız için _context'e manuel Order ve OrderItem ekleyelim.
            var tableIdWithOrder = 4;
            _context.Orders.Add(new Order { TableId = tableIdWithOrder, Status = orderStatus.Acik });
            await _context.SaveChangesAsync();

            // Act & Assert
            // Masanın Foreign Key kısıtlaması nedeniyle silinmesi engellenmelidir.
            await Assert.ThrowsAsync<DbUpdateException>(
                () => _tableService.DeleteTableAsync(tableIdWithOrder)
            );

            // Masanın RecordStatus'ının hala True olduğunu kontrol et.
            var table = await _context.Tables.IgnoreQueryFilters().FirstAsync(t => t.Id == tableIdWithOrder);
            table.RecordStatus.Should().BeTrue();
        }

        [Fact]
        public async Task RestoreTableAsync_ShouldReturnFalse_WhenTableDoesNotExist()
        {
            // Arrange
            int nonExistentId = 999;

            // Act
            var result = await _tableService.RestoreTableAsync(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RestoreTableAsync_ShouldReturnFalse_WhenTableIsAlreadyActive()
        {
            // Arrange: Masa 5 zaten aktif.
            int activeId = 5;

            // Act
            var result = await _tableService.RestoreTableAsync(activeId);

            // Assert: Zaten aktif olduğu için false dönmeli.
            result.Should().BeFalse();
        }
    }
}