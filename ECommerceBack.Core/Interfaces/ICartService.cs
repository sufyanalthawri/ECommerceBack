using ECommerceBack.Core.Entities;

namespace ECommerceBack.ECommerceBack.Core.Interfaces; 
public interface ICartService
{
    /// <summary>الحصول على سلة المستخدم (أو إنشاء واحدة جديدة إذا لم تكن موجودة).</summary>
    Task<Cart> GetOrCreateCartAsync(int userId);

    /// <summary>إضافة منتج إلى سلة المستخدم (إذا كان موجوداً يزيد الكمية).</summary>
    Task AddToCartAsync(int userId, int productId, int quantity);

    /// <summary>تعديل كمية منتج معين في السلة.</summary>
    Task UpdateCartItemQuantityAsync(int userId, int cartItemId, int quantity);

    /// <summary>إزالة منتج معين من السلة.</summary>
    Task RemoveFromCartAsync(int userId, int cartItemId);

    /// <summary>تفريغ السلة بالكامل (حذف جميع العناصر).</summary>
    Task ClearCartAsync(int userId);
}