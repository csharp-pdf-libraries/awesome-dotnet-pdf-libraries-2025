# Migration Guide: ExpertPdf to IronPDF

## Why Migrate from ExpertPdf to IronPDF

ExpertPdf has not received documentation updates since 2018 and relies on a wrapper around outdated Chrome versions, creating security and compatibility risks. IronPDF offers modern, actively maintained PDF generation with up-to-date Chromium rendering, comprehensive documentation, and competitive pricing. Migrating to IronPDF ensures your application benefits from current web standards, ongoing support, and a more intuitive API.

## NuGet Package Changes

```bash
# Remove ExpertPdf
dotnet remove package ExpertPdf.HtmlToPdf

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| ExpertPdf | IronPDF |
|-----------|---------|
| `ExpertPdf.HtmlToPdf` | `IronPdf` |
| `ExpertPdf.HtmlToPdf.PdfConverter` | `IronPdf.ChromePdfRenderer` |
| `ExpertPdf.HtmlToPdf.PdfDocumentOptions` | `IronPdf.ChromePdfRenderOptions` |

## API Mapping

| ExpertPdf API | IronPDF API | Notes |
|---------------|-------------|-------|
| `PdfConverter.ConvertUrlToFile()` | `ChromePdfRenderer.RenderUrlAsPdf()` | IronPDF returns PdfDocument object |
| `PdfConverter.ConvertHtmlToFile()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | IronPDF supports chaining options |
| `PdfDocumentOptions.PdfPageSize` | `ChromePdfRenderOptions.PaperSize` | Similar page size enums |
| `PdfDocumentOptions.PdfPageOrientation` | `ChromePdfRenderOptions.PaperOrientation` | Portrait/Landscape options |
| `PdfDocumentOptions.MarginLeft/Right/Top/Bottom` | `ChromePdfRenderOptions.MarginTop/Bottom/Left/Right` | Direct property mapping |
| `PdfConverter.LicenseKey` | `IronPdf.License.LicenseKey` | Set license globally |
| `PdfConverter.SavePdfFromUrlToFile()` | `RenderUrlAsPdf().SaveAs()` | Two-step process in IronPDF |
| `PdfConverter.GetPdfBytesFromUrl()` | `RenderUrlAsPdf().BinaryData` | Access byte array from document |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

PdfConverter pdfConverter = new PdfConverter();
pdfConverter.LicenseKey = "YOUR_LICENSE_KEY";

string html = "<h1>Hello World</h1><p>This is a test PDF.</p>";
byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(html);

System.IO.File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
string html = "<h1>Hello World</h1><p>This is a test PDF.</p>";

PdfDocument pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Custom Options

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

PdfConverter pdfConverter = new PdfConverter();
pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
pdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Landscape;
pdfConverter.PdfDocumentOptions.MarginLeft = 20;
pdfConverter.PdfDocumentOptions.MarginRight = 20;
pdfConverter.PdfDocumentOptions.MarginTop = 30;
pdfConverter.PdfDocumentOptions.MarginBottom = 30;

pdfConverter.SavePdfFromUrlToFile("https://example.com", "output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;
renderer.RenderingOptions.MarginTop = 30;
renderer.RenderingOptions.MarginBottom = 30;

PdfDocument pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 3: HTML String to PDF with Custom Page Size

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

PdfConverter pdfConverter = new PdfConverter();
pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.Letter;
pdfConverter.PdfDocumentOptions.ShowHeader = true;
pdfConverter.PdfDocumentOptions.ShowFooter = true;

string html = @"
    <html>
        <body>
            <h1>Invoice</h1>
            <p>Invoice details here...</p>
        </body>
    </html>";

pdfConverter.SavePdfFromHtmlStringToFile(html, "invoice.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.Letter;
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter() 
{ 
    HtmlFragment = "<div style='text-align:center'>Header Content</div>" 
};
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter() 
{ 
    HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>" 
};

string html = @"
    <html>
        <body>
            <h1>Invoice</h1>
            <p>Invoice details here...</p>
        </body>
    </html>";

PdfDocument pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

## Common Gotchas

### 1. **Method Return Types**
ExpertPdf methods often save directly to file or return byte arrays. IronPDF returns `PdfDocument` objects that provide more flexibility (save, manipulate, convert, etc.).

### 2. **License Key Assignment**
ExpertPdf sets the license on the converter instance. IronPDF sets it globally via `IronPdf.License.LicenseKey`.

### 3. **Header/Footer Configuration**
ExpertPdf uses simple boolean flags for headers/footers. IronPDF requires `HtmlHeaderFooter` objects with customizable HTML content, providing much more control.

### 4. **Rendering Engine**
IronPDF uses modern Chromium while ExpertPdf uses an outdated Chrome wrapper. Your HTML/CSS may render more accurately in IronPDF, potentially requiring minor style adjustments.

### 5. **Async Support**
IronPDF provides async methods (e.g., `RenderUrlAsPdfAsync()`) for better performance in modern applications. ExpertPdf primarily offers synchronous operations.

### 6. **JavaScript Execution**
IronPDF has better JavaScript support and rendering timeout controls through `RenderingOptions.RenderDelay` and `RenderingOptions.WaitFor` for dynamic content.

## Additional Resources

- **IronPDF Documentation:** https://ironpdf.com/docs/
- **IronPDF Tutorials:** https://ironpdf.com/tutorials/
- **Code Examples:** https://ironpdf.com/examples/