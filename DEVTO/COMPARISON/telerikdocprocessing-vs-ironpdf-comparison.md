---
title: "Telerik Document Processing vs IronPDF: the real-world comparison for 2026"
published: false
tags: dotnet, csharp, pdf, comparison
---
Consider a common architectural question: your application needs to generate PDF reports from HTML dashboards, and Telerik Document Processing is on the shortlist because it advertises HTML support. The architectural detail worth understanding up front is that `HtmlFormatProvider` imports HTML into a `RadFlowDocument`—an intermediate word-processing model—before `PdfFormatProvider` exports that flow document to PDF. Modern CSS features that have no equivalent in a flow-document model (CSS Grid, advanced Flexbox, absolute positioning) typically do not round-trip cleanly through this two-step conversion. That is not a defect; it is the architectural shape of the library.

Telerik Document Processing's `RadPdfProcessing` (part of the broader Progress Telerik suite, distinct from Telerik Reporting) is fundamentally designed for programmatic PDF construction—drawing shapes, positioning text, building tables from data structures—rather than for preserving arbitrary HTML layouts. Understanding that architectural distinction up front saves teams the loop of debugging why HTML that renders cleanly in a browser does not survive the flow-document intermediary unchanged.

## Understanding IronPDF

IronPDF uses Chromium—the same rendering engine that powers Google Chrome—to convert HTML directly to PDF. There's no intermediate format, no flow document translation, no layout model conversion. The HTML you write renders identically to Chrome's print preview because it's literally using Chrome's rendering pipeline. CSS Grid works, Flexbox works, modern JavaScript executes, web fonts load—everything that works in a browser works in the PDF.

This direct rendering approach eliminates the troubleshooting loop of "why did my layout break during conversion?" If it displays correctly in Chrome, it will appear identically in the PDF. For teams building from HTML templates, this architectural match between development environment (browser) and output (PDF) determines whether layout debugging is straightforward or perpetually mysterious. See the [HTML to PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) for implementation patterns.

## Architectural Considerations for Telerik Document Processing

### Product Status

Telerik Document Processing is actively developed as part of Progress Telerik's broader UI/document suite. Release cadence, supported .NET versions, and the exact mix of features in any given release vary over time; for current specifics consult the [Telerik release notes](https://www.telerik.com/support/whats-new/document-processing-libraries) directly. The product is commercially supported as part of Telerik's bundled offerings.

### HTML Conversion Model

**Two-step HTML pipeline**: Telerik's HTML conversion follows a documented two-step process. `HtmlFormatProvider` imports HTML into a `RadFlowDocument` (a word-processing flow model), then `PdfFormatProvider` exports that flow document to PDF. Each step has its own representational constraints:

1. HTML elements map to flow-document equivalents (sections, paragraphs, tables, runs)
2. CSS properties map to whatever the flow model can express
3. The flow model's layout primitives bound what HTML structures can be preserved

In common configurations, modern HTML/CSS features that have no flow-document equivalent — CSS Grid, advanced Flexbox, transforms, animations, complex absolute positioning — do not round-trip through this pipeline. Verify against your specific version and content.

**JavaScript execution**: `HtmlFormatProvider` parses static HTML. There is no browser engine in the pipeline, so JavaScript-driven content (SPAs, AJAX-populated sections, client-side templating) is not part of what gets imported.

### Debugging Surface

**Two-layer mental model**: When HTML output looks wrong, the cause may sit in the HTML → flow-document import, in the flow-document → PDF export, or in the interaction between them. Debugging benefits from familiarity with both `RadFlowDocument` (word-processing model) and `RadFixedDocument` (PDF fixed model).

**Package layout**: A complete HTML-to-PDF setup typically pulls in several Telerik packages — for example `Telerik.Documents.Core`, `Telerik.Documents.Fixed`, `Telerik.Documents.Flow`, `Telerik.Documents.Flow.FormatProviders.Html`, `Telerik.Documents.Flow.FormatProviders.Pdf`, and `Telerik.Documents.ImageUtils` for image handling. Each contributes its own dependencies and version-alignment requirements.

**Flow-to-fixed pagination**: The flow-to-fixed conversion applies automatic pagination, text reflow, and layout adjustments. Content positioning in the final PDF reflects the flow engine's choices rather than literal HTML/CSS coordinates.

### Support

Telerik provides commercial support through a ticketing system. Documentation is comprehensive for the programmatic API; HTML-conversion edge cases are less heavily documented because programmatic construction is the library's primary design center.

### Architectural Summary

`RadPdfProcessing` and its sibling flow/spreadsheet/word libraries are designed first for programmatic document construction, with HTML import as a secondary convenience rather than a Chromium-class renderer. Teams whose workflows are HTML-template-first generally find that they are leaning on a secondary feature rather than the primary design.

## Feature Comparison Overview

| Aspect | Telerik Document Processing | IronPDF |
|--------|----------------------------|---------|
| **Current Status** | Active commercial product | Active commercial product |
| **HTML Approach** | Flow-document import + PDF export | Chromium rendering engine |
| **HTML Fidelity Model** | Bounded by flow-document representation | Bounded by Chromium |
| **Installation** | Multiple packages | Single package (includes Chromium) |
| **Support** | Commercial ticketing | Commercial engineering support |
| **Primary Design Focus** | Programmatic document construction | HTML-to-PDF rendering |

## Code Comparison

### Telerik Document Processing — HTML via Flow Document

```csharp
// NuGet: Install-Package Telerik.Documents.Flow
// NuGet: Install-Package Telerik.Documents.Flow.FormatProviders.Pdf
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class TelerikHtmlConverter
{
    public void ConvertHtmlStringToPdf(string htmlContent, string outputPath)
    {
        // Step 1: HTML -> RadFlowDocument (flow document model)
        HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
        RadFlowDocument flowDocument = htmlProvider.Import(htmlContent);

        // Step 2: RadFlowDocument -> PDF
        PdfFormatProvider pdfProvider = new PdfFormatProvider();
        using (FileStream output = File.OpenWrite(outputPath))
        {
            pdfProvider.Export(flowDocument, output);
        }
    }

    public async Task ConvertUrlToPdfAsync(string url, string outputPath)
    {
        // No browser engine in this pipeline; HTML is fetched externally
        using (var httpClient = new HttpClient())
        {
            string html = await httpClient.GetStringAsync(url);

            // External resources are resolved via an explicit hook
            HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
            htmlProvider.ImportSettings.LoadFromUri += (sender, args) =>
            {
                // Application code resolves relative URLs and fetches bytes
            };

            RadFlowDocument flowDocument = htmlProvider.Import(html);

            PdfFormatProvider pdfProvider = new PdfFormatProvider();
            using (FileStream output = File.OpenWrite(outputPath))
            {
                pdfProvider.Export(flowDocument, output);
            }
        }
    }

    public void ConvertComplexHtml(string outputPath)
    {
        // HTML with CSS Grid and Flexbox
        string html = @"
            <style>
                .grid-container {
                    display: grid;
                    grid-template-columns: 1fr 2fr 1fr;
                    gap: 20px;
                }
                .flex-item {
                    display: flex;
                    justify-content: space-between;
                }
            </style>
            <div class='grid-container'>
                <div>Column 1</div>
                <div>Column 2</div>
                <div>Column 3</div>
            </div>";

        HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
        RadFlowDocument flowDocument = htmlProvider.Import(html);

        // The flow-document model does not have a grid primitive;
        // grid-specific layout intent is not preserved through the pipeline.

        PdfFormatProvider pdfProvider = new PdfFormatProvider();
        using (FileStream output = File.OpenWrite(outputPath))
        {
            pdfProvider.Export(flowDocument, output);
        }
    }

    public void ConvertHtmlWithImages(string htmlContent, string outputPath)
    {
        // Image fetching is wired through an explicit hook
        HtmlFormatProvider htmlProvider = new HtmlFormatProvider();

        htmlProvider.ImportSettings.LoadFromUri += (sender, e) =>
        {
            // Application code supplies image bytes for each URI
            if (e.Uri.EndsWith(".jpg") || e.Uri.EndsWith(".png"))
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    e.SetData(client.GetByteArrayAsync(e.Uri).Result);
                }
            }
        };

        RadFlowDocument flowDocument = htmlProvider.Import(htmlContent);

        PdfFormatProvider pdfProvider = new PdfFormatProvider();
        using (FileStream output = File.OpenWrite(outputPath))
        {
            pdfProvider.Export(flowDocument, output);
        }
    }
}
```

**Things to plan for with this pipeline:**

1. **Grid/Flexbox representation**: The flow-document model does not include grid or flex primitives, so layout intent expressed in those CSS features is not preserved through import.

2. **Two-layer mental model**: Debugging benefits from comfort with both `RadFlowDocument` (word-processing model) and `RadFixedDocument` (PDF fixed model).

3. **External resource loading**: Images, CSS files, and fonts are resolved through the `LoadFromUri` hook rather than by a browser engine.

4. **No JavaScript runtime**: Dynamic content (AJAX, SPAs, client-side templating) is not part of what gets imported; only the static HTML is parsed.

5. **Flow-to-fixed pagination**: The flow-to-PDF step applies its own pagination and reflow; positioning in the final PDF reflects the flow engine's choices.

6. **Preview workflow**: Browser print preview is not a 1:1 preview of this pipeline's output, because the flow-document step is a separate layout pass.

### IronPDF — Direct Chromium Rendering

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System.Threading.Tasks;

public class IronPdfHtmlConverter
{
    private readonly ChromePdfRenderer _renderer;

    public IronPdfHtmlConverter()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();
    }

    public async Task<byte[]> ConvertHtmlStringToPdfAsync(string htmlContent)
    {
        // Direct HTML → PDF, no intermediate format
        var pdf = await _renderer.RenderHtmlAsPdfAsync(htmlContent);
        return pdf.BinaryData;
    }

    public async Task<byte[]> ConvertUrlToPdfAsync(string url)
    {
        // Browser engine fetches URL, loads resources, executes JavaScript
        var pdf = await _renderer.RenderUrlAsPdfAsync(url);
        return pdf.BinaryData;
    }

    public async Task<byte[]> ConvertComplexHtmlAsync()
    {
        // Modern CSS works because Chromium supports it
        string html = @"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    .grid-container {
                        display: grid;
                        grid-template-columns: 1fr 2fr 1fr;
                        gap: 20px;
                    }
                    .flex-item {
                        display: flex;
                        justify-content: space-between;
                    }
                </style>
            </head>
            <body>
                <div class='grid-container'>
                    <div>Column 1</div>
                    <div>Column 2</div>
                    <div>Column 3</div>
                </div>
            </body>
            </html>";
        
        var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }

    public async Task<byte[]> ConvertHtmlWithImages(string htmlWithImages)
    {
        // Images load automatically - browser engine handles resource fetching
        var pdf = await _renderer.RenderHtmlAsPdfAsync(htmlWithImages);
        return pdf.BinaryData;
    }
}
```

**Troubleshooting Advantages:**

- **Chrome Preview = PDF Output**: Debug in Chrome's print preview—what you see is exactly what the PDF will contain
- **No Intermediate Format**: One rendering pass, one layout model to understand
- **Automatic Resource Loading**: Images, CSS, fonts load as they would in a browser
- **JavaScript Execution**: Dynamic content, AJAX, SPAs work as expected
- **Modern CSS Support**: Grid, Flexbox, transforms, animations all function
- **Async Operations**: Non-blocking for high-throughput scenarios

For detailed HTML rendering configuration, see the [HTML string to PDF guide](https://ironpdf.com/how-to/html-string-to-pdf/).

## Troubleshooting Common Scenarios

### Scenario 1: CSS Grid Layout Not Preserved

**Telerik Document Processing:**
```csharp
// HTML with CSS Grid
string html = "<style>.grid { display: grid; }</style><div class='grid'>...</div>";
RadFlowDocument flow = htmlProvider.Import(html);

// The flow-document model does not include a grid primitive.
// A common workaround is to express the layout as an HTML <table>,
// which the flow model represents directly.
```

**Why**: The flow-document model is a word-processing abstraction; it does not include a CSS Grid layout primitive.

**IronPDF Equivalent:**
```csharp
// Chromium supports CSS Grid; the same HTML renders directly.
var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
```

### Scenario 2: External Images

**Telerik Document Processing:**
```csharp
// Image fetching is wired through an explicit hook
htmlProvider.ImportSettings.LoadFromUri += (sender, e) =>
{
    // Application code fetches the bytes for each URI
    // and resolves any relative paths against a chosen base
};
```

**Things to check**:
1. `LoadFromUri` is wired up
2. Relative paths resolve against a known base URL
3. Image bytes are supplied in a format the import expects
4. Exceptions inside the handler surface where you can see them

**IronPDF Equivalent:**
```csharp
// The browser engine fetches images, CSS, and fonts as part of rendering.
var pdf = await _renderer.RenderUrlAsPdfAsync("https://example.com");
```

### Scenario 3: Final Layout vs. HTML

**Telerik Document Processing:**
```csharp
// HTML -> Flow Document -> PDF
// Each step applies its own layout pass.
// Final positioning reflects the flow-to-fixed conversion.
```

**Inspection strategies**:
1. Export the flow document to DOCX to see the intermediate representation
2. Check whether the HTML constructs in question have flow-document equivalents
3. Where possible, express layout in flow-friendly structures (e.g., `<table>` rather than CSS Grid)
4. Treat the two-stage pipeline's layout choices as part of the design

**IronPDF Equivalent:**
```csharp
// Chrome print preview is a 1:1 reference for the output.
// HTML -> PDF is a single rendering pass through Chromium.
```

For comprehensive rendering configuration and troubleshooting, see the [ChromePdfRenderer API reference](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderer.html).

## API Mapping Reference

| Telerik Concept | IronPDF Equivalent | Notes |
|-----------------|-------------------|-------|
| `HtmlFormatProvider` | `ChromePdfRenderer` | Import vs. render |
| `RadFlowDocument` | (none) | No intermediate document model |
| `htmlProvider.Import()` | `RenderHtmlAsPdf()` | Parse vs. render |
| `PdfFormatProvider` | Built-in `SaveAs` | Explicit vs. implicit |
| `LoadFromUri` event | Built-in resource loading | Hook vs. browser engine |
| Flow-document model | HTML/CSS model | Word-processing vs. web |
| `Paragraph`/`Run` | HTML elements | Object model vs. markup |
| Table (flow) | HTML `<table>` | Same structure, different API |
| Section breaks | CSS page breaks | Object vs. rule |
| Synchronous `Export` | `Async` variants available | API style |
| Multiple packages | Single package | Modular vs. unified |

## Comprehensive Feature Comparison

| Feature | Telerik Document Processing | IronPDF |
|---------|----------------------------|---------|
| **Status & Support** |
| Active development | Active commercial product | Active commercial product |
| .NET versions | Framework + modern .NET (check current release) | Framework + modern .NET |
| Commercial support | Ticket-based | Engineering support included |
| **HTML Conversion** |
| HTML rendering | Indirect (via flow document) | Direct (Chromium) |
| CSS3 features | Bounded by flow model | Bounded by Chromium |
| CSS Grid | Not represented in flow model | Supported |
| Flexbox | Bounded by flow model | Supported |
| JavaScript | Not executed | Executed by Chromium |
| External resources | Hook-based (`LoadFromUri`) | Fetched by browser engine |
| Web fonts | Bounded by flow model | Supported |
| **PDF Operations** |
| Create from scratch | Yes (programmatic API) | Yes (HTML or code) |
| Merge PDFs | Yes | Yes |
| Extract text | Yes | Yes |
| Digital signatures | Yes | Yes |
| Form filling | Yes | Yes |
| **Development** |
| Primary design | Programmatic construction | HTML rendering |
| Async API | Synchronous `Export` primary | Async variants throughout |
| Package count | Several packages | One package |
| Installation footprint | Lightweight managed assemblies | Bundles Chromium binaries |
| Preview workflow | Export to DOCX/PDF | Chrome print preview |
| **Architecture** |
| HTML role | Convenience import | Primary design center |
| Conversion steps | Two (HTML → flow → PDF) | One (HTML → PDF) |
| Layout model | Flow document | Browser rendering |
| Debugging surface | Two document models | One rendering model |

## Installation Comparison

**Telerik Document Processing Setup:**
```bash
Install-Package Telerik.Documents.Core
Install-Package Telerik.Documents.Fixed
Install-Package Telerik.Documents.Flow
Install-Package Telerik.Documents.Flow.FormatProviders.Html
Install-Package Telerik.Documents.Flow.FormatProviders.Pdf
Install-Package Telerik.Documents.ImageUtils
```

```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using System.IO;

var htmlProvider = new HtmlFormatProvider();
var flowDoc = htmlProvider.Import(html);
var pdfProvider = new PdfFormatProvider();
using (var output = File.OpenWrite("output.pdf"))
{
    pdfProvider.Export(flowDoc, output);
}
```

**IronPDF Setup:**
```bash
dotnet add package IronPdf
```

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

## When to Stay with Telerik / When IronPDF is Better

**Telerik Document Processing tends to fit best when:**
- Your primary use case is programmatic PDF construction, not HTML conversion
- You build PDFs from data structures rather than from HTML templates
- Your organization already licenses a Telerik UI or DevCraft suite
- You want the full set of Telerik document libraries (Word, Excel, PDF) under one vendor
- HTML conversion is a minor portion of overall PDF generation
- The HTML you import is shaped well for a flow-document representation

**IronPDF tends to fit best when:**
- HTML-to-PDF is the primary requirement rather than an occasional task
- Modern CSS features (Grid, Flexbox) are essential to layouts
- JavaScript execution is required (SPAs, dynamic content)
- You want Chrome print preview to be a 1:1 reference for the PDF
- A single rendering pass keeps debugging straightforward
- External resources should be fetched by a browser engine without per-resource hooks
- High-throughput scenarios benefit from async APIs throughout
- A single-package install simplifies deployment

## Conclusion

Telerik Document Processing is built around programmatic PDF construction—creating documents through code with control over positioning, drawing, and structure. The `RadFixedDocument` API is well-suited to scenarios that need coordinate-level positioning or where C# code owns the entire document-construction workflow. For those scenarios, Telerik provides a comprehensive, documented toolset that integrates with the broader Telerik suite.

HTML conversion in this library is a convenience feature: `HtmlFormatProvider` imports HTML into the same flow-document model used by the word-processing side, and `PdfFormatProvider` exports that model to PDF. That two-step shape is a deliberate design — HTML import was added on top of an existing document library rather than being its foundation.

IronPDF inverts that emphasis: HTML rendering is the foundation, and the Chromium engine renders HTML directly. For HTML-template-driven workflows, the practical effect is that Chrome print preview is a useful reference for the output and there is one layout pass to reason about rather than two.

**For teams evaluating these approaches:** does your workflow naturally express documents as HTML templates, or as programmatic C# construction? The answer is usually the most useful indicator of which library's primary design center aligns with your day-to-day work.

**Related resources:**
- [HTML to PDF Tutorial](https://ironpdf.com/tutorials/html-to-pdf/) - Complete guide to direct HTML rendering
- [ChromePdfRenderer API Documentation](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderer.html) - Detailed async method reference
