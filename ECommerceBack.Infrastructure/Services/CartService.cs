using System.Collections.Concurrent;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.ECommerceBack.Core.Interfaces;

namespace ECommerceBack.Infrastructure.Services
{
    /// <summary>
    /// خدمة إدارة سلة التسوق. تطبق واجهة ICartService.
    /// تستخدم قفلاً (SemaphoreSlim) لكل مستخدم لضمان سلامة العمليات المتزامنة على نفس السلة.
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        // قاموس لتخزين قفل لكل مستخدم (يسمح بدخول خيط واحد فقط لكل مستخدم)
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> _userLocks = new();

        public CartService(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        /// <summary>الحصول على قفل خاص بالمستخدم (ينشئ قفلاً جديداً إذا لم يكن موجوداً).</summary>
        private SemaphoreSlim GetUserLock(int userId)
        {
            return _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        }

        /// <summary>الحصول على سلة المستخدم أو إنشاء واحدة جديدة (داخلية، بدون قفل).</summary>
        private async Task<Cart> GetOrCreateCartInternalAsync(int userId)
        {
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartRepository.AddAsync(cart);
            }
            return cart;
        }

        /// <summary>
        /// الحصول على سلة المستخدم (أو إنشاء واحدة جديدة) مع تطبيق القفل لضمان عدم تداخل العمليات.
        /// </summary>
        /// <param name="userId">معرف المستخدم.</param>
        /// <returns>سلة المستخدم (مضمونة الوجود).</returns>
        public async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var userLock = GetUserLock(userId);
            await userLock.WaitAsync();
            try
            {
                return await GetOrCreateCartInternalAsync(userId);
            }
            finally
            {
                userLock.Release();
            }
        }

        /// <summary>
        /// إضافة منتج إلى سلة المستخدم. إذا كان المنتج موجوداً بالفعل، تزيد الكمية، وإلا يضاف عنصر جديد.
        /// </summary>
        /// <param name="userId">معرف المستخدم.</param>
        /// <param name="productId">معرف المنتج.</param>
        /// <param name="quantity">الكمية المطلوب إضافتها (أكبر من صفر).</param>
        public async Task AddToCartAsync(int userId, int productId, int quantity)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("Quantity must be positive.");

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new InvalidOperationException("Product not found.");

            var userLock = GetUserLock(userId);
            await userLock.WaitAsync();
            try
            {
                var cart = await GetOrCreateCartInternalAsync(userId);
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    await _cartRepository.UpdateAsync(cart);
                }
                else
                {
                    var newItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        Quantity = quantity
                    };
                    cart.CartItems.Add(newItem);
                    await _cartRepository.UpdateAsync(cart);
                }
            }
            finally
            {
                userLock.Release();
            }
        }

        /// <summary>
        /// تعديل كمية عنصر معين في السلة. إذا كانت الكمية 0، يُحذف العنصر.
        /// </summary>
        /// <param name="userId">معرف المستخدم.</param>
        /// <param name="cartItemId">معرف عنصر السلة المراد تعديله.</param>
        /// <param name="quantity">الكمية الجديدة (غير سالبة).</param>
        public async Task UpdateCartItemQuantityAsync(int userId, int cartItemId, int quantity)
        {
            if (quantity < 0)
                throw new InvalidOperationException("Quantity cannot be negative.");

            var userLock = GetUserLock(userId);
            await userLock.WaitAsync();
            try
            {
                var cart = await GetOrCreateCartInternalAsync(userId);
                var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                if (item == null)
                    throw new InvalidOperationException("Cart item not found.");

                if (quantity == 0)
                {
                    cart.CartItems.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                await _cartRepository.UpdateAsync(cart);
            }
            finally
            {
                userLock.Release();
            }
        }

        /// <summary>
        /// إزالة عنصر محدد من السلة.
        /// </summary>
        /// <param name="userId">معرف المستخدم.</param>
        /// <param name="cartItemId">معرف عنصر السلة المراد إزالته.</param>
        public async Task RemoveFromCartAsync(int userId, int cartItemId)
        {
            var userLock = GetUserLock(userId);
            await userLock.WaitAsync();
            try
            {
                var cart = await GetOrCreateCartInternalAsync(userId);
                var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                if (item != null)
                {
                    cart.CartItems.Remove(item);
                    await _cartRepository.UpdateAsync(cart);
                }
            }
            finally
            {
                userLock.Release();
            }
        }

        /// <summary>
        /// تفريغ السلة بالكامل (حذف جميع العناصر).
        /// </summary>
        /// <param name="userId">معرف المستخدم.</param>
        public async Task ClearCartAsync(int userId)
        {
            var userLock = GetUserLock(userId);
            await userLock.WaitAsync();
            try
            {
                var cart = await GetOrCreateCartInternalAsync(userId);
                cart.CartItems.Clear();
                await _cartRepository.UpdateAsync(cart);
            }
            finally
            {
                userLock.Release();
            }
        }
    }
}