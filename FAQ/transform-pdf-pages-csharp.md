# How Can I Transform PDF Pages in C#? (Scale, Move, Rotate, and More with IronPDF)

Need to tidy up a stack of messy PDFs in C#? IronPDF empowers you to resize, move, rotate, and extend PDF pages with code—no more manual Acrobat fixes! This FAQ covers how to scale content, fix sideways scans, add margins, batch process files, and combine transformations for practical, real-world scenarios.

## Why Would I Need to Transform PDF Pages?

PDFs are often treated as final, but real-world files are messy. You might get:

- Scanned documents rotated the wrong way
- Content that overflows or needs more margin
- Pages with mismatched sizes in a single PDF
- No room for signatures, annotations, or binding space

With IronPDF, you can automate these fixes in C#, without needing the original source. It’s perfect for prepping PDFs for print, archiving, or digital workflows.

## How Do I Set Up IronPDF for Page Transformation?

Install IronPDF from NuGet and reference it in your project:

```csharp
using IronPdf;
// NuGet: Install-Package IronPdf

var pdfDoc = PdfDocument.FromFile("input.pdf");
var page = pdfDoc.Pages[0];
```

IronPDF is a commercial library, but you can get started with a free trial. For more advanced page operations like adding, copying, or deleting pages, see [How can I add, copy, or delete PDF pages in C#?](add-copy-delete-pdf-pages-csharp.md).

## What Types of Page Transformations Are Available with IronPDF?

IronPDF supports several core page transformations:

| Operation  | Description                         | Method                      |
|------------|-------------------------------------|-----------------------------|
| Scale      | Shrink or enlarge content           | `Transform()`               |
| Translate  | Move content on the page            | `Transform()`               |
| Rotate     | Change page orientation             | `SetPageRotation()`         |
| Extend     | Add space to page edges             | `ExtendPage()`              |
| Resize     | Change physical page size           | `SetCustomPaperSize()`      |

For more on creating PDFs from markup languages, see [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md) or [How do I convert XAML to PDF in .NET MAUI/C#?](xaml-to-pdf-maui-csharp.md).

## How Can I Scale PDF Content and Add Margins?

To shrink content and center it (useful for creating whitespace margins):

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("report.pdf");

foreach (var page in pdf.Pages)
{
    double scaleFactor = 0.85;
    double offsetX = (page.Width - page.Width * scaleFactor) / 2;
    double offsetY = (page.Height - page.Height * scaleFactor) / 2;
    page.Transform(offsetX, offsetY, scaleFactor, scaleFactor);
}

pdf.SaveAs("report_with_margins.pdf");
```

To normalize pages to A4 regardless of their initial size, you can combine extending the page and scaling content accordingly. This ensures all pages print without clipping.

## How Do I Move (Translate) Content on a PDF Page?

Need to nudge everything to the right for a binding gutter or annotation? Just use `Transform()`:

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("draft.pdf");

// Move content 72 points (1 inch) right and down
foreach (var page in pdf.Pages)
{
    page.Transform(72, 72, 1.0, 1.0);
}

pdf.SaveAs("shifted_draft.pdf");
```

You can combine translating and scaling in one step if you need to both shrink and reposition content.

## How Can I Rotate Pages to Fix Sideways Scans?

To quickly rotate all pages 90 degrees clockwise:

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("scanned.pdf");
pdf.SetAllPageRotations(PdfPageRotation.Clockwise90);
pdf.SaveAs("rotated.pdf");
```

To rotate only specific pages, use:

```csharp
pdf.SetPageRotation(0, PdfPageRotation.Clockwise90); // First page only
```

For batch rotation patterns (like duplex scans), loop through pages as needed.

For more on working with images, including Base64 and Data URI support, see [How do I use Data URI and Base64 images in PDFs with C#?](data-uri-base64-images-pdf-csharp.md).

## How Do I Extend or Resize Pages for Annotations or Binding?

When you need to physically add space to a page (for signatures or binding):

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("contract.pdf");

// Add extra space: 40 points to left, 30 to bottom
pdf.Pages[0].ExtendPage(40, 0, 0, 30, MeasurementUnit.Points);

pdf.SaveAs("contract_expanded.pdf");
```

If you want to standardize the page size (like converting Letter to A4), use `ExtendPage()` or set a custom size when rendering new PDFs.

## Can I Combine Scaling, Moving, and Rotating in One Workflow?

Absolutely! Here’s an example for prepping scans for archiving:

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("archive.pdf");

foreach (var page in pdf.Pages)
{
    // Add gutter for binding
    page.ExtendPage(12, 0, 0, 0, MeasurementUnit.Millimeters);

    // Scale down to avoid clipping
    page.Transform(5, 0, 0.95, 0.95);

    // Rotate landscape pages to portrait
    if (page.Width > page.Height)
        pdf.SetPageRotation(pdf.Pages.IndexOf(page), PdfPageRotation.Clockwise270);
}

pdf.SaveAs("archive_fixed.pdf");
```

You can adapt this approach for booklets or selective transformations based on page index.

## How Do I Batch Process Multiple PDFs?

Automate transformations across entire folders like this:

```csharp
using IronPdf;
using System.IO;

void BatchTransform(string inputFolder, string outputFolder)
{
    Directory.CreateDirectory(outputFolder);

    foreach (var file in Directory.GetFiles(inputFolder, "*.pdf"))
    {
        var pdf = PdfDocument.FromFile(file);
        foreach (var page in pdf.Pages)
        {
            double scale = 0.9;
            double offsetX = (page.Width - page.Width * scale) / 2;
            double offsetY = (page.Height - page.Height * scale) / 2;
            page.Transform(offsetX, offsetY, scale, scale);
        }
        var output = Path.Combine(outputFolder, Path.GetFileName(file));
        pdf.SaveAs(output);
    }
}
```

This is handy when standardizing hundreds of PDFs for archiving or printing.

## What Are Common Pitfalls When Transforming PDFs?

- **Image Quality:** Vector graphics scale well, but raster images can get blurry if upscaled.
- **Transform vs. Page Size:** `Transform()` only modifies content position/scale—not the page size itself.
- **Units:** Know your measurement units (1 inch = 72 points = 25.4mm).
- **Rotation State:** `SetPageRotation()` sets the absolute rotation. Double-check before applying.

For updates on .NET features, see [What's new in .NET 10?](whats-new-in-dotnet-10.md). For more layout tricks, IronPDF’s [page transformation docs](https://ironpdf.com/how-to/transform-pdf-pages/) are a great resource.

## Where Can I Learn More or Get Support?

- **Product Docs:** [IronPDF Documentation](https://ironpdf.com)
- **Developer Tools:** [Iron Software](https://ironsoftware.com)
- **Related FAQs:**  
    - [Add, copy, or delete PDF pages in C#](add-copy-delete-pdf-pages-csharp.md)  
    - [XML to PDF in C#](xml-to-pdf-csharp.md)  
    - [XAML to PDF in .NET MAUI/C#](xaml-to-pdf-maui-csharp.md)  
    - [Base64 Images in PDF](data-uri-base64-images-pdf-csharp.md)  
    - [What's new in .NET 10?](whats-new-in-dotnet-10.md)  

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

**About the Author**: Jacob Mellor is Chief Technology Officer at Iron Software, where PDF tools are used by NASA and other Fortune 500 companies. With expertise in .NET, C++, C#, Jacob focuses on making PDF generation accessible to all .NET developers. [Author Page](https://ironsoftware.com/about-us/authors/jacobmellor/)
