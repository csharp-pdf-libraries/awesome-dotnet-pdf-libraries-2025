# Migration Guide: Telerik Document Processing → IronPDF

## Why Migrate to IronPDF

IronPDF provides production-grade HTML-to-PDF rendering with full support for modern CSS frameworks including Bootstrap, Flexbox, and Grid layouts. Unlike Telerik's limited CSS parser, IronPDF uses a Chromium-based rendering engine that handles complex stylesheets, external CSS files, and responsive designs exactly as they appear in browsers. IronPDF eliminates common memory issues and provides reliable document generation without the parsing limitations that plague Telerik's flow document model.

## NuGet Package Changes

```bash
# Remove Telerik packages
dotnet remove package Telerik.Documents.Core
dotnet remove package Telerik.Documents.Flow
dotnet remove package Telerik.Documents.Flow.FormatProviders.Pdf
dotnet remove package Telerik.Documents.Fixed

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Telerik Document Processing | IronPDF |
|----------------------------|---------|
| `Telerik.Windows.Documents.Flow.Model` | `IronPdf` |
| `Telerik.Windows.Documents.Flow.FormatProviders.Html` | `IronPdf` |
| `Telerik.Windows.Documents.Flow.FormatProviders.Pdf` | `IronPdf` |
| `Telerik.Windows.Documents.Fixed.Model` | `IronPdf` |
| `Telerik.Documents.Primitives` | `IronPdf` (built-in) |

## API Mapping

| Telerik API | IronPDF API | Notes |
|------------|-------------|-------|
| `HtmlFormatProvider` | `ChromePdfRenderer` | Full CSS support, no div→paragraph conversion |
| `PdfFormatProvider` | `PdfDocument` | Direct PDF manipulation |
| `RadFlowDocument` | `PdfDocument` | No intermediate flow model needed |
| `HtmlImportSettings` | `RenderingOptions` | Comprehensive rendering options |
| `PdfExportSettings` | `PdfPrintOptions` | Page size, margins, headers/footers |
| `Section.Blocks.AddParagraph()` | Direct HTML rendering | No manual block construction |
| `RadFlowDocumentEditor` | `PdfDocument` methods | Merge, split, edit operations |
| `Bookmark` | Native PDF bookmarks | Fully supported |
| `Hyperlink` | Native HTML links | Preserved in PDF |

## Code Examples

### Example 1: HTML to PDF Conversion

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;

// Broken: Bootstrap divs become paragraphs, CSS ignored
HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
RadFlowDocument document = htmlProvider.Import(htmlContent);

PdfFormatProvider pdfProvider = new PdfFormatProvider();
byte[] pdfBytes = pdfProvider.Export(document);
File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

// Full Bootstrap and modern CSS support
var renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

### Example 2: HTML File with External CSS

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;

// External CSS partially supported, complex selectors fail
HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
HtmlImportSettings settings = new HtmlImportSettings();
htmlProvider.ImportSettings = settings;

string html = File.ReadAllText("document.html");
RadFlowDocument document = htmlProvider.Import(html);

PdfFormatProvider pdfProvider = new PdfFormatProvider();
byte[] output = pdfProvider.Export(document);
```

**After (IronPDF):**
```csharp
using IronPdf;

// External CSS fully supported, resolved like in browsers
var renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlFileAsPdf("document.html");
pdf.SaveAs("output.pdf");
```

### Example 3: Custom Page Settings and Headers

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Documents.Primitives;

HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
RadFlowDocument document = htmlProvider.Import(htmlContent);

PdfExportSettings exportSettings = new PdfExportSettings();
exportSettings.ImageQuality = ImageQuality.High;

// Manual header/footer construction required
Section section = document.Sections.First();
Header header = section.Headers.Add();
Paragraph headerParagraph = header.Blocks.AddParagraph();
headerParagraph.Inlines.AddText("Company Header");

PdfFormatProvider pdfProvider = new PdfFormatProvider();
pdfProvider.ExportSettings = exportSettings;
byte[] output = pdfProvider.Export(document);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 40;
renderer.RenderingOptions.MarginBottom = 40;

// Simple HTML-based headers/footers
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:center'>Company Header</div>"
};

PdfDocument pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

## Common Gotchas

### 1. No Flow Document Model
IronPDF renders HTML directly to PDF without an intermediate flow document. You don't need to construct `Section`, `Paragraph`, or `Block` objects manually. Write HTML/CSS instead.

### 2. CSS Just Works
Don't try to work around CSS limitations—IronPDF supports modern CSS including:
- Bootstrap grid system (no more div→paragraph flattening)
- Flexbox and CSS Grid layouts
- `display: inline`, `display: block`, `display: flex`
- Complex selectors and pseudo-elements
- External stylesheets via `<link>` tags

### 3. Memory Management
IronPDF handles large documents without the `OutOfMemoryException` issues reported with Telerik's `XlsFormatProvider`. No special configuration needed for standard document sizes.

### 4. URL vs. File vs. String
IronPDF offers explicit methods:
- `RenderHtmlAsPdf(string html)` for HTML strings
- `RenderHtmlFileAsPdf(string filePath)` for HTML files
- `RenderUrlAsPdf(string url)` for web pages

Choose the appropriate method instead of relying on a single `Import()` method.

### 5. PDF Operations Are Direct
Merging, splitting, and editing PDFs work directly on `PdfDocument` objects:
```csharp
var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
var pages = pdf.CopyPages(0, 5); // Extract first 5 pages
```

### 6. Licensing
IronPDF requires a license key for production use. Set it at application startup:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 7. Bookmarks and Hyperlinks
These work automatically in IronPDF. HTML anchor tags (`<a href="#section">`) and IDs (`<div id="section">`) are preserved as PDF bookmarks and internal links—no special configuration required.

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **API Reference**: https://ironpdf.com/docs/api/
- **Code Examples**: https://ironpdf.com/examples/