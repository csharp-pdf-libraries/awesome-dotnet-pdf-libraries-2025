# SSRS to IronPDF: three steps and you're done

The SSRS instance hasn't had a meaningful update in three years. Report subscriptions fail silently, the mobile layout never worked properly, and getting a new report deployed still requires DBA involvement for dataset configuration and a Reporting Services Manager session on a Windows Server that nobody wants to touch. SSRS is infrastructure masquerading as a development tool, and the maintenance overhead is real — not because the reports are broken, but because keeping the reporting infrastructure running in modern environments takes effort that scales poorly.

If you're looking at whether SSRS reports can be replaced with something that deploys with your application code, this article covers the migration path to IronPDF. You'll have working before/after code for HTML-to-PDF generation, merge, watermark, and password protection by the end.

---

## Why Migrate (Without Drama)

Teams moving reports from SSRS to code-based generation commonly encounter:

1. **Infrastructure overhead** — SSRS requires a Windows Server, SQL Server licensing, IIS configuration, and dedicated maintenance.
2. **Deployment gap** — getting a new `.rdl` report into production requires server access and DBA coordination; code-based reports deploy with the application.
3. **.NET Core/Linux incompatibility** — SSRS server runs on Windows only; the in-process `Microsoft.Reporting.NETCore` path has limitations (no subreports in some versions, rendering differences).
4. **Modern CSS/HTML** — SSRS's HTML renderer is outdated; it doesn't support modern web layouts. SSRS's PDF output comes from its own renderer, not a browser engine.
5. **Designer tooling** — `.rdl` files require Report Builder or Visual Studio RDLC designer; non-trivial onboarding for developers who work in code.
6. **Subscription complexity** — email subscriptions, data-driven subscriptions, and schedule management are SSRS server configuration, not application code.
7. **Version control** — `.rdl` XML files diff reasonably, but the workflow for testing changes still requires a report server.
8. **Mobile/responsive output** — SSRS's Mobile Report Publisher is a separate tool with a different model; web-responsive PDF from HTML is simpler.
9. **Docker/cloud** — SSRS server doesn't containerize easily; code-based PDF generation can run anywhere .NET runs.
10. **Testing** — unit testing SSRS reports is difficult; HTML templates can be tested and previewed in any browser.

### Comparison Table

| Aspect | SSRS | IronPDF |
|---|---|---|
| Focus | Server-based enterprise reporting platform | HTML-to-PDF + PDF manipulation library |
| Pricing | SQL Server + Windows Server licensing — verify at Microsoft | Commercial library license — verify at ironsoftware.com |
| API Style | SOAP/REST API to report server, or in-process ReportViewer | .NET library; no external server |
| Learning Curve | High — server infrastructure + RDL designer + data source config | Low for .NET devs; HTML/CSS templates |
| HTML Rendering | SSRS renderer — not browser-based | Embedded Chromium |
| Page Indexing | 1-based in SSRS rendering APIs | 0-based |
| Thread Safety | Server handles concurrency | Verify IronPDF concurrent instance guidance |
| Namespace | `Microsoft.Reporting.NETCore` (in-process) / SOAP proxy | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | SSRS | IronPDF Equivalent | Complexity |
|---|---|---|---|
| Basic report to PDF | SSRS subscription / URL render / ReportViewer | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | High (RDL → HTML) |
| Data binding | Dataset / data source in RDL | C# string interpolation / template engine | High |
| Tables / lists | Report data region (tablix) | HTML `<table>` | Medium |
| Charts | SSRS chart data region | Chart.js / similar via HTML | High |
| Sub-reports | `.rdl` sub-report control | HTML composition | High |
| Scheduled delivery | SSRS subscription manager | Background service + email API | High |
| Export to file | URL rendering / `LocalReport.Render()` | `pdf.SaveAs(path)` | Low |
| Export to stream | `LocalReport.Render()` | `pdf.Stream` | Low |
| Merge PDFs | Not native | `PdfDocument.Merge()` | Medium |
| Password protection | Not native in standard SSRS | `pdf.SecuritySettings` | Low |
| Headers/footers | Report header/footer sections | `RenderingOptions.HtmlHeader/Footer` | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Simple tabular reports with no charts or subscriptions | Switch — HTML table templates are straightforward |
| Complex multi-dataset charts and sub-reports | Evaluate migration cost carefully — high rewrite scope |
| Reports need to deploy without server infrastructure | Switch — eliminates SSRS server dependency |
| Scheduled email delivery of reports is the primary feature | SSRS handles this well; code replacement needs background service + email API |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)
- HTML templates for each SSRS report (or a plan for creating them)

### Find All SSRS/ReportViewer References

```bash
# Find ReportViewer / LocalReport usage
rg -l "LocalReport\|ReportViewer\|ReportDataSource\|ReportParameter\|Microsoft\.Reporting" --type cs
rg "LocalReport\|ReportViewer\|\.Render\b" --type cs -n

# Find .rdl and .rdlc files
find . -name "*.rdl" -o -name "*.rdlc" | sort
find . -name "*.rdl" | wc -l  # scope of template migration

# Find SSRS SOAP proxy usage
rg "ReportService2010\|ReportingService\|SoapHttpClientProtocol" --type cs -n

# Find NuGet references
grep -r "Microsoft\.Reporting\|ReportViewer" *.csproj **/*.csproj 2>/dev/null
```

### Uninstall / Install

```bash
# Remove ReportViewer packages
dotnet remove package Microsoft.Reporting.NETCore  # verify package name
dotnet remove package Microsoft.ReportViewer.Core  # verify package name

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
using Microsoft.Reporting.NETCore;  // in-process path
// or SSRS SOAP proxy namespace for server path
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic Report Generation

**Before (SSRS in-process via LocalReport):**
```csharp
using Microsoft.Reporting.NETCore;
using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var report = new LocalReport();
        report.ReportPath = "Reports/SalesReport.rdlc";

        // Bind data source
        var data = GetSalesData(); // IEnumerable<SalesRow>
        report.DataSources.Add(new ReportDataSource("DataSet1", data));

        // Set parameters
        report.SetParameters(new[] {
            new ReportParameter("ReportTitle", "Q3 2024 Sales"),
            new ReportParameter("DateRange", "Jul–Sep 2024"),
        });

        // Render to PDF
        var result = report.Render("PDF");
        File.WriteAllBytes("sales-report.pdf", result);
        Console.WriteLine("Saved sales-report.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// HTML template replaces .rdlc file — data bound via C# interpolation
var data = GetSalesData();
var html = BuildSalesReportHtml(data, title: "Q3 2024 Sales", dateRange: "Jul–Sep 2024");

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("sales-report.pdf");

Console.WriteLine($"Saved sales-report.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## The RDL → HTML Template Migration

The most significant work in this migration is converting `.rdl` / `.rdlc` report definitions to HTML templates. SSRS report definition XML describes the layout declaratively — the equivalent in IronPDF is an HTML file.

For each report, the process is:

1. Export a sample PDF from SSRS (visual reference)
2. Identify the data regions (tablix tables, matrices, sub-reports, charts)
3. Build an HTML/CSS template for the layout
4. Replace SSRS expression syntax (`=Fields!ColumnName.Value`) with C# interpolation or a template engine

A simple tabular report conversion:

```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var salesData = new[]
{
    new { Region = "North America", Revenue = 12_400_000m, Units = 4820 },
    new { Region = "Europe",        Revenue =  9_100_000m, Units = 3210 },
    new { Region = "APAC",          Revenue =  6_800_000m, Units = 2440 },
};

// Build HTML — replaces SSRS tablix data region
var rows = new StringBuilder();
foreach (var row in salesData)
{
    rows.Append($"<tr><td>{row.Region}</td><td>${row.Revenue:N0}</td><td>{row.Units:N0}</td></tr>");
}

var totalRevenue = salesData.Sum(r => r.Revenue);
var totalUnits   = salesData.Sum(r => r.Units);

var html = $@"
    <html>
    <head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; color: #333; }}
        h1 {{ font-size: 20px; margin-bottom: 4px; }}
        .subtitle {{ color: #666; font-size: 12px; margin-bottom: 20px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{ background: #2c3e50; color: white; padding: 8px; text-align: left; font-size: 12px; }}
        td {{ border-bottom: 1px solid #eee; padding: 8px; font-size: 12px; }}
        .total-row {{ font-weight: bold; background: #f9f9f9; }}
    </style>
    </head>
    <body>
        <h1>Q3 2024 Sales Report</h1>
        <div class='subtitle'>Jul–Sep 2024</div>
        <table>
            <tr><th>Region</th><th>Revenue</th><th>Units</th></tr>
            {rows}
            <tr class='total-row'>
                <td>Total</td>
                <td>${totalRevenue:N0}</td>
                <td>{totalUnits:N0}</td>
            </tr>
        </table>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
// https://ironpdf.com/how-to/headers-and-footers/
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:right; font-size:9px; color:#999'>Page {page} of {total-pages}</div>",
};

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("q3-sales.pdf");
Console.WriteLine($"Saved q3-sales.pdf ({pdf.PageCount} page(s))");
```

For larger template sets, consider a dedicated template engine: **Scriban** or **Fluid** (both MIT-licensed) provide a separation between template markup and C# code that's closer to SSRS's RDL model.

---

## API Mapping Tables

### Namespace Mapping

| SSRS / ReportViewer | IronPDF | Notes |
|---|---|---|
| `Microsoft.Reporting.NETCore` | `IronPdf` | Core namespace |
| `Microsoft.Reporting.NETCore.ReportDataSource` | N/A — data bound in HTML | Template-level data binding |
| SSRS SOAP proxy namespace | N/A — removed | No external server |

### Core Class Mapping

| SSRS Concept | IronPDF Class | Description |
|---|---|---|
| `LocalReport` + `.rdlc` file | `ChromePdfRenderer` + HTML | No .rdlc file needed |
| `ReportDataSource` | HTML template + C# data | Data passed as template variables |
| `ReportParameter` | HTML string interpolation | Report parameters become template inputs |
| N/A | `PdfDocument` | PDF manipulation object |

### Document Loading Methods

| Operation | SSRS / LocalReport | IronPDF |
|---|---|---|
| Load report definition | `report.ReportPath = "file.rdlc"` | HTML string / file / URL |
| Bind data | `report.DataSources.Add(...)` | C# interpolation / template engine |
| Render to bytes | `report.Render("PDF")` | `renderer.RenderHtmlAsPdfAsync(html)` |
| Save to file | `File.WriteAllBytes(path, bytes)` | `pdf.SaveAs(path)` |

### Page Operations

| Operation | SSRS | IronPDF |
|---|---|---|
| Page count | From render result | `pdf.PageCount` |
| Remove page | N/A | `pdf.RemovePage(index)` — verify |
| Extract text | N/A from LocalReport | `pdf.ExtractAllText()` |
| Rotate | N/A | Verify in IronPDF docs |

### Merge / Split Operations

| Operation | SSRS | IronPDF |
|---|---|---|
| Merge | Not native | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not native | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. Report to PDF (LocalReport in-process)

**Before (SSRS LocalReport):**
```csharp
using Microsoft.Reporting.NETCore;
using System;
using System.Collections.Generic;
using System.IO;

class ReportToPdfBefore
{
    static void Main()
    {
        var report = new LocalReport();
        report.ReportPath = Path.Combine(AppContext.BaseDirectory, "Reports", "Invoice.rdlc");

        // Bind typed data — matches DataSet name in the .rdlc XML
        var invoiceData = new List<InvoiceRow>
        {
            new() { ProductName = "Widget Pro", Qty = 3, UnitPrice = 149.00m },
            new() { ProductName = "Gadget Plus", Qty = 1, UnitPrice = 299.00m },
        };
        report.DataSources.Add(new ReportDataSource("InvoiceDataSet", invoiceData));

        // Parameters defined in .rdlc
        report.SetParameters(new[]
        {
            new ReportParameter("CustomerName", "Acme Corp"),
            new ReportParameter("InvoiceNumber", "INV-2024-0099"),
        });

        // Render
        var pdfBytes = report.Render("PDF");
        File.WriteAllBytes("invoice.pdf", pdfBytes);
        Console.WriteLine($"Saved invoice.pdf ({pdfBytes.Length} bytes)");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var invoiceLines = new[]
{
    new { Product = "Widget Pro",  Qty = 3, Unit = 149.00m },
    new { Product = "Gadget Plus", Qty = 1, Unit = 299.00m },
};
var total = invoiceLines.Sum(l => l.Qty * l.Unit);

var rows = new StringBuilder();
foreach (var line in invoiceLines)
    rows.Append($"<tr><td>{line.Product}</td><td>{line.Qty}</td><td>${line.Unit:N2}</td><td>${line.Qty * line.Unit:N2}</td></tr>");

var html = $@"
    <html><head><style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        h1 {{ font-size: 20px; }} table {{ width: 100%; border-collapse: collapse; }}
        th {{ background: #f0f0f0; padding: 8px; border: 1px solid #ddd; font-size: 12px; }}
        td {{ padding: 8px; border: 1px solid #ddd; font-size: 12px; }}
        .total {{ font-weight: bold; text-align: right; margin-top: 10px; }}
    </style></head>
    <body>
        <h1>Invoice INV-2024-0099 — Acme Corp</h1>
        <table><tr><th>Product</th><th>Qty</th><th>Unit</th><th>Total</th></tr>{rows}</table>
        <div class='total'>Total: ${total:N2}</div>
    </body></html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");
Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (SSRS — not native; secondary library required):**
```csharp
using Microsoft.Reporting.NETCore;
using System;
using System.IO;
using System.Collections.Generic;

class MergeBefore
{
    static void Main()
    {
        // SSRS LocalReport has no merge; generate each section separately
        var sections = new[]
        {
            ("Reports/Section1.rdlc", "DataSet1", GetSection1Data()),
            ("Reports/Section2.rdlc", "DataSet2", GetSection2Data()),
        };

        var pdfFiles = new List<string>();
        for (int i = 0; i < sections.Length; i++)
        {
            var (rdlcPath, dsName, data) = sections[i];
            var report = new LocalReport();
            report.ReportPath = rdlcPath;
            report.DataSources.Add(new ReportDataSource(dsName, data));
            var bytes = report.Render("PDF");
            var outPath = $"section{i + 1}.pdf";
            File.WriteAllBytes(outPath, bytes);
            pdfFiles.Add(outPath);
        }

        // Merge via secondary library — no merge in LocalReport
        Console.WriteLine("Merge requires secondary library alongside SSRS LocalReport");
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

**Before (SSRS — CSS text or secondary library post-processing):**
```csharp
using Microsoft.Reporting.NETCore;
using System;
using System.IO;

class WatermarkBefore
{
    static void Main()
    {
        // SSRS: watermarks added via text box in Report Header section (designer)
        // OR post-processing with secondary PDF library
        var report = new LocalReport();
        report.ReportPath = "Reports/Report.rdlc";
        report.DataSources.Add(new ReportDataSource("DataSet", GetData()));
        var bytes = report.Render("PDF");

        // Post-process watermark via secondary library (no LocalReport watermark API):
        // var watermarked = SomePdfLib.AddWatermark(bytes, "DRAFT");
        // File.WriteAllBytes("watermarked.pdf", watermarked);

        Console.WriteLine("SSRS watermark requires designer section or secondary library");
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
    BuildReportHtml(GetData())
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
pdf.SaveAs("watermarked-report.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (SSRS LocalReport — not supported):**
```csharp
using Microsoft.Reporting.NETCore;
using System;
using System.IO;

class PasswordBefore
{
    static void Main()
    {
        var report = new LocalReport();
        report.ReportPath = "Reports/Report.rdlc";
        report.DataSources.Add(new ReportDataSource("DataSet", GetData()));

        // LocalReport.Render("PDF") has no security/password options
        var bytes = report.Render("PDF");
        File.WriteAllBytes("report.pdf", bytes);

        // Apply password via secondary library (required):
        // var secured = SomePdfLib.SetPassword(bytes, "open123", "admin456");
        // File.WriteAllBytes("secured.pdf", secured);

        Console.WriteLine("Password protection requires secondary library with SSRS LocalReport");
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

### RDL Count → Scope

Count your `.rdl` / `.rdlc` files before scoping the migration:

```bash
find . -name "*.rdl" -o -name "*.rdlc" | wc -l
```

Each report definition needs an HTML equivalent. Simple tabular reports: 30–60 minutes each. Complex reports with charts, sub-reports, or matrix data regions: several hours each.

### Charts

SSRS has a native chart data region. HTML templates can render charts via JavaScript chart libraries (Chart.js, ApexCharts, Highcharts) — IronPDF executes JavaScript in the Chromium renderer. For static PDF output, these render cleanly:

```csharp
var htmlWithChart = @"
    <html>
    <head>
    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
    </head>
    <body>
    <canvas id='salesChart' width='600' height='300'></canvas>
    <script>
        new Chart(document.getElementById('salesChart'), {
            type: 'bar',
            data: {
                labels: ['Q1', 'Q2', 'Q3', 'Q4'],
                datasets: [{ label: 'Revenue ($M)', data: [8.2, 9.4, 12.4, 11.1] }]
            }
        });
    </script>
    </body></html>";

// IronPDF waits for JS to execute before rendering
// Verify JS wait settings in: https://ironpdf.com/how-to/rendering-options/
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(htmlWithChart);
```

### Page Numbering Migration

SSRS uses page number expressions in report header/footer sections (`=Globals!PageNumber`). IronPDF uses tokens in `HtmlHeaderFooter`:

```csharp
// SSRS footer expression: =Globals!PageNumber & " of " & Globals!TotalPages
// IronPDF equivalent:
// https://ironpdf.com/how-to/headers-and-footers/
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:right; font-size:9px'>{page} of {total-pages}</div>",
    // Verify exact token names in current IronPDF docs
};
```

### Scheduled Delivery

SSRS's subscription-based email delivery has no direct IronPDF equivalent — that's application code:

```csharp
using IronPdf;
using System.Net.Mail;
using System.Threading.Tasks;

// IronPDF generates the PDF; your application handles email delivery
async Task SendReportByEmail(string recipientEmail)
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(BuildReportHtml(GetData()));
    var pdfBytes = pdf.BinaryData;

    using var message = new MailMessage("reports@yourcompany.com", recipientEmail)
    {
        Subject = "Monthly Report",
        Body = "Please find your report attached.",
        Attachments = { new Attachment(new System.IO.MemoryStream(pdfBytes), "report.pdf", "application/pdf") },
    };

    using var smtp = new SmtpClient("your-smtp-server");
    await smtp.SendMailAsync(message);
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
// Replace SSRS subscriptions with parallel generation + direct delivery
var reportRequests = GetPendingReportRequests();

await Task.WhenAll(reportRequests.Select(async req =>
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(BuildReportHtml(req.Data));
    pdf.SaveAs(req.OutputPath);
    // Or: await SendReportByEmail(req.Email, pdf.BinaryData);
}));

Console.WriteLine($"Generated {reportRequests.Length} reports in parallel");
// See: https://ironpdf.com/how-to/async/
```

### Renderer Warm-Up

```csharp
using IronPdf;

// Initialize at application startup to avoid cold-start latency on first report
var renderer = new ChromePdfRenderer();
using var _ = await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");
// Renderer ready for production traffic
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();

// Use 'using' for automatic disposal
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// For email attachment (bytes):
var bytes = pdf.BinaryData;
// pdf disposed at end of 'using' block
```

---

## Migration Checklist

### Pre-Migration
- [ ] Count `.rdl` and `.rdlc` files (`find . -name "*.rdl*" | wc -l`)
- [ ] Categorize reports: tabular, matrix, chart-heavy, sub-report compositions
- [ ] Identify scheduled subscriptions that need replacement with background services
- [ ] Document current data source connections to replicate in application code
- [ ] Identify secondary libraries used with SSRS (merge, security)
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `Microsoft.Reporting.NETCore` / ReportViewer packages
- [ ] Add license key at application startup
- [ ] Convert each `.rdlc` report to HTML template
- [ ] Replace `ReportDataSource` binding with C# data binding / template engine
- [ ] Replace `report.SetParameters()` with template variable injection
- [ ] Replace `report.Render("PDF")` with `ChromePdfRenderer.RenderHtmlAsPdfAsync()`
- [ ] Replace SSRS page number expressions with `HtmlHeaderFooter` tokens
- [ ] Build background service to replace SSRS subscription delivery
- [ ] Replace secondary merge/security libraries with IronPDF equivalents

### Testing
- [ ] Compare PDF output visually against SSRS reference exports
- [ ] Verify data binding — all fields present and correctly formatted
- [ ] Test page numbers render correctly in headers/footers
- [ ] Test chart rendering if using JS chart libraries
- [ ] Test merge and security features
- [ ] Benchmark report generation time vs SSRS baseline
- [ ] Test in Docker/Linux if applicable (eliminated SSRS Windows Server dependency)

### Post-Migration
- [ ] Decommission SSRS server (after validation period)
- [ ] Remove ReportViewer NuGet packages
- [ ] Archive `.rdl`/`.rdlc` files for reference
- [ ] Remove SSRS-specific SQL Server components if no longer needed

---

## One Last Thing

The RDL count and chart complexity are the two variables that determine whether this migration takes days or weeks. Simple tabular reports from `LocalReport` migrate cleanly and quickly. Chart-heavy, sub-report-composed, subscription-driven reporting infrastructure is a larger project.

The operational win — running reports without a dedicated Windows Server, IIS, and SQL Server Reporting Services installation — is the clearest immediate benefit once any report type is migrated.

**Discussion question:** Which SSRS report feature was hardest to replicate in HTML — was it charts, sub-reports, matrix data regions, or something the article didn't cover?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
