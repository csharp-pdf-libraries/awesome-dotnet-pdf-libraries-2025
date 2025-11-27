# Migrating from wkhtmltopdf: Complete Guide to Modern Alternatives

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF

[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()
[![Security](https://img.shields.io/badge/CVE--2022--35583-CRITICAL-red)]()

> wkhtmltopdf is abandoned, has unpatched critical vulnerabilities, and can't render modern CSS. If you're still using it (or DinkToPdf, NReco, Rotativa, or TuesPechkin), it's time to migrate. Here's how.

---

## Table of Contents

1. [Why You Must Migrate](#why-you-must-migrate)
2. [Affected Libraries](#affected-libraries)
3. [Migration Options](#migration-options)
4. [Step-by-Step Migration to IronPDF](#step-by-step-migration-to-ironpdf)
5. [API Mapping](#api-mapping)
6. [Code Examples](#code-examples)
7. [Common Issues and Solutions](#common-issues-and-solutions)

---

## Why You Must Migrate

### Critical Security Vulnerability

**CVE-2022-35583** (CVSS 9.8 - CRITICAL)

wkhtmltopdf is vulnerable to Server-Side Request Forgery (SSRF). An attacker can craft HTML that causes wkhtmltopdf to:

- Access internal network resources
- Read local files
- Probe infrastructure
- Potentially achieve remote code execution

**This vulnerability is UNPATCHED and will NEVER be fixed because wkhtmltopdf is abandoned.**

### Project Abandonment

| Indicator | Status |
|-----------|--------|
| Last meaningful commit | 2020 |
| Open issues | 1,500+ unaddressed |
| Pull requests | Ignored |
| Maintainer response | None |
| Security patches | None |

### Outdated Rendering Engine

wkhtmltopdf uses Qt WebKit from 2015:

| Feature | wkhtmltopdf | Modern Browsers |
|---------|-------------|-----------------|
| CSS Flexbox | ❌ Broken | ✅ Full support |
| CSS Grid | ❌ None | ✅ Full support |
| ES6+ JavaScript | ❌ No | ✅ Full support |
| CSS Variables | ❌ No | ✅ Full support |
| Web Fonts | ⚠️ Partial | ✅ Full support |

**If your HTML uses any modern CSS, wkhtmltopdf produces broken output.**

---

## Affected Libraries

If you use any of these, you're affected:

| Library | What It Is | Inherits CVE |
|---------|-----------|--------------|
| **wkhtmltopdf** | The base tool | ✅ Yes |
| **DinkToPdf** | .NET Core wrapper | ✅ Yes |
| **NReco.PdfGenerator** | .NET wrapper | ✅ Yes |
| **Rotativa** | ASP.NET MVC wrapper | ✅ Yes |
| **TuesPechkin** | Thread-safe wrapper | ✅ Yes |
| **Haukcode.DinkToPdf** | DinkToPdf fork | ✅ Yes |

All these libraries are wrappers around wkhtmltopdf. They inherit all its security vulnerabilities and rendering limitations.

---

## Migration Options

### Option 1: IronPDF (Recommended)

**Pros:**
- ✅ Full Chromium rendering (modern CSS)
- ✅ Built-in PDF manipulation
- ✅ Simple API migration
- ✅ Professional support
- ✅ Accessibility compliance

**Cons:**
- Commercial license ($749)

### Option 2: PuppeteerSharp

**Pros:**
- ✅ Full Chromium rendering
- ✅ Free (Apache 2.0)

**Cons:**
- ❌ 300MB deployment (downloads Chromium)
- ❌ No PDF manipulation
- ❌ Memory management required
- ❌ More complex API

### Option 3: Playwright

**Pros:**
- ✅ Full Chromium rendering
- ✅ Free (Apache 2.0)
- ✅ Microsoft-backed

**Cons:**
- ❌ 400MB+ deployment (3 browsers)
- ❌ Testing-focused (PDF secondary)
- ❌ No PDF manipulation

### Recommendation

For most teams, **IronPDF** provides the smoothest migration path with the simplest API and full feature set. The license cost is recovered in saved developer time.

---

## Step-by-Step Migration to IronPDF

### Step 1: Remove Old Packages

```bash
# Remove wkhtmltopdf wrappers
dotnet remove package DinkToPdf
dotnet remove package NReco.PdfGenerator
dotnet remove package Rotativa.AspNetCore
dotnet remove package TuesPechkin

# Remove wkhtmltopdf binaries from your project
rm -rf wkhtmltopdf/
rm -rf libwkhtmltox*
```

### Step 2: Install IronPDF

```bash
dotnet add package IronPdf
```

### Step 3: Update Code

See API mapping and examples below.

### Step 4: Test Rendering

```csharp
// Test with your actual HTML
var pdf = ChromePdfRenderer.RenderHtmlAsPdf(yourHtml);
pdf.SaveAs("test-output.pdf");

// Compare to wkhtmltopdf output - IronPDF should match browser rendering
```

### Step 5: Remove Native Dependencies

Delete wkhtmltopdf binaries from:
- Project folders
- Docker images
- CI/CD pipelines
- Server deployments

---

## API Mapping

### DinkToPdf to IronPDF

| DinkToPdf | IronPDF |
|-----------|---------|
| `new SynchronizedConverter(new PdfTools())` | `new ChromePdfRenderer()` |
| `converter.Convert(document)` | `renderer.RenderHtmlAsPdf(html)` |
| `GlobalSettings.DocumentTitle` | `renderer.RenderingOptions.Title` |
| `GlobalSettings.PaperSize` | `renderer.RenderingOptions.PaperSize` |
| `GlobalSettings.Margins` | `renderer.RenderingOptions.MarginTop/Bottom/Left/Right` |
| `ObjectSettings.HtmlContent` | First parameter to `RenderHtmlAsPdf()` |
| `ObjectSettings.WebSettings.DefaultEncoding` | UTF-8 by default |

### Rotativa to IronPDF

| Rotativa | IronPDF |
|----------|---------|
| `new ViewAsPdf("ViewName", model)` | Render view to HTML, then `RenderHtmlAsPdf()` |
| `new UrlAsPdf("https://...")` | `ChromePdfRenderer.RenderUrlAsPdf()` |
| `CustomSwitches = "--page-size A4"` | `renderer.RenderingOptions.PaperSize = PdfPaperSize.A4` |
| `PageMargins` | `renderer.RenderingOptions.MarginTop/Bottom/Left/Right` |

### NReco to IronPDF

| NReco | IronPDF |
|-------|---------|
| `new HtmlToPdfConverter()` | `new ChromePdfRenderer()` |
| `converter.GeneratePdf(html)` | `renderer.RenderHtmlAsPdf(html).BinaryData` |
| `converter.GeneratePdfFromFile(path)` | `renderer.RenderHtmlFileAsPdf(path)` |
| `converter.PageWidth` | `renderer.RenderingOptions.PaperSize` |
| `converter.Margins` | `renderer.RenderingOptions.MarginTop/Bottom/Left/Right` |

---

## Code Examples

### Before: DinkToPdf

```csharp
// OLD: DinkToPdf
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
        var doc = new HtmlToPdfDocument
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

### After: IronPDF

```csharp
// NEW: IronPDF
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.MarginTop = 10;
        _renderer.RenderingOptions.MarginBottom = 10;
    }

    public byte[] GeneratePdf(string html)
    {
        return _renderer.RenderHtmlAsPdf(html).BinaryData;
    }
}
```

### Before: Rotativa

```csharp
// OLD: Rotativa
using Rotativa.AspNetCore;

public class ReportsController : Controller
{
    public IActionResult Invoice(int id)
    {
        var model = _invoiceService.GetById(id);
        return new ViewAsPdf("InvoiceView", model)
        {
            PageSize = Size.A4,
            PageMargins = new Margins(10, 10, 10, 10),
            CustomSwitches = "--print-media-type"
        };
    }
}
```

### After: IronPDF

```csharp
// NEW: IronPDF
using IronPdf;

public class ReportsController : Controller
{
    private readonly IRazorViewToStringRenderer _viewRenderer;

    public async Task<IActionResult> Invoice(int id)
    {
        var model = _invoiceService.GetById(id);
        var html = await _viewRenderer.RenderViewToStringAsync("InvoiceView", model);

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 10;
        renderer.RenderingOptions.MarginBottom = 10;
        renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

        var pdf = renderer.RenderHtmlAsPdf(html);
        return File(pdf.BinaryData, "application/pdf", $"invoice-{id}.pdf");
    }
}
```

### Before: NReco

```csharp
// OLD: NReco
using NReco.PdfGenerator;

public byte[] ConvertHtmlToPdf(string html)
{
    var converter = new HtmlToPdfConverter();
    converter.PageWidth = 210;
    converter.PageHeight = 297;
    converter.Margins = new PageMargins { Top = 10, Bottom = 10 };

    return converter.GeneratePdf(html);
}
```

### After: IronPDF

```csharp
// NEW: IronPDF
using IronPdf;

public byte[] ConvertHtmlToPdf(string html)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.MarginTop = 10;
    renderer.RenderingOptions.MarginBottom = 10;

    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

---

## Common Issues and Solutions

### Issue 1: Flexbox/Grid Now Renders

**Symptom:** PDF looks different (better) than before

**Reason:** wkhtmltopdf couldn't render modern CSS. IronPDF renders it correctly.

**Solution:** This is expected. Your PDFs now match what users see in browsers.

### Issue 2: JavaScript Executes

**Symptom:** Dynamic content appears that didn't before

**Reason:** wkhtmltopdf had limited JavaScript. IronPDF executes full JavaScript.

**Solution:** Usually beneficial. If problematic, disable with:

```csharp
renderer.RenderingOptions.EnableJavaScript = false;
```

### Issue 3: Web Fonts Load

**Symptom:** Fonts look different (usually better)

**Reason:** wkhtmltopdf had poor web font support.

**Solution:** Usually beneficial. For consistency, ensure HTML uses web fonts:

```html
<link href="https://fonts.googleapis.com/css2?family=Roboto" rel="stylesheet">
```

### Issue 4: Custom wkhtmltopdf Switches

**Symptom:** Custom command-line switches no longer work

**Solution:** Map to IronPDF equivalents:

| wkhtmltopdf Switch | IronPDF Equivalent |
|-------------------|-------------------|
| `--page-size A4` | `PaperSize = PdfPaperSize.A4` |
| `--orientation Landscape` | `PaperOrientation = PdfPaperOrientation.Landscape` |
| `--margin-top 10` | `MarginTop = 10` |
| `--print-media-type` | `CssMediaType = PdfCssMediaType.Print` |
| `--javascript-delay 200` | `WaitFor.JavaScript(200)` |
| `--disable-javascript` | `EnableJavaScript = false` |
| `--header-html file.html` | `HtmlHeader = new HtmlHeaderFooter { ... }` |

### Issue 5: Native Library Errors

**Symptom:** Errors about missing libwkhtmltox

**Solution:** Remove all wkhtmltopdf native dependencies:

```bash
# Linux
rm -rf /usr/local/lib/libwkhtmltox*

# Docker
# Remove apt-get install wkhtmltopdf from Dockerfile

# Project
rm -rf libwkhtmltox.* wkhtmltopdf*
```

---

## Docker Migration

### Before: wkhtmltopdf Docker

```dockerfile
# OLD
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y \
    wkhtmltopdf \
    xvfb \
    && rm -rf /var/lib/apt/lists/*

COPY . /app
WORKDIR /app
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### After: IronPDF Docker

```dockerfile
# NEW
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y \
    libgdiplus libc6-dev libx11-xcb1 libxcomposite1 \
    libxcursor1 libxdamage1 libxi6 libxtst6 libnss3 \
    libcups2 libxss1 libxrandr2 libasound2 libatk1.0-0 \
    libatk-bridge2.0-0 libpangocairo-1.0-0 libgtk-3-0 \
    && rm -rf /var/lib/apt/lists/*

COPY . /app
WORKDIR /app
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

---

## Verification Checklist

After migration, verify:

- [ ] All PDFs generate without errors
- [ ] Modern CSS (Flexbox, Grid) renders correctly
- [ ] Fonts appear as expected
- [ ] Headers/footers work
- [ ] Page numbers work
- [ ] All wkhtmltopdf binaries removed
- [ ] No libwkhtmltox references in code
- [ ] Docker images updated
- [ ] CI/CD pipelines updated
- [ ] Security scan passes (no CVE-2022-35583)

---

## Conclusion

Migrating from wkhtmltopdf is essential:

1. **Security:** CVE-2022-35583 is unpatched and critical
2. **Rendering:** Modern CSS requires modern rendering engine
3. **Support:** Abandoned projects don't get fixes

IronPDF provides a straightforward migration path with a simpler API and better output.

---

## Related Tutorials

- **[HTML to PDF](html-to-pdf-csharp.md)** — Modern HTML conversion
- **[Best PDF Libraries 2025](best-pdf-libraries-dotnet-2025.md)** — Library comparison
- **[Free vs Paid](free-vs-paid-pdf-libraries.md)** — Cost analysis
- **[Cross-Platform](cross-platform-pdf-dotnet.md)** — Deployment guide

### Migration Guides
- **[DinkToPdf Guide](dinktopdf/)** — wkhtmltopdf wrapper migration
- **[NReco Guide](nrecopdfgenerator/)** — NReco.PdfGenerator migration
- **[Rotativa Guide](rotativa/)** — Rotativa migration
- **[IronPDF Guide](ironpdf/)** — Target library

### Resources
- **[IronPDF Documentation](https://ironpdf.com/docs/)** — Complete API reference
- **[Free Trial](https://ironpdf.com/trial/)** — 30-day full features
- **[CVE-2022-35583](https://nvd.nist.gov/vuln/detail/CVE-2022-35583)** — Vulnerability details

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*
