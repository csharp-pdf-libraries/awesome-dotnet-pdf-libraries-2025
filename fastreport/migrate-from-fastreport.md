# Migration Guide: FastReport.NET → IronPDF

## Why Migrate to IronPDF

IronPDF provides a simpler, more intuitive approach to PDF generation focused on HTML-to-PDF conversion, eliminating the need for visual designers and complex reporting templates. It's ideal for developers who want to generate PDFs programmatically using familiar web technologies (HTML/CSS) rather than learning report-specific design tools. IronPDF offers better integration with modern web applications and requires significantly less boilerplate code.

## NuGet Package Changes

```bash
# Remove FastReport.NET
dotnet remove package FastReport.OpenSource

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| FastReport.NET | IronPDF |
|----------------|---------|
| `FastReport` | `IronPdf` |
| `FastReport.Utils` | `IronPdf.Rendering` |
| `FastReport.Export.PdfSimple` | `IronPdf` (built-in) |
| `FastReport.Data` | N/A (use standard .NET data sources) |

## API Mapping

| FastReport.NET | IronPDF | Notes |
|----------------|---------|-------|
| `Report.Load()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | HTML-based instead of report files |
| `Report.RegisterData()` | Direct HTML templating | Use Razor, string interpolation, or templating engine |
| `Report.Prepare()` | N/A | Not needed - direct rendering |
| `Report.Export(new PDFSimpleExport())` | `PdfDocument.SaveAs()` | Simplified export |
| `Report.Designer` | N/A | Use HTML/CSS editors instead |
| `TextObject` | HTML `<p>`, `<span>` | Standard HTML elements |
| `TableObject` | HTML `<table>` | Standard HTML tables |
| `DataBand` | Loop in templating engine | Use Razor or string building |

## Code Examples

### Example 1: Simple PDF Generation

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.PdfSimple;

var report = new Report();
report.Load("invoice.frx");
report.Prepare();

using (var export = new PDFSimpleExport())
{
    report.Export(export, "invoice.pdf");
}
report.Dispose();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Invoice</h1><p>Invoice content here</p>");
pdf.SaveAs("invoice.pdf");
```

### Example 2: PDF from Data

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.PdfSimple;
using System.Data;

var report = new Report();
report.Load("products.frx");

var dataTable = GetProductData();
report.RegisterData(dataTable, "Products");
report.Prepare();

using (var export = new PDFSimpleExport())
{
    report.Export(export, "products.pdf");
}
report.Dispose();
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Text;

var products = GetProductData();
var html = new StringBuilder("<h1>Products</h1><table>");

foreach (var product in products)
{
    html.Append($"<tr><td>{product.Name}</td><td>${product.Price}</td></tr>");
}
html.Append("</table>");

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html.ToString());
pdf.SaveAs("products.pdf");
```

### Example 3: PDF with Styling

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.PdfSimple;

var report = new Report();
report.Load("styled_report.frx");

// Styling done in designer
var textObj = report.FindObject("Text1") as TextObject;
textObj.Font = new Font("Arial", 14, FontStyle.Bold);
textObj.FillColor = Color.LightBlue;

report.Prepare();
using (var export = new PDFSimpleExport())
{
    report.Export(export, "styled.pdf");
}
report.Dispose();
```

**After (IronPDF):**
```csharp
using IronPdf;

var html = @"
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        h1 { font-size: 14pt; font-weight: bold; 
             background-color: lightblue; padding: 10px; }
    </style>
</head>
<body>
    <h1>Styled Header</h1>
    <p>Content here</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("styled.pdf");
```

## Common Gotchas

1. **No Visual Designer**: IronPDF doesn't have a visual report designer. Use HTML/CSS tools or templating engines like Razor Pages instead.

2. **Data Binding Approach**: FastReport uses data bands and bindings; IronPDF requires you to build HTML strings with your data using loops, LINQ, or templating engines.

3. **Report File Format**: `.frx` report files won't work with IronPDF. Convert your reports to HTML templates.

4. **Page Breaks**: Use CSS `page-break-after: always;` or `page-break-before: always;` instead of FastReport's band-based page management.

5. **Headers/Footers**: Set up using `RenderingOptions`:
   ```csharp
   var renderer = new ChromePdfRenderer();
   renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter() { HtmlFragment = "<h3>Header</h3>" };
   ```

6. **Complex Layouts**: FastReport's nested bands must be translated to HTML tables or CSS Grid/Flexbox layouts.

7. **Licensing**: IronPDF requires a license key for production use. Set it with `IronPdf.License.LicenseKey = "YOUR-KEY";`

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/docs/questions/html-to-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/