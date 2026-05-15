using ECommerceBack.Core.Entities;

namespace ECommerceBack.Core.Interfaces
{
    /// <summary>واجهة خاصة لعمليات الطلبات، بالإضافة إلى الـ CRUD الأساسي.</summary>
    public interface IOrderRepository : IRepository<Order>
    {
        /// <summary>جلب طلب محدد مع تحميل عناصره ومعاملة الدفع المرتبطة.</summary>
        /// <param name="orderId">معرف الطلب.</param>
        Task<Order?> GetOrderWithItemsAndPaymentAsync(int orderId);

        /// <summary>جلب جميع طلبات مستخدم معين (مرتبة من الأحدث إلى الأقدم).</summary>
        /// <param name="userId">معرف المستخدم.</param>
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
    }
}