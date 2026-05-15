using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ECommerceBack.Core.DTOs;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;

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
            _logger = logger;
            _concurrencyManager = concurrencyManager;


        }
        // نسخة غير آمنة – بدون RowVersion ولا إعادة محاولة
        //public async Task<Order> CreateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber)
        //{
        //    if (quantity <= 0) throw new InvalidOperationException("Quantity must be positive.");

        //    await using var transaction = await _context.Database.BeginTransactionAsync();

        //    var product = await _context.Products.FindAsync(productId);
        //    if (product == null) throw new InvalidOperationException("Product not found.");

        //    if (product.Stock < quantity) throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

        //    product.Stock -= quantity;
        //    await _context.SaveChangesAsync();

        //    // محاكاة الدفع (ناجح دائماً للتبسيط)
        //    var order = new Order { UserId = userId, OrderDate = DateTime.UtcNow, TotalAmount = product.Price * quantity, Status = OrderStatus.Paid };
        //    order.OrderItems.Add(new OrderItem { ProductId = productId, Quantity = quantity, UnitPrice = product.Price });
        //    await _context.Orders.AddAsync(order);
        //    await _context.SaveChangesAsync();

        //    await transaction.CommitAsync();
        //    return order;
        //}


        public async Task<Order> vCreateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be positive.");

            const int maxRetries = 3;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var product = await _context.Products.FindAsync(productId);
                    if (product == null) throw new InvalidOperationException("Product not found.");

                    if (product.Stock < quantity)
                        throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

                    product.Stock -= quantity;
                    product.LastUpdated = DateTime.UtcNow;

                    await _context.SaveChangesAsync();  // قد يرمي DbUpdateConcurrencyException

                    var paymentResult = await _paymentGateway.ProcessPaymentAsync(product.Price * quantity, cardNumber);
                    if (!paymentResult.Success) throw new InvalidOperationException("Payment failed.");

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

                    var paymentTransaction = new PaymentTransaction
                    {
                        Order = order,
                        Amount = product.Price * quantity,
                        GatewayReference = paymentResult.TransactionId,
                        Status = PaymentStatus.Success,
                        PaymentDate = DateTime.UtcNow
                    };

                    await _context.PaymentTransactions.AddAsync(paymentTransaction);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // (بدون Hangfire في هذه النسخة التجريبية)
                    return order;
                }
                catch (DbUpdateConcurrencyException) when (attempt < maxRetries - 1)
                {
                    await transaction.RollbackAsync();
                    attempt++;
                    await Task.Delay(50 * attempt);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            throw new InvalidOperationException("Concurrency conflict. Please retry.");
        }
        /// <summary>
        /// NF1 + NF2: Optimistic Concurrency + Limit concurrent operations per product (e.g., max 10)
        /// </summary>
        public async Task<Order> nCreateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber)
        {
            return await _concurrencyManager.ExecuteWithLimitAsync(
                $"checkout_{productId}",
                async () =>
                {
                    if (quantity <= 0)
                        throw new InvalidOperationException("Quantity must be positive.");

                    const int maxRetries = 3;
                    int attempt = 0;

                    while (attempt < maxRetries)
                    {
                        await using var transaction = await _context.Database.BeginTransactionAsync();

                        try
                        {
                            var product = await _context.Products.FindAsync(productId);
                            if (product == null) throw new InvalidOperationException("Product not found.");

                            if (product.Stock < quantity)
                                throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

                            product.Stock -= quantity;
                            product.LastUpdated = DateTime.UtcNow;

                            await _context.SaveChangesAsync();  // قد يرمي DbUpdateConcurrencyException

                            var paymentResult = await _paymentGateway.ProcessPaymentAsync(product.Price * quantity, cardNumber);
                            if (!paymentResult.Success) throw new InvalidOperationException("Payment failed.");

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

                            var paymentTransaction = new PaymentTransaction
                            {
                                Order = order,
                                Amount = product.Price * quantity,
                                GatewayReference = paymentResult.TransactionId,
                                Status = PaymentStatus.Success,
                                PaymentDate = DateTime.UtcNow
                            };

                            await _context.PaymentTransactions.AddAsync(paymentTransaction);
                            await _context.SaveChangesAsync();

                            await transaction.CommitAsync();

                            return order;
                        }
                        catch (DbUpdateConcurrencyException) when (attempt < maxRetries - 1)
                        {
                            await transaction.RollbackAsync();
                            attempt++;
                            await Task.Delay(50 * attempt);
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }

                    throw new InvalidOperationException("Concurrency conflict. Please retry.");
                },
                maxConcurrency: 10);   // الحد الأقصى للطلبات المتزامنة على نفس المنتج
        }


        public async Task<Order> CreateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be positive.");

            const int maxRetries = 30;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // جلب المنتج (في كل محاولة للحصول على أحدث RowVersion)
                    var product = await _context.Products.FindAsync(productId);
                    if (product == null)
                        throw new InvalidOperationException("Product not found.");

                    _logger.LogInformation($"Attempt {attempt + 1}: Stock={product.Stock}, RowVersion={Convert.ToBase64String(product.RowVersion ?? Array.Empty<byte>())}");

                    if (product.Stock < quantity)
                        throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

                    // تحديث المخزون
                    product.Stock -= quantity;
                    product.LastUpdated = DateTime.UtcNow;

                    // حفظ التغييرات – قد يرمي DbUpdateConcurrencyException
                    await _context.SaveChangesAsync();

                    // معالجة الدفع
                    var paymentResult = await _paymentGateway.ProcessPaymentAsync(product.Price * quantity, cardNumber);
                    if (!paymentResult.Success)
                        throw new InvalidOperationException("Payment failed.");

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
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    // ===== المهام غير المتزامنة (Hangfire) =====
                    BackgroundJob.Enqueue<IBackgroundJobService>(x => x.SendOrderConfirmationEmailAsync(order.Id));
                    BackgroundJob.Enqueue<IBackgroundJobService>(x => x.GenerateInvoicePdfAsync(order.Id));
                    BackgroundJob.Schedule<IBackgroundJobService>(x => x.UpdateSalesStatisticsAsync(order.Id), TimeSpan.FromMinutes(5));

                    _logger.LogInformation($"Order {order.Id} created successfully. Stock left: {product.Stock}");
                    return order;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    attempt++;
                    _logger.LogWarning(ex, $"Concurrency conflict on attempt {attempt} for product {productId}");
                    if (attempt >= maxRetries)
                        throw new InvalidOperationException("Concurrency conflict. Please retry.");
                    await Task.Delay(50 * attempt);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            throw new InvalidOperationException("Concurrency conflict. Please retry.");
        }

        // إنشاء طلب من السلة (الطريقة القديمة - يمكن الاحتفاظ بها)
        public async Task<Order> CreateOrderFromCartAsync(int userId, PaymentInfo paymentInfo)
        {
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
            if (cart == null || !cart.CartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // حساب الإجمالي والتحقق من المخزون
                decimal totalAmount = 0;
                foreach (var item in cart.CartItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null || product.Stock < item.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");
                    totalAmount += product.Price * item.Quantity;
                }

                // معالجة الدفع
                var paymentResult = await _paymentGateway.ProcessPaymentAsync(totalAmount, paymentInfo.CardNumber);
                if (!paymentResult.Success)
                    throw new InvalidOperationException("Payment failed.");

                // إنشاء الطلب
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Paid,
                    OrderItems = cart.CartItems.Select(ci => new OrderItem
                    {
                        ProductId = ci.ProductId,
                        Quantity = ci.Quantity,
                        UnitPrice = ci.Product.Price
                    }).ToList()
                };

                await _context.Orders.AddAsync(order);

                // تحديث المخزون
                foreach (var item in cart.CartItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    product.Stock -= item.Quantity;
                    product.LastUpdated = DateTime.UtcNow;
                    await _productRepository.UpdateAsync(product);
                }

                // تسجيل معاملة الدفع
                var paymentTransaction = new PaymentTransaction
                {
                    Order = order,
                    Amount = totalAmount,
                    GatewayReference = paymentResult.TransactionId,
                    Status = PaymentStatus.Success,
                    PaymentDate = DateTime.UtcNow
                };
                await _context.PaymentTransactions.AddAsync(paymentTransaction);

                // تفريغ السلة
                cart.CartItems.Clear();
                await _cartRepository.UpdateAsync(cart);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // مهام Hangfire الخلفية
                BackgroundJob.Enqueue<IBackgroundJobService>(x => x.SendOrderConfirmationEmailAsync(order.Id));
                BackgroundJob.Enqueue<IBackgroundJobService>(x => x.GenerateInvoicePdfAsync(order.Id));

                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

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














//namespace ECommerceBack.Infrastructure.Services
//{
//    public class OrderService : IOrderService
//    {
//        private readonly IOrderRepository _orderRepository;
//        private readonly ICartRepository _cartRepository;
//        private readonly IProductRepository _productRepository;
//        private readonly IPaymentGateway _paymentGateway;
//        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
//        private readonly IBackgroundJobService _backgroundJobService;
//        private readonly IConcurrencyManager _concurrencyManager;
//        private readonly AppDbContext _context;
//        private readonly ILogger<OrderService> _logger;

//        public OrderService(
//            IOrderRepository orderRepository,
//            ICartRepository cartRepository,
//            IProductRepository productRepository,
//            IPaymentGateway paymentGateway,
//            IPaymentTransactionRepository paymentTransactionRepository,
//            IBackgroundJobService backgroundJobService,
//            IConcurrencyManager concurrencyManager,
//            AppDbContext context,
//            ILogger<OrderService> logger)
//        {
//            _orderRepository = orderRepository;
//            _cartRepository = cartRepository;
//            _productRepository = productRepository;
//            _paymentGateway = paymentGateway;
//            _paymentTransactionRepository = paymentTransactionRepository;
//            _backgroundJobService = backgroundJobService;
//            _concurrencyManager = concurrencyManager;
//            _context = context;
//            _logger = logger;
//        }

//        // إنشاء طلب مباشر (بدون سلة) مع التزامن المتفائل + التحكم في السعة
//        public async Task<Order> CreateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber)
//        {
//            // نحدد مفتاحاً فريداً للمورد (المنتج) باستخدام `checkout_{productId}`
//            return await _concurrencyManager.ExecuteWithLimitAsync(
//                $"checkout_{productId}",
//                async () =>
//                {
//                    if (quantity <= 0)
//                        throw new InvalidOperationException("Quantity must be positive.");

//                    const int maxRetries = 3;
//                    int attempt = 0;

//                    while (attempt < maxRetries)
//                    {
//                        await using var transaction = await _context.Database.BeginTransactionAsync();

//                        try
//                        {
//                            // جلب المنتج (في كل محاولة للحصول على أحدث RowVersion)
//                            var product = await _context.Products.FindAsync(productId);
//                            if (product == null)
//                                throw new InvalidOperationException("Product not found.");

//                            _logger.LogInformation($"Attempt {attempt + 1}: Stock={product.Stock}, RowVersion={Convert.ToBase64String(product.RowVersion ?? Array.Empty<byte>())}");

//                            if (product.Stock < quantity)
//                                throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

//                            // تحديث المخزون
//                            product.Stock -= quantity;
//                            product.LastUpdated = DateTime.UtcNow;

//                            // حفظ التغييرات – قد يرمي DbUpdateConcurrencyException
//                            await _context.SaveChangesAsync();

//                            // معالجة الدفع
//                            var paymentResult = await _paymentGateway.ProcessPaymentAsync(product.Price * quantity, cardNumber);
//                            if (!paymentResult.Success)
//                                throw new InvalidOperationException("Payment failed.");

//                            // إنشاء الطلب
//                            var order = new Order
//                            {
//                                UserId = userId,
//                                OrderDate = DateTime.UtcNow,
//                                TotalAmount = product.Price * quantity,
//                                Status = OrderStatus.Paid,
//                                OrderItems = new List<OrderItem>
//                                {
//                                    new OrderItem { ProductId = productId, Quantity = quantity, UnitPrice = product.Price }
//                                }
//                            };

//                            await _context.Orders.AddAsync(order);

//                            // تسجيل معاملة الدفع
//                            var paymentTransaction = new PaymentTransaction
//                            {
//                                Order = order,
//                                Amount = product.Price * quantity,
//                                GatewayReference = paymentResult.TransactionId,
//                                Status = PaymentStatus.Success,
//                                PaymentDate = DateTime.UtcNow
//                            };

//                            await _context.PaymentTransactions.AddAsync(paymentTransaction);
//                            await _context.SaveChangesAsync();

//                            await transaction.CommitAsync();

//                            // المهام غير المتزامنة (Hangfire)
//                            BackgroundJob.Enqueue<IBackgroundJobService>(x => x.SendOrderConfirmationEmailAsync(order.Id));
//                            BackgroundJob.Enqueue<IBackgroundJobService>(x => x.GenerateInvoicePdfAsync(order.Id));
//                            BackgroundJob.Schedule<IBackgroundJobService>(x => x.UpdateSalesStatisticsAsync(order.Id), TimeSpan.FromMinutes(5));

//                            _logger.LogInformation($"Order {order.Id} created successfully. Stock left: {product.Stock}");
//                            return order;
//                        }
//                        catch (DbUpdateConcurrencyException ex)
//                        {
//                            await transaction.RollbackAsync();
//                            attempt++;
//                            _logger.LogWarning(ex, $"Concurrency conflict on attempt {attempt} for product {productId}");
//                            if (attempt >= maxRetries)
//                                throw new InvalidOperationException("Concurrency conflict. Please retry.");
//                            await Task.Delay(50 * attempt);
//                        }
//                        catch
//                        {
//                            await transaction.RollbackAsync();
//                            throw;
//                        }
//                    }

//                    throw new InvalidOperationException("Concurrency conflict. Please retry.");
//                },
//                maxConcurrency: 10   // الحد الأقصى للطلبات المتزامنة لنفس المنتج
//            );
//        }

//        // إنشاء طلب من السلة (الطريقة القديمة - يمكن الاحتفاظ بها)
//        public async Task<Order> CreateOrderFromCartAsync(int userId, PaymentInfo paymentInfo)
//        {
//            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
//            if (cart == null || !cart.CartItems.Any())
//                throw new InvalidOperationException("Cart is empty");

//            await using var transaction = await _context.Database.BeginTransactionAsync();

//            try
//            {
//                // حساب الإجمالي والتحقق من المخزون
//                decimal totalAmount = 0;
//                foreach (var item in cart.CartItems)
//                {
//                    var product = await _productRepository.GetByIdAsync(item.ProductId);
//                    if (product == null || product.Stock < item.Quantity)
//                        throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");
//                    totalAmount += product.Price * item.Quantity;
//                }

//                // معالجة الدفع
//                var paymentResult = await _paymentGateway.ProcessPaymentAsync(totalAmount, paymentInfo.CardNumber);
//                if (!paymentResult.Success)
//                    throw new InvalidOperationException("Payment failed.");

//                // إنشاء الطلب
//                var order = new Order
//                {
//                    UserId = userId,
//                    OrderDate = DateTime.UtcNow,
//                    TotalAmount = totalAmount,
//                    Status = OrderStatus.Paid,
//                    OrderItems = cart.CartItems.Select(ci => new OrderItem
//                    {
//                        ProductId = ci.ProductId,
//                        Quantity = ci.Quantity,
//                        UnitPrice = ci.Product.Price
//                    }).ToList()
//                };

//                await _context.Orders.AddAsync(order);

//                // تحديث المخزون
//                foreach (var item in cart.CartItems)
//                {
//                    var product = await _productRepository.GetByIdAsync(item.ProductId);
//                    product.Stock -= item.Quantity;
//                    product.LastUpdated = DateTime.UtcNow;
//                    await _productRepository.UpdateAsync(product);
//                }

//                // تسجيل معاملة الدفع
//                var paymentTransaction = new PaymentTransaction
//                {
//                    Order = order,
//                    Amount = totalAmount,
//                    GatewayReference = paymentResult.TransactionId,
//                    Status = PaymentStatus.Success,
//                    PaymentDate = DateTime.UtcNow
//                };
//                await _context.PaymentTransactions.AddAsync(paymentTransaction);

//                // تفريغ السلة
//                cart.CartItems.Clear();
//                await _cartRepository.UpdateAsync(cart);

//                await _context.SaveChangesAsync();
//                await transaction.CommitAsync();

//                // مهام Hangfire الخلفية
//                BackgroundJob.Enqueue<IBackgroundJobService>(x => x.SendOrderConfirmationEmailAsync(order.Id));
//                BackgroundJob.Enqueue<IBackgroundJobService>(x => x.GenerateInvoicePdfAsync(order.Id));

//                return order;
//            }
//            catch
//            {
//                await transaction.RollbackAsync();
//                throw;
//            }
//        }

//        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
//        {
//            return await _orderRepository.GetOrdersByUserIdAsync(userId);
//        }

//        public async Task<Order?> GetOrderDetailsAsync(int orderId, int userId)
//        {
//            var order = await _orderRepository.GetOrderWithItemsAndPaymentAsync(orderId);
//            if (order == null || order.UserId != userId)
//                return null;
//            return order;
//        }
//    }
//}




