// REST API — no official .NET SDK; integrate via HttpClient.
// Docs: https://pdfbolt.com/docs/quick-start-guide/csharp
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var html = "<html><body><h1>Hello World</h1></body></html>";
        var base64Html = Convert.ToBase64String(Encoding.UTF8.GetBytes(html));

        var payload = JsonSerializer.Serialize(new { html = base64Html });
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("API-KEY", "YOUR-PDFBOLT-API-KEY");

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await http.PostAsync("https://api.pdfbolt.com/v1/direct", content);
        response.EnsureSuccessStatusCode();

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync("output.pdf", pdfBytes);
    }
}
