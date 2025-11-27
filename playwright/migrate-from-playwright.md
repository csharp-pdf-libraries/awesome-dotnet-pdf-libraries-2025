# Migration Guide: Playwright for .NET → IronPDF

## Why Migrate from Playwright for .NET to IronPDF

Playwright for .NET is primarily a browser automation and testing framework where PDF generation is a secondary feature, requiring complex async patterns and browser context management. IronPDF is purpose-built for PDF generation with a simpler, more intuitive API that doesn't require understanding browser lifecycles or disposal patterns. Additionally, IronPDF avoids the ~400MB+ download overhead of multiple browser engines, offering a leaner solution specifically optimized for document generation rather than testing workflows.

## NuGet Package Changes

```bash
# Remove Playwright
dotnet remove package Microsoft.Playwright

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Playwright for .NET | IronPDF |
|---------------------|---------|
| `Microsoft.Playwright` | `IronPdf` |
| N/A | `IronPdf.ChromePdfRenderer` |

## API Mapping

| Playwright for .NET | IronPDF | Notes |
|---------------------|---------|-------|
| `Playwright.CreateAsync()` | `new ChromePdfRenderer()` | No async initialization needed |
| `playwright.Chromium.LaunchAsync()` | N/A | No browser management required |
| `browser.NewPageAsync()` | N/A | No page context needed |
| `page.GotoAsync(url)` | `renderer.RenderUrlAsPdf(url)` | Direct URL rendering |
| `page.PdfAsync()` | `renderer.RenderHtmlAsPdf(html)` | Synchronous by default |
| `page.SetContentAsync(html)` + `page.PdfAsync()` | `renderer.RenderHtmlAsPdf(html)` | Single method call |
| `PdfOptions.Format` | `ChromePdfRenderOptions.PaperSize` | Page size configuration |
| `PdfOptions.MarginOptions` | `ChromePdfRenderOptions.MarginTop/Bottom/Left/Right` | Individual margin properties |
| `page.CloseAsync()` + `browser.CloseAsync()` | N/A | Automatic resource management |

## Code Examples

### Example 1: Basic HTML to PDF

**Before (Playwright for .NET):**
```csharp
using Microsoft.Playwright;

var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();

await page.SetContentAsync("<h1>Hello World</h1>");
await page.PdfAsync(new() { Path = "output.pdf" });

await page.CloseAsync();
await browser.CloseAsync();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Options

**Before (Playwright for .NET):**
```csharp
using Microsoft.Playwright;

var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();

await page.GotoAsync("https://example.com");
await page.PdfAsync(new()
{
    Path = "output.pdf",
    Format = "A4",
    Margin = new() { Top = "1cm", Bottom = "1cm" }
});

await page.CloseAsync();
await browser.CloseAsync();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 3: Advanced Configuration

**Before (Playwright for .NET):**
```csharp
using Microsoft.Playwright;

var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();

await page.SetViewportSizeAsync(1920, 1080);
await page.SetContentAsync("<h1>Custom Page</h1>");
await page.PdfAsync(new()
{
    Path = "output.pdf",
    PrintBackground = true,
    PreferCSSPageSize = true,
    DisplayHeaderFooter = true,
    HeaderTemplate = "<div style='font-size:10px'>Header</div>",
    FooterTemplate = "<div style='font-size:10px'>Footer</div>"
});

await page.CloseAsync();
await browser.CloseAsync();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    CenterText = "Header",
    FontSize = 10
};
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Footer",
    FontSize = 10
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Custom Page</h1>");
pdf.SaveAs("output.pdf");
```

## Common Gotchas

- **No async/await required**: IronPDF uses synchronous methods by default. Remove all `async`/`await` keywords unless using async overloads.
- **No browser lifecycle management**: You don't need to launch, manage, or close browsers. IronPDF handles this internally.
- **Installation differences**: IronPDF doesn't require running `playwright install` to download browsers. Everything is packaged automatically.
- **Margin units**: Playwright uses strings with units ("1cm"), while IronPDF uses numeric values in millimeters. Convert accordingly.
- **Header/Footer templates**: IronPDF uses `TextHeaderFooter` or `HtmlHeaderFooter` objects instead of HTML template strings.
- **Resource cleanup**: No need for explicit `CloseAsync()` or `DisposeAsync()` calls for browser contexts in IronPDF.
- **PDF modification**: IronPDF returns `PdfDocument` objects that can be further manipulated (merge, split, add pages) before saving, unlike Playwright which directly writes files.

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)