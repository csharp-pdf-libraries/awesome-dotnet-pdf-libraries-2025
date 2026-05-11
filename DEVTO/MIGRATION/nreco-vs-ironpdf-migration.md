---
title: "Replacing NReco PDF Generator with IronPDF: what breaks, what doesn't"
published: false
tags: dotnet, csharp, pdf, migration
---

The PDF looks close. The heading font is right, the table renders, the footer is in the right position — but the column widths are slightly off, the image didn't align correctly, and the multi-line cell that looked fine in browser preview wrapped differently on page 3. You've spent hours tweaking CSS and the gap keeps closing but never quite closes. That's the CSS fidelity wall that teams hit with wkhtmltopdf-based generators, and NReco PDF Generator is a .NET wrapper around wkhtmltopdf.

This isn't a critique — wkhtmltopdf does a lot correctly. But its WebKit-based renderer has known limitations with modern CSS that a Chromium-based renderer handles differently. This article covers migrating from NReco PDF Generator to IronPDF and the troubleshooting patterns for the most common breakage points.

---

## Diagnosing the fidelity problem first

Before migrating, confirm that the fidelity gap is renderer-related (WebKit vs Chromium) rather than a configuration issue. Some CSS fidelity problems with NReco/wkhtmltopdf are fixable.

### Common NReco fidelity failures and their diagnosis

**Problem: Flexbox or CSS Grid layout broken**

wkhtmltopdf uses a WebKit build from approximately 2012. CSS Flexbox and Grid have very limited support:

```bash
# Check your wkhtmltopdf version
wkhtmltopdf --version
# If < 0.12.6, Flexbox may not work at all
# Even 0.12.6 has incomplete Flexbox — Grid is largely unsupported
```

**Fix within NReco:** Convert layouts to `display: table` or float-based layouts (pre-Flexbox techniques). This works but means maintaining two CSS codebases if your HTML also renders in a browser.

**Fix via migration:** Chromium-based renderer supports current CSS — no layout workarounds needed.

**Problem: `@media print` rules not applying**

```csharp
// NReco: enable media type setting
var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
htmlToPdf.CustomWkhtmltopdfArgs = "--print-media-type";
// Required if your CSS uses @media print
```

If it was already set and still broken, the issue is WebKit-level — a renderer change is needed.

**Problem: Background colors/images not rendering**

```csharp
// NReco: enable background rendering
var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
htmlToPdf.CustomWkhtmltopdfArgs = "--background";
// Also check: --disable-smart-shrinking
```

**Problem: Page breaks in wrong places**

```csharp
// NReco/wkhtmltopdf page break control via CSS
// In your HTML:
// <div style="page-break-before: always;"></div>
// Or: break-before: page; (CSS3 — limited support in wkhtmltopdf's WebKit)
```

If after applying these fixes the output still doesn't match, the gap is likely renderer-level and migration is the more efficient path.

---

## Why migrate (without drama)

Eight neutral triggers for leaving NReco PDF Generator:

1. **WebKit vs Chromium rendering gap** — modern CSS (Grid, Flexbox, CSS variables, some pseudo-elements) works in Chromium-based renderers but not in wkhtmltopdf's WebKit.
2. **wkhtmltopdf is no longer actively developed** — the upstream wkhtmltopdf repository was archived in January 2023 (last stable release 0.12.6 in June 2020), and NReco.PdfGenerator 1.2.1 was published shortly after with no further updates since.
3. **Binary dependency management** — wkhtmltopdf binary (or libwkhtmltopdf) must be installed in every environment. Docker images require explicit installation steps.
4. **No PDF manipulation** — NReco generates PDFs; it doesn't merge, split, watermark, or encrypt them. Secondary libraries are needed.
5. **Subprocess or P/Invoke architecture** — both patterns add complexity: process management, error handling, timeout management, interop fragility.
6. **JavaScript rendering** — wkhtmltopdf's JS support is limited. Dynamic content generated via modern JS frameworks may not render.
7. **Font handling differences** — system fonts on the render server must match what the template expects; font subsetting behavior differs from Chromium.
8. **.NET version trajectory** — with NReco's package effectively frozen since early 2023, check its compatibility with your target .NET version as you upgrade.

### Comparison table

| Aspect | NReco PDF Generator | IronPDF |
|---|---|---|
| Focus | HTML-to-PDF via wkhtmltopdf | HTML-to-PDF + PDF manipulation |
| Pricing | Free for non-SaaS single-server use; $199 enterprise pack for SaaS / multi-server | Commercial (see ironsoftware.com) |
| API Style | Wrapper around wkhtmltopdf subprocess/native | In-process .NET library |
| Learning Curve | Low for basic use | Medium |
| HTML Rendering | WebKit (wkhtmltopdf) — pre-modern CSS | Chromium-based — full CSS3 |
| Page Indexing | N/A (generation only) | 0-based |
| Thread Safety | Subprocess: process-level isolation | Renderer instance reuse — see async docs |
| Namespace | `NReco.PdfGenerator` | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | NReco approach | Effort to migrate |
|---|---|---|
| HTML string to PDF | `HtmlToPdfConverter.GeneratePdf()` | Low |
| URL to PDF | `GeneratePdfFromUrl()` | Low |
| Custom margins | `CustomWkhtmltopdfArgs` or property | Low |
| Headers / footers | `HtmlHeader` / `HtmlFooter` | Medium |
| wkhtmltopdf CLI flags | `CustomWkhtmltopdfArgs` | Medium — map to `ChromePdfRenderOptions` |
| Merge PDFs | External library required | Low (native in IronPDF) |
| Watermark | External library required | Low |
| Password protection | External library required | Low |
| CSS Grid / Flexbox layouts | Broken in WebKit | Low to fix — automatic in Chromium |
| JavaScript rendering | Limited in WebKit | Low to improve — Chromium supports more |
| Binary management | Required | Eliminated |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| CSS fidelity gap is the primary problem | Chromium-based renderer is the fix; evaluate IronPDF or PuppeteerSharp |
| wkhtmltopdf works fine for your templates | No urgent reason to migrate — factor in wkhtmltopdf maintenance trajectory |
| Need merge/watermark/security in same library | IronPDF consolidates; removes secondary library dependency |
| Open source budget constraint | PuppeteerSharp (free, Chromium-based) is an alternative |

---

## Before you start

### Prerequisites

- .NET 6+ target
- HTML template inventory for render comparison testing (critical for confirming CSS fidelity improvements)
- wkhtmltopdf version noted for diagnostic purposes

### Find NReco references in your codebase

```bash
# Find all NReco.PdfGenerator usage
rg -l "NReco\|HtmlToPdfConverter" --type cs

# Find converter instantiation
rg "HtmlToPdfConverter\|new.*PdfConverter" --type cs -n

# Find CustomWkhtmltopdfArgs usage — flags to migrate to options
rg "CustomWkhtmltopdfArgs\|WkhtmltopdfPath" --type cs -n

# Find using directives
rg "using NReco" --type cs -n

# Find wkhtmltopdf binary references
rg "wkhtmltopdf" . -n 2>/dev/null
find . -name "wkhtmltopdf*" 2>/dev/null
```

### Remove NReco, install IronPDF

```bash
# Remove NReco PDF Generator
dotnet remove package NReco.PdfGenerator

# Install IronPDF
dotnet add package IronPdf
dotnet restore
```

Also remove wkhtmltopdf from Docker images and CI/CD:

```bash
# Find Docker references to wkhtmltopdf
rg "wkhtmltopdf" Dockerfile* -n

# Find CI/CD install steps
rg "wkhtmltopdf" .github/ azure-pipelines.yml -l 2>/dev/null
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (NReco — no license key for basic use):**
```csharp
using NReco.PdfGenerator;

// NReco: no license key required for non-SaaS single-server use
// $199 enterprise pack required for SaaS / multi-server / redistribution
var htmlToPdf = new HtmlToPdfConverter();
// Optional: set wkhtmltopdf binary path if not in PATH
// htmlToPdf.WkhtmltopdfPath = "/usr/local/bin/";
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// License guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
using NReco.PdfGenerator;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering; // for ChromePdfRenderOptions
```

### Step 3: Basic HTML-to-PDF

**Before (NReco):**
```csharp
using NReco.PdfGenerator;
using System;
using System.IO;

var htmlToPdf = new HtmlToPdfConverter();
htmlToPdf.CustomWkhtmltopdfArgs = "--print-media-type --background";

byte[] pdfBytes = htmlToPdf.GeneratePdf("<h1>Hello World</h1>");
File.WriteAllBytes("output.pdf", pdfBytes);
Console.WriteLine("Saved: output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Troubleshooting: common migration failures

### Problem: "Flexbox layout is broken in NReco but we need to keep it"

This is the renderer issue — the fix is Chromium, not configuration:

```csharp
// NReco/WebKit: Flexbox broken
// <div style="display: flex; gap: 10px;">...</div>

// After migrating to IronPDF — same HTML works natively:
var renderer = new ChromePdfRenderer();
string html = @"<html><body>
  <div style='display: flex; gap: 10px;'>
    <div style='flex: 1;'>Column 1</div>
    <div style='flex: 1;'>Column 2</div>
  </div>
</body></html>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("flex-layout.pdf");
```

### Problem: "wkhtmltopdf flag has no IronPDF equivalent"

Map each `CustomWkhtmltopdfArgs` flag to a `ChromePdfRenderOptions` property:

| wkhtmltopdf flag | IronPDF equivalent |
|---|---|
| `--page-size A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` |
| `--margin-top 10mm` | `RenderingOptions.MarginTop = 10` |
| `--orientation Landscape` | `RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape` |
| `--print-media-type` | Not needed — Chromium handles this |
| `--background` | Not needed — Chromium handles this |
| `--disable-smart-shrinking` | `RenderingOptions.FitToPaperMode = FitToPaperModes.Zoom` |
| `--javascript-delay 1000` | `RenderingOptions.WaitFor.JavaScript()` |

```csharp
// Mapping NReco options to IronPDF
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize    = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop    = 10; // mm
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.MarginLeft   = 15;
renderer.RenderingOptions.MarginRight  = 15;
renderer.RenderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Portrait;
// Full options: https://ironpdf.com/how-to/rendering-options/
```

### Problem: "Background colors still don't render"

In IronPDF this is controlled via CSS — no special flag needed:

```html
<!-- This renders correctly in IronPDF with no special configuration -->
<div style="background-color: #f0f0f0; padding: 20px;">
  <h1>Section Header</h1>
</div>
```

### Problem: "Page headers and footers format differently"

NReco passes HTML headers/footers to wkhtmltopdf via specific properties. IronPDF uses `HtmlHeaderFooter`:

```csharp
// NReco header pattern:
// htmlToPdf.PageHeaderHtml = "<div style='text-align:right'>Page {page}</div>";

// IronPDF equivalent:
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:right;font-size:9pt;'>" +
                   "Page {page} of {total-pages}</div>",
    DrawDividerLine = true
};
// Headers guide: https://ironpdf.com/how-to/headers-and-footers/
```

---

## API mapping tables

### Namespace mapping

| NReco | IronPDF | Notes |
|---|---|---|
| `NReco.PdfGenerator` | `IronPdf` | Core |
| N/A | `IronPdf.Rendering` | Options/configuration |
| N/A | `IronPdf.Editing` | Manipulation ops |

### Core class mapping

| NReco class | IronPDF class | Description |
|---|---|---|
| `HtmlToPdfConverter` | `ChromePdfRenderer` | HTML-to-PDF entry point |
| `CustomWkhtmltopdfArgs` (string) | `ChromePdfRenderOptions` | Render configuration |
| N/A | `PdfDocument` | PDF document model |
| `byte[]` return | `PdfDocument` return | Output — save, stream, or manipulate |

### Document loading methods

| Operation | NReco | IronPDF |
|---|---|---|
| HTML string | `htmlToPdf.GeneratePdf(html)` | `renderer.RenderHtmlAsPdf(html)` |
| URL | `GeneratePdfFromUrl(url)` | `renderer.RenderUrlAsPdf(url)` |
| HTML file | File-read + string | `renderer.RenderHtmlFileAsPdf(path)` |
| Existing PDF | Not NReco scope | `PdfDocument.FromFile(path)` |

### Page operations

| Operation | NReco | IronPDF |
|---|---|---|
| Paper size | `CustomWkhtmltopdfArgs --page-size` | `ChromePdfRenderOptions.PaperSize` |
| Margins | `CustomWkhtmltopdfArgs --margin-*` | `ChromePdfRenderOptions.Margin*` |
| Orientation | `CustomWkhtmltopdfArgs --orientation` | `ChromePdfRenderOptions.PaperOrientation` |
| Page count | Not accessible | `pdf.PageCount` |

### Merge/split operations

| Operation | NReco | IronPDF |
|---|---|---|
| Merge | External library required | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | External library required | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

### 1. HTML to PDF

**Before (NReco PDF Generator):**
```csharp
using NReco.PdfGenerator;
using System;
using System.IO;

class HtmlToPdfExample
{
    static void Main()
    {
        var htmlToPdf = new HtmlToPdfConverter();

        // Common NReco configuration pattern
        htmlToPdf.CustomWkhtmltopdfArgs =
            "--print-media-type --background --dpi 180 " +
            "--margin-top 10mm --margin-bottom 10mm " +
            "--margin-left 15mm --margin-right 15mm";

        // Optional: set explicit binary path
        // htmlToPdf.WkhtmltopdfPath = "/usr/local/bin/";

        string html = @"
            <html><head><style>
                body { font-family: Arial; font-size: 10pt; }
                h1 { font-size: 18pt; }
                .total { font-weight: bold; }
            </style></head><body>
                <h1>Invoice #1234</h1>
                <p>Customer: ACME Corp</p>
                <p class='total'>Total: $500.00</p>
            </body></html>";

        byte[] pdfBytes = htmlToPdf.GeneratePdf(html);
        File.WriteAllBytes("invoice.pdf", pdfBytes);
        Console.WriteLine("Saved: invoice.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize  = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop    = 10;
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.MarginLeft   = 15;
renderer.RenderingOptions.MarginRight  = 15;

var pdf = renderer.RenderHtmlAsPdf(@"
    <html><head><style>
        body { font-family: Arial; font-size: 10pt; }
        h1 { font-size: 18pt; }
        .total { font-weight: bold; }
    </style></head><body>
        <h1>Invoice #1234</h1>
        <p>Customer: ACME Corp</p>
        <p class='total'>Total: $500.00</p>
    </body></html>");

pdf.SaveAs("invoice.pdf");
// Rendering options: https://ironpdf.com/how-to/rendering-options/
```

---

### 2. Merge PDFs

**Before (NReco — not supported natively; secondary library):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;

class MergePdfsExample
{
    static void Main()
    {
        // NReco doesn't merge PDFs
        // PdfSharp is a common secondary choice
        using var output = new PdfDocument();

        foreach (string path in new[] { "section1.pdf", "section2.pdf", "section3.pdf" })
        {
            using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            foreach (PdfPage page in input.Pages)
                output.AddPage(page);
        }

        output.Save("merged.pdf");
        Console.WriteLine("Merged: merged.pdf");
    }
}
```

**After (IronPDF native):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var merged = PdfDocument.Merge(
    PdfDocument.FromFile("section1.pdf"),
    PdfDocument.FromFile("section2.pdf"),
    PdfDocument.FromFile("section3.pdf")
);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (NReco — not supported; secondary library):**
```csharp
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;

class WatermarkExample
{
    static void Main()
    {
        // NReco generates; iTextSharp stamps
        using var reader  = new PdfReader("generated.pdf");
        using var fs      = new FileStream("watermarked.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs);

        var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
        for (int page = 1; page <= reader.NumberOfPages; page++)
        {
            var cb = stamper.GetOverContent(page);
            cb.SaveState();
            var gs1 = new PdfGState { FillOpacity = 0.3f };
            cb.SetGState(gs1);
            cb.BeginText();
            cb.SetFontAndSize(baseFont, 60);
            cb.SetColorFill(BaseColor.GRAY);
            cb.ShowTextAligned(Element.ALIGN_CENTER, "CONFIDENTIAL", 300, 420, 45);
            cb.EndText();
            cb.RestoreState();
        }
        Console.WriteLine("Watermarked: watermarked.pdf");
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
    Text = "CONFIDENTIAL",
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

**Before (NReco — not supported; secondary library):**
```csharp
using iTextSharp.text.pdf;
using System.IO;
using System.Text;

class SecurityExample
{
    static void Main()
    {
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

### wkhtmltopdf binary cleanup

```bash
# Remove from Dockerfile
# Before: RUN apt-get install -y wkhtmltopdf
# or:     COPY wkhtmltopdf /usr/local/bin/

# Remove from CI/CD
rg "wkhtmltopdf" .github/ azure-pipelines.yml Jenkinsfile -l 2>/dev/null

# Remove from application config
rg "WkhtmltopdfPath\|wkhtmltopdf" appsettings*.json -n
```

### `byte[]` output → `PdfDocument`

NReco returns `byte[]`. IronPDF returns `PdfDocument`. Update call sites:

```csharp
// Before: byte[] pdfBytes = htmlToPdf.GeneratePdf(html);
// After:
var pdf = renderer.RenderHtmlAsPdf(html);

// If you need bytes (for HTTP response, storage, etc.):
pdf.SaveAs("output.pdf");
// Or stream to memory:
using var ms = new MemoryStream();
pdf.Stream.CopyTo(ms);
byte[] pdfBytes = ms.ToArray();
// Memory stream guide: https://ironpdf.com/how-to/pdf-memory-stream/
```

### Page indexing

IronPDF is 0-based. NReco doesn't expose a page model post-render, but if you're feeding output to page-aware tools, check their indexing convention.

---

## Performance considerations

### No subprocess overhead

NReco spawns wkhtmltopdf as a process (or calls native library). IronPDF runs in-process. Subprocess spawn cost (~50-200ms per call, system-dependent) is eliminated:

```csharp
// Reuse renderer for batch work — amortizes Chromium initialization
var renderer = new ChromePdfRenderer();

foreach (var html in batchTemplates)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"doc_{Guid.NewGuid()}.pdf");
}
```

### Async rendering

```csharp
// Async for web controllers — NReco doesn't have native async
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
return File(pdf.Stream, "application/pdf", "document.pdf");
// Async guide: https://ironpdf.com/how-to/async/
```

### Edge cases

- **JavaScript-heavy templates:** Chromium supports modern JS; tune timing for async content with `WaitFor` options.
- **Font rendering:** Test custom fonts explicitly — system font resolution differs between WebKit and Chromium.
- **Memory footprint:** Chromium has higher baseline memory than wkhtmltopdf. Profile under load.

---

## Migration checklist

### Pre-migration

- [ ] Document all `CustomWkhtmltopdfArgs` flags in use — map each to `ChromePdfRenderOptions`
- [ ] Inventory all HTML templates for render comparison testing
- [ ] Identify secondary libraries (iTextSharp, PDFsharp) added to supplement NReco
- [ ] Find all wkhtmltopdf binary references: Dockerfile, CI/CD, appsettings
- [ ] Verify IronPDF .NET target framework compatibility
- [ ] Confirm commercial license requirements
- [ ] Set up IronPDF trial license in dev environment
- [ ] Run side-by-side render test on your templates before committing

### Code migration

- [ ] Remove `NReco.PdfGenerator` NuGet package
- [ ] Remove secondary libraries (if only supplementing NReco)
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `using NReco.PdfGenerator` with `using IronPdf`
- [ ] Replace `HtmlToPdfConverter` with `ChromePdfRenderer`
- [ ] Replace `CustomWkhtmltopdfArgs` flags with `ChromePdfRenderOptions` properties
- [ ] Replace `GeneratePdf()` with `renderer.RenderHtmlAsPdf()`
- [ ] Update output from `byte[]` to `PdfDocument` model
- [ ] Replace merge, watermark, security operations with IronPDF natives
- [ ] Add IronPDF license key to config

### Testing

- [ ] Render all templates and visually compare against NReco output
- [ ] Focus on: Flexbox/Grid layouts, background colors, custom fonts
- [ ] Verify headers and footers on multi-page documents
- [ ] Test merge, watermark, and security
- [ ] Verify CSS improvements match expectations (Flexbox, Grid)
- [ ] Load test concurrent rendering at expected peak
- [ ] Test in Docker/CI environment — confirm no binary dependency errors

### Post-migration

- [ ] Remove wkhtmltopdf binary/install from Dockerfiles
- [ ] Remove wkhtmltopdf install from CI/CD pipelines
- [ ] Remove `WkhtmltopdfPath` from appsettings
- [ ] Monitor memory usage first production week

---

## Before You Ship

The fidelity gap between WebKit and Chromium is real and well-documented. For most modern HTML templates — especially ones that use Flexbox for layout or CSS variables for theming — the improvement is immediate after switching renderers. The regression risk is typically in edge cases: unusual font handling, very specific table rendering, or page break behavior.

**Which feature was hardest to replicate when migrating from NReco PDF Generator, and why?** Particularly interested in teams who had complex header/footer HTML or JavaScript-driven content — those are the areas where renderer differences show up most.
