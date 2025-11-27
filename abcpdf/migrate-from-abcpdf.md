# Migration Guide: ABCpdf → IronPDF

## Why Migrate to IronPDF

IronPDF offers a simplified licensing model with transparent pricing and straightforward deployment options. It provides excellent cross-platform support with native compatibility for Windows, Linux, macOS, and Docker environments. The modern API design and comprehensive documentation make it easier to implement and maintain PDF functionality in your applications.

## NuGet Package Changes

```bash
# Remove ABCpdf
dotnet remove package ABCpdf

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| ABCpdf | IronPDF |
|--------|---------|
| `WebSupergoo.ABCpdf13` | `IronPdf` |
| `WebSupergoo.ABCpdf13.Objects` | `IronPdf.Rendering` |
| `WebSupergoo.ABCpdf13.Atoms` | `IronPdf` |

## API Mapping Table

| ABCpdf API | IronPDF API | Notes |
|------------|-------------|-------|
| `Doc doc = new Doc()` | `ChromePdfRenderer renderer = new ChromePdfRenderer()` | IronPDF uses Chrome rendering engine |
| `doc.AddImageUrl(url)` | `renderer.RenderUrlAsPdf(url)` | URL to PDF conversion |
| `doc.AddImageHtml(html)` | `renderer.RenderHtmlAsPdf(html)` | HTML string to PDF |
| `doc.Save(path)` | `pdf.SaveAs(path)` | Save PDF to file |
| `doc.GetData()` | `pdf.BinaryData` | Get PDF as byte array |
| `doc.Rect.String` | Page settings via `RenderingOptions` | Page dimensions configuration |
| `doc.HtmlOptions.Engine` | Built-in Chrome engine | No engine selection needed |
| `doc.Page` | `pdf.Pages[index]` | Page navigation |
| `doc.AddBookmark()` | `pdf.AddBookmark()` | Bookmark creation |
| `doc.Flatten()` | `pdf.FlattenPdfForm()` | Flatten form fields |

## Code Examples

### Example 1: HTML to PDF Conversion

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;

Doc doc = new Doc();
doc.HtmlOptions.Engine = EngineType.Gecko;
doc.Rect.Inset(20, 20);
doc.AddImageHtml("<h1>Hello World</h1><p>This is a PDF</p>");
doc.Save("output.pdf");
doc.Clear();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>This is a PDF</p>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Options

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;

Doc doc = new Doc();
doc.HtmlOptions.Engine = EngineType.Chrome;
doc.HtmlOptions.PageCacheEnabled = false;
doc.Rect.String = "A4";
doc.AddImageUrl("https://example.com");
byte[] pdfData = doc.GetData();
doc.Clear();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.EnableJavaScript = true;
var pdf = renderer.RenderUrlAsPdf("https://example.com");
byte[] pdfData = pdf.BinaryData;
```

### Example 3: Merging Multiple PDFs

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;

Doc doc1 = new Doc();
doc1.Read("first.pdf");
Doc doc2 = new Doc();
doc2.Read("second.pdf");

doc1.Append(doc2);
doc1.Save("merged.pdf");
doc1.Clear();
doc2.Clear();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("first.pdf");
var pdf2 = PdfDocument.FromFile("second.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

## Common Gotchas

### 1. Rendering Engine
- **ABCpdf**: Requires explicit engine selection (Gecko, Chrome, etc.)
- **IronPDF**: Uses Chrome engine by default—no configuration needed

### 2. Memory Management
- **ABCpdf**: Requires explicit `doc.Clear()` calls to release resources
- **IronPDF**: Implements IDisposable; use `using` statements for automatic cleanup

### 3. Page Sizing
- **ABCpdf**: Uses `doc.Rect.String = "A4"` for page size
- **IronPDF**: Uses `renderer.RenderingOptions.PaperSize = PdfPaperSize.A4`

### 4. Licensing Setup
- **ABCpdf**: License key set via configuration or registry
- **IronPDF**: License key set in code: `IronPdf.License.LicenseKey = "YOUR-KEY"`

### 5. Multi-page HTML
- **ABCpdf**: Requires loops with `AddImageUrl()` or `AddImageHtml()`
- **IronPDF**: Automatically handles multi-page content with pagination

### 6. Linux/Docker Deployment
- **ABCpdf**: May require additional configuration for cross-platform
- **IronPDF**: Include `IronPdf.Linux` or platform-specific packages as needed

### 7. Header/Footer
- **ABCpdf**: Programmatically added to each page
- **IronPDF**: Use `RenderingOptions.TextHeader` and `RenderingOptions.TextFooter` or HTML templates

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/