namespace ECommerceBack.Core.DTOs
{
    /// <summary>
    /// كائن نقل بيانات (DTO) يمثل المنتج الأكثر مبيعاً خلال فترة زمنية محددة.
    /// يستخدم في واجهة التقارير (FR21) لعرض المنتج الأكثر مبيعاً في آخر 7 أيام.
    /// </summary>
    public class TopProductDto
    {
        /// <summary>معرف المنتج الأكثر مبيعاً.</summary>
        public int ProductId { get; set; }

        /// <summary>اسم المنتج الأكثر مبيعاً.</summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>إجمالي الكمية المباعة من هذا المنتج خلال الفترة.</summary>
        public int TotalQuantitySold { get; set; }
    }
}