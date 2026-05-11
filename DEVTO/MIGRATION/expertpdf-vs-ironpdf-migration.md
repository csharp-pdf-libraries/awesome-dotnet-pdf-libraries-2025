---
title: "Migrating from ExpertPdf to IronPDF: less setup, same output"
published: false
tags: dotnet, csharp, pdf, migration
---

Performance regressions are usually what surfaces ExpertPdf as a migration candidate. Teams running batch document generation at scale start noticing memory pressure, render-time variance, or output fidelity issues on complex templates. The question isn't whether ExpertPdf can produce a PDF — it can — but whether it can hold up under your specific workload profile. This article approaches the migration from a measurement-first mindset: establish baselines, understand the tradeoffs, then migrate with clear benchmarks to verify the outcome.

By the end, you'll have a complete migration scaffold, API mapping tables, and four working before/after code examples. The benchmarking approach is useful regardless of which library you end up running.

---

## Why Migrate (Without Drama)

1. **CSS rendering fidelity** — ExpertPdf historically supports a Trident/IE engine plus a WebKit2 engine added in v12.2. Modern CSS (Grid, Flexbox, CSS variables, custom fonts via @font-face) renders best on the WebKit2 engine; the older engines are partial.
2. **JavaScript execution** — Older rendering pipelines don't reliably run modern JavaScript. JS-generated DOM may not appear in the output.
3. **Linux deployment** — ExpertPdf's `.NetCore` packages target .NET Standard 2.0, so they run on .NET 5–9, but container environments may need additional native dependencies depending on the engine used.
4. **Performance at scale** — Under concurrent load, rendering throughput and memory behavior are worth profiling before assuming either library is better for your case.
5. **.NET targeting** — ExpertPdf's `.NetCore` packages target .NET Standard 2.0 / .NET Framework 4.6.1, so you do not get native multi-target builds or trimming-friendly assemblies.
6. **Async rendering** — ExpertPdf's `PdfConverter` is primarily synchronous; async-first ASP.NET pipelines have to wrap calls in `Task.Run`.
7. **Fragmented product suite** — Merging, security, splitting, and PDF-to-image are separate NuGet packages (`ExpertPdf.MergePdf`, `ExpertPdf.PdfSecurity`, `ExpertPdf.SplitPdf`, `ExpertPdf.PdfToImage`), each typically licensed separately.
8. **Debug experience** — Custom-engine rendering failures can be opaque. Chromium-based renderers tend to produce more actionable output.
9. **URL rendering** — `NavigationTimeout` and `ConversionDelay` are measured in seconds, with a different timeout model than Chromium-based renderers.
10. **Print CSS media queries** — Behavior of `@media print` rules differs between the Trident, WebKit2, and Chromium engines.

### Side-by-Side Comparison

| Aspect | ExpertPdf | IronPDF |
|---|---|---|
| Focus | HTML/URL → PDF | HTML/URL → PDF via Chromium |
| Vendor | Outside Software Inc. | Iron Software |
| API Style | `PdfConverter` with `PdfDocumentOptions` | `ChromePdfRenderer` with `RenderingOptions` |
| Learning Curve | Low for basic use | Low for basic use |
| HTML Rendering | Trident + WebKit2 engines | Chromium, full CSS3/JS |
| Product Model | Fragmented (6+ NuGet packages) | All-in-one library |
| Thread Safety | New `PdfConverter` per call recommended | `ChromePdfRenderer` reusable |
| Namespace | `ExpertPdf.HtmlToPdf` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Complexity | Notes |
|---|---|---|
| HTML string → PDF | Low | `PdfConverter` → `ChromePdfRenderer` |
| URL → PDF | Low | `GetPdfBytesFromUrl` → `RenderUrlAsPdf` |
| HTML file → PDF | Low | `GetPdfBytesFromHtmlFile` → `RenderHtmlFileAsPdf` |
| Rendering options (margins, size) | Low | `PdfDocumentOptions` → `RenderingOptions` |
| Text extraction | Medium | Available in `ExpertPdf.PdfExtractor`; consolidated in IronPDF |
| Merge PDFs | Low | `PDFMerge` package → built-in `PdfDocument.Merge()` |
| Split PDFs | Medium | `ExpertPdf.SplitPdf` package → built-in `pdf.CopyPages` |
| Watermark | Medium | Header HTML overlay → IronPDF `TextStamper` |
| Password / security | Low | `PdfSecurityOptions` → `pdf.SecuritySettings` |
| Headers / footers | Medium | Text-based options → HTML-based `HtmlHeaderFooter` |
| JavaScript rendering | N/A → Low | New capability in IronPDF |

### Decision Matrix

| Scenario | Recommendation |
|---|---|
| CSS Grid/Flexbox templates rendering incorrectly | Migrate — Chromium will resolve these |
| Simple HTML templates, no CSS complexity | Lower urgency; migrate when convenient |
| Batch PDF generation, performance-sensitive | Benchmark both before deciding |
| Using 3+ ExpertPdf packages (merge, security, split) | Migrate — IronPDF consolidates into one |

---

## Benchmarking Approach

> **Disclaimer:** No published benchmark numbers are cited here because workload-specific measurements vary significantly. Run these tests on your own hardware with your own templates.

### Benchmark Template Selection

Choose templates that represent your actual workload distribution:

```
Template categories to test:
1. Simple HTML (no CSS, no images)         — baseline
2. CSS-heavy (flexbox, grid, custom fonts) — rendering fidelity + timing
3. Image-heavy (multiple embedded images)  — memory + throughput
4. Long document (100+ pages)             — scaling behavior
5. Complex table (1000+ rows)             — streaming vs. in-memory
```

### BenchmarkDotNet Setup

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using IronPdf;
using ExpertPdf.HtmlToPdf;

[MemoryDiagnoser]
[SimpleJob]
public class PdfBenchmarks
{
    private static readonly ChromePdfRenderer _ironRenderer = new ChromePdfRenderer();

    private string _simpleHtml = "<h1>Benchmark</h1><p>Test content.</p>";

    [Benchmark]
    public byte[] IronPdf_SimpleHtml()
    {
        using var pdf = _ironRenderer.RenderHtmlAsPdf(_simpleHtml);
        return pdf.BinaryData;
    }

    [Benchmark]
    public byte[] ExpertPdf_SimpleHtml()
    {
        // PdfConverter is generally instantiated per call
        PdfConverter pdfConverter = new PdfConverter();
        return pdfConverter.GetPdfBytesFromHtmlString(_simpleHtml);
    }
}

class Program
{
    static void Main() => BenchmarkRunner.Run<PdfBenchmarks>();
}
```

### What to Measure

```csharp
// Memory measurement via dotnet-counters
// dotnet-counters monitor --process-id <PID> System.Runtime

// Key metrics to watch:
// - gen-0-gc-count, gen-1-gc-count (GC pressure)
// - working-set (MB)
// - threadpool-queue-length (under concurrent load)

// Throughput test pattern
using IronPdf;
using System.Diagnostics;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();

// Warm up
using var _ = renderer.RenderHtmlAsPdf("<p>warmup</p>");

int iterations = 100;
var sw = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++)
{
    using var pdf = renderer.RenderHtmlAsPdf($"<h1>Doc {i}</h1><p>Content.</p>");
    // Don't save — measure pure render time
}
sw.Stop();

double avgMs = sw.ElapsedMilliseconds / (double)iterations;
Console.WriteLine($"IronPDF avg render time: {avgMs:F1}ms over {iterations} iterations");
```

### Parallel Throughput Test

```csharp
using IronPdf;
using System.Threading.Tasks;
using System.Diagnostics;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();

// Warm up
using var _ = renderer.RenderHtmlAsPdf("<p>warmup</p>");

int concurrency = 10;
int docsPerThread = 20;

var sw = Stopwatch.StartNew();
var tasks = Enumerable.Range(0, concurrency).Select(t => Task.Run(() =>
{
    for (int i = 0; i < docsPerThread; i++)
    {
        using var pdf = renderer.RenderHtmlAsPdf($"<h1>Thread {t} Doc {i}</h1>");
        // Measure throughput without I/O overhead
    }
}));

await Task.WhenAll(tasks);
sw.Stop();

int total = concurrency * docsPerThread;
Console.WriteLine($"Total: {total} docs in {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"Throughput: {total / sw.Elapsed.TotalSeconds:F1} docs/sec");
// Docs for parallel patterns: https://ironpdf.com/examples/parallel/
```

---

## Before You Start

### Find ExpertPdf References

```bash
# Find all ExpertPdf usages
rg "ExpertPdf" --type cs -l

# Find PdfConverter usages — main migration targets
rg "PdfConverter|PDFMerge|PdfSecurityOptions" --type cs

# Find URL rendering
rg "GetPdfBytesFromUrl|GetPdfBytesFromHtmlString|GetPdfBytesFromHtmlFile" --type cs
```

### Uninstall / Install

```bash
# Remove ExpertPdf packages (use whichever variants you have installed)
dotnet remove package ExpertPdfHtmlToPdf            # .NET Framework
dotnet remove package ExpertPdf.HtmlToPdf.NetCore   # .NET Core / 5-9
dotnet remove package ExpertPdf.MergePdf
dotnet remove package ExpertPdf.PdfSecurity
dotnet remove package ExpertPdf.SplitPdf
dotnet remove package ExpertPdf.PdfToImage
dotnet remove package ExpertPdf.PdfCreator

# Install IronPDF (includes all features)
dotnet add package IronPdf

dotnet list package
```

### License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Docs: https://ironpdf.com/how-to/license-keys/
```

---

## Quick Start Migration (3 Steps)

### Step 1: License

```csharp
// Before (ExpertPdf — per-instance)
PdfConverter pdfConverter = new PdfConverter();
pdfConverter.LicenseKey = "YOUR-LICENSE-KEY";

// After (IronPDF — global, once at startup)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 2: Namespace Imports

```csharp
// Before
using ExpertPdf.HtmlToPdf;

// After
using IronPdf;
```

### Step 3: Basic HTML → PDF

```csharp
// Before (ExpertPdf)
PdfConverter pdfConverter = new PdfConverter();
pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString("<h1>Hello</h1>");
System.IO.File.WriteAllBytes("output.pdf", pdfBytes);

// After (IronPDF)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
using var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("output.pdf");
// Docs: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| ExpertPdf | IronPDF | Notes |
|---|---|---|
| `ExpertPdf.HtmlToPdf` | `IronPdf` | Core namespace |
| `ExpertPdf.MergePdf` | `IronPdf` | Merging consolidated |
| `ExpertPdf.PdfSecurity` | `IronPdf` | Security consolidated |
| `ExpertPdf.SplitPdf` | `IronPdf` | Splitting consolidated |

### Core Class Mapping

| ExpertPdf Class | IronPDF Class | Description |
|---|---|---|
| `PdfConverter` | `ChromePdfRenderer` | Main rendering class |
| `PdfDocumentOptions` | `ChromePdfRenderOptions` (via `RenderingOptions`) | Per-render settings |
| `PdfHeaderOptions` / `PdfFooterOptions` | `HtmlHeaderFooter` | Header/footer config |
| `PDFMerge` | `PdfDocument.Merge()` | Static merge helper |
| `PdfSecurityOptions` | `pdf.SecuritySettings` | Per-document security |
| `PdfPageSize` | `IronPdf.Rendering.PdfPaperSize` | Page size enum |

### Document Loading

| Operation | ExpertPdf | IronPDF |
|---|---|---|
| Render HTML string | `pdfConverter.GetPdfBytesFromHtmlString(html)` | `renderer.RenderHtmlAsPdf(html)` |
| Render URL | `pdfConverter.GetPdfBytesFromUrl(url)` | `renderer.RenderUrlAsPdf(url)` |
| Render HTML file | `pdfConverter.GetPdfBytesFromHtmlFile(path)` | `renderer.RenderHtmlFileAsPdf(path)` |
| Save URL → file | `pdfConverter.SavePdfFromUrlToFile(url, path)` | `renderer.RenderUrlAsPdf(url).SaveAs(path)` |
| Load existing PDF | (via separate package) | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | ExpertPdf | IronPDF |
|---|---|---|
| Page size | `PdfDocumentOptions.PdfPageSize` | `RenderingOptions.PaperSize` |
| Orientation | `PdfDocumentOptions.PdfPageOrientation` | `RenderingOptions.PaperOrientation` |
| Margins | `PdfDocumentOptions.MarginTop` / `MarginBottom` / `MarginLeft` / `MarginRight` | Same property names on `RenderingOptions` |
| Custom page size | `CustomPdfPageWidth` / `CustomPdfPageHeight` (points) | `SetCustomPaperSizeInMillimeters(w, h)` |

### Header/Footer Placeholders

| ExpertPdf Token | IronPDF Placeholder |
|---|---|
| `&p;` | `{page}` |
| `&P;` | `{total-pages}` |
| `&d;` | `{date}` |
| `&t;` | `{time}` |
| `&u;` | `{url}` |

### Merge / Split

| Operation | ExpertPdf | IronPDF |
|---|---|---|
| Merge PDFs | `PDFMerge.AppendPDFFile(path)` + `SaveMergedPDFToFile(path)` | `PdfDocument.Merge(a, b)` |
| Split | `ExpertPdf.SplitPdf` package | `pdf.CopyPages(start, end)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (ExpertPdf):**

```csharp
using ExpertPdf.HtmlToPdf;
using System.IO;

class Program
{
    static void Main()
    {
        PdfConverter pdfConverter = new PdfConverter();
        pdfConverter.LicenseKey = "YOUR-LICENSE-KEY";

        string html = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; }
                    .report-header { background: #0077cc; color: white; padding: 20px; }
                    table { width: 100%; border-collapse: collapse; }
                    td, th { border: 1px solid #ccc; padding: 8px; }
                </style>
            </head>
            <body>
                <div class='report-header'><h1>Sales Report</h1></div>
                <table>
                    <tr><th>Product</th><th>Units</th><th>Revenue</th></tr>
                    <tr><td>Widget A</td><td>1,200</td><td>$24,000</td></tr>
                    <tr><td>Widget B</td><td>800</td><td>$32,000</td></tr>
                </table>
            </body>
            </html>";

        pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
        pdfConverter.PdfDocumentOptions.MarginTop = 15;
        pdfConverter.PdfDocumentOptions.MarginBottom = 15;

        byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(html);
        File.WriteAllBytes("sales_report.pdf", pdfBytes);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        // Docs: https://ironpdf.com/how-to/license-keys/

        string html = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; }
                    .report-header { background: #0077cc; color: white; padding: 20px; }
                    table { width: 100%; border-collapse: collapse; }
                    td, th { border: 1px solid #ccc; padding: 8px; }
                </style>
            </head>
            <body>
                <div class='report-header'><h1>Sales Report</h1></div>
                <table>
                    <tr><th>Product</th><th>Units</th><th>Revenue</th></tr>
                    <tr><td>Widget A</td><td>1,200</td><td>$24,000</td></tr>
                </table>
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 15;
        renderer.RenderingOptions.MarginBottom = 15;
        // Docs: https://ironpdf.com/how-to/rendering-options/

        using var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("sales_report.pdf");
    }
}
```

---

### 2. Merge PDFs

**Before (ExpertPdf — requires the separate `ExpertPdf.MergePdf` package):**

```csharp
using ExpertPdf.MergePdf;
using ExpertPdf.HtmlToPdf;

class MergeSample
{
    static void Main()
    {
        PdfDocumentOptions options = new PdfDocumentOptions();
        options.PdfPageSize = PdfPageSize.A4;

        PDFMerge pdfMerge = new PDFMerge(options);
        pdfMerge.AppendPDFFile("doc_a.pdf");
        pdfMerge.AppendPDFFile("doc_b.pdf");
        pdfMerge.SaveMergedPDFToFile("merged.pdf");
    }
}
```

**After (IronPDF — merging is part of the main package):**

```csharp
using IronPdf;

class MergeSample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var docA = PdfDocument.FromFile("doc_a.pdf");
        using var docB = PdfDocument.FromFile("doc_b.pdf");

        using var merged = PdfDocument.Merge(docA, docB);
        merged.SaveAs("merged.pdf");
        // Docs: https://ironpdf.com/how-to/merge-or-split-pdfs/
    }
}
```

---

### 3. Watermark

**Before (ExpertPdf — watermarks are typically applied via header HTML overlay):**

```csharp
using ExpertPdf.HtmlToPdf;

class WatermarkSample
{
    static void Main()
    {
        PdfConverter pdfConverter = new PdfConverter();
        pdfConverter.LicenseKey = "YOUR-LICENSE-KEY";

        // Header HTML used as a translucent overlay across pages
        pdfConverter.PdfHeaderOptions.ShowHeader = true;
        pdfConverter.PdfHeaderOptions.HeaderText = "CONFIDENTIAL";
        pdfConverter.PdfHeaderOptions.HeaderTextAlignment = HorizontalTextAlign.Center;

        byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString("<p>Document</p>");
        System.IO.File.WriteAllBytes("watermarked.pdf", pdfBytes);
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;
using IronPdf.Editing;

class WatermarkSample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        var stamper = new TextStamper
        {
            Text = "CONFIDENTIAL",
            FontSize = 45,
            Opacity = 30,
            Rotation = 45,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        pdf.ApplyStamp(stamper); // all pages
        pdf.SaveAs("watermarked.pdf");
        // Docs: https://ironpdf.com/how-to/stamp-text-image/
    }
}
```

---

### 4. Password Protection

**Before (ExpertPdf — uses `PdfSecurityOptions`):**

```csharp
using ExpertPdf.HtmlToPdf;

class SecuritySample
{
    static void Main()
    {
        PdfConverter pdfConverter = new PdfConverter();
        pdfConverter.LicenseKey = "YOUR-LICENSE-KEY";

        pdfConverter.PdfSecurityOptions.UserPassword = "user123";
        pdfConverter.PdfSecurityOptions.OwnerPassword = "owner456";
        pdfConverter.PdfSecurityOptions.CanPrint = true;
        pdfConverter.PdfSecurityOptions.CanCopyContent = false;

        byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString("<p>Secure doc</p>");
        System.IO.File.WriteAllBytes("secured.pdf", pdfBytes);
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;
using IronPdf.Security;

class SecuritySample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        // Docs: https://ironpdf.com/how-to/pdf-permissions-passwords/
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;

        pdf.SaveAs("secured.pdf");
    }
}
```

---

## Critical Migration Notes

### Bytes vs. Document Object
ExpertPdf's converter pattern returns `byte[]` directly. IronPDF returns a `PdfDocument` object from which you can get `BinaryData` (bytes), save to file, or return as a stream. Update callers that expect raw bytes:

```csharp
// Old pattern — byte[] from ExpertPdf
// byte[] pdf = pdfConverter.GetPdfBytesFromHtmlString(html);
// return File(pdf, "application/pdf");

// New pattern — extract BinaryData from PdfDocument
using var pdf = renderer.RenderHtmlAsPdf(html);
return File(pdf.BinaryData, "application/pdf", "document.pdf");
// PdfDocument is disposed after BinaryData is copied
```

### Page Size Enums
ExpertPdf uses `PdfPageSize` (e.g., `PdfPageSize.A4`, `PdfPageSize.Letter`). IronPDF uses `IronPdf.Rendering.PdfPaperSize`. Map common sizes explicitly rather than assuming enum names match.

### Custom Page Size Units
ExpertPdf custom page sizes are in points (`CustomPdfPageWidth = 432; CustomPdfPageHeight = 288;` for 6"×4"). IronPDF uses millimeters: `renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(152.4, 101.6);`. The conversion is `points / 72 * 25.4 = millimeters`.

### Header/Footer HTML
ExpertPdf uses text-based `PdfHeaderOptions.HeaderText` plus the `&p;` / `&P;` page tokens. IronPDF replaces both with an `HtmlHeaderFooter` object containing an `HtmlFragment` and `{page}` / `{total-pages}` placeholders. The migration is mechanical but the placeholder syntax has to be converted.

---

## Performance Considerations

### Reuse `ChromePdfRenderer`

```csharp
// Singleton for ASP.NET / worker services
public class DocumentService
{
    // One renderer per application — not per request
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    public async Task<byte[]> GenerateAsync(string html)
    {
        using var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
        // Async docs: https://ironpdf.com/how-to/async/
    }
}
```

### Cold Start Optimization

```csharp
// Warm up in IHostedService or Startup
public async Task StartAsync(CancellationToken _)
{
    IronPdf.License.LicenseKey = config["IronPdf:LicenseKey"];
    using var warmup = await _renderer.RenderHtmlAsPdfAsync("<p>init</p>");
    _logger.LogInformation("IronPDF renderer warmed up");
}
```

### Memory Profiling After Migration

After migrating, profile memory for at least 24 hours under production load. Key metrics:
- Gen 2 GC frequency (large objects from PDF bytes)
- Working set trend (should be stable, not growing)
- Thread pool queue depth under burst load

If using `MemoryStream` for output, ensure streams are disposed after the response is sent.

---

## Migration Checklist

### Pre-Migration
- [ ] Inventory all `ExpertPdf` usages: `rg "ExpertPdf" --type cs -l`
- [ ] List all methods that return `byte[]` from ExpertPdf — callers need updating
- [ ] Note which packages are in use: `ExpertPdfHtmlToPdf` / `ExpertPdf.HtmlToPdf.NetCore`, `ExpertPdf.MergePdf`, `ExpertPdf.PdfSecurity`, `ExpertPdf.SplitPdf`, `ExpertPdf.PdfToImage`, `ExpertPdf.PdfCreator`
- [ ] Confirm target .NET version is supported by IronPDF
- [ ] Set up IronPDF license configuration in all environments
- [ ] Test IronPDF Linux dependencies on your container base image
- [ ] Run baseline render benchmark (timing + memory) with ExpertPdf
- [ ] Capture visual screenshots of key templates before migration

### Code Migration
- [ ] Replace `using ExpertPdf.HtmlToPdf` with `using IronPdf`
- [ ] Replace `new PdfConverter()` with `new ChromePdfRenderer()`
- [ ] Replace `GetPdfBytesFromHtmlString()` → `renderer.RenderHtmlAsPdf(html).BinaryData`
- [ ] Replace `GetPdfBytesFromUrl()` → `renderer.RenderUrlAsPdf(url).BinaryData`
- [ ] Replace `GetPdfBytesFromHtmlFile()` → `renderer.RenderHtmlFileAsPdf(path).BinaryData`
- [ ] Map `PdfPageSize` → `IronPdf.Rendering.PdfPaperSize`
- [ ] Convert custom page sizes from points to millimeters
- [ ] Replace `PdfSecurityOptions` with `pdf.SecuritySettings`
- [ ] Convert text headers/footers to `HtmlHeaderFooter` with `{page}` / `{total-pages}` placeholders
- [ ] Replace `PDFMerge` with `PdfDocument.Merge()`
- [ ] Add `using` disposal to all `PdfDocument` instances

### Testing
- [ ] Visual diff screenshots for all templates (before vs. after)
- [ ] Specifically test CSS Grid / Flexbox templates
- [ ] Test `@font-face` custom font rendering
- [ ] Test JavaScript-rendered content (new capability)
- [ ] Run throughput benchmark — compare to ExpertPdf baseline
- [ ] Memory profiling under 30-min sustained load
- [ ] Test password protection with PDF reader
- [ ] Test merge output page counts

### Post-Migration
- [ ] Remove `ExpertPdf.*` packages
- [ ] Update Dockerfile if Linux deps changed
- [ ] Archive ExpertPdf license for records
- [ ] Document throughput delta (up or down) for future reference

---

## Next Steps

The ExpertPdf migration is relatively mechanical — both libraries are HTML-first converters, so the conceptual model transfers. The trickiest parts are the options object property mapping, the points-to-millimeters conversion for custom page sizes, and any callers expecting `byte[]` directly from the conversion call rather than a `PdfDocument` object.

The performance story is worth measuring rather than assuming. First-render latency with IronPDF will be higher due to Chromium startup, but sustained throughput and memory behavior under concurrent load are what matter for server workloads. Run the benchmarks before making claims.

**Technical question for comments:** For teams running batch PDF generation at scale — what concurrency model have you found works best with Chromium-based renderers? Single renderer with parallel calls, multiple renderer instances, or renderer pool? Real production numbers welcome.
