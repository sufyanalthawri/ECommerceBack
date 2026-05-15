using Microsoft.EntityFrameworkCore;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;

namespace ECommerceBack.Infrastructure.Repositories;

/// <summary>
/// تطبيق خاص بعمليات سلة التسوق، يعتمد على GenericRepository ويضيف دوالاً لجلب السلة مع عناصرها.
/// </summary>
public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    public CartRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// جلب سلة مستخدم مع تحميل جميع عناصرها ومنتجاتها دفعة واحدة (Eager Loading).
    /// </summary>
    /// <param name="userId">معرف المستخدم.</param>
    /// <returns>كائن Cart يحتوي على عناصره، أو null إذا لم توجد سلة.</returns>
    public async Task<Cart?> GetCartWithItemsByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    /// <summary>
    /// جلب عنصر معين من السلة باستخدام معرف العنصر، مع تحميل بيانات المنتج المرتبط.
    /// </summary>
    /// <param name="cartItemId">معرف عنصر السلة.</param>
    /// <returns>كائن CartItem مع منتجه، أو null إذا لم يوجد.</returns>
    public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
    {
        return await _context.Set<CartItem>()
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
    }

    /// <summary>
    /// حذف عنصر من السلة مباشرة من قاعدة البيانات.
    /// </summary>
    /// <param name="cartItem">العنصر المراد حذفه.</param>
    public async Task RemoveCartItemAsync(CartItem cartItem)
    {
        _context.Set<CartItem>().Remove(cartItem);
        await _context.SaveChangesAsync();
    }
}