# iText / iTextSharp C# PDF: A Comprehensive Comparison with IronPDF

When evaluating PDF libraries for C#, iText / iTextSharp is frequently mentioned as a robust option. Known for its comprehensive feature set, iText / iTextSharp allows developers to create, modify, and manipulate PDF documents efficiently. However, while iText / iTextSharp is renowned for these capabilities, it also presents some notable challenges, especially concerning licensing models. This article will explore these aspects in detail and compare them with another popular library, IronPDF.

![iText / iTextSharp](https://itextpdf.com)

## Overview of iText / iTextSharp

iText / iTextSharp is a dual-licensed library that supports generating PDFs from scratch, modifying existing documents, and performing various operations like adding text, images, and security features to PDFs. It has been regarded as an essential library for those dealing with complex PDF generation needs in a C# environment.

### Strengths of iText / iTextSharp

1. **Comprehensive Feature Set**: iText / iTextSharp provides extensive functionalities covering almost every aspect of PDF generation and manipulation. From adding metadata to optimizing and compressing PDF files, it offers a comprehensive toolkit for developers.
2. **Wide Adoption and Community Support**: Due to its longevity and robustness, there is a wealth of community knowledge, forums, and documentation to assist developers in overcoming challenges.
3. **Cross-Platform Compatibility**: The library can be seamlessly integrated into various platforms, enhancing its utility for diverse application needs.

### Weaknesses of iText / iTextSharp

1. **AGPL License Trap**: The licensing model of iText / iTextSharp is one of its significant drawbacks. The AGPL license is highly restrictive as it requires that any web application using iText / iTextSharp open-source its entire codebase or pay for a commercial license, which can be prohibitively expensive.
2. **Subscription Pricing**: iText has phased out perpetual licensing, insisting on a recurring annual subscription for a commercial license, which might not be suitable for all budgets.
3. **Not Native HTML-to-PDF**: To convert HTML to PDF, developers are required to invest in an additional add-on called pdfHTML, which increases costs and complexity.

## Introduction to [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/)

IronPDF is a strong competitor in the PDF library market, offering several advantages over traditional libraries like iText / iTextSharp. It provides a more flexible licensing model and includes native HTML-to-PDF functionality, which simplifies development processes and reduces costs.

- Explore more about converting HTML to PDF with IronPDF [here](https://ironpdf.com/how-to/html-file-to-pdf/).
- For tutorials and comprehensive guides, visit [IronPDF Tutorials](https://ironpdf.com/tutorials/).

### Benefits of Choosing IronPDF

1. **Perpetual Licensing Option**: Unlike iText, IronPDF offers a perpetual license model, allowing one-time purchase without recurring subscription fees.
2. **Viral Licensing Avoidance**: IronPDF's licensing does not impose viral obligations, enabling proprietary code to remain closed-source without additional scrutiny or cost.
3. **Built-in HTML-to-PDF Conversion**: IronPDF simplifies c# html to pdf conversion as part of its base product, saving the need for separate add-ons.

For pricing and capability comparisons, check out the [full analysis](https://ironsoftware.com/suite/blog/comparison/compare-itext-vs-ironpdf/).

## Comparison of iText / iTextSharp and IronPDF

| **Feature**                   | **iText / iTextSharp**                                                                         | **IronPDF**                                                                                                   |
|-------------------------------|------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------|
| **Licensing Model**           | Dual (AGPL / Commercial Subscription)                                                          | Perpetual, Commercial                                                                                         |
| **HTML-to-PDF Conversion**    | Requires additional pdfHTML add-on                                                            | Included in the base product                                                                                  |
| **Open Source Requirement**   | AGPL demands open-sourcing entire application or purchasing a commercial license              | No such requirement; simple and straightforward licensing                                                     |
| **Support and Community**     | Extensive documentation and a broad community                                                 | Comprehensive tutorials available, consistent updates, and active support                                     |
| **Pricing**                   | Requires annual subscription                                                                  | Options for both subscription and perpetual pricing models; generally more cost-effective                      |
| **HTML to PDF C# Integration**| Requires pdfHTML add-on purchase                                                              | Native html to pdf c# support included                                                                        |
| **Ease of Use**               | Feature-rich yet potentially complex due to separate modules for specific functionalities      | Intuitive with built-in features and capabilities like HTML-to-PDF out of the box                             |

## C# Code Example

Here's how you might harness the capabilities of IronPDF to perform a simple HTML-to-PDF conversion:

```csharp
using IronPdf;

class Program
{
    static void Main(string[] args)
    {
        // Create a PDF from an HTML file
        HtmlToPdf htmlToPdf = new HtmlToPdf();
        
        // Specify the source HTML file and the destination PDF file
        PdfDocument pdf = htmlToPdf.RenderHtmlFileAsPdf("example.html");
        
        // Save the PDF to disk
        pdf.SaveAs("output.pdf");
        
        Console.WriteLine("PDF Created Successfully!");
    }
}
```

This minimal example showcases IronPDF's easy-to-use approach, illustrating how to convert HTML directly into a PDF document without the need for complex add-ons.

---

## How Do I Convert HTML to PDF in C# with iText / iTextSharp C# PDF: A Comprehensive Comparison with IronPDF?

Here's how **iText / iTextSharp C# PDF: A Comprehensive Comparison with IronPDF** handles this:

```csharp
// NuGet: Install-Package itext7
using iText.Html2pdf;
using System.IO;

class Program
{
    static void Main()
    {
        string html = "<h1>Hello World</h1><p>This is a PDF from HTML.</p>";
        string outputPath = "output.pdf";
        
        using (FileStream fs = new FileStream(outputPath, FileMode.Create))
        {
            HtmlConverter.ConvertToPdf(html, fs);
        }
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        string html = "<h1>Hello World</h1><p>This is a PDF from HTML.</p>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Create PDF Text Images?

Here's how **iText / iTextSharp C# PDF: A Comprehensive Comparison with IronPDF** handles this:

```csharp
// NuGet: Install-Package itext7
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;

class Program
{
    static void Main()
    {
        string outputPath = "document.pdf";
        
        using (PdfWriter writer = new PdfWriter(outputPath))
        using (PdfDocument pdf = new PdfDocument(writer))
        using (Document document = new Document(pdf))
        {
            document.Add(new Paragraph("Sample PDF Document"));
            document.Add(new Paragraph("This document contains text and an image."));
            
            Image img = new Image(ImageDataFactory.Create("image.jpg"));
            img.SetWidth(200);
            document.Add(img);
        }
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        string html = @"
            <h1>Sample PDF Document</h1>
            <p>This document contains text and an image.</p>
            <img src='image.jpg' width='200' />";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("document.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Merge Multiple PDFs in C#?

Here's how **iText / iTextSharp C# PDF: A Comprehensive Comparison with IronPDF** handles this:

```csharp
// NuGet: Install-Package itext7
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System.IO;

class Program
{
    static void Main()
    {
        string outputPath = "merged.pdf";
        string[] inputFiles = { "document1.pdf", "document2.pdf", "document3.pdf" };
        
        using (PdfWriter writer = new PdfWriter(outputPath))
        using (PdfDocument pdfDoc = new PdfDocument(writer))
        {
            PdfMerger merger = new PdfMerger(pdfDoc);
            
            foreach (string file in inputFiles)
            {
                using (PdfDocument sourcePdf = new PdfDocument(new PdfReader(file)))
                {
                    merger.Merge(sourcePdf, 1, sourcePdf.GetNumberOfPages());
                }
            }
        }
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var pdfDocuments = new List<PdfDocument>
        {
            PdfDocument.FromFile("document1.pdf"),
            PdfDocument.FromFile("document2.pdf"),
            PdfDocument.FromFile("document3.pdf")
        };
        
        var merged = PdfDocument.Merge(pdfDocuments);
        merged.SaveAs("merged.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from iText / iTextSharp to IronPDF?

### The AGPL License Trap

iText presents serious legal and business risks for commercial applications:

1. **AGPL Viral License**: Web apps using iText must **open-source their ENTIRE codebase** or pay expensive commercial license
2. **No Perpetual License**: iText forces annual subscription renewals—no one-time purchase option
3. **pdfHTML Add-On Cost**: HTML-to-PDF requires separate pdfHTML purchase at additional cost
4. **Programmatic API**: Requires manual low-level PDF construction with `Paragraph`, `Table`, `Cell`
5. **No JavaScript**: Even with pdfHTML, no JavaScript execution capability

### Quick Migration Overview

| Aspect | iText 7 / iTextSharp | IronPDF |
|--------|---------------------|---------|
| License | AGPL (viral) or subscription | Commercial, perpetual option |
| HTML-to-PDF | Separate pdfHTML add-on | Built-in Chromium |
| CSS Support | Basic CSS | Full CSS3, Flexbox, Grid |
| JavaScript | None | Full execution |
| API Paradigm | Programmatic construction | HTML-first design |
| Learning Curve | Steep (PDF coordinates) | Web developer friendly |
| Open Source Risk | Must open-source web apps | No viral requirements |

### Key API Mappings

| iText 7 | iTextSharp | IronPDF | Notes |
|---------|-----------|---------|-------|
| `new PdfWriter(stream)` | `new PdfWriter(stream)` | `new ChromePdfRenderer()` | Different paradigm |
| `new PdfDocument(writer)` | `new Document(stream)` | `renderer.RenderHtmlAsPdf()` | Returns PdfDocument |
| `new Document(pdfDoc)` | `document.Open()` | N/A (automatic) | Not needed |
| `document.Add(new Paragraph())` | `document.Add(new Paragraph())` | HTML `<p>` element | Use HTML |
| `new Table(columns)` | `new PdfPTable(columns)` | HTML `<table>` | Use HTML |
| `new Cell()` | `new PdfPCell()` | HTML `<td>` | Use HTML |
| `HtmlConverter.ConvertToPdf()` | N/A | `renderer.RenderHtmlAsPdf()` | Built-in |
| `PdfMerger.Merge()` | `PdfCopy` | `PdfDocument.Merge()` | Simpler API |
| `PdfTextExtractor.GetTextFromPage()` | `PdfTextExtractor` | `pdf.ExtractAllText()` | One-liner |
| `SetTextAlignment()` | `SetAlignment()` | CSS `text-align` | Use CSS |
| `SetFontSize(12)` | `font.Size` | CSS `font-size: 12px` | Use CSS |
| `SetBold()` | `Font.BOLD` | CSS `font-weight: bold` | Use CSS |

### Migration Code Example

**Before (iText 7):**
```csharp
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

public class ItextService
{
    public byte[] CreateReport(string title, List<Item> items)
    {
        using (var ms = new MemoryStream())
        {
            using (var writer = new PdfWriter(ms))
            using (var pdfDoc = new PdfDocument(writer))
            using (var document = new Document(pdfDoc))
            {
                document.Add(new Paragraph(title)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(24)
                    .SetBold());

                var table = new Table(UnitValue.CreatePercentArray(3))
                    .UseAllAvailableWidth();

                table.AddHeaderCell(new Cell().Add(new Paragraph("ID")));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Name")));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Value")));

                foreach (var item in items)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.Id.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(item.Name)));
                    table.AddCell(new Cell().Add(new Paragraph(item.Value.ToString("C"))));
                }

                document.Add(table);
            }
            return ms.ToArray();
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();
    }

    public byte[] CreateReport(string title, List<Item> items)
    {
        string html = $@"
            <html>
            <head>
                <style>
                    h1 {{ text-align: center; font-size: 24px; font-weight: bold; }}
                    table {{ width: 100%; border-collapse: collapse; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; }}
                    th {{ background-color: #4CAF50; color: white; }}
                </style>
            </head>
            <body>
                <h1>{title}</h1>
                <table>
                    <tr><th>ID</th><th>Name</th><th>Value</th></tr>
                    {string.Join("", items.Select(i => $"<tr><td>{i.Id}</td><td>{i.Name}</td><td>{i.Value:C}</td></tr>"))}
                </table>
            </body>
            </html>";

        return _renderer.RenderHtmlAsPdf(html).BinaryData;
    }
}
```

### Critical Migration Notes

1. **Paradigm Shift**: iText builds PDFs programmatically; IronPDF uses HTML/CSS:
   ```csharp
   // iText: document.Add(new Paragraph("Title").SetBold());
   // IronPDF: "<h1 style='font-weight:bold;'>Title</h1>"
   ```

2. **AGPL Elimination**: Remove AGPL compliance worries—IronPDF has clean commercial license

3. **No pdfHTML Add-On**: IronPDF includes HTML-to-PDF; no separate purchase needed

4. **CSS Instead of Methods**: Replace `SetTextAlignment()`, `SetFontSize()` with CSS properties

5. **Simpler Merging**: Replace `PdfMerger` with one-liner:
   ```csharp
   PdfDocument.Merge(pdf1, pdf2).SaveAs("merged.pdf");
   ```

6. **Event Handlers → HTML Headers**: Replace `IEventHandler` with simple HTML:
   ```csharp
   renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter {
       HtmlFragment = "<div>Page {page}</div>"
   };
   ```

### NuGet Package Migration

```bash
# Remove iText packages
dotnet remove package itext7
dotnet remove package itext7.pdfhtml
dotnet remove package itextsharp

# Install IronPDF
dotnet add package IronPdf
```

### Find All iText References

```bash
# Find iText usage
grep -r "using iText\|using iTextSharp" --include="*.cs" .
grep -r "PdfWriter\|PdfDocument\|Paragraph\|Table\|Cell" --include="*.cs" .
grep -r "HtmlConverter\|ConverterProperties" --include="*.cs" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping (iText 7 + iTextSharp → IronPDF)
- Namespace mapping table (9 namespaces)
- Class mapping table (12+ classes)
- Method mapping table (20+ operations)
- 10 detailed code conversion examples
- Programmatic → HTML-first paradigm shift guide
- Event handler to HTML header/footer conversion
- Razor template integration
- Performance comparison data
- Troubleshooting guide for 8+ common issues
- Pre/post migration checklists

**[Complete Migration Guide: iText / iTextSharp → IronPDF](migrate-from-itext-itextsharp.md)**


## Conclusion

Choosing the right PDF library for your C# application is crucial. iText / iTextSharp, with its comprehensive features, is a powerful tool but comes with significant licensing hurdles and costs. In contrast, IronPDF offers a streamlined, all-inclusive experience with competitive pricing options and excellent development support. For developers eager to avoid the restrictions posed by iText's AGPL license, IronPDF presents a compelling alternative, simplifying PDF tasks and maintaining the integrity and privacy of your source code.

For further [tutorials](https://ironpdf.com/tutorials/) and more in-depth guides on using IronPDF, visit their official resources.

---

## Related Tutorials

- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Comprehensive library comparison
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Modern HTML conversion (what iText lacks)
- **[Free vs Paid Libraries](../free-vs-paid-pdf-libraries.md)** — AGPL risk analysis
- **[Decision Flowchart](../choosing-a-pdf-library.md)** — Find the right library

### PDF Operations
- **[Form Filling](../fill-pdf-forms-csharp.md)** — iText vs IronPDF comparison
- **[Digital Signatures](../digital-signatures-pdf-csharp.md)** — Signing comparison
- **[Merge & Split](../merge-split-pdf-csharp.md)** — Manipulation comparison

### Migration Guide
- **[Migrate to IronPDF](migrate-from-itext-itextsharp.md)** — Escape AGPL trap

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*

---

[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) is the CTO of Iron Software, where he leads a globally distributed engineering team of 50+ engineers building .NET libraries that have achieved over 41 million NuGet downloads. With four decades of coding experience, Jacob has founded and scaled multiple successful software companies, bringing deep technical expertise to Iron Software's cross-platform development solutions. Based in Chiang Mai, Thailand, he remains passionate about advancing the .NET ecosystem and creating developer-first tooling. Connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).