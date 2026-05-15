using ECommerceBack.Core.Interfaces;
using ECommerceBack.Core.Entities;

namespace ECommerceBack.Infrastructure.Services
{
    /// <summary>
    /// تطبيق وهمي (Fake) لواجهة IPaymentGateway.
    /// يحاكي معالجة الدفع بتأخير عشوائي (200–500 مللي ثانية) ونجاح بنسبة 90%.
    /// يستخدم للاختبار والتطوير دون الحاجة إلى بوابة دفع حقيقية.
    /// </summary>
    public class FakePaymentGateway : IPaymentGateway
    {
        private readonly Random _random = new();

        /// <summary>
        /// معالجة طلب دفع وهمي.
        /// </summary>
        /// <param name="amount">المبلغ المطلوب دفعه.</param>
        /// <param name="cardNumber">رقم البطاقة (لا يُستخدم في المحاكاة).</param>
        /// <returns>كائن PaymentResult يحتوي على نتيجة الدفع (نجاح/فشل)، معرف المعاملة، ورسالة.</returns>
        public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, string cardNumber)
        {
            await Task.Delay(_random.Next(200, 500));
            bool success = _random.NextDouble() < 0.9; // 90% success

            return new PaymentResult
            {
                Success = success,
                TransactionId = success ? Guid.NewGuid().ToString() : string.Empty,
                Message = success ? "Payment approved" : "Payment declined"
            };
        }
    }
}