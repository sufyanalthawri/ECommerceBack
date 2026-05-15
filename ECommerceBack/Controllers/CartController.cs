using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.Core.DTOs;
using ECommerceBack.ECommerceBack.Core.Interfaces;

namespace ECommerceBack.API.Controllers;

/// <summary>
/// وحدة تحكم سلة التسوق (Cart Controller).
/// يتطلب مصادقة المستخدم (Authorize).
/// يغطي المتطلبات الوظيفية  (إضافة، تعديل، حذف، تفريغ السلة، وعرض محتوياتها).
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedAccessException("Invalid user ID");
        return userId;
    }

    /// <summary>
    /// عرض محتويات سلة المستخدم الحالي).
    /// </summary>
    /// <returns>
    /// كائن يحتوي على:
    /// - معرف السلة
    /// - قائمة العناصر (معرف المنتج، اسمه، الكمية، السعر الواحد، الإجمالي)
    /// - الإجمالي الكلي للسلة
    /// </returns>
    /// <response code="200">تم استرداد محتويات السلة بنجاح.</response>
    /// <response code="401">المستخدم غير مصرح له (غير مسجل الدخول).</response>
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();
        var cart = await _cartService.GetOrCreateCartAsync(userId);

        return Ok(new
        {
            cart.Id,
            Items = cart.CartItems.Select(ci => new
            {
                ci.Id,
                ci.ProductId,
                ProductName = ci.Product?.Name ?? "Unknown",
                ci.Quantity,
                UnitPrice = ci.Product?.Price ?? 0,
                TotalPrice = (ci.Product?.Price ?? 0) * ci.Quantity
            }),
            TotalAmount = cart.CartItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity)
        });
    }

    /// <summary>
    /// إضافة منتج إلى سلة التسوق .
    /// </summary>
    /// <param name="request">بيانات المنتج المراد إضافته (المعرف والكمية).</param>
    /// <returns>رسالة تأكيد بنجاح العملية.</returns>
    /// <response code="200">تمت إضافة المنتج بنجاح.</response>
    /// <response code="400">البيانات غير صالحة (كمية غير صحيحة، منتج غير موجود).</response>
    /// <response code="401">المستخدم غير مصرح له.</response>
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetUserId();
            await _cartService.AddToCartAsync(userId, request.ProductId, request.Quantity);
            return Ok(new { message = "Item added to cart" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        // تم إزالة catch (Exception ex) العام – سيتم التعامل معه بواسطة GlobalExceptionFilter
    }

    /// <summary>
    /// تعديل كمية منتج موجود في السلة 
    /// </summary>
    /// <param name="cartItemId">معرف عنصر السلة المراد تعديله.</param>
    /// <param name="request">الكمية الجديدة (0 للحذف).</param>
    /// <returns>رسالة تأكيد بنجاح العملية.</returns>
    /// <response code="200">تم تحديث العنصر بنجاح.</response>
    /// <response code="400">البيانات غير صالحة (كمية سالبة، عنصر غير موجود).</response>
    /// <response code="401">المستخدم غير مصرح له.</response>
    [HttpPut("item/{cartItemId}")]
    public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetUserId();
            await _cartService.UpdateCartItemQuantityAsync(userId, cartItemId, request.Quantity);
            return Ok(new { message = "Cart item updated" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        // تم إزالة catch (Exception ex) العام
    }

    /// <summary>
    /// حذف منتج معين من السلة .
    /// </summary>
    /// <param name="cartItemId">معرف عنصر السلة المراد حذفه.</param>
    /// <returns>رسالة تأكيد بنجاح الحذف.</returns>
    /// <response code="200">تم حذف العنصر بنجاح.</response>
    /// <response code="401">المستخدم غير مصرح له.</response>
    [HttpDelete("item/{cartItemId}")]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        var userId = GetUserId();
        await _cartService.RemoveFromCartAsync(userId, cartItemId);
        return Ok(new { message = "Item removed from cart" });
    }

    /// <summary>
    /// تفريغ السلة بالكامل (حذف جميع العناصر) .
    /// </summary>
    /// <returns>رسالة تأكيد بنجاح التفريغ.</returns>
    /// <response code="200">تم تفريغ السلة بنجاح.</response>
    /// <response code="401">المستخدم غير مصرح له.</response>
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        await _cartService.ClearCartAsync(userId);
        return Ok(new { message = "Cart cleared" });
    }
}
