using System.Net.Http.Json;

/// <summary>
/// برنامج اختبار التزامن (Concurrency Test) للتحقق من حماية البيانات من التضارب.
/// يقوم بإرسال 20 طلب شراء متزامن على منتج مخزونه 5 وحدات.
/// يتوقع نجاح 5 طلبات فقط وفشل الباقي بسبب نفاد المخزون أو التضارب.
/// </summary>
Console.WriteLine("Starting concurrency test (20 requests on a product with stock 5)");

/// <summary>
/// عنوان قاعدة API    يجب تعديله حسب إعدادات التطبيق
/// </summary>
var baseUrl = "https://localhost:7124";

/// <summary>
/// تكوين HttpClient لتجاهل أخطاء شهادة SSL (للتطوير المحلي فقط).
/// ملاحظة: هذا الإعداد لا يُستخدم في بيئة الإنتاج.
/// </summary>
HttpClientHandler handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
var client = new HttpClient(handler);

/// <summary>
/// قائمة بمهام HTTP غير المتزامنة لإرسال الطلبات المتزامنة.
/// </summary>
var tasks = new List<Task<HttpResponseMessage>>();

/// <summary>
/// إنشاء 20 طلب شراء متزامن (كل طلب يشتري وحدة واحدة من المنتج رقم 1).
/// </summary>
for (int i = 0; i < 20; i++)
{
    var requestBody = new
    {
        productId = 1,
        quantity = 1,
        cardNumber = $"411111111111111{i}" // رقم بطاقة مختلف لكل طلب لتمييز المعاملات
    };
    tasks.Add(client.PostAsJsonAsync($"{baseUrl}/api/orders/checkout", requestBody));
}

Console.WriteLine("Sending 20 concurrent requests...");

/// <summary>
/// انتظار اكتمال جميع الطلبات المتزامنة.
/// </summary>
var responses = await Task.WhenAll(tasks);

/// <summary>
/// حساب عدد الطلبات الناجحة  والفاشلة  .
/// </summary>
int successCount = responses.Count(r => r.IsSuccessStatusCode);
int failCount = responses.Count(r => !r.IsSuccessStatusCode);

/// <summary>
/// عرض نتائج الاختبار.
/// </summary>
Console.WriteLine($"Successful requests: {successCount}");
Console.WriteLine($"Failed requests: {failCount}");
Console.WriteLine($"Expected: 5 success / 15 failure");

/// <summary>
/// عرض تفاصيل أول 3 أخطاء للتحقق من سبب الفشل (عادةً "Insufficient stock").
/// </summary>
foreach (var resp in responses.Where(r => !r.IsSuccessStatusCode).Take(3))
{
    var errorText = await resp.Content.ReadAsStringAsync();
    Console.WriteLine($"Error: {errorText}");
}