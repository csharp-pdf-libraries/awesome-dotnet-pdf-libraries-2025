# How Do I Migrate from Haukcode.DinkToPdf to IronPDF in C#?

## Table of Contents

1. [Why Migrate from Haukcode.DinkToPdf to IronPDF](#why-migrate-from-haukcodedinktopdf-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Security Improvements](#security-improvements)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from Haukcode.DinkToPdf to IronPDF

### The Critical Security Problem

Haukcode.DinkToPdf is a fork of the abandoned DinkToPdf project, which wraps the wkhtmltopdf binary. **wkhtmltopdf has critical security vulnerabilities that will never be patched because the project is abandoned.**

#### CVE-2022-35583 - Critical SSRF Vulnerability (CVSS 9.8)

The wkhtmltopdf library (and all wrappers including Haukcode.DinkToPdf) is vulnerable to **Server-Side Request Forgery (SSRF)**:

- **Attack Vector**: Malicious HTML content can make the server fetch internal resources
- **AWS Metadata Attack**: Can access `http://169.254.169.254` to steal AWS credentials
- **Internal Network Access**: Can scan and access internal services
- **Local File Inclusion**: Can read local files via `file://` protocol
- **Impact**: Complete infrastructure takeover possible

**There is NO fix for this vulnerability** because wkhtmltopdf is abandoned (archived since January 2023).

### Additional wkhtmltopdf Issues

1. **Abandoned Project**: Last release was 0.12.6 in 2020; project archived in 2023
2. **Outdated WebKit Engine**: Uses Qt WebKit from ~2015—missing years of security patches
3. **No HTML5/CSS3 Support**: Limited rendering of modern web standards
4. **Native Binary Dependency**: Must distribute platform-specific binaries (Windows/Linux/macOS)
5. **Thread Safety Issues**: Requires `SynchronizedConverter` singleton pattern
6. **JavaScript Limitations**: Limited and insecure JavaScript execution

### What IronPDF Offers Instead

| Aspect | Haukcode.DinkToPdf | IronPDF |
|--------|-------------------|---------|
| Underlying Engine | wkhtmltopdf (Qt WebKit ~2015) | Chromium (regularly updated) |
| Security Status | CVE-2022-35583 (CRITICAL, unfixable) | Actively patched |
| Project Status | Fork of abandoned project | Actively developed |
| HTML5/CSS3 | Limited | Full support |
| JavaScript | Limited, insecure | Full V8 engine |
| Native Binaries | Required (platform-specific) | Self-contained |
| Thread Safety | Requires singleton pattern | Thread-safe by design |
| Support | Community only | Professional support |
| Updates | None expected | Regular releases |

---

## Before You Start

### Prerequisites

1. **.NET Version**: IronPDF supports .NET Framework 4.6.2+ and .NET Core 3.1+ / .NET 5+
2. **License Key**: Obtain from [IronPDF website](https://ironpdf.com/licensing/)
3. **Remove Native Binaries**: Plan to delete wkhtmltopdf DLLs/binaries

### Identify Haukcode.DinkToPdf Usage

Find all DinkToPdf usage in your codebase:

```bash
# Find DinkToPdf namespace usage
grep -r "using DinkToPdf\|using Haukcode" --include="*.cs" .

# Find converter usage
grep -r "SynchronizedConverter\|BasicConverter\|HtmlToPdfDocument" --include="*.cs" .

# Find native library loading
grep -r "wkhtmltopdf\|libwkhtmltox" --include="*.cs" --include="*.csproj" .

# Find GlobalSettings/ObjectSettings usage
grep -r "GlobalSettings\|ObjectSettings\|MarginSettings" --include="*.cs" .
```

### Dependency Audit

Check your project file for DinkToPdf packages:

```bash
grep -r "DinkToPdf\|Haukcode" --include="*.csproj" .
```

Common package names:
- `DinkToPdf`
- `Haukcode.DinkToPdf`
- `Haukcode.WkHtmlToPdf-DotNet`

---

## Quick Start Migration

### Step 1: Remove DinkToPdf and Native Binaries

```bash
# Remove NuGet packages
dotnet remove package DinkToPdf
dotnet remove package Haukcode.DinkToPdf
dotnet remove package Haukcode.WkHtmlToPdf-DotNet

# Install IronPDF
dotnet add package IronPdf
```

**Delete native binaries:**
- `libwkhtmltox.dll` (Windows)
- `libwkhtmltox.so` (Linux)
- `libwkhtmltox.dylib` (macOS)

### Step 2: Update Code

**Before (Haukcode.DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

public class PdfService
{
    private readonly IConverter _converter;

    public PdfService()
    {
        _converter = new SynchronizedConverter(new PdfTools());
    }

    public byte[] GeneratePdf(string html)
    {
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10, Bottom = 10 }
            },
            Objects = {
                new ObjectSettings {
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
        };

        return _converter.Convert(doc);
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
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.MarginTop = 10;
        _renderer.RenderingOptions.MarginBottom = 10;
    }

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}

// No singleton required - IronPDF is thread-safe!
```

### Step 3: Update Dependency Injection

**Before (Haukcode.DinkToPdf):**
```csharp
// Startup.cs - MUST be singleton due to thread safety issues
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
}
```

**After (IronPDF):**
```csharp
// Startup.cs - Can be singleton or transient (both work)
public void ConfigureServices(IServiceCollection services)
{
    IronPdf.License.LicenseKey = Configuration["IronPdf:LicenseKey"];
    services.AddSingleton<IPdfService, IronPdfService>();
    // Or services.AddTransient<IPdfService, IronPdfService>() - both are safe!
}
```

---

## Complete API Reference

### Converter Class Mapping

| Haukcode.DinkToPdf | IronPDF | Notes |
|-------------------|---------|-------|
| `SynchronizedConverter` | `ChromePdfRenderer` | Thread-safe, no singleton required |
| `BasicConverter` | `ChromePdfRenderer` | Same class handles both |
| `PdfTools` | N/A | Not needed |
| `IConverter` | N/A | Use renderer directly |

### Document Configuration Mapping

| Haukcode.DinkToPdf | IronPDF | Notes |
|-------------------|---------|-------|
| `HtmlToPdfDocument` | Method call | Use `RenderHtmlAsPdf()` directly |
| `GlobalSettings` | `RenderingOptions` | Set before rendering |
| `ObjectSettings` | `RenderingOptions` | Combined into one |
| `converter.Convert(doc)` | `renderer.RenderHtmlAsPdf(html)` | Returns `PdfDocument` |

### GlobalSettings Property Mapping

| GlobalSettings Property | IronPDF Property | Notes |
|------------------------|------------------|-------|
| `ColorMode` | `RenderingOptions.GrayScale` | Boolean, set `true` for grayscale |
| `Orientation` | `RenderingOptions.PaperOrientation` | `Portrait` or `Landscape` |
| `PaperSize` | `RenderingOptions.PaperSize` | Use `PdfPaperSize` enum |
| `Margins.Top` | `RenderingOptions.MarginTop` | In millimeters |
| `Margins.Bottom` | `RenderingOptions.MarginBottom` | In millimeters |
| `Margins.Left` | `RenderingOptions.MarginLeft` | In millimeters |
| `Margins.Right` | `RenderingOptions.MarginRight` | In millimeters |
| `DPI` | `RenderingOptions.Dpi` | Dots per inch |
| `Out` | `pdf.SaveAs(path)` | Save after rendering |
| `DocumentTitle` | `pdf.MetaData.Title` | Set after rendering |
| `Collate` | N/A | IronPDF handles automatically |
| `Copies` | N/A | Handle in application code |

### ObjectSettings Property Mapping

| ObjectSettings Property | IronPDF Property | Notes |
|------------------------|------------------|-------|
| `HtmlContent` | First parameter to `RenderHtmlAsPdf()` | Direct parameter |
| `Page` (URL) | `renderer.RenderUrlAsPdf(url)` | Separate method |
| `WebSettings.DefaultEncoding` | Automatic | UTF-8 by default |
| `WebSettings.LoadImages` | `RenderingOptions.EnableJavaScript` | Part of rendering |
| `WebSettings.UserStyleSheet` | `RenderingOptions.CustomCssUrl` | External CSS |
| `PagesCount` | N/A | Use `pdf.PageCount` |
| `UseLocalLinks` | Automatic | Links preserved |
| `ProduceForms` | Automatic | Forms preserved |

### Header/Footer Settings Mapping

| DinkToPdf HeaderSettings | IronPDF Property | Notes |
|-------------------------|------------------|-------|
| `FontSize` | `TextHeaderFooter.FontSize` | Numeric |
| `FontName` | `TextHeaderFooter.FontFamily` | Font family name |
| `Left` | `TextHeaderFooter.LeftText` | Left-aligned text |
| `Center` | `TextHeaderFooter.CenterText` | Center-aligned text |
| `Right` | `TextHeaderFooter.RightText` | Right-aligned text |
| `Line` | `TextHeaderFooter.DrawDividerLine` | Boolean |
| `Spacing` | N/A | CSS in HtmlHeaderFooter |
| `[page]` placeholder | `{page}` | Page number |
| `[toPage]` placeholder | `{total-pages}` | Total pages |
| `[date]` placeholder | `{date}` | Current date |
| `[title]` placeholder | `{title}` | Document title |

### WebSettings Mapping

| WebSettings Property | IronPDF Property | Notes |
|---------------------|------------------|-------|
| `DefaultEncoding` | Automatic | UTF-8 default |
| `LoadImages` | Automatic | Always loads |
| `EnablePlugins` | N/A | Chromium handles |
| `EnableJavascript` | `RenderingOptions.EnableJavaScript` | Boolean |
| `UserStyleSheet` | `RenderingOptions.CustomCssUrl` | CSS file path/URL |
| `PrintMediaType` | `RenderingOptions.CssMediaType` | `Print` or `Screen` |
| `EnableIntelligentShrinking` | `RenderingOptions.FitToPaperMode` | Fit options |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (Haukcode.DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

public byte[] ConvertHtmlToPdf(string html)
{
    var converter = new SynchronizedConverter(new PdfTools());

    var doc = new HtmlToPdfDocument()
    {
        GlobalSettings = {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4
        },
        Objects = {
            new ObjectSettings {
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
    };

    return converter.Convert(doc);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertHtmlToPdf(string html)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 2: URL to PDF with Custom Settings

**Before (Haukcode.DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

public byte[] ConvertUrlToPdf(string url)
{
    var converter = new SynchronizedConverter(new PdfTools());

    var doc = new HtmlToPdfDocument()
    {
        GlobalSettings = {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Landscape,
            PaperSize = PaperKind.Letter,
            Margins = new MarginSettings {
                Top = 20, Bottom = 20, Left = 15, Right = 15
            },
            DPI = 300
        },
        Objects = {
            new ObjectSettings {
                Page = url,
                WebSettings = {
                    DefaultEncoding = "utf-8",
                    LoadImages = true,
                    EnableJavascript = true
                }
            }
        }
    };

    return converter.Convert(doc);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertUrlToPdf(string url)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
    renderer.RenderingOptions.MarginTop = 20;
    renderer.RenderingOptions.MarginBottom = 20;
    renderer.RenderingOptions.MarginLeft = 15;
    renderer.RenderingOptions.MarginRight = 15;
    renderer.RenderingOptions.Dpi = 300;
    renderer.RenderingOptions.EnableJavaScript = true;

    var pdf = renderer.RenderUrlAsPdf(url);
    return pdf.BinaryData;
}
```

### Example 3: Headers and Footers with Page Numbers

**Before (Haukcode.DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

public byte[] CreateDocumentWithHeaderFooter(string html)
{
    var converter = new SynchronizedConverter(new PdfTools());

    var doc = new HtmlToPdfDocument()
    {
        GlobalSettings = {
            ColorMode = ColorMode.Color,
            PaperSize = PaperKind.A4,
            DPI = 96
        },
        Objects = {
            new ObjectSettings {
                HtmlContent = html,
                HeaderSettings = {
                    FontSize = 10,
                    FontName = "Arial",
                    Left = "Document Title",
                    Right = "Page [page] of [toPage]",
                    Line = true,
                    Spacing = 2.812
                },
                FooterSettings = {
                    FontSize = 9,
                    FontName = "Arial",
                    Center = "Confidential - © 2024 Company",
                    Line = true
                }
            }
        }
    };

    return converter.Convert(doc);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateDocumentWithHeaderFooter(string html)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.Dpi = 96;

    // Text-based header
    renderer.RenderingOptions.TextHeader = new TextHeaderFooter
    {
        LeftText = "Document Title",
        RightText = "Page {page} of {total-pages}",
        FontFamily = "Arial",
        FontSize = 10,
        DrawDividerLine = true
    };

    // Text-based footer
    renderer.RenderingOptions.TextFooter = new TextHeaderFooter
    {
        CenterText = "Confidential - © 2024 Company",
        FontFamily = "Arial",
        FontSize = 9,
        DrawDividerLine = true
    };

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}

// Or use HTML for more control:
public byte[] CreateDocumentWithHtmlHeaderFooter(string html)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = @"
            <div style='width:100%; font-family:Arial; font-size:10px;'>
                <span style='float:left;'>Document Title</span>
                <span style='float:right;'>Page {page} of {total-pages}</span>
            </div>",
        DrawDividerLine = true
    };

    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = @"
            <div style='width:100%; text-align:center; font-family:Arial; font-size:9px;'>
                Confidential - © 2024 Company
            </div>",
        DrawDividerLine = true
    };

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 4: Custom Margins and Grayscale

**Before (Haukcode.DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

public void CreateGrayscaleDocument(string html)
{
    var converter = new SynchronizedConverter(new PdfTools());

    var doc = new HtmlToPdfDocument()
    {
        GlobalSettings = {
            ColorMode = ColorMode.Grayscale,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
            Margins = new MarginSettings {
                Top = 25, Bottom = 25, Left = 30, Right = 30, Unit = Unit.Millimeters
            },
            Out = @"C:\output\grayscale.pdf"
        },
        Objects = {
            new ObjectSettings { HtmlContent = html }
        }
    };

    converter.Convert(doc);  // Saves to Out path
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreateGrayscaleDocument(string html)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.GrayScale = true;
    renderer.RenderingOptions.MarginTop = 25;
    renderer.RenderingOptions.MarginBottom = 25;
    renderer.RenderingOptions.MarginLeft = 30;
    renderer.RenderingOptions.MarginRight = 30;

    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(@"C:\output\grayscale.pdf");
}
```

### Example 5: Multiple HTML Sections (Objects)

**Before (Haukcode.DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

public byte[] CreateMultiSectionDocument(string coverHtml, string contentHtml, string appendixHtml)
{
    var converter = new SynchronizedConverter(new PdfTools());

    var doc = new HtmlToPdfDocument()
    {
        GlobalSettings = {
            PaperSize = PaperKind.A4
        },
        Objects = {
            new ObjectSettings {
                HtmlContent = coverHtml,
                WebSettings = { DefaultEncoding = "utf-8" }
            },
            new ObjectSettings {
                HtmlContent = contentHtml,
                WebSettings = { DefaultEncoding = "utf-8" },
                HeaderSettings = { Right = "Page [page]" }
            },
            new ObjectSettings {
                HtmlContent = appendixHtml,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
    };

    return converter.Convert(doc);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateMultiSectionDocument(string coverHtml, string contentHtml, string appendixHtml)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

    // Render each section
    var coverPdf = renderer.RenderHtmlAsPdf(coverHtml);

    // Content with header
    renderer.RenderingOptions.TextHeader = new TextHeaderFooter
    {
        RightText = "Page {page}"
    };
    var contentPdf = renderer.RenderHtmlAsPdf(contentHtml);

    // Appendix without header
    renderer.RenderingOptions.TextHeader = null;
    var appendixPdf = renderer.RenderHtmlAsPdf(appendixHtml);

    // Merge all sections
    var finalPdf = PdfDocument.Merge(coverPdf, contentPdf, appendixPdf);

    // Cleanup
    coverPdf.Dispose();
    contentPdf.Dispose();
    appendixPdf.Dispose();

    return finalPdf.BinaryData;
}
```

### Example 6: Custom Paper Size

**Before (Haukcode.DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

public byte[] CreateCustomSizeDocument(string html)
{
    var converter = new SynchronizedConverter(new PdfTools());

    var doc = new HtmlToPdfDocument()
    {
        GlobalSettings = {
            PaperSize = null,  // Use custom width/height
            PaperWidth = "210mm",
            PaperHeight = "297mm"  // A4 dimensions
        },
        Objects = {
            new ObjectSettings { HtmlContent = html }
        }
    };

    return converter.Convert(doc);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateCustomSizeDocument(string html)
{
    var renderer = new ChromePdfRenderer();

    // Use predefined size
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

    // Or set custom dimensions in millimeters
    renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(210, 297);

    // Or in inches
    // renderer.RenderingOptions.SetCustomPaperSizeInInches(8.27, 11.69);

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 7: JavaScript Execution Wait

**Before (Haukcode.DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

public byte[] ConvertWithJavaScript(string html)
{
    var converter = new SynchronizedConverter(new PdfTools());

    var doc = new HtmlToPdfDocument()
    {
        GlobalSettings = { PaperSize = PaperKind.A4 },
        Objects = {
            new ObjectSettings {
                HtmlContent = html,
                WebSettings = {
                    EnableJavascript = true,
                    // Limited JavaScript support, no reliable wait mechanism
                }
            }
        }
    };

    return converter.Convert(doc);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertWithJavaScript(string html)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.EnableJavaScript = true;

    // Wait for JavaScript to complete
    renderer.RenderingOptions.RenderDelay = 2000;  // 2 seconds

    // Or wait for specific condition
    renderer.RenderingOptions.WaitFor.JavaScript("window.dataLoaded === true");

    // Or wait for specific element
    renderer.RenderingOptions.WaitFor.HtmlElementById("content-loaded");

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 8: ASP.NET Core Dependency Injection

**Before (Haukcode.DinkToPdf):**
```csharp
// Startup.cs
using DinkToPdf;
using DinkToPdf.Contracts;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // MUST be singleton - not thread-safe otherwise!
        services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
    }
}

// Controller
public class ReportController : Controller
{
    private readonly IConverter _converter;

    public ReportController(IConverter converter)
    {
        _converter = converter;
    }

    public IActionResult GeneratePdf()
    {
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = { PaperSize = PaperKind.A4 },
            Objects = { new ObjectSettings { HtmlContent = "<h1>Report</h1>" } }
        };

        byte[] pdf = _converter.Convert(doc);
        return File(pdf, "application/pdf", "report.pdf");
    }
}
```

**After (IronPDF):**
```csharp
// Program.cs (.NET 6+)
using IronPdf;

var builder = WebApplication.CreateBuilder(args);

// Set license once at startup
IronPdf.License.LicenseKey = builder.Configuration["IronPdf:LicenseKey"];

// Can be singleton, scoped, or transient - all thread-safe!
builder.Services.AddSingleton<IPdfService, PdfService>();

// PdfService.cs
public interface IPdfService
{
    byte[] GeneratePdf(string html);
}

public class PdfService : IPdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    }

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}

// Controller
public class ReportController : Controller
{
    private readonly IPdfService _pdfService;

    public ReportController(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    public IActionResult GeneratePdf()
    {
        byte[] pdf = _pdfService.GeneratePdf("<h1>Report</h1>");
        return File(pdf, "application/pdf", "report.pdf");
    }
}
```

### Example 9: External CSS Stylesheet

**Before (Haukcode.DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

public byte[] ConvertWithExternalCss(string html, string cssPath)
{
    var converter = new SynchronizedConverter(new PdfTools());

    var doc = new HtmlToPdfDocument()
    {
        GlobalSettings = { PaperSize = PaperKind.A4 },
        Objects = {
            new ObjectSettings {
                HtmlContent = html,
                WebSettings = {
                    UserStyleSheet = cssPath  // Local file path
                }
            }
        }
    };

    return converter.Convert(doc);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertWithExternalCss(string html, string cssPath)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.CustomCssUrl = cssPath;  // File path or URL

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}

// Alternative: Inject CSS directly
public byte[] ConvertWithInlineCss(string html, string css)
{
    var renderer = new ChromePdfRenderer();

    // Inject CSS into HTML
    string htmlWithCss = $"<style>{css}</style>{html}";

    var pdf = renderer.RenderHtmlAsPdf(htmlWithCss);
    return pdf.BinaryData;
}
```

### Example 10: Print vs Screen Media Type

**Before (Haukcode.DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

public byte[] ConvertAsPrintMedia(string html)
{
    var converter = new SynchronizedConverter(new PdfTools());

    var doc = new HtmlToPdfDocument()
    {
        GlobalSettings = { PaperSize = PaperKind.A4 },
        Objects = {
            new ObjectSettings {
                HtmlContent = html,
                WebSettings = {
                    PrintMediaType = true  // Use @media print styles
                }
            }
        }
    };

    return converter.Convert(doc);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertAsPrintMedia(string html)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
    // Or use PdfCssMediaType.Screen for screen styles

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

---

## Advanced Scenarios

### Removing Native Binary Loading Code

**Before (with native library loading):**
```csharp
// Common pattern: Custom native library resolver
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Load native library based on platform
        var architectureFolder = IntPtr.Size == 8 ? "64bit" : "32bit";
        var wkHtmlToPdfPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            $"wkhtmltopdf\\{architectureFolder}\\libwkhtmltox");

        CustomAssemblyLoadContext context = new CustomAssemblyLoadContext();
        context.LoadUnmanagedLibrary(wkHtmlToPdfPath);

        services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
    }
}

public class CustomAssemblyLoadContext : AssemblyLoadContext
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

**After (IronPDF - no native loading needed):**
```csharp
// All this code can be DELETED!
// IronPDF is self-contained - no native library loading required

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        IronPdf.License.LicenseKey = Configuration["IronPdf:LicenseKey"];
        services.AddSingleton<IPdfService, PdfService>();
    }
}

// DELETE: CustomAssemblyLoadContext class
// DELETE: wkhtmltopdf folder and all native binaries
// DELETE: Platform detection code
```

### Thread Safety Improvements

**Before (DinkToPdf - must be singleton):**
```csharp
// DinkToPdf documentation explicitly states:
// "Use SynchronizedConverter in multi-threaded applications and web servers.
// Conversion tasks are saved to a blocking collection and executed on a single thread."

public class UnsafeUsage  // DON'T DO THIS!
{
    public byte[] GeneratePdf(string html)
    {
        // Creating new converter per request can cause crashes!
        var converter = new SynchronizedConverter(new PdfTools());
        // ...
    }
}
```

**After (IronPDF - thread-safe by design):**
```csharp
// IronPDF is fully thread-safe - create renderers freely
public class SafeUsage
{
    public byte[] GeneratePdf(string html)
    {
        // Safe to create per-request
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}

// Or use shared instance - also safe
public class SharedRenderer
{
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

---

## Security Improvements

### SSRF Protection

**Before (DinkToPdf - VULNERABLE):**
```csharp
// DANGER: User-controlled HTML can access internal resources!
public byte[] ConvertUserHtml(string userHtml)
{
    var converter = new SynchronizedConverter(new PdfTools());
    var doc = new HtmlToPdfDocument
    {
        Objects = { new ObjectSettings { HtmlContent = userHtml } }
    };
    return converter.Convert(doc);
}

// Attack payload that works with wkhtmltopdf:
// <iframe src="http://169.254.169.254/latest/meta-data/iam/security-credentials/">
// <script>fetch('http://internal-server/admin').then(r=>r.text()).then(t=>document.body.innerHTML=t)</script>
```

**After (IronPDF - more secure by default):**
```csharp
using IronPdf;

public byte[] ConvertUserHtml(string userHtml)
{
    var renderer = new ChromePdfRenderer();

    // IronPDF's Chromium engine has better security defaults
    // But still sanitize user input!

    // Option 1: Sanitize HTML before rendering
    var sanitizedHtml = HtmlSanitizer.Sanitize(userHtml);

    // Option 2: Disable JavaScript for untrusted content
    renderer.RenderingOptions.EnableJavaScript = false;

    // Option 3: Use Content Security Policy
    var wrappedHtml = $@"
        <html>
        <head>
            <meta http-equiv='Content-Security-Policy'
                  content=""default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline';"">
        </head>
        <body>{sanitizedHtml}</body>
        </html>";

    var pdf = renderer.RenderHtmlAsPdf(wrappedHtml);
    return pdf.BinaryData;
}
```

### Input Validation Best Practices

```csharp
using IronPdf;

public class SecurePdfService
{
    public byte[] GeneratePdfSecurely(string html)
    {
        // 1. Never render untrusted URLs
        if (ContainsUntrustedUrls(html))
            throw new SecurityException("Untrusted URLs detected");

        // 2. Strip iframe tags from user content
        html = RemoveIframes(html);

        // 3. Disable dangerous features for user content
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.EnableJavaScript = false;

        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }

    private bool ContainsUntrustedUrls(string html)
    {
        // Check for internal IP addresses
        var dangerousPatterns = new[]
        {
            "169.254.169.254",  // AWS metadata
            "localhost",
            "127.0.0.1",
            "10.",
            "192.168.",
            "file://"
        };

        return dangerousPatterns.Any(p =>
            html.Contains(p, StringComparison.OrdinalIgnoreCase));
    }

    private string RemoveIframes(string html)
    {
        return Regex.Replace(html, @"<iframe[^>]*>.*?</iframe>",
            string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }
}
```

---

## Troubleshooting

### Issue 1: "Native library not found" errors

**Cause**: IronPDF doesn't need native libraries—you may have leftover DinkToPdf code

**Solution**: Remove all native library loading code and wkhtmltopdf binaries:
```bash
# Delete these files if they exist
rm -rf libwkhtmltox.*
rm -rf wkhtmltopdf/
```

### Issue 2: "SynchronizedConverter throws ObjectDisposedException"

**Cause**: This was a DinkToPdf issue—won't occur in IronPDF

**Solution**: Just use `ChromePdfRenderer` directly:
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 3: "Page numbers show [page] instead of actual number"

**Cause**: Different placeholder syntax

**Solution**: Use IronPDF placeholder format:
```csharp
// DinkToPdf: [page] of [toPage]
// IronPDF: {page} of {total-pages}
renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    RightText = "Page {page} of {total-pages}"  // Correct format
};
```

### Issue 4: "Headers/footers look different"

**Cause**: DinkToPdf uses text-only headers; IronPDF supports both text and HTML

**Solution**: Use `HtmlHeaderFooter` for more control:
```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='...'>Your custom header</div>"
};
```

### Issue 5: "PDF looks different from wkhtmltopdf output"

**Cause**: IronPDF uses modern Chromium engine with better CSS support

**Solution**: This is usually an improvement. If you need specific styling:
```csharp
// Force print media type (similar to wkhtmltopdf)
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

// Disable modern CSS features if causing issues
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
```

### Issue 6: "License key not working"

**Cause**: License key not set or invalid

**Solution**:
```csharp
// Set before any IronPDF operations
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Verify license
bool isLicensed = IronPdf.License.IsLicensed;
```

### Issue 7: "Output file path not working (GlobalSettings.Out)"

**Cause**: IronPDF returns `PdfDocument` object instead of writing directly

**Solution**:
```csharp
// DinkToPdf: GlobalSettings.Out = "output.pdf";
// IronPDF:
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Issue 8: "ColorMode.Grayscale not found"

**Cause**: Different property name

**Solution**:
```csharp
// DinkToPdf: ColorMode = ColorMode.Grayscale
// IronPDF:
renderer.RenderingOptions.GrayScale = true;
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit all DinkToPdf usage locations**
  ```bash
  grep -r "using DinkToPdf" --include="*.cs" .
  grep -r "SynchronizedConverter\|HtmlToPdfDocument" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current GlobalSettings/ObjectSettings configurations**
  ```csharp
  // Find patterns like:
  var globalSettings = new GlobalSettings {
      PaperSize = PaperKind.A4,
      Orientation = Orientation.Landscape
  };
  var objectSettings = new ObjectSettings {
      PagesCount = true,
      HtmlContent = "<html>...</html>"
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Identify native library loading code**
  ```csharp
  // Look for code like:
  var context = new CustomAssemblyLoadContext();
  context.LoadUnmanagedLibrary("libwkhtmltox");
  ```
  **Why:** IronPDF does not require native binaries, so this code can be removed.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Review security implications of current implementation**
  **Why:** Understand current vulnerabilities to ensure they are addressed during migration.

- [ ] **Test IronPDF in development environment**
  **Why:** Verify IronPDF meets functional requirements before full migration.

### Code Migration

- [ ] **Remove DinkToPdf NuGet packages**
  ```bash
  dotnet remove package DinkToPdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to the project for PDF generation.

- [ ] **Update namespace imports (`DinkToPdf` → `IronPdf`)**
  ```csharp
  // Before (DinkToPdf)
  using DinkToPdf;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure the code references the correct library.

- [ ] **Replace `SynchronizedConverter` with `ChromePdfRenderer`**
  ```csharp
  // Before (DinkToPdf)
  var converter = new SynchronizedConverter(new PdfTools());

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering.

- [ ] **Convert `HtmlToPdfDocument` to direct method calls**
  ```csharp
  // Before (DinkToPdf)
  var doc = new HtmlToPdfDocument {
      GlobalSettings = globalSettings,
      Objects = { objectSettings }
  };
  converter.Convert(doc);

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(objectSettings.HtmlContent);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Simplifies code by using direct rendering methods.

- [ ] **Convert `GlobalSettings` to `RenderingOptions`**
  ```csharp
  // Before (DinkToPdf)
  globalSettings.PaperSize = PaperKind.A4;
  globalSettings.Orientation = Orientation.Landscape;

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  ```
  **Why:** IronPDF's RenderingOptions centralize all page settings.

- [ ] **Convert `ObjectSettings` to `RenderingOptions`**
  ```csharp
  // Before (DinkToPdf)
  objectSettings.PagesCount = true;
  objectSettings.HtmlContent = "<html>...</html>";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlContent = "<html>...</html>";
  ```
  **Why:** Streamlines configuration by using a single settings object.

- [ ] **Update header/footer placeholders (`[page]` → `{page}`)**
  ```csharp
  // Before (DinkToPdf)
  objectSettings.Header = "[page] of [toPage]";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with modern placeholders.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Infrastructure Cleanup

- [ ] **Delete native binaries (libwkhtmltox.*)**
  **Why:** IronPDF does not require native binaries, simplifying deployment.

- [ ] **Remove native library loading code**
  **Why:** No longer needed as IronPDF is fully managed.

- [ ] **Remove CustomAssemblyLoadContext if present**
  **Why:** Unnecessary with IronPDF, which simplifies the codebase.

- [ ] **Update Dependency Injection (singleton no longer required)**
  **Why:** IronPDF does not require a singleton, allowing more flexible DI configurations.

- [ ] **Remove platform detection code**
  **Why:** IronPDF is cross-platform without needing platform-specific binaries.

### Security Improvements

- [ ] **Review HTML input sources for SSRF vulnerabilities**
  **Why:** Ensure all input sources are secure against SSRF attacks.

- [ ] **Add input validation for user-provided content**
  **Why:** Protect against malicious input that could lead to security vulnerabilities.

- [ ] **Consider disabling JavaScript for untrusted content**
  ```csharp
  // Before (DinkToPdf)
  objectSettings.LoadSettings.EnableJavascript = true;

  // After (IronPDF)
  renderer.RenderingOptions.EnableJavaScript = false;
  ```
  **Why:** Disabling JavaScript for untrusted content enhances security.

- [ ] **Implement CSP headers where appropriate**
  **Why:** Content Security Policy (CSP) headers can mitigate XSS attacks.

### Testing

- [ ] **Test HTML to PDF conversion**
  **Why:** Verify that HTML content is correctly rendered to PDF.

- [ ] **Test URL to PDF conversion**
  **Why:** Ensure URLs are rendered accurately with full JavaScript support.

- [ ] **Verify page settings (size, orientation, margins)**
  **Why:** Confirm that document layout matches expectations.

- [ ] **Verify headers and footers**
  **Why:** Ensure headers and footers render correctly with placeholders.

- [ ] **Verify page numbers render correctly**
  **Why:** Page numbering should be accurate and consistent.

- [ ] **Test with your actual HTML templates**
  **Why:** Real-world testing ensures all templates render as expected.

- [ ] **Performance test under load**
  **Why:** Validate that the application performs well under expected load conditions.

### Post-Migration

- [ ] **Delete wkhtmltopdf folder and contents**
  **Why:** Clean up obsolete files from the project.

- [ ] **Remove DinkToPdf from any documentation**
  **Why:** Ensure all documentation reflects the current library usage.

- [ ] **Update deployment scripts**
  **Why:** Deployment scripts should no longer reference DinkToPdf or its binaries.

- [ ] **Monitor for any errors in production**
  **Why:** Early detection of issues ensures a smooth transition.

- [ ] **Enjoy the security improvements!**
  **Why:** IronPDF provides enhanced security and modern rendering capabilities.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [wkhtmltopdf CVE-2022-35583 Details](https://security.snyk.io/vuln/SNYK-UNMANAGED-WKHTMLTOPDFWKHTMLTOPDF-2988835)

---

## Summary

Migrating from Haukcode.DinkToPdf to IronPDF—covered in detail in the [complete migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-haukcode-dinktopdf-to-ironpdf/)—addresses critical security vulnerabilities and provides modern PDF generation capabilities:

1. **Security**: Eliminates CVE-2022-35583 (SSRF) and other wkhtmltopdf vulnerabilities
2. **Modern Engine**: Uses actively-updated Chromium instead of abandoned Qt WebKit
3. **No Native Binaries**: Self-contained library—no platform-specific DLLs
4. **Thread Safety**: No singleton requirement—use freely in any pattern
5. **Better HTML/CSS**: Full HTML5, CSS3, and JavaScript support
6. **Simpler API**: Direct method calls instead of document objects
7. **Active Support**: Professional support and regular updates

The migration eliminates technical debt from depending on abandoned technology while providing a more capable, secure PDF generation solution.
