---
title: The SelectPdf to IronPDF migration nobody dramatised
published: false
tags: dotnet, csharp, pdf, migration
---

Page manipulation is the feature that breaks the SelectPdf evaluation. SelectPdf can generate PDFs from HTML, but when teams need to merge documents from multiple sources, extract text for indexing, add watermarks post-generation, or apply password encryption — they reach for another library. That second library adds a dependency, increases complexity, and sometimes conflicts. The missing-feature list, combined with SelectPdf's Windows-only constraint, is what typically puts it on the replacement shortlist.

This article covers migrating from SelectPdf to IronPDF. You'll have benchmark-comparable migration code and a comprehensive checklist by the end. The comparison and decision tables apply regardless of which alternative you pick.

---

## Why Migrate (Without Drama)

Teams evaluating SelectPdf alternatives commonly encounter:

1. **Windows-only deployment** — SelectPdf's [own documentation](https://selectpdf.com/pdf-library-for-net/) states it requires Windows and does not run on Linux, macOS, or Xamarin. That blocks Linux containers, Azure App Service (Linux), AWS Lambda, and most modern cloud targets.
2. **Default WebKit rendering engine** — per [SelectPdf's RenderingEngine docs](https://selectpdf.com/pdf-library/html/RenderingEngine.htm), the default engine is an internal WebKit. A Blink option (Chromium 124, released April 2024) was added in v19.1, but the WebKit default still ships and CSS Grid, gap, and CSS variables can fail there.
3. **Community Edition page cap and watermark** — per [selectpdf.com/community-edition](https://selectpdf.com/community-edition/), the Community Edition is hard-capped at 5 pages per PDF and stamps a watermark on every page until a license key is applied.
4. **Missing manipulation features** — merge, text extraction, watermarking, digital signatures, and security availability varies by edition; teams routinely add a second PDF library to fill the gaps.
5. **CSS rendering fidelity** — the default WebKit engine does not consistently support modern CSS (flex with gap, grid, custom properties).
6. **Synchronous API** — SelectPdf's `HtmlToPdf` converter exposes synchronous `ConvertHtmlString` / `ConvertUrl`, which is awkward inside async .NET request pipelines.
7. **Header/footer placeholder differences** — SelectPdf uses `{page_number}` / `{total_pages}`; IronPDF uses `{page}` / `{total-pages}`. Easy to miss in a string-by-string port.
8. **OEM and renewal pricing** — OEM is a separate, more expensive SKU at every tier on SelectPdf's [pricing page](https://selectpdf.com/pricing/), and updates beyond year one require a renewal.
9. **Secondary library accumulation** — teams often add PDFSharp, iTextSharp, or similar alongside SelectPdf for missing features, which increases complexity and dependency surface.

### Comparison Table

| Aspect | SelectPdf | IronPDF |
|---|---|---|
| Focus | HTML-to-PDF generation | HTML-to-PDF + PDF manipulation |
| Pricing | Community (5 pages, watermarked) + paid from $499 | Commercial license from $749 (Lite) |
| API Style | `HtmlToPdf` converter object | `ChromePdfRenderer` + `PdfDocument` |
| Learning Curve | Low for basic HTML-to-PDF | Low for .NET devs; similar API style |
| HTML Rendering | Default WebKit; optional Blink (Chromium 124) | Embedded modern Chromium |
| Platforms | Windows only | Windows, Linux, macOS, Docker, Azure, AWS |
| Async API | Synchronous | Sync and async (`RenderHtmlAsPdfAsync`) |
| Namespace | `SelectPdf` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | SelectPdf | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | `HtmlToPdf.ConvertHtmlString()` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | `HtmlToPdf.ConvertUrl()` | `ChromePdfRenderer.RenderUrlAsPdfAsync()` | Low |
| HTML file to PDF | `ConvertHtmlString(html, baseUrl)` (read file first) | `RenderHtmlFileAsPdfAsync(path)` | Low |
| Save to file | `doc.Save(path)` | `pdf.SaveAs(path)` | Low |
| Save to stream | `doc.Save(stream)` | `pdf.Stream` / `pdf.BinaryData` | Low |
| Custom page size | `HtmlToPdfOptions.PdfPageSize` | `RenderingOptions.PaperSize` | Low |
| Custom margins | `HtmlToPdfOptions.MarginTop` etc. | `RenderingOptions.Margin*` | Low |
| Headers/footers | `converter.Header.Add` / `Footer.Add` (`{page_number}`) | `RenderingOptions.HtmlHeader/Footer` (`{page}`) | Medium |
| Merge PDFs | Available in commercial edition | `PdfDocument.Merge()` | Medium |
| Watermark | Manual or commercial-edition feature | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Commercial edition | `pdf.SecuritySettings` | Medium |
| Text extraction | Limited | `pdf.ExtractAllText()` | Medium |
| Digital signatures | Not supported | `pdf.Sign()` | Medium-High |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Community Edition with 5-page cap or per-page watermark | Switch — IronPDF has no page cap and no per-page watermark on licensed output |
| Need merge, text extraction, security in one library | Switch — IronPDF covers all; SelectPdf often requires supplements |
| Modern CSS (flex with gap, grid) in HTML templates | Switch — modern Chromium vs default WebKit rendering |
| Linux, Docker, Azure Functions, or AWS Lambda deployment | Switch — SelectPdf is Windows-only |
| Basic HTML-to-PDF on Windows, working well, no manipulation needs | Evaluate cost of switching — SelectPdf may be sufficient |

---

## Before You Start

### Prerequisites

- .NET Framework 4.6.2+ or .NET 6 / 7 / 8 / 9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All SelectPdf References

```bash
# Find SelectPdf usage
rg -l "SelectPdf|HtmlToPdf|ConvertHtmlString|ConvertUrl" --type cs
rg "SelectPdf|HtmlToPdf\b" --type cs -n

# Find project references — note the real package IDs
grep -r "Select.HtmlToPdf" *.csproj **/*.csproj 2>/dev/null

# Count usage density to estimate migration scope
rg "HtmlToPdf\.|ConvertHtmlString|ConvertUrl\b" --type cs | wc -l
```

### Uninstall / Install

```bash
# Remove SelectPdf (the real NuGet IDs are Select.HtmlToPdf and Select.HtmlToPdf.NetCore,
# not "SelectPdf")
dotnet remove package Select.HtmlToPdf
# or, on .NET Core / .NET 5+:
dotnet remove package Select.HtmlToPdf.NetCore

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
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using SelectPdf;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic Conversion

**Before (SelectPdf):**
```csharp
using SelectPdf;
using System;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdf();
        converter.Options.PdfPageSize = PdfPageSize.A4;
        converter.Options.MarginTop = 20;
        converter.Options.MarginBottom = 20;

        var doc = converter.ConvertHtmlString(
            "<html><body><h1>Hello</h1></body></html>"
        );
        doc.Save("output.pdf");
        doc.Close();
        Console.WriteLine("Saved output.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;

var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Hello</h1></body></html>"
);
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Benchmark Reference Patterns

> Measurement structures only — no performance claims. Run against your actual HTML templates in your environment. Compare SelectPdf and IronPDF baseline times under equivalent load.

### Single-Document Render Time

```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, sans-serif; padding: 30px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { border: 1px solid #ddd; padding: 6px; font-size: 11px; }
        th { background: #f5f5f5; }
    </style>
    </head>
    <body>
        <h1>Benchmark Document</h1>
        <table>
            <tr><th>Item</th><th>Quantity</th><th>Price</th><th>Total</th></tr>
            <tr><td>Widget A</td><td>10</td><td>$12.50</td><td>$125.00</td></tr>
            <tr><td>Widget B</td><td>5</td><td>$24.00</td><td>$120.00</td></tr>
        </table>
    </body>
    </html>";

async Task<(double avg, double p95)> BenchmarkIronPdf(int runs = 25)
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
    return (times.Average(), times[(int)(times.Count * 0.95)]);
}

var (avg, p95) = await BenchmarkIronPdf(25);
Console.WriteLine($"IronPDF — Avg: {avg:F1}ms | P95: {p95:F1}ms");

// SelectPdf benchmark structure (synchronous — wrap with Stopwatch):
// var sw = Stopwatch.StartNew();
// var converter = new HtmlToPdf();
// var doc = converter.ConvertHtmlString(html);
// doc.Save("bench.pdf");
// doc.Close();
// Console.WriteLine($"SelectPdf: {sw.Elapsed.TotalMilliseconds:F1}ms");
```

### Concurrent Throughput

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// https://ironpdf.com/examples/parallel/
async Task BenchmarkConcurrency(int degree)
{
    var jobs = Enumerable.Range(1, degree)
        .Select(i => $"<html><body><h1>Document {i}</h1></body></html>")
        .ToArray();

    var sw = Stopwatch.StartNew();

    await Task.WhenAll(jobs.Select(async html =>
    {
        var renderer = new ChromePdfRenderer();
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.PageCount;
    }));

    sw.Stop();
    Console.WriteLine($"Degree {degree}: {sw.Elapsed.TotalMilliseconds:F0}ms total | {sw.Elapsed.TotalMilliseconds / degree:F1}ms/doc");
}

await BenchmarkConcurrency(5);
await BenchmarkConcurrency(10);
await BenchmarkConcurrency(20);
// See: https://ironpdf.com/how-to/async/
```

### Memory Profile Under Load

```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

static async Task MeasureMemory(int iterations)
{
    var renderer = new ChromePdfRenderer();
    var html = "<html><body><h1>Memory test</h1></body></html>";
    var before = GC.GetTotalMemory(forceFullCollection: true);

    for (int i = 0; i < iterations; i++)
    {
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        // pdf disposed each iteration
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    var after = GC.GetTotalMemory(forceFullCollection: true);
    Console.WriteLine($"Delta after {iterations} renders + GC: {(after - before) / 1024:F1} KB");
}

await MeasureMemory(50);
```

---

## API Mapping Tables

### Namespace Mapping

| SelectPdf | IronPDF | Notes |
|---|---|---|
| `SelectPdf` | `IronPdf` | Core namespace |
| `SelectPdf.HtmlToPdfOptions` | `IronPdf.Rendering.ChromePdfRenderOptions` | Rendering config |
| N/A | `IronPdf.Editing` | Watermark / stamp |

### Core Class Mapping

| SelectPdf Class | IronPDF Class | Description |
|---|---|---|
| `HtmlToPdf` | `ChromePdfRenderer` | HTML-to-PDF converter |
| `HtmlToPdfOptions` | `ChromePdfRenderOptions` | Page size, margins, options |
| `PdfDocument` (SelectPdf) | `PdfDocument` (IronPDF) | Output document — different API surface |
| `PdfPageSize` | `PdfPaperSize` | Page size enum |
| `PdfPageOrientation` | `PdfPaperOrientation` | Orientation enum |
| N/A | `PdfDocument.Merge()` | Static merge of multiple PDFs |

### Document Loading Methods

| Operation | SelectPdf | IronPDF |
|---|---|---|
| HTML string | `converter.ConvertHtmlString(html)` | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | `converter.ConvertUrl(url)` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML file | `ConvertHtmlString(File.ReadAllText(path), baseUrl)` | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Load existing PDF | `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | SelectPdf | IronPDF |
|---|---|---|
| Page count | `doc.Pages.Count` | `pdf.PageCount` |
| Remove page | `doc.Pages.Remove(doc.Pages[i])` | `pdf.RemovePages(i)` |
| Extract text | `doc.Pages[i].Text` | `pdf.ExtractAllText()` |
| Close / dispose | `doc.Close()` | `using var pdf = ...` / dispose pattern |

### Merge / Split Operations

| Operation | SelectPdf | IronPDF |
|---|---|---|
| Merge | Commercial-edition feature | `PdfDocument.Merge(doc1, doc2)` |
| Split | Commercial-edition feature | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

### Header/Footer Placeholders

| SelectPdf | IronPDF |
|---|---|
| `{page_number}` | `{page}` |
| `{total_pages}` | `{total-pages}` |
| `{url}` | `{url}` |
| `{date}` | `{date}` |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before (SelectPdf):**
```csharp
using SelectPdf;
using System;

class HtmlToPdfBefore
{
    static void Main()
    {
        var html = @"
            <html>
            <head>
            <style>
                body { font-family: Arial; padding: 30px; }
                .header { font-size: 22px; font-weight: bold; }
                table { width: 100%; border-collapse: collapse; }
                td, th { border: 1px solid #ccc; padding: 6px; }
            </style>
            </head>
            <body>
                <div class='header'>Order Confirmation #ORD-8821</div>
                <table>
                    <tr><th>Product</th><th>Qty</th><th>Price</th></tr>
                    <tr><td>Widget Pro</td><td>2</td><td>$149.00</td></tr>
                </table>
            </body>
            </html>";

        var converter = new HtmlToPdf();
        converter.Options.PdfPageSize = PdfPageSize.A4;
        converter.Options.MarginTop = 30;
        converter.Options.MarginBottom = 30;
        converter.Options.MarginLeft = 25;
        converter.Options.MarginRight = 25;

        var doc = converter.ConvertHtmlString(html);
        doc.Save("order-confirmation.pdf");
        Console.WriteLine($"Saved order-confirmation.pdf ({doc.Pages.Count} pages)");
        doc.Close();
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial; padding: 30px; }
        .header { font-size: 22px; font-weight: bold; }
        table { width: 100%; border-collapse: collapse; }
        td, th { border: 1px solid #ccc; padding: 6px; }
    </style>
    </head>
    <body>
        <div class='header'>Order Confirmation #ORD-8821</div>
        <table>
            <tr><th>Product</th><th>Qty</th><th>Price</th></tr>
            <tr><td>Widget Pro</td><td>2</td><td>$149.00</td></tr>
        </table>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 30;
renderer.RenderingOptions.MarginBottom = 30;
renderer.RenderingOptions.MarginLeft = 25;
renderer.RenderingOptions.MarginRight = 25;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("order-confirmation.pdf");

Console.WriteLine($"Saved order-confirmation.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (SelectPdf — merge is a commercial-edition feature; teams on the Community Edition typically pull in a secondary library):**
```csharp
using SelectPdf;
using System;
using System.Collections.Generic;
using System.IO;

class MergeBefore
{
    static void Main()
    {
        // Community Edition does not include merge; a second library is
        // commonly added to combine generated section PDFs.
        var htmlSections = new[]
        {
            "<html><body><h1>Invoice Header</h1></body></html>",
            "<html><body><h1>Invoice Line Items</h1></body></html>",
        };

        var pdfBytes = new List<byte[]>();

        foreach (var html in htmlSections)
        {
            var converter = new HtmlToPdf();
            var doc = converter.ConvertHtmlString(html);
            using var ms = new MemoryStream();
            doc.Save(ms);
            pdfBytes.Add(ms.ToArray());
            doc.Close();
        }

        // Combine pdfBytes with a separate PDF library or the commercial
        // SelectPdf edition.
        Console.WriteLine($"Generated {pdfBytes.Count} section PDFs ready to combine.");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

var results = await Task.WhenAll(
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Invoice Header</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Invoice Line Items</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("full-invoice.pdf");

Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (SelectPdf — built-in watermarking is limited; a common pattern is to inject watermark HTML into the source markup before conversion):**
```csharp
using SelectPdf;
using System;

class WatermarkBefore
{
    static void Main()
    {
        var html = @"
            <html><body>
                <div style='position: fixed; top: 40%; left: 30%;
                            font-size: 80px; color: rgba(0,0,0,0.15);
                            transform: rotate(-30deg);'>DRAFT</div>
                <h1>Report</h1>
            </body></html>";

        var converter = new HtmlToPdf();
        var doc = converter.ConvertHtmlString(html);
        doc.Save("watermarked.pdf");
        doc.Close();
        Console.WriteLine("Watermark applied via inline HTML.");
    }
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Editing;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Report</h1></body></html>");

// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = "#888888",
    Opacity = 15, // 0-100
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked-report.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (SelectPdf — password protection is a commercial-edition feature accessed via the document's security collection):**
```csharp
using SelectPdf;
using System;

class PasswordBefore
{
    static void Main()
    {
        var converter = new HtmlToPdf();
        var doc = converter.ConvertHtmlString("<html><body><h1>Secured</h1></body></html>");

        // Security configuration is part of the commercial edition.
        // doc.Security.OpenPassword = "open123";
        // doc.Security.PermissionPassword = "admin456";

        doc.Save("secured.pdf");
        doc.Close();
        Console.WriteLine("Saved secured.pdf (security requires SelectPdf commercial edition)");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Secured</h1></body></html>");

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### API Style is Close — but the Names Differ

SelectPdf and IronPDF share a conceptually similar API style (`Converter` + options → `Document` → `Save`). The migration is mostly find-and-replace for simple cases. The main divergences:

| Pattern | SelectPdf | IronPDF |
|---|---|---|
| Converter instantiation | `new HtmlToPdf()` | `new ChromePdfRenderer()` |
| Options property | `converter.Options.*` | `renderer.RenderingOptions.*` |
| Page size enum | `PdfPageSize` | `PdfPaperSize` |
| Orientation enum | `PdfPageOrientation` | `PdfPaperOrientation` |
| Convert method | `converter.ConvertHtmlString(html)` | `renderer.RenderHtmlAsPdfAsync(html)` |
| Convert URL | `converter.ConvertUrl(url)` | `renderer.RenderUrlAsPdfAsync(url)` |
| Save | `doc.Save(path)` | `pdf.SaveAs(path)` |
| Close | `doc.Close()` | Not required — use `using` |
| Header placeholder | `{page_number}` / `{total_pages}` | `{page}` / `{total-pages}` |

### Async Migration

SelectPdf's conversion methods are synchronous. IronPDF exposes both synchronous (`RenderHtmlAsPdf`) and asynchronous (`RenderHtmlAsPdfAsync`) variants. Prefer the async form in ASP.NET / async pipelines:

```csharp
// Avoid blocking in async contexts:
// var pdf = renderer.RenderHtmlAsPdfAsync(html).Result; // bad

// Use:
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
// See: https://ironpdf.com/how-to/async/
```

### Header/Footer Placeholder Tokens

If your SelectPdf header or footer HTML uses `{page_number}` and `{total_pages}`, those tokens render as literal text in IronPDF. Rename them to `{page}` and `{total-pages}` as part of the port.

### Community Edition Output Watermark and 5-Page Cap

Per [selectpdf.com/community-edition](https://selectpdf.com/community-edition/), the SelectPdf Community Edition caps output at 5 pages per PDF and stamps a watermark on every page until a license key is applied. After migrating to IronPDF (with a valid license), verify that test PDFs no longer carry the watermark and that any document longer than 5 pages renders completely — both are useful migration validation steps.

### Platform Compatibility

SelectPdf is Windows-only per its [own product page](https://selectpdf.com/pdf-library-for-net/). If part of the motivation for migrating is Linux containers, Azure App Service (Linux), or AWS Lambda, the deployment target itself is the migration test — exercise the IronPDF code path in the actual Linux runtime, not just on a developer Windows box.

---

## Performance Considerations

### Renderer Reuse

```csharp
using IronPdf;
using System.Collections.Generic;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Reuse renderer for batch generation.
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var htmlJobs = new List<(string Html, string Path)>
{
    (BuildInvoiceHtml(data1), "inv-001.pdf"),
    (BuildInvoiceHtml(data2), "inv-002.pdf"),
};

foreach (var job in htmlJobs)
{
    using var pdf = await renderer.RenderHtmlAsPdfAsync(job.Html);
    pdf.SaveAs(job.Path);
}
```

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

// https://ironpdf.com/examples/parallel/
var jobs = invoiceDataList.Select(data => BuildInvoiceHtml(data)).ToArray();

var pdfs = await Task.WhenAll(jobs.Select(async html =>
{
    var r = new ChromePdfRenderer();
    return await r.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Generated {pdfs.Length} invoices in parallel");
foreach (var pdf in pdfs) pdf.Dispose();
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// SelectPdf: doc.Save(path) or doc.Save(stream), then doc.Close()
// IronPDF: pdf.SaveAs(path) or pdf.Stream; using-block handles cleanup
pdf.SaveAs("output.pdf");
// pdf disposed at end of 'using' block
```

---

## Migration Checklist

### Pre-Migration
- [ ] Identify all SelectPdf usage (`rg "SelectPdf|HtmlToPdf\b|ConvertHtmlString" --type cs`)
- [ ] Verify which SelectPdf edition is in use (Community vs commercial)
- [ ] Document which features rely on edition-specific APIs (merge, security, watermark)
- [ ] Identify secondary libraries added to fill SelectPdf gaps
- [ ] Measure baseline render times for comparison benchmarking
- [ ] Obtain IronPDF license key
- [ ] Confirm target deployment platforms (Linux containers vs Windows)
- [ ] Audit header/footer HTML for `{page_number}` / `{total_pages}` tokens

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `Select.HtmlToPdf` / `Select.HtmlToPdf.NetCore` package reference
- [ ] Add license key at application startup
- [ ] Replace `new HtmlToPdf()` with `new ChromePdfRenderer()`
- [ ] Replace `converter.Options.*` with `renderer.RenderingOptions.*`
- [ ] Replace `PdfPageSize` / `PdfPageOrientation` with `PdfPaperSize` / `PdfPaperOrientation`
- [ ] Replace `ConvertHtmlString()` with `RenderHtmlAsPdfAsync()`
- [ ] Replace `ConvertUrl()` with `RenderUrlAsPdfAsync()`
- [ ] Replace `doc.Save(path)` with `pdf.SaveAs(path)`
- [ ] Remove `doc.Close()` calls (replace with `using` blocks)
- [ ] Rename `{page_number}` / `{total_pages}` to `{page}` / `{total-pages}`
- [ ] Convert synchronous calls to async where appropriate
- [ ] Replace secondary merge / security / watermark libraries with IronPDF equivalents

### Testing
- [ ] Render each HTML template and compare visual output
- [ ] Verify Community Edition watermark is absent after migration (if applicable)
- [ ] Verify documents longer than 5 pages render completely (Community Edition cap removed)
- [ ] Test URL rendering — verify authenticated URLs behave correctly
- [ ] Test headers and footers — confirm the new `{page}` / `{total-pages}` placeholders render
- [ ] Benchmark render time vs SelectPdf baseline
- [ ] Test concurrent rendering at target throughput
- [ ] Verify PDF output in target viewers and downstream consumers

### Post-Migration
- [ ] Remove the `Select.HtmlToPdf*` NuGet packages from all projects
- [ ] Remove secondary PDF libraries now replaced by IronPDF
- [ ] Update any documentation that references the SelectPdf API
- [ ] Re-test on Linux / Docker / Azure (Linux) if cross-platform was a driver

---

## Next Steps

The SelectPdf-to-IronPDF migration is one of the more direct in this space because the API styles are conceptually similar. The main work is resolving the feature gaps — any secondary library added to fill missing merge, security, or watermark functionality can usually be removed and the code paths consolidated — and switching the rendering engine to modern Chromium so the CSS layer behaves predictably.

The most useful benchmark to run is not single-render latency but concurrent throughput — that is typically where the rendering engine difference shows up most clearly.

**Discussion question:** What version of SelectPdf are you migrating from, and did anything break unexpectedly — particularly around headers/footers, the `{page_number}` placeholder, or Linux deployment?
