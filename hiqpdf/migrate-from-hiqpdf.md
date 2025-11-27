# Migration Guide: HiQPdf → IronPDF

## Why Migrate from HiQPdf to IronPDF

HiQPdf's "free" version imposes a 3-page limit before adding obtrusive watermarks, making it effectively a trial rather than a viable free option. IronPDF uses a modern Chromium rendering engine that handles contemporary JavaScript frameworks and CSS3 features that HiQPdf's older WebKit engine struggles with. Additionally, IronPDF provides first-class support for .NET Core, .NET 5+, and .NET Framework with clear documentation and active maintenance.

## NuGet Package Changes

```bash
# Remove HiQPdf
dotnet remove package HiQPdf

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| HiQPdf | IronPDF |
|--------|---------|
| `HiQPdf` | `IronPdf` |
| `HiQPdf.HtmlToPdf` | `IronPdf.ChromePdfRenderer` |
| `HiQPdf.PdfDocument` | `IronPdf.PdfDocument` |
| `HiQPdf.PdfPage` | `IronPdf.PdfPage` |

## API Mapping Table

| HiQPdf API | IronPDF Equivalent |
|------------|-------------------|
| `HtmlToPdf.ConvertHtmlToMemory()` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `HtmlToPdf.ConvertUrlToMemory()` | `ChromePdfRenderer.RenderUrlAsPdf()` |
| `HtmlToPdf.ConvertHtmlToFile()` | `ChromePdfRenderer.RenderHtmlAsPdf().SaveAs()` |
| `HtmlToPdf.BrowserWidth` | `ChromePdfRenderer.RenderingOptions.ViewPortWidth` |
| `HtmlToPdf.BrowserHeight` | `ChromePdfRenderer.RenderingOptions.ViewPortHeight` |
| `HtmlToPdf.TriggerMode` | `ChromePdfRenderer.RenderingOptions.RenderDelay` |
| `HtmlToPdf.PageOrientation` | `ChromePdfRenderer.RenderingOptions.PaperOrientation` |
| `PdfDocument.AddDocument()` | `PdfDocument.Merge()` |
| `PdfDocument.Save()` | `PdfDocument.SaveAs()` |

## Code Examples

### Example 1: Convert HTML String to PDF

**Before (HiQPdf):**
```csharp
using HiQPdf;

var htmlToPdf = new HtmlToPdf();
htmlToPdf.BrowserWidth = 1024;
byte[] pdfBytes = htmlToPdf.ConvertHtmlToMemory("<h1>Hello World</h1>", "");
File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.ViewPortWidth = 1024;
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: Convert URL to PDF

**Before (HiQPdf):**
```csharp
using HiQPdf;

var htmlToPdf = new HtmlToPdf();
htmlToPdf.TriggerMode = TriggerMode.WaitTime;
htmlToPdf.WaitBeforeConvert = 2000;
byte[] pdfBytes = htmlToPdf.ConvertUrlToMemory("https://example.com");
File.WriteAllBytes("webpage.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.RenderDelay = 2000; // milliseconds
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Custom Page Settings and Headers/Footers

**Before (HiQPdf):**
```csharp
using HiQPdf;

var htmlToPdf = new HtmlToPdf();
htmlToPdf.PageOrientation = PdfPageOrientation.Landscape;
htmlToPdf.Header.Enabled = true;
htmlToPdf.Header.Height = 50;
htmlToPdf.Header.HtmlSource = "<div>Header Content</div>";
htmlToPdf.Footer.Enabled = true;
htmlToPdf.Footer.DisplayOnFirstPage = false;
byte[] pdfBytes = htmlToPdf.ConvertHtmlToMemory("<h1>Document</h1>", "");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
{
    DrawDividerLine = true,
    CenterText = "Header Content",
    FontSize = 12
};
renderer.RenderingOptions.FirstPageNumber = 2; // Skip header on first page
var pdf = renderer.RenderHtmlAsPdf("<h1>Document</h1>");
pdf.SaveAs("document.pdf");
```

## Common Gotchas

### 1. **Rendering Timing**
- **HiQPdf** uses `TriggerMode` and `WaitBeforeConvert` for JavaScript rendering delays
- **IronPDF** uses `RenderingOptions.RenderDelay` (simpler) or `WaitFor.RenderDelay()` for more control
- Set `RenderingOptions.WaitFor.RenderDelay(milliseconds)` for JavaScript-heavy pages

### 2. **Memory vs File Output**
- **HiQPdf** returns `byte[]` from `ConvertHtmlToMemory()`
- **IronPDF** returns `PdfDocument` objects; use `.BinaryData` for byte array or `.SaveAs()` for files
- IronPDF's approach allows chaining operations (merge, encrypt, etc.) before saving

### 3. **License Key Configuration**
- **HiQPdf** typically sets license in constructor: `new HtmlToPdf(licenseKey)`
- **IronPDF** sets globally: `IronPdf.License.LicenseKey = "YOUR-KEY";` at application startup
- Place IronPDF license configuration before any PDF operations

### 4. **CSS Media Type**
- **HiQPdf** defaults may vary by version
- **IronPDF** defaults to `PdfCssMediaType.Print` which respects CSS `@media print` rules
- Use `RenderingOptions.CssMediaType = PdfCssMediaType.Screen` for screen rendering if needed

### 5. **Page Margins**
- **HiQPdf** uses properties like `TopMargin`, `BottomMargin` (in points)
- **IronPDF** uses `RenderingOptions.MarginTop`, `MarginBottom`, etc. (in millimeters by default)
- Convert units: 1 inch = 72 points = 25.4mm

### 6. **Custom Headers/Footers**
- **HiQPdf** uses `Header.HtmlSource` and `Footer.HtmlSource` for HTML content
- **IronPDF** offers `TextHeaderFooter` (simple) or `HtmlHeaderFooter` (advanced) classes
- IronPDF HTML headers/footers require separate HTML strings with specific structure

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **Code Examples**: Browse tutorials for HTML-to-PDF, URL-to-PDF, and advanced scenarios
- **API Reference**: Full API documentation available at https://ironpdf.com/docs/