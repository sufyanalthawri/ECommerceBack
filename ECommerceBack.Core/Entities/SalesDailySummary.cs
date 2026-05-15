using ECommerceBack.Core.Entities;

/// <summary>
/// يمثل ملخص المبيعات اليومية الذي يتم إنشاؤه عبر مهمة Batch Processing (Hangfire).
/// يستخدم لتسريع استعلامات التقارير (FR20, FR21).
/// </summary>
public class SalesDailySummary
{
    /// <summary>المعرف الفريد للملخص.</summary>
    public int Id { get; set; }

    /// <summary>تاريخ الملخص (اليوم الذي تم تلخيصه).</summary>
    public DateTime Date { get; set; }

    /// <summary>إجمالي عدد الطلبات في ذلك اليوم.</summary>
    public int TotalOrders { get; set; }

    /// <summary>إجمالي قيمة المبيعات في ذلك اليوم.</summary>
    public decimal TotalSales { get; set; }

    /// <summary>معرف المنتج الأكثر مبيعاً في ذلك اليوم (nullable في حال عدم وجود مبيعات).</summary>
    public int? TopProductId { get; set; }

    /// <summary>المنتج الأكثر مبيعاً (علاقة اختيارية).</summary>
    public Product? TopProduct { get; set; }

    /// <summary>تاريخ آخر تحديث للملخص (وقت تشغيل مهمة Batch Processing).</summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}