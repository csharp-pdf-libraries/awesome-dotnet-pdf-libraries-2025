# From XFINIUM.PDF to IronPDF: what actually changes in your code

Two major versions separate where you are from where the documentation is. XFINIUM.PDF shipped breaking API changes between versions, the upgrade guide is incomplete, and the version you're on has a known rendering issue with complex table layouts that the newer version supposedly fixes — but nobody on the team has validated the upgrade because the last time someone tried it, three things broke and the sprint got redirected. Staying on the old version is a tax. Upgrading is an unknown risk. Switching is on the table.

This article covers migrating from XFINIUM.PDF to IronPDF. You'll have working before/after code for HTML-to-PDF, merge, watermark, and password protection by the end. The troubleshooting section covers the patterns that commonly surface during this migration, and the checklist applies regardless of the path you take.

---

## Why Migrate (Without Drama)

Teams evaluating XFINIUM.PDF alternatives commonly cite:

1. **Version pinning friction** — XFINIUM.PDF has had breaking changes between major versions; upgrade paths can be non-trivial.
2. **HTML input gap** — XFINIUM.PDF creates PDFs from a document model, not HTML; HTML-to-PDF requires a conversion step.
3. **Document model verbosity** — layout-heavy PDF generation via the drawing API requires explicit coordinate positioning.
4. **CSS/web content source** — teams whose content starts as HTML templates must convert to the XFINIUM document model, adding intermediate steps.
5. **Licensing model** — verify commercial use terms at xfinium.com for your deployment scale.
6. **Community support** — XFINIUM has dedicated documentation but a smaller community Q&A footprint than larger ecosystems.
7. **Missing features vs version** — features available in newer XFINIUM versions may require an upgrade that introduces breaking changes.
8. **.NET version compatibility** — verify .NET 6/7/8/9 support for your specific XFINIUM version.
9. **No native PDF manipulation** in the HTML path — adding merge, watermark, or security to HTML-generated PDFs requires the same API used for document creation.
10. **Team familiarity** — teams with web backgrounds find HTML/CSS templates easier to maintain than drawing API code.

### Comparison Table

| Aspect | XFINIUM.PDF | IronPDF |
|---|---|---|
| Focus | PDF creation/editing via document model | HTML-to-PDF + PDF manipulation |
| Pricing | Commercial — verify at xfinium.com | Commercial — verify at ironsoftware.com |
| API Style | Document model + drawing API | `ChromePdfRenderer` + HTML input |
| Learning Curve | Medium — own document model | Low for web devs; HTML/CSS is the input |
| HTML Rendering | Not applicable | Embedded Chromium |
| Page Indexing | Verify in XFINIUM docs | 0-based |
| Thread Safety | Verify in XFINIUM docs | Verify IronPDF concurrent instance guidance |
| Namespace | `Xfinium.Pdf.*` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | XFINIUM.PDF | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | Not applicable | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low (new capability) |
| Document model → PDF | Drawing API (`PdfPage`, `PdfGraphics`) | HTML/CSS template | High (rewrite) |
| Save to file | `doc.Save(path)` | `pdf.SaveAs(path)` | Low |
| Save to stream | `doc.Save(stream)` | `pdf.Stream` / `pdf.BinaryData` | Low |
| Load existing PDF | `PdfFixedDocument(path)` | `PdfDocument.FromFile(path)` | Low |
| Merge PDFs | `doc.AppendPage()` or similar | `PdfDocument.Merge()` | Low–Medium |
| Watermark | Via graphics layer | `TextStamper` / `ImageStamper` | Low |
| Password protection | `PdfStandardSecurity` | `pdf.SecuritySettings` | Low |
| Text extraction | `PdfContentExtractionSink` | `pdf.ExtractAllText()` | Low |
| Form fields | XFINIUM form API | Verify IronPDF form fill API | Medium |
| Annotations | XFINIUM annotations API | Verify IronPDF annotations | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Stuck on old version due to API breaking changes | Switch resolves the version-pinning constraint |
| HTML-to-PDF from web templates is the primary need | Switch — XFINIUM is document-first; IronPDF is HTML-first |
| Extensive programmatic drawing/form usage | Evaluate migration cost — drawing API → HTML rewrite |
| .NET upgrade blocked by XFINIUM compatibility | Verify at xfinium.com; switch if not supported |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All XFINIUM.PDF References

```bash
# Find XFINIUM API usage (verify namespace in your version)
rg -l "Xfinium\|XfiniumPdf\|PdfFixedDocument\|PdfGraphics\b" --type cs
rg "Xfinium\|PdfFixedDocument\|PdfGraphics" --type cs -n

# Count drawing API vs save usage
rg "PdfGraphics\|DrawString\|DrawLine\|DrawRect" --type cs | wc -l
rg "\.Save\(|\.Load\(|PdfFixedDocument" --type cs -n

# Find XFINIUM packages
grep -r "Xfinium\|XfiniumPdf" *.csproj **/*.csproj 2>/dev/null

# Capture all method calls before removing anything
rg "Xfinium\.\|PdfFixed\|PdfGraphics\|PdfStandardSecurity" --type cs -n > xfinium-usage.txt
```

### Uninstall / Install

```bash
# Remove XFINIUM packages (verify exact names at NuGet)
dotnet remove package XfiniumPdf  # verify exact package name

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
using Xfinium.Pdf;          // verify exact namespace
using Xfinium.Pdf.Graphics; // verify
using Xfinium.Pdf.Security; // verify
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
```

### Step 3 — Basic PDF Generation

**Before (XFINIUM document model — verify all API names):**
```csharp
using Xfinium.Pdf;          // VERIFY
using Xfinium.Pdf.Graphics; // VERIFY
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // VERIFY: all XFINIUM class and method names against your version
        var doc = new PdfFixedDocument();
        var page = doc.Pages.Add();

        var font = new PdfStandardFont(PdfStandardFontName.Helvetica, 24); // VERIFY
        var brush = new PdfBrush(PdfRgbColor.Black); // VERIFY

        var graphics = page.Graphics;
        graphics.DrawString("Hello, PDF!", font, brush, 40, 40); // VERIFY

        using var stream = File.Create("output.pdf");
        doc.Save(stream); // VERIFY
        Console.WriteLine("Saved output.pdf — verify all XFINIUM API names");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// HTML replaces drawing API — no coordinate positioning
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body style='font-family:Arial; padding:40px'><h1>Hello, PDF!</h1></body></html>"
);
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| XFINIUM.PDF | IronPDF | Notes |
|---|---|---|
| `Xfinium.Pdf` | `IronPdf` | Core namespace |
| `Xfinium.Pdf.Graphics` | N/A — not needed for HTML path | Drawing replaced by HTML/CSS |
| `Xfinium.Pdf.Security` | `IronPdf` (security on PdfDocument) | Security on output object |

### Core Class Mapping

| XFINIUM Class | IronPDF Class | Description |
|---|---|---|
| `PdfFixedDocument` | `ChromePdfRenderer` | Different model — HTML is the input |
| `PdfPage` / page graphics | N/A | Drawing API replaced by HTML |
| `PdfStandardSecurity` | `pdf.SecuritySettings` | Password + permission settings |
| N/A | `PdfDocument` | PDF manipulation (load/merge/stamp) |

### Document Loading Methods

| Operation | XFINIUM | IronPDF |
|---|---|---|
| Create from model | `new PdfFixedDocument()` + pages | `renderer.RenderHtmlAsPdfAsync(html)` |
| Load existing PDF | `new PdfFixedDocument(path)` | `PdfDocument.FromFile(path)` |
| Save to file | `doc.Save(path)` | `pdf.SaveAs(path)` |
| Save to stream | `doc.Save(stream)` | `pdf.Stream.CopyTo(stream)` |

### Page Operations

| Operation | XFINIUM | IronPDF |
|---|---|---|
| Page count | `doc.Pages.Count` | `pdf.PageCount` |
| Remove page | `doc.Pages.RemoveAt(i)` | `pdf.RemovePage(index)` — verify |
| Extract text | `PdfContentExtractionSink` | `pdf.ExtractAllText()` |
| Rotate | Verify | Verify in IronPDF docs |

### Merge / Split Operations

| Operation | XFINIUM | IronPDF |
|---|---|---|
| Merge | `doc.AppendPage()` / clone | `PdfDocument.Merge(doc1, doc2)` |
| Split | Verify | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF (New Capability)

**Before (XFINIUM — document model; HTML input not native):**
```csharp
using Xfinium.Pdf;           // VERIFY
using Xfinium.Pdf.Graphics;  // VERIFY
using System;
using System.IO;
using System.Text;

class ReportBefore
{
    static void Main()
    {
        // XFINIUM: build document model — no HTML input
        // All names below are illustrative — VERIFY at xfinium.com

        var doc = new PdfFixedDocument();
        var page = doc.Pages.Add();
        var graphics = page.Graphics;

        var titleFont = new PdfStandardFont(PdfStandardFontName.HelveticaBold, 18);
        var bodyFont  = new PdfStandardFont(PdfStandardFontName.Helvetica, 12);
        var brush     = new PdfBrush(PdfRgbColor.Black);

        // Manual positioning for each element
        graphics.DrawString("Q3 Invoice #4421", titleFont, brush, 40, 40);
        graphics.DrawString("Customer: Acme Corp",    bodyFont, brush, 40, 80);
        graphics.DrawString("Total Due: $8,400.00",   bodyFont, brush, 40, 110);

        using var stream = File.Create("invoice.pdf");
        doc.Save(stream);
        Console.WriteLine("Saved invoice.pdf — verify all XFINIUM API names");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// HTML/CSS handles layout — no manual coordinate math
var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; }
        h1 { font-size: 20px; }
        .field { margin-top: 8px; font-size: 13px; }
        .total { font-weight: bold; font-size: 16px; margin-top: 20px; }
    </style>
    </head>
    <body>
        <h1>Q3 Invoice #4421</h1>
        <div class='field'>Customer: Acme Corp</div>
        <div class='total'>Total Due: $8,400.00</div>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");
Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (XFINIUM — verify merge API):**
```csharp
using Xfinium.Pdf;  // VERIFY
using System;
using System.IO;

class MergeBefore
{
    static void Main()
    {
        // VERIFY: XFINIUM merge API — may be AppendPage, AppendDocument, or Clone
        // All names below are illustrative

        using var s1 = File.OpenRead("section1.pdf");
        var doc1 = new PdfFixedDocument(s1); // VERIFY

        using var s2 = File.OpenRead("section2.pdf");
        var doc2 = new PdfFixedDocument(s2); // VERIFY

        // VERIFY: XFINIUM page append/merge API
        // for (int i = 0; i < doc2.Pages.Count; i++)
        //     doc1.Pages.Add(doc2.Pages[i]); // verify method

        using var outStream = File.Create("merged.pdf");
        doc1.Save(outStream); // VERIFY
        Console.WriteLine("Merged — verify XFINIUM page append API");
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
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 1</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 2</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (XFINIUM — drawing layer approach):**
```csharp
using Xfinium.Pdf;           // VERIFY
using Xfinium.Pdf.Graphics;  // VERIFY
using System;
using System.IO;

class WatermarkBefore
{
    static void Main()
    {
        // XFINIUM: apply watermark by drawing semi-transparent text on each page
        // VERIFY: all API names, especially transparency/opacity support

        using var s = File.OpenRead("source.pdf");
        var doc = new PdfFixedDocument(s); // VERIFY

        foreach (var page in doc.Pages)
        {
            var graphics = page.Graphics;
            // VERIFY: XFINIUM transparency API for watermark text
            var font  = new PdfStandardFont(PdfStandardFontName.Helvetica, 60); // VERIFY
            var brush = new PdfBrush(new PdfRgbColor(0.5, 0.5, 0.5)); // VERIFY alpha

            graphics.SaveGraphicsState();
            // VERIFY: translate + rotate transform API
            graphics.DrawString("DRAFT", font, brush, 200, 350); // VERIFY
            graphics.RestoreGraphicsState();
        }

        using var outStream = File.Create("watermarked.pdf");
        doc.Save(outStream);
        Console.WriteLine("Verify XFINIUM watermark API — transparency support may vary by version");
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
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Document</h1></body></html>");

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

**Before (XFINIUM security API — verify):**
```csharp
using Xfinium.Pdf;           // VERIFY
using Xfinium.Pdf.Security;  // VERIFY namespace
using System;
using System.IO;

class PasswordBefore
{
    static void Main()
    {
        var doc = new PdfFixedDocument(); // VERIFY
        doc.Pages.Add();
        // ... build document ...

        // VERIFY: XFINIUM security API — class and property names vary by version
        var security = new PdfStandardSecurity(); // VERIFY class name
        security.UserPassword = "open123";         // VERIFY property
        security.OwnerPassword = "admin456";       // VERIFY property
        // security.Permissions = ...;             // VERIFY permissions API
        doc.Security = security;                   // VERIFY assignment

        using var stream = File.Create("secured.pdf");
        doc.Save(stream);
        Console.WriteLine("Saved secured.pdf — verify XFINIUM security API names");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Protected</h1></body></html>");

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Troubleshooting Common Migration Issues

### "Version breaking changes — which API names are correct?"

**Symptom:** `CS0117` or `CS1061` errors after switching between XFINIUM versions; method names in old code don't exist in new version.

**Diagnosis:** XFINIUM has had API surface changes between major versions. Before migration, capture the exact version in use and map the exact method signatures:

```bash
# Find installed XFINIUM version
grep -r "XfiniumPdf\|Xfinium" *.csproj **/*.csproj 2>/dev/null

# List the package and its version
dotnet list package | grep -i xfinium

# Save current method usage to file for reference during migration
rg "Xfinium\.\|PdfFixed\|PdfGraphics\." --type cs -n > xfinium-methods-in-use.txt
cat xfinium-methods-in-use.txt
```

**Resolution:** Before removing XFINIUM, document every method call. Each one becomes a migration point. The IronPDF equivalent is HTML for drawing API calls; the API calls map closely for save/load/security operations.

### "Drawing API code is too large to rewrite immediately"

**Symptom:** The codebase has dozens of `graphics.DrawString`, `DrawRectangle`, `FillPath` calls that all need to become HTML/CSS. The migration scope is larger than the sprint allows.

**Resolution:** Migrate in phases — start with the HTML generation path and leave the drawing API code in place temporarily with both libraries installed:

```bash
# Phase 1: Identify which code paths generate from HTML vs drawing API
rg "DrawString\|DrawRect\|FillPath" --type cs -n > drawing-api-sites.txt
wc -l drawing-api-sites.txt  # migration scope for phase 2

# Phase 2: Migrate HTML-first paths (new capability)
# Phase 3: Migrate drawing API paths to HTML templates (over subsequent sprints)
```

### "Text extraction behavior differs"

**Symptom:** After migrating text extraction from `PdfContentExtractionSink` to `pdf.ExtractAllText()`, the extracted text differs in whitespace, order, or Unicode handling.

**Diagnosis:**

```csharp
using IronPdf;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Compare extraction results
var pdf = PdfDocument.FromFile("source.pdf");
var allText = pdf.ExtractAllText();

// For per-page extraction, verify IronPDF page-level API in docs:
// https://ironpdf.com/how-to/extract-text-and-images/
Console.WriteLine(allText);
```

Text extraction output varies by library implementation. If exact text positioning matters, verify what the new output produces before replacing any downstream logic that parses extracted text.

### "Save API differs — stream handling changed"

**Symptom:** Code that called `doc.Save(stream)` and then continued using the stream doesn't work after migration.

**Resolution:**

```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// XFINIUM: doc.Save(stream) writes to stream and keeps it open
// IronPDF: pdf.Stream is a MemoryStream; copy it to your output stream

// Pattern 1 — copy to existing stream:
pdf.Stream.CopyTo(yourOutputStream);

// Pattern 2 — get bytes:
var bytes = pdf.BinaryData;

// Pattern 3 — save to file:
pdf.SaveAs("output.pdf");

// pdf disposed at end of 'using' block — copy bytes BEFORE disposal
```

---

## Critical Migration Notes

### XFINIUM Is Document-First — IronPDF Is HTML-First

The architectural shift: XFINIUM builds PDFs from a document model (pages, graphics, fonts, coordinates). IronPDF renders PDFs from HTML. These are different mental models.

For teams whose PDF content originates from:
- **HTML/Razor templates** → IronPDF is the natural fit; migration is primarily removing XFINIUM's intermediate step
- **Data-driven drawing code** → HTML rewrite is required; scope the effort before committing

### Page Indexing

Both XFINIUM and IronPDF use 0-based page indexing. This typically doesn't require code changes.

### `doc.Save()` → `pdf.SaveAs()` Pattern

The save pattern shifts from stream-centric to object-centric:

```csharp
// XFINIUM (stream-centric):
// using var stream = File.Create("output.pdf");
// doc.Save(stream);

// IronPDF (object-centric):
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("output.pdf");
// OR: pdf.Stream.CopyTo(yourStream);
// pdf disposed by 'using'
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
var pdfs = await Task.WhenAll(reportDataList.Select(async data =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(BuildHtml(data));
}));

foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Disposal Pattern

```csharp
using IronPdf;

// XFINIUM: dispose via 'using' block on PdfFixedDocument
// IronPDF: dispose via 'using' block on PdfDocument

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("output.pdf");
// disposed automatically
```

---

## Migration Checklist

### Pre-Migration
- [ ] Find all XFINIUM usage (`rg "Xfinium\|PdfFixed\|PdfGraphics" --type cs`)
- [ ] Count drawing API vs save/load calls (scope drawing API rewrite)
- [ ] Document exact XFINIUM version in use
- [ ] Capture all method signatures used (`rg` to file)
- [ ] Identify secondary libraries used alongside XFINIUM
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility
- [ ] Note page indexing in existing XFINIUM code

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove XFINIUM NuGet packages
- [ ] Add license key at application startup
- [ ] Replace HTML generation paths with `ChromePdfRenderer.RenderHtmlAsPdfAsync()`
- [ ] Convert drawing API code to HTML/CSS templates
- [ ] Replace `doc.Save(path)` with `pdf.SaveAs(path)`
- [ ] Replace `doc.Save(stream)` with `pdf.Stream.CopyTo(stream)` or `pdf.BinaryData`
- [ ] Replace `new PdfFixedDocument(path)` with `PdfDocument.FromFile(path)`
- [ ] Replace XFINIUM merge with `PdfDocument.Merge()`
- [ ] Replace `PdfStandardSecurity` with `pdf.SecuritySettings`

### Testing
- [ ] Compare PDF output visually against XFINIUM reference
- [ ] Verify text extraction output if used
- [ ] Test page indexing in page manipulation code
- [ ] Test merge output page count and order
- [ ] Test password protection
- [ ] Benchmark render time vs XFINIUM baseline
- [ ] Test in Docker/Linux if applicable

### Post-Migration
- [ ] Remove XFINIUM NuGet packages
- [ ] Remove secondary libraries replaced by IronPDF
- [ ] Archive XFINIUM drawing API code for reference
- [ ] Update deployment docs — no XFINIUM license management needed

---

## Conclusion

The version pinning constraint — staying on an old version to avoid breaking changes while watching features accumulate in the newer version — is a common forcing function. Migrating to a different library removes that constraint and resets the version timeline.

The drawing API scope is the variable that determines timeline. Document model code that draws shapes and places text needs HTML equivalents; the more of it there is, the longer the migration takes. HTML input paths (if any) migrate in hours.

**Discussion question:** What edge cases did you hit that this article didn't cover — particularly around XFINIUM's text extraction API, form field handling, or specific drawing operations that needed a non-obvious HTML equivalent?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
