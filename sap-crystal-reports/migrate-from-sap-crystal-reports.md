# Migration Guide: SAP Crystal Reports → IronPDF

## Why Migrate from SAP Crystal Reports to IronPDF

SAP Crystal Reports has become a heavyweight legacy solution with complex deployment requirements and significant SAP ecosystem dependencies that limit flexibility. Modern web-based reporting needs demand lighter, more maintainable solutions that integrate seamlessly with .NET applications without massive installations. IronPDF offers a streamlined, modern approach to PDF generation with simple deployment, active development, and no vendor lock-in.

## NuGet Package Changes

```xml
<!-- Remove SAP Crystal Reports -->
<PackageReference Remove="CrystalReports.Engine" />
<PackageReference Remove="CrystalReports.Shared" />
<PackageReference Remove="CrystalReports.ReportAppServer" />

<!-- Add IronPDF -->
<PackageReference Include="IronPdf" Version="2024.*" />
```

## Namespace Mapping

| SAP Crystal Reports | IronPDF |
|---------------------|---------|
| `CrystalDecisions.CrystalReports.Engine` | `IronPdf` |
| `CrystalDecisions.Shared` | `IronPdf` |
| `CrystalDecisions.ReportAppServer` | `IronPdf.Rendering` |
| `CrystalDecisions.Web` | `IronPdf` |

## API Mapping

| SAP Crystal Reports API | IronPDF Equivalent | Notes |
|------------------------|-------------------|-------|
| `ReportDocument` | `ChromePdfRenderer` | Core rendering engine |
| `ExportToDisk()` | `RenderHtmlAsPdf().SaveAs()` | Export to file |
| `ExportToStream()` | `RenderHtmlAsPdf()` | Returns PdfDocument |
| `SetDataSource()` | HTML templates with data binding | Use Razor, templating, or direct HTML |
| `SetParameterValue()` | String interpolation/templating | Dynamic content insertion |
| `Load()` | `RenderUrlAsPdf()` / `RenderHtmlAsPdf()` | Load from URL or HTML |
| `ExportFormatType.PortableDocFormat` | Default output format | PDF is native format |
| `ReportDocument.PrintToPrinter()` | `PdfDocument.Print()` | Print PDF directly |
| `Database.Tables` | Data source in HTML/Razor | Use standard .NET data access |

## Code Examples

### Example 1: Basic PDF Generation from Data

**Before (SAP Crystal Reports):**

```csharp
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

// Load report template
ReportDocument report = new ReportDocument();
report.Load(@"C:\Reports\InvoiceReport.rpt");

// Set data source
report.SetDataSource(invoiceDataSet);

// Set parameters
report.SetParameterValue("InvoiceNumber", "INV-12345");
report.SetParameterValue("CustomerName", "Acme Corp");

// Export to PDF
report.ExportToDisk(ExportFormatType.PortableDocFormat, 
    @"C:\Output\Invoice.pdf");

report.Close();
report.Dispose();
```

**After (IronPDF):**

```csharp
using IronPdf;

// Create HTML content with data
string htmlContent = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .header {{ background-color: #0066cc; color: white; padding: 20px; }}
        .invoice-details {{ margin: 20px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Invoice: INV-12345</h1>
    </div>
    <div class='invoice-details'>
        <h2>Customer: Acme Corp</h2>
        <table>
            {GenerateInvoiceTableRows(invoiceDataSet)}
        </table>
    </div>
</body>
</html>";

// Render PDF
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs(@"C:\Output\Invoice.pdf");
```

### Example 2: PDF Generation from URL/Template

**Before (SAP Crystal Reports):**

```csharp
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Data;

// Create and configure report
ReportDocument report = new ReportDocument();
report.Load(Server.MapPath("~/Reports/SalesReport.rpt"));

// Bind data
DataTable salesData = GetSalesData();
report.Database.Tables[0].SetDataSource(salesData);

// Apply filters
report.SetParameterValue("StartDate", startDate);
report.SetParameterValue("EndDate", endDate);
report.SetParameterValue("Region", "North America");

// Export to stream
System.IO.Stream stream = report.ExportToStream(
    ExportFormatType.PortableDocFormat);

// Send to browser
Response.ContentType = "application/pdf";
Response.BinaryWrite(((System.IO.MemoryStream)stream).ToArray());

report.Close();
```

**After (IronPDF):**

```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;

// Option 1: Render from URL (if you have a web page)
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf(
    $"https://myapp.com/reports/sales?start={startDate}&end={endDate}&region=NorthAmerica");

// Option 2: Generate HTML from data
var salesData = GetSalesData();
string htmlContent = $@"
<html>
<head>
    <style>
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; }}
        th {{ background-color: #4CAF50; color: white; }}
    </style>
</head>
<body>
    <h1>Sales Report: {startDate:d} to {endDate:d}</h1>
    <h2>Region: North America</h2>
    <table>
        <tr><th>Product</th><th>Sales</th><th>Revenue</th></tr>
        {string.Join("", salesData.Select(row => 
            $"<tr><td>{row.Product}</td><td>{row.Sales}</td><td>${row.Revenue:N2}</td></tr>"))}
    </table>
</body>
</html>";

pdf = renderer.RenderHtmlAsPdf(htmlContent);

// Return as file download
return File(pdf.BinaryData, "application/pdf", "SalesReport.pdf");
```

### Example 3: Advanced Formatting and Configuration

**Before (SAP Crystal Reports):**

```csharp
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

ReportDocument report = new ReportDocument();
report.Load(@"C:\Reports\DetailedReport.rpt");
report.SetDataSource(dataSet);

// Configure export options
PdfFormatOptions pdfOptions = new PdfFormatOptions();
pdfOptions.UsePageRange = true;
pdfOptions.StartPageNumber = 1;
pdfOptions.EndPageNumber = 10;

PdfRtfWordFormatOptions wordOptions = new PdfRtfWordFormatOptions();

ExportOptions exportOptions = report.ExportOptions;
exportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
exportOptions.ExportFormatOptions = pdfOptions;
exportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
exportOptions.ExportDestinationOptions = 
    new DiskFileDestinationOptions { DiskFileName = @"C:\Output\Report.pdf" };

report.Export();
report.Close();
```

**After (IronPDF):**

```csharp
using IronPdf;
using IronPdf.Rendering;

// Generate HTML content
string htmlContent = GenerateReportHtml(dataSet);

// Configure rendering options
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
renderer.RenderingOptions.MarginTop = 40;
renderer.RenderingOptions.MarginBottom = 40;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
renderer.RenderingOptions.PrintHtmlBackgrounds = true;

// Custom headers and footers
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Confidential Report</div>",
    DrawDividerLine = true
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>"
};

// Generate PDF
var pdf = renderer.RenderHtmlAsPdf(htmlContent);

// Extract specific pages (equivalent to page range)
var pageSubset = pdf.CopyPages(0, 9); // Pages 1-10 (0-indexed)
pageSubset.SaveAs(@"C:\Output\Report.pdf");
```

## Common Gotchas

### 1. **No Direct .rpt File Support**
Crystal Reports `.rpt` files cannot be directly converted. You must recreate report layouts using HTML/CSS or Razor templates. Consider using your existing reports as visual references for rebuilding in HTML.

### 2. **Data Binding Approach**
Crystal Reports uses proprietary data binding mechanisms. With IronPDF, you'll need to generate HTML with data already embedded using standard .NET techniques (string interpolation, Razor views, template engines like Scriban or Handlebars).

### 3. **Formula Fields**
Crystal Reports formulas must be converted to C# logic that generates the HTML content. Calculate values in your .NET code before rendering to HTML.

### 4. **Subreports**
Subreports must be converted to HTML sections or separate PDF documents that can be merged using IronPDF's `PdfDocument.Merge()` method.

### 5. **Parameter Prompts**
Crystal Reports' built-in parameter prompts need to be replaced with your own UI (web forms, WPF dialogs, etc.) to collect user input before PDF generation.

### 6. **Database Connectivity**
Remove Crystal Reports database drivers and connection logic. Use standard Entity Framework, Dapper, or ADO.NET to retrieve data, then pass it to your HTML templates.

### 7. **Licensing and Deployment**
IronPDF requires a license key for production use but has no complex runtime dependencies. Set your license key once in application startup:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 8. **Performance Considerations**
First PDF render may be slower as Chrome engine initializes. Consider warming up the renderer on application startup for production environments:
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderHtmlAsPdf("<html><body>Warmup</body></html>");
```

## Additional Resources

- **IronPDF Documentation:** https://ironpdf.com/docs/
- **IronPDF Tutorials:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/docs/questions/html-to-pdf/
- **Code Examples:** https://ironpdf.com/examples/