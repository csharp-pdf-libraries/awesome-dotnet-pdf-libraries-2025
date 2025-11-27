# Migration Guide: ZetPDF → IronPDF

## Why Migrate from ZetPDF to IronPDF

ZetPDF, built on PDFSharp, inherits significant limitations including the lack of HTML-to-PDF conversion and restricted modern PDF features. IronPDF provides enterprise-grade HTML-to-PDF rendering using Chromium, supports advanced PDF manipulation, and offers extensive documentation and support. Migrating eliminates the need for workarounds and provides direct access to a more capable PDF library.

## NuGet Package Changes

```bash
# Remove ZetPDF
dotnet remove package ZetPDF

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| ZetPDF | IronPDF |
|--------|---------|
| `ZetPdf` | `IronPdf` |
| `ZetPdf.Drawing` | `IronPdf.Rendering` |
| `ZetPdf.Fonts` | `IronPdf.Rendering` (built-in font support) |

## API Mapping Table

| ZetPDF | IronPDF | Notes |
|--------|---------|-------|
| `PdfDocument` | `PdfDocument` | Core document class |
| `PdfDocument.AddPage()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | IronPDF uses HTML rendering |
| `PdfDocument.Save()` | `PdfDocument.SaveAs()` | Different method name |
| `PdfDocument.Pages` | `PdfDocument.Pages` | Similar page collection |
| `XGraphics` | N/A | Use HTML/CSS instead |
| `XFont` | N/A | Use CSS font styling |
| `XBrush` | N/A | Use CSS colors |
| `PdfPage` | `PdfPage` | Page access |
| N/A | `ChromePdfRenderer` | New: HTML-to-PDF engine |
| N/A | `PdfDocument.Merge()` | New: PDF merging |

## Code Examples

### Example 1: Creating a Simple PDF

**Before (ZetPDF):**
```csharp
using ZetPdf;
using ZetPdf.Drawing;

var document = new PdfDocument();
var page = document.AddPage();
var graphics = XGraphics.FromPdfPage(page);
var font = new XFont("Arial", 20);

graphics.DrawString("Hello World!", font, XBrushes.Black, 
    new XPoint(50, 50));

document.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World!</h1>");

pdf.SaveAs("output.pdf");
```

### Example 2: Creating Multi-Page Documents

**Before (ZetPDF):**
```csharp
using ZetPdf;
using ZetPdf.Drawing;

var document = new PdfDocument();

// Page 1
var page1 = document.AddPage();
var gfx1 = XGraphics.FromPdfPage(page1);
gfx1.DrawString("Page 1", new XFont("Arial", 16), 
    XBrushes.Black, new XPoint(50, 50));

// Page 2
var page2 = document.AddPage();
var gfx2 = XGraphics.FromPdfPage(page2);
gfx2.DrawString("Page 2", new XFont("Arial", 16), 
    XBrushes.Black, new XPoint(50, 50));

document.Save("multi-page.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var html = @"
<div style='page-break-after: always;'>
    <h1>Page 1</h1>
</div>
<div>
    <h1>Page 2</h1>
</div>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);

pdf.SaveAs("multi-page.pdf");
```

### Example 3: Loading and Modifying Existing PDFs

**Before (ZetPDF):**
```csharp
using ZetPdf;
using ZetPdf.IO;
using ZetPdf.Drawing;

var document = PdfReader.Open("existing.pdf", PdfDocumentOpenMode.Modify);
var page = document.Pages[0];
var gfx = XGraphics.FromPdfPage(page);

gfx.DrawString("CONFIDENTIAL", new XFont("Arial", 48), 
    XBrushes.Red, new XPoint(100, 100));

document.Save("modified.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

var pdf = PdfDocument.FromFile("existing.pdf");

var stamper = new TextStamper
{
    Text = "CONFIDENTIAL",
    FontSize = 48,
    FontFamily = "Arial",
    HorizontalOffset = 100,
    VerticalOffset = 100
};

pdf.ApplyStamp(stamper);
pdf.SaveAs("modified.pdf");
```

## Common Gotchas

### 1. Graphics Drawing vs HTML Rendering
ZetPDF uses imperative graphics commands while IronPDF uses declarative HTML/CSS. Learn basic HTML if unfamiliar.

### 2. Coordinate Systems
ZetPDF uses point-based coordinates. IronPDF uses HTML layout with CSS positioning (pixels, percentages, etc.).

### 3. Font Handling
ZetPDF requires explicit font objects. IronPDF uses CSS font-family declarations and automatically handles system fonts.

### 4. Page Breaks
ZetPDF requires manual page creation. IronPDF handles page breaks automatically or via CSS `page-break-after`/`page-break-before`.

### 5. Licensing
IronPDF requires a license for commercial use. Free for development. Set license key:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 6. Dependencies
IronPDF packages Chromium renderer (~200MB). Ensure sufficient disk space and deployment pipeline accommodates larger package size.

### 7. Text Measurement
ZetPDF provides `MeasureString()` for layout calculations. In IronPDF, use HTML/CSS for layout instead of programmatic measurement.

## Additional Resources

- **Documentation:** https://ironpdf.com/docs/
- **Tutorials:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/docs/questions/html-to-pdf/
- **API Reference:** https://ironpdf.com/object-reference/api/