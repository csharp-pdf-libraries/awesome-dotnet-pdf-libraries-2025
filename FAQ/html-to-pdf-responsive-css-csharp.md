# How Can I Fix Responsive CSS Issues When Exporting HTML to PDF in C#?

Struggling to get your HTML and CSS to look professional in PDF exports from your C# app? You’re not alone. Many developers hit snags with responsive layouts, missing table headers, or “mobile view” PDFs. This FAQ will show you practical strategies to control CSS media types, set breakpoints, and get pixel-perfect results using IronPDF.

---

## Why Does Responsive CSS Break When Converting HTML to PDF?

Browsers and PDF engines process CSS media queries differently. By default, browsers use `screen` media, but PDF converters (and printers) rely on `print` media rules. If your CSS is only tuned for `screen`, your PDF may ignore key layout instructions—leading to collapsed columns, navigation menus in odd places, or missing table headers. Previewing your HTML in your browser’s print preview is a great way to spot these issues before generating the PDF.

For a step-by-step guide on basic HTML to PDF conversion, see [Convert HTML to PDF in C#](convert-html-to-pdf-csharp.md).

---

## How Do I Render HTML to PDF With the Correct CSS Media in C#?

To ensure your PDFs use the right CSS media types, set IronPDF to use `print` media. Here’s a quick example:

```csharp
using IronPdf; // Install-Package IronPdf

var pdfRenderer = new ChromePdfRenderer();
pdfRenderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

string htmlContent = @"
<!DOCTYPE html>
<html>
<head>
  <style>
    @media screen { nav { display: block; } }
    @media print { nav { display: none; } }
  </style>
</head>
<body>
  <nav>Main Menu</nav>
  <h1>PDF Export Example</h1>
</body>
</html>
";
var pdfDoc = pdfRenderer.RenderHtmlAsPdf(htmlContent);
pdfDoc.SaveAs("output.pdf");
```

This code hides the navigation menu in the PDF, while keeping it visible on screen. For more advanced scenarios, check out [Advanced HTML to PDF in C#](advanced-html-to-pdf-csharp.md).

---

## Which NuGet Package Should I Use for HTML-to-PDF in .NET?

You only need [IronPDF](https://ironpdf.com). Install it via NuGet:

```powershell
Install-Package IronPdf
```
or
```bash
dotnet add package IronPdf
```

IronPDF is powered by Chromium, so it supports the latest CSS features (like Grid and Flexbox) right out of the box. No extra dependencies necessary. Learn more about IronPDF at [Iron Software](https://ironsoftware.com).

---

## What’s the Difference Between Screen and Print Media When Exporting PDFs?

`Screen` media renders your HTML as you see it in a browser, including all navigation, sidebars, and even dark backgrounds. `Print` media, on the other hand, is optimized for clean, readable output—hiding navigation, ensuring table headers repeat, and converting colors for print clarity. For business documents, always set `CssMediaType.Print`.

For more on configuring media types, visit [Base URL HTML to PDF C#](base-url-html-to-pdf-csharp.md).

---

## How Should I Write CSS for Both Screen and PDF Outputs?

Use `@media` queries to target both screen and print outputs. Here’s a basic pattern:

```css
body { font-size: 12pt; }
@media screen {
  .sidebar { display: block; }
  .print-only { display: none; }
}
@media print {
  .sidebar { display: none; }
  .print-only { display: block; }
  a[href]:after { content: ' (' attr(href) ')'; }
}
```

This ensures your web page and PDF are both optimized. Embed this CSS directly in your HTML string or template. Anything inside `.print-only` will only appear in your PDF.

---

## How Can I Force Desktop Layouts Instead of Mobile in My PDFs?

PDF renderers treat the viewport differently than browsers, sometimes defaulting to widths that trigger your mobile CSS. To fix this, explicitly set the viewport width in IronPDF:

```csharp
pdfRenderer.RenderingOptions.ViewPortWidth = 1200; // Forces desktop layout
```

Set this property before rendering to ensure your PDF uses the desktop-responsive CSS. For complex layouts and further responsive strategies, see [Advanced HTML to PDF in C#](advanced-html-to-pdf-csharp.md).

---

## How Can I Make Table Headers and Footers Repeat on Each PDF Page?

To ensure table headers and footers repeat across pages, use semantic HTML (`<thead>`, `<tfoot>`) and add the following print CSS:

```css
@media print {
  table thead { display: table-header-group; }
  table tfoot { display: table-footer-group; }
}
```

This ensures every page in your PDF has column headers and totals where needed. For more details, see [Convert HTML to PDF in C#](convert-html-to-pdf-csharp.md).

---

## How Do I Control Page Breaks and Pagination in PDF Output?

Use these CSS rules to control where pages break:

```css
@media print {
  .section { page-break-after: always; }
  .avoid-break { page-break-inside: avoid; }
}
```

Apply `.section` to force a new page after a block, or `.avoid-break` to keep content together. Always preview in your browser’s print dialog to check results. For more on pagination and sanitizing output, see [Sanitize PDF C#](sanitize-pdf-csharp.md).

---

## Does IronPDF Support Modern CSS Like Grid, Flexbox, and Frameworks?

Yes! IronPDF’s Chromium engine means if your CSS works in Chrome, it works in IronPDF. This includes CSS Grid, Flexbox, modern selectors, and frameworks like Bootstrap or Tailwind. If you’re loading external CSS, make sure to set the `BaseUrl`—see [Base URL HTML to PDF C#](base-url-html-to-pdf-csharp.md).

---

## How Does IronPDF Compare to wkhtmltopdf for Professional Print Output?

IronPDF (Chromium-based) beats wkhtmltopdf (old WebKit) in CSS support. IronPDF properly handles print media, repeating headers, CSS Grid, Flexbox, and variables. Migrating from wkhtmltopdf? See [Migrate Syncfusion to IronPDF](migrate-syncfusion-to-ironpdf.md).

---

## What Are Common Pitfalls When Exporting Responsive HTML to PDF?

- **PDF shows mobile layout:** Set `ViewPortWidth` to a desktop size.
- **Navigation or dark backgrounds in PDF:** Use print media CSS to hide/show as needed.
- **Headers/footers missing:** Use `<thead>`, `<tfoot>`, and print CSS.
- **Weird page breaks:** Use `page-break-after` and `page-break-inside` in print CSS.
- **External styles not loading:** Set the correct `BaseUrl`.
- **Wrong media type:** Always explicitly set `CssMediaType.Print` for professional output.

For troubleshooting, check out [PDF rendering tips](https://ironpdf.com/java/how-to/print-pdf/).

---

## Where Can I Learn More About Responsive CSS for PDF in C#?

Dive deeper with the [IronPDF documentation](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/), or see more real-world responsive layouts in [Advanced HTML to PDF in C#](advanced-html-to-pdf-csharp.md).

---

*Questions? [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), CTO at Iron Software, is always happy to help. Drop a comment below or check out [IronPDF](https://ironpdf.com) for more tutorials.*
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

**Jacob Mellor** — Creator of IronPDF, CTO at Iron Software. 41 years coding, 50++ person team, 41+ million NuGet downloads. Created first .NET components in 2005. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
