---
title: "Telerik Document Processing to IronPDF: less config, same output"
published: false
tags: dotnet, csharp, pdf, migration
---

The conversation starts with a dependency audit. Someone runs `dotnet list package --include-transitive` and the Telerik Document Processing transitive dependency tree scrolls for several screens. There are multiple `Telerik.Documents.*` packages — some for PDF, some for Word, some for spreadsheets, some for imaging — and the application only uses PDF generation. The install footprint is wider than the actual use case, and with each major upgrade, the chance that one of those packages has a breaking change or a new native dependency grows.

This article covers migrating from the Telerik Document Processing Library (RadPdfProcessing / RadFlowDocument) to IronPDF for PDF generation use cases. You'll have working before/after code and a checklist by the end. The analysis is useful even if you're evaluating other alternatives.

---

## Why Migrate (Without Drama)

Teams evaluating Telerik Document Processing alternatives commonly cite:

1. **Package footprint** — TDP involves multiple NuGet packages; teams using only PDF generation often pull in the Flow, Fixed, and Core packages together.
2. **Telerik license dependency** — TDP is not sold standalone; it ships bundled inside any Telerik UI suite (UI for Blazor, UI for WPF, etc.) or a DevCraft bundle.
3. **Two-model API** — TDP exposes two different document object models: `RadFlowDocument` (flow layout for HTML/Word-style content) and `RadFixedDocument` (fixed-page layout for PDF manipulation). HTML import flows through one model; merging and page-level editing flow through the other.
4. **HTML rendering via Flow Document conversion** — `HtmlFormatProvider` imports HTML into a `RadFlowDocument`, which flattens the DOM into paragraphs and sections before export. Modern CSS layout (Flexbox, Grid, Bootstrap columns) does not survive the conversion.
5. **Upgrade coupling** — the Document Processing version typically tracks the parent Telerik UI suite's release cadence; upgrading PDF code may pull in unrelated UI updates.
6. **CSS layout gap** — if your report content starts as HTML/CSS, the round trip through Flow Document is the main source of layout drift.
7. **Cross-platform packaging** — TDP ships both `Telerik.Windows.Documents.*` (.NET Framework) and `Telerik.Documents.*` (.NET Standard / cross-platform) variants; managing the right package per target adds friction.
8. **Multiple source packages** — different Telerik documents packages for PDF (Fixed), Word/HTML (Flow), and Excel (Spreadsheet) means version management across several package IDs.
9. **API verbosity for common tasks** — text extraction iterates over `ContentElementBase`; watermarking uses `FixedContentEditor` with manual position translation; merging requires per-page copying between documents.
10. **Community support** — TDP documentation is thorough, but community Q&A is concentrated in the Telerik forums rather than Stack Overflow.

### Comparison Table

| Aspect | Telerik Document Processing | IronPDF |
|---|---|---|
| Focus | Document generation (PDF, Word, Excel) via two document models | HTML-to-PDF + PDF manipulation |
| Pricing | Bundled inside a Telerik UI suite or DevCraft (no standalone SKU) | Standalone commercial license |
| HTML API | `HtmlFormatProvider` → `RadFlowDocument` → `PdfFormatProvider.Export()` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| PDF manipulation API | `RadFixedDocument` + `FixedContentEditor` | `PdfDocument` |
| Learning Curve | Medium-High; two document models with many types | Low for web devs; HTML/CSS is the input |
| HTML Rendering Engine | Flow Document conversion | Embedded Chromium |
| Page Indexing | 0-based | 0-based |
| Namespace | `Telerik.Windows.Documents.Flow.*` / `Telerik.Windows.Documents.Fixed.*` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Telerik Document Processing | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | `HtmlFormatProvider.Import(html)` → `PdfFormatProvider.Export(doc, stream)` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | `HttpClient.GetStringAsync()` → Flow import → PDF export | `renderer.RenderUrlAsPdfAsync()` | Low |
| Save to file | `PdfFormatProvider.Export(doc, stream)` | `pdf.SaveAs(path)` | Low |
| Save to stream | `PdfFormatProvider.Export(doc, stream)` | `pdf.Stream` / `pdf.BinaryData` | Low |
| Document model creation | `RadFlowDocument` + `Section` + `Paragraph` | HTML/CSS template | High (rewrite) |
| Merge PDFs | Open as `RadFixedDocument`, copy `Pages` into a new `RadFixedDocument`, export | `PdfDocument.Merge()` | Low |
| Watermark | `FixedContentEditor` with manual position translation | `pdf.ApplyWatermark()` or `TextStamper` | Medium |
| Password protection | `PdfExportSettings.UserPassword` / `OwnerPassword` | `pdf.SecuritySettings` | Low |
| Text extraction | Iterate `RadFixedPage.Content` looking for `TextFragment` | `pdf.ExtractAllText()` | Medium |
| Headers / footers | `Section.Headers` + `Paragraph` + manual page-number fields | `RenderingOptions.HtmlHeader` / `HtmlFooter` | Low |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| PDF generation from HTML templates is the primary need | Switch — IronPDF is HTML-first; the Flow Document round-trip is the main pain point |
| Using TDP for PDF + Word + Excel in same project | Evaluate carefully — TDP covers more document formats; you may keep Flow/Spreadsheet for non-PDF work |
| Telerik UI suite already in use (bundled DPL) | Evaluate migration cost vs ongoing footprint — bundled does not mean free of upgrade overhead |
| Docker/cloud deployment where package footprint matters | Switch — IronPDF has a smaller, focused dependency set |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All Telerik Document Processing References

```bash
# Find TDP API usage across both document models
rg -l "Telerik\.Windows\.Documents|RadFlowDocument|RadFixedDocument|HtmlFormatProvider|PdfFormatProvider|FixedContentEditor" --type cs
rg "RadFlowDocument|RadFixedDocument|HtmlFormatProvider|PdfFormatProvider" --type cs -n

# Count document-model usage (higher migration cost)
rg "RadFlow|RadFixed|FixedContent|HtmlFormatProvider" --type cs | wc -l

# Find TDP packages in project files
grep -r "Telerik\.Documents\|Telerik\.Windows\.Documents" *.csproj **/*.csproj 2>/dev/null

# List all Telerik packages in use
dotnet list package | grep -i telerik
```

### Uninstall / Install

```bash
# Remove Telerik Document Processing packages
# Cross-platform / .NET Standard names shown; .NET Framework variants are Telerik.Windows.Documents.*
# Note: removing these may affect other Telerik packages — check dependencies first
dotnet remove package Telerik.Documents.Core
dotnet remove package Telerik.Documents.Flow
dotnet remove package Telerik.Documents.Flow.FormatProviders.Pdf
dotnet remove package Telerik.Documents.Fixed
# Remove any other Telerik.Documents.* packages specific to your use

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;
// For PDF manipulation (merge, watermark, security):
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;
```

**After:**
```csharp
using IronPdf;
```

### Step 3 — Basic PDF Generation

**Before (Telerik — HTML → RadFlowDocument → PDF):**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;
using System.IO;

string html = "<html><body><h1>Hello, PDF!</h1></body></html>";

HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
RadFlowDocument document = htmlProvider.Import(html);

PdfFormatProvider pdfProvider = new PdfFormatProvider();
using (FileStream output = File.OpenWrite("output.pdf"))
{
    pdfProvider.Export(document, output);
}
```

**After:**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body style='font-family:Arial; padding:40px'><h1>Hello, PDF!</h1></body></html>"
);
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| Telerik Document Processing | IronPDF | Notes |
|---|---|---|
| `Telerik.Windows.Documents.Flow.Model` | `IronPdf` | Flow document model not needed |
| `Telerik.Windows.Documents.Flow.FormatProviders.Html` | `IronPdf` (`ChromePdfRenderer`) | HTML input is direct |
| `Telerik.Windows.Documents.Flow.FormatProviders.Pdf` | `IronPdf` (`PdfDocument.SaveAs`) | Export handled by pdf object |
| `Telerik.Windows.Documents.Fixed.Model` | `IronPdf` (`PdfDocument`) | Fixed-page model replaced by `PdfDocument` |
| `Telerik.Windows.Documents.Fixed.FormatProviders.Pdf` | `IronPdf` | Load/save unified on `PdfDocument` |

### Core Class Mapping

| Telerik Class | IronPDF Equivalent | Description |
|---|---|---|
| `HtmlFormatProvider` | `ChromePdfRenderer` | Direct HTML rendering |
| `RadFlowDocument` | Not needed | No intermediate model |
| `PdfFormatProvider` (Flow) | `pdf.SaveAs()` / `pdf.Stream` | Export is on the pdf object |
| `RadFixedDocument` | `PdfDocument` | Loaded PDF representation |
| `FixedContentEditor` | `pdf.ApplyWatermark()` / `pdf.ApplyStamp()` | No manual position translation |
| `PdfExportSettings` | `RenderingOptions` / `SecuritySettings` | Configuration is split by concern |

### Document Loading Methods

| Operation | Telerik | IronPDF |
|---|---|---|
| HTML string to PDF | `HtmlFormatProvider.Import(html)` + Flow export | `renderer.RenderHtmlAsPdfAsync(html)` |
| HTML file to PDF | `File.ReadAllText` + `HtmlFormatProvider.Import` | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Render URL | `HttpClient.GetStringAsync` + Flow import | `renderer.RenderUrlAsPdfAsync(url)` |
| Load existing PDF | `PdfFormatProvider.Import(stream)` → `RadFixedDocument` | `PdfDocument.FromFile(path)` |
| Save to stream | `PdfFormatProvider.Export(doc, stream)` | `pdf.Stream.CopyTo(stream)` |

### Page Operations

| Operation | Telerik | IronPDF |
|---|---|---|
| Page count | `doc.Pages.Count` | `pdf.PageCount` |
| Remove page | `doc.Pages.Remove(page)` | `pdf.RemovePages(index)` |
| Extract text | Iterate `RadFixedPage.Content` for `TextFragment` | `pdf.ExtractAllText()` |
| Rotate page | `RadFixedPage.Rotation` | `pdf.RotateAllPages()` / per-page rotation API |

### Merge / Split Operations

| Operation | Telerik | IronPDF |
|---|---|---|
| Merge | Open each PDF as `RadFixedDocument`, copy `Pages` into a new `RadFixedDocument`, export | `PdfDocument.Merge(doc1, doc2)` |
| Split | Copy a page range into a new `RadFixedDocument` and export | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (Telerik Document Processing — Flow Document round-trip):**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;
using System.IO;

class ReportBefore
{
    static void Main()
    {
        // HTML imported into a flow document model, then exported as PDF
        string html = @"
            <html><body>
                <h1>Invoice #2024-0099</h1>
                <p>Customer: Acme Corp</p>
                <p><strong>Total: $4,200.00</strong></p>
            </body></html>";

        HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
        RadFlowDocument document = htmlProvider.Import(html);

        PdfFormatProvider pdfProvider = new PdfFormatProvider();
        using (FileStream output = File.OpenWrite("invoice.pdf"))
        {
            pdfProvider.Export(document, output);
        }
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// HTML template renders directly — no intermediate document model
var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; }
        .title { font-size: 20px; font-weight: bold; }
        .field { margin-top: 8px; font-size: 13px; }
        .total { font-size: 16px; font-weight: bold; margin-top: 20px; }
    </style>
    </head>
    <body>
        <div class='title'>Invoice #2024-0099</div>
        <div class='field'>Customer: Acme Corp</div>
        <div class='total'>Total: $4,200.00</div>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");
Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (Telerik Document Processing — Fixed model, per-page copy):**
```csharp
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;
using System.IO;

class MergeBefore
{
    static void Main()
    {
        PdfFormatProvider provider = new PdfFormatProvider();

        RadFixedDocument document1;
        using (FileStream input = File.OpenRead("section1.pdf"))
        {
            document1 = provider.Import(input);
        }

        RadFixedDocument document2;
        using (FileStream input = File.OpenRead("section2.pdf"))
        {
            document2 = provider.Import(input);
        }

        // Merging is manual page copying between fixed documents
        RadFixedDocument merged = new RadFixedDocument();
        foreach (var page in document1.Pages)
        {
            merged.Pages.Add(page);
        }
        foreach (var page in document2.Pages)
        {
            merged.Pages.Add(page);
        }

        using (FileStream output = File.OpenWrite("merged.pdf"))
        {
            provider.Export(merged, output);
        }
    }
}
```

**After:**
```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

var results = await Task.WhenAll(
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 1</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 2</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (Telerik Document Processing — FixedContentEditor with manual positioning):**
```csharp
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.Model.ColorSpaces;
using Telerik.Windows.Documents.Fixed.Model.Editing;
using System.IO;

class WatermarkBefore
{
    static void Main()
    {
        PdfFormatProvider provider = new PdfFormatProvider();

        RadFixedDocument document;
        using (FileStream input = File.OpenRead("source.pdf"))
        {
            document = provider.Import(input);
        }

        // Watermark drawn via FixedContentEditor — translate, rotate, then draw
        foreach (RadFixedPage page in document.Pages)
        {
            FixedContentEditor editor = new FixedContentEditor(page);
            editor.GraphicProperties.IsFilled = true;
            editor.GraphicProperties.FillColor = new RgbColor(200, 200, 200, 200);

            editor.Position.Translate(page.Size.Width / 2, page.Size.Height / 2);
            editor.Position.Rotate(45);

            Block block = new Block();
            block.TextProperties.FontSize = 72;
            block.InsertText("DRAFT");

            editor.DrawBlock(block);
        }

        using (FileStream output = File.OpenWrite("watermarked.pdf"))
        {
            provider.Export(document, output);
        }
    }
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Editing;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Document</h1></body></html>"
);

// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = "#808080",
    Opacity = 15, // 0–100
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (Telerik Document Processing — PdfExportSettings on the Fixed provider):**
```csharp
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export;
using Telerik.Windows.Documents.Fixed.Model;
using System.IO;

class PasswordBefore
{
    static void Main()
    {
        PdfFormatProvider provider = new PdfFormatProvider();

        RadFixedDocument document;
        using (FileStream input = File.OpenRead("source.pdf"))
        {
            document = provider.Import(input);
        }

        PdfExportSettings exportSettings = new PdfExportSettings();
        exportSettings.UserPassword = "open123";
        exportSettings.OwnerPassword = "admin456";

        provider.ExportSettings = exportSettings;

        using (FileStream output = File.OpenWrite("secured.pdf"))
        {
            provider.Export(document, output);
        }
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Confidential</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### The Two-Model Architecture → Single `PdfDocument`

Telerik Document Processing exposes two distinct PDF-facing object models:

- **`RadFlowDocument`** — flow layout, used for HTML import and Word-style content. Reached via `HtmlFormatProvider.Import()`.
- **`RadFixedDocument`** — fixed page layout, used for loading existing PDFs and page-level manipulation (merge, watermark, security). Reached via `PdfFormatProvider.Import()` from the `Fixed` namespace.

IronPDF collapses both into a single `PdfDocument` produced by `ChromePdfRenderer.RenderHtmlAsPdfAsync()` or `PdfDocument.FromFile()`. Once you have a `PdfDocument`, merge/watermark/security/extraction all hang off the same object.

| TDP Pattern | IronPDF Equivalent |
|---|---|
| `HtmlFormatProvider.Import(html)` → `RadFlowDocument` → `PdfFormatProvider.Export()` | `renderer.RenderHtmlAsPdfAsync(html)` → `pdf.SaveAs()` |
| `PdfFormatProvider.Import(stream)` → `RadFixedDocument` | `PdfDocument.FromFile(path)` |
| `FixedContentEditor.DrawBlock()` for overlays | `pdf.ApplyStamp()` / `pdf.ApplyWatermark()` |
| Manual page copy between `RadFixedDocument` instances | `PdfDocument.Merge(a, b)` |

For complex positioned layouts that were previously hand-built with `FixedContentEditor.Position.Translate()`, the migration is a design exercise — the HTML template is what ships, not translated coordinate code.

### `PdfFormatProvider.Export(doc, stream)` → `pdf.SaveAs()`

The most common Telerik export pattern maps directly:

```csharp
// Telerik:
// var provider = new PdfFormatProvider();
// using (var stream = File.OpenWrite("out.pdf"))
//     provider.Export(document, stream);

// IronPDF:
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("out.pdf");
// Or: pdf.Stream.CopyTo(yourStream);
```

### Package Removal Is Gradual

If your application uses Telerik for PDF and also for Word/Excel documents, remove the PDF-specific packages while keeping Flow (for Word) and Spreadsheet (for Excel). Don't remove the entire `Telerik.Documents` suite at once if it serves multiple use cases.

```bash
# Check what each Telerik.Documents package is used for before removing
rg "Telerik\.Windows\.Documents\.Fixed" --type cs  # PDF page model
rg "Telerik\.Windows\.Documents\.Flow" --type cs   # Word / HTML import
rg "Telerik\.Windows\.Documents\.Spreadsheet" --type cs  # Excel
```

---

## Performance Considerations

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// https://ironpdf.com/examples/parallel/
var pdfs = await Task.WhenAll(reportData.Select(async data =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(BuildHtml(data));
}));

foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Disposal vs `PdfFormatProvider` Pattern

```csharp
using IronPdf;
using System.IO;

// Telerik: provider.Export(doc, stream) — caller owns the stream
// IronPDF: 'using' on PdfDocument handles disposal

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// Safe export patterns:
pdf.SaveAs("output.pdf");               // file
pdf.Stream.CopyTo(existingStream);      // stream
var bytes = pdf.BinaryData;             // bytes
// pdf disposed automatically at end of 'using' block
```

---

## Migration Checklist

### Pre-Migration
- [ ] Count Flow + Fixed usage (`rg "RadFlow|RadFixed|FixedContent|HtmlFormatProvider" --type cs | wc -l`)
- [ ] Identify which Telerik.Documents packages are PDF-specific vs Word/Excel
- [ ] Confirm which Telerik UI suite or DevCraft bundle currently includes your Document Processing license
- [ ] Measure current install footprint (`dotnet list package --include-transitive | wc -l`)
- [ ] Document features in use: HTML-to-PDF, merge, security, watermarks, text extraction
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility
- [ ] Plan HTML template rewrite for each document type that currently uses Flow Document construction

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `Telerik.Documents.Core`, `Telerik.Documents.Flow`, `Telerik.Documents.Flow.FormatProviders.Pdf`, `Telerik.Documents.Fixed`, and related PDF-only packages
- [ ] Add license key at application startup
- [ ] Replace `HtmlFormatProvider.Import()` + `PdfFormatProvider.Export()` with `ChromePdfRenderer.RenderHtmlAsPdfAsync()` + `pdf.SaveAs()`
- [ ] Replace `RadFixedDocument` page-copy merge with `PdfDocument.Merge()`
- [ ] Replace `FixedContentEditor` watermark drawing with `pdf.ApplyWatermark()` or `TextStamper`
- [ ] Replace `PdfExportSettings.UserPassword` / `OwnerPassword` with `pdf.SecuritySettings`
- [ ] Replace `HttpClient` + Flow import URL workflow with `renderer.RenderUrlAsPdfAsync()`
- [ ] Replace `RadFixedPage.Content` text iteration with `pdf.ExtractAllText()`

### Testing
- [ ] Compare PDF output visually against TDP reference exports (expect CSS layout improvements)
- [ ] Verify package footprint reduction (`dotnet list package | grep -i telerik`)
- [ ] Test merge output page count and order
- [ ] Test password protection
- [ ] Benchmark render time vs TDP baseline
- [ ] Test Docker/Linux deployment
- [ ] Verify concurrent rendering behavior

### Post-Migration
- [ ] Confirm remaining Telerik packages are still needed (Word/Excel use cases)
- [ ] Update bundle size metrics if applicable
- [ ] Archive TDP document model code for reference

---

## Wrapping Up

The package footprint reduction is often the most immediately measurable outcome. Running `dotnet list package --include-transitive` before and after the PDF-specific Telerik packages are removed shows the size delta.

The Flow-Document-to-HTML rewrite is the main creative work. The most complex TDP layouts — those built imperatively via `FixedContentEditor` with precise positioning, overlapping elements, or complex table structures — take the most HTML design work to replicate. Simpler single-column report layouts that already started as HTML strings are fast because the HTML stays largely intact and just gets handed to `ChromePdfRenderer` instead.

**Discussion question:** After migrating, what were your before/after figures on package count or bundle size — and did the switch in rendering model (Flow Document conversion vs Chromium HTML) require significant template redesign work?
