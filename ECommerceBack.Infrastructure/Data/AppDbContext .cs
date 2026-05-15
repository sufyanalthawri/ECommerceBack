using Microsoft.EntityFrameworkCore;
using ECommerceBack.Core.Entities;

namespace ECommerceBack.Infrastructure.Data
{
    /// <summary>
    /// السياق الرئيسي لقاعدة البيانات (DbContext) .
    /// يدير جميع الكيانات ويكوّن العلاقات والفهارس والقيود.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>جدول المستخدمين.</summary>
        public DbSet<User> Users => Set<User>();

        /// <summary>جدول المنتجات  ت.</summary>
        public DbSet<Product> Products => Set<Product>();

        /// <summary>جدول سلات التسوق.</summary>
        public DbSet<Cart> Carts => Set<Cart>();

        /// <summary>جدول عناصر السلة.</summary>
        public DbSet<CartItem> CartItems => Set<CartItem>();

        /// <summary>جدول الطلبات النهائية.</summary>
        public DbSet<Order> Orders => Set<Order>();

        /// <summary>جدول عناصر الطلبات.</summary>
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        /// <summary>جدول معاملات الدفع.</summary>
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

        /// <summary>جدول الفواتير.</summary>
        public DbSet<Invoice> Invoices => Set<Invoice>();

        /// <summary>جدول ملخص المبيعات اليومية ـ  .</summary>
        public DbSet<SalesDailySummary> SalesDailySummaries => Set<SalesDailySummary>();

        /// <summary>
        /// تكوين العلاقات، الفهارس، القيود، ودقة الأعمدة.
        /// </summary>
        /// <param name="modelBuilder">باني نموذج EF Core.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== 1. التزامن المتفائل (Optimistic Concurrency) ==========
            modelBuilder.Entity<Product>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            // ========== 2. تحديد دقة الأعداد العشرية (لتفادي تحذيرات EF) ==========
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

            // ========== 3. العلاقات (Relationships) ==========
            // User - Cart (One-to-One)
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart - CartItems (One-to-Many)
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId);

            // CartItem - Product (Many-to-One)
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);

            // User - Orders (One-to-Many)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            // Order - OrderItems (One-to-Many)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            // OrderItem - Product (Many-to-One)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            // Order - PaymentTransaction (One-to-One)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.PaymentTransaction)
                .WithOne(pt => pt.Order)
                .HasForeignKey<PaymentTransaction>(pt => pt.OrderId);

            // Invoice - Order (Many-to-One)
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Order)
                .WithMany()
                .HasForeignKey(i => i.OrderId);

            // SalesDailySummary - Product (Optional relationship)
            modelBuilder.Entity<SalesDailySummary>()
                .HasOne(s => s.TopProduct)
                .WithMany()
                .HasForeignKey(s => s.TopProductId)
                .OnDelete(DeleteBehavior.SetNull);   // عند حذف منتج، يبقى الملخص مع TopProductId = null

            // ========== 4. الفهارس لتحسين الأداء (Indexes) ==========
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

            // ========== 5. قيود مستوى الجدول (Check Constraints) ==========
            modelBuilder.Entity<Product>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Product_Stock", "Stock >= 0"));

            modelBuilder.Entity<Order>()
                .ToTable(tb => tb.HasCheckConstraint("CK_Order_TotalAmount", "TotalAmount >= 0"));

            modelBuilder.Entity<CartItem>()
                .ToTable(tb => tb.HasCheckConstraint("CK_CartItem_Quantity", "Quantity > 0"));
        }
    }
}