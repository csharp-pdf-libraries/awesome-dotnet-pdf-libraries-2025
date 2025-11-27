# How Do I Control PDF Page Orientation and Rotation in C# with IronPDF?

Need to ensure your PDFs are always the right way up, or want to mix portrait and landscape pages in the same document? IronPDF for .NET gives you precise control over PDF page orientation and rotation—whether you’re generating new reports, tidying up scanned files, or merging content with mixed layouts. Here’s a practical FAQ that covers the most common (and a few advanced) scenarios, with copy-paste code to get you unstuck.

---

## What’s the Difference Between Page Orientation and Rotation in PDFs?

Before you start tweaking your PDFs, it’s crucial to know the difference:

- **Orientation** decides the layout shape—portrait (tall) or landscape (wide)—when the PDF is created. It’s set before content is rendered.
- **Rotation** changes the viewing angle of pages after creation. For example, you might rotate a scanned document that’s come in sideways.

Think of orientation as a printing setting, and rotation as a fix for pages that end up the wrong way around.

| Concept     | When You Use It      | Example Use Case                |
|-------------|---------------------|-------------------------------|
| Orientation | During PDF creation | Set landscape for wide charts  |
| Rotation    | On existing PDFs    | Fix upside-down scans          |

Both are useful—especially if you’re combining or repairing PDFs from diverse sources.

---

## How Do I Set Up IronPDF for Orientation and Rotation Tasks?

First, make sure you have IronPDF installed from NuGet. It’s a comprehensive PDF library for .NET that simplifies these operations.

```bash
// Install-Package IronPdf
```

Reference it in your C# files:

```csharp
using IronPdf;
// For file and directory handling:
using System.IO;
```

For more advanced PDF use cases, see [IronPDF’s documentation](https://ironpdf.com) and [Iron Software’s developer tools](https://ironsoftware.com).

---

## How Can I Create PDFs in Portrait or Landscape Orientation?

IronPDF lets you explicitly set the page orientation when rendering HTML or other content to PDF.

```csharp
using IronPdf;
// Install-Package IronPdf

var pdfRenderer = new ChromePdfRenderer();
pdfRenderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

var htmlContent = "<h1>Quarterly Dashboard</h1><table style='width:100%;'><tr><td>Data</td></tr></table>";

var document = pdfRenderer.RenderHtmlAsPdf(htmlContent);
document.SaveAs("dashboard-landscape.pdf");
```

**Tip:** Always specify orientation (even if portrait is default), so your layout stays predictable.

### When Should I Use Portrait vs. Landscape?

- **Portrait:** Best for letters, legal docs, or text-heavy content.
- **Landscape:** Ideal for spreadsheets, wide charts, or dashboards.

For more on controlling PDF structure and pagination, see [How do I control PDF pagination in C#?](html-to-pdf-page-breaks-csharp.md)

---

## How Do I Rotate Pages in an Existing PDF File?

IronPDF can rotate pages in any PDF—either all at once or selectively.

### How Do I Rotate Every Page in the Document?

```csharp
using IronPdf;
// Install-Package IronPdf

var myPdf = PdfDocument.FromFile("input.pdf");
myPdf.SetAllPageRotations(PdfPageRotation.Clockwise90);
myPdf.SaveAs("rotated-all-pages.pdf");
```

- `PdfPageRotation.Clockwise90` rotates pages 90° right.
- Use `Clockwise180` or `Clockwise270` for other angles.

### How Can I Rotate Only Certain Pages?

```csharp
using IronPdf;

var pdfDoc = PdfDocument.FromFile("multi-orientation.pdf");

// Rotate first page only
pdfDoc.SetPageRotation(0, PdfPageRotation.Clockwise90);

// Rotate pages 2, 3, and 5 upside down
pdfDoc.SetPageRotations(new[] { 1, 2, 4 }, PdfPageRotation.Clockwise180);

pdfDoc.SaveAs("some-pages-rotated.pdf");
```

**Note:** Pages are zero-indexed (first page is 0).

---

## How Can I Detect and Fix Upside-Down Pages Automatically?

To normalize a batch of scanned PDFs (often with some pages upside-down), you can check each page’s rotation and correct it in a loop.

```csharp
using IronPdf;

public void FixUpsideDownPages(string source, string destination)
{
    var doc = PdfDocument.FromFile(source);

    for (int i = 0; i < doc.PageCount; i++)
    {
        if (doc.GetPageRotation(i) == PdfPageRotation.Clockwise180)
            doc.SetPageRotation(i, PdfPageRotation.None);
    }

    doc.SaveAs(destination);
}
```

You can expand this pattern for any rotation scenario.

---

## How Do I Mix Portrait and Landscape Pages in a Single PDF?

If you want, say, a portrait cover and a landscape chart in the same file, render each page with the desired orientation, then merge.

```csharp
using IronPdf;

var portrait = new ChromePdfRenderer();
portrait.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

var landscape = new ChromePdfRenderer();
landscape.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

var coverPage = portrait.RenderHtmlAsPdf("<h1>Yearly Report</h1>");
var chartPage = landscape.RenderHtmlAsPdf("<h2>Growth Chart</h2><img src='https://placehold.co/800x200/chart.png' style='width:100%;'/>");

var finalPdf = PdfDocument.Merge(coverPage, chartPage);
finalPdf.SaveAs("mixed-orientation.pdf");
```

**FYI:** Each page keeps the orientation it was rendered with. For more on merging, see [How do I compare and merge PDFs using C# and Java?](compare-csharp-to-java.md)

---

## Can I Change Orientation on an Existing PDF (Portrait to Landscape)?

You can rotate all content, but the page dimensions won't change—portrait pages become sideways, not truly landscape.

```csharp
using IronPdf;

var doc = PdfDocument.FromFile("portrait.pdf");
doc.SetAllPageRotations(PdfPageRotation.Clockwise90);
doc.SaveAs("rotated-to-landscape.pdf");
```

If you need actual landscape page size (not just rotated content), re-render with the correct orientation.

---

## How Do I Set Custom Paper Sizes Alongside Orientation?

IronPDF supports both standard and custom sizes, combined with orientation.

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

// Or custom size: 297mm x 210mm (A4 landscape)
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(297, 210);

var pdf = renderer.RenderHtmlAsPdf("<h2>Custom Size Example</h2>");
pdf.SaveAs("custom-paper.pdf");
```

---

## How Can I Render a Live Web Page to PDF in Landscape?

Great for dashboards and wide reports—just set orientation and paper size before rendering a URL.

```csharp
using IronPdf;

var chromeRenderer = new ChromePdfRenderer();
chromeRenderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
chromeRenderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

var webPdf = chromeRenderer.RenderUrlAsPdf("https://example.com/dashboard");
webPdf.SaveAs("webpage-landscape.pdf");
```

If you need to handle page breaks in HTML, see [How do I control PDF pagination in C#?](html-to-pdf-page-breaks-csharp.md)

---

## What’s the Best Way to Batch Rotate Many PDFs?

Automate the process to fix hundreds of PDFs at once:

```csharp
using IronPdf;
using System.IO;

public void BatchRotatePdfs(string inDir, string outDir, PdfPageRotation rotation)
{
    foreach (var file in Directory.GetFiles(inDir, "*.pdf"))
    {
        using var doc = PdfDocument.FromFile(file);
        doc.SetAllPageRotations(rotation);
        doc.SaveAs(Path.Combine(outDir, Path.GetFileName(file)));
    }
}
```

Remember to dispose of each `PdfDocument` to avoid memory leaks.

---

## Can I Automatically Normalize All Pages to Portrait Orientation?

Yes! Detect landscape pages and rotate them into portrait view:

```csharp
using IronPdf;

public void NormalizeToPortrait(string input, string output)
{
    var pdf = PdfDocument.FromFile(input);

    for (int i = 0; i < pdf.PageCount; i++)
    {
        var info = pdf.GetPageInfo(i);
        if (info.Width > info.Height)
            pdf.SetPageRotation(i, PdfPageRotation.Clockwise270);
    }

    pdf.SaveAs(output);
}
```

---

## Will Merged PDFs Retain the Original Page Orientations?

Absolutely. IronPDF maintains each page’s original orientation when merging different files.

```csharp
using IronPdf;

var doc1 = PdfDocument.FromFile("portrait.pdf");
var doc2 = PdfDocument.FromFile("landscape.pdf");

var mergedPdf = PdfDocument.Merge(doc1, doc2);
mergedPdf.SaveAs("merged.pdf");
```

If you need to add page numbers or other features, see [How do I add page numbers to PDFs in C#?](add-page-numbers-pdf-csharp.md)

---

## Can I Rotate Pages Based on Their Content Automatically?

Yes, you can extract text and rotate pages if certain keywords appear:

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");

for (int i = 0; i < pdf.PageCount; i++)
{
    var pageText = pdf.ExtractTextFromPage(i);
    if (pageText.Contains("CONFIDENTIAL"))
        pdf.SetPageRotation(i, PdfPageRotation.Clockwise90);
}

pdf.SaveAs("rotated-by-content.pdf");
```

For XML-based workflows, check out [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md)

---

## What Are the Most Common Pitfalls with Orientation and Rotation?

- **Rotating doesn’t change page size:** Rotating a portrait page makes content sideways but keeps the page shape. For full landscape, re-render.
- **Mismatched merged PDFs:** Mixing page sizes and orientations can look odd in some PDF viewers—always test merged files.
- **Printer quirks:** Some printers ignore rotation. Try creating PDFs with the correct orientation instead.
- **Zero-based indices:** Remember, page indices start at 0 in IronPDF.
- **Memory management:** Dispose of `PdfDocument` objects in loops, especially with large batches.
- **Licensing:** IronPDF is commercial software—grab a license to remove [watermarks](https://ironpdf.com/how-to/pdf-memory-stream/).

---

## Where Can I Get More Help or Advanced Examples?

- See the [IronPDF documentation](https://ironpdf.com) for advanced rendering, digital signing, and more.
- For digital signatures, check [How do I digitally sign PDFs in C#?](digitally-sign-pdf-csharp.md)
- For a comparison across platforms, see [How does PDF handling in C# compare to Java?](compare-csharp-to-java.md)

---

*[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) created IronPDF to solve PDF generation headaches in .NET. He's now CTO at Iron Software, leading a team focused on developer tools.*
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

**About the Author**: Jacob Mellor is Chief Technology Officer at Iron Software, where PDF tools are used by Azure and other Fortune 500 companies. With expertise in WebAssembly, Rust, C++, Jacob focuses on making PDF generation accessible to all .NET developers. [Author Page](https://ironsoftware.com/about-us/authors/jacobmellor/)
