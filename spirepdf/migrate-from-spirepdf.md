# Migration Guide: Spire.PDF to IronPDF

## Why Migrate

IronPDF resolves critical issues with Spire.PDF's HTML-to-PDF conversion by rendering text as actual selectable text rather than images, ensuring PDFs are searchable and accessible. Unlike Spire.PDF's reliance on Internet Explorer's rendering engine, IronPDF uses a modern Chromium-based engine that accurately renders contemporary HTML5, CSS3, and JavaScript. Additionally, IronPDF provides reliable font embedding and doesn't suffer from the text-selection and searchability problems that plague Spire.PDF's LoadFromHTML method.

## NuGet Package Changes

```bash
# Remove Spire.PDF
dotnet remove package Spire.PDF

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Spire.PDF | IronPDF |
|-----------|---------|
| `Spire.Pdf` | `IronPdf` |
| `Spire.Pdf.Graphics` | `IronPdf.Rendering` |
| `Spire.Pdf.HtmlConverter` | `IronPdf` (ChromePdfRenderer) |
| `Spire.Pdf.Widget` | `IronPdf.Forms` |

## API Mapping Table

| Spire.PDF API | IronPDF API | Notes |
|---------------|-------------|-------|
| `PdfDocument.LoadFromHTML(html)` | `ChromePdfRenderer.RenderHtmlAsPdf(html)` | IronPDF renders real text, not images |
| `PdfDocument.LoadFromFile(path)` | `PdfDocument.FromFile(path)` | Loading existing PDFs |
| `PdfDocument.SaveToFile(path)` | `PdfDocument.SaveAs(path)` | Saving PDFs |
| `PdfHTMLLayoutFormat` | `ChromePdfRenderOptions` | Configuration options |
| `PdfDocument.Pages` | `PdfDocument.Pages` | Page collection access |
| `PdfPageBase.CreateGraphics()` | Direct rendering methods | Graphics operations |
| `PdfTrueTypeFont` | `IronPdf.Fonts` | Font handling |
| `PdfDocument.Attachments` | `PdfDocument.Attachments` | File attachments |

## Code Examples

### Example 1: HTML to PDF Conversion

**Before (Spire.PDF):**
```csharp
using Spire.Pdf;
using Spire.Pdf.HtmlConverter;

// Creates image-based PDF (text not selectable)
PdfDocument pdf = new PdfDocument();
pdf.LoadFromHTML("<html><body><h1>Hello World</h1></body></html>", 
    false, true, true);
pdf.SaveToFile("output.pdf");
pdf.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;

// Creates real text-based PDF (fully selectable/searchable)
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Hello World</h1></body></html>");
pdf.SaveAs("output.pdf");
```

### Example 2: HTML File to PDF with Options

**Before (Spire.PDF):**
```csharp
using Spire.Pdf;
using Spire.Pdf.HtmlConverter;

PdfDocument pdf = new PdfDocument();
PdfHTMLLayoutFormat htmlLayoutFormat = new PdfHTMLLayoutFormat();
htmlLayoutFormat.Layout = PdfLayoutType.FitToPage;
pdf.LoadFromHTML("input.html", true, htmlLayoutFormat);
pdf.SaveToFile("output.pdf");
pdf.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.FitToPaperMode = FitToPaperModes.AutomaticZoom;
var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 3: URL to PDF

**Before (Spire.PDF):**
```csharp
using Spire.Pdf;

PdfDocument pdf = new PdfDocument();
pdf.LoadFromHTML("https://example.com", false, true, true);
pdf.SaveToFile("webpage.pdf");
pdf.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

## Common Gotchas

| Issue | Solution |
|-------|----------|
| **Text rendered as images in old PDFs** | Spire.PDF-generated PDFs with LoadFromHTML are image-based and cannot be fixed. Regenerate with IronPDF for searchable text. |
| **Missing fonts** | IronPDF automatically handles font embedding. Use `renderer.RenderingOptions.CustomCssUrl` for custom fonts. |
| **IE rendering differences** | IronPDF uses Chromium. Test existing HTML as rendering will be more modern and accurate. |
| **Layout differences** | IronPDF respects modern CSS. Use `ChromePdfRenderOptions` to adjust margins, paper size, and scaling. |
| **JavaScript not executing** | IronPDF fully supports JavaScript. Use `renderer.RenderingOptions.RenderDelay` if content loads asynchronously. |
| **Synchronous API differences** | IronPDF methods are synchronous by default. Async versions available with `Async` suffix (e.g., `RenderHtmlAsPdfAsync`). |
| **Licensing** | IronPDF requires a license key for production. Set with `IronPdf.License.LicenseKey = "YOUR-KEY";` |

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/tutorials/html-to-pdf/
- **API Reference**: https://ironpdf.com/docs/