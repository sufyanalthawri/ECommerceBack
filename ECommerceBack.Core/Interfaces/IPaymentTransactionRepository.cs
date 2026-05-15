using ECommerceBack.Core.Entities;

namespace ECommerceBack.Core.Interfaces
{
    /// <summary>واجهة خاصة بمعاملات الدفع.</summary>
    public interface IPaymentTransactionRepository : IRepository<PaymentTransaction>
    {
        /// <summary>جلب معاملة الدفع المرتبطة بطلب معين.</summary>
        /// <param name="orderId">معرف الطلب.</param>
        Task<PaymentTransaction?> GetByOrderIdAsync(int orderId);
    }
}