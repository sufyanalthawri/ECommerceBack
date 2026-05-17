using System.Net.Http.Json;



var baseUrl = "https://localhost:7124";
var client = new HttpClient();

var handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
client = new HttpClient(handler);

var tasks = new List<Task<HttpResponseMessage>>();
for (int i = 0; i < 11; i++)
{
    var requestBody = new { productId = 1, quantity = 1, cardNumber = "4111111111111111" };
    tasks.Add(client.PostAsJsonAsync($"{baseUrl}/api/orders/checkout", requestBody));
}

Console.WriteLine("Sending 20 concurrent requests...");
var responses = await Task.WhenAll(tasks);

int success = responses.Count(r => r.IsSuccessStatusCode);
int badRequest = responses.Count(r => r.StatusCode == System.Net.HttpStatusCode.BadRequest);
int conflict = responses.Count(r => (int)r.StatusCode == 409);
int other = responses.Count(r => !r.IsSuccessStatusCode && r.StatusCode != System.Net.HttpStatusCode.BadRequest && (int)r.StatusCode != 409);

Console.WriteLine($"Success (200): {success}");
Console.WriteLine($"BadRequest (400): {badRequest}");
Console.WriteLine($"Conflict (409): {conflict}");
Console.WriteLine($"Other errors: {other}");
Console.WriteLine("Expected: 5 success, 15 BadRequest (Insufficient stock)");

foreach (var resp in responses.Where(r => !r.IsSuccessStatusCode).Take(3))
{
    var errorText = await resp.Content.ReadAsStringAsync();
    Console.WriteLine($"Error sample: {errorText}");
}