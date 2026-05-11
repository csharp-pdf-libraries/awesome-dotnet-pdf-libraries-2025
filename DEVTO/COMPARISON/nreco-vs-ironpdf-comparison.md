---
title: "NReco PDF Generator vs IronPDF: what the docs do not tell you"
published: false
canonical_url: https://ironsoftware.com/blog/compare-to-other-components/nreco-net-core-html-to-pdf-alternatives/
tags: dotnet, csharp, pdf, comparison
---
## When the PDF library runs your code instead of the other way around

NReco.PdfGenerator is a .NET wrapper around the wkhtmltopdf native binary, which means every PDF conversion spawns an external process. That architectural choice shapes everything downstream: temp directory permissions, Windows security policies, diagnostic depth when something fails. The wrapper surfaces what the executable returns — often just an exit code.

That worked well enough when Qt WebKit was a reasonable rendering target. The wkhtmltopdf project's last stable release (0.12.6) shipped in June 2020, and the upstream repository was archived on January 2, 2023. The underlying WebKit fork dates to roughly 2012. The ecosystem moved on; teams running NReco often have not, because migration feels expensive up front.

This guide walks through what that calculation looks like today: what still works on NReco.PdfGenerator, where the friction surfaces, and when the math tips toward a modern alternative. For a step-by-step API map, see the [IronPDF migration guide for NReco.PdfGenerator](https://ironpdf.com/blog/migration-guides/migrate-from-nreco-pdfgenerator-to-ironpdf/).

## Understanding IronPDF

IronPDF renders HTML to PDF using an embedded Chromium engine—the same Blink renderer that powers Chrome, Edge, and Electron apps. This means HTML5, CSS Grid, Flexbox, ES6 JavaScript, and WOFF2 fonts work exactly as they do in a modern browser. No "this CSS property isn't supported" surprises. No external executables to spawn or monitor.

The library provides both HTML-to-PDF rendering and comprehensive PDF manipulation in one package: merge documents, extract text, add watermarks, apply digital signatures, fill forms programmatically. For teams managing document workflows beyond simple generation, this consolidates tooling instead of forcing integration across multiple libraries.

## Key characteristics of NReco.PdfGenerator

### Product status
NReco.PdfGenerator 1.2.1 was published on NuGet in January 2023 and there have been no further releases on nuget.org. It bundles wkhtmltopdf 0.12.6 (June 2020). The upstream wkhtmltopdf repository was archived on January 2, 2023. The underlying Qt WebKit fork is from roughly 2012 and does not include CSS Grid, Flexbox, ES6 modules, or other web standards introduced in the years since.

### Scope
HTML-to-PDF rendering is the documented scope. PDF manipulation operations (merge, split, watermark, digital signature, form fill, text extraction) are not part of the package — those require pairing with another library such as iTextSharp.

### Architecture notes
The library shells out to an external wkhtmltopdf binary for every conversion. Temp directory permissions, IIS app pool identity, and binary extraction paths all become operational concerns. JavaScript runs inside the Qt WebKit context, which broadly tracks the ECMAScript 5 era. Modern CSS features such as `flex`, `grid`, and `@supports` are not supported by the bundled engine. WOFF2 web fonts are not supported by the bundled engine.

### Support and maintenance
NReco offers community support through the project's issue tracker; there is no commercial SLA tier listed for the free edition. With no upstream updates to the underlying renderer since 2020 and the wkhtmltopdf project archived, the rendering engine itself is no longer receiving fixes.

### Deployment notes
Docker and Linux deployments require manual wkhtmltopdf binary management or the paid `NReco.PdfGenerator.LT` package together with a separately installed wkhtmltopdf. The free edition's license terms restrict production use to non-SaaS single-server scenarios; SaaS, multi-server, and redistribution use require the $199 enterprise pack per the NReco site. Process spawning introduces per-conversion overhead, and when a conversion fails the wrapper typically surfaces an exit code rather than a stack trace.

## Feature Comparison Overview

| Feature | NReco.PdfGenerator | IronPDF |
|---------|-------------------|---------|
| **Current status** | Last NuGet release Jan 2023; bundles wkhtmltopdf 0.12.6 (Jun 2020) | Active development, regular releases |
| **HTML support** | Qt WebKit fork (~2012); CSS Grid and Flexbox not supported by the engine | Chromium Blink; HTML5, modern CSS, ES6+ |
| **Rendering engine** | wkhtmltopdf native binary | Embedded Chromium |
| **Installation** | NuGet package + extracts wkhtmltopdf binary at runtime; VC++ runtime on Windows | NuGet package; Chromium binaries included |
| **Support** | Community issue tracker | Commercial support with SLA tiers |
| **Upstream status** | wkhtmltopdf repository archived Jan 2023 | Active vendor roadmap |

## HTML String Rendering with Headers and Footers

### NReco — HTML String to PDF with Custom Headers

```csharp
using NReco.PdfGenerator;
using System;
using System.IO;

namespace PdfGenerationExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var pdfConverter = new HtmlToPdfConverter();

            // Page dimensions in millimetres
            pdfConverter.PageWidth = 210;  // A4 width
            pdfConverter.PageHeight = 297; // A4 height
            pdfConverter.Margins = new PageMargins
            {
                Top = 20,
                Bottom = 20,
                Left = 15,
                Right = 15
            };

            // PageHeaderHtml is a string passed through to wkhtmltopdf
            pdfConverter.PageHeaderHtml = "Report Generated: " + DateTime.Now.ToShortDateString();

            // HTML content
            string htmlContent = @"
                <html>
                <head>
                    <style>
                        body { font-family: Arial; }
                        .invoice { border: 1px solid #ccc; padding: 20px; }
                    </style>
                </head>
                <body>
                    <div class='invoice'>
                        <h1>Monthly Report</h1>
                        <p>Generated on " + DateTime.Now + @"</p>
                    </div>
                </body>
                </html>";

            // GeneratePdf invokes the bundled wkhtmltopdf binary
            byte[] pdfBytes = pdfConverter.GeneratePdf(htmlContent);

            File.WriteAllBytes("nreco_output.pdf", pdfBytes);
        }
    }
}
```

**Behavioural notes for this code path:**
- `PageHeaderHtml` is forwarded to wkhtmltopdf as a plain string; rich HTML/CSS in the header is constrained by what the bundled engine supports
- Each call spawns the wkhtmltopdf executable, which adds per-conversion process startup overhead
- The wkhtmltopdf process needs write permissions on the configured temp directory
- When a conversion fails, the wrapper typically returns an exit code rather than a structured stack trace
- CSS Grid, Flexbox, and WOFF2 fonts are not supported by the engine
- JavaScript runs inside the Qt WebKit context (broadly ECMAScript 5 era)

### IronPDF — HTML String to PDF with HTML Headers

For complete API reference, see the [ChromePdfRenderer class documentation](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderer.html).

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

// HTML headers support full HTML/CSS markup
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    MaxHeight = 20,
    HtmlFragment = "<div style='text-align:center; font-size:10px;'>Report Generated: {date}</div>",
    DrawDividerLine = true
};

// HTML content with modern CSS
string htmlContent = @"
    <style>
        .invoice {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
        }
    </style>
    <div class='invoice'>
        <h1>Monthly Report</h1>
        <p>Generated on {date}</p>
    </div>";

// Render with Chromium engine
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("ironpdf_output.pdf");
```

IronPDF headers support full HTML/CSS styling and dynamic placeholders like `{page}`, `{total-pages}`, `{date}`. The Chromium renderer handles CSS Grid and modern layouts without workarounds. Learn more about [HTML string rendering techniques](https://ironpdf.com/how-to/html-string-to-pdf/).

## URL to PDF Conversion with JavaScript Execution

### NReco — Rendering URLs with JavaScript Wait

```csharp
using NReco.PdfGenerator;
using System;
using System.IO;

namespace UrlToPdfExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var converter = new HtmlToPdfConverter();

            // JavaScript is executed inside the Qt WebKit context
            converter.EnableJavaScript = true;

            // Fixed delay in milliseconds before snapshotting the page
            converter.JavaScriptDelay = 1000;

            converter.Orientation = PageOrientation.Portrait;
            converter.Size = PageSize.A4;

            converter.Margins.Top = 10;
            converter.Margins.Bottom = 10;
            converter.Margins.Left = 10;
            converter.Margins.Right = 10;

            // Forward wkhtmltopdf stdout/stderr to the host process
            converter.Quiet = false;

            try
            {
                // Synchronous call; returns when the wkhtmltopdf process exits
                var pdfBytes = converter.GeneratePdfFromFile(
                    "https://example.com/dashboard",
                    null
                );

                File.WriteAllBytes("url_output.pdf", pdfBytes);
            }
            catch (PdfGenerationException ex)
            {
                Console.WriteLine($"Generation failed: {ex.Message}");
            }
        }
    }
}
```

**Behavioural notes for this code path:**
- The Qt WebKit engine broadly tracks ECMAScript 5; modern syntax such as `const`/`let`, arrow functions, and async/await may not be available depending on the build
- `GeneratePdfFromFile` is a synchronous call; there is no first-class async API
- Page readiness is gated by a fixed `JavaScriptDelay`, not by a load/condition wait
- Network and rendering errors typically surface as wkhtmltopdf exit codes
- CORS and TLS behaviour depend on what was compiled into the bundled wkhtmltopdf build (last release June 2020) — verify against your version

### IronPDF — Async URL Rendering with Wait-for-Load

For comprehensive rendering options, review the [ChromePdfRenderOptions documentation](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderOptions.html).

```csharp
using IronPdf;
using System.Threading.Tasks;

async Task GenerateFromUrl()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.EnableJavaScript = true;
    renderer.RenderingOptions.WaitFor.RenderDelay(1000); // ms after page load

    var pdf = await renderer.RenderUrlAsPdfAsync(new Uri("https://example.com/dashboard"));
    await pdf.SaveAsAsync("url_output.pdf");
}
```

IronPDF's Chromium engine understands modern JavaScript (ES2020+) and handles dynamic SPAs that rely on fetch, promises, and async rendering. The async methods don't block threads. Detailed walkthrough: [Pixel-perfect HTML to PDF rendering](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/).

## Local HTML File with External Assets

### NReco — File Conversion with Asset References

```csharp
using NReco.PdfGenerator;
using System;
using System.IO;

namespace FileToPdfExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var converter = new HtmlToPdfConverter();

            converter.Size = PageSize.Letter;
            converter.Orientation = PageOrientation.Portrait;
            converter.Margins = new PageMargins(15, 15, 15, 15);

            converter.Zoom = 1.0f;

            converter.GenerateGrayscale = false;
            converter.LowQuality = false;

            // Footer placeholders: [page] / [topage] in wkhtmltopdf syntax
            converter.PageFooterHtml = "Page [page] of [topage]";

            try
            {
                // File path should be absolute
                string absolutePath = Path.GetFullPath("template.html");

                // Asset references in the HTML resolve relative to the file
                byte[] pdfContent = converter.GeneratePdfFromFile(
                    absolutePath,
                    null
                );

                File.WriteAllBytes("file_output.pdf", pdfContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Conversion error: {ex.Message}");
                // wkhtmltopdf binaries are extracted under App_Data/wkhtmltopdf/ by default
            }
        }
    }
}
```

**Behavioural notes for this code path:**
- Asset paths must be absolute or relative to the HTML file location
- The bundled Qt WebKit engine supports TTF/WOFF fonts; WOFF2 is not supported
- `@import` and other file:// resource loads depend on wkhtmltopdf's local-file access flags — verify against your configuration
- Modern image formats such as WebP and AVIF may not render in the bundled engine
- The wkhtmltopdf process needs read access to any referenced files
- wkhtmltopdf uses `[page]` and `[topage]` placeholders in header/footer markup

### IronPDF — File Rendering with Auto Asset Resolution

Complete file conversion guide: [HTML File to PDF Tutorial](https://ironpdf.com/how-to/html-file-to-pdf/).

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

// IronPDF placeholders: {page}, {total-pages}, {date}, {time}, {html-title}, {url}
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    MaxHeight = 15,
    HtmlFragment = "<center><i>Page {page} of {total-pages}</i></center>"
};

// Relative paths resolve from the HTML file's directory
var pdf = renderer.RenderHtmlFileAsPdf("template.html");
pdf.SaveAs("file_output.pdf");
```

IronPDF automatically sets the base URL to the HTML file's directory, so relative paths to CSS, images, and fonts work without manual configuration. Supports all modern image formats (WebP, AVIF) and font formats (WOFF2, variable fonts).

## Batch Processing Multiple HTML Files

### NReco — Sequential File Batch Processing

```csharp
using NReco.PdfGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BatchProcessingExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var converter = new HtmlToPdfConverter();

            converter.Size = PageSize.A4;
            converter.Margins = new PageMargins(20, 20, 20, 20);

            // Per-conversion timeout in milliseconds
            converter.ExecutionTimeout = 60000;

            string[] htmlFiles = Directory.GetFiles("invoices/", "*.html");

            var results = new List<(string filename, bool success, string error)>();

            // Sequential loop: each iteration spawns wkhtmltopdf
            foreach (var htmlFile in htmlFiles)
            {
                try
                {
                    byte[] pdfBytes = converter.GeneratePdfFromFile(htmlFile, null);

                    string outputPath = Path.ChangeExtension(htmlFile, ".pdf");
                    File.WriteAllBytes(outputPath, pdfBytes);

                    results.Add((htmlFile, true, null));
                }
                catch (PdfGenerationException ex)
                {
                    // Exception message is typically the wkhtmltopdf exit code / stderr
                    results.Add((htmlFile, false, ex.Message));
                }
                catch (Exception ex)
                {
                    results.Add((htmlFile, false, ex.Message));
                }
            }

            // Report results
            int succeeded = results.Count(r => r.success);
            int failed = results.Count(r => !r.success);

            Console.WriteLine($"Completed: {succeeded} succeeded, {failed} failed");

            foreach (var result in results.Where(r => !r.success))
            {
                Console.WriteLine($"Failed: {result.filename} - {result.error}");
            }
        }
    }
}
```

**Behavioural notes for this code path:**
- Each conversion spawns the wkhtmltopdf process; parallelising the loop multiplies that overhead
- Timeout is per-conversion; there is no batch-level control surface
- Error context is limited to whatever the wkhtmltopdf process surfaces (typically exit codes and stderr lines)
- No first-class progress or cancellation callbacks
- Long-running batches benefit from defensive temp-directory cleanup — verify against your version

### IronPDF — Parallel Batch with Thread Safety

```csharp
using IronPdf;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var htmlFiles = Directory.GetFiles("invoices/", "*.html");

Parallel.ForEach(htmlFiles, htmlFile =>
{
    var pdf = renderer.RenderHtmlFileAsPdf(htmlFile);
    pdf.SaveAs(Path.ChangeExtension(htmlFile, ".pdf"));
});
```

IronPDF's Chromium engine is designed for in-process parallel rendering, so the loop above runs each file through the same renderer without spawning external executables. Resource pooling is handled inside the library.

## API Mapping Reference

| NReco.PdfGenerator | IronPDF Equivalent |
|--------------------|-------------------|
| `HtmlToPdfConverter` class | `ChromePdfRenderer` class |
| `GeneratePdf(string htmlContent)` | `RenderHtmlAsPdf(string html)` |
| `GeneratePdfFromFile(string path, string output)` | `RenderHtmlFileAsPdf(string path)` |
| `EnableJavaScript` property | `RenderingOptions.EnableJavaScript` |
| `JavaScriptDelay` property | `RenderingOptions.WaitFor.RenderDelay(ms)` |
| `PageHeaderHtml` property (text only) | `RenderingOptions.HtmlHeader.HtmlFragment` (full HTML) |
| `PageFooterHtml` property (text only) | `RenderingOptions.HtmlFooter.HtmlFragment` (full HTML) |
| `Margins` property | `RenderingOptions.MarginTop/Bottom/Left/Right` |
| `Size` property (PageSize enum) | `RenderingOptions.PaperSize` |
| `Orientation` property | `RenderingOptions.PaperOrientation` |
| `Zoom` property | `RenderingOptions.Zoom` |
| `GenerateGrayscale` property | `RenderingOptions.GrayScale` |
| `CustomCssUrl` property | `RenderingOptions.CustomCssUrl` |
| `ExecutionTimeout` | Operation-level timeout via CancellationToken |

## Comprehensive Feature Comparison

| Category | Feature | NReco.PdfGenerator | IronPDF |
|----------|---------|-------------------|---------|
| **Status** | Last NuGet release | January 2023 (1.2.1) | Active, regular releases |
| | Upstream engine | wkhtmltopdf, repository archived Jan 2023 | Chromium, actively maintained |
| | Public roadmap | Not published for the free package | Available |
| **Support** | Documentation | Project docs and issue tracker | Vendor docs and tutorials |
| | Technical support | Community issue tracker | Commercial support tiers |
| | SLA available | No (free edition) | Yes (Enterprise) |
| **Content creation** | HTML to PDF | Yes (Qt WebKit) | Yes (Chromium) |
| | URL to PDF | Yes (sync) | Yes (sync and async) |
| | Modern CSS (Grid, Flexbox) | Not supported by the engine | Supported |
| | JavaScript | Qt WebKit (ES5 era) | Chromium (modern ES) |
| | Custom fonts | TTF/WOFF | TTF/WOFF/WOFF2 and others |
| | Headers/Footers | wkhtmltopdf header/footer HTML | HtmlHeaderFooter with full HTML/CSS |
| | Watermarks | Not in package | Built-in |
| | Page-number placeholders | `[page]`, `[topage]` | `{page}`, `{total-pages}`, `{date}`, `{html-title}` |
| **PDF operations** | Merge PDFs | Not in package | Yes |
| | Split PDFs | Not in package | Yes |
| | Extract text | Not in package | Yes |
| | Extract images | Not in package | Yes |
| | Fill forms | Not in package | Yes |
| | Create forms from HTML | Not in package | Yes |
| | Digital signatures | Not in package | Yes |
| | Encryption / passwords | Not in package | Yes |
| **Security** | TLS support | Depends on the bundled wkhtmltopdf build | Tracks Chromium |
| | Known CVEs in engine | CVE-2020-21365, CVE-2022-35583 (unpatched upstream) | Patched as Chromium updates |
| **Development** | Async/await | Synchronous API | Full async support |
| | Thread safety | Not documented for the wrapper | Designed for concurrent use |
| | Parallel processing | Multiplies process-spawn overhead | In-process parallelism |
| | Docker support | Manual wkhtmltopdf binary management | Chromium included with the package |
| | Cross-platform | Requires `NReco.PdfGenerator.LT` + wkhtmltopdf install | Cross-platform from the same package |
| | .NET support | .NET Framework and .NET Core/.NET 5+ | .NET Framework and .NET 6+ |

### Patterns to watch for in production

These behaviour patterns show up across community discussion threads for wkhtmltopdf-based wrappers; verify against your own version and configuration before changing anything:

1. Permission errors from the IIS app pool identity when the Windows temp folder or wkhtmltopdf extraction path is not writable
2. Process-timeout exit codes when rendering very large or complex HTML
3. Missing images or CSS when relative asset paths do not resolve under the wkhtmltopdf process
4. IIS deployments where the `App_Data` extraction directory is locked or recycled between requests
5. Unicode and font-fallback behaviour that depends on which fonts are available to the Qt WebKit engine

## Installation Comparison

### NReco.PdfGenerator Installation

```bash
# Windows-only version (includes wkhtmltopdf binaries)
dotnet add package NReco.PdfGenerator

# Cross-platform version (requires manual wkhtmltopdf installation)
dotnet add package NReco.PdfGenerator.LT
```

```csharp
using NReco.PdfGenerator;

// Windows: works immediately after NuGet restore
// Linux/macOS: requires wkhtmltopdf installation and path configuration
var converter = new HtmlToPdfConverter();

// For LT version on Linux
converter.WkHtmlToPdfExeName = "wkhtmltopdf";
converter.PdfToolPath = "/usr/local/bin/"; // where wkhtmltopdf is installed
```

NReco.PdfGenerator requires the Visual C++ 2015 Runtime on Windows. Docker and Linux deployments need manual wkhtmltopdf binary management or the paid `NReco.PdfGenerator.LT` package alongside a separately installed wkhtmltopdf. The free edition is licensed for non-SaaS single-server production environments; SaaS, multi-server, or redistribution scenarios require the $199 enterprise pack per the NReco site.

### IronPDF Installation

Complete installation steps: [Chrome PDF Rendering Engine Guide](https://ironpdf.com/how-to/ironpdf-2021-chrome-rendering-engine-eap/).

```bash
dotnet add package IronPdf
```

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Same package works on Windows, Linux, macOS, and Docker
var renderer = new ChromePdfRenderer();
```

IronPDF is fully managed .NET code with embedded Chromium engine. No native dependencies to install. Cross-platform support included without separate packages.

## When to stay on NReco, when to migrate

NReco.PdfGenerator can be a reasonable fit when HTML templates use older layout patterns (no CSS Grid or Flexbox), templates change rarely, and the deployment target is a stable Windows host where the wkhtmltopdf process has been reliable. The free edition's licence covers non-SaaS single-server production use, which is sometimes enough on its own.

The math shifts when CSS Grid or Flexbox layouts are required, modern JavaScript needs to run, process timeouts or permission errors appear in production logs, or PDF operations beyond rendering (merge, split, watermark, digital signature, form fill, text extraction) enter scope. Docker and cross-platform requirements also push toward alternatives because the wkhtmltopdf binary management has to be solved separately.

The practical question is how much engineering time goes into working around the wkhtmltopdf wrapper versus investing once in a migration. With the wkhtmltopdf upstream archived since January 2023, that calculation only moves in one direction over time.

IronPDF consolidates HTML rendering and PDF manipulation in a single package built on an embedded Chromium engine. The API is async-friendly, the library runs the same way on Windows, Linux, macOS, and Docker, and there are no external executables to manage or to debug when a conversion fails. For a step-by-step API map and code-level diff, see the [NReco.PdfGenerator to IronPDF migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-nreco-pdfgenerator-to-ironpdf/).

Have you run into any of these wkhtmltopdf-wrapper patterns in production, and how did you work around them?

**Related resources:**
- [IronPDF licensing and pricing](https://ironpdf.com/licensing/)
- [Chrome PDF rendering engine technical details](https://ironpdf.com/how-to/ironpdf-2021-chrome-rendering-engine-eap/)
