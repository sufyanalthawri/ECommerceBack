using ECommerceBack.Core.DTOs;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceBack.Infrastructure.Decorators
{
    /// <summary>
    /// Decorator لإضافة Logging تلقائي حول جميع دوال IOrderService.
    /// يطبق نمط AOP (البرمجة الموجهة نحو الجوانب) لفصل التسجيل عن منطق الأعمال الأساسي.
    /// يسجل دخول وخروج كل دالة مع المعاملات والنتائج (مثل عدد السجلات المُعادة أو وجود الكائن).
    /// </summary>
    public class OrderServiceLoggingDecorator : IOrderService
    {
        private readonly IOrderService _inner;
        private readonly ILogger<OrderServiceLoggingDecorator> _logger;

        public OrderServiceLoggingDecorator(IOrderService inner, ILogger<OrderServiceLoggingDecorator> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        /// <summary>إنشاء طلب مباشر (بدون سلة) مع تسجيل الدخول/الخروج ونتيجة الطلب.</summary>
        public async Task<Order> CreateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber)
        {
            _logger.LogInformation(" OrderService.CreateOrderDirectAsync called (User:{UserId}, Product:{ProductId})", userId, productId);
            var result = await _inner.CreateOrderDirectAsync(userId, productId, quantity, cardNumber);
            _logger.LogInformation(" OrderService.CreateOrderDirectAsync completed (Order:{OrderId})", result.Id);
            return result;
        }

        /// <summary>إنشاء طلب من السلة مع تسجيل المعاملات.</summary>
        public async Task<Order> CreateOrderFromCartAsync(int userId, PaymentInfo paymentInfo)
        {
            _logger.LogInformation("OrderService.CreateOrderFromCartAsync called (User:{UserId})", userId);
            var result = await _inner.CreateOrderFromCartAsync(userId, paymentInfo);
            _logger.LogInformation("OrderService.CreateOrderFromCartAsync completed (Order:{OrderId})", result.Id);
            return result;
        }

        /// <summary>جلب قائمة طلبات المستخدم مع تسجيل عدد النتائج.</summary>
        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            _logger.LogInformation("▶ OrderService.GetUserOrdersAsync (User:{UserId})", userId);
            var result = await _inner.GetUserOrdersAsync(userId);
            _logger.LogInformation(" OrderService.GetUserOrdersAsync returned {Count} orders", result.Count());
            return result;
        }

        /// <summary>جلب تفاصيل طلب محدد مع تسجيل وجود النتيجة أم لا.</summary>
        public async Task<Order?> GetOrderDetailsAsync(int orderId, int userId)
        {
            _logger.LogInformation(" OrderService.GetOrderDetailsAsync (Order:{OrderId}, User:{UserId})", orderId, userId);
            var result = await _inner.GetOrderDetailsAsync(orderId, userId);
            _logger.LogInformation(" OrderService.GetOrderDetailsAsync found: {Found}", result != null);
            return result;
        }
    }
}