# How Do I Migrate from PDFmyURL to IronPDF in C#?

## Why Migrate from PDFmyURL?

PDFmyURL is a cloud-based API service that sends your documents to external servers for processing. While convenient for quick integrations, this architecture creates significant concerns for production applications:

### The Cloud Processing Problem

1. **Privacy & Data Security**: Every document you convert travels to and through PDFmyURL's servers—sensitive contracts, financial reports, personal data all processed externally
2. **Ongoing Subscription Costs**: Plans start at $20/month (Starter, 500 PDFs), $40/month (Professional, 2,000 PDFs), $70/month (Advanced, 5,000 PDFs), with no ownership at any tier
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
| Pricing Model | Monthly subscription ($20–$70+) | Perpetual license available |
| Rate Limits | Yes (plan-dependent) | None |
| Data Privacy | Data sent externally | Data stays local |
| HTML/CSS/JS Support | Server-side rendering (W3C compliant) | Full Chromium html to pdf c# engine |
| Async Pattern | HTTP request (network-bound) | Sync and async options |
| PDF Manipulation | Conversion only | Full suite (merge, split, edit) |

Explore performance benchmarks, cost analysis, and privacy enhancements in the [comprehensive migration documentation](https://ironpdf.com/blog/migration-guides/migrate-from-pdfmyurl-to-ironpdf/).

---

## NuGet Package Changes

PDFmyURL has **no NuGet package** — the service is a REST API and the optional `PDFmyURL.NET.dll` component is a direct DLL download (not on nuget.org). Most integrations call the API with `WebClient` / `HttpClient`, so the migration is mainly about code, not package references:

```bash
# Install IronPDF
dotnet add package IronPdf
```

If you used the PDFmyURL.NET.dll, remove the assembly reference from your project after migrating.

---

## Namespace Changes

```csharp
// Before: PDFmyURL — either plain HttpClient/WebClient against pdfmyurl.com/api,
// or the optional .NET component:
using PDFmyURLdotNET;          // only if you used PDFmyURL.NET.dll
using System.Net;               // WebClient / HttpClient

// After: IronPDF
using IronPdf;
using IronPdf.Rendering;
```

---

## API Mapping Reference

### Core Classes / Entry Points

| PDFmyURL | IronPDF | Notes |
|----------|---------|-------|
| `WebClient` / `HttpClient` posting to `https://pdfmyurl.com/api` | `ChromePdfRenderer` | Main conversion entry point |
| `PDFmyURLdotNET.PDFmyURL` (optional .NET component) | `ChromePdfRenderer` | Class from PDFmyURL.NET.dll |
| Form / query parameters | `ChromePdfRenderOptions` | Rendering configuration |
| HTTP response body bytes | `PdfDocument` | Result PDF object |

### Methods

| PDFmyURL | IronPDF | Notes |
|----------|---------|-------|
| `WebClient.DownloadFile(".../api?license=...&url=...", file)` | `renderer.RenderUrlAsPdf(url).SaveAs(file)` | URL to PDF |
| `await httpClient.PostAsync(".../api", content)` | `await Task.Run(() => renderer.RenderUrlAsPdf(url))` | Async URL conversion |
| POST with `html=` parameter | `renderer.RenderHtmlAsPdf(html)` | HTML string to PDF |
| `pdf.ConvertURL(url, file)` *(PDFmyURL.NET.dll)* | `renderer.RenderUrlAsPdf(url).SaveAs(file)` | .NET component method |
| `pdf.ConvertHTML(html, file)` *(PDFmyURL.NET.dll)* | `renderer.RenderHtmlAsPdf(html).SaveAs(file)` | .NET component method |
| `File.ReadAllText("input.html")` then POST `html=` | `renderer.RenderHtmlFileAsPdf(path)` | HTML file to PDF |
| `File.WriteAllBytes(file, responseBytes)` | `pdf.SaveAs(filename)` | Save to disk |
| HTTP response body (`byte[]`) | `pdf.BinaryData` | Get raw bytes |
| HTTP response stream | `pdf.Stream` | Get as stream |

### Configuration Options

PDFmyURL settings are passed as form/query parameters on the HTTP request (see https://pdfmyurl.com/html-to-pdf-api). The table below maps real PDFmyURL parameter names to IronPDF's `RenderingOptions`.

| PDFmyURL parameter | IronPDF (RenderingOptions) | Notes |
|--------------------|----------------------------|-------|
| `page_size=A4` | `.PaperSize = PdfPaperSize.A4` | Paper size |
| `page_size=Letter` | `.PaperSize = PdfPaperSize.Letter` | US Letter |
| `orientation=landscape` | `.PaperOrientation = PdfPaperOrientation.Landscape` | Orientation |
| `orientation=portrait` | `.PaperOrientation = PdfPaperOrientation.Portrait` | Portrait |
| `top=10&unit=mm` | `.MarginTop = 10` | Top margin (mm) |
| `bottom=10&unit=mm` | `.MarginBottom = 10` | Bottom margin (mm) |
| `left=10&unit=mm` | `.MarginLeft = 10` | Left margin (mm) |
| `right=10&unit=mm` | `.MarginRight = 10` | Right margin (mm) |
| `top=0&bottom=0&left=0&right=0` | `.MarginTop/Bottom/Left/Right = 0` | Zero margins |
| `width=8.5&unit=in` | `.SetCustomPaperSizeInInches(8.5, 11)` | Custom width |
| `height=11&unit=in` | `.SetCustomPaperSizeInInches(8.5, 11)` | Custom height |
| `header=<html>` | `.HtmlHeader = new HtmlHeaderFooter { HtmlFragment = html }` | Header |
| `footer=<html>` | `.HtmlFooter = new HtmlHeaderFooter { HtmlFragment = html }` | Footer |
| `zoom_factor=1.5` | `.Zoom = 150` | Zoom percentage |
| `javascript_time=500` | `.RenderDelay = 500` | JS wait time (ms) |
| `no_javascript=true` | `.EnableJavaScript = false` | Disable JS |
| `css_media_type=print` | `.CssMediaType = PdfCssMediaType.Print` | Print CSS |
| `css_media_type=screen` | `.CssMediaType = PdfCssMediaType.Screen` | Screen CSS |
| `encryption_level=128aes` | `pdf.SecuritySettings.MakeDocumentPrintOnly()` | Encryption |
| `user_password=pass` | `pdf.SecuritySettings.UserPassword = "pass"` | User password |
| `owner_password=pass` | `pdf.SecuritySettings.OwnerPassword = "pass"` | Owner password |
| `no_print=true` | `pdf.SecuritySettings.AllowUserPrinting = NoPrint` | Disable printing |
| `no_copy=true` | `pdf.SecuritySettings.AllowUserCopyPasteContent = false` | Disable copy |
| `no_modify=true` | `pdf.SecuritySettings.AllowUserEdits = NoEdit` | Disable edits |

### Authentication Comparison

| PDFmyURL | IronPDF |
|----------|---------|
| `license=<key>` query/form parameter on every API request | `IronPdf.License.LicenseKey = "LICENSE-KEY"` |
| License token per request | One-time at startup |
| Required for every call | Set once globally |

---

## Code Migration Examples

### Example 1: Basic URL to PDF

**Before (PDFmyURL REST API):**
```csharp
using System;
using System.Net;

class Program
{
    static void Main()
    {
        try
        {
            // License token is sent on every request to the public endpoint
            using (var client = new WebClient())
            {
                client.QueryString.Add("license", "your-license-key");
                client.QueryString.Add("url", "https://example.com");
                // URL sent to external servers; PDF binary returned in response body
                client.DownloadFile("https://pdfmyurl.com/api", "output.pdf");
            }

            Console.WriteLine("PDF created");
        }
        catch (WebException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
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

**Before (PDFmyURL REST API):**
```csharp
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

class Program
{
    static void Main()
    {
        try
        {
            string html = @"
                <html>
                <head><style>body { font-family: Arial; }</style></head>
                <body><h1>Report</h1><p>Content here</p></body>
                </html>";

            using (var client = new WebClient())
            {
                // Configure via form parameters on the API request
                var values = new NameValueCollection
                {
                    { "license",     "your-license-key" },
                    { "html",        html },
                    { "page_size",   "A4" },
                    { "orientation", "landscape" },
                    { "top",         "15" },
                    { "bottom",      "15" },
                    { "left",        "20" },
                    { "right",       "20" },
                    { "unit",        "mm" }
                };

                byte[] pdfBytes = client.UploadValues("https://pdfmyurl.com/api", "POST", values);
                File.WriteAllBytes("report.pdf", pdfBytes);
            }
        }
        catch (WebException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
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

**Before (PDFmyURL REST API):**
```csharp
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

class Program
{
    static void Main()
    {
        try
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    { "license", "your-license-key" },
                    { "html",    "<h1>Main Content</h1>" },
                    // Header — PDFmyURL accepts an HTML fragment in the `header` parameter
                    { "header",  "<div style='text-align:center;'>Company Report</div>" },
                    // Footer with PDFmyURL placeholder tokens for page numbers
                    { "footer",  "<div style='text-align:center;'>Page [page] of [topage]</div>" }
                };

                byte[] pdfBytes = client.UploadValues("https://pdfmyurl.com/api", "POST", values);
                File.WriteAllBytes("report.pdf", pdfBytes);
            }
        }
        catch (WebException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
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

**Before (PDFmyURL REST API):**
```csharp
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

public class ReportController : Controller
{
    private readonly string _license = "your-license-key";
    private static readonly HttpClient _http = new HttpClient();

    [HttpGet]
    public async Task<IActionResult> GenerateReport(string url)
    {
        try
        {
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("license", _license),
                new KeyValuePair<string, string>("url", url)
            });

            // Async HTTP call to external service
            var response = await _http.PostAsync("https://pdfmyurl.com/api", form);
            response.EnsureSuccessStatusCode();
            byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();

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

**Before (PDFmyURL REST API):**
```csharp
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

class Program
{
    static void Main()
    {
        try
        {
            string html = @"
                <div id='content'>Loading...</div>
                <script>
                    setTimeout(function() {
                        document.getElementById('content').innerHTML = 'Loaded!';
                    }, 1000);
                </script>";

            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    { "license",         "your-license-key" },
                    { "html",            html },
                    // Wait for JavaScript to execute (milliseconds)
                    { "javascript_time", "2000" },
                    // Use screen media for SPA-style rendering
                    { "css_media_type",  "screen" }
                };

                byte[] pdfBytes = client.UploadValues("https://pdfmyurl.com/api", "POST", values);
                File.WriteAllBytes("spa.pdf", pdfBytes);
            }
        }
        catch (WebException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
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

**Before (PDFmyURL REST API):**
```csharp
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

class Program
{
    static void Main()
    {
        try
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    { "license",          "your-license-key" },
                    { "html",             "<h1>Confidential</h1>" },
                    // Enable encryption and set passwords
                    { "encryption_level", "128aes" },
                    { "user_password",    "userpass123" },
                    { "owner_password",   "ownerpass456" }
                };

                byte[] pdfBytes = client.UploadValues("https://pdfmyurl.com/api", "POST", values);
                File.WriteAllBytes("secure.pdf", pdfBytes);
            }
        }
        catch (WebException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
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

**Before (PDFmyURL REST API):**
```csharp
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

class Program
{
    static void Main()
    {
        try
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    { "license", "your-license-key" },
                    { "html",    "<h1>Custom Size</h1>" },
                    // Custom page dimensions; `unit` switches between in/mm/cm/pt/px
                    { "width",   "5" },
                    { "height",  "7" },
                    { "unit",    "in" }
                    // Or:  width=127, height=178, unit=mm
                };

                byte[] pdfBytes = client.UploadValues("https://pdfmyurl.com/api", "POST", values);
                File.WriteAllBytes("custom.pdf", pdfBytes);
            }
        }
        catch (WebException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
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

**Before (PDFmyURL REST API):**
```csharp
using System;
using System.Net;

class Program
{
    static void Main()
    {
        try
        {
            using (var client = new WebClient())
            {
                client.QueryString.Add("license",        "your-license-key");
                client.QueryString.Add("url",            "https://example.com");
                // Use print media type (or "screen" for web-like rendering)
                client.QueryString.Add("css_media_type", "print");

                client.DownloadFile("https://pdfmyurl.com/api", "output.pdf");
            }
        }
        catch (WebException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
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

**Before (PDFmyURL REST API):**
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        const string license = "your-license-key";
        var http = new HttpClient();
        var urls = new List<string>
        {
            "https://example.com/page1",
            "https://example.com/page2",
            "https://example.com/page3"
        };

        // Each URL requires an API call (rate limits apply per plan tier)
        for (int i = 0; i < urls.Count; i++)
        {
            try
            {
                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("license", license),
                    new KeyValuePair<string, string>("url", urls[i])
                });

                var response = await http.PostAsync("https://pdfmyurl.com/api", form);
                response.EnsureSuccessStatusCode();
                byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes($"page_{i + 1}.pdf", pdfBytes);
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

**Before (PDFmyURL REST API):**
```csharp
using System;
using System.Net;

class Program
{
    static void Main()
    {
        try
        {
            using (var client = new WebClient())
            {
                client.QueryString.Add("license", "your-license-key");
                client.QueryString.Add("url", "https://example.com");
                client.DownloadFile("https://pdfmyurl.com/api", "output.pdf");
            }
        }
        catch (WebException ex)
        {
            // HTTP-level failures: invalid license, rate limit, network error,
            // service unavailable, invalid URL — inspect ex.Response/StatusCode
            Console.WriteLine($"PDFmyURL HTTP Error: {ex.Message}");
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

## Header / Footer Placeholders

PDFmyURL supports header and footer placeholder tokens (page numbers, dates, etc.) inside the `header` / `footer` form parameters; consult the live API reference at https://pdfmyurl.com/html-to-pdf-api for the canonical token list. IronPDF uses its own token syntax inside `HtmlHeaderFooter.HtmlFragment`:

| IronPDF Placeholder | Description |
|---------------------|-------------|
| `{page}` | Current page |
| `{total-pages}` | Total pages |
| `{date}` | Current date |
| `{time}` | Current time |
| `{html-title}` | HTML `<title>` |
| `{url}` | Source URL |

**Migration Example:**
```csharp
// Before (PDFmyURL form parameter)
//   footer=<div>Page [page] of [topage]</div>

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

### 1. License Token vs License Key

```csharp
// PDFmyURL: license token sent on every API request
client.QueryString.Add("license", "your-license-key");

// IronPDF: One-time license at startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Set once, typically in Program.cs or Startup.cs
```

### 2. Async Patterns

```csharp
// PDFmyURL: HttpClient.PostAsync against pdfmyurl.com/api
var response = await http.PostAsync("https://pdfmyurl.com/api", form);

// IronPDF: Sync by default, wrap for async
var pdf = await Task.Run(() => renderer.RenderUrlAsPdf(url));
```

### 3. Error Handling

```csharp
// PDFmyURL: HTTP errors surface as WebException / non-success status
catch (WebException e) { ... }

// IronPDF: Typed exceptions
catch (IronPdf.Exceptions.IronPdfRenderingException e) { ... }
```

### 4. Response Objects

```csharp
// PDFmyURL: HTTP response body is the PDF binary
File.WriteAllBytes("file.pdf", responseBytes);

// IronPDF: PdfDocument
pdf.SaveAs("file.pdf");
byte[] bytes = pdf.BinaryData;
```

### 5. Configuration Pattern

```csharp
// PDFmyURL: form/query parameters
values.Add("page_size", "A4");
values.Add("orientation", "landscape");

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

- [ ] Inventory all PDFmyURL API calls (search for `pdfmyurl.com/api` and `PDFmyURLdotNET`)
- [ ] Document current configuration parameters used (page_size, orientation, margins, header/footer, etc.)
- [ ] Identify header/footer placeholder tokens to migrate
- [ ] Note any async patterns that need adjustment
- [ ] Plan license key storage (environment variables recommended)
- [ ] Test with IronPDF trial license first

## Post-Migration Checklist

- [ ] Remove the optional `PDFmyURL.NET.dll` reference (if used) — there is no NuGet package to uninstall
- [ ] Update all namespace imports
- [ ] Replace the per-request license token with `IronPdf.License.LicenseKey`
- [ ] Convert form/query parameters to `RenderingOptions` properties
- [ ] Update placeholder syntax in headers/footers
- [ ] Update error handling code (WebException -> typed IronPDF exceptions)
- [ ] Test PDF output quality matches expectations
- [ ] Verify async patterns work correctly
- [ ] Install Linux dependencies if deploying to Linux
- [ ] Remove API credentials from configuration

---

## Find All PDFmyURL References

```bash
# Find PDFmyURL endpoint and component usage
grep -r "pdfmyurl.com/api\|PDFmyURLdotNET\|new PDFmyURL(" --include="*.cs" .

# Find license-token references
grep -r "license=\|licensekey" --include="*.cs" --include="*.json" --include="*.config" .
```

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PDFmyURL usages in codebase**
  ```bash
  grep -r "pdfmyurl.com/api\|PDFmyURLdotNET" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Install IronPdf (no PDFmyURL NuGet package exists to remove)**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** PDFmyURL is REST-only; remove any reference to `PDFmyURL.NET.dll` if you used the optional component.

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
