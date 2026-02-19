# How Do I Migrate from Telerik Document Processing to IronPDF in C#?

## Table of Contents
1. [Why Migrate from Telerik Document Processing](#why-migrate-from-telerik-document-processing)
2. [The CSS/HTML Rendering Problem](#the-csshtml-rendering-problem)
3. [The Flow Document Problem](#the-flow-document-problem)
4. [Quick Start Migration (5 Minutes)](#quick-start-migration-5-minutes)
5. [Complete API Reference](#complete-api-reference)
6. [Code Migration Examples](#code-migration-examples)
7. [Feature Comparison](#feature-comparison)
8. [Licensing Comparison](#licensing-comparison)
9. [Troubleshooting Guide](#troubleshooting-guide)
10. [Migration Checklist](#migration-checklist)
11. [Additional Resources](#additional-resources)

---

## Why Migrate from Telerik Document Processing

### Critical Technical Limitations

Telerik Document Processing has fundamental issues when handling modern HTML/CSS:

| Issue | Impact | IronPDF Solution |
|-------|--------|------------------|
| **CSS parsing limitations** | Modern CSS frameworks like Bootstrap fail | Full Chromium CSS support |
| **Div-to-paragraph conversion** | HTML structure flattened, layouts break | Direct HTML rendering |
| **Flow document model** | Forces intermediate conversion | Native HTML-to-PDF |
| **External CSS issues** | Complex selectors ignored | Full CSS file support |
| **Memory issues** | OutOfMemoryException on large docs | Efficient streaming |

### The Core Problem: HTML is Not Rendered Correctly

Telerik Document Processing converts HTML to an intermediate "Flow Document" model before generating PDF. This process:

1. **Flattens HTML structure** - `<div>` becomes paragraphs
2. **Ignores modern CSS** - Flexbox, Grid layouts fail
3. **Breaks Bootstrap** - Column systems don't work
4. **Loses formatting** - Complex selectors ignored

```html
<!-- This modern HTML/CSS BREAKS in Telerik -->
<div class="container">
    <div class="row">
        <div class="col-md-6">Column 1</div>
        <div class="col-md-6">Column 2</div>
    </div>
</div>

<div style="display: flex; gap: 20px;">
    <div style="flex: 1;">Flex Item 1</div>
    <div style="flex: 1;">Flex Item 2</div>
</div>

<div style="display: grid; grid-template-columns: repeat(3, 1fr);">
    <div>Grid Item 1</div>
    <div>Grid Item 2</div>
    <div>Grid Item 3</div>
</div>
```

### Feature Comparison Overview

| Feature | Telerik Document Processing | IronPDF |
|---------|---------------------------|---------|
| **HTML Rendering** | Flow Document conversion | Direct Chromium rendering |
| **CSS3 Support** | Limited, many features fail | Full CSS3 |
| **Flexbox** | Not supported | Full support |
| **CSS Grid** | Not supported | Full support |
| **Bootstrap** | Broken (div flattening) | Full support |
| **External CSS** | Partial | Full support |
| **JavaScript** | Not supported | Full support |
| **Large Documents** | Memory issues | Efficient streaming |
| **API Complexity** | Complex (providers, models) | Simple (one class) |

---

## The CSS/HTML Rendering Problem

### What Happens in Telerik

Telerik's `HtmlFormatProvider` converts HTML to a `RadFlowDocument`:

```csharp
// ❌ Telerik - HTML is converted to Flow Document model
HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
RadFlowDocument document = htmlProvider.Import(htmlContent);

// What happens during import:
// 1. <div class="container"> → Paragraph
// 2. <div class="row"> → Paragraph
// 3. <div class="col-md-6"> → Paragraph
// 4. All layout CSS is IGNORED
// 5. Bootstrap grid becomes sequential paragraphs
// 6. Flexbox/Grid layouts fail completely
```

### CSS Features That Fail in Telerik

```css
/* ❌ These CSS features DON'T WORK in Telerik */

/* Flexbox - Not supported */
.container { display: flex; }
.item { flex: 1; }

/* CSS Grid - Not supported */
.grid { display: grid; grid-template-columns: repeat(3, 1fr); }

/* Bootstrap columns - Converted to paragraphs */
.col-md-6 { /* Ignored, becomes linear text */ }

/* CSS Variables - Not supported */
:root { --primary: #007bff; }
.btn { color: var(--primary); }

/* Complex selectors - Often ignored */
.container > .row:first-child { }
.item:hover { }
.content::before { }

/* Modern units - Limited support */
.box { width: calc(100% - 20px); }
.text { font-size: 1.2rem; }
```

### IronPDF's Chromium Engine

```csharp
// ✅ IronPDF - Renders exactly like a browser
var renderer = new ChromePdfRenderer();

var html = @"
<style>
    .container { display: flex; gap: 20px; }
    .item { flex: 1; padding: 20px; background: #f0f0f0; }
    .grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
</style>
<div class='container'>
    <div class='item'>Column 1</div>
    <div class='item'>Column 2</div>
</div>
<div class='grid'>
    <div>Grid 1</div>
    <div>Grid 2</div>
    <div>Grid 3</div>
</div>";

var pdf = renderer.RenderHtmlAsPdf(html);
// All CSS renders correctly - exactly like in Chrome!
```

---

## The Flow Document Problem

### Telerik's Complex Architecture

Telerik requires understanding multiple concepts:

```csharp
// ❌ Telerik - Complex provider/model architecture

// 1. Import HTML to Flow Document
HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
RadFlowDocument document = htmlProvider.Import(htmlContent);

// 2. Manually modify document model
RadFlowDocumentEditor editor = new RadFlowDocumentEditor(document);
Section section = document.Sections.First();
Paragraph para = section.Blocks.AddParagraph();
para.Inlines.AddText("Additional text");

// 3. Configure export settings
PdfExportSettings exportSettings = new PdfExportSettings();
exportSettings.ImageQuality = ImageQuality.High;

// 4. Create PDF provider with settings
PdfFormatProvider pdfProvider = new PdfFormatProvider();
pdfProvider.ExportSettings = exportSettings;

// 5. Export to bytes
byte[] pdfBytes = pdfProvider.Export(document);

// 6. Save to file
File.WriteAllBytes("output.pdf", pdfBytes);
```

### IronPDF's Simple Approach

```csharp
// ✅ IronPDF - Direct rendering, no intermediate models

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");

// That's it - 3 lines vs 15+ lines!
```

---

## Quick Start Migration (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove Telerik packages
dotnet remove package Telerik.Documents.Core
dotnet remove package Telerik.Documents.Flow
dotnet remove package Telerik.Documents.Flow.FormatProviders.Pdf
dotnet remove package Telerik.Documents.Fixed

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Using Statements

```csharp
// Before
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Documents.Primitives;

// After
using IronPdf;
```

### Step 3: Add License Key

```csharp
// Add at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 4: Update Code

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;

HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
RadFlowDocument document = htmlProvider.Import(htmlContent);

PdfFormatProvider pdfProvider = new PdfFormatProvider();
byte[] pdfBytes = pdfProvider.Export(document);

File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

---

## Complete API Reference

### Namespace Mapping

| Telerik Namespace | IronPDF Namespace | Notes |
|-------------------|-------------------|-------|
| `Telerik.Windows.Documents.Flow.Model` | `IronPdf` | Core classes |
| `Telerik.Windows.Documents.Flow.FormatProviders.Html` | `IronPdf` | `ChromePdfRenderer` |
| `Telerik.Windows.Documents.Flow.FormatProviders.Pdf` | `IronPdf` | Direct PDF ops |
| `Telerik.Windows.Documents.Fixed.Model` | `IronPdf` | `PdfDocument` |
| `Telerik.Documents.Primitives` | Not needed | Built-in |

### Core Class Mapping

| Telerik Class | IronPDF Equivalent | Notes |
|---------------|-------------------|-------|
| `HtmlFormatProvider` | `ChromePdfRenderer` | Direct HTML rendering |
| `PdfFormatProvider` | `PdfDocument` | Load/save PDFs |
| `RadFlowDocument` | Not needed | No intermediate model |
| `RadFlowDocumentEditor` | `PdfDocument` | Direct manipulation |
| `Section` | Not needed | Use HTML structure |
| `Paragraph` | Not needed | Use HTML `<p>` |
| `Header` | `HtmlHeaderFooter` | HTML-based headers |
| `Footer` | `HtmlHeaderFooter` | HTML-based footers |
| `HtmlImportSettings` | `RenderingOptions` | Configuration |
| `PdfExportSettings` | `RenderingOptions` | Configuration |

### Method Mapping

| Telerik Method | IronPDF Method | Notes |
|----------------|----------------|-------|
| `htmlProvider.Import(html)` | `renderer.RenderHtmlAsPdf(html)` | HTML string |
| `htmlProvider.Import(html)` | `renderer.RenderHtmlFileAsPdf(path)` | HTML file |
| `pdfProvider.Export(doc)` | `pdf.BinaryData` | Get bytes |
| `File.WriteAllBytes(path, bytes)` | `pdf.SaveAs(path)` | Save file |
| `document.Sections.First()` | `pdf.Pages[0]` | Access pages |
| `section.Blocks.AddParagraph()` | Use HTML | Add content |
| `editor.InsertText()` | Use HTML | Add text |
| `PdfDocument.Merge()` | `PdfDocument.Merge()` | Same method name |

### Configuration Mapping

| Telerik Config | IronPDF Config | Notes |
|----------------|----------------|-------|
| `PdfExportSettings.ImageQuality` | `RenderingOptions.ImageQuality` | Image quality |
| `HtmlImportSettings.DefaultStyleSheet` | CSS in HTML | Inline or linked |
| `Section.PageMargins` | `RenderingOptions.MarginTop/Bottom/Left/Right` | Margins (mm) |
| `Section.PageSize` | `RenderingOptions.PaperSize` | Paper size |
| `Section.PageOrientation.Landscape` | `RenderingOptions.PaperOrientation` | Orientation |
| `Header.Blocks.Add()` | `RenderingOptions.HtmlHeader` | HTML header |
| `Footer.Blocks.Add()` | `RenderingOptions.HtmlFooter` | HTML footer |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;

class Program
{
    static void Main()
    {
        string htmlContent = "<html><body><h1>Hello World</h1></body></html>";

        HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
        RadFlowDocument document = htmlProvider.Import(htmlContent);

        PdfFormatProvider pdfProvider = new PdfFormatProvider();
        byte[] pdfBytes = pdfProvider.Export(document);

        File.WriteAllBytes("output.pdf", pdfBytes);
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
        string htmlContent = "<html><body><h1>Hello World</h1></body></html>";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 2: HTML File with External CSS

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;
using System.IO;

class Program
{
    static void Main()
    {
        // Read HTML file
        string html = File.ReadAllText("document.html");

        // Complex CSS often breaks or is ignored
        HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
        HtmlImportSettings settings = new HtmlImportSettings();
        htmlProvider.ImportSettings = settings;

        RadFlowDocument document = htmlProvider.Import(html);

        PdfFormatProvider pdfProvider = new PdfFormatProvider();
        byte[] pdfBytes = pdfProvider.Export(document);

        File.WriteAllBytes("output.pdf", pdfBytes);
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
        // External CSS fully supported, resolved like browsers
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlFileAsPdf("document.html");
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 3: Headers and Footers

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;

class Program
{
    static void Main()
    {
        HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
        RadFlowDocument document = htmlProvider.Import("<h1>Content</h1>");

        // Manual header construction
        Section section = document.Sections.First();

        Header header = section.Headers.Add();
        Paragraph headerParagraph = header.Blocks.AddParagraph();
        headerParagraph.Inlines.AddText("Company Header");
        headerParagraph.StyleId = "Heading2";

        Footer footer = section.Footers.Add();
        Paragraph footerParagraph = footer.Blocks.AddParagraph();
        footerParagraph.Inlines.AddText("Page ");
        // Page numbers require complex field handling...

        PdfFormatProvider pdfProvider = new PdfFormatProvider();
        byte[] pdfBytes = pdfProvider.Export(document);

        File.WriteAllBytes("output.pdf", pdfBytes);
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
        var renderer = new ChromePdfRenderer();

        // Simple HTML-based headers with full styling
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
        {
            HtmlFragment = @"
                <div style='width:100%; text-align:center; font-size:14px;
                            border-bottom: 1px solid #ccc; padding: 5px;'>
                    Company Header
                </div>"
        };

        // Footer with automatic page numbers
        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
        {
            HtmlFragment = @"
                <div style='width:100%; text-align:center; font-size:10px;'>
                    Page {page} of {total-pages}
                </div>"
        };

        renderer.RenderingOptions.MarginTop = 40;
        renderer.RenderingOptions.MarginBottom = 30;

        var pdf = renderer.RenderHtmlAsPdf("<h1>Content</h1>");
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 4: Custom Page Settings

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Documents.Primitives;

class Program
{
    static void Main()
    {
        HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
        RadFlowDocument document = htmlProvider.Import("<h1>Content</h1>");

        // Configure page settings on section
        Section section = document.Sections.First();
        section.PageSize = PaperTypeConverter.ToSize(PaperTypes.A4);
        section.PageOrientation = PageOrientation.Landscape;
        section.PageMargins = new Padding(
            Unit.InchToDip(1),   // Left
            Unit.InchToDip(1),   // Top
            Unit.InchToDip(1),   // Right
            Unit.InchToDip(1)    // Bottom
        );

        PdfExportSettings exportSettings = new PdfExportSettings();
        exportSettings.ImageQuality = ImageQuality.High;

        PdfFormatProvider pdfProvider = new PdfFormatProvider();
        pdfProvider.ExportSettings = exportSettings;
        byte[] pdfBytes = pdfProvider.Export(document);

        File.WriteAllBytes("output.pdf", pdfBytes);
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
        var renderer = new ChromePdfRenderer();

        // Simple configuration
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.MarginTop = 25.4;    // 1 inch in mm
        renderer.RenderingOptions.MarginBottom = 25.4;
        renderer.RenderingOptions.MarginLeft = 25.4;
        renderer.RenderingOptions.MarginRight = 25.4;

        var pdf = renderer.RenderHtmlAsPdf("<h1>Content</h1>");
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 5: Merge Multiple PDFs

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using System.IO;

class Program
{
    static void Main()
    {
        PdfFormatProvider provider = new PdfFormatProvider();

        // Load first document
        RadFixedDocument doc1;
        using (Stream stream = File.OpenRead("doc1.pdf"))
        {
            doc1 = provider.Import(stream);
        }

        // Load second document
        RadFixedDocument doc2;
        using (Stream stream = File.OpenRead("doc2.pdf"))
        {
            doc2 = provider.Import(stream);
        }

        // Create merged document
        RadFixedDocument merged = new RadFixedDocument();

        // Copy pages manually
        foreach (RadFixedPage page in doc1.Pages)
        {
            merged.Pages.Add(page);
        }
        foreach (RadFixedPage page in doc2.Pages)
        {
            merged.Pages.Add(page);
        }

        // Export merged
        byte[] mergedBytes = provider.Export(merged);
        File.WriteAllBytes("merged.pdf", mergedBytes);
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
        var pdf1 = PdfDocument.FromFile("doc1.pdf");
        var pdf2 = PdfDocument.FromFile("doc2.pdf");

        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
    }
}
```

### Example 6: URL to PDF

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Flow.FormatProviders.Html;
using Telerik.Windows.Documents.Flow.FormatProviders.Pdf;
using System.Net.Http;

class Program
{
    static async Task Main()
    {
        // Must download HTML first
        using (var client = new HttpClient())
        {
            string html = await client.GetStringAsync("https://example.com");

            // External resources may not load correctly
            HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
            RadFlowDocument document = htmlProvider.Import(html);

            PdfFormatProvider pdfProvider = new PdfFormatProvider();
            byte[] pdfBytes = pdfProvider.Export(document);

            File.WriteAllBytes("webpage.pdf", pdfBytes);
        }
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
        var renderer = new ChromePdfRenderer();

        // Direct URL rendering with all resources
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

### Example 7: JavaScript Execution

**Before (Telerik):**
```csharp
// ❌ Telerik - JavaScript NOT SUPPORTED
// There is no way to execute JavaScript before PDF generation
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();

        // Enable JavaScript
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.WaitFor.JavaScript(2000);

        var html = @"
            <div id='content'></div>
            <script>
                fetch('/api/data')
                    .then(r => r.json())
                    .then(data => {
                        document.getElementById('content').innerHTML =
                            JSON.stringify(data);
                    });
            </script>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("dynamic.pdf");
    }
}
```

### Example 8: Password Protection

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export;

class Program
{
    static void Main()
    {
        PdfFormatProvider provider = new PdfFormatProvider();

        RadFixedDocument document;
        using (Stream stream = File.OpenRead("document.pdf"))
        {
            document = provider.Import(stream);
        }

        PdfExportSettings exportSettings = new PdfExportSettings();
        exportSettings.UserPassword = "userpass";
        exportSettings.OwnerPassword = "ownerpass";
        // Permissions configuration is complex...

        provider.ExportSettings = exportSettings;
        byte[] protectedBytes = provider.Export(document);

        File.WriteAllBytes("protected.pdf", protectedBytes);
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
        var pdf = PdfDocument.FromFile("document.pdf");

        // Simple security configuration
        pdf.SecuritySettings.UserPassword = "userpass";
        pdf.SecuritySettings.OwnerPassword = "ownerpass";
        pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

        pdf.SaveAs("protected.pdf");
    }
}
```

### Example 9: Text Extraction

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using System.Text;

class Program
{
    static void Main()
    {
        PdfFormatProvider provider = new PdfFormatProvider();

        RadFixedDocument document;
        using (Stream stream = File.OpenRead("document.pdf"))
        {
            document = provider.Import(stream);
        }

        StringBuilder text = new StringBuilder();

        // Complex text extraction through content elements
        foreach (RadFixedPage page in document.Pages)
        {
            foreach (ContentElementBase element in page.Content)
            {
                if (element is TextFragment textFragment)
                {
                    text.Append(textFragment.Text);
                }
            }
        }

        Console.WriteLine(text.ToString());
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
        var pdf = PdfDocument.FromFile("document.pdf");

        // Simple text extraction
        string allText = pdf.ExtractAllText();
        Console.WriteLine(allText);

        // Or per page
        foreach (var page in pdf.Pages)
        {
            Console.WriteLine(page.Text);
        }
    }
}
```

### Example 10: Watermarks

**Before (Telerik):**
```csharp
using Telerik.Windows.Documents.Fixed.Model;
using Telerik.Windows.Documents.Fixed.Model.Editing;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;

class Program
{
    static void Main()
    {
        PdfFormatProvider provider = new PdfFormatProvider();

        RadFixedDocument document;
        using (Stream stream = File.OpenRead("document.pdf"))
        {
            document = provider.Import(stream);
        }

        // Complex manual watermark drawing
        foreach (RadFixedPage page in document.Pages)
        {
            FixedContentEditor editor = new FixedContentEditor(page);
            editor.GraphicProperties.IsFilled = true;
            editor.GraphicProperties.FillColor = new RgbColor(200, 255, 0, 0);

            // Manual positioning required
            editor.Position.Translate(page.Size.Width / 2, page.Size.Height / 2);
            editor.Position.Rotate(45);

            Block block = new Block();
            block.TextProperties.FontSize = 72;
            block.InsertText("CONFIDENTIAL");

            editor.DrawBlock(block);
        }

        byte[] watermarkedBytes = provider.Export(document);
        File.WriteAllBytes("watermarked.pdf", watermarkedBytes);
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
        var pdf = PdfDocument.FromFile("document.pdf");

        // Simple HTML-based watermark
        pdf.ApplyWatermark(
            "<h1 style='color:red; opacity:0.3; font-size:72px;'>CONFIDENTIAL</h1>",
            rotation: 45,
            verticalAlignment: VerticalAlignment.Middle,
            horizontalAlignment: HorizontalAlignment.Center
        );

        pdf.SaveAs("watermarked.pdf");
    }
}
```

---

## Feature Comparison

### HTML/CSS Rendering

| Feature | Telerik | IronPDF |
|---------|---------|---------|
| **HTML5** | Partial | Full |
| **CSS3** | Limited | Full |
| **Flexbox** | ❌ Not supported | ✅ Full |
| **CSS Grid** | ❌ Not supported | ✅ Full |
| **Bootstrap** | ❌ Broken (divs flatten) | ✅ Full |
| **CSS Variables** | ❌ Not supported | ✅ Full |
| **External CSS** | ⚠️ Partial | ✅ Full |
| **Web Fonts** | ⚠️ Limited | ✅ Full |
| **JavaScript** | ❌ Not supported | ✅ Full |

### PDF Operations

| Feature | Telerik | IronPDF |
|---------|---------|---------|
| **Merge PDFs** | ✅ Complex API | ✅ Simple |
| **Split PDFs** | ✅ Complex API | ✅ Simple |
| **Extract Text** | ✅ Complex iteration | ✅ Simple method |
| **Add Watermarks** | ✅ Complex drawing | ✅ HTML-based |
| **Password Protection** | ✅ | ✅ |
| **Form Filling** | ✅ | ✅ |
| **Digital Signatures** | ✅ | ✅ |
| **PDF/A** | ⚠️ Limited | ✅ Full |

### API Complexity

| Task | Telerik Lines | IronPDF Lines |
|------|--------------|---------------|
| HTML to PDF | 8-10 | 3 |
| Add Header/Footer | 15+ | 5 |
| Merge PDFs | 20+ | 4 |
| Extract Text | 15+ | 2 |
| Add Watermark | 20+ | 5 |
| Password Protect | 12+ | 5 |

---

## Licensing Comparison

### Telerik Licensing

| License | Price Range | Includes |
|---------|-------------|----------|
| **DevCraft UI** | ~$1,599/year | Document Processing included |
| **Ultimate** | ~$2,299/year | Full suite |
| **Per-product** | Not available | Must buy suite |

**Issues:**
- Must buy entire Telerik suite
- Annual renewal required
- Complex deployment licensing

### IronPDF Licensing

| License | Price | Includes |
|---------|-------|----------|
| **Lite** | $749 | 1 developer, 1 project |
| **Professional** | $1,499 | 10 developers, 10 projects |
| **Unlimited** | $2,999 | Unlimited |

**Benefits:**
- Standalone purchase
- Perpetual license option
- Clear pricing
- Professional support included

---

## Troubleshooting Guide

### Issue 1: Layouts Look Different After Migration

**Symptom:** PDF layouts changed (usually for the better!)

**Cause:** Telerik's flow document model lost CSS information

**Solution:** IronPDF renders CSS correctly - your layouts should look like browsers now:
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Screen;

var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 2: RadFlowDocument Not Found

**Symptom:** Class doesn't exist in IronPDF

**Solution:** No intermediate document model needed:
```csharp
// ❌ Remove this pattern
HtmlFormatProvider htmlProvider = new HtmlFormatProvider();
RadFlowDocument document = htmlProvider.Import(html);
PdfFormatProvider pdfProvider = new PdfFormatProvider();
byte[] bytes = pdfProvider.Export(document);

// ✅ Use this instead
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Issue 3: HtmlFormatProvider Not Found

**Symptom:** Import method doesn't exist

**Solution:** Use `ChromePdfRenderer`:
```csharp
// ❌ Remove
HtmlFormatProvider provider = new HtmlFormatProvider();
var doc = provider.Import(html);

// ✅ Use
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 4: PdfExportSettings Not Available

**Symptom:** Export settings class not found

**Solution:** Use `RenderingOptions`:
```csharp
var renderer = new ChromePdfRenderer();

// Configure options on renderer
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.PrintHtmlBackgrounds = true;

var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 5: Section/Paragraph Not Available

**Symptom:** Document model classes not found

**Solution:** Use HTML instead of programmatic construction:
```csharp
// ❌ Remove this
Section section = document.Sections.First();
Paragraph para = section.Blocks.AddParagraph();
para.Inlines.AddText("Hello World");

// ✅ Use HTML
var html = "<p>Hello World</p>";
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 6: Unit Conversions

**Symptom:** Margins/sizes don't match

**Solution:** IronPDF uses millimeters:
```csharp
// Telerik uses DIPs (device-independent pixels)
// IronPDF uses millimeters

// 1 inch = 25.4mm
renderer.RenderingOptions.MarginTop = 25.4;    // 1 inch
renderer.RenderingOptions.MarginBottom = 25.4;

// Or use helper
renderer.RenderingOptions.SetCustomPaperSizeInInches(8.5, 11);
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit all Telerik Document Processing usage**
  ```bash
  grep -r "using Telerik.Windows.Documents" --include="*.cs" .
  grep -r "RadFlowDocument\|HtmlFormatProvider\|PdfFormatProvider" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **List all format providers used (Html, Pdf)**
  ```csharp
  // Before (Telerik)
  var htmlProvider = new HtmlFormatProvider();
  var pdfProvider = new PdfFormatProvider();
  ```
  **Why:** Determine which format providers are in use to plan their replacement with IronPDF.

- [ ] **Document header/footer implementations**
  ```csharp
  // Before (Telerik)
  section.Headers.Default.Blocks.AddParagraph().Inlines.AddRun("Header Text");
  section.Footers.Default.Blocks.AddParagraph().Inlines.AddRun("Footer Text");
  ```
  **Why:** Document existing header/footer logic to replicate it using IronPDF's HTML-based headers/footers.

- [ ] **Identify custom page settings**
  ```csharp
  // Before (Telerik)
  section.PageSize = PaperSize.A4;
  section.PageOrientation = PageOrientation.Landscape;
  section.PageMargins = new Padding(20);
  ```
  **Why:** Custom page settings need to be mapped to IronPDF's RenderingOptions for consistent output.

- [ ] **Note any Flow Document model modifications**
  ```csharp
  // Before (Telerik)
  var document = new RadFlowDocument();
  document.Sections.Add(new Section());
  ```
  **Why:** Understand document structure to transition to direct HTML rendering with IronPDF.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Set up test environment**
  **Why:** Ensure a safe environment to test the migration without affecting production systems.

### During Migration

- [ ] **Remove Telerik NuGet packages**
  ```bash
  dotnet remove package Telerik.Documents.SpreadsheetStreaming
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to the project for PDF generation.

- [ ] **Update using statements**
  ```csharp
  // Before (Telerik)
  using Telerik.Windows.Documents;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure the code references the correct namespaces for IronPDF.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace `HtmlFormatProvider` with `ChromePdfRenderer`**
  ```csharp
  // Before (Telerik)
  var htmlProvider = new HtmlFormatProvider();
  var document = htmlProvider.Import(html);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** IronPDF uses ChromePdfRenderer for direct HTML-to-PDF rendering.

- [ ] **Remove `RadFlowDocument` intermediate step**
  ```csharp
  // Before (Telerik)
  var document = new RadFlowDocument();
  var section = document.Sections.AddSection();

  // After (IronPDF)
  // Direct HTML rendering, no intermediate document needed
  ```
  **Why:** Simplifies the process by removing unnecessary intermediate document creation.

- [ ] **Replace `PdfFormatProvider` with direct save**
  ```csharp
  // Before (Telerik)
  var pdfProvider = new PdfFormatProvider();
  pdfProvider.Export(document, stream);

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Directly save the PDF using IronPDF's methods.

- [ ] **Update header/footer to `HtmlHeaderFooter`**
  ```csharp
  // Before (Telerik)
  section.Headers.Default.Blocks.AddParagraph().Inlines.AddRun("Header Text");

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Header Text</div>",
      MaxHeight = 25
  };
  ```
  **Why:** Use IronPDF's HTML-based headers for more flexibility and styling options.

- [ ] **Convert page settings to `RenderingOptions`**
  ```csharp
  // Before (Telerik)
  section.PageSize = PaperSize.A4;
  section.PageOrientation = PageOrientation.Landscape;

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  ```
  **Why:** IronPDF's RenderingOptions provide a unified way to set page configurations.

- [ ] **Update margin units (DIPs → mm)**
  ```csharp
  // Before (Telerik)
  section.PageMargins = new Padding(20);

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 5.08; // 20 DIPs to mm
  renderer.RenderingOptions.MarginBottom = 5.08;
  renderer.RenderingOptions.MarginLeft = 5.08;
  renderer.RenderingOptions.MarginRight = 5.08;
  ```
  **Why:** IronPDF uses millimeters for margin settings.

- [ ] **Replace Section/Paragraph with HTML**
  ```csharp
  // Before (Telerik)
  section.Blocks.AddParagraph().Inlines.AddRun("Text");

  // After (IronPDF)
  var htmlContent = "<p>Text</p>";
  var pdf = renderer.RenderHtmlAsPdf(htmlContent);
  ```
  **Why:** Use HTML for content to leverage IronPDF's rendering capabilities.

- [ ] **Update merge operations**
  ```csharp
  // Before (Telerik)
  var mergedDocument = new RadFlowDocument();
  mergedDocument.Merge(document1, document2);

  // After (IronPDF)
  var mergedPdf = PdfDocument.Merge(pdf1, pdf2);
  ```
  **Why:** IronPDF provides a straightforward method for merging PDFs.

### Post-Migration

- [ ] **Run all unit tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Verify CSS rendering (should improve!)**
  **Why:** IronPDF's Chromium engine provides full CSS3 support.

- [ ] **Test Bootstrap layouts (should work now)**
  **Why:** Bootstrap layouts are fully supported with IronPDF's HTML rendering.

- [ ] **Verify Flexbox/Grid (should work now)**
  **Why:** Modern CSS features like Flexbox and Grid are supported by IronPDF.

- [ ] **Test JavaScript execution**
  **Why:** Ensure dynamic content renders correctly with IronPDF's JavaScript support.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Performance test**
  **Why:** Ensure the migration does not negatively impact performance.

- [ ] **Update CI/CD pipelines**
  **Why:** Ensure build and deployment processes are updated to use IronPDF.

- [ ] **Update documentation**
  **Why:** Reflect changes in code and capabilities in project documentation.
---

## Additional Resources

### Migration Reference
- **[Telerik Document Processing to IronPDF](https://ironpdf.com/blog/migration-guides/migrate-from-telerik-document-processing-to-ironpdf/)**

### IronPDF Documentation
- **Getting Started**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF**: https://ironpdf.com/how-to/html-file-to-pdf/

### Support
- **IronPDF Support**: https://ironpdf.com/support/
- **License Information**: https://ironpdf.com/licensing/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*
