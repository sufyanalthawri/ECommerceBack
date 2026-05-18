using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ECommerceBack.Core.DTOs;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceBack.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;
        private readonly IConcurrencyManager _concurrencyManager;



        public OrderService(
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IProductRepository productRepository,
            IPaymentGateway paymentGateway,
            IPaymentTransactionRepository paymentTransactionRepository,
            IBackgroundJobService backgroundJobService,
            AppDbContext context,

            IConcurrencyManager concurrencyManager,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _paymentGateway = paymentGateway;
            _paymentTransactionRepository = paymentTransactionRepository;
            _backgroundJobService = backgroundJobService;
            _context = context;
            _paymentGateway = paymentGateway;

            _logger = logger;
            _concurrencyManager = concurrencyManager;


        }
        public async Task<Order> CreateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber)
        {
            // استخدام Concurrency Manager للحد من التزامن على نفس المنتج
            return await _concurrencyManager.ExecuteWithLimitAsync(
                resourceKey: $"product_{productId}",
                action: () => CreateOrderDirectCoreAsync(userId, productId, quantity, cardNumber),
                maxConcurrency: 10  // يمكن تعديلها حسب المتطلبات
            );
        }
        private async Task<Order> CreateOrderDirectCoreAsync(int userId, int productId, int quantity, string cardNumber)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new InvalidOperationException("Product not found.");
            if (product.Stock < quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

            // تحديث المخزون
            product.Stock -= quantity;
            product.LastUpdated = DateTime.UtcNow;

            try
            {
                // قد ترمي DbUpdateConcurrencyException إذا تغير المنتج بواسطة معاملة أخرى
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // إعادة تحميل الكيان بأحدث البيانات من قاعدة البيانات
                await _context.Entry(product).ReloadAsync();
                _context.Entry(product).State = EntityState.Detached;

                // إعادة رمي الاستثناء لكي يقوم الـ Decorator بإعادة المحاولة
                throw;
            }

            // معالجة الدفع (لا نعيد المحاولة هنا؛ أي فشل في الدفع يعتبر خطأ نهائياً)
            var paymentResult = await _paymentGateway.ProcessPaymentAsync(product.Price * quantity, cardNumber);
            if (!paymentResult.Success)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Payment failed.");
            }

            // إنشاء الطلب
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = product.Price * quantity,
                Status = OrderStatus.Paid,
                OrderItems = new List<OrderItem>
        {
            new OrderItem { ProductId = productId, Quantity = quantity, UnitPrice = product.Price }
        }
            };
            await _context.Orders.AddAsync(order);

            // تسجيل معاملة الدفع
            var paymentTransaction = new PaymentTransaction
            {
                Order = order,
                Amount = product.Price * quantity,
                GatewayReference = paymentResult.TransactionId,
                Status = PaymentStatus.Success,
                PaymentDate = DateTime.UtcNow
            };
            await _context.PaymentTransactions.AddAsync(paymentTransaction);

            // حفظ باقي الكيانات (Order, PaymentTransaction)
            await _context.SaveChangesAsync();

            // إتمام المعاملة
            await transaction.CommitAsync();

            return order;
        }
        //public async Task<Order> CrateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber)
        //{
        //    if (quantity <= 0)
        //        throw new InvalidOperationException("Quantity must be positive.");

        //    const int maxRetries = 9;  
        //    int attempt = 0;

        //    while (attempt < maxRetries)
        //    {
        //        await using var transaction = await _context.Database.BeginTransactionAsync();
        //        try
        //        {
        //            // جلب المنتج (سيتم تتبعه بواسطة ChangeTracker)
        //            var product = await _context.Products.FindAsync(productId);
        //            if (product == null)
        //                throw new InvalidOperationException("Product not found.");

        //            _logger.LogInformation($"Attempt {attempt + 1}: Stock={product.Stock}, RowVersion={Convert.ToBase64String(product.RowVersion ?? Array.Empty<byte>())}");

        //            if (product.Stock < quantity)
        //                throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

        //            // تحديث المخزون
        //            product.Stock -= quantity;
        //            product.LastUpdated = DateTime.UtcNow;

        //            // حفظ التغييرات – قد يرمي DbUpdateConcurrencyException
        //            await _context.SaveChangesAsync();

        //            // معالجة الدفع
        //            var paymentResult = await _paymentGateway.ProcessPaymentAsync(product.Price * quantity, cardNumber);
        //            if (!paymentResult.Success)
        //                throw new InvalidOperationException("Payment failed.");

        //            // إنشاء الطلب
        //            var order = new Order
        //            {
        //                UserId = userId,
        //                OrderDate = DateTime.UtcNow,
        //                TotalAmount = product.Price * quantity,
        //                Status = OrderStatus.Paid,
        //                OrderItems = new List<OrderItem>
        //        {
        //            new OrderItem { ProductId = productId, Quantity = quantity, UnitPrice = product.Price }
        //        }
        //            };

        //            await _context.Orders.AddAsync(order);

        //            // تسجيل معاملة الدفع
        //            var paymentTransaction = new PaymentTransaction
        //            {
        //                Order = order,
        //                Amount = product.Price * quantity,
        //                GatewayReference = paymentResult.TransactionId,
        //                Status = PaymentStatus.Success,
        //                PaymentDate = DateTime.UtcNow
        //            };

        //            await _context.PaymentTransactions.AddAsync(paymentTransaction);
        //            await _context.SaveChangesAsync();

        //            await transaction.CommitAsync();

        //            _logger.LogInformation($"Order {order.Id} created successfully. Stock left: {product.Stock}");
        //            return order;
        //        }
        //        catch (DbUpdateConcurrencyException ex)
        //        {
        //            await transaction.RollbackAsync();
        //            attempt++;
        //            _logger.LogWarning(ex, $"Concurrency conflict on attempt {attempt} for product {productId}");

        //            if (attempt >= maxRetries)
        //                throw new InvalidOperationException("Concurrency conflict. Please retry.");
        //            var staleProduct = await _context.Products.FindAsync(productId);
        //            if (staleProduct != null)
        //            {
        //                _context.Entry(staleProduct).State = EntityState.Detached;
        //            }
        //            await Task.Delay(10 * attempt);
        //        }
        //        catch (InvalidOperationException) 
        //        {
        //            await transaction.RollbackAsync();
        //            throw;
        //        }
        //        catch
        //        {
        //            await transaction.RollbackAsync();
        //            throw;
        //        }
        //    }

        //    throw new InvalidOperationException("Concurrency conflict. Please retry.");
        //}        public async Task<Order> CreateOrderFromCartAsync(int userId, PaymentInfo paymentInfo)
   
        //{
        //    var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
        //    if (cart == null || !cart.CartItems.Any())
        //        throw new InvalidOperationException("Cart is empty");

        //    await using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        // حساب الإجمالي والتحقق من المخزون
        //        decimal totalAmount = 0;
        //        foreach (var item in cart.CartItems)
        //        {
        //            var product = await _productRepository.GetByIdAsync(item.ProductId);
        //            if (product == null || product.Stock < item.Quantity)
        //                throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");
        //            totalAmount += product.Price * item.Quantity;
        //        }

        //        // معالجة الدفع
        //        var paymentResult = await _paymentGateway.ProcessPaymentAsync(totalAmount, paymentInfo.CardNumber);
        //        if (!paymentResult.Success)
        //            throw new InvalidOperationException("Payment failed.");

        //        // إنشاء الطلب
        //        var order = new Order
        //        {
        //            UserId = userId,
        //            OrderDate = DateTime.UtcNow,
        //            TotalAmount = totalAmount,
        //            Status = OrderStatus.Paid,
        //            OrderItems = cart.CartItems.Select(ci => new OrderItem
        //            {
        //                ProductId = ci.ProductId,
        //                Quantity = ci.Quantity,
        //                UnitPrice = ci.Product.Price
        //            }).ToList()
        //        };

        //        await _context.Orders.AddAsync(order);

        //        // تحديث المخزون
        //        foreach (var item in cart.CartItems)
        //        {
        //            var product = await _productRepository.GetByIdAsync(item.ProductId);
        //            product.Stock -= item.Quantity;
        //            product.LastUpdated = DateTime.UtcNow;
        //            await _productRepository.UpdateAsync(product);
        //        }

        //        // تسجيل معاملة الدفع
        //        var paymentTransaction = new PaymentTransaction
        //        {
        //            Order = order,
        //            Amount = totalAmount,
        //            GatewayReference = paymentResult.TransactionId,
        //            Status = PaymentStatus.Success,
        //            PaymentDate = DateTime.UtcNow
        //        };
        //        await _context.PaymentTransactions.AddAsync(paymentTransaction);

        //        // تفريغ السلة
        //        cart.CartItems.Clear();
        //        await _cartRepository.UpdateAsync(cart);

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();

        //        // مهام Hangfire الخلفية
        //        BackgroundJob.Enqueue<IBackgroundJobService>(x => x.SendOrderConfirmationEmailAsync(order.Id));
        //        BackgroundJob.Enqueue<IBackgroundJobService>(x => x.GenerateInvoicePdfAsync(order.Id));

        //        return order;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _orderRepository.GetOrdersByUserIdAsync(userId);
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId, int userId)
        {
            var order = await _orderRepository.GetOrderWithItemsAndPaymentAsync(orderId);
            if (order == null || order.UserId != userId)
                return null;
            return order;
        }
    }
}













