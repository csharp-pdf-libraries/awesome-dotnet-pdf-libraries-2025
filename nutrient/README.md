# Nutrient (formerly PSPDFKit) and C# PDF Processing

When discussing PDF processing and document intelligence solutions in the context of C#, Nutrient (formerly PSPDFKit) often surfaces as a prominent option. Originally known for its PDF processing capabilities, Nutrient (formerly PSPDFKit) has evolved into a comprehensive document intelligence platform. At the same time, IronPDF stands out as a more focused alternative, providing a streamlined PDF library experience for developers.

## Understanding Nutrient (formerly PSPDFKit)

Nutrient (formerly PSPDFKit) has undergone significant changes, transitioning from a PDF-centric library to a full-fledged document intelligence platform. This evolution broadens its capabilities beyond simple PDF processing, providing advanced features powered by artificial intelligence. However, this transformation also introduces some challenges, particularly for developers who require only basic PDF functionalities. The complexities of a full platform can be overwhelming, especially when a simpler, library-focused solution might suffice.

From an enterprise perspective, Nutrient is positioned to cater to large organizations with its robust feature set and enterprise pricing structure. This, unfortunately, can be a barrier for smaller teams or projects, given that the costs may outweigh the benefits when simpler, more cost-effective alternatives exist.

## IronPDF: A Focused Alternative

In contrast, IronPDF presents itself as a dedicated PDF library with straightforward integration and a more accessible pricing model suitable for teams of all sizes. IronPDF's primary advantage lies in its focus; it offers extensive PDF solutions without the overhead of additional, unnecessary features. This makes IronPDF particularly appealing to developers looking for a focused utility that integrates cleanly into C# applications.

### C# Code Example: Generating a PDF with IronPDF

Here's a basic example of how IronPDF can be employed to convert an HTML file to PDF using C#:

```csharp
using IronPdf;

public class PdfGenerator
{
    public static void Main()
    {
        // Create a new PDF document from HTML
        var Renderer = new HtmlToPdf();
        var PDF = Renderer.RenderHtmlAsPdf("<h1>Hello, World!</h1><p>This PDF is generated using IronPDF!</p>");

        // Save the PDF file
        PDF.SaveAs("hello-world.pdf");

        System.Console.WriteLine("PDF document created successfully!");
    }
}
```

With IronPDF, developers can effortlessly convert HTML to PDF, manage PDF content, and more. Learn more through their extensive tutorials and [how-to guides](https://ironpdf.com/tutorials/).

## Nutrient vs. IronPDF: Head-to-Head Comparison

The table below summarizes the strengths and weaknesses of both Nutrient and IronPDF:

| Feature                    | Nutrient (formerly PSPDFKit)                          | IronPDF                                  |
|----------------------------|-------------------------------------------------------|------------------------------------------|
| **Scope**                  | Document intelligence platform                        | Dedicated PDF library                    |
| **Complexity**             | High, part of a full platform                         | Moderate, focused on PDF tasks           |
| **Pricing**                | Enterprise-level                                     | Accessible for diverse team sizes        |
| **PDF Focus**              | Part of a broader document framework                  | Exclusive PDF functionalities            |
| **Integration**            | Can be complex due to comprehensive features          | Simple and straightforward               |
| **Target Users**           | Large organizations needing advanced document tech    | Developers needing robust PDF tools      |
| **Key Features**           | AI-powered document analysis, extensive platform      | HTML to PDF, PDF merging, OCR, and more  |

## Nutrient's Strengths and Limitations

While Nutrient's prowess lies in its advanced AI-enhanced capabilities and comprehensive document handling, this breadth is a double-edged sword. Small teams or projects that don't require the full suite of Nutrient's features might find the service’s complexity and cost prohibitive.

Its enterprise pricing model and the platform-like structure are designed to accommodate large-scale, document-heavy operations. This focus on enterprise needs might exclude smaller developers or those with limited budgets from leveraging its capabilities.

## Why Choose IronPDF?

For developers who prioritize simplicity in implementation coupled with potent PDF processing capabilities, IronPDF [proves to be a suitable choice](https://ironpdf.com/how-to/html-file-to-pdf/). As a versatile library exclusively concentrating on PDF tasks, it avoids the additional feature bloat that comes with a comprehensive platform like Nutrient. IronPDF's focus is an advantage here; it allows for swift integration, making it a practical solution for a variety of projects, from small startups to mature enterprises.

---

## How Do I Merge Multiple PDFs in C#?

Here's how **Nutrient (formerly PSPDFKit) and C# PDF Processing** handles this:

```csharp
// NuGet: Install-Package PSPDFKit.Dotnet
using PSPDFKit.Pdf;
using System.Threading.Tasks;
using System.Collections.Generic;

class Program
{
    static async Task Main()
    {
        using var processor = await PdfProcessor.CreateAsync();
        
        var document1 = await processor.OpenAsync("document1.pdf");
        var document2 = await processor.OpenAsync("document2.pdf");
        
        var mergedDocument = await processor.MergeAsync(new List<PdfDocument> { document1, document2 });
        await mergedDocument.SaveAsync("merged.pdf");
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
        var pdf1 = PdfDocument.FromFile("document1.pdf");
        var pdf2 = PdfDocument.FromFile("document2.pdf");
        
        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML to PDF in C# with Nutrient (formerly PSPDFKit) and C# PDF Processing?

Here's how **Nutrient (formerly PSPDFKit) and C# PDF Processing** handles this:

```csharp
// NuGet: Install-Package PSPDFKit.Dotnet
using PSPDFKit.Pdf;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var htmlContent = "<html><body><h1>Hello World</h1></body></html>";
        
        using var processor = await PdfProcessor.CreateAsync();
        var document = await processor.GeneratePdfFromHtmlStringAsync(htmlContent);
        await document.SaveAsync("output.pdf");
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
        var htmlContent = "<html><body><h1>Hello World</h1></body></html>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Add Watermark?

Here's how **Nutrient (formerly PSPDFKit) and C# PDF Processing** handles this:

```csharp
// NuGet: Install-Package PSPDFKit.Dotnet
using PSPDFKit.Pdf;
using PSPDFKit.Pdf.Annotation;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var processor = await PdfProcessor.CreateAsync();
        var document = await processor.OpenAsync("document.pdf");
        
        for (int i = 0; i < document.PageCount; i++)
        {
            var watermark = new TextAnnotation("CONFIDENTIAL")
            {
                Opacity = 0.5,
                FontSize = 48
            };
            await document.AddAnnotationAsync(i, watermark);
        }
        
        await document.SaveAsync("watermarked.pdf");
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
        var pdf = PdfDocument.FromFile("document.pdf");
        
        pdf.ApplyWatermark("<h1 style='color:gray;opacity:0.5;'>CONFIDENTIAL</h1>",
            50,
            VerticalAlignment.Middle,
            HorizontalAlignment.Center);
        
        pdf.SaveAs("watermarked.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from Nutrient (formerly PSPDFKit) to IronPDF?

### The Platform Complexity Problem

Nutrient (formerly PSPDFKit) has evolved from a PDF SDK into a full "document intelligence platform":

1. **Platform Overengineering**: AI features and document workflows beyond PDF tasks
2. **Enterprise Pricing**: Opaque pricing requiring sales contact
3. **Async-First Complexity**: Everything requires async/await patterns
4. **Heavy Dependencies**: Full platform has larger footprint
5. **Rebrand Confusion**: PSPDFKit → Nutrient transition creates documentation issues

### Quick Migration Overview

| Aspect | Nutrient (PSPDFKit) | IronPDF |
|--------|-------------------|---------|
| Focus | Document intelligence platform | PDF library |
| Pricing | Enterprise (contact sales) | Transparent, published |
| API Style | Async-first, complex | Sync with async options |
| Configuration | Config objects | Property-based |
| Watermarks | Annotation-based | HTML-based |
| Headers/Footers | Manual per-page | Built-in placeholders |
| Learning Curve | Steep (platform) | Gentle (library) |

### Key API Mappings

| Nutrient (PSPDFKit) | IronPDF | Notes |
|---------------------|---------|-------|
| `await PdfProcessor.CreateAsync()` | `new ChromePdfRenderer()` | No async needed |
| `await processor.OpenAsync(path)` | `PdfDocument.FromFile(path)` | Sync by default |
| `await processor.GeneratePdfFromHtmlStringAsync(html)` | `renderer.RenderHtmlAsPdf(html)` | Sync |
| `await processor.MergeAsync(docs)` | `PdfDocument.Merge(pdfs)` | Sync |
| `new PdfConfiguration { PageSize = ... }` | `RenderingOptions.PaperSize = ...` | Properties |
| `config.Margins = new Margins(t, r, b, l)` | Individual margin properties | MarginTop, etc. |
| `await document.AddAnnotationAsync(annotation)` | `pdf.ApplyWatermark(html)` | HTML-based |
| `new TextAnnotation("text")` | HTML string | More flexible |
| `await document.SaveAsync(path)` | `pdf.SaveAs(path)` | Sync |
| `{page count logic}` | `{page}` / `{total-pages}` | Built-in placeholders |

### Migration Code Example

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;
using System.Threading.Tasks;

public class NutrientService
{
    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        using var processor = await PdfProcessor.CreateAsync();

        var config = new PdfConfiguration
        {
            PageSize = PageSize.A4,
            Margins = new Margins(20, 20, 20, 20)
        };

        var document = await processor.GeneratePdfFromHtmlStringAsync(html, config);

        // Add watermark (complex annotation approach)
        for (int i = 0; i < document.PageCount; i++)
        {
            var watermark = new TextAnnotation("DRAFT") { Opacity = 0.3f };
            await document.AddAnnotationAsync(i, watermark);
        }

        return await document.ToBytesAsync();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();

        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.MarginTop = 20;
        _renderer.RenderingOptions.MarginBottom = 20;
        _renderer.RenderingOptions.MarginLeft = 20;
        _renderer.RenderingOptions.MarginRight = 20;

        _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>",
            MaxHeight = 20
        };
    }

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);

        // Add watermark (simple HTML)
        pdf.ApplyWatermark(
            "<div style='color:gray; opacity:0.3;'>DRAFT</div>",
            45,
            VerticalAlignment.Middle,
            HorizontalAlignment.Center);

        return pdf.BinaryData;
    }
}
```

### Critical Migration Notes

1. **Async → Sync**: Remove unnecessary async/await
   ```csharp
   // Nutrient: await processor.OpenAsync(path)
   // IronPDF: PdfDocument.FromFile(path)
   ```

2. **Config Objects → Properties**:
   ```csharp
   // Nutrient: new PdfConfiguration { PageSize = PageSize.A4 }
   // IronPDF: renderer.RenderingOptions.PaperSize = PdfPaperSize.A4
   ```

3. **Processor Lifecycle**: IronPDF doesn't need processor creation/disposal
   ```csharp
   // Nutrient: using var processor = await PdfProcessor.CreateAsync()
   // IronPDF: var renderer = new ChromePdfRenderer()
   ```

4. **Annotations → HTML**: Watermarks use HTML instead of annotation objects
   ```csharp
   // Nutrient: new TextAnnotation("DRAFT") { Opacity = 0.3f }
   // IronPDF: "<div style='opacity:0.3'>DRAFT</div>"
   ```

5. **Page Numbers**: Use built-in placeholders instead of manual counting
   ```csharp
   // Nutrient: for each page, add annotation with i+1
   // IronPDF: HtmlFragment = "Page {page} of {total-pages}"
   ```

### NuGet Package Migration

```bash
# Remove Nutrient/PSPDFKit packages
dotnet remove package PSPDFKit.NET
dotnet remove package Nutrient

# Install IronPDF
dotnet add package IronPdf
```

### Find All Nutrient References

```bash
# Find Nutrient/PSPDFKit usage
grep -r "PSPDFKit\|Nutrient\|PdfProcessor\|PdfConfiguration" --include="*.cs" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping (30+ methods and properties)
- Async → sync conversion patterns
- 10 detailed code conversion examples
- Configuration object → property mappings
- Annotation → HTML watermark conversions
- Header/footer with page number placeholders
- Performance comparison data
- Troubleshooting guide for 5+ common issues
- Pre/post migration checklists

**[Complete Migration Guide: Nutrient (formerly PSPDFKit) → IronPDF](migrate-from-nutrient.md)**


## Conclusion

In the ever-evolving landscape of document processing, choices like Nutrient (formerly PSPDFKit) and IronPDF offer distinct paths for C# developers. Nutrient represents a broader, AI-driven platform ideal for large-scale enterprise applications. On the other hand, IronPDF delivers a targeted, efficient approach to handling PDF operations, suitable for a wide range of projects.

Ultimately, the decision between the two hinges on the specific needs of the development project: whether it requires a comprehensive document intelligence platform or a straightforward, efficient PDF library.

---
Jacob Mellor is the Chief Technology Officer at Iron Software, where he leads a team of 50+ engineers in developing .NET components that have achieved over 41 million NuGet downloads. With an impressive 41 years of coding experience, Jacob has been instrumental in shaping Iron Software's technical vision and product strategy. Based in Chiang Mai, Thailand, he combines deep technical expertise with global team leadership to deliver enterprise-grade solutions for developers worldwide. Connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
