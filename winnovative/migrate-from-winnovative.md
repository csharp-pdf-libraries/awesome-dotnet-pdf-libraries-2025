# Migration Guide: Winnovative to IronPDF

## Why Migrate

Winnovative relies on a WebKit engine from 2016 that lacks support for modern CSS features like Grid and has buggy Flexbox implementation. Modern JavaScript (ES6+) is unreliable or completely unsupported, and despite its name suggesting "innovation," the product has seen minimal updates in recent years. IronPDF provides a modern, actively maintained Chrome-based rendering engine with full support for contemporary web standards.

## NuGet Package Changes

```bash
# Remove Winnovative
dotnet remove package Winnovative.WebKitHtmlToPdf

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Winnovative | IronPDF |
|------------|---------|
| `Winnovative.WebKit` | `IronPdf` |
| `Winnovative.WebKit.HtmlToPdfConverter` | `IronPdf.ChromePdfRenderer` |
| `Winnovative.WebKit.PdfDocument` | `IronPdf.PdfDocument` |

## API Mapping

| Winnovative | IronPDF |
|------------|---------|
| `HtmlToPdfConverter()` | `new ChromePdfRenderer()` |
| `ConvertUrlToPdf(url)` | `RenderUrlAsPdf(url)` |
| `ConvertHtmlToPdf(html, baseUrl)` | `RenderHtmlAsPdf(html)` |
| `PdfDocumentOptions.PdfPageSize` | `RenderingOptions.PaperSize` |
| `PdfDocumentOptions.PdfPageOrientation` | `RenderingOptions.PaperOrientation` |
| `PdfDocumentOptions.TopMargin` | `RenderingOptions.MarginTop` |
| `PdfDocumentOptions.BottomMargin` | `RenderingOptions.MarginBottom` |
| `PdfDocumentOptions.LeftMargin` | `RenderingOptions.MarginLeft` |
| `PdfDocumentOptions.RightMargin` | `RenderingOptions.MarginRight` |
| `ConvertHtmlToPdfFile(html, filePath)` | `RenderHtmlAsPdf(html).SaveAs(filePath)` |

## Code Examples

### Example 1: Basic URL to PDF Conversion

**Before (Winnovative):**
```csharp
using Winnovative.WebKit;

var converter = new HtmlToPdfConverter();
converter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
converter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;

byte[] pdfBytes = converter.ConvertUrlToPdf("https://example.com");
File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 2: HTML String to PDF with Margins

**Before (Winnovative):**
```csharp
using Winnovative.WebKit;

var converter = new HtmlToPdfConverter();
converter.PdfDocumentOptions.TopMargin = 20;
converter.PdfDocumentOptions.BottomMargin = 20;
converter.PdfDocumentOptions.LeftMargin = 15;
converter.PdfDocumentOptions.RightMargin = 15;

string html = "<html><body><h1>Hello World</h1></body></html>";
byte[] pdfBytes = converter.ConvertHtmlToPdf(html, "");

File.WriteAllBytes("document.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 15;
renderer.RenderingOptions.MarginRight = 15;

string html = "<html><body><h1>Hello World</h1></body></html>";
var pdf = renderer.RenderHtmlAsPdf(html);

pdf.SaveAs("document.pdf");
```

### Example 3: Custom Page Size and Headers

**Before (Winnovative):**
```csharp
using Winnovative.WebKit;

var converter = new HtmlToPdfConverter();
converter.PdfDocumentOptions.PdfPageSize = PdfPageSize.Letter;
converter.PdfDocumentOptions.ShowHeader = true;
converter.PdfHeaderOptions.HeaderText = "Company Report";
converter.PdfHeaderOptions.HeaderTextColor = System.Drawing.Color.Blue;

byte[] pdfBytes = converter.ConvertUrlToPdf("https://example.com/report");
File.WriteAllBytes("report.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='color:blue;'>Company Report</div>"
};

var pdf = renderer.RenderUrlAsPdf("https://example.com/report");
pdf.SaveAs("report.pdf");
```

## Common Gotchas

- **Headers/Footers**: Winnovative uses simple text properties while IronPDF uses HTML fragments for greater flexibility
- **Return Types**: Winnovative methods often return `byte[]` directly; IronPDF returns `PdfDocument` objects that must be saved or converted
- **Base URLs**: Winnovative's `ConvertHtmlToPdf()` requires a base URL parameter; IronPDF handles this automatically or via `RenderingOptions.BaseUrl`
- **CSS Rendering**: Expect improved layout rendering in IronPDF—existing workarounds for Winnovative's CSS bugs can likely be removed
- **JavaScript Execution**: Modern ES6+ JavaScript will now work correctly; you may need to update timeouts using `RenderingOptions.RenderDelay`
- **Licensing**: IronPDF requires initialization with a license key: `IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";`

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)