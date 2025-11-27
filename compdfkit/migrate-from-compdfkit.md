# Migration Guide: ComPDFKit → IronPDF

## Why Migrate from ComPDFKit to IronPDF

IronPDF offers a more mature and battle-tested solution with extensive documentation, a large active community, and comprehensive Stack Overflow support. With over a decade in the market, IronPDF provides enterprise-grade stability and a proven track record across thousands of production environments. The library's intuitive API and rich feature set make PDF generation and manipulation more straightforward with better long-term support.

## NuGet Package Changes

```bash
# Remove ComPDFKit
dotnet remove package ComPDFKit.NetCore

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| ComPDFKit | IronPDF |
|-----------|---------|
| `ComPDFKit.PDFDocument` | `IronPdf` |
| `ComPDFKit.PDFPage` | `IronPdf.PdfDocument` |
| `ComPDFKit.Import` | `IronPdf.ChromePdfRenderer` |
| `ComPDFKit.Conversion` | `IronPdf.Rendering` |

## API Mapping

| ComPDFKit API | IronPDF API | Notes |
|---------------|-------------|-------|
| `CPDFDocument.Open()` | `PdfDocument.FromFile()` | Load existing PDF |
| `CPDFDocument.Create()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Create new PDF |
| `CPDFDocument.Save()` | `PdfDocument.SaveAs()` | Save PDF file |
| `CPDFPage.GetTextPage()` | `PdfDocument.ExtractAllText()` | Extract text content |
| `CPDFDocument.GetPageCount()` | `PdfDocument.PageCount` | Get page count (property) |
| `CPDFDocument.ImportPages()` | `PdfDocument.Merge()` | Merge PDFs |
| `CPDFConverter.ConvertToPDF()` | `ImageToPdfConverter.ImageToPdf()` | Convert images to PDF |
| `CPDFPage.RenderPageBitmap()` | `PdfDocument.RasterizeToImageFiles()` | Render pages as images |

## Code Examples

### Example 1: Creating a PDF from HTML

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;
using ComPDFKit.Import;

var document = CPDFDocument.Create();
var htmlConverter = new CPDFHTMLConverter();
htmlConverter.SetHTML("<h1>Hello World</h1>");
htmlConverter.ConvertToPDF(document);
document.Save("output.pdf");
document.Release();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: Extracting Text from PDF

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;

var document = CPDFDocument.Open("input.pdf");
var page = document.GetPage(0);
var textPage = page.GetTextPage();
string extractedText = textPage.GetText(0, textPage.CountChars);
document.Release();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");
string extractedText = pdf.ExtractAllText();
```

### Example 3: Merging Multiple PDFs

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;

var mainDocument = CPDFDocument.Open("first.pdf");
var secondDocument = CPDFDocument.Open("second.pdf");
mainDocument.ImportPages(secondDocument, "0-" + (secondDocument.GetPageCount() - 1));
mainDocument.Save("merged.pdf");
mainDocument.Release();
secondDocument.Release();
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

- **License Key Setup**: IronPDF requires a license key to be set before use. Set it at application startup: `IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";`

- **Disposal Pattern**: IronPDF uses standard .NET IDisposable patterns. ComPDFKit uses `Release()` methods - replace these with `using` statements or explicit `Dispose()` calls in IronPDF.

- **Page Indexing**: Both libraries use zero-based indexing, but IronPDF's page operations are more intuitive with built-in collections and LINQ support.

- **HTML Rendering Engine**: IronPDF uses a Chromium-based renderer which provides superior HTML/CSS support compared to ComPDFKit's converter, but may have different rendering results for complex layouts.

- **Async Operations**: IronPDF provides async versions of most operations (e.g., `RenderHtmlAsPdfAsync()`), whereas ComPDFKit primarily uses synchronous methods.

- **Memory Management**: IronPDF handles memory management automatically in most cases. Remove explicit memory cleanup code from ComPDFKit that called `Release()` or similar methods.

- **Thread Safety**: IronPDF renderer instances are not thread-safe. Create separate renderer instances for concurrent operations, unlike ComPDFKit's document-level threading model.

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)