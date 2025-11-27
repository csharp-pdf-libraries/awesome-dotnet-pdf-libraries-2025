# Migration Guide: Api2pdf → IronPDF

## Why Migrate from Api2pdf to IronPDF

Api2pdf sends your sensitive HTML and documents to third-party servers, creating security and compliance risks. You pay per conversion indefinitely, with costs accumulating over time and creating vendor lock-in. IronPDF runs entirely on your infrastructure with a one-time license, eliminating security concerns, ongoing costs, and dependency on external services.

## NuGet Package Changes

```bash
# Remove Api2pdf
dotnet remove package Api2Pdf

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Api2pdf | IronPDF |
|---------|---------|
| `Api2Pdf` | `IronPdf` |
| N/A | `IronPdf.Extensions.Mvc` (for ASP.NET MVC) |

## API Mapping Table

| Api2pdf | IronPDF | Notes |
|---------|---------|-------|
| `new Api2Pdf("API_KEY")` | `ChromePdfRenderer` or `HtmlToPdf` | No API key needed - runs locally |
| `a2pClient.WkHtmlToPdf.FromHtml()` | `renderer.RenderHtmlAsPdf()` | Direct HTML rendering |
| `a2pClient.WkHtmlToPdf.FromUrl()` | `renderer.RenderUrlAsPdf()` | URL rendering |
| `a2pClient.Headless.FromHtml()` | `renderer.RenderHtmlAsPdf()` | Use ChromePdfRenderer |
| `a2pClient.Headless.FromUrl()` | `renderer.RenderUrlAsPdf()` | Use ChromePdfRenderer |
| `response.Pdf` | `pdf.BinaryData` or `pdf.SaveAs()` | Get bytes or save to file |
| N/A | `pdf.Stream` | Stream the PDF directly |

## Code Examples

### Example 1: HTML String to PDF

**Before (Api2pdf):**
```csharp
using Api2Pdf;

var a2pClient = new Api2PdfClient("YOUR_API_KEY");
var apiResponse = await a2pClient.WkHtmlToPdf.FromHtml(
    "<h1>Hello World</h1><p>This is a PDF</p>"
);
var pdfUrl = apiResponse.Pdf;
// Download from URL to get actual PDF
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>This is a PDF</p>");
pdf.SaveAs("output.pdf");
// Or get bytes: byte[] pdfBytes = pdf.BinaryData;
```

### Example 2: URL to PDF

**Before (Api2pdf):**
```csharp
using Api2Pdf;

var a2pClient = new Api2PdfClient("YOUR_API_KEY");
var apiResponse = await a2pClient.Headless.FromUrl(
    "https://example.com",
    inline: false,
    filename: "output.pdf"
);
var pdfUrl = apiResponse.Pdf;
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 3: HTML with Custom Options

**Before (Api2pdf):**
```csharp
using Api2Pdf;

var a2pClient = new Api2PdfClient("YOUR_API_KEY");
var options = new Dictionary<string, string>
{
    { "orientation", "landscape" },
    { "pageSize", "A4" }
};
var apiResponse = await a2pClient.WkHtmlToPdf.FromHtml(
    "<h1>Invoice</h1>",
    options: options
);
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

var pdf = renderer.RenderHtmlAsPdf("<h1>Invoice</h1>");
pdf.SaveAs("invoice.pdf");
```

## Common Gotchas

### 1. **No Async Download Required**
Api2pdf returns a URL to download the PDF asynchronously. IronPDF generates the PDF synchronously and returns it immediately - no separate download step needed.

### 2. **Local Installation Required**
IronPDF runs on your server, so ensure your deployment environment supports .NET and has sufficient permissions. Docker users may need to install additional dependencies.

### 3. **Licensing Model**
Remove all API key configurations. IronPDF uses a license key set once via `License.LicenseKey = "YOUR-LICENSE-KEY"` or placed in `appsettings.json`. Development is free with a watermark.

### 4. **Rendering Engine Differences**
Api2pdf offers multiple engines (WkHtmlToPdf, Headless Chrome). IronPDF's `ChromePdfRenderer` uses Chromium by default, providing modern CSS/JavaScript support similar to Api2pdf's Headless Chrome option.

### 5. **Error Handling**
Api2pdf errors come from HTTP responses. IronPDF throws standard .NET exceptions, so adjust your try-catch blocks accordingly.

### 6. **File Output**
Api2pdf typically returns URLs. With IronPDF, you have direct access to the PDF via `BinaryData`, `Stream`, or `SaveAs()` - choose based on your workflow (save to disk, return in HTTP response, upload to cloud storage, etc.).

## Additional Resources

- **Documentation:** https://ironpdf.com/docs/
- **Tutorials:** https://ironpdf.com/tutorials/
- **Code Examples:** https://ironpdf.com/examples/