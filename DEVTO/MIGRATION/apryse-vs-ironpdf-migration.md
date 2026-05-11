---
title: "Migrating from Apryse PDF to IronPDF: what breaks, what does not"
published: false
description: "A story-driven migration guide for .NET teams moving from Apryse (PDFTron) SDK to IronPDF. Covers the enterprise-SDK-to-focused-library shift, API mapping, and a printable checklist."
tags: dotnet, csharp, pdf, migration
---

The renewal email arrived on a Tuesday. Attached was a PDF—ironic, given the context—outlining the new licensing structure for the Apryse SDK, formerly PDFTron. The per-platform, per-feature, add-on pricing model had been reorganized. Again. Our legal team had questions. Our procurement team had questions. Our engineering team had one question: how much of this SDK do we actually use?

The answer, after two days of grepping, was humbling. We used `HTML2PDF` to convert Razor templates into reports. We used `PDFDoc.MergePages` to combine those reports into bundles. We used `SecurityHandler` to set passwords. That was it. Three features out of an SDK with a hundred. We were licensed for redaction, OCR, form design, CAD conversion, annotation collaboration, and a real-time WebViewer—none of which existed in our codebase.

Apryse builds enterprise document infrastructure. It is a serious platform for serious document workflows. But if your use case is "HTML in, encrypted PDF out," the licensing, API surface, and deployment weight of a full document platform create friction that compounds over time.

This guide walks through migrating to [IronPDF](https://ironpdf.com)—a library focused specifically on the workflow we actually needed. Eighty percent of the content here is general migration knowledge. The remaining twenty percent is IronPDF-specific.

## Why Migrate (Without Drama)

Apryse is a well-funded, well-supported platform. Here is why teams trim it from their stack:

1. **Feature-to-usage ratio.** You are licensed for an enterprise document platform. You use three features. The delta between what you pay for and what you use is the migration trigger.
2. **Licensing complexity.** Apryse uses per-platform, per-module licensing with add-ons for OCR, redaction, digital signatures, CAD conversion, etc. Understanding what you are licensed for—and what you are not—requires reading contract attachments.
3. **Pricing at scale.** Apryse SDK pricing is enterprise-tier. For teams that need HTML-to-PDF and basic manipulation, the cost-per-feature is high relative to focused libraries.
4. **API surface area.** PDFNet's API covers PDF internals, annotations, appearance objects, widget types, and content streams. For a team that just wants `RenderHtmlAsPdf()`, the cognitive overhead of navigating the full SDK is real.
5. **HTML2PDF module limitations.** Apryse's `HTML2PDF` module uses its own rendering engine. Teams report issues with CSS fidelity compared to Chrome-based renderers, particularly with modern CSS grid, flexbox, and web fonts.
6. **Native binary footprint.** Apryse ships platform-specific native libraries. Docker images grow, and ensuring the right binaries are present for each target platform requires careful configuration.
7. **Migration path from PDFTron.** The rebrand from PDFTron to Apryse introduced package name and namespace changes. Teams on older PDFTron versions face a migration just to stay on the current Apryse SDK—which creates an opportunity to re-evaluate the dependency entirely.
8. **Add-on fragmentation.** Need OCR? That is a module. Need redaction? Another module. Need the WebViewer? Another license. Each add-on increases the licensing surface and the deployment footprint.
9. **Documentation and onboarding.** Apryse documentation is extensive but can be overwhelming. Multiple users report that finding the right entry point for a specific task takes trial and error, especially with the documentation covering multiple platforms and languages.
10. **Vendor consolidation strategy.** Apryse acquired ActivePDF and iText, bringing multiple PDF products under one corporate umbrella. For teams concerned about long-term product strategy—which product line gets investment, which gets sunset—this consolidation introduces uncertainty.

### Comparison Table

| Aspect | Apryse PDF SDK (PDFTron) | IronPDF |
|---|---|---|
| **Focus** | Enterprise document platform (PDF + 30 formats) | HTML-first PDF generation + manipulation |
| **Pricing** | Enterprise, per-platform, add-on modules | Per-developer, perpetual licenses available |
| **API Style** | Low-level PDFNet + higher-level convenience classes | Fluent, HTML-centric, property-based |
| **Learning Curve** | High—large API surface, multiple modules | Low—focused API, HTML/CSS developers productive quickly |
| **HTML Rendering** | `HTML2PDF` module (proprietary engine) | Chrome rendering engine |
| **Page Indexing** | 1-based via PageIterator | 0-based |
| **Thread Safety** | PDFNet requires `PDFNet.Initialize()` call before any SDK use | `ChromePdfRenderer` is reusable and can be shared |
| **Namespace** | `pdftron.PDF` / `pdftron.SDF` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Complexity | Notes |
|---|---|---|
| HTML → PDF | Low | Replace `HTML2PDF` module with `ChromePdfRenderer`—likely better CSS fidelity |
| Merge | Low | Both support merge; different API patterns |
| Split / extract pages | Low | Different API, same concept |
| Watermark | Medium | Apryse uses stamper/appearance; IronPDF uses HTML overlay |
| Password / encryption | Medium | Apryse uses `SecurityHandler`; IronPDF uses `SecuritySettings` |
| Digital signatures | Medium-High | Different signing APIs; review IronPDF's signing coverage against your requirements |
| Annotation | High / N/A | Apryse has deep annotation support; IronPDF does not focus on annotations |
| Redaction | N/A | No IronPDF equivalent—Apryse-specific feature |
| OCR | N/A | Not an IronPDF feature—consider IronOCR separately |
| WebViewer | N/A | No IronPDF equivalent—browser-based viewer is an Apryse product |
| CAD / Office conversion | N/A | Not an IronPDF feature; use dedicated converters |

### Decision Matrix

| Scenario | Recommendation |
|---|---|
| HTML-to-PDF + merge + encrypt, no viewer/annotation/OCR | Migrate—IronPDF covers this at a fraction of the API surface and cost |
| Deep annotation, redaction, or collaboration features | Stay—Apryse's document platform features have no IronPDF equivalent |
| WebViewer embedded in your application | Stay for the viewer; optionally use IronPDF for server-side generation |
| Evaluating during PDFTron → Apryse rebrand migration | Good time to right-size—migrate the parts you use to IronPDF |

---

## Before You Start

### Prerequisites

- .NET 6+ (or .NET Framework 4.6.2+)
- A trial or licensed [IronPDF key](https://ironpdf.com/get-started/license-keys/)
- An honest audit of which Apryse features your codebase actually calls

### Find Apryse/PDFTron References

```bash
# Find all Apryse/PDFTron using statements
rg -l "pdftron|PDFTron|PDFNet|PDFDoc|HTML2PDF|SecurityHandler" --glob "*.cs"

# Count usage of advanced features (annotation, redaction, OCR)
rg -c "Annot|Redactor|OCR|WebViewer|ContentReplacer" --glob "*.cs"

# Count HTML2PDF usage (the feature you're likely migrating)
rg -c "HTML2PDF" --glob "*.cs"

# Find project references
rg -l "PDFTron|pdftron|Apryse" --glob "*.csproj"
```

In PowerShell: `Get-ChildItem -Recurse -Filter *.cs | Select-String "pdftron|PDFNet|PDFDoc|HTML2PDF"`.

**Key insight:** If `Annot|Redactor|OCR|WebViewer` count is zero but `HTML2PDF|PDFDoc|SecurityHandler` count is non-zero, you are using 10% of the SDK. That is the ideal migration profile.

### Swap NuGet Packages

```bash
# Apryse retained the PDFTron.* NuGet IDs after the 2023 rebrand.
# Remove whichever variant is in your project:
dotnet remove package PDFTron.NET.x64
dotnet remove package PDFTron.NetFramework.x64
dotnet remove package PDFTron.NETCore.Windows.x64
dotnet remove package PDFNet

# Install IronPDF
dotnet add package IronPdf
```

---

## Quick Start Migration (3 Steps)

### Step 1: License Configuration

**Before (Apryse/PDFTron):**

```csharp
using pdftron;
using pdftron.PDF;
using System;

class Program
{
    static void Main()
    {
        // PDFNet requires global initialization before any operations
        PDFNet.Initialize("YOUR-LICENSE-KEY");

        // Optional: set a resource path if your deployment requires it
        // PDFNet.SetResourcesPath("path/to/resources");

        Console.WriteLine("PDFNet initialized.");

        // All PDFNet operations must happen after Initialize
        // ...

        PDFNet.Terminate();
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
        // No global initialization. No terminate. No resource paths.
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        Console.WriteLine($"IronPDF licensed: {License.IsLicensed}");
    }
}
```

The `PDFNet.Initialize()` / `PDFNet.Terminate()` lifecycle goes away entirely.

### Step 2: Namespace Imports

**Before:**

```csharp
using pdftron;           // core
using pdftron.PDF;       // PDF operations
using pdftron.SDF;       // low-level document structure
using pdftron.Common;    // common types
```

**After:**

```csharp
using IronPdf;
using IronPdf.Editing;    // watermarks, headers, footers
using IronPdf.Rendering;  // render options
```

### Step 3: Basic HTML-to-PDF

**Before (Apryse HTML2PDF — ~20 lines):**

```csharp
using pdftron;
using pdftron.PDF;
using System;

class Program
{
    static void Main()
    {
        PDFNet.Initialize("YOUR-LICENSE-KEY");

        using var doc = new PDFDoc();

        var converter = new HTML2PDF();
        // Optional: converter.SetModulePath("path/to/html2pdf");

        converter.InsertFromHtmlString(
            "<h1>Quarterly Report</h1><p>Generated by Apryse HTML2PDF</p>");

        if (!converter.Convert(doc))
        {
            Console.WriteLine("Conversion failed: " + converter.GetLog());
            PDFNet.Terminate();
            return;
        }

        doc.Save("report_apryse.pdf", SDFDoc.SaveOptions.e_linearized);
        Console.WriteLine("Saved with Apryse HTML2PDF.");

        PDFNet.Terminate();
    }
}
```

**After (IronPDF — 10 lines):**

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.Letter;

        var pdf = renderer.RenderHtmlAsPdf(
            "<h1>Quarterly Report</h1><p>Generated by IronPDF</p>");

        pdf.SaveAs("report_ironpdf.pdf");
        Console.WriteLine("Saved with IronPDF.");
    }
}
```

No initialization. No termination. No converter object. No success-check boilerplate. The Chrome engine renders modern CSS faithfully. See the [IronPDF HTML-to-PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/).

---

## API Mapping Tables

### Namespace Mapping

| Apryse Namespace | IronPDF Namespace | Purpose |
|---|---|---|
| `pdftron` | N/A (no global init) | SDK initialization |
| `pdftron.PDF` | `IronPdf` | PDF document operations |
| `pdftron.SDF` | N/A | Low-level document structure (not needed in IronPDF) |

### Core Class Mapping

| Apryse Class | IronPDF Class | Description |
|---|---|---|
| `PDFNet` | `IronPdf.License` | Global initialization / licensing |
| `PDFDoc` | `PdfDocument` | The PDF document object |
| `HTML2PDF` | `ChromePdfRenderer` | HTML-to-PDF conversion |
| `SecurityHandler` | `pdf.SecuritySettings` | Encryption and password protection |

### Document Loading Methods

| Operation | Apryse | IronPDF |
|---|---|---|
| Open from file | `new PDFDoc("file.pdf")` | `PdfDocument.FromFile("file.pdf")` |
| Open with password | `new PDFDoc("file.pdf"); doc.InitStdSecurityHandler("pass")` | `PdfDocument.FromFile("file.pdf", "pass")` |
| Create new | `new PDFDoc()` | `renderer.RenderHtmlAsPdf(html)` |
| Save | `doc.Save("out.pdf", saveOptions)` | `pdf.SaveAs("out.pdf")` |

### Page Operations

| Operation | Apryse | IronPDF |
|---|---|---|
| Get page count | `doc.GetPageCount()` | `pdf.PageCount` |
| Get page | `doc.GetPage(pageNum)` (1-based) | `pdf.Pages[index]` (0-based) |
| Delete page | `doc.PageRemove(pageIterator)` | `pdf.RemovePages(index)` |
| Insert pages | `doc.InsertPages(...)` | `PdfDocument.Merge()` or page copy |

### Merge / Split Operations

| Operation | Apryse | IronPDF |
|---|---|---|
| Merge | `doc.InsertPages()` or `doc.MergePages()` | `PdfDocument.Merge(pdf1, pdf2)` |
| Extract pages | Page iterator + new doc | `pdf.CopyPages(start, end)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (Apryse HTML2PDF):**

```csharp
using pdftron;
using pdftron.PDF;
using System;

class HtmlToPdfApryse
{
    static void Main()
    {
        PDFNet.Initialize("YOUR-LICENSE-KEY");

        using var doc = new PDFDoc();
        var converter = new HTML2PDF();

        var settings = new HTML2PDF.WebPageSettings();
        settings.SetPrintBackground(true);
        settings.SetLoadImages(true);

        string html = @"
            <html><head><style>
                body { font-family: 'Segoe UI', sans-serif; }
                .banner { background: #1b5e20; color: white; padding: 30px; }
                .metrics { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; padding: 20px; }
                .card { border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px; }
            </style></head><body>
                <div class='banner'><h1>Environment Impact Report</h1></div>
                <div class='metrics'>
                    <div class='card'><h3>Carbon Offset</h3><p>1,242 tons</p></div>
                    <div class='card'><h3>Energy Saved</h3><p>85,000 kWh</p></div>
                </div>
            </body></html>";

        converter.InsertFromHtmlString(html, settings);

        if (!converter.Convert(doc))
        {
            Console.WriteLine("HTML2PDF failed: " + converter.GetLog());
            PDFNet.Terminate();
            return;
        }

        doc.Save("report_apryse.pdf", SDFDoc.SaveOptions.e_linearized);
        PDFNet.Terminate();
        Console.WriteLine("Saved with Apryse.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;
using System;

class HtmlToPdfIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.Letter;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.PrintHtmlBackgrounds = true;
        renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Screen;

        string html = @"
            <html><head><style>
                body { font-family: 'Segoe UI', sans-serif; }
                .banner { background: #1b5e20; color: white; padding: 30px; }
                .metrics { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; padding: 20px; }
                .card { border: 1px solid #e0e0e0; border-radius: 8px; padding: 20px; }
            </style></head><body>
                <div class='banner'><h1>Environment Impact Report</h1></div>
                <div class='metrics'>
                    <div class='card'><h3>Carbon Offset</h3><p>1,242 tons</p></div>
                    <div class='card'><h3>Energy Saved</h3><p>85,000 kWh</p></div>
                </div>
            </body></html>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report_ironpdf.pdf");
        Console.WriteLine("Saved with IronPDF.");
    }
}
```

CSS grid renders correctly because IronPDF uses a real Chrome engine. Apryse's HTML2PDF module is built on a different engine, so modern layout features like CSS grid and flexbox can render differently—worth side-by-side testing against your existing templates. See the [IronPDF HTML-to-PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/).

### 2. Merge PDFs

**Before (Apryse):**

```csharp
using pdftron;
using pdftron.PDF;
using System;

class MergeApryse
{
    static void Main()
    {
        PDFNet.Initialize("YOUR-LICENSE-KEY");

        using var doc1 = new PDFDoc("part1.pdf");
        using var doc2 = new PDFDoc("part2.pdf");

        // Append all pages from doc2 onto the end of doc1 (1-based ranges)
        doc1.InsertPages(
            doc1.GetPageCount() + 1,  // insert position
            doc2,
            1,                         // start page
            doc2.GetPageCount(),       // end page
            PDFDoc.InsertFlag.e_none
        );

        doc1.Save("merged_apryse.pdf", SDFDoc.SaveOptions.e_linearized);
        PDFNet.Terminate();
        Console.WriteLine("Merged with Apryse.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class MergeIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf1 = PdfDocument.FromFile("part1.pdf");
        var pdf2 = PdfDocument.FromFile("part2.pdf");

        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
        Console.WriteLine("Merged with IronPDF.");
    }
}
```

No page-count arithmetic. No insert position. No flags. See [IronPDF merge documentation](https://ironpdf.com/examples/merge-pdfs/).

### 3. Watermark

**Before (Apryse — Stamper or appearance-based):**

```csharp
using pdftron;
using pdftron.PDF;
using System;

class WatermarkApryse
{
    static void Main()
    {
        PDFNet.Initialize("YOUR-LICENSE-KEY");

        using var doc = new PDFDoc("input.pdf");

        using var stamper = new Stamper(
            Stamper.SizeType.e_relative_scale, 0.5, 0.5);

        stamper.SetAlignment(
            Stamper.HorizontalAlignment.e_horizontal_center,
            Stamper.VerticalAlignment.e_vertical_center);
        stamper.SetFontColor(new ColorPt(0.8, 0.8, 0.8));  // light gray
        stamper.SetRotation(45);
        stamper.SetAsBackground(false);                    // overlay

        var pgSet = new PageSet(1, doc.GetPageCount());
        stamper.StampText(doc, "CONFIDENTIAL", pgSet);

        doc.Save("watermarked_apryse.pdf", SDFDoc.SaveOptions.e_linearized);
        PDFNet.Terminate();
        Console.WriteLine("Watermarked with Apryse.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class WatermarkIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        pdf.ApplyWatermark(
            "<h1 style='color:rgba(200,200,200,0.5);font-size:48px;font-family:Helvetica;'>CONFIDENTIAL</h1>",
            rotation: 45
        );

        pdf.SaveAs("watermarked.pdf");
        Console.WriteLine("Watermarked with IronPDF.");
    }
}
```

Stamper objects, ColorPt values, PageSet ranges, and alignment enums reduce to one HTML string.

### 4. Password Protection

**Before (Apryse — SecurityHandler):**

```csharp
using pdftron;
using pdftron.PDF;
using pdftron.SDF;
using System;

class PasswordApryse
{
    static void Main()
    {
        PDFNet.Initialize("YOUR-LICENSE-KEY");

        using var doc = new PDFDoc("input.pdf");

        var handler = new SecurityHandler();
        handler.ChangeUserPassword("user456");
        handler.ChangeMasterPassword("owner123");

        handler.SetPermission(SecurityHandler.Permission.e_print, false);
        handler.SetPermission(SecurityHandler.Permission.e_doc_modify, false);

        doc.SetSecurityHandler(handler);
        doc.Save("protected_apryse.pdf", SDFDoc.SaveOptions.e_linearized);

        PDFNet.Terminate();
        Console.WriteLine("Protected with Apryse.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class PasswordIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        pdf.SecuritySettings.OwnerPassword = "owner123";
        pdf.SecuritySettings.UserPassword = "user456";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.NoPrint;
        pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;

        pdf.SaveAs("protected.pdf");
        Console.WriteLine("Protected with IronPDF.");
    }
}
```

No `SecurityHandler` construction, no algorithm type enum, no permission flag method calls. See [IronPDF security documentation](https://ironpdf.com/how-to/pdf-permissions-passwords/).

---

## Critical Migration Notes

### PDFNet.Initialize / Terminate Lifecycle

Apryse requires `PDFNet.Initialize()` before any SDK call and `PDFNet.Terminate()` at shutdown. Forgetting either causes crashes or resource leaks. IronPDF has no global lifecycle—set the license key and start working.

### Page Indexing

Apryse uses 1-based page numbering in most operations. IronPDF uses 0-based indexing. Audit every page reference.

### SDFDoc Save Options

Apryse's `Save` method takes `SDFDoc.SaveOptions` flags (linearized, incremental, remove unused, etc.). IronPDF's `SaveAs` performs a standard full save. If you relied on linearized output for byte-range web streaming, plan an equivalent post-processing step or accept the standard save.

### HTML2PDF Engine Differences

Apryse's `HTML2PDF` module uses a proprietary rendering engine (not Chrome). IronPDF uses Chrome. The same HTML may render differently—particularly for CSS grid, flexbox, `@font-face`, and JavaScript-dependent content. Test your templates and expect IronPDF's Chrome output to be closer to what you see in a desktop browser.

### Features You Lose

Be explicit about features that do not migrate: annotation, redaction, OCR, WebViewer, CAD conversion, real-time collaboration. If any of these exist in your codebase (even unused), document them before removing the Apryse dependency.

---

## Performance Considerations

### Rendering Engine Comparison

Apryse's `HTML2PDF` module and IronPDF's Chrome engine are fundamentally different renderers. IronPDF's Chrome engine typically handles modern CSS more faithfully because it is, essentially, Chrome. Apryse's engine can render simpler HTML faster. Benchmark with your specific templates before drawing conclusions.

### Renderer Reuse

```csharp
private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();
```

### Disposal

```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Edge Cases Worth Flagging

- **Linearized PDF output:** Apryse supports linearized (web-optimized) PDF saves. IronPDF's `SaveAs` produces a standard (non-linearized) PDF. If byte-range streaming matters for your distribution, run a post-process linearizer or accept the standard save.
- **Annotation preservation:** If existing PDFs in your pipeline contain Apryse-created annotations, IronPDF can open these files but does not have annotation editing APIs. Annotations are preserved in the PDF but cannot be programmatically modified.
- **PDFTron → Apryse namespace migration:** If you are still on older PDFTron package names, you are facing a namespace migration regardless. This is a natural point to evaluate whether Apryse is the right target or whether IronPDF better fits your actual usage.

---

## Migration Checklist

### Pre-Migration (8 items)

- [ ] Audit codebase: which Apryse features do you actually use? (HTML2PDF, merge, security, annotations, redaction, OCR, WebViewer, other)
- [ ] Run `rg` to count advanced feature usage vs. basic feature usage
- [ ] Identify any Apryse WebViewer frontend components that need separate handling
- [ ] Obtain IronPDF trial key from [ironpdf.com/get-started/license-keys/](https://ironpdf.com/get-started/license-keys/)
- [ ] Create migration branch
- [ ] Document the Apryse license modules your contract covers
- [ ] Identify any `PDFNet.Initialize()` / `Terminate()` lifecycle patterns
- [ ] Check for Apryse-specific PDF features (annotations, custom stamps) in existing documents

### Code Migration (10 items)

- [ ] Remove Apryse/PDFTron NuGet packages
- [ ] Add `IronPdf` NuGet package
- [ ] Remove all `PDFNet.Initialize()` and `PDFNet.Terminate()` calls
- [ ] Replace namespace imports from `pdftron.*` to `IronPdf`
- [ ] Replace `HTML2PDF` conversion with `ChromePdfRenderer.RenderHtmlAsPdf()`
- [ ] Replace `PDFDoc.InsertPages()` merge pattern with `PdfDocument.Merge()`
- [ ] Replace `Stamper` watermark with `ApplyWatermark()` HTML
- [ ] Replace `SecurityHandler` with `SecuritySettings` properties
- [ ] Convert 1-based page indices to 0-based
- [ ] Remove `SDFDoc.SaveOptions` flags from save calls

### Testing (7 items)

- [ ] Compare HTML-to-PDF output side-by-side for your top templates (Chrome vs. Apryse HTML2PDF engine)
- [ ] Verify CSS grid, flexbox, and web fonts render correctly in IronPDF
- [ ] Test password protection round-trip
- [ ] Test merge with documents from mixed sources
- [ ] Verify watermark positioning on single-page and multi-page documents
- [ ] Load-test concurrent rendering
- [ ] Validate on all target platforms (Windows, Linux, Docker)

### Post-Migration (4 items)

- [ ] Cancel or downgrade Apryse license at next renewal
- [ ] Remove native Apryse binaries from Docker images and deployment artifacts
- [ ] Update architecture documentation—document platform reduced to focused library
- [ ] Monitor production for two weeks, comparing PDF quality and render times

---

## The Bottom Line

Two days of grepping revealed that our six-figure document platform was a three-feature PDF library with extra weight. The migration to IronPDF took a week—most of that was testing HTML templates in the Chrome engine against the Apryse HTML2PDF output to verify CSS fidelity.

The lesson is not "Apryse is bad." It is "right-size your dependencies." If you need annotation, redaction, OCR, and a WebViewer, Apryse earns its place. If you need HTML → PDF → merge → encrypt, a focused library eliminates licensing conversations, reduces your API surface, and simplifies your deployment.

**Worth knowing even without IronPDF:** if you decide to stay with Apryse but want better HTML rendering, you can replace just the `HTML2PDF` module by rendering with Puppeteer Sharp or Playwright first, then loading the resulting PDF into Apryse for post-processing. This gives you Chrome-quality HTML rendering while keeping Apryse's annotation and manipulation features.

My question for the comments: if you have moved off Apryse (or PDFTron), what was the tipping point? Was it the licensing renewal, a specific feature gap in `HTML2PDF`, or something else entirely? I suspect the answers cluster around licensing, but I would love to be surprised.

---

