using System.Linq.Expressions;

namespace ECommerceBack.Core.Interfaces
{
    /// <summary>
    /// واجهة عامة لعمليات قاعدة البيانات (CRUD الأساسي).
    /// تطبق في طبقة Infrastructure باستخدام GenericRepository.
    /// </summary>
    /// <typeparam name="T">نوع الكيان (class).</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>جلب كيان بالمعرف.</summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>جلب جميع الكيانات).</summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>جلب كيانات حسب شرط معين (LINQ Expression).</summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>إضافة كيان جديد.</summary>
        Task<T> AddAsync(T entity);

        /// <summary>تحديث كيان موجود.</summary>
        Task UpdateAsync(T entity);

        /// <summary>حذف كيان.</summary>
        Task DeleteAsync(T entity);

        /// <summary>التحقق من وجود كيان وفق شرط.</summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}