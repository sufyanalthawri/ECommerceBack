using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceBack.Infrastructure.Decorators
{
    /// <summary>
    /// Decorator لإضافة Logging تلقائي حول جميع دوال IProductService.
    /// يطبق نمط AOP (البرمجة الموجهة نحو الجوانب) لفصل التسجيل عن منطق الأعمال الأساسي.
    /// يسجل دخول وخروج كل دالة مع المعاملات والنتائج (عدد العناصر، وجود المنتج، المعرف المُنشأ).
    /// </summary>
    public class ProductServiceLoggingDecorator : IProductService
    {
        private readonly IProductService _inner;
        private readonly ILogger<ProductServiceLoggingDecorator> _logger;

        public ProductServiceLoggingDecorator(IProductService inner, ILogger<ProductServiceLoggingDecorator> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        /// <summary>جلب جميع المنتجات مع تسجيل عدد النتائج.</summary>
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            _logger.LogInformation(" ProductService.GetAllProductsAsync called");
            var result = await _inner.GetAllProductsAsync();
            _logger.LogInformation(" ProductService.GetAllProductsAsync completed (Count: {Count})", result.Count());
            return result;
        }

        /// <summary>جلب منتج محدد بالمعرف مع تسجيل وجود المنتج.</summary>
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            _logger.LogInformation(" ProductService.GetProductByIdAsync called (ProductId: {ProductId})", id);
            var result = await _inner.GetProductByIdAsync(id);
            _logger.LogInformation(" ProductService.GetProductByIdAsync completed (Found: {Found})", result != null);
            return result;
        }

        /// <summary>إضافة منتج جديد مع تسجيل الاسم والسعر والمعرف المُنشأ.</summary>
        public async Task<Product> CreateProductAsync(Product product)
        {
            _logger.LogInformation(" ProductService.CreateProductAsync called (Name: {Name}, Price: {Price})", product.Name, product.Price);
            var result = await _inner.CreateProductAsync(product);
            _logger.LogInformation(" ProductService.CreateProductAsync completed (ProductId: {ProductId})", result.Id);
            return result;
        }

        /// <summary>تحديث منتج موجود مع تسجيل المعرف والاسم.</summary>
        public async Task UpdateProductAsync(Product product)
        {
            _logger.LogInformation("ProductService.UpdateProductAsync called (ProductId: {ProductId}, Name: {Name})", product.Id, product.Name);
            await _inner.UpdateProductAsync(product);
            _logger.LogInformation(" ProductService.UpdateProductAsync completed");
        }

        /// <summary>حذف منتج مع تسجيل المعرف.</summary>
        public async Task DeleteProductAsync(int id)
        {
            _logger.LogInformation("▶️ ProductService.DeleteProductAsync called (ProductId: {ProductId})", id);
            await _inner.DeleteProductAsync(id);
            _logger.LogInformation("✅ ProductService.DeleteProductAsync completed");
        }
    }
}