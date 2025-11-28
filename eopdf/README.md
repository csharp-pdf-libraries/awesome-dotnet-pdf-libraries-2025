# EO.Pdf + C# + PDF

When developers look for robust PDF generation options using C#, EO.Pdf often enters the conversation due to its integration of Chrome rendering capabilities and a reputation for creating high-quality PDF documents. EO.Pdf is a commercial library offered at a price point of $799 per license and claims to deliver a rich feature set that aligns with the needs of many enterprise-level applications. Despite its popularity, EO.Pdf, however, presents a mixed bag of strengths and challenges that need careful consideration.

EO.Pdf boasts an architecture built on a custom engine, ensuring that it no longer relies on Internet Explorer, a significant step forward. Yet, its migration to a Chromium-based system has not been without its challenges, as developers have encountered an array of compatibility issues attributed to its legacy baggage. Additionally, although EO.Pdf positions itself as a cross-platform tool, its performance and ease-of-use are primarily Windows-centric, with Linux support often seen as more of an afterthought. 

---

## How Do I HTML File To PDF Settings?

Here's how **EO.Pdf** handles this:

```csharp
// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;

class Program
{
    static void Main()
    {
        HtmlToPdfOptions options = new HtmlToPdfOptions();
        options.PageSize = PdfPageSizes.A4;
        options.OutputArea = new RectangleF(0.5f, 0.5f, 7.5f, 10.5f);
        
        HtmlToPdf.ConvertUrl("file:///C:/input.html", "output.pdf", options);
        Console.WriteLine("PDF with custom settings created.");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.MarginLeft = 20;
        renderer.RenderingOptions.MarginRight = 20;
        
        var pdf = renderer.RenderHtmlFileAsPdf("C:/input.html");
        pdf.SaveAs("output.pdf");
        Console.WriteLine("PDF with custom settings created.");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Merge Multiple PDFs in C#?

Here's how **EO.Pdf** handles this:

```csharp
// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;

class Program
{
    static void Main()
    {
        PdfDocument doc1 = new PdfDocument("file1.pdf");
        PdfDocument doc2 = new PdfDocument("file2.pdf");
        
        PdfDocument mergedDoc = new PdfDocument();
        mergedDoc.Append(doc1);
        mergedDoc.Append(doc2);
        
        mergedDoc.Save("merged.pdf");
        
        Console.WriteLine("PDFs merged successfully!");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var pdf1 = PdfDocument.FromFile("file1.pdf");
        var pdf2 = PdfDocument.FromFile("file2.pdf");
        
        var merged = PdfDocument.Merge(new List<PdfDocument> { pdf1, pdf2 });
        merged.SaveAs("merged.pdf");
        
        Console.WriteLine("PDFs merged successfully!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML to PDF in C# with EO.Pdf?

Here's how **EO.Pdf** handles this:

```csharp
// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;

class Program
{
    static void Main()
    {
        string html = "<html><body><h1>Hello World</h1><p>This is a PDF generated from HTML.</p></body></html>";
        
        HtmlToPdf.ConvertHtml(html, "output.pdf");
        
        Console.WriteLine("PDF created successfully!");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        string html = "<html><body><h1>Hello World</h1><p>This is a PDF generated from HTML.</p></body></html>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
        
        Console.WriteLine("PDF created successfully!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert a URL to PDF in .NET?

Here's how **EO.Pdf** handles this:

```csharp
// NuGet: Install-Package EO.Pdf
using EO.Pdf;
using System;

class Program
{
    static void Main()
    {
        string url = "https://www.example.com";
        
        HtmlToPdf.ConvertUrl(url, "webpage.pdf");
        
        Console.WriteLine("PDF from URL created successfully!");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        string url = "https://www.example.com";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderUrlAsPdf(url);
        pdf.SaveAs("webpage.pdf");
        
        Console.WriteLine("PDF from URL created successfully!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from EO.Pdf to IronPDF?

EO.Pdf suffers from a bloated 126MB footprint due to bundled Chrome dependencies, making deployment cumbersome and increasing infrastructure costs. Its legacy architecture—originally built on Internet Explorer before migrating to Chrome—introduces compatibility issues and technical debt.

**Migrating from EO.Pdf to IronPDF involves:**

1. **NuGet Package Change**: Install `IronPdf` package
2. **Namespace Update**: Replace `EO.Pdf` with `IronPdf`
3. **API Adjustments**: Update your code to use IronPDF's modern API patterns

**Key Benefits of Migrating:**

- Modern Chromium rendering engine with full CSS/JavaScript support
- Active maintenance and security updates
- Better .NET integration and async/await support
- Comprehensive documentation and professional support

For a complete step-by-step migration guide with detailed code examples and common gotchas, see:
**[Complete Migration Guide: EO.Pdf → IronPDF](migrate-from-eopdf.md)**


## Comparing EO.Pdf and IronPDF

When evaluating organizations’ selection of PDF libraries, EO.Pdf frequently gets juxtaposed with IronPDF. IronPDF is purpose-built for modern .NET environments, right from the ground up. It emphasizes optimized Chromium packaging, resulting in a more manageable footprint and equal support for all platforms, rather than favoring Windows. This inherent cross-platform versatility can be crucial for applications deployed or developed in diverse environments.

### Comparison Table

| Feature                             | EO.Pdf                              | IronPDF                          |
|-------------------------------------|-------------------------------------|----------------------------------|
| Cost                                | $799 per license                    | Varies (competitive pricing)     |
| Rendering Engine                    | Chromium-based                      | Optimized Chromium               |
| Platform Support                    | Primarily Windows                   | Fully cross-platform             |
| Library Size                        | 126MB                               | Compact & optimized              |
| Legacy Issues                       | Yes, migration from IE              | None, modern .NET framework      |
| Ease of Integration                 | Moderate                            | High                             |
| Additional Resources                | Limited                             | Rich tutorials and guides        |

## Key Strengths and Weaknesses

EO.Pdf’s principal strength lies in its Chromium-based rendering, which translates to a high level of W3C compliance. Compared to other libraries with similar claims, EO.Pdf competes well on this front. However, its significant footprint of 126MB can pose an unnecessary burden, particularly in scenarios where deployment size is a consideration. The prior reliance on Internet Explorer also adds unwanted complexity due to legacies that might result in compatibility problems during its migration phase.

In contrast, [IronPDF](https://ironpdf.com/how-to/html-file-to-pdf/) provides a smoother ride for developers needing streamlined integration as their focus is on ease-of-use. IronPDF has focused efforts on creating detailed [tutorials](https://ironpdf.com/tutorials/) and how-to guides that enable developers to ramp up quickly and effectively.

### Code Example Using EO.Pdf

Here is a simple C# example demonstrating how to convert HTML to PDF using EO.Pdf:

```csharp
using EO.Pdf;
using EO.Pdf.Acm;

class Program
{
    static void Main()
    {
        // Initialize EO.PDF library
        PdfDocument doc = new PdfDocument();
        // Create the PDF page
        PdfPage page = doc.Pages.Add();
        AcmRender render = new AcmRender(page);

        // Create an ACM block for HTML content
        AcmBlock block = new AcmBlock("<html><body><h1>Hello World</h1><p>This is a PDF generated by EO.Pdf</p></body></html>")
        {
            Style = { Width = new AcmUnit(100, AcmUnitType.Percent) }
        };

        // Render the block for pdf
        render.Render(block);

        // Save the PDF document
        doc.Save("output.pdf");
    }
}
```

This example illustrates the process of embedding HTML content into a PDF document using EO.Pdf’s facilities, highlighting how to kickstart PDF generation with minimal setup.

## Conclusion

EO.Pdf offers robust PDF generation capabilities with the advantage of Chrome-based rendering, useful for developers seeking compliance and quality. Yet, its inherited downsides, such as its massive library size, high cost, and limited cross-platform support can be significant determinants when evaluating which library to employ. IronPDF presents itself as a competitive alternative, encapsulating the same rendering power with a reduced footprint and enhanced platform support.

Both libraries present beneficial features, but the decision largely rests upon specific organizational needs — whether those are ease of deployment, pricing, platform support, or the necessity for modern .NET integrations. IronPDF's offerings might appeal more to developers seeking the latest advancements without the baggage, but EO.Pdf remains a viable option where its specific strengths align with business objectives.

---

## Related Tutorials

- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Comprehensive library comparison
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Modern HTML conversion comparison
- **[Cross-Platform Deployment](../cross-platform-pdf-dotnet.md)** — Linux, Docker, Cloud options
- **[Free vs Paid Libraries](../free-vs-paid-pdf-libraries.md)** — Cost analysis

### Alternative Commercial Libraries
- **[Aspose.PDF](../asposepdf/)** — Enterprise alternative
- **[SelectPdf](../selectpdf/)** — Another commercial option

### Migration Guide
- **[Migrate to IronPDF](migrate-from-eopdf.md)** — Step-by-step migration

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person team building developer tools that have been downloaded over 41 million times on NuGet. With an impressive 41 years of coding experience, Jacob combines deep technical expertise with a passion for creating solutions that make developers' lives easier. Based in Chiang Mai, Thailand, he continues to drive innovation in the PDF and document processing space. Connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
