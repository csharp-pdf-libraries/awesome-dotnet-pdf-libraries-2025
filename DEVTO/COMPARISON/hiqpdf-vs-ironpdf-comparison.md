---
title: "HiQPdf vs IronPDF: feature by feature for .NET in 2026"
published: false
tags: dotnet, csharp, pdf, comparison
---

A common question when evaluating HTML-to-PDF libraries for .NET: does the legacy WebKit-based engine still fit a modern stack, or is it time to move to a Chromium-rendered output? HiQPdf has been a recognizable name in this space for years, and the classic `HiQPdf` NuGet package targets Windows x64 with a WebKit engine. Vendors in this category have moved toward Chromium over time — verify which package family your project actually depends on, since capabilities differ.

For .NET teams evaluating HTML-to-PDF libraries in 2026, HiQPdf is a mature commercial product where the choice of NuGet package determines a lot: platform support, rendering engine, and modern web standards coverage. The question often comes down to whether managing that variant matrix is worth it compared to a single-package alternative.

## Understanding IronPDF

IronPDF provides a single, unified library across all supported platforms: Windows, Linux, macOS, Docker, Azure, AWS. One NuGet package (`IronPdf`) installs everywhere, with native Chromium binaries automatically selected for your target platform — no engine-generation decision, no platform-specific packages, no runtime package coordination.

The library targets .NET Framework 4.6.2+, .NET Core, .NET Standard 2.0, and current .NET. The Chromium rendering engine matches Chrome's "Print to PDF" output. See the [HTML to PDF examples](https://ironpdf.com/examples/using-html-to-create-a-pdf/) for implementation patterns.

## Key Characteristics of HiQPdf

### Product Status
HiQPdf remains actively developed as a commercial product. The lineup spans multiple NuGet packages including the classic `HiQPdf` (WebKit-based, Windows x64), `HiQPdf.Free` (capped at 3 pages per document), `HiQPdf_NetCore` (.NET Core target), and Chromium-based variants such as `HiQPdf.NG` and `HiQPdf.Chromium.Windows`. Each has different capabilities, platform support, and deployment requirements — verify the exact package set on nuget.org for your target.

### Capability Notes (Varies by Package)
- **Classic `HiQPdf`:** Windows x64 only on the WebKit engine; modern CSS and JavaScript features can be limited compared with Chromium-based engines
- **Chromium variants:** May require separate platform-specific runtime packages; coordinate versions across packages
- **Async surface:** Synchronous conversion API on the Classic engine; verify async availability in newer variants for your version

### Technical Considerations
- **Package selection:** Choose among Classic, Free, .NET Core, and Chromium variants based on platform and requirements
- **Engine choice:** WebKit (Classic) and Chromium variants have different performance and CSS/JS support characteristics
- **Licensing:** Commercial licensing with a perpetual model; verify terms for multi-server deployments at the vendor site
- **Cross-package feature parity:** Features may differ between WebKit and Chromium variants — verify against your version

### Support Model
Commercial product with email-based support.

### Deployment Notes
- **Package matrix:** Different packages for .NET Framework vs .NET Core, Windows vs cross-platform, WebKit vs Chromium
- **Migration between engines:** Moving from a WebKit variant to a Chromium variant may require code changes; it is not always a drop-in replacement

## Feature Comparison Overview

| Feature | HiQPdf | IronPDF |
|---------|--------|---------|
| **Current Status** | Active (commercial) | Active (commercial) |
| **HTML Engine** | WebKit (Classic) or Chromium (newer variants) | Chromium (all platforms) |
| **Rendering Output** | Engine-dependent | Chrome-equivalent output |
| **Installation** | Multiple packages per platform/engine | Single NuGet package |
| **Support** | Email (commercial) | 24/5 engineer chat |
| **Package Model** | Variant-specific | Unified library |

---

## Code Comparison: HTML String to PDF

### HiQPdf — HTML String to PDF (Classic Engine)

```csharp
// NuGet: Install-Package HiQPdf
using HiQPdf;
using System;

class Program
{
    static void Main()
    {
        HtmlToPdf htmlToPdfConverter = new HtmlToPdf();

        // Configure page settings
        htmlToPdfConverter.Document.PageSize = PdfPageSize.A4;
        htmlToPdfConverter.Document.PageOrientation = PdfPageOrientation.Portrait;
        htmlToPdfConverter.Document.Margins = new PdfMargins(10, 10, 10, 10);

        // Set rendering viewport
        htmlToPdfConverter.BrowserWidth = 1024;
        htmlToPdfConverter.BrowserHeight = 768;

        string htmlContent = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; }
                    h1 { color: navy; }
                </style>
            </head>
            <body>
                <h1>Invoice #12345</h1>
                <table border='1'>
                    <tr><td>Item</td><td>Quantity</td><td>Price</td></tr>
                    <tr><td>Widget A</td><td>5</td><td>$25.00</td></tr>
                    <tr><td>Widget B</td><td>3</td><td>$15.00</td></tr>
                </table>
                <p><b>Total: $200.00</b></p>
            </body>
            </html>";

        // Convert HTML to PDF as a byte buffer
        byte[] pdfBytes = htmlToPdfConverter.ConvertHtmlToMemory(htmlContent, null);

        System.IO.File.WriteAllBytes("output-classic.pdf", pdfBytes);
    }
}
```

Notes on the Classic engine:

1. **WebKit baseline:** The Classic `HiQPdf` package targets a WebKit-based engine; modern CSS3 and ES2015+ JavaScript coverage depends on engine version
2. **Platform:** Classic ships for Windows x64; cross-platform requires moving to a different HiQPdf variant
3. **Synchronous API:** `ConvertHtmlToMemory` is a blocking call
4. **CSS and JS coverage:** Verify support for features your templates depend on (Grid, Flex, ES2015+) against your installed version

### IronPDF — HTML String to PDF

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Instantiate renderer (Chromium engine)
        var renderer = new ChromePdfRenderer();

        // Optional rendering tweaks
        renderer.RenderingOptions.Timeout = 60; // seconds
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.RenderDelay = 0; // milliseconds

        string htmlContent = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; }
                    h1 { color: navy; }
                </style>
            </head>
            <body>
                <h1>Invoice #12345</h1>
                <table border='1'>
                    <tr><td>Item</td><td>Quantity</td><td>Price</td></tr>
                    <tr><td>Widget A</td><td>5</td><td>$25.00</td></tr>
                    <tr><td>Widget B</td><td>3</td><td>$15.00</td></tr>
                </table>
                <p><b>Total: $200.00</b></p>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output-ironpdf.pdf");
    }
}
```

IronPDF's Chromium engine renders modern HTML/CSS without picking a separate package per engine generation. Batch processing benefits from reusing a single `ChromePdfRenderer` across conversions. See the [API reference](https://ironpdf.com/object-reference/api/) for tuning options.

---

## Code Comparison: Batch Processing

### HiQPdf — Batch Rendering

```csharp
// NuGet: Install-Package HiQPdf
using HiQPdf;
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        // Create converter instance once for batch
        var converter = new HtmlToPdf();

        // Prepare batch of HTML documents
        var htmlBatch = Enumerable.Range(1, 100).Select(i =>
            $"<html><body><h1>Invoice #{i}</h1><p>Total: ${i * 10}.00</p></body></html>"
        ).ToList();

        var results = new List<byte[]>();

        // Serial processing
        foreach (var html in htmlBatch)
        {
            var pdfBytes = converter.ConvertHtmlToMemory(html, "");
            results.Add(pdfBytes);
        }
    }
}
```

Batch considerations on the Classic engine:

1. **Converter instance reuse:** Create once, reuse for the batch to amortize initialization cost
2. **Parallelism:** Verify parallel batch support against your version's documentation before relying on it
3. **Memory:** Watch for accumulation across large batches; dispose results as you write them out
4. **Async surface:** The Classic conversion call is synchronous

### IronPDF — Batch Rendering with Parallelization

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Prepare batch of HTML documents
        var htmlBatch = Enumerable.Range(1, 100).Select(i =>
            $"<html><body><h1>Invoice #{i}</h1><p>Total: ${i * 10}.00</p></body></html>"
        ).ToList();

        // Parallel processing
        var pdfResults = new ConcurrentBag<PdfDocument>();

        Parallel.ForEach(htmlBatch, new ParallelOptions { MaxDegreeOfParallelism = 4 }, html =>
        {
            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf(html);
            pdfResults.Add(pdf);
        });

        // Async pattern for web applications
        var asyncTasks = htmlBatch.Select(async html =>
        {
            var renderer = new ChromePdfRenderer();
            return await renderer.RenderHtmlAsPdfAsync(html);
        });

        var asyncResults = await Task.WhenAll(asyncTasks);
    }
}
```

IronPDF supports `Parallel.ForEach` for batch processing and provides native async methods (`RenderHtmlAsPdfAsync`, `RenderUrlAsPdfAsync`) for web applications. For high-volume scenarios, see the optimization guides in the [documentation](https://ironpdf.com/docs/).

---

## Performance Characteristics (Architectural)

| Scenario | HiQPdf Classic (WebKit) | HiQPdf Chromium Variants | IronPDF | Notes |
|----------|------------------------|--------------------------|---------|-------|
| Simple HTML | Lightweight engine; lower baseline cost | Chromium baseline cost | Chromium baseline cost | Measure with your workload |
| Complex HTML / modern CSS | CSS3/ES2015+ coverage may be limited | Modern coverage via Chromium | Modern coverage via Chromium | Engine generation matters |
| Cold start | Lower (WebKit) | Higher (Chromium init) | Chromium init | First conversion incurs warmup |
| Batch serial | Reuse converter to amortize init | Reuse converter to amortize init | Reuse `ChromePdfRenderer` | Verify per version |
| Batch parallel | Verify per version | Verify per version | `Parallel.ForEach` supported | See IronPDF docs |
| Memory per conversion | Typically lower (WebKit class) | Chromium memory profile | Chromium memory profile | Varies with content |

*Architectural notes only. Always measure with your actual workload, content, and hardware before drawing comparisons.*

---

## API Mapping Reference

| HiQPdf (Classic) | IronPDF Equivalent |
|------------------|-------------------|
| `HtmlToPdf` | `ChromePdfRenderer` |
| `ConvertHtmlToMemory(html, baseUrl)` | `RenderHtmlAsPdf(html, baseUrl)` |
| `ConvertUrlToMemory(url)` | `RenderUrlAsPdf(url)` |
| `Document.PageSize` | `RenderingOptions.PaperSize` |
| `Document.PageOrientation` | `RenderingOptions.PaperOrientation` |
| `Document.Margins` | `RenderingOptions.MarginTop/Bottom/Left/Right` |
| `BrowserWidth` | `RenderingOptions.ViewPortWidth` |
| `HtmlToPdfVariableElement` (header/footer) | `HtmlHeaderFooter.HtmlFragment` |
| `{CrtPage}` / `{PageCount}` placeholders | `{page}` / `{total-pages}` |
| `SerialNumber` (license) | `IronPdf.License.LicenseKey` (set globally) |
| N/A | `RenderHtmlAsPdfAsync` (native async) |

---

## Comprehensive Feature Comparison

| Category | Feature | HiQPdf Classic (WebKit) | HiQPdf Chromium Variants | IronPDF |
|----------|---------|-------------------------|--------------------------|---------|
| **Status** | Active Development | Yes | Yes | Yes |
| **Status** | Rendering Engine | WebKit | Chromium | Chromium |
| **Status** | Platform Support | Windows x64 | Cross-platform per variant | Cross-platform |
| **Support** | Support Model | Email (commercial) | Email (commercial) | 24/5 chat + email |
| **Content Creation** | Modern JavaScript (ES2015+) | Limited — verify version | Supported | Supported |
| **Content Creation** | CSS Grid | Limited — verify version | Supported | Supported |
| **Content Creation** | CSS Flexbox | Partial — verify version | Supported | Supported |
| **Content Creation** | Web Fonts | Limited — verify version | Supported | Supported |
| **PDF Operations** | Merge PDFs | Yes | Yes | Yes |
| **PDF Operations** | Split / Extract Pages | Yes | Yes | Yes |
| **PDF Operations** | Extract Text | Yes | Yes | Yes |
| **PDF Operations** | Edit Existing PDFs | Yes | Yes | Yes |
| **PDF Operations** | Forms Support | Yes | Yes | Yes (fill/flatten/create) |
| **PDF Operations** | Watermarks | Yes | Yes | Yes |
| **PDF Operations** | Digital Signatures | Yes | Yes | Yes (X.509) |
| **Security** | Encryption | Yes | Yes | Yes (AES-256) |
| **Security** | Permissions Control | Yes | Yes | Yes |
| **Development** | Native Async Methods | No (synchronous API) | Verify per version | Yes |
| **Development** | Parallel Batch Support | Verify per version | Verify per version | Documented (`Parallel.ForEach`) |
| **Development** | Single NuGet Package | Variant-specific | Variant-specific | Yes |
| **Development** | .NET Framework | Supported | Supported | 4.6.2+ |
| **Development** | .NET Core / .NET 5+ | Via `HiQPdf_NetCore` | Supported | Yes |
| **Development** | Docker Support | Windows containers (Classic) | Per variant | Yes (all platforms) |

---

## Installation Comparison

### HiQPdf Installation (Classic Engine)

```bash
# Install the classic HiQPdf package (Windows x64, WebKit engine)
dotnet add package HiQPdf

# For .NET Core targets, use the .NET Core build:
# dotnet add package HiQPdf_NetCore

# For Chromium-based variants, verify the current package name on nuget.org
# (e.g., HiQPdf.NG, HiQPdf.Chromium.Windows) and any platform-specific
# runtime packages required.
```

**Namespace imports:**
```csharp
using HiQPdf;
```

**Additional setup:**
- Commercial license key required
- Platform-specific package required for your target (Windows / .NET Core / Chromium variant)
- Deployment must ship the correct package for the target OS and runtime

### IronPDF Installation

```bash
# Single package for all platforms
dotnet add package IronPdf
```

**Namespace imports:**
```csharp
using IronPdf;
```

**Additional setup:**
- License key for production
- Native libraries auto-select based on platform
- No additional runtime packages required

---

## Conclusion

HiQPdf spans multiple engine and package options: the classic WebKit-based `HiQPdf` (Windows x64), a `.NET Core` build, and newer Chromium-based variants. Each targets different scenarios and requires the right package combination for the target platform and engine generation.

Migration to IronPDF tends to be attractive when you want unified cross-platform support without coordinating multiple packages, native async/await for web applications, documented parallel processing for batch scenarios, or simpler deployment from a single NuGet package.

IronPDF provides Chromium rendering across platforms from a single NuGet package, with native async methods, documented `Parallel.ForEach` support, 24/5 engineer chat support, and a broad PDF manipulation API. It runs wherever .NET runs — Windows, Linux, macOS, Docker, serverless — with automatic platform detection.

**What's your experience with multi-package library coordination? Have you hit version-mismatch issues between core and runtime packages?**

For optimization techniques and advanced scenarios, explore the [HTML to PDF examples](https://ironpdf.com/examples/using-html-to-create-a-pdf/) and [API documentation](https://ironpdf.com/object-reference/api/).
