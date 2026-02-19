# How Do I Migrate from CraftMyPDF to IronPDF in C#?

## Table of Contents
1. [Why Migrate to IronPDF](#why-migrate-to-ironpdf)
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

## Why Migrate to IronPDF

### The Problem with Cloud-Based PDF APIs

CraftMyPDF, like many cloud PDF services, introduces fundamental issues that make it unsuitable for many production environments:

1. **Your Data Leaves Your System**: Every HTML template and JSON data payload is transmitted to CraftMyPDF's servers. For invoices, contracts, medical records, or any sensitive business data, this creates compliance risks (HIPAA, GDPR, SOC2).

2. **Network Latency and Reliability**: Each PDF generation requires a round-trip to the cloud. Average latency of 1.5-30 seconds per PDF (per CraftMyPDF's own documentation) vs. milliseconds for local generation.

3. **Ongoing Costs**: Pay-per-PDF pricing (1 credit per PDF) adds up. 10,000 PDFs/month = significant recurring costs vs. one-time license.

4. **Print-Optimized, Not Screen-Accurate**: Cloud PDF APIs typically optimize for print—reducing backgrounds, simplifying colors to save "ink." The output never looks like your HTML on screen.

5. **Template Lock-In**: CraftMyPDF requires their proprietary drag-and-drop editor. You can't use standard HTML/CSS freely.

### Key Advantages of IronPDF

| Aspect | CraftMyPDF | IronPDF |
|--------|------------|---------|
| **Data Location** | Cloud (your data leaves your system) | On-premise (data never leaves) |
| **Latency** | 1.5-30 seconds per PDF | Milliseconds |
| **Pricing** | Per-PDF subscription ($0.01-0.05/PDF) | One-time perpetual license |
| **Template System** | Proprietary drag-and-drop only | Any HTML/CSS/JavaScript |
| **Output Quality** | Print-optimized (ink reduction) | Pixel-perfect screen rendering |
| **API Dependency** | Internet required | Works offline |
| **Rendering Engine** | Cloud renderer | Local Chromium (Chrome quality) |
| **Compliance** | Data leaves organization | SOC2/HIPAA friendly |

### Feature Comparison

Additional context and code samples are provided in the [full walkthrough](https://ironpdf.com/blog/migration-guides/migrate-from-craftmypdf-to-ironpdf/).

| Feature | CraftMyPDF | IronPDF |
|---------|------------|---------|
| HTML to PDF | Via API templates | ✅ Native |
| URL to PDF | Via API | ✅ Native |
| Custom templates | Proprietary editor only | ✅ Any HTML |
| CSS3 support | Limited | ✅ Full |
| JavaScript rendering | Limited | ✅ Full |
| Merge/Split PDFs | Via API | ✅ Native |
| Form filling | Via API | ✅ Native |
| Digital signatures | Via API | ✅ Native |
| Watermarks | Via API | ✅ Native |
| Works offline | ❌ | ✅ |
| Self-hosted | ❌ | ✅ |

### True Cost Comparison

**CraftMyPDF Costs (Monthly):**
- Lite Plan: $19/month for 1,200 PDFs
- Professional: $49/month for 5,000 PDFs
- Enterprise: $99/month for 15,000 PDFs
- At scale: 100,000 PDFs = ~$500-600/month

**IronPDF Cost (One-Time):**
- Lite License: $749 (one developer, one project)
- Professional: $1,499 (unlimited projects)
- Unlimited PDFs forever after one-time payment

**Break-even: ~2-3 months depending on volume**

---

## Before You Start

### Prerequisites

- **.NET Version**: IronPDF supports .NET Framework 4.6.2+, .NET Core 3.1+, .NET 5/6/7/8/9
- **NuGet Access**: Ensure you can install packages from nuget.org
- **License Key**: Obtain from [IronPDF website](https://ironpdf.com/) (free trial available)

### Find All CraftMyPDF References

```bash
# Find all CraftMyPDF usages in your codebase
grep -r "CraftMyPdf\|craftmypdf\|api.craftmypdf.com" --include="*.cs" .
grep -r "X-API-KEY" --include="*.cs" .

# Find API key references
grep -r "your-api-key\|template-id\|template_id" --include="*.cs" .

# Find NuGet package references
grep -r "CraftMyPdf\|RestSharp" --include="*.csproj" .
```

### Breaking Changes Overview

| Change | CraftMyPDF | IronPDF | Impact |
|--------|------------|---------|--------|
| **Architecture** | Cloud REST API | Local .NET library | Remove HTTP calls |
| **Templates** | Proprietary editor | Standard HTML | Convert templates to HTML |
| **API Key** | Required for every call | License at startup | Remove API key handling |
| **Async pattern** | Required (HTTP) | Optional | Remove await if preferred |
| **Image handling** | Upload via API | Embed in HTML | Inline images |
| **Error handling** | HTTP status codes | Exceptions | Change try/catch patterns |
| **Data binding** | JSON templates | String interpolation | Simplify data binding |

---

## Quick Start (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove CraftMyPDF (if using SDK) and RestSharp
dotnet remove package RestSharp
# No CraftMyPDF package exists - it's REST-only

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Using Statements

```csharp
// Before
using RestSharp;
using System.IO;

// After
using IronPdf;
```

### Step 3: Apply License Key (Once at Startup)

```csharp
// Add at application startup (Program.cs or Global.asax)
// This replaces all X-API-KEY headers
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 4: Basic Code Migration

**Before (CraftMyPDF):**
```csharp
using RestSharp;
using System.IO;

var client = new RestClient("https://api.craftmypdf.com/v1/create");
var request = new RestRequest(Method.POST);
request.AddHeader("X-API-KEY", "your-api-key");
request.AddJsonBody(new
{
    template_id = "your-template-id",
    data = new { name = "John Doe", amount = "$1,000" }
});

var response = await client.ExecuteAsync(request);
if (response.IsSuccessful)
{
    File.WriteAllBytes("output.pdf", response.RawBytes);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

var html = @"
    <h1>Invoice</h1>
    <p>Name: John Doe</p>
    <p>Amount: $1,000</p>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// No API key, no HTTP calls, no await needed!
```

---

## Complete API Reference

### Namespace Mapping

| CraftMyPDF Pattern | IronPDF Namespace | Notes |
|--------------------|-------------------|-------|
| `RestSharp.RestClient` | `IronPdf` | No HTTP client needed |
| `api.craftmypdf.com/v1/*` | `IronPdf.*` | All operations local |
| `X-API-KEY` header | `License.LicenseKey` | Set once at startup |
| JSON data binding | C# string interpolation | Standard .NET patterns |

### API Endpoint Mapping

| CraftMyPDF Endpoint | IronPDF Method | Notes |
|--------------------|----------------|-------|
| `POST /v1/create` | `renderer.RenderHtmlAsPdf(html)` | Template → HTML |
| `POST /v1/create` (from URL) | `renderer.RenderUrlAsPdf(url)` | Direct URL rendering |
| `POST /v1/merge` | `PdfDocument.Merge(pdfs)` | Local merge |
| `POST /v1/add-watermark` | `pdf.ApplyWatermark(html)` | HTML-based watermark |
| `POST /v1/pdf-to-image` | `pdf.RasterizeToImageFiles()` | Local conversion |
| Template editor | Standard HTML/CSS | Use any editor |
| Webhook callbacks | Not needed | Sync by default |

### Configuration Mapping

| CraftMyPDF Option | IronPDF Equivalent | Notes |
|-------------------|-------------------|-------|
| `template_id` | HTML string | Use your own HTML |
| `data` JSON | C# interpolation | `$"Hello {name}"` |
| `page_size: "A4"` | `PaperSize = PdfPaperSize.A4` | |
| `orientation: "landscape"` | `PaperOrientation = Landscape` | |
| `margin_top: 20` | `MarginTop = 20` | In millimeters |
| `header` | `HtmlHeader` | Full HTML support |
| `footer` | `HtmlFooter` | Full HTML support |
| `output: "pdf"` | Default | Always PDF |
| `async: true` | Use `*Async()` methods | Optional |

### Template Feature Mapping

| CraftMyPDF Feature | IronPDF Equivalent |
|--------------------|-------------------|
| `{%name%}` template variables | `$"{name}"` C# interpolation |
| `{%loop items%}` | LINQ + `string.Join()` |
| `{%if condition%}` | C# ternary or if statements |
| Drag-and-drop layout | CSS Flexbox/Grid |
| Image component | `<img src="...">` |
| QR code component | Use QR library + `<img>` |
| Barcode component | Use barcode library + `<img>` |
| Chart component | Use charting library or SVG |

---

## Code Migration Examples

### Example 1: Simple HTML to PDF

**Before (CraftMyPDF):**
```csharp
using RestSharp;
using System.IO;

class Program
{
    static async Task Main()
    {
        var client = new RestClient("https://api.craftmypdf.com/v1/create");
        var request = new RestRequest(Method.POST);
        request.AddHeader("X-API-KEY", "your-api-key");
        request.AddJsonBody(new
        {
            template_id = "simple-template-id",
            data = new
            {
                title = "Hello World",
                body = "This is a PDF from CraftMyPDF"
            }
        });

        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            File.WriteAllBytes("output.pdf", response.RawBytes);
            Console.WriteLine("PDF created!");
        }
        else
        {
            Console.WriteLine($"Error: {response.ErrorMessage}");
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

        var html = @"
            <html>
            <body>
                <h1>Hello World</h1>
                <p>This is a PDF from IronPDF</p>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
        Console.WriteLine("PDF created!");
        // No error handling needed for HTTP issues!
    }
}
```

### Example 2: URL to PDF

**Before (CraftMyPDF):**
```csharp
using RestSharp;
using System.IO;

class Program
{
    static async Task Main()
    {
        var client = new RestClient("https://api.craftmypdf.com/v1/create");
        var request = new RestRequest(Method.POST);
        request.AddHeader("X-API-KEY", "your-api-key");
        request.AddJsonBody(new
        {
            template_id = "url-template-id",
            data = new
            {
                url = "https://example.com"
            },
            export_type = "pdf",
            page_size = "A4",
            orientation = "portrait"
        });

        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful && response.Content != null)
        {
            // CraftMyPDF returns JSON with PDF URL, need second request
            var pdfUrl = ExtractPdfUrl(response.Content);
            var pdfClient = new RestClient(pdfUrl);
            var pdfResponse = await pdfClient.ExecuteAsync(new RestRequest());
            File.WriteAllBytes("webpage.pdf", pdfResponse.RawBytes);
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
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
        // One call, no second request needed!
    }
}
```

### Example 3: Data-Bound Invoice Template

**Before (CraftMyPDF):**
```csharp
using RestSharp;
using System.IO;

class Program
{
    static async Task Main()
    {
        var items = new[]
        {
            new { name = "Product A", qty = 2, price = 50.00 },
            new { name = "Product B", qty = 1, price = 75.00 },
            new { name = "Product C", qty = 3, price = 25.00 }
        };

        var client = new RestClient("https://api.craftmypdf.com/v1/create");
        var request = new RestRequest(Method.POST);
        request.AddHeader("X-API-KEY", "your-api-key");
        request.AddJsonBody(new
        {
            template_id = "invoice-template-id", // Template designed in CraftMyPDF editor
            data = new
            {
                invoice_number = "INV-2024-001",
                customer_name = "John Doe",
                customer_email = "john@example.com",
                items = items,
                total = items.Sum(i => i.qty * i.price)
            }
        });

        var response = await client.ExecuteAsync(request);
        File.WriteAllBytes("invoice.pdf", response.RawBytes);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Linq;

class Program
{
    static void Main()
    {
        var items = new[]
        {
            new { name = "Product A", qty = 2, price = 50.00 },
            new { name = "Product B", qty = 1, price = 75.00 },
            new { name = "Product C", qty = 3, price = 25.00 }
        };

        double total = items.Sum(i => i.qty * i.price);

        string itemRows = string.Join("",
            items.Select(i => $@"
                <tr>
                    <td>{i.name}</td>
                    <td>{i.qty}</td>
                    <td>${i.price:F2}</td>
                    <td>${(i.qty * i.price):F2}</td>
                </tr>"));

        var html = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 40px; }}
                    h1 {{ color: #333; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
                    th {{ background-color: #4CAF50; color: white; }}
                    .total {{ font-size: 1.2em; font-weight: bold; text-align: right; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <h1>Invoice INV-2024-001</h1>
                <p><strong>Customer:</strong> John Doe</p>
                <p><strong>Email:</strong> john@example.com</p>

                <table>
                    <tr>
                        <th>Product</th>
                        <th>Quantity</th>
                        <th>Unit Price</th>
                        <th>Subtotal</th>
                    </tr>
                    {itemRows}
                </table>

                <p class='total'>Total: ${total:F2}</p>
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("invoice.pdf");
    }
}
```

### Example 4: Headers and Footers

**Before (CraftMyPDF):**
```csharp
using RestSharp;

var client = new RestClient("https://api.craftmypdf.com/v1/create");
var request = new RestRequest(Method.POST);
request.AddHeader("X-API-KEY", "your-api-key");
request.AddJsonBody(new
{
    template_id = "your-template-id",
    data = new
    {
        content = "Main document content",
        header = "<div>Company Logo | Report Title</div>",
        footer = "<div>Page {page} of {total_pages}</div>"
    },
    header_height = "50",
    footer_height = "30"
});
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = @"
        <div style='width:100%; text-align:center; font-size:12px;'>
            Company Logo | Report Title
        </div>",
    DrawDividerLine = true
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = @"
        <div style='width:100%; text-align:center; font-size:10px;'>
            Page {page} of {total-pages}
        </div>",
    DrawDividerLine = true
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Main Document Content</h1>");
pdf.SaveAs("document.pdf");
```

### Example 5: Merge PDFs

**Before (CraftMyPDF):**
```csharp
using RestSharp;
using System.IO;

var client = new RestClient("https://api.craftmypdf.com/v1/merge");
var request = new RestRequest(Method.POST);
request.AddHeader("X-API-KEY", "your-api-key");

// Must upload files to CraftMyPDF first, get URLs
request.AddJsonBody(new
{
    urls = new[]
    {
        "https://craftmypdf.com/uploads/file1.pdf",
        "https://craftmypdf.com/uploads/file2.pdf"
    }
});

var response = await client.ExecuteAsync(request);
// Download merged PDF from response URL...
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("file1.pdf");
var pdf2 = PdfDocument.FromFile("file2.pdf");
var pdf3 = PdfDocument.FromFile("file3.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("merged.pdf");
// No file upload, no download, instant local merge!
```

### Example 6: Add Watermark

**Before (CraftMyPDF):**
```csharp
using RestSharp;

var client = new RestClient("https://api.craftmypdf.com/v1/add-watermark");
var request = new RestRequest(Method.POST);
request.AddHeader("X-API-KEY", "your-api-key");
request.AddJsonBody(new
{
    url = "https://storage.craftmypdf.com/your-pdf.pdf",
    watermark_text = "CONFIDENTIAL",
    opacity = 0.3,
    rotation = 45
});
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

pdf.ApplyWatermark(@"
    <div style='
        font-size: 72px;
        color: rgba(255, 0, 0, 0.3);
        transform: rotate(-45deg);
        text-align: center;
        width: 100%;
        position: absolute;
        top: 40%;
    '>CONFIDENTIAL</div>");

pdf.SaveAs("watermarked.pdf");
```

### Example 7: Images in PDFs

**Before (CraftMyPDF):**
```csharp
using RestSharp;

// Must upload image to CraftMyPDF or use external URL
var client = new RestClient("https://api.craftmypdf.com/v1/create");
var request = new RestRequest(Method.POST);
request.AddHeader("X-API-KEY", "your-api-key");
request.AddJsonBody(new
{
    template_id = "template-with-image-placeholder",
    data = new
    {
        logo_url = "https://example.com/logo.png", // Must be publicly accessible
        content = "Document content"
    }
});
```

**After (IronPDF):**
```csharp
using IronPdf;

// Option 1: Base64 embedded (works with private images)
byte[] logoBytes = File.ReadAllBytes("logo.png");
string logoBase64 = Convert.ToBase64String(logoBytes);

var html = $@"
    <html>
    <body>
        <img src='data:image/png;base64,{logoBase64}' width='200' />
        <p>Document content</p>
    </body>
    </html>";

// Option 2: Local file path
var html2 = @"
    <html>
    <body>
        <img src='file:///C:/images/logo.png' width='200' />
        <p>Document content</p>
    </body>
    </html>";

// Option 3: URL (like CraftMyPDF)
var html3 = @"
    <html>
    <body>
        <img src='https://example.com/logo.png' width='200' />
        <p>Document content</p>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("with-image.pdf");
```

### Example 8: QR Codes and Barcodes

**Before (CraftMyPDF):**
```csharp
// CraftMyPDF has built-in QR code component in template editor
request.AddJsonBody(new
{
    template_id = "qr-code-template",
    data = new
    {
        qr_code_data = "https://example.com",
        barcode_data = "1234567890"
    }
});
```

**After (IronPDF):**
```csharp
using IronPdf;
using QRCoder; // NuGet: Install-Package QRCoder

// Generate QR code
var qrGenerator = new QRCodeGenerator();
var qrCodeData = qrGenerator.CreateQrCode("https://example.com", QRCodeGenerator.ECCLevel.Q);
var qrCode = new PngByteQRCode(qrCodeData);
byte[] qrBytes = qrCode.GetGraphic(10);
string qrBase64 = Convert.ToBase64String(qrBytes);

var html = $@"
    <html>
    <body>
        <h1>Document with QR Code</h1>
        <img src='data:image/png;base64,{qrBase64}' width='150' />
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("with-qr.pdf");
```

### Example 9: Async Operations (if needed)

**Before (CraftMyPDF):**
```csharp
// CraftMyPDF is async by nature (HTTP calls)
var response = await client.ExecuteAsync(request);

// With webhook for async processing
request.AddJsonBody(new
{
    template_id = "template-id",
    async = true,
    webhook_url = "https://yoursite.com/webhook"
});
```

**After (IronPDF):**
```csharp
using IronPdf;

// IronPDF is sync by default (faster, no network)
var pdf = renderer.RenderHtmlAsPdf(html);

// But async methods are available if you need them
var pdfAsync = await renderer.RenderHtmlAsPdfAsync(html);

// No webhooks needed - results are immediate!
```

### Example 10: Error Handling

**Before (CraftMyPDF):**
```csharp
using RestSharp;

try
{
    var response = await client.ExecuteAsync(request);

    if (!response.IsSuccessful)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            Console.WriteLine("Invalid API key");
        else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            Console.WriteLine("Rate limit exceeded");
        else if (response.StatusCode == HttpStatusCode.BadRequest)
            Console.WriteLine($"Template error: {response.Content}");
        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            Console.WriteLine("CraftMyPDF service down");
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (TimeoutException)
{
    Console.WriteLine("Request timed out");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

try
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
catch (Exception ex)
{
    Console.WriteLine($"PDF generation error: {ex.Message}");
    // No network errors, no rate limits, no service outages!
}
```

---

## Advanced Scenarios

### ASP.NET Core Integration

**Before (CraftMyPDF):**
```csharp
using Microsoft.AspNetCore.Mvc;
using RestSharp;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly string _apiKey;

    public ReportController(IConfiguration config)
    {
        _apiKey = config["CraftMyPdf:ApiKey"];
    }

    [HttpGet("generate")]
    public async Task<IActionResult> GenerateReport(int reportId)
    {
        var client = new RestClient("https://api.craftmypdf.com/v1/create");
        var request = new RestRequest(Method.POST);
        request.AddHeader("X-API-KEY", _apiKey);
        request.AddJsonBody(new
        {
            template_id = "report-template",
            data = await GetReportData(reportId)
        });

        var response = await client.ExecuteAsync(request);

        if (!response.IsSuccessful)
            return StatusCode(502, "PDF generation failed");

        return File(response.RawBytes, "application/pdf", "report.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    [HttpGet("generate")]
    public async Task<IActionResult> GenerateReport(int reportId)
    {
        var data = await GetReportData(reportId);

        var html = GenerateReportHtml(data);

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);

        return File(pdf.BinaryData, "application/pdf", "report.pdf");
        // No external API, no API key management, instant response!
    }

    private string GenerateReportHtml(ReportData data)
    {
        return $@"
            <html>
            <body>
                <h1>Report #{data.Id}</h1>
                <p>{data.Content}</p>
            </body>
            </html>";
    }
}
```

### Dependency Injection

**Before (CraftMyPDF):**
```csharp
// No standard DI - just use RestClient
public class PdfService
{
    private readonly string _apiKey;
    private readonly string _templateId;

    public PdfService(IConfiguration config)
    {
        _apiKey = config["CraftMyPdf:ApiKey"];
        _templateId = config["CraftMyPdf:TemplateId"];
    }

    public async Task<byte[]> GeneratePdfAsync(object data)
    {
        var client = new RestClient("https://api.craftmypdf.com/v1/create");
        // ... HTTP setup
    }
}
```

**After (IronPDF):**
```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ChromePdfRenderer>();
}

// Service
public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService(ChromePdfRenderer renderer)
    {
        _renderer = renderer;
    }

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Batch Processing

**Before (CraftMyPDF):**
```csharp
// Must make separate API calls, limited by rate limits
foreach (var invoice in invoices)
{
    var request = new RestRequest(Method.POST);
    request.AddHeader("X-API-KEY", apiKey);
    request.AddJsonBody(new { template_id = templateId, data = invoice });

    var response = await client.ExecuteAsync(request);
    // Rate limit: may need to wait between calls
    await Task.Delay(100); // Avoid rate limiting
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Threading.Tasks;

// Process in parallel - no rate limits!
Parallel.ForEach(invoices, invoice =>
{
    var renderer = new ChromePdfRenderer();
    var html = GenerateInvoiceHtml(invoice);
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"invoice-{invoice.Id}.pdf");
});

// Or merge into single document
var pdfs = invoices.AsParallel()
    .Select(inv => new ChromePdfRenderer().RenderHtmlAsPdf(GenerateInvoiceHtml(inv)))
    .ToList();

var combined = PdfDocument.Merge(pdfs);
combined.SaveAs("all-invoices.pdf");
```

### Docker Deployment

**Before (CraftMyPDF):**
```dockerfile
# No special requirements - just API calls
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# But requires internet access to api.craftmypdf.com
```

**After (IronPDF):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# IronPDF Linux dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libx11-dev \
    libnss3 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApp.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Works completely offline!
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

---

## Performance Considerations

### Speed Comparison

| Scenario | CraftMyPDF | IronPDF | Improvement |
|----------|------------|---------|-------------|
| Simple HTML | 1.5-3 seconds | 50-100ms | 15-30x faster |
| Complex template | 5-15 seconds | 200-500ms | 10-30x faster |
| URL rendering | 10-30 seconds | 1-3 seconds | 5-10x faster |
| Batch (100 PDFs) | Rate limited | Parallel | No limits |

### Why Local is Faster

1. **No Network Latency**: Round-trip to CraftMyPDF servers adds 200-500ms minimum
2. **No Upload/Download**: File transfer time eliminated
3. **No Queue Wait**: CraftMyPDF processes requests in queue
4. **No Rate Limits**: Generate as many PDFs as your CPU allows
5. **Caching**: Chromium engine caches resources locally

### Memory Optimization

```csharp
using IronPdf;

// For high-volume scenarios
Installation.ChromeGpuMode = IronPdf.Engines.Chrome.ChromeGpuModes.Disabled;

// Reuse renderer for multiple PDFs
var renderer = new ChromePdfRenderer();

foreach (var html in htmlDocuments)
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output-{Guid.NewGuid()}.pdf");
}
```

---

## Troubleshooting Guide

### Issue 1: API Key Removal

**Error:** Code still references API key

**Solution:** Replace with license key at startup:

```csharp
// Before (CraftMyPDF)
request.AddHeader("X-API-KEY", "your-api-key");

// After (IronPDF) - once at startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// No headers needed for individual calls
```

### Issue 2: Template Conversion

**Problem:** CraftMyPDF templates don't translate directly

**Solution:** Convert to HTML:

```csharp
// CraftMyPDF template placeholders:
// {%name%}, {%loop items%}, {%if condition%}

// IronPDF uses standard C# string interpolation:
var html = $@"
    <h1>{name}</h1>
    {string.Join("", items.Select(i => $"<p>{i}</p>"))}
    {(condition ? "<span>True</span>" : "")}";
```

### Issue 3: Async to Sync

**Problem:** Code uses await but IronPDF is sync by default

**Solution:** Either use sync or async methods:

```csharp
// Sync (simpler, recommended for most cases)
var pdf = renderer.RenderHtmlAsPdf(html);

// Async (if you need it)
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### Issue 4: Image URLs

**Problem:** Private images were hosted on CraftMyPDF

**Solution:** Embed images or use local paths:

```csharp
// Base64 embedding (works for private images)
var bytes = File.ReadAllBytes("private-image.png");
var base64 = Convert.ToBase64String(bytes);
var html = $"<img src='data:image/png;base64,{base64}' />";

// Or local file paths
var html = "<img src='file:///C:/images/logo.png' />";
```

### Issue 5: Rate Limiting Gone

**Problem:** Code has rate limit handling that's no longer needed

**Solution:** Remove delay/retry logic:

```csharp
// Before (CraftMyPDF) - needed to avoid 429 errors
await Task.Delay(100);
if (response.StatusCode == TooManyRequests) { /* retry */ }

// After (IronPDF) - no limits, just generate
var pdf = renderer.RenderHtmlAsPdf(html);
// Remove all rate limit code!
```

### Issue 6: Webhook Removal

**Problem:** Using async webhooks for PDF completion

**Solution:** IronPDF is synchronous—PDF is ready immediately:

```csharp
// Before (CraftMyPDF)
// POST with webhook_url, wait for callback
// Implement webhook endpoint to receive PDF

// After (IronPDF)
var pdf = renderer.RenderHtmlAsPdf(html);
// PDF is ready right now, no callback needed!
pdf.SaveAs("output.pdf");
```

### Issue 7: Output Looks Different

**Problem:** PDF output differs from CraftMyPDF

**Solution:** IronPDF uses Chromium, renders like Chrome browser:

```csharp
// IronPDF renders exactly like Chrome
// If output differs, check:

// 1. CSS compatibility (IronPDF has full CSS3 support)
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Screen; // vs Print

// 2. JavaScript execution
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.JavaScript(3000);

// 3. Web fonts
renderer.RenderingOptions.WaitFor.AllFontsLoaded();
```

### Issue 8: Linux Deployment

**Error:** Missing libraries in Docker/Linux

**Solution:** Install dependencies:

```bash
apt-get install -y libgdiplus libc6-dev libx11-dev libnss3 \
    libatk-bridge2.0-0 libdrm2 libxkbcommon0 libxcomposite1 \
    libxdamage1 libxfixes3 libxrandr2 libgbm1 libasound2
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all CraftMyPDF API calls**
  ```bash
  grep -r "CraftMyPDF" --include="*.cs" .
  grep -r "CreatePdf\|SendRequest" --include="*.cs" .
  ```
  **Why:** Identify all API calls to ensure complete migration coverage.

- [ ] **Export/document all template designs**
  ```bash
  # Export templates from CraftMyPDF dashboard
  ```
  **Why:** Preserve existing designs for accurate HTML/CSS conversion.

- [ ] **List all template variables and data bindings**
  ```bash
  # Document variables like {{Name}}, {{Date}}
  ```
  **Why:** Ensure all dynamic content is correctly mapped in the new system.

- [ ] **Calculate current API costs (for ROI comparison)**
  **Why:** Compare ongoing API costs with IronPDF's one-time license fee.

- [ ] **Note any webhook integrations**
  **Why:** Identify dependencies that need removal or replacement.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Set up test environment**
  **Why:** Ensure a safe space to test migration without affecting production.

### During Migration

- [ ] **Remove RestSharp/HTTP client code**
  ```csharp
  // Before (CraftMyPDF)
  var client = new RestClient("https://api.craftmypdf.com");
  var request = new RestRequest(Method.POST);

  // After (IronPDF)
  // No HTTP client needed
  ```
  **Why:** IronPDF operates locally, eliminating the need for HTTP requests.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to the project for local PDF generation.

- [ ] **Add license key to startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Convert templates to HTML/CSS**
  ```html
  <!-- Before (CraftMyPDF) -->
  <template>
    <div>{{Name}}</div>
  </template>

  <!-- After (IronPDF) -->
  <html>
    <body>
      <div>@Model.Name</div>
    </body>
  </html>
  ```
  **Why:** IronPDF uses standard HTML/CSS, allowing for more flexibility and control.

- [ ] **Replace template variables with C# interpolation**
  ```csharp
  // Before (CraftMyPDF)
  var template = "<div>{{Name}}</div>";

  // After (IronPDF)
  var template = $"<div>{name}</div>";
  ```
  **Why:** Use C# string interpolation for dynamic content insertion.

- [ ] **Remove async/await if not needed**
  ```csharp
  // Before (CraftMyPDF)
  var result = await client.ExecuteAsync(request);

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** Simplify code by removing unnecessary async operations.

- [ ] **Remove API key configuration**
  ```csharp
  // Before (CraftMyPDF)
  var apiKey = "API_KEY";

  // After (IronPDF)
  // No API key needed
  ```
  **Why:** IronPDF operates without external API keys.

- [ ] **Remove rate limiting code**
  ```csharp
  // Before (CraftMyPDF)
  if (requestsPerMinute > limit) { /* handle */ }

  // After (IronPDF)
  // No rate limiting needed
  ```
  **Why:** Local processing removes the need for rate limiting.

- [ ] **Remove webhook handlers**
  ```csharp
  // Before (CraftMyPDF)
  app.UseWebhookHandler();

  // After (IronPDF)
  // No webhooks needed
  ```
  **Why:** IronPDF does not rely on webhooks for PDF generation.

- [ ] **Update error handling (no HTTP errors)**
  ```csharp
  // Before (CraftMyPDF)
  if (response.StatusCode != HttpStatusCode.OK) { /* handle */ }

  // After (IronPDF)
  try {
      var pdf = renderer.RenderHtmlAsPdf(html);
  } catch (Exception ex) {
      // handle
  }
  ```
  **Why:** Transition from HTTP error handling to local exception handling.

### Post-Migration

- [ ] **Run all PDF generation tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Measure performance improvement**
  **Why:** Local generation should be significantly faster than cloud-based APIs.

- [ ] **Verify all templates converted correctly**
  **Why:** Ensure all templates render as expected with the new system.

- [ ] **Test batch processing**
  **Why:** Confirm that batch operations perform efficiently with IronPDF.

- [ ] **Test in all target environments**
  **Why:** Ensure compatibility across different deployment environments.

- [ ] **Update CI/CD pipelines**
  **Why:** Integrate IronPDF into the build and deployment processes.

- [ ] **Update Docker configurations**
  **Why:** Ensure Docker images include IronPDF dependencies.

- [ ] **Cancel CraftMyPDF subscription**
  **Why:** Reduce costs by eliminating unnecessary subscriptions.

- [ ] **Remove API key from secrets/config**
  **Why:** Clean up configuration files by removing unused keys.
---

## Additional Resources

### IronPDF Documentation
- **Getting Started**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF**: https://ironpdf.com/how-to/html-file-to-pdf/

### CraftMyPDF Reference (for migration)
- **API Documentation**: https://craftmypdf.com/pdf-generation-api/
- **Pricing**: https://craftmypdf.com/pricing/

### Migration Support
- **IronPDF Support**: https://ironpdf.com/support/
- **License Information**: https://ironpdf.com/licensing/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
