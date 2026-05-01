# How Do I Migrate from pdforge to IronPDF in C#?

> **Naming note:** pdforge rebranded to "pdf noodle" (pdfnoodle.com) in 2026. The
> `api.pdforge.com` hostname continues to work via 301 redirects through end of
> 2026. This guide uses `api.pdfnoodle.com` for new code and refers to the
> service as "pdforge" for continuity with existing integrations. There is no
> official .NET SDK on NuGet — integration is `HttpClient` against the
> documented REST endpoints.

## Why Migrate from pdforge?

pdforge is a cloud-based PDF generation API that processes your documents on external servers. While this simplifies initial setup, it introduces significant limitations for production applications:

### The Cloud API Dependency Problem

1. **External Server Processing**: Every PDF you generate requires sending your HTML/data to pdforge's servers—your documents leave your infrastructure
2. **Privacy & Compliance Risks**: Sensitive data (contracts, financial reports, personal information) travels over the internet to third-party servers
3. **Ongoing Subscription Costs**: Monthly fees accumulate indefinitely with no asset ownership
4. **Internet Dependency**: No PDF generation when network is unavailable
5. **Rate Limits**: API usage caps can throttle high-volume applications
6. **Network Latency**: Round-trip time adds seconds to every PDF generation
7. **Vendor Lock-in**: Your application depends on pdforge's service availability and API stability

### Why [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/)?

IronPDF processes everything locally within your application:

- **Complete Data Privacy**: Documents never leave your server
- **One-Time Licensing**: Perpetual license eliminates recurring costs
- **Offline Capability**: Works without internet after initial setup
- **No Rate Limits**: Generate unlimited PDFs
- **Lower Latency**: No network round-trips
- **Full Control**: You own the processing environment
- **Modern Rendering**: Chromium-based engine with full CSS3/JS support
- **Rich Feature Set**: PDF manipulation, merging, text extraction, security

---

## Quick Migration Overview

| Aspect | pdforge | IronPDF |
|--------|---------|---------|
| Processing Location | External cloud servers | Local (your server) |
| Authentication | API key per request | One-time license key |
| Network Required | Every generation | Only initial setup |
| Pricing Model | Monthly subscription | Perpetual license available |
| Rate Limits | Yes (plan-dependent) | None |
| Data Privacy | Data sent externally | Data stays local with html to pdf c# |
| Offline Support | No | Yes |
| PDF Manipulation | Limited | Full suite (merge, split, edit) |
| Text Extraction | No | Yes |
| Async Pattern | Required | Optional (sync/async) |

For in-depth technical comparisons, visit the [analysis article](https://ironsoftware.com/suite/blog/comparison/compare-pdforge-vs-ironpdf/).

---

## NuGet Package Changes

```bash
# pdforge has no official .NET SDK on NuGet — integration is HttpClient.
# If your project depends only on built-in System.Net.Http, there is no
# competitor package to remove. Just install IronPDF:
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// Before: pdforge — raw HttpClient against api.pdfnoodle.com
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

// After: IronPDF
using IronPdf;
using IronPdf.Rendering;
```

---

## API Mapping Reference

### Core Concepts

| pdforge (REST) | IronPDF | Notes |
|---------|---------|-------|
| `HttpClient` + `Authorization: Bearer` | `ChromePdfRenderer` | Main PDF generator |
| JSON body `{ html, pdfParams }` | `ChromePdfRenderOptions` | HTML conversion config |
| Same JSON body (no dedicated URL endpoint) | `RenderUrlAsPdf(url)` | URL conversion path |
| HTTP POST → JSON envelope with `signedUrl` | `PdfDocument` | Result object |

### Operations

| pdforge | IronPDF | Notes |
|---------|---------|-------|
| `POST /v1/html-to-pdf/sync` with `{ html }` | `renderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| Fetch URL, then POST its HTML | `renderer.RenderUrlAsPdf(url)` | URL to PDF |
| `POST` with HTML string body | `renderer.RenderHtmlAsPdf(html)` | HTML string |
| Fetch URL first, then POST | `renderer.RenderUrlAsPdf(url)` | URL conversion |
| `File.ReadAllText(path)` then POST | `renderer.RenderHtmlFileAsPdf(path)` | HTML file |
| Download `signedUrl` then `File.WriteAllBytes` | `pdf.SaveAs(path)` | Save to disk |
| `await http.GetByteArrayAsync(signedUrl)` | `pdf.BinaryData` | Get raw bytes |
| Stream from signed URL | `pdf.Stream` | Get as stream |

### Configuration Options (pdfParams → RenderingOptions)

| pdforge `pdfParams` | IronPDF (RenderingOptions) | Notes |
|---------|---------------------------|-------|
| `format: "A4"` | `.PaperSize = PdfPaperSize.A4` | Paper size |
| `format: "Letter"` | `.PaperSize = PdfPaperSize.Letter` | US Letter |
| `landscape: true` | `.PaperOrientation = PdfPaperOrientation.Landscape` | Orientation |
| `landscape: false` | `.PaperOrientation = PdfPaperOrientation.Portrait` | Portrait |
| `margin: { top: "20px" }` | `.MarginTop = 20` | Top margin |
| `margin: { bottom: "20px" }` | `.MarginBottom = 20` | Bottom margin |
| `margin: { left: "15px" }` | `.MarginLeft = 15` | Left margin |
| `margin: { right: "15px" }` | `.MarginRight = 15` | Right margin |
| `headerTemplate: "<div>...</div>"` | `.HtmlHeader = new HtmlHeaderFooter { HtmlFragment = "..." }` | HTML header |
| `footerTemplate: "<div>...</div>"` | `.HtmlFooter = new HtmlHeaderFooter { HtmlFragment = "..." }` | HTML footer |
| `displayHeaderFooter: true` | Implicit when `HtmlHeader`/`HtmlFooter` set | Header/footer visibility |
| `printBackground: true` | `.PrintHtmlBackgrounds = true` | Background rendering |
| `width` / `height` | `.SetCustomPaperSizeInInches(...)` | Custom page size |

### Authentication Comparison

| pdforge | IronPDF |
|---------|---------|
| `Authorization: Bearer pdfnoodle_api_...` per request | `IronPdf.License.LicenseKey = "YOUR-KEY"` |
| Per-request authentication | One-time at startup |
| API key in HTTP header | Global property |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (pdforge):**
```csharp
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
        // Bearer token sent on every request
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "pdfnoodle_api_YOUR_KEY");

        var body = new { html = "<html><body><h1>Hello World</h1><p>PDF content</p></body></html>" };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        // Data sent to external servers
        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();

        // Two-step: response gives a signed URL; fetch the actual bytes from it
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var signedUrl = doc.RootElement.GetProperty("signedUrl").GetString();
        var pdfBytes = await http.GetByteArrayAsync(signedUrl);
        File.WriteAllBytes("output.pdf", pdfBytes);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        // One-time license configuration
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // All processing happens locally
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Hello World</h1><p>PDF content</p></body></html>");
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 2: URL to PDF with Options

**Before (pdforge):**
```csharp
// pdforge has no dedicated URL-to-PDF endpoint. The pattern is to fetch the
// page yourself and post its HTML to /v1/html-to-pdf/sync.
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

        var sourceHtml = await http.GetStringAsync("https://example.com");

        var body = new
        {
            html = sourceHtml,
            pdfParams = new
            {
                format = "A4",
                landscape = true,
                margin = new { top = "20px", bottom = "20px", left = "15px", right = "15px" }
            }
        };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var signedUrl = doc.RootElement.GetProperty("signedUrl").GetString();
        var pdfBytes = await http.GetByteArrayAsync(signedUrl);
        File.WriteAllBytes("webpage.pdf", pdfBytes);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.MarginLeft = 15;
        renderer.RenderingOptions.MarginRight = 15;

        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

### Example 3: Headers and Footers with Page Numbers

**Before (pdforge):**
```csharp
// pdforge uses Chromium-style header/footer templates: HTML fragments containing
// <span class="pageNumber"> / <span class="totalPages"> elements that are
// substituted at render time. There are no plain-text {page}/{totalPages}
// placeholders — only the templated HTML form, which requires
// displayHeaderFooter: true.
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

        var body = new
        {
            html = "<h1>Report</h1><p>Content here...</p>",
            pdfParams = new
            {
                displayHeaderFooter = true,
                headerTemplate = "<div style='font-size:10px;width:100%;text-align:center;'>Company Report</div>",
                footerTemplate = "<div style='font-size:10px;width:100%;text-align:center;'>" +
                                 "Page <span class=\"pageNumber\"></span> of <span class=\"totalPages\"></span></div>",
                margin = new { top = "60px", bottom = "60px" }
            }
        };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
        File.WriteAllBytes("report.pdf", pdfBytes);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        renderer.RenderingOptions.TextHeader = new TextHeaderFooter
        {
            CenterText = "Company Report",
            DrawDividerLine = true,
            FontSize = 12
        };

        renderer.RenderingOptions.TextFooter = new TextHeaderFooter
        {
            CenterText = "Page {page} of {total-pages}",  // Note: {total-pages} with hyphen
            DrawDividerLine = true
        };

        var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1><p>Content here...</p>");
        pdf.SaveAs("report.pdf");
    }
}
```

### Example 4: HTML Headers and Footers

**Before (pdforge):**
```csharp
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

        var body = new
        {
            html = "<h1>Report Content</h1>",
            pdfParams = new
            {
                displayHeaderFooter = true,
                headerTemplate = "<div style='text-align:center;'><img src='logo.png'/> Company Name</div>",
                footerTemplate = "<div style='text-align:center;'>Confidential - Page <span class=\"pageNumber\"></span></div>",
                margin = new { top = "50px", bottom = "30px" }
            }
        };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
        File.WriteAllBytes("branded-report.pdf", pdfBytes);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;'><img src='logo.png'/> Company Name</div>",
            MaxHeight = 50
        };

        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;'>Confidential - Page {page}</div>",
            MaxHeight = 30
        };

        var pdf = renderer.RenderHtmlAsPdf("<h1>Report Content</h1>");
        pdf.SaveAs("branded-report.pdf");
    }
}
```

For a complete breakdown of API authentication removal strategies and offline deployment patterns, refer to the [step-by-step guide](https://ironpdf.com/blog/migration-guides/migrate-from-pdforge-to-ironpdf/).

### Example 5: Async Web Application

**Before (pdforge):**
```csharp
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ReportController : Controller
{
    private readonly HttpClient _http;

    public ReportController(IHttpClientFactory factory)
    {
        _http = factory.CreateClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "pdfnoodle_api_YOUR_KEY");
    }

    [HttpGet]
    public async Task<IActionResult> GenerateReport()
    {
        var body = new { html = "<h1>Sales Report</h1><p>Q4 Results</p>" };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        try
        {
            // Async call to external API (network latency, possible rate-limit response)
            var resp = await _http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var pdfBytes = await _http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
            return File(pdfBytes, "application/pdf", "report.pdf");
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"PDF generation failed: {ex.Message}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class ReportController : Controller
{
    public ReportController()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    }

    [HttpGet]
    public async Task<IActionResult> GenerateReport()
    {
        try
        {
            // Local processing - wrap in Task.Run for async if needed
            var pdf = await Task.Run(() =>
            {
                var renderer = new ChromePdfRenderer();
                return renderer.RenderHtmlAsPdf("<h1>Sales Report</h1><p>Q4 Results</p>");
            });

            return File(pdf.BinaryData, "application/pdf", "report.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"PDF generation failed: {ex.Message}");
        }
    }
}
```

### Example 6: JavaScript Wait for Dynamic Content

**Before (pdforge):**
```csharp
// pdforge runs the underlying Chromium engine with JavaScript enabled by
// default. There is no public `pdfParams` field for an explicit script delay
// in the documented options — the recommended approach is to use a
// network-idle / DOMContentLoaded signal in your page (e.g., a sentinel
// element your script writes when ready). Below sends the page as-is and
// relies on the engine's own load detection.
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

        var body = new
        {
            html = @"
                <div id='chart'>Loading...</div>
                <script src='https://cdn.example.com/chart.js'></script>
                <script>renderChart('chart');</script>
            "
        };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
        File.WriteAllBytes("chart.pdf", pdfBytes);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // JavaScript is enabled by default in IronPDF
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.RenderDelay = 3000;  // Wait 3 seconds for JS

        string html = @"
            <div id='chart'>Loading...</div>
            <script src='https://cdn.example.com/chart.js'></script>
            <script>renderChart('chart');</script>
        ";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("chart.pdf");
    }
}
```

### Example 7: Custom Page Size

**Before (pdforge):**
```csharp
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

        var body = new
        {
            html = "<h1>Custom Size Document</h1>",
            pdfParams = new
            {
                width = "5in",
                height = "7in"
            }
        };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
        File.WriteAllBytes("custom.pdf", pdfBytes);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Set custom paper size in inches
        renderer.RenderingOptions.SetCustomPaperSizeInInches(5, 7);

        // Or in millimeters
        // renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(127, 178);

        var pdf = renderer.RenderHtmlAsPdf("<h1>Custom Size Document</h1>");
        pdf.SaveAs("custom.pdf");
    }
}
```

### Example 8: Password-Protected PDF

**Before (pdforge):**
```csharp
// pdforge / pdf noodle does not expose password protection or encryption
// in its documented HTML-to-PDF parameters. The typical workaround is to
// download the generated PDF, then encrypt it locally with a separate
// library (e.g., qpdf, iText, or IronPDF). The fetch step is shown below;
// the encryption step is your responsibility.
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

        var body = new { html = "<h1>Confidential Document</h1>" };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
        File.WriteAllBytes("secure.pdf", pdfBytes);

        // Encrypt secure.pdf with a third-party tool here — pdforge has no
        // built-in password / owner-password parameter.
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential Document</h1>");

        // Apply security after rendering
        pdf.SecuritySettings.UserPassword = "secret123";
        pdf.SecuritySettings.OwnerPassword = "admin456";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;

        pdf.SaveAs("secure.pdf");
    }
}
```

### Example 9: Batch Processing Multiple Files

**Before (pdforge):**
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

        var documents = new List<string>
        {
            "<h1>Document 1</h1>",
            "<h1>Document 2</h1>",
            "<h1>Document 3</h1>"
        };

        // Each document is one POST. Plan rate limits apply (Starter: 60 rpm,
        // Business: 120 rpm, Scale: 240 rpm) — handle 429 responses explicitly.
        for (int i = 0; i < documents.Count; i++)
        {
            var body = new { html = documents[i] };
            var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);

            if (resp.StatusCode == (HttpStatusCode)429)
            {
                Console.WriteLine("Rate limited. Waiting...");
                await Task.Delay(60000);
                i--; // Retry
                continue;
            }
            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
            File.WriteAllBytes($"doc_{i + 1}.pdf", pdfBytes);
            Console.WriteLine($"Generated doc_{i + 1}.pdf");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var documents = new List<string>
        {
            "<h1>Document 1</h1>",
            "<h1>Document 2</h1>",
            "<h1>Document 3</h1>"
        };

        var renderer = new ChromePdfRenderer();
        var allPdfs = new List<PdfDocument>();

        // No rate limits - c# html to pdf process as fast as your hardware allows
        for (int i = 0; i < documents.Count; i++)
        {
            var pdf = renderer.RenderHtmlAsPdf(documents[i]);
            pdf.SaveAs($"doc_{i + 1}.pdf");
            allPdfs.Add(pdf);
            Console.WriteLine($"Generated doc_{i + 1}.pdf");
        }

        // Bonus: Merge all documents (not available in pdforge)
        var merged = PdfDocument.Merge(allPdfs);
        merged.SaveAs("all_documents.pdf");
        Console.WriteLine("Merged all documents into all_documents.pdf");
    }
}
```

### Example 10: Error Handling Comparison

**Before (pdforge):**
```csharp
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "pdfnoodle_api_YOUR_KEY");

        var body = new { html = "<h1>Test</h1>" };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        try
        {
            var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);

            // Status-based error handling — there are no SDK exceptions.
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
                Console.WriteLine("Invalid API key");
            else if (resp.StatusCode == (HttpStatusCode)429)
                Console.WriteLine("Rate limit exceeded");
            else if (!resp.IsSuccessStatusCode)
                Console.WriteLine($"API error: {(int)resp.StatusCode} {resp.ReasonPhrase}");
            else
            {
                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
                File.WriteAllBytes("output.pdf", pdfBytes);
            }
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Request timed out");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error: {ex.Message}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        try
        {
            IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf("<h1>Test</h1>");
            pdf.SaveAs("output.pdf");
        }
        catch (IronPdf.Exceptions.IronPdfLicenseException ex)
        {
            Console.WriteLine($"License error: {ex.Message}");
            // Invalid or expired license
        }
        catch (IronPdf.Exceptions.IronPdfRenderingException ex)
        {
            Console.WriteLine($"Rendering error: {ex.Message}");
            // HTML parsing or rendering issue
        }
        catch (System.IO.IOException ex)
        {
            Console.WriteLine($"File I/O error: {ex.Message}");
            // File system access issue
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
```

---

## Placeholder Migration

pdforge inherits Chromium's header/footer template format: HTML fragments
containing `<span class="..."></span>` elements that the rendering engine
substitutes at print time. IronPDF uses bracketed placeholders that are
swapped into either a `TextHeaderFooter.CenterText` or an `HtmlHeaderFooter.HtmlFragment`.

| pdforge (Chromium template) | IronPDF Placeholder | Description |
|--------------------|---------------------|-------------|
| `<span class="pageNumber"></span>` | `{page}` | Current page |
| `<span class="totalPages"></span>` | `{total-pages}` | Total pages |
| `<span class="date"></span>` | `{date}` | Current date |
| (no built-in time class) | `{time}` | Current time |
| `<span class="title"></span>` | `{html-title}` | HTML title |
| `<span class="url"></span>` | `{url}` | Source URL |

**Migration Example:**
```csharp
// Before (pdforge): Chromium-style HTML template inside pdfParams.footerTemplate
// "<div>Page <span class=\"pageNumber\"></span> of <span class=\"totalPages\"></span></div>"

// After (IronPDF)
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Page {page} of {total-pages}"
};
```

---

## Features Not Available in pdforge

IronPDF provides many capabilities that pdforge cannot offer:

### PDF Merging and Splitting

```csharp
// Merge multiple PDFs
var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("combined.pdf");

// Split a PDF
var firstChapter = pdf.CopyPages(0, 9);
firstChapter.SaveAs("chapter1.pdf");

// Add pages from another PDF
pdf.AppendPdf(anotherPdf);

// Remove pages
pdf.RemovePages(5);
```

### Text Extraction

```csharp
// Extract all text
string text = pdf.ExtractAllText();

// Extract text from specific page
string pageText = pdf.Pages[0].Text;

// Search for text
var results = pdf.ExtractAllText().Contains("keyword");
```

### Watermarks

```csharp
// Add text watermark
pdf.ApplyWatermark("<h2 style='color:red;opacity:0.5;'>CONFIDENTIAL</h2>",
    30,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);

// Add image watermark
pdf.ApplyWatermark("<img src='watermark.png' style='opacity:0.3;' />",
    50,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);
```

### Stamps and Annotations

```csharp
// Add HTML stamp
var stamp = new HtmlStamp
{
    Html = "<img src='approved.png' />",
    Width = 100,
    Height = 50,
    HorizontalAlignment = HorizontalAlignment.Right,
    VerticalAlignment = VerticalAlignment.Top
};
pdf.ApplyStamp(stamp, 0);  // Apply to first page
```

### Form Handling

```csharp
// Fill form fields
pdf.Form.GetFieldByName("CustomerName").Value = "John Doe";
pdf.Form.GetFieldByName("Date").Value = DateTime.Now.ToShortDateString();

// Get all form fields
foreach (var field in pdf.Form.Fields)
{
    Console.WriteLine($"{field.Name}: {field.Value}");
}

// Flatten forms (make non-editable)
pdf.Form.Flatten();
```

### Digital Signatures

```csharp
// Sign PDF with certificate
var signature = new PdfSignature("certificate.pfx", "password")
{
    SigningContact = "support@company.com",
    SigningReason = "Document Approval",
    SigningLocation = "New York"
};
pdf.Sign(signature);
```

### Page Manipulation

```csharp
// Rotate pages
pdf.RotateAllPages(PdfPageRotation.Rotate90);
pdf.RotatePage(0, PdfPageRotation.Rotate180);

// Get page dimensions
var page = pdf.Pages[0];
Console.WriteLine($"Size: {page.Width} x {page.Height}");
```

---

## Common Migration Issues

### 1. Async to Sync Conversion

```csharp
// pdforge: HTTP call is always async
var resp = await http.PostAsync(".../v1/html-to-pdf/sync", json);

// IronPDF: Sync by default
var pdf = renderer.RenderHtmlAsPdf(html);

// IronPDF: Async when needed
var pdf = await Task.Run(() => renderer.RenderHtmlAsPdf(html));
```

### 2. Return Type Changes

```csharp
// pdforge: Two-step — JSON envelope, then fetch bytes from signedUrl
var resp = await http.PostAsync(".../v1/html-to-pdf/sync", json);
using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
File.WriteAllBytes("output.pdf", pdfBytes);

// IronPDF: Returns PdfDocument directly
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");           // Direct save
byte[] bytes = pdf.BinaryData;      // Get bytes
Stream stream = pdf.Stream;         // Get stream
```

### 3. License Configuration

```csharp
// pdforge: API key sent on every request
http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", "pdfnoodle_api_YOUR_KEY");

// IronPDF: One-time at startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Set once in Program.cs or Startup.cs
```

### 4. Page Size Configuration

```csharp
// pdforge: Field on JSON pdfParams object
var body = new { html, pdfParams = new { format = "A4" } };

// IronPDF: Property on RenderingOptions
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
```

### 5. Network Error Handling Removed

```csharp
// pdforge: Handle HTTP status codes and network errors
if (resp.StatusCode == (HttpStatusCode)429) { /* wait and retry */ }
catch (TaskCanceledException) { /* timeout retry */ }

// IronPDF: No network errors possible
// Just handle local exceptions
catch (IronPdf.Exceptions.IronPdfRenderingException) { /* fix HTML */ }
```

---

## Server Deployment

### pdforge
- No server configuration needed (cloud API)
- Requires outbound internet access on every call
- Subject to API rate limits

### IronPDF
- First run downloads Chromium (~150MB one-time)
- Works completely offline after initial setup
- No rate limits

#### Linux Dependencies

```bash
# Ubuntu/Debian
apt-get update && apt-get install -y \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2

# Docker (add to Dockerfile)
RUN apt-get update && apt-get install -y \
    libnss3 libatk1.0-0 libatk-bridge2.0-0 libdrm2 \
    libxkbcommon0 libxcomposite1 libxdamage1 libxfixes3 \
    libxrandr2 libgbm1 libasound2
```

---

## Pre-Migration Checklist

- [ ] Inventory all pdforge API calls in your codebase
- [ ] Document current configuration options used
- [ ] Identify Chromium-style header/footer templates to convert to IronPDF placeholders
- [ ] Note async patterns that need adjustment
- [ ] Plan IronPDF license key storage (environment variables recommended)
- [ ] Test with IronPDF trial license first
- [ ] Check server environment for Linux dependencies if applicable

## Post-Migration Checklist

- [ ] Remove HttpClient calls to `api.pdfnoodle.com` / `api.pdforge.com`
- [ ] Replace `System.Net.Http` imports with `IronPdf` / `IronPdf.Rendering`
- [ ] Replace Bearer token with IronPDF license key (set once at startup)
- [ ] Convert JSON `pdfParams` fields to RenderingOptions properties
- [ ] Convert Chromium templates (`<span class="totalPages">`) to IronPDF placeholders (`{total-pages}`)
- [ ] Convert async patterns if using sync methods
- [ ] Update error handling (remove HTTP status checks, add IronPDF exception types)
- [ ] Test PDF output quality
- [ ] Verify offline operation works
- [ ] Remove API credentials from configuration
- [ ] Install Linux dependencies if deploying to Linux

---

## Find All pdforge References

```bash
# Find pdforge / pdf noodle endpoint usage
grep -r "api\.pdforge\.com\|api\.pdfnoodle\.com" --include="*.cs" .

# Find API key / Bearer token references
grep -r "pdfnoodle_api_\|pdforge_api_" --include="*.cs" --include="*.json" --include="*.config" .

# Find Chromium header/footer templates that need migrating
grep -r "totalPages\|pageNumber\|footerTemplate\|headerTemplate" --include="*.cs" .

# Find synchronous endpoint calls
grep -r "/v1/html-to-pdf/sync\|/v1/html-to-pdf/async" --include="*.cs" .
```

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all pdforge usages in codebase**
  ```bash
  grep -r "api\.pdforge\.com\|api\.pdfnoodle\.com" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current `pdfParams` configurations used in your JSON requests**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Install IronPdf**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** pdforge has no .NET SDK to remove — you only add IronPDF and delete your HttpClient call sites.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace PDF generation calls**
  ```csharp
  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering.

- [ ] **Update page settings**
  ```csharp
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Configure headers/footers**
  ```csharp
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine provides modern rendering. Verify key documents.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  var merged = PdfDocument.Merge(pdf1, pdf2);
  pdf.SecuritySettings.UserPassword = "secret";
  pdf.ApplyWatermark("<h1>DRAFT</h1>");
  ```
  **Why:** IronPDF provides many additional features.

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **Getting Started Guide**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Code Examples**: https://ironpdf.com/examples/
- **Tutorials**: https://ironpdf.com/tutorials/
- **Support**: https://ironpdf.com/support/
