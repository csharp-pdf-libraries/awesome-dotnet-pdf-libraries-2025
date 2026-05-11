# Winnovative to IronPDF: three steps and you're done

The last commit to Winnovative's NuGet packages was years ago. The open GitHub issues haven't received responses in the same timeframe. The library still works — it does what it always did — but when a bug surfaces or a .NET runtime behavior changes, there's no update path. The decision isn't whether to migrate; it's when and to what.

This article covers migrating from Winnovative HTML to PDF Converter for .NET to IronPDF. You'll have benchmark reference patterns and working before/after code. The comparison tables and checklist apply regardless of which replacement you choose.

---

## Why Migrate (Without Drama)

Teams migrating from Winnovative to IronPDF commonly encounter:

1. **Maintenance signal** — verify current commit/release activity; outdated libraries carry growing technical risk as .NET evolves.
2. **Rendering engine age** — older rendering engines don't support modern CSS (flex, grid, custom properties); verify what your version supports.
3. **.NET version support** — verify .NET 6/7/8/9 compatibility at winnovative-software.com; newer runtimes may not be explicitly tested or supported.
4. **Native component management** — some Winnovative versions require native binaries or COM components, adding deployment friction.
5. **Linux/Docker gap** — if the rendering engine has Windows dependencies, cross-platform deployment isn't possible.
6. **No PDF manipulation** — HTML-to-PDF conversion is the primary feature; merge, watermark, security, and text extraction typically require secondary libraries.
7. **Thread safety** — verify concurrent rendering behavior in your version; this varies across HTML-to-PDF tools.
8. **Security updates** — an unmaintained library doesn't receive security patches; verify risk for your use case.
9. **Documentation staleness** — outdated documentation creates friction when troubleshooting edge cases.
10. **Support void** — no active maintainer means no path to resolution when bugs surface.

### Comparison Table

| Aspect | Winnovative HTML to PDF | IronPDF |
|---|---|---|
| Focus | HTML-to-PDF conversion | HTML-to-PDF + PDF manipulation |
| Pricing | Commercial — verify at winnovative-software.com | Commercial — verify at ironsoftware.com |
| API Style | `HtmlToPdfConverter.ConvertHtmlString()` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` |
| Learning Curve | Low for basic use | Low for .NET devs |
| HTML Rendering | Verify current rendering engine | Embedded Chromium |
| Page Indexing | Verify in Winnovative docs | 0-based |
| Thread Safety | Verify in Winnovative docs | Verify IronPDF concurrent instance guidance |
| Namespace | `Winnovative.HtmlToPdfConverter.*` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Winnovative | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | `converter.ConvertHtmlString(html)` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | `converter.ConvertUrl(url)` | `renderer.RenderUrlAsPdfAsync(url)` | Low |
| Save to file | `doc.Save(path)` or byte[] + write | `pdf.SaveAs(path)` | Low |
| Save to stream | Verify stream API | `pdf.Stream` / `pdf.BinaryData` | Low |
| Custom page size | `converter.PdfPageSize = ...` | `RenderingOptions.PaperSize` | Low |
| Margins | Converter settings | `RenderingOptions.Margin*` | Low |
| Headers/footers | Verify API | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Merge PDFs | Verify support | `PdfDocument.Merge()` | Medium |
| Watermark | Verify support | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Verify support | `pdf.SecuritySettings` | Low |
| Text extraction | Verify support | `pdf.ExtractAllText()` | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Active maintenance is the primary concern | Switch — eliminates risk of unmaintained dependency |
| Modern CSS (flex, grid) rendering needed | Switch — Chromium vs older rendering engine |
| Linux/Docker deployment | Switch — verify Winnovative Linux support; likely not available |
| Winnovative working well, no maintenance concern | Evaluate migration cost vs risk timeline |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All Winnovative References

```bash
# Find Winnovative usage (verify namespace in your version)
rg -l "Winnovative\|HtmlToPdfConverter\b" --type cs
rg "Winnovative\|ConvertHtmlString\|ConvertUrl\b" --type cs -n

# Find NuGet references
grep -r "Winnovative" *.csproj **/*.csproj 2>/dev/null

# Count usage density
rg "HtmlToPdfConverter\|ConvertHtmlString\|ConvertUrl\b" --type cs | wc -l
```

### Uninstall / Install

```bash
# Remove Winnovative packages (verify exact package names at NuGet)
dotnet remove package Winnovative.HtmlToPdfConverter  # verify exact name

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
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
    ?? throw new InvalidOperationException("IRONPDF_LICENSE_KEY not set");
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using Winnovative;           // verify namespace
using Winnovative.HtmlToPdfConverter; // verify
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic HTML to PDF

**Before (Winnovative — verify all API names):**
```csharp
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // VERIFY: Winnovative API names for your version
        // All names below are based on commonly documented usage — verify at their site

        var converter = new HtmlToPdfConverter();

        // Converter settings — VERIFY property names
        // converter.PdfPageSize = PdfPageSize.A4;
        // converter.PdfPageOrientation = PdfPageOrientation.Portrait;

        // Convert HTML string — VERIFY method name and return type
        var doc = converter.ConvertHtmlString(
            "<html><body><h1>Hello</h1></body></html>"
        );

        // Save — VERIFY save method
        doc.Save("output.pdf");
        Console.WriteLine("Saved output.pdf — verify all Winnovative API names");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Hello</h1></body></html>");
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Benchmark Reference Patterns

> Measurement structures only — no claims. Run against your own HTML and environment. Use these to compare Winnovative and IronPDF baseline times before committing.

### Single Render Timing

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
        body { font-family: Arial, sans-serif; padding: 40px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { border: 1px solid #ddd; padding: 6px; font-size: 11px; }
        th { background: #f0f0f0; }
    </style>
    </head>
    <body>
        <h1>Benchmark Report</h1>
        <table>
            <tr><th>Region</th><th>Revenue</th><th>YoY</th></tr>
            <tr><td>North America</td><td>$12.4M</td><td>+8.2%</td></tr>
            <tr><td>Europe</td><td>$9.1M</td><td>+3.7%</td></tr>
            <tr><td>APAC</td><td>$6.8M</td><td>+14.3%</td></tr>
        </table>
    </body>
    </html>";

async Task<(double avg, double p95)> BenchmarkIronPdf(int runs = 25)
{
    var renderer = new ChromePdfRenderer();
    using var warmup = await renderer.RenderHtmlAsPdfAsync(html); // warm up

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

// Winnovative benchmark structure (synchronous):
// var sw = Stopwatch.StartNew();
// var converter = new HtmlToPdfConverter(); // VERIFY
// var doc = converter.ConvertHtmlString(html); // VERIFY
// doc.Save("bench.pdf"); // VERIFY
// Console.WriteLine($"Winnovative: {sw.Elapsed.TotalMilliseconds:F1}ms");
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
    var jobs = Enumerable.Range(1, degree)
        .Select(i => $"<html><body><h1>Doc {i}</h1></body></html>")
        .ToArray();

    var sw = Stopwatch.StartNew();

    await Task.WhenAll(jobs.Select(async html =>
    {
        var renderer = new ChromePdfRenderer();
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.PageCount;
    }));

    sw.Stop();
    Console.WriteLine($"Degree {degree}: {sw.Elapsed.TotalMilliseconds:F0}ms | {sw.Elapsed.TotalMilliseconds / degree:F1}ms/doc");
}

await BenchmarkConcurrency(5);
await BenchmarkConcurrency(10);
await BenchmarkConcurrency(20);
// See: https://ironpdf.com/how-to/async/
```

### Memory Profile

```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

static async Task MeasureMemoryDelta(int iterations)
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
    Console.WriteLine($"{iterations} renders: delta {(after - before) / 1024:F0} KB after GC");
}

await MeasureMemoryDelta(50);
```

---

## API Mapping Tables

### Namespace Mapping

| Winnovative | IronPDF | Notes |
|---|---|---|
| `Winnovative.HtmlToPdfConverter` | `IronPdf` | Core namespace |
| N/A | `IronPdf.Rendering` | Rendering config |
| N/A | `IronPdf.Editing` | Watermark / stamp |

### Core Class Mapping

| Winnovative Class | IronPDF Class | Description |
|---|---|---|
| `HtmlToPdfConverter` | `ChromePdfRenderer` | Primary rendering class |
| Converter settings object | `ChromePdfRenderOptions` | Page size, margins, options |
| PDF document result | `PdfDocument` | Output object |
| N/A | `PdfDocument.Merge()` | Static merge |

### Document Loading Methods

| Operation | Winnovative | IronPDF |
|---|---|---|
| HTML string | `converter.ConvertHtmlString(html)` | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | `converter.ConvertUrl(url)` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML file | Verify | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Load existing PDF | Verify | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | Winnovative | IronPDF |
|---|---|---|
| Page count | Verify | `pdf.PageCount` |
| Remove page | Verify | `pdf.RemovePage(index)` — verify |
| Extract text | Verify | `pdf.ExtractAllText()` |
| Rotate | Verify | Verify in IronPDF docs |

### Merge / Split Operations

| Operation | Winnovative | IronPDF |
|---|---|---|
| Merge | Verify support | `PdfDocument.Merge(doc1, doc2)` |
| Split | Verify | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before (Winnovative — verify all API names):**
```csharp
using System;
using System.IO;

class HtmlToPdfBefore
{
    static void Main()
    {
        // VERIFY: all Winnovative class and method names at winnovative-software.com

        var html = @"
            <html>
            <head><style>
            body { font-family: Arial; padding: 40px; }
            .header { font-size: 20px; font-weight: bold; }
            table { width: 100%; border-collapse: collapse; }
            td, th { border: 1px solid #ccc; padding: 6px; }
            </style></head>
            <body>
                <div class='header'>Invoice #INV-2024-0099</div>
                <p>Customer: Acme Corp | Due: 2024-12-31</p>
                <table>
                    <tr><th>Item</th><th>Qty</th><th>Price</th></tr>
                    <tr><td>Widget Pro</td><td>5</td><td>$149.00</td></tr>
                </table>
            </body></html>";

        // VERIFY: HtmlToPdfConverter class and properties
        var converter = new HtmlToPdfConverter();
        // converter.PdfPageSize = PdfPageSize.A4; // VERIFY property name/enum

        // VERIFY: ConvertHtmlString method name and return type
        var doc = converter.ConvertHtmlString(html);

        // VERIFY: Save method
        doc.Save("invoice.pdf");
        Console.WriteLine("Saved invoice.pdf — verify all Winnovative API names");
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
    <head><style>
    body { font-family: Arial; padding: 40px; }
    .header { font-size: 20px; font-weight: bold; }
    table { width: 100%; border-collapse: collapse; }
    td, th { border: 1px solid #ccc; padding: 6px; }
    </style></head>
    <body>
        <div class='header'>Invoice #INV-2024-0099</div>
        <p>Customer: Acme Corp | Due: 2024-12-31</p>
        <table>
            <tr><th>Item</th><th>Qty</th><th>Price</th></tr>
            <tr><td>Widget Pro</td><td>5</td><td>$149.00</td></tr>
        </table>
    </body></html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");
Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (Winnovative — verify merge support):**
```csharp
using System;
using System.Collections.Generic;
using System.IO;

class MergeBefore
{
    static void Main()
    {
        // VERIFY: Winnovative merge support and API
        // May require generating each section separately and merging with secondary library

        var sections = new[]
        {
            "<html><body><h1>Section 1</h1></body></html>",
            "<html><body><h1>Section 2</h1></body></html>",
        };

        var pdfBytes = new List<byte[]>();

        foreach (var html in sections)
        {
            var converter = new HtmlToPdfConverter(); // VERIFY
            var doc = converter.ConvertHtmlString(html); // VERIFY
            // VERIFY: export to bytes
            // pdfBytes.Add(doc.ToBytes()); // illustrative
        }

        // Merge via secondary library if Winnovative doesn't support it:
        // var merged = SomePdfLib.Merge(pdfBytes);
        Console.WriteLine("Verify Winnovative merge API — secondary library may be needed");
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

var results = await Task.WhenAll(
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 1</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 2</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (Winnovative — verify support):**
```csharp
using System;
// VERIFY: Winnovative watermark API — may not be native
// If not supported, secondary library needed after generation

class WatermarkBefore
{
    static void Main()
    {
        // var converter = new HtmlToPdfConverter(); // VERIFY
        // var doc = converter.ConvertHtmlString("<html><body><h1>Report</h1></body></html>");
        // VERIFY: watermark method — may not exist

        // If not native: apply via secondary library
        // var bytes = doc.ToBytes();
        // var watermarked = SomePdfLib.AddTextWatermark(bytes, "DRAFT");
        // File.WriteAllBytes("watermarked.pdf", watermarked);

        Console.WriteLine("Verify Winnovative watermark API — may require secondary library");
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
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Report</h1></body></html>");

// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronPdf.Imaging.Color.Gray,
    Opacity = 0.15,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (Winnovative — verify security API):**
```csharp
using System;
// VERIFY: Winnovative password/security API

class PasswordBefore
{
    static void Main()
    {
        // var converter = new HtmlToPdfConverter(); // VERIFY
        // var doc = converter.ConvertHtmlString("<html><body><h1>Secured</h1></body></html>");

        // VERIFY: Winnovative security API — property names are version-dependent
        // doc.Security.UserPassword = "open123";   // illustrative — VERIFY
        // doc.Security.OwnerPassword = "admin456"; // illustrative — VERIFY
        // doc.Save("secured.pdf");

        Console.WriteLine("Verify Winnovative security API — check winnovative-software.com docs");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

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

### Verify API Names Before Any Code Removal

Winnovative has limited public documentation compared to larger libraries. Before removing any Winnovative code, run the codebase audit to understand exact method signatures in use:

```bash
# Capture all method calls before removing anything
rg "converter\.\|HtmlToPdfConverter\.\|ConvertHtml\|ConvertUrl\b" --type cs -n > winnovative-usage.txt
cat winnovative-usage.txt
# Use this as the migration map — verify each line against Winnovative docs
```

### Rendering Engine Difference

Winnovative uses an internal rendering engine. IronPDF uses Chromium. Modern CSS that didn't work in Winnovative may now work — and some Winnovative-specific CSS workarounds may no longer be needed:

```bash
# Find CSS that was added as Winnovative workarounds
rg "TODO.*winnovative\|HACK.*render\|workaround.*pdf" --type css --type html -in

# After migration, test whether these workarounds are still needed
```

### `ConvertHtmlString` Return Type

Winnovative's return type varies by version — it may return `byte[]` directly or a document object. Map to IronPDF accordingly:

```csharp
// If Winnovative returned byte[]:
// var bytes = converter.ConvertHtmlString(html);
// File.WriteAllBytes("output.pdf", bytes);

// IronPDF equivalent:
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
var bytes = pdf.BinaryData; // equivalent byte[]
pdf.SaveAs("output.pdf");   // or save directly
```

### Page Indexing

Verify Winnovative's page indexing (if any page manipulation was used) before assuming 0-based. IronPDF uses 0-based indexing.

---

## Performance Considerations

### Parallel Generation

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/examples/parallel/
var pdfs = await Task.WhenAll(dataList.Select(async data =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(BuildHtml(data));
}));

foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// Map from Winnovative return:
// If Winnovative returned byte[]: pdf.BinaryData is the equivalent
// If Winnovative returned a document with a Save(stream): pdf.Stream.CopyTo(stream)
pdf.SaveAs("output.pdf");
// pdf disposed at end of 'using' block
```

---

## Migration Checklist

### Pre-Migration
- [ ] Verify Winnovative maintenance status at winnovative-software.com
- [ ] Find all Winnovative API usage (`rg "Winnovative\|ConvertHtmlString\|ConvertUrl" --type cs`)
- [ ] Document exact method signatures in use
- [ ] Identify secondary libraries used for merge/security/watermark
- [ ] Measure baseline render time for benchmark comparison
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility
- [ ] Check for native/COM dependency requirements in current Winnovative setup

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove Winnovative NuGet packages
- [ ] Add license key at application startup
- [ ] Replace `HtmlToPdfConverter.ConvertHtmlString()` with `ChromePdfRenderer.RenderHtmlAsPdfAsync()`
- [ ] Replace `converter.ConvertUrl()` with `renderer.RenderUrlAsPdfAsync()`
- [ ] Replace `doc.Save(path)` with `pdf.SaveAs(path)`
- [ ] Map converter settings properties to `RenderingOptions.*`
- [ ] Replace secondary merge library with `PdfDocument.Merge()`
- [ ] Replace secondary watermark library with `TextStamper`
- [ ] Replace secondary security library with `pdf.SecuritySettings`

### Testing
- [ ] Compare PDF output visually against Winnovative reference
- [ ] Test modern CSS rendering (flex, grid) — may now work where it didn't before
- [ ] Benchmark render time — single and concurrent
- [ ] Test Linux/Docker deployment if applicable
- [ ] Test merge, watermark, and security
- [ ] Verify page size and margin settings match reference

### Post-Migration
- [ ] Remove Winnovative NuGet packages
- [ ] Remove secondary libraries now replaced by IronPDF
- [ ] Remove Winnovative native components from deployment if applicable
- [ ] Archive Winnovative CSS workarounds in case reference is needed

---

## One Last Thing

The maintenance signal — last update years ago, issues unanswered — is the clearest migration trigger. The technical migration is straightforward because both tools do the same job (HTML-to-PDF); the API surface maps closely even if names differ.

The benchmark patterns above are most useful for understanding how concurrent throughput changes — particularly if the current Winnovative setup has any thread-safety workarounds that added infrastructure overhead.

**Discussion question:** Which feature was hardest to replicate in IronPDF — was it a specific CSS behavior that worked differently, a margin/header setting that didn't map directly, or something in the rendering output that surprised you?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
