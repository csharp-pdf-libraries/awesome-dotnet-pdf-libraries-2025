# Kaizen.io HTML-to-PDF + C# + PDF

Kaizen.io HTML-to-PDF and IronPDF both convert HTML into PDF in a .NET context, but they ship as fundamentally different products. Kaizen.io HTML-to-PDF is a self-hosted Docker container (`kaizenio.azurecr.io/html-to-pdf`) that exposes a single REST endpoint at `POST /html-to-pdf` on port 8080 — there is no official .NET SDK or NuGet package, so C# integration is hand-rolled `HttpClient` calls against the running container. IronPDF, by contrast, is delivered as the `IronPdf` NuGet package and runs in-process inside your .NET application. This article compares the two approaches.

Kaizen.io HTML-to-PDF v1.x exposes a deliberately small API. The documented JSON body accepts only an `html` field; URL-to-PDF input, custom stylesheets, headers/footers, and page-layout options are listed as roadmap items rather than shipping features, so anything beyond rendering a string of HTML must be expressed inside the HTML itself (typically via `@page` CSS and absolute-positioned divs). Licensing is a one-time fee with a watermarked free tier, configured via the `KAIZEN_PDF_LICENSE` environment variable on the container.

In contrast, [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/) provides a strongly-typed in-process renderer (`ChromePdfRenderer`) with first-class support for URL input, file input, headers and footers (text or HTML), `{page}` / `{total-pages}` placeholders, paper size and orientation, margins, JavaScript wait conditions, and direct `SaveAs` to disk — all without a sidecar container or HTTP hop.

## Kaizen.io HTML-to-PDF vs. IronPDF: A Comprehensive Comparison

Here's a detailed comparison of Kaizen.io HTML-to-PDF and IronPDF across various dimensions:

| Feature                         | Kaizen.io HTML-to-PDF                          | IronPDF                                   |
| ------------------------------- | ---------------------------------------------- | ----------------------------------------- |
| **Deployment Model**            | Self-hosted Docker container                   | In-process .NET library                   |
| **Distribution**                | Docker image `kaizenio.azurecr.io/html-to-pdf` | NuGet `IronPdf`                           |
| **C# Integration**              | Hand-rolled `HttpClient` POST (no SDK)         | Strongly-typed `ChromePdfRenderer` API    |
| **Endpoint Surface (v1.x)**     | `POST /html-to-pdf` with `{ "html": ... }`     | Methods for HTML, file, URL, and stream   |
| **URL → PDF**                   | Roadmap                                        | `RenderUrlAsPdf(url)`                     |
| **Headers / Footers**           | Not in v1.x API; fake via fixed-position CSS   | `TextHeader/Footer` and `HtmlHeader/Footer` |
| **Page Numbers**                | Unsupported                                    | `{page}` and `{total-pages}` placeholders |
| **Page Size / Orientation**     | Embed `@page` CSS in HTML                      | `RenderingOptions.PaperSize` etc.         |
| **Licensing**                   | One-time license; free tier watermarks output  | Commercial (annual or perpetual)          |

### Strengths and Weaknesses

#### Kaizen.io HTML-to-PDF

**Strengths:**

1. **Self-Hosted, No Outbound Calls:**
   - Runs as a Docker container on your own infrastructure; the rendered HTML never leaves your network.

2. **Inexpensive Licensing:**
   - One-time license with a watermarked free tier — useful for small workloads or evaluation.

3. **Operationally Simple:**
   - One image, one port, one JSON field — easy to slot in as a sidecar.

**Weaknesses:**

1. **No .NET SDK:**
   - Every C# call is hand-rolled `HttpClient` + JSON. No IntelliSense, no compile-time checks on the request shape.

2. **Tiny v1.x API Surface:**
   - The documented body accepts only `html`. URL-to-PDF, custom stylesheets, headers/footers, page size/margins, and JS-wait fields are roadmap items, not features.

3. **No Page-Number Support:**
   - The API has no `{page}` / `{total}` placeholders, so "Page X of Y" footers cannot be produced server-side.

4. **Container Operations Overhead:**
   - You operate the container — pulls, restarts, port management, license env var, monitoring.

#### IronPDF

**Strengths:**

1. **Local Processing:**
   - Provides complete control over resources and ensures data privacy by processing everything internally.

2. **Low Latency:**
   - Local execution reduces the time taken to generate PDFs, boosting performance for time-sensitive tasks with superior c# html to pdf processing.

3. **Extensive Integration Support:**
   - Deep integration possibilities with C# and .NET, augmented by a rich set of tutorials and guides.

Explore performance benchmarks and pricing in the [comprehensive guide](https://ironsoftware.com/suite/blog/comparison/compare-kaizen-io-vs-ironpdf/).

**Weaknesses:**

1. **Scalability:**
   - Limited by available hardware resources, requiring potentially complex infrastructure setup for large-scale deployment.

2. **Initial Setup:**
   - May require more initial setup effort compared to cloud-based solutions.

### C# Code Example Using IronPDF

Below is a simple C# code example demonstrating how to convert an HTML file to a PDF document using IronPDF:

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.TextHeader = new TextHeaderFooter
        {
            CenterText = "{page} of {total-pages}"
        };

        // Convert HTML file to PDF
        var pdf = renderer.RenderHtmlFileAsPdf("input.html");

        // Save to file
        pdf.SaveAs("output.pdf");

        System.Console.WriteLine("PDF generated successfully!");
    }
}
```

For further instructions and more examples, you can visit the official [IronPDF tutorial page](https://ironpdf.com/tutorials/) or check out the [HTML to PDF guide](https://ironpdf.com/how-to/html-file-to-pdf/).

### Conclusion

Choosing between Kaizen.io HTML-to-PDF and IronPDF ultimately depends on your specific application needs. If the priority is high scalability with managed infrastructure, Kaizen.io offers a compelling service with its cloud-based model. Conversely, for localized processing, heightened security, and full integration with .NET applications, IronPDF stands out as the preferred choice. Each library brings unique strengths and trade-offs, and developers should weigh these factors based on their project requirements.

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person team building developer tools that have racked up over 41 million NuGet downloads. With four decades of coding under his belt, he's seen it all—from punch cards to cloud native (okay, maybe not punch cards, but close). When he's not shipping software, you can find him based in Chiang Mai, Thailand, probably debugging something over a really good cup of coffee. Connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).

---

## How Do I Url To PDF Headers Footers?

Here's how **Kaizen.io HTML-to-PDF** handles this. There is no `ConvertUrl`, no header/footer fields, and no page-number placeholders in v1.x — you fetch the URL yourself, then wrap the HTML with `@page` CSS and fixed-position divs to fake header/footer:

```csharp
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var http = new HttpClient();
        var page = await http.GetStringAsync("https://example.com");
        var html = $@"<!doctype html><html><head><style>
            @page {{ margin: 20mm; }}
            .h {{ position: fixed; top: -15mm; left: 0; right: 0; text-align: center; }}
            .f {{ position: fixed; bottom: -15mm; left: 0; right: 0; text-align: center; }}
        </style></head><body>
            <div class='h'>Company Header</div>
            <div class='f'>Footer (page numbers unsupported in Kaizen v1.x)</div>
            {page}
        </body></html>";

        var payload = JsonSerializer.Serialize(new { html });
        var response = await http.PostAsync(
            "http://localhost:8080/html-to-pdf",
            new StringContent(payload, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        File.WriteAllBytes("webpage.pdf", await response.Content.ReadAsByteArrayAsync());
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.TextHeader.CenterText = "Company Header";
        renderer.RenderingOptions.TextFooter.CenterText = "Page {page} of {total-pages}";
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert an HTML File to PDF?

Here's how **Kaizen.io HTML-to-PDF** handles this. There is no file endpoint — read the file yourself, embed page-size/orientation in `@page` CSS, and POST it to the container:

```csharp
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var body = File.ReadAllText("input.html");
        var html = "<style>@page { size: A4 portrait; }</style>" + body;

        using var http = new HttpClient();
        var payload = JsonSerializer.Serialize(new { html });
        var response = await http.PostAsync(
            "http://localhost:8080/html-to-pdf",
            new StringContent(payload, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        File.WriteAllBytes("document.pdf", await response.Content.ReadAsByteArrayAsync());
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        var pdf = renderer.RenderHtmlFileAsPdf("input.html");
        pdf.SaveAs("document.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Basic HTML To PDF?

Here's how **Kaizen.io HTML-to-PDF** handles this. There is no .NET SDK — you POST JSON to the container's REST endpoint:

```csharp
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var http = new HttpClient();
        var payload = JsonSerializer.Serialize(new { html = "<html><body><h1>Hello World</h1></body></html>" });
        var response = await http.PostAsync(
            "http://localhost:8080/html-to-pdf",
            new StringContent(payload, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        File.WriteAllBytes("output.pdf", await response.Content.ReadAsByteArrayAsync());
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System.IO;

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

## How Can I Migrate from Kaizen.io HTML-to-PDF to IronPDF?

### The Container-API Challenges

Kaizen.io HTML-to-PDF is delivered as a Docker container with a single REST endpoint and no .NET SDK. That model has real limits:

1. **Container to Operate**: You run, monitor, and update `kaizenio.azurecr.io/html-to-pdf:latest` somewhere reachable from your app.
2. **No SDK**: Every C# call is hand-rolled `HttpClient` + JSON; no IntelliSense, no compile-time checks.
3. **Tiny v1.x API Surface**: The body accepts only `html` — no URL input, no headers/footers, no page-size/margin fields, no JS-wait field.
4. **No Page Numbers**: There is no `{page}` / `{total}` placeholder support.
5. **Watermark on Free Tier**: Without `KAIZEN_PDF_LICENSE`, output is watermarked.
6. **HTTP Round-Trip per PDF**: Even on `localhost` you pay JSON serialization and a socket hop on every call.

### Quick Migration Overview

| Aspect | Kaizen.io HTML-to-PDF | IronPDF |
|--------|-----------------------|---------|
| Distribution | Docker container | NuGet `IronPdf` |
| C# Integration | Hand-rolled `HttpClient` POST | `ChromePdfRenderer` API |
| URL Input | Not in v1.x | `RenderUrlAsPdf(url)` |
| Headers / Footers | Not in v1.x | `TextHeader/Footer`, `HtmlHeader/Footer` |
| Page Numbers | Unsupported | `{page}` / `{total-pages}` |
| Page Size / Margins | Embed `@page` CSS | `RenderingOptions.*` |
| Process Model | Out-of-process container | In-process |

### Key Concept Mappings

| Kaizen.io concept | IronPDF | Notes |
|-------------------|---------|-------|
| `HttpClient.PostAsync("…/html-to-pdf", { html })` | `renderer.RenderHtmlAsPdf(html)` | Direct method call |
| (No URL endpoint in v1.x) | `renderer.RenderUrlAsPdf(url)` | First-class |
| Inline `@page { size: A4 portrait }` | `RenderingOptions.PaperSize` / `PaperOrientation` | Enum |
| Inline `@page { margin: 20mm }` | `RenderingOptions.MarginTop` etc. | Millimeters |
| Fake `<div class='header'>` | `RenderingOptions.HtmlHeader` | Real header zone |
| Manual `html.Replace("{total}", n)` | `{total-pages}` placeholder | Resolved server-side |
| `KAIZEN_PDF_LICENSE` env var | `IronPdf.License.LicenseKey` | Set once at startup |
| HTTP `byte[]` body | `pdf.BinaryData` / `pdf.SaveAs(path)` | `PdfDocument` |

### Migration Code Example

**Before (Kaizen.io — fake header/footer baked into HTML, no page numbers):**
```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class KaizenService
{
    private readonly HttpClient _http = new HttpClient();

    public async Task<byte[]> GeneratePdfAsync(string body)
    {
        var html = $@"<!doctype html><html><head><style>
            @page {{ size: A4; margin: 20mm; }}
            .h {{ position: fixed; top: -15mm; left: 0; right: 0; }}
            .f {{ position: fixed; bottom: -15mm; left: 0; right: 0; }}
        </style></head><body>
            <div class='h'>Company Report</div>
            <div class='f'>(page numbers unsupported in Kaizen v1.x)</div>
            {body}
        </body></html>";

        var payload = JsonSerializer.Serialize(new { html });
        var response = await _http.PostAsync(
            "http://localhost:8080/html-to-pdf",
            new StringContent(payload, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
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
        _renderer.RenderingOptions.MarginTop = 20;
        _renderer.RenderingOptions.MarginBottom = 20;

        _renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = "<div>Company Report</div>",
            MaxHeight = 25
        };

        _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div>Page {page} of {total-pages}</div>",
            MaxHeight = 25
        };
    }

    public byte[] GeneratePdf(string html)
    {
        return _renderer.RenderHtmlAsPdf(html).BinaryData;
    }
}
```

### Critical Migration Notes

1. **License Lives in Code, Not the Container**: Replace `-e KAIZEN_PDF_LICENSE=…` on the Docker run with `IronPdf.License.LicenseKey = "…"` once at app startup.

2. **Placeholder Conventions**: If you previously substituted strings into HTML before POSTing, switch to IronPDF's render-time placeholders:
   - `{page}` (unchanged)
   - `{total}` → `{total-pages}`
   - `{title}` → `{html-title}`

3. **Return Type**: Kaizen returns raw bytes via HTTP; IronPDF returns `PdfDocument`:
   ```csharp
   var pdf = renderer.RenderHtmlAsPdf(html);
   byte[] bytes = pdf.BinaryData;  // Get bytes
   pdf.SaveAs("output.pdf");        // Or save directly
   ```

4. **Delete HTTP Plumbing**: Remove `HttpClient` lifetime management, JSON serialization, status-code checks, and "container unreachable" error handling.

5. **Options Move to a Real Object**: Configuration that lived in `@page` CSS or fixed-position div hacks moves to `RenderingOptions` properties.

### Container Teardown

There is no Kaizen NuGet package to remove — Kaizen ships only as a Docker image:

```bash
# Stop and remove the running Kaizen container
docker stop kaizen-pdf
docker rm kaizen-pdf

# Install IronPDF
dotnet add package IronPdf
```

### Find All Kaizen.io References

Because Kaizen has no SDK, search for the endpoint and license env var rather than a namespace:

```bash
grep -r "html-to-pdf\|KAIZEN_PDF_LICENSE\|kaizenio.azurecr.io\|localhost:8080" \
  --include="*.cs" --include="*.json" --include="*.yml" .
```

**Ready for the complete migration?** The full guide includes:
- Concept-to-`RenderingOptions` mapping for size, orientation, margins, headers, footers
- 10 detailed code conversion examples (basic, URL, headers/footers, async, custom size, JS wait, file save, license, error handling, full service)
- Container-to-in-process migration patterns (Docker teardown, CI/CD updates)
- HTTP plumbing and "container unreachable" error-handling removal
- Header/footer migration with `{page}` / `{total-pages}` placeholders
- Performance considerations
- Troubleshooting guide for 8+ common issues
- Pre/post migration checklists

**[Complete Migration Guide: Kaizen.io HTML-to-PDF → IronPDF](migrate-from-kaizenio-html-to-pdf.md)**

