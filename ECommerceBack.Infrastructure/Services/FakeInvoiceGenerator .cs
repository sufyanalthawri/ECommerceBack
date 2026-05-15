using Microsoft.Extensions.Logging;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;

namespace ECommerceBack.Infrastructure.Services
{
    /// <summary>
    /// تطبيق وهمي (Fake) لواجهة IInvoiceGenerator.
    /// يحاكي إنشاء فاتورة   بتأخير عشوائي (100–300 مللي ثانية) وتسجيل في السجلات.
    /// يستخدم للاختبار والتطوير دون الحاجة إلى مكتبة  حقيقية.
    /// </summary>
    public class FakeInvoiceGenerator : IInvoiceGenerator
    {
        private readonly ILogger<FakeInvoiceGenerator> _logger;

        public FakeInvoiceGenerator(ILogger<FakeInvoiceGenerator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// إنشاء فاتورة وهمية للطلب المحدد.
        /// </summary>
        /// <param name="order">الطلب الذي سيتم إنشاء الفاتورة له.</param>
        public async Task GenerateAsync(Order order)
        {
            await Task.Delay(Random.Shared.Next(100, 300));
            _logger.LogInformation($"[FAKE INVOICE] Generated invoice for Order {order.Id}, Total: {order.TotalAmount:C}");
        }
    }
}