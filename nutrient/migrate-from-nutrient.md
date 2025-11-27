# Migration Guide: Nutrient (formerly PSPDFKit) → IronPDF

## Why Migrate to IronPDF

Nutrient is a comprehensive platform with AI features and enterprise-level complexity that many projects don't require. IronPDF provides a focused, straightforward library specifically for PDF generation and manipulation without platform overhead. If you need a simple, cost-effective solution for PDF operations in .NET applications, IronPDF offers better value and ease of use.

## NuGet Package Changes

```bash
# Remove Nutrient (PSPDFKit)
dotnet remove package PSPDFKit
dotnet remove package PSPDFKit.PDF

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Nutrient (PSPDFKit) | IronPDF |
|---------------------|---------|
| `PSPDFKit.Pdf` | `IronPdf` |
| `PSPDFKit.Pdf.Document` | `IronPdf.PdfDocument` |
| `PSPDFKit.Pdf.Rendering` | `IronPdf.Rendering` |
| `PSPDFKit.Pdf.Forms` | `IronPdf.Forms` |
| `PSPDFKit.Pdf.Annotation` | `IronPdf.Annotations` |

## API Mapping

| Nutrient (PSPDFKit) | IronPDF |
|---------------------|---------|
| `Document.Load()` | `PdfDocument.FromFile()` |
| `Document.Save()` | `PdfDocument.SaveAs()` |
| `Document.Render()` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `Document.GetPages()` | `PdfDocument.Pages` |
| `Document.Merge()` | `PdfDocument.Merge()` |
| `Document.AddPage()` | `PdfDocument.AppendPdf()` |
| `PdfProcessor.CreatePdf()` | `ChromePdfRenderer.RenderUrlAsPdf()` |
| `Document.GetFormFields()` | `PdfDocument.Form.Fields` |
| `Document.FlattenAnnotations()` | `PdfDocument.FlattenPdfForm()` |

## Code Examples

### Example 1: Creating PDF from HTML

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;

var htmlContent = "<h1>Hello World</h1><p>Sample content</p>";
var document = PdfProcessor.GeneratePdfFromHtml(htmlContent);
document.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var htmlContent = "<h1>Hello World</h1><p>Sample content</p>";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

### Example 2: Loading and Merging PDFs

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;

var doc1 = Document.Load("document1.pdf");
var doc2 = Document.Load("document2.pdf");
var merged = Document.Merge(new[] { doc1, doc2 });
merged.Save("merged.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("document1.pdf");
var pdf2 = PdfDocument.FromFile("document2.pdf");
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

### Example 3: Converting URL to PDF

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;

var configuration = new PdfConfiguration 
{
    PageSize = PageSize.A4,
    Margins = new Margins(20, 20, 20, 20)
};
var document = PdfProcessor.CreatePdfFromUrl("https://example.com", configuration);
document.Save("webpage.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

## Common Gotchas

- **Licensing**: IronPDF requires a license key for production use. Set it with `License.LicenseKey = "YOUR-KEY";` at application startup.
- **Renderer Initialization**: IronPDF uses `ChromePdfRenderer` for HTML/URL conversion, while Nutrient uses static methods on `PdfProcessor`.
- **Configuration Pattern**: IronPDF uses property-based configuration on renderer instances (`renderer.RenderingOptions`) rather than configuration objects passed to methods.
- **Page Indexing**: Both libraries use zero-based indexing, but IronPDF's `Pages` collection is more directly accessible.
- **Async Operations**: IronPDF methods are primarily synchronous. Use `Task.Run()` if you need async wrappers for non-blocking operations.
- **Form Flattening**: IronPDF uses `FlattenPdfForm()` instead of `FlattenAnnotations()` for removing form field editability.
- **Memory Management**: Always dispose of `PdfDocument` objects when done, especially when processing multiple files.

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)