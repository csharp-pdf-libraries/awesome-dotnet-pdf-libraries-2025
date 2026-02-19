# How Do I Migrate from Rotativa to IronPDF in C#?

## ⚠️ CRITICAL SECURITY ADVISORY

**Rotativa wraps wkhtmltopdf, which has CRITICAL UNPATCHED SECURITY VULNERABILITIES.**

### CVE-2022-35583 — Server-Side Request Forgery (SSRF)

| Attribute | Value |
|-----------|-------|
| **CVE ID** | CVE-2022-35583 |
| **Severity** | **CRITICAL (9.8/10)** |
| **Attack Vector** | Network |
| **Status** | **WILL NEVER BE PATCHED** |
| **Affected** | ALL Rotativa versions |

**wkhtmltopdf was officially abandoned in December 2022.** The maintainers explicitly stated they will NOT fix security vulnerabilities. Every application using Rotativa is permanently exposed.

### How the Attack Works

```html
<!-- Attacker submits this content via your MVC model -->
<iframe src="http://169.254.169.254/latest/meta-data/iam/security-credentials/"></iframe>
<img src="http://internal-database:5432/admin" />
```

**Impact:**
- Access AWS/Azure/GCP cloud metadata endpoints
- Steal internal API data and credentials
- Port scan internal networks
- Exfiltrate sensitive configuration

---

## Table of Contents
1. [Why Migrate NOW](#why-migrate-now)
2. [Rotativa's Critical Problems](#rotativas-critical-problems)
3. [Quick Start Migration (5 Minutes)](#quick-start-migration-5-minutes)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [ASP.NET Core Migration](#aspnet-core-migration)
7. [Features Not Available in Rotativa](#features-not-available-in-rotativa)
8. [Deployment Comparison](#deployment-comparison)
9. [Troubleshooting Guide](#troubleshooting-guide)
10. [Migration Checklist](#migration-checklist)
11. [Additional Resources](#additional-resources)

---

## Why Migrate NOW

### The Security Crisis

These risks, extensively documented in the [comprehensive migration walkthrough](https://ironpdf.com/blog/migration-guides/migrate-from-rotativa-to-ironpdf/), represent critical vulnerabilities that organizations must address immediately.

| Risk | Rotativa | IronPDF |
|------|----------|---------|
| **CVE-2022-35583 (SSRF)** | ❌ VULNERABLE | ✅ Protected |
| **Local File Access** | ❌ VULNERABLE | ✅ Sandboxed |
| **Internal Network Access** | ❌ VULNERABLE | ✅ Restricted |
| **Security Patches** | ❌ NEVER | ✅ Regular updates |
| **Active Development** | ❌ Abandoned | ✅ Monthly releases |

### The Technology Crisis

Rotativa wraps wkhtmltopdf, which uses:
- **Qt WebKit 4.8** (from 2012)
- **No Flexbox support**
- **No CSS Grid support**
- **Broken JavaScript execution**
- **No ES6+ support**

### Feature Comparison

| Feature | Rotativa | IronPDF |
|---------|----------|---------|
| **Security** | ❌ Critical CVEs | ✅ No vulnerabilities |
| **HTML Rendering** | ⚠️ Outdated WebKit | ✅ Modern Chromium |
| **CSS3** | ❌ Partial | ✅ Full support |
| **Flexbox/Grid** | ❌ Not supported | ✅ Full support |
| **JavaScript** | ⚠️ Unreliable | ✅ Full ES6+ |
| **ASP.NET Core** | ⚠️ Limited ports | ✅ Native support |
| **Razor Pages** | ❌ Not supported | ✅ Full support |
| **Blazor** | ❌ Not supported | ✅ Full support |
| **PDF Manipulation** | ❌ Not available | ✅ Full |
| **Digital Signatures** | ❌ Not available | ✅ Full |
| **PDF/A Compliance** | ❌ Not available | ✅ Full |
| **Async/Await** | ❌ Synchronous only | ✅ Full async |
| **Active Maintenance** | ❌ Abandoned | ✅ Weekly updates |

---

## Rotativa's Critical Problems

### Problem 1: MVC-Only Architecture

Rotativa was designed for ASP.NET MVC 5 and earlier:

```csharp
// ❌ Rotativa - Only works with classic MVC pattern
public class InvoiceController : Controller
{
    public ActionResult InvoicePdf(int id)
    {
        var model = GetInvoice(id);
        return new ViewAsPdf("Invoice", model);  // Tied to MVC Views
    }
}

// Problems:
// - No Razor Pages support
// - No Blazor support
// - No minimal APIs support
// - No ASP.NET Core native integration
```

### Problem 2: wkhtmltopdf Binary Management

```csharp
// ❌ Rotativa - Requires manual binary management
// Must include wkhtmltopdf.exe in your project
// Different binaries for x86/x64/Linux/Mac
// Security vulnerabilities in all versions

// Deployment headaches:
// 1. Copy wkhtmltopdf.exe to deployment
// 2. Set correct permissions
// 3. Handle different platforms
// 4. Update PATH environment
```

### Problem 3: Synchronous Only

```csharp
// ❌ Rotativa - Blocks the thread
public ActionResult GeneratePdf()
{
    return new ViewAsPdf("Report");
    // This blocks the request thread until PDF is complete
    // Poor scalability under load
}

// ✅ IronPDF - Full async support
public async Task<IActionResult> GeneratePdf()
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return File(pdf.BinaryData, "application/pdf");
    // Non-blocking, better scalability
}
```

---

## Quick Start Migration (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove Rotativa
dotnet remove package Rotativa
dotnet remove package Rotativa.AspNetCore

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Remove wkhtmltopdf Binaries

Delete these files from your project:
- `wkhtmltopdf.exe`
- `wkhtmltox.dll`
- Any `Rotativa/` folders

### Step 3: Update Using Statements

```csharp
// Before
using Rotativa;
using Rotativa.Options;

// After
using IronPdf;
```

### Step 4: Add License Key

```csharp
// Add in Program.cs or Startup.cs
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 5: Update Controller Code

**Before (Rotativa):**
```csharp
using Rotativa;

public class ReportController : Controller
{
    public ActionResult DownloadReport(int id)
    {
        var model = GetReportData(id);
        return new ViewAsPdf("Report", model)
        {
            FileName = "Report.pdf",
            PageOrientation = Orientation.Portrait,
            PageSize = Size.A4
        };
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class ReportController : Controller
{
    public async Task<IActionResult> DownloadReport(int id)
    {
        var model = GetReportData(id);

        // Render view to HTML first
        var html = await RenderViewToStringAsync("Report", model);

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

        var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return File(pdf.BinaryData, "application/pdf", "Report.pdf");
    }
}
```

---

## Complete API Reference

### Namespace Mapping

| Rotativa Namespace | IronPDF Namespace |
|-------------------|-------------------|
| `Rotativa` | `IronPdf` |
| `Rotativa.Options` | `IronPdf.Rendering` |
| `Rotativa.AspNetCore` | `IronPdf` |

### Core Class Mapping

| Rotativa Class | IronPDF Equivalent | Notes |
|----------------|-------------------|-------|
| `ViewAsPdf` | `ChromePdfRenderer` | Render HTML |
| `ActionAsPdf` | `ChromePdfRenderer.RenderUrlAsPdf()` | Render URL |
| `UrlAsPdf` | `ChromePdfRenderer.RenderUrlAsPdf()` | Render URL |
| `Orientation` enum | `PdfPaperOrientation` enum | Orientation |
| `Size` enum | `PdfPaperSize` enum | Paper size |

### Property Mapping

| Rotativa Property | IronPDF Property |
|-------------------|------------------|
| `FileName` | Use `File()` method name parameter |
| `PageOrientation` | `RenderingOptions.PaperOrientation` |
| `PageSize` | `RenderingOptions.PaperSize` |
| `PageWidth` | `SetCustomPaperSizeInMillimeters()` |
| `PageHeight` | `SetCustomPaperSizeInMillimeters()` |
| `PageMargins.Top` | `RenderingOptions.MarginTop` |
| `PageMargins.Bottom` | `RenderingOptions.MarginBottom` |
| `PageMargins.Left` | `RenderingOptions.MarginLeft` |
| `PageMargins.Right` | `RenderingOptions.MarginRight` |
| `CustomSwitches` | Various `RenderingOptions` properties |

### CustomSwitches Migration

| Rotativa CustomSwitch | IronPDF Equivalent |
|----------------------|-------------------|
| `--page-offset 0` | `RenderingOptions.FirstPageNumber = 1` |
| `--footer-center [page]` | `HtmlFooter` with `{page}` |
| `--footer-font-size 8` | CSS in footer HTML |
| `--header-html ...` | `RenderingOptions.HtmlHeader` |
| `--footer-html ...` | `RenderingOptions.HtmlFooter` |
| `--javascript-delay 500` | `WaitFor.JavaScript(500)` |
| `--no-background` | `PrintHtmlBackgrounds = false` |
| `--print-media-type` | `CssMediaType = Print` |
| `--disable-smart-shrinking` | `FitToPaperMode` settings |

---

## Code Migration Examples

### Example 1: Basic View to PDF

**Before (Rotativa):**
```csharp
using Rotativa;

public class InvoiceController : Controller
{
    public ActionResult InvoicePdf(int id)
    {
        var invoice = _invoiceService.GetInvoice(id);
        return new ViewAsPdf("Invoice", invoice)
        {
            FileName = "Invoice.pdf"
        };
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;

public class InvoiceController : Controller
{
    private readonly IViewRenderService _viewRenderService;

    public InvoiceController(IViewRenderService viewRenderService)
    {
        _viewRenderService = viewRenderService;
    }

    public async Task<IActionResult> InvoicePdf(int id)
    {
        var invoice = _invoiceService.GetInvoice(id);

        // Render Razor view to HTML string
        var html = await _viewRenderService.RenderToStringAsync("Invoice", invoice);

        var renderer = new ChromePdfRenderer();
        var pdf = await renderer.RenderHtmlAsPdfAsync(html);

        return File(pdf.BinaryData, "application/pdf", "Invoice.pdf");
    }
}
```

### Example 2: URL to PDF

**Before (Rotativa):**
```csharp
using Rotativa;

public ActionResult DownloadWebPage()
{
    return new UrlAsPdf("https://example.com/report")
    {
        FileName = "Report.pdf",
        PageSize = Size.Letter
    };
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public async Task<IActionResult> DownloadWebPage()
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;

    var pdf = await renderer.RenderUrlAsPdfAsync("https://example.com/report");
    return File(pdf.BinaryData, "application/pdf", "Report.pdf");
}
```

### Example 3: Custom Page Size and Margins

**Before (Rotativa):**
```csharp
using Rotativa;
using Rotativa.Options;

public ActionResult CustomPdf()
{
    var model = GetData();
    return new ViewAsPdf("Report", model)
    {
        FileName = "Report.pdf",
        PageOrientation = Orientation.Landscape,
        PageSize = Size.A4,
        PageMargins = new Margins(20, 15, 20, 15)
    };
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public async Task<IActionResult> CustomPdf()
{
    var model = GetData();
    var html = await RenderViewToStringAsync("Report", model);

    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.MarginTop = 20;
    renderer.RenderingOptions.MarginBottom = 20;
    renderer.RenderingOptions.MarginLeft = 15;
    renderer.RenderingOptions.MarginRight = 15;

    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return File(pdf.BinaryData, "application/pdf", "Report.pdf");
}
```

### Example 4: Headers and Footers with Page Numbers

**Before (Rotativa):**
```csharp
using Rotativa;

public ActionResult ReportWithHeaders()
{
    return new ViewAsPdf("Report", model)
    {
        FileName = "Report.pdf",
        CustomSwitches = @"
            --header-html ""C:\Templates\header.html""
            --footer-center ""Page [page] of [topage]""
            --footer-font-size 9
            --header-spacing 5
            --footer-spacing 5"
    };
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public async Task<IActionResult> ReportWithHeaders()
{
    var html = await RenderViewToStringAsync("Report", model);

    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
    {
        HtmlFragment = @"
            <div style='width:100%; text-align:center; font-size:12px;
                        border-bottom: 1px solid #ccc; padding: 10px;'>
                Company Report Header
            </div>",
        MaxHeight = 30
    };

    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
    {
        HtmlFragment = @"
            <div style='width:100%; text-align:center; font-size:9px;'>
                Page {page} of {total-pages}
            </div>",
        MaxHeight = 20
    };

    renderer.RenderingOptions.MarginTop = 40;
    renderer.RenderingOptions.MarginBottom = 30;

    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return File(pdf.BinaryData, "application/pdf", "Report.pdf");
}
```

### Example 5: JavaScript Wait

**Before (Rotativa):**
```csharp
using Rotativa;

public ActionResult DynamicContent()
{
    return new ViewAsPdf("Dashboard", model)
    {
        CustomSwitches = "--javascript-delay 3000"
    };
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public async Task<IActionResult> DynamicContent()
{
    var html = await RenderViewToStringAsync("Dashboard", model);

    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.EnableJavaScript = true;
    renderer.RenderingOptions.WaitFor.JavaScript(3000);

    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return File(pdf.BinaryData, "application/pdf", "Dashboard.pdf");
}
```

### Example 6: Action to PDF (Internal URL)

**Before (Rotativa):**
```csharp
using Rotativa;

public ActionResult DownloadInvoice(int id)
{
    return new ActionAsPdf("Invoice", new { id = id })
    {
        FileName = $"Invoice_{id}.pdf"
    };
}

public ActionResult Invoice(int id)
{
    var model = GetInvoice(id);
    return View(model);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public async Task<IActionResult> DownloadInvoice(int id)
{
    // Option 1: Render the view directly
    var model = GetInvoice(id);
    var html = await RenderViewToStringAsync("Invoice", model);

    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);

    return File(pdf.BinaryData, "application/pdf", $"Invoice_{id}.pdf");
}

// Option 2: Render the URL (if needed)
public async Task<IActionResult> DownloadInvoiceFromUrl(int id)
{
    var renderer = new ChromePdfRenderer();
    var url = Url.Action("Invoice", "Report", new { id = id }, Request.Scheme);
    var pdf = await renderer.RenderUrlAsPdfAsync(url);

    return File(pdf.BinaryData, "application/pdf", $"Invoice_{id}.pdf");
}
```

---

## ASP.NET Core Migration

### View Render Service

To render Razor views to HTML strings in ASP.NET Core, create a service:

```csharp
public interface IViewRenderService
{
    Task<string> RenderToStringAsync(string viewName, object model);
}

public class ViewRenderService : IViewRenderService
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ViewRenderService(
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> RenderToStringAsync(string viewName, object model)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var actionContext = new ActionContext(
            httpContext,
            httpContext.GetRouteData(),
            new ActionDescriptor());

        using var sw = new StringWriter();

        var viewResult = _viewEngine.FindView(actionContext, viewName, false);

        if (viewResult.View == null)
        {
            viewResult = _viewEngine.GetView(null, viewName, false);
        }

        if (viewResult.View == null)
        {
            throw new ArgumentNullException($"View {viewName} not found");
        }

        var viewDictionary = new ViewDataDictionary(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary())
        {
            Model = model
        };

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
            sw,
            new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}
```

### Register in Program.cs

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();

// Add IronPDF license
IronPdf.License.LicenseKey = builder.Configuration["IronPdf:LicenseKey"];
```

### Alternative: Direct HTML Approach

If you don't need Razor views, use HTML directly:

```csharp
public async Task<IActionResult> GeneratePdf(int id)
{
    var invoice = await _invoiceService.GetAsync(id);

    var html = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .header {{ background: #007bff; color: white; padding: 20px; }}
                table {{ width: 100%; border-collapse: collapse; }}
                th, td {{ padding: 10px; border: 1px solid #ddd; }}
            </style>
        </head>
        <body>
            <div class='header'>
                <h1>Invoice #{invoice.Id}</h1>
            </div>
            <p>Date: {invoice.Date:d}</p>
            <p>Customer: {invoice.CustomerName}</p>
            <table>
                <tr><th>Item</th><th>Qty</th><th>Price</th></tr>
                {string.Join("", invoice.Items.Select(i =>
                    $"<tr><td>{i.Name}</td><td>{i.Qty}</td><td>{i.Price:C}</td></tr>"))}
            </table>
            <p><strong>Total: {invoice.Total:C}</strong></p>
        </body>
        </html>";

    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);

    return File(pdf.BinaryData, "application/pdf", $"Invoice_{id}.pdf");
}
```

---

## Features Not Available in Rotativa

### PDF Manipulation

```csharp
// ❌ Rotativa cannot do ANY of this

// ✅ IronPDF - Merge PDFs
var pdf1 = PdfDocument.FromFile("doc1.pdf");
var pdf2 = PdfDocument.FromFile("doc2.pdf");
var merged = PdfDocument.Merge(pdf1, pdf2);

// ✅ IronPDF - Split PDFs
var pdf = PdfDocument.FromFile("document.pdf");
var pages1to5 = pdf.CopyPages(0, 4);

// ✅ IronPDF - Add watermarks
pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");
```

### Digital Signatures

```csharp
// ❌ Rotativa cannot sign PDFs

// ✅ IronPDF - Digital signatures
var pdf = PdfDocument.FromFile("contract.pdf");
var signature = new PdfSignature("certificate.pfx", "password")
{
    SigningContact = "legal@company.com",
    SigningReason = "Contract Approval"
};
pdf.Sign(signature);
```

### PDF Security

```csharp
// ❌ Rotativa has no security features

// ✅ IronPDF - Full security control
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SecuritySettings.OwnerPassword = "owner123";
pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;
```

### PDF/A Compliance

```csharp
// ❌ Rotativa cannot create PDF/A

// ✅ IronPDF - PDF/A for archival
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAsPdfA("archive.pdf", PdfAVersions.PdfA3b);
```

### Form Filling

```csharp
// ❌ Rotativa cannot fill forms

// ✅ IronPDF - Fill PDF forms
var pdf = PdfDocument.FromFile("form.pdf");
pdf.Form.GetFieldByName("FirstName").Value = "John";
pdf.Form.GetFieldByName("LastName").Value = "Doe";
```

---

## Deployment Comparison

### Rotativa Deployment (Complex)

```
project/
├── MyApp.dll
├── Rotativa/
│   ├── wkhtmltopdf.exe      # 50MB+ binary
│   ├── wkhtmltox.dll        # Additional DLL
│   └── [platform variants]   # x86, x64, Linux, etc.
```

**Deployment steps:**
1. Copy wkhtmltopdf binaries
2. Set PATH or configure path in code
3. Handle platform-specific binaries
4. Manage permissions (execute rights)
5. Handle security vulnerabilities (none available!)

### IronPDF Deployment (Simple)

```
project/
├── MyApp.dll
└── (IronPDF handles Chromium internally)
```

**Deployment steps:**
1. Just deploy your app - done!

### Docker Comparison

**Rotativa Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Complex wkhtmltopdf installation with security vulnerabilities
RUN apt-get update && apt-get install -y \
    wget fontconfig libfreetype6 libjpeg62-turbo \
    libpng16-16 libx11-6 libxcb1 libxext6 libxrender1 \
    xfonts-75dpi xfonts-base \
    && wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6-1/wkhtmltox_0.12.6-1.buster_amd64.deb \
    && dpkg -i wkhtmltox_0.12.6-1.buster_amd64.deb || apt-get install -f -y

# WARNING: CVE-2022-35583 is now in your container!
```

**IronPDF Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y \
    libgdiplus libc6-dev libx11-dev libnss3 \
    libatk-bridge2.0-0 libdrm2 libxkbcommon0 \
    libxcomposite1 libxdamage1 libxfixes3 \
    libxrandr2 libgbm1 libasound2 \
    && rm -rf /var/lib/apt/lists/*

# Secure, modern, no vulnerable binaries
```

---

## Troubleshooting Guide

### Issue 1: ViewAsPdf Not Available

**Solution:** Use `ChromePdfRenderer` with view rendering:
```csharp
// Create ViewRenderService (see ASP.NET Core section)
var html = await _viewRenderService.RenderToStringAsync("ViewName", model);
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 2: Page Number Placeholders Don't Work

**Problem:** Rotativa uses `[page]`, IronPDF uses `{page}`

**Solution:**
```csharp
// Update placeholder syntax
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "Page {page} of {total-pages}"  // Not [page] of [topage]
};
```

### Issue 3: CustomSwitches Not Available

**Problem:** No direct wkhtmltopdf argument support

**Solution:** Use typed `RenderingOptions`:
```csharp
// --javascript-delay 2000
renderer.RenderingOptions.WaitFor.JavaScript(2000);

// --no-background
renderer.RenderingOptions.PrintHtmlBackgrounds = false;

// --print-media-type
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
```

### Issue 4: Async Pattern Change

**Problem:** Rotativa is synchronous, IronPDF supports async

**Solution:**
```csharp
// Change return type
public async Task<IActionResult> GeneratePdf()  // Not ActionResult

// Use async methods
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### Issue 5: View Not Found

**Problem:** View path differences

**Solution:** Use full path or implement `IViewRenderService`:
```csharp
// Use explicit view path
var html = await _viewRenderService.RenderToStringAsync(
    "~/Views/Invoice/Invoice.cshtml", model);
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit all Rotativa usages in codebase**
  ```bash
  grep -r "using Rotativa" --include="*.cs" .
  grep -r "ViewAsPdf\|ActionAsPdf\|UrlAsPdf" --include="*.cs" .
  ```
  **Why:** Rotativa wraps wkhtmltopdf which has critical SSRF vulnerability (CVE-2022-35583). Identify all usages to ensure complete migration away from vulnerable code.

- [ ] **Document all CustomSwitches configurations**
  ```csharp
  // Find patterns like:
  CustomSwitches = @"
      --header-html ""header.html""
      --footer-center ""Page [page] of [topage]""
      --javascript-delay 2000"
  ```
  **Why:** CustomSwitches map to IronPDF's typed `RenderingOptions`. Document these now so no functionality is lost during migration.

- [ ] **Locate wkhtmltopdf binaries**
  ```bash
  find . -name "wkhtmltopdf.exe"
  find . -name "wkhtmltox.dll"
  find . -name "libwkhtmltox*"
  ls -la Rotativa/
  ```
  **Why:** These binaries contain the security vulnerabilities. They must be completely removed—IronPDF doesn't need them.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### During Migration

- [ ] **Remove Rotativa NuGet packages and install IronPdf**
  ```bash
  dotnet remove package Rotativa
  dotnet remove package Rotativa.AspNetCore
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch. IronPDF uses modern Chromium rendering instead of vulnerable wkhtmltopdf.

- [ ] **Delete wkhtmltopdf binaries**
  ```bash
  rm -rf Rotativa/
  rm -f wkhtmltopdf.exe wkhtmltox.dll
  ```
  **Why:** These are the source of CVE-2022-35583. IronPDF needs no native binaries.

- [ ] **Update using statements**
  ```csharp
  // Before
  using Rotativa;
  using Rotativa.Options;

  // After
  using IronPdf;
  ```
  **Why:** IronPDF uses a single namespace for all functionality.

- [ ] **Add license key initialization**
  ```csharp
  // Add in Program.cs or Startup.cs
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Implement IViewRenderService for ASP.NET Core**
  ```csharp
  // Register in Program.cs
  builder.Services.AddHttpContextAccessor();
  builder.Services.AddScoped<IViewRenderService, ViewRenderService>();

  // Use in controller
  var html = await _viewRenderService.RenderToStringAsync("Invoice", model);
  ```
  **Why:** Rotativa's `ViewAsPdf` rendered MVC views directly. IronPDF needs HTML, so you need a service to convert Razor views to HTML strings. See full implementation in ASP.NET Core section.

- [ ] **Replace ViewAsPdf with ChromePdfRenderer**
  ```csharp
  // Before (Rotativa)
  public ActionResult InvoicePdf(int id)
  {
      var model = GetInvoice(id);
      return new ViewAsPdf("Invoice", model)
      {
          FileName = "Invoice.pdf",
          PageSize = Size.A4
      };
  }

  // After (IronPDF)
  public async Task<IActionResult> InvoicePdf(int id)
  {
      var model = GetInvoice(id);
      var html = await _viewRenderService.RenderToStringAsync("Invoice", model);

      var renderer = new ChromePdfRenderer();
      renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

      var pdf = await renderer.RenderHtmlAsPdfAsync(html);
      return File(pdf.BinaryData, "application/pdf", "Invoice.pdf");
  }
  ```
  **Why:** IronPDF separates view rendering from PDF generation. This is actually more flexible—you can render any HTML, not just MVC views.

- [ ] **Replace UrlAsPdf with RenderUrlAsPdf**
  ```csharp
  // Before (Rotativa)
  return new UrlAsPdf("https://example.com") { FileName = "Report.pdf" };

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = await renderer.RenderUrlAsPdfAsync("https://example.com");
  return File(pdf.BinaryData, "application/pdf", "Report.pdf");
  ```
  **Why:** Direct URL rendering without wkhtmltopdf's SSRF vulnerability.

- [ ] **Convert CustomSwitches to RenderingOptions**
  ```csharp
  // Before (Rotativa CustomSwitches)
  CustomSwitches = @"
      --javascript-delay 2000
      --no-background
      --print-media-type
      --footer-center ""Page [page] of [topage]""
      --footer-font-size 9"

  // After (IronPDF RenderingOptions)
  renderer.RenderingOptions.EnableJavaScript = true;
  renderer.RenderingOptions.WaitFor.JavaScript(2000);
  renderer.RenderingOptions.PrintHtmlBackgrounds = false;
  renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='font-size:9px; text-align:center;'>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** Typed properties instead of string arguments. Better IntelliSense, compile-time checking, and no risk of typos.

- [ ] **Update page placeholder syntax**
  ```csharp
  // Rotativa placeholders → IronPDF placeholders
  // [page]      → {page}
  // [topage]    → {total-pages}
  // [date]      → {date}
  // [time]      → {time}
  // [title]     → {html-title}
  // [sitepage]  → {url}
  ```
  **Why:** Different placeholder syntax. Search and replace in all header/footer templates.

- [ ] **Update headers/footers to HTML format**
  ```csharp
  // Before (Rotativa - limited formatting)
  CustomSwitches = @"
      --header-html ""C:\Templates\header.html""
      --footer-center ""Page [page] of [topage]""
      --footer-font-size 9
      --footer-line"

  // After (IronPDF - full HTML/CSS)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = File.ReadAllText("Templates/header.html")
          .Replace("[page]", "{page}")
          .Replace("[topage]", "{total-pages}"),
      MaxHeight = 30
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = @"<div style='font-size:9px; text-align:center;
          border-top:1px solid #ccc; padding-top:5px;'>
          Page {page} of {total-pages}
      </div>",
      DrawDividerLine = false
  };
  ```
  **Why:** IronPDF uses full HTML/CSS for headers/footers instead of limited wkhtmltopdf options. More flexible and consistent rendering.

- [ ] **Change to async pattern**
  ```csharp
  // Before (Rotativa - synchronous)
  public ActionResult GeneratePdf()
  {
      return new ViewAsPdf("Report");  // Blocks thread
  }

  // After (IronPDF - async)
  public async Task<IActionResult> GeneratePdf()
  {
      var html = await _viewRenderService.RenderToStringAsync("Report", model);
      var pdf = await renderer.RenderHtmlAsPdfAsync(html);
      return File(pdf.BinaryData, "application/pdf", "Report.pdf");
  }
  ```
  **Why:** Async pattern improves scalability. Rotativa blocked the request thread; IronPDF supports full async/await for better server performance.

- [ ] **Convert page settings**
  ```csharp
  // Before (Rotativa)
  new ViewAsPdf("Report")
  {
      PageOrientation = Orientation.Landscape,
      PageSize = Size.Letter,
      PageMargins = new Margins(20, 15, 20, 15)
  }

  // After (IronPDF)
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  renderer.RenderingOptions.MarginLeft = 15;
  renderer.RenderingOptions.MarginRight = 15;
  ```
  **Why:** Direct property mapping. Note margin order in Rotativa was Top, Right, Bottom, Left.

### Post-Migration

- [ ] **Run all unit tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine renders more accurately than wkhtmltopdf's outdated WebKit. Usually better, but verify key documents.

- [ ] **Verify CSS rendering improvements**
  ```csharp
  // This now works (broke in Rotativa)
  var html = @"
      <div style='display: flex; justify-content: space-between;'>
          <div>Left</div>
          <div>Right</div>
      </div>
      <div style='display: grid; grid-template-columns: 1fr 1fr 1fr;'>
          <div>Col 1</div><div>Col 2</div><div>Col 3</div>
      </div>";
  var pdf = renderer.RenderHtmlAsPdf(html);  // Works!
  ```
  **Why:** Remove CSS workarounds for wkhtmltopdf limitations. Modern CSS (Flexbox/Grid) now works correctly.

- [ ] **Test JavaScript execution**
  **Why:** Dynamic content that was unreliable in Rotativa should now render consistently with Chromium.

- [ ] **Verify security scan passes**
  ```bash
  # Security scanners should no longer flag:
  # - CVE-2022-35583
  # - wkhtmltopdf vulnerabilities
  ```
  **Why:** The primary reason for migration is security. Verify your scanners no longer flag the vulnerabilities.

- [ ] **Update Docker configurations**
  ```dockerfile
  # Remove these lines:
  RUN wget wkhtmltox... && dpkg -i wkhtmltox...

  # IronPDF only needs:
  RUN apt-get install -y libgdiplus libnss3 libatk-bridge2.0-0 libdrm2 \
      libxkbcommon0 libxcomposite1 libxdamage1 libxfixes3 libxrandr2 libgbm1 libasound2
  ```
  **Why:** Simplify Docker builds and remove vulnerable wkhtmltopdf binaries.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available that Rotativa couldn't do:
  // Merge PDFs
  var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);

  // Digital signatures
  var signature = new PdfSignature("certificate.pfx", "password");
  pdf.Sign(signature);

  // Password protection
  pdf.SecuritySettings.UserPassword = "secret";

  // Watermarks
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");

  // PDF/A archival
  pdf.SaveAsPdfA("archive.pdf", PdfAVersions.PdfA3b);
  ```
  **Why:** IronPDF provides many features Rotativa never had. Consider using them to enhance your application.

---

## Additional Resources

### IronPDF Documentation
- **Getting Started**: https://ironpdf.com/docs/
- **ASP.NET Core Tutorial**: https://ironpdf.com/tutorials/aspnet-core-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/

### Security Resources
- **CVE-2022-35583**: https://nvd.nist.gov/vuln/detail/CVE-2022-35583
- **wkhtmltopdf Status**: https://wkhtmltopdf.org/status.html

### Related Migration Guides
- **[wkhtmltopdf Migration](../wkhtmltopdf/migrate-from-wkhtmltopdf.md)** — Parent technology
- **[DinkToPdf Migration](../dinktopdf/migrate-from-dinktopdf.md)** — Similar wrapper
- **[TuesPechkin Migration](../tuespechkin/migrate-from-tuespechkin.md)** — Similar wrapper

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*
