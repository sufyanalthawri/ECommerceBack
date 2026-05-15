using ECommerceBack.Core.Entities;

namespace ECommerceBack.Core.Interfaces
{
    /// <summary>خدمة إدارة المنتجات د.</summary>
    public interface IProductService
    {
        /// <summary>جلب جميع المنتجات  .</summary>
        Task<IEnumerable<Product>> GetAllProductsAsync();

        /// <summary>جلب منتج محدد بالمعرف.</summary>
        Task<Product?> GetProductByIdAsync(int id);

        /// <summary>إضافة منتج جديد.</summary>
        Task<Product> CreateProductAsync(Product product);

        /// <summary>تحديث منتج موجود.</summary>
        Task UpdateProductAsync(Product product);

        /// <summary>حذف منتج بالمعرف.</summary>
        Task DeleteProductAsync(int id);
    }
}