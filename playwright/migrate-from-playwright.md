# How Do I Migrate from Playwright for .NET to IronPDF in C#?

## Why Migrate from Playwright to IronPDF

Playwright for .NET is a browser automation and testing framework where PDF generation is a secondary feature. While capable, using Playwright for PDF generation means dealing with:

- **400MB+ browser downloads** required before first use
- **Complex async patterns** with browser contexts and page management
- **Testing-first architecture** not optimized for document generation
- **Print-to-PDF limitations** equivalent to Ctrl+P browser print
- **No PDF/A or PDF/UA support** for accessibility compliance
- **Resource-heavy operations** requiring full browser instances

**IronPDF is purpose-built for PDF generation**, offering a simpler API, better performance, and professional document features without the browser automation overhead.

---

## The Testing Framework Problem

Playwright was designed for end-to-end testing, not document generation. This creates fundamental issues when using it for PDFs:

| Aspect | Playwright | IronPDF |
|--------|------------|---------|
| **Primary Purpose** | Browser testing | PDF generation |
| **Browser Download** | 400MB+ (Chromium, Firefox, WebKit) | Built-in optimized engine |
| **API Complexity** | Async browser/context/page lifecycle | Synchronous one-liners |
| **Initialization** | `playwright install` + CreateAsync + LaunchAsync | `new ChromePdfRenderer()` |
| **Memory Usage** | 280-420MB per conversion | 80-120MB per conversion |
| **Cold Start** | 4.5 seconds | 2.8 seconds |
| **Subsequent Renders** | 3.8-4.1 seconds | 0.8-1.2 seconds |
| **PDF/A Support** | ❌ Not available | ✅ Full support |
| **PDF/UA Accessibility** | ❌ Not available | ✅ Full support |
| **Digital Signatures** | ❌ Not available | ✅ Full support |
| **PDF Editing** | ❌ Not available | ✅ Merge, split, stamp, edit |
| **Professional Support** | Community | Commercial with SLA |

---

## NuGet Package Changes

```bash
# Remove Playwright
dotnet remove package Microsoft.Playwright

# Remove browser binaries (reclaim ~400MB disk space)
# Delete the .playwright folder in your project or user directory

# Add IronPDF
dotnet add package IronPdf
```

**No `playwright install` required with IronPDF** - the rendering engine is bundled automatically.

---

## Namespace Mapping

| Playwright | IronPDF |
|------------|---------|
| `Microsoft.Playwright` | `IronPdf` |
| `IPlaywright` | Not needed |
| `IBrowser` | Not needed |
| `IBrowserContext` | Not needed |
| `IPage` | Not needed |
| `PagePdfOptions` | `ChromePdfRenderOptions` |
| `Margin` | `RenderingOptions.Margin*` properties |

---

## API Mapping

| Playwright API | IronPDF API | Notes |
|----------------|-------------|-------|
| `Playwright.CreateAsync()` | `new ChromePdfRenderer()` | No async needed |
| `playwright.Chromium.LaunchAsync()` | Not needed | No browser management |
| `browser.NewPageAsync()` | Not needed | No page context |
| `browser.NewContextAsync()` | Not needed | No context management |
| `page.GotoAsync(url)` | `renderer.RenderUrlAsPdf(url)` | Direct URL rendering |
| `page.SetContentAsync(html)` + `page.PdfAsync()` | `renderer.RenderHtmlAsPdf(html)` | Single method |
| `page.PdfAsync(options)` | `renderer.RenderHtmlAsPdf()` | Options via RenderingOptions |
| `page.CloseAsync()` | Not needed | Automatic cleanup |
| `browser.CloseAsync()` | Not needed | Automatic cleanup |
| `playwright.Dispose()` | Not needed | Automatic cleanup |
| `PagePdfOptions.Format` | `RenderingOptions.PaperSize` | Paper size |
| `PagePdfOptions.Margin` | `RenderingOptions.MarginTop/Bottom/Left/Right` | Individual margins |
| `PagePdfOptions.PrintBackground` | `RenderingOptions.PrintHtmlBackgrounds` | Background printing |
| `PagePdfOptions.HeaderTemplate` | `RenderingOptions.HtmlHeader` | HTML headers |
| `PagePdfOptions.FooterTemplate` | `RenderingOptions.HtmlFooter` | HTML footers |
| `PagePdfOptions.Scale` | `RenderingOptions.Zoom` | Page zoom |
| `PagePdfOptions.Landscape` | `RenderingOptions.PaperOrientation` | Orientation |
| `page.SetViewportSizeAsync()` | `RenderingOptions.ViewPortWidth/Height` | Viewport control |
| N/A | `pdf.Merge()` | Merge PDFs |
| N/A | `pdf.ApplyStamp()` | Add watermarks |
| N/A | `pdf.SecuritySettings` | Encrypt PDFs |
| N/A | `pdf.SignWithFile()` | Digital signatures |

---

## Code Examples

### Example 1: Basic HTML to PDF

**Before (Playwright) - Complex async with browser lifecycle:**
```csharp
using Microsoft.Playwright;

public class PlaywrightPdfGenerator
{
    public async Task GeneratePdfAsync()
    {
        // Initialize Playwright (requires 'playwright install' first)
        using var playwright = await Playwright.CreateAsync();

        // Launch browser instance
        await using var browser = await playwright.Chromium.LaunchAsync();

        // Create page context
        var page = await browser.NewPageAsync();

        // Set HTML content
        await page.SetContentAsync("<h1>Hello World</h1>");

        // Generate PDF
        await page.PdfAsync(new PagePdfOptions { Path = "output.pdf" });

        // Cleanup (await using handles this, but explicit cleanup often needed)
        await page.CloseAsync();
        await browser.CloseAsync();
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

### Example 2: URL to PDF

**Before (Playwright):**
```csharp
using Microsoft.Playwright;

public async Task UrlToPdfAsync(string url)
{
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync();
    var page = await browser.NewPageAsync();

    // Navigate to URL
    await page.GotoAsync(url);

    // Wait for network idle (common pattern)
    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Generate PDF
    await page.PdfAsync(new PagePdfOptions
    {
        Path = "output.pdf",
        Format = "A4"
    });

    await browser.CloseAsync();
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

    var pdf = renderer.RenderUrlAsPdf(url);
    pdf.SaveAs("output.pdf");
}
```

### Example 3: Custom Page Settings and Margins

**Before (Playwright):**
```csharp
using Microsoft.Playwright;

public async Task CustomPdfAsync()
{
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync();
    var page = await browser.NewPageAsync();

    // Set viewport for consistent rendering
    await page.SetViewportSizeAsync(1920, 1080);

    await page.SetContentAsync("<h1>Custom Document</h1>");

    await page.PdfAsync(new PagePdfOptions
    {
        Path = "custom.pdf",
        Format = "Letter",
        Landscape = true,
        Margin = new Margin
        {
            Top = "1in",
            Bottom = "1in",
            Left = "0.75in",
            Right = "0.75in"
        },
        PrintBackground = true,
        Scale = 0.8f
    });

    await browser.CloseAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CustomPdf()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    // Viewport control
    renderer.RenderingOptions.ViewPortWidth = 1920;
    renderer.RenderingOptions.ViewPortHeight = 1080;

    // Page settings
    renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

    // Margins in millimeters (25.4mm = 1 inch, 19mm ≈ 0.75 inch)
    renderer.RenderingOptions.MarginTop = 25;
    renderer.RenderingOptions.MarginBottom = 25;
    renderer.RenderingOptions.MarginLeft = 19;
    renderer.RenderingOptions.MarginRight = 19;

    // Print options
    renderer.RenderingOptions.PrintHtmlBackgrounds = true;
    renderer.RenderingOptions.Zoom = 80; // Percentage

    var pdf = renderer.RenderHtmlAsPdf("<h1>Custom Document</h1>");
    pdf.SaveAs("custom.pdf");
}
```

### Example 4: Headers and Footers

**Before (Playwright) - Limited template syntax:**
```csharp
using Microsoft.Playwright;

public async Task HeaderFooterPdfAsync()
{
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync();
    var page = await browser.NewPageAsync();

    await page.SetContentAsync("<h1>Document with Header/Footer</h1>");

    await page.PdfAsync(new PagePdfOptions
    {
        Path = "output.pdf",
        DisplayHeaderFooter = true,
        // Limited Playwright template classes: date, title, url, pageNumber, totalPages
        HeaderTemplate = @"
            <div style='font-size:10px; width:100%; text-align:center;'>
                <span class='title'></span>
            </div>",
        FooterTemplate = @"
            <div style='font-size:10px; width:100%; text-align:center;'>
                Page <span class='pageNumber'></span> of <span class='totalPages'></span>
            </div>",
        Margin = new Margin { Top = "100px", Bottom = "80px" }
    });

    await browser.CloseAsync();
}
```

**After (IronPDF) - Full HTML/CSS support:**
```csharp
using IronPdf;

public void HeaderFooterPdf()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    // Full HTML header with styling
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

**Before (Playwright) - Complex wait strategies:**
```csharp
using Microsoft.Playwright;

public async Task WaitForContentAsync()
{
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync();
    var page = await browser.NewPageAsync();

    await page.GotoAsync("https://example.com/dynamic");

    // Multiple wait strategies often needed
    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

    // Wait for specific element
    await page.WaitForSelectorAsync("#content-loaded");

    // Additional delay for animations
    await Task.Delay(1000);

    await page.PdfAsync(new PagePdfOptions { Path = "output.pdf" });
    await browser.CloseAsync();
}
```

**After (IronPDF) - Simple render delay:**
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

**Before (Playwright):**
```csharp
using Microsoft.Playwright;

public async Task ExecuteJavaScriptAsync()
{
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync();
    var page = await browser.NewPageAsync();

    await page.SetContentAsync("<div id='target'>Original</div>");

    // Execute JavaScript
    await page.EvaluateAsync(@"
        document.getElementById('target').innerHTML = 'Modified by JavaScript';
        document.body.style.backgroundColor = '#f0f0f0';
    ");

    await page.PdfAsync(new PagePdfOptions { Path = "output.pdf" });
    await browser.CloseAsync();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void ExecuteJavaScript()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    // JavaScript runs automatically - just include in HTML
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

### Example 7: Multiple Browser Support (Migration Consideration)

**Before (Playwright) - Testing different browsers:**
```csharp
using Microsoft.Playwright;

public async Task MultiBrowserTestAsync()
{
    using var playwright = await Playwright.CreateAsync();

    // Test across browsers (useful for testing, not PDF generation)
    var browsers = new[]
    {
        await playwright.Chromium.LaunchAsync(),
        await playwright.Firefox.LaunchAsync(),
        await playwright.Webkit.LaunchAsync()
    };

    foreach (var browser in browsers)
    {
        var page = await browser.NewPageAsync();
        await page.SetContentAsync("<h1>Multi-browser test</h1>");
        // Firefox and WebKit don't support page.PdfAsync()!
        // Only Chromium supports PDF generation in Playwright
        await browser.CloseAsync();
    }
}
```

**After (IronPDF) - Consistent Chromium rendering:**
```csharp
using IronPdf;

public void ConsistentRendering()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    // IronPDF uses a single optimized Chromium engine
    // Consistent output every time, no browser variance
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf("<h1>Consistent rendering</h1>");
    pdf.SaveAs("output.pdf");
}
```

### Example 8: PDF Modification (Not Available in Playwright)

**Playwright:** Cannot modify PDFs after generation - writes directly to file.

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

public void ModifyPdf()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    // Generate initial PDF
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf("<h1>Original Document</h1>");

    // Add watermark
    var watermark = new TextStamper
    {
        Text = "CONFIDENTIAL",
        FontSize = 48,
        FontColor = IronSoftware.Drawing.Color.Red,
        Opacity = 30,
        Rotation = -45,
        VerticalAlignment = VerticalAlignment.Middle,
        HorizontalAlignment = HorizontalAlignment.Center
    };
    pdf.ApplyStamp(watermark);

    // Add another PDF
    var appendix = renderer.RenderHtmlAsPdf("<h1>Appendix</h1>");
    pdf.AppendPdf(appendix);

    // Add page numbers
    var pageNumbers = new TextStamper
    {
        Text = "Page {page} of {total-pages}",
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Center
    };
    pdf.ApplyStamp(pageNumbers);

    pdf.SaveAs("modified.pdf");
}
```

### Example 9: PDF Security (Not Available in Playwright)

**Playwright:** No PDF security features.

**After (IronPDF):**
```csharp
using IronPdf;

public void SecurePdf()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf("<h1>Secure Document</h1>");

    // Password protection
    pdf.SecuritySettings.OwnerPassword = "owner123";
    pdf.SecuritySettings.UserPassword = "user123";

    // Restrict permissions
    pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrinting;
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;
    pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

    pdf.SaveAs("secure.pdf");
}
```

### Example 10: Digital Signatures (Not Available in Playwright)

**Playwright:** No digital signature support.

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Signing;

public void SignPdf()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf("<h1>Contract Agreement</h1>");

    // Digital signature with certificate
    var signature = new PdfSignature("certificate.pfx", "password")
    {
        SigningContact = "legal@company.com",
        SigningLocation = "New York, USA",
        SigningReason = "Contract Approval"
    };

    pdf.Sign(signature);
    pdf.SaveAs("signed.pdf");
}
```

### Example 11: PDF/A Compliance (Not Available in Playwright)

**Playwright:** Cannot create PDF/A documents for archival compliance.

**After (IronPDF):**
```csharp
using IronPdf;

public void CreatePdfA()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf("<h1>Archival Document</h1>");

    // Convert to PDF/A-3B for long-term archival
    pdf.SaveAsPdfA("archive.pdf", PdfAVersions.PdfA3B);
}
```

### Example 12: Async Usage (When Needed)

**Before (Playwright) - Async required:**
```csharp
using Microsoft.Playwright;

public async Task<byte[]> GeneratePdfBytesAsync(string html)
{
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync();
    var page = await browser.NewPageAsync();

    await page.SetContentAsync(html);
    var pdfBytes = await page.PdfAsync();

    await browser.CloseAsync();
    return pdfBytes;
}
```

**After (IronPDF) - Async optional:**
```csharp
using IronPdf;

// Synchronous (default)
public byte[] GeneratePdfBytes(string html)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}

// Async (when needed)
public async Task<byte[]> GeneratePdfBytesAsync(string html)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

---

## Common Gotchas

### 1. No Browser Installation Required
- **Playwright:** Requires `playwright install` to download ~400MB of browsers
- **IronPDF:** Rendering engine is bundled in the NuGet package

### 2. Async vs Sync
- **Playwright:** All operations are async, requiring `async`/`await` patterns
- **IronPDF:** Synchronous by default, async methods available when needed

### 3. Browser Lifecycle Management
- **Playwright:** Must manage browser launch, context creation, page creation, and cleanup
- **IronPDF:** Automatic - just create renderer and call methods

### 4. Margin Units
- **Playwright:** Uses strings with units ("1cm", "10px", "0.5in")
- **IronPDF:** Uses numeric values in millimeters (25.4mm = 1 inch)

### 5. Header/Footer Templates
- **Playwright:** Uses special CSS classes (`pageNumber`, `totalPages`, `date`, etc.)
- **IronPDF:** Uses placeholders like `{page}`, `{total-pages}`, `{date}`

### 6. PDF Output
- **Playwright:** Writes directly to file path, returns bytes
- **IronPDF:** Returns `PdfDocument` object for further manipulation before saving

### 7. Print vs Screen Rendering
- **Playwright:** Uses browser print mode by default (may differ from screen)
- **IronPDF:** Uses `CssMediaType.Screen` or `CssMediaType.Print` - your choice

### 8. Resource Disposal
- **Playwright:** Requires explicit disposal of browser, context, and page
- **IronPDF:** Automatic resource management

### 9. Test Framework Integration
- **Playwright:** Designed for testing frameworks (NUnit, xUnit, MSTest)
- **IronPDF:** Works in any context - not tied to testing infrastructure

### 10. Performance for Multiple PDFs
- **Playwright:** Each PDF may require new page/context management
- **IronPDF:** Reuse single `ChromePdfRenderer` for multiple PDFs efficiently

---

## Find All Playwright References

```bash
# Find all Playwright PDF usages in your codebase
grep -r "Microsoft.Playwright\|page.PdfAsync\|PagePdfOptions\|Playwright.CreateAsync" --include="*.cs" .
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Playwright PDF generation code**
  ```bash
  grep -r "Microsoft.Playwright" --include="*.cs" .
  grep -r "page.PdfAsync\|PagePdfOptions" --include="*.cs" .
  grep -r "Playwright.CreateAsync\|browser.NewPageAsync" --include="*.cs" .
  ```
  **Why:** Playwright is a testing framework where PDF is secondary. Identify all PDF generation patterns to simplify with purpose-built IronPDF.

- [ ] **Document header/footer templates**
  ```csharp
  // Find patterns like:
  HeaderTemplate = "<div><span class='pageNumber'></span></div>"
  FooterTemplate = "<div><span class='totalPages'></span></div>"
  ```
  **Why:** Playwright uses CSS class-based placeholders (`pageNumber`, `totalPages`). IronPDF uses `{page}`, `{total-pages}` format.

- [ ] **List wait strategies used**
  ```csharp
  // Find patterns like:
  await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
  await page.WaitForSelectorAsync("#content-loaded");
  await Task.Delay(1000);
  ```
  **Why:** Playwright requires complex async wait strategies. IronPDF simplifies with `WaitFor.RenderDelay` or `WaitFor.JavaScript()`.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Package Changes

- [ ] **Remove Playwright packages and browser binaries**
  ```bash
  dotnet remove package Microsoft.Playwright
  # Delete browser binaries (~400MB saved!)
  rm -rf .playwright
  rm -rf ~/.cache/ms-playwright  # Linux/Mac
  # Windows: Delete %USERPROFILE%\.cache\ms-playwright
  ```
  **Why:** Playwright requires 400MB+ of browser downloads. IronPDF has zero external dependencies—the rendering engine is bundled in the NuGet package.

- [ ] **Install IronPdf**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** No `playwright install` required. Just add the NuGet package and start using it.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Code Updates

- [ ] **Replace async browser lifecycle with ChromePdfRenderer**
  ```csharp
  // Before (Playwright - complex async lifecycle)
  using var playwright = await Playwright.CreateAsync();
  await using var browser = await playwright.Chromium.LaunchAsync();
  var page = await browser.NewPageAsync();
  await page.SetContentAsync(html);
  await page.PdfAsync(new PagePdfOptions { Path = "output.pdf" });
  await page.CloseAsync();
  await browser.CloseAsync();

  // After (IronPDF - simple)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF eliminates browser lifecycle management. No CreateAsync, LaunchAsync, NewPageAsync, or disposal code needed.

- [ ] **Convert URL rendering**
  ```csharp
  // Before (Playwright)
  var page = await browser.NewPageAsync();
  await page.GotoAsync(url);
  await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
  await page.PdfAsync(new PagePdfOptions { Path = "output.pdf" });

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderUrlAsPdf(url);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Single method call instead of goto + wait + pdf pattern.

- [ ] **Update margin values from strings to millimeters**
  ```csharp
  // Before (Playwright - string units)
  Margin = new Margin
  {
      Top = "1in",      // 25.4mm
      Bottom = "0.75in", // 19mm
      Left = "1cm",     // 10mm
      Right = "20px"    // ~7.5mm at 96dpi
  }

  // After (IronPDF - numeric millimeters)
  renderer.RenderingOptions.MarginTop = 25;    // mm
  renderer.RenderingOptions.MarginBottom = 19;
  renderer.RenderingOptions.MarginLeft = 10;
  renderer.RenderingOptions.MarginRight = 8;
  ```
  **Why:** IronPDF uses millimeters for all margins. Convert: 1 inch = 25.4mm, 1cm = 10mm, pixels ÷ 96 × 25.4 = mm.

- [ ] **Convert header/footer templates**
  ```csharp
  // Before (Playwright - CSS class placeholders)
  HeaderTemplate = @"<div style='font-size:10px;'>
      <span class='title'></span>
      <span class='date'></span>
  </div>",
  FooterTemplate = @"<div style='font-size:10px;'>
      Page <span class='pageNumber'></span> of <span class='totalPages'></span>
  </div>"

  // After (IronPDF - curly brace placeholders)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = @"<div style='font-size:10px;'>
          {html-title} {date}
      </div>"
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = @"<div style='font-size:10px;'>
          Page {page} of {total-pages}
      </div>"
  };
  ```
  **Why:** Different placeholder syntax. Playwright: `<span class='pageNumber'>`, IronPDF: `{page}`.

- [ ] **Placeholder syntax conversion reference**
  ```csharp
  // Playwright class → IronPDF placeholder
  // pageNumber     → {page}
  // totalPages     → {total-pages}
  // date           → {date}
  // title          → {html-title}
  // url            → {url}
  ```
  **Why:** Search and replace all Playwright class-based placeholders with IronPDF curly brace format.

- [ ] **Replace wait strategies**
  ```csharp
  // Before (Playwright - multiple async waits)
  await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
  await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
  await page.WaitForSelectorAsync("#content-loaded");
  await Task.Delay(1000);

  // After (IronPDF - simple options)
  // Option 1: Fixed delay
  renderer.RenderingOptions.WaitFor.RenderDelay = 2000; // ms

  // Option 2: Wait for JavaScript condition
  renderer.RenderingOptions.WaitFor.JavaScript("window.contentLoaded === true", 5000);

  // Option 3: Wait for element
  renderer.RenderingOptions.WaitFor.HtmlElementId = "content-loaded";
  ```
  **Why:** IronPDF provides simpler wait strategies without async complexity.

- [ ] **Remove browser/page/context disposal code**
  ```csharp
  // Before (Playwright - explicit cleanup required)
  await page.CloseAsync();
  await context.CloseAsync();
  await browser.CloseAsync();
  playwright.Dispose();

  // After (IronPDF - no disposal needed)
  // Just use the renderer - automatic cleanup
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  // No cleanup code!
  ```
  **Why:** IronPDF handles resource management automatically. Delete all browser lifecycle cleanup code.

- [ ] **Convert page settings**
  ```csharp
  // Before (Playwright)
  await page.SetViewportSizeAsync(1920, 1080);
  await page.PdfAsync(new PagePdfOptions
  {
      Format = "A4",
      Landscape = true,
      PrintBackground = true,
      Scale = 0.8f
  });

  // After (IronPDF)
  renderer.RenderingOptions.ViewPortWidth = 1920;
  renderer.RenderingOptions.ViewPortHeight = 1080;
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  renderer.RenderingOptions.PrintHtmlBackgrounds = true;
  renderer.RenderingOptions.Zoom = 80; // percentage instead of decimal
  ```
  **Why:** Direct property mapping. Note Zoom is percentage (80) not decimal (0.8).

- [ ] **Convert async to sync (when appropriate)**
  ```csharp
  // Before (Playwright - async required)
  public async Task<byte[]> GeneratePdfAsync(string html)
  {
      using var playwright = await Playwright.CreateAsync();
      // ... many async calls ...
      return await page.PdfAsync();
  }

  // After (IronPDF - sync default, async optional)
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

- [ ] **Test dynamic content loading**
  **Why:** Verify `WaitFor` options work for JavaScript-heavy pages that previously needed NetworkIdle waits.

- [ ] **Validate margin and page sizing**
  **Why:** Confirm millimeter conversions are correct. Compare output dimensions with Playwright originals.

### Post-Migration Benefits (New Capabilities)

- [ ] **Implement PDF merging/splitting (not available in Playwright)**
  ```csharp
  // Merge multiple PDFs
  var pdf1 = renderer.RenderHtmlAsPdf(html1);
  var pdf2 = renderer.RenderHtmlAsPdf(html2);
  var merged = PdfDocument.Merge(pdf1, pdf2);

  // Split pages
  var pages1to5 = pdf.CopyPages(0, 4);
  ```
  **Why:** Playwright can only generate PDFs. IronPDF can manipulate them after creation.

- [ ] **Add watermarks and stamps (not available in Playwright)**
  ```csharp
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>",
      50, IronPdf.Editing.VerticalAlignment.Middle);
  ```
  **Why:** Add confidential marks, draft indicators, or branding to generated PDFs.

- [ ] **Configure PDF security (not available in Playwright)**
  ```csharp
  pdf.SecuritySettings.OwnerPassword = "admin";
  pdf.SecuritySettings.UserPassword = "readonly";
  pdf.SecuritySettings.AllowUserCopyPasteContent = false;
  pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrinting;
  ```
  **Why:** Password protect and restrict permissions on PDFs.

- [ ] **Add digital signatures (not available in Playwright)**
  ```csharp
  var signature = new PdfSignature("certificate.pfx", "password");
  pdf.Sign(signature);
  ```
  **Why:** Legally sign contracts and official documents.

- [ ] **Create PDF/A compliant documents (not available in Playwright)**
  ```csharp
  pdf.SaveAsPdfA("archive.pdf", PdfAVersions.PdfA3b);
  ```
  **Why:** Generate archival-compliant PDFs for legal/regulatory requirements.

---

## Performance Comparison Summary

| Metric | Playwright | IronPDF | Improvement |
|--------|------------|---------|-------------|
| First PDF (Cold Start) | 4.5s | 2.8s | **38% faster** |
| Subsequent PDFs | 3.8-4.1s | 0.8-1.2s | **70-80% faster** |
| Memory per Conversion | 280-420MB | 80-120MB | **65-70% less memory** |
| Disk Space (browsers) | 400MB+ | 0 | **Eliminate browser downloads** |
| Setup Commands | `playwright install` | None | **Zero setup** |

---

## Additional Resources

- **IronPDF Documentation:** https://ironpdf.com/docs/
- **Tutorials and Examples:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference:** https://ironpdf.com/object-reference/api/
