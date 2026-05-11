---
title: "BCL easyPDF SDK vs IronPDF: the practical breakdown for .NET"
published: false
tags: dotnet, csharp, pdf, comparison
---

When teams migrate from legacy PDF tooling, the transition rarely happens because the old solution broke — it happens because the ecosystem around it evolved. A common pattern in .NET teams goes like this: someone integrated BCL easyPDF SDK years ago for batch "Word to PDF" jobs. It worked. Requirements expanded to include web-based invoicing, then HTML email receipts as PDFs, then dynamic charts rendered from JavaScript libraries. Now the codebase contains increasingly elaborate workarounds, and every new feature request starts with the question "can we even do this?"

BCL easyPDF SDK emerged during an era when .NET PDF solutions primarily meant converting Office documents or generating PDFs from fixed templates. For those specific workflows, it delivered value. The challenge for modern .NET applications is that "PDF generation" now frequently means "render whatever a modern browser can display," which fundamentally shifts what a PDF library has to do.

## Product Status: BCL Was Acquired by Apryse in March 2020

BCL Technologies — the original publisher of easyPDF SDK — was acquired by Apryse (formerly PDFTron) in March 2020. The `bcltechnologies.com` domain now redirects to `apryse.com`, and Apryse generally steers new evaluators toward the Apryse PDF SDK product line. The easyPDF SDK is still listed and supported for existing customers, but treating it as a legacy product when planning new .NET 8/9/10 work is a reasonable starting assumption. Confirm current maintenance cadence and supported runtime matrix with Apryse before committing to it for greenfield projects.

## Understanding IronPDF

IronPDF approaches PDF generation through a Chromium rendering engine, treating HTML/CSS/JavaScript as the primary authoring format. If content displays correctly in Chrome, it renders the same way in the PDF. The library handles HTML strings, files, URLs, and dynamic content with CSS3, web fonts, SVG, and JavaScript execution.

For .NET 6+ applications running on Windows, Linux, or macOS, IronPDF ships as a single NuGet package that bundles the rendering engine — no separate browser installation or external native-dependency manager required. The [ChromePdfRenderer class](https://ironpdf.com/how-to/html-string-to-pdf/) manages the conversion pipeline, exposing granular control over margins, headers, JavaScript execution timing, and asset loading.

## Architectural Characteristics of BCL easyPDF SDK

### Distribution Model

The easyPDF SDK does not ship as a NuGet package. It is installed via MSI, which registers a virtual printer driver and COM components and lays down .NET assemblies (`BCL.easyPDF.PDFConverter.dll` for .NET Framework, `BCL.easyPDF.PDFConverter.NetCore.dll` for .NET Core) that you reference directly from your `.csproj`. Production deployments typically also need the MSI installed on each server.

### Rendering Approach

The HTML-to-PDF path historically routes through a printer-driver pipeline rather than a modern headless-browser engine. Teams evaluating it for modern web content should verify against their actual templates how the engine handles:

- CSS3 layouts (flexbox, grid), CSS animations, and HTML5 semantic elements
- JavaScript execution timing for chart libraries (Chart.js, D3.js) and DOM manipulation
- Web font handling, Google Fonts, custom font embedding
- SVG and Canvas rendering
- Media query handling for paper sizes

### Platform Footprint

The product was designed for Windows servers with COM interop and, for Office document conversion, a Microsoft Office installation on the host. Linux, macOS, and Docker Linux containers are out of scope for the historical product. Confirm the current platform matrix with Apryse if cross-platform is a hard requirement.

### Support and Roadmap

Commercial vendor support is available through Apryse. Response-time SLAs, the easyPDF-specific roadmap, and the long-term position of easyPDF relative to the broader Apryse PDF SDK should be discussed with the vendor directly before relying on it for a multi-year build.

## Feature Comparison Overview

| Category | BCL easyPDF SDK | IronPDF |
|----------|-----------------|---------|
| **Vendor** | Apryse (acquired BCL March 2020) | Iron Software |
| **HTML Engine** | Virtual-printer / Office-based pipeline | Headless Chromium |
| **Installation** | MSI installer plus direct DLL reference | Single NuGet package |
| **Platforms** | Windows (Office often required for doc conversion) | Windows, Linux, macOS, Docker |
| **Async API** | Callback-based | Native `async`/`await` |
| **.NET Support** | .NET Framework and limited .NET Core via `.NetCore.dll` | .NET Framework 4.6.2+, .NET 6/7/8/9/10 |
| **Support** | Commercial vendor support via Apryse | Engineering support (24/5; 24/7 Premium) |

---

## Code Comparison: Common Operations

### BCL easyPDF SDK — HTML to PDF Conversion

```csharp
// easyPDF SDK is MSI-installed; reference
// BCL.easyPDF.PDFConverter.dll (.NET Framework) or
// BCL.easyPDF.PDFConverter.NetCore.dll (.NET Core).
using BCL.easyPDF;
using System;

class Program
{
    static void Main()
    {
        var pdf = new PDFDocument();
        var htmlConverter = new HTMLConverter();
        htmlConverter.ConvertHTML("<h1>Hello World</h1>", pdf);
        pdf.Save("output.pdf");
        pdf.Close();
    }
}
```

Notes on this pipeline:

- The SDK is installed by MSI, which also registers COM components and a virtual printer driver. External CSS, JavaScript, and image references may need explicit path handling.
- The exception surface mixes file I/O, COM, and printer-driver errors, which can make HTML-rendering issues harder to isolate.
- The conversion call is synchronous; integrate it with `Task.Run` if you need to keep request threads free.
- Confirm support for print CSS, custom headers/footers, and page-break controls against your installed SDK version.

### IronPDF — HTML to PDF Conversion

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlFileAsPdf("invoice.html");
pdf.SaveAs("output.pdf");
```

The [HTML File to PDF guide](https://ironpdf.com/how-to/html-file-to-pdf/) covers how `RenderHtmlFileAsPdf` resolves relative paths for CSS, JavaScript, and images automatically. For HTML strings, the [HTML String to PDF tutorial](https://ironpdf.com/how-to/html-string-to-pdf/) shows how to set `BaseUrlOrPath` for asset context.

---

### BCL easyPDF SDK — PDF Merging

```csharp
using BCL.easyPDF;
using System;

class Program
{
    static void Main()
    {
        var pdf1 = new PDFDocument("document1.pdf");
        var pdf2 = new PDFDocument("document2.pdf");
        pdf1.Append(pdf2);
        pdf1.Save("merged.pdf");
        pdf1.Close();
        pdf2.Close();
    }
}
```

Notes on this pattern:

- Merging is file-based via `PDFDocument.Append`. Confirm in-memory / stream support against your installed version.
- Page-range extraction is done through `ExtractPages(start, end)` using 1-based indexing.
- Verify how bookmarks, form fields, and annotations are preserved when merging documents that originated outside easyPDF.

### IronPDF — PDF Merging

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System.Collections.Generic;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdfs = new List<PdfDocument>
{
    PdfDocument.FromFile("document1.pdf"),
    PdfDocument.FromFile("document2.pdf")
};
var merged = PdfDocument.Merge(pdfs);
merged.SaveAs("merged.pdf");
```

The [Merge & Split tutorial](https://ironpdf.com/how-to/merge-or-split-pdfs/) demonstrates additional scenarios: merging specific page ranges, combining HTML-generated PDFs with existing files, and adding cover pages during a merge.

---

### BCL easyPDF SDK — URL to PDF

```csharp
using BCL.easyPDF;
using System;

class Program
{
    static void Main()
    {
        var pdf = new PDFDocument();
        var htmlConverter = new HTMLConverter();
        htmlConverter.ConvertURL("https://example.com", pdf);
        pdf.Save("webpage.pdf");
        pdf.Close();
    }
}
```

### IronPDF — URL to PDF

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

---

### IronPDF — Adding Watermarks

Watermark support in the easyPDF SDK is exposed through `PDFDocument` and `PDFProcessor` APIs; consult the [easyPDF SDK user manual](https://www.pdfonline.com/Easypdf/sdk/usermanual/) for the exact method names in your installed version. The IronPDF equivalent uses HTML directly:

```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("document.pdf");
pdf.ApplyWatermark("<h2 style='color:gray;opacity:0.5'>CONFIDENTIAL</h2>",
    rotation: 45,
    verticalAlignment: VerticalAlignment.Middle,
    horizontalAlignment: HorizontalAlignment.Center);
pdf.SaveAs("watermarked.pdf");
```

The [PDF Watermarking guide](https://ironpdf.com/how-to/custom-watermark/) shows how HTML-based watermarks support web fonts, images, CSS styling, and precise positioning. Dynamic watermarks can include page numbers, dates, or custom logic.

---

### IronPDF — Form Filling

```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("template.pdf");
var form = pdf.Form;

form.FindFormField("CustomerName").Value = "Acme Corp";
form.FindFormField("InvoiceDate").Value = DateTime.Now.ToString("yyyy-MM-dd");
form.FindFormField("TotalAmount").Value = "1250.00";

pdf.SaveAs("filled-form.pdf");
```

The [PdfDocument API reference](https://ironpdf.com/object-reference/api/IronPdf.PdfDocument.html) covers the `Form` property for AcroForm fields, including value assignment and flattening. For form-filling on the easyPDF side, check the vendor manual for the exact field-access pattern in your installed SDK version, and confirm AcroForm vs XFA coverage if your templates use XFA.

---

## API Mapping Reference

| Operation | BCL easyPDF SDK | IronPDF |
|-----------|-----------------|---------|
| Initialize converter | `new HTMLConverter()` (or `Printer` for the legacy printer-driver path) | `new ChromePdfRenderer()` |
| HTML string to PDF | `htmlConverter.ConvertHTML(html, pdfDoc)` | `RenderHtmlAsPdf(html)` |
| URL to PDF | `htmlConverter.ConvertURL(url, pdfDoc)` | `RenderUrlAsPdf(url)` |
| Open existing PDF | `new PDFDocument("file.pdf")` | `PdfDocument.FromFile(path)` |
| Merge PDFs | `pdf1.Append(pdf2)` | `PdfDocument.Merge(pdf1, pdf2)` |
| Extract pages | `doc.ExtractPages(start, end)` (1-based) | `pdf.CopyPages(start, end)` (0-based) |
| Save | `doc.Save(path)` | `pdf.SaveAs(path)` |
| Close / dispose | `doc.Close()` | `pdf.Dispose()` or `using` |
| Extract text | `doc.ExtractText()` | `ExtractAllText()` |
| Set user password | `doc.SetPassword(pwd)` | `pdf.SecuritySettings.UserPassword = "..."` |
| Set owner password | `doc.SetOwnerPassword(pwd)` | `pdf.SecuritySettings.OwnerPassword = "..."` |
| Form field access | Consult vendor manual | `Form.FindFormField(name)` |

---

## Comprehensive Feature Comparison

### Status

| Feature | BCL easyPDF SDK | IronPDF |
|---------|-----------------|---------|
| Active development | Maintained for existing customers under Apryse | Monthly releases |
| .NET 8 support | Confirm with vendor | Yes |
| .NET 9 support | Confirm with vendor | Yes |
| .NET 10 support | Confirm with vendor | Yes |
| .NET Framework 4.6.2+ | Yes | Yes |
| Linux support | Not in scope historically | Yes |
| macOS support | Not in scope historically | Yes |
| NuGet availability | No (MSI + direct DLL reference) | Yes (`IronPdf`) |

### Support

| Feature | BCL easyPDF SDK | IronPDF |
|---------|-----------------|---------|
| Documentation | Vendor user manual via Apryse / pdfonline.com | Comprehensive |
| Code examples | Vendor manual and KB | 100+ examples |
| Community forum | Vendor channel | Active forum |
| Technical support | Commercial via Apryse | Engineering support |
| Response time | Confirm SLA with vendor | < 1 minute median (chat) |

### Content Creation

| Feature | BCL easyPDF SDK | IronPDF |
|---------|-----------------|---------|
| HTML5 support | Confirm against installed version | Full Chromium |
| CSS3 support | Limited compared to a modern browser engine | All CSS3 features |
| JavaScript execution | Limited compared to a modern browser engine | Full JS engine |
| Web fonts | Confirm support | Google Fonts, custom |
| SVG rendering | Confirm support | Native |
| Canvas / WebGL | Confirm support | Via Chromium |
| Responsive layouts | Confirm media query handling | Media queries |
| Print CSS | Confirm support | `@media print` |

### PDF Operations

| Feature | BCL easyPDF SDK | IronPDF |
|---------|-----------------|---------|
| Merge PDFs | Yes (`Append`) | Yes |
| Split PDFs | Yes (`ExtractPages`, 1-based) | Yes (`CopyPages`, 0-based) |
| Extract pages | Yes | Yes |
| Rotate pages | Yes | Yes |
| Extract text | Yes | Yes |
| Extract images | Confirm support | Yes |
| Form filling | Confirm AcroForm / XFA coverage | AcroForm |
| Form flattening | Confirm support | Yes |
| Headers / footers | Confirm support | HTML-based |
| Page numbers | Confirm support | Dynamic |

### Security

| Feature | BCL easyPDF SDK | IronPDF |
|---------|-----------------|---------|
| Password encryption | Supported via `SetPassword` / `SetOwnerPassword` | 128 / 256-bit AES |
| User permissions | Granular via `SetPrintPermission`, `SetCopyPermission`, etc. | Granular |
| Digital signatures | Confirm PAdES support with vendor | X.509 |
| Redaction | Confirm support | Permanent |
| Metadata removal | Confirm support | Yes |

### Development

| Feature | BCL easyPDF SDK | IronPDF |
|---------|-----------------|---------|
| NuGet installation | No (MSI + direct DLL reference) | One package |
| External dependencies | MSI installer, virtual printer driver, COM components; Office often needed for doc conversion | None |
| Async API | Callback-based (`BeginPrintToFile`) | Native `async`/`await` |
| Container deployment | Not in scope historically | Docker / Kubernetes |

---

## Installation Comparison

### BCL easyPDF SDK Installation

There is no NuGet package. Installation flow:

```text
1. Run the easyPDF SDK MSI installer (registers virtual printer + COM types)
2. Add a direct DLL <Reference> in your .csproj:
   - BCL.easyPDF.PDFConverter.dll       (.NET Framework)
   - BCL.easyPDF.PDFConverter.NetCore.dll (.NET Core)
3. Ensure the MSI is also installed on every target server
```

```csharp
using BCL.easyPDF;
```

### IronPDF Installation

```bash
dotnet add package IronPdf
```

```csharp
using IronPdf;
using IronPdf.Rendering;
```

Dependency comparison:

- **BCL easyPDF SDK** — MSI installer, virtual printer driver, COM components, and (for Office-document conversion) Microsoft Office on the host.
- **IronPDF** — single NuGet package, cross-platform binaries bundled, no Office or printer driver required.

---

## When Migration Becomes Mandatory

Teams typically hit forcing functions around BCL easyPDF SDK when:

**Platform evolution.** Moving to .NET 8+ or to containerized Linux deployments where COM, virtual printer drivers, and MSI installers create friction. If the library assumes Windows-specific COM objects and an interactive printer driver, container and serverless targets compound the cost with every delayed sprint.

**Modern HTML requirements.** When product designs assume CSS Grid, web fonts, or JavaScript-generated charts will "just work" in PDFs. Simplifying HTML to fit a non-browser rendering engine creates technical debt — every new design requires translation rather than direct implementation.

**Support and roadmap clarity.** Following the Apryse acquisition, confirm with the vendor that easyPDF SDK is the right product line for your time horizon, versus a migration onto the broader Apryse PDF SDK or another vendor.

Migration carries real costs for established codebases: regression-testing existing PDFs, updating templates, verifying output consistency across document types. Those costs should be weighed against the accumulating workarounds required to handle modern web standards with a legacy pipeline.

## IronPDF's Technical Approach

IronPDF uses a headless Chromium engine for PDF generation, so HTML rendering happens through the same engine that powers Chrome. This eliminates the "it works in the browser but not in the PDF" class of bugs.

The rendering pipeline is: HTML/CSS/JavaScript inputs → Chromium rendering → `PdfDocument`. The [HTML to PDF tutorial](https://ironpdf.com/how-to/html-string-to-pdf/) covers rendering options including JavaScript execution timing, print-CSS handling, and asset-loading strategies.

For document manipulation, the `PdfDocument` class provides methods for merging, splitting, security, content extraction, and modification of existing PDFs. Because generation and manipulation share the same API surface, workflows like "generate invoice from HTML, merge with terms PDF, add watermark, encrypt" become straightforward chains rather than tool-switching operations.

---

**What's been your experience with .NET PDF libraries when modern HTML features hit a legacy rendering pipeline? Drop a comment with the workaround that finally pushed you to evaluate alternatives.**

**Learn more:**

- [HTML to PDF with Chrome rendering](https://ironpdf.com/how-to/html-string-to-pdf/)
- [Merge and split PDF documents](https://ironpdf.com/how-to/merge-or-split-pdfs/)
- [Migrate from BCL easyPDF SDK to IronPDF](https://ironpdf.com/blog/migration-guides/migrate-from-bcl-easypdf-sdk-to-ironpdf/)
