# How Do I Migrate from SQL Server Reporting Services (SSRS) to IronPDF in C#?

## Why Migrate from SSRS?

SQL Server Reporting Services (SSRS) is Microsoft's enterprise reporting platform that requires significant infrastructure investment. Key reasons to migrate:

1. **Heavy Infrastructure**: Requires SQL Server, Report Server, and IIS configuration
2. **Microsoft Ecosystem Lock-in**: Tied to SQL Server licensing and Windows Server
3. **Complex Deployment**: Report deployment, security configuration, and subscription management
4. **Expensive Licensing**: SQL Server licenses, especially for enterprise features
5. **Limited Web Support**: Difficult to integrate with modern SPA frameworks
6. **Maintenance Overhead**: Server patching, database maintenance, report management
7. **No Cloud-Native Option**: Designed for on-premises, cloud support is awkward

### When SSRS is Overkill

For many PDF generation scenarios, SSRS infrastructure is excessive:

| Your Need | SSRS Overhead |
|-----------|---------------|
| Generate invoices | Full report server |
| Export data tables | SQL Server license |
| Create PDFs from data | Windows Server |
| Simple document generation | Report subscriptions |

IronPDF provides in-process PDF generation without any server infrastructure, as demonstrated throughout the [step-by-step migration reference](https://ironpdf.com/blog/migration-guides/migrate-from-ssrs-to-ironpdf/).

---

## Quick Start: SSRS to IronPDF

### Step 1: Replace Dependencies

```bash
# SSRS has no direct NuGet - it's server-based
# Remove any ReportViewer controls

dotnet remove package Microsoft.ReportingServices.ReportViewerControl.WebForms
dotnet remove package Microsoft.ReportingServices.ReportViewerControl.WinForms

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using Microsoft.Reporting.WebForms;
using Microsoft.Reporting.WinForms;

// After
using IronPdf;
```

### Step 3: Initialize License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## API Mapping Reference

| SSRS Concept | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `LocalReport` | `ChromePdfRenderer` | Core rendering |
| `ServerReport` | `RenderUrlAsPdf()` | URL-based rendering |
| `.rdlc` files | HTML/CSS templates | Template format |
| `ReportParameter` | String interpolation | Parameters |
| `ReportDataSource` | C# data + HTML | Data binding |
| `LocalReport.Render("PDF")` | `RenderHtmlAsPdf()` | PDF output |
| `SubReport` | Merged PDFs | Nested reports |
| `Report Server URL` | Not needed | No server required |
| `ReportViewer` control | Not needed | Direct PDF generation |
| Export formats | PDF is native | Focused output |

---

## Code Examples

### Example 1: Basic Report Generation (C#)

**SSRS with LocalReport:**
```csharp
using Microsoft.Reporting.WebForms;
using System.Data;

// Load RDLC report
LocalReport report = new LocalReport();
report.ReportPath = Server.MapPath("~/Reports/Invoice.rdlc");

// Set data source
DataTable invoiceData = GetInvoiceData(invoiceId);
ReportDataSource rds = new ReportDataSource("InvoiceDataSet", invoiceData);
report.DataSources.Add(rds);

// Set parameters
ReportParameter[] parameters = new ReportParameter[]
{
    new ReportParameter("InvoiceNumber", "INV-001"),
    new ReportParameter("CustomerName", "Acme Corp"),
    new ReportParameter("InvoiceDate", DateTime.Now.ToString("yyyy-MM-dd"))
};
report.SetParameters(parameters);

// Render to PDF
string mimeType, encoding, fileNameExtension;
string[] streams;
Warning[] warnings;

byte[] pdfBytes = report.Render(
    "PDF",
    null,
    out mimeType,
    out encoding,
    out fileNameExtension,
    out streams,
    out warnings);

// Save or return
File.WriteAllBytes("invoice.pdf", pdfBytes);
```

**IronPDF:**
```csharp
using IronPdf;

// Get data
var invoice = GetInvoice(invoiceId);

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        h1 {{ color: navy; border-bottom: 2px solid navy; padding-bottom: 10px; }}
        .info {{ margin: 20px 0; }}
        .info p {{ margin: 5px 0; }}
        .label {{ font-weight: bold; display: inline-block; width: 120px; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 30px; }}
        th {{ background: #f0f0f0; padding: 12px; text-align: left; border: 1px solid #ddd; }}
        td {{ padding: 10px; border: 1px solid #eee; }}
        .amount {{ text-align: right; }}
        .total {{ font-weight: bold; font-size: 18px; }}
    </style>
</head>
<body>
    <h1>INVOICE</h1>

    <div class='info'>
        <p><span class='label'>Invoice #:</span> {invoice.Number}</p>
        <p><span class='label'>Customer:</span> {invoice.CustomerName}</p>
        <p><span class='label'>Date:</span> {invoice.Date:yyyy-MM-dd}</p>
    </div>

    <table>
        <thead>
            <tr><th>Item</th><th>Qty</th><th>Price</th><th>Total</th></tr>
        </thead>
        <tbody>
            {string.Join("", invoice.Items.Select(i => $@"
            <tr>
                <td>{i.Description}</td>
                <td class='amount'>{i.Quantity}</td>
                <td class='amount'>{i.UnitPrice:C}</td>
                <td class='amount'>{i.Total:C}</td>
            </tr>"))}
        </tbody>
        <tfoot>
            <tr class='total'>
                <td colspan='3'>Grand Total</td>
                <td class='amount'>{invoice.GrandTotal:C}</td>
            </tr>
        </tfoot>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

### Example 2: Database-Driven Report

**SSRS:**
```csharp
using Microsoft.Reporting.WebForms;
using System.Data.SqlClient;

LocalReport report = new LocalReport();
report.ReportPath = "Reports/SalesReport.rdlc";

// Execute query and fill dataset
using (SqlConnection conn = new SqlConnection(connectionString))
{
    SqlCommand cmd = new SqlCommand(@"
        SELECT ProductName, Category, SUM(Quantity) as TotalQty, SUM(Amount) as TotalAmount
        FROM Sales
        WHERE YEAR(SaleDate) = @Year
        GROUP BY ProductName, Category
        ORDER BY TotalAmount DESC", conn);

    cmd.Parameters.AddWithValue("@Year", 2024);

    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
    DataTable dt = new DataTable();
    adapter.Fill(dt);

    report.DataSources.Add(new ReportDataSource("SalesDataSet", dt));
}

report.SetParameters(new ReportParameter("ReportYear", "2024"));

byte[] pdfBytes = report.Render("PDF");
Response.BinaryWrite(pdfBytes);
```

**IronPDF:**
```csharp
using IronPdf;
using Microsoft.Data.SqlClient;

// Get data
var salesData = new List<SalesItem>();
using (var conn = new SqlConnection(connectionString))
{
    var cmd = new SqlCommand(@"
        SELECT ProductName, Category, SUM(Quantity) as TotalQty, SUM(Amount) as TotalAmount
        FROM Sales
        WHERE YEAR(SaleDate) = @Year
        GROUP BY ProductName, Category
        ORDER BY TotalAmount DESC", conn);

    cmd.Parameters.AddWithValue("@Year", 2024);
    conn.Open();

    using (var reader = cmd.ExecuteReader())
    {
        while (reader.Read())
        {
            salesData.Add(new SalesItem
            {
                ProductName = reader.GetString(0),
                Category = reader.GetString(1),
                TotalQty = reader.GetInt32(2),
                TotalAmount = reader.GetDecimal(3)
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
        h1 {{ color: #333; }}
        .subtitle {{ color: #666; margin-bottom: 30px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{ background: #2c3e50; color: white; padding: 12px; text-align: left; }}
        td {{ padding: 10px; border-bottom: 1px solid #eee; }}
        tr:nth-child(even) {{ background: #f9f9f9; }}
        .number {{ text-align: right; }}
        .category {{ color: #666; font-size: 12px; }}
        .summary {{ margin-top: 20px; padding: 15px; background: #f0f0f0; }}
    </style>
</head>
<body>
    <h1>Sales Report</h1>
    <p class='subtitle'>Year: 2024</p>

    <table>
        <thead>
            <tr>
                <th>Product</th>
                <th>Category</th>
                <th>Quantity</th>
                <th>Total Amount</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", salesData.Select(s => $@"
            <tr>
                <td>{s.ProductName}</td>
                <td class='category'>{s.Category}</td>
                <td class='number'>{s.TotalQty:N0}</td>
                <td class='number'>{s.TotalAmount:C}</td>
            </tr>"))}
        </tbody>
    </table>

    <div class='summary'>
        <strong>Total Revenue:</strong> {salesData.Sum(s => s.TotalAmount):C}
    </div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);

// Return in web response
return File(pdf.BinaryData, "application/pdf", "SalesReport_2024.pdf");
```

### Example 3: Headers and Footers with Page Numbers

**SSRS:** Configured in .rdlc designer with expressions like `=Globals!PageNumber`

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// HTML Header with logo
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; padding: 10px 0; border-bottom: 2px solid #2c3e50;'>
            <table style='width: 100%;'>
                <tr>
                    <td style='width: 50%;'>
                        <img src='logo.png' style='height: 40px;'>
                    </td>
                    <td style='width: 50%; text-align: right; color: #666; font-size: 10pt;'>
                        Sales Report - {date}
                    </td>
                </tr>
            </table>
        </div>",
    MaxHeight = 50,
    BaseUrl = new Uri("file:///C:/images/")
};

// HTML Footer with page numbers
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; padding: 10px 0; border-top: 1px solid #ccc; font-size: 9pt; color: #666;'>
            <table style='width: 100%;'>
                <tr>
                    <td style='width: 50%;'>Confidential - Internal Use Only</td>
                    <td style='width: 50%; text-align: right;'>Page {page} of {total-pages}</td>
                </tr>
            </table>
        </div>",
    MaxHeight = 40
};

renderer.RenderingOptions.MarginTop = 60;
renderer.RenderingOptions.MarginBottom = 50;

var pdf = renderer.RenderHtmlAsPdf(reportHtml);
pdf.SaveAs("report.pdf");
```

### Example 4: Subreports / Master-Detail Reports

**SSRS:** Uses SubReport control with parameters

**IronPDF:**
```csharp
using IronPdf;

// Option 1: Generate all in one HTML with nested data
var customers = GetCustomersWithOrders();

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial; padding: 30px; }}
        .customer {{ margin-bottom: 40px; }}
        .customer-header {{ background: #2c3e50; color: white; padding: 15px; }}
        .orders-table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
        .orders-table th, .orders-table td {{ padding: 8px; border: 1px solid #ddd; }}
        .page-break {{ page-break-after: always; }}
    </style>
</head>
<body>
    {string.Join("", customers.Select((c, idx) => $@"
    <div class='customer {(idx < customers.Count - 1 ? "page-break" : "")}'>
        <div class='customer-header'>
            <h2>{c.Name}</h2>
            <p>Customer ID: {c.Id} | Total Orders: {c.Orders.Count}</p>
        </div>

        <table class='orders-table'>
            <tr><th>Order #</th><th>Date</th><th>Items</th><th>Amount</th></tr>
            {string.Join("", c.Orders.Select(o => $@"
            <tr>
                <td>{o.OrderNumber}</td>
                <td>{o.Date:yyyy-MM-dd}</td>
                <td>{o.ItemCount}</td>
                <td style='text-align:right;'>{o.Amount:C}</td>
            </tr>"))}
        </table>

        <p style='text-align:right; font-weight:bold;'>
            Customer Total: {c.Orders.Sum(o => o.Amount):C}
        </p>
    </div>"))}
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("customer_orders.pdf");

// Option 2: Merge separate PDFs
var coverPdf = renderer.RenderHtmlAsPdf(coverHtml);
var summaryPdf = renderer.RenderHtmlAsPdf(summaryHtml);
var detailPdf = renderer.RenderHtmlAsPdf(detailHtml);

var merged = PdfDocument.Merge(coverPdf, summaryPdf, detailPdf);
merged.SaveAs("complete_report.pdf");
```

### Example 5: Grouping and Aggregation

**SSRS:** Uses group expressions in table/matrix controls

**IronPDF:**
```csharp
using IronPdf;

var salesByRegion = GetSales()
    .GroupBy(s => s.Region)
    .Select(g => new
    {
        Region = g.Key,
        Products = g.GroupBy(p => p.Product)
                    .Select(pg => new { Product = pg.Key, Total = pg.Sum(x => x.Amount) }),
        RegionTotal = g.Sum(x => x.Amount)
    });

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial; padding: 30px; }}
        .region {{ margin-bottom: 30px; }}
        .region-header {{ background: #34495e; color: white; padding: 10px 15px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th, td {{ padding: 10px; text-align: left; border: 1px solid #ddd; }}
        .subtotal {{ background: #ecf0f1; font-weight: bold; }}
        .grand-total {{ background: #2c3e50; color: white; font-size: 18px; }}
    </style>
</head>
<body>
    <h1>Sales by Region</h1>

    {string.Join("", salesByRegion.Select(r => $@"
    <div class='region'>
        <div class='region-header'>
            <h2>{r.Region}</h2>
        </div>
        <table>
            <tr><th>Product</th><th style='text-align:right;'>Total Sales</th></tr>
            {string.Join("", r.Products.Select(p => $@"
            <tr>
                <td>{p.Product}</td>
                <td style='text-align:right;'>{p.Total:C}</td>
            </tr>"))}
            <tr class='subtotal'>
                <td>Region Subtotal</td>
                <td style='text-align:right;'>{r.RegionTotal:C}</td>
            </tr>
        </table>
    </div>"))}

    <table>
        <tr class='grand-total'>
            <td>GRAND TOTAL</td>
            <td style='text-align:right;'>{salesByRegion.Sum(r => r.RegionTotal):C}</td>
        </tr>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("sales_by_region.pdf");
```

### Example 6: Charts and Visualizations

**SSRS:** Built-in chart control

**IronPDF with Chart.js:**
```csharp
using IronPdf;

var chartData = GetMonthlySales();

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
    <style>
        body {{ font-family: Arial; padding: 30px; }}
        .chart-container {{ width: 100%; max-width: 700px; margin: 30px auto; }}
    </style>
</head>
<body>
    <h1>Monthly Sales Trend</h1>

    <div class='chart-container'>
        <canvas id='salesChart'></canvas>
    </div>

    <script>
        new Chart(document.getElementById('salesChart'), {{
            type: 'line',
            data: {{
                labels: [{string.Join(",", chartData.Select(d => $"'{d.Month}'"))}],
                datasets: [{{
                    label: 'Sales ($)',
                    data: [{string.Join(",", chartData.Select(d => d.Amount))}],
                    borderColor: 'rgb(75, 192, 192)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    fill: true,
                    tension: 0.4
                }}]
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ position: 'bottom' }}
                }},
                scales: {{
                    y: {{ beginAtZero: true }}
                }}
            }}
        }});
    </script>

    <table style='width:100%; margin-top:30px; border-collapse:collapse;'>
        <tr style='background:#f0f0f0;'>
            <th style='padding:10px; border:1px solid #ddd;'>Month</th>
            <th style='padding:10px; border:1px solid #ddd; text-align:right;'>Amount</th>
        </tr>
        {string.Join("", chartData.Select(d => $@"
        <tr>
            <td style='padding:10px; border:1px solid #eee;'>{d.Month}</td>
            <td style='padding:10px; border:1px solid #eee; text-align:right;'>{d.Amount:C}</td>
        </tr>"))}
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.JavaScript(2000);

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("sales_chart.pdf");
```

### Example 7: ASP.NET Core Integration

**SSRS:** Requires ReportViewer control or REST API calls to Report Server

**IronPDF:**
```csharp
using Microsoft.AspNetCore.Mvc;
using IronPdf;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales/{year}")]
    public async Task<IActionResult> GetSalesReport(int year)
    {
        var data = await _reportService.GetSalesDataAsync(year);
        var html = GenerateSalesReportHtml(data, year);

        var renderer = new ChromePdfRenderer();
        ConfigureRenderer(renderer);

        var pdf = renderer.RenderHtmlAsPdf(html);

        return File(pdf.BinaryData, "application/pdf", $"SalesReport_{year}.pdf");
    }

    [HttpGet("invoice/{id}")]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var invoice = await _reportService.GetInvoiceAsync(id);
        var html = GenerateInvoiceHtml(invoice);

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);

        return File(pdf.BinaryData, "application/pdf", $"Invoice_{invoice.Number}.pdf");
    }

    private void ConfigureRenderer(ChromePdfRenderer renderer)
    {
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 30;
        renderer.RenderingOptions.MarginBottom = 30;

        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center; font-size:9pt; color:#666;'>Page {page} of {total-pages}</div>",
            MaxHeight = 25
        };
    }
}
```

### Example 8: Razor View Templates

**SSRS:** Uses .rdlc designer

**IronPDF with Razor:**
```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public class PdfService
{
    private readonly ICompositeViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;

    public PdfService(ICompositeViewEngine viewEngine, ITempDataProvider tempDataProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
    }

    public async Task<byte[]> GeneratePdfFromViewAsync<TModel>(
        ControllerContext context,
        string viewName,
        TModel model)
    {
        // Render Razor view to HTML
        var html = await RenderViewToStringAsync(context, viewName, model);

        // Convert to PDF
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);

        return pdf.BinaryData;
    }

    private async Task<string> RenderViewToStringAsync<TModel>(
        ControllerContext context,
        string viewName,
        TModel model)
    {
        var viewResult = _viewEngine.FindView(context, viewName, false);

        using (var writer = new StringWriter())
        {
            var viewContext = new ViewContext(
                context,
                viewResult.View,
                new ViewDataDictionary<TModel>(
                    new EmptyModelMetadataProvider(),
                    new ModelStateDictionary()) { Model = model },
                new TempDataDictionary(context.HttpContext, _tempDataProvider),
                writer,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);

            return writer.ToString();
        }
    }
}
```

**Views/Reports/Invoice.cshtml:**
```html
@model InvoiceViewModel
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial; padding: 40px; }
        h1 { color: navy; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        th, td { padding: 10px; border: 1px solid #ddd; }
        th { background: #f0f0f0; }
    </style>
</head>
<body>
    <h1>Invoice #@Model.Number</h1>
    <p><strong>Customer:</strong> @Model.CustomerName</p>
    <p><strong>Date:</strong> @Model.Date.ToString("yyyy-MM-dd")</p>

    <table>
        <tr>
            <th>Item</th>
            <th>Qty</th>
            <th>Price</th>
            <th>Total</th>
        </tr>
        @foreach (var item in Model.Items)
        {
            <tr>
                <td>@item.Description</td>
                <td style="text-align:right;">@item.Quantity</td>
                <td style="text-align:right;">@item.UnitPrice.ToString("C")</td>
                <td style="text-align:right;">@item.Total.ToString("C")</td>
            </tr>
        }
    </table>

    <p style="text-align:right; font-size:18px; margin-top:20px;">
        <strong>Total: @Model.GrandTotal.ToString("C")</strong>
    </p>
</body>
</html>
```

---

## Feature Comparison

| Feature | SSRS | IronPDF |
|---------|------|---------|
| **Infrastructure** | | |
| Server Required | Yes (Report Server) | No |
| SQL Server License | Required | Not needed |
| Windows Server | Required | Any platform |
| Database Required | Yes (ReportServer DB) | No |
| **Development** | | |
| Visual Designer | Yes (.rdlc) | HTML editors |
| Template Format | RDLC/RDL | HTML/CSS/Razor |
| Data Sources | Built-in DSN | Any C# data |
| **Rendering** | | |
| HTML to PDF | No | Full Chromium |
| URL to PDF | No | Yes |
| CSS Support | Limited | Full CSS3 |
| JavaScript | No | Full ES2024 |
| Charts | Built-in | Via JS libraries |
| **Deployment** | | |
| Report Deployment | To server | With app |
| Configuration | Complex | Simple |
| Maintenance | High | Low |
| **Features** | | |
| Subscriptions | Built-in | Build your own |
| Caching | Built-in | Build your own |
| Security | Integrated | Per-app |
| Multi-format Export | Yes | PDF-focused |

---

## Common Migration Issues

### Issue 1: RDLC Report Definitions

**SSRS:** Uses proprietary .rdlc XML format.

**Solution:** Convert to HTML templates:
1. Open .rdlc in Visual Studio
2. Document the layout structure
3. Recreate in HTML/CSS
4. Use Razor for data binding

### Issue 2: Shared Data Sources

**SSRS:** Connection strings in Report Server.

**Solution:** Use your application's data access layer:
```csharp
var data = await _dbContext.Sales.ToListAsync();
// Then bind to HTML template
```

### Issue 3: Report Parameters UI

**SSRS:** Built-in parameter prompts.

**Solution:** Build parameter UI in your application:
```csharp
// Your own parameter form, then:
var pdf = GenerateReport(startDate, endDate, region);
```

### Issue 4: Subscriptions/Scheduled Reports

**SSRS:** Built-in subscription engine.

**Solution:** Use background job framework:
```csharp
// Using Hangfire or similar
RecurringJob.AddOrUpdate("weekly-report",
    () => GenerateAndEmailReport(), Cron.Weekly);
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all SSRS reports**
  ```bash
  grep -r "<Report" --include="*.rdlc" .
  ```
  **Why:** Identify all reports to ensure complete migration coverage.

- [ ] **Document data sources and connections**
  ```csharp
  // Before (SSRS)
  var dataSource = new ReportDataSource("DataSourceName", data);

  // After (IronPDF)
  var htmlTemplate = "<html><body><!-- Data binding logic here --></body></html>";
  ```
  **Why:** These data sources map to C# data binding in HTML templates for IronPDF.

- [ ] **Screenshot report layouts**
  **Why:** Preserve the visual design of reports for accurate HTML template creation.

- [ ] **List report parameters**
  ```csharp
  // Before (SSRS)
  var parameters = new ReportParameter("ParameterName", "Value");

  // After (IronPDF)
  var htmlTemplate = "<html><body>Parameter: {{ParameterName}}</body></html>";
  ```
  **Why:** Parameters will be interpolated in HTML templates.

- [ ] **Note subscription schedules**
  **Why:** Ensure report delivery schedules are maintained post-migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Remove ReportViewer packages**
  ```bash
  dotnet remove package Microsoft.ReportViewer
  ```
  **Why:** Clean package removal to switch to IronPDF.

- [ ] **Install IronPdf package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF for in-process PDF generation.

- [ ] **Convert .rdlc to HTML templates**
  ```csharp
  // Before (SSRS)
  var report = new LocalReport();
  report.ReportPath = "Report.rdlc";

  // After (IronPDF)
  var htmlTemplate = "<html><body><!-- HTML content here --></body></html>";
  ```
  **Why:** HTML templates are used for rendering PDFs with IronPDF.

- [ ] **Replace LocalReport with ChromePdfRenderer**
  ```csharp
  // Before (SSRS)
  var report = new LocalReport();
  report.Render("PDF");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(htmlTemplate);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** ChromePdfRenderer provides modern rendering with IronPDF.

- [ ] **Update data binding patterns**
  ```csharp
  // Before (SSRS)
  report.DataSources.Add(new ReportDataSource("DataSourceName", data));

  // After (IronPDF)
  var htmlTemplate = "<html><body><!-- Data binding logic here --></body></html>";
  ```
  **Why:** Data is bound directly in HTML templates for IronPDF.

- [ ] **Implement headers/footers**
  ```csharp
  // Before (SSRS)
  report.SetParameters(new ReportParameter("Header", "Header Text"));

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Header Text</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Infrastructure

- [ ] **Plan Report Server decommission**
  **Why:** Remove unnecessary infrastructure to reduce costs and complexity.

- [ ] **Migrate subscriptions to job scheduler**
  **Why:** Maintain report delivery schedules using a simpler job scheduler.

- [ ] **Update deployment scripts**
  **Why:** Ensure deployment processes are aligned with the new PDF generation approach.

### Testing

- [ ] **Compare PDF output**
  **Why:** Verify that the visual output matches the original SSRS reports.

- [ ] **Verify data accuracy**
  **Why:** Ensure that data binding and rendering are correct in the new PDFs.

- [ ] **Test pagination**
  **Why:** Confirm that pagination is handled correctly in the generated PDFs.

- [ ] **Check all parameters**
  **Why:** Ensure that all report parameters are correctly applied in the new templates.

- [ ] **Performance testing**
  **Why:** Validate that the new PDF generation process meets performance expectations.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [ASP.NET Core Integration](https://ironpdf.com/how-to/aspnet-core-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
