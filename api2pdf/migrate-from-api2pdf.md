# How Do I Migrate from Api2pdf to IronPDF in C#?

## Table of Contents
1. [Why Migrate from Api2pdf to IronPDF](#why-migrate-from-api2pdf-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start (5 Minutes)](#quick-start-5-minutes)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Migration Checklist](#migration-checklist)
10. [Additional Resources](#additional-resources)

---

## Why Migrate from Api2pdf to IronPDF

### Security and Compliance Risks with Api2pdf

Api2pdf operates as a cloud-based service where **your sensitive HTML and documents are sent to third-party servers** for processing. This creates significant concerns:

| Risk | Api2pdf | IronPDF |
|------|---------|---------|
| **Data Transmission** | All content sent to external servers | Processes locally on your infrastructure |
| **GDPR Compliance** | Data crosses jurisdictions | Data never leaves your environment |
| **HIPAA Compliance** | PHI transmitted externally | PHI stays within your systems |
| **SOC 2** | Third-party dependency | Full control over data handling |
| **PCI DSS** | Card data potentially exposed | No external transmission |

### Cost Comparison

Api2pdf charges **per conversion** indefinitely, while IronPDF offers a **one-time perpetual license**:

| Volume | Api2pdf (Annual) | IronPDF (One-Time) |
|--------|-----------------|-------------------|
| 10,000 PDFs/month | ~$600/year | $749 (Lite) |
| 50,000 PDFs/month | ~$3,000/year | $749 (Lite) |
| 100,000 PDFs/month | ~$6,000/year | $1,499 (Plus) |
| 500,000 PDFs/month | ~$30,000/year | $2,999 (Professional) |

*Api2pdf pricing: ~$0.005/conversion. IronPDF pays for itself in months.*

### Technical Advantages of IronPDF

1. **No Network Latency**: Generate PDFs in milliseconds vs. seconds with API calls
2. **No Dependency on External Services**: Works offline, in air-gapped environments
3. **Modern Chromium Engine**: Full CSS3, JavaScript, Flexbox, Grid support
4. **Complete Control**: Headers, footers, watermarks, encryption all configurable locally
5. **Synchronous & Async**: Choose the programming model that fits your app
6. **10M+ NuGet Downloads**: Battle-tested in production environments worldwide

---

## Before You Start

### Prerequisites

- **.NET Framework 4.6.2+** or **.NET Core 3.1+** or **.NET 5/6/7/8/9**
- **Visual Studio 2019+** or **VS Code** with C# extension
- **NuGet Package Manager** access
- **Existing Api2pdf codebase** to migrate

### Find All Api2pdf References

Before migrating, identify all Api2pdf usage in your codebase:

```bash
# Find all Api2pdf using statements
grep -r "using Api2Pdf" --include="*.cs" .

# Find Api2PdfClient instantiations
grep -r "Api2PdfClient\|new Api2Pdf" --include="*.cs" .

# Find all async API calls
grep -r "FromHtmlAsync\|FromUrlAsync\|MergePdfs\|LibreOffice" --include="*.cs" .
```

### Breaking Changes to Expect

| Api2pdf Pattern | Change Required |
|-----------------|-----------------|
| API key authentication | Remove entirely—IronPDF runs locally |
| Async HTTP calls | Synchronous by default (async optional) |
| URL-based PDF retrieval | Direct in-memory PDF objects |
| Per-call pricing/metering | No metering needed |
| Multiple rendering engines | Single Chromium engine (better quality) |
| Cloud scaling | Your infrastructure scales instead |

---

## Quick Start (5 Minutes)

### Step 1: Replace NuGet Packages

```bash
# Remove Api2pdf packages
dotnet remove package Api2Pdf

# Install IronPDF
dotnet add package IronPdf
```

Or via Package Manager Console:
```powershell
Uninstall-Package Api2Pdf
Install-Package IronPdf
```

### Step 2: Update Namespaces

```csharp
// ❌ Remove these
using Api2Pdf;
using Api2Pdf.DotNet;

// ✅ Add these
using IronPdf;
using IronPdf.Rendering;
```

### Step 3: Convert Your First PDF

**Before (Api2pdf):**
```csharp
var a2pClient = new Api2PdfClient("YOUR_API_KEY");
var response = await a2pClient.HeadlessChrome.FromHtmlAsync("<h1>Hello</h1>");
var pdfUrl = response.Pdf;
// Then download PDF from URL...
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("output.pdf");
// PDF is ready immediately!
```

### Step 4: Set License Key (Optional for Development)

```csharp
// Set once at application startup
IronPdf.License.LicenseKey = "YOUR-IRONPDF-LICENSE-KEY";

// Or in appsettings.json:
// { "IronPdf.LicenseKey": "YOUR-IRONPDF-LICENSE-KEY" }
```

---

## Complete API Reference

### Namespace Mapping

| Api2pdf Namespace | IronPDF Namespace | Purpose |
|-------------------|-------------------|---------|
| `Api2Pdf` | `IronPdf` | Core PDF functionality |
| `Api2Pdf.DotNet` | `IronPdf` | .NET-specific classes |
| N/A | `IronPdf.Rendering` | Rendering options |
| N/A | `IronPdf.Editing` | PDF editing features |
| N/A | `IronPdf.Forms` | PDF form handling |
| N/A | `IronPdf.Signing` | Digital signatures |

### Core Class Mapping

| Api2pdf Class | IronPDF Equivalent | Notes |
|---------------|-------------------|-------|
| `Api2PdfClient` | `ChromePdfRenderer` | Main rendering class |
| `Api2PdfResult` | `PdfDocument` | Represents the PDF |
| `HeadlessChromeOptions` | `ChromePdfRenderOptions` | Rendering configuration |
| `WkhtmlHtmlToPdfRequest` | `ChromePdfRenderOptions` | IronPDF uses Chromium for all rendering |
| N/A | `HtmlToPdf` | Legacy renderer (use ChromePdfRenderer) |

### Complete Method Mapping

#### HTML to PDF Conversion

| Api2pdf Method | IronPDF Method | Notes |
|----------------|----------------|-------|
| `HeadlessChrome.FromHtmlAsync(html)` | `renderer.RenderHtmlAsPdf(html)` | Synchronous by default |
| `HeadlessChrome.FromHtmlAsync(html, options)` | `renderer.RenderHtmlAsPdf(html)` | Set options on renderer first |
| `WkHtml.FromHtmlAsync(html)` | `renderer.RenderHtmlAsPdf(html)` | Use Chromium instead of wkhtmltopdf |
| `WkHtml.FromHtmlAsync(html, options)` | `renderer.RenderHtmlAsPdf(html)` | Configure via RenderingOptions |

#### URL to PDF Conversion

| Api2pdf Method | IronPDF Method | Notes |
|----------------|----------------|-------|
| `HeadlessChrome.FromUrlAsync(url)` | `renderer.RenderUrlAsPdf(url)` | Full page capture |
| `HeadlessChrome.FromUrlAsync(url, options)` | `renderer.RenderUrlAsPdf(url)` | Configure via RenderingOptions |
| `WkHtml.FromUrlAsync(url)` | `renderer.RenderUrlAsPdf(url)` | Use Chromium for better results |

#### Document Conversion (LibreOffice)

| Api2pdf Method | IronPDF Method | Notes |
|----------------|----------------|-------|
| `LibreOffice.ConvertAsync(url)` | `PdfDocument.FromFile(path)` | IronPDF opens existing PDFs |
| N/A | `renderer.RenderHtmlAsPdf(html)` | Convert HTML tables to PDF |

*Note: For Office document conversion, consider using IronWord, IronXL, or keeping a subset of Api2pdf calls.*

#### PDF Merging

| Api2pdf Method | IronPDF Method | Notes |
|----------------|----------------|-------|
| `PdfSharp.MergePdfsAsync(urls)` | `PdfDocument.Merge(pdfs)` | Merge PdfDocument objects |
| `PdfSharp.MergePdfsAsync(request)` | `PdfDocument.Merge(pdf1, pdf2, ...)` | Accepts multiple PDFs |

#### PDF Output

| Api2pdf Property | IronPDF Method | Notes |
|------------------|----------------|-------|
| `response.Pdf` (URL) | `pdf.SaveAs(path)` | Save to file |
| `response.Pdf` (download) | `pdf.BinaryData` | Get as byte array |
| N/A | `pdf.Stream` | Get as MemoryStream |
| N/A | `pdf.SaveAsAsync(path)` | Async file save |

#### PDF Security

| Api2pdf Method | IronPDF Property | Notes |
|----------------|------------------|-------|
| `PdfSharp.SetPasswordAsync(url, password)` | `pdf.SecuritySettings.OwnerPassword` | Owner password |
| N/A | `pdf.SecuritySettings.UserPassword` | User password |
| N/A | `pdf.SecuritySettings.AllowUserPrinting` | Print permission |
| N/A | `pdf.SecuritySettings.AllowUserCopyPasteContent` | Copy permission |
| N/A | `pdf.SecuritySettings.AllowUserEdits` | Edit permission |

#### Image Conversion

| Api2pdf Method | IronPDF Method | Notes |
|----------------|----------------|-------|
| `HeadlessChrome.HtmlToImageAsync(html)` | `pdf.RasterizeToImageFiles(path)` | Render pages as images |
| `HeadlessChrome.UrlToImageAsync(url)` | `pdf.ToBitmap()` | Get System.Drawing.Bitmap |

#### Rendering Options

| Api2pdf Option | IronPDF Option | Notes |
|----------------|----------------|-------|
| `Landscape = true` | `RenderingOptions.PaperOrientation = Landscape` | Page orientation |
| `PageSize = "A4"` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Paper size |
| `PrintBackground = true` | `RenderingOptions.PrintHtmlBackgrounds = true` | Background colors |
| `MarginTop`, etc. | `RenderingOptions.MarginTop`, etc. | Margins in mm |
| `Delay = 5000` | `RenderingOptions.WaitFor.RenderDelay(5000)` | Wait before render |
| `Scale = 0.8` | `RenderingOptions.Zoom = 80` | Zoom percentage |
| N/A | `RenderingOptions.CssMediaType = Print` | CSS media type |
| N/A | `RenderingOptions.EnableJavaScript = true` | JS execution |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (Api2pdf):**
```csharp
using Api2Pdf.DotNet;

class Program
{
    static async Task Main()
    {
        var a2pClient = new Api2PdfClient("YOUR_API_KEY");
        var response = await a2pClient.HeadlessChrome.FromHtmlAsync(
            "<h1>Hello World</h1><p>This is my PDF</p>"
        );

        if (response.Success)
        {
            Console.WriteLine($"PDF available at: {response.Pdf}");
            // Download PDF from URL
            using var httpClient = new HttpClient();
            var pdfBytes = await httpClient.GetByteArrayAsync(response.Pdf);
            File.WriteAllBytes("output.pdf", pdfBytes);
        }
        else
        {
            Console.WriteLine($"Error: {response.Error}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(
            "<h1>Hello World</h1><p>This is my PDF</p>"
        );

        pdf.SaveAs("output.pdf");
        Console.WriteLine("PDF created successfully!");

        // Or get as bytes directly:
        byte[] pdfBytes = pdf.BinaryData;
    }
}
```

### Example 2: URL to PDF with Options

**Before (Api2pdf):**
```csharp
using Api2Pdf.DotNet;

var a2pClient = new Api2PdfClient("YOUR_API_KEY");
var options = new HeadlessChromeOptions
{
    Landscape = true,
    PrintBackground = true,
    MarginTop = 10,
    MarginBottom = 10,
    Delay = 3000 // Wait 3 seconds for JS
};

var response = await a2pClient.HeadlessChrome.FromUrlAsync(
    "https://example.com/report",
    options
);

// Download and save PDF...
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;

var renderer = new ChromePdfRenderer();

// Configure all options on the renderer
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.WaitFor.RenderDelay(3000);

var pdf = renderer.RenderUrlAsPdf("https://example.com/report");
pdf.SaveAs("report.pdf");
```

### Example 3: Merging Multiple PDFs

**Before (Api2pdf):**
```csharp
using Api2Pdf.DotNet;

var a2pClient = new Api2PdfClient("YOUR_API_KEY");

var pdfUrls = new List<string>
{
    "https://example.com/pdf1.pdf",
    "https://example.com/pdf2.pdf",
    "https://example.com/pdf3.pdf"
};

var request = new PdfMergeRequest { Urls = pdfUrls };
var response = await a2pClient.PdfSharp.MergePdfsAsync(request);

if (response.Success)
{
    // Download merged PDF from response.Pdf URL
}
```

**After (IronPDF):**
```csharp
using IronPdf;

// Load PDFs from files or URLs
var pdf1 = PdfDocument.FromFile("document1.pdf");
var pdf2 = PdfDocument.FromFile("document2.pdf");
var pdf3 = PdfDocument.FromFile("document3.pdf");

// Merge all PDFs
var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("merged.pdf");

// Or merge from a collection
var pdfs = new List<PdfDocument> { pdf1, pdf2, pdf3 };
var mergedFromList = PdfDocument.Merge(pdfs);
```

### Example 4: Password-Protected PDF

**Before (Api2pdf):**
```csharp
using Api2Pdf.DotNet;

var a2pClient = new Api2PdfClient("YOUR_API_KEY");

// Generate PDF first
var pdfResponse = await a2pClient.HeadlessChrome.FromHtmlAsync("<h1>Confidential</h1>");

// Then add password protection
var protectedResponse = await a2pClient.PdfSharp.SetPasswordAsync(
    pdfResponse.Pdf,
    "secretpassword"
);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential</h1>");

// Set security in one step
pdf.SecuritySettings.OwnerPassword = "owner123";
pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

pdf.SaveAs("protected.pdf");
```

### Example 5: HTML File to PDF with Custom Styling

**Before (Api2pdf):**
```csharp
using Api2Pdf.DotNet;

var a2pClient = new Api2PdfClient("YOUR_API_KEY");

string htmlContent = File.ReadAllText("template.html");

var options = new HeadlessChromeOptions
{
    PrintBackground = true,
    Scale = 0.9
};

var response = await a2pClient.HeadlessChrome.FromHtmlAsync(htmlContent, options);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Configure rendering
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.Zoom = 90;
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

// Render from file
var pdf = renderer.RenderHtmlFileAsPdf("template.html");
pdf.SaveAs("output.pdf");

// Or render HTML string
string htmlContent = File.ReadAllText("template.html");
var pdf2 = renderer.RenderHtmlAsPdf(htmlContent);
```

### Example 6: PDF to Image Conversion

**Before (Api2pdf):**
```csharp
using Api2Pdf.DotNet;

var a2pClient = new Api2PdfClient("YOUR_API_KEY");

var response = await a2pClient.HeadlessChrome.HtmlToImageAsync(
    "<h1>Preview Image</h1>",
    new HeadlessChromeImageOptions { Width = 800, Height = 600 }
);

// Download image from response URL
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Drawing;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Preview Image</h1>");

// Export all pages as images
pdf.RasterizeToImageFiles("page_*.png", DPI: 150);

// Or get as Bitmap objects
Bitmap[] images = pdf.ToBitmap(DPI: 150);
images[0].Save("preview.png");
```

### Example 7: Headers and Footers

**Before (Api2pdf):**
```csharp
using Api2Pdf.DotNet;

var a2pClient = new Api2PdfClient("YOUR_API_KEY");

var options = new HeadlessChromeOptions
{
    HeaderHtml = "<div style='font-size:10px'>Company Header</div>",
    FooterHtml = "<div style='font-size:10px'>Page <span class='pageNumber'></span></div>"
};

var response = await a2pClient.HeadlessChrome.FromHtmlAsync(html, options);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// HTML headers and footers with full styling
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='font-size:10px; text-align:center;'>Company Header</div>",
    DrawDividerLine = true
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='font-size:10px; text-align:center;'>Page {page} of {total-pages}</div>",
    DrawDividerLine = true
};

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("with-headers.pdf");
```

### Example 8: Async PDF Generation

**Before (Api2pdf):**
```csharp
using Api2Pdf.DotNet;

// Api2pdf is always async (HTTP calls)
var a2pClient = new Api2PdfClient("YOUR_API_KEY");
var response = await a2pClient.HeadlessChrome.FromHtmlAsync("<h1>Hello</h1>");
```

**After (IronPDF):**
```csharp
using IronPdf;

// Synchronous (default)
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");

// Async when needed
var pdfAsync = await renderer.RenderHtmlAsPdfAsync("<h1>Hello</h1>");

// Parallel generation
var tasks = new[]
{
    renderer.RenderHtmlAsPdfAsync("<h1>Doc 1</h1>"),
    renderer.RenderHtmlAsPdfAsync("<h1>Doc 2</h1>"),
    renderer.RenderHtmlAsPdfAsync("<h1>Doc 3</h1>")
};
var results = await Task.WhenAll(tasks);
```

### Example 9: PDF Manipulation (Extract, Split)

**Before (Api2pdf):**
```csharp
// Api2pdf doesn't support splitting or extracting pages
// You would need a separate library
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("large-document.pdf");

// Extract specific pages
var extracted = pdf.CopyPages(0, 4); // First 5 pages
extracted.SaveAs("first-5-pages.pdf");

// Extract a single page
var singlePage = pdf.CopyPage(2); // Page 3
singlePage.SaveAs("page-3.pdf");

// Remove pages
pdf.RemovePage(0); // Remove first page
pdf.SaveAs("without-first-page.pdf");

// Split into individual pages
for (int i = 0; i < pdf.PageCount; i++)
{
    var page = pdf.CopyPage(i);
    page.SaveAs($"page_{i + 1}.pdf");
}
```

### Example 10: Text Extraction

**Before (Api2pdf):**
```csharp
// Api2pdf doesn't support text extraction
// You would need a separate library
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Extract all text
string allText = pdf.ExtractAllText();
Console.WriteLine(allText);

// Extract text from specific page
string pageText = pdf.ExtractTextFromPage(0);

// Extract text from page range
string rangeText = pdf.ExtractTextFromPages(
    PageIndexes: new[] { 0, 1, 2 }
);
```

---

## Advanced Scenarios

### ASP.NET Core Integration

**Before (Api2pdf in ASP.NET):**
```csharp
[ApiController]
public class PdfController : ControllerBase
{
    private readonly Api2PdfClient _client;

    public PdfController()
    {
        _client = new Api2PdfClient("YOUR_API_KEY");
    }

    [HttpGet("generate")]
    public async Task<IActionResult> GeneratePdf()
    {
        var response = await _client.HeadlessChrome.FromHtmlAsync("<h1>Report</h1>");

        if (!response.Success)
            return BadRequest(response.Error);

        // Redirect to download URL
        return Redirect(response.Pdf);
    }
}
```

**After (IronPDF in ASP.NET Core):**
```csharp
[ApiController]
public class PdfController : ControllerBase
{
    [HttpGet("generate")]
    public IActionResult GeneratePdf()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1>");

        // Return PDF directly
        return File(pdf.BinaryData, "application/pdf", "report.pdf");
    }

    [HttpGet("generate-async")]
    public async Task<IActionResult> GeneratePdfAsync()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = await renderer.RenderHtmlAsPdfAsync("<h1>Report</h1>");

        return File(pdf.Stream, "application/pdf", "report.pdf");
    }
}
```

### Dependency Injection Setup

```csharp
// Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register ChromePdfRenderer as a service
    services.AddScoped<ChromePdfRenderer>();

    // Or create a service wrapper
    services.AddScoped<IPdfService, IronPdfService>();
}

// IPdfService.cs
public interface IPdfService
{
    PdfDocument GenerateFromHtml(string html);
    Task<PdfDocument> GenerateFromHtmlAsync(string html);
    PdfDocument GenerateFromUrl(string url);
}

// IronPdfService.cs
public class IronPdfService : IPdfService
{
    private readonly ChromePdfRenderer _renderer;

    public IronPdfService()
    {
        _renderer = new ChromePdfRenderer();
        ConfigureRenderer();
    }

    private void ConfigureRenderer()
    {
        _renderer.RenderingOptions.PrintHtmlBackgrounds = true;
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.MarginTop = 20;
        _renderer.RenderingOptions.MarginBottom = 20;
    }

    public PdfDocument GenerateFromHtml(string html) =>
        _renderer.RenderHtmlAsPdf(html);

    public Task<PdfDocument> GenerateFromHtmlAsync(string html) =>
        _renderer.RenderHtmlAsPdfAsync(html);

    public PdfDocument GenerateFromUrl(string url) =>
        _renderer.RenderUrlAsPdf(url);
}
```

### Blazor Server Integration

```csharp
// PdfService.cs for Blazor
public class BlazorPdfService
{
    public byte[] GenerateInvoice(InvoiceModel invoice)
    {
        var renderer = new ChromePdfRenderer();

        string html = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial; }}
                    .invoice-header {{ font-size: 24px; }}
                    .total {{ font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='invoice-header'>Invoice #{invoice.Number}</div>
                <p>Customer: {invoice.CustomerName}</p>
                <p class='total'>Total: {invoice.Total:C}</p>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}

// In Blazor component
@inject BlazorPdfService PdfService
@inject IJSRuntime JS

<button @onclick="DownloadInvoice">Download Invoice</button>

@code {
    private async Task DownloadInvoice()
    {
        var pdfBytes = PdfService.GenerateInvoice(currentInvoice);
        await JS.InvokeVoidAsync("downloadFile", "invoice.pdf", pdfBytes);
    }
}
```

### Error Handling Migration

**Before (Api2pdf):**
```csharp
var response = await a2pClient.HeadlessChrome.FromHtmlAsync(html);

if (!response.Success)
{
    // Handle error from response object
    Console.WriteLine($"Error: {response.Error}");
    return;
}

// Check for HTTP errors during download
try
{
    var pdfBytes = await httpClient.GetByteArrayAsync(response.Pdf);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Download failed: {ex.Message}");
}
```

**After (IronPDF):**
```csharp
try
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
catch (IronPdf.Exceptions.IronPdfLicenseException ex)
{
    // License issues
    Console.WriteLine($"License error: {ex.Message}");
}
catch (IronPdf.Exceptions.IronPdfRenderingException ex)
{
    // Rendering issues (bad HTML, JavaScript errors)
    Console.WriteLine($"Rendering error: {ex.Message}");
}
catch (IOException ex)
{
    // File system issues
    Console.WriteLine($"File error: {ex.Message}");
}
catch (Exception ex)
{
    // General errors
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Docker Deployment

**Before (Api2pdf):**
```dockerfile
# No special requirements - API calls work from any container
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

**After (IronPDF):**
```dockerfile
# IronPDF requires additional dependencies for Chromium
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install dependencies for Chromium
RUN apt-get update && apt-get install -y \
    libc6 \
    libgdiplus \
    libx11-6 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libxss1 \
    libxtst6 \
    libnss3 \
    libatk-bridge2.0-0 \
    libgtk-3-0 \
    libgbm1 \
    libasound2 \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

Or use the official IronPDF Docker image:
```dockerfile
FROM ironsoftwareofficial/ironpdfengine:latest
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

---

## Performance Considerations

### Api2pdf vs IronPDF Performance

| Metric | Api2pdf | IronPDF |
|--------|---------|---------|
| **Simple HTML** | 2-5 seconds (network) | 100-500ms (local) |
| **Complex page** | 5-10 seconds | 500ms-2s |
| **Batch of 100** | Minutes (rate limits) | Seconds (parallel) |
| **Offline** | Not available | Works fully |
| **Cold start** | None | ~2s (first render) |

### Optimizing IronPDF Performance

```csharp
// 1. Reuse renderer instance
private static readonly ChromePdfRenderer SharedRenderer = new ChromePdfRenderer();

public PdfDocument GeneratePdf(string html)
{
    return SharedRenderer.RenderHtmlAsPdf(html);
}

// 2. Parallel generation
public async Task<List<PdfDocument>> GenerateBatch(List<string> htmlDocs)
{
    var tasks = htmlDocs.Select(html =>
        Task.Run(() => SharedRenderer.RenderHtmlAsPdf(html)));

    var results = await Task.WhenAll(tasks);
    return results.ToList();
}

// 3. Disable unnecessary features
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = false; // If not needed
renderer.RenderingOptions.WaitFor.RenderDelay(0);   // No delay
renderer.RenderingOptions.Timeout = 30000;          // 30s max

// 4. Use appropriate paper size
renderer.RenderingOptions.FitToPaperMode = FitToPaperModes.Zoom;
```

### Memory Management

```csharp
// IronPDF documents are IDisposable
using (var pdf = renderer.RenderHtmlAsPdf(html))
{
    pdf.SaveAs("output.pdf");
} // Automatically disposed

// For batch processing
foreach (var html in htmlDocuments)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"doc_{counter++}.pdf");
}
```

---

## Troubleshooting Guide

### Issue 1: "License key is invalid or missing"

**Symptom:** Watermark on PDFs or license exception

**Solution:**
```csharp
// Set license at application startup
IronPdf.License.LicenseKey = "YOUR-KEY";

// Verify license
bool isLicensed = IronPdf.License.IsLicensed;
Console.WriteLine($"Licensed: {isLicensed}");
```

### Issue 2: "Failed to launch browser" in Docker

**Symptom:** Chromium cannot start in container

**Solution:**
```csharp
// Add to startup
Installation.ChromeGpuMode = IronPdf.Engines.Chrome.ChromeGpuModes.Disabled;
Installation.LinuxAndDockerDependenciesAutoConfig = true;
```

### Issue 3: PDF looks different from Api2pdf output

**Symptom:** Layout or styling differences

**Solution:**
```csharp
var renderer = new ChromePdfRenderer();

// Match Api2pdf Headless Chrome settings
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.ViewPortWidth = 1280;
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.RenderDelay(1000);
```

### Issue 4: Missing API key configuration

**Symptom:** Looking for API key setup

**Solution:** IronPDF doesn't need API keys—it runs locally!
```csharp
// ❌ Remove this (Api2pdf pattern)
var client = new Api2PdfClient("API_KEY");

// ✅ Just create renderer
var renderer = new ChromePdfRenderer();
```

### Issue 5: Async code not compiling

**Symptom:** `await` not available

**Solution:**
```csharp
// Synchronous (works everywhere)
var pdf = renderer.RenderHtmlAsPdf(html);

// Async (requires async context)
var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// In non-async method
var pdf = renderer.RenderHtmlAsPdfAsync(html).GetAwaiter().GetResult();
```

### Issue 6: Large PDF file sizes

**Symptom:** PDFs larger than Api2pdf output

**Solution:**
```csharp
// Compress images
renderer.RenderingOptions.ImageQuality = 80;

// Or compress after generation
pdf.CompressImages(quality: 75);
pdf.SaveAs("compressed.pdf");
```

### Issue 7: External resources not loading

**Symptom:** Images or CSS missing from PDF

**Solution:**
```csharp
var renderer = new ChromePdfRenderer();

// Wait for resources to load
renderer.RenderingOptions.WaitFor.AllFontsLoaded(timeout: 10000);
renderer.RenderingOptions.WaitFor.NetworkIdle(timeout: 10000);

// Or use base URL for relative paths
var pdf = renderer.RenderHtmlAsPdf(html, baseUrl: "https://example.com/");
```

### Issue 8: Font rendering issues

**Symptom:** Wrong fonts in PDF

**Solution:**
```csharp
// Embed fonts in HTML
var html = @"
<style>
    @import url('https://fonts.googleapis.com/css2?family=Roboto');
    body { font-family: 'Roboto', sans-serif; }
</style>
<body>Content</body>";

// Or use system fonts
renderer.RenderingOptions.CustomCssUrl = "path/to/custom.css";
```

---

## Migration Checklist

### Pre-Migration Checklist

- [ ] **Document current Api2pdf usage patterns**
  ```csharp
  // Before (Api2pdf)
  var api2PdfClient = new Api2PdfClient("your-api-key");
  var response = api2PdfClient.Chrome.ConvertHtml(html);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** Understanding current usage helps map functionality to IronPDF equivalents.

- [ ] **List all `Api2PdfClient` instantiations**
  ```csharp
  // Before (Api2pdf)
  var client = new Api2PdfClient("your-api-key");

  // After (IronPDF)
  // No direct client instantiation needed
  ```
  **Why:** IronPDF does not require a client object, simplifying initialization.

- [ ] **Identify all rendering engines used (Chrome, WkHtml, LibreOffice)**
  **Why:** IronPDF uses a modern Chromium engine, which may affect rendering results.

- [ ] **Note all configuration options in use**
  ```csharp
  // Before (Api2pdf)
  var options = new { MarginTop = 10, MarginBottom = 10 };

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 10;
  renderer.RenderingOptions.MarginBottom = 10;
  ```
  **Why:** Ensures all configurations are correctly mapped to IronPDF's options.

- [ ] **Catalog PDF manipulation needs (merge, split, encrypt)**
  ```csharp
  // Before (Api2pdf)
  // Custom API calls for merge, split, encrypt

  // After (IronPDF)
  var pdf1 = PdfDocument.FromFile("file1.pdf");
  var pdf2 = PdfDocument.FromFile("file2.pdf");
  var merged = PdfDocument.Merge(pdf1, pdf2);
  ```
  **Why:** IronPDF provides built-in methods for common PDF manipulations.

- [ ] **Review security requirements (password protection, permissions)**
  ```csharp
  // Before (Api2pdf)
  // External API for security settings

  // After (IronPDF)
  pdf.SecuritySettings.UserPassword = "password";
  pdf.SecuritySettings.OwnerPassword = "ownerpassword";
  ```
  **Why:** IronPDF allows setting security options directly within your application.

- [ ] **Check deployment environments (Windows, Linux, Docker)**
  **Why:** IronPDF supports multiple environments, ensuring compatibility with your deployment.

- [ ] **Calculate current Api2pdf costs for ROI comparison**
  **Why:** IronPDF's one-time license can be more cost-effective than ongoing Api2pdf fees.

- [ ] **Obtain IronPDF license key for production**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### During Migration Checklist

- [ ] **Remove `Api2Pdf` NuGet package**
  ```bash
  dotnet remove package Api2Pdf
  ```
  **Why:** Clean removal of the old library to avoid conflicts.

- [ ] **Install `IronPdf` NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project to start using its features.

- [ ] **Update namespace imports**
  ```csharp
  // Before (Api2pdf)
  using Api2Pdf;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure your code references the correct library.

- [ ] **Replace `Api2PdfClient` with `ChromePdfRenderer`**
  ```csharp
  // Before (Api2pdf)
  var client = new Api2PdfClient("your-api-key");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses `ChromePdfRenderer` for PDF generation.

- [ ] **Convert async patterns (if using sync)**
  ```csharp
  // Before (Api2pdf)
  var response = await api2PdfClient.Chrome.ConvertHtmlAsync(html);

  // After (IronPDF)
  var pdf = await renderer.RenderHtmlAsPdfAsync(html);
  ```
  **Why:** IronPDF supports both synchronous and asynchronous operations.

- [ ] **Update options configuration**
  ```csharp
  // Before (Api2pdf)
  var options = new { MarginTop = 10 };

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 10;
  ```
  **Why:** Ensure all configurations are correctly applied in IronPDF.

- [ ] **Remove API key references**
  **Why:** IronPDF does not require an API key, simplifying security management.

- [ ] **Add IronPDF license key setup**
  ```csharp
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Update error handling to use exceptions**
  ```csharp
  // Before (Api2pdf)
  if (!response.Success) { /* handle error */ }

  // After (IronPDF)
  try
  {
      var pdf = renderer.RenderHtmlAsPdf(html);
  }
  catch (Exception ex)
  {
      // handle error
  }
  ```
  **Why:** IronPDF uses exceptions for error handling, providing more detailed error information.

- [ ] **Test each converted feature**
  **Why:** Ensure all functionality works as expected after migration.

### Post-Migration Checklist

- [ ] **Verify PDF output quality matches expectations**
  **Why:** Ensure the new library meets your quality standards.

- [ ] **Test all edge cases (large documents, special characters)**
  **Why:** Confirm IronPDF handles all scenarios your application requires.

- [ ] **Validate performance metrics**
  **Why:** IronPDF should provide faster PDF generation due to local processing.

- [ ] **Update Docker configurations if applicable**
  **Why:** Ensure IronPDF runs smoothly in containerized environments.

- [ ] **Remove Api2pdf portal account/API key**
  **Why:** Decommission old resources to avoid unnecessary costs or security risks.

- [ ] **Update monitoring (no more API latency tracking)**
  **Why:** IronPDF runs locally, eliminating network latency concerns.

- [ ] **Document any IronPDF-specific configurations**
  **Why:** Maintain clear documentation for future maintenance and onboarding.

- [ ] **Train team on new API patterns**
  **Why:** Ensure your team is familiar with IronPDF's features and usage.

- [ ] **Update CI/CD pipelines if needed**
  **Why:** Ensure continuous integration and deployment processes accommodate the new library.
---

## Additional Resources

### Official Documentation
- **IronPDF Documentation**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Getting Started Guide**: https://ironpdf.com/docs/

### Tutorials
- **HTML to PDF Tutorial**: https://ironpdf.com/how-to/html-file-to-pdf/
- **URL to PDF Guide**: https://ironpdf.com/tutorials/url-to-pdf/
- **ASP.NET Integration**: https://ironpdf.com/tutorials/aspx-to-pdf/

### Code Examples
- **GitHub Examples**: https://github.com/nicholashew/IronPdf-Examples
- **Code Samples**: https://ironpdf.com/examples/

### Support
- **Technical Support**: https://ironpdf.com/support/
- **Community Forum**: https://forum.ironpdf.com/
- **Stack Overflow**: Search `[ironpdf]` tag

### Api2pdf References
- **Api2pdf GitHub**: https://github.com/Api2Pdf/api2pdf.dotnet
- **Api2pdf Documentation**: https://www.api2pdf.com/documentation/v2

---

*This migration guide covers the transition from Api2pdf to IronPDF. For additional assistance, contact IronPDF support or consult the official documentation.*
