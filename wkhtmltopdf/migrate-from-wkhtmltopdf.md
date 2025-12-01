# How Do I Migrate from wkhtmltopdf to IronPDF in C#?

## Why Migrate from wkhtmltopdf to IronPDF

**wkhtmltopdf is a critical security risk.** The project has a CRITICAL severity vulnerability (CVE-2022-35583, CVSS 9.8) that allows Server-Side Request Forgery (SSRF), enabling attackers to potentially take over your infrastructure. This vulnerability will **never be patched** because the project has been **officially abandoned** since 2016-2017.

### The Security Crisis

| Issue | Severity | Status |
|-------|----------|--------|
| **CVE-2022-35583** | CRITICAL (9.8/10) | **UNPATCHED** |
| **SSRF Vulnerability** | Infrastructure takeover risk | **UNPATCHED** |
| **Last Update** | 2016-2017 | **ABANDONED** |
| **WebKit Version** | 2015 (Qt WebKit) | **OBSOLETE** |
| **CSS Grid Support** | None | Broken |
| **Flexbox Support** | Partial | Broken |
| **ES6+ JavaScript** | None | Broken |

**Every day you continue using wkhtmltopdf, your infrastructure is at risk.**

---

## The Abandonment Problem

wkhtmltopdf is not just outdated—it's a dead project with no future:

| Aspect | wkhtmltopdf | IronPDF |
|--------|-------------|---------|
| **Security Status** | CRITICAL CVE unpatched | Zero known CVEs |
| **Last Meaningful Update** | 2016-2017 | Active development |
| **Rendering Engine** | Qt WebKit (2015) | Modern Chromium |
| **CSS Grid** | ❌ Not supported | ✅ Full support |
| **Flexbox** | ⚠️ Broken | ✅ Full support |
| **ES6+ JavaScript** | ❌ Not supported | ✅ Full support |
| **Async/Await** | ❌ Not supported | ✅ Full support |
| **PDF Manipulation** | ❌ Not supported | ✅ Full support |
| **Digital Signatures** | ❌ Not supported | ✅ Full support |
| **PDF/A Compliance** | ❌ Not supported | ✅ Full support |
| **Professional Support** | None (abandoned) | Commercial with SLA |

### Affected Wrapper Libraries

All .NET wrappers for wkhtmltopdf inherit these vulnerabilities:

| Wrapper Library | Status | Security Risk |
|-----------------|--------|---------------|
| **DinkToPdf** | Abandoned | ⚠️ CRITICAL |
| **Rotativa** | Abandoned | ⚠️ CRITICAL |
| **TuesPechkin** | Abandoned | ⚠️ CRITICAL |
| **WkHtmlToPdf-DotNet** | Abandoned | ⚠️ CRITICAL |
| **NReco.PdfGenerator** | Uses wkhtmltopdf | ⚠️ CRITICAL |

**If you use any of these libraries, you are vulnerable to CVE-2022-35583.**

---

## Understanding CVE-2022-35583 (SSRF)

The Server-Side Request Forgery vulnerability allows attackers to:

1. **Access Internal Services**: Reach internal APIs, databases, and services behind your firewall
2. **Steal Credentials**: Access cloud metadata endpoints (AWS, GCP, Azure) to steal IAM credentials
3. **Port Scanning**: Scan your internal network from within your infrastructure
4. **Data Exfiltration**: Extract sensitive data through crafted HTML/CSS

### How the Attack Works

```html
<!-- Malicious HTML submitted to your PDF generator -->
<iframe src="http://169.254.169.254/latest/meta-data/iam/security-credentials/"></iframe>
<img src="http://internal-database:5432/admin"/>
```

When wkhtmltopdf renders this HTML, it fetches these URLs from your server's network context, bypassing firewalls and security controls.

**IronPDF mitigates this with secure defaults and network isolation options.**

---

## NuGet Package Changes

```bash
# Remove wkhtmltopdf wrapper (whichever you're using)
dotnet remove package WkHtmlToPdf-DotNet
dotnet remove package DinkToPdf
dotnet remove package TuesPechkin
dotnet remove package Rotativa
dotnet remove package Rotativa.AspNetCore
dotnet remove package NReco.PdfGenerator

# Remove wkhtmltopdf binary from your deployment
# Delete wkhtmltopdf.exe, wkhtmltox.dll, etc.

# Add IronPDF (secure, modern alternative)
dotnet add package IronPdf
```

---

## Namespace Mapping

| wkhtmltopdf Wrapper | IronPDF |
|---------------------|---------|
| `WkHtmlToPdfDotNet` | `IronPdf` |
| `DinkToPdf` | `IronPdf` |
| `TuesPechkin` | `IronPdf` |
| `Rotativa` | `IronPdf` |
| `Rotativa.AspNetCore` | `IronPdf` |
| `NReco.PdfGenerator` | `IronPdf` |

---

## API Mapping

### wkhtmltopdf CLI to IronPDF

| wkhtmltopdf CLI Option | IronPDF Equivalent | Notes |
|------------------------|-------------------|-------|
| `wkhtmltopdf input.html output.pdf` | `renderer.RenderHtmlFileAsPdf()` | File to PDF |
| `wkhtmltopdf URL output.pdf` | `renderer.RenderUrlAsPdf()` | URL to PDF |
| `--page-size A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Paper size |
| `--page-size Letter` | `RenderingOptions.PaperSize = PdfPaperSize.Letter` | US Letter |
| `--orientation Landscape` | `RenderingOptions.PaperOrientation = Landscape` | Orientation |
| `--margin-top 10mm` | `RenderingOptions.MarginTop = 10` | Margins in mm |
| `--margin-bottom 10mm` | `RenderingOptions.MarginBottom = 10` | |
| `--margin-left 10mm` | `RenderingOptions.MarginLeft = 10` | |
| `--margin-right 10mm` | `RenderingOptions.MarginRight = 10` | |
| `--header-html header.html` | `RenderingOptions.HtmlHeader` | HTML header |
| `--header-center "text"` | `RenderingOptions.TextHeader` | Text header |
| `--footer-html footer.html` | `RenderingOptions.HtmlFooter` | HTML footer |
| `--footer-center "text"` | `RenderingOptions.TextFooter` | Text footer |
| `--footer-center "[page]"` | `{page}` placeholder | Page number |
| `--footer-center "[toPage]"` | `{total-pages}` placeholder | Total pages |
| `--enable-javascript` | Enabled by default | JavaScript |
| `--javascript-delay 500` | `RenderingOptions.WaitFor.RenderDelay = 500` | JS delay |
| `--print-media-type` | `RenderingOptions.CssMediaType = Print` | CSS media |
| `--dpi 300` | `RenderingOptions.Dpi = 300` | DPI setting |
| `--grayscale` | `RenderingOptions.GrayScale = true` | Grayscale |
| `--zoom 0.8` | `RenderingOptions.Zoom = 80` | Zoom (%) |
| `--disable-smart-shrinking` | `RenderingOptions.FitToPaperMode` | Fit options |
| `--enable-local-file-access` | Allowed by default | Local files |

### C# Wrapper API to IronPDF

| wkhtmltopdf Wrapper | IronPDF | Notes |
|---------------------|---------|-------|
| `SynchronizedConverter` | `ChromePdfRenderer` | Main renderer |
| `HtmlToPdfDocument` | `RenderingOptions` | Configuration |
| `GlobalSettings.Out` | `pdf.SaveAs()` | Output file |
| `GlobalSettings.PaperSize` | `RenderingOptions.PaperSize` | Paper size |
| `GlobalSettings.Orientation` | `RenderingOptions.PaperOrientation` | Orientation |
| `GlobalSettings.Margins` | `RenderingOptions.Margin*` | Individual margins |
| `ObjectSettings.Page` | `RenderHtmlFileAsPdf()` | File input |
| `ObjectSettings.HtmlContent` | `RenderHtmlAsPdf()` | HTML string |
| `HeaderSettings.Center` | `TextHeader.CenterText` | Header text |
| `FooterSettings.Center` | `TextFooter.CenterText` | Footer text |
| `converter.Convert(doc)` | `renderer.RenderHtmlAsPdf()` | Generate PDF |

---

## Code Examples

### Example 1: Basic HTML to PDF

**Before (wkhtmltopdf CLI):**
```bash
wkhtmltopdf input.html output.pdf
```

**Before (C# Wrapper - WkHtmlToPdf-DotNet):**
```csharp
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;
using System.IO;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Portrait,
        PaperSize = PaperKind.A4
    },
    Objects = {
        new ObjectSettings()
        {
            Page = "input.html"
        }
    }
};
byte[] pdf = converter.Convert(doc);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 2: HTML String to PDF

**Before (wkhtmltopdf CLI):**
```bash
echo "<h1>Hello World</h1>" | wkhtmltopdf - output.pdf
```

**Before (C# Wrapper):**
```csharp
using WkHtmlToPdfDotNet;
using System.IO;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Portrait,
        PaperSize = PaperKind.A4
    },
    Objects = {
        new ObjectSettings()
        {
            HtmlContent = "<h1>Hello World</h1>"
        }
    }
};
byte[] pdf = converter.Convert(doc);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 3: URL to PDF

**Before (wkhtmltopdf CLI):**
```bash
wkhtmltopdf https://www.example.com output.pdf
```

**Before (C# Wrapper):**
```csharp
using WkHtmlToPdfDotNet;
using System.IO;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = {
        ColorMode = ColorMode.Color,
        PaperSize = PaperKind.A4
    },
    Objects = {
        new ObjectSettings()
        {
            Page = "https://www.example.com"
        }
    }
};
byte[] pdf = converter.Convert(doc);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
pdf.SaveAs("output.pdf");
```

### Example 4: Custom Page Settings and Margins

**Before (wkhtmltopdf CLI):**
```bash
wkhtmltopdf \
    --page-size A4 \
    --orientation Landscape \
    --margin-top 20mm \
    --margin-bottom 20mm \
    --margin-left 15mm \
    --margin-right 15mm \
    input.html output.pdf
```

**Before (C# Wrapper):**
```csharp
using WkHtmlToPdfDotNet;
using System.IO;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Landscape,
        PaperSize = PaperKind.A4,
        Margins = new MarginSettings()
        {
            Top = 20,
            Bottom = 20,
            Left = 15,
            Right = 15
        }
    },
    Objects = {
        new ObjectSettings()
        {
            Page = "input.html"
        }
    }
};
byte[] pdf = converter.Convert(doc);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 15;
renderer.RenderingOptions.MarginRight = 15;

var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 5: Headers and Footers with Page Numbers

**Before (wkhtmltopdf CLI):**
```bash
wkhtmltopdf \
    --margin-top 25mm \
    --margin-bottom 20mm \
    --header-center "Company Report" \
    --header-font-size 10 \
    --header-spacing 5 \
    --footer-center "Page [page] of [toPage]" \
    --footer-font-size 8 \
    input.html output.pdf
```

**Before (C# Wrapper):**
```csharp
using WkHtmlToPdfDotNet;
using System.IO;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = {
        ColorMode = ColorMode.Color,
        PaperSize = PaperKind.A4,
        Margins = new MarginSettings() { Top = 25, Bottom = 20 }
    },
    Objects = {
        new ObjectSettings()
        {
            Page = "input.html",
            HeaderSettings = {
                Center = "Company Report",
                FontSize = 10,
                Spacing = 5
            },
            FooterSettings = {
                Center = "Page [page] of [toPage]",
                FontSize = 8
            }
        }
    }
};
byte[] pdf = converter.Convert(doc);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.MarginTop = 25;
renderer.RenderingOptions.MarginBottom = 20;

// Text-based header
renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    CenterText = "Company Report",
    FontSize = 10,
    DrawDividerLine = true
};

// Text-based footer with page numbers
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Page {page} of {total-pages}",
    FontSize = 8,
    DrawDividerLine = true
};

var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 6: HTML Headers and Footers

**Before (wkhtmltopdf CLI):**
```bash
wkhtmltopdf \
    --margin-top 40mm \
    --margin-bottom 30mm \
    --header-html header.html \
    --footer-html footer.html \
    input.html output.pdf
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.MarginTop = 40;
renderer.RenderingOptions.MarginBottom = 30;

// Full HTML header
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='text-align: center; font-size: 12px;'>
            <img src='logo.png' style='height: 25px;' />
            <span style='margin-left: 15px;'>{html-title}</span>
        </div>",
    DrawDividerLine = true
};

// Full HTML footer
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='text-align: center; font-size: 10px; color: #666;'>
            Page {page} of {total-pages} | Generated: {date}
        </div>",
    DrawDividerLine = true
};

var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

**IronPDF Header/Footer Placeholders (vs wkhtmltopdf):**
| wkhtmltopdf | IronPDF | Description |
|-------------|---------|-------------|
| `[page]` | `{page}` | Current page |
| `[toPage]` | `{total-pages}` | Total pages |
| `[date]` | `{date}` | Current date |
| `[time]` | `{time}` | Current time |
| `[title]` | `{html-title}` | HTML title |
| `[url]` | `{url}` | Source URL |

### Example 7: JavaScript Rendering with Delay

**Before (wkhtmltopdf CLI):**
```bash
wkhtmltopdf \
    --enable-javascript \
    --javascript-delay 2000 \
    --no-stop-slow-scripts \
    input.html output.pdf
```

**Before (C# Wrapper):**
```csharp
using WkHtmlToPdfDotNet;
using System.IO;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = {
        PaperSize = PaperKind.A4
    },
    Objects = {
        new ObjectSettings()
        {
            Page = "input.html",
            LoadSettings = {
                JSDelay = 2000,
                StopSlowScript = false
            },
            WebSettings = {
                EnableJavascript = true
            }
        }
    }
};
byte[] pdf = converter.Convert(doc);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

// JavaScript enabled by default, add delay for dynamic content
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.RenderDelay = 2000;

// Or wait for specific condition
renderer.RenderingOptions.WaitFor.JavaScript = "window.dataLoaded === true";

// Or wait for element
renderer.RenderingOptions.WaitFor.HtmlElementId = "content-ready";

var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 8: DinkToPdf Migration

**Before (DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using System.IO;

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
            },
            Objects = {
                new ObjectSettings() {
                    PagesCount = true,
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]" },
                    FooterSettings = { FontSize = 9, Line = true, Center = "Report" }
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
        _renderer.RenderingOptions.TextHeader = new TextHeaderFooter
        {
            RightText = "Page {page} of {total-pages}",
            FontSize = 9
        };
        _renderer.RenderingOptions.TextFooter = new TextHeaderFooter
        {
            CenterText = "Report",
            FontSize = 9,
            DrawDividerLine = true
        };
    }

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Example 9: Rotativa Migration (ASP.NET MVC)

**Before (Rotativa):**
```csharp
using Rotativa;
using System.Web.Mvc;

public class ReportsController : Controller
{
    public ActionResult GenerateReport()
    {
        var model = GetReportData();
        return new ViewAsPdf("ReportView", model)
        {
            FileName = "Report.pdf",
            PageSize = Size.A4,
            PageOrientation = Orientation.Portrait,
            CustomSwitches = "--print-media-type --disable-smart-shrinking"
        };
    }
}
```

**After (IronPDF with ASP.NET Core):**
```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;

public class ReportsController : Controller
{
    public IActionResult GenerateReport()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var model = GetReportData();
        var htmlContent = RenderViewToString("ReportView", model);

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

        var pdf = renderer.RenderHtmlAsPdf(htmlContent);

        return File(pdf.BinaryData, "application/pdf", "Report.pdf");
    }

    private string RenderViewToString(string viewName, object model)
    {
        // Use a view rendering service to convert Razor view to HTML
        // Many NuGet packages available: RazorLight, RazorTemplating, etc.
    }
}
```

### Example 10: PDF Merging (Not Available in wkhtmltopdf)

**wkhtmltopdf:** Cannot merge PDFs.

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

var cover = renderer.RenderHtmlAsPdf("<h1>Cover Page</h1>");
var content = renderer.RenderHtmlAsPdf("<h1>Main Report</h1><p>Content...</p>");
var appendix = renderer.RenderHtmlAsPdf("<h1>Appendix</h1>");

var merged = PdfDocument.Merge(cover, content, appendix);
merged.SaveAs("complete-report.pdf");
```

### Example 11: PDF Security (Not Available in wkhtmltopdf)

**wkhtmltopdf:** Cannot secure PDFs.

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential Document</h1>");

pdf.SecuritySettings.OwnerPassword = "admin-password";
pdf.SecuritySettings.UserPassword = "reader-password";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrinting;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

pdf.SaveAs("secure.pdf");
```

### Example 12: Digital Signatures (Not Available in wkhtmltopdf)

**wkhtmltopdf:** Cannot sign PDFs.

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Signing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Contract</h1><p>Terms...</p>");

var signature = new PdfSignature("certificate.pfx", "password")
{
    SigningContact = "legal@company.com",
    SigningLocation = "New York, USA",
    SigningReason = "Contract Approval"
};

pdf.Sign(signature);
pdf.SaveAs("signed-contract.pdf");
```

### Example 13: PDF/A Compliance (Not Available in wkhtmltopdf)

**wkhtmltopdf:** Cannot create PDF/A documents.

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Archival Document</h1>");

pdf.SaveAsPdfA("archive.pdf", PdfAVersions.PdfA3B);
```

### Example 14: Watermarks (Not Available in wkhtmltopdf)

**wkhtmltopdf:** Cannot add watermarks.

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Draft Document</h1>");

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
pdf.SaveAs("draft.pdf");
```

### Example 15: Async Support (Not Available in wkhtmltopdf)

**wkhtmltopdf:** Synchronous only, blocks thread.

**After (IronPDF):**
```csharp
using IronPdf;

public async Task<byte[]> GeneratePdfAsync(string html)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

---

## Common Gotchas

### 1. Page Number Placeholders
- **wkhtmltopdf:** `[page]`, `[toPage]`, `[date]`, `[time]`, `[title]`
- **IronPDF:** `{page}`, `{total-pages}`, `{date}`, `{time}`, `{html-title}`

### 2. Margins Are in Millimeters
Both use millimeters, but IronPDF uses numeric values:
```csharp
renderer.RenderingOptions.MarginTop = 20; // 20mm
```

### 3. Modern CSS Works Out of the Box
IronPDF uses Chromium—CSS Grid, Flexbox, and modern JavaScript work without polyfills or workarounds (unlike wkhtmltopdf's broken 2015 WebKit).

### 4. Licensing Required for Production
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 5. Remove wkhtmltopdf Binary
Don't forget to remove the wkhtmltopdf executable from your deployment to eliminate the security vulnerability completely.

### 6. Path Resolution
For local file paths, set a base URL:
```csharp
renderer.RenderingOptions.BaseUrl = new Uri("file:///path/to/resources/");
```

### 7. JavaScript Enabled by Default
Unlike wkhtmltopdf where you must enable JavaScript, IronPDF has it enabled by default.

### 8. Print Media Type
```csharp
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
```

---

## Security Comparison

| Security Aspect | wkhtmltopdf | IronPDF |
|-----------------|-------------|---------|
| **Known CVEs** | CVE-2022-35583 (9.8 CRITICAL) | None |
| **SSRF Vulnerability** | YES - Unpatched | Mitigated |
| **Security Updates** | None since 2017 | Regular |
| **Rendering Engine** | Qt WebKit 2015 | Modern Chromium |
| **Network Isolation** | Not available | Configurable |
| **Input Sanitization** | User responsibility | Built-in options |

---

## Find All wkhtmltopdf References

```bash
# Find all wkhtmltopdf usages in your codebase
grep -r "wkhtmltopdf\|WkHtmlToPdf\|DinkToPdf\|Rotativa\|TuesPechkin\|NReco.PdfGenerator" --include="*.cs" --include="*.csproj" .
```

---

## Migration Checklist

### Pre-Migration (Security Assessment)

- [ ] **Identify all wkhtmltopdf/wrapper usages**
  ```bash
  # Find all wkhtmltopdf wrapper usages
  grep -r "wkhtmltopdf\|WkHtmlToPdf\|DinkToPdf\|Rotativa\|TuesPechkin\|NReco.PdfGenerator" \
       --include="*.cs" --include="*.csproj" .

  # Check for binary files
  find . -name "wkhtmlto*" -o -name "libwkhtmltox*"
  ```
  **Why:** Locate all vulnerable code paths that need immediate remediation.

- [ ] **Document which wrapper library is used**
  ```csharp
  // Common wrappers - check your using statements:
  using DinkToPdf;           // DinkToPdf
  using Rotativa;            // Rotativa (MVC)
  using Rotativa.AspNetCore; // Rotativa.AspNetCore
  using TuesPechkin;         // TuesPechkin
  using NReco.PdfGenerator;  // NReco.PdfGenerator
  ```
  **Why:** Different wrappers have slightly different migration paths.

- [ ] **Assess SSRF exposure**
  ```csharp
  // VULNERABLE: Any user-provided URL or HTML is a security risk
  converter.ConvertHtml(userProvidedHtml);  // SSRF risk!
  converter.ConvertUrl(userProvidedUrl);    // SSRF risk!
  ```
  **Why:** CVE-2022-35583 allows attackers to read internal files and access internal services.

### Package Changes

- [ ] **Remove wkhtmltopdf wrapper packages**
  ```bash
  # Remove wrapper packages
  dotnet remove package DinkToPdf
  dotnet remove package Rotativa
  dotnet remove package Rotativa.AspNetCore
  dotnet remove package TuesPechkin
  dotnet remove package NReco.PdfGenerator

  # Verify removal
  dotnet list package | grep -i pdf
  ```
  **Why:** Eliminate the vulnerable dependency chain entirely.

- [ ] **DELETE wkhtmltopdf binary from deployment**
  ```bash
  # Find and delete wkhtmltopdf binaries
  find . -name "wkhtmlto*" -type f -delete
  find . -name "libwkhtmltox*" -type f -delete
  rm -rf wkhtmltopdf/ libwkhtmltox/

  # Check deployment scripts for wkhtmltopdf installation
  grep -r "wkhtmltopdf" --include="*.yml" --include="*.yaml" --include="Dockerfile" .
  ```
  **Why:** The binary itself is the vulnerability—it MUST be removed.

- [ ] **Install IronPDF**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Secure Chromium-based replacement with no known vulnerabilities.

- [ ] **Add license key initialization**
  ```csharp
  // Program.cs or Startup.cs
  IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
      ?? throw new InvalidOperationException("IronPDF license key not configured");
  ```
  **Why:** Configure before first use; use environment variables in production.

### Code Updates

- [ ] **Convert DinkToPdf to IronPDF**
  ```csharp
  // Before (DinkToPdf)
  var converter = new SynchronizedConverter(new PdfTools());
  var doc = new HtmlToPdfDocument()
  {
      GlobalSettings = { PaperSize = PaperKind.A4, Orientation = Orientation.Portrait },
      Objects = { new ObjectSettings { HtmlContent = html } }
  };
  byte[] pdf = converter.Convert(doc);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
  var pdf = renderer.RenderHtmlAsPdf(html);
  byte[] bytes = pdf.BinaryData;
  ```
  **Why:** Direct API replacement with enhanced capabilities.

- [ ] **Convert Rotativa to IronPDF**
  ```csharp
  // Before (Rotativa MVC)
  public ActionResult GeneratePdf()
  {
      return new ViewAsPdf("Invoice", model)
      {
          PageSize = Size.A4,
          PageMargins = new Margins(10, 10, 10, 10)
      };
  }

  // After (IronPDF in ASP.NET Core)
  public IActionResult GeneratePdf()
  {
      var html = RenderViewToString("Invoice", model);
      var renderer = new ChromePdfRenderer();
      renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
      renderer.RenderingOptions.MarginTop = 10;
      renderer.RenderingOptions.MarginBottom = 10;
      renderer.RenderingOptions.MarginLeft = 10;
      renderer.RenderingOptions.MarginRight = 10;

      var pdf = renderer.RenderHtmlAsPdf(html);
      return File(pdf.BinaryData, "application/pdf", "Invoice.pdf");
  }
  ```
  **Why:** Eliminate MVC-specific wrapper complexity.

- [ ] **Update header/footer placeholder syntax**
  ```csharp
  // wkhtmltopdf placeholders → IronPDF placeholders
  // [page]         → {page}
  // [toPage]       → {total-pages}
  // [date]         → {date}
  // [time]         → {time}
  // [title]        → {html-title}
  // [url]          → {url}

  // Before (wkhtmltopdf)
  FooterSettings = { Left = "Page [page] of [toPage]" }

  // After (IronPDF)
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
  {
      HtmlFragment = "<div style='text-align:left;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses different placeholder format.

- [ ] **Convert header/footer to HTML**
  ```csharp
  // Before (wkhtmltopdf) - limited text-based headers
  GlobalSettings = {
      HeaderSettings = { Left = "Report", Center = "[date]", Right = "Page [page]" }
  }

  // After (IronPDF) - full HTML/CSS headers
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
  {
      HtmlFragment = @"
          <div style='display:flex; justify-content:space-between; font-size:10pt; color:gray;'>
              <span>Report</span>
              <span>{date}</span>
              <span>Page {page} of {total-pages}</span>
          </div>",
      MaxHeight = 30
  };
  ```
  **Why:** HTML headers/footers provide more styling flexibility.

- [ ] **Update JavaScript delay settings**
  ```csharp
  // Before (wkhtmltopdf)
  GlobalSettings = { JavascriptDelay = 500 }

  // After (IronPDF) - multiple options
  renderer.RenderingOptions.EnableJavaScript = true;

  // Option 1: Fixed delay
  renderer.RenderingOptions.WaitFor.RenderDelay(500);

  // Option 2: Wait for specific element (more reliable)
  renderer.RenderingOptions.WaitFor.HtmlElementById("content-loaded");

  // Option 3: Wait for JavaScript function
  renderer.RenderingOptions.WaitFor.JavaScript("window.renderComplete === true");
  ```
  **Why:** IronPDF has better JavaScript handling.

### Testing

- [ ] **Visual comparison of PDF output**
  ```csharp
  // Generate test PDFs
  var testCases = new[] { "invoice.html", "report.html", "certificate.html" };
  foreach (var testCase in testCases)
  {
      var pdf = renderer.RenderHtmlFileAsPdf(testCase);
      pdf.SaveAs($"test_output/{testCase.Replace(".html", ".pdf")}");
  }
  ```
  **Why:** Ensure migration doesn't break layouts (output should be BETTER with modern CSS).

- [ ] **Verify modern CSS rendering**
  ```csharp
  // Test CSS that wkhtmltopdf couldn't handle
  var html = @"
  <style>
      .grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
      .flex { display: flex; justify-content: space-between; align-items: center; }
      .modern { backdrop-filter: blur(10px); }
  </style>
  <div class='grid'>
      <div>Column 1</div>
      <div>Column 2</div>
      <div>Column 3</div>
  </div>";

  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** This is a key upgrade benefit - test CSS that didn't work before.

- [ ] **Security scan for residual wkhtmltopdf files**
  ```bash
  # Scan for any remaining wkhtmltopdf artifacts
  find /var/www/ -name "*wkhtmlto*" 2>/dev/null
  find /usr/local/bin/ -name "*wkhtmlto*" 2>/dev/null
  docker images | grep wkhtmltopdf

  # Check if any process is still using it
  ps aux | grep wkhtmltopdf
  ```
  **Why:** Ensure complete removal of vulnerable components.

### Post-Migration Benefits

- [ ] **Enable async rendering (improves web app performance)**
  ```csharp
  // wkhtmltopdf wrappers were synchronous and blocked threads

  // IronPDF async support
  public async Task<byte[]> GeneratePdfAsync(string html)
  {
      var renderer = new ChromePdfRenderer();
      var pdf = await renderer.RenderHtmlAsPdfAsync(html);
      return pdf.BinaryData;
  }
  ```
  **Why:** Prevent thread blocking in high-load web applications.

- [ ] **Add PDF security (new capability)**
  ```csharp
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SecuritySettings.OwnerPassword = "owner123";
  pdf.SecuritySettings.UserPassword = "user456";
  pdf.SecuritySettings.AllowUserCopyPasteContent = false;
  pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
  pdf.SaveAs("protected.pdf");
  ```
  **Why:** Password protection wasn't easy with wkhtmltopdf.

- [ ] **Implement PDF merging (new capability)**
  ```csharp
  var cover = renderer.RenderHtmlAsPdf("<h1>Cover Page</h1>");
  var content = renderer.RenderHtmlAsPdf(reportHtml);
  var appendix = PdfDocument.FromFile("appendix.pdf");

  var merged = PdfDocument.Merge(cover, content, appendix);
  merged.SaveAs("complete_report.pdf");
  ```
  **Why:** Combining PDFs required external tools with wkhtmltopdf.
---

## Why This Migration is Urgent

1. **CVE-2022-35583 is actively exploitable** - SSRF attacks are common and easy to execute
2. **No patch will ever come** - wkhtmltopdf is officially abandoned
3. **Your infrastructure is at risk** - Internal services, cloud credentials, and sensitive data are exposed
4. **Compliance requirements** - Security audits will flag this vulnerability
5. **Modern web support** - Your PDFs will finally render CSS Grid and Flexbox correctly

---

## Additional Resources

- **IronPDF Documentation:** https://ironpdf.com/docs/
- **Tutorials and Examples:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference:** https://ironpdf.com/object-reference/api/
