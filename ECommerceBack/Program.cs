using System.Text;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ECommerceBack.API.Filters;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;
using ECommerceBack.Infrastructure.Repositories;
using ECommerceBack.Infrastructure.Services;
using ECommerceBack.ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Decorators;
using ECommerceBack.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. إضافة الخدمات الأساسية
// ============================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ============================================================
// 2. إضافة DbContext
// ============================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================================
// 3. تسجيل الـ Repositories
// ============================================================
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();

// ============================================================
// 4. تسجيل الـ Services الأساسية
// ============================================================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISalesSummaryBatchService, SalesSummaryBatchService>();

// ============================================================
// 5. تسجيل الخدمات المساعدة (Fake, Helpers)
// ============================================================
builder.Services.AddScoped<IEmailSender, FakeEmailSender>();
builder.Services.AddScoped<IInvoiceGenerator, FakeInvoiceGenerator>();
builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();
builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();
builder.Services.AddSingleton<IConcurrencyManager, ConcurrencyManager>();

// ============================================================
// 6. إضافة Façade (تبسيط العمليات المركبة)
// ============================================================
builder.Services.AddScoped<ICheckoutFacade, CheckoutFacade>();

// ============================================================
// 7. إضافة Decorators باستخدام Scrutor (AOP على مستوى الخدمة)
// ============================================================
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.Decorate<IOrderService, OrderServiceLoggingDecorator>();

builder.Services.AddScoped<ICartService, CartService>();
builder.Services.Decorate<ICartService, CartServiceLoggingDecorator>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.Decorate<IProductService, ProductServiceLoggingDecorator>();

// ============================================================
// 8. إعداد الفلاتر (AOP على مستوى API)
// ============================================================
builder.Services.AddScoped<TransactionFilter>();
builder.Services.AddScoped<GlobalExceptionFilter>();
builder.Services.AddScoped<PerformanceFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<TransactionFilter>();      // إدارة المعاملات (لـ POST, PUT, DELETE)
    options.Filters.Add<GlobalExceptionFilter>();  // معالجة الأخطاء العالمية
    options.Filters.Add<PerformanceFilter>();      // قياس أداء الـ Actions
});

// ============================================================
// 9. إعداد JWT Authentication
// ============================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "fallback-secret-key-min-32-chars-long!!!!");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ============================================================
// 10. إعداد Hangfire (للمهام غير المتزامنة والدفعات)
// ============================================================
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("HangfireConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
            PrepareSchemaIfNecessary = true
        }));
builder.Services.AddHangfireServer();

// ============================================================
// بناء التطبيق
// ============================================================
var app = builder.Build();

// ============================================================
// 11. إعداد الـ Middleware pipeline
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Hangfire Dashboard (محمي بـ Authorization Filter)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
});

// ============================================================
// 12. تهيئة قاعدة البيانات وإضافة البيانات التجريبية 
// ============================================================
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    await dbContext.Database.EnsureCreatedAsync();

//    const int targetOrders = 2000;
//    int currentOrders = await dbContext.Orders.CountAsync();
//    if (currentOrders < targetOrders)
//    {
//        var random = new Random();
//        var ordersToAdd = new List<Order>();
//        var orderItemsToAdd = new List<OrderItem>();

//        for (int i = currentOrders + 1; i <= targetOrders; i++)
//        {
//            var orderDate = DateTime.UtcNow.AddDays(-i);
//            var totalAmount = random.Next(50, 1000);

//            var order = new Order
//            {
//                UserId = 1,
//                OrderDate = orderDate,
//                TotalAmount = totalAmount,
//                Status = OrderStatus.Paid
//            };
//            ordersToAdd.Add(order);

//            int itemsCount = random.Next(1, 4);
//            for (int j = 0; j < itemsCount; j++)
//            {
//                orderItemsToAdd.Add(new OrderItem
//                {
//                    Order = order,
//                    ProductId = 1,
//                    Quantity = random.Next(1, 5),
//                    UnitPrice = 100
//                });
//            }

//            if (ordersToAdd.Count >= 500)
//            {
//                await dbContext.Orders.AddRangeAsync(ordersToAdd);
//                await dbContext.OrderItems.AddRangeAsync(orderItemsToAdd);
//                await dbContext.SaveChangesAsync();
//                ordersToAdd.Clear();
//                orderItemsToAdd.Clear();
//                Console.WriteLine($"[Seed] Added {i} orders so far...");
//            }
//        }

//        if (ordersToAdd.Any())
//        {
//            await dbContext.Orders.AddRangeAsync(ordersToAdd);
//            await dbContext.OrderItems.AddRangeAsync(orderItemsToAdd);
//            await dbContext.SaveChangesAsync();
//        }

//        Console.WriteLine($"[Seed] Successfully added {targetOrders} dummy orders with OrderItems.");
//    }
//    else
//    {
//        Console.WriteLine($"[Seed] Already have {currentOrders} orders (>= {targetOrders}), skipping order seeding.");
//    }
//}

// ============================================================
// 13. جدولة المهمة الدورية (Batch Processing - المتطلب الرابع)
// ============================================================
RecurringJob.AddOrUpdate<ISalesSummaryBatchService>(
    "daily-sales-summary",
    service => service.ProcessDailySalesSummaryAsync(DateTime.UtcNow.Date.AddDays(-1)),
    Cron.Daily(2, 0));

// ============================================================
// تشغيل التطبيق
// ============================================================
app.Run();