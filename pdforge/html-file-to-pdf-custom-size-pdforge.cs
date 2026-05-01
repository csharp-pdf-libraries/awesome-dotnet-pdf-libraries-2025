// REST API — no official .NET SDK. pdforge rebranded to "pdf noodle" (pdfnoodle.com)
// in 2026. Page size / orientation / margins are passed via the optional `pdfParams`
// object; format names follow the underlying Chromium/Puppeteer convention ("A4",
// "Letter", landscape: true).
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

        var htmlContent = File.ReadAllText("input.html");

        var body = new
        {
            html = htmlContent,
            pdfParams = new
            {
                format = "A4",
                landscape = true
            }
        };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var signedUrl = doc.RootElement.GetProperty("signedUrl").GetString();

        var pdfBytes = await http.GetByteArrayAsync(signedUrl);
        File.WriteAllBytes("output.pdf", pdfBytes);
    }
}
