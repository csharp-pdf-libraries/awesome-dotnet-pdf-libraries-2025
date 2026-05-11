---
title: "The VectSharp to IronPDF migration nobody dramatised"
published: false
tags: dotnet, csharp, pdf, migration
---

PDF generation is the feature that surfaces VectSharp's scope limits. VectSharp is a vector graphics library — it draws shapes, lines, text, and figures with precision. What it doesn't do natively is take an HTML string and produce a paginated PDF from it. Teams that chose VectSharp for diagram and chart rendering eventually hit the point where they also need report-style HTML output, and the two requirements are architecturally separate. Adding a second library to handle HTML-to-PDF creates a dependency split that compounds over time.

This article covers migrating from VectSharp's PDF output to IronPDF. You'll have working before/after code for HTML-to-PDF, merge, watermark, and password protection by the end. The troubleshooting section and comparison tables are useful regardless of which replacement you choose.

---

## Why Migrate (Without Drama)

Teams migrating from VectSharp to IronPDF for PDF generation typically encounter:

1. **HTML input gap** — VectSharp generates PDFs from drawing operations; generating from HTML requires converting HTML to VectSharp drawing calls, which is a significant intermediate step.
2. **Layout calculation burden** — positioning text and layout elements requires explicit coordinate calculation; HTML/CSS handles this automatically.
3. **CSS/web content source** — if the input data comes from a web template or Razor view, VectSharp requires converting that to drawing API calls.
4. **Table and list rendering** — complex tabular layouts that are trivial in HTML (`<table>`) require manual coordinate math in VectSharp.
5. **Font management** — VectSharp has its own font loading mechanism (FontLibrary); web fonts from Google Fonts or embedded CSS `@font-face` aren't directly usable.
6. **No PDF manipulation** — VectSharp creates PDFs; it doesn't merge, watermark, add passwords, or extract text from existing PDFs.
7. **Two-library pattern** — teams using VectSharp for diagrams + another library for HTML-to-PDF carry both dependencies.
8. **Page numbering** — adding page numbers across a multi-page document in VectSharp requires manual tracking; HTML footer patterns handle this automatically.
9. **Image-heavy report layouts** — embedding images in VectSharp PDFs requires explicit rasterization or SVG embedding steps.
10. **Team familiarity** — teams with web development backgrounds find HTML/CSS templates easier to maintain than coordinate-based drawing code.

### Comparison Table

| Aspect | VectSharp | IronPDF |
|---|---|---|
| Focus | Vector graphics creation → PDF/SVG/raster output | HTML-to-PDF + PDF manipulation |
| Pricing | Open source (LGPL-3.0) | Commercial (see ironsoftware.com for current plans) |
| API Style | `Graphics.FillText`, `FillPath`, `FillRectangle` | `ChromePdfRenderer` + HTML input |
| Learning Curve | Low for devs with graphics background; harder for report layouts | Low for web devs; HTML/CSS is the input |
| HTML Rendering | Not applicable | Embedded Chromium |
| Page Indexing | 0-based | 0-based |
| Thread Safety | Single-threaded `Graphics` per page | Use one `ChromePdfRenderer` per concurrent task |
| Namespace | `VectSharp`, `VectSharp.PDF` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | VectSharp | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | Not applicable | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low (new capability) |
| Drawing-based PDF | `Graphics.*` drawing API | HTML/CSS template | High (rewrite) |
| Save to file | `Document.SaveAsPDF(path)` | `pdf.SaveAs(path)` | Low |
| Save to stream | `Document.SaveAsPDF(stream)` | `pdf.Stream` / `pdf.BinaryData` | Low |
| Custom page size | `Page(width, height)` constructor | `RenderingOptions.PaperSize` | Low |
| Merge PDFs | Not native | `PdfDocument.Merge()` | Medium |
| Watermark | Drawing layer (manual) | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Not native | `pdf.SecuritySettings` | Low |
| Text extraction | Not applicable | `pdf.ExtractAllText()` | Medium |
| Diagrams/charts | Native strength — vector drawing | Via HTML + JS chart libraries | High |
| Custom fonts | `FontLibrary` system | CSS `@font-face` / Google Fonts | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Primary need is HTML/CSS report generation | Switch — VectSharp is drawing-first; IronPDF is HTML-first |
| Primary need is precise diagram/chart rendering from data | Keep VectSharp for diagrams; add IronPDF for HTML reports |
| Both diagrams and HTML reports needed | Both libraries together; scope carefully |
| PDF merge, watermark, security on existing PDFs | Switch for those operations — VectSharp doesn't cover them |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All VectSharp References

```bash
# Find VectSharp usage
rg -l "VectSharp\|VectSharp\.PDF\|PDFContextInterpreter" --type cs
rg "VectSharp\|PDFContextInterpreter\|\.DrawString\b" --type cs -n

# Count drawing API vs file save usage
rg "Graphics\.\|DrawString\|FillPath\|DrawRectangle" --type cs | wc -l
rg "SaveAsPDF\b\|\.PDF\b" --type cs -n

# Find VectSharp NuGet references
grep -r "VectSharp" *.csproj **/*.csproj 2>/dev/null
```

### Uninstall / Install

```bash
# Remove VectSharp packages (PDF first, then core)
dotnet remove package VectSharp.PDF
dotnet remove package VectSharp

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
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
    ?? throw new InvalidOperationException("IRONPDF_LICENSE_KEY not set");
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using VectSharp;
using VectSharp.PDF;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic PDF Generation

**Before (VectSharp drawing API):**
```csharp
using VectSharp;
using VectSharp.PDF;
using System;

class Program
{
    static void Main()
    {
        // VectSharp: draw content via graphics API
        var doc = new Document();
        var page = new Page(595, 842); // A4 in points
        doc.Pages.Add(page);

        var g = page.Graphics;

        // VectSharp resolves standard fonts via FontFamily.ResolveFontFamily(...)
        var font = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 24);
        g.FillText(40, 40, "Hello, PDF!", font, Colours.Black);

        doc.SaveAsPDF("output.pdf"); // SaveAsPDF is an extension method from VectSharp.PDF
        Console.WriteLine("Saved output.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// HTML replaces drawing API — no coordinate calculation
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

| VectSharp | IronPDF | Notes |
|---|---|---|
| `VectSharp` | `IronPdf` | Core namespace |
| `VectSharp.PDF` | `IronPdf` (PDF output on result) | No separate PDF package needed |
| N/A | `IronPdf.Editing` | Watermark / stamp |

### Core Class Mapping

| VectSharp Class | IronPDF Class | Description |
|---|---|---|
| `Document` (VectSharp) | `ChromePdfRenderer` | Different model — HTML is the input |
| `Page` | `RenderingOptions.PaperSize` | Page configuration |
| `Graphics` | N/A — CSS handles layout | No drawing API needed |
| N/A | `PdfDocument` | PDF manipulation object |

### Document Loading Methods

| Operation | VectSharp | IronPDF |
|---|---|---|
| Create from drawing | `new Document()` + Graphics | `renderer.RenderHtmlAsPdfAsync(html)` |
| Render URL | Not applicable | `renderer.RenderUrlAsPdfAsync(url)` |
| Load existing PDF | Not supported | `PdfDocument.FromFile(path)` |
| Save to file | `doc.SaveAsPDF(path)` | `pdf.SaveAs(path)` |

### Page Operations

| Operation | VectSharp | IronPDF |
|---|---|---|
| Page count | `doc.Pages.Count` | `pdf.PageCount` |
| Add page | `doc.Pages.Add(new Page(...))` | Via HTML page breaks / multi-page content |
| Extract text | Not applicable | `pdf.ExtractAllText()` |
| Rotate | Via page transform | `pdf.RotateAllPages(PageRotation.Clockwise90)` |

### Merge / Split Operations

| Operation | VectSharp | IronPDF |
|---|---|---|
| Merge | Not native | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not native | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. PDF Generation (Drawing API → HTML)

**Before (VectSharp drawing API for a report-style document):**
```csharp
using VectSharp;
using VectSharp.PDF;
using System;

class ReportBefore
{
    static void Main()
    {
        // VectSharp: explicit coordinate positioning for every element

        var doc = new Document();
        var page = new Page(595, 842); // A4 in points (72 DPI)
        doc.Pages.Add(page);
        var g = page.Graphics;

        // Title — manual coordinate (40, 40)
        var titleFont = new Font(
            FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 20);
        g.FillText(40, 40, "Q3 Invoice #4421", titleFont, Colours.Black);

        // Customer field — manual coordinate (40, 80)
        var bodyFont = new Font(
            FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);
        g.FillText(40, 80, "Customer: Acme Corp", bodyFont, Colours.Black);

        // Amount — manual coordinate (40, 110)
        g.FillText(40, 110, "Total: $8,400.00", bodyFont, Colours.Black);

        doc.SaveAsPDF("invoice.pdf");
        Console.WriteLine("Saved invoice.pdf — manual coordinates for every element");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// HTML/CSS handles layout — no manual coordinate math
var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; color: #222; }
        h1 { font-size: 20px; font-weight: bold; }
        .field { margin-top: 8px; font-size: 13px; }
        .total { font-size: 16px; font-weight: bold; margin-top: 20px; }
    </style>
    </head>
    <body>
        <h1>Q3 Invoice #4421</h1>
        <div class='field'>Customer: Acme Corp</div>
        <div class='total'>Total: $8,400.00</div>
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

**Before (VectSharp — not native; workaround via page copying):**
```csharp
using VectSharp;
using VectSharp.PDF;
using System;

class MergeBefore
{
    static void Main()
    {
        // VectSharp builds Documents from scratch via Graphics calls.
        // It does not import pages from existing PDFs — merging existing PDFs
        // is outside the library's scope.

        var merged = new Document();

        // Create section A (illustrative)
        var sectionA = new Document();
        var pageA = new Page(595, 842);
        sectionA.Pages.Add(pageA);
        var gA = pageA.Graphics;
        // ... draw section A content ...

        // Create section B (illustrative)
        var sectionB = new Document();
        var pageB = new Page(595, 842);
        sectionB.Pages.Add(pageB);
        var gB = pageB.Graphics;
        // ... draw section B content ...

        // To "merge" with VectSharp, you build a single Document and append
        // freshly drawn pages. Importing pages from existing PDF files is not
        // supported by VectSharp.PDF.
        Console.WriteLine("VectSharp builds new PDFs — importing existing pages is not supported");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();

var results = await Task.WhenAll(
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section A</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section B</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (VectSharp — drawing layer on existing pages):**
```csharp
using VectSharp;
using VectSharp.PDF;
using System;

class WatermarkBefore
{
    static void Main()
    {
        // VectSharp: watermark by drawing semi-transparent text on each page.
        // VectSharp creates PDFs from scratch; modifying existing PDFs is out of scope.

        var doc = new Document();
        var page = new Page(595, 842);
        doc.Pages.Add(page);
        var g = page.Graphics;

        // Main content
        var bodyFont = new Font(
            FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 14);
        g.FillText(40, 40, "Document Content", bodyFont, Colours.Black);

        // Draw rotated semi-transparent text via a coordinate transform.
        // Colour.FromRgba(r, g, b, a) uses doubles in 0..1 range.
        var watermarkFont = new Font(
            FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 60);
        g.Save();
        g.Translate(297, 421); // center of A4 page
        g.Rotate(-Math.PI / 4); // -45 degrees
        g.FillText(-100, 0, "DRAFT", watermarkFont, Colour.FromRgba(0.5, 0.5, 0.5, 0.15));
        g.Restore();

        doc.SaveAsPDF("watermarked.pdf");
        Console.WriteLine("Watermark drawn into newly generated PDF");
    }
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Editing;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Document Content</h1></body></html>"
);

// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontFamily = "Arial",
    FontSize = 60,
    TextColor = "#808080",
    Opacity = 15, // 0–100
    Rotation = -45,
    IsStampBehindContent = false,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (VectSharp — not supported):**
```csharp
using VectSharp;
using VectSharp.PDF;
using System;

class PasswordBefore
{
    static void Main()
    {
        // VectSharp.PDF has no built-in password protection API.
        // Protection requires a secondary library after SaveAsPDF.

        var doc = new Document();
        var page = new Page(595, 842);
        doc.Pages.Add(page);
        // ... build document ...
        doc.SaveAsPDF("temp.pdf");

        // Password via secondary library (illustrative):
        // var bytes = File.ReadAllBytes("temp.pdf");
        // var secured = SomePdfLib.SetPassword(bytes, "open123", "admin456");
        // File.WriteAllBytes("secured.pdf", secured);

        Console.WriteLine("VectSharp.PDF has no native password API — secondary library required");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Protected Document</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Troubleshooting Common Migration Issues

### "Drawing API has no HTML equivalent — how do I migrate a complex diagram?"

**Symptom:** A VectSharp drawing that generates a precise chart or diagram — with exact coordinate positioning, bezier curves, custom paths — has no direct HTML/CSS equivalent.

**Resolution:** For diagram content, VectSharp's SVG output is the bridge:

```csharp
// Option A: Keep VectSharp for diagrams, use IronPDF for report wrapper
// Step 1: Render diagram as SVG with VectSharp
using VectSharp;
using VectSharp.SVG;
using System;

var doc = new Document();
var page = new Page(400, 300);
doc.Pages.Add(page);
var g = page.Graphics;
// ... draw your diagram ...
// VectSharp.SVG exposes a SaveAsSVG extension on Page:
// page.SaveAsSVG("diagram.svg");

// Step 2: Embed SVG in HTML for IronPDF
using IronPdf;
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var svgContent = System.IO.File.ReadAllText("diagram.svg");
var html = $@"
    <html><body style='padding:40px'>
        <h1>Report with Embedded Diagram</h1>
        {svgContent}
        <p>Diagram analysis...</p>
    </body></html>";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("report-with-diagram.pdf");
```

### "Font in VectSharp output doesn't match font in IronPDF output"

**Symptom:** A report was using VectSharp's `FontFamily.StandardFontFamilies.Helvetica` and after migration to IronPDF, the font looks different.

**Root cause:** Helvetica is a licensed font. Standard PDF font embedding in VectSharp uses Type 1 standard fonts. IronPDF renders via Chromium using system fonts or web fonts — Helvetica may not be available and fallback to Arial or system sans-serif.

**Resolution:**
```csharp
// Specify font explicitly in HTML (closer control)
var html = @"
    <html>
    <head>
    <style>
        /* Use Arial as the near-equivalent to Helvetica on most systems */
        body { font-family: Arial, 'Helvetica Neue', Helvetica, sans-serif; }
        /* Or use Google Fonts for exact web font */
        /* @import url('https://fonts.googleapis.com/css2?family=Inter'); */
    </style>
    </head>
    <body><h1>Report Title</h1></body>
    </html>";
```

### "Page size in VectSharp was in points, IronPDF uses different units"

**Symptom:** VectSharp pages are specified in points (`new Page(595, 842)` for A4). IronPDF rendering options use enum or millimeters.

**Resolution:**
```csharp
using IronPdf.Rendering;

// Instead of specifying raw points, use the enum:
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
// Or Letter, A3, A5, etc.

// For custom sizes, IronPDF accepts pixels or points:
// renderer.RenderingOptions.SetCustomPaperSizeinPixelsOrPoints(595, 842);
// See: https://ironpdf.com/how-to/rendering-options/
```

### "SaveAsPDF is gone — what's the stream equivalent?"

**Symptom:** VectSharp's `document.SaveAsPDF(stream)` was used to write to a `MemoryStream` for API responses.

**Resolution:**
```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// Option 1: get bytes directly
var bytes = pdf.BinaryData;
return File(bytes, "application/pdf", "output.pdf");

// Option 2: copy stream
using var ms = new MemoryStream();
pdf.Stream.CopyTo(ms);
return ms.ToArray();
// pdf disposed at end of 'using' block
```

---

## Critical Migration Notes

### VectSharp Is Not HTML-First — This Is a Model Change

The migration from VectSharp to IronPDF for HTML-to-PDF isn't a library swap — it's a model change. VectSharp's model is: describe what to draw, where to draw it. IronPDF's model is: provide HTML, get back a PDF.

If the existing codebase builds content via VectSharp drawing calls, those drawing calls become HTML/CSS. If it builds content via HTML templates, IronPDF is the natural fit and the migration is more direct.

### Diagrams Are VectSharp's Strength — Consider Keeping Both

For precise data-driven diagrams, VectSharp's vector drawing API can be more appropriate than trying to achieve equivalent precision with SVG + JavaScript chart libraries. Consider keeping VectSharp for the diagram component and adding IronPDF for HTML report generation. The bridge is SVG export from VectSharp embedded in HTML rendered by IronPDF.

### Page Indexing

Both VectSharp and IronPDF use 0-based page indexing. This typically doesn't require changes for page manipulation code.

---

## Performance Considerations

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/examples/parallel/
var pdfs = await Task.WhenAll(reportDataList.Select(async data =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(BuildHtml(data));
}));

foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Disposal Pattern

```csharp
using IronPdf;

// VectSharp Document: no explicit disposal (managed objects)
// IronPDF PdfDocument: 'using' block recommended

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("output.pdf");
// pdf disposed automatically
```

---

## Migration Checklist

### Pre-Migration
- [ ] Count drawing API call sites (`rg "g\.Fill\|g\.Draw\|FillText" --type cs | wc -l`)
- [ ] Identify diagram vs report-style code (diagrams may stay with VectSharp)
- [ ] Find VectSharp SVG export usage — bridge for embedded diagrams
- [ ] Identify secondary libraries used for merge/security (not native to VectSharp)
- [ ] Document current page sizes used (`new Page(width, height)` values)
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `VectSharp` and `VectSharp.PDF` packages (if not keeping for diagrams)
- [ ] Add license key at application startup
- [ ] Convert each drawing-based PDF generation to HTML template
- [ ] Replace `doc.SaveAsPDF(path)` with `pdf.SaveAs(path)`
- [ ] Replace `doc.SaveAsPDF(stream)` with `pdf.Stream.CopyTo(stream)` or `pdf.BinaryData`
- [ ] Add merge capability via `PdfDocument.Merge()` if needed
- [ ] Add watermark via `TextStamper` / `ImageStamper`
- [ ] Add password protection via `pdf.SecuritySettings`
- [ ] Bridge diagram content via VectSharp SVG → HTML `<img>` or inline SVG

### Testing
- [ ] Compare PDF output visually against VectSharp reference
- [ ] Verify font rendering in deployment environment
- [ ] Test page size and margin match reference
- [ ] Test merge, watermark, and security
- [ ] Benchmark render time for typical document sizes
- [ ] Test in Docker/Linux if applicable

### Post-Migration
- [ ] Remove VectSharp packages if no longer needed for diagrams
- [ ] Remove secondary PDF libraries replaced by IronPDF
- [ ] Archive VectSharp drawing code for reference
- [ ] Update documentation to note HTML template approach

---

## Next Steps

The drawing API to HTML transition is the core challenge in this migration. For report-style documents — tabular data, text, simple formatting — HTML is generally simpler to write and maintain than coordinate-based drawing code. For precision diagrams, VectSharp's approach is often more appropriate, and the SVG bridge keeps both tools working in the same pipeline.

The missing features (merge, watermark, security, text extraction) migrate with minimal effort since they're additive — new API calls on existing rendered output.

**Discussion question:** What version of VectSharp were you migrating from, and did anything break unexpectedly — particularly around the font management system or the drawing-to-HTML template conversion for complex layouts?

