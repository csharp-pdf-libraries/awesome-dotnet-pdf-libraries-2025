---
title: "PDF Duo .NET vs IronPDF: the practical breakdown for .NET"
published: false
canonical_url: https://github.com/csharp-pdf-libraries/awesome-dotnet-pdf-libraries-2025/blob/main/pdf-duo/
tags: dotnet, csharp, pdf, comparison
---
## When "it still compiles" doesn't mean you should keep using it

PDF Duo .NET's last public release (v2.4) shipped in December 2010 and targets .NET Framework 1.1 through 3.5. If you maintain a reporting pipeline that was wired up on that runtime and has hummed along untouched for a decade, the day you need to add QR codes, embed an international font, or move the host process onto modern .NET is the day the library's constraints stop being academic.

The contrast looks like this:

| Metric | PDF Duo .NET | Modern Baseline |
|--------|---------------|----------------|
| .NET runtime support | .NET Framework 1.1 - 3.5 | .NET 6 / 8 / 10 |
| HTML rendering | Custom engine (engine not disclosed by vendor) | Chromium / Blink |
| Font formats | Basic embedding | WOFF2, variable fonts |
| Async API | Not part of the documented surface | Full async/await |
| Release cadence | Last release December 2010 | Regular releases |

This isn't a referendum on whether PDF Duo was well-made in 2010 — it was a reasonable component for the runtime it targeted. The point is narrower: the web standards that landed after 2010 (CSS Grid, Flexbox, ES6, WOFF2) post-date the library, and the runtimes most new .NET projects target (Core / 5+ / 6+ / 8 / 10) post-date its supported framework list. Either of those is enough to motivate the migration; together they make it almost forced.

Teams keeping a 2010-era library in production are usually making a risk calculation, not a technical decision: how much does this part of the system matter, and what breaks if we change it? The rest of this guide is meant to support that calculation with concrete API mappings rather than vibes.


## Understanding IronPDF

IronPDF uses Chromium's Blink rendering engine (the same technology as Chrome, Edge, Brave) to convert HTML to PDF. This means the library supports modern web standards by default: HTML5 semantic elements, CSS Grid layouts, ES2020+ JavaScript, WebAssembly modules, WOFF2 fonts. When Chrome browser adds support for new CSS features, IronPDF inherits them in the next release because they share the underlying rendering engine.

Beyond HTML-to-PDF conversion, IronPDF provides programmatic PDF manipulation: merge multiple documents, extract text and images, add watermarks, apply digital signatures, fill form fields, encrypt with passwords. For teams managing document workflows beyond simple generation, this consolidates functionality into a single well-maintained library.

## Key Constraints of PDF Duo .NET

### Release status
The vendor's own download listing shows v2.4 dated December 10, 2010 as the last public release, with no new versions since. The DuoDimension website is still reachable, but there is no visible repository, issue tracker, or changelog beyond that page.

### Runtime targeting
The vendor lists supported runtimes as .NET Framework 1.1 / 2.0 / 3.0 / 3.5 on Windows. There is no documented support for .NET Framework 4.x, .NET Core, .NET 5+, Linux, or macOS, and the component is not distributed on NuGet — installation is a `PDFDuo.dll` reference from a downloaded archive.

### Feature surface
The documented public API is essentially `DuoDimension.HtmlToPdf` with `OpenHTML(...)` and `SavePDF(...)`. The vendor docs do not document native APIs for watermarking, password protection, digital signatures, form filling, text extraction, PDF/A, or PDF merging — teams that needed merge in this era typically paired PDF Duo with a second library such as iTextSharp 4.x.

### Rendering engine
PDF Duo is described as a self-contained component with no Office or Acrobat dependency, but the vendor does not publish which HTML/CSS engine it uses. Modern CSS (Grid, Flexbox, Variables), ES6+ JavaScript, WOFF2 fonts, and WebP/AVIF images are not claimed as supported features — verify against your own templates before assuming they render.

### Documentation and community
One vendor product page, a couple of sample snippets, and a 2010 press release. No API reference site, no GitHub repository, and no Stack Overflow tag of meaningful size. Community-sourced answers for edge cases are scarce.

## Feature Comparison Overview

| Feature | PDF Duo .NET | IronPDF |
|---------|-------------|---------|
| **Last release** | v2.4, December 2010 | Active, regular releases |
| **HTML engine** | Vendor-custom (engine not disclosed) | Chromium Blink |
| **Distribution** | `PDFDuo.dll` download from duodimension.com | NuGet `IronPdf` |
| **Runtime support** | .NET Framework 1.1 - 3.5, Windows | .NET Framework 4.6.2+, .NET 6/7/8/9/10, Linux, macOS, Docker |
| **Support model** | Vendor contact only | Engineering support with SLA options |
| **Feature surface** | HTML-to-PDF (`OpenHTML` / `SavePDF`) | HTML-to-PDF, merge, security, signatures, OCR, forms, watermark |

## Benchmark: HTML String Conversion Speed

### PDF Duo — Basic HTML to PDF

```csharp
// PDF Duo .NET is not on NuGet. Add a reference to PDFDuo.dll from the
// vendor download. Public API: DuoDimension.HtmlToPdf with OpenHTML / SavePDF.
using DuoDimension;
using System;
using System.Diagnostics;
using System.IO;

namespace PdfDuoPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            int iterations = 100;
            long totalMilliseconds = 0;

            for (int i = 0; i < iterations; i++)
            {
                stopwatch.Restart();

                string html = @"
                    <html>
                    <head>
                        <style>
                            body { font-family: Arial; font-size: 12pt; }
                            .header { font-weight: bold; }
                        </style>
                    </head>
                    <body>
                        <div class='header'>Invoice #" + i + @"</div>
                        <p>Date: " + DateTime.Now.ToShortDateString() + @"</p>
                        <table border='1'>
                            <tr><td>Item</td><td>Price</td></tr>
                            <tr><td>Widget</td><td>$10.00</td></tr>
                        </table>
                    </body>
                    </html>";

                try
                {
                    // OpenHTML takes a file path, URL, or HTML written to disk first.
                    // There is no documented in-memory string overload.
                    string tempHtml = Path.GetTempFileName() + ".html";
                    File.WriteAllText(tempHtml, html);

                    var conv = new HtmlToPdf();
                    conv.OpenHTML(tempHtml);
                    conv.SavePDF($"output_{i}.pdf");

                    stopwatch.Stop();
                    totalMilliseconds += stopwatch.ElapsedMilliseconds;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Conversion {i} failed: {ex.Message}");
                }
            }

            double averageMs = totalMilliseconds / (double)iterations;
            Console.WriteLine($"Average conversion time: {averageMs}ms");
            Console.WriteLine($"Total time: {totalMilliseconds}ms");
        }
    }
}
```

**Architectural characteristics worth measuring on your hardware:**
- Each render requires writing the HTML payload to disk before `OpenHTML`, adding a filesystem round trip per call
- The documented API is synchronous — no `async` overloads, so the calling thread is blocked for the duration of the render
- No documented renderer-reuse / pooling pattern; each conversion constructs a fresh `HtmlToPdf` instance
- Modern CSS (Grid, Flexbox, CSS Variables) and ES6+ JavaScript are not claimed as supported — verify against your templates before benchmarking complex layouts

### IronPDF — Modern HTML Conversion Benchmark

Complete API reference: [ChromePdfRenderer documentation](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderer.html).

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

class BenchmarkProgram
{
    static async Task Main(string[] args)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var stopwatch = new Stopwatch();
        int iterations = 100;
        long totalMilliseconds = 0;

        var renderer = new ChromePdfRenderer();

        for (int i = 0; i < iterations; i++)
        {
            stopwatch.Restart();

            // Modern HTML with CSS Grid, Flexbox, modern fonts
            string html = $@"
                <style>
                    body {{ font-family: -apple-system, system-ui; }}
                    .invoice {{
                        display: grid;
                        grid-template-columns: 1fr 1fr;
                        gap: 20px;
                    }}
                </style>
                <div class='invoice'>
                    <div><strong>Invoice #{i}</strong></div>
                    <div>{DateTime.Now:yyyy-MM-dd}</div>
                </div>";

            var pdf = await renderer.RenderHtmlAsPdfAsync(html);
            await pdf.SaveAsAsync($"output_{i}.pdf");
            pdf.Dispose();

            stopwatch.Stop();
            totalMilliseconds += stopwatch.ElapsedMilliseconds;
        }

        double averageMs = totalMilliseconds / (double)iterations;
        Console.WriteLine($"Average: {averageMs}ms | Total: {totalMilliseconds}ms");
    }
}
```

**Architectural characteristics on the IronPDF side:**
- The `ChromePdfRenderer` instance can be created once and reused across renders, amortising Chromium startup
- Modern CSS (Grid, Flexbox, Variables) renders through Blink without manual workarounds
- `RenderHtmlAsPdfAsync` / `SaveAsAsync` return `Task`, so the calling thread is not blocked
- The renderer is documented as safe to drive in parallel for batch scenarios
- Chromium updates ship in regular IronPDF releases, so engine improvements flow into the library over time

Numbers will vary by hardware, template complexity, and image payload — run the benchmark on your own templates rather than trusting any vendor's quoted figures.

## Benchmark: Memory Usage in Batch Processing

### PDF Duo — Memory Profile (single-threaded sequential)

```csharp
using DuoDimension;
using System;
using System.Diagnostics;
using System.IO;

class MemoryTest
{
    static void Main(string[] args)
    {
        var process = Process.GetCurrentProcess();
        long startMemory = process.WorkingSet64;

        Console.WriteLine($"Starting memory: {startMemory / 1024 / 1024}MB");

        // Generate 500 PDFs sequentially
        for (int i = 0; i < 500; i++)
        {
            string html = GenerateInvoiceHtml(i);

            // OpenHTML expects a path/URL, so write the HTML to a temp file first.
            string tempHtml = Path.GetTempFileName() + ".html";
            File.WriteAllText(tempHtml, html);

            var conv = new HtmlToPdf();
            conv.OpenHTML(tempHtml);
            conv.SavePDF($"invoice_{i}.pdf");

            if (i % 100 == 0)
            {
                long currentMemory = process.WorkingSet64;
                long used = (currentMemory - startMemory) / 1024 / 1024;
                Console.WriteLine($"After {i} PDFs: {used}MB used");

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                long afterGC = process.WorkingSet64;
                long afterGCUsed = (afterGC - startMemory) / 1024 / 1024;
                Console.WriteLine($"After GC: {afterGCUsed}MB used");
            }
        }

        long finalMemory = process.WorkingSet64;
        long totalUsed = (finalMemory - startMemory) / 1024 / 1024;
        Console.WriteLine($"Final memory used: {totalUsed}MB");
    }

    static string GenerateInvoiceHtml(int id)
    {
        return $"<html><body><h1>Invoice {id}</h1></body></html>";
    }
}
```

**Things to watch when running this on your own templates:**
- Each call constructs a new `HtmlToPdf` and writes a temp HTML file — sequential cost adds up in long batches
- The vendor docs do not document a renderer-reuse / pooling pattern, so steady-state memory depends on GC behavior
- Long-running services should track working-set growth across many thousands of renders rather than assuming the per-call cost is the whole picture

### IronPDF — Memory Profile (Parallel Processing)

Detailed rendering options: [ChromePdfRenderOptions class](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderOptions.html).

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

class MemoryTest
{
    static async Task Main(string[] args)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var process = Process.GetCurrentProcess();
        long startMemory = process.WorkingSet64 / 1024 / 1024;

        Console.WriteLine($"Starting memory: {startMemory}MB");

        var renderer = new ChromePdfRenderer();
        var tasks = Enumerable.Range(0, 500).Select(i => GeneratePdfAsync(renderer, i));

        await Task.WhenAll(tasks);

        GC.Collect();
        GC.WaitForPendingFinalizers();

        long finalMemory = process.WorkingSet64 / 1024 / 1024;
        Console.WriteLine($"Final memory: {finalMemory}MB ({finalMemory - startMemory}MB used)");
    }

    static async Task GeneratePdfAsync(ChromePdfRenderer renderer, int id)
    {
        string html = $"<html><body><h1>Invoice {id}</h1></body></html>";

        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        await pdf.SaveAsAsync($"invoice_{id}.pdf");
        // Explicit disposal releases Chromium resources
    }
}
```

**Things to watch on the IronPDF side:**
- One `ChromePdfRenderer` is reused for every render, so the Chromium subprocess is amortised rather than spun up per call
- `PdfDocument` implements `IDisposable` — `using var pdf = ...` returns native resources promptly
- The renderer is documented as safe to drive in parallel, so batch workers can fan out without extra synchronisation
- Steady-state memory in a long-running service depends on both managed GC and the Chromium engine — measure on your own workload before extrapolating

## Modern HTML / CSS Feature Coverage

The features below all post-date PDF Duo's 2010 release and are not part of its claimed support matrix. The most reliable approach is to drop a representative template through each engine and inspect the output, rather than rely on yes/no checklists. A minimal scaffold for that on the IronPDF side:

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var testCases = new[]
{
    ("CSS Grid Layout", "<style>.grid{display:grid;grid-template-columns:1fr 1fr;}</style><div class='grid'><div>A</div><div>B</div></div>"),
    ("Flexbox", "<style>.flex{display:flex;justify-content:space-between;}</style><div class='flex'><div>Left</div><div>Right</div></div>"),
    ("CSS Variables", "<style>:root{--color:blue;}.test{color:var(--color);}</style><div class='test'>Text</div>"),
    ("Web Fonts (WOFF2)", "<style>@font-face{font-family:'Custom';src:url('font.woff2');}</style><div style='font-family:Custom;'>Text</div>"),
    ("Modern Selectors", "<style>div:not(.skip){color:red;}</style><div>Red</div><div class='skip'>Black</div>"),
    ("Box Shadow", "<style>.shadow{box-shadow:0 4px 8px rgba(0,0,0,0.2);}</style><div class='shadow'>Shadow</div>")
};

var renderer = new ChromePdfRenderer();
foreach (var (feature, html) in testCases)
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"feature_{feature.Replace(' ', '_')}.pdf");
}
```

**Feature coverage in this comparison:**

| Feature | PDF Duo .NET | IronPDF |
|---------|-------------|---------|
| CSS Grid | Not claimed by vendor | Supported via Chromium |
| Flexbox | Not claimed by vendor | Supported via Chromium |
| CSS Variables | Not claimed by vendor | Supported via Chromium |
| WOFF2 web fonts | Not claimed by vendor | Supported via Chromium |
| `@media` queries | Not claimed by vendor | Supported via Chromium |
| Modern selectors (`:not()` etc.) | Not claimed by vendor | Supported via Chromium |
| Transform / transition | Not claimed by vendor | Supported via Chromium |
| SVG | Vendor docs do not characterise depth | Supported via Chromium |
| WebP images | Not claimed by vendor | Supported via Chromium |
| ES6+ JavaScript | Not claimed by vendor | Supported via Chromium |

For any cell marked "not claimed by vendor," verify against your specific template — the feature may render partially or not at all depending on which CSS / HTML constructs the document actually uses.

## API Mapping Reference

| PDF Duo .NET | IronPDF |
|--------------|---------|
| `new DuoDimension.HtmlToPdf()` | `new ChromePdfRenderer()` |
| `conv.OpenHTML(htmlFile); conv.SavePDF(path);` | `renderer.RenderHtmlFileAsPdf(htmlFile).SaveAs(path)` |
| `conv.OpenHTML(url); conv.SavePDF(path);` | `renderer.RenderUrlAsPdf(url).SaveAs(path)` |
| Write HTML string to temp file, then `OpenHTML` | `renderer.RenderHtmlAsPdf(htmlString).SaveAs(path)` |
| _(no documented settings object for page layout)_ | `RenderingOptions.PaperSize` |
| _(no documented orientation property)_ | `RenderingOptions.PaperOrientation` |
| _(no documented margin object)_ | `RenderingOptions.MarginTop / MarginBottom / MarginLeft / MarginRight` |
| Font embedding not characterised in vendor docs | Automatic from HTML / CSS via Chromium |
| JavaScript not part of documented API | `RenderingOptions.EnableJavaScript = true` |
| Async APIs not part of documented surface | Full `async` / `await` (`RenderHtmlAsPdfAsync`, `SaveAsAsync`) |
| Parallel use pattern not documented | Renderer documented as safe to drive in parallel |
| PDF merging not part of `HtmlToPdf` surface | `PdfDocument.Merge(IEnumerable<PdfDocument>)` |
| Watermarking not part of `HtmlToPdf` surface | `pdf.ApplyWatermark(html)` |

## Comprehensive Feature Comparison

| Category | Feature | PDF Duo .NET | IronPDF |
|----------|---------|-------------|---------|
| **Release** | Last version | v2.4, December 2010 | Active, regular releases |
| | Distribution | `PDFDuo.dll` download | NuGet `IronPdf` |
| | Update cadence | No public releases since 2010 | Regular updates |
| **Support** | Documentation | Single vendor product page | Documentation site and API reference |
| | Technical support | Vendor contact only | Engineering support with SLA options |
| | Community | Negligible footprint | Active community |
| **Rendering** | HTML engine | Vendor-custom (engine not disclosed) | Chromium / Blink |
| | URL to PDF | `OpenHTML(url)` | `RenderUrlAsPdf(url)` |
| | CSS3 features | Not claimed | Supported via Chromium |
| | HTML5 | Not claimed | Supported via Chromium |
| | JavaScript | Not part of documented API | Supported via Chromium |
| | Modern fonts (WOFF2) | Not claimed | Supported via Chromium |
| | Headers / footers | Not in documented `HtmlToPdf` surface | `HtmlHeader` / `HtmlFooter` |
| | Watermarks | Not in documented surface | `pdf.ApplyWatermark(...)` |
| **PDF operations** | Merge | Not in documented surface | `PdfDocument.Merge(...)` |
| | Split | Not in documented surface | Supported |
| | Extract text | Not in documented surface | `pdf.ExtractAllText()` |
| | Forms | Not in documented surface | `pdf.Form` |
| | Digital signatures | Not in documented surface | `pdf.SignWithFile(...)` |
| | Encryption | Not in documented surface | `pdf.SecuritySettings` |
| **Runtimes** | .NET Framework | 1.1, 2.0, 3.0, 3.5 | 4.6.2+ |
| | .NET Core / 5+ | Not claimed | .NET 6 / 7 / 8 / 9 / 10 |
| | Async / await | Not in documented API | Full support |
| | Parallel use | Not documented | Documented as safe to drive in parallel |
| | Docker | Not claimed | Supported |
| | Platforms | Windows | Windows, Linux, macOS |

### Considerations when staying on a 2010-era component

Because PDF Duo has not received public releases since 2010, anything that has changed in the broader stack since then is not reflected in the library. Worth checking against your own deployment before assuming behaviour:

1. Unicode and emoji coverage for non-Latin scripts in your templates
2. Whether the HTML parser hardens against untrusted input — if any of your HTML comes from user input, treat the surface as unaudited
3. Working-set behaviour in long-running services, since there is no public profiling story
4. Compatibility with .NET Framework 4.x and later — the documented target list ends at 3.5
5. TLS / HTTPS behaviour for `OpenHTML(url)` against modern endpoints, since the underlying stack predates current cipher suites

## Installation Comparison

### PDF Duo .NET installation

```text
# PDF Duo .NET is not distributed on NuGet.
# Download the archive from https://www.duodimension.com/ and add a
# reference to PDFDuo.dll in your project. Target framework must be
# .NET Framework 1.1 - 3.5 per the vendor's supported runtime list.
```

```csharp
using DuoDimension;

var conv = new HtmlToPdf();
conv.OpenHTML("input.html");
conv.SavePDF("output.pdf");
```

There is no `dotnet add package` story and no SemVer feed. If your CI builds against a private NuGet mirror, you will need to vendor `PDFDuo.dll` into the repository or an internal artifact store.

### IronPDF Installation

Complete installation guide: [HTML String to PDF Tutorial](https://ironpdf.com/how-to/html-string-to-pdf/).

```bash
dotnet add package IronPdf
```

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<html><body>Content</body></html>");
pdf.SaveAs("output.pdf");
```

Standard NuGet installation. Targets .NET Framework 4.6.2+ and .NET 6 / 7 / 8 / 9 / 10, with cross-platform binaries for Windows, Linux, and macOS.

## Migration economics: the real cost of staying on PDF Duo

### Where the time tends to go

The figures below are rough planning numbers from typical .NET HTML-to-PDF migrations — treat them as a starting point for your own estimate rather than a universal benchmark.

**Time absorbed by working around an older engine:**
- Reshaping HTML templates so they avoid CSS features the engine does not claim to support: typically half a day to a day per non-trivial template
- Manual visual checks because there is no "preview in Chrome" workflow that matches the rendered PDF: a few hours per template
- Troubleshooting rendering issues when the engine surfaces a generic exception: variable, often the longest tail

**Opportunity cost:**
- Modern CSS Grid / Flexbox layouts force a fallback to table-based designs
- Web fonts beyond what is system-installed are not part of the documented surface
- HTML pulled from external sources (CMS templates, marketing pages) may not render the way they look in a browser

**Technical-debt drag:**
- The supported runtime list ends at .NET Framework 3.5, so the host process is pinned to that runtime
- No public release stream means no security or bug-fix flow

### Migration effort estimate

**Indicative scope for a small document-generation surface (5-10 PDF templates):**
- Install IronPDF and wire up the license key: under an hour
- Replace `OpenHTML` / `SavePDF` call sites with `RenderHtmlAsPdf` / `SaveAs`: a few hours
- Re-test templates with modern CSS features enabled rather than worked around: an hour or two
- Remove the temp-file detour required by `OpenHTML` for HTML strings: an hour or two

**What typically falls out of the migration:**
- Template authors can use the same CSS they use in the browser
- The Chromium preview matches the PDF output, so visual review converges quickly
- PDF post-processing (merge, watermark, encrypt, extract text) moves into one library

For the longer-form walk-through, see: [HTML File to PDF conversion](https://ironpdf.com/how-to/html-file-to-pdf/).

## When to migrate

Staying on PDF Duo can be reasonable if all of the following hold: you generate a modest volume of PDFs from a small set of frozen templates, those templates rely only on layout features the engine handled in 2010 (table-based layouts, system fonts), the host process is happy on .NET Framework 3.5, and the HTML you feed in is fully under your control.

Migration tends to become necessary when any of those constraints break: rendering quality degrades because modern HTML / CSS does not match the engine, you need capabilities that aren't part of the documented `HtmlToPdf` surface (merging, watermarks, signatures), the runtime needs to move onto .NET Core / .NET 6+, or a security review flags unmaintained dependencies. Performance is rarely the headline driver — the more common reason is that a needed feature is simply not on the table.

A practical heuristic: count the engineering hours per quarter spent working around the older engine. If that number is non-trivial, the migration usually pays for itself inside a quarter.

IronPDF provides modern HTML rendering via Chromium, handles CSS Grid / Flexbox / Variables without manual workarounds, and consolidates PDF post-processing (merge, watermark, encrypt, extract) into a single library. For new .NET projects in 2026, those are baseline expectations rather than nice-to-haves.

Have you hit a similar "frozen library" scenario in your codebase? What finally pushed the migration?

**Related resources:**
- [IronPDF Chrome Rendering Engine](https://ironpdf.com/how-to/ironpdf-2021-chrome-rendering-engine-eap/)
- [Pixel-Perfect HTML to PDF Rendering](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/)
