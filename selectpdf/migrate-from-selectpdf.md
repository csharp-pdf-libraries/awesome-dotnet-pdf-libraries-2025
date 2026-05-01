# How Do I Migrate from SelectPdf to IronPDF in C#?

## Table of Contents
1. [Why Migrate from SelectPdf](#why-migrate-from-selectpdf)
2. [The Windows-Only Problem](#the-windows-only-problem)
3. [The Outdated Rendering Engine](#the-outdated-rendering-engine)
4. [The 5-Page Free Version Trap](#the-5-page-free-version-trap)
5. [Quick Start Migration (5 Minutes)](#quick-start-migration-5-minutes)
6. [Complete API Reference](#complete-api-reference)
7. [Code Migration Examples](#code-migration-examples)
8. [Feature Comparison](#feature-comparison)
9. [Licensing Comparison](#licensing-comparison)
10. [Platform Support Comparison](#platform-support-comparison)
11. [Troubleshooting Guide](#troubleshooting-guide)
12. [Migration Checklist](#migration-checklist)
13. [Additional Resources](#additional-resources)

---

## Why Migrate from SelectPdf

### Critical Limitations

| Issue | Impact | IronPDF Solution |
|-------|--------|------------------|
| **Windows-only** | Cannot deploy to Linux, Docker, Azure Functions | Full cross-platform support |
| **Default WebKit engine, optional older Chromium** | Modern CSS may fail unless Blink engine is enabled | Up-to-date Chromium |
| **5-page free version limit** | Watermark on every page until licensed | Generous trial |
| **Linux/macOS deployment blocked** | No cloud Linux, Docker, AWS Lambda | Cloud-native |
| **Cloud deployment blocked (non-Windows)** | Can't use Azure Functions, AWS Lambda | Cloud-native |

### The Core Problems

SelectPdf markets itself as a robust HTML-to-PDF solution, but has fundamental limitations:

1. **Windows-Only**: SelectPdf's own documentation states it "currently require[s] Windows; [does] not run on Linux, macOS or Xamarin"
2. **Default WebKit, optional older Chromium**: The default engine is an internal WebKit; the Blink option ships with Chromium 124 (released April 2024)
3. **Restrictive Free Version**: 5-page hard cap, plus a per-page watermark on every PDF generated without a license key
4. **No Linux Cloud Support**: Cannot deploy to Linux Azure Functions, AWS Lambda, or Linux Docker

### Feature Comparison Overview

| Feature | SelectPdf | IronPDF |
|---------|-----------|---------|
| **Windows** | ✅ | ✅ |
| **Linux** | ❌ NOT SUPPORTED | ✅ 10+ distros |
| **macOS** | ❌ NOT SUPPORTED | ✅ Full support |
| **Docker** | ❌ NOT SUPPORTED | ✅ Official images |
| **Azure Functions** | ❌ NOT SUPPORTED | ✅ Full support |
| **AWS Lambda** | ❌ NOT SUPPORTED | ✅ Full support |
| **CSS Grid** | ⚠️ Limited | ✅ Full support |
| **Flexbox** | ⚠️ Limited | ✅ Full support |
| **CSS Variables** | ⚠️ WebKit engine: limited | ✅ Full support |
| **.NET 10** | ✅ (Windows only, via .NET Standard 2.0) | ✅ Full support (all platforms) |
| **Free version limit** | 5 pages + per-page watermark | Generous trial |

These limitations and the full migration path are examined in the [complete SelectPdf to IronPDF guide](https://ironpdf.com/blog/migration-guides/migrate-from-selectpdf-to-ironpdf/).

---

## The Windows-Only Problem

### SelectPdf's Platform Limitation

Despite any marketing claims, SelectPdf explicitly **does not support**:

- Linux (any distribution)
- macOS
- Docker containers
- Azure Functions
- AWS Lambda
- Google Cloud Functions
- Any ARM-based systems

This is a fundamental architectural limitation—SelectPdf depends on Windows-specific libraries and cannot be ported.

### What This Means for Your Application

```csharp
// ❌ SelectPdf - This code FAILS on Linux/Docker
using SelectPdf;

// Deployment to Azure App Service (Linux) - FAILS
// Deployment to Docker container - FAILS
// Deployment to AWS Lambda - FAILS
// GitHub Actions on ubuntu-latest - FAILS

var converter = new HtmlToPdf();
var doc = converter.ConvertHtmlString("<h1>Hello</h1>");
// Throws e.g. DllNotFoundException: "Unable to load shared library 'kernel32.dll'"
// (see selectpdf/selectpdf-free-html-to-pdf-converter#2 on GitHub)
```

### IronPDF Cross-Platform Support

```csharp
// ✅ IronPDF - Works everywhere
using IronPdf;

// Azure App Service (Linux) - WORKS
// Docker container - WORKS
// AWS Lambda - WORKS
// GitHub Actions on ubuntu-latest - WORKS
// macOS development - WORKS

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("output.pdf");
```

### Supported Platforms

| Platform | SelectPdf | IronPDF |
|----------|-----------|---------|
| Windows Server 2019+ | ✅ | ✅ |
| Windows 10/11 | ✅ | ✅ |
| Ubuntu 20.04+ | ❌ | ✅ |
| Debian 10+ | ❌ | ✅ |
| CentOS 7+ | ❌ | ✅ |
| Alpine Linux | ❌ | ✅ |
| Amazon Linux 2 | ❌ | ✅ |
| macOS 10.15+ | ❌ | ✅ |
| Azure App Service (Linux) | ❌ | ✅ |
| Azure Functions | ❌ | ✅ |
| AWS Lambda | ❌ | ✅ |
| Docker (Linux) | ❌ | ✅ |
| Kubernetes | ❌ | ✅ |

---

## The Outdated Rendering Engine

### SelectPdf's Engine Options

SelectPdf ships three rendering engines (per [vendor docs](https://selectpdf.com/pdf-library/html/RenderingEngine.htm)): an internal **WebKit** (the default), **Blink** (introduced in v19.1, shipped with Chromium 124.0.6367.201 — released April 2024), and **Chromium/CEF**. The default WebKit engine in particular hasn't kept pace with modern web standards:

```html
<!-- This modern CSS FAILS or renders incorrectly in SelectPdf -->

<!-- CSS Grid - Broken -->
<div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px;">
    <div>Item 1</div>
    <div>Item 2</div>
    <div>Item 3</div>
</div>

<!-- Advanced Flexbox - Broken -->
<div style="display: flex; gap: 20px; flex-wrap: wrap;">
    <div style="flex: 1 1 300px;">Flex item</div>
</div>

<!-- CSS Variables - Not supported -->
<style>
:root { --primary-color: #007bff; }
h1 { color: var(--primary-color); }
</style>

<!-- Modern @media queries - Limited -->
<style>
@media (prefers-color-scheme: dark) { ... }
@media print { ... }
</style>
```

### IronPDF's Modern Chromium

```csharp
// ✅ IronPDF - Uses latest stable Chromium
var renderer = new ChromePdfRenderer();

var html = @"
<style>
    :root { --primary: #007bff; --gap: 20px; }
    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: var(--gap); }
    .flex { display: flex; gap: var(--gap); flex-wrap: wrap; }
</style>
<div class='grid'>
    <div style='background: var(--primary); color: white; padding: 1rem;'>Item 1</div>
    <div style='background: var(--primary); color: white; padding: 1rem;'>Item 2</div>
    <div style='background: var(--primary); color: white; padding: 1rem;'>Item 3</div>
</div>";

var pdf = renderer.RenderHtmlAsPdf(html);
// All modern CSS features render correctly!
```

### CSS Feature Support Comparison

| CSS Feature | SelectPdf (WebKit default) | SelectPdf (Blink, Chromium 124) | IronPDF |
|-------------|----------------------------|----------------------------------|---------|
| CSS Grid | ⚠️ Partial/broken | ✅ | ✅ Full |
| Flexbox (basic) | ✅ | ✅ | ✅ |
| Flexbox (gap property) | ❌ | ✅ | ✅ |
| CSS Variables | ❌ | ✅ | ✅ |
| CSS calc() | ⚠️ Limited | ✅ | ✅ |
| @media print | ⚠️ Limited | ✅ | ✅ |
| @font-face | ⚠️ Limited | ✅ | ✅ |
| Web Fonts | ⚠️ Limited | ✅ | ✅ |
| SVG | ⚠️ Basic | ✅ | ✅ Full |
| CSS Transforms | ⚠️ Limited | ✅ | ✅ |
| CSS Animations | ❌ | ⚠️ Partial | ✅ |

---

## The 5-Page Free Version Trap

### SelectPdf's Aggressive Limitations

SelectPdf's Community Edition has two restrictions that combine into a tight free-tier window (per [selectpdf.com/community-edition](https://selectpdf.com/community-edition/) and the [License doc](https://selectpdf.com/docs/License.htm)):

```csharp
// ❌ SelectPdf Community Edition / unlicensed trial
// - Hard cap of 5 pages per PDF (Community Edition)
// - Per-page watermark on EVERY page until a license key is applied
//   ("free trial is fully functional, but without a license key,
//   a watermark will be displayed in each page" — vendor docs)
// - Watermark cannot be removed without purchase

var converter = new HtmlToPdf();
var doc = converter.ConvertHtmlString(longHtmlContent);
// Without a license key:
//   - Every page is watermarked
//   - Generation aborts/truncates past page 5 in Community Edition
```

### Commercial Pricing Comparison

| Aspect | SelectPdf | IronPDF |
|--------|-----------|---------|
| **Starting Price** | $499 (Single Developer) | $749 (Lite) |
| **Free Trial Pages** | 5 pages max (Community Edition) | Generous trial |
| **Watermark Behavior** | On every page until licensed | Trial watermark |
| **License Type** | Perpetual w/ 1 yr maintenance | Perpetual available |
| **Price Transparency** | Published per-tier | Clear pricing |

---

## Quick Start Migration (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove SelectPdf (use Select.HtmlToPdf.NetCore on .NET Core/.NET 5+)
dotnet remove package Select.HtmlToPdf
# or: dotnet remove package Select.HtmlToPdf.NetCore

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Using Statements

```csharp
// Before
using SelectPdf;

// After
using IronPdf;
```

### Step 3: Add License Key (Optional for Trial)

```csharp
// Add at application startup (for licensed version)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 4: Update Code

**Before (SelectPdf):**
```csharp
using SelectPdf;

var converter = new HtmlToPdf();
converter.Options.PdfPageSize = PdfPageSize.A4;
converter.Options.MarginTop = 20;
converter.Options.MarginBottom = 20;

PdfDocument doc = converter.ConvertHtmlString("<h1>Hello World</h1>");
doc.Save("output.pdf");
doc.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;

var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// No Close() needed
```

---

## Complete API Reference

### Namespace Mapping

| SelectPdf Namespace | IronPDF Namespace | Notes |
|---------------------|-------------------|-------|
| `SelectPdf` | `IronPdf` | Main namespace |
| `SelectPdf.HtmlToPdf` | `IronPdf.ChromePdfRenderer` | Renderer |
| `SelectPdf.PdfDocument` | `IronPdf.PdfDocument` | Same name |
| N/A | `IronPdf.Rendering` | Enums |
| N/A | `IronPdf.Editing` | Stampers |

### Core Class Mapping

| SelectPdf | IronPDF | Notes |
|-----------|---------|-------|
| `HtmlToPdf` | `ChromePdfRenderer` | Main converter |
| `PdfDocument` | `PdfDocument` | Same name, different API |
| `PdfPageSize` | `PdfPaperSize` | Page size enum |
| `PdfPageOrientation` | `PdfPaperOrientation` | Orientation enum |
| `SelectPdf.HtmlToPdfOptions` | `ChromePdfRenderOptions` | Via RenderingOptions |

### Method Mapping

| SelectPdf Method | IronPDF Method | Notes |
|------------------|----------------|-------|
| `converter.ConvertUrl(url)` | `renderer.RenderUrlAsPdf(url)` | URL to PDF |
| `converter.ConvertHtmlString(html)` | `renderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| `converter.ConvertHtmlString(html, baseUrl)` | `renderer.RenderHtmlAsPdf(html, baseUrl)` | With base URL |
| `doc.Save(path)` | `pdf.SaveAs(path)` | Save file |
| `doc.Save(stream)` | `pdf.Stream` or `pdf.BinaryData` | Get bytes |
| `doc.Close()` | Not needed | IDisposable |
| `doc.Pages.Count` | `pdf.PageCount` | Page count |

### Options Mapping

| SelectPdf Option | IronPDF Option | Notes |
|------------------|----------------|-------|
| `Options.PdfPageSize` | `RenderingOptions.PaperSize` | Page size |
| `Options.PdfPageOrientation` | `RenderingOptions.PaperOrientation` | Orientation |
| `Options.MarginTop` | `RenderingOptions.MarginTop` | Top margin (mm) |
| `Options.MarginBottom` | `RenderingOptions.MarginBottom` | Bottom margin (mm) |
| `Options.MarginLeft` | `RenderingOptions.MarginLeft` | Left margin (mm) |
| `Options.MarginRight` | `RenderingOptions.MarginRight` | Right margin (mm) |
| `Options.WebPageWidth` | `RenderingOptions.ViewPortWidth` | Viewport width |
| `Options.WebPageHeight` | `RenderingOptions.ViewPortHeight` | Viewport height |
| `Options.RenderingEngine` | N/A | Always Chromium |
| `Options.CssMediaType` | `RenderingOptions.CssMediaType` | Screen/Print |
| `Options.MinPageLoadTime` | `RenderingOptions.RenderDelay` | Wait time (ms) |
| `Options.MaxPageLoadTime` | `RenderingOptions.Timeout` | Timeout |
| `Options.JsRenderingDelay` | `RenderingOptions.WaitFor.JavaScript()` | JS wait |

### Header/Footer Mapping

| SelectPdf | IronPDF | Notes |
|-----------|---------|-------|
| `converter.Header.Add(html)` | `RenderingOptions.HtmlHeader` | Header |
| `converter.Footer.Add(html)` | `RenderingOptions.HtmlFooter` | Footer |
| `{page_number}` | `{page}` | Current page |
| `{total_pages}` | `{total-pages}` | Total pages |
| `{url}` | `{url}` | Same |
| `{date}` | `{date}` | Same |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (SelectPdf):**
```csharp
using SelectPdf;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdf();
        PdfDocument doc = converter.ConvertHtmlString("<h1>Hello World</h1>");
        doc.Save("output.pdf");
        doc.Close();
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
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 2: URL to PDF

**Before (SelectPdf):**
```csharp
using SelectPdf;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdf();
        converter.Options.PdfPageSize = PdfPageSize.A4;

        PdfDocument doc = converter.ConvertUrl("https://example.com");
        doc.Save("webpage.pdf");
        doc.Close();
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
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

### Example 3: Custom Page Size and Margins

**Before (SelectPdf):**
```csharp
using SelectPdf;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdf();

        converter.Options.PdfPageSize = PdfPageSize.Letter;
        converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;
        converter.Options.MarginTop = 25;
        converter.Options.MarginBottom = 25;
        converter.Options.MarginLeft = 20;
        converter.Options.MarginRight = 20;

        PdfDocument doc = converter.ConvertHtmlString("<h1>Custom Settings</h1>");
        doc.Save("custom.pdf");
        doc.Close();
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
        var renderer = new ChromePdfRenderer();

        renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.MarginTop = 25;
        renderer.RenderingOptions.MarginBottom = 25;
        renderer.RenderingOptions.MarginLeft = 20;
        renderer.RenderingOptions.MarginRight = 20;

        var pdf = renderer.RenderHtmlAsPdf("<h1>Custom Settings</h1>");
        pdf.SaveAs("custom.pdf");
    }
}
```

### Example 4: Headers and Footers

**Before (SelectPdf):**
```csharp
using SelectPdf;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdf();
        converter.Options.MarginTop = 50;
        converter.Options.MarginBottom = 50;

        // Header
        PdfHtmlSection header = new PdfHtmlSection("<div style='text-align:center; font-size:12px;'>Company Name</div>");
        header.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
        converter.Header.Add(header);

        // Footer with page numbers
        PdfHtmlSection footer = new PdfHtmlSection("<div style='text-align:center; font-size:10px;'>Page {page_number} of {total_pages}</div>");
        footer.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
        converter.Footer.Add(footer);

        PdfDocument doc = converter.ConvertHtmlString("<h1>Document Content</h1>");
        doc.Save("with-header-footer.pdf");
        doc.Close();
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
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.MarginTop = 50;
        renderer.RenderingOptions.MarginBottom = 50;

        // Header
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:center; font-size:12px;'>Company Name</div>"
        };

        // Footer with page numbers (note different placeholders)
        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:center; font-size:10px;'>Page {page} of {total-pages}</div>"
        };

        var pdf = renderer.RenderHtmlAsPdf("<h1>Document Content</h1>");
        pdf.SaveAs("with-header-footer.pdf");
    }
}
```

### Example 5: HTML File to PDF

**Before (SelectPdf):**
```csharp
using SelectPdf;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdf();

        // Read HTML file
        string html = File.ReadAllText("template.html");
        string baseUrl = Path.GetDirectoryName(Path.GetFullPath("template.html"));

        PdfDocument doc = converter.ConvertHtmlString(html, baseUrl);
        doc.Save("output.pdf");
        doc.Close();
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
        var renderer = new ChromePdfRenderer();

        // Direct file rendering with automatic base URL handling
        var pdf = renderer.RenderHtmlFileAsPdf("template.html");
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 6: JavaScript Execution

**Before (SelectPdf):**
```csharp
using SelectPdf;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdf();

        // Wait for JavaScript - SelectPdf has limited JS support
        converter.Options.MinPageLoadTime = 3;  // seconds
        converter.Options.MaxPageLoadTime = 30; // seconds

        var html = @"
            <div id='content'></div>
            <script>
                document.getElementById('content').innerHTML = 'Loaded via JS';
            </script>";

        PdfDocument doc = converter.ConvertHtmlString(html);
        doc.Save("js-content.pdf");
        doc.Close();
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
        var renderer = new ChromePdfRenderer();

        // Full JavaScript support
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.WaitFor.JavaScript(3000); // milliseconds

        var html = @"
            <div id='content'></div>
            <script>
                document.getElementById('content').innerHTML = 'Loaded via JS';
            </script>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("js-content.pdf");
    }
}
```

### Example 7: Save to Stream/Bytes

**Before (SelectPdf):**
```csharp
using SelectPdf;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdf();
        PdfDocument doc = converter.ConvertHtmlString("<h1>Stream Example</h1>");

        // Save to stream
        using (var stream = new MemoryStream())
        {
            doc.Save(stream);
            byte[] pdfBytes = stream.ToArray();
            // Use pdfBytes...
        }

        doc.Close();
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
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Stream Example</h1>");

        // Get bytes directly
        byte[] pdfBytes = pdf.BinaryData;

        // Or use stream
        var stream = pdf.Stream;
    }
}
```

### Example 8: Viewport Width (Web Page Width)

**Before (SelectPdf):**
```csharp
using SelectPdf;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdf();

        // Set web page width for responsive rendering
        converter.Options.WebPageWidth = 1280;
        converter.Options.WebPageHeight = 0; // Auto

        PdfDocument doc = converter.ConvertUrl("https://responsive-site.com");
        doc.Save("responsive.pdf");
        doc.Close();
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
        var renderer = new ChromePdfRenderer();

        // Set viewport width for responsive rendering
        renderer.RenderingOptions.ViewPortWidth = 1280;
        // ViewPortHeight auto by default

        var pdf = renderer.RenderUrlAsPdf("https://responsive-site.com");
        pdf.SaveAs("responsive.pdf");
    }
}
```

---

## Feature Comparison

### PDF Generation Features

| Feature | SelectPdf | IronPDF |
|---------|-----------|---------|
| HTML to PDF | ✅ | ✅ |
| URL to PDF | ✅ | ✅ |
| HTML File to PDF | ✅ (manual) | ✅ (direct method) |
| Headers/Footers | ✅ | ✅ |
| Page Numbers | ✅ | ✅ |
| Custom Margins | ✅ | ✅ |
| Custom Page Size | ✅ | ✅ |
| JavaScript Support | ⚠️ Limited | ✅ Full |
| CSS Grid | ⚠️ Limited | ✅ Full |
| Flexbox | ⚠️ Limited | ✅ Full |

### PDF Manipulation Features

| Feature | SelectPdf | IronPDF |
|---------|-----------|---------|
| Merge PDFs | ✅ | ✅ |
| Split PDFs | ⚠️ Limited | ✅ |
| Page Extraction | ⚠️ | ✅ |
| Add Watermarks | ⚠️ Manual | ✅ Built-in |
| Digital Signatures | ❌ | ✅ |
| PDF Security | ⚠️ Basic | ✅ Full |
| Form Filling | ❌ | ✅ |
| Text Extraction | ⚠️ Limited | ✅ Full |
| PDF/A Compliance | ❌ | ✅ |

---

## Licensing Comparison

### SelectPdf Pricing

(Verified at [selectpdf.com/pricing](https://selectpdf.com/pricing/) — perpetual licenses with 1 year of maintenance/upgrades included.)

| License | Price | Notes |
|---------|-------|-------|
| Community Edition | $0 | 5 pages max, watermark on every page |
| Single Developer | $499 | 1 developer, 1 deployment machine |
| Single Developer OEM | $799 | 1 developer, unlimited deployment machines |
| 5-Developers | $799 | up to 5 developers, up to 10 deployment machines |
| 5-Developers OEM | $1,099 | up to 5 developers, unlimited deployment machines |
| Enterprise | $1,199 | unlimited developers, up to 100 deployment machines |
| Enterprise OEM | $1,599 | unlimited developers, unlimited deployment machines |

A separate cloud-API plan (selectpdf.com REST API) is sold by monthly conversion volume ($19/mo entry through $449/mo dedicated).

**Cost considerations:**
- Annual renewal required to receive updates and support after year 1
- OEM licensing is a separate, more expensive SKU at every tier

### IronPDF Pricing

| License | Price | Notes |
|---------|-------|-------|
| Lite | $749 | 1 developer, 1 project |
| Professional | $1,499 | 10 developers, 10 projects |
| Unlimited | $2,999 | Unlimited |

**Benefits:**
- Perpetual license option
- Includes 1 year updates/support
- Clear, transparent pricing
- SaaS/OEM available

---

## Platform Support Comparison

### Development Environments

| Environment | SelectPdf | IronPDF |
|-------------|-----------|---------|
| Visual Studio (Windows) | ✅ | ✅ |
| Visual Studio (Mac) | ❌ | ✅ |
| VS Code (Windows) | ✅ | ✅ |
| VS Code (macOS) | ❌ | ✅ |
| VS Code (Linux) | ❌ | ✅ |
| JetBrains Rider (all) | Windows only | ✅ All |

### Deployment Targets

| Target | SelectPdf | IronPDF |
|--------|-----------|---------|
| Windows Server | ✅ | ✅ |
| IIS | ✅ | ✅ |
| Azure App Service (Windows) | ✅ | ✅ |
| Azure App Service (Linux) | ❌ | ✅ |
| Azure Functions | ❌ | ✅ |
| AWS EC2 (Windows) | ✅ | ✅ |
| AWS EC2 (Linux) | ❌ | ✅ |
| AWS Lambda | ❌ | ✅ |
| Docker (Windows) | ⚠️ Limited | ✅ |
| Docker (Linux) | ❌ | ✅ |
| Kubernetes | ❌ | ✅ |

### .NET Framework Support

| Framework | SelectPdf (Windows only) | IronPDF (all platforms) |
|-----------|--------------------------|-------------------------|
| .NET Framework 4.6.1+ | ✅ | ✅ |
| .NET Core 3.1 | ✅ | ✅ |
| .NET 5 | ✅ | ✅ |
| .NET 6 | ✅ | ✅ |
| .NET 7 | ✅ | ✅ |
| .NET 8 | ✅ | ✅ |
| .NET 9 | ✅ | ✅ |
| .NET 10 | ✅ (via .NET Standard 2.0) | ✅ |

---

## Troubleshooting Guide

### Issue 1: "SelectPdf only works on Windows" Error on Linux

**Symptom:** Exception when deploying to Linux/Docker

**Solution:** Migrate to IronPDF:
```csharp
// IronPDF works on Linux out of the box
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
```

For Docker, add dependencies:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y \
    libgdiplus libc6-dev libx11-dev libnss3 \
    libatk-bridge2.0-0 libdrm2 libxkbcommon0 \
    libxcomposite1 libxdamage1 libxfixes3 \
    libxrandr2 libgbm1 libasound2
```

### Issue 2: Page Number Placeholders Not Working

**Symptom:** Page numbers show as literal text

**Cause:** SelectPdf uses `{page_number}`, IronPDF uses `{page}`

**Solution:**
```csharp
// Update placeholder syntax
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "Page {page} of {total-pages}"  // Not {page_number}
};
```

### Issue 3: CSS Grid Layout Broken

**Symptom:** Grid layouts don't render correctly

**Cause:** SelectPdf's outdated rendering engine

**Solution:** IronPDF with modern Chromium:
```csharp
var renderer = new ChromePdfRenderer();
var html = @"
    <div style='display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px;'>
        <div>Item 1</div>
        <div>Item 2</div>
        <div>Item 3</div>
    </div>";
var pdf = renderer.RenderHtmlAsPdf(html);
// Renders correctly!
```

### Issue 4: Missing doc.Close() Equivalent

**Symptom:** Wondering if Close() is needed

**Solution:** IronPDF uses dispose pattern:
```csharp
// Option 1: Let garbage collection handle it
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// No Close() needed

// Option 2: Explicit disposal
using (var pdf = renderer.RenderHtmlAsPdf(html))
{
    pdf.SaveAs("output.pdf");
}
```

### Issue 5: Base URL Not Working

**Symptom:** Relative paths in HTML not resolving

**Solution:** Use explicit base URL or file rendering:
```csharp
// Option 1: Provide base URL
var pdf = renderer.RenderHtmlAsPdf(html, "file:///path/to/assets/");

// Option 2: Use file rendering (handles base URL automatically)
var pdf = renderer.RenderHtmlFileAsPdf("template.html");
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit all SelectPdf usages in codebase**
  ```bash
  grep -r "using SelectPdf" --include="*.cs" .
  grep -r "HtmlToPdf\|PdfDocument" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current converter options**
  ```csharp
  // Find patterns like:
  var options = new HtmlToPdfOptions {
      PdfPageSize = PdfPageSize.A4,
      PdfPageOrientation = PdfPageOrientation.Portrait
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Identify header/footer implementations**
  ```csharp
  // Before (SelectPdf)
  converter.Header.Add("<div>Header Content</div>");
  converter.Footer.Add("<div>Footer Content</div>");
  ```
  **Why:** IronPDF uses HtmlHeaderFooter for headers/footers with more flexible HTML support.

- [ ] **Check page number placeholder syntax**
  ```csharp
  // Before (SelectPdf)
  "{page_number} of {total_pages}"

  // After (IronPDF)
  "{page} of {total-pages}"
  ```
  **Why:** Ensure correct placeholders for page numbers in IronPDF.

- [ ] **Note base URL handling patterns**
  ```csharp
  // Before (SelectPdf)
  converter.ConvertHtmlString(html, baseUrl);

  // After (IronPDF)
  renderer.RenderHtmlAsPdf(html, baseUrl);
  ```
  **Why:** Base URL handling is crucial for resolving relative paths in HTML.

- [ ] **Verify target deployment platforms**
  **Why:** IronPDF supports cross-platform deployments, unlike SelectPdf which is Windows-only.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Set up test environment**
  **Why:** Ensure a controlled environment to verify migration success and PDF output quality.

### During Migration

- [ ] **Remove SelectPdf NuGet package**
  ```bash
  # .NET Framework projects:
  dotnet remove package Select.HtmlToPdf
  # .NET Core / .NET 5+ projects:
  dotnet remove package Select.HtmlToPdf.NetCore
  ```
  **Why:** Clean package removal to prevent conflicts. The real package IDs are `Select.HtmlToPdf` and `Select.HtmlToPdf.NetCore` (not "SelectPdf").

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to enable new PDF generation capabilities.

- [ ] **Update using statements**
  ```csharp
  // Before (SelectPdf)
  using SelectPdf;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure the code references the correct library.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace `HtmlToPdf` with `ChromePdfRenderer`**
  ```csharp
  // Before (SelectPdf)
  var converter = new HtmlToPdf();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses ChromePdfRenderer for modern HTML/CSS rendering.

- [ ] **Update option property names**
  ```csharp
  // Before (SelectPdf)
  converter.Options.PdfPageSize = PdfPageSize.A4;

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** Align property names with IronPDF's API.

- [ ] **Convert `PdfPageSize` to `PdfPaperSize`**
  ```csharp
  // Before (SelectPdf)
  converter.Options.PdfPageSize = PdfPageSize.A4;

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** Ensure page size settings are correctly applied in IronPDF.

- [ ] **Convert `PdfPageOrientation` to `PdfPaperOrientation`**
  ```csharp
  // Before (SelectPdf)
  converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;

  // After (IronPDF)
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  ```
  **Why:** Maintain document orientation settings in IronPDF.

- [ ] **Update header/footer to `HtmlHeaderFooter`**
  ```csharp
  // Before (SelectPdf)
  converter.Header.Add("<div>Header Content</div>");

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div>Header Content</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF provides enhanced header/footer customization using HTML.

- [ ] **Fix page number placeholders (`{page}`, `{total-pages}`)**
  ```csharp
  // Before (SelectPdf)
  "<div>Page {page_number} of {total_pages}</div>"

  // After (IronPDF)
  "<div>Page {page} of {total-pages}</div>"
  ```
  **Why:** Ensure correct page numbering in IronPDF.

- [ ] **Replace `doc.Save()` with `pdf.SaveAs()`**
  ```csharp
  // Before (SelectPdf)
  doc.Save("output.pdf");

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Use IronPDF's method for saving PDF files.

- [ ] **Remove `doc.Close()` calls**
  ```csharp
  // Before (SelectPdf)
  doc.Close();

  // After (IronPDF)
  // Not needed
  ```
  **Why:** IronPDF handles resource management automatically.

- [ ] **Update stream handling**
  ```csharp
  // Before (SelectPdf)
  var stream = new MemoryStream();
  doc.Save(stream);

  // After (IronPDF)
  var stream = pdf.Stream;
  ```
  **Why:** Use IronPDF's stream handling for PDF data.

### Post-Migration

- [ ] **Run all unit tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Verify CSS rendering (especially Grid/Flexbox)**
  **Why:** IronPDF's Chromium engine provides better CSS support.

- [ ] **Test JavaScript execution**
  **Why:** Ensure dynamic content renders correctly with IronPDF's JavaScript support.

- [ ] **Verify header/footer page numbers**
  **Why:** Confirm page numbers are displayed correctly in headers/footers.

- [ ] **Test on target platforms (Linux, Docker, etc.)**
  **Why:** IronPDF supports cross-platform deployments, unlike SelectPdf.

- [ ] **Performance test**
  **Why:** Ensure IronPDF meets performance requirements.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine may render differently; verify key documents.

- [ ] **Update CI/CD pipelines**
  **Why:** Ensure continuous integration and deployment processes are updated for IronPDF.

- [ ] **Test cloud deployments (if applicable)**
  **Why:** Verify IronPDF's cloud-native capabilities on platforms like AWS Lambda and Azure Functions.

- [ ] **Update documentation**
  **Why:** Provide accurate information on using IronPDF in the codebase.
---

## Additional Resources

### IronPDF Documentation
- **Getting Started**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF**: https://ironpdf.com/how-to/html-file-to-pdf/

### Platform-Specific Guides
- **Linux Deployment**: https://ironpdf.com/how-to/linux-deployment/
- **Docker**: https://ironpdf.com/how-to/docker-deployment/
- **Azure**: https://ironpdf.com/how-to/azure-deployment/

### Support
- **IronPDF Support**: https://ironpdf.com/support/
- **License Information**: https://ironpdf.com/licensing/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*
