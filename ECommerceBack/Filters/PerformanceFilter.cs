using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerceBack.API.Filters
{
    /// <summary>
    /// فلتر أداء يقوم بقياس زمن تنفيذ كل Action وتسجيله.
    /// يطبق نمط AOP (البرمجة الموجهة نحو الجوانب) لمراقبة الأداء.
    /// </summary>
    /// <remarks>
    /// يقوم هذا الفلتر بتشغيل Stopwatch قبل تنفيذ الـ Action وإيقافه بعد الانتهاء،
    /// ثم يسجل الزمن المستغرق. يساعد في اكتشاف نقاط البطء والاختناقات.
    /// </remarks>
    public class PerformanceFilter : IAsyncActionFilter
    {
        private readonly ILogger<PerformanceFilter> _logger;

        /// <summary>
        /// تهيئة نسخة جديدة من كلاس <see cref="PerformanceFilter"/>.
        /// </summary>
        /// <param name="logger">كائن التسجيل لتسجيل مقاييس الأداء.</param>
        public PerformanceFilter(ILogger<PerformanceFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// يتم استدعاؤها بشكل غير متزامن قبل وبعد تنفيذ الـ Action لقياس الأداء.
        /// </summary>
        /// <param name="context">سياق الـ Action الجاري تنفيذه.</param>
        /// <param name="next">المفوض الذي يمثل باقي خطوات المعالجة.</param>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();
            await next();
            stopwatch.Stop();
            var actionName = context.ActionDescriptor.DisplayName;
            _logger.LogInformation($"AOP Performance: {actionName} took {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}