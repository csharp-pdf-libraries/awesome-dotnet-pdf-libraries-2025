# Migration Guide: BitMiracle Docotic.Pdf → IronPDF

## Why Migrate to IronPDF

IronPDF offers native HTML-to-PDF conversion capabilities, making it ideal for generating PDFs from web content, templates, and dynamic data. With a larger community and extensive documentation, developers benefit from more resources, examples, and support. IronPDF provides a more comprehensive feature set for modern PDF generation workflows while maintaining competitive licensing for commercial applications.

## NuGet Package Changes

```bash
# Remove BitMiracle Docotic.Pdf
dotnet remove package BitMiracle.Docotic.Pdf

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| BitMiracle Docotic.Pdf | IronPDF |
|------------------------|---------|
| `BitMiracle.Docotic.Pdf` | `IronPdf` |
| `BitMiracle.Docotic.Pdf.Layout` | `IronPdf.Rendering` |
| N/A | `IronPdf.Extensions.Mvc` |

## API Mapping

| BitMiracle Docotic.Pdf | IronPDF | Notes |
|------------------------|---------|-------|
| `PdfDocument.CreateEmpty()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | HTML-based approach |
| `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` | Load existing PDF |
| `document.Save(path)` | `pdf.SaveAs(path)` | Save PDF |
| `document.Pages.Add()` | Create via HTML | Page creation differs |
| `document.AddText()` | Use HTML markup | Text rendering |
| `document.TextData.GetText()` | `pdf.ExtractAllText()` | Text extraction |
| `PdfPage.Canvas.DrawText()` | Use HTML/CSS | Drawing operations |
| `document.Metadata` | `pdf.MetaData` | Metadata access |

## Code Examples

### Example 1: Creating a Simple PDF

**Before (BitMiracle Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;

var document = PdfDocument.CreateEmpty();
var page = document.Pages.Add();
var canvas = page.Canvas;

canvas.FontSize = 24;
canvas.DrawText("Hello World", 50, 50);

document.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");

pdf.SaveAs("output.pdf");
```

### Example 2: Reading an Existing PDF

**Before (BitMiracle Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;

using (var document = PdfDocument.Load("input.pdf"))
{
    string text = document.TextData.GetText();
    int pageCount = document.PageCount;
    Console.WriteLine($"Pages: {pageCount}");
    Console.WriteLine($"Text: {text}");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");
string text = pdf.ExtractAllText();
int pageCount = pdf.PageCount;

Console.WriteLine($"Pages: {pageCount}");
Console.WriteLine($"Text: {text}");
```

### Example 3: Creating a Multi-Page Document with Formatting

**Before (BitMiracle Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;

var document = PdfDocument.CreateEmpty();

// Page 1
var page1 = document.Pages.Add();
page1.Canvas.FontSize = 18;
page1.Canvas.DrawText("Page 1 - Title", 50, 50);
page1.Canvas.FontSize = 12;
page1.Canvas.DrawText("Content here...", 50, 100);

// Page 2
var page2 = document.Pages.Add();
page2.Canvas.FontSize = 18;
page2.Canvas.DrawText("Page 2 - Title", 50, 50);

document.Save("multi-page.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var html = @"
<div style='page-break-after: always;'>
    <h2>Page 1 - Title</h2>
    <p>Content here...</p>
</div>
<div>
    <h2>Page 2 - Title</h2>
</div>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);

pdf.SaveAs("multi-page.pdf");
```

## Common Gotchas

### 1. **Paradigm Shift: Programmatic vs HTML-Based**
IronPDF is primarily HTML-based. Instead of drawing elements programmatically with coordinates, design your PDF content using HTML and CSS. This offers more flexibility but requires thinking in web layout terms.

### 2. **Page Size and Margins**
Set page dimensions using `RenderingOptions` rather than programmatic page creation:
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
```

### 3. **Font Handling**
IronPDF uses system fonts or web fonts through HTML/CSS. You don't need to register fonts programmatically as with Docotic.Pdf—simply reference them in your CSS.

### 4. **Coordinate Systems**
Docotic.Pdf uses bottom-left origin coordinates. IronPDF uses HTML layout, which flows top-to-bottom. Use CSS positioning (`position: absolute`) if precise placement is needed.

### 5. **Licensing**
Ensure you apply your IronPDF license key at application startup:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 6. **Text Extraction Differences**
Text extraction may yield slightly different results due to different PDF engines. Test thoroughly if text extraction is critical to your application.

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/docs/questions/html-to-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/