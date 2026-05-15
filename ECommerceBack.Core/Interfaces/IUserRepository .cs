using ECommerceBack.Core.Entities;

namespace ECommerceBack.Core.Interfaces
{
    /// <summary>واجهة خاصة بعمليات المستخدم (بالإضافة إلى الـ CRUD الأساسي).</summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>البحث عن مستخدم بواسطة البريد الإلكتروني .</summary>
        /// <param name="email">البريد الإلكتروني.</param>
        Task<User?> GetByEmailAsync(string email);
    }
}