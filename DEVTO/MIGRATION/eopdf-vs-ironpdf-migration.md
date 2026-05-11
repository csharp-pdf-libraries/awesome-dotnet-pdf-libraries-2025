---
title: "Migrating from EO.Pdf to IronPDF: the parts that actually matter"
published: false
tags: dotnet, csharp, pdf, migration
---

The error message that sends most teams down the EO.Pdf migration path looks something like this: a hard-to-trace exception inside the static `HtmlToPdf` pipeline, triggered by a perfectly valid HTML file that rendered fine last deploy. EO.Pdf supports .NET Core 3.1+ on Windows only — Linux and macOS are not officially supported targets — and that surface area becomes a problem the moment your deployment plan crosses platforms. This article is a practical, troubleshooting-first guide through the migration to IronPDF, structured around the problems teams actually encounter, not a sales pitch.

By the end, you'll have a working migration scaffold, the API mappings, and four complete before/after code examples. The patterns here are useful whether you migrate everything or just the problem paths.

---

## Why Migrate (Without Drama)

Migrations off EO.Pdf tend to happen when one of these pressure points tips over:

1. **No official Linux or macOS support on .NET Core** — Per [Essential Objects' own documentation](https://www.essentialobjects.com/Doc/Common/dotnetcore.html), EO.Pdf supports .NET Core 3.1+ on Windows only. If your deployment plan includes Linux containers or macOS dev machines, the EO.Pdf path runs out.
2. **Static global options are not thread-safe** — EO.Pdf's primary entry point is the static `HtmlToPdf` class with shared `HtmlToPdf.Options`. Mutating that global state from multiple threads is awkward in multi-tenant web applications.
3. **Legacy architecture baggage** — EO.Pdf transitioned from an embedded IE/Trident engine to a Chromium-based renderer in later versions, which has left behind API quirks and version-to-version behavior differences.
4. **Bundled Chromium footprint** — EO.Pdf bundles its own Chromium renderer, which inflates Docker images and install size. IronPDF also bundles Chromium, so this is a wash if you're comparing renderers — but it's a real cost vs. non-Chrome libraries.
5. **`@page` and print media queries** — Print fidelity may vary if your templates were designed for screen, and debugging is harder when the engine has its own historical quirks.
6. **Limited async support** — Synchronous-only rendering becomes a bottleneck in async ASP.NET controller pipelines. IronPDF offers `RenderHtmlAsPdfAsync` / `RenderUrlAsPdfAsync` natively.
7. **Per-developer commercial license** — EO.Pdf is sold as paid Single, 3-License Bundle, Corporate, or Corporate Plus tiers with a 30-day watermarked trial. Pricing is shown only on the order page; check [essentialobjects.com/order](https://www.essentialobjects.com/order) before budgeting.
8. **Debug experience** — When EO.Pdf fails, the exception stack often lands inside library internals. IronPDF surfaces standard .NET exceptions with actionable messages.
9. **No JavaScript execution by default** — EO.Pdf's pre-Chromium versions don't execute JavaScript. If you're on a legacy install, charts and lazy-loaded content rendered after page load are invisible.
10. **Mixed APIs** — EO.Pdf mixes the static `HtmlToPdf` class, the object model around `PdfDocument`, and the Advanced Content Model (ACM) for hand-positioned content. IronPDF unifies on `ChromePdfRenderer` + HTML/CSS.

### Side-by-Side Comparison

| Aspect | EO.Pdf | IronPDF |
|---|---|---|
| Renderer | Bundled Chromium (transitioned from IE/Trident) | Embedded Chromium from the start |
| Platform support | Windows-only on .NET Core | Windows, Linux, macOS, Docker, Azure |
| Configuration | Static `HtmlToPdf.Options` (global, not thread-safe) | Instance `ChromePdfRenderer` (thread-safe) |
| Pricing | Single / 3-License / Corporate / Corporate Plus, paid only (30-day trial) | Per-developer commercial; free dev trial |
| API Style | Mixed static + ACM object model | Unified renderer + HTML/CSS |
| Modern .NET | .NET Framework + .NET Core 3.1+ (Windows only) | .NET Framework + .NET 6/7/8/9 cross-platform |
| Async | Limited | Full `RenderHtmlAsPdfAsync` / `RenderUrlAsPdfAsync` |
| Namespace | `EO.Pdf` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Complexity | Notes |
|---|---|---|
| HTML string → PDF | Low | Static `HtmlToPdf.ConvertHtml` → instance `RenderHtmlAsPdf` |
| URL → PDF | Low | Direct API on both; IronPDF adds full JS support |
| HTML file → PDF | Low | Path input, similar concept |
| Text extraction | Medium | API shape differs |
| Merge documents | Low | Both expose static `PdfDocument.Merge(...)` |
| Watermark | Medium | ACM positioning → `ApplyWatermark` or `HtmlStamper` |
| Password protection | Low | `doc.Security.*` → `pdf.SecuritySettings.*` |
| Headers/footers via events | Medium-High | `AfterRenderPage` event → `HtmlHeaderFooter` template |
| JavaScript rendering | N/A → Low | Full Chromium JS in IronPDF |
| CSS Grid/Flexbox | Variable → Low | Chromium handles modern CSS uniformly |

### Decision Matrix

| Scenario | Recommendation |
|---|---|
| Running on .NET Framework only, Windows only, no migration plans | EO.Pdf may continue to suit you |
| Targeting net6+ or Linux containers | Strong reason to migrate — EO.Pdf isn't officially supported there |
| Heavy use of `AfterRenderPage` event for per-page content | Plan extra time; the model differs |
| Multi-threaded ASP.NET workloads on EO.Pdf static options | Migrate — the static option model is the root issue |
| Minimal HTML, simple output, single-tenant Windows service | Lower urgency; migrate when convenient |

---

## Troubleshooting Common EO.Pdf Problems (and IronPDF Equivalents)

This section addresses the actual runtime problems teams hit, before and after migration.

### Problem 1: Application Fails to Run on Linux Containers

**EO.Pdf cause:** EO.Pdf does not officially support .NET Core on Linux or macOS — only on Windows. Attempting to run in a Linux container is not a supported configuration regardless of which packages you install.

**EO.Pdf workaround:** There isn't an officially supported one — the recommended path is to stay on Windows or migrate.

**IronPDF equivalent situation:** IronPDF runs on Linux on .NET Core / .NET 6+. It uses Chromium and requires a set of system packages.

```bash
# IronPDF Linux dependencies (Debian/Ubuntu)
# Current list documented at https://ironpdf.com/how-to/linux/
apt-get install -y \
  libnss3 libatk1.0-0 libatk-bridge2.0-0 libcups2 \
  libdrm2 libxkbcommon0 libxcomposite1 libxdamage1 \
  libxrandr2 libgbm1 libpango-1.0-0 libcairo2 libasound2

# Or use the prebuilt Docker image
# FROM ironpdf/ironpdf-dotnet:latest
```

The pattern — install system deps, then run — is the same as you'd expect for any Chromium-backed library. IronPDF documents the Linux setup explicitly.

---

### Problem 2: Static Options Causing Cross-Thread Bleed

**EO.Pdf cause:** `HtmlToPdf.Options` is global. Two ASP.NET requests setting different page sizes or base URLs can stomp on each other, and the failure mode is subtle — wrong margins or wrong base URL in a PDF, with no exception.

**Debugging approach for EO.Pdf:** Inspect every call site that mutates `HtmlToPdf.Options.*` and confirm whether it's being run on a thread that could overlap with another request. The fix in EO.Pdf is to serialize access or always reset all options at the start of each operation — neither is pleasant.

**IronPDF:** Each `ChromePdfRenderer` instance owns its own `RenderingOptions`. Spin up a renderer per request (or per scope) and there is no shared state to manage:

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.BaseUrl = new Uri("https://example.com");

using var pdf = renderer.RenderHtmlAsPdf("<h1>Test</h1>");
pdf.SaveAs("output.pdf");
```

---

### Problem 3: CSS Not Rendering Correctly

**EO.Pdf behavior:** Older EO.Pdf versions used an IE/Trident-based engine; newer versions use Chromium. Mixed-version environments produce inconsistent CSS support, and the silent-ignore behavior of the older engine is hard to debug.

**Debugging approach for EO.Pdf:** Confirm which renderer your installed version uses, then simplify CSS progressively until rendering looks correct. Test with inline styles first, then add stylesheets back piece by piece.

**IronPDF:** Uses Chromium consistently — what Chrome renders, IronPDF renders. Use browser DevTools to debug CSS first, then hand the same HTML to IronPDF. Enable print media emulation if needed:

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Use print CSS media type for @media print rules
renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;
// Docs: https://ironpdf.com/how-to/rendering-options/

using var pdf = renderer.RenderHtmlAsPdf(htmlWithComplexCss);
pdf.SaveAs("output.pdf");
```

---

### Problem 4: Memory Growth in Long-Running Process

**EO.Pdf situation:** `PdfDocument` exposes a `Dispose()` method — call it explicitly when you're done with each document, especially in worker services and Windows services that process many documents.

```csharp
using EO.Pdf;

var doc = new PdfDocument("input.pdf");
try
{
    // ... work with doc ...
}
finally
{
    doc.Dispose();
}
```

**IronPDF:** `PdfDocument` is `IDisposable`. Always use `using`.

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer(); // reuse this — don't dispose between renders

// PdfDocument: always using
using var pdf = renderer.RenderHtmlAsPdf(html);
byte[] data = pdf.BinaryData; // extract what you need
// pdf is disposed here — no lingering memory
```

---

## Before You Start

### Find EO.Pdf References

```bash
# Find all files using EO.Pdf namespace
rg "EO\.Pdf" --type cs -l

# Find HtmlToPdf static usages — primary migration targets
rg "HtmlToPdf\." --type cs

# Find PdfDocument references (EO.Pdf class — note same name as IronPDF)
rg "EO\.Pdf\.PdfDocument|new PdfDocument" --type cs

# Find ACM and event-callback usages — complex migration
rg "Acm|AfterRenderPage" --type cs
```

> **Naming collision warning:** EO.Pdf uses `PdfDocument` as a class name. IronPDF also uses `PdfDocument`. During a phased migration with both packages installed, you'll get ambiguous reference errors. Use fully qualified names or aliases:
> ```csharp
> using EoPdfDoc = EO.Pdf.PdfDocument;
> using IronPdfDoc = IronPdf.PdfDocument;
> ```

### Uninstall / Install

```bash
# Remove EO.Pdf
dotnet remove package EO.Pdf

# Install IronPDF
dotnet add package IronPdf

# Check no leftover EO.Pdf references
dotnet list package
```

### License Setup

```csharp
// Set before any IronPDF call
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Docs: https://ironpdf.com/how-to/license-keys/
```

---

## Quick Start Migration (3 Steps)

### Step 1: License

```csharp
// Before (EO.Pdf — license configured via Essential Objects' runtime; no
// equivalent of a single static API key assignment in current docs)
// See https://www.essentialobjects.com/ for current activation flow

// After (IronPDF)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 2: Namespace Imports

```csharp
// Before
using EO.Pdf;

// After
using IronPdf;
```

### Step 3: Basic HTML → PDF

```csharp
// Before (EO.Pdf — static API)
string html = "<h1>Hello</h1>";
HtmlToPdf.ConvertHtml(html, "output.pdf");

// After (IronPDF)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("output.pdf");
// Docs: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| EO.Pdf | IronPDF | Notes |
|---|---|---|
| `EO.Pdf` | `IronPdf` | Core namespace |
| `EO.Pdf.Acm` | HTML/CSS | No ACM equivalent — use HTML/CSS instead |
| `EO.Pdf.Contents` | `IronPdf.Editing` | Low-level content / editing |
| `EO.Pdf.Drawing` | HTML/CSS or `IronPdf.Editing` | Graphics operations |
| `EO.Pdf.PdfDocument` | `IronPdf.PdfDocument` | **Name collision — use fully qualified** |

### Core Class Mapping

| EO.Pdf Class | IronPDF Class | Description |
|---|---|---|
| `HtmlToPdf` (static) | `ChromePdfRenderer` (instance) | Main HTML rendering entry point |
| `EO.Pdf.PdfDocument` | `IronPdf.PdfDocument` | Document object (name collision) |
| `HtmlToPdfOptions` | `ChromePdfRenderOptions` (via `RenderingOptions`) | Rendering configuration |
| `PdfDocumentSecurity` | `PdfDocument.SecuritySettings` | Password/permissions |
| `AcmRender` / `AcmText` / `AcmBlock` | HTML/CSS | Replace ACM positioning with HTML |

### Document Loading

| Operation | EO.Pdf | IronPDF |
|---|---|---|
| Load from file | `new PdfDocument(path)` | `PdfDocument.FromFile(path)` |
| Load from stream | `new PdfDocument(stream)` | `PdfDocument.FromStream(stream)` |
| Render HTML string | `HtmlToPdf.ConvertHtml(html, path)` | `renderer.RenderHtmlAsPdf(html)` |
| Render URL | `HtmlToPdf.ConvertUrl(url, path)` | `renderer.RenderUrlAsPdf(url)` |

### Page Operations

| Operation | EO.Pdf | IronPDF |
|---|---|---|
| Page count | `doc.Pages.Count` | `pdf.PageCount` |
| Get specific page | `doc.Pages[i]` | `pdf.Pages[i]` |
| Remove page | `doc.Pages.RemoveAt(i)` | `pdf.RemovePages(index)` |
| Copy pages | manual via `Pages` collection | `pdf.CopyPages(start, end)` |

### Merge / Split

| Operation | EO.Pdf | IronPDF |
|---|---|---|
| Merge PDFs | `PdfDocument.Merge(docA, docB, ...)` (static) | `PdfDocument.Merge(docA, docB, ...)` (static) |
| Split by page range | manual via `Pages` collection | `pdf.CopyPages(start, end)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (EO.Pdf):**

```csharp
using EO.Pdf;
using System.Drawing;

class Program
{
    static void Main()
    {
        string html = @"
            <html>
            <body>
                <h1 style='color: #333;'>Monthly Report</h1>
                <p>Generated: <strong>2026-05</strong></p>
                <table border='1'>
                    <tr><th>Item</th><th>Value</th></tr>
                    <tr><td>Revenue</td><td>$120,000</td></tr>
                </table>
            </body>
            </html>";

        // Static options apply globally
        HtmlToPdf.Options.PageSize = PdfPageSizes.A4;
        // OutputArea is in inches: x, y, width, height inside the page
        HtmlToPdf.Options.OutputArea = new RectangleF(0.5f, 0.5f, 7.5f, 10.5f);

        // Direct write to file path
        HtmlToPdf.ConvertHtml(html, "report.pdf");
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

        string html = @"
            <html>
            <body>
                <h1 style='color: #333;'>Monthly Report</h1>
                <p>Generated: <strong>2026-05</strong></p>
                <table border='1'>
                    <tr><th>Item</th><th>Value</th></tr>
                    <tr><td>Revenue</td><td>$120,000</td></tr>
                </table>
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        // Margins in millimeters (0.5 inch = 12.7mm)
        renderer.RenderingOptions.MarginTop = 12.7;
        renderer.RenderingOptions.MarginBottom = 12.7;
        renderer.RenderingOptions.MarginLeft = 12.7;
        renderer.RenderingOptions.MarginRight = 12.7;
        // Docs: https://ironpdf.com/how-to/rendering-options/

        using var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report.pdf");
    }
}
```

---

### 2. Merge PDFs

**Before (EO.Pdf):**

```csharp
using EO.Pdf;

class MergeSample
{
    static void Main()
    {
        var docA = new PdfDocument("doc_a.pdf");
        var docB = new PdfDocument("doc_b.pdf");

        // EO.Pdf has no instance Append(); the canonical merge is the static
        // PdfDocument.Merge(...) which returns a new combined document.
        PdfDocument merged = PdfDocument.Merge(docA, docB);

        merged.Save("merged.pdf");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;
using System.Collections.Generic;

class MergeSample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var docA = PdfDocument.FromFile("doc_a.pdf");
        using var docB = PdfDocument.FromFile("doc_b.pdf");

        using var merged = PdfDocument.Merge(new List<PdfDocument> { docA, docB });
        merged.SaveAs("merged.pdf");
        // Docs: https://ironpdf.com/how-to/merge-or-split-pdfs/
    }
}
```

---

### 3. Watermark

**Before (EO.Pdf — via ACM positioning):**

```csharp
using EO.Pdf;
using EO.Pdf.Acm;
using System.Drawing;

class WatermarkSample
{
    static void Main()
    {
        var doc = new PdfDocument("input.pdf");

        foreach (PdfPage page in doc.Pages)
        {
            AcmText watermark = new AcmText("CONFIDENTIAL");
            watermark.Style.FontSize = 72;
            watermark.Style.ForegroundColor = Color.FromArgb(100, 200, 200, 200);
            watermark.Style.HorizontalAlign = AcmHorizontalAlign.Center;

            AcmRender render = new AcmRender(
                page, 0, page.Size.Height / 2 - 50,
                page.Size.Width, 100);
            render.Render(watermark);
        }

        doc.Save("watermarked.pdf");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class WatermarkSample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        // ApplyWatermark uses HTML — full CSS available
        pdf.ApplyWatermark(
            "<h1 style='color:rgba(200,200,200,0.5); font-size:72pt;'>CONFIDENTIAL</h1>",
            rotation: 45,
            opacity: 50);

        pdf.SaveAs("watermarked.pdf");
        // Docs: https://ironpdf.com/how-to/stamp-text-image/
    }
}
```

---

### 4. Password Protection

**Before (EO.Pdf):**

```csharp
using EO.Pdf;

class SecuritySample
{
    static void Main()
    {
        var doc = new PdfDocument("input.pdf");

        doc.Security.UserPassword = "user123";
        doc.Security.OwnerPassword = "owner456";
        doc.Security.AllowPrint = true;
        doc.Security.AllowCopy = false;
        doc.Security.AllowModify = false;

        doc.Save("secured.pdf");
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

        // Docs: https://ironpdf.com/how-to/pdf-permissions-passwords/
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

        pdf.SaveAs("secured.pdf");
    }
}
```

---

## Critical Migration Notes

### The `PdfDocument` Name Collision
Both EO.Pdf and IronPDF have a class named `PdfDocument`. If you're doing a phased migration with both packages installed, you'll get ambiguous reference errors. Use fully qualified names or aliases:

```csharp
// Alias approach during transition
using EoPdfDoc = EO.Pdf.PdfDocument;
using IronPdfDoc = IronPdf.PdfDocument;
```

Once EO.Pdf is fully removed, drop the aliases.

### Static Options vs Instance Options
EO.Pdf's `HtmlToPdf.Options` is a global static. Every place in your codebase that touches `HtmlToPdf.Options.*` becomes a per-request `renderer.RenderingOptions.*` after migration. If your previous code relied on "set once at startup and forget", you may need to push some options closer to the call site — or wrap a `ChromePdfRenderer` in a service that pre-configures the options once.

### Event-Based Page Rendering
EO.Pdf's `AfterRenderPage` event handler runs ACM positioning code per page (headers, footers, overlays). IronPDF doesn't use per-page events — it uses `HtmlHeaderFooter` for headers and footers (with `{page}` / `{total-pages}` placeholders) and `HtmlStamper` / `ApplyStamp` for overlays. If you're relying heavily on `AfterRenderPage`, plan extra time for this restructuring.

### Page Indexing
Both libraries use 0-based page indexing internally. Off-by-one errors are silent — they'll affect the wrong page without throwing an exception.

### Margin Units
EO.Pdf's `OutputArea` is specified in inches via `RectangleF`. IronPDF's margin properties are in millimeters. Convert: `inches × 25.4 = millimeters`.

### Exception Model
EO.Pdf surfaces some failures via return codes or events depending on version. IronPDF throws standard .NET exceptions with descriptive messages. Review your error-handling code as part of migration.

---

## Performance Considerations

### Cold Start
IronPDF initializes a Chromium instance on first render. Plan for a 1–3 second startup cost (varies by hardware). Warm up at application startup:

```csharp
// In Program.cs / startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();
// Optionally render a dummy doc to warm up Chromium
using var warmup = renderer.RenderHtmlAsPdf("<p>init</p>");
// Now your first real render won't pay the startup cost
```

### Concurrency
`ChromePdfRenderer` can be reused across requests. For high-concurrency workloads, consider a renderer pool or using IronPDF's [parallel render patterns](https://ironpdf.com/examples/parallel/).

```csharp
// Singleton pattern for ASP.NET
builder.Services.AddSingleton<ChromePdfRenderer>();
```

### Disposal Discipline
Every `PdfDocument` should be in a `using` block. In ASP.NET, returning a `File()` result after disposing is safe — `BinaryData` is a byte array that survives disposal.

```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
return File(pdf.BinaryData, "application/pdf", "report.pdf");
// pdf disposed after this line — BinaryData already copied
```

---

## Migration Checklist

### Pre-Migration
- [ ] Run `rg "EO\.Pdf" --type cs -l` and review all affected files
- [ ] Flag `PdfDocument` name collisions — plan aliasing strategy
- [ ] Identify `HtmlToPdf.Options.*` static usages — these become per-instance options
- [ ] Identify `AfterRenderPage` event handlers — these need redesign as `HtmlHeaderFooter` or stamps
- [ ] Identify ACM (`AcmText`, `AcmBlock`, `AcmRender`) usages — these need a rewrite as HTML/CSS
- [ ] Confirm IronPDF license is available and configured
- [ ] Test IronPDF Linux deps on your deployment target before committing
- [ ] Review [rendering options](https://ironpdf.com/how-to/rendering-options/) for paper size / margin equivalents

### Code Migration
- [ ] Add using alias for `EO.Pdf.PdfDocument` during transition
- [ ] Replace `HtmlToPdf.ConvertHtml(html, path)` with `renderer.RenderHtmlAsPdf(html)` + `pdf.SaveAs(path)`
- [ ] Replace `HtmlToPdf.ConvertUrl(url, path)` with `renderer.RenderUrlAsPdf(url)` + `pdf.SaveAs(path)`
- [ ] Replace static `HtmlToPdf.Options.*` with instance `renderer.RenderingOptions.*`
- [ ] Convert margin values from inches to millimeters (`× 25.4`)
- [ ] Replace `new PdfDocument(path)` with `PdfDocument.FromFile(path)` / `FromStream(stream)` / `FromBinaryData(bytes)`
- [ ] Replace `doc.Security.*` with `pdf.SecuritySettings.*`
- [ ] Replace ACM watermark code with `ApplyWatermark(...)` or `HtmlStamper`
- [ ] Replace `PdfDocument.Merge(...)` (static) — same signature on both sides
- [ ] Replace `AfterRenderPage` handlers with `HtmlHeaderFooter` (with `{page}` / `{total-pages}` placeholders)
- [ ] Add `using` to all `PdfDocument` instances

### Testing
- [ ] Visual regression test every rendered template
- [ ] Test JS-rendered content (charts, dynamic tables)
- [ ] Test password protection with Adobe Reader or equivalent
- [ ] Test merge output page count
- [ ] Load test at target concurrency
- [ ] Verify memory stability over 30 minutes of continuous rendering

### Post-Migration
- [ ] Remove `EO.Pdf` NuGet package
- [ ] Remove `EO.Pdf.PdfDocument` aliases
- [ ] Update Dockerfile / deployment scripts for IronPDF Linux deps
- [ ] Document the migration for your team's runbook

---

## The Bottom Line

The EO.Pdf migration is mostly mechanical for HTML-to-PDF workflows, with three sharp corners: the `PdfDocument` name collision, the global static `HtmlToPdf.Options`, and the `AfterRenderPage` event-based per-page model. The name collision is a compile error you'll catch immediately; the static-options rewrite touches every call site that mutates options; and the event model takes design time.

For teams already running into the .NET Core Windows-only constraint or the static-options thread-safety problem, IronPDF's instance-based, cross-platform model removes the architectural cause rather than working around it.

**Technical question for comments:** What's the most complex `AfterRenderPage` handler you've had to migrate? And how did you restructure it when the new library expected HTML templates instead of callbacks?
