# How Do I Migrate from PDFBolt to IronPDF in C#?

## Why Migrate from PDFBolt to IronPDF?

PDFBolt is a cloud-only SaaS service that processes your documents on external servers. While convenient for quick prototypes, this architecture creates significant challenges for production applications:

### Critical PDFBolt Limitations

1. **Cloud-Only Processing**: Every document passes through external servers—no self-hosted option
2. **Data Privacy Risks**: Sensitive documents (contracts, medical records, financial data) transmitted externally
3. **Usage Limits**: Free tier capped at 100 documents/month; pay-per-document pricing adds up quickly
4. **Network Dependency**: Internet outages or PDFBolt downtime = your PDF generation stops
5. **Latency**: Network round-trip adds seconds to every conversion
6. **Compliance Issues**: GDPR, HIPAA, SOC2 audits complicated by external processing
7. **API Key Security**: Leaked keys = unauthorized usage billed to your account
8. **Vendor Lock-in**: Your application fails if PDFBolt changes terms or shuts down

### IronPDF Advantages

| Concern | PDFBolt | IronPDF |
|---------|---------|---------|
| **Data Location** | External servers | Your servers only |
| **Usage Limits** | 100 free, then per-document | Unlimited |
| **Internet Required** | Yes, always | No |
| **Latency** | Network round-trip | Milliseconds |
| **Compliance** | Complex (external processing) | Simple (local processing) |
| **Cost Model** | Per-document | One-time or annual |
| **Offline Operation** | Impossible | Fully supported |
| **API Key Risks** | Leaked = billed | License key, no billing risk |

---

## NuGet Package Changes

```bash
# Remove PDFBolt
dotnet remove package PDFBolt

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// PDFBolt
using PDFBolt;
using PDFBolt.Models;
using PDFBolt.Options;

// IronPDF
using IronPdf;
using IronPdf.Editing;
using IronPdf.Rendering;
```

---

## Complete API Reference

### Core Class Mappings

| PDFBolt | IronPDF | Notes |
|---------|---------|-------|
| `new Client(apiKey)` | `new ChromePdfRenderer()` | No API key needed |
| `new HtmlToPdfConverter()` | `new ChromePdfRenderer()` | Same renderer for all |
| `new PdfOptions()` | `renderer.RenderingOptions` | Property-based config |
| `PdfResult` | `PdfDocument` | Rich document object |

### Conversion Methods

| PDFBolt | IronPDF | Notes |
|---------|---------|-------|
| `await client.HtmlToPdf(html)` | `renderer.RenderHtmlAsPdf(html)` | Sync by default |
| `await client.HtmlToPdf(html, options)` | `renderer.RenderHtmlAsPdf(html)` | Options on renderer |
| `await client.UrlToPdf(url)` | `renderer.RenderUrlAsPdf(url)` | Sync by default |
| `await client.UrlToPdf(url, options)` | `renderer.RenderUrlAsPdf(url)` | Options on renderer |
| `await client.FileToPdf(path)` | `renderer.RenderHtmlFileAsPdf(path)` | HTML files |
| `converter.ConvertHtmlString(html)` | `renderer.RenderHtmlAsPdf(html)` | Returns PdfDocument |
| `converter.ConvertUrl(url)` | `renderer.RenderUrlAsPdf(url)` | Returns PdfDocument |

### Output Methods

| PDFBolt | IronPDF | Notes |
|---------|---------|-------|
| `await result.SaveToFile(path)` | `pdf.SaveAs(path)` | Sync method |
| `result.GetBytes()` | `pdf.BinaryData` | Property access |
| `await result.GetBytesAsync()` | `pdf.BinaryData` | Already available |
| `result.GetStream()` | `pdf.Stream` | Stream property |

### Page Configuration

| PDFBolt | IronPDF | Notes |
|---------|---------|-------|
| `options.PageSize = PageSize.A4` | `renderer.RenderingOptions.PaperSize = PdfPaperSize.A4` | Enum names differ |
| `options.PageSize = PageSize.Letter` | `renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter` | Standard sizes |
| `options.Orientation = Orientation.Landscape` | `renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape` | Landscape mode |
| `options.Orientation = Orientation.Portrait` | `renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait` | Default |
| `options.Width = 210` | `renderer.RenderingOptions.SetCustomPaperSizeinMillimeters(210, 297)` | Custom size |

### Margins Configuration

| PDFBolt | IronPDF | Notes |
|---------|---------|-------|
| `options.MarginTop = 20` | `renderer.RenderingOptions.MarginTop = 20` | In millimeters |
| `options.MarginBottom = 20` | `renderer.RenderingOptions.MarginBottom = 20` | Individual properties |
| `options.MarginLeft = 15` | `renderer.RenderingOptions.MarginLeft = 15` | No margins object |
| `options.MarginRight = 15` | `renderer.RenderingOptions.MarginRight = 15` | Direct assignment |
| `options.Margins = new Margins(t,r,b,l)` | Individual properties | See above |

### Rendering Options

| PDFBolt | IronPDF | Notes |
|---------|---------|-------|
| `options.PrintBackground = true` | `renderer.RenderingOptions.PrintHtmlBackgrounds = true` | CSS backgrounds |
| `options.WaitForNetworkIdle = true` | `renderer.RenderingOptions.WaitFor.NetworkIdle()` | Wait strategy |
| `options.WaitTime = 2000` | `renderer.RenderingOptions.WaitFor.RenderDelay(2000)` | Milliseconds |
| `options.Scale = 1.0` | `renderer.RenderingOptions.Zoom = 100` | Percentage |
| `options.PreferCssPageSize = true` | `renderer.RenderingOptions.UseCssPageSize = true` | CSS @page rules |

### Headers and Footers

| PDFBolt | IronPDF | Notes |
|---------|---------|-------|
| `options.Header = "Header text"` | `renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter { HtmlFragment = "<div>Header</div>" }` | HTML-based |
| `options.Footer = "Footer text"` | `renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter { HtmlFragment = "<div>Footer</div>" }` | Full CSS support |
| `options.DisplayHeaderFooter = true` | Automatic when HtmlHeader/HtmlFooter set | Implicit |
| `{pageNumber}` | `{page}` | Current page |
| `{totalPages}` | `{total-pages}` | Total pages |
| `{date}` | `{date}` | Same |
| `{title}` | `{html-title}` | Document title |

### PDF Manipulation (NEW in IronPDF)

| PDFBolt | IronPDF | Notes |
|---------|---------|-------|
| _(not available)_ | `PdfDocument.Merge(pdf1, pdf2)` | Merge PDFs |
| _(not available)_ | `pdf.CopyPages(start, end)` | Extract pages |
| _(not available)_ | `pdf.RemovePages(indices)` | Delete pages |
| _(not available)_ | `pdf.InsertPdf(other, index)` | Insert PDF |
| _(not available)_ | `pdf.RotatePage(index, degrees)` | Rotate pages |
| _(not available)_ | `pdf.ApplyWatermark(html)` | Add watermarks |
| _(not available)_ | `pdf.ExtractAllText()` | Extract text |
| _(not available)_ | `pdf.RasterizeToImageFiles()` | PDF to images |
| _(not available)_ | `pdf.SecuritySettings` | Encryption |

---

## Code Migration Examples

For complete migration workflows including authentication removal and offline deployment strategies, consult the [full migration documentation](https://ironpdf.com/blog/migration-guides/migrate-from-pdfbolt-to-ironpdf/).

### Example 1: Remove API Key Pattern

**Before (PDFBolt):**
```csharp
using PDFBolt;

public class PdfService
{
    private readonly Client _client;

    public PdfService(IConfiguration config)
    {
        // API key from config - security risk if leaked
        var apiKey = config["PDFBolt:ApiKey"];
        _client = new Client(apiKey);
    }

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        try
        {
            var result = await _client.HtmlToPdf(html);
            return result.GetBytes();
        }
        catch (PDFBoltException ex) when (ex.Code == "RATE_LIMIT")
        {
            throw new Exception("Monthly limit exceeded");
        }
        catch (PDFBoltException ex) when (ex.Code == "INVALID_API_KEY")
        {
            throw new Exception("API key invalid or revoked");
        }
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
        // License key set once at startup (not per-request)
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();
    }

    public byte[] GeneratePdf(string html)
    {
        // No rate limits, no API key validation
        // No network required, no external processing
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Example 2: Async to Sync Conversion

**Before (PDFBolt):**
```csharp
using PDFBolt;

public async Task<ActionResult> GenerateInvoice(int orderId)
{
    var order = await _orderService.GetOrderAsync(orderId);
    var html = await _templateService.RenderInvoiceAsync(order);

    var client = new Client(_apiKey);
    var options = new PdfOptions
    {
        PageSize = PageSize.A4,
        MarginTop = 20,
        MarginBottom = 20
    };

    // Network round-trip to PDFBolt servers
    var result = await client.HtmlToPdf(html, options);
    var bytes = result.GetBytes();

    return File(bytes, "application/pdf", $"invoice-{orderId}.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public ActionResult GenerateInvoice(int orderId)
{
    var order = _orderService.GetOrder(orderId);
    var html = _templateService.RenderInvoice(order);

    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.MarginTop = 20;
    renderer.RenderingOptions.MarginBottom = 20;

    // Local processing - no network, no external servers
    var pdf = renderer.RenderHtmlAsPdf(html);

    return File(pdf.BinaryData, "application/pdf", $"invoice-{orderId}.pdf");
}
```

### Example 3: URL to PDF with Headers/Footers

**Before (PDFBolt):**
```csharp
using PDFBolt;
using PDFBolt.Models;

public async Task<byte[]> CaptureWebpageAsync(string url)
{
    var client = new Client(_apiKey);
    var options = new PdfOptions
    {
        PageSize = PageSize.A4,
        Orientation = Orientation.Portrait,
        DisplayHeaderFooter = true,
        Header = "Captured from: " + url,
        Footer = "Page {pageNumber} of {totalPages}",
        WaitForNetworkIdle = true,
        PrintBackground = true
    };

    var result = await client.UrlToPdf(url, options);
    return result.GetBytes();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CaptureWebpage(string url)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
    renderer.RenderingOptions.PrintHtmlBackgrounds = true;
    renderer.RenderingOptions.WaitFor.NetworkIdle();

    // HTML-based headers with full CSS support
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = $"<div style='font-size:10px; color:#666;'>Captured from: {url}</div>",
        MaxHeight = 20
    };

    // Built-in placeholders: {page}, {total-pages}, {date}, {time}
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='font-size:10px; text-align:center;'>Page {page} of {total-pages}</div>",
        MaxHeight = 20
    };

    var pdf = renderer.RenderUrlAsPdf(url);
    return pdf.BinaryData;
}
```

### Example 4: Batch Processing Without Rate Limits

**Before (PDFBolt):**
```csharp
using PDFBolt;

public async Task GenerateMonthlyReports(List<Report> reports)
{
    var client = new Client(_apiKey);
    int processed = 0;

    foreach (var report in reports)
    {
        // Check rate limit - PDFBolt free tier = 100/month
        if (processed >= 100)
        {
            throw new Exception("PDFBolt monthly limit reached!");
        }

        try
        {
            var html = RenderReport(report);
            var result = await client.HtmlToPdf(html);
            await result.SaveToFile($"report-{report.Id}.pdf");
            processed++;

            // Add delay to avoid hitting rate limits
            await Task.Delay(1000);
        }
        catch (PDFBoltException ex) when (ex.Code == "RATE_LIMIT")
        {
            throw new Exception($"Rate limited after {processed} reports");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void GenerateMonthlyReports(List<Report> reports)
{
    var renderer = new ChromePdfRenderer();

    // No rate limits, no monthly caps, no delays needed
    foreach (var report in reports)
    {
        var html = RenderReport(report);
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs($"report-{report.Id}.pdf");
    }

    // Or process in parallel for speed:
    Parallel.ForEach(reports, report =>
    {
        var localRenderer = new ChromePdfRenderer();
        var html = RenderReport(report);
        var pdf = localRenderer.RenderHtmlAsPdf(html);
        pdf.SaveAs($"report-{report.Id}.pdf");
    });
}
```

### Example 5: Error Handling Simplified

**Before (PDFBolt):**
```csharp
using PDFBolt;

public async Task<byte[]> SafeGeneratePdfAsync(string html)
{
    var client = new Client(_apiKey);

    try
    {
        var result = await client.HtmlToPdf(html);
        return result.GetBytes();
    }
    catch (PDFBoltException ex) when (ex.Code == "RATE_LIMIT")
    {
        _logger.LogError("PDFBolt rate limit exceeded");
        throw;
    }
    catch (PDFBoltException ex) when (ex.Code == "INVALID_API_KEY")
    {
        _logger.LogError("PDFBolt API key invalid");
        throw;
    }
    catch (PDFBoltException ex) when (ex.Code == "TIMEOUT")
    {
        _logger.LogError("PDFBolt request timed out");
        throw;
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError("Network error connecting to PDFBolt: {Message}", ex.Message);
        throw;
    }
    catch (TaskCanceledException)
    {
        _logger.LogError("PDFBolt request cancelled (timeout)");
        throw;
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] SafeGeneratePdf(string html)
{
    try
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
    catch (Exception ex)
    {
        // Only local processing errors - no network, no rate limits, no auth
        _logger.LogError("PDF generation error: {Message}", ex.Message);
        throw;
    }
}
```

### Example 6: PDF Merging (NEW Feature)

**Before (PDFBolt):**
```csharp
// PDFBolt doesn't support PDF merging
// You would need a separate library or service
throw new NotSupportedException("PDFBolt cannot merge PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] MergeInvoices(List<string> invoiceHtmls)
{
    var renderer = new ChromePdfRenderer();

    // Generate individual PDFs
    var pdfs = invoiceHtmls.Select(html => renderer.RenderHtmlAsPdf(html)).ToList();

    // Merge all into one document
    var merged = PdfDocument.Merge(pdfs);

    // Add page numbers to merged document
    merged.AddHtmlFooters(new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>",
        MaxHeight = 20
    });

    return merged.BinaryData;
}
```

### Example 7: Watermarks (NEW Feature)

**Before (PDFBolt):**
```csharp
// PDFBolt doesn't support watermarks
// You would need post-processing with another tool
throw new NotSupportedException("PDFBolt cannot add watermarks");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

public byte[] GenerateConfidentialDocument(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Add watermark with full CSS support
    pdf.ApplyWatermark(
        "<div style='color:red; font-size:72px; opacity:0.3; transform:rotate(-45deg);'>CONFIDENTIAL</div>",
        45,
        VerticalAlignment.Middle,
        HorizontalAlignment.Center);

    return pdf.BinaryData;
}
```

### Example 8: Security Settings (NEW Feature)

**Before (PDFBolt):**
```csharp
// PDFBolt doesn't support PDF security
// You would need post-processing with another tool
throw new NotSupportedException("PDFBolt cannot encrypt PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] GenerateSecureDocument(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Set password protection
    pdf.SecuritySettings.OwnerPassword = "admin123";
    pdf.SecuritySettings.UserPassword = "user456";

    // Control permissions
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;
    pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.NoPrint;
    pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;

    return pdf.BinaryData;
}
```

### Example 9: Text Extraction (NEW Feature)

**Before (PDFBolt):**
```csharp
// PDFBolt doesn't support text extraction
throw new NotSupportedException("PDFBolt cannot extract text");
```

**After (IronPDF):**
```csharp
using IronPdf;

public string ExtractTextFromPdf(string pdfPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    return pdf.ExtractAllText();
}

public string ExtractTextFromPage(string pdfPath, int pageIndex)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    return pdf.Pages[pageIndex].Text;
}
```

### Example 10: Offline/Air-Gapped Operation

**Before (PDFBolt):**
```csharp
// PDFBolt cannot work offline - requires internet
public async Task<byte[]> GeneratePdfOffline(string html)
{
    // Check internet connection
    if (!await IsInternetAvailable())
    {
        throw new InvalidOperationException("PDFBolt requires internet connection");
    }

    // Even with internet, PDFBolt servers might be down
    var client = new Client(_apiKey);
    return (await client.HtmlToPdf(html)).GetBytes();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] GeneratePdfOffline(string html)
{
    // Works completely offline - no internet required
    // Perfect for air-gapped environments, submarines, remote sites
    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

---

## Placeholder Mapping

| PDFBolt Placeholder | IronPDF Placeholder | Description |
|---------------------|---------------------|-------------|
| `{pageNumber}` | `{page}` | Current page number |
| `{totalPages}` | `{total-pages}` | Total page count |
| `{date}` | `{date}` | Current date |
| `{time}` | `{time}` | Current time |
| `{title}` | `{html-title}` | Document title |
| _(not available)_ | `{url}` | Source URL |

---

## Configuration Migration

### From Environment Variables

**Before (PDFBolt):**
```csharp
// Must secure API key in environment
var apiKey = Environment.GetEnvironmentVariable("PDFBOLT_API_KEY");
var client = new Client(apiKey);
```

**After (IronPDF):**
```csharp
// License key - set once at startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Or from config (no security risk like API key billing)
```

### From appsettings.json

**Before (PDFBolt):**
```json
{
  "PDFBolt": {
    "ApiKey": "pk_live_xxxxx",
    "Timeout": 30000,
    "RetryCount": 3
  }
}
```

**After (IronPDF):**
```json
{
  "IronPdf": {
    "LicenseKey": "YOUR-LICENSE-KEY"
  }
}
```

---

## Dependency Injection Migration

**Before (PDFBolt):**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<Client>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        return new Client(config["PDFBolt:ApiKey"]);
    });
}
```

**After (IronPDF):**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Set license once
    IronPdf.License.LicenseKey = Configuration["IronPdf:LicenseKey"];

    // Register renderer as transient (thread-safe creation)
    services.AddTransient<ChromePdfRenderer>();
}
```

---

## Common Migration Gotchas

### 1. Async → Sync Pattern

PDFBolt requires async for network calls. IronPDF is sync by default:

```csharp
// PDFBolt: await required
var result = await client.HtmlToPdf(html);
var bytes = result.GetBytes();

// IronPDF: No await needed
var pdf = renderer.RenderHtmlAsPdf(html);
var bytes = pdf.BinaryData;
```

### 2. No More API Key Validation

Remove all API key checking code:

```csharp
// PDFBolt: Must validate
if (string.IsNullOrEmpty(apiKey))
    throw new Exception("API key required");

// IronPDF: Remove this check entirely
```

### 3. Margin Units

Both use millimeters, but verify your values:

```csharp
// PDFBolt: Might be pixels in some versions
options.MarginTop = 20; // pixels?

// IronPDF: Always millimeters
renderer.RenderingOptions.MarginTop = 20; // mm
```

### 4. Page Number Placeholders

```csharp
// PDFBolt
"Page {pageNumber} of {totalPages}"

// IronPDF
"Page {page} of {total-pages}"
```

### 5. Remove Rate Limit Handling

Delete all rate limiting code—IronPDF has no limits:

```csharp
// PDFBolt: Rate limit handling
catch (PDFBoltException ex) when (ex.Code == "RATE_LIMIT")
{
    await Task.Delay(60000); // Wait and retry
}

// IronPDF: Delete this entirely
```

### 6. Remove Network Error Handling

Local processing means no network errors:

```csharp
// PDFBolt: Network error handling
catch (HttpRequestException ex)
catch (TaskCanceledException)
catch (TimeoutException)

// IronPDF: Remove network-specific catches
```

---

## Pre-Migration Checklist

- [ ] Inventory all PDFBolt API calls in codebase
- [ ] Document current API key management
- [ ] List all async methods that need sync conversion
- [ ] Identify rate limit handling code to remove
- [ ] Check margin units (pixels vs millimeters)
- [ ] Map placeholder syntax differences
- [ ] Identify features PDFBolt couldn't provide (merge, watermark, security)
- [ ] Plan license key deployment

---

## Post-Migration Checklist

- [ ] Remove PDFBolt NuGet package
- [ ] Delete API key from configuration files
- [ ] Remove API key from secret managers
- [ ] Convert async methods to sync
- [ ] Remove rate limit handling code
- [ ] Remove network error handling
- [ ] Test all PDF generation paths
- [ ] Verify placeholder syntax in headers/footers
- [ ] Add new features (watermarks, security) as needed
- [ ] Update documentation

---

## Finding PDFBolt References

```bash
# Find all PDFBolt usage
grep -r "PDFBolt\|Client\|HtmlToPdf\|UrlToPdf\|PdfOptions" --include="*.cs" .

# Find API key references
grep -r "PDFBOLT\|ApiKey\|api.key\|apiKey" --include="*.cs" --include="*.json" --include="*.config" .

# Find async patterns to convert
grep -r "await.*client\.\|await.*HtmlToPdf\|await.*UrlToPdf" --include="*.cs" .
```

---

## Performance Comparison

| Metric | PDFBolt | IronPDF |
|--------|---------|---------|
| Simple HTML (1 page) | 2-5 seconds (network) | 100-300ms (local) |
| Complex HTML (10 pages) | 5-15 seconds | 500ms-2s |
| Batch (100 documents) | Rate limited | No limits |
| Offline operation | Impossible | Full support |
| First request | 3-8 seconds (cold start) | 500ms (engine init) |

---

## Feature Comparison Summary

| Feature | PDFBolt | IronPDF |
|---------|---------|---------|
| HTML to PDF | ✓ | ✓ |
| URL to PDF | ✓ | ✓ |
| Headers/Footers | ✓ (text) | ✓ (full HTML) |
| Page Numbers | ✓ | ✓ |
| Custom Page Sizes | ✓ | ✓ |
| Margins | ✓ | ✓ |
| PDF Merging | ✗ | ✓ |
| PDF Splitting | ✗ | ✓ |
| Watermarks | ✗ | ✓ |
| Password Protection | ✗ | ✓ |
| Text Extraction | ✗ | ✓ |
| PDF to Images | ✗ | ✓ |
| Form Filling | ✗ | ✓ |
| Digital Signatures | ✗ | ✓ |
| Offline Operation | ✗ | ✓ |
| Unlimited Processing | ✗ | ✓ |
| Data Privacy | ✗ (cloud) | ✓ (local) |

---

## Troubleshooting

### "License key required for production"

```csharp
// Set license key before any PDF operations
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Free for development, license for production
```

### "First PDF generation is slow"

The Chromium engine initializes on first use. Pre-warm if needed:

```csharp
// Warm up during application startup
var renderer = new ChromePdfRenderer();
renderer.RenderHtmlAsPdf("<html><body></body></html>");
```

### "Async methods no longer await"

IronPDF methods are synchronous. Remove await keywords:

```csharp
// Old: var result = await client.HtmlToPdf(html);
// New: var pdf = renderer.RenderHtmlAsPdf(html);
```

### "Rate limit code throws compile errors"

Remove all PDFBolt-specific exception handling:

```csharp
// Delete: catch (PDFBoltException ex) when (ex.Code == "RATE_LIMIT")
// IronPDF has no rate limits
```

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PDFBolt usages in codebase**
  ```bash
  grep -r "using PDFBolt" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package PDFBolt
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

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Tutorial](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [API Reference](https://ironpdf.com/object-reference/api/)
