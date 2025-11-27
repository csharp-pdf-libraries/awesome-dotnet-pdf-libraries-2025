# How Can I Draw Lines and Rectangles on PDFs Using C# and IronPDF?

Need to programmatically add lines, boxes, or annotations to your PDFs in C#? IronPDF makes it straightforward to draw directly on your documents—perfect for signature fields, diagrams, grids, and more. This FAQ covers practical scenarios for drawing with IronPDF, from setup to advanced tips.

## Why Would I Want to Draw Directly on a PDF?

Sometimes HTML-to-PDF just isn't flexible enough. By drawing directly onto a PDF, you can precisely position highlights, boxes, diagrams, or annotations—ideal for features like:
- Signature fields on forms
- Organizational charts and diagrams
- Custom borders or separators
- Highlighting important content
- Creating graph paper or table layouts

For a deeper dive into manipulating PDF structure, see [How do I access the PDF DOM object in C#?](access-pdf-dom-object-csharp.md).

## How Do I Set Up IronPDF to Draw on PDFs?

Setting up IronPDF is easy via NuGet:

```csharp
// Install-Package IronPdf
```

Then add these using statements at the top of your C# file:

```csharp
using IronPdf;
using IronPdf.Drawing;
using System.Drawing;
```

And you're ready to start drawing!

## How Does the PDF Coordinate System Work?

PDFs use a top-left origin. All coordinates are measured in points (1 inch = 72 points). For example, Letter size is 612x792 points, while A4 is 595x842 points. To place a line or box exactly, use these units for precision.

## How Can I Draw Lines on a PDF in C#?

Drawing a line is as simple as specifying start and end points, color, and width:

```csharp
using IronPdf;
using IronPdf.Drawing;
using System.Drawing;
// Install-Package IronPdf

var doc = new PdfDocument("input.pdf");
doc.DrawLine(new PointF(50, 200), new PointF(550, 200), Color.Black, 1);
doc.SaveAs("output-with-line.pdf");
```

You can set any color and thickness, or draw at any angle. For advanced line drawing (including text overlays), check out [How do I draw text or bitmaps on a PDF in C#?](draw-text-bitmap-pdf-csharp.md).

### Can I Customize Line Styles or Draw on Multiple Pages?

While IronPDF currently supports solid lines, you can adjust color and width:

```csharp
doc.DrawLine(new PointF(50, 100), new PointF(550, 100), Color.Gray, 2);
```

To draw on specific pages, use the `pageIndex` parameter (zero-based):

```csharp
doc.DrawLine(new PointF(60, 60), new PointF(500, 60), Color.Red, 2, pageIndex: 0); // Page 1
```

If you need dashed or dotted lines, see the workaround in the common issues section below.

## How Do I Draw Rectangles, Borders, or Highlights?

Drawing a rectangle is just as direct:

```csharp
var doc = new PdfDocument("form.pdf");
doc.DrawRectangle(new RectangleF(50, 100, 200, 80), Color.Black, 1);
doc.SaveAs("form-box.pdf");
```

The rectangle starts at the top-left corner, with specified width and height in points.

### Can I Draw Filled Rectangles or Highlights?

IronPDF's `DrawRectangle` draws outlines by default, but you can simulate a filled box (useful for highlights or redaction) by stacking thin lines:

```csharp
var fillRect = new RectangleF(100, 200, 300, 40);
var highlight = Color.FromArgb(120, 255, 255, 0); // Semi-transparent yellow
for (float y = fillRect.Top; y < fillRect.Bottom; y++)
{
    doc.DrawLine(new PointF(fillRect.Left, y), new PointF(fillRect.Right, y), highlight, 1);
}
doc.DrawRectangle(fillRect, Color.Orange, 2); // Border
doc.SaveAs("highlighted.pdf");
```

Want to add text labels inside boxes? See [How do I draw text or bitmaps on a PDF in C#?](draw-text-bitmap-pdf-csharp.md).

## How Can I Add Signature Boxes or Annotate PDFs?

Signature boxes are a common use case. Here's how you might create a labeled signature field:

```csharp
int lastPage = doc.PageCount - 1;
var boxRect = new RectangleF(50, 650, 250, 60);
doc.DrawRectangle(boxRect, Color.Black, 1, lastPage);

// Draw a line for the signature
float lineY = boxRect.Bottom - 15;
doc.DrawLine(new PointF(boxRect.Left + 10, lineY), new PointF(boxRect.Right - 10, lineY), Color.Black, 0.5f, lastPage);
doc.SaveAs("signature-box.pdf");
```

For adding custom text or instructions, combine with `DrawText` as shown in [How do I draw text or bitmaps on a PDF in C#?](draw-text-bitmap-pdf-csharp.md).

## How Do I Draw Borders or Grids on a PDF Page?

To draw a page border (great for certificates):

```csharp
float margin = 30;
float width = doc.PageSizes[0].Width, height = doc.PageSizes[0].Height;
var border = new RectangleF(margin, margin, width - 2 * margin, height - 2 * margin);
doc.DrawRectangle(border, Color.DarkBlue, 3);
doc.SaveAs("bordered.pdf");
```

For custom grids (like graph paper):

```csharp
float startX = 50, startY = 50, gridW = 500, gridH = 700, cell = 20;
for (float x = startX; x <= startX + gridW; x += cell)
    doc.DrawLine(new PointF(x, startY), new PointF(x, startY + gridH), Color.LightGray, 0.5f);
for (float y = startY; y <= startY + gridH; y += cell)
    doc.DrawLine(new PointF(startX, y), new PointF(startX + gridW, y), Color.LightGray, 0.5f);
doc.DrawRectangle(new RectangleF(startX, startY, gridW, gridH), Color.Black, 1);
doc.SaveAs("grid.pdf");
```

For more grid and layout ideas, see [How do I draw lines and rectangles on a PDF in C#?](draw-line-rectangle-pdf-csharp.md).

## Can I Use Custom Colors or Transparency?

Absolutely! Any `System.Drawing.Color` (including alpha for transparency) is supported:

```csharp
var brandBlue = Color.FromArgb(0, 122, 204);
var transparentBlack = Color.FromArgb(100, 0, 0, 0);
doc.DrawRectangle(new RectangleF(50, 100, 200, 80), brandBlue, 3);
doc.DrawRectangle(new RectangleF(100, 400, 200, 100), transparentBlack, 1);
doc.SaveAs("custom-colors.pdf");
```

Not all PDF viewers handle transparency the same, so test on multiple platforms. For watermarking tips, see [this watermark guide](https://ironpdf.com/how-to/custom-watermark/).

## What Are Some Common Issues When Drawing on PDFs?

Here are some typical problems and solutions:

### Why Do My Drawings Appear in the Wrong Place?

PDF origin is top-left; make sure your coordinates and page size match your expectations. For more layout control, see [How do I access the PDF DOM object in C#?](access-pdf-dom-object-csharp.md).

### How Can I Create Dashed or Dotted Lines?

IronPDF doesn't natively support dashed lines, but you can mimic them by drawing short segments in a loop:

```csharp
float dash = 10, gap = 5, x1 = 60, x2 = 200, y = 300;
for (float x = x1; x < x2; x += dash + gap)
{
    float end = Math.Min(x + dash, x2);
    doc.DrawLine(new PointF(x, y), new PointF(end, y), Color.Black, 1);
}
```

### What If My Annotations Cover Existing Content?

Drawings are layered over the page. Use transparency for highlights, or adjust positions to avoid overlap. For advanced annotation, see [How do I draw text or bitmaps on a PDF in C#?](draw-text-bitmap-pdf-csharp.md).

### Is There a Performance Impact with Many Drawings?

Drawing hundreds of shapes can slow things down. Batch operations and avoid unnecessary redraws. For very complex layouts, consider rendering to a bitmap and embedding that image.

## Where Can I Learn More or Get Help?

For more drawing techniques, check out [How do I draw lines and rectangles on a PDF in C#?](draw-line-rectangle-pdf-csharp.md) or visit [IronPDF](https://ironpdf.com) for full documentation and tools. If you need to manipulate numbers (such as for grid spacing), see [How do I round numbers to 2 decimal places in C#?](csharp-round-to-2-decimal-places.md). And if your PDFs are large, see [How can I compress PDFs in C#?](pdf-compression-csharp.md).

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

[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) leads Iron Software's 50+-person engineering team from Chiang Mai, Thailand. A First-Class Honours Bachelor of Engineering (BEng) graduate from The University of Manchester, Jacob has built PDF libraries achieving 41+ million downloads. [GitHub](https://github.com/jacob-mellor) | [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/)
