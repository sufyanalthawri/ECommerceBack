using ECommerceBack.Core.Entities;

namespace ECommerceBack.Core.Interfaces
{
    /// <summary>
    /// نمط Façade (واجهة مبسطة) لعملية الشراء الكاملة.
    /// يجمع استدعاءات السلة، الطلب، Hangfire، وتفريغ السلة في دالة واحدة.
    /// </summary>
    public interface ICheckoutFacade
    {
        /// <summary>
        /// تنفيذ عملية شراء كاملة: إضافة إلى السلة، إنشاء الطلب، جدولة المهام الخلفية، تفريغ السلة.
        /// </summary>
        Task<Order> PlaceOrderAsync(int userId, int productId, int quantity, string cardNumber);
    }
}