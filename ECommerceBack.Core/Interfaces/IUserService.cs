using ECommerceBack.Core.Entities;

namespace ECommerceBack.ECommerceBack.Core.Interfaces;
public interface IUserService
{
    /// <summary>تسجيل مستخدم جديد (يتحقق من عدم تكرار البريد).</summary>
    Task<User> RegisterAsync(string name, string email, string password);

    /// <summary>تسجيل الدخول: التحقق من البريد وكلمة المرور وإرجاع JWT token.</summary>
    Task<string?> LoginAsync(string email, string password);

    /// <summary>جلب مستخدم بالمعرف (للملف الشخصي).</summary>
    Task<User?> GetUserByIdAsync(int id);
}