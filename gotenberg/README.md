# Gotenberg + C# + PDF

Gotenberg is an open-source, Docker-based document conversion API that serves as a solution for developers looking to handle document transformations like HTML to PDF. Designed with a microservices architecture, Gotenberg leverages the capabilities of Docker to encapsulate its operations in a containerized environment. Although Gotenberg is powerful in terms of flexibility, its requirement for Docker introduces certain complexities and infrastructure overhead. For C# developers seeking seamless integration into their projects, understanding the trade-offs of using Gotenberg is crucial.

## Overview of Gotenberg

Gotenberg provides a robust platform for converting HTML, Markdown, and Office documents to PDFs. It is deployed as a Docker container, making it inherently cross-platform and adaptable to many environments. However, this advantage comes with the downside of requiring container orchestration and service management, such as Kubernetes. Gotenberg's reliance on Docker introduces infrastructure overhead and potentially increases cold start latency due to the need to initialize containers upon request.

### Strengths of Gotenberg

- **Flexibility**: Can handle a variety of document formats for conversion.
- **Open Source**: Distributed under the MIT license, allowing free use and modification.
- **Cross-Platform Compatibility**: Thanks to Docker, it runs consistently across different systems.

### Weaknesses of Gotenberg

- **Infrastructure Overhead**: Requires deployment in a Docker environment, which can be complex without prior containerization experience.
- **Network Dependency**: The necessity for network communication with a standalone service can introduce latency.
- **Cold Start Latency**: Container start-up times can lead to initial request delays.

## IronPDF: An Alternative

IronPDF presents a contrasting approach to document conversion, emphasizing simplicity and efficiency. Unlike Gotenberg, IronPDF integrates directly into your C# application as a library, eliminating the need for additional infrastructure or network overhead.

### Advantages of IronPDF

- **In-Process Generation**: This results in no network latency and immediate response times.
- **Simpler Deployment**: No requirement for Docker or any containers; installable via NuGet.
- **Wide Range of Features**: Provides extensive functionalities for PDF generation, manipulation, and conversion.
  
For further exploration of IronPDF, consider the [IronPDF HTML to PDF tutorial](https://ironpdf.com/how-to/html-file-to-pdf/) or delve into various [IronPDF tutorials](https://ironpdf.com/tutorials/).

## Comparative Analysis

Below is a comparative analysis of Gotenberg and IronPDF, outlining key differences and aspects to consider:

| Feature                     | Gotenberg                                  | IronPDF                                      |
|-----------------------------|--------------------------------------------|----------------------------------------------|
| **Deployment**              | Docker required                            | NuGet package                                |
| **Latency**                 | Network and container start latency        | Minimal latency due to in-process execution  |
| **Ease of Use**             | Requires container orchestration knowledge | Simple installation and use                  |
| **Cost**                    | Free (MIT License)                         | Commercial license                           |
| **Integration**             | Requires REST API calls                    | Direct library integration with C#           |
| **Flexibility**             | Supports multiple formats                  | Extensive PDF features and controls          |

## C# Code Example with Gotenberg

Here's a simple demonstration of how you might implement a request to Gotenberg using C# to convert HTML content to a PDF document:

```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public class GotenbergExample
{
    private static readonly HttpClient httpClient = new HttpClient();
    
    public static async Task ConvertHtmlToPdf(string htmlFilePath, string pdfOutputPath)
    {
        var requestUri = "http://localhost:3000/forms/libreoffice/convert";

        using (var content = new MultipartFormDataContent())
        {
            var fileContent = new StreamContent(File.OpenRead(htmlFilePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html");
            content.Add(fileContent, "files", Path.GetFileName(htmlFilePath));

            HttpResponseMessage response = await httpClient.PostAsync(requestUri, content);
            response.EnsureSuccessStatusCode();

            await using (var fileStream = new FileStream(pdfOutputPath, FileMode.Create, FileAccess.Write))
            {
                await response.Content.CopyToAsync(fileStream);
            }
        }

        Console.WriteLine($"Converted '{htmlFilePath}' to '{pdfOutputPath}'");
    }

    public static async Task Main(string[] args)
    {
        await ConvertHtmlToPdf("example.html", "output.pdf");
    }
}
```

---

## How Do I Convert HTML to PDF in C# with Gotenberg?

Here's how **Gotenberg** handles this:

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

class GotenbergExample
{
    static async Task Main()
    {
        var gotenbergUrl = "http://localhost:3000/forms/chromium/convert/html";
        
        using var client = new HttpClient();
        using var content = new MultipartFormDataContent();
        
        var html = "<html><body><h1>Hello from Gotenberg</h1></body></html>";
        content.Add(new StringContent(html), "files", "index.html");
        
        var response = await client.PostAsync(gotenbergUrl, content);
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        
        await File.WriteAllBytesAsync("output.pdf", pdfBytes);
        Console.WriteLine("PDF generated successfully");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using System;
using IronPdf;

class IronPdfExample
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        var html = "<html><body><h1>Hello from IronPDF</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        
        pdf.SaveAs("output.pdf");
        Console.WriteLine("PDF generated successfully");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert a URL to PDF in .NET?

Here's how **Gotenberg** handles this:

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

class GotenbergUrlToPdf
{
    static async Task Main()
    {
        var gotenbergUrl = "http://localhost:3000/forms/chromium/convert/url";
        
        using var client = new HttpClient();
        using var content = new MultipartFormDataContent();
        
        content.Add(new StringContent("https://example.com"), "url");
        
        var response = await client.PostAsync(gotenbergUrl, content);
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        
        await File.WriteAllBytesAsync("webpage.pdf", pdfBytes);
        Console.WriteLine("PDF from URL generated successfully");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using System;
using IronPdf;

class IronPdfUrlToPdf
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        
        pdf.SaveAs("webpage.pdf");
        Console.WriteLine("PDF from URL generated successfully");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Create Custom-Sized PDFs?

Here's how **Gotenberg** handles this:

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

class GotenbergCustomSize
{
    static async Task Main()
    {
        var gotenbergUrl = "http://localhost:3000/forms/chromium/convert/html";
        
        using var client = new HttpClient();
        using var content = new MultipartFormDataContent();
        
        var html = "<html><body><h1>Custom Size PDF</h1></body></html>";
        content.Add(new StringContent(html), "files", "index.html");
        content.Add(new StringContent("8.5"), "paperWidth");
        content.Add(new StringContent("11"), "paperHeight");
        content.Add(new StringContent("0.5"), "marginTop");
        content.Add(new StringContent("0.5"), "marginBottom");
        
        var response = await client.PostAsync(gotenbergUrl, content);
        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        
        await File.WriteAllBytesAsync("custom-size.pdf", pdfBytes);
        Console.WriteLine("Custom size PDF generated successfully");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using System;
using IronPdf;
using IronPdf.Rendering;

class IronPdfCustomSize
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
        renderer.RenderingOptions.MarginTop = 50;
        renderer.RenderingOptions.MarginBottom = 50;
        
        var html = "<html><body><h1>Custom Size PDF</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        
        pdf.SaveAs("custom-size.pdf");
        Console.WriteLine("Custom size PDF generated successfully");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from Gotenberg to IronPDF?

Gotenberg requires significant infrastructure overhead including Docker containers, Kubernetes orchestration, service discovery, and load balancing, adding complexity to your deployment pipeline. Every PDF generation requires a network call to a separate service, introducing latency and potential failure points.

**Migrating from Gotenberg to IronPDF involves:**

1. **NuGet Package Change**: Install `IronPdf` package
2. **Namespace Update**: Use `IronPdf` namespace
3. **API Adjustments**: Update your code to use IronPDF's modern API patterns

**Key Benefits of Migrating:**

- Modern Chromium rendering engine with full CSS/JavaScript support
- Active maintenance and security updates
- Better .NET integration and async/await support
- Comprehensive documentation and professional support

For a complete step-by-step migration guide with detailed code examples and common gotchas, see:
**[Complete Migration Guide: Gotenberg → IronPDF](migrate-from-gotenberg.md)**


## Conclusion

Both Gotenberg and IronPDF offer distinct approaches to PDF generation and conversion within C# environments. Gotenberg is an excellent option for those willing to manage Docker-based services and relish flexibility across different document types. On the flip side, IronPDF simplifies the entire process with a direct, code-based approach that removes the need for additional infrastructure, offering a quick and seamless integration experience. Ultimately, the choice between the two will hinge on your specific project requirements and existing operational resources.

---

Jacob Mellor serves as Chief Technology Officer at Iron Software, where he leads a 50+ person engineering team in developing .NET components that have achieved over 41 million NuGet downloads. With four decades of coding experience, he specializes in building enterprise-grade PDF and document processing solutions for the .NET ecosystem. Based in Chiang Mai, Thailand, Jacob continues to drive technical innovation in developer tools and libraries. Connect with him on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
