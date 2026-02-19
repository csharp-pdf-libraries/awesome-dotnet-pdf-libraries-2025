# How Do I Migrate from Telerik Reporting to IronPDF in C#?

## Why Migrate from Telerik Reporting?

Telerik Reporting is a full-featured enterprise reporting platform with a visual designer, but comes with significant overhead for PDF generation tasks. Key reasons to migrate:

1. **Expensive Bundle Licensing**: Requires DevCraft bundle ($1,000+ per developer) or standalone license
2. **Report Designer Dependency**: Requires installing Visual Studio extensions and runtime components
3. **Complex Infrastructure**: Needs report service hosting, connection strings, and data source configuration
4. **Proprietary Format**: Uses `.trdp`/`.trdx` files that lock you into the Telerik ecosystem
5. **Heavy Runtime**: Large deployment footprint for what may be simple PDF generation
6. **Annual Subscription**: Ongoing costs for updates and support

### When Telerik Reporting is Overkill

If you're using Telerik just to generate PDFs from data, you're paying for features you don't need:

| You Need | Telerik Provides (Unused) |
|----------|---------------------------|
| PDF from HTML | Visual designer, drill-downs |
| Simple reports | Interactive viewer, exports |
| Server-side PDFs | Desktop controls, charting engine |

IronPDF provides focused PDF generation without the enterprise reporting overhead—the full transition is mapped out in the [step-by-step migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-telerik-reporting-to-ironpdf/).

---

## Quick Start: Telerik Reporting to IronPDF

### Step 1: Replace NuGet Packages

```bash
# Remove Telerik Reporting packages
dotnet remove package Telerik.Reporting
dotnet remove package Telerik.Reporting.Services.AspNetCore
dotnet remove package Telerik.ReportViewer.Mvc

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using Telerik.Reporting.Drawing;

// After
using IronPdf;
```

### Step 3: Initialize License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## API Mapping Reference

| Telerik Reporting | IronPDF | Notes |
|-------------------|---------|-------|
| `Report` class | `ChromePdfRenderer` | Core rendering |
| `ReportProcessor` | `renderer.RenderHtmlAsPdf()` | PDF generation |
| `ReportSource` | HTML string or file | Content source |
| `.trdp` / `.trdx` files | HTML/CSS templates | Template format |
| `ReportParameter` | String interpolation / Razor | Parameters |
| `ReportDataSource` | C# data binding | Data source |
| `RenderReport("PDF")` | `RenderHtmlAsPdf()` | PDF output |
| `Export()` | `pdf.SaveAs()` | Save file |
| `TextBox` report item | HTML `<span>`, `<p>`, `<div>` | Text elements |
| `Table` report item | HTML `<table>` | Tables |
| `PictureBox` | HTML `<img>` | Images |
| `PageSettings` | `RenderingOptions` | Page configuration |

---

## Code Examples

### Example 1: Basic Report Generation

**Telerik Reporting:**
```csharp
using Telerik.Reporting;
using Telerik.Reporting.Processing;

// Load report definition
var reportSource = new TypeReportSource();
reportSource.TypeName = typeof(InvoiceReport).AssemblyQualifiedName;

// Set parameters
reportSource.Parameters.Add("InvoiceNumber", "INV-001");
reportSource.Parameters.Add("CustomerName", "Acme Corp");

// Render to PDF
var reportProcessor = new ReportProcessor();
var result = reportProcessor.RenderReport("PDF", reportSource, null);

// Save
using (FileStream fs = new FileStream("invoice.pdf", FileMode.Create))
{
    fs.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
}
```

**IronPDF:**
```csharp
using IronPdf;

var invoiceNumber = "INV-001";
var customerName = "Acme Corp";

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        h1 {{ color: navy; }}
        .invoice-header {{ margin-bottom: 30px; }}
        .field {{ margin: 10px 0; }}
        .label {{ font-weight: bold; display: inline-block; width: 120px; }}
    </style>
</head>
<body>
    <div class='invoice-header'>
        <h1>Invoice</h1>
        <div class='field'><span class='label'>Invoice #:</span> {invoiceNumber}</div>
        <div class='field'><span class='label'>Customer:</span> {customerName}</div>
        <div class='field'><span class='label'>Date:</span> {DateTime.Now:yyyy-MM-dd}</div>
    </div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

### Example 2: Data-Driven Report with Table

**Telerik Reporting:**
```csharp
using Telerik.Reporting;
using Telerik.Reporting.Processing;

// Create report programmatically
var report = new Report();
report.Name = "SalesReport";

// Configure data source
var sqlDataSource = new SqlDataSource();
sqlDataSource.ConnectionString = connectionString;
sqlDataSource.SelectCommand = "SELECT * FROM Orders WHERE Year = @Year";
sqlDataSource.Parameters.Add("@Year", DbType.Int32, 2024);
report.DataSource = sqlDataSource;

// Create table
var table = new Table();
table.DataSource = report.DataSource;

// Configure columns and bindings (verbose setup)
// ... many lines of column configuration ...

report.Items.Add(table);

var reportProcessor = new ReportProcessor();
var result = reportProcessor.RenderReport("PDF", report, null);
```

**IronPDF:**
```csharp
using IronPdf;
using System.Data.SqlClient;

// Get data
var orders = new List<Order>();
using (var conn = new SqlConnection(connectionString))
{
    var cmd = new SqlCommand("SELECT * FROM Orders WHERE Year = @Year", conn);
    cmd.Parameters.AddWithValue("@Year", 2024);
    conn.Open();

    using (var reader = cmd.ExecuteReader())
    {
        while (reader.Read())
        {
            orders.Add(new Order
            {
                Id = reader.GetInt32(0),
                Customer = reader.GetString(1),
                Amount = reader.GetDecimal(2),
                Date = reader.GetDateTime(3)
            });
        }
    }
}

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        h1 {{ color: navy; margin-bottom: 30px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{ background: #f0f0f0; padding: 12px; text-align: left; border: 1px solid #ddd; }}
        td {{ padding: 10px; border: 1px solid #eee; }}
        tr:nth-child(even) {{ background: #f9f9f9; }}
        .amount {{ text-align: right; }}
        .total {{ font-weight: bold; background: #e8e8e8; }}
    </style>
</head>
<body>
    <h1>Sales Report - 2024</h1>
    <table>
        <thead>
            <tr>
                <th>Order ID</th>
                <th>Customer</th>
                <th>Date</th>
                <th>Amount</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", orders.Select(o => $@"
            <tr>
                <td>{o.Id}</td>
                <td>{o.Customer}</td>
                <td>{o.Date:yyyy-MM-dd}</td>
                <td class='amount'>{o.Amount:C}</td>
            </tr>"))}
            <tr class='total'>
                <td colspan='3'>Total</td>
                <td class='amount'>{orders.Sum(o => o.Amount):C}</td>
            </tr>
        </tbody>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("sales_report.pdf");
```

### Example 3: Headers and Footers with Page Numbers

**Telerik Reporting:**
```csharp
using Telerik.Reporting;
using Telerik.Reporting.Drawing;

var report = new Report();

// Page header
var pageHeader = new PageHeaderSection();
pageHeader.Height = new Unit(0.5, UnitType.Inch);

var headerText = new TextBox();
headerText.Value = "Company Report - Confidential";
headerText.Style.Font.Size = new Unit(10, UnitType.Point);
headerText.Style.Color = Color.Gray;
pageHeader.Items.Add(headerText);
report.Items.Add(pageHeader);

// Page footer with page numbers
var pageFooter = new PageFooterSection();
pageFooter.Height = new Unit(0.5, UnitType.Inch);

var pageNumber = new TextBox();
pageNumber.Value = "= 'Page ' + PageNumber + ' of ' + PageCount";
pageNumber.Style.TextAlign = HorizontalAlign.Right;
pageFooter.Items.Add(pageNumber);
report.Items.Add(pageFooter);

// Process and render
var processor = new ReportProcessor();
var result = processor.RenderReport("PDF", report, null);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// HTML Header
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; font-size: 10pt; color: gray; border-bottom: 1px solid #ccc; padding-bottom: 5px;'>
            Company Report - Confidential
        </div>",
    MaxHeight = 30
};

// HTML Footer with page numbers
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; font-size: 10pt; color: gray; text-align: right; border-top: 1px solid #ccc; padding-top: 5px;'>
            Page {page} of {total-pages}
        </div>",
    MaxHeight = 30
};

// Margins for header/footer space
renderer.RenderingOptions.MarginTop = 40;
renderer.RenderingOptions.MarginBottom = 40;

var pdf = renderer.RenderHtmlAsPdf(reportContent);
pdf.SaveAs("report.pdf");
```

### Example 4: Subreports / Nested Reports

**Telerik Reporting:**
```csharp
using Telerik.Reporting;

var mainReport = new Report();

// Create subreport
var subReport = new SubReport();
subReport.ReportSource = new TypeReportSource
{
    TypeName = typeof(DetailReport).AssemblyQualifiedName
};
subReport.Parameters.Add("ParentId", "= Fields.Id");

mainReport.Items.Add(subReport);

var processor = new ReportProcessor();
var result = processor.RenderReport("PDF", mainReport, null);
```

**IronPDF:**
```csharp
using IronPdf;

// Option 1: Embed detail sections in main HTML
var mainHtml = $@"
<h1>Master Report</h1>
{string.Join("", masterRecords.Select(m => $@"
    <div class='section'>
        <h2>{m.Name}</h2>
        <table>
            {string.Join("", m.Details.Select(d => $"<tr><td>{d.Item}</td><td>{d.Value}</td></tr>"))}
        </table>
    </div>
    <div style='page-break-after: always;'></div>
"))}";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(mainHtml);
pdf.SaveAs("report.pdf");

// Option 2: Merge separate PDFs
var coverPdf = renderer.RenderHtmlAsPdf(coverHtml);
var contentPdf = renderer.RenderHtmlAsPdf(contentHtml);
var appendixPdf = renderer.RenderHtmlAsPdf(appendixHtml);

var merged = PdfDocument.Merge(coverPdf, contentPdf, appendixPdf);
merged.SaveAs("complete_report.pdf");
```

### Example 5: Charts and Visualizations

**Telerik Reporting:**
```csharp
using Telerik.Reporting;

var report = new Report();

// Create chart
var chart = new Chart();
chart.ChartType = ChartTypes.Bar;
chart.DataSource = GetChartData();
chart.Series.Add(new ChartSeries
{
    DataFieldY = "Value",
    DataFieldX = "Category"
});

report.Items.Add(chart);

var processor = new ReportProcessor();
var result = processor.RenderReport("PDF", report, null);
```

**IronPDF (using Chart.js):**
```csharp
using IronPdf;

var chartData = GetChartData();

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        .chart-container {{ width: 600px; height: 400px; margin: 0 auto; }}
    </style>
</head>
<body>
    <h1>Sales by Category</h1>
    <div class='chart-container'>
        <canvas id='chart'></canvas>
    </div>
    <script>
        new Chart(document.getElementById('chart'), {{
            type: 'bar',
            data: {{
                labels: [{string.Join(",", chartData.Select(d => $"'{d.Category}'"))}],
                datasets: [{{
                    label: 'Value',
                    data: [{string.Join(",", chartData.Select(d => d.Value))}],
                    backgroundColor: 'rgba(54, 162, 235, 0.8)'
                }}]
            }},
            options: {{
                responsive: true,
                maintainAspectRatio: true
            }}
        }});
    </script>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.JavaScript(2000);

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("chart_report.pdf");
```

### Example 6: ASP.NET Core Web API Endpoint

**Telerik Reporting:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    [HttpGet("invoice/{id}")]
    public IActionResult GetInvoice(int id)
    {
        var reportSource = new TypeReportSource();
        reportSource.TypeName = typeof(InvoiceReport).AssemblyQualifiedName;
        reportSource.Parameters.Add("InvoiceId", id);

        var reportProcessor = new ReportProcessor();
        var result = reportProcessor.RenderReport("PDF", reportSource, null);

        return File(result.DocumentBytes, "application/pdf", $"invoice_{id}.pdf");
    }
}
```

**IronPDF:**
```csharp
using Microsoft.AspNetCore.Mvc;
using IronPdf;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public ReportsController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet("invoice/{id}")]
    public IActionResult GetInvoice(int id)
    {
        var invoice = _invoiceService.GetInvoice(id);

        var html = GenerateInvoiceHtml(invoice);

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);

        return File(pdf.BinaryData, "application/pdf", $"invoice_{id}.pdf");
    }

    private string GenerateInvoiceHtml(Invoice invoice)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial; padding: 40px; }}
                h1 {{ color: navy; }}
                table {{ width: 100%; border-collapse: collapse; }}
                th, td {{ padding: 10px; border: 1px solid #ddd; }}
            </style>
        </head>
        <body>
            <h1>Invoice #{invoice.Number}</h1>
            <p><strong>Customer:</strong> {invoice.CustomerName}</p>
            <p><strong>Date:</strong> {invoice.Date:yyyy-MM-dd}</p>

            <table>
                <tr><th>Item</th><th>Qty</th><th>Price</th><th>Total</th></tr>
                {string.Join("", invoice.Items.Select(i => $@"
                <tr>
                    <td>{i.Name}</td>
                    <td>{i.Quantity}</td>
                    <td>{i.Price:C}</td>
                    <td>{i.Total:C}</td>
                </tr>"))}
            </table>

            <p style='text-align:right; font-size:18px; margin-top:20px;'>
                <strong>Grand Total: {invoice.Total:C}</strong>
            </p>
        </body>
        </html>";
    }
}
```

### Example 7: Razor Template Integration

**Telerik Reporting:** Uses proprietary report designer

**IronPDF with Razor:**
```csharp
using IronPdf;
using RazorLight;

// Setup Razor engine
var engine = new RazorLightEngineBuilder()
    .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates"))
    .UseMemoryCachingProvider()
    .Build();

// Render template with model
var model = new InvoiceViewModel
{
    Number = "INV-001",
    CustomerName = "Acme Corp",
    Items = GetInvoiceItems(),
    Total = 1500.00m
};

string html = await engine.CompileRenderAsync("Invoice.cshtml", model);

// Convert to PDF
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

**Templates/Invoice.cshtml:**
```html
@model InvoiceViewModel
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial; padding: 40px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 10px; border: 1px solid #ddd; }
    </style>
</head>
<body>
    <h1>Invoice #@Model.Number</h1>
    <p><strong>Customer:</strong> @Model.CustomerName</p>

    <table>
        <tr><th>Item</th><th>Qty</th><th>Price</th><th>Total</th></tr>
        @foreach (var item in Model.Items)
        {
            <tr>
                <td>@item.Name</td>
                <td>@item.Quantity</td>
                <td>@item.Price.ToString("C")</td>
                <td>@item.Total.ToString("C")</td>
            </tr>
        }
    </table>

    <p style="text-align:right; font-size:18px;">
        <strong>Total: @Model.Total.ToString("C")</strong>
    </p>
</body>
</html>
```

---

## Feature Comparison

| Feature | Telerik Reporting | IronPDF |
|---------|-------------------|---------|
| **Licensing** | | |
| Cost Model | DevCraft bundle or standalone | Per-developer |
| Annual Fee | Required for updates | Optional |
| **Development** | | |
| Visual Designer | Yes | Use HTML editors |
| Template Format | `.trdp` / `.trdx` | HTML/CSS/Razor |
| Learning Curve | Telerik-specific | Standard web |
| **Rendering** | | |
| HTML to PDF | Limited | Full Chromium |
| URL to PDF | No | Yes |
| CSS Support | Limited | Full CSS3 |
| JavaScript | No | Full ES2024 |
| Charts | Built-in | Via JS libraries |
| **Output** | | |
| PDF Export | Yes | Native |
| Other Formats | Excel, Word, etc. | PDF-focused |
| **Infrastructure** | | |
| Report Server | Optional | Not needed |
| Runtime Size | Large | Smaller |
| Dependencies | Many | Minimal |
| **PDF Features** | | |
| Merge PDFs | Limited | Built-in |
| Split PDFs | Limited | Built-in |
| Watermarks | Via designer | HTML/CSS |
| Digital Signatures | No | Yes |
| PDF/A | No | Yes |

---

## Common Migration Issues

### Issue 1: Report Definitions (.trdp/.trdx files)

**Telerik:** Uses proprietary XML report definitions.

**Solution:** Convert to HTML templates:
1. Open report in designer
2. Note layout, data bindings, formatting
3. Recreate as HTML/CSS template
4. Use Razor for data binding

### Issue 2: Data Source Binding

**Telerik:** Uses SqlDataSource and object data sources with expression binding.

**Solution:** Fetch data in C#, bind to HTML:
```csharp
var data = await dbContext.Orders.ToListAsync();
var html = $"<table>{string.Join("", data.Select(d => $"<tr><td>{d.Name}</td></tr>"))}</table>";
```

### Issue 3: Report Parameters

**Telerik:** `ReportParameter` with built-in parameter UI.

**Solution:** Pass parameters to HTML generation:
```csharp
public string GenerateReport(string customerId, DateTime fromDate)
{
    return $"<h1>Report for {customerId}</h1><p>From: {fromDate:d}</p>";
}
```

### Issue 4: Interactive Features

**Telerik:** Drill-down, sorting, filtering in viewer.

**Solution:** IronPDF generates static PDFs. For interactivity, keep data in web UI, generate PDF when user clicks "Export".

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Telerik reports**
  ```bash
  grep -r "using Telerik.Reporting" --include="*.cs" .
  grep -r "Report\|ReportProcessor" --include="*.cs" .
  ```
  **Why:** Identify all Telerik report usages to ensure complete migration coverage.

- [ ] **Document data sources and parameters**
  ```csharp
  // Find patterns like:
  var report = new Report();
  report.DataSource = myDataSource;
  report.Parameters.Add(new ReportParameter("ParameterName", value));
  ```
  **Why:** These data sources and parameters will need to be mapped to IronPDF's HTML templates with string interpolation or Razor.

- [ ] **Screenshot current report layouts**
  **Why:** Visual reference for ensuring the new PDF output matches the original design.

- [ ] **Identify shared report components**
  **Why:** Determine common elements that can be reused in IronPDF templates to maintain consistency and reduce duplication.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Remove Telerik NuGet packages**
  ```bash
  dotnet remove package Telerik.Reporting
  ```
  **Why:** Clean package removal to switch to IronPDF.

- [ ] **Install IronPdf package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project for PDF generation capabilities.

- [ ] **Convert .trdp/.trdx to HTML templates**
  ```html
  <!-- Before (.trdp/.trdx) -->
  <!-- Telerik report definition -->

  <!-- After (IronPDF) -->
  <html>
  <body>
    <!-- HTML/CSS template -->
  </body>
  </html>
  ```
  **Why:** IronPDF uses HTML/CSS for templates, allowing for more flexibility and ease of editing.

- [ ] **Replace ReportProcessor with ChromePdfRenderer**
  ```csharp
  // Before (Telerik)
  var processor = new ReportProcessor();
  var result = processor.RenderReport("PDF", reportSource, null);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(htmlContent);
  ```
  **Why:** ChromePdfRenderer provides modern PDF rendering with full HTML/CSS support.

- [ ] **Update data binding to string interpolation/Razor**
  ```csharp
  // Before (Telerik)
  report.Parameters["ParameterName"].Value = value;

  // After (IronPDF)
  var htmlContent = $"<p>Value: {value}</p>";
  ```
  **Why:** Use C# string interpolation or Razor syntax for dynamic content in HTML templates.

- [ ] **Convert headers/footers to HTML**
  ```csharp
  // Before (Telerik)
  report.PageSettings.Header = "Header text";
  report.PageSettings.Footer = "Footer text";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Header text</div>",
      MaxHeight = 25
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Footer text</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders for dynamic content.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Testing

- [ ] **Compare PDF output visually**
  **Why:** Ensure the new PDF output matches the original design and layout.

- [ ] **Verify data accuracy**
  **Why:** Confirm that all data is correctly rendered in the new PDF format.

- [ ] **Test pagination**
  **Why:** Check that pagination is handled correctly in the new PDF output.

- [ ] **Check headers/footers on all pages**
  **Why:** Ensure consistent headers and footers across all pages in the PDF.

- [ ] **Performance testing**
  **Why:** Verify that PDF generation performance meets requirements and identify any potential bottlenecks.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [Razor Template Integration](https://ironpdf.com/how-to/razor-to-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
