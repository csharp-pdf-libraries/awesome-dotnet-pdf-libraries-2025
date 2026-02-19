# How Do I Migrate from PDFreactor to IronPDF in C#?

PDFreactor is a powerful Java-based HTML-to-PDF conversion server with excellent CSS Paged Media support. While it excels at high-fidelity document rendering, its Java dependency and server architecture create significant complexity in .NET environments. This guide provides a comprehensive migration path to IronPDF, a native .NET library that offers equivalent rendering capabilities without external dependencies.

## Table of Contents

1. [Why Migrate from PDFreactor to IronPDF?](#why-migrate-from-pdfreactor-to-ironpdf)
2. [Architectural Differences](#architectural-differences)
3. [Installation and Setup](#installation-and-setup)
4. [Core API Mappings](#core-api-mappings)
5. [Code Migration Examples](#code-migration-examples)
6. [Configuration Migration](#configuration-migration)
7. [CSS Paged Media Migration](#css-paged-media-migration)
8. [Headers and Footers](#headers-and-footers)
9. [JavaScript and Async Content](#javascript-and-async-content)
10. [Server vs Library Architecture](#server-vs-library-architecture)
11. [Performance Optimization](#performance-optimization)
12. [Troubleshooting](#troubleshooting)
13. [Migration Checklist](#migration-checklist)

---

## Why Migrate from PDFreactor to IronPDF?

### The Java Dependency Problem

PDFreactor's architecture creates several challenges in .NET environments:

1. **Java Runtime Required**: Requires JRE/JDK installation on all servers
2. **Server Architecture**: Runs as a separate service requiring additional infrastructure
3. **Complex Deployment**: Managing Java dependencies in .NET CI/CD pipelines
4. **Inter-Process Communication**: REST API or socket communication adds latency
5. **Separate License Management**: License bound to server instance, not application
6. **Resource Isolation**: Separate process memory and CPU management
7. **Operational Overhead**: Two runtimes to maintain, monitor, and update

### [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/) Advantages

| Aspect | PDFreactor | IronPDF |
|--------|-----------|---------|
| Runtime | Java (external) | Native .NET |
| Architecture | Server-based service | In-process library |
| Deployment | Complex (Java + service) | NuGet package |
| Dependencies | JRE + REST client | Self-contained |
| Latency | Network/IPC overhead | Direct method calls |
| Scaling | Per-server licensing | Per-developer licensing |
| Integration | REST API calls | Native .NET API |
| Memory | Separate process | In-process control |
| CSS Support | Excellent (Paged Media) | Excellent (Chromium html to pdf c#) |
| PDF Manipulation | Conversion only | Full lifecycle |

For in-depth technical comparisons, visit the [analysis article](https://ironsoftware.com/suite/blog/comparison/compare-pdfreactor-vs-ironpdf/).

---

## Architectural Differences

### PDFreactor Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   .NET App      │────▶│  PDFreactor     │────▶│   PDF Output    │
│  (REST Client)  │ HTTP│    Server       │     │                 │
└─────────────────┘     │   (Java)        │     └─────────────────┘
                        └─────────────────┘
```

### IronPDF Architecture

```
┌─────────────────────────────────────────┐
│              .NET Application           │
│  ┌─────────────────────────────────┐    │
│  │         IronPDF Library         │    │
│  │    (Embedded Chromium Engine)   │───▶│ PDF Output
│  └─────────────────────────────────┘    │
└─────────────────────────────────────────┘
```

---

## Installation and Setup

### Remove PDFreactor

```bash
# Remove PDFreactor NuGet packages
dotnet remove package PDFreactor.NET
dotnet remove package PDFreactor.Native.Windows.x64

# Stop PDFreactor server service (if running locally)
# Windows: net stop PDFreactor
# Linux: sudo systemctl stop pdfreactor
```

### Install IronPDF

```bash
dotnet add package IronPdf
```

### License Configuration

**PDFreactor (server-based):**
```csharp
// License configured on server via config file or command line
// Client connects to licensed server
var pdfReactor = new PDFreactor("http://pdfreactor-server:9423");
```

**IronPDF (application-level):**
```csharp
// One-time setup at application startup
IronPdf.License.LicenseKey = "YOUR-IRONPDF-LICENSE-KEY";

// Or via appsettings.json
{
  "IronPdf": {
    "LicenseKey": "YOUR-IRONPDF-LICENSE-KEY"
  }
}
```

---

## Core API Mappings

Comprehensive mapping tables and CSS Paged Media conversion strategies are available in the [full migration resource](https://ironpdf.com/blog/migration-guides/migrate-from-pdfreactor-to-ironpdf/).

### Classes and Objects

| PDFreactor | IronPDF | Notes |
|------------|---------|-------|
| `PDFreactor` | `ChromePdfRenderer` | Main conversion class |
| `Configuration` | `ChromePdfRenderOptions` | PDF settings |
| `Result` | `PdfDocument` | Output document |
| `Configuration.Document` | `RenderHtmlAsPdf(html)` | HTML input |
| `Result.Document` (byte[]) | `pdf.BinaryData` | Raw bytes |
| `Configuration.BaseURL` | `RenderingOptions.BaseUrl` | Base URL for resources |

### Configuration Properties

| PDFreactor Configuration | IronPDF RenderingOptions | Notes |
|-------------------------|-------------------------|-------|
| `config.Document = html` | `renderer.RenderHtmlAsPdf(html)` | HTML content |
| `config.Document = url` | `renderer.RenderUrlAsPdf(url)` | URL conversion |
| `config.BaseURL` | `RenderingOptions.BaseUrl` | Resource base path |
| `config.EnableJavaScript = true` | `RenderingOptions.EnableJavaScript = true` | JS execution |
| `config.JavaScriptSettings.Timeout` | `RenderingOptions.WaitFor.RenderDelay` | JS timeout |
| `config.PageFormat = PageFormat.A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Paper size |
| `config.PageOrientation = Orientation.LANDSCAPE` | `RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape` | Orientation |
| `config.PageMargins` | `RenderingOptions.MarginTop/Bottom/Left/Right` | Margins |
| `config.UserStyleSheets.Add()` | `RenderingOptions.CssMediaType` | CSS settings |
| `config.Encryption` | `pdf.SecuritySettings` | PDF security |
| `config.Title` | `pdf.MetaData.Title` | Metadata |
| `config.Author` | `pdf.MetaData.Author` | Metadata |
| `config.ColorSpace` | `RenderingOptions.GrayScale` | Color options |
| `config.ConformanceType` | `RenderingOptions.PdfA = true` | PDF/A compliance |
| `config.Cookies` | `RenderingOptions.CustomCookies` | HTTP cookies |

### HTTP/Connection Settings

| PDFreactor | IronPDF | Notes |
|------------|---------|-------|
| `new PDFreactor(serverUrl)` | N/A (in-process) | No server needed |
| `pdfReactor.ServiceUrl` | N/A | No service URL |
| `config.RequestTimeout` | `RenderingOptions.Timeout` | Render timeout |
| `config.HttpProxy` | `RenderingOptions.Proxy` | Proxy settings |
| `config.Authentication` | `RenderingOptions.HttpLogin*` | HTTP auth |

### CSS and Stylesheet Settings

| PDFreactor | IronPDF | Notes |
|------------|---------|-------|
| `config.AddUserStyleSheet(css)` | Embed in HTML or use `CustomCssUrl` | CSS injection |
| `config.AddUserScript(js)` | `RenderingOptions.Javascript` | JS injection |
| `config.MediaTypes` | `RenderingOptions.CssMediaType` | Screen/Print |
| `config.MergeMode` | CSS in HTML | Control via HTML/CSS |
| `config.DocumentDefaultLanguage` | HTML `lang` attribute | Document language |

### Advanced Conversion Options

| PDFreactor | IronPDF | Notes |
|------------|---------|-------|
| `config.AddAttachment()` | `pdf.Attachments.Add()` | PDF attachments |
| `config.EnableBookmarks = true` | `RenderingOptions.CreatePdfFormsFromHtml` | Bookmarks |
| `config.EnableLinks = true` | Automatic | Links always enabled |
| `config.EnableOverprint = true` | N/A (automatic) | Overprint handling |
| `config.FullCompression = true` | N/A (optimized) | Compression |
| `config.IntegrationMode` | N/A (direct) | Integration |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**PDFreactor:**
```csharp
using RealObjects.PDFreactor;
using System.IO;

var pdfReactor = new PDFreactor("http://localhost:9423");

var config = new Configuration
{
    Document = "<html><body><h1>Hello World</h1></body></html>"
};

Result result = pdfReactor.Convert(config);
File.WriteAllBytes("output.pdf", result.Document);
```

**IronPDF:**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Hello World</h1></body></html>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF

**PDFreactor:**
```csharp
using RealObjects.PDFreactor;
using System.IO;

var pdfReactor = new PDFreactor("http://localhost:9423");

var config = new Configuration
{
    Document = "https://www.example.com",
    EnableJavaScript = true
};

Result result = pdfReactor.Convert(config);
File.WriteAllBytes("webpage.pdf", result.Document);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;

var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Page Configuration

**PDFreactor:**
```csharp
using RealObjects.PDFreactor;

var config = new Configuration
{
    Document = htmlContent,
    PageFormat = PageFormat.LETTER,
    PageOrientation = Orientation.LANDSCAPE,
    PageMargins = new PageMargins
    {
        Top = "1in",
        Bottom = "1in",
        Left = "0.5in",
        Right = "0.5in"
    }
};

Result result = pdfReactor.Convert(config);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 25.4;      // 1 inch in mm
renderer.RenderingOptions.MarginBottom = 25.4;
renderer.RenderingOptions.MarginLeft = 12.7;    // 0.5 inch in mm
renderer.RenderingOptions.MarginRight = 12.7;

var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("document.pdf");
```

### Example 4: CSS Stylesheets

**PDFreactor:**
```csharp
using RealObjects.PDFreactor;

var config = new Configuration
{
    Document = htmlContent
};

// Add CSS Paged Media stylesheet
config.AddUserStyleSheet(
    "@page { size: A4 landscape; margin: 2cm; }" +
    "@page :first { margin-top: 5cm; }" +
    "body { font-family: Arial; }"
);

Result result = pdfReactor.Convert(config);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;
renderer.RenderingOptions.FirstPageNumber = 1;

// Embed CSS directly in HTML or use CustomCss
renderer.RenderingOptions.CustomCssUrl = "styles.css";

// Or inject CSS directly
string htmlWithCss = $@"
<html>
<head>
    <style>body {{ font-family: Arial; }}</style>
</head>
<body>{htmlContent}</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(htmlWithCss);
```

### Example 5: Headers and Footers

**PDFreactor (CSS Paged Media):**
```csharp
using RealObjects.PDFreactor;

var config = new Configuration
{
    Document = htmlContent
};

config.AddUserStyleSheet(@"
    @page {
        @top-center {
            content: 'Company Report';
            font-size: 10pt;
        }
        @bottom-left {
            content: 'Confidential';
        }
        @bottom-center {
            content: 'Page ' counter(page) ' of ' counter(pages);
        }
        @bottom-right {
            content: string(date);
        }
    }
");

Result result = pdfReactor.Convert(config);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Text-based headers/footers
renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    CenterText = "Company Report",
    FontSize = 10
};

renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    LeftText = "Confidential",
    CenterText = "Page {page} of {total-pages}",
    RightText = "{date}"
};

var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("report.pdf");

// OR use HTML headers/footers for more control
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center; font-size:10pt;'>Company Report</div>",
    DrawDividerLine = true
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width:100%; font-size:9pt;'>
            <span style='float:left;'>Confidential</span>
            <span style='float:right;'>{page} of {total-pages}</span>
        </div>"
};
```

### Example 6: JavaScript Execution

**PDFreactor:**
```csharp
using RealObjects.PDFreactor;

var config = new Configuration
{
    Document = htmlWithJs,
    EnableJavaScript = true,
    JavaScriptSettings = new JavaScriptSettings
    {
        Timeout = 30000,  // 30 seconds
        WaitForDocumentReady = true
    }
};

Result result = pdfReactor.Convert(config);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.RenderDelay = 3000;  // Wait 3 seconds

// Or wait for specific element
renderer.RenderingOptions.WaitFor.HtmlElementExists = "#chart-loaded";

// Or wait for JavaScript function
renderer.RenderingOptions.WaitFor.JavaScript = "window.chartReady === true";

var pdf = renderer.RenderHtmlAsPdf(htmlWithJs);
```

### Example 7: Async/Concurrent Conversions

**PDFreactor:**
```csharp
using RealObjects.PDFreactor;

public async Task<byte[]> ConvertAsync(string html)
{
    var pdfReactor = new PDFreactor("http://localhost:9423");

    var config = new Configuration { Document = html };

    // PDFreactor async via async client
    var result = await pdfReactor.ConvertAsync(config);
    return result.Document;
}
```

**IronPDF:**
```csharp
using IronPdf;

public async Task<byte[]> ConvertAsync(string html)
{
    var renderer = new ChromePdfRenderer();

    // IronPDF supports Task.Run for async
    var pdf = await Task.Run(() => renderer.RenderHtmlAsPdf(html));
    return pdf.BinaryData;
}

// Concurrent conversions
public async Task<List<byte[]>> ConvertManyAsync(List<string> htmlList)
{
    var tasks = htmlList.Select(html => Task.Run(() =>
    {
        var renderer = new ChromePdfRenderer();
        return renderer.RenderHtmlAsPdf(html).BinaryData;
    }));

    var results = await Task.WhenAll(tasks);
    return results.ToList();
}
```

### Example 8: PDF Metadata and Security

**PDFreactor:**
```csharp
using RealObjects.PDFreactor;

var config = new Configuration
{
    Document = htmlContent,
    Title = "Financial Report",
    Author = "Accounting Team",
    Subject = "Q4 Results",
    Encryption = new Encryption
    {
        UserPassword = "readpassword",
        OwnerPassword = "adminpassword",
        AllowPrint = true,
        AllowCopy = false
    }
};

Result result = pdfReactor.Convert(config);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);

// Set metadata
pdf.MetaData.Title = "Financial Report";
pdf.MetaData.Author = "Accounting Team";
pdf.MetaData.Subject = "Q4 Results";

// Set security
pdf.SecuritySettings.UserPassword = "readpassword";
pdf.SecuritySettings.OwnerPassword = "adminpassword";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;

pdf.SaveAs("secure-report.pdf");
```

### Example 9: Watermarks

**PDFreactor (CSS-based):**
```csharp
using RealObjects.PDFreactor;

var config = new Configuration
{
    Document = htmlContent
};

config.AddUserStyleSheet(@"
    @page {
        background-image: url('watermark.png');
        background-position: center;
        background-repeat: no-repeat;
        background-size: 50%;
    }
");

Result result = pdfReactor.Convert(config);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);

// Apply HTML watermark
pdf.ApplyWatermark("<div style='color:red; font-size:48pt; opacity:0.3; transform:rotate(-45deg);'>CONFIDENTIAL</div>",
    rotation: 0,
    opacity: 50);

// Or stamp image
pdf.ApplyStamp(new ImageStamper("watermark.png")
{
    Opacity = 30,
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Middle
});

pdf.SaveAs("watermarked.pdf");
```

### Example 10: Cookies and Authentication

**PDFreactor:**
```csharp
using RealObjects.PDFreactor;

var config = new Configuration
{
    Document = "https://secure.example.com/report",
    Cookies = new List<Cookie>
    {
        new Cookie { Name = "session", Value = "abc123", Domain = "example.com" }
    },
    Authentication = new Authentication
    {
        Username = "user",
        Password = "pass",
        Type = AuthenticationType.BASIC
    }
};

Result result = pdfReactor.Convert(config);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Add cookies
renderer.RenderingOptions.CustomCookies = new Dictionary<string, string>
{
    { "session", "abc123" }
};

// HTTP Basic Authentication
renderer.RenderingOptions.HttpLoginCredentials = new HttpLoginCredentials
{
    NetworkUsername = "user",
    NetworkPassword = "pass"
};

var pdf = renderer.RenderUrlAsPdf("https://secure.example.com/report");
pdf.SaveAs("authenticated-report.pdf");
```

---

## Configuration Migration

### Page Format Mapping

| PDFreactor PageFormat | IronPDF PdfPaperSize |
|----------------------|---------------------|
| `PageFormat.A0` | `PdfPaperSize.A0` |
| `PageFormat.A1` | `PdfPaperSize.A1` |
| `PageFormat.A2` | `PdfPaperSize.A2` |
| `PageFormat.A3` | `PdfPaperSize.A3` |
| `PageFormat.A4` | `PdfPaperSize.A4` |
| `PageFormat.A5` | `PdfPaperSize.A5` |
| `PageFormat.A6` | `PdfPaperSize.A6` |
| `PageFormat.B4` | `PdfPaperSize.B4` |
| `PageFormat.B5` | `PdfPaperSize.B5` |
| `PageFormat.LETTER` | `PdfPaperSize.Letter` |
| `PageFormat.LEGAL` | `PdfPaperSize.Legal` |
| `PageFormat.TABLOID` | `PdfPaperSize.Tabloid` |
| `PageFormat.EXECUTIVE` | `PdfPaperSize.Executive` |
| Custom dimensions | `PdfPaperSize.Custom(width, height)` |

### Orientation Mapping

| PDFreactor | IronPDF |
|------------|---------|
| `Orientation.PORTRAIT` | `PdfPaperOrientation.Portrait` |
| `Orientation.LANDSCAPE` | `PdfPaperOrientation.Landscape` |

### Encryption Mapping

| PDFreactor Encryption | IronPDF SecuritySettings |
|-----------------------|-------------------------|
| `UserPassword` | `UserPassword` |
| `OwnerPassword` | `OwnerPassword` |
| `AllowPrint` | `AllowUserPrinting` |
| `AllowCopy` | `AllowUserCopyPasteContent` |
| `AllowModify` | `AllowUserEdits` |
| `AllowAnnotations` | `AllowUserAnnotations` |
| `AllowForms` | `AllowUserFormData` |
| `AllowExtraction` | `AllowUserCopyPasteContentForAccessibility` |
| `EncryptionType.TYPE_128` | 128-bit (default) |
| `EncryptionType.TYPE_256` | 256-bit encryption |

### Color Space Mapping

| PDFreactor | IronPDF |
|------------|---------|
| `ColorSpace.RGB` | Default (RGB) |
| `ColorSpace.CMYK` | CMYK support |
| `ColorSpace.GRAYSCALE` | `RenderingOptions.GrayScale = true` |

---

## CSS Paged Media Migration

PDFreactor's strength is CSS Paged Media support. IronPDF handles most scenarios through its API or inline CSS:

### Page Size and Margins

**PDFreactor CSS:**
```css
@page {
    size: A4 portrait;
    margin: 2cm 1.5cm;
}

@page :first {
    margin-top: 5cm;
}

@page :left {
    margin-left: 2cm;
    margin-right: 1cm;
}

@page :right {
    margin-left: 1cm;
    margin-right: 2cm;
}
```

**IronPDF Equivalent:**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 15;
renderer.RenderingOptions.MarginRight = 15;

// For first page different margin, use HTML approach
var pdf = renderer.RenderHtmlAsPdf($@"
<html>
<head>
<style>
    @page {{ margin: 20mm 15mm; }}
    @page :first {{ margin-top: 50mm; }}
</style>
</head>
<body>{content}</body>
</html>");
```

### Running Headers/Footers (CSS counters)

**PDFreactor CSS:**
```css
@page {
    @top-center { content: element(header); }
    @bottom-center { content: "Page " counter(page) " of " counter(pages); }
}

.header { position: running(header); }
```

**IronPDF Equivalent:**
```csharp
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
};
```

### Page Breaks

**PDFreactor CSS:**
```css
h1 { page-break-before: always; }
.no-break { page-break-inside: avoid; }
table { page-break-after: always; }
```

**IronPDF (same CSS works):**
```csharp
string htmlWithPageBreaks = @"
<html>
<head>
<style>
    h1 { page-break-before: always; }
    .no-break { page-break-inside: avoid; }
    .page-break { page-break-after: always; }
</style>
</head>
<body>
    <h1>Chapter 1</h1>
    <div class='no-break'>Keep this together</div>
    <div class='page-break'>New page after this</div>
</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(htmlWithPageBreaks);
```

---

## Headers and Footers

### PDFreactor CSS-Based vs IronPDF API

**PDFreactor:**
```css
@page {
    @top-left { content: "Left Header"; }
    @top-center { content: "Center Header"; }
    @top-right { content: "Right Header"; }
    @bottom-left { content: "Left Footer"; }
    @bottom-center { content: "Page " counter(page) " of " counter(pages); }
    @bottom-right { content: "Right Footer"; }
}
```

**IronPDF Text Headers/Footers:**
```csharp
renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    LeftText = "Left Header",
    CenterText = "Center Header",
    RightText = "Right Header",
    FontSize = 10,
    FontFamily = "Arial"
};

renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    LeftText = "Left Footer",
    CenterText = "Page {page} of {total-pages}",
    RightText = "Right Footer"
};
```

**IronPDF HTML Headers/Footers (more flexible):**
```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width:100%; font-size:10pt; border-bottom:1px solid #ccc; padding-bottom:5px;'>
            <span style='float:left;'>Left Header</span>
            <span style='float:right;'>Right Header</span>
            <span style='text-align:center; display:block;'>Center Header</span>
        </div>",
    DrawDividerLine = false,
    MaxHeight = 30
};
```

### Header/Footer Placeholder Mapping

| PDFreactor CSS | IronPDF Placeholder |
|----------------|---------------------|
| `counter(page)` | `{page}` |
| `counter(pages)` | `{total-pages}` |
| `string(title)` | `{html-title}` |
| `string(date)` | `{date}` |
| `string(time)` | `{time}` |
| `string(url)` | `{url}` |

---

## JavaScript and Async Content

### Handling Dynamic Content

**PDFreactor:**
```csharp
var config = new Configuration
{
    Document = htmlWithCharts,
    EnableJavaScript = true,
    JavaScriptSettings = new JavaScriptSettings
    {
        Timeout = 30000,
        WaitForDocumentReady = true,
        WaitOnBeforeUnload = true
    }
};
```

**IronPDF:**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;

// Wait for render delay (simple)
renderer.RenderingOptions.WaitFor.RenderDelay = 5000;

// Wait for specific element to appear
renderer.RenderingOptions.WaitFor.HtmlElementExists = "#chart-container svg";

// Wait for JavaScript condition
renderer.RenderingOptions.WaitFor.JavaScript = "window.chartsLoaded === true";

// Wait for network idle (no pending requests)
renderer.RenderingOptions.WaitFor.NetworkIdle0 = 500;  // Wait 500ms after last network request

var pdf = renderer.RenderHtmlAsPdf(htmlWithCharts);
```

### Ajax/Fetch Content

**PDFreactor:**
```csharp
var config = new Configuration
{
    Document = htmlWithAjax,
    EnableJavaScript = true,
    JavaScriptSettings = new JavaScriptSettings
    {
        WaitForDocumentReady = true
    }
};
```

**IronPDF:**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;

// Wait for all network activity to complete
renderer.RenderingOptions.WaitFor.NetworkIdle0 = 1000;

// Or use custom JavaScript condition
renderer.RenderingOptions.WaitFor.JavaScript = @"
    (function() {
        var pendingRequests = window.pendingAjaxRequests || 0;
        return pendingRequests === 0 && document.readyState === 'complete';
    })()
";

var pdf = renderer.RenderHtmlAsPdf(htmlWithAjax);
```

---

## Server vs Library Architecture

### Eliminating Server Dependency

**PDFreactor Server Architecture:**
```csharp
// Requires PDFreactor server running
// Configuration in pdfreactor.xml or command line
// http://localhost:9423/service/status

public class PdfReactorService
{
    private readonly string _serverUrl;

    public PdfReactorService(string serverUrl)
    {
        _serverUrl = serverUrl;
    }

    public byte[] Convert(string html)
    {
        var pdfReactor = new PDFreactor(_serverUrl);
        var config = new Configuration { Document = html };
        var result = pdfReactor.Convert(config);
        return result.Document;
    }
}
```

**IronPDF In-Process:**
```csharp
// No server required - runs in-process
public class PdfService
{
    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    }

    public byte[] Convert(string html)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Deployment Comparison

**PDFreactor Deployment:**
```dockerfile
# Requires Java base image
FROM openjdk:11-jre-slim

# Install PDFreactor
COPY pdfreactor-server.jar /opt/pdfreactor/
COPY license.key /opt/pdfreactor/

# Start server
EXPOSE 9423
CMD ["java", "-jar", "/opt/pdfreactor/pdfreactor-server.jar"]
```

**IronPDF Deployment:**
```dockerfile
# Standard .NET image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Copy application (IronPDF included via NuGet)
COPY ./publish /app
WORKDIR /app

# Linux dependencies for Chromium
RUN apt-get update && apt-get install -y \
    libglib2.0-0 libnss3 libatk1.0-0 libatk-bridge2.0-0 \
    libcups2 libdrm2 libxkbcommon0 libxcomposite1 \
    libxdamage1 libxrandr2 libgbm1 libasound2

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

---

## Performance Optimization

### IronPDF Performance Tips

```csharp
// Reuse renderer instance for c# html to pdf
public class OptimizedPdfService
{
    private readonly ChromePdfRenderer _renderer;

    public OptimizedPdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Configure once, reuse many times
        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    }

    public byte[] Convert(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}

// Parallel processing
public async Task<List<byte[]>> ConvertBatchAsync(List<string> htmlList)
{
    var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
    var results = new ConcurrentBag<byte[]>();

    Parallel.ForEach(htmlList, options, html =>
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        results.Add(pdf.BinaryData);
    });

    return results.ToList();
}
```

### Memory Management

```csharp
// Dispose large PDFs
using (var pdf = renderer.RenderHtmlAsPdf(largeHtml))
{
    pdf.SaveAs("output.pdf");
}  // PDF disposed here

// Stream output for large files
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
using (var stream = new MemoryStream(pdf.BinaryData))
{
    await stream.CopyToAsync(responseStream);
}
```

---

## Troubleshooting

### Common Migration Issues

#### 1. CSS Rendering Differences

**Problem:** PDFreactor CSS Paged Media renders differently than IronPDF.

**Solution:**
```csharp
// Use print media type for consistent rendering
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

// Inject CSS to match PDFreactor behavior
renderer.RenderingOptions.CustomCssUrl = "print-styles.css";
```

#### 2. JavaScript Timing Issues

**Problem:** Dynamic content not rendered.

**Solution:**
```csharp
// Wait for specific condition
renderer.RenderingOptions.WaitFor.JavaScript = "window.loaded === true";

// Or use delay
renderer.RenderingOptions.WaitFor.RenderDelay = 5000;  // 5 seconds
```

#### 3. Font Rendering

**Problem:** Fonts look different from PDFreactor.

**Solution:**
```csharp
// Ensure fonts are available
// Option 1: Use web fonts
string html = @"
<html>
<head>
    <link href='https://fonts.googleapis.com/css?family=Roboto' rel='stylesheet'>
    <style>body { font-family: 'Roboto', sans-serif; }</style>
</head>
<body>Content</body>
</html>";

// Option 2: Embed fonts
renderer.RenderingOptions.CustomCssUrl = "embedded-fonts.css";
```

#### 4. Page Breaks

**Problem:** Page breaks differ from PDFreactor.

**Solution:**
```csharp
// Use CSS page-break properties in your HTML
string html = @"
<style>
    .page-break { page-break-after: always; }
    .no-break { page-break-inside: avoid; }
    h1 { page-break-before: always; }
</style>";
```

#### 5. Running Headers with Variables

**Problem:** PDFreactor's `string()` CSS function for running content.

**Solution:**
```csharp
// Use IronPDF's placeholder system
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<span>{html-title}</span>",
    DrawDividerLine = true
};

// Available placeholders: {page}, {total-pages}, {date}, {time}, {html-title}, {url}
```

#### 6. Server Connection Errors Eliminated

**Problem:** PDFreactor server unavailable errors.

**Solution:** IronPDF runs in-process—no server connection needed!
```csharp
// PDFreactor required server connection handling:
// try { pdfReactor.Connect(); } catch (ServiceUnavailableException) { ... }

// IronPDF: Just use it directly
var pdf = renderer.RenderHtmlAsPdf(html);
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PDFreactor API calls in codebase**
  ```bash
  grep -r "using com.realobjects.pdfreactor" --include="*.cs" .
  grep -r "PDFreactor\|Configuration" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document custom CSS Paged Media stylesheets**
  ```csharp
  // Example of CSS Paged Media
  @page {
      size: A4;
      margin: 20mm;
  }
  ```
  **Why:** These styles need to be translated to IronPDF's rendering options for consistent output.

- [ ] **List all PDFreactor configuration settings used**
  ```csharp
  // Example configuration
  Configuration config = new Configuration();
  config.setJavaScriptEnabled(true);
  config.setAuthor("Author Name");
  ```
  **Why:** Documenting these settings helps map them to IronPDF's RenderingOptions.

- [ ] **Identify server infrastructure dependencies**
  **Why:** PDFreactor requires a server setup, which will be decommissioned in favor of IronPDF's in-process library.

- [ ] **Test sample PDFs for visual comparison baseline**
  **Why:** Establish a baseline to compare PDF outputs before and after migration for quality assurance.

- [ ] **Review PDFreactor license terms for migration timeline**
  **Why:** Ensure compliance with existing licenses and plan for a smooth transition to IronPDF.

### Code Migration

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to the project to begin migration.

- [ ] **Remove PDFreactor packages and server references**
  ```bash
  dotnet remove package PDFreactor
  ```
  **Why:** Clean up dependencies and references to switch fully to IronPDF.

- [ ] **Replace `PDFreactor` class with `ChromePdfRenderer`**
  ```csharp
  // Before (PDFreactor)
  PDFreactor reactor = new PDFreactor();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses ChromePdfRenderer for rendering HTML/CSS to PDF.

- [ ] **Convert `Configuration` objects to `RenderingOptions`**
  ```csharp
  // Before (PDFreactor)
  Configuration config = new Configuration();
  config.setPageSize("A4");

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** IronPDF's RenderingOptions provides a unified way to set page configurations.

- [ ] **Update header/footer implementation**
  ```csharp
  // Before (PDFreactor)
  config.setHeader("<header>Page [page] of [total]</header>");

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with dynamic placeholders.

- [ ] **Migrate CSS Paged Media rules to IronPDF equivalents**
  **Why:** Ensure that CSS styles are accurately represented in the IronPDF output.

- [ ] **Convert async patterns if applicable**
  ```csharp
  // Before (PDFreactor)
  Task<PDFreactorResult> result = reactor.convertAsync(config);

  // After (IronPDF)
  var pdf = await renderer.RenderHtmlAsPdfAsync(html);
  ```
  **Why:** IronPDF supports async operations for non-blocking PDF generation.

- [ ] **Update error handling (Result checking → try/catch)**
  ```csharp
  // Before (PDFreactor)
  if (result.hasErrors()) { /* handle errors */ }

  // After (IronPDF)
  try
  {
      var pdf = renderer.RenderHtmlAsPdf(html);
  }
  catch (Exception ex)
  {
      // handle exceptions
  }
  ```
  **Why:** IronPDF uses exceptions for error handling, aligning with .NET practices.

### Configuration Migration

- [ ] **Page size and orientation settings**
  ```csharp
  // Before (PDFreactor)
  config.setPageSize("A4");
  config.setOrientation(Configuration.ORIENTATION_LANDSCAPE);

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  ```
  **Why:** Consistent page settings ensure the PDF layout remains unchanged.

- [ ] **Margin configurations (convert units to mm)**
  ```csharp
  // Before (PDFreactor)
  config.setMarginTop(20);

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 20;
  ```
  **Why:** IronPDF uses millimeters for margin settings, aligning with standard PDF practices.

- [ ] **JavaScript execution settings**
  ```csharp
  // Before (PDFreactor)
  config.setJavaScriptEnabled(true);

  // After (IronPDF)
  renderer.RenderingOptions.EnableJavaScript = true;
  ```
  **Why:** Enable JavaScript for dynamic content rendering.

- [ ] **PDF metadata properties**
  ```csharp
  // Before (PDFreactor)
  config.setAuthor("Author Name");

  // After (IronPDF)
  pdf.MetaData.Author = "Author Name";
  ```
  **Why:** Set PDF metadata for document information consistency.

- [ ] **Security and encryption settings**
  ```csharp
  // Before (PDFreactor)
  config.setEncryption(new Encryption("password"));

  // After (IronPDF)
  pdf.SecuritySettings.UserPassword = "password";
  ```
  **Why:** Secure PDFs with passwords and permissions.

- [ ] **Authentication and cookie handling**
  **Why:** Ensure that any authenticated content is accessible during PDF generation.

### Infrastructure Migration

- [ ] **Remove Java runtime requirement**
  **Why:** Simplifies deployment by eliminating Java dependencies.

- [ ] **Decommission PDFreactor server**
  **Why:** Reduce infrastructure complexity and costs.

- [ ] **Update Docker/deployment configurations**
  **Why:** Ensure that the deployment pipeline supports IronPDF's requirements.

- [ ] **Configure Linux dependencies for IronPDF Chromium**
  **Why:** IronPDF requires specific libraries on Linux for Chromium rendering.

- [ ] **Update CI/CD pipelines**
  **Why:** Ensure build and deployment processes are optimized for IronPDF.

### Testing

- [ ] **Visual comparison of converted PDFs**
  **Why:** Verify that the visual output matches expectations.

- [ ] **Header/footer rendering verification**
  **Why:** Ensure headers and footers are correctly rendered with IronPDF.

- [ ] **JavaScript execution testing**
  **Why:** Confirm that JavaScript content is rendered as expected.

- [ ] **Performance benchmarking**
  **Why:** Measure performance improvements or regressions.

- [ ] **Load testing for high-volume scenarios**
  **Why:** Ensure IronPDF can handle production loads.

- [ ] **Cross-platform testing (Windows, Linux, macOS)**
  **Why:** Verify consistent behavior across different operating systems.

### Post-Migration

- [ ] **Monitor PDF generation metrics**
  **Why:** Track performance and identify potential issues.

- [ ] **Compare output file sizes**
  **Why:** Ensure file sizes are within acceptable limits.

- [ ] **Verify all edge cases handled**
  **Why:** Confirm that all scenarios are correctly managed.

- [ ] **Update documentation**
  **Why:** Provide accurate information for future maintenance and development.

- [ ] **Train team on IronPDF API**
  **Why:** Ensure the team is proficient with the new library.

- [ ] **Remove PDFreactor infrastructure**
  **Why:** Complete the migration by decommissioning old systems.
---

## Quick Reference Card

### PDFreactor to IronPDF Cheat Sheet

```csharp
// ========== BEFORE (PDFreactor) ==========
using RealObjects.PDFreactor;

var pdfReactor = new PDFreactor("http://server:9423");
var config = new Configuration
{
    Document = html,
    PageFormat = PageFormat.A4,
    PageOrientation = Orientation.LANDSCAPE,
    EnableJavaScript = true
};
config.AddUserStyleSheet("@page { @bottom-center { content: 'Page ' counter(page); } }");
var result = pdfReactor.Convert(config);
File.WriteAllBytes("output.pdf", result.Document);

// ========== AFTER (IronPDF) ==========
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-KEY";
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.TextFooter = new TextHeaderFooter { CenterText = "Page {page}" };
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

---

## Summary

Migrating from PDFreactor to IronPDF eliminates Java dependencies and server infrastructure while providing equivalent HTML-to-PDF capabilities. Key benefits include:

1. **Simplified Architecture**: No separate server process required
2. **Native .NET Integration**: Direct API calls instead of REST/IPC
3. **Reduced Complexity**: Single NuGet package vs. Java runtime + server
4. **Full PDF Lifecycle**: Beyond conversion—merge, split, edit, sign
5. **Cross-Platform**: Windows, Linux, macOS support
6. **Modern API**: Clean, intuitive .NET patterns

The migration typically involves replacing Configuration objects with RenderingOptions, converting CSS Paged Media rules to IronPDF's header/footer API, and removing server infrastructure. Most conversions are straightforward one-to-one mappings.

For technical support during migration, contact IronPDF support or consult the [official documentation](https://ironpdf.com/docs/).
