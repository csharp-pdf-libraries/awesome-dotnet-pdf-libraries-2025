
# SelectPdf + C# + PDF

When it comes to HTML-to-PDF conversion tools, SelectPdf is a frequently considered option within the C# development ecosystem. SelectPdf is known for its commercial viability in converting HTML documents to PDFs efficiently. However, an often-debated topic among developers is how SelectPdf stacks up against other competitors in the field, notably [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/). This article will dissect the characteristics, strengths, and weaknesses of SelectPdf, offering a deep dive comparison with IronPDF.

## Introduction to SelectPdf

SelectPdf is a commercial library designed to convert HTML content into PDFs using C#. The library is tailored towards developers who require seamless integration of PDF generation functionality within their applications. Embedding SelectPdf in a project allows C# developers to execute on-the-fly HTML to PDF conversions. The strength of SelectPdf lies in its simple API, making it an appealing option for those new to PDF generation.

While SelectPdf boasts several remarkable features, potential users must also be aware of its limitations — and that the library now comes in two forms. The classic `Select.Pdf` and `Select.HtmlToPdf` packages, which most existing projects reference, remain Windows-only: the vendor's own documentation states that all four of their rendering engines require Windows, ruling out Linux, macOS, and Xamarin. SelectPdf.Universal, first released in October 2026, is the cross-platform answer and runs on Windows, Linux, macOS (Apple Silicon only), and Docker — but adopting it is itself a migration, since the namespace changes, `System.Drawing` types are replaced, direct printing and conversion events are dropped, and the vendor puts API carry-over at 82 percent. The free Community Edition is capped at 5 pages per PDF, and the classic library still defaults to an internal WebKit engine whose JavaScript support stops at ES5.

Below is a simple C# code example demonstrating how SelectPdf is typically used:

```csharp
using SelectPdf;

class Program
{
    static void Main()
    {
        // HTML content to be converted
        string htmlString = "<h1>Welcome to SelectPdf</h1><p>This is a sample PDF document.</p>";

        // Create a new HTML to PDF converter
        HtmlToPdf converter = new HtmlToPdf();

        // Convert the HTML string to a PDF document
        PdfDocument doc = converter.ConvertHtmlString(htmlString);

        // Save the document to disk
        doc.Save("Sample.pdf");

        // Close the document
        doc.Close();

        System.Console.WriteLine("PDF Created successfully.");
    }
}
```

## SelectPdf vs. IronPDF: A Direct Comparison

So, how does SelectPdf compare to IronPDF, another acclaimed HTML to PDF library? IronPDF is worthy of careful examination due to its comprehensive cross-platform capabilities, fully supported modern web standards, and transparent pricing model.

### Flexibility and Platform Support

The most glaring contrast between SelectPdf and IronPDF lies in how recently each arrived at cross-platform support. The classic SelectPdf packages confine their functionality strictly to Windows environments, which starkly impacts developers who aim to leverage cross-platform environments like Linux, Docker, Azure, and AWS for their applications. SelectPdf.Universal lifts that restriction, but it has been available only since October 2026, and on macOS it covers Apple Silicon alone; Alpine and other musl-based images need a separate engine container rather than the in-process library.

On the other hand, [IronPDF](https://ironpdf.com/how-to/html-file-to-pdf/) offers truly cross-platform deployment capabilities. The library is flexible enough to function across more than ten Linux distributions as well as cloud services, making it a more versatile choice for developers looking for broader deployment possibilities.

### Compatibility with Modern Web Standards

The classic SelectPdf library defaults to an internal WebKit engine whose JavaScript support stops at ES5, which can cause issues with newer CSS3 features such as CSS Grid, the Flexbox `gap` property, and modern `@media` queries; reaching current standards there means opting into its Blink or Chromium engine, each shipped as a separate package. SelectPdf.Universal removes the choice and renders with Chromium 154 only. Meanwhile, IronPDF utilizes a current Chromium rendering engine by default, ensuring consistent rendering of complex web designs and layouts with no engine selection to manage.

### Pricing and Free Version

SelectPdf's commercial model begins at $499 for a single developer / single deployment machine and scales to $1,599 for an Enterprise OEM license (verified at selectpdf.com/pricing). All licenses are perpetual but include only one year of maintenance and updates — annual renewal is required to keep receiving upgrades. The Community Edition allows PDFs up to 5 pages and is free for commercial use; the watermark that curtails evaluation belongs instead to the trial of the commercial library, which has no page cap.

Contrastingly, [IronPDF's pricing model](https://ironpdf.com/tutorials/) is more transparent and does not deter users with restrictive features on its free version. IronPDF’s flexible pricing approach is tailored to accommodate different developer needs, ranging from individuals to enterprise-level solutions.

### Support for .NET Features

For developers working with different versions of .NET, both SelectPdf and IronPDF extend support across modern .NET. SelectPdf's classic NetCore package targets .NET Standard 2.0 and is documented as compatible with .NET 5 through .NET 10, but only on Windows — "NetCore" names the framework, not the operating system, which neutralises that compatibility for any team deploying to Linux containers, AWS Lambda, or Linux-based Azure App Service. SelectPdf.Universal targets .NET Standard 2.0, .NET 8, and .NET 10 across all three operating systems. IronPDF supports the same .NET versions and runs on Windows, Linux, macOS, and Docker.

### Comparison Table

Below is a comparison table summarizing key differences and similarities between the classic SelectPdf packages and IronPDF:

| Feature                            | SelectPdf (classic)              | IronPDF                           |
|------------------------------------|----------------------------------|-----------------------------------|
| Platform Support                   | Windows only                     | Full cross-platform, 10+ distros  |
| Modern Web Standards Support       | WebKit by default (ES5 JavaScript); Blink and Chromium engines as separate packages | Full CSS3, current Chromium       |
| Maximum Free Version Page Limit    | 5 pages (Community Edition, no watermark) | Flexible, no hard limit specified |
| Pricing                            | $499 Single Dev to $1,599 Enterprise OEM | Transparent and flexible pricing |
| .NET 10 Support                    | Yes, Windows only                | Yes, all platforms                |
| Linux / Docker / AWS Lambda        | Not supported                    | Fully supported                   |

And the same comparison against SelectPdf.Universal, the cross-platform library the vendor released in October 2026:

| Feature                            | SelectPdf.Universal (26.5)       | IronPDF                           |
|------------------------------------|----------------------------------|-----------------------------------|
| Platform Support                   | Windows, Linux, macOS (Apple Silicon), Docker | Full cross-platform, 10+ distros  |
| Modern Web Standards Support       | Chromium 154 only                | Full CSS3, current Chromium       |
| Maximum Free Version Page Limit    | 5 pages (Community Edition, no watermark) | Flexible, no hard limit specified |
| Pricing                            | Same licence as classic, $499 to $1,599 | Transparent and flexible pricing |
| .NET 10 Support                    | Yes, all three platforms         | Yes, all platforms                |
| Linux / Docker / AWS Lambda        | Supported since October 2026; Alpine needs a separate engine container | Fully supported                   |

---

## How Do I Set Custom Page Settings in PDFs?

Here's how **SelectPdf** handles this:

```csharp
// NuGet: Install-Package Select.HtmlToPdf
using SelectPdf;
using System;

class Program
{
    static void Main()
    {
        HtmlToPdf converter = new HtmlToPdf();
        
        converter.Options.PdfPageSize = PdfPageSize.A4;
        converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
        converter.Options.MarginTop = 20;
        converter.Options.MarginBottom = 20;
        converter.Options.MarginLeft = 20;
        converter.Options.MarginRight = 20;
        
        string html = "<html><body><h1>Custom Page Settings</h1></body></html>";
        PdfDocument doc = converter.ConvertHtmlString(html);
        doc.Save("custom-settings.pdf");
        doc.Close();
        
        Console.WriteLine("PDF with custom settings created");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Engines.Chrome;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.MarginLeft = 20;
        renderer.RenderingOptions.MarginRight = 20;
        
        string html = "<html><body><h1>Custom Page Settings</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("custom-settings.pdf");
        
        Console.WriteLine("PDF with custom settings created");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML to PDF in C# with SelectPdf?

Here's how **SelectPdf** handles this:

```csharp
// NuGet: Install-Package Select.HtmlToPdf
using SelectPdf;
using System;

class Program
{
    static void Main()
    {
        HtmlToPdf converter = new HtmlToPdf();
        PdfDocument doc = converter.ConvertUrl("https://www.example.com");
        doc.Save("output.pdf");
        doc.Close();
        
        Console.WriteLine("PDF created successfully");
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
        pdf.SaveAs("output.pdf");
        
        Console.WriteLine("PDF created successfully");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert an HTML String to PDF?

Here's how **SelectPdf** handles this:

```csharp
// NuGet: Install-Package Select.HtmlToPdf
using SelectPdf;
using System;

class Program
{
    static void Main()
    {
        string htmlContent = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";
        
        HtmlToPdf converter = new HtmlToPdf();
        PdfDocument doc = converter.ConvertHtmlString(htmlContent);
        doc.Save("document.pdf");
        doc.Close();
        
        Console.WriteLine("PDF generated from HTML string");
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
        string htmlContent = "<html><body><h1>Hello World</h1><p>This is a PDF document.</p></body></html>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("document.pdf");
        
        Console.WriteLine("PDF generated from HTML string");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from SelectPdf to IronPDF?

The classic `Select.HtmlToPdf` package requires Windows — the vendor's documentation states that all four of its rendering engines do — making it unsuitable for Linux container or AWS Lambda deployments, and its default WebKit engine stops at ES5 JavaScript. SelectPdf.Universal has covered Linux, macOS, and Docker since October 2026, but moving to it changes the namespace and drops direct printing and conversion events, so a team facing that rewrite is already choosing a library rather than upgrading one.

**Migrating from SelectPdf to IronPDF involves:**

1. **NuGet Package Change**: Remove `Select.HtmlToPdf`, add `IronPdf`
2. **Namespace Update**: Replace `SelectPdf` with `IronPdf`
3. **API Adjustments**: Update your code to use IronPDF's modern API patterns

**Key Benefits of Migrating:**

- Modern Chromium rendering engine with full CSS/JavaScript support
- Active maintenance and security updates
- Better .NET integration and async/await support
- Comprehensive documentation and professional support

For a complete step-by-step migration guide with detailed code examples and common gotchas, see:
**[Complete Migration Guide: SelectPdf → IronPDF](migrate-from-selectpdf.md)**


## Conclusion

While SelectPdf is a potent HTML-to-PDF conversion library with a straightforward API, its limitations in platform flexibility and outdated web standard support may deter some developers working on html to pdf c# projects requiring modern CSS features. Developers seeking robust, modern, and cross-platform solutions are likely to lean towards IronPDF, especially given its extensive support for contemporary web standards and deployment environments for c# html to pdf scenarios, coupled with its flexible and clear pricing. Explore the [detailed feature comparison](https://ironsoftware.com/suite/blog/comparison/compare-selectpdf-vs-ironpdf/) for technical specifications.

---

## Related Tutorials

- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Comprehensive library comparison
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Modern HTML conversion comparison
- **[Free vs Paid Libraries](../free-vs-paid-pdf-libraries.md)** — Cost analysis
- **[Cross-Platform Deployment](../cross-platform-pdf-dotnet.md)** — Linux, Docker, Cloud options

### Alternative Commercial Libraries
- **[Aspose.PDF](../asposepdf/)** — Enterprise alternative
- **[HiQPdf](../hiqpdf/)** — Another commercial option
- **[Winnovative](../winnovative/)** — Windows-focused alternative

### Migration Guide
- **[Migrate to IronPDF](migrate-from-selectpdf.md)** — Step-by-step migration

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*

---

Jacob Mellor is the CTO of [Iron Software](https://www.linkedin.com/in/jacob-mellor-iron-software/), where he leads a talented team of 50+ developers creating powerful PDF and document processing tools for .NET. With an impressive 41 years of coding experience under his belt, Jacob brings deep technical expertise and a passion for building software that makes developers' lives easier. When he's not architecting innovative solutions that have achieved over 41 million NuGet downloads, you can find him working remotely from his base in Chiang Mai, Thailand.

