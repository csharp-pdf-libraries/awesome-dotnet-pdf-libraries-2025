# CraftMyPDF + C# + PDF

In the ever-evolving landscape of digital document management, businesses and developers are always on the lookout for reliable and efficient PDF generation solutions. Among the plethora of options available, CraftMyPDF and IronPDF stand out as two distinct approaches to handling PDF creation. The former is a cloud-based, template-driven API, while the latter is a versatile C# library that offers more flexibility and control.

## Overview of CraftMyPDF

CraftMyPDF is a powerful API designed to facilitate the creation of PDF documents. One of its standout features is its web-based drag-and-drop editor that allows users to design PDF templates directly in the browser. This makes CraftMyPDF particularly useful for creating reusable templates and high-quality PDFs from JSON data. The editor includes a variety of layout components and supports advanced formatting, expressions, and data binding, offering a robust toolset to meet diverse PDF generation requirements.

That said, CraftMyPDF has its limitations. The API is template-locked, meaning you must use their template designer, which might restrict your creative freedom. Furthermore, being a cloud-only service, it lacks an on-premise deployment option, potentially presenting challenges for businesses with strict data governance policies. Lastly, CraftMyPDF operates on a commercial subscription model, necessitating ongoing monthly payments to access the service.

## Overview of IronPDF

[IronPDF](https://ironpdf.com) offers a different perspective on PDF creation. It's a .NET library that allows developers to convert HTML files into PDFs effortlessly. One of its significant advantages is the flexibility to use any HTML as a template, bypassing the constraints of a specific designer tool. Unlike CraftMyPDF, IronPDF provides an on-premise option, making it suitable for organizations focused on maintaining tight control over their infrastructure and data. Furthermore, IronPDF offers a perpetual license, allowing businesses to pay once and use the library indefinitely, which can be more cost-effective over time.

## C# Code Example

Below is an example of how IronPDF can be used to convert an HTML file into a PDF document:

```csharp
using IronPdf;

public class PdfGenerator
{
    public static void GeneratePdfFromHtml(string htmlFilePath, string outputPdfPath)
    {
        var Renderer = new HtmlToPdf();
        
        // Load the HTML file
        var PDF = Renderer.RenderHTMLFileAsPdf(htmlFilePath);

        // Save the PDF document
        PDF.SaveAs(outputPdfPath);
    }
}

// Usage
PdfGenerator.GeneratePdfFromHtml("sample.html", "output.pdf");
```
For more tutorials on how to utilize IronPDF, visit [IronPDF Tutorials](https://ironpdf.com/tutorials/).

---

## How Do I Convert HTML to PDF in C# with CraftMyPDF?

Here's how **CraftMyPDF** handles this:

```csharp
// NuGet: Install-Package RestSharp
using System;
using RestSharp;
using System.IO;

class Program
{
    static void Main()
    {
        var client = new RestClient("https://api.craftmypdf.com/v1/create");
        var request = new RestRequest(Method.POST);
        request.AddHeader("X-API-KEY", "your-api-key");
        request.AddJsonBody(new
        {
            template_id = "your-template-id",
            data = new
            {
                html = "<h1>Hello World</h1><p>This is a PDF from HTML</p>"
            }
        });
        
        var response = client.Execute(request);
        File.WriteAllBytes("output.pdf", response.RawBytes);
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>This is a PDF from HTML</p>");
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Add Headers and Footers to PDFs?

Here's how **CraftMyPDF** handles this:

```csharp
// NuGet: Install-Package RestSharp
using System;
using RestSharp;
using System.IO;

class Program
{
    static void Main()
    {
        var client = new RestClient("https://api.craftmypdf.com/v1/create");
        var request = new RestRequest(Method.POST);
        request.AddHeader("X-API-KEY", "your-api-key");
        request.AddJsonBody(new
        {
            template_id = "your-template-id",
            data = new
            {
                html = "<h1>Document Content</h1>",
                header = "<div>Page Header</div>",
                footer = "<div>Page {page} of {total_pages}</div>"
            }
        });
        
        var response = client.Execute(request);
        File.WriteAllBytes("document.pdf", response.RawBytes);
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using System;
using IronPdf;
using IronPdf.Rendering;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
        {
            CenterText = "Page Header"
        };
        renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
        {
            CenterText = "Page {page} of {total-pages}"
        };
        
        var pdf = renderer.RenderHtmlAsPdf("<h1>Document Content</h1>");
        pdf.SaveAs("document.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert a URL to PDF in .NET?

Here's how **CraftMyPDF** handles this:

```csharp
// NuGet: Install-Package RestSharp
using System;
using RestSharp;
using System.IO;

class Program
{
    static void Main()
    {
        var client = new RestClient("https://api.craftmypdf.com/v1/create");
        var request = new RestRequest(Method.POST);
        request.AddHeader("X-API-KEY", "your-api-key");
        request.AddJsonBody(new
        {
            template_id = "your-template-id",
            data = new
            {
                url = "https://example.com"
            },
            export_type = "pdf"
        });
        
        var response = client.Execute(request);
        File.WriteAllBytes("webpage.pdf", response.RawBytes);
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from CraftMyPDF to IronPDF?

### The Problem with Cloud-Based PDF APIs

CraftMyPDF and similar cloud PDF services introduce fundamental issues:

1. **Your Data Leaves Your System**: Every HTML template and JSON payload is transmitted to third-party servers—creating HIPAA, GDPR, and SOC2 compliance risks for sensitive documents like invoices, contracts, and medical records.

2. **Network Latency**: CraftMyPDF's own documentation states 1.5-30 seconds per PDF. IronPDF generates locally in milliseconds.

3. **Print-Optimized Output**: Cloud APIs often optimize for print—reducing backgrounds and simplifying colors to save "ink." The result never looks like your HTML on screen.

4. **Per-PDF Costs Add Up**: 10,000 PDFs/month at $0.01-0.05 each vs. one-time perpetual license.

### Quick Migration Overview

| Aspect | CraftMyPDF | IronPDF |
|--------|------------|---------|
| Data Location | Cloud (leaves your system) | On-premise (stays local) |
| Latency | 1.5-30 seconds per PDF | Milliseconds |
| Pricing | Per-PDF subscription | One-time perpetual license |
| Template System | Proprietary drag-and-drop | Any HTML/CSS/JavaScript |
| Output Quality | Print-optimized | Pixel-perfect screen rendering |
| Works Offline | No (requires internet) | Yes |
| Compliance | Data leaves organization | SOC2/HIPAA friendly |

### Key API Mappings

| CraftMyPDF | IronPDF | Notes |
|------------|---------|-------|
| `POST /v1/create` | `renderer.RenderHtmlAsPdf(html)` | No API call needed |
| `X-API-KEY` header | `License.LicenseKey = "..."` | Set once at startup |
| `template_id` | Standard HTML string | Use any HTML |
| `{%name%}` placeholders | `$"{name}"` C# interpolation | Standard .NET |
| `POST /v1/merge` | `PdfDocument.Merge(pdfs)` | Local, instant |
| `POST /v1/add-watermark` | `pdf.ApplyWatermark(html)` | HTML-based |
| Webhook callbacks | Not needed | Results are synchronous |
| Rate limiting | Not applicable | No limits |

### Migration Code Example

**Before (CraftMyPDF):**
```csharp
using RestSharp;
using System.IO;

var client = new RestClient("https://api.craftmypdf.com/v1/create");
var request = new RestRequest(Method.POST);
request.AddHeader("X-API-KEY", "your-api-key");
request.AddJsonBody(new
{
    template_id = "invoice-template-id",
    data = new
    {
        customer = "John Doe",
        amount = "$1,000",
        items = invoiceItems
    }
});

var response = await client.ExecuteAsync(request);
// Handle rate limits, network errors, timeouts...
if (response.IsSuccessful)
    File.WriteAllBytes("invoice.pdf", response.RawBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

var html = $@"
<html>
<body>
    <h1>Invoice</h1>
    <p>Customer: John Doe</p>
    <p>Amount: $1,000</p>
    {GenerateItemsTable(invoiceItems)}
</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
// No network, no API key, no rate limits, instant!
```

### Critical Migration Notes

1. **Remove All HTTP Code**: No RestClient, no API calls, no response handling—IronPDF runs locally.

2. **One-Time License**: Replace per-request `X-API-KEY` headers with single `License.LicenseKey` at app startup.

3. **Template → HTML**: Convert proprietary `{%variable%}` placeholders to C# string interpolation `$"{variable}"`.

4. **Sync by Default**: Remove async/await—IronPDF is synchronous (async methods available if needed).

5. **No Rate Limits**: Remove all retry logic, delays, and rate limit handling—generate unlimited PDFs.

### NuGet Package Migration

```bash
# Remove HTTP client
dotnet remove package RestSharp

# Install IronPDF
dotnet add package IronPdf
```

### Find All CraftMyPDF References

```bash
grep -r "api.craftmypdf.com\|X-API-KEY\|template_id" --include="*.cs" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API endpoint mappings
- 10 detailed code conversion examples
- Template placeholder conversion patterns
- Cloud-to-local architecture changes
- Performance comparison (15-30x faster)
- Docker deployment configuration
- Troubleshooting guide for 8+ common issues
- Pre/post migration checklists

**[Complete Migration Guide: CraftMyPDF → IronPDF](migrate-from-craftmypdf.md)**


## Comparison Table

Here's a quick comparison of CraftMyPDF and IronPDF:

| Feature                         | CraftMyPDF                                         | IronPDF                                                 |
|---------------------------------|----------------------------------------------------|---------------------------------------------------------|
| Template Design                 | Must use CraftMyPDF's template designer            | Use any HTML as a template                              |
| Deployment                      | Cloud-only                                         | On-premise option available                             |
| Licensing                       | Ongoing subscription                               | Perpetual license option available                      |
| Data Binding & Expressions      | Advanced support with CraftMyPDF editor            | Achieved through custom HTML and C# logic               |
| Code Language                   | API available across platforms                     | C# library for .NET ecosystems                          |

## Strengths and Weaknesses

### CraftMyPDF Strengths
- **User-Friendly Interface**: The web-based drag-and-drop editor simplifies the template creation process.
- **Advanced Formatting**: Supports complex expressions, data binding, and a rich set of layout components.

### CraftMyPDF Weaknesses
- **Template-Locked**: Users are constrained to the CraftMyPDF template designer.
- **Cloud-Only Solution**: No option for on-premise deployment, which can be a dealbreaker for some organizations.
- **Subscription Model**: Requires ongoing monthly payments.

### IronPDF Strengths
- **Template Flexibility**: Freedom to use any HTML as a template, providing creative liberty.
- **On-Premise Deployment**: Suitable for organizations that require strict data control.
- **Cost-Effective Licensing**: Offers a perpetual licensing model that can be more economical over time.

### IronPDF Weaknesses
- **Requires C# Knowledge**: As a C# library, it necessitates a certain level of coding proficiency.
- **Initial Setup**: May require more initial setup compared to plug-and-play SaaS solutions like CraftMyPDF.

In conclusion, the choice between CraftMyPDF and IronPDF largely depends on your specific project requirements and constraints. CraftMyPDF is an excellent option for businesses seeking a turnkey, template-based PDF generation solution, especially if they value ease of use over customization. In contrast, IronPDF offers unparalleled flexibility and control, particularly beneficial for developers seeking to integrate PDF functionality into custom applications with greater security requirements.

For those interested, you can explore more on converting HTML to PDF with IronPDF through this detailed [guide](https://ironpdf.com/how-to/html-file-to-pdf/).

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person team building .NET tools that have been downloaded over 41 million times on NuGet. With four decades of coding experience, he's passionate about the .NET ecosystem and cross-platform development, creating solutions now used by space agencies and automotive giants. Based in Chiang Mai, Thailand, Jacob shares his insights on [Medium](https://medium.com/@jacob.mellor) and [GitHub](https://github.com/jacob-mellor).
