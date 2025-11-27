# Migration Guide: PuppeteerSharp → IronPDF

## Why Migrate

IronPDF eliminates the 300MB+ Chromium dependency, reducing deployment size by up to 90% and dramatically improving cold start times in serverless environments. Unlike PuppeteerSharp, IronPDF provides built-in memory management without manual browser instance recycling, preventing memory leaks under sustained load. Additionally, IronPDF offers comprehensive PDF manipulation capabilities including merging, splitting, password protection, and form filling—features absent in PuppeteerSharp's generation-only approach.

## NuGet Package Changes

```bash
# Remove PuppeteerSharp
dotnet remove package PuppeteerSharp

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| PuppeteerSharp | IronPDF |
|----------------|---------|
| `PuppeteerSharp` | `IronPdf` |
| `PuppeteerSharp.Media` | `IronPdf` |
| N/A | `IronPdf.Editing` |
| N/A | `IronPdf.Signing` |

## API Mapping

| PuppeteerSharp | IronPDF | Notes |
|----------------|---------|-------|
| `new BrowserFetcher().DownloadAsync()` | N/A | No browser download required |
| `await Puppeteer.LaunchAsync()` | N/A | No browser instance management |
| `await page.GoToAsync(url)` | `ChromePdfRenderer.RenderUrlAsPdf(url)` | Direct rendering |
| `await page.SetContentAsync(html)` | `ChromePdfRenderer.RenderHtmlAsPdf(html)` | Direct rendering |
| `await page.PdfAsync(path)` | `pdf.SaveAs(path)` | After rendering |
| `PdfOptions.Format` | `ChromePdfRenderOptions.PaperSize` | Page size configuration |
| `PdfOptions.MarginOptions` | `ChromePdfRenderOptions.MarginTop/Bottom/Left/Right` | Individual margin control |
| N/A | `PdfDocument.Merge()` | PDF manipulation |
| N/A | `pdf.Password` | PDF security |

## Code Examples

### Example 1: Basic HTML to PDF

**Before (PuppeteerSharp):**
```csharp
using PuppeteerSharp;

var browserFetcher = new BrowserFetcher();
await browserFetcher.DownloadAsync();

var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = true
});

var page = await browser.NewPageAsync();
await page.SetContentAsync("<h1>Hello World</h1>");
await page.PdfAsync("output.pdf");

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

**Before (PuppeteerSharp):**
```csharp
using PuppeteerSharp;
using PuppeteerSharp.Media;

var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
var page = await browser.NewPageAsync();

await page.GoToAsync("https://example.com", new NavigationOptions
{
    WaitUntil = new[] { WaitUntilNavigation.NetworkIdle2 }
});

await page.PdfAsync("output.pdf", new PdfOptions
{
    Format = PaperFormat.A4,
    MarginOptions = new MarginOptions
    {
        Top = "20mm",
        Bottom = "20mm",
        Left = "10mm",
        Right = "10mm"
    }
});

await browser.CloseAsync();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 10;
renderer.RenderingOptions.MarginRight = 10;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 3: Advanced PDF Operations (Not Possible in PuppeteerSharp)

**Before (PuppeteerSharp):**
```csharp
// NOT POSSIBLE - Would require external library like iTextSharp
// to merge PDFs, add passwords, or manipulate existing PDFs
```

**After (IronPDF):**
```csharp
using IronPdf;

// Generate multiple PDFs
var renderer = new ChromePdfRenderer();
var pdf1 = renderer.RenderHtmlAsPdf("<h1>Document 1</h1>");
var pdf2 = renderer.RenderHtmlAsPdf("<h1>Document 2</h1>");

// Merge PDFs
var merged = PdfDocument.Merge(pdf1, pdf2);

// Add password protection
merged.Password = "secure123";

// Add watermark
merged.ApplyWatermark("<h1>CONFIDENTIAL</h1>", 50);

merged.SaveAs("merged_secure.pdf");
```

## Common Gotchas

### 1. No Browser Lifecycle Management
**Issue:** PuppeteerSharp requires explicit browser launch/close. IronPDF handles this internally.
```csharp
// ❌ Don't try to manage browser instances
// No Launch/Close methods exist

// ✅ Just render directly
var pdf = new ChromePdfRenderer().RenderHtmlAsPdf(html);
```

### 2. Margins Use Different Units
**Issue:** PuppeteerSharp uses strings with units ("20mm"), IronPDF uses numeric values in millimeters.
```csharp
// ❌ PuppeteerSharp style
// renderer.RenderingOptions.MarginTop = "20mm"; // Won't compile

// ✅ IronPDF style
renderer.RenderingOptions.MarginTop = 20; // Numeric value in mm
```

### 3. Synchronous API Available
**Issue:** PuppeteerSharp is fully async. IronPDF offers both sync and async methods.
```csharp
// ✅ Synchronous (simplifies code in many scenarios)
var pdf = renderer.RenderHtmlAsPdf(html);

// ✅ Asynchronous (for async contexts)
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### 4. Page Wait Behavior
**Issue:** PuppeteerSharp requires explicit wait strategies. IronPDF has intelligent defaults.
```csharp
// IronPDF waits for network idle automatically
// For custom timing:
renderer.RenderingOptions.RenderDelay = 1000; // milliseconds
renderer.RenderingOptions.Timeout = 60; // seconds
```

### 5. Licensing Required
**Issue:** IronPDF requires a license for production use (free for development).
```csharp
// Add at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

## Additional Resources

- **Documentation:** https://ironpdf.com/docs/
- **Tutorials:** https://ironpdf.com/tutorials/
- **API Reference:** https://ironpdf.com/docs/