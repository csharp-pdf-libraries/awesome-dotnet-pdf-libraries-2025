# How Can I Rotate Text in PDFs Using C#?

Rotating text in PDFs is a common need—whether for watermarks, vertical labels, or fitting data into tight table columns. In C#, you can rotate text in PDFs easily using IronPDF, a library that supports both HTML/CSS-based generation and direct PDF manipulation. Here’s how to get started, which techniques work best, and what to watch out for.

---

## Why Would I Need to Rotate Text in a PDF?

There are plenty of situations where rotated text makes your PDFs more useful or professional. You might want to:

- Add diagonal or repeating watermarks (like "CONFIDENTIAL")
- Display vertical or angled sidebar labels
- Create angled table headers to save space
- Stamp "APPROVED" or "VOID" across existing pages
- Design callouts or attention-grabbing ribbons

If you’re interested in stamping text or images specifically, check out [How do I stamp text or images onto a PDF in C#?](stamp-text-image-pdf-csharp.md).

---

## How Do I Set Up IronPDF for Text Rotation?

Getting started is straightforward. First, add IronPDF to your project via NuGet:

```bash
dotnet add package IronPdf
```

Or, using the Package Manager Console:

```powershell
Install-Package IronPdf
```

Requirements:
- .NET 6+ (works with .NET Framework as well)
- The IronPDF NuGet package

You’ll mainly use the `ChromePdfRenderer` for creating PDFs from HTML/CSS, or the `PdfDocument` class for working with existing PDFs. For more on PDF versions and compatibility, see [What PDF versions does IronPDF support in C#?](pdf-versions-csharp.md).

---

## How Can I Rotate Text When Generating PDFs from HTML/CSS?

IronPDF’s HTML-to-PDF engine supports CSS transforms, making it easy to rotate text any way you want. Here are some common scenarios:

### How Do I Add a Diagonal Watermark?

A diagonal watermark can be achieved by rotating a fixed-position DIV using CSS:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var html = @"
<div style='
    position: fixed; 
    top: 50%; left: 50%; 
    transform: translate(-50%, -50%) rotate(-45deg); 
    font-size: 88px; 
    color: rgba(220,0,0,0.12); 
    font-weight: bold; 
    user-select: none;
    z-index: 0;'>
    CONFIDENTIAL
</div>
<div style='z-index: 1; padding: 60px; position: relative;'>
    <h1>Project Report</h1>
    <p>Main content goes here.</p>
</div>";
var pdfDoc = renderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("diagonal-watermark.pdf");
```
For more advanced watermarking, check out [this watermark guide](https://ironpdf.com/java/blog/using-ironpdf-for-java/java-watermark-pdf-tutorial/).

### How Can I Create Vertical Labels or Sidebar Text?

Vertical or sidebar text is handy for margins or IDs:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var html = @"
<div style='
    position: fixed; 
    left: 12px; 
    top: 50%; 
    transform: rotate(-90deg) translateY(-50%);
    font-size: 15px; 
    color: #444;
    opacity: 0.7;'>
    Document ID: 2024-123
</div>
<h1>Document Content</h1>";
var pdfDoc = renderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("vertical-label.pdf");
```

### How Do I Rotate Table Headers?

Rotate header text to save horizontal space:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var html = @"
<table>
  <tr>
    <th><span style='display: block; transform: rotate(-45deg);'>Revenue</span></th>
    <th><span style='display: block; transform: rotate(-45deg);'>Expenses</span></th>
  </tr>
  <tr>
    <td>$1M</td>
    <td>$500k</td>
  </tr>
</table>";
var pdfDoc = renderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("angled-table.pdf");
```

### Can I Use Custom Rotation Angles and Origins?

Absolutely! Use `transform: rotate(Xdeg)` and specify `transform-origin` as needed:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var html = @"<div style='transform: rotate(-30deg); transform-origin: top left; font-size: 28px;'>Custom Rotation</div>";
var pdfDoc = renderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("custom-rotation.pdf");
```

### What About More Complex Effects (Ribbons, Tiled Watermarks, etc.)?

You can chain multiple CSS transforms for effects like ribbons or tiled marks. For example:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var html = @"<div style='position: absolute; top: 30px; right: -40px; background: crimson; color: white; padding: 6px; transform: rotate(35deg);'>NEW!</div>";
var pdfDoc = renderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("ribbon.pdf");
```

For more on extracting or replacing text, see [How do I parse or extract text from a PDF in C#?](parse-pdf-extract-text-csharp.md) and [How can I find and replace text in a PDF using C#?](find-replace-text-pdf-csharp.md).

---

## How Can I Rotate Text in Existing PDFs?

Already have a PDF and need to stamp rotated text? IronPDF provides two main options:

### How Do I Use TextStamper for Simple Rotated Stamps?

For quick, uniform rotated text (like "DRAFT" on every page):

```csharp
using IronPdf;
using IronPdf.Stamps; // Install-Package IronPdf

var pdf = PdfDocument.FromFile("input.pdf");
var stamp = new TextStamper
{
    Text = "DRAFT",
    FontSize = 80,
    FontColor = Color.DarkGray,
    Opacity = 20,
    Rotation = -45,
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Middle
};
pdf.ApplyStamp(stamp);
pdf.SaveAs("stamped.pdf");
```

### How Do I Use HtmlStamper for Advanced Rotated Content?

For more styling and layout flexibility, use HTML/CSS with `HtmlStamper`:

```csharp
using IronPdf;
using IronPdf.Stamps; // Install-Package IronPdf

var pdf = PdfDocument.FromFile("input.pdf");
var htmlStamp = new HtmlStamper
{
    Html = "<div style='transform: rotate(20deg); color: orange; font-size: 32px; font-weight: bold;'>APPROVED</div>",
    HorizontalAlignment = HorizontalAlignment.Left,
    VerticalAlignment = VerticalAlignment.Bottom,
    HorizontalOffset = new Length(35),
    VerticalOffset = new Length(35)
};
pdf.ApplyStamp(htmlStamp);
pdf.SaveAs("html-stamped.pdf");
```

Want details on stamping images? See [How do I stamp text or images onto a PDF in C#?](stamp-text-image-pdf-csharp.md).

---

## What Common Issues Should I Watch Out For When Rotating Text?

- **Content Overlap:** Use low opacity for watermarks and position rotated elements underneath main content using `z-index`.
- **Unexpected Rotation Origin:** Always set `transform-origin` to control pivot points.
- **CSS Limitations:** IronPDF uses a Chrome-based engine, so most—but not all—CSS3 is supported. Avoid `writing-mode` for vertical text; stick with rotation.
- **Font Consistency:** For custom fonts, use `@font-face` in HTML or ensure the font is available on your server.
- **Rotation Units:** IronPDF expects rotation angles in degrees, not radians.
- **Multi-Page PDFs:** By default, stamps apply to all pages, but you can specify page ranges.
- **Debugging:** Test your HTML/CSS in Chrome before generating a PDF for best results.

For more on .NET and browser compatibility, see [Can I use IronPDF with .NET 10 and WebAssembly?](webassembly-dotnet-10.md).

---

## Is There a Quick Reference for Rotation Angles and CSS?

Yes! Here’s a handy table for CSS rotation:

| Effect                | Example                         |
|-----------------------|---------------------------------|
| Diagonal watermark    | `transform: rotate(-45deg)`     |
| Vertical (left side)  | `transform: rotate(-90deg)`     |
| Vertical (right side) | `transform: rotate(90deg)`      |
| Upside down           | `transform: rotate(180deg)`     |
| Custom angle          | `transform: rotate(Xdeg)`       |

Set `transform-origin` like `top left`, `bottom center`, or percentages for different pivot points.

---

## Where Can I Learn More About PDF Manipulation in C#?

IronPDF’s documentation is a great resource: [IronPDF](https://ironpdf.com). For broader tools and libraries, visit [Iron Software](https://ironsoftware.com).

---

*Have more questions? Find [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) on the Iron Software team page. As CTO, Jacob leads development of IronPDF and the Iron Suite.*
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

**Jacob Mellor** — Creator of IronPDF, CTO at Iron Software. 41 years coding, 50++ person team, 41+ million NuGet downloads. First software business opened in London in 1999. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
