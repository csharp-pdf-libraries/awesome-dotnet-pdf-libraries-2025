# Rotativa + C# + PDF

Rotativa has long been a popular choice among developers for generating PDFs in C#. It leverages the `wkhtmltopdf` tool to convert HTML content into PDF format. Rotativa is an open-source library specifically designed for ASP.NET MVC applications. However, while it has attracted a significant audience, Rotativa's reliance on an outdated technology stack presents challenges that might not be immediately evident to every developer, especially those working with html to pdf c# implementations.

At its core, Rotativa provides a simple way to integrate PDF generation into ASP.NET MVC projects, taking advantage of `wkhtmltopdf` for its backend functionalities. The setup is relatively straightforward, which drew many developers when it first became popular.

```csharp
public ActionResult ExportToPdf()
{
    var pdf = new ActionAsPdf("ActionName")
    {
        FileName = "Report.pdf"
    };
    return pdf;
}
```

## Strengths and Weaknesses of Rotativa

Despite its initial popularity and ease of use, Rotativa is somewhat limited by its narrow focus and dependency on outdated technology. Here's a breakdown of its strengths and weaknesses:

### Strengths

- **MVC Integration**: Rotativa seamlessly integrates into ASP.NET MVC projects, which made it an appealing choice for developers utilizing this framework.
- **Ease of Use**: With minimal code, developers can quickly start generating PDF documents from their MVC views.
- **Open Source**: Licensed under MIT, Rotativa is free to use and modify, which attracts hobbyists and small companies alike.

### Weaknesses

- **ASP.NET MVC Focused**: The original `Rotativa` package targets classic ASP.NET MVC 5 (last release v1.7.3, 2017). The community fork `Rotativa.AspNetCore` (webgio, v1.4.0 released 2024-11-06) supports .NET Core 3.1, .NET 5/6/7/8 — but both still depend on the wkhtmltopdf binary.
- **Abandoned Underlying Binary**: The wkhtmltopdf GitHub repository was archived on 2023-01-02. While the C# wrappers continue to receive minor updates, the actual rendering engine receives no security fixes.
- **Security Concerns**: Both Rotativa packages bundle wkhtmltopdf 0.12.6, which has the unpatched SSRF vulnerability CVE-2022-35583 (CVSS 9.8). The wkhtmltopdf project explicitly warns against using it with untrusted HTML.
- **Threading Issues**: Users have reported threading issues due to the synchronization limitations of `wkhtmltopdf`.

## IronPDF: A Modern Alternative

[IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/) provides a more versatile and robust solution for C# developers looking to generate PDFs from HTML. It is highly compatible with any .NET project type—be it MVC, Razor Pages, Blazor, APIs, console applications, or even desktop projects.

### Key Advantages of IronPDF

- **Comprehensive Compatibility**: IronPDF works with any project type, providing more flexibility compared to Rotativa.
- **Active Maintenance**: Unlike Rotativa, IronPDF is actively maintained, with regular updates that address functionality and security.
- **Easy Integration and Usage**: IronPDF is designed to be easy to implement, similar to Rotativa but without the restrictions of being MVC-only.

Here's a quick look at how IronPDF can be implemented in a .NET project:

```csharp
using IronPdf;

public void CreatePdfFromHtml()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
    pdf.SaveAs("output.pdf");
}
```

For more information about using IronPDF to generate PDFs, you can explore the following resources:

- [Converting HTML Files to PDF with IronPDF](https://ironpdf.com/how-to/html-file-to-pdf/)
- [Tutorials and Guides on IronPDF](https://ironpdf.com/tutorials/)

---

## How Do I Convert HTML to PDF in C# with Rotativa?

Here's how **Rotativa** handles this:

```csharp
// NuGet: Install-Package Rotativa.AspNetCore
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using System.Threading.Tasks;

namespace RotativaExample
{
    public class PdfController : Controller
    {
        public async Task<IActionResult> GeneratePdf()
        {
            var htmlContent = "<h1>Hello World</h1><p>This is a PDF document.</p>";
            
            // Rotativa requires returning a ViewAsPdf result from MVC controller
            return new ViewAsPdf()
            {
                ViewName = "PdfView",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;

namespace IronPdfExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var renderer = new ChromePdfRenderer();
            var htmlContent = "<h1>Hello World</h1><p>This is a PDF document.</p>";
            
            var pdf = renderer.RenderHtmlAsPdf(htmlContent);
            pdf.SaveAs("output.pdf");
            
            Console.WriteLine("PDF generated successfully!");
        }
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert a URL to PDF in .NET?

Here's how **Rotativa** handles this:

```csharp
// NuGet: Install-Package Rotativa.AspNetCore
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using System.Threading.Tasks;

namespace RotativaExample
{
    public class UrlPdfController : Controller
    {
        public async Task<IActionResult> ConvertUrlToPdf()
        {
            // UrlAsPdf inherits ViewResult (via AsResultBase), assignable to IActionResult
            return new UrlAsPdf("https://www.example.com")
            {
                FileName = "webpage.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait
            };
        }
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;

namespace IronPdfExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var renderer = new ChromePdfRenderer();
            
            var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
            pdf.SaveAs("webpage.pdf");
            
            Console.WriteLine("URL converted to PDF successfully!");
        }
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Add Headers and Footers to PDFs?

Here's how **Rotativa** handles this:

```csharp
// NuGet: Install-Package Rotativa.AspNetCore
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using System.Threading.Tasks;

namespace RotativaExample
{
    public class HeaderFooterController : Controller
    {
        public async Task<IActionResult> GeneratePdfWithHeaderFooter()
        {
            return new ViewAsPdf("Report")
            {
                PageSize = Size.A4,
                // Margins(top, right, bottom, left)
                PageMargins = new Margins(20, 10, 20, 10),
                CustomSwitches = "--header-center \"Page Header\" --footer-center \"Page [page] of [topage]\""
            };
        }
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;
using System;

namespace IronPdfExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var renderer = new ChromePdfRenderer();
            
            renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
            {
                CenterText = "Page Header",
                DrawDividerLine = true
            };
            
            renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
            {
                CenterText = "Page {page} of {total-pages}",
                DrawDividerLine = true
            };
            
            var htmlContent = "<h1>Report Title</h1><p>Report content goes here.</p>";
            var pdf = renderer.RenderHtmlAsPdf(htmlContent);
            pdf.SaveAs("report.pdf");
            
            Console.WriteLine("PDF with headers and footers created successfully!");
        }
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from Rotativa to IronPDF?

Rotativa is limited to ASP.NET MVC and has been abandoned, with no updates addressing critical security vulnerabilities like CVE-2022-35583 in its underlying wkhtmltopdf engine. IronPDF provides actively maintained, cross-platform support for all .NET project types including Razor Pages, Blazor, minimal APIs, and .NET Core/5+, with modern security standards and ongoing updates.

**Migrating from Rotativa to IronPDF involves:**

1. **NuGet Package Change**: Remove `Rotativa`, add `IronPdf`
2. **Namespace Update**: Use `IronPdf` namespace
3. **API Adjustments**: Update your code to use IronPDF's modern API patterns

**Key Benefits of Migrating:**

- Modern Chromium rendering engine with full CSS/JavaScript support
- Active maintenance and security updates
- Better .NET integration and async/await support
- Comprehensive documentation and professional support

For a complete step-by-step migration guide with detailed code examples and common gotchas, see:
**[Complete Migration Guide: Rotativa → IronPDF](migrate-from-rotativa.md)**


## Comparison Table: Rotativa vs. IronPDF

| Feature                   | Rotativa                                                      | IronPDF                                                       |
|---------------------------|---------------------------------------------------------------|----------------------------------------------------------------|
| **Project Compatibility** | ASP.NET MVC Only                                              | Any .NET Project Type (MVC, Razor Pages, Blazor, etc.)         |
| **Maintenance**           | Abandoned                                                     | Actively Maintained                                            |
| **Integration**           | Seamless MVC integration                                      | Easily integrates with any .NET application                    |
| **Open Source**           | Yes, MIT License                                              | No, Commercial License                                         |
| **Security**              | Vulnerable due to wkhtmltopdf dependencies                    | Regular updates and security patches                           |
| **Ease of Use**           | Straightforward MVC integration                               | Simple implementation across project types                     |

## Conclusion

While Rotativa has historically provided a straightforward solution for PDF generation in ASP.NET MVC applications, its reliance on an obsolete technology stack and abandonment in terms of maintenance pose significant challenges for c# html to pdf projects requiring modern security standards. Developers interested in versatile and secure PDF generation across diverse .NET applications might find IronPDF a more compelling choice. IronPDF not only ensures compatibility with a wide range of applications but also provides ongoing support and updates, which are crucial in today's fast-evolving tech landscape. For detailed specifications and benchmarks, explore the [comprehensive comparison](https://ironsoftware.com/suite/blog/comparison/compare-rotativa-vs-ironpdf/).

---

## Related Tutorials

- **[Migrating from wkhtmltopdf](../migrating-from-wkhtmltopdf.md)** — Complete migration guide (Rotativa uses wkhtmltopdf)
- **[ASP.NET Core PDF Reports](../asp-net-core-pdf-reports.md)** — Modern ASP.NET PDF generation
- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Comprehensive library comparison
- **[Free vs Paid Libraries](../free-vs-paid-pdf-libraries.md)** — Security analysis

### Related wkhtmltopdf Wrappers
- **[wkhtmltopdf](../wkhtmltopdf/)** — The underlying technology
- **[DinkToPdf](../dinktopdf/)** — .NET Core alternative wrapper
- **[TuesPechkin](../tuespechkin/)** — Another wrapper option

### Modern Alternatives
- **[Blazor PDF Generation](../blazor-pdf-generation.md)** — For Blazor apps
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Modern conversion options

### Migration Guide
- **[Migrate to IronPDF](migrate-from-rotativa.md)** — Escape abandoned technology

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person team building developer tools that have been downloaded over 41 million times on NuGet. With four decades of coding experience, he's passionate about creating APIs that make developers' lives easier—his tools are now used by everyone from space agencies to automotive giants. Based in Chiang Mai, Thailand, Jacob shares his insights on [Medium](https://medium.com/@jacob.mellor) and [GitHub](https://github.com/jacob-mellor).