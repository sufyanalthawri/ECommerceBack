using Microsoft.EntityFrameworkCore;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;

namespace ECommerceBack.Infrastructure.Repositories;

/// <summary>
/// تطبيق خاص بعمليات المستخدم. يضيف دوالاً للبحث بالبريد الإلكترون).
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// البحث عن مستخدم بواسطة البريد الإلكتروني .
    /// </summary>
    /// <param name="email">البريد الإلكتروني للمستخدم.</param>
    /// <returns>كائن User، أو null إذا لم يوجد.</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
}