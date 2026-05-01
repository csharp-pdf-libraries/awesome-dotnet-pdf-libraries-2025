# pdforge + C# + PDF

When it comes to generating PDFs in C#, two names often pop up for consideration: pdforge and IronPDF. pdforge (rebranded to "pdf noodle" in 2026 — pdfnoodle.com; the api.pdforge.com hostname continues to work via 301 redirects through end of 2026) is a cloud-based, template-driven PDF generation API offering a straightforward way to produce PDF files by integrating with your application through HTTP calls. There is no official .NET SDK on NuGet — integration is done with `HttpClient` against the documented REST endpoints. This contrasts with IronPDF, a local library providing full control over the PDF generation process without external dependencies. In examining these two solutions, developers will find both distinct advantages and certain limitations.

pdforge's strengths lie in its simplicity and its cloud-based architecture, which can simplify the development process. By offloading the task of PDF creation to an external API, developers can focus on other areas of their application. However, pdforge presents drawbacks such as external dependencies, limited customization options, and ongoing subscription costs that developers should be aware of.

## Key Features and Comparisons

Let's explore the key features of pdforge and IronPDF, and compare them based on various technical and operational aspects.

### How pdforge Works

pdforge is a PDF generation API designed for ease of integration with cloud applications. It allows developers to send HTML, along with required parameters, to generate a PDF document that can be used in various business applications.

Here is a simple code example in C# illustrating how you might use pdforge to generate a PDF:

```csharp
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class PDFExample
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task GeneratePDFAsync()
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "pdfnoodle_api_YOUR_KEY");

        var body = new { html = "<html><body><h1>This is a PDF</h1></body></html>" };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        // Sync HTML-to-PDF endpoint returns a JSON envelope with a signed S3 URL.
        var response = await client.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var signedUrl = doc.RootElement.GetProperty("signedUrl").GetString();
        var pdfBytes = await client.GetByteArrayAsync(signedUrl);
        File.WriteAllBytes("pdforgeSample.pdf", pdfBytes);
    }
}
```

### How IronPDF Works

IronPDF differentiates itself by providing a fully local library, granting developers complete control over the PDF creation process. This is particularly advantageous for applications where internal handling of files is preferred, or where external API calls introduce security concerns.

IronPDF supports a wide range of functionalities for PDF manipulation, including converting HTML to PDF, editing existing PDFs, and extracting content. For detailed tutorials and use cases, you can refer to [IronPDF's HTML to PDF guide](https://ironpdf.com/how-to/html-file-to-pdf/) and general [tutorials](https://ironpdf.com/tutorials/).

### Strengths and Weaknesses

Both pdforge and IronPDF have their own set of strengths and weaknesses, summarized in the following table:

| Feature                 | pdforge                                         | IronPDF                                                      |
|-------------------------|-------------------------------------------------|--------------------------------------------------------------|
| **Deployment Type**     | Cloud-based API                                 | Local library                                                |
| **Dependencies**        | Requires internet and API authentication        | No external dependencies                                     |
| **Customization**       | Limited control over PDF generation             | Full control over customization                              |
| **Cost Structure**      | Ongoing subscription                            | One-time purchase option                                     |
| **Security**            | Potential concerns with data sent over the web  | Keeps data processing entirely within the local environment  |
| **Setup Complexity**    | Easier initial setup due to external handling   | Requires more initial setup and configuration                |

### Use Cases for Each

- **pdforge** is ideal for applications where ease of setup and quick deployment are paramount, especially when there is no existing infrastructure to support PDF generation.
- **IronPDF** suits scenarios requiring significant customization and security, or if the operational environment has restrictions on internet use.

### Concerns

#### Security

When using pdforge, developers need to accommodate security concerns related to data being sent to an external API. If the PDF content includes sensitive information, this could be a critical consideration. On the other hand, IronPDF processes everything locally, minimizing such risks.

#### Cost

pdforge's SaaS model introduces continuous operational expenditure which can accumulate over time. Conversely, IronPDF presents an opportunity for a one-time license acquisition, which could be more cost-effective in the long run.

#### Performance and Reliability

IronPDF, being a local library, may offer better performance as there is no round-trip time involved in web requests. However, pdforge benefits from managed infrastructure that scales, potentially offering reliability in load-balanced environments.

### Conclusion

Deciding between pdforge and IronPDF depends largely on specific project requirements, notably in terms of customization needs, budget, and security considerations. pdforge offers a streamlined entry into PDF generation with minimal setup, trading off some aspects of control and potentially higher long-term costs. IronPDF, on the other hand, offers a more comprehensive suite of tools with robust security benefits for developers able to manage local deployments.

---

Jacob Mellor is the Chief Technology Officer at Iron Software, where he leads a team of 50+ engineers in developing robust .NET components that have achieved over 41 million NuGet downloads. With an impressive 41 years of coding experience, Jacob brings deep technical expertise to Iron Software's mission of empowering developers with reliable, production-ready tools. Based in Chiang Mai, Thailand, he continues to drive innovation in the .NET ecosystem while maintaining a hands-on approach to software architecture and development. Connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
---

## How Do I Convert a URL to PDF in .NET?

Here's how **pdforge** handles this:

```csharp
// REST API — no .NET SDK on NuGet. Integration is HttpClient + JSON POST.
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "pdfnoodle_api_YOUR_KEY");

        // pdf noodle has no dedicated URL endpoint; fetch the page, then POST its HTML.
        var sourceHtml = await http.GetStringAsync("https://example.com");
        var body = new { html = sourceHtml };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
        File.WriteAllBytes("webpage.pdf", pdfBytes);
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
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML to PDF in C# with pdforge?

Here's how **pdforge** handles this:

```csharp
// REST API — no .NET SDK on NuGet. Integration is HttpClient + JSON POST.
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "pdfnoodle_api_YOUR_KEY");

        var body = new { html = "<html><body><h1>Hello World</h1></body></html>" };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
        File.WriteAllBytes("output.pdf", pdfBytes);
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
        var html = "<html><body><h1>Hello World</h1></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML Files to PDF with Custom Settings?

Here's how **pdforge** handles this:

```csharp
// REST API — no .NET SDK on NuGet. Integration is HttpClient + JSON POST.
// Page size / orientation flow through the optional `pdfParams` object.
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "pdfnoodle_api_YOUR_KEY");

        var htmlContent = File.ReadAllText("input.html");
        var body = new { html = htmlContent, pdfParams = new { format = "A4", landscape = true } };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
        File.WriteAllBytes("output.pdf", pdfBytes);
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
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        var htmlContent = System.IO.File.ReadAllText("input.html");
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from pdforge to IronPDF?

### The Cloud API Dependency Problem

pdforge processes all documents on external cloud servers. This architecture creates significant concerns:

1. **External Server Processing**: Every PDF you generate requires sending your HTML/data to pdforge's servers
2. **Privacy & Compliance Risks**: Sensitive data travels over the internet to third-party servers
3. **Ongoing Subscription Costs**: Monthly fees accumulate indefinitely with no asset ownership
4. **Internet Dependency**: No PDF generation when network is unavailable
5. **Rate Limits**: API usage caps can throttle high-volume applications
6. **Network Latency**: Round-trip time adds seconds to every PDF generation

### Quick Migration Overview

| Aspect | pdforge | IronPDF |
|--------|---------|---------|
| Processing Location | External cloud servers | Local (your server) |
| Authentication | API key per request | One-time license key |
| Network Required | Every generation | Only initial setup |
| Pricing Model | Monthly subscription | Perpetual license available |
| Rate Limits | Yes (plan-dependent) | None |
| Data Privacy | Data sent externally | Data stays local |
| PDF Manipulation | Limited | Full suite (merge, split, edit) |

### Key API Mappings

| pdforge | IronPDF | Notes |
|---------|---------|-------|
| `Authorization: Bearer pdfnoodle_api_...` | `new ChromePdfRenderer()` | No per-request credentials |
| `POST /v1/html-to-pdf/sync` with `{ html }` | `renderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| Fetch URL, then POST its HTML | `renderer.RenderUrlAsPdf(url)` | URL to PDF (no dedicated endpoint) |
| `pdfParams: { format: "A4" }` | `renderer.RenderingOptions.PaperSize = PdfPaperSize.A4` | Paper size |
| `pdfParams: { landscape: true }` | `renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape` | Orientation |
| `headerTemplate: "<div>...</div>"` | `renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter { HtmlFragment = "..." }` | Header |
| `footerTemplate` with `<span class="pageNumber">` / `<span class="totalPages">` | `renderer.RenderingOptions.TextFooter = new TextHeaderFooter { CenterText = "Page {page} of {total-pages}" }` | Footer |
| Download `signedUrl` then `File.WriteAllBytes` | `pdf.SaveAs(path)` | Save to disk |
| _(not available)_ | `PdfDocument.Merge()` | NEW: Merge PDFs |
| _(not available)_ | `pdf.ExtractAllText()` | NEW: Text extraction |
| _(not available)_ | `pdf.ApplyWatermark()` | NEW: Watermarks |

### Migration Code Example

**Before (pdforge):**
```csharp
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using var http = new HttpClient();
http.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", "pdfnoodle_api_YOUR_KEY");

var body = new
{
    html = "<h1>Report</h1>",
    pdfParams = new
    {
        format = "A4",
        displayHeaderFooter = true,
        footerTemplate = "<div style='font-size:10px;width:100%;text-align:center;'>" +
                         "Page <span class=\"pageNumber\"></span> of <span class=\"totalPages\"></span></div>"
    }
};
var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
resp.EnsureSuccessStatusCode();

using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
var pdfBytes = await http.GetByteArrayAsync(doc.RootElement.GetProperty("signedUrl").GetString());
File.WriteAllBytes("report.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Page {page} of {total-pages}"  // Note: different placeholder
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1>");
pdf.SaveAs("report.pdf");
```

### Critical Migration Notes

1. **Placeholder Syntax**: pdforge uses Chromium's `<span class="totalPages">` template tags; IronPDF uses bracketed placeholders
   ```csharp
   // pdforge footerTemplate: "Page <span class=\"pageNumber\"></span> of <span class=\"totalPages\"></span>"
   // IronPDF: "Page {page} of {total-pages}"
   ```

2. **Async to Sync**: pdforge HTTP calls are async; IronPDF is sync by default
   ```csharp
   // pdforge: await http.PostAsync(".../v1/html-to-pdf/sync", json)
   // IronPDF: renderer.RenderHtmlAsPdf(html)
   // IronPDF async: await Task.Run(() => renderer.RenderHtmlAsPdf(html))
   ```

3. **Return Type**: pdforge returns a JSON envelope with `signedUrl`; IronPDF returns `PdfDocument`
   ```csharp
   // pdforge: parse signedUrl → http.GetByteArrayAsync → byte[]
   // IronPDF: var pdf = renderer.RenderHtmlAsPdf(html);
   //          byte[] bytes = pdf.BinaryData;  // if you need bytes
   ```

4. **API Key → License Key**: One-time setup at app startup
   ```csharp
   // pdforge: Bearer token in Authorization header on every request
   // IronPDF: IronPdf.License.LicenseKey = "KEY" - once at startup
   ```

5. **Configuration Location**: JSON `pdfParams` → RenderingOptions
   ```csharp
   // pdforge: pdfParams = new { format = "A4" }
   // IronPDF: renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
   ```

### NuGet Package Migration

```bash
# pdforge has no .NET SDK on NuGet — nothing to remove. Just install IronPDF:
dotnet add package IronPdf
```

### Find All pdforge References

```bash
# Find pdforge / pdf noodle endpoint usage
grep -r "api\.pdforge\.com\|api\.pdfnoodle\.com" --include="*.cs" .

# Find Chromium template patterns to migrate
grep -r "totalPages\|pageNumber\|footerTemplate\|headerTemplate" --include="*.cs" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping (30+ methods and properties)
- 10 detailed code conversion examples
- Async to sync pattern migration
- Header/footer placeholder conversion
- New features (PDF manipulation, text extraction, watermarks, security)
- Server deployment (Linux dependencies)
- Troubleshooting guide for common issues
- Pre/post migration checklists

**[Complete Migration Guide: pdforge → IronPDF](migrate-from-pdforge.md)**

