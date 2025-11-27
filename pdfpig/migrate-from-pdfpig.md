# Migration Guide: PdfPig → IronPDF

## Why Migrate

PdfPig is excellent for reading and extracting content from PDFs but lacks robust document creation and HTML-to-PDF conversion capabilities. IronPDF provides comprehensive PDF generation from HTML, advanced creation features, and full manipulation capabilities alongside extraction features. This migration is ideal when you need to generate professional PDFs from web content or require more advanced document creation beyond basic text extraction.

## NuGet Package Changes

```bash
# Remove PdfPig
dotnet remove package PdfPig

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| PdfPig | IronPDF |
|--------|---------|
| `UglyToad.PdfPig` | `IronPdf` |
| `UglyToad.PdfPig.Content` | `IronPdf` |
| `UglyToad.PdfPig.Writer` | `IronPdf` |
| `UglyToad.PdfPig.DocumentLayoutAnalysis` | `IronPdf.Rendering` |

## API Mapping

| PdfPig | IronPDF | Notes |
|--------|---------|-------|
| `PdfDocument.Open()` | `PdfDocument.FromFile()` | Opens existing PDF |
| `document.GetPage(n)` | `pdf.ExtractTextFromPage(n)` | Access page content |
| `page.Text` | `pdf.ExtractAllText()` | Extract text |
| `PdfDocumentBuilder` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Create PDF (IronPDF uses HTML) |
| `document.NumberOfPages` | `pdf.PageCount` | Get page count |
| N/A | `ChromePdfRenderer.RenderUrlAsPdf()` | Convert URL to PDF |
| N/A | `pdf.SaveAs()` | Save document |

## Code Examples

### Example 1: Extract Text from PDF

**Before (PdfPig):**
```csharp
using UglyToad.PdfPig;

using (PdfDocument document = PdfDocument.Open("input.pdf"))
{
    foreach (var page in document.GetPages())
    {
        string text = page.Text;
        Console.WriteLine(text);
    }
    
    int pageCount = document.NumberOfPages;
    Console.WriteLine($"Total pages: {pageCount}");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");
string allText = pdf.ExtractAllText();
Console.WriteLine(allText);

int pageCount = pdf.PageCount;
Console.WriteLine($"Total pages: {pageCount}");
```

### Example 2: Create PDF from HTML

**Before (PdfPig):**
```csharp
using UglyToad.PdfPig.Writer;

// PdfPig has limited creation - requires manual positioning
PdfDocumentBuilder builder = new PdfDocumentBuilder();
PdfPageBuilder page = builder.AddPage(PageSize.A4);

// Very basic text positioning only
page.AddText("Hello World", 12, new PdfPoint(50, 800), 
    PdfDocumentBuilder.AddedFont.Helvetica);

byte[] pdf = builder.Build();
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

// Rich HTML support with CSS styling
var renderer = new ChromePdfRenderer();
string html = @"
    <html>
        <body>
            <h1>Hello World</h1>
            <p>Full HTML and CSS support!</p>
        </body>
    </html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 3: Convert URL to PDF

**Before (PdfPig):**
```csharp
// Not supported - PdfPig cannot convert URLs or HTML to PDF
// Would need to use another library or manual HTTP client + HTML parsing
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");

// Additional options available
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
```

## Common Gotchas

- **License Required**: IronPDF requires a license for commercial use (free trial available). PdfPig is open-source and free.
- **Creation Paradigm Shift**: PdfPig uses low-level PDF construction while IronPDF uses HTML/CSS rendering. You'll need to convert coordinate-based layouts to HTML markup.
- **Page Indexing**: IronPDF uses 0-based indexing for pages (`ExtractTextFromPage(0)` for first page), while PdfPig's `GetPage()` uses 1-based indexing.
- **Memory Usage**: IronPDF uses Chrome rendering engine which requires more memory than PdfPig's parser. Plan resources accordingly.
- **Text Extraction Differences**: Text extraction may produce slightly different results due to different parsing algorithms. Always test critical extraction logic.
- **No Streaming API**: Unlike PdfPig's page-by-page processing, IronPDF loads entire documents. Handle large files with `RenderingOptions.CreatePdfFormsFromHtml = false` for better performance.
- **Platform Dependencies**: IronPDF requires platform-specific binaries (Windows/Linux/macOS). Ensure correct runtime packages are installed.

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **API Reference**: https://ironpdf.com/docs/questions/api-reference/