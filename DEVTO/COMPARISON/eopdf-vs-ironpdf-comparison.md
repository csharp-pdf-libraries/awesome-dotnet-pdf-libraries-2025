---
title: "EO.Pdf vs IronPDF: feature by feature for .NET"
published: false
tags: dotnet, csharp, pdf, comparison
---

A recurring support pattern in .NET PDF implementations goes something like this: HTML converts fine locally, rendering breaks in Azure App Service. Or: fonts display correctly in development, production PDFs show squares. Or: JavaScript charts work most of the time, then fail silently on a small percentage of runs. These are not bugs in the traditional sense — they are environment-specific behavior patterns that emerge when rendering engines interact with different runtime contexts.

EO.Pdf uses a Chromium-based approach similar to IronPDF, but with platform constraints and architectural differences that surface in specific deployment scenarios. Teams evaluating options often ask "which is more stable?" but the more actionable question is "which edge cases align with my infrastructure?"

## Understanding IronPDF

IronPDF wraps a Chromium rendering engine into a .NET library that runs on Windows, Linux, and macOS. The architecture bundles platform-specific Chromium binaries within the NuGet package, eliminating the need for separate browser installations or runtime dependencies outside the .NET ecosystem.

The library handles HTML to PDF conversion through the `ChromePdfRenderer` class, providing control over JavaScript execution, CSS media types, custom fonts, and asset loading. For document manipulation, the `PdfDocument` class offers merging, splitting, encryption, form filling, and content extraction without requiring separate tools.

## Key Characteristics of EO.Pdf

### Product Status

EO.Pdf receives regular updates; the migration guide references NuGet version 26.1.34 (March 2026). The library is actively maintained by Essential Objects. Platform support remains Windows-only on .NET Core / .NET 5+, which creates constraints for teams deploying to Linux containers or macOS development environments.

### Platform Coverage

**Cross-platform deployment**: EO.Pdf explicitly does not support non-Windows systems on .NET Core. From the vendor's own documentation: "EO.Pdf and EO.WebBrowser supports .NET Core 3.1 and above (.NET Core 3.1, .NET 5 and .NET 6) on Windows only. Non Windows systems are not supported." See https://www.essentialobjects.com/Doc/Common/dotnetcore.html.

This limitation impacts:

- Docker deployments on Linux
- Azure App Service Linux plans
- macOS local development
- AWS Lambda or serverless functions on non-Windows runtimes

### Technical Notes

**Dependency management**: .NET Core / .NET 5+ projects typically require explicit installation of `System.Drawing.Common` and `System.Text.Encoding.CodePages` packages, depending on the features used. Missing these dependencies can cause runtime errors that may not surface until specific code paths execute.

**Process architecture**: The HTML rendering pipeline runs through an out-of-process helper (`eowp.exe` is the EO.WebBrowser process), which introduces inter-process communication overhead and additional considerations around process cleanup, especially in high-concurrency scenarios.

### Support Model

Commercial library with vendor support. Active updates indicate ongoing maintenance; specific SLA, response time, and support tier details should be verified against your licensing level.

### Architecture Considerations

**Windows-only deployment**: Teams building cloud-native applications face architectural constraints. If your stack includes Linux containers, serverless functions, or cross-platform CI/CD pipelines, EO.Pdf's Windows-only requirement forces infrastructure decisions at the platform level.

**Legacy WebForms heritage**: Some of EO's documentation and examples have a heritage from the ASP.NET WebForms era, which may require adaptation for modern MVC / Razor Pages / Blazor workflows.

## Feature Comparison Overview

| Category | EO.Pdf | IronPDF |
|----------|--------|---------|
| **Current Status** | v26.1.34 (March 2026) | Active monthly releases |
| **HTML Support** | Chromium HTML5/CSS3 | Chromium HTML5/CSS3 |
| **Rendering Quality** | Browser-based | Browser-based |
| **Installation** | NuGet + extra runtime packages | Single NuGet |
| **Support** | Commercial vendor | Commercial vendor (engineering support) |
| **Platform Reach** | Windows-only on .NET Core | Cross-platform .NET 6+ |

---

## Code Comparison: Common Operations

### EO.Pdf — HTML String to PDF

```csharp
// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;

class Program
{
    static void Main()
    {
        string html = "<html><body><h1>Hello World</h1><p>This is a PDF generated from HTML.</p></body></html>";

        HtmlToPdf.ConvertHtml(html, "output.pdf");

        Console.WriteLine("PDF created successfully!");
    }
}
```

**Common troubleshooting scenarios:**

- **System.Drawing.Common not found**: On .NET Core / .NET 5+, the error "Could not load file or assembly 'System.Drawing.Common'" typically requires explicit package installation. Add `<PackageReference Include="System.Drawing.Common" Version="6.0.0" />` to your project file.

- **Encoding errors with special characters**: If PDFs display garbled text for non-ASCII characters, install `System.Text.Encoding.CodePages` and register encodings:

```csharp
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
```

- **Process cleanup**: In high-throughput scenarios, the `eowp.exe` helper process can accumulate. Monitor process count and implement application-level recycling if process handles do not clean up as expected.

- **Local file access**: When HTML references local files (images, CSS), set `HtmlToPdfOptions.BaseUrl` to the base path. Without this, relative paths may not resolve.

- **Azure deployment**: On Azure App Service, ensure the app runs in 64-bit mode if using the 64-bit NuGet package. Platform mismatch can produce "BadImageFormatException" at runtime.

- **Thread safety**: EO.Pdf's primary entry point is the static `HtmlToPdf` class with shared `HtmlToPdf.Options`. Mutating that global state from multiple threads is not safe; use locking or isolate configuration per request.

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

        string html = "<html><body><h1>Hello World</h1><p>This is a PDF generated from HTML.</p></body></html>";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");

        Console.WriteLine("PDF created successfully!");
    }
}
```

The [HTML to PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) covers platform-specific nuances for Windows, Linux, and macOS deployments. IronPDF handles its native dependencies internally — no separate `System.Drawing.Common` installation is required.

---

### EO.Pdf — HTML File with External Assets

```csharp
// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;
using System.Drawing;

class Program
{
    static void Main()
    {
        HtmlToPdfOptions options = new HtmlToPdfOptions();
        options.PageSize = PdfPageSizes.A4;
        // OutputArea is in inches: x, y, width, height inside the page.
        options.OutputArea = new RectangleF(0.5f, 0.5f, 7.5f, 10.5f);

        HtmlToPdf.ConvertUrl("file:///C:/input.html", "output.pdf", options);

        Console.WriteLine("PDF with custom settings created.");
    }
}
```

**Troubleshooting common asset-loading issues:**

- **Images not appearing**: If HTML contains `<img src="logo.png">`, ensure `BaseUrl` points to the directory containing `logo.png`. Absolute paths tend to be more reliable than relative paths.

- **CSS stylesheet failures**: External stylesheets referenced as `<link rel="stylesheet" href="styles.css">` may fail if CORS or file-access permissions block loading. Inline critical CSS as a workaround.

- **Font rendering**: Custom fonts loaded via `@font-face` may not render if the font file path is inaccessible from the rendering process. Test font paths in a regular browser first.

- **JavaScript timeouts**: If conversion hangs indefinitely, tune the timeout value and check JavaScript console errors. Some libraries (e.g., Google Maps) wait for network resources that may not load in headless environments.

- **Memory consumption**: Large images or complex layouts can cause memory pressure. Monitor memory usage and implement resource limits when rendering user-supplied HTML.

### IronPDF — HTML File with External Assets

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.MarginLeft = 20;
        renderer.RenderingOptions.MarginRight = 20;

        var pdf = renderer.RenderHtmlFileAsPdf("C:/input.html");
        pdf.SaveAs("output.pdf");

        Console.WriteLine("PDF with custom settings created.");
    }
}
```

The [HTML File to PDF guide](https://ironpdf.com/how-to/html-file-to-pdf/) explains how IronPDF handles base-path resolution by treating the HTML file's directory as the asset root. For explicit control, use `BaseUrlOrPath`:

```csharp
renderer.RenderingOptions.BaseUrlOrPath = @"C:\Projects\Assets";
```

---

### EO.Pdf — JavaScript-Heavy HTML

```csharp
// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;

class Program
{
    static void Main()
    {
        string htmlWithJs = @"
            <html>
            <head>
                <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
            </head>
            <body>
                <canvas id='myChart' width='400' height='400'></canvas>
                <script>
                    var ctx = document.getElementById('myChart').getContext('2d');
                    var myChart = new Chart(ctx, {
                        type: 'bar',
                        data: {
                            labels: ['Red', 'Blue', 'Yellow'],
                            datasets: [{
                                label: '# of Votes',
                                data: [12, 19, 3]
                            }]
                        }
                    });
                </script>
            </body>
            </html>";

        HtmlToPdfOptions options = new HtmlToPdfOptions();
        // Wait for client-side script execution before snapshotting the page.
        options.AfterRenderDelay = 2000; // 2 seconds

        HtmlToPdf.ConvertHtml(htmlWithJs, "chart.pdf", options);

        Console.WriteLine("JavaScript-based PDF created");
    }
}
```

**JavaScript execution notes:**

- **Chart not rendering**: If the canvas appears blank, increase `AfterRenderDelay`. Some chart libraries require additional time to complete animations and rendering.

- **External script loading**: HTTPS resources may fail if the rendering process does not trust certain certificates. Test with CDN scripts known to have valid certificates.

- **Console visibility**: JavaScript console output is not exposed by the conversion API by default. Open the HTML in a regular browser to debug JS errors before automating.

- **Timing**: Scripts that depend on `window.onload` or `DOMContentLoaded` may behave inconsistently under fixed delays. Prefer explicit delay tuning or signal-based mechanisms where available.

- **Library compatibility**: Not every JavaScript library behaves the same in a headless rendering context. Libraries expecting user interaction (e.g., click events) require workarounds.

### IronPDF — JavaScript-Heavy HTML

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        string htmlWithJs = @"
            <html>
            <head>
                <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
            </head>
            <body>
                <canvas id='myChart' width='400' height='400'></canvas>
                <script>
                    var ctx = document.getElementById('myChart').getContext('2d');
                    var myChart = new Chart(ctx, {
                        type: 'bar',
                        data: {
                            labels: ['Red', 'Blue', 'Yellow'],
                            datasets: [{
                                label: '# of Votes',
                                data: [12, 19, 3]
                            }]
                        }
                    });
                </script>
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.WaitFor.JavaScript();

        var pdf = renderer.RenderHtmlAsPdf(htmlWithJs);
        pdf.SaveAs("chart.pdf");
    }
}
```

The [JavaScript in HTML to PDF guide](https://ironpdf.com/examples/javascript-html-to-pdf/) shows how IronPDF's `WaitFor` API and JavaScript signaling provide explicit control over rendering timing, which can reduce guesswork around fixed delay values.

---

### EO.Pdf — PDF Merging

```csharp
// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;

class Program
{
    static void Main()
    {
        PdfDocument doc1 = new PdfDocument("file1.pdf");
        PdfDocument doc2 = new PdfDocument("file2.pdf");

        // EO.Pdf has no instance Append(); merge is a static method that returns a new PdfDocument.
        PdfDocument mergedDoc = PdfDocument.Merge(doc1, doc2);

        mergedDoc.Save("merged.pdf");

        Console.WriteLine("PDFs merged successfully!");
    }
}
```

**Merge operation notes:**

- **Memory footprint**: Merging large PDFs can consume significant memory. Process documents in batches and dispose of intermediate objects promptly.

- **Page numbering**: Merged documents do not automatically renumber pages. Implement custom logic for continuous page numbering if needed.

- **Bookmarks**: If source PDFs contain bookmarks, behavior across merge boundaries varies. Verify bookmark handling against the vendor's documentation or flatten bookmarks before merging.

- **Form field collisions**: PDFs with interactive forms may have field name collisions after merging. Flatten forms before merging if post-merge editing is not required.

- **Font embedding**: Missing fonts in source PDFs can cause rendering issues in merged output. Ensure all fonts are embedded in source documents.

### IronPDF — PDF Merging

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf1 = PdfDocument.FromFile("file1.pdf");
        var pdf2 = PdfDocument.FromFile("file2.pdf");

        var merged = PdfDocument.Merge(new List<PdfDocument> { pdf1, pdf2 });
        merged.SaveAs("merged.pdf");

        Console.WriteLine("PDFs merged successfully!");
    }
}
```

The [Merge & Split tutorial](https://ironpdf.com/how-to/merge-or-split-pdfs/) covers additional scenarios like merging specific page ranges and combining newly generated PDFs with existing documents in a single workflow.

---

## API Mapping Reference

| Operation | EO.Pdf | IronPDF |
|-----------|--------|---------|
| HTML string to PDF | `HtmlToPdf.ConvertHtml(html, path)` | `ChromePdfRenderer.RenderHtmlAsPdf(html)` |
| HTML file to PDF | `HtmlToPdf.ConvertHtml(File.ReadAllText(path), output)` | `ChromePdfRenderer.RenderHtmlFileAsPdf(path)` |
| URL to PDF | `HtmlToPdf.ConvertUrl(url, path)` | `ChromePdfRenderer.RenderUrlAsPdf(url)` |
| Enable JavaScript | `options.EnableJavaScript = true` | `RenderingOptions.EnableJavaScript = true` |
| Set base path | `options.BaseUrl = path` | `RenderingOptions.BaseUrlOrPath = path` |
| Wait for JavaScript | `options.AfterRenderDelay = ms` | `RenderingOptions.WaitFor.JavaScript()` |
| Load existing PDF | `new PdfDocument(path)` | `PdfDocument.FromFile(path)` |
| Merge PDFs | `PdfDocument.Merge(doc1, doc2)` (static) | `PdfDocument.Merge(pdf1, pdf2)` or `Merge(IEnumerable<PdfDocument>)` |
| Set password | `doc.Security.UserPassword = pwd` | `pdf.SecuritySettings.UserPassword = pwd` |
| Watermark | Verify in vendor docs | `ApplyWatermark(html, rotation, opacity)` |

---

## Comprehensive Feature Comparison

### Status

| Feature | EO.Pdf | IronPDF |
|---------|--------|---------|
| Active development | v26.1.34 (March 2026) | Monthly releases |
| .NET 8 support | Yes (Windows) | Yes (cross-platform) |
| .NET 9 support | Yes (Windows) | Yes (cross-platform) |
| .NET Framework 4.6.2+ | Yes | Yes |
| Linux support | Not supported on .NET Core | Yes |
| macOS support | Not supported on .NET Core | Yes |
| Docker support | Windows containers only | All platforms |

### Support

| Feature | EO.Pdf | IronPDF |
|---------|--------|---------|
| Documentation | Available | Comprehensive |
| Code examples | Available | Extensive examples |
| Community forum | Available | Active forum |
| Technical support | Commercial | Engineering support |

### Content Creation

| Feature | EO.Pdf | IronPDF |
|---------|--------|---------|
| HTML5 support | Chromium-based | Chromium-based |
| CSS3 support | Chromium-based | Chromium-based |
| JavaScript execution | Yes | Yes |
| Web fonts | Yes | Google Fonts, custom |
| SVG rendering | Native | Native |
| Canvas rendering | Yes | Yes |
| Responsive layouts | Media queries | Media queries |
| Print CSS | `@media print` | `@media print` |

### PDF Operations

| Feature | EO.Pdf | IronPDF |
|---------|--------|---------|
| Merge PDFs | Yes (static `PdfDocument.Merge`) | Yes |
| Split PDFs | Yes | Yes |
| Extract pages | Yes | Yes |
| Rotate pages | Yes | Yes |
| Extract text | Yes | Yes |
| Extract images | Yes | Yes |
| Form filling | Yes | AcroForm |
| Form flattening | Yes | Yes |
| Headers/footers | Yes (via events / ACM) | HTML-based |
| Page numbers | Yes | HTML placeholders |

### Security

| Feature | EO.Pdf | IronPDF |
|---------|--------|---------|
| Password encryption | Yes | 128/256-bit AES |
| User permissions | Yes | Granular |
| Digital signatures | Yes | X.509 |
| Certificate validation | Verify in vendor docs | Yes |
| Redaction | Verify in vendor docs | Permanent |
| Metadata removal | Yes | Yes |

### Architectural Differences

| Aspect | EO.Pdf | IronPDF |
|--------|--------|---------|
| Platform reach | Windows-only on .NET Core | Windows / Linux / macOS |
| HTML rendering process | Out-of-process (`eowp.exe`) | In-process |
| Extra runtime packages | `System.Drawing.Common`, encoding providers | None required |
| Configuration model | Static `HtmlToPdf.Options` (global) | Instance-based `ChromePdfRenderer` |
| Azure App Service notes | 64-bit config alignment required | Platform-agnostic NuGet |

### Development

| Feature | EO.Pdf | IronPDF |
|---------|--------|---------|
| NuGet installation | Multiple packages | One package |
| External dependencies | `System.Drawing.Common`, etc. | None |
| Async API | Limited | Full async |
| Error messages | Standard exceptions | Detailed |
| Testing support | Unit-testable | Unit-testable |

---

## Installation Comparison

### EO.Pdf Installation

```bash
Install-Package EO.Pdf
# For .NET Core / .NET 5+, you may also need:
Install-Package System.Drawing.Common
Install-Package System.Text.Encoding.CodePages
```

```csharp
using EO.Pdf;
using System.Text;

// Register code pages for .NET Core / .NET 5+
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
```

### IronPDF Installation

```bash
Install-Package IronPdf
```

```csharp
using IronPdf;
using IronPdf.Rendering;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

**Deployment checklist:**

- **EO.Pdf**: Verify 64-bit vs 32-bit package matches runtime, ensure `System.Drawing.Common` is compatible with the hosting environment, register encoding providers in .NET Core startup.
- **IronPDF**: Single NuGet package includes platform binaries; no additional encoding registration required.

---

## When Migration Becomes Mandatory

Platform constraints typically drive migration decisions more than feature gaps. Teams encounter forcing functions around EO.Pdf in these scenarios:

**Linux deployment requirements**: If your organization standardizes on Linux-based Docker images or Azure App Service Linux plans, EO.Pdf's Windows-only constraint on .NET Core becomes a blocker. The cost of maintaining Windows-specific infrastructure for PDF generation may not justify the feature set.

**Cross-platform development**: Teams with developers on macOS or Linux cannot run EO.Pdf locally for development. This can create environment parity issues — Windows developers produce PDFs locally, while colleagues on other platforms need Windows VMs or containers to debug PDF generation.

**Process management**: If production logs show `eowp.exe` accumulation under high load, the architectural difference between EO.Pdf's external process model and IronPDF's in-process approach becomes relevant. Teams without Windows server expertise may find process lifecycle management adds operational overhead.

**Dependency coordination**: Projects targeting .NET 8+ may encounter `System.Drawing.Common` deprecation considerations or compatibility issues. EO.Pdf's dependency on this package can create ongoing maintenance friction as the .NET ecosystem moves toward cross-platform graphics abstractions.

## IronPDF's Technical Approach

IronPDF bundles Chromium binaries within its NuGet package for Windows, Linux, and macOS. This removes the dependency chain that requires separate package installations and encoding provider registration. The rendering engine runs in-process rather than as a separate executable, which can simplify resource management and cleanup.

For troubleshooting, IronPDF provides detailed exception messages that distinguish between HTML rendering issues, JavaScript errors, and file system problems. The [HTML to PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) includes platform-specific deployment guides for Azure, AWS, Docker, and on-premises environments.

The `ChromePdfRenderOptions` class exposes granular control:

```csharp
renderer.RenderingOptions.Timeout = 60; // seconds
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.RenderDelay(500); // milliseconds
```

This configuration surface makes behavior explicit rather than implicit, which can reduce the "works sometimes" class of bugs.

---

**What platform constraints have you encountered when deploying .NET PDF libraries? Share your Docker or Linux deployment experiences in the comments.**

**Learn more:**

- [HTML to PDF with Chrome rendering](https://ironpdf.com/tutorials/html-to-pdf/)
- [JavaScript execution in PDFs](https://ironpdf.com/examples/javascript-html-to-pdf/)
- [Merge & Split PDFs](https://ironpdf.com/how-to/merge-or-split-pdfs/)
