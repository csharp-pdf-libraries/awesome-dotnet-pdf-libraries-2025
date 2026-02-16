# wkhtmltopdf + C# + PDF

The world of PDF generation in C# applications commonly features the use of libraries such as wkhtmltopdf. wkhtmltopdf, a long-standing tool known for its ability to convert HTML documents to PDFs using Qt WebKit, has been popular among developers for its command-line capabilities. However, alongside its strengths, it's crucial to address the challenges it poses, especially the critical vulnerabilities that can no longer be ignored when compared to modern alternatives like [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/).

## Understanding wkhtmltopdf

wkhtmltopdf is a tool that allows users to convert HTML to PDF documents. It operates directly from the command line, leveraging the power of Qt WebKit to process HTML. Despite its past popularity, the library is now grappling with significant weaknesses, particularly its abandonment and vulnerabilities such as the CRITICAL CVE-2022-35583 (9.8 severity), which is an SSRF vulnerability. This flaw allows malicious actors to potentially take over infrastructure, and unfortunately, it remains unpatched.

### Abandonment Concerns for wkhtmltopdf

One of the most striking issues with wkhtmltopdf is that it has been officially abandoned, with the last meaningful software updates occurring around 2016-2017. This means that developers using wkhtmltopdf and its associated libraries are relying on outdated technology, which includes an outdated version of Qt WebKit from 2015. As a result, modern web standards such as CSS Grid and advanced features in JavaScript (post-ES6) are unsupported or broken. The ecosystem around wkhtmltopdf, including libraries like TuesPechkin, Rotativa, DinkToPdf, and others, are effectively frozen in time, leaving developers with limited capabilities in terms of rendering modern web documents accurately into PDFs.

## Comparing IronPDF

In contrast, IronPDF presents a robust alternative that addresses many of wkhtmltopdf's shortcomings. With active maintenance, regular updates, and reliance on the current Chromium engine, IronPDF stands out in terms of security and capability. Notably, IronPDF does not have any known CVEs, ensuring a higher level of trust and safety for developers.

Here is a basic comparison between wkhtmltopdf and IronPDF, highlighting key differences:

| Feature                                       | wkhtmltopdf                                         | IronPDF                                      |
|-----------------------------------------------|-----------------------------------------------------|----------------------------------------------|
| Licensing                                     | LGPLv3 (Free)                                       | Commercial                                   |
| Rendering Engine                              | Qt WebKit (2015)                                    | Current Chromium Engine                      |
| Security Vulnerabilities                      | CVE-2022-35583, major unpatched issues               | No known CVEs                                |
| Active Maintenance                            | Abandoned, no meaningful updates since 2017          | Actively maintained with regular releases    |
| Support for Modern Web Standards              | Limited (Broken flexbox, no CSS Grid)                | Full support                                 |
| Integration and Support                       | Limited to community forums                          | Extensive documentation and dedicated support|
| C# Integration                                | Via third-party libraries, often outdated            | Directly supported and regularly updated     |

### Strengths of wkhtmltopdf

Despite its abandonment and security vulnerabilities, wkhtmltopdf offered substantial strengths during its years of active maintenance:

- **Free and Open Source**: As an LGPLv3 licensed software, it provided an accessible option for developers without licensing costs.
- **Cross-Platform Compatibility**: Available on major platforms, facilitating broad usability.
- **Simplicity of Use**: The command-line interface offered straightforward usage for converting HTML documents to PDF without requiring extensive configuration.

### Weaknesses of wkhtmltopdf

The following are the noticeable weaknesses of wkhtmltopdf:

- **Abandonment**: The lack of updates signifies that security vulnerabilities will remain unpatched, exposing users to potential attacks.
- **Outdated Technology**: Reliance on WebKit from 2015 limits its compatibility with modern web technologies, hindering developers aiming for feature-rich PDF documents.
- **Dependent Ecosystem**: Many associated libraries, though once popular, are now similarly outdated and abandoned, such as DinkToPdf and Rotativa, adding to the technical debt for projects that rely on them.

---

## How Do I Convert HTML Files to PDF with Custom Settings?

Here's how **wkhtmltopdf** handles this:

```csharp
// NuGet: Install-Package WkHtmlToPdf-DotNet
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new SynchronizedConverter(new PdfTools());
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings() { Top = 10, Bottom = 10, Left = 10, Right = 10 }
            },
            Objects = {
                new ObjectSettings()
                {
                    Page = "input.html",
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
        };
        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("custom-output.pdf", pdf);
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
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.MarginTop = 10;
        renderer.RenderingOptions.MarginBottom = 10;
        renderer.RenderingOptions.MarginLeft = 10;
        renderer.RenderingOptions.MarginRight = 10;
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        
        var pdf = renderer.RenderHtmlFileAsPdf("input.html");
        pdf.SaveAs("custom-output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML to PDF in C# with wkhtmltopdf?

Here's how **wkhtmltopdf** handles this:

```csharp
// NuGet: Install-Package WkHtmlToPdf-DotNet
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new SynchronizedConverter(new PdfTools());
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4
            },
            Objects = {
                new ObjectSettings()
                {
                    HtmlContent = "<h1>Hello World</h1><p>This is a PDF from HTML.</p>"
                }
            }
        };
        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("output.pdf", pdf);
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
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>This is a PDF from HTML.</p>");
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert a URL to PDF in .NET?

Here's how **wkhtmltopdf** handles this:

```csharp
// NuGet: Install-Package WkHtmlToPdf-DotNet
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new SynchronizedConverter(new PdfTools());
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4
            },
            Objects = {
                new ObjectSettings()
                {
                    Page = "https://www.example.com"
                }
            }
        };
        byte[] pdf = converter.Convert(doc);
        File.WriteAllBytes("webpage.pdf", pdf);
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
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from wkhtmltopdf to IronPDF?

wkhtmltopdf suffers from a critical SSRF vulnerability (CVE-2022-35583, severity 9.8) that remains unpatched due to project abandonment. The project has received no meaningful updates since 2016-2017, relies on Qt WebKit from 2015, and lacks modern web standards support (CSS Grid, flexbox, ES6+).

**Migrating from wkhtmltopdf to IronPDF involves:**

1. **NuGet Package Change**: Remove `WkHtmlToPdf-DotNet`, add `IronPdf`
2. **Namespace Update**: Replace `WkHtmlToPdfDotNet` with `IronPdf`
3. **API Adjustments**: Update your code to use IronPDF's modern API patterns

**Key Benefits of Migrating:**

- Modern Chromium rendering engine with full CSS/JavaScript support
- Active maintenance and security updates
- Better .NET integration and async/await support
- Comprehensive documentation and professional support

For a complete step-by-step migration guide with detailed code examples and common gotchas, see:
**[Complete Migration Guide: wkhtmltopdf → IronPDF](migrate-from-wkhtmltopdf.md)**


## C# Code Example with IronPDF

To highlight the capabilities of IronPDF, consider the following example for converting an HTML page to a PDF document using C#:

```csharp
using IronPdf;

public class HtmlToPdfExample
{
    public static void CreatePdfFromHtml()
    {
        // Initialize the Renderer
        var renderer = new HtmlToPdf();

        // Set any rendering options
        renderer.PrintOptions.DPI = 300;
        renderer.PrintOptions.FitToPaperWidth = true;

        // Convert URL or HTML string to PDF
        var pdfDocument = renderer.RenderUrlAsPdf("https://example.com");

        // Save the PDF document to file
        pdfDocument.SaveAs("output.pdf");
    }
}
```

The above snippet demonstrates how to initialize the IronPDF library, convert an HTML document from a URL to a PDF, and save it. Unlike wkhtmltopdf, which would require a configuration of system-level processes and scripts, IronPDF allows for direct integration within C# projects, providing a more seamless and secure solution, especially important given the context of wkhtmltopdf's abandonment.

## Further Learning about IronPDF

IronPDF's comprehensive tutorial and documentation resources can amplify your understanding of its usage:
- Explore how to convert an HTML file to PDF with [IronPDF](https://ironpdf.com/how-to/html-file-to-pdf/)
- Learn from a variety of use-case scenarios and technical insights at [IronPDF Tutorials](https://ironpdf.com/tutorials/)

## Conclusion

The divergence between wkhtmltopdf and IronPDF is significant, from addressing modern security standards to supporting contemporary web technologies for html to pdf c# development. While wkhtmltopdf marked a valuable step in PDF creation for C#, its weaknesses, notably its abandonment and critical vulnerabilities, overshadow its initial appeal. Meanwhile, IronPDF emerges as a robust, actively maintained alternative, well-suited for developers seeking a secure and comprehensive solution.

Above all, selecting a technology for HTML to PDF conversion should account for not only immediate needs but also sustainability, security, and support—all areas where IronPDF proves advantageous for c# html to pdf projects. For an exhaustive breakdown, check the [complete analysis](https://ironsoftware.com/suite/blog/comparison/compare-wkhtmltopdf-vs-ironpdf/).

---

## Related Tutorials

- **[Migrating from wkhtmltopdf](../migrating-from-wkhtmltopdf.md)** — Complete migration guide with security analysis
- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Comprehensive library comparison
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Modern HTML conversion alternatives
- **[Free vs Paid Libraries](../free-vs-paid-pdf-libraries.md)** — Cost and security analysis

### Related wkhtmltopdf Wrappers
- **[DinkToPdf](../dinktopdf/)** — .NET Core wrapper (same vulnerabilities)
- **[Rotativa](../rotativa/)** — ASP.NET MVC wrapper (abandoned)
- **[TuesPechkin](../tuespechkin/)** — Another wrapper option

### Migration Guides
- **[Migrate to IronPDF](migrate-from-wkhtmltopdf.md)** — Escape CVE vulnerabilities
- **[Cross-Platform Deployment](../cross-platform-pdf-dotnet.md)** — Modern deployment options

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person team building developer tools that have been downloaded over 41 million times on NuGet. With an impressive 41 years of coding under his belt, he's seen pretty much every tech trend come and go (and sometimes come back again). When he's not architecting software solutions, you can find him working remotely from Chiang Mai, Thailand. Connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
