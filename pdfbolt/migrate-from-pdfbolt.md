# How Do I Migrate from PDFBolt to IronPDF in C#?

## Why Migrate from PDFBolt to IronPDF?

PDFBolt is a cloud-only SaaS service that processes your documents on external servers. While convenient for quick prototypes, this architecture creates significant challenges for production applications:

### Critical PDFBolt Limitations

1. **Cloud-Only Processing**: Every document passes through external servers—no self-hosted option
2. **Data Privacy Risks**: Sensitive documents (contracts, medical records, financial data) transmitted externally
3. **Usage Limits**: Free tier capped at 100 documents/month (20 req/min, 1 concurrent); paid tiers start at $19/mo for 2,000 docs and scale to $249/mo for 50,000 docs (pdfbolt.com/pricing)
4. **Network Dependency**: Internet outages or PDFBolt downtime = your PDF generation stops
5. **Latency**: Network round-trip adds seconds to every conversion
6. **Compliance Issues**: GDPR, HIPAA, SOC2 audits complicated by external processing
7. **API Key Security**: Leaked keys = unauthorized usage billed to your account
8. **Vendor Lock-in**: Your application fails if PDFBolt changes terms or shuts down

### IronPDF Advantages

| Concern | PDFBolt | IronPDF |
|---------|---------|---------|
| **Data Location** | External servers | Your servers only |
| **Usage Limits** | 100 free/month, then tiered subscriptions | Unlimited |
| **Internet Required** | Yes, always | No |
| **Latency** | Network round-trip | Milliseconds |
| **Compliance** | Complex (external processing) | Simple (local processing) |
| **Cost Model** | Monthly subscription tiers ($19–$249/mo) | One-time or annual |
| **Offline Operation** | Impossible | Fully supported |
| **API Key Risks** | Leaked = billed | License key, no billing risk |

---

## Package / Integration Changes

PDFBolt does **not publish an official .NET SDK** — there is no `PDFBolt` NuGet package. Integration is via `HttpClient` against the documented REST endpoints (`https://api.pdfbolt.com/v1/direct`, `/v1/sync`, `/v1/async`). To migrate, delete the HTTP wrapper code you wrote and install IronPDF:

```bash
# Install IronPDF (replaces hand-rolled PDFBolt HttpClient code)
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// PDFBolt — REST API, typically wrapped behind System.Net.Http
using System.Net.Http;
using System.Text;
using System.Text.Json;

// IronPDF
using IronPdf;
using IronPdf.Editing;
using IronPdf.Rendering;
```

---

## Complete API Reference

### Core Concept Mapping

| PDFBolt (REST) | IronPDF | Notes |
|----------------|---------|-------|
| `HttpClient` + `API-KEY` header | `new ChromePdfRenderer()` | No API key, no HTTP |
| JSON request body | `renderer.RenderingOptions` | Strongly-typed config |
| `https://api.pdfbolt.com/v1/direct` (raw PDF) | `renderer.RenderHtmlAsPdf(...)` | Local Chromium |
| `https://api.pdfbolt.com/v1/sync` (URL response) | `pdf.SaveAs(path)` | No download step |
| Response `byte[]` | `PdfDocument` | Rich document object |

### Conversion Patterns

| PDFBolt (REST) | IronPDF | Notes |
|----------------|---------|-------|
| `POST /v1/direct` with `{ "html": base64 }` | `renderer.RenderHtmlAsPdf(html)` | No base64 needed |
| `POST /v1/direct` with `{ "url": "..." }` | `renderer.RenderUrlAsPdf(url)` | Direct URL render |
| Read HTML file, base64, POST | `renderer.RenderHtmlFileAsPdf(path)` | One call |

### Output Handling

| PDFBolt (REST) | IronPDF | Notes |
|----------------|---------|-------|
| `await response.Content.ReadAsByteArrayAsync()` then `File.WriteAllBytes` | `pdf.SaveAs(path)` | One method |
| `await response.Content.ReadAsByteArrayAsync()` | `pdf.BinaryData` | Property access |
| `await response.Content.ReadAsStreamAsync()` | `pdf.Stream` | Stream property |

### Page Configuration

| PDFBolt JSON field | IronPDF | Notes |
|--------------------|---------|-------|
| `"format": "A4"` | `renderer.RenderingOptions.PaperSize = PdfPaperSize.A4` | Strongly-typed enum |
| `"format": "Letter"` | `renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter` | Standard sizes |
| `"landscape": true` | `renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape` | Landscape mode |
| `"landscape": false` | `renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait` | Default |
| `"width": "210mm", "height": "297mm"` | `renderer.RenderingOptions.SetCustomPaperSizeinMillimeters(210, 297)` | Custom size |

### Margins Configuration

| PDFBolt JSON field | IronPDF | Notes |
|--------------------|---------|-------|
| `"margin": { "top": "20mm" }` | `renderer.RenderingOptions.MarginTop = 20` | In millimeters |
| `"margin": { "bottom": "20mm" }` | `renderer.RenderingOptions.MarginBottom = 20` | Individual properties |
| `"margin": { "left": "15mm" }` | `renderer.RenderingOptions.MarginLeft = 15` | No margins object |
| `"margin": { "right": "15mm" }` | `renderer.RenderingOptions.MarginRight = 15` | Direct assignment |

### Rendering Options

| PDFBolt JSON field | IronPDF | Notes |
|--------------------|---------|-------|
| `"printBackground": true` | `renderer.RenderingOptions.PrintHtmlBackgrounds = true` | CSS backgrounds |
| `"waitUntil": "networkidle0"` | `renderer.RenderingOptions.WaitFor.NetworkIdle()` | Wait strategy |
| `"delay": 2000` | `renderer.RenderingOptions.WaitFor.RenderDelay(2000)` | Milliseconds |
| `"scale": 1.0` | `renderer.RenderingOptions.Zoom = 100` | Percentage |
| `"preferCSSPageSize": true` | `renderer.RenderingOptions.UseCssPageSize = true` | CSS @page rules |

### Headers and Footers

| PDFBolt | IronPDF | Notes |
|---------|---------|-------|
| `"headerTemplate": <base64 HTML>` | `renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter { HtmlFragment = "<div>Header</div>" }` | HTML-based, not base64 |
| `"footerTemplate": <base64 HTML>` | `renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter { HtmlFragment = "<div>Footer</div>" }` | Full CSS support |
| `"displayHeaderFooter": true` | Automatic when HtmlHeader/HtmlFooter set | Implicit |
| `<span class="pageNumber"></span>` | `{page}` | Current page |
| `<span class="totalPages"></span>` | `{total-pages}` | Total pages |
| `<span class="date"></span>` | `{date}` | Date |
| `<span class="title"></span>` | `{html-title}` | Document title |
| `<span class="url"></span>` | `{url}` | Source URL |

### PDF Manipulation (NEW in IronPDF)

| PDFBolt | IronPDF | Notes |
|---------|---------|-------|
| _(not available)_ | `PdfDocument.Merge(pdf1, pdf2)` | Merge PDFs |
| _(not available)_ | `pdf.CopyPages(start, end)` | Extract pages |
| _(not available)_ | `pdf.RemovePages(indices)` | Delete pages |
| _(not available)_ | `pdf.InsertPdf(other, index)` | Insert PDF |
| _(not available)_ | `pdf.RotatePage(index, degrees)` | Rotate pages |
| _(not available)_ | `pdf.ApplyWatermark(html)` | Add watermarks |
| _(not available)_ | `pdf.ExtractAllText()` | Extract text |
| _(not available)_ | `pdf.RasterizeToImageFiles()` | PDF to images |
| _(not available)_ | `pdf.SecuritySettings` | Encryption |

---

## Code Migration Examples

For complete migration workflows including authentication removal and offline deployment strategies, consult the [full migration documentation](https://ironpdf.com/blog/migration-guides/migrate-from-pdfbolt-to-ironpdf/).

### Example 1: Remove API Key Pattern

**Before (PDFBolt — HttpClient against `/v1/direct`):**
```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class PdfService
{
    private readonly HttpClient _http;

    public PdfService(IConfiguration config, HttpClient http)
    {
        // API key from config - security risk if leaked
        var apiKey = config["PDFBolt:ApiKey"];
        _http = http;
        _http.DefaultRequestHeaders.Add("API-KEY", apiKey);
    }

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        var base64Html = Convert.ToBase64String(Encoding.UTF8.GetBytes(html));
        var body = JsonSerializer.Serialize(new { html = base64Html });
        using var content = new StringContent(body, Encoding.UTF8, "application/json");

        using var response = await _http.PostAsync(
            "https://api.pdfbolt.com/v1/direct", content);

        // Quota exhaustion / invalid key surface as 4xx — handle them yourself
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        // License key set once at startup (not per-request)
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();
    }

    public byte[] GeneratePdf(string html)
    {
        // No quotas, no API key validation
        // No network required, no external processing
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Example 2: Async to Sync Conversion

**Before (PDFBolt):**
```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

public async Task<ActionResult> GenerateInvoice(int orderId)
{
    var order = await _orderService.GetOrderAsync(orderId);
    var html = await _templateService.RenderInvoiceAsync(order);

    var payload = JsonSerializer.Serialize(new
    {
        html = Convert.ToBase64String(Encoding.UTF8.GetBytes(html)),
        format = "A4",
        margin = new { top = "20mm", bottom = "20mm" }
    });

    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("API-KEY", _apiKey);
    using var content = new StringContent(payload, Encoding.UTF8, "application/json");

    // Network round-trip to PDFBolt servers
    using var response = await http.PostAsync(
        "https://api.pdfbolt.com/v1/direct", content);
    response.EnsureSuccessStatusCode();
    var bytes = await response.Content.ReadAsByteArrayAsync();

    return File(bytes, "application/pdf", $"invoice-{orderId}.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public ActionResult GenerateInvoice(int orderId)
{
    var order = _orderService.GetOrder(orderId);
    var html = _templateService.RenderInvoice(order);

    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.MarginTop = 20;
    renderer.RenderingOptions.MarginBottom = 20;

    // Local processing - no network, no external servers
    var pdf = renderer.RenderHtmlAsPdf(html);

    return File(pdf.BinaryData, "application/pdf", $"invoice-{orderId}.pdf");
}
```

### Example 3: URL to PDF with Headers/Footers

**Before (PDFBolt):**
```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

public async Task<byte[]> CaptureWebpageAsync(string url)
{
    // PDFBolt expects header/footer HTML to be base64-encoded;
    // page-number tokens are HTML class spans, not text placeholders.
    var headerHtml = $"<div style='font-size:10px'>Captured from: {url}</div>";
    var footerHtml = "<div style='font-size:10px'>Page <span class=\"pageNumber\"></span> of <span class=\"totalPages\"></span></div>";

    var payload = JsonSerializer.Serialize(new
    {
        url,
        format = "A4",
        landscape = false,
        displayHeaderFooter = true,
        headerTemplate = Convert.ToBase64String(Encoding.UTF8.GetBytes(headerHtml)),
        footerTemplate = Convert.ToBase64String(Encoding.UTF8.GetBytes(footerHtml)),
        waitUntil = "networkidle0",
        printBackground = true
    });

    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("API-KEY", _apiKey);
    using var content = new StringContent(payload, Encoding.UTF8, "application/json");

    using var response = await http.PostAsync(
        "https://api.pdfbolt.com/v1/direct", content);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CaptureWebpage(string url)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
    renderer.RenderingOptions.PrintHtmlBackgrounds = true;
    renderer.RenderingOptions.WaitFor.NetworkIdle();

    // HTML-based headers with full CSS support
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = $"<div style='font-size:10px; color:#666;'>Captured from: {url}</div>",
        MaxHeight = 20
    };

    // Built-in placeholders: {page}, {total-pages}, {date}, {time}
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='font-size:10px; text-align:center;'>Page {page} of {total-pages}</div>",
        MaxHeight = 20
    };

    var pdf = renderer.RenderUrlAsPdf(url);
    return pdf.BinaryData;
}
```

### Example 4: Batch Processing Without Rate Limits

**Before (PDFBolt):**
```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

public async Task GenerateMonthlyReports(List<Report> reports)
{
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("API-KEY", _apiKey);
    int processed = 0;

    foreach (var report in reports)
    {
        // Check monthly quota — PDFBolt free tier = 100 documents/month,
        // and Free is rate-limited to 20 requests/minute, 1 concurrent.
        if (processed >= 100)
        {
            throw new Exception("PDFBolt monthly free-tier limit reached!");
        }

        var html = RenderReport(report);
        var payload = JsonSerializer.Serialize(new
        {
            html = Convert.ToBase64String(Encoding.UTF8.GetBytes(html))
        });
        using var content = new StringContent(payload, Encoding.UTF8, "application/json");

        using var response = await http.PostAsync(
            "https://api.pdfbolt.com/v1/direct", content);

        if ((int)response.StatusCode == 429)
            throw new Exception($"Rate limited after {processed} reports");

        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync($"report-{report.Id}.pdf", bytes);
        processed++;

        // Throttle to stay under the per-minute cap
        await Task.Delay(1000);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void GenerateMonthlyReports(List<Report> reports)
{
    var renderer = new ChromePdfRenderer();

    // No rate limits, no monthly caps, no delays needed
    foreach (var report in reports)
    {
        var html = RenderReport(report);
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs($"report-{report.Id}.pdf");
    }

    // Or process in parallel for speed:
    Parallel.ForEach(reports, report =>
    {
        var localRenderer = new ChromePdfRenderer();
        var html = RenderReport(report);
        var pdf = localRenderer.RenderHtmlAsPdf(html);
        pdf.SaveAs($"report-{report.Id}.pdf");
    });
}
```

### Example 5: Error Handling Simplified

**Before (PDFBolt):**
```csharp
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public async Task<byte[]> SafeGeneratePdfAsync(string html)
{
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("API-KEY", _apiKey);
    var payload = JsonSerializer.Serialize(new
    {
        html = Convert.ToBase64String(Encoding.UTF8.GetBytes(html))
    });
    using var content = new StringContent(payload, Encoding.UTF8, "application/json");

    try
    {
        using var response = await http.PostAsync(
            "https://api.pdfbolt.com/v1/direct", content);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            { _logger.LogError("PDFBolt rate limit exceeded"); throw new Exception("429"); }
        if (response.StatusCode == HttpStatusCode.Unauthorized
            || response.StatusCode == HttpStatusCode.Forbidden)
            { _logger.LogError("PDFBolt API key invalid"); throw new Exception("auth"); }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError("Network error connecting to PDFBolt: {Message}", ex.Message);
        throw;
    }
    catch (TaskCanceledException)
    {
        _logger.LogError("PDFBolt request cancelled (timeout)");
        throw;
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] SafeGeneratePdf(string html)
{
    try
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
    catch (Exception ex)
    {
        // Only local processing errors - no network, no rate limits, no auth
        _logger.LogError("PDF generation error: {Message}", ex.Message);
        throw;
    }
}
```

### Example 6: PDF Merging (NEW Feature)

**Before (PDFBolt):**
```csharp
// PDFBolt doesn't support PDF merging
// You would need a separate library or service
throw new NotSupportedException("PDFBolt cannot merge PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] MergeInvoices(List<string> invoiceHtmls)
{
    var renderer = new ChromePdfRenderer();

    // Generate individual PDFs
    var pdfs = invoiceHtmls.Select(html => renderer.RenderHtmlAsPdf(html)).ToList();

    // Merge all into one document
    var merged = PdfDocument.Merge(pdfs);

    // Add page numbers to merged document
    merged.AddHtmlFooters(new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>",
        MaxHeight = 20
    });

    return merged.BinaryData;
}
```

### Example 7: Watermarks (NEW Feature)

**Before (PDFBolt):**
```csharp
// PDFBolt doesn't support watermarks
// You would need post-processing with another tool
throw new NotSupportedException("PDFBolt cannot add watermarks");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

public byte[] GenerateConfidentialDocument(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Add watermark with full CSS support
    pdf.ApplyWatermark(
        "<div style='color:red; font-size:72px; opacity:0.3; transform:rotate(-45deg);'>CONFIDENTIAL</div>",
        45,
        VerticalAlignment.Middle,
        HorizontalAlignment.Center);

    return pdf.BinaryData;
}
```

### Example 8: Security Settings (NEW Feature)

**Before (PDFBolt):**
```csharp
// PDFBolt doesn't support PDF security
// You would need post-processing with another tool
throw new NotSupportedException("PDFBolt cannot encrypt PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] GenerateSecureDocument(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Set password protection
    pdf.SecuritySettings.OwnerPassword = "admin123";
    pdf.SecuritySettings.UserPassword = "user456";

    // Control permissions
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;
    pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.NoPrint;
    pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;

    return pdf.BinaryData;
}
```

### Example 9: Text Extraction (NEW Feature)

**Before (PDFBolt):**
```csharp
// PDFBolt doesn't support text extraction
throw new NotSupportedException("PDFBolt cannot extract text");
```

**After (IronPDF):**
```csharp
using IronPdf;

public string ExtractTextFromPdf(string pdfPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    return pdf.ExtractAllText();
}

public string ExtractTextFromPage(string pdfPath, int pageIndex)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    return pdf.Pages[pageIndex].Text;
}
```

### Example 10: Offline/Air-Gapped Operation

**Before (PDFBolt):**
```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

// PDFBolt cannot work offline - requires internet
public async Task<byte[]> GeneratePdfOffline(string html)
{
    // Check internet connection
    if (!await IsInternetAvailable())
    {
        throw new InvalidOperationException("PDFBolt requires internet connection");
    }

    // Even with internet, api.pdfbolt.com might be down
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("API-KEY", _apiKey);
    var payload = JsonSerializer.Serialize(new
    {
        html = Convert.ToBase64String(Encoding.UTF8.GetBytes(html))
    });
    using var content = new StringContent(payload, Encoding.UTF8, "application/json");
    using var response = await http.PostAsync(
        "https://api.pdfbolt.com/v1/direct", content);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] GeneratePdfOffline(string html)
{
    // Works completely offline - no internet required
    // Perfect for air-gapped environments, submarines, remote sites
    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

---

## Placeholder Mapping

PDFBolt's header/footer placeholders are HTML class spans inside the (base64-encoded) `headerTemplate` / `footerTemplate` body, mirroring Chrome's headless print API. IronPDF uses simple curly-brace tokens inside `HtmlFragment`. PDFBolt explicitly does not support Handlebars-style `{{var}}` tokens in header/footer templates.

| PDFBolt Placeholder | IronPDF Placeholder | Description |
|---------------------|---------------------|-------------|
| `<span class="pageNumber"></span>` | `{page}` | Current page number |
| `<span class="totalPages"></span>` | `{total-pages}` | Total page count |
| `<span class="date"></span>` | `{date}` | Current date |
| _(not documented)_ | `{time}` | Current time |
| `<span class="title"></span>` | `{html-title}` | Document title |
| `<span class="url"></span>` | `{url}` | Source URL |

---

## Configuration Migration

### From Environment Variables

**Before (PDFBolt):**
```csharp
// Must secure API key in environment
var apiKey = Environment.GetEnvironmentVariable("PDFBOLT_API_KEY");
var http = new HttpClient();
http.DefaultRequestHeaders.Add("API-KEY", apiKey);
```

**After (IronPDF):**
```csharp
// License key - set once at startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Or from config (no security risk like API key billing)
```

### From appsettings.json

**Before (PDFBolt):**
```json
{
  "PDFBolt": {
    "ApiKey": "pk_live_xxxxx",
    "Timeout": 30000,
    "RetryCount": 3
  }
}
```

**After (IronPDF):**
```json
{
  "IronPdf": {
    "LicenseKey": "YOUR-LICENSE-KEY"
  }
}
```

---

## Dependency Injection Migration

**Before (PDFBolt):**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient("PDFBolt", (sp, http) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        http.BaseAddress = new Uri("https://api.pdfbolt.com/v1/");
        http.DefaultRequestHeaders.Add("API-KEY", config["PDFBolt:ApiKey"]);
    });
}
```

**After (IronPDF):**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Set license once
    IronPdf.License.LicenseKey = Configuration["IronPdf:LicenseKey"];

    // Register renderer as transient (thread-safe creation)
    services.AddTransient<ChromePdfRenderer>();
}
```

---

## Common Migration Gotchas

### 1. Async → Sync Pattern

PDFBolt requires async for network calls. IronPDF is sync by default:

```csharp
// PDFBolt: await required (HttpClient over the wire)
using var response = await http.PostAsync("https://api.pdfbolt.com/v1/direct", content);
var bytes = await response.Content.ReadAsByteArrayAsync();

// IronPDF: No await needed (local Chromium)
var pdf = renderer.RenderHtmlAsPdf(html);
var bytes = pdf.BinaryData;
```

### 2. No More API Key Validation

Remove all API key checking code:

```csharp
// PDFBolt: Must validate
if (string.IsNullOrEmpty(apiKey))
    throw new Exception("API key required");

// IronPDF: Remove this check entirely
```

### 3. Margin Units

PDFBolt accepts CSS-style strings ("20mm", "1in"). IronPDF margins are always millimeters as a numeric property:

```csharp
// PDFBolt: string with CSS unit suffix
margin = new { top = "20mm" }

// IronPDF: millimeters as a number
renderer.RenderingOptions.MarginTop = 20; // mm
```

### 4. Page Number Placeholders

PDFBolt's headers/footers use Chrome-headless class spans (the body itself is base64-encoded inside `headerTemplate`/`footerTemplate`); IronPDF uses curly-brace tokens inside the `HtmlFragment` string:

```csharp
// PDFBolt (decoded headerTemplate body)
"Page <span class=\"pageNumber\"></span> of <span class=\"totalPages\"></span>"

// IronPDF
"Page {page} of {total-pages}"
```

### 5. Remove Rate Limit Handling

Delete all 429 / quota retry logic — IronPDF has no per-document quotas:

```csharp
// PDFBolt: HTTP 429 retry
if (response.StatusCode == HttpStatusCode.TooManyRequests)
{
    await Task.Delay(60000); // Wait and retry
}

// IronPDF: Delete this entirely
```

### 6. Remove Network Error Handling

Local processing means no network errors:

```csharp
// PDFBolt: Network error handling
catch (HttpRequestException ex)
catch (TaskCanceledException)
catch (TimeoutException)

// IronPDF: Remove network-specific catches
```

### 7. Drop the API Wrapper Classes

If you wrote a `PDFBoltClient` / `Client` wrapper around `HttpClient` (PDFBolt does not ship one — there is no `PDFBolt` NuGet package and no `using PDFBolt;` namespace), delete it; `ChromePdfRenderer` is the only class you need.

---

## Pre-Migration Checklist

- [ ] Inventory all PDFBolt REST calls in codebase (`api.pdfbolt.com`)
- [ ] Document current API key management
- [ ] List all async methods that need sync conversion
- [ ] Identify 429 / quota retry code to remove
- [ ] Note PDFBolt margin strings (`"20mm"`) → IronPDF mm numbers
- [ ] Map placeholder syntax differences (class spans → `{page}` tokens)
- [ ] Identify features PDFBolt cannot provide (merge, watermark, security, text extraction)
- [ ] Plan license key deployment

---

## Post-Migration Checklist

- [ ] Delete the hand-rolled PDFBolt HttpClient wrapper / DTOs
- [ ] Delete API key from configuration files
- [ ] Remove API key from secret managers
- [ ] Convert async methods to sync
- [ ] Remove 429 / quota retry handling
- [ ] Remove network error handling
- [ ] Test all PDF generation paths
- [ ] Verify placeholder syntax (class spans → `{page}` / `{total-pages}`)
- [ ] Add new features (watermarks, security) as needed
- [ ] Update documentation

---

## Finding PDFBolt References

```bash
# Find all PDFBolt REST calls
grep -rE "api\.pdfbolt\.com|API-KEY|/v1/(direct|sync|async)" --include="*.cs" .

# Find API key references (config, code, secrets)
grep -r "PDFBOLT\|ApiKey\|api.key\|apiKey" --include="*.cs" --include="*.json" --include="*.config" .

# Find async PostAsync patterns wrapping the API
grep -rE "PostAsync\(.*pdfbolt|api\.pdfbolt\.com" --include="*.cs" .
```

---

## Performance Comparison

| Metric | PDFBolt | IronPDF |
|--------|---------|---------|
| Simple HTML (1 page) | 2-5 seconds (network) | 100-300ms (local) |
| Complex HTML (10 pages) | 5-15 seconds | 500ms-2s |
| Batch (100 documents) | Rate limited | No limits |
| Offline operation | Impossible | Full support |
| First request | 3-8 seconds (cold start) | 500ms (engine init) |

---

## Feature Comparison Summary

| Feature | PDFBolt | IronPDF |
|---------|---------|---------|
| HTML to PDF | ✓ | ✓ |
| URL to PDF | ✓ | ✓ |
| Headers/Footers | ✓ (text) | ✓ (full HTML) |
| Page Numbers | ✓ | ✓ |
| Custom Page Sizes | ✓ | ✓ |
| Margins | ✓ | ✓ |
| PDF Merging | ✗ | ✓ |
| PDF Splitting | ✗ | ✓ |
| Watermarks | ✗ | ✓ |
| Password Protection | ✗ | ✓ |
| Text Extraction | ✗ | ✓ |
| PDF to Images | ✗ | ✓ |
| Form Filling | ✗ | ✓ |
| Digital Signatures | ✗ | ✓ |
| Offline Operation | ✗ | ✓ |
| Unlimited Processing | ✗ | ✓ |
| Data Privacy | ✗ (cloud) | ✓ (local) |

---

## Troubleshooting

### "License key required for production"

```csharp
// Set license key before any PDF operations
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Free for development, license for production
```

### "First PDF generation is slow"

The Chromium engine initializes on first use. Pre-warm if needed:

```csharp
// Warm up during application startup
var renderer = new ChromePdfRenderer();
renderer.RenderHtmlAsPdf("<html><body></body></html>");
```

### "Async methods no longer await"

IronPDF methods are synchronous. Remove await keywords:

```csharp
// Old: var response = await http.PostAsync("https://api.pdfbolt.com/v1/direct", content);
// New: var pdf = renderer.RenderHtmlAsPdf(html);
```

### "Rate limit code throws compile errors"

Remove all PDFBolt-specific HTTP status handling:

```csharp
// Delete: 429 / TooManyRequests retry blocks for api.pdfbolt.com
// IronPDF has no per-document quotas
```

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PDFBolt usages in codebase**
  ```bash
  grep -rE "api\.pdfbolt\.com|API-KEY" --include="*.cs" .
  ```
  **Why:** Identify every call site of the REST API so nothing is missed.

- [ ] **Document current configurations**
  **Why:** Each JSON field (`format`, `margin`, `printBackground`, `headerTemplate`, etc.) maps to a `RenderingOptions` property; capture them before deleting the wrapper.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Delete the HttpClient wrapper and install IronPdf**
  ```bash
  # Delete your hand-rolled PDFBolt HttpClient wrapper / DTOs
  dotnet add package IronPdf
  ```
  **Why:** PDFBolt has no NuGet SDK, so there is no package to remove — only your wrapper code. Then install IronPDF.

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

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Tutorial](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [API Reference](https://ironpdf.com/object-reference/api/)
