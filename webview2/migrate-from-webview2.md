# Migration Guide: WebView2 (Microsoft Edge) → IronPDF

## Why Migrate from WebView2 to IronPDF

WebView2 is fundamentally limited to Windows environments and requires UI thread contexts, making it unsuitable for cross-platform server deployments, Docker containers, or headless Linux systems. IronPDF provides true cross-platform PDF generation (Windows, Linux, macOS, Docker) without UI dependencies, making it ideal for web servers, APIs, and cloud environments. Additionally, IronPDF offers better stability for long-running processes and eliminates the memory leak issues commonly reported with WebView2 in production scenarios.

## NuGet Package Changes

**Remove:**
```xml
<PackageReference Include="Microsoft.Web.WebView2" Version="1.x.x" />
```

**Add:**
```xml
<PackageReference Include="IronPdf" Version="2024.x.x" />
```

## Namespace Mapping

| WebView2 | IronPDF |
|----------|---------|
| `Microsoft.Web.WebView2.Core` | `IronPdf` |
| `Microsoft.Web.WebView2.WinForms` | `IronPdf` (no UI dependency) |
| `Microsoft.Web.WebView2.Wpf` | `IronPdf` (no UI dependency) |

## API Mapping Table

| WebView2 API | IronPDF API | Notes |
|--------------|-------------|-------|
| `CoreWebView2.PrintToPdfAsync()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Direct HTML/URL to PDF |
| `CoreWebView2.NavigateToString()` | `ChromePdfRenderer.RenderHtmlAsPdf(html)` | Pass HTML directly |
| `CoreWebView2.Navigate(url)` | `ChromePdfRenderer.RenderUrlAsPdf(url)` | URL rendering |
| `CoreWebView2.ExecuteScriptAsync()` | `RenderingOptions.JavaScript` | Enable/configure JS execution |
| `CoreWebView2Environment.CreateAsync()` | `new ChromePdfRenderer()` | No environment setup needed |
| `WebView2.EnsureCoreWebView2Async()` | N/A | No initialization required |
| `PrintSettings` | `PdfPrintOptions` | Configure page size, margins, etc. |
| `CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync()` | `RenderingOptions.CustomCssUrl` / `.HtmlHeader` | Inject custom scripts/styles |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (WebView2):**
```csharp
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

// Requires WinForms context
var webView = new WebView2();
await webView.EnsureCoreWebView2Async();

string html = "<h1>Hello World</h1><p>This is a test document.</p>";
webView.CoreWebView2.NavigateToString(html);

// Wait for navigation to complete
webView.CoreWebView2.NavigationCompleted += async (sender, args) =>
{
    await webView.CoreWebView2.PrintToPdfAsync("output.pdf");
};
```

**After (IronPDF):**
```csharp
using IronPdf;

// Works in console apps, web servers, anywhere
var renderer = new ChromePdfRenderer();
string html = "<h1>Hello World</h1><p>This is a test document.</p>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Custom Settings

**Before (WebView2):**
```csharp
using Microsoft.Web.WebView2.Core;

var webView = new WebView2();
await webView.EnsureCoreWebView2Async();

webView.CoreWebView2.Navigate("https://example.com");

webView.CoreWebView2.NavigationCompleted += async (sender, args) =>
{
    var printSettings = webView.CoreWebView2.Environment.CreatePrintSettings();
    printSettings.PageWidth = 8.27; // A4 width in inches
    printSettings.PageHeight = 11.69; // A4 height
    printSettings.MarginTop = 0.4;
    printSettings.MarginBottom = 0.4;
    
    await webView.CoreWebView2.PrintToPdfAsync("webpage.pdf", printSettings);
};
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 10; // millimeters
renderer.RenderingOptions.MarginBottom = 10;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: HTML with JavaScript Execution

**Before (WebView2):**
```csharp
using Microsoft.Web.WebView2.Core;

var webView = new WebView2();
await webView.EnsureCoreWebView2Async();

// Add script before navigation
await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
    "document.addEventListener('DOMContentLoaded', () => { document.body.style.backgroundColor = 'lightblue'; });"
);

string html = "<h1>Dynamic Content</h1><script>document.write('<p>Generated: ' + new Date() + '</p>');</script>";
webView.CoreWebView2.NavigateToString(html);

webView.CoreWebView2.NavigationCompleted += async (sender, args) =>
{
    // Wait for JS to execute
    await Task.Delay(1000);
    await webView.CoreWebView2.PrintToPdfAsync("dynamic.pdf");
};
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.RenderDelay = 1000; // Wait for JS execution (ms)

string html = "<h1>Dynamic Content</h1><script>document.write('<p>Generated: ' + new Date() + '</p>');</script>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("dynamic.pdf");
```

## Common Gotchas

### 1. **No UI Thread Required**
IronPDF doesn't require a UI context or message pump. Remove all `Application.Run()`, `STA` thread attributes, or WinForms/WPF initialization code.

### 2. **Synchronous Methods Available**
Unlike WebView2's async-only approach, IronPDF supports both synchronous and asynchronous operations. Choose based on your application architecture.

### 3. **License Required for Production**
IronPDF requires a license key for production use. Set it at application startup:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 4. **Docker/Linux Deployment**
IronPDF works seamlessly in Docker containers but requires specific dependencies. Use the official Docker images or install required packages:
```dockerfile
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev
```

### 5. **Different Measurement Units**
WebView2 uses inches for margins and dimensions; IronPDF primarily uses millimeters. Convert accordingly or use the `PdfPaperSize` enum.

### 6. **Base URL for Relative Paths**
When rendering HTML with relative image/CSS paths, set the base URL:
```csharp
renderer.RenderingOptions.BaseUrl = new Uri("https://yourdomain.com");
```

### 7. **JavaScript Timing**
Use `RenderDelay` instead of manual `Task.Delay()` calls to ensure JavaScript completes before PDF generation.

## Additional Resources

- **Documentation:** https://ironpdf.com/docs/
- **Tutorials:** https://ironpdf.com/tutorials/
- **API Reference:** Complete API documentation available in the docs
- **Cross-Platform Guide:** Specific instructions for Linux, macOS, and Docker deployments