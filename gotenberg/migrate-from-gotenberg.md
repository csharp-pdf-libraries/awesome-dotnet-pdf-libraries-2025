# How Do I Migrate from Gotenberg to IronPDF in C#?

## Table of Contents

1. [Why Migrate from Gotenberg to IronPDF](#why-migrate-from-gotenberg-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Comparison](#performance-comparison)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from Gotenberg to IronPDF

### The Gotenberg Architecture Problem

Gotenberg is a Docker-based microservice architecture for PDF generation. While powerful and flexible, it introduces significant complexity:

1. **Infrastructure Overhead**: Requires Docker, container orchestration (Kubernetes/Docker Compose), service discovery, and load balancing
2. **Network Latency**: Every PDF operation requires an HTTP call to a separate service—adds 10-100ms+ per request
3. **Cold Start Issues**: Container startup can add 2-5 seconds to first requests; even warm containers have network overhead
4. **Operational Complexity**: Need to manage container health, scaling, logging, and monitoring as separate concerns
5. **Failure Points**: Network timeouts, service unavailability, and container crashes all become your problem
6. **Multipart Form Data**: Every request requires constructing multipart/form-data payloads—verbose and error-prone
7. **Version Management**: Gotenberg images update separately from your application; API changes can break integrations
8. **Resource Isolation**: Running a separate Chromium instance in a container consumes significant memory

### What IronPDF Offers Instead

| Aspect | Gotenberg | IronPDF |
|--------|-----------|---------|
| Deployment | Docker container + orchestration | Single NuGet package |
| Latency | Network round-trip (10-100ms+) | In-process (< 1ms overhead) |
| Cold Start | Container init (2-5 seconds) | First-render only (~1-2s) |
| Infrastructure | Docker, Kubernetes, load balancers | None required |
| Failure Modes | Network, container, service failures | Standard .NET exceptions |
| API Style | REST multipart/form-data | Native C# method calls |
| Scaling | Horizontal (more containers) | Vertical (in-process) |
| Resource Management | Separate container resources | Shared application resources |
| Debugging | Distributed tracing needed | Standard debugger |
| Version Control | Container image tags | NuGet package versions |

---

## Before You Start

### Prerequisites

1. **.NET Version**: IronPDF supports .NET Framework 4.6.2+ and .NET Core 3.1+ / .NET 5+
2. **License Key**: Obtain from [IronPDF website](https://ironpdf.com/licensing/)
3. **Remove Infrastructure**: Plan to decommission Gotenberg containers post-migration

### Identify Gotenberg Usage

Find all Gotenberg API calls in your codebase:

```bash
# Find direct HTTP calls to Gotenberg
grep -r "gotenberg\|/forms/chromium\|/forms/libreoffice\|/forms/pdfengines" --include="*.cs" .

# Find GotenbergSharpApiClient usage
grep -r "GotenbergSharpClient\|Gotenberg.Sharp\|ChromiumRequest\|MergeBuilder" --include="*.cs" .

# Find Docker/Kubernetes Gotenberg configuration
grep -r "gotenberg/gotenberg\|gotenberg:" --include="*.yml" --include="*.yaml" .
```

### Dependency Audit

Check which Gotenberg client library you're using:

```bash
# Check NuGet packages
grep -r "Gotenberg.Sharp.API.Client" --include="*.csproj" .

# Check package.json or other config
grep -r "gotenberg" --include="*.json" .
```

---

## Quick Start Migration

### Step 1: Install IronPDF

```bash
# Remove Gotenberg client (if using)
dotnet remove package Gotenberg.Sharp.API.Client

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Code

**Before (Gotenberg with HttpClient):**
```csharp
using System.Net.Http;

public class PdfService
{
    private readonly HttpClient _client;
    private readonly string _gotenbergUrl = "http://gotenberg:3000";

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(html), "files", "index.html");
        content.Add(new StringContent("0.5"), "marginTop");
        content.Add(new StringContent("0.5"), "marginBottom");

        var response = await _client.PostAsync(
            $"{_gotenbergUrl}/forms/chromium/convert/html", content);
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
        _renderer.RenderingOptions.MarginTop = 12.7;  // 0.5 inches = 12.7mm
        _renderer.RenderingOptions.MarginBottom = 12.7;
    }

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }

    // Async version if needed
    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        return await Task.Run(() => GeneratePdf(html));
    }
}
```

### Step 3: Remove Infrastructure

```yaml
# REMOVE from docker-compose.yml:
# services:
#   gotenberg:
#     image: gotenberg/gotenberg:8
#     ports:
#       - 3000:3000
```

---

## Complete API Reference

### Gotenberg Endpoint to IronPDF Mapping

| Gotenberg Route | IronPDF Equivalent | Notes |
|-----------------|-------------------|-------|
| `POST /forms/chromium/convert/html` | `ChromePdfRenderer.RenderHtmlAsPdf()` | HTML string to PDF |
| `POST /forms/chromium/convert/url` | `ChromePdfRenderer.RenderUrlAsPdf()` | URL to PDF |
| `POST /forms/chromium/convert/markdown` | Render Markdown as HTML first | Use Markdig library |
| `POST /forms/libreoffice/convert` | N/A (use different approach) | IronPDF is HTML-focused |
| `POST /forms/pdfengines/merge` | `PdfDocument.Merge()` | Merge multiple PDFs |
| `POST /forms/pdfengines/convert` | `pdf.SaveAs()` with settings | PDF/A conversion |
| `POST /forms/pdfengines/metadata/read` | `pdf.MetaData` | Read metadata |
| `POST /forms/pdfengines/metadata/write` | `pdf.MetaData.Author = "..."` | Write metadata |
| `GET /health` | N/A | No external service |

### Form Parameter to RenderingOptions Mapping

| Gotenberg Parameter | IronPDF Property | Conversion Notes |
|--------------------|------------------|------------------|
| `paperWidth` (inches) | `RenderingOptions.SetCustomPaperSizeInInches()` | Use method for custom |
| `paperHeight` (inches) | `RenderingOptions.SetCustomPaperSizeInInches()` | Use method for custom |
| `marginTop` (inches) | `RenderingOptions.MarginTop` | Multiply by 25.4 for mm |
| `marginBottom` (inches) | `RenderingOptions.MarginBottom` | Multiply by 25.4 for mm |
| `marginLeft` (inches) | `RenderingOptions.MarginLeft` | Multiply by 25.4 for mm |
| `marginRight` (inches) | `RenderingOptions.MarginRight` | Multiply by 25.4 for mm |
| `preferCssPageSize` | `RenderingOptions.UseCssPageSize` | Boolean |
| `printBackground` | `RenderingOptions.PrintHtmlBackgrounds` | Boolean |
| `landscape` | `RenderingOptions.PaperOrientation` | `Landscape` enum |
| `scale` | `RenderingOptions.Zoom` | Percentage (100 = 1.0) |
| `nativePageRanges` | `pdf.CopyPages()` | Extract specific pages |
| `waitDelay` | `RenderingOptions.RenderDelay` | Convert to milliseconds |
| `waitForExpression` | `RenderingOptions.WaitFor` | JavaScript expression |
| `emulatedMediaType` | `RenderingOptions.CssMediaType` | `Screen` or `Print` |
| `cookies` | `RenderingOptions.CustomCookies` | Dictionary |
| `extraHttpHeaders` | `RenderingOptions.CustomHeaders` | Dictionary |
| `failOnHttpStatusCodes` | N/A (handle in code) | Check response manually |
| `failOnConsoleExceptions` | `RenderingOptions.ThrowOnJavaScriptExceptions` | Boolean |
| `skipNetworkIdleEvent` | N/A | IronPDF handles automatically |
| `pdfFormat` (PDF/A) | `pdf.SaveAsPdfA()` | Save as PDF/A |
| `pdfua` | `pdf.SaveAsPdfUA()` | Save as PDF/UA |

### GotenbergSharpApiClient to IronPDF Mapping

If you're using the GotenbergSharpApiClient NuGet package:

| GotenbergSharp Class/Method | IronPDF Equivalent |
|----------------------------|-------------------|
| `GotenbergSharpClient` | `ChromePdfRenderer` |
| `ChromiumRequest` | `ChromePdfRenderer.RenderingOptions` |
| `UrlRequest.Create(url)` | `renderer.RenderUrlAsPdf(url)` |
| `HtmlRequest.Create(html)` | `renderer.RenderHtmlAsPdf(html)` |
| `MarkdownRequest` | Convert to HTML first |
| `MergeBuilder.Create()` | `PdfDocument.Merge()` |
| `.WithDimensions(dim)` | `RenderingOptions.PaperSize` |
| `.WithMargins(margin)` | `RenderingOptions.MarginTop/Bottom/Left/Right` |
| `.WithPageRanges(range)` | `pdf.CopyPages()` |
| `.WithScale(scale)` | `RenderingOptions.Zoom` |
| `.WithWaitDelay(delay)` | `RenderingOptions.RenderDelay` |
| `.WithCookies(cookies)` | `RenderingOptions.CustomCookies` |
| `.WithExtraHttpHeaders(headers)` | `RenderingOptions.CustomHeaders` |
| `client.BuildRequestAsync()` | N/A (direct method call) |
| `client.PostAsync(request)` | Synchronous call to `Render*` |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (Gotenberg HttpClient):**
```csharp
using System.Net.Http;
using System.Text;

public async Task<byte[]> HtmlToPdfGotenberg(string htmlContent)
{
    using var client = new HttpClient();
    using var content = new MultipartFormDataContent();

    // Create HTML file content
    var htmlBytes = Encoding.UTF8.GetBytes(htmlContent);
    var htmlStream = new ByteArrayContent(htmlBytes);
    htmlStream.Headers.ContentType = new MediaTypeHeaderValue("text/html");
    content.Add(htmlStream, "files", "index.html");

    var response = await client.PostAsync(
        "http://gotenberg:3000/forms/chromium/convert/html", content);

    if (!response.IsSuccessStatusCode)
        throw new Exception($"Gotenberg error: {response.StatusCode}");

    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] HtmlToPdfIronPdf(string htmlContent)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(htmlContent);
    return pdf.BinaryData;
}
```

### Example 2: URL to PDF with Custom Options

**Before (Gotenberg):**
```csharp
public async Task<byte[]> UrlToPdfGotenberg(string url)
{
    using var client = new HttpClient();
    using var content = new MultipartFormDataContent();

    content.Add(new StringContent(url), "url");
    content.Add(new StringContent("true"), "landscape");
    content.Add(new StringContent("8.5"), "paperWidth");
    content.Add(new StringContent("11"), "paperHeight");
    content.Add(new StringContent("1"), "marginTop");
    content.Add(new StringContent("1"), "marginBottom");
    content.Add(new StringContent("0.5"), "marginLeft");
    content.Add(new StringContent("0.5"), "marginRight");
    content.Add(new StringContent("3s"), "waitDelay");
    content.Add(new StringContent("true"), "printBackground");

    var response = await client.PostAsync(
        "http://gotenberg:3000/forms/chromium/convert/url", content);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] UrlToPdfIronPdf(string url)
{
    var renderer = new ChromePdfRenderer();

    // Paper and orientation
    renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

    // Margins (convert inches to mm: multiply by 25.4)
    renderer.RenderingOptions.MarginTop = 25.4;      // 1 inch
    renderer.RenderingOptions.MarginBottom = 25.4;   // 1 inch
    renderer.RenderingOptions.MarginLeft = 12.7;     // 0.5 inch
    renderer.RenderingOptions.MarginRight = 12.7;    // 0.5 inch

    // Wait delay (convert "3s" to milliseconds)
    renderer.RenderingOptions.RenderDelay = 3000;

    // Print background
    renderer.RenderingOptions.PrintHtmlBackgrounds = true;

    var pdf = renderer.RenderUrlAsPdf(url);
    return pdf.BinaryData;
}
```

### Example 3: Merging PDFs

**Before (Gotenberg):**
```csharp
public async Task<byte[]> MergePdfsGotenberg(List<string> filePaths)
{
    using var client = new HttpClient();
    using var content = new MultipartFormDataContent();

    int i = 1;
    foreach (var filePath in filePaths)
    {
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "files", $"{i++}.pdf");
    }

    var response = await client.PostAsync(
        "http://gotenberg:3000/forms/pdfengines/merge", content);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] MergePdfsIronPdf(List<string> filePaths)
{
    var pdfs = filePaths.Select(path => PdfDocument.FromFile(path)).ToList();
    var merged = PdfDocument.Merge(pdfs);

    // Dispose source documents
    foreach (var pdf in pdfs)
        pdf.Dispose();

    return merged.BinaryData;
}
```

### Example 4: PDF with Headers and Footers

**Before (Gotenberg):**
```csharp
public async Task<byte[]> PdfWithHeaderFooterGotenberg(string html)
{
    using var client = new HttpClient();
    using var content = new MultipartFormDataContent();

    // Main content
    content.Add(new StringContent(html), "files", "index.html");

    // Header HTML file
    var headerHtml = "<div style='font-size:10px;'>Company Name - Confidential</div>";
    content.Add(new StringContent(headerHtml), "files", "header.html");

    // Footer HTML file
    var footerHtml = "<div style='font-size:9px;text-align:center;'>" +
        "Page <span class='pageNumber'></span> of <span class='totalPages'></span></div>";
    content.Add(new StringContent(footerHtml), "files", "footer.html");

    var response = await client.PostAsync(
        "http://gotenberg:3000/forms/chromium/convert/html", content);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] PdfWithHeaderFooterIronPdf(string html)
{
    var renderer = new ChromePdfRenderer();

    // Header
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
    {
        HtmlFragment = "<div style='font-size:10px;'>Company Name - Confidential</div>",
        DrawDividerLine = false
    };

    // Footer with page numbers
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
    {
        HtmlFragment = "<div style='font-size:9px;text-align:center;'>" +
            "Page {page} of {total-pages}</div>",
        DrawDividerLine = false
    };

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 5: GotenbergSharpApiClient Migration

**Before (GotenbergSharpApiClient):**
```csharp
using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;

public class PdfService
{
    private readonly GotenbergSharpClient _client;

    public PdfService(IHttpClientFactory factory)
    {
        _client = new GotenbergSharpClient(
            factory.CreateClient(),
            new GotenbergSharpClientOptions { ServiceUrl = "http://gotenberg:3000" });
    }

    public async Task<byte[]> GenerateInvoicePdf(string html)
    {
        var request = await new ChromiumRequest()
            .SetHtml(html)
            .WithDimensions(builder => builder
                .UseLetter()
                .MarginInInches(1, 1, 0.5, 0.5)
                .Landscape())
            .WithWaitDelay(TimeSpan.FromSeconds(2))
            .WithPrintBackground()
            .BuildAsync();

        return await _client.PostAsync(request);
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

        // Configure once, reuse
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
        _renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        _renderer.RenderingOptions.MarginTop = 25.4;      // 1 inch
        _renderer.RenderingOptions.MarginBottom = 25.4;
        _renderer.RenderingOptions.MarginLeft = 12.7;     // 0.5 inch
        _renderer.RenderingOptions.MarginRight = 12.7;
        _renderer.RenderingOptions.RenderDelay = 2000;
        _renderer.RenderingOptions.PrintHtmlBackgrounds = true;
    }

    public byte[] GenerateInvoicePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Example 6: PDF with Authentication (Cookies/Headers)

**Before (Gotenberg):**
```csharp
public async Task<byte[]> SecureUrlToPdfGotenberg(string url, string authToken)
{
    using var client = new HttpClient();
    using var content = new MultipartFormDataContent();

    content.Add(new StringContent(url), "url");

    // Add custom headers as JSON
    var headers = new Dictionary<string, string>
    {
        { "Authorization", $"Bearer {authToken}" },
        { "X-Custom-Header", "value" }
    };
    content.Add(new StringContent(JsonSerializer.Serialize(headers)), "extraHttpHeaders");

    // Add cookies
    var cookies = new[]
    {
        new { name = "session", value = "abc123", domain = "example.com" }
    };
    content.Add(new StringContent(JsonSerializer.Serialize(cookies)), "cookies");

    var response = await client.PostAsync(
        "http://gotenberg:3000/forms/chromium/convert/url", content);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] SecureUrlToPdfIronPdf(string url, string authToken)
{
    var renderer = new ChromePdfRenderer();

    // Custom headers
    renderer.RenderingOptions.CustomHeaders = new Dictionary<string, string>
    {
        { "Authorization", $"Bearer {authToken}" },
        { "X-Custom-Header", "value" }
    };

    // Cookies
    renderer.RenderingOptions.CustomCookies = new Dictionary<string, string>
    {
        { "session", "abc123" }
    };

    var pdf = renderer.RenderUrlAsPdf(url);
    return pdf.BinaryData;
}
```

### Example 7: PDF/A Compliance

**Before (Gotenberg):**
```csharp
public async Task<byte[]> GeneratePdfAGotenberg(string html)
{
    using var client = new HttpClient();
    using var content = new MultipartFormDataContent();

    content.Add(new StringContent(html), "files", "index.html");
    content.Add(new StringContent("PDF/A-1a"), "pdfFormat");

    var response = await client.PostAsync(
        "http://gotenberg:3000/forms/chromium/convert/html", content);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] GeneratePdfAIronPdf(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Convert to PDF/A
    pdf.SaveAsPdfA("output-pdfa.pdf", PdfAVersions.PdfA1a);

    // Or get bytes
    using var stream = new MemoryStream();
    pdf.SaveAsPdfA(stream, PdfAVersions.PdfA1a);
    return stream.ToArray();
}
```

### Example 8: Batch Processing

**Before (Gotenberg):**
```csharp
public async Task<Dictionary<string, byte[]>> BatchGeneratePdfsGotenberg(
    Dictionary<string, string> documents)
{
    var results = new Dictionary<string, byte[]>();

    using var client = new HttpClient();

    // Must process sequentially or risk overloading Gotenberg
    foreach (var (name, html) in documents)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(html), "files", "index.html");

        var response = await client.PostAsync(
            "http://gotenberg:3000/forms/chromium/convert/html", content);

        if (response.IsSuccessStatusCode)
        {
            results[name] = await response.Content.ReadAsByteArrayAsync();
        }
    }

    return results;
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Concurrent;

public Dictionary<string, byte[]> BatchGeneratePdfsIronPdf(
    Dictionary<string, string> documents)
{
    var results = new ConcurrentDictionary<string, byte[]>();

    // Process in parallel - IronPDF handles concurrency
    Parallel.ForEach(documents, doc =>
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(doc.Value);
        results[doc.Key] = pdf.BinaryData;
        pdf.Dispose();
    });

    return new Dictionary<string, byte[]>(results);
}
```

### Example 9: Office Document to PDF

**Before (Gotenberg with LibreOffice):**
```csharp
public async Task<byte[]> DocxToPdfGotenberg(string docxPath)
{
    using var client = new HttpClient();
    using var content = new MultipartFormDataContent();

    var fileBytes = await File.ReadAllBytesAsync(docxPath);
    var fileContent = new ByteArrayContent(fileBytes);
    fileContent.Headers.ContentType =
        new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
    content.Add(fileContent, "files", Path.GetFileName(docxPath));

    var response = await client.PostAsync(
        "http://gotenberg:3000/forms/libreoffice/convert", content);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF + IronWord):**
```csharp
// For Office documents, use IronWord for DOCX
using IronWord;
using IronPdf;

public byte[] DocxToPdfIronSuite(string docxPath)
{
    // Option 1: Use IronWord for native DOCX handling
    var wordDoc = new WordDocument(docxPath);
    wordDoc.SaveAs("temp.pdf");  // IronWord can export to PDF
    return File.ReadAllBytes("temp.pdf");

    // Option 2: For simple documents, convert to HTML first
    // using DocumentFormat.OpenXml and render with IronPDF
}

// Alternative: Use HTML-based approach
public byte[] WordToHtmlToPdf(string docxContent)
{
    // Convert DOCX to HTML using OpenXML SDK or similar
    // Then render with IronPDF
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(htmlFromWord);
    return pdf.BinaryData;
}
```

### Example 10: Webhook-Style Processing

**Before (Gotenberg with Webhook):**
```csharp
public async Task QueuePdfGenerationGotenberg(string html, string webhookUrl)
{
    using var client = new HttpClient();
    using var content = new MultipartFormDataContent();

    content.Add(new StringContent(html), "files", "index.html");
    content.Add(new StringContent(webhookUrl), "webhookUrl");
    content.Add(new StringContent("POST"), "webhookMethod");

    // Fire and forget - Gotenberg will POST result to webhook
    await client.PostAsync(
        "http://gotenberg:3000/forms/chromium/convert/html", content);
}
```

**After (IronPDF with Background Processing):**
```csharp
using IronPdf;
using System.Threading.Channels;

public class PdfBackgroundService
{
    private readonly Channel<PdfJob> _channel;
    private readonly HttpClient _httpClient;

    public void QueuePdfGeneration(string html, string webhookUrl)
    {
        _channel.Writer.TryWrite(new PdfJob(html, webhookUrl));
    }

    // Background worker
    private async Task ProcessJobsAsync(CancellationToken ct)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(ct))
        {
            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf(job.Html);

            // POST to webhook
            using var content = new ByteArrayContent(pdf.BinaryData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            await _httpClient.PostAsync(job.WebhookUrl, content);

            pdf.Dispose();
        }
    }
}
```

---

## Advanced Scenarios

### Service Registration Pattern Change

**Before (ASP.NET Core with Gotenberg):**
```csharp
// Program.cs / Startup.cs
builder.Services.AddHttpClient<IGotenbergClient, GotenbergClient>(client =>
{
    client.BaseAddress = new Uri(config["Gotenberg:Url"]);
    client.Timeout = TimeSpan.FromMinutes(5);
});

// appsettings.json
{
    "Gotenberg": {
        "Url": "http://gotenberg:3000"
    }
}
```

**After (ASP.NET Core with IronPDF):**
```csharp
// Program.cs / Startup.cs
builder.Services.AddSingleton<IPdfService, IronPdfService>();

// Configure license at startup
IronPdf.License.LicenseKey = builder.Configuration["IronPdf:LicenseKey"];

// Optional: Configure logging
IronPdf.Logging.Logger.LogFilePath = "ironpdf.log";
IronPdf.Logging.Logger.LoggingMode = IronPdf.Logging.Logger.LoggingModes.All;

// appsettings.json - much simpler
{
    "IronPdf": {
        "LicenseKey": "YOUR-LICENSE-KEY"
    }
}
```

### Docker Compose Simplification

**Before (with Gotenberg):**
```yaml
version: '3.8'

services:
  app:
    build: .
    depends_on:
      - gotenberg
    environment:
      - GOTENBERG_URL=http://gotenberg:3000
    networks:
      - pdf-network

  gotenberg:
    image: gotenberg/gotenberg:8
    ports:
      - "3000:3000"
    networks:
      - pdf-network
    deploy:
      resources:
        limits:
          memory: 2G
        reservations:
          memory: 512M
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:3000/health"]
      interval: 30s
      timeout: 10s
      retries: 3

networks:
  pdf-network:
```

**After (IronPDF - no external service needed):**
```yaml
version: '3.8'

services:
  app:
    build: .
    environment:
      - IRONPDF_LICENSE_KEY=${IRONPDF_LICENSE_KEY}
```

### Health Check Migration

**Before (Checking Gotenberg health):**
```csharp
public async Task<bool> IsGotenbergHealthy()
{
    try
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("http://gotenberg:3000/health");
        return response.IsSuccessStatusCode;
    }
    catch
    {
        return false;
    }
}
```

**After (IronPDF - no external health check needed):**
```csharp
public bool IsPdfServiceHealthy()
{
    // IronPDF runs in-process, no external service to check
    // Optionally verify license
    return IronPdf.License.IsLicensed;
}
```

### Error Handling Migration

**Before (Gotenberg):**
```csharp
public async Task<byte[]> GeneratePdfWithRetry(string html)
{
    var retryPolicy = Policy
        .Handle<HttpRequestException>()
        .Or<TaskCanceledException>()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    return await retryPolicy.ExecuteAsync(async () =>
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(html), "files", "index.html");

        var response = await client.PostAsync(
            "http://gotenberg:3000/forms/chromium/convert/html", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Gotenberg error: {response.StatusCode} - {error}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    });
}
```

**After (IronPDF):**
```csharp
public byte[] GeneratePdf(string html)
{
    try
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
    catch (IronPdf.Exceptions.IronPdfException ex)
    {
        // Specific IronPDF exception
        throw new PdfGenerationException("PDF rendering failed", ex);
    }
    catch (Exception ex)
    {
        // General exception
        throw new PdfGenerationException("Unexpected error during PDF generation", ex);
    }

    // No retry policy needed - no network failures possible
}
```

### Unit Conversion Helper

Since Gotenberg uses inches and IronPDF uses millimeters:

```csharp
public static class UnitConverter
{
    public static double InchesToMillimeters(double inches) => inches * 25.4;
    public static double InchesToPoints(double inches) => inches * 72;

    // Migrate Gotenberg margin string (e.g., "0.5") to IronPDF mm
    public static double GotenbergMarginToIronPdf(string gotenbergMargin)
    {
        if (double.TryParse(gotenbergMargin, out var inches))
            return InchesToMillimeters(inches);
        return 25.4; // Default 1 inch
    }

    // Migrate Gotenberg wait delay (e.g., "3s", "500ms") to IronPDF milliseconds
    public static int GotenbergDelayToIronPdf(string gotenbergDelay)
    {
        if (gotenbergDelay.EndsWith("ms"))
            return int.Parse(gotenbergDelay.Replace("ms", ""));
        if (gotenbergDelay.EndsWith("s"))
            return int.Parse(gotenbergDelay.Replace("s", "")) * 1000;
        return 0;
    }
}
```

---

## Performance Comparison

### Latency Comparison

| Operation | Gotenberg (Warm) | Gotenberg (Cold) | IronPDF (First) | IronPDF (Subsequent) |
|-----------|------------------|------------------|-----------------|----------------------|
| Simple HTML | 150-300ms | 2-5s | 1-2s | 50-150ms |
| Complex HTML | 500-1500ms | 3-7s | 1.5-3s | 200-800ms |
| URL Render | 1-5s | 3-10s | 1-5s | 500ms-3s |
| Merge 10 PDFs | 200-500ms | 2-5s | 100-300ms | 100-300ms |

### Resource Usage

| Resource | Gotenberg | IronPDF |
|----------|-----------|---------|
| Container Memory | 512MB-2GB | N/A (in-process) |
| Application Memory | ~50MB (HTTP client) | ~200-500MB (Chromium) |
| Network | Required | None |
| Disk I/O | Container layers | None |
| CPU | Separate process | Shared with app |

### Throughput (requests/minute)

| Scenario | Gotenberg (1 container) | Gotenberg (3 containers) | IronPDF |
|----------|------------------------|--------------------------|---------|
| Simple PDFs | 100-200 | 300-500 | 200-400 |
| Complex PDFs | 20-50 | 60-150 | 40-100 |
| Mixed Workload | 50-100 | 150-300 | 100-200 |

---

## Troubleshooting

### Issue 1: "First PDF takes a long time"

**Symptom**: First PDF generation after application start takes 1-2 seconds

**Solution**: Pre-warm IronPDF at startup:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Pre-warm IronPDF during startup
IronPdf.License.LicenseKey = "YOUR-KEY";
var warmup = new ChromePdfRenderer();
warmup.RenderHtmlAsPdf("<html><body>Warmup</body></html>");

var app = builder.Build();
```

### Issue 2: "Memory usage higher than expected"

**Symptom**: Application uses more memory than before

**Cause**: IronPDF's Chromium engine runs in-process instead of a separate container

**Solution**:
```csharp
// Dispose PDFs after use
using (var pdf = renderer.RenderHtmlAsPdf(html))
{
    return pdf.BinaryData;
}

// Or explicitly dispose
var pdf = renderer.RenderHtmlAsPdf(html);
var bytes = pdf.BinaryData;
pdf.Dispose();
return bytes;
```

### Issue 3: "Margins look different"

**Symptom**: PDF margins don't match Gotenberg output

**Cause**: Gotenberg uses inches, IronPDF uses millimeters

**Solution**:
```csharp
// Gotenberg: marginTop = "0.5" (inches)
// IronPDF: multiply by 25.4
renderer.RenderingOptions.MarginTop = 0.5 * 25.4; // 12.7mm
```

### Issue 4: "Wait delay not working"

**Symptom**: JavaScript content not fully rendered

**Cause**: Gotenberg uses string format ("3s"), IronPDF uses milliseconds

**Solution**:
```csharp
// Gotenberg: waitDelay = "3s"
// IronPDF: use milliseconds
renderer.RenderingOptions.RenderDelay = 3000;

// For more control, use JavaScript wait condition
renderer.RenderingOptions.WaitFor.JavaScript("window.dataLoaded === true");
```

### Issue 5: "Page numbers use wrong format"

**Symptom**: Page numbers not rendering in footers

**Cause**: Gotenberg uses `.pageNumber` CSS class, IronPDF uses `{page}` placeholder

**Solution**:
```csharp
// Gotenberg footer:
// <span class="pageNumber"></span> of <span class="totalPages"></span>

// IronPDF footer:
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "<span>Page {page} of {total-pages}</span>"
};
```

### Issue 6: "Authentication headers not working"

**Symptom**: Authenticated URLs return 401/403

**Solution**:
```csharp
renderer.RenderingOptions.CustomHeaders = new Dictionary<string, string>
{
    { "Authorization", "Bearer your-token" }
};

// Or use cookies
renderer.RenderingOptions.CustomCookies = new Dictionary<string, string>
{
    { "session", "cookie-value" }
};
```

### Issue 7: "PDF/A conversion fails"

**Symptom**: SaveAsPdfA throws exception

**Solution**:
```csharp
// Ensure fonts are embedded
renderer.RenderingOptions.InputEncoding = Encoding.UTF8;

// Use specific PDF/A version
pdf.SaveAsPdfA("output.pdf", PdfAVersions.PdfA3b); // Try different versions
```

### Issue 8: "Office document conversion doesn't work"

**Symptom**: No LibreOffice equivalent in IronPDF

**Solution**: IronPDF focuses on HTML-to-PDF. For Office documents:

```csharp
// Option 1: Use IronWord for DOCX
// Option 2: Use IronXL for Excel
// Option 3: Use a conversion service to HTML first, then IronPDF

// Example with OpenXML to HTML conversion
public byte[] ConvertDocxToPdf(string docxPath)
{
    // Convert DOCX to HTML using OpenXML SDK
    string html = ConvertDocxToHtml(docxPath);

    // Render HTML to PDF
    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Gotenberg API calls in codebase**
  ```bash
  grep -r "Gotenberg" --include="*.cs" .
  grep -r "HttpClient\|PostAsync" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current Gotenberg configuration (timeouts, margins, etc.)**
  ```csharp
  // Find patterns like:
  var config = new GotenbergConfig {
      Timeout = TimeSpan.FromSeconds(30),
      Margins = "1in"
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Identify all Docker/Kubernetes Gotenberg configuration**
  **Why:** Plan for removing unnecessary infrastructure components post-migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Test IronPDF in development environment**
  **Why:** Ensure compatibility and performance in a controlled setting before full migration.

- [ ] **Plan infrastructure decommissioning**
  **Why:** Reduce overhead by removing unused Docker/Kubernetes resources after migration.

### Code Migration

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project for PDF generation capabilities.

- [ ] **Remove Gotenberg client packages (Gotenberg.Sharp.API.Client, RestSharp for Gotenberg)**
  ```bash
  dotnet remove package Gotenberg.Sharp.API.Client
  dotnet remove package RestSharp
  ```
  **Why:** Clean up dependencies no longer needed after migration.

- [ ] **Replace all HTTP calls to Gotenberg with IronPDF method calls**
  ```csharp
  // Before (Gotenberg)
  var client = new HttpClient();
  var response = await client.PostAsync("http://gotenberg:3000/forms/chromium/convert/html", content);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(htmlContent);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Switch to in-process PDF generation with IronPDF for reduced latency and complexity.

- [ ] **Convert margin units from inches to millimeters**
  ```csharp
  // Before (Gotenberg)
  var margins = "1in";

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 25.4;
  renderer.RenderingOptions.MarginBottom = 25.4;
  renderer.RenderingOptions.MarginLeft = 25.4;
  renderer.RenderingOptions.MarginRight = 25.4;
  ```
  **Why:** IronPDF uses millimeters for margin settings, ensuring precise layout control.

- [ ] **Convert wait delays from strings to milliseconds**
  ```csharp
  // Before (Gotenberg)
  var delay = "2000";

  // After (IronPDF)
  renderer.RenderingOptions.WaitFor.JavaScript(2000);
  ```
  **Why:** IronPDF uses integer milliseconds for JavaScript execution delays, providing clear timing control.

- [ ] **Update header/footer page number placeholders**
  ```csharp
  // Before (Gotenberg)
  var header = "Page [page] of [topage]";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML with placeholders for dynamic header/footer content.

- [ ] **Update error handling (HTTP errors → .NET exceptions)**
  ```csharp
  // Before (Gotenberg)
  if (!response.IsSuccessStatusCode)
  {
      throw new Exception("Failed to generate PDF");
  }

  // After (IronPDF)
  try
  {
      var pdf = renderer.RenderHtmlAsPdf(htmlContent);
  }
  catch (Exception ex)
  {
      Console.WriteLine("PDF generation failed: " + ex.Message);
  }
  ```
  **Why:** Handle exceptions directly in .NET without relying on HTTP status codes.

- [ ] **Migrate authentication (cookies, headers)**
  ```csharp
  // Before (Gotenberg)
  client.DefaultRequestHeaders.Add("Authorization", "Bearer token");

  // After (IronPDF)
  // No direct equivalent needed; manage authentication at application level if required.
  ```
  **Why:** Simplify authentication management by handling it within your application logic.

- [ ] **Add license key initialization at startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Configuration Migration

- [ ] **Remove Gotenberg URL from configuration**
  **Why:** Eliminate unnecessary configuration settings post-migration.

- [ ] **Add IronPDF license key to configuration**
  **Why:** Ensure the license key is accessible for IronPDF initialization.

- [ ] **Remove Gotenberg from Docker Compose / Kubernetes**
  **Why:** Reduce infrastructure complexity by removing unused services.

- [ ] **Update CI/CD pipelines (remove Gotenberg image pulls)**
  **Why:** Streamline build and deployment processes by removing unnecessary steps.

- [ ] **Remove Gotenberg health checks**
  **Why:** Simplify monitoring by eliminating checks for services no longer in use.

### Testing

- [ ] **Unit test PDF generation**
  **Why:** Verify that PDF generation logic works correctly with IronPDF.

- [ ] **Verify PDF output matches previous output**
  **Why:** Ensure that the migration maintains document fidelity.

- [ ] **Test margin and sizing accuracy**
  **Why:** Confirm that layout adjustments translate correctly in IronPDF.

- [ ] **Test header/footer rendering**
  **Why:** Validate that dynamic content in headers/footers is displayed as expected.

- [ ] **Test page number formatting**
  **Why:** Ensure page numbers are correctly formatted in the new system.

- [ ] **Performance test under load**
  **Why:** Assess IronPDF's performance and scalability under typical usage conditions.

- [ ] **Test first-render warmup time**
  **Why:** Measure initial PDF generation time to anticipate user experience impacts.

### Post-Migration

- [ ] **Remove Gotenberg container deployments**
  **Why:** Free up resources by decommissioning unused containers.

- [ ] **Archive Gotenberg configuration files**
  **Why:** Retain configuration for historical reference or rollback if needed.

- [ ] **Update documentation**
  **Why:** Reflect changes in system architecture and usage guidelines.

- [ ] **Monitor application memory usage**
  **Why:** Ensure IronPDF's in-process operations do not adversely affect application performance.

- [ ] **Monitor PDF generation latency**
  **Why:** Track performance to ensure user expectations are met.

- [ ] **Verify no orphaned network connections**
  **Why:** Confirm that all network dependencies have been properly removed.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [Rendering Options Reference](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderOptions.html)
- [PDF/A Compliance Guide](https://ironpdf.com/how-to/pdfa-pdf-ua-validation/)

---

## Summary

Migrating from Gotenberg to IronPDF eliminates infrastructure complexity by moving PDF generation from a separate Docker service to an in-process library. The key changes are:

1. **Architecture**: REST API calls → Native C# methods
2. **Infrastructure**: Docker containers → NuGet package
3. **Units**: Inches → Millimeters for margins
4. **Delays**: String format ("3s") → Milliseconds (3000)
5. **Page Numbers**: CSS classes → Placeholders ({page}, {total-pages})
6. **Error Handling**: HTTP status codes → .NET exceptions
7. **Resource Management**: Container scaling → In-process disposal

The result is simpler deployment, lower latency, fewer failure points, and easier debugging—all while maintaining full HTML/CSS/JavaScript rendering capabilities through IronPDF's embedded Chromium engine.
