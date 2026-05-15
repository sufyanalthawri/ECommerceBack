using ECommerceBack.Core.Entities;

namespace ECommerceBack.Core.Interfaces
{
    /// <summary>
    /// واجهة خاصة بمنتجات (ترث من IRepository). فارغة حالياً لأن الدوال الأساسية كافية.
    /// يمكن إضافة دوال بحث أو تصفية لاحقاً.
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        // يتم توريث دوال CRUD الأساسية (GetById, GetAll, Add, Update, Delete)
    }
}