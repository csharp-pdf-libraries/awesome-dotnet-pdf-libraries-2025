---
title: "Migrating from ComPDFKit to IronPDF: from install to ship"
published: false
tags: dotnet, csharp, pdf, migration
---

Performance numbers are the first thing people reach for when justifying a library migration to management. They're also the easiest to misrepresent — a benchmark that doesn't match your workload is worse than no benchmark at all.

This article takes a benchmark-aware approach to migrating from **ComPDFKit** to **IronPDF**. That means: showing you how to measure what actually matters in your use case, providing before/after code for the operations most teams care about, and flagging where the libraries differ in architecture in ways that will affect throughput under real load. No synthetic numbers that don't apply to your environment.

You'll leave with working migration code, an API mapping table, and a repeatable benchmark harness you can run against your own workloads — because the only relevant performance number is the one measured with your data.

---

## Why Migrate (Without Drama)

Teams using ComPDFKit on .NET typically adopted it for PDF viewing, annotation, or interactive document scenarios. Server-side batch generation and HTML-to-PDF are where friction tends to appear:

1. **HTML-to-PDF workloads** — ComPDFKit's .NET SDK does not include a native HTML/CSS rendering engine. The vendor's documented path for HTML conversion is a separate cloud Conversion API. If browser-quality HTML rendering is a core requirement, an in-process Chromium renderer is a structurally different fit.
2. **Server-side batch generation** — Libraries optimized for interactive viewers have different performance characteristics at scale than batch generators. Measure your workload specifically.
3. **Cross-platform server deployment** — ComPDFKit ships SDKs for Windows, Linux, and macOS, but cross-platform server scenarios still benefit from confirming the deployment story matches your target environment.
4. **NuGet package management** — Native binary distribution outside of NuGet adds CI/CD pipeline complexity. IronPDF's runtime binaries are pulled through NuGet.
5. **API surface for common operations** — Merge, watermark, and encrypt require more setup steps in a viewer-focused SDK than in a generation-focused one.
6. **.NET version compatibility** — IronPDF supports .NET Framework 4.6.2+, .NET Core 3.1+, and .NET 5/6/7/8/9. Confirm your ComPDFKit package matches your target framework.
7. **Memory footprint under load** — Viewer-optimized libraries sometimes carry rendering state that adds overhead in stateless server contexts.
8. **License model alignment** — Confirm ComPDFKit licensing terms cover server/SaaS use against the vendor's current pricing page.
9. **Async/parallel generation** — IronPDF exposes `RenderHtmlAsPdfAsync` and supports parallel rendering through renderer reuse; confirm the equivalent guidance for ComPDFKit's `CPDFDocument` lifecycle.
10. **Azure/cloud deployment** — IronPDF documents Azure App Service, AWS Lambda, and Docker deployment paths.

### Comparison Table

| Aspect | ComPDFKit | IronPDF |
|---|---|---|
| Focus | PDF viewer, annotation, forms, editing | HTML-to-PDF, edit, merge, security |
| Pricing | Per-app or per-platform | Per-developer or royalty-free |
| API Style | SDK-style, C++ influenced, verbose | High-level renderer + document model |
| Learning Curve | Steep for server-side; designed for UI integration | Gradual for HTML; renderer-first mental model |
| HTML Rendering | No native HTML/CSS engine in the .NET SDK | Chromium-based |
| Page Indexing | 0-based | 0-based |
| Memory Cleanup | Manual `Release()` calls | Automatic (GC) |
| Namespace | `ComPDFKit.PDFDocument`, `ComPDFKit.PDFPage`, etc. | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Effort | Notes |
|---|---|---|
| HTML string to PDF | Low–Medium | New capability vs ComPDFKit's manual layout path |
| HTML file to PDF | Low–Medium | Same |
| Merge PDFs | Medium | API model differs |
| Split PDFs | Medium | `CopyPages` ranges in IronPDF |
| Watermark | Medium | `CPDFWatermark` vs IronPDF `ApplyWatermark` HTML |
| Password protection | Low–Medium | Both support; property names differ |
| Annotation migration | High | ComPDFKit annotation model is rich; test parity |
| Form field editing | Medium | IronPDF exposes `pdf.Form.SetFieldValue` |
| Server-side batch | Medium | Architecture change; test throughput |
| Async / parallel | Medium | IronPDF supports `Async` variants and `Parallel.ForEach` patterns |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Interactive PDF viewer/editor application | Evaluate carefully — ComPDFKit is designed for this; IronPDF is not a UI viewer |
| Server-side HTML-to-PDF batch generation | IronPDF's Chromium renderer is a better fit |
| API-driven PDF operations (merge, stamp, encrypt) | Both work; IronPDF API is more concise for these ops |
| Mixed viewer + server generation | May need both libraries — evaluate consolidation cost |

---

## Before You Start: Set Up a Measurement Baseline

Before removing ComPDFKit, capture timing data for your most frequent operations. This gives you apples-to-apples comparison after migration.

```csharp
using System;
using System.Diagnostics;

// Benchmark harness — run against your actual workload before migration
class PdfBenchmark
{
    static void Main()
    {
        int iterations = 50;
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            // Run your most common ComPDFKit operation here
            // Example: ConvertHtmlToPdf("sample.html", $"out_{i}.pdf")
        }

        sw.Stop();
        double avgMs = (double)sw.ElapsedMilliseconds / iterations;
        Console.WriteLine($"ComPDFKit average: {avgMs:F1} ms per operation ({iterations} iterations)");
        Console.WriteLine($"Total: {sw.ElapsedMilliseconds} ms");
    }
}
```

Run this benchmark. Record the numbers. Then run the equivalent IronPDF benchmark after migration. The delta in your environment is the only number that matters.

**Find ComPDFKit references in your codebase:**

```bash
# Find all ComPDFKit imports
rg "using ComPDFKit" --type cs -l

# Find SDK initialization patterns
rg "CPDFSDKVerifier|LicenseVerify" --type cs

# Find document operations
rg "CPDFDocument|CPDFPage|CPDFAnnotation" --type cs
```

**Install IronPDF:**

```bash
# Add IronPDF (keep ComPDFKit while running parallel benchmarks)
dotnet add package IronPdf
dotnet restore
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

**Before (ComPDFKit):**

```csharp
using ComPDFKit.PDFDocument;

class Program
{
    static void Main()
    {
        // ComPDFKit license verification — must run before document operations
        CPDFSDKVerifier.LicenseVerify(
            "YOUR-LICENSE-KEY", "YOUR-LICENSE-KEY",
            "YOUR-LICENSE-KEY", "YOUR-LICENSE-KEY");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 2 — Namespace Imports

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Import;
```

**After:**
```csharp
using IronPdf;
```

### Step 3 — Basic HTML-to-PDF

**Before (ComPDFKit):**
```csharp
// ComPDFKit's .NET SDK has no native HTML/CSS renderer. To approximate
// HTML output you would either lay out text and images manually via the
// page editor, or call ComPDFKit's separate cloud Conversion API
// (https://api.compdf.com/api-libraries) as a different SKU.

using ComPDFKit.PDFDocument;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.CreateDocument();
        document.InsertPage(0, 595, 842, string.Empty);
        // Manual page editor operations would go here to place text/images.
        document.WriteToFilePath("output.pdf");
        document.Release();
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        using var pdf = renderer.RenderHtmlAsPdf(
            "<html><body><h1>Hello</h1></body></html>"
        );
        pdf.SaveAs("output.pdf");
        Console.WriteLine("Done");
    }
}
```

---

## API Mapping Tables

### Namespace Mapping

| ComPDFKit | IronPDF | Notes |
|---|---|---|
| `ComPDFKit.PDFDocument` | `IronPdf` | Core document operations |
| `ComPDFKit.PDFPage` | `IronPdf` | Page-level operations |
| `ComPDFKit.PDFAnnotation` | `IronPdf` | Annotations |
| `ComPDFKit.PDFWatermark` | `IronPdf` | Watermarks |
| `ComPDFKit.Import` | `IronPdf` | Import/conversion |

### Core Class Mapping

| ComPDFKit | IronPDF Class | Description |
|---|---|---|
| `CPDFDocument` | `PdfDocument` | PDF document object |
| `CPDFPage` | `PdfPage` | Page reference |
| `CPDFSDKVerifier` | `IronPdf.License` | SDK/license initialization |
| `CPDFWatermark` | `ApplyWatermark(html)` | Watermark application |
| Manual layout / cloud Conversion API | `ChromePdfRenderer` | HTML-to-PDF rendering |

### Document Loading Methods

| Operation | ComPDFKit | IronPDF |
|---|---|---|
| Load from file | `CPDFDocument.InitWithFilePath(path)` | `PdfDocument.FromFile(path)` |
| Load from stream | `CPDFDocument.InitWithStream(stream)` | `PdfDocument.FromStream(stream)` |
| Load from bytes | Via stream | `PdfDocument.FromBinaryData(bytes)` |
| Create from HTML | Not natively supported | `renderer.RenderHtmlAsPdf(html)` |
| Save to file | `document.WriteToFilePath(path)` | `pdf.SaveAs(path)` |
| Save to bytes | Via stream | `pdf.BinaryData` |

### Page Operations

| Operation | ComPDFKit | IronPDF |
|---|---|---|
| Page count | `document.PageCount` | `pdf.PageCount` |
| Get page | `document.PageAtIndex(n)` | `pdf.Pages[n]` (0-based) |
| Remove page | `document.RemovePage(n)` | `pdf.RemovePages(n)` |
| Insert page | `document.InsertPage(i, w, h, "")` | Via merge / `CopyPages` |
| Rotate page | `page.SetRotation(angle)` | `pdf.Pages[i].Rotation = ...` |
| Extract pages | `document.ExtractPages(range)` | `pdf.CopyPages(start, end)` |

### Merge/Split

| Operation | ComPDFKit | IronPDF |
|---|---|---|
| Merge PDFs | `doc1.ImportPagesAtIndex(doc2, range, index)` | `PdfDocument.Merge(pdf1, pdf2)` |
| Extract pages | `document.ExtractPages(range)` | `pdf.CopyPages(start, end)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (ComPDFKit):**

```csharp
using System;
using ComPDFKit.PDFDocument;

// ComPDFKit's .NET SDK does not provide HTML/CSS rendering. Two practical
// options: lay out text and images manually with the page editor, or use
// the separate cloud Conversion API at https://api.compdf.com/api-libraries.

class Program
{
    static void Main()
    {
        // License verification — must precede document operations
        CPDFSDKVerifier.LicenseVerify(
            "YOUR-LICENSE-KEY", "YOUR-LICENSE-KEY",
            "YOUR-LICENSE-KEY", "YOUR-LICENSE-KEY");

        var document = CPDFDocument.CreateDocument();
        document.InsertPage(0, 595, 842, string.Empty); // blank A4 page

        // Manual editor-based text placement would happen here.
        // For HTML input, ComPDFKit recommends their cloud Conversion API.

        document.WriteToFilePath("output.pdf");
        document.Release();
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Chromium-based rendering — handles modern CSS
        // https://ironpdf.com/how-to/html-string-to-pdf/
        var renderer = new ChromePdfRenderer();

        string html = @"<html>
            <body style='font-family:Arial; padding:40px'>
                <h1 style='color:#1D4ED8'>Invoice #3810</h1>
                <table border='1' style='width:100%'>
                    <tr><th>Item</th><th>Amount</th></tr>
                    <tr><td>Service A</td><td>$800</td></tr>
                    <tr><td>Service B</td><td>$400</td></tr>
                </table>
            </body>
        </html>";

        using var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("invoice.pdf");
        Console.WriteLine("Saved invoice.pdf");
    }
}
```

### 2. Merge PDFs

**Before (ComPDFKit):**

```csharp
using System;
using ComPDFKit.PDFDocument;
using ComPDFKit.Import;

class Program
{
    static void Main()
    {
        var document1 = CPDFDocument.InitWithFilePath("part1.pdf");
        var document2 = CPDFDocument.InitWithFilePath("part2.pdf");

        // Append all pages from document2 to the end of document1
        string range = $"0-{document2.PageCount - 1}";
        document1.ImportPagesAtIndex(document2, range, document1.PageCount);

        document1.WriteToFilePath("merged.pdf");
        document1.Release();
        document2.Release();
    }
}
```

**After (IronPDF):**

```csharp
using System;
using System.Collections.Generic;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // https://ironpdf.com/how-to/merge-or-split-pdfs/
        var pdf1 = PdfDocument.FromFile("part1.pdf");
        var pdf2 = PdfDocument.FromFile("part2.pdf");
        var pdf3 = PdfDocument.FromFile("part3.pdf");

        using var merged = PdfDocument.Merge(new List<PdfDocument> { pdf1, pdf2, pdf3 });
        merged.SaveAs("merged.pdf");
        Console.WriteLine($"Merged {merged.PageCount} pages to merged.pdf");
    }
}
```

### 3. Watermark

**Before (ComPDFKit):**

```csharp
using System;
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFWatermark;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.InitWithFilePath("input.pdf");

        // CPDFDocument.InitWatermark returns a CPDFWatermark, which is then
        // configured and committed with CreateWatermark().
        CPDFWatermark watermark = document.InitWatermark(
            C_Watermark_Type.WATERMARK_TYPE_TEXT);
        watermark.SetText("CONFIDENTIAL");
        watermark.SetFontName("Helvetica");
        watermark.SetFontSize(48);
        watermark.SetTextRGBColor(255, 0, 0);
        watermark.SetRotation(45);
        watermark.SetOpacity(76); // 0..255 (76 ~ 30%)
        watermark.SetVertalign(C_Watermark_Vertalign.WATERMARK_VERTALIGN_CENTER);
        watermark.SetHorizalign(C_Watermark_Horizalign.WATERMARK_HORIZALIGN_CENTER);
        watermark.SetPages($"0-{document.PageCount - 1}");
        watermark.SetFront(true);
        watermark.CreateWatermark();

        document.WriteToFilePath("watermarked.pdf");
        document.Release();
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        // https://ironpdf.com/how-to/watermark/
        pdf.ApplyWatermark(
            "<h1 style='color:rgba(255,0,0,0.3);'>CONFIDENTIAL</h1>",
            rotation: 45,
            verticalAlignment: VerticalAlignment.Middle,
            horizontalAlignment: HorizontalAlignment.Center);

        pdf.SaveAs("watermarked.pdf");
        Console.WriteLine("Watermarked PDF saved");
    }
}
```

### 4. Password Protection

**Before (ComPDFKit):**

```csharp
using System;
using ComPDFKit.PDFDocument;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.InitWithFilePath("input.pdf");

        // CPDFPermissionsInfo configures the permission bitmask; Encrypt()
        // applies both passwords and permissions in a single call.
        var permission = new CPDFPermissionsInfo
        {
            AllowsCopying = true,
            AllowsPrinting = true,
            AllowsDocumentChanges = false
        };

        document.Encrypt("user123", "owner456", permission);
        document.WriteToFilePath("protected.pdf");
        document.Release();
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // https://ironpdf.com/how-to/pdf-permissions-passwords/
        using var pdf = PdfDocument.FromFile("input.pdf");

        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting =
            IronPdf.Security.PdfPrintSecurity.FullPrintRights;

        pdf.SaveAs("protected.pdf");
        Console.WriteLine("Password protected PDF saved");
    }
}
```

---

## Benchmark Harness: Measure Before You Decide

Run this after migration and compare against your ComPDFKit baseline numbers:

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using IronPdf;

class IronPdfBenchmark
{
    // Reuse renderer — important for fair throughput measurement
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    static async Task Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        int iterations = 50;

        // Sequential benchmark
        Console.WriteLine($"Sequential HTML-to-PDF ({iterations} iterations)...");
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var pdf = await _renderer.RenderHtmlAsPdfAsync(GetHtml(i));
            // Don't save to disk — measures render time only
        }
        sw.Stop();
        double seqAvg = (double)sw.ElapsedMilliseconds / iterations;
        Console.WriteLine($"Sequential avg: {seqAvg:F1} ms/render");

        // Parallel benchmark
        Console.WriteLine($"\nParallel HTML-to-PDF ({iterations} iterations, 4-way)...");
        sw.Restart();
        var tasks = new List<Task>();
        // https://ironpdf.com/examples/parallel/
        for (int i = 0; i < iterations; i++)
        {
            int j = i;
            tasks.Add(Task.Run(async () =>
            {
                using var pdf = await _renderer.RenderHtmlAsPdfAsync(GetHtml(j));
            }));
            if (tasks.Count >= 4)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }
        if (tasks.Count > 0) await Task.WhenAll(tasks);
        sw.Stop();
        double parAvg = (double)sw.ElapsedMilliseconds / iterations;
        Console.WriteLine($"Parallel avg: {parAvg:F1} ms/render effective");
    }

    static string GetHtml(int i) => $@"<html>
        <body style='font-family:Arial'>
            <h1>Document #{i}</h1>
            <p>Generated at {DateTime.Now:O}</p>
        </body>
    </html>";
}
```

Compare `seqAvg` and `parAvg` against your ComPDFKit baseline measurements.

---

## Critical Migration Notes

### SDK Initialization vs License Key

ComPDFKit uses an SDK initialization step (`CPDFSDKVerifier.LicenseVerify(...)`) that must run before any document operation. IronPDF's license is a static string set once. The initialization model is simpler but the pattern in web applications differs:

```csharp
// Startup.cs / Program.cs — set once, early
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_KEY");
```

Don't set the license key inside hot paths or per-request code.

### Manual Memory Cleanup

ComPDFKit requires `document.Release()` for every `CPDFDocument` instance (and `Release()` on `CPDFPage` / `CPDFTextPage` handles you walk). Missing one leaks the underlying native object. IronPDF objects are managed — `using` is optional but works with `PdfDocument`. Remove every `Release()` call when migrating.

### Page Indexing

Both libraries use 0-based page indexing, so loops that walk pages from 0 to `PageCount - 1` carry over unchanged. The difference is access shape: `document.PageAtIndex(n)` in ComPDFKit becomes `pdf.Pages[n]` in IronPDF.

### Annotation Round-Trips

If your application creates annotations and expects them to survive PDF round-trips (save, reload, verify), test annotation persistence with IronPDF before committing to the migration. Annotation formats are standardized in PDF, but library implementations differ in which annotation types and properties they preserve.

---

## Performance Considerations

### Chromium Renderer Process

IronPDF spawns a Chromium renderer process. This process startup is the largest latency contributor for the first render. Reusing `ChromePdfRenderer` across requests amortizes this cost. In a web application, register it as a singleton.

### Memory Under Load

Run a memory profile under your target concurrent load. The Chromium renderer's memory behavior under concurrent requests is different from an in-process renderer. Test at your expected peak concurrency, not just sequentially.

### Disk vs Memory Output

If your downstream process consumes the PDF as bytes (upload to S3, email attachment), avoid writing to disk:

```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
byte[] bytes = pdf.BinaryData; // No disk write

// Or stream directly
using var stream = new MemoryStream();
pdf.Stream.CopyTo(stream);
```

See [memory stream docs](https://ironpdf.com/how-to/pdf-memory-stream/) for patterns.

### Async Parallel Workloads

See [async rendering docs](https://ironpdf.com/how-to/async/) and [parallel examples](https://ironpdf.com/examples/parallel/) for patterns that scale under load. The benchmark harness above gives you a starting measurement point.

---

## Migration Checklist

### Pre-Migration
- [ ] Run benchmark harness against ComPDFKit — record baseline numbers
- [ ] Run `rg "ComPDFKit|CPDFDocument" --type cs -l` to find affected files
- [ ] Identify all `CPDFSDKVerifier.LicenseVerify(...)` and `Release()` calls
- [ ] Check annotation usage — test IronPDF annotation parity
- [ ] Check form field editing — IronPDF exposes `pdf.Form.SetFieldValue`
- [ ] Confirm .NET target version compatibility with IronPDF
- [ ] Test IronPDF in parallel with ComPDFKit before removing ComPDFKit
- [ ] Identify any 32-bit / platform-specific project configurations

### Code Migration
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `CPDFSDKVerifier.LicenseVerify(...)` with `IronPdf.License.LicenseKey = "..."`
- [ ] Remove all `.Release()` calls on `CPDFDocument`, `CPDFPage`, `CPDFTextPage`
- [ ] Replace `using ComPDFKit.*` with `using IronPdf`
- [ ] Replace `CPDFDocument.InitWithFilePath()` with `PdfDocument.FromFile()`
- [ ] Replace `WriteToFilePath()` with `SaveAs()`
- [ ] Replace manual layout / cloud Conversion calls with `ChromePdfRenderer.RenderHtmlAsPdf()`
- [ ] Replace `ImportPagesAtIndex` merges with `PdfDocument.Merge()`
- [ ] Replace `InitWatermark` + `CPDFWatermark` with `pdf.ApplyWatermark(html)`
- [ ] Replace `document.Encrypt(...)` with `SecuritySettings` properties
- [ ] Replace `document.RemovePage(i)` with `pdf.RemovePages(i)`

### Testing
- [ ] Run IronPDF benchmark harness — compare to ComPDFKit baseline
- [ ] Visual comparison: HTML-to-PDF output quality and CSS rendering
- [ ] Memory profile under target peak concurrency
- [ ] Annotation round-trip verification (create, save, reload)
- [ ] Password protection opens with correct credentials
- [ ] Merge page count and order verification
- [ ] Linux/cloud deployment test if applicable

### Post-Migration
- [ ] Remove ComPDFKit NuGet packages (`ComPDFKit.NetCore` / `ComPDFKit.NetFramework`) and any native SDK files from build
- [ ] Update CI/CD build agent configuration
- [ ] Update IronPDF benchmark comparison doc for future reference
- [ ] Update internal documentation

---

## That's the Migration

The migration from ComPDFKit to IronPDF is most straightforward when your use case is server-side generation — HTML-to-PDF, merge, stamp, encrypt. The rougher edges are annotation workflows and any code that depended on ComPDFKit's native rendering pipeline directly.

The benchmark harness matters here more than in most migrations because the performance characteristics of a Chromium-backed renderer versus a native PDF SDK are architecturally different. Don't assume — measure.

**Question for the comments:** What's the largest throughput you've achieved with a .NET PDF renderer in production — and how did you get there? Renderer pooling, async batching, pre-warmed processes? Curious about real-world approaches.
