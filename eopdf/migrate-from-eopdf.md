# Migration Guide: EO.Pdf → IronPDF

## Why Migrate from EO.Pdf to IronPDF

EO.Pdf suffers from a bloated 126MB footprint due to bundled Chrome dependencies, making deployment cumbersome and increasing infrastructure costs. Its legacy architecture—originally built on Internet Explorer before migrating to Chrome—introduces compatibility issues and technical debt. IronPDF offers a modern, efficient alternative with better cross-platform support, cleaner API design, and genuine Linux/macOS compatibility beyond Windows-centric implementations.

## NuGet Package Changes

```xml
<!-- Remove EO.Pdf -->
<PackageReference Include="EO.Pdf" Version="*" Remove />

<!-- Add IronPDF -->
<PackageReference Include="IronPdf" Version="2024.*" />
```

## Namespace Mapping

| EO.Pdf | IronPDF |
|--------|---------|
| `EO.Pdf` | `IronPdf` |
| `EO.Pdf.HtmlToPdf` | `IronPdf.ChromePdfRenderer` |
| `EO.Pdf.Contents` | `IronPdf.Editing` |
| `EO.Pdf.Acm` | `IronPdf.Forms` |

## API Mapping

| EO.Pdf API | IronPDF API | Notes |
|------------|-------------|-------|
| `HtmlToPdf.ConvertHtml()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | IronPDF uses Chrome renderer by default |
| `HtmlToPdf.ConvertUrl()` | `ChromePdfRenderer.RenderUrlAsPdf()` | Direct URL rendering |
| `PdfDocument.Save()` | `PdfDocument.SaveAs()` | More explicit naming |
| `HtmlToPdf.Options.PageSize` | `ChromePdfRenderer.RenderingOptions.PaperSize` | Similar paper size options |
| `HtmlToPdf.Options.OutputArea` | `ChromePdfRenderer.RenderingOptions.MarginTop/Bottom/Left/Right` | Separate margin properties |
| `PdfDocument.Merge()` | `PdfDocument.Merge()` | Same method name, different syntax |
| `PdfDocument.Pages` | `PdfDocument.Pages` | Similar page collection access |
| `HtmlToPdf.Options.BaseUrl` | `ChromePdfRenderer.RenderingOptions.BaseUrl` | Base URL for relative resources |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (EO.Pdf):**
```csharp
using EO.Pdf;

var html = "<h1>Hello World</h1><p>This is a PDF document.</p>";
HtmlToPdf.ConvertHtml(html, "output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var html = "<h1>Hello World</h1><p>This is a PDF document.</p>";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Custom Page Size

**Before (EO.Pdf):**
```csharp
using EO.Pdf;

HtmlToPdf.Options.PageSize = PdfPageSizes.A4;
HtmlToPdf.Options.OutputArea = new RectangleF(0.5f, 0.5f, 7.5f, 10.5f);
HtmlToPdf.ConvertUrl("https://example.com", "output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.MarginLeft = 10;
renderer.RenderingOptions.MarginRight = 10;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 3: Merging Multiple PDFs

**Before (EO.Pdf):**
```csharp
using EO.Pdf;

var doc1 = new PdfDocument("file1.pdf");
var doc2 = new PdfDocument("file2.pdf");
doc1.Merge(doc2);
doc1.Save("merged.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var doc1 = PdfDocument.FromFile("file1.pdf");
var doc2 = PdfDocument.FromFile("file2.pdf");
var merged = PdfDocument.Merge(doc1, doc2);
merged.SaveAs("merged.pdf");
```

## Common Gotchas

### 1. **Rendering Returns Object Instead of Direct Save**
EO.Pdf often saves files directly in conversion methods. IronPDF returns a `PdfDocument` object first, requiring explicit `.SaveAs()` call. This provides more flexibility for post-processing.

### 2. **Margin Units**
EO.Pdf uses inches in `RectangleF` for output areas. IronPDF uses millimeters by default for margins. Convert accordingly: `inches * 25.4 = millimeters`.

### 3. **Options Configuration**
EO.Pdf uses static `HtmlToPdf.Options` global configuration. IronPDF uses instance-based `RenderingOptions` on the renderer object, providing better thread safety and isolation.

### 4. **Base URL Handling**
When converting HTML strings with relative resource paths (CSS, images), always set `BaseUrl` explicitly in IronPDF:
```csharp
renderer.RenderingOptions.BaseUrl = new Uri("https://yourdomain.com/");
```

### 5. **Async Operations**
IronPDF provides async versions of most operations (`RenderHtmlAsPdfAsync()`, `RenderUrlAsPdfAsync()`). Use these for better performance in web applications instead of blocking synchronous calls.

### 6. **License Key Configuration**
IronPDF requires license key to be set early in application lifecycle:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

## Additional Resources

- **Documentation:** https://ironpdf.com/docs/
- **Tutorials & Examples:** https://ironpdf.com/tutorials/
- **API Reference:** Complete API documentation available in the docs section
- **Support:** Access community forums and direct support through the IronPDF website