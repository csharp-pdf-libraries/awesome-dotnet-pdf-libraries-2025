// REST API — no official .NET SDK. pdforge rebranded to "pdf noodle" (pdfnoodle.com)
// in 2026. The HTML-to-PDF endpoint accepts only an `html` payload — there is no
// dedicated URL-to-PDF endpoint, so URL rendering is done by fetching the page first
// and submitting its HTML, OR by passing an iframe/<a> referencing the URL.
// Docs: https://docs.pdfnoodle.com/api-reference/convert-html-to-pdf/synchronous
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "pdfnoodle_api_YOUR_KEY");

        // Fetch source page HTML, then submit it to pdf noodle for PDF rendering.
        var sourceHtml = await http.GetStringAsync("https://example.com");

        var body = new { html = sourceHtml };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var signedUrl = doc.RootElement.GetProperty("signedUrl").GetString();

        var pdfBytes = await http.GetByteArrayAsync(signedUrl);
        File.WriteAllBytes("webpage.pdf", pdfBytes);
    }
}
