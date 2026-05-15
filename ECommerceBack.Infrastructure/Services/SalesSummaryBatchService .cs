using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;

namespace ECommerceBack.Infrastructure.Services
{
    /// <summary>
    /// خدمة معالجة المبيعات اليومية على دفعات (Batch Processing).
    /// تطبق واجهة ISalesSummaryBatchService.
    /// تستخدم Cursor-based Pagination (WHERE Id > lastId) لقراءة الطلبات على دفعات صغيرة (500 طلب)
    /// لتجنب استنزاف الذاكرة. تُستخدم هذه الخدمة ضمن مهمة Hangfire الدورية (المتطلب الرابع).
    /// </summary>
    public class SalesSummaryBatchService : ISalesSummaryBatchService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SalesSummaryBatchService> _logger;

        public SalesSummaryBatchService(AppDbContext context, ILogger<SalesSummaryBatchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// حساب ملخص المبيعات ليوم محدد باستخدام استراتيجية الدفعات (Chunks).
        /// يقوم بقراءة الطلبات على دفعات (حجم الدفعة = 500) باستخدام
        /// Cursor Pagination،
        /// ويجمع الإحصائيات (عدد الطلبات، إجمالي المبيعات، المنتج الأكثر مبيعاً)،
        /// ثم يخزن النتيجة في جدول SalesDailySummary .
        /// </summary>
        /// <param name="targetDate">التاريخ المطلوب حساب ملخصه).</param>
        public async Task ProcessDailySalesSummaryAsync(DateTime targetDate)
        {
            var startDate = targetDate.Date;
            var endDate = startDate.AddDays(1);

            _logger.LogInformation($"Processing daily sales summary for {startDate:yyyy-MM-dd}");

            const int batchSize = 500;
            int lastOrderId = 0;
            int totalOrders = 0;
            decimal totalSales = 0;
            var productSales = new Dictionary<int, int>();

            bool hasMore = true;
            while (hasMore)
            {
                // جلب دفعة من الطلبات باستخدام Cursor Pagination (أداء ثابت حتى مع ملايين السجلات)
                var ordersBatch = await _context.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate && o.Id > lastOrderId)
                    .OrderBy(o => o.Id)
                    .Take(batchSize)
                    .Include(o => o.OrderItems)
                    .ToListAsync();

                if (!ordersBatch.Any())
                {
                    hasMore = false;
                    break;
                }

                foreach (var order in ordersBatch)
                {
                    totalOrders++;
                    totalSales += order.TotalAmount;
                    foreach (var item in order.OrderItems)
                    {
                        productSales.TryGetValue(item.ProductId, out int current);
                        productSales[item.ProductId] = current + item.Quantity;
                    }
                }

                lastOrderId = ordersBatch.Last().Id;
                _logger.LogDebug($"Processed batch up to OrderId {lastOrderId}");
            }

            // تحديد المنتج الأكثر مبيعاً (أعلى كمية مباعة)
            int topProductId = 0;
            int maxQty = 0;
            foreach (var kvp in productSales)
            {
                if (kvp.Value > maxQty)
                {
                    maxQty = kvp.Value;
                    topProductId = kvp.Key;
                }
            }

            // حفظ أو تحديث الملخص في قاعدة البيانات (Upsert)
            var existing = await _context.SalesDailySummaries
                .FirstOrDefaultAsync(s => s.Date == startDate);
            if (existing != null)
            {
                existing.TotalOrders = totalOrders;
                existing.TotalSales = totalSales;
                existing.TopProductId = topProductId;
                existing.ProcessedAt = DateTime.UtcNow;
            }
            else
            {
                var summary = new SalesDailySummary
                {
                    Date = startDate,
                    TotalOrders = totalOrders,
                    TotalSales = totalSales,
                    TopProductId = topProductId,
                    ProcessedAt = DateTime.UtcNow
                };
                await _context.SalesDailySummaries.AddAsync(summary);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Sales summary for {startDate:yyyy-MM-dd}: Orders={totalOrders}, Sales={totalSales:C}, TopProductId={topProductId}");
        }
    }
}