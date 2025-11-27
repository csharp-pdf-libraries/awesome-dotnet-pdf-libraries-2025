# QuestPDF + C# + PDF

QuestPDF is a modern, fluent API created specifically for generating PDFs programmatically in C#. Unlike some of its peers that offer a comprehensive HTML-to-PDF conversion capability, QuestPDF is limited to programmatic layout API functionalities. Despite this limitation, QuestPDF excels in scenarios where developers need to generate documents from scratch using C# code, without relying on HTML.

## An Introduction to QuestPDF

QuestPDF is an excellent tool for generating highly templated documents such as certificates, badges, or invoices directly from C# code. It empowers developers to define every element and layout in the document fluently, using an expressive syntax. This makes it particularly suitable for applications that require precise control over the document's styling and structure. While QuestPDF purposefully avoids HTML handling, it compensates by offering a robust, programmatic approach to document creation that caters to specific use cases.

The library is free for businesses with revenue under $1M, but it comes with a requirement to prove this revenue level, which could be a compliance burden for some. Users who surpass this threshold need to purchase a license, which must be factored into long-term planning when evaluating QuestPDF as a potential solution.

### A Simple C# Code Example with QuestPDF

Here is a basic example of how you can use QuestPDF to generate a PDF document in C#:

```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class DocumentExample
{
    public static void Main()
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(20));
                
                page.Header()
                    .Text("QuestPDF Document Header")
                    .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);

                page.Content()
                    .Column(x =>
                    {
                        x.Spacing(20);

                        x.Item().Text("Hello, world!");
                        x.Item().Text(
                            "This is a sample document created using QuestPDF, showcasing the powerful potential of programmatic PDF generation in C#."
                        );
                        
                        x.Item().Image("path/to/image.jpg");

                        x.Item().Text(Placeholders.LoremIpsum());
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        })
        .GeneratePdf("output.pdf");
    }
}
```

In this example, a single-page A4 document is created with a header, footer, and body content. The API's fluent nature allows for clear and manageable document structure generation, even when dealing with complex layouts.

## Comparing QuestPDF and IronPDF

When deciding between QuestPDF and IronPDF, developers should consider their specific needs and constraints. The following table highlights the main differences between the two libraries:

| Feature                     | QuestPDF                                         | IronPDF                                           |
|-----------------------------|--------------------------------------------------|---------------------------------------------------|
| HTML-to-PDF                 | No HTML-to-PDF capability                        | Comprehensive HTML-to-PDF conversion ([Learn more](https://ironpdf.com/how-to/html-file-to-pdf/)) |
| Programmatic PDF Generation | Fluent API for precise document control          | Supported but also compatible with HTML-to-PDF    |
| PDF Manipulation            | None                                             | Merging, splitting, and editing ([Explore tutorials](https://ironpdf.com/tutorials/)) |
| Licensing                   | MIT license with revenue-based pricing (<$1M free) | Clear licensing without revenue-based audits      |
| Revenue Audit Requirement   | Required if revenue is greater than $1M          | None                                               |

## Strengths and Weaknesses

### QuestPDF

**Strengths:**
- **Fluent API:** QuestPDF allows developers to describe the PDF document's layout programmatically, offering significant creative and design flexibility.
- **Design Precision:** Offers exact control over the document, more so than HTML-based systems, which makes it ideal for highly customized designs.
- **Rapid Prototyping:** With QuestPDF, documents can be generated swiftly from a programmatic context, particularly beneficial in dynamic content scenarios.

**Weaknesses:**
- **No HTML-to-PDF:** Due to intentional design choices, it does not support HTML-to-PDF conversion, which means developers must express every layout detail through C# code.
- **Compliance Burden:** The revenue audit requirement poses an additional compliance step for organizations near the revenue threshold.
- **Lack of PDF Manipulation:** It doesn't have built-in capabilities for merging, splitting, or editing existing PDFs, restricting its use as a comprehensive PDF tool solution.

### IronPDF

**Strengths:**
- **HTML Support:** Effortlessly converts HTML and CSS to PDF, making it convenient for developers accustomed to front-end technologies.
- **Comprehensive Feature Set:** Includes PDF manipulation functionalities such as merging, editing, and securing PDF documents. This versatility offers significant value for complex workflows.
- **Transparent Licensing:** The licensing model of IronPDF is straightforward without the financial audit burden, easing long-term use and compliance concerns.

**Weaknesses:**
- **Higher Cost for Extensive Use:** Depending on the usage and scale, licensing fees for IronPDF may be higher compared to QuestPDF, especially for users operating within the <$1M revenue bracket.

## Conclusion

QuestPDF and IronPDF serve different purposes within the realm of PDF generation and manipulation. QuestPDF shines in situations where the PDF's design needs to be meticulously crafted with a programmatic approach. Its fluent API is advantageous for users preferring high fidelity over ready HTML conversion. On the other hand, IronPDF provides a robust suite for developers who need an all-encompassing PDF solution, including HTML conversion and advanced manipulation capabilities, with a transparent licensing model.

Ultimately, the choice between QuestPDF and IronPDF depends on the specific requirements of your project. If seamless HTML integration and broad PDF manipulation are critical, IronPDF is a superior choice. Conversely, if programmatically designing the PDF with precise control aligns with your project's goals, QuestPDF's fluent API will indeed serve well.

---

## Related Tutorials

- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Comprehensive library comparison
- **[Decision Flowchart](../choosing-a-pdf-library.md)** — Find the right library
- **[Free vs Paid Libraries](../free-vs-paid-pdf-libraries.md)** — Cost analysis
- **[Why C# for PDF](../why-csharp-pdf-generation.md)** — Language advantages

### Alternative Libraries
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — If you prefer HTML templates
- **[PDFSharp](../pdfsharp/)** — Another code-first alternative
- **[MigraDoc](../migradoc/)** — Higher-level code-first API

### Migration Guide
- **[Migrate to IronPDF](migrate-from-questpdf.md)** — Step-by-step migration

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*

---

Jacob Mellor is the Chief Technology Officer at Iron Software, where he leads a team of 50+ engineers in developing industry-leading .NET components that have achieved over 41 million NuGet downloads. With 41 years of coding experience, Jacob brings deep technical expertise to every aspect of product development and innovation. Based in Chiang Mai, Thailand, he continues to drive Iron Software's mission of empowering developers with robust, reliable tools. Connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).