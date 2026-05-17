using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;
using ECommerceBack.Infrastructure.Repositories;
using ECommerceBack.Infrastructure.Services;
using ECommerceBack.Core.Entities;

[MemoryDiagnoser(true)]
[SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 2, iterationCount: 5)]
public class OrderConcurrencyBenchmark
{
    private ServiceProvider _serviceProvider_NoLimit;
    private ServiceProvider _serviceProvider_WithLimit;
    private IOrderService _orderService_NoLimit;
    private IOrderService _orderService_WithLimit;

    [Params(5, 10, 30)]
    public int ConcurrentRequests { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        string connectionString = "Server=(localdb)\\mssqllocaldb;Database=ECommerceDb;Trusted_Connection=True;MultipleActiveResultSets=false";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        // ----- الحالة 1: بدون ConcurrencyManager -----
        var services1 = new ServiceCollection();
        services1.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
        services1.AddScoped<IUserRepository, UserRepository>();
        services1.AddScoped<IProductRepository, ProductRepository>();
        services1.AddScoped<ICartRepository, CartRepository>();
        services1.AddScoped<IOrderRepository, OrderRepository>();
        services1.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services1.AddScoped<IPaymentGateway, FakePaymentGateway>();
        services1.AddScoped<IOrderService, OrderService_NoLimit>();
        _serviceProvider_NoLimit = services1.BuildServiceProvider();
        _orderService_NoLimit = _serviceProvider_NoLimit.GetRequiredService<IOrderService>();

        // ----- الحالة 2: مع ConcurrencyManager (الحد = 10) -----
        var services2 = new ServiceCollection();
        services2.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
        services2.AddScoped<IUserRepository, UserRepository>();
        services2.AddScoped<IProductRepository, ProductRepository>();
        services2.AddScoped<ICartRepository, CartRepository>();
        services2.AddScoped<IOrderRepository, OrderRepository>();
        services2.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services2.AddScoped<IPaymentGateway, FakePaymentGateway>();
        services2.AddSingleton<IConcurrencyManager, ConcurrencyManager>();
        services2.AddScoped<IOrderService, OrderService_WithLimit>();
        _serviceProvider_WithLimit = services2.BuildServiceProvider();
        _orderService_WithLimit = _serviceProvider_WithLimit.GetRequiredService<IOrderService>();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider_NoLimit?.Dispose();
        _serviceProvider_WithLimit?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public async Task CreateMultipleOrders_NoLimit()
    {
        var tasks = Enumerable.Range(0, ConcurrentRequests)
            .Select(_ => _orderService_NoLimit.CreateOrderDirectAsync(1, 1, 1, "4111111111111111"));
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task CreateMultipleOrders_WithLimit()
    {
        var tasks = Enumerable.Range(0, ConcurrentRequests)
            .Select(_ => _orderService_WithLimit.CreateOrderDirectAsync(1, 1, 1, "4111111111111111"));
        await Task.WhenAll(tasks);
    }
}

