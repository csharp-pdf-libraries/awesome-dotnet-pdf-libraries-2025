# Migration Guide: Gotenberg → IronPDF

## Why Migrate from Gotenberg to IronPDF

Gotenberg requires significant infrastructure overhead including Docker containers, Kubernetes orchestration, service discovery, and load balancing, adding complexity to your deployment pipeline. Every PDF generation requires a network call to a separate service, introducing latency and potential failure points. IronPDF eliminates these issues by running directly in your .NET application process, removing cold start latency, network dependencies, and infrastructure management.

## NuGet Package Changes

**Remove:**
```xml
<!-- HTTP client for Gotenberg API calls -->
<PackageReference Include="RestSharp" Version="*" />
<!-- or similar HTTP client library -->
```

**Add:**
```xml
<PackageReference Include="IronPdf" Version="2024.*" />
```

## Namespace Mapping

| Gotenberg | IronPDF |
|-----------|---------|
| N/A (HTTP API calls) | `IronPdf` |
| N/A | `IronPdf.Rendering` |
| N/A | `IronPdf.Engines.Chrome` |

## API Mapping

| Gotenberg Endpoint | IronPDF Equivalent |
|-------------------|-------------------|
| `POST /forms/chromium/convert/html` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `POST /forms/chromium/convert/url` | `ChromePdfRenderer.RenderUrlAsPdf()` |
| `POST /merge` | `PdfDocument.Merge()` |
| `waitDelay` parameter | `RenderingOptions.RenderDelay` |
| `paperWidth` / `paperHeight` | `RenderingOptions.PaperSize` |
| `marginTop` / `marginBottom` etc. | `RenderingOptions.MarginTop` / `MarginBottom` |
| `landscape` parameter | `RenderingOptions.PaperOrientation` |
| `scale` parameter | `RenderingOptions.Zoom` |
| `printBackground` | `RenderingOptions.CssMediaType` |

## Code Examples

### Example 1: HTML String to PDF

**Before (Gotenberg):**
```csharp
using RestSharp;

var client = new RestClient("http://gotenberg:3000");
var request = new RestRequest("/forms/chromium/convert/html", Method.Post);

request.AddFile("index.html", Encoding.UTF8.GetBytes("<h1>Invoice</h1>"), "index.html");
request.AddParameter("marginTop", "0.5");
request.AddParameter("marginBottom", "0.5");

var response = await client.ExecuteAsync(request);
var pdfBytes = response.RawBytes;

await File.WriteAllBytesAsync("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.MarginTop = 12.7; // 0.5 inches = 12.7mm
renderer.RenderingOptions.MarginBottom = 12.7;

var pdf = renderer.RenderHtmlAsPdf("<h1>Invoice</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Wait Delay

**Before (Gotenberg):**
```csharp
using RestSharp;

var client = new RestClient("http://gotenberg:3000");
var request = new RestRequest("/forms/chromium/convert/url", Method.Post);

request.AddParameter("url", "https://example.com/report");
request.AddParameter("waitDelay", "3s");
request.AddParameter("landscape", "true");

var response = await client.ExecuteAsync(request);
await File.WriteAllBytesAsync("report.pdf", response.RawBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.RenderDelay = 3000; // milliseconds
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

var pdf = renderer.RenderUrlAsPdf("https://example.com/report");
pdf.SaveAs("report.pdf");
```

### Example 3: Merging Multiple PDFs

**Before (Gotenberg):**
```csharp
using RestSharp;

var client = new RestClient("http://gotenberg:3000");
var request = new RestRequest("/forms/pdfengines/merge", Method.Post);

request.AddFile("file1.pdf", "path/to/file1.pdf");
request.AddFile("file2.pdf", "path/to/file2.pdf");
request.AddFile("file3.pdf", "path/to/file3.pdf");

var response = await client.ExecuteAsync(request);
await File.WriteAllBytesAsync("merged.pdf", response.RawBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("path/to/file1.pdf");
var pdf2 = PdfDocument.FromFile("path/to/file2.pdf");
var pdf3 = PdfDocument.FromFile("path/to/file3.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("merged.pdf");
```

## Common Gotchas

- **Margin units**: Gotenberg typically uses inches, IronPDF uses millimeters by default. Convert accordingly (1 inch = 25.4mm).
- **Delay timing**: Gotenberg uses string format like "3s", IronPDF uses milliseconds as integers (`3000`).
- **No service configuration needed**: Remove Docker Compose files, Kubernetes manifests, and service discovery configuration.
- **Licensing**: IronPDF requires a license key for production use. Set via `IronPdf.License.LicenseKey = "YOUR-KEY";`
- **Async operations**: IronPDF methods are synchronous by default. Wrap in `Task.Run()` if you need async behavior in web applications.
- **Resource cleanup**: Dispose of `PdfDocument` objects when done: `pdf.Dispose()` or use `using` statements.
- **First-run initialization**: The first PDF generation may take longer as IronPDF initializes its Chrome engine (subsequent calls are fast).

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/