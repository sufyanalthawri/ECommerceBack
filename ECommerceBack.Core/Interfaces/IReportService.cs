using ECommerceBack.Core.DTOs;

namespace ECommerceBack.Core.Interfaces
{
    /// <summary>خدمة التقارير (المبيعات، المنتجات الأكثر مبيعاً).</summary>
    public interface IReportService
    {
        /// <summary>جلب ملخص المبيعات ليوم محدد (يعتمد على جدول SalesDailySummary).</summary>
        Task<DailySalesSummaryDto> GetDailySalesSummaryAsync(DateTime date);

        /// <summary>جلب المنتج الأكثر مبيعاً خلال آخر 7 أيام.</summary>
        Task<TopProductDto> GetTopProductLast7DaysAsync();
    }
}