---
title: "Switching from GrabzIt to IronPDF: copy-paste and ship"
published: false
tags: dotnet, csharp, pdf, migration
---

You've got a service that generates PDFs and screenshots. It worked fine at one pod. Now you're running three, and the GrabzIt API calls are stepping on each other — timeouts, corrupt outputs, occasional 429s from the cloud endpoint. The rate limiter doesn't care that your traffic is legitimate. You're scaling up, and the architecture doesn't support it.

This article walks through migrating from GrabzIt to IronPDF for teams hitting concurrency walls or wanting an on-premises rendering pipeline they fully control. Even if you don't switch, the migration mapping tables and checklist are reference material worth bookmarking.

---

## Why migrate (without drama)

Switching PDF libraries is a non-trivial investment. Here are 9 neutral reasons teams run this migration — not all will apply to you:

1. **Concurrency limits hit** — GrabzIt cloud API has rate limits. Under burst traffic, requests queue or fail.
2. **Network dependency at render time** — every PDF generation makes an outbound HTTP call. Latency spikes in cloud environments, VPN-locked offices, or air-gapped systems.
3. **Data residency requirements** — HTML content is sent to GrabzIt servers. Some compliance frameworks prohibit sending document content to third-party endpoints.
4. **Offline/edge deployments** — embedded systems, offline factory floors, or on-prem government environments can't reach the GrabzIt API.
5. **Docker/Kubernetes complexity** — managing API keys in secrets, retry logic for transient failures, fallback strategies: overhead that a local renderer eliminates.
6. **Cost structure changes** — SaaS pricing scales with usage. At volume, a one-time license can look different on a spreadsheet than per-render billing. (No pricing comparison made here — evaluate both against your own usage numbers.)
7. **CSS/JS fidelity** — cloud renderers have their own Chromium version and update cadence. Your HTML may render differently than expected.
8. **Timeout handling** — async callback requests over HTTP introduce timeout complexity not present in in-process rendering.
9. **Vendor lock-in** — thin wrapper pattern means your code is coupled to GrabzIt's API contract. If they change endpoints or deprecate methods, your builds break.

### Comparison table

| Aspect | GrabzIt | IronPDF |
|---|---|---|
| Focus | Cloud screenshot + PDF SaaS | Local PDF generation / manipulation |
| Pricing | Subscription tiers | Per-developer license |
| API Style | REST client wrapper — async callback model | In-process .NET library — synchronous + async |
| Learning Curve | Low (minimal setup), but limited control | Medium (more API surface, more control) |
| HTML Rendering | Cloud Chromium (version managed by vendor) | Chromium-based local renderer |
| Output Type | Image-based PDF (screenshot wrapped in PDF) | Vector PDF with selectable text |
| Thread Safety | Rate-limited by API tier | Renderer instance reuse — see async docs |
| Namespace | `GrabzIt`, `GrabzIt.Parameters` | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | GrabzIt approach | Effort to migrate |
|---|---|---|
| HTML string to PDF | `HTMLToPDF` | Low |
| URL to PDF | `URLToPDF` | Low |
| Screenshot capture | `HTMLToImage` / `URLToImage` | High — IronPDF generates PDFs, not standalone screenshots |
| Merge PDFs | Not a GrabzIt core feature | Low (IronPDF has it natively) |
| Watermark | `CustomWaterMarkId` (pre-registered) | Low (IronPDF accepts inline HTML) |
| Password protection | `PDFOptions.Password` | Low |
| Custom headers/footers | `TemplateId` (pre-configured) | Medium |
| Async batch rendering | API-level queuing | Medium — see IronPDF async docs |
| Retry / fallback logic | Custom in your code | Medium — already in your code, stays there |
| Data residency compliance | N/A (data leaves your infra) | High value — eliminates the concern |
| Custom CSS / fonts | Limited by cloud renderer | Medium — test your specific templates |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| Need standalone screenshot capture (not just PDF) | GrabzIt remains relevant; IronPDF doesn't replace standalone screenshot use cases |
| Air-gapped / offline deployment | IronPDF or another local renderer; GrabzIt won't work |
| Data residency / compliance requirement | Local renderer required; evaluate IronPDF, PuppeteerSharp, or wkhtmltopdf |
| Scaling concurrent PDF generation | Evaluate local renderer; test both under load before committing |

---

## Before you start

### Prerequisites

- .NET 6+ recommended (IronPDF supports .NET Framework 4.6.2+ and .NET Core 3.1+ / .NET 5+)
- NuGet access or offline package cache
- A dev copy of your codebase
- GrabzIt API key + secret (to reference while writing equivalents)

### Find GrabzIt references in your codebase

```bash
# Find all files referencing GrabzIt
rg -l "GrabzIt" --type cs

# Find specific client instantiation
rg "GrabzItClient" --type cs -n

# Find all using statements
rg "using GrabzIt" --type cs -n

# Find callback handlers
rg "GrabzIt" -g "*.ashx" -g "*.aspx" -n

# Find API key references in config
rg "APPLICATION_KEY|APPLICATION_SECRET|grabzit" -g "*.config" -g "*.json"
```

### Remove GrabzIt, install IronPDF

```bash
# Remove GrabzIt NuGet package
dotnet remove package GrabzIt

# Install IronPDF
dotnet add package IronPdf

# Restore
dotnet restore
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (GrabzIt API key + secret at client instantiation):**
```csharp
using GrabzIt;

// GrabzIt requires your API key + secret at client instantiation
var grabzIt = new GrabzItClient("YOUR_APPLICATION_KEY", "YOUR_APPLICATION_SECRET");
// Key and signed signature are sent with every API request to the cloud
```

**After (IronPDF license key):**
```csharp
using IronPdf;

// Set once at app startup — appsettings or environment variable
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// License setup guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
using GrabzIt;
using GrabzIt.Parameters;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering; // for ChromePdfRenderOptions
```

### Step 3: Basic HTML-to-PDF conversion

**Before (GrabzIt HTML to PDF):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;
using System;

class Program
{
    static void Main()
    {
        var grabzIt = new GrabzItClient("YOUR_APPLICATION_KEY", "YOUR_APPLICATION_SECRET");

        var options = new PDFOptions();
        options.CustomId = "my-pdf";

        // HTMLToPDF queues the capture; SaveTo blocks until the result is ready
        grabzIt.HTMLToPDF("<h1>Hello World</h1>", options);
        grabzIt.SaveTo("output.pdf");

        Console.WriteLine("Saved");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// Full guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API mapping tables

### Namespace mapping

| GrabzIt | IronPDF | Notes |
|---|---|---|
| `GrabzIt` | `IronPdf` | Top-level namespace |
| `GrabzIt.Parameters` | `IronPdf.Rendering` | Options/config classes |
| Screenshot APIs (`HTMLToImage`, `URLToImage`) | N/A | Standalone screenshot feature not in IronPDF |

### Core class mapping

| GrabzIt class | IronPDF class | Description |
|---|---|---|
| `GrabzItClient` | `ChromePdfRenderer` | Main rendering entry point |
| `PDFOptions` | `ChromePdfRenderOptions` | Render configuration |
| `ImageOptions` | `ChromePdfRenderOptions` (PDF only) | Image-specific options have no direct equivalent |
| N/A | `PdfDocument` | Represents a loaded/generated PDF |

### Document loading methods

| Operation | GrabzIt | IronPDF |
|---|---|---|
| HTML string | `HTMLToPDF(html, options)` | `renderer.RenderHtmlAsPdf(html)` |
| URL | `URLToPDF(url, options)` | `renderer.RenderUrlAsPdf(url)` |
| File | `FileToPDF(path)` | `renderer.RenderHtmlFileAsPdf(path)` |
| Existing PDF | N/A | `PdfDocument.FromFile(path)` |
| Retrieve result | `Save(callbackUrl)` / `SaveTo(path)` / `GetResult(id)` | `pdf.SaveAs(path)` or `pdf.BinaryData` |

### Page operations

| Operation | GrabzIt | IronPDF |
|---|---|---|
| Page count | N/A | `pdf.PageCount` |
| Extract page | N/A | `pdf.CopyPages(startIndex, endIndex)` |
| Page size | `PDFOptions.PageSize` | `ChromePdfRenderOptions.PaperSize` |
| Margins | `PDFOptions.MarginTop` etc. | `ChromePdfRenderOptions.MarginTop` etc. |

### Merge/split operations

| Operation | GrabzIt | IronPDF |
|---|---|---|
| Merge PDFs | Not a core feature | `PdfDocument.Merge(pdf1, pdf2)` |
| Split PDF | Not a core feature | `pdf.CopyPages(0, n)` |

---

## Four complete before/after migrations

### 1. HTML to PDF

**Before (GrabzIt):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;
using System;

class HtmlToPdfExample
{
    static void Main(string[] args)
    {
        var client = new GrabzItClient("YOUR_APPLICATION_KEY", "YOUR_APPLICATION_SECRET");

        var pdfOptions = new PDFOptions
        {
            MarginTop = 20,
            MarginBottom = 20,
            IncludeBackground = true
        };

        // Queue the capture
        client.HTMLToPDF(
            "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>",
            pdfOptions
        );

        // SaveTo blocks until the capture is complete and downloads the file
        bool saved = client.SaveTo("invoice.pdf");
        if (!saved)
            Console.WriteLine("Conversion failed or timed out");
        else
            Console.WriteLine("Saved: invoice.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.PrintHtmlBackgrounds = true;

var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>"
);
pdf.SaveAs("invoice.pdf");
// Docs: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (GrabzIt — merging is not a native GrabzIt feature; typical workaround):**
```csharp
using System.IO;
using PdfSharp.Pdf;      // Common workaround — teams add a second library
using PdfSharp.Pdf.IO;

class MergePdfsExample
{
    static void Main()
    {
        // Generate each PDF via GrabzIt separately, then merge with PdfSharp
        // (or iTextSharp, or another library)
        using var output = new PdfDocument();

        foreach (var filePath in new[] { "part1.pdf", "part2.pdf" })
        {
            using var input = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            foreach (PdfPage page in input.Pages)
                output.AddPage(page);
        }

        output.Save("merged.pdf");
        Console.WriteLine("Merged.");
    }
}
```

**After (IronPDF native merge):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf1 = PdfDocument.FromFile("part1.pdf");
var pdf2 = PdfDocument.FromFile("part2.pdf");
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
// Docs: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (GrabzIt — references a pre-registered watermark by ID):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

class WatermarkExample
{
    static void Main()
    {
        var client = new GrabzItClient("YOUR_APPLICATION_KEY", "YOUR_APPLICATION_SECRET");

        // Watermarks must be registered ahead of time (dashboard or AddWaterMark API)
        // and are then referenced from PDFOptions only by string ID — no inline styling.
        var options = new PDFOptions
        {
            CustomWaterMarkId = "watermark123" // pre-registered watermark
        };

        client.HTMLToPDF("<h1>Quarterly Report</h1>", options);
        client.SaveTo("watermarked.pdf");
    }
}
```

**After (IronPDF — inline HTML watermark, no pre-registration):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Quarterly Report</h1>");

pdf.ApplyWatermark(
    "<div style='color:red; font-size:48px; font-weight:bold; " +
    "transform:rotate(-45deg); opacity:0.3;'>CONFIDENTIAL</div>",
    opacity: 30,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);

pdf.SaveAs("watermarked.pdf");
// Docs: https://ironpdf.com/how-to/custom-watermark/
```

---

### 4. Password protection

**Before (GrabzIt — single password via PDFOptions):**
```csharp
using GrabzIt;
using GrabzIt.Parameters;

class PasswordExample
{
    static void Main()
    {
        var client = new GrabzItClient("YOUR_APPLICATION_KEY", "YOUR_APPLICATION_SECRET");

        // PDFOptions exposes a single Password field — applied to the rendered PDF
        var options = new PDFOptions
        {
            Password = "secretpassword"
        };

        client.HTMLToPDF("<h1>Confidential</h1>", options);
        client.SaveTo("protected.pdf");
    }
}
```

**After (IronPDF — granular owner/user passwords and permissions):**
```csharp
using IronPdf;
using IronPdf.Security;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential</h1>");

pdf.SecuritySettings.OwnerPassword = "ownerpass";
pdf.SecuritySettings.UserPassword = "userpass";
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SaveAs("protected.pdf");
// Docs: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Output type difference

GrabzIt produces image-based PDFs — essentially screenshots wrapped in a PDF container. IronPDF produces vector PDFs with selectable, searchable text. This is usually a welcome upgrade (smaller files, real text), but it does mean any code that depended on the image-based nature (such as treating every page as a raster) needs review.

### Page indexing

IronPDF uses 0-based page indexing. If your codebase has any page-number logic that was passed through to GrabzIt headers/footers or built externally on a 1-based assumption, audit those sites:

```csharp
// IronPDF: page 0 = first page
var firstPage = pdf.Pages[0]; // not [1]
```

### Network vs in-process model

GrabzIt is fundamentally a network call with a callback or polling result pattern. Your existing retry/timeout/circuit-breaker logic around those HTTP calls should be reviewed — it may no longer be necessary, or it may need to be rewritten around different failure modes (renderer startup, memory pressure).

### Standalone screenshot use case gap

IronPDF generates PDFs and can rasterize a generated PDF to images via `ToBitmap()`, but it is not a general-purpose screenshot service. If your codebase uses GrabzIt's `HTMLToImage` / `URLToImage` alongside PDF generation, you'll need a separate tool for that use case — [PuppeteerSharp](https://github.com/hardkoded/puppeteer-sharp) is a common .NET option. Plan this before starting the migration.

### Async patterns

GrabzIt is inherently async (cloud callback model). IronPDF has both sync and async APIs. If you're migrating to async IronPDF rendering, see [IronPDF async documentation](https://ironpdf.com/how-to/async/).

---

## Performance considerations

### Renderer reuse

Instantiating `ChromePdfRenderer` is relatively lightweight, but instantiating repeatedly in tight loops is inefficient. Reuse instances where possible:

```csharp
// Preferred for batch work
var renderer = new ChromePdfRenderer();
// Reuse renderer across renders in same scope
foreach (var html in htmlBatch)
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{Guid.NewGuid()}.pdf");
}
```

### Concurrency

For concurrent rendering, create separate renderer instances per thread rather than sharing one. See [parallel rendering examples](https://ironpdf.com/examples/parallel/).

```csharp
// Per-thread renderer instantiation is safe
Parallel.ForEach(htmlBatch, html =>
{
    var renderer = new ChromePdfRenderer(); // separate instance per thread
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"{Guid.NewGuid()}.pdf");
});
```

### Disposal

`PdfDocument` implements `IDisposable`. Use `using` blocks or explicit disposal in long-running services:

```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// Disposed at end of block
```

### Edge cases worth flagging

- **JavaScript-heavy pages:** IronPDF's Chromium renderer supports JS. For pages that load content asynchronously, use the `WaitFor` rendering options to delay capture until your condition is satisfied.
- **Font availability:** Local fonts on your rendering server need to be present. GrabzIt managed this on their end; you now own it.
- **Memory under load:** Local rendering has a higher memory footprint than a thin HTTP client. Load test before going to production.

---

## Migration checklist

### Pre-migration

- [ ] Audit all GrabzIt usages: `rg "GrabzIt" --type cs`
- [ ] Identify standalone screenshot use cases — plan separate tool if needed
- [ ] Identify callback handlers (`.ashx`, webhook endpoints) that will be decommissioned
- [ ] Verify IronPDF .NET version compatibility for your target framework
- [ ] Confirm data residency requirements are met by local rendering
- [ ] Pull all HTML templates used with GrabzIt for render testing
- [ ] Document current GrabzIt `PDFOptions` settings used in production
- [ ] Set up IronPDF trial license key for dev environment

### Code migration

- [ ] Remove `GrabzIt` NuGet package
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `GrabzItClient` instantiation with `ChromePdfRenderer`
- [ ] Replace `using GrabzIt` / `using GrabzIt.Parameters` imports with `using IronPdf`
- [ ] Replace `HTMLToPDF` calls with `renderer.RenderHtmlAsPdf` (see mapping table)
- [ ] Replace `URLToPDF` calls with `renderer.RenderUrlAsPdf`
- [ ] Replace `Save(callbackUrl)` / `SaveTo(path)` with `pdf.SaveAs(path)` or `pdf.BinaryData`
- [ ] Move `PDFOptions` settings onto `renderer.RenderingOptions`
- [ ] Replace `CustomWaterMarkId` references with `pdf.ApplyWatermark(...)`
- [ ] Replace `TemplateId` headers/footers with `RenderingOptions.HtmlHeader` / `HtmlFooter`
- [ ] Remove GrabzIt API key + secret from secrets management / inject IronPDF license key
- [ ] Update async code if migrating from callback model to async/await

### Testing

- [ ] Render each HTML template and visually compare output
- [ ] Test with production-representative HTML (fonts, images, CSS)
- [ ] Verify page count matches expected output
- [ ] Test merge and split operations
- [ ] Test watermark rendering on multi-page documents
- [ ] Test password protection: open with correct and incorrect passwords
- [ ] Verify text is selectable in output PDFs (a behavior change from GrabzIt's image PDFs)
- [ ] Load test: concurrent rendering at expected peak volume

### Post-migration

- [ ] Delete callback handler files (`.ashx`) and remove Web.config handler entries
- [ ] Remove GrabzIt environment variables / secrets from CI/CD and cloud config
- [ ] Update infrastructure: outbound HTTP to grabz.it no longer required (update firewall rules if applicable)
- [ ] Monitor memory usage in first week of production traffic
- [ ] Archive or remove GrabzIt-specific retry/circuit-breaker logic
- [ ] Cancel GrabzIt subscription once verified stable

---

## Next Steps

The main structural difference in this migration isn't the API surface — it's the architectural shift from a cloud-dependent HTTP client with an async callback model to an in-process renderer that returns results synchronously. Your error handling, retry logic, callback handlers, and async patterns all need revisiting with that in mind.

The checklist above covers the common cases, but real codebases always have wrinkles.

**What would you add to this migration checklist based on your own GrabzIt integration?** Particularly interested in teams who had GrabzIt wired into background jobs or serverless functions — that pattern has extra steps not covered here.
