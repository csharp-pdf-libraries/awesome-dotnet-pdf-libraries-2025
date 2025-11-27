# How Do I Create a Clickable Table of Contents in C# PDFs with IronPDF?

If you’ve ever been lost in a long PDF, you know the pain of hunting for “Section 7.4.2” by endless scrolling. A well-crafted, clickable Table of Contents (TOC) transforms PDFs, making them easy to navigate—especially for reports, manuals, or eBooks. In C#, IronPDF makes it surprisingly straightforward to generate an interactive TOC, complete with custom styling and bookmarks. This FAQ covers everything from getting started to advanced tricks, so you can build PDFs that truly work for your readers.

---

## Why Should I Add a Table of Contents to My C# PDF, and How Does IronPDF Help?

Adding a TOC isn’t just about looks—it’s about usability. Whether your PDF is a technical manual or a business proposal, a TOC gives readers a roadmap and clickable links to every important section. IronPDF is popular because it automatically scans your HTML for headings (`<h1>`–`<h6>`) and generates a TOC with hierarchical links and optional styling. Plus, it supports PDF bookmarks for sidebar navigation, letting users jump between chapters effortlessly.

Want to see it in action? Try the sample code below—less than 10 lines to a navigable PDF!

---

## How Can I Automatically Generate a Table of Contents in C# Using IronPDF?

IronPDF can produce a clickable TOC from your HTML with almost no extra code. Here’s a quick way to do it:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf

var pdfCreator = new ChromePdfRenderer();
pdfCreator.RenderingOptions.TableOfContents = TableOfContentsTypes.WithLinks;

// Sample HTML with headings
var html = @"
<h1>Overview</h1>
<p>Start here...</p>
<h2>Background</h2>
<p>Background details...</p>
<h1>Main Section</h1>
<h2>Details</h2>
";

var pdfDoc = pdfCreator.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("toc-sample.pdf");
```

Open the resulting PDF and you’ll find a TOC at the start, with each entry linking to a section. IronPDF automatically detects headings and builds the hierarchy for you.

For more on converting XML or XAML to PDF, see [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md) and [How do I convert XAML to PDF in .NET MAUI?](xaml-to-pdf-maui-csharp.md).

---

## How Does IronPDF’s Automatic TOC Detection Work?

IronPDF parses your HTML, looking for headings (`<h1>` through `<h6>`). It then establishes the hierarchy (e.g., `h1` as top-level, `h2` as subheading) and generates a TOC page—usually at the start—containing clickable links to each heading. These links work in any mainstream PDF reader. As long as you use semantic headings in your HTML, IronPDF does the rest.

---

## How Can I Control Where the TOC Appears in My PDF?

By default, IronPDF places the TOC at the beginning of your document. However, if you want to move it elsewhere, just add a placeholder in your HTML:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf

var pdfRenderer = new ChromePdfRenderer();
pdfRenderer.RenderingOptions.TableOfContents = TableOfContentsTypes.WithLinks;

var htmlContent = @"
<html>
  <body>
    <h1>Front Matter</h1>
    <p>Welcome!</p>
    <div id='ironpdf-toc'></div>
    <h1>Chapter One</h1>
    <p>Content...</p>
  </body>
</html>
";

var outputPdf = pdfRenderer.RenderHtmlAsPdf(htmlContent);
outputPdf.SaveAs("custom-toc-location.pdf");
```
Just insert `<div id='ironpdf-toc'></div>` at the desired spot—the TOC will be rendered right there!

---

## How Do I Style the TOC with Custom CSS?

IronPDF adds specific CSS classes to TOC elements, allowing you to style them as needed. Here’s how you might create a custom look:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.TableOfContents = TableOfContentsTypes.WithLinks;

var styledHtml = @"
<html>
<head>
  <style>
    .ironpdf-toc { background: #eaeaea; padding: 1em; border-radius: 6px; }
    .ironpdf-toc-title { font-size: 24px; font-weight: bold; color: #114488; }
    .ironpdf-toc-item-h1 { margin-left: 0; }
    .ironpdf-toc-item-h2 { margin-left: 16px; color: #333; }
    .ironpdf-toc-item-h3 { margin-left: 32px; color: #888; }
    .ironpdf-toc-item a { color: #0056b3; text-decoration: none; }
    .ironpdf-toc-item a:hover { text-decoration: underline; }
  </style>
</head>
<body>
  <div id='ironpdf-toc'></div>
  <h1>Start Here</h1>
  <h2>Install</h2>
  <h2>Configure</h2>
  <h1>Advanced</h1>
  <h2>Options</h2>
</body>
</html>
";

var pdf = renderer.RenderHtmlAsPdf(styledHtml);
pdf.SaveAs("custom-styled-toc.pdf");
```
Target classes like `.ironpdf-toc`, `.ironpdf-toc-title`, and `.ironpdf-toc-item-hX` for granular control over appearance.

For more on custom fonts and icons, check [How do I use web fonts and icons in PDF output?](web-fonts-icons-pdf-csharp.md).

---

## Can I Limit Which Headings Are Included in the Table of Contents?

Yes! If your document is very long, you might want only certain heading levels to appear in the TOC. You can do this with a bit of CSS:

```csharp
using IronPdf;

var pdfRenderer = new ChromePdfRenderer();
pdfRenderer.RenderingOptions.TableOfContents = TableOfContentsTypes.WithLinks;

var html = @"
<html>
<head>
  <style>
    .ironpdf-toc-item-h3,
    .ironpdf-toc-item-h4,
    .ironpdf-toc-item-h5,
    .ironpdf-toc-item-h6 { display: none; }
  </style>
</head>
<body>
  <div id='ironpdf-toc'></div>
  <h1>Part 1</h1>
  <h2>Subpart 1.1</h2>
  <h3>Detail 1.1.1</h3> <!-- Won't appear in TOC -->
  <h1>Part 2</h1>
</body>
</html>
";

var pdfOutput = pdfRenderer.RenderHtmlAsPdf(html);
pdfOutput.SaveAs("filtered-toc.pdf");
```
With this CSS, only `h1` and `h2` will show up in the TOC.

---

## How Do I Add Page Numbers to the Table of Contents?

Classic TOCs show page numbers with dotted lines. While IronPDF can’t automatically update these numbers at render time, you can mimic the effect using CSS, and add actual page numbers in footers for consistency:

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.TableOfContents = TableOfContentsTypes.WithLinks;
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center; font-size:10px;'>Page {page} of {total-pages}</div>"
};

var html = @"
<html>
<head>
  <style>
    .ironpdf-toc-item {
      display: flex;
      justify-content: space-between;
      padding-right: 1em;
    }
    .ironpdf-toc-item-text { flex: 1; }
    .ironpdf-toc-item-page::before {
      content: '.........................';
      color: #ccc;
      margin-right: 8px;
    }
  </style>
</head>
<body>
  <div id='ironpdf-toc'></div>
  <h1>Chapter One</h1>
  <h2>Introduction</h2>
</body>
</html>
";

var finalPdf = renderer.RenderHtmlAsPdf(html);
finalPdf.SaveAs("toc-with-pages.pdf");
```
The links will still be clickable, and users can reference page numbers in the footers. For more on adding [page numbers](https://ironpdf.com/blog/compare-to-other-components/questpdf-add-page-number-to-pdf/), see the official IronPDF documentation.

---

## How Can I Add or Edit Bookmarks for Sidebar Navigation?

Bookmarks are a powerful way to give your PDF sidebar navigation, especially in technical documentation or eBooks.

### How Do I Insert Bookmarks Programmatically?

You can add bookmarks to specific pages either during or after PDF creation:

```csharp
using IronPdf;

var pdfDoc = PdfDocument.FromFile("manual.pdf");

pdfDoc.Bookmarks.AddBookMarkAtEnd("Overview", 0);
pdfDoc.Bookmarks.AddBookMarkAtEnd("Details", 5);
pdfDoc.Bookmarks.AddBookMarkAtEnd("Conclusion", 10);

pdfDoc.SaveAs("with-bookmarks.pdf");
```
Bookmarks appear in the PDF reader’s sidebar, letting users jump to key sections.

### Can I Create Nested Bookmarks for Chapters and Subsections?

Absolutely. Bookmarks can be nested for hierarchical navigation:

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("guide.pdf");

var ch1 = pdf.Bookmarks.AddBookMarkAtEnd("Chapter 1", 0);
ch1.Children.AddBookMarkAtEnd("1.1 Getting Started", 2);
ch1.Children.AddBookMarkAtEnd("1.2 Setup", 4);

var ch2 = pdf.Bookmarks.AddBookMarkAtEnd("Chapter 2", 10);
ch2.Children.AddBookMarkAtEnd("2.1 Deep Dive", 12);

pdf.SaveAs("nested-bookmarks.pdf");
```
This is ideal for complex manuals or reference works.

### How Do I Read or Update Existing Bookmarks?

You can iterate through and modify bookmarks as follows:

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("existing-bookmarks.pdf");
var allBookmarks = pdf.Bookmarks.GetAllBookmarks();

foreach (var bm in allBookmarks)
{
    Console.WriteLine($"Bookmark: {bm.Title}, Page: {bm.PageIndex}");
    foreach (var child in bm.Children)
    {
        Console.WriteLine($"  Sub: {child.Title}, Page: {child.PageIndex}");
    }
}
```
This is handy for auditing or updating navigation in existing documents.

For more on merging, splitting, or organizing PDFs, see [How do I split, merge, or organize PDFs in C#?](organize-merge-split-pdfs-csharp.md).

---

## What Should I Know About Merging PDFs and Maintaining Navigation?

If you merge multiple PDFs, note that auto-generated TOCs only work if you’re starting from HTML—once heading tags are gone, you can’t auto-create a TOC. However, you can add bookmarks to the merged document:

```csharp
using IronPdf;

var firstPart = PdfDocument.FromFile("section1.pdf");
var secondPart = PdfDocument.FromFile("section2.pdf");
var merged = PdfDocument.Merge(firstPart, secondPart);

merged.Bookmarks.AddBookMarkAtEnd("Section 1", 0);
merged.Bookmarks.AddBookMarkAtEnd("Section 2", firstPart.PageCount);

merged.SaveAs("merged-with-nav.pdf");
```
For detailed merging techniques, check out [this IronPDF tutorial on merging PDFs](https://ironpdf.com/java/how-to/java-merge-pdf-tutorial/).

---

## How Do I Generate a TOC from Dynamic Data or Localized Content?

### How Do I Build a TOC from Dynamic Sources Like a Database?

Just generate your HTML string with headings as needed, and IronPDF will pick them up:

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.TableOfContents = TableOfContentsTypes.WithLinks;

var chapters = new[]
{
    new { Name = "Introduction", Body = "Welcome..." },
    new { Name = "Install", Body = "Installation steps..." },
};

var html = "<div id='ironpdf-toc'></div>";
foreach (var ch in chapters)
{
    html += $"<h1>{ch.Name}</h1><p>{ch.Body}</p>";
}

var doc = renderer.RenderHtmlAsPdf(html);
doc.SaveAs("dynamic-content-toc.pdf");
```
This is perfect for app-generated reports or user-driven content.

### How Do I Localize the TOC and Headings?

Simply use your target language in the headings. To customize the TOC title, override it with CSS or insert your own element above the TOC:

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.TableOfContents = TableOfContentsTypes.WithLinks;

var html = @"
<html>
<head>
  <style>
    .ironpdf-toc-title { content: 'Contenidos'; }
  </style>
</head>
<body>
  <div id='ironpdf-toc'></div>
  <h1>Introducción</h1>
  <h2>Configuración</h2>
</body>
</html>
";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("localized-toc.pdf");
```

---

## What Are Common Pitfalls and How Can I Troubleshoot TOC Issues in IronPDF?

- **My TOC Isn’t Showing Up:**  
  Ensure `TableOfContents` is set in `RenderingOptions` (not `None`).

- **TOC Is in the Wrong Place:**  
  Only the first `<div id='ironpdf-toc'></div>` is used.

- **Missing Headings:**  
  Only static HTML headings are detected. Dynamic content added via JavaScript won’t appear.

- **Merged PDFs Don’t Have TOC:**  
  Only HTML source can generate a TOC; use bookmarks for merged files.

- **Styling or Font Issues:**  
  Inline CSS in the HTML `<head>`. For web fonts, check out [How do I use web fonts and icons in PDF output?](web-fonts-icons-pdf-csharp.md).

- **Bookmarks Link to Wrong Page:**  
  Remember, page indices start at zero.

- **TOC Too Long?**  
  Use CSS to hide unwanted heading levels.

- **Links Don’t Work in Some Readers:**  
  Most modern readers are supported; always test in Adobe Reader or browsers.

If you’re running into unusual issues, check the [IronPDF documentation](https://ironpdf.com) or reach out to the [Iron Software](https://ironsoftware.com) team.

For advanced TOC controls, see [How do I use advanced HTML-to-PDF features in C#?](advanced-html-to-pdf-csharp.md).

---

## Where Can I Learn More or Get Help with IronPDF?

You can find more detailed guides, code samples, and troubleshooting help at the [IronPDF website](https://ironpdf.com) and the [Iron Software site](https://ironsoftware.com). If you have unique requirements—like converting XML, XAML, or adding custom navigation—see these related FAQs:

- [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md)
- [How do I convert XAML to PDF in .NET MAUI?](xaml-to-pdf-maui-csharp.md)
- [How do I use web fonts and icons in PDF output?](web-fonts-icons-pdf-csharp.md)
- [How do I split, merge, or organize PDFs in C#?](organize-merge-split-pdfs-csharp.md)
- [How do I use advanced HTML-to-PDF features in C#?](advanced-html-to-pdf-csharp.md)

---

*[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) is CTO at Iron Software, where he leads a team of 50+ engineers building .NET document processing tools. He originally created IronPDF.*
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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/) serves as CTO at [Iron Software](https://ironsoftware.com/about-us/authors/jacobmellor/), creator of IronPDF and leader of a globally distributed engineering team. Created IronPDF to solve real PDF problems encountered across decades of startup development. With 25+ years of commercial development experience, he continues to push the boundaries of .NET PDF technology from Chiang Mai, Thailand.
