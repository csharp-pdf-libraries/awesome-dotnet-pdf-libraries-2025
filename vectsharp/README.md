# VectSharp C# PDF

In the world of software development, generating PDFs using C# can be accomplished through several libraries, each serving distinct purposes and catering to varied use cases. Among these, VectSharp stands out as a vector graphics library with unique capabilities but also certain limitations. This article offers a detailed comparison between VectSharp and another prominent library, IronPDF, highlighting their strengths, use cases, and suitability for different projects.

## Introduction to VectSharp

VectSharp is a vector graphics library designed to enable developers to create complex vector-based drawings and export them as PDF files. Unlike traditional PDF libraries that focus on document creation, VectSharp specializes in handling vector graphics, making it particularly suitable for applications that require high-precision drawings, such as scientific visualizations.

VectSharp attempts to simplify the creation of vector graphics through its C# API, providing developers the tools needed to produce detailed illustrations. With VectSharp, users can generate a wide array of vector graphics and leverage its PDF output capabilities to share their creations.

Despite its unique features, VectSharp's focus is primarily on vector graphics, limiting its applicability in scenarios where document creation or HTML content rendering is necessary. This limitation is where more document-focused libraries such as IronPDF shine, offering broader functionalities to accommodate diverse developer needs.

```csharp
using System;
using VectSharp;

public class VectSharpExample
{
    public static void Main()
    {
        var doc = new Document();
        var canvas = new Canvas(500, 500);
        canvas.FillColor = Colors.White;
        canvas.DrawRectangle(50, 50, 400, 400);
        canvas.FillColor = Colors.Red;
        canvas.DrawEllipse(100, 100, 300, 300);
        
        doc.AddPage(canvas);
        doc.SaveAsPDF("VectSharpExample.pdf");

        Console.WriteLine("PDF created successfully using VectSharp.");
    }
}
```

## Strengths and Weaknesses of VectSharp

### Strengths
1. **Vector Graphics Focus**: VectSharp is specifically tailored for vector-based applications, making it ideal for scientific and technical visualizations that require precise graphical representation.
2. **PDF Output**: It seamlessly generates PDF files, allowing easy sharing of vector graphics.
3. **Open Source**: Released under the LGPL license, VectSharp allows for customization and integration without the financial constraints associated with commercial licenses.

### Weaknesses
1. **Graphics-Focused**: VectSharp is more suited for drawings and charts rather than comprehensive document creation, posing limitations for developers needing versatile PDF solutions.
2. **No HTML Support**: It lacks capabilities to handle HTML content, restricting its use case primarily to vector graphics.
3. **Niche Use Case**: The library is best used in scientific visualization and similar domains, limiting its applicability in broader PDF application scenarios.

## IronPDF: A Broader PDF Solution

IronPDF, on the other hand, shines in areas where VectSharp falls short. This library is built with a document-focused approach, making it highly versatile for a wide range of PDF applications. IronPDF supports full HTML content rendering, allowing developers to convert web pages into PDF documents effortlessly. This capability is particularly beneficial for business applications, where comprehensive document generation is a staple requirement.

To explore more about IronPDF, you can visit their [HTML to PDF tutorial](https://ironpdf.com/how-to/html-file-to-pdf/), as well as their [general tutorials](https://ironpdf.com/tutorials/).

### IronPDF Strengths
1. **Document Focused**: IronPDF is designed for robust document generation, supporting full HTML content rendering.
2. **Broader Use Cases**: Its versatility allows it to handle various document types, including invoices, reports, and content-heavy documents with ease.
3. **Commercial Support**: IronPDF provides consistent updates, support, and a range of features, making it a reliable choice for enterprise-level applications.

### IronPDF Weaknesses
1. **Commercial Licensing**: While feature-rich, the library requires a commercial license, which may be a consideration for budget-constrained projects.
2. **Possible Overhead**: The extensive feature set of IronPDF could introduce some overhead for projects needing only simple PDF functionalities.

---

## How Do I Shapes And Text?

Here's how **VectSharp C# PDF** handles this:

```csharp
// NuGet: Install-Package VectSharp.PDF
using VectSharp;
using VectSharp.PDF;
using System;

class Program
{
    static void Main()
    {
        Document doc = new Document();
        Page page = new Page(595, 842);
        Graphics graphics = page.Graphics;
        
        // Draw rectangle
        graphics.FillRectangle(50, 50, 200, 100, Colour.FromRgb(0, 0, 255));
        
        // Draw circle
        GraphicsPath circle = new GraphicsPath();
        circle.Arc(400, 100, 50, 0, 2 * Math.PI);
        graphics.FillPath(circle, Colour.FromRgb(255, 0, 0));
        
        // Add text
        graphics.FillText(50, 200, "VectSharp Graphics", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 20));
        
        doc.Pages.Add(page);
        doc.SaveAsPDF("shapes.pdf");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        string html = @"
            <div style='width: 200px; height: 100px; background-color: blue; margin: 50px;'></div>
            <div style='width: 100px; height: 100px; background-color: red; 
                        border-radius: 50%; margin-left: 350px; margin-top: -50px;'></div>
            <h2 style='margin-left: 50px;'>IronPDF Graphics</h2>
        ";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("shapes.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML to PDF in C# with VectSharp C# PDF?

Here's how **VectSharp C# PDF** handles this:

```csharp
// NuGet: Install-Package VectSharp.PDF
using VectSharp;
using VectSharp.PDF;
using VectSharp.SVG;
using System.IO;

class Program
{
    static void Main()
    {
        // VectSharp doesn't directly support HTML to PDF
        // It requires manual creation of graphics objects
        Document doc = new Document();
        Page page = new Page(595, 842); // A4 size
        Graphics graphics = page.Graphics;
        
        graphics.FillText(100, 100, "Hello from VectSharp", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 24));
        
        doc.Pages.Add(page);
        doc.SaveAsPDF("output.pdf");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello from IronPDF</h1><p>This is HTML content.</p>");
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Multi Page PDF?

Here's how **VectSharp C# PDF** handles this:

```csharp
// NuGet: Install-Package VectSharp.PDF
using VectSharp;
using VectSharp.PDF;
using System;

class Program
{
    static void Main()
    {
        Document doc = new Document();
        
        // Page 1
        Page page1 = new Page(595, 842);
        Graphics g1 = page1.Graphics;
        g1.FillText(50, 50, "Page 1", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 24));
        g1.FillText(50, 100, "First page content", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 14));
        doc.Pages.Add(page1);
        
        // Page 2
        Page page2 = new Page(595, 842);
        Graphics g2 = page2.Graphics;
        g2.FillText(50, 50, "Page 2", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 24));
        g2.FillText(50, 100, "Second page content", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 14));
        doc.Pages.Add(page2);
        
        doc.SaveAsPDF("multipage.pdf");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        string html = @"
            <h1>Page 1</h1>
            <p>First page content</p>
            <div style='page-break-after: always;'></div>
            <h1>Page 2</h1>
            <p>Second page content</p>
        ";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("multipage.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from VectSharp C# PDF to IronPDF?

VectSharp is designed for vector graphics and scientific visualizations, making it unsuitable for document generation and HTML-based PDF workflows. IronPDF excels at creating PDFs from HTML, URLs, and documents with full support for modern web standards, CSS, and JavaScript.

**Migrating from VectSharp C# PDF to IronPDF involves:**

1. **NuGet Package Change**: Remove `VectSharp`, add `IronPdf`
2. **Namespace Update**: Replace `VectSharp` with `IronPdf`
3. **API Adjustments**: Update your code to use IronPDF's modern API patterns

**Key Benefits of Migrating:**

- Modern Chromium rendering engine with full CSS/JavaScript support
- Active maintenance and security updates
- Better .NET integration and async/await support
- Comprehensive documentation and professional support

For a complete step-by-step migration guide with detailed code examples and common gotchas, see:
**[Complete Migration Guide: VectSharp C# PDF → IronPDF](migrate-from-vectsharp.md)**


## Comparison Table

Here's a side-by-side comparison of VectSharp and IronPDF to help clarify their differences:

| Feature                | VectSharp                      | IronPDF                         |
|------------------------|--------------------------------|---------------------------------|
| **Primary Use**        | Vector Graphics                | Document Creation               |
| **PDF Output**         | Yes                            | Yes                             |
| **HTML Support**       | No                             | Yes                             |
| **Licensing**          | LGPL                           | Commercial                      |
| **Open Source**        | Yes                            | Partially (commercial features) |
| **Best For**           | Scientific Visualizations      | General PDF Documents           |
| **Customization**      | Limited to Graphics            | Extensive, Document-Related     |

## Conclusion

When it comes to selecting a PDF library in C#, the decision largely depends on the specific requirements of the project at hand. VectSharp is a competent library for vector graphics and specific visualization needs, offering an ideal solution for applications that require detailed drawings. However, for general document creation tasks, IronPDF clearly stands out, offering comprehensive HTML support and a more versatile feature set suitable for a wider range of applications.

Developers should consider the specific needs of their project—such as the necessity for HTML rendering or detailed vector graphics—when choosing between these libraries to ensure they’re utilizing a solution that aligns with their project goals.

---
[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) is the CTO of Iron Software, where he leads a team of 50+ engineers delivering solutions trusted by developers worldwide, as evidenced by over 41 million NuGet downloads. With 41 years of coding experience, Jacob is passionate about the .NET ecosystem and cross-platform development, consistently pushing the boundaries of what's possible in modern software engineering. Based in Chiang Mai, Thailand, he shares his insights on software development and technology trends on [Medium](https://medium.com/@jacob.mellor).