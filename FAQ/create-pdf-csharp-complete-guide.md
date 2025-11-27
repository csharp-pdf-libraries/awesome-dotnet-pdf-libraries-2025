# How Can I Generate PDFs in C#? The Ultimate .NET PDF FAQ for 2024

If you're developing business apps with .NET, chances are you'll need to create PDFs—whether for invoices, reports, forms, or labels. Generating PDFs in C# can be surprisingly tricky, but with the right approach, you can have production-ready code in minutes. This FAQ covers everything you need: library selection, HTML-to-PDF strategies, code examples, troubleshooting, and best practices for .NET 6/7/8+.

---

## Why Is Generating PDFs in C# So Complicated?

.NET is great for building APIs, web apps, and desktop tools, but it doesn't include built-in PDF generation support—not in .NET Core, .NET 6+, or even in older .NET Framework versions. That means you need to rely on a third-party PDF library. The challenge? Not all libraries are created equal. You need to watch out for:

- Support for modern HTML/CSS (so your PDFs look professional)
- Licensing issues (to avoid unexpected legal trouble)
- Maintenance and updates (so you’re not stuck with abandoned tech)
- Performance and scalability (especially for batch jobs)

The right library will save you massive headaches as your project grows.

---

## What Should I Look for When Choosing a C# PDF Library?

### How Have PDF Libraries for .NET Evolved Over Time?

Here's a quick rundown of popular options:

- **iTextSharp:** One of the oldest and most feature-rich, but switched to AGPL licensing. This means you must buy a commercial license for closed-source projects, or you'll have to open source your own code. Many developers have been caught out—be cautious!
- **wkhtmltopdf:** Well-loved for HTML-to-PDF, but uses an ancient WebKit browser engine (from 2015). Modern CSS often breaks, and it’s no longer actively maintained, making it risky for new projects.
- **PDFSharp:** Open source (MIT license), but lacks HTML-to-PDF support—you’ll be drawing text and shapes by X/Y coordinates, which is only practical for very simple documents.
- **IronPDF:** Commercial library with simple licensing, up-to-date Chromium rendering (so, Chrome-level HTML/CSS/JS support), and native .NET integration. No external processes or wrappers—just drop it in via NuGet and go.

For most business apps, IronPDF is the sweet spot. For a practical guide to creating PDFs directly in C#, check out [How do I create a PDF in C#?](create-pdf-csharp.md).

### How Can I Avoid PDF Licensing Pitfalls?

Short answer: **Don’t use AGPL libraries in commercial apps unless you’re ready for legal hassles.** MIT and Apache licenses are safe, but might come with limited features. Commercial libraries like IronPDF are worth considering for peace of mind and support.

### Why Is Modern HTML/CSS Support Important for PDFs?

Libraries that rely on a modern browser engine (like Chromium) can render your PDFs to match your web app, including Flexbox, Google Fonts, SVG, and even JavaScript-driven charts. This is why tools like IronPDF are a game-changer compared to older libraries.

---

## What Are the Two Main Ways to Generate PDFs in C#?

### When Should I Use Programmatic (Code-First) PDF Construction?

With programmatic construction, you build PDFs by specifying every element’s position in code:

- **Best for:** Simple, fixed layouts (like shipping labels, barcodes, or forms)
- **Drawbacks:** Any layout change means tweaking X/Y coordinates. Complex layouts and tables are difficult, and designers can't help without editing code.

**Example using PDFSharp:**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;
// Install-Package PdfSharp

var doc = new PdfDocument();
var page = doc.AddPage();
var gfx = XGraphics.FromPdfPage(page);
var font = new XFont("Arial", 12);

gfx.DrawString("Invoice #1001", font, XBrushes.Black, new XPoint(100, 100));
gfx.DrawString("Total: $456.78", font, XBrushes.Black, new XPoint(100, 120));

doc.Save("invoice.pdf");
```
For more on programmatic PDF creation, see [How do I create a PDF in C#?](create-pdf-csharp.md)

### What Is HTML-to-PDF Conversion and Why Is It Preferred?

HTML-to-PDF lets you use HTML/CSS templates (created by designers!) and inject data at runtime. The PDF library renders the HTML to a PDF—just like printing a web page.

- **Best for:** Invoices, reports, contracts—any document with structured layouts
- **Advantages:** Designers can edit templates. Developers can inject data with string replacement or templating engines. Changes are fast and low-risk.

**Example with IronPDF:**
```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();
string htmlTemplate = File.ReadAllText("invoice-template.html")
    .Replace("{{NUMBER}}", "INV-2024-01")
    .Replace("{{CUSTOMER}}", "Iron Software")
    .Replace("{{AMOUNT}}", "$5000.00");

var pdfDoc = renderer.RenderHtmlAsPdf(htmlTemplate);
pdfDoc.SaveAs("invoice.pdf");
```
For more about templating and PDF forms, see [How do I create PDF forms in C#?](create-pdf-forms-csharp.md)

---

## How Do I Install and Set Up IronPDF in My Project?

IronPDF is easy to install via NuGet—no extra dependencies or setup required.

```bash
# .NET CLI
dotnet add package IronPdf

# Or in Visual Studio:
Install-Package IronPdf
```

**Testing your setup:**
```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h2>Test PDF</h2>");
pdf.SaveAs("test.pdf");
```
If "test.pdf" opens with your heading, you're ready to go.

---

## How Do I Build Real-World PDFs from Templates and Data?

### How Can I Convert Static HTML to a PDF in C#?

For quick, static documents, you can plug HTML directly into IronPDF:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();
string html = @"
<html>
<head>
  <style>body { font-family: Arial; margin: 30px; }</style>
</head>
<body>
  <h1>Hello from IronPDF!</h1>
  <p>This PDF was generated in .NET 8.</p>
</body>
</html>
";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("hello.pdf");
```
For more on HTML-to-PDF and pagination, check out [How do I control PDF pagination?](https://ironpdf.com/how-to/html-to-pdf-page-breaks/)

### How Do I Generate Dynamic PDFs, Like Invoices, Using Templates?

Suppose you want to generate an invoice with line items from a template:

**Sample invoice template (`invoice-template.html`):**
```html
<html>
<body>
  <h2>Invoice #{{NUMBER}}</h2>
  <p>Customer: {{CUSTOMER}}</p>
  <table>
    <tbody>
      {{ITEMS}}
    </tbody>
  </table>
  <p>Total: {{TOTAL}}</p>
</body>
</html>
```

**C# code to generate dynamic rows:**
```csharp
using IronPdf;
using System.Text;

var invoice = new {
  Number = "INV-2002",
  Customer = "Acme Corp",
  Items = new[] {
    new { Desc = "Service A", Qty = 2, Price = 30.0 },
    new { Desc = "Service B", Qty = 3, Price = 20.0 }
  }
};

var template = File.ReadAllText("invoice-template.html");
var itemsRows = new StringBuilder();
foreach (var item in invoice.Items)
{
    itemsRows.AppendLine(
        $"<tr><td>{item.Desc}</td><td>{item.Qty}</td><td>{item.Price:C}</td></tr>"
    );
}

string html = template
    .Replace("{{NUMBER}}", invoice.Number)
    .Replace("{{CUSTOMER}}", invoice.Customer)
    .Replace("{{ITEMS}}", itemsRows.ToString())
    .Replace("{{TOTAL}}", invoice.Items.Sum(i => i.Qty * i.Price).ToString("C"));

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs($"Invoice-{invoice.Number}.pdf");
```

### Can I Use a Real Templating Engine for Conditional Logic and Loops?

Yes! For complex templates, use a C# templating engine like Handlebars.NET, RazorLight, or Scriban.

**Handlebars.NET example:**
```csharp
using IronPdf;
using HandlebarsDotNet;

var src = File.ReadAllText("invoice-template.html");
var template = Handlebars.Compile(src);

var model = new {
    NUMBER = "INV-301",
    CUSTOMER = "Beta Inc.",
    ITEMS = new[] { new { Desc="Item 1", Qty=1, Price=50.0 } },
    TOTAL = 50.0
};

string html = template(model);

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice-handlebars.pdf");
```
Handlebars supports `{{#each}}` for loops and `{{#if}}` for conditional sections.

---

## How Do I Add CSS, Images, and Assets to My PDFs?

IronPDF supports external and inline CSS, images, and even web fonts. For reliable asset loading:

- Use absolute paths or `file:///` URLs for images.
- Embed images as base64 to avoid path issues.

**Embed an image with base64:**
```csharp
string logoPath = "logo.png";
string logoBase64 = Convert.ToBase64String(File.ReadAllBytes(logoPath));
html = html.Replace("{{LOGO_BASE64}}", logoBase64);
```
**Template usage:**
```html
<img src="data:image/png;base64,{{LOGO_BASE64}}" alt="Logo" />
```
IronPDF also supports JavaScript execution before rendering, so you can include charts and dynamic content.

For in-depth DOM access, see [How do I access the PDF DOM in C#?](access-pdf-dom-object-csharp.md)

---

## What Advanced PDF Features Can I Use in C#?

### How Can I Add Headers, Footers, or Page Numbers to PDFs?

IronPDF makes it easy to add consistent headers/footers and dynamic placeholders like page numbers.

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new IronPdf.Rendering.HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center;'>Header Content</div>",
    DrawDividerLine = true
};

renderer.RenderingOptions.HtmlFooter = new IronPdf.Rendering.HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
};

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("header-footer.pdf");
```
For more on [page numbers](https://ironpdf.com/blog/compare-to-other-components/questpdf-add-page-number-to-pdf/)

### How Do I Control Page Size, Margins, and Orientation?

You can set standard or custom sizes, orientation, and margins:

```csharp
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 20; // mm
renderer.RenderingOptions.MarginBottom = 15;
```
Custom sizes and margins work great for labels or receipts.

### How Do I Add Watermarks or Backgrounds to a PDF?

For stamps like "DRAFT" or company letterheads, IronPDF supports watermarks and backgrounds.

**Text watermark:**
```csharp
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.ApplyWatermark("<div style='font-size:60px; opacity:0.15;'>DRAFT</div>");
pdf.SaveAs("watermarked.pdf");
```
See the [Watermarks guide](https://ironpdf.com/how-to/rasterize-pdf-to-images/) for more.

**Background PDF:**  
```csharp
var letterhead = PdfDocument.FromFile("letterhead.pdf");
pdf.AddBackgroundPdf(letterhead);
pdf.SaveAs("with-letterhead.pdf");
```

### How Do I Generate PDF/A for Archiving?

Enable PDF/A compliance (for legal or archival requirements) like this:

```csharp
renderer.RenderingOptions.PdfACompliant = true;
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("archived.pdf");
```
All fonts are embedded, encryption is disabled, and long-term readability is ensured.

### How Can I Merge or Split PDFs in C#?

Merging multiple PDFs is simple:

```csharp
var doc1 = PdfDocument.FromFile("file1.pdf");
var doc2 = PdfDocument.FromFile("file2.pdf");
doc1.Merge(doc2);
doc1.SaveAs("merged.pdf");
```
You can also add blank pages or split PDFs as needed. For more, check [How do I merge and split PDFs in C#?](merge-split-pdf-csharp.md)

### What About Batch PDF Generation and Performance?

For high-volume generation, use IronPDF's async APIs and process documents in manageable batches. For example:

```csharp
var renderer = new ChromePdfRenderer();
var tasks = invoices.Select(inv => renderer.RenderHtmlAsPdfAsync(GenerateHtml(inv))).ToArray();
var pdfs = await Task.WhenAll(tasks);

for (int i = 0; i < pdfs.Length; i++)
{
    pdfs[i].SaveAs($"Invoice-{invoices[i].Number}.pdf");
    pdfs[i].Dispose();
}
```
Dispose PDFs after saving to avoid memory leaks, and consider chunking large jobs.

---

## What Are the Most Common Issues When Generating PDFs in C#?

**1. Images Missing:** Check paths, use absolute or base64 URIs.
**2. CSS Not Rendering:** Inline CSS for reliability; avoid external CDNs for critical styles.
**3. Fonts Not Embedded:** Use `@font-face` and check PDF/A requirements.
**4. JavaScript Not Running:** Only pre-render JS is supported; no user interactivity.
**5. Watermarks or Licensing Warnings:** Make sure you have a valid IronPDF license for production.
**6. Large PDFs:** Compress or optimize images, use SVGs for logos.
**7. Slow Rendering:** Split very big docs into smaller PDFs.
**8. Deployment Issues:** IronPDF is self-contained, but double-check .NET compatibility, especially on Linux.
**9. Thread Safety:** Each `ChromePdfRenderer` is thread-safe—never share `PdfDocument` across threads.
**10. PDF Merging Fails:** Ensure all source PDFs are valid and not corrupted or encrypted.

For more troubleshooting or advanced DOM access, see [How do I access the PDF DOM in C#?](access-pdf-dom-object-csharp.md)

---

## What Are the Best Practices for PDF Generation in C#?

- **Favor HTML-to-PDF for complex layouts:** It’s easier to maintain and lets designers contribute.
- **Separate templates and data:** Store templates in files, fill with data at runtime, and use a template engine for logic.
- **Check library licensing early:** IronPDF has clear, business-friendly licensing and strong support from [Iron Software](https://ironsoftware.com).
- **Leverage async batch processing:** Use async APIs and chunk large jobs for speed and stability.
- **Use PDF/A when required:** Especially for regulated industries.
- **Make use of headers, footers, and watermarks:** For professional, branded output.
- **Stay up to date:** Keep an eye on new features, like PDF forms and digital signatures. For comparisons with other languages, see [How does PDF generation in C# compare to Python?](compare-csharp-to-python.md)

For more tips and advanced topics (like forms), check [How do I create PDF forms in C#?](create-pdf-forms-csharp.md).

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

**About the Author**: Jacob Mellor is Chief Technology Officer at Iron Software, where PDF tools are used by NASA and other Fortune 500 companies. With expertise in C++, .NET, Rust, Jacob focuses on making PDF generation accessible to all .NET developers. [Author Page](https://ironsoftware.com/about-us/authors/jacobmellor/)
