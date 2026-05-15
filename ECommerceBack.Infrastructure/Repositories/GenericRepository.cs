using System.Linq.Expressions;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerceBack.Infrastructure.Repositories;

/// <summary>
/// تطبيق عام لواجهة IRepository لكل الكيانات. يوفر دوال CRUD أساسية باستخدام DbContext.
/// </summary>
/// <typeparam name="T">نوع الكيان (class).</typeparam>
public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>جلب كيان بالمعرف الرئيسي (Primary Key).</summary>
    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    /// <summary>جلب جميع الكيانات .</summary>
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    /// <summary>جلب الكيانات التي تحقق شرطاً معيناً (LINQ Expression).</summary>
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    /// <summary>إضافة كيان جديد وحفظ التغييرات فوراً.</summary>
    /// <returns>الكيان بعد الإضافة (يحتوي على المعرف المولّد).</returns>
    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>تحديث كيان موجود وحفظ التغييرات.</summary>
    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>حذف كيان من قاعدة البيانات.</summary>
    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>التحقق من وجود كيان يحقق شرطاً معيناً.</summary>
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AnyAsync(predicate);
}