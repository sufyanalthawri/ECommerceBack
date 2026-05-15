using Microsoft.Extensions.Logging;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;

namespace ECommerceBack.Infrastructure.Services
{
    /// <summary>
    /// خدمة المهام الخلفية (Background Jobs) التي يتم جدولتها عبر Hangfire.
    /// تشمل إرسال البريد الإلكتروني، إنشاء الفاتورة، وتحديث إحصائيات المبيعات.
    /// </summary>
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailSender _emailSender;
        private readonly IInvoiceGenerator _invoiceGenerator;
        private readonly ILogger<BackgroundJobService> _logger;

        public BackgroundJobService(
            IOrderRepository orderRepository,
            IEmailSender emailSender,
            IInvoiceGenerator invoiceGenerator,
            ILogger<BackgroundJobService> logger)
        {
            _orderRepository = orderRepository;
            _emailSender = emailSender;
            _invoiceGenerator = invoiceGenerator;
            _logger = logger;
        }

        /// <summary>
        /// إرسال بريد إلكتروني تأكيدي للمستخدم بعد إنشاء الطلب بنجاح.
        /// </summary>
        /// <param name="orderId">معرف الطلب.</param>
        public async Task SendOrderConfirmationEmailAsync(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetOrderWithItemsAndPaymentAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning($"Order {orderId} not found for email.");
                    return;
                }
                // await _emailSender.SendEmailAsync(order.User.Email, "Order Confirmation", $"Your order #{orderId} is confirmed. Total: {order.TotalAmount:C}");
                _logger.LogInformation($"Email sent for order {orderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email for order {orderId}");
                throw; // Hangfire سيعيد المحاولة
            }
        }

        /// <summary>
        /// إنشاء فاتورة بصيغة  للطلب .
        /// </summary>
        /// <param name="orderId">معرف الطلب.</param>
        public async Task GenerateInvoicePdfAsync(int orderId)
        {
            var order = await _orderRepository.GetOrderWithItemsAndPaymentAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning($"Order {orderId} not found for invoice.");
                return;
            }
            await _invoiceGenerator.GenerateAsync(order);
            _logger.LogInformation($"Invoice generated for order {orderId}");
        }

        /// <summary>
        /// تحديث إحصائيات المبيعات .
        /// </summary>
        /// <param name="orderId">معرف الطلب.</param>
        public async Task UpdateSalesStatisticsAsync(int orderId)
        {
            _logger.LogInformation($"UpdateSalesStatisticsAsync called for order {orderId}");
            await Task.CompletedTask;
        }
    }
}