---
title: "From PuppeteerSharp to IronPDF: what actually changes in your code"
published: false
tags: dotnet, csharp, pdf, migration
---

The version pin is a known problem. You're on PuppeteerSharp 10.x because 11.x changed the PDF API in a way that broke your pipeline, and the upgrade path requires touching code in places you'd rather not. Or you're on a newer version but Chrome's Puppeteer dependency keeps updating underneath you and some render subtlety shifts between browser versions. Either way, you're managing browser binaries in a PDF generation workflow, which is overhead you didn't sign up for.

This article covers the specific code changes when migrating from PuppeteerSharp to IronPDF. By the end, you'll have working before/after snippets for the four most common PDF operations. The comparison tables and checklist are useful even if you evaluate other libraries.

---

## Why Migrate (Without Drama)

Teams migrating from PuppeteerSharp to a dedicated PDF library commonly hit these conditions:

1. **Browser binary download in CI/Docker** — `BrowserFetcher.DownloadAsync()` or `RevisionInfo` downloads add hundreds of MB to CI step time and Docker cache misses.
2. **Version pinning friction** — PuppeteerSharp tracks Chrome releases; pinning a Chromium revision for render stability creates drift between the browser and the wrapper.
3. **Browser lifecycle management** — `IBrowser`, `IPage`, `BrowserLaunchOptions` are full browser automation objects; they're overhead for a PDF generation task.
4. **API churn between major versions** — PuppeteerSharp has had breaking changes between major releases (e.g., LaunchOptions vs BrowserTypeLaunchOptions patterns changed). Each upgrade requires code changes.
5. **Missing PDF manipulation features** — `Page.PdfAsync()` returns bytes; there's no PuppeteerSharp API for merge, split, watermark, security, or text extraction.
6. **Async browser context overhead** — every render requires creating and disposing a browser/page/context, which is significant overhead for single-document generation.
7. **Resource leaks under error conditions** — if `PdfAsync()` throws, ensuring the `IBrowser` and `IPage` are properly disposed requires careful `await using` / try-finally patterns.
8. **Temp file management** — some patterns require writing HTML to a temp file and navigating to `file://` to avoid content encoding issues.
9. **Docker image complexity** — caching the Chromium download directory in Docker layers adds container configuration complexity.
10. **Feature parity gap** — PDF headers/footers, security, watermark, and page manipulation all need secondary libraries.

### Comparison Table

| Aspect | PuppeteerSharp | IronPDF |
|---|---|---|
| Focus | Browser automation; PDF is secondary | PDF generation and manipulation |
| Pricing | MIT open source | Commercial license |
| API Style | Browser automation objects (`IBrowser`, `IPage`) | PDF-focused C# objects; no browser lifecycle |
| Learning Curve | High for PDF-only use | Low for .NET devs; focused PDF API |
| HTML Rendering | Downloaded Chromium binary | Embedded Chromium |
| Page Indexing | N/A — output only | 0-based |
| Thread Safety | Browser context isolation required | Single renderer reusable across threads |
| Namespace | `PuppeteerSharp` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | PuppeteerSharp | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | `SetContentAsync()` + `PdfAsync()` | `RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | `GoToAsync()` + `PdfAsync()` | `RenderUrlAsPdfAsync()` | Low |
| Save to file | `File.WriteAllBytesAsync()` | `pdf.SaveAs()` | Low |
| Save to MemoryStream | Wrap byte array | `pdf.Stream` | Low |
| Custom page size | `PdfOptions.Format` | `RenderingOptions.PaperSize` | Low |
| Custom margins | `PdfOptions.MarginOptions` | `RenderingOptions.Margin*` | Low |
| Headers/footers | `PdfOptions.HeaderTemplate` / `FooterTemplate` | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Browser lifecycle removal | Multiple objects + `await using` | No equivalent needed | Low (remove) |
| Merge PDFs | Not supported | `PdfDocument.Merge()` | Medium |
| Watermark | Not supported | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Not supported | `pdf.SecuritySettings` | Medium |
| Browser download step | `BrowserFetcher.DownloadAsync()` | N/A — bundled | Low (remove) |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| PDF generation only, no browser automation | Switch — eliminates browser lifecycle and download overhead |
| E2E testing + PDF generation in same codebase | Keep PuppeteerSharp for tests; add IronPDF for generation |
| CI/CD where Chromium download is a bottleneck | Switch — IronPDF bundles Chromium; no download step |
| Version pinning is causing ongoing maintenance overhead | Switch — IronPDF versioning is independent of Chrome releases |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All PuppeteerSharp References

```bash
# Find PDF-specific PuppeteerSharp usage
rg -l "PdfAsync\|PdfOptions\|IBrowser\|IPage\|BrowserFetcher" --type cs
rg "PdfAsync\|PdfOptions\|GoToAsync\|SetContentAsync" --type cs -n

# Find browser launch and download patterns
rg "LaunchAsync\|BrowserFetcher\|DownloadAsync\|FetchAsync" --type cs -n

# Find Chromium cache directory references in CI/Docker
grep -r "\.local-chromium\|chromium-pack\|BrowserFetcher\|PUPPETEER_PRODUCT" \
  Dockerfile .github/**/*.yml 2>/dev/null

# Check project files
grep -r "PuppeteerSharp" *.csproj **/*.csproj 2>/dev/null
```

### Uninstall / Install

```bash
# Remove PuppeteerSharp
dotnet remove package PuppeteerSharp

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

Remove the browser download step from Dockerfile and CI:

```bash
# Remove these patterns from CI config:
# - run: dotnet run --project ./BrowserDownloader
# - name: Cache Puppeteer Chromium
#   uses: actions/cache@v3
#   with:
#     path: ~/.local-chromium
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
    ?? throw new InvalidOperationException("IRONPDF_LICENSE_KEY not set");
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Threading.Tasks;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic Conversion

**Before:**
```csharp
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Threading.Tasks;
using System.IO;

class Program
{
    static async Task Main()
    {
        // Download Chromium on first use (or pre-downloaded)
        var fetcher = new BrowserFetcher();
        await fetcher.DownloadAsync();

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        await using var page = await browser.NewPageAsync();

        await page.SetContentAsync("<html><body><h1>Hello</h1></body></html>");
        var bytes = await page.PdfAsync(new PdfOptions { Format = PaperFormat.A4 });
        await File.WriteAllBytesAsync("output.pdf", bytes);
        Console.WriteLine("Saved output.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// No browser download, no browser lifecycle management
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Hello</h1></body></html>");
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| PuppeteerSharp | IronPDF | Notes |
|---|---|---|
| `PuppeteerSharp` | `IronPdf` | Core namespace |
| `PuppeteerSharp.Media` | `IronPdf.Rendering` | Paper size, margin types |
| N/A | `IronPdf.Editing` | Watermark / stamp operations |

### Core Class Mapping

| PuppeteerSharp Class | IronPDF Class | Description |
|---|---|---|
| `BrowserFetcher` | N/A — removed | Browser download step eliminated |
| `IBrowser` + `IPage` | `ChromePdfRenderer` | Replace multi-object lifecycle with single renderer |
| `PdfOptions` | `ChromePdfRenderOptions` | Page size, margins, headers, print background |
| N/A | `PdfDocument` | PDF object returned by renderer |

### Document Loading Methods

| Operation | PuppeteerSharp | IronPDF |
|---|---|---|
| HTML string | `page.SetContentAsync(html)` + `PdfAsync()` | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | `page.GoToAsync(url)` + `PdfAsync()` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML file | `page.GoToAsync("file:///...")` + `PdfAsync()` | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Load existing PDF | N/A | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | PuppeteerSharp | IronPDF |
|---|---|---|
| Page count | N/A — output only | `pdf.PageCount` |
| Remove page | N/A | `pdf.RemovePages(index)` |
| Extract text | N/A | `pdf.ExtractAllText()` |
| Copy pages | N/A | `pdf.CopyPages(start, end)` |

### Merge / Split Operations

| Operation | PuppeteerSharp | IronPDF |
|---|---|---|
| Merge | Not supported | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not supported | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before:**
```csharp
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.IO;
using System.Threading.Tasks;

class HtmlToPdfBefore
{
    static async Task Main()
    {
        var fetcher = new BrowserFetcher();
        await fetcher.DownloadAsync();

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
        });

        await using var page = await browser.NewPageAsync();

        var html = @"
            <html>
            <head>
            <style>
                body { font-family: Arial, sans-serif; padding: 40px; }
                .total { font-weight: bold; border-top: 2px solid #333; }
            </style>
            </head>
            <body>
                <h1>Invoice #3001</h1>
                <p>Client: Acme Corp</p>
                <div class='total'>Total Due: $4,500.00</div>
            </body>
            </html>";

        await page.SetContentAsync(html);
        var pdfBytes = await page.PdfAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions { Top = "40px", Bottom = "40px" }
        });

        await File.WriteAllBytesAsync("invoice.pdf", pdfBytes);
        Console.WriteLine($"Saved invoice.pdf ({pdfBytes.Length} bytes)");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; }
        .total { font-weight: bold; border-top: 2px solid #333; }
    </style>
    </head>
    <body>
        <h1>Invoice #3001</h1>
        <p>Client: Acme Corp</p>
        <div class='total'>Total Due: $4,500.00</div>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 40;
renderer.RenderingOptions.MarginBottom = 40;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");

Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (PuppeteerSharp — no native merge):**
```csharp
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

class MergeBefore
{
    static async Task Main()
    {
        var fetcher = new BrowserFetcher();
        await fetcher.DownloadAsync();
        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

        var sections = new[]
        {
            ("<html><body><h1>Chapter 1: Introduction</h1></body></html>", "ch1.pdf"),
            ("<html><body><h1>Chapter 2: Analysis</h1></body></html>", "ch2.pdf"),
        };

        foreach (var (html, filename) in sections)
        {
            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(html);
            var bytes = await page.PdfAsync();
            await File.WriteAllBytesAsync(filename, bytes);
        }

        // PuppeteerSharp has no merge API — requires another library
        // e.g., PDFsharp, iTextSharp, etc.
        Console.WriteLine("Merge requires a secondary library — not built into PuppeteerSharp");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();

// Render concurrently
var results = await Task.WhenAll(
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Chapter 1: Introduction</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Chapter 2: Analysis</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("document.pdf");

Console.WriteLine($"Merged document: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (PuppeteerSharp — CSS injection only; no stamp API):**
```csharp
using PuppeteerSharp;
using System;
using System.IO;
using System.Threading.Tasks;

class WatermarkBefore
{
    static async Task Main()
    {
        var fetcher = new BrowserFetcher();
        await fetcher.DownloadAsync();
        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        await using var page = await browser.NewPageAsync();

        // No watermark API in PuppeteerSharp — CSS injection required
        // CSS watermark rendering varies by template; this is a workaround
        var html = @"
            <html>
            <head>
            <style>
                body::before {
                    content: 'INTERNAL';
                    position: fixed;
                    top: 50%; left: 50%;
                    transform: translate(-50%, -50%) rotate(-45deg);
                    font-size: 100px;
                    color: rgba(200, 200, 200, 0.3);
                    z-index: 9999;
                    pointer-events: none;
                }
            </style>
            </head>
            <body><h1>Internal Document</h1><p>Sensitive content.</p></body>
            </html>";

        await page.SetContentAsync(html);
        var bytes = await page.PdfAsync(new PdfOptions { PrintBackground = true });
        await File.WriteAllBytesAsync("internal.pdf", bytes);
        Console.WriteLine("CSS watermark applied — fidelity depends on HTML template");
    }
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Editing;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Internal Document</h1><p>Sensitive content.</p></body></html>"
);

// Post-render watermark — applied consistently regardless of HTML content
// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "INTERNAL",
    FontColor = IronSoftware.Drawing.Color.Gray,
    Opacity = 20,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("internal.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (PuppeteerSharp — no security API):**
```csharp
using PuppeteerSharp;
using System;
using System.IO;
using System.Threading.Tasks;

class PasswordBefore
{
    static async Task Main()
    {
        var fetcher = new BrowserFetcher();
        await fetcher.DownloadAsync();
        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        await using var page = await browser.NewPageAsync();

        await page.SetContentAsync("<html><body><h1>Secured Report</h1></body></html>");

        // PuppeteerSharp PdfAsync() has no password/encryption options
        var bytes = await page.PdfAsync();

        // Post-process with another library required — adds a dependency
        // Pseudo-code: bytes = SomePdfLib.SetPassword(bytes, "open123");

        await File.WriteAllBytesAsync("report.pdf", bytes);
        Console.WriteLine("Password protection requires a secondary library with PuppeteerSharp");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Secured Report</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured-report.pdf");
Console.WriteLine("Saved secured-report.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### Browser Download Step Removal

The most impactful infrastructure change is removing the Chromium download step. In PuppeteerSharp, this typically appears as:

```csharp
// Remove this pattern — IronPDF bundles Chromium
var fetcher = new BrowserFetcher();
await fetcher.DownloadAsync();
// Or: await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
```

Find and remove all occurrences:

```bash
rg "BrowserFetcher\|DownloadAsync.*chromium\|FetchAsync" --type cs -n
```

### `LaunchOptions.Args` to Rendering Options

PuppeteerSharp `LaunchOptions.Args` often included `--no-sandbox` and `--disable-dev-shm-usage` for Docker compatibility. IronPDF manages sandbox configuration internally for supported environments.

```csharp
// IronPDF manages sandbox settings internally.
// For Docker/container deployment guidance, see:
// https://ironpdf.com/how-to/azure/
```

### `PdfOptions.HeaderTemplate` Migration

PuppeteerSharp supports `HeaderTemplate` and `FooterTemplate` in `PdfOptions` as HTML strings. IronPDF uses `RenderingOptions.HtmlHeader` and `HtmlFooter`:

```csharp
// PuppeteerSharp:
// new PdfOptions {
//   DisplayHeaderFooter = true,
//   HeaderTemplate = "<div style='font-size:9px; text-align:right'><span class='pageNumber'></span></div>",
//   FooterTemplate = "<div style='font-size:9px; text-align:center'><span class='title'></span></div>",
// }

// IronPDF equivalent:
// https://ironpdf.com/how-to/headers-and-footers/
// IronPDF placeholders: {page}, {total-pages}, {html-title}, {url}, {date}, {time}
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='font-size:9px; text-align:right'>{page} of {total-pages}</div>",
};
```

### Margin Unit Conversion

PuppeteerSharp `MarginOptions` uses CSS string values (`"40px"`, `"1cm"`, `"0.5in"`). IronPDF `RenderingOptions.Margin*` properties use numeric millimeters. Conversion reference: `1 inch = 25.4 mm`.

```csharp
// PuppeteerSharp: MarginOptions { Top = "1cm", Bottom = "1cm" }
// IronPDF: numeric millimeters
renderer.RenderingOptions.MarginTop = 10;    // 10 mm
renderer.RenderingOptions.MarginBottom = 10; // 10 mm
// See: https://ironpdf.com/how-to/custom-margins/
```

### Page Indexing

PuppeteerSharp's `PdfAsync()` produces output-only PDFs with no page manipulation. If you add IronPDF page operations, use 0-based indexing.

---

## Performance Considerations

### Eliminating Browser Download Overhead

In CI environments, PuppeteerSharp's browser download adds time on cache misses. IronPDF's Chromium is bundled in the NuGet package — it's part of the `dotnet restore` step, not a separate download.

### Browser Startup vs In-Process Rendering

Each PuppeteerSharp render requires launching or reusing a browser process. IronPDF renders in-process. For sustained workloads, the difference is meaningful — benchmark for your specific load pattern.

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Warm up to avoid cold-start skewing measurements
var renderer = new ChromePdfRenderer();
using var _ = await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");

// Benchmark
var times = new List<double>();
for (int i = 0; i < 20; i++)
{
    var sw = Stopwatch.StartNew();
    using var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Benchmark</h1></body></html>");
    sw.Stop();
    times.Add(sw.Elapsed.TotalMilliseconds);
}

times.Sort();
Console.WriteLine($"Avg: {times.Average():F1}ms | P95: {times[(int)(times.Count * 0.95)]:F1}ms");
Console.WriteLine("Compare against PuppeteerSharp render time in your environment");
```

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/examples/parallel/
var htmlJobs = Enumerable.Range(1, 8)
    .Select(i => $"<html><body><h1>Document {i}</h1></body></html>")
    .ToArray();

var pdfs = await Task.WhenAll(htmlJobs.Select(async html =>
{
    var r = new ChromePdfRenderer();
    return await r.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Rendered {pdfs.Length} PDFs in parallel");
foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

// PuppeteerSharp required 'await using' on IBrowser and IPage
// IronPDF: use standard 'using' on PdfDocument
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// MemoryStream pattern
using var ms = new MemoryStream();
pdf.Stream.CopyTo(ms);
return ms.ToArray();
// pdf disposed here
```

---

## Migration Checklist

### Pre-Migration
- [ ] Identify all PuppeteerSharp PDF calls (`rg "PdfAsync\|PdfOptions\|GoToAsync\|SetContentAsync" --type cs`)
- [ ] Separate PDF generation code from browser automation / scraping code
- [ ] Identify secondary libraries used for merge, security, or watermark
- [ ] Find `BrowserFetcher` calls to remove (`rg "BrowserFetcher\|DownloadAsync" --type cs`)
- [ ] Find CI/Docker caching steps for Chromium to remove
- [ ] Document current render times for comparison benchmarking
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `PuppeteerSharp` package reference
- [ ] Add license key at application startup
- [ ] Remove `BrowserFetcher.DownloadAsync()` calls
- [ ] Replace `Puppeteer.LaunchAsync()` + `browser.NewPageAsync()` with `ChromePdfRenderer`
- [ ] Replace `page.SetContentAsync()` + `PdfAsync()` with `RenderHtmlAsPdfAsync()`
- [ ] Replace `page.GoToAsync()` + `PdfAsync()` with `RenderUrlAsPdfAsync()`
- [ ] Convert `PdfOptions.Format` / `PaperFormat.*` to `RenderingOptions.PaperSize`
- [ ] Convert `MarginOptions` CSS strings to `RenderingOptions.Margin*` numeric values
- [ ] Migrate `HeaderTemplate` / `FooterTemplate` to `RenderingOptions.HtmlHeader/Footer`

### Testing
- [ ] Render each HTML template and compare visual output
- [ ] Verify `PrintBackground` behavior — IronPDF renders backgrounds by default
- [ ] Verify margins — check unit conversion from CSS strings to IronPDF numeric values
- [ ] Test headers and footers render on all pages
- [ ] Test merge output page count and order
- [ ] Test password-protected PDFs open correctly
- [ ] Verify Docker build succeeds without Chromium download step
- [ ] Benchmark render time vs PuppeteerSharp baseline

### Post-Migration
- [ ] Remove `PuppeteerSharp` NuGet package
- [ ] Remove Chromium cache directories from `.gitignore` and Docker `.dockerignore`
- [ ] Update CI config — remove Chromium download/cache steps
- [ ] Remove secondary PDF manipulation libraries consolidated into IronPDF

---

## Final Thoughts

The version pinning problem that may have started this evaluation is resolved structurally: IronPDF's version is independent of Chrome release cadence. The browser lifecycle code deletion is the most satisfying part of this migration — `BrowserFetcher`, `LaunchAsync`, `NewPageAsync`, `await using` blocks — all of that goes away.

The edge cases most likely to cause test failures: margin unit conversion (CSS string to numeric), header/footer template token differences, and any HTML that relied on specific Chromium revision behavior.

**Discussion question:** What edge cases did you hit that this article didn't cover — specifically around the header/footer template migration or Docker environment issues?
