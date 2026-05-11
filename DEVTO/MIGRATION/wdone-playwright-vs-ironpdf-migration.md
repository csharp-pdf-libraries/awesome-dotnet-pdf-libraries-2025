# Playwright to IronPDF: three steps and you're done

The GitHub repository hasn't had a meaningful commit in two years. Issues sit open with no response. The NuGet package version number hasn't moved while the .NET ecosystem has shipped three major versions. That's the pattern — not a catastrophic failure, just a slow fade. If you're using Playwright's .NET bindings specifically for PDF generation, you may have noticed that the wrapper layer has its own maintenance rhythm separate from the upstream Playwright project, and that rhythm has sometimes slowed.

This article covers migrating Playwright-based PDF generation to IronPDF. By the end, you'll have working before/after code for HTML-to-PDF, merge, watermark, and password protection. The comparison tables and checklist are useful regardless of which library you choose as a replacement.

> **Note on scope:** This article focuses on the PDF generation use case of Playwright. If your codebase uses Playwright for browser automation, E2E testing, or scraping, those use cases don't transfer to IronPDF — Playwright is the right tool for that work. This guide is for teams whose primary Playwright use case is generating PDFs.

---

## Why Migrate (Without Drama)

Teams using Playwright only for PDF generation frequently reassess when:

1. **Browser binary management in CI** — `playwright install chromium` in every CI run or Docker build adds 300MB+ download and caching complexity.
2. **Startup latency** — launching a full browser instance per PDF generation request adds measurable latency, especially on cold starts.
3. **Docker image size** — a Chromium install in the image increases final image size significantly.
4. **API surface mismatch** — Playwright's PDF API is minimal (`Page.PdfAsync()`); it doesn't expose merge, split, watermark, security, or text extraction. Those need additional libraries.
5. **Async browser lifecycle management** — managing `IBrowser`, `IPage`, `IBrowserContext` objects, launch options, and proper disposal is overhead for what is ultimately a PDF generation task.
6. **Version pinning friction** — Playwright's browser binary version and NuGet package version must stay in sync; version drift causes subtle rendering differences.
7. **Third-party wrapper packages** — if you're using a wrapper on top of Playwright for PDF, its maintenance status is separate from the core Playwright project.
8. **Missing PDF manipulation features** — Playwright produces a PDF file; it doesn't manipulate it. Merge, watermark, and security all require adding another library.
9. **Test tooling bleed** — Playwright is optimized for testing; using it in production code paths exposes you to API changes designed for test workflows.
10. **Licensing considerations** — verify current Playwright licensing terms for production use at scale.

### Comparison Table

| Aspect | Playwright (.NET) | IronPDF |
|---|---|---|
| Focus | Browser automation + E2E testing; PDF is secondary | PDF generation and manipulation |
| Pricing | MIT license (core); verify production terms | Commercial license — verify at ironsoftware.com |
| API Style | Async browser lifecycle management | PDF-focused C# objects; no browser lifecycle to manage |
| Learning Curve | High for PDF-only use; full browser API surface | Low for .NET devs; focused PDF API |
| HTML Rendering | Chromium (full browser) | Embedded Chromium |
| Page Indexing | N/A — not a PDF manipulation library | 0-based |
| Thread Safety | Browser context isolation required | Verify IronPDF concurrent instance guidance |
| Namespace | `Microsoft.Playwright` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Playwright Approach | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | Navigate to `data:text/html,...` + `PdfAsync()` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | `Page.GotoAsync(url)` + `PdfAsync()` | `ChromePdfRenderer.RenderUrlAsPdfAsync()` | Low |
| Save to file | `File.WriteAllBytesAsync()` on PDF bytes | `pdf.SaveAs(path)` | Low |
| Save to MemoryStream | Wrap byte array | `pdf.Stream` | Low |
| Custom page size | `PdfOptions.Format` | `RenderingOptions.PaperSize` | Low |
| Custom margins | `PdfOptions.Margin` | `RenderingOptions.Margin*` | Low |
| Headers/footers | Not supported via `PdfAsync()` — CSS only | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Merge PDFs | Not supported — requires secondary library | `PdfDocument.Merge()` | Medium |
| Watermark | Not supported — requires secondary library | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Not supported | `pdf.SecuritySettings` | Medium |
| Browser lifecycle management | `await using IBrowserContext` etc. | N/A — no browser lifecycle | Low (remove) |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| PDF generation only, no browser automation needed | Switch — IronPDF is purpose-built; eliminates browser lifecycle overhead |
| Mix of E2E testing and PDF generation in the same project | Keep Playwright for testing; add IronPDF for generation |
| CI/CD environment where Docker image size is critical | Switch — eliminates Playwright browser install step |
| PDF requires JavaScript-heavy SPA rendering with complex auth flows | Playwright may be better suited; IronPDF can also render URLs with JS |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial key](https://ironpdf.com/how-to/license-keys/)
- Note: IronPDF bundles its own Chromium; no separate browser install step needed

### Find All Playwright PDF References

```bash
# Find Playwright PDF-specific usage
rg -l "PdfAsync\|PdfOptions\|\.Pdf\b" --type cs
rg "PdfAsync\|PdfOptions" --type cs -n

# Find browser lifecycle code that may be PDF-specific
rg "IBrowser\|IPage\|IBrowserContext\|LaunchAsync" --type cs -n

# Find playwright install references in CI/Docker
grep -r "playwright install\|playwright-chromium\|playwright install-deps" \
  Dockerfile .github/**/*.yml .github/**/*.yaml 2>/dev/null

# Check project files
grep -r "Microsoft.Playwright" *.csproj **/*.csproj 2>/dev/null
```

### Uninstall / Install

```bash
# Remove Playwright NuGet package
dotnet remove package Microsoft.Playwright

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

Also remove the browser install step from your Dockerfile and CI pipeline:

```bash
# Remove these lines from your Dockerfile / CI config:
# RUN dotnet playwright install chromium
# RUN dotnet playwright install-deps
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// Set before any IronPDF API call
// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
    ?? throw new InvalidOperationException("IRONPDF_LICENSE_KEY not set");
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using Microsoft.Playwright;
using System.Threading.Tasks;
```

**After:**
```csharp
using IronPdf;
using System;
```

### Step 3 — Basic Conversion

**Before:**
```csharp
using Microsoft.Playwright;
using System.Threading.Tasks;
using System.IO;

class Program
{
    static async Task Main()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.SetContentAsync("<html><body><h1>Hello</h1></body></html>");

        var pdfBytes = await page.PdfAsync(new PagePdfOptions
        {
            Format = "A4",
        });

        await File.WriteAllBytesAsync("output.pdf", pdfBytes);
        Console.WriteLine("Saved output.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

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

| Playwright | IronPDF | Notes |
|---|---|---|
| `Microsoft.Playwright` | `IronPdf` | Core namespace |
| `Microsoft.Playwright.PagePdfOptions` | `IronPdf.Rendering.ChromePdfRenderOptions` | Rendering config |
| N/A | `IronPdf.Editing` | Watermark / stamp operations |

### Core Class Mapping

| Playwright Class | IronPDF Class | Description |
|---|---|---|
| `IPlaywright` / `Playwright` | N/A — no equivalent lifecycle | IronPDF has no global init object |
| `IBrowser` / `IPage` | `ChromePdfRenderer` | Replace browser + page with single renderer |
| `PagePdfOptions` | `ChromePdfRenderOptions` | Page size, margins, print background |
| N/A | `PdfDocument` | PDF object returned by renderer |

### Document Loading Methods

| Operation | Playwright | IronPDF |
|---|---|---|
| HTML string | `page.SetContentAsync(html)` + `PdfAsync()` | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | `page.GotoAsync(url)` + `PdfAsync()` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML file | `page.GotoAsync("file://...")` + `PdfAsync()` | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Load existing PDF | N/A | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | Playwright | IronPDF |
|---|---|---|
| Page count | N/A — output only | `pdf.PageCount` |
| Remove page | N/A | `pdf.RemovePage(index)` — verify |
| Rotate | N/A | Verify in IronPDF docs |
| Extract text | N/A | `pdf.ExtractAllText()` |

### Merge / Split Operations

| Operation | Playwright | IronPDF |
|---|---|---|
| Merge | Not supported — requires secondary library | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not supported | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before:**
```csharp
using Microsoft.Playwright;
using System;
using System.IO;
using System.Threading.Tasks;

class HtmlToPdfBefore
{
    static async Task Main()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
        await using var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        var html = "<html><body><h1>Invoice #2042</h1><p>Due: $1,200</p></body></html>";
        await page.SetContentAsync(html);

        var pdfBytes = await page.PdfAsync(new PagePdfOptions
        {
            Format = "A4",
            PrintBackground = true,
            Margin = new Margin { Top = "40px", Bottom = "40px" }
        });

        await File.WriteAllBytesAsync("invoice.pdf", pdfBytes);
        Console.WriteLine($"Saved {pdfBytes.Length} bytes to invoice.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var html = "<html><body><h1>Invoice #2042</h1><p>Due: $1,200</p></body></html>";

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

**Before (Playwright — no native merge; secondary library pattern):**
```csharp
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

class MergeBefore
{
    static async Task Main()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var pdfBytesList = new List<byte[]>();

        var sections = new[]
        {
            "<html><body><h1>Section 1</h1></body></html>",
            "<html><body><h1>Section 2</h1></body></html>",
        };

        foreach (var html in sections)
        {
            var page = await browser.NewPageAsync();
            await page.SetContentAsync(html);
            pdfBytesList.Add(await page.PdfAsync());
        }

        // Playwright doesn't merge PDFs — requires a secondary library
        // Pseudo-code: var merged = SomePdfLib.Merge(pdfBytesList);
        // File.WriteAllBytes("merged.pdf", merged);
        Console.WriteLine("Merge requires a separate library alongside Playwright");
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

var pdf1 = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 1</h1></body></html>");
var pdf2 = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 2</h1></body></html>");

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");

Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (Playwright — CSS injection only):**
```csharp
using Microsoft.Playwright;
using System;
using System.IO;
using System.Threading.Tasks;

class WatermarkBefore
{
    static async Task Main()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        // Playwright has no watermark API — inject via CSS
        var html = @"
            <html>
            <head>
            <style>
                body::after {
                    content: 'DRAFT';
                    position: fixed; top: 50%; left: 50%;
                    transform: translate(-50%, -50%) rotate(-45deg);
                    font-size: 100px; opacity: 0.1; color: #999;
                    pointer-events: none; z-index: 9999;
                }
            </style>
            </head>
            <body><h1>Report</h1></body>
            </html>";

        await page.SetContentAsync(html);
        var bytes = await page.PdfAsync(new PagePdfOptions { PrintBackground = true });
        await File.WriteAllBytesAsync("watermarked.pdf", bytes);
        Console.WriteLine("Watermark via CSS — fidelity varies");
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
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Report</h1></body></html>");

// Post-render programmatic watermark
// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronPdf.Imaging.Color.Gray,
    Opacity = 0.15,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (Playwright — not supported):**
```csharp
using Microsoft.Playwright;
using System;
using System.IO;
using System.Threading.Tasks;

class PasswordBefore
{
    static async Task Main()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.SetContentAsync("<html><body><h1>Confidential</h1></body></html>");
        var bytes = await page.PdfAsync();

        // Playwright PdfAsync() has no password/security options
        // Teams must post-process with another library
        // Pseudo-code: bytes = SomePdfLib.SetPassword(bytes, "open123", "admin456");

        await File.WriteAllBytesAsync("document.pdf", bytes);
        Console.WriteLine("Password protection requires a secondary library with Playwright");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Confidential</h1></body></html>");

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("protected.pdf");
Console.WriteLine("Saved protected.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### Browser Lifecycle Removal

The largest code change in this migration is removing browser lifecycle management. In Playwright, every PDF operation required:

```csharp
// Playwright lifecycle — replace all of this:
using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(...);
await using var context = await browser.NewContextAsync(...);
var page = await context.NewPageAsync();
// ... generate PDF ...
// browser disposes via 'await using'
```

In IronPDF, the equivalent is:

```csharp
// IronPDF — no browser lifecycle management
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

Search your codebase for these patterns and scope the removal work:

```bash
rg "Playwright.CreateAsync\|LaunchAsync\|NewContextAsync\|NewPageAsync" --type cs -n
```

### Page Indexing

IronPDF uses 0-based page indexing. Playwright produces output-only PDFs with no page manipulation API, so this typically only affects new IronPDF manipulation code rather than existing logic.

### PrintBackground Default

Playwright's `PdfAsync()` defaults `PrintBackground` to `false`, which can make PDFs look different from browser preview. IronPDF renders CSS backgrounds by default. If your HTML has background colors or images, verify the output matches expectations.

```csharp
// Playwright equivalent of PrintBackground = true:
// renderer.RenderingOptions.PrintBackground is enabled by default in IronPDF
// Verify current default in IronPDF docs if output differs
```

### Margin Units

Playwright's `PdfOptions.Margin` uses string values with CSS units (`"40px"`, `"1cm"`). IronPDF's `RenderingOptions.Margin*` properties use numeric values. Verify the unit convention in current IronPDF documentation before converting.

```csharp
// Playwright: "40px", "1cm", "0.5in"
// IronPDF: numeric values — verify unit (pixels? mm?) in docs
renderer.RenderingOptions.MarginTop = 40;    // verify unit in IronPDF docs
renderer.RenderingOptions.MarginBottom = 40; // https://ironpdf.com/how-to/custom-margins/
```

---

## Performance Considerations

### Eliminating Browser Startup Overhead

Playwright starts a full browser instance per logical operation context. IronPDF initializes Chromium internally and reuses it across renders. For high-throughput scenarios, this is a meaningful difference — the browser launch overhead in Playwright adds latency that doesn't exist in IronPDF.

Warm up the renderer at application startup if you want the first production request to avoid initialization delay:

```csharp
using IronPdf;

// Warm up during application startup
var renderer = new ChromePdfRenderer();
await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");
// Subsequent renders avoid Chromium cold-start latency
```

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Parallel rendering: https://ironpdf.com/examples/parallel/
var htmlJobs = new[]
{
    "<html><body><h1>Doc 1</h1></body></html>",
    "<html><body><h1>Doc 2</h1></body></html>",
    "<html><body><h1>Doc 3</h1></body></html>",
};

var tasks = htmlJobs.Select(async html =>
{
    // Verify per-task vs shared renderer guidance in IronPDF docs
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
});

var pdfs = await Task.WhenAll(tasks);
Console.WriteLine($"Rendered {pdfs.Length} PDFs");
foreach (var pdf in pdfs) pdf.Dispose();

// See async guide: https://ironpdf.com/how-to/async/
```

### Disposal

```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();

// Use 'using' on PdfDocument for prompt resource release
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
using var ms = new MemoryStream();
pdf.Stream.CopyTo(ms);
return ms.ToArray();
// pdf disposed at end of block
```

### Docker Image Size Impact

With Playwright, your Dockerfile included a `playwright install chromium` step adding ~300MB. With IronPDF, the Chromium binary is bundled in the NuGet package. The net effect on Docker image size depends on your base image and layer caching strategy — measure for your specific image.

```dockerfile
# Remove these Playwright-specific lines:
# RUN dotnet playwright install chromium
# RUN dotnet playwright install-deps chromium

# IronPDF bundles Chromium — no separate install step needed
COPY . .
RUN dotnet publish -c Release -o out
```

---

## Migration Checklist

### Pre-Migration
- [ ] Identify all Playwright PDF usage (`rg "PdfAsync\|PdfOptions" --type cs`)
- [ ] Separate PDF generation code from browser automation / E2E test code
- [ ] Identify secondary PDF libraries used to supplement Playwright (merge, security, etc.)
- [ ] Remove `playwright install chromium` from Dockerfile and CI config
- [ ] Document current PDF render times (baseline for comparison)
- [ ] Verify PrintBackground behavior in existing PDFs
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET framework compatibility

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `Microsoft.Playwright` package reference
- [ ] Add license key at application startup
- [ ] Replace `Playwright.CreateAsync()` + `LaunchAsync()` + `NewPageAsync()` with `ChromePdfRenderer`
- [ ] Replace `page.SetContentAsync()` + `PdfAsync()` with `RenderHtmlAsPdfAsync()`
- [ ] Replace `page.GotoAsync()` + `PdfAsync()` with `RenderUrlAsPdfAsync()`
- [ ] Convert `PdfOptions.Format` to `RenderingOptions.PaperSize`
- [ ] Convert `PdfOptions.Margin` strings to `RenderingOptions.Margin*` numeric values
- [ ] Replace CSS-injected watermarks with `TextStamper` / `ImageStamper`
- [ ] Replace secondary security library with `pdf.SecuritySettings`

### Testing
- [ ] Render each HTML template and compare visual output
- [ ] Verify background colors and images render (PrintBackground default difference)
- [ ] Verify margins match — unit conversion from px/cm to IronPDF units
- [ ] Test password-protected PDFs open with correct passwords
- [ ] Test merge output page count and order
- [ ] Verify Docker image builds without `playwright install` step
- [ ] Benchmark render time vs Playwright baseline

### Post-Migration
- [ ] Remove `Microsoft.Playwright` NuGet package
- [ ] Remove secondary PDF manipulation libraries consolidated into IronPDF
- [ ] Update Dockerfile and CI — remove Playwright browser install steps
- [ ] Update `PLAYWRIGHT_BROWSERS_PATH` environment variable references if present

---

## That's the Migration

The main code volume in this migration is browser lifecycle removal — `CreateAsync`, `LaunchAsync`, `NewContextAsync`, `NewPageAsync` all go away. That's mechanical but can touch a lot of files if PDF generation is spread across the codebase rather than centralized in a service class.

The secondary libraries that were added because Playwright's `PdfAsync()` doesn't support merge, watermark, or security are often the part that takes the most thought — audit those carefully before claiming the migration is complete.

**Discussion question:** Which Playwright feature was hardest to replicate in IronPDF — and was it something the article covered, or an edge case specific to your implementation?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
