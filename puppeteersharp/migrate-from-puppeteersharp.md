# How Do I Migrate from PuppeteerSharp to IronPDF in C#?

## Why Migrate from PuppeteerSharp to IronPDF

PuppeteerSharp is a .NET port of Google's Puppeteer designed for browser automation, where PDF generation is a secondary feature. While capable, using PuppeteerSharp for PDF generation creates significant production challenges:

- **300MB+ Chromium downloads** required before first use
- **Memory leaks under load** requiring manual browser recycling
- **Complex async patterns** with browser lifecycle management
- **Print-to-PDF output** (equivalent to Ctrl+P, not screen capture)
- **No PDF/A or PDF/UA support** for compliance requirements
- **No PDF manipulation** - generation only, no merge/split/edit

**IronPDF is purpose-built for PDF generation**, offering a leaner footprint, automatic memory management, and comprehensive PDF manipulation without browser automation overhead.

---

## The Browser Automation Problem

PuppeteerSharp was designed for web testing and scraping, not document generation. This creates fundamental issues when using it for PDFs:

| Aspect | PuppeteerSharp | IronPDF |
|--------|----------------|---------|
| **Primary Purpose** | Browser automation | PDF generation |
| **Chromium Dependency** | 300MB+ separate download | Built-in optimized engine |
| **API Complexity** | Async browser/page lifecycle | Synchronous one-liners |
| **Initialization** | `BrowserFetcher.DownloadAsync()` + LaunchAsync | `new ChromePdfRenderer()` |
| **Memory Management** | Manual browser recycling required | Automatic |
| **Memory Under Load** | 500MB+ with leaks | ~50MB stable |
| **Cold Start** | 45+ seconds | ~20 seconds |
| **PDF/A Support** | ❌ Not available | ✅ Full support |
| **PDF/UA Accessibility** | ❌ Not available | ✅ Full support |
| **PDF Editing** | ❌ Not available | ✅ Merge, split, stamp, edit |
| **Digital Signatures** | ❌ Not available | ✅ Full support |
| **Professional Support** | Community | Commercial with SLA |

---

## The Memory Leak Problem

PuppeteerSharp is notorious for memory accumulation under sustained load:

```csharp
// ❌ PuppeteerSharp - Memory grows with each operation
// Requires explicit browser recycling every N operations
for (int i = 0; i < 1000; i++)
{
    var page = await browser.NewPageAsync();
    await page.SetContentAsync($"<h1>Document {i}</h1>");
    await page.PdfAsync($"doc_{i}.pdf");
    await page.CloseAsync(); // Memory still accumulates!
}
// Must periodically: await browser.CloseAsync(); and re-launch

// ✅ IronPDF - Stable memory, reuse renderer
var renderer = new ChromePdfRenderer();
for (int i = 0; i < 1000; i++)
{
    var pdf = renderer.RenderHtmlAsPdf($"<h1>Document {i}</h1>");
    pdf.SaveAs($"doc_{i}.pdf");
    // Memory managed automatically
}
```

---

## NuGet Package Changes

```bash
# Remove PuppeteerSharp
dotnet remove package PuppeteerSharp

# Remove downloaded Chromium binaries (~300MB recovered)
# Delete the .local-chromium folder

# Add IronPDF
dotnet add package IronPdf
```

**No `BrowserFetcher.DownloadAsync()` required with IronPDF** - the rendering engine is bundled automatically.

---

## Namespace Mapping

| PuppeteerSharp | IronPDF |
|----------------|---------|
| `PuppeteerSharp` | `IronPdf` |
| `PuppeteerSharp.Media` | `IronPdf` |
| `BrowserFetcher` | Not needed |
| `LaunchOptions` | Not needed |
| `PdfOptions` | `ChromePdfRenderOptions` |
| `MarginOptions` | `RenderingOptions.Margin*` |
| N/A | `IronPdf.Editing` |
| N/A | `IronPdf.Signing` |

---

## API Mapping

| PuppeteerSharp API | IronPDF API | Notes |
|--------------------|-------------|-------|
| `new BrowserFetcher().DownloadAsync()` | Not needed | No browser download |
| `Puppeteer.LaunchAsync(options)` | Not needed | No browser management |
| `browser.NewPageAsync()` | Not needed | No page context |
| `page.GoToAsync(url)` | `renderer.RenderUrlAsPdf(url)` | Direct rendering |
| `page.SetContentAsync(html)` | `renderer.RenderHtmlAsPdf(html)` | Direct rendering |
| `page.PdfAsync(path)` | `pdf.SaveAs(path)` | After rendering |
| `page.PdfAsync(options)` | `renderer.RenderHtmlAsPdf()` | Options via RenderingOptions |
| `await page.CloseAsync()` | Not needed | Automatic cleanup |
| `await browser.CloseAsync()` | Not needed | Automatic cleanup |
| `PdfOptions.Format` | `RenderingOptions.PaperSize` | Paper size |
| `PdfOptions.Landscape` | `RenderingOptions.PaperOrientation` | Orientation |
| `PdfOptions.MarginOptions` | `RenderingOptions.MarginTop/Bottom/Left/Right` | Individual margins |
| `PdfOptions.PrintBackground` | `RenderingOptions.PrintHtmlBackgrounds` | Background printing |
| `PdfOptions.HeaderTemplate` | `RenderingOptions.HtmlHeader` | HTML headers |
| `PdfOptions.FooterTemplate` | `RenderingOptions.HtmlFooter` | HTML footers |
| `PdfOptions.Scale` | `RenderingOptions.Zoom` | Page zoom |
| `page.SetViewportAsync()` | `RenderingOptions.ViewPortWidth/Height` | Viewport control |
| `page.WaitForSelectorAsync()` | `RenderingOptions.WaitFor.HtmlElementId` | Wait for element |
| `page.WaitForNetworkIdleAsync()` | Automatic | Built-in intelligent waiting |
| N/A | `PdfDocument.Merge()` | Merge PDFs |
| N/A | `pdf.ApplyStamp()` | Add watermarks |
| N/A | `pdf.SecuritySettings` | Encrypt PDFs |
| N/A | `pdf.Sign()` | Digital signatures |

---

## Code Examples

### Example 1: Basic HTML to PDF

**Before (PuppeteerSharp) - Complex async with browser lifecycle:**
```csharp
using PuppeteerSharp;

public class PuppeteerPdfGenerator
{
    public async Task GeneratePdfAsync()
    {
        // Download Chromium (~300MB) if not already present
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        // Launch browser instance
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });

        // Create page context
        await using var page = await browser.NewPageAsync();

        // Set HTML content
        await page.SetContentAsync("<h1>Hello World</h1>");

        // Generate PDF
        await page.PdfAsync("output.pdf");

        // Cleanup handled by await using, but memory may still accumulate
    }
}
```

**After (IronPDF) - Simple synchronous call:**
```csharp
using IronPdf;

public class IronPdfGenerator
{
    public void GeneratePdf()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 2: URL to PDF with Options

**Before (PuppeteerSharp):**
```csharp
using PuppeteerSharp;
using PuppeteerSharp.Media;

public async Task UrlToPdfAsync(string url)
{
    var browserFetcher = new BrowserFetcher();
    await browserFetcher.DownloadAsync();

    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true
    });

    await using var page = await browser.NewPageAsync();

    // Navigate with wait strategy
    await page.GoToAsync(url, new NavigationOptions
    {
        WaitUntil = new[] { WaitUntilNavigation.NetworkIdle2 }
    });

    // Generate PDF with options
    await page.PdfAsync("output.pdf", new PdfOptions
    {
        Format = PaperFormat.A4,
        PrintBackground = true,
        MarginOptions = new MarginOptions
        {
            Top = "20mm",
            Bottom = "20mm",
            Left = "15mm",
            Right = "15mm"
        }
    });
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void UrlToPdf(string url)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PrintHtmlBackgrounds = true;
    renderer.RenderingOptions.MarginTop = 20;
    renderer.RenderingOptions.MarginBottom = 20;
    renderer.RenderingOptions.MarginLeft = 15;
    renderer.RenderingOptions.MarginRight = 15;

    var pdf = renderer.RenderUrlAsPdf(url);
    pdf.SaveAs("output.pdf");
}
```

### Example 3: Custom Page Size and Orientation

**Before (PuppeteerSharp):**
```csharp
using PuppeteerSharp;
using PuppeteerSharp.Media;

public async Task CustomPagePdfAsync()
{
    var browserFetcher = new BrowserFetcher();
    await browserFetcher.DownloadAsync();

    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true
    });

    await using var page = await browser.NewPageAsync();

    // Set viewport for consistent rendering
    await page.SetViewportAsync(new ViewPortOptions
    {
        Width = 1920,
        Height = 1080
    });

    await page.SetContentAsync("<h1>Landscape Document</h1>");

    await page.PdfAsync("custom.pdf", new PdfOptions
    {
        Format = PaperFormat.Letter,
        Landscape = true,
        Scale = 0.8m,
        PrintBackground = true
    });
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CustomPagePdf()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    // Viewport control
    renderer.RenderingOptions.ViewPortWidth = 1920;
    renderer.RenderingOptions.ViewPortHeight = 1080;

    // Page settings
    renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
    renderer.RenderingOptions.Zoom = 80; // Percentage (same as Scale 0.8)
    renderer.RenderingOptions.PrintHtmlBackgrounds = true;

    var pdf = renderer.RenderHtmlAsPdf("<h1>Landscape Document</h1>");
    pdf.SaveAs("custom.pdf");
}
```

### Example 4: Headers and Footers

**Before (PuppeteerSharp) - Limited template syntax:**
```csharp
using PuppeteerSharp;
using PuppeteerSharp.Media;

public async Task HeaderFooterPdfAsync()
{
    var browserFetcher = new BrowserFetcher();
    await browserFetcher.DownloadAsync();

    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true
    });

    await using var page = await browser.NewPageAsync();
    await page.SetContentAsync("<h1>Document with Header/Footer</h1>");

    await page.PdfAsync("output.pdf", new PdfOptions
    {
        DisplayHeaderFooter = true,
        // Limited Puppeteer template classes
        HeaderTemplate = @"
            <div style='font-size:10px; width:100%; text-align:center;'>
                <span class='title'></span>
            </div>",
        FooterTemplate = @"
            <div style='font-size:10px; width:100%; text-align:center;'>
                Page <span class='pageNumber'></span> of <span class='totalPages'></span>
            </div>",
        MarginOptions = new MarginOptions
        {
            Top = "100px",
            Bottom = "80px"
        }
    });
}
```

**After (IronPDF) - Full HTML/CSS support:**
```csharp
using IronPdf;

public void HeaderFooterPdf()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    // Full HTML header with images and styling
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = @"
            <div style='text-align: center; font-size: 12px; color: #333;'>
                <img src='logo.png' style='height: 30px;' />
                <span style='margin-left: 20px;'>{html-title}</span>
            </div>",
        DrawDividerLine = true
    };

    // Full HTML footer with dynamic placeholders
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = @"
            <div style='text-align: center; font-size: 10px; color: #666;'>
                Page {page} of {total-pages} | Generated: {date} {time}
            </div>",
        DrawDividerLine = true
    };

    renderer.RenderingOptions.MarginTop = 35;
    renderer.RenderingOptions.MarginBottom = 25;

    var pdf = renderer.RenderHtmlAsPdf("<h1>Document with Header/Footer</h1>");
    pdf.SaveAs("output.pdf");
}
```

**IronPDF Header/Footer Placeholders:**
- `{page}` - Current page number
- `{total-pages}` - Total page count
- `{date}` - Current date
- `{time}` - Current time
- `{html-title}` - Document title from HTML
- `{url}` - Source URL

### Example 5: Wait for Dynamic Content

**Before (PuppeteerSharp) - Complex wait strategies:**
```csharp
using PuppeteerSharp;

public async Task WaitForContentAsync()
{
    var browserFetcher = new BrowserFetcher();
    await browserFetcher.DownloadAsync();

    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true
    });

    await using var page = await browser.NewPageAsync();

    await page.GoToAsync("https://example.com/dynamic");

    // Multiple wait strategies
    await page.WaitForNetworkIdleAsync();

    // Wait for specific element
    await page.WaitForSelectorAsync("#content-loaded");

    // Additional delay for animations
    await Task.Delay(1000);

    await page.PdfAsync("output.pdf");
}
```

**After (IronPDF) - Simple configuration:**
```csharp
using IronPdf;

public void WaitForContent()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    // Wait for JavaScript/AJAX content
    renderer.RenderingOptions.WaitFor.RenderDelay = 2000; // milliseconds

    // Or wait for specific JavaScript condition
    renderer.RenderingOptions.WaitFor.JavaScript = "window.contentLoaded === true";

    // Or wait for HTML element
    renderer.RenderingOptions.WaitFor.HtmlElementId = "content-loaded";

    var pdf = renderer.RenderUrlAsPdf("https://example.com/dynamic");
    pdf.SaveAs("output.pdf");
}
```

### Example 6: JavaScript Execution

**Before (PuppeteerSharp):**
```csharp
using PuppeteerSharp;

public async Task ExecuteJavaScriptAsync()
{
    var browserFetcher = new BrowserFetcher();
    await browserFetcher.DownloadAsync();

    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true
    });

    await using var page = await browser.NewPageAsync();

    await page.SetContentAsync("<div id='target'>Original</div>");

    // Execute JavaScript manually
    await page.EvaluateExpressionAsync(@"
        document.getElementById('target').innerHTML = 'Modified by JavaScript';
        document.body.style.backgroundColor = '#f0f0f0';
    ");

    await page.PdfAsync("output.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void ExecuteJavaScript()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    // JavaScript runs automatically - include in HTML
    string html = @"
        <div id='target'>Original</div>
        <script>
            document.getElementById('target').innerHTML = 'Modified by JavaScript';
            document.body.style.backgroundColor = '#f0f0f0';
        </script>";

    // Enable JavaScript execution
    renderer.RenderingOptions.EnableJavaScript = true;
    renderer.RenderingOptions.WaitFor.RenderDelay = 500;

    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
```

### Example 7: Browser Pool Management (PuppeteerSharp Pattern)

**Before (PuppeteerSharp) - Complex pooling required:**
```csharp
using PuppeteerSharp;
using System.Collections.Concurrent;

public class PuppeteerBrowserPool
{
    private readonly ConcurrentBag<IBrowser> _browsers = new();
    private readonly SemaphoreSlim _semaphore = new(5);
    private int _operationCount = 0;
    private const int RecycleAfter = 100;

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        await _semaphore.WaitAsync();
        IBrowser browser = null;
        try
        {
            browser = await GetOrCreateBrowserAsync();

            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(html);
            var pdfBytes = await page.PdfDataAsync();

            // Track operations for recycling
            if (Interlocked.Increment(ref _operationCount) >= RecycleAfter)
            {
                await RecycleBrowserAsync(browser);
                Interlocked.Exchange(ref _operationCount, 0);
            }
            else
            {
                _browsers.Add(browser);
            }

            return pdfBytes;
        }
        catch
        {
            if (browser != null)
                await browser.CloseAsync();
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<IBrowser> GetOrCreateBrowserAsync()
    {
        if (_browsers.TryTake(out var browser))
            return browser;

        await new BrowserFetcher().DownloadAsync();
        return await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
    }

    private async Task RecycleBrowserAsync(IBrowser browser)
    {
        await browser.CloseAsync();
        // Memory freed after GC
    }
}
```

**After (IronPDF) - No pooling needed:**
```csharp
using IronPdf;

public class IronPdfGenerator
{
    // Reuse single renderer - thread-safe, automatic memory management
    private readonly ChromePdfRenderer _renderer = new();

    public IronPdfGenerator()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    }

    public byte[] GeneratePdf(string html)
    {
        // Thread-safe, no pooling or recycling needed
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Example 8: PDF Merging (Not Available in PuppeteerSharp)

**PuppeteerSharp:** Cannot merge PDFs - generation only.

**After (IronPDF):**
```csharp
using IronPdf;

public void MergePdfs()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    // Generate individual PDFs
    var cover = renderer.RenderHtmlAsPdf("<h1>Cover Page</h1>");
    var content = renderer.RenderHtmlAsPdf("<h1>Main Content</h1><p>Document body...</p>");
    var appendix = renderer.RenderHtmlAsPdf("<h1>Appendix</h1>");

    // Merge into single document
    var merged = PdfDocument.Merge(cover, content, appendix);
    merged.SaveAs("complete-document.pdf");
}
```

### Example 9: PDF Splitting (Not Available in PuppeteerSharp)

**PuppeteerSharp:** Cannot split PDFs.

**After (IronPDF):**
```csharp
using IronPdf;

public void SplitPdf()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var pdf = PdfDocument.FromFile("large-document.pdf");

    // Extract specific pages
    var firstChapter = pdf.CopyPages(0, 9);  // Pages 1-10
    firstChapter.SaveAs("chapter1.pdf");

    var secondChapter = pdf.CopyPages(10, 19);  // Pages 11-20
    secondChapter.SaveAs("chapter2.pdf");

    // Or split into individual pages
    for (int i = 0; i < pdf.PageCount; i++)
    {
        var singlePage = pdf.CopyPage(i);
        singlePage.SaveAs($"page_{i + 1}.pdf");
    }
}
```

### Example 10: Watermarks and Stamps (Not Available in PuppeteerSharp)

**PuppeteerSharp:** Cannot add watermarks to PDFs.

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

public void AddWatermark()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential Document</h1>");

    // Text watermark
    var watermark = new TextStamper
    {
        Text = "DRAFT",
        FontSize = 72,
        FontColor = IronSoftware.Drawing.Color.Red,
        Opacity = 25,
        Rotation = -45,
        VerticalAlignment = VerticalAlignment.Middle,
        HorizontalAlignment = HorizontalAlignment.Center
    };
    pdf.ApplyStamp(watermark);

    // Image watermark/stamp
    var logo = new ImageStamper(new Uri("company-logo.png"))
    {
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Right,
        Opacity = 50
    };
    pdf.ApplyStamp(logo);

    pdf.SaveAs("watermarked.pdf");
}
```

### Example 11: PDF Security (Not Available in PuppeteerSharp)

**PuppeteerSharp:** Cannot secure PDFs.

**After (IronPDF):**
```csharp
using IronPdf;

public void SecurePdf()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf("<h1>Secure Document</h1>");

    // Password protection
    pdf.SecuritySettings.OwnerPassword = "owner-can-edit";
    pdf.SecuritySettings.UserPassword = "user-can-view";

    // Restrict permissions
    pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrinting;
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;
    pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
    pdf.SecuritySettings.AllowUserAnnotations = false;

    pdf.SaveAs("secure.pdf");
}
```

### Example 12: Digital Signatures (Not Available in PuppeteerSharp)

**PuppeteerSharp:** Cannot digitally sign PDFs.

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Signing;

public void SignPdf()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf("<h1>Contract Agreement</h1><p>Terms and conditions...</p>");

    // Digital signature with certificate
    var signature = new PdfSignature("certificate.pfx", "password")
    {
        SigningContact = "legal@company.com",
        SigningLocation = "New York, USA",
        SigningReason = "Contract Approval"
    };

    pdf.Sign(signature);
    pdf.SaveAs("signed-contract.pdf");
}
```

### Example 13: PDF/A Compliance (Not Available in PuppeteerSharp)

**PuppeteerSharp:** Cannot create PDF/A documents.

**After (IronPDF):**
```csharp
using IronPdf;

public void CreatePdfA()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf("<h1>Archival Document</h1>");

    // Convert to PDF/A for long-term archival
    pdf.SaveAsPdfA("archive.pdf", PdfAVersions.PdfA3B);
}
```

### Example 14: Text Extraction (Not Available in PuppeteerSharp)

**PuppeteerSharp:** Cannot extract text from PDFs.

**After (IronPDF):**
```csharp
using IronPdf;

public void ExtractText()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var pdf = PdfDocument.FromFile("document.pdf");

    // Extract all text
    string allText = pdf.ExtractAllText();
    Console.WriteLine(allText);

    // Extract text from specific page
    string pageText = pdf.ExtractTextFromPage(0);
    Console.WriteLine($"Page 1: {pageText}");
}
```

### Example 15: Async Usage (When Needed)

**Before (PuppeteerSharp) - Async required:**
```csharp
using PuppeteerSharp;

public async Task<byte[]> GeneratePdfBytesAsync(string html)
{
    var browserFetcher = new BrowserFetcher();
    await browserFetcher.DownloadAsync();

    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true
    });

    await using var page = await browser.NewPageAsync();
    await page.SetContentAsync(html);
    return await page.PdfDataAsync();
}
```

**After (IronPDF) - Async optional:**
```csharp
using IronPdf;

// Synchronous (default - simpler code)
public byte[] GeneratePdfBytes(string html)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}

// Async (when needed for async contexts)
public async Task<byte[]> GeneratePdfBytesAsync(string html)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

Browser lifecycle management and memory leak resolution patterns are addressed in the [full migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-puppeteersharp-to-ironpdf/).

---

## Common Gotchas

### 1. No Browser Download Required
- **PuppeteerSharp:** Requires `BrowserFetcher.DownloadAsync()` to download ~300MB Chromium
- **IronPDF:** Rendering engine is bundled in the NuGet package

### 2. Async vs Sync
- **PuppeteerSharp:** All operations are async, requiring `async`/`await` throughout
- **IronPDF:** Synchronous by default, async methods available when needed

### 3. Browser Lifecycle Management
- **PuppeteerSharp:** Must manage browser launch, pages, and cleanup; memory leaks common
- **IronPDF:** Automatic - just create renderer and call methods

### 4. Margin Units
- **PuppeteerSharp:** Uses strings with units ("20mm", "1in")
- **IronPDF:** Uses numeric values in millimeters (25.4mm = 1 inch)

### 5. Header/Footer Templates
- **PuppeteerSharp:** Uses special CSS classes (`pageNumber`, `totalPages`)
- **IronPDF:** Uses placeholders like `{page}`, `{total-pages}`

### 6. PDF Output
- **PuppeteerSharp:** Writes directly to file path or returns bytes
- **IronPDF:** Returns `PdfDocument` object for further manipulation before saving

### 7. Wait Strategies
- **PuppeteerSharp:** Manual wait for NetworkIdle, selectors, etc.
- **IronPDF:** Intelligent defaults with optional `WaitFor` configuration

### 8. Memory Management
- **PuppeteerSharp:** Requires manual browser recycling every N operations
- **IronPDF:** Automatic memory management, stable under load

### 9. Thread Safety
- **PuppeteerSharp:** Limited - browser instances not fully thread-safe
- **IronPDF:** Full thread safety - reuse single renderer across threads

### 10. Licensing
- **PuppeteerSharp:** MIT open source
- **IronPDF:** Commercial license required for production

---

## Find All PuppeteerSharp References

```bash
# Find all PuppeteerSharp usages in your codebase
grep -r "PuppeteerSharp\|BrowserFetcher\|Puppeteer.LaunchAsync\|page.PdfAsync" --include="*.cs" .
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PuppeteerSharp PDF generation code**
  ```bash
  grep -r "PuppeteerSharp" --include="*.cs" .
  grep -r "BrowserFetcher\|Puppeteer.LaunchAsync" --include="*.cs" .
  grep -r "page.PdfAsync\|page.PdfDataAsync\|PdfOptions" --include="*.cs" .
  ```
  **Why:** PuppeteerSharp is browser automation where PDF is secondary. Identify all PDF generation to simplify with purpose-built IronPDF.

- [ ] **Document browser pooling or recycling logic**
  ```csharp
  // Find patterns like:
  private readonly ConcurrentBag<IBrowser> _browsers;
  if (operationCount >= RecycleAfter) await browser.CloseAsync();
  ```
  **Why:** PuppeteerSharp has memory leaks requiring manual browser recycling. IronPDF has automatic memory management—all pooling code can be deleted.

- [ ] **Document wait strategies used**
  ```csharp
  // Find patterns like:
  await page.WaitForNetworkIdleAsync();
  await page.WaitForSelectorAsync("#content");
  await Task.Delay(1000);
  ```
  **Why:** PuppeteerSharp requires complex async wait strategies. IronPDF simplifies with `WaitFor.RenderDelay` or `WaitFor.JavaScript()`.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Package Changes

- [ ] **Remove PuppeteerSharp package and browser binaries**
  ```bash
  dotnet remove package PuppeteerSharp
  # Delete browser binaries (~300MB saved!)
  rm -rf .local-chromium
  rm -rf ~/.cache/puppeteer  # Linux/Mac
  # Windows: Delete %USERPROFILE%\.cache\puppeteer
  ```
  **Why:** PuppeteerSharp requires 300MB+ Chromium download. IronPDF has zero external dependencies—the rendering engine is bundled in the NuGet package.

- [ ] **Install IronPdf**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** No `BrowserFetcher.DownloadAsync()` required. Just add the NuGet package and start using it.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Code Updates

- [ ] **Remove BrowserFetcher.DownloadAsync() calls**
  ```csharp
  // Before (PuppeteerSharp - delete this entire block)
  var browserFetcher = new BrowserFetcher();
  await browserFetcher.DownloadAsync();

  // After (IronPDF)
  // Nothing needed - rendering engine is bundled
  ```
  **Why:** IronPDF doesn't require downloading browser binaries.

- [ ] **Remove browser launch and lifecycle code**
  ```csharp
  // Before (PuppeteerSharp - delete all this)
  await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
  {
      Headless = true,
      Args = new[] { "--no-sandbox" }
  });
  await using var page = await browser.NewPageAsync();
  // ... use page ...
  await page.CloseAsync();
  await browser.CloseAsync();

  // After (IronPDF - just this)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF eliminates browser lifecycle management. No launch, no pages, no cleanup.

- [ ] **Replace URL rendering**
  ```csharp
  // Before (PuppeteerSharp)
  await page.GoToAsync(url, new NavigationOptions
  {
      WaitUntil = new[] { WaitUntilNavigation.NetworkIdle2 }
  });
  await page.PdfAsync("output.pdf");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderUrlAsPdf(url);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Single method call instead of goto + wait + pdf pattern.

- [ ] **Replace HTML rendering**
  ```csharp
  // Before (PuppeteerSharp)
  await page.SetContentAsync(html);
  await page.PdfAsync("output.pdf");
  // Or: var bytes = await page.PdfDataAsync();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  // Or: byte[] bytes = pdf.BinaryData;
  ```
  **Why:** Single method call instead of set content + generate pdf.

- [ ] **Update margin values from strings to millimeters**
  ```csharp
  // Before (PuppeteerSharp - string units)
  MarginOptions = new MarginOptions
  {
      Top = "20mm",
      Bottom = "1in",      // 25.4mm
      Left = "0.5in",      // 12.7mm
      Right = "15mm"
  }

  // After (IronPDF - numeric millimeters)
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 25;  // 1 inch = 25.4mm
  renderer.RenderingOptions.MarginLeft = 13;    // 0.5 inch ≈ 13mm
  renderer.RenderingOptions.MarginRight = 15;
  ```
  **Why:** IronPDF uses millimeters for all margins. Convert: 1 inch = 25.4mm.

- [ ] **Convert header/footer templates**
  ```csharp
  // Before (PuppeteerSharp - CSS class placeholders)
  HeaderTemplate = @"<div style='font-size:10px;'>
      <span class='title'></span>
  </div>",
  FooterTemplate = @"<div style='font-size:10px;'>
      Page <span class='pageNumber'></span> of <span class='totalPages'></span>
  </div>"

  // After (IronPDF - curly brace placeholders)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = @"<div style='font-size:10px;'>{html-title}</div>"
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = @"<div style='font-size:10px;'>
          Page {page} of {total-pages}
      </div>"
  };
  ```
  **Why:** Different placeholder syntax. PuppeteerSharp: `<span class='pageNumber'>`, IronPDF: `{page}`.

- [ ] **Placeholder syntax conversion reference**
  ```csharp
  // PuppeteerSharp class → IronPDF placeholder
  // pageNumber     → {page}
  // totalPages     → {total-pages}
  // date           → {date}
  // title          → {html-title}
  // url            → {url}
  ```
  **Why:** Search and replace all PuppeteerSharp class-based placeholders with IronPDF curly brace format.

- [ ] **Replace wait strategies**
  ```csharp
  // Before (PuppeteerSharp - multiple async waits)
  await page.WaitForNetworkIdleAsync();
  await page.WaitForSelectorAsync("#content-loaded");
  await Task.Delay(1000);

  // After (IronPDF - simple options)
  // Option 1: Fixed delay
  renderer.RenderingOptions.WaitFor.RenderDelay = 2000;

  // Option 2: Wait for JavaScript condition
  renderer.RenderingOptions.WaitFor.JavaScript("window.loaded === true", 5000);

  // Option 3: Wait for element
  renderer.RenderingOptions.WaitFor.HtmlElementId = "content-loaded";
  ```
  **Why:** IronPDF provides simpler wait strategies without async complexity.

- [ ] **Delete browser pooling infrastructure**
  ```csharp
  // Before (PuppeteerSharp - delete entire class)
  public class PuppeteerBrowserPool
  {
      private readonly ConcurrentBag<IBrowser> _browsers;
      private readonly SemaphoreSlim _semaphore;
      private int _operationCount;
      // ... recycling logic ...
  }

  // After (IronPDF - simple reuse)
  public class PdfService
  {
      private readonly ChromePdfRenderer _renderer = new();

      public byte[] Generate(string html)
      {
          return _renderer.RenderHtmlAsPdf(html).BinaryData;
      }
  }
  ```
  **Why:** IronPDF has automatic memory management. No pooling, recycling, or semaphores needed.

- [ ] **Convert page settings**
  ```csharp
  // Before (PuppeteerSharp)
  await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });
  await page.PdfAsync(new PdfOptions
  {
      Format = PaperFormat.A4,
      Landscape = true,
      Scale = 0.8m,
      PrintBackground = true
  });

  // After (IronPDF)
  renderer.RenderingOptions.ViewPortWidth = 1920;
  renderer.RenderingOptions.ViewPortHeight = 1080;
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  renderer.RenderingOptions.Zoom = 80; // percentage instead of decimal
  renderer.RenderingOptions.PrintHtmlBackgrounds = true;
  ```
  **Why:** Direct property mapping. Note Scale 0.8 becomes Zoom 80 (percentage).

- [ ] **Convert async to sync (when appropriate)**
  ```csharp
  // Before (PuppeteerSharp - async required)
  public async Task<byte[]> GeneratePdfAsync(string html)
  {
      await new BrowserFetcher().DownloadAsync();
      await using var browser = await Puppeteer.LaunchAsync(...);
      await using var page = await browser.NewPageAsync();
      await page.SetContentAsync(html);
      return await page.PdfDataAsync();
  }

  // After (IronPDF - sync default)
  public byte[] GeneratePdf(string html)
  {
      var renderer = new ChromePdfRenderer();
      return renderer.RenderHtmlAsPdf(html).BinaryData;
  }

  // Or async when needed
  public async Task<byte[]> GeneratePdfAsync(string html)
  {
      var renderer = new ChromePdfRenderer();
      var pdf = await renderer.RenderHtmlAsPdfAsync(html);
      return pdf.BinaryData;
  }
  ```
  **Why:** IronPDF supports both sync and async. Sync is simpler for most use cases.

### Testing

- [ ] **Visual comparison of PDF output**
  **Why:** Chromium rendering should be identical, but verify key documents look correct.

- [ ] **Verify header/footer rendering**
  **Why:** Confirm placeholder conversion worked correctly. Check `{page}` and `{total-pages}` display.

- [ ] **Load test for memory stability**
  ```csharp
  // Run 1000 conversions and monitor memory
  var renderer = new ChromePdfRenderer();
  for (int i = 0; i < 1000; i++)
  {
      var pdf = renderer.RenderHtmlAsPdf($"<h1>Doc {i}</h1>");
      pdf.SaveAs($"test_{i}.pdf");
  }
  // Memory should stay stable - no recycling needed
  ```
  **Why:** PuppeteerSharp leaked memory requiring browser recycling. Verify IronPDF stays stable under load.

- [ ] **Validate margin and page sizing**
  **Why:** Confirm millimeter conversions are correct. Compare output dimensions with PuppeteerSharp originals.

### Post-Migration Benefits (New Capabilities)

- [ ] **Implement PDF merging/splitting (not available in PuppeteerSharp)**
  ```csharp
  // Merge multiple PDFs
  var pdf1 = renderer.RenderHtmlAsPdf(html1);
  var pdf2 = renderer.RenderHtmlAsPdf(html2);
  var merged = PdfDocument.Merge(pdf1, pdf2);

  // Split pages
  var pages1to5 = pdf.CopyPages(0, 4);
  ```
  **Why:** PuppeteerSharp can only generate PDFs. IronPDF can manipulate them after creation.

- [ ] **Add watermarks and stamps (not available in PuppeteerSharp)**
  ```csharp
  var watermark = new TextStamper
  {
      Text = "CONFIDENTIAL",
      FontSize = 48,
      Opacity = 30,
      Rotation = -45
  };
  pdf.ApplyStamp(watermark);
  ```
  **Why:** Add confidential marks, draft indicators, or branding to generated PDFs.

- [ ] **Configure PDF security (not available in PuppeteerSharp)**
  ```csharp
  pdf.SecuritySettings.OwnerPassword = "admin";
  pdf.SecuritySettings.UserPassword = "readonly";
  pdf.SecuritySettings.AllowUserCopyPasteContent = false;
  pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrinting;
  ```
  **Why:** Password protect and restrict permissions on PDFs.

- [ ] **Add digital signatures (not available in PuppeteerSharp)**
  ```csharp
  var signature = new PdfSignature("certificate.pfx", "password");
  pdf.Sign(signature);
  ```
  **Why:** Legally sign contracts and official documents.

- [ ] **Create PDF/A compliant documents (not available in PuppeteerSharp)**
  ```csharp
  pdf.SaveAsPdfA("archive.pdf", PdfAVersions.PdfA3b);
  ```
  **Why:** Generate archival-compliant PDFs for legal/regulatory requirements.

- [ ] **Extract text from PDFs (not available in PuppeteerSharp)**
  ```csharp
  var pdf = PdfDocument.FromFile("document.pdf");
  string text = pdf.ExtractAllText();
  ```
  **Why:** Search, index, or process PDF content programmatically.

---

## Performance Comparison Summary

| Metric | PuppeteerSharp | IronPDF | Improvement |
|--------|----------------|---------|-------------|
| First PDF (Cold Start) | 45s+ | ~20s | **55%+ faster** |
| Subsequent PDFs | Variable | Consistent | **Predictable** |
| Memory Usage | 500MB+ (grows) | ~50MB (stable) | **90% less memory** |
| Disk Space (Chromium) | 300MB+ | 0 | **Eliminate downloads** |
| Browser Download | Required | Not needed | **Zero setup** |
| Thread Safety | Limited | Full | **Reliable concurrency** |

---

## Additional Resources

- **IronPDF Documentation:** https://ironpdf.com/docs/
- **Tutorials and Examples:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference:** https://ironpdf.com/object-reference/api/
