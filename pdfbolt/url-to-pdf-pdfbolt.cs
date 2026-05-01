// REST API — no official .NET SDK; integrate via HttpClient.
// Docs: https://pdfbolt.com/docs/quick-start-guide/csharp
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var payload = JsonSerializer.Serialize(new { url = "https://www.example.com" });

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("API-KEY", "YOUR-PDFBOLT-API-KEY");

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await http.PostAsync("https://api.pdfbolt.com/v1/direct", content);
        response.EnsureSuccessStatusCode();

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync("webpage.pdf", pdfBytes);
    }
}
