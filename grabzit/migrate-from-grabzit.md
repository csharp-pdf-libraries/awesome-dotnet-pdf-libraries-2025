# How Do I Migrate from GrabzIt to IronPDF in C#?

## Table of Contents

1. [Why Migrate from GrabzIt to IronPDF](#why-migrate-from-grabzit-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Comparison](#performance-comparison)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from GrabzIt to IronPDF

### The GrabzIt Architecture Problem

GrabzIt is a cloud-based screenshot and PDF capture service. While convenient for quick integrations, it has fundamental architectural limitations:

1. **Image-Based PDFs**: GrabzIt creates screenshot-based PDFs where text is not selectable—essentially images wrapped in PDF format
2. **External Processing**: All content is sent to GrabzIt's servers for processing—privacy and compliance concerns for sensitive data
3. **Network Dependency**: Every PDF generation requires an HTTP call to external servers—latency, availability, and rate limiting issues
4. **Callback Complexity**: Asynchronous callback model requires webhook handling infrastructure
5. **Per-Capture Pricing**: Pay-per-use model can become expensive at scale
6. **No Text Search**: Since PDFs are image-based, text search and extraction don't work without OCR
7. **Larger File Sizes**: Image-based PDFs are significantly larger than vector-based PDFs
8. **No Offline Capability**: Cannot generate PDFs without internet connection

### What IronPDF Offers Instead

| Aspect | GrabzIt | IronPDF |
|--------|---------|---------|
| PDF Type | Image-based (screenshot) | True vector PDF |
| Text Selection | Not possible | Full text selection |
| Text Search | Requires OCR | Native searchable |
| Processing Location | External servers | Local/in-process |
| Privacy | Data sent externally | Data stays local |
| Latency | Network round-trip (500ms-5s) | Local processing (~100ms) |
| Pricing Model | Per-capture | Per-developer license |
| Offline Capability | No | Yes |
| File Size | Large (image data) | Small (vector data) |
| Callback Required | Yes (async) | No (sync/async) |
| CSS/JS Support | Limited | Full Chromium engine |

---

## Before You Start

### Prerequisites

1. **.NET Version**: IronPDF supports .NET Framework 4.6.2+ and .NET Core 3.1+ / .NET 5+
2. **License Key**: Obtain from [IronPDF website](https://ironpdf.com/licensing/)
3. **Remove GrabzIt Infrastructure**: Plan to remove callback handlers and API key configuration

### Identify GrabzIt Usage

Find all GrabzIt API calls in your codebase:

```bash
# Find GrabzIt client usage
grep -r "GrabzItClient\|GrabzIt\." --include="*.cs" .

# Find callback handlers
grep -r "GrabzIt\|grabzit" --include="*.ashx" --include="*.aspx" --include="*.cs" .

# Find configuration
grep -r "APPLICATION_KEY\|APPLICATION_SECRET\|grabzit" --include="*.config" --include="*.json" .
```

### Dependency Audit

Check your current GrabzIt package:

```bash
# Check NuGet packages
grep -r "GrabzIt" --include="*.csproj" .
```

---

## Quick Start Migration

### Step 1: Install IronPDF

```bash
# Remove GrabzIt
dotnet remove package GrabzIt

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Code

**Before (GrabzIt):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

public class PdfService
{
    private readonly GrabzItClient _grabzIt;

    public PdfService()
    {
        _grabzIt = new GrabzItClient("APP_KEY", "APP_SECRET");
    }

    public void GeneratePdf(string html, string callbackUrl)
    {
        var options = new PDFOptions();
        options.MarginTop = 20;
        options.MarginBottom = 20;

        _grabzIt.HTMLToPDF(html, options);
        _grabzIt.Save(callbackUrl);  // Async - callback receives result
    }
}

// Callback handler (separate endpoint)
public class GrabzItHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string id = context.Request.QueryString["id"];
        GrabzItClient grabzIt = new GrabzItClient("APP_KEY", "APP_SECRET");
        GrabzItFile file = grabzIt.GetResult(id);
        file.Save("output.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.MarginTop = 20;
        _renderer.RenderingOptions.MarginBottom = 20;
    }

    public byte[] GeneratePdf(string html)
    {
        // Synchronous - no callback needed!
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }

    public void GeneratePdfToFile(string html, string outputPath)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs(outputPath);
    }
}

// No callback handler needed - delete GrabzItHandler!
```

### Step 3: Remove Infrastructure

- Delete callback handler files (`.ashx`, handler endpoints)
- Remove GrabzIt API keys from configuration
- Remove webhook URL configuration
- Delete any GrabzIt status checking code

---

## Complete API Reference

### GrabzItClient to IronPDF Mapping

| GrabzIt Method | IronPDF Equivalent | Notes |
|---------------|-------------------|-------|
| `new GrabzItClient(key, secret)` | `new ChromePdfRenderer()` | No authentication needed |
| `HTMLToPDF(html)` | `renderer.RenderHtmlAsPdf(html)` | Returns PDF directly |
| `HTMLToPDF(html, options)` | Configure `RenderingOptions` first | Set options before render |
| `URLToPDF(url)` | `renderer.RenderUrlAsPdf(url)` | Returns PDF directly |
| `URLToPDF(url, options)` | Configure `RenderingOptions` first | Set options before render |
| `FileToPDF(path)` | `renderer.RenderHtmlFileAsPdf(path)` | Local HTML file |
| `HTMLToImage(html)` | `pdf.ToBitmap()` | Render then convert |
| `URLToImage(url)` | `pdf.ToBitmap()` | Render then convert |
| `Save(callbackUrl)` | `pdf.SaveAs(path)` or `pdf.BinaryData` | Immediate result |
| `SaveTo(filePath)` | `pdf.SaveAs(filePath)` | Same functionality |
| `GetResult(id)` | N/A | No callbacks needed |
| `GetStatus(id)` | N/A | Synchronous operation |

### PDFOptions to RenderingOptions Mapping

| GrabzIt PDFOptions | IronPDF Property | Notes |
|-------------------|------------------|-------|
| `MarginTop` | `RenderingOptions.MarginTop` | Same unit (mm) |
| `MarginBottom` | `RenderingOptions.MarginBottom` | Same unit (mm) |
| `MarginLeft` | `RenderingOptions.MarginLeft` | Same unit (mm) |
| `MarginRight` | `RenderingOptions.MarginRight` | Same unit (mm) |
| `PageSize` (A4, Letter, etc.) | `RenderingOptions.PaperSize` | Use `PdfPaperSize` enum |
| `Orientation` | `RenderingOptions.PaperOrientation` | `Portrait` or `Landscape` |
| `BrowserWidth` | `RenderingOptions.ViewPortWidth` | Viewport width in pixels |
| `BrowserHeight` | `RenderingOptions.ViewPortHeight` | Viewport height in pixels |
| `CSSMediaType` | `RenderingOptions.CssMediaType` | `Screen` or `Print` |
| `Delay` | `RenderingOptions.RenderDelay` | In milliseconds |
| `ClickElement` | Use JavaScript instead | `WaitFor.JavaScript()` |
| `HideElement` | Use CSS/JavaScript | Inject CSS to hide |
| `TemplateId` | `RenderingOptions.HtmlHeader/Footer` | Use HTML templates |
| `CustomWaterMark` | `pdf.ApplyWatermark()` | After rendering |
| `Password` | `pdf.SecuritySettings.UserPassword` | After rendering |
| `IncludeBackground` | `RenderingOptions.PrintHtmlBackgrounds` | Boolean |
| `IncludeLinks` | Always included | IronPDF preserves links |
| `CoverURL` | Render separately and merge | `PdfDocument.Merge()` |
| `TargetElement` | Use JavaScript | Target specific element |

### ImageOptions to IronPDF Mapping

| GrabzIt ImageOptions | IronPDF Equivalent | Notes |
|---------------------|-------------------|-------|
| `Format` (png, jpg) | `bitmap.Save(path, ImageFormat.Png)` | After `ToBitmap()` |
| `Width` | `RenderingOptions.ViewPortWidth` | Or resize bitmap |
| `Height` | `RenderingOptions.ViewPortHeight` | Or resize bitmap |
| `Quality` | `EncoderParameters` | When saving JPEG |
| `HD` | Higher DPI in `ToBitmap()` | Use DPI parameter |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (GrabzIt with Callback):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

public class GrabzItService
{
    private readonly GrabzItClient _client;

    public GrabzItService()
    {
        _client = new GrabzItClient("APP_KEY", "APP_SECRET");
    }

    public void CreatePdf(string html)
    {
        _client.HTMLToPDF(html);
        _client.Save("https://myserver.com/grabzit-callback");
        // Result arrives later via callback...
    }
}

// Callback handler - receives result asynchronously
public class GrabzItCallback : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string captureId = context.Request.QueryString["id"];
        var client = new GrabzItClient("APP_KEY", "APP_SECRET");
        var result = client.GetResult(captureId);
        result.Save(Server.MapPath("~/pdfs/" + captureId + ".pdf"));
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    public byte[] CreatePdf(string html)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;  // Immediate result!
    }

    public void CreatePdfToFile(string html, string path)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs(path);  // Done!
    }
}

// No callback handler needed - delete it!
```

### Example 2: URL to PDF with Options

**Before (GrabzIt):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

public void CaptureWebPage(string url)
{
    var client = new GrabzItClient("APP_KEY", "APP_SECRET");

    var options = new PDFOptions();
    options.PageSize = PageSize.A4;
    options.Orientation = PageOrientation.Landscape;
    options.MarginTop = 25;
    options.MarginBottom = 25;
    options.BrowserWidth = 1280;
    options.Delay = 3000;  // Wait 3 seconds
    options.IncludeBackground = true;

    client.URLToPDF(url, options);
    client.SaveTo("webpage.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CaptureWebPage(string url)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
    renderer.RenderingOptions.MarginTop = 25;
    renderer.RenderingOptions.MarginBottom = 25;
    renderer.RenderingOptions.ViewPortWidth = 1280;
    renderer.RenderingOptions.RenderDelay = 3000;  // Wait 3 seconds
    renderer.RenderingOptions.PrintHtmlBackgrounds = true;

    var pdf = renderer.RenderUrlAsPdf(url);
    pdf.SaveAs("webpage.pdf");
}
```

### Example 3: Watermarking PDFs

**Before (GrabzIt):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

public void CreateWatermarkedPdf(string html)
{
    var client = new GrabzItClient("APP_KEY", "APP_SECRET");

    var options = new PDFOptions();
    options.SetCustomWaterMark("watermark123", // pre-created watermark ID
        HorizontalPosition.Center,
        VerticalPosition.Middle);

    client.HTMLToPDF(html, options);
    client.SaveTo("watermarked.pdf");
}

// Note: Watermarks must be pre-created in GrabzIt dashboard
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreateWatermarkedPdf(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // HTML-based watermark - fully customizable!
    pdf.ApplyWatermark(
        "<div style='color:red; font-size:48px; font-weight:bold; " +
        "transform:rotate(-45deg); opacity:0.3;'>CONFIDENTIAL</div>",
        opacity: 30,
        VerticalAlignment.Middle,
        HorizontalAlignment.Center);

    pdf.SaveAs("watermarked.pdf");
}

// Or use image watermark
public void CreateImageWatermark(string html, string watermarkPath)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    pdf.ApplyWatermark(
        $"<img src='{watermarkPath}' style='width:200px;opacity:0.5;'>",
        opacity: 50,
        VerticalAlignment.Middle,
        HorizontalAlignment.Center);

    pdf.SaveAs("watermarked.pdf");
}
```

### Example 4: Password Protection

**Before (GrabzIt):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

public void CreateProtectedPdf(string html)
{
    var client = new GrabzItClient("APP_KEY", "APP_SECRET");

    var options = new PDFOptions();
    options.Password = "secretpassword";

    client.HTMLToPDF(html, options);
    client.SaveTo("protected.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreateProtectedPdf(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // More granular control over permissions
    pdf.SecuritySettings.UserPassword = "secretpassword";
    pdf.SecuritySettings.OwnerPassword = "ownerpassword";
    pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
    pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;

    pdf.SaveAs("protected.pdf");
}
```

### Example 5: HTML to Image

**Before (GrabzIt):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

public void HtmlToImage(string html)
{
    var client = new GrabzItClient("APP_KEY", "APP_SECRET");

    var options = new ImageOptions();
    options.Format = ImageFormat.png;
    options.Width = 800;
    options.Height = 600;
    options.HD = true;

    client.HTMLToImage(html, options);
    client.SaveTo("screenshot.png");
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Drawing;
using System.Drawing.Imaging;

public void HtmlToImage(string html)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.ViewPortWidth = 800;

    var pdf = renderer.RenderHtmlAsPdf(html);

    // Convert to images at 300 DPI for high quality
    var images = pdf.ToBitmap(300);

    // Save first page as PNG
    images[0].Save("screenshot.png", ImageFormat.Png);

    // Or save all pages
    for (int i = 0; i < images.Length; i++)
    {
        images[i].Save($"page_{i + 1}.png", ImageFormat.Png);
    }
}
```

### Example 6: Click Element Before Capture

**Before (GrabzIt):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

public void CaptureAfterClick(string url)
{
    var client = new GrabzItClient("APP_KEY", "APP_SECRET");

    var options = new PDFOptions();
    options.ClickElement = "#loadMoreButton";  // CSS selector
    options.Delay = 2000;  // Wait after click

    client.URLToPDF(url, options);
    client.SaveTo("clicked.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CaptureAfterClick(string url)
{
    var renderer = new ChromePdfRenderer();

    // Use JavaScript to click and wait
    renderer.RenderingOptions.Javascript = @"
        document.querySelector('#loadMoreButton').click();
    ";
    renderer.RenderingOptions.RenderDelay = 2000;  // Wait after click

    var pdf = renderer.RenderUrlAsPdf(url);
    pdf.SaveAs("clicked.pdf");
}

// Or use WaitFor for more control
public void CaptureAfterDynamicContent(string url)
{
    var renderer = new ChromePdfRenderer();

    // Wait for specific condition after clicking
    renderer.RenderingOptions.WaitFor.JavaScript("window.dataLoaded === true");

    var pdf = renderer.RenderUrlAsPdf(url);
    pdf.SaveAs("dynamic.pdf");
}
```

### Example 7: Hide Elements

**Before (GrabzIt):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

public void CaptureWithHiddenElements(string url)
{
    var client = new GrabzItClient("APP_KEY", "APP_SECRET");

    var options = new PDFOptions();
    options.HideElement = ".ads, .navigation, #cookie-banner";

    client.URLToPDF(url, options);
    client.SaveTo("clean.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CaptureWithHiddenElements(string url)
{
    var renderer = new ChromePdfRenderer();

    // Inject CSS to hide elements
    renderer.RenderingOptions.CustomCssUrl = null;  // Or use external CSS
    renderer.RenderingOptions.Javascript = @"
        var style = document.createElement('style');
        style.textContent = '.ads, .navigation, #cookie-banner { display: none !important; }';
        document.head.appendChild(style);
    ";

    var pdf = renderer.RenderUrlAsPdf(url);
    pdf.SaveAs("clean.pdf");
}

// Or use HTML directly with embedded styles
public void CaptureHtmlWithHiddenElements(string html)
{
    var renderer = new ChromePdfRenderer();

    // Add CSS to hide elements
    string cleanHtml = @"
        <style>.ads, .navigation, #cookie-banner { display: none !important; }</style>
    " + html;

    var pdf = renderer.RenderHtmlAsPdf(cleanHtml);
    pdf.SaveAs("clean.pdf");
}
```

### Example 8: Cover Page

**Before (GrabzIt):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

public void CreatePdfWithCover(string html)
{
    var client = new GrabzItClient("APP_KEY", "APP_SECRET");

    var options = new PDFOptions();
    options.CoverURL = "https://mysite.com/cover-page.html";

    client.HTMLToPDF(html, options);
    client.SaveTo("with-cover.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreatePdfWithCover(string html)
{
    var renderer = new ChromePdfRenderer();

    // Render cover page
    var coverPdf = renderer.RenderUrlAsPdf("https://mysite.com/cover-page.html");

    // Render main content
    var contentPdf = renderer.RenderHtmlAsPdf(html);

    // Merge cover + content
    var finalPdf = PdfDocument.Merge(coverPdf, contentPdf);
    finalPdf.SaveAs("with-cover.pdf");

    // Cleanup
    coverPdf.Dispose();
    contentPdf.Dispose();
}
```

### Example 9: Headers and Footers

**Before (GrabzIt):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

public void CreatePdfWithHeaderFooter(string html)
{
    var client = new GrabzItClient("APP_KEY", "APP_SECRET");

    var options = new PDFOptions();
    options.TemplateId = "my-template-id";  // Pre-configured in dashboard

    client.HTMLToPDF(html, options);
    client.SaveTo("templated.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreatePdfWithHeaderFooter(string html)
{
    var renderer = new ChromePdfRenderer();

    // Full HTML header - no pre-configuration needed!
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
    {
        HtmlFragment = @"
            <div style='width:100%; text-align:center; font-size:12px;'>
                <img src='https://mysite.com/logo.png' style='height:30px;'>
                <span style='margin-left:20px;'>Company Name - Confidential</span>
            </div>",
        DrawDividerLine = true
    };

    // Footer with page numbers
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
    {
        HtmlFragment = @"
            <div style='width:100%; font-size:10px;'>
                <span style='float:left;'>Generated: {date}</span>
                <span style='float:right;'>Page {page} of {total-pages}</span>
            </div>",
        DrawDividerLine = true
    };

    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("templated.pdf");
}
```

### Example 10: Batch Processing

**Before (GrabzIt):**
```csharp
using GrabzIt;
using System.Collections.Generic;

public class GrabzItBatchService
{
    private readonly GrabzItClient _client;
    private readonly Dictionary<string, string> _pendingCaptures = new();

    public GrabzItBatchService()
    {
        _client = new GrabzItClient("APP_KEY", "APP_SECRET");
    }

    public void QueuePdfs(List<string> htmlDocuments, string callbackUrl)
    {
        foreach (var html in htmlDocuments)
        {
            _client.HTMLToPDF(html);
            string captureId = _client.Save(callbackUrl);
            _pendingCaptures[captureId] = html;
        }
        // Wait for callbacks to arrive...
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class PdfBatchService
{
    public Dictionary<string, byte[]> GeneratePdfs(List<string> htmlDocuments)
    {
        var results = new ConcurrentDictionary<string, byte[]>();

        // Process in parallel - all local, immediate results
        Parallel.ForEach(htmlDocuments, (html, state, index) =>
        {
            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf(html);
            results[$"document_{index}"] = pdf.BinaryData;
            pdf.Dispose();
        });

        return new Dictionary<string, byte[]>(results);
    }

    // Async version
    public async Task<List<byte[]>> GeneratePdfsAsync(List<string> htmlDocuments)
    {
        var tasks = htmlDocuments.Select(html => Task.Run(() =>
        {
            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf(html);
            var bytes = pdf.BinaryData;
            pdf.Dispose();
            return bytes;
        }));

        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }
}
```

---

## Advanced Scenarios

### Removing Callback Infrastructure

**Before (ASP.NET with GrabzIt Callback Handler):**
```csharp
// GrabzItHandler.ashx.cs
public class GrabzItHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        // Validate callback (security)
        string signature = context.Request.QueryString["sig"];
        if (!ValidateSignature(signature))
        {
            context.Response.StatusCode = 403;
            return;
        }

        string captureId = context.Request.QueryString["id"];
        string customId = context.Request.QueryString["customid"];

        try
        {
            var client = new GrabzItClient("APP_KEY", "APP_SECRET");
            var result = client.GetResult(captureId);

            if (result != null)
            {
                string filePath = Server.MapPath($"~/pdfs/{customId}.pdf");
                result.Save(filePath);

                // Update database, notify user, etc.
                UpdatePdfStatus(customId, "completed", filePath);
            }
        }
        catch (Exception ex)
        {
            UpdatePdfStatus(customId, "failed", ex.Message);
        }
    }
}

// Web.config route
<system.web>
    <httpHandlers>
        <add verb="*" path="grabzit-callback.ashx" type="GrabzItHandler"/>
    </httpHandlers>
</system.web>
```

**After (IronPDF - No Handler Needed):**
```csharp
// PdfService.cs - Direct, synchronous generation
public class PdfService
{
    public async Task<PdfResult> GeneratePdfAsync(string html, string documentId)
    {
        try
        {
            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf(html);

            string filePath = Path.Combine(_pdfStoragePath, $"{documentId}.pdf");
            pdf.SaveAs(filePath);

            return new PdfResult
            {
                DocumentId = documentId,
                Status = "completed",
                FilePath = filePath,
                FileSize = new FileInfo(filePath).Length
            };
        }
        catch (Exception ex)
        {
            return new PdfResult
            {
                DocumentId = documentId,
                Status = "failed",
                Error = ex.Message
            };
        }
    }
}

// DELETE: GrabzItHandler.ashx
// DELETE: Web.config httpHandler entry
// DELETE: Callback URL configuration
```

### Migrating from Async Callback Pattern

**Before (GrabzIt Async Pattern):**
```csharp
public class GrabzItPdfController : ApiController
{
    [HttpPost]
    public IHttpActionResult RequestPdf([FromBody] PdfRequest request)
    {
        var client = new GrabzItClient("APP_KEY", "APP_SECRET");

        var options = new PDFOptions();
        options.CustomId = request.DocumentId;  // Track this request

        client.HTMLToPDF(request.Html, options);
        string captureId = client.Save(CallbackUrl);

        // Return immediately - PDF not ready yet
        return Ok(new { CaptureId = captureId, Status = "processing" });
    }

    [HttpGet]
    public IHttpActionResult CheckStatus(string captureId)
    {
        var client = new GrabzItClient("APP_KEY", "APP_SECRET");
        var status = client.GetStatus(captureId);

        return Ok(new { CaptureId = captureId, Status = status.Processing ? "processing" : "complete" });
    }
}
```

**After (IronPDF Synchronous Pattern):**
```csharp
public class PdfController : ApiController
{
    [HttpPost]
    public IHttpActionResult GeneratePdf([FromBody] PdfRequest request)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(request.Html);

        // Return PDF directly - no callback, no polling!
        return new FileContentResult(pdf.BinaryData, "application/pdf")
        {
            FileDownloadName = $"{request.DocumentId}.pdf"
        };
    }

    // Or save and return URL
    [HttpPost]
    public IHttpActionResult GeneratePdfAndSave([FromBody] PdfRequest request)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(request.Html);

        string filePath = Path.Combine(_storagePath, $"{request.DocumentId}.pdf");
        pdf.SaveAs(filePath);

        return Ok(new {
            DocumentId = request.DocumentId,
            Status = "complete",  // Always complete!
            DownloadUrl = $"/pdfs/{request.DocumentId}.pdf"
        });
    }
}
```

### Service Registration Changes

**Before (with GrabzIt):**
```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IGrabzItClient>(sp =>
        new GrabzItClient(
            Configuration["GrabzIt:AppKey"],
            Configuration["GrabzIt:AppSecret"]));

    services.AddHttpClient<IGrabzItCallbackService>();
}

// appsettings.json
{
    "GrabzIt": {
        "AppKey": "your-app-key",
        "AppSecret": "your-app-secret",
        "CallbackUrl": "https://yourserver.com/grabzit-callback"
    }
}
```

**After (with IronPDF):**
```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Set license once at startup
    IronPdf.License.LicenseKey = Configuration["IronPdf:LicenseKey"];

    // Register as singleton (thread-safe)
    services.AddSingleton<IPdfService, IronPdfService>();
}

// appsettings.json - much simpler!
{
    "IronPdf": {
        "LicenseKey": "your-license-key"
    }
}
```

### Text Extraction - Now Possible!

**Before (GrabzIt):**
```csharp
// NOT POSSIBLE with GrabzIt - PDFs are image-based
// Would need OCR to extract text:

// var ocrClient = new TesseractEngine("tessdata", "eng");
// var result = ocrClient.Process(pdfImage);
// string text = result.GetText();  // Inaccurate, slow
```

**After (IronPDF):**
```csharp
using IronPdf;

public string ExtractTextFromPdf(string pdfPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    // Full text extraction - works because text is real, not image!
    string allText = pdf.ExtractAllText();

    // Or extract per page
    for (int i = 0; i < pdf.PageCount; i++)
    {
        string pageText = pdf.ExtractTextFromPage(i);
        Console.WriteLine($"Page {i + 1}: {pageText}");
    }

    return allText;
}

// Search within PDF
public bool PdfContainsText(string pdfPath, string searchTerm)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    return pdf.ExtractAllText().Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
}
```

---

## Performance Comparison

### Latency Comparison

| Operation | GrabzIt | IronPDF | Notes |
|-----------|---------|---------|-------|
| Simple HTML | 2-5 seconds | 100-500ms | Network vs local |
| Complex HTML | 5-15 seconds | 500ms-2s | Rendering overhead |
| URL Capture | 3-10 seconds | 1-5 seconds | Both need to fetch |
| Batch (10 PDFs) | 20-60 seconds | 2-10 seconds | Parallel processing |
| With Callback | +500ms-2s | N/A | No callback needed |

### Reliability Comparison

| Aspect | GrabzIt | IronPDF |
|--------|---------|---------|
| Network Dependency | Required | None |
| Service Availability | 99.9% SLA | 100% (local) |
| Rate Limiting | Yes (per plan) | None |
| Timeout Issues | Common | Rare |
| Retry Logic Needed | Yes | Rarely |

### File Size Comparison

| Content | GrabzIt (Image PDF) | IronPDF (Vector PDF) |
|---------|--------------------|--------------------|
| Simple page | 500KB-2MB | 50-200KB |
| Text-heavy | 1-5MB | 100-500KB |
| With images | 2-10MB | 500KB-2MB |

---

## Troubleshooting

### Issue 1: "My PDFs used to be screenshots, now they look different"

**Cause**: GrabzIt creates image-based PDFs; IronPDF creates true vector PDFs

**Solution**: This is actually an improvement! Text is now selectable and searchable. If you need screenshot-style output:

```csharp
// Convert PDF pages to images, then back to PDF
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
var images = pdf.ToBitmap(150); // 150 DPI

// Create image-based PDF (not recommended)
var imagePdf = new PdfDocument();
foreach (var img in images)
{
    // ... add images as pages
}
```

### Issue 2: "No callback is being called"

**Cause**: IronPDF doesn't use callbacks—operations are synchronous

**Solution**: Remove callback handlers and get results directly:

```csharp
// Instead of waiting for callback:
var pdf = renderer.RenderHtmlAsPdf(html);
var bytes = pdf.BinaryData;  // Available immediately!
```

### Issue 3: "API keys not working"

**Cause**: IronPDF uses a different licensing model

**Solution**:
```csharp
// Remove GrabzIt keys
// var client = new GrabzItClient("APP_KEY", "APP_SECRET");  // DELETE

// Add IronPDF license
IronPdf.License.LicenseKey = "YOUR-IRONPDF-LICENSE-KEY";
var renderer = new ChromePdfRenderer();
```

### Issue 4: "TemplateId not found"

**Cause**: GrabzIt templates don't exist in IronPDF

**Solution**: Use HTML headers/footers instead:
```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div>Your header content</div>"
};
```

### Issue 5: "Delay parameter not working"

**Cause**: Different parameter name

**Solution**:
```csharp
// GrabzIt: options.Delay = 3000;
// IronPDF:
renderer.RenderingOptions.RenderDelay = 3000;  // milliseconds
```

### Issue 6: "Watermarks look different"

**Cause**: GrabzIt uses pre-configured watermarks; IronPDF uses HTML

**Solution**:
```csharp
pdf.ApplyWatermark(
    "<div style='font-size:48px; color:red; transform:rotate(-45deg);'>DRAFT</div>",
    opacity: 30);
```

### Issue 7: "File sizes are smaller"

**Cause**: This is expected—vector PDFs are more efficient than image-based

**Solution**: No action needed—smaller files are better!

### Issue 8: "Text can now be selected"

**Cause**: This is a feature, not a bug—IronPDF creates real text

**Solution**: If you need to prevent text selection for security:
```csharp
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all GrabzIt API calls in codebase**
  ```bash
  grep -r "using GrabzIt" --include="*.cs" .
  grep -r "GrabzItClient\|HTMLToPDF\|URLToPDF" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Identify callback handlers and webhook endpoints**
  ```bash
  grep -r "callback" --include="*.ashx" .
  grep -r "webhook" --include="*.config" .
  ```
  **Why:** Determine all asynchronous processing points that need to be refactored.

- [ ] **Document current GrabzIt options and templates**
  ```csharp
  // Find patterns like:
  var options = new GrabzItPDFOptions {
      PageSize = "A4",
      Orientation = "Landscape"
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Test IronPDF in development environment**
  **Why:** Verify IronPDF works in your development setup before full migration.

- [ ] **Plan callback handler decommissioning**
  **Why:** IronPDF does not require asynchronous callbacks, simplifying infrastructure.

### Code Migration

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project to start using its features.

- [ ] **Remove GrabzIt NuGet package**
  ```bash
  dotnet remove package GrabzIt
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Replace `GrabzItClient` with `ChromePdfRenderer`**
  ```csharp
  // Before (GrabzIt)
  var client = new GrabzItClient("API_KEY");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses ChromePdfRenderer for PDF generation, leveraging Chromium for rendering.

- [ ] **Convert `HTMLToPDF()` to `RenderHtmlAsPdf()`**
  ```csharp
  // Before (GrabzIt)
  client.HTMLToPDF(html, options);

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF provides direct HTML to PDF rendering with modern CSS/JS support.

- [ ] **Convert `URLToPDF()` to `RenderUrlAsPdf()`**
  ```csharp
  // Before (GrabzIt)
  client.URLToPDF(url, options);

  // After (IronPDF)
  var pdf = renderer.RenderUrlAsPdf(url);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF allows URL rendering with full JavaScript execution and CSS support.

- [ ] **Replace `Save(callback)` with `SaveAs(path)` or `BinaryData`**
  ```csharp
  // Before (GrabzIt)
  client.Save(callback);

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  // or
  var binaryData = pdf.BinaryData;
  ```
  **Why:** IronPDF provides synchronous saving methods, simplifying the codebase.

- [ ] **Update options from `PDFOptions` to `RenderingOptions`**
  ```csharp
  // Before (GrabzIt)
  options.PageSize = "A4";
  options.Orientation = "Landscape";

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  ```
  **Why:** IronPDF's RenderingOptions provide comprehensive configuration for PDF output.

- [ ] **Convert templates to HTML headers/footers**
  ```csharp
  // Before (GrabzIt)
  options.Header = "Page [page] of [total]";
  options.Footer = "Document Title";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>{html-title}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders for dynamic content.

- [ ] **Update watermark code**
  ```csharp
  // Before (GrabzIt)
  options.Watermark = "Confidential";

  // After (IronPDF)
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>Confidential</h1>");
  ```
  **Why:** IronPDF allows HTML-based watermarks for more flexible styling.

### Infrastructure Migration

- [ ] **Delete callback handler files (`.ashx`, etc.)**
  **Why:** IronPDF does not require asynchronous callbacks, simplifying infrastructure.

- [ ] **Remove GrabzIt API keys from configuration**
  **Why:** Clean up configuration by removing unused API keys.

- [ ] **Remove webhook URL configuration**
  **Why:** IronPDF does not require webhooks, reducing configuration complexity.

- [ ] **Add IronPDF license key to configuration**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Remove polling/status checking code**
  **Why:** IronPDF provides synchronous operations, eliminating the need for status polling.

### Testing

- [ ] **Test HTML to PDF conversion**
  **Why:** Verify that HTML content is correctly converted to PDF using IronPDF.

- [ ] **Test URL to PDF conversion**
  **Why:** Ensure URLs are rendered accurately with full CSS/JS support.

- [ ] **Verify text is selectable in output PDFs**
  **Why:** IronPDF generates vector-based PDFs with selectable text, unlike image-based PDFs.

- [ ] **Test text extraction works**
  **Why:** Ensure that text can be extracted from PDFs for search and indexing.

- [ ] **Verify file sizes are smaller**
  **Why:** IronPDF's vector-based PDFs are typically smaller than image-based PDFs.

- [ ] **Test header/footer rendering**
  **Why:** Ensure headers and footers are rendered correctly with dynamic content.

- [ ] **Test watermarks**
  **Why:** Verify that watermarks are applied correctly with desired styling.

- [ ] **Test password protection**
  ```csharp
  // Example
  pdf.SecuritySettings.UserPassword = "secret";
  ```
  **Why:** Ensure PDFs are secured with passwords if required.

- [ ] **Performance test without network latency**
  **Why:** IronPDF processes PDFs locally, reducing latency compared to cloud-based solutions.

### Post-Migration

- [ ] **Cancel GrabzIt subscription**
  **Why:** Avoid unnecessary costs by canceling unused services.

- [ ] **Archive callback handler code**
  **Why:** Preserve old code for reference or rollback if needed.

- [ ] **Update documentation**
  **Why:** Ensure all documentation reflects the new PDF generation process.

- [ ] **Monitor for any GrabzIt-related errors**
  **Why:** Ensure all GrabzIt dependencies are fully removed and no errors persist.

- [ ] **Enjoy faster PDF generation!**
  **Why:** Benefit from IronPDF's efficient local processing and rich feature set.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [Text Extraction Guide](https://ironpdf.com/how-to/extract-text-from-pdf/)

---

## Summary

Migrating from GrabzIt to IronPDF provides several significant improvements:

1. **True PDFs**: Vector-based PDFs with selectable, searchable text
2. **Local Processing**: No external server dependency—faster, more private
3. **No Callbacks**: Synchronous operations—immediate results
4. **Smaller Files**: Vector PDFs are 5-10x smaller than image-based
5. **Text Extraction**: Search and extract text without OCR
6. **Simpler Architecture**: No callback handlers, webhooks, or polling
7. **Predictable Costs**: Per-developer licensing instead of per-capture

The key paradigm shift is from asynchronous callback-based processing to synchronous in-process generation. This simplifies your code significantly by eliminating callback handlers, status polling, and external service dependencies.
