---
title: "Switching from PDFreactor to IronPDF: copy-paste and ship"
published: false
tags: dotnet, csharp, pdf, migration
---

The concurrency problem shows up at scale. The job queue is filling faster than the renderer can drain it, you spin up more application instances, and then you hit the licensing ceiling or discover that the renderer isn't safe to share across threads without external coordination. You're now building a rate-limiting wrapper around a PDF library, which is not what you set out to do.

This article covers migrating from PDFreactor to IronPDF. You'll have benchmark-comparable migration code for HTML-to-PDF, merge, watermark, and password protection, plus a concurrency section that covers the patterns teams typically reach for when scaling up.

---

## Why Migrate (Without Drama)

Teams evaluating PDFreactor alternatives commonly encounter these conditions. Not all apply to every version:

1. **Java runtime dependency** — every deployment needs a JRE, which increases container base image size and adds a runtime to manage.
2. **Service architecture overhead** — PDFreactor runs as a separate Java/Jetty Web Service process, requiring health checks, startup coordination, and inter-process error handling.
3. **Concurrent request throughput** — request queuing to the PDFreactor server introduces latency under load; scaling requires multiple server instances.
4. **Licensing model at scale** — PDFreactor uses commercial licensing tied to the server instance, which can become a planning constraint before infrastructure limits do.
5. **Docker/container complexity** — Java + PDFreactor server + .NET app in one container or separate sidecar adds orchestration complexity.
6. **Network hop latency** — the .NET-to-PDFreactor HTTP call adds measurable latency per document, especially at high volume.
7. **CSS Paged Media specificity** — PDFreactor's CSS Paged Media compliance is high-quality for print-exact typographic work, but that same specificity can require significant HTML/CSS preparation for web-derived content.
8. **Cold start time** — PDFreactor server startup time affects warm-up and auto-scaling behavior.
9. **Single point of failure** — if the PDFreactor service goes down or becomes unresponsive, PDF generation for the entire application stops.
10. **Licensing cost growth** — as throughput grows, per-server licensing scales with the number of service instances, which is a common renewal-trigger for evaluation.

### Comparison Table

| Aspect | PDFreactor | IronPDF |
|---|---|---|
| Focus | CSS Paged Media / print-quality HTML-to-PDF | General HTML-to-PDF + PDF manipulation |
| Pricing | Commercial per-server license | Commercial license — see ironsoftware.com |
| API Style | .NET wrapper over HTTP to Java server | Native .NET objects; no external process |
| Learning Curve | Medium; CSS Paged Media concepts help | Low for .NET developers |
| HTML Rendering | CSS Paged Media W3C compliant | Chromium-based |
| Page Indexing | 1-based (CSS `counter(page)`) | 0-based |
| Thread Safety | Server-side; concurrent requests via HTTP | Each `ChromePdfRenderer` is a separate Chromium instance |
| Namespace | `RealObjects.PDFreactor.Webservice.Client` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | PDFreactor | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | `pdfReactor.Convert(config)` with `config.Document = html` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | `pdfReactor.Convert(config)` with `config.Document = url` | `ChromePdfRenderer.RenderUrlAsPdfAsync()` | Low |
| Save to bytes/file | `result.Document` (byte[]) | `pdf.SaveAs()` / `pdf.BinaryData` | Low |
| Custom page size | `config.PageFormat` / CSS `@page` | `RenderingOptions.PaperSize` | Low |
| Headers and footers | CSS Paged Media `@page` margin boxes | `RenderingOptions.HtmlHeader/Footer` | Medium |
| JavaScript rendering | `config.EnableJavaScript`, `JavaScriptSettings` | Built-in Chromium JS + `WaitFor` settings | Medium |
| Merge PDFs | Not native — generate sections, merge via secondary library | `PdfDocument.Merge()` | Medium |
| Watermark | CSS `@page` background or running elements | `TextStamper` / `ImageStamper` | Medium |
| Password protection | `config.Encryption` with `UserPassword`/`OwnerPassword` | `pdf.SecuritySettings` | Medium |
| Async parallel | `ConvertAsync` returns documentId; poll `GetProgress`/`GetDocument` | `Task.WhenAll` + async render methods | Medium-High |
| CSS Paged Media features | Native support | Limited to CSS print standards via Chromium | High |
| Custom hyphenation / typographic spacing | Supported | Not a direct equivalent | High |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Print-exact typographic output (books, legal docs with strict CSS Paged Media requirements) | PDFreactor may be better suited; evaluate carefully |
| Web-derived content, dashboard output, reports from HTML templates | Switch — Chromium fidelity is appropriate; simpler deployment |
| Kubernetes/Docker scaling with throughput requirements | Switch — no Java sidecar, no inter-process latency |
| Concurrent high-volume rendering under tight licensing constraints | Evaluate IronPDF's licensing model at target volume |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- Java runtime removal is optional — decommission after migration is validated
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All PDFreactor References

```bash
# Search for PDFreactor API calls
rg -l "PDFreactor|RealObjects" --type cs
rg "PDFreactor|RealObjects" --type cs -n

# Check project files
grep -r "PDFreactor\|RealObjects" *.csproj **/*.csproj 2>/dev/null

# Find any PDFreactor server config files
find . -name "pdfreactor*.xml" -o -name "pdfreactor*.json" 2>/dev/null

# Find CSS files that use @page or CSS Paged Media rules
rg "@page|prince-|pdfreactor-" --type css
```

### Uninstall / Install

```bash
# PDFreactor is NOT distributed via NuGet. The .NET wrapper ships as
# PDFreactor.dll inside the PDFreactor install folder
# (clients/netstandard2/bin or clients/netframework40/bin).
# Remove the assembly reference from your .csproj and uninstall the
# PDFreactor Web Service (Java/Jetty, default port 9423).

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// Set before any IronPDF call — https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using RealObjects.PDFreactor.Webservice.Client;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic HTML-to-PDF

**Before:**
```csharp
using RealObjects.PDFreactor.Webservice.Client;
using System.IO;

class Program
{
    static void Main()
    {
        // Default service URL: http://localhost:9423/service/rest
        PDFreactor pdfReactor = new PDFreactor();

        string html = "<html><body><h1>Hello World</h1></body></html>";

        Configuration config = new Configuration();
        config.Document = html;

        Result result = pdfReactor.Convert(config);
        File.WriteAllBytes("output.pdf", result.Document);
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

// No external service required — renders locally
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Hello World</h1></body></html>");
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Benchmark Reference Points

> **Important:** The figures below are illustrative benchmark structures — not measured values. Run these patterns in your own environment against your actual HTML templates. Hardware, template complexity, JavaScript usage, and image count all significantly affect render times. Do not publish these as performance claims without measuring.

### Benchmark Harness

```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Simple HTML template for baseline measurement
var baselineHtml = @"
    <html>
    <head><style>body { font-family: Arial; }</style></head>
    <body>
        <h1>Benchmark Document</h1>
        <p>Paragraph content for render timing test.</p>
        <table>
            <tr><th>Column A</th><th>Column B</th></tr>
            <tr><td>Value 1</td><td>Value 2</td></tr>
        </table>
    </body>
    </html>";

async Task<(double avgMs, double p95Ms)> BenchmarkIronPdf(string html, int iterations = 20)
{
    var renderer = new ChromePdfRenderer();
    var times = new List<double>();

    // Warm up — first render may include Chromium initialization overhead
    await renderer.RenderHtmlAsPdfAsync(html);

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
        p95Ms: times[(int)(times.Count * 0.95)]
    );
}

var (avg, p95) = await BenchmarkIronPdf(baselineHtml, iterations: 30);
Console.WriteLine($"IronPDF — Average: {avg:F1}ms, P95: {p95:F1}ms");
Console.WriteLine("Compare these figures against your PDFreactor baseline (including HTTP round-trip)");
```

### Parallel Throughput Benchmark

```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Parallel rendering: https://ironpdf.com/examples/parallel/
async Task<double> BenchmarkParallelThroughput(int concurrency = 10)
{
    var htmlJobs = Enumerable.Range(1, concurrency)
        .Select(i => $"<html><body><h1>Document {i}</h1></body></html>")
        .ToList();

    var sw = Stopwatch.StartNew();

    var tasks = htmlJobs.Select(async html =>
    {
        // Each task uses its own ChromePdfRenderer instance for isolation
        var renderer = new ChromePdfRenderer();
        return await renderer.RenderHtmlAsPdfAsync(html);
    });

    var results = await Task.WhenAll(tasks);
    sw.Stop();

    var totalMs = sw.Elapsed.TotalMilliseconds;
    Console.WriteLine($"{concurrency} docs in parallel: {totalMs:F0}ms ({totalMs / concurrency:F1}ms avg)");
    
    // Dispose results
    foreach (var pdf in results) pdf.Dispose();
    
    return totalMs;
}

// Run at different concurrency levels and compare against PDFreactor throughput
await BenchmarkParallelThroughput(5);
await BenchmarkParallelThroughput(10);
await BenchmarkParallelThroughput(20);

// See: https://ironpdf.com/how-to/async/
```

### Memory Allocation Benchmark

```csharp
using IronPdf;
using System;
using System.Diagnostics;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Check GC pressure under sustained load
static async Task MeasureMemoryProfile(int iterations = 50)
{
    var renderer = new ChromePdfRenderer();
    var html = "<html><body><h1>Memory test</h1></body></html>";

    var beforeGc = GC.GetTotalMemory(forceFullCollection: true);
    
    for (int i = 0; i < iterations; i++)
    {
        // Using 'using' ensures disposal — important for memory management
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        // pdf goes out of scope and is disposed here
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    var afterGc = GC.GetTotalMemory(forceFullCollection: true);

    Console.WriteLine($"Memory delta after {iterations} renders + GC: {(afterGc - beforeGc) / 1024:F1} KB");
}

await MeasureMemoryProfile(iterations: 100);
```

---

## API Mapping Tables

### Namespace Mapping

| PDFreactor .NET SDK | IronPDF | Notes |
|---|---|---|
| `RealObjects.PDFreactor.Webservice.Client` | `IronPdf` | Core namespace |
| `PDFreactor` (service client) | `ChromePdfRenderer` | No external service required |
| `Configuration` | `ChromePdfRenderOptions` | Rendering options |

### Core Class Mapping

| PDFreactor Concept | IronPDF Class | Description |
|---|---|---|
| `PDFreactor` service client | `ChromePdfRenderer` | Renders HTML/URL to PDF |
| `Configuration` object | `ChromePdfRenderOptions` | Page size, margins, JS settings |
| `Result` object | `PdfDocument` | PDF object with manipulation methods |
| N/A (via HTTP calls) | `PdfDocument.Merge()` | Local merge |

### Document Loading Methods

| Operation | PDFreactor | IronPDF |
|---|---|---|
| HTML string | `config.Document = html` | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | `config.Document = url` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML file | Set `config.Document` to file URI | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Existing PDF | N/A — PDFreactor is conversion-only | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | PDFreactor | IronPDF |
|---|---|---|
| Page count | Inspect `result.Document` with a separate PDF library | `pdf.PageCount` |
| Remove page | Not native — requires secondary library | `pdf.RemovePages(index)` |
| Extract page range | Not native — requires secondary library | `pdf.CopyPages(startIndex, endIndex)` |
| Rotate | Not native — requires secondary library | `pdf.RotateAllPages(PdfPageRotation.Clockwise90)` |

### Merge / Split Operations

| Operation | PDFreactor | IronPDF |
|---|---|---|
| Merge | Not native — generate sections, then merge with another library | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not native | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before:**
```csharp
using RealObjects.PDFreactor.Webservice.Client;
using System.IO;

class HtmlToPdfBefore
{
    static void Main()
    {
        // PDFreactor .NET wrapper talks REST to the Java Web Service
        PDFreactor pdfReactor = new PDFreactor("http://localhost:9423/service/rest");

        Configuration config = new Configuration
        {
            Document = @"
                <html>
                <head><style>@page { size: A4; margin: 2cm; }</style></head>
                <body><h1>Report Q4 2024</h1><p>Financial summary here.</p></body>
                </html>"
        };

        Result result = pdfReactor.Convert(config);
        File.WriteAllBytes("report.pdf", result.Document);
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
        @page { size: A4; margin: 2cm; }
        body { font-family: Arial, sans-serif; }
    </style>
    </head>
    <body><h1>Report Q4 2024</h1><p>Financial summary here.</p></body>
    </html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

// No service process — renders locally
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("report.pdf");

Console.WriteLine($"Saved report.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before:**
```csharp
using RealObjects.PDFreactor.Webservice.Client;
using System.Collections.Generic;
using System.IO;

class MergeBefore
{
    static void Main()
    {
        // PDFreactor does not merge PDFs natively — generate each section,
        // then combine the byte arrays with a secondary library.
        var pdfReactor = new PDFreactor("http://localhost:9423/service/rest");

        var sections = new[] {
            "<html><body><h1>Section 1: Overview</h1></body></html>",
            "<html><body><h1>Section 2: Detail</h1></body></html>",
        };

        var pdfs = new List<byte[]>();
        foreach (var html in sections)
        {
            var config = new Configuration { Document = html };
            pdfs.Add(pdfReactor.Convert(config).Document);
        }

        // Combine pdfs[0] and pdfs[1] with a secondary PDF library
        // (PDFreactor itself is conversion-only)
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

// Render sections concurrently: https://ironpdf.com/how-to/async/
var task1 = renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 1: Overview</h1></body></html>");
var task2 = renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 2: Detail</h1></body></html>");

var results = await Task.WhenAll(task1, task2);

// Merge locally: https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("combined-report.pdf");

Console.WriteLine($"Merged: {merged.PageCount} total pages");
```

---

### 3. Watermark

**Before:**
```csharp
using RealObjects.PDFreactor.Webservice.Client;
using System.Collections.Generic;
using System.IO;

class WatermarkBefore
{
    static void Main()
    {
        // PDFreactor watermarks are typically applied via CSS Paged Media:
        // a @page background image, or @page margin-box content.
        var pdfReactor = new PDFreactor("http://localhost:9423/service/rest");

        var config = new Configuration
        {
            Document = "<html><body><h1>Q4 Report</h1></body></html>"
        };

        config.UserStyleSheets = new List<Resource>
        {
            new Resource { Content = @"
                @page {
                    background-image: url('watermark.png');
                    background-position: center;
                    background-repeat: no-repeat;
                    background-size: 50%;
                }
            " }
        };

        Result result = pdfReactor.Convert(config);
        File.WriteAllBytes("draft-report.pdf", result.Document);
    }
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Editing;
using IronSoftware.Drawing;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Q4 Report</h1><p>Content...</p></body></html>"
);

// Programmatic watermark applied post-render
// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = Color.Gray,
    Opacity = 15, // 0-100 integer percentage
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("draft-report.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before:**
```csharp
using RealObjects.PDFreactor.Webservice.Client;
using System.IO;

class PasswordBefore
{
    static void Main()
    {
        var pdfReactor = new PDFreactor("http://localhost:9423/service/rest");

        var config = new Configuration
        {
            Document = "<html><body><h1>Confidential Report</h1></body></html>",
            Encryption = new Encryption
            {
                UserPassword = "open123",
                OwnerPassword = "admin456",
                AllowPrint = true,
                AllowCopy = false
            }
        };

        Result result = pdfReactor.Convert(config);
        File.WriteAllBytes("secured-report.pdf", result.Document);
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Confidential Report</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;

pdf.SaveAs("secured-report.pdf");
Console.WriteLine("Password-protected — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### CSS Paged Media Gap

PDFreactor has deep CSS Paged Media support — running headers/footers via `@page` margin boxes, complex footnotes, table header repetition. These features work in PDFreactor because it implements the W3C CSS Paged Media spec closely.

IronPDF's Chromium renderer supports a subset of CSS print properties — `@page` size and margins, `page-break-*`, `orphans`/`widows`. It does not implement the full CSS Paged Media spec.

If your templates use CSS features like `running()`, `string()`, `@footnote`, or named page flows, audit those templates carefully before migration. You may need to migrate those header/footer patterns to IronPDF's `RenderingOptions.HtmlHeader` / `HtmlFooter` API instead.

```csharp
// PDFreactor CSS running header: @page { @top-center { content: element(header); } }
// IronPDF equivalent: HtmlHeader/Footer via RenderingOptions

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center; font-size:10px;'>{page} of {total-pages}</div>",
    BaseUrl = new Uri(System.IO.Path.GetFullPath(".")).ToString()
};
// https://ironpdf.com/how-to/rendering-options/
```

### Page Indexing

IronPDF uses 0-based page indexing for API methods like `pdf.RemovePages(0)`. PDFreactor reports pages 1-based in CSS counters (`counter(page)`). If you have post-processing logic that indexes by page number, audit those code paths during migration.

### Server Process Cleanup

After migration is validated, decommission the PDFreactor Web Service from Docker images, CI pipelines, and startup scripts. This is where the deployment simplification is realized — removing the Java runtime dependency from container images can significantly reduce image size.

### Async Pattern Alignment

PDFreactor's `ConvertAsync` returns a document ID and requires polling `GetProgress` and `GetDocument` until the job is finished. IronPDF exposes native `Async` methods that return a `Task<PdfDocument>` directly:

```csharp
// PDFreactor pattern (paraphrased):
// string id = pdfReactor.ConvertAsync(config);
// while (!pdfReactor.GetProgress(id).Finished) { Thread.Sleep(500); }
// byte[] bytes = pdfReactor.GetDocument(id);

// IronPDF:
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
// See: https://ironpdf.com/how-to/async/
```

---

## Performance Considerations

### Renderer Warm-Up

The first IronPDF render in a process incurs Chromium startup time. For long-running services, warm up the renderer at startup:

```csharp
using IronPdf;

// Warm up during application startup
var renderer = new ChromePdfRenderer();
await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");

// Now renderer is warm for production traffic
// Assign to DI container, static, or similar pattern
```

### Disposal and Memory

```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();

// Always dispose PdfDocument — especially important under high volume
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// For MemoryStream use: https://ironpdf.com/how-to/pdf-memory-stream/
using var ms = new MemoryStream(pdf.BinaryData);
return ms.ToArray();
```

### Comparing Against PDFreactor Baseline

When benchmarking, account for the eliminated network hop:

```csharp
// PDFreactor: render time = (HTTP call to service) + (Java render time)
// IronPDF:    render time = (local Chromium render time)

// Depending on service location and load:
// - Local PDFreactor service: IronPDF may be comparable or faster
// - Remote PDFreactor service: IronPDF will typically be faster (no network)
// - Cold-start comparison: PDFreactor server amortizes startup; IronPDF has per-process warmup

// Measure both under your realistic load pattern
// https://ironpdf.com/examples/parallel/
```

---

## Migration Checklist

### Pre-Migration
- [ ] Identify all PDFreactor service calls (`rg "PDFreactor|RealObjects" --type cs`)
- [ ] Audit HTML templates for CSS Paged Media features (`@page` margin boxes, footnotes, named flows)
- [ ] Identify CSS that depends on PDFreactor-specific extensions
- [ ] Document PDFreactor server configuration (service URL, timeout, thread settings)
- [ ] Measure baseline PDFreactor render times including HTTP round-trip
- [ ] Verify IronPDF .NET compatibility
- [ ] Obtain IronPDF license key

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `PDFreactor.dll` reference from `.csproj`
- [ ] Add license key at application startup
- [ ] Replace `new PDFreactor(...)` with `new ChromePdfRenderer()`
- [ ] Replace `config.Document = html` pattern with `RenderHtmlAsPdfAsync()`
- [ ] Migrate CSS Paged Media headers/footers to `RenderingOptions.HtmlHeader/Footer`
- [ ] Replace synchronous `Convert()` calls with async equivalents
- [ ] Replace secondary merge libraries with `PdfDocument.Merge()`
- [ ] Migrate CSS-based watermark patterns to `TextStamper` / `ImageStamper`
- [ ] Migrate `config.Encryption` to `pdf.SecuritySettings`

### Testing
- [ ] Render each template and compare visual output
- [ ] Check CSS Paged Media features — headers, footers, page numbering
- [ ] Test JavaScript-heavy pages with appropriate `WaitFor` settings
- [ ] Verify page count matches PDFreactor output
- [ ] Benchmark render time vs PDFreactor baseline
- [ ] Test parallel throughput at target concurrency
- [ ] Verify Docker image build succeeds without Java runtime

### Post-Migration
- [ ] Remove PDFreactor Web Service from Docker images and CI configurations
- [ ] Remove Java runtime dependency from all deployment environments
- [ ] Update health checks and monitoring to remove PDFreactor service checks
- [ ] Document CSS Paged Media features that changed behavior

---

## The Bottom Line

The most important pre-migration step is the CSS Paged Media audit. If your templates use basic `@page` sizing and `page-break-*`, migration is low-risk. If they use running elements, footnotes, or named page flows, those require rewriting before the migration is comparable output.

The deployment simplification — removing the Java sidecar or service dependency — is the most concrete operational win and is verifiable before writing any migration code by checking your Docker image size and startup sequence.

**Discussion question:** Looking at your own migration checklist, what would you add — particularly around CSS Paged Media feature gaps or concurrency patterns that the article didn't cover?
