---
title: "Switching from SAP Crystal Reports to IronPDF: copy-paste and ship"
published: false
tags: dotnet, csharp, pdf, migration
---

Concurrency is where Crystal Reports shows its age. The ReportDocument class is not thread-safe — you can't share a single instance across requests, and if you try to reuse one from a pool without careful locking, you get corrupted output or runtime exceptions under load. The common fix is one ReportDocument per request, which works but doesn't scale cheaply: each instance loads the RPT file, resolves the data source, and holds resources until explicitly closed. Under sustained concurrent load, this becomes a bottleneck that no amount of tuning entirely removes.

If you're reaching that limit, this article covers the migration path to IronPDF. You'll have working before/after code for HTML-to-PDF, merge, watermark, and password protection by the end. The comparison tables and checklist are useful regardless of which library you choose.

---

## Why Migrate (Without Drama)

Teams evaluating Crystal Reports replacements commonly encounter these conditions:

1. **Thread safety** — `ReportDocument` is not thread-safe; concurrent rendering requires one instance per request or thread, which consumes substantial memory and COM resources.
2. **Runtime redistribution complexity** — the Crystal Reports runtime is a large, COM-registered redistributable that must be version-matched to the development runtime.
3. **COM interop overhead** — the internal COM bridge adds latency and makes debugging runtime errors more opaque.
4. **Visual Studio version coupling** — Crystal Reports runtime versions are often tied to specific Visual Studio releases, creating upgrade friction.
5. **RPT file dependency** — report templates are binary `.rpt` files, not text templates; they require the Crystal Reports designer to modify and don't diff well in version control.
6. **SAP support lifecycle** — SAP has confirmed end of mainstream maintenance for CR4VS at end of 2027, with CR 2020 ending December 2026 and CR 2025 ending December 31, 2027. The 32-bit CR .NET runtime was discontinued after SP 39 (December 2025); future service packs are 64-bit only.
7. **No HTML input** — Crystal Reports is a data-binding report engine, not an HTML renderer; generating PDFs from HTML content requires a different approach.
8. **Deployment environment constraints** — COM registration and the large runtime footprint complicate Docker/cloud deployments.
9. **Missing modern PDF features** — digital signatures, PDF/A compliance, and annotation workflows require additional configuration or aren't available.
10. **Scaling cost** — one-instance-per-request threading model has a memory ceiling that becomes a real constraint under concurrent load.

### Comparison Table

| Aspect | SAP Crystal Reports | IronPDF |
|---|---|---|
| Focus | Report design + data-bound multi-format export | HTML-to-PDF generation + PDF manipulation |
| Pricing | CR4VS free; designer/BI Platform commercial SAP SKUs | Commercial license; free trial available |
| API Style | `ReportDocument` load/bind/export pattern; COM-based | Native .NET objects; no COM dependency |
| Learning Curve | Medium; requires RPT designer for templates | Low for .NET devs; HTML/CSS templates |
| HTML Rendering | Not supported — data-binding model | Embedded Chromium |
| Page Indexing | 1-based | 0-based |
| Thread Safety | Not thread-safe — one instance per request | Per-task `ChromePdfRenderer` for concurrent rendering |
| Namespace | `CrystalDecisions.CrystalReports.Engine` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Crystal Reports | IronPDF Equivalent | Complexity |
|---|---|---|---|
| Export to PDF | `ReportDocument.ExportToDisk()` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Medium (template rewrite) |
| Save to file | `ExportOptions.DiskFileName` | `pdf.SaveAs(path)` | Low |
| Save to stream | `ExportToStream()` | `pdf.Stream` | Low |
| Data binding | Report parameters + `SetDataSource()` | HTML template string interpolation | High |
| Sub-reports | Report composition | HTML composition / partial includes | High |
| Custom page size | Report page setup in designer | `RenderingOptions.PaperSize` | Low |
| Headers/footers | Report section bands | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Merge PDFs | Not native | `PdfDocument.Merge()` | Medium |
| Watermark | Via designer section | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Not native — secondary library | `pdf.SecuritySettings` | Medium |
| Digital signatures | Not in the CR runtime | `pdf.Sign()` | Medium-High |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Reports are complex, data-bound with multiple sub-reports | Evaluate migration cost carefully — RPT-to-HTML is a non-trivial rewrite |
| Need HTML-to-PDF from web templates (not report-designer format) | Switch — IronPDF is designed for this; Crystal Reports is not |
| Thread safety under concurrent load is the primary concern | Switch — eliminates per-request instantiation constraint |
| Deploying to Docker/cloud where COM registration is impractical | Switch — IronPDF has no COM dependency |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)
- HTML templates for each Crystal Reports report type (or a plan for creating them)

### Find All Crystal Reports References

```bash
# Find ReportDocument usage
rg -l "ReportDocument\|CrystalDecisions\|ExportToDisk\|ExportToStream" --type cs
rg "ReportDocument\|CrystalDecisions" --type cs -n

# Find .rpt files in the project
find . -name "*.rpt" | sort

# Find Crystal Reports NuGet package references
grep -r "CrystalDecisions\|Crystal\.Reports" *.csproj **/*.csproj 2>/dev/null

# Count distinct report templates to estimate migration scope
find . -name "*.rpt" | wc -l
```

### Uninstall / Install

```bash
# CR4VS is normally referenced as GAC assemblies (CrystalDecisions.*.dll)
# delivered by the Crystal Reports runtime MSI. Remove those references
# from your .csproj. If you used a community NuGet wrapper, remove it too:
dotnet remove package CrystalReports.Engine
dotnet remove package CrystalReports.Shared
dotnet remove package CrystalReports.ReportAppServer

# Uninstall the Crystal Reports runtime MSI from build/deploy targets.

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
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic Export

**Before (Crystal Reports):**
```csharp
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;

class Program
{
    static void Main()
    {
        // One ReportDocument per execution — not thread-safe for shared use
        using var report = new ReportDocument();
        report.Load("Reports/SalesReport.rpt");

        // Bind data source
        report.SetDataSource(GetReportData()); // application data

        // Export to PDF
        report.ExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
        report.ExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;

        var diskOptions = (DiskFileDestinationOptions)report.ExportOptions.DestinationOptions;
        diskOptions.DiskFileName = "output.pdf";

        report.Export();
        Console.WriteLine("Exported output.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// HTML template replaces .rpt file — data bound via string interpolation
var data = GetReportData(); // your data source
var html = BuildSalesReportHtml(data); // HTML template method

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## The Template Migration Story

The most significant work in this migration isn't the API swap — it's replacing RPT file templates with HTML templates. A Crystal Reports `.rpt` file is a binary designer format; the equivalent in IronPDF is an HTML string or file.

For each report, the migration pattern is:

1. Export a sample PDF from Crystal Reports (visual reference)
2. Build an HTML/CSS template that produces equivalent visual output
3. Replace `report.SetDataSource(data)` with C# string interpolation (or Razor, Scriban, Fluid, etc.)
4. Render via `ChromePdfRenderer`

A minimal Razor-based example:

```csharp
using IronPdf;
using System;
using System.Text;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Data model that replaces ReportDocument data source binding
var reportData = new SalesReportData
{
    ReportTitle = "Sales Report — Q3 2024",
    GeneratedAt = DateTime.UtcNow,
    Lines = new[]
    {
        new SalesLine { Region = "North America", Revenue = 12_400_000m, YoY = 8.2 },
        new SalesLine { Region = "Europe",        Revenue =  9_100_000m, YoY = 3.7 },
        new SalesLine { Region = "APAC",          Revenue =  6_800_000m, YoY = 14.3 },
    }
};

// Build HTML from data — replaces the .rpt data binding model
var sb = new StringBuilder();
sb.Append(@"<html><head><style>
    body { font-family: Arial, sans-serif; padding: 40px; }
    h1 { font-size: 20px; }
    table { width: 100%; border-collapse: collapse; }
    th, td { border: 1px solid #ccc; padding: 8px; font-size: 12px; }
    th { background: #f0f0f0; }
</style></head><body>");

sb.Append($"<h1>{reportData.ReportTitle}</h1>");
sb.Append($"<p>Generated: {reportData.GeneratedAt:yyyy-MM-dd HH:mm} UTC</p>");
sb.Append("<table><tr><th>Region</th><th>Revenue</th><th>YoY Change</th></tr>");

foreach (var line in reportData.Lines)
{
    sb.Append($"<tr><td>{line.Region}</td>");
    sb.Append($"<td>${line.Revenue:N0}</td>");
    sb.Append($"<td>{(line.YoY >= 0 ? "+" : "")}{line.YoY:F1}%</td></tr>");
}

sb.Append("</table></body></html>");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(sb.ToString());
pdf.SaveAs("sales-report.pdf");

Console.WriteLine($"Saved sales-report.pdf ({pdf.PageCount} page(s))");
```

For larger applications, consider a dedicated template engine like **Scriban** or **Fluid** (both MIT-licensed) to separate template files from C# code — similar to how RPT files were separate from code.

---

## API Mapping Tables

### Namespace Mapping

| Crystal Reports | IronPDF | Notes |
|---|---|---|
| `CrystalDecisions.CrystalReports.Engine` | `IronPdf` | Core namespace |
| `CrystalDecisions.Shared` | `IronPdf.Rendering` | Options/config |
| `CrystalDecisions.ReportAppServer.*` | N/A — removed | Report server layer not applicable |

### Core Class Mapping

| Crystal Reports Class | IronPDF Class | Description |
|---|---|---|
| `ReportDocument` | `ChromePdfRenderer` | Primary rendering class (different input model) |
| `ExportOptions` | `ChromePdfRenderOptions` | Output configuration |
| `DiskFileDestinationOptions` | `pdf.SaveAs(path)` | File output |
| `MemoryStreamDestinationOptions` | `pdf.Stream` / `pdf.BinaryData` | Stream output |

### Document Loading Methods

| Operation | Crystal Reports | IronPDF |
|---|---|---|
| Load report template | `report.Load("file.rpt")` | HTML string / file / URL as input |
| Bind data | `report.SetDataSource(dataset)` | String interpolation / template engine |
| Export to file | `report.ExportToDisk(format, path)` | `pdf.SaveAs(path)` |
| Export to stream | `report.ExportToStream(format)` | `pdf.Stream` |

### Page Operations

| Operation | Crystal Reports | IronPDF |
|---|---|---|
| Page count | Available via export | `pdf.PageCount` |
| Remove page | Not available | `pdf.RemovePages(index)` |
| Extract text | Limited | `pdf.ExtractAllText()` |
| Rotate | Report designer | `pdf.RotateAllPages()` |

### Merge / Split Operations

| Operation | Crystal Reports | IronPDF |
|---|---|---|
| Merge | Not native | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not native | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF (data-bound report replacement)

**Before (Crystal Reports):**
```csharp
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Data;
using System.IO;

class SalesReportExport
{
    static void Main()
    {
        // Load binary .rpt template — not diff-able, requires CR designer
        using var report = new ReportDocument();
        report.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "Sales.rpt"));

        // Bind DataSet to report — Crystal Reports data binding model
        var ds = BuildDataSet();  // your data access layer
        report.SetDataSource(ds);

        // Configure export
        report.ExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
        report.ExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;

        var diskOpts = new DiskFileDestinationOptions { DiskFileName = "sales-q3.pdf" };
        report.ExportOptions.DestinationOptions = diskOpts;

        report.Export();
        Console.WriteLine("Exported sales-q3.pdf");
        report.Close();
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var data = BuildReportData(); // same data source, different binding
var html = RenderSalesReportTemplate(data); // HTML template replaces .rpt

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("sales-q3.pdf");

Console.WriteLine($"Saved sales-q3.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (Crystal Reports — no native merge):**
```csharp
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;

class MergeBefore
{
    static void Main()
    {
        // Crystal Reports doesn't support merging separate report PDFs natively.
        // Teams generate each separately and use a secondary library to combine.

        var reports = new[] { "Section1.rpt", "Section2.rpt" };
        var pdfPaths = new System.Collections.Generic.List<string>();

        foreach (var rpt in reports)
        {
            using var doc = new ReportDocument();
            doc.Load(rpt);
            doc.SetDataSource(GetData());
            var outPath = $"{rpt}.pdf";
            doc.ExportToDisk(ExportFormatType.PortableDocFormat, outPath);
            pdfPaths.Add(outPath);
            doc.Close();
        }

        // Merge via secondary library — SomePdfLib.Merge(pdfPaths) — illustrative
        Console.WriteLine("Merge requires secondary library alongside Crystal Reports");
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
    renderer.RenderHtmlAsPdfAsync(RenderSection1Html()),
    renderer.RenderHtmlAsPdfAsync(RenderSection2Html())
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("annual-report.pdf");

Console.WriteLine($"Merged: {merged.PageCount} total pages");
```

---

### 3. Watermark

**Before (Crystal Reports — designer section or secondary library):**
```csharp
using CrystalDecisions.CrystalReports.Engine;
using System;

class WatermarkBefore
{
    static void Main()
    {
        // Crystal Reports watermark: typically a text/image placed in an
        // "Unbound" section or Page Header/Footer in the RPT designer.
        // Programmatic watermark post-export requires a secondary PDF library.

        // Post-export approach (illustrative — use secondary library):
        // var pdfBytes = ExportReportToBytes();
        // var watermarked = SomePdfLib.AddTextWatermark(pdfBytes, "DRAFT");
        // File.WriteAllBytes("watermarked.pdf", watermarked);

        Console.WriteLine("Programmatic watermark requires secondary library with Crystal Reports");
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
    "<html><body><h1>Sales Report Q3 2024</h1></body></html>"
);

// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronSoftware.Drawing.Color.Gray,
    Opacity = 15, // 0-100
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("draft-report.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (Crystal Reports — not native; secondary library required):**
```csharp
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.IO;

class PasswordBefore
{
    static void Main()
    {
        using var report = new ReportDocument();
        report.Load("ConfidentialReport.rpt");
        report.SetDataSource(GetData());

        // Export to PDF first — no security options in ExportOptions for PDF password
        report.ExportToDisk(ExportFormatType.PortableDocFormat, "temp.pdf");
        report.Close();

        // Then apply password via secondary library (required)
        // var pdfBytes = File.ReadAllBytes("temp.pdf");
        // var secured = SomePdfLib.SetPassword(pdfBytes, "open123", "admin456");
        // File.WriteAllBytes("secured.pdf", secured);
        // File.Delete("temp.pdf");

        Console.WriteLine("Password protection requires a secondary library with Crystal Reports");
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
    "<html><body><h1>Confidential Report</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### Thread Safety — The Origin Problem

The thread-safety issue that often triggers this migration has a direct IronPDF parallel. For concurrent rendering, the recommended pattern is one `ChromePdfRenderer` per task rather than sharing a single instance across threads:

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

// Per-task renderer (safe regardless of thread-safety guarantees)
// https://ironpdf.com/examples/parallel/
var jobs = new[] { html1, html2, html3 };

var pdfs = await Task.WhenAll(jobs.Select(async html =>
{
    var renderer = new ChromePdfRenderer(); // one per task
    return await renderer.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Rendered {pdfs.Length} PDFs concurrently");
foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Page Indexing Shift

Crystal Reports uses 1-based page references in its API. IronPDF uses 0-based page indexing. Any page manipulation code needs index adjustment.

### Report Count → RPT Migration Scope

Before committing to a migration timeline, count your distinct `.rpt` files — that number determines the bulk of the work:

```bash
find . -name "*.rpt" | wc -l
```

A reasonable estimate for each report template conversion to HTML: 30 minutes for simple single-section reports, 2–4 hours for complex multi-section reports with sub-reports or complex data binding.

### Sub-Report Handling

Crystal Reports sub-reports embed other reports within a main report. The HTML equivalent is composing HTML partials. For complex sub-report structures, consider whether the template composition logic should live in a template engine or in C#:

```csharp
// Crystal Reports sub-report: embedded secondary .rpt file
// IronPDF equivalent: compose HTML sections in code

var mainHtml = BuildMainReportHtml(mainData);
var subHtml = BuildSubReportHtml(subData);

// Option A: Embed sub-content in main HTML before rendering
var combinedHtml = mainHtml.Replace("{{SUB_REPORT}}", subHtml);
var pdf = await renderer.RenderHtmlAsPdfAsync(combinedHtml);

// Option B: Render separately and merge
var mainPdf = await renderer.RenderHtmlAsPdfAsync(mainHtml);
var subPdf  = await renderer.RenderHtmlAsPdfAsync(subHtml);
var merged  = PdfDocument.Merge(mainPdf, subPdf);
```

---

## Performance Considerations

### Concurrent Rendering Pattern

```csharp
using IronPdf;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Unlike Crystal Reports, each render doesn't require COM resource allocation
// Parallel pattern: https://ironpdf.com/examples/parallel/

var reportJobs = new List<(string Html, string OutPath)>
{
    (BuildSalesHtml(salesData),      "reports/sales.pdf"),
    (BuildInventoryHtml(invData),    "reports/inventory.pdf"),
    (BuildFinanceHtml(financeData),  "reports/finance.pdf"),
};

await Task.WhenAll(reportJobs.Select(async job =>
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(job.Html);
    pdf.SaveAs(job.OutPath);
}));

Console.WriteLine("All reports generated concurrently");
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

// Crystal Reports: report.Close() was required
// IronPDF: using statement handles disposal

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// In-memory bytes if needed (e.g., API response, email attachment)
var bytes = pdf.BinaryData;
// pdf disposed at end of 'using' block
```

### Renderer Warm-Up for Long-Running Services

```csharp
using IronPdf;

// Amortize Chromium initialization over application lifetime
// vs Crystal Reports COM init per ReportDocument
var renderer = new ChromePdfRenderer();
using var _ = await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");
// Ready for production traffic
```

---

## Migration Checklist

### Pre-Migration
- [ ] Count `.rpt` files (`find . -name "*.rpt" | wc -l`) — determines scope
- [ ] Categorize report complexity (simple, tabular, multi-section, sub-reports)
- [ ] Identify secondary libraries currently used for watermark/security/merge
- [ ] Audit thread safety issues — find locations where ReportDocument is shared
- [ ] Document current export options (page size, orientation, compression)
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility
- [ ] Choose a template engine if needed (Scriban, Fluid, or Razor)

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove Crystal Reports package references
- [ ] Add license key at application startup
- [ ] Create HTML templates for each `.rpt` report (highest-effort step)
- [ ] Replace `report.SetDataSource()` with template data binding
- [ ] Replace `report.ExportToDisk()` with `pdf.SaveAs()`
- [ ] Replace `report.ExportToStream()` with `pdf.Stream` / `pdf.BinaryData`
- [ ] Replace secondary merge library with `PdfDocument.Merge()`
- [ ] Replace secondary security library with `pdf.SecuritySettings`
- [ ] Replace watermark designer sections with `TextStamper` / `ImageStamper`

### Testing
- [ ] Compare PDF output visually against Crystal Reports reference exports
- [ ] Verify data binding — all fields present and correctly formatted
- [ ] Test concurrent rendering under load (validate thread safety improvement)
- [ ] Test page count matches reference
- [ ] Test password protection
- [ ] Test merge output page order
- [ ] Verify fonts render correctly in target deployment environment

### Post-Migration
- [ ] Remove Crystal Reports runtime from deployment packages
- [ ] Remove COM registration steps from deployment scripts
- [ ] Archive `.rpt` files for reference (don't delete immediately)
- [ ] Remove secondary PDF libraries now consolidated into IronPDF

---

## Final Thoughts

The `.rpt` file count is the single most important number before starting this migration. Two reports or twenty-two changes the timeline dramatically. The API swap itself is mechanical; the template rewrite is proportional to the number and complexity of your reports.

The concurrency improvement — eliminating the one-instance-per-request COM bottleneck — is immediate and verifiable once the first report type is migrated.

**Discussion question:** What would you add to the migration checklist above — particularly around complex sub-report patterns or Crystal Reports data-binding features that didn't translate cleanly to HTML templates?
