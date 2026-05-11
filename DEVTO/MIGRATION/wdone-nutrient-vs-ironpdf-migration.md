# Switching from Nutrient.io to IronPDF: copy-paste and ship

You added a second pod. The PDF rendering service handled it fine. You added a third, and that's when it surfaced: concurrent render requests were stepping on each other in ways that hadn't been visible at lower traffic. Sometimes a PDF would come back blank. Occasionally two renders would produce outputs that were swapped. The root cause investigation led to a shared renderer state that the scaling documentation hadn't addressed clearly.

This article is for teams in that exact position — scaling up and finding that their Nutrient.io integration's concurrency model doesn't match their traffic pattern. It covers the migration path to IronPDF and what changes in your code. The concurrency and renderer reuse sections stand on their own even if you end up with a different library.

---

## Concurrency: diagnosing the real problem

Before looking at library options, make sure you've identified the exact failure mode. Concurrency problems in PDF generation come from a few distinct sources:

**Shared renderer state** — if a renderer object is instantiated once and shared across threads without thread-safe access, simultaneous calls can corrupt state.

**Shared output stream** — if multiple render calls write to the same buffer or stream without locking, outputs can interleave.

**Resource exhaustion** — if the underlying renderer (Chromium, WebKit, etc.) has per-process or per-instance limits that you're hitting, requests queue or fail.

**Diagnosis pattern:**

```csharp
// Add logging around render calls to surface timing issues
var sw = System.Diagnostics.Stopwatch.StartNew();
try
{
    // your render call here
    sw.Stop();
    _logger.LogInformation("Render completed in {ms}ms on thread {tid}",
        sw.ElapsedMilliseconds, System.Threading.Thread.CurrentThread.ManagedThreadId);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Render failed after {ms}ms on thread {tid}",
        sw.ElapsedMilliseconds, System.Threading.Thread.CurrentThread.ManagedThreadId);
}
```

If you see failures clustering on specific thread IDs or time-overlapping requests, shared state is likely the cause.

---

## Why migrate (without drama)

Nine reasons teams evaluate alternatives to Nutrient.io for .NET server-side PDF generation:

1. **Concurrency model mismatch** — SDK designed for client-side or single-user scenarios may not fit server-side concurrent generation at scale.
2. **Pricing tier** — Nutrient is a premium commercial SDK; if your use case is primarily HTML-to-PDF generation, a more focused library may be more cost-effective at scale. (No pricing comparison made — verify both against your usage.)
3. **API surface breadth** — Nutrient covers many PDF use cases. If you only need HTML-to-PDF + basic manipulation, you're paying for (and managing) features you don't use.
4. **Deployment complexity** — verify whether Nutrient's .NET integration requires external services or native binaries in your target environment.
5. **HTTP API dependency** — if your Nutrient integration is API-based, every render involves a network call. In-process rendering eliminates latency and dependency.
6. **Docker/Kubernetes constraints** — verify Nutrient's container deployment requirements against your infrastructure.
7. **Licensing model changes** — commercial SDK licensing can change between contract cycles. Renewal conversations are natural trigger points for evaluation.
8. **CSS fidelity requirements** — verify Nutrient's HTML rendering approach if your templates use modern CSS.
9. **Feature set consolidation** — teams with secondary libraries (for merge, watermark, etc.) evaluate whether a single library can handle all PDF operations.

### Comparison table

| Aspect | Nutrient.io (.NET) | IronPDF |
|---|---|---|
| Focus | Full PDF SDK (view, edit, annotate, generate) | HTML-to-PDF + PDF manipulation |
| Pricing | Commercial — verify at nutrient.io | Commercial — verify at ironsoftware.com |
| API Style | Verify — library or HTTP client | In-process .NET library |
| Learning Curve | Medium-High (broad SDK) | Medium |
| HTML Rendering | Verify rendering approach | Chromium-based |
| Page Indexing | Verify | 0-based |
| Thread Safety | Verify for your specific integration | Renderer instance reuse — see async docs |
| Namespace | Verify — `PSPDFKit` or `Nutrient` namespace | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | Nutrient approach | Effort to migrate |
|---|---|---|
| HTML to PDF generation | Verify Nutrient API | Low-Medium |
| PDF viewing / annotations | Nutrient core feature | N/A — IronPDF doesn't replace viewer |
| Merge PDFs | Verify Nutrient API | Low (native in IronPDF) |
| Watermark / stamp | Verify Nutrient API | Low |
| Password protection | Verify Nutrient API | Low |
| Digital signatures | Verify Nutrient API | Medium — verify IronPDF signing |
| Form filling | Verify Nutrient API | Medium — verify IronPDF form API |
| OCR / text extraction | Verify Nutrient API | Medium — verify IronPDF extraction |
| Concurrent rendering | Problematic in some configurations | Low — explicit per-thread pattern |
| HTTP API round trips | Per-render latency | Eliminated — in-process |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| Need PDF viewer in browser | Nutrient's viewer has no IronPDF equivalent — partial migration |
| Server-side generation only | IronPDF fits well; evaluate feature coverage |
| Concurrency at scale is the trigger | IronPDF's per-thread renderer pattern is explicit and predictable |
| Annotation / form workflows | Verify IronPDF coverage before committing |

---

## Before you start

### Prerequisites

- .NET 6+ target framework
- Nutrient SDK documentation for your specific version
- Inventory of Nutrient features your server-side code actually uses

### Find Nutrient references in your codebase

```bash
# Find all Nutrient/PSPDFKit usage — both branding periods
rg -l "Nutrient\|PSPDFKit\|pspdfkit" --type cs -i

# Find class instantiation
rg "PdfDocument\|PdfApi\|NutrientClient\|PSPDFKitClient" --type cs -n

# Find namespace imports
rg "using Nutrient\|using PSPDFKit" --type cs -n

# Find API endpoint references (if HTTP-based integration)
rg "nutrient\.io\|pspdfkit\.com\|api/pdf" --type cs -n

# Find NuGet package references
grep -r -i "nutrient\|pspdfkit" **/*.csproj *.csproj 2>/dev/null
```

### Remove Nutrient SDK, install IronPDF

```bash
# Remove Nutrient NuGet package — verify exact package name
dotnet remove package PSPDFKit    # older branding
dotnet remove package Nutrient    # newer branding — verify exact name

# Install IronPDF
dotnet add package IronPdf
dotnet restore
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (Nutrient — verify license initialization in SDK docs):**
```csharp
// Nutrient SDK license setup — verify exact method in current docs
// The namespace may be PSPDFKit or Nutrient depending on SDK version
// using PSPDFKit; // or: using Nutrient;

// License pattern — verify before using:
// NutrientLicense.SetKey("YOUR_NUTRIENT_KEY"); // hypothetical
// PSPDFKit.License.SetKey("YOUR_KEY");          // hypothetical
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR_IRONPDF_LICENSE_KEY";
// License guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
// Verify — namespace may be PSPDFKit, Nutrient, or sub-namespaces
using PSPDFKit;  // verify
// using Nutrient; // alternate — verify
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
using IronPdf.Security;
```

### Step 3: Basic HTML-to-PDF with thread-safe pattern

**Before (Nutrient — verify API; illustrative pattern only):**
```csharp
// VERIFY all class and method names against Nutrient.io SDK docs
// This is a structural placeholder — do not use without verification

class PdfService
{
    // Verify: is this the right pattern for concurrent Nutrient usage?
    // Does Nutrient require per-request instances or support shared instances?
    private readonly object _lock = new();

    public byte[] GeneratePdf(string html)
    {
        lock (_lock) // Are you doing this because concurrent calls fail?
        {
            // var converter = new NutrientPdfConverter(); // hypothetical
            // return converter.HtmlToPdf(html);           // hypothetical
            throw new NotImplementedException("Replace with verified Nutrient API");
        }
    }
}
```

**After (IronPDF — explicit thread-safe pattern):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

// Thread-safe: each render gets its own renderer instance
class PdfService
{
    public PdfDocument GeneratePdf(string html)
    {
        // No locking needed — renderer instances are independent
        var renderer = new ChromePdfRenderer();
        return renderer.RenderHtmlAsPdf(html);
    }

    // For high-throughput: use async
    public async Task<PdfDocument> GeneratePdfAsync(string html)
    {
        var renderer = new ChromePdfRenderer();
        return await renderer.RenderHtmlAsPdfAsync(html);
    }
}
// Async guide: https://ironpdf.com/how-to/async/
```

---

## The concurrency pattern — in detail

This is the section that addresses the opening scenario. Here's the explicit IronPDF concurrency approach:

### Pattern 1: Per-request renderer (recommended for web apps)

```csharp
// ASP.NET controller — new renderer per request
// Each renderer is independent — no shared state issues
[HttpGet("generate")]
public async Task<IActionResult> GeneratePdf([FromQuery] string templateId)
{
    string html = await _templateService.GetHtmlAsync(templateId);

    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return File(pdf.Stream, "application/pdf", $"{templateId}.pdf");
}
```

### Pattern 2: Parallel batch rendering

```csharp
using System.Threading.Tasks;
using IronPdf;

// Render a batch in parallel — each task gets its own renderer
async Task<PdfDocument[]> RenderBatchAsync(IEnumerable<string> htmlItems)
{
    return await Task.WhenAll(
        htmlItems.Select(async html =>
        {
            // One renderer per concurrent task — explicitly safe
            var renderer = new ChromePdfRenderer();
            return await renderer.RenderHtmlAsPdfAsync(html);
        })
    );
}
// Parallel examples: https://ironpdf.com/examples/parallel/
```

### Pattern 3: Bounded concurrency with SemaphoreSlim

```csharp
using System.Threading;
using System.Threading.Tasks;
using IronPdf;

class BoundedPdfRenderer
{
    private readonly SemaphoreSlim _semaphore;

    // Limit concurrent renders to avoid memory pressure
    public BoundedPdfRenderer(int maxConcurrent = 4)
        => _semaphore = new SemaphoreSlim(maxConcurrent);

    public async Task<PdfDocument> RenderAsync(string html)
    {
        await _semaphore.WaitAsync();
        try
        {
            var renderer = new ChromePdfRenderer();
            return await renderer.RenderHtmlAsPdfAsync(html);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

---

## API mapping tables

### Namespace mapping

| Nutrient/PSPDFKit | IronPDF | Notes |
|---|---|---|
| `PSPDFKit` / `Nutrient` | `IronPdf` | Core |
| SDK sub-namespaces | `IronPdf.Rendering` | Render config |
| SDK sub-namespaces | `IronPdf.Editing` | Manipulation |

### Core class mapping

| Nutrient class | IronPDF class | Description |
|---|---|---|
| HTML-to-PDF converter | `ChromePdfRenderer` | Render entry point |
| Options/config class | `ChromePdfRenderOptions` | Render configuration |
| PDF document class | `PdfDocument` | Document representation |
| N/A | `PdfDocument` static methods | Merge, split |

### Document loading methods

| Operation | Nutrient | IronPDF |
|---|---|---|
| HTML string | Verify method | `renderer.RenderHtmlAsPdf(html)` |
| URL | Verify method | `renderer.RenderUrlAsPdf(url)` |
| HTML file | Verify method | `renderer.RenderHtmlFileAsPdf(path)` |
| Existing PDF | Verify method | `PdfDocument.FromFile(path)` |

### Page operations

| Operation | Nutrient | IronPDF |
|---|---|---|
| Page count | Verify | `pdf.PageCount` |
| Paper size | Verify | `ChromePdfRenderOptions.PaperSize` |
| Margins | Verify | `ChromePdfRenderOptions.Margin*` |
| Orientation | Verify | `ChromePdfRenderOptions.PaperOrientation` |

### Merge/split operations

| Operation | Nutrient | IronPDF |
|---|---|---|
| Merge | Verify Nutrient API | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | Verify Nutrient API | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

> **Note:** Nutrient.io API specifics depend on SDK version and integration tier. "Before" blocks show illustrative patterns with verification reminders.

### 1. HTML to PDF

**Before (Nutrient — verify all API names before implementing):**
```csharp
// VERIFY: All class names and method signatures in Nutrient.io SDK docs
// This is a structural placeholder — do not ship without verification

using System;
// using PSPDFKit; // or: using Nutrient; -- verify namespace

class HtmlToPdfExample
{
    static async System.Threading.Tasks.Task Main()
    {
        // Verify: correct class name, constructor, and method signatures
        // var converter = new NutrientHtmlConverter(); // hypothetical
        // var options = new HtmlConvertOptions
        // {
        //     PaperSize = "A4",         // verify property names
        //     MarginTop = "10mm",        // verify
        // };
        // byte[] pdf = await converter.ConvertAsync(html, options); // hypothetical

        Console.WriteLine("Replace with verified Nutrient.io SDK API");
    }
}
```

**After (IronPDF — thread-safe single-render pattern):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize  = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop    = 10;
renderer.RenderingOptions.MarginBottom = 10;

var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>"
);
pdf.SaveAs("invoice.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (Nutrient — verify merge API):**
```csharp
// Verify: does Nutrient support PDF merge via the .NET SDK?
// If yes, use their API (verify method names)
// If no, a secondary library is typically used

// Common secondary library pattern (PdfSharp):
// using PdfSharp.Pdf;
// using PdfSharp.Pdf.IO;
// using var output = new PdfDocument();
// foreach (string path in inputPaths)
// {
//     using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
//     foreach (PdfPage page in input.Pages)
//         output.AddPage(page);
// }
// output.Save(outputPath);

Console.WriteLine("Verify Nutrient.io merge API or secondary library pattern");
```

**After (IronPDF native):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var merged = PdfDocument.Merge(
    PdfDocument.FromFile("section1.pdf"),
    PdfDocument.FromFile("section2.pdf")
);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (Nutrient — verify watermark API):**
```csharp
// VERIFY: Does Nutrient support watermarking via .NET SDK?
// Verify class names and method signatures in Nutrient.io docs

// If not natively supported, iTextSharp pattern is typical:
// using var reader = new PdfReader("input.pdf");
// using var stamper = new PdfStamper(reader, outputStream);
// var cb = stamper.GetOverContent(1);
// ... (standard iTextSharp watermark pattern)

Console.WriteLine("Verify Nutrient.io watermark API before implementing");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf = PdfDocument.FromFile("input.pdf");
var stamper = new TextStamper
{
    Text = "CONFIDENTIAL",
    FontColor = IronSoftware.Drawing.Color.LightGray,
    FontSize = 60,
    Opacity = 30,
    Rotation = 45,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};
pdf.ApplyStamp(stamper);
pdf.SaveAs("watermarked.pdf");
// Guide: https://ironpdf.com/how-to/custom-watermark/
```

---

### 4. Password protection

**Before (Nutrient — verify security API):**
```csharp
// VERIFY: Nutrient likely supports PDF security — verify exact .NET API
// The pattern below is a placeholder — replace with actual Nutrient SDK calls

// using PSPDFKit; // or using Nutrient; -- verify

// Hypothetical Nutrient security pattern:
// var doc = await NutrientPdf.OpenAsync("input.pdf"); // verify
// doc.SetPassword("userpass", "ownerpass");            // verify
// await doc.SaveAsync("secured.pdf");                  // verify

Console.WriteLine("Replace with verified Nutrient.io security API");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Security;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf = PdfDocument.FromFile("input.pdf");
pdf.SecuritySettings.UserPassword  = "userpass";
pdf.SecuritySettings.OwnerPassword = "ownerpass";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Viewer functionality

If your application uses Nutrient's PDF viewer component (browser-side PDF rendering), that's out of scope for an IronPDF migration. IronPDF is a server-side generation and manipulation library. The viewer and the server-side generator are separate concerns — you can migrate the server-side component while keeping the viewer, or plan a separate viewer replacement.

### Page indexing

IronPDF uses 0-based page indexing. Verify Nutrient's convention in their documentation, then audit all page index references:

```csharp
// IronPDF: 0-based
var firstPage = pdf.Pages[0];  // first
var lastPage  = pdf.Pages[pdf.PageCount - 1]; // last
```

### Output model

Verify whether Nutrient's .NET SDK returns `byte[]`, `Stream`, or a document object. IronPDF returns `PdfDocument` — update call sites accordingly.

### DI registration in ASP.NET

```csharp
// If registering as singleton for performance:
builder.Services.AddSingleton<ChromePdfRenderer>(sp =>
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
    return renderer;
});

// NOTE: If sharing a single instance, ensure thread safety by not
// modifying RenderingOptions after registration.
// For per-request options variation: use transient registration instead.
builder.Services.AddTransient<ChromePdfRenderer>();
```

---

## Performance considerations

### Benchmark your concurrency scenario

The opening scenario was concurrent rendering failures. Before finalizing the migration, benchmark IronPDF's concurrent behavior under your specific load:

```csharp
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using IronPdf;

class ConcurrencyBenchmark
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "YOUR_KEY";
        const int concurrentRequests = 10;
        const int totalRequests = 100;
        string html = "<html><body><h1>Test</h1><p>Content</p></body></html>";

        var sw = Stopwatch.StartNew();
        int completed = 0;

        // Simulate concurrent rendering
        await Task.WhenAll(
            Enumerable.Range(0, totalRequests).Select(async i =>
            {
                var renderer = new ChromePdfRenderer(); // one per task
                using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
                System.Threading.Interlocked.Increment(ref completed);
                if (completed % 10 == 0)
                    Console.WriteLine($"{completed}/{totalRequests} completed");
            })
        );

        sw.Stop();
        Console.WriteLine($"Total: {sw.ElapsedMilliseconds}ms for {totalRequests} renders");
        Console.WriteLine($"Throughput: {totalRequests / sw.Elapsed.TotalSeconds:F1} renders/sec");
    }
}
```

### Disposal

```csharp
// Always dispose PdfDocument after use in long-running services
using var pdf = renderer.RenderHtmlAsPdf(html);
// Use pdf...
// Disposed at end of block
```

### Edge cases

- **Cold start:** First render after startup includes Chromium initialization overhead. Pre-warm if latency-sensitive.
- **Memory under concurrent load:** Each concurrent renderer has a Chromium footprint. Profile with realistic concurrency levels in your environment.
- **HTTP response streaming:** Return PDF streams directly rather than buffering to memory where possible.

---

## Migration checklist

### Pre-migration

- [ ] Identify exact Nutrient SDK tier and integration pattern (library vs HTTP API)
- [ ] Inventory server-side features only — separate viewer from generation concerns
- [ ] Find all Nutrient usage: `rg "Nutrient\|PSPDFKit" --type cs -i`
- [ ] Document the concurrency failure mode with logs (thread IDs, timestamps)
- [ ] Verify IronPDF .NET target framework compatibility
- [ ] Confirm commercial license requirements
- [ ] Set up IronPDF trial license in dev environment
- [ ] Run concurrency benchmark scaffold in staging before committing

### Code migration

- [ ] Remove Nutrient NuGet package
- [ ] Add `IronPdf` NuGet package
- [ ] Replace Nutrient namespace imports
- [ ] Replace license initialization
- [ ] Replace HTML-to-PDF calls with per-request `ChromePdfRenderer` pattern
- [ ] Replace merge operations
- [ ] Replace watermark operations
- [ ] Replace security/password operations
- [ ] Update ASP.NET DI registration
- [ ] Update output handling to `PdfDocument` model
- [ ] Add IronPDF license key to config

### Testing

- [ ] Render each HTML template and visually compare output
- [ ] Run concurrency benchmark at production traffic levels — verify no corruption
- [ ] Test merge, watermark, and security operations
- [ ] Test concurrent rendering with multiple parallel requests
- [ ] Test async rendering in web controller context
- [ ] Verify PDF viewer (if not migrating) still functions with IronPDF-generated output

### Post-migration

- [ ] Remove Nutrient license key from config / secrets
- [ ] Update deployment documentation
- [ ] Monitor memory under production concurrent load
- [ ] Verify concurrent failure mode is resolved — compare error rates

---

## The Bottom Line

The thread safety story in server-side PDF generation is underspecified in most library documentation. The explicit per-request renderer pattern in IronPDF makes the threading model visible in the code rather than hidden in documentation footnotes.

**What would you add to this migration checklist based on your own Nutrient.io integration?** Particularly interested in teams who had the concurrent rendering issue at scale — what was the actual failure mode, and did the per-request renderer pattern resolve it?

---

*Tags: `dotnet`, `csharp`, `pdf`, `migration`*
