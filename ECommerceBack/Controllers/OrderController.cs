using Microsoft.AspNetCore.Mvc;
using ECommerceBack.Core.DTOs;
using ECommerceBack.Core.Interfaces;
using ECommerceBack.API.Filters;

namespace ECommerceBack.API.Controllers;


/// <summary>
/// وحدة تحكم الطلبات (Orders Controller).
/// يستخدم نمط Façade (ICheckoutFacade) لتبسيط عملية الشراء المعقدة.
/// لأن خدمة OrderService تدير معاملاتها الداخلية بنفسها.
/// يغطي المتطلبات الوظيفية (إنشاء الطلب، الدفع، عرض الطلبات).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[SkipTransactionFilter]

public class OrdersController : ControllerBase
{
    private readonly ICheckoutFacade _checkoutFacade;
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ICheckoutFacade checkoutFacade, IOrderService orderService, ILogger<OrdersController> logger)
    {
        _checkoutFacade = checkoutFacade;
        _orderService = orderService;
        _logger = logger;
    }

    private int GetUserId() => 1;

    /// <summary>
    /// إنشاء طلب شراء جديد (Checkout).
    /// </summary>
    /// <remarks>
    /// هذه العملية تستخدم ICheckoutFacade التي تجمع:
    /// - إضافة المنتج إلى السلة
    /// - إنشاء الطلب (تحديث المخزون، معالجة الدفع)
    /// - جدولة المهام غير المتزامنة (بريد إلكتروني، فاتورة، تحديث إحصائيات)
    /// - تفريغ السلة
    /// </remarks>
    /// <param name="request">بيانات الطلب: معرف المنتج، الكمية، رقم البطاقة.</param>
    /// <returns>كائن يحتوي على معرف الطلب وحالته (Paid/Pending).</returns>
    /// <response code="200">تم إنشاء الطلب بنجاح.</response>
    /// <response code="400">بيانات الطلب غير صالحة (نقص المخزون، منتج غير موجود، إلخ).</response>
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var order = await _checkoutFacade.PlaceOrderAsync(GetUserId(), request.ProductId, request.Quantity, request.CardNumber);
            return Ok(new { orderId = order.Id, status = order.Status.ToString() });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>
    /// الحصول على قائمة طلبات المستخدم الحالي.
    /// </summary>
    /// <returns>قائمة تحتوي على طلبات المستخدم (مرتبة من الأحدث إلى الأقدم).</returns>
    /// <response code="200">تم استرداد الطلبات بنجاح.</response>
    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var orders = await _orderService.GetUserOrdersAsync(GetUserId());
        return Ok(orders);
    }

    /// <summary>
    /// الحصول على تفاصيل طلب محدد.
    /// </summary>
    /// <param name="orderId">معرف الطلب.</param>
    /// <returns>كائن Order يحتوي على عناصر الطلب ومعاملة الدفع.</returns>
    /// <response code="200">تم العثور على الطلب وإرجاعه.</response>
    /// <response code="404">الطلب غير موجود أو لا يخص المستخدم الحالي.</response>
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderById(int orderId)
    {
        var order = await _orderService.GetOrderDetailsAsync(orderId, GetUserId());
        if (order == null) return NotFound();
        return Ok(order);
    }
}




