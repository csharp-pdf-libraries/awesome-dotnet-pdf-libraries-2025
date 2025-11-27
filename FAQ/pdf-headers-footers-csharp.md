# How Do I Add Headers and Footers to PDFs in C# Using IronPDF?

Adding headers and footers can take your PDFs from bland to boardroom-ready. With IronPDF—a powerful .NET PDF library from [Iron Software](https://ironsoftware.com)—you can easily add everything from page numbers to custom HTML layouts and logos. This FAQ explains how to create professional headers and footers in C#, with practical code, tips, and solutions to common problems.

---
## Why Should I Use Headers and Footers in My PDFs?

Headers and footers help make your PDFs look polished and organized. They’re essential for:
- Page numbering (especially in multi-page reports)
- Branding with company names or logos
- Adding generation dates/timestamps
- Inserting legal disclaimers or confidentiality notices
- Creating alternating or section-specific headers (like books or appendices)

IronPDF lets you go beyond plain text—you can use HTML and CSS for advanced layouts and visuals.

---
## How Do I Quickly Add a Simple Header and Footer with IronPDF?

Here’s a straightforward example: add a centered title at the top, and page numbers at the bottom of every page.

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    CenterText = "Financial Report",
    FontSize = 16,
    FontFamily = "Segoe UI"
};

renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Page {page} of {total-pages}",
    FontSize = 12
};

var pdfDoc = renderer.RenderHtmlAsPdf("<h1>PDF content goes here</h1>");
pdfDoc.SaveAs("simple-header-footer.pdf");
```
**Tip:** Use placeholders like `{page}` and `{total-pages}`—IronPDF replaces these with the actual page numbers.

---
## Should I Use Text or HTML Headers/Footers? What’s the Difference?

IronPDF supports both simple text and rich HTML/CSS headers and footers:

| Type  | Speed  | Customization      | Best For                       |
|-------|--------|-------------------|--------------------------------|
| Text  | Fast   | Basic (font, size, align) | Quick info, page numbers      |
| HTML  | Slower | All HTML/CSS, images | Logos, branding, complex layouts |

**Choose text headers/footers** for basic tasks like page numbers or short notices.  
**Switch to HTML** when you want images, advanced layout, or styling with CSS.  
You can even use a mix—text in the header, HTML in the footer, or vice versa.

---
## What Dynamic Placeholders Can I Use in IronPDF Headers and Footers?

IronPDF provides several dynamic placeholders:

| Placeholder    | Replaced With                      |
|----------------|------------------------------------|
| `{page}`       | The current page number            |
| `{total-pages}`| The total number of pages          |
| `{date}`       | The current date                   |
| `{time}`       | The current time                   |
| `{title}`      | The document title                 |
| `{url}`        | Source URL (if rendering from URL) |

**Example:** Add a timestamp and page numbers in your footer.

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    LeftText = "Created: {date} {time}",
    RightText = "Page {page} of {total-pages}",
    FontSize = 10
};

var pdf = renderer.RenderHtmlAsPdf("<p>Document body here.</p>");
pdf.SaveAs("footer-timestamp.pdf");
```

*Need to customize date/time format? Use an HTML header/footer and insert formatted C# values instead.*

---
## How Can I Create Advanced HTML Headers and Footers (With Images or Custom Layouts)?

For more sophisticated designs, use the `HtmlHeader` and `HtmlFooter` options. You can include logos, colors, and styled text.

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
      <div style='display: flex; align-items: center; justify-content: space-between; width: 100%; font-family: Arial;'>
        <img src='logo.png' style='height: 36px;' alt='Logo'/>
        <span style='font-size: 18px; color: #333;'>Executive Summary</span>
      </div>",
    DrawDividerLine = true,
    DividerLineColor = "#0078D7"
};

var pdf = renderer.RenderHtmlAsPdf("<p>Main content here.</p>");
pdf.SaveAs("header-html.pdf");
```
**Note:** IronPDF supports most HTML and CSS, and even JavaScript for dynamic content (but keep it simple for speed).

---
## How Do I Make Sure Images in My Headers/Footers Always Show Up?

To guarantee your images (like logos or QR codes) always appear, embed them using Base64 encoding:

```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();
var logoBytes = File.ReadAllBytes("logo.png");
var base64Logo = Convert.ToBase64String(logoBytes);

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = $@"
      <div style='display: flex; align-items: center;'>
        <img src='data:image/png;base64,{base64Logo}' style='height: 32px;' alt='Logo'/>
        <span style='font-weight: bold; font-size: 20px;'>My Company</span>
      </div>",
    MaxHeight = 40
};

var pdf = renderer.RenderHtmlAsPdf("<div>Body content here.</div>");
pdf.SaveAs("header-base64-logo.pdf");
```
**Why use Base64?** No worries about missing files or bad paths—perfect for cloud or Docker.

---
## How Can I Adjust the Height of My Header or Footer?

Use the `MaxHeight` property (in millimeters) to set the header/footer height.

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<img src='banner.png' style='width:100%;' />",
    MaxHeight = 50
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<p style='text-align:center;'>Page {page}</p>",
    MaxHeight = 18
};
```
**Reminder:** Test with real data—oversized headers/footers can crowd your main content.

---
## How Do I Add Divider Lines Between Content and Headers/Footers?

Add a divider with `DrawDividerLine` or by using a CSS border/`<hr>` tag.

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<h2>Handbook</h2>",
    DrawDividerLine = true,
    DividerLineColor = "#e5e5e5"
};
```
You can also add custom lines directly in your HTML using `<hr>` or styled `<div>` elements for more control.

---
## Can I Style Headers and Footers with CSS?

Absolutely! Inline styles, `<style>` blocks, and even custom fonts are supported in HTML headers/footers.

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
      <style>
        .header-main { font-family: 'Georgia', serif; color: #2c3e50; }
      </style>
      <div class='header-main'>
        <span>Annual Report 2024</span> <span style='font-size:13px;'>{date}</span>
      </div>"
};
```
**Note:** For external fonts, ensure they’re accessible during PDF generation.

---
## How Do I Add Headers or Footers to Existing PDFs?

IronPDF supports updating PDFs post-creation:

```csharp
using IronPdf;

var pdfDoc = new PdfDocument("existing.pdf");

pdfDoc.AddTextHeaders(new TextHeaderFooter
{
    CenterText = "CONFIDENTIAL",
    FontSize = 14,
    FontFamily = "Arial"
});

pdfDoc.AddHtmlFooters(new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center;'>Updated 2024</div>"
});

pdfDoc.SaveAs("updated.pdf");
```
Great for compliance workflows or batch watermarking.

---
## How Can I Target Specific Pages or Sections with Different Headers/Footers?

You can apply headers/footers to specific pages using the `pageIndices` parameter:

```csharp
using System.Linq;
using IronPdf;

var pdf = new PdfDocument("multi.pdf");

// Header on pages 2-10 (zero-based)
pdf.AddTextHeaders(
    new TextHeaderFooter { CenterText = "Main Section" },
    pageIndices: Enumerable.Range(1, 9).ToArray()
);

// Footer on appendix pages (pages 11-13)
pdf.AddTextFooters(
    new TextHeaderFooter { CenterText = "Appendix - Page {page}" },
    pageIndices: new[] { 10, 11, 12 }
);
```
Page indices start at zero.

---
## How Do I Set Different Headers for Odd and Even Pages?

Perfect for book layouts or academic papers:

```csharp
using System.Linq;
using IronPdf;

var pdf = new PdfDocument("book.pdf");
var pageCount = pdf.PageCount;

var oddPages = Enumerable.Range(0, pageCount).Where(i => i % 2 == 0).ToArray();
pdf.AddTextHeaders(
    new TextHeaderFooter { RightText = "Book Title" },
    pageIndices: oddPages
);

var evenPages = Enumerable.Range(0, pageCount).Where(i => i % 2 == 1).ToArray();
pdf.AddTextHeaders(
    new TextHeaderFooter { LeftText = "Chapter 1" },
    pageIndices: evenPages
);
```
Mix odd/even headers and footers as needed.

---
## Can I Use HTML Footers for Legal Notices or Contact Info?

Yes! HTML footers are perfect for legal text, email links, or styled layouts.

```csharp
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
      <div style='font-size: 10px; text-align: center; color: #555;'>
        © 2024 ExampleCorp | <a href='mailto:info@example.com'>info@example.com</a> | Page {page} of {total-pages}
      </div>",
    DrawDividerLine = true
};
```
Links are usually clickable in most PDF viewers.

---
## What Are Some Real-World Patterns or Recipes for Headers and Footers?

- **Multi-language headers:** Set header text based on user locale.
- **Conditional footers:** Show “DRAFT” only on unpublished docs.
- **Watermarks:** Use HTML with opacity for faint “CONFIDENTIAL” marks.
- **Barcodes/QR codes:** Generate as Base64 images and embed in the header.
- **Dynamic data:** Use string interpolation to insert usernames, dates, or custom values.

For more about handling dynamic document content, check out [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md) and [How do I use XAML to PDF in .NET MAUI?](xaml-to-pdf-maui-csharp.md).

---
## What Common Issues Should I Watch Out For?

- **Image paths failing?** Use Base64 images for reliability.
- **Overlapping content:** Ensure `MaxHeight` is set correctly and test with dense documents.
- **Fonts don’t render right?** System fonts only for text headers; use web fonts with HTML and check network access.
- **Placeholders not replaced?** Double-check spelling and context.
- **Slow rendering:** Keep HTML/CSS in headers and footers lightweight.
- **Different page sizes:** Test headers/footers on all page types.
- **Artifact issues in PDF viewers:** Stick to standard CSS for best compatibility.
- **Encrypted PDFs:** You need to unlock the PDF before editing.

If you’re working with HTTP headers in PDF generation, see [How do I set HTTP request headers for PDF in C#?](http-request-headers-pdf-csharp.md).  
Interested in more about IronPDF’s capabilities? See [What is IronPDF and what can it do?](what-is-ironpdf-overview.md).

---
## Where Can I Learn More About IronPDF?

Check the [official IronPDF site](https://ironpdf.com) for full documentation, advanced features, and enterprise solutions. For more programming tricks, see related FAQs—including [How do I wait 5 seconds using JavaScript?](javascript-wait-5-seconds.md) if your workflow involves JavaScript delays in PDF rendering.

---

*This FAQ was written by [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), CTO at Iron Software. Got a question that's not covered here? Drop it in the comments.*
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

**Jacob Mellor** — Creator of IronPDF, CTO at Iron Software. 41 years coding, 50++ person team, 41+ million NuGet downloads. Civil Engineering degree holder turned software pioneer. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
