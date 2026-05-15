using ECommerceBack.Core.Entities;

namespace ECommerceBack.Core.Interfaces
{
    /// <summary>
    /// واجهة خاصة لعمليات سلة التسوق، بالإضافة إلى الـ CRUD الأساسي من IRepository.
    /// </summary>
    public interface ICartRepository : IRepository<Cart>
    {
        /// <summary>جلب سلة المستخدم مع تحميل جميع عناصرها (بما فيها بيانات المنتج).</summary>
        /// <param name="userId">معرف المستخدم.</param>
        /// <returns>سلة المستخدم مع عناصرها، أو null إذا لم توجد.</returns>
        Task<Cart?> GetCartWithItemsByUserIdAsync(int userId);

        /// <summary>جلب عنصر معين من السلة باستخدام معرف العنصر.</summary>
        /// <param name="cartItemId">معرف عنصر السلة.</param>
        Task<CartItem?> GetCartItemByIdAsync(int cartItemId);

        /// <summary>إزالة عنصر من السلة.</summary>
        /// <param name="cartItem">عنصر السلة المراد إزالته.</param>
        Task RemoveCartItemAsync(CartItem cartItem);
    }
}