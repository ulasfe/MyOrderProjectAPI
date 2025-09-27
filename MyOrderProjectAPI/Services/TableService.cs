using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.Data;
using MyOrderProjectAPI.DTOs;
using MyOrderProjectAPI.Extensions;
using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.Services
{
    public class TableService : ITableService
    {
        private readonly ApplicationDbContext _context;

        public TableService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TableDetailDTO>> GetAllTablesAsync()
        {
            //TableDetailDto'ya dönüştürürken, masanın CurrentOrderId'sini hesaplama işlemi:
            return await _context.Tables
                .Select(t => new TableDetailDTO
                {
                    Id = t.Id,
                    TableNumber = t.TableNumber,
                    Status = t.Status,
                    // Bu masaya ait durumu "Kapalı" olmayan (yani açık olan) siparişin ID'sini al.
                    CurrentOrderId = t.Orders.Any(o => o.Status != orderStatus.Kapali)
                                     ? (int?)t.Orders.FirstOrDefault(o => o.Status != orderStatus.Kapali)!.Id
                                     : null
                })
                .ToListAsync();
        }

        public async Task<TableDetailDTO?> GetTableByIdAsync(int id)
        {
            //Tek bir masa için detay çek
            return await _context.Tables
                .Where(t => t.Id == id)
                .Select(t => new TableDetailDTO
                {
                    Id = t.Id,
                    TableNumber = t.TableNumber,
                    Status = t.Status,
                    CurrentOrderId = t.Orders.Any(o => o.Status != orderStatus.Kapali)
                                     ? (int?)t.Orders.FirstOrDefault(o => o.Status != orderStatus.Kapali)!.Id
                                     : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TableDetailDTO> CreateTableAsync(TableCreateUpdateDTO tableDto)
        {
            var table = new Table
            {
                TableNumber = tableDto.TableNumber,
                Status = tableDto.Status, // Varsayılan "Boş" 
            };

            _context.Tables.Add(table);

            // Masa Numarası benzersiz olmalıdır UI
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException) // Veritabanı benzersizlik hatası
            {
                throw new InvalidOperationException("Bu masa numarası zaten mevcut.");
            }

            return await GetTableByIdAsync(table.Id) ?? throw new Exception("Masa oluşturulamadı.");
        }


        public async Task<bool> UpdateTableAsync(int id, TableCreateUpdateDTO tableDto)
        {
            var table = await _context.Tables.FindAsync(id);

            if (table == null) return false;

            // Yalnızca TableNumber ve Status alanlarını güncelle
            table.TableNumber = tableDto.TableNumber;
            table.Status = tableDto.Status;
            table.ModifyDate = tableDto.ModifyDate;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTableStatusAsync(int id, Status newStatus)
        {
            var table = await _context.Tables.FindAsync(id);

            if (table == null) return false;

            table.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTableAsync(int id)
        {
            var table = await _context.Tables.FindAsync(id);

            if (table is null) return false;
            if (!table.RecordStatus) throw new InvalidOperationException($"Id değeri {id} olan masa {table.ModifyDate} tarihinde zaten silinmiş.");
            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreTableAsync(int id)
        {
            var table = await _context.Tables.FindAsync(id);

            if (table is null) return false;
            if(table.RecordStatus) throw new InvalidOperationException($"Id değeri {id} olan masa zaten aktif.");
            _context.Restore(table);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
