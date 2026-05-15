using ECommerceBack.Core.DTOs;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceBack.Infrastructure.Services
{
    public class OrderService_NoLimit : IOrderService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // لم نعد نحتاج إلى IPaymentGateway – سنحاكي الدفع يدوياً
        public OrderService_NoLimit(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<Order> CreateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber)
        {
            return await CreateOrderCoreAsync(userId, productId, quantity);
        }

        private async Task<Order> CreateOrderCoreAsync(int userId, int productId, int quantity)
        {
            const int maxRetries = 30;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                // ننشئ نطاقاً جديداً لكل محاولة للحصول على DbContext مستقل
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    var product = await context.Products.FindAsync(productId);
                    if (product == null) throw new InvalidOperationException("Product not found");
                    if (product.Stock < quantity) throw new InvalidOperationException("Insufficient stock");

                    product.Stock -= quantity;
                    await context.SaveChangesAsync();

                    // محاكاة ناجحة للدفع (بدون استدعاء IPaymentGateway)
                    // يمكن إضافة تأخير بسيط لمحاكاة عمل حقيقي إن أردت
                    // await Task.Delay(50); 

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
                    await context.Orders.AddAsync(order);

                    var paymentTransaction = new PaymentTransaction
                    {
                        Order = order,
                        Amount = product.Price * quantity,
                        GatewayReference = Guid.NewGuid().ToString(),
                        Status = PaymentStatus.Success,
                        PaymentDate = DateTime.UtcNow
                    };
                    await context.PaymentTransactions.AddAsync(paymentTransaction);

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return order;
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    attempt++;
                    if (attempt >= maxRetries) throw new Exception("Concurrency conflict");
                    await Task.Delay(50 * attempt);
                }
            }

            throw new Exception("Concurrency conflict");
        }

        // التنفيذات الأخرى غير المستخدمة في البنشمارك نتركها رمي استثناء
        public Task<IEnumerable<Order>> GetUserOrdersAsync(int userId) => throw new NotImplementedException();
        public Task<Order?> GetOrderDetailsAsync(int orderId, int userId) => throw new NotImplementedException();
        public Task<Order> CreateOrderFromCartAsync(int userId, PaymentInfo paymentInfo) => throw new NotImplementedException();
    }
}