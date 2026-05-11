---
title: "Kaizen.io vs IronPDF: an unbiased look for .NET teams"
published: false
tags: dotnet, csharp, pdf, comparison
---

Kaizen.io HTML-to-PDF ships as a self-hosted Docker image (`kaizenio.azurecr.io/html-to-pdf`) that exposes one REST endpoint: `POST /html-to-pdf`. There is no official .NET SDK or NuGet package, so every C# call site is a hand-rolled `HttpClient` POST against the running container. That single architectural fact drives most of what follows: HTTP serialization on every PDF, an HTML-only request body at v1.x, no header/footer/page-number fields, and a container you have to operate alongside your .NET app.

IronPDF takes the other path: a NuGet package that hosts Chromium inside your process, returns a `PdfDocument`, and exposes paper size, margins, headers, footers, and placeholders as typed options. This article compares the two head-to-head so you can decide which model fits your stack.

## Understanding IronPDF

IronPDF is a native .NET library that embeds a Chrome rendering engine directly into your application process. Unlike containerized services, there's no HTTP layer, no container runtime dependency, and no network serialization overhead. The library handles [HTML to PDF conversion](https://ironpdf.com/examples/using-html-to-create-a-pdf/) in-process, returning a `PdfDocument` object you can manipulate further before saving.

For .NET applications, this architecture eliminates the operational burden of running and monitoring a separate PDF generation service. Your PDF code runs in the same memory space as your business logic, uses the same error handling patterns, and scales with your application's existing infrastructure—no additional containers to orchestrate or network calls to optimize.

## Key Constraints of Kaizen.io HTML-to-PDF

### Product Status
Kaizen.io HTML-to-PDF is a commercial container-based service distributed through `kaizenio.azurecr.io`. Confirm current release cadence, security update policy, and support terms against the [vendor product page](https://www.kaizen.io/products/html-to-pdf/) before production deployment.

### Capability Scope (v1.x)
- **No .NET SDK or NuGet package**: integration is `HttpClient` + JSON POST against `POST /html-to-pdf` on port 8080.
- **Container runtime required**: the rendering engine ships as a Docker image; the .NET app and the container have to be co-located or networked.
- **HTML-only request body documented**: the v1.x JSON request accepts an `html` field. URL-to-PDF, headers/footers, page size, orientation, and margin fields are listed as roadmap items rather than shipping features.
- **No `{page}` / `{total}` placeholders in the API**: page-X-of-Y is not expressible through the request body at v1.x.
- **Free tier watermarks output**: without the `KAIZEN_PDF_LICENSE` environment variable, generated PDFs carry a watermark.

### Architectural Trade-offs
The HTTP boundary adds steps that an in-process library does not: JSON serialization on every call, status-code inspection, and a separate container lifecycle to monitor. For synchronous PDF paths (user-facing invoice download, real-time report generation), the localhost HTTP round-trip is fixed overhead per request. Error handling has to account for container reachability and HTTP status codes in addition to rendering errors.

## Feature Comparison Overview

| Dimension | Kaizen.io | IronPDF |
|-----------|-----------|---------|
| **Distribution** | Commercial Docker image | Commercial .NET library (NuGet) |
| **Rendering Engine** | Chromium inside container | Chromium in-process |
| **.NET Integration** | Raw `HttpClient` + JSON | `ChromePdfRenderer` typed API |
| **Installation** | `docker pull` + `docker run` | `dotnet add package IronPdf` |
| **API Surface (v1.x)** | Single endpoint, `html` field | Full `RenderingOptions` object |
| **License Model** | One-time per Kaizen license (free tier watermarks) | Commercial license |

## Code Comparison: Core Operations

### Operation 1: HTML String to PDF

#### Kaizen.io — HTML String to PDF

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class KaizenPdfGenerator : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _containerUrl;

    public KaizenPdfGenerator(string containerUrl = "http://localhost:8080")
    {
        _containerUrl = containerUrl;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<byte[]> GenerateFromHtmlString(string htmlContent)
    {
        var requestBody = new
        {
            html = htmlContent
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync(
            $"{_containerUrl}/html-to-pdf",
            jsonContent
        );

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Usage - assumes container running locally
using (var generator = new KaizenPdfGenerator())
{
    var html = "<h1>Invoice #12345</h1><p>Total: $599.99</p>";

    try
    {
        var pdfBytes = await generator.GenerateFromHtmlString(html);
        await File.WriteAllBytesAsync("output.pdf", pdfBytes);
    }
    catch (HttpRequestException ex)
    {
        // Container not running, network issue, or DNS failure
        Console.WriteLine($"Container communication failed: {ex.Message}");
    }
    catch (TaskCanceledException ex)
    {
        // Rendering timeout exceeded
        Console.WriteLine($"PDF generation timeout: {ex.Message}");
    }
}
```

**Integration characteristics:**
- The Kaizen container must be running and reachable before any C# code path that generates a PDF executes.
- Each conversion serializes HTML to JSON, POSTs it to `/html-to-pdf`, and reads the byte stream back.
- The v1.x request body documents an `html` field only, so paper size, margins, and headers/footers have to be inlined into the HTML itself via `@page` CSS and absolute-positioned divs.
- Differentiating a container-reachability failure from a render failure requires HTTP status-code inspection.
- The Chrome lifecycle inside the container is managed by Kaizen, not by your C# code.

#### IronPDF — HTML String to PDF

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var html = "<h1>Invoice #12345</h1><p>Total: $599.99</p>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

IronPDF renders HTML in-process with zero network overhead. The [HTML to PDF documentation](https://ironpdf.com/examples/using-html-to-create-a-pdf/) shows advanced options for margins, page size, and rendering behavior. Chrome engine runs directly in your application's process space.

### Operation 2: URL Rendering with Timeout Configuration

#### Kaizen.io — URL to PDF (workaround)

Kaizen v1.x has no URL-input field on the JSON body — the documented body accepts `html`. To "convert a URL", you fetch the page yourself and POST the resulting HTML.

```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public async Task<byte[]> GenerateFromUrl(string url, int timeoutSeconds = 30)
{
    using var httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(timeoutSeconds + 5)
    };

    // Step 1: fetch the page client-side because Kaizen has no URL endpoint.
    var pageHtml = await httpClient.GetStringAsync(url);

    // Step 2: paper size, orientation and margins live in @page CSS because
    // the v1.x JSON body doesn't expose those fields.
    var wrapped = "<style>@page { size: A4 landscape; margin: 15mm 10mm; }</style>" + pageHtml;

    var jsonContent = new StringContent(
        JsonSerializer.Serialize(new { html = wrapped }),
        Encoding.UTF8,
        "application/json"
    );

    var response = await httpClient.PostAsync(
        "http://localhost:8080/html-to-pdf",
        jsonContent
    );

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"PDF generation failed: {error}");
    }

    return await response.Content.ReadAsByteArrayAsync();
}

// Usage
try
{
    var pdfBytes = await GenerateFromUrl("https://example.com/invoice/12345", timeoutSeconds: 45);
    await File.WriteAllBytesAsync("invoice.pdf", pdfBytes);
}
catch (TaskCanceledException)
{
    // Client-side timeout exceeded.
    Console.WriteLine("URL rendering timeout - consider increasing timeout or optimizing page");
}
catch (HttpRequestException ex)
{
    // Container unreachable or upstream URL fetch failed.
    Console.WriteLine($"HTTP error: {ex.Message}");
}
```

**Integration characteristics:**
- The URL fetch happens in your .NET process, so cookies, auth headers, and proxy rules are your responsibility — not the container's.
- Resource resolution inside the fetched HTML (CSS, images) depends on whether you rewrite relative URLs before POSTing.
- The client timeout has to exceed the container's render time; otherwise the request is cancelled mid-render.
- Paper size, orientation, and margins are configured via `@page` CSS embedded in the HTML rather than as request fields.

#### IronPDF — URL to PDF

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.Timeout = 60; // seconds
renderer.RenderingOptions.MarginTop = 50;
renderer.RenderingOptions.MarginBottom = 50;

var pdf = renderer.RenderUrlAsPdf("https://example.com/invoice/12345");
pdf.SaveAs("invoice.pdf");
```

IronPDF's [URL rendering capabilities](https://ironpdf.com/tutorials/html-to-pdf/) include configurable timeouts, JavaScript wait conditions, and custom HTTP headers. The rendering happens in-process with direct exception handling—no network layer to debug.

### Operation 3: HTML File with External Assets

#### Kaizen.io — HTML File Rendering

```csharp
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public async Task<byte[]> GenerateFromHtmlFile(string filePath)
{
    using var httpClient = new HttpClient();

    // Read the file contents and POST the string. The v1.x JSON body
    // accepts an `html` field; there is no documented base_url field, so
    // relative asset paths inside the file have to be made resolvable to
    // the container (URL-hosted, inlined, or converted to data URIs).
    var htmlContent = await File.ReadAllTextAsync(filePath);

    var requestBody = new { html = htmlContent };

    var jsonContent = new StringContent(
        JsonSerializer.Serialize(requestBody),
        Encoding.UTF8,
        "application/json"
    );

    var response = await httpClient.PostAsync(
        "http://localhost:8080/html-to-pdf",
        jsonContent
    );

    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsByteArrayAsync();
}
```

**Integration characteristics:**
- The container does not share the application's filesystem, so relative `<img>` / `<link>` references will not resolve to local files by default.
- Common workarounds are inlining CSS, embedding images as data URIs, or serving assets from a URL the container can reach.
- Payload size grows with the file because the HTML travels in the JSON body on every call.

#### IronPDF — HTML File to PDF

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlFileAsPdf("invoice-template.html");
pdf.SaveAs("final-invoice.pdf");
```

IronPDF automatically resolves relative asset paths using the HTML file's directory as the base, matching standard browser behavior. See [HTML file rendering documentation](https://ironpdf.com/how-to/html-file-to-pdf/) for details on asset resolution and advanced file handling.

### Operation 4: Concurrent PDF Generation

#### Kaizen.io — Parallel Requests

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public async Task<List<byte[]>> GenerateMultiplePdfs(List<string> htmlContents)
{
    // Concurrency is bounded by the container instance's CPU, memory, and
    // however many Chromium workers the image is configured to run.

    using var httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    var tasks = htmlContents.Select(async html =>
    {
        var requestBody = new { html = html };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.PostAsync(
            "http://localhost:8080/html-to-pdf",
            jsonContent
        );

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    });

    return (await Task.WhenAll(tasks)).ToList();
}

// Usage
var invoices = new List<string>
{
    "<h1>Invoice #001</h1>",
    "<h1>Invoice #002</h1>",
    // ... 100 more invoices
};

try
{
    var pdfs = await GenerateMultiplePdfs(invoices);
    Console.WriteLine($"Generated {pdfs.Count} PDFs");
}
catch (Exception ex)
{
    // Batch failure surfaces as HTTP-layer errors from the container.
    Console.WriteLine($"Batch generation failed: {ex.Message}");
}
```

**Integration characteristics:**
- Throughput is capped by what the single container instance can serve before requests queue.
- Each in-flight request occupies a socket and JSON payload buffer in addition to the renderer slot inside the container.
- Container metrics (queue depth, memory, render time) live in the container's own logs and are not exposed to the calling .NET process by default.
- Horizontal scaling means running additional container replicas with a load balancer in front.

#### IronPDF — Parallel Rendering

```csharp
using IronPdf;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

var htmlContents = new List<string>
{
    "<h1>Invoice #001</h1>",
    "<h1>Invoice #002</h1>",
    // ... 100 more
};

var pdfs = await Task.WhenAll(htmlContents.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await Task.Run(() => renderer.RenderHtmlAsPdf(html));
}));

for (int i = 0; i < pdfs.Length; i++)
{
    pdfs[i].SaveAs($"invoice_{i:000}.pdf");
}
```

IronPDF leverages standard .NET threading with no architectural limits. The [HTML to PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) covers async patterns and batch processing optimization.

## API Mapping Reference

| Kaizen.io Concept | IronPDF Equivalent | Notes |
|-------------------|-------------------|-------|
| Container HTTP endpoint | `new ChromePdfRenderer()` | No endpoint needed |
| POST `/html-to-pdf` with JSON | `RenderHtmlAsPdf(html)` | Direct method call |
| JSON `{html: "..."}` | Method parameter | No serialization |
| URL conversion | `RenderUrlAsPdf(url)` | Native method |
| HTTP timeout configuration | `RenderingOptions.Timeout` | Strongly typed |
| Container memory limits | .NET memory management | Standard GC |
| Docker health checks | Standard error handling | No container lifecycle |
| Container logs | .NET logging/exceptions | Direct stack traces |
| Container restart on failure | Application error handling | In-process recovery |
| Load balancing multiple containers | Thread pool | Standard concurrency |
| Base URL for assets | File path or URL base | Automatic resolution |
| Response byte array | `PdfDocument` object | Rich manipulation API |

## Comprehensive Feature Comparison

| Feature | Kaizen.io | IronPDF |
|---------|-----------|---------|
| **Distribution & Support** | | |
| Distribution Channel | Docker image (`kaizenio.azurecr.io`) | NuGet (`IronPdf`) |
| .NET Framework Support | Via `HttpClient` (any TFM) | Native (.NET 4.6.2+) |
| .NET Core / 6+ / 8+ Support | Via `HttpClient` (any TFM) | Native |
| Commercial Support | See vendor pricing page | Included with license |
| **Content Creation** | | |
| HTML String to PDF | Yes (`html` field) | Yes (`RenderHtmlAsPdf`) |
| HTML File to PDF | Read file, POST contents | `RenderHtmlFileAsPdf` |
| URL to PDF | Not in v1.x API (client-side fetch workaround) | `RenderUrlAsPdf` |
| Custom Headers/Footers | Not in v1.x API (CSS workaround) | `TextHeader` / `HtmlHeader` |
| Page Number Placeholders | Not in v1.x API | `{page}` / `{total-pages}` |
| Asset Base Path | Not in v1.x API | Resolved from file / URL |
| CSS Media Type Control | Not in v1.x API | Configurable |
| JavaScript Execution | Chromium inside container | Configurable with `WaitFor` |
| **PDF Operations** | | |
| Merge PDFs | Not documented | Native |
| Split PDFs | Not documented | Native |
| Add Watermarks | Not documented | Native |
| Form Filling | Not documented | Comprehensive |
| Extract Text | Not documented | Native |
| Digital Signatures | Not documented | Native |
| **Security** | | |
| Password Protection | Not documented | `SecuritySettings.UserPassword` |
| Permissions Control | Not documented | Granular |
| Data Path | Local container | In-process |
| **Development** | | |
| Installation | Docker + container pull | NuGet package |
| Deployment Footprint | Container runtime | Library |
| Local Development | Container running | `F5` in Visual Studio |
| Production Scaling | Container replicas + LB | App instances |
| Debugging | Container logs + HTTP traces | Standard .NET debugging |
| Error Surface | HTTP status codes | Typed exceptions |
| **Architectural Properties** | | |
| Cold Start | Container pull / start | Chromium init on first render |
| Per-Call Overhead | HTTP serialization round-trip | Direct method call |
| Memory Model | Container memory limits | Standard .NET GC |
| Port Allocation | Requires a host port | Not applicable |

## Installation Comparison

**Kaizen.io:**
```bash
# Pull container from Azure registry
docker pull kaizenio.azurecr.io/html-to-pdf:latest

# Run container locally
docker run -d --rm -p 8080:8080 \
  -e KAIZEN_PDF_LICENSE=your_license_key \
  --pull=always --name kaizen-pdf \
  kaizenio.azurecr.io/html-to-pdf:latest

# In .NET project - no package, use HttpClient
```
```csharp
using System.Net.Http;
using System.Text.Json;
```

**IronPDF:**
```bash
dotnet add package IronPdf
```
```csharp
using IronPdf;
```

## Conclusion

Kaizen.io HTML-to-PDF fits teams that already operate container infrastructure and want PDF generation as a separately deployed service. The per-license one-time price and the ability to run the renderer outside the .NET process are real advantages in that context.

For .NET teams without an existing container platform, the Docker dependency and the v1.x API surface — `html`-only request body, no headers / footers / URL input / page-number placeholders — push significant complexity back into your application code. Paper layout becomes `@page` CSS embedded in every document, page-X-of-Y is not expressible through the API, and error handling has to distinguish container reachability from render failure.

Migration becomes attractive when the operational cost of running the container outweighs the value of architectural separation, when per-call HTTP serialization is in your hot path, or when you need PDF features beyond HTML-to-PDF (merge, split, watermark, signatures) without adding more services.

IronPDF provides those operations as a NuGet package with an [embedded Chrome engine](https://ironpdf.com/object-reference/api/index.html). The [HTML to PDF conversion](https://ironpdf.com/examples/using-html-to-create-a-pdf/) runs in-process with no runtime dependencies beyond .NET itself.

**Have you found container-based PDF services worth the orchestration overhead, or do you prefer in-process execution?**

For migration guidance, see the [HTML string to PDF documentation](https://ironpdf.com/how-to/html-string-to-pdf/) and [comprehensive API reference](https://ironpdf.com/object-reference/api/index.html).
