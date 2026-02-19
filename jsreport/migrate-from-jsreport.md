# How Do I Migrate from jsreport to IronPDF in C#?

## Table of Contents
1. [Why Migrate from jsreport](#why-migrate-from-jsreport)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from jsreport

### The jsreport Challenges

jsreport introduces complexity that doesn't belong in a pure .NET environment:

1. **Node.js Dependency**: Requires Node.js runtime and binaries, adding infrastructure complexity
2. **External Binary Management**: Must download and manage platform-specific binaries (Windows, Linux, OSX)
3. **Separate Server Process**: Either runs as utility or web server—additional process management needed
4. **JavaScript Templating**: Forces learning Handlebars, JsRender, or other JS templating systems
5. **Complex Request Structure**: Verbose `RenderRequest` with nested `Template` objects
6. **Licensing Limitations**: Free tier limits template count; scaling requires commercial license
7. **Stream-Based Output**: Returns streams requiring manual file operations

### The IronPDF Advantage

| Feature | jsreport | IronPDF |
|---------|----------|---------|
| Runtime | Node.js + .NET | Pure .NET |
| Binary Management | Manual (jsreport.Binary packages) | Automatic |
| Server Process | Required (utility or web server) | In-process |
| Templating | JavaScript (Handlebars, etc.) | C# (Razor, string interpolation) |
| API Style | Verbose request objects | Clean fluent methods |
| Output | Stream | PdfDocument object |
| PDF Manipulation | Limited | Extensive (merge, split, edit) |
| Async Support | Primary | Both sync and async |

### Migration Benefits

- **Eliminate Node.js**: No more managing Node.js binaries or versions
- **Simpler API**: Replace 20+ lines with 3-5 lines of code
- **Native C#**: Use C# templating instead of JavaScript
- **In-Process**: No server management or startup delays
- **Rich PDF Operations**: Full merge, split, watermark, form, and security support
- **Smaller Footprint**: Single NuGet package, no platform-specific binaries

Each of these benefits is backed by complete, runnable examples in the [comprehensive migration walkthrough](https://ironpdf.com/blog/migration-guides/migrate-from-jsreport-to-ironpdf/).

---

## Before You Start

### Prerequisites

1. **.NET Environment**: .NET Framework 4.6.2+ or .NET Core 3.1+ / .NET 5+
2. **NuGet Access**: Ability to install NuGet packages
3. **IronPDF License**: Free trial or purchased license key

### Installation

```bash
# Remove jsreport packages
dotnet remove package jsreport.Binary
dotnet remove package jsreport.Binary.Linux
dotnet remove package jsreport.Binary.OSX
dotnet remove package jsreport.Local
dotnet remove package jsreport.Types
dotnet remove package jsreport.Client

# Install IronPDF
dotnet add package IronPdf
```

### License Configuration

```csharp
// Add at application startup (Program.cs or Startup.cs)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Identify jsreport Usage

```bash
# Find all jsreport references
grep -r "using jsreport\|LocalReporting\|RenderRequest\|RenderAsync" --include="*.cs" .
grep -r "JsReportBinary\|Template\|Recipe\|Engine\." --include="*.cs" .
```

---

## Quick Start Migration

### Minimal Change Example

**Before (jsreport):**
```csharp
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;

public class JsReportPdfService
{
    private readonly ILocalUtilityReportingService _rs;

    public JsReportPdfService()
    {
        _rs = new LocalReporting()
            .UseBinary(JsReportBinary.GetBinary())
            .AsUtility()
            .Create();
    }

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        var report = await _rs.RenderAsync(new RenderRequest
        {
            Template = new Template
            {
                Recipe = Recipe.ChromePdf,
                Engine = Engine.None,
                Content = html
            }
        });

        using (var memoryStream = new MemoryStream())
        {
            await report.Content.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
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
    }

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }

    // Async version if needed
    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}
```

---

## Complete API Reference

### Namespace Mappings

| jsreport Namespace | IronPDF Equivalent |
|-------------------|-------------------|
| `jsreport.Local` | `IronPdf` |
| `jsreport.Types` | `IronPdf` |
| `jsreport.Binary` | _(not needed)_ |
| `jsreport.Client` | _(not needed)_ |

### Class Mappings

| jsreport Class | IronPDF Equivalent | Notes |
|---------------|-------------------|-------|
| `LocalReporting` | `ChromePdfRenderer` | Main renderer |
| `ReportingService` | `ChromePdfRenderer` | Same class |
| `RenderRequest` | Method parameters | No wrapper needed |
| `Template` | Method parameters | No wrapper needed |
| `Chrome` | `RenderingOptions` | Chrome options |
| `Report` | `PdfDocument` | Result object |
| `Engine` | _(not needed)_ | C# for templating |

### Method Mappings

| jsreport Method | IronPDF Equivalent | Notes |
|----------------|-------------------|-------|
| `LocalReporting().UseBinary().AsUtility().Create()` | `new ChromePdfRenderer()` | One-liner |
| `LocalReporting().UseBinary().AsWebServer().Create()` | `new ChromePdfRenderer()` | No server needed |
| `rs.RenderAsync(request)` | `renderer.RenderHtmlAsPdf(html)` | Direct call |
| `rs.StartAsync()` | _(not needed)_ | In-process |
| `rs.KillAsync()` | _(not needed)_ | Auto-cleanup |
| `report.Content.CopyTo(stream)` | `pdf.SaveAs(path)` or `pdf.BinaryData` | Direct access |

### RenderRequest Property Mappings

| jsreport Template Property | IronPDF Equivalent | Notes |
|---------------------------|-------------------|-------|
| `Template.Content` | First parameter to `RenderHtmlAsPdf()` | Direct HTML string |
| `Template.Recipe` | _(not needed)_ | Always ChromePdf |
| `Template.Engine` | _(not needed)_ | Use C# templating |
| `Template.Chrome.HeaderTemplate` | `RenderingOptions.HtmlHeader` | HTML headers |
| `Template.Chrome.FooterTemplate` | `RenderingOptions.HtmlFooter` | HTML footers |
| `Template.Chrome.DisplayHeaderFooter` | _(automatic)_ | Headers auto-enabled |
| `Template.Chrome.MarginTop` | `RenderingOptions.MarginTop` | In millimeters |
| `Template.Chrome.MarginBottom` | `RenderingOptions.MarginBottom` | In millimeters |
| `Template.Chrome.MarginLeft` | `RenderingOptions.MarginLeft` | In millimeters |
| `Template.Chrome.MarginRight` | `RenderingOptions.MarginRight` | In millimeters |
| `Template.Chrome.Format` | `RenderingOptions.PaperSize` | Enum value |
| `Template.Chrome.Landscape` | `RenderingOptions.PaperOrientation` | Enum value |
| `Template.Chrome.MediaType` | `RenderingOptions.CssMediaType` | Screen or Print |
| `Template.Chrome.WaitForNetworkIdle` | `RenderingOptions.WaitFor.NetworkIdle()` | Wait strategy |
| `Template.Chrome.WaitForJS` | `RenderingOptions.WaitFor.JavaScript()` | Wait for JS |
| `Template.Chrome.PrintBackground` | `RenderingOptions.PrintHtmlBackgrounds` | Background printing |
| `Template.Chrome.Scale` | `RenderingOptions.Zoom` | Zoom percentage |
| `Template.Chrome.Url` | `renderer.RenderUrlAsPdf(url)` | Separate method |

### Placeholder Mappings (Headers/Footers)

| jsreport Placeholder | IronPDF Placeholder | Notes |
|---------------------|-------------------|-------|
| `{#pageNum}` | `{page}` | Current page |
| `{#numPages}` | `{total-pages}` | Total pages |
| `{#timestamp}` | `{date}` | Current date |
| `{#title}` | `{html-title}` | Document title |
| `{#url}` | `{url}` | Document URL |

### Paper Size Mappings

| jsreport Format | IronPDF PaperSize |
|-----------------|-------------------|
| `"A4"` | `PdfPaperSize.A4` |
| `"Letter"` | `PdfPaperSize.Letter` |
| `"Legal"` | `PdfPaperSize.Legal` |
| `"A3"` | `PdfPaperSize.A3` |
| `"A5"` | `PdfPaperSize.A5` |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (jsreport):**
```csharp
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;

public async Task<byte[]> ConvertHtmlToPdf(string html)
{
    var rs = new LocalReporting()
        .UseBinary(JsReportBinary.GetBinary())
        .AsUtility()
        .Create();

    var report = await rs.RenderAsync(new RenderRequest
    {
        Template = new Template
        {
            Recipe = Recipe.ChromePdf,
            Engine = Engine.None,
            Content = html
        }
    });

    using (var ms = new MemoryStream())
    {
        await report.Content.CopyToAsync(ms);
        return ms.ToArray();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertHtmlToPdf(string html)
{
    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Example 2: URL to PDF with Options

**Before (jsreport):**
```csharp
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;

public async Task<byte[]> ConvertUrlToPdf(string url)
{
    var rs = new LocalReporting()
        .UseBinary(JsReportBinary.GetBinary())
        .AsUtility()
        .Create();

    var report = await rs.RenderAsync(new RenderRequest
    {
        Template = new Template
        {
            Recipe = Recipe.ChromePdf,
            Engine = Engine.None,
            Content = "",  // URL mode requires empty or basic HTML
            Chrome = new Chrome
            {
                Url = url,
                WaitForNetworkIdle = true,
                Format = "A4",
                Landscape = true,
                MediaType = MediaType.Print
            }
        }
    });

    using (var ms = new MemoryStream())
    {
        await report.Content.CopyToAsync(ms);
        return ms.ToArray();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertUrlToPdf(string url)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
    renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
    renderer.RenderingOptions.WaitFor.NetworkIdle();

    return renderer.RenderUrlAsPdf(url).BinaryData;
}
```

### Example 3: Headers and Footers

**Before (jsreport):**
```csharp
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;

public async Task<byte[]> CreateReportWithHeaderFooter(string html)
{
    var rs = new LocalReporting()
        .UseBinary(JsReportBinary.GetBinary())
        .AsUtility()
        .Create();

    var report = await rs.RenderAsync(new RenderRequest
    {
        Template = new Template
        {
            Recipe = Recipe.ChromePdf,
            Engine = Engine.None,
            Content = html,
            Chrome = new Chrome
            {
                DisplayHeaderFooter = true,
                HeaderTemplate = @"
                    <div style='font-size:10px; text-align:center; width:100%;'>
                        Company Report - Confidential
                    </div>",
                FooterTemplate = @"
                    <div style='font-size:10px; width:100%;'>
                        <span style='float:left;'>Printed: {#timestamp}</span>
                        <span style='float:right;'>Page {#pageNum} of {#numPages}</span>
                    </div>",
                MarginTop = "2cm",
                MarginBottom = "2cm"
            }
        }
    });

    using (var ms = new MemoryStream())
    {
        await report.Content.CopyToAsync(ms);
        return ms.ToArray();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateReportWithHeaderFooter(string html)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = @"
            <div style='font-size:10px; text-align:center; width:100%;'>
                Company Report - Confidential
            </div>",
        MaxHeight = 20
    };

    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = @"
            <div style='font-size:10px; width:100%;'>
                <span style='float:left;'>Printed: {date}</span>
                <span style='float:right;'>Page {page} of {total-pages}</span>
            </div>",
        MaxHeight = 20
    };

    renderer.RenderingOptions.MarginTop = 20;  // mm (2cm ≈ 20mm)
    renderer.RenderingOptions.MarginBottom = 20;

    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Example 4: Handlebars Template to C# Razor

**Before (jsreport with Handlebars):**
```csharp
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;

public async Task<byte[]> CreateInvoice(Invoice invoice)
{
    var rs = new LocalReporting()
        .UseBinary(JsReportBinary.GetBinary())
        .AsUtility()
        .Create();

    var report = await rs.RenderAsync(new RenderRequest
    {
        Template = new Template
        {
            Recipe = Recipe.ChromePdf,
            Engine = Engine.Handlebars,
            Content = @"
                <html>
                <body>
                    <h1>Invoice #{{number}}</h1>
                    <p>Customer: {{customerName}}</p>
                    <table>
                        <tr><th>Item</th><th>Qty</th><th>Price</th></tr>
                        {{#each items}}
                        <tr>
                            <td>{{name}}</td>
                            <td>{{quantity}}</td>
                            <td>{{price}}</td>
                        </tr>
                        {{/each}}
                    </table>
                    <p><strong>Total: {{total}}</strong></p>
                </body>
                </html>"
        },
        Data = invoice  // Handlebars data binding
    });

    using (var ms = new MemoryStream())
    {
        await report.Content.CopyToAsync(ms);
        return ms.ToArray();
    }
}
```

**After (IronPDF with C# string interpolation):**
```csharp
using IronPdf;

public byte[] CreateInvoice(Invoice invoice)
{
    string itemRows = string.Join("", invoice.Items.Select(item => $@"
        <tr>
            <td>{item.Name}</td>
            <td>{item.Quantity}</td>
            <td>{item.Price:C}</td>
        </tr>"));

    string html = $@"
        <html>
        <body>
            <h1>Invoice #{invoice.Number}</h1>
            <p>Customer: {invoice.CustomerName}</p>
            <table>
                <tr><th>Item</th><th>Qty</th><th>Price</th></tr>
                {itemRows}
            </table>
            <p><strong>Total: {invoice.Total:C}</strong></p>
        </body>
        </html>";

    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Example 5: Web Server Mode to In-Process

**Before (jsreport Web Server mode):**
```csharp
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;

public class JsReportWebServerService : IDisposable
{
    private readonly ILocalWebServerReportingService _rs;

    public JsReportWebServerService()
    {
        _rs = new LocalReporting()
            .UseBinary(JsReportBinary.GetBinary())
            .AsWebServer()
            .Create();
    }

    public async Task StartAsync()
    {
        await _rs.StartAsync();
    }

    public async Task<byte[]> RenderAsync(string html)
    {
        var report = await _rs.RenderAsync(new RenderRequest
        {
            Template = new Template
            {
                Recipe = Recipe.ChromePdf,
                Engine = Engine.None,
                Content = html
            }
        });

        using (var ms = new MemoryStream())
        {
            await report.Content.CopyToAsync(ms);
            return ms.ToArray();
        }
    }

    public async Task StopAsync()
    {
        await _rs.KillAsync();
    }

    public void Dispose()
    {
        _rs.KillAsync().Wait();
    }
}
```

**After (IronPDF - no server management):**
```csharp
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();
    }

    // No StartAsync needed!
    // No StopAsync needed!
    // No Dispose pattern for server cleanup!

    public byte[] Render(string html)
    {
        return _renderer.RenderHtmlAsPdf(html).BinaryData;
    }

    public async Task<byte[]> RenderAsync(string html)
    {
        var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}
```

### Example 6: Custom Chrome Options

**Before (jsreport):**
```csharp
var report = await rs.RenderAsync(new RenderRequest
{
    Template = new Template
    {
        Recipe = Recipe.ChromePdf,
        Engine = Engine.None,
        Content = html,
        Chrome = new Chrome
        {
            Format = "A4",
            Landscape = false,
            MarginTop = "1.5cm",
            MarginBottom = "1.5cm",
            MarginLeft = "1cm",
            MarginRight = "1cm",
            PrintBackground = true,
            Scale = 0.9,
            MediaType = MediaType.Print,
            WaitForNetworkIdle = true
        }
    }
});
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
renderer.RenderingOptions.MarginTop = 15;   // mm
renderer.RenderingOptions.MarginBottom = 15;
renderer.RenderingOptions.MarginLeft = 10;
renderer.RenderingOptions.MarginRight = 10;
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.Zoom = 90;  // 0.9 = 90%
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
renderer.RenderingOptions.WaitFor.NetworkIdle();

var pdf = renderer.RenderHtmlAsPdf(html);
```

### Example 7: Configuration Options

**Before (jsreport with configuration):**
```csharp
var rs = new LocalReporting()
    .UseBinary(JsReportBinary.GetBinary())
    .Configure(cfg => cfg
        .DoTrustUserCode()
        .BaseUrlAsWorkingDirectory()
        .ChromeExecutablePath("/path/to/chrome"))
    .AsUtility()
    .Create();
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();

// Chrome path is automatic, but can be configured if needed
IronPdf.Installation.ChromeGpuMode = IronPdf.Engines.Chrome.ChromeGpuModes.Disabled;
IronPdf.Installation.SingleProcess = true;

// Base URL for relative resources
renderer.RenderingOptions.BaseUrl = new Uri("file:///path/to/assets/");
```

### Example 8: Async Operations

**Before (jsreport - always async):**
```csharp
public async Task<byte[]> GeneratePdfAsync(string html)
{
    var rs = new LocalReporting()
        .UseBinary(JsReportBinary.GetBinary())
        .AsUtility()
        .Create();

    var report = await rs.RenderAsync(new RenderRequest
    {
        Template = new Template
        {
            Recipe = Recipe.ChromePdf,
            Engine = Engine.None,
            Content = html
        }
    });

    using (var ms = new MemoryStream())
    {
        await report.Content.CopyToAsync(ms);
        return ms.ToArray();
    }
}
```

**After (IronPDF - sync or async):**
```csharp
// Synchronous (simple use case)
public byte[] GeneratePdf(string html)
{
    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}

// Asynchronous (web applications)
public async Task<byte[]> GeneratePdfAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

### Example 9: Platform-Specific Binaries

**Before (jsreport with platform binaries):**
```csharp
// Must install different packages per platform
// jsreport.Binary - Windows
// jsreport.Binary.Linux - Linux
// jsreport.Binary.OSX - macOS

public IJsReportBinary GetPlatformBinary()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        return JsReportBinary.GetBinary();
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        return JsReportBinary.Linux.GetBinary();
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        return JsReportBinary.OSX.GetBinary();

    throw new PlatformNotSupportedException();
}

var rs = new LocalReporting()
    .UseBinary(GetPlatformBinary())
    .AsUtility()
    .Create();
```

**After (IronPDF - automatic platform detection):**
```csharp
// No platform-specific code needed!
// IronPDF automatically handles Windows, Linux, macOS

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Example 10: Complete Service Class Migration

**Before (jsreport service):**
```csharp
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;

public class JsReportPdfService : IDisposable
{
    private readonly ILocalUtilityReportingService _rs;
    private bool _disposed;

    public JsReportPdfService()
    {
        _rs = new LocalReporting()
            .UseBinary(JsReportBinary.GetBinary())
            .Configure(cfg => cfg.DoTrustUserCode())
            .AsUtility()
            .Create();
    }

    public async Task<byte[]> GenerateReportAsync(ReportData data)
    {
        string template = LoadTemplate("report-template.html");

        var report = await _rs.RenderAsync(new RenderRequest
        {
            Template = new Template
            {
                Recipe = Recipe.ChromePdf,
                Engine = Engine.Handlebars,
                Content = template,
                Chrome = new Chrome
                {
                    Format = "A4",
                    DisplayHeaderFooter = true,
                    HeaderTemplate = "<div style='font-size:10px;text-align:center;'>{{title}}</div>",
                    FooterTemplate = "<div style='font-size:10px;text-align:center;'>Page {#pageNum} of {#numPages}</div>",
                    MarginTop = "2cm",
                    MarginBottom = "2cm"
                }
            },
            Data = data
        });

        using (var ms = new MemoryStream())
        {
            await report.Content.CopyToAsync(ms);
            return ms.ToArray();
        }
    }

    private string LoadTemplate(string name)
    {
        return File.ReadAllText(Path.Combine("Templates", name));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // jsreport cleanup
            _disposed = true;
        }
    }
}
```

**After (IronPDF service):**
```csharp
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.MarginTop = 20;
        _renderer.RenderingOptions.MarginBottom = 20;
    }

    public byte[] GenerateReport(ReportData data)
    {
        string html = GenerateHtml(data);

        _renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = $"<div style='font-size:10px;text-align:center;'>{data.Title}</div>",
            MaxHeight = 20
        };

        _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='font-size:10px;text-align:center;'>Page {page} of {total-pages}</div>",
            MaxHeight = 20
        };

        return _renderer.RenderHtmlAsPdf(html).BinaryData;
    }

    private string GenerateHtml(ReportData data)
    {
        // Use C# string interpolation instead of Handlebars
        return $@"
            <html>
            <head><title>{data.Title}</title></head>
            <body>
                <h1>{data.Title}</h1>
                <p>Generated: {DateTime.Now:yyyy-MM-dd}</p>
                <table>
                    <tr><th>Column A</th><th>Column B</th></tr>
                    {string.Join("", data.Rows.Select(r => $"<tr><td>{r.A}</td><td>{r.B}</td></tr>"))}
                </table>
            </body>
            </html>";
    }
}
```

---

## Advanced Scenarios

### Eliminating JavaScript Templating

**Replace Handlebars with C#:**

```csharp
// Handlebars: {{#each items}}...{{/each}}
// C#: string.Join("", items.Select(i => $"..."))

// Handlebars: {{#if condition}}...{{else}}...{{/if}}
// C#: condition ? "..." : "..."

// Handlebars: {{helper value}}
// C#: HelperMethod(value)
```

### Using Razor for Complex Templates

```csharp
// For complex templates, use RazorLight or ASP.NET Core views
using RazorLight;

var engine = new RazorLightEngineBuilder()
    .UseMemoryCachingProvider()
    .Build();

string html = await engine.CompileRenderAsync("Reports/Invoice.cshtml", model);
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Removing Server Management

```csharp
// jsreport required:
// - await rs.StartAsync();
// - await rs.KillAsync();
// - Dispose pattern for cleanup
// - Port conflict handling
// - Process monitoring

// IronPDF: Nothing! Just use it.
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
```

---

## Performance Considerations

### Startup Time

| Aspect | jsreport | IronPDF |
|--------|----------|---------|
| First render | 2-5s (binary startup) | 1-3s (Chromium init) |
| Web server startup | 3-8s | N/A |
| Subsequent renders | 200-500ms | 50-200ms |

### Memory Usage

| Aspect | jsreport | IronPDF |
|--------|----------|---------|
| Base footprint | Node.js + Chrome | Chromium only |
| Server mode | Persistent process | In-process |
| Binary size | 100-200MB | 50-100MB |

### Optimization Tips

1. **Reuse Renderer**: Create `ChromePdfRenderer` once, reuse for all renders
2. **Async for Web**: Use `RenderHtmlAsPdfAsync()` in web applications
3. **Template Caching**: Cache compiled Razor templates
4. **Warm Up**: Render a blank PDF at startup to initialize Chromium

```csharp
public class OptimizedPdfService
{
    private readonly ChromePdfRenderer _renderer;
    private static bool _warmedUp;

    public OptimizedPdfService()
    {
        _renderer = new ChromePdfRenderer();

        if (!_warmedUp)
        {
            _renderer.RenderHtmlAsPdf("<html></html>");
            _warmedUp = true;
        }
    }
}
```

---

## Troubleshooting

### Issue 1: Node.js/Binary Not Found

**Cause**: jsreport requires Node.js binaries to be present
**IronPDF Solution**: No Node.js needed—delete all binary references

```csharp
// Remove all of these:
// JsReportBinary.GetBinary()
// jsreport.Binary.Linux
// jsreport.Binary.OSX
```

### Issue 2: Server Port Conflicts

**Cause**: jsreport web server mode conflicts with other ports
**IronPDF Solution**: In-process rendering—no ports needed

### Issue 3: Handlebars Template Errors

**Cause**: JavaScript templating syntax errors
**IronPDF Solution**: Use C# string interpolation

```csharp
// Replace {{variable}} with {variable}
// Replace {{#each items}} with string.Join()
// Replace {{#if}} with ternary operator
```

### Issue 4: Async-Only API Confusion

**Cause**: jsreport is async-only; sync contexts cause deadlocks
**IronPDF Solution**: Both sync and async available

```csharp
// Sync for console apps
var pdf = renderer.RenderHtmlAsPdf(html);

// Async for web apps
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### Issue 5: Placeholder Syntax Differences

**Cause**: jsreport uses `{#pageNum}`, IronPDF uses `{page}`
**IronPDF Solution**: Find and replace placeholders

```csharp
// {#pageNum} → {page}
// {#numPages} → {total-pages}
// {#timestamp} → {date}
```

### Issue 6: Margin Unit Differences

**Cause**: jsreport uses CSS units ("2cm"), IronPDF uses millimeters
**IronPDF Solution**: Convert units

```csharp
// "2cm" = 20mm
// "1in" = 25.4mm (round to 25)
// "1px" ≈ 0.26mm
```

### Issue 7: Binary Disposal

**Cause**: jsreport requires explicit cleanup
**IronPDF Solution**: No cleanup needed

```csharp
// DELETE: await _rs.KillAsync();
// DELETE: Dispose() implementation for server
// IronPDF handles cleanup automatically
```

### Issue 8: Recipe/Engine Confusion

**Cause**: jsreport has multiple recipes (ChromePdf, Xlsx, etc.)
**IronPDF Solution**: Always PDF, no recipe concept

```csharp
// DELETE: Recipe = Recipe.ChromePdf
// DELETE: Engine = Engine.Handlebars
// IronPDF is always ChromePdf equivalent
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Identify all jsreport usage patterns**
  ```bash
  grep -r "using jsreport" --include="*.cs" .
  grep -r "LocalReporting\|RenderRequest\|Template" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **List templates using Handlebars/JsRender**
  ```csharp
  // Look for patterns like:
  var template = new Template { Content = "Handlebars content here" };
  ```
  **Why:** These will need to be converted to C# string interpolation.

- [ ] **Document current Chrome options used**
  ```csharp
  // Find patterns like:
  var chromeOptions = new Chrome { MarginTop = "1cm", MarginBottom = "1cm" };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Check for web server vs utility mode**
  ```csharp
  // Look for:
  LocalReporting().UseBinary().AsUtility()
  LocalReporting().UseBinary().AsWebServer()
  ```
  **Why:** IronPDF operates in-process, eliminating the need for separate server processes.

- [ ] **Note platform-specific binary packages**
  ```bash
  // Check for installed packages:
  dotnet list package | grep "jsreport.Binary"
  ```
  **Why:** IronPDF does not require platform-specific binaries, simplifying deployment.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Package Changes

- [ ] **Remove `jsreport.Binary` package**
  ```bash
  dotnet remove package jsreport.Binary
  ```
  **Why:** IronPDF does not require external binaries, reducing complexity.

- [ ] **Remove `jsreport.Binary.Linux` package**
  ```bash
  dotnet remove package jsreport.Binary.Linux
  ```
  **Why:** Platform-specific binaries are no longer needed with IronPDF.

- [ ] **Remove `jsreport.Binary.OSX` package**
  ```bash
  dotnet remove package jsreport.Binary.OSX
  ```
  **Why:** IronPDF simplifies deployment by not requiring OS-specific binaries.

- [ ] **Remove `jsreport.Local` package**
  ```bash
  dotnet remove package jsreport.Local
  ```
  **Why:** IronPDF operates entirely within the .NET environment.

- [ ] **Remove `jsreport.Types` package**
  ```bash
  dotnet remove package jsreport.Types
  ```
  **Why:** IronPDF uses native .NET types, eliminating the need for additional type packages.

- [ ] **Remove `jsreport.Client` package**
  ```bash
  dotnet remove package jsreport.Client
  ```
  **Why:** IronPDF provides a direct API without the need for client wrappers.

- [ ] **Install `IronPdf` package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Transition to IronPDF for improved PDF generation capabilities.

- [ ] **Update using statements**
  ```csharp
  // Before
  using jsreport.Local;

  // After
  using IronPdf;
  ```
  **Why:** Ensure code references the correct library for PDF operations.

### Code Changes

- [ ] **Add license key configuration**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace `LocalReporting` with `ChromePdfRenderer`**
  ```csharp
  // Before
  var rs = new LocalReporting().UseBinary().AsUtility().Create();

  // After
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** Simplifies setup by using IronPDF's in-process rendering.

- [ ] **Remove `RenderRequest` wrapper**
  ```csharp
  // Before
  var request = new RenderRequest { Template = new Template { Content = html } };
  var report = await rs.RenderAsync(request);

  // After
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** IronPDF uses direct method calls, reducing complexity.

- [ ] **Remove `Template` wrapper**
  ```csharp
  // Before
  var template = new Template { Content = html };

  // After
  // Directly use the HTML content
  ```
  **Why:** IronPDF does not require a template wrapper, simplifying the code.

- [ ] **Convert `Chrome` options to `RenderingOptions`**
  ```csharp
  // Before
  var chromeOptions = new Chrome { MarginTop = "1cm" };

  // After
  renderer.RenderingOptions.MarginTop = 10; // in millimeters
  ```
  **Why:** IronPDF's RenderingOptions provide a unified configuration interface.

- [ ] **Update placeholder syntax ({#pageNum} → {page})**
  ```csharp
  // Before
  var header = "Page {#pageNum} of {#numPages}";

  // After
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** IronPDF uses a different placeholder syntax for headers and footers.

- [ ] **Convert margin units (cm → mm)**
  ```csharp
  // Before
  chromeOptions.MarginTop = "1cm";

  // After
  renderer.RenderingOptions.MarginTop = 10; // in millimeters
  ```
  **Why:** IronPDF uses millimeters for margin settings, aligning with standard PDF practices.

- [ ] **Replace Handlebars with C# string interpolation**
  ```csharp
  // Before
  var content = "{{name}}";

  // After
  var content = $"{name}";
  ```
  **Why:** Utilize C#'s native string interpolation for cleaner and more efficient templating.

- [ ] **Remove `StartAsync()` / `KillAsync()` calls**
  ```csharp
  // Before
  await rs.StartAsync();
  await rs.KillAsync();

  // After
  // No equivalent needed
  ```
  **Why:** IronPDF operates in-process, eliminating the need for server lifecycle management.

- [ ] **Replace stream copying with `BinaryData` or `SaveAs()`**
  ```csharp
  // Before
  report.Content.CopyTo(stream);

  // After
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF provides direct file saving methods, simplifying output handling.

- [ ] **Remove Dispose patterns for server cleanup**
  ```csharp
  // Before
  rs.Dispose();

  // After
  // No equivalent needed
  ```
  **Why:** IronPDF's in-process model does not require explicit disposal.

### Testing

- [ ] **Test all PDF generation paths**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Verify header/footer rendering**
  **Why:** Ensure headers and footers render correctly with the new syntax.

- [ ] **Check page numbering**
  **Why:** Confirm that page numbers are accurate with IronPDF's placeholders.

- [ ] **Validate margin spacing**
  **Why:** Ensure that margins are correctly applied in millimeters.

- [ ] **Test with complex CSS/JavaScript pages**
  **Why:** IronPDF's Chromium engine should handle complex pages more reliably.

- [ ] **Benchmark performance**
  **Why:** Compare performance to ensure IronPDF meets or exceeds previous capabilities.

### Post-Migration

- [ ] **Delete jsreport binary files**
  **Why:** Clean up unnecessary files now that IronPDF is in use.

- [ ] **Remove Node.js dependencies if no longer needed**
  **Why:** Simplify the project by removing Node.js if it's no longer required.

- [ ] **Update deployment scripts**
  **Why:** Ensure deployment scripts reflect the new library and its requirements.

- [ ] **Update documentation**
  **Why:** Provide accurate documentation for future maintenance and development.
---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/
- **jsreport .NET Documentation**: https://jsreport.net/learn/dotnet-local
- **jsreport GitHub**: https://github.com/jsreport/jsreport-dotnet

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
