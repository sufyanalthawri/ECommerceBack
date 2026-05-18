using ECommerceBack.Core.DTOs;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceBack.Infrastructure.Services
{
    public class OrderService_WithLimit : IOrderService
    {
        private readonly IConcurrencyManager _concurrencyManager;
        private readonly OrderService_NoLimit _inner;

        // نحتاج IServiceScopeFactory لتمريره إلى OrderService_NoLimit الداخلية
        public OrderService_WithLimit(IConcurrencyManager concurrencyManager, IServiceScopeFactory scopeFactory)
        {
            _concurrencyManager = concurrencyManager;
            _inner = new OrderService_NoLimit(scopeFactory);
        }

        public async Task<Order> CreateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber)
        {
            return await _concurrencyManager.ExecuteWithLimitAsync(
                $"checkout_{productId}",
                () => _inner.CreateOrderDirectAsync(userId, productId, quantity, cardNumber),
                maxConcurrency: 10);
        }

        public Task<IEnumerable<Order>> GetUserOrdersAsync(int userId) => throw new NotImplementedException();
        public Task<Order?> GetOrderDetailsAsync(int orderId, int userId) => throw new NotImplementedException();
        //public Task<Order> CreateOrderFromCartAsync(int userId, PaymentInfo paymentInfo) => throw new NotImplementedException();
    }
}