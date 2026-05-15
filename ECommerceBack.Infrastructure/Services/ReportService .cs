using Microsoft.EntityFrameworkCore;
using ECommerceBack.Core.DTOs;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;

namespace ECommerceBack.Infrastructure.Services
{
    /// <summary>
    /// خدمة التقارير (Report Service).
    /// توفر إحصائيات المبيعات اليومية والمنتج الأكثر مبيعاً في آخر 7 أيام.
    /// تعتمد على جدول SalesDailySummary (الذي يُحدث بواسطة Batch Processing) مع إمكانية الحساب المباشر كحل احتياطي.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// الحصول على ملخص المبيعات ليوم محدد.
        /// يقرأ أولاً من جدول SalesDailySummary، وإذا لم يكن موجوداً يحسبه مباشرة (على الطاير).
        /// </summary>
        /// <param name="date">التاريخ المطلوب (يؤخذ اليوم فقط).</param>
        /// <returns>كائن DailySalesSummaryDto يحتوي على إجمالي الطلبات، المبيعات، وأفضل منتج.</returns>
        public async Task<DailySalesSummaryDto> GetDailySalesSummaryAsync(DateTime date)
        {
            var targetDate = date.Date;
            var summary = await _context.SalesDailySummaries
                .FirstOrDefaultAsync(s => s.Date == targetDate);

            if (summary == null)
                return await CalculateDailySummaryOnTheFlyAsync(targetDate);

            string topProductName = "Unknown";
            if (summary.TopProductId.HasValue)
            {
                var product = await _context.Products.FindAsync(summary.TopProductId.Value);
                topProductName = product?.Name ?? "Unknown";
            }

            return new DailySalesSummaryDto
            {
                Date = summary.Date,
                TotalOrders = summary.TotalOrders,
                TotalSales = summary.TotalSales,
                TopProductId = summary.TopProductId ?? 0,
                TopProductName = topProductName
            };
        }

        /// <summary>
        /// حساب ملخص المبيعات بشكل مباشر (بدون الاعتماد على جدول الملخصات).
        /// يستخدم كحل احتياطي في حال عدم وجود بيانات مجهزة مسبقاً.
        /// </summary>
        private async Task<DailySalesSummaryDto> CalculateDailySummaryOnTheFlyAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var orders = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.Status == OrderStatus.Paid)
                .Include(o => o.OrderItems)
                .ToListAsync();

            var totalOrders = orders.Count;
            var totalSales = orders.Sum(o => o.TotalAmount);

            var topProduct = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.Quantity)
                .FirstOrDefault();

            string topProductName = "";
            if (topProduct != null)
            {
                var product = await _context.Products.FindAsync(topProduct.ProductId);
                topProductName = product?.Name ?? "Unknown";
            }

            return new DailySalesSummaryDto
            {
                Date = startDate,
                TotalOrders = totalOrders,
                TotalSales = totalSales,
                TopProductId = topProduct?.ProductId ?? 0,
                TopProductName = topProductName
            };
        }

        /// <summary>
        /// الحصول على المنتج الأكثر مبيعاً خلال آخر 7 أيام.
        /// يستخدم جدول SalesDailySummary إن أمكن، وإلا يحسب مباشرة.
        /// </summary>
        /// <returns>كائن TopProductDto يحتوي على معرف المنتج واسمه وإجمالي الكمية المباعة.</returns>
        public async Task<TopProductDto> GetTopProductLast7DaysAsync()
        {
            var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-7);

            var summaries = await _context.SalesDailySummaries
                .Where(s => s.Date >= sevenDaysAgo)
                .ToListAsync();

            if (!summaries.Any())
                return await CalculateTopProductDirectlyAsync(sevenDaysAgo);

            var productSales = new Dictionary<int, int>();
            foreach (var summary in summaries)
            {
                if (summary.TopProductId.HasValue)
                {
                    var productId = summary.TopProductId.Value;
                    productSales.TryGetValue(productId, out int current);
                    productSales[productId] = current + 1;
                }
            }

            var top = productSales.OrderByDescending(kv => kv.Value).FirstOrDefault();
            if (top.Key == 0)
                return new TopProductDto();

            var product = await _context.Products.FindAsync(top.Key);
            return new TopProductDto
            {
                ProductId = top.Key,
                ProductName = product?.Name ?? "Unknown",
                TotalQuantitySold = top.Value
            };
        }

        /// <summary>
        /// حساب المنتج الأكثر مبيعاً بشكل مباشر (بدون الاعتماد على جدول الملخصات).
        /// </summary>
        private async Task<TopProductDto> CalculateTopProductDirectlyAsync(DateTime fromDate)
        {
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderDate >= fromDate && oi.Order.Status == OrderStatus.Paid)
                .ToListAsync();

            var top = orderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.TotalQuantity)
                .FirstOrDefault();

            if (top == null)
                return new TopProductDto();

            var product = await _context.Products.FindAsync(top.ProductId);
            return new TopProductDto
            {
                ProductId = top.ProductId,
                ProductName = product?.Name ?? "Unknown",
                TotalQuantitySold = top.TotalQuantity
            };
        }
    }
}