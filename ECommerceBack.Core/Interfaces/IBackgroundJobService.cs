namespace ECommerceBack.Core.Interfaces
{
    /// <summary>
    /// واجهة للمهام الخلفية غير المتزامنة التي تُنفذ عبر Hangfire.
    /// تستخدم في المتطلب الثالث (المعالجة غير المتزامنة).
    /// </summary>
    public interface IBackgroundJobService
    {
        /// <summary>إرسال بريد إلكتروني تأكيدي للمستخدم بعد إنشاء الطلب بنجاح.</summary>
        /// <param name="orderId">معرف الطلب.</param>
        Task SendOrderConfirmationEmailAsync(int orderId);

        /// <summary>إنشاء فاتورة بصيغة PDF للطلب.</summary>
        /// <param name="orderId">معرف الطلب.</param>
        Task GenerateInvoicePdfAsync(int orderId);

        /// <summary>تحديث إحصائيات المبيعات (يُستخدم في المتطلب الرابع).</summary>
        /// <param name="orderId">معرف الطلب.</param>
        Task UpdateSalesStatisticsAsync(int orderId);
    }
}