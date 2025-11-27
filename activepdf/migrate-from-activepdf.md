# Migration Guide: ActivePDF to IronPDF

## Why Migrate from ActivePDF to IronPDF

ActivePDF's acquisition by Foxit has created uncertainty around the product's future development and support. IronPDF offers a modern, actively maintained alternative with comprehensive documentation, regular updates, and a straightforward licensing model. The migration provides access to enhanced HTML-to-PDF rendering, better .NET Core/.NET 5+ support, and a more intuitive API.

## NuGet Package Changes

```bash
# Remove ActivePDF package
Uninstall-Package APToolkitNET

# Install IronPDF
Install-Package IronPdf
```

Or via .NET CLI:

```bash
dotnet remove package APToolkitNET
dotnet add package IronPdf
```

## Namespace Mapping

| ActivePDF | IronPDF |
|-----------|---------|
| `APToolkitNET` | `IronPdf` |
| `APToolkitNET.PDFObjects` | `IronPdf` |
| `APToolkitNET.Common` | `IronPdf` |

## API Mapping Table

| ActivePDF API | IronPDF API | Notes |
|---------------|-------------|-------|
| `new APToolkit()` | `ChromePdfRenderer` | IronPDF uses Chrome rendering engine |
| `OpenOutputFile()` | `RenderHtmlAsPdf().SaveAs()` | Separate render and save operations |
| `NewPage()` | `RenderHtmlAsPdf()` | HTML/URL-based page creation |
| `AddHtmlText()` | `RenderHtmlAsPdf(htmlContent)` | Direct HTML rendering |
| `SaveAs()` | `SaveAs()` | Similar method name |
| `AddImage()` | `PdfDocument.AddImage()` | Image insertion to existing PDF |
| `SetPageSize()` | `RenderingOptions.PaperSize` | Set before rendering |
| `SetOrientation()` | `RenderingOptions.PaperOrientation` | Set before rendering |
| `Merge()` | `PdfDocument.Merge()` | PDF merging functionality |
| `GetText()` | `ExtractAllText()` | Text extraction |
| `Encrypt()` | `Encrypt()` | Password protection |
| `AddWatermark()` | `ApplyWatermark()` | Watermarking |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (ActivePDF):**

```csharp
using APToolkitNET;

var pdf = new APToolkit();
pdf.OpenOutputFile("output.pdf");
pdf.NewPage();
pdf.AddHtmlText("<h1>Hello World</h1><p>This is a test PDF.</p>");
pdf.SaveAs("output.pdf");
pdf.CloseOutputFile();
```

**After (IronPDF):**

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>This is a test PDF.</p>");
pdf.SaveAs("output.pdf");
```

### Example 2: Converting URL to PDF with Page Settings

**Before (ActivePDF):**

```csharp
using APToolkitNET;

var pdf = new APToolkit();
pdf.OpenOutputFile("webpage.pdf");
pdf.SetPageSize(612, 792); // Letter size
pdf.SetOrientation("Portrait");
pdf.OpenUrl("https://example.com");
pdf.SaveAs("webpage.pdf");
pdf.CloseOutputFile();
```

**After (IronPDF):**

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Merging Multiple PDFs

**Before (ActivePDF):**

```csharp
using APToolkitNET;

var pdf = new APToolkit();
pdf.OpenOutputFile("merged.pdf");
pdf.Merge("document1.pdf");
pdf.Merge("document2.pdf");
pdf.Merge("document3.pdf");
pdf.SaveAs("merged.pdf");
pdf.CloseOutputFile();
```

**After (IronPDF):**

```csharp
using IronPdf;
using System.Collections.Generic;

var pdfs = new List<PdfDocument>
{
    PdfDocument.FromFile("document1.pdf"),
    PdfDocument.FromFile("document2.pdf"),
    PdfDocument.FromFile("document3.pdf")
};

var merged = PdfDocument.Merge(pdfs);
merged.SaveAs("merged.pdf");
```

## Common Gotchas

### 1. **Renderer Instantiation Required**
Unlike ActivePDF's single `APToolkit` object, IronPDF requires you to create a `ChromePdfRenderer` instance for HTML/URL conversions.

### 2. **No Explicit Open/Close File Pattern**
IronPDF uses a fluent API pattern. You don't need to explicitly open or close output files—just render and save directly.

### 3. **Rendering Options Must Be Set Before Rendering**
Configure `RenderingOptions` on the renderer object *before* calling `RenderHtmlAsPdf()` or `RenderUrlAsPdf()`. ActivePDF allowed setting properties at various stages.

### 4. **Different Page Size Units**
ActivePDF used points for page dimensions. IronPDF uses predefined `PdfPaperSize` enums (Letter, A4, etc.) or custom dimensions in millimeters/inches.

### 5. **Licensing Approach**
IronPDF requires a license key set via `IronPdf.License.LicenseKey = "YOUR-KEY"` at application startup, rather than ActivePDF's machine-locked licensing.

### 6. **Text Extraction Method Names**
ActivePDF's `GetText()` becomes `ExtractAllText()` in IronPDF. More granular extraction methods are available through `ExtractAllText()`, `ExtractTextFromPage()`, etc.

### 7. **Image Handling**
When adding images to existing PDFs, IronPDF's `PdfDocument` object has the `AddImage()` method, but you work with loaded `PdfDocument` instances rather than a stateful toolkit object.

## Additional Resources

- **Official Documentation**: https://ironpdf.com/docs/
- **Tutorials & Examples**: https://ironpdf.com/tutorials/
- **API Reference**: Available in the documentation section
- **Support**: IronPDF offers comprehensive support during migration