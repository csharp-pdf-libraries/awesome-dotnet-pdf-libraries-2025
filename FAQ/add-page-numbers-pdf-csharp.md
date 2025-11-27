# How Can I Add Page Numbers to PDFs in C# Using IronPDF?

Adding page numbers to your PDFs in .NET doesn't have to be complex. With IronPDF, you can quickly insert styled page numbers into new or existing PDF documents—no coordinate math or event handlers required. This FAQ covers real-world scenarios and code examples for numbering pages, skipping select pages, styling, and more.

---

## Why Should I Add Page Numbers to My PDFs?

Page numbers are essential for navigation, referencing, and maintaining professionalism in any PDF document. They let users cite content, jump to sections, and quickly gauge document length. In my experience, features like this dramatically improve usability for contracts, invoices, and technical documents—and save both users and support teams a lot of time.

---

## What’s the Typical Challenge With Page Numbering in C#?

Many C# PDF libraries (like iTextSharp) require subclassing and manual coordinate math to add page numbers, which can be error-prone and awkward to maintain. For example, using `PdfPageEventHelper` in iTextSharp involves complex overrides and can easily break if your document structure changes. IronPDF, on the other hand, uses placeholders and simple APIs, making the process much more straightforward.

---

## How Do I Get Started With IronPDF for Page Numbers?

First, install the IronPDF NuGet package:

```powershell
Install-Package IronPdf
```
Or with the .NET CLI:
```bash
dotnet add package IronPdf
```

Then, create a PDF with auto-numbered footers using placeholders:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var doc = renderer.RenderHtmlAsPdf(@"
    <h1>Sample Report</h1>
    <p>This is page 1.</p>
    <div style='page-break-after: always;'></div>
    <h2>Section 2</h2>
    <p>This is page 2.</p>
");

var footer = new TextHeaderFooter
{
    CenterText = "Page {page} of {total-pages}",
    FontSize = 10
};

doc.AddTextFooters(footer);
doc.SaveAs("numbered-report.pdf");
```

The `{page}` and `{total-pages}` placeholders are replaced automatically—no need for extra calculations. For more quick code samples, see [IronPDF Cookbook: Quick Examples](ironpdf-cookbook-quick-examples.md).

---

## How Can I Add Page Numbers When Creating PDFs From HTML or Templates?

You can set up headers or footers during PDF rendering so every generated PDF (from HTML, Razor, or markdown) includes page numbers automatically. This is perfect for batch jobs or automated invoice/report generation:

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    RightText = "Page {page} of {total-pages}",
    FontSize = 9
};

var html = "<h1>Invoice</h1><div style='page-break-after: always;'></div><p>Page 2 content</p>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice-with-numbers.pdf");
```

---

## How Do I Insert Page Numbers Into Existing PDFs?

IronPDF lets you open any existing PDF and overlay page numbers—handy for legacy docs, scanned files, or third-party exports. Here’s how:

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("old-report.pdf");
var footer = new TextHeaderFooter
{
    CenterText = "- {page} -",
    FontSize = 11
};
pdf.AddTextFooters(footer);
pdf.SaveAs("numbered-old-report.pdf");
```

You can even batch process entire folders by looping through files, as demonstrated in [Add Copy/Delete PDF Pages in C#](add-copy-delete-pdf-pages-csharp.md).

---

## What’s the Difference Between TextHeaderFooter and HtmlHeaderFooter?

**TextHeaderFooter** is best for simple, fast numbering and minimal styling.  
**HtmlHeaderFooter** allows full HTML/CSS—ideal for branding, logos, colors, or even images.

**Text example:**
```csharp
var footer = new TextHeaderFooter
{
    CenterText = "Page {page}",
    FontSize = 8
};
pdf.AddTextFooters(footer);
```

**HTML example:**
```csharp
var htmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='color:#007acc;'>Page {page} of {total-pages}</div>"
};
pdf.AddHtmlFooters(htmlFooter);
```

For including company logos or custom fonts, use `HtmlHeaderFooter`. Learn how to insert images in footers in [Add Images to PDF in C#](add-images-to-pdf-csharp.md).

---

## Can I Skip Page Numbers on Certain Pages (e.g., Cover Pages)?

Yes. IronPDF lets you choose which pages get numbers and even set the starting number for the sequence. For example, to start numbering after the first page:

```csharp
using IronPdf;
using System.Linq;

var footer = new HtmlHeaderFooter { HtmlFragment = "<center>Page {page}</center>" };
var skipCover = Enumerable.Range(0, pdf.PageCount).Skip(1);
pdf.AddHtmlFooters(footer, firstPageNumber: 1, pageIndices: skipCover);
```

This is handy for skipping covers, tables of contents, or appendix sections. See [Add Attachments to PDF in C#](add-attachments-pdf-csharp.md) for similar document manipulation tasks.

---

## How Can I Apply Page Numbers To Odd/Even Pages, or Only Certain Pages?

You can use LINQ to specify indices for odd, even, first, or last pages:

**Odd pages:**
```csharp
var oddPages = Enumerable.Range(0, pdf.PageCount).Where(i => i % 2 == 0);
pdf.AddTextFooters(new TextHeaderFooter { CenterText = "Page {page}" }, pageIndices: oddPages);
```

**First or last page:**
```csharp
pdf.AddTextFooters(new TextHeaderFooter { CenterText = "Page {page}" }, pageIndices: new[] { 0 });
pdf.AddTextFooters(new TextHeaderFooter { CenterText = "Page {page}" }, pageIndices: new[] { pdf.PageCount - 1 });
```

---

## Can I Customize Page Number Formats (e.g., Roman numerals, Section Labels)?

Absolutely. You can create custom numbering schemes by looping through pages and injecting your own logic. For Roman numerals:

```csharp
string Roman(int n) => n < 1 ? "" :
    n >= 1000 ? "M" + Roman(n - 1000) :
    n >= 900 ? "CM" + Roman(n - 900) :
    n >= 500 ? "D" + Roman(n - 500) :
    n >= 400 ? "CD" + Roman(n - 400) :
    n >= 100 ? "C" + Roman(n - 100) :
    n >= 90 ? "XC" + Roman(n - 90) :
    n >= 50 ? "L" + Roman(n - 50) :
    n >= 40 ? "XL" + Roman(n - 40) :
    n >= 10 ? "X" + Roman(n - 10) :
    n >= 9 ? "IX" + Roman(n - 9) :
    n >= 5 ? "V" + Roman(n - 5) :
    n >= 4 ? "IV" + Roman(n - 4) :
    "I" + Roman(n - 1);

foreach (int i in Enumerable.Range(0, 3))
    pdf.AddTextFooters(new TextHeaderFooter { CenterText = Roman(i + 1) }, pageIndices: new[] { i });
```

---

## How Do I Add Bates Numbers for Legal or Archival PDFs?

For legal documents, you may need Bates numbering—unique, sequential IDs for each page:

```csharp
for (int i = 0; i < pdf.PageCount; i++)
{
    var bates = $"CASE24-{(i + 1):D5}";
    pdf.AddTextFooters(new TextHeaderFooter { RightText = bates, FontSize = 8 }, pageIndices: new[] { i });
}
```

---

## What Are Common Pitfalls and Troubleshooting Tips for Page Numbering?

- **Overlap with content:** Increase `MarginBottom` in `RenderingOptions` to prevent footers from clashing with main text.
- **Wrong numbers after merging:** Always add numbers *after* merging PDFs to ensure `{total-pages}` is accurate.
- **Selecting specific pages:** Use the `pageIndices` parameter; don't manually edit pages.
- **Headers/footers missing on blank pages:** Ensure your source HTML isn't skipping rendering for those pages.

If you’re migrating from another library, see [Migrate Telerik to IronPDF](migrate-telerik-to-ironpdf.md).

---

## Where Can I Learn More or Get Help With IronPDF?

The [IronPDF documentation](https://ironpdf.com) offers full guides, including advanced header/footer placement, page numbering, and more. For broader PDF manipulation (attachments, image insertion, page management), check [Iron Software](https://ironsoftware.com) and our other FAQs like [Add Copy/Delete PDF Pages in C#](add-copy-delete-pdf-pages-csharp.md).

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

Jacob Mellor (CTO, Iron Software) has spent 41 years solving document processing challenges. His philosophy: "You name it I'll learn it. I always do my best work in new programming languages when I don't know what isn't possible." Connect on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
