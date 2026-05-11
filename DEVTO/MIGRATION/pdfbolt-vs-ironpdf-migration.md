---
title: "The PDFBolt to IronPDF migration nobody dramatised"
published: false
tags: dotnet, csharp, pdf, migration
---

You opened the PDFBolt documentation looking for PDF/A compliance export and it isn't there. Your legal team needs archival-format output for a document retention workflow, and the library you've been using for HTML-to-PDF generation simply doesn't support the standard. Adding a second library to post-process the output is one option — or you evaluate whether the primary library should be replaced.

This article covers the migration from PDFBolt to IronPDF. The API mapping tables, checklist, and before/after code are useful reference material regardless of which library you land on.

---

## Why migrate (without drama)

Nine neutral triggers for this migration:

1. **Missing PDF/A compliance** — archival document standards (PDF/A-1b, PDF/A-3b) require specific conformance. If the library doesn't support them, a second library or migration is necessary.
2. **Cloud API dependency** — PDFBolt is a SaaS REST API. Every PDF generation makes an outbound HTTP call to `api.pdfbolt.com`. Latency, rate limits, and network availability all affect your render pipeline.
3. **Data residency** — HTML content sent to a cloud API leaves your infrastructure. Some compliance frameworks prohibit this for sensitive documents.
4. **Rate limiting at scale** — cloud APIs have tier-based limits. PDFBolt's free tier is capped at 100 documents per month, 20 requests per minute, and one concurrent request; paid tiers scale from $19/mo to $249/mo (pricing per pdfbolt.com).
5. **Offline / air-gapped environments** — cannot call a cloud API from an isolated environment.
6. **Feature gaps** — merge, split, watermark, digital signature, form filling, and text extraction are not part of PDFBolt's surface; you supplement with a second library.
7. **API key management** — cloud keys in CI/CD pipelines and container environments add secrets management overhead.
8. **Cost structure** — at volume, per-render billing vs. per-developer licensing may look different on a spreadsheet. Run the numbers for your workload.
9. **Vendor dependency** — API contract changes, endpoint deprecations, or service outages affect your rendering pipeline when you're not in control.

### Comparison table

| Aspect | PDFBolt | IronPDF |
|---|---|---|
| Focus | Cloud HTML-to-PDF REST API | HTML-to-PDF + PDF manipulation |
| Pricing | $19–$249/mo tiered subscription | Commercial — see ironsoftware.com |
| Integration model | REST API (no .NET SDK) | In-process .NET library |
| Learning curve | Low (simple JSON payloads) | Medium |
| HTML rendering | Cloud renderer (Chrome headless) | Chromium-based local |
| Page indexing | N/A (cloud — no page model exposed) | 0-based |
| Thread safety | API tier limits apply | Renderer instance reuse |
| Namespace | None (HttpClient-based) | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | PDFBolt approach | Effort to migrate |
|---|---|---|
| HTML string to PDF | `POST /v1/direct` with base64 HTML | Low |
| URL to PDF | `POST /v1/direct` with `url` field | Low |
| Merge PDFs | Not supported — second library | Low (native in IronPDF) |
| Watermark | Not supported — second library | Low |
| Password protection | Not supported — second library | Low |
| PDF/A compliance | Not supported | Low in IronPDF |
| Digital signatures | Not supported | Medium |
| Custom headers/footers | `headerTemplate`/`footerTemplate` (base64) | Medium |
| Async/batch generation | API-level queuing (`/v1/async`) | Medium |
| Offline rendering | Not supported (cloud dependency) | Resolved — in-process |
| Data residency compliance | Data leaves infra | Resolved — local render |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| PDF/A compliance required | PDFBolt does not support; IronPDF has it natively |
| Data residency constraint | Cloud API is incompatible; local renderer required |
| High-volume concurrent rendering | Evaluate rate limits vs in-process cost; benchmark both |
| Simple HTML-to-PDF, no compliance needs | PDFBolt may suffice if working; evaluate migration ROI |

---

## Before you start

### Prerequisites

- .NET 6+ target
- NuGet access
- All HTML templates for render comparison
- PDFBolt API key (keep for reference while migrating)

### Find PDFBolt references in your codebase

```bash
# Find all PDFBolt REST calls and API key usage
rg "api\.pdfbolt\.com|API-KEY|/v1/(direct|sync|async)" --type cs -n

# Find any hand-rolled wrapper classes around the REST API
rg "PdfBolt|pdfbolt" --type cs -n -i

# Find API key references in config and code
rg "PDFBOLT|PdfBoltKey|pdfbolt.*key" --type cs --type json -n -i

# Find PostAsync calls hitting the PDFBolt endpoint
rg "PostAsync\(.*pdfbolt|api\.pdfbolt\.com" --type cs -n
```

### Remove PDFBolt integration, install IronPDF

PDFBolt does not publish a NuGet package — there is no PDFBolt SDK to uninstall. Integration is via `HttpClient` against the documented REST endpoints. To migrate, delete your hand-rolled HTTP wrapper code and install IronPDF:

```bash
# Install IronPDF (replaces the hand-rolled PDFBolt HttpClient code)
dotnet add package IronPdf
dotnet restore
```

On Windows via Package Manager Console: `Install-Package IronPdf`.

---

## Quick start migration (3 steps)

### Step 1: API key to license key

**Before (PDFBolt — API key sent on every HTTP request):**
```csharp
using System.Net.Http;

// API key from config — sent as a header with each request
var apiKey = Environment.GetEnvironmentVariable("PDFBOLT_API_KEY");

var http = new HttpClient();
http.DefaultRequestHeaders.Add("API-KEY", apiKey);
// No in-process license — every request is authenticated over the wire
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// License guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before (PDFBolt — no SDK, just standard HTTP/JSON types):**
```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
using IronPdf.Security;
```

### Step 3: Basic HTML-to-PDF

**Before (PDFBolt — base64 HTML in JSON to `/v1/direct`):**
```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class BasicConversionExample
{
    static async Task Main()
    {
        var html = "<h1>Hello World</h1>";
        var base64Html = Convert.ToBase64String(Encoding.UTF8.GetBytes(html));

        var payload = JsonSerializer.Serialize(new { html = base64Html });

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("API-KEY", "YOUR-PDFBOLT-API-KEY");

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await http.PostAsync(
            "https://api.pdfbolt.com/v1/direct", content);
        response.EnsureSuccessStatusCode();

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync("output.pdf", pdfBytes);
    }
}
```

**After (IronPDF — local, synchronous + async available):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API mapping tables

### Integration mapping

| PDFBolt (REST) | IronPDF | Notes |
|---|---|---|
| `HttpClient` + `API-KEY` header | `new ChromePdfRenderer()` | No HTTP, no API key per request |
| JSON request body | `renderer.RenderingOptions` | Strongly-typed config |
| `POST https://api.pdfbolt.com/v1/direct` | `renderer.RenderHtmlAsPdf(...)` | Local Chromium |
| `await response.Content.ReadAsByteArrayAsync()` | `pdf.BinaryData` / `pdf.SaveAs(path)` | Document object |

### Conversion patterns

| PDFBolt (REST) | IronPDF |
|---|---|
| `POST /v1/direct` with `{ "html": <base64> }` | `renderer.RenderHtmlAsPdf(html)` |
| `POST /v1/direct` with `{ "url": "..." }` | `renderer.RenderUrlAsPdf(url)` |
| Read file, base64-encode, POST | `renderer.RenderHtmlFileAsPdf(path)` |
| N/A (cloud — no PDF load) | `PdfDocument.FromFile(path)` |

### Page configuration

| PDFBolt JSON field | IronPDF |
|---|---|
| `"format": "A4"` | `ChromePdfRenderOptions.PaperSize = PdfPaperSize.A4` |
| `"margin": { "top": "20mm" }` | `ChromePdfRenderOptions.MarginTop = 20` (mm, numeric) |
| `"landscape": true` | `ChromePdfRenderOptions.PaperOrientation = PdfPaperOrientation.Landscape` |
| `"printBackground": true` | `ChromePdfRenderOptions.PrintHtmlBackgrounds = true` |
| `"scale": 1.0` | `ChromePdfRenderOptions.Zoom = 100` |
| `"waitUntil": "networkidle0"` | `ChromePdfRenderOptions.WaitFor.NetworkIdle()` |

### Merge/split operations

| Operation | PDFBolt | IronPDF |
|---|---|---|
| Merge | Not in API surface | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | Not in API surface | `pdf.CopyPages(startIndex, endIndex)` |

### Header/footer placeholders

PDFBolt's headers/footers are base64-encoded HTML inside `headerTemplate`/`footerTemplate`, with Chrome-headless class spans for tokens. IronPDF uses curly-brace tokens inside `HtmlFragment`.

| PDFBolt placeholder | IronPDF placeholder |
|---|---|
| `<span class="pageNumber"></span>` | `{page}` |
| `<span class="totalPages"></span>` | `{total-pages}` |
| `<span class="date"></span>` | `{date}` |
| `<span class="title"></span>` | `{html-title}` |
| `<span class="url"></span>` | `{url}` |

---

## Four complete before/after migrations

### 1. HTML to PDF

**Before (PDFBolt — REST POST with base64 HTML):**
```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class HtmlToPdfExample
{
    static async Task Main()
    {
        var html = "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>";
        var base64Html = Convert.ToBase64String(Encoding.UTF8.GetBytes(html));

        var payload = JsonSerializer.Serialize(new
        {
            html = base64Html,
            format = "A4",
            margin = new { top = "10mm" }
        });

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("API-KEY", "YOUR-PDFBOLT-API-KEY");

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await http.PostAsync(
            "https://api.pdfbolt.com/v1/direct", content);
        response.EnsureSuccessStatusCode();

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync("invoice.pdf", pdfBytes);
        Console.WriteLine("Saved: invoice.pdf");
    }
}
```

**After (IronPDF — local, no HTTP):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize  = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop  = 10;

var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>"
);
pdf.SaveAs("invoice.pdf");
// Full guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (PDFBolt does not expose merge — second library required):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;

class MergePdfsExample
{
    static void Main()
    {
        // PDFBolt's REST surface does not include merge — supplement with PdfSharp
        using var output = new PdfDocument();

        foreach (string path in new[] { "part1.pdf", "part2.pdf" })
        {
            using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            foreach (PdfPage page in input.Pages)
                output.AddPage(page);
        }

        output.Save("merged.pdf");
        Console.WriteLine("Merged to: merged.pdf");
    }
}
```

**After (IronPDF native):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var merged = PdfDocument.Merge(
    PdfDocument.FromFile("part1.pdf"),
    PdfDocument.FromFile("part2.pdf")
);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (PDFBolt does not expose watermark — second library required):**
```csharp
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;

class WatermarkExample
{
    static void Main()
    {
        // PDFBolt's REST surface does not include watermark — supplement with iTextSharp
        using var reader  = new PdfReader("generated.pdf");
        using var fs      = new FileStream("watermarked.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs);

        var font = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
        for (int i = 1; i <= reader.NumberOfPages; i++)
        {
            var cb = stamper.GetOverContent(i);
            cb.SaveState();
            cb.SetGState(new PdfGState { FillOpacity = 0.3f });
            cb.BeginText();
            cb.SetFontAndSize(font, 60);
            cb.SetColorFill(BaseColor.GRAY);
            cb.ShowTextAligned(Element.ALIGN_CENTER, "DRAFT", 300, 420, 45);
            cb.EndText();
            cb.RestoreState();
        }
        Console.WriteLine("Watermarked.");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("generated.pdf");
var stamper = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronSoftware.Drawing.Color.Gray,
    FontSize = 60,
    Opacity = 30,
    Rotation = 45,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};
pdf.ApplyStamp(stamper);
pdf.SaveAs("watermarked.pdf");
// Guide: https://ironpdf.com/how-to/custom-watermark/
```

---

### 4. Password protection

**Before (PDFBolt does not expose encryption — second library required):**
```csharp
using iTextSharp.text.pdf;
using System.IO;
using System.Text;

class SecurityExample
{
    static void Main()
    {
        // PDFBolt's REST surface does not include encryption — supplement with iTextSharp
        byte[] userPass  = Encoding.ASCII.GetBytes("readpass");
        byte[] ownerPass = Encoding.ASCII.GetBytes("adminpass");

        using var reader  = new PdfReader("generated.pdf");
        using var fs      = new FileStream("secured.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs, '\0', false);
        stamper.SetEncryption(
            userPass, ownerPass,
            PdfWriter.ALLOW_PRINTING,
            PdfWriter.ENCRYPTION_AES_128
        );
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Security;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("generated.pdf");
pdf.SecuritySettings.UserPassword  = "readpass";
pdf.SecuritySettings.OwnerPassword = "adminpass";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Architecture shift: cloud REST API to in-process

PDFBolt and IronPDF are fundamentally different architecturally:

```csharp
// Before: async HTTP call with network latency and potential failure modes
// using var response = await http.PostAsync(
//     "https://api.pdfbolt.com/v1/direct", content); // network hop

// After: in-process, synchronous or async, no network dependency
var pdf = renderer.RenderHtmlAsPdf(html); // local render

// Retry/timeout logic for HTTP failures and 429 quota responses
// is no longer applicable. Circuit-breaker patterns around the
// cloud endpoint can be removed.
```

Remove HTTP client retry, timeout, and circuit breaker logic that was protecting against API call failures. Replace with exception handling for local render errors.

### Async pattern migration

PDFBolt is naturally async because it is a network call. IronPDF offers both sync and async:

```csharp
// Async option (recommended for web applications):
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
// Async guide: https://ironpdf.com/how-to/async/
```

### PDF/A output

The feature gap that opened this migration — PDF/A — is available in IronPDF:

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Archived Document</h1>");

// Export as PDF/A for long-term archiving
pdf.SaveAsPdfA("archived.pdf", IronPdf.PdfAVersions.PdfA3b);
// PDF/A guide: https://ironpdf.com/how-to/pdfa/
```

### Page indexing

PDFBolt returns a flat PDF byte stream over HTTP — there is no in-process page model to index against. IronPDF uses 0-based page indexing for post-render manipulation:

```csharp
var firstPage = pdf.Pages[0]; // 0-based
var lastPage  = pdf.Pages[pdf.PageCount - 1];
```

### Margin units

PDFBolt accepts CSS-style strings (`"20mm"`, `"1in"`) in the JSON body. IronPDF margins are millimeters as a numeric property:

```csharp
// PDFBolt JSON body
// margin = new { top = "20mm" }

// IronPDF
renderer.RenderingOptions.MarginTop = 20; // mm, as a number
```

### Header/footer placeholders

PDFBolt's `headerTemplate`/`footerTemplate` values are base64-encoded HTML whose page-number tokens are Chrome-headless class spans. IronPDF uses curly-brace tokens inside `HtmlFragment`:

```csharp
// PDFBolt (decoded headerTemplate body)
// "Page <span class=\"pageNumber\"></span> of <span class=\"totalPages\"></span>"

// IronPDF
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "Page {page} of {total-pages}"
};
```

---

## Performance considerations

### Eliminate network latency

Cloud API rendering includes HTTP round-trip overhead. In-process rendering eliminates this:

```csharp
// Reuse renderer for batch work — amortizes Chromium initialization
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

foreach (var html in templateBatch)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{Guid.NewGuid()}.pdf");
}
```

### Concurrent rendering

```csharp
// Per-task renderer — no shared state
await Task.WhenAll(htmlBatch.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    await pdf.SaveAsAsync($"{Guid.NewGuid()}.pdf");
}));
// Parallel guide: https://ironpdf.com/examples/parallel/
```

### Disposal

```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// Disposed at end of block
```

### Edge cases

- **Cold start:** Local Chromium has initialization overhead on first render. Pre-warm in latency-sensitive services.
- **Memory footprint:** A Chromium-based renderer has higher baseline memory than a thin HTTP client. Profile under load.
- **Rate limiting no longer applies:** Remove rate limiter middleware added to manage cloud API quotas, and remove 429 retry blocks.

---

## Migration checklist

### Pre-migration

- [ ] Inventory all PDFBolt REST calls: `rg "api\.pdfbolt\.com|API-KEY" --type cs`
- [ ] Identify PDF/A or other feature gaps that triggered evaluation
- [ ] Identify secondary libraries added to supplement PDFBolt (merge, watermark, security, text extraction)
- [ ] Verify IronPDF .NET target framework compatibility
- [ ] Confirm commercial license requirements
- [ ] Set up IronPDF trial license in dev environment
- [ ] Pull all HTML templates for render comparison

### Code migration

- [ ] Delete the hand-rolled PDFBolt HttpClient wrapper and DTOs
- [ ] Remove secondary libraries (if supplementing PDFBolt only)
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `using System.Net.Http;` PDFBolt imports with `using IronPdf;`
- [ ] Replace `API-KEY` header initialization with `IronPdf.License.LicenseKey`
- [ ] Replace `POST /v1/direct` calls with `ChromePdfRenderer.RenderHtmlAsPdf` / `RenderUrlAsPdf`
- [ ] Convert base64 HTML encoding step away — IronPDF takes raw HTML
- [ ] Replace merge, watermark, security operations with IronPDF natives
- [ ] Add PDF/A export where required
- [ ] Remove HTTP retry, timeout, and circuit-breaker logic
- [ ] Remove 429 / quota handling
- [ ] Add IronPDF license key to config

### Testing

- [ ] Render each HTML template and visually compare output
- [ ] Test PDF/A output with a validator (PAC, veraPDF — free tools)
- [ ] Test merge with representative document sets
- [ ] Test watermark on multi-page documents
- [ ] Test password protection (correct and incorrect credentials)
- [ ] Load test at expected peak concurrency
- [ ] Test in Docker/CI environment (no outbound API calls needed)

### Post-migration

- [ ] Remove PDFBolt API key from config and secrets management
- [ ] Remove outbound firewall rule to `api.pdfbolt.com` if applicable
- [ ] Remove HTTP client retry logic
- [ ] Monitor memory baseline (Chromium footprint vs HTTP client)

---

## Next Steps

The most significant architectural change in this migration is the shift from a cloud REST dependency to an in-process renderer. That change is structural — it affects your error handling, your async patterns, your secrets management, and your container configuration. The HTML-to-PDF API surface is a smaller concern by comparison.

**What did you find trickiest in the move off PDFBolt — the async-to-sync conversion, the base64 HTML payload removal, or the secrets cleanup?** Interested to hear from teams who had the cloud dependency deeply integrated into background job pipelines.
