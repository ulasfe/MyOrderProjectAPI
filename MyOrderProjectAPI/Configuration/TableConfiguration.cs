using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyOrderProjectAPI.Models;

namespace MyOrderProjectAPI.Configuration
{
    public class TableConfiguration : IEntityTypeConfiguration<Table>
    {
        public void Configure(EntityTypeBuilder<Table> builder)
        {
            // Temel kayıt atamaları için
            builder.HasKey(k => k.Id);

            // Özellik kısıtlamaları
            builder.Property(k => k.TableNumber)
                .IsRequired()
                .HasMaxLength(10);
            
        }
    }
}
