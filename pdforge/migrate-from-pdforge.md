# How Do I Migrate from pdforge to IronPDF in C#?

## Why Migrate from pdforge?

pdforge is a cloud-based PDF generation API that processes your documents on external servers. While this simplifies initial setup, it introduces significant limitations for production applications:

### The Cloud API Dependency Problem

1. **External Server Processing**: Every PDF you generate requires sending your HTML/data to pdforge's servers—your documents leave your infrastructure
2. **Privacy & Compliance Risks**: Sensitive data (contracts, financial reports, personal information) travels over the internet to third-party servers
3. **Ongoing Subscription Costs**: Monthly fees accumulate indefinitely with no asset ownership
4. **Internet Dependency**: No PDF generation when network is unavailable
5. **Rate Limits**: API usage caps can throttle high-volume applications
6. **Network Latency**: Round-trip time adds seconds to every PDF generation
7. **Vendor Lock-in**: Your application depends on pdforge's service availability and API stability

### Why [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/)?

IronPDF processes everything locally within your application:

- **Complete Data Privacy**: Documents never leave your server
- **One-Time Licensing**: Perpetual license eliminates recurring costs
- **Offline Capability**: Works without internet after initial setup
- **No Rate Limits**: Generate unlimited PDFs
- **Lower Latency**: No network round-trips
- **Full Control**: You own the processing environment
- **Modern Rendering**: Chromium-based engine with full CSS3/JS support
- **Rich Feature Set**: PDF manipulation, merging, text extraction, security

---

## Quick Migration Overview

| Aspect | pdforge | IronPDF |
|--------|---------|---------|
| Processing Location | External cloud servers | Local (your server) |
| Authentication | API key per request | One-time license key |
| Network Required | Every generation | Only initial setup |
| Pricing Model | Monthly subscription | Perpetual license available |
| Rate Limits | Yes (plan-dependent) | None |
| Data Privacy | Data sent externally | Data stays local with html to pdf c# |
| Offline Support | No | Yes |
| PDF Manipulation | Limited | Full suite (merge, split, edit) |
| Text Extraction | No | Yes |
| Async Pattern | Required | Optional (sync/async) |

For in-depth technical comparisons, visit the [analysis article](https://ironsoftware.com/suite/blog/comparison/compare-pdforge-vs-ironpdf/).

---

## NuGet Package Changes

```bash
# Remove pdforge packages
dotnet remove package pdforge
dotnet remove package PdfForge

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// Before: pdforge
using PdForge;
using PdForge.Client;
using PdForge.Models;

// After: IronPDF
using IronPdf;
using IronPdf.Rendering;
```

---

## API Mapping Reference

### Core Classes

| pdforge | IronPDF | Notes |
|---------|---------|-------|
| `PdfClient` | `ChromePdfRenderer` | Main PDF generator |
| `HtmlToPdfRequest` | `ChromePdfRenderOptions` | HTML conversion config |
| `UrlToPdfRequest` | `ChromePdfRenderOptions` | URL conversion config |
| `HtmlToPdfConverter` | `ChromePdfRenderer` | Alternative class name |
| API response (byte[]) | `PdfDocument` | Result object |

### Methods

| pdforge | IronPDF | Notes |
|---------|---------|-------|
| `client.GenerateAsync(request)` | `renderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| `client.GenerateAsync(urlRequest)` | `renderer.RenderUrlAsPdf(url)` | URL to PDF |
| `converter.ConvertHtmlString(html)` | `renderer.RenderHtmlAsPdf(html)` | HTML string |
| `converter.ConvertUrl(url)` | `renderer.RenderUrlAsPdf(url)` | URL conversion |
| `converter.ConvertFile(path)` | `renderer.RenderHtmlFileAsPdf(path)` | HTML file |
| `File.WriteAllBytes(path, bytes)` | `pdf.SaveAs(path)` | Save to disk |
| Return type: `byte[]` | `pdf.BinaryData` | Get raw bytes |
| Return type: `Task<byte[]>` | `pdf.Stream` | Get as stream |

### Configuration Options

| pdforge | IronPDF (RenderingOptions) | Notes |
|---------|---------------------------|-------|
| `PageSize = PageSize.A4` | `.PaperSize = PdfPaperSize.A4` | Paper size |
| `PageSize = PageSize.Letter` | `.PaperSize = PdfPaperSize.Letter` | US Letter |
| `Orientation = Orientation.Landscape` | `.PaperOrientation = PdfPaperOrientation.Landscape` | Orientation |
| `Orientation = Orientation.Portrait` | `.PaperOrientation = PdfPaperOrientation.Portrait` | Portrait |
| `MarginTop = 20` | `.MarginTop = 20` | Top margin |
| `MarginBottom = 20` | `.MarginBottom = 20` | Bottom margin |
| `MarginLeft = 15` | `.MarginLeft = 15` | Left margin |
| `MarginRight = 15` | `.MarginRight = 15` | Right margin |
| `Header = "text"` | `.TextHeader = new TextHeaderFooter { CenterText = "text" }` | Header |
| `Footer = "text"` | `.TextFooter = new TextHeaderFooter { CenterText = "text" }` | Footer |
| `HeaderHtml = "<div>..."` | `.HtmlHeader = new HtmlHeaderFooter { HtmlFragment = "..." }` | HTML header |
| `FooterHtml = "<div>..."` | `.HtmlFooter = new HtmlHeaderFooter { HtmlFragment = "..." }` | HTML footer |
| `JavascriptDelay = 2000` | `.RenderDelay = 2000` | JS wait time (ms) |
| `PrintBackground = true` | `.PrintHtmlBackgrounds = true` | Background rendering |
| `Scale = 1.5` | `.Zoom = 150` | Zoom percentage |

### Authentication Comparison

| pdforge | IronPDF |
|---------|---------|
| `new PdfClient("your-api-key")` | `IronPdf.License.LicenseKey = "YOUR-KEY"` |
| Per-request authentication | One-time at startup |
| API key in constructor | Global property |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // API key required for every client instance
        var client = new PdfClient("your-api-key");

        var request = new HtmlToPdfRequest
        {
            Html = "<html><body><h1>Hello World</h1><p>PDF content</p></body></html>"
        };

        // Data sent to external servers
        byte[] pdfBytes = await client.GenerateAsync(request);
        File.WriteAllBytes("output.pdf", pdfBytes);
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
        // One-time license configuration
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // All processing happens locally
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Hello World</h1><p>PDF content</p></body></html>");
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 2: URL to PDF with Options

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new PdfClient("your-api-key");

        var request = new UrlToPdfRequest
        {
            Url = "https://example.com",
            PageSize = PageSize.A4,
            Orientation = Orientation.Landscape,
            MarginTop = 20,
            MarginBottom = 20,
            MarginLeft = 15,
            MarginRight = 15
        };

        byte[] pdfBytes = await client.GenerateAsync(request);
        File.WriteAllBytes("webpage.pdf", pdfBytes);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.MarginLeft = 15;
        renderer.RenderingOptions.MarginRight = 15;

        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

### Example 3: Headers and Footers with Page Numbers

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new PdfClient("your-api-key");

        var request = new HtmlToPdfRequest
        {
            Html = "<h1>Report</h1><p>Content here...</p>",
            Header = "Company Report",
            Footer = "Page {page} of {totalPages}"
        };

        byte[] pdfBytes = await client.GenerateAsync(request);
        File.WriteAllBytes("report.pdf", pdfBytes);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        renderer.RenderingOptions.TextHeader = new TextHeaderFooter
        {
            CenterText = "Company Report",
            DrawDividerLine = true,
            FontSize = 12
        };

        renderer.RenderingOptions.TextFooter = new TextHeaderFooter
        {
            CenterText = "Page {page} of {total-pages}",  // Note: {total-pages} with hyphen
            DrawDividerLine = true
        };

        var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1><p>Content here...</p>");
        pdf.SaveAs("report.pdf");
    }
}
```

### Example 4: HTML Headers and Footers

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new PdfClient("your-api-key");

        var request = new HtmlToPdfRequest
        {
            Html = "<h1>Report Content</h1>",
            HeaderHtml = "<div style='text-align:center;'><img src='logo.png'/> Company Name</div>",
            FooterHtml = "<div style='text-align:center;'>Confidential - Page {page}</div>",
            HeaderHeight = 50,
            FooterHeight = 30
        };

        byte[] pdfBytes = await client.GenerateAsync(request);
        File.WriteAllBytes("branded-report.pdf", pdfBytes);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;'><img src='logo.png'/> Company Name</div>",
            MaxHeight = 50
        };

        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;'>Confidential - Page {page}</div>",
            MaxHeight = 30
        };

        var pdf = renderer.RenderHtmlAsPdf("<h1>Report Content</h1>");
        pdf.SaveAs("branded-report.pdf");
    }
}
```

### Example 5: Async Web Application

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class ReportController : Controller
{
    private readonly PdfClient _pdfClient;

    public ReportController()
    {
        _pdfClient = new PdfClient("your-api-key");
    }

    [HttpGet]
    public async Task<IActionResult> GenerateReport()
    {
        var request = new HtmlToPdfRequest
        {
            Html = "<h1>Sales Report</h1><p>Q4 Results</p>"
        };

        try
        {
            // Async call to external API (network latency)
            byte[] pdfBytes = await _pdfClient.GenerateAsync(request);
            return File(pdfBytes, "application/pdf", "report.pdf");
        }
        catch (ApiException ex)
        {
            return StatusCode(500, $"PDF generation failed: {ex.Message}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class ReportController : Controller
{
    public ReportController()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    }

    [HttpGet]
    public async Task<IActionResult> GenerateReport()
    {
        try
        {
            // Local processing - wrap in Task.Run for async if needed
            var pdf = await Task.Run(() =>
            {
                var renderer = new ChromePdfRenderer();
                return renderer.RenderHtmlAsPdf("<h1>Sales Report</h1><p>Q4 Results</p>");
            });

            return File(pdf.BinaryData, "application/pdf", "report.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"PDF generation failed: {ex.Message}");
        }
    }
}
```

### Example 6: JavaScript Wait for Dynamic Content

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new PdfClient("your-api-key");

        var request = new HtmlToPdfRequest
        {
            Html = @"
                <div id='chart'>Loading...</div>
                <script src='https://cdn.example.com/chart.js'></script>
                <script>renderChart('chart');</script>
            ",
            JavascriptDelay = 3000,  // Wait 3 seconds for JS
            EnableJavascript = true
        };

        byte[] pdfBytes = await client.GenerateAsync(request);
        File.WriteAllBytes("chart.pdf", pdfBytes);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // JavaScript is enabled by default in IronPDF
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.RenderDelay = 3000;  // Wait 3 seconds for JS

        string html = @"
            <div id='chart'>Loading...</div>
            <script src='https://cdn.example.com/chart.js'></script>
            <script>renderChart('chart');</script>
        ";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("chart.pdf");
    }
}
```

### Example 7: Custom Page Size

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new PdfClient("your-api-key");

        var request = new HtmlToPdfRequest
        {
            Html = "<h1>Custom Size Document</h1>",
            PageWidth = 5,     // inches
            PageHeight = 7,    // inches
        };

        byte[] pdfBytes = await client.GenerateAsync(request);
        File.WriteAllBytes("custom.pdf", pdfBytes);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Set custom paper size in inches
        renderer.RenderingOptions.SetCustomPaperSizeInInches(5, 7);

        // Or in millimeters
        // renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(127, 178);

        var pdf = renderer.RenderHtmlAsPdf("<h1>Custom Size Document</h1>");
        pdf.SaveAs("custom.pdf");
    }
}
```

### Example 8: Password-Protected PDF

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new PdfClient("your-api-key");

        var request = new HtmlToPdfRequest
        {
            Html = "<h1>Confidential Document</h1>",
            Password = "secret123",
            OwnerPassword = "admin456"
        };

        byte[] pdfBytes = await client.GenerateAsync(request);
        File.WriteAllBytes("secure.pdf", pdfBytes);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential Document</h1>");

        // Apply security after rendering
        pdf.SecuritySettings.UserPassword = "secret123";
        pdf.SecuritySettings.OwnerPassword = "admin456";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;

        pdf.SaveAs("secure.pdf");
    }
}
```

### Example 9: Batch Processing Multiple Files

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new PdfClient("your-api-key");

        var documents = new List<string>
        {
            "<h1>Document 1</h1>",
            "<h1>Document 2</h1>",
            "<h1>Document 3</h1>"
        };

        // Each document requires an API call (rate limits may apply)
        for (int i = 0; i < documents.Count; i++)
        {
            try
            {
                var request = new HtmlToPdfRequest { Html = documents[i] };
                byte[] pdfBytes = await client.GenerateAsync(request);
                File.WriteAllBytes($"doc_{i + 1}.pdf", pdfBytes);
                Console.WriteLine($"Generated doc_{i + 1}.pdf");
            }
            catch (RateLimitException)
            {
                Console.WriteLine($"Rate limited. Waiting...");
                await Task.Delay(60000);  // Wait a minute
                i--;  // Retry
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var documents = new List<string>
        {
            "<h1>Document 1</h1>",
            "<h1>Document 2</h1>",
            "<h1>Document 3</h1>"
        };

        var renderer = new ChromePdfRenderer();
        var allPdfs = new List<PdfDocument>();

        // No rate limits - c# html to pdf process as fast as your hardware allows
        for (int i = 0; i < documents.Count; i++)
        {
            var pdf = renderer.RenderHtmlAsPdf(documents[i]);
            pdf.SaveAs($"doc_{i + 1}.pdf");
            allPdfs.Add(pdf);
            Console.WriteLine($"Generated doc_{i + 1}.pdf");
        }

        // Bonus: Merge all documents (not available in pdforge)
        var merged = PdfDocument.Merge(allPdfs);
        merged.SaveAs("all_documents.pdf");
        Console.WriteLine("Merged all documents into all_documents.pdf");
    }
}
```

### Example 10: Error Handling Comparison

**Before (pdforge):**
```csharp
using PdForge;
using PdForge.Client;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new PdfClient("your-api-key");

        var request = new HtmlToPdfRequest
        {
            Html = "<h1>Test</h1>"
        };

        try
        {
            byte[] pdfBytes = await client.GenerateAsync(request);
            File.WriteAllBytes("output.pdf", pdfBytes);
        }
        catch (ApiAuthenticationException ex)
        {
            Console.WriteLine($"Invalid API key: {ex.Message}");
        }
        catch (RateLimitException ex)
        {
            Console.WriteLine($"Rate limit exceeded: {ex.Message}");
        }
        catch (ApiTimeoutException ex)
        {
            Console.WriteLine($"Request timed out: {ex.Message}");
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"API error: {ex.Message}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        try
        {
            IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf("<h1>Test</h1>");
            pdf.SaveAs("output.pdf");
        }
        catch (IronPdf.Exceptions.IronPdfLicenseException ex)
        {
            Console.WriteLine($"License error: {ex.Message}");
            // Invalid or expired license
        }
        catch (IronPdf.Exceptions.IronPdfRenderingException ex)
        {
            Console.WriteLine($"Rendering error: {ex.Message}");
            // HTML parsing or rendering issue
        }
        catch (System.IO.IOException ex)
        {
            Console.WriteLine($"File I/O error: {ex.Message}");
            // File system access issue
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
```

---

## Placeholder Migration

pdforge and IronPDF use slightly different placeholder syntax in headers and footers:

| pdforge Placeholder | IronPDF Placeholder | Description |
|--------------------|---------------------|-------------|
| `{page}` | `{page}` | Current page (same) |
| `{totalPages}` | `{total-pages}` | Total pages (hyphen) |
| `{date}` | `{date}` | Current date (same) |
| `{time}` | `{time}` | Current time (same) |
| `{title}` | `{html-title}` | HTML title |
| `{url}` | `{url}` | Source URL (same) |

**Migration Example:**
```csharp
// Before (pdforge)
Footer = "Page {page} of {totalPages}"

// After (IronPDF)
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Page {page} of {total-pages}"  // Note hyphen in total-pages
};
```

---

## Features Not Available in pdforge

IronPDF provides many capabilities that pdforge cannot offer:

### PDF Merging and Splitting

```csharp
// Merge multiple PDFs
var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("combined.pdf");

// Split a PDF
var firstChapter = pdf.CopyPages(0, 9);
firstChapter.SaveAs("chapter1.pdf");

// Add pages from another PDF
pdf.AppendPdf(anotherPdf);

// Remove pages
pdf.RemovePage(5);
```

### Text Extraction

```csharp
// Extract all text
string text = pdf.ExtractAllText();

// Extract text from specific page
string pageText = pdf.Pages[0].Text;

// Search for text
var results = pdf.ExtractAllText().Contains("keyword");
```

### Watermarks

```csharp
// Add text watermark
pdf.ApplyWatermark("<h2 style='color:red;opacity:0.5;'>CONFIDENTIAL</h2>",
    30,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);

// Add image watermark
pdf.ApplyWatermark("<img src='watermark.png' style='opacity:0.3;' />",
    50,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);
```

### Stamps and Annotations

```csharp
// Add HTML stamp
var stamp = new HtmlStamp
{
    Html = "<img src='approved.png' />",
    Width = 100,
    Height = 50,
    HorizontalAlignment = HorizontalAlignment.Right,
    VerticalAlignment = VerticalAlignment.Top
};
pdf.ApplyStamp(stamp, 0);  // Apply to first page
```

### Form Handling

```csharp
// Fill form fields
pdf.Form.GetFieldByName("CustomerName").Value = "John Doe";
pdf.Form.GetFieldByName("Date").Value = DateTime.Now.ToShortDateString();

// Get all form fields
foreach (var field in pdf.Form.Fields)
{
    Console.WriteLine($"{field.Name}: {field.Value}");
}

// Flatten forms (make non-editable)
pdf.Form.Flatten();
```

### Digital Signatures

```csharp
// Sign PDF with certificate
var signature = new PdfSignature("certificate.pfx", "password")
{
    SigningContact = "support@company.com",
    SigningReason = "Document Approval",
    SigningLocation = "New York"
};
pdf.Sign(signature);
```

### Page Manipulation

```csharp
// Rotate pages
pdf.RotateAllPages(PdfPageRotation.Rotate90);
pdf.RotatePage(0, PdfPageRotation.Rotate180);

// Get page dimensions
var page = pdf.Pages[0];
Console.WriteLine($"Size: {page.Width} x {page.Height}");
```

---

## Common Migration Issues

### 1. Async to Sync Conversion

```csharp
// pdforge: Always async
byte[] pdfBytes = await client.GenerateAsync(request);

// IronPDF: Sync by default
var pdf = renderer.RenderHtmlAsPdf(html);

// IronPDF: Async when needed
var pdf = await Task.Run(() => renderer.RenderHtmlAsPdf(html));
```

### 2. Return Type Changes

```csharp
// pdforge: Returns byte[]
byte[] pdfBytes = await client.GenerateAsync(request);
File.WriteAllBytes("output.pdf", pdfBytes);

// IronPDF: Returns PdfDocument
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");           // Direct save
byte[] bytes = pdf.BinaryData;      // Get bytes
Stream stream = pdf.Stream;         // Get stream
```

### 3. License Configuration

```csharp
// pdforge: API key per client
var client = new PdfClient("your-api-key");

// IronPDF: One-time at startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Set once in Program.cs or Startup.cs
```

### 4. Page Size Configuration

```csharp
// pdforge: Property on request
request.PageSize = PageSize.A4;

// IronPDF: Property on RenderingOptions
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
```

### 5. Network Error Handling Removed

```csharp
// pdforge: Handle network errors
catch (ApiTimeoutException) { /* retry */ }
catch (RateLimitException) { /* wait and retry */ }

// IronPDF: No network errors possible
// Just handle local exceptions
catch (IronPdfRenderingException) { /* fix HTML */ }
```

---

## Server Deployment

### pdforge
- No server configuration needed (cloud API)
- Requires outbound internet access on every call
- Subject to API rate limits

### IronPDF
- First run downloads Chromium (~150MB one-time)
- Works completely offline after initial setup
- No rate limits

#### Linux Dependencies

```bash
# Ubuntu/Debian
apt-get update && apt-get install -y \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2

# Docker (add to Dockerfile)
RUN apt-get update && apt-get install -y \
    libnss3 libatk1.0-0 libatk-bridge2.0-0 libdrm2 \
    libxkbcommon0 libxcomposite1 libxdamage1 libxfixes3 \
    libxrandr2 libgbm1 libasound2
```

---

## Pre-Migration Checklist

- [ ] Inventory all pdforge API calls in your codebase
- [ ] Document current configuration options used
- [ ] Identify header/footer placeholders to update
- [ ] Note async patterns that need adjustment
- [ ] Plan IronPDF license key storage (environment variables recommended)
- [ ] Test with IronPDF trial license first
- [ ] Check server environment for Linux dependencies if applicable

## Post-Migration Checklist

- [ ] Remove pdforge NuGet packages
- [ ] Update all namespace imports
- [ ] Replace API key with IronPDF license key (set once at startup)
- [ ] Convert request objects to RenderingOptions properties
- [ ] Update placeholder syntax (`{totalPages}` → `{total-pages}`)
- [ ] Convert async patterns if using sync methods
- [ ] Update error handling (remove API-specific exceptions)
- [ ] Test PDF output quality
- [ ] Verify offline operation works
- [ ] Remove API credentials from configuration
- [ ] Install Linux dependencies if deploying to Linux

---

## Find All pdforge References

```bash
# Find pdforge usage
grep -r "PdForge\|PdfClient\|HtmlToPdfRequest\|UrlToPdfRequest" --include="*.cs" .

# Find API key references
grep -r "api-key\|apikey\|PdfClient(" --include="*.cs" --include="*.json" .

# Find placeholder patterns to migrate
grep -r "{totalPages}" --include="*.cs" .

# Find async patterns
grep -r "GenerateAsync" --include="*.cs" .
```

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all pdforge usages in codebase**
  ```bash
  grep -r "using pdforge" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package pdforge
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace PDF generation calls**
  ```csharp
  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering.

- [ ] **Update page settings**
  ```csharp
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Configure headers/footers**
  ```csharp
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine provides modern rendering. Verify key documents.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  var merged = PdfDocument.Merge(pdf1, pdf2);
  pdf.SecuritySettings.UserPassword = "secret";
  pdf.ApplyWatermark("<h1>DRAFT</h1>");
  ```
  **Why:** IronPDF provides many additional features.

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **Getting Started Guide**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Code Examples**: https://ironpdf.com/examples/
- **Tutorials**: https://ironpdf.com/tutorials/
- **Support**: https://ironpdf.com/support/
