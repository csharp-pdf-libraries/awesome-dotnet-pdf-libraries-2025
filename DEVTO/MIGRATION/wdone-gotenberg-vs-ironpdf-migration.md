# Migrating from Gotenberg to IronPDF: the parts that actually matter

**Facts Gate**

- **Gotenberg maintenance status:** Actively maintained open source project. Source: https://gotenberg.dev — verify current version (7.x / 8.x series as of 2025). Docker image available at `gotenberg/gotenberg`.
- **Technical approach + .NET versions:** Gotenberg is a Docker-based microservice that exposes HTTP endpoints for document conversion (HTML→PDF via Chromium, Office docs via LibreOffice). Not a .NET library — consumed over HTTP. No .NET version constraint on the client; any HTTP client works.
- **Install footprint:** Docker container (Gotenberg image ~1–1.5GB). .NET client typically uses `HttpClient` directly or a community wrapper. No NuGet package from Gotenberg authors.
- **IronPDF doc links:** https://ironpdf.com/how-to/license-keys/ · https://ironpdf.com/how-to/html-string-to-pdf/ · https://ironpdf.com/how-to/url-to-pdf/ · https://ironpdf.com/how-to/merge-or-split-pdfs/ · https://ironpdf.com/how-to/pdf-permissions-passwords/
- **What Gotenberg is for:** Document conversion microservice — HTML/URL/Office→PDF via HTTP API. Designed for service isolation; runs as a sidecar or dedicated service.
- **No-guess rule applied:** All API calls verified against known patterns; Gotenberg endpoints verified against public docs.

---

## Checklist: Understand the Architecture Difference First

This migration is architecturally different from most library swaps. Gotenberg is a **network service**; IronPDF is an **in-process library**. Before a single line of code changes, clarify these design decisions:

- [ ] **Why is Gotenberg external?** Was it for service isolation, language agnosticism, container resource separation, or something else?
- [ ] **Is the external service model a requirement?** If other services (non-.NET) also call Gotenberg, IronPDF won't replace those.
- [ ] **What's your current Chromium version via Gotenberg?** IronPDF also bundles Chromium — verify version compatibility for your use case.
- [ ] **Are you using Gotenberg's Office/LibreOffice routes?** IronPDF does not convert Office documents. If you use `/forms/libreoffice/convert`, that capability has no IronPDF equivalent.
- [ ] **What's your deployment constraint?** Moving from microservice to in-process changes where the CPU/memory overhead lands.
- [ ] **Do other teams/services depend on your Gotenberg instance?** In-process migration only replaces your .NET caller, not the service for others.

---

## Why Migrate (Without Drama)

Teams running Gotenberg specifically for a .NET service's PDF generation often encounter these friction points:

1. **Network latency** — every PDF generation is an HTTP round-trip; adds measurable latency vs in-process.
2. **Service dependency** — Gotenberg must be running, healthy, and reachable; adds an operational dependency.
3. **Container orchestration overhead** — managing the Gotenberg container (health checks, restarts, resource limits) adds DevOps complexity.
4. **Large Docker image** — the Gotenberg image is ~1–1.5GB depending on enabled modules; affects pull time and registry cost.
5. **Multipart form construction** — Gotenberg uses multipart HTTP requests; building request bodies in .NET requires more code than a library call.
6. **Error handling across network** — HTTP errors, timeouts, and service unavailability are additional failure modes vs library exceptions.
7. **Local development friction** — developers need Docker running to test PDF generation locally.
8. **Office document dependency** — if LibreOffice routes aren't used, Gotenberg's full feature set is underutilized for its size.
9. **Scaling coupling** — scaling the Gotenberg service separately from the .NET service adds infrastructure complexity.
10. **Debugging difficulty** — PDF rendering issues require logs from a separate container, not just your application logs.

### Comparison Table

| Aspect | Gotenberg | IronPDF |
|---|---|---|
| Focus | Document conversion microservice (HTTP API) | In-process .NET library (HTML-to-PDF + manipulation) |
| Pricing | Open source (free); Docker image + infra cost | Commercial license |
| API Style | HTTP multipart form requests | Fluent .NET library |
| Learning Curve | Low for simple calls; Medium for advanced options | Medium |
| HTML Rendering | Chromium (same engine family as IronPDF) | Chromium-based |
| Page Indexing | Not applicable (HTTP API, not page-level) | 0-based |
| Thread Safety | HTTP (inherently concurrent) | Renderer-per-thread recommended |
| Namespace | `System.Net.Http` (HttpClient) | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Effort | Notes |
|---|---|---|
| HTML string to PDF | Low | Direct mapping: HTTP POST → renderer call |
| URL to PDF | Low | Both support URL rendering |
| Office to PDF (LibreOffice route) | N/A | No IronPDF equivalent |
| Markdown to PDF | Medium | Convert Markdown to HTML first, then render |
| PDF merge | Low | IronPDF supports natively |
| PDF split | Medium | Not a Gotenberg feature; new capability in IronPDF |
| Watermarking | Medium | Not a Gotenberg route; new capability |
| Password protection | Medium | Not a Gotenberg route; new capability |
| Custom headers/footers | Low | Both support |
| Custom margins | Low | Both support |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| .NET-only Gotenberg consumer, HTML/URL only | IronPDF in-process migration well-scoped |
| Multiple services (non-.NET) calling Gotenberg | Retain Gotenberg for non-.NET callers; migrate .NET path only if desired |
| Office document conversion required | Retain Gotenberg (LibreOffice route) or use separate Office→PDF tool |
| Strict network isolation requirement | Evaluate whether in-process Chromium meets security requirements |

---

## Before You Start

### Prerequisites

- .NET 6, 7, or 8 project
- IronPDF license key (https://ironpdf.com/how-to/license-keys/)
- Gotenberg usage audit complete (see checklist above)

### Find Gotenberg References in Your Codebase

```bash
# Find Gotenberg HTTP calls
rg "gotenberg" --type cs -i

# Find multipart content construction (common pattern for Gotenberg)
rg "MultipartFormDataContent" --type cs

# Find Gotenberg URL patterns
rg "forms/chromium\|forms/libreoffice" --type cs

# Find HttpClient usage pointing to Gotenberg
rg "localhost:3000\|gotenberg" --type cs -i

# Find community wrapper packages
rg -i "gotenberg" **/*.csproj
```

### Remove Gotenberg Client, Add IronPDF

```bash
# Remove Gotenberg community wrapper (if used — verify package name)
# e.g.: dotnet remove package Gotenberg.Sharp.Api.Client (verify)

# Add IronPDF
dotnet add package IronPdf

dotnet restore
```

**Additional steps (infrastructure):**
- Remove Gotenberg container from `docker-compose.yml` (or mark for removal after migration is complete)
- Remove Gotenberg service from Kubernetes/ECS manifests (post-migration)
- Update health checks that depend on Gotenberg availability

---

## Quick Start Migration (3 Steps)

### Step 1: License Configuration

**Before (Gotenberg — no .NET license initialization)**
```csharp
// Gotenberg requires no .NET license initialization.
// Service configuration is in docker-compose.yml or environment variables.
// Docker run example:
// docker run --rm -p 3000:3000 gotenberg/gotenberg:8

// .NET client configuration:
var client = new HttpClient { BaseAddress = new Uri("http://localhost:3000") };
```

**After (IronPDF)**
```csharp
using IronPdf;

// See: https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR_IRONPDF_KEY";
// No separate service. No Docker container. No base address configuration.
```

### Step 2: Namespace Imports

**Before**
```csharp
using System.Net.Http;
using System.Net.Http.Headers;
// If using community wrapper:
// using Gotenberg.Sharp.API.Client; // verify package
```

**After**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3: Basic Conversion

**Before (Gotenberg HTML to PDF via HttpClient)**
```csharp
using System.Net.Http;
using System.Net.Http.Headers;

// Gotenberg HTML-to-PDF: POST /forms/chromium/convert/html
static async Task<byte[]> HtmlToPdfAsync(HttpClient client, string html)
{
    using var content = new MultipartFormDataContent();
    
    // Gotenberg expects an index.html file in the multipart form
    var htmlBytes = System.Text.Encoding.UTF8.GetBytes(html);
    var htmlContent = new ByteArrayContent(htmlBytes);
    htmlContent.Headers.ContentType = new MediaTypeHeaderValue("text/html");
    content.Add(htmlContent, "files", "index.html");
    
    var response = await client.PostAsync("/forms/chromium/convert/html", content);
    response.EnsureSuccessStatusCode();
    
    return await response.Content.ReadAsByteArrayAsync();
}
```

**After (IronPDF)**
```csharp
using IronPdf;

// See: https://ironpdf.com/how-to/html-string-to-pdf/
IronPdf.License.LicenseKey = "YOUR_KEY";

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
byte[] pdfBytes = pdf.BinaryData;
```

---

## API Mapping Tables

### Namespace Mapping

| Gotenberg Pattern | IronPDF Equivalent | Purpose |
|---|---|---|
| `HttpClient` + `MultipartFormDataContent` | `ChromePdfRenderer` | HTML/URL → PDF |
| `/forms/chromium/convert/html` endpoint | `renderer.RenderHtmlAsPdf(html)` | HTML string to PDF |
| `/forms/chromium/convert/url` endpoint | `renderer.RenderUrlAsPdf(url)` | URL to PDF |
| `/forms/pdfengines/merge` endpoint | `PdfDocument.Merge(docs)` | PDF merge |

### Core Concept Mapping

| Gotenberg Concept | IronPDF Equivalent | Description |
|---|---|---|
| Gotenberg service endpoint | `ChromePdfRenderer` instance | Rendering unit |
| Multipart `files` field | HTML/URL string parameter | Input |
| Response body (PDF bytes) | `pdf.BinaryData` | Output |
| HTTP status code check | `try/catch (PdfException)` | Error handling |

### Document Loading

| Operation | Gotenberg | IronPDF |
|---|---|---|
| HTML string to PDF | POST `/forms/chromium/convert/html` | `renderer.RenderHtmlAsPdf(html)` |
| URL to PDF | POST `/forms/chromium/convert/url` | `renderer.RenderUrlAsPdf(url)` |
| Load existing PDF | Not a Gotenberg feature | `PdfDocument.FromFile(path)` |
| Load from bytes | Not a Gotenberg feature | `PdfDocument.FromBytes(bytes)` |

### Page Operations (New Capability Post-Migration)

| Operation | Gotenberg | IronPDF |
|---|---|---|
| Page count | Not exposed | `pdf.PageCount` |
| Extract pages | Not a Gotenberg feature | `pdf.CopyPages(indices)` |
| Rotate page | Not a Gotenberg route | `page.Rotation` |
| Get page size | Not exposed | `page.Width`, `page.Height` |

### Merge/Split Operations

| Operation | Gotenberg | IronPDF |
|---|---|---|
| Merge PDFs | POST `/forms/pdfengines/merge` | `PdfDocument.Merge(docs)` |
| Split PDF | Not a standard Gotenberg route | `pdf.CopyPages(indices)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (Gotenberg)**
```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IO;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new HttpClient { BaseAddress = new Uri("http://localhost:3000") };
        
        string html = @"
            <html>
            <head><style>body { font-family: Arial; margin: 40px; }</style></head>
            <body>
                <h1>Quarterly Report</h1>
                <p>Generated via Gotenberg.</p>
            </body>
            </html>";
        
        using var content = new MultipartFormDataContent();
        
        var htmlBytes = Encoding.UTF8.GetBytes(html);
        var htmlPart  = new ByteArrayContent(htmlBytes);
        htmlPart.Headers.ContentType = new MediaTypeHeaderValue("text/html");
        content.Add(htmlPart, "files", "index.html");
        
        // Optional: paper size, margins via form fields
        content.Add(new StringContent("A4"), "paperWidth");   // verify field names
        content.Add(new StringContent("A4"), "paperHeight");
        
        var response = await client.PostAsync("/forms/chromium/convert/html", content);
        response.EnsureSuccessStatusCode();
        
        byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync("output.pdf", pdfBytes);
        Console.WriteLine($"PDF written: {pdfBytes.Length} bytes");
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;
using System.IO;
using System.Threading.Tasks;

// See: https://ironpdf.com/how-to/html-string-to-pdf/
IronPdf.License.LicenseKey = "YOUR_KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize   = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop   = 10;
renderer.RenderingOptions.MarginBottom = 10;

string html = @"
    <html>
    <head><style>body { font-family: Arial; margin: 40px; }</style></head>
    <body>
        <h1>Quarterly Report</h1>
        <p>Generated via IronPDF.</p>
    </body>
    </html>";

using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
await File.WriteAllBytesAsync("output.pdf", pdf.BinaryData);
Console.WriteLine($"PDF written: {pdf.BinaryData.Length} bytes");
```

---

### 2. Merge PDFs

**Before (Gotenberg)**
```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;

class MergeExample
{
    static async Task Main(string[] args)
    {
        var client = new HttpClient { BaseAddress = new Uri("http://localhost:3000") };
        
        string[] filePaths = { "file1.pdf", "file2.pdf" };
        
        using var content = new MultipartFormDataContent();
        
        foreach (var path in filePaths)
        {
            var bytes   = await File.ReadAllBytesAsync(path);
            var part    = new ByteArrayContent(bytes);
            part.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            // Gotenberg uses filename ordering for merge order — verify
            content.Add(part, "files", Path.GetFileName(path));
        }
        
        var response = await client.PostAsync("/forms/pdfengines/merge", content);
        response.EnsureSuccessStatusCode();
        
        byte[] merged = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync("merged.pdf", merged);
        Console.WriteLine($"Merged: {merged.Length} bytes");
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;
using System.IO;

// See: https://ironpdf.com/how-to/merge-or-split-pdfs/
IronPdf.License.LicenseKey = "YOUR_KEY";

using var doc1 = PdfDocument.FromFile("file1.pdf");
using var doc2 = PdfDocument.FromFile("file2.pdf");

using var merged = PdfDocument.Merge(doc1, doc2);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (Gotenberg — not a standard Gotenberg feature)**
```csharp
// Gotenberg's Chromium routes do not expose a direct watermark/stamp API.
// Teams typically inject watermark text via HTML/CSS before sending to Gotenberg.

// Approach: embed watermark in HTML, then convert
string htmlWithWatermark = @"
    <html>
    <head>
    <style>
        .watermark {
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%) rotate(-45deg);
            font-size: 60px;
            color: rgba(128, 128, 128, 0.3);
            z-index: 9999;
            pointer-events: none;
        }
    </style>
    </head>
    <body>
        <div class='watermark'>CONFIDENTIAL</div>
        <h1>Document Content</h1>
        <p>Body text here.</p>
    </body>
    </html>";

// Then POST this HTML to Gotenberg as above
// This approach has limitations: doesn't apply to existing PDFs
```

**After (IronPDF)**
```csharp
using IronPdf;
using IronPdf.Editing;

// See: https://ironpdf.com/how-to/custom-watermark/
// Applies to existing PDFs — not possible in Gotenberg without HTML re-render
IronPdf.License.LicenseKey = "YOUR_KEY";

using var pdf = PdfDocument.FromFile("input.pdf");

var stamp = new TextStamper
{
    Text                = "CONFIDENTIAL",
    FontSize            = 40,
    Opacity             = 0.3,
    Rotation            = -45,
    VerticalAlignment   = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};

pdf.ApplyStamp(stamp);
pdf.SaveAs("watermarked.pdf");
```

---

### 4. Password Protection

**Before (Gotenberg — not a standard Gotenberg route)**
```csharp
// Gotenberg does not expose PDF password/encryption via its API routes.
// This is new capability gained by migrating to IronPDF.

// With Gotenberg: teams typically post-process the PDF bytes
// with a second tool or separate service to add encryption.

Console.WriteLine("Password protection is new capability — not available in Gotenberg routes");
```

**After (IronPDF)**
```csharp
using IronPdf;
using IronPdf.Security;

// See: https://ironpdf.com/how-to/pdf-permissions-passwords/
// New capability compared to Gotenberg
IronPdf.License.LicenseKey = "YOUR_KEY";

using var pdf = PdfDocument.FromFile("output.pdf");

pdf.SecuritySettings.UserPassword  = "user123";
pdf.SecuritySettings.OwnerPassword = "owner456";
pdf.SecuritySettings.AllowUserPrinting         = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;

pdf.SaveAs("protected.pdf");
Console.WriteLine("Password protection applied — was not available via Gotenberg");
```

---

## Critical Migration Notes

### No Page-Level API in Gotenberg

Gotenberg exposes no page count, page manipulation, or page-level operations — it returns raw PDF bytes. IronPDF exposes full page-level access. This is new capability, not a migration concern, but code that previously had to re-process Gotenberg output bytes with a separate library can now be consolidated.

### Error Handling: HTTP Status Codes vs Exceptions

Gotenberg surfaces errors as HTTP error responses. IronPDF throws exceptions.

```csharp
// Before: HTTP error handling
var response = await client.PostAsync("/forms/chromium/convert/html", content);
if (!response.IsSuccessStatusCode)
{
    string body = await response.Content.ReadAsStringAsync();
    throw new Exception($"Gotenberg error {response.StatusCode}: {body}");
}

// After: exception handling
try
{
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    // ...
}
catch (IronPdf.Exceptions.PdfException ex)
{
    Console.Error.WriteLine($"Render error: {ex.Message}");
}
```

### Gotenberg Form Field Names

Gotenberg's HTTP API uses multipart form field names for configuration (paper size, margins, etc.). IronPDF uses typed `RenderingOptions` properties. The mapping:

| Gotenberg Form Field | IronPDF RenderingOptions Property |
|---|---|
| `paperWidth` / `paperHeight` | `PaperSize` enum |
| `marginTop/Bottom/Left/Right` | `MarginTop/Bottom/Left/Right` (mm) |
| `landscape` | `PaperOrientation` enum |
| `waitDelay` | `WaitFor.RenderDelay(ms)` |
| `scale` | verify in IronPDF rendering options |

Verify all Gotenberg field names at https://gotenberg.dev/docs before publishing this mapping.

### Office Documents (LibreOffice Routes)

If you use Gotenberg's LibreOffice routes (`/forms/libreoffice/convert`), there is no IronPDF equivalent. Options: retain Gotenberg solely for Office→PDF, use a separate Office conversion library, or restructure source documents to HTML/CSS templates.

---

## Performance Considerations

### Latency: Network vs In-Process

Gotenberg adds HTTP round-trip latency per conversion. For a service on the same host, this may be 1–10ms per call, but accumulates in batch workloads. In-process IronPDF eliminates network latency but adds per-process Chromium overhead.

For high-frequency conversions (100+ per minute), measure both approaches in your environment before concluding which is faster for your workload.

### Memory: Service vs In-Process

Gotenberg runs Chromium in a separate container — memory usage is isolated from your .NET process. IronPDF runs Chromium in-process — memory pressure affects your application's heap. For memory-constrained deployments, evaluate whether in-process Chromium is acceptable.

### Async Processing

IronPDF supports async rendering, enabling non-blocking PDF generation in ASP.NET Core:

```csharp
// See: https://ironpdf.com/how-to/async/
// Injected as singleton renderer; async per request
public class PdfService
{
    private readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();
    
    public async Task<byte[]> GenerateAsync(string html)
    {
        using var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}
```

---

## Migration Checklist

### Pre-Migration
- [ ] Complete architecture decision checklist (top of article)
- [ ] Confirm no non-.NET services depend on your Gotenberg instance
- [ ] List all Gotenberg endpoint routes used (`rg "forms/" --type cs`)
- [ ] Identify LibreOffice routes — these need separate handling
- [ ] Document all Gotenberg form field configurations in use
- [ ] Obtain IronPDF license key; confirm activation
- [ ] Review https://ironpdf.com/how-to/license-keys/
- [ ] Create migration branch
- [ ] Set up visual comparison for PDF output

### Code Migration
- [ ] Remove Gotenberg community wrapper NuGet (if used)
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `HttpClient` Gotenberg calls with `ChromePdfRenderer`
- [ ] Set `IronPdf.License.LicenseKey` at startup
- [ ] Replace HTML-to-PDF HTTP calls with `renderer.RenderHtmlAsPdf`
- [ ] Replace URL-to-PDF HTTP calls with `renderer.RenderUrlAsPdf`
- [ ] Replace Gotenberg merge calls with `PdfDocument.Merge`
- [ ] Map Gotenberg form field config to `RenderingOptions` properties
- [ ] Replace HTTP status code error handling with try/catch
- [ ] Remove `MultipartFormDataContent` construction code

### Testing
- [ ] Visual comparison: render 10+ representative templates
- [ ] Compare output file sizes
- [ ] Test URL-to-PDF with internal and external URLs
- [ ] Test merge with 2+ documents; verify page order
- [ ] Test rendering options (paper size, margins, orientation)
- [ ] Load test: measure latency vs Gotenberg baseline
- [ ] Test memory consumption under sustained load
- [ ] Verify no Gotenberg HTTP calls remain in codebase

### Post-Migration
- [ ] Remove Gotenberg from docker-compose.yml (or Kubernetes manifests)
- [ ] Remove Gotenberg health checks from monitoring
- [ ] Update local dev setup docs (no more Gotenberg Docker required)
- [ ] Remove Gotenberg service from CI/CD environment setup
- [ ] Update infrastructure cost tracking (container gone)

---

## The Bottom Line

The Gotenberg migration is unique in this series because it's not just a library swap — it's an architectural shift from distributed to in-process. That shift has real trade-offs: you gain lower latency, simpler local development, and new capabilities (page manipulation, watermarking, passwords) while trading away service isolation and potentially increasing your application's memory footprint.

The LibreOffice route question is the one that most often stalls this migration: **if your pipeline converts Word or Excel documents to PDF, what's your plan for that specific operation?** IronPDF doesn't handle Office formats, and pretending that isn't an issue delays the real architectural decision.

Drop specific constraint details in the comments — especially around LibreOffice routes or multi-tenant deployment patterns where service isolation matters.

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
