---
title: "Migrating from Api2pdf to IronPDF: less setup, same output"
published: false
description: "A checklist-heavy migration guide for teams moving from the Api2pdf REST API to IronPDF's in-process .NET library. Covers the SaaS-to-library paradigm shift with printable checklists throughout."
tags: dotnet, csharp, pdf, migration
---

You refreshed the dashboard and the number was right there: 214,000 API calls this month. Your application generates a PDF for every order confirmation, every shipping label, and every monthly statement. Each one is an HTTP POST to `v2.api2pdf.com`, a wait for the response, a download of the generated file from a temporary URL, and a prayer that the 24-hour auto-delete does not fire before your async job picks it up.

Api2pdf solves a real problem: it is genuinely hard to run wkhtmltopdf or headless Chrome on a managed hosting platform. But at 214,000 calls per month, you are paying for network latency, external service availability, and temporary file management that you would not need if the PDF engine ran inside your own process.

This guide covers the migration from Api2pdf's REST API to [IronPDF](https://ironpdf.com)'s in-process .NET library. This is not an API-to-API migration—it is a paradigm shift from SaaS to library. Eighty percent of this content helps you evaluate any in-process alternative. The remaining twenty percent is IronPDF-specific.

## Why Migrate (Without Drama)

Api2pdf removes the pain of hosting Chrome or wkhtmltopdf yourself. Here is why teams outgrow it:

1. **Latency per render.** Every PDF is an HTTP round-trip: POST HTML → wait for render → download file URL. For high-volume applications, this adds 1–5 seconds per PDF that an in-process library eliminates.
2. **Temporary file URLs.** Api2pdf returns a URL to the generated file, not the file itself. Your application must download the PDF before the 24-hour expiry. This introduces a race condition and a failure mode that does not exist with in-process generation.
3. **External dependency for a core function.** If Api2pdf's service goes down, your PDF generation stops. If your internet connection degrades, your PDF generation slows. For applications where PDFs are on the critical path (invoices, contracts, compliance docs), this is a risk.
4. **Data leaves your network.** Your HTML—which may contain customer names, addresses, financial data—is sent to Api2pdf's servers for rendering. For regulated industries (healthcare, finance, legal), this data transit may require additional compliance review.
5. **Per-call cost at scale.** Api2pdf's usage-based pricing is economical for low volume. At hundreds of thousands of calls per month, a one-time library license may be cheaper.
6. **No offline capability.** If your application needs to generate PDFs in disconnected environments (on-premise deployments, air-gapped networks), a REST API is not an option.
7. **Limited post-processing.** Api2pdf handles generation and merge well, but more advanced manipulation (watermarking, page-level security settings, form filling) requires additional API calls or is not supported.
8. **wkhtmltopdf deprecation risk.** Api2pdf offers wkhtmltopdf as a rendering option. wkhtmltopdf is based on a deprecated version of WebKit and has not received updates. Chrome is the better option, but not all Api2pdf integrations use it.
9. **File size and timeout limits.** Cloud APIs have practical limits on request payload size and processing time. Very large or complex HTML documents may timeout.
10. **Debugging opacity.** When a render fails or produces unexpected output, you cannot inspect the rendering engine's state. With an in-process library, you can step through with a debugger.

### Comparison Table

| Aspect | Api2pdf (REST API) | IronPDF (.NET library) |
|---|---|---|
| **Focus** | Cloud PDF generation via REST | In-process PDF generation + manipulation |
| **Pricing** | Usage-based per API call | Per-developer, perpetual license |
| **API Style** | HTTP REST + client SDK | Native .NET API, no network calls |
| **Learning Curve** | Low—simple HTTP calls | Low—simple .NET API calls |
| **HTML Rendering** | Headless Chrome or wkhtmltopdf (remote) | Chrome engine (in-process) |
| **Page Indexing** | N/A (service handles internally) | 0-based |
| **Thread Safety** | N/A (stateless API) | `ChromePdfRenderer` stateless, safe to share |
| **Namespace** | `Api2Pdf` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Complexity | Notes |
|---|---|---|
| HTML → PDF (Chrome) | Low | Direct replacement; IronPDF runs Chrome in-process |
| URL → PDF | Low | `RenderUrlAsPdf` replaces the Chrome URL endpoint |
| HTML → PDF (wkhtmltopdf) | Low | Replace with IronPDF Chrome renderer—better output quality |
| Merge | Low | `PdfDocument.Merge()` replaces the PdfSharp merge endpoint |
| Office → PDF (LibreOffice) | High / N/A | No direct IronPDF equivalent. Use LibreOffice headless locally or a separate service |
| Password protection | Low | IronPDF `SecuritySettings` replaces the password endpoint |
| Bookmarks | Medium | IronPDF exposes bookmark APIs on `PdfDocument` |
| Barcode generation | N/A | Not a PDF feature—use a dedicated barcode library (e.g., IronBarcode, ZXing.NET) |
| File deletion (security) | N/A | Not needed—files never leave your server |
| Thumbnail generation | Medium | Use `pdf.RasterizeToImageFiles(...)` or `pdf.ToBitmap()` |
| HTML → DOCX/XLSX | N/A | Not an IronPDF feature—use a dedicated library |

### Decision Matrix

| Scenario | Recommendation |
|---|---|
| High-volume HTML-to-PDF with latency concerns | Migrate—in-process rendering eliminates network overhead |
| Regulated industry with data residency requirements | Migrate—HTML never leaves your infrastructure |
| Low-volume, diverse format conversion (Office, email, images) | Stay or partial migrate—keep Api2pdf for Office conversion, use IronPDF for HTML-to-PDF |
| Air-gapped or on-premise deployment | Migrate—Api2pdf requires internet |

---

## Before You Start

> **Checklist: Pre-Migration Readiness**
>
> - [ ] Count your monthly Api2pdf API calls (check your dashboard)
> - [ ] Categorize calls by endpoint: Chrome HTML, Chrome URL, wkhtmltopdf, LibreOffice, merge, password
> - [ ] Identify any LibreOffice (Office → PDF) calls—these need a separate solution
> - [ ] Check if your HTML contains sensitive data sent to Api2pdf's servers
> - [ ] Verify your deployment supports running IronPDF (needs .NET 6+ or Framework 4.6.2+)
> - [ ] Obtain an IronPDF trial key from [ironpdf.com/get-started/license-keys/](https://ironpdf.com/get-started/license-keys/)

### Find Api2pdf References

```bash
# Find all Api2pdf client usage
rg -l "Api2Pdf|api2pdf|ChromeHtmlToPdfRequest|WkhtmlHtmlToPdfRequest" --glob "*.cs"

# Count API call sites
rg -c "\.Chrome\.|\.Wkhtml\.|\.PdfSharp\.|\.LibreOffice\." --glob "*.cs"

# Find configuration (API keys)
rg "Api2Pdf|api2pdf" --glob "*.json" --glob "*.config" --glob "*.cs"
```

In PowerShell: `Get-ChildItem -Recurse -Filter *.cs | Select-String "Api2Pdf|api2pdf"`.

### Swap Dependencies

```bash
# Remove Api2pdf
dotnet remove package Api2Pdf

# Install IronPDF
dotnet add package IronPdf
```

---

## Quick Start Migration (3 Steps)

### Step 1: License Configuration

**Before (Api2pdf):**

```csharp
// Api2pdf — API key passed to constructor
using Api2Pdf;
using System;

class Program
{
    static void Main()
    {
        // API key from portal.api2pdf.com
        var a2pClient = new Api2Pdf("your-api2pdf-api-key");

        // Every call after this hits the internet
        Console.WriteLine("Api2pdf client ready.");
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
        // License key — no internet required for rendering
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        Console.WriteLine($"IronPDF licensed: {License.IsLicensed}");
        // Everything runs locally from here
    }
}
```

### Step 2: Namespace Imports

**Before:**

```csharp
using Api2Pdf;
```

**After:**

```csharp
using IronPdf;
using IronPdf.Editing;    // watermarks, headers, footers
using IronPdf.Rendering;  // render options
```

### Step 3: Basic HTML-to-PDF

**Before (Api2pdf — ~15 lines):**

```csharp
using Api2Pdf;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var a2pClient = new Api2Pdf("your-api-key");

        var request = new ChromeHtmlToPdfRequest
        {
            Html = "<h1>Invoice #1042</h1><p>Amount: $5,200.00</p>"
        };

        var result = await a2pClient.Chrome.HtmlToPdfAsync(request);

        if (result.Success)
        {
            // Result is a URL — you must download the file
            Console.WriteLine("PDF available at: " + result.FileUrl);

            // Download before the 24-hour expiry
            using var http = new HttpClient();
            var pdfBytes = await http.GetByteArrayAsync(result.FileUrl);
            File.WriteAllBytes("invoice.pdf", pdfBytes);
            Console.WriteLine("Downloaded and saved.");
        }
        else
        {
            Console.WriteLine("Error: " + result.Error);
        }
    }
}
```

**After (IronPDF — 8 lines):**

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        var pdf = renderer.RenderHtmlAsPdf(
            "<h1>Invoice #1042</h1><p>Amount: $5,200.00</p>");

        pdf.SaveAs("invoice.pdf");
        Console.WriteLine("Saved locally. No download needed.");
    }
}
```

> **Checklist: What You Just Eliminated**
>
> - [x] HTTP round-trip to external API
> - [x] Temporary file URL with 24-hour expiry
> - [x] `HttpClient.GetByteArrayAsync` download step
> - [x] Internet connectivity requirement for rendering
> - [x] API key management and rotation
> - [x] External service availability dependency

---

## API Mapping Tables

### Namespace Mapping

| Api2pdf Namespace | IronPDF Namespace | Purpose |
|---|---|---|
| `Api2Pdf` | `IronPdf` | Core client/renderer |
| `Api2Pdf.Models` | `IronPdf.Rendering` | Request options / render settings |
| N/A | `IronPdf.Editing` | Watermarks, headers, footers |

### Core Class Mapping

| Api2pdf Class | IronPDF Class | Description |
|---|---|---|
| `Api2Pdf` (client) | `ChromePdfRenderer` | The entry point for PDF generation |
| `ChromeHtmlToPdfRequest` | `ChromePdfRenderOptions` | Render configuration |
| `Api2PdfResult` | `PdfDocument` | The generated PDF (URL vs. in-memory object) |
| `a2pClient.PdfSharp` | `PdfDocument` static methods | Merge and manipulation |

### Document Loading Methods

| Operation | Api2pdf | IronPDF |
|---|---|---|
| HTML → PDF | `a2pClient.Chrome.HtmlToPdf(request)` → URL | `renderer.RenderHtmlAsPdf(html)` → `PdfDocument` |
| URL → PDF | `a2pClient.Chrome.UrlToPdf(request)` → URL | `renderer.RenderUrlAsPdf(url)` → `PdfDocument` |
| File → download | `result.SaveFile("path")` | `pdf.SaveAs("path")` |
| Merge | `a2pClient.PdfSharp.MergePdfs(...)` → URL | `PdfDocument.Merge(pdf1, pdf2)` → `PdfDocument` |

### Page Operations

| Operation | Api2pdf | IronPDF |
|---|---|---|
| Get page count | N/A (not exposed via API) | `pdf.PageCount` |
| Access specific page | N/A | `pdf.Pages[index]` (0-based) |
| Add watermark | Not directly supported | `pdf.ApplyWatermark(html)` |
| Set password | `a2pClient.PdfSharp.SetPassword(new PdfPasswordRequest { ... })` | `pdf.SecuritySettings.UserPassword = "..."` |

### Merge / Split Operations

| Operation | Api2pdf | IronPDF |
|---|---|---|
| Merge PDFs | `a2pClient.PdfSharp.MergePdfs(new PdfMergeRequest { Urls = ... })` → URL | `PdfDocument.Merge(pdf1, pdf2)` |
| Split / extract | Not directly supported | `pdf.CopyPages(start, end)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (Api2pdf):**

```csharp
using Api2Pdf;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class HtmlToPdfApi2pdf
{
    static async Task Main()
    {
        var a2p = new Api2Pdf("your-api-key");

        var request = new ChromeHtmlToPdfRequest
        {
            Html = @"<html><head><style>
                body { font-family: Arial; }
                .header { background: #0d47a1; color: white; padding: 20px; }
                table { width: 100%; border-collapse: collapse; margin: 20px 0; }
                td, th { padding: 8px; border: 1px solid #ddd; }
            </style></head><body>
                <div class='header'><h1>Statement</h1></div>
                <table>
                    <tr><th>Date</th><th>Description</th><th>Amount</th></tr>
                    <tr><td>2025-01-15</td><td>Service fee</td><td>$250.00</td></tr>
                    <tr><td>2025-01-20</td><td>Subscription</td><td>$99.00</td></tr>
                </table>
            </body></html>"
        };

        var result = await a2p.Chrome.HtmlToPdfAsync(request);

        if (!result.Success)
        {
            Console.WriteLine("API error: " + result.Error);
            return;
        }

        // Must download from temporary URL
        using var http = new HttpClient();
        var bytes = await http.GetByteArrayAsync(result.FileUrl);
        File.WriteAllBytes("statement.pdf", bytes);
        Console.WriteLine("Downloaded statement.pdf");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class HtmlToPdfIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PrintHtmlBackgrounds = true;

        var pdf = renderer.RenderHtmlAsPdf(@"<html><head><style>
                body { font-family: Arial; }
                .header { background: #0d47a1; color: white; padding: 20px; }
                table { width: 100%; border-collapse: collapse; margin: 20px 0; }
                td, th { padding: 8px; border: 1px solid #ddd; }
            </style></head><body>
                <div class='header'><h1>Statement</h1></div>
                <table>
                    <tr><th>Date</th><th>Description</th><th>Amount</th></tr>
                    <tr><td>2025-01-15</td><td>Service fee</td><td>$250.00</td></tr>
                    <tr><td>2025-01-20</td><td>Subscription</td><td>$99.00</td></tr>
                </table>
            </body></html>");

        pdf.SaveAs("statement.pdf");
        Console.WriteLine("Saved locally — no download step.");
    }
}
```

No HTTP client. No temporary URL. No download race condition. See [IronPDF HTML-to-PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/).

### 2. Merge PDFs

**Before (Api2pdf):**

```csharp
using Api2Pdf;
using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

class MergeApi2pdf
{
    static async Task Main()
    {
        var a2p = new Api2Pdf("your-api-key");

        // Api2pdf merge requires URLs to existing PDFs
        var pdfUrls = new List<string>
        {
            "https://your-server.com/files/part1.pdf",
            "https://your-server.com/files/part2.pdf"
        };

        var result = await a2p.PdfSharp.MergePdfsAsync(new PdfMergeRequest { Urls = pdfUrls });

        if (!result.Success)
        {
            Console.WriteLine("Merge error: " + result.Error);
            return;
        }

        // Download the merged result
        using var http = new HttpClient();
        var bytes = await http.GetByteArrayAsync(result.FileUrl);
        File.WriteAllBytes("merged.pdf", bytes);
        Console.WriteLine("Merged and downloaded.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class MergeIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf1 = PdfDocument.FromFile("part1.pdf");
        var pdf2 = PdfDocument.FromFile("part2.pdf");

        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
        Console.WriteLine("Merged locally.");
    }
}
```

> **Checklist: Merge Migration**
>
> - [x] No need to host PDFs at public URLs for merge input
> - [x] No download step after merge
> - [x] Merge works with local files, byte arrays, or in-memory PdfDocuments
> - [x] No 24-hour expiry on the merged result

See [IronPDF merge documentation](https://ironpdf.com/examples/merge-pdfs/).

### 3. Watermark

**Before (Api2pdf — not directly supported):**

```csharp
// Api2pdf does not have a native watermark endpoint.
// Common workaround: generate the watermark as HTML overlay,
// or use a separate library after downloading the PDF.
// This is a gap in the Api2pdf feature set.

// You would need to:
// 1. Generate PDF via Api2pdf
// 2. Download the PDF
// 3. Use a local library (PdfSharp, iText, etc.) to add watermark
// 4. Save the result

Console.WriteLine("Watermarking requires a second tool with Api2pdf.");
```

**After (IronPDF):**

```csharp
using IronPdf;

class WatermarkIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        pdf.ApplyWatermark(
            "<h1 style='color:rgba(0,0,0,0.15);font-size:60px;'>DRAFT</h1>",
            rotation: 45
        );

        pdf.SaveAs("watermarked.pdf");
        Console.WriteLine("Watermarked locally.");
    }
}
```

IronPDF handles watermarking natively—no second library needed.

### 4. Password Protection

**Before (Api2pdf):**

```csharp
using Api2Pdf;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class PasswordApi2pdf
{
    static async Task Main()
    {
        var a2p = new Api2Pdf("your-api-key");

        // Api2pdf password endpoint requires a URL to an existing PDF
        var result = await a2p.PdfSharp.SetPasswordAsync(new PdfPasswordRequest
        {
            Url = "https://your-server.com/files/input.pdf",
            UserPassword = "user456",
            OwnerPassword = "owner123"
        });

        if (!result.Success)
        {
            Console.WriteLine("Password error: " + result.Error);
            return;
        }

        using var http = new HttpClient();
        var bytes = await http.GetByteArrayAsync(result.FileUrl);
        File.WriteAllBytes("protected.pdf", bytes);
        Console.WriteLine("Password-protected and downloaded.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class PasswordIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        pdf.SecuritySettings.OwnerPassword = "owner123";
        pdf.SecuritySettings.UserPassword = "user456";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.NoPrint;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;

        pdf.SaveAs("protected.pdf");
        Console.WriteLine("Protected locally.");
    }
}
```

IronPDF gives you granular permission control—not just a single password. Your PDF file never leaves your server. See [IronPDF security documentation](https://ironpdf.com/how-to/pdf-permissions-passwords/).

---

## Critical Migration Notes

### The Paradigm Shift: SaaS → Library

This is not a typical API-to-API migration. You are changing the execution model:

| Concern | Api2pdf (SaaS) | IronPDF (Library) |
|---|---|---|
| Where rendering happens | Api2pdf's AWS/GCP servers | Your server / container |
| Data transit | HTML sent over internet | HTML stays in-process |
| Availability | Depends on Api2pdf uptime | Depends on your uptime |
| Latency | Network round-trip + render time | Render time only |
| File lifecycle | 24-hour temp URL | Your filesystem / memory |

### Async Patterns

Api2pdf calls are inherently async (HTTP). IronPDF rendering is synchronous by default but offers async variants:

```csharp
// IronPDF async rendering
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### Error Handling Shift

Api2pdf returns `result.Success` / `result.Error` on every call. IronPDF throws exceptions. Update your error handling:

```csharp
// Api2pdf pattern
if (!result.Success) { log(result.Error); return; }

// IronPDF pattern
try { var pdf = renderer.RenderHtmlAsPdf(html); }
catch (Exception ex) { log(ex.Message); return; }
```

### File Management

With Api2pdf, you downloaded PDFs from temporary URLs. With IronPDF, you have `PdfDocument` objects in memory. You choose when and where to persist them. Remove any download-and-cleanup logic from your codebase.

---

## Performance Considerations

### Latency Reduction

The most significant performance gain is eliminating the network round-trip. A rough benchmark:

- Api2pdf: ~1,500–4,000ms per render (network + remote render + download)
- IronPDF in-process: ~200–1,000ms per render (render only, depends on HTML complexity)

These are illustrative ranges, not benchmarks—your actual numbers depend on network conditions, HTML complexity, and server resources.

### Renderer Reuse

```csharp
private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();
```

### Disposal

```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Edge Cases Worth Flagging

- **LibreOffice replacement:** If you use Api2pdf's LibreOffice endpoint for Office-to-PDF conversion, IronPDF does not replace this. Run LibreOffice headless locally (`libreoffice --headless --convert-to pdf input.docx`) or use a dedicated service. Alternatively, the [IronWord library from Iron Software](https://ironsoftware.com) handles DOCX-to-PDF within .NET.
- **wkhtmltopdf rendering differences:** If your existing Api2pdf integration uses wkhtmltopdf (not Chrome), switching to IronPDF's Chrome engine may produce different output for the same HTML. Test your templates and adjust CSS if needed—Chrome's rendering is more modern and accurate.
- **Barcode generation:** Api2pdf offers barcode/QR code generation. IronPDF does not. Use a dedicated library like ZXing.NET or IronBarcode, and embed the generated barcode image in your HTML before rendering.

---

## Migration Checklist

### Pre-Migration (8 items)

- [ ] Export your Api2pdf usage data: call count, endpoints used, average render time
- [ ] Categorize calls: Chrome HTML, Chrome URL, wkhtmltopdf, LibreOffice, merge, password
- [ ] Identify LibreOffice calls that need a separate replacement
- [ ] Check for wkhtmltopdf-specific HTML that may render differently in Chrome
- [ ] Verify your deployment environment supports IronPDF (.NET 6+ or Framework 4.6.2+)
- [ ] Obtain an IronPDF trial key from [ironpdf.com/get-started/license-keys/](https://ironpdf.com/get-started/license-keys/)
- [ ] Create migration branch
- [ ] Audit your HTML templates for sensitive data currently sent to Api2pdf servers

### Code Migration (10 items)

- [ ] Remove `Api2Pdf` NuGet package
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `new Api2Pdf("key")` with `IronPdf.License.LicenseKey = "..."`
- [ ] Replace `a2pClient.Chrome.HtmlToPdf()` with `renderer.RenderHtmlAsPdf()`
- [ ] Replace `a2pClient.Chrome.UrlToPdf()` with `renderer.RenderUrlAsPdf()`
- [ ] Replace `a2pClient.PdfSharp.MergePdfs()` with `PdfDocument.Merge()`
- [ ] Replace `a2pClient.PdfSharp.SetPassword()` with `SecuritySettings` properties
- [ ] Remove all `HttpClient.GetByteArrayAsync(result.FileUrl)` download code
- [ ] Remove any temporary-file cleanup logic tied to the 24-hour URL lifecycle
- [ ] Replace `result.Success` checks with try/catch exception handling

### Testing (7 items)

- [ ] Compare PDF output visually for your top 10 templates (Chrome engine may differ slightly from Api2pdf's Chrome version)
- [ ] Test any wkhtmltopdf-specific HTML in IronPDF's Chrome engine—adjust CSS as needed
- [ ] Verify password-protected PDFs open correctly in Adobe Reader
- [ ] Test merge with 3+ documents
- [ ] Benchmark render time: in-process vs. Api2pdf round-trip
- [ ] Test in your production deployment environment (Docker, Azure, etc.)
- [ ] Verify no sensitive data is being sent externally after migration

### Post-Migration (4 items)

- [ ] Cancel or downgrade your Api2pdf subscription
- [ ] Remove Api2pdf API key from secrets/configuration management
- [ ] Remove download-and-cleanup background jobs
- [ ] Monitor production render times and error rates for two weeks

---

## Before You Ship

The core win in this migration is architectural: your PDF pipeline moves from a remote service dependency to an in-process operation. Your HTML never leaves your server, your files never sit on a temporary URL with a 24-hour countdown, and your rendering is not gated by network latency or third-party availability.

**Worth knowing even without IronPDF:** if you are evaluating in-process alternatives and want to stay open-source, Puppeteer Sharp is a .NET port of Puppeteer that drives headless Chrome locally. It requires managing a Chrome installation but produces the same quality output as Api2pdf's Chrome endpoint. Playwright for .NET is another option with broader browser support.

Here is my question: if you are currently running Api2pdf at high volume, what is your download-and-cleanup strategy? Do you download synchronously in the request, run a background job, or use webhooks? I am curious how teams handle the temporary URL lifecycle in production—it is the part of the Api2pdf model that introduces the most operational complexity.

---

