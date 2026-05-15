using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;

namespace ECommerceBack.Infrastructure.Repositories
{
    /// <summary>
    /// تطبيق خاص بعمليات المنتج (Product).
    /// يورث من GenericRepository ويطبق IProductRepository.
    /// حالياً لا يحتوي على دوال إضافية، لكن يمكن توسيعه مستقبلاً بدوال بحث أو تصفية خاصة بالمنتجات.
    /// </summary>
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context) { }
    }
}