---
title: "iTextSharp vs IronPDF: the decision guide for .NET teams"
published: false
tags: dotnet, csharp, pdf, comparison
---

## Scope note

This article compares the **legacy iTextSharp 5.x line** (the .NET port that has been in security-fix-only mode for years) against IronPDF. iText Group's current product is iText 9 (package name `itext`, with `itext7` retained as a deprecated alias), which has a separate codebase, different APIs, and the same dual AGPL/commercial licensing model. If you are evaluating the current iText 9 line rather than legacy iTextSharp, most of the API-level points here still apply in spirit, but the package names, namespaces, and class APIs differ.

## Critical status context

iTextSharp 5.x has reached end-of-life and transitioned to maintenance mode, [per the official iText documentation](https://itextpdf.com/products/itextsharp). The GitHub repository is marked `[DEPRECATED]` with the note "only security fixes will be added." Recent point releases on the 5.x line have backported security patches and BouncyCastle updates rather than adding features. Teams using iTextSharp today are operating on legacy code that receives security fixes only. iText 7 (the successor product, now at version 9) is a different library with breaking API changes and dual AGPL/commercial licensing. iText Group is now part of Apryse following a 2025 corporate combination.

iTextSharp was built for low-level PDF construction using coordinate-based positioning—you specify exact X/Y coordinates for text, images, and shapes. This gives precise control but requires PDF spec knowledge and produces verbose code. HTML rendering requires the separate XMLWorker package, which parses a defined subset of HTML and CSS but does not target modern standards such as Flexbox, Grid, or full CSS3. Common friction points reported by teams include thread safety considerations on `PdfWriter`, 1-based page indexing (versus C#'s 0-based collections), and fragmented community documentation across Stack Overflow now that official support is reserved for iText 7+.

## Understanding IronPDF

IronPDF takes a web-first approach: it uses a Chromium rendering engine to convert HTML/CSS/JavaScript directly to PDF, treating PDF generation as "print to PDF from a browser." This means your HTML templates render pixel-perfect without manual coordinate calculations. The library handles document structure, pagination, and resource loading automatically. Installation is a single NuGet package (`IronPdf`) with no external dependencies—Chromium is bundled and managed internally.

The core workflow is: create a `ChromePdfRenderer`, call `RenderHtmlAsPdf()` with your HTML string/file/URL, and save the resulting `PdfDocument`. For editing existing PDFs, use static methods like `PdfDocument.FromFile()` and `PdfDocument.Merge()`. All operations use 0-based page indexing (standard C# convention) and throw exceptions on errors (no status code checking). The API is designed to minimize boilerplate for common tasks while exposing advanced options through rendering configurations.

## Key Limitations of iTextSharp

### **Product Status**

iTextSharp 5.x is officially deprecated and receives security patches only—no bug fixes, no feature development. The [GitHub repository](https://github.com/itext/itextsharp) directs new users to iText 7+, which is a separate product with incompatible APIs and the same dual AGPL/commercial license model. Teams on iTextSharp 5.x have no in-place upgrade route that preserves existing code. For production systems targeting current .NET versions in 2026, building on a library line that has been in feature-freeze for years tends to accumulate technical debt over time.

### **Missing Capabilities**

Native HTML rendering requires the separate `itextsharp.xmlworker` package, which targets a subset of HTML 4 / CSS 2.1 rather than modern web standards. Layouts that depend on Flexbox, CSS Grid, viewport units, or JavaScript execution typically need to be reconstructed against iTextSharp's programmatic API. No built-in async methods for I/O. No URL-to-PDF without a separate HTTP client step. Form field manipulation exists but is coordinate-based. Digital signatures are supported but require manual certificate and timestamp-server wiring.

### **Technical Issues**

Concurrency on `PdfWriter` and `PdfStamper` requires manual synchronization—they are not designed to be shared across threads, which can lead to corrupted output in concurrent environments. 1-based page indexing differs from C# conventions (0-based arrays/lists), which is a common source of off-by-one bugs. Error reporting is mixed across the API surface. Memory growth has been reported when readers and documents are not disposed deterministically (see the [iTextSharp Stack Overflow tag](https://stackoverflow.com/questions/tagged/itextsharp) for community discussion). Unit conversions are point-based (1/72 inch) with no built-in helpers for common units.

### **Support Status**

No vendor support for iTextSharp 5.x—commercial support applies only to current iText 7/8/9. Community answers on Stack Overflow often reference the deprecated API. Official documentation now centers on the iText 7+ line, so legacy 5.x docs and tutorials are gradually aging out. There is no public roadmap for additional 5.x security patches beyond what iText Group chooses to backport. Teams on long-term support contracts for legacy iTextSharp typically need to migrate or negotiate custom OEM terms.

### **Architecture Constraints**

Coordinate-based layout requires calculating pixel positions in code, which makes dynamic content harder to maintain—changing a font size or adding a paragraph can ripple downstream. HTML/CSS-driven design tends to be translated by hand into procedural PDF construction. There is no built-in template engine; teams typically integrate a third-party engine such as Razor. Optional features ship as separate packages (`itextsharp.pdfa`, `itextsharp.xfaworker`, `itextsharp.xtra`), so the dependency tree grows feature by feature.

---

## Feature Comparison Overview

| Aspect | iTextSharp 5.x | IronPDF |
|--------|---------------|---------|
| **Current Status** | Deprecated; security patches only | Active development |
| **HTML Support** | XMLWorker (HTML 4 / CSS 2.1 subset) | Chromium engine (CSS3 + JS) |
| **Rendering Model** | Coordinate-based primitives | Browser-engine layout |
| **Installation** | Multiple NuGet packages | Single NuGet package |
| **Vendor Support** | Not available for 5.x | Commercial support available |
| **Roadmap** | None published for 5.x | Regular releases |

---

## Code Comparisons

### HTML to PDF Conversion

#### iTextSharp — HTML to PDF

```csharp
// Install packages:
// Install-Package iTextSharp
// Install-Package itextsharp.xmlworker

using System;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

public class HtmlToPdfConverter
{
    public void ConvertHtmlToPdf()
    {
        var htmlContent = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; margin: 20px; }
                    h1 { color: #2c3e50; font-size: 24px; }
                    .invoice-table { width: 100%; border-collapse: collapse; }
                    .invoice-table th { background-color: #3498db; color: white; }
                </style>
            </head>
            <body>
                <h1>Invoice #2026-001</h1>
                <table class='invoice-table'>
                    <tr><th>Item</th><th>Amount</th></tr>
                    <tr><td>Consulting</td><td>$5,000</td></tr>
                </table>
            </body>
            </html>";
        
        using (var stream = new FileStream("invoice.pdf", FileMode.Create))
        {
            // Create document with explicit page size
            var document = new Document(PageSize.A4, 50, 50, 25, 25);
            
            // Initialize writer
            var writer = PdfWriter.GetInstance(document, stream);
            document.Open();
            
            try
            {
                // Parse HTML through XMLWorker
                using (var htmlStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent)))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, htmlStream);
                }
            }
            finally
            {
                document.Close();
                writer.Close();
            }
        }
    }
}
```

**Architectural characteristics:**

- **Separate package**: HTML parsing lives in the `itextsharp.xmlworker` add-on, not the core
- **CSS subset**: XMLWorker targets HTML 4 / CSS 2.1; Flexbox, Grid, and most CSS3 features are out of scope
- **Manual resource management**: `document.Open()`, `document.Close()`, and writer disposal are explicit
- **No JavaScript execution**: Client-side scripts do not run during conversion
- **Try/finally for cleanup**: Exception-safe cleanup is the caller's responsibility
- **Setup boilerplate**: Several lines of document/writer wiring precede the actual parse call

#### IronPDF — HTML to PDF

```csharp
// Install package:
// Install-Package IronPdf

using IronPdf;

public class HtmlToPdfConverter
{
    public void ConvertHtmlToPdf()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var htmlContent = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; margin: 20px; }
                    h1 { color: #2c3e50; font-size: 24px; }
                    .invoice-table { width: 100%; border-collapse: collapse; }
                    .invoice-table th { background-color: #3498db; color: white; }
                </style>
            </head>
            <body>
                <h1>Invoice #2026-001</h1>
                <table class='invoice-table'>
                    <tr><th>Item</th><th>Amount</th></tr>
                    <tr><td>Consulting</td><td>$5,000</td></tr>
                </table>
            </body>
            </html>";

        // Render with CSS3 and JavaScript support via Chromium
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("invoice.pdf");
    }
}
```

IronPDF's Chrome rendering engine handles modern CSS automatically. For advanced rendering options like custom headers, footers, and paper sizes, see the [HTML to PDF documentation](https://ironsoftware.com/suite/blog/using-ironsuite/html-to-pdf-ironpdf-tutorial/).

---

### PDF Merging Operations

#### iTextSharp — Merge PDFs

```csharp
// Install-Package iTextSharp

using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

public class PdfMerger
{
    public void MergePdfs()
    {
        var outputFile = "merged.pdf";
        var inputFiles = new[] { "report1.pdf", "report2.pdf", "report3.pdf" };
        
        using (var stream = new FileStream(outputFile, FileMode.Create))
        {
            var document = new Document();
            var copy = new PdfCopy(document, stream);
            document.Open();
            
            try
            {
                foreach (var inputFile in inputFiles)
                {
                    var reader = new PdfReader(inputFile);

                    try
                    {
                        // Pages are 1-indexed in iTextSharp
                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            var importedPage = copy.GetImportedPage(reader, i);
                            copy.AddPage(importedPage);
                        }

                        copy.FreeReader(reader);
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            finally
            {
                document.Close();
            }
        }
    }
}
```

**Architectural characteristics:**

- **Loop-based merge**: No single-call merge helper—the caller iterates files and pages
- **1-based page indexing**: `i = 1; i <= NumberOfPages` versus C#'s 0-based collections
- **Explicit resource management**: Multiple nested `using`/`try`/`finally` blocks
- **Reader instances per file**: Each input is opened and held during its merge step
- **Sequential processing**: No built-in batch helper for many inputs
- **Layered error handling**: Exceptions can surface at the file, page, or document level

#### IronPDF — Merge PDFs

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

        // Single merge call
        var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
        merged.SaveAs("merged.pdf");
    }
}
```

For splitting PDFs and advanced merge scenarios (custom page selection, preserving metadata), see the [merge and split documentation](https://ironpdf.com/how-to/merge-or-split-pdfs/).

---

### Adding Watermarks

#### iTextSharp — Apply Watermark

```csharp
// Install-Package iTextSharp

using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

public class PdfWatermarker
{
    public void AddWatermark()
    {
        var inputFile = "original.pdf";
        var outputFile = "watermarked.pdf";
        
        using (var reader = new PdfReader(inputFile))
        using (var stream = new FileStream(outputFile, FileMode.Create))
        {
            var stamper = new PdfStamper(reader, stream);
            
            try
            {
                // Set up the font explicitly
                var baseFont = BaseFont.CreateFont(
                    BaseFont.HELVETICA_BOLD,
                    BaseFont.CP1252,
                    BaseFont.NOT_EMBEDDED
                );

                // Stamp each page (1-indexed)
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    var contentByte = stamper.GetOverContent(i);

                    // Compute center from the page size
                    var pageSize = reader.GetPageSizeWithRotation(i);
                    var centerX = pageSize.Width / 2;
                    var centerY = pageSize.Height / 2;

                    // Apply fill opacity via graphics state
                    var gstate = new PdfGState { FillOpacity = 0.3f };
                    contentByte.SetGState(gstate);

                    contentByte.BeginText();
                    contentByte.SetColorFill(BaseColor.RED);
                    contentByte.SetFontAndSize(baseFont, 48);
                    contentByte.ShowTextAligned(
                        Element.ALIGN_CENTER,
                        "CONFIDENTIAL",
                        centerX, centerY, 45 // x, y, rotation
                    );
                    contentByte.EndText();
                }
            }
            finally
            {
                stamper.Close();
                reader.Close();
            }
        }
    }
}
```

**Architectural characteristics:**

- **Manual positioning**: Center coordinates are computed from each page's size
- **Page-by-page loop**: No bulk watermark helper at this layer
- **Graphics-state opacity**: Transparency goes through `PdfGState` rather than a property
- **Text-primitive watermarks**: HTML/CSS styling is not part of this API
- **Explicit font setup**: `BaseFont` requires encoding and embedding choices up front
- **Coordinate-based rotation**: Rotation angles are separate from a higher-level positioning helper

#### IronPDF — Apply Watermark

```csharp
// Install-Package IronPdf

using IronPdf;
using IronPdf.Editing;

public class PdfWatermarker
{
    public void AddWatermark()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("original.pdf");

        // HTML watermark styled with CSS
        var watermarkHtml = @"
            <div style='
                color: red;
                font-size: 48px;
                font-weight: bold;
                opacity: 0.3;
                text-align: center;'>
                CONFIDENTIAL
            </div>";

        pdf.ApplyWatermark(watermarkHtml, 45, VerticalAlignment.Middle, HorizontalAlignment.Center);
        pdf.SaveAs("watermarked.pdf");
    }
}
```

IronPDF's watermarking accepts HTML with CSS styling, eliminating manual coordinate calculations. For image watermarks and advanced positioning, see the [watermarking guide](https://ironsoftware.com/customers/white-papers/stamp-watermark-hr/).

---

### Password Protection / Permissions

#### iTextSharp — Encrypt PDF

```csharp
// Install-Package iTextSharp

using System;
using System.IO;
using iTextSharp.text.pdf;

public class PdfSecurer
{
    public void ProtectPdf()
    {
        var inputFile = "original.pdf";
        var outputFile = "protected.pdf";
        var userPassword = "user123";
        var ownerPassword = "owner456";
        
        using (var reader = new PdfReader(inputFile))
        using (var stream = new FileStream(outputFile, FileMode.Create))
        {
            var stamper = new PdfStamper(reader, stream);
            
            try
            {
                // Permissions are combined as bitwise flags on PdfWriter
                stamper.SetEncryption(
                    PdfWriter.STRENGTH128BITS,
                    userPassword,
                    ownerPassword,
                    PdfWriter.ALLOW_PRINTING | PdfWriter.ALLOW_COPY
                    // Other flags include ALLOW_MODIFY_CONTENTS, ALLOW_MODIFY_ANNOTATIONS,
                    // ALLOW_FILL_IN, ALLOW_SCREENREADERS, ALLOW_ASSEMBLY, ALLOW_DEGRADED_PRINTING
                );
            }
            finally
            {
                stamper.Close();
                reader.Close();
            }
        }
    }
}
```

**Architectural characteristics:**

- **Bitwise permission flags**: Options are combined with `|` and identified by named constants
- **Two printing constants**: `ALLOW_PRINTING` and `ALLOW_DEGRADED_PRINTING` map to different PDF capabilities
- **Single-call API**: One `SetEncryption` method takes strength, passwords, and the flag mask together
- **Strength-dependent behavior**: Available flag combinations differ between 128-bit and 256-bit encryption
- **Integer constants vs typed enums**: Permissions are int constants rather than a typed enum

#### IronPDF — Encrypt PDF

```csharp
// Install-Package IronPdf

using IronPdf;

public class PdfSecurer
{
    public void ProtectPdf()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("original.pdf");

        // Passwords live under SecuritySettings
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";

        // Permission properties are typed (boolean / enum), not bitwise flags
        pdf.SecuritySettings.AllowUserCopyPasteContent = true;
        pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserFormData = false;
        pdf.SecuritySettings.AllowUserAnnotations = false;
        pdf.SecuritySettings.AllowUserAccessibilityExtractContent = true;

        pdf.SaveAs("protected.pdf");
    }
}
```

IronPDF uses explicit boolean properties and enums for permissions, avoiding bitwise flag confusion. For digital signatures and advanced security, see the [PDF security documentation](https://ironsoftware.com/suite/blog/using-ironsuite/html-to-pdf-ironpdf-tutorial/).

---

## API Mapping Reference

| iTextSharp Class/Method | IronPDF Equivalent |
|------------------------|-------------------|
| `Document` | `PdfDocument` |
| `PdfWriter.GetInstance()` | `ChromePdfRenderer()` |
| `PdfReader(filename)` | `PdfDocument.FromFile(filename)` |
| `PdfReader(stream)` | `PdfDocument.FromStream(stream)` |
| `HTMLWorker.ParseToList()` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `XMLWorkerHelper.ParseXHtml()` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `PdfCopy` + manual loop | `PdfDocument.Merge()` |
| `PdfStamper.GetOverContent()` | `PdfDocument.ApplyWatermark()` |
| `PdfStamper.SetEncryption()` | `PdfDocument.SecuritySettings.*` |
| `reader.NumberOfPages` | `pdf.PageCount` |
| `reader.GetPageN(1)` (1-based) | `pdf.CopyPage(0)` (0-based) |
| `BaseFont.CreateFont()` | Not needed—CSS handles fonts |
| `document.Open()` / `Close()` | Automatic—use `using` statement |
| Custom HTTP for URLs | `ChromePdfRenderer.RenderUrlAsPdf(url)` |
| Manual page size calculation | `ChromePdfRenderOptions.PaperSize` |

---

## Comprehensive Feature Comparison

### Status & Support

| Feature | iTextSharp 5.x | IronPDF |
|---------|---------------|---------|
| **Product Status** | Deprecated; security patches only | Active development |
| **New Features** | Frozen on 5.x line | Ongoing |
| **Vendor Support** | Not available for 5.x | Commercial available |
| **Community** | Stack Overflow archive | Active forums + docs |
| **Documentation Focus** | Centered on iText 7+ line | Current + maintained |
| **Public Roadmap (5.x)** | None published | Public roadmap |
| **Security Patches** | Backports on critical issues | Regular updates |
| **License Model** | AGPL or commercial | Commercial (trial available) |

### Content Creation

| Feature | iTextSharp 5.x | IronPDF |
|---------|---------------|---------|
| **HTML to PDF** | Limited (XMLWorker) | Full Chromium engine |
| **CSS Support** | CSS 2.1 (partial) | CSS3 (full) |
| **JavaScript Execution** | No | Yes |
| **Responsive Design** | No | Yes (media queries) |
| **Modern Layout** | No Flexbox/Grid | Full Flexbox/Grid |
| **Web Fonts** | Manual embedding | Automatic (@font-face) |
| **SVG Rendering** | Limited | Full SVG support |
| **URL to PDF** | Custom implementation | Built-in method |
| **Async Methods** | No | Yes (`RenderHtmlAsPdfAsync`) |

### PDF Operations

| Feature | iTextSharp 5.x | IronPDF |
|---------|---------------|---------|
| **Merge PDFs** | Manual iteration | `PdfDocument.Merge()` |
| **Split PDFs** | Manual page extraction | `CopyPages()` / `CopyPage()` |
| **Page Indexing** | 1-based | 0-based (C# standard) |
| **Rotate Pages** | `PdfDictionary` manipulation | `RotatePage(index, angle)` |
| **Extract Pages** | `PdfCopy` + loop | `CopyPages(start, end)` |
| **Delete Pages** | `SelectPages()` | `RemovePages(index)` |
| **Page Count** | `NumberOfPages` | `PageCount` property |
| **Text Extraction** | `PdfTextExtractor` | `ExtractAllText()` |

### Security & Encryption

| Feature | iTextSharp 5.x | IronPDF |
|---------|---------------|---------|
| **Password Protection** | Yes (bitwise flags) | Yes (explicit properties) |
| **Permissions** | Bitwise constants | Typed enums/booleans |
| **Encryption Strength** | 40/128-bit | 128/256-bit (AES) |
| **Digital Signatures** | Manual cert handling | Simplified API |
| **PDF/A Support** | Separate package required | Built-in |
| **Metadata Control** | Manual XMP | Property setters |

### Operational Characteristics

| Aspect | iTextSharp 5.x | IronPDF |
|------------|---------------|---------|
| **Threading model** | `PdfWriter`/`PdfStamper` not designed for shared concurrent use | Async API available |
| **Disposal discipline** | Memory growth reported without explicit close/dispose | Managed via standard `using` |
| **HTML rendering scope** | XMLWorker CSS 2.1 subset | Chromium browser engine |
| **Unicode** | May require explicit font embedding and encoding | Browser-managed via Chromium |
| **Large file handling** | Holds readers in memory during merge | Streaming-friendly APIs |

### Development Experience

| Feature | iTextSharp 5.x | IronPDF |
|---------|---------------|---------|
| **API Style** | Coordinate-based | HTML/CSS-based |
| **Learning Curve** | Steep (PDF spec knowledge) | Moderate (web skills) |
| **Code Verbosity** | High (20-40 lines typical) | Low (5-15 lines) |
| **Error Handling** | Mixed (codes + exceptions) | Consistent exceptions |
| **Installation** | 4-5 NuGet packages | Single package |
| **Dependencies** | BouncyCastle, XMLWorker | None (bundled Chromium) |
| **.NET Support** | .NET Framework 2.0-4.8 | .NET Framework 4.6.2+, .NET 6/7/8/9/10 |
| **Cross-Platform** | Windows only | Windows + Linux + Docker |

---

## Commonly Reported Friction Points

These patterns show up frequently in community threads and GitHub issues for iTextSharp 5.x. Behavior may vary by version and configuration—verify against your specific setup:

- **CSS table rendering**: XMLWorker's handling of `border-collapse`, `padding`, and nested tables is a known limitation of its CSS 2.1 subset, and complex tables may need adjustments ([discussion on Stack Overflow](https://stackoverflow.com/questions/tagged/itextsharp+xmlworker))
- **Unicode characters**: Non-Latin scripts (Arabic, Chinese, etc.) typically require explicit font embedding and encoding configuration to render correctly
- **Memory growth in loops**: Generating PDFs in tight loops without explicit `reader.Close()` / `document.Dispose()` can leave allocations alive until GC, which may matter in batch jobs
- **Concurrency**: `PdfWriter` and `PdfStamper` are not designed to be shared across threads on the same file
- **1-based page indexing**: `for (int i = 1; i <= NumberOfPages; i++)` is a common source of off-by-one mistakes for developers used to 0-based C# collections

---

## When Teams Consider iTextSharp Migration

**Feature freeze on 5.x** caps how far the library can move with .NET. The iTextSharp 5.x line does not pick up features such as nullable reference types, records, or .NET 6+ improvements (minimal APIs, source generators) on its own. Teams building new features in 2026 typically weigh layering workarounds in legacy code versus moving to a library on an active release cadence.

**No vendor support on 5.x** means escalation paths are limited. If you hit a CSS rendering edge case in XMLWorker or a concurrency issue around `PdfStamper`, the practical options are community threads or reading the source. For regulated industries (finance, healthcare) where vendor support is part of compliance documentation, this is a real constraint.

**Long-standing 5.x issues stay where they are.** Unicode handling, table-border rendering through XMLWorker, and memory growth in batch operations are well-documented in community threads and unlikely to be redesigned on the deprecated line. Workarounds tend to accumulate as institutional knowledge over time.

**Capability gaps widen over time.** Modern design systems lean on CSS Grid, viewport units, and JavaScript-driven charts. When those don't map naturally onto a coordinate-based PDF API, teams end up maintaining a translation layer between web design and PDF output—often a meaningful share of the PDF subsystem's code.

**Deployment surface** is shaped by the original .NET Framework target. Teams standardizing on .NET 8+ and Linux containers typically need to plan around this when running legacy iTextSharp alongside modern services.

---

## Installation Comparison

### iTextSharp

```bash
# Core package (legacy 5.x line — pin to a current security-patched version)
dotnet add package iTextSharp

# Required for HTML rendering
dotnet add package itextsharp.xmlworker

# Optional packages for specific features
dotnet add package itextsharp.pdfa  # PDF/A compliance
dotnet add package itextsharp.xtra  # Additional features
```

```csharp
// Typical namespace imports (8+ lines)
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline;
```

### IronPDF

```bash
# Single package install
dotnet add package IronPdf
```

```csharp
// Minimal imports (1-2 lines)
using IronPdf;
using IronPdf.Rendering; // Optional for advanced rendering options
```

For framework-specific extensions (ASP.NET Core, Azure Functions), check the [official documentation](https://ironsoftware.com/suite/blog/using-ironsuite/csharp-create-pdf-ironpdf-tutorial/) for current package names.

---

## Conclusion

iTextSharp shaped how a generation of .NET developers built PDFs—a low-level, coordinate-driven API at a time when programmatic construction was the standard model. For teams running stable workloads on existing 5.x codebases, the library continues to function, security backports cover critical issues, and the AGPL option remains valid when source disclosure is acceptable. Many organizations on a paid commercial agreement with iText Group will simply migrate forward to iText 7/8/9 rather than to a different library, and that's a reasonable path.

Migration to a different library tends to make sense when: (1) modern HTML/CSS rendering matters and you don't want to maintain a translation layer onto a coordinate-based API; (2) you're targeting .NET 6+ on Linux and want to avoid dual build paths; (3) you keep running into the same long-tail 5.x friction with no vendor escalation path; or (4) commercial support documentation is required by your compliance posture and you don't want to renegotiate with iText Group / Apryse.

IronPDF maps to a different model: a Chromium rendering engine for modern web standards, 0-based page indexing that matches C# conventions, async APIs for I/O-heavy workloads, and a single NuGet package. Merge and watermark operations collapse from longer multi-step blocks to a few lines, and the AGPL question doesn't come up at all because the license model is commercial. For teams already maintaining HTML templates for web UIs, the same templates can drive PDF output.

**What's your hardest PDF pain right now—HTML fidelity, deployment constraints, or API complexity?** Drop your edge cases in the comments.

---

*For technical deep-dives, see the [IronPDF performance guide](https://ironsoftware.com/suite/blog/using-ironsuite/pdf-sdk-guide/) and [licensing documentation](https://ironsoftware.com/suite/blog/using-ironsuite/csharp-html-to-pdf-example/) for deployment strategies and trial setup.*
