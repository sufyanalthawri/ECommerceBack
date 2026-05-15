using System.Collections.Concurrent;
using ECommerceBack.Core.Interfaces;

namespace ECommerceBack.Infrastructure.Services
{
    /// <summary>
    /// تطبيق واجهة IConcurrencyManager باستخدام ConcurrentDictionary و SemaphoreSlim.
    /// يحد من عدد العمليات المتزامنة على مورد معين عبر قفل لكل مفتاح (مثل المنتج).
    /// يستخدم في المتطلب غير الوظيفي الثاني (إدارة الموارد).
    /// </summary>
    public class ConcurrencyManager : IConcurrencyManager
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

        /// <summary>
        /// ينفذ عملية معينة مع تحديد عدد المهام المتزامنة المسموح بها لمفتاح مورد معين.
        /// </summary>
        /// <param name="resourceKey">مفتاح فريد للمورد (مثلاً "checkout_1" للمنتج رقم 1).</param>
        /// <param name="action">الدالة غير المتزامنة المراد تنفيذها مع حماية التزامن.</param>
        /// <param name="maxConcurrency">الحد الأقصى للعمليات المتزامنة المسموح بها (افتراضي 10).</param>
        /// <typeparam name="T">نوع القيمة المعادة من الدالة.</typeparam>
        /// <returns>نتيجة تنفيذ الدالة.</returns>
        public async Task<T> ExecuteWithLimitAsync<T>(string resourceKey, Func<Task<T>> action, int maxConcurrency = 10)
        {
            var semaphore = _semaphores.GetOrAdd(resourceKey, _ => new SemaphoreSlim(maxConcurrency, maxConcurrency));
            await semaphore.WaitAsync();
            try
            {
                return await action();
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}