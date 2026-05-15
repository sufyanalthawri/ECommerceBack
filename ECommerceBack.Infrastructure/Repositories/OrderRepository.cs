using Microsoft.EntityFrameworkCore;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;
using ECommerceBack.Core.Entities;

namespace ECommerceBack.Infrastructure.Repositories;

/// <summary>
/// تطبيق خاص بالطلبات، يوفر دوالاً لجلب الطلب مع عناصره ومعاملة الدفع.
/// </summary>
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// جلب طلب محدد مع تحميل عناصره (OrderItems) ومنتجاتها، بالإضافة إلى معاملة الدفع المرتبطة.
    /// </summary>
    /// <param name="orderId">معرف الطلب.</param>
    /// <returns>كائن Order مع كامل البيانات المرتبطة، أو null.</returns>
    public async Task<Order?> GetOrderWithItemsAndPaymentAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.PaymentTransaction)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    /// <summary>
    /// جلب جميع طلبات مستخدم معين، مرتبة من الأحدث إلى الأقدم، مع تحميل عناصرها ومنتجاتها وبيانات الدفع.
    /// </summary>
    /// <param name="userId">معرف المستخدم.</param>
    /// <returns>قائمة الطلبات (قد تكون فارغة).</returns>
    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.PaymentTransaction)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }
}