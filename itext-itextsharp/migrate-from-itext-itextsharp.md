# Migration Guide: iText / iTextSharp → IronPDF

## Why Migrate from iText/iTextSharp to IronPDF

iText's AGPL license requires you to open-source your entire application if used in web apps, creating significant legal and business risks. IronPDF offers a straightforward commercial license without viral open-source requirements, includes native HTML-to-PDF rendering without additional add-ons, and provides perpetual licensing options instead of forcing annual subscriptions. The modern API is also simpler and more intuitive for common PDF tasks.

## NuGet Package Changes

```bash
# Remove iText packages
dotnet remove package itext7
dotnet remove package itextsharp

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| iText / iTextSharp | IronPDF |
|-------------------|---------|
| `iText.Kernel.Pdf` | `IronPdf` |
| `iText.Layout` | `IronPdf` |
| `iText.Layout.Element` | `IronPdf` |
| `iText.Html2Pdf` | `IronPdf` (built-in) |
| `iTextSharp.text` | `IronPdf` |
| `iTextSharp.text.pdf` | `IronPdf` |

## API Mapping Table

| Task | iText 7 / iTextSharp | IronPDF |
|------|---------------------|---------|
| HTML to PDF | `HtmlConverter.ConvertToPdf()` + pdfHTML | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| Create PDF from scratch | `PdfDocument` + `Document` | `PdfDocument.FromHtml()` |
| Open existing PDF | `new PdfDocument(new PdfReader())` | `PdfDocument.FromFile()` |
| Save PDF | `document.Close()` | `pdf.SaveAs()` |
| Merge PDFs | `PdfDocument.CopyPagesTo()` | `PdfDocument.Merge()` |
| Add text/content | `document.Add(new Paragraph())` | Use HTML/CSS rendering |
| Set metadata | `pdfDoc.GetDocumentInfo()` | `pdf.MetaData` |
| Add watermark | Manual positioning | `pdf.ApplyWatermark()` |
| Password protect | `WriterProperties.SetStandardEncryption()` | `pdf.Password()` |
| Extract text | `PdfTextExtractor.GetTextFromPage()` | `pdf.ExtractAllText()` |

## Code Examples

### Example 1: HTML to PDF Conversion

**Before (iText 7 with pdfHTML):**
```csharp
using iText.Html2Pdf;
using iText.Kernel.Pdf;
using System.IO;

// Requires separate pdfHTML add-on license
string html = "<h1>Hello World</h1><p>This is a PDF document.</p>";
using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
{
    ConverterProperties properties = new ConverterProperties();
    HtmlConverter.ConvertToPdf(html, fs, properties);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

// Built-in HTML rendering, no add-ons needed
string html = "<h1>Hello World</h1><p>This is a PDF document.</p>";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: Creating PDF from URL

**Before (iText 7):**
```csharp
using iText.Html2Pdf;
using iText.Kernel.Pdf;
using System.Net;

string url = "https://example.com";
WebClient client = new WebClient();
string html = client.DownloadString(url);

using (FileStream fs = new FileStream("webpage.pdf", FileMode.Create))
{
    HtmlConverter.ConvertToPdf(html, fs);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

string url = "https://example.com";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf(url);
pdf.SaveAs("webpage.pdf");
```

### Example 3: Merging PDFs

**Before (iText 7):**
```csharp
using iText.Kernel.Pdf;
using iText.Kernel.Utils;

using (PdfDocument pdfDoc = new PdfDocument(new PdfWriter("merged.pdf")))
{
    PdfMerger merger = new PdfMerger(pdfDoc);
    
    using (PdfDocument firstDoc = new PdfDocument(new PdfReader("first.pdf")))
    {
        merger.Merge(firstDoc, 1, firstDoc.GetNumberOfPages());
    }
    
    using (PdfDocument secondDoc = new PdfDocument(new PdfReader("second.pdf")))
    {
        merger.Merge(secondDoc, 1, secondDoc.GetNumberOfPages());
    }
    
    merger.Close();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("first.pdf");
var pdf2 = PdfDocument.FromFile("second.pdf");
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

## Common Gotchas

### 1. **Rendering Engine Difference**
- **iText**: Low-level PDF construction; HTML support requires parsing and manual layout
- **IronPDF**: Uses Chromium engine for pixel-perfect HTML/CSS rendering
- **Solution**: Embrace HTML/CSS for layouts instead of absolute positioning

### 2. **Document Creation Paradigm**
- **iText**: Programmatic element-by-element construction (`Paragraph`, `Table`, etc.)
- **IronPDF**: HTML-first approach with CSS for styling
- **Solution**: Generate HTML templates (Razor, string interpolation) instead of procedural code

### 3. **File Locking**
- **iText**: Explicit `Close()` or `using` statements required
- **IronPDF**: Objects are disposable; use `using` or explicit `Dispose()`
- **Solution**: Wrap `PdfDocument` in `using` blocks for proper cleanup

### 4. **Coordinate Systems**
- **iText**: Requires understanding of PDF coordinate system (bottom-left origin)
- **IronPDF**: Uses standard web coordinates through HTML/CSS
- **Solution**: Use CSS positioning (`margin`, `padding`, `position`) instead of coordinates

### 5. **Font Embedding**
- **iText**: Manual font registration and embedding
- **IronPDF**: Automatic system font detection and web font support
- **Solution**: Use standard fonts or `@font-face` in CSS

### 6. **License Key Configuration**
- **IronPDF**: Requires license key to be set before rendering
- **Solution**: Set `IronPdf.License.LicenseKey = "YOUR-KEY"` at application startup

### 7. **Async Operations**
- **iText**: Mostly synchronous API
- **IronPDF**: Supports async with `RenderHtmlAsPdfAsync()`, `RenderUrlAsPdfAsync()`
- **Solution**: Use async methods in web applications for better scalability

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Support**: https://ironpdf.com/support/