# How Do I Migrate from PDFmyURL to IronPDF in C#?

## Why Migrate from PDFmyURL?

PDFmyURL is a cloud-based API service that sends your documents to external servers for processing. While convenient for quick integrations, this architecture creates significant concerns for production applications:

### The Cloud Processing Problem

1. **Privacy & Data Security**: Every document you convert travels to and through PDFmyURL's servers—sensitive contracts, financial reports, personal data all processed externally
2. **Ongoing Subscription Costs**: Starting at $39/month, annual costs exceed $468/year with no ownership
3. **Internet Dependency**: Every conversion requires network connectivity—no offline capability
4. **Rate Limits & Throttling**: API calls can be throttled during peak usage
5. **Latency**: Network round-trips add seconds to every conversion
6. **Service Availability**: Your application depends on a third-party service being online
7. **Vendor Lock-in**: API changes can break your integration without notice

### [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/) Benefits

IronPDF processes everything locally within your application:

- **Complete Privacy**: Documents never leave your server
- **One-Time Cost**: Perpetual license option eliminates recurring fees
- **Offline Capable**: Works without internet after initial setup
- **No Rate Limits**: Process unlimited documents
- **Lower Latency**: No network overhead
- **Full Control**: You control the processing environment
- **Modern Engine**: Chromium-based rendering with full CSS3/JS support

---

## Quick Migration Overview

| Aspect | PDFmyURL | IronPDF |
|--------|----------|---------|
| Processing Location | External servers | Local (your server) |
| Authentication | API key per request | One-time license key |
| Network Required | Every conversion | Only initial setup |
| Pricing Model | Monthly subscription ($39+) | Perpetual license available |
| Rate Limits | Yes (plan-dependent) | None |
| Data Privacy | Data sent externally | Data stays local |
| HTML/CSS/JS Support | W3C compliant | Full Chromium html to pdf c# engine |
| Async Pattern | Required (async only) | Sync and async options |
| PDF Manipulation | Limited | Full suite (merge, split, edit) |

Explore performance benchmarks and pricing in the [comprehensive guide](https://ironsoftware.com/suite/blog/comparison/compare-pdfmyurl-vs-ironpdf/).

---

## NuGet Package Changes

```bash
# Remove PDFmyURL packages
dotnet remove package PdfMyUrl
dotnet remove package Pdfcrowd

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// Before: PDFmyURL
using PdfMyUrl;
using Pdfcrowd;

// After: IronPDF
using IronPdf;
using IronPdf.Rendering;
```

---

## API Mapping Reference

### Core Classes

| PDFmyURL | IronPDF | Notes |
|----------|---------|-------|
| `PdfMyUrlClient` | `ChromePdfRenderer` | Main conversion class |
| `HtmlToPdfClient` | `ChromePdfRenderer` | Pdfcrowd SDK equivalent |
| `ConvertOptions` | `ChromePdfRenderOptions` | Rendering configuration |
| API response object | `PdfDocument` | Result PDF object |

### Methods

| PDFmyURL | IronPDF | Notes |
|----------|---------|-------|
| `client.ConvertUrl(url)` | `renderer.RenderUrlAsPdf(url)` | URL to PDF |
| `client.ConvertUrlAsync(url)` | `await Task.Run(() => renderer.RenderUrlAsPdf(url))` | Async URL conversion |
| `client.ConvertHtml(html)` | `renderer.RenderHtmlAsPdf(html)` | HTML string to PDF |
| `client.ConvertHtmlAsync(html)` | `await Task.Run(() => renderer.RenderHtmlAsPdf(html))` | Async HTML conversion |
| `client.ConvertHtmlFile(path)` | `renderer.RenderHtmlFileAsPdf(path)` | HTML file to PDF |
| `convertUrlToFile(url, file)` | `renderer.RenderUrlAsPdf(url).SaveAs(file)` | Pdfcrowd style |
| `convertStringToFile(html, file)` | `renderer.RenderHtmlAsPdf(html).SaveAs(file)` | Pdfcrowd style |
| `convertFileToFile(input, output)` | `renderer.RenderHtmlFileAsPdf(input).SaveAs(output)` | File to file |
| `response.Save(filename)` | `pdf.SaveAs(filename)` | Save to disk |
| `response.GetBytes()` | `pdf.BinaryData` | Get raw bytes |
| `response.GetStream()` | `pdf.Stream` | Get as stream |

### Configuration Options

| PDFmyURL (setXxx methods) | IronPDF (RenderingOptions) | Notes |
|---------------------------|---------------------------|-------|
| `setPageSize("A4")` | `.PaperSize = PdfPaperSize.A4` | Paper size |
| `setPageSize("Letter")` | `.PaperSize = PdfPaperSize.Letter` | US Letter |
| `setOrientation("landscape")` | `.PaperOrientation = PdfPaperOrientation.Landscape` | Orientation |
| `setOrientation("portrait")` | `.PaperOrientation = PdfPaperOrientation.Portrait` | Portrait |
| `setMarginTop("10mm")` | `.MarginTop = 10` | Top margin (mm) |
| `setMarginBottom("10mm")` | `.MarginBottom = 10` | Bottom margin (mm) |
| `setMarginLeft("10mm")` | `.MarginLeft = 10` | Left margin (mm) |
| `setMarginRight("10mm")` | `.MarginRight = 10` | Right margin (mm) |
| `setNoMargins(true)` | `.MarginTop/Bottom/Left/Right = 0` | Zero margins |
| `setPageWidth("8.5in")` | `.SetCustomPaperSizeInInches(8.5, 11)` | Custom width |
| `setPageHeight("11in")` | `.SetCustomPaperSizeInInches(8.5, 11)` | Custom height |
| `setHeaderHtml(html)` | `.HtmlHeader = new HtmlHeaderFooter { HtmlFragment = html }` | Header |
| `setFooterHtml(html)` | `.HtmlFooter = new HtmlHeaderFooter { HtmlFragment = html }` | Footer |
| `setHeaderHeight("20mm")` | `.HtmlHeader.MaxHeight = 20` | Header height |
| `setFooterHeight("20mm")` | `.HtmlFooter.MaxHeight = 20` | Footer height |
| `setZoomFactor(1.5)` | `.Zoom = 150` | Zoom percentage |
| `setJavascriptDelay(500)` | `.RenderDelay = 500` | JS wait time (ms) |
| `setDisableJavascript(true)` | `.EnableJavaScript = false` | Disable JS |
| `setUsePrintMedia(true)` | `.CssMediaType = PdfCssMediaType.Print` | Print CSS |
| `setUseScreenMedia(true)` | `.CssMediaType = PdfCssMediaType.Screen` | Screen CSS |
| `setEncrypt(true)` | `pdf.SecuritySettings.MakeDocumentPrintOnly()` | Encryption |
| `setUserPassword("pass")` | `pdf.SecuritySettings.UserPassword = "pass"` | User password |
| `setOwnerPassword("pass")` | `pdf.SecuritySettings.OwnerPassword = "pass"` | Owner password |

### Authentication Comparison

| PDFmyURL | IronPDF |
|----------|---------|
| `new HtmlToPdfClient("username", "apikey")` | `IronPdf.License.LicenseKey = "LICENSE-KEY"` |
| API key per request | One-time at startup |
| Required for every call | Set once globally |

---

## Code Migration Examples

### Example 1: Basic URL to PDF

**Before (PDFmyURL/Pdfcrowd):**
```csharp
using Pdfcrowd;
using System;

class Program
{
    static void Main()
    {
        try
        {
            // Requires API credentials for every conversion
            var client = new HtmlToPdfClient("username", "apikey");

            // URL sent to external servers for processing
            client.convertUrlToFile("https://example.com", "output.pdf");

            Console.WriteLine("PDF created");
        }
        catch (Error why)
        {
            Console.WriteLine("Error: " + why);
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
        // One-time license configuration
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // All processing happens locally
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("output.pdf");

        Console.WriteLine("PDF created");
    }
}
```

### Example 2: HTML String with Options

**Before (PDFmyURL/Pdfcrowd):**
```csharp
using Pdfcrowd;
using System;

class Program
{
    static void Main()
    {
        try
        {
            var client = new HtmlToPdfClient("username", "apikey");

            // Configure via setter methods
            client.setPageSize("A4");
            client.setOrientation("landscape");
            client.setMarginTop("15mm");
            client.setMarginBottom("15mm");
            client.setMarginLeft("20mm");
            client.setMarginRight("20mm");

            string html = @"
                <html>
                <head><style>body { font-family: Arial; }</style></head>
                <body><h1>Report</h1><p>Content here</p></body>
                </html>";

            client.convertStringToFile(html, "report.pdf");
        }
        catch (Error why)
        {
            Console.WriteLine("Error: " + why);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Configure via RenderingOptions properties
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.MarginTop = 15;
        renderer.RenderingOptions.MarginBottom = 15;
        renderer.RenderingOptions.MarginLeft = 20;
        renderer.RenderingOptions.MarginRight = 20;

        string html = @"
            <html>
            <head><style>body { font-family: Arial; }</style></head>
            <body><h1>Report</h1><p>Content here</p></body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report.pdf");
    }
}
```

### Example 3: Headers and Footers

**Before (PDFmyURL/Pdfcrowd):**
```csharp
using Pdfcrowd;
using System;

class Program
{
    static void Main()
    {
        try
        {
            var client = new HtmlToPdfClient("username", "apikey");

            // Set header
            client.setHeaderHtml("<div style='text-align:center;'>Company Report</div>");
            client.setHeaderHeight("25mm");

            // Set footer with page numbers
            client.setFooterHtml("<div style='text-align:center;'>Page {page_number} of {total_pages}</div>");
            client.setFooterHeight("20mm");

            client.convertStringToFile("<h1>Main Content</h1>", "report.pdf");
        }
        catch (Error why)
        {
            Console.WriteLine("Error: " + why);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Set header
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;'>Company Report</div>",
            MaxHeight = 25
        };

        // Set footer with page numbers (different placeholders)
        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
            MaxHeight = 20
        };

        var pdf = renderer.RenderHtmlAsPdf("<h1>Main Content</h1>");
        pdf.SaveAs("report.pdf");
    }
}
```

### Example 4: Async Web Application

**Before (PDFmyURL):**
```csharp
using PdfMyUrl;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class ReportController : Controller
{
    private readonly string _apiKey = "your-api-key";

    [HttpGet]
    public async Task<IActionResult> GenerateReport(string url)
    {
        try
        {
            var client = new PdfMyUrlClient(_apiKey);

            // Async call to external service (required for API)
            var response = await client.ConvertUrlAsync(url);
            byte[] pdfBytes = response.GetBytes();

            return File(pdfBytes, "application/pdf", "report.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "PDF generation failed: " + ex.Message);
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
    public async Task<IActionResult> GenerateReport(string url)
    {
        try
        {
            // Local processing - wrap in Task.Run for async pattern
            var pdf = await Task.Run(() =>
            {
                var renderer = new ChromePdfRenderer();
                return renderer.RenderUrlAsPdf(url);
            });

            return File(pdf.BinaryData, "application/pdf", "report.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "PDF generation failed: " + ex.Message);
        }
    }
}
```

### Example 5: JavaScript Rendering with Delay

**Before (PDFmyURL/Pdfcrowd):**
```csharp
using Pdfcrowd;
using System;

class Program
{
    static void Main()
    {
        try
        {
            var client = new HtmlToPdfClient("username", "apikey");

            // Wait for JavaScript to execute
            client.setJavascriptDelay(2000);

            // Enable screen media for SPA rendering
            client.setUseScreenMedia(true);

            string html = @"
                <div id='content'>Loading...</div>
                <script>
                    setTimeout(function() {
                        document.getElementById('content').innerHTML = 'Loaded!';
                    }, 1000);
                </script>";

            client.convertStringToFile(html, "spa.pdf");
        }
        catch (Error why)
        {
            Console.WriteLine("Error: " + why);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Wait for JavaScript to execute
        renderer.RenderingOptions.RenderDelay = 2000;

        // Enable screen media for SPA rendering
        renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Screen;

        // JavaScript enabled by default
        renderer.RenderingOptions.EnableJavaScript = true;

        string html = @"
            <div id='content'>Loading...</div>
            <script>
                setTimeout(function() {
                    document.getElementById('content').innerHTML = 'Loaded!';
                }, 1000);
            </script>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("spa.pdf");
    }
}
```

### Example 6: Password-Protected PDF

**Before (PDFmyURL/Pdfcrowd):**
```csharp
using Pdfcrowd;
using System;

class Program
{
    static void Main()
    {
        try
        {
            var client = new HtmlToPdfClient("username", "apikey");

            // Enable encryption and set passwords
            client.setEncrypt(true);
            client.setUserPassword("userpass123");
            client.setOwnerPassword("ownerpass456");

            client.convertStringToFile("<h1>Confidential</h1>", "secure.pdf");
        }
        catch (Error why)
        {
            Console.WriteLine("Error: " + why);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential</h1>");

        // Apply security after rendering
        pdf.SecuritySettings.UserPassword = "userpass123";
        pdf.SecuritySettings.OwnerPassword = "ownerpass456";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;

        pdf.SaveAs("secure.pdf");
    }
}
```

### Example 7: Custom Page Size

**Before (PDFmyURL/Pdfcrowd):**
```csharp
using Pdfcrowd;
using System;

class Program
{
    static void Main()
    {
        try
        {
            var client = new HtmlToPdfClient("username", "apikey");

            // Custom page dimensions
            client.setPageWidth("5in");
            client.setPageHeight("7in");

            // Or set in mm
            // client.setPageWidth("127mm");
            // client.setPageHeight("178mm");

            client.convertStringToFile("<h1>Custom Size</h1>", "custom.pdf");
        }
        catch (Error why)
        {
            Console.WriteLine("Error: " + why);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Custom page dimensions in inches
        renderer.RenderingOptions.SetCustomPaperSizeInInches(5, 7);

        // Or in millimeters
        // renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(127, 178);

        // Or in centimeters
        // renderer.RenderingOptions.SetCustomPaperSizeInCentimeters(12.7, 17.8);

        var pdf = renderer.RenderHtmlAsPdf("<h1>Custom Size</h1>");
        pdf.SaveAs("custom.pdf");
    }
}
```

### Example 8: Print Media vs Screen Media

**Before (PDFmyURL/Pdfcrowd):**
```csharp
using Pdfcrowd;
using System;

class Program
{
    static void Main()
    {
        try
        {
            var client = new HtmlToPdfClient("username", "apikey");

            // Use print media type
            client.setUsePrintMedia(true);

            // Or screen media for web-like rendering
            // client.setUseScreenMedia(true);

            client.convertUrlToFile("https://example.com", "output.pdf");
        }
        catch (Error why)
        {
            Console.WriteLine("Error: " + why);
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Use print media type
        renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

        // Or screen media for web-like rendering
        // renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Screen;

        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 9: Batch Processing Multiple URLs

**Before (PDFmyURL):**
```csharp
using PdfMyUrl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new PdfMyUrlClient("your-api-key");
        var urls = new List<string>
        {
            "https://example.com/page1",
            "https://example.com/page2",
            "https://example.com/page3"
        };

        // Each URL requires an API call (rate limits apply)
        for (int i = 0; i < urls.Count; i++)
        {
            try
            {
                var response = await client.ConvertUrlAsync(urls[i]);
                response.Save($"page_{i + 1}.pdf");
                Console.WriteLine($"Converted: page_{i + 1}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed: {urls[i]} - {ex.Message}");
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
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var urls = new List<string>
        {
            "https://example.com/page1",
            "https://example.com/page2",
            "https://example.com/page3"
        };

        var renderer = new ChromePdfRenderer();
        var pdfs = new List<PdfDocument>();

        // Process locally - no rate limits
        foreach (var url in urls)
        {
            var pdf = await Task.Run(() => renderer.RenderUrlAsPdf(url));
            pdfs.Add(pdf);
            Console.WriteLine($"Converted: {url}");
        }

        // Bonus: Merge all into single PDF (not available in PDFmyURL)
        var merged = PdfDocument.Merge(pdfs);
        merged.SaveAs("all_pages.pdf");
        Console.WriteLine("Merged all pages into all_pages.pdf");
    }
}
```

### Example 10: Error Handling Comparison

**Before (PDFmyURL/Pdfcrowd):**
```csharp
using Pdfcrowd;
using System;

class Program
{
    static void Main()
    {
        try
        {
            var client = new HtmlToPdfClient("username", "apikey");
            client.convertUrlToFile("https://example.com", "output.pdf");
        }
        catch (Error why)
        {
            // Pdfcrowd-specific error type
            Console.WriteLine($"Pdfcrowd Error: {why.Message}");

            // Could be: invalid credentials, rate limit, network error,
            // service unavailable, invalid URL, etc.
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Error: {ex.Message}");
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
            var pdf = renderer.RenderUrlAsPdf("https://example.com");
            pdf.SaveAs("output.pdf");
        }
        catch (IronPdf.Exceptions.IronPdfLicenseException ex)
        {
            Console.WriteLine($"License Error: {ex.Message}");
            // Invalid or expired license key
        }
        catch (IronPdf.Exceptions.IronPdfRenderingException ex)
        {
            Console.WriteLine($"Rendering Error: {ex.Message}");
            // URL unreachable, invalid HTML, JavaScript error
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Error: {ex.Message}");
        }
    }
}
```

---

## Placeholder Migration

PDFmyURL/Pdfcrowd uses different placeholder syntax for headers and footers:

| PDFmyURL Placeholder | IronPDF Placeholder | Description |
|---------------------|---------------------|-------------|
| `{page_number}` | `{page}` | Current page |
| `{total_pages}` | `{total-pages}` | Total pages |
| `{page_number}/{total_pages}` | `{page} of {total-pages}` | Page X of Y |
| `{date}` | `{date}` | Current date |
| `{time}` | `{time}` | Current time |
| `{title}` | `{html-title}` | HTML title |
| `{url}` | `{url}` | Source URL |

**Migration Example:**
```csharp
// Before (PDFmyURL)
client.setFooterHtml("Page {page_number} of {total_pages}");

// After (IronPDF)
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "Page {page} of {total-pages}"
};
```

---

## Features Not Available in PDFmyURL

IronPDF provides many features that PDFmyURL cannot offer:

### PDF Manipulation

```csharp
// Merge PDFs
var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);

// Split PDF
var pages = pdf.CopyPages(0, 4);

// Add/Remove pages
pdf.AppendPdf(otherPdf);
pdf.RemovePage(2);

// Rotate pages
pdf.RotateAllPages(PdfPageRotation.Rotate90);
```

### Text Extraction

```csharp
// Extract text
string allText = pdf.ExtractAllText();
string pageText = pdf.Pages[0].Text;
```

### Watermarks

```csharp
// Add watermark
pdf.ApplyWatermark("<h2 style='color:red;'>CONFIDENTIAL</h2>",
    30,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);
```

### Form Handling

```csharp
// Fill form fields
pdf.Form.GetFieldByName("CustomerName").Value = "John Doe";
pdf.Form.GetFieldByName("OrderDate").Value = DateTime.Now.ToShortDateString();

// Flatten forms
pdf.Form.Flatten();
```

### Annotations and Stamps

```csharp
// Add stamp
var stamp = new HtmlStamp()
{
    Html = "<img src='logo.png' />",
    Width = 100,
    Height = 50
};
pdf.ApplyStamp(stamp);
```

### Digital Signatures

```csharp
// Sign PDF
pdf.Sign(new PdfSignature("certificate.pfx", "password")
{
    SigningContact = "support@company.com",
    SigningReason = "Document Approval"
});
```

---

## Common Migration Issues

### 1. API Key vs License Key

```csharp
// PDFmyURL: API key per request
var client = new HtmlToPdfClient("username", "apikey");

// IronPDF: One-time license at startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Set once, typically in Program.cs or Startup.cs
```

### 2. Async Patterns

```csharp
// PDFmyURL: Native async
var response = await client.ConvertUrlAsync(url);

// IronPDF: Sync by default, wrap for async
var pdf = await Task.Run(() => renderer.RenderUrlAsPdf(url));
```

### 3. Error Handling

```csharp
// PDFmyURL: Pdfcrowd.Error
catch (Pdfcrowd.Error e) { ... }

// IronPDF: Standard exceptions
catch (IronPdf.Exceptions.IronPdfRenderingException e) { ... }
```

### 4. Response Objects

```csharp
// PDFmyURL: Response wrapper
response.Save("file.pdf");
byte[] bytes = response.GetBytes();

// IronPDF: PdfDocument
pdf.SaveAs("file.pdf");
byte[] bytes = pdf.BinaryData;
```

### 5. Configuration Pattern

```csharp
// PDFmyURL: Setter methods
client.setPageSize("A4");
client.setOrientation("landscape");

// IronPDF: Properties
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
```

---

## Server Deployment

### PDFmyURL
- No server configuration needed (API-based)
- Requires outbound internet access
- Subject to API rate limits

### IronPDF
- First run downloads Chromium (~150MB)
- Works offline after initial setup
- Linux dependencies required:

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

- [ ] Inventory all PDFmyURL/Pdfcrowd API calls
- [ ] Document current configuration options used
- [ ] Identify header/footer placeholders to migrate
- [ ] Note any async patterns that need adjustment
- [ ] Plan license key storage (environment variables recommended)
- [ ] Test with IronPDF trial license first

## Post-Migration Checklist

- [ ] Remove PDFmyURL/Pdfcrowd NuGet packages
- [ ] Update all namespace imports
- [ ] Replace API key with IronPDF license key
- [ ] Convert setter methods to RenderingOptions properties
- [ ] Update placeholder syntax in headers/footers
- [ ] Update error handling code
- [ ] Test PDF output quality matches expectations
- [ ] Verify async patterns work correctly
- [ ] Install Linux dependencies if deploying to Linux
- [ ] Remove API credentials from configuration

---

## Find All PDFmyURL References

```bash
# Find PDFmyURL usage
grep -r "PdfMyUrl\|Pdfcrowd\|HtmlToPdfClient" --include="*.cs" .

# Find API key references
grep -r "apikey\|api-key\|api_key" --include="*.cs" --include="*.json" --include="*.config" .

# Find placeholder patterns to migrate
grep -r "{page_number}\|{total_pages}" --include="*.cs" .
```

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PDFmyURL usages in codebase**
  ```bash
  grep -r "using PDFmyURL" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package PDFmyURL
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
