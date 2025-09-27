using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.Commons;
using MyOrderProjectAPI.Data;

namespace MyOrderProjectAPI.Extensions
{
    public static class SoftDeleteExtensions
    {
        public static void Restore<TEntity>(this ApplicationDbContext context, TEntity entity)
            where TEntity : class, ISoftDelete // Sadece ISoftDelete Interface'i kullanan sınıfları kabul et
        {
            // Kaydı aktif hale getiriyoruz.
            entity.RecordStatus = true;
            entity.ModifyDate = DateTime.UtcNow;

            // EF Core'da da nesnenin bu durumunu değişiriyoruz.
            context.Entry(entity).State = EntityState.Modified;
        }
    }
}
