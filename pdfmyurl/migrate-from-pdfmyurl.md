# Migration Guide: PDFmyURL → IronPDF

## Why Migrate to IronPDF

PDFmyURL is an API service that processes documents on external servers, creating privacy concerns and requiring ongoing monthly subscriptions ($39+/month). IronPDF is a true .NET library that runs locally in your application, giving you full control over document processing, eliminating recurring costs after purchase, and keeping sensitive data secure. Switching to IronPDF provides better performance, offline capability, and significantly lower long-term costs.

## NuGet Package Changes

```bash
# Remove PDFmyURL
dotnet remove package PdfMyUrl

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| PDFmyURL | IronPDF |
|----------|---------|
| `PdfMyUrl` | `IronPdf` |
| N/A (API-based) | `IronPdf.ChromePdfRenderer` |
| N/A (API-based) | `IronPdf.Rendering` |

## API Mapping

| PDFmyURL | IronPDF | Notes |
|----------|---------|-------|
| `new PdfMyUrlClient(licenseKey)` | `var renderer = new ChromePdfRenderer()` | No API key needed for local processing |
| `client.ConvertUrl(url)` | `renderer.RenderUrlAsPdf(url)` | Renders URL to PDF locally |
| `client.ConvertHtml(html)` | `renderer.RenderHtmlAsPdf(html)` | Processes HTML string |
| `client.ConvertHtmlFile(path)` | `renderer.RenderHtmlFileAsPdf(path)` | Renders local HTML file |
| `response.Save(filename)` | `pdf.SaveAs(filename)` | Direct save method |
| `response.GetBytes()` | `pdf.BinaryData` | Access raw PDF bytes |
| API parameters in request | `renderer.RenderingOptions` | Configure before rendering |

## Code Examples

### Example 1: Basic URL to PDF Conversion

**Before (PDFmyURL):**
```csharp
using PdfMyUrl;

var client = new PdfMyUrlClient("your-api-key");
var response = await client.ConvertUrlAsync("https://example.com");
response.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 2: HTML String to PDF with Options

**Before (PDFmyURL):**
```csharp
using PdfMyUrl;

var client = new PdfMyUrlClient("your-api-key");
var options = new ConvertOptions
{
    PageSize = "A4",
    Orientation = "Portrait",
    MarginTop = 10
};

string html = "<h1>Hello World</h1><p>PDF content</p>";
var response = await client.ConvertHtmlAsync(html, options);
await response.SaveAsync("document.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
renderer.RenderingOptions.MarginTop = 10;

string html = "<h1>Hello World</h1><p>PDF content</p>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("document.pdf");
```

### Example 3: Getting PDF as Byte Array

**Before (PDFmyURL):**
```csharp
using PdfMyUrl;

var client = new PdfMyUrlClient("your-api-key");
var response = await client.ConvertUrlAsync("https://example.com");
byte[] pdfBytes = response.GetBytes();

// Send via HTTP response
return File(pdfBytes, "application/pdf", "document.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
byte[] pdfBytes = pdf.BinaryData;

// Send via HTTP response
return File(pdfBytes, "application/pdf", "document.pdf");
```

## Common Gotchas

1. **Licensing Model**: IronPDF requires a license key set via `IronPdf.License.LicenseKey = "YOUR-KEY"` at application startup. Unlike PDFmyURL's per-request API key, this is a one-time configuration.

2. **Synchronous by Default**: IronPDF methods are synchronous by default. For async operations in web applications, wrap calls in `Task.Run()` or use IronPDF's async methods where available.

3. **First Run Initialization**: IronPDF downloads Chrome binaries on first use (~150MB). This happens automatically but may take time on initial execution. Consider pre-warming in production environments.

4. **Memory Management**: Since processing happens locally, ensure proper disposal of PDF objects using `using` statements or explicit `.Dispose()` calls to free memory.

5. **Network Requirements**: IronPDF can work completely offline after initial setup, unlike PDFmyURL which requires internet connectivity for every conversion.

6. **CSS/JavaScript Rendering**: IronPDF renders JavaScript by default. Use `renderer.RenderingOptions.RenderDelay` if content loads asynchronously (default is 0ms).

7. **Server Deployment**: On Linux servers, install Chrome dependencies: `apt-get install -y libnss3 libatk1.0-0 libatk-bridge2.0-0 libdrm2 libxkbcommon0 libxcomposite1 libxdamage1 libxfixes3 libxrandr2 libgbm1 libasound2`.

## Additional Resources

- **Official Documentation**: https://ironpdf.com/docs/
- **Tutorials & Examples**: https://ironpdf.com/tutorials/
- **API Reference**: Complete class and method documentation available in the docs
- **Support**: Commercial license includes priority support for migration assistance