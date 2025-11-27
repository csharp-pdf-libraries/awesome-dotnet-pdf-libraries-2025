# Migration Guide: Kaizen.io HTML-to-PDF → IronPDF

## Why Migrate to IronPDF

IronPDF eliminates cloud dependencies by processing PDF generation entirely on your local infrastructure, ensuring your sensitive data never leaves your network. You'll experience significantly faster performance without network latency while maintaining complete control over your rendering environment. IronPDF provides a robust, production-ready solution with extensive customization options and enterprise-grade support.

## NuGet Package Changes

**Remove:**
```bash
dotnet remove package Kaizen.HtmlToPdf
```

**Add:**
```bash
dotnet add package IronPdf
```

## Namespace Mapping

| Kaizen.io HTML-to-PDF | IronPDF |
|----------------------|---------|
| `Kaizen.HtmlToPdf` | `IronPdf` |
| N/A | `IronPdf.Rendering` |

## API Mapping

| Kaizen.io Concept | IronPDF Equivalent | Notes |
|-------------------|-------------------|-------|
| `HtmlToPdfConverter` | `ChromePdfRenderer` | Main rendering class |
| `ConvertFromHtmlString()` | `RenderHtmlAsPdf()` | Convert HTML string to PDF |
| `ConvertFromUrl()` | `RenderUrlAsPdf()` | Convert URL to PDF |
| `PageSize` property | `RenderingOptions.PaperSize` | Set page dimensions |
| `Orientation` property | `RenderingOptions.PaperOrientation` | Portrait/Landscape |
| `MarginTop/Bottom/Left/Right` | `RenderingOptions.MarginTop/Bottom/Left/Right` | Page margins |
| N/A | `RenderingOptions.CssMediaType` | Print/Screen CSS |
| N/A | `RenderingOptions.EnableJavaScript` | JavaScript execution control |

## Code Migration Examples

### Example 1: Basic HTML String to PDF

**Before (Kaizen.io):**
```csharp
using Kaizen.HtmlToPdf;

var converter = new HtmlToPdfConverter();
var html = "<h1>Hello World</h1><p>This is a test document.</p>";
var pdfBytes = converter.ConvertFromHtmlString(html);
File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var html = "<h1>Hello World</h1><p>This is a test document.</p>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Options

**Before (Kaizen.io):**
```csharp
using Kaizen.HtmlToPdf;

var converter = new HtmlToPdfConverter
{
    PageSize = PageSize.A4,
    Orientation = PageOrientation.Landscape,
    MarginTop = 20,
    MarginBottom = 20,
    MarginLeft = 15,
    MarginRight = 15
};
var pdfBytes = converter.ConvertFromUrl("https://example.com");
File.WriteAllBytes("webpage.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 15;
renderer.RenderingOptions.MarginRight = 15;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Advanced Configuration with Headers/Footers

**Before (Kaizen.io):**
```csharp
using Kaizen.HtmlToPdf;

var converter = new HtmlToPdfConverter
{
    PageSize = PageSize.Letter,
    MarginTop = 50,
    MarginBottom = 50
};
// Limited header/footer options
var html = "<html><body><h1>Report</h1><p>Content here</p></body></html>";
var pdfBytes = converter.ConvertFromHtmlString(html);
File.WriteAllBytes("report.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
renderer.RenderingOptions.MarginTop = 50;
renderer.RenderingOptions.MarginBottom = 50;

// Rich header/footer support
renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    CenterText = "Company Report - {page} of {total-pages}",
    Font = IronPdf.Font.Arial,
    FontSize = 10
};
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    LeftText = "{date}",
    RightText = "Confidential",
    FontSize = 9
};

var html = "<html><body><h1>Report</h1><p>Content here</p></body></html>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

## Common Gotchas

### 1. **Return Type Difference**
- **Kaizen.io** returns `byte[]` directly
- **IronPDF** returns `PdfDocument` object with methods like `SaveAs()`, `Stream`, and `BinaryData`
- Use `pdf.BinaryData` to get byte array if needed

### 2. **Licensing Requirements**
- IronPDF requires a license key for production use
- Set via `IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";` at application startup
- Free for development/testing

### 3. **First Render Delay**
- IronPDF's first PDF render may take longer as it initializes the Chrome engine
- Consider warming up the renderer at application startup: `new ChromePdfRenderer();`

### 4. **JavaScript Execution**
- IronPDF enables JavaScript by default (`EnableJavaScript = true`)
- Kaizen.io behavior may vary; explicitly set `RenderingOptions.EnableJavaScript` if needed

### 5. **CSS Media Types**
- IronPDF defaults to `PdfCssMediaType.Print`
- Use `RenderingOptions.CssMediaType = PdfCssMediaType.Screen` for screen-optimized CSS

### 6. **File Paths vs Streams**
- IronPDF's `SaveAs()` handles file creation automatically
- No need to manually write bytes with `File.WriteAllBytes()` unless working with streams

### 7. **Async Operations**
- IronPDF supports async with `RenderHtmlAsPdfAsync()` and `RenderUrlAsPdfAsync()`
- Use for better performance in web applications

## Additional Resources

- **Official Documentation:** https://ironpdf.com/docs/
- **Tutorials & Examples:** https://ironpdf.com/tutorials/
- **API Reference:** Comprehensive API documentation available in the docs section