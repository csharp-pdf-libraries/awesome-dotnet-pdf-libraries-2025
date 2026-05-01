# PDFreactor + C# + PDF: A Comprehensive Comparison with IronPDF

When it comes to transforming HTML content into PDF format, two significant players in the market, PDFreactor and IronPDF, offer unique solutions for developers working in C# environments. While both have their strengths and weaknesses, understanding these nuances is essential to choosing the right tool for your project.

## An Introduction to PDFreactor

PDFreactor is a powerful HTML-to-PDF conversion server that integrates seamlessly across various platforms. As a commercial solution, PDFreactor leverages its proprietary technology to convert HTML and CSS content into high-quality PDF documents. Among its notable attributes, PDFreactor supports a wide array of CSS properties which makes it a strong candidate for complex layout rendering. However, PDFreactor's reliance on Java presents certain challenges in .NET environments where its non-native nature may complicate deployment and integration.

Despite these challenges, PDFreactor continues to be favored in situations where the highest fidelity rendering of CSS is required. Its ability to handle complex documents and render them with precision is a testament to its powerful conversion engine.

### PDFreactor Sample C# Code

The PDFreactor .NET integration is a Web Service client: the client library `PDFreactor.dll` (namespace `RealObjects.PDFreactor.Webservice.Client`) ships in the PDFreactor installation under `clients/netstandard2/bin/` and talks REST to a separately-installed PDFreactor Web Service running on Java/Jetty. Here is a simplified code example:

```csharp
using System;
using RealObjects.PDFreactor.Webservice.Client;

public class PDFreactorDemo
{
    public static void Main(string[] args)
    {
        // Defaults to http://localhost:9423/service/rest
        PDFreactor pdfReactor = new PDFreactor();

        // Set up the configuration
        Configuration config = new Configuration();
        config.Document = "<html><body><h1>Hello, PDFreactor!</h1></body></html>";

        // Convert to PDF
        Result result = pdfReactor.Convert(config);

        // Save PDF to file
        System.IO.File.WriteAllBytes("output.pdf", result.Document);
    }
}
```

### Strengths of PDFreactor

1. **Advanced CSS Support**: PDFreactor excels in supporting advanced CSS properties, making it ideal for complex grid layouts and media queries.
2. **Cross-Platform**: It runs on any system with Java, offering flexibility across different OS environments.
3. **Ideal for Server Environments**: As a server-based solution, PDFreactor is optimized for high-volume PDF document generation.

### Weaknesses of PDFreactor

1. **Java Web Service Dependency**: The .NET integration is a REST client that requires the PDFreactor Web Service (Java/Jetty) running locally or remotely — your stack still needs a JRE somewhere.
2. **Server Architecture Requirement**: Every conversion is an out-of-process HTTP round-trip to a separately deployed service.
3. **No NuGet Distribution**: The `PDFreactor.dll` client ships inside the PDFreactor installer (`clients/netstandard2/bin/`), not on nuget.org, so dependency management is manual.

## IronPDF: A Native .NET Solution

In contrast to PDFreactor, IronPDF presents itself as a native .NET library, specifically designed to integrate seamlessly into .NET projects without external dependencies like Java. This feature alone makes IronPDF significantly more attractive to developers looking for simplicity and seamless deployment.

IronPDF uses a bundled Chromium rendering engine, allowing it to convert HTML to PDF with just a few lines of code. Its API is intuitive, emphasizing ease of use, while still offering robust document manipulation capabilities, such as editing, merging, splitting, form filling, and digital signatures.

### IronPDF Links

- [Convert HTML File to PDF with IronPDF](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)

### Strengths of IronPDF

1. **100% .NET Solution**: Eliminates the need for Java or any other extraneous dependencies, streamlining application development.
2. **Simple Deployment**: Easy integration and deployment with minimal configuration, thanks to its native .NET library status.
3. **Comprehensive PDF Features**: Beyond conversion, it provides extensive manipulation tools including the ability to edit, merge, and digitally sign PDFs.

### Weaknesses of IronPDF

1. **Bundled Chromium**: While convenient, this carries with it the overhead associated with bundling a browser engine.
2. **Licensing Cost**: As a commercial product, licensing can become expensive, especially for broader developer distribution requirements.

### C# Code Example Using IronPDF

Here's how straightforward it can be to convert HTML to PDF using IronPDF in a C# application:

```csharp
using IronPdf;

public class IronPdfDemo
{
    public static void Main(string[] args)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Create a new PDF from HTML
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Hello, IronPDF!</h1></body></html>");

        // Save PDF to file
        pdf.SaveAs("output.pdf");
    }
}
```

## A Comparative Look at PDFreactor and IronPDF

The table below provides a direct comparison of PDFreactor and IronPDF, examining their core differences and similarities:

| Feature/Aspect               | PDFreactor                                       | IronPDF                                               |
|------------------------------|--------------------------------------------------|-------------------------------------------------------|
| Native .NET Library          | No (Java-based)                                  | Yes                                                   |
| Cross-Platform Capability    | Yes (Java-dependent)                             | Yes (Bundled Chromium)                                |
| CSS Support                  | Advanced support for CSS3, CSS Paged Media, etc. | Comprehensive, but primarily for HTML5/CSS3 support   |
| Deployment Complexity        | More complex due to Java                         | Simple, directly integrates with .NET                 |
| PDF Manipulation Features    | Basic (Generation only)                          | Extensive, including merge, split, edit, and annotate |
| Licensing Model              | Commercial                                       | Commercial                                            |
| Primary Use Case             | High fidelity, complex documents                 | Broad use, ease-of-use in .NET apps                   |

---

## How Do I Convert HTML to PDF in C# with PDFreactor?

Here's how **PDFreactor** handles this:

```csharp
// PDFreactor .NET wrapper is a Web Service client (no NuGet package).
// Reference PDFreactor.dll from <PDFreactor-install>/clients/netstandard2/bin/
// and run the PDFreactor Web Service (Java/Jetty) locally or remotely.
using RealObjects.PDFreactor.Webservice.Client;
using System.IO;

class Program
{
    static void Main()
    {
        PDFreactor pdfReactor = new PDFreactor();
        
        string html = "<html><body><h1>Hello World</h1></body></html>";
        
        Configuration config = new Configuration();
        config.Document = html;
        
        Result result = pdfReactor.Convert(config);
        
        File.WriteAllBytes("output.pdf", result.Document);
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
        
        string html = "<html><body><h1>Hello World</h1></body></html>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I HTML To PDF Headers Footers?

Here's how **PDFreactor** handles this:

```csharp
// PDFreactor .NET wrapper is a Web Service client (no NuGet package).
// Reference PDFreactor.dll from <PDFreactor-install>/clients/netstandard2/bin/
// and run the PDFreactor Web Service (Java/Jetty) locally or remotely.
using RealObjects.PDFreactor.Webservice.Client;
using System.IO;

class Program
{
    static void Main()
    {
        PDFreactor pdfReactor = new PDFreactor();
        
        string html = "<html><body><h1>Document with Headers</h1><p>Content here</p></body></html>";
        
        Configuration config = new Configuration();
        config.Document = html;
        config.UserStyleSheets = new System.Collections.Generic.List<Resource>
        {
            new Resource { Content = "@page { @top-center { content: 'Header Text'; } @bottom-center { content: 'Page ' counter(page); } }" }
        };
        
        Result result = pdfReactor.Convert(config);
        
        File.WriteAllBytes("document.pdf", result.Document);
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
        
        renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
        {
            CenterText = "Header Text"
        };
        
        renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
        {
            CenterText = "Page {page}"
        };
        
        string html = "<html><body><h1>Document with Headers</h1><p>Content here</p></body></html>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        
        pdf.SaveAs("document.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert a URL to PDF in .NET?

Here's how **PDFreactor** handles this:

```csharp
// PDFreactor .NET wrapper is a Web Service client (no NuGet package).
// Reference PDFreactor.dll from <PDFreactor-install>/clients/netstandard2/bin/
// and run the PDFreactor Web Service (Java/Jetty) locally or remotely.
using RealObjects.PDFreactor.Webservice.Client;
using System.IO;

class Program
{
    static void Main()
    {
        PDFreactor pdfReactor = new PDFreactor();
        
        Configuration config = new Configuration();
        config.Document = "https://www.example.com";
        
        Result result = pdfReactor.Convert(config);
        
        File.WriteAllBytes("webpage.pdf", result.Document);
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

## How Can I Migrate from PDFreactor to IronPDF?

### The Java Dependency Problem

PDFreactor requires a Java runtime and runs as a separate server process, creating significant complexity:

1. **Java Runtime Required**: Must install and maintain JRE/JDK on all servers
2. **Server Architecture**: Runs as a separate service requiring REST API calls
3. **Complex Deployment**: Two runtimes (Java + .NET) to manage in CI/CD pipelines
4. **Network Latency**: Every PDF conversion requires HTTP round-trip to server
5. **Separate Infrastructure**: Additional server to monitor, scale, and maintain
6. **License Complexity**: Per-server licensing tied to Java service instance

### Quick Migration Overview

| Aspect | PDFreactor | IronPDF |
|--------|-----------|---------|
| Runtime | Java (external server) | Native .NET (in-process) |
| Architecture | REST API service | NuGet library |
| Deployment | Java + server config | Single NuGet package |
| Dependencies | JRE + HTTP client | Self-contained |
| Latency | Network round-trip | Direct method calls |
| CSS Support | CSS Paged Media | Chromium engine |
| PDF Manipulation | Conversion only | Full lifecycle |

### Key API Mappings

| PDFreactor | IronPDF | Notes |
|------------|---------|-------|
| `new PDFreactor(serverUrl)` | `new ChromePdfRenderer()` | No server needed |
| `config.Document = html` | `renderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| `config.Document = url` | `renderer.RenderUrlAsPdf(url)` | URL to PDF |
| `config.PageFormat = PageFormat.A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Paper size |
| `config.PageOrientation` | `RenderingOptions.PaperOrientation` | Orientation |
| `config.PageMargins` | `RenderingOptions.MarginTop/Bottom/Left/Right` | Margins (mm) |
| `config.EnableJavaScript = true` | `RenderingOptions.EnableJavaScript = true` | JS execution |
| `config.UserStyleSheets.Add(new Resource { Content = css })` | Embed CSS in HTML | CSS injection |
| `result.Document` (byte[]) | `pdf.BinaryData` | Raw bytes |
| `pdfReactor.Convert(config)` | `renderer.RenderHtmlAsPdf(html)` | Convert |
| `config.Title` | `pdf.MetaData.Title` | Metadata |
| `config.Encryption` | `pdf.SecuritySettings` | Security |
| _(not available)_ | `PdfDocument.Merge()` | NEW: Merge PDFs |
| _(not available)_ | `pdf.ApplyWatermark()` | NEW: Watermarks |
| _(not available)_ | `pdf.ExtractAllText()` | NEW: Text extraction |

### Migration Code Example

**Before (PDFreactor):**
```csharp
using RealObjects.PDFreactor.Webservice.Client;
using System.Collections.Generic;
using System.IO;

var pdfReactor = new PDFreactor("http://localhost:9423/service/rest");

var config = new Configuration
{
    Document = "<h1>Report</h1>",
    PageFormat = PageFormat.A4,
    PageOrientation = Orientation.LANDSCAPE,
    EnableJavaScript = true
};

config.UserStyleSheets = new List<Resource>
{
    new Resource { Content = "@page { @bottom-center { content: 'Page ' counter(page); } }" }
};

Result result = pdfReactor.Convert(config);
File.WriteAllBytes("report.pdf", result.Document);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Page {page}"
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1>");
pdf.SaveAs("report.pdf");
```

### Critical Migration Notes

1. **No Server Required**: IronPDF runs in-process—no Java server to configure
   ```csharp
   // PDFreactor: new PDFreactor("http://localhost:9423/service/rest")
   // IronPDF: new ChromePdfRenderer()  // No server URL
   ```

2. **CSS Paged Media → IronPDF API**: Replace CSS `@page` rules with RenderingOptions
   ```csharp
   // PDFreactor CSS: @page { @bottom-center { content: 'Page ' counter(page); } }
   // IronPDF: renderer.RenderingOptions.TextFooter = new TextHeaderFooter { CenterText = "Page {page}" };
   ```

3. **Margin Units**: PDFreactor uses strings ("1in"); IronPDF uses millimeters
   ```csharp
   // PDFreactor: config.PageMargins.Top = "1in"
   // IronPDF: renderer.RenderingOptions.MarginTop = 25.4  // 1 inch in mm
   ```

4. **Result Handling**: Configuration + Result → Direct PdfDocument
   ```csharp
   // PDFreactor: Result result = pdfReactor.Convert(config); byte[] bytes = result.Document;
   // IronPDF: var pdf = renderer.RenderHtmlAsPdf(html); byte[] bytes = pdf.BinaryData;
   ```

### Package Migration

```bash
# PDFreactor isn't on NuGet — remove the direct PDFreactor.dll reference
# from your .csproj, decommission the PDFreactor Web Service, then:

# Install IronPDF (single in-process NuGet, no Java runtime)
dotnet add package IronPdf
```

### Find All PDFreactor References

```bash
# Find PDFreactor usage
grep -r "PDFreactor\|RealObjects\|Configuration.*Document" --include="*.cs" .

# Find CSS Paged Media rules to convert
grep -r "@page\|counter(page)\|counter(pages)" --include="*.cs" --include="*.css" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping (40+ methods and properties)
- 10 detailed code conversion examples
- CSS Paged Media to IronPDF API migration
- Header/footer placeholder conversion
- Server architecture elimination guide
- JavaScript and async content handling
- Docker deployment examples (Java removal)
- Pre/post migration checklists

**[Complete Migration Guide: PDFreactor → IronPDF](migrate-from-pdfreactor.md)**


## Conclusion

Selecting the right tool between PDFreactor and IronPDF largely depends on specific project requirements and existing infrastructure. If your project demands high-fidelity rendering with extensive CSS support and can handle Java dependencies, PDFreactor is a strong candidate. Conversely, if you're developing within a .NET environment and desire seamless integration and extensive PDF functionalities, IronPDF is a more appropriate choice.

Ultimately, both tools offer robust solutions for HTML-to-PDF conversion with their unique sets of advantages and trade-offs. Thoroughly assessing your project needs, constraints, and environment will guide you in making an informed decision.

---

Jacob Mellor is the Chief Technology Officer at Iron Software, where he leads a 50+ person engineering team building industry-leading .NET components. With 41 years of coding experience, he has architected solutions that have been trusted by developers worldwide, resulting in over 41 million NuGet downloads. Jacob brings deep technical expertise in document processing, PDF generation, and enterprise software development. Connect with him on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).