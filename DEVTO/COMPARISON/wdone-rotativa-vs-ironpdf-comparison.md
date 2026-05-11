---
title: "Rotativa vs IronPDF: what the docs do not tell you"
published: false
tags: dotnet, csharp, pdf, comparison
---
# Rotativa vs IronPDF: what the docs do not tell you

You're building an ASP.NET Core MVC application. Product owners want "Export to PDF" buttons on reports. You search ".NET MVC PDF" and Rotativa appears with clean examples showing `return new ViewAsPdf()`. It looks simple—until you deploy to Linux and discover wkhtmltopdf binary incompatibilities, or move to Azure App Service and hit file permission errors, or try to generate PDFs outside controller actions and find the API doesn't support it. Rotativa is a convenient wrapper for ASP.NET MVC controllers, but it's architecturally constrained to web request contexts and carries the operational complexity of managing the wkhtmltopdf binary.

This comparison exists because Rotativa and IronPDF both generate PDFs from HTML in .NET, but they make fundamentally different architectural choices. Rotativa wraps wkhtmltopdf (an external Qt WebKit-based command-line tool) for MVC controller convenience. IronPDF embeds a Chromium rendering engine as a library component. For teams evaluating "ASP.NET PDF generation," this comparison provides decision checklists covering deployment requirements, architectural constraints, and feature completeness to determine which tool fits your stack.

**Canonical/source version:** This article is also available at [IronPDF's comparison guides](https://ironpdf.com/).

## Understanding IronPDF

IronPDF is a .NET library with an embedded Chromium rendering engine. It works in any .NET application context: MVC controllers, background services, console apps, Azure Functions, worker processes. You call `ChromePdfRenderer.RenderHtmlAsPdf()` from any code location, and IronPDF handles HTML parsing, CSS layout, JavaScript execution, and PDF generation in-process. No external binaries to manage, no command-line tool invocations, no temporary file handling. IronPDF runs on Windows, Linux, and macOS without platform-specific binary dependencies.

IronPDF supports .NET Core, .NET 5+, and .NET Framework 4.6.2+. Because it's a library, not a controller helper, it integrates with any architecture: microservices generating PDFs asynchronously, batch processors creating reports overnight, API endpoints streaming PDFs to clients, or background jobs rendering thousands of documents. See the [HTML to PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) for implementation patterns.

## Understanding Rotativa

Rotativa (and Rotativa.AspNetCore) is an ASP.NET MVC library that wraps wkhtmltopdf, a Qt WebKit-based command-line tool for HTML-to-PDF conversion. Rotativa provides convenience methods like `ViewAsPdf`, `UrlAsPdf`, and `ActionAsPdf` that return `ActionResult` objects from MVC controllers. The library handles invoking wkhtmltopdf as an external process, passing HTML content via temporary files, and streaming the resulting PDF back to the HTTP response.

Rotativa is designed specifically for MVC controllers responding to web requests. It simplifies the common pattern of "render Razor view → convert to PDF → return to browser." However, this convenience comes with constraints: tight coupling to MVC request contexts, dependency on wkhtmltopdf binaries in specific file system locations, and limited support for non-web scenarios (background jobs, API services, console apps).

## Key Limitations of Rotativa

### Product Status
Rotativa.AspNetCore is actively maintained (version 1.4.0 latest, updated 2024). The original Rotativa for .NET Framework is in maintenance mode with limited updates. The project has community adoption and works well for its designed use case (MVC controllers generating PDFs). However, it inherits wkhtmltopdf's limitations: wkhtmltopdf itself is in maintenance mode (last major release 2020) with minimal active development on its Qt WebKit rendering engine.

### Missing Capabilities
**MVC-only architecture:** Rotativa's API is built around `ActionResult` types and MVC controller contexts. Generating PDFs outside controller actions (background jobs, console apps, Azure Functions) requires workarounds or alternative approaches. You cannot easily use Rotativa in non-web architectures.

**No PDF manipulation features:** Rotativa generates PDFs but does not merge them, split them, extract text, add watermarks programmatically, apply digital signatures, fill forms, or encrypt with passwords. For post-generation operations, you need additional libraries.

**Limited rendering engine:** Rotativa uses wkhtmltopdf's Qt WebKit engine, which is based on WebKit from ~2015. It does not support modern CSS features like Grid or Flexbox consistently, has limited JavaScript ES6+ support, and may render differently from current Chrome. For cutting-edge web standards, wkhtmltopdf lags behind Chromium.

### Technical Issues
**Binary management complexity:** Rotativa requires wkhtmltopdf executables in a specific folder structure (typically `wwwroot/Rotativa` or configured path). These binaries are platform-specific (Windows .exe, Linux binaries with library dependencies). Managing binaries across development, staging, and production environments adds deployment friction.

**File system dependencies:** Rotativa writes temporary HTML files to disk, invokes wkhtmltopdf as an external process, and reads the resulting PDF from disk. In read-only file systems (some cloud environments, immutable containers), this causes failures. Azure App Service, AWS Lambda, and Kubernetes pods with restricted write permissions require careful configuration.

**External process overhead:** Every PDF generation spawns a wkhtmltopdf process. Process creation overhead (~50-100 ms on Linux, ~100-200 ms on Windows) adds latency. For high-throughput scenarios, this overhead compounds. Process cleanup must be reliable; leaked processes consume memory.

### Support Status
Rotativa.AspNetCore has community support via GitHub issues. Maintainer (Giorgio Bozio) is responsive for major bugs. However, there is no commercial support offering, no SLA for bug fixes, and no 24/7 engineering assistance. For production issues, you're self-service via GitHub discussions and Stack Overflow. Additionally, wkhtmltopdf (the underlying renderer) is in maintenance mode with no active feature development, meaning rendering bugs or missing CSS features are unlikely to be fixed.

### Architecture Problems
**Tight coupling to MVC request pipeline:** Rotativa's convenience methods (`ViewAsPdf`, `ActionAsPdf`) require `ControllerContext` and depend on MVC infrastructure. Background services, console apps, or microservices without MVC pipelines must manually construct contexts or find alternative approaches.

**No async-first design:** Rotativa's `BuildFile` method is synchronous. For async controllers or high-concurrency scenarios, this blocks threads during process invocation and file I/O, reducing scalability.

**Configuration via static methods:** Rotativa configuration (`RotativaConfiguration.Setup()`) uses static state, which is less testable and can cause issues in multi-tenant or per-request configuration scenarios.

## Feature Comparison Overview

| Category | Rotativa | IronPDF |
|----------|----------|---------|
| **Current Status** | Actively maintained (1.4.0 latest for AspNetCore) | Actively maintained, commercial with 30-day trial |
| **HTML Support** | Qt WebKit (circa 2015 standards) via wkhtmltopdf | Modern Chrome HTML5/CSS3/JS (embedded Chromium) |
| **Rendering Quality** | Good for pre-2015 CSS; limited Grid/Flexbox | Pixel-perfect Chrome rendering (modern CSS, JS) |
| **Installation** | NuGet + wkhtmltopdf binary (platform-specific) | NuGet with embedded engine (self-contained) |
| **Support** | Community via GitHub issues | 24/5 engineer support (24/7 premium), live chat |
| **Future Viability** | Tied to wkhtmltopdf (maintenance mode) | Commercial backing, enterprise SLAs |

---

## Architecture Decision Checklist

Use this checklist to determine which tool fits your application architecture:

### ✅ Consider Rotativa if you answer YES to ALL:
- [ ] Your application is ASP.NET Core MVC or .NET Framework MVC
- [ ] PDFs are only generated from HTTP controller actions
- [ ] You have file system write access in all deployment environments
- [ ] Your HTML/CSS uses pre-2015 web standards (no CSS Grid, minimal Flexbox)
- [ ] You can manage platform-specific binaries (wkhtmltopdf) in deployments
- [ ] You only need basic PDF generation (no merging, watermarking, signing, etc.)
- [ ] You're comfortable with community support and no commercial SLA

### ✅ Consider IronPDF if you answer YES to ANY:
- [ ] You generate PDFs outside MVC controllers (background jobs, console apps, APIs)
- [ ] You need modern CSS support (Grid, Flexbox, custom properties)
- [ ] Your deployment is read-only file systems or restricted environments
- [ ] You require PDF manipulation (merge, split, watermark, sign, encrypt)
- [ ] You need cross-platform deployments without binary management
- [ ] You want commercial support with SLAs
- [ ] Your application uses Docker, Kubernetes, or serverless functions (Azure Functions, AWS Lambda)
- [ ] You need async-first PDF generation with high concurrency

---

## Code Comparison: Basic MVC PDF Generation

### Rotativa — MVC Controller Action

Rotativa shines in its primary use case: returning PDFs from MVC controller actions. Here's a complete example:

```csharp
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using System;
using System.Collections.Generic;

namespace MyApp.Controllers
{
    public class ReportController : Controller
    {
        public class ReportData
        {
            public string Title { get; set; }
            public DateTime GeneratedDate { get; set; }
            public List<ReportItem> Items { get; set; }
        }

        public class ReportItem
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total => Quantity * UnitPrice;
        }

        // Return PDF directly from controller action
        public IActionResult GeneratePdf()
        {
            var model = new ReportData
            {
                Title = "Q4 Sales Report",
                GeneratedDate = DateTime.Now,
                Items = new List<ReportItem>
                {
                    new ReportItem { ProductName = "Widget A", Quantity = 100, UnitPrice = 25.50m },
                    new ReportItem { ProductName = "Gadget B", Quantity = 50, UnitPrice = 47.99m }
                }
            };

            // Rotativa convenience: render Razor view as PDF
            return new ViewAsPdf("ReportTemplate", model)
            {
                FileName = $"report-{DateTime.Now:yyyyMMdd}.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
                PageMargins = new Margins(10, 10, 10, 10),
                // Additional wkhtmltopdf options
                CustomSwitches = "--enable-local-file-access"
            };
        }

        // For saving to disk instead of returning to browser
        public IActionResult SavePdfToDisk()
        {
            var model = new ReportData
            {
                Title = "Q4 Sales Report",
                GeneratedDate = DateTime.Now,
                Items = new List<ReportItem>
                {
                    new ReportItem { ProductName = "Widget A", Quantity = 100, UnitPrice = 25.50m }
                }
            };

            var pdf = new ViewAsPdf("ReportTemplate", model)
            {
                PageSize = Size.A4
            };

            // BuildFile is synchronous, blocks thread during generation
            byte[] pdfBytes = pdf.BuildFile(ControllerContext);
            System.IO.File.WriteAllBytes("output.pdf", pdfBytes);

            return Ok("PDF saved to disk");
        }
    }
}
```

**Corresponding Razor view** (`Views/Report/ReportTemplate.cshtml`):
```html
@model MyApp.Controllers.ReportController.ReportData

<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        h1 { color: #333; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        th, td { padding: 10px; border: 1px solid #ddd; text-align: left; }
        .total { text-align: right; font-weight: bold; }
    </style>
</head>
<body>
    <h1>@Model.Title</h1>
    <p>Generated: @Model.GeneratedDate.ToString("yyyy-MM-dd")</p>
    <table>
        <thead>
            <tr><th>Product</th><th>Qty</th><th>Price</th><th>Total</th></tr>
        </thead>
        <tbody>
            @foreach (var item in Model.Items)
            {
                <tr>
                    <td>@item.ProductName</td>
                    <td>@item.Quantity</td>
                    <td>$@item.UnitPrice.ToString("F2")</td>
                    <td class="total">$@item.Total.ToString("F2")</td>
                </tr>
            }
        </tbody>
    </table>
</body>
</html>
```

**Setup required** (`Program.cs` for .NET 6+):
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseRouting();

// Configure Rotativa (points to wkhtmltopdf binary location)
IWebHostEnvironment env = app.Environment;
Rotativa.AspNetCore.RotativaConfiguration.Setup(env.WebRootPath, "Rotativa");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

**Deployment checklist for Rotativa:**
- [ ] wkhtmltopdf binaries in `wwwroot/Rotativa` folder (Windows: `wkhtmltopdf.exe`, Linux: `wkhtmltopdf`)
- [ ] Linux: Install dependencies (`libgdiplus`, `libfontconfig1`, X11 libraries)
- [ ] File system write permissions for temp file creation
- [ ] Process execution permissions (wkhtmltopdf must be executable)
- [ ] Network access if HTML references external resources

**Technical limitations:**
- **MVC dependency:** Cannot easily use outside controller actions.
- **Synchronous BuildFile:** Blocks threads during generation (50-500 ms depending on complexity).
- **Binary management:** Must ship correct wkhtmltopdf binary for deployment platform.
- **Read-only file system issues:** Fails in Azure App Service (Windows), AWS Lambda, Kubernetes pods without temp directory configuration.
- **No PDF manipulation:** Generated PDF cannot be merged, watermarked, or signed within Rotativa—requires external libraries.

### IronPDF — Any Context (MVC, Background, Console)

IronPDF works in any .NET context. Here's the same report in an MVC controller:

```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace MyApp.Controllers
{
    public class ReportController : Controller
    {
        public class ReportData
        {
            public string Title { get; set; }
            public DateTime GeneratedDate { get; set; }
            public List<ReportItem> Items { get; set; }
        }

        public class ReportItem
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total => Quantity * UnitPrice;
        }

        // Generate PDF and return to browser
        public IActionResult GeneratePdf()
        {
            var model = new ReportData
            {
                Title = "Q4 Sales Report",
                GeneratedDate = DateTime.Now,
                Items = new List<ReportItem>
                {
                    new ReportItem { ProductName = "Widget A", Quantity = 100, UnitPrice = 25.50m },
                    new ReportItem { ProductName = "Gadget B", Quantity = 50, UnitPrice = 47.99m }
                }
            };

            // Build HTML (could also render Razor view to string)
            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 40px; }}
                        h1 {{ color: #333; }}
                        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                        th, td {{ padding: 10px; border: 1px solid #ddd; text-align: left; }}
                        .total {{ text-align: right; font-weight: bold; }}
                    </style>
                </head>
                <body>
                    <h1>{model.Title}</h1>
                    <p>Generated: {model.GeneratedDate:yyyy-MM-dd}</p>
                    <table>
                        <thead>
                            <tr><th>Product</th><th>Qty</th><th>Price</th><th>Total</th></tr>
                        </thead>
                        <tbody>
                            {string.Join("", model.Items.Select(item => $@"
                            <tr>
                                <td>{item.ProductName}</td>
                                <td>{item.Quantity}</td>
                                <td>${item.UnitPrice:F2}</td>
                                <td class='total'>${item.Total:F2}</td>
                            </tr>"))}
                        </tbody>
                    </table>
                </body>
                </html>
            ";

            // Generate PDF (no external process, no temp files)
            var renderer = new ChromePdfRenderer();
            using var pdf = renderer.RenderHtmlAsPdf(html);

            // Return as file to browser
            return File(pdf.BinaryData, "application/pdf", $"report-{DateTime.Now:yyyyMMdd}.pdf");
        }

        // Async version for high-concurrency scenarios
        public async Task<IActionResult> GeneratePdfAsync()
        {
            var model = new ReportData
            {
                Title = "Q4 Sales Report",
                GeneratedDate = DateTime.Now,
                Items = new List<ReportItem>
                {
                    new ReportItem { ProductName = "Widget A", Quantity = 100, UnitPrice = 25.50m }
                }
            };

            var html = $@"<!DOCTYPE html><html><body><h1>{model.Title}</h1></body></html>";

            var renderer = new ChromePdfRenderer();
            using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

            return File(pdf.BinaryData, "application/pdf", "report.pdf");
        }
    }
}
```

**Deployment checklist for IronPDF:**
- [ ] None—just add NuGet package

**Technical advantages:**
- **No MVC dependency:** Same code works in console apps, background services, Azure Functions.
- **Async support:** `RenderHtmlAsPdfAsync()` for non-blocking generation.
- **No binary management:** Embedded Chromium engine, no external files required.
- **Works in read-only file systems:** No temp file writes.
- **PDF manipulation:** Can merge, watermark, sign, encrypt in the same library—see [merge/split guide](https://ironpdf.com/how-to/merge-or-split-pdfs/).

IronPDF also works outside MVC contexts. Example background service:

```csharp
public class ReportGenerationService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Generate nightly reports
            var renderer = new ChromePdfRenderer();
            using var pdf = await renderer.RenderHtmlAsPdfAsync("<h1>Nightly Report</h1>");
            pdf.SaveAs($"reports/nightly-{DateTime.Now:yyyyMMdd}.pdf");

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
```

Rotativa cannot easily support this pattern without controller contexts.

---

## Code Comparison: Applying Watermarks

### Rotativa — No Built-in Watermark API

Rotativa does not provide watermark functionality. Watermarks must be added to the HTML itself or applied post-generation with external libraries:

```csharp
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

public class ReportController : Controller
{
    public IActionResult GeneratePdfWithWatermark()
    {
        // Watermark must be in HTML/CSS (not a true PDF watermark)
        var html = @"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body { font-family: Arial; margin: 40px; position: relative; }
                    .watermark {
                        position: fixed;
                        top: 50%;
                        left: 50%;
                        transform: translate(-50%, -50%) rotate(-45deg);
                        font-size: 72px;
                        color: rgba(200, 200, 200, 0.3);
                        z-index: -1;
                        white-space: nowrap;
                    }
                </style>
            </head>
            <body>
                <div class='watermark'>CONFIDENTIAL</div>
                <h1>Quarterly Financial Report</h1>
                <p>This document contains sensitive information.</p>
            </body>
            </html>
        ";

        // Generate PDF with HTML watermark (not a true PDF watermark layer)
        return new ViewAsPdf
        {
            ViewName = "WatermarkTemplate", // Or use SetContentAsync if passing HTML string
            PageSize = Size.A4
        };
    }
}
```

**Technical limitations:**
- **Not a true PDF watermark:** This is HTML styled to look like a watermark. It's part of page content, not a PDF annotation layer.
- **No post-generation watermark:** Cannot apply watermarks to already-generated PDFs without external libraries.
- **No opacity/rotation control:** Limited to CSS capabilities, which may not render consistently across wkhtmltopdf versions.
- **No per-page variation:** All pages get the same watermark; no dynamic per-page watermarks.

To apply true PDF watermarks to Rotativa-generated PDFs, you must use external libraries like iText, PdfSharp, or IronPDF post-generation.

### IronPDF — Built-in Watermark API

IronPDF provides dedicated watermarking methods:

```csharp
using IronPdf;

public class ReportController : Controller
{
    public IActionResult GeneratePdfWithWatermark()
    {
        // Generate or load PDF
        var renderer = new ChromePdfRenderer();
        using var pdf = renderer.RenderHtmlAsPdf(@"
            <h1>Quarterly Financial Report</h1>
            <p>This document contains sensitive information.</p>
        ");

        // Apply true PDF watermark (annotation layer, not page content)
        pdf.ApplyWatermark("<div style='font-size: 72px; color: #cccccc; font-weight: bold;'>CONFIDENTIAL</div>", 
            rotation: -45, 
            opacity: 30);

        return File(pdf.BinaryData, "application/pdf", "report-watermarked.pdf");
    }
}
```

IronPDF watermarks are true PDF annotation layers with configurable rotation, opacity, and positioning. See [watermarking guide](https://ironpdf.com/how-to/custom-watermark/) for advanced scenarios.

---

## Deployment Checklist

### Rotativa Deployment Requirements

**File System Setup:**
- [ ] Create `wwwroot/Rotativa` folder (or configured path)
- [ ] Copy platform-specific wkhtmltopdf binary:
  - Windows: `wkhtmltopdf.exe` (~50 MB)
  - Linux: `wkhtmltopdf` binary (~50 MB)
- [ ] Ensure binaries are executable (Linux: `chmod +x wkhtmltopdf`)
- [ ] Configure `RotativaConfiguration.Setup(env, "Rotativa")` in `Program.cs`

**Linux Dependencies (typically required):**
```bash
# Ubuntu/Debian
apt-get update
apt-get install -y \
    libgdiplus \
    libfontconfig1 \
    libxrender1 \
    libxext6 \
    libx11-6 \
    libssl1.1

# Alpine (lighter, but more dependencies)
apk add --no-cache \
    libgdiplus \
    libstdc++ \
    fontconfig \
    freetype
```

**Azure App Service:**
- [ ] Add wkhtmltopdf binaries to `D:\home\site\wwwroot\Rotativa`
- [ ] Verify write permissions for temp directory (`D:\local\Temp`)
- [ ] May require custom deployment scripts to ensure binaries are deployed

**Docker:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install wkhtmltopdf dependencies
RUN apt-get update && apt-get install -y \
    wkhtmltopdf \
    libgdiplus \
    libfontconfig1

WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

**Kubernetes:**
- [ ] Base image must include wkhtmltopdf and dependencies
- [ ] Persistent volume or init container to copy binaries
- [ ] Writable temp directory mounted

**Failure Modes:**
- "wkhtmltopdf not found" → Binary missing or wrong path
- "Permission denied" → Binary not executable (Linux)
- "Cannot open display" → Missing X11 dependencies (Linux)
- "Failed to load PDF" → Temp directory not writable

### IronPDF Deployment Requirements

**All Platforms:**
- [ ] Add `IronPdf` NuGet package

That's it. No binaries, no dependencies, no configuration.

**Docker:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

**Kubernetes:**
- Standard .NET deployment—no special requirements

**Azure App Service / AWS Lambda / Google Cloud Run:**
- Works out-of-box with no configuration

---

## API Mapping Reference

| Operation | Rotativa API | IronPDF API |
|-----------|--------------|-------------|
| **Render Razor view to PDF** | `new ViewAsPdf("ViewName", model)` | Render view to string, then `ChromePdfRenderer.RenderHtmlAsPdf()` |
| **Render HTML string to PDF** | `new HtmlAsPdf() { ViewData = html }` | `ChromePdfRenderer.RenderHtmlAsPdf(html)` |
| **Render URL to PDF** | `new UrlAsPdf("https://example.com")` | `ChromePdfRenderer.RenderUrlAsPdf("https://example.com")` |
| **Set page size** | `PageSize = Size.A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` |
| **Set margins** | `PageMargins = new Margins(10, 10, 10, 10)` | `RenderingOptions.MarginTop/Bottom/Left/Right` |
| **Return PDF to browser** | Return `ViewAsPdf` from controller | Return `File(pdf.BinaryData, "application/pdf")` |
| **Save PDF to disk** | `pdf.BuildFile(ControllerContext)` | `pdf.SaveAs("path/to/file.pdf")` |
| **Async generation** | Not supported (BuildFile is synchronous) | `await RenderHtmlAsPdfAsync()` |
| **Apply watermark** | Not supported (use HTML/CSS) | `pdf.ApplyWatermark()` or `pdf.ApplyStamp()` |
| **Merge PDFs** | Not supported | `PdfDocument.Merge()` |
| **Extract text** | Not supported | `PdfDocument.ExtractAllText()` |
| **Digital signature** | Not supported | `PdfDocument.Sign()` |
| **Password protection** | Not supported | `SecuritySettings.UserPassword/OwnerPassword` |

---

## Comprehensive Feature Comparison

| Category | Feature | Rotativa | IronPDF |
|----------|---------|----------|---------|
| **Status** | Active development | Yes (1.4.0 latest for AspNetCore) | Yes |
| | Commercial support | Community support only | 24/5 engineer support (24/7 premium) |
| | Underlying engine status | wkhtmltopdf (maintenance mode, 2020) | Chromium (actively updated) |
| **Support** | Documentation quality | Good (MVC-focused examples) | Extensive (tutorials, how-tos, API reference) |
| | Response time | Community-dependent (GitHub issues) | <1 min median chat response |
| **Content Creation** | HTML rendering | Qt WebKit via wkhtmltopdf (~2015 standards) | Modern Chromium (full HTML5, CSS3, ES6+) |
| | CSS Grid support | Limited/inconsistent | Full support |
| | Flexbox support | Partial | Full support |
| | JavaScript execution | Limited (older engine) | Full modern JavaScript (ES6+, async/await) |
| | External binary required | Yes (wkhtmltopdf, ~50 MB) | No (embedded engine) |
| **Architecture** | MVC controller support | Excellent (primary use case) | Supported (via File() return) |
| | Background service support | Difficult (requires ControllerContext workarounds) | Native support |
| | Console app support | Difficult | Native support |
| | Azure Functions / Lambda | Difficult (binary management, temp files) | Native support |
| | Async/await support | No (BuildFile is synchronous) | Yes (RenderHtmlAsPdfAsync) |
| **PDF Operations** | Merge PDFs | No | Yes |
| | Split PDFs | No | Yes (extract pages) |
| | Add watermarks | No (HTML only) | Yes (true PDF watermarks) |
| | Digital signatures | No | Yes |
| | Form filling | No | Yes |
| | Extract text | No | Yes |
| | Password protection | No | Yes |
| **Deployment** | Binary management | Required (wkhtmltopdf) | Not required |
| | Linux dependencies | Many (X11, fontconfig, etc.) | None |
| | Read-only file systems | Fails (needs temp directory) | Supported |
| | Docker complexity | Medium (base image + deps) | Low (standard .NET image) |
| | Azure App Service | Requires configuration | Works out-of-box |
| **Known Issues** | wkhtmltopdf maintenance mode | Yes (no feature development) | N/A |
| | Modern CSS support | Limited (pre-2015) | Full (current Chromium) |
| | File permission errors | Common in cloud/container environments | Rare |
| | Synchronous blocking | Yes (BuildFile) | No (async support) |
| **Development** | Learning curve | Low (if familiar with MVC) | Low (HTML-focused API) |
| | .NET Core 3.1, 5, 6, 7, 8 | Yes | Yes |
| | .NET Framework 4.6.2+ | Yes (via Rotativa) | Yes |
| | Linux support | Yes (with dependencies) | Yes (native) |
| | macOS support | Limited | Yes |

---

## Decision Matrix

Use this matrix to score your requirements:

| Requirement | Rotativa Score | IronPDF Score |
|-------------|---------------|---------------|
| **Only MVC controllers** | ✅✅✅ (Excellent fit) | ✅✅ (Supported) |
| **Background jobs / console apps** | ❌ (Difficult) | ✅✅✅ (Native) |
| **Modern CSS (Grid, Flexbox)** | ⚠️ (Limited) | ✅✅✅ (Full support) |
| **Read-only file systems** | ❌ (Fails) | ✅✅✅ (Supported) |
| **Cross-platform (Linux, macOS)** | ⚠️ (Requires dependencies) | ✅✅✅ (Native) |
| **PDF manipulation (merge, watermark)** | ❌ (Not supported) | ✅✅✅ (Built-in) |
| **Async/await for high concurrency** | ❌ (Synchronous only) | ✅✅✅ (Full async) |
| **Commercial support / SLA** | ❌ (Community only) | ✅✅✅ (24/5 or 24/7) |
| **No binary management** | ❌ (Requires wkhtmltopdf) | ✅✅✅ (Embedded engine) |
| **Simple MVC setup** | ✅✅✅ (Purpose-built) | ✅✅ (Straightforward) |

**Scoring key:** ✅✅✅ Excellent | ✅✅ Good | ✅ Acceptable | ⚠️ Requires workarounds | ❌ Not supported/difficult

---

## Installation Comparison

### Rotativa

```bash
# Install via NuGet
dotnet add package Rotativa.AspNetCore

# Or via Package Manager Console
Install-Package Rotativa.AspNetCore
```

**Additional setup required:**
1. Create `wwwroot/Rotativa` folder
2. Download wkhtmltopdf binary for your platform
3. Copy binary to `wwwroot/Rotativa` folder
4. Configure in `Program.cs`:

```csharp
using Rotativa.AspNetCore;

var app = builder.Build();
IWebHostEnvironment env = app.Environment;
RotativaConfiguration.Setup(env.WebRootPath, "Rotativa");
```

5. On Linux, install dependencies (see deployment checklist above)

### IronPDF

```bash
# Install via NuGet
dotnet add package IronPdf

# Or via Package Manager Console
Install-Package IronPdf
```

That's it. No additional setup, no binaries, no dependencies.

---

## Conclusion

Rotativa is a specialized tool for ASP.NET MVC applications that need simple "Export to PDF" buttons on web pages. If your application is MVC-only, you have file system write access, and your HTML uses pre-2015 CSS standards, Rotativa's convenience methods (`ViewAsPdf`, `ActionAsPdf`) provide quick integration. The project is actively maintained for its core use case, and the community support is reasonable for straightforward scenarios.

However, Rotativa's architectural constraints become friction points when your requirements expand. Background services, console apps, Azure Functions, and serverless architectures require workarounds. Modern CSS features (Grid, Flexbox) render inconsistently. Read-only file systems in cloud environments cause deployment failures. Post-generation operations (merging, watermarking, signing) require external libraries. The dependency on wkhtmltopdf—a maintenance-mode project with a 2015-era rendering engine—limits future viability for cutting-edge web standards.

IronPDF's embedded Chromium architecture eliminates these constraints. It works in any .NET context (MVC, background services, console apps, functions), supports modern HTML5/CSS3/JavaScript standards, requires no binary management, and includes comprehensive PDF manipulation features in a single library. The async-first API scales for high-concurrency scenarios, and 24/5 commercial support (24/7 premium) provides production confidence. For teams building beyond simple MVC controller actions or deploying to modern cloud/container environments, IronPDF's library-based approach is the pragmatic choice.

Does your application architecture fit Rotativa's MVC-only constraints, or do you need the flexibility of a general-purpose PDF library?

**Further reading:**
- [IronPDF HTML to PDF tutorial with Razor view integration](https://ironpdf.com/tutorials/html-to-pdf/)
- [Deploy IronPDF in Docker and Kubernetes](https://ironpdf.com/how-to/html-file-to-pdf/)
