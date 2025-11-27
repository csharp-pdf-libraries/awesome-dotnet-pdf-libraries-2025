# Migration Guide: Haukcode.DinkToPdf → IronPDF

## Why Migrate

Haukcode.DinkToPdf is a fork of an abandoned project that relies on the unmaintained wkhtmltopdf library, inheriting its known security vulnerabilities (CVEs) and limited HTML5/CSS3 support. IronPDF is actively maintained with a modern Chromium rendering engine, providing better security, performance, and comprehensive HTML/CSS rendering. The migration eliminates dependency on native binaries while gaining enterprise-grade features and reliable support.

## NuGet Package Changes

```bash
# Remove old package
dotnet remove package DinkToPdf

# Install new package
dotnet add package IronPdf
```

## Namespace Mapping

| Haukcode.DinkToPdf | IronPDF |
|-------------------|---------|
| `DinkToPdf` | `IronPdf` |
| `DinkToPdf.Contracts` | `IronPdf` |
| No equivalent | `IronPdf.Rendering` |

## API Mapping

| Haukcode.DinkToPdf | IronPDF |
|-------------------|---------|
| `SynchronizedConverter` | `ChromePdfRenderer` |
| `HtmlToPdfDocument` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `ObjectSettings` | `PdfPrintOptions` / `RenderingOptions` |
| `GlobalSettings` | `PdfPrintOptions` |
| `converter.Convert()` | `renderer.RenderHtmlAsPdf()` |
| `PaperKind` | `PaperSize` enum |
| `Orientation` | `PdfPrintOptions.PaperOrientation` |
| `DPI` | `PdfPrintOptions.Dpi` |
| `LoadSettings` | `RenderingOptions` |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (Haukcode.DinkToPdf):**
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
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Options

**Before (Haukcode.DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

var converter = new SynchronizedConverter(new PdfTools());

var doc = new HtmlToPdfDocument()
{
    GlobalSettings = {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Landscape,
        PaperSize = PaperKind.A4,
        Margins = new MarginSettings() { Top = 10, Bottom = 10 }
    },
    Objects = {
        new ObjectSettings() {
            Page = "https://example.com"
        }
    }
};

byte[] pdf = converter.Convert(doc);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 3: Custom Headers, Footers, and Page Settings

**Before (Haukcode.DinkToPdf):**
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
        DPI = 300
    },
    Objects = {
        new ObjectSettings() {
            HtmlContent = "<html><body><h1>Content</h1></body></html>",
            HeaderSettings = { 
                FontSize = 9, 
                Right = "Page [page] of [toPage]",
                Line = true 
            },
            FooterSettings = { 
                FontSize = 9, 
                Center = "© 2024 Company",
                Line = true
            }
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
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.Dpi = 300;
renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
{
    RightText = "Page {page} of {total-pages}",
    DrawDividerLine = true,
    FontSize = 9
};
renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
{
    CenterText = "© 2024 Company",
    DrawDividerLine = true,
    FontSize = 9
};

var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Content</h1></body></html>");
pdf.SaveAs("output.pdf");
```

## Common Gotchas

- **No Native Binaries Required**: IronPDF doesn't require manual installation of wkhtmltopdf binaries or platform-specific libraries. Remove any native library loading code.

- **Return Type Difference**: DinkToPdf returns `byte[]` directly, while IronPDF returns a `PdfDocument` object. Use `.BinaryData` for byte array or `.SaveAs()` for file output.

- **Synchronous by Default**: IronPDF operations are synchronous by default. Use async methods like `RenderHtmlAsPdfAsync()` for async scenarios.

- **Configuration Location**: DinkToPdf uses `GlobalSettings` and `ObjectSettings` objects, while IronPDF uses `RenderingOptions` and `PdfPrintOptions` properties on the renderer.

- **Dependency Injection**: IronPDF doesn't require manual converter registration. Simply instantiate `ChromePdfRenderer` directly or use factory patterns.

- **License Key**: IronPDF requires a license key for production use. Set it via `IronPdf.License.LicenseKey = "YOUR-KEY"` or use the free development license.

- **JavaScript Execution**: IronPDF has better JavaScript support with configurable timeouts via `RenderingOptions.RenderDelay` instead of DinkToPdf's limited JavaScript settings.

- **Character Encoding**: UTF-8 is handled automatically in IronPDF. Remove explicit encoding settings unless dealing with legacy content.

## Additional Resources

- **Official Documentation**: https://ironpdf.com/docs/
- **Tutorials and Examples**: https://ironpdf.com/tutorials/
- **API Reference**: Available in the documentation site with detailed examples for all features