# How Do I Migrate from wkhtmltopdf to IronPDF in C#?

If you’re still using wkhtmltopdf in your .NET projects, you’re risking security and reliability. Since wkhtmltopdf was archived in early 2023, it’s become outdated and unsupported. This FAQ walks you through why and how to migrate to IronPDF—a modern, actively maintained .NET library for HTML-to-PDF conversion—with practical steps and examples.

---

## Why Is wkhtmltopdf Considered Obsolete and Unsafe?

wkhtmltopdf was once the go-to tool for HTML-to-PDF conversion, but it’s fallen far behind. The last release was back in June 2020, and the project was officially archived in January 2023. It relies on an outdated QtWebKit engine from 2015, so it lacks support for modern CSS (like Flexbox and Grid), advanced JavaScript, and HTML5 features. Worse, it no longer receives bug fixes or security updates, leaving your applications exposed—especially dangerous in regulated industries or public-facing apps.

## Why Should I Choose IronPDF Over wkhtmltopdf?

IronPDF is built for today’s web and .NET stack. Here’s what sets it apart:

- **Chromium Rendering:** IronPDF uses a real Chromium engine, so it handles modern HTML, CSS, JavaScript, and web fonts just like Chrome. Features like Flexbox, Grid, or dynamic JS apps render accurately.
- **No External Processes:** Unlike wkhtmltopdf, which spawns external processes and requires managing separate binaries, IronPDF is a pure .NET library. Just add the NuGet package—no external tools needed.
- **Cross-Platform Support:** IronPDF works out of the box on Windows, Linux (including Docker), and macOS—no more platform-specific hacks.
- **Active Maintenance:** Frequent updates, .NET 8+ support, and responsive commercial support help ensure your project remains future-proof.

If you’re considering migrating from other PDF libraries, you might also want to check out [how to migrate from Telerik to IronPDF](migrate-telerik-to-ironpdf.md), [Syncfusion to IronPDF](migrate-syncfusion-to-ironpdf.md), or [QuestPDF to IronPDF](migrate-questpdf-to-ironpdf.md).

### Can IronPDF Handle Modern CSS Layouts Like Flexbox or Grid?

Yes, IronPDF fully supports modern web standards. For example, Flexbox layouts render just as they do in Chrome—something wkhtmltopdf can’t do. Here’s a simple code snippet:

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<div style='display: flex; gap: 1em;'>
    <div style='background: #e0e0e0;'>Left</div>
    <div style='background: #b0b0b0;'>Right</div>
</div>";

var pdfRenderer = new ChromePdfRenderer();
var doc = pdfRenderer.RenderHtmlAsPdf(html);
doc.SaveAs("flexbox-demo.pdf");
```

## How Do I Replace Command-Line Calls to wkhtmltopdf with IronPDF?

With wkhtmltopdf, you probably used Process.Start to run the binary. IronPDF replaces this with a managed API, making your code cleaner and less error-prone.

**Old Approach (wkhtmltopdf):**
```csharp
using System.Diagnostics;
// ... process setup code omitted for brevity ...
```

**New Approach (IronPDF):**
```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var doc = renderer.RenderHtmlFileAsPdf("input.html");
doc.SaveAs("output.pdf");
```
No more fiddling with temp files or process monitoring.

## How Can I Convert In-Memory HTML Strings Directly to PDF?

IronPDF lets you convert raw HTML strings straight to PDF, skipping the need to write temporary files—unlike typical wkhtmltopdf workflows.

```csharp
using IronPdf; // Install-Package IronPdf

var html = "<h2>Welcome to IronPDF!</h2>";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("welcome.pdf");
```

## How Do I Map wkhtmltopdf Command-Line Options to IronPDF?

Instead of mapping dozens of command-line flags, IronPDF provides strongly-typed properties in the API. For example, to set paper size, orientation, and margins:

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 20;

var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("styled.pdf");
```

Custom sizes are easy too:
```csharp
renderer.RenderingOptions.PaperSize = new PdfPaperSize(210, 297); // mm for A4
```

## How Do I Add Advanced Headers and Footers with IronPDF?

Headers and footers in IronPDF are full HTML fragments, so you can use any styles, images, or tokens you need.

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center;'>Confidential</div>"
};
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<span>Page {page} of {total-pages}</span>"
};

var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("header-footer.pdf");
```

Dynamic tokens like `{page}` are replaced at render time. For more on advanced PDF drawing (such as lines or rectangles), see [How do I draw lines and rectangles in a PDF with C#?](draw-line-rectangle-pdf-csharp.md)

## Does IronPDF Support Full JavaScript Execution Before Rendering?

Yes—IronPDF executes client-side JavaScript and waits for the page to be ready. Set a render delay or wait for a specific DOM element for SPAs:

```csharp
using IronPdf;

var html = @"
<div id='main'>Loading...</div>
<script>
  setTimeout(() => {
    document.getElementById('main').innerHTML = 'Loaded!';
  }, 500);
</script>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.RenderDelay = 1000;

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("js-rendered.pdf");
```

## How Can I Migrate from C# wkhtmltopdf Wrappers Like DinkToPDF, WkHtmlToPdfDotNet, or Rotativa?

You can swap out wrapper-specific code for IronPDF’s API with minimal effort.

**Example: Migrating from DinkToPDF**
```csharp
using IronPdf;
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("output.pdf");
```

**Example: Replacing Rotativa in ASP.NET MVC**
```csharp
public ActionResult Invoice()
{
    var html = RenderViewToString("InvoiceView", model);
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    return File(pdf.BinaryData, "application/pdf", "invoice.pdf");
}
```
If you’re migrating from other engines, see [How do I migrate from QuestPDF to IronPDF?](migrate-questpdf-to-ironpdf.md)

## What Advanced PDF Features Does IronPDF Offer?

IronPDF is more than just HTML-to-PDF:

- **Merge & Split PDFs:** Combine or separate documents easily.
- **Watermarking:** Add text or image watermarks ([see guide](https://ironpdf.com/java/blog/using-ironpdf-for-java/java-watermark-pdf-tutorial/)).
- **Form Filling:** Programmatically fill out PDF forms (AcroForms).
- **Text and Image Extraction:** Read content and images from existing PDFs.
- **Security:** Add passwords and restrict actions like printing or copying.

**Example: Merging PDFs and Adding a Watermark**
```csharp
using IronPdf;

var doc1 = PdfDocument.FromFile("file1.pdf");
var doc2 = PdfDocument.FromFile("file2.pdf");
doc1.Merge(doc2);
doc1.StampText("Confidential", x: 50, y: 100, color: PdfColor.Red, size: 36);
doc1.SaveAs("merged-watermarked.pdf");
```

For more advanced tips, see the [IronPDF documentation](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/).

## What Are Common Migration Pitfalls and How Do I Fix Them?

- **Missing Fonts:** On Linux, install `libgdiplus` and ensure fonts are present. On Windows, make sure your fonts are installed server-side.
- **JavaScript Not Finishing:** Increase `RenderDelay` or use `WaitForElements` to ensure all scripts complete before rendering.
- **Broken Images:** Verify URLs are accessible; for local images, use absolute paths.
- **Docker Issues:** Install required dependencies (`libgdiplus`, `libx11-dev`, etc.) in your Dockerfile.
- **Page Breaks or Layout Issues:** Set paper size and margins explicitly. For more, see [How do I control HTML-to-PDF page breaks?](html-to-pdf-page-breaks-csharp.md)
- **Large PDF Sizes:** Optimize images and limit font usage; use compression options if needed.

## How Does IronPDF Perform Compared to wkhtmltopdf?

IronPDF avoids spawning new processes for each conversion, making it significantly faster—especially for bulk operations or API servers. In real-world benchmarks, IronPDF can be up to twice as fast as wkhtmltopdf for converting batches of documents.

## What About Licensing and Commercial Use?

wkhtmltopdf is technically “free,” but maintenance and security issues cost you in the long run. IronPDF is a commercial product (starting at $749 per developer), which includes ongoing updates, support, and security patches. For most teams, the savings in development and risk far outweigh the licensing cost.

## What’s a Good Checklist for Migrating to IronPDF?

1. Audit your current wkhtmltopdf usage (including wrappers).
2. Install IronPDF via NuGet.
3. Replace process calls with the IronPDF API.
4. Map command-line options to strongly-typed properties.
5. Migrate headers/footers to the HTML-based approach.
6. Test thoroughly—especially dynamic or framework-heavy HTML.
7. Remove old wkhtmltopdf binaries and dependencies.
8. Update Docker/Kubernetes configs as needed.
9. Performance test under load.
10. Sort out IronPDF licensing for production.
11. Educate your team on the new workflow.

---

Ready to modernize your PDF conversion? Download IronPDF from [ironpdf.com](https://ironpdf.com) or get started with [Iron Software](https://ironsoftware.com).
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
