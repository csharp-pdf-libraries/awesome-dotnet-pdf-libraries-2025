---
title: "Nutrient.io vs IronPDF: side by side for .NET teams in 2026"
published: false
canonical_url: https://ironsoftware.com/suite/blog/comparison/
tags: dotnet, csharp, pdf, comparison
---
## When "comprehensive PDF toolkit" means something different than you expected

Nutrient (rebranded from PSPDFKit on 2024-10-23) is a multi-product family covering interactive PDF viewers, annotation tooling, mobile/Web SDKs, and a server-side .NET SDK that is actually GdPicture.NET — the imaging toolkit Nutrient acquired from ORPALIS and folded in as their .NET line. That breadth is the source of most scope-mismatch friction teams hit when they reach for it expecting a focused HTML-to-PDF library.

If your requirement is "embed a PDF viewer where users can highlight text and sign documents," a viewer SDK like Nutrient is the right shape. If your requirement is "render an invoice from an HTML template on a backend server and stream it back," the surface area you actually exercise is much narrower, and the licensing/packaging tradeoffs shift.

This guide compares Nutrient's .NET SDK (`GdPicture`) with IronPDF for that second scenario: backend HTML-to-PDF generation. It is not a knock on Nutrient's viewer story, which is genuinely strong.


## Understanding IronPDF

IronPDF focuses on one workflow: converting HTML (strings, files, URLs) to PDF documents using an embedded Chromium engine. The API surfaces HTML-to-PDF rendering methods first, with PDF manipulation capabilities (merge, split, watermark, encrypt, extract text) integrated alongside. The design priority is backend document generation — invoice rendering, report exports, automated document assembly — not building PDF viewer interfaces.

No UI components ship with IronPDF. No annotation toolbars. No form-filling widgets. If your requirement is "generate PDF from HTML template and serve it as download," IronPDF matches directly. If your requirement is "embed a PDF viewer where users can highlight text and add comments," you need a viewer SDK like Nutrient.

## Where Nutrient's .NET SDK Fits for HTML-to-PDF Workflows

### Product Status
Actively maintained. Nutrient's .NET offering ships as the `GdPicture` NuGet package (namespace `GdPicture14`, owner ORPALIS, a Nutrient subsidiary). HTML conversion is one capability inside a broader imaging-and-document toolkit that also covers OCR, barcode, TWAIN scanning, and raster processing.

### HTML-to-PDF Surface
The .NET SDK's HTML path uses `GdPictureDocumentConverter.LoadFromFile(path, DocumentFormat.DocumentFormatHTML)` then `SaveAsPDF(outPath)`. Input is file/URL based — there is no direct HTML-string entry point, so an in-memory HTML string is typically written to a temp file first. The converter shells out to a system Chrome or Edge install (or a portable path you set via `SetWebBrowserPath`) for the actual HTML rendering.

### Footprint and Scope
The full `GdPicture` package brings imaging, OCR, barcode, and scanning surface area in addition to PDF. If your project only needs HTML-to-PDF generation, you carry that broader surface area regardless.

### Documentation Center of Gravity
Documentation skews toward viewer/editor scenarios — React components, MAUI embedding, iOS/Android viewer integration. Backend HTML-to-PDF coverage exists in the .NET SDK docs, but is not the headline workflow.

### Licensing Model
Pricing is sales-led; `nutrient.io/sdk/pricing` routes to a Contact Us form. Quotes are typically per-developer plus deployment tier, which fits well with interactive viewer deployments and is harder to compare against published per-developer pricing for pure backend generation without a sales call.

## Feature Comparison Overview

| Feature | Nutrient .NET SDK (GdPicture) | IronPDF |
|---------|-------------------------------|---------|
| **Current Status** | Active development (rebranded from PSPDFKit, Oct 2024) | Active development |
| **HTML Input** | File / URL via `GdPictureDocumentConverter` | Direct HTML string, file, or URL |
| **HTML Rendering Engine** | System Chrome or Edge (or path set via `SetWebBrowserPath`) | Embedded Chromium |
| **Installation** | Broad imaging + PDF + OCR + barcode SDK | Focused PDF package |
| **Support** | Tilted toward viewer/editor scenarios | Generation-focused docs and support |
| **Product Shape** | Multi-product family (Document Engine, Web/mobile SDKs, .NET SDK) | Single .NET package |

## Troubleshooting: HTML Input Shape Differs Between the Two

### Problem: Nutrient .NET SDK — File/URL Input Only

**Scenario:** You have an HTML string already in memory (generated from a Razor view, a template engine, or a database column) and want to convert it to a PDF.

**Root cause:** `GdPictureDocumentConverter.LoadFromFile(path, DocumentFormat.DocumentFormatHTML)` takes a file path; `LoadFromHttp(url)` takes a URL. There is no direct HTML-string entry point, so the in-memory string is typically staged to a temp file first. HTML rendering itself is delegated to a system Chrome or Edge install (or a portable browser path set via `SetWebBrowserPath`), which means CSS/JS behavior tracks whatever browser version is installed on the host — verify against your deployment target.

**Diagnostic steps:**
1. Confirm whether your host has a compatible Chrome or Edge installed, or whether `SetWebBrowserPath` is pointing at a portable copy
2. Test with minimal HTML first (`<html><body>Test</body></html>`) before adding CSS
3. Compare output across different Chrome/Edge versions if you target multiple deployment environments

**Workaround:**
Write the HTML string to a temp file, then load via `LoadFromFile(..., DocumentFormat.DocumentFormatHTML)`:

```csharp
using GdPicture14;
using System.IO;

File.WriteAllText("input.html", html);

using var converter = new GdPictureDocumentConverter();
converter.LoadFromFile("input.html", DocumentFormat.DocumentFormatHTML);
converter.SaveAsPDF("output.pdf");
```

### Solution: IronPDF — Direct HTML Rendering

For comprehensive rendering documentation, see [Pixel-Perfect HTML to PDF](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/).

```csharp
using IronPdf;

// Direct HTML to PDF conversion
var renderer = new ChromePdfRenderer();

// CSS Grid and Flexbox work exactly as in Chrome
string htmlContent = @"
    <style>
        .invoice {
            display: grid;
            grid-template-columns: 2fr 1fr;
            gap: 20px;
        }
    </style>
    <div class='invoice'>
        <div>Left column content</div>
        <div>Right column content</div>
    </div>";

var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("invoice.pdf");
```

**Why this works:** IronPDF uses Chromium Blink engine—the same renderer as Chrome browser. If your HTML displays correctly in Chrome print preview, IronPDF will match it exactly. No separate conversion layer or document model.

## Troubleshooting: System Browser Dependency

### Problem: Nutrient .NET SDK — Host Needs Chrome or Edge

**Scenario:** Your infrastructure team flags that the .NET SDK's HTML conversion path expects a browser binary on the host. In container or hardened-server environments, that browser isn't always present.

**Root cause:** `GdPictureDocumentConverter` delegates HTML rendering to a system Chrome or Edge installation. You can point it at a portable browser via `SetWebBrowserPath`, but a browser binary still needs to exist somewhere on the host. Nutrient also offers a separate Document Engine product (a Docker microservice) for some HTML-to-PDF scenarios, which is a different deployment shape than the in-process .NET SDK.

**Diagnostic steps:**
1. Confirm Chrome or Edge is installed on each deployment target — including build agents, CI runners, and container images
2. If installing a system browser is not acceptable, package a portable Chromium and set `converter.SetWebBrowserPath(...)`
3. If you are evaluating Document Engine (the Docker product) instead of the in-process SDK, budget for the operational overhead of running it as a microservice

**Workaround:**
Bake a portable Chrome/Edge into your container image and configure `SetWebBrowserPath` at startup. Verify rendering output against your target browser version before promoting to production.

### Solution: IronPDF — Embedded Chromium, No External Browser

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

// Entire conversion happens in-process
var pdf = renderer.RenderHtmlAsPdf("<html><body>Content</body></html>");
pdf.SaveAs("output.pdf");
```

**Why this works:** IronPDF ships its own embedded Chromium binary inside the NuGet package. There is no system Chrome/Edge dependency to install on each host, and no separate microservice to deploy. For the full API surface, see the [ChromePdfRenderer documentation](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderer.html).

## Troubleshooting: SDK Footprint During Batch Processing

### Problem: Nutrient .NET SDK — Broad Surface Area for Generation-Only Jobs

**Scenario:** You are batch-generating PDFs from HTML templates, but the project only ever uses the HTML conversion path. The full `GdPicture` package still carries OCR engines, imaging utilities, barcode readers, and scanning surface area you do not exercise.

**Root cause:** The .NET SDK is a single comprehensive package that covers imaging, PDF, OCR, barcode, and scanning. Selective trimming of features is limited compared to libraries scoped purely to PDF.

**Diagnostic steps:**
1. Profile which `GdPicture14` assemblies are actually loaded during a typical batch run
2. Confirm `GdPicturePDF` / `GdPictureDocumentConverter` instances are being disposed (both implement `IDisposable`)
3. Inventory whether any non-HTML features (OCR, barcode, imaging) are actually used; if not, the broader surface area is unused weight

**Workaround:**
Wrap each conversion in a `using` block so the converter and PDF are disposed promptly, and consider chunking large batches so the working set is bounded. Long-running processes that never recycle can accumulate native resources held by the underlying browser binary.

### Solution: IronPDF — Scoped to PDF, Tuned for Batch

```csharp
using IronPdf;
using System.IO;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var htmlFiles = Directory.GetFiles("templates/", "*.html");

Parallel.ForEach(htmlFiles, htmlFile =>
{
    using var pdf = renderer.RenderHtmlFileAsPdf(htmlFile);
    pdf.SaveAs(Path.ChangeExtension(htmlFile, ".pdf"));
    // Disposing the PdfDocument releases the embedded Chromium resources promptly.
});
```

**Why this works:** The package surface is scoped to PDF generation and manipulation. The embedded Chromium is reused across conversions on a single renderer instance, and the `IDisposable` pattern releases native resources promptly per document.

## Troubleshooting: Pricing Comparison Requires a Sales Call

### Problem: Nutrient .NET SDK — Sales-Led Pricing

**Scenario:** You need to compare per-server or per-developer costs across a few candidate libraries for a backend generation workload. Nutrient's pricing page routes to a Contact Us form.

**Root cause:** `nutrient.io/sdk/pricing` directs prospects to a sales conversation rather than publishing a per-developer price list. Quotes are typically tailored per-developer plus deployment tier, which is a sensible fit for interactive viewer deployments and harder to drop into a quick comparison spreadsheet for a pure backend generation use case.

**Diagnostic steps:**
1. Request a quote scoped specifically to backend HTML-to-PDF generation
2. Confirm which deployment tiers (dev, staging, production servers) the quote covers
3. Verify whether the quote includes Document Engine (the Docker microservice) or only the in-process .NET SDK
4. Compare the quoted price against published per-developer pricing from other vendors

**Workaround:**
Be explicit with sales that you do not need the viewer/annotation surface so the quote reflects only the components you will use.

### Solution: IronPDF — Published Per-Developer Pricing

IronPDF publishes per-developer pricing on its website. A license covers unlimited PDF generation volume. Pricing structure: [IronPDF Licensing Options](https://ironpdf.com/licensing/).

```csharp
using IronPdf;

// Set license once at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

for (int i = 0; i < 50000; i++)
{
    using var pdf = renderer.RenderHtmlAsPdf($"<html><body>Invoice {i}</body></html>");
    pdf.SaveAs($"invoice_{i}.pdf");
}
```

**Why this works:** IronPDF's licensing is per-developer / per-deployment server and is published on the website. You can drop the numbers straight into a comparison spreadsheet without a sales call.

## Troubleshooting: CSS/JavaScript Compatibility Tracks Host Browser

### Problem: Nutrient .NET SDK — Output Pinned to Installed Chrome/Edge

**Scenario:** Your HTML templates use modern CSS features (Grid, custom properties, container queries). On a host with an up-to-date Chrome, output looks correct; on a host with an older Edge build, layouts collapse or styles do not apply.

**Root cause:** The .NET SDK's HTML renderer delegates to whatever Chrome or Edge install (or portable browser path) the host provides. CSS/JS support therefore tracks that browser's version — verify against the actual deployment target rather than your developer machine.

**Diagnostic steps:**
1. Capture the Chrome/Edge version on each deployment target (dev, CI, staging, production)
2. Test with minimal HTML containing `display: grid` and any CSS features your templates rely on against each version
3. If you ship a portable browser via `SetWebBrowserPath`, decide explicitly which version you are pinning to

**Workaround:**
Standardize on a single bundled portable Chromium across environments and configure `converter.SetWebBrowserPath(...)` so output is consistent regardless of what the host happens to have installed.

### Solution: IronPDF — Full Modern Web Standards Support

Complete rendering options reference: [ChromePdfRenderOptions documentation](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderOptions.html).

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Modern CSS just works - Chromium Blink engine
string modernHtml = @"
    <style>
        :root {
            --primary-color: #007bff;
            --spacing: 20px;
        }
        .container {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: var(--spacing);
        }
        .card {
            background: var(--primary-color);
            padding: var(--spacing);
        }
    </style>
    <div class='container'>
        <div class='card'>Card 1</div>
        <div class='card'>Card 2</div>
        <div class='card'>Card 3</div>
    </div>";

var pdf = renderer.RenderHtmlAsPdf(modernHtml);
pdf.SaveAs("modern_layout.pdf");
```

**Why this works:** The Chromium Blink renderer embedded in IronPDF supports CSS Grid, Flexbox, CSS Variables, Subgrid, Container Queries, and other modern features. Output is pinned to the Chromium version IronPDF ships with rather than whatever browser happens to be on the host.

## API Mapping Reference

| Nutrient .NET SDK (GdPicture) | IronPDF |
|-------------------------------|---------|
| `GdPictureDocumentConverter` (HTML/Office loader) | `ChromePdfRenderer` |
| `converter.LoadFromFile(path, DocumentFormat.DocumentFormatHTML)` | `renderer.RenderHtmlFileAsPdf(path)` |
| `converter.LoadFromHttp(url)` + `SaveAsPDF` | `renderer.RenderUrlAsPdf(url)` |
| HTML string staged via temp file then `LoadFromFile` | `renderer.RenderHtmlAsPdf(html)` |
| `GdPicturePDF` (PDF object model) | `PdfDocument` |
| `pdf.LoadFromFile(path)` | `PdfDocument.FromFile(path)` |
| `pdf.MergeDocuments(...)` / `converter.CombineToPDF(...)` | `PdfDocument.Merge(pdfs)` |
| `pdf.GetPageText()` (per page, 1-based) | `pdf.ExtractAllText()` |
| `pdf.DeletePage()` (after `SelectPage(i)`, 1-based) | `pdf.RemovePages(index)` (0-based) |
| `pdf.SetFillAlpha(...)` + `pdf.DrawTextBox(...)` | `pdf.ApplyWatermark(html, ...)` |
| `pdf.AddStampAnnotation(...)` | `pdf.ApplyStamp(...)` |
| `pdf.FlattenFormFields()` | `pdf.Form.Flatten()` |
| `pdf.SetFormFieldValue(name, value)` | `pdf.Form.FindFormField(name).Value = value` |
| OCR (built-in via GdPicture) | Not included (IronOCR available separately) |
| Barcode reading (built-in via GdPicture) | Not included (IronBarcode available separately) |
| Annotation / viewer UI components | Not included (IronPDF is generation-focused) |
| Watermarking | `pdf.ApplyWatermark(html, rotation, vAlign, hAlign)` |

## Comprehensive Feature Comparison

| Category | Feature | Nutrient .NET SDK (GdPicture) | IronPDF |
|----------|---------|-------------------------------|---------|
| **Status** | Maintenance | Active development | Active development |
| | Primary Focus | Imaging + PDF + OCR + barcode toolkit | HTML-to-PDF generation |
| **Support** | Documentation focus | Viewer/editor scenarios across the product family | Backend generation scenarios |
| | SLA Available | Yes | Yes (Enterprise) |
| **Content Creation** | HTML Input | File / URL via `GdPictureDocumentConverter` | Direct HTML string, file, or URL |
| | HTML Rendering Engine | System Chrome / Edge | Embedded Chromium |
| | URL to PDF | Yes (`LoadFromHttp`) | Yes (`RenderUrlAsPdf`) |
| | Custom Fonts | Supported | Supported (including WOFF2) |
| | Headers / Footers | Manual `DrawText` per page | `HtmlHeader` / `HtmlFooter` with `{page}` tokens |
| | Watermarks | Composed via `SetFillAlpha` + `DrawTextBox` | `pdf.ApplyWatermark(html, ...)` |
| | Page Numbering | Manual drawing per page | `{page}` / `{total-pages}` placeholders |
| **PDF Operations** | Merge PDFs | Yes (`CombineToPDF` / `MergeDocuments`) | Yes (`PdfDocument.Merge`) |
| | Extract Text | Yes (per page via `GetPageText`) | Yes (`ExtractAllText`) |
| | Fill Forms | Yes (index-based) | Yes (name-keyed `Form.FindFormField`) |
| | Digital Signatures | Yes | Yes |
| | Encryption / Password | Yes (AES-256 via `SaveToFile` overloads) | Yes (`SecuritySettings`) |
| **Interactive Features** | Annotation / Viewer UI | Core part of the Nutrient product family | Not included |
| | Form-Filling UI | Yes (viewer SDKs) | Not included |
| **Specialized Features** | OCR | Built-in | Separate library (IronOCR) |
| | Barcode Reading | Built-in | Separate library (IronBarcode) |
| | TWAIN Scanning | Built-in | Not applicable |
| | Image Processing | Extensive | Basic |
| **Development** | Thread Safety | Yes | Fully thread-safe |
| | Docker Support | Browser binary needs to be in the image | Embedded Chromium ships with the package |
| | Cross-Platform | Windows / Linux / macOS | Windows / Linux / macOS |
| | .NET Version Support | .NET Framework + .NET 6/7/8/10 | .NET Framework + .NET 6+ |
| | Page Indexing | 1-based | 0-based |

## Installation Comparison

### Nutrient .NET SDK Installation

```bash
# Nutrient's server-side .NET SDK ships as the GdPicture NuGet package
# (owner ORPALIS, a Nutrient subsidiary). Namespace is GdPicture14.
dotnet add package GdPicture
```

```csharp
using GdPicture14;
using System.IO;

// HTML rendering delegates to a system Chrome / Edge install, or a portable
// browser path set via converter.SetWebBrowserPath(...).
var htmlContent = "<html><body><h1>Hello World</h1></body></html>";
File.WriteAllText("input.html", htmlContent);

using var converter = new GdPictureDocumentConverter();
converter.LoadFromFile("input.html", DocumentFormat.DocumentFormatHTML);
converter.SaveAsPDF("output.pdf");
```

The full `GdPicture` package brings imaging, OCR, barcode, and scanning surface area alongside PDF generation. For a project that only needs HTML-to-PDF, that broader surface is along for the ride.

### IronPDF Installation

Step-by-step guide: [HTML String to PDF Tutorial](https://ironpdf.com/how-to/html-string-to-pdf/).

```bash
dotnet add package IronPdf
```

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<html><body>Content</body></html>");
pdf.SaveAs("output.pdf");
```

A single NuGet package with embedded Chromium — no system browser to install on each host and no separate microservice to deploy.

## When to Use Nutrient, When to Use IronPDF

Use Nutrient when you're building interactive PDF experiences: document viewers with annotation toolbars, form-filling interfaces, real-time collaboration features, or apps where users need to markup and sign PDFs within your UI. Nutrient excels at these scenarios and provides cross-platform viewer SDKs (Web, iOS, Android, desktop) with consistent APIs.

Use IronPDF when your requirement is backend document generation: rendering invoices from HTML templates, exporting reports to PDF, batch-converting content for archival, or automated document assembly. IronPDF's direct HTML-to-PDF workflow and generation-focused licensing align with these use cases.

The red flag scenario: you're using Nutrient purely for HTML-to-PDF generation without leveraging its viewer/editor capabilities. In these cases, you're paying for features you don't use and working with an SDK optimized for different problems.

The green flag scenario: you need both generation and interactive viewing. Evaluate whether Nutrient's comprehensive approach makes sense or whether combining IronPDF (generation) + a simpler viewer library (PDF.js, browser native) costs less and integrates easier.

For teams generating thousands of PDFs per day from HTML templates with no interactive viewing requirements, IronPDF's focused API and licensing structure typically prove more cost-effective. For teams building document-centric applications where users annotate, sign, and collaborate on PDFs, Nutrient's comprehensive SDK justifies its scope.

Have you hit any of these Nutrient integration pain points? How did you handle the decision between comprehensive PDF SDKs versus focused HTML-to-PDF libraries?

**Related resources:**
- [IronPDF HTML File to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [Chrome Rendering Engine Technical Documentation](https://ironpdf.com/how-to/ironpdf-2021-chrome-rendering-engine-eap/)
