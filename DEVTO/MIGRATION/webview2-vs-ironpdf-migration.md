---
title: "Moving off WebView2: practical IronPDF migration notes"
published: false
tags: dotnet, csharp, pdf, migration
---

The .NET 8 upgrade was the deadline. WebView2's `CoreWebView2.PrintToPdfAsync()` integration looked straightforward in .NET Framework — IWebView2PrintSettings, async print to stream, done. Then the team found that WebView2's `PrintToPdfAsync` has a documented limitation: it requires an initialized WebView2 environment, which requires a running Windows message loop, which doesn't work in a console app or background service without additional infrastructure. ASP.NET Core service on Linux? WebView2 is Windows-only. The constraints compound as the stack modernizes.

This article covers migrating from WebView2-based PDF generation to IronPDF. You'll have benchmark reference patterns and before/after code for the core operations. The comparison tables apply even if you're evaluating other alternatives.

---

## Why Migrate (Without Drama)

Teams migrating from WebView2-based PDF generation to IronPDF typically encounter:

1. **Windows-only constraint** — WebView2 Runtime is Windows-only; ASP.NET Core services on Linux or Docker on Linux cannot use WebView2.
2. **Message loop requirement** — `PrintToPdfAsync` requires an initialized `CoreWebView2Environment` and typically a Windows message loop; background services require complex workarounds.
3. **Runtime installation dependency** — the WebView2 Runtime must be installed on the target machine (or bundled as Fixed Version distribution, adding ~150MB).
4. **Not designed for server workloads** — WebView2 is a GUI control; using it in a background service or API is outside its intended design.
5. **Concurrency complexity** — managing multiple simultaneous WebView2 instances for concurrent PDF generation adds infrastructure overhead.
6. **PDF manipulation gap** — `PrintToPdfAsync` produces a PDF; it doesn't merge, watermark, add security, or extract text.
7. **.NET cross-platform migration** — teams moving to .NET 8 on Linux can't carry WebView2 forward.
8. **Version coupling** — WebView2 Runtime auto-updates (Evergreen mode) which can change rendering behavior; Fixed Version adds 150MB to deployment.
9. **ASP.NET Core incompatibility** — `PrintToPdfAsync` doesn't work in ASP.NET Core without additional setup; the STA/message-loop requirement conflicts with the async HTTP pipeline.
10. **Feature scope mismatch** — carrying a full browser control dependency for one PDF generation feature is heavyweight.

### Comparison Table

| Aspect | WebView2 PDF | IronPDF |
|---|---|---|
| Focus | Embedded browser control (PDF as secondary feature) | HTML-to-PDF + PDF manipulation |
| Pricing | Free (part of OS/runtime) | Commercial (see [ironpdf.com](https://ironpdf.com)) |
| API Style | `CoreWebView2.PrintToPdfAsync(path)` — async | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` |
| Learning Curve | Low to print existing page; complex for server use | Low for .NET devs |
| HTML Rendering | Chromium | Embedded Chromium |
| Page Indexing | Not exposed via PrintToPdfAsync | 0-based |
| Thread Safety | Requires Windows message loop | Per-task `ChromePdfRenderer` instances |
| Namespace | `Microsoft.Web.WebView2.Core` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | WebView2 | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | Load HTML + `PrintToPdfAsync` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | Navigate to URL + `PrintToPdfAsync` | `renderer.RenderUrlAsPdfAsync()` | Low |
| Save to file | `PrintToPdfAsync(filePath)` | `pdf.SaveAs(path)` | Low |
| Save to stream | `PrintToPdfAsync(stream)` | `pdf.Stream` / `pdf.BinaryData` | Low |
| Custom page size | `CoreWebView2PrintSettings.PageWidth/Height` | `RenderingOptions.PaperSize` | Low |
| Margins | `CoreWebView2PrintSettings.Margin*` | `RenderingOptions.Margin*` | Low |
| Headers/footers | Via HTML `@media print` styles | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Concurrent rendering | Multiple WebView2 instances (complex) | Task.WhenAll with per-task renderer | Low |
| Merge PDFs | Not native | `PdfDocument.Merge()` | Medium |
| Watermark | Not native | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Not native | `pdf.SecuritySettings` | Low |
| Linux/Docker | Not supported | Supported | Low (eliminate) |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Server-side PDF generation in ASP.NET Core | Switch — WebView2 is a GUI control, not designed for this |
| Linux/Docker deployment needed | Switch — WebView2 is Windows-only |
| WPF/WinForms desktop app + PDF generation | WebView2 may integrate better in existing UI stack; evaluate |
| Concurrent PDF generation at scale | Switch — eliminates message loop infrastructure |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All WebView2 PDF References

```bash
# Find WebView2 print usage
rg -l "PrintToPdfAsync\|CoreWebView2Print\|WebView2\b" --type cs
rg "PrintToPdfAsync\|CoreWebView2Print" --type cs -n

# Find WebView2 environment initialization
rg "CoreWebView2Environment\|InitializeAsync\|EnsureCoreWebView2Async" --type cs -n

# Find WebView2 NuGet reference
grep -r "Microsoft\.Web\.WebView2" *.csproj **/*.csproj 2>/dev/null

# Find Windows-specific threading workarounds
rg "ApartmentState\.STA\|Dispatcher\b\|Application\.Run\b" --type cs -n
```

### Uninstall / Install

```bash
# Remove WebView2 package
dotnet remove package Microsoft.Web.WebView2

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
using Microsoft.Web.WebView2.Core;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic HTML to PDF

**Before (WebView2 in WinForms/WPF context — requires UI thread):**
```csharp
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

class PrintToPdfBefore
{
    static async Task Main()
    {
        // WebView2 requires UI message loop — complex in background services
        var form = new Form();
        var webView = new Microsoft.Web.WebView2.WinForms.WebView2();
        form.Controls.Add(webView);

        // Environment init required before use
        var env = await CoreWebView2Environment.CreateAsync();
        await webView.EnsureCoreWebView2Async(env);

        // Navigate to content
        webView.CoreWebView2.NavigateToString(
            "<html><body><h1>Hello</h1></body></html>"
        );

        // Wait for navigation, then print
        await Task.Delay(1000); // crude wait — timing not reliable

        var settings = new CoreWebView2PrintSettings
        {
            Duplex = CoreWebView2PrintDuplex.OneSided,
        };
        await webView.CoreWebView2.PrintToPdfAsync("output.pdf", settings);
        Console.WriteLine("Saved output.pdf — requires Windows message loop");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// No UI control, no message loop, no environment init
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Hello</h1></body></html>");
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Benchmark Reference Patterns

> Measurement structures only — no performance claims. Run against your actual HTML in your environment to compare WebView2 and IronPDF baseline times.

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
        body { font-family: Arial; padding: 40px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { border: 1px solid #ddd; padding: 6px; font-size: 11px; }
    </style>
    </head>
    <body>
        <h1>Benchmark Document</h1>
        <table>
            <tr><th>Item</th><th>Qty</th><th>Price</th></tr>
            <tr><td>Widget A</td><td>10</td><td>$12.50</td></tr>
            <tr><td>Widget B</td><td>5</td><td>$24.00</td></tr>
        </table>
    </body>
    </html>";

async Task<(double avg, double p95)> BenchmarkIronPdf(int runs = 25)
{
    var renderer = new ChromePdfRenderer();

    // Warm up — amortize Chromium init
    using var warmup = await renderer.RenderHtmlAsPdfAsync(html);

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

// WebView2 comparison benchmark (for reference structure — omits message loop overhead):
// Each WebView2 render includes:
// - CoreWebView2Environment.CreateAsync() (first time)
// - EnsureCoreWebView2Async()
// - NavigateToString() + navigation completion wait
// - PrintToPdfAsync()
// These all add to wall-clock time in a server context
```

### Concurrent Throughput

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// WebView2: each concurrent render requires its own initialized WebView2 instance
// IronPDF: Task.WhenAll with per-task renderer
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
    Console.WriteLine($"Degree {degree}: {sw.Elapsed.TotalMilliseconds:F0}ms | {sw.Elapsed.TotalMilliseconds / degree:F1}ms/doc");
}

await BenchmarkConcurrency(5);
await BenchmarkConcurrency(10);
await BenchmarkConcurrency(20);
// See: https://ironpdf.com/how-to/async/
```

### Memory Under Concurrent Load

```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

static async Task MeasureMemory(int iterations)
{
    var renderer = new ChromePdfRenderer();
    var html = "<html><body><h1>Memory benchmark</h1></body></html>";

    var before = GC.GetTotalMemory(forceFullCollection: true);

    for (int i = 0; i < iterations; i++)
    {
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        // pdf disposed each iteration — stream not held
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    var after = GC.GetTotalMemory(forceFullCollection: true);

    Console.WriteLine($"{iterations} renders: delta {(after - before) / 1024:F0} KB after GC");
}

await MeasureMemory(50);
// WebView2: each instance holds an OS process; memory profile is fundamentally different
```

---

## API Mapping Tables

### Namespace Mapping

| WebView2 | IronPDF | Notes |
|---|---|---|
| `Microsoft.Web.WebView2.Core` | `IronPdf` | Core namespace |
| `Microsoft.Web.WebView2.WinForms` | N/A — no UI control needed | No UI dependency |
| N/A | `IronPdf.Editing` | Watermark / stamp |

### Core Class Mapping

| WebView2 Class | IronPDF Class | Description |
|---|---|---|
| `CoreWebView2` | `ChromePdfRenderer` | Primary rendering class |
| `CoreWebView2PrintSettings` | `ChromePdfRenderOptions` | Rendering configuration |
| `CoreWebView2Environment` | N/A — no init required | Environment setup not needed |
| N/A | `PdfDocument` | PDF manipulation object |

### Document Loading Methods

| Operation | WebView2 | IronPDF |
|---|---|---|
| HTML string | `NavigateToString(html)` + `PrintToPdfAsync()` | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | `Navigate(url)` + `PrintToPdfAsync()` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML file | `Navigate("file:///path")` + print | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Save to file | `PrintToPdfAsync(filePath)` | `pdf.SaveAs(filePath)` |

### Page Operations

| Operation | WebView2 | IronPDF |
|---|---|---|
| Page count | Not exposed | `pdf.PageCount` |
| Remove page | Not applicable | `pdf.RemovePages(index)` |
| Extract text | Not applicable | `pdf.ExtractAllText()` |
| Rotate | Via CSS or print settings | `pdf.RotateAllPages(PdfRotation.Clockwise90)` |

### Merge / Split Operations

| Operation | WebView2 | IronPDF |
|---|---|---|
| Merge | Not native | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not native | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before (WebView2 — server context workaround):**
```csharp
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class HtmlToPdfBefore
{
    // WebView2 requires Windows message loop for CoreWebView2 operations
    static async Task Main()
    {
        // In a server context, WebView2 requires threading workarounds
        // This is a simplified structure — production use needs STA thread + message pump

        var tcs = new TaskCompletionSource<byte[]>();

        var staThread = new Thread(async () =>
        {
            try
            {
                var env = await CoreWebView2Environment.CreateAsync(
                    null, Path.GetTempPath());

                // WebView2 needs a control/handle — not usable headless without additional setup
                // This code is illustrative — production requires WinForms/WPF host or custom pump
                tcs.SetResult(Array.Empty<byte>()); // placeholder
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();

        var bytes = await tcs.Task;
        Console.WriteLine("WebView2 server-side PDF is complex — requires message loop infrastructure");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// No UI control, no message loop, no environment init, no STA thread
var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; }
        h1 { font-size: 20px; }
        .amount { font-weight: bold; margin-top: 20px; }
    </style>
    </head>
    <body>
        <h1>Invoice #2024-0099</h1>
        <p>Customer: Acme Corp</p>
        <div class='amount'>Total Due: $4,200.00</div>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");
Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s)) — no message loop required");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (WebView2 — not native):**
```csharp
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;

class MergeBefore
{
    static async System.Threading.Tasks.Task Main()
    {
        // WebView2 generates one PDF per navigation — no merge capability
        // Each section requires separate environment + navigation + print cycle

        var sections = new[]
        {
            ("<html><body><h1>Section 1</h1></body></html>", "section1.pdf"),
            ("<html><body><h1>Section 2</h1></body></html>", "section2.pdf"),
        };

        // Each section requires its own WebView2 lifecycle (simplified — omits message loop)
        // Then merge via secondary library
        Console.WriteLine("WebView2: separate PDF per navigation + secondary merge library required");
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
Console.WriteLine($"Merged: {merged.PageCount} pages — concurrent + merge in one step");
```

---

### 3. Watermark

**Before (WebView2 — requires embedding in HTML or secondary library):**
```csharp
using System;
// WebView2 can add watermarks via CSS before printing (embedded in HTML):
// .watermark { position: fixed; top: 50%; left: 50%; opacity: 0.15;
//              font-size: 80px; transform: rotate(-45deg) translate(-50%, -50%); }
// But programmatic post-generation watermark requires a secondary library.

class WatermarkBefore
{
    static void Main()
    {
        // Option A (CSS): embed watermark in HTML before WebView2 print
        var htmlWithWatermark = @"
            <html><head><style>
            .wm { position:fixed; top:50%; left:50%; opacity:0.1;
                  font-size:80px; transform:rotate(-45deg) translate(-50%,-50%);
                  color:gray; pointer-events:none; }
            </style></head>
            <body><div class='wm'>DRAFT</div><h1>Content</h1></body></html>";

        // Then print via WebView2 (requires message loop setup — omitted here)
        // Option B: post-generation via secondary PDF library
        Console.WriteLine("WebView2 watermark needs CSS embed or secondary library");
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
    "<html><body><h1>Document Content</h1></body></html>"
);

// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronSoftware.Drawing.Color.Gray,
    Opacity = 15,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (WebView2 — not supported; secondary library required):**
```csharp
using System;
using System.IO;
// WebView2's PrintToPdfAsync has no security/password settings
// Password must be applied via secondary PDF library after generation

class PasswordBefore
{
    static void Main()
    {
        // Step 1: Generate PDF via WebView2 PrintToPdfAsync (omitting message loop):
        // await webView.CoreWebView2.PrintToPdfAsync("temp.pdf");

        // Step 2: Apply password via secondary library:
        // var bytes = File.ReadAllBytes("temp.pdf");
        // var secured = SomePdfLib.SetPassword(bytes, "open123", "admin456");
        // File.WriteAllBytes("secured.pdf", secured);

        Console.WriteLine("WebView2 has no password API — secondary library required");
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
    "<html><body><h1>Protected Document</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### WebView2 Is a UI Control — IronPDF Is a Library

The key architectural difference: WebView2 is designed as an embeddable browser control in a Windows application. Using it for server-side PDF generation requires bending its design intent. IronPDF is a headless library designed specifically for programmatic PDF generation — it doesn't require a UI host.

### Windows Runtime Removal

The WebView2 Runtime must be present on the target machine. After migration:

```bash
# Remove WebView2 NuGet package
dotnet remove package Microsoft.Web.WebView2

# The WebView2 Runtime on production machines can be uninstalled if no other
# applications require it — check shared dependencies before removing
```

### Message Loop Infrastructure

Find and remove the STA thread / message loop workarounds:

```bash
# Find message loop workarounds
rg "Application\.Run\|Dispatcher\.Run\|ApartmentState\.STA" --type cs -n
rg "TaskCompletionSource.*WebView\|CoreWebView2\b" --type cs -n
```

### `PrintToPdfAsync` Settings → `RenderingOptions`

```csharp
// Before (CoreWebView2PrintSettings):
// settings.PageWidth = 8.5; // inches
// settings.PageHeight = 11;
// settings.MarginTop = 1;   // inches

// After (IronPDF RenderingOptions):
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.Letter;
renderer.RenderingOptions.MarginTop = 25; // millimeters (1 inch = 25.4 mm)
// See: https://ironpdf.com/how-to/rendering-options/
```

### Header/Footer Tokens

WebView2 uses CSS `@media print` for headers/footers. IronPDF supports `HtmlHeaderFooter`:

```csharp
// https://ironpdf.com/how-to/headers-and-footers/
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:right; font-size:9px; padding:0 20px'>Page {page} of {total-pages}</div>",
};
```

---

## Performance Considerations

### ASP.NET Core Integration

```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    [HttpPost("generate")]
    public async Task<IActionResult> GeneratePdf([FromBody] ReportRequest req)
    {
        // IronPDF works directly in async controller — no message loop needed
        // https://ironpdf.com/how-to/async/
        var renderer = new ChromePdfRenderer();
        using var pdf = await renderer.RenderHtmlAsPdfAsync(BuildHtml(req));
        return File(pdf.BinaryData, "application/pdf", "report.pdf");
    }
}
```

### Parallel Rendering in Background Service

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

// https://ironpdf.com/examples/parallel/
public async Task GenerateBatchAsync(IEnumerable<ReportData> reports)
{
    var pdfs = await Task.WhenAll(reports.Select(async data =>
    {
        var renderer = new ChromePdfRenderer();
        return await renderer.RenderHtmlAsPdfAsync(BuildHtml(data));
    }));

    // Process each pdf...
    foreach (var pdf in pdfs) pdf.Dispose();
}
```

### Renderer Warm-Up

```csharp
using IronPdf;

// Amortize Chromium init at startup (equivalent to WebView2 environment init)
var renderer = new ChromePdfRenderer();
using var _ = await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");
// Ready for production requests
```

---

## Migration Checklist

### Pre-Migration
- [ ] Find all WebView2 print usage (`rg "PrintToPdfAsync\|NavigateToString" --type cs`)
- [ ] Find message loop / STA thread infrastructure
- [ ] Find WebView2 Runtime installation in deployment scripts
- [ ] Identify secondary libraries used alongside WebView2 (merge, security, etc.)
- [ ] Verify target platforms — Linux deployment is the driver here
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `Microsoft.Web.WebView2` NuGet package
- [ ] Add license key at application startup
- [ ] Replace `PrintToPdfAsync(path)` with `renderer.RenderHtmlAsPdfAsync(html)` + `pdf.SaveAs()`
- [ ] Replace `Navigate(url)` + print with `renderer.RenderUrlAsPdfAsync(url)`
- [ ] Remove `CoreWebView2Environment.CreateAsync()` initialization
- [ ] Remove STA thread / message loop wrappers
- [ ] Map `CoreWebView2PrintSettings.*` properties to `RenderingOptions.*`
- [ ] Replace secondary merge library with `PdfDocument.Merge()`
- [ ] Replace secondary security library with `pdf.SecuritySettings`

### Testing
- [ ] Verify PDF output on Linux (the original blocker)
- [ ] Test in Docker container — confirm system library requirements
- [ ] Test concurrent rendering at target throughput (no message loop bottleneck)
- [ ] Compare visual output against WebView2 reference
- [ ] Benchmark render time: warm and cold
- [ ] Test merge, watermark, and password protection
- [ ] Verify ASP.NET Core async context works correctly

### Post-Migration
- [ ] Remove `Microsoft.Web.WebView2` package
- [ ] Remove WebView2 Runtime installation from deployment scripts
- [ ] Remove STA thread infrastructure code
- [ ] Remove secondary PDF libraries now replaced by IronPDF

---

## Done Migrating? Here's What's Next

The Linux/Docker constraint is usually the definitive migration trigger — WebView2 has no Linux binary, and the constraint is architectural. Once IronPDF replaces the PDF generation path, the Windows-only runtime dependency and the message loop infrastructure disappear together.

The benchmark structures above are most useful for comparing concurrent throughput — that's where the architectural difference (UI control vs headless library) shows up most clearly in server workloads.

**Discussion question:** After migrating, what were your before/after figures on bundle size or render time under concurrent load — particularly if you were previously throttling WebView2 instances through a semaphore or queue?

