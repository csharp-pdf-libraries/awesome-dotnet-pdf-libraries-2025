---
title: "PdfPig vs IronPDF: a .NET developer honest take"
published: false
tags: dotnet, csharp, pdf, comparison
---

*Canonical/source version on Iron Software blog: https://ironsoftware.com/suite/blog*

## When your PDF library solves the wrong problem

Picture a `PdfDocumentBuilder` call list that started as a one-page receipt and is now four hundred lines of coordinate math, line-spacing constants, and manual page-break logic. The code compiles, the file opens, the layout almost lines up — and then someone asks for a styled invoice table with a logo header. That moment is when many teams discover PdfPig is a parser-first library with a deliberately narrow writer surface, not an HTML-style generation toolkit.

PdfPig is an Apache-2.0 open-source library (with roots tracing back to Apache PDFBox) built specifically for reading existing PDFs and extracting their contents. It excels at pulling text with positional data, analyzing document layouts, and exposing the internal PDF structure. The library operates at a low level — you work with `Letter` objects, bounding boxes, and PDF operators directly. It also ships a `PdfMerger` for concatenating documents and a `PdfDocumentBuilder` for coordinate-based authoring. For document analysis, data mining, and content extraction, PdfPig is excellent. For high-level layout-driven generation, it provides primitives (text drawing, path operations, merge) but no layout engine, no HTML rendering, and no abstraction over coordinate math.

## Understanding IronPDF

IronPDF approaches PDF generation from the opposite direction: it assumes you have web content (HTML/CSS/JavaScript) and want a PDF that looks exactly like it would in a browser. The library embeds a Chromium rendering engine, so your templates—whether they're Bootstrap dashboards, Tailwind invoices, or vanilla HTML—render pixel-perfect without manual positioning. You write HTML, call `RenderHtmlAsPdf()`, and get a PDF. No coordinate calculations, no font metrics, no page break logic.

The core architecture is straightforward: `ChromePdfRenderer` manages the Chromium process, `PdfDocument` represents the output, and rendering options let you configure paper size, margins, headers, and footers. For reading existing PDFs, IronPDF uses `PdfDocument.FromFile()` and exposes text extraction, page manipulation, and metadata editing. Installation is a single NuGet package with no external service dependencies—Chromium is bundled and lifecycle-managed internally.

## Key Limitations of PdfPig

### **Product Status**

PdfPig is pre-1.0 (current 0.1.x line, e.g. v0.1.14), and the README explicitly states "while the version is below 1.0.0 minor versions will change the public API without warning (SemVer will not be followed until 1.0.0 is reached)." Active maintenance continues, but teams building long-lived systems on a pre-1.0 API typically pin to a specific version and budget for upgrade testing.

### **Missing Capabilities**

No HTML-to-PDF conversion — the project wiki lists "Converting HTML or other formats to PDF" under "Things you can't do." No CSS rendering, no JavaScript execution, no web font support. New-document creation is limited to drawing text and geometric shapes at explicit coordinates via `PdfDocumentBuilder`. No built-in table layouts, no automatic text wrapping, no multi-column support out of the box. Per the project wiki, "forms are readonly and values cannot be changed or added using PdfPig." There is no watermarking API, no digital-signature authoring, and no PDF/A compliance pipeline. PdfPig does ship a `PdfMerger` for concatenating documents, but high-level split / rotate / edit is left to the caller. The library is designed for reading and parsing, with a narrow writer surface.

### **Technical Considerations**

Coordinate-based positioning means calculating X/Y for every text element manually when using `PdfDocumentBuilder`. No layout engine to handle word wrapping, justification, or pagination — those are caller responsibilities. Multi-page documents require manual page-break logic and height tracking. Font handling exposes the TrueType parser directly. Text extraction offers multiple approaches (`page.Text` vs `ContentOrderTextExtractor` vs `NearestNeighbourWordExtractor`) because reading-order text is non-trivial in PDFs. Table detection typically pairs with external libraries such as Tabula Sharp.

### **Support Model**

Community-driven open-source project, maintained primarily by Eliot Jones, with GitHub Issues and Stack Overflow as the main support channels. No commercial support contract or SLA. Documentation centers on the GitHub README, wiki, and API reference; comprehensive guides for advanced scenarios are limited.

### **Architecture Shape**

The parser-first architecture shows in the API: `PdfDocument.Open()` loads existing files, while `PdfDocumentBuilder` and `PdfMerger` cover the (narrow) write surface. There is no template system, no data binding, and no rendering pipeline. For dynamic documents (reports, invoices, dashboards), the rendering layer is the caller's responsibility — PdfPig provides PDF primitives, and the application supplies the layout logic on top.

---

## Feature Comparison Overview

| Aspect | PdfPig | IronPDF |
|--------|--------|---------|
| **Current Status** | Active (pre-1.0, API may change in minor versions) | Active (stable releases) |
| **HTML Support** | Not supported (use external tools) | Chromium-based engine |
| **Creation Model** | Coordinate-based via `PdfDocumentBuilder` | HTML/CSS rendering |
| **Installation** | Single NuGet package | Single NuGet package |
| **Support** | Community (GitHub, Stack Overflow) | Commercial available |
| **License** | Apache 2.0 | Commercial (with trial) |

---

## Code Comparisons

### Extracting Text from Existing PDFs

#### PdfPig — Text Extraction

```csharp
// Install-Package PdfPig

using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

public class PdfTextExtractor
{
    public void ExtractText()
    {
        using (PdfDocument document = PdfDocument.Open(@"C:\reports\invoice.pdf"))
        {
            foreach (Page page in document.GetPages())
            {
                // page.Text returns the raw stream order; for reading
                // order use ContentOrderTextExtractor.
                // string rawText = page.Text;

                string text = ContentOrderTextExtractor.GetText(page);
                Console.WriteLine($"Page {page.Number}:");
                Console.WriteLine(text);
                
                // Alternative: Access letters for positional data
                IReadOnlyList<Letter> letters = page.Letters;
                Console.WriteLine($"Found {letters.Count} letters");
                
                // Get words with position information
                IEnumerable<Word> words = page.GetWords();
                foreach (var word in words)
                {
                    Console.WriteLine($"'{word.Text}' at ({word.BoundingBox.Left}, {word.BoundingBox.Bottom})");
                }
            }
        }
    }
}
```

**Architectural notes:**

- **Multiple extraction strategies**: Raw `page.Text` returns stream order; reading-order text typically uses `ContentOrderTextExtractor`
- **Positional API**: Structured text access goes through bounding boxes and layout analysis primitives
- **No semantic structure**: The API surfaces letters and words, not headers, paragraphs, or tables — semantic structure is the caller's responsibility
- **Explicit iteration**: Pages are iterated by the caller; no single "extract everything" helper
- **Layout analysis options**: Complex documents may require choosing between several word-extraction algorithms (`DefaultWordExtractor`, `NearestNeighbourWordExtractor`)
- **Learning curve**: `Letter` / `Word` / `BoundingBox` concepts are part of the public model

#### IronPDF — Text Extraction

```csharp
// Install-Package IronPdf

using System;
using IronPdf;

public class PdfTextExtractor
{
    public void ExtractText()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile(@"C:\reports\invoice.pdf");

        // Extract all text (automatically handles reading order)
        string allText = pdf.ExtractAllText();
        Console.WriteLine(allText);

        // Extract text from specific pages (0-based indexing)
        string firstPageText = pdf.ExtractTextFromPage(0);
        Console.WriteLine($"Page 1: {firstPageText}");
    }
}
```

PdfPig's strength is exposing positional data for advanced analysis. For straightforward text extraction from PDFs, see [IronPDF's text extraction capabilities](https://ironsoftware.com/suite/blog/using-ironsuite/csharp-create-pdf-ironpdf-tutorial/).

---

### Creating Basic PDF Documents

#### PdfPig — Basic PDF Creation

```csharp
// Install-Package PdfPig

using System;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

public class PdfCreator
{
    public void CreateSimpleDocument()
    {
        var builder = new PdfDocumentBuilder();
        
        // Register font (required before use)
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        
        // Add page with explicit size
        PdfPageBuilder page = builder.AddPage(PageSize.A4);
        
        // Calculate positions manually (A4 width = 595 points, height = 842 points)
        double leftMargin = 50;
        double topMargin = 792; // 842 - 50
        
        // Draw text at specific coordinates
        page.AddText("Invoice #2026-001", 24, new PdfPoint(leftMargin, topMargin), font);
        
        // Calculate next line position (manual line spacing)
        double lineHeight = 30;
        double currentY = topMargin - lineHeight;
        
        page.AddText("Date: February 11, 2026", 12, new PdfPoint(leftMargin, currentY), font);
        currentY -= 20;
        
        page.AddText("Customer: Acme Corp", 12, new PdfPoint(leftMargin, currentY), font);
        currentY -= 40;
        
        page.AddText("Item: Consulting Services", 12, new PdfPoint(leftMargin, currentY), font);
        currentY -= 20;
        
        page.AddText("Amount: $5,000.00", 12, new PdfPoint(leftMargin, currentY), font);
        
        // Save document
        byte[] documentBytes = builder.Build();
        System.IO.File.WriteAllBytes(@"C:\output\invoice.pdf", documentBytes);
    }
}
```

**Architectural notes:**

- **Manual coordinate calculation**: Every text element requires explicit X/Y positioning
- **No layout engine**: Line spacing, wrapping, and alignment are caller-implemented
- **Font registration**: Fonts (including Standard 14) are explicitly added to the builder before use
- **No styling abstractions**: Colors and sizes are per-element; there is no CSS-like style cascade
- **No table primitive**: Tables are built from text and lines at calculated cell positions
- **Units in PDF points**: Coordinates use PDF points (1/72 inch); unit helpers are not in-box

#### IronPDF — HTML-Based Creation

```csharp
// Install-Package IronPdf

using IronPdf;

public class PdfCreator
{
    public void CreateSimpleDocument()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var htmlContent = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; margin: 50px; }
                    h1 { font-size: 24px; margin-bottom: 10px; }
                    .invoice-info { margin: 20px 0; }
                    table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                    th, td { padding: 10px; border: 1px solid #ccc; text-align: left; }
                    th { background-color: #f5f5f5; }
                </style>
            </head>
            <body>
                <h1>Invoice #2026-001</h1>
                <div class='invoice-info'>
                    <p>Date: February 11, 2026</p>
                    <p>Customer: Acme Corp</p>
                </div>
                <table>
                    <tr><th>Item</th><th>Amount</th></tr>
                    <tr><td>Consulting Services</td><td>$5,000.00</td></tr>
                </table>
            </body>
            </html>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs(@"C:\output\invoice.pdf");
    }
}
```

For teams with existing HTML templates or web development skills, IronPDF eliminates coordinate math. See [HTML-to-PDF rendering options](https://ironsoftware.com/suite/blog/using-ironsuite/html-to-pdf-ironpdf-tutorial/) for advanced layouts.

---

### Analyzing PDF Document Structure

#### PdfPig — Document Analysis

```csharp
// Install-Package PdfPig

using System;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

public class PdfAnalyzer
{
    public void AnalyzeDocument()
    {
        using (PdfDocument document = PdfDocument.Open(@"C:\docs\report.pdf"))
        {
            Console.WriteLine($"Pages: {document.NumberOfPages}");
            Console.WriteLine($"Version: {document.Version}");
            
            foreach (Page page in document.GetPages())
            {
                // Get page dimensions
                var size = page.Size;
                Console.WriteLine($"Page {page.Number}: {size.Width} x {size.Height} points");
                
                // Extract images
                var images = page.GetImages();
                Console.WriteLine($"Images: {images.Count()}");
                
                // Get hyperlinks
                var links = page.GetHyperlinks();
                foreach (var link in links)
                {
                    Console.WriteLine($"Link: {link.Uri}");
                }
                
                // Access metadata
                var info = document.Information;
                Console.WriteLine($"Title: {info.Title}");
                Console.WriteLine($"Author: {info.Author}");
                Console.WriteLine($"Created: {info.CreationDate}");
                
                // Access raw PDF structure (advanced)
                var structure = document.Structure;
                var catalog = structure.Catalog;
            }
        }
    }
}
```

**Architectural notes:**

- **Read-mostly analysis**: Inspection of existing documents is the strength; in-place editing is not the focus
- **Metadata is read-only**: The `Information` dictionary is exposed for reading
- **No reporting layer**: Analysis results are formatted by the caller
- **Low-level access**: Advanced analysis maps directly onto PDF internals
- **No content rewriting**: Editing text or restructuring pages of an existing PDF is not in the read API

#### IronPDF — Document Inspection

```csharp
// Install-Package IronPdf

using System;
using IronPdf;

public class PdfAnalyzer
{
    public void AnalyzeDocument()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile(@"C:\docs\report.pdf");

        // Basic properties
        Console.WriteLine($"Pages: {pdf.PageCount}");
        Console.WriteLine($"Title: {pdf.MetaData.Title}");
        Console.WriteLine($"Author: {pdf.MetaData.Author}");
        
        // Extract all text for content analysis
        string fullText = pdf.ExtractAllText();
        int wordCount = fullText.Split(' ').Length;
        Console.WriteLine($"Approximate words: {wordCount}");
        
        // Access per-page text
        for (int i = 0; i < pdf.PageCount; i++)
        {
            string pageText = pdf.ExtractTextFromPage(i);
            Console.WriteLine($"Page {i + 1} length: {pageText.Length} chars");
        }
    }
}
```

PdfPig excels at granular document inspection—useful for PDF forensics, data extraction pipelines, and layout analysis. IronPDF focuses on generation workflows with basic reading capabilities.

---

## API Mapping Reference

| PdfPig Class/Method | IronPDF Equivalent |
|---------------------|-------------------|
| `PdfDocument.Open(path)` | `PdfDocument.FromFile(path)` |
| `page.Text` / `ContentOrderTextExtractor` | `pdf.ExtractAllText()` |
| `page.Letters` | No direct equivalent—text-level analysis not exposed |
| `page.GetWords()` | No direct equivalent—use text extraction |
| `page.GetImages()` | Image extraction via `PdfDocument` APIs |
| `page.GetHyperlinks()` | No direct equivalent — text-level extraction |
| `document.Information` | `pdf.MetaData.*` properties |
| `PdfDocumentBuilder` | `ChromePdfRenderer.RenderHtmlAsPdf()` (different paradigm) |
| `builder.AddPage()` | HTML `<div style='page-break-after: always;'>` |
| `page.AddText()` | HTML/CSS text rendering |
| `builder.AddStandard14Font()` | Not needed — CSS handles fonts |
| `PdfMerger.Merge(...)` | `PdfDocument.Merge(...)` |
| Font parsing | Not exposed — handled by the Chromium engine |
| Table extraction | Not a core IronPDF feature (PdfPig pairs with Tabula Sharp here) |

---

## Comprehensive Feature Comparison

### Status & Support

| Feature | PdfPig | IronPDF |
|---------|--------|---------|
| **Product Status** | Active (pre-1.0) | Active (stable) |
| **API Stability** | Pre-1.0 — minor versions may break the public API per the README | Stable |
| **Official Support** | Community only | Commercial available |
| **Documentation** | GitHub README + API ref | Full docs + tutorials |
| **Update Frequency** | Regular commits | Regular releases |
| **License Model** | Apache 2.0 (open source) | Commercial with trial |

### Content Creation

| Feature | PdfPig | IronPDF |
|---------|--------|---------|
| **HTML to PDF** | No | Yes (Chromium engine) |
| **CSS Support** | No | CSS3 full support |
| **JavaScript Execution** | No | Yes |
| **Table Layouts** | Manual coordinate math | Automatic HTML tables |
| **Multi-column** | Manual | CSS columns |
| **Images** | Manual positioning | HTML `<img>` tags |
| **Fonts** | Manual TrueType loading | Automatic (@font-face) |
| **Templates** | No built-in | HTML/CSS templates |

### PDF Operations

| Feature | PdfPig | IronPDF |
|---------|--------|---------|
| **Read PDFs** | Yes (core feature) | Yes |
| **Create PDFs** | Basic (text/paths via `PdfDocumentBuilder`) | Full (HTML rendering) |
| **Merge PDFs** | Yes (`PdfMerger.Merge`) | `PdfDocument.Merge()` |
| **Split PDFs** | Manual (no high-level API) | `CopyPages()` |
| **Edit Existing** | Minimal | Yes |
| **Text Extraction** | Yes (detailed, positional) | Yes (text-only) |
| **Image Extraction** | Yes (`page.GetImages()`) | Yes |
| **Metadata** | Read-only | Read/write |

### Analysis & Inspection

| Feature | PdfPig | IronPDF |
|---------|--------|---------|
| **Letter-level Access** | Yes | No |
| **Bounding Boxes** | Yes | No |
| **Layout Analysis** | Yes (multiple algorithms) | No |
| **Document Structure** | Low-level access | High-level only |
| **Form Field Reading** | Yes (read-only; cannot fill) | Yes (read + fill + flatten) |
| **Hyperlink Extraction** | Yes (`page.GetHyperlinks()`) | Limited |

### Development

| Feature | PdfPig | IronPDF |
|---------|--------|---------|
| **Primary Use Case** | Reading/analyzing PDFs | Creating PDFs from HTML |
| **Learning Curve** | Steep (PDF internals) | Moderate (web skills) |
| **Code Verbosity** | High (coordinate math) | Low (HTML/CSS) |
| **Installation** | Single package | Single package |
| **.NET Support** | .NET Standard 2.0+ | .NET 6+ and Framework 4.6.2+ |
| **Cross-Platform** | Yes | Yes |

---

## Commonly reported friction points

Teams stretching PdfPig toward layout-heavy generation (rather than its parsing focus) commonly mention:

- **Coordinate calculation**: Manual X/Y placement is sensitive to dynamic content lengths
- **Font handling**: Embedded vs. non-embedded font choices can produce viewer-to-viewer differences
- **Multi-page layout**: Page-break logic and height tracking are caller responsibilities and tend to grow with the document
- **No table primitive**: Invoice tables and data grids are typically composed manually or with third-party helpers
- **Pre-1.0 API contract**: Public API may change in minor versions (documented behavior — see the README note on SemVer)

---

## When Teams Consider PdfPig Migration

**Use-case mismatch** drives most migration discussions. Teams sometimes adopt PdfPig expecting a general-purpose PDF generation library and later realize it is parsing-first by design. The wiki explicitly lists HTML-to-PDF under "Things you can't do." When requirements grow to include styled invoices, branded reports, or template-based documents, the work moves outside the library's design intent.

**Coordinate-math maintenance** grows with the document. A "simple" invoice picks up a line-items table, tax calculations, terms and conditions, and a logo. Each change requires recalculating positions downstream. Two-column layouts or justified text typically mean re-implementing layout logic that browsers already provide.

**No HTML rendering** means web templates aren't reusable. Email templates, landing pages, and dashboards are usually already HTML/CSS. With a coordinate-based primitive set, those designs are rebuilt rather than reused — and the two surfaces (web UI and PDF) tend to drift over time.

**Pre-1.0 API contract** changes the upgrade calculus. The project documents that minor versions can change the public API. For long-lived applications, this typically means pinning to a known-good version and reviewing each upgrade.

**Modern document requirements** (accessibility tagging, PDF/A compliance, digital-signature authoring, form filling) tend to push teams toward libraries that target generation directly. PdfPig's roadmap focuses on reading and analysis, so those generation capabilities are out of scope.

---

## Installation Comparison

### PdfPig

```bash
# Single package
dotnet add package PdfPig
```

```csharp
// Minimal imports for text extraction
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

// For document creation (basic features)
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Fonts.Standard14Fonts;

// For advanced layout analysis
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
```

### IronPDF

```bash
# Single package
dotnet add package IronPdf
```

```csharp
// Minimal imports
using IronPdf;
using IronPdf.Rendering; // Optional for rendering config
```

---

## Conclusion

PdfPig fills a specific niche: reading PDF documents and extracting structured data. For teams doing document analysis, text mining, or content extraction, PdfPig's low-level access to PDF internals is exactly what you need. The Apache 2.0 license and active development make it a solid choice for parsing workflows. If your pipeline involves ingesting PDFs and pulling out data for processing, PdfPig does this well.

Migration becomes mandatory when: (1) your actual need is PDF generation from web content, not parsing existing documents, (2) you're spending more time implementing layout engines than solving business problems, (3) maintaining coordinate-based positioning code has become a bottleneck for document updates, or (4) you need features PdfPig explicitly doesn't provide (HTML rendering, templates, advanced formatting). The decision isn't about PdfPig being inadequate—it's about matching tool capabilities to actual requirements.

IronPDF addresses the generation use case mechanically: Chromium engine handles modern HTML/CSS/JavaScript, eliminating coordinate calculations and layout logic. The bundled rendering engine means no external dependencies or service deployments. For teams already using HTML templates for web UIs, reusing those templates for PDF generation avoids the dual-maintenance burden of coordinate-based alternatives.

**Are you using PdfPig for text extraction (its strength) or trying to generate complex PDFs with it (against the grain)? What's blocking your migration—API learning curve, existing coordinate math, or uncertainty about template conversion?**

*For HTML-based PDF generation workflows, see the [IronPDF rendering guide](https://ironsoftware.com/suite/blog/using-ironsuite/html-to-pdf-ironpdf-tutorial/). For text extraction and analysis comparisons, review the [PDF processing documentation](https://ironsoftware.com/suite/blog/using-ironsuite/csharp-create-pdf-ironpdf-tutorial/).*
