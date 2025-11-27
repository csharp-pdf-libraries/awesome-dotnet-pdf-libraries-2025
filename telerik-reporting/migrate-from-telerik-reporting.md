# Migration Guide: Telerik Reporting → IronPDF

## Why Migrate from Telerik Reporting to IronPDF

IronPDF eliminates the need to purchase an expensive DevCraft bundle and removes the complexity of report designer installations. Unlike Telerik's report-centric approach, IronPDF provides straightforward, general-purpose PDF generation from HTML, making it ideal for modern web applications. The simpler licensing model and HTML-based workflow significantly reduce both costs and development time.

## NuGet Package Changes

**Remove:**
```xml
<PackageReference Include="Telerik.Reporting" Version="*" />
<PackageReference Include="Telerik.Reporting.Services.AspNetCore" Version="*" />
```

**Add:**
```xml
<PackageReference Include="IronPdf" Version="2024.*" />
```

## Namespace Mapping

| Telerik Reporting | IronPDF |
|------------------|---------|
| `Telerik.Reporting` | `IronPdf` |
| `Telerik.Reporting.Processing` | `IronPdf` |
| `Telerik.ReportViewer.Html5` | N/A (Direct PDF generation) |
| `Telerik.Reporting.Drawing` | Use standard CSS styling |

## API Mapping

| Telerik Reporting | IronPDF | Notes |
|------------------|---------|-------|
| `Report` class | `ChromePdfRenderer` | Core rendering engine |
| `ReportProcessor` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Main conversion method |
| `ReportDocument` | `PdfDocument` | PDF document object |
| `RenderReport()` | `RenderHtmlAsPdf()` / `RenderUrlAsPdf()` | Rendering methods |
| `.trdp` / `.trdx` files | HTML/CSS templates | Use standard web formats |
| `Table` / `TextBox` report items | HTML elements | Use `<table>`, `<div>`, etc. |
| `PageSettings` | `ChromePdfRenderer.RenderingOptions` | Page configuration |
| `Export()` | `SaveAs()` | Save PDF to file |
| `DataSource` binding | Razor/template engine | Use C# string interpolation |

## Code Examples

### Example 1: Basic Report Generation

**Before (Telerik Reporting):**
```csharp
using Telerik.Reporting;
using Telerik.Reporting.Processing;

var reportSource = new TypeReportSource();
reportSource.TypeName = typeof(MyReport).AssemblyQualifiedName;

var reportProcessor = new ReportProcessor();
var result = reportProcessor.RenderReport("PDF", reportSource, null);

var fileName = result.DocumentName + "." + result.Extension;
using (FileStream fs = new FileStream(fileName, FileMode.Create))
{
    fs.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var htmlContent = "<h1>My Report</h1><p>Report content here</p>";
var pdf = renderer.RenderHtmlAsPdf(htmlContent);

pdf.SaveAs("MyReport.pdf");
```

### Example 2: Data-Driven Report with Table

**Before (Telerik Reporting):**
```csharp
using Telerik.Reporting;
using Telerik.Reporting.Drawing;

var report = new Report();
var table = new Table();
table.DataSource = GetDataSource();
table.Body.Rows.Add(new TableBodyRow());

// Complex binding setup
var textBox = new TextBox();
textBox.Value = "=Fields.CustomerName";
table.Body.Rows[0].Cells.Add(textBox);

report.Items.Add(table);

var reportProcessor = new ReportProcessor();
var result = reportProcessor.RenderReport("PDF", report, null);
```

**After (IronPDF):**
```csharp
using IronPdf;

var data = GetDataSource();
var html = $@"
<html>
<head><style>
    table {{ border-collapse: collapse; width: 100%; }}
    th, td {{ border: 1px solid black; padding: 8px; }}
</style></head>
<body>
    <h1>Customer Report</h1>
    <table>
        <tr><th>Customer Name</th><th>Amount</th></tr>
        {string.Join("", data.Select(d => $"<tr><td>{d.CustomerName}</td><td>{d.Amount:C}</td></tr>"))}
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("CustomerReport.pdf");
```

### Example 3: Page Settings and Headers/Footers

**Before (Telerik Reporting):**
```csharp
using Telerik.Reporting;
using Telerik.Reporting.Drawing;

var report = new Report();
report.PageSettings.PaperKind = PaperKind.A4;
report.PageSettings.Landscape = false;
report.PageSettings.Margins = new MarginsU(new Unit(1, UnitType.Inch));

var pageHeader = new PageHeaderSection();
pageHeader.Height = new Unit(0.5, UnitType.Inch);
var headerText = new TextBox();
headerText.Value = "Company Report";
pageHeader.Items.Add(headerText);
report.PageHeaderSection = pageHeader;

var reportProcessor = new ReportProcessor();
var result = reportProcessor.RenderReport("PDF", report, null);
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
renderer.RenderingOptions.MarginTop = 40;
renderer.RenderingOptions.MarginBottom = 40;
renderer.RenderingOptions.MarginLeft = 40;
renderer.RenderingOptions.MarginRight = 40;

renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
{
    CenterText = "Company Report",
    DrawDividerLine = true,
    FontSize = 12
};

renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
{
    RightText = "Page {page} of {total-pages}",
    FontSize = 10
};

var html = "<h1>Report Content</h1><p>Your content here</p>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("CompanyReport.pdf");
```

## Common Gotchas

### 1. **No Visual Designer**
IronPDF doesn't have a visual report designer like Telerik. Use HTML/CSS instead, which offers more flexibility and can be edited with standard web development tools.

### 2. **Template Engine Integration**
For complex data binding, integrate with Razor Pages, Handlebars, or similar template engines instead of relying on Telerik's built-in expressions.

```csharp
// Use Razor for complex templates
var html = await RazorTemplateEngine.RenderAsync("ReportTemplate.cshtml", model);
var pdf = renderer.RenderHtmlAsPdf(html);
```

### 3. **Licensing Configuration**
Remember to set your IronPDF license key at application startup:

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 4. **CSS Print Styles**
Use CSS `@media print` rules for print-specific styling, and `page-break-after` for controlling pagination:

```css
@media print {
    .page-break { page-break-after: always; }
    .no-print { display: none; }
}
```

### 5. **External Resources**
Ensure external CSS, images, and fonts are accessible. Use `RenderingOptions.UseGpu = false` for server environments without GPU.

```csharp
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
renderer.RenderingOptions.EnableJavaScript = true; // If needed
```

### 6. **Async Operations**
IronPDF supports async rendering for better performance:

```csharp
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

## Additional Resources

- **Official Documentation:** https://ironpdf.com/docs/
- **Tutorials and Examples:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/docs/questions/html-to-pdf/
- **Code Examples Repository:** https://ironpdf.com/examples/