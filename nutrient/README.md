# Nutrient (formerly PSPDFKit) and C# PDF Processing

When evaluating PDF and document-processing options for C#, Nutrient — the company formerly known as PSPDFKit, rebranded on 2024-10-23 after Insight Partners' 2021 investment — is one of the larger commercial vendors. For server-side .NET, Nutrient's offering is **GdPicture.NET**, the toolkit they acquired from ORPALIS, now positioned as the "Nutrient .NET SDK." IronPDF is the focused, PDF-only alternative considered in this article.

## Understanding the Nutrient .NET SDK

The Nutrient .NET SDK is delivered as the `GdPicture` NuGet package, with a `GdPicture14` root namespace and primary classes such as `GdPicturePDF` and `GdPictureDocumentConverter`. It runs on .NET Framework 4.6.2 and modern .NET (6 / 7 / 8 / 10) across Windows, macOS, and Linux. The toolkit covers more than PDF — OCR, barcodes, TWAIN scanning, image processing, and document conversion are all in scope — which is useful if you need that breadth, but is more surface area than a PDF-only project requires. HTML-to-PDF in particular relies on a system-installed Chrome or Edge (or a portable path you supply via `SetWebBrowserPath`).

Pricing for the .NET SDK is sales-led: the `nutrient.io/sdk/pricing` page routes to a contact form rather than publishing rates, which makes side-by-side cost comparison harder for small teams.

## [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/): A Focused Alternative

In contrast, IronPDF presents itself as a dedicated PDF library with straightforward integration and a more accessible pricing model suitable for teams of all sizes. IronPDF's primary advantage lies in its focus; it offers extensive PDF solutions without the overhead of additional, unnecessary features. This makes IronPDF particularly appealing to developers looking for a focused utility that integrates cleanly into C# applications.

### C# Code Example: Generating a PDF with IronPDF

Here's a basic example of how IronPDF can be employed to convert an HTML file to PDF using C#:

```csharp
using IronPdf;

public class PdfGenerator
{
    public static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Create a new PDF document from HTML
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello, World!</h1><p>This PDF is generated using IronPDF!</p>");

        // Save the PDF file
        pdf.SaveAs("hello-world.pdf");

        System.Console.WriteLine("PDF document created successfully!");
    }
}
```

With IronPDF, developers can effortlessly convert HTML to PDF, manage PDF content, and more. Learn more through their extensive tutorials and [how-to guides](https://ironpdf.com/tutorials/).

## Nutrient vs. IronPDF: Head-to-Head Comparison

The table below summarizes the strengths and weaknesses of both Nutrient and IronPDF:

| Feature                    | Nutrient .NET SDK (GdPicture)                                 | IronPDF                                  |
|----------------------------|----------------------------------------------------------------|------------------------------------------|
| **NuGet package**          | `GdPicture` (owner ORPALIS, a Nutrient subsidiary)             | `IronPdf`                                |
| **Root namespace**         | `GdPicture14`                                                  | `IronPdf`                                |
| **Scope**                  | PDF + OCR + barcode + scanning + imaging                       | PDF library                              |
| **Pricing**                | Sales-led; contact for quote                                   | Published per-developer pricing          |
| **HTML to PDF**            | Requires system Chrome / Edge (or portable path)               | Embedded Chromium, no host browser       |
| **Cross-platform**         | Windows / Linux / macOS, .NET Framework + .NET 6/7/8/10        | Windows / Linux / macOS, .NET Framework + .NET 6+ |
| **API style**              | Mostly synchronous, procedural draw calls                      | Sync with async option, fluent           |
| **Target users**           | Teams needing imaging + scanning + PDF in one toolkit          | Teams needing PDF only                   |

## Nutrient's Strengths and Limitations

The Nutrient .NET SDK's strength is breadth: PDF, OCR, barcode reading and writing, TWAIN scanning, and conversion across more than 100 file formats from a single package. Teams that need that full surface area get a lot in one library.

The trade-off is twofold. First, that breadth shows up in the API: PDF operations are largely procedural (draw text, set alpha, manage OCG layers, manage page selections), and HTML-to-PDF depends on a system Chrome / Edge install. Second, the .NET SDK is sold via Contact-Sales rather than published pricing, which is harder to evaluate at small-team scale.

## Why Choose IronPDF?

For developers who prioritize simplicity in implementation coupled with potent PDF processing capabilities, IronPDF [proves to be a suitable choice](https://ironpdf.com/how-to/html-file-to-pdf/). As a versatile library exclusively concentrating on PDF tasks with superior c# html to pdf functionality, it avoids the additional feature bloat that comes with a comprehensive platform like Nutrient. IronPDF's focus is an advantage here; it allows for swift integration, making it a practical solution for a variety of projects, from small startups to mature enterprises.

Explore performance benchmarks and pricing in the [comprehensive guide](https://ironsoftware.com/suite/blog/comparison/compare-nutrient-vs-ironpdf/).

---

## How Do I Merge Multiple PDFs in C#?

Here's how the **Nutrient .NET SDK (GdPicture)** handles this:

```csharp
// NuGet: Install-Package GdPicture
using GdPicture14;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        IEnumerable<string> sourceFiles = new List<string>
        {
            "document1.pdf",
            "document2.pdf"
        };

        using var converter = new GdPictureDocumentConverter();
        converter.CombineToPDF(sourceFiles, "merged.pdf", PdfConformance.PDF);
    }
}
```

**With IronPDF**, the same task takes a couple of lines:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf1 = PdfDocument.FromFile("document1.pdf");
        var pdf2 = PdfDocument.FromFile("document2.pdf");

        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML to PDF in C# with the Nutrient .NET SDK?

Here's how the **Nutrient .NET SDK (GdPicture)** handles this. Note that the HTML loader is file-based, so a string has to be staged to disk first, and HTML rendering needs Chrome or Edge installed:

```csharp
// NuGet: Install-Package GdPicture
using GdPicture14;
using System.IO;

class Program
{
    static void Main()
    {
        var htmlContent = "<html><body><h1>Hello World</h1></body></html>";
        File.WriteAllText("input.html", htmlContent);

        using var converter = new GdPictureDocumentConverter();
        converter.LoadFromFile("input.html", DocumentFormat.DocumentFormatHTML);
        converter.SaveAsPDF("output.pdf");
    }
}
```

**With IronPDF**, an HTML string goes directly through the embedded Chromium:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var htmlContent = "<html><body><h1>Hello World</h1></body></html>";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Add a Watermark?

Here's how the **Nutrient .NET SDK (GdPicture)** handles this — composed from primitive draw calls in a per-page loop:

```csharp
// NuGet: Install-Package GdPicture
using GdPicture14;

class Program
{
    static void Main()
    {
        using var pdf = new GdPicturePDF();
        pdf.LoadFromFile("document.pdf");
        pdf.SetMeasurementUnit(PdfMeasurementUnit.PdfMeasurementUnitPoint);

        var font = pdf.AddStandardFont(PdfStandardFont.PdfStandardFontHelveticaBold);
        int pageCount = pdf.GetPageCount();

        for (int i = 1; i <= pageCount; i++)
        {
            pdf.SelectPage(i);
            pdf.SetFillAlpha(128);  // ~50% (0..255)
            pdf.SetTextSize(48);
            pdf.SetOriginRotationInDegrees(45);
            pdf.DrawTextBox(font, 100, 300, 500, 400, "CONFIDENTIAL",
                PdfHorizontalAlignment.PdfHorizontalAlignmentCenter,
                PdfVerticalAlignment.PdfVerticalAlignmentMiddle);
        }

        pdf.SaveToFile("watermarked.pdf");
    }
}
```

**With IronPDF**, the same task is one HTML/CSS string:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

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

### The Practical Differences

The Nutrient .NET SDK is GdPicture under the hood — broad and toolkit-shaped. Migrating to IronPDF tends to be motivated by:

1. **Single-purpose package**: PDF only, no imaging / scanning / OCR layers to drag in
2. **Published pricing**: per-developer rates on the IronPDF site rather than a sales call
3. **Embedded Chromium**: HTML rendering doesn't depend on a system Chrome / Edge install
4. **One-call watermarks and headers**: HTML/CSS watermarks and `{page}` / `{total-pages}` tokens replace per-page draw loops
5. **Brand and package churn**: PSPDFKit → Nutrient (2024-10-23) plus the GdPicture acquisition means three names to track in older code

### Quick Migration Overview

| Aspect | Nutrient .NET SDK (GdPicture) | IronPDF |
|--------|-------------------------------|---------|
| NuGet | `GdPicture` | `IronPdf` |
| Namespace | `GdPicture14` | `IronPdf` |
| HTML rendering | System Chrome / Edge | Embedded Chromium |
| Pricing | Sales-led | Published |
| Watermarks | Procedural draw calls | `pdf.ApplyWatermark(html, ...)` |
| Headers / Footers | Manual per-page DrawText | `HtmlHeaderFooter` with `{page}` / `{total-pages}` |
| Page indexing | 1-based | 0-based |

### Key API Mappings

| Nutrient .NET SDK (GdPicture) | IronPDF | Notes |
|-------------------------------|---------|-------|
| `new GdPictureDocumentConverter()` | `new ChromePdfRenderer()` | HTML / URL / Office input |
| `new GdPicturePDF()` + `LoadFromFile(path)` | `PdfDocument.FromFile(path)` | PDF object model |
| `converter.LoadFromFile(html, DocumentFormatHTML)` + `SaveAsPDF(out)` | `renderer.RenderHtmlAsPdf(html)` | String input directly |
| `converter.CombineToPDF(paths, out, conformance)` | `PdfDocument.Merge(pdfs)` | — |
| `converter.HtmlPageWidth = 8.27f` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Named sizes |
| `pdf.SetFillAlpha(...) + DrawTextBox(...)` | `pdf.ApplyWatermark(html, rotation, vAlign, hAlign)` | HTML/CSS |
| Per-page `DrawText` loop | `RenderingOptions.HtmlFooter` with `{page}` / `{total-pages}` | Tokens |
| `pdf.SaveToFile(path)` | `pdf.SaveAs(path)` | — |

### Migration Code Example

**Before (Nutrient .NET SDK / GdPicture):**
```csharp
using GdPicture14;
using System.IO;

public class NutrientService
{
    public byte[] GeneratePdf(string html)
    {
        File.WriteAllText("input.html", html);

        using (var converter = new GdPictureDocumentConverter())
        {
            converter.HtmlPageWidth  = 8.27f;
            converter.HtmlPageHeight = 11.69f;
            converter.HtmlMarginTop = 0.78f;
            converter.HtmlMarginBottom = 0.78f;
            converter.HtmlMarginLeft = 0.78f;
            converter.HtmlMarginRight = 0.78f;

            converter.LoadFromFile("input.html", DocumentFormat.DocumentFormatHTML);
            converter.SaveAsPDF("temp.pdf");
        }

        // Watermark each page via primitive draw calls
        using var pdf = new GdPicturePDF();
        pdf.LoadFromFile("temp.pdf");
        var font = pdf.AddStandardFont(PdfStandardFont.PdfStandardFontHelveticaBold);
        int pageCount = pdf.GetPageCount();
        for (int i = 1; i <= pageCount; i++)
        {
            pdf.SelectPage(i);
            pdf.SetFillAlpha(76);
            pdf.SetTextSize(72);
            pdf.SetOriginRotationInDegrees(45);
            pdf.DrawTextBox(font, 100, 300, 500, 400, "DRAFT",
                PdfHorizontalAlignment.PdfHorizontalAlignmentCenter,
                PdfVerticalAlignment.PdfVerticalAlignmentMiddle);
        }

        pdf.SaveToFile("output.pdf");
        return File.ReadAllBytes("output.pdf");
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

1. **HTML input**: GdPicture's HTML loader is file/URL-based; IronPDF accepts strings directly
   ```csharp
   // GdPicture: write to disk, then converter.LoadFromFile(...)
   // IronPDF:   renderer.RenderHtmlAsPdf(html)
   ```

2. **Configuration model**: per-property on the converter vs. on `RenderingOptions`
   ```csharp
   // GdPicture: converter.HtmlPageWidth = 8.27f
   // IronPDF:   renderer.RenderingOptions.PaperSize = PdfPaperSize.A4
   ```

3. **No "processor" object**: IronPDF only needs a `ChromePdfRenderer` for HTML/URL → PDF and `PdfDocument` for PDF manipulation
   ```csharp
   // GdPicture: using var converter = new GdPictureDocumentConverter();
   // IronPDF:   var renderer = new ChromePdfRenderer();
   ```

4. **Watermarks**: procedural draw calls become a single HTML/CSS string
   ```csharp
   // GdPicture: SetFillAlpha + DrawTextBox + OCG markers per page
   // IronPDF:   pdf.ApplyWatermark("<div style='opacity:0.3'>DRAFT</div>", ...)
   ```

5. **Page numbers**: built-in placeholders instead of manual counting
   ```csharp
   // GdPicture: for each page, DrawText "Page i of N"
   // IronPDF:   HtmlFragment = "Page {page} of {total-pages}"
   ```

### NuGet Package Migration

```bash
# Remove the Nutrient .NET SDK (GdPicture) and any legacy PSPDFKit packages
dotnet remove package GdPicture
dotnet remove package GdPicture.WPF
dotnet remove package GdPicture.WinForms

# Install IronPDF
dotnet add package IronPdf
```

### Find All Nutrient References

```bash
# Find GdPicture / Nutrient / PSPDFKit usage
grep -rE "GdPicture14|GdPicturePDF|GdPictureDocumentConverter|PSPDFKit|Nutrient" --include="*.cs" .
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
