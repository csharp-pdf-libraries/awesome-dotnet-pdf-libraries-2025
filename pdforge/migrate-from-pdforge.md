# Migration Guide: pdforge → IronPDF

## Why Migrate from pdforge to IronPDF

IronPDF eliminates external API dependencies by processing PDFs entirely on your local infrastructure, giving you complete control over your document generation pipeline. You'll benefit from extensive customization options, faster processing times without network latency, and a one-time licensing model that reduces long-term costs. Additionally, IronPDF offers enhanced security by keeping sensitive documents within your own environment.

## NuGet Package Changes

```bash
# Remove pdforge
dotnet remove package pdforge

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| pdforge | IronPDF |
|---------|---------|
| `using PdForge;` | `using IronPdf;` |
| `using PdForge.Client;` | `using IronPdf;` |
| `using PdForge.Models;` | `using IronPdf;` |

## API Mapping

| pdforge | IronPDF | Notes |
|---------|---------|-------|
| `PdfClient` | `ChromePdfRenderer` | Main rendering engine |
| `HtmlToPdfRequest` | `RenderHtmlAsPdf()` | HTML to PDF conversion |
| `UrlToPdfRequest` | `RenderUrlAsPdf()` | URL to PDF conversion |
| `GenerateAsync()` | `RenderAsync()` | Async PDF generation |
| `SetPageSize()` | `PdfPrintOptions.PaperSize` | Page configuration |
| `SetMargins()` | `PdfPrintOptions.MarginTop/Bottom/Left/Right` | Margin settings |
| `SetOrientation()` | `PdfPrintOptions.PaperOrientation` | Portrait/Landscape |
| `AddHeader()` | `RenderingOptions.TextHeader` | Header content |
| `AddFooter()` | `RenderingOptions.TextFooter` | Footer content |
| `SetApiKey()` | `License.LicenseKey` | License configuration |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;

var client = new PdfClient("your-api-key");
var request = new HtmlToPdfRequest
{
    Html = "<h1>Hello World</h1><p>This is a test document.</p>"
};

var pdfBytes = await client.GenerateAsync(request);
File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

License.LicenseKey = "your-license-key";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>This is a test document.</p>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Custom Settings

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;

var client = new PdfClient("your-api-key");
var request = new UrlToPdfRequest
{
    Url = "https://example.com",
    PageSize = PageSize.A4,
    Orientation = Orientation.Landscape,
    MarginTop = 20,
    MarginBottom = 20
};

var pdfBytes = await client.GenerateAsync(request);
File.WriteAllBytes("webpage.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

License.LicenseKey = "your-license-key";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Adding Headers and Footers

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;

var client = new PdfClient("your-api-key");
var request = new HtmlToPdfRequest
{
    Html = "<h1>Report</h1><p>Content here...</p>",
    Header = "Company Report",
    Footer = "Page {page} of {totalPages}"
};

var pdfBytes = await client.GenerateAsync(request);
File.WriteAllBytes("report.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

License.LicenseKey = "your-license-key";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    CenterText = "Company Report",
    DrawDividerLine = true
};
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Page {page} of {total-pages}"
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1><p>Content here...</p>");
pdf.SaveAs("report.pdf");
```

## Common Gotchas

### 1. **Synchronous vs Asynchronous Methods**
- pdforge primarily uses async methods (`GenerateAsync()`), while IronPDF offers both sync and async options
- Use `RenderHtmlAsPdfAsync()` or `RenderUrlAsPdfAsync()` for async operations in IronPDF

### 2. **License Key Configuration**
- pdforge uses API keys passed to the client constructor
- IronPDF requires setting `License.LicenseKey` once at application startup (not per-request)

### 3. **Page Variable Syntax**
- pdforge uses `{page}` and `{totalPages}` in headers/footers
- IronPDF uses `{page}` and `{total-pages}` (note the hyphen)

### 4. **Return Types**
- pdforge returns `byte[]` directly from API calls
- IronPDF returns `PdfDocument` objects with methods like `SaveAs()`, `Stream`, or `BinaryData`

### 5. **Margin Units**
- pdforge typically uses pixels or points depending on settings
- IronPDF uses millimeters by default (can be changed via settings)

### 6. **Error Handling**
- pdforge errors come from API call failures (network, authentication, etc.)
- IronPDF errors are local exceptions (rendering issues, file I/O, licensing)
- No need to handle network timeouts or retry logic with IronPDF

### 7. **Installation Requirements**
- IronPDF may require installing browser dependencies on Linux servers
- Use `IronPdf.Linux` package for Linux environments
- Docker containers may need additional Chrome/Chromium dependencies

## Additional Resources

- **Official Documentation**: https://ironpdf.com/docs/
- **Tutorials and Examples**: https://ironpdf.com/tutorials/
- **API Reference**: https://ironpdf.com/docs/
- **Support**: Contact IronPDF support team for migration assistance