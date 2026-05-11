---
title: "Moving off PeachPDF: practical IronPDF migration notes"
published: false
tags: dotnet, csharp, pdf, migration
---

The .NET upgrade was supposed to be the boring part. Update the TFM, fix a few deprecated APIs, run the tests — done. But somewhere in that process, the PDF library started causing problems: maybe PeachPDF's `net8.0`-only target boxes you out of a multi-TFM library, or its layout engine doesn't render the CSS your reports rely on. PeachPDF is a small, single-maintainer HTML-to-PDF library (pre-1.0, BSD-3-Clause) and that scope shows up quickly the moment you need anything beyond emitting a freshly generated PDF from an HTML string.

This article covers migrating from PeachPDF to IronPDF, with emphasis on the troubleshooting patterns that come up when switching between HTML renderers on a modern .NET stack.

---

## Why Migrate (Without Drama)

Teams moving off PeachPDF commonly cite some combination of these conditions:

1. **Pre-1.0 feature set** — PeachPDF (latest `0.7.26`, October 2025) ships HTML-to-PDF only. There is no API to load an existing PDF, merge, split, watermark via API, password-protect, or sign.
2. **.NET 8 only target** — the package targets `net8.0`. Library authors who need to support `net6.0` or `netstandard2.0` consumers cannot pull it in.
3. **Layout engine, not a browser** — PeachPDF renders an HTML+CSS subset on top of PdfSharpCore. There is no JavaScript engine, so SPA pages and any layout that depends on JS execution will not render.
4. **CSS fidelity gaps** — modern features like CSS Grid and complex Flexbox are partial or not guaranteed in a non-browser layout engine.
5. **Maintenance profile** — a single GitHub maintainer (`jhaygood86`) and a pre-1.0 version line are a fine fit for hobby projects, but a tight bus-factor for production systems.
6. **No header/footer API** — page chrome has to be embedded in the source HTML with `position: fixed`. There's no `HtmlHeader`/`HtmlFooter` equivalent.
7. **Stream-only save target** — `document.Save(Stream)` is the only output path. There's no `SaveAs(string path)` convenience.
8. **No enterprise support** — there's no commercial SLA, paid support, or guaranteed response window if production breaks at 2 AM.
9. **Limited documentation** — a single README is the canonical reference; community Q&A is correspondingly thin.
10. **Secondary library accretion** — because PeachPDF only generates new PDFs, teams typically pair it with PdfSharpCore or another library for merge/edit. Migration is an opportunity to consolidate.

### Comparison Table

| Aspect | PeachPDF | IronPDF |
|---|---|---|
| Focus | HTML-to-PDF generation only | HTML-to-PDF + full PDF manipulation |
| License & pricing | BSD-3-Clause, free | Commercial license — pricing at [ironpdf.com](https://ironpdf.com/) |
| API style | `PdfGenerator` + `PdfGenerateConfig`, async-only | Fluent `ChromePdfRenderer` returning `PdfDocument` |
| Learning curve | Small surface area, but pre-1.0 | Low for .NET developers |
| HTML rendering | Layout engine on PdfSharpCore | Embedded Chromium (full CSS3) |
| JavaScript | Not executed | Full ES2024 |
| .NET targets | `net8.0` only | Broad, including modern .NET versions |
| Namespace | `PeachPDF` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | PeachPDF | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | `await generator.GeneratePdf(html, config)` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | `HttpClientNetworkAdapter` wired through config | `ChromePdfRenderer.RenderUrlAsPdfAsync()` | Low |
| Save to file | `document.Save(Stream)` (Stream only) | `pdf.SaveAs(path)` | Low |
| Save to bytes/stream | `document.Save(memStream)` then `.ToArray()` | `pdf.BinaryData` / `pdf.Stream` | Low |
| Custom page size | `PdfGenerateConfig.PageSize` | `RenderingOptions.PaperSize` | Low |
| Page orientation | `PdfGenerateConfig.PageOrientation` | `RenderingOptions.PaperOrientation` | Low |
| Headers/footers | Manual `position: fixed` in source HTML | `RenderingOptions.HtmlHeader/HtmlFooter` | Medium |
| JavaScript rendering | Not supported | Built-in; `RenderingOptions.WaitFor` | Medium |
| Merge | Not supported (drop to PdfSharpCore) | `PdfDocument.Merge()` | Medium |
| Watermark | Manual in source HTML | `pdf.ApplyWatermark()` / `TextStamper` | Medium |
| Password protection | Not supported | `pdf.SecuritySettings` | Medium |
| Digital signatures | Not supported | `IronPdf.Signing.PdfSignature` | Medium |
| Async rendering | Async-only API | Built-in async methods | Low |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Need to load, merge, split, or sign existing PDFs | Switch — PeachPDF is HTML-to-PDF only |
| Need to support `net6.0` or `netstandard2.0` consumers | Switch — PeachPDF targets `net8.0` only |
| CSS rendering fidelity is critical (Grid, Flexbox, web fonts) | Switch — Chromium renders current CSS standards |
| HTML depends on JavaScript execution | Switch — PeachPDF does not execute JS |
| All usage is basic HTML-to-PDF on `net8.0` with no manipulation | PeachPDF may be sufficient; commercial licensing trade-off is real |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9 target framework
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)
- Current codebase compiling on the new .NET version (or identify that PeachPDF is the blocking issue)

### Find All PeachPDF References

```bash
# Search for PeachPDF API usage
rg -l "PeachPDF" --type cs
rg "PdfGenerator|PdfGenerateConfig|GeneratePdf" --type cs -n

# Check project files
rg "PeachPDF" --type-add 'csproj:*.csproj' --type csproj -n

# Find HTML template strings that feed the PDF library
rg "GeneratePdf|HtmlConverter|HtmlToPdf" --type cs -n

# Find any rendering options or configuration classes
rg "PageSize|PageOrientation|NetworkAdapter" --type cs -n
```

### Uninstall / Install

```bash
# Remove PeachPDF
dotnet remove package PeachPDF

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

If PeachPDF was blocking the `dotnet build` on your new TFM, confirm the build succeeds after this swap before writing any migration code.

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// Set at application startup — https://ironpdf.com/how-to/license-keys/
// Set via environment variable (recommended for server deployments):
// export IRONPDF_LICENSE_KEY=YOUR-LICENSE-KEY

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
    ?? throw new InvalidOperationException("IRONPDF_LICENSE_KEY environment variable not set");
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using PeachPDF;
using PeachPDF.PdfSharpCore;
// PeachPDF.Network is added when you wire up URL fetching via HttpClientNetworkAdapter
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic Conversion

**Before (PeachPDF):**
```csharp
using PeachPDF;
using PeachPDF.PdfSharpCore;
using System.IO;
using System.Threading.Tasks;

class PeachPdfBefore
{
    static async Task Main()
    {
        var html = "<html><body><h1>Hello</h1></body></html>";

        var pdfConfig = new PdfGenerateConfig
        {
            PageSize = PageSize.A4,
            PageOrientation = PageOrientation.Portrait
        };

        var generator = new PdfGenerator();
        var document = await generator.GeneratePdf(html, pdfConfig);

        using var stream = File.Create("output.pdf");
        document.Save(stream);
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

| PeachPDF | IronPDF | Notes |
|---|---|---|
| `PeachPDF` | `IronPdf` | Core namespace |
| `PeachPDF.PdfSharpCore` | `IronPdf.Rendering` | Rendering enums (page size, orientation) |
| `PeachPDF.Network` | n/a (built into `ChromePdfRenderer`) | URL fetching |

### Core Class Mapping

| PeachPDF Concept | IronPDF Class | Description |
|---|---|---|
| `PdfGenerator` (engine) | `ChromePdfRenderer` | Renders HTML/URL to PDF |
| `PdfGenerateConfig` (settings) | `ChromePdfRenderOptions` (`renderer.RenderingOptions`) | Page size, orientation, margins, JS wait, headers |
| Output of `GeneratePdf(...)` | `PdfDocument` | PDF object; save, merge, manipulate |
| n/a | `PdfDocument.Merge()` | Static multi-document merge |

### Document Loading Methods

| Operation | PeachPDF | IronPDF |
|---|---|---|
| HTML string | `await generator.GeneratePdf(html, config)` | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | Wire `HttpClientNetworkAdapter` via `config.NetworkAdapter`, then call `GeneratePdf(null, config)` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML file | Read file, pass string to `GeneratePdf` | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Load existing PDF | Not supported | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | PeachPDF | IronPDF |
|---|---|---|
| Page count | Not exposed (HTML-to-PDF only) | `pdf.PageCount` |
| Remove page | Not supported | `pdf.RemovePages(index)` |
| Extract range | Not supported | `pdf.CopyPages(startIndex, endIndex)` |
| Rotate | Not supported | `pdf.RotateAllPages(PageRotation.Clockwise90)` |

### Merge / Split Operations

| Operation | PeachPDF | IronPDF |
|---|---|---|
| Merge | Not supported (drop to PdfSharpCore) | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not supported | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before (PeachPDF):**
```csharp
using PeachPDF;
using PeachPDF.PdfSharpCore;
using System.IO;
using System.Threading.Tasks;

class HtmlToPdfBefore
{
    static async Task Main()
    {
        var html = @"
            <html>
            <head>
            <style>
                body { font-family: Georgia, serif; color: #333; padding: 40px; }
                .title { font-size: 28px; border-bottom: 1px solid #999; }
                .meta { color: #666; font-size: 12px; }
            </style>
            </head>
            <body>
                <div class='title'>Q3 2024 Financial Summary</div>
                <div class='meta'>Generated: 2024-10-01</div>
            </body>
            </html>";

        var pdfConfig = new PdfGenerateConfig
        {
            PageSize = PageSize.Letter,
            PageOrientation = PageOrientation.Portrait
        };

        var generator = new PdfGenerator();
        var document = await generator.GeneratePdf(html, pdfConfig);

        using var stream = File.Create("summary.pdf");
        document.Save(stream);
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
        body { font-family: Georgia, serif; color: #333; padding: 40px; }
        .title { font-size: 28px; border-bottom: 1px solid #999; }
        .meta { color: #666; font-size: 12px; }
    </style>
    </head>
    <body>
        <div class='title'>Q3 2024 Financial Summary</div>
        <div class='meta'>Generated: 2024-10-01</div>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.Letter;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("summary.pdf");

Console.WriteLine($"Saved summary.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (PeachPDF — not supported natively):**
```csharp
using System;

class MergeBefore
{
    static void Main()
    {
        // PeachPDF is HTML-to-PDF only — it does not load, parse, or merge
        // existing PDFs. Teams typically drop down to PdfSharpCore directly,
        // or pair PeachPDF with a secondary library for merging. Migration to
        // IronPDF is an opportunity to consolidate onto one library.
        Console.WriteLine("PeachPDF has no merge API — secondary library required");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var pdf1 = PdfDocument.FromFile("part1.pdf");
var pdf2 = PdfDocument.FromFile("part2.pdf");
var pdf3 = PdfDocument.FromFile("part3.pdf");

// Merge: https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("combined.pdf");

Console.WriteLine($"Merged: {merged.PageCount} pages");
pdf1.Dispose(); pdf2.Dispose(); pdf3.Dispose();
```

---

### 3. Watermark

**Before (PeachPDF — CSS-only approach):**
```csharp
using PeachPDF;
using PeachPDF.PdfSharpCore;
using System.IO;
using System.Threading.Tasks;

class WatermarkBefore
{
    static async Task Main()
    {
        // PeachPDF has no watermark API. The standard approach is a CSS
        // pseudo-element on the source HTML before conversion.
        var html = @"
            <html>
            <head>
            <style>
                .watermark {
                    position: fixed;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%) rotate(-45deg);
                    font-size: 80px;
                    color: rgba(150, 150, 150, 0.2);
                    z-index: 9999;
                    white-space: nowrap;
                    pointer-events: none;
                }
            </style>
            </head>
            <body>
                <div class='watermark'>INTERNAL USE ONLY</div>
                <h1>Document</h1>
            </body>
            </html>";

        var generator = new PdfGenerator();
        var document = await generator.GeneratePdf(html, new PdfGenerateConfig { PageSize = PageSize.Letter });

        using var stream = File.Create("internal-document.pdf");
        document.Save(stream);
        // Fidelity depends on PeachPDF's CSS subset — transforms and rgba
        // opacity are not guaranteed in a non-browser layout engine.
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
    "<html><body><h1>Document</h1><p>Content...</p></body></html>"
);

// Post-render programmatic watermark — consistent regardless of source HTML
// https://ironpdf.com/how-to/custom-watermark/
pdf.ApplyWatermark(
    "<div style='color: rgba(150,150,150,0.4); font-size: 80px; transform: rotate(-45deg);'>INTERNAL USE ONLY</div>"
);

pdf.SaveAs("internal-document.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (PeachPDF — not supported):**
```csharp
using System;

class PasswordBefore
{
    static void Main()
    {
        // PeachPDF has no password/encryption API. Teams typically
        // generate the PDF with PeachPDF, then post-process the bytes
        // with a secondary library to add user/owner passwords.
        //
        //   var unprotectedBytes = /* PeachPDF output */;
        //   var protectedBytes = SecondaryPdfLib.AddPassword(
        //       unprotectedBytes, userPwd: "open123", ownerPwd: "admin456");
        //
        // Migration to IronPDF removes the need for the secondary library.
        Console.WriteLine("PeachPDF has no security API — secondary library required");
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
    "<html><body><h1>Restricted Document</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;

pdf.SaveAs("restricted.pdf");
Console.WriteLine("Saved restricted.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Troubleshooting Common Migration Issues

### Build Fails After Package Swap

**Symptom:** After removing PeachPDF and adding IronPdf, the project builds but throws a runtime exception on the first render call.

**Common cause:** License key not set.

**Resolution:**
```csharp
using IronPdf;

// Check that the license key is set before any IronPDF API call
// A missing or invalid key will throw at runtime, not compile time
if (string.IsNullOrEmpty(IronPdf.License.LicenseKey))
{
    IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
        ?? throw new InvalidOperationException("Set IRONPDF_LICENSE_KEY environment variable");
}

// Validate: https://ironpdf.com/how-to/license-keys/
Console.WriteLine($"License valid: {IronPdf.License.IsValidLicense}");
```

### CSS Renders Differently After Migration

**Symptom:** HTML that rendered acceptably in PeachPDF looks different in IronPDF.

**Root cause:** Different rendering engines. IronPDF uses Chromium; PeachPDF uses a layout engine on top of PdfSharpCore with an HTML+CSS subset. The new rendering may actually be *more* correct for modern CSS, but it can look different — especially for Grid, Flexbox, transforms, and `rgba()` opacity.

**Debugging approach:**
```csharp
// Step 1: Test your HTML directly in Chrome DevTools print preview
// IronPDF's Chromium renders CSS as Chrome does with @media print rules

// Step 2: Add explicit @media print styles for predictable output
var html = @"
    <html>
    <head>
    <style>
        /* Screen styles */
        body { font-family: Arial; }

        /* Print-specific — IronPDF applies these via Chromium */
        @media print {
            .no-print { display: none; }
            .page-break { page-break-before: always; }
            table { page-break-inside: avoid; }
        }
    </style>
    </head>
    <body>...</body>
    </html>";

// Step 3: Adjust rendering options to match desired output
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
// See: https://ironpdf.com/how-to/rendering-options/
// See: https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/
```

### External Resources Not Loading

**Symptom:** Images, CSS files, or fonts referenced in the HTML don't appear in the PDF.

**Root cause:** Relative paths in HTML may not resolve correctly when rendering from a string. PeachPDF wired this through `HttpClientNetworkAdapter` with an explicit base URI; IronPDF uses `BaseUrlPath` on `RenderingOptions`.

**Resolution:**
```csharp
using IronPdf;
using System;
using System.IO;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();

// Option 1: Set base URL so relative paths resolve correctly
renderer.RenderingOptions.BaseUrlPath = new DirectoryInfo(
    Path.GetFullPath("./templates")
).FullName;

// Option 2: Use absolute URLs in the HTML
var html = @"
    <html>
    <head>
    <link rel='stylesheet' href='https://cdn.example.com/styles.css'>
    </head>
    <body>
        <img src='https://cdn.example.com/logo.png' alt='logo'>
    </body>
    </html>";

// Option 3: Embed resources as base64
// var logoBase64 = Convert.ToBase64String(File.ReadAllBytes("logo.png"));
// var html = $"<img src='data:image/png;base64,{logoBase64}'>";

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("output.pdf");
```

### Azure / Cloud Deployment Failures

**Symptom:** Works locally but throws exceptions in Azure App Service, Azure Functions, or similar cloud environments.

**Resolution:**
```csharp
// IronPDF has specific guidance for Azure deployments
// https://ironpdf.com/how-to/azure/

// Common requirement: set Azure App Service to 64-bit
// Common requirement: ensure the hosting plan has enough memory
// for Chromium rendering (Basic tier or above is typically needed)

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("output.pdf");

// See full Azure guidance: https://ironpdf.com/how-to/azure/
```

### High Memory Usage Under Load

**Symptom:** Memory climbs under sustained rendering load; GC doesn't fully reclaim.

**Resolution:**
```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Always use 'using' on PdfDocument — IronPDF wraps native Chromium resources
static async Task ProcessBatch(IEnumerable<string> htmlDocuments)
{
    var renderer = new ChromePdfRenderer();

    foreach (var html in htmlDocuments)
    {
        // 'using' ensures timely disposal of Chromium resources
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        pdf.SaveAs($"output_{Guid.NewGuid():N}.pdf");
        // pdf disposed here — native resources released
    }
}

// For high-volume: consider processing in batches
// and explicitly calling GC.Collect() between batches if needed
```

### Renderer Reuse vs Per-Request Instantiation

**Symptom:** Slow response times under concurrent load when instantiating `ChromePdfRenderer` per request.

**Resolution:**
```csharp
using IronPdf;
using System.Threading.Tasks;

// Option 1: Reuse a single renderer across requests
public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");
        _renderer = new ChromePdfRenderer();
        // Configure once
        _renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
    }

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        using var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}

// Option 2: Per-request renderer (thread-isolated but slower to initialize)
// https://ironpdf.com/how-to/async/
```

---

## Critical Migration Notes

### .NET Version Verification

The trigger for this migration is often a .NET version constraint — PeachPDF targets `net8.0` only, so libraries that need to support older TFMs cannot pull it in. After migrating, confirm the project file targets the framework you need:

```xml
<!-- IronPDF supports a broad set of modern .NET targets -->
<!-- See: https://www.nuget.org/packages/IronPdf -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="IronPdf" Version="*" />
  </ItemGroup>
</Project>
```

### Async Migration Pattern

PeachPDF's primary API is already async-only — `await generator.GeneratePdf(...)` — so most callers will already have `async`/`await` in place. If any callers were forcing `.GetAwaiter().GetResult()` to bridge into a sync code path, this is the place to fix the anti-pattern:

```csharp
// If old code was:
// [HttpGet("pdf")]
// public IActionResult GetPdf() {
//     var document = peachGenerator.GeneratePdf(html, config).GetAwaiter().GetResult(); // sync-over-async
//     using var ms = new MemoryStream();
//     document.Save(ms);
//     return File(ms.ToArray(), "application/pdf");
// }

// Migrate to:
[HttpGet("pdf")]
public async Task<IActionResult> GetPdfAsync()
{
    var renderer = new ChromePdfRenderer(); // or inject via DI
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return File(pdf.BinaryData, "application/pdf", "document.pdf");
}
```

### Page Indexing

IronPDF uses 0-based page indexing. If you have page manipulation code in a secondary library you added to fill PeachPDF gaps, audit page index references when porting that logic over to IronPDF.

### Secondary Library Cleanup

A common discovery during this migration: a secondary library (PdfSharpCore, iTextSharp, or similar) was added for watermarking, security, or merge because PeachPDF doesn't support those features. After migrating to IronPDF, those secondary packages can typically be removed. Audit your NuGet dependency tree after migration.

---

## Performance Considerations

### Warm-Up for Long-Running Services

```csharp
using IronPdf;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

// Warm up during application startup — avoids cold-start latency on first request
public class PdfWarmupService : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

        // Trigger Chromium initialization at startup
        var renderer = new ChromePdfRenderer();
        using var _ = await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");

        // Renderer is now warm for production traffic
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

### Parallel Rendering

```csharp
using IronPdf;
using System;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/examples/parallel/
var htmlBatch = Enumerable.Range(1, 5)
    .Select(i => $"<html><body><h1>Document {i}</h1></body></html>")
    .ToArray();

var tasks = htmlBatch.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
});

var pdfs = await Task.WhenAll(tasks);
Console.WriteLine($"Rendered {pdfs.Length} documents");

foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Memory Stream for API Responses

```csharp
using IronPdf;
using System.IO;

// Return PDF bytes from an API endpoint without writing to disk
var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// https://ironpdf.com/how-to/pdf-memory-stream/
await using var ms = new MemoryStream();
pdf.Stream.CopyTo(ms);
return ms.ToArray(); // bytes safe to return after pdf is disposed
```

---

## Migration Checklist

### Pre-Migration
- [ ] Confirm PeachPDF is the .NET version or feature blocker (`dotnet build` errors, missing features)
- [ ] Identify all PeachPDF API calls (`rg "PdfGenerator|PdfGenerateConfig|GeneratePdf" --type cs`)
- [ ] Identify secondary libraries added to fill PeachPDF gaps (merge, security, watermark)
- [ ] Document rendering options in use (page size, orientation, network adapter base URI)
- [ ] Catalog HTML templates used for PDF generation
- [ ] Confirm IronPDF supports your target .NET version
- [ ] Obtain IronPDF license key
- [ ] Check Azure/cloud deployment environment requirements

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove PeachPDF package
- [ ] Add license key at application startup
- [ ] Replace `PdfGenerator` + `PdfGenerateConfig` with `ChromePdfRenderer` + `RenderingOptions`
- [ ] Map `PageSize`/`PageOrientation` to `PaperSize`/`PaperOrientation`
- [ ] Replace `document.Save(Stream)` with `pdf.SaveAs(path)` or `pdf.BinaryData`/`pdf.Stream`
- [ ] Move CSS-positioned headers/footers into `RenderingOptions.HtmlHeader`/`HtmlFooter` where appropriate
- [ ] Migrate any CSS watermark approach to `pdf.ApplyWatermark()`
- [ ] Add security settings via `pdf.SecuritySettings` (PeachPDF had none)
- [ ] Replace any secondary merge library with `PdfDocument.Merge()`
- [ ] Fix any absolute/relative URL issues with `BaseUrlPath`

### Testing
- [ ] Render each HTML template and compare output
- [ ] Verify page sizes and orientation match expectations
- [ ] Test CSS layouts — especially Grid/Flexbox that PeachPDF couldn't fully render
- [ ] Confirm external resources (images, fonts, CSS) load correctly
- [ ] Test in target deployment environment (Azure, Docker, IIS)
- [ ] Test parallel rendering at expected concurrency
- [ ] Confirm error handling catches IronPDF exception types
- [ ] Test password-protected PDF creation and opening

### Post-Migration
- [ ] Remove secondary library packages no longer needed (PdfSharpCore, etc.)
- [ ] Update environment variable documentation — add `IRONPDF_LICENSE_KEY`
- [ ] Confirm `dotnet build` succeeds on all target TFMs
- [ ] Update CI/CD pipeline if base image or dependencies changed

---

## One Last Thing

PeachPDF migrations are usually triggered by one of two things: a feature ceiling (you need merge, signatures, password protection, or a real CSS engine) or a TFM constraint (PeachPDF is `net8.0` only and you need broader targets). Either way the main work after the swap is verifying that the rendering output is equivalent — which is the purpose of the testing checklist above.

One practical note: if your migration also includes moving from .NET Framework to a modern TFM, test IronPDF on Linux as early as possible — it's faster to catch platform issues in a local Docker container than in a CI pipeline.

**Discussion question:** After completing the migration, what were your before/after figures — either bundle size, average render time, or memory footprint under sustained load? Particularly interested in cases where the new Chromium-based renderer behaved unexpectedly compared to PeachPDF's PdfSharpCore-backed layout engine.
