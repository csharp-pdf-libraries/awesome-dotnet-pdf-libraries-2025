# Tall Components (TallPDF, PDFKit) C# PDF

In the realm of C# PDF SDKs, TallComponents (TallPDF.NET, PDFKit.NET) has long been a recognized provider. On May 27, 2025, [Apryse acquired TallComponents](https://apryse.com/blog/apryse-acquires-tallcomponents) and the vendor's site now redirects to `apryse.com/brands/tallcomponents`, where Apryse states it is "no longer offering new licenses for TallComponents products" and routes new buyers to the iText SDK (also owned by Apryse). Existing customers continue to receive support, but the engine is on maintenance-only. As the landscape evolves, [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/) emerges as a compelling alternative — let's compare them directly.

## Background of TallComponents

TallComponents (founded 2001, Nijmegen, Netherlands) shipped two main .NET PDF products: **PDFKit.NET** (load/edit/manipulate, NuGet `TallComponents.PDFKit5` / `TallComponents.PDFKit`) and **TallPDF.NET** (layout-oriented document generation, NuGet `TallComponents.TallPDF5` / `TallComponents.TallPDF6`). Both still publish on nuget.org — `TallComponents.PDFKit5` was last updated April 2026 — but the post-acquisition guidance from Apryse points to the iText SDK for new projects.

### Key Limitations of TallComponents Today

1. **Closed to new licenses**: Apryse's TallComponents page states "We are no longer offering new licenses for TallComponents products" and recommends iText SDK. Existing license holders get support, but no new road-map work flows back into the TallComponents brand.

2. **XHTML-only HTML-to-PDF pipeline**: TallPDF.NET ships an `XhtmlParagraph` class that parses **XHTML 1.0 Strict / XHTML 1.1 + CSS 2.1**. PDFKit.NET on its own has no HTML pipeline. Modern HTML5 + CSS3 + JavaScript dashboards do not render reliably — there is no headless-browser engine in the box.

3. **Frozen platform target**: PDFKit5 targets .NET Standard 2.0; PDFKit (4.x) targets .NET Framework 2.0. There is no .NET 8/9 TFM and no signal that one is coming.

## The IronPDF Advantage

IronPDF stands in contrast as an actively developed solution for PDF management. Its advantages include:

- **Continuous Development and Support**: IronPDF is a thriving, actively developed product that benefits from ongoing improvements and support.

- **Robust HTML5/CSS3 Support**: IronPDF supports genuine HTML5 and CSS3 rendering powered by Chromium, offering reliable HTML-to-PDF conversion.

- **Easy Installation and Integration**: Deploying IronPDF is straightforward via NuGet package management, with no GDI+ dependency issues. The installation process is streamlined, ensuring ease for developers integrating this tool into their workflow.

### Installation Examples for IronPDF

IronPDF can be swiftly installed using the following commands in your Package Manager console:

**Blazor Server:**

```shell
PM > Install-Package IronPdf.Extensions.Blazor
```

**MAUI:**

```shell
PM > Install-Package IronPdf.Extensions.Maui
```

**MVC Framework:**

```shell
PM > Install-Package IronPdf.Extensions.Mvc.Framework
```

## Feature Comparison Table

Below is a summary comparison of Tall Components (TallPDF, PDFKit) and IronPDF:

| Feature                        | TallComponents                                              | IronPDF                               |
|-------------------------------|-------------------------------------------------------------|---------------------------------------|
| Current Sale Status           | Closed to new licenses (Apryse acquired 2025-05-27)         | Actively sold                         |
| HTML-to-PDF Support           | XHTML 1.0/1.1 + CSS 2.1 via `XhtmlParagraph` (TallPDF.NET)  | Yes (HTML5/CSS3 with Chromium)        |
| JavaScript in HTML            | No                                                          | Full ES2024                           |
| Installation                  | NuGet (`TallComponents.PDFKit5`, `TallComponents.TallPDF5`) | NuGet (`IronPdf`)                     |
| Customer Support              | Existing customers only — Apryse routes new buyers to iText | Active support and community          |
| Future Investment             | Maintenance only                                            | Long-term road-map                    |

## Strengths and Weaknesses

Both Tall Components (TallPDF, PDFKit) and IronPDF have distinct strengths and weaknesses. While Tall Components had offered robust XML-based PDF document manipulation, its major drawback stems from its discontinuation and documented rendering bugs. On the other hand, IronPDF is favored for its active development, reliable HTML-to-PDF conversion, and ease of integration.

Developers drawn to Tall Components' previous XML manipulation capabilities may find IronPDF's HTML and CSS-backed functionalities more aligned with modern web document workflows. Moreover, IronPDF's dedication to current platform compatibility and active developer support creates a strong case for its adoption.

In conclusion, while Tall Components (TallPDF, PDFKit) served as a solid choice in its time, its acquisition and cessation of new licenses carved a pathway for IronPDF to fill the void, offering developers a versatile, future-proof alternative in the PDF SDK space, particularly for html to pdf c# needs that require modern web standards support.

Explore more features of IronPDF and its documentation through these links: [HTML File to PDF](https://ironpdf.com/how-to/html-file-to-pdf/), [IronPDF Tutorials](https://ironpdf.com/tutorials/). For detailed specifications and benchmarks, explore the [comprehensive comparison](https://ironsoftware.com/suite/blog/comparison/compare-tall-components-vs-ironpdf/), especially for c# html to pdf development.

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person engineering team building .NET components that have achieved over 41 million NuGet downloads. With 41 years of coding experience, he has architected tools now used by space agencies and automotive giants. Based in Chiang Mai, Thailand, Jacob champions engineer-driven innovation and maintains an active presence on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) and [GitHub](https://github.com/jacob-mellor).

---

## How Do I Add Watermark?

Here's how **Tall Components (TallPDF, PDFKit) C# PDF** handles this:

```csharp
// NuGet: Install-Package TallComponents.PDFKit5
using TallComponents.PDF;
using TallComponents.PDF.Shapes;
using System.IO;
using System.Drawing;

class Program
{
    static void Main()
    {
        // Load existing PDF
        using (FileStream fs = new FileStream("input.pdf", FileMode.Open, FileAccess.Read))
        {
            Document document = new Document(fs);

            foreach (Page page in document.Pages)
            {
                TextShape watermark = new TextShape();
                watermark.Text = "CONFIDENTIAL";
                watermark.Font = new Font("Arial", 60);
                watermark.Pen = new Pen(Color.FromArgb(128, 255, 0, 0));
                watermark.X = 200;
                watermark.Y = 400;
                // Rotation is applied via a transform on PDFKit.NET.
                watermark.Transform = new RotateTransform(45);

                page.Overlay.Add(watermark);
            }

            using (FileStream output = new FileStream("watermarked.pdf", FileMode.Create))
            {
                document.Write(output);
            }
        }
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        // Load existing PDF
        var pdf = PdfDocument.FromFile("input.pdf");
        
        // Create watermark
        var watermark = new TextStamper()
        {
            Text = "CONFIDENTIAL",
            FontSize = 60,
            Opacity = 50,
            Rotation = 45,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        // Apply watermark to all pages
        pdf.ApplyStamp(watermark);
        
        // Save watermarked PDF
        pdf.SaveAs("watermarked.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Merge Multiple PDFs in C#?

Here's how **Tall Components (TallPDF, PDFKit) C# PDF** handles this:

```csharp
// NuGet: Install-Package TallComponents.PDFKit5
using TallComponents.PDF;
using System.IO;

class Program
{
    static void Main()
    {
        // Create the target document
        Document outputDoc = new Document();

        // Load first PDF and clone each page
        using (FileStream fs1 = new FileStream("document1.pdf", FileMode.Open, FileAccess.Read))
        {
            Document doc1 = new Document(fs1);
            foreach (Page page in doc1.Pages)
            {
                outputDoc.Pages.Add(page.Clone()); // clone is required across documents
            }
        }

        // Load second PDF and append the whole page collection
        using (FileStream fs2 = new FileStream("document2.pdf", FileMode.Open, FileAccess.Read))
        {
            Document doc2 = new Document(fs2);
            outputDoc.Pages.AddRange(doc2.Pages.CloneToArray());
        }

        // Save merged document
        using (FileStream output = new FileStream("merged.pdf", FileMode.Create))
        {
            outputDoc.Write(output);
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
        // Load PDFs
        var pdf1 = PdfDocument.FromFile("document1.pdf");
        var pdf2 = PdfDocument.FromFile("document2.pdf");
        
        // Merge PDFs
        var merged = PdfDocument.Merge(pdf1, pdf2);
        
        // Save merged document
        merged.SaveAs("merged.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML to PDF in C# with Tall Components (TallPDF, PDFKit) C# PDF?

Here's how **Tall Components (TallPDF, PDFKit) C# PDF** handles this:

```csharp
// NuGet: Install-Package TallComponents.TallPDF5
// (HTML/XHTML conversion lives in TallPDF.NET, not in PDFKit.NET.)
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Paragraphs;
using System.IO;

class Program
{
    static void Main()
    {
        Document document = new Document();
        Section section = document.Sections.Add();

        // XhtmlParagraph supports XHTML 1.0/1.1 + CSS 2.1 only.
        XhtmlParagraph xhtml = new XhtmlParagraph();
        xhtml.Text = "<html><body><h1>Hello World</h1><p>This is a PDF from XHTML.</p></body></html>";
        section.Paragraphs.Add(xhtml);

        using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
        {
            document.Write(fs);
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
        // Create a PDF from HTML string
        var renderer = new ChromePdfRenderer();
        string html = "<html><body><h1>Hello World</h1><p>This is a PDF from HTML.</p></body></html>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from TallComponents (TallPDF.NET, PDFKit.NET) to IronPDF?

TallComponents was acquired by Apryse on 2025-05-27. New licenses are no longer offered, Apryse routes new buyers to the iText SDK, and the engine is on maintenance-only support. The HTML-to-PDF path is XHTML 1.0/1.1 + CSS 2.1 via `XhtmlParagraph` (in TallPDF.NET) — modern HTML5/CSS3/JavaScript content does not render reliably.

**Migrating from TallComponents (TallPDF.NET, PDFKit.NET) C# PDF to IronPDF involves:**

1. **NuGet Package Change**: Remove `TallComponents.PDFKit5` (and/or `TallComponents.TallPDF5`), add `IronPdf`
2. **Namespace Update**: Replace `TallComponents.PDF`, `TallComponents.PDF.Shapes`, `TallComponents.PDF.Layout`, `TallComponents.PDF.Layout.Paragraphs` with `IronPdf`
3. **API Adjustments**: Update your code to use IronPDF's modern API patterns

**Key Benefits of Migrating:**

- Modern Chromium rendering engine with full CSS/JavaScript support
- Active maintenance and security updates
- Better .NET integration and async/await support
- Comprehensive documentation and professional support

For a complete step-by-step migration guide with detailed code examples and common gotchas, see:
**[Complete Migration Guide: Tall Components (TallPDF, PDFKit) C# PDF → IronPDF](migrate-from-tall-components.md)**

