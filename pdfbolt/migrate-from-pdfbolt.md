# Migration Guide: PDFBolt → IronPDF

## Why Migrate to IronPDF

IronPDF provides a self-hosted solution that processes documents locally, eliminating data privacy concerns associated with cloud-only services. Unlike PDFBolt's restrictive free tier of 100 documents per month, IronPDF offers unlimited local processing with no external dependencies. This ensures better performance, security, and compliance with data protection regulations.

## NuGet Package Changes

```bash
# Remove PDFBolt
dotnet remove package PDFBolt

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| PDFBolt | IronPDF |
|---------|---------|
| `using PDFBolt;` | `using IronPdf;` |
| `PDFBolt.Client` | `IronPdf.ChromePdfRenderer` |
| `PDFBolt.Models` | `IronPdf` (no separate models namespace) |

## API Mapping

| PDFBolt API | IronPDF API |
|-------------|-------------|
| `new Client(apiKey)` | `new ChromePdfRenderer()` |
| `client.HtmlToPdf(html)` | `renderer.RenderHtmlAsPdf(html)` |
| `client.UrlToPdf(url)` | `renderer.RenderUrlAsPdf(url)` |
| `result.GetBytes()` | `pdf.BinaryData` |
| `result.SaveToFile(path)` | `pdf.SaveAs(path)` |
| `options.PageSize` | `renderer.RenderingOptions.PaperSize` |
| `options.Orientation` | `renderer.RenderingOptions.PaperOrientation` |
| `options.Margins` | `renderer.RenderingOptions.MarginTop/Bottom/Left/Right` |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (PDFBolt):**
```csharp
using PDFBolt;

var client = new Client("your-api-key");
var html = "<h1>Hello World</h1><p>This is a PDF document.</p>";
var result = await client.HtmlToPdf(html);
await result.SaveToFile("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var html = "<h1>Hello World</h1><p>This is a PDF document.</p>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Options

**Before (PDFBolt):**
```csharp
using PDFBolt;
using PDFBolt.Models;

var client = new Client("your-api-key");
var options = new PdfOptions
{
    PageSize = PageSize.A4,
    Orientation = Orientation.Landscape,
    Margins = new Margins { Top = 20, Bottom = 20, Left = 15, Right = 15 }
};
var result = await client.UrlToPdf("https://example.com", options);
byte[] pdfBytes = result.GetBytes();
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Engines.Chrome;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 15;
renderer.RenderingOptions.MarginRight = 15;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
byte[] pdfBytes = pdf.BinaryData;
```

### Example 3: Advanced HTML with CSS

**Before (PDFBolt):**
```csharp
using PDFBolt;

var client = new Client("your-api-key");
var options = new PdfOptions
{
    WaitForNetworkIdle = true,
    PrintBackground = true
};

var html = @"
<html>
<head>
    <style>
        body { font-family: Arial; background-color: #f0f0f0; }
        .header { color: #333; }
    </style>
</head>
<body>
    <div class='header'>Styled Document</div>
</body>
</html>";

var result = await client.HtmlToPdf(html, options);
await result.SaveToFile("styled.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.WaitFor.RenderDelay(500); // Wait for content
renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;
renderer.RenderingOptions.PrintHtmlBackgrounds = true;

var html = @"
<html>
<head>
    <style>
        body { font-family: Arial; background-color: #f0f0f0; }
        .header { color: #333; }
    </style>
</head>
<body>
    <div class='header'>Styled Document</div>
</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("styled.pdf");
```

## Common Gotchas

- **No API Key Required**: IronPDF runs locally, so remove all API key references and authentication code
- **Synchronous by Default**: IronPDF methods are synchronous by default; remove `await` keywords unless using async overloads
- **License for Production**: IronPDF requires a license key for production use (free for development)
- **Margin Units**: IronPDF margins are in millimeters by default, while PDFBolt may use pixels
- **Network Idle**: Replace `WaitForNetworkIdle` with `RenderingOptions.WaitFor.RenderDelay()` or other wait strategies
- **Binary Data Access**: Use `pdf.BinaryData` instead of calling a separate `GetBytes()` method
- **Paper Size Enums**: Enum names differ slightly (e.g., `PdfPaperSize.A4` vs `PageSize.A4`)

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/