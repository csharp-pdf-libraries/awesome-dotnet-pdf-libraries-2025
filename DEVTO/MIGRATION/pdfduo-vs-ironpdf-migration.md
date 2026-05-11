---
title: "PDF Duo .NET to IronPDF: an honest migration walkthrough"
published: false
tags: dotnet, csharp, pdf, migration
---

Renewal season has a way of making you look at the dependency list with fresh eyes. The PDF generation library works, but "works" is doing a lot of heavy lifting — you have a workaround for the encoding issue, a secondary library handling merge, and a comment in the code about a rendering quirk that nobody has touched in months. Then the renewal cost lands on your desk, and suddenly the migration question that has been in the backlog feels timely.

This article walks through migrating from PDF Duo .NET (DuoDimension Software) to IronPDF — what changes in your code, where to expect friction, and how to troubleshoot the common breakage points. PDF Duo .NET is a niche HTML-to-PDF component whose last public release (v2.4) shipped in December 2010, targets .NET Framework 1.1 through 3.5 only, and is distributed as a DLL download rather than a NuGet package. That alone makes most of the migration mechanical — there is no `dotnet remove package` step on the way out — but the API mapping and the feature gaps are where the real work lives.

---

## Troubleshooting: what typically breaks during a license-trigger migration

When cost or renewal is the trigger, the migration often starts at the wrong end — you are in the "replace it" mindset before you have done the "what does it actually do" audit. That audit prevents surprises.

### Step 1: Inventory the actual usage

```bash
# What PDF Duo .NET usage exists in your codebase?
rg "DuoDimension|HtmlToPdf\b|OpenHTML|SavePDF" --type cs -n

# Find all PDF-generating code paths
rg "class.*Pdf|\.pdf|GeneratePdf|CreatePdf|RenderPdf" --type cs -n -i

# Find secondary libraries paired with PDF Duo .NET (typically iTextSharp 4.x
# for the merge/manipulation gap)
grep -r "iTextSharp\|PdfSharp\|wkhtmltopdf\|MuPdf" **/*.csproj *.csproj 2>/dev/null
```

Build a table before writing migration code:

| PDF operation | Used by | Secondary library? |
|---|---|---|
| HTML-to-PDF | OrderService.cs, ReportService.cs | No |
| Merge | InvoiceBundle.cs | Yes — iTextSharp 4.x |
| Watermark | DraftPdf.cs | Yes — iTextSharp 4.x |
| Password | SecureReport.cs | Yes — iTextSharp 4.x |

This table tells you the actual migration scope and whether you can simplify the dependency tree. PDF Duo .NET only generates from HTML — anything beyond that was almost certainly handled by a second library, and IronPDF can usually absorb both.

### Step 2: Reproduce the workarounds before removing PDF Duo

If your codebase has workarounds (encoding fixes, retry logic, CSS patches), document what they address before migrating. Some workarounds are renderer-specific and disappear when you switch. Others reveal bugs in your data or templates that will follow you to the new library.

```bash
# Find workaround comments
rg "TODO.*pdf|FIXME.*pdf|workaround.*pdf|hack.*pdf" --type cs -n -i

# Find encoding patches
rg "Encoding|charset|utf" --type cs -n | grep -i pdf
```

---

## Why migrate (without drama)

Eight neutral reasons teams move off PDF Duo .NET at renewal:

1. **Cost structure reassessment** — at renewal, the per-developer or per-deployment cost is compared against alternatives. No pricing analysis here — do this yourself.
2. **Maintenance signal** — PDF Duo .NET's most recent public release (v2.4) is dated December 10, 2010 per the vendor's own download listing. Roughly fifteen years without a new version is a legitimate maintenance concern.
3. **Feature gaps** — PDF Duo's documented surface is essentially `HtmlToPdf` with `OpenHTML(...)` and `SavePDF(...)`. There is no documented native API for watermarking, password protection, digital signatures, form filling, text extraction, PDF/A, or PDF merging.
4. **Dependency sprawl** — because PDF Duo only generates, most teams that needed merge, watermark, or security paired it with iTextSharp 4.x. Consolidating both into one library is a valid motivation on its own.
5. **Rendering engine** — the vendor describes PDF Duo as a self-contained component but does not publish which HTML/CSS engine it uses. CSS3, modern JavaScript, web fonts, and flex/grid layout are not claimed by the vendor.
6. **No NuGet distribution** — PDF Duo .NET is shipped as a DLL download from duodimension.com. There is no `dotnet add package` story, no SemVer, no transitive dependency resolution. That is a deployment-pipeline problem in 2026.
7. **.NET version compatibility** — the vendor lists supported runtimes as .NET Framework 1.1 through 3.5 on Windows XP / Vista / 7 / 2000 / 2003. There is no documented support for .NET Framework 4.x, .NET Core, .NET 5+, Linux, or 64-bit-clean modern builds.
8. **Vendor lock-in via undocumented surface area** — when a component has one product page and no API reference site, "the API" is whatever the binary happens to expose. Migrating to a library with a published API reference is a way out of that coupling.

### Comparison table

| Aspect | PDF Duo .NET | IronPDF |
|---|---|---|
| Focus | HTML-to-PDF only | HTML-to-PDF + PDF manipulation |
| Last release | v2.4 (December 2010) | Active, regular releases |
| Distribution | DLL download from duodimension.com | NuGet `IronPdf` |
| Runtime support | .NET Framework 1.1 - 3.5, Windows | .NET FX 4.6.2+, .NET 6/7/8/9/10, Linux, macOS, Docker |
| API surface | `DuoDimension.HtmlToPdf` (`OpenHTML`, `SavePDF`) | `ChromePdfRenderer`, `PdfDocument`, security, editing |
| HTML rendering | Engine not disclosed by vendor | Chromium-based |
| Namespace | `DuoDimension` | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | PDF Duo .NET approach | Effort to migrate |
|---|---|---|
| HTML string to PDF | Write to temp file, then `OpenHTML(path)` | Low |
| URL to PDF | `OpenHTML(url)` | Low |
| HTML file to PDF | `OpenHTML(path)` | Low |
| Custom margins | Not in documented API | Low (new feature in IronPDF) |
| Headers / footers | Not in documented API | Low (new feature in IronPDF) |
| Merge PDFs | Not native — secondary library | Low |
| Watermark | Not native — secondary library | Low |
| Password protection | Not native — secondary library | Low |
| Async rendering | Not documented | Medium |
| PDF/A compliance | Not in documented API | Low in IronPDF |
| Digital signatures | Not in documented API | Medium |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| Cost is only trigger, PDF Duo works well | Evaluate total cost of migration vs continued use — but note the 2010 last-release date |
| Dependency sprawl (iTextSharp 4.x alongside PDF Duo) | IronPDF can usually absorb both — evaluate feature overlap |
| Pinned to .NET Framework 1.1–3.5 | Migration unblocks .NET 6/7/8/9/10 and Linux/Docker targets |
| Open source budget requirement | IronPDF is commercial — evaluate wkhtmltopdf wrappers, PuppeteerSharp |

---

## Before you start

### Prerequisites

- A modern .NET target — IronPDF supports .NET Framework 4.6.2+, .NET 6/7/8/9/10
- Any PDF Duo .NET license documentation you still have (helpful when reading old code)
- All HTML templates for render comparison testing

### Find PDF Duo references in your codebase

```bash
# Find all PDF Duo .NET usage (vendor namespace is DuoDimension)
rg -l "DuoDimension|HtmlToPdf\b|OpenHTML|SavePDF" --type cs

# Find class instantiation
rg "new\s+(DuoDimension\.)?HtmlToPdf" --type cs -n

# Find using directives
rg "using DuoDimension" --type cs -n

# PDF Duo .NET is NOT a NuGet package — there is nothing to grep for in
# .csproj files unless someone vendored the DLL alongside a NuGet manifest.
# Check the project's /lib or /bin references manually for PDFDuo.dll.
```

### Remove PDF Duo .NET, install IronPDF

```bash
# PDF Duo .NET is not on NuGet — removal is a manual step:
# In Visual Studio: References -> remove PDFDuo.dll (and any associated
# files such as PDFDuoNET.dll). Delete the binary from your /lib folder
# if you vendored it.

# Install IronPDF
dotnet add package IronPdf
dotnet restore
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (PDF Duo .NET):**
```csharp
// PDF Duo .NET has no documented runtime license API in its public surface.
// Licensing was handled at install/download time by the vendor — there is no
// LicenseKey property or Set() call to migrate.
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
using DuoDimension;
// PDF Duo .NET exposes a single documented root namespace. Most code that
// uses the library only ever imports DuoDimension.
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
```

### Step 3: Basic HTML-to-PDF

**Before:**
```csharp
using DuoDimension;
using System.IO;

class BasicConversionExample
{
    static void Main()
    {
        // PDF Duo's OpenHTML accepts a file path, URL, or HTML written to disk first.
        // There is no documented "convert HTML string to PDF" call, so the typical
        // pattern is: write to a temp file, OpenHTML, SavePDF.
        var tempHtml = Path.GetTempFileName() + ".html";
        File.WriteAllText(tempHtml, "<h1>Hello World</h1>");

        var conv = new HtmlToPdf();
        conv.OpenHTML(tempHtml);
        conv.SavePDF("output.pdf");
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
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Troubleshooting: common migration failure patterns

### Problem: "Encoding issues followed me to the new library"

Encoding problems in PDF generation are often in the HTML source or the rendering pipeline, not the library:

```csharp
// Ensure HTML is explicitly UTF-8
string html = @"<html>
<head><meta charset='utf-8' /></head>
<body><p>Content with special chars: ñ, ü, 中文</p></body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
// Chromium handles UTF-8 natively — no special encoding flag needed

// If rendering from a file, set encoding at read time:
// string html = File.ReadAllText("template.html", System.Text.Encoding.UTF8);
```

### Problem: "Margin/page-size settings have no equivalent"

PDF Duo .NET does not document a settings object on `HtmlToPdf` covering paper size, orientation, or margins. IronPDF exposes them on `RenderingOptions`:

```csharp
// IronPDF options — configure before rendering
var renderer = new ChromePdfRenderer();
var opts = renderer.RenderingOptions;

opts.PaperSize         = IronPdf.Rendering.PdfPaperSize.A4;
opts.MarginTop         = 10;   // mm
opts.MarginBottom      = 10;
opts.MarginLeft        = 15;
opts.MarginRight       = 15;
opts.PaperOrientation  = IronPdf.Rendering.PdfPaperOrientation.Portrait;
// All rendering options: https://ironpdf.com/how-to/rendering-options/
```

### Problem: "Secondary library (iTextSharp 4.x) is still needed for merge/watermark/security"

After migrating HTML-to-PDF to IronPDF, audit whether the secondary library is still pulling its weight:

```csharp
// Before migration:
// PDF Duo .NET    -> HTML-to-PDF generation
// iTextSharp 4.x  -> merge, watermark, password protection

// After:
// IronPDF -> generation + merge + watermark + security
// The secondary library can usually be removed.

// IronPDF merge:
var merged = PdfDocument.Merge(
    PdfDocument.FromFile("doc1.pdf"),
    PdfDocument.FromFile("doc2.pdf")
);
merged.SaveAs("merged.pdf");
```

### Problem: "Output model changed — PDF Duo wrote to disk, IronPDF gives me a PdfDocument"

PDF Duo .NET's `SavePDF(path)` always writes to a file path; there is no documented in-memory byte API. IronPDF returns a `PdfDocument` object that you can save, stream, or convert to bytes:

```csharp
// Old pattern: conv.SavePDF("output.pdf");

// IronPDF returns PdfDocument:
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf"); // to disk

// To byte[]:
byte[] pdfBytes = pdf.BinaryData;

// To HTTP response (ASP.NET):
return File(pdf.Stream, "application/pdf", "document.pdf");
```

### Problem: "CSS renders differently after migration"

PDF Duo .NET does not publish its rendering engine, so the rendering delta from migrating to IronPDF's Chromium engine is hard to predict template-by-template. Systematic approach:

```bash
# 1. Render all templates with PDF Duo .NET (save as "baseline_*.pdf")
# 2. Render all templates with IronPDF (save as "new_*.pdf")
# 3. Diff visually — or use an image comparison tool

# Example bash loop for batch render comparison:
for template in templates/*.html; do
  base=$(basename "$template" .html)
  echo "Comparing: $base"
  # Use your PDF-to-image tool here, then image diff
done
```

Priority areas to check after switching to Chromium:

| CSS feature | Risk level |
|---|---|
| Flexbox / Grid | High — PDF Duo predates these by years |
| CSS variables | High — same reason |
| `@media print` | Medium |
| Background colors | Medium — PDF Duo's print defaults are undocumented |
| Custom fonts | Medium — font resolution differs between engines |
| Table layout | Low-Medium — test complex tables |
| `page-break-*` CSS | Low — standard behavior |

---

## API mapping tables

### Namespace mapping

| PDF Duo .NET | IronPDF | Notes |
|---|---|---|
| `DuoDimension` | `IronPdf` | Core |
| _(no documented sub-namespaces)_ | `IronPdf.Rendering` | Configuration types |
| _(no documented sub-namespaces)_ | `IronPdf.Editing` | Manipulation |

### Core class mapping

| PDF Duo .NET class | IronPDF class | Description |
|---|---|---|
| `DuoDimension.HtmlToPdf` | `ChromePdfRenderer` | HTML-to-PDF entry point |
| _(no options object)_ | `ChromePdfRenderOptions` | Render configuration |
| _(no document model)_ | `PdfDocument` | PDF document model |
| _(no document model)_ | `PdfDocument` static | Merge, split, load |

### Document loading methods

| Operation | PDF Duo .NET | IronPDF |
|---|---|---|
| HTML string | Write to temp file, then `OpenHTML(path)` | `renderer.RenderHtmlAsPdf(html)` |
| URL | `conv.OpenHTML(url)` | `renderer.RenderUrlAsPdf(url)` |
| HTML file | `conv.OpenHTML(path)` | `renderer.RenderHtmlFileAsPdf(path)` |
| Existing PDF | _not native — PDF Duo only writes_ | `PdfDocument.FromFile(path)` |

### Page operations

| Operation | PDF Duo .NET | IronPDF |
|---|---|---|
| Paper size | _not in documented API_ | `ChromePdfRenderOptions.PaperSize` |
| Margins | _not in documented API_ | `ChromePdfRenderOptions.Margin*` |
| Orientation | _not in documented API_ | `ChromePdfRenderOptions.PaperOrientation` |
| Page count | _not exposed_ | `pdf.PageCount` |

### Merge/split operations

| Operation | PDF Duo .NET | IronPDF |
|---|---|---|
| Merge | _not native — typically iTextSharp 4.x_ | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | _not native_ | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

### 1. HTML to PDF

**Before (PDF Duo .NET):**
```csharp
using DuoDimension;
using System.IO;

class HtmlToPdfExample
{
    static void Main()
    {
        // PDF Duo's OpenHTML accepts a file path, URL, or stream — there is no
        // documented "convert HTML string to PDF" call, so write to a temp file first.
        // There is also no documented settings object for page size or margins.
        var tempHtml = Path.GetTempFileName() + ".html";
        File.WriteAllText(tempHtml, "<html><body><h1>Invoice #1234</h1></body></html>");

        var conv = new HtmlToPdf();
        conv.OpenHTML(tempHtml);
        conv.SavePDF("invoice.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize    = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop    = 10;
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.MarginLeft   = 15;
renderer.RenderingOptions.MarginRight  = 15;

var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>"
);
pdf.SaveAs("invoice.pdf");
// Full guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (PDF Duo .NET + iTextSharp 4.x — typical 2010-era pattern):**
```csharp
using DuoDimension;
// using iTextSharp.text.pdf;  // separate dependency — PDF Duo has no native merge

// 1) Render each HTML source to its own PDF with PDF Duo
var conv = new HtmlToPdf();
conv.OpenHTML("page1.html"); conv.SavePDF("document1.pdf");
conv = new HtmlToPdf();
conv.OpenHTML("page2.html"); conv.SavePDF("document2.pdf");

// 2) Merge with a *different* library — PDF Duo .NET does not expose a
//    merge call on HtmlToPdf. The 2010-era pattern was iTextSharp 4.x:
//      var reader1 = new PdfReader("document1.pdf");
//      var reader2 = new PdfReader("document2.pdf");
//      // PdfCopy.Append(...)
```

**After (IronPDF native):**
```csharp
using IronPdf;
using System.Linq;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var paths = new[] { "document1.pdf", "document2.pdf" };
var pdfs = paths.Select(PdfDocument.FromFile).ToList();
var merged = PdfDocument.Merge(pdfs);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (PDF Duo .NET + iTextSharp 4.x — PDF Duo has no native watermark):**
```csharp
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

class WatermarkExample
{
    static void Main()
    {
        // PDF Duo .NET does not document a watermark API. Teams that needed
        // watermarks combined PDF Duo's HTML-to-PDF output with iTextSharp 4.x:
        using var reader  = new PdfReader("input.pdf");
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
            cb.ShowTextAligned(Element.ALIGN_CENTER, "DRAFT", 297, 420, 45);
            cb.EndText();
            cb.RestoreState();
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("input.pdf");
pdf.ApplyWatermark(
    "<h1 style='color:gray; opacity:0.3; font-size:72px;'>DRAFT</h1>",
    45,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);
pdf.SaveAs("watermarked.pdf");
// Guide: https://ironpdf.com/how-to/custom-watermark/
```

---

### 4. Password protection

**Before (PDF Duo .NET — no native security API):**
```csharp
// PDF Duo .NET's documented surface does not include password protection,
// encryption, or permission flags. Teams that needed PDF security applied it
// in a second pass using iTextSharp 4.x or an equivalent library:
//
//   var reader = new PdfReader("input.pdf");
//   PdfEncryptor.Encrypt(reader, fs, PdfWriter.STRENGTH128BITS,
//       "userpass", "ownerpass", PdfWriter.AllowPrinting);
//
// IronPDF folds this into the same document model.
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("input.pdf");
pdf.SecuritySettings.UserPassword  = "userpass";
pdf.SecuritySettings.OwnerPassword = "ownerpass";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserAnnotations = false;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Page indexing

IronPDF uses 0-based page indexing throughout. PDF Duo .NET does not expose page-level operations on `HtmlToPdf`, so there is rarely a 1-based versus 0-based off-by-one to translate — but be aware of the convention for anything you build on top of IronPDF:

```csharp
// IronPDF: 0-based
var firstPage = pdf.Pages[0];
var lastPage  = pdf.Pages[pdf.PageCount - 1];

// Page operations on a range:
var firstThreePages = pdf.CopyPages(0, 2); // pages 0, 1, 2
```

### Error handling model

PDF Duo .NET's `OpenHTML` / `SavePDF` do not have a well-documented exception surface. Replace any best-effort error handling with try/catch around the IronPDF calls:

```csharp
// IronPDF throws on failure — not null return
try
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
catch (IronPdf.Exceptions.IronPdfException ex)
{
    _logger.LogError(ex, "PDF render failed: {message}", ex.Message);
    throw; // or handle gracefully
}
```

### Removing the vendored DLL

Because PDF Duo .NET is not on NuGet, there is no `dotnet remove package` step. Clean it up by hand once IronPDF is in place:

```bash
# Find PDFDuo references in csproj files
rg "PDFDuo\.dll|PDFDuoNET\.dll" -n

# Find any vendored binary in /lib or /libs folders
rg --files | rg -i "pdfduo.*\.dll"

# Then in Visual Studio: References -> remove PDFDuo.dll. Delete the
# binary from your /lib folder. Commit the project file changes.
```

---

## Performance considerations

### Renderer reuse for batch work

```csharp
// One renderer, many renders — efficient for batch processing
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

foreach (var (id, html) in documentQueue)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{id}.pdf");
}
```

### Async for web applications

```csharp
[HttpPost("generate")]
public async Task<IActionResult> GeneratePdf([FromBody] string html)
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return File(pdf.Stream, "application/pdf", "document.pdf");
    // Async guide: https://ironpdf.com/how-to/async/
}
```

### Consolidating secondary libraries

If you paired PDF Duo .NET with iTextSharp 4.x (the typical combination for merge/watermark/security), profile whether removing the second library affects build size and cold-start time:

```csharp
// Before: 2 libraries (PDF Duo .NET DLL + iTextSharp 4.x)
// After:  1 library (IronPDF)
// Expected: fewer package loads, simplified dependency graph, no vendored DLL
// Profile: NuGet restore time, Docker image size, startup time
```

### Edge cases

- **Temp-file detour:** Old PDF Duo code that writes HTML to a temp file before `OpenHTML` can drop the temp file entirely once it moves to `RenderHtmlAsPdf(string)`. Audit for orphaned `Path.GetTempFileName()` calls during cleanup.
- **Font subsetting:** IronPDF's Chromium renderer handles font subsetting differently than 2010-era HTML-to-PDF engines. Verify embedded font sizes in output PDFs if file size is a concern.
- **Custom paper sizes:** If your templates use non-standard paper sizes, configure the `ChromePdfRenderOptions` API for custom dimensions — PDF Duo had no equivalent.

---

## Migration checklist

### Pre-migration

- [ ] Find every `DuoDimension` import and every `HtmlToPdf` / `OpenHTML` / `SavePDF` call
- [ ] Build a usage inventory table (operations, files, secondary libraries)
- [ ] Document all current workarounds and what they address
- [ ] Pull all HTML templates for render comparison testing
- [ ] Identify any iTextSharp 4.x or other secondary library used alongside PDF Duo
- [ ] Verify IronPDF .NET target framework compatibility for your project
- [ ] Confirm commercial license requirements
- [ ] Set up IronPDF trial license in dev environment

### Code migration

- [ ] Remove the PDFDuo.dll reference from your project (Visual Studio: References → Remove)
- [ ] Remove iTextSharp 4.x or other secondary libraries if they were only filling PDF Duo's gaps
- [ ] Add the `IronPdf` NuGet package
- [ ] Replace `using DuoDimension;` with `using IronPdf;`
- [ ] Add `IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";` at startup
- [ ] Replace `new HtmlToPdf()` with `new ChromePdfRenderer()`
- [ ] Replace `OpenHTML(path) + SavePDF(out)` with `RenderHtml*AsPdf(...).SaveAs(out)`
- [ ] Drop temp-file detours for HTML-string conversion — use `RenderHtmlAsPdf(string)`
- [ ] Replace any iTextSharp merge code with `PdfDocument.Merge()`
- [ ] Replace any iTextSharp watermark code with `pdf.ApplyWatermark()`
- [ ] Replace any iTextSharp security code with `pdf.SecuritySettings`
- [ ] Update output handling — PDF Duo wrote to disk; IronPDF gives you `PdfDocument` (use `SaveAs`, `BinaryData`, or `Stream`)

### Testing

- [ ] Render each HTML template and visually compare against PDF Duo output
- [ ] Focus on CSS features that PDF Duo's engine may not have supported (flexbox, grid, variables)
- [ ] Verify workarounds that were in the old code are no longer needed
- [ ] Test merge with representative document sets
- [ ] Test watermark on multi-page documents
- [ ] Test password protection (correct and incorrect credentials)
- [ ] Load test concurrent rendering at expected peak

### Post-migration

- [ ] Delete the vendored PDFDuo.dll from `/lib` and remove it from source control
- [ ] Remove iTextSharp 4.x from project if it was only there to fill PDF Duo's gaps
- [ ] Update deployment documentation — no more "drop the DLL in /lib" step
- [ ] Monitor the first production week for any regression

---

## Done migrating? Here's what's next

The renewal-trigger migration often reveals how many secondary workarounds have accumulated around a library over time. With PDF Duo .NET specifically, the audit usually surfaces an iTextSharp 4.x pairing that has been quietly handling everything PDF Duo could not — and IronPDF can usually absorb both. The migration audit, finding the workarounds and verifying whether they follow you to the new library, is frequently the most valuable part of the process independent of which library you end up with.

What edge cases did you hit that this walkthrough did not cover? Particularly interested in teams who had encoding quirks or document-specific CSS patches that predated the migration — whether those issues resolved or followed them to IronPDF.
