# PeachPDF + C# + PDF

PeachPDF is a relatively new entrant in the .NET ecosystem designed for developers who need to convert HTML to PDF. As a library, PeachPDF promises a pure .NET implementation, setting itself apart by not relying on external processes, ensuring it can be seamlessly integrated across platforms where .NET is supported. This characteristic positions PeachPDF as an appealing choice for projects looking for a lightweight, managed library solution. Despite its potential, PeachPDF is still in development, highlighting both exciting possibilities and notable limitations.

PeachPDF remains enticing because of its pure .NET core, which promises straightforward deployment in diverse environments. Unlike some PDF generation tools that require interfacing with native code or platform-specific APIs, PeachPDF's managed approach fosters greater compatibility and ease of maintenance. However, it also translates into limited adoption, with a smaller user base and community-driven support. For developers seeking robust HTML-to-PDF functionality in .NET, this makes PeachPDF a consideration with caveats, particularly when compared to more established libraries like IronPDF.

## C# Code Example Using PeachPDF

Below is a simple illustration of how PeachPDF can be used in a C# project to convert HTML to PDF. The library exposes a `PdfGenerator` whose async `GeneratePdf` method takes an HTML string and a `PdfGenerateConfig`, returning a document you save to a stream:

```csharp
using PeachPDF;
using PeachPDF.PdfSharpCore;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var html = "<html><body><h1>Hello, World!</h1></body></html>";

        var config = new PdfGenerateConfig
        {
            PageSize = PageSize.Letter,
            PageOrientation = PageOrientation.Portrait
        };

        var generator = new PdfGenerator();
        var document = await generator.GeneratePdf(html, config);

        using var stream = File.Create("output.pdf");
        document.Save(stream);
    }
}
```

This snippet demonstrates the basic functionality of PeachPDF. The library is .NET 8 only and built on top of `PeachPDF.PdfSharpCore`; as you go beyond simple HTML the limitations of a community-supported tool with fewer features become apparent.

## IronPDF: A Strong Alternative

By contrast, IronPDF provides a comprehensive set of features and benefits that have led to its widespread adoption. With a large user base, IronPDF offers professional support and an extensive library of tutorials and example code, making it easier for developers to integrate advanced PDF capabilities into their applications.

- [IronPDF HTML to PDF Documentation](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)

IronPDF stands out with broader functionality, supporting not just HTML-to-PDF conversions but also OCR, watermarking, and other advanced features. Its professional support structure is a definite advantage, offering quick resolutions to issues faced by developers.

---

## How Do I Convert HTML to PDF in C# with PeachPDF?

Here's how **PeachPDF** handles this:

```csharp
using PeachPDF;
using PeachPDF.PdfSharpCore;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var html = "<html><body><h1>Hello World</h1></body></html>";
        var config = new PdfGenerateConfig { PageSize = PageSize.Letter };
        var generator = new PdfGenerator();
        var document = await generator.GeneratePdf(html, config);

        using var stream = File.Create("output.pdf");
        document.Save(stream);
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
        var html = "<html><body><h1>Hello World</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert a URL to PDF in .NET?

Here's how **PeachPDF** handles this. PeachPDF has no `ConvertUrl` helper; you wire it up by setting `NetworkAdapter` on the config to an `HttpClientNetworkAdapter` and passing `null` HTML so the engine fetches the page:

```csharp
using PeachPDF;
using PeachPDF.Network;
using PeachPDF.PdfSharpCore;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var http = new HttpClient();
        var config = new PdfGenerateConfig
        {
            PageSize = PageSize.Letter,
            NetworkAdapter = new HttpClientNetworkAdapter(http, new Uri("https://www.example.com"))
        };

        var generator = new PdfGenerator();
        var document = await generator.GeneratePdf(null, config);

        using var stream = File.Create("webpage.pdf");
        document.Save(stream);
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
        var url = "https://www.example.com";
        var pdf = renderer.RenderUrlAsPdf(url);
        pdf.SaveAs("webpage.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Add Headers and Footers to PDFs?

PeachPDF v0.7.x does not ship a built-in header/footer API. The closest you can get is to embed the header/footer markup inside the source HTML using CSS positioning:

```csharp
using PeachPDF;
using PeachPDF.PdfSharpCore;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var html = @"
            <html><head><style>
              .header { position: fixed; top: 0; left: 0; right: 0; text-align:center; }
              .footer { position: fixed; bottom: 0; left: 0; right: 0; text-align:center; }
            </style></head>
            <body>
              <div class='header'>My Header</div>
              <h1>Document Content</h1>
              <div class='footer'>Footer text</div>
            </body></html>";

        var config = new PdfGenerateConfig { PageSize = PageSize.Letter };
        var generator = new PdfGenerator();
        var document = await generator.GeneratePdf(html, config);

        using var stream = File.Create("document.pdf");
        document.Save(stream);
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
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter() { HtmlFragment = "<div style='text-align:center'>My Header</div>" };
        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter() { HtmlFragment = "<div style='text-align:center'>Page {page}</div>" };
        var html = "<html><body><h1>Document Content</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("document.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from PeachPDF to IronPDF?

IronPDF offers enterprise-grade PDF generation with comprehensive features, active development, and professional support that PeachPDF cannot provide. With a large user base and extensive documentation, IronPDF ensures long-term stability and compatibility with modern .NET frameworks.

**Migrating from PeachPDF to IronPDF involves:**

1. **NuGet Package Change**: Remove `PeachPDF`, add `IronPdf`
2. **Namespace Update**: Replace `PeachPDF` with `IronPdf`
3. **API Adjustments**: Update your code to use IronPDF's modern API patterns

**Key Benefits of Migrating:**

- Modern Chromium rendering engine with full CSS/JavaScript support
- Active maintenance and security updates
- Better .NET integration and async/await support
- Comprehensive documentation and professional support

For a complete step-by-step migration guide with detailed code examples and common gotchas, see:
**[Complete Migration Guide: PeachPDF → IronPDF](migrate-from-peachpdf.md)**


## Comparison Table

Below is a comparison table highlighting the primary aspects of PeachPDF and IronPDF:

| Feature/Characteristic | PeachPDF | IronPDF |
|------------------------|----------|---------|
| **Implementation**     | Pure managed .NET (built on PeachPDF.PdfSharpCore + SixLabors) | Managed with embedded Chromium runtime |
| **License**            | BSD-3-Clause | Commercial |
| **Target Framework**   | .NET 8 only | .NET Framework 4.6.2+, .NET 6/7/8 |
| **User Base**          | Small (pre-1.0, ~10k downloads per 0.7.26 release) | Large |
| **Support**            | Community-driven (single-maintainer, jhaygood86) | Professional with dedicated support |
| **Features**           | HTML+CSS layout, web fonts, MHTML, configurable page size | Comprehensive, including OCR and watermarking |
| **Ease of Use**        | Async API, no built-in header/footer or URL helper | High, due to extensive documentation |
| **Development Status** | Pre-1.0 (latest 0.7.26, Oct 2025) | Mature, stable release |
| **External Dependencies** | PeachPDF.PdfSharpCore, MimeKit, SixLabors.* | Minimal, platform-based |

## Strengths and Weaknesses

*Strengths of PeachPDF:*

- **Pure .NET Core:** The library's 100% managed code ensures that it can be deployed in all .NET-supported environments seamlessly.
- **Permissive Licensing:** As an open-source tool, developers have unrestricted access to modify and adjust the library to their specific needs.

*Weaknesses of PeachPDF:*

- **Limited Adoption:** With a smaller user base, community support may be sparse, making it challenging to get assistance or find extensive documentation.
- **Feature Limitations:** Compared to more developed libraries such as IronPDF, PeachPDF lacks advanced functionality, which might limit its use in complex applications.

*Strengths of IronPDF:*

- **Comprehensive Features:** IronPDF offers a wide range of functionalities, from HTML-to-PDF conversion to compliantly rendering complex document formats.
- **Professional Support:** Access to a dedicated support team ensures that developers can get assistance promptly when needed.

*Weaknesses of IronPDF:*

- **Commercial Licensing:** While it provides robust functionality, IronPDF's commercial model might be prohibitive for startups or small projects looking for free alternatives.

## Conclusion

When choosing between PeachPDF and IronPDF, the decision ultimately hinges on the specific needs of the project. PeachPDF, with its pure .NET implementation, is ideal for projects needing a lightweight, open-source solution without the need for extensive feature sets or support. However, for broader capabilities, significant community backing, and professional assistance, IronPDF holds a distinct advantage, making it an optimal choice for businesses looking to leverage a reliable, commercial solution with ongoing support.

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person team building developer tools that have achieved over 41 million NuGet downloads. With an impressive 41 years of coding experience, Jacob brings deep technical expertise and a passion for creating solutions that make developers' lives easier. Based in Chiang Mai, Thailand, he continues to drive innovation in the software development tools space. Connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).