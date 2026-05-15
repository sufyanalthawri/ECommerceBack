namespace ECommerceBack.Core.Interfaces
{
    /// <summary>
    /// يدير العمليات المتزامنة ويحدد عدد الخيوط المسموح بها لمورد معين.
    /// يستخدم في المتطلب غير الوظيفي رقم 2: إدارة الموارد الحاسوبية.
    /// </summary>
    public interface IConcurrencyManager
    {
        /// <summary>
        /// ينفذ عملية معينة مع تحديد عدد المهام المتزامنة المسموح بها.
        /// </summary>
        /// <param name="resourceKey">مفتاح فريد للمورد (مثلاً "product_1" أو "checkout")</param>
        /// <param name="action">الدالة المراد تنفيذها مع تحديد عدد الخيوط المتزامنة</param>
        /// <param name="maxConcurrency">الحد الأقصى للعمليات المتزامنة (افتراضي 10)</param>
        Task<T> ExecuteWithLimitAsync<T>(string resourceKey, Func<Task<T>> action, int maxConcurrency = 10);
    }
}