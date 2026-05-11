# Switching from TextControl to IronPDF: copy-paste and ship

Scaling changes the problem. A single-threaded report generation service handles the request queue without incident at fifty requests per hour. At five hundred, the behavior changes — PDF output becomes inconsistent, some documents render with corrupted layout, and the logs show thread collisions in the TextControl rendering layer. TextControl's core components weren't designed for concurrent server workloads; the COM-based rendering that works correctly when serialized through a single thread starts producing unreliable output under contention. The fix — a queue, a semaphore, a fixed thread pool — adds infrastructure to work around a library constraint.

This article covers migrating from TX Text Control / TextControl PDF generation to IronPDF. You'll have working before/after code for the core operations by the end. The comparison tables and checklist apply even if IronPDF isn't your final choice.

---

## Why Migrate (Without Drama)

Teams evaluating TextControl replacements for PDF generation commonly encounter:

1. **Thread safety / STA constraints** — some TextControl configurations require STA thread affinity; multi-threaded server workloads require workarounds (queues, thread pools, serialization).
2. **Concurrency scaling ceiling** — the STA constraint means one document at a time per thread; horizontal scaling is limited by threading infrastructure, not compute.
3. **Installation complexity** — native components and COM registration requirements create deployment friction in Docker/cloud environments.
4. **Primary use case mismatch** — TextControl is a rich-text editor; teams using it only for PDF export are carrying the full library footprint for one feature.
5. **HTML input gap** — generating PDFs from HTML content requires conversion to TextControl's document model first.
6. **Mail merge vs template engine** — TextControl's mail merge is purpose-built but different from web-familiar template engines (Razor, Scriban, Handlebars).
7. **Linux/Docker compatibility** — verify cross-platform support for server-side PDF export at textcontrol.com; native component requirements affect container deployments.
8. **License model** — TextControl licensing is per-developer plus deployment; verify terms at textcontrol.com for your scale.
9. **Package count** — multiple `TXTextControl.*` packages for a PDF-only use case adds NuGet surface area.
10. **Async support** — verify current async/await support in their server-side components for ASP.NET Core integrations.

### Comparison Table

| Aspect | TX Text Control | IronPDF |
|---|---|---|
| Focus | Rich text editor + document export (PDF, DOCX) | HTML-to-PDF + PDF manipulation |
| Pricing | Commercial — verify at textcontrol.com | Commercial — verify at ironsoftware.com |
| API Style | Document load/edit/save model; STA affinity | `ChromePdfRenderer` — no threading constraints |
| Learning Curve | Medium; own document model | Low for .NET devs; HTML/CSS is the input |
| HTML Rendering | Via document conversion — not native | Embedded Chromium |
| Page Indexing | Verify in TextControl docs | 0-based |
| Thread Safety | STA constraints in some configs — verify | Verify IronPDF concurrent instance guidance |
| Namespace | `TXTextControl.*` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | TX Text Control | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | Via document conversion | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| DOCX to PDF | Native export | N/A — not in IronPDF scope | High (keep TextControl for this) |
| Mail merge to PDF | Built-in merge engine | HTML template engine + `RenderHtmlAsPdfAsync()` | Medium-High |
| Save to file | `doc.Save(path, format)` | `pdf.SaveAs(path)` | Low |
| Save to stream | `doc.Save(stream, format)` | `pdf.Stream` | Low |
| Concurrent rendering | STA thread pool workaround needed | No threading workaround needed | Low (eliminate workaround) |
| Merge PDFs | Not native for PDF | `PdfDocument.Merge()` | Medium |
| Watermark | Verify TextControl API | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Verify export settings | `pdf.SecuritySettings` | Low |
| Headers/footers | Document section headers/footers | `RenderingOptions.HtmlHeader/Footer` | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Concurrent PDF generation is the primary scaling bottleneck | Switch — eliminates STA threading constraint |
| HTML-to-PDF is the primary use case | Switch — IronPDF is HTML-first; TextControl requires a conversion step |
| DOCX editing + PDF export is the primary use case | Keep TextControl for DOCX path; evaluate IronPDF for HTML-to-PDF separately |
| Mail merge documents | Evaluate template engine (Scriban, Fluid) as TextControl mail merge replacement |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All TextControl References

```bash
# Find TextControl API usage
rg -l "TXTextControl\|ServerTextControl\|TXTextControl" --type cs
rg "TXTextControl\|ServerTextControl\|StreamType\." --type cs -n

# Find STA thread workarounds (the scaling problem)
rg "ApartmentState\.STA\|STAThread\|SemaphoreSlim\|ConcurrentQueue" --type cs -n

# Find TextControl NuGet references
grep -r "TXTextControl" *.csproj **/*.csproj 2>/dev/null

# Find TextControl document templates
find . -name "*.tx" -o -name "*.rtf" | sort 2>/dev/null
find . -name "*.tx" | wc -l  # if using TX format templates
```

### Uninstall / Install

```bash
# Remove TextControl packages (verify exact names for your edition)
dotnet remove package TXTextControl.TextControl.ASP.SDK  # verify name
dotnet remove package TXTextControl.Core                  # verify name

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
using TXTextControl;
using TXTextControl.ServerTextControl;  // verify namespace
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic PDF Generation

**Before (TextControl ServerTextControl — illustrative, verify all API names):**
```csharp
using TXTextControl;  // VERIFY namespace
using System;
using System.IO;
using System.Threading;

class Program
{
    static void Main()
    {
        // TextControl ServerTextControl typically requires STA thread
        var thread = new Thread(() =>
        {
            // VERIFY: ServerTextControl class and method names
            using var tx = new ServerTextControl();
            tx.Create();

            // Load content — VERIFY method names
            tx.Load("<html><body><h1>Hello</h1></body></html>",
                StringStreamType.HTMLFormat); // VERIFY enum

            // Export to PDF — VERIFY Save overload
            tx.Save("output.pdf", StreamType.AdobePDF); // VERIFY enum
            Console.WriteLine("Saved output.pdf — verify all TextControl API names");
        });

        thread.SetApartmentState(ApartmentState.STA); // STA required for some versions
        thread.Start();
        thread.Join();
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// No STA thread required — runs on any thread
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Hello</h1></body></html>"
);
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Eliminating the STA Threading Workaround

The concurrency issue that triggered this evaluation shows up most clearly when you look for the workaround code in the codebase:

```bash
# Find the threading workaround code
rg "ApartmentState\.STA\|STAThread\|SetApartmentState" --type cs -n
rg "SemaphoreSlim\|Semaphore\b" --type cs -n  # may be throttling TextControl access
rg "ConcurrentQueue\|BlockingCollection" --type cs -n  # request queuing
```

After migration, this infrastructure is removed entirely. IronPDF's `ChromePdfRenderer` doesn't require STA thread affinity — concurrent rendering works without a wrapper:

```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// BEFORE: required STA thread pool + semaphore to serialize TextControl access
// AFTER: direct parallel rendering, no threading infrastructure
// https://ironpdf.com/examples/parallel/

var documentRequests = GetPendingDocumentRequests(); // your data source

var results = await Task.WhenAll(documentRequests.Select(async req =>
{
    var renderer = new ChromePdfRenderer();  // one per task — no STA concern
    using var pdf = await renderer.RenderHtmlAsPdfAsync(BuildHtml(req));
    pdf.SaveAs(req.OutputPath);
    return req.OutputPath;
}));

Console.WriteLine($"Generated {results.Length} PDFs concurrently — no STA workaround needed");
// See: https://ironpdf.com/how-to/async/
```

---

## API Mapping Tables

### Namespace Mapping

| TX Text Control | IronPDF | Notes |
|---|---|---|
| `TXTextControl` | `IronPdf` | Core namespace |
| `TXTextControl.ServerTextControl` | `IronPdf` (renderer included) | No separate server class |
| N/A | `IronPdf.Editing` | Watermark / stamp |

### Core Class Mapping

| TextControl Class | IronPDF Class | Description |
|---|---|---|
| `ServerTextControl` | `ChromePdfRenderer` | Primary rendering class |
| `StreamType.AdobePDF` | `pdf.SaveAs()` / `pdf.Stream` | PDF output on the result object |
| `StringStreamType.HTMLFormat` | HTML string input to renderer | HTML is direct input, no conversion |
| N/A | `PdfDocument` | PDF manipulation object |

### Document Loading Methods

| Operation | TX Text Control | IronPDF |
|---|---|---|
| Load HTML | `tx.Load(html, StringStreamType.HTMLFormat)` | `renderer.RenderHtmlAsPdfAsync(html)` |
| Load URL | Verify | `renderer.RenderUrlAsPdfAsync(url)` |
| Load HTML file | Verify | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Load existing PDF | Verify | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | TX Text Control | IronPDF |
|---|---|---|
| Page count | Verify | `pdf.PageCount` |
| Remove page | Verify | `pdf.RemovePage(index)` — verify |
| Extract text | Verify | `pdf.ExtractAllText()` |
| Rotate | Verify | Verify in IronPDF docs |

### Merge / Split Operations

| Operation | TX Text Control | IronPDF |
|---|---|---|
| Merge | Not native for PDF output | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not native | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (TextControl with STA threading):**
```csharp
using TXTextControl;  // VERIFY
using System;
using System.IO;
using System.Threading;

class HtmlToPdfBefore
{
    static void Main()
    {
        // STA threading wrapper required — the concurrency problem
        Exception threadException = null;
        var thread = new Thread(() =>
        {
            try
            {
                // VERIFY: all class and method names against your TextControl version
                using var tx = new ServerTextControl();
                tx.Create();

                var html = @"
                    <html><body>
                    <h1>Invoice #2024-0099</h1>
                    <p>Customer: Acme Corp | Total: $4,200.00</p>
                    </body></html>";

                tx.Load(html, StringStreamType.HTMLFormat); // VERIFY enum name

                // Export to PDF — VERIFY Save method and enum
                tx.Save("invoice.pdf", StreamType.AdobePDF); // VERIFY
                Console.WriteLine("Saved invoice.pdf");
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (threadException != null)
            throw new Exception("TextControl error in STA thread", threadException);
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// No STA thread wrapper, no thread.Start/Join, no exception capture
var html = @"
    <html>
    <head>
    <style>body { font-family: Arial, sans-serif; padding: 40px; }
    h1 { font-size: 22px; } .total { font-weight: bold; font-size: 16px; }
    </style></head>
    <body>
        <h1>Invoice #2024-0099</h1>
        <p>Customer: Acme Corp</p>
        <div class='total'>Total: $4,200.00</div>
    </body></html>";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");
Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (TextControl — not native for PDF):**
```csharp
using TXTextControl;  // VERIFY
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class MergeBefore
{
    static void Main()
    {
        // TextControl doesn't merge PDFs natively.
        // Generate each section separately (each in its own STA thread), then merge externally.

        var pdfPaths = new List<string>();
        var htmlSections = new[] { "<html><body><h1>Section 1</h1></body></html>",
                                   "<html><body><h1>Section 2</h1></body></html>" };

        for (int i = 0; i < htmlSections.Length; i++)
        {
            var html = htmlSections[i];
            var outPath = $"section{i + 1}.pdf";

            var thread = new Thread(() =>
            {
                using var tx = new ServerTextControl(); // VERIFY
                tx.Create();
                tx.Load(html, StringStreamType.HTMLFormat); // VERIFY
                tx.Save(outPath, StreamType.AdobePDF);      // VERIFY
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            pdfPaths.Add(outPath);
        }

        // Merge via secondary library — TextControl doesn't merge PDFs
        Console.WriteLine("PDF merge requires secondary library + STA thread per section");
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
Console.WriteLine($"Merged: {merged.PageCount} pages — no STA thread needed");
```

---

### 3. Watermark

**Before (TextControl — verify watermark API):**
```csharp
using TXTextControl;  // VERIFY
using System;
using System.Threading;

class WatermarkBefore
{
    static void Main()
    {
        // VERIFY: TextControl programmatic watermark API
        // Watermark on PDF output may require secondary library

        var thread = new Thread(() =>
        {
            using var tx = new ServerTextControl(); // VERIFY
            tx.Create();
            tx.Load("<html><body><h1>Report</h1></body></html>",
                StringStreamType.HTMLFormat); // VERIFY

            // If TextControl supports watermark (verify):
            // tx.HeaderFooter.Header.Add(watermarkElement); // illustrative

            // Or post-export via secondary library:
            // tx.Save("temp.pdf", StreamType.AdobePDF);
            // var watermarked = SomePdfLib.AddTextWatermark(File.ReadAllBytes("temp.pdf"), "DRAFT");
            // File.WriteAllBytes("watermarked.pdf", watermarked);
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Console.WriteLine("Verify TextControl watermark API — may need secondary library");
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
    "<html><body><h1>Report</h1></body></html>"
);

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

**Before (TextControl — verify security API):**
```csharp
using TXTextControl;  // VERIFY
using System;
using System.Threading;

class PasswordBefore
{
    static void Main()
    {
        // VERIFY: TextControl PDF security export options
        // Password on PDF export may or may not be in the API — verify at textcontrol.com

        var thread = new Thread(() =>
        {
            using var tx = new ServerTextControl(); // VERIFY
            tx.Create();
            tx.Load("<html><body><h1>Confidential</h1></body></html>",
                StringStreamType.HTMLFormat); // VERIFY

            // If PDF password available in Save options (verify):
            // var saveSettings = new SaveSettings { Password = "open123" }; // illustrative
            // tx.Save("secured.pdf", StreamType.AdobePDF, saveSettings); // VERIFY

            Console.WriteLine("Verify TextControl PDF password API at textcontrol.com");
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
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

### Mail Merge Replacement

TextControl's mail merge engine has no direct IronPDF equivalent — IronPDF is an HTML renderer, not a document editor. If mail merge is a use case, consider:

**Option A: Scriban or Fluid template engine (MIT licensed)**
```csharp
// Scriban is a lightweight, fast .NET template engine
// https://github.com/scriban/scriban

// var template = Template.Parse(File.ReadAllText("invoice.sbn"));
// var htmlResult = template.Render(new { Customer = "Acme Corp", Amount = 4200m });

// Then render with IronPDF:
// var pdf = await renderer.RenderHtmlAsPdfAsync(htmlResult);
```

**Option B: Razor in ASP.NET Core (if already in that stack)**
```csharp
// var html = await razorRenderer.RenderViewToStringAsync("Invoice", model);
// var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### STA Thread Infrastructure Removal

After migration, the STA workaround code should be removed. Audit for:

```bash
# STA thread creation patterns
rg "ApartmentState\.STA\|SetApartmentState\|STAThread" --type cs -n

# Request queuing (may have been managing TextControl concurrency)
rg "SemaphoreSlim\|BlockingCollection\|ConcurrentQueue" --type cs -n

# After removing TextControl, verify these are no longer needed
# before deleting — some may serve other purposes
```

### DOCX Path Separation

If your codebase uses TextControl for both DOCX editing AND PDF export, separate the migration:

- Keep TextControl for DOCX creation/editing
- Replace only the PDF export path with IronPDF (HTML → PDF)
- Remove the DOCX → TextControl → PDF path if applicable

### Page Indexing

IronPDF uses 0-based page indexing. Verify TextControl's page indexing in their documentation — audit any page manipulation code.

---

## Performance Considerations

### Concurrent Rendering Without STA Overhead

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Before: each request needed its own STA thread (serialized or pooled)
// After: Task.WhenAll with no threading constraint
// https://ironpdf.com/examples/parallel/

var sw = Stopwatch.StartNew();

var requests = Enumerable.Range(1, 20)
    .Select(i => $"<html><body><h1>Document {i}</h1></body></html>")
    .ToArray();

var pdfs = await Task.WhenAll(requests.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
}));

sw.Stop();
Console.WriteLine($"20 concurrent PDFs in {sw.Elapsed.TotalMilliseconds:F0}ms — no STA overhead");
foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

// TextControl: tx.Dispose() or using block
// IronPDF: 'using' on PdfDocument

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// For API response:
return pdf.BinaryData;

// For file:
pdf.SaveAs(outputPath);
// pdf disposed at end of 'using' block
```

### Renderer Instance Per Task

```csharp
using IronPdf;
using System.Threading.Tasks;

// Safe pattern for concurrent rendering:
// One ChromePdfRenderer per Task (verify thread-safety in IronPDF docs)
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

// If renderer is shared, verify thread-safety in current IronPDF documentation
// The conservative pattern is one renderer per concurrent operation
```

---

## Migration Checklist

### Pre-Migration
- [ ] Find all TextControl usage (`rg "TXTextControl\|ServerTextControl" --type cs`)
- [ ] Find STA thread workaround code (`rg "ApartmentState\.STA\|SetApartmentState" --type cs`)
- [ ] Identify concurrent request concurrency ceiling (current semaphore/queue settings)
- [ ] Determine if DOCX path and PDF path need to be separated
- [ ] Identify mail merge use cases — choose template engine replacement
- [ ] Document page sizes, fonts, headers/footers in use
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove TextControl NuGet packages
- [ ] Add license key at application startup
- [ ] Replace `ServerTextControl` + STA thread pattern with `ChromePdfRenderer`
- [ ] Replace `tx.Load(html, StringStreamType.HTMLFormat)` with HTML string input
- [ ] Replace `tx.Save(path, StreamType.AdobePDF)` with `pdf.SaveAs(path)`
- [ ] Remove STA thread creation, `.SetApartmentState()`, `.Start()`, `.Join()` boilerplate
- [ ] Remove semaphore/queue infrastructure managing TextControl concurrency
- [ ] Replace secondary merge library with `PdfDocument.Merge()`
- [ ] Replace security via secondary library with `pdf.SecuritySettings`

### Testing
- [ ] Render each HTML template and compare output
- [ ] Validate concurrent rendering at target throughput (the original scaling problem)
- [ ] Verify STA thread wrapper removal doesn't break other code paths
- [ ] Test merge, watermark, and security features
- [ ] Test in Docker/Linux if applicable
- [ ] Benchmark throughput vs previous STA-constrained baseline

### Post-Migration
- [ ] Remove all `TXTextControl.*` NuGet packages (or only PDF-path packages if DOCX kept)
- [ ] Remove STA threading infrastructure code
- [ ] Remove semaphore/queue that was throttling TextControl concurrency
- [ ] Update throughput/scaling documentation — ceiling is now much higher

---

## Before You Ship

The STA threading constraint and the secondary library accumulation are both resolved by this migration. The concurrent rendering capacity — which was limited by thread pool size and STA serialization — is no longer the bottleneck; compute and I/O are.

The remaining scope to plan for is mail merge replacement (if that was a TextControl use case) and the DOCX path decision (keep TextControl for it, or replace separately).

**Discussion question:** After migrating, what did you add to the migration checklist — particularly around STA thread removal, mail merge replacement patterns, or concurrent rendering validation?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
