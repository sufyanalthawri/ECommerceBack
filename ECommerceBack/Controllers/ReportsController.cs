using Microsoft.AspNetCore.Mvc;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Core.DTOs;

namespace ECommerceBack.API.Controllers;

/// <summary>
/// وحدة تحكم التقارير (Reports Controller).
/// توفر نقاط نهاية لعرض إحصائيات المبيعات اليومية والمنتج الأكثر مبيعاً.
/// </summary>
[ApiController]
[Route("api/[controller]")]
// [Authorize] // يمكن تفعيلها لاحقاً للمسؤولين فقط
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// عرض إجمالي المبيعات ليوم محدد .
    /// </summary>
    /// <param name="date">التاريخ المطلوب (اختياري، القيمة الافتراضية هي التاريخ الحالي).</param>
    /// <returns>
    /// كائن DailySalesSummaryDto يحتوي على:
    /// - تاريخ الملخص
    /// - إجمالي عدد الطلبات
    /// - إجمالي قيمة المبيعات
    /// - المنتج الأكثر مبيعاً وكميته
    /// </returns>
    /// <response code="200">تم استرداد البيانات بنجاح.</response>
    [HttpGet("daily-summary")]
    public async Task<IActionResult> GetDailySummary([FromQuery] DateTime? date)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var summary = await _reportService.GetDailySalesSummaryAsync(targetDate);
        return Ok(summary);
    }

    /// <summary>
    /// عرض المنتج الأكثر مبيعاً خلال آخر 7 أيام (FR21).
    /// </summary>
    /// <returns>
    /// كائن TopProductDto يحتوي على:
    /// - معرف المنتج الأكثر مبيعاً
    /// - اسم المنتج
    /// - إجمالي الكمية المباعة خلال الأسبوع الماضي
    /// </returns>
    /// <response code="200">تم استرداد البيانات بنجاح.</response>
    [HttpGet("top-product-week")]
    public async Task<IActionResult> GetTopProductLast7Days()
    {
        var topProduct = await _reportService.GetTopProductLast7DaysAsync();
        return Ok(topProduct);
    }
}