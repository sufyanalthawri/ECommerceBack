using Microsoft.AspNetCore.Mvc.Filters;
using ECommerceBack.Infrastructure.Data;

namespace ECommerceBack.API.Filters
{
    /// <summary>
    /// فلتر المعاملات الذي يدير معاملات قاعدة البيانات تلقائياً لعمليات الكتابة (POST, PUT, DELETE).
    /// يطبق نمط AOP (البرمجة الموجهة نحو الجوانب) لإدارة المعاملات بشكل مركزي.
    /// </summary>
    /// <remarks>
    /// لكل طلب من نوع POST أو PUT أو DELETE:
    /// - يبدأ معاملة جديدة في قاعدة البيانات
    /// - ينفذ الـ Action
    /// - ينفذ Commit إذا لم يحدث استثناء وكان ModelState صحيحاً
    /// - ينفذ Rollback إذا حدث استثناء
    /// طلبات GET لا يتم تغليفها بمعاملات.
    /// الـ Controllers أو الـ Actions التي تحمل السمة [SkipTransactionFilter] يتم استثناؤها.
    /// </remarks>
    public class TransactionFilter : IAsyncActionFilter
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TransactionFilter> _logger;

        /// <summary>
        /// تهيئة نسخة جديدة من كلاس <see cref="TransactionFilter"/>.
        /// </summary>
        /// <param name="context">سياق قاعدة البيانات لإدارة المعاملات.</param>
        /// <param name="logger">كائن التسجيل لتسجيل أحداث المعاملات.</param>
        public TransactionFilter(AppDbContext context, ILogger<TransactionFilter> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// يتم استدعاؤها بشكل غير متزامن قبل وبعد تنفيذ الـ Action لإدارة المعاملة.
        /// </summary>
        /// <param name="context">سياق الـ Action الجاري تنفيذه.</param>
        /// <param name="next">المفوض الذي يمثل باقي خطوات المعالجة.</param>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // التحقق من وجود سمة الاستثناء على الـ Controller أو الـ Action
            var skipTransaction = context.Controller.GetType().GetCustomAttributes(typeof(SkipTransactionFilterAttribute), true).Any()
                                  || context.ActionDescriptor.EndpointMetadata.Any(em => em.GetType() == typeof(SkipTransactionFilterAttribute));

            if (skipTransaction)
            {
                _logger.LogDebug(" Transaction skipped because the Controller or Action has the SkipTransactionFilterAttribute");
                return;
            }

            var method = context.HttpContext.Request.Method;
            if (method == "POST" || method == "PUT" || method == "DELETE")
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                _logger.LogInformation(" Started new transaction for {Method} {Path}", method, context.HttpContext.Request.Path);

                var result = await next();

                if (result.Exception == null && context.ModelState.IsValid)
                {
                    await transaction.CommitAsync();
                    _logger.LogWarning(" Transaction rolled back due to an error");
                }
                else
                {
                    await transaction.RollbackAsync();
                    _logger.LogWarning(" Transaction rolled back due to an error");
                }
            }
            else
            {
                await next();
            }
        }
    }
}