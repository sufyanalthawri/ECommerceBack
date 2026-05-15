namespace ECommerceBack.Core.DTOs
{
    /// <summary>
    /// كائن نقل بيانات (DTO) يمثل ملخص المبيعات اليومية.
    /// لعرض إجمالي المبيعات لمنتج معين أو لليوم.
    /// يتم تعبئة هذا الكائن من جدول SalesDailySummary (الذي يُحدث بواسطة Batch Processing).
    /// </summary>
    public class DailySalesSummaryDto
    {
        /// <summary>تاريخ الملخص (اليوم الذي تم تلخيصه).</summary>
        public DateTime Date { get; set; }

        /// <summary>إجمالي عدد الطلبات في ذلك اليوم.</summary>
        public int TotalOrders { get; set; }

        /// <summary>إجمالي قيمة المبيعات (مجموع TotalAmount لجميع الطلبات).</summary>
        public decimal TotalSales { get; set; }

        /// <summary>معرف المنتج الأكثر مبيعاً في ذلك اليوم.</summary>
        public int TopProductId { get; set; }

        /// <summary>اسم المنتج الأكثر مبيعاً في ذلك اليوم.</summary>
        public string TopProductName { get; set; } = string.Empty;
    }
}