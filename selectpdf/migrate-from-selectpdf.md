# Migration Guide: SelectPdf → IronPDF

## Why Migrate from SelectPdf

SelectPdf falsely markets itself as cross-platform but explicitly does not support Linux, macOS, Docker, or Azure Functions—making it unsuitable for modern cloud deployments. The free version is severely limited to 5 pages before aggressive watermarking kicks in, and its outdated Chromium fork struggles with modern CSS features like Grid and advanced Flexbox layouts. IronPDF offers true cross-platform support, a generous free tier, and uses an up-to-date rendering engine compatible with modern web standards.

## NuGet Package Changes

```bash
# Remove SelectPdf
dotnet remove package Select.HtmlToPdf

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| SelectPdf | IronPDF |
|-----------|---------|
| `SelectPdf` | `IronPdf` |
| `SelectPdf.HtmlToPdf` | `IronPdf.ChromePdfRenderer` |
| `SelectPdf.PdfDocument` | `IronPdf.PdfDocument` |
| `SelectPdf.PageSize` | `IronPdf.Rendering.PdfPaperSize` |
| `SelectPdf.PdfPageOrientation` | `IronPdf.Rendering.PdfPaperOrientation` |

## API Mapping Table

| SelectPdf API | IronPDF API | Notes |
|---------------|-------------|-------|
| `new HtmlToPdf()` | `new ChromePdfRenderer()` | Renderer initialization |
| `converter.ConvertUrl(url)` | `renderer.RenderUrlAsPdf(url)` | URL to PDF |
| `converter.ConvertHtmlString(html)` | `renderer.RenderHtmlAsPdf(html)` | HTML string to PDF |
| `converter.ConvertHtmlString(html, baseUrl)` | `renderer.RenderHtmlAsPdf(html, baseUrl)` | HTML with base URL |
| `converter.Options.PdfPageSize` | `renderer.RenderingOptions.PaperSize` | Page size configuration |
| `converter.Options.PdfPageOrientation` | `renderer.RenderingOptions.PaperOrientation` | Page orientation |
| `converter.Options.MarginTop` | `renderer.RenderingOptions.MarginTop` | Top margin (in mm) |
| `converter.Options.MarginBottom` | `renderer.RenderingOptions.MarginBottom` | Bottom margin (in mm) |
| `converter.Options.MarginLeft` | `renderer.RenderingOptions.MarginLeft` | Left margin (in mm) |
| `converter.Options.MarginRight` | `renderer.RenderingOptions.MarginRight` | Right margin (in mm) |
| `doc.Save(path)` | `pdf.SaveAs(path)` | Save to file |
| `doc.Save(stream)` | `pdf.Stream.CopyTo(stream)` | Save to stream |
| `converter.Header.Add(html)` | `renderer.RenderingOptions.HtmlHeader` | Add HTML header |
| `converter.Footer.Add(html)` | `renderer.RenderingOptions.HtmlFooter` | Add HTML footer |
| `converter.Options.WebPageWidth` | `renderer.RenderingOptions.ViewPortWidth` | Viewport width |
| `converter.Options.WebPageHeight` | `renderer.RenderingOptions.ViewPortHeight` | Viewport height |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (SelectPdf):**
```csharp
using SelectPdf;

var converter = new HtmlToPdf();
PdfDocument doc = converter.ConvertHtmlString("<h1>Hello World</h1>");
doc.Save("output.pdf");
doc.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Custom Page Settings

**Before (SelectPdf):**
```csharp
using SelectPdf;

var converter = new HtmlToPdf();
converter.Options.PdfPageSize = PdfPageSize.A4;
converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape;
converter.Options.MarginTop = 20;
converter.Options.MarginBottom = 20;

PdfDocument doc = converter.ConvertUrl("https://example.com");
doc.Save("webpage.pdf");
doc.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;

PdfDocument pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: HTML with Headers and Footers

**Before (SelectPdf):**
```csharp
using SelectPdf;

var converter = new HtmlToPdf();
converter.Options.MarginTop = 50;
converter.Options.MarginBottom = 50;

converter.Header.Add("<div style='text-align:center'>Page Header</div>");
converter.Footer.Add("<div style='text-align:center'>Page {page_number} of {total_pages}</div>");

string html = "<h1>Document Title</h1><p>Content here...</p>";
PdfDocument doc = converter.ConvertHtmlString(html);
doc.Save("document.pdf");
doc.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.MarginTop = 50;
renderer.RenderingOptions.MarginBottom = 50;

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:center'>Page Header</div>"
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>"
};

string html = "<h1>Document Title</h1><p>Content here...</p>";
PdfDocument pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("document.pdf");
```

## Common Gotchas

- **No explicit Close() needed**: IronPDF's `PdfDocument` implements `IDisposable` but doesn't require manual `Close()` calls like SelectPdf
- **Margin units**: Both use millimeters by default, but verify your existing margin values during migration
- **Page numbering syntax**: SelectPdf uses `{page_number}` and `{total_pages}`, while IronPDF uses `{page}` and `{total-pages}`
- **Base URL handling**: IronPDF requires explicit base URL parameter when rendering HTML strings with relative paths
- **Stream handling**: SelectPdf's `Save(stream)` vs IronPDF's `pdf.Stream.CopyTo(stream)` or `pdf.BinaryData` property
- **License configuration**: IronPDF requires `IronPdf.License.LicenseKey = "YOUR-KEY"` to be set before rendering (for licensed versions)
- **Linux deployment**: IronPDF works on Linux out-of-the-box (unlike SelectPdf), but ensure required dependencies are installed for Docker environments

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)