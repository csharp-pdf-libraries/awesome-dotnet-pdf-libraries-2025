# Migrating from DynamicPDF to IronPDF: a working migration in an afternoon

If your DynamicPDF project is generating PDFs from HTML and the output looks like it was rendered in 2009, you're not alone. DynamicPDF's HTML rendering pipeline was never its primary strength — it was built around a programmatic/layout API. Teams that started with invoice templates and then got feature-creep'd into "render this React-generated HTML report" often hit a wall. This article gives you a structured migration path to IronPDF, with benchmark comparisons where the data is real, and "verify before publishing" flags everywhere else.

You'll leave with copy-pasteable migration scaffolding, a full API mapping table, and four complete before/after code samples. Even if you keep DynamicPDF for some workloads, the patterns here are worth knowing.

---

## Why Migrate (Without Drama)

Teams typically reach for a different library when one or more of these triggers accumulates:

1. **HTML fidelity** — DynamicPDF renders HTML as a layout region, not a full browser engine. Complex CSS (flexbox, grid, custom fonts, SVG) may degrade or be ignored.
2. **CSS @media print support** — Headless-browser renderers honor print stylesheets; layout-API renderers often don't.
3. **JavaScript execution** — If your templates use JS (charting libraries, dynamic tables), DynamicPDF won't run it. IronPDF uses Chromium internally and waits for JS execution.
4. **Linux/container deployment** — Verify DynamicPDF's current Linux native-deps story; some versions require GDI+ shims.
5. **API verbosity** — DynamicPDF's programmatic API is expressive but verbose for simple "render this URL to PDF" tasks.
6. **Async support** — Check whether your version of DynamicPDF exposes async rendering. IronPDF has documented [async/parallel patterns](https://ironpdf.com/how-to/async/).
7. **Maintenance cadence** — If you're on an older CoreSuite version, check whether your .NET target (e.g., net8.0) is fully supported.
8. **Commercial licensing model** — License structures change; verify current pricing independently.
9. **Thread safety** — For high-concurrency servers, verify the DynamicPDF `Document` class thread-safety guarantees in their docs.
10. **PDF/A and compliance** — DynamicPDF has PDF/A support; IronPDF also has [PDF/A generation](https://ironpdf.com/how-to/pdfa/) — compare feature parity for your compliance requirements.

### Side-by-Side Comparison

| Aspect | DynamicPDF | IronPDF |
|---|---|---|
| Focus | Programmatic layout API | HTML/URL → PDF via Chromium |
| Pricing | Commercial, per-developer — verify | Commercial, per-developer — verify |
| API Style | Builder/layout pattern | Fluent + simple static methods |
| Learning Curve | Medium — layout model requires learning | Low for HTML-first workflows |
| HTML Rendering | Basic HTML regions (no full browser engine) | Chromium-based, full CSS/JS support |
| Page Indexing | 0-based | 0-based |
| Thread Safety | Verify in current docs | `ChromePdfRenderer` is reusable; docs confirm |
| Namespace | `ceTe.DynamicPDF` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Complexity | Notes |
|---|---|---|
| HTML string → PDF | Low | Direct 1:1 if you were using HtmlArea |
| URL → PDF | Low | DynamicPDF doesn't do this natively; net new |
| Programmatic page layout | High | No equivalent in IronPDF; keep DynamicPDF or move to HTML templates |
| Text extraction | Medium | Different API shape; verify extraction capabilities |
| Merge documents | Low | Both support merge; API names differ |
| Split documents | Low | Similar complexity |
| Watermark / stamp | Medium | DynamicPDF uses label/image placement; IronPDF has stamp API |
| Password protection | Low | Both expose encrypt/password APIs |
| Digital signatures | Medium-High | Verify feature parity in both libraries |
| PDF/A compliance | Medium | Both claim support; verify output conformance with validator |
| JavaScript-rendered HTML | N/A → Low | DynamicPDF doesn't support this; new capability in IronPDF |

### Decision Matrix

| Scenario | Recommendation |
|---|---|
| All PDF generation is programmatic layout (no HTML input) | Stay on DynamicPDF — IronPDF won't simplify this |
| Mixed: some programmatic layout + some HTML rendering | Partial migration or wrapper abstraction |
| Primarily HTML/CSS/URL rendering | IronPDF is a natural fit; migrate |
| High-volume headless server with Linux containers | Verify both libraries' Linux support before deciding |

---

## Before You Start

### Prerequisites
- .NET 6+ recommended (both libraries support it — verify for your exact targets)
- Access to your NuGet feed
- `ripgrep` or `grep` available for codebase scanning

### Find DynamicPDF References

```bash
# Find all files referencing DynamicPDF namespaces
rg "ceTe\.DynamicPDF" --type cs -l

# Find specific Document/Page usages
rg "new Document\(\)|new Page\(" --type cs

# Find HtmlArea usages — these are your HTML migration targets
rg "HtmlArea" --type cs
```

### Uninstall / Install

```bash
# Remove DynamicPDF
dotnet remove package DynamicPDF.CoreSuite

# Install IronPDF
dotnet add package IronPdf

# Verify
dotnet list package
```

Set your license key before running (see [IronPDF license setup](https://ironpdf.com/how-to/license-keys/)):

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Or via environment variable: IRONPDF_LICENSEKEY
// Or in appsettings.json — verify current config options in docs
```

---

## Quick Start Migration (3 Steps)

### Step 1: License

```csharp
// Before (DynamicPDF — verify exact license API in their current docs)
// ceTe.DynamicPDF.License.SetLicenseKey("YOUR-KEY"); // verify method name

// After (IronPDF)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Docs: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace Imports

```csharp
// Before
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements;
using ceTe.DynamicPDF.PageElements.Html; // verify — HtmlArea namespace

// After
using IronPdf;
```

### Step 3: Basic HTML → PDF Conversion

```csharp
// Before (DynamicPDF HtmlArea approach — verify current API)
// var doc = new Document();
// var page = new Page(PageSize.Letter);
// var html = new HtmlArea("<h1>Hello</h1>", 0, 0, 612, 792); // verify constructor sig
// page.Elements.Add(html);
// doc.Pages.Add(page);
// doc.Draw("output.pdf");

// After (IronPDF)
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("output.pdf");
// Docs: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| DynamicPDF | IronPDF | Notes |
|---|---|---|
| `ceTe.DynamicPDF` | `IronPdf` | Core namespace |
| `ceTe.DynamicPDF.PageElements` | `IronPdf` (integrated) | No separate elements namespace |
| `ceTe.DynamicPDF.PageElements.Html` | `IronPdf` | HtmlArea → `ChromePdfRenderer` |

### Core Class Mapping

| DynamicPDF Class | IronPDF Class | Description |
|---|---|---|
| `Document` | `PdfDocument` | The PDF document object |
| `Page` | `PdfDocument` (pages collection) | Pages are accessed via document |
| `HtmlArea` | `ChromePdfRenderer` | HTML rendering engine |
| `MergeDocument` | `PdfDocument.Merge()` | Merge helper |

### Document Loading

| Operation | DynamicPDF | IronPDF |
|---|---|---|
| Load from file | `new Document(path)` — verify | `PdfDocument.FromFile(path)` |
| Load from bytes | verify in docs | `PdfDocument.FromBytes(bytes)` |
| Load from stream | verify in docs | `PdfDocument.FromStream(stream)` |
| Render HTML string | `HtmlArea` + `Document` | `renderer.RenderHtmlAsPdf(html)` |

### Page Operations

| Operation | DynamicPDF | IronPDF |
|---|---|---|
| Page count | `doc.Pages.Count` — verify | `pdf.PageCount` |
| Get page | `doc.Pages[i]` — verify | `pdf.Pages[i]` |
| Add page | `doc.Pages.Add(page)` — verify | see docs |
| Remove page | verify in docs | `pdf.RemovePage(index)` |

### Merge / Split

| Operation | DynamicPDF | IronPDF |
|---|---|---|
| Merge two PDFs | `MergeDocument.Merge(a, b)` — verify | `PdfDocument.Merge(a, b)` |
| Split at page | verify in docs | `pdf.CopyPages(0, n)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (DynamicPDF — verify API names):**

```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements.Html; // verify namespace

class Program
{
    static void Main()
    {
        // DynamicPDF license — verify method name
        // License.SetLicenseKey("YOUR-KEY");

        string html = "<h1>Quarterly Report</h1><p>Data as of Q4.</p>";

        // Create document with a letter-size page
        var doc = new Document();
        var page = new Page(PageSize.Letter); // verify PageSize enum

        // HtmlArea positions HTML as a region on the page
        // Signature: verify in current DynamicPDF docs
        var area = new HtmlArea(html, 0, 0, 612, 792);
        page.Elements.Add(area); // verify Elements API

        doc.Pages.Add(page);
        doc.Draw("report.pdf"); // verify Draw method name
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        // Docs: https://ironpdf.com/how-to/license-keys/

        string html = "<h1>Quarterly Report</h1><p>Data as of Q4.</p>";

        var renderer = new ChromePdfRenderer();
        // renderer.RenderingOptions allows margin, paper size, etc.
        // Docs: https://ironpdf.com/how-to/rendering-options/

        using var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report.pdf");
        // Docs: https://ironpdf.com/how-to/html-string-to-pdf/
    }
}
```

---

### 2. Merge PDFs

**Before (DynamicPDF — verify API):**

```csharp
using ceTe.DynamicPDF;
using System.IO;

class MergeSample
{
    static void Main()
    {
        // DynamicPDF merge — verify MergeDocument class exists in your version
        // and that this API signature is current

        string pathA = "doc_a.pdf";
        string pathB = "doc_b.pdf";
        string output = "merged.pdf";

        // MergeDocument approach — verify exact constructor/method
        // var merged = MergeDocument.Merge(pathA, pathB); // verify
        // merged.Draw(output); // verify

        // Fallback if using Document directly:
        var docA = new Document(pathA); // verify
        var docB = new Document(pathB); // verify
        // Page copy approach — verify in DynamicPDF docs
        // foreach (var page in docB.Pages) docA.Pages.Add(page);
        docA.Draw(output); // verify
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class MergeSample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var docA = PdfDocument.FromFile("doc_a.pdf");
        using var docB = PdfDocument.FromFile("doc_b.pdf");

        // Static merge — returns new PdfDocument
        using var merged = PdfDocument.Merge(docA, docB);
        merged.SaveAs("merged.pdf");
        // Docs: https://ironpdf.com/how-to/merge-or-split-pdfs/
    }
}
```

---

### 3. Watermark

**Before (DynamicPDF — verify API):**

```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements; // verify namespace

class WatermarkSample
{
    static void Main()
    {
        // DynamicPDF watermarking via Label element — verify current API
        var doc = new Document("input.pdf"); // verify load API

        foreach (var page in doc.Pages) // verify iteration
        {
            // Label or image overlay — verify element types and constructors
            // var label = new Label("CONFIDENTIAL", 200, 400, 200, 50); // verify
            // label.Angle = 45; // verify
            // page.Elements.Add(label); // verify
        }

        doc.Draw("watermarked.pdf"); // verify
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;
using IronPdf.Editing;

class WatermarkSample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        // Stamp text as watermark on all pages
        // Docs: https://ironpdf.com/how-to/stamp-text-image/
        var stamper = new TextStamper
        {
            Text = "CONFIDENTIAL",
            FontSize = 40,
            Opacity = 50,
            Rotation = 45
        };

        pdf.ApplyStamp(stamper); // applies to all pages
        pdf.SaveAs("watermarked.pdf");
        // For HTML-based watermarks: https://ironpdf.com/how-to/custom-watermark/
    }
}
```

---

### 4. Password Protection

**Before (DynamicPDF — verify API):**

```csharp
using ceTe.DynamicPDF;

class SecuritySample
{
    static void Main()
    {
        var doc = new Document("input.pdf"); // verify

        // DynamicPDF security — verify class name and property names
        // var security = new Security(); // verify
        // security.UserPassword = "user123"; // verify
        // security.OwnerPassword = "owner456"; // verify
        // doc.Security = security; // verify attachment method

        doc.Draw("secured.pdf"); // verify
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;
using IronPdf.Security;

class SecuritySample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        // Set user + owner passwords
        // Docs: https://ironpdf.com/how-to/pdf-permissions-passwords/
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";

        // Optional: restrict printing, copying
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;

        pdf.SaveAs("secured.pdf");
    }
}
```

---

## Critical Migration Notes

### Page Indexing
Both DynamicPDF and IronPDF use 0-based page indexing, but verify this in your specific DynamicPDF version. An off-by-one error here will silently affect the wrong pages.

### API Patterns: Draw vs SaveAs
DynamicPDF uses `doc.Draw(path)` or `doc.Draw(stream)` as its output method. IronPDF uses `pdf.SaveAs(path)` or `pdf.Stream` / `pdf.BinaryData` for stream output. This is a mechanical find-replace, but watch for spots where you're passing the draw output to another method — the IronPDF equivalent returns a byte array or `MemoryStream` directly from the render call.

### Status Codes vs Exceptions
DynamicPDF may return status codes or use event-based error reporting in some versions. IronPDF throws exceptions on failure. If you have try/catch blocks keyed to DynamicPDF status codes, rework them as standard .NET exception handling.

### HtmlArea Positioning
DynamicPDF's `HtmlArea` takes explicit `x, y, width, height` positioning. IronPDF's renderer fills the entire page by default and uses `RenderingOptions` for margins and paper size. If you were using `HtmlArea` for partial-page HTML rendering (placing HTML alongside other page elements), there's no direct equivalent in IronPDF — you'd need to pre-compose the HTML to represent the full page, or use IronPDF's stamp/overlay API to layer content.

---

## Performance Considerations

### Renderer Reuse
`ChromePdfRenderer` is designed to be reused. Instantiate once, use many times. Avoid newing it per request in a server context.

```csharp
// Good pattern for ASP.NET or worker services
public class PdfService
{
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    public byte[] GeneratePdf(string html)
    {
        using var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Disposal
`PdfDocument` implements `IDisposable`. Use `using` consistently, especially in loops or server workloads. For [parallel rendering patterns](https://ironpdf.com/examples/parallel/) — creating multiple renderer instances across threads — see the IronPDF parallel docs.

### Async
IronPDF exposes async render methods. For high-throughput servers, prefer `await renderer.RenderHtmlAsPdfAsync(html)` over the synchronous form. See [async docs](https://ironpdf.com/how-to/async/).

### DynamicPDF Programmatic Layouts
If you're migrating a workload that was previously built with DynamicPDF's layout engine (TextArea, ImageArea, etc.) and you're converting it to HTML templates, budget time for template QA. HTML rendering faithfully represents CSS, but your layout logic needs to move into HTML/CSS, not C# code. This is typically the highest-effort part of a DynamicPDF migration.

---

## Benchmark Notes

> **Important:** The numbers below are illustrative of the *type* of benchmarking worth running, not published figures. Run your own benchmarks on your hardware and workload before making migration decisions based on performance.

Aspects worth measuring for your specific case:

| Test | What to measure | Tooling |
|---|---|---|
| Simple HTML → PDF throughput | Renders/second, p95 latency | BenchmarkDotNet |
| Memory usage per render | Peak MB, GC pressure | dotMemory or `dotnet-counters` |
| Complex CSS fidelity | Visual diff of output | Playwright screenshot comparison |
| Parallel render scaling | Renders/sec at N threads | Custom load test |
| Cold start time | First-render latency (Chromium init) | Stopwatch around first call |

IronPDF's first render is slower than subsequent ones because of Chromium initialization. Warm up the renderer at application startup if latency matters. DynamicPDF's layout engine has a lighter startup profile; factor this into your p50/p99 analysis.

---

## Migration Checklist

### Pre-Migration
- [ ] Inventory all `ceTe.DynamicPDF` usages via `rg` / grep
- [ ] Identify `HtmlArea` usages — these are your primary migration targets
- [ ] Identify programmatic layout code (TextArea, ImageArea, etc.) — these may NOT migrate to IronPDF
- [ ] Confirm DynamicPDF license terms for parallel deployment
- [ ] Set up IronPDF license key in your config system
- [ ] Confirm .NET target compatibility for IronPDF in your environments
- [ ] Run a benchmark on a representative workload before migrating
- [ ] Review IronPDF [rendering options](https://ironpdf.com/how-to/rendering-options/) for paper size / margin config

### Code Migration
- [ ] Replace `ceTe.DynamicPDF` usings with `IronPdf`
- [ ] Replace `new Document()` + `new Page()` + `HtmlArea` with `ChromePdfRenderer` + `RenderHtmlAsPdf`
- [ ] Replace `doc.Draw(path)` with `pdf.SaveAs(path)`
- [ ] Replace `MergeDocument.Merge()` with `PdfDocument.Merge()`
- [ ] Replace security/password API calls — verify DynamicPDF security API names first
- [ ] Replace watermark/label elements with `TextStamper` or HTML-based watermark
- [ ] Update exception handling from status-code patterns to try/catch
- [ ] Ensure `ChromePdfRenderer` is instantiated as a singleton/scoped service, not per-request
- [ ] Add `using` disposal to all `PdfDocument` instances
- [ ] Update page index references — confirm 0-based across both sides

### Testing
- [ ] Visual regression test on all PDF outputs (screenshot diff recommended)
- [ ] Test with your most complex HTML/CSS template
- [ ] Test password-protected output with a PDF reader (not just code)
- [ ] Test merge output page count = sum of inputs
- [ ] Test watermark opacity and rotation
- [ ] Test under concurrent load (at least 10 parallel renders)
- [ ] Verify PDF/A compliance if required (use a conformance validator)

### Post-Migration
- [ ] Remove `DynamicPDF.CoreSuite` NuGet package
- [ ] Update deployment docs / Dockerfiles if native-deps changed
- [ ] Monitor memory usage for first week in production
- [ ] Archive DynamicPDF license keys for audit trail

---

## Before You Ship

The sharpest edge in a DynamicPDF → IronPDF migration is the programmatic layout code. If your codebase is full of `TextArea`, `ImageArea`, and `Column` layout logic, you're not migrating a library — you're migrating a rendering architecture. That work belongs in HTML templates, not in a swap of `using` statements. Plan accordingly.

For `HtmlArea`-heavy codebases, the migration is mechanical and the payoff is immediate — full CSS/JS rendering with far less boilerplate.

**Technical question for comments:** If you've migrated from a programmatic PDF layout API to an HTML-first renderer, how did you handle the cases where HTML layout didn't match the original pixel-perfect layout? Specific workarounds welcome — especially for multi-column layouts and precise absolute positioning.

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
