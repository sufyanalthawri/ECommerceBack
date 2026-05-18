using Microsoft.EntityFrameworkCore;
using ECommerceBack.Core.Entities;

namespace ECommerceBack.Infrastructure.Data
{
    /// <summary>
    /// السياق الرئيسي لقاعدة البيانات (DbContext).
    /// يدير جميع الكيانات ويكوّن العلاقات والفهارس والقيود.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<SalesDailySummary> SalesDailySummaries => Set<SalesDailySummary>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. التزامن المتفائل (Optimistic Concurrency)
            modelBuilder.Entity<Product>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            // 2. دقة الأعداد العشرية
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentTransaction>()
                .Property(pt => pt.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SalesDailySummary>()
                .Property(s => s.TotalSales)
                .HasPrecision(18, 2);

            // 3. العلاقات (Relationships)
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.PaymentTransaction)
                .WithOne(pt => pt.Order)
                .HasForeignKey<PaymentTransaction>(pt => pt.OrderId);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Order)
                .WithMany()
                .HasForeignKey(i => i.OrderId);

            modelBuilder.Entity<SalesDailySummary>()
                .HasOne(s => s.TopProduct)
                .WithMany()
                .HasForeignKey(s => s.TopProductId)
                .OnDelete(DeleteBehavior.SetNull);

            // 4. الفهارس (Indexes)
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .HasDatabaseName("IX_Products_Name");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderDate)
                .HasDatabaseName("IX_Orders_OrderDate");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId)
                .HasDatabaseName("IX_Orders_UserId");

            modelBuilder.Entity<SalesDailySummary>()
                .HasIndex(s => s.Date)
                .IsUnique()
                .HasDatabaseName("IX_SalesDailySummaries_Date");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            modelBuilder.Entity<PaymentTransaction>()
                .HasIndex(pt => pt.OrderId)
                .IsUnique()
                .HasDatabaseName("IX_PaymentTransactions_OrderId");

            // 5. قيود مستوى الجدول (Check Constraints)
            modelBuilder.Entity<Product>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Product_Stock", "Stock >= 0"));

            modelBuilder.Entity<Order>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Order_TotalAmount", "TotalAmount >= 0"));

            modelBuilder.Entity<CartItem>()
                .ToTable(tb => tb.HasCheckConstraint("CK_CartItem_Quantity", "Quantity > 0"));
        }
    }
}