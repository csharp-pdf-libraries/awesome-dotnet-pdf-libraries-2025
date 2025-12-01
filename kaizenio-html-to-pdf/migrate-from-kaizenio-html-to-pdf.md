# How Do I Migrate from Kaizen.io HTML-to-PDF to IronPDF in C#?

## Table of Contents
1. [Why Migrate from Kaizen.io](#why-migrate-from-kaizenio)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from Kaizen.io

### The Cloud-Based API Challenges

Kaizen.io HTML-to-PDF, like other cloud-based PDF services, introduces limitations:

1. **Cloud Dependency**: Requires constant internet connection and external service availability
2. **Data Privacy Concerns**: Sensitive HTML content transmitted to third-party servers
3. **Network Latency**: Every PDF generation incurs network round-trip delays
4. **Per-Request Pricing**: Costs scale directly with usage volume
5. **Rate Limiting**: API throttling during high-traffic periods
6. **Limited Customization**: Constrained by what the cloud API exposes
7. **Vendor Lock-In**: API changes or service discontinuation risk

### The IronPDF Advantage

| Feature | Kaizen.io | IronPDF |
|---------|-----------|---------|
| Processing | Cloud (external servers) | Local (in-process) |
| Data Privacy | Data transmitted externally | Data never leaves your infrastructure |
| Latency | Network round-trip (100-500ms+) | Local processing (50-200ms) |
| Availability | Depends on external service | 100% under your control |
| Pricing | Per-request or subscription | One-time or annual license |
| Offline Mode | Not possible | Full functionality |
| Customization | Limited to API options | Full Chrome/Rendering control |
| JavaScript | Limited support | Full Chromium execution |

### Migration Benefits

- **Eliminate Cloud Dependency**: No internet required for PDF generation
- **Complete Data Privacy**: Sensitive documents never leave your network
- **Lower Latency**: 2-10x faster without network overhead
- **Predictable Costs**: Fixed license vs. usage-based pricing
- **Full Control**: Configure every aspect of rendering
- **Offline Capability**: Works without network connectivity
- **No Rate Limits**: Generate as many PDFs as your hardware allows

---

## Before You Start

### Prerequisites

1. **.NET Environment**: .NET Framework 4.6.2+ or .NET Core 3.1+ / .NET 5+
2. **NuGet Access**: Ability to install NuGet packages
3. **IronPDF License**: Free trial or purchased license key

### Installation

```bash
# Remove Kaizen.io package (package name may vary)
dotnet remove package Kaizen.HtmlToPdf
dotnet remove package Kaizen.IO.HtmlToPdf

# Install IronPDF
dotnet add package IronPdf
```

### License Configuration

```csharp
// Add at application startup (Program.cs or Startup.cs)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Identify Kaizen.io Usage

```bash
# Find all Kaizen.io references
grep -r "using Kaizen\|HtmlToPdfConverter\|ConversionOptions" --include="*.cs" .
grep -r "ConvertUrl\|ConvertHtml\|Kaizen" --include="*.cs" .
```

---

## Quick Start Migration

### Minimal Change Example

**Before (Kaizen.io):**
```csharp
using Kaizen.IO;

public class KaizenPdfService
{
    private readonly HtmlToPdfConverter _converter;
    private readonly string _apiKey;

    public KaizenPdfService(string apiKey)
    {
        _apiKey = apiKey;
        _converter = new HtmlToPdfConverter(apiKey);
    }

    public byte[] GeneratePdf(string html)
    {
        var options = new ConversionOptions
        {
            PageSize = PageSize.A4,
            Orientation = Orientation.Portrait,
            MarginTop = 20,
            MarginBottom = 20
        };

        return _converter.Convert(html, options);
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
        _renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        _renderer.RenderingOptions.MarginTop = 20;
        _renderer.RenderingOptions.MarginBottom = 20;
    }

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

---

## Complete API Reference

### Class Mappings

| Kaizen.io Class | IronPDF Equivalent | Notes |
|-----------------|-------------------|-------|
| `HtmlToPdfConverter` | `ChromePdfRenderer` | Main converter |
| `ConversionOptions` | `ChromePdfRenderOptions` | Via `RenderingOptions` |
| `HeaderOptions` | `HtmlHeaderFooter` | HTML headers |
| `FooterOptions` | `HtmlHeaderFooter` | HTML footers |
| `PageSize` | `PdfPaperSize` | Paper size enum |
| `Orientation` | `PdfPaperOrientation` | Orientation enum |

### Method Mappings

| Kaizen.io Method | IronPDF Equivalent | Notes |
|------------------|-------------------|-------|
| `converter.Convert(html)` | `renderer.RenderHtmlAsPdf(html)` | Returns PdfDocument |
| `converter.Convert(html, options)` | Configure `RenderingOptions` then `RenderHtmlAsPdf()` | Options on renderer |
| `converter.ConvertUrl(url)` | `renderer.RenderUrlAsPdf(url)` | Direct URL support |
| `converter.ConvertUrl(url, options)` | Configure `RenderingOptions` then `RenderUrlAsPdf()` | Options on renderer |
| `converter.ConvertFile(path)` | `renderer.RenderHtmlFileAsPdf(path)` | File-based conversion |
| `converter.ConvertAsync(...)` | `renderer.RenderHtmlAsPdfAsync(...)` | Async version |

### ConversionOptions Property Mappings

| Kaizen.io Property | IronPDF Equivalent | Notes |
|-------------------|-------------------|-------|
| `PageSize` | `RenderingOptions.PaperSize` | Enum value |
| `Orientation` | `RenderingOptions.PaperOrientation` | Portrait/Landscape |
| `MarginTop` | `RenderingOptions.MarginTop` | In millimeters |
| `MarginBottom` | `RenderingOptions.MarginBottom` | In millimeters |
| `MarginLeft` | `RenderingOptions.MarginLeft` | In millimeters |
| `MarginRight` | `RenderingOptions.MarginRight` | In millimeters |
| `Header` | `RenderingOptions.HtmlHeader` | HTML-based |
| `Footer` | `RenderingOptions.HtmlFooter` | HTML-based |
| `Header.HtmlContent` | `HtmlHeader.HtmlFragment` | Header HTML |
| `Footer.HtmlContent` | `HtmlFooter.HtmlFragment` | Footer HTML |
| `BaseUrl` | `RenderingOptions.BaseUrl` | For relative resources |
| `Timeout` | `RenderingOptions.Timeout` | In milliseconds |
| `EnableJavaScript` | `RenderingOptions.EnableJavaScript` | Default true |
| `WaitForComplete` | `RenderingOptions.WaitFor` | Wait strategies |
| `PrintBackground` | `RenderingOptions.PrintHtmlBackgrounds` | Background printing |
| `Scale` | `RenderingOptions.Zoom` | Zoom percentage |

### PageSize Mappings

| Kaizen.io PageSize | IronPDF PaperSize |
|-------------------|-------------------|
| `PageSize.A4` | `PdfPaperSize.A4` |
| `PageSize.Letter` | `PdfPaperSize.Letter` |
| `PageSize.Legal` | `PdfPaperSize.Legal` |
| `PageSize.A3` | `PdfPaperSize.A3` |
| `PageSize.A5` | `PdfPaperSize.A5` |
| Custom dimensions | `SetCustomPaperSizeInMillimeters()` |

### Placeholder Mappings

| Kaizen.io Placeholder | IronPDF Placeholder | Notes |
|----------------------|-------------------|-------|
| `{page}` | `{page}` | Current page |
| `{total}` | `{total-pages}` | Total pages |
| `{date}` | `{date}` | Current date |
| `{time}` | `{time}` | Current time |
| `{title}` | `{html-title}` | Document title |
| `{url}` | `{url}` | Document URL |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (Kaizen.io):**
```csharp
using Kaizen.IO;

public byte[] ConvertHtmlToPdf(string html)
{
    var converter = new HtmlToPdfConverter("YOUR_API_KEY");
    return converter.Convert(html);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertHtmlToPdf(string html)
{
    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Example 2: URL to PDF with Options

**Before (Kaizen.io):**
```csharp
using Kaizen.IO;

public byte[] ConvertUrlToPdf(string url)
{
    var converter = new HtmlToPdfConverter("YOUR_API_KEY");
    var options = new ConversionOptions
    {
        PageSize = PageSize.A4,
        Orientation = Orientation.Landscape,
        MarginTop = 15,
        MarginBottom = 15,
        MarginLeft = 10,
        MarginRight = 10
    };

    return converter.ConvertUrl(url, options);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertUrlToPdf(string url)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
    renderer.RenderingOptions.MarginTop = 15;
    renderer.RenderingOptions.MarginBottom = 15;
    renderer.RenderingOptions.MarginLeft = 10;
    renderer.RenderingOptions.MarginRight = 10;

    return renderer.RenderUrlAsPdf(url).BinaryData;
}
```

### Example 3: Headers and Footers

**Before (Kaizen.io):**
```csharp
using Kaizen.IO;

public byte[] CreateDocumentWithHeaderFooter(string html)
{
    var converter = new HtmlToPdfConverter("YOUR_API_KEY");
    var options = new ConversionOptions
    {
        Header = new HeaderOptions
        {
            HtmlContent = "<div style='text-align:center; font-size:10px;'>Company Report</div>"
        },
        Footer = new FooterOptions
        {
            HtmlContent = "<div style='text-align:center; font-size:10px;'>Page {page} of {total}</div>"
        },
        MarginTop = 30,
        MarginBottom = 30
    };

    return converter.Convert(html, options);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateDocumentWithHeaderFooter(string html)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center; font-size:10px;'>Company Report</div>",
        MaxHeight = 25
    };

    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center; font-size:10px;'>Page {page} of {total-pages}</div>",
        MaxHeight = 25
    };

    renderer.RenderingOptions.MarginTop = 30;
    renderer.RenderingOptions.MarginBottom = 30;

    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Example 4: Async Operations

**Before (Kaizen.io):**
```csharp
using Kaizen.IO;

public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
{
    var converter = new HtmlToPdfConverter("YOUR_API_KEY");
    var options = new ConversionOptions { PageSize = PageSize.A4 };

    return await converter.ConvertAsync(html, options);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

### Example 5: Custom Page Size

**Before (Kaizen.io):**
```csharp
using Kaizen.IO;

public byte[] CreateCustomSizePdf(string html, int widthMm, int heightMm)
{
    var converter = new HtmlToPdfConverter("YOUR_API_KEY");
    var options = new ConversionOptions
    {
        PageWidth = widthMm,
        PageHeight = heightMm
    };

    return converter.Convert(html, options);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateCustomSizePdf(string html, int widthMm, int heightMm)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(widthMm, heightMm);

    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Example 6: JavaScript Wait

**Before (Kaizen.io):**
```csharp
using Kaizen.IO;

public byte[] ConvertSpaPage(string url)
{
    var converter = new HtmlToPdfConverter("YOUR_API_KEY");
    var options = new ConversionOptions
    {
        WaitForComplete = true,
        JavaScriptDelay = 3000  // Wait 3 seconds for JS
    };

    return converter.ConvertUrl(url, options);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertSpaPage(string url)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.EnableJavaScript = true;
    renderer.RenderingOptions.RenderDelay = 3000;  // Wait 3 seconds

    // Or use JavaScript-based wait
    // renderer.RenderingOptions.WaitFor.JavaScript("window.appReady === true");

    return renderer.RenderUrlAsPdf(url).BinaryData;
}
```

### Example 7: Save to File vs Bytes

**Before (Kaizen.io):**
```csharp
using Kaizen.IO;

public void SavePdfToFile(string html, string outputPath)
{
    var converter = new HtmlToPdfConverter("YOUR_API_KEY");
    var pdfBytes = converter.Convert(html);
    File.WriteAllBytes(outputPath, pdfBytes);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void SavePdfToFile(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);  // Direct file saving
}
```

### Example 8: API Key to License Key

**Before (Kaizen.io):**
```csharp
using Kaizen.IO;

public class KaizenService
{
    private readonly HtmlToPdfConverter _converter;

    public KaizenService()
    {
        // API key per request
        _converter = new HtmlToPdfConverter("YOUR_KAIZEN_API_KEY");
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
        // License key set once at startup
        IronPdf.License.LicenseKey = "YOUR_IRONPDF_LICENSE_KEY";
        _renderer = new ChromePdfRenderer();
    }
}
```

### Example 9: Error Handling

**Before (Kaizen.io):**
```csharp
using Kaizen.IO;

public byte[] SafeConvert(string html)
{
    try
    {
        var converter = new HtmlToPdfConverter("YOUR_API_KEY");
        return converter.Convert(html);
    }
    catch (ApiException ex) when (ex.StatusCode == 429)
    {
        throw new Exception("Rate limit exceeded. Please wait and retry.");
    }
    catch (ApiException ex) when (ex.StatusCode == 401)
    {
        throw new Exception("Invalid API key.");
    }
    catch (HttpRequestException ex)
    {
        throw new Exception("Network error: " + ex.Message);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] SafeConvert(string html)
{
    try
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
    catch (IronPdf.Exceptions.IronPdfLicensingException ex)
    {
        throw new Exception("License error: " + ex.Message);
    }
    // No network errors, rate limits, or API availability issues!
}
```

### Example 10: Complete Service Migration

**Before (Kaizen.io Service):**
```csharp
using Kaizen.IO;
using System.Net.Http;

public class KaizenPdfService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public KaizenPdfService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<byte[]> GenerateReportAsync(ReportData data)
    {
        var converter = new HtmlToPdfConverter(_apiKey);
        var options = new ConversionOptions
        {
            PageSize = PageSize.A4,
            Orientation = Orientation.Portrait,
            MarginTop = 25,
            MarginBottom = 25,
            Header = new HeaderOptions
            {
                HtmlContent = $"<div>{data.Title}</div>"
            },
            Footer = new FooterOptions
            {
                HtmlContent = "<div>Page {page} of {total}</div>"
            }
        };

        string html = GenerateHtml(data);

        try
        {
            return await converter.ConvertAsync(html, options);
        }
        catch (ApiException ex)
        {
            // Handle rate limits, API errors, network issues
            if (ex.StatusCode == 429)
            {
                await Task.Delay(1000);
                return await converter.ConvertAsync(html, options);  // Retry
            }
            throw;
        }
    }

    private string GenerateHtml(ReportData data) => $"<html><body><h1>{data.Title}</h1></body></html>";
}
```

**After (IronPDF Service):**
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
        _renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        _renderer.RenderingOptions.MarginTop = 25;
        _renderer.RenderingOptions.MarginBottom = 25;
    }

    public async Task<byte[]> GenerateReportAsync(ReportData data)
    {
        // Set dynamic header
        _renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = $"<div>{data.Title}</div>",
            MaxHeight = 25
        };

        _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div>Page {page} of {total-pages}</div>",
            MaxHeight = 25
        };

        string html = GenerateHtml(data);

        // No retry logic needed - no network errors, no rate limits!
        var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }

    private string GenerateHtml(ReportData data) => $"<html><body><h1>{data.Title}</h1></body></html>";
}
```

---

## Advanced Scenarios

### Eliminating Network Dependency

```csharp
// Kaizen.io: Requires network, fails offline
// IronPDF: Works anywhere

// Even in air-gapped environments:
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);  // No network call!
```

### Removing Rate Limit Handling

```csharp
// DELETE all rate limit code:
// - Retry logic
// - Exponential backoff
// - Rate limit tracking
// - 429 error handling

// IronPDF has no rate limits - generate unlimited PDFs
```

### Parallel Processing Without API Limits

```csharp
// Kaizen.io: Limited by API rate limits and concurrency

// IronPDF: Limited only by your hardware
var renderer = new ChromePdfRenderer();

var tasks = htmlDocuments.Select(html =>
    renderer.RenderHtmlAsPdfAsync(html));

var pdfs = await Task.WhenAll(tasks);  // No throttling!
```

### Data Privacy Enhancement

```csharp
// Before: Sensitive data sent to cloud
var converter = new HtmlToPdfConverter(apiKey);
var pdfBytes = converter.Convert(sensitivePatientData);  // Data leaves network!

// After: Data stays local
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(sensitivePatientData);  // Never leaves server!
```

---

## Performance Considerations

### Latency Comparison

| Scenario | Kaizen.io | IronPDF |
|----------|-----------|---------|
| Simple HTML | 100-300ms | 50-100ms |
| Complex page | 500-1500ms | 100-300ms |
| With JavaScript | 1000-3000ms | 200-500ms |
| First render | 200-400ms | 1-3s (init) |
| Subsequent | 100-500ms | 50-200ms |

### Cost Comparison

| Scenario | Kaizen.io | IronPDF |
|----------|-----------|---------|
| 1,000 PDFs/month | Per-request cost | Same license cost |
| 10,000 PDFs/month | 10x cost | Same license cost |
| 100,000 PDFs/month | 100x cost | Same license cost |
| Offline usage | Not possible | Included |

### Optimization Tips

1. **Reuse Renderer**: Create once, use for all conversions
2. **Warm Up**: Initialize renderer at app startup
3. **Async Methods**: Use async for web applications
4. **Parallel Processing**: No API throttling limits

```csharp
public class OptimizedPdfService
{
    private readonly ChromePdfRenderer _renderer;
    private static bool _warmedUp;

    public OptimizedPdfService()
    {
        _renderer = new ChromePdfRenderer();

        if (!_warmedUp)
        {
            _renderer.RenderHtmlAsPdf("<html></html>");
            _warmedUp = true;
        }
    }
}
```

---

## Troubleshooting

### Issue 1: API Key Not Working

**Cause**: Kaizen.io API key no longer needed
**IronPDF Solution**: Use license key instead

```csharp
// DELETE: new HtmlToPdfConverter("API_KEY");
// ADD: IronPdf.License.LicenseKey = "LICENSE_KEY";
```

### Issue 2: Network Errors

**Cause**: Kaizen.io requires internet connection
**IronPDF Solution**: Works offline—delete all network error handling

### Issue 3: Rate Limit Errors

**Cause**: Kaizen.io API throttling
**IronPDF Solution**: No rate limits—delete retry logic

### Issue 4: Placeholder Syntax

**Cause**: Different placeholder format
**IronPDF Solution**: Update placeholders

```csharp
// {total} → {total-pages}
// {title} → {html-title}
```

### Issue 5: Return Type Difference

**Cause**: Kaizen returns `byte[]`, IronPDF returns `PdfDocument`
**IronPDF Solution**: Use `.BinaryData` property

```csharp
var pdf = renderer.RenderHtmlAsPdf(html);
byte[] bytes = pdf.BinaryData;
```

### Issue 6: Missing File Save

**Cause**: Kaizen.io returns bytes only
**IronPDF Solution**: Use `SaveAs()` method

```csharp
// Before: File.WriteAllBytes("output.pdf", pdfBytes);
// After: pdf.SaveAs("output.pdf");
```

### Issue 7: Timeout Differences

**Cause**: Network timeout vs render timeout
**IronPDF Solution**: Use `RenderingOptions.Timeout`

```csharp
renderer.RenderingOptions.Timeout = 60000;  // 60 seconds
```

### Issue 8: First Render Slow

**Cause**: IronPDF initializes Chromium on first use
**IronPDF Solution**: Warm up at startup

```csharp
// In Program.cs or Startup.cs:
new ChromePdfRenderer().RenderHtmlAsPdf("<html></html>");
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Identify all Kaizen.io usage**
  ```bash
  grep -r "using Kaizen.HtmlToPdf" --include="*.cs" .
  grep -r "HtmlToPdfConverter\|ConversionOptions" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document conversion options used**
  ```csharp
  // Find patterns like:
  var options = new ConversionOptions {
      PageSize = PageSize.A4,
      Orientation = Orientation.Portrait
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Note header/footer templates**
  ```csharp
  // Before (Kaizen.io)
  converter.Header = "Page [page] of [total]";
  converter.Footer = "Document Title";
  ```
  **Why:** Document existing templates to map them to IronPDF's HTML-based headers/footers.

- [ ] **List placeholder syntax**
  ```csharp
  // Before (Kaizen.io)
  var header = "Page {page} of {total}";
  ```
  **Why:** IronPDF uses different placeholders such as {total-pages}. Ensure all placeholders are updated correctly.

- [ ] **Check async patterns**
  ```csharp
  // Before (Kaizen.io)
  var pdfBytes = await converter.ConvertAsync(html);
  ```
  **Why:** Ensure async patterns are compatible with IronPDF's async methods.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Package Changes

- [ ] **Remove `Kaizen.HtmlToPdf` package (or similar)**
  ```bash
  dotnet remove package Kaizen.HtmlToPdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Install `IronPdf` NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Install IronPDF to replace the old PDF generation library.

- [ ] **Update using statements**
  ```csharp
  // Before (Kaizen.io)
  using Kaizen.HtmlToPdf;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure the code references the correct namespaces for IronPDF.

### Code Changes

- [ ] **Add license key configuration at startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace `HtmlToPdfConverter` with `ChromePdfRenderer`**
  ```csharp
  // Before (Kaizen.io)
  var converter = new HtmlToPdfConverter();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses ChromePdfRenderer for PDF generation.

- [ ] **Convert `ConversionOptions` to `RenderingOptions`**
  ```csharp
  // Before (Kaizen.io)
  var options = new ConversionOptions { PageSize = PageSize.A4 };

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** IronPDF's RenderingOptions provides comprehensive configuration for PDF rendering.

- [ ] **Update `Convert()` to `RenderHtmlAsPdf()`**
  ```csharp
  // Before (Kaizen.io)
  var pdfBytes = converter.Convert(html);

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** IronPDF's RenderHtmlAsPdf method is used for HTML to PDF conversion.

- [ ] **Update `ConvertUrl()` to `RenderUrlAsPdf()`**
  ```csharp
  // Before (Kaizen.io)
  var pdfBytes = converter.ConvertUrl(url);

  // After (IronPDF)
  var pdf = renderer.RenderUrlAsPdf(url);
  ```
  **Why:** IronPDF provides direct URL rendering with full JavaScript support.

- [ ] **Update placeholder syntax ({total} → {total-pages})**
  ```csharp
  // Before (Kaizen.io)
  var header = "Page {page} of {total}";

  // After (IronPDF)
  var header = "<div style='text-align:center;'>Page {page} of {total-pages}</div>";
  ```
  **Why:** Ensure placeholders are correctly mapped to IronPDF's syntax.

- [ ] **Replace `byte[]` returns with `pdf.BinaryData`**
  ```csharp
  // Before (Kaizen.io)
  byte[] pdfBytes = converter.Convert(html);

  // After (IronPDF)
  byte[] pdfBytes = pdf.BinaryData;
  ```
  **Why:** IronPDF's PdfDocument provides a BinaryData property for accessing the PDF content.

- [ ] **Add `.SaveAs()` for file saving**
  ```csharp
  // Before (Kaizen.io)
  File.WriteAllBytes("output.pdf", pdfBytes);

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF's PdfDocument provides a convenient SaveAs method for file output.

- [ ] **Remove retry/rate-limit logic**
  **Why:** IronPDF operates locally without network rate limits.

- [ ] **Remove network error handling for API calls**
  **Why:** IronPDF does not rely on external network calls, eliminating the need for such error handling.

### Testing

- [ ] **Test all PDF generation paths**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Verify header/footer rendering**
  **Why:** Ensure headers and footers are rendered correctly with IronPDF's HTML-based configuration.

- [ ] **Check placeholder rendering**
  **Why:** Confirm that all placeholders are replaced correctly in the generated PDFs.

- [ ] **Validate margins and page sizes**
  **Why:** Ensure that the page layout matches the expected output after migration.

- [ ] **Test async operations**
  **Why:** Verify that async PDF generation works as expected with IronPDF.

- [ ] **Benchmark performance improvement**
  **Why:** Measure performance improvements due to local processing with IronPDF.

### Post-Migration

- [ ] **Remove Kaizen.io API key configuration**
  **Why:** No longer needed as IronPDF operates locally.

- [ ] **Update environment variables**
  **Why:** Remove any environment variables related to the old library.

- [ ] **Remove rate limit configuration**
  **Why:** IronPDF does not have rate limits, simplifying configuration.

- [ ] **Update monitoring/alerting**
  **Why:** Adjust any monitoring that was specific to the old library's API usage.

- [ ] **Document new error patterns**
  **Why:** Ensure that any new error patterns introduced by IronPDF are documented for future reference.
---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
