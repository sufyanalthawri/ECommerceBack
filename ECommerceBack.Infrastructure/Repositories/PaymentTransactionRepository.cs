using Microsoft.EntityFrameworkCore;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;

namespace ECommerceBack.Infrastructure.Repositories;

/// <summary>
/// تطبيق خاص بمعاملات الدفع. يوفر دوالاً إضافية للبحث بالطلب المرتبط.
/// </summary>
public class PaymentTransactionRepository : GenericRepository<PaymentTransaction>, IPaymentTransactionRepository
{
    public PaymentTransactionRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// جلب معاملة الدفع المرتبطة بطلب معين.
    /// </summary>
    /// <param name="orderId">معرف الطلب.</param>
    /// <returns>كائن PaymentTransaction، أو null إذا لم توجد معاملة.</returns>
    public async Task<PaymentTransaction?> GetByOrderIdAsync(int orderId)
    {
        return await _dbSet.FirstOrDefaultAsync(pt => pt.OrderId == orderId);
    }
}