# Dropping QuestPDF for IronPDF: a .NET migration that fits in an afternoon

It works on your machine. It generates the PDF exactly right. Then you push it through the CI pipeline — a Docker container running a minimal base image — and the font rendering breaks, or the runner exits with a native library error. QuestPDF has made real progress on cross-platform support, but teams on minimal Linux images occasionally still hit SkiaSharp or font provider issues that don't reproduce locally. That gap between local and pipeline behavior is the pain that typically triggers an evaluation.

This article covers migrating from QuestPDF to IronPDF. You'll have benchmark-comparable before/after code for the standard operations, plus specific notes on the most common CI/container failure modes.

---

## Why Migrate (Without Drama)

Teams evaluating alternatives to QuestPDF commonly encounter these conditions:

1. **SkiaSharp native library issues in containers** — `libfontconfig`, `libfreetype`, or `libskia` not present in minimal Docker base images (alpine, distroless).
2. **Font provider configuration** — QuestPDF requires a font provider setup; on CI runners without system fonts, this needs explicit configuration.
3. **Document model vs HTML input** — QuestPDF's fluent DSL requires rewriting every document from scratch; HTML templates from the frontend can't be reused directly.
4. **No HTML rendering path** — web developers maintaining templates in HTML can't directly use QuestPDF without rewriting to the DSL.
5. **DSL maintenance overhead** — complex layouts in QuestPDF's fluent API can become verbose and harder to maintain than equivalent HTML/CSS.
6. **Compile-time coupling** — every layout change requires a code change; HTML templates allow non-developer modification.
7. **Limited PDF manipulation** — QuestPDF is a generation library; merge, split, watermark, security, and text extraction need additional libraries.
8. **Version upgrade breaking changes** — QuestPDF's API has had changes between major versions that require document model rewrites.
9. **CI environment divergence** — SkiaSharp native bindings behave differently on various CI runner configurations.
10. **Testing complexity** — visual regression testing for fluent API documents requires more infrastructure than HTML preview.

### Comparison Table

| Aspect | QuestPDF | IronPDF |
|---|---|---|
| Focus | Programmatic PDF via fluent DSL | HTML-to-PDF + PDF manipulation |
| Pricing | MIT (QuestPDF Community) — verify commercial terms for high revenue | Commercial license — verify at ironsoftware.com |
| API Style | Fluent document model / layout DSL | HTML renderer + PDF manipulation objects |
| Learning Curve | Medium; DSL has its own concepts (containers, columns, etc.) | Low for web devs; HTML/CSS is the input |
| HTML Rendering | Not supported — DSL only | Embedded Chromium |
| Page Indexing | N/A — generation only | 0-based |
| Thread Safety | Verify in QuestPDF docs | Verify IronPDF concurrent instance guidance |
| Namespace | `QuestPDF.Fluent`, `QuestPDF.Infrastructure` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | QuestPDF | IronPDF Equivalent | Complexity |
|---|---|---|---|
| Simple document with text | Fluent DSL | HTML string | Low (rewrite as HTML) |
| Table layout | `.Table()` fluent API | HTML `<table>` | Low–Medium |
| Images | `.Image()` component | `<img>` in HTML | Low |
| Custom fonts | Font provider config | CSS `@font-face` | Medium |
| Headers/footers | `.Header()` / `.Footer()` components | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Page numbers | Via footer component | `{page}` token in header/footer | Medium |
| Multi-section document | Page model composition | Multiple HTML sections | Medium |
| Merge PDFs | Not native | `PdfDocument.Merge()` | Medium |
| Watermark | Not native (custom component) | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Not native | `pdf.SecuritySettings` | Medium |
| Dynamic data binding | C# lambda expressions in DSL | HTML template strings / Razor | Medium |
| SkiaSharp native deps | Required | Not required (Chromium) | Low (remove) |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Documents already designed as HTML/CSS templates | Switch — direct HTML input eliminates DSL rewrite |
| Minimal Docker images (alpine, distroless) with SkiaSharp issues | Switch — IronPDF avoids SkiaSharp native dependency |
| Complex programmatic layouts where DSL is working well | QuestPDF may be appropriate — evaluate migration cost |
| Documents need HTML-to-PDF from existing web content | Switch — IronPDF accepts live HTML/URLs directly |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All QuestPDF References

```bash
# Find QuestPDF usage
rg -l "QuestPDF\|Document\.Create\|\.Compose\|IDocument" --type cs
rg "QuestPDF\|\.Column\(.*\.Item\|\.Table\(" --type cs -n

# Find SkiaSharp references (sometimes used directly alongside QuestPDF)
rg "SkiaSharp\|SKColor\|SKFont\|SKBitmap" --type cs -n

# Find font provider setup
rg "FontManager\|FontResolver\|AddFont\|RegisterFont" --type cs -n

# Check project files
grep -r "QuestPDF\|SkiaSharp" *.csproj **/*.csproj 2>/dev/null
```

### Uninstall / Install

```bash
# Remove QuestPDF (and SkiaSharp if only used by QuestPDF)
dotnet remove package QuestPDF

# Check if SkiaSharp can be removed too
dotnet remove package SkiaSharp  # only if no other dependency requires it

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

Also clean up Docker if you added SkiaSharp system libraries:

```dockerfile
# Remove SkiaSharp system dependencies if they were only for QuestPDF:
# RUN apt-get install -y libfontconfig1 libfreetype6 libgdiplus

# IronPDF: no SkiaSharp; verify IronPDF's own system requirements in their Docker docs
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
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic Document

**Before (QuestPDF fluent DSL):**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.Content().Text("Hello, PDF!").FontSize(24);
    });
}).GeneratePdf("output.pdf");

Console.WriteLine("Saved output.pdf");
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
// 2cm margins — verify unit convention in IronPDF docs
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;

var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1 style='font-size:24px'>Hello, PDF!</h1></body></html>"
);
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Benchmark Reference Patterns

> These are measurement structures, not performance claims. Run against your actual document complexity in your environment. QuestPDF render time is CPU-bound (SkiaSharp). IronPDF render time includes Chromium layout. Measure both under your realistic workload.

### Single-Document Render Time

```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, sans-serif; padding: 2cm; }
        h1 { font-size: 20px; }
        table { width: 100%; border-collapse: collapse; }
        td, th { border: 1px solid #ddd; padding: 6px; font-size: 11px; }
    </style>
    </head>
    <body>
        <h1>Quarterly Report</h1>
        <table>
            <tr><th>Region</th><th>Q3 Revenue</th><th>YoY Change</th></tr>
            <tr><td>North America</td><td>$12.4M</td><td>+8.2%</td></tr>
            <tr><td>Europe</td><td>$9.1M</td><td>+3.7%</td></tr>
            <tr><td>APAC</td><td>$6.8M</td><td>+14.3%</td></tr>
        </table>
    </body>
    </html>";

async Task<(double avg, double p95, double min, double max)> BenchmarkRenders(int runs = 25)
{
    var renderer = new ChromePdfRenderer();
    using var _ = await renderer.RenderHtmlAsPdfAsync(html); // warm-up

    var times = new List<double>();
    for (int i = 0; i < runs; i++)
    {
        var sw = Stopwatch.StartNew();
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        sw.Stop();
        times.Add(sw.Elapsed.TotalMilliseconds);
    }

    times.Sort();
    return (times.Average(), times[(int)(times.Count * 0.95)], times.First(), times.Last());
}

var stats = await BenchmarkRenders(25);
Console.WriteLine($"IronPDF — Avg: {stats.avg:F1}ms | P95: {stats.p95:F1}ms | Min: {stats.min:F1}ms | Max: {stats.max:F1}ms");

// QuestPDF benchmark structure (run separately in their API):
// var sw = Stopwatch.StartNew();
// Document.Create(...).GeneratePdf("output.pdf");
// Console.WriteLine($"QuestPDF: {sw.Elapsed.TotalMilliseconds:F1}ms");
```

### Memory Allocation Under Load

```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

static async Task MeasureMemoryFootprint(int iterations = 50)
{
    var renderer = new ChromePdfRenderer();
    var html = "<html><body><h1>Memory test document</h1></body></html>";

    var before = GC.GetTotalMemory(forceFullCollection: true);

    for (int i = 0; i < iterations; i++)
    {
        // 'using' ensures disposal on each iteration
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        // pdf disposed here
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    var after = GC.GetTotalMemory(forceFullCollection: true);

    Console.WriteLine($"Memory delta after {iterations} renders + GC: {(after - before) / 1024:F1} KB");
}

await MeasureMemoryFootprint(50);
```

### Concurrent Throughput

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/examples/parallel/
async Task BenchmarkConcurrency(int degree)
{
    var htmlJobs = Enumerable.Range(1, degree)
        .Select(i => $"<html><body><h1>Document {i}</h1><p>Content</p></body></html>")
        .ToArray();

    var sw = Stopwatch.StartNew();

    var results = await Task.WhenAll(htmlJobs.Select(async html =>
    {
        var renderer = new ChromePdfRenderer();
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.PageCount;
    }));

    sw.Stop();
    Console.WriteLine($"Degree {degree}: {sw.Elapsed.TotalMilliseconds:F0}ms | {sw.Elapsed.TotalMilliseconds / degree:F1}ms avg");
}

await BenchmarkConcurrency(5);
await BenchmarkConcurrency(10);
await BenchmarkConcurrency(20);
// See: https://ironpdf.com/how-to/async/
```

---

## API Mapping Tables

### Namespace Mapping

| QuestPDF | IronPDF | Notes |
|---|---|---|
| `QuestPDF.Fluent` | `IronPdf` | Core namespace |
| `QuestPDF.Infrastructure` | `IronPdf.Rendering` | Config types |
| `QuestPDF.Helpers` (PageSizes, etc.) | `IronPdf.Rendering.PdfPaperSize` | Paper size enum |

### Core Class Mapping

| QuestPDF Concept | IronPDF Class | Description |
|---|---|---|
| `Document.Create()` fluent builder | `ChromePdfRenderer` | Replace DSL builder with HTML + renderer |
| `IDocument` / page component | `PdfDocument` | PDF object returned by renderer |
| `PageSizes.A4` etc. | `PdfPaperSize.A4` etc. | Paper size constants |
| Font provider / `FontManager` | CSS `@font-face` | Fonts declared in HTML |

### Document Loading Methods

| Operation | QuestPDF | IronPDF |
|---|---|---|
| Generate from DSL | `Document.Create(...).GeneratePdf()` | `renderer.RenderHtmlAsPdfAsync(html)` |
| Generate from URL | Not supported | `renderer.RenderUrlAsPdfAsync(url)` |
| Load existing PDF | Not supported | `PdfDocument.FromFile(path)` |
| Save to stream | `.GeneratePdf(stream)` | `pdf.Stream` |

### Page Operations

| Operation | QuestPDF | IronPDF |
|---|---|---|
| Page count | Via page callbacks | `pdf.PageCount` |
| Remove page | N/A — generation only | `pdf.RemovePage(index)` — verify |
| Extract text | N/A | `pdf.ExtractAllText()` |
| Rotate | N/A | Verify in IronPDF docs |

### Merge / Split Operations

| Operation | QuestPDF | IronPDF |
|---|---|---|
| Merge | Not native | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not native | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF (from DSL to HTML-driven)

**Before (QuestPDF fluent DSL):**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;

class InvoiceBefore
{
    static void Main()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header()
                    .Text("Invoice #5001")
                    .SemiBold()
                    .FontSize(24);

                page.Content()
                    .Column(col =>
                    {
                        col.Item().Text("Client: Acme Corp").FontSize(12);
                        col.Item().PaddingTop(10).Text("Amount Due: $3,200").FontSize(12);
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x => x.CurrentPageNumber());
            });
        }).GeneratePdf("invoice.pdf");

        Console.WriteLine("Saved invoice.pdf");
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
        body { font-family: Arial, sans-serif; padding: 2cm; }
        h1 { font-size: 24px; font-weight: 600; }
        p { font-size: 12px; margin: 4px 0; }
    </style>
    </head>
    <body>
        <h1>Invoice #5001</h1>
        <p>Client: Acme Corp</p>
        <p>Amount Due: $3,200</p>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

// Page numbers via footer: https://ironpdf.com/how-to/headers-and-footers/
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center; font-size:10px'>{page}</div>",
};

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");

Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (QuestPDF — no native merge):**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

class MergeBefore
{
    static void Main()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        // QuestPDF doesn't support merging separate documents.
        // Teams typically generate each section as a separate file
        // then use a secondary library to combine them.

        var sections = new[]
        {
            ("Section 1: Overview", "sec1.pdf"),
            ("Section 2: Detail", "sec2.pdf"),
        };

        foreach (var (title, filename) in sections)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Content().Text(title).FontSize(20);
                });
            }).GeneratePdf(filename);
        }

        // Now merge sec1.pdf + sec2.pdf using secondary library (e.g., PDFsharp)
        Console.WriteLine("Merge requires secondary library — not built into QuestPDF");
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
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 1: Overview</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 2: Detail</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("combined.pdf");

Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (QuestPDF — custom layer component):**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using SkiaSharp;
using System;

class WatermarkBefore
{
    static void Main()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        // QuestPDF watermark via custom Canvas element — requires SkiaSharp directly
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);

                // Foreground layer for watermark
                page.Foreground().Canvas((canvas, size) =>
                {
                    using var paint = new SKPaint
                    {
                        Color = SKColors.Gray.WithAlpha(50),
                        TextSize = 72,
                        IsAntialias = true,
                    };
                    // Manual rotation and positioning required
                    canvas.Save();
                    canvas.Translate(size.Width / 2, size.Height / 2);
                    canvas.RotateDegrees(-45);
                    canvas.DrawText("DRAFT", 0, 0, paint);
                    canvas.Restore();
                });

                page.Content().Text("Report Content").FontSize(14);
            });
        }).GeneratePdf("watermarked.pdf");

        Console.WriteLine("SkiaSharp watermark applied");
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
    "<html><body><h1>Report Content</h1></body></html>"
);

// No SkiaSharp required — programmatic text stamp
// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronPdf.Imaging.Color.Gray,
    Opacity = 0.2,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (QuestPDF — not supported; secondary library required):**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

class PasswordBefore
{
    static void Main()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        // Step 1: Generate PDF with QuestPDF
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Content().Text("Confidential Document").FontSize(18);
            });
        }).GeneratePdf();

        // Step 2: Apply password with a secondary library (required)
        // QuestPDF has no security/password API
        // Pseudo-code:
        // var secured = SomePdfLib.SetPassword(pdfBytes, "open123", "admin456");
        // File.WriteAllBytes("secured.pdf", secured);

        Console.WriteLine("Password protection requires a secondary library with QuestPDF");
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
    "<html><body><h1>Confidential Document</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### DSL to HTML Rewrite

The fundamental migration challenge with QuestPDF is that its fluent DSL doesn't map 1:1 to HTML. There's no automated converter. Every document needs to be rewritten as HTML/CSS.

Assess your document portfolio before starting:

```bash
# Count QuestPDF document definitions
rg "Document\.Create\|\.Page(" --type cs | wc -l

# Identify unique document types
rg -l "Document\.Create" --type cs
```

The rule of thumb: simple single-column documents (text, basic tables, logos) take ~30 minutes each to rewrite as HTML. Complex multi-column layouts with precise spacing may take longer.

### SkiaSharp System Library Removal

After removing QuestPDF, verify that SkiaSharp packages can be removed:

```bash
dotnet list package | grep -i "skia"
# If SkiaSharp appears only as a QuestPDF transitive dependency, it can be removed
```

In Dockerfiles, remove any SkiaSharp system library installs:

```dockerfile
# Remove if only needed for QuestPDF/SkiaSharp:
# RUN apt-get install -y libfontconfig1 libfreetype6 libgdiplus libssl-dev

# Verify IronPDF's own system requirements for your base image
```

### Page Number Migration

QuestPDF's page numbering uses `x.CurrentPageNumber()` in the document model. IronPDF uses placeholder tokens in HtmlHeaderFooter:

```csharp
// QuestPDF: page.Footer().Text(x => x.CurrentPageNumber())
// IronPDF equivalent via HtmlFooter:
// https://ironpdf.com/how-to/headers-and-footers/
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='text-align:center; font-size:9px; color:#666'>
            Page {page} of {total-pages}
        </div>",
};
// Verify exact token names ({page}, {total-pages}) in current IronPDF docs
```

### QuestPDF License Setting

QuestPDF requires a license declaration at startup: `QuestPDF.Settings.License = LicenseType.Community`. Remove this and replace with the IronPDF license key:

```bash
# Find and remove QuestPDF license setting
rg "QuestPDF\.Settings\.License\|LicenseType\." --type cs -n
```

---

## Performance Considerations

### Render Characteristics Differ

QuestPDF renders via SkiaSharp — a vector graphics API. IronPDF renders via Chromium layout. The performance profile is different:
- QuestPDF: CPU-bound SkiaSharp rasterization; no HTML parse or layout pass
- IronPDF: Chromium HTML parse + layout + render; includes JS execution if present

For simple text documents, QuestPDF may be faster. For complex HTML-derived content, IronPDF avoids the DSL rewrite overhead. Measure for your specific templates.

### Renderer Reuse

```csharp
using IronPdf;
using System.Threading.Tasks;

// Reuse renderer across document renders
// Verify thread-safety guidance in current IronPDF docs
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

// Render multiple docs with same renderer config
foreach (var htmlJob in htmlJobs)
{
    using var pdf = await renderer.RenderHtmlAsPdfAsync(htmlJob);
    pdf.SaveAs(/* output path */);
}
```

### Parallel Rendering Pattern

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

// https://ironpdf.com/examples/parallel/
var pdfs = await Task.WhenAll(htmlJobs.Select(async html =>
{
    var renderer = new ChromePdfRenderer(); // per-task instance
    return await renderer.RenderHtmlAsPdfAsync(html);
}));

foreach (var pdf in pdfs) pdf.Dispose();
```

---

## Migration Checklist

### Pre-Migration
- [ ] Count QuestPDF document definitions (`rg "Document\.Create" --type cs | wc -l`)
- [ ] Categorize documents by complexity (simple text, tables, multi-column, custom fonts)
- [ ] Identify secondary libraries used to supplement QuestPDF (merge, security)
- [ ] Check if SkiaSharp system libraries are in Dockerfiles (candidates for removal)
- [ ] Document current render times per document type for comparison
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `QuestPDF` package reference
- [ ] Remove `SkiaSharp` packages if only used by QuestPDF
- [ ] Remove `QuestPDF.Settings.License = LicenseType.*` call
- [ ] Add IronPDF license key at application startup
- [ ] Rewrite each `Document.Create()` block as HTML/CSS (assess per-document)
- [ ] Migrate `.Header()` / `.Footer()` components to `RenderingOptions.HtmlHeader/Footer`
- [ ] Replace SkiaSharp canvas watermarks with `TextStamper` / `ImageStamper`
- [ ] Replace secondary merge library with `PdfDocument.Merge()`
- [ ] Replace secondary security library with `pdf.SecuritySettings`

### Testing
- [ ] Render each migrated document and compare visual output
- [ ] Verify page numbers render correctly via HtmlFooter tokens
- [ ] Verify custom fonts load (CSS `@font-face` or CDN links)
- [ ] Test password protection
- [ ] Test merge output page count and order
- [ ] Verify Docker build succeeds after SkiaSharp removal
- [ ] Benchmark render time vs QuestPDF baseline per document type

### Post-Migration
- [ ] Remove `QuestPDF` and `SkiaSharp` NuGet packages
- [ ] Remove SkiaSharp system library installs from Dockerfiles if applicable
- [ ] Remove secondary PDF manipulation libraries now replaced by IronPDF
- [ ] Update CI/CD environment variable documentation

---

## Wrapping Up

The document rewrite (DSL to HTML) is the main cost in this migration, and it scales linearly with the number of distinct document types in your codebase. Running the `rg "Document.Create"` count before committing to a timeline is essential.

The operational wins — removing SkiaSharp native library dependencies, eliminating font provider config, cleaning up Docker image system libraries — are immediate and verifiable before any code is written.

**Discussion question:** What version of QuestPDF are you migrating from, and did anything break unexpectedly — particularly around font handling in containers or page layout behavior?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
