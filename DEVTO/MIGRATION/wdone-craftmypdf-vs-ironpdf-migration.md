# Migrating from CraftMyPDF to IronPDF: a practical .NET migration guide

---

Migrating off a cloud PDF service is architecturally different from migrating between two .NET libraries. You're not swapping a NuGet package — you're moving from an HTTP dependency to a local process, which changes your latency profile, cost model, data residency posture, and offline availability.

This article gives you the checklists and code to move from **CraftMyPDF** (cloud API) to **IronPDF** (local .NET library). The migration is worth doing when network latency matters, when data leaving your environment is a compliance concern, or when per-API-call cost adds up at scale. It's less clearly worth it if templates change frequently and you value CraftMyPDF's visual editor.

Work through the checklists in order. The pre-migration phase matters here more than in library-to-library migrations because you're changing the fundamental execution model.

---

## Why Migrate (Without Drama)

CraftMyPDF solves a real problem — PDF generation without running PDF infrastructure. The friction appears when requirements change:

1. **Data residency / compliance** — PDF content (customer PII, financial data) leaves your network with every API call. Some compliance frameworks prohibit or complicate this.
2. **Network latency** — Each PDF generation round-trips to CraftMyPDF's servers. For low-latency or high-volume scenarios, this adds measurable delay.
3. **API cost at scale** — Cloud services price per operation. Above a threshold, local rendering is cheaper. Calculate your current API call volume × rate.
4. **Offline / air-gapped environments** — CraftMyPDF requires internet access. Air-gapped servers, offline modes, or edge deployments can't use cloud APIs.
5. **Vendor dependency risk** — API changes, service outages, or pricing changes from a third-party affect your application.
6. **Template flexibility** — CraftMyPDF templates are managed in their UI. Complex dynamic logic (conditional sections, loops, computed fields) requires their template language rather than full C# control flow.
7. **CI/CD test environment** — Integration tests that hit a live cloud API are slower and fragile. Local rendering makes tests hermetic.
8. **Response time SLA** — If PDF generation is in a synchronous request path, cloud round-trip adds to your p99 latency.
9. **Custom fonts and assets** — Managing font assets in a cloud template system can be less flexible than local filesystem access.
10. **On-premises deployment requirements** — Some enterprise environments prohibit cloud service dependencies in production workloads.

### Comparison Table

| Aspect | CraftMyPDF | IronPDF |
|---|---|---|
| Focus | Cloud template PDF generation | Local HTML-to-PDF, edit, merge, security |
| Pricing | Per-API-call or subscription | Per-developer or royalty-free |
| API Style | REST/HTTP with JSON template data | .NET method calls |
| Learning Curve | Simple for template-driven; limited for complex logic | Gradual; HTML/CSS knowledge transfers directly |
| HTML Rendering | Verify CraftMyPDF renderer | Chromium-based |
| Page Indexing | N/A (cloud output) | 0-based |
| Thread Safety | N/A (stateless HTTP) | Renderer reusable; verify thread safety docs |
| Namespace | HttpClient (no local SDK) | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Effort | Notes |
|---|---|---|
| Simple HTML-to-PDF | Low | Replace HTTP call with local renderer |
| Template-driven PDF (data injection) | Medium | Replace JSON template with HTML + C# string interpolation or Razor |
| Merge PDFs | Medium | CraftMyPDF may handle this server-side; need local merge code |
| Watermark | Medium | New capability to implement locally |
| Password protection | Low | New capability; simple API |
| Complex conditional templates | High | Rebuild logic in C# / Razor |
| Batch generation at scale | Medium | Test local throughput vs API throughput |
| Test environment hermeticity | Low | Tests no longer need network access |
| Error handling model | Medium | HTTP errors → .NET exceptions |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Data residency is a compliance requirement | Local rendering strongly worth evaluating |
| API cost is significant and growing | Calculate break-even point; local often cheaper at scale |
| Offline or air-gapped deployment | Cloud API is not viable; local rendering required |
| Template managed by non-developers | CraftMyPDF's UI is a genuine advantage; evaluate migration cost |

---

## Pre-Migration Checklist

Complete every item before writing migration code.

### Understand Current Usage
- [ ] List all CraftMyPDF API endpoints your app calls (typically `create-pdf`, `create-image`, `download`, others)
- [ ] Count monthly API calls — check your CraftMyPDF dashboard
- [ ] Identify all templates in CraftMyPDF — screenshot or export their layouts
- [ ] Document each template: what data fields does it accept? What's the output format?
- [ ] Identify which templates have complex logic (conditionals, loops, computed values)
- [ ] Note any custom fonts or image assets uploaded to CraftMyPDF
- [ ] Document current average response times for API calls (check APM or logs)

### Compliance and Data Review
- [ ] Identify what data is sent in API payloads (PII, financial, health data?)
- [ ] Confirm data residency requirements with your compliance/security team
- [ ] Document current data handling — does CraftMyPDF retain data? Check their privacy policy.
- [ ] If data residency is a driver, confirm IronPDF processes all data locally (it does — verify current docs)

### Cost Analysis
- [ ] Export current monthly API call volume from CraftMyPDF dashboard
- [ ] Calculate current monthly API cost
- [ ] Estimate IronPDF license cost for your developer count
- [ ] Estimate hosting cost if scaling PDF generation (CPU/memory for Chromium renderer)
- [ ] Calculate break-even point: (IronPDF license + infra) vs CraftMyPDF API cost

### Technical Inventory
- [ ] Find all `HttpClient` calls to CraftMyPDF API: `rg "craftmypdf\|craftmypdf\.com" --type cs -l`
- [ ] Find all template IDs referenced in code: `rg "template_id\|templateId" --type cs`
- [ ] Find all JSON data structures sent to CraftMyPDF (these become your HTML template variables)
- [ ] Identify the deployment environment (Windows, Linux, cloud) for IronPDF compatibility check
- [ ] Confirm .NET version: `dotnet --version`

---

## Setup Checklist

### Install IronPDF
- [ ] Add package: `dotnet add package IronPdf`
- [ ] Restore: `dotnet restore`
- [ ] Set license key in config (never hardcode in source):
  ```bash
  # appsettings.json or environment variable
  # IronPdf__LicenseKey or IRONPDF_LICENSE_KEY
  ```
- [ ] Configure license in Program.cs / Startup.cs:
  ```csharp
  IronPdf.License.LicenseKey = builder.Configuration["IronPdf:LicenseKey"];
  ```
- [ ] Verify license: `Console.WriteLine(IronPdf.License.IsValidLicense);`

### Linux / Container Setup (if applicable)
- [ ] Install native dependencies):
  ```dockerfile
  RUN apt-get update && apt-get install -y \
      libgdiplus libx11-6 libc6 libgtk-3-0 libxcomposite1 libxdamage1 \
      libxrandr2 libgbm1 libxkbcommon0 libasound2
  ```
- [ ] Test PDF generation in target container before writing full migration

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

**Before (CraftMyPDF — HTTP call pattern):**

```csharp
// No local license — auth via API key in HTTP headers
private const string CRAFTMYPDF_API_KEY = "YOUR-CRAFTMYPDF-KEY";
private const string CRAFTMYPDF_BASE_URL = "https://api.craftmypdf.com/v1/";

var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-API-KEY", CRAFTMYPDF_API_KEY);
```

**After (IronPDF):**

```csharp
using IronPdf;

// Set once at startup — https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");
```

### Step 2 — Remove HTTP Client PDF Generation

**Before (CraftMyPDF request pattern):**
```csharp
using System.Net.Http.Json;

// Data model matching CraftMyPDF template fields
var payload = new
{
    template_id = "YOUR-TEMPLATE-ID",
    data = new { name = "Alice", amount = 1200, due = "2025-12-01" }
};

var response = await httpClient.PostAsJsonAsync($"{CRAFTMYPDF_BASE_URL}create-pdf", payload);
response.EnsureSuccessStatusCode();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf(GetInvoiceHtml("Alice", 1200, "2025-12-01"));
```

### Step 3 — Template Conversion Pattern

Replace CraftMyPDF JSON templates with C# HTML generation:

```csharp
// Template method replacing CraftMyPDF template ID
static string GetInvoiceHtml(string name, decimal amount, string due) => $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        h1 {{ color: #2563EB; }}
        .amount {{ font-size: 24px; font-weight: bold; }}
    </style>
</head>
<body>
    <h1>Invoice for {System.Net.WebUtility.HtmlEncode(name)}</h1>
    <p class='amount'>Amount: ${amount:F2}</p>
    <p>Due: {due}</p>
</body>
</html>";
```

For Razor-based applications, use partial views instead of string interpolation.

---

## Code Migration Checklist

- [ ] Replace `HttpClient` CraftMyPDF calls with `ChromePdfRenderer.RenderHtmlAsPdf()`
- [ ] Create HTML template methods for each CraftMyPDF template ID in use
- [ ] Transfer custom fonts from CraftMyPDF to local filesystem or embedded resources
- [ ] Replace CraftMyPDF image assets with local paths or embedded base64 in HTML
- [ ] Implement error handling: HTTP errors become `try/catch` on IronPDF calls
- [ ] Remove CraftMyPDF API key from configuration/secrets
- [ ] Remove `HttpClient` setup code specific to CraftMyPDF
- [ ] Replace any CraftMyPDF merge API calls with `PdfDocument.Merge()`
- [ ] Implement local watermarking if CraftMyPDF templates included watermarks
- [ ] Implement local password protection if required
- [ ] Remove CraftMyPDF SDK/wrapper NuGet package if one was used

---

## Four Complete Before/After Migrations

### 1. HTML to PDF (API Call → Local Render)

**Before (CraftMyPDF API call):**

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient _http = new HttpClient();
    private const string API_KEY = "YOUR-CRAFTMYPDF-KEY";
    private const string BASE_URL = "https://api.craftmypdf.com/v1/";

    static async Task Main()
    {
        _http.DefaultRequestHeaders.Add("X-API-KEY", API_KEY);

        // Template ID and data sent to CraftMyPDF cloud
        var payload = new
        {
            template_id = "abc123",  // your template ID
            data = new
            {
                customer_name = "Alice Smith",
                invoice_number = "INV-2071",
                amount = 1200.00,
                due_date = "2025-12-01"
            },
            export_type = "json",
            expiry = 10
        };

        // Round-trip to CraftMyPDF servers
        var response = await _http.PostAsJsonAsync($"{BASE_URL}create-pdf", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CraftMyPdfResult>();
        // Download the PDF from their CDN
        var pdfBytes = await _http.GetByteArrayAsync(result.file);
        await System.IO.File.WriteAllBytesAsync("invoice.pdf", pdfBytes);
        Console.WriteLine("Invoice downloaded from CraftMyPDF");
    }
}

record CraftMyPdfResult(string file, string status);
```

**After (IronPDF local render):**

```csharp
using System;
using IronPdf;

class Program
{
    // Reuse renderer — don't create per-request
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-KEY";

        // No network call — renders locally with Chromium
        // https://ironpdf.com/how-to/html-string-to-pdf/
        string html = BuildInvoiceHtml("Alice Smith", "INV-2071", 1200.00m, "2025-12-01");
        using var pdf = _renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("invoice.pdf");
        Console.WriteLine("Invoice generated locally");
    }

    static string BuildInvoiceHtml(string name, string invoiceNum, decimal amount, string due) =>
        $@"<!DOCTYPE html><html><body style='font-family:Arial;padding:40px'>
            <h1>Invoice {System.Net.WebUtility.HtmlEncode(invoiceNum)}</h1>
            <p>Customer: {System.Net.WebUtility.HtmlEncode(name)}</p>
            <p>Amount: ${amount:F2}</p>
            <p>Due: {due}</p>
        </body></html>";
}
```

### 2. Merge PDFs

**Before (CraftMyPDF — verify if merge is supported via API):**

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient _http = new HttpClient();

    static async Task Main()
    {
        _http.DefaultRequestHeaders.Add("X-API-KEY", "YOUR-KEY");

        // CraftMyPDF may not have a direct merge API — verify
        // Typically you generate each PDF separately then merge client-side
        // or use their batch API — verify current API docs at craftmypdf.com

        var pdf1Bytes = await GeneratePdfFromTemplate("template_a", new { section = 1 });
        var pdf2Bytes = await GeneratePdfFromTemplate("template_b", new { section = 2 });

        // Manual merge: save both, then merge with another tool — or:
        // CraftMyPDF doesn't provide a merge endpoint (verify in docs)
        Console.WriteLine("Merge requires additional tooling with CraftMyPDF");
    }

    static async Task<byte[]> GeneratePdfFromTemplate(string templateId, object data)
    {
        var payload = new { template_id = templateId, data, export_type = "json", expiry = 10 };
        var response = await _http.PostAsJsonAsync("https://api.craftmypdf.com/v1/create-pdf", payload);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CraftMyPdfResult>();
        return await _http.GetByteArrayAsync(result.file);
    }
}

record CraftMyPdfResult(string file, string status);
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-KEY";

        // https://ironpdf.com/how-to/merge-or-split-pdfs/
        var pdf1 = PdfDocument.FromFile("section1.pdf");
        var pdf2 = PdfDocument.FromFile("section2.pdf");

        using var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
        Console.WriteLine($"Merged: {merged.PageCount} pages");
    }
}
```

### 3. Watermark

**Before (CraftMyPDF — template-side watermark):**

```csharp
// With CraftMyPDF, watermarks are typically configured in the template editor
// — not added via API. To add a watermark you edit the template in their UI.
// For dynamic watermarks (e.g., per-customer "DRAFT"), you inject a watermark
// variable into the template data payload:

var payload = new
{
    template_id = "abc123",
    data = new
    {
        customer_name = "Alice",
        watermark_text = "DRAFT"  // template must have a watermark element
    },
    export_type = "json",
    expiry = 10
};
// The template designer in CraftMyPDF UI handles the positioning and opacity
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        // Full control over watermark properties — https://ironpdf.com/how-to/stamp-text-image/
        var stamper = new TextStamper
        {
            Text = "DRAFT",
            FontSize = 72,
            Opacity = 30,
            Rotation = 45,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        pdf.ApplyStamp(stamper);
        pdf.SaveAs("watermarked.pdf");
        Console.WriteLine("Watermarked PDF saved");
    }
}
```

### 4. Password Protection

**Before (CraftMyPDF — check if encryption is supported in API):**

```csharp
// CraftMyPDF password protection support — verify in current API docs
// Some PDF generation SaaS services don't support client-specified encryption
// If CraftMyPDF doesn't support it, you may already be encrypting post-download
// using a separate library — that separate library code becomes your before state

// If currently doing post-download encryption with another library, migrate that:
// var pdfBytes = await DownloadFromCraftMyPdf(...);
// [existing encryption code here]
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-KEY";

        // Generate or load the PDF
        var renderer = new ChromePdfRenderer();
        using var pdf = renderer.RenderHtmlAsPdf("<html><body><p>Confidential</p></body></html>");

        // Encrypt in the same step — https://ironpdf.com/how-to/pdf-permissions-passwords/
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting =
            IronPdf.Security.PdfPrintSecurity.FullPrintRights;

        pdf.SaveAs("protected.pdf");
        Console.WriteLine("Encrypted PDF saved");
    }
}
```

---

## API Mapping Tables

### Namespace Mapping

| CraftMyPDF (HTTP) | IronPDF | Notes |
|---|---|---|
| `HttpClient` + API key header | `IronPdf.License.LicenseKey` | No HTTP client needed |
| `application/json` POST body | Method parameters | Data becomes HTML template inputs |
| Download URL from response | `pdf.BinaryData` or `pdf.SaveAs()` | Local output |

### Core Operation Mapping

| CraftMyPDF Operation | IronPDF Equivalent | Description |
|---|---|---|
| `POST /create-pdf` with template_id | `renderer.RenderHtmlAsPdf(html)` | Generate PDF |
| Template UI design | HTML + CSS string or Razor view | Template format |
| JSON `data` field injection | C# string interpolation or Razor model | Data binding |
| Download from CDN URL | `pdf.BinaryData` or `pdf.Stream` | PDF retrieval |

### Document Operations

| Operation | CraftMyPDF | IronPDF |
|---|---|---|
| Generate from template | API call with template_id + data | `renderer.RenderHtmlAsPdf(html)` |
| Merge documents | Not natively | `PdfDocument.Merge(pdf1, pdf2)` |
| Add watermark | Template editor + data injection | `pdf.ApplyStamp(stamper)` |
| Password protect | Verify API support | `pdf.SecuritySettings.*` |

---

## Critical Migration Notes

### Error Handling Model Change

Cloud APIs fail with HTTP status codes and JSON error bodies. Local library calls throw .NET exceptions. Update your error handling pattern:

```csharp
// Before — HTTP error handling
var response = await _http.PostAsJsonAsync(url, payload);
if (!response.IsSuccessStatusCode)
{
    var error = await response.Content.ReadAsStringAsync();
    throw new Exception($"CraftMyPDF API error {(int)response.StatusCode}: {error}");
}

// After — exception handling
try
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
catch (Exception ex)
{
    _logger.LogError(ex, "PDF generation failed");
    throw;
}
```

### Template Conversion Strategy

Don't try to migrate all templates at once. Work template-by-template:

1. Screenshot the CraftMyPDF template output
2. Write an HTML equivalent
3. Render with IronPDF
4. Visual diff against the screenshot
5. Adjust CSS until output matches
6. Move to next template

### Font Migration

Fonts uploaded to CraftMyPDF need to be available locally:

```html
<!-- Reference local font in HTML template -->
<style>
    @font-face {
        font-family: 'CustomFont';
        src: url('/fonts/custom-font.woff2') format('woff2');
    }
    body { font-family: 'CustomFont', Arial, sans-serif; }
</style>
```

Or use system fonts / Google Fonts.

### Latency Profile Change

CraftMyPDF calls are async HTTP — they fit naturally into async code paths. IronPDF rendering is CPU-bound. The first render after creating a new `ChromePdfRenderer` has startup overhead. Subsequent renders are faster. Account for this in your p99 latency measurements.

```csharp
// Use async rendering to avoid blocking threads
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

See [async docs](https://ironpdf.com/how-to/async/) for patterns.

---

## Testing Checklist

- [ ] Visual comparison: IronPDF output vs CraftMyPDF output for each template (screenshot comparison)
- [ ] Font rendering: custom fonts display correctly
- [ ] Image rendering: embedded or referenced images appear correctly
- [ ] Page size and margins match original templates
- [ ] Multi-page documents paginate correctly (check page breaks)
- [ ] Special characters and Unicode render correctly
- [ ] Test with production-representative data (not just placeholder data)
- [ ] Test in target deployment environment (Windows server, Linux container, etc.)
- [ ] Measure latency under concurrent load — verify against current SLA requirements
- [ ] Password-protected PDFs open with correct credentials

---

## Post-Migration Checklist

- [ ] Remove CraftMyPDF API key from all configuration files and secrets stores
- [ ] Remove CraftMyPDF API key from CI/CD environment variables
- [ ] Delete or archive CraftMyPDF templates (download PDFs of each as reference)
- [ ] Update runbooks documenting PDF generation dependency (remove cloud API dependency)

---

## Conclusion

Moving from a cloud PDF API to a local library is one of the less common migration patterns in .NET, but it comes up whenever data residency, offline requirements, or cost-at-scale push teams to bring generation in-house. The template conversion is the most time-consuming step — budget roughly one working day per complex template for visual regression testing.

The payoff is removing a network dependency from a synchronous code path, which tends to show up clearly in p99 latency numbers once it's gone.

**Question for the comments:** For teams that moved from cloud PDF APIs to local generation — how did you handle the visual regression testing? Pixel diff tooling, manual review, or something else? And how many template conversion cycles did it take to get acceptable output?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
