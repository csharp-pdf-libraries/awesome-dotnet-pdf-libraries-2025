# Migration Guide: VectSharp → IronPDF

## Why Migrate from VectSharp to IronPDF

VectSharp is designed for vector graphics and scientific visualizations, making it unsuitable for document generation and HTML-based PDF workflows. IronPDF excels at creating PDFs from HTML, URLs, and documents with full support for modern web standards, CSS, and JavaScript. If you need to generate business documents, reports, or convert web content to PDF, IronPDF provides a more appropriate and feature-rich solution.

## NuGet Package Changes

```bash
# Remove VectSharp
dotnet remove package VectSharp
dotnet remove package VectSharp.PDF

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| VectSharp | IronPDF |
|-----------|---------|
| `VectSharp` | `IronPdf` |
| `VectSharp.PDF` | `IronPdf` |
| `VectSharp.Graphics` | `IronPdf` (HTML/CSS based) |
| `VectSharp.Canvas` | Not applicable (use HTML canvas) |

## API Mapping

| VectSharp Concept | IronPDF Equivalent |
|-------------------|-------------------|
| `Document` | `ChromePdfRenderer` |
| `Page` | HTML pages / page breaks |
| `Graphics.FillRectangle()` | HTML `<div>` with CSS styling |
| `Graphics.StrokePath()` | SVG or HTML5 Canvas in HTML |
| `SaveAsPDF()` | `RenderHtmlAsPdf()` / `RenderUrlAsPdf()` |
| Vector drawing commands | HTML/CSS/SVG markup |
| `Colour` | CSS color values |
| Manual page sizing | `PdfPrintOptions` page settings |

## Code Examples

### Example 1: Creating a Simple PDF

**Before (VectSharp):**
```csharp
using VectSharp;
using VectSharp.PDF;

Document doc = new Document();
Page page = new Page(595, 842); // A4 size
Graphics graphics = page.Graphics;

graphics.FillRectangle(50, 50, 200, 100, Colour.FromRgb(0, 0, 255));
graphics.FillText(60, 70, "Hello from VectSharp", 
    new Font(new FontFamily("Arial"), 20), Colour.FromRgb(255, 255, 255));

doc.Pages.Add(page);
doc.SaveAsPDF("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        .box {
            width: 200px;
            height: 100px;
            background-color: blue;
            padding: 20px;
            margin: 50px;
        }
        h1 {
            color: white;
            font-family: Arial;
            font-size: 20px;
        }
    </style>
</head>
<body>
    <div class='box'>
        <h1>Hello from IronPDF</h1>
    </div>
</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: Multi-Page Document

**Before (VectSharp):**
```csharp
using VectSharp;
using VectSharp.PDF;

Document doc = new Document();

for (int i = 1; i <= 3; i++)
{
    Page page = new Page(595, 842);
    Graphics graphics = page.Graphics;
    
    graphics.FillText(50, 50, $"Page {i}", 
        new Font(new FontFamily("Arial"), 24), Colours.Black);
    
    doc.Pages.Add(page);
}

doc.SaveAsPDF("multipage.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial; font-size: 24px; }
        .page-break { page-break-after: always; }
    </style>
</head>
<body>
    <div class='page-break'><h1>Page 1</h1></div>
    <div class='page-break'><h1>Page 2</h1></div>
    <div><h1>Page 3</h1></div>
</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("multipage.pdf");
```

### Example 3: Custom Page Size and Drawing

**Before (VectSharp):**
```csharp
using VectSharp;
using VectSharp.PDF;

Document doc = new Document();
Page page = new Page(800, 600); // Custom size
Graphics graphics = page.Graphics;

// Draw a circle
GraphicsPath path = new GraphicsPath();
path.Arc(400, 300, 100, 0, 2 * Math.PI);
graphics.FillPath(path, Colour.FromRgb(255, 0, 0));

// Add text
graphics.FillText(350, 290, "Circle", 
    new Font(new FontFamily("Arial"), 16), Colours.White);

doc.Pages.Add(page);
doc.SaveAsPDF("custom.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;

var renderer = new ChromePdfRenderer();

// Set custom page size
renderer.RenderingOptions.PaperSize = PdfPaperSize.Custom;
renderer.RenderingOptions.CustomPaperWidth = 800;
renderer.RenderingOptions.CustomPaperHeight = 600;

string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { margin: 0; }
        svg { display: block; }
        text { fill: white; font-family: Arial; font-size: 16px; }
    </style>
</head>
<body>
    <svg width='800' height='600'>
        <circle cx='400' cy='300' r='100' fill='red' />
        <text x='360' y='305'>Circle</text>
    </svg>
</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("custom.pdf");
```

## Common Gotchas

1. **Paradigm Shift**: VectSharp uses imperative drawing commands while IronPDF uses declarative HTML/CSS. You must convert drawing logic to HTML markup and styling.

2. **No Direct Drawing API**: IronPDF doesn't have methods like `FillRectangle()` or `DrawLine()`. Use HTML elements (`<div>`, `<span>`) or SVG for vector graphics within your HTML.

3. **Page Sizing**: VectSharp uses points directly, while IronPDF uses `PaperSize` enums or custom dimensions through `RenderingOptions`. Set page size before rendering.

4. **Font Handling**: VectSharp requires Font objects, while IronPDF uses CSS `font-family`. Ensure fonts are installed on the system or use web fonts.

5. **Coordinate Systems**: VectSharp uses bottom-left origin by default; HTML/CSS uses top-left. Adjust positioning accordingly.

6. **Performance**: IronPDF renders through a browser engine, which may be slower than VectSharp's direct vector drawing for simple graphics but offers much more flexibility.

7. **Licensing**: IronPDF requires a license for commercial use, while VectSharp is open-source. Review licensing requirements at https://ironpdf.com/licensing/.

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **Tutorials & Examples**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/docs/questions/html-to-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/