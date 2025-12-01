# HiQPdf C# PDF

Creating PDF documents from HTML content is a common requirement in many C# applications today. Two popular libraries that fulfill this need are HiQPdf and IronPDF. In this article, we compare these two tools to help you choose the one that suits your project requirements the best.

HiQPdf, a commercial HTML-to-PDF library, offers HTML5/CSS3 support, making it appealing for a variety of web content rendering scenarios. However, HiQPdf's free version imposes a significant limitation—a 3-page maximum on PDF outputs, followed by an intrusive watermark. For developers evaluating the library, this makes thorough testing on larger documents difficult. Let's delve deeper into HiQPdf, alongside its competitor IronPDF.

## Strengths and Weaknesses of HiQPdf

While HiQPdf supports HTML5/CSS3 content well, it relies on a WebKit-based engine. This older rendering technology can have challenges with modern JavaScript frameworks and complex HTML structures that are commonplace in today's web development. Furthermore, the documentation does not explicitly state support for newer .NET versions, such as .NET Core or .NET 5+, raising questions about its compatibility with modern applications.

Here is a simple example of how HiQPdf might be used in C#:

```csharp
using HiQPdf;

public class PdfConverter
{
    public byte[] ConvertHtmlToPdf(string html)
    {
        HtmlToPdf htmlToPdfConverter = new HtmlToPdf();
        htmlToPdfConverter.WindowOptions.EnableOutline = true;
        byte[] pdfBytes = htmlToPdfConverter.ConvertHtmlToMemory(html, null);

        return pdfBytes;
    }
}
```

### Comparison with IronPDF

IronPDF, on the other hand, embraces the latest Chromium rendering engine, enabling better support for modern web frameworks. Additionally, IronPDF offers a genuine 30-day full-featured trial, allowing for comprehensive testing without the constraints of page limits or aggressive watermarks. The library also boasts clear documentation on compatibility with .NET versions 6, 7, and beyond.

Below is a table comparing the two libraries:

| Feature                      | HiQPdf                                   | IronPDF                                    |
|------------------------------|------------------------------------------|--------------------------------------------|
| Rendering Engine             | WebKit-based (older)                     | True Chromium                              |
| Free Tier Limitations        | 3-page limit, followed by watermark      | 30-day trial with full features            |
| Modern JavaScript Support    | Limited                                  | Extensive due to Chromium engine           |
| .NET Core/5+ Compatibility   | Not clearly documented                   | Fully documented for .NET 6, 7, 8, 9, 10   |
| HTML5/CSS3 Support           | Yes                                      | Yes                                        |

### IronPDF Advantages

IronPDF provides a robust and developer-friendly environment for transforming HTML into high-fidelity PDFs. It leverages the capabilities of the latest web technologies, ensuring that dynamic sites with complex layouts render accurately. Moreover, the extensive documentation and community support enhance its appeal.

To find out more about how IronPDF works and its capabilities, you can explore its [HTML to PDF tutorial](https://ironpdf.com/tutorials/) and [HTML file conversion guide](https://ironpdf.com/how-to/html-file-to-pdf/).

### Conclusion

Choosing the right HTML-to-PDF library depends on your specific project needs. If you are constrained by budget and only dealing with small-scale document generation, HiQPdf might fit your requirements. However, if you're working with modern web stacks or need to integrate with current .NET frameworks effectively, IronPDF presents a compelling case with its complete feature set during the trial and support for the latest technologies.

Evaluate both options carefully, considering both the short-term practicalities and long-term scalability of your application. By understanding their respective strengths and weaknesses, you'll be better positioned to make an informed decision that enhances your development workflow.

---

---

## How Do I Convert HTML to PDF in C# with HiQPdf C# PDF?

Here's how **HiQPdf C# PDF** handles this:

```csharp
// NuGet: Install-Package HiQPdf
using HiQPdf;
using System;

class Program
{
    static void Main()
    {
        HtmlToPdf htmlToPdfConverter = new HtmlToPdf();
        byte[] pdfBuffer = htmlToPdfConverter.ConvertUrlToMemory("https://example.com");
        System.IO.File.WriteAllBytes("output.pdf", pdfBuffer);
        
        // Convert HTML string
        string html = "<h1>Hello World</h1><p>This is a PDF document.</p>";
        byte[] pdfFromHtml = htmlToPdfConverter.ConvertHtmlToMemory(html, "");
        System.IO.File.WriteAllBytes("fromhtml.pdf", pdfFromHtml);
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
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("output.pdf");
        
        // Convert HTML string
        string html = "<h1>Hello World</h1><p>This is a PDF document.</p>";
        var pdfFromHtml = renderer.RenderHtmlAsPdf(html);
        pdfFromHtml.SaveAs("fromhtml.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Merge Multiple PDFs in C#?

Here's how **HiQPdf C# PDF** handles this:

```csharp
// NuGet: Install-Package HiQPdf
using HiQPdf;
using System;

class Program
{
    static void Main()
    {
        // Create first PDF
        HtmlToPdf converter1 = new HtmlToPdf();
        byte[] pdf1 = converter1.ConvertHtmlToMemory("<h1>First Document</h1>", "");
        System.IO.File.WriteAllBytes("doc1.pdf", pdf1);
        
        // Create second PDF
        HtmlToPdf converter2 = new HtmlToPdf();
        byte[] pdf2 = converter2.ConvertHtmlToMemory("<h1>Second Document</h1>", "");
        System.IO.File.WriteAllBytes("doc2.pdf", pdf2);
        
        // Merge PDFs
        PdfDocument document1 = PdfDocument.FromFile("doc1.pdf");
        PdfDocument document2 = PdfDocument.FromFile("doc2.pdf");
        document1.AddDocument(document2);
        document1.WriteToFile("merged.pdf");
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
        var renderer = new ChromePdfRenderer();
        
        // Create first PDF
        var pdf1 = renderer.RenderHtmlAsPdf("<h1>First Document</h1>");
        pdf1.SaveAs("doc1.pdf");
        
        // Create second PDF
        var pdf2 = renderer.RenderHtmlAsPdf("<h1>Second Document</h1>");
        pdf2.SaveAs("doc2.pdf");
        
        // Merge PDFs
        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Add Headers and Footers to PDFs?

Here's how **HiQPdf C# PDF** handles this:

```csharp
// NuGet: Install-Package HiQPdf
using HiQPdf;
using System;

class Program
{
    static void Main()
    {
        HtmlToPdf htmlToPdfConverter = new HtmlToPdf();
        
        // Add header
        htmlToPdfConverter.Document.Header.Height = 50;
        HtmlToPdfVariableElement headerHtml = new HtmlToPdfVariableElement("<div style='text-align:center'>Page Header</div>", "");
        htmlToPdfConverter.Document.Header.Add(headerHtml);
        
        // Add footer with page number
        htmlToPdfConverter.Document.Footer.Height = 50;
        HtmlToPdfVariableElement footerHtml = new HtmlToPdfVariableElement("<div style='text-align:center'>Page {CrtPage} of {PageCount}</div>", "");
        htmlToPdfConverter.Document.Footer.Add(footerHtml);
        
        byte[] pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory("<h1>Document with Headers and Footers</h1>", "");
        System.IO.File.WriteAllBytes("header-footer.pdf", pdfBuffer);
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
        
        // Configure header and footer
        renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
        {
            CenterText = "Page Header",
            FontSize = 12
        };
        
        renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
        {
            CenterText = "Page {page} of {total-pages}",
            FontSize = 10
        };
        
        var pdf = renderer.RenderHtmlAsPdf("<h1>Document with Headers and Footers</h1>");
        pdf.SaveAs("header-footer.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from HiQPdf to IronPDF?

### The HiQPdf Challenges

HiQPdf has several limitations that drive developers to seek alternatives:

1. **3-Page Limit**: "Free" version caps output at 3 pages with intrusive watermarks—essentially a demo
2. **WebKit Engine**: Older rendering technology struggles with modern JavaScript frameworks
3. **Fragmented Packages**: Separate packages for different platforms (HiQPdf.Free, HiQPdf)
4. **Limited .NET Support**: Unclear compatibility with .NET Core/.NET 5+
5. **Point-Based Units**: Uses points (1/72 inch) while IronPDF uses millimeters
6. **Proprietary Placeholders**: `{CrtPage}`, `{PageCount}` syntax differs from IronPDF

### Quick Migration Overview

| Aspect | HiQPdf | IronPDF |
|--------|--------|---------|
| Rendering Engine | WebKit (older) | Chromium (modern) |
| Page Limit | 3 pages (free) | Full documents |
| Unit System | Points (72/inch) | Millimeters |
| .NET Core/5+ | Unclear | Full support |
| Modern JS | Limited | Full Chromium support |
| NuGet Package | HiQPdf / HiQPdf.Free | IronPdf |

### Key API Mappings

| HiQPdf | IronPDF | Notes |
|--------|---------|-------|
| `new HtmlToPdf()` | `new ChromePdfRenderer()` | Main converter |
| `ConvertHtmlToMemory(html, baseUrl)` | `RenderHtmlAsPdf(html, baseUrl)` | Returns PdfDocument |
| `ConvertUrlToMemory(url)` | `RenderUrlAsPdf(url)` | Returns PdfDocument |
| `ConvertHtmlToFile(html, baseUrl, path)` | `RenderHtmlAsPdf(html).SaveAs(path)` | Two-step in IronPDF |
| `Document.Header` | `RenderingOptions.HtmlHeader` | HTML-based headers |
| `Document.Footer` | `RenderingOptions.HtmlFooter` | HTML-based footers |
| `Document.Header.Height` | `RenderingOptions.MarginTop` | Adjust margin |
| `BrowserWidth` | `RenderingOptions.ViewPortWidth` | Viewport width |
| `TriggerMode.Auto` | Default behavior | No equivalent needed |
| `TriggerMode.WaitTime` | `RenderingOptions.RenderDelay` | In milliseconds |
| `TriggerMode.Manual` | `RenderingOptions.WaitFor` | JavaScript-based |
| `{CrtPage}` | `{page}` | Current page placeholder |
| `{PageCount}` | `{total-pages}` | Total pages placeholder |

### Migration Code Example

**Before (HiQPdf):**
```csharp
using HiQPdf;

public class HiQPdfService
{
    public byte[] CreatePdf(string html)
    {
        HtmlToPdf converter = new HtmlToPdf();

        // Page setup (points: 1 inch = 72 points)
        converter.Document.PageSize = PdfPageSize.A4;
        converter.Document.PageOrientation = PdfPageOrientation.Portrait;
        converter.Document.Margins = new PdfMargins(72, 72, 72, 72);  // 1 inch

        // Header with page numbers
        converter.Document.Header.Height = 36;  // 0.5 inch
        HtmlToPdfVariableElement header = new HtmlToPdfVariableElement(
            "<div>Page {CrtPage} of {PageCount}</div>", "");
        converter.Document.Header.Add(header);

        // Wait for JavaScript
        converter.TriggerMode = ConversionTriggerMode.WaitTime;
        converter.WaitBeforeConvert = 2;  // 2 seconds

        return converter.ConvertHtmlToMemory(html, null);
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

        // Page setup (millimeters)
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        _renderer.RenderingOptions.MarginTop = 25;     // ~1 inch = 25.4mm
        _renderer.RenderingOptions.MarginBottom = 25;
        _renderer.RenderingOptions.MarginLeft = 25;
        _renderer.RenderingOptions.MarginRight = 25;

        // Header with page numbers (note placeholder syntax!)
        _renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div>Page {page} of {total-pages}</div>",
            MaxHeight = 15  // ~0.5 inch = 12.7mm
        };

        // Wait for JavaScript
        _renderer.RenderingOptions.RenderDelay = 2000;  // 2000 milliseconds
    }

    public byte[] CreatePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Critical Migration Notes

1. **Unit Conversion**: HiQPdf uses points (72 per inch), IronPDF uses millimeters (25.4 per inch):
   ```csharp
   int millimeters = (int)(hiqpdfPoints * 25.4 / 72);
   ```

2. **Placeholder Syntax**: Must update all header/footer placeholders:
   - `{CrtPage}` → `{page}`
   - `{PageCount}` → `{total-pages}`
   - `{CrtPageUri}` → `{url}`
   - `{CrtPageTitle}` → `{html-title}`

3. **TriggerMode Migration**:
   - `TriggerMode.Auto` → Default (no action needed)
   - `TriggerMode.WaitTime` → `RenderingOptions.RenderDelay = milliseconds`
   - `TriggerMode.Manual` → `RenderingOptions.WaitFor.JavaScript("callback")`

4. **No 3-Page Limit**: IronPDF generates complete documents without artificial limits

5. **Header/Footer Height**: HiQPdf `Document.Header.Height` becomes `HtmlHeader.MaxHeight` in IronPDF

### NuGet Package Migration

```bash
# Remove HiQPdf
dotnet remove package HiQPdf
dotnet remove package HiQPdf.Free

# Install IronPDF
dotnet add package IronPdf
```

### Find All HiQPdf References

```bash
# Find all HiQPdf usage
grep -r "HiQPdf\|HtmlToPdf\|ConvertHtmlToMemory\|CrtPage\|PageCount" --include="*.cs" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping (50+ methods and properties)
- 10 detailed code conversion examples
- TriggerMode to RenderDelay/WaitFor migration
- Header/Footer element conversion patterns
- Unit conversion helpers (points → millimeters)
- PdfDocument manipulation migration
- Performance comparison data
- Troubleshooting guide for 8+ common issues
- Pre/post migration checklists

**[Complete Migration Guide: HiQPdf → IronPDF](migrate-from-hiqpdf.md)**


## Related Tutorials

- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Comprehensive library comparison
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Modern HTML conversion comparison
- **[Free vs Paid Libraries](../free-vs-paid-pdf-libraries.md)** — Cost analysis

### Alternative Commercial Libraries
- **[SelectPdf](../selectpdf/)** — Windows-only alternative
- **[Winnovative](../winnovative/)** — WebKit-based option
- **[ExpertPdf](../expertpdf/)** — Another commercial choice

### Migration Guide
- **[Migrate to IronPDF](migrate-from-hiqpdf.md)** — Step-by-step migration

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person team building developer tools that have earned over 41 million NuGet downloads. With 41 years of coding experience and a track record of founding and scaling multiple successful software companies, Jacob is a long-time champion of the .NET community. Based in Chiang Mai, Thailand, you can follow his insights on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) and [Medium](https://medium.com/@jacob.mellor).
