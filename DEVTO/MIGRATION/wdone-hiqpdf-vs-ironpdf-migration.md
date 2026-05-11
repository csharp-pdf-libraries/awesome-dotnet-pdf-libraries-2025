# The HiQPdf to IronPDF migration nobody dramatised

Here's a concrete missing-feature scenario that comes up: you've been using HiQPdf for HTML-to-PDF conversion and it works, but you need to add PDF/A compliance output for document archiving. You open the HiQPdf docs looking for a PDF/A export option, and it isn't there — or it's marked as a feature in a tier you don't currently have. That's the trigger for this article. Not drama, just a gap in capability that forces a library evaluation.

This walkthrough covers the migration path from HiQPdf to IronPDF: what transfers cleanly, what needs attention, and what to measure before you commit.

---

## Why migrate (without drama)

Nine reasons teams evaluate alternatives to HiQPdf — take what applies:

1. **Missing features** — PDF/A compliance, specific digital signature workflows, or advanced PDF manipulation operations may not be in the version you're on.
2. **Platform support gap** — HiQPdf's Linux support posture matters if you're moving workloads to Linux containers. Verify current state before assuming.
3. **Commercial license model** — both HiQPdf and IronPDF are commercial. If you're re-evaluating, compare what's included at your tier. No pricing analysis here — do this yourself against your usage.
4. **CSS fidelity deltas** — even among Chromium-based renderers, version differences and configuration options produce different output. Teams with pixel-sensitive documents test this case by case.
5. **API verbosity** — some operations require more boilerplate in HiQPdf than equivalent IronPDF calls (or vice versa — verify for your specific operations).
6. **Documentation coverage** — HiQPdf's public documentation scope. If you're hitting underdocumented corners, that's friction.
7. **Thread safety and concurrency model** — verify how HiQPdf handles concurrent render requests before scaling.
8. **Ecosystem integration** — NuGet, Azure DevOps, GitHub Actions compatibility. Verify any CI/CD-specific package consumption nuances.
9. **Support responsiveness** — commercial library support quality varies. If you're blocked on issues, that's a migration signal.

### Comparison table

| Aspect | HiQPdf | IronPDF |
|---|---|---|
| Focus | HTML-to-PDF + PDF operations | HTML-to-PDF + full PDF manipulation |
| Pricing | Commercial — verify at hiqpdf.com | Commercial — verify at ironsoftware.com |
| API Style | Converter + document classes | Renderer + document model |
| Learning Curve | Medium | Medium |
| HTML Rendering | Chromium-based | Chromium-based |
| Page Indexing | Verify in HiQPdf docs | 0-based |
| Thread Safety | Verify in HiQPdf docs | Renderer instance reuse — see async docs |
| Namespace | `HiQPdf` | `IronPdf` |

---

## Benchmark framing

This section covers what to measure — not invented numbers. Run these benchmarks in your environment before making a call.

### What to measure

| Metric | Why it matters | How to test |
|---|---|---|
| Single-page HTML-to-PDF time (ms) | Baseline render performance | `Stopwatch` around render call, 100 iterations, median |
| Multi-page document render time | Scales differently across renderers | 20-page template, 50 iterations |
| Memory allocation per render | Critical for high-volume services | `GC.GetTotalMemory()` before and after; or memory profiler |
| Concurrent renders throughput | Relevant if scaling horizontally | N threads, M renders each, measure wall clock |
| Cold start time | Relevant for serverless / per-request scaling | First render after process start vs warm |
| Output file size for same input | Affects storage and transfer costs | Same HTML, compare byte counts |
| CSS fidelity score | Your real templates, visual comparison | Side-by-side screenshot diff |

### Benchmark scaffold (use for both libraries)

```csharp
using System;
using System.Diagnostics;
using System.Collections.Generic;
using IronPdf; // Swap to HiQPdf namespace when testing that library

class RenderBenchmark
{
    static void Main(string[] args)
    {
        IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

        const int warmupRounds = 3;
        const int timedRounds = 50;
        string html = System.IO.File.ReadAllText("test_template.html");

        var renderer = new ChromePdfRenderer();

        // Warm up — exclude from timing
        for (int i = 0; i < warmupRounds; i++)
        {
            using var warmup = renderer.RenderHtmlAsPdf(html);
        }

        // Timed runs
        var times = new List<long>();
        var sw = new Stopwatch();

        for (int i = 0; i < timedRounds; i++)
        {
            sw.Restart();
            using var pdf = renderer.RenderHtmlAsPdf(html);
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }

        times.Sort();
        Console.WriteLine($"Median: {times[timedRounds / 2]}ms");
        Console.WriteLine($"P95:    {times[(int)(timedRounds * 0.95)]}ms");
        Console.WriteLine($"P99:    {times[(int)(timedRounds * 0.99)]}ms");
        Console.WriteLine($"Min:    {times[0]}ms");
        Console.WriteLine($"Max:    {times[timedRounds - 1]}ms");
    }
}
```

Run the same scaffold with HiQPdf substituted in. Compare your numbers — not numbers from this article.

---

## Migration complexity assessment

### Effort by feature

| Feature | HiQPdf approach | Effort to migrate |
|---|---|---|
| HTML string to PDF | Converter class | Low |
| URL to PDF | Converter class | Low |
| HTML file to PDF | Converter class | Low |
| PDF/A compliance output | Verify HiQPdf support — may be N/A | Low in IronPDF if starting from zero |
| Merge PDFs | Verify HiQPdf support | Low |
| Watermark | Verify HiQPdf support | Low |
| Password protection | Verify HiQPdf support | Low |
| Custom margins | Options object | Low |
| Headers / footers | Verify HiQPdf API | Medium |
| Digital signatures | Verify HiQPdf support | Medium — verify in IronPDF docs |
| Concurrent rendering | Verify HiQPdf thread model | Medium |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| PDF/A compliance required | Verify HiQPdf tier; IronPDF has it — test both outputs for compliance |
| Windows-only deployment | Both should work — verify |
| Linux/Docker deployment | Verify HiQPdf Linux support carefully before committing to either |
| Both libraries are commercial | Compare feature set per license tier against your specific needs |

---

## Before you start

### Prerequisites

- .NET 6+ recommended
- Dev environment with HiQPdf currently working
- Your complete HTML template set for render comparison
- A PDF/A validator (PAC, veraPDF — free tools) if that's the trigger

### Find HiQPdf references in your codebase

```bash
# Find all files using HiQPdf
rg -l "HiQPdf\|hiqpdf" --type cs -i

# Find class instantiation
rg "HtmlToPdf\|PdfDocument" --type cs -n
# Note: class names are generic — filter by using/namespace context

# Find using statements
rg "using HiQPdf" --type cs -n

# Find license key references
rg "SetLicense\|License" --type cs -n | grep -i hiq
```

### Remove HiQPdf, install IronPDF

```bash
# Remove HiQPdf — verify exact package name first
dotnet remove package HiQPdf

# Install IronPDF
dotnet add package IronPdf

# Restore
dotnet restore
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (HiQPdf license — verify exact method name in HiQPdf docs):**
```csharp
using HiQPdf; // verify namespace

// HiQPdf typically sets license key before use
// Verify current API at hiqpdf.com documentation
HtmlToPdf.SetLicense("YOUR_HIQPDF_KEY"); // verify method name
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR_IRONPDF_LICENSE_KEY";
// Guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
using HiQPdf; // verify — may have sub-namespaces
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3: Basic conversion

**Before:**
```csharp
using HiQPdf;

// Verify class and method names in HiQPdf docs — patterns below
// are illustrative; exact API differs by version
var converter = new HtmlToPdf(); // verify class name
var pdf = converter.ConvertHtmlString("<h1>Hello World</h1>"); // verify method
System.IO.File.WriteAllBytes("output.pdf", pdf);
```

**After:**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API mapping tables

### Namespace mapping

| HiQPdf | IronPDF | Notes |
|---|---|---|
| `HiQPdf` | `IronPdf` | Core — verify HiQPdf sub-namespaces |
| HiQPdf options classes | `IronPdf.Rendering` | Render configuration |
| HiQPdf PDF manipulation | `IronPdf` / `IronPdf.Editing` | Manipulation ops |

### Core class mapping

| HiQPdf class | IronPDF class | Description |
|---|---|---|
| `HtmlToPdf` | `ChromePdfRenderer` | Main rendering entry point |
| Converter options class | `ChromePdfRenderOptions` | Render configuration |
| PDF document class | `PdfDocument` | Represents a PDF |
| N/A | `PdfDocument` static methods | Merge, split, load from file |

### Document loading methods

| Operation | HiQPdf | IronPDF |
|---|---|---|
| HTML string | `ConvertHtmlString()` | `renderer.RenderHtmlAsPdf(html)` |
| URL | `ConvertUrl()` | `renderer.RenderUrlAsPdf(url)` |
| HTML file | File-based method | `renderer.RenderHtmlFileAsPdf(path)` |
| Existing PDF | File load method | `PdfDocument.FromFile(path)` |

### Page operations

| Operation | HiQPdf | IronPDF |
|---|---|---|
| Page count | Verify | `pdf.PageCount` |
| Paper size | Options property | `ChromePdfRenderOptions.PaperSize` |
| Margins | Options property | `ChromePdfRenderOptions.Margin*` |
| Orientation | Options property | `ChromePdfRenderOptions.PaperOrientation` |

### Merge/split operations

| Operation | HiQPdf | IronPDF |
|---|---|---|
| Merge | Verify support in HiQPdf | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | Verify support in HiQPdf | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

### 1. HTML to PDF

**Before (HiQPdf — verify exact API before using):**
```csharp
using HiQPdf;
using System;
using System.IO;

class HtmlToPdfExample
{
    static void Main()
    {
        // Verify HiQPdf class names and method signatures in official docs
        // The pattern below is illustrative
        HtmlToPdf.SetLicense("YOUR_HIQPDF_KEY"); // verify method name

        var converter = new HtmlToPdf();
        // Verify property names for paper size, margins, etc.
        converter.Document.PageSize = HiQPdfPageSize.A4; // verify enum/class names

        byte[] pdfBytes = converter.ConvertHtmlString(
            "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>",
            "https://localhost/" // base URL — verify if required
        );

        File.WriteAllBytes("invoice.pdf", pdfBytes);
        Console.WriteLine("Saved: invoice.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>"
);
pdf.SaveAs("invoice.pdf");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. PDF/A compliance output

**Before (HiQPdf — verify PDF/A support in your licensed tier):**
```csharp
using HiQPdf;
using System.IO;

class PdfAExample
{
    static void Main()
    {
        // Verify whether HiQPdf supports PDF/A in your tier
        // If not, this is the feature gap triggering migration
        HtmlToPdf.SetLicense("YOUR_KEY"); // verify method

        var converter = new HtmlToPdf();
        // PDF/A settings — verify if HiQPdf exposes this at all
        // If not present: verify in docs before concluding N/A
        // converter.Document.PdfStandard = PdfStandard.PdfA1b; // hypothetical — verify

        byte[] pdfBytes = converter.ConvertHtmlString("<h1>Archived Document</h1>");
        File.WriteAllBytes("archived.pdf", pdfBytes);
    }
}
```

**After (IronPDF with PDF/A):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Archived Document</h1>");

// Convert to PDF/A-3b for long-term archiving
pdf.SaveAsPdfA("archived_pdfa.pdf", IronPdf.PdfAVersions.PdfA3b);
// PDF/A guide: https://ironpdf.com/how-to/pdfa/
```

---

### 3. Watermark

**Before (HiQPdf — verify watermark API exists and method names):**
```csharp
using HiQPdf;
using System.IO;

class WatermarkExample
{
    static void Main()
    {
        // Verify HiQPdf watermark support and API
        HtmlToPdf.SetLicense("YOUR_KEY"); // verify

        // If HiQPdf doesn't support watermarking natively,
        // teams typically add a secondary library here
        // Pattern depends on your current approach — verify before migrating
        byte[] pdfBytes = File.ReadAllBytes("input.pdf");

        // Hypothetical watermark API — verify in HiQPdf docs:
        // var watermarker = new PdfWatermark(); // if exists
        // watermarker.Apply(pdfBytes, "CONFIDENTIAL"); // verify
        File.WriteAllBytes("watermarked.pdf", pdfBytes);
    }
}
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
    Opacity = 40,
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

**Before (HiQPdf — verify security API):**
```csharp
using HiQPdf;
using System.IO;

class SecurityExample
{
    static void Main()
    {
        // Verify HiQPdf security settings API
        HtmlToPdf.SetLicense("YOUR_KEY"); // verify

        var converter = new HtmlToPdf();
        // Hypothetical — verify property names in HiQPdf docs:
        // converter.Document.Security.UserPassword = "userpass"; // verify
        // converter.Document.Security.OwnerPassword = "ownerpass"; // verify

        byte[] pdf = converter.ConvertHtmlString("<h1>Secure Document</h1>");
        File.WriteAllBytes("secured.pdf", pdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Security;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Secure Document</h1>");
pdf.SecuritySettings.UserPassword = "userpass";
pdf.SecuritySettings.OwnerPassword = "ownerpass";
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Page indexing

Verify HiQPdf's page indexing convention in its docs. IronPDF uses 0-based indexing throughout. If you're migrating page manipulation code, audit all index references.

### Byte array vs file model

HiQPdf commonly returns `byte[]` from conversion. IronPDF returns a `PdfDocument` object, which you then save or stream. Update call sites accordingly:

```csharp
// HiQPdf pattern (illustrative — verify):
byte[] pdfBytes = converter.ConvertHtmlString(html);
File.WriteAllBytes("output.pdf", pdfBytes);

// IronPDF pattern:
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");

// Or to MemoryStream:
using var ms = new MemoryStream();
pdf.Stream.CopyTo(ms);
// Guide: https://ironpdf.com/how-to/pdf-memory-stream/
```

### Error handling model

HiQPdf may return null or empty bytes on failure; IronPDF throws exceptions. Update error handling:

```csharp
// IronPDF exception pattern
try
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
catch (IronPdf.Exceptions.IronPdfException ex)
{
    // Handle render errors
    Console.Error.WriteLine($"Render failed: {ex.Message}");
}
```

---

## Performance considerations

### Renderer reuse

```csharp
// Configure once, reuse for batch work
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 10;

foreach (var html in templates)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{Guid.NewGuid()}.pdf");
}
```

### Async for I/O-heavy workloads

```csharp
// Async rendering for web applications
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
await pdf.SaveAsAsync("output.pdf");
// Docs: https://ironpdf.com/how-to/async/
```

### Parallel rendering

```csharp
// Separate renderer per task — don't share across threads
var results = await Task.WhenAll(
    htmlItems.Select(async html =>
    {
        var r = new ChromePdfRenderer();
        return await r.RenderHtmlAsPdfAsync(html);
    })
);
```

### Edge cases

- **Chromium version differences:** Both libraries use Chromium, but potentially different versions. Test edge-case CSS rendering between them.
- **PDF/A validation:** If PDF/A is the feature trigger, validate output with PAC or veraPDF after migration — don't assume conformance without verification.
- **Memory profiling:** Profile actual memory consumption in your environment. Numbers from other contexts don't apply.

---

## Migration checklist

### Pre-migration

- [ ] Inventory all HiQPdf usages: `rg "HiQPdf" --type cs`
- [ ] Identify the specific feature gap that triggered evaluation
- [ ] Pull all HTML templates for render comparison testing
- [ ] Set up PDF/A validator if that's part of requirements (PAC, veraPDF)
- [ ] Verify IronPDF .NET version compatibility
- [ ] Confirm license/procurement path for IronPDF
- [ ] Document current HiQPdf API calls for mapping
- [ ] Set up IronPDF trial license in dev environment

### Code migration

- [ ] Remove HiQPdf NuGet package
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `using HiQPdf` imports with `using IronPdf`
- [ ] Replace license initialization
- [ ] Replace converter class with `ChromePdfRenderer`
- [ ] Replace HTML-to-PDF calls
- [ ] Replace URL-to-PDF calls
- [ ] Update output pattern from `byte[]` to `PdfDocument` model
- [ ] Update error handling from null-check pattern to try/catch
- [ ] Add PDF/A output where required

### Testing

- [ ] Render each HTML template and compare visual output side by side
- [ ] Run benchmark scaffold on your templates — record median and P95
- [ ] Validate PDF/A output with PAC or veraPDF
- [ ] Test merge, watermark, security operations
- [ ] Test concurrent rendering at expected peak throughput
- [ ] Test memory allocation under sustained load

### Post-migration

- [ ] Remove HiQPdf license key from config and secrets management
- [ ] Update deployment docs / runbooks
- [ ] Document benchmark results for team reference
- [ ] Monitor memory and CPU for first production week

---

## Done Migrating? Here's What's Next

The migration itself is mechanical — the interesting question is whether the output matches. Both HiQPdf and IronPDF use Chromium-based rendering, which means fidelity differences are likely to be subtle and template-specific rather than systematic.

**What version of HiQPdf are you migrating from, and did anything break unexpectedly during render testing?** Particularly interested in teams with complex CSS or multi-page table layouts — those tend to be where Chromium version differences surface.

---

*Tags: `dotnet`, `csharp`, `pdf`, `migration`*
