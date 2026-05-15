using ECommerceBack.Core.Entities;

namespace ECommerceBack.Core.Interfaces
{
    /// <summary>واجهة لإنشاء الفواتير  .</summary>
    public interface IInvoiceGenerator
    {
        /// <summary>إنشاء فاتورة للطلب المحدد.</summary>
        /// <param name="order">الطلب المراد إنشاء الفاتورة له.</param>
        Task GenerateAsync(Order order);
    }
}