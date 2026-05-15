using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerceBack.API.Filters
{
    /// <summary>
    /// فلتر استثناءات عام يلتقط جميع الاستثناءات غير المعالجة في جميع الـ Controllers والـ Actions.
    /// ينفذ معالجة مركزية للأخطاء ويعيد استجابة موحدة (500 Internal Server Error).
    /// </summary>
    /// <remarks>
    /// يقوم هذا الفلتر بتسجيل تفاصيل الاستثناء باستخدام ILogger ويمنع تسريب المعلومات الحساسة (مثل Stack Trace)
    /// إلى العميل. يقوم بتعيين ExceptionHandled = true لمنع انتشار الاستثناء.
    /// </remarks>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        /// <summary>
        /// تهيئة نسخة جديدة من كلاس <see cref="GlobalExceptionFilter"/>.
        /// </summary>
        /// <param name="logger">كائن التسجيل لتسجيل تفاصيل الاستثناءات.</param>
        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// يتم استدعاؤها عند حدوث استثناء غير معالج أثناء تنفيذ الـ Action.
        /// </summary>
        /// <param name="context">سياق الاستثناء الذي يحتوي على تفاصيل الخطأ وسياق HTTP.</param>
        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception");
            context.Result = new ObjectResult(new { error = "An internal error occurred. Please try again later." })
            {
                StatusCode = 500
            };
            context.ExceptionHandled = true;
        }
    }
}