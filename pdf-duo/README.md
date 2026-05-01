# PDF Duo .NET + C# + PDF

PDF Duo .NET, from DuoDimension Software, is a lesser-known HTML-to-PDF component for .NET. Its last public release (v2.4) shipped in December 2010 and targets only .NET Framework 1.1 through 3.5; it is not distributed via NuGet. PDF Duo .NET is characterized by limited documentation, sparse community engagement, and ~15 years of inactivity, making it a poor choice for production-grade applications on modern .NET runtimes.

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
// PDF Duo .NET is not on NuGet — reference PDFDuo.dll from the vendor download.
using DuoDimension;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // OpenHTML accepts a file path, URL, or stream; there is no documented
        // "convert HTML string" call, so write the markup to a temp file first.
        var tempHtml = Path.GetTempFileName() + ".html";
        File.WriteAllText(tempHtml, "<h1>Hello World</h1><p>This is a PDF document.</p>");

        var conv = new HtmlToPdf();
        conv.OpenHTML(tempHtml);
        conv.SavePDF("output.pdf");
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
// PDF Duo .NET is not on NuGet — reference PDFDuo.dll from the vendor download.
using DuoDimension;
using System;

class Program
{
    static void Main()
    {
        var conv = new HtmlToPdf();
        conv.OpenHTML("https://www.example.com");
        conv.SavePDF("webpage.pdf");
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

**PDF Duo .NET does not provide a native PDF-merge API.** The documented surface is HTML-to-PDF only, so teams that needed to merge typically rendered each PDF with PDF Duo and then concatenated them with a separate library (commonly iTextSharp 4.x in 2010-era projects):

```csharp
// PDF Duo .NET is not on NuGet — reference PDFDuo.dll from the vendor download.
using DuoDimension;
using System;

class Program
{
    static void Main()
    {
        // Render each source HTML to its own PDF
        var conv = new HtmlToPdf();
        conv.OpenHTML("page1.html");
        conv.SavePDF("document1.pdf");

        conv = new HtmlToPdf();
        conv.OpenHTML("page2.html");
        conv.SavePDF("document2.pdf");

        // Merging into a single PDF requires another library (e.g., iTextSharp).
        Console.WriteLine("PDF Duo has no native merge — combine output with another library.");
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

1. **Vendor**: DuoDimension Software (duodimension.com) — small ISV, no public roadmap
2. **Abandoned Status**: Last public release v2.4 dated December 10, 2010; no documented updates since
3. **Pre-modern Targeting**: Documented support for .NET Framework 1.1–3.5 only — no .NET Core / .NET 5+ / Linux / Docker story
4. **Not on NuGet**: DLL download from the vendor; no SemVer or transitive resolution
5. **Unknown Rendering Engine**: Vendor materials do not disclose the HTML/CSS engine used
6. **Narrow Feature Set**: HTML-to-PDF only — no native merge, watermark, encryption, signing, forms, or text extraction
7. **Negligible Community**: No GitHub repo, no meaningful Stack Overflow tag

### Quick Migration Overview

| Aspect | PDF Duo .NET | IronPDF |
|--------|--------------|---------|
| Last release | v2.4 (December 2010) | Active development |
| Distribution | DLL download from duodimension.com | NuGet `IronPdf` |
| Runtime support | .NET Framework 1.1 - 3.5 | .NET FX 4.6.2+, .NET 6/7/8/9/10, Linux, macOS, Docker |
| Documentation | One vendor product page | Comprehensive docs + API reference |
| Rendering engine | Not disclosed | Chromium-based |
| Support | None visible | Professional support |
| Headers/Footers | Not in documented API | Full HTML support |
| Watermarks | Not in documented API | HTML-based watermarks |
| Security | Not in documented API | Encryption + permissions |
| PDF merge | Not in documented API | `PdfDocument.Merge` |
| Community | Negligible | Active community |

### Key API Mappings

| PDF Duo .NET | IronPDF | Notes |
|--------------|---------|-------|
| `new DuoDimension.HtmlToPdf()` | `new ChromePdfRenderer()` | Modern renderer |
| `conv.OpenHTML(htmlFile); conv.SavePDF(path);` | `renderer.RenderHtmlFileAsPdf(htmlFile).SaveAs(path)` | File input |
| `conv.OpenHTML(url); conv.SavePDF(path);` | `renderer.RenderUrlAsPdf(url).SaveAs(path)` | URL rendering |
| _(write string to temp .html, then `OpenHTML`)_ | `renderer.RenderHtmlAsPdf(html).SaveAs(path)` | Direct HTML string |
| _(not in documented API)_ | `PdfDocument.Merge(pdf1, pdf2)` | NEW: native merge |
| _(not in documented API)_ | `HtmlHeaderFooter` | NEW: headers/footers |
| _(not in documented API)_ | `pdf.ApplyWatermark()` | NEW: watermarks |
| _(not in documented API)_ | `pdf.SecuritySettings` | NEW: encryption |
| _(not in documented API)_ | `pdf.ExtractAllText()` | NEW: text extraction |
| _(no settings/options object)_ | `renderer.RenderingOptions.PaperSize` | Use `PdfPaperSize` |
| _(no settings/options object)_ | `RenderingOptions.MarginTop` etc. | Individual margin properties |

### Migration Code Example

**Before (PDF Duo .NET):**
```csharp
using DuoDimension;
using System.IO;

public class PdfDuoService
{
    public void CreatePdf(string html, string outputPath)
    {
        // PDF Duo's HtmlToPdf does not document a settings object covering
        // page size or margins; OpenHTML / SavePDF is the public surface.
        var tempHtml = Path.GetTempFileName() + ".html";
        File.WriteAllText(tempHtml, html);

        var conv = new HtmlToPdf();
        conv.OpenHTML(tempHtml);
        conv.SavePDF(outputPath);

        // No headers, footers, watermarks, security, page numbers,
        // or text extraction in the documented API.
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

1. **No settings object → IronPDF `RenderingOptions`**:
   ```csharp
   // PDF Duo: HtmlToPdf has no documented settings object for paper size or margins.
   // IronPDF: explicit options on RenderingOptions
   renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
   renderer.RenderingOptions.MarginTop = 15;
   renderer.RenderingOptions.MarginBottom = 15;
   ```

2. **No native merge → static `PdfDocument.Merge`**:
   ```csharp
   // PDF Duo: no merge API; teams paired it with iTextSharp 4.x.
   // IronPDF: load files, then static merge
   var merged = PdfDocument.Merge(pdf1, pdf2);
   ```

3. **NEW Feature - Page Numbers**: Use `{page}` and `{total-pages}` placeholders
   ```csharp
   HtmlFragment = "Page {page} of {total-pages}"
   ```

4. **NEW Feature - Watermarks**: HTML-based with full CSS support
   ```csharp
   pdf.ApplyWatermark("<div style='...'>DRAFT</div>");
   ```

### Package / Reference Migration

```bash
# PDF Duo .NET is not on NuGet — remove the project reference to PDFDuo.dll
# in Visual Studio (References -> right-click -> Remove) and delete the
# vendored binary.

# Install IronPDF
dotnet add package IronPdf
```

### Find All PDF Duo References

```bash
# Find PDF Duo usage in your codebase
grep -r "DuoDimension\|HtmlToPdf\b\|OpenHTML\|SavePDF" --include="*.cs" .
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
| **Last release**         | v2.4 (December 2010)                   | Active             |
| **Distribution**         | DLL from duodimension.com (no NuGet) | NuGet `IronPdf`    |
| **Runtime**              | .NET Framework 1.1 - 3.5              | .NET FX 4.6.2+, .NET 6/7/8/9/10, Linux, macOS, Docker |
| **Documentation**        | One vendor product page                | Comprehensive       |
| **Community Support**    | Negligible                             | Active community    |
| **HTML/CSS Support**     | Engine not disclosed                   | Modern Chromium     |
| **Support Services**     | None visible                           | Professional support|

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

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("example.pdf");
```

By merely scratching the surface with IronPDF, developers can appreciate the library’s straightforward approach — further cemented by its active and thriving support system.

## Conclusion

In conclusion, while PDF Duo .NET tantalizes with the allure of simplicity and potential autonomy from external dependencies, its practicality is severely limited by the mystery of its development and support status. The hidden costs of selecting an unsupported or poorly documented library can outweigh the benefits, leading to stalled projects and unmet deadlines.

By opting for a well-supported and thoroughly documented library like IronPDF, developers safeguard their projects from the pitfalls of neglect and position themselves to leverage advanced PDF generation features. Iron Software's commitment to continuous improvement and dedicated support positions IronPDF as a reliable and future-proof choice for anyone working within the C# and .NET ecosystems.

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person engineering team building .NET libraries that have achieved over 41 million NuGet downloads. With 41 years of coding experience, he champions engineer-driven innovation and architectural excellence in enterprise software development. Connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) or [GitHub](https://github.com/jacob-mellor).
