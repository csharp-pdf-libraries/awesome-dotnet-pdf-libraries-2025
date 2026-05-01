# How Do I Migrate from Kaizen.io HTML-to-PDF to IronPDF in C#?

## Table of Contents
1. [Why Migrate from Kaizen.io](#why-migrate-from-kaizenio)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from Kaizen.io

### The Container-API Challenges

Kaizen.io HTML-to-PDF is a self-hosted Docker container that exposes a single REST endpoint (`POST /html-to-pdf` on port 8080) — there is **no official .NET SDK or NuGet package**. Every C# call has to be a hand-rolled `HttpClient` POST against the running container. That model has real limits:

1. **Container Required**: You must run `kaizenio.azurecr.io/html-to-pdf:latest` somewhere reachable from your app — local Docker, sidecar, or a separate host.
2. **Per-Process HTTP Hop**: Each PDF is an HTTP round-trip + JSON serialization, even when the container is on `localhost`.
3. **Tiny v1.x API Surface**: The documented JSON body accepts only an `html` field. URL-to-PDF, custom stylesheets, headers/footers, page size, orientation, and margins are roadmap items, not shipping features.
4. **Page Numbers Don't Exist Yet**: There is no `{page}` / `{total}` placeholder support in the API — you cannot ask Kaizen for page X of Y.
5. **Watermark on Free Tier**: Without the `KAIZEN_PDF_LICENSE` env var, every PDF carries a watermark.
6. **No SDK Means No IntelliSense**: You write raw HTTP, parse responses, and handle the connection lifecycle yourself.

### The IronPDF Advantage

| Feature | Kaizen.io HTML-to-PDF | IronPDF |
|---------|-----------------------|---------|
| Integration | Docker container + raw HTTP from C# | NuGet `IronPdf`, in-process |
| Processing | Out-of-process Chromium in container | In-process Chromium |
| Headers / Footers | Not in v1.x API | `TextHeader` + `HtmlHeader` with placeholders |
| Page Numbers | Not supported | `{page}` and `{total-pages}` |
| URL → PDF | Roadmap | `RenderUrlAsPdf(url)` shipping |
| Page Size / Orientation / Margins | Style with CSS `@page` | `RenderingOptions.PaperSize` etc. |
| Async | Use `HttpClient.PostAsync` yourself | `RenderHtmlAsPdfAsync` first-class |
| Free-tier output | Watermarked | Full-quality, dev/trial |

### Migration Benefits

- **No container to operate**: Replace Docker + sidecar plumbing with a NuGet package.
- **Real options object**: Configure paper size, orientation, headers, footers, JavaScript timing in code rather than embedding `@page` CSS in every document.
- **Page numbering and totals**: `{page}` / `{total-pages}` placeholders are supported on both `TextHeader/TextFooter` and `HtmlHeader/HtmlFooter`.
- **Strongly-typed API**: `ChromePdfRenderer` has compile-time-checked properties; the Kaizen REST shape is JSON-by-convention.
- **Lower per-call overhead**: No HTTP serialization round-trip on every PDF.

---

## Before You Start

### Prerequisites

1. **.NET Environment**: .NET Framework 4.6.2+, .NET Core 3.1+, or .NET 5+
2. **NuGet Access**: Ability to install NuGet packages
3. **IronPDF License**: Free trial or purchased license key (IronPDF; no relation to `KAIZEN_PDF_LICENSE`)

### Installation

There is **no Kaizen NuGet package to remove** — Kaizen ships only as the Docker image `kaizenio.azurecr.io/html-to-pdf`. Stop and remove the running container, then add IronPDF:

```bash
# Stop and remove the Kaizen container (if running)
docker stop kaizen-pdf
docker rm kaizen-pdf

# Install IronPDF
dotnet add package IronPdf
```

### License Configuration

```csharp
// Add at application startup (Program.cs or Startup.cs)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Identify Kaizen.io Usage

Because Kaizen has no SDK, calls into it look like generic `HttpClient` POSTs. Search for the endpoint or hostname instead:

```bash
# Find the Kaizen endpoint and license env var across the codebase
grep -r "html-to-pdf\|KAIZEN_PDF_LICENSE\|kaizenio.azurecr.io\|localhost:8080" --include="*.cs" --include="*.json" --include="*.yml" .
```

---

## Quick Start Migration

### Minimal Change Example

**Before (Kaizen.io — raw HTTP against the Docker container):**
```csharp
// Container must be running:
//   docker run -d -p 8080:8080 -e KAIZEN_PDF_LICENSE=... \
//     kaizenio.azurecr.io/html-to-pdf:latest
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class KaizenPdfService
{
    private readonly HttpClient _http = new HttpClient();

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        var payload = JsonSerializer.Serialize(new { html });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("http://localhost:8080/html-to-pdf", content);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        _renderer.RenderingOptions.MarginTop = 20;
        _renderer.RenderingOptions.MarginBottom = 20;
    }

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

---

## Complete API Reference

### Concept Mappings

There is no Kaizen .NET class to map; only the JSON request shape and the conventions you used around it.

| Kaizen.io concept | IronPDF Equivalent | Notes |
|-------------------|--------------------|-------|
| `HttpClient` + `POST /html-to-pdf` | `ChromePdfRenderer` | In-process, no HTTP |
| JSON `{ "html": "..." }` body | `RenderHtmlAsPdf(string html)` | Direct method call |
| (No URL field in v1.x) | `RenderUrlAsPdf(string url)` | First-class |
| (No file field in v1.x) | `RenderHtmlFileAsPdf(string path)` | First-class |
| Inline `@page` CSS for size | `RenderingOptions.PaperSize` | `PdfPaperSize` enum |
| Inline `@page` CSS for orientation | `RenderingOptions.PaperOrientation` | `PdfPaperOrientation` enum |
| Inline `@page { margin: ... }` | `RenderingOptions.MarginTop/Bottom/Left/Right` | Millimeters |
| Inline `<div class='header'>` hack | `RenderingOptions.TextHeader` / `HtmlHeader` | Real header zone |
| Inline `<div class='footer'>` hack | `RenderingOptions.TextFooter` / `HtmlFooter` | Real footer zone |
| (No placeholder in API) | `{page}`, `{total-pages}`, `{html-title}`, `{date}` | Substituted at render time |
| `HttpClient.PostAsync` | `RenderHtmlAsPdfAsync` | Native async |
| Container env var `KAIZEN_PDF_LICENSE` | `IronPdf.License.LicenseKey` | Set once at startup |
| HTTP `byte[]` response | `pdf.BinaryData` or `pdf.SaveAs(path)` | `PdfDocument` |

### Header / Footer Mapping

Kaizen v1.x has no header/footer fields. The migration replaces "fake header divs styled with absolute CSS positioning" with IronPDF's real header zones:

| Goal | IronPDF |
|------|---------|
| Plain-text header centered | `RenderingOptions.TextHeader = new TextHeaderFooter { CenterText = "Company Report" }` |
| HTML-rich header | `RenderingOptions.HtmlHeader = new HtmlHeaderFooter { HtmlFragment = "<div>...</div>", MaxHeight = 25 }` |
| Page numbers | `CenterText = "Page {page} of {total-pages}"` |
| Document title | `CenterText = "{html-title}"` |

### Placeholder Mappings

Kaizen has **no** server-side placeholders. If you previously hand-substituted strings into the HTML before POSTing, replace those with IronPDF's render-time placeholders:

| What you used to do | IronPDF Placeholder |
|---------------------|---------------------|
| `html.Replace("{page}", currentPage.ToString())` | `{page}` |
| `html.Replace("{total}", total.ToString())` | `{total-pages}` |
| `html.Replace("{date}", DateTime.Now.ToShortDateString())` | `{date}` |
| `html.Replace("{title}", docTitle)` | `{html-title}` |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (Kaizen.io):**
```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
{
    using var http = new HttpClient();
    var payload = JsonSerializer.Serialize(new { html });
    var response = await http.PostAsync(
        "http://localhost:8080/html-to-pdf",
        new StringContent(payload, Encoding.UTF8, "application/json"));
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertHtmlToPdf(string html)
{
    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Example 2: URL to PDF with Options

Kaizen v1.x has no URL endpoint and no options. Workaround was to fetch the page yourself, optionally rewrite resource URLs, and send the result.

**Before (Kaizen.io):**
```csharp
public async Task<byte[]> ConvertUrlToPdfAsync(string url)
{
    using var http = new HttpClient();

    // No ConvertUrl in Kaizen — fetch the page client-side.
    var pageHtml = await http.GetStringAsync(url);

    // No paper-size / orientation / margin fields — embed in CSS.
    var wrapped = "<style>@page { size: A4 landscape; margin: 15mm 10mm; }</style>" + pageHtml;

    var payload = JsonSerializer.Serialize(new { html = wrapped });
    var response = await http.PostAsync(
        "http://localhost:8080/html-to-pdf",
        new StringContent(payload, Encoding.UTF8, "application/json"));
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertUrlToPdf(string url)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
    renderer.RenderingOptions.MarginTop = 15;
    renderer.RenderingOptions.MarginBottom = 15;
    renderer.RenderingOptions.MarginLeft = 10;
    renderer.RenderingOptions.MarginRight = 10;

    return renderer.RenderUrlAsPdf(url).BinaryData;
}
```

### Example 3: Headers and Footers

This is one of the biggest wins from migrating: Kaizen's API does not have headers/footers at all, so you previously had to fake them in CSS with no page-number support.

**Before (Kaizen.io):**
```csharp
public async Task<byte[]> CreateDocumentWithHeaderFooterAsync(string body)
{
    using var http = new HttpClient();

    // Fake header/footer via fixed-position divs and @page margins.
    // Page-X-of-Y is impossible — the API has no placeholders.
    var html = $@"<!doctype html><html><head><style>
        @page {{ margin: 30mm; }}
        .h {{ position: fixed; top: -25mm; left: 0; right: 0; text-align: center; font-size: 10px; }}
        .f {{ position: fixed; bottom: -25mm; left: 0; right: 0; text-align: center; font-size: 10px; }}
    </style></head><body>
        <div class='h'>Company Report</div>
        <div class='f'>Footer (no page numbers in Kaizen v1.x)</div>
        {body}
    </body></html>";

    var payload = JsonSerializer.Serialize(new { html });
    var response = await http.PostAsync(
        "http://localhost:8080/html-to-pdf",
        new StringContent(payload, Encoding.UTF8, "application/json"));
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateDocumentWithHeaderFooter(string html)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center; font-size:10px;'>Company Report</div>",
        MaxHeight = 25
    };

    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center; font-size:10px;'>Page {page} of {total-pages}</div>",
        MaxHeight = 25
    };

    renderer.RenderingOptions.MarginTop = 30;
    renderer.RenderingOptions.MarginBottom = 30;

    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Example 4: Async Operations

**Before (Kaizen.io):** every call was already async because it was raw HTTP.
```csharp
public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
{
    using var http = new HttpClient();
    var payload = JsonSerializer.Serialize(new { html });
    var response = await http.PostAsync(
        "http://localhost:8080/html-to-pdf",
        new StringContent(payload, Encoding.UTF8, "application/json"));
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

### Example 5: Custom Page Size

**Before (Kaizen.io):** embed a custom `@page` size in CSS.
```csharp
public async Task<byte[]> CreateCustomSizePdfAsync(string body, int widthMm, int heightMm)
{
    using var http = new HttpClient();
    var html = $@"<style>@page {{ size: {widthMm}mm {heightMm}mm; }}</style>{body}";
    var payload = JsonSerializer.Serialize(new { html });
    var response = await http.PostAsync(
        "http://localhost:8080/html-to-pdf",
        new StringContent(payload, Encoding.UTF8, "application/json"));
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateCustomSizePdf(string html, int widthMm, int heightMm)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(widthMm, heightMm);
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Example 6: JavaScript Wait

Kaizen's container runs Chromium internally, but the v1.x API has **no documented field** to delay or wait for JS readiness. If your page needs JS to settle, you have to inline the wait into the HTML (e.g., a synchronous `setTimeout` blocker) or rely on container defaults.

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertSpaPage(string url)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.EnableJavaScript = true;
    renderer.RenderingOptions.WaitFor.RenderDelay(3000);  // Wait 3 seconds

    // Or wait for a specific JS condition:
    // renderer.RenderingOptions.WaitFor.JavaScript("window.appReady === true");

    return renderer.RenderUrlAsPdf(url).BinaryData;
}
```

### Example 7: Save to File vs Bytes

**Before (Kaizen.io):**
```csharp
public async Task SavePdfToFileAsync(string html, string outputPath)
{
    using var http = new HttpClient();
    var payload = JsonSerializer.Serialize(new { html });
    var response = await http.PostAsync(
        "http://localhost:8080/html-to-pdf",
        new StringContent(payload, Encoding.UTF8, "application/json"));
    response.EnsureSuccessStatusCode();
    var bytes = await response.Content.ReadAsByteArrayAsync();
    File.WriteAllBytes(outputPath, bytes);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void SavePdfToFile(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);  // Direct file saving
}
```

### Example 8: License Configuration

Kaizen's license is a Docker env var on the container. IronPDF's license is a static property on the `License` class set once at app startup.

**Before (Kaizen.io):**
```bash
docker run -d --rm -p 8080:8080 \
  -e KAIZEN_PDF_LICENSE=YOUR_KAIZEN_LICENSE \
  --pull=always --name kaizen-pdf kaizenio.azurecr.io/html-to-pdf:latest
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        // License key set once at startup — no container, no env var.
        IronPdf.License.LicenseKey = "YOUR_IRONPDF_LICENSE_KEY";
        _renderer = new ChromePdfRenderer();
    }
}
```

### Example 9: Error Handling

Kaizen errors arrive as HTTP status codes; IronPDF errors arrive as typed exceptions.

**Before (Kaizen.io):**
```csharp
public async Task<byte[]> SafeConvertAsync(string html)
{
    using var http = new HttpClient();
    var payload = JsonSerializer.Serialize(new { html });

    try
    {
        var response = await http.PostAsync(
            "http://localhost:8080/html-to-pdf",
            new StringContent(payload, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            // Container may be down, license invalid/expired, or render failed.
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"Kaizen returned {(int)response.StatusCode}: {body}");
        }
        return await response.Content.ReadAsByteArrayAsync();
    }
    catch (HttpRequestException ex)
    {
        // Container not running or unreachable.
        throw new Exception("Kaizen container unreachable: " + ex.Message);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] SafeConvert(string html)
{
    try
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
    catch (IronPdf.Exceptions.IronPdfLicensingException ex)
    {
        throw new Exception("License error: " + ex.Message);
    }
    // No "container down" or "endpoint unreachable" branch — IronPDF runs in-process.
}
```

### Example 10: Complete Service Migration

**Before (Kaizen.io Service):**
```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class KaizenPdfService
{
    private readonly HttpClient _http;
    private readonly string _endpoint;

    public KaizenPdfService(string endpoint = "http://localhost:8080/html-to-pdf")
    {
        _endpoint = endpoint;
        _http = new HttpClient();
    }

    public async Task<byte[]> GenerateReportAsync(ReportData data)
    {
        // Bake page-size, margins, header, and footer into the HTML
        // because the v1.x JSON API has no fields for them.
        var html = $@"<!doctype html><html><head><style>
            @page {{ size: A4 portrait; margin: 25mm; }}
            .h {{ position: fixed; top: -20mm; left: 0; right: 0; text-align: center; }}
            .f {{ position: fixed; bottom: -20mm; left: 0; right: 0; text-align: center; }}
        </style></head><body>
            <div class='h'>{data.Title}</div>
            <div class='f'>(page numbers unsupported)</div>
            <h1>{data.Title}</h1>
        </body></html>";

        var payload = JsonSerializer.Serialize(new { html });
        var response = await _http.PostAsync(
            _endpoint,
            new StringContent(payload, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }
}
```

**After (IronPDF Service):**
```csharp
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        _renderer.RenderingOptions.MarginTop = 25;
        _renderer.RenderingOptions.MarginBottom = 25;
    }

    public async Task<byte[]> GenerateReportAsync(ReportData data)
    {
        // Real header/footer zone with page-number placeholders.
        _renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = $"<div>{data.Title}</div>",
            MaxHeight = 25
        };

        _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div>Page {page} of {total-pages}</div>",
            MaxHeight = 25
        };

        var html = $"<html><body><h1>{data.Title}</h1></body></html>";
        var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}
```

---

## Advanced Scenarios

### Removing the Container Dependency

```csharp
// Kaizen.io: requires a Docker container running on a reachable host.
// IronPDF: in-process — no container, no port, no sidecar.

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);  // Just runs.
```

For end-to-end migration patterns including container teardown and CI/CD updates, the [detailed guide](https://ironpdf.com/blog/migration-guides/migrate-from-kaizen-io-to-ironpdf/) walks through architectural considerations.

### Removing the HTTP Plumbing

```csharp
// DELETE all of:
// - HttpClient lifetime management
// - JSON serialization of { html }
// - StatusCode checks
// - "container not reachable" retry/backoff
// - byte[] reading via ReadAsByteArrayAsync

// All replaced with one in-process call:
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Parallel Processing

```csharp
// Kaizen.io: concurrency is bounded by what your container instance can serve
// before requests queue or time out.

// IronPDF: limited only by your hardware.
var renderer = new ChromePdfRenderer();

var tasks = htmlDocuments.Select(html =>
    renderer.RenderHtmlAsPdfAsync(html));

var pdfs = await Task.WhenAll(tasks);
```

### Data Privacy

```csharp
// Kaizen self-hosted: data stays inside your container, but still leaves
// your application process and travels over a (usually localhost) socket.
// The container image is pulled from kaizenio.azurecr.io.

// IronPDF: data never leaves the calling process.
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(sensitivePatientData);
```

---

## Performance Considerations

### Latency Comparison

| Scenario | Kaizen.io (localhost container) | IronPDF |
|----------|---------------------------------|---------|
| Simple HTML | HTTP + JSON + Chromium render | Chromium render only |
| Complex page | + serialization overhead | In-process |
| With JavaScript | No documented JS-wait field; tune in HTML | `WaitFor.RenderDelay` / `WaitFor.JavaScript` |
| First render | Container cold start + warm-up | Chromium init (1-3s) |
| Subsequent | Per-request HTTP round-trip | Direct method call |

Numbers vary heavily by host and document complexity; benchmark on your own hardware before quoting figures.

### Cost Comparison

| Item | Kaizen.io HTML-to-PDF | IronPDF |
|------|-----------------------|---------|
| License | One-time **$50** per Kaizen license (free tier watermarks output) | Commercial license (annual or perpetual) |
| Infrastructure | You run + maintain the container | None beyond your app process |
| Per-request | No usage fees; your own compute | No usage fees; your own compute |

Source: vendor pricing page at https://www.kaizen.io/products/html-to-pdf/ as of this writing — confirm current price before quoting.

### Optimization Tips

1. **Reuse Renderer**: Create a `ChromePdfRenderer` once and share it.
2. **Warm Up**: Render a trivial HTML at startup so the first user request doesn't pay the Chromium init cost.
3. **Async Methods**: Prefer `RenderHtmlAsPdfAsync` in web apps.
4. **Parallel Processing**: No artificial throttle — bound by CPU/RAM.

```csharp
public class OptimizedPdfService
{
    private readonly ChromePdfRenderer _renderer;
    private static bool _warmedUp;

    public OptimizedPdfService()
    {
        _renderer = new ChromePdfRenderer();

        if (!_warmedUp)
        {
            _renderer.RenderHtmlAsPdf("<html></html>");
            _warmedUp = true;
        }
    }
}
```

---

## Troubleshooting

### Issue 1: License Configuration Differs

**Cause**: Kaizen's `KAIZEN_PDF_LICENSE` is a container env var, not an SDK setting.
**IronPDF Solution**: Replace with `IronPdf.License.LicenseKey` at startup.

```csharp
// DELETE container env var: -e KAIZEN_PDF_LICENSE=...
// ADD: IronPdf.License.LicenseKey = "LICENSE_KEY";
```

### Issue 2: Container Connection Errors

**Cause**: Kaizen relies on a reachable container at port 8080.
**IronPDF Solution**: In-process — there is no endpoint to connect to. Delete connection error handling.

### Issue 3: Missing Headers, Footers, Page Numbers

**Cause**: Kaizen v1.x has no header/footer or placeholder fields; you faked them in CSS, with no `{page}` / `{total}` available.
**IronPDF Solution**: Use real `TextHeader/TextFooter` or `HtmlHeader/HtmlFooter`, with `{page}` and `{total-pages}` placeholders supported natively.

```csharp
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Page {page} of {total-pages}"
};
```

### Issue 4: Placeholder Conventions

**Cause**: If you previously substituted `{page}` / `{total}` in your own code before POSTing to Kaizen, those placeholders are now resolved by IronPDF.
**IronPDF Solution**:
- `{page}` (unchanged)
- `{total}` → `{total-pages}`
- `{title}` → `{html-title}`

### Issue 5: Return Type Difference

**Cause**: Kaizen returns raw bytes via HTTP; IronPDF returns a `PdfDocument`.
**IronPDF Solution**: Use `.BinaryData` if you still need bytes.

```csharp
var pdf = renderer.RenderHtmlAsPdf(html);
byte[] bytes = pdf.BinaryData;
```

### Issue 6: Saving to a File

**Cause**: With Kaizen you wrote HTTP bytes via `File.WriteAllBytes`.
**IronPDF Solution**: Use `pdf.SaveAs(path)` directly.

```csharp
// Before: File.WriteAllBytes("output.pdf", pdfBytes);
// After:  pdf.SaveAs("output.pdf");
```

### Issue 7: Timeouts

**Cause**: Kaizen used `HttpClient.Timeout`; IronPDF has its own render timeout.
**IronPDF Solution**:

```csharp
renderer.RenderingOptions.Timeout = 60;  // seconds
```

### Issue 8: First Render Slow

**Cause**: IronPDF initializes Chromium on first use.
**IronPDF Solution**: Warm up at startup.

```csharp
new ChromePdfRenderer().RenderHtmlAsPdf("<html></html>");
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Locate every Kaizen call site**
  ```bash
  grep -r "html-to-pdf\|KAIZEN_PDF_LICENSE\|kaizenio.azurecr.io\|localhost:8080" \
    --include="*.cs" --include="*.json" --include="*.yml" .
  ```
  **Why:** Without an SDK, calls look like generic HTTP — search for the endpoint and license env var.

- [ ] **Document inline `@page` CSS used for layout**
  ```csharp
  // Find embedded styles like:
  // <style>@page { size: A4 portrait; margin: 20mm }</style>
  ```
  **Why:** These map to `RenderingOptions.PaperSize` / `PaperOrientation` / `MarginTop` etc.

- [ ] **Document fake header/footer divs**
  ```csharp
  // Look for fixed-position divs in the HTML you POST,
  // e.g. .header { position: fixed; top: -20mm; ... }
  ```
  **Why:** These map to IronPDF's real `TextHeader` / `HtmlHeader` zones.

- [ ] **List any client-side placeholder substitution**
  ```csharp
  // Look for: html.Replace("{page}", ...), html.Replace("{total}", ...)
  ```
  **Why:** IronPDF resolves `{page}` and `{total-pages}` server-side; remove the manual substitution.

- [ ] **Confirm async patterns**
  **Why:** Kaizen calls were already async (HttpClient); IronPDF has matching `*Async` methods.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license for production. Free trial at https://ironpdf.com/.

### Container Teardown

- [ ] **Stop and remove the running Kaizen container**
  ```bash
  docker stop kaizen-pdf
  docker rm kaizen-pdf
  ```
  **Why:** No longer needed once IronPDF takes over.

- [ ] **Remove Kaizen pull/run steps from CI/CD**
  **Why:** Container is no longer part of the deployment unit.

- [ ] **Remove `KAIZEN_PDF_LICENSE` from secrets/env**
  **Why:** Replaced by IronPDF's license key.

- [ ] **Install `IronPdf` NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** This is the actual integration — a NuGet package, not a container.

### Code Changes

- [ ] **Add license key configuration at startup**
  ```csharp
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** Set once before any PDF operation.

- [ ] **Replace `HttpClient` POST with `ChromePdfRenderer`**
  ```csharp
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** In-process renderer in place of the HTTP-to-container pattern.

- [ ] **Move embedded CSS layout into `RenderingOptions`**
  ```csharp
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** Configuration-as-API instead of CSS-by-convention.

- [ ] **Replace JSON POST with `RenderHtmlAsPdf()`**
  ```csharp
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** Direct method call replaces the `POST /html-to-pdf` round-trip.

- [ ] **Use `RenderUrlAsPdf()` for URL input**
  ```csharp
  var pdf = renderer.RenderUrlAsPdf(url);
  ```
  **Why:** Kaizen v1.x has no URL endpoint; IronPDF supports URL input directly.

- [ ] **Replace fake header/footer divs with real header zones**
  ```csharp
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
  {
      HtmlFragment = "<div>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** Real header/footer with page-number placeholders.

- [ ] **Update placeholder syntax (`{total}` → `{total-pages}`)**
  **Why:** IronPDF's placeholder names differ from common third-party conventions.

- [ ] **Replace `byte[]` HTTP body with `pdf.BinaryData`**
  ```csharp
  byte[] pdfBytes = pdf.BinaryData;
  ```
  **Why:** `PdfDocument.BinaryData` is the equivalent of the previous response body.

- [ ] **Use `.SaveAs()` instead of `File.WriteAllBytes()`**
  ```csharp
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Cleaner write path; can also stream, encrypt, or merge before saving.

- [ ] **Delete container-reachability error handling**
  **Why:** No HTTP, no socket, no "container down" branch.

### Testing

- [ ] **Test all PDF generation paths**
  **Why:** Verify behavior matches the pre-migration baseline.

- [ ] **Verify header/footer rendering and page numbers**
  **Why:** This is one of the biggest behavioral upgrades — confirm placeholders render.

- [ ] **Validate margins, page size, orientation**
  **Why:** Confirm `RenderingOptions` settings produced the same output as the embedded `@page` CSS.

- [ ] **Test async operations**
  **Why:** Switch from `HttpClient.PostAsync` to `RenderHtmlAsPdfAsync`.

- [ ] **Benchmark performance improvement**
  **Why:** Confirm the HTTP round-trip overhead is gone.

### Post-Migration

- [ ] **Tear down Kaizen container infrastructure**
  **Why:** No longer needed.

- [ ] **Update environment variables / secrets**
  **Why:** Remove `KAIZEN_PDF_LICENSE`, add IronPDF license storage.

- [ ] **Update monitoring/alerting**
  **Why:** Remove container health checks; add IronPDF-specific telemetry.

- [ ] **Document new error patterns**
  **Why:** Errors are now typed exceptions, not HTTP status codes.

---

## Additional Resources

- **Kaizen.io HTML-to-PDF product page**: https://www.kaizen.io/products/html-to-pdf/
- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
