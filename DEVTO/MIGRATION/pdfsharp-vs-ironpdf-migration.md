---
title: "PDFSharp to IronPDF: an honest migration walkthrough"
published: false
tags: dotnet, csharp, pdf, migration
---

License renewal comes up and the question gets asked that usually stays unasked: is this still the right tool for the job? PDFSharp is a solid open-source library for PDF construction and manipulation. It does what it does well. But teams evaluating options at renewal time often discover that what they actually need has shifted — less programmatic PDF construction, more reliable HTML-to-PDF rendering. And those are two very different problem domains.

This article covers what changes when you migrate from PDFSharp to IronPDF, what benchmarks matter, and where the migration will cost you time versus where it's a direct swap.

---

## Why Migrate (Without Drama)

PDFSharp and IronPDF serve partially overlapping but distinct use cases. Teams typically evaluate switching when:

1. **HTML-to-PDF is the primary use case** — PDFSharp doesn't render HTML; you'd need to add another library for that.
2. **Template-driven documents** — maintaining a PDFSharp drawing-API-based template is more work than maintaining an HTML template.
3. **CSS-controlled layout** — CSS is far better understood than PDFSharp's manual coordinate system for most web developers.
4. **License model review** — PDFSharp is free/MIT; evaluating commercial alternatives is a cost-vs-features calculation. See [IronPDF licensing](https://ironpdf.com/licensing/) for current pricing.
5. **Maintenance overhead** — PDF rendering code written against PDFSharp's drawing API tends to be fragile under layout changes.
6. **Dynamic content** — content driven by a CMS or data model is easier to express as HTML templates than as drawing API calls.
7. **Multi-developer maintenance** — HTML/CSS is a broader skill set than PDFSharp's API.
8. **Cross-platform rendering consistency** — PDFSharp's font handling depends on installed system fonts, which can produce different output across .NET versions and platforms.
9. **Missing features** — PDFSharp focuses on PDF creation/manipulation, not annotation workflows, form filling, or digital signatures.
10. **Build system simplification** — if a separate HTML renderer was added alongside PDFSharp, consolidating into one library may reduce complexity.

### Comparison Table

| Aspect | PDFSharp | IronPDF |
|---|---|---|
| Focus | Programmatic PDF construction + manipulation | HTML-to-PDF rendering + PDF manipulation |
| Pricing | Open source (MIT) | Commercial — see [ironpdf.com/licensing](https://ironpdf.com/licensing/) |
| API Style | Drawing API — GfxPath, XGraphics, XFont, coordinates | HTML renderer + document object API |
| Learning Curve | Medium; requires understanding PDF coordinate system | Low for web developers; HTML/CSS is the input |
| HTML Rendering | Not built-in; requires separate library | Embedded Chromium |
| Page Indexing | 0-based | 0-based |
| Thread Safety | Not thread-safe; create instances per thread | Renderer instances should be created per task |
| Namespace | `PdfSharp`, `PdfSharp.Drawing`, `PdfSharp.Pdf` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | PDFSharp | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | Not built-in — requires paired library | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low (if replacing HTML renderer) |
| URL to PDF | Not built-in | `ChromePdfRenderer.RenderUrlAsPdfAsync()` | Low |
| Programmatic text placement | `XGraphics.DrawString()` | Not a direct equivalent — use HTML | High |
| Programmatic shapes/lines | `XGraphics.DrawLine()` etc. | Not a direct equivalent — use HTML/CSS | High |
| Load existing PDF | `PdfDocument.Open()` | `PdfDocument.FromFile()` | Low |
| Save to file | `doc.Save(path)` | `pdf.SaveAs(path)` | Low |
| Merge PDFs | `PdfDocument` page copying | `PdfDocument.Merge()` | Medium |
| Watermark | Custom drawing code | `TextStamper` / `ImageStamper` | Medium |
| Password protection | `doc.SecuritySettings.OwnerPassword` | `pdf.SecuritySettings.OwnerPassword` | Low |
| Form fields | Limited form support | `pdf.Form` field accessors | Medium-High |
| Extract text | Page-level text extraction | `pdf.ExtractAllText()` | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Codebase is primarily drawing-API PDF construction (charts, forms, custom layouts) | PDFSharp is appropriate for this — migration would be high effort for unclear gain |
| Templates are HTML/CSS and HTML-to-PDF is the primary need | Switch — IronPDF is designed for this use case |
| Mix of HTML-to-PDF and existing PDF manipulation | Evaluate by feature — IronPDF covers both, but drawing API replacement requires template rewrite |
| Using PDFSharp only for merge/split/security on PDFs generated elsewhere | Evaluate — IronPDF supports this, but PDFSharp (being free) may be sufficient for just this use |

---

## Before You Start

### Prerequisites

- .NET 6, 7, 8, or 9 supported by IronPDF
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)
- Baseline render time measurements from your current PDFSharp workflow

### Find All PDFSharp References

```bash
# Find all PDFSharp usages
rg -l "PdfSharp\|MigraDoc\|XGraphics\|XFont" --type cs
rg "using PdfSharp\|using MigraDoc" --type cs -n

# Check project files
grep -r "PDFsharp\|PdfSharp\|MigraDoc" *.csproj **/*.csproj 2>/dev/null

# Count usage density to estimate migration scope
rg "PdfSharp\." --type cs | wc -l
rg "XGraphics\." --type cs | wc -l
```

### Uninstall / Install

```bash
# Remove PDFsharp packages — official package IDs from PDFsharp-Team
dotnet remove package PDFsharp
# dotnet remove package PDFsharp-WPF        # WPF build
# dotnet remove package PDFsharp-GDI        # GDI build
# dotnet remove package PDFsharp-MigraDoc   # MigraDoc on top
# dotnet remove package PdfSharpCore        # community .NET Standard port

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using MigraDoc.DocumentObjectModel;   // if used
using MigraDoc.Rendering;             // if used
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic Conversion

**Before (PDFSharp with HTML renderer — illustrative pattern):**
```csharp
using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

class Program
{
    static void Main()
    {
        // PDFSharp: programmatic PDF construction
        // HTML-to-PDF was NOT built into PDFSharp
        // Teams typically used a separate HTML renderer or MigraDoc

        using var document = new PdfDocument();
        document.Info.Title = "Sample Report";

        var page = document.AddPage();
        page.Size = PdfSharp.PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);
        var font = new XFont("Arial", 20, XFontStyleEx.Bold);

        // Manual coordinate placement — fragile under content changes
        gfx.DrawString("Report Title", font, XBrushes.Black,
            new XRect(0, 50, page.Width, 40),
            XStringFormats.Center);

        var bodyFont = new XFont("Arial", 12);
        gfx.DrawString("Content paragraph text here.", bodyFont, XBrushes.Black,
            new XPoint(50, 120));

        document.Save("output.pdf");
        Console.WriteLine("Saved output.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// HTML input instead of drawing API calls
var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, sans-serif; margin: 50px; }
        h1 { text-align: center; font-size: 20px; }
        p { font-size: 12px; }
    </style>
    </head>
    <body>
        <h1>Report Title</h1>
        <p>Content paragraph text here.</p>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Benchmark Patterns

> These benchmark structures are for measuring your own workloads. No performance figures are stated — measure against your actual templates in your actual environment.

### Single-Document Render Time

```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Your actual HTML template — the simpler the HTML, the faster the render
var html = @"
    <html>
    <body>
        <h1>Benchmark Document</h1>
        <table>
            <tr><th>Item</th><th>Value</th></tr>
            <tr><td>Row 1</td><td>Data</td></tr>
            <tr><td>Row 2</td><td>Data</td></tr>
        </table>
    </body>
    </html>";

async Task<(double avgMs, double p95Ms, double minMs, double maxMs)>
    BenchmarkRenders(int iterations = 30)
{
    var renderer = new ChromePdfRenderer();

    // Warm up — Chromium initialization on first render
    await renderer.RenderHtmlAsPdfAsync(html);

    var times = new List<double>();
    for (int i = 0; i < iterations; i++)
    {
        var sw = Stopwatch.StartNew();
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        sw.Stop();
        times.Add(sw.Elapsed.TotalMilliseconds);
    }

    times.Sort();
    return (
        avgMs: times.Average(),
        p95Ms: times[(int)(times.Count * 0.95)],
        minMs: times.First(),
        maxMs: times.Last()
    );
}

var stats = await BenchmarkRenders(30);
Console.WriteLine($"Avg: {stats.avgMs:F1}ms | P95: {stats.p95Ms:F1}ms | Min: {stats.minMs:F1}ms | Max: {stats.maxMs:F1}ms");

// Compare these against your PDFSharp workflow:
// - PDFSharp drawing-API generation time
// - Plus any HTML renderer time if you were using one alongside PDFSharp
```

### Merge Throughput

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

async Task BenchmarkMerge(int documentCount = 10)
{
    var renderer = new ChromePdfRenderer();
    var sw = Stopwatch.StartNew();

    // Generate test documents
    var pdfs = await Task.WhenAll(
        Enumerable.Range(1, documentCount)
            .Select(i => renderer.RenderHtmlAsPdfAsync($"<html><body><h1>Document {i}</h1></body></html>"))
    );
    
    var renderTime = sw.Elapsed.TotalMilliseconds;

    // Measure merge time separately
    sw.Restart();
    var merged = PdfDocument.Merge(pdfs);
    var mergeTime = sw.Elapsed.TotalMilliseconds;

    Console.WriteLine($"Render {documentCount} docs: {renderTime:F0}ms");
    Console.WriteLine($"Merge {documentCount} docs: {mergeTime:F0}ms");
    Console.WriteLine($"Total pages: {merged.PageCount}");

    // Cleanup
    merged.Dispose();
    foreach (var pdf in pdfs) pdf.Dispose();
}

await BenchmarkMerge(10);
await BenchmarkMerge(50);
// https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

## API Mapping Tables

### Namespace Mapping

| PDFSharp | IronPDF | Notes |
|---|---|---|
| `PdfSharp.Pdf` | `IronPdf` | Core document namespace |
| `PdfSharp.Drawing` | N/A — use HTML/CSS | Drawing API has no direct equivalent |
| `PdfSharp.Pdf.IO` | `IronPdf` | File I/O built into PdfDocument |

### Core Class Mapping

| PDFSharp Class | IronPDF Class | Description |
|---|---|---|
| `PdfDocument` | `PdfDocument` | Top-level document (different API surface) |
| `XGraphics` | N/A | No drawing API equivalent — use HTML |
| `PdfPage` | Individual pages via `pdf.Pages` | Page object |
| `PdfOutline` | `pdf.Bookmarks` | Bookmark/outline structure |

### Document Loading Methods

| Operation | PDFSharp | IronPDF |
|---|---|---|
| Create from HTML | Not built-in | `renderer.RenderHtmlAsPdfAsync(html)` |
| Create from URL | Not built-in | `renderer.RenderUrlAsPdfAsync(url)` |
| Open existing PDF | `PdfReader.Open(path)` | `PdfDocument.FromFile(path)` |
| Open from stream | `PdfReader.Open(stream)` | `PdfDocument.FromStream(stream)` |

### Page Operations

| Operation | PDFSharp | IronPDF |
|---|---|---|
| Page count | `doc.PageCount` | `pdf.PageCount` |
| Add page | `doc.AddPage()` | `pdf.AppendPdf(otherPdf)` to add pages from another document |
| Remove page | `doc.Pages.Remove(page)` | `pdf.RemovePages(index)` |
| Copy page from another doc | Manual copy | `pdf.CopyPage(index)` |

### Merge / Split Operations

| Operation | PDFSharp | IronPDF |
|---|---|---|
| Merge | Manual page-by-page copy from multiple `PdfDocument` objects | `PdfDocument.Merge(doc1, doc2)` |
| Split | Manual page-range extraction | `pdf.CopyPage(index)` / `pdf.CopyPages(range)` — [guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF (from programmatic to HTML-driven)

**Before (PDFSharp programmatic approach):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System;

class InvoiceBefore
{
    static void Main()
    {
        using var doc = new PdfDocument();
        doc.Info.Title = "Invoice #1042";

        var page = doc.AddPage();
        page.Size = PdfSharp.PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);

        // Manual coordinate placement — every element positioned by hand
        var titleFont = new XFont("Arial", 18, XFontStyleEx.Bold);
        gfx.DrawString("Invoice #1042", titleFont, XBrushes.Black,
            new XRect(50, 50, page.Width - 100, 30), XStringFormats.TopLeft);

        var bodyFont = new XFont("Arial", 11);
        gfx.DrawString("Bill To: Acme Corp", bodyFont, XBrushes.Black,
            new XPoint(50, 100));
        gfx.DrawString("Amount Due: $1,500.00", bodyFont, XBrushes.Black,
            new XPoint(50, 120));

        // Line for visual separation
        gfx.DrawLine(XPens.Black, 50, 140, page.Width - 50, 140);

        // Table header (manual layout)
        gfx.DrawString("Description", bodyFont, XBrushes.Black, new XPoint(50, 155));
        gfx.DrawString("Qty", bodyFont, XBrushes.Black, new XPoint(350, 155));
        gfx.DrawString("Total", bodyFont, XBrushes.Black, new XPoint(450, 155));

        doc.Save("invoice.pdf");
        Console.WriteLine("Saved invoice.pdf");
    }
}
```

**After (IronPDF HTML-driven):**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, sans-serif; padding: 50px; }
        h1 { font-size: 18px; }
        .bill-to, .amount { font-size: 11px; }
        hr { border: 1px solid #000; margin: 20px 0; }
        table { width: 100%; border-collapse: collapse; font-size: 11px; }
        th { text-align: left; padding: 4px 0; }
    </style>
    </head>
    <body>
        <h1>Invoice #1042</h1>
        <div class='bill-to'>Bill To: Acme Corp</div>
        <div class='amount'>Amount Due: $1,500.00</div>
        <hr/>
        <table>
            <tr><th>Description</th><th>Qty</th><th>Total</th></tr>
        </table>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");

Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (PDFSharp merge via page copy):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.IO;

class MergeBefore
{
    static void Main()
    {
        // PDFSharp merge: open each PDF, copy pages into output document
        var output = new PdfDocument();
        output.Info.Title = "Combined Report";

        // File 1
        using var part1 = PdfReader.Open("section1.pdf", PdfDocumentOpenMode.Import);
        foreach (var page in part1.Pages)
            output.AddPage(page);

        // File 2
        using var part2 = PdfReader.Open("section2.pdf", PdfDocumentOpenMode.Import);
        foreach (var page in part2.Pages)
            output.AddPage(page);

        output.Save("combined.pdf");
        Console.WriteLine($"Merged: {output.PageCount} pages");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Load existing PDFs
var part1 = PdfDocument.FromFile("section1.pdf");
var part2 = PdfDocument.FromFile("section2.pdf");

// Merge: https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(part1, part2);
merged.SaveAs("combined.pdf");

Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (PDFSharp drawing-API watermark):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using System;

class WatermarkBefore
{
    static void Main()
    {
        using var doc = PdfReader.Open("document.pdf", PdfDocumentOpenMode.Modify);

        foreach (var page in doc.Pages)
        {
            using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

            var font = new XFont("Arial", 72, XFontStyleEx.Bold);
            var state = gfx.Save();

            // Manual translation + rotation for diagonal watermark
            gfx.TranslateTransform(page.Width / 2, page.Height / 2);
            gfx.RotateTransform(-45);

            var brush = new XSolidBrush(XColor.FromArgb(50, 200, 0, 0)); // semi-transparent
            gfx.DrawString("CONFIDENTIAL", font, brush,
                new XRect(-200, -30, 400, 60), XStringFormats.Center);

            gfx.Restore(state);
        }

        doc.Save("watermarked.pdf");
        Console.WriteLine("Watermark applied");
    }
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Editing;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var pdf = PdfDocument.FromFile("document.pdf");

// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "CONFIDENTIAL",
    FontSize = 72,
    IsBold = true,
    FontColor = IronSoftware.Drawing.Color.Red,
    Opacity = 20,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (PDFSharp security settings — largely compatible API concept):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using System;

class PasswordBefore
{
    static void Main()
    {
        using var doc = new PdfDocument();
        var page = doc.AddPage();
        // ... content addition omitted

        // PDFSharp security settings
        var security = doc.SecuritySettings;
        security.UserPassword = "open123";
        security.OwnerPassword = "admin456";
        security.PermitPrint = true;
        security.PermitModifyDocument = false;

        doc.Save("secured.pdf");
        Console.WriteLine("Saved secured.pdf");
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
    "<html><body><h1>Secured Document</h1></body></html>"
);

// Security API — conceptually similar to PDFSharp
// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";
pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### The Drawing API Has No Direct Equivalent

This is the most important scoping issue for PDFSharp migrations. If your codebase uses `XGraphics.DrawString()`, `XGraphics.DrawLine()`, `XGraphics.DrawImage()`, or any other drawing API extensively, migration to IronPDF means rewriting those sections as HTML/CSS templates.

This is not always more work in the long run — HTML templates are often easier to maintain and modify — but the initial conversion is a non-trivial rewrite. Scope this carefully before committing to a migration timeline.

### Page Indexing Alignment

Both PDFSharp and IronPDF use 0-based page indexing. This is one area where the migration is lower-friction.

### SecuritySettings API — Conceptually Similar

PDFSharp's `SecuritySettings` concept maps fairly directly to IronPDF's `SecuritySettings`. Property names differ — audit and replace rather than assuming they're identical.

### MigraDoc

If your project uses MigraDoc alongside PDFSharp for document modeling, that's a separate migration surface. MigraDoc templates would need to be rewritten as HTML. Scope this separately.

### Async Migration

PDFSharp's API is synchronous. IronPDF's rendering methods are async. If you have synchronous code paths, refactor to `async/await` rather than blocking with `.Result`:

```csharp
// Avoid blocking in async contexts:
// var pdf = renderer.RenderHtmlAsPdfAsync(html).Result; // bad

// Use:
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

---

## Performance Considerations

### PDF Construction vs Rendering

PDFSharp's drawing API is fast for simple content because it writes PDF primitives directly. IronPDF's Chromium renderer parses and renders HTML/CSS before writing PDF. For very simple documents, PDFSharp construction may be faster. For complex, data-driven HTML, IronPDF's render time is the cost of the layout engine.

Measure for your specific content type — don't assume either direction.

```csharp
// Measure your actual template complexity:
var sw = Stopwatch.StartNew();
var renderer = new ChromePdfRenderer();
await renderer.RenderHtmlAsPdfAsync(yourActualHtmlTemplate); // warm-up

var times = new System.Collections.Generic.List<double>();
for (int i = 0; i < 20; i++)
{
    sw.Restart();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(yourActualHtmlTemplate);
    sw.Stop();
    times.Add(sw.Elapsed.TotalMilliseconds);
}

Console.WriteLine($"Your template avg: {times.Average():F1}ms");
// Compare against your PDFSharp baseline for the same output
```

### Disposal Pattern

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Always dispose PdfDocument
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("output.pdf");
// Disposed here — important for high-throughput scenarios
```

---

## Migration Checklist

### Pre-Migration
- [ ] Count PDFSharp API call density (`rg "XGraphics\.\|PdfDocument\.\|PdfReader\." --type cs | wc -l`)
- [ ] Separate drawing-API calls from file I/O and manipulation calls
- [ ] Identify HTML templates already in use (those can migrate directly)
- [ ] Identify MigraDoc usage if present (separate migration scope)
- [ ] Document PDFSharp SecuritySettings properties in use
- [ ] Measure baseline render times for comparison
- [ ] Obtain IronPDF license key
- [ ] Verify .NET version compatibility

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove PDFsharp packages (PDFsharp, PDFsharp-MigraDoc, PDFsharp-WPF, PDFsharp-GDI, or PdfSharpCore depending on which variant your project references)
- [ ] Add license key at startup
- [ ] Convert programmatic page construction to HTML/CSS templates (assess per-template)
- [ ] Replace `PdfReader.Open()` with `PdfDocument.FromFile()`
- [ ] Replace `doc.Save()` with `pdf.SaveAs()`
- [ ] Replace manual merge (page-copy loop) with `PdfDocument.Merge()`
- [ ] Replace XGraphics watermark with `TextStamper` / `ImageStamper`
- [ ] Migrate SecuritySettings properties (not a 1:1 match — audit and replace property names)
- [ ] Convert synchronous calls to async

### Testing
- [ ] Compare PDF output for each migrated template
- [ ] Verify SecuritySettings — test password-protected files open correctly
- [ ] Test merge output page count and page order
- [ ] Benchmark render time against PDFSharp baseline for your templates
- [ ] Test in Linux/Docker if applicable
- [ ] Verify PDFs open correctly in all downstream consumers

### Post-Migration
- [ ] Remove PDFSharp NuGet packages
- [ ] Verify no `PdfSharp.*` or `MigraDoc.*` namespaces remain in codebase
- [ ] Archive any drawing-API template code for reference during post-migration debugging
- [ ] Update internal documentation

---

## Next Steps

The primary migration complexity question with PDFSharp is always: how much of your code is using the drawing API versus how much is file I/O and manipulation? File I/O and manipulation (merge, open, save, security) migrates with low effort. Drawing API code requires template rewrites.

Run the `rg` count commands in the pre-migration checklist before scoping the work — the split between drawing calls and file operations will determine whether this is a day of work or several weeks.

**Discussion question:** What edge cases did you hit that this article didn't cover? Particularly interested in drawing API patterns that were difficult to express as HTML/CSS, or SecuritySettings property differences between PDFSharp and IronPDF.
