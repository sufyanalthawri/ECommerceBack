using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;

namespace ECommerceBack.Infrastructure.Services
{
    /// <summary>
    /// خدمة إدارة المنتجات (Product Service).
    /// تطبق واجهة IProductService وتوفر دوال CRUD الأساسية للمنتجات.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>جلب جميع المنتجاة.</summary>
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        /// <summary>جلب منتج محدد باستخدام المعرف.</summary>
        /// <param name="id">معرف المنتج.</param>
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        /// <summary>إضافة منتج جديد إلى قاعدة البيانات.</summary>
        /// <param name="product">كائن المنتج المراد إضافته.</param>
        /// <returns>المنتج بعد الإضافة.</returns>
        public async Task<Product> CreateProductAsync(Product product)
        {
            return await _productRepository.AddAsync(product);
        }

        /// <summary>تحديث منتج موجود.</summary>
        /// <param name="product">كائن المنتج مع البيانات المحدثة.</param>
        public async Task UpdateProductAsync(Product product)
        {
            await _productRepository.UpdateAsync(product);
        }

        /// <summary>حذف منتج بالمعرف.</summary>
        /// <param name="id">معرف المنتج المراد حذفه.</param>
        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
                await _productRepository.DeleteAsync(product);
        }
    }
}