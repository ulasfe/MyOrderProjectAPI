using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.Services
{
    public interface ITableService
    {
        Task<IEnumerable<TableDetailDTO>> GetAllTablesAsync();
        Task<TableDetailDTO?> GetTableByIdAsync(int id);
        Task<TableDetailDTO> CreateTableAsync(TableCreateUpdateDTO tableDto);
        Task<bool> UpdateTableAsync(int id, TableCreateUpdateDTO tableDto);
        Task<bool> DeleteTableAsync(int id);
        Task<bool> UpdateTableStatusAsync(int id, Status newStatus);
        Task<bool> RestoreTableAsync(int id);
    }
}
