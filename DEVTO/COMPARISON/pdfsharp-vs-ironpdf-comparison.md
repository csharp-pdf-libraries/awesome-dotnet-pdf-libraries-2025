---
title: "PDFsharp vs IronPDF: a developer comparison"
published: false
tags: dotnet, csharp, pdf, comparison
---

*Canonical/source version on Iron Software blog: https://ironsoftware.com/suite/blog*

## Performance isn't always about raw speed

Per-document throughput is one axis; per-layout-change developer time is another. A coordinate-based renderer can produce an invoice in tens of milliseconds, yet a single "move the footer down five points and add a tax breakdown" request can cascade through hundreds of lines of positioning math. That trade-off — fast execution, expensive iteration — is the recurring story of PDFsharp on template-heavy workloads.

PDFsharp is a low-level PDF construction library (MIT-licensed, originally by empira Software) built for explicit control. You position every element using X/Y coordinates in points (1/72 inch). This gives pixel-perfect precision but requires understanding PDF coordinate systems, font metrics, and layout algorithms. The library excels when you're building visualizations, rendering CAD drawings, or creating geometric designs where explicit positioning is the point. For document generation from templates, you're implementing a layout engine from scratch — or pairing PDFsharp with its companion MigraDoc library for flow content.

## Understanding IronPDF

IronPDF trades explicit control for development velocity. Instead of calculating coordinates, you write HTML/CSS templates and let Chromium handle layout, text wrapping, pagination, and font rendering. The performance characteristic shifts: higher per-document overhead (Chromium startup), but zero layout development time. For teams with web development skills, this is often a net win—you ship features instead of debugging coordinate math.

The architecture is straightforward: `ChromePdfRenderer` wraps a Chromium process, HTML/CSS determines layout, `PdfDocument` handles the result. Installation is one NuGet package with Chromium bundled. The library runs cross-platform and supports modern CSS (Flexbox, Grid, media queries). For performance-critical batch operations, renderer reuse and async methods help amortize Chromium startup costs.

## Key Characteristics of PDFsharp

### **Product Status**

Open-source under the MIT license, actively maintained on the 6.x line by the PDFsharp-Team (origin: empira Software). The 6.x line targets modern .NET and supersedes pre-6 APIs, which differ significantly; migration notes are documented by the maintainers. A community .NET Standard port, `PdfSharpCore` (ststeiger/PdfSharpCore), is also MIT-licensed. Commercial support arrangements are available from empira for teams that need them.

### **Scope of the Library**

PDFsharp is a graphics-and-PDF-primitives API, not a template engine. It does not perform HTML-to-PDF rendering; there is no CSS, JavaScript, or web-font pipeline built in. The community add-on `HtmlRenderer.PdfSharp` covers HTML 4.01 / CSS level 2 only — no flexbox, grid, or JavaScript. Tables, text wrapping, and pagination are handled by the calling code (or by MigraDoc, the companion flow-document library by the same authors).

### **Programming Model**

Coordinate-based positioning requires calculating point locations for every element. Font metrics must be queried to determine text width/height before positioning. Multi-column layouts require manual column-width calculations. Page breaks need explicit height tracking and page insertion logic. UTF-8/Unicode works but requires careful font selection (not all fonts support all glyphs). Image positioning requires explicit sizing and coordinate placement. Note that PDFsharp 6.x renamed `XFontStyle` to `XFontStyleEx`.

### **Support Channels**

Community-driven support via GitHub Issues and the project forum (forum.pdfsharp.net), with documented commercial options from empira (pdfsharp.com). API reference is comprehensive; deeper step-by-step guides for complex scenarios are thinner. Stack Overflow carries an active PDFsharp tag.

### **Architectural Notes**

The low-level API exposes PDF primitives directly — strong for control, verbose for common tasks. There is no built-in separation between document structure and presentation: you specify coordinates inline with content. Three build variants (Core / GDI+ / WPF) have different imaging capabilities — the Core build typically needs a custom font resolver on non-Windows platforms. There is no built-in template system; teams build one or layer MigraDoc on top.

---

## Feature Comparison Overview

| Aspect | PDFsharp | IronPDF |
|--------|----------|---------|
| **Current Status** | Active (MIT open-source, 6.x line) | Active (commercial) |
| **HTML Support** | None (coordinate-based API) | Chromium-based engine |
| **Layout Model** | Explicit X/Y positioning | Browser layout (flow + CSS) |
| **Installation** | Single package (multiple builds) | Single package |
| **Support** | Community + commercial options | Commercial |
| **Companion Library** | MigraDoc for flow content | N/A |

---

## Performance Patterns (Test in Your Environment)

### Pattern 1: Simple Invoice Generation

#### PDFsharp — Coordinate-Based Invoice

```csharp
// Install-Package PDFsharp  (PDFsharp-Team 6.x, MIT)

using System;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

public class InvoiceGenerator
{
    public void GenerateInvoice()
    {
        // Create document
        PdfDocument document = new PdfDocument();
        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);

        // Define fonts (explicit sizing). Note: PDFsharp 6.x renamed XFontStyle to XFontStyleEx.
        XFont fontTitle = new XFont("Arial", 24, XFontStyleEx.Bold);
        XFont fontNormal = new XFont("Arial", 12, XFontStyleEx.Regular);
        XFont fontBold = new XFont("Arial", 12, XFontStyleEx.Bold);
        
        // Calculate positions (points from top-left)
        double leftMargin = 50;
        double topMargin = 50;
        double currentY = topMargin;
        
        // Draw title
        gfx.DrawString("INVOICE #2026-001", fontTitle, XBrushes.Black, leftMargin, currentY);
        currentY += 40;
        
        // Draw invoice info
        gfx.DrawString($"Date: {DateTime.Now:yyyy-MM-dd}", fontNormal, XBrushes.Black, leftMargin, currentY);
        currentY += 20;
        gfx.DrawString("Customer: Acme Corp", fontNormal, XBrushes.Black, leftMargin, currentY);
        currentY += 40;
        
        // Draw table header (manual rectangle + text positioning)
        double tableTop = currentY;
        double col1X = leftMargin;
        double col2X = leftMargin + 300;
        double rowHeight = 25;
        
        // Header background
        XRect headerRect = new XRect(col1X, tableTop, page.Width - 100, rowHeight);
        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(240, 240, 240)), headerRect);
        
        // Header text
        gfx.DrawString("Description", fontBold, XBrushes.Black, col1X + 5, tableTop + 17);
        gfx.DrawString("Amount", fontBold, XBrushes.Black, col2X + 5, tableTop + 17);
        currentY += rowHeight;
        
        // Draw table row
        gfx.DrawString("Consulting Services", fontNormal, XBrushes.Black, col1X + 5, currentY + 17);
        gfx.DrawString("$5,000.00", fontNormal, XBrushes.Black, col2X + 5, currentY + 17);
        currentY += rowHeight;
        
        // Draw line
        gfx.DrawLine(XPens.Black, col1X, currentY, col2X + 100, currentY);
        currentY += 10;
        
        // Total
        gfx.DrawString("Total:", fontBold, XBrushes.Black, col1X + 5, currentY + 17);
        gfx.DrawString("$5,000.00", fontBold, XBrushes.Black, col2X + 5, currentY + 17);
        
        // Save
        document.Save("invoice_pdfsharp.pdf");
    }
}
```

**Performance characteristics:**

- **Fast execution**: minimal overhead and direct PDF writing — per-document timings are typically tens of milliseconds on commodity hardware (benchmark in your own environment)
- **High up-front coding cost**: ~40 lines of positioning code per page; layout changes cascade
- **Memory efficient**: streaming writes, low heap allocation
- **No startup delay**: instantiates quickly with no external processes
- **Scalability**: handles batch operations linearly
- **Maintenance cost**: layout changes require coordinate recalculation

#### IronPDF — HTML Template Invoice

```csharp
// Install-Package IronPdf

using IronPdf;

public class InvoiceGenerator
{
    public void GenerateInvoice()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var htmlTemplate = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; margin: 50px; }
                    h1 { font-size: 24px; font-weight: bold; }
                    .info { margin: 20px 0; }
                    table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                    th { background-color: #f0f0f0; padding: 10px; text-align: left; font-weight: bold; }
                    td { padding: 10px; border-bottom: 1px solid #ccc; }
                    .total { font-weight: bold; margin-top: 10px; }
                </style>
            </head>
            <body>
                <h1>INVOICE #2026-001</h1>
                <div class='info'>
                    <p>Date: 2026-02-11</p>
                    <p>Customer: Acme Corp</p>
                </div>
                <table>
                    <tr><th>Description</th><th>Amount</th></tr>
                    <tr><td>Consulting Services</td><td>$5,000.00</td></tr>
                </table>
                <p class='total'>Total: $5,000.00</p>
            </body>
            </html>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlTemplate);
        pdf.SaveAs("invoice_ironpdf.pdf");
    }
}
```

**Performance characteristics:**

- **Higher per-document overhead**: Chromium process startup typically adds a couple hundred milliseconds on first render (amortized with renderer reuse)
- **Lower development cost**: ~25 lines of HTML/CSS; layout changes are CSS edits
- **Moderate memory**: Chromium process memory footprint in the tens of MB baseline
- **Startup delay**: first render slower, subsequent renders faster with reuse
- **Scalability pattern**: use renderer reuse and async methods for batch operations
- **Maintainability**: designers update HTML/CSS without touching C# code

**Performance optimization for IronPDF batch operations:**

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Reuse renderer for batch operations
var renderer = new ChromePdfRenderer();

foreach (var order in orders) // 10,000 orders
{
    var html = GenerateInvoiceHtml(order);
    var pdf = renderer.RenderHtmlAsPdf(html); // Renderer reuse amortizes startup
    pdf.SaveAs($"invoice_{order.Id}.pdf");
}
// Chromium process started once, not 10,000 times
```

For batch performance patterns, see [IronPDF performance optimization](https://ironsoftware.com/suite/blog/using-ironsuite/pdf-sdk-guide/).

---

### Pattern 2: PDF Merging Operations

#### PDFsharp — Manual Merge Implementation

```csharp
// Install-Package PDFsharp  (PDFsharp-Team 6.x, MIT)

using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

public class PdfMerger
{
    public void MergePdfs()
    {
        // Create output document
        using PdfDocument outputDocument = new PdfDocument();
        
        string[] inputFiles = { "report1.pdf", "report2.pdf", "report3.pdf" };
        
        foreach (string file in inputFiles)
        {
            // Open input document
            using PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);
            
            // Copy each page
            for (int pageIndex = 0; pageIndex < inputDocument.PageCount; pageIndex++)
            {
                PdfPage page = inputDocument.Pages[pageIndex];
                outputDocument.AddPage(page);
            }
        }
        
        // Save merged document
        outputDocument.Save("merged.pdf");
    }
}
```

**Performance characteristics:**

- **Fast merge**: direct page copying with minimal overhead per page
- **Memory efficient**: streams pages without loading entire documents
- **No re-rendering**: preserves original page content
- **Handles large files**: efficient for merging large PDFs

#### IronPDF — Single-Call Merge

```csharp
// Install-Package IronPdf

using IronPdf;

public class PdfMerger
{
    public void MergePdfs()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf1 = PdfDocument.FromFile("report1.pdf");
        var pdf2 = PdfDocument.FromFile("report2.pdf");
        var pdf3 = PdfDocument.FromFile("report3.pdf");
        
        var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
        merged.SaveAs("merged.pdf");
    }
}
```

**Performance comparison:**
- PDFsharp: lower overhead, more verbose (~15 lines vs 5 lines)
- IronPDF: higher-level API, similar performance for merge operations

For advanced merge scenarios, see [PDF merge documentation](https://ironpdf.com/how-to/merge-or-split-pdfs/).

---

## API Mapping Reference

| PDFsharp Class/Method | IronPDF Equivalent |
|----------------------|-------------------|
| `new PdfDocument()` | `ChromePdfRenderer.RenderHtmlAsPdf()` (different approach) |
| `document.AddPage()` | HTML page-break CSS |
| `XGraphics.FromPdfPage()` | Not applicable—HTML rendering |
| `gfx.DrawString()` | HTML text elements |
| `gfx.DrawRectangle()` | CSS backgrounds/borders |
| `gfx.DrawLine()` | CSS borders or `<hr>` |
| `gfx.DrawImage()` | HTML `<img>` tags |
| `XFont()` | CSS font properties |
| `PdfReader.Open()` | `PdfDocument.FromFile()` |
| `document.Pages[i]` | `pdf.CopyPage(i)` |
| `document.Save()` | `pdf.SaveAs()` |
| MigraDoc document model | HTML/CSS document structure |
| Page size constants | CSS `@page { size: }` |

---

## Comprehensive Feature Comparison

### Status & Licensing

| Feature | PDFsharp | IronPDF |
|---------|----------|---------|
| **License** | MIT (open-source) | Commercial |
| **Product Status** | Active maintenance (6.x line) | Active development |
| **Source Code** | Available (GitHub) | Binary only |
| **.NET Support** | .NET Framework 4.6.2+ and modern .NET — verify the current 6.x package's target list | .NET Framework 4.6.2+ and modern .NET — verify against current release notes |
| **Platform** | Windows / Linux / macOS (Core build needs a font resolver on Linux/macOS) | Windows / Linux / macOS |
| **Commercial Use** | Yes (MIT) | Requires license |

### Content Creation

| Feature | PDFsharp | IronPDF |
|---------|----------|---------|
| **HTML to PDF** | No (graphics API) | Yes (Chromium-based) |
| **CSS Support** | No (modern CSS) | CSS3 |
| **JavaScript** | No | Yes |
| **Coordinate Drawing** | Yes (core feature) | Not the primary model |
| **Text Drawing** | Manual positioning | Automatic layout |
| **Images** | Manual `XImage` placement | HTML `<img>` |
| **Tables** | Manual cell drawing (or MigraDoc) | HTML tables |
| **Fonts** | TrueType / Type1 / Standard 14 | System and web fonts |

### PDF Operations

| Feature | PDFsharp | IronPDF |
|---------|----------|---------|
| **Create PDFs** | Yes (low-level) | Yes (HTML-based) |
| **Read PDFs** | Yes | Yes |
| **Merge PDFs** | Yes (page copy) | Yes (`Merge()` method) |
| **Split PDFs** | Yes (page extraction) | Yes (`CopyPage()` / `CopyPages()`) |
| **Edit PDFs** | Limited (add content) | Stamps, annotations, form fields |
| **Rotate Pages** | Yes | Yes |
| **Extract Text** | Basic | Yes |
| **Extract Images** | Yes | Yes (verify against current docs) |

### Performance Profile

| Metric | PDFsharp | IronPDF |
|--------|----------|---------|
| **Startup Time** | Negligible | Adds Chromium initialization on first render |
| **Per-Doc Overhead** | Low | Moderate (HTML rendering pipeline) |
| **Memory Footprint** | Low | Moderate (Chromium baseline) |
| **Batch Scaling** | Linear | Amortized with renderer reuse |
| **Development Time** | High (layout code) | Lower (HTML/CSS) |
| **Maintenance Cost** | High (coordinate changes) | Lower (template edits) |

---

## When Teams Consider PDFsharp Migration

**Coordinate math burnout** is the most common migration trigger. Teams start with simple single-page documents (logo + text), and PDFsharp handles it fine. Requirements expand: multi-column layouts, dynamic tables, responsive page breaks. Suddenly you're maintaining a positioning engine. By the time the tenth "just move the footer down five points" ticket lands, developers typically start evaluating alternatives.

**HTML skills reuse** drives migration for web-first teams. If your team knows CSS Grid, Flexbox, and responsive design, rewriting those layouts in coordinate-based code feels backwards. PDFsharp's model is PDF coordinate systems; an HTML-rendering tool lets you apply existing web knowledge so your invoices can share patterns with your email templates.

**Development velocity** matters when shipping features fast. PDFsharp's per-document performance is excellent, but development time can dominate. Building a complex invoice template in coordinate code is meaningfully slower than its HTML/CSS equivalent. For most teams, the development-time savings dwarf per-document execution-time differences.

**Template reusability** becomes critical at scale. Marketing teams typically want PDFs that match the website, and designers edit HTML/CSS rather than C#. With PDFsharp, developers rebuild designs by hand; with HTML-based tools, designers own templates directly.

**Modern layout requirements** (CSS Grid, Flexbox, SVG) sit outside PDFsharp's scope by design. The library is a graphics API, not a CSS layout engine. Implementing a CSS Grid equivalent in coordinate math is possible but unreasonable; teams building modern document designs typically reach for a different abstraction.

---

## Installation Comparison

### PDFsharp

```bash
# Core build (cross-platform; typically needs a custom font resolver on Linux/macOS)
dotnet add package PDFsharp

# GDI+ build (Windows, includes GDI+ imaging)
dotnet add package PDFsharp-gdi

# WPF build (Windows, includes WPF imaging)
dotnet add package PDFsharp-wpf

# MigraDoc (companion flow-document model, optional)
dotnet add package PDFsharp-MigraDoc

# Community .NET Standard port (MIT, separate project)
# dotnet add package PdfSharpCore
```

```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel; // If using MigraDoc
```

### IronPDF

```bash
dotnet add package IronPdf
```

```csharp
using IronPdf;
```

---

## Conclusion

PDFsharp delivers on its promise: explicit control over PDF construction at the coordinate level. For teams building data visualizations, CAD output, or geometrically precise documents, this control is essential. The MIT license and active maintenance on the 6.x line make it a sustainable open-source choice. If your documents are programmatically generated graphics (charts, diagrams, technical drawings), PDFsharp's API is designed for you — and MigraDoc is there when you need flow content on top of it.

Migration becomes worth considering when: (1) coordinate-based positioning has become a maintenance burden that outweighs any per-document performance gains, (2) your team's core competency is web development and rewriting HTML layouts in coordinates feels wasteful, (3) template-driven document generation (invoices, reports, letters) dominates your workload, or (4) design iteration speed matters more than raw execution speed. The question isn't whether PDFsharp is fast — it is — but whether development velocity and maintenance costs matter more in your context.

IronPDF trades per-document performance for development velocity: HTML/CSS eliminates coordinate math, Chromium handles modern layout, and templates enable designer collaboration. The performance profile shifts from "fastest per-document" to "fastest to market." For teams where developer hours cost more than server milliseconds, this is often the right trade.

**What's your performance bottleneck — per-document execution time (favor PDFsharp), development/maintenance time (favor HTML-based), or something else entirely?**

*For HTML template patterns and performance optimization, see [IronPDF HTML rendering guide](https://ironsoftware.com/suite/blog/using-ironsuite/html-to-pdf-ironpdf-tutorial/). For PDF operations performance, review the [PDF SDK documentation](https://ironsoftware.com/suite/blog/using-ironsuite/pdf-sdk-guide/).*
