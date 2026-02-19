# How Do I Migrate from PdfPig to IronPDF in C#?

## Why Migrate from PdfPig?

PdfPig is an excellent open-source library for reading and extracting content from PDFs. However, its capabilities are fundamentally limited to parsing existing documents. When your application needs to grow beyond extraction—generating PDFs, converting HTML, manipulating documents, or adding security—PdfPig cannot help.

### The Reading-Only Limitation

PdfPig focuses exclusively on PDF reading:

1. **No PDF Generation**: Cannot create PDFs from HTML, URLs, or programmatically
2. **No HTML-to-PDF**: Cannot convert web content to PDF documents
3. **No Document Manipulation**: Cannot merge, split, or modify PDFs
4. **No Security Features**: Cannot add passwords, encryption, or digital signatures
5. **No Watermarks/Stamps**: Cannot add visual overlays to existing documents
6. **No Form Filling**: Cannot programmatically fill PDF forms
7. **Basic Text Layout**: Limited document creation with manual coordinate positioning only

### Why Choose [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/)?

IronPDF provides complete PDF lifecycle management:

- **Full PDF Generation**: Create PDFs from HTML, URLs, images, and more
- **Rich Extraction**: Text, images, and metadata extraction (like PdfPig)
- **Document Manipulation**: Merge, split, rotate, and modify PDFs
- **Security Features**: Passwords, encryption, digital signatures
- **Watermarks and Stamps**: Add visual overlays and branding
- **Form Handling**: Fill and flatten PDF forms
- **Modern Rendering**: Chromium-based engine with full CSS3/JS support
- **Active Support**: Commercial support and regular updates

---

## Quick Migration Overview

| Aspect | PdfPig | IronPDF |
|--------|--------|---------|
| Primary Focus | Reading/Extraction | Full PDF lifecycle |
| PDF Creation | Very limited | Comprehensive html to pdf c# |
| HTML to PDF | Not supported | Full Chromium engine |
| URL to PDF | Not supported | Full support |
| Text Extraction | Excellent | Excellent |
| Image Extraction | Yes | Yes |
| Metadata Access | Yes | Yes |
| PDF Manipulation | Not supported | Merge, split, rotate |
| Watermarks | Not supported | Full support |
| Security/Encryption | Not supported | Full support |
| Form Filling | Not supported | Full support |
| Digital Signatures | Not supported | Full support |
| License | Apache 2.0 (free) | Commercial |
| Support | Community | Professional |

For a side-by-side feature analysis, see the [detailed comparison](https://ironsoftware.com/suite/blog/comparison/compare-pdfpig-vs-ironpdf/).

---

## NuGet Package Changes

```bash
# Remove PdfPig
dotnet remove package PdfPig

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// Before: PdfPig
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

// After: IronPDF
using IronPdf;
using IronPdf.Rendering;
```

---

## API Mapping Reference

### Document Loading

| PdfPig | IronPDF | Notes |
|--------|---------|-------|
| `PdfDocument.Open(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `PdfDocument.Open(bytes)` | `PdfDocument.FromBinaryData(bytes)` | Load from bytes |
| `PdfDocument.Open(stream)` | `PdfDocument.FromStream(stream)` | Load from stream |
| `using (var doc = ...)` | `var pdf = ...` | IronPDF doesn't require using |

### Page Access and Properties

| PdfPig | IronPDF | Notes |
|--------|---------|-------|
| `document.NumberOfPages` | `pdf.PageCount` | Total page count |
| `document.GetPages()` | `pdf.Pages` | Page collection |
| `document.GetPage(1)` | `pdf.Pages[0]` | Single page (note: 1-based vs 0-based) |
| `page.Width` | `pdf.Pages[i].Width` | Page width |
| `page.Height` | `pdf.Pages[i].Height` | Page height |
| `page.Rotation` | `pdf.Pages[i].Rotation` | Page rotation |

### Text Extraction

| PdfPig | IronPDF | Notes |
|--------|---------|-------|
| `page.Text` | `pdf.Pages[i].Text` | Page text |
| `page.GetWords()` | `pdf.ExtractTextFromPage(i)` | Words/text from page |
| (manual loop) | `pdf.ExtractAllText()` | All text at once |
| `page.Letters` | N/A (use Text) | IronPDF uses text blocks |
| `word.Text` | N/A (string result) | Direct string access |
| `word.BoundingBox` | N/A | IronPDF doesn't expose word positions |

### Metadata

| PdfPig | IronPDF | Notes |
|--------|---------|-------|
| `document.Information.Title` | `pdf.MetaData.Title` | Document title |
| `document.Information.Author` | `pdf.MetaData.Author` | Author |
| `document.Information.Subject` | `pdf.MetaData.Subject` | Subject |
| `document.Information.Creator` | `pdf.MetaData.Creator` | Creator |
| `document.Information.Producer` | `pdf.MetaData.Producer` | Producer |
| `document.Information.Keywords` | `pdf.MetaData.Keywords` | Keywords |
| `document.Information.CreationDate` | `pdf.MetaData.CreationDate` | Creation date |
| `document.Information.ModifiedDate` | `pdf.MetaData.ModifiedDate` | Modified date |

### PDF Creation (Major Paradigm Shift)

| PdfPig | IronPDF | Notes |
|--------|---------|-------|
| `new PdfDocumentBuilder()` | `new ChromePdfRenderer()` | Different paradigm |
| `builder.AddPage(PageSize.A4)` | `renderer.RenderHtmlAsPdf(html)` | HTML-based creation |
| `page.AddText(text, size, point, font)` | `<p style='...'>{text}</p>` | CSS-based styling |
| `builder.Build()` → `byte[]` | `pdf.SaveAs(path)` | Different output |
| Manual coordinate positioning | HTML/CSS layout | Layout approach |

### Features Not in PdfPig (NEW in IronPDF)

| Feature | IronPDF Method |
|---------|----------------|
| HTML to PDF | `renderer.RenderHtmlAsPdf(html)` |
| URL to PDF | `renderer.RenderUrlAsPdf(url)` |
| Merge PDFs | `PdfDocument.Merge(pdfs)` |
| Split PDF | `pdf.CopyPages(start, end)` |
| Add Watermark | `pdf.ApplyWatermark(html)` |
| Password Protection | `pdf.SecuritySettings.UserPassword` |
| Digital Signature | `pdf.Sign(certificate)` |
| Fill Forms | `pdf.Form.GetFieldByName(name).Value` |
| Add Headers/Footers | `renderer.RenderingOptions.HtmlHeader` |

---

## Code Migration Examples

### Example 1: Basic Text Extraction

**Before (PdfPig):**
```csharp
using UglyToad.PdfPig;
using System;
using System.Text;

class Program
{
    static void Main()
    {
        using (var document = PdfDocument.Open("input.pdf"))
        {
            var text = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                text.AppendLine(page.Text);
            }

            Console.WriteLine(text.ToString());
            Console.WriteLine($"Total pages: {document.NumberOfPages}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        // Extract all text at once
        string text = pdf.ExtractAllText();
        Console.WriteLine(text);

        Console.WriteLine($"Total pages: {pdf.PageCount}");
    }
}
```

### Example 2: Page-by-Page Text Extraction

**Before (PdfPig):**
```csharp
using UglyToad.PdfPig;
using System;

class Program
{
    static void Main()
    {
        using (var document = PdfDocument.Open("input.pdf"))
        {
            for (int i = 1; i <= document.NumberOfPages; i++)
            {
                var page = document.GetPage(i);  // 1-based index
                Console.WriteLine($"--- Page {i} ---");
                Console.WriteLine(page.Text);
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        for (int i = 0; i < pdf.PageCount; i++)  // 0-based index
        {
            Console.WriteLine($"--- Page {i + 1} ---");
            string pageText = pdf.ExtractTextFromPage(i);
            Console.WriteLine(pageText);
        }
    }
}
```

### Example 3: Word Extraction with Layout

**Before (PdfPig):**
```csharp
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using System;
using System.Linq;

class Program
{
    static void Main()
    {
        using (var document = PdfDocument.Open("input.pdf"))
        {
            foreach (var page in document.GetPages())
            {
                var words = page.GetWords();

                foreach (var word in words)
                {
                    Console.WriteLine($"Word: '{word.Text}' at ({word.BoundingBox.Left}, {word.BoundingBox.Top})");
                }

                // Get words on same line
                var lineWords = words
                    .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
                    .OrderByDescending(g => g.Key);

                foreach (var line in lineWords)
                {
                    var lineText = string.Join(" ", line.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text));
                    Console.WriteLine(lineText);
                }
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        // IronPDF extracts text in reading order by default
        foreach (var page in pdf.Pages)
        {
            Console.WriteLine($"--- Page {page.PageIndex + 1} ---");
            Console.WriteLine(page.Text);
        }

        // Note: IronPDF doesn't expose word-level bounding boxes
        // For advanced layout analysis, consider using the text as-is
        // or combine with PdfPig for specialized layout analysis
    }
}
```

The [extensive documentation](https://ironpdf.com/blog/migration-guides/migrate-from-pdfpig-to-ironpdf/) provides additional strategies for converting extraction-focused workflows to full PDF lifecycle operations.

### Example 4: Metadata Extraction

**Before (PdfPig):**
```csharp
using UglyToad.PdfPig;
using System;

class Program
{
    static void Main()
    {
        using (var document = PdfDocument.Open("input.pdf"))
        {
            var info = document.Information;

            Console.WriteLine($"Title: {info.Title}");
            Console.WriteLine($"Author: {info.Author}");
            Console.WriteLine($"Subject: {info.Subject}");
            Console.WriteLine($"Creator: {info.Creator}");
            Console.WriteLine($"Producer: {info.Producer}");
            Console.WriteLine($"Keywords: {info.Keywords}");
            Console.WriteLine($"Created: {info.CreationDate}");
            Console.WriteLine($"Modified: {info.ModifiedDate}");
            Console.WriteLine($"Pages: {document.NumberOfPages}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");
        var info = pdf.MetaData;

        Console.WriteLine($"Title: {info.Title}");
        Console.WriteLine($"Author: {info.Author}");
        Console.WriteLine($"Subject: {info.Subject}");
        Console.WriteLine($"Creator: {info.Creator}");
        Console.WriteLine($"Producer: {info.Producer}");
        Console.WriteLine($"Keywords: {info.Keywords}");
        Console.WriteLine($"Created: {info.CreationDate}");
        Console.WriteLine($"Modified: {info.ModifiedDate}");
        Console.WriteLine($"Pages: {pdf.PageCount}");
    }
}
```

### Example 5: Basic PDF Creation (Paradigm Shift)

**Before (PdfPig):**
```csharp
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Core;
using System.IO;

class Program
{
    static void Main()
    {
        // PdfPig requires manual coordinate-based positioning
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);

        // Manual positioning - tedious and error-prone
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText("Hello World", 24, new PdfPoint(50, 800), font);
        page.AddText("This is a paragraph.", 12, new PdfPoint(50, 770), font);
        page.AddText("Line 2 of the paragraph.", 12, new PdfPoint(50, 755), font);
        page.AddText("Line 3 of the paragraph.", 12, new PdfPoint(50, 740), font);

        byte[] pdfBytes = builder.Build();
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // IronPDF uses HTML/CSS - natural and powerful
        var renderer = new ChromePdfRenderer();

        string html = @"
            <html>
            <head>
                <style>
                    body { font-family: Helvetica, Arial, sans-serif; margin: 50px; }
                    h1 { font-size: 24px; }
                    p { font-size: 12px; line-height: 1.5; }
                </style>
            </head>
            <body>
                <h1>Hello World</h1>
                <p>
                    This is a paragraph.
                    Line 2 of the paragraph.
                    Line 3 of the paragraph.
                </p>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 6: HTML to PDF (Not Possible in PdfPig)

**Before (PdfPig):**
```csharp
// NOT SUPPORTED
// PdfPig cannot convert HTML to PDF
// You would need an entirely different library
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Full HTML/CSS/JavaScript support
        string html = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; }
                    .header { background: #333; color: white; padding: 20px; }
                    .content { padding: 20px; }
                    table { border-collapse: collapse; width: 100%; }
                    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Sales Report</h1>
                </div>
                <div class='content'>
                    <table>
                        <tr><th>Product</th><th>Q1</th><th>Q2</th></tr>
                        <tr><td>Widget A</td><td>$10,000</td><td>$12,000</td></tr>
                        <tr><td>Widget B</td><td>$8,000</td><td>$9,500</td></tr>
                    </table>
                </div>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report.pdf");
    }
}
```

### Example 7: URL to PDF (Not Possible in PdfPig)

**Before (PdfPig):**
```csharp
// NOT SUPPORTED
// PdfPig cannot convert URLs to PDF
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Configure rendering options
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.RenderDelay = 2000;  // Wait for JS

        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

### Example 8: Merge PDFs (Not Possible in PdfPig)

**Before (PdfPig):**
```csharp
// NOT SUPPORTED
// PdfPig cannot merge PDFs
// Would need iTextSharp, PdfSharp, or similar
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Load PDFs
        var pdf1 = PdfDocument.FromFile("chapter1.pdf");
        var pdf2 = PdfDocument.FromFile("chapter2.pdf");
        var pdf3 = PdfDocument.FromFile("chapter3.pdf");

        // Merge all
        var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
        merged.SaveAs("complete-book.pdf");

        // Or merge from list
        var files = new List<string> { "a.pdf", "b.pdf", "c.pdf" };
        var combined = PdfDocument.Merge(files);
        combined.SaveAs("combined.pdf");
    }
}
```

### Example 9: Add Watermark (Not Possible in PdfPig)

**Before (PdfPig):**
```csharp
// NOT SUPPORTED
// PdfPig cannot add watermarks to existing PDFs
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");

        // Add HTML watermark
        pdf.ApplyWatermark(
            "<h2 style='color:red; opacity:0.5; font-size:48px;'>CONFIDENTIAL</h2>",
            rotation: 45,
            IronPdf.Editing.VerticalAlignment.Middle,
            IronPdf.Editing.HorizontalAlignment.Center
        );

        pdf.SaveAs("watermarked.pdf");
    }
}
```

### Example 10: Password Protection (Not Possible in PdfPig)

**Before (PdfPig):**
```csharp
// NOT SUPPORTED
// PdfPig cannot add password protection
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Create or load a PDF
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential Document</h1>");

        // Add security
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;

        pdf.SaveAs("secure.pdf");
    }
}
```

---

## Page Indexing Difference

PdfPig uses 1-based indexing, IronPDF uses 0-based:

```csharp
// PdfPig: 1-based
var page = document.GetPage(1);  // First page

// IronPDF: 0-based
var page = pdf.Pages[0];         // First page
string text = pdf.ExtractTextFromPage(0);  // First page
```

**Migration pattern:**
```csharp
// PdfPig loop
for (int i = 1; i <= document.NumberOfPages; i++)
{
    var page = document.GetPage(i);
}

// IronPDF loop
for (int i = 0; i < pdf.PageCount; i++)
{
    var page = pdf.Pages[i];
}
```

---

## Features Not Available in PdfPig

IronPDF provides many capabilities PdfPig cannot offer:

### PDF Generation

```csharp
// From HTML string with c# html to pdf
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");

// From URL
var webPdf = renderer.RenderUrlAsPdf("https://example.com");

// From HTML file
var filePdf = renderer.RenderHtmlFileAsPdf("template.html");
```

### Headers and Footers

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Company Report</div>",
    MaxHeight = 25
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>",
    MaxHeight = 20
};
```

### PDF Manipulation

```csharp
// Merge
var merged = PdfDocument.Merge(pdf1, pdf2);

// Split
var chapter1 = pdf.CopyPages(0, 9);

// Append
pdf.AppendPdf(otherPdf);

// Remove page
pdf.RemovePage(5);

// Rotate
pdf.RotatePage(0, PdfPageRotation.Rotate90);
```

### Form Handling

```csharp
// Fill form fields
pdf.Form.GetFieldByName("CustomerName").Value = "John Doe";
pdf.Form.GetFieldByName("Date").Value = DateTime.Now.ToShortDateString();

// Flatten (make non-editable)
pdf.Form.Flatten();
```

### Digital Signatures

```csharp
var signature = new PdfSignature("certificate.pfx", "password")
{
    SigningContact = "support@company.com",
    SigningReason = "Document Approval"
};
pdf.Sign(signature);
```

---

## Common Migration Issues

### 1. Using Statement Not Required

```csharp
// PdfPig: Requires using for disposal
using (var document = PdfDocument.Open("input.pdf"))
{
    // ...
}

// IronPDF: No using required (but can use for cleanup)
var pdf = PdfDocument.FromFile("input.pdf");
// ...
// pdf.Dispose(); // Optional
```

### 2. Page Index Adjustment

```csharp
// PdfPig: document.GetPage(1) - 1-based
// IronPDF: pdf.Pages[0] or pdf.ExtractTextFromPage(0) - 0-based

// Migration helper
int pdfPigIndex = 1;
int ironPdfIndex = pdfPigIndex - 1;
```

### 3. Creation Paradigm

```csharp
// PdfPig: Coordinate-based manual positioning
page.AddText("Hello", 12, new PdfPoint(50, 800), font);

// IronPDF: HTML/CSS-based layout
renderer.RenderHtmlAsPdf("<p style='margin:50px;'>Hello</p>");
```

### 4. Word Position Data

```csharp
// PdfPig: Provides word bounding boxes
foreach (var word in page.GetWords())
{
    var box = word.BoundingBox;  // Available
}

// IronPDF: Text extraction without position data
string text = pdf.ExtractAllText();  // Just text

// If you need word positions, consider keeping PdfPig for that task
// or use IronPDF for generation and PdfPig for analysis
```

### 5. License Configuration

```csharp
// PdfPig: No license needed (Apache 2.0)

// IronPDF: License required
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Set once at application startup
```

---

## Hybrid Approach

For advanced text analysis with PDF generation, consider using both libraries:

```csharp
using IronPdf;
using UglyToad.PdfPig;

class HybridPdfProcessor
{
    public void AnalyzeAndWatermark(string inputPath, string outputPath)
    {
        // Use PdfPig for detailed text analysis
        using (var pdfPigDoc = UglyToad.PdfPig.PdfDocument.Open(inputPath))
        {
            foreach (var page in pdfPigDoc.GetPages())
            {
                var words = page.GetWords();
                // Analyze word positions, detect tables, etc.
            }
        }

        // Use IronPDF for modification
        var ironPdf = IronPdf.PdfDocument.FromFile(inputPath);
        ironPdf.ApplyWatermark("<h2>REVIEWED</h2>");
        ironPdf.SaveAs(outputPath);
    }
}
```

---

## Server Deployment

### PdfPig
- Pure .NET, no native dependencies
- Works anywhere .NET runs
- Lightweight memory footprint

### IronPDF
- First run downloads Chromium (~150MB one-time)
- Linux requires additional dependencies:

```bash
# Ubuntu/Debian
apt-get update && apt-get install -y \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2
```

---

## Pre-Migration Checklist

- [ ] Inventory all PdfPig usage in your codebase
- [ ] Identify if you need word-level position data (consider hybrid approach)
- [ ] Note all page index references (need to convert 1-based to 0-based)
- [ ] Determine if PdfDocumentBuilder is used (requires HTML conversion)
- [ ] Plan IronPDF license key storage
- [ ] Test with IronPDF trial license first

## Post-Migration Checklist

- [ ] Remove PdfPig NuGet package (unless using hybrid approach)
- [ ] Update all namespace imports
- [ ] Set IronPDF license key at application startup
- [ ] Convert page indices from 1-based to 0-based
- [ ] Convert coordinate-based creation to HTML/CSS
- [ ] Remove `using` statements if not needed
- [ ] Test text extraction output matches expectations
- [ ] Test all PDF generation scenarios
- [ ] Install Linux dependencies if deploying to Linux

---

## Find All PdfPig References

```bash
# Find PdfPig usage
grep -r "UglyToad\.PdfPig\|PdfDocument\.Open\|GetPages\(\)" --include="*.cs" .

# Find page index references (may need 1→0 conversion)
grep -r "GetPage(\|NumberOfPages" --include="*.cs" .

# Find builder usage (requires paradigm shift)
grep -r "PdfDocumentBuilder\|AddText\|PdfPoint" --include="*.cs" .
```

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PdfPig usages in codebase**
  ```bash
  grep -r "using PdfPig" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package PdfPig
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace PDF generation calls**
  ```csharp
  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering.

- [ ] **Update page settings**
  ```csharp
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Configure headers/footers**
  ```csharp
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine provides modern rendering. Verify key documents.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  var merged = PdfDocument.Merge(pdf1, pdf2);
  pdf.SecuritySettings.UserPassword = "secret";
  pdf.ApplyWatermark("<h1>DRAFT</h1>");
  ```
  **Why:** IronPDF provides many additional features.

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **Getting Started Guide**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Code Examples**: https://ironpdf.com/examples/
- **Tutorials**: https://ironpdf.com/tutorials/
- **Support**: https://ironpdf.com/support/
