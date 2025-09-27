using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Services;
using MyOrderProjectAPI.Tests.Base;
using MyOrderProjectAPI.Tests.Base.MyOrderProjectAPI.Tests.Base;
using System.Threading.Tasks;
using Xunit;

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

            allTables.Should().HaveCount(_context.Tables.Where(k=> k.RecordStatus).Count());
        }
    }
}