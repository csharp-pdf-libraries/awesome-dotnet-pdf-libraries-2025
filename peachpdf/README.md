# PeachPDF + C# + PDF

PeachPDF is a relatively new entrant in the .NET ecosystem designed for developers who need to convert HTML to PDF. As a library, PeachPDF promises a pure .NET implementation, setting itself apart by not relying on external processes, ensuring it can be seamlessly integrated across platforms where .NET is supported. This characteristic positions PeachPDF as an appealing choice for projects looking for a lightweight, managed library solution. Despite its potential, PeachPDF is still in development, highlighting both exciting possibilities and notable limitations.

PeachPDF remains enticing because of its pure .NET core, which promises straightforward deployment in diverse environments. Unlike some PDF generation tools that require interfacing with native code or platform-specific APIs, PeachPDF's managed approach fosters greater compatibility and ease of maintenance. However, it also translates into limited adoption, with a smaller user base and community-driven support. For developers seeking robust HTML-to-PDF functionality in .NET, this makes PeachPDF a consideration with caveats, particularly when compared to more established libraries like IronPDF.

## C# Code Example Using PeachPDF

Below is a simple illustration of how PeachPDF can be used in a C# project to convert HTML to PDF:

```csharp
using PeachPDF;
using System;

class Program
{
    static void Main(string[] args)
    {
        string htmlContent = "<html><body><h1>Hello, World!</h1></body></html>";
        string outputPath = "output.pdf";

        using (var pdf = new PdfDocument())
        {
            pdf.AddHtml(htmlContent);
            pdf.Save(outputPath);
        }

        Console.WriteLine("PDF generated successfully at " + outputPath);
    }
}
```

This code snippet demonstrates the basic functionality of PeachPDF, showing how easy it is to convert a simple HTML string to a PDF file. However, as you delve deeper, the limitations of a community-supported tool with fewer features might become apparent.

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
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var html = "<html><body><h1>Hello World</h1></body></html>";
        var pdf = converter.Convert(html);
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
        var html = "<html><body><h1>Hello World</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert a URL to PDF in .NET?

Here's how **PeachPDF** handles this:

```csharp
using PeachPDF;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        var url = "https://www.example.com";
        var pdf = converter.ConvertUrl(url);
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
        var url = "https://www.example.com";
        var pdf = renderer.RenderUrlAsPdf(url);
        pdf.SaveAs("webpage.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Add Headers and Footers to PDFs?

Here's how **PeachPDF** handles this:

```csharp
using PeachPDF;
using System.IO;

class Program
{
    static void Main()
    {
        var converter = new HtmlToPdfConverter();
        converter.Header = "<div style='text-align:center'>My Header</div>";
        converter.Footer = "<div style='text-align:center'>Page {page}</div>";
        var html = "<html><body><h1>Document Content</h1></body></html>";
        var pdf = converter.Convert(html);
        File.WriteAllBytes("document.pdf", pdf);
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
| **Implementation**     | Pure .NET| Managed with broad compatibility |
| **License**            | Open Source (BSD-3-Clause) | Commercial |
| **User Base**          | Small    | Large   |
| **Support**            | Community-driven | Professional with dedicated support |
| **Features**           | Basic    | Comprehensive, including OCR and watermarking |
| **Ease of Use**        | Moderate | High, due to extensive documentation |
| **Development Status** | In development | Mature, stable release |
| **External Dependencies** | None   | Minimal, platform-based |

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