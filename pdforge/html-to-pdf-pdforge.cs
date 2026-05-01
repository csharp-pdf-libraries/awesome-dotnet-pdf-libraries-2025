// REST API — no official .NET SDK. pdforge rebranded to "pdf noodle" (pdfnoodle.com)
// in 2026; api.pdforge.com URLs continue to work via 301 redirects through end of 2026.
// Docs: https://docs.pdfnoodle.com/api-reference/convert-html-to-pdf/synchronous
// Integration: HttpClient + JSON POST + Bearer token.
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

        var body = new { html = "<html><body><h1>Hello World</h1></body></html>" };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        // Sync endpoint returns a JSON envelope containing a signed S3 URL to the PDF.
        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var signedUrl = doc.RootElement.GetProperty("signedUrl").GetString();

        var pdfBytes = await http.GetByteArrayAsync(signedUrl);
        File.WriteAllBytes("output.pdf", pdfBytes);
    }
}
