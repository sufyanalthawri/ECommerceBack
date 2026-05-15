using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.ECommerceBack.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceBack.Infrastructure.Decorators
{
    /// <summary>
    /// Decorator لإضافة Logging تلقائي حول جميع دوال ICartService.
    /// يطبق نمط AOP (البرمجة الموجهة نحو الجوانب) لفصل التسجيل عن منطق الأعمال الأساسي.
    /// يسجل دخول وخروج كل دالة مع المعاملات والنتائج.
    /// </summary>
    public class CartServiceLoggingDecorator : ICartService
    {
        private readonly ICartService _inner;
        private readonly ILogger<CartServiceLoggingDecorator> _logger;

        public CartServiceLoggingDecorator(ICartService inner, ILogger<CartServiceLoggingDecorator> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        /// <summary>الحصول على سلة المستخدم (أو إنشاؤها) مع تسجيل الدخول/الخروج.</summary>
        public async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            _logger.LogInformation("▶️ CartService.GetOrCreateCartAsync called (UserId: {UserId})", userId);
            var result = await _inner.GetOrCreateCartAsync(userId);
            _logger.LogInformation(" CartService.GetOrCreateCartAsync completed (CartId: {CartId})", result.Id);
            return result;
        }

        /// <summary>إضافة منتج إلى السلة مع تسجيل المعاملات.</summary>
        public async Task AddToCartAsync(int userId, int productId, int quantity)
        {
            _logger.LogInformation(" CartService.AddToCartAsync called (UserId: {UserId}, ProductId: {ProductId}, Quantity: {Quantity})", userId, productId, quantity);
            await _inner.AddToCartAsync(userId, productId, quantity);
            _logger.LogInformation(" CartService.AddToCartAsync completed");
        }

        /// <summary>تعديل كمية منتج في السلة مع تسجيل المعاملات.</summary>
        public async Task UpdateCartItemQuantityAsync(int userId, int cartItemId, int quantity)
        {
            _logger.LogInformation(" CartService.UpdateCartItemQuantityAsync called (UserId: {UserId}, CartItemId: {CartItemId}, Quantity: {Quantity})", userId, cartItemId, quantity);
            await _inner.UpdateCartItemQuantityAsync(userId, cartItemId, quantity);
            _logger.LogInformation(" CartService.UpdateCartItemQuantityAsync completed");
        }

        /// <summary>إزالة منتج من السلة مع تسجيل المعاملات.</summary>
        public async Task RemoveFromCartAsync(int userId, int cartItemId)
        {
            _logger.LogInformation("CartService.RemoveFromCartAsync called (UserId: {UserId}, CartItemId: {CartItemId})", userId, cartItemId);
            await _inner.RemoveFromCartAsync(userId, cartItemId);
            _logger.LogInformation(" CartService.RemoveFromCartAsync completed");
        }

        /// <summary>تفريغ السلة بالكامل مع تسجيل المعاملات.</summary>
        public async Task ClearCartAsync(int userId)
        {
            _logger.LogInformation(" CartService.ClearCartAsync called (UserId: {UserId})", userId);
            await _inner.ClearCartAsync(userId);
            _logger.LogInformation(" CartService.ClearCartAsync completed");
        }
    }
}