# How Can I Generate Custom-Sized PDFs in C# Using IronPDF?

If you’ve ever struggled to create PDFs that aren’t just A4 or Letter in C#, you’re not alone. Whether you need to output shipping labels, receipts, business cards, or even huge posters, being able to set the exact PDF paper size is a must. IronPDF makes this process straightforward, letting you specify paper dimensions, margins, orientation, and more—so your PDFs always match your real-world needs.

Below, you’ll find answers to the most common developer questions about generating custom-sized PDFs in .NET using IronPDF, complete with code samples and troubleshooting tips.

---

## Why Would I Need Custom PDF Paper Sizes in C#?

Many real-world PDF scenarios require sizes beyond the standard A4 or Letter. For example:

- **Shipping labels** (like 4”x6”)
- **Thermal receipts** (80mm wide, variable length)
- **Business cards**, tickets, or wristbands
- **Large posters** or banners

Printers, postal services, and even label manufacturers often require very specific paper dimensions. With IronPDF, you aren’t constrained to the usual standards—you can tailor PDFs for all sorts of physical formats. For more on converting different markup formats to PDF, see [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md) or [How can I generate PDFs from XAML in .NET MAUI?](xaml-to-pdf-maui-csharp.md).

---

## How Do I Quickly Create a Custom-Sized PDF in C#?

You can specify any paper size when generating a PDF with IronPDF. Let’s say you want a 100mm x 150mm shipping label—here’s a practical example:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();

// Set the paper size to 100mm x 150mm
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(100, 150);

// Example shipping label HTML
var html = @"
    <style>
        body { font-family: Arial, sans-serif; }
        .address { font-size: 14px; margin-bottom: 18px; }
        .barcode { font-size: 25px; letter-spacing: 2px; }
    </style>
    <div class='address'>
        <strong>Jane Smith</strong><br>
        456 Oak Street<br>
        Hometown, CA 90210
    </div>
    <div class='barcode'>1Z999AA10123456784</div>
";
var pdfDoc = renderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("shipping-label.pdf");
```

This method gives you complete control over the output size, ensuring your PDF fits your printer or label stock exactly.

---

## What Standard Paper Sizes Does IronPDF Support?

IronPDF comes with a wide variety of built-in paper sizes—no need to look up dimensions. You just set the `PaperSize` property:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
renderer.RenderingOptions.PaperSize = PdfPaperSize.A5;

var pdf = renderer.RenderHtmlAsPdf("<h1>Standard size example</h1>");
pdf.SaveAs("standard.pdf");
```

### Why Use Preset Sizes?

- Prevent mistakes with dimensions
- Match common international or North American standards
- Avoid compatibility issues with printers and viewers

Here’s a handy reference for common sizes:

| Name      | mm (WxH)        | in (WxH)      | Common Use         |
|-----------|-----------------|---------------|--------------------|
| A4        | 210 x 297       | 8.27 x 11.69  | Most world docs    |
| Letter    | 216 x 279       | 8.5 x 11      | US forms           |
| Legal     | 216 x 356       | 8.5 x 14      | Legal docs         |
| A3        | 297 x 420       | 11.69 x 16.54 | Posters, diagrams  |
| A5        | 148 x 210       | 5.83 x 8.27   | Flyers, booklets   |

---

## How Do I Set a Custom Paper Size for a PDF in C#?

If you need a very specific size (for example, for a thermal printer or a non-standard label), you can specify the dimensions in millimeters, inches, or points. Here’s how you can do each:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();

// In millimeters
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(80, 200);

// In inches
renderer.RenderingOptions.SetCustomPaperSizeInInches(3.5, 2);

// In points (72 points per inch)
renderer.RenderingOptions.SetCustomPaperSizeInPixelsOrPoints(252, 144);
```

**Tip:** Always check your printer’s documentation for supported sizes and margin requirements, as some printers are very particular.

---

## What Are Some Real-World Use Cases for Custom PDF Sizes?

IronPDF’s flexibility comes in handy for a variety of print scenarios. Here are a few practical examples:

### How Do I Generate Thermal Receipt PDFs?

Thermal printers often require narrow, long pages. Here’s how to set up an 80mm-wide, 300mm-long receipt:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(80, 300);

renderer.RenderingOptions.MarginTop = 3;
renderer.RenderingOptions.MarginBottom = 3;
renderer.RenderingOptions.MarginLeft = 3;
renderer.RenderingOptions.MarginRight = 3;

string receiptHtml = @"
<style>
    body { font-family: monospace; font-size: 11px; }
    .center { text-align: center; }
    hr { border-top: 1px dashed #333; }
</style>
<div class='center'>
    <h3>Pizza Place</h3>
    <p>789 Elm Road</p>
</div>
<hr>
<p>Large Pizza ...... $15.99</p>
<p>Soda ............ $1.99</p>
<hr>
<p><strong>Total: $17.98</strong></p>
<div class='center'><p>Thanks for your order!</p></div>
";

var pdf = renderer.RenderHtmlAsPdf(receiptHtml);
pdf.SaveAs("receipt.pdf");
```

If your receipts vary in length, consider dynamically setting the page height or generating multiple pages.

### How Can I Print Business Cards as PDFs?

Standard business cards are 3.5” x 2”. Here’s how to produce an exact-size card:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.SetCustomPaperSizeInInches(3.5, 2);

renderer.RenderingOptions.MarginTop = 3;
renderer.RenderingOptions.MarginBottom = 3;
renderer.RenderingOptions.MarginLeft = 3;
renderer.RenderingOptions.MarginRight = 3;

string cardHtml = @"
<style>
    body { font-family: 'Segoe UI', Arial; margin: 0; }
    .container {
        display: flex; flex-direction: column; align-items: center; justify-content: center;
        height: 100%; padding: 6px;
    }
    h2 { margin: 0; font-size: 16px; }
    p { margin: 2px 0; font-size: 10px; }
</style>
<div class='container'>
    <h2>Jane Developer</h2>
    <p>Full Stack Engineer</p>
    <p>jane@yourcompany.com</p>
    <p>+1 555 321 7654</p>
</div>
";

var pdf = renderer.RenderHtmlAsPdf(cardHtml);
pdf.SaveAs("business-card.pdf");
```

To print multiple cards on a single page, use a standard page size and CSS grid to layout several cards per sheet.

### How Do I Create Large Poster PDFs?

If you need to generate a poster, say 24” x 36”, it’s just as simple:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.SetCustomPaperSizeInInches(24, 36);

string posterHtml = @"
<style>
    body { 
        display: flex; align-items: center; justify-content: center;
        height: 100%; background: #fff;
    }
    h1 { font-size: 170px; color: #d32f2f; font-family: Impact, sans-serif; margin: 0; }
</style>
<h1>SALE!</h1>
";

var pdf = renderer.RenderHtmlAsPdf(posterHtml);
pdf.SaveAs("poster.pdf");
```

If you’re going really large, double-check with your print shop and test your PDF in multiple viewers.

### How Can I Print Multiple Labels on a Single Sheet?

To print a sheet of labels (such as 10-up on Letter paper), use CSS for layout and set the page size to Letter:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.MarginLeft = 10;
renderer.RenderingOptions.MarginRight = 10;

string labelSheetHtml = @"
<style>
    .labels { display: flex; flex-wrap: wrap; width: 100%; }
    .label {
        width: 180px; height: 90px;
        border: 1px solid #333; margin: 5px; padding: 6px;
        font-family: Arial; font-size: 12px;
        display: flex; align-items: center; justify-content: center;
    }
</style>
<div class='labels'>
    <div class='label'>Label 1</div>
    <div class='label'>Label 2</div>
    <div class='label'>Label 3</div>
    <div class='label'>Label 4</div>
    <div class='label'>Label 5</div>
    <div class='label'>Label 6</div>
    <div class='label'>Label 7</div>
    <div class='label'>Label 8</div>
    <div class='label'>Label 9</div>
    <div class='label'>Label 10</div>
</div>
";

var pdf = renderer.RenderHtmlAsPdf(labelSheetHtml);
pdf.SaveAs("label-sheet.pdf");
```

Be sure to match your CSS dimensions to your actual label stock for perfect alignment. For advanced label scenarios using web fonts or icons, you might also find [How do I use web fonts and icons in PDFs?](web-fonts-icons-pdf-csharp.md) helpful.

---

## How Can I Generate PDFs in Multiple Paper Sizes from the Same Source?

If you need to output the same content in various formats (like A4, Letter, and A5), automate it with a simple loop:

```csharp
using IronPdf;
// Install-Package IronPdf
using System.Collections.Generic;

string html = "<h1>Multi-size PDF</h1><p>This prints at different dimensions.</p>";

var sizes = new Dictionary<string, (double width, double height)>
{
    { "a4", (210, 297) },
    { "letter", (216, 279) },
    { "a5", (148, 210) }
};

foreach (var (name, (width, height)) in sizes)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(width, height);

    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"doc-{name}.pdf");
}
```

This is extremely useful for print portals or apps serving international customers.

---

## How Do I Control PDF Orientation (Portrait vs. Landscape)?

By default, if your width is greater than your height, IronPDF treats it as landscape. But you can set orientation explicitly:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();

// Set preset to A4 landscape
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

// Or manually set a custom landscape size
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(297, 210);
```

If you’re using custom sizes, the order of width and height determines orientation.

---

## How Can I Make My PDF Content Responsive to the Page Size?

Sometimes your HTML content doesn’t fit the page or needs to scale. IronPDF offers features to help with this:

### How Do I Scale Content to Fit the Paper?

Set the `FitToPaperMode` to ensure your content scales:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(100, 100);
renderer.RenderingOptions.FitToPaperMode = FitToPaperModes.Zoom;

var html = @"
<style>
    @page { size: 100mm 100mm; margin: 5mm; }
    .big-img { width: 200mm; }
</style>
<img class='big-img' src='https://via.placeholder.com/600x200' />
";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("scaled-content.pdf");
```

### How Do I Set the "Viewport Width" for Responsive HTML?

If your HTML uses responsive design or media queries, set the `ViewPortWidth` to control how IronPDF renders it:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(100, 150);
renderer.RenderingOptions.ViewPortWidth = 320;

string responsiveHtml = @"
<style>
    body { background: #eef; }
    @media (max-width: 400px) { body { background: #cfc; } }
</style>
<h1>This background changes if viewport is narrow!</h1>
";

var pdf = renderer.RenderHtmlAsPdf(responsiveHtml);
pdf.SaveAs("responsive.pdf");
```

Adjust `ViewPortWidth` to simulate different device widths.

---

## What Are Some Advanced Custom PDF Scenarios?

IronPDF can handle complex scenarios, such as dynamically sized receipts or overlaying content on certificate backgrounds.

### How Do I Create a Receipt with Dynamic Height?

To avoid wasted space, set an extra-tall page and adjust content or crop after rendering:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(80, 500);

string items = "";
for (int i = 1; i <= 20; i++)
    items += $"<p>Item {i} ..... ${i * 2:F2}</p>";

string html = $@"
<div class='center'><h3>Big Receipt</h3></div>
<hr>
{items}
<hr>
<p><strong>Total: $400.00</strong></p>
<div class='center'>Thank you!</div>
";

var pdfDoc = renderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("long-receipt.pdf");
```

For truly dynamic receipts, you might generate page heights based on content or split output into multiple pages.

### How Can I Overlay Content on a PDF Background Image?

Perfect for certificates or branded documents, just use CSS `background`:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.SetCustomPaperSizeInInches(8.5, 11);

string html = @"
<style>
    body {
        background: url('https://yourcdn.com/certificate-bg.png') no-repeat center center;
        background-size: cover;
        height: 100%;
        margin: 0;
    }
    .content {
        position: absolute;
        top: 40%;
        left: 10%;
        right: 10%;
        text-align: center;
        font-size: 28px;
        color: #222;
    }
</style>
<div class='content'>
    <h2>Certificate of Achievement</h2>
    <p>This certifies that</p>
    <h3>Alex Johnson</h3>
    <p>has completed the course</p>
    <h4>IronPDF for Developers</h4>
</div>
";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("certificate.pdf");
```

For more about using custom fonts or SVG icons in your PDFs, see [How do I use web fonts and icons in PDFs?](web-fonts-icons-pdf-csharp.md).

---

## What Common Mistakes Should I Avoid When Setting Custom PDF Sizes?

Here are a few frequent pitfalls:

- **Mixing up units**: Double-check whether you’re using mm, inches, or points.
- **Margins**: Setting margins too large can shrink your content or make it misaligned. Some printers enforce minimum margins.
- **Viewport mismatch**: If your content looks squished or stretched, experiment with `ViewPortWidth`.
- **Printer limits**: Not all printers handle custom sizes well—test your PDF in Adobe Acrobat or with your target printer.
- **Scaling issues**: Fit-to-page options may distort your design; for pixel-perfect output, size your HTML/CSS to match your paper size.
- **Excessive page height**: Don’t set extreme heights for receipts—most printers and viewers have upper bounds.
- **PDF/A compliance**: If you need PDF/A for archiving, be aware not all features (like images or certain fonts) are supported. See IronPDF documentation for details.
- **Font problems**: Use web fonts or embed with `@font-face` for reliable rendering. See [How do I use web fonts and icons in PDFs?](web-fonts-icons-pdf-csharp.md).

If you’re converting markup or social content, check out [How do I export Twitter posts to PDFs in .NET?](twitter-posts-pdfs-dotnet.md).

---

## Where Can I Learn More and Get Support?

For in-depth documentation and more advanced use cases, visit [IronPDF’s website](https://ironpdf.com). To learn about the team and the suite of .NET document tools, see [Iron Software](https://ironsoftware.com). For related conversion tasks and formatting questions, check out these guides:
- [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md)
- [How can I generate XAML to PDF in MAUI C#?](xaml-to-pdf-maui-csharp.md)
- [How do I work with web fonts and icons in PDFs?](web-fonts-icons-pdf-csharp.md)
- [How do I export Twitter posts to PDFs in .NET?](twitter-posts-pdfs-dotnet.md)
- [How do I convert HTML to PDF in C#?](html-to-pdf-csharp.md)

---

*About the author: [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) founded IronPDF and now serves as CTO at [Iron Software](https://ironsoftware.com), where he oversees the Iron Suite of .NET libraries.*
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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/) is CTO at Iron Software, leading a team of 50+ engineers building .NET libraries downloaded over 41+ million times. With 41 years of coding experience, Jacob is passionate about developer experience and API design. Based in Chiang Mai, Thailand.
