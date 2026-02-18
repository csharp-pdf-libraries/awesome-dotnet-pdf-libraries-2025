# PDF Duo .NET + C# + PDF

PDF Duo .NET is an elusive and lesser-known library in the .NET ecosystem aimed at converting HTML and other formats to PDF. While many developers might find themselves intrigued by the potential utility of PDF Duo .NET for PDF generation in C#, the obscurity of this library presents significant challenges. PDF Duo .NET is characterized by limited documentation, sparse community engagement, and uncertainty in support and maintenance, making it less than ideal for any production-grade application.

A contrasting option that many developers might consider as a reliable alternative is IronPDF. With a robust presence in the PDF generation landscape, detailed documentation, and active support networks, IronPDF offers a solid choice for developers requiring PDF functionalities.

## Understanding PDF Duo .NET

PDF Duo .NET's primary allure lies in its advertised simplicity — a purported promise of sleek functionality without the overhead of external DLL dependencies. However, the reality is that this library's claims are overshadowed by its ambiguous status. Any attempts to utilize PDF Duo .NET are hindered by the scarcity of reliable documentation or community support platforms, posing significant challenges in problem-solving or implementing more advanced features.

The library's strength may lie in its potential ease of integration if one can interpret its sparse documentation effectively. But the lack of updates and the very real risk of abandonment compromise its viability for significant projects.

## [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/) - A Robust Alternative

IronPDF, in stark contrast, stands as a well-documented and actively maintained library developed by Iron Software. Well-regarded for its diverse range of features and supported by a comprehensive base of tutorials and technical guides, IronPDF provides a powerful solution for HTML to PDF conversion.

### Key Features of IronPDF

- **HTML to PDF Capabilities**: A seamless conversion experience that supports complex HTML/CSS with powerful html to pdf c# rendering.
- **Professional Support**: Backed by a dedicated support team, ensuring issues are resolved quickly.
- **Regular Updates**: Ensures compatibility with the latest technologies and environments.

Resourceful documentation and a professional support network with exceptional c# html to pdf features make IronPDF a preferable choice, especially when compared against the uncertainties of PDF Duo .NET.

See the [complete feature breakdown](https://ironsoftware.com/suite/blog/comparison/compare-pdf-duo-vs-ironpdf/) for detailed specifications.

---

## How Do I Convert HTML to PDF in C# with PDF Duo .NET?

Here's how **PDF Duo .NET** handles this:

```csharp
// NuGet: Install-Package PDFDuo.NET
using PDFDuo;
using System;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var htmlContent = "<h1>Hello World</h1><p>This is a PDF document.</p>";
        converter.ConvertHtmlString(htmlContent, "output.pdf");
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
        var renderer = new ChromePdfRenderer();
        var htmlContent = "<h1>Hello World</h1><p>This is a PDF document.</p>";
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
        Console.WriteLine("PDF created successfully!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert a URL to PDF in .NET?

Here's how **PDF Duo .NET** handles this:

```csharp
// NuGet: Install-Package PDFDuo.NET
using PDFDuo;
using System;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        converter.ConvertUrl("https://www.example.com", "webpage.pdf");
        Console.WriteLine("Webpage converted to PDF!");
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
        var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
        pdf.SaveAs("webpage.pdf");
        Console.WriteLine("Webpage converted to PDF!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I PDF Merging?

Here's how **PDF Duo .NET** handles this:

```csharp
// NuGet: Install-Package PDFDuo.NET
using PDFDuo;
using System;

class Program
{
    static void Main()
    {
        var merger = new PdfMerger();
        merger.AddFile("document1.pdf");
        merger.AddFile("document2.pdf");
        merger.Merge("merged.pdf");
        Console.WriteLine("PDFs merged successfully!");
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
        var pdf1 = PdfDocument.FromFile("document1.pdf");
        var pdf2 = PdfDocument.FromFile("document2.pdf");
        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
        Console.WriteLine("PDFs merged successfully!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from PDF Duo .NET to IronPDF?

### The PDF Duo Risk Problem

PDF Duo .NET presents significant risks for production applications:

1. **Unclear Provenance**: Unknown developer/company, no verifiable support channel
2. **Abandoned Status**: No recent updates, sparse documentation
3. **Unknown Rendering Engine**: No transparency about underlying technology
4. **Missing Features**: No headers/footers, watermarks, or security features
5. **Zero Community**: Virtually no Stack Overflow presence or community help

### Quick Migration Overview

| Aspect | PDF Duo .NET | IronPDF |
|--------|--------------|---------|
| Maintenance | Abandoned/Unknown | Active development |
| Documentation | Nearly non-existent | Comprehensive |
| Rendering Engine | Unknown | Chromium-based |
| Support | None | Professional support |
| Headers/Footers | Not supported | Full HTML support |
| Watermarks | Not supported | HTML-based watermarks |
| Security | Not supported | Encryption & permissions |
| Community | None | Active community |

### Key API Mappings

| PDF Duo .NET | IronPDF | Notes |
|--------------|---------|-------|
| `new HtmlToPdfConverter()` | `new ChromePdfRenderer()` | Modern renderer |
| `converter.ConvertHtmlString(html, path)` | `renderer.RenderHtmlAsPdf(html).SaveAs(path)` | Chainable API |
| `converter.ConvertUrl(url, path)` | `renderer.RenderUrlAsPdf(url).SaveAs(path)` | URL rendering |
| `converter.ConvertHtmlFile(file, path)` | `renderer.RenderHtmlFileAsPdf(file).SaveAs(path)` | File input |
| `new PdfMerger()` | `PdfDocument.Merge()` | Static method |
| `merger.AddFile(path)` | `PdfDocument.FromFile(path)` | Load first |
| `merger.Merge(output)` | `merged.SaveAs(output)` | After merge |
| `converter.PageWidth = ...` | `renderer.RenderingOptions.PaperSize` | Use PdfPaperSize |
| `converter.PageHeight = ...` | `renderer.RenderingOptions.SetCustomPaperSize()` | Custom sizes |
| `converter.Margins = new Margins(...)` | Individual margin properties | MarginTop, etc. |
| _(not available)_ | `HtmlHeaderFooter` | NEW feature |
| _(not available)_ | `pdf.ApplyWatermark()` | NEW feature |
| _(not available)_ | `pdf.SecuritySettings` | NEW feature |
| _(not available)_ | `pdf.ExtractAllText()` | NEW feature |

### Migration Code Example

**Before (PDF Duo .NET):**
```csharp
using PDFDuo;

public class PdfDuoService
{
    public void CreatePdf(string html, string outputPath)
    {
        var converter = new HtmlToPdfConverter();

        // Limited configuration options
        converter.PageWidth = 8.5f;
        converter.PageHeight = 11f;
        converter.Margins = new Margins(0.5f, 0.5f, 0.5f, 0.5f);

        converter.ConvertHtmlString(html, outputPath);

        // No headers, footers, watermarks, or security
        // No way to add page numbers
        // No text extraction capability
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

        // Paper size with built-in options
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;

        // Individual margin properties (in mm)
        _renderer.RenderingOptions.MarginTop = 15;
        _renderer.RenderingOptions.MarginBottom = 15;
        _renderer.RenderingOptions.MarginLeft = 15;
        _renderer.RenderingOptions.MarginRight = 15;

        // NEW: Headers and footers with page numbers
        _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center; font-size:10px;'>Page {page} of {total-pages}</div>",
            MaxHeight = 20
        };
    }

    public void CreatePdf(string html, string outputPath)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);

        // NEW: Watermark support
        pdf.ApplyWatermark("<div style='color:gray; opacity:0.3; font-size:72px;'>CONFIDENTIAL</div>",
            45,
            VerticalAlignment.Middle,
            HorizontalAlignment.Center);

        // NEW: Security settings
        pdf.SecuritySettings.OwnerPassword = "admin123";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;

        pdf.SaveAs(outputPath);
    }

    // NEW: Text extraction (not possible in PDF Duo)
    public string ExtractText(string pdfPath)
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        return pdf.ExtractAllText();
    }
}
```

### Critical Migration Notes

1. **Margins Object → Individual Properties**:
   ```csharp
   // PDF Duo: converter.Margins = new Margins(top, right, bottom, left)
   // IronPDF: Individual properties in millimeters
   renderer.RenderingOptions.MarginTop = 15;
   renderer.RenderingOptions.MarginBottom = 15;
   ```

2. **Page Size → PaperSize Enum**:
   ```csharp
   // PDF Duo: converter.PageWidth = 8.5f; converter.PageHeight = 11f;
   // IronPDF: Use built-in paper sizes
   renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
   ```

3. **Merger Pattern → Static Merge**:
   ```csharp
   // PDF Duo: merger.AddFile(path); merger.Merge(output);
   // IronPDF: Load files, then static merge
   var merged = PdfDocument.Merge(pdf1, pdf2);
   ```

4. **NEW Feature - Page Numbers**: Use `{page}` and `{total-pages}` placeholders
   ```csharp
   HtmlFragment = "Page {page} of {total-pages}"
   ```

5. **NEW Feature - Watermarks**: HTML-based with full CSS support
   ```csharp
   pdf.ApplyWatermark("<div style='...'>DRAFT</div>");
   ```

### NuGet Package Migration

```bash
# Remove PDF Duo .NET
dotnet remove package PDFDuo.NET

# Install IronPDF
dotnet add package IronPdf
```

### Find All PDF Duo References

```bash
# Find PDF Duo usage in your codebase
grep -r "PDFDuo\|HtmlToPdfConverter\|PdfMerger" --include="*.cs" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping reference
- 10 detailed code conversion examples
- Feature comparison showing IronPDF advantages
- New features not available in PDF Duo (headers, footers, watermarks, security, text extraction, PDF to image, form filling)
- Troubleshooting common migration issues
- Pre/post migration checklists

**[Complete Migration Guide: PDF Duo .NET → IronPDF](migrate-from-pdf-duo.md)**


## Comparison Table

Here's a more structured comparison between PDF Duo .NET and IronPDF:

| Feature                  | PDF Duo .NET                          | IronPDF             |
|--------------------------|---------------------------------------|---------------------|
| **Documentation**        | Limited and hard to find              | Comprehensive       |
| **Community Support**    | Minimal, potential risks of abandonment | Active community    |
| **Updates**              | Sporadic, unclear maintenance         | Regular and reliable|
| **HTML/CSS Support**     | Limited                               | Full support        |
| **Ease of Use**          | Unknown due to limited documentation  | User-friendly       |
| **Support Services**     | Unknown                               | Professional support|

## Exploring the Code: PDF Duo .NET vs. IronPDF

Considering the limitations and obscurity surrounding PDF Duo .NET, let's explore a sample C# implementation that might mimic a scenario using this library:

```csharp
public class PdfDuoMystery
{
    public void NobodyUsesThis()
    {
        // Attempting to invoke PDF Duo .NET functionalities
        // Claims "no extra DLLs"
        // Reality: Mystery persists with no tangible output
        // Documentation? What documentation?
        // Support forum has 3 posts from 2019
    }
}
```

In reality, the above is reflective of the user experience with PDF Duo .NET — a cautionary tale of selecting a tool with little to no usable guidance or community insight.

On the other side, IronPDF offers a clear path forward with structured, well-documented examples found readily across the following resources:
- [IronPDF HTML File to PDF Tutorial](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Various Tutorials](https://ironpdf.com/tutorials/)

A simple IronPDF usage example, illustrating its capability, could be structured as follows:

```csharp
using IronPdf;

var Renderer = new HtmlToPdf();
var PDF = Renderer.RenderUrlAsPdf("https://example.com");
PDF.SaveAs("example.pdf");
```

By merely scratching the surface with IronPDF, developers can appreciate the library’s straightforward approach — further cemented by its active and thriving support system.

## Conclusion

In conclusion, while PDF Duo .NET tantalizes with the allure of simplicity and potential autonomy from external dependencies, its practicality is severely limited by the mystery of its development and support status. The hidden costs of selecting an unsupported or poorly documented library can outweigh the benefits, leading to stalled projects and unmet deadlines.

By opting for a well-supported and thoroughly documented library like IronPDF, developers safeguard their projects from the pitfalls of neglect and position themselves to leverage advanced PDF generation features. Iron Software's commitment to continuous improvement and dedicated support positions IronPDF as a reliable and future-proof choice for anyone working within the C# and .NET ecosystems.

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person engineering team building .NET libraries that have achieved over 41 million NuGet downloads. With 41 years of coding experience, he champions engineer-driven innovation and architectural excellence in enterprise software development. Connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) or [GitHub](https://github.com/jacob-mellor).
