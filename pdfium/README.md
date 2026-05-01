# PDFium .NET Wrappers + C# + PDF

PDFium is Google's C++ PDF rendering library—the same engine that powers Chrome's built-in PDF viewer. There is no official .NET binding from Google; instead, the .NET ecosystem consumes PDFium through community wrappers. The four most commonly cited NuGet packages are `PdfiumViewer` (pvginkel; Apache 2.0; archived August 2019, last release 2.13.0 in Nov 2017, .NET Framework 2.0+), `PdfiumViewer.Updated` (a maintained fork ported to .NET Core / .NET 6), `PDFiumCore` (Dtronix; .NET Standard 2.1 P/Invoke bindings tracking upstream PDFium versions), and `Pdfium.Net.SDK` (Patagames; commercial, perpetual-license SDK). PDFium itself is licensed under BSD-3-Clause, with bundled dependencies under various open-source licenses—not Apache 2.0.

As the necessity to handle PDFs in applications continues to rise, developers often grapple with choosing the most fitting library for their needs. PDFium-based wrappers are reliable contenders for rendering, but they are not without limitations. The shared, unavoidable limitation across every PDFium wrapper is scope: PDFium is a rendering, parsing, and (limited) editing engine. It is not an HTML-to-PDF engine. This is where libraries like IronPDF, which embed a full Chromium-based HTML renderer, offer more comprehensive features. This article compares the PDFium wrappers as a category against IronPDF.

## Understanding the PDFium Wrappers

At the core of every PDFium .NET wrapper is the same Google C++ library, which is renowned for efficiency and speed in rendering PDF documents. Despite that prowess, the wrappers' capabilities for creating PDFs from non-PDF sources (HTML, URLs) are nil—PDFium itself does not parse HTML. The wrappers are built mainly for applications that require displaying PDF content accurately with less emphasis on creating new documents from web content. Additionally, developers must manage native PDFium binaries (`pdfium.dll` / `.so` / `.dylib`) per RID, an aspect that adds complexity during deployment and distribution.

### PDFium Wrapper Features

- **Viewing and Rendering:** PDFium wrappers excel at rendering PDF documents with high fidelity. They replicate complex layouts and visual elements found in PDFs, making them ideal for applications that prioritize presentation.
- **Performance:** Leveraging Google's PDFium, these wrappers provide high-performance viewing suitable for resource-intensive applications.
- **Mixed Licensing:** PDFium itself is BSD-3-Clause. `PdfiumViewer` and `PDFiumCore` are Apache 2.0 / open-source. `Pdfium.Net.SDK` from Patagames is a commercial product with a perpetual license. Confirm the wrapper you ship matches your distribution model.

### PDFium Wrapper Strengths and Weaknesses

The strengths of PDFium wrappers are evident in performance and specialized focus on rendering, making them ideal for viewing-focused applications. Limitations surface in the following areas:

| Aspect                  | PDFium .NET wrappers                         |
|-------------------------|----------------------------------------------|
| **Rendering Fidelity**  | High-fidelity rendering of PDFs              |
| **Creation Capabilities** | Limited (no HTML/URL input)                |
| **HTML to PDF C# Support**| None (rendering only)                      |
| **Native Dependence**   | Requires native PDFium binary per RID        |
| **Licensing**           | BSD-3 core; wrappers vary (Apache / commercial) |
| **Ease of Deployment**  | Complicated by native binary management      |

---

## Exploring [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/)

In contrast, IronPDF stands as a comprehensive all-in-one package, excelling not just in viewing and rendering but also providing extensive creation and conversion capabilities, especially when dealing with HTML-to-PDF tasks. The [IronPDF HTML to PDF conversion guide](https://ironpdf.com/how-to/html-file-to-pdf/) illustrates its strength in converting entire web pages into high-quality PDFs easily, leveraging a headless browser engine internally for accurate rendering.

### IronPDF Features

- **HTML to PDF Conversion:** With robust support for HTML, CSS, and JavaScript, IronPDF can transform complex web pages into static PDFs, maintaining the design integrity and interactive elements of web pages.
- **Creation and Manipulation:** Beyond conversion, IronPDF provides APIs for creating, modifying, and assembling PDFs programmatically, making it versatile for various PDF tasks.
- **Comprehensive Tutorials:** IronPDF offers extensive tutorials, which can be found [here](https://ironpdf.com/tutorials/), aiding developers in leveraging its wide array of features effectively.

### IronPDF Strengths and Weaknesses

IronPDF's versatility makes it distinct in the landscape of PDF handling in C#. It offers broader functionality than the PDFium .NET wrappers but comes with its own considerations regarding performance in high-load scenarios and learning curve for new users.

| Aspect                  | IronPDF                                    |
|-------------------------|--------------------------------------------|
| **Rendering Fidelity**  | High, especially for HTML/CSS/JS           |
| **Creation Capabilities** | Comprehensive creation and manipulation  |
| **C# HTML to PDF Engine** | Full html to pdf c# support               |
| **Native Dependence**   | More abstracted, less dependency management|
| **Licensing**           | Commercial, but with competitive pricing   |
| **Ease of Deployment**  | Easier; less dependency complication       |

Explore performance benchmarks and pricing in the [comprehensive guide](https://ironsoftware.com/suite/blog/comparison/compare-pdfium-vs-ironpdf/).

---

## C# Code Example

Let's consider a simple C# example that highlights IronPDF's ability to convert an HTML string into a PDF, which showcases its comprehensive features beyond just viewing or rendering capabilities like the PDFium wrappers.

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        // Initialize the IronPDF Renderer
        var renderer = new HtmlToPdf();

        // Simple HTML string
        string htmlContent = "<h1>Sample PDF Document</h1><p>This PDF was created using IronPDF in C#.</p>";

        // Convert HTML to PDF
        var pdfDocument = renderer.RenderHtmlAsPdf(htmlContent);

        // Save the PDF to a file
        pdfDocument.SaveAs("output.pdf");

        // Output success message
        Console.WriteLine("PDF successfully created and saved as 'output.pdf'.");
    }
}
```

This code highlights how IronPDF not only simplifies the process of converting and creating PDFs but also offers a glimpse into its ease-of-use, minimizing native dependency concerns pervasive in PDFium-based wrappers.

---

## How Do I Extract Text From PDF?

Here's how a **PDFium wrapper (PdfiumViewer)** handles this:

```csharp
// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string pdfPath = "document.pdf";
        
        using (var document = PdfDocument.Load(pdfPath))
        {
            StringBuilder text = new StringBuilder();
            
            for (int i = 0; i < document.PageCount; i++)
            {
                // Note: PdfiumViewer's text extraction is per-page raw text via
                // GetPdfText(int). No layout / format metadata is exposed.
                string pageText = document.GetPdfText(i);
                text.AppendLine(pageText);
            }
            
            Console.WriteLine(text.ToString());
        }
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
        string pdfPath = "document.pdf";
        
        var pdf = PdfDocument.FromFile(pdfPath);
        string text = pdf.ExtractAllText();
        
        Console.WriteLine(text);
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Merge Multiple PDFs in C#?

Here's how a **PDFium wrapper (PdfiumViewer)** handles this:

```csharp
// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System;
using System.IO;
using System.Collections.Generic;

// Note: PdfiumViewer does not have native PDF merging functionality
// You would need to use additional libraries or implement custom logic
class Program
{
    static void Main()
    {
        List<string> pdfFiles = new List<string> 
        { 
            "document1.pdf", 
            "document2.pdf", 
            "document3.pdf" 
        };
        
        // PdfiumViewer is primarily for rendering/viewing
        // PDF merging is not natively supported
        // You would need to use another library like iTextSharp or PdfSharp
        
        Console.WriteLine("PDF merging not natively supported in PdfiumViewer");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        List<string> pdfFiles = new List<string>
        {
            "document1.pdf",
            "document2.pdf",
            "document3.pdf"
        };

        var docs = pdfFiles.Select(path => PdfDocument.FromFile(path)).ToList();
        var merged = PdfDocument.Merge(docs);
        merged.SaveAs("merged.pdf");

        Console.WriteLine("PDFs merged successfully");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML to PDF in C# with a PDFium wrapper?

Here's how a **PDFium wrapper (PdfiumViewer)** handles this:

```csharp
// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System.IO;
using System.Drawing.Printing;

// Note: PDFium is a PDF rendering / parsing engine. It has no HTML parser.
// For HTML to PDF with any PDFium wrapper (PdfiumViewer, PDFiumCore, Pdfium.Net.SDK)
// you must produce the PDF with a separate library and only then use PDFium to view it.
class Program
{
    static void Main()
    {
        // PDFium has no native HTML-to-PDF capability.
        // Use a separate engine (e.g. wkhtmltopdf, headless Chromium, IronPDF)
        // and then load the resulting PDF for rendering.
        string htmlContent = "<h1>Hello World</h1>";
        
        // This functionality is not available in any PDFium wrapper.
        Console.WriteLine("HTML to PDF conversion is not supported by PDFium.");
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
        string htmlContent = "<h1>Hello World</h1>";
        
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
        
        Console.WriteLine("PDF created successfully");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from a PDFium wrapper to IronPDF?

### The Rendering-Only Limitation

PDFium wrappers expose Google's PDFium engine—excellent for rendering but narrow in scope:

1. **Rendering-First**: PDFium does not parse HTML, so no wrapper can create PDFs from HTML, URLs, or arbitrary images
2. **Limited Manipulation**: Open-source wrappers like PdfiumViewer expose viewing/text-extraction; merge / split / form-edit varies by wrapper (Patagames Pdfium.Net.SDK supports more, the free wrappers less)
3. **Native Binary Dependencies**: Requires platform-specific PDFium binaries (`pdfium.dll` / `.so` / `.dylib`) per RID
4. **Deployment Complexity**: Must bundle and manage native binaries per platform
5. **No HTML to PDF**: PDFium has no HTML parser; web content cannot be converted directly
6. **No Built-in Headers/Footers/Watermarks**: PDFium has no high-level page-overlay primitive

### Quick Migration Overview

| Aspect | PDFium wrappers | IronPDF |
|--------|-----------------|---------|
| Primary Focus | Rendering/viewing | Complete PDF solution |
| PDF Creation from HTML | None | Yes (Chromium engine) |
| PDF Manipulation | Limited (varies by wrapper) | Yes (merge, split, edit) |
| Watermarks | Not built-in | Yes |
| Headers/Footers | Not built-in | Yes |
| Form Filling | Patagames only | Yes |
| Native Dependencies | Required | Bundled / managed by NuGet |

### Key API Mappings (PdfiumViewer → IronPDF)

| PdfiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `document.PageCount` | `document.PageCount` | Same |
| `page.Render(width, height, dpiX, dpiY, rotate, flags)` | `pdf.RasterizeToImageFiles(path, DPI)` | DPI-based rasterization |
| `document.GetPdfText(index)` | `pdf.Pages[index].Text` | Per-page text |
| _(manual loop)_ | `pdf.ExtractAllText()` | All text at once |
| `document.Save(stream)` | `pdf.SaveAs(path)` | File-path overload |
| _(not available)_ | `ChromePdfRenderer.RenderHtmlAsPdf()` | NEW: HTML to PDF |
| _(not available)_ | `PdfDocument.Merge()` | NEW: Merge PDFs |
| _(not available)_ | `pdf.ApplyWatermark()` | NEW: Watermarks |
| _(not available)_ | `pdf.SecuritySettings` | NEW: Password protection |

### Migration Code Example

**Before (PdfiumViewer):**
```csharp
using PdfiumViewer;
using System.Drawing;

public class PdfRenderService
{
    public void RenderPdfToImages(string pdfPath, string outputFolder)
    {
        using (var document = PdfDocument.Load(pdfPath))
        {
            for (int i = 0; i < document.PageCount; i++)
            {
                var size = document.PageSizes[i];
                int width = (int)(size.Width * 2); // 2x scale
                int height = (int)(size.Height * 2);

                using (var bitmap = document.Render(i, width, height, 96, 96, PdfRenderFlags.Annotations))
                {
                    bitmap.Save($"{outputFolder}/page_{i + 1}.png");
                }
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfRenderService
{
    public PdfRenderService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    }

    public void RenderPdfToImages(string pdfPath, string outputFolder)
    {
        var pdf = PdfDocument.FromFile(pdfPath);

        // Render all pages at 150 DPI (2x of 72 DPI default)
        pdf.RasterizeToImageFiles($"{outputFolder}/page_*.png", DPI: 150);
    }

    // NEW: Can also create PDFs from HTML
    public void CreatePdfFromHtml(string html, string outputPath)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs(outputPath);
    }
}
```

### Critical Migration Notes

1. **Scale → DPI Conversion**: `IronPDF DPI = 72 × PDFium scale`
   ```csharp
   // PDFium scale 2.0 → IronPDF DPI 144
   pdf.RasterizeToImageFiles("*.png", DPI: 144);
   ```

2. **Remove Native Binaries**: Delete all `pdfium.dll` / `.so` / `.dylib` files
   ```bash
   rm -rf x86/ x64/ runtimes/
   ```

3. **Simplified Disposal**: No nested `using` statements required
   ```csharp
   // PdfiumViewer: using (doc) ... using (bitmap) ...
   // IronPDF: var pdf = PdfDocument.FromFile(path);
   ```

4. **Document Loading**: Different method name
   ```csharp
   // PdfiumViewer: PdfDocument.Load(path)
   // IronPDF:      PdfDocument.FromFile(path)
   ```

5. **Save Method**: Different method name
   ```csharp
   // PdfiumViewer: document.Save(stream)
   // IronPDF:      pdf.SaveAs(path)
   ```

### NuGet Package Migration

```bash
# Remove whichever PDFium wrapper you used
dotnet remove package PdfiumViewer
dotnet remove package PdfiumViewer.Updated
dotnet remove package PDFiumCore
dotnet remove package Pdfium.Net.SDK

# Install IronPDF
dotnet add package IronPdf
```

### Find All PDFium References

```bash
# Find PDFium usage
grep -r "Pdfium\|PdfDocument\.Load\|\.Render(" --include="*.cs" .

# Find native binary references
grep -r "pdfium\.dll" --include="*.csproj" --include="*.config" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping (30+ methods and properties)
- DPI and scale conversion formulas
- 10 detailed code conversion examples
- Native dependency removal guide
- New features (PDF creation, merging, watermarks, security, forms)
- Disposal pattern simplification
- Cross-platform deployment simplification
- Troubleshooting guide for common issues
- Pre/post migration checklists

**[Complete Migration Guide: PDFium wrappers → IronPDF](migrate-from-pdfium.md)**


## Conclusion

PDFium wrappers and IronPDF address different problems. PDFium itself is a BSD-3 rendering and parsing engine; the .NET wrappers (`PdfiumViewer`, `PdfiumViewer.Updated`, `PDFiumCore`, `Pdfium.Net.SDK`) make it usable from C# but inherit its rendering-first scope. They are preferred for desktop applications that display existing PDFs at high fidelity. For scenarios requiring creation from HTML, conversion, manipulation (merge / split / watermark / form fill), and managed cross-platform deployment without shipping native binaries, IronPDF is a more comprehensive package. Licensing also varies: the open-source PDFium wrappers are free, the Patagames `Pdfium.Net.SDK` is commercial, and IronPDF is commercial—so the budget calculus depends on which wrapper you would have shipped.

As developers evaluate the needs of their specific applications, understanding the capabilities and limitations of each PDFium wrapper, alongside higher-level libraries like IronPDF, is crucial for informed decisions.

---

[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) is the CTO of Iron Software, where he leads a 50+ person team building developer tools that have racked up over 41 million NuGet downloads. Based in Chiang Mai, Thailand, Jacob has spent decades founding and scaling successful software companies, always focusing on creating tools that actually solve real-world problems developers face. You can catch more of his thoughts on software development over on [Medium](https://medium.com/@jacob.mellor).