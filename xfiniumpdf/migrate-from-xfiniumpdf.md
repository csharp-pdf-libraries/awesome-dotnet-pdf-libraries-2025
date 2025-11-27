# Migration Guide: XFINIUM.PDF → IronPDF

## Why Migrate to IronPDF

IronPDF offers superior HTML-to-PDF conversion capabilities with full CSS3 and JavaScript support, making it ideal for modern web-based document generation. It has a larger, more active community with extensive documentation, tutorials, and support resources. IronPDF's intuitive API and comprehensive feature set make PDF generation and manipulation significantly easier for .NET developers.

## NuGet Package Changes

```bash
# Remove XFINIUM.PDF
dotnet remove package Xfinium.Pdf

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| XFINIUM.PDF | IronPDF |
|-------------|---------|
| `Xfinium.Pdf` | `IronPdf` |
| `Xfinium.Pdf.Graphics` | `IronPdf.Rendering` |
| `Xfinium.Pdf.Actions` | `IronPdf` |
| `Xfinium.Pdf.Content` | `IronPdf` |
| `Xfinium.Pdf.Forms` | `IronPdf.Forms` |

## API Mapping

| XFINIUM.PDF | IronPDF | Notes |
|-------------|---------|-------|
| `PdfFixedDocument` | `PdfDocument` | Main document class |
| `PdfPage` | `PdfDocument.Pages[index]` | Page access |
| `PdfUnicodeTrueTypeFont` | Built-in font handling | Automatic font management |
| `PdfRgbColor` | CSS color strings | Use standard CSS colors |
| `PdfStringAppearanceOptions` | `ChromePdfRenderOptions` | Rendering configuration |
| `PdfStringLayoutOptions` | HTML/CSS | Use HTML for layout |
| `Save(stream)` | `SaveAs(path)` or `Stream` | Multiple save options |
| `DrawString()` | HTML content | Use HTML instead of graphics |
| `PdfHtmlTextElement` | `ChromeHtmlToPdf` | HTML rendering |

## Code Examples

### Example 1: Creating a Simple PDF Document

**Before (XFINIUM.PDF):**
```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;

PdfFixedDocument document = new PdfFixedDocument();
PdfPage page = document.Pages.Add();

PdfUnicodeTrueTypeFont font = new PdfUnicodeTrueTypeFont("Arial", 12, true);
PdfBrush brush = new PdfBrush(new PdfRgbColor(0, 0, 0));

page.Graphics.DrawString("Hello World", font, brush, 50, 50);

document.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");

pdf.SaveAs("output.pdf");
```

### Example 2: Creating PDF from HTML

**Before (XFINIUM.PDF):**
```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Content;

PdfFixedDocument document = new PdfFixedDocument();
PdfPage page = document.Pages.Add();

string html = "<html><body><h1>Title</h1><p>Content here</p></body></html>";
PdfHtmlTextElement htmlElement = new PdfHtmlTextElement(0, 0, 500, html);
htmlElement.Draw(page);

document.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

string html = "<html><body><h1>Title</h1><p>Content here</p></body></html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);

pdf.SaveAs("output.pdf");
```

### Example 3: Advanced Formatting and Styling

**Before (XFINIUM.PDF):**
```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;

PdfFixedDocument document = new PdfFixedDocument();
PdfPage page = document.Pages.Add();

PdfUnicodeTrueTypeFont titleFont = new PdfUnicodeTrueTypeFont("Arial", 24, true);
PdfUnicodeTrueTypeFont bodyFont = new PdfUnicodeTrueTypeFont("Arial", 12, true);

PdfBrush titleBrush = new PdfBrush(new PdfRgbColor(0, 0, 255));
PdfBrush bodyBrush = new PdfBrush(new PdfRgbColor(0, 0, 0));

page.Graphics.DrawString("Document Title", titleFont, titleBrush, 50, 50);
page.Graphics.DrawString("Body text content", bodyFont, bodyBrush, 50, 100);

PdfPen pen = new PdfPen(new PdfRgbColor(255, 0, 0), 2);
page.Graphics.DrawRectangle(pen, 50, 150, 200, 100);

document.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

string html = @"
<html>
<head>
    <style>
        h1 { color: blue; font-size: 24px; font-family: Arial; }
        p { color: black; font-size: 12px; font-family: Arial; }
        .box { border: 2px solid red; width: 200px; height: 100px; 
               margin-top: 20px; }
    </style>
</head>
<body>
    <h1>Document Title</h1>
    <p>Body text content</p>
    <div class='box'></div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);

pdf.SaveAs("output.pdf");
```

## Common Gotchas

### 1. **HTML-First Approach**
IronPDF is designed for HTML-to-PDF conversion. Instead of programmatic drawing with coordinates, use HTML and CSS for layout and styling. This is more intuitive and maintainable.

### 2. **Font Handling**
IronPDF automatically handles fonts through the HTML rendering engine. You don't need to manually create font objects—just use standard CSS `font-family` declarations.

### 3. **Page Size and Margins**
Configure page settings using `ChromePdfRenderOptions` instead of page constructors:
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
```

### 4. **Coordinate System**
XFINIUM uses direct coordinate positioning, while IronPDF uses HTML flow layout. Embrace CSS positioning (flexbox, grid, absolute positioning) instead of calculating coordinates.

### 5. **Licensing**
IronPDF requires a license key for production use. Set it at application startup:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 6. **Drawing Operations**
IronPDF doesn't have direct drawing APIs like `DrawString()` or `DrawRectangle()`. Use HTML elements and CSS styling instead. For complex graphics, consider using SVG embedded in HTML.

### 7. **Performance Considerations**
IronPDF uses a Chrome rendering engine, which may have different performance characteristics. For high-volume generation, consider using `RenderHtmlFileAsPdf()` with cached HTML files.

## Additional Resources

- **Official Documentation**: https://ironpdf.com/docs/
- **Tutorials and Examples**: https://ironpdf.com/tutorials/
- **API Reference**: https://ironpdf.com/object-reference/api/