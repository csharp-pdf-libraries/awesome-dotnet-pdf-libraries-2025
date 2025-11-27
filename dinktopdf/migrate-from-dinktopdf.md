# Migration Guide: DinkToPdf → IronPDF

## Why Migrate

DinkToPdf relies on wkhtmltopdf, which is no longer actively maintained and contains critical security vulnerabilities like CVE-2022-35583 (SSRF). The library requires complex native binary deployment across different platforms and suffers from thread-safety issues that cause crashes in production environments. IronPDF provides a modern, actively maintained alternative with native .NET integration, built-in thread safety, and no external dependencies.

## NuGet Package Changes

```bash
# Remove DinkToPdf
dotnet remove package DinkToPdf

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| DinkToPdf | IronPDF |
|-----------|---------|
| `DinkToPdf` | `IronPdf` |
| `DinkToPdf.Contracts` | `IronPdf` |
| No equivalent | `IronPdf.Rendering` |

## API Mapping

| DinkToPdf | IronPDF |
|-----------|---------|
| `SynchronizedConverter` | `ChromePdfRenderer` |
| `HtmlToPdfDocument` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `ObjectSettings` | `ChromePdfRenderOptions` |
| `GlobalSettings` | `ChromePdfRenderOptions` |
| `Convert(doc)` | `RenderHtmlAsPdf(html)` |
| `PaperKind` | `PdfPrintOptions.PaperSize` |
| `Orientation` | `PdfPrintOptions.PaperOrientation` |
| `MarginSettings` | `PdfPrintOptions.MarginTop/Bottom/Left/Right` |

## Before/After Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Portrait,
        PaperSize = PaperKind.A4,
    },
    Objects = {
        new ObjectSettings() {
            HtmlContent = "<h1>Hello World</h1>",
            WebSettings = { DefaultEncoding = "utf-8" }
        }
    }
};
byte[] pdf = converter.Convert(doc);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Custom Settings

**Before (DinkToPdf):**
```csharp
using DinkToPdf;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Landscape,
        PaperSize = PaperKind.A4,
        Margins = new MarginSettings() { Top = 10, Bottom = 10, Left = 10, Right = 10 }
    },
    Objects = {
        new ObjectSettings() {
            Page = "https://example.com"
        }
    }
};
byte[] pdf = converter.Convert(doc);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.MarginLeft = 10;
renderer.RenderingOptions.MarginRight = 10;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 3: Dependency Injection Setup

**Before (DinkToPdf):**
```csharp
// Startup.cs
using DinkToPdf;
using DinkToPdf.Contracts;

public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
}

// Controller
public class PdfController : Controller
{
    private readonly IConverter _converter;
    
    public PdfController(IConverter converter)
    {
        _converter = converter;
    }
    
    public IActionResult GeneratePdf()
    {
        var doc = new HtmlToPdfDocument()
        {
            Objects = {
                new ObjectSettings() {
                    HtmlContent = "<h1>Report</h1>"
                }
            }
        };
        var pdf = _converter.Convert(doc);
        return File(pdf, "application/pdf", "report.pdf");
    }
}
```

**After (IronPDF):**
```csharp
// Program.cs or Startup.cs
using IronPdf;

// No DI registration needed - IronPDF is thread-safe by default

// Controller
public class PdfController : Controller
{
    public IActionResult GeneratePdf()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1>");
        return File(pdf.BinaryData, "application/pdf", "report.pdf");
    }
}
```

## Common Gotchas

### 1. Thread Safety
- **DinkToPdf**: Required `SynchronizedConverter` wrapper for thread safety, still prone to crashes
- **IronPDF**: Thread-safe by default, create `ChromePdfRenderer` instances freely

### 2. Return Types
- **DinkToPdf**: Returns `byte[]` directly
- **IronPDF**: Returns `PdfDocument` object with `BinaryData` property and additional manipulation methods

### 3. No Native Binaries Required
- **DinkToPdf**: Required platform-specific wkhtmltopdf binaries in your deployment
- **IronPDF**: No external dependencies - everything is included in the NuGet package

### 4. Header/Footer Configuration
- **DinkToPdf**: Used `HeaderSettings` and `FooterSettings` objects
- **IronPDF**: Use `RenderingOptions.TextHeader` and `RenderingOptions.TextFooter` or HTML-based headers/footers

### 5. Licensing
- **DinkToPdf**: Free and open source (but unmaintained)
- **IronPDF**: Commercial license required for production use (free for development)

### 6. CSS and JavaScript Rendering
- **DinkToPdf**: Limited CSS3 support, inconsistent JavaScript execution
- **IronPDF**: Full modern browser rendering engine with complete CSS3 and JavaScript support

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **API Reference**: https://ironpdf.com/docs/