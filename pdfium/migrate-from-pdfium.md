# Migration Guide: Pdfium.NET → IronPDF

## Why Migrate?

IronPDF offers comprehensive PDF creation, editing, and rendering capabilities beyond Pdfium.NET's viewing-focused functionality. It eliminates native dependency management by providing a fully managed .NET library with cross-platform support. IronPDF includes a free development license and straightforward commercial licensing, making it suitable for production applications without complex native binary configurations.

## NuGet Package Changes

```xml
<!-- Remove Pdfium.NET -->
<PackageReference Include="Pdfium.NET" Version="*" />

<!-- Add IronPDF -->
<PackageReference Include="IronPdf" Version="2024.*" />
```

## Namespace Mapping

| Pdfium.NET | IronPDF |
|------------|---------|
| `Pdfium` | `IronPdf` |
| `Pdfium.PdfDocument` | `IronPdf.PdfDocument` |
| `Pdfium.PdfPage` | `IronPdf.PdfPage` |
| N/A | `IronPdf.ChromePdfRenderer` |
| N/A | `IronPdf.HtmlToPdf` |

## API Mapping

| Pdfium.NET | IronPDF | Notes |
|------------|---------|-------|
| `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` | Load existing PDF |
| `PdfDocument.Load(stream)` | `PdfDocument.FromStream(stream)` | Load from stream |
| `document.Pages[index]` | `document.Pages[index]` | Access pages |
| `page.Render(width, height)` | `page.RasterizeToImage()` | Render to image |
| `document.Save(path)` | `document.SaveAs(path)` | Save PDF |
| N/A | `ChromePdfRenderer.RenderHtmlAsPdf(html)` | Create from HTML |
| N/A | `ChromePdfRenderer.RenderUrlAsPdf(url)` | Create from URL |
| `document.PageCount` | `document.PageCount` | Get page count |
| `page.Width`, `page.Height` | `page.Width`, `page.Height` | Page dimensions |

## Code Examples

### Example 1: Loading and Rendering a PDF Page

**Before (Pdfium.NET):**
```csharp
using Pdfium;

var document = PdfDocument.Load("input.pdf");
var page = document.Pages[0];
var bitmap = page.Render(1024, 768);
bitmap.Save("output.png");
document.Dispose();
```

**After (IronPDF):**
```csharp
using IronPdf;

var document = PdfDocument.FromFile("input.pdf");
var page = document.Pages[0];
var bitmap = page.RasterizeToImage();
bitmap.SaveAs("output.png");
```

### Example 2: Extracting Page Information

**Before (Pdfium.NET):**
```csharp
using Pdfium;

var document = PdfDocument.Load("document.pdf");
int pageCount = document.PageCount;

foreach (var page in document.Pages)
{
    double width = page.Width;
    double height = page.Height;
    Console.WriteLine($"Page: {width}x{height}");
}
document.Dispose();
```

**After (IronPDF):**
```csharp
using IronPdf;

var document = PdfDocument.FromFile("document.pdf");
int pageCount = document.PageCount;

foreach (var page in document.Pages)
{
    double width = page.Width;
    double height = page.Height;
    Console.WriteLine($"Page: {width}x{height}");
}
```

### Example 3: Creating PDFs (New Capability)

**Before (Pdfium.NET):**
```csharp
// Not supported - Pdfium.NET is primarily for viewing/rendering
// Would require external library or manual PDF construction
```

**After (IronPDF):**
```csharp
using IronPdf;

// Create from HTML
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>Created with IronPDF</p>");
pdf.SaveAs("created.pdf");

// Or create from URL
var urlPdf = renderer.RenderUrlAsPdf("https://example.com");
urlPdf.SaveAs("webpage.pdf");
```

## Common Gotchas

### 1. **License Configuration**
IronPDF requires license key configuration for production use:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```
Free license available for development: https://ironpdf.com/docs/

### 2. **Rendering Methods**
Pdfium.NET's `Render()` requires explicit dimensions, while IronPDF's `RasterizeToImage()` uses page dimensions by default:
```csharp
// Specify custom DPI if needed
var bitmap = page.RasterizeToImage(300); // 300 DPI
```

### 3. **No Native Dependencies**
Remove any native PDFium binary references from your project - IronPDF manages dependencies automatically. Delete any `pdfium.dll` or platform-specific native libraries.

### 4. **Disposal Patterns**
IronPDF implements `IDisposable` but is more forgiving. Wrap in `using` statements for best practices:
```csharp
using var document = PdfDocument.FromFile("input.pdf");
```

### 5. **Page Indexing**
Both libraries use zero-based indexing, but IronPDF provides additional page manipulation methods:
```csharp
document.Pages.RemoveAt(0); // Remove first page (IronPDF only)
```

### 6. **Thread Safety**
IronPDF's `ChromePdfRenderer` instances should not be shared across threads. Create separate instances per thread or use locking mechanisms.

### 7. **Cross-Platform Considerations**
IronPDF automatically handles platform-specific dependencies for Windows, Linux, and macOS. No manual configuration needed for Docker or cloud deployments.

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **API Reference**: Comprehensive guide for HTML to PDF, editing, merging, and advanced features