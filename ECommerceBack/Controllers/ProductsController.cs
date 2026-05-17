using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceBack.Core.Entities;
using ECommerceBack.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using ECommerceBack.Infrastructure.Data;
using ECommerceBack.Core.DTOs;

namespace ECommerceBack.API.Controllers;

/// <summary>
/// وحدة تحكم المنتجات (Products Controller).
/// توفر نقاط نهاية لعرض وإدارة المنتجات.
/// يستخدم Cursor Pagination لعرض المنتجات على دفعات لتجنب تحميل كميات كبيرة من البيانات.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly AppDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, AppDbContext context, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// الحصول على قائمة المنتجات مع دعم Pagination (Cursor-based) – تطبيق لمعالجة الدفعات (Batch Processing).
    /// </summary>
    /// <param name="cursor">آخر Id تم استلامه (يستخدم للصفحة التالية، القيمة الافتراضية 0 تعني الصفحة الأولى).</param>
    /// <param name="limit">عدد المنتجات في الدفعة الواحدة (الحد الأقصى 100، القيمة الافتراضية 20).</param>
    /// <returns>
    /// كائن يحتوي على:
    /// - Data: قائمة المنتجات في الدفعة الحالية
    /// - NextCursor: آخر Id في الدفعة الحالية (للاستخدام في الصفحة التالية)
    /// - HasMore: يشير إلى وجود منتجات إضافية (true/false)
    /// </returns>
    /// <response code="200">تم استرداد الدفعة بنجاح.</response>
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] int cursor = 0, [FromQuery] int limit = 20)
    {
        if (limit > 100) limit = 100;
        if (limit < 1) limit = 1;

        var query = _context.Products
            .Where(p => p.Id > cursor)
            .OrderBy(p => p.Id);

        var products = await query.Take(limit).ToListAsync();
        var nextCursor = products.LastOrDefault()?.Id ?? cursor;
        var hasMore = products.Count == limit;

        return Ok(new { Data = products, NextCursor = nextCursor, HasMore = hasMore });
    }

    /// <summary>
    /// عرض تفاصيل منتج محدد باستخدام المعرف.
    /// </summary>
    /// <param name="id">معرف المنتج.</param>
    /// <returns>كائن المنتج إذا وُجد، وإلا 404 Not Found.</returns>
    /// <response code="200">تم العثور على المنتج وإرجاعه.</response>
    /// <response code="404">المنتج غير موجود.</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    /// <summary>
    /// إضافة منتج جديد إلى النظام. يتطلب صلاحيات المسؤول (Authorize).
    /// </summary>
    /// <param name="request">بيانات المنتج الجديد (الاسم، السعر، الكمية).</param>
    /// <returns>المنتج المُضاف مع رابط التفاصيل في رأس الـ Location.</returns>
    /// <response code="201">تم إنشاء المنتج بنجاح.</response>
    /// <response code="400">البيانات غير صالحة.</response>
    /// <response code="401">المستخدم غير مصرح له.</response>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock,
            LastUpdated = DateTime.UtcNow
        };

        var created = await _productService.CreateProductAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// تعديل بيانات منتج موجود. يتطلب صلاحيات المسؤول (Authorize).
    /// </summary>
    /// <param name="id">معرف المنتج المراد تعديله.</param>
    /// <param name="request">البيانات الجديدة للمنتج.</param>
    /// <returns>المنتج بعد التعديل.</returns>
    /// <response code="200">تم التعديل بنجاح.</response>
    /// <response code="400">البيانات غير صالحة.</response>
    /// <response code="401">المستخدم غير مصرح له.</response>
    /// <response code="404">المنتج غير موجود.</response>
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _productService.GetProductByIdAsync(id);
        if (existing == null) return NotFound();

        existing.Name = request.Name;
        existing.Price = request.Price;
        existing.Stock = request.Stock;
        existing.LastUpdated = DateTime.UtcNow;

        await _productService.UpdateProductAsync(existing);
        return Ok(existing);
    }

    /// <summary>
    /// حذف منتج من النظام. يتطلب صلاحيات المسؤول (Authorize).
    /// </summary>
    /// <param name="id">معرف المنتج المراد حذفه.</param>
    /// <returns>204 No Content إذا تم الحذف بنجاح.</returns>
    /// <response code="204">تم الحذف بنجاح.</response>
    /// <response code="401">المستخدم غير مصرح له.</response>
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }
}