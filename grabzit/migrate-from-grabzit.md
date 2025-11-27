# Migration Guide: GrabzIt → IronPDF

## Why Migrate from GrabzIt to IronPDF

GrabzIt creates image-based PDFs rather than true searchable, text-based documents, which limits accessibility and file size efficiency. Additionally, GrabzIt processes content on external servers, raising privacy and latency concerns for sensitive data. IronPDF generates true native PDFs locally with full customization control, preserving text selection, searchability, and professional document quality.

## NuGet Package Changes

```bash
# Remove GrabzIt
dotnet remove package GrabzIt

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| GrabzIt | IronPDF |
|---------|---------|
| `GrabzIt` | `IronPdf` |
| `GrabzIt.Parameters` | `IronPdf.Rendering` |
| `GrabzIt.Enums` | `IronPdf` (integrated into main namespace) |

## API Mapping

| GrabzIt API | IronPDF API | Notes |
|-------------|-------------|-------|
| `GrabzItClient` | `ChromePdfRenderer` | Main PDF generation class |
| `URLToPDF()` | `RenderUrlAsPdf()` | Convert URL to PDF |
| `HTMLToPDF()` | `RenderHtmlAsPdf()` | Convert HTML string to PDF |
| `Save()` / `SaveTo()` | `SaveAs()` | Save PDF to file |
| `ImageOptions` | `RenderingOptions` | Configuration settings |
| `SetCustomWaterMark()` | `ApplyWatermark()` | Add watermarks |
| `BrowserWidth` | `RenderingOptions.ViewPortWidth` | Set viewport width |
| `BrowserHeight` | `RenderingOptions.ViewPortHeight` | Set viewport height |

## Code Examples

### Example 1: URL to PDF

**Before (GrabzIt):**
```csharp
using GrabzIt;

var grabzIt = new GrabzItClient("app_key", "app_secret");
var options = new ImageOptions();
options.BrowserWidth = 1024;

grabzIt.URLToPDF("https://example.com", options);
grabzIt.SaveTo("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.ViewPortWidth = 1024;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 2: HTML String to PDF

**Before (GrabzIt):**
```csharp
using GrabzIt;

var grabzIt = new GrabzItClient("app_key", "app_secret");
string html = "<html><body><h1>Hello World</h1></body></html>";

grabzIt.HTMLToPDF(html);
grabzIt.SaveTo("document.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
string html = "<html><body><h1>Hello World</h1></body></html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("document.pdf");
```

### Example 3: Custom Page Size and Watermark

**Before (GrabzIt):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

var grabzIt = new GrabzItClient("app_key", "app_secret");
var options = new PDFOptions();
options.PageSize = PageSize.A4;
options.SetCustomWaterMark("watermark.png", 0, 0, 50, 50);

grabzIt.URLToPDF("https://example.com", options);
grabzIt.SaveTo("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.ApplyWatermark("<img src='watermark.png' style='width:50px;height:50px;'>", 
    opacity: 50, 
    rotation: 0);
pdf.SaveAs("output.pdf");
```

## Common Gotchas

| Issue | Solution |
|-------|----------|
| **No API keys needed** | IronPDF runs locally, no authentication required. Remove all API key configuration. |
| **Synchronous by default** | GrabzIt requires callbacks/waiting. IronPDF renders immediately and returns the PDF object. |
| **Real text vs images** | IronPDF PDFs contain actual text. Text extraction and searching work natively without OCR. |
| **File size differences** | IronPDF produces smaller files as they're vector-based, not image screenshots. |
| **Local processing** | No internet connection required for rendering. HTML/URLs are processed on your server. |
| **CSS rendering** | IronPDF uses Chrome engine for accurate modern CSS support including Flexbox and Grid. |
| **License requirement** | IronPDF requires a license key for production. Set via `IronPdf.License.LicenseKey = "KEY"`. |

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **API Reference**: Full IntelliSense support included in NuGet package