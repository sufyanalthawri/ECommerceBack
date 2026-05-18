using ECommerceBack.Core.DTOs;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerceBack.Infrastructure.Decorators
{
    /// <summary>
    /// Decorator لإضافة آلية إعادة المحاولة التلقائية (Retry) حول دوال IOrderService التي تكتب في قاعدة البيانات.
    /// يطبق نمط AOP (البرمجة الموجهة نحو الجوانب) لفصل منطق إعادة المحاولة عن خدمة الطلب الأساسية.
    /// يقوم بإعادة محاولة العمليات الفاشلة بسبب DbUpdateConcurrencyException حتى 3 مرات مع تأخير تصاعدي.
    /// عمليات القراءة (GetUserOrdersAsync, GetOrderDetailsAsync) لا تحتاج إلى إعادة محاولة.
    /// </summary>
    public class RetryDecorator : IOrderService
    {
        private readonly IOrderService _inner;
        private readonly ILogger<RetryDecorator> _logger;
        private const int MaxRetries = 10;

        public RetryDecorator(IOrderService inner, ILogger<RetryDecorator> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        /// <summary>
        /// إنشاء طلب مباشر مع إعادة محاولة تصل إلى  مرات عند حدوث DbUpdateConcurrencyException.
        /// </summary>
        /// <param name="userId">معرف المستخدم.</param>
        /// <param name="productId">معرف المنتج.</param>
        /// <param name="quantity">الكمية المطلوبة.</param>
        /// <param name="cardNumber">رقم البطاقة (محاكاة).</param>
        /// <returns>كائن Order الذي تم إنشاؤه.</returns>
        /// <exception cref="InvalidOperationException">عند تجاوز الحد الأقصى للمحاولات.</exception>
        public async Task<Order> CreateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber)
        {
            int attempt = 0;
            while (attempt < MaxRetries)
            {
                try
                {
                    return await _inner.CreateOrderDirectAsync(userId, productId, quantity, cardNumber);
                }
                catch (DbUpdateConcurrencyException ex) when (attempt < MaxRetries - 1)
                {
                    attempt++;
                    _logger.LogWarning(ex, "⚠️ Retry {Attempt}/{MaxRetries} for CreateOrderDirectAsync (Product {ProductId})", attempt, MaxRetries, productId);
                    await Task.Delay(50 * attempt);
                }
            }
            throw new InvalidOperationException("Max retries exceeded for CreateOrderDirectAsync");
        }

        /// <summary>
        /// إنشاء طلب من السلة مع إعادة محاولة تصل إلى 3 مرات عند حدوث DbUpdateConcurrencyException.
        /// </summary>
        /// <param name="userId">معرف المستخدم.</param>
        /// <param name="paymentInfo">معلومات الدفع (رقم البطاقة).</param>
        /// <returns>كائن Order الذي تم إنشاؤه.</returns>
        /// <exception cref="InvalidOperationException">عند تجاوز الحد الأقصى للمحاولات.</exception>

        /// <summary>عمليات القراءة لا تحتاج إلى إعادة محاولة.</summary>
        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _inner.GetUserOrdersAsync(userId);
        }

        /// <summary>عمليات القراءة لا تحتاج إلى إعادة محاولة.</summary>
        public async Task<Order?> GetOrderDetailsAsync(int orderId, int userId)
        {
            return await _inner.GetOrderDetailsAsync(orderId, userId);
        }
    }
}