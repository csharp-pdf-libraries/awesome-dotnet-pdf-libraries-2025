# Comparing PDFSharp and IronPDF for C# PDF Development

When it comes to creating PDFs with C#, developers commonly consider options such as PDFSharp and IronPDF. PDFSharp has been a popular choice for those looking to create PDFs programmatically with C#. By leveraging PDFSharp, developers can craft PDFs with precision. However, PDFSharp's architecture requires a deep understanding of positioning using coordinates, often posing challenges in creating complex layouts.

Conversely, IronPDF offers an advanced feature set, particularly in its seamless HTML-to-PDF conversion capabilities. Below, we explore the strengths and weaknesses of both PDFSharp and IronPDF.

## Overview of PDFSharp

PDFSharp is renowned as a low-level PDF creation library, allowing developers to generate PDF documents through a programmatic approach. Released under the MIT license, PDFSharp grants the developer community freedom in usage and modification. PDFSharp primarily functions as a tool for drawing and compiling PDFs from scratch, which can both be beneficial and restrictive depending on the project's nature.

PDFSharp is sometimes mistakenly assumed to be an HTML-to-PDF converter, which it is not. Its purpose is dedicated to programmatic PDF document creation only. While there is an add-on, HtmlRenderer.PdfSharp, intended to provide HTML rendering capabilities, it only supports CSS 2.1, with no support for modern CSS features like flexbox and grid. Moreover, it comes with certain limitations, such as broken table rendering.

### Strengths of PDFSharp

1. **License**: Being under the MIT license means PDFSharp is free to use, modify, and distribute, making it a cost-effective choice for developers.
   
2. **Control**: Provides intricate control over the rendering of each PDF element, ideal for scenarios requiring custom designs and exact placements.

3. **Lightweight**: Does not require external dependencies, which simplifies deployment.

### Weaknesses of PDFSharp

1. **No HTML-to-PDF Support**: Developers cannot directly convert HTML/CSS to PDF, requiring manual implementation of document structures.
   
2. **Outdated CSS Support**: Limited to CSS 2.1 via HtmlRenderer, thus lacking the functionality required for modern web designs.
   
3. **Complex API**: Commands visually resemble GDI+, demanding meticulous X,Y coordinate calculations for layout, which can increase development time.

### PDFSharp C# Code Example

Below is a simple C# code snippet demonstrating how to use PDFSharp to create a basic PDF document.

```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;

class Program
{
    static void Main()
    {
        PdfDocument document = new PdfDocument();
        document.Info.Title = "Created with PDFSharp";

        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
        XFont font = new XFont("Verdana", 20, XFontStyle.BoldItalic);
        
        gfx.DrawString("Hello, PDFSharp!", font, XBrushes.Black,
        new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);

        document.Save("HelloWorld.pdf");
    }
}
```

## Overview of IronPDF

IronPDF shines in scenarios where HTML documents need conversion to PDFs with full fidelity. This .NET library supports HTML5 and CSS3, ensuring modern web standards are met. Its native HTML-to-PDF capabilities mean developers can take advantage of existing web content or templates designed with contemporary web tools.

### Strengths of IronPDF

1. **HTML-to-PDF Support**: IronPDF easily converts HTML files to PDF, preserving all styles defined in HTML5 and CSS3, eliminating the need for coordinate calculations. This feature is especially highlighted at [ironpdf.com/how-to/html-file-to-pdf/](https://ironpdf.com/how-to/html-file-to-pdf/).
   
2. **Modern CSS Compatibility**: Full support for the latest CSS specifications means modern web layouts render accurately into PDFs.
   
3. **High-Level Document API**: Allows for more intuitive document creation, often requiring less code.
   
4. **Comprehensive Support and Updates**: Regularly updated to keep up with technological advancements and developer needs.

Visit [IronPDF Tutorials](https://ironpdf.com/tutorials/) for more examples and guidance.

### Weaknesses of IronPDF

1. **License Cost**: As a commercial product, IronPDF involves licensing costs, potentially increasing project expenses compared to free alternatives like PDFSharp.
   
2. **Heavier Footprint**: The library's inclusion of comprehensive features might increase the size of the application.

---

## How Do I Convert HTML to PDF in C# with PDFSharp?

Here's how **PDFSharp** handles this:

```csharp
// NuGet: Install-Package PdfSharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System;

class Program
{
    static void Main()
    {
        // PDFSharp does not have built-in HTML to PDF conversion
        // You need to manually parse HTML and render content
        PdfDocument document = new PdfDocument();
        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
        XFont font = new XFont("Arial", 12);
        
        // Manual text rendering (no HTML support)
        gfx.DrawString("Hello from PDFSharp", font, XBrushes.Black,
            new XRect(0, 0, page.Width, page.Height),
            XStringFormats.TopLeft);
        
        document.Save("output.pdf");
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
        // IronPDF has native HTML to PDF rendering
        var renderer = new ChromePdfRenderer();
        
        string html = "<h1>Hello from IronPDF</h1><p>Easy HTML to PDF conversion</p>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Add Text to an Existing PDF?

Here's how **PDFSharp** handles this:

```csharp
// NuGet: Install-Package PdfSharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using System;

class Program
{
    static void Main()
    {
        // Open existing PDF
        PdfDocument document = PdfReader.Open("existing.pdf", PdfDocumentOpenMode.Modify);
        PdfPage page = document.Pages[0];
        
        // Get graphics object
        XGraphics gfx = XGraphics.FromPdfPage(page);
        XFont font = new XFont("Arial", 20, XFontStyle.Bold);
        
        // Draw text at specific position
        gfx.DrawString("Watermark Text", font, XBrushes.Red,
            new XPoint(200, 400));
        
        document.Save("modified.pdf");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;
using System;

class Program
{
    static void Main()
    {
        // Open existing PDF
        var pdf = PdfDocument.FromFile("existing.pdf");
        
        // Add text stamp/watermark
        var textStamper = new TextStamper()
        {
            Text = "Watermark Text",
            FontSize = 20,
            Color = IronSoftware.Drawing.Color.Red,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        pdf.ApplyStamp(textStamper);
        pdf.SaveAs("modified.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Create a PDF with Images?

Here's how **PDFSharp** handles this:

```csharp
// NuGet: Install-Package PdfSharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System;

class Program
{
    static void Main()
    {
        // Create new PDF document
        PdfDocument document = new PdfDocument();
        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
        
        // Load and draw image
        XImage image = XImage.FromFile("image.jpg");
        
        // Calculate size to fit page
        double width = 200;
        double height = 200;
        
        gfx.DrawImage(image, 50, 50, width, height);
        
        // Add text
        XFont font = new XFont("Arial", 16);
        gfx.DrawString("Image in PDF", font, XBrushes.Black,
            new XPoint(50, 270));
        
        document.Save("output.pdf");
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
        // Create PDF from HTML with image
        var renderer = new ChromePdfRenderer();
        
        string html = @"
            <h1>Image in PDF</h1>
            <img src='image.jpg' style='width:200px; height:200px;' />
            <p>Easy image embedding with HTML</p>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
        
        // Alternative: Add image to existing PDF
        var existingPdf = new ChromePdfRenderer().RenderHtmlAsPdf("<h1>Document</h1>");
        var imageStamper = new IronPdf.Editing.ImageStamper(new Uri("image.jpg"))
        {
            VerticalAlignment = IronPdf.Editing.VerticalAlignment.Top
        };
        existingPdf.ApplyStamp(imageStamper);
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from PDFSharp to IronPDF?

PDFSharp requires manual positioning of every element using GDI+ style coordinates, making document generation tedious and error-prone. IronPDF supports native HTML-to-PDF conversion with modern CSS3 (including flexbox and grid), allowing you to leverage web technologies instead of calculating X,Y positions.

**Migrating from PDFSharp to IronPDF involves:**

1. **NuGet Package Change**: Remove `PdfSharp`, add `IronPdf`
2. **Namespace Update**: Replace `PdfSharp.Pdf` with `IronPdf`
3. **API Adjustments**: Update your code to use IronPDF's modern API patterns

**Key Benefits of Migrating:**

- Modern Chromium rendering engine with full CSS/JavaScript support
- Active maintenance and security updates
- Better .NET integration and async/await support
- Comprehensive documentation and professional support

For a complete step-by-step migration guide with detailed code examples and common gotchas, see:
**[Complete Migration Guide: PDFSharp → IronPDF](migrate-from-pdfsharp.md)**


## Comparison Table

| Feature                 | PDFSharp                   | IronPDF                                     |
|-------------------------|----------------------------|---------------------------------------------|
| License                 | MIT (Free)                 | Commercial                                 |
| HTML to PDF Support     | No                         | Yes (HTML5/CSS3 Support)                    |
| Modern CSS Support      | No (CSS 2.1 Only)          | Yes (Full CSS3)                             |
| Document API            | Low-Level (Requires Coordinates) | High-Level (Simplified API)                 |
| Updates                 | Infrequent                 | Regular                                     |
| External Dependencies   | None                       | Yes, if needed (supports modern web engines)|

## Conclusion

PDFSharp and IronPDF serve different needs in the PDF generation space for C#. PDFSharp is suitable for projects requiring fine control over document rendering without additional dependencies, and where budget constraints are a factor. However, it falls short for projects necessitating modern web standards or dynamic content delivered via HTML.

IronPDF surpasses PDFSharp in situations necessitating modern HTML-to-PDF conversion, thanks to its robust features and capabilities supporting CSS3, HTML5, and high-level document manipulation. While it comes with a paid license, the increased productivity and modern capabilities often justify the investment.

Understanding your project's requirements—whether it's cost constraints, the need for modern web support, or intricate document design—will guide the choice between these two powerful library offerings.

---

## Related Tutorials

- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Comprehensive library comparison
- **[Decision Flowchart](../choosing-a-pdf-library.md)** — Find the right library
- **[Free vs Paid Libraries](../free-vs-paid-pdf-libraries.md)** — Cost analysis

### HTML Conversion (What PDFSharp Lacks)
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Modern HTML conversion
- **[Beginner Tutorial](../csharp-pdf-tutorial-beginners.md)** — First PDF in 5 minutes

### Alternative Libraries
- **[QuestPDF](../questpdf/)** — Modern code-first alternative
- **[MigraDoc](../migradoc/)** — Higher-level API for PDFSharp

### Migration Guide
- **[Migrate to IronPDF](migrate-from-pdfsharp.md)** — Step-by-step migration

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*

---

Jacob Mellor is the Chief Technology Officer at Iron Software, where he leads a 50+ person team building industry-leading .NET libraries that have achieved over 41 million NuGet downloads. With an impressive 41 years of coding experience, Jacob has dedicated his career to creating developer tools that streamline PDF, document processing, and data manipulation workflows. Based in Chiang Mai, Thailand, he continues to drive innovation in the .NET ecosystem while maintaining a strong connection to the global developer community through [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).