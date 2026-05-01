# How Do I Migrate from SAP Crystal Reports to IronPDF in C#?

## Why Migrate from SAP Crystal Reports?

SAP Crystal Reports, developer version for Microsoft Visual Studio (CR4VS) is a legacy enterprise reporting platform that has become increasingly burdensome for modern .NET development. Key reasons to migrate:

1. **Heavy Runtime Installer**: Crystal Reports Runtime ships as an MSI redistributable (typically tens to a few hundred MB depending on 32/64-bit and locale) rather than a NuGet package
2. **SAP Ecosystem Lock-in**: Tied to SAP's release cycles and product roadmap; SAP confirmed end of mainstream maintenance for CR4VS at end of 2027
3. **Licensing**: CR4VS itself is free to download and the runtime is free to redistribute, but the Crystal Reports designer family (CR 2020 / CR 2025) and BI Platform are commercial SAP SKUs
4. **Legacy Architecture**: COM-dependent, .NET Framework 4.x only — runtime DLLs cannot load under .NET Core / .NET 5+
5. **No .NET Core / .NET 5+ Support**: There is no roadmap for a Crystal Reports runtime targeting modern .NET; you are pinned to .NET Framework 4.x
6. **32-bit Runtime Discontinued**: SAP confirmed the 32-bit CR .NET runtime is discontinued after SP 39 (December 2025); future service packs are 64-bit only
7. **Report Designer Dependency**: Requires the CR4VS Visual Studio extension or a standalone Crystal Reports designer to author `.rpt` files

### The Hidden Costs of Crystal Reports

| Cost Factor | Crystal Reports | IronPDF |
|-------------|-----------------|---------|
| Runtime Distribution | MSI redistributable installer | NuGet package |
| Installation | Runtime installer + VS extension | `dotnet add package IronPdf` |
| Deployment | Run MSI on target machine | xcopy / publish |
| 64-bit Support | Yes (32-bit runtime ends after SP 39, Dec 2025) | Native |
| .NET Core / .NET 5+ | Not supported (Framework 4.x only) | Full support (.NET 6/8/9, Framework 4.6.2+) |
| Linux / Docker | Not supported (Windows-only) | Supported |

---

## Quick Start: Crystal Reports to IronPDF

### Step 1: Remove Crystal Reports

```bash
# Crystal Reports for Visual Studio is not distributed by SAP on NuGet.
# Projects typically reference the GAC-installed CrystalDecisions.*.dll
# assemblies (delivered by the CR4VS / CR runtime MSI). Community-maintained
# NuGet wrappers such as CrystalReports.Engine (publisher: ennerperez) exist
# for build automation but are not official SAP packages.

# 1. Remove direct assembly references (CrystalDecisions.*.dll) from your .csproj
# 2. If using a community NuGet wrapper, remove it:
dotnet remove package CrystalReports.Engine
dotnet remove package CrystalReports.Shared
dotnet remove package CrystalReports.ReportAppServer
dotnet remove package CrystalReports.Web

# 3. Uninstall the Crystal Reports runtime MSI from build/deploy targets

# 4. Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.ReportAppServer;

// After
using IronPdf;
```

### Step 3: Initialize License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## API Mapping Reference

| Crystal Reports | IronPDF | Notes |
|-----------------|---------|-------|
| `ReportDocument` | `ChromePdfRenderer` | Core rendering |
| `ReportDocument.Load()` | `RenderHtmlAsPdf()` | Load content |
| `.rpt` files | HTML/CSS templates | Template format |
| `SetDataSource()` | HTML with data | Data binding |
| `SetParameterValue()` | String interpolation | Parameters |
| `ExportToDisk()` | `pdf.SaveAs()` | Save file |
| `ExportToStream()` | `pdf.BinaryData` | Get bytes |
| `PrintToPrinter()` | `pdf.Print()` | Printing |
| `Database.Tables` | C# data access | Data source |
| `FormulaFieldDefinitions` | C# logic | Calculations |
| `SummaryInfo` | `pdf.MetaData` | PDF metadata |
| `ExportFormatType.PortableDocFormat` | Default output | PDF native |

---

## Code Examples

### Example 1: Basic Report to PDF

**Crystal Reports:**
```csharp
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

// Load report
ReportDocument report = new ReportDocument();
report.Load(Server.MapPath("~/Reports/Invoice.rpt"));

// Set data source
report.SetDataSource(GetInvoiceDataSet());

// Set parameters
report.SetParameterValue("InvoiceNumber", "INV-001");
report.SetParameterValue("CustomerName", "Acme Corp");

// Export to PDF
report.ExportToDisk(ExportFormatType.PortableDocFormat, @"C:\Output\Invoice.pdf");

// Clean up (required!)
report.Close();
report.Dispose();
```

**IronPDF:**
```csharp
using IronPdf;

// Get data
var invoice = GetInvoice("INV-001");

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        h1 {{ color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 15px; }}
        .header-info {{ display: flex; justify-content: space-between; margin: 30px 0; }}
        .info-block p {{ margin: 5px 0; }}
        .label {{ font-weight: bold; color: #666; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 30px; }}
        th {{ background: #3498db; color: white; padding: 12px; text-align: left; }}
        td {{ padding: 10px; border-bottom: 1px solid #eee; }}
        tr:nth-child(even) {{ background: #f9f9f9; }}
        .amount {{ text-align: right; }}
        .total-row {{ background: #2c3e50 !important; color: white; font-size: 16px; }}
    </style>
</head>
<body>
    <h1>INVOICE</h1>

    <div class='header-info'>
        <div class='info-block'>
            <p><span class='label'>Invoice #:</span> {invoice.Number}</p>
            <p><span class='label'>Date:</span> {invoice.Date:yyyy-MM-dd}</p>
            <p><span class='label'>Due Date:</span> {invoice.DueDate:yyyy-MM-dd}</p>
        </div>
        <div class='info-block'>
            <p><span class='label'>Bill To:</span></p>
            <p>{invoice.CustomerName}</p>
            <p>{invoice.CustomerAddress}</p>
        </div>
    </div>

    <table>
        <thead>
            <tr>
                <th>Description</th>
                <th>Qty</th>
                <th>Unit Price</th>
                <th>Total</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", invoice.Items.Select(i => $@"
            <tr>
                <td>{i.Description}</td>
                <td class='amount'>{i.Quantity}</td>
                <td class='amount'>{i.UnitPrice:C}</td>
                <td class='amount'>{i.LineTotal:C}</td>
            </tr>"))}
        </tbody>
        <tfoot>
            <tr>
                <td colspan='3' class='amount'><strong>Subtotal:</strong></td>
                <td class='amount'>{invoice.Subtotal:C}</td>
            </tr>
            <tr>
                <td colspan='3' class='amount'><strong>Tax ({invoice.TaxRate:P0}):</strong></td>
                <td class='amount'>{invoice.TaxAmount:C}</td>
            </tr>
            <tr class='total-row'>
                <td colspan='3' class='amount'><strong>TOTAL:</strong></td>
                <td class='amount'><strong>{invoice.Total:C}</strong></td>
            </tr>
        </tfoot>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs(@"C:\Output\Invoice.pdf");
// No Dispose() needed - IronPDF handles cleanup
```

### Example 2: Database-Connected Report

**Crystal Reports:**
```csharp
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Data;

ReportDocument report = new ReportDocument();
report.Load(@"Reports\SalesReport.rpt");

// Set database logon credentials
foreach (Table table in report.Database.Tables)
{
    TableLogOnInfo logOnInfo = table.LogOnInfo;
    logOnInfo.ConnectionInfo.ServerName = "SQLServer";
    logOnInfo.ConnectionInfo.DatabaseName = "SalesDB";
    logOnInfo.ConnectionInfo.UserID = "user";
    logOnInfo.ConnectionInfo.Password = "password";
    table.ApplyLogOnInfo(logOnInfo);
}

// Set parameters
report.SetParameterValue("StartDate", startDate);
report.SetParameterValue("EndDate", endDate);
report.SetParameterValue("Region", "North");

// Export
Stream stream = report.ExportToStream(ExportFormatType.PortableDocFormat);
Response.ContentType = "application/pdf";
Response.BinaryWrite(((MemoryStream)stream).ToArray());

report.Close();
```

**IronPDF:**
```csharp
using IronPdf;
using Microsoft.Data.SqlClient;

// Get data using your existing data access
var sales = await GetSalesDataAsync(startDate, endDate, "North");

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        h1 {{ color: #2c3e50; }}
        .report-header {{ background: #ecf0f1; padding: 20px; margin-bottom: 30px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{ background: #34495e; color: white; padding: 12px; text-align: left; }}
        td {{ padding: 10px; border-bottom: 1px solid #ddd; }}
        tr:hover {{ background: #f5f5f5; }}
        .number {{ text-align: right; font-family: 'Courier New', monospace; }}
        .summary {{ margin-top: 30px; padding: 20px; background: #3498db; color: white; }}
    </style>
</head>
<body>
    <h1>Sales Report</h1>

    <div class='report-header'>
        <p><strong>Period:</strong> {startDate:MMM d, yyyy} - {endDate:MMM d, yyyy}</p>
        <p><strong>Region:</strong> North</p>
        <p><strong>Generated:</strong> {DateTime.Now:f}</p>
    </div>

    <table>
        <thead>
            <tr>
                <th>Date</th>
                <th>Product</th>
                <th>Customer</th>
                <th>Qty</th>
                <th>Amount</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", sales.Select(s => $@"
            <tr>
                <td>{s.Date:yyyy-MM-dd}</td>
                <td>{s.Product}</td>
                <td>{s.Customer}</td>
                <td class='number'>{s.Quantity}</td>
                <td class='number'>{s.Amount:C}</td>
            </tr>"))}
        </tbody>
    </table>

    <div class='summary'>
        <h2>Summary</h2>
        <p>Total Transactions: {sales.Count}</p>
        <p>Total Units: {sales.Sum(s => s.Quantity):N0}</p>
        <p>Total Revenue: {sales.Sum(s => s.Amount):C}</p>
    </div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);

return File(pdf.BinaryData, "application/pdf", "SalesReport.pdf");
```

### Example 3: Formula Fields and Calculations

**Crystal Reports:** Uses Crystal formula syntax in .rpt file

```
// Crystal Formula Field
{@TotalWithDiscount} = {Orders.Amount} * (1 - {Orders.DiscountRate})
{@ProfitMargin} = ({Orders.Amount} - {Orders.Cost}) / {Orders.Amount} * 100
```

**IronPDF:** Use C# for all calculations

```csharp
using IronPdf;

var orders = GetOrders();

// Perform calculations in C#
var orderData = orders.Select(o => new
{
    o.OrderId,
    o.Product,
    o.Amount,
    o.DiscountRate,
    o.Cost,
    TotalWithDiscount = o.Amount * (1 - o.DiscountRate),
    ProfitMargin = ((o.Amount - o.Cost) / o.Amount) * 100
}).ToList();

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial; padding: 30px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{ background: #2c3e50; color: white; padding: 10px; }}
        td {{ padding: 8px; border: 1px solid #ddd; }}
        .number {{ text-align: right; }}
        .positive {{ color: green; }}
        .negative {{ color: red; }}
    </style>
</head>
<body>
    <h1>Orders with Calculations</h1>
    <table>
        <tr>
            <th>Order ID</th>
            <th>Product</th>
            <th>Amount</th>
            <th>Discount</th>
            <th>Net Amount</th>
            <th>Profit Margin</th>
        </tr>
        {string.Join("", orderData.Select(o => $@"
        <tr>
            <td>{o.OrderId}</td>
            <td>{o.Product}</td>
            <td class='number'>{o.Amount:C}</td>
            <td class='number'>{o.DiscountRate:P0}</td>
            <td class='number'>{o.TotalWithDiscount:C}</td>
            <td class='number {(o.ProfitMargin >= 0 ? "positive" : "negative")}'>{o.ProfitMargin:F1}%</td>
        </tr>"))}
    </table>

    <h3>Totals</h3>
    <p>Total Revenue: {orderData.Sum(o => o.TotalWithDiscount):C}</p>
    <p>Average Profit Margin: {orderData.Average(o => o.ProfitMargin):F1}%</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("orders_with_calculations.pdf");
```

### Example 4: Subreports

**Crystal Reports:**
```csharp
using CrystalDecisions.CrystalReports.Engine;

ReportDocument mainReport = new ReportDocument();
mainReport.Load(@"Reports\MainReport.rpt");

// Subreports are embedded in .rpt file and linked via parameters
mainReport.SetDataSource(GetMainData());

// Set subreport data source
mainReport.Subreports["DetailSubreport.rpt"].SetDataSource(GetDetailData());

mainReport.ExportToDisk(ExportFormatType.PortableDocFormat, "report.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

// Option 1: Single HTML with all data
var customers = GetCustomersWithDetails();

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial; padding: 30px; }}
        .customer {{ margin-bottom: 50px; page-break-inside: avoid; }}
        .customer-header {{ background: #2c3e50; color: white; padding: 15px; }}
        .details {{ margin-left: 20px; }}
        table {{ width: 100%; border-collapse: collapse; margin: 10px 0; }}
        th, td {{ padding: 8px; border: 1px solid #ddd; text-align: left; }}
        th {{ background: #ecf0f1; }}
    </style>
</head>
<body>
    <h1>Customer Report with Details</h1>

    {string.Join("", customers.Select(c => $@"
    <div class='customer'>
        <div class='customer-header'>
            <h2>{c.Name}</h2>
            <p>Customer ID: {c.Id} | Since: {c.JoinDate:yyyy}</p>
        </div>

        <div class='details'>
            <h3>Recent Orders</h3>
            <table>
                <tr><th>Order #</th><th>Date</th><th>Amount</th><th>Status</th></tr>
                {string.Join("", c.Orders.Select(o => $@"
                <tr>
                    <td>{o.OrderNumber}</td>
                    <td>{o.Date:yyyy-MM-dd}</td>
                    <td style='text-align:right;'>{o.Amount:C}</td>
                    <td>{o.Status}</td>
                </tr>"))}
            </table>
            <p><strong>Total Orders:</strong> {c.Orders.Sum(o => o.Amount):C}</p>
        </div>
    </div>"))}
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("customer_report.pdf");

// Option 2: Merge separate PDFs
var coverPdf = renderer.RenderHtmlAsPdf(GenerateCoverHtml());
var summaryPdf = renderer.RenderHtmlAsPdf(GenerateSummaryHtml());
var detailPdf = renderer.RenderHtmlAsPdf(GenerateDetailHtml());

var merged = PdfDocument.Merge(coverPdf, summaryPdf, detailPdf);
merged.SaveAs("complete_report.pdf");
```

### Example 5: Cross-Tab / Matrix Reports

**Crystal Reports:** Uses Cross-Tab control

**IronPDF:**
```csharp
using IronPdf;

var salesData = GetSalesData();

// Pivot the data
var regions = salesData.Select(s => s.Region).Distinct().OrderBy(r => r).ToList();
var products = salesData.Select(s => s.Product).Distinct().OrderBy(p => p).ToList();

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial; padding: 30px; }}
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #ddd; padding: 10px; text-align: center; }}
        th {{ background: #2c3e50; color: white; }}
        .row-header {{ background: #34495e; color: white; text-align: left; }}
        .total {{ background: #ecf0f1; font-weight: bold; }}
        .number {{ text-align: right; }}
    </style>
</head>
<body>
    <h1>Sales Cross-Tab Report</h1>

    <table>
        <thead>
            <tr>
                <th>Product</th>
                {string.Join("", regions.Select(r => $"<th>{r}</th>"))}
                <th class='total'>Total</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", products.Select(p =>
            {
                var productSales = regions.Select(r =>
                    salesData.Where(s => s.Product == p && s.Region == r)
                             .Sum(s => s.Amount)).ToList();
                var productTotal = productSales.Sum();

                return $@"
            <tr>
                <td class='row-header'>{p}</td>
                {string.Join("", productSales.Select(s => $"<td class='number'>{s:C0}</td>"))}
                <td class='number total'>{productTotal:C0}</td>
            </tr>";
            }))}
        </tbody>
        <tfoot>
            <tr class='total'>
                <td>Total</td>
                {string.Join("", regions.Select(r =>
                    $"<td class='number'>{salesData.Where(s => s.Region == r).Sum(s => s.Amount):C0}</td>"))}
                <td class='number'>{salesData.Sum(s => s.Amount):C0}</td>
            </tr>
        </tfoot>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("sales_crosstab.pdf");
```

### Example 6: Charts

**Crystal Reports:** Built-in chart objects

**IronPDF with Chart.js:**
```csharp
using IronPdf;

var salesByMonth = GetMonthlySales();

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
    <style>
        body {{ font-family: Arial; padding: 30px; }}
        .chart-container {{ width: 100%; max-width: 800px; margin: 30px auto; }}
        .data-table {{ margin-top: 40px; }}
    </style>
</head>
<body>
    <h1>Monthly Sales Report</h1>

    <div class='chart-container'>
        <canvas id='salesChart'></canvas>
    </div>

    <script>
        new Chart(document.getElementById('salesChart'), {{
            type: 'bar',
            data: {{
                labels: [{string.Join(",", salesByMonth.Select(s => $"'{s.Month}'"))}],
                datasets: [{{
                    label: 'Sales ($)',
                    data: [{string.Join(",", salesByMonth.Select(s => s.Amount))}],
                    backgroundColor: 'rgba(52, 152, 219, 0.8)',
                    borderColor: 'rgba(52, 152, 219, 1)',
                    borderWidth: 1
                }}]
            }},
            options: {{
                responsive: true,
                plugins: {{
                    title: {{
                        display: true,
                        text: 'Sales by Month'
                    }}
                }},
                scales: {{
                    y: {{
                        beginAtZero: true,
                        ticks: {{
                            callback: function(value) {{
                                return '$' + value.toLocaleString();
                            }}
                        }}
                    }}
                }}
            }}
        }});
    </script>

    <table class='data-table' style='width:100%; border-collapse:collapse;'>
        <tr style='background:#2c3e50; color:white;'>
            <th style='padding:10px;'>Month</th>
            <th style='padding:10px; text-align:right;'>Sales</th>
            <th style='padding:10px; text-align:right;'>% of Total</th>
        </tr>
        {string.Join("", salesByMonth.Select(s =>
        {
            var pct = (s.Amount / salesByMonth.Sum(x => x.Amount)) * 100;
            return $@"
        <tr style='border-bottom:1px solid #ddd;'>
            <td style='padding:10px;'>{s.Month}</td>
            <td style='padding:10px; text-align:right;'>{s.Amount:C0}</td>
            <td style='padding:10px; text-align:right;'>{pct:F1}%</td>
        </tr>";
        }))}
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.JavaScript(2000);

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("sales_chart.pdf");
```

### Example 7: Printing Directly

**Crystal Reports:**
```csharp
using CrystalDecisions.CrystalReports.Engine;

ReportDocument report = new ReportDocument();
report.Load(@"Reports\Invoice.rpt");
report.SetDataSource(invoiceData);

// Print to specific printer
report.PrintOptions.PrinterName = "HP LaserJet";
report.PrintOptions.NumberOfCopies = 2;
report.PrintToPrinter(2, false, 0, 0);

report.Close();
```

**IronPDF:**
```csharp
using IronPdf;

// Generate and print
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(invoiceHtml);

// Print with options
pdf.Print(new PrintOptions
{
    PrinterName = "HP LaserJet",
    NumberOfCopies = 2,
    DPI = 300
});

// Or show print dialog
pdf.PrintWithDialog();
```

### Example 8: Security and Metadata

**Crystal Reports:**
```csharp
// Limited security options in Crystal Reports
// Typically handled at report server level
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(reportHtml);

// Set metadata
pdf.MetaData.Title = "Quarterly Sales Report";
pdf.MetaData.Author = "Finance Department";
pdf.MetaData.Subject = "Q4 2024 Sales Analysis";
pdf.MetaData.Keywords = "sales, quarterly, 2024, finance";
pdf.MetaData.Creator = "MyApp Report Generator";

// Set security
pdf.SecuritySettings.OwnerPassword = "admin123";
pdf.SecuritySettings.UserPassword = "view123";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserAnnotations = false;

pdf.SaveAs("secure_report.pdf");
```

---

## Feature Comparison

| Feature | Crystal Reports for VS (CR4VS) | IronPDF |
|---------|--------------------------------|---------|
| **Installation** | | |
| Runtime Distribution | MSI redistributable | NuGet package |
| Installation Method | MSI + Visual Studio extension | `dotnet add package IronPdf` |
| Deployment | Run runtime MSI on each target | xcopy / publish |
| **Platform Support** | | |
| .NET Framework 4.x | Yes | Yes (4.6.2+) |
| .NET Core / .NET 5+ | No | Yes (.NET 6, 8, 9) |
| 64-bit Native | Yes (32-bit ends after SP 39, Dec 2025) | Yes |
| Linux / Docker | No (Windows-only) | Yes |
| Visual Studio | 2012-2022 (VS 2026 pending SP 40) | Any |
| **Development** | | |
| Report Designer | Required (CR4VS extension or CR designer) | Optional (HTML) |
| Template Format | `.rpt` (proprietary binary; not RDL) | HTML/CSS |
| Learning Curve | Crystal Formula language | Web standards |
| IntelliSense | Limited (designer only) | Full C# |
| **Rendering** | | |
| HTML to PDF | Not natively supported | Full Chromium |
| URL to PDF | Not natively supported | Yes |
| CSS Support | Not natively supported | Full CSS3 |
| JavaScript | Not natively supported | Full ES (Chromium) |
| **PDF Features** | | |
| Merge PDFs | Not in CR runtime | Yes |
| Split PDFs | Not in CR runtime | Yes |
| Watermarks | Via report design (background images, formula sections) | Full HTML overlay |
| Digital Signatures | Not in CR runtime | Yes |
| PDF/A export | Yes (PDF/A-1 from CR designer / runtime export) | Yes |
| **Lifecycle** | | |
| End of Mainstream Maintenance | CR4VS: end of 2027; CR 2020: Dec 2026; CR 2025: Dec 31, 2027 | Active |
| Latest Release | SP 39 (Dec 5, 2025) | Current |

---

## Common Migration Issues

### Issue 1: .rpt File Conversion

**Crystal Reports:** Binary .rpt files with embedded layout, data, formulas.

**Solution:** Cannot directly convert - must recreate as HTML:
1. Open .rpt in Crystal Reports designer
2. Document layout, fonts, colors
3. Note all formula fields
4. Recreate in HTML/CSS
5. Convert formulas to C# code

### Issue 2: Database Connections

**Crystal Reports:** Embedded connection strings and ODBC.

**Solution:** Use your application's data layer:
```csharp
// Instead of Crystal's database integration
var data = await _dbContext.Orders
    .Where(o => o.Date >= startDate && o.Date <= endDate)
    .ToListAsync();

// Bind to HTML template
var html = GenerateReportHtml(data);
```

### Issue 3: Runtime Dependencies

**Crystal Reports:** Requires Crystal Reports Runtime installation.

**Solution:** IronPDF is self-contained:
```bash
# Just add the NuGet package
dotnet add package IronPdf
# That's it - no additional installs needed
```

### Issue 4: 32-bit/64-bit and .NET Framework Lock-in

**Crystal Reports:** Crystal's runtime is COM-dependent and ships in separate 32-bit and 64-bit MSIs. SAP confirmed the 32-bit CR .NET runtime is discontinued after SP 39 (December 2025); future service packs are 64-bit only. The runtime targets .NET Framework 4.x and cannot be loaded under .NET Core / .NET 5+.

**Solution:** IronPDF runs on .NET Framework 4.6.2+, .NET 6, .NET 8, and .NET 9 — including Linux and Docker:
```csharp
// No special configuration needed
// Works in any modern .NET environment, x64 or AnyCPU
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all .rpt files**
  ```bash
  find . -name "*.rpt"
  ```
  **Why:** Identify all report files to ensure complete migration coverage.

- [ ] **Screenshot each report layout**
  **Why:** Preserve visual layout for accurate HTML/CSS conversion.

- [ ] **Document formula fields and calculations**
  ```csharp
  // Example formula in Crystal Reports
  // {Orders.OrderAmount} * 0.1

  // Equivalent in C#
  var discount = order.OrderAmount * 0.1;
  ```
  **Why:** Ensure all business logic is correctly translated to C#.

- [ ] **List all data sources and parameters**
  **Why:** Identify data dependencies and parameter usage for accurate data binding.

- [ ] **Identify printing requirements**
  **Why:** Ensure all printing functionalities are replicated with IronPDF.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Remove Crystal Reports packages/references**
  ```bash
  # CR4VS is normally referenced as GAC assemblies (CrystalDecisions.*.dll)
  # delivered by the runtime MSI — remove these references from your .csproj.
  # If you used a community NuGet wrapper, remove it as well:
  dotnet remove package CrystalReports.Engine
  ```
  **Why:** Clean removal of Crystal Reports dependencies.

- [ ] **Remove runtime installation from deployment**
  **Why:** Simplifies deployment by eliminating the need for Crystal Reports runtime.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to the project for PDF generation capabilities.

- [ ] **Convert .rpt layouts to HTML/CSS**
  ```html
  <!-- Example HTML layout -->
  <div class="report">
      <h1>Report Title</h1>
      <p>Report content here...</p>
  </div>
  ```
  **Why:** HTML/CSS provides a flexible and modern way to define report layouts.

- [ ] **Convert Crystal formulas to C#**
  ```csharp
  // Before (Crystal)
  // {Orders.OrderAmount} * 0.1

  // After (C#)
  var discount = order.OrderAmount * 0.1;
  ```
  **Why:** Use C# for calculations to leverage .NET capabilities.

- [ ] **Update data binding code**
  ```csharp
  // Before (Crystal)
  reportDocument.SetDataSource(dataTable);

  // After (IronPDF)
  var htmlContent = "<html>...</html>"; // Use data to generate HTML
  ```
  **Why:** Use HTML with data for flexible and dynamic content generation.

- [ ] **Update printing code**
  ```csharp
  // Before (Crystal)
  reportDocument.PrintToPrinter(1, false, 0, 0);

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(htmlContent);
  pdf.Print();
  ```
  **Why:** IronPDF provides straightforward printing capabilities.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Infrastructure

- [ ] **Remove Crystal Runtime from servers**
  **Why:** Reduces server footprint and simplifies maintenance.

- [ ] **Update deployment scripts**
  **Why:** Ensure deployment processes are aligned with new dependencies.

- [ ] **Remove 32-bit compatibility mode**
  **Why:** IronPDF supports native 64-bit, simplifying deployment.

- [ ] **Update Docker images (if applicable)**
  **Why:** Ensure Docker images contain IronPDF and are optimized for new dependencies.

### Testing

- [ ] **Compare PDF output to original**
  **Why:** Verify that the new PDFs match the original reports in appearance and content.

- [ ] **Verify all calculations**
  **Why:** Ensure all business logic is accurately implemented in C#.

- [ ] **Test all parameters**
  **Why:** Confirm that parameterized reports function correctly.

- [ ] **Test printing**
  **Why:** Ensure that printing functionality works as expected with IronPDF.

- [ ] **Performance testing**
  **Why:** Validate that the new solution meets performance requirements.

- [ ] **64-bit testing**
  **Why:** Confirm that the application runs smoothly in a 64-bit environment.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [Printing PDFs](https://ironpdf.com/how-to/print-pdf/)
- [PDF Security](https://ironpdf.com/how-to/pdf-security/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [SAP Crystal Reports to IronPDF Migration Guide](https://ironpdf.com/blog/migration-guides/migrate-from-sap-crystal-reports-to-ironpdf/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
