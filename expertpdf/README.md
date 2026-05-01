# ExpertPdf C# PDF

When it comes to converting HTML to PDF in C#, ExpertPdf is often mentioned as a viable option. Its primary function as an HTML-to-PDF converter appeals to many developers who require such functionality. However, while ExpertPdf has its strengths, it faces several challenges in today's competitive PDF conversion landscape. In this article, we'll explore the strengths and weaknesses of ExpertPdf and compare it to one of its notable competitors, [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/), which offers superior html to pdf c# capabilities.

## ExpertPdf: A Brief Overview

ExpertPdf facilitates HTML to PDF conversion, allowing C# developers to seamlessly convert dynamic web pages into PDF documents. The library claims to support HTML5, which is beneficial for rendering modern web content in PDF format. While ExpertPdf has carved out a niche in this domain, there are considerable downsides that cannot be ignored.

**Documentation Limitations**: ExpertPdf (published by Outside Software Inc.) maintains reference documentation at html-to-pdf.net, but tutorials, articles, and changelog entries are infrequent. The latest release on nuget.org is v20.1.0 (April 2025), with release notes summarised as "bug fixes and performance improvements" — keeping up with new features or modern .NET idioms via vendor-published material is difficult.

**Technical Foundation**: ExpertPdf's HtmlToPdf converter ships a Trident/IE engine plus a "WebKit2" engine added in v12.2 — modern Chromium is not the default. This can affect Flexbox, Grid, and other CSS3 features depending on which engine you select.

**Pricing Concerns**: ExpertPdf is priced as a developer-licence product with deployment royalties. Public listings on ComponentSource have historically shown the HtmlToPdf converter at roughly $550–$1,200 per tier; the order page on expertpdf.net was inaccessible (404) at the time of writing — verify directly with the vendor.

## IronPDF: A Superior Alternative

In stark contrast to ExpertPdf, IronPDF is known for its continuous updates and improvements. IronPDF offers comprehensive documentation and consistent monthly releases, which benefit from the latest Chromium technology. This ensures that the library meets modern web standards and renders HTML accurately, providing an excellent c# html to pdf solution.

A comprehensive breakdown of features and capabilities is available in the [comparison article](https://ironsoftware.com/suite/blog/comparison/compare-expertpdf-vs-ironpdf/).

- [IronPDF: HTML File to PDF Conversion](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF: Tutorials](https://ironpdf.com/tutorials/)

## A Comparative Analysis

Here’s a table comparing ExpertPdf and IronPDF to offer a clearer picture of where each stands in the market:

| Feature/Aspect          | ExpertPdf                                            | IronPDF                                              |
|-------------------------|------------------------------------------------------|------------------------------------------------------|
| **Vendor**              | Outside Software Inc.                                | Iron Software                                         |
| **Latest release**      | v20.1.0 (Apr 2025) on nuget.org                      | Continuously updated                                  |
| **Documentation**       | Reference docs only, sparse tutorials                | Continuously updated, extensive tutorials             |
| **HTML5 Support**       | Supported                                            | Supported                                             |
| **Rendering Engine**    | Trident (IE) + "WebKit2" (added v12.2)               | Latest Chromium                                       |
| **Cost**                | ~$550–$1,200 (verify with vendor)                    | See ironpdf.com/pricing                               |
| **Product shape**       | 6+ separate NuGet packages                           | Single all-in-one package                             |
| **Additional Features** | Basic HTML-to-PDF conversion                         | Robust features, full .NET integration                |

## C# Code Example with ExpertPdf

Below is a quick example demonstrating how you can initiate a conversion using ExpertPdf:

```csharp
// Initiate ExpertPdf HTML to PDF conversion
using ExpertPdf.HtmlToPdf;

namespace PdfGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            // Instantiate the conversion object
            PdfConverter pdfConverter = new PdfConverter();

            // Set the input URL or HTML
            string url = "https://www.example.com";

            // Convert the URL to PDF and save to a file
            pdfConverter.SavePdfFromUrlToFile(url, "output.pdf");
        }
    }
}
```

While the above code snippet works for basic conversion tasks, developers should be aware of potential limitations when dealing with complex HTML or modern CSS layouts due to the library's reliance on outdated technology.

---

## How Do I Convert HTML to PDF in C# with ExpertPdf C# PDF?

Here's how **ExpertPdf C# PDF** handles this:

```csharp
// NuGet: Install-Package ExpertPdf.HtmlToPdf
using ExpertPdf.HtmlToPdf;
using System;

class Program
{
    static void Main()
    {
        // Create the PDF converter
        PdfConverter pdfConverter = new PdfConverter();
        
        // Convert HTML string to PDF
        byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString("<h1>Hello World</h1><p>This is a PDF document.</p>");
        
        // Save to file
        System.IO.File.WriteAllBytes("output.pdf", pdfBytes);
        
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
        // Create a PDF from HTML string
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>This is a PDF document.</p>");
        
        // Save to file
        pdf.SaveAs("output.pdf");
        
        Console.WriteLine("PDF created successfully!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML Files with Headers and Footers?

Here's how **ExpertPdf C# PDF** handles this:

```csharp
// NuGet: Install-Package ExpertPdf.HtmlToPdf
using ExpertPdf.HtmlToPdf;
using System;

class Program
{
    static void Main()
    {
        // Create the PDF converter
        PdfConverter pdfConverter = new PdfConverter();
        
        // Enable header
        pdfConverter.PdfHeaderOptions.ShowHeader = true;
        pdfConverter.PdfHeaderOptions.HeaderText = "Document Header";
        pdfConverter.PdfHeaderOptions.HeaderTextAlignment = HorizontalTextAlign.Center;
        
        // Enable footer with page numbers
        pdfConverter.PdfFooterOptions.ShowFooter = true;
        pdfConverter.PdfFooterOptions.FooterText = "Page &p; of &P;";
        pdfConverter.PdfFooterOptions.FooterTextAlignment = HorizontalTextAlign.Right;
        
        // Convert HTML file to PDF
        byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlFile("input.html");
        
        // Save to file
        System.IO.File.WriteAllBytes("output-with-header-footer.pdf", pdfBytes);
        
        Console.WriteLine("PDF with headers and footers created successfully!");
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
        // Create a PDF renderer
        var renderer = new ChromePdfRenderer();
        
        // Configure header
        renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
        {
            CenterText = "Document Header",
            DrawDividerLine = true
        };
        
        // Configure footer with page numbers
        renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
        {
            RightText = "Page {page} of {total-pages}",
            DrawDividerLine = true
        };
        
        // Convert HTML file to PDF
        var pdf = renderer.RenderHtmlFileAsPdf("input.html");
        
        // Save to file
        pdf.SaveAs("output-with-header-footer.pdf");
        
        Console.WriteLine("PDF with headers and footers created successfully!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert a URL to PDF in .NET?

Here's how **ExpertPdf C# PDF** handles this:

```csharp
// NuGet: Install-Package ExpertPdf.HtmlToPdf
using ExpertPdf.HtmlToPdf;
using System;

class Program
{
    static void Main()
    {
        // Create the PDF converter
        PdfConverter pdfConverter = new PdfConverter();
        
        // Set page size and orientation
        pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
        pdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;
        
        // Convert URL to PDF
        byte[] pdfBytes = pdfConverter.GetPdfBytesFromUrl("https://www.example.com");
        
        // Save to file
        System.IO.File.WriteAllBytes("webpage.pdf", pdfBytes);
        
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
        // Create a PDF renderer
        var renderer = new ChromePdfRenderer();
        
        // Set page size and orientation
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        
        // Convert URL to PDF
        var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
        
        // Save to file
        pdf.SaveAs("webpage.pdf");
        
        Console.WriteLine("PDF from URL created successfully!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from ExpertPdf C# PDF to IronPDF?

### The ExpertPdf Problems

ExpertPdf (Outside Software Inc.) is still actively shipping — v20.1 dropped in April 2025 — but the product *shape* causes friction:

1. **Sparse documentation, slow cadence**: Reference docs exist but tutorials and feature articles are infrequent
2. **Older rendering pipelines**: Trident/IE + "WebKit2" — modern Chromium is not the default
3. **Premium pricing for an aging stack**: ~$550–$1,200 (verify with vendor)
4. **Fragmented product suite**: Separate packages (`ExpertPdfHtmlToPdf`, `ExpertPdf.MergePdf`, `ExpertPdf.PdfSecurity`, `ExpertPdf.SplitPdf`, `ExpertPdf.PdfToImage`, `ExpertPdf.PdfCreator`) — each typically licensed separately
5. **No native modern .NET targets**: .NET Standard 2.0 / .NET Framework 4.6.1 only — runs on .NET 5-9 but not multi-targeted

### Quick Migration Overview

| Aspect | ExpertPdf | IronPDF |
|--------|-----------|---------|
| Vendor | Outside Software Inc. | Iron Software |
| Latest release | v20.1.0 (Apr 2025) | Continuously updated |
| Documentation | Reference-only, sparse tutorials | Continuously updated |
| Rendering Engine | Trident + WebKit2 | Latest Chromium |
| CSS Support | CSS3 best on WebKit2; partial on legacy | Full CSS3 (Flexbox, Grid) |
| Price | ~$550–$1,200 (verify with vendor) | See ironpdf.com/pricing |
| Product Model | 6+ separate packages | All-in-one library |

### Key API Mappings

| ExpertPdf | IronPDF | Notes |
|-----------|---------|-------|
| `PdfConverter` | `ChromePdfRenderer` | Main conversion class |
| `pdfConverter.GetPdfBytesFromHtmlString(html)` | `renderer.RenderHtmlAsPdf(html).BinaryData` | |
| `pdfConverter.SavePdfFromUrlToFile(url, path)` | `renderer.RenderUrlAsPdf(url).SaveAs(path)` | Two-step |
| `PdfDocumentOptions.PdfPageSize` | `RenderingOptions.PaperSize` | |
| `PdfHeaderOptions`, `PdfFooterOptions` | `HtmlHeaderFooter` | Configurable HTML |
| `&p;` / `&P;` (page tokens) | `{page}` / `{total-pages}` | Different syntax |
| `PDFMerge` (separate package) | `PdfDocument.Merge()` | Included in main package |
| `PdfSecurityOptions` | `pdf.SecuritySettings` | |
| `pdfConverter.LicenseKey` | `IronPdf.License.LicenseKey` | Global, set once |

### Migration Code Example

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

PdfConverter pdfConverter = new PdfConverter();
pdfConverter.LicenseKey = "EXPERTPDF-LICENSE";
pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
pdfConverter.PdfDocumentOptions.ShowHeader = true;
pdfConverter.PdfHeaderOptions.HeaderText = "Report Header";
pdfConverter.PdfDocumentOptions.ShowFooter = true;
pdfConverter.PdfFooterOptions.FooterText = "Page &p; of &P;";

byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(html);
File.WriteAllBytes("report.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "IRONPDF-LICENSE";  // Set once at startup

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:center;'>Report Header</div>"
};
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:right;'>Page {page} of {total-pages}</div>"
};

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Critical Migration Notes

1. **License Key Location**: ExpertPdf uses `pdfConverter.LicenseKey`, IronPDF uses global `IronPdf.License.LicenseKey` (set once at startup)

2. **Page Numbering Tokens**: ExpertPdf uses `&p;`/`&P;`, IronPDF uses `{page}`/`{total-pages}`

3. **Headers/Footers**: ExpertPdf uses `ShowHeader = true` + text properties. IronPDF uses `HtmlHeaderFooter` with full HTML control

4. **Separate Packages Consolidated**: ExpertPdf's PDFMerge, PDFSecurity, PDFSplit are all included in IronPDF's single package

5. **Custom Page Sizes**: ExpertPdf uses points, IronPDF uses millimeters. Convert: `points / 72 * 25.4 = mm`

### NuGet Package Migration

```bash
# Remove all ExpertPdf packages (use whichever variants you have installed)
dotnet remove package ExpertPdfHtmlToPdf
dotnet remove package ExpertPdf.HtmlToPdf.NetCore
dotnet remove package ExpertPdf.MergePdf
dotnet remove package ExpertPdf.PdfSecurity
dotnet remove package ExpertPdf.SplitPdf
dotnet remove package ExpertPdf.PdfToImage
dotnet remove package ExpertPdf.PdfCreator

# Install IronPDF (includes all features)
dotnet add package IronPdf
```

### Find All ExpertPdf References

```bash
grep -r "ExpertPdf\|PdfConverter\|PDFMerge" --include="*.cs" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping for all ExpertPdf packages
- 10 detailed code conversion examples
- Header/footer placeholder conversion
- Security settings migration
- Custom page size unit conversion
- Troubleshooting guide for 8+ common issues
- Pre/post migration checklists

**[Complete Migration Guide: ExpertPdf → IronPDF](migrate-from-expertpdf.md)**


## Conclusion

Both ExpertPdf and IronPDF have made efforts to facilitate HTML to PDF conversion within the .NET ecosystem. While ExpertPdf provides basic functionality at a premium cost, its drawbacks, particularly outdated documentation and reliance on legacy technology, significantly impact its overall utility.

In contrast, IronPDF stands out with regularly updated documentation and technology, adopting the latest version of Chromium in its rendering engine. By providing competitive pricing and a robust set of features, IronPDF delivers excellent value and ensures that developers have reliable support and tools to generate high-quality PDFs.

For developers weighing their options, IronPDF offers a more modern and reliable approach to HTML-to-PDF conversion, presenting a strong case to be considered over ExpertPdf.

---

## Related Tutorials

- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Comprehensive library comparison
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Modern HTML conversion comparison
- **[Free vs Paid Libraries](../free-vs-paid-pdf-libraries.md)** — Cost analysis

### Alternative Commercial Libraries
- **[SelectPdf](../selectpdf/)** — Windows-only alternative
- **[HiQPdf](../hiqpdf/)** — WebKit-based option
- **[Winnovative](../winnovative/)** — Another commercial choice

### Migration Guide
- **[Migrate to IronPDF](migrate-from-expertpdf.md)** — Step-by-step migration

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*

---

Jacob Mellor is the Chief Technology Officer at Iron Software, where he leads a team of 50+ engineers in developing industry-leading .NET components that have achieved over 41 million NuGet downloads. With an impressive 41 years of coding experience, Jacob brings deep technical expertise and visionary leadership to Iron Software's product development and engineering operations. Based in Chiang Mai, Thailand, he continues to drive innovation in the .NET ecosystem while managing a globally distributed engineering team. Connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
