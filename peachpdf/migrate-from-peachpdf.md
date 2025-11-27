# Migration Guide: PeachPDF → IronPDF

## Why Migrate?

IronPDF offers enterprise-grade PDF generation with comprehensive features, active development, and professional support that PeachPDF cannot provide. With a large user base and extensive documentation, IronPDF ensures long-term stability and compatibility with modern .NET frameworks. The migration provides access to advanced features like HTML-to-PDF conversion, digital signatures, and superior rendering capabilities.

## NuGet Package Changes

```bash
# Remove PeachPDF
dotnet remove package PeachPDF

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| PeachPDF | IronPDF |
|----------|---------|
| `PeachPDF` | `IronPdf` |
| `PeachPDF.Document` | `IronPdf.PdfDocument` |
| `PeachPDF.Rendering` | `IronPdf.Rendering` |

## API Mapping

| PeachPDF API | IronPDF API | Notes |
|--------------|-------------|-------|
| `PdfDocument.Create()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | HTML to PDF conversion |
| `Document.AddPage()` | `PdfDocument.AddPdfPages()` | Page manipulation |
| `Document.Save(path)` | `PdfDocument.SaveAs(path)` | Save to file |
| `Document.ToByteArray()` | `PdfDocument.BinaryData` | Get PDF as bytes |
| `PdfWriter.WriteText()` | `TextStamper.StampText()` | Add text to PDF |
| `PdfReader.LoadFromFile()` | `PdfDocument.FromFile()` | Load existing PDF |
| `Document.SetMetadata()` | `PdfDocument.MetaData` | Document properties |

## Code Examples

### Example 1: Creating a PDF from HTML

**Before (PeachPDF):**
```csharp
using PeachPDF;

var document = PdfDocument.Create();
document.AddHtmlContent("<h1>Hello World</h1>");
document.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: Loading and Modifying Existing PDF

**Before (PeachPDF):**
```csharp
using PeachPDF;

var document = PdfReader.LoadFromFile("input.pdf");
document.AddPage();
document.Save("modified.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");
var newPage = new ChromePdfRenderer().RenderHtmlAsPdf("<p>New Page</p>");
pdf.AddPdfPages(newPage);
pdf.SaveAs("modified.pdf");
```

### Example 3: Converting HTML to PDF Bytes

**Before (PeachPDF):**
```csharp
using PeachPDF;

var document = PdfDocument.Create();
document.AddHtmlContent("<div>Content</div>");
byte[] pdfBytes = document.ToByteArray();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<div>Content</div>");
byte[] pdfBytes = pdf.BinaryData;
```

## Common Gotchas

### 1. Licensing Requirements
IronPDF requires a license key for production use. Set it at application startup:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 2. Rendering Engine Differences
IronPDF uses a Chromium-based renderer, which may produce different layout results than PeachPDF. Test your PDFs thoroughly after migration.

### 3. Method Naming Conventions
IronPDF uses `SaveAs()` instead of `Save()`. Update all save operations accordingly.

### 4. Page Addition
IronPDF requires you to add complete PDF documents or render new content, rather than adding empty pages directly.

### 5. Async Operations
IronPDF supports async operations with `RenderHtmlAsPdfAsync()`. Consider using async methods for better performance:
```csharp
var pdf = await renderer.RenderHtmlAsPdfAsync("<h1>Async PDF</h1>");
```

### 6. Configuration Options
IronPDF offers extensive rendering options through `RenderingOptions`. Configure paper size, margins, and headers/footers:
```csharp
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 40;
```

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/