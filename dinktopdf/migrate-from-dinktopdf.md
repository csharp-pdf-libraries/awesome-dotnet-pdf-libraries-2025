# How Do I Migrate from DinkToPdf to IronPDF in C#?

## Table of Contents
1. [Why Migrate to IronPDF](#why-migrate-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start (5 Minutes)](#quick-start-5-minutes)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Migration Checklist](#migration-checklist)
10. [Additional Resources](#additional-resources)

---

## Why Migrate to IronPDF

### Critical Security Issues with DinkToPdf

DinkToPdf wraps wkhtmltopdf, which has **critical unpatched security vulnerabilities**:

1. **CVE-2022-35583 (SSRF)**: Server-Side Request Forgery allowing attackers to access internal network resources
2. **Abandoned Project**: wkhtmltopdf has been unmaintained since 2020
3. **No Security Patches**: Known vulnerabilities will never be fixed

### DinkToPdf Technical Problems

| Issue | Impact |
|-------|--------|
| **Thread Safety** | SynchronizedConverter still crashes in production |
| **Native Binaries** | Complex deployment with platform-specific binaries |
| **CSS Limitations** | No Flexbox, Grid, or modern CSS support |
| **JavaScript** | Inconsistent execution, timeouts |
| **Rendering** | Outdated WebKit engine (circa 2015) |
| **Maintenance** | Last update: 2018 |

### Key Advantages of IronPDF

| Aspect | DinkToPdf | IronPDF |
|--------|-----------|---------|
| **Security** | CVE-2022-35583 (SSRF), unpatched | No known vulnerabilities |
| **Rendering Engine** | Outdated WebKit (2015) | Modern Chromium |
| **Thread Safety** | Crashes in concurrent use | Fully thread-safe |
| **Native Dependencies** | Platform-specific binaries | Pure NuGet package |
| **CSS Support** | No Flexbox/Grid | Full CSS3 |
| **JavaScript** | Limited, inconsistent | Full support |
| **Maintenance** | Abandoned (2018) | Actively maintained |
| **Support** | Community only | Professional support |

### Feature Comparison

| Feature | DinkToPdf | IronPDF |
|---------|-----------|---------|
| HTML to PDF | ✅ (outdated engine) | ✅ (Chromium) |
| URL to PDF | ✅ | ✅ |
| Custom margins | ✅ | ✅ |
| Headers/Footers | ✅ (limited) | ✅ (full HTML) |
| CSS3 | ❌ Limited | ✅ Full |
| Flexbox/Grid | ❌ | ✅ |
| JavaScript | ⚠️ Limited | ✅ Full |
| PDF manipulation | ❌ | ✅ |
| Form filling | ❌ | ✅ |
| Digital signatures | ❌ | ✅ |
| Encryption | ❌ | ✅ |
| Watermarks | ❌ | ✅ |
| Merge/Split | ❌ | ✅ |

---

## Before You Start

### Prerequisites

- **.NET Version**: IronPDF supports .NET Framework 4.6.2+, .NET Core 3.1+, .NET 5/6/7/8/9
- **NuGet Access**: Ensure you can install packages from nuget.org
- **License Key**: Obtain from [IronPDF website](https://ironpdf.com/) (free trial available)

### Find All DinkToPdf References

```bash
# Find all DinkToPdf usages in your codebase
grep -r "using DinkToPdf" --include="*.cs" .
grep -r "SynchronizedConverter\|HtmlToPdfDocument\|ObjectSettings" --include="*.cs" .

# Find NuGet package references
grep -r "DinkToPdf" --include="*.csproj" .
grep -r "DinkToPdf" --include="packages.config" .

# Find wkhtmltopdf binaries
find . -name "*.dll" | xargs file | grep -i wkhtmltopdf
find . -name "libwkhtmltox*"
```

### Breaking Changes Overview

| Change | DinkToPdf | IronPDF | Impact |
|--------|-----------|---------|--------|
| **Converter** | `SynchronizedConverter(new PdfTools())` | `ChromePdfRenderer` | Simpler instantiation |
| **Document** | `HtmlToPdfDocument` | Direct method call | No document object |
| **Settings** | `GlobalSettings` + `ObjectSettings` | `RenderingOptions` | Single options object |
| **Return type** | `byte[]` | `PdfDocument` | More powerful object |
| **Binary** | `libwkhtmltox.dll/so` | None (managed) | Remove native files |
| **Thread safety** | `SynchronizedConverter` required | Thread-safe by default | Simpler code |
| **DI** | Singleton required | Any lifetime | Flexible |

---

## Quick Start (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove DinkToPdf
dotnet remove package DinkToPdf

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Remove Native Binaries

Delete these files from your project:
- `libwkhtmltox.dll` (Windows)
- `libwkhtmltox.so` (Linux)
- `libwkhtmltox.dylib` (macOS)

### Step 3: Update Using Statements

```csharp
// Before
using DinkToPdf;
using DinkToPdf.Contracts;

// After
using IronPdf;
```

### Step 4: Apply License Key

```csharp
// Add at application startup (Program.cs or Global.asax)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 5: Basic Code Migration

**Before (DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Portrait,
        PaperSize = PaperKind.A4
    },
    Objects = {
        new ObjectSettings() {
            HtmlContent = "<h1>Hello World</h1>",
            WebSettings = { DefaultEncoding = "utf-8" }
        }
    }
};
byte[] pdf = converter.Convert(doc);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// No thread safety issues, no native binaries!
```

---

## Complete API Reference

### Namespace Mapping

| DinkToPdf Namespace | IronPDF Namespace | Notes |
|---------------------|-------------------|-------|
| `DinkToPdf` | `IronPdf` | Main namespace |
| `DinkToPdf.Contracts` | `IronPdf` | No separate contracts |
| `DinkToPdf.Documents` | `IronPdf` | Unified API |
| `DinkToPdf.Settings` | `IronPdf.Rendering` | Options namespace |

### Core Class Mapping

| DinkToPdf | IronPDF | Notes |
|-----------|---------|-------|
| `SynchronizedConverter` | `ChromePdfRenderer` | Thread-safe by default |
| `BasicConverter` | `ChromePdfRenderer` | Same class, simpler |
| `PdfTools` | Not needed | Internalized |
| `HtmlToPdfDocument` | Not needed | Direct method calls |
| `GlobalSettings` | `RenderingOptions` | Part of renderer |
| `ObjectSettings` | `RenderingOptions` | Part of renderer |
| `MarginSettings` | Individual margin properties | `MarginTop`, `MarginBottom`, etc. |
| `HeaderSettings` | `HtmlHeader` / `TextHeader` | Full HTML support |
| `FooterSettings` | `HtmlFooter` / `TextFooter` | Full HTML support |
| `WebSettings` | `RenderingOptions` | Part of renderer |

### GlobalSettings Mapping

| DinkToPdf GlobalSettings | IronPDF Equivalent |
|--------------------------|-------------------|
| `ColorMode = ColorMode.Color` | Default (always color) |
| `Orientation = Orientation.Portrait` | `PaperOrientation = PdfPaperOrientation.Portrait` |
| `Orientation = Orientation.Landscape` | `PaperOrientation = PdfPaperOrientation.Landscape` |
| `PaperSize = PaperKind.A4` | `PaperSize = PdfPaperSize.A4` |
| `PaperSize = PaperKind.Letter` | `PaperSize = PdfPaperSize.Letter` |
| `DocumentTitle = "Title"` | `pdf.MetaData.Title = "Title"` |
| `Out = "output.pdf"` | `pdf.SaveAs("output.pdf")` |
| `Margins = new MarginSettings()` | Individual margin properties |

### MarginSettings Mapping

| DinkToPdf Margins | IronPDF Equivalent |
|-------------------|-------------------|
| `Margins.Top = 10` | `MarginTop = 10` |
| `Margins.Bottom = 10` | `MarginBottom = 10` |
| `Margins.Left = 10` | `MarginLeft = 10` |
| `Margins.Right = 10` | `MarginRight = 10` |
| Units: millimeters | Units: millimeters |

### ObjectSettings Mapping

| DinkToPdf ObjectSettings | IronPDF Equivalent |
|--------------------------|-------------------|
| `HtmlContent = "..."` | `RenderHtmlAsPdf("...")` |
| `Page = "https://..."` | `RenderUrlAsPdf("...")` |
| `PagesCount = true` | Automatic |
| `WebSettings.DefaultEncoding = "utf-8"` | Default UTF-8 |
| `WebSettings.UserStyleSheet = "..."` | CSS in HTML `<style>` tags |
| `WebSettings.LoadImages = true` | Default true |
| `WebSettings.EnableJavascript = true` | `EnableJavaScript = true` |

### HeaderSettings/FooterSettings Mapping

| DinkToPdf Headers/Footers | IronPDF Equivalent |
|---------------------------|-------------------|
| `HeaderSettings.FontName` | CSS `font-family` in HTML |
| `HeaderSettings.FontSize` | CSS `font-size` in HTML |
| `HeaderSettings.Right = "[page] of [toPage]"` | `{page} of {total-pages}` |
| `HeaderSettings.Left = "Title"` | HTML content |
| `HeaderSettings.Center = "..."` | HTML with CSS `text-align: center` |
| `HeaderSettings.Line = true` | CSS `border-bottom` in HTML |
| `HeaderSettings.HtmUrl = "header.html"` | `HtmlHeader.HtmlFragment` |
| `FooterSettings.*` | Same pattern with `HtmlFooter` |

### WebSettings Mapping

| DinkToPdf WebSettings | IronPDF Equivalent |
|-----------------------|-------------------|
| `DefaultEncoding = "utf-8"` | Default (always UTF-8) |
| `EnableJavascript = true` | `EnableJavaScript = true` |
| `LoadImages = true` | Default true |
| `UserStyleSheet = "..."` | CSS in HTML or `CustomCssUrl` |
| `PrintMediaType = true` | `CssMediaType = PdfCssMediaType.Print` |
| `MinimumFontSize = 12` | CSS `min-font-size` |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new SynchronizedConverter(new PdfTools());

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
            },
            Objects = {
                new ObjectSettings() {
                    HtmlContent = "<h1>Hello World</h1><p>This is a test.</p>",
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
        };

        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("output.pdf", pdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>This is a test.</p>");
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 2: URL to PDF

**Before (DinkToPdf):**
```csharp
using DinkToPdf;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new SynchronizedConverter(new PdfTools());

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A4,
            },
            Objects = {
                new ObjectSettings() {
                    Page = "https://example.com"
                }
            }
        };

        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("webpage.pdf", pdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

### Example 3: Custom Margins

**Before (DinkToPdf):**
```csharp
using DinkToPdf;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new SynchronizedConverter(new PdfTools());

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings() {
                    Top = 20,
                    Bottom = 20,
                    Left = 15,
                    Right = 15
                }
            },
            Objects = {
                new ObjectSettings() {
                    HtmlContent = "<h1>Document with Margins</h1>"
                }
            }
        };

        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("margins.pdf", pdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.MarginLeft = 15;
        renderer.RenderingOptions.MarginRight = 15;

        var pdf = renderer.RenderHtmlAsPdf("<h1>Document with Margins</h1>");
        pdf.SaveAs("margins.pdf");
    }
}
```

### Example 4: Headers and Footers

**Before (DinkToPdf):**
```csharp
using DinkToPdf;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new SynchronizedConverter(new PdfTools());

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings() { Top = 25, Bottom = 25 }
            },
            Objects = {
                new ObjectSettings() {
                    PagesCount = true,
                    HtmlContent = "<h1>Document Content</h1>",
                    HeaderSettings = {
                        FontName = "Arial",
                        FontSize = 9,
                        Right = "Page [page] of [toPage]",
                        Line = true,
                        Center = "Document Title"
                    },
                    FooterSettings = {
                        FontName = "Arial",
                        FontSize = 9,
                        Line = true,
                        Center = "Confidential"
                    }
                }
            }
        };

        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("headers.pdf", pdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 25;
        renderer.RenderingOptions.MarginBottom = 25;

        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
        {
            HtmlFragment = @"
                <div style='width:100%; font-family:Arial; font-size:9px; border-bottom:1px solid black; padding-bottom:5px;'>
                    <span style='float:left;'>Document Title</span>
                    <span style='float:right;'>Page {page} of {total-pages}</span>
                </div>",
            DrawDividerLine = false // Already in HTML
        };

        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
        {
            HtmlFragment = @"
                <div style='width:100%; font-family:Arial; font-size:9px; border-top:1px solid black; padding-top:5px; text-align:center;'>
                    Confidential
                </div>"
        };

        var pdf = renderer.RenderHtmlAsPdf("<h1>Document Content</h1>");
        pdf.SaveAs("headers.pdf");
    }
}
```

### Example 5: Multiple Pages/Objects

**Before (DinkToPdf):**
```csharp
using DinkToPdf;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new SynchronizedConverter(new PdfTools());

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                PaperSize = PaperKind.A4
            },
            Objects = {
                new ObjectSettings() {
                    HtmlContent = "<h1>Cover Page</h1>",
                    WebSettings = { DefaultEncoding = "utf-8" }
                },
                new ObjectSettings() {
                    HtmlContent = "<h1>Chapter 1</h1><p>Content...</p>",
                    WebSettings = { DefaultEncoding = "utf-8" }
                },
                new ObjectSettings() {
                    Page = "https://example.com"
                }
            }
        };

        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("multi.pdf", pdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

        // Render each section
        var cover = renderer.RenderHtmlAsPdf("<h1>Cover Page</h1>");
        var chapter = renderer.RenderHtmlAsPdf("<h1>Chapter 1</h1><p>Content...</p>");
        var webpage = renderer.RenderUrlAsPdf("https://example.com");

        // Merge all sections
        var combined = PdfDocument.Merge(cover, chapter, webpage);
        combined.SaveAs("multi.pdf");
    }
}
```

### Example 6: Dependency Injection (ASP.NET Core)

**Before (DinkToPdf):**
```csharp
// Startup.cs
using DinkToPdf;
using DinkToPdf.Contracts;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // MUST be singleton - crashes otherwise
        services.AddSingleton(typeof(IConverter),
            new SynchronizedConverter(new PdfTools()));
    }
}

// Controller
using DinkToPdf;
using DinkToPdf.Contracts;

public class ReportController : Controller
{
    private readonly IConverter _converter;

    public ReportController(IConverter converter)
    {
        _converter = converter;
    }

    public IActionResult Generate()
    {
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = { PaperSize = PaperKind.A4 },
            Objects = {
                new ObjectSettings() {
                    HtmlContent = "<h1>Report</h1>"
                }
            }
        };

        var pdf = _converter.Convert(doc);
        return File(pdf, "application/pdf", "report.pdf");
    }
}
```

**After (IronPDF):**
```csharp
// Program.cs (optional DI registration)
using IronPdf;

// Can be any lifetime - IronPDF is thread-safe
builder.Services.AddSingleton<ChromePdfRenderer>();

// Controller
using IronPdf;

public class ReportController : Controller
{
    public IActionResult Generate()
    {
        // Can create inline - no singleton required
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

        var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1>");
        return File(pdf.BinaryData, "application/pdf", "report.pdf");
    }
}
```

### Example 7: JavaScript Enabled

**Before (DinkToPdf):**
```csharp
using DinkToPdf;

var doc = new HtmlToPdfDocument()
{
    Objects = {
        new ObjectSettings() {
            HtmlContent = "<div id='dynamic'></div><script>document.getElementById('dynamic').textContent = 'Loaded!';</script>",
            WebSettings = {
                EnableJavascript = true,
                // Note: JavaScript execution is unreliable in wkhtmltopdf
            }
        }
    }
};
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.JavaScript(1000); // Wait for JS to complete

var html = @"
    <div id='dynamic'></div>
    <script>
        document.getElementById('dynamic').textContent = 'Loaded!';
    </script>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("js-content.pdf");
// JavaScript executes reliably with Chromium!
```

### Example 8: Custom Styles

**Before (DinkToPdf):**
```csharp
using DinkToPdf;

var doc = new HtmlToPdfDocument()
{
    Objects = {
        new ObjectSettings() {
            HtmlContent = "<h1>Styled</h1>",
            WebSettings = {
                UserStyleSheet = "/path/to/styles.css"
            }
        }
    }
};
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Option 1: Inline CSS
var html = @"
    <html>
    <head>
        <style>
            h1 { color: blue; font-family: Arial; }
            body { padding: 20px; }
        </style>
    </head>
    <body>
        <h1>Styled</h1>
    </body>
    </html>";

var pdf = renderer.RenderHtmlAsPdf(html);

// Option 2: External CSS
renderer.RenderingOptions.CustomCssUrl = "https://example.com/styles.css";
var pdf2 = renderer.RenderHtmlAsPdf("<h1>Styled</h1>");
```

### Example 9: Print Media Type

**Before (DinkToPdf):**
```csharp
using DinkToPdf;

var doc = new HtmlToPdfDocument()
{
    Objects = {
        new ObjectSettings() {
            HtmlContent = "<h1>Print-optimized</h1>",
            WebSettings = {
                PrintMediaType = true
            }
        }
    }
};
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
// Or use Screen for screen-accurate rendering
// renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Screen;

var pdf = renderer.RenderHtmlAsPdf("<h1>Print-optimized</h1>");
pdf.SaveAs("print.pdf");
```

### Example 10: Error Handling

**Before (DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

class Program
{
    static void Main()
    {
        IConverter converter = null;

        try
        {
            converter = new SynchronizedConverter(new PdfTools());

            var doc = new HtmlToPdfDocument()
            {
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = "<h1>Test</h1>"
                    }
                }
            };

            var pdf = converter.Convert(doc);
            File.WriteAllBytes("output.pdf", pdf);
        }
        catch (DllNotFoundException ex)
        {
            Console.WriteLine("wkhtmltopdf binary not found!");
        }
        catch (Exception ex) when (ex.Message.Contains("thread"))
        {
            Console.WriteLine("Thread safety crash!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        try
        {
            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf("<h1>Test</h1>");
            pdf.SaveAs("output.pdf");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            // No DLL errors, no thread crashes!
        }
    }
}
```

---

## Advanced Scenarios

### ASP.NET Core Integration

**Before (DinkToPdf):**
```csharp
// Startup.cs - Complex singleton setup
public void ConfigureServices(IServiceCollection services)
{
    // Platform-specific native library loading
    var context = new CustomAssemblyLoadContext();
    context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(),
        "libwkhtmltox.dll"));

    // MUST be singleton - otherwise crashes
    services.AddSingleton(typeof(IConverter),
        new SynchronizedConverter(new PdfTools()));
}

// Custom loader for native libraries
internal class CustomAssemblyLoadContext : AssemblyLoadContext
{
    public IntPtr LoadUnmanagedLibrary(string absolutePath)
    {
        return LoadUnmanagedDll(absolutePath);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        return LoadUnmanagedDllFromPath(unmanagedDllName);
    }
}
```

**After (IronPDF):**
```csharp
// Program.cs - Simple setup
var builder = WebApplication.CreateBuilder(args);

// Optional - IronPDF works without DI registration
builder.Services.AddSingleton<ChromePdfRenderer>();

// Set license (can also use environment variable)
IronPdf.License.LicenseKey = builder.Configuration["IronPdf:LicenseKey"];

var app = builder.Build();
// That's it - no native library loading needed!
```

### Thread-Safe Batch Processing

**Before (DinkToPdf):**
```csharp
using DinkToPdf;

// DinkToPdf crashes with parallel execution
// Must process sequentially
var converter = new SynchronizedConverter(new PdfTools());

foreach (var html in htmlDocuments) // Sequential only!
{
    var doc = new HtmlToPdfDocument()
    {
        Objects = {
            new ObjectSettings() { HtmlContent = html }
        }
    };

    var pdf = converter.Convert(doc);
    // Process...
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Threading.Tasks;

// IronPDF is fully thread-safe
Parallel.ForEach(htmlDocuments, html =>
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output-{Guid.NewGuid()}.pdf");
});

// Or async pattern
var tasks = htmlDocuments.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
});

var pdfs = await Task.WhenAll(tasks);
```

### Modern CSS (Flexbox, Grid)

**Before (DinkToPdf):**
```csharp
// wkhtmltopdf doesn't support modern CSS
// This layout breaks:
var html = @"
    <div style='display: flex; justify-content: space-between;'>
        <div>Left</div>
        <div>Right</div>
    </div>";

// Have to use tables instead:
var workaround = @"
    <table width='100%'>
        <tr>
            <td>Left</td>
            <td style='text-align:right'>Right</td>
        </tr>
    </table>";
```

**After (IronPDF):**
```csharp
using IronPdf;

// Full Flexbox support
var flexHtml = @"
    <div style='display: flex; justify-content: space-between;'>
        <div>Left</div>
        <div>Right</div>
    </div>";

// Full Grid support
var gridHtml = @"
    <div style='display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 20px;'>
        <div>Column 1</div>
        <div>Column 2</div>
        <div>Column 3</div>
    </div>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(flexHtml);
// Works perfectly with Chromium!
```

### Docker Deployment

**Before (DinkToPdf):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Complex wkhtmltopdf installation
RUN apt-get update && apt-get install -y \
    wget \
    fontconfig \
    libfreetype6 \
    libjpeg62-turbo \
    libpng16-16 \
    libx11-6 \
    libxcb1 \
    libxext6 \
    libxrender1 \
    xfonts-75dpi \
    xfonts-base \
    && wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6-1/wkhtmltox_0.12.6-1.buster_amd64.deb \
    && dpkg -i wkhtmltox_0.12.6-1.buster_amd64.deb \
    || apt-get install -f -y \
    && rm wkhtmltox_0.12.6-1.buster_amd64.deb

# Still have security vulnerabilities!
```

**After (IronPDF):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# IronPDF Linux dependencies (much simpler)
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libx11-dev \
    libnss3 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApp.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# No vulnerable wkhtmltopdf binaries!
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

---

## Performance Considerations

### Speed Comparison

| Scenario | DinkToPdf | IronPDF | Notes |
|----------|-----------|---------|-------|
| Simple HTML | 200-500ms | 100-200ms | IronPDF faster |
| Complex CSS | Fails/slow | 200-400ms | IronPDF handles it |
| JavaScript | Unreliable | 300-500ms | IronPDF works |
| Batch (100) | Sequential only | Parallel | 10x+ faster |
| Large pages | Memory issues | Stable | IronPDF stable |

### Why IronPDF is Faster

1. **Modern Engine**: Chromium vs. outdated WebKit
2. **Thread Safety**: Parallel processing vs. sequential
3. **No Native P/Invoke**: Managed code is more efficient
4. **Optimized Rendering**: Modern GPU acceleration

### Memory Optimization

```csharp
using IronPdf;

// For high-volume scenarios
Installation.ChromeGpuMode = IronPdf.Engines.Chrome.ChromeGpuModes.Disabled;

// Batch processing with explicit disposal
foreach (var html in htmlDocuments)
{
    using (var renderer = new ChromePdfRenderer())
    {
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs($"output-{Guid.NewGuid()}.pdf");
    }
}
```

---

## Troubleshooting Guide

### Issue 1: Native Binary Errors

**Error (DinkToPdf):**
```
DllNotFoundException: Unable to load DLL 'libwkhtmltox'
```

**Solution:** IronPDF has no native binaries:

```csharp
// Just works - no DLL required
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
```

### Issue 2: Thread Safety Crashes

**Error (DinkToPdf):**
```
System.AccessViolationException: Attempted to read or write protected memory
```

**Solution:** IronPDF is thread-safe:

```csharp
// Create renderers anywhere - no SynchronizedConverter needed
Parallel.For(0, 100, i =>
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf($"<h1>Document {i}</h1>");
    pdf.SaveAs($"doc-{i}.pdf");
});
```

### Issue 3: CSS Layout Broken

**Error (DinkToPdf):** Flexbox/Grid layouts don't render

**Solution:** IronPDF supports full CSS3:

```csharp
var html = @"
    <div style='display: flex; gap: 20px;'>
        <div style='flex: 1;'>Column 1</div>
        <div style='flex: 1;'>Column 2</div>
    </div>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
// Renders correctly!
```

### Issue 4: JavaScript Not Executing

**Error (DinkToPdf):** Dynamic content not rendered

**Solution:** IronPDF has reliable JavaScript:

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.JavaScript(2000);

var pdf = renderer.RenderHtmlAsPdf(htmlWithJs);
```

### Issue 5: Return Type Change

**Error:** Code expects `byte[]` but gets `PdfDocument`

**Solution:** Use `BinaryData` property:

```csharp
// Before (DinkToPdf)
byte[] pdfBytes = converter.Convert(doc);

// After (IronPDF)
var pdf = renderer.RenderHtmlAsPdf(html);
byte[] pdfBytes = pdf.BinaryData; // Get bytes
// Or save directly
pdf.SaveAs("output.pdf");
```

### Issue 6: Header/Footer Format

**Error:** Header placeholders don't work

**Solution:** Use IronPDF placeholders:

```csharp
// DinkToPdf: [page], [toPage]
// IronPDF: {page}, {total-pages}

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "Page {page} of {total-pages}"
};
```

### Issue 7: Singleton Removal

**Error:** Code requires singleton registration

**Solution:** IronPDF doesn't need singletons:

```csharp
// Before (DinkToPdf) - MUST be singleton
services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

// After (IronPDF) - any lifetime works
services.AddScoped<ChromePdfRenderer>();
// Or just create inline:
public IActionResult Generate()
{
    var renderer = new ChromePdfRenderer();
    // ...
}
```

### Issue 8: Linux Deployment

**Error:** Missing libraries on Linux

**Solution:** Install dependencies:

```bash
apt-get install -y libgdiplus libc6-dev libx11-dev libnss3 \
    libatk-bridge2.0-0 libdrm2 libxkbcommon0 libxcomposite1 \
    libxdamage1 libxfixes3 libxrandr2 libgbm1 libasound2
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all DinkToPdf usages in codebase**
  ```bash
  grep -r "using DinkToPdf" --include="*.cs" .
  grep -r "SynchronizedConverter\|HtmlToPdfDocument" --include="*.cs" .
  ```
  **Why:** DinkToPdf wraps wkhtmltopdf which has critical unpatched security vulnerabilities (CVE-2022-35583 SSRF). Identify all usages to ensure complete migration away from vulnerable code.

- [ ] **Document all GlobalSettings configurations**
  ```csharp
  // Find patterns like:
  GlobalSettings = {
      ColorMode = ColorMode.Color,
      Orientation = Orientation.Portrait,
      PaperSize = PaperKind.A4,
      Margins = new MarginSettings() { Top = 20 }
  }
  ```
  **Why:** GlobalSettings map to IronPDF's `RenderingOptions`. Document these now to ensure consistent PDF output after migration.

- [ ] **Locate all wkhtmltopdf native binaries**
  ```bash
  find . -name "libwkhtmltox*"
  find . -name "*.dll" | xargs file | grep -i wkhtmltopdf
  ```
  **Why:** These binaries contain the security vulnerabilities. They must be completely removed—IronPDF is 100% managed code with no native dependencies.

- [ ] **Identify singleton registrations**
  ```csharp
  // Find patterns like:
  services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
  ```
  **Why:** DinkToPdf REQUIRES singleton to avoid crashes. IronPDF is thread-safe with any DI lifetime—you can simplify your registration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### During Migration

- [ ] **Remove DinkToPdf NuGet package and install IronPdf**
  ```bash
  dotnet remove package DinkToPdf
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch. IronPDF has no dependency on wkhtmltopdf—it uses modern Chromium rendering.

- [ ] **Delete native binaries (libwkhtmltox.*)**
  ```bash
  rm -f libwkhtmltox.dll libwkhtmltox.so libwkhtmltox.dylib
  ```
  **Why:** These are the source of security vulnerabilities. IronPDF needs no native binaries.

- [ ] **Update using statements**
  ```csharp
  // Before
  using DinkToPdf;
  using DinkToPdf.Contracts;

  // After
  using IronPdf;
  ```
  **Why:** IronPDF uses a single namespace for all functionality.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace SynchronizedConverter with ChromePdfRenderer**
  ```csharp
  // Before (DinkToPdf)
  var converter = new SynchronizedConverter(new PdfTools());
  var doc = new HtmlToPdfDocument()
  {
      GlobalSettings = { PaperSize = PaperKind.A4 },
      Objects = { new ObjectSettings() { HtmlContent = html } }
  };
  byte[] pdf = converter.Convert(doc);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** ChromePdfRenderer is thread-safe by default—no special wrapper needed. No more SynchronizedConverter crashes in production.

- [ ] **Convert GlobalSettings to RenderingOptions**
  ```csharp
  // Before (DinkToPdf GlobalSettings)
  GlobalSettings = {
      Orientation = Orientation.Landscape,
      PaperSize = PaperKind.Letter,
      Margins = new MarginSettings { Top = 20, Bottom = 20 }
  }

  // After (IronPDF RenderingOptions)
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  ```
  **Why:** IronPDF consolidates settings into a single RenderingOptions object on the renderer.

- [ ] **Convert URL rendering**
  ```csharp
  // Before (DinkToPdf)
  new ObjectSettings() { Page = "https://example.com" }

  // After (IronPDF)
  var pdf = renderer.RenderUrlAsPdf("https://example.com");
  ```
  **Why:** Direct method call instead of nested ObjectSettings configuration.

- [ ] **Update header/footer format with HTML**
  ```csharp
  // Before (DinkToPdf)
  HeaderSettings = {
      FontName = "Arial",
      FontSize = 9,
      Right = "[page] of [toPage]",
      Line = true
  }

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = @"<div style='font-family:Arial; font-size:9px;
          border-bottom:1px solid black; text-align:right;'>
          Page {page} of {total-pages}
      </div>"
  };
  ```
  **Why:** IronPDF uses full HTML/CSS for headers/footers, giving complete control over styling. No more limited font/position options.

- [ ] **Fix page placeholder syntax**
  ```csharp
  // DinkToPdf placeholders → IronPDF placeholders
  // [page]      → {page}
  // [toPage]    → {total-pages}
  // [date]      → {date}
  // [time]      → {time}
  // [title]     → {html-title}
  ```
  **Why:** Different placeholder syntax. Search and replace all occurrences in header/footer templates.

- [ ] **Update DI registrations (simplify)**
  ```csharp
  // Before (DinkToPdf - MUST be singleton)
  services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

  // After (IronPDF - any lifetime works)
  services.AddScoped<ChromePdfRenderer>();
  // Or just create inline - no DI required:
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF is thread-safe. You can simplify DI registration or remove it entirely and create renderers inline.

- [ ] **Update return type handling**
  ```csharp
  // Before (DinkToPdf returns byte[])
  byte[] pdf = converter.Convert(doc);
  File.WriteAllBytes("output.pdf", pdf);
  return File(pdf, "application/pdf");

  // After (IronPDF returns PdfDocument)
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  return File(pdf.BinaryData, "application/pdf");
  ```
  **Why:** IronPDF returns a PdfDocument object with more capabilities. Use `.BinaryData` to get bytes, or `.SaveAs()` to save directly.

- [ ] **Enable JavaScript with proper waiting**
  ```csharp
  // Before (DinkToPdf - unreliable)
  WebSettings = { EnableJavascript = true }

  // After (IronPDF - reliable)
  renderer.RenderingOptions.EnableJavaScript = true;
  renderer.RenderingOptions.WaitFor.JavaScript(2000); // Wait for JS completion
  ```
  **Why:** DinkToPdf's wkhtmltopdf has inconsistent JavaScript execution. IronPDF's Chromium engine reliably executes JavaScript with configurable wait times.

- [ ] **Use modern CSS (Flexbox/Grid)**
  ```csharp
  // Before (DinkToPdf - doesn't work)
  var html = "<div style='display: flex;'>...</div>";  // Broken!

  // After (IronPDF - full support)
  var html = @"
      <div style='display: flex; justify-content: space-between;'>
          <div>Left</div>
          <div>Right</div>
      </div>";
  var pdf = renderer.RenderHtmlAsPdf(html);  // Works!
  ```
  **Why:** wkhtmltopdf uses outdated WebKit (circa 2015) with no Flexbox/Grid support. IronPDF uses modern Chromium with full CSS3 support.

- [ ] **Convert multiple objects to merge**
  ```csharp
  // Before (DinkToPdf - multiple Objects)
  Objects = {
      new ObjectSettings() { HtmlContent = cover },
      new ObjectSettings() { HtmlContent = chapter1 },
      new ObjectSettings() { Page = "https://example.com" }
  }

  // After (IronPDF - merge separate PDFs)
  var coverPdf = renderer.RenderHtmlAsPdf(cover);
  var chapter1Pdf = renderer.RenderHtmlAsPdf(chapter1);
  var webPdf = renderer.RenderUrlAsPdf("https://example.com");
  var combined = PdfDocument.Merge(coverPdf, chapter1Pdf, webPdf);
  combined.SaveAs("output.pdf");
  ```
  **Why:** IronPDF renders each section separately then merges. This gives you more control over individual sections.

### Post-Migration

- [ ] **Run all unit tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Test multi-threaded scenarios**
  ```csharp
  // This should now work (crashed with DinkToPdf)
  Parallel.For(0, 100, i =>
  {
      var renderer = new ChromePdfRenderer();
      var pdf = renderer.RenderHtmlAsPdf($"<h1>Doc {i}</h1>");
      pdf.SaveAs($"doc-{i}.pdf");
  });
  ```
  **Why:** Verify IronPDF's thread safety. DinkToPdf crashed with concurrent execution even with SynchronizedConverter.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently than wkhtmltopdf's outdated WebKit. Usually better, but verify key documents.

- [ ] **Verify CSS rendering (Flexbox/Grid)**
  **Why:** If you had CSS workarounds for wkhtmltopdf limitations, you can now remove them and use modern CSS.

- [ ] **Test JavaScript execution**
  **Why:** Dynamic content that was unreliable in DinkToPdf should now render consistently.

- [ ] **Update CI/CD pipelines (remove wkhtmltopdf)**
  ```dockerfile
  # Remove these lines from Dockerfile:
  RUN wget wkhtmltox... && dpkg -i wkhtmltox...

  # IronPDF only needs:
  RUN apt-get install -y libgdiplus libnss3 libatk-bridge2.0-0 libdrm2 \
      libxkbcommon0 libxcomposite1 libxdamage1 libxfixes3 libxrandr2 libgbm1 libasound2
  ```
  **Why:** Simplify Docker builds and remove vulnerable wkhtmltopdf binaries from CI/CD.

- [ ] **Verify security scan passes**
  **Why:** Security scanners should no longer flag CVE-2022-35583 or other wkhtmltopdf vulnerabilities after migration.

---

## Additional Resources

### IronPDF Documentation
- **Getting Started**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF**: https://ironpdf.com/how-to/html-file-to-pdf/

### DinkToPdf Reference (for migration)
- **GitHub**: https://github.com/rdvojmoc/DinkToPdf
- **NuGet**: https://www.nuget.org/packages/DinkToPdf

### Security Resources
- **CVE-2022-35583**: https://nvd.nist.gov/vuln/detail/CVE-2022-35583
- **wkhtmltopdf Security**: https://wkhtmltopdf.org/status.html

### Code Examples
- **IronPDF Examples**: https://ironpdf.com/examples/
- **Stack Overflow**: Search `[ironpdf]` tag

### Support
- **IronPDF Support**: https://ironpdf.com/support/
- **License Information**: https://ironpdf.com/licensing/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
