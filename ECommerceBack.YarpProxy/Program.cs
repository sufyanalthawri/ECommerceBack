using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// تهيئة تطبيق الـ Reverse Proxy باستخدام YARP (Yet Another Reverse Proxy).
/// يستخدم هذا التطبيق كموزع أحمال (Load Balancer) لتوجيه الطلبات إلى عدة نسخ من API التجارة الإلكترونية.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// إضافة خدمة الـ Reverse Proxy وتحميل إعدادات المسارات (Routes) والمجموعات (Clusters)
/// من قسم "ReverseProxy" في ملف appsettings.json.
/// </summary>
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
       
        handler.SslOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
    });

/// <summary>
/// بناء تطبيق الـ Reverse Proxy.
/// </summary>
var app = builder.Build();

/// <summary>
/// تعيين مسار الـ Reverse Proxy لبدء توجيه الطلبات الواردة إلى الوجهات المحددة في الإعدادات.
/// </summary>
app.MapReverseProxy();

/// <summary>
/// تشغيل التطبيق والبدء في الاستماع للطلبات    .
/// </summary>
app.Run();