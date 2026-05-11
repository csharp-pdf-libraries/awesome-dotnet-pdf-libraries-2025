---
title: "PDFmyURL to IronPDF: less config, same output"
published: false
tags: dotnet, csharp, pdf, migration
---

The install story for PDFmyURL-based workflows usually goes the same way: an outbound HTTPS call on every render, an API key threaded through configuration and secrets, and error-handling code that branches on HTTP status. The rendering itself may be fine, but the network round-trip is a recurring tax on every document.

This article walks through replacing that setup with IronPDF — a .NET-native library with no external service dependency. You'll have copy-paste migration code for URL-to-PDF, HTML-to-PDF, merge, watermark, and password protection by the end. Even if you don't adopt IronPDF, the comparison tables and checklist are useful for evaluating any replacement.

---

## Why Migrate (Without Drama)

PDFmyURL works well for straightforward URL-to-PDF use cases in environments where outbound HTTP is unrestricted. Teams commonly start looking at alternatives when one or more of these conditions appear:

1. **Air-gapped or restricted network environments** — every render requires an outbound HTTPS call to pdfmyurl.com.
2. **Latency sensitivity** — round-trip time to an external API adds to every document generation request.
3. **Data privacy requirements** — HTML content containing PII or confidential data leaves the network boundary on every call.
4. **Offline or on-premise deployments** — SaaS dependency conflicts with deployment requirements.
5. **API rate limits** — high-volume scenarios require plan upgrades or request throttling logic.
6. **Subscription cost growth** — published PDFmyURL tiers scale by monthly page volume (Starter, Professional, Advanced) and add up at scale.
7. **Error handling complexity** — HTTP status codes from a remote service require different error-handling patterns than local exceptions.
8. **CI/CD environment variability** — network policy changes or firewall rules can silently break renders in build pipelines.
9. **Dependency on third-party uptime** — any SaaS outage directly impacts your PDF generation.
10. **Local test environments** — mocking an external API for unit tests adds friction.

### Comparison Table

| Aspect | PDFmyURL | IronPDF |
|---|---|---|
| Focus | Remote HTML-to-PDF SaaS API | Local .NET PDF library (HTML render + manipulation) |
| Pricing | Monthly subscription tiers by page volume | Perpetual license available — see [pricing](https://ironpdf.com/licensing/) |
| API Style | REST API over HTTP (`WebClient` / `HttpClient`) | Native C# objects and method calls |
| Learning Curve | Low for basic URL conversion; HTTP handling required | Low for .NET developers; Chromium renderer internals abstracted |
| HTML Rendering | Server-side W3C-compliant engine | Embedded Chromium |
| Page Indexing | N/A — not a page manipulation library | 0-based |
| Thread Safety | Stateless HTTP calls — inherently parallelizable | Renderer reuse supported; safe to share within reasonable concurrency |
| Namespace | `System.Net` / `System.Net.Http` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | PDFmyURL Approach | IronPDF Equivalent | Complexity |
|---|---|---|---|
| URL to PDF | `GET/POST https://pdfmyurl.com/api?url=...` | `ChromePdfRenderer.RenderUrlAsPdf()` | Low |
| HTML string to PDF | `POST html=<html>...` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Low |
| Save to disk | Write response bytes | `pdf.SaveAs()` | Low |
| Save to MemoryStream | Read response stream | `pdf.Stream` | Low |
| Merge PDFs | Not a native feature — requires multiple API calls + local assembly | `PdfDocument.Merge()` | Medium |
| Watermark | Not a native feature | `pdf.ApplyWatermark()` | Medium |
| Password protection | `user_password` / `owner_password` form params | `pdf.SecuritySettings.UserPassword` / `OwnerPassword` | Low |
| Page manipulation | Not a native feature | `pdf.RemovePages()`, `pdf.CopyPages()`, etc. | Medium |
| Custom headers/footers | `header=` / `footer=` form params with `[page]` / `[topage]` tokens | `RenderingOptions.HtmlHeader` / `HtmlFooter` with `{page}` / `{total-pages}` | Medium |
| Async parallel rendering | Manual `Task.WhenAll` over HTTP | `Task.Run` + `Task.WhenAll` | Medium |
| Error handling migration | HTTP status → exception mapping | Exception-based — refactor status-code logic | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Air-gapped or on-premise deployment | Switch — PDFmyURL cannot operate without outbound network |
| Low-volume, internet-connected, URL-only renders | Staying on PDFmyURL is reasonable; evaluate based on latency/privacy needs |
| PII or confidential content in rendered HTML | Switch — data leaves the network boundary on every PDFmyURL call |
| High-volume with complex PDF manipulation needs | Switch — PDFmyURL lacks local merge/watermark/page-edit features |

---

## Before You Start

### Prerequisites

- .NET 6, 7, 8, or 9
- NuGet access or local package source
- IronPDF license key — [get a trial key here](https://ironpdf.com/how-to/license-keys/)
- `grep` or `ripgrep` installed for codebase scanning

### Find All PDFmyURL References

```bash
# Find all PDFmyURL API endpoint usage
rg "pdfmyurl\.com/api" --type cs -n

# Find the optional .NET component (PDFmyURL.NET.dll) usage
rg "PDFmyURLdotNET" --type cs -n

# Find the per-request license token
rg "license=" --type cs --type json --type xml
```

### Remove PDFmyURL References / Install IronPDF

PDFmyURL has no NuGet package — the service is REST-only, with an optional `PDFmyURL.NET.dll` component installed by direct download. Remove any DLL reference from your `.csproj`, then install IronPDF:

```bash
# Install IronPDF
dotnet add package IronPdf

# Restore
dotnet restore
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
// Program.cs or application startup
using IronPdf;

// Set license key before any IronPDF call
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Alternatively, set via environment variable: IRONPDF_LICENSE_KEY
// See: https://ironpdf.com/how-to/license-keys/
```

### Step 2 — Namespace Imports

**Before (PDFmyURL via WebClient/HttpClient):**
```csharp
using System;
using System.Net;                    // WebClient
using System.Net.Http;               // or HttpClient
using System.Threading.Tasks;
using System.IO;
// using PDFmyURLdotNET;             // only if you used the optional PDFmyURL.NET.dll
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;
using System.IO;
```

### Step 3 — Basic URL-to-PDF Conversion

**Before:**
```csharp
using System;
using System.Net;

class Program
{
    static void Main()
    {
        try
        {
            using (var client = new WebClient())
            {
                // License token is sent on every request
                client.QueryString.Add("license", Environment.GetEnvironmentVariable("PDFMYURL_KEY"));
                client.QueryString.Add("url", "https://example.com");
                // URL sent to external servers; PDF binary returned in response body
                client.DownloadFile("https://pdfmyurl.com/api", "output.pdf");
            }
            Console.WriteLine("Saved output.pdf");
        }
        catch (WebException ex)
        {
            // HTTP-level failures: invalid license, rate limit, network error, service unavailable
            Console.WriteLine($"PDF generation failed: {ex.Message}");
        }
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

// License: https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();

// Render URL directly — no network round-trip to an external service
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");

Console.WriteLine("Saved output.pdf");
```

---

## API Mapping Tables

### Namespace Mapping

| PDFmyURL Pattern | IronPDF Equivalent | Notes |
|---|---|---|
| `System.Net.WebClient` / `System.Net.Http.HttpClient` | `IronPdf.ChromePdfRenderer` | Renderer replaces HTTP client |
| `PDFmyURLdotNET.PDFmyURL` (optional .NET component) | `IronPdf.ChromePdfRenderer` | Class from PDFmyURL.NET.dll |
| HTTP response bytes | `IronPdf.PdfDocument` | Returned directly from renderer |
| Manual byte/stream handling | `pdf.SaveAs()` / `pdf.BinaryData` / `pdf.Stream` | Built-in save and access methods |

### Core Class Mapping

| PDFmyURL Concept | IronPDF Class | Description |
|---|---|---|
| API client / `WebClient` / `HttpClient` | `ChromePdfRenderer` | Renders HTML/URL to PDF |
| Response bytes | `PdfDocument` | Full PDF object with manipulation methods |
| Form / query parameters | `ChromePdfRenderOptions` | Rendering config: page size, margins, headers |
| N/A (no local manipulation) | `PdfDocument.Merge()` | Static merge method |

### Document Loading Methods

| PDFmyURL | IronPDF | Notes |
|---|---|---|
| `GET/POST url=https://...` | `renderer.RenderUrlAsPdf(url)` | Renders live URL |
| `POST html=<html>` | `renderer.RenderHtmlAsPdf(html)` | Renders HTML string |
| Read file then `POST html=` | `renderer.RenderHtmlFileAsPdf(path)` | Renders local HTML file |
| N/A | `PdfDocument.FromFile(path)` | Loads existing PDF |

### Page Operations

| Operation | PDFmyURL | IronPDF |
|---|---|---|
| Page count | Not available locally | `pdf.PageCount` |
| Remove page | Not available | `pdf.RemovePages(index)` |
| Copy pages | Not available | `pdf.CopyPages(startIndex, endIndex)` |
| Rotate pages | Not available | `pdf.RotateAllPages(PdfPageRotation.Rotate90)` |

### Merge / Split Operations

| Operation | PDFmyURL | IronPDF |
|---|---|---|
| Merge multiple PDFs | Multiple API calls + manual byte assembly | `PdfDocument.Merge(doc1, doc2)` |
| Split PDF | Not a native feature | `pdf.CopyPages(...)` — [merge/split guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before (PDFmyURL via WebClient):**
```csharp
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

class HtmlToPdfBefore
{
    static void Main()
    {
        var license = Environment.GetEnvironmentVariable("PDFMYURL_KEY");
        var html = "<html><body><h1>Invoice #1042</h1><p>Amount due: $1,200</p></body></html>";

        try
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    { "license",   license },
                    { "html",      html },
                    { "page_size", "A4" }
                };

                // POST against the single PDFmyURL endpoint
                byte[] pdfBytes = client.UploadValues("https://pdfmyurl.com/api", "POST", values);
                File.WriteAllBytes("invoice.pdf", pdfBytes);

                Console.WriteLine($"Saved {pdfBytes.Length} bytes to invoice.pdf");
            }
        }
        catch (WebException ex)
        {
            Console.WriteLine($"API error: {ex.Message}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;
using System;

// License: https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var html = "<html><body><h1>Invoice #1042</h1><p>Amount due: $1,200</p></body></html>";

var renderer = new ChromePdfRenderer();
// Rendering options: https://ironpdf.com/how-to/rendering-options/
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");

Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// See: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge Multiple PDFs

**Before (PDFmyURL — manual assembly):**
```csharp
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;

class MergeBefore
{
    // PDFmyURL doesn't support merge natively.
    // Each section is rendered separately, then a local library combines the byte arrays.
    static void Main()
    {
        var license = Environment.GetEnvironmentVariable("PDFMYURL_KEY");

        var htmlPages = new[]
        {
            "<html><body><h1>Section 1</h1></body></html>",
            "<html><body><h1>Section 2</h1></body></html>",
        };

        var pdfBytesList = new List<byte[]>();

        foreach (var html in htmlPages)
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    { "license", license },
                    { "html",    html }
                };

                byte[] bytes = client.UploadValues("https://pdfmyurl.com/api", "POST", values);
                pdfBytesList.Add(bytes);
            }
        }

        // A separate local library is required to merge — shown as pseudo-code
        // since the merge step depends on the chosen library:
        // var merged = SomePdfLibrary.Merge(pdfBytesList);
        // File.WriteAllBytes("merged.pdf", merged);

        Console.WriteLine("Merge step requires an additional local PDF library");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();

var pdf1 = renderer.RenderHtmlAsPdf("<html><body><h1>Section 1</h1></body></html>");
var pdf2 = renderer.RenderHtmlAsPdf("<html><body><h1>Section 2</h1></body></html>");

// Merge: https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");

Console.WriteLine($"Merged PDF: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (PDFmyURL — not natively supported):**
```csharp
using System;

class WatermarkBefore
{
    static void Main()
    {
        // PDFmyURL does not provide a watermark API.
        // Two common workarounds:
        //   1. Embed watermark text/image in the source HTML before sending
        //   2. Apply watermark post-render using a separate local library

        // Option 1 — CSS-based watermark in HTML:
        var html = @"
            <html>
            <head>
            <style>
                body::after {
                    content: 'DRAFT';
                    position: fixed;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%) rotate(-45deg);
                    font-size: 100px;
                    opacity: 0.15;
                    color: gray;
                    z-index: 9999;
                }
            </style>
            </head>
            <body><h1>Document</h1></body>
            </html>";

        // Then POST html to PDFmyURL API...
        Console.WriteLine("CSS-injection watermark only — placement and opacity controlled in HTML");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Document</h1></body></html>");

// Watermark: https://ironpdf.com/how-to/custom-watermark/
pdf.ApplyWatermark(
    "<h2 style='color:gray;'>DRAFT</h2>",
    rotation: 30,
    verticalAlignment: VerticalAlignment.Middle,
    horizontalAlignment: HorizontalAlignment.Center);

pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — see: https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (PDFmyURL — via form parameters):**
```csharp
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

class PasswordBefore
{
    static void Main()
    {
        var license = Environment.GetEnvironmentVariable("PDFMYURL_KEY");

        try
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    { "license",          license },
                    { "html",             "<h1>Confidential</h1>" },
                    { "encryption_level", "128aes" },
                    { "user_password",    "open123" },
                    { "owner_password",   "admin456" }
                };

                byte[] pdfBytes = client.UploadValues("https://pdfmyurl.com/api", "POST", values);
                File.WriteAllBytes("protected.pdf", pdfBytes);
            }
        }
        catch (WebException ex)
        {
            Console.WriteLine($"API error: {ex.Message}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential Report</h1>");

// Security: https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";
pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;

pdf.SaveAs("protected.pdf");
Console.WriteLine("Saved protected.pdf — see: https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### HTTP Status Codes → Exceptions

PDFmyURL integration typically involves `WebException` handling or checking `response.IsSuccessStatusCode` and branching on HTTP error codes. IronPDF throws typed .NET exceptions instead. Audit all error-handling code that inspects status codes and convert to try/catch blocks.

```csharp
// Pattern to refactor:
// catch (WebException ex) { ... }
// or
// if (!response.IsSuccessStatusCode) { /* handle */ }

// Replace with:
try
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
catch (IronPdf.Exceptions.IronPdfLicenseException ex)
{
    // Invalid or expired license key
    Console.Error.WriteLine($"License error: {ex.Message}");
}
catch (IronPdf.Exceptions.IronPdfRenderingException ex)
{
    // URL unreachable, invalid HTML, JavaScript error
    Console.Error.WriteLine($"Render failed: {ex.Message}");
}
```

### Header / Footer Placeholders

PDFmyURL uses square-bracket placeholders inside `header` / `footer` form parameters (e.g., `[page]`, `[topage]`). IronPDF uses curly-brace placeholders inside `HtmlHeaderFooter.HtmlFragment`:

```csharp
// Before (PDFmyURL form parameter):
//   footer=<div>Page [page] of [topage]</div>

// After (IronPDF):
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "Page {page} of {total-pages}"
};
```

Available IronPDF tokens include `{page}`, `{total-pages}`, `{date}`, `{time}`, `{html-title}`, and `{url}`.

### Page Indexing

IronPDF uses 0-based page indexing.

### No Network Dependency

Tests that mocked or stubbed the PDFmyURL HTTP endpoint can be simplified — IronPDF renders locally so no network mock is needed. Remove `HttpMessageHandler` mocks and test with actual rendered output.

### Async Patterns

PDFmyURL integration uses `await` for HTTP calls. IronPDF's primary rendering methods are synchronous; wrap them in `Task.Run` to preserve async call sites:

```csharp
var pdf = await Task.Run(() => renderer.RenderUrlAsPdf(url));
```

See the [async guide](https://ironpdf.com/how-to/async/) for details.

---

## Performance Considerations

### Renderer Reuse

Instantiating `ChromePdfRenderer` per request is functional but suboptimal for high-volume scenarios. A common pattern is to reuse a single renderer instance:

```csharp
var renderer = new ChromePdfRenderer();

// Render multiple documents
foreach (var job in renderQueue)
{
    var pdf = renderer.RenderHtmlAsPdf(job.Html);
    pdf.SaveAs(job.OutputPath);
}
```

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

// Parallel rendering: https://ironpdf.com/examples/parallel/
var htmlJobs = new[] { "<h1>Doc 1</h1>", "<h1>Doc 2</h1>", "<h1>Doc 3</h1>" };

var tasks = htmlJobs.Select(html =>
    Task.Run(() =>
    {
        var r = new ChromePdfRenderer();
        return r.RenderHtmlAsPdf(html);
    }));

var results = await Task.WhenAll(tasks);
Console.WriteLine($"Rendered {results.Length} PDFs in parallel");
```

### Disposal

`PdfDocument` implements `IDisposable`. Use `using` blocks for proper cleanup:

```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// Disposed at end of block
```

---

## Migration Checklist

### Pre-Migration
- [ ] Identify all usages of the PDFmyURL endpoint (`rg "pdfmyurl\.com/api" --type cs`)
- [ ] Identify any usage of the optional `PDFmyURL.NET.dll` component (`PDFmyURLdotNET` namespace)
- [ ] Identify all HTTP error-handling code paths that check status codes or catch `WebException`
- [ ] List all PDFmyURL form parameters in use (page_size, orientation, margins, header/footer, encryption, etc.)
- [ ] Document any header/footer placeholder tokens (`[page]`, `[topage]`, etc.) that need migrating
- [ ] Document current average render latency (baseline for comparison)
- [ ] Review data privacy requirements — confirm no constraint on local rendering
- [ ] Obtain IronPDF license key and add to secrets manager
- [ ] Verify IronPDF .NET version compatibility with your target framework

### Code Migration
- [ ] Install IronPDF via NuGet (`dotnet add package IronPdf`)
- [ ] Remove any reference to `PDFmyURL.NET.dll` from `.csproj` (there is no NuGet package to uninstall)
- [ ] Add license key configuration at application startup (`IronPdf.License.LicenseKey`)
- [ ] Replace all `WebClient` / `HttpClient` PDFmyURL calls with `ChromePdfRenderer` calls
- [ ] Replace `POST html=<html>` pattern with `RenderHtmlAsPdf()`
- [ ] Replace `GET/POST url=https://` pattern with `RenderUrlAsPdf()`
- [ ] Convert form/query parameters to `RenderingOptions` properties
- [ ] Update header/footer placeholder syntax from `[page]` / `[topage]` to `{page}` / `{total-pages}`
- [ ] Convert HTTP status-code error handling and `WebException` to typed IronPDF exception catches
- [ ] Migrate any CSS-embedded watermarks to `pdf.ApplyWatermark()`
- [ ] Migrate `user_password` / `owner_password` form params to `pdf.SecuritySettings`
- [ ] Remove network mocks from unit tests

### Testing
- [ ] Run existing HTML-to-PDF tests and compare visual output
- [ ] Test URL rendering for pages with dynamic content or auth-gated content
- [ ] Test error paths — invalid HTML, missing URLs, over-large documents
- [ ] Verify output PDF opens correctly in target viewers (Adobe, browser, etc.)
- [ ] Check PDF metadata (author, title, creation date) if used downstream
- [ ] Benchmark render time vs. PDFmyURL baseline (account for network latency difference)
- [ ] Test in CI environment — confirm no outbound network calls required
- [ ] On Linux, confirm Chromium dependencies are installed (`libnss3`, `libatk1.0-0`, etc.)

### Post-Migration
- [ ] Remove `PDFMYURL_KEY` (and per-request `license=` references) from all environments and secrets stores
- [ ] Update documentation / runbooks to remove PDFmyURL references
- [ ] Archive or remove any separate merge/watermark/security library that was only there to supplement PDFmyURL
- [ ] Monitor memory usage — local rendering has a different resource profile than HTTP calls

---

## Final Thoughts

The main structural change in this migration is the shift from HTTP response handling to exception-based error handling — that's where most of the code rewrite lives. The render call itself is simpler, and form parameters become typed `RenderingOptions` properties.

One area worth benchmarking carefully: if your PDFmyURL integration was handling very large pages or complex CSS, measure local render times against your previous API response times. In network-constrained environments PDFmyURL calls may have been the bottleneck; in others, a cold Chromium startup may be. The tradeoff is different per workload.

**Discussion question for the comments:** After migrating, what was your before/after bundle size or average render latency? Particularly interested in cases where the external API was faster or slower than expected compared to local rendering.
