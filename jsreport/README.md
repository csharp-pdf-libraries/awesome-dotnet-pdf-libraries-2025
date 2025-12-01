# jsreport + C# + PDF

In the world of C# and PDF generation, jsreport stands out as a versatile tool that allows developers to produce high-quality reports using web technologies. With jsreport being a node.js-based platform, developers can leverage HTML, CSS, and JavaScript to design reports, which is an advantage for web development teams familiar with these technologies. However, to fully utilize jsreport in a .NET environment, developers must integrate it using its .NET SDK. This integration opens up the possibilities for using jsreport twice within traditional C# applications, while allowing developers to design highly visual documents easily.

## jsreport Strengths and Weaknesses

### Strengths
- **Web Skills Leverage**: jsreport allows developers to utilize existing HTML, CSS, and JavaScript skills, making it easier for those familiar with web development to jump into report generation without needing to learn new paradigms.
- **Microservice Friendly**: Running as a standalone server, jsreport fits naturally into microservices architectures where it can be deployed as a separate service, ready to handle report requests from multiple applications.
- **REST API**: Its API, accessible over REST, makes it a good fit for web applications and integration with other services, regardless of where they are hosted.

### Weaknesses
- **Node.js Dependency**: It requires a Node.js runtime and a separate server, which could be a downside for teams focused strictly on C# environments who prefer native .NET solutions.
- **Complex Templating System**: Learning the jsreport templating system, which includes mastering the use of JavaScript templating engines like Handlebars or JsRender, can present a learning curve.
- **Licensing Model**: While there's a free tier, it limits the number of templates one can use, and scaling up to an enterprise might lead to increased costs.

### Comparison to IronPDF

While jsreport offers a robust approach for those well-versed in JavaScript and web technologies, there's a notable alternative in the form of IronPDF, particularly for developers seeking a C# native solution for PDF generation.

#### IronPDF Advantages

- **Native C# Library**: IronPDF integrates seamlessly within .NET projects without the need for an additional server or Node.js environment, streamlining the development and deployment process.
- **HTML/Razor Usage**: IronPDF allows the use of existing HTML and Razor skills, making it easier for C# developers to create PDFs.
- **Comprehensive Documentation and Tutorials**: With resources such as [IronPDF HTML File to PDF](https://ironpdf.com/how-to/html-file-to-pdf/) and [IronPDF Tutorials](https://ironpdf.com/tutorials/), developers can quickly get up to speed and implement PDF generation features.

### Comparison Table

| Criteria                   | jsreport                           | IronPDF                           |
|----------------------------|------------------------------------|-----------------------------------|
| Technology Base            | Node.js                            | Native C#                         |
| Server Requirement         | Yes (separate server)              | No                                |
| Templating System          | HTML, CSS, JavaScript              | HTML, Razor                       |
| Developer Skills Required  | Web technologies                   | C#                                |
| Licensing                  | Commercial (Free tier available)   | Commercial with perpetual license |
| Integration Complexity     | Requires API interaction           | Integrated as a library           |

### jsreport in C# Integration Example

To demonstrate how to use jsreport within a C# application, let’s consider a simple scenario that shows how to generate a PDF report using the jsreport's .NET SDK. This integration will showcase how a C# application sends a request to the jsreport server, specifying a template and data, and then retrieves the PDF result.

```csharp
using jsreport.Local;
using jsreport.Types;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var rs = new LocalReporting()
                   .UseBinary(JsReportBinary.GetBinary()) // points to the jsreport binary
                   .AspNetCore().Renderer(new AspNetCore.ReportingOptions())
                   .Create();

        var report = await rs.RenderAsync(new RenderRequest
        {
            Template = new Template
            {
                Content = "<h1>Hello, {{name}}</h1><p>This is a test of jsreport and C# integration.</p>",
                Engine = Engine.Handlebars,
                Recipe = Recipe.ChromePdf
            },
            Data = new { name = "World" }
        });

        await report.Content.CopyToAsync(File.OpenWrite("report.pdf"));
    }
}
```

In this example, the `LocalReporting` class initializes a jsreport server locally from within the .NET application. The request specifies an HTML template with the Handlebars templating engine and uses the `chrome-pdf` recipe to generate the PDF. The resulting PDF is stored locally as `report.pdf`.

### Concluding Thoughts

When choosing between jsreport and IronPDF, the decision largely hinges on your existing development environment and expertise. If your team is comfortable with Node.js and seeks the power of leveraging HTML, CSS, and JavaScript in reports, jsreport can be quite advantageous. However, if your projects are primarily C# and you need a library that integrates directly without additional infrastructure, IronPDF's native C# library provides a simpler, more directly integrated approach.

Ultimately, both libraries offer the capability to produce high-quality PDFs, but the decision should align with team capacity to manage external servers (in the case of jsreport) versus sticking to a purely C# environment (as with IronPDF).

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person team building developer tools that have earned over 41 million NuGet downloads. With an impressive 41 years of coding under his belt, he's seen the software industry evolve from its earliest days to the cutting-edge tech of today. When he's not architecting solutions for developers worldwide, you can find him working remotely from Chiang Mai, Thailand—check out his [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) to connect.

---

## How Do I Convert HTML to PDF in C# with jsreport?

Here's how **jsreport** handles this:

```csharp
// NuGet: Install-Package jsreport.Binary
// NuGet: Install-Package jsreport.Local
// NuGet: Install-Package jsreport.Types
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var rs = new LocalReporting()
            .UseBinary(JsReportBinary.GetBinary())
            .AsUtility()
            .Create();

        var report = await rs.RenderAsync(new RenderRequest()
        {
            Template = new Template()
            {
                Recipe = Recipe.ChromePdf,
                Engine = Engine.None,
                Content = "<h1>Hello from jsreport</h1><p>This is a PDF document.</p>"
            }
        });

        using (var fileStream = File.Create("output.pdf"))
        {
            report.Content.CopyTo(fileStream);
        }

        Console.WriteLine("PDF created successfully!");
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
    static void Main(string[] args)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello from IronPDF</h1><p>This is a PDF document.</p>");
        pdf.SaveAs("output.pdf");
        Console.WriteLine("PDF created successfully!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert a URL to PDF in .NET?

Here's how **jsreport** handles this:

```csharp
// NuGet: Install-Package jsreport.Binary
// NuGet: Install-Package jsreport.Local
// NuGet: Install-Package jsreport.Types
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var rs = new LocalReporting()
            .UseBinary(JsReportBinary.GetBinary())
            .AsUtility()
            .Create();

        var report = await rs.RenderAsync(new RenderRequest()
        {
            Template = new Template()
            {
                Recipe = Recipe.ChromePdf,
                Engine = Engine.None,
                Content = "<html><body><script>window.location='https://example.com';</script></body></html>"
            }
        });

        using (var fileStream = File.Create("webpage.pdf"))
        {
            report.Content.CopyTo(fileStream);
        }

        Console.WriteLine("Webpage PDF created successfully!");
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
    static void Main(string[] args)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
        Console.WriteLine("Webpage PDF created successfully!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I HTML To PDF Headers Footers?

Here's how **jsreport** handles this:

```csharp
// NuGet: Install-Package jsreport.Binary
// NuGet: Install-Package jsreport.Local
// NuGet: Install-Package jsreport.Types
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var rs = new LocalReporting()
            .UseBinary(JsReportBinary.GetBinary())
            .AsUtility()
            .Create();

        var report = await rs.RenderAsync(new RenderRequest()
        {
            Template = new Template()
            {
                Recipe = Recipe.ChromePdf,
                Engine = Engine.None,
                Content = "<h1>Document with Header and Footer</h1><p>Main content goes here.</p>",
                Chrome = new Chrome()
                {
                    DisplayHeaderFooter = true,
                    HeaderTemplate = "<div style='font-size:10px; text-align:center; width:100%;'>Custom Header</div>",
                    FooterTemplate = "<div style='font-size:10px; text-align:center; width:100%;'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></div>"
                }
            }
        });

        using (var fileStream = File.Create("document_with_headers.pdf"))
        {
            report.Content.CopyTo(fileStream);
        }

        Console.WriteLine("PDF with headers and footers created successfully!");
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
    static void Main(string[] args)
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
        {
            CenterText = "Custom Header",
            FontSize = 10
        };
        renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
        {
            CenterText = "Page {page} of {total-pages}",
            FontSize = 10
        };
        
        var pdf = renderer.RenderHtmlAsPdf("<h1>Document with Header and Footer</h1><p>Main content goes here.</p>");
        pdf.SaveAs("document_with_headers.pdf");
        Console.WriteLine("PDF with headers and footers created successfully!");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from jsreport to IronPDF?

### The jsreport Challenges

jsreport introduces complexity that doesn't belong in a pure .NET environment:

1. **Node.js Dependency**: Requires Node.js runtime and binaries, adding infrastructure complexity
2. **External Binary Management**: Must download/manage platform-specific binaries (Windows, Linux, OSX)
3. **Separate Server Process**: Runs as utility or web server—additional process management needed
4. **JavaScript Templating**: Forces learning Handlebars, JsRender, or other JS templating systems
5. **Complex Request Structure**: Verbose `RenderRequest` with nested `Template` objects
6. **Stream-Based Output**: Returns streams requiring manual file operations

### Quick Migration Overview

| Aspect | jsreport | IronPDF |
|--------|----------|---------|
| Runtime | Node.js + .NET | Pure .NET |
| Binary Management | Manual (jsreport.Binary packages) | Automatic |
| Server Process | Required (utility or web server) | In-process |
| Templating | JavaScript (Handlebars) | C# (Razor, string interpolation) |
| API Style | Verbose request objects | Clean fluent methods |
| Output | Stream | PdfDocument object |
| Async Support | Async-only | Both sync and async |

### Key API Mappings

| jsreport | IronPDF | Notes |
|----------|---------|-------|
| `new LocalReporting().UseBinary().AsUtility().Create()` | `new ChromePdfRenderer()` | One-liner |
| `rs.RenderAsync(request)` | `renderer.RenderHtmlAsPdf(html)` | Direct call |
| `Template.Content` | First parameter | HTML string |
| `Template.Recipe = Recipe.ChromePdf` | _(not needed)_ | Default |
| `Template.Engine = Engine.Handlebars` | _(not needed)_ | Use C# |
| `Chrome.HeaderTemplate` | `RenderingOptions.HtmlHeader` | HTML |
| `Chrome.FooterTemplate` | `RenderingOptions.HtmlFooter` | HTML |
| `Chrome.MarginTop = "2cm"` | `RenderingOptions.MarginTop = 20` | mm units |
| `Chrome.Format = "A4"` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Enum |
| `Chrome.Landscape = true` | `RenderingOptions.PaperOrientation = Landscape` | Enum |
| `{#pageNum}` | `{page}` | Placeholder |
| `{#numPages}` | `{total-pages}` | Placeholder |
| `rs.StartAsync()` | _(not needed)_ | In-process |
| `rs.KillAsync()` | _(not needed)_ | Auto-cleanup |

### Migration Code Example

**Before (jsreport):**
```csharp
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;

public class JsReportService
{
    private readonly ILocalUtilityReportingService _rs;

    public JsReportService()
    {
        _rs = new LocalReporting()
            .UseBinary(JsReportBinary.GetBinary())
            .AsUtility()
            .Create();
    }

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        var report = await _rs.RenderAsync(new RenderRequest
        {
            Template = new Template
            {
                Recipe = Recipe.ChromePdf,
                Engine = Engine.None,
                Content = html,
                Chrome = new Chrome
                {
                    Format = "A4",
                    DisplayHeaderFooter = true,
                    FooterTemplate = "<div>Page {#pageNum} of {#numPages}</div>",
                    MarginBottom = "2cm"
                }
            }
        });

        using (var ms = new MemoryStream())
        {
            await report.Content.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();

        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div>Page {page} of {total-pages}</div>",
            MaxHeight = 20
        };
        _renderer.RenderingOptions.MarginBottom = 20;
    }

    public byte[] GeneratePdf(string html)
    {
        return _renderer.RenderHtmlAsPdf(html).BinaryData;
    }
}
```

### Critical Migration Notes

1. **No Server Required**: IronPDF runs in-process—delete `StartAsync()`, `KillAsync()`

2. **No Binary Management**: Remove all platform-specific package references:
   ```bash
   dotnet remove package jsreport.Binary
   dotnet remove package jsreport.Binary.Linux
   dotnet remove package jsreport.Binary.OSX
   ```

3. **Placeholder Syntax**: Update all header/footer placeholders:
   - `{#pageNum}` → `{page}`
   - `{#numPages}` → `{total-pages}`
   - `{#timestamp}` → `{date}`

4. **Margin Units**: jsreport uses CSS units ("2cm"), IronPDF uses millimeters:
   ```csharp
   // "2cm" = 20mm, "1in" = 25mm
   ```

5. **Replace Handlebars**: Use C# string interpolation instead:
   ```csharp
   // Handlebars: {{#each items}}...{{/each}}
   // C#: string.Join("", items.Select(i => $"..."))
   ```

### NuGet Package Migration

```bash
# Remove jsreport packages
dotnet remove package jsreport.Binary
dotnet remove package jsreport.Local
dotnet remove package jsreport.Types
dotnet remove package jsreport.Client

# Install IronPDF
dotnet add package IronPdf
```

### Find All jsreport References

```bash
# Find jsreport usage
grep -r "using jsreport\|LocalReporting\|RenderRequest\|RenderAsync" --include="*.cs" .
grep -r "JsReportBinary\|Template\|Recipe\|Engine\." --include="*.cs" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping (30+ methods and properties)
- RenderRequest property mappings
- 10 detailed code conversion examples
- Handlebars → C# template conversion
- Web server mode elimination
- Platform binary removal
- Performance comparison data
- Troubleshooting guide for 8+ common issues
- Pre/post migration checklists

**[Complete Migration Guide: jsreport → IronPDF](migrate-from-jsreport.md)**

