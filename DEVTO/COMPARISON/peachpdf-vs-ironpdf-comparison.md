---
title: "PeachPDF vs IronPDF: feature by feature for .NET in 2026"
published: false
tags: dotnet, csharp, pdf, comparison
---
*Canonical/source version on Iron Software blog: https://ironsoftware.com/suite/blog*

## When "pure .NET" solves one problem but creates others

Pure-managed HTML-to-PDF in .NET is attractive in containerized workloads: no Chromium to bundle, no browser process to manage, a single small NuGet, predictable memory. The architectural contrast: a layout engine that interprets a subset of HTML/CSS will diverge from a browser on anything beyond simple documents — typically CSS Grid, Flexbox, web fonts pulled over HTTP, and modern selectors. Whether that tradeoff is right for you depends on how close your templates sit to "browser-grade web layout" vs "static document markup."

PeachPDF is an open-source HTML-to-PDF library written in managed .NET. It pairs PdfSharpCore (a PDF construction library) with a custom HTML/CSS parser to convert HTML into PDF documents. The architecture avoids external processes — no Puppeteer, no wkhtmltopdf, no Chromium. Deployment is a single NuGet package and resource usage is low. The tradeoff: CSS and HTML support is narrower than a browser-based engine. Simple documents (text, basic tables, simple styling) render reliably; complex layouts (CSS Grid, Flexbox, advanced selectors) typically need workarounds or may not render as expected.

## Understanding IronPDF

IronPDF uses a different architecture: embedded Chromium engine for HTML-to-PDF conversion. This means comprehensive HTML5, CSS3, and JavaScript support—if it renders in Chrome, it renders in the PDF. The tradeoff: larger deployment footprint (Chromium bundled) and higher memory usage (browser process overhead). For teams prioritizing rendering accuracy over deployment simplicity, Chromium's standards compliance eliminates CSS troubleshooting. For teams prioritizing lightweight deployment, PeachPDF's pure .NET approach avoids browser dependencies.

IronPDF's workflow is: write HTML/CSS, call `RenderHtmlAsPdf()`, get pixel-perfect output. The library handles web fonts, JavaScript execution, CSS media queries, and modern layout techniques without configuration. Installation is a single NuGet package with Chromium lifecycle managed automatically. The architectural choice is fundamental: comprehensive rendering with higher resource usage, or simple rendering with minimal dependencies.

## Key Limitations of PeachPDF

### Product Status

Open-source project on GitHub (BSD-3-Clause license, maintained by `jhaygood86`). Pre-1.0 (latest release 0.7.26, October 2025). Requires .NET 8 (no .NET Framework support). Community-driven support via GitHub Issues; no commercial support option. For teams that require LTS or vendor support, this is a constraint to weigh. For teams comfortable with OSS, the permissive license enables forking and private maintenance.

### Architectural Scope

The engine targets a subset of HTML/CSS rather than full browser standards. CSS Grid, Flexbox, transforms, transitions, and animations are not implemented in current releases — verify against your version. JavaScript is not executed; rendering is static. Web fonts via `@font-face` are supported but typically require explicit configuration. CSS media queries are limited. Complex selectors (attribute selectors, advanced pseudo-classes) may not behave the same as in a browser. SVG support is limited and `<canvas>` is not executed.

### Asset and Layout Handling

Remote HTTP images require wiring up a `NetworkAdapter` (the package ships an `HttpClientNetworkAdapter`); without one, only local files and `data:` URIs resolve. Base URLs for relative paths are not auto-detected — you configure them on the adapter or via a `<base>` tag. The layout engine is custom (not browser-based), so visual results can differ from Chrome on the same markup. Tables work for straightforward cases; deeply nested or complex tables may need simplification. Font fallback is limited — missing glyphs may render as blanks.

### Support Model

Community support via GitHub Issues. No SLA. Documentation is primarily the README. The BSD-3-Clause license enables forking and private maintenance if internal support becomes preferable.

### Debugging Considerations

With a custom HTML/CSS parser, unsupported CSS can fail silently rather than throwing — there is no DevTools-style inspector to introspect the rendered tree. Iteration on unsupported features typically means adjusting the HTML/CSS to a supported subset. Output quality is consistent for the supported subset and can vary as templates push into unsupported territory.

---

## Feature Comparison Overview

| Aspect | PeachPDF | IronPDF |
|--------|----------|---------|
| **Current Status** | Pre-1.0 OSS, .NET 8 | Established commercial, multi-platform |
| **HTML Support** | HTML/CSS subset | Full HTML5 (Chromium) |
| **Rendering Quality** | Reliable for simple layouts | Browser-grade |
| **Installation** | Lightweight (pure managed .NET) | Single NuGet (includes Chromium) |
| **Support** | Community (GitHub Issues) | Commercial (multiple channels) |
| **Backing** | Individual maintainer | Commercial entity |

---

## Troubleshooting Common Scenarios

### Scenario 1: Images Not Displaying in Generated PDFs

#### PeachPDF — Image Loading Issues

```csharp
// Install-Package PeachPDF

using System;
using System.IO;
using System.Threading.Tasks;
using PeachPDF;

public class PdfImageTroubleshooting
{
    public async Task GenerateWithRemoteImage()
    {
        // Without a NetworkAdapter, only local files and data: URIs resolve.
        var htmlWithRemoteImage = @"
            <html>
            <body>
                <h1>Report</h1>
                <img src='https://example.com/logo.png' />
            </body>
            </html>";

        var config = new PdfGenerateConfig
        {
            PageSize = PageSize.Letter,
            PageOrientation = PageOrientation.Portrait
        };

        var generator = new PdfGenerator();

        using var stream = new MemoryStream();
        var document = await generator.GeneratePdf(htmlWithRemoteImage, config);
        document.Save(stream);
    }

    public async Task UseDataUri()
    {
        // Option 1: embed images inline as data: URIs.
        byte[] imageBytes = File.ReadAllBytes("logo.png");
        string base64Image = Convert.ToBase64String(imageBytes);
        string dataUri = $"data:image/png;base64,{base64Image}";

        var html = $@"
            <html>
            <body>
                <h1>Report</h1>
                <img src='{dataUri}' />
            </body>
            </html>";

        var generator = new PdfGenerator();
        var config = new PdfGenerateConfig { PageSize = PageSize.Letter };

        using var stream = new MemoryStream();
        var document = await generator.GeneratePdf(html, config);
        document.Save(stream);
    }

    public async Task UseLocalFile()
    {
        // Option 2: reference images by local path resolvable at render time.
        var html = @"
            <html>
            <body>
                <h1>Report</h1>
                <img src='logo.png' />
            </body>
            </html>";

        File.Copy(@"C:\assets\logo.png", "logo.png", overwrite: true);

        var generator = new PdfGenerator();
        var config = new PdfGenerateConfig { PageSize = PageSize.Letter };

        using var stream = new MemoryStream();
        var document = await generator.GeneratePdf(html, config);
        document.Save(stream);
    }

    public async Task UseNetworkAdapter()
    {
        // Option 3: wire up the HttpClientNetworkAdapter so PeachPDF can fetch over HTTP.
        using var httpClient = new HttpClient();

        var config = new PdfGenerateConfig
        {
            PageSize = PageSize.Letter,
            NetworkAdapter = new HttpClientNetworkAdapter(
                httpClient,
                new Uri("https://example.com")
            )
        };

        var html = @"
            <html>
            <head>
                <base href='https://example.com/' />
            </head>
            <body>
                <h1>Report</h1>
                <img src='logo.png' />
            </body>
            </html>";

        var generator = new PdfGenerator();
        using var stream = new MemoryStream();
        var document = await generator.GeneratePdf(html, config);
        document.Save(stream);
    }
}
```

**Things to watch for:**

- **Remote images**: HTTP loading requires a `NetworkAdapter`; PeachPDF ships `HttpClientNetworkAdapter`.
- **Relative paths**: Base URI is not auto-detected; configure on the adapter or via a `<base>` tag.
- **Data URI size**: Large base64 payloads bloat HTML; local files are usually leaner.
- **Image formats**: PNG/JPEG are the common path; verify SVG/WebP support against your version.
- **Silent fallbacks**: Unresolved images may render as blank space without an exception — verify visually.

#### IronPDF — Image Handling (No Configuration Needed)

```csharp
// Install-Package IronPdf

using IronPdf;

public class PdfImageHandling
{
    public void GenerateWithImages()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Chromium handles HTTP fetching, relative paths, and standard image formats.
        var html = @"
            <html>
            <body>
                <h1>Report</h1>
                <img src='https://example.com/logo.png' />
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}
```

**Difference:** Chromium handles HTTP requests, relative paths, and image formats natively. No configuration needed for standard scenarios. See [HTML-to-PDF guide](https://ironsoftware.com/csharp/pdf/how-to/html-to-pdf/) for advanced image handling.

---

### Scenario 2: CSS Layout Not Rendering Correctly

#### PeachPDF — CSS Limitations

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using PeachPDF;

public class PdfCssTroubleshooting
{
    public async Task FlexboxLayoutAttempt()
    {
        // Flexbox is not part of PeachPDF's documented CSS subset; verify against your version.
        var htmlWithFlexbox = @"
            <html>
            <head>
                <style>
                    .container {
                        display: flex;
                        justify-content: space-between;
                    }
                    .item {
                        flex: 1;
                        padding: 10px;
                        border: 1px solid black;
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='item'>Column 1</div>
                    <div class='item'>Column 2</div>
                    <div class='item'>Column 3</div>
                </div>
            </body>
            </html>";

        var generator = new PdfGenerator();
        var config = new PdfGenerateConfig { PageSize = PageSize.Letter };

        using var stream = new MemoryStream();
        var document = await generator.GeneratePdf(htmlWithFlexbox, config);
        document.Save(stream);
    }

    public async Task UseTableLayoutInstead()
    {
        // Tables are supported and tend to be the reliable fallback for column layouts.
        var htmlWithTable = @"
            <html>
            <head>
                <style>
                    table {
                        width: 100%;
                        border-collapse: collapse;
                    }
                    td {
                        width: 33.33%;
                        padding: 10px;
                        border: 1px solid black;
                    }
                </style>
            </head>
            <body>
                <table>
                    <tr>
                        <td>Column 1</td>
                        <td>Column 2</td>
                        <td>Column 3</td>
                    </tr>
                </table>
            </body>
            </html>";

        var generator = new PdfGenerator();
        var config = new PdfGenerateConfig { PageSize = PageSize.Letter };

        using var stream = new MemoryStream();
        var document = await generator.GeneratePdf(htmlWithTable, config);
        document.Save(stream);
    }

    public async Task CssGridAttempt()
    {
        // CSS Grid is similarly outside the documented CSS subset in current releases.
        // For column layouts, tables or float-based layouts are the typical fallback.
        var htmlWithGrid = @"
            <html>
            <head>
                <style>
                    .grid {
                        display: grid;
                        grid-template-columns: 1fr 1fr 1fr;
                        gap: 10px;
                    }
                </style>
            </head>
            <body>
                <div class='grid'>
                    <div>Item 1</div>
                    <div>Item 2</div>
                    <div>Item 3</div>
                </div>
            </body>
            </html>";
    }
}
```

**Things to watch for:**

- **Flexbox/Grid**: Not in the documented CSS subset — tables or float-based layouts are the typical fallback.
- **Complex selectors**: Attribute selectors and advanced pseudo-classes may not match the same way as a browser; verify against your version.
- **Pseudo-elements**: `::before`/`::after` support can vary.
- **Cascade/specificity**: Partial coverage — keep stylesheets focused.
- **Media queries**: Limited; print-specific styles may not apply as expected.
- **Transforms/transitions**: Static rendering only.

#### IronPDF — Full CSS3 Support

```csharp
using IronPdf;

public class PdfCssHandling
{
    public void ModernCssLayout()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var html = @"
            <html>
            <head>
                <style>
                    .container {
                        display: flex;
                        justify-content: space-between;
                    }
                    .item {
                        flex: 1;
                        padding: 10px;
                        border: 1px solid black;
                    }
                    /* Grid also works */
                    .grid {
                        display: grid;
                        grid-template-columns: repeat(3, 1fr);
                        gap: 10px;
                    }
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='item'>Column 1</div>
                    <div class='item'>Column 2</div>
                    <div class='item'>Column 3</div>
                </div>
            </body>
            </html>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}
```

Chromium supports modern CSS without workarounds. For CSS compatibility details, see [CSS media types documentation](https://ironpdf.com/how-to/css-media-types/).

---

### Scenario 3: Font Rendering Issues

#### PeachPDF — Custom Font Configuration

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using PeachPDF;

public class PdfFontTroubleshooting
{
    public async Task DefaultFontBehavior()
    {
        // Font availability is OS-dependent; cross-platform deployments may see substitution
        // if the requested family is not installed on the host. Verify against your environment.
        var html = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; }
                </style>
            </head>
            <body>
                <h1>Invoice</h1>
                <p>This should use Arial font.</p>
            </body>
            </html>";

        var generator = new PdfGenerator();
        var config = new PdfGenerateConfig { PageSize = PageSize.Letter };

        using var stream = new MemoryStream();
        var document = await generator.GeneratePdf(html, config);
        document.Save(stream);
    }

    public async Task WebFontViaFontFace()
    {
        // @font-face is supported; sources typically need to be reachable through the
        // configured NetworkAdapter or pointed at a local font file.
        var html = @"
            <html>
            <head>
                <style>
                    @font-face {
                        font-family: 'CustomFont';
                        src: url('customfont.ttf');
                    }
                    body { font-family: 'CustomFont'; }
                </style>
            </head>
            <body>
                <h1>Custom Typography</h1>
            </body>
            </html>";

        var generator = new PdfGenerator();
        var config = new PdfGenerateConfig { PageSize = PageSize.Letter };

        using var stream = new MemoryStream();
        var document = await generator.GeneratePdf(html, config);
        document.Save(stream);
    }
}
```

**Things to watch for:**

- **Cross-platform font availability**: Installed fonts vary by OS — verify the families you request exist on every target host, or ship them with the app.
- **Glyph fallback**: Missing glyphs may render as blanks rather than substituting a similar face.
- **`@font-face` sources**: URLs typically need to be reachable through the configured `NetworkAdapter`, or point at local files.
- **System font enumeration**: Not exposed as a public API — choose families up front rather than discovering at runtime.

#### IronPDF — Automatic Font Handling

```csharp
using IronPdf;

public class PdfFontHandling
{
    public void WebFontsAutomatic()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var html = @"
            <html>
            <head>
                <link rel='stylesheet' href='https://fonts.googleapis.com/css2?family=Roboto' />
                <style>
                    body { font-family: 'Roboto', Arial; }
                </style>
            </head>
            <body>
                <h1>Modern Typography</h1>
                <p>Web fonts load automatically.</p>
            </body>
            </html>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}
```

Chromium downloads and embeds web fonts automatically. System fonts work cross-platform with standard CSS font-family declarations.

---

## API Mapping Reference

| PeachPDF API | IronPDF Equivalent |
|--------------|-------------------|
| `new PdfGenerator()` | `new ChromePdfRenderer()` |
| `GeneratePdf(html, config)` | `RenderHtmlAsPdf(html)` |
| `PdfGenerateConfig` | `ChromePdfRenderer.RenderingOptions` |
| `PageSize.Letter` | `RenderingOptions.PaperSize` |
| `PageOrientation.Portrait` | `RenderingOptions.PaperOrientation` |
| `@font-face` / configured fonts | Fonts load from HTML automatically via Chromium |
| `NetworkAdapter` | Not needed — HTTP handled by Chromium |
| `document.Save(stream)` | `pdf.SaveAs(path)` or `pdf.BinaryData` |
| MHTML support | Not directly supported |
| No equivalent | `pdf.MetaData` properties |
| No equivalent | `pdf.SecuritySettings` |
| No equivalent | Merge/split operations |
| No equivalent | Text extraction APIs |

---

## Comprehensive Feature Comparison

### Status & Support

| Feature | PeachPDF | IronPDF |
|---------|----------|---------|
| **License** | BSD-3-Clause (open-source) | Commercial |
| **Product Status** | Pre-1.0 (community) | Established (commercial) |
| **Support** | GitHub Issues | Multiple channels |
| **.NET Support** | .NET 8 | .NET 6/7/8/9 + .NET Framework |
| **Platform** | Windows/Linux/macOS | Windows/Linux/macOS |
| **Backing** | Individual maintainer | Commercial entity |

### HTML/CSS Support

| Feature | PeachPDF | IronPDF |
|---------|----------|---------|
| **HTML5** | Basic | Full (Chromium) |
| **CSS3** | Limited | Full support |
| **Flexbox** | No | Yes |
| **CSS Grid** | No | Yes |
| **Transforms** | No | Yes |
| **Transitions** | No | Yes |
| **Media Queries** | Limited | Yes |
| **Web Fonts** | Manual loading | Automatic |
| **Complex Selectors** | Limited | Full |

### Content Handling

| Feature | PeachPDF | IronPDF |
|---------|----------|---------|
| **Remote Images** | Requires NetworkAdapter | Automatic |
| **Data URIs** | Yes | Yes |
| **JavaScript** | No | Yes |
| **SVG** | Limited | Yes |
| **Canvas** | No | Yes |
| **Forms** | Display only | Interactive |

### PDF Operations

| Feature | PeachPDF | IronPDF |
|---------|----------|---------|
| **Create PDFs** | Yes (HTML-to-PDF) | Yes (HTML-to-PDF) |
| **Merge PDFs** | No | Yes |
| **Split PDFs** | No | Yes |
| **Text Extraction** | No | Yes |
| **Watermarks** | No | Yes |
| **Encryption** | No | Yes |
| **Digital Signatures** | No | Yes |
| **Edit Existing** | No | Limited |

### Deployment

| Feature | PeachPDF | IronPDF |
|---------|----------|---------|
| **Install Size** | Small (~5MB) | Moderate (~100MB with Chromium) |
| **Memory Usage** | Low | Moderate (browser process) |
| **Dependencies** | Managed .NET only | Chromium bundled |
| **Container-Friendly** | Very (lightweight) | Yes (standard) |

---

## When Teams Consider PeachPDF Migration

**CSS scope** drives most migration evaluations. Teams typically adopt PeachPDF for its deployment simplicity — pure managed .NET, no browser dependency — and then encounter CSS features outside its documented subset. Flexbox, Grid, and some complex selectors aren't part of current releases, so column layouts are usually expressed with tables or floats. When the design brief is "match the existing web template," that gap can become a blocker.

**Asset wiring** introduces friction. Remote images require a `NetworkAdapter`, relative paths need an explicit base URL, and data URIs work but inflate the HTML payload. For document pipelines pulling images from CDNs or external APIs, that configuration may offset some of the deployment simplicity gains. Browser-based engines fetch over HTTP automatically.

**No JavaScript execution** means templates that rely on client-side rendering need to be flattened to static HTML before generation. For server-rendered templates this is a non-issue; for SPA-style or computed-content templates, you refactor to server-side rendering.

**Production support model**: PeachPDF is community-driven (BSD-3-Clause, single maintainer). For apps requiring SLA or vendor support, that is a constraint to weigh. The permissive license enables forking and private maintenance if internal capacity is available.

**Rendering fidelity**: a custom HTML/CSS engine will diverge from a browser on edge cases. For invoices, receipts, and straightforward reports, the supported subset is usually sufficient. For brand-sensitive marketing collateral or layouts authored against modern CSS specs, browser-grade rendering tends to be closer to "what you see is what you print."

---

## Installation Comparison

### PeachPDF

```bash
dotnet add package PeachPDF
```

```csharp
using PeachPDF;
```

### IronPDF

```bash
dotnet add package IronPdf
```

```csharp
using IronPdf;
```

---

## Conclusion

PeachPDF solves a specific problem: HTML-to-PDF conversion in pure managed .NET without a browser dependency. The lightweight architecture makes deployment trivial — single NuGet, no native runtime to manage. For teams prioritizing minimal resource usage and documents that fit within a defined HTML/CSS subset (text, basic tables, simple styling), PeachPDF delivers without external process overhead. The BSD-3-Clause license provides transparency and flexibility for teams comfortable with a community support model.

Migration becomes worth evaluating when: (1) CSS requirements push beyond the supported subset and HTML workarounds start to dominate; (2) asset wiring (HTTP images, relative paths, web fonts) offsets the deployment simplicity gains; (3) rendering parity with a browser preview becomes a hard requirement; or (4) operational requirements call for an SLA. The evaluation centers on deployment simplicity versus rendering breadth.

IronPDF takes the other side of that tradeoff: a Chromium engine for HTML5, CSS3, and JavaScript at browser parity, with a larger deployment footprint (Chromium bundled) and higher per-render memory. For teams where rendering accuracy, CSS compatibility, and commercial support outweigh deployment size, Chromium-based rendering shortens the path from template to PDF.

**What drives your PDF library choice — minimal deployment footprint or comprehensive CSS rendering? Which CSS/HTML features are non-negotiable for your documents?**

*For Chromium-based rendering patterns, see [HTML-to-PDF conversion guide](https://ironsoftware.com/csharp/pdf/how-to/html-to-pdf/). For troubleshooting rendering issues, review the [troubleshooting documentation](https://ironpdf.com/troubleshooting/).*
