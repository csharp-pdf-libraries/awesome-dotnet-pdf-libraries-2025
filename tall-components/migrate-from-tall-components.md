# Migration Guide: Tall Components (TallPDF, PDFKit) → IronPDF

## Why Migrate from Tall Components?

Tall Components (TallPDF, PDFKit) has been discontinued after its acquisition by Apryse, with no new licenses available and users redirected to iText SDK. The product lacks modern HTML-to-PDF capabilities, supporting only XML-based document creation, making it unsuitable for contemporary web-based PDF generation. Additionally, extensive documented rendering bugs including blank pages, disappearing graphics, missing text, and incorrect font rendering make it unreliable for production applications.

## NuGet Package Changes

```bash
# Remove Tall Components packages
dotnet remove package TallComponents.PDF.Kit
dotnet remove package TallComponents.PDF.Layout
dotnet remove package TallComponents.PDF.Layout.Drawing

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Tall Components | IronPDF |
|----------------|---------|
| `TallComponents.PDF.Kit` | `IronPdf` |
| `TallComponents.PDF.Layout` | `IronPdf` |
| `TallComponents.PDF.Layout.Drawing` | `IronPdf.Drawing` |
| `TallComponents.PDF.Fonts` | `IronPdf.Fonts` |
| `TallComponents.PDF.Forms` | `IronPdf.Forms` |

## API Mapping

| Tall Components API | IronPDF API | Notes |
|--------------------|-------------|-------|
| `Document` | `PdfDocument` | Main PDF document class |
| `Document.Pages.Add()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Create PDF from HTML instead of XML |
| `Document.Write()` | `PdfDocument.SaveAs()` | Save PDF to file |
| `Document.Save(Stream)` | `PdfDocument.Stream` or `PdfDocument.BinaryData` | Get PDF as stream/bytes |
| `XMLDocument.Generate()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | HTML replaces XML for layout |
| `Page.Canvas` | Direct HTML/CSS rendering | No manual canvas needed |
| `Font.FromFile()` | `IronPdf.Fonts.FontTypes` | Font handling |
| `TextShape` | HTML/CSS text elements | Use standard HTML markup |
| `ImageShape` | `<img>` tags in HTML | Images via HTML |
| `PdfKit.Merger.Merge()` | `PdfDocument.Merge()` | Combine PDFs |
| `Page.Transformations` | CSS transforms | Use CSS for transformations |
| `Document.Security` | `PdfDocument.SecuritySettings` | PDF encryption/permissions |

## Code Examples

### Example 1: Creating a Simple PDF Document

**Before (Tall Components):**
```csharp
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Paragraphs;

// Create document with XML-based layout
Document document = new Document();
Section section = document.Sections.Add();

TextParagraph paragraph = new TextParagraph();
paragraph.Text = "Hello World";
section.Paragraphs.Add(paragraph);

using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
{
    document.Write(fs);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

// Create PDF from HTML
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: Adding Images and Formatted Content

**Before (Tall Components):**
```csharp
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Paragraphs;
using TallComponents.PDF.Layout.Shapes;

Document document = new Document();
Section section = document.Sections.Add();

// Add text with manual formatting
TextParagraph title = new TextParagraph();
title.Text = "Report Title";
title.Font = new Font("Arial", 24);
section.Paragraphs.Add(title);

// Add image with manual positioning
ImageParagraph imagePara = new ImageParagraph();
imagePara.Image = new FileImage("logo.png");
section.Paragraphs.Add(imagePara);

using (FileStream fs = new FileStream("report.pdf", FileMode.Create))
{
    document.Write(fs);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

// Create PDF with HTML and CSS
string html = @"
    <style>
        h1 { font-family: Arial; font-size: 24px; }
    </style>
    <h1>Report Title</h1>
    <img src='logo.png' alt='Logo' />
";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 3: Merging Multiple PDFs

**Before (Tall Components):**
```csharp
using TallComponents.PDF.Kit;

// Merge PDFs using PdfKit
Document doc1 = new Document("file1.pdf");
Document doc2 = new Document("file2.pdf");

PdfKit.Merger merger = new PdfKit.Merger();
merger.Append(doc1);
merger.Append(doc2);

Document merged = merger.Merge();
using (FileStream fs = new FileStream("merged.pdf", FileMode.Create))
{
    merged.Write(fs);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

// Merge PDFs with IronPDF
var pdf1 = PdfDocument.FromFile("file1.pdf");
var pdf2 = PdfDocument.FromFile("file2.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

## Common Gotchas

### 1. XML Layout vs HTML/CSS
Tall Components uses XML-based layout definitions, while IronPDF uses standard HTML/CSS. Convert your XML layout templates to HTML equivalents.

**Migration tip:** Use your existing XML data but render it with HTML templates using Razor, string interpolation, or templating engines.

### 2. Canvas-Based Drawing
Tall Components requires manual canvas drawing operations. IronPDF handles layout automatically through HTML/CSS.

**Migration tip:** Replace `Canvas`, `TextShape`, and `ImageShape` calls with equivalent HTML markup and CSS styling.

### 3. Font Handling
Tall Components uses `Font.FromFile()` for custom fonts. IronPDF automatically uses system fonts or embedded web fonts.

**Migration tip:** Use `@font-face` in CSS for custom fonts or rely on standard web-safe fonts.

### 4. Stream Handling
Tall Components uses `Document.Write(Stream)`. IronPDF provides multiple output options.

**Migration tip:** Use `PdfDocument.SaveAs()` for files, `PdfDocument.Stream` for MemoryStream, or `PdfDocument.BinaryData` for byte arrays.

### 5. Page Size and Orientation
Tall Components sets page properties on Document/Section objects. IronPDF uses CSS or rendering options.

**Migration tip:** Use `ChromePdfRenderer.RenderingOptions.PaperSize` and `PaperOrientation` properties, or set via CSS `@page` rules.

### 6. No Manual Page Management
Tall Components requires explicit page and section creation. IronPDF handles pagination automatically.

**Migration tip:** Let HTML content flow naturally. Use CSS `page-break-before/after` for manual page breaks when needed.

### 7. Security and Encryption
Both libraries support PDF security, but with different APIs.

**Migration tip:** Use `PdfDocument.SecuritySettings.SetOwnerPassword()` and `SetUserPassword()` in IronPDF instead of Tall Components' `Document.Security` properties.

## Additional Resources

- **IronPDF Documentation:** https://ironpdf.com/docs/
- **IronPDF Tutorials:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/docs/questions/html-to-pdf/
- **API Reference:** https://ironpdf.com/object-reference/api/