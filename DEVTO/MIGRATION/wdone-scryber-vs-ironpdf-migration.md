# Scryber.Core → IronPDF: an honest migration walkthrough

License renewal prompts the conversation that was already overdue. The bill arrives, someone opens the NuGet page to check the latest version, and they find the last release was two years ago. Or the issue tracker has open bugs that haven't been touched in months. Scryber.Core is a capable PDF generation library that uses an XML-based template model — but before renewing or committing further, teams often do a quick evaluation pass of alternatives, and that's usually how this migration starts.

This article covers migrating from Scryber.Core to IronPDF. You'll have working before/after code and a thorough checklist by the end. The troubleshooting section applies regardless of which alternative you evaluate.

---

## Why Migrate (Without Drama)

Teams evaluating Scryber.Core alternatives typically cite:

1. **Maintenance cadence** — verify current release frequency; slower update cycles may mean lag on .NET version support.
2. **Template model specificity** — Scryber.Core's XML/PDFX template format requires learning its own document model rather than reusing HTML/CSS knowledge.
3. **CSS subset limitations** — Scryber.Core implements a subset of CSS; not all CSS properties behave as in a browser.
4. **Community size** — smaller community means fewer Stack Overflow answers and community examples.
5. **Missing manipulation features** — merge, split, watermark, security, and text extraction may require supplementary libraries.
6. **No live URL rendering** — Scryber.Core generates from templates/code; rendering a live web page isn't a primary use case.
7. **Documentation gaps** — complex layout scenarios may have limited examples in the official docs.
8. **.NET version support** — verify current Scryber.Core target frameworks at NuGet before assuming compatibility.
9. **Font handling** — custom font registration in Scryber.Core has its own configuration requirements.
10. **Error messaging** — XML template errors can surface as opaque runtime exceptions.

### Comparison Table

| Aspect | Scryber.Core | IronPDF |
|---|---|---|
| Focus | PDF generation from XML/template model | HTML-to-PDF + PDF manipulation |
| Pricing | Verify license at github.com/richard-scryber | Commercial license — verify at ironsoftware.com |
| API Style | XML template + C# binding | HTML renderer + C# objects |
| Learning Curve | Medium; own document model to learn | Low for web devs; HTML/CSS is the input |
| HTML Rendering | Partial HTML/CSS implementation | Embedded Chromium |
| Page Indexing | Verify in Scryber docs | 0-based |
| Thread Safety | Verify in Scryber docs | Verify IronPDF concurrent instance guidance |
| Namespace | `Scryber.Components`, `Scryber.PDF` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Scryber.Core | IronPDF Equivalent | Complexity |
|---|---|---|---|
| Template to PDF | `PDFDocument.ParseDocument()` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Medium (template rewrite) |
| Save to file | `doc.ProcessDocument(stream)` | `pdf.SaveAs(path)` | Low |
| Save to stream | `ProcessDocument(stream)` | `pdf.Stream` / `pdf.BinaryData` | Low |
| Data binding | `doc.Params["key"] = value` | HTML string interpolation / template engine | Medium |
| Custom page size | `@page-width`, `@page-height` in template | `RenderingOptions.PaperSize` | Low |
| CSS styling | Scryber CSS subset | Full CSS via Chromium | Medium |
| Merge PDFs | Verify native support | `PdfDocument.Merge()` | Medium |
| Watermark | Via template element | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Verify Scryber security API | `pdf.SecuritySettings` | Medium |
| Headers/footers | Template header/footer sections | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Text extraction | Verify | `pdf.ExtractAllText()` | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Templates already in XML/PDFX format and working well | Evaluate migration cost — Scryber templates don't auto-convert to HTML |
| Need full modern CSS support (flex, grid, custom properties) | Switch — Chromium renders current CSS; Scryber's CSS is a subset |
| Need HTML-to-PDF from live web pages or web templates | Switch — IronPDF supports URL rendering |
| Need programmatic PDF manipulation (merge, security, annotations) | Switch — IronPDF covers all of these natively |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All Scryber.Core References

```bash
# Find Scryber API usage
rg -l "Scryber\|PDFDocument\|ParseDocument\|pdfx\b" --type cs
rg "Scryber\|PDFDocument\|ParseDocument" --type cs -n

# Find .pdfx template files
find . -name "*.pdfx" | sort

# Count template files
find . -name "*.pdfx" | wc -l

# Find Scryber in project files
grep -r "Scryber" *.csproj **/*.csproj 2>/dev/null
```

### Uninstall / Install

```bash
# Remove Scryber.Core
dotnet remove package Scryber.Core

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
    ?? throw new InvalidOperationException("IRONPDF_LICENSE_KEY not set");
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using Scryber.Components;
using Scryber.PDF;
// Verify actual Scryber namespaces for your version
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic Conversion

**Before (Scryber.Core template approach):**
```csharp
using Scryber.Components;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // Load and process a .pdfx template
        // VERIFY: Scryber API method names for your version
        using var stream = File.OpenRead("Templates/Report.pdfx");
        var doc = PDFDocument.ParseDocument(stream);

        // Bind data parameters
        doc.Params["ReportTitle"] = "Q3 2024 Report";
        doc.Params["GeneratedDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Write to file
        using var outStream = File.Create("output.pdf");
        doc.ProcessDocument(outStream);
        Console.WriteLine("Saved output.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// HTML replaces .pdfx template — standard CSS applies
var html = $@"
    <html>
    <head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        h1 {{ font-size: 22px; }}
        .meta {{ color: #666; font-size: 12px; }}
    </style>
    </head>
    <body>
        <h1>Q3 2024 Report</h1>
        <div class='meta'>Generated: {DateTime.UtcNow:yyyy-MM-dd}</div>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| Scryber.Core | IronPDF | Notes |
|---|---|---|
| `Scryber.Components` | `IronPdf` | Core namespace |
| `Scryber.PDF` | `IronPdf.Rendering` | Rendering config |
| Scryber styles/CSS namespace | N/A — CSS in HTML | CSS handled via HTML `<style>` |

### Core Class Mapping

| Scryber.Core Class | IronPDF Class | Description |
|---|---|---|
| `PDFDocument` | `ChromePdfRenderer` | Primary rendering object |
| Template document model | `ChromePdfRenderOptions` | Rendering configuration |
| `doc.Params` / data model | HTML template string | Data binding mechanism |
| N/A | `PdfDocument` | PDF object for manipulation |

### Document Loading Methods

| Operation | Scryber.Core | IronPDF |
|---|---|---|
| Load .pdfx template | `PDFDocument.ParseDocument(stream)` | N/A — use HTML instead |
| Render HTML string | N/A | `renderer.RenderHtmlAsPdfAsync(html)` |
| Render URL | N/A | `renderer.RenderUrlAsPdfAsync(url)` |
| Load existing PDF | Verify | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | Scryber.Core | IronPDF |
|---|---|---|
| Page count | Verify | `pdf.PageCount` |
| Remove page | Verify | `pdf.RemovePage(index)` — verify |
| Extract text | Verify | `pdf.ExtractAllText()` |
| Rotate | Verify | Verify in IronPDF docs |

### Merge / Split Operations

| Operation | Scryber.Core | IronPDF |
|---|---|---|
| Merge | Verify native support | `PdfDocument.Merge(doc1, doc2)` |
| Split | Verify | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. Template to PDF

**Before (Scryber.Core):**
```csharp
using Scryber.Components;
using System;
using System.IO;

class TemplateToPdfBefore
{
    static void Main()
    {
        // Scryber template (.pdfx) — XML-based document model
        // VERIFY: all method and property names against Scryber documentation

        var templatePath = "Templates/Invoice.pdfx";
        using var stream = File.OpenRead(templatePath);

        var doc = PDFDocument.ParseDocument(stream);

        // Data binding via Params dictionary
        doc.Params["CustomerName"] = "Acme Corp";
        doc.Params["InvoiceNumber"] = "INV-2024-0042";
        doc.Params["TotalAmount"] = "$4,200.00";
        doc.Params["DueDate"] = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");

        using var outStream = File.Create("invoice.pdf");
        doc.ProcessDocument(outStream);

        Console.WriteLine("Saved invoice.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Data replaces Params dictionary
var customerName = "Acme Corp";
var invoiceNumber = "INV-2024-0042";
var totalAmount = "$4,200.00";
var dueDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");

var html = $@"
    <html>
    <head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        .header {{ font-size: 22px; font-weight: bold; }}
        .field {{ margin-top: 10px; font-size: 13px; }}
        .label {{ color: #666; width: 140px; display: inline-block; }}
        .total {{ font-size: 16px; font-weight: bold; margin-top: 20px; }}
    </style>
    </head>
    <body>
        <div class='header'>Invoice {invoiceNumber}</div>
        <div class='field'><span class='label'>Customer:</span> {customerName}</div>
        <div class='field'><span class='label'>Due Date:</span> {dueDate}</div>
        <div class='total'>Total: {totalAmount}</div>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");

Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (Scryber.Core — verify merge support):**
```csharp
using Scryber.Components;
using System;
using System.Collections.Generic;
using System.IO;

class MergeBefore
{
    static void Main()
    {
        // VERIFY: Scryber.Core merge capability
        // If not supported natively, teams add a secondary library

        var templates = new[] { "Section1.pdfx", "Section2.pdfx" };
        var pdfBytes = new List<byte[]>();

        foreach (var tmpl in templates)
        {
            using var stream = File.OpenRead(tmpl);
            var doc = PDFDocument.ParseDocument(stream);
            using var ms = new MemoryStream();
            doc.ProcessDocument(ms);
            pdfBytes.Add(ms.ToArray());
        }

        // Merge via secondary library if Scryber doesn't support it:
        // var merged = SomePdfLib.Merge(pdfBytes);
        // File.WriteAllBytes("merged.pdf", merged);

        Console.WriteLine("Verify Scryber merge support or use secondary library");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();

var results = await Task.WhenAll(
    renderer.RenderHtmlAsPdfAsync(BuildSection1Html()),
    renderer.RenderHtmlAsPdfAsync(BuildSection2Html())
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("combined.pdf");

Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (Scryber.Core — template watermark element or secondary library):**
```csharp
using Scryber.Components;
using System;
using System.IO;

class WatermarkBefore
{
    static void Main()
    {
        // Scryber.Core: watermark defined in the .pdfx template
        // OR applied post-generation via a secondary library

        // If template-based: requires editing the .pdfx XML to add a
        // positioned text element with opacity setting (verify PDFX syntax)

        // If programmatic post-generation (verify if Scryber supports it):
        // using var stream = File.OpenRead("Template.pdfx");
        // var doc = PDFDocument.ParseDocument(stream);
        // ... add watermark element programmatically? Verify API

        Console.WriteLine("Verify Scryber programmatic watermark API — may need secondary library");
    }
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Editing;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Draft Report</h1></body></html>"
);

// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronPdf.Imaging.Color.Gray,
    Opacity = 0.15,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (Scryber.Core — verify security API):**
```csharp
using Scryber.Components;
using System;
using System.IO;

class PasswordBefore
{
    static void Main()
    {
        // VERIFY: Scryber.Core password/security API
        // If not native, requires secondary library

        using var stream = File.OpenRead("Template.pdfx");
        var doc = PDFDocument.ParseDocument(stream);

        // Illustrative — verify actual Scryber security API:
        // doc.SecurityOptions.UserPassword = "open123";
        // doc.SecurityOptions.OwnerPassword = "admin456";

        using var outStream = File.Create("secured.pdf");
        doc.ProcessDocument(outStream);

        Console.WriteLine("Verify Scryber security API names in documentation");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Secured Document</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Troubleshooting Common Migration Issues

### CSS Property Not Rendering as Expected

**Symptom:** HTML that uses CSS flex or grid layouts renders differently after migrating from Scryber.Core.

**Cause:** Scryber.Core implements a CSS subset. Your existing templates may have been designed to work within those constraints. IronPDF uses Chromium, which implements current CSS standards.

**Resolution:** After migration, CSS limitations from Scryber become non-issues — flex, grid, custom properties all work. However, templates that were specifically limited to avoid Scryber's CSS gaps may look different (usually better) under Chromium. Review and simplify CSS where workarounds were added.

```csharp
// After migration, test your templates in Chrome browser with @media print styles:
// DevTools → More tools → Rendering → Emulate CSS media type → print
// IronPDF renders these print styles as Chrome does
// See: https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/
```

### Template Data Binding Gap

**Symptom:** Scryber's `doc.Params["key"] = value` pattern has no direct equivalent.

**Cause:** IronPDF takes HTML strings as input; it doesn't have a params dictionary.

**Resolution:** Several options depending on your preference:

```csharp
// Option 1: C# string interpolation (simplest)
var html = $"<h1>Invoice {invoiceNumber}</h1><p>Customer: {customerName}</p>";

// Option 2: Template engine (better for complex templates)
// Scriban (MIT): https://github.com/scriban/scriban
// var template = Template.Parse(File.ReadAllText("invoice.sbn"));
// var html = template.Render(new { invoiceNumber, customerName });

// Option 3: Razor (if already using ASP.NET Core)
// var html = await razorRenderer.RenderToStringAsync("Invoice", model);

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### .pdfx Template File Handling

**Symptom:** Existing `.pdfx` files have no IronPDF equivalent import path.

**Cause:** Scryber.Core uses a proprietary XML-based template format. IronPDF uses HTML.

**Resolution:** Each `.pdfx` file needs to be converted to an HTML template. There's no automated converter. The `.pdfx` XML structure gives you a visual reference for the layout to recreate in HTML:

```bash
# View a .pdfx file to understand the layout to recreate
cat Templates/Invoice.pdfx
# It's XML — identify the data fields and layout elements
# Then recreate as HTML/CSS with equivalent structure
```

### Font Not Appearing in Output

**Symptom:** A font specified in the template renders as a fallback in IronPDF output.

**Cause:** Scryber.Core has its own font registration system. IronPDF renders via Chromium, which uses system fonts or web fonts.

**Resolution:**
```csharp
// Option 1: Use system fonts available in your deployment environment
var html = "<html><head><style>body { font-family: Arial, sans-serif; }</style></head>...";

// Option 2: Embed via Google Fonts CDN
var html = @"
    <html>
    <head>
    <link href='https://fonts.googleapis.com/css2?family=Open+Sans' rel='stylesheet'>
    <style>body { font-family: 'Open Sans', sans-serif; }</style>
    </head>...";

// Option 3: Base64 embed for offline/air-gapped environments
// var fontBase64 = Convert.ToBase64String(File.ReadAllBytes("MyFont.woff2"));
// CSS: @font-face { font-family: MyFont; src: url(data:font/woff2;base64,...); }
```

### Page Size / Orientation Mismatch

**Symptom:** PDF page dimensions don't match the Scryber template's configured page size.

**Cause:** Scryber configures page size in the `.pdfx` template; IronPDF uses `RenderingOptions`.

**Resolution:**
```csharp
using IronPdf;
using IronPdf.Rendering;

var renderer = new ChromePdfRenderer();

// Map Scryber template page size to IronPDF enum
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;  // or Letter, Legal, etc.
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait; // verify enum

// For custom sizes: verify in IronPDF docs
// See: https://ironpdf.com/how-to/rendering-options/
```

---

## Critical Migration Notes

### Template File Count Drives Scope

The number of `.pdfx` files is the single number that determines migration effort. Each one must be converted to an HTML template:

```bash
find . -name "*.pdfx" | wc -l
```

A simple single-column `.pdfx` typically converts to HTML in 20–30 minutes. Complex multi-column layouts may take longer.

### Page Indexing

IronPDF uses 0-based page indexing. Verify Scryber's indexing in their documentation — any page manipulation code needs to be audited for off-by-one adjustments.

### ProcessDocument Stream Pattern

Scryber's `ProcessDocument(stream)` pattern maps to IronPDF's stream output:

```csharp
// Scryber: doc.ProcessDocument(stream) writes to provided stream
// IronPDF: access pdf.Stream or pdf.BinaryData after rendering

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// Write to existing stream
pdf.Stream.CopyTo(existingStream);

// Or get bytes
var bytes = pdf.BinaryData;
```

---

## Performance Considerations

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/examples/parallel/
var htmlJobs = templateDataList.Select(data => BuildHtml(data)).ToArray();

var pdfs = await Task.WhenAll(htmlJobs.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Rendered {pdfs.Length} PDFs in parallel");
foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// Scryber used MemoryStream passed into ProcessDocument
// IronPDF: use pdf.BinaryData or pdf.Stream
using var ms = new MemoryStream();
pdf.Stream.CopyTo(ms);
return ms.ToArray();
// pdf disposed at end of 'using' block
```

---

## Migration Checklist

### Pre-Migration
- [ ] Count `.pdfx` template files (`find . -name "*.pdfx" | wc -l`)
- [ ] Categorize templates by complexity and data binding depth
- [ ] Identify secondary libraries used alongside Scryber (merge, security, etc.)
- [ ] Document current page sizes, orientations, and font requirements
- [ ] Choose template engine for data binding (string interpolation, Scriban, Razor)
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility
- [ ] Review open Scryber.Core issues relevant to your use cases

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove `Scryber.Core` package reference
- [ ] Add license key at application startup
- [ ] Convert each `.pdfx` template to an HTML template
- [ ] Replace `doc.Params["key"] = value` binding with chosen template approach
- [ ] Replace `PDFDocument.ParseDocument()` with `ChromePdfRenderer`
- [ ] Replace `doc.ProcessDocument(stream)` with `pdf.Stream` / `pdf.SaveAs()`
- [ ] Migrate CSS from Scryber format to standard HTML/CSS
- [ ] Replace secondary merge library with `PdfDocument.Merge()`
- [ ] Replace secondary security library with `pdf.SecuritySettings`

### Testing
- [ ] Render each HTML template and compare against Scryber reference output
- [ ] Verify all data binding fields are present and correctly formatted
- [ ] Test page sizes and margins match reference
- [ ] Verify font rendering in deployment environment
- [ ] Test password protection
- [ ] Test merge output
- [ ] Run under concurrent load — verify no thread safety issues

### Post-Migration
- [ ] Remove `Scryber.Core` NuGet package
- [ ] Archive `.pdfx` files for reference (don't delete immediately)
- [ ] Remove secondary PDF libraries now replaced by IronPDF
- [ ] Update documentation — note template engine choice for future maintainers

---

## The Bottom Line

The `.pdfx` template count and complexity is the key variable before scoping this migration. The API swap is straightforward; the template rewrite is the time investment. Running the `find . -name "*.pdfx"` count and categorizing templates by complexity is the highest-value pre-migration step.

For the data binding gap, using a lightweight template engine like Scriban or Fluid gives you a separation of template and code that's closer to Scryber's model than raw string interpolation.

**Discussion question:** What edge cases did you hit that this article didn't cover — particularly around Scryber's CSS subset behavior or the data binding model conversion?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
