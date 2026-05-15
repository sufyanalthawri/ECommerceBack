using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HangfireTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== اختبار المعالجة غير المتزامنة (Hangfire) ===");
            Console.Write("أدخل عدد الطلبات: ");
            int count = int.Parse(Console.ReadLine() ?? "10");

            var baseUrl = "https://localhost:7124";
            var client = CreateHttpClient();

            var tasks = new List<Task<HttpResponseMessage>>();
            var swTotal = Stopwatch.StartNew();
            var individualTimes = new List<long>();

            for (int i = 0; i < count; i++)
            {
                var requestBody = new { productId = 1, quantity = 1, cardNumber = "4111111111111111" };
                var requestStopwatch = Stopwatch.StartNew();

                var task = client.PostAsJsonAsync($"{baseUrl}/api/orders/checkout", requestBody)
                    .ContinueWith(t =>
                    {
                        requestStopwatch.Stop();
                        individualTimes.Add(requestStopwatch.ElapsedMilliseconds);
                        return t.Result;
                    });
                tasks.Add(task);
            }

            var responses = await Task.WhenAll(tasks);
            swTotal.Stop();

            int successCount = responses.Count(r => r.IsSuccessStatusCode);
            int failCount = responses.Count(r => !r.IsSuccessStatusCode);

            Console.WriteLine($"\n📊 النتائج:");
            Console.WriteLine($"✅ النجاح: {successCount}");
            Console.WriteLine($"❌ الفشل: {failCount}");
            Console.WriteLine($"⏱️ إجمالي الزمن الكلي: {swTotal.ElapsedMilliseconds} ms");
            Console.WriteLine($"📈 متوسط زمن الاستجابة: {(individualTimes.Count > 0 ? individualTimes.Average() : 0):F0} ms");
            Console.WriteLine($"⚡ أسرع طلب: {(individualTimes.Count > 0 ? individualTimes.Min() : 0)} ms");
            Console.WriteLine($"🐢 أبطأ طلب: {(individualTimes.Count > 0 ? individualTimes.Max() : 0)} ms");
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            // تجاهل أخطاء شهادة SSL في بيئة التطوير المحلي
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            return new HttpClient(handler);
        }
    }
}