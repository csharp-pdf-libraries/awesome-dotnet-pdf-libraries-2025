# Migration Guide: MuPDF (.NET Bindings) → IronPDF

## Why Migrate from MuPDF to IronPDF

MuPDF's AGPL license poses viral licensing concerns for commercial applications, requiring costly commercial licenses for proprietary software. While MuPDF excels at rendering PDFs, it's not designed for PDF creation or manipulation workflows common in .NET applications. IronPDF offers a commercial-friendly license, eliminates native dependency complexities, and provides comprehensive PDF generation and manipulation features purpose-built for .NET developers.

## NuGet Package Changes

```bash
# Remove MuPDF
dotnet remove package MuPDF.NET

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| MuPDF (.NET) | IronPDF |
|--------------|---------|
| `MuPDF.NET` | `IronPdf` |
| `MuPDF.NET.Document` | `IronPdf.PdfDocument` |
| `MuPDF.NET.Page` | `IronPdf.PdfPage` |
| N/A (limited support) | `IronPdf.Rendering` |

## API Mapping Table

| MuPDF (.NET) API | IronPDF Equivalent | Notes |
|------------------|-------------------|-------|
| `new Document(path)` | `PdfDocument.FromFile(path)` | Load existing PDF |
| `document.GetPage(index)` | `pdf.Pages[index]` | Access pages |
| `page.Width` / `page.Height` | `page.Width` / `page.Height` | Page dimensions |
| `page.ToPixmap()` | `pdf.RasterizeToImageFiles()` | Render to images |
| N/A | `ChromePdfRenderer.RenderHtmlAsPdf()` | Create PDFs from HTML |
| N/A | `PdfDocument.Merge()` | Merge multiple PDFs |
| `document.SaveAs(path)` | `pdf.SaveAs(path)` | Save PDF |
| N/A | `pdf.ApplyWatermark()` | Add watermarks |
| N/A | `pdf.Password` | Password protection |

## Code Examples

### Example 1: Loading and Rendering a PDF

**Before (MuPDF):**
```csharp
using MuPDF.NET;

var document = new Document("input.pdf");
var page = document.GetPage(0);
var pixmap = page.ToPixmap(1.0f);
pixmap.SaveAsPng("output.png");
document.Dispose();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");
pdf.RasterizeToImageFiles("output*.png");
```

### Example 2: Extracting Text from PDF

**Before (MuPDF):**
```csharp
using MuPDF.NET;

var document = new Document("document.pdf");
var text = string.Empty;

for (int i = 0; i < document.PageCount; i++)
{
    var page = document.GetPage(i);
    text += page.GetText();
}

Console.WriteLine(text);
document.Dispose();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
string text = pdf.ExtractAllText();
Console.WriteLine(text);
```

### Example 3: Creating a PDF (Not Natively Supported in MuPDF)

**Before (MuPDF - Requires External Tools):**
```csharp
// MuPDF focuses on rendering, not creation
// Would require using external libraries or tools
// to generate PDF, then load with MuPDF
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>Created with IronPDF</p>");
pdf.SaveAs("output.pdf");
```

## Common Gotchas

### 1. **Native Dependencies**
- **MuPDF**: Requires native binaries for each platform (Windows, Linux, macOS)
- **IronPDF**: Fully managed with automatic dependency resolution

### 2. **PDF Creation**
- **MuPDF**: Not designed for creating PDFs from scratch or from HTML
- **IronPDF**: Full HTML-to-PDF rendering with CSS support via `ChromePdfRenderer`

### 3. **Page Indexing**
- **MuPDF**: Uses `GetPage(index)` method (0-based)
- **IronPDF**: Uses `Pages[index]` collection (0-based)

### 4. **Licensing**
- **MuPDF**: AGPL by default; commercial license required for proprietary apps
- **IronPDF**: Commercial license with trial keys available; no viral licensing concerns

### 5. **Disposal Patterns**
- **MuPDF**: Requires explicit disposal of documents and resources
- **IronPDF**: Use `using` statements or explicit disposal; automatic resource management

### 6. **Image Rendering Quality**
- **MuPDF**: Specify scale factor in `ToPixmap(scale)`
- **IronPDF**: Configure via `RenderingOptions` with DPI settings

```csharp
// IronPDF rendering configuration
var pdf = PdfDocument.FromFile("input.pdf");
pdf.RasterizeToImageFiles("output*.png", DPI: 300);
```

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [API Reference](https://ironpdf.com/docs/)