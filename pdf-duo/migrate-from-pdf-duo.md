# Migration Guide: PDF Duo .NET → IronPDF

## Why Migrate to IronPDF

IronPDF is a well-established, actively maintained PDF library with comprehensive documentation and enterprise-grade support. Unlike PDF Duo .NET, which has unclear provenance and limited resources, IronPDF offers regular updates, extensive tutorials, and a proven track record in production environments. Migrating ensures long-term stability, security updates, and access to modern PDF features.

## NuGet Package Changes

```bash
# Remove PDF Duo .NET
dotnet remove package PDFDuo.NET

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| PDF Duo .NET | IronPDF |
|--------------|---------|
| `PDFDuo` | `IronPdf` |
| `PDFDuo.Document` | `IronPdf` |
| `PDFDuo.Rendering` | `IronPdf.Rendering` |
| `PDFDuo.Settings` | `IronPdf` |

## API Mapping Table

| PDF Duo .NET | IronPDF | Notes |
|--------------|---------|-------|
| `PDFDocument.Create()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | HTML to PDF conversion |
| `PDFDocument.Load()` | `PdfDocument.FromFile()` | Load existing PDF |
| `PDFDocument.Save()` | `PdfDocument.SaveAs()` | Save PDF to file |
| `PDFDocument.AddPage()` | `PdfDocument.AppendPdf()` | Add pages/documents |
| `PDFConverter.FromHtml()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | HTML rendering |
| `PDFConverter.FromUrl()` | `ChromePdfRenderer.RenderUrlAsPdf()` | URL to PDF |
| `PDFSettings.PageSize` | `ChromePdfRenderOptions.PaperSize` | Page configuration |
| `PDFSettings.Margins` | `ChromePdfRenderOptions.MarginTop/Bottom/Left/Right` | Margin settings |
| `PDFDocument.Merge()` | `PdfDocument.Merge()` | Combine PDFs |
| `PDFDocument.ToBytes()` | `PdfDocument.BinaryData` | Get PDF as byte array |

## Code Examples

### Example 1: HTML to PDF Conversion

**Before (PDF Duo .NET):**
```csharp
using PDFDuo;

var settings = new PDFSettings
{
    PageSize = PageSize.A4,
    Margins = new Margins(20, 20, 20, 20)
};

var html = "<h1>Hello World</h1><p>This is a test document.</p>";
var pdf = PDFConverter.FromHtml(html, settings);
pdf.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;

var html = "<h1>Hello World</h1><p>This is a test document.</p>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF Conversion

**Before (PDF Duo .NET):**
```csharp
using PDFDuo;

var pdf = PDFConverter.FromUrl("https://example.com");
pdf.Save("webpage.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Merging PDFs

**Before (PDF Duo .NET):**
```csharp
using PDFDuo;

var doc1 = PDFDocument.Load("document1.pdf");
var doc2 = PDFDocument.Load("document2.pdf");
var merged = PDFDocument.Merge(new[] { doc1, doc2 });
merged.Save("combined.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var doc1 = PdfDocument.FromFile("document1.pdf");
var doc2 = PdfDocument.FromFile("document2.pdf");
var merged = PdfDocument.Merge(doc1, doc2);
merged.SaveAs("combined.pdf");
```

## Common Gotchas

### 1. Licensing Requirements
IronPDF requires a license key for production use. Set it at application startup:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 2. Rendering Engine Differences
IronPDF uses Chrome/Chromium rendering engine, which may produce different output than PDF Duo .NET. Test your HTML/CSS thoroughly after migration.

### 3. Margin Configuration
IronPDF uses separate properties for each margin (Top, Bottom, Left, Right) instead of a single Margins object. Set each margin individually.

### 4. Async Operations
IronPDF supports async methods for better performance:
```csharp
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### 5. File Path Methods
Use `FromFile()` instead of `Load()` and `SaveAs()` instead of `Save()` to match IronPDF conventions.

### 6. Memory Management
Dispose of PdfDocument objects when done to free resources:
```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

## Additional Resources

- **Official Documentation:** https://ironpdf.com/docs/
- **Tutorials & Examples:** https://ironpdf.com/tutorials/
- **API Reference:** https://ironpdf.com/docs/
- **Support:** Contact IronPDF support team for migration assistance