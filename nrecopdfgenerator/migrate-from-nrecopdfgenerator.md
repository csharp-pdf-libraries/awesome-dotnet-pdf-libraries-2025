# Migration Guide: NReco.PdfGenerator → IronPDF

## Why Migrate

NReco.PdfGenerator's free version includes watermarks and requires purchasing a commercial license for production use with opaque pricing that requires contacting sales. Additionally, it inherits all CVEs from the underlying wkhtmltopdf engine, creating ongoing security concerns. IronPDF offers a modern, actively maintained solution with transparent licensing, better performance, and comprehensive .NET support.

## NuGet Package Changes

```bash
# Remove NReco.PdfGenerator
dotnet remove package NReco.PdfGenerator

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| NReco.PdfGenerator | IronPdf |
|-------------------|---------|
| `NReco.PdfGenerator` | `IronPdf` |
| `NReco.PdfGenerator.HtmlToPdfConverter` | `IronPdf.ChromePdfRenderer` |
| N/A | `IronPdf.Rendering` |

## API Mapping

| NReco.PdfGenerator | IronPDF | Notes |
|-------------------|---------|-------|
| `new HtmlToPdfConverter()` | `new ChromePdfRenderer()` | Main renderer object |
| `GeneratePdf(html)` | `RenderHtmlAsPdf(html).SaveAs(path)` | Generate and save PDF |
| `GeneratePdfFromFile(htmlFile, pdfFile)` | `RenderHtmlFileAsPdf(htmlFile).SaveAs(pdfFile)` | File-based conversion |
| `Orientation = PageOrientation.Landscape` | `RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape` | Set page orientation |
| `PageWidth`, `PageHeight` | `RenderingOptions.PaperSize` | Paper size configuration |
| `Margins` | `RenderingOptions.MarginTop/Bottom/Left/Right` | Page margins |
| `Zoom` | `RenderingOptions.Zoom` | Zoom level |
| `GeneratePdf(htmlContent)` returns `byte[]` | `RenderHtmlAsPdf(html)` returns `PdfDocument` | Return type difference |

## Code Examples

### Basic HTML to PDF Conversion

**Before (NReco.PdfGenerator):**
```csharp
using NReco.PdfGenerator;

var converter = new HtmlToPdfConverter();
var htmlContent = "<html><body><h1>Hello World</h1></body></html>";
var pdfBytes = converter.GeneratePdf(htmlContent);
File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var htmlContent = "<html><body><h1>Hello World</h1></body></html>";
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

### HTML File to PDF with Options

**Before (NReco.PdfGenerator):**
```csharp
using NReco.PdfGenerator;

var converter = new HtmlToPdfConverter
{
    Orientation = PageOrientation.Landscape,
    Margins = new PageMargins { Top = 10, Bottom = 10, Left = 5, Right = 5 },
    Size = PageSize.A4
};
converter.GeneratePdfFromFile("input.html", "output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.MarginLeft = 5;
renderer.RenderingOptions.MarginRight = 5;
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### URL to PDF with Custom Settings

**Before (NReco.PdfGenerator):**
```csharp
using NReco.PdfGenerator;

var converter = new HtmlToPdfConverter
{
    Zoom = 1.5f,
    Quiet = false
};

var htmlContent = new System.Net.WebClient().DownloadString("https://example.com");
var pdfBytes = converter.GeneratePdf(htmlContent);
File.WriteAllBytes("webpage.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.Zoom = 150; // Percentage-based

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

## Common Gotchas

- **Return Type**: NReco returns `byte[]` directly, while IronPDF returns a `PdfDocument` object that provides additional manipulation capabilities before saving
- **Zoom Values**: NReco uses float (1.5), IronPDF uses percentage integers (150)
- **Margins**: Both use similar margin properties, but IronPDF uses individual properties instead of a `Margins` object
- **Licensing**: IronPDF requires a license key for production use. Set via `IronPdf.License.LicenseKey = "YOUR-KEY";` before rendering
- **Dependencies**: IronPDF doesn't require external wkhtmltopdf binaries; it uses Chromium internally and manages dependencies automatically
- **Asynchronous API**: IronPDF supports async/await patterns with methods like `RenderHtmlAsPdfAsync()` for better performance in web applications
- **Error Handling**: IronPDF provides more detailed exception messages; wrap PDF operations in try-catch blocks for production code

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [IronPDF API Reference](https://ironpdf.com/docs/)