// Kaizen.io HTML-to-PDF is a self-hosted Docker REST API — no .NET SDK.
// At v1.x the documented JSON body only accepts an "html" field; page-size,
// orientation, headers, footers, and URL-input are listed as roadmap items
// (https://www.kaizen.io/products/html-to-pdf/). Read the local file and
// POST its contents — page layout has to be styled inside the HTML itself
// (e.g., @page { size: A4 portrait } in CSS) until the API exposes options.
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var htmlContent = File.ReadAllText("input.html");

        using var http = new HttpClient();
        var payload = JsonSerializer.Serialize(new { html = htmlContent });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await http.PostAsync("http://localhost:8080/html-to-pdf", content);
        response.EnsureSuccessStatusCode();
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        File.WriteAllBytes("document.pdf", pdfBytes);
    }
}
