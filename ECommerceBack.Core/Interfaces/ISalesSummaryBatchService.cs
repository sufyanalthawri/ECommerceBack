namespace ECommerceBack.Core.Interfaces
{
    /// <summary>
    /// خدمة مسؤولة عن معالجة المبيعات اليومية على دفعات (Batch Processing)
    /// وتخزين النتائج في جدول SalesDailySummary.
    /// يستخدم في المتطلب غير الوظيفي رقم 4: معالجة البيانات الضخمة على دفعات.
    /// </summary>
    public interface ISalesSummaryBatchService
    {
        /// <summary>
        /// يقوم بحساب ملخص المبيعات ليوم محدد باستخدام استراتيجية الدفعات (Cursor Pagination).
        /// </summary>
        /// <param name="targetDate">التاريخ المطلوب حساب ملخصه .</param>
        Task ProcessDailySalesSummaryAsync(DateTime targetDate);
    }
}