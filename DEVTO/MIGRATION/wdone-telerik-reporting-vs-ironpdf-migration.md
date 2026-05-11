# Replacing Telerik Reporting with IronPDF: what breaks, what doesn't

The design goes through three rounds of review and gets approved. Then the TRDX report runs, and the output is close but not right — the table header font is slightly heavier than the mock, the spacing between sections is off, and the custom border radius on the summary box isn't supported. You file it under "acceptable fidelity loss" and move on. A month later it's the same conversation with a different report. The gap between what CSS produces in a browser and what a report designer produces in Telerik Reporting is a recurring negotiation, not a one-time compromise.

If you're at the point where the fidelity loss is costing more in back-and-forth than a migration would cost in rewriting, this article covers the path from Telerik Reporting to IronPDF. You'll have working before/after code for the core operations by the end. The comparison tables and checklist are useful even if you choose a different path.

---

## Why Migrate (Without Drama)

Teams evaluating Telerik Reporting replacements commonly encounter:

1. **HTML/CSS fidelity gap** — Telerik's report engine renders from its own layout model; modern CSS features (flex, grid, custom properties, advanced typography) aren't available.
2. **Designer dependency** — report templates live in `.trdx` files that require the Telerik Report Designer to modify; developers who work in code find this a friction point.
3. **Telerik subscription scope** — Reporting is bundled with Telerik UI; organizations that only need PDF generation pay for a larger product set.
4. **Version coupling** — Telerik Reporting version is coupled to other Telerik packages in the same solution, creating upgrade coordination overhead.
5. **Viewer deployment** — interactive Telerik Report Viewers require server-side report processing infrastructure, adding deployment complexity.
6. **Custom layout limits** — complex pixel-perfect layouts that work in HTML/CSS may not be achievable in the Telerik report designer without workarounds.
7. **Report server infrastructure** — server-based report delivery requires Telerik Report Server or REST API setup, similar in overhead to SSRS.
8. **Testing difficulty** — testing Telerik reports requires running the report processor; HTML templates can be previewed in any browser.
9. **JavaScript chart rendering** — charts in Telerik Reporting use their own chart engine; web-native chart libraries (Chart.js, ApexCharts) require HTML rendering.
10. **Version control workflow** — `.trdx` XML is diffable but the development workflow still requires the designer for most changes.

### Comparison Table

| Aspect | Telerik Reporting | IronPDF |
|---|---|---|
| Focus | Full reporting platform (design, view, export) | HTML-to-PDF + PDF manipulation |
| Pricing | Telerik subscription — verify at telerik.com | Commercial license — verify at ironsoftware.com |
| API Style | `ReportProcessor` + report source objects | `ChromePdfRenderer` + HTML input |
| Learning Curve | Medium-High; visual designer + API | Low for .NET devs; HTML/CSS is the input |
| HTML Rendering | Telerik's report engine — not browser-based | Embedded Chromium |
| Page Indexing | 1-based in report design | 0-based |
| Thread Safety | ReportProcessor is thread-safe | Verify IronPDF concurrent instance guidance |
| Namespace | `Telerik.Reporting.*` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Telerik Reporting | IronPDF Equivalent | Complexity |
|---|---|---|---|
| Export report to PDF | `ReportProcessor.RenderReport("PDF", ...)` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | High (template rewrite) |
| TRDX template to HTML | N/A — incompatible formats | HTML/CSS template (per report) | High |
| Data binding | Data source in report designer | C# interpolation / template engine | High |
| Headers/footers | Report Header/Footer sections | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Charts | Telerik chart items | Chart.js or similar via HTML | High |
| Page numbering | Built-in page fields | `HtmlHeaderFooter` tokens | Medium |
| Save to file | Write bytes from render result | `pdf.SaveAs(path)` | Low |
| Save to stream | Write to `MemoryStream` | `pdf.Stream` | Low |
| Merge PDFs | Multiple report exports + secondary lib | `PdfDocument.Merge()` | Medium |
| Password protection | Verify Telerik security API | `pdf.SecuritySettings` | Low |
| Watermark | Report designer element | `TextStamper` / `ImageStamper` | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Reports drive from HTML/CSS design (from a web designer) | Switch — eliminates the HTML → report designer translation step |
| Reports need interactive viewer (drill-down, parameters) | Telerik Reporting has this; IronPDF does not |
| Reports are simple tabular exports from data | Switch — HTML tables render cleanly |
| Reporting is one component among many Telerik UI uses | Evaluate partial migration carefully |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All Telerik Reporting References

```bash
# Find ReportProcessor and report source usage
rg -l "Telerik\.Reporting\|ReportProcessor\|InstanceReportSource\|UriReportSource" --type cs
rg "ReportProcessor\|RenderReport\|InstanceReportSource" --type cs -n

# Find TRDX/TRBP template files
find . -name "*.trdx" -o -name "*.trbp" | sort
find . -name "*.trdx" | wc -l  # scope of template migration

# Find Telerik Reporting NuGet references
grep -r "Telerik\.Reporting" *.csproj **/*.csproj 2>/dev/null

# Find Telerik Reporting service registrations (ASP.NET Core)
rg "AddTelerikReporting\|UseTelerikReporting\|ReportServiceConfiguration" --type cs -n
```

### Uninstall / Install

```bash
# Remove Telerik Reporting packages (verify exact names)
dotnet remove package Telerik.Reporting
dotnet remove package Telerik.Reporting.Services.WebApi   # if used
dotnet remove package Telerik.Reporting.OpenXmlRendering  # if used

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
using Telerik.Reporting;
using Telerik.Reporting.Processing;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic PDF Export

**Before (Telerik ReportProcessor):**
```csharp
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    static void Main()
    {
        var processor = new ReportProcessor();
        var deviceInfo = new Hashtable();

        // Load report from TRDX file
        var reportSource = new UriReportSource();
        reportSource.Uri = "Reports/SalesReport.trdx";
        reportSource.Parameters.Add(new Parameter("ReportTitle", "Q3 2024 Sales"));

        // Render to PDF
        var result = processor.RenderReport("PDF", reportSource, deviceInfo);

        File.WriteAllBytes("sales-report.pdf", result.DocumentBytes);
        Console.WriteLine($"Saved sales-report.pdf ({result.DocumentBytes.Length} bytes)");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// HTML template replaces .trdx file — data bound via C# interpolation
var data = GetSalesData();
var html = BuildSalesReportHtml(data, title: "Q3 2024 Sales");

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("sales-report.pdf");

Console.WriteLine($"Saved sales-report.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## The TRDX → HTML Template Rewrite

The core migration work is converting each `.trdx` report definition to an HTML template. There is no automated converter — the `.trdx` format is Telerik-specific XML, and the output is Telerik's rendering engine.

The practical process per report:

1. Export a reference PDF from Telerik Reporting (visual target)
2. Identify data bindings (`=Fields.ColumnName`) in the TRDX and map to C# model properties
3. Build an HTML/CSS template that produces equivalent layout
4. Replace `ReportProcessor.RenderReport()` with `ChromePdfRenderer.RenderHtmlAsPdfAsync()`

For data-heavy tabular reports, this pattern works well:

```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Data that was bound via Telerik data source configuration
var salesLines = new[]
{
    new { Region = "North America", Q1 = 3_200_000m, Q2 = 4_100_000m, Q3 = 5_100_000m },
    new { Region = "Europe",        Q1 = 2_400_000m, Q2 = 2_900_000m, Q3 = 3_800_000m },
    new { Region = "APAC",          Q1 = 1_800_000m, Q2 = 2_200_000m, Q3 = 3_400_000m },
};

// Build HTML — replaces TRDX tablix data region
var rows = new StringBuilder();
foreach (var line in salesLines)
{
    rows.Append($@"<tr>
        <td>{line.Region}</td>
        <td>${line.Q1:N0}</td>
        <td>${line.Q2:N0}</td>
        <td>${line.Q3:N0}</td>
        <td>${line.Q1 + line.Q2 + line.Q3:N0}</td>
    </tr>");
}

var html = $@"
    <html>
    <head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; color: #222; }}
        h1 {{ font-size: 20px; margin-bottom: 4px; }}
        .subtitle {{ color: #888; font-size: 12px; margin-bottom: 24px; }}
        table {{ width: 100%; border-collapse: collapse; font-size: 12px; }}
        th {{ background: #1a3a5c; color: white; padding: 8px 10px; text-align: left; }}
        td {{ padding: 7px 10px; border-bottom: 1px solid #e0e0e0; }}
        tr:nth-child(even) td {{ background: #f8f8f8; }}
    </style>
    </head>
    <body>
        <h1>Regional Sales Report</h1>
        <div class='subtitle'>Fiscal Year 2024 · Q1–Q3</div>
        <table>
            <tr><th>Region</th><th>Q1</th><th>Q2</th><th>Q3</th><th>YTD Total</th></tr>
            {rows}
        </table>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
// https://ironpdf.com/how-to/headers-and-footers/
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:right;font-size:9px;color:#999;padding:0 40px'>Page {page} of {total-pages}</div>",
};

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("regional-sales.pdf");
Console.WriteLine($"Saved regional-sales.pdf ({pdf.PageCount} page(s))");
```

For organizations already using Razor in their web layer, a Razor-based renderer keeps the template authoring model consistent:

```csharp
// If using ASP.NET Core — Razor renders to HTML string, IronPDF renders that to PDF
// var html = await razorRenderer.RenderViewToStringAsync("Reports/SalesReport", model);
// var pdf = await renderer.RenderHtmlAsPdfAsync(html);
// This gives you Razor's data binding model without the Telerik designer dependency
```

---

## API Mapping Tables

### Namespace Mapping

| Telerik Reporting | IronPDF | Notes |
|---|---|---|
| `Telerik.Reporting` | `IronPdf` | Core namespace |
| `Telerik.Reporting.Processing` | `IronPdf` (renderer included) | No separate processor class |
| `Telerik.Reporting.Services.*` | N/A — removed | REST service layer not needed |

### Core Class Mapping

| Telerik Reporting Class | IronPDF Class | Description |
|---|---|---|
| `ReportProcessor` | `ChromePdfRenderer` | Renders HTML (not TRDX) to PDF |
| `UriReportSource` | HTML string / file path / URL | Template source format changes |
| `InstanceReportSource` | N/A — template is HTML | No report instance object |
| `RenderingResult` | `PdfDocument` | Output document object |

### Document Loading Methods

| Operation | Telerik Reporting | IronPDF |
|---|---|---|
| Load TRDX template | `new UriReportSource { Uri = "file.trdx" }` | HTML string / file / URL |
| Set parameters | `reportSource.Parameters.Add(new Parameter(...))` | Template variable injection |
| Render to bytes | `processor.RenderReport("PDF", source, info)` | `renderer.RenderHtmlAsPdfAsync(html)` |
| Save to file | `File.WriteAllBytes(path, result.DocumentBytes)` | `pdf.SaveAs(path)` |

### Page Operations

| Operation | Telerik Reporting | IronPDF |
|---|---|---|
| Page count | From render result | `pdf.PageCount` |
| Remove page | Not applicable | `pdf.RemovePage(index)` — verify |
| Extract text | N/A from standard API | `pdf.ExtractAllText()` |
| Rotate | Report designer | Verify in IronPDF docs |

### Merge / Split Operations

| Operation | Telerik Reporting | IronPDF |
|---|---|---|
| Merge | Not native — secondary lib required | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not native | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. Report to PDF

**Before (Telerik ReportProcessor):**
```csharp
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using System;
using System.Collections;
using System.IO;

class ReportExportBefore
{
    static void Main()
    {
        var processor = new ReportProcessor();
        var deviceInfo = new Hashtable();

        // TRDX report source — binary template file
        var reportSource = new UriReportSource
        {
            Uri = Path.Combine(AppContext.BaseDirectory, "Reports", "Invoice.trdx")
        };

        // Report parameters defined in the designer
        reportSource.Parameters.Add(new Parameter("CustomerId", "ACME-042"));
        reportSource.Parameters.Add(new Parameter("InvoiceNumber", "INV-2024-0099"));
        reportSource.Parameters.Add(new Parameter("TotalAmount", 4200.00m));

        // Render to PDF format
        var result = processor.RenderReport("PDF", reportSource, deviceInfo);

        if (result.HasErrors)
        {
            foreach (var error in result.Errors)
                Console.Error.WriteLine($"Report error: {error}");
            return;
        }

        File.WriteAllBytes("invoice.pdf", result.DocumentBytes);
        Console.WriteLine($"Saved invoice.pdf ({result.DocumentBytes.Length:N0} bytes)");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Data that was passed as report parameters — now template variables
var customerId = "ACME-042";
var invoiceNumber = "INV-2024-0099";
var totalAmount = 4200.00m;

// HTML template replaces Invoice.trdx — full CSS fidelity
var html = BuildInvoiceHtml(customerId, invoiceNumber, totalAmount);

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");
Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (Telerik Reporting — multiple renders + secondary library):**
```csharp
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

class MergeBefore
{
    static void Main()
    {
        var processor = new ReportProcessor();
        var deviceInfo = new Hashtable();

        // Render each report separately — no merge in ReportProcessor
        var reportPaths = new[] { "Reports/Section1.trdx", "Reports/Section2.trdx" };
        var pdfFiles = new List<string>();

        for (int i = 0; i < reportPaths.Length; i++)
        {
            var src = new UriReportSource { Uri = reportPaths[i] };
            var result = processor.RenderReport("PDF", src, deviceInfo);
            var outPath = $"section{i + 1}.pdf";
            File.WriteAllBytes(outPath, result.DocumentBytes);
            pdfFiles.Add(outPath);
        }

        // Merge via secondary library — ReportProcessor has no merge
        Console.WriteLine("Merge requires secondary library alongside Telerik Reporting");
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
    renderer.RenderHtmlAsPdfAsync(BuildSection1Html(GetSection1Data())),
    renderer.RenderHtmlAsPdfAsync(BuildSection2Html(GetSection2Data()))
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("combined-report.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (Telerik Reporting — designer element or secondary library):**
```csharp
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using System;
using System.Collections;
using System.IO;

class WatermarkBefore
{
    static void Main()
    {
        // Telerik Reporting: watermarks placed in Page Header/Footer sections
        // or as a background TextBox in the report designer.
        // Programmatic post-render watermark requires secondary library.

        var processor = new ReportProcessor();
        var deviceInfo = new Hashtable();
        var src = new UriReportSource { Uri = "Reports/Report.trdx" };
        var result = processor.RenderReport("PDF", src, deviceInfo);

        // Post-render watermark (secondary library required):
        // var watermarked = SomePdfLib.AddTextWatermark(result.DocumentBytes, "DRAFT");
        // File.WriteAllBytes("watermarked.pdf", watermarked);

        Console.WriteLine("Programmatic watermark requires secondary library with Telerik Reporting");
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
var pdf = await renderer.RenderHtmlAsPdfAsync(BuildReportHtml(GetData()));

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
pdf.SaveAs("watermarked-report.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (Telerik Reporting — verify security API):**
```csharp
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using System;
using System.Collections;
using System.IO;

class PasswordBefore
{
    static void Main()
    {
        // VERIFY: Telerik Reporting PDF security options
        // The standard RenderReport() deviceInfo may support some PDF settings
        // but password encryption is typically not exposed — verify at docs.telerik.com

        var processor = new ReportProcessor();
        var deviceInfo = new Hashtable();
        var src = new UriReportSource { Uri = "Reports/Report.trdx" };
        var result = processor.RenderReport("PDF", src, deviceInfo);

        // If password not available via deviceInfo — secondary library required:
        // var secured = SomePdfLib.SetPassword(result.DocumentBytes, "open123", "admin456");
        // File.WriteAllBytes("secured.pdf", secured);

        Console.WriteLine("Verify Telerik Reporting PDF password API — may need secondary library");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(BuildReportHtml(GetData()));

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### The Fidelity Gap Closes — and Opens Differently

The HTML/CSS fidelity gap that triggered this migration closes: Chromium renders current CSS standards exactly as Chrome browser does. The design that was approved in the browser is the design that comes out of the PDF.

The new gap to watch: print-specific CSS behavior. Some CSS properties behave differently between `@media screen` and `@media print`. Test your templates with Chrome's DevTools print preview before rendering with IronPDF:

```csharp
// IronPDF renders @media print CSS — preview in Chrome DevTools:
// DevTools → More tools → Rendering → Emulate CSS media type → print
// See: https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

// Add @media print styles to your HTML for print-specific layout
var html = @"
    <html>
    <head>
    <style>
        @media print {
            .no-print { display: none; }
            .page-break { page-break-before: always; }
        }
        body { font-family: Arial, sans-serif; padding: 40px; }
    </style>
    </head>
    <body>...</body>
    </html>";

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### TRDX Template Count Determines Scope

Before committing to a timeline:

```bash
find . -name "*.trdx" -o -name "*.trbp" | wc -l
```

Each report template requires conversion to HTML. A simple tabular report: 30–60 minutes. A complex report with sub-reports, charts, and conditional formatting: several hours.

### Page Number Fields

Telerik Reporting has built-in page number fields. The HTML equivalent uses IronPDF footer tokens:

```csharp
// Telerik: PageCount / PageNumber built-in fields in designer
// IronPDF equivalent:
// https://ironpdf.com/how-to/headers-and-footers/
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"<div style='font-size:9px; text-align:right; padding:0 40px'>
        Page {page} of {total-pages}
    </div>",
    // Verify exact token names in current IronPDF docs
};
```

### Interactive Viewer Replacement

Telerik Reporting has interactive HTML viewers with drill-down and parameter inputs. IronPDF generates static PDFs only — there is no equivalent interactive viewer. If interactive report viewing is a requirement, evaluate whether a browser-based HTML viewer could replace it, or whether Telerik Reporting should remain for those specific use cases.

### `result.HasErrors` Pattern

Telerik's `RenderingResult.HasErrors` becomes exception-based in IronPDF:

```csharp
// Before — check HasErrors:
// var result = processor.RenderReport("PDF", source, deviceInfo);
// if (result.HasErrors) { ... }

// After — exception-based:
try
{
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    pdf.SaveAs(outputPath);
}
catch (IronPdf.Exceptions.IronPdfException ex)
{
    Console.Error.WriteLine($"Render failed: {ex.Message}");
}
```

---

## Performance Considerations

### Parallel Report Generation

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/examples/parallel/
var reportJobs = reportDataList.Select(data => (
    Html: BuildReportHtml(data),
    Path: $"reports/{data.Id}.pdf"
)).ToArray();

await Task.WhenAll(reportJobs.Select(async job =>
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(job.Html);
    pdf.SaveAs(job.Path);
}));

Console.WriteLine($"Generated {reportJobs.Length} reports in parallel");
// See: https://ironpdf.com/how-to/async/
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

// Telerik: result.DocumentBytes is a byte array — no disposal needed
// IronPDF: 'using' on PdfDocument handles disposal

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// If you need bytes:
var bytes = pdf.BinaryData;
// pdf disposed at end of 'using' block

// If you need a stream:
using var ms = new MemoryStream();
pdf.Stream.CopyTo(ms);
```

### Renderer Warm-Up

```csharp
using IronPdf;

// Amortize Chromium initialization at application startup
// The ReportProcessor initializes on first use too — this is equivalent
var renderer = new ChromePdfRenderer();
using var _ = await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");
// Ready for production traffic
```

---

## Migration Checklist

### Pre-Migration
- [ ] Count `.trdx` and `.trbp` files (`find . -name "*.trdx" | wc -l`)
- [ ] Categorize by complexity: tabular, chart-heavy, sub-reports, interactive
- [ ] Identify reports used with interactive viewer (may not migrate cleanly)
- [ ] Identify secondary libraries used for merge/security post-rendering
- [ ] Document current page sizes, orientations, and font usage
- [ ] Choose template engine for data binding (interpolation, Razor, Scriban)
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `Telerik.Reporting` NuGet packages
- [ ] Add license key at application startup
- [ ] Convert each `.trdx` template to HTML template
- [ ] Replace `UriReportSource` / `InstanceReportSource` with HTML template input
- [ ] Replace `reportSource.Parameters.Add()` with template variable injection
- [ ] Replace `processor.RenderReport("PDF", ...)` with `renderer.RenderHtmlAsPdfAsync()`
- [ ] Replace `result.DocumentBytes` with `pdf.BinaryData` or `pdf.SaveAs()`
- [ ] Replace `result.HasErrors` checks with try/catch
- [ ] Replace page number fields with `HtmlHeaderFooter` tokens
- [ ] Replace secondary merge/security libraries with IronPDF equivalents

### Testing
- [ ] Compare PDF output visually against Telerik reference exports
- [ ] Test `@media print` CSS styles render as expected
- [ ] Verify page numbers in headers/footers
- [ ] Test chart rendering if using JS chart libraries
- [ ] Test merge and password protection
- [ ] Benchmark render time vs Telerik baseline
- [ ] Test concurrent rendering under load

### Post-Migration
- [ ] Remove all `Telerik.Reporting.*` NuGet packages
- [ ] Archive `.trdx` / `.trbp` files for reference
- [ ] Remove Telerik REST service configuration from ASP.NET Core if present
- [ ] Remove secondary PDF libraries now replaced by IronPDF

---

## Where to Go From Here

The HTML/CSS fidelity gap — the most common migration trigger for Telerik Reporting — is resolved structurally: the input is the same HTML and CSS that browsers render, so the design approved in the browser is the design that goes into the PDF. The negotiation ends.

The remaining scope question is always TRDX file count and chart complexity. Charts that used Telerik's chart items need to be replaced with JS-based alternatives (Chart.js, ApexCharts) rendered via Chromium — doable, but each chart type requires verification.

**Discussion question:** Which Telerik Reporting feature was hardest to replicate in HTML — was it charts, sub-reports, conditional formatting, or interactive viewer functionality that the article didn't cover?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
