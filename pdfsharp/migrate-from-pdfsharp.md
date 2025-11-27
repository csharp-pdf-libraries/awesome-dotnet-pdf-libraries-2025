# Migration Guide: PDFSharp → IronPDF

## Why Migrate from PDFSharp to IronPDF

PDFSharp requires manual positioning of every element using GDI+ style coordinates, making document generation tedious and error-prone. IronPDF supports native HTML-to-PDF conversion with modern CSS3 (including flexbox and grid), allowing you to leverage web technologies instead of calculating X,Y positions. This dramatically reduces development time and makes PDF generation maintainable through standard HTML/CSS skills.

## NuGet Package Changes

```bash
# Remove PDFSharp
dotnet remove package PdfSharp

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| PDFSharp | IronPDF |
|----------|---------|
| `PdfSharp.Pdf` | `IronPdf` |
| `PdfSharp.Drawing` | Not needed (use HTML/CSS) |
| `PdfSharp.Pdf.IO` | `IronPdf` |

## API Mapping

| PDFSharp API | IronPDF API | Notes |
|--------------|-------------|-------|
| `new PdfDocument()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Create from HTML instead of empty document |
| `document.AddPage()` | Not needed | Pages created automatically from HTML content |
| `XGraphics.DrawString()` | Use HTML `<p>`, `<h1>`, etc. | Position with CSS instead of coordinates |
| `XGraphics.DrawImage()` | Use HTML `<img>` tag | Position with CSS |
| `XFont` | Use CSS `font-family`, `font-size` | Standard CSS font properties |
| `XBrush`, `XPen` | Use CSS colors/borders | `color`, `background-color`, `border` |
| `document.Save()` | `pdf.SaveAs()` | Similar save functionality |
| `PdfReader.Open()` | `PdfDocument.FromFile()` | Open existing PDF |

## Code Examples

### Example 1: Simple Document with Text

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;

PdfDocument document = new PdfDocument();
PdfPage page = document.AddPage();
XGraphics gfx = XGraphics.FromPdfPage(page);
XFont font = new XFont("Arial", 20);

gfx.DrawString("Hello, World!", font, XBrushes.Black,
    new XRect(50, 50, page.Width, page.Height),
    XStringFormats.TopLeft);

document.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello, World!</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: Styled Document with Multiple Elements

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;

PdfDocument document = new PdfDocument();
PdfPage page = document.AddPage();
XGraphics gfx = XGraphics.FromPdfPage(page);

XFont titleFont = new XFont("Arial", 24, XFontStyle.Bold);
XFont bodyFont = new XFont("Arial", 12);

gfx.DrawString("Invoice", titleFont, XBrushes.Black,
    new XRect(50, 50, page.Width, page.Height),
    XStringFormats.TopLeft);

gfx.DrawString("Customer: John Doe", bodyFont, XBrushes.Black,
    new XRect(50, 100, page.Width, page.Height),
    XStringFormats.TopLeft);

gfx.DrawString("Total: $150.00", bodyFont, XBrushes.Black,
    new XRect(50, 130, page.Width, page.Height),
    XStringFormats.TopLeft);

document.Save("invoice.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

string html = @"
<html>
<head>
    <style>
        h1 { font-size: 24px; font-weight: bold; }
        p { font-size: 12px; margin: 10px 0; }
    </style>
</head>
<body>
    <h1>Invoice</h1>
    <p>Customer: John Doe</p>
    <p>Total: $150.00</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

### Example 3: Document with Images

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;

PdfDocument document = new PdfDocument();
PdfPage page = document.AddPage();
XGraphics gfx = XGraphics.FromPdfPage(page);

XFont font = new XFont("Arial", 16);
gfx.DrawString("Company Report", font, XBrushes.Black,
    new XRect(50, 50, page.Width, page.Height),
    XStringFormats.TopLeft);

XImage image = XImage.FromFile("logo.png");
gfx.DrawImage(image, 50, 100, 150, 75);

document.Save("report.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

string html = @"
<html>
<head>
    <style>
        h1 { font-size: 16px; }
        img { width: 150px; height: 75px; }
    </style>
</head>
<body>
    <h1>Company Report</h1>
    <img src='logo.png' alt='Logo' />
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

## Common Gotchas

### 1. **Coordinate System vs. Flow Layout**
- **PDFSharp:** You must calculate exact X,Y coordinates for positioning
- **IronPDF:** Uses HTML flow layout; elements position automatically. Use CSS for precise control.

### 2. **Page Breaks**
- **PDFSharp:** Manually create new pages and track content overflow
- **IronPDF:** Automatic page breaks. Control with CSS: `page-break-before`, `page-break-after`, `page-break-inside: avoid`

### 3. **Fonts**
- **PDFSharp:** Must instantiate `XFont` objects and embed fonts manually
- **IronPDF:** Use standard CSS `@font-face` or system fonts via `font-family`

### 4. **File Paths for Images**
- **PDFSharp:** Requires full file system paths
- **IronPDF:** Supports relative paths, URLs, or base64-encoded images in `src` attributes

### 5. **Tables and Layouts**
- **PDFSharp:** Must calculate cell positions and draw borders manually
- **IronPDF:** Use HTML `<table>` or CSS Grid/Flexbox for complex layouts

### 6. **License Requirements**
- **PDFSharp:** Open source (MIT license)
- **IronPDF:** Commercial product; requires license for production use (free trial available)

### 7. **Drawing Operations**
- **PDFSharp:** Direct graphics operations (`DrawLine`, `DrawRectangle`)
- **IronPDF:** Use CSS borders, SVG, or HTML5 Canvas for graphics

## Additional Resources

- **Official Documentation:** https://ironpdf.com/docs/
- **Tutorials and Examples:** https://ironpdf.com/tutorials/
- **API Reference:** https://ironpdf.com/object-reference/api/