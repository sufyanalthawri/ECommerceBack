using Hangfire;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using Microsoft.Extensions.Logging;
using ECommerceBack.ECommerceBack.Core.Interfaces;

namespace ECommerceBack.Infrastructure.Services
{
    /// <summary>
    /// تطبيق نمط Façade (واجهة مبسطة) لعملية الشراء الكاملة.
    /// يجمع استدعاءات السلة، إنشاء الطلب، جدولة المهام الخلفية، وتفريغ السلة في خطوة واحدة.
    /// </summary>
    /// 

    public class CheckoutFacade : ICheckoutFacade
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly ILogger<CheckoutFacade> _logger;

        public CheckoutFacade(ICartService cartService, IOrderService orderService,
            IBackgroundJobService backgroundJobService, ILogger<CheckoutFacade> logger)
        {
            _cartService = cartService;
            _orderService = orderService;
            _backgroundJobService = backgroundJobService;
            _logger = logger;
        }

        /// <summary>
        /// تنفيذ عملية شراء كاملة:
        /// - إضافة المنتج إلى السلة
        /// - إنشاء الطلب (تحديث المخزون، الدفع، حفظ الطلب)
        /// - جدولة المهام الخلفية (بريد إلكتروني، فاتورة، تحديث إحصائيات)
        /// - تفريغ السلة
        /// </summary>
        /// <param name="userId">معرف المستخدم.</param>
        /// <param name="productId">معرف المنتج.</param>
        /// <param name="quantity">الكمية المطلوبة.</param>
        /// <param name="cardNumber">رقم البطاقة (محاكاة).</param>
        /// <returns>كائن Order الذي تم إنشاؤه.</returns>
        public async Task<Order> PlaceOrderAsync(int userId, int productId, int quantity, string cardNumber)
        {
            await _cartService.AddToCartAsync(userId, productId, quantity);
            var order = await _orderService.CreateOrderDirectAsync(userId, productId, quantity, cardNumber);
            BackgroundJob.Enqueue<IBackgroundJobService>(x => x.SendOrderConfirmationEmailAsync(order.Id));
            BackgroundJob.Enqueue<IBackgroundJobService>(x => x.GenerateInvoicePdfAsync(order.Id));
            BackgroundJob.Schedule<IBackgroundJobService>(x => x.UpdateSalesStatisticsAsync(order.Id), TimeSpan.FromMinutes(5));
            await _cartService.ClearCartAsync(userId);
            _logger.LogInformation($"CheckoutFacade: Order {order.Id} completed");
            return order;
        }
    }
}