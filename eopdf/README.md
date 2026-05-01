# EO.Pdf + C# + PDF

When developers look for robust PDF generation options using C#, EO.Pdf (by Essential Objects) often enters the conversation due to its bundled Chromium-based renderer and a reputation for creating high-quality PDF documents. EO.Pdf is a paid commercial library — sold as Single License, 3-License Bundle, Corporate Bundle, and Corporate Plus tiers (no permanent free tier; the trial watermarks output for 30 days) — and claims to deliver a rich feature set that aligns with the needs of many enterprise-level applications. Despite its popularity, EO.Pdf presents a mixed bag of strengths and challenges that need careful consideration, particularly regarding its html to pdf c# implementation.

EO.Pdf's HTML-to-PDF converter originally embedded Internet Explorer / Trident before the project moved to a Chromium-based renderer, and some legacy quirks remain across versions. More importantly, although EO.Pdf is marketed as cross-platform, Essential Objects' own .NET Core docs explicitly state that EO.Pdf supports .NET Core 3.1+ and .NET 5/6 **on Windows only** — Linux and macOS deployments are not officially supported. For developers seeking a modern c# html to pdf library that runs natively on Linux and Docker, [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/) provides a more streamlined alternative.

For benchmarks, pricing details, and c# html to pdf performance analysis, see the [full comparison](https://ironsoftware.com/suite/blog/comparison/compare-eo-pdf-vs-ironpdf/). 

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

        // EO.Pdf has no Append; merge is a static method that returns a new PdfDocument.
        PdfDocument mergedDoc = PdfDocument.Merge(doc1, doc2);

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

### The EO.Pdf Problems

EO.Pdf has several significant issues:

1. **Bundled Chromium increases footprint**: Like IronPDF, EO.Pdf ships its own Chromium runtime, which inflates Docker images and slows deployments
2. **Legacy IE/Trident Baggage**: HTML-to-PDF was originally built on Internet Explorer before the migration to Chromium, leaving behind some compatibility quirks
3. **Paid-only commercial license**: Sold as Single / 3-License / Corporate / Corporate Plus tiers (exact prices on the order page); no permanent free tier — only a 30-day watermarked trial
4. **Static Global Options**: `HtmlToPdf.Options` is shared global state and is not thread-safe for multi-tenant applications
5. **Windows-only on .NET Core**: Essential Objects' docs explicitly state .NET Core / .NET 5+ support is Windows-only; Linux and macOS are not officially supported

### Quick Migration Overview

| Aspect | EO.Pdf | IronPDF |
|--------|--------|---------|
| Renderer | Bundled Chromium (was IE/Trident) | Embedded Chromium |
| Legacy Issues | IE/Trident migration baggage | Built on Chromium from the start |
| Configuration | Static `HtmlToPdf.Options` (not thread-safe) | Instance `ChromePdfRenderer` (thread-safe) |
| Platform Support | Windows-only on .NET Core | Windows, Linux, macOS, Docker |
| License model | Paid Single/3-License/Corporate/Corporate Plus | Per-developer commercial; free trial |
| Documentation | Reference + forum | Comprehensive tutorials and how-to library |

### Key API Mappings

| EO.Pdf | IronPDF | Notes |
|--------|---------|-------|
| `HtmlToPdf.ConvertHtml(html, path)` | `renderer.RenderHtmlAsPdf(html)` then `SaveAs()` | Two-step in IronPDF |
| `HtmlToPdf.ConvertUrl(url, path)` | `renderer.RenderUrlAsPdf(url)` then `SaveAs()` | |
| `HtmlToPdf.Options.PageSize` | `renderer.RenderingOptions.PaperSize` | Instance, not static |
| `HtmlToPdf.Options.OutputArea` | `MarginTop/Bottom/Left/Right` | Individual properties |
| `new PdfDocument(path)` | `PdfDocument.FromFile(path)` | Static factory |
| `PdfDocument.Merge(doc1, doc2)` | `PdfDocument.Merge(doc1, doc2)` | Both are static; EO.Pdf has no instance `Append` |
| `doc.Security.UserPassword` | `pdf.SecuritySettings.UserPassword` | |
| `AcmRender`, `AcmText`, `AcmBlock` | HTML/CSS | No ACM needed |
| `AfterRenderPage` event | `HtmlHeaderFooter` | For headers/footers |

### Migration Code Example

**Before (EO.Pdf - static options, not thread-safe):**
```csharp
using EO.Pdf;
using System.Drawing;

// DANGER: Static options affect ALL threads!
HtmlToPdf.Options.PageSize = PdfPageSizes.A4;
HtmlToPdf.Options.OutputArea = new RectangleF(0.5f, 0.5f, 7.5f, 10.5f);  // inches
HtmlToPdf.Options.BaseUrl = "https://example.com";
HtmlToPdf.ConvertHtml(html, "output.pdf");
```

**After (IronPDF - instance-based, thread-safe):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 12.7;    // 0.5" = 12.7mm
renderer.RenderingOptions.MarginBottom = 12.7;
renderer.RenderingOptions.MarginLeft = 12.7;
renderer.RenderingOptions.MarginRight = 12.7;
renderer.RenderingOptions.BaseUrl = new Uri("https://example.com");

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// Thread-safe, isolated options per renderer instance!
```

### Critical Migration Notes

1. **Margin Unit Conversion**: EO.Pdf uses inches in `OutputArea`, IronPDF uses millimeters. Convert: `inches × 25.4 = mm`

2. **Static to Instance**: Replace all `HtmlToPdf.Options.X` with `renderer.RenderingOptions.X`

3. **ACM to HTML**: If using `AcmRender`, `AcmText`, `AcmBlock`, migrate to HTML/CSS (much simpler!)

4. **Two-Step Save**: EO.Pdf saves directly in `ConvertHtml()`. IronPDF returns `PdfDocument`, then call `SaveAs()`

5. **Constructor to Factory**: `new PdfDocument(path)` becomes `PdfDocument.FromFile(path)`

### NuGet Package Migration

```bash
# Remove EO.Pdf
dotnet remove package EO.Pdf

# Install IronPDF
dotnet add package IronPdf
```

### Find All EO.Pdf References

```bash
grep -r "EO.Pdf\|HtmlToPdf\|AcmRender\|PdfPageSizes" --include="*.cs" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping for HtmlToPdf, ACM, and PdfDocument classes
- 10 detailed code conversion examples
- ACM to HTML/CSS migration patterns
- Thread-safe service patterns
- Unit conversion helpers (inches to mm)
- Troubleshooting guide for 8+ common issues
- Pre/post migration checklists

**[Complete Migration Guide: EO.Pdf → IronPDF](migrate-from-eopdf.md)**


## Comparing EO.Pdf and IronPDF

When evaluating organizations’ selection of PDF libraries, EO.Pdf frequently gets juxtaposed with IronPDF. IronPDF is purpose-built for modern .NET environments, right from the ground up. It emphasizes optimized Chromium packaging, resulting in a more manageable footprint and equal support for all platforms, rather than favoring Windows. This inherent cross-platform versatility can be crucial for applications deployed or developed in diverse environments.

### Comparison Table

| Feature                             | EO.Pdf                              | IronPDF                          |
|-------------------------------------|-------------------------------------|----------------------------------|
| Cost                                | Paid tiers: Single / 3-License / Corporate / Corporate Plus | Per-developer commercial; free trial |
| Rendering Engine                    | Bundled Chromium                    | Embedded Chromium                |
| Platform Support                    | Windows-only on .NET Core           | Windows, Linux, macOS, Docker    |
| Library Footprint                   | Bundles Chromium runtime            | Bundles Chromium runtime         |
| Legacy Issues                       | Yes, migration from IE/Trident      | Built on Chromium from the start |
| Ease of Integration                 | Static-class API + ACM object model | Instance-based renderer + HTML/CSS |
| Additional Resources                | Reference docs + forum              | Tutorials, how-to guides, samples |

## Key Strengths and Weaknesses

EO.Pdf’s principal strength lies in its Chromium-based rendering, which translates to a high level of W3C compliance. Compared to other libraries with similar claims, EO.Pdf competes well on this front. However, bundling a full Chromium runtime adds a meaningful deployment footprint — a tradeoff shared by other Chromium-backed libraries — and the prior reliance on Internet Explorer / Trident leaves behind some legacy quirks that occasionally surface as cross-version differences.

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
