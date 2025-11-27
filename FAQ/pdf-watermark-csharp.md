# How Do I Add Watermarks to PDF Files in C# with IronPDF?

Need to stamp a big "CONFIDENTIAL" or your company logo across a PDF? With IronPDF for C#, watermarking is flexible, powerful, and fast—no magic required. This FAQ covers practical patterns, code examples, and common pitfalls, so you can confidently watermark PDFs in your projects.

---

## What Is a PDF Watermark and When Should I Use One?

A PDF watermark is a semi-transparent overlay—text or image—placed on one or more pages to indicate status (like "DRAFT" or "CONFIDENTIAL"), apply branding, or mark copyright. Unlike a sticker-like stamp, watermarks usually blend with underlying content through transparency and styling. Typical uses include branding reports, marking internal documents, or subtly adding logos.

---

## How Can I Add a Simple Watermark to a PDF in C#?

You can watermark a PDF with just a few lines using IronPDF. Here’s the quickest way to add a bold text watermark:

```csharp
using IronPdf; // Install-Package IronPdf

var doc = new PdfDocument("input.pdf");
doc.ApplyWatermark("<h1 style='color:red;'>DRAFT</h1>");
doc.SaveAs("with-watermark.pdf");
```

This loads a PDF, applies an HTML watermark, and saves the result. For more on creating PDFs from markup, see [Xml To Pdf Csharp](xml-to-pdf-csharp.md) or [Xaml To Pdf Maui Csharp](xaml-to-pdf-maui-csharp.md).

---

## Why Choose IronPDF for Watermarking PDFs?

IronPDF stands out because it lets you use HTML and CSS to style your watermarks, not just plain text. You can overlay images, combine elements, control placement, and tweak opacity or rotation. It’s robust for both small and large PDFs, and the [API supports advanced scenarios](https://ironpdf.com/how-to/stamp-text-image/).

---

## How Can I Style Text Watermarks Using CSS?

With IronPDF, any HTML/CSS is fair game. Here’s an example of a styled, centered watermark:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1><p>Data goes here.</p>");

string watermarkHtml = "<h1 style='color:#888; font-size:60px; font-family:sans-serif; opacity:0.8;'>CONFIDENTIAL</h1>";
pdf.ApplyWatermark(watermarkHtml);
pdf.SaveAs("styled-watermark.pdf");
```

You have full control—font, color, size, and effects. For custom fonts and icons, see [Web Fonts Icons Pdf Csharp](web-fonts-icons-pdf-csharp.md).

---

## How Do I Adjust Watermark Opacity for Subtlety?

Opacity is controlled via the `opacity` parameter (0–100). Lower values make the watermark more transparent:

```csharp
using IronPdf; // Install-Package IronPdf

var pdf = new PdfDocument("financials.pdf");
pdf.ApplyWatermark("<h2 style='color:blue;'>SAMPLE</h2>", opacity: 20);
pdf.SaveAs("subtle-sample.pdf");
```

For most documents, 15–30% is subtle yet visible. If you're printing, slightly higher values can help.

---

## Can I Rotate My Watermark Diagonally or Vertically?

Absolutely—just use the `rotation` parameter:

```csharp
using IronPdf; // Install-Package IronPdf

var pdf = new PdfDocument("nda.pdf");
pdf.ApplyWatermark("<h1 style='color:#b30000; font-size:100px;'>VOID</h1>", rotation: -45, opacity: 35);
pdf.SaveAs("diagonal-void.pdf");
```

Negative values rotate counter-clockwise; positive values are clockwise. Vertical watermarks? Use `rotation: 90`.

---

## How Do I Position Watermarks (Corners, Center, Custom)?

IronPDF lets you anchor watermarks to any of nine grid positions—top/middle/bottom × left/center/right. For example:

```csharp
using IronPdf; // Install-Package IronPdf

var pdf = new PdfDocument("doc.pdf");
// Center
pdf.ApplyWatermark("<h1>DRAFT</h1>");
// Top-right logo
pdf.ApplyWatermark("<img src='logo.png' style='width:80px;' />", verticalAlignment: VerticalAlignment.Top, horizontalAlignment: HorizontalAlignment.Right, opacity: 22);
pdf.SaveAs("positioned.pdf");
```

Need pixel-perfect control? Wrap your watermark HTML in a `<div>` with inline CSS like `top:50px; left:100px;`.

---

## Can I Watermark with Images or Combine Images and Text?

Yes—you can use `<img>` tags (file or Base64). Here’s how to embed a logo:

```csharp
using IronPdf; // Install-Package IronPdf

var pdf = new PdfDocument("proposal.pdf");
// Embed image via Base64
var bytes = File.ReadAllBytes("logo.png");
var base64 = Convert.ToBase64String(bytes);
var imgTag = $"<img src='data:image/png;base64,{base64}' style='width:100px;' />";
pdf.ApplyWatermark(imgTag, opacity: 30);
pdf.SaveAs("logo-watermark.pdf");
```

Mix text and images by combining them in a single HTML block. For advanced web font icons, see [Web Fonts Icons Pdf Csharp](web-fonts-icons-pdf-csharp.md).

---

## How Do I Watermark Only Certain Pages?

Use the `pageIndices` parameter to specify which pages to watermark (zero-based):

```csharp
using IronPdf; // Install-Package IronPdf

var pdf = new PdfDocument("multi.pdf");
pdf.ApplyWatermark("<h1>Intro</h1>", pageIndices: new[] { 0 }); // Only first page
pdf.ApplyWatermark("<h1>Appendix</h1>", pageIndices: new[] { pdf.PageCount - 1 }); // Last page
pdf.SaveAs("selective.pdf");
```

This is great for covers, appendices, or selective branding.

---

## What Are Common Pitfalls When Watermarking PDFs?

**External images not loading:** Use absolute paths or embed as Base64.  
**Opacity confusion:** IronPDF’s `opacity` and CSS `opacity` multiply together—use one or the other.  
**Page indices:** Remember, page numbers start at zero.  
**Overlapping watermarks:** Multiple calls to `ApplyWatermark` stack overlays.  
**Fonts not rendering:** Reference web fonts or ensure fonts are installed. See [Web Fonts Icons Pdf Csharp](web-fonts-icons-pdf-csharp.md) for font tips.

---

## Can I Remove a Watermark After Saving?

No—once a watermark is applied, it’s part of the page content. There’s no “undo” unless you kept an original. Always save a clean backup before watermarking.

---

## What About Batch Processing or Automated Watermarking?

Batch-processing multiple PDFs is straightforward. Here’s a quick loop:

```csharp
using IronPdf; // Install-Package IronPdf

foreach (var file in Directory.GetFiles("invoices", "*.pdf"))
{
    var pdf = new PdfDocument(file);
    string label = file.Contains("PAID") ? "PAID" : "DUE";
    int opacity = label == "PAID" ? 20 : 40;
    pdf.ApplyWatermark($"<h2 style='color:red;'>{label}</h2>", opacity: opacity, rotation: -30);
    pdf.SaveAs(Path.Combine("output", Path.GetFileName(file)));
}
```

Perfect for status stamping invoices, contracts, or reports.

---

## Where Can I Learn More About PDF Watermarking and Related PDF Tasks?

For more on PDF creation, conversion, and manipulation in C#, check out:
- [Xml To Pdf Csharp](xml-to-pdf-csharp.md)
- [Xaml To Pdf Maui Csharp](xaml-to-pdf-maui-csharp.md)
- [Web Fonts Icons Pdf Csharp](web-fonts-icons-pdf-csharp.md)
- [Why Pdf Libraries Exist And Cost Money](why-pdf-libraries-exist-and-cost-money.md)
- [Pdf To Images Csharp](pdf-to-images-csharp.md)

Explore the full IronPDF library at [IronPDF](https://ironpdf.com) and find more developer tools at [Iron Software](https://ironsoftware.com).

---

*[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), CTO at [Iron Software](https://ironsoftware.com). Building tools that make developers' lives easier since 2017.*
---

## Related Resources

### 📚 Tutorials & Guides
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Complete conversion tutorial
- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Library comparison
- **[Beginner Tutorial](../csharp-pdf-tutorial-beginners.md)** — First PDF in 5 minutes
- **[Decision Flowchart](../choosing-a-pdf-library.md)** — Find the right library

### 🔧 PDF Operations
- **[Merge & Split PDFs](../merge-split-pdf-csharp.md)** — Combine documents
- **[Digital Signatures](../digital-signatures-pdf-csharp.md)** — Sign legally
- **[Extract Text](../extract-text-from-pdf-csharp.md)** — Text extraction
- **[Fill PDF Forms](../fill-pdf-forms-csharp.md)** — Form automation

### 🚀 Framework Integration
- **[ASP.NET Core PDF](../asp-net-core-pdf-reports.md)** — Web app integration
- **[Blazor PDF Generation](../blazor-pdf-generation.md)** — Blazor support
- **[Cross-Platform Deployment](../cross-platform-pdf-dotnet.md)** — Docker, Linux, Cloud

### 📖 Library Documentation
- **[IronPDF](../ironpdf/)** — Full Chromium rendering
- **[QuestPDF](../questpdf/)** — Code-first generation
- **[PDFSharp](../pdfsharp/)** — Open source option

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared with 167 FAQ articles.*


---

[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) is the CTO of Iron Software, where he leads a 50+ person team building .NET libraries with over 41+ million NuGet downloads. With 41 years of coding experience, Jacob focuses on developer experience and cross-platform solutions. Based in Chiang Mai, Thailand, connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
