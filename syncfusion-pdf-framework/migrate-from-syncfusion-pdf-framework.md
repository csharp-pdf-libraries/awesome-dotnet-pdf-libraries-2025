# Migration Guide: Syncfusion PDF Framework → IronPDF

## Why Migrate to IronPDF

IronPDF offers straightforward, standalone licensing without forcing you to purchase an entire suite of components. Unlike Syncfusion's restrictive community license (requiring both <$1M revenue AND <5 developers), IronPDF provides flexible licensing options suitable for teams of any size. IronPDF also simplifies PDF generation with its HTML-to-PDF rendering engine, eliminating complex positioning and layout code.

## NuGet Package Changes

```bash
# Remove Syncfusion package
dotnet remove package Syncfusion.Pdf.Net.Core

# Add IronPDF package
dotnet add package IronPdf
```

## Namespace Mapping

| Syncfusion PDF Framework | IronPDF |
|-------------------------|---------|
| `Syncfusion.Pdf` | `IronPdf` |
| `Syncfusion.Pdf.Graphics` | `IronPdf` |
| `Syncfusion.Pdf.Grid` | `IronPdf` (use HTML tables) |
| `Syncfusion.Drawing` | N/A (use HTML/CSS) |

## API Mapping

| Syncfusion PDF Framework | IronPDF | Notes |
|-------------------------|---------|-------|
| `PdfDocument` | `PdfDocument` | Core document class |
| `PdfPage.Add()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Use HTML instead of manual positioning |
| `PdfGraphics.DrawString()` | Use HTML content | HTML-first approach |
| `PdfGrid` | Use HTML `<table>` | More flexible with CSS |
| `PdfDocument.Save(stream)` | `PdfDocument.SaveAs()` | Similar functionality |
| `PdfLoadedDocument` | `PdfDocument.FromFile()` | Load existing PDFs |
| `PdfTextExtractor` | `PdfDocument.ExtractAllText()` | Text extraction |
| `PdfDocumentBase.Pages.Add()` | `PdfDocument.AppendPdf()` | Add pages/merge |

## Code Examples

### Example 1: Creating a Simple PDF Document

**Before (Syncfusion):**
```csharp
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;

// Create a new PDF document
PdfDocument document = new PdfDocument();
PdfPage page = document.Pages.Add();

// Create fonts and brushes
PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);
PdfBrush brush = new PdfSolidBrush(Color.Black);

// Draw text with manual positioning
page.Graphics.DrawString("Hello, World!", font, brush, new PointF(10, 10));

// Save the document
using (FileStream stream = new FileStream("output.pdf", FileMode.Create))
{
    document.Save(stream);
}
document.Close(true);
```

**After (IronPDF):**
```csharp
using IronPdf;

// Create PDF from HTML
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello, World!</h1>");

// Save the document
pdf.SaveAs("output.pdf");
```

### Example 2: Creating a PDF with Table Data

**Before (Syncfusion):**
```csharp
using Syncfusion.Pdf;
using Syncfusion.Pdf.Grid;
using Syncfusion.Pdf.Graphics;

PdfDocument document = new PdfDocument();
PdfPage page = document.Pages.Add();

// Create a grid
PdfGrid grid = new PdfGrid();
grid.DataSource = GetDataTable(); // Your data source

// Style the grid
PdfGridCellStyle headerStyle = new PdfGridCellStyle();
headerStyle.BackgroundBrush = new PdfSolidBrush(new PdfColor(68, 114, 196));
headerStyle.TextBrush = PdfBrushes.White;
grid.Headers[0].Style = headerStyle;

// Draw grid
grid.Draw(page, new PointF(10, 10));

using (FileStream stream = new FileStream("table.pdf", FileMode.Create))
{
    document.Save(stream);
}
document.Close(true);
```

**After (IronPDF):**
```csharp
using IronPdf;

string htmlTable = @"
<html>
<head>
    <style>
        table { border-collapse: collapse; width: 100%; }
        th { background-color: #4472C4; color: white; padding: 10px; }
        td { border: 1px solid #ddd; padding: 8px; }
    </style>
</head>
<body>
    <table>
        <tr><th>Name</th><th>Age</th><th>City</th></tr>
        <tr><td>John</td><td>30</td><td>New York</td></tr>
        <tr><td>Jane</td><td>25</td><td>London</td></tr>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlTable);
pdf.SaveAs("table.pdf");
```

### Example 3: Modifying Existing PDFs

**Before (Syncfusion):**
```csharp
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Graphics;

// Load existing PDF
PdfLoadedDocument loadedDocument = new PdfLoadedDocument("existing.pdf");

// Get the first page
PdfLoadedPage page = loadedDocument.Pages[0] as PdfLoadedPage;

// Add text to existing page
PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
page.Graphics.DrawString("CONFIDENTIAL", font, 
    PdfBrushes.Red, new PointF(200, 10));

// Save modified document
loadedDocument.Save("modified.pdf");
loadedDocument.Close(true);
```

**After (IronPDF):**
```csharp
using IronPdf;

// Load existing PDF
var pdf = PdfDocument.FromFile("existing.pdf");

// Add text stamp/watermark
pdf.ApplyStamp(new TextStamper()
{
    Text = "CONFIDENTIAL",
    FontSize = 12,
    FontFamily = "Helvetica",
    Color = System.Drawing.Color.Red,
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Top
});

// Save modified document
pdf.SaveAs("modified.pdf");
```

## Common Gotchas

### 1. **HTML-First Approach**
IronPDF uses HTML/CSS for content generation rather than manual graphics positioning. Convert your positioning code to HTML layout.

**Issue:** Trying to manually position elements with coordinates.
**Solution:** Use HTML/CSS (flexbox, grid, absolute positioning) for layout control.

### 2. **Font Handling**
Syncfusion requires explicit font objects; IronPDF uses CSS font declarations.

**Syncfusion:** `new PdfStandardFont(PdfFontFamily.Helvetica, 12)`
**IronPDF:** Use CSS: `font-family: Helvetica; font-size: 12px;`

### 3. **Page Size Configuration**
Different approaches to setting page dimensions.

**Syncfusion:**
```csharp
page.Graphics.ClientSize = new SizeF(595, 842); // A4 size
```

**IronPDF:**
```csharp
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
// Or custom size:
renderer.RenderingOptions.SetCustomPaperSize(595, 842);
```

### 4. **Stream Handling**
IronPDF simplifies file operations without requiring explicit stream management.

**Issue:** Managing FileStream disposal and document closing.
**Solution:** IronPDF handles resources automatically; use `SaveAs()` for simple file operations.

### 5. **Image Insertion**
Convert image positioning code to HTML `<img>` tags.

**Syncfusion:** `page.Graphics.DrawImage(image, new PointF(x, y))`
**IronPDF:** Use `<img src="path/to/image.png" style="position: absolute; top: y; left: x;">`

### 6. **Licensing and Initialization**
Remember to set your IronPDF license key.

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

## Additional Resources

- **IronPDF Documentation:** https://ironpdf.com/docs/
- **IronPDF Tutorials:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/docs/
- **Code Examples:** https://ironpdf.com/tutorials/