---
title: "PDFium to IronPDF: three steps and you're done"
published: false
tags: dotnet, csharp, pdf, migration
---

The .NET ecosystem has no official Google-supplied binding for PDFium — instead, four community wrappers (PdfiumViewer, PdfiumViewer.Updated, PDFiumCore, and Patagames Pdfium.Net.SDK) wrap the C++ engine with varying degrees of activity and feature coverage. PdfiumViewer itself was archived in August 2019 with its last NuGet release in November 2017, so teams that depend on it are sitting on a wrapper whose upstream is frozen. Add the fact that PDFium has no HTML parser, and the moment your service needs HTML-to-PDF generation is the moment to assess alternatives.

This article covers migrating from PDFium-based .NET integrations to IronPDF, with a benchmark framing: what to measure, how to measure it, and what the code changes look like.

---

## What to measure before you commit

Benchmark data from a migration article would be fabricated — don't trust numbers from migration guides. Run your own benchmarks in your environment against your actual templates. Here's the scaffold:

### Benchmark scaffold (run for both libraries)

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using IronPdf;

class PdfBenchmark
{
    // Replace html with your actual production template content
    static readonly string TestHtml = System.IO.File.Exists("test_template.html")
        ? System.IO.File.ReadAllText("test_template.html")
        : "<html><body><h1>Test Document</h1><p>Paragraph content for benchmark.</p></body></html>";

    static void Main(string[] args)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        const int warmupRounds = 5;
        const int timedRounds  = 50;

        var renderer = new ChromePdfRenderer();

        // Warm up — don't time cold-start
        Console.WriteLine("Warming up...");
        for (int i = 0; i < warmupRounds; i++)
        {
            using var warmup = renderer.RenderHtmlAsPdf(TestHtml);
        }

        // Timed single-threaded renders
        var times = new List<long>();
        var sw    = new Stopwatch();

        Console.WriteLine($"Running {timedRounds} renders...");
        for (int i = 0; i < timedRounds; i++)
        {
            sw.Restart();
            using var pdf = renderer.RenderHtmlAsPdf(TestHtml);
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        times.Sort();
        Console.WriteLine($"\n--- Single-threaded results ({timedRounds} renders) ---");
        Console.WriteLine($"Median:  {times[timedRounds / 2]}ms");
        Console.WriteLine($"P95:     {times[(int)(timedRounds * 0.95)]}ms");
        Console.WriteLine($"P99:     {times[(int)(timedRounds * 0.99)]}ms");
        Console.WriteLine($"Min:     {times[0]}ms");
        Console.WriteLine($"Max:     {times[timedRounds - 1]}ms");
        Console.WriteLine($"Average: {Average(times):F1}ms");
    }

    static double Average(List<long> list)
    {
        long sum = 0;
        foreach (var v in list) sum += v;
        return (double)sum / list.Count;
    }
}
```

### Concurrent benchmark scaffold

```csharp
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IronPdf;

class ConcurrentBenchmark
{
    static async Task Main(string[] args)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        const int concurrentTasks = 8;  // adjust to your expected peak
        const int totalRenders    = 80;
        string html = "<html><body><h1>Concurrent Test</h1></body></html>";

        int completed = 0;
        var sw = Stopwatch.StartNew();

        // Semaphore to bound concurrency
        var sem = new SemaphoreSlim(concurrentTasks);

        await Task.WhenAll(
            Enumerable.Range(0, totalRenders).Select(async i =>
            {
                await sem.WaitAsync();
                try
                {
                    // One renderer per task — explicit thread-safety pattern
                    var renderer = new ChromePdfRenderer();
                    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
                    Interlocked.Increment(ref completed);
                }
                finally
                {
                    sem.Release();
                }
            })
        );

        sw.Stop();
        double throughput = totalRenders / sw.Elapsed.TotalSeconds;
        Console.WriteLine($"Completed {totalRenders} concurrent renders");
        Console.WriteLine($"Wall clock: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"Throughput: {throughput:F1} renders/sec");
    }
}
```

### What metrics to collect

| Metric | Why | How |
|---|---|---|
| Median render time | Typical-case latency | Stopwatch, 50+ iterations |
| P95 render time | Tail latency for SLAs | Sort + index |
| Memory per render | Container sizing | GC.GetTotalMemory before/after |
| Concurrent throughput | Scaling capacity | WhenAll with SemaphoreSlim |
| Output file size | Storage cost | `new FileInfo(path).Length` |
| Cold start time | Serverless / first-request | Time first render separately |
| CSS fidelity | Visual accuracy | Side-by-side visual review |

PDFium wrappers do not natively convert HTML to PDF, so the HTML-side benchmark is IronPDF-only. For PDF-to-image rendering and text extraction, run the equivalent operations under both libraries against your production corpus and compare. Don't compare numbers from different environments.

---

## Why migrate (without drama)

Nine reasons teams make this transition:

1. **HTML-to-PDF gap** — PDFium renders and parses PDFs; it has no HTML parser, so no wrapper can convert HTML or URLs to PDF. Teams that need both add a second tool. Migration consolidates this.
2. **Wrapper maintenance signal** — PdfiumViewer was archived in August 2019. The maintained PdfiumViewer.Updated fork, PDFiumCore, and the commercial Patagames Pdfium.Net.SDK each move at different paces, with different feature surfaces.
3. **Native binary management** — architecture-specific `pdfium.dll` / `libpdfium.so` / `libpdfium.dylib` must be sourced, bundled, and updated separately from your .NET packages.
4. **P/Invoke fragility** — native interop breaks across .NET runtime upgrades, OS updates, and architecture changes in non-obvious ways.
5. **Licensing variance across wrappers** — PdfiumViewer is Apache 2.0; Patagames Pdfium.Net.SDK is commercial. PDFium itself is BSD-3-Clause, but build artifacts and wrappers carry their own terms.
6. **ARM64 deployment** — ARM64 PDFium binaries must be sourced separately per RID.
7. **Feature gaps in the free wrappers** — merge, split, watermarks, headers/footers, encryption, and form fill are not exposed in PdfiumViewer or PDFiumCore. Patagames Pdfium.Net.SDK fills in some of these for a license fee.
8. **Thread safety** — PDFium has documented thread-safety constraints at the C library level; the per-wrapper guarantees vary.
9. **Docker complexity** — native binary distribution in multi-arch Docker builds adds maintenance overhead.

### Comparison table

| Aspect | PDFium (.NET wrapper) | IronPDF |
|---|---|---|
| Focus | PDF rendering + parsing | HTML-to-PDF + PDF manipulation |
| Pricing | Wrapper-dependent (open source through commercial) | Commercial — see ironsoftware.com |
| API Style | P/Invoke wrapper — varies | In-process .NET library |
| Learning Curve | High (wrapper-dependent) | Medium |
| HTML Rendering | None (no HTML parser) | Chromium-based |
| Page Indexing | 0-based | 0-based |
| Thread Safety | PDFium constraints | Renderer instance reuse |
| Namespace | `PdfiumViewer`, `PDFiumCore`, `Patagames.Pdf` | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | PDFium approach | Effort to migrate |
|---|---|---|
| Render PDF page to image | PDFium native strength | Map `Render(page, w, h, dpiX, dpiY, flags)` → `RasterizeToImageFiles(path, DPI)` |
| Text extraction | `GetPdfText(pageIndex)` per page | Low — `pdf.ExtractAllText()` or `pdf.Pages[i].Text` |
| HTML to PDF | None (no HTML parser) | Low (native in IronPDF) |
| Open/parse existing PDF | `PdfDocument.Load(path)` | Low — `PdfDocument.FromFile(path)` |
| Merge PDFs | Free wrappers: none; Patagames: partial | Low (native in IronPDF) |
| Watermark | Not in free wrappers | Low |
| Password protection | Patagames only | Low |
| Page count / metadata | `PageCount`, `PageSizes[i]` | Low |
| Native binary management | Required (per RID) | Eliminated |
| ARM64 deployment | Manual binary sourcing | Simplified |
| Thread-safe concurrent use | Constrained | Explicit per-thread pattern |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| Primary use: render PDF pages to images | PDFium is well-suited; IronPDF offers `RasterizeToImageFiles` for the same job |
| Need HTML-to-PDF + PDF rendering | IronPDF covers both in one library |
| Native binary management is primary pain | IronPDF via NuGet eliminates this |
| Open source constraint | PdfiumViewer (Apache 2.0); IronPDF is commercial |

---

## Before you start

### Prerequisites

- .NET 6+ target
- Identify your specific PDFium wrapper package (PdfiumViewer, PdfiumViewer.Updated, PDFiumCore, or Patagames Pdfium.Net.SDK)
- HTML templates and PDF test corpus for render comparison

### Find PDFium references in your codebase

```bash
# Find the wrapper package — check .csproj first
grep -rE "PdfiumViewer|PDFiumCore|Patagames\.Pdf" --include="*.csproj" .

# Find all usage in C# files
rg -E "PdfiumViewer|PDFiumCore|Patagames\.Pdf|PdfDocument\.Load|\.Render\(" --type cs

# Find P/Invoke declarations if using direct interop
rg -E "DllImport.*pdfium|extern.*FPDF" --type cs -i

# Find native binary references
find . -name "pdfium.dll" -o -name "libpdfium.so" -o -name "libpdfium.dylib"
```

### Remove PDFium wrapper, install IronPDF

```bash
# Remove whichever wrapper you used
dotnet remove package PdfiumViewer
dotnet remove package PdfiumViewer.Updated
dotnet remove package PDFiumCore
dotnet remove package Pdfium.Net.SDK

# Install IronPDF
dotnet add package IronPdf
dotnet restore
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (PdfiumViewer — Apache 2.0 wrapper, no license key):**
```csharp
// PdfiumViewer is Apache 2.0; no in-process license key.
// Patagames Pdfium.Net.SDK is commercial and ships its own activation API.
using PdfiumViewer;
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// License guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
// Use the namespace that matches the wrapper you installed:
using PdfiumViewer;       // PdfiumViewer / PdfiumViewer.Updated
// using PDFiumCore;      // Dtronix PDFiumCore (P/Invoke bindings)
// using Patagames.Pdf;   // Patagames Pdfium.Net.SDK
// using Patagames.Pdf.Net;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
using IronPdf.Security;
```

### Step 3: Basic PDF operation

**Before (PdfiumViewer — load PDF and rasterize page to image):**
```csharp
using PdfiumViewer;
using System.Drawing;

class BasicPdfiumExample
{
    static void Main()
    {
        using var document = PdfDocument.Load("input.pdf");

        int pageCount = document.PageCount;
        var size = document.PageSizes[0];

        // Render page 0 at 2x scale, 96 DPI
        using var bitmap = document.Render(
            page: 0,
            width:  (int)(size.Width  * 2),
            height: (int)(size.Height * 2),
            dpiX: 96, dpiY: 96,
            flags: PdfRenderFlags.Annotations);

        bitmap.Save("page1.png");
    }
}
```

**After (IronPDF — HTML-to-PDF or PDF manipulation):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// HTML to PDF (PDFium has no HTML parser):
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/

// Open existing PDF and rasterize at 150 DPI:
var existing = PdfDocument.FromFile("input.pdf");
existing.RasterizeToImageFiles("page_*.png", DPI: 150);

// Text extraction:
string text = existing.ExtractAllText();
// Guide: https://ironpdf.com/how-to/extract-text-and-images/
```

---

## API mapping tables

### Namespace mapping

| PDFium wrapper | IronPDF | Notes |
|---|---|---|
| `PdfiumViewer` / `PDFiumCore` / `Patagames.Pdf` | `IronPdf` | Core namespace |
| `PdfiumViewer` (rendering) | `IronPdf.Rendering` | Render options |
| N/A | `IronPdf.Editing` | Stamp, annotate |

### Core class mapping

| PdfiumViewer | IronPDF | Description |
|---|---|---|
| `PdfiumViewer.PdfDocument` | `IronPdf.PdfDocument` | Same simple name, different namespace |
| `PdfRenderer` (WinForms control) | _(not applicable)_ | PdfiumViewer also ships UI controls; IronPDF is headless |
| _(not available)_ | `ChromePdfRenderer` | HTML / URL → PDF |
| _(not available)_ | `HtmlHeaderFooter` | Headers/footers |

### Document loading methods

| Operation | PdfiumViewer | IronPDF |
|---|---|---|
| Open existing PDF | `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` |
| Open from stream | `PdfDocument.Load(stream)` | `PdfDocument.FromStream(stream)` |
| Open from bytes | wrap bytes in `MemoryStream` | `PdfDocument.FromBinaryData(bytes)` |
| HTML to PDF | Not available | `renderer.RenderHtmlAsPdf(html)` |
| URL to PDF | Not available | `renderer.RenderUrlAsPdf(url)` |

### Page operations

| Operation | PdfiumViewer | IronPDF |
|---|---|---|
| Page count | `document.PageCount` | `pdf.PageCount` |
| Page size | `document.PageSizes[i]` (`SizeF`) | `pdf.Pages[i].Width / Height` |
| Extract text | `document.GetPdfText(i)` | `pdf.ExtractAllText()` / `pdf.Pages[i].Text` |
| Render to image | `document.Render(i, w, h, dpiX, dpiY, flags)` | `pdf.RasterizeToImageFiles(path, DPI)` |

### Merge/split operations

| Operation | PDFium wrapper | IronPDF |
|---|---|---|
| Merge | Free wrappers: none; Patagames: partial | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | Free wrappers: none; Patagames: partial | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

### 1. HTML to PDF

**Before (PDFium — no HTML parser; teams add a second tool):**
```csharp
using System;
using System.Diagnostics;
using System.IO;

class HtmlToPdfWithSecondTool
{
    static void Main()
    {
        // PDFium cannot convert HTML to PDF — no .NET wrapper exposes this
        // because the engine itself has no HTML parser. Teams pair PDFium with
        // a separate engine (wkhtmltopdf, headless Chromium, IronPDF, etc.).

        string html     = "<html><body><h1>Invoice #1234</h1></body></html>";
        string tempHtml = Path.GetTempFileName() + ".html";
        string tempPdf  = Path.GetTempFileName() + ".pdf";

        File.WriteAllText(tempHtml, html);

        var psi = new ProcessStartInfo("wkhtmltopdf", $"\"{tempHtml}\" \"{tempPdf}\"")
        {
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var proc = Process.Start(psi)!;
        proc.WaitForExit(30_000);

        if (proc.ExitCode != 0)
            throw new Exception($"wkhtmltopdf failed: {proc.StandardError.ReadToEnd()}");

        byte[] pdfBytes = File.ReadAllBytes(tempPdf);
        File.Delete(tempHtml);
        File.Delete(tempPdf);
        Console.WriteLine("Generated PDF via secondary tool");
    }
}
```

**After (IronPDF — single library for both):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>"
);
pdf.SaveAs("invoice.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (PdfiumViewer / PDFiumCore — merge not exposed):**
```csharp
// Open-source PDFium wrappers (PdfiumViewer, PDFiumCore) do not expose
// document-merge APIs. A common fallback is PdfSharp:

using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;

class MergeExample
{
    static void Main()
    {
        using var output = new PdfDocument();
        foreach (string path in new[] { "part1.pdf", "part2.pdf" })
        {
            using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            foreach (PdfPage page in input.Pages)
                output.AddPage(page);
        }
        output.Save("merged.pdf");
        Console.WriteLine("Merged.");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var merged = PdfDocument.Merge(
    PdfDocument.FromFile("part1.pdf"),
    PdfDocument.FromFile("part2.pdf")
);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (PdfiumViewer / PDFiumCore — no stamping primitive):**
```csharp
// PDFium itself has no high-level watermark or stamp API, and neither
// PdfiumViewer nor PDFiumCore exposes one. iTextSharp is a common fallback:

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;

class WatermarkExample
{
    static void Main()
    {
        using var reader  = new PdfReader("input.pdf");
        using var fs      = new FileStream("watermarked.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs);

        var font = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
        for (int page = 1; page <= reader.NumberOfPages; page++)
        {
            var cb = stamper.GetOverContent(page);
            cb.BeginText();
            cb.SetFontAndSize(font, 60);
            cb.SetColorFill(new BaseColor(180, 180, 180));
            cb.ShowTextAligned(Element.ALIGN_CENTER, "CONFIDENTIAL", 300, 420, 45);
            cb.EndText();
        }
        Console.WriteLine("Watermarked.");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("input.pdf");
pdf.ApplyWatermark(
    "<div style='color:lightgray; font-size:60px; opacity:0.3; transform:rotate(-45deg);'>CONFIDENTIAL</div>",
    45,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);
pdf.SaveAs("watermarked.pdf");
// Guide: https://ironpdf.com/how-to/custom-watermark/
```

---

### 4. Password protection

**Before (PdfiumViewer / PDFiumCore — encryption not exposed):**
```csharp
// Free PDFium wrappers (PdfiumViewer, PDFiumCore) do not expose
// encryption/permissions APIs. (Patagames Pdfium.Net.SDK exposes some
// security features.) iTextSharp is a common fallback for the free wrappers:

using iTextSharp.text.pdf;
using System.IO;
using System.Text;

class SecurityExample
{
    static void Main()
    {
        byte[] userPass  = Encoding.ASCII.GetBytes("readonly");
        byte[] ownerPass = Encoding.ASCII.GetBytes("admin");

        using var reader  = new PdfReader("input.pdf");
        using var fs      = new FileStream("secured.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs, '\0', false);
        stamper.SetEncryption(
            userPass, ownerPass,
            PdfWriter.ALLOW_PRINTING,
            PdfWriter.ENCRYPTION_AES_128
        );
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Security;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("input.pdf");
pdf.SecuritySettings.UserPassword  = "readonly";
pdf.SecuritySettings.OwnerPassword = "admin";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### PDF-to-image rendering

PDFium's primary strength — rendering PDF pages to bitmap images — has a direct counterpart in IronPDF via `RasterizeToImageFiles`. The difference is the parameter model: PdfiumViewer takes explicit pixel dimensions plus `dpiX` / `dpiY`, while IronPDF takes a DPI value and computes the bitmap size from the page's points.

```csharp
// PdfiumViewer: per-page Render call with pixel size + DPI
using var document = PdfDocument.Load("input.pdf");
var size = document.PageSizes[0];
using var bitmap = document.Render(
    page: 0,
    width:  (int)(size.Width  * 2),
    height: (int)(size.Height * 2),
    dpiX: 96, dpiY: 96,
    flags: PdfRenderFlags.Annotations);
bitmap.Save("page1.png");

// IronPDF: DPI-based rendering across the whole document
var pdf = IronPdf.PdfDocument.FromFile("input.pdf");
pdf.RasterizeToImageFiles("page_*.png", DPI: 150);
```

Use the conversion `IronPDF DPI = 72 × PDFium scale factor` (PDFium scale 1.0 = 72 DPI, scale 2.0 = 144 DPI, scale 3.0 = 216 DPI).

### Native binary cleanup

```bash
# Find PDFium native binaries
find . -name "pdfium*.dll" -o -name "libpdfium*.so" -o -name "libpdfium*.dylib"

# Remove from Docker COPY commands
rg -E "pdfium|libpdfium" Dockerfile*

# Remove from build output directories
find . -path "*/bin/*pdfium*" -o -path "*/bin/*libpdfium*"

# Delete runtime-specific folders
rm -rf x86/ x64/ runtimes/
```

### Page indexing

Both PDFium (at the C level) and IronPDF use 0-based page indexing. PdfiumViewer's `Render(page, ...)` and `GetPdfText(pageIndex)` are 0-based, as is `IronPdf.PdfDocument.Pages[i]`.

```csharp
// IronPDF: 0-based throughout
var firstPage = pdf.Pages[0];
var lastPage  = pdf.Pages[pdf.PageCount - 1];
```

### Text extraction parity

If text extraction is a key feature, compare IronPDF's output against PdfiumViewer's `GetPdfText` output on your actual document corpus before committing. PdfiumViewer exposes per-page raw text without layout metadata; IronPDF provides text in logical / visual order.

```csharp
var pdf = PdfDocument.FromFile("document.pdf");

// Full document text
string allText = pdf.ExtractAllText();

// Per-page
foreach (var page in pdf.Pages)
    Console.WriteLine($"Page {page.PageIndex}: {page.Text.Length} chars");

// Guide: https://ironpdf.com/how-to/extract-text-and-images/
```

---

## Performance considerations

### Run the benchmark scaffold first

The numbers that matter are yours, not anyone else's. Run the scaffold at the top of this article against your actual HTML templates and PDF workloads before making a capacity decision.

### Renderer reuse for HTML-to-PDF

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

foreach (var html in batchTemplates)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{Guid.NewGuid()}.pdf");
}
```

### Async parallel rendering

```csharp
// Separate renderer per task
await Task.WhenAll(
    htmlItems.Select(async html =>
    {
        var renderer = new ChromePdfRenderer();
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        await pdf.SaveAsAsync($"{Guid.NewGuid()}.pdf");
    })
);
// Parallel guide: https://ironpdf.com/examples/parallel/
```

### Memory profiling pattern

```csharp
long memBefore = GC.GetTotalMemory(forceFullCollection: true);

using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");

long memAfter = GC.GetTotalMemory(forceFullCollection: false);
Console.WriteLine($"Memory delta: {(memAfter - memBefore) / 1024:N0} KB");
```

---

## Migration checklist

### Pre-migration

- [ ] Identify your specific PDFium .NET wrapper package (PdfiumViewer, PdfiumViewer.Updated, PDFiumCore, or Patagames Pdfium.Net.SDK) and version
- [ ] Audit all features in use: HTML gen? Page rendering? Text extraction? Merge?
- [ ] Identify secondary libraries added alongside PDFium (wkhtmltopdf, iTextSharp, PdfSharp, etc.)
- [ ] Find all native binaries: `find . -name "pdfium*" -o -name "libpdfium*"`
- [ ] Confirm the licensing terms of the specific wrapper build you ship
- [ ] Verify IronPDF .NET target framework compatibility
- [ ] Run benchmark scaffold in current environment (baseline measurement)
- [ ] Set up IronPDF trial license in dev environment

### Code migration

- [ ] Remove PDFium wrapper NuGet package(s)
- [ ] Remove secondary libraries (wkhtmltopdf, iTextSharp, PdfSharp) if only supplementing
- [ ] Add `IronPdf` NuGet package
- [ ] Replace wrapper namespace imports
- [ ] Replace license/initialization
- [ ] Replace HTML-to-PDF secondary tool with `ChromePdfRenderer`
- [ ] Replace `GetPdfText` calls with `ExtractAllText` / `Pages[i].Text`
- [ ] Replace `Render(page, w, h, dpiX, dpiY, flags)` with `RasterizeToImageFiles(path, DPI)`
- [ ] Replace merge operations
- [ ] Replace watermark operations
- [ ] Replace security/encryption operations

### Testing

- [ ] Render HTML templates and compare output
- [ ] Test text extraction on representative PDF corpus
- [ ] Test merge with representative document sets
- [ ] Test watermark on multi-page documents
- [ ] Test security (correct and incorrect credentials)
- [ ] Run concurrent benchmark and compare against baseline
- [ ] Verify no native binary load errors in all environments

### Post-migration

- [ ] Remove native PDFium binaries from project directories
- [ ] Remove COPY/ADD references to PDFium binaries in Dockerfiles
- [ ] Update deployment documentation
- [ ] Record benchmark results for team reference

---

## That's the Migration

PDFium's strength is PDF rendering and parsing. If PDF-to-image rendering is your primary use case, IronPDF's `RasterizeToImageFiles` covers it directly — and you gain HTML-to-PDF, merge, watermark, headers/footers, encryption, and forms in the same library. The benchmark scaffold helps you understand whether the consolidated approach fits your performance requirements before you commit.

**Which feature was hardest to replicate when moving away from PDFium, and why?** Particularly interested in teams who had both PDF rendering and HTML generation in the same service — how did you handle the split, or did you find a consolidated path?
