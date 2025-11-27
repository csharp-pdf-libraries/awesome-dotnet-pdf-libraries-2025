# Migration Guide: SSRS → IronPDF

## Why Migrate from SSRS to IronPDF

SSRS requires a full SQL Server infrastructure and dedicated report server, making it heavyweight and costly for many applications. IronPDF is a lightweight .NET library that can be embedded directly into your application, eliminating server dependencies and Microsoft ecosystem lock-in. It offers greater deployment flexibility, lower infrastructure costs, and simpler integration for modern .NET applications.

## NuGet Package Changes

```xml
<!-- Remove SSRS dependencies (no direct NuGet packages, typically server-based) -->
<!-- SSRS is server-based and doesn't use NuGet packages in client applications -->

<!-- Add IronPDF -->
<PackageReference Include="IronPdf" Version="2024.*" />
```

## Namespace Mapping

| SSRS | IronPDF |
|------|---------|
| `Microsoft.Reporting.WebForms` | `IronPdf` |
| `Microsoft.Reporting.WinForms` | `IronPdf` |
| `Microsoft.ReportingServices` | `IronPdf` |
| N/A (Server-based) | `IronPdf.Rendering` |

## API Mapping

| SSRS Concept | IronPDF Equivalent |
|--------------|-------------------|
| `LocalReport.Render()` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `ReportViewer.ServerReport` | `HtmlToPdf.RenderUrlAsPdf()` |
| `.rdlc` files | HTML/Razor templates |
| `ReportParameter` | HTML string interpolation / Razor models |
| `ReportDataSource` | Data binding in HTML/Razor |
| `ExportFormat.PDF` | Default PDF output |
| `LocalReport.SetParameters()` | Pass data to HTML templates |
| `SubReport` | Multiple `PdfDocument` merge |

## Code Examples

### Example 1: Basic Report Generation

**Before (SSRS):**
```csharp
using Microsoft.Reporting.WebForms;
using System.Data;

// Load RDLC report
LocalReport report = new LocalReport();
report.ReportPath = "Reports/Invoice.rdlc";

// Bind data
DataTable dt = GetInvoiceData();
ReportDataSource rds = new ReportDataSource("InvoiceDataSet", dt);
report.DataSources.Add(rds);

// Set parameters
ReportParameter[] parameters = new ReportParameter[]
{
    new ReportParameter("InvoiceNumber", "INV-001"),
    new ReportParameter("Date", DateTime.Now.ToString())
};
report.SetParameters(parameters);

// Render to PDF
byte[] pdfBytes = report.Render("PDF");
File.WriteAllBytes("invoice.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

// Create HTML from data
var invoice = GetInvoiceData();
string html = $@"
    <html>
    <head><style>body {{ font-family: Arial; }}</style></head>
    <body>
        <h1>Invoice {invoice.Number}</h1>
        <p>Date: {DateTime.Now:yyyy-MM-dd}</p>
        <table>
            <tr><th>Item</th><th>Amount</th></tr>
            {string.Join("", invoice.Items.Select(i => 
                $"<tr><td>{i.Name}</td><td>${i.Price}</td></tr>"))}
        </table>
    </body>
    </html>";

// Render to PDF
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

### Example 2: Report with Database Data

**Before (SSRS):**
```csharp
using Microsoft.Reporting.WebForms;
using System.Data.SqlClient;

LocalReport report = new LocalReport();
report.ReportPath = "Reports/SalesReport.rdlc";

// Get data from database
string connectionString = "Server=.;Database=Sales;Integrated Security=true;";
using (SqlConnection conn = new SqlConnection(connectionString))
{
    SqlCommand cmd = new SqlCommand("SELECT * FROM Orders WHERE Year = @Year", conn);
    cmd.Parameters.AddWithValue("@Year", 2024);
    
    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
    DataTable dt = new DataTable();
    adapter.Fill(dt);
    
    ReportDataSource rds = new ReportDataSource("OrdersDataSet", dt);
    report.DataSources.Add(rds);
}

byte[] pdfBytes = report.Render("PDF");
Response.BinaryWrite(pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Data.SqlClient;
using System.Text;

// Get data from database
string connectionString = "Server=.;Database=Sales;Integrated Security=true;";
var orders = new List<Order>();

using (SqlConnection conn = new SqlConnection(connectionString))
{
    SqlCommand cmd = new SqlCommand("SELECT * FROM Orders WHERE Year = @Year", conn);
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
                Amount = reader.GetDecimal(2)
            });
        }
    }
}

// Build HTML report
var html = $@"
    <html>
    <body>
        <h1>Sales Report 2024</h1>
        <table border='1'>
            <tr><th>Order ID</th><th>Customer</th><th>Amount</th></tr>
            {string.Join("", orders.Select(o => 
                $"<tr><td>{o.Id}</td><td>{o.Customer}</td><td>${o.Amount:N2}</td></tr>"))}
        </table>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
// Return PDF in web response
return File(pdf.BinaryData, "application/pdf", "SalesReport.pdf");
```

### Example 3: Using Razor Templates (Recommended Approach)

**Before (SSRS):**
```csharp
using Microsoft.Reporting.WebForms;

LocalReport report = new LocalReport();
report.ReportPath = "Reports/CustomerStatement.rdlc";

// Complex parameter setup
ReportParameterCollection parameters = new ReportParameterCollection();
parameters.Add(new ReportParameter("CustomerName", customer.Name));
parameters.Add(new ReportParameter("AccountNumber", customer.AccountNumber));
parameters.Add(new ReportParameter("Balance", customer.Balance.ToString()));
report.SetParameters(parameters);

// Multiple data sources
report.DataSources.Add(new ReportDataSource("Transactions", GetTransactions()));
report.DataSources.Add(new ReportDataSource("Summary", GetSummary()));

byte[] pdfBytes = report.Render("PDF");
```

**After (IronPDF with Razor):**
```csharp
using IronPdf;
using RazorEngine;
using RazorEngine.Templating;

// Create strongly-typed model
public class StatementModel
{
    public Customer Customer { get; set; }
    public List<Transaction> Transactions { get; set; }
    public Summary Summary { get; set; }
}

// Razor template (Views/Statement.cshtml)
string razorTemplate = @"
    @model StatementModel
    <html>
    <body>
        <h1>Statement for @Model.Customer.Name</h1>
        <p>Account: @Model.Customer.AccountNumber</p>
        <p>Balance: $@Model.Customer.Balance</p>
        
        <h2>Transactions</h2>
        <table>
            @foreach(var txn in Model.Transactions)
            {
                <tr>
                    <td>@txn.Date.ToShortDateString()</td>
                    <td>@txn.Description</td>
                    <td>$@txn.Amount</td>
                </tr>
            }
        </table>
    </body>
    </html>";

// Render with data
var model = new StatementModel
{
    Customer = customer,
    Transactions = GetTransactions(),
    Summary = GetSummary()
};

string html = Engine.Razor.RunCompile(razorTemplate, "statementKey", typeof(StatementModel), model);

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("statement.pdf");
```

## Common Gotchas

1. **No Direct RDLC Support**: IronPDF doesn't read `.rdlc` files. Convert your reports to HTML/CSS templates or use Razor views for templating.

2. **Data Binding Differences**: SSRS uses `ReportDataSource` objects; IronPDF requires you to build HTML with data already embedded using string interpolation, templating engines, or Razor.

3. **Server Infrastructure**: SSRS requires IIS and SQL Server Reporting Services. IronPDF runs in-process within your application—no server setup needed.

4. **Licensing Model Change**: SSRS licensing is tied to SQL Server. IronPDF has per-developer licensing. Review at https://ironpdf.com/docs/

5. **Report Designer**: SSRS has Visual Studio Report Designer for WYSIWYG editing. With IronPDF, create reports using HTML/CSS (any editor) or Razor views.

6. **Parameters**: SSRS has built-in parameter UI. IronPDF requires you to build any parameter input forms in your application UI.

7. **Subreports**: SSRS subreports must be converted to either nested HTML or merged PDFs using `PdfDocument.Merge()`.

8. **Deployment**: SSRS requires report server deployment. IronPDF deploys with your application DLLs—no separate deployment process.

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **Tutorials & Examples**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/docs/
- **Razor Template Integration**: https://ironpdf.com/tutorials/