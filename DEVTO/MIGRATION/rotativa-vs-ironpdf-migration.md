---
title: "Replacing Rotativa with IronPDF: what breaks, what doesn't"
published: false
tags: dotnet, csharp, pdf, migration
---

The rendered PDF looks like a printout of the 2000s web. The fonts are different from the design, the table columns are slightly off, and that gradient you spent an hour on is just gone. Rotativa wraps wkhtmltopdf, which uses an older WebKit rendering engine. Your designs are built on modern CSS — flexbox, custom properties, Grid — and wkhtmltopdf doesn't fully support any of them. You can work around it, but every workaround is technical debt that accumulates until a redesign forces the issue.

There is also a security dimension worth naming up front. The wkhtmltopdf project was archived on January 2, 2023, and [CVE-2022-35583](https://nvd.nist.gov/vuln/detail/CVE-2022-35583) (a server-side request forgery) has no fixed version. Any Rotativa.AspNetCore deployment that renders untrusted HTML inherits that exposure.

This article covers migrating from Rotativa to IronPDF. You'll have working before/after code for the four core operations and a checklist for every step from codebase audit to production rollout.

---

## Why Migrate (Without Drama)

Teams evaluating Rotativa replacements commonly cite these conditions:

1. **wkhtmltopdf CSS fidelity** — wkhtmltopdf uses an older WebKit and doesn't support modern CSS (flex, grid, CSS custom properties, many `@media print` features).
2. **Unpatched SSRF (CVE-2022-35583)** — the underlying wkhtmltopdf binary was archived in January 2023, so this CVE will not be patched upstream.
3. **Binary deployment complexity** — wkhtmltopdf must be present and executable on the server; path configuration, permissions, and binary availability vary per environment.
4. **Docker image complexity** — the wkhtmltopdf binary and its dependencies (`libXrender`, `libXext`, `libx11`, `libfontconfig`, etc.) add to Docker image size and configuration.
5. **Action result coupling** — Rotativa is tightly coupled to ASP.NET MVC action results; using it outside a controller (in a background service, queue consumer, etc.) requires workarounds.
6. **Maintenance asymmetry** — the `Rotativa.AspNetCore` wrapper still ships updates (1.4.0 was published 2024-11-06), but the wkhtmltopdf binary it wraps has been archived since 2023.
7. **Missing PDF manipulation** — Rotativa generates PDFs via wkhtmltopdf but doesn't support merge, split, watermark, or security.
8. **No programmatic rendering** — Rotativa is designed for controller actions; rendering from a service class requires creating a fake HTTP context.
9. **wkhtmltopdf permission issues** — running as a non-root user in containers often requires specific permissions for the wkhtmltopdf binary.
10. **JavaScript rendering limitations** — wkhtmltopdf's JavaScript support is partial; dynamic content from modern frameworks may not render.

### Comparison Table

| Aspect | Rotativa (AspNetCore) | IronPDF |
|---|---|---|
| Focus | ASP.NET MVC PDF via Razor views | HTML-to-PDF + PDF manipulation |
| Pricing | Open source (MIT-licensed wrapper over an archived wkhtmltopdf binary) | Commercial license — [pricing at ironpdf.com](https://ironpdf.com/licensing/) |
| API Style | MVC action result (`ViewAsPdf`) | Library API — no controller required |
| Learning Curve | Low for MVC devs; integrated into action results | Low for .NET devs; usable anywhere in the stack |
| HTML Rendering | wkhtmltopdf (WebKit-based) | Embedded Chromium |
| Page Indexing | N/A — generation only | 0-based |
| Concurrency Model | One wkhtmltopdf process per render | Async-first; reuse a single renderer instance and parallelize via `Task.WhenAll` |
| Namespace | `Rotativa.AspNetCore` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Rotativa | IronPDF Equivalent | Complexity |
|---|---|---|---|
| View to PDF (from controller) | `return new ViewAsPdf("ViewName", model)` | Render view to HTML string + `RenderHtmlAsPdfAsync()` | Medium |
| Partial view to PDF | `PartialViewAsPdf` | Render partial to HTML string + renderer | Medium |
| URL to PDF | `UrlAsPdf(url)` | `RenderUrlAsPdfAsync(url)` | Low |
| HTML string to PDF | Not a primary feature | `RenderHtmlAsPdfAsync(html)` | Low |
| Custom page size | `CustomSwitches` flag or property | `RenderingOptions.PaperSize` | Low |
| Custom margins | `CustomSwitches` | `RenderingOptions.Margin*` | Low |
| Headers/footers | `CustomSwitches` (wkhtmltopdf flags) | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Password protection | Via `CustomSwitches --user-password` / `--owner-password` | `pdf.SecuritySettings` | Medium |
| Merge PDFs | Not supported | `PdfDocument.Merge()` | Medium |
| Watermark | Not supported | `TextStamper` / `ImageStamper` | Medium |
| Non-controller usage | Requires fake context or workaround | Direct API — works anywhere | Low |
| wkhtmltopdf binary management | Required | N/A — Chromium bundled in NuGet | Low (remove) |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Modern CSS (flex, grid, CSS variables) in templates | Switch — Chromium renders current CSS; wkhtmltopdf doesn't |
| PDF generation needed outside controller actions | Switch — IronPDF works anywhere; Rotativa is controller-bound |
| Docker/Linux deployment with binary management issues | Switch — eliminates wkhtmltopdf binary complexity |
| Rendering untrusted HTML (user-submitted content) | Switch — CVE-2022-35583 has no upstream patch |
| Simple HTML tables from Razor views with no CSS concerns | Rotativa may be acceptable; evaluate based on CSS needs |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All Rotativa References

```bash
# Find Rotativa action results
rg -l "ViewAsPdf\|PartialViewAsPdf\|UrlAsPdf\|Rotativa" --type cs
rg "ViewAsPdf\|UrlAsPdf\|Rotativa\.AspNetCore" --type cs -n

# Find wkhtmltopdf binary references in project
find . -name "wkhtmltopdf*" -o -name "wkhtmltoimage*" 2>/dev/null

# Find wkhtmltopdf in Dockerfiles and CI
grep -r "wkhtmltopdf\|libXrender\|libXext" Dockerfile* .github/**/*.yml 2>/dev/null

# Find Rotativa config in startup
rg "RotativaConfiguration\|Rotativa" --type cs -n
```

### Uninstall / Install

```bash
# Remove Rotativa.AspNetCore
dotnet remove package Rotativa.AspNetCore

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

Remove wkhtmltopdf binary from project and Docker:

```bash
# Remove wkhtmltopdf from wwwroot or Tools folder in project
rm -f wwwroot/Rotativa/wkhtmltopdf
rm -rf ./Rotativa/  # if folder exists

# Clean up Docker wkhtmltopdf installation:
# RUN apt-get install -y wkhtmltopdf  <- remove this
# COPY Rotativa/wkhtmltopdf ...       <- remove this
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// Add to Program.cs or Startup.cs
// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

Also remove the Rotativa startup configuration:

```csharp
// Remove from Program.cs / Startup.cs:
// Rotativa.AspNetCore.RotativaConfiguration.Setup(env.WebRootPath, "Rotativa");
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic Conversion

**Before (Rotativa action result):**
```csharp
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;

[HttpGet("invoice/{id}")]
public IActionResult GetInvoicePdf(int id)
{
    var model = _invoiceService.GetById(id);
    return new ViewAsPdf("Invoice", model)
    {
        FileName = $"invoice-{id}.pdf",
        PageSize = Size.A4,
    };
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
using Microsoft.AspNetCore.Mvc;

[HttpGet("invoice/{id}")]
public async Task<IActionResult> GetInvoicePdf(int id)
{
    var model = _invoiceService.GetById(id);

    // Render Razor view to HTML string first
    var html = await _viewRenderer.RenderToStringAsync("Invoice", model);

    // Then render to PDF
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return File(pdf.BinaryData, "application/pdf", $"invoice-{id}.pdf");
    // https://ironpdf.com/how-to/html-string-to-pdf/
}
```

> The `_viewRenderer.RenderToStringAsync()` call requires a Razor view renderer service. See the section below for a complete implementation.

---

## Razor View to HTML String

Rotativa's key value was rendering Razor views directly to PDF without a separate HTML step. When migrating, you need to render Razor views to HTML strings first. This is a well-known ASP.NET Core pattern:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.IO;
using System.Threading.Tasks;

public class RazorViewRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public RazorViewRenderer(
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> RenderToStringAsync<TModel>(string viewName, TModel model)
    {
        var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        using var sw = new StringWriter();

        var viewResult = _viewEngine.FindView(actionContext, viewName, false);
        if (!viewResult.Success)
            throw new InvalidOperationException($"View '{viewName}' not found.");

        var viewDictionary = new ViewDataDictionary<TModel>(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary())
        {
            Model = model
        };

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}
```

Register in DI:

```csharp
// Program.cs
builder.Services.AddScoped<RazorViewRenderer>();
```

---

## API Mapping Tables

### Namespace Mapping

| Rotativa | IronPDF | Notes |
|---|---|---|
| `Rotativa.AspNetCore` | `IronPdf` | Core namespace |
| `Rotativa.AspNetCore.Options` | `IronPdf.Rendering` | Rendering options |
| N/A | `IronPdf.Editing` | Watermark / stamp |

### Core Class Mapping

| Rotativa Class | IronPDF Class | Description |
|---|---|---|
| `ViewAsPdf` | `ChromePdfRenderer` + `RazorViewRenderer` | Render Razor view -> HTML string -> PDF |
| `UrlAsPdf` | `ChromePdfRenderer.RenderUrlAsPdfAsync()` | Render URL to PDF |
| `ContentAsPdf` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Render HTML string to PDF |
| N/A | `PdfDocument` | PDF object with manipulation methods |

### Document Loading Methods

| Operation | Rotativa | IronPDF |
|---|---|---|
| Razor view | `new ViewAsPdf("View", model)` | `RenderToStringAsync()` + `RenderHtmlAsPdfAsync()` |
| URL | `new UrlAsPdf("https://...")` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML string | `new ContentAsPdf(html)` | `renderer.RenderHtmlAsPdfAsync(html)` |
| Existing PDF | N/A | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | Rotativa | IronPDF |
|---|---|---|
| Page count | N/A | `pdf.PageCount` |
| Remove page | N/A | `pdf.RemovePages(index)` |
| Extract text | N/A | `pdf.ExtractAllText()` |
| Rotate | N/A | `pdf.RotateAllPages(PdfPageRotation.Clockwise90)` |

### Merge / Split Operations

| Operation | Rotativa | IronPDF |
|---|---|---|
| Merge | Not supported | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not supported | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. View to PDF (from controller)

**Before (Rotativa):**
```csharp
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using System;

[ApiController]
[Route("reports")]
public class ReportController : ControllerBase
{
    [HttpGet("quarterly/{year}/{quarter}")]
    public IActionResult GetQuarterlyReport(int year, int quarter)
    {
        var reportData = new ReportViewModel
        {
            Year = year,
            Quarter = quarter,
            Revenue = 12400000,
            Growth = 8.2,
        };

        return new ViewAsPdf("QuarterlyReport", reportData)
        {
            FileName = $"report-q{quarter}-{year}.pdf",
            PageSize = Size.A4,
            PageMargins = { Top = 20, Bottom = 20, Left = 25, Right = 25 },
        };
    }
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("reports")]
public class ReportController : ControllerBase
{
    private readonly RazorViewRenderer _viewRenderer;

    public ReportController(RazorViewRenderer viewRenderer)
        => _viewRenderer = viewRenderer;

    [HttpGet("quarterly/{year}/{quarter}")]
    public async Task<IActionResult> GetQuarterlyReport(int year, int quarter)
    {
        var reportData = new ReportViewModel
        {
            Year = year,
            Quarter = quarter,
            Revenue = 12400000,
            Growth = 8.2,
        };

        // Step 1: Render Razor view to HTML
        var html = await _viewRenderer.RenderToStringAsync("QuarterlyReport", reportData);

        // Step 2: Render HTML to PDF
        // https://ironpdf.com/how-to/html-string-to-pdf/
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.MarginLeft = 25;
        renderer.RenderingOptions.MarginRight = 25;

        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return File(pdf.BinaryData, "application/pdf", $"report-q{quarter}-{year}.pdf");
    }
}
```

---

### 2. Merge PDFs

**Before (Rotativa — not supported):**
```csharp
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

class MergeBefore
{
    // Rotativa has no merge feature.
    // Teams typically generate each section's PDF separately
    // (saving to temp files or byte arrays) and use a secondary library to merge.

    static async Task MergeReports()
    {
        // PSEUDO-CODE — Rotativa doesn't have a programmatic API outside controller context
        // var page1 = new ViewAsPdf(...).BuildFile(...); // not directly supported
        // var page2 = new ViewAsPdf(...).BuildFile(...);
        // var merged = SomePdfLib.Merge(page1, page2);
        // File.WriteAllBytes("merged.pdf", merged);

        Console.WriteLine("Merge requires a secondary library — Rotativa doesn't provide it");
        await Task.CompletedTask;
    }
}
```

**After:**
```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

// Generate sections concurrently
var results = await Task.WhenAll(
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Q1: Overview</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Q2: Detail</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("annual-report.pdf");

Console.WriteLine($"Merged annual report: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (Rotativa — CSS injection only):**
```csharp
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using System;

class WatermarkBefore
{
    // Rotativa has no watermark API — the watermark must be expressed as CSS in the Razor view

    // In the Razor view (.cshtml):
    // <style>
    //   body::after {
    //     content: 'DRAFT';
    //     position: fixed; top:50%; left:50%;
    //     transform: translate(-50%,-50%) rotate(-45deg);
    //     font-size: 100px; opacity: 0.1; color: #999;
    //   }
    // </style>

    // The ViewAsPdf result then includes the watermark via CSS.
    // Rendering depends on wkhtmltopdf's CSS support, which is older WebKit.
    static void Main() =>
        Console.WriteLine("Rotativa watermarks are CSS-only via the Razor view");
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Editing;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Draft Report</h1><p>Content...</p></body></html>"
);

// Post-render programmatic watermark — applied uniformly across all pages
// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronSoftware.Drawing.Color.Gray,
    Opacity = 15, // 0-100 (percent)
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("draft-report.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (Rotativa — `CustomSwitches` for wkhtmltopdf flags):**
```csharp
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using System;

class PasswordBefore
{
    static IActionResult GetProtectedReport()
    {
        // Rotativa exposes wkhtmltopdf flags via CustomSwitches.
        // wkhtmltopdf accepts --user-password / --owner-password / --encrypt-level
        // to produce an encrypted PDF. Flag spelling varies by wkhtmltopdf
        // build, so consult wkhtmltopdf --extended-help for your binary.

        return new ViewAsPdf("ConfidentialReport", new { })
        {
            CustomSwitches = "--encrypt-level 128bit --owner-password admin456 --user-password open123",
        };
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

pdf.SaveAs("confidential-report.pdf");
Console.WriteLine("Password protected — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### The Razor View Rendering Step

Rotativa's main value was eliminating the need to manually render Razor views to HTML. After migration, you need to add a `RazorViewRenderer` service (see the complete implementation earlier in this article) to bridge Razor views to IronPDF.

This is the most significant code addition in the migration — register it in DI and inject it into controllers that need PDF rendering:

```bash
# Find all ViewAsPdf, PartialViewAsPdf, ContentAsPdf usages to scope the work
rg "ViewAsPdf\|PartialViewAsPdf\|ContentAsPdf" --type cs -n | wc -l
```

### wkhtmltopdf CSS Workarounds Can Be Removed

If you added CSS workarounds specifically to compensate for wkhtmltopdf's limited rendering, audit those after migration. Many workarounds become unnecessary with Chromium rendering:

```bash
# Common patterns that may be wkhtmltopdf workarounds
grep -r "overflow.*hidden.*pdf\|display.*table.*pdf\|float.*pdf" Views/ --include="*.cshtml" 2>/dev/null

# CSS comments that mention wkhtmltopdf
grep -r "wkhtmltopdf\|wkhtml\|webkit.*pdf" Views/ --include="*.cshtml" 2>/dev/null
```

### `CustomSwitches` Migration

Rotativa's `CustomSwitches` property accepted raw wkhtmltopdf CLI flags. These don't transfer directly — map them to IronPDF `RenderingOptions` properties:

| wkhtmltopdf CustomSwitch | IronPDF Equivalent |
|---|---|
| `--page-size A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` |
| `--margin-top 20` | `RenderingOptions.MarginTop = 20` |
| `--orientation Landscape` | `RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape` |
| `--header-html file.html` | `RenderingOptions.HtmlHeader` |
| `--footer-center [page]` | `RenderingOptions.HtmlFooter` with `{page}` token |
| `--javascript-delay 1000` | `RenderingOptions.WaitFor.JavaScript(1000)` |
| `--no-background` | `RenderingOptions.PrintHtmlBackgrounds = false` |

### RotativaConfiguration.Setup Removal

Rotativa required startup configuration:

```csharp
// Remove from Program.cs or Startup.cs:
// Rotativa.AspNetCore.RotativaConfiguration.Setup(env.WebRootPath, "Rotativa");
```

Replace with IronPDF license configuration:

```csharp
// Program.cs
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Page Indexing

IronPDF uses 0-based page indexing. Rotativa doesn't expose page manipulation, so this only applies to new IronPDF manipulation code.

---

## Performance Considerations

### Renderer Reuse in Controllers

Don't instantiate `ChromePdfRenderer` per request. Register it in DI as a singleton or scoped service:

```csharp
// Singleton — one renderer for the application lifetime
builder.Services.AddSingleton<ChromePdfRenderer>();

// Or scoped per request, if you mutate RenderingOptions per call
builder.Services.AddScoped<ChromePdfRenderer>();
```

### Background Service Pattern

Rotativa was tied to the HTTP request lifecycle. IronPDF can run in background services, making PDF generation non-blocking:

```csharp
using IronPdf;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class PdfGenerationWorker : BackgroundService
{
    private readonly IPdfJobQueue _queue;

    public PdfGenerationWorker(IPdfJobQueue queue) => _queue = queue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var renderer = new ChromePdfRenderer();

        while (!stoppingToken.IsCancellationRequested)
        {
            var job = await _queue.DequeueAsync(stoppingToken);
            if (job is null) continue;

            try
            {
                using var pdf = await renderer.RenderHtmlAsPdfAsync(job.Html);
                pdf.SaveAs(job.OutputPath);
            }
            catch (Exception ex)
            {
                // Log and continue — don't crash the worker
                Console.Error.WriteLine($"PDF job failed: {ex.Message}");
            }
        }
    }
}
```

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

// https://ironpdf.com/examples/parallel/
// If rendering multiple views in a single request:
var htmlSections = await Task.WhenAll(
    viewRenderer.RenderToStringAsync("Section1", model1),
    viewRenderer.RenderToStringAsync("Section2", model2)
);

var renderer = new ChromePdfRenderer();
var pdfs = await Task.WhenAll(
    htmlSections.Select(html => renderer.RenderHtmlAsPdfAsync(html))
);

var merged = PdfDocument.Merge(pdfs);
// See: https://ironpdf.com/how-to/async/
```

---

## Migration Checklist

### Pre-Migration
- [ ] Count all Rotativa action results (`rg "ViewAsPdf\|PartialViewAsPdf\|UrlAsPdf" --type cs | wc -l`)
- [ ] Audit CSS in Razor views for wkhtmltopdf-specific workarounds
- [ ] Find `CustomSwitches` usage and map to IronPDF equivalents
- [ ] Locate wkhtmltopdf binary in project (wwwroot/Rotativa/ or similar)
- [ ] Find wkhtmltopdf in Dockerfiles and CI configs
- [ ] Identify secondary PDF libraries used for merge, security, watermark
- [ ] Obtain IronPDF license key
- [ ] Confirm IronPDF .NET version support matches your target framework

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `Rotativa.AspNetCore` package reference
- [ ] Add `RazorViewRenderer` service class and register in DI
- [ ] Remove `RotativaConfiguration.Setup()` from startup
- [ ] Add IronPDF license configuration at startup
- [ ] Replace each `ViewAsPdf` with `RenderToStringAsync()` + `RenderHtmlAsPdfAsync()`
- [ ] Replace each `UrlAsPdf` with `RenderUrlAsPdfAsync()`
- [ ] Map `CustomSwitches` flags to `RenderingOptions` properties
- [ ] Replace CSS-injected watermarks with `TextStamper` / `ImageStamper`
- [ ] Migrate `CustomSwitches` password flags to `pdf.SecuritySettings`

### Testing
- [ ] Render each view and compare visual output
- [ ] Test that CSS layouts work correctly — especially flex/grid that wkhtmltopdf didn't support
- [ ] Verify page sizes and margins match expectations
- [ ] Test headers and footers render on all pages
- [ ] Test password protection
- [ ] Verify Docker build succeeds without wkhtmltopdf binary
- [ ] Test `UrlAsPdf` equivalent for authenticated URLs if applicable

### Post-Migration
- [ ] Remove `Rotativa.AspNetCore` NuGet package
- [ ] Delete wkhtmltopdf binary from project (wwwroot/Rotativa/)
- [ ] Remove wkhtmltopdf system library installs from Dockerfiles
- [ ] Remove secondary PDF manipulation libraries now in IronPDF
- [ ] Clean up CSS workarounds added specifically for wkhtmltopdf

---

## Before You Ship

The CSS fidelity gap is typically the clearest immediate win in this migration. CSS that was carefully limited to wkhtmltopdf's capabilities can be opened up — modern flex layouts, CSS Grid, custom properties all become usable. Templates that required extensive `.cshtml` workarounds become simpler.

The main new code in the migration is the `RazorViewRenderer` service, which replaces what Rotativa handled internally. It's reusable across all controllers once registered in DI, and the pattern is well-established in the ASP.NET Core community.

**Discussion question:** Which Rotativa feature was hardest to replicate — was it a `CustomSwitches` flag that didn't have a direct `RenderingOptions` equivalent, or something else?
