using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.Commons;
using MyOrderProjectAPI.Models;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace MyOrderProjectAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //DBSteler Veritabano tablolarını temsilen 
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Table> Tables { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            //IsoftDelete interface'i kullanan sınıfları ayırmak için kullanılan LINQ sorgusu:
            var entries = ChangeTracker.Entries().Where(e => e.Entity is ISoftDelete && (
                        e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

            foreach (var entityEntry in entries)
            {
                var entity = (ISoftDelete)entityEntry.Entity;
                var now = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedDate = now;
                    entity.RecordStatus = true;
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    entity.ModifyDate = now;
                }
                else if (entityEntry.State == EntityState.Deleted)
                {
                    // Soft Deletion işlemi için silme işlemi yerine update işlemi yap.
                    entityEntry.State = EntityState.Modified; 
                    entity.RecordStatus = false;             
                    entity.ModifyDate = now;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Product Fiyatı
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(10, 2)")
                .IsRequired();

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPriceAtTimeOfOrder)
                .HasColumnType("decimal(10, 2)")
                .IsRequired();

            modelBuilder.Entity<Order>()
               .Property(o => o.TotalAmount)
               .HasColumnType("decimal(10, 2)"); 

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(10, 2)")
                .IsRequired();

           
            modelBuilder.Entity<Table>()
                .HasIndex(t => t.TableNumber)
                .IsUnique();

            modelBuilder.Entity<Table>()
                .Property(t => t.Status)
                .HasDefaultValue(Status.Boş);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            //Bir sipariş silindiğinde, ona bağlı tüm sipariş satırları da silinir.
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bir sipariş silindiğinde, ona bağlı ödeme kayıtları da silinmelidir.
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");

                    var property = Expression.Property(parameter, nameof(ISoftDelete.RecordStatus));

                    var trueConstant = Expression.Constant(true);

                    var equals = Expression.Equal(property, trueConstant);

                    var lambda = Expression.Lambda(equals, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
