# Gotenberg vs IronPDF: Docker PDF Generation vs In-Process C# Library

**Looking for a Gotenberg alternative for C# PDF generation?** Gotenberg is a Docker-based microservice that converts HTML to PDF via REST API calls. While flexible, this architecture introduces significant infrastructure overhead, network latency, and operational complexity. For C# developers with html to pdf c# requirements, there's a simpler approach: **[IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/)** provides the same Chromium-based rendering as an in-process NuGet package—no Docker containers, no network calls, no infrastructure to manage.

## The Gotenberg Architecture Problem

Gotenberg requires you to:

1. **Deploy and manage Docker containers** - Kubernetes, Docker Compose, or manual container management
2. **Handle network communication** - Every PDF request is an HTTP call with 10-100ms+ latency
3. **Manage cold starts** - Container startup adds 2-5 seconds to first requests
4. **Scale horizontally** - Need more PDF capacity? Deploy more containers
5. **Monitor service health** - Container crashes, network failures, timeouts
6. **Construct multipart/form-data** - Verbose API for every request

**For C# applications, this is unnecessary overhead.** IronPDF runs in-process, eliminating all of this complexity and providing excellent c# html to pdf performance.

For detailed benchmarks and feature comparisons, visit the [complete comparison guide](https://ironsoftware.com/suite/blog/comparison/compare-gotenberg-vs-ironpdf/).

---

## Gotenberg vs IronPDF: Complete Comparison

| Factor | Gotenberg | IronPDF |
|--------|-----------|---------|
| **Deployment** | Docker container + orchestration | Single NuGet package |
| **Architecture** | Microservice (REST API) | In-process library |
| **Latency per request** | 10-100ms+ (network round-trip) | < 1ms overhead |
| **Cold start** | 2-5 seconds (container init) | 1-2 seconds (first render only) |
| **Infrastructure** | Docker, Kubernetes, load balancers | None required |
| **Network dependency** | Required | None |
| **Failure modes** | Network, container, service failures | Standard .NET exceptions |
| **API style** | REST multipart/form-data | Native C# method calls |
| **Scaling** | Horizontal (more containers) | Vertical (in-process) |
| **Debugging** | Distributed tracing | Standard debugger |
| **Memory management** | Separate container (512MB-2GB) | Shared application memory |
| **Version control** | Container image tags | NuGet package versions |
| **Health checks** | HTTP endpoints | N/A (in-process) |
| **CI/CD complexity** | Container builds, registry pushes | Standard .NET build |
| **Cost** | Free (MIT license) | Commercial |
| **Support** | Community | Professional support |

---

## Why Gotenberg's Docker Architecture Hurts C# Applications

### 1. Network Latency on Every Request

```csharp
// ❌ Gotenberg - Every PDF request requires network round-trip
public async Task<byte[]> GeneratePdfGotenberg(string html)
{
    using var client = new HttpClient();
    using var content = new MultipartFormDataContent();
    content.Add(new StringContent(html), "files", "index.html");

    // Network call: 10-100ms+ added to EVERY request
    var response = await client.PostAsync("http://gotenberg:3000/forms/chromium/convert/html", content);
    return await response.Content.ReadAsByteArrayAsync();
}

// ✅ IronPDF - In-process, no network overhead
public byte[] GeneratePdfIronPdf(string html)
{
    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### 2. Cold Start Penalty

```csharp
// ❌ Gotenberg - First request after container start: 2-5+ seconds
// Every pod restart, every scale-up event, every deployment = cold starts

// ✅ IronPDF - First render: 1-2s, subsequent renders: <200ms
// Cold start only once per application lifetime
```

### 3. Infrastructure Complexity

**Gotenberg Docker Compose:**
```yaml
# ❌ Gotenberg - Requires container management
version: '3.8'
services:
  app:
    depends_on:
      - gotenberg
    environment:
      - GOTENBERG_URL=http://gotenberg:3000

  gotenberg:
    image: gotenberg/gotenberg:8
    ports:
      - "3000:3000"
    deploy:
      resources:
        limits:
          memory: 2G
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:3000/health"]
      interval: 30s
```

**IronPDF:**
```yaml
# ✅ IronPDF - No additional services needed
version: '3.8'
services:
  app:
    environment:
      - IRONPDF_LICENSE_KEY=${IRONPDF_LICENSE_KEY}
# That's it. No Gotenberg service. No health checks. No resource limits.
```

### 4. Verbose Multipart API

```csharp
// ❌ Gotenberg - Every request requires multipart/form-data construction
using var content = new MultipartFormDataContent();
content.Add(new StringContent(html), "files", "index.html");
content.Add(new StringContent("0.5"), "marginTop");
content.Add(new StringContent("0.5"), "marginBottom");
content.Add(new StringContent("0.5"), "marginLeft");
content.Add(new StringContent("0.5"), "marginRight");
content.Add(new StringContent("3s"), "waitDelay");
content.Add(new StringContent("true"), "printBackground");
content.Add(new StringContent("true"), "landscape");

// ✅ IronPDF - Clean, typed C# properties
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.MarginTop = 12.7;     // mm
renderer.RenderingOptions.MarginBottom = 12.7;
renderer.RenderingOptions.RenderDelay = 3000;   // ms
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
```

### 5. Error Handling Complexity

```csharp
// ❌ Gotenberg - Must handle network failures, HTTP errors, timeouts
try
{
    var response = await client.PostAsync(gotenbergUrl, content);
    response.EnsureSuccessStatusCode(); // What if 500? 503? Timeout?
}
catch (HttpRequestException ex) { /* Network error */ }
catch (TaskCanceledException ex) { /* Timeout */ }
catch (Exception ex) { /* Container down? */ }

// ✅ IronPDF - Standard .NET exception handling
try
{
    var pdf = renderer.RenderHtmlAsPdf(html);
}
catch (IronPdf.Exceptions.IronPdfException ex)
{
    // Clear, specific exception
}
```

---

## C# Code Examples: Gotenberg vs IronPDF

### HTML to PDF Conversion

**Gotenberg (complex):**
```csharp
using System.Net.Http;
using System.Threading.Tasks;

class GotenbergPdf
{
    private readonly HttpClient _client;
    private readonly string _gotenbergUrl = "http://gotenberg:3000";

    public async Task<byte[]> ConvertHtmlToPdf(string html)
    {
        using var content = new MultipartFormDataContent();

        // Must construct multipart form data
        content.Add(new StringContent(html), "files", "index.html");

        // String-based configuration (error-prone)
        content.Add(new StringContent("8.5"), "paperWidth");
        content.Add(new StringContent("11"), "paperHeight");

        var response = await _client.PostAsync(
            $"{_gotenbergUrl}/forms/chromium/convert/html", content);

        // Must check HTTP status manually
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Gotenberg failed: {error}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }
}
```

**IronPDF (simple):**
```csharp
using IronPdf;

class IronPdfExample
{
    public byte[] ConvertHtmlToPdf(string html)
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;

        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### URL to PDF with Custom Options

**Gotenberg:**
```csharp
public async Task<byte[]> UrlToPdfGotenberg(string url)
{
    using var content = new MultipartFormDataContent();

    content.Add(new StringContent(url), "url");
    content.Add(new StringContent("true"), "landscape");
    content.Add(new StringContent("1"), "marginTop");
    content.Add(new StringContent("1"), "marginBottom");
    content.Add(new StringContent("3s"), "waitDelay");
    content.Add(new StringContent("true"), "printBackground");

    var response = await _client.PostAsync(
        $"{_gotenbergUrl}/forms/chromium/convert/url", content);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsByteArrayAsync();
}
```

**IronPDF:**
```csharp
public byte[] UrlToPdfIronPdf(string url)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
    renderer.RenderingOptions.MarginTop = 25.4;      // 1 inch in mm
    renderer.RenderingOptions.MarginBottom = 25.4;
    renderer.RenderingOptions.RenderDelay = 3000;    // 3s in ms
    renderer.RenderingOptions.PrintHtmlBackgrounds = true;

    return renderer.RenderUrlAsPdf(url).BinaryData;
}
```

### PDF with Headers and Footers

**Gotenberg:**
```csharp
public async Task<byte[]> PdfWithHeaderFooterGotenberg(string html)
{
    using var content = new MultipartFormDataContent();

    content.Add(new StringContent(html), "files", "index.html");

    // Separate files for header and footer
    var headerHtml = "<div style='font-size:10px;'>Company Name</div>";
    content.Add(new StringContent(headerHtml), "files", "header.html");

    // Page numbers use special CSS classes
    var footerHtml = @"<div style='font-size:9px; text-align:center;'>
        Page <span class='pageNumber'></span> of <span class='totalPages'></span>
    </div>";
    content.Add(new StringContent(footerHtml), "files", "footer.html");

    var response = await _client.PostAsync(
        $"{_gotenbergUrl}/forms/chromium/convert/html", content);

    return await response.Content.ReadAsByteArrayAsync();
}
```

**IronPDF:**
```csharp
public byte[] PdfWithHeaderFooterIronPdf(string html)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
    {
        HtmlFragment = "<div style='font-size:10px;'>Company Name</div>"
    };

    // Page numbers use placeholders - cleaner syntax
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
    {
        HtmlFragment = @"<div style='font-size:9px; text-align:center;'>
            Page {page} of {total-pages}
        </div>"
    };

    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Merge Multiple PDFs

**Gotenberg:**
```csharp
public async Task<byte[]> MergePdfsGotenberg(List<string> filePaths)
{
    using var content = new MultipartFormDataContent();

    int i = 1;
    foreach (var path in filePaths)
    {
        var bytes = await File.ReadAllBytesAsync(path);
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "files", $"{i++}.pdf");
    }

    var response = await _client.PostAsync(
        $"{_gotenbergUrl}/forms/pdfengines/merge", content);

    return await response.Content.ReadAsByteArrayAsync();
}
```

**IronPDF:**
```csharp
public byte[] MergePdfsIronPdf(List<string> filePaths)
{
    var pdfs = filePaths.Select(PdfDocument.FromFile).ToList();
    var merged = PdfDocument.Merge(pdfs);

    foreach (var pdf in pdfs) pdf.Dispose();

    return merged.BinaryData;
}
```

---

## Performance Comparison

### Latency per Operation

| Operation | Gotenberg (Warm Container) | Gotenberg (Cold Start) | IronPDF (First Render) | IronPDF (Subsequent) |
|-----------|---------------------------|------------------------|------------------------|---------------------|
| Simple HTML | 150-300ms | 2-5 seconds | 1-2 seconds | 50-150ms |
| Complex HTML | 500-1500ms | 3-7 seconds | 1.5-3 seconds | 200-800ms |
| URL Render | 1-5 seconds | 3-10 seconds | 1-5 seconds | 500ms-3s |
| PDF Merge | 200-500ms | 2-5 seconds | 100-300ms | 100-300ms |

### Infrastructure Cost

| Resource | Gotenberg | IronPDF |
|----------|-----------|---------|
| Containers required | 1-N (scaling) | 0 |
| Memory per container | 512MB-2GB | N/A |
| Network overhead | 10-100ms per request | 0ms |
| DevOps complexity | High (K8s/Docker) | None |
| Health check endpoints | Required | Not needed |
| Load balancer | Often needed | Not needed |

---

## When to Use Each

### Use Gotenberg If:
- You're building a polyglot microservices system (not just C#)
- You already have Kubernetes infrastructure and expertise
- You need to share PDF generation across multiple language services
- You're okay with network latency and infrastructure overhead

### Use IronPDF If:
- You're building a C# / .NET application
- You want simple deployment (just NuGet)
- You need lowest possible latency
- You don't want to manage Docker containers
- You prefer professional support over community forums
- You need advanced PDF features (forms, signatures, encryption)

---

## Migration from Gotenberg to IronPDF

### Step 1: Replace NuGet Packages

```bash
dotnet remove package Gotenberg.Sharp.API.Client  # If using
dotnet add package IronPdf
```

### Step 2: Update Code

Replace HTTP calls with IronPDF method calls. Key conversions:

| Gotenberg | IronPDF |
|-----------|---------|
| `POST /forms/chromium/convert/html` | `RenderHtmlAsPdf()` |
| `POST /forms/chromium/convert/url` | `RenderUrlAsPdf()` |
| `POST /forms/pdfengines/merge` | `PdfDocument.Merge()` |
| `marginTop = "0.5"` (inches) | `MarginTop = 12.7` (mm) |
| `waitDelay = "3s"` | `RenderDelay = 3000` (ms) |
| `<span class="pageNumber">` | `{page}` placeholder |

### Step 3: Remove Infrastructure

Delete Gotenberg from your Docker Compose / Kubernetes manifests:

```yaml
# REMOVE:
# gotenberg:
#   image: gotenberg/gotenberg:8
#   ports:
#     - "3000:3000"
```

For a complete step-by-step migration guide with 10+ code examples:

**[Complete Migration Guide: Gotenberg → IronPDF](migrate-from-gotenberg.md)**

---

## Advantages of IronPDF Over Gotenberg

### 1. Zero Infrastructure
No Docker, no Kubernetes, no containers, no health checks, no load balancers. Just `dotnet add package IronPdf`.

### 2. In-Process Performance
No network latency. No cold start penalties (after first render). Native C# performance.

### 3. Type-Safe API
Strongly-typed properties instead of string-based multipart form data. IntelliSense and compile-time checking.

### 4. Advanced PDF Features
Beyond HTML-to-PDF, IronPDF offers:
- Digital signatures
- PDF form filling
- PDF security and encryption
- PDF/A compliance
- Text and image extraction
- Watermarks and stamping
- Page manipulation

### 5. Professional Support
Commercial license includes professional support—not just community forums.

### 6. Simpler Error Handling
Standard .NET exceptions instead of HTTP status codes and network failure modes.

---

## Conclusion

Gotenberg is a capable PDF generation service for microservices architectures, but for C# applications, **its Docker-based approach introduces unnecessary complexity**:

- Extra containers to deploy and manage
- Network latency on every request
- Cold start penalties
- Complex multipart/form-data API
- Infrastructure health monitoring

**IronPDF provides the same Chromium-based rendering without the overhead.** It's a single NuGet package that runs in-process, with native C# types, professional support, and advanced PDF features.

For C# developers, the choice is clear: **skip the container complexity and use IronPDF**.

---

## Related Resources

- **[Complete Migration Guide: Gotenberg → IronPDF](migrate-from-gotenberg.md)** — Step-by-step code conversion
- **[IronPDF Documentation](https://ironpdf.com/docs/)** — Getting started guide
- **[IronPDF Tutorials](https://ironpdf.com/tutorials/)** — Code examples and use cases
- **[HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)** — HTML conversion deep-dive

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person engineering team building developer tools with over 41 million NuGet downloads. With four decades of programming experience, he specializes in building PDF generation and document processing solutions for the .NET ecosystem. Based in Chiang Mai, Thailand, Jacob continues to push the boundaries of what's possible with in-process PDF generation. Connect with him on [GitHub](https://github.com/jacob-mellor) and [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
