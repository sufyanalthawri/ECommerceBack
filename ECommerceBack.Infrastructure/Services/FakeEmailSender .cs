using Microsoft.Extensions.Logging;
using ECommerceBack.Core.Interfaces;

namespace ECommerceBack.Infrastructure.Services
{
    /// <summary>
    /// تطبيق وهمي (Fake) لواجهة IEmailSender.
    /// يستخدم لاختبار Hangfire وإعادة المحاولة التلقائية دون الحاجة إلى خدمة بريد حقيقية.
    /// يرمي استثناءً عشوائياً بنسبة 50% لمحاكاة فشل عابر.
    /// </summary>
    public class FakeEmailSender : IEmailSender
    {
        private readonly ILogger<FakeEmailSender> _logger;
        private static readonly Random _random = new();

        public FakeEmailSender(ILogger<FakeEmailSender> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// محاكاة إرسال بريد إلكتروني: تأخير (200–500 مللي ثانية) وفشل عشوائي 50%.
        /// </summary>
        /// <param name="to">عنوان المستلم.</param>
        /// <param name="subject">عنوان البريد.</param>
        /// <param name="body">محتوى البريد (نص أو HTML).</param>
        /// <exception cref="Exception">يتم رميه بشكل عشوائي لاختبار إعادة محاولة Hangfire.</exception>
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // محاكاة تأخير الشبكة
            await Task.Delay(Random.Shared.Next(200, 500));

            // إلقاء استثناء بشكل عشوائي بنسبة 50% (يمكن تعديل النسبة حسب الرغبة)
            if (_random.NextDouble() < 0.5)
            {
                _logger.LogWarning($"⚠️ [FAKE EMAIL] فشل عشوائي محاكى لإرسال البريد إلى {to}");
                throw new Exception("Simulated random email failure - will be retried by Hangfire");
            }

            _logger.LogInformation($"📧 [FAKE EMAIL] To: {to}, Subject: {subject}, Body: {body}");
        }
    }
}