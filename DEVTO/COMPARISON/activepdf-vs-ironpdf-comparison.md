---
title: "ActivePDF vs IronPDF: what the docs do not tell you"
published: false
tags: dotnet, csharp, pdf, comparison
canonical_url: https://ironsoftware.com/csharp/pdf/blog/compare-to-competitors/
---

A typical scenario: a .NET team picked ActivePDF Toolkit years ago for PDF form filling and merging, then later needed HTML email templates rendered to PDF. Toolkit itself does not render HTML — that capability lives in a separate product (WebGrabber), with separate licensing and a separate install. What looked like adding a feature turned into a procurement and integration exercise across two SKUs.

ActivePDF (now part of Apryse, after PDFTron acquired ActivePDF in June 2020 and rebranded to Apryse in February 2023) is a mature enterprise PDF suite split across several products. IronPDF bundles HTML-to-PDF and PDF manipulation into a single library. This comparison looks at where the modular approach earns its complexity and where unified tooling reduces it.

## Understanding IronPDF

[IronPDF](https://ironpdf.com) combines HTML-to-PDF conversion and PDF manipulation in a single library. Install via `Install-Package IronPdf`, no additional components needed. The `ChromePdfRenderer` handles HTML rendering with an embedded Chromium engine, while the `PdfDocument` class provides merge, split, security, and form operations — all in one namespace.

For teams, this means one procurement process, one license, one set of documentation, and one support channel. HTML conversion, PDF editing, form filling, and digital signatures sit behind a single API surface.

## Key Characteristics of ActivePDF

### Product Status

ActivePDF Toolkit is actively maintained as a commercial enterprise product. The current Toolkit package on nuget.org is `ActivePDF.Toolkit` (v11.4.4, published December 2025). The brand is owned by Apryse and still ships under its original product names: Toolkit (manipulation), WebGrabber (HTML-to-PDF), DocConverter (Office conversion), Server (PostScript), and Toolkit Ultimate (combined features).

### Modular Architecture

ActivePDF is organized as several products rather than a single library. Toolkit handles PDF creation and manipulation but does not render HTML. HTML-to-PDF requires WebGrabber. Office document conversion requires DocConverter. Each component has its own installation, licensing, and configuration.

Cross-platform support varies by component — Toolkit and WebGrabber are documented as Windows-focused. Modern .NET range (.NET 5 through .NET 10) support should be verified per component against current Apryse/ActivePDF documentation.

### Integration Considerations

Teams using multiple ActivePDF components coordinate versions across SKUs and manage component-specific configuration. Starting with Toolkit 10, native libraries are no longer auto-copied to the system folder, so the `Toolkit` constructor often needs an explicit `CoreLibPath` pointing at the installed directory — relevant for Docker, CI, and any deployment without the installer.

The WebGrabber HTML rendering engine differs from a modern Chromium pipeline; teams porting CSS-heavy templates typically validate rendering against the target Toolkit/WebGrabber version.

### Support

Apryse provides commercial support for ActivePDF products. Documentation is organized per product. Pricing is structured per-component, so the total cost of ownership depends on which products are required for the workflow.

### Architectural Notes

The multi-product model adds decision points: which component for which task, how components interact, version compatibility across components, separate procurement per module. For teams that need HTML-to-PDF plus PDF manipulation, the working set is at least two products (WebGrabber + Toolkit) versus a single library.

ActivePDF products are primarily Windows-focused; containerization typically uses Windows containers, and Azure App Service requires a Windows plan.

## Feature Comparison Overview

| Aspect | ActivePDF (Toolkit + WebGrabber) | IronPDF |
|--------|----------------------------------|---------|
| **Current Status** | Active (Toolkit v11.4.4, Dec 2025) | Active (regular updates) |
| **HTML Rendering** | WebGrabber (separate product) | Built-in Chromium engine |
| **Installation** | Multi-component (Toolkit + WebGrabber) | Single NuGet package |
| **API Style** | Integer return codes, stateful | Exceptions, fluent |
| **Support** | Commercial (Apryse) | Commercial (Iron Software) |
| **Cross-platform** | Primarily Windows | Windows/Linux/macOS |

---

## PDF Form Filling and Merging

### ActivePDF — Toolkit for PDF Manipulation

```csharp
using APToolkitNET;
using System;
using System.Collections.Generic;
using System.IO;

public class ActivePdfFormProcessor
{
    public byte[] FillFormAndMerge(
        string templatePath,
        Dictionary<string, string> formData,
        string attachmentPath)
    {
        using (var toolkit = new Toolkit())
        {
            // Open output PDF (returns 0 on success).
            int result = toolkit.OpenOutputFile("output.pdf");
            if (result != 0)
            {
                throw new Exception($"Failed to create output: {result}");
            }

            // Open input template.
            result = toolkit.OpenInputFile(templatePath);
            if (result != 0)
            {
                throw new Exception($"Failed to open template: {result}");
            }

            // Copy all pages from input to output.
            result = toolkit.CopyForm(-1, 0);
            if (result != 0)
            {
                throw new Exception($"Failed to copy form: {result}");
            }

            // Fill form fields by name.
            foreach (var field in formData)
            {
                result = toolkit.SetFormFieldData(field.Key, field.Value, 0, 1);
                if (result != 0)
                {
                    Console.WriteLine($"Warning: Field '{field.Key}' not set");
                }
            }

            toolkit.CloseInputFile();

            // Merge an additional PDF into the open output file.
            if (!string.IsNullOrEmpty(attachmentPath))
            {
                // MergeFile(FileName, StartPage, EndPage); -1 means "to end".
                toolkit.MergeFile(attachmentPath, 1, -1);
            }

            toolkit.CloseOutputFile();

            return File.ReadAllBytes("output.pdf");
        }
    }
}

// Usage
var processor = new ActivePdfFormProcessor();
var formData = new Dictionary<string, string>
{
    { "CustomerName", "Acme Corp" },
    { "InvoiceNumber", "INV-2026-001" },
    { "Amount", "$1,500.00" }
};

byte[] pdf = processor.FillFormAndMerge(
    "invoice_template.pdf",
    formData,
    "terms_conditions.pdf");
```

**API characteristics to plan around:**

1. **Integer return codes**: Toolkit methods return `int` status codes rather than throwing — every operation must be checked.
2. **File-based pipeline**: `OpenOutputFile`/`OpenInputFile` operate on disk paths; stream-first workflows need adaptation.
3. **Explicit open/close pairing**: `CloseInputFile` and `CloseOutputFile` must be called to finalize the output file.
4. **Stateful API**: The `Toolkit` instance carries state between open/copy/close calls.
5. **Field-name strings**: `SetFormFieldData` takes string field names with no compile-time validation.

### IronPDF — Unified Form and Merge Operations

```csharp
using IronPdf;
using System.Collections.Generic;
using System.Threading.Tasks;

public class IronPdfFormProcessor
{
    public async Task<byte[]> FillFormAndMergeAsync(
        string templatePath,
        Dictionary<string, string> formData,
        string attachmentPath)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Load template PDF.
        using var templatePdf = PdfDocument.FromFile(templatePath);

        // Fill form fields.
        var form = templatePdf.Form;
        foreach (var field in formData)
        {
            if (form.Fields.ContainsKey(field.Key))
            {
                form.Fields[field.Key].Value = field.Value;
            }
        }

        // Load attachment and merge.
        using var attachmentPdf = PdfDocument.FromFile(attachmentPath);
        var merged = PdfDocument.Merge(templatePdf, attachmentPdf);

        return merged.BinaryData;
    }
}

// Usage
var processor = new IronPdfFormProcessor();
var formData = new Dictionary<string, string>
{
    { "CustomerName", "Acme Corp" },
    { "InvoiceNumber", "INV-2026-001" },
    { "Amount", "$1,500.00" }
};

byte[] pdf = await processor.FillFormAndMergeAsync(
    "invoice_template.pdf",
    formData,
    "terms_conditions.pdf");
```

IronPDF uses standard .NET patterns: exceptions for errors, stream-based operations, automatic disposal via `using`, and async/await on rendering. Form fields are exposed via a dictionary on `PdfDocument.Form`. See the [PDF generation settings](https://ironpdf.com/examples/pdf-generation-settings/) reference for more.

---

## HTML to PDF Conversion Requires WebGrabber

### ActivePDF — Separate WebGrabber Component

```csharp
// HTML-to-PDF in ActivePDF is handled by the WebGrabber product, not Toolkit.
// NuGet: Install-Package ActivePDF.WebGrabber
// Docs: https://documentation.activepdf.com/webgrabber_api/
using APWebGrabber;
using System;
using System.IO;

public class ActivePdfHtmlConverter
{
    public byte[] ConvertHtmlToPdf(string html)
    {
        WebGrabber wg = new WebGrabber();

        // WebGrabber renders from a URL or HTML file path; write the HTML to a temp file first.
        string tempHtml = Path.Combine(Path.GetTempPath(), "input.html");
        File.WriteAllText(tempHtml, html);

        wg.URL = tempHtml;
        wg.OutputDirectory = Path.GetTempPath();
        wg.OutputFilename = "output.pdf";

        // ConvertToPDF returns 0 on success.
        if (wg.ConvertToPDF() == 0)
        {
            return File.ReadAllBytes(Path.Combine(Path.GetTempPath(), "output.pdf"));
        }

        throw new Exception("WebGrabber conversion failed");
    }
}
```

**Modular architecture characteristics:**

1. **Separate product license**: WebGrabber is licensed independently of Toolkit.
2. **Component installation**: WebGrabber ships its own installer in addition to (or instead of) NuGet.
3. **Version coordination**: Toolkit and WebGrabber versions need to stay compatible across deployments.
4. **Deployment surface**: Both components must be present in server/CI environments.
5. **Windows-focused**: WebGrabber is documented for Windows environments.

### IronPDF — Integrated HTML Rendering

```csharp
using IronPdf;
using System.Threading.Tasks;

public class IronPdfHtmlConverter
{
    public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}

// Usage
var converter = new IronPdfHtmlConverter();
string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial; padding: 20px; }
        .invoice { border: 1px solid #ccc; padding: 15px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
    </style>
</head>
<body>
    <div class='invoice'>
        <h1>Invoice</h1>
        <table>
            <tr><th>Item</th><th>Amount</th></tr>
            <tr><td>Service</td><td>$1,500</td></tr>
        </table>
    </div>
</body>
</html>";

byte[] pdf = await converter.ConvertHtmlToPdfAsync(html);
```

IronPDF includes HTML rendering in the base package — there is no separate WebGrabber-equivalent component to purchase. See the [HTML string to PDF guide](https://ironpdf.com/how-to/html-string-to-pdf/).

---

## Office Document Conversion Requires DocConverter

### ActivePDF — Separate DocConverter Component

```csharp
// DocConverter is a separate ActivePDF product for Word/Excel/PowerPoint to PDF conversion.
// Separate installation, licensing, and configuration; refer to vendor docs for the current API:
// https://documentation.activepdf.com/docconverter/

public class ActivePdfOfficeConverter
{
    public byte[] ConvertOfficeToPdf(string officePath)
    {
        // The typical workflow is:
        // 1. Initialize DocConverter
        // 2. Configure conversion options
        // 3. Open the Office file
        // 4. Export to PDF
        // 5. Read the generated file
        //
        // Method names and configuration depend on the DocConverter version
        // in use; consult the current ActivePDF DocConverter documentation.

        throw new NotImplementedException(
            "DocConverter requires a separate product license and installation. " +
            "See ActivePDF DocConverter documentation for current API details.");
    }
}
```

**Multi-component considerations:**

1. **Three SKUs for full coverage**: Toolkit + WebGrabber + DocConverter cover PDF manipulation, HTML rendering, and Office conversion respectively.
2. **Per-component licensing**: Each product is licensed independently.
3. **Three installation paths**: Each component ships its own installer/configuration.
4. **Compatibility matrix**: Component versions need to be validated together.
5. **Office runtime requirement**: DocConverter relies on Microsoft Office or LibreOffice automation on the server, depending on configuration.

### IronPDF — HTML-Based Workflow for Office Content

```csharp
using IronPdf;
using System.Threading.Tasks;

public class IronPdfWorkflow
{
    public async Task<byte[]> GenerateFromDataAsync()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // IronPDF does not convert Office files directly.
        // The common pattern is to render data as HTML and convert to PDF.
        string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial; }
        table { width: 100%; border-collapse: collapse; }
        th, td { border: 1px solid #000; padding: 5px; }
    </style>
</head>
<body>
    <h1>Report</h1>
    <table>
        <tr><th>Column 1</th><th>Column 2</th></tr>
        <tr><td>Data A</td><td>Data B</td></tr>
    </table>
</body>
</html>";

        var renderer = new ChromePdfRenderer();
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}
```

IronPDF does not convert Office files directly; teams typically render data as HTML templates. Trade-off: HTML/CSS skill instead of Office templates, in exchange for no Office runtime or COM automation and cross-platform deployment. See the [pixel-perfect HTML to PDF documentation](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/).

---

## API Mapping Reference

| ActivePDF Concept | IronPDF Equivalent |
|-------------------|-------------------|
| `APToolkitNET.Toolkit` class | `PdfDocument` class |
| `OpenInputFile()` | `PdfDocument.FromFile()` |
| `OpenOutputFile()` | `new PdfDocument()` or result of rendering |
| `CopyForm()` | `PdfDocument.Merge()` |
| `SetFormFieldData(name, value, ...)` | `pdf.Form.Fields[name].Value = value` |
| `CloseInputFile()` / `CloseOutputFile()` | `Dispose()` (automatic with `using`) |
| Return code error handling | Exception-based errors |
| WebGrabber (separate product) | `ChromePdfRenderer` (built-in) |
| DocConverter (separate product) | HTML-based workflow (no direct Office conversion) |
| Multiple product licenses | Single license for all features |
| DLL via `CoreLibPath` / Program Files | NuGet package management |
| Primarily Windows | Cross-platform (Windows/Linux/macOS) |
| File-based operations | Stream and file support |
| State-based API | Stateless rendering |

---

## Comprehensive Feature Comparison

| Feature Category | ActivePDF | IronPDF |
|------------------|-----------|---------|
| **Status** | | |
| Maintenance Status | Active (Toolkit v11.4.4, Dec 2025) | Active |
| Product Architecture | Modular (multiple products) | Unified library |
| **Support** | | |
| Commercial Support | Yes (Apryse) | Yes (Iron Software) |
| Documentation | Per-product | Unified |
| **Content Creation** | | |
| HTML to PDF | WebGrabber (separate) | Built-in `ChromePdfRenderer` |
| Office to PDF | DocConverter (separate) | HTML workflow (no direct Office) |
| PDF from Scratch | Toolkit | `PdfDocument` API |
| Form Filling | Toolkit | Built-in `Form` API |
| PDF Merging | Toolkit | Built-in `Merge()` |
| **PDF Operations** | | |
| Create PDFs | Toolkit | Yes |
| Merge PDFs | Toolkit | Yes |
| Split PDFs | Toolkit | Yes |
| Extract Text | Toolkit Ultimate | Yes |
| Extract Images | Toolkit Ultimate | Yes |
| Add Watermarks | Toolkit | Yes |
| Digital Signatures | Toolkit | Yes |
| Encryption | Toolkit | Yes |
| Compression | Toolkit | Yes |
| **Security** | | |
| Password Protection | Toolkit | Yes |
| AES Encryption | Toolkit | Yes (up to 256-bit) |
| Digital Signatures | Toolkit | Yes |
| Permissions | Toolkit | Yes |
| **Licensing & Deployment** | | |
| Licensing Model | Per-component | Single license |
| Installations | Toolkit + WebGrabber + DocConverter as needed | One NuGet package |
| Native Library Management | `CoreLibPath` / installer | NuGet-managed |
| Version Coordination | Across components | Single version |
| **Development** | | |
| .NET Framework | 4.5+ (per package metadata) | 4.6.2+ |
| .NET 5+ / .NET 9-10 | Verify per component | .NET 5 through .NET 10 |
| Cross-Platform | Primarily Windows | Windows/Linux/macOS |
| API Style | Return codes, state-based | Exceptions, fluent |
| Async Support | Verify per component | Full async/await |

---

## Installation Comparison

**ActivePDF:**

```bash
# Toolkit (PDF manipulation)
Install-Package ActivePDF.Toolkit

# For HTML-to-PDF, also install WebGrabber:
Install-Package ActivePDF.WebGrabber
# (and license/install separately per Apryse documentation)

# For Office conversion, DocConverter is a separate product;
# installation and licensing are handled outside NuGet.
```

```csharp
using APToolkitNET;

using (Toolkit toolkit = new Toolkit())
{
    if (toolkit.OpenOutputFile("output.pdf") == 0)
    {
        // ... operations with integer return-code checks ...
        toolkit.CloseOutputFile();
    }
}
```

**IronPDF:**

```bash
Install-Package IronPdf
# HTML-to-PDF, manipulation, forms, and security included.
```

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("output.pdf");
```

---

## Conclusion

ActivePDF Toolkit is a mature commercial product with a long history of serving enterprise PDF workflows, particularly around high-volume form filling, merging, and server-side manipulation. For organizations already running ActivePDF with existing licenses, installer-managed deployments, and Windows infrastructure, the investment continues to pay off.

The modular architecture is the trade-off: HTML-to-PDF requires WebGrabber, Office conversion requires DocConverter, and end-to-end PDF workflows often span at least two SKUs. That implies multiple license purchases, separate installation steps, version coordination across components, and Windows-centric deployment.

Migration from ActivePDF to IronPDF tends to be worth evaluating when:

- An HTML-to-PDF requirement appears and adding WebGrabber procurement/integration is heavier than swapping libraries.
- Cross-platform deployment (Linux, Docker, Kubernetes) becomes a requirement.
- Multi-component procurement timelines create delays.
- The team prefers standard .NET patterns (async/await, exceptions, fluent API) over integer return codes and explicit open/close.
- Single-vendor, single-license simplicity is a goal.

IronPDF provides HTML-to-PDF and PDF manipulation in one library, with a single license and a single install. The [ChromePdfRenderer API](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderer.html/) uses Chromium for HTML rendering, while `PdfDocument` handles manipulation. Cross-platform support is native.

For new projects, the choice typically comes down to whether enterprise modularity (ActivePDF's strength) or unified developer tooling (IronPDF's approach) better fits the team's procurement, deployment, and platform constraints.

**If you have run ActivePDF in production, which components did you end up combining — and did the procurement story shape that decision?**

**Related Resources:**

- [IronPDF HTML to PDF Conversion Guide](https://ironpdf.com/how-to/html-string-to-pdf/)
- [PDF Generation Settings Documentation](https://ironpdf.com/examples/pdf-generation-settings/)
