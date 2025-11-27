# Migration Guide: Rotativa → IronPDF

## Why Migrate from Rotativa

Rotativa is limited to ASP.NET MVC and has been abandoned, with no updates addressing critical security vulnerabilities like CVE-2022-35583 in its underlying wkhtmltopdf engine. IronPDF provides actively maintained, cross-platform support for all .NET project types including Razor Pages, Blazor, minimal APIs, and .NET Core/5+, with modern security standards and ongoing updates.

## NuGet Package Changes

```bash
# Remove Rotativa
dotnet remove package Rotativa

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Rotativa | IronPDF |
|----------|---------|
| `using Rotativa;` | `using IronPdf;` |
| `using Rotativa.Options;` | `using IronPdf.Rendering;` |

## API Mapping Table

| Rotativa | IronPDF |
|----------|---------|
| `new ViewAsPdf()` | `ChromePdfRenderer.RenderHtmlAsPdf()` or `ChromePdfRenderer.RenderRazorToPdf()` |
| `new ActionAsPdf()` | `ChromePdfRenderer.RenderUrlAsPdf()` |
| `new UrlAsPdf()` | `ChromePdfRenderer.RenderUrlAsPdf()` |
| `.FileName` property | `PdfDocument.SaveAs()` method |
| `.PageOrientation` | `RenderingOptions.PaperOrientation` |
| `.PageSize` | `RenderingOptions.PaperSize` |
| `.CustomSwitches` | `RenderingOptions` properties |
| Return PDF from controller | Return `File()` with `PdfDocument.BinaryData` |

## Before/After Code Examples

### Example 1: View to PDF

**Before (Rotativa):**
```csharp
using Rotativa;

public class InvoiceController : Controller
{
    public ActionResult InvoicePdf(int id)
    {
        var model = GetInvoiceModel(id);
        return new ViewAsPdf("Invoice", model)
        {
            FileName = "Invoice.pdf",
            PageOrientation = Orientation.Portrait,
            PageSize = Size.A4
        };
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;

public class InvoiceController : Controller
{
    public async Task<IActionResult> InvoicePdf(int id)
    {
        var model = GetInvoiceModel(id);
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        
        var pdf = await renderer.RenderRazorToPdfAsync("Views/Invoice/Invoice.cshtml", model);
        return File(pdf.BinaryData, "application/pdf", "Invoice.pdf");
    }
}
```

### Example 2: URL to PDF

**Before (Rotativa):**
```csharp
public ActionResult DownloadReport()
{
    return new UrlAsPdf("https://example.com/report")
    {
        FileName = "Report.pdf",
        PageSize = Size.Letter
    };
}
```

**After (IronPDF):**
```csharp
public async Task<IActionResult> DownloadReport()
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
    
    var pdf = await renderer.RenderUrlAsPdfAsync("https://example.com/report");
    return File(pdf.BinaryData, "application/pdf", "Report.pdf");
}
```

### Example 3: HTML String to PDF

**Before (Rotativa):**
```csharp
public ActionResult GeneratePdf()
{
    var htmlContent = "<h1>Hello World</h1><p>Generated PDF</p>";
    return new ViewAsPdf()
    {
        ViewData = new ViewDataDictionary { Model = htmlContent },
        FileName = "Document.pdf",
        CustomSwitches = "--page-offset 0 --footer-center [page] --footer-font-size 8"
    };
}
```

**After (IronPDF):**
```csharp
public IActionResult GeneratePdf()
{
    var htmlContent = "<h1>Hello World</h1><p>Generated PDF</p>";
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.FirstPageNumber = 1;
    renderer.RenderingOptions.CenterFooter = "{page}";
    renderer.RenderingOptions.FooterFontSize = 8;
    
    var pdf = renderer.RenderHtmlAsPdf(htmlContent);
    return File(pdf.BinaryData, "application/pdf", "Document.pdf");
}
```

## Common Gotchas

### Async/Await Pattern
- **Rotativa**: Synchronous operations only
- **IronPDF**: Use `async` methods (`RenderUrlAsPdfAsync`, `RenderRazorToPdfAsync`) for better performance and scalability

### Razor View Rendering
- **Rotativa**: Automatically finds views using MVC conventions
- **IronPDF**: Requires explicit view path, e.g., `"Views/Invoice/Invoice.cshtml"` or use `RenderHtmlAsPdf()` with manually rendered HTML

### Return Type
- **Rotativa**: Returns `ActionResult` subclass directly
- **IronPDF**: Returns `PdfDocument` object; use `File()` method with `pdf.BinaryData` to return as download

### Configuration Options
- **Rotativa**: Uses `CustomSwitches` string for wkhtmltopdf flags
- **IronPDF**: Uses strongly-typed `RenderingOptions` properties (IntelliSense-friendly)

### Licensing
- **Rotativa**: Free and open source
- **IronPDF**: Commercial license required for production use (free trial available)

### Deployment
- **Rotativa**: Requires wkhtmltopdf binaries in project
- **IronPDF**: Self-contained NuGet package with Chrome rendering engine included

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [ASP.NET Core PDF Generation Tutorial](https://ironpdf.com/tutorials/aspnet-core-pdf/)