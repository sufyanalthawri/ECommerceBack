using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HangfireTest
{
    /// <summary>
    /// برنامج اختبار للمعالجة غير المتزامنة باستخدام Hangfire.
    /// يقوم بإرسال عدد محدد من الطلبات المتزامنة إلى واجهة إنشاء الطلب (/api/orders/checkout)
    /// ويقيس زمن الاستجابة وعدد النجاحات والفشل.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// نقطة الدخول الرئيسية للبرنامج.
        /// </summary>
        /// <param name="args">معاملات سطر الأوامر (غير مستخدمة).</param>
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Testing Asynchronous Processing (Hangfire) ===");
            Console.Write("Enter number of requests: ");
            int count = int.Parse(Console.ReadLine() ?? "10");

            var baseUrl = "https://localhost:7124";
            var client = CreateHttpClient();

            var tasks = new List<Task<HttpResponseMessage>>();
            var swTotal = Stopwatch.StartNew();
            var individualTimes = new List<long>();

            for (int i = 0; i < count; i++)
            {
                var requestBody = new { productId = 1, quantity = 1, cardNumber = $"41111111111111" };
                var requestStopwatch = Stopwatch.StartNew();

                var task = client.PostAsJsonAsync($"{baseUrl}/api/orders/checkout", requestBody)
                    .ContinueWith(t =>
                    {
                        requestStopwatch.Stop();
                        lock (individualTimes) 
                        {
                            individualTimes.Add(requestStopwatch.ElapsedMilliseconds);
                        }
                        return t.Result;
                    });
                tasks.Add(task);
            }

            var responses = await Task.WhenAll(tasks);
            swTotal.Stop();

            int successCount = responses.Count(r => r.IsSuccessStatusCode);
            int failCount = responses.Count(r => !r.IsSuccessStatusCode);

            Console.WriteLine($"\n Results:");
            Console.WriteLine($" Success: {successCount}");
            Console.WriteLine($" Failure: {failCount}");
            Console.WriteLine($" Total elapsed time: {swTotal.ElapsedMilliseconds} ms");
            Console.WriteLine($" Average response time: {(individualTimes.Count > 0 ? individualTimes.Average() : 0):F0} ms");
            Console.WriteLine($" Fastest request: {(individualTimes.Count > 0 ? individualTimes.Min() : 0)} ms");
            Console.WriteLine($" Slowest request: {(individualTimes.Count > 0 ? individualTimes.Max() : 0)} ms");
        }

     
        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            // تجاهل أخطاء شهادة SSL في بيئة التطوير المحلية فقط
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            return new HttpClient(handler);
        }
    }
}