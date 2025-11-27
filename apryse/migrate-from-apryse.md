# Migration Guide: Apryse (formerly PDFTron) → IronPDF

## Why Migrate from Apryse to IronPDF

Apryse targets enterprise customers with premium pricing that can be prohibitive for small to medium-sized projects. IronPDF offers a more straightforward, cost-effective solution with simpler integration and minimal configuration overhead. If your primary need is PDF generation, conversion, and basic manipulation rather than a full document management platform, IronPDF provides the essential features without unnecessary complexity.

## NuGet Package Changes

```bash
# Remove Apryse (PDFTron)
dotnet remove package PDFTron.NET.x64
# or
dotnet remove package pdftron

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Apryse (PDFTron) | IronPDF |
|------------------|---------|
| `pdftron` | `IronPdf` |
| `pdftron.PDF` | `IronPdf` |
| `pdftron.PDF.Convert` | `IronPdf` |
| `pdftron.SDF` | `IronPdf` |

## API Mapping Table

| Apryse (PDFTron) | IronPDF | Notes |
|------------------|---------|-------|
| `PDFNet.Initialize()` | `License.LicenseKey = "key"` | License configuration |
| `HTML2PDF.Convert()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | HTML to PDF conversion |
| `Convert.ToPdf()` | `ChromePdfRenderer.RenderUrlAsPdf()` | URL to PDF conversion |
| `PDFDoc.Save()` | `PdfDocument.SaveAs()` | Save PDF to file |
| `PDFDoc pdfDoc = new PDFDoc(filename)` | `PdfDocument.FromFile(filename)` | Load existing PDF |
| `Stamper.StampText()` | `PdfDocument.ApplyStamp()` | Add text stamps/watermarks |
| `PDFDoc.GetPageCount()` | `PdfDocument.PageCount` | Get page count |
| `Convert.ToXod()` | `PdfDocument.ToBitmap()` | Convert to image format |
| `Flattener.Flatten()` | `PdfDocument.Flatten()` | Flatten form fields |
| `PDFDoc.AppendPages()` | `PdfDocument.Merge()` | Merge PDFs |

## Code Examples

### Example 1: HTML to PDF Conversion

**Before (Apryse/PDFTron):**

```csharp
using pdftron;
using pdftron.PDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

using (PDFDoc doc = new PDFDoc())
{
    HTML2PDF converter = new HTML2PDF();
    converter.InsertFromURL("https://example.com");
    
    if (converter.Convert(doc))
    {
        doc.Save("output.pdf", SDFDoc.SaveOptions.e_linearized);
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 2: HTML String to PDF

**Before (Apryse/PDFTron):**

```csharp
using pdftron;
using pdftron.PDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

string htmlContent = "<html><body><h1>Hello World</h1></body></html>";

using (PDFDoc doc = new PDFDoc())
{
    HTML2PDF converter = new HTML2PDF();
    converter.SetModulePath("path/to/html2pdf");
    converter.InsertFromHtmlString(htmlContent);
    
    if (converter.Convert(doc))
    {
        doc.Save("output.pdf", SDFDoc.SaveOptions.e_linearized);
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

License.LicenseKey = "YOUR_LICENSE_KEY";

string htmlContent = "<html><body><h1>Hello World</h1></body></html>";

var renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

### Example 3: Merge Multiple PDFs

**Before (Apryse/PDFTron):**

```csharp
using pdftron;
using pdftron.PDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

using (PDFDoc doc1 = new PDFDoc("file1.pdf"))
using (PDFDoc doc2 = new PDFDoc("file2.pdf"))
{
    doc1.AppendPages(doc2, 1, doc2.GetPageCount());
    doc1.Save("merged.pdf", SDFDoc.SaveOptions.e_linearized);
}
```

**After (IronPDF):**

```csharp
using IronPdf;

License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf1 = PdfDocument.FromFile("file1.pdf");
var pdf2 = PdfDocument.FromFile("file2.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

## Common Gotchas

- **No module path required**: IronPDF doesn't require setting module paths or external binaries—everything works out of the box after NuGet installation.
- **License initialization**: IronPDF uses `License.LicenseKey` property instead of `PDFNet.Initialize()`. Set this once at application startup.
- **Rendering engine**: IronPDF uses Chromium for HTML rendering, which may produce slightly different output than Apryse's rendering engine. Test your existing HTML/CSS for compatibility.
- **Method naming**: IronPDF uses more modern C# conventions (e.g., `SaveAs()` instead of `Save()` with parameters).
- **Disposal pattern**: Both libraries use IDisposable, but IronPDF's `PdfDocument` objects should be disposed or allowed to go out of scope properly.
- **File formats**: If you were using Apryse's XOD or other proprietary formats, you'll need to use standard formats like PDF, images (PNG, JPEG), or HTML with IronPDF.
- **Advanced features**: Some enterprise-level Apryse features (like redaction, OCR, or digital signatures) may require additional configuration or separate IronPDF products.

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)