// Kaizen.io HTML-to-PDF (Docker REST API) does not expose URL-input,
// header, footer, or margin options at v1.x — the documented JSON body
// only accepts "html" (https://www.kaizen.io/products/html-to-pdf/).
// Workaround: fetch the URL yourself, then inline a header/footer into
// the markup with @page CSS for margins. Page numbers/totals are not
// supported by the API. Once Kaizen ships URL-to-PDF and template
// fields, switch to those instead of this client-side fetch.
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var http = new HttpClient();

        // 1. Fetch the page locally because Kaizen has no ConvertUrl.
        var pageHtml = await http.GetStringAsync("https://example.com");

        // 2. Inject our own header/footer + margins via CSS.
        var wrapped = $@"<!doctype html>
<html><head><style>
  @page {{ margin: 20mm; }}
  .k-header {{ position: fixed; top: -15mm; left: 0; right: 0; text-align: center; }}
  .k-footer {{ position: fixed; bottom: -15mm; left: 0; right: 0; text-align: center; }}
</style></head><body>
  <div class='k-header'>Company Header</div>
  <div class='k-footer'>Footer (page numbers unsupported by Kaizen v1.x)</div>
  {pageHtml}
</body></html>";

        var payload = JsonSerializer.Serialize(new { html = wrapped });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await http.PostAsync("http://localhost:8080/html-to-pdf", content);
        response.EnsureSuccessStatusCode();
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        File.WriteAllBytes("webpage.pdf", pdfBytes);
    }
}
