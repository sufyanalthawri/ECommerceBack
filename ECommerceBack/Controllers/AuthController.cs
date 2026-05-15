using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Core.DTOs;
using ECommerceBack.ECommerceBack.Core.Interfaces;

namespace ECommerceBack.API.Controllers;

/// <summary>
/// وحدة تحكم المصادقة (Authentication Controller).
/// يوفر نقاط نهاية لتسجيل المستخدمين، تسجيل الدخول، وعرض الملف الشخصي.
/// يغطي المتطلبات الوظيفية FR1 (تسجيل مستخدم جديد)، FR2 (تسجيل الدخول)، FR3 (عرض الملف الشخصي).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// تسجيل مستخدم جديد في النظام (FR1).
    /// </summary>
    /// <param name="request">بيانات المستخدم: الاسم، البريد الإلكتروني، كلمة المرور.</param>
    /// <returns>كائن يحتوي على معرف المستخدم واسمه وبريده الإلكتروني مع رسالة تأكيد.</returns>
    /// <response code="200">تم تسجيل المستخدم بنجاح.</response>
    /// <response code="400">البيانات غير صالحة أو البريد الإلكتروني موجود مسبقاً.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userService.RegisterAsync(request.Name, request.Email, request.Password);
            return Ok(new { user.Id, user.Name, user.Email, Message = "Registration successful" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// تسجيل الدخول إلى النظام (FR2).
    /// </summary>
    /// <param name="request">البريد الإلكتروني وكلمة المرور.</param>
    /// <returns>JWT token للمصادقة في الطلبات اللاحقة.</returns>
    /// <response code="200">تم تسجيل الدخول بنجاح، ويعيد التوكن.</response>
    /// <response code="400">البيانات غير صالحة.</response>
    /// <response code="401">البريد الإلكتروني أو كلمة المرور غير صحيحين.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var token = await _userService.LoginAsync(request.Email, request.Password);
        if (token == null)
            return Unauthorized(new { error = "Invalid email or password" });

        return Ok(new { token });
    }

    /// <summary>
    /// عرض الملف الشخصي للمستخدم الحالي (FR3).
    /// </summary>
    /// <returns>بيانات المستخدم (الاسم، البريد الإلكتروني، تاريخ التسجيل).</returns>
    /// <response code="200">تم استرداد بيانات المستخدم بنجاح.</response>
    /// <response code="401">المستخدم غير مصرح له (غير مسجل الدخول).</response>
    /// <response code="404">المستخدم غير موجود.</response>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            return Unauthorized();

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(new { user.Id, user.Name, user.Email, user.CreatedAt });
    }
}

