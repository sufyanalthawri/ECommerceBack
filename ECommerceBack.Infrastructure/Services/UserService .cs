using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.ECommerceBack.Core.Interfaces;

namespace ECommerceBack.Infrastructure.Services
{
    /// <summary>
    /// خدمة إدارة المستخدمين (User Service).
    /// تطبق واجهة IUserService وتوفر وظائف التسجيل، تسجيل الدخول، وجلب بيانات المستخدم.
    /// تستخدم BCrypt لتشفير كلمات المرور و JWT لإنشاء توكنات المصادقة.
    /// تغطي المتطلبات الوظيفية  (تسجيل مستخدم جديد)،  (تسجيل الدخول)،  (عرض الملف الشخصي).
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        /// <summary>
        /// تسجيل مستخدم جديد في النظام.
        /// يتحقق من عدم وجود بريد إلكتروني مكرر، ويخزن كلمة المرور مشفرة باستخدام BCrypt.
        /// </summary>
        /// <param name="name">اسم المستخدم.</param>
        /// <param name="email">البريد الإلكتروني (فريد).</param>
        /// <param name="password">كلمة المرور (نص واضح، ستُشفر قبل التخزين).</param>
        /// <returns>كائن User الذي تم إنشاؤه (مع المعرف).</returns>
        /// <exception cref="InvalidOperationException">يتم رميه إذا كان البريد الإلكتروني مستخدمًا بالفعل.</exception>
        public async Task<User> RegisterAsync(string name, string email, string password)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
                throw new InvalidOperationException("Email already exists");

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            return user;
        }

        /// <summary>
        /// تسجيل الدخول باستخدام البريد الإلكتروني وكلمة المرور.
        /// يتحقق من صحة البيانات ويعيد JWT token إذا كانت صحيحة.
        /// </summary>
        /// <param name="email">البريد الإلكتروني.</param>
        /// <param name="password">كلمة المرور (نص واضح).</param>
        /// <returns>JWT token كسلسلة نصية، أو null إذا فشل تسجيل الدخول.</returns>
        public async Task<string?> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return null;

            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isValid)
                return null;

            // إنشاء JWT token مع صلاحية 7 أيام
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "fallback_secret_key_min_32_characters_long_!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// جلب مستخدم باستخدام المعرف.
        /// يُستخدم في عرض الملف الشخصي للمستخدم الحالي .
        /// </summary>
        /// <param name="id">معرف المستخدم.</param>
        /// <returns>كائن User إذا وُجد، وإلا null.</returns>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }
    }
}