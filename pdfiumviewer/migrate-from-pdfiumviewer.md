# Migration Guide: PDFiumViewer → IronPDF

## Why Migrate to IronPDF

IronPDF provides comprehensive PDF functionality beyond viewing, including PDF creation, editing, and manipulation. Unlike PDFiumViewer's Windows Forms limitation, IronPDF works across multiple platforms (.NET Framework, .NET Core, .NET 5+) and application types (Console, Web, Desktop). IronPDF is actively maintained with regular updates and commercial support options.

## NuGet Package Changes

```bash
# Remove PDFiumViewer
dotnet remove package PdfiumViewer

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| PDFiumViewer | IronPDF |
|--------------|---------|
| `PdfiumViewer` | `IronPdf` |
| `PdfiumViewer.PdfDocument` | `IronPdf.PdfDocument` |
| `PdfiumViewer.PdfRenderer` | `IronPdf.Rendering.ChromePdfRenderer` |

## API Mapping Table

| PDFiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `PdfDocument.Load(string)` | `PdfDocument.FromFile(string)` | Load PDF from file |
| `PdfDocument.Load(Stream)` | `PdfDocument.FromStream(Stream)` | Load PDF from stream |
| `PdfDocument.PageCount` | `PdfDocument.PageCount` | Get number of pages |
| `PdfDocument.Render(int, int, int, bool)` | `PdfDocument.RasterizeToImageFiles(string, int)` | Render pages to images |
| `PdfDocument.Save(string)` | `PdfDocument.SaveAs(string)` | Save PDF to file |
| `PdfViewer` control | Not applicable | IronPDF is backend-focused; use alternative viewer controls |
| `PdfDocument.GetPdfImage(int)` | `PdfDocument.ToBitmap(int)` | Extract page as image |

## Code Examples

### Example 1: Loading and Displaying PDF Information

**Before (PDFiumViewer):**
```csharp
using PdfiumViewer;

// Load PDF
using (var document = PdfDocument.Load("document.pdf"))
{
    Console.WriteLine($"Pages: {document.PageCount}");
    
    // Render first page to image
    var image = document.Render(0, 300, 300, true);
    image.Save("page1.png");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

// Load PDF
var document = PdfDocument.FromFile("document.pdf");
Console.WriteLine($"Pages: {document.PageCount}");

// Rasterize first page to image
document.RasterizeToImageFiles("page*.png", 1, 1);
// Or get as bitmap
var bitmap = document.ToBitmap(0);
bitmap.Save("page1.png");
```

### Example 2: Creating PDFs from HTML (Not Possible in PDFiumViewer)

**Before (PDFiumViewer):**
```csharp
// NOT SUPPORTED - PDFiumViewer is viewing only
// Would require additional library for PDF creation
```

**After (IronPDF):**
```csharp
using IronPdf;

// Create PDF from HTML string
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>Created with IronPDF</p>");
pdf.SaveAs("output.pdf");

// Or from URL
var pdfFromUrl = renderer.RenderUrlAsPdf("https://example.com");
pdfFromUrl.SaveAs("webpage.pdf");
```

### Example 3: Extracting and Manipulating Pages

**Before (PDFiumViewer):**
```csharp
using PdfiumViewer;
using System.Drawing;

// Load and render specific page
using (var document = PdfDocument.Load("document.pdf"))
{
    // Render page 3 at 96 DPI
    using (var image = document.Render(2, 96, 96, false))
    {
        image.Save("page3.png", System.Drawing.Imaging.ImageFormat.Png);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

// Load PDF
var document = PdfDocument.FromFile("document.pdf");

// Extract specific page as new PDF
var page3 = document.CopyPage(2);
page3.SaveAs("page3.pdf");

// Or rasterize to image with DPI control
document.RasterizeToImageFiles("page3.png", 96, 3, 3);

// Extract text from page
string text = document.ExtractTextFromPage(2);
Console.WriteLine(text);
```

## Common Gotchas

### 1. **No Built-in Viewer Control**
IronPDF is a backend library without WinForms/WPF viewer controls. For viewing, save PDFs and use third-party viewer controls or browser-based solutions.

### 2. **License Requirements**
IronPDF requires a license key for commercial use. Set it in your code:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```
Free trials available at https://ironpdf.com/

### 3. **Page Indexing**
Both libraries use 0-based page indexing, but IronPDF methods are more explicit about this in their naming.

### 4. **Image Rendering Differences**
IronPDF's rasterization produces different quality defaults. Adjust DPI settings explicitly:
```csharp
document.RasterizeToImageFiles("output*.png", dpi: 300, pageIndexes: new[] { 0, 1 });
```

### 5. **Memory Management**
IronPDF handles disposal automatically in most cases, but explicitly dispose when working with large documents:
```csharp
using var pdf = PdfDocument.FromFile("large.pdf");
// Work with PDF
```

### 6. **Platform Dependencies**
IronPDF requires platform-specific binaries. Ensure proper deployment packages are installed for Linux/Docker deployments.

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **API Reference**: Available in the documentation
- **Support**: Commercial support available with licensed versions