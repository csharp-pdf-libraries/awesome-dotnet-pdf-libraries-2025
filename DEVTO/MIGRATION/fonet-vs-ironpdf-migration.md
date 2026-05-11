---
title: "Migrating from FO.NET to IronPDF: drop-in replacement guide"
published: false
tags: dotnet, csharp, pdf, migration
---

The FO.NET migration question usually starts with a specific failure: an XSL-FO stylesheet that worked for a decade stops rendering correctly on a new server, or the team discovers FO.NET has had no meaningful updates in over a decade, or a new requirement comes in that FO.NET simply cannot fulfill. XSL-FO is a powerful layout specification, but FO.NET is one of the more obscure implementation choices for .NET projects, and its maintenance trajectory is what it is — the legacy `Fonet` package on nuget.org was last published in April 2011 targeting .NET Framework 2.0, and the community `Fonet.Standard` fork hasn't shipped since May 2020.

This article is structured around the troubleshooting path: identifying what's broken, deciding what to migrate, and making the switch to IronPDF systematically. The conceptual gap between XSL-FO and HTML-to-PDF is real and worth understanding before writing any code.

---

## Why Migrate (Without Drama)

1. **Abandonment** — FO.NET has no active maintenance. The legacy `Fonet` NuGet was last published in April 2011 (v1.0.0, .NET Framework 2.0). The `Fonet.Standard` fork was last published May 2020. .NET runtime changes (especially .NET Core+) may break it silently or loudly.
2. **.NET Core / .NET 5+ compatibility** — The original `Fonet` build targets .NET Framework 2.0. The `Fonet.Standard` fork targets .NET Standard 2.0 and is also no longer actively developed.
3. **XSL-FO expertise** — The team that understood XSL-FO may have left. HTML + CSS is far more accessible to modern .NET developers, and the W3C closed the XSL-FO Working Group in 2013.
4. **Runtime exceptions on upgrade** — Upgrading the .NET runtime or OS frequently reveals FO.NET incompatibilities.
5. **Docker/Linux** — The legacy build depends on `System.Drawing` GDI+ APIs that effectively limit it to Windows. Linux containers will fail.
6. **Feature gap** — Modern requirements (CSS3 flexbox/grid, JS-rendered charts, modern web font loading) are incompatible with XSL-FO processing.
7. **Tooling** — XSL-FO editors and validators are rare. HTML templates can be built and debugged in any browser.
8. **Pipeline complexity** — XSLT to XSL-FO to FO.NET to PDF is a multi-step transformation chain. HTML to IronPDF to PDF is one step.
9. **XSL-FO spec maintenance** — XSL-FO itself is a stable but aging W3C spec with no further development since 2013.
10. **Error diagnosis** — FO.NET errors are often cryptic XML processing exceptions with no actionable message.

### Side-by-Side Comparison

| Aspect | FO.NET | IronPDF |
|---|---|---|
| Focus | XSL-FO processor to PDF | HTML/URL to PDF via Chromium |
| Maintenance | Abandoned (last legacy release 2011; fork last 2020) | Actively maintained |
| Pricing | Free (Apache 2.0) | Commercial — see ironpdf.com for current pricing |
| API Style | XML/stream processing | Simple render methods |
| Learning Curve | High — XSL-FO is complex | Low for HTML-first workflows |
| HTML Rendering | Not applicable — XSL-FO input only | Chromium, full CSS3/JS |
| Page Indexing | N/A | 0-based |
| Thread Safety | Single-threaded by design | `ChromePdfRenderer` reusable |
| Namespace | `Fonet`, `Fonet.Render.Pdf` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Complexity | Notes |
|---|---|---|
| XML data to PDF | Medium-High | Pipeline changes; XSL-FO to HTML template conversion |
| XSL-FO layout to HTML/CSS | High | Significant design work; not a code swap |
| Tables with complex spans | High | XSL-FO table model maps to HTML table: similar concept, different syntax |
| Running headers/footers | Medium | XSL-FO `<fo:static-content>` to IronPDF HTML headers |
| Multi-column layout | High | XSL-FO has explicit multi-column; CSS Multi-column spec differs |
| Page numbering | Medium | XSL-FO `<fo:page-number>` to IronPDF `{page}` placeholder |
| Merge output | Low | IronPDF adds first-class merge; FO.NET had no native merge |
| Password/security | Low | IronPDF adds AES-256, granular permissions; FO.NET only had legacy RC4 |
| Watermark | Medium | IronPDF stamp API; FO.NET required overlay tricks in XSL-FO |
| Conditional page masters | High | XSL-FO page master sequences to HTML CSS @page |

### Decision Matrix

| Scenario | Recommendation |
|---|---|
| FO.NET no longer runs on your .NET version | Must migrate |
| XSL-FO templates are working; team understands them | Lower urgency; validate long-term maintenance plan |
| New requirements can't be met by XSL-FO | Migrate those requirements to IronPDF |
| Need Linux container support | Legacy FO.NET will not work; migrate |

---

## Troubleshooting Common FO.NET Problems (and Migration Path)

### Problem 1: FO.NET Won't Load on .NET 6+

**Symptom:** `System.BadImageFormatException` or `System.IO.FileNotFoundException` when loading the FO.NET DLL.

**Root cause:** The legacy `Fonet` build targets .NET Framework 2.0. On .NET Core / .NET 6+, it may not load via `Assembly.LoadFrom()` or package references. The `Fonet.Standard` fork targets .NET Standard 2.0 but is itself unmaintained.

**Diagnosis:**

```bash
# Check FO.NET DLL target framework
# On Windows (PowerShell):
[System.Reflection.Assembly]::ReflectionOnlyLoadFrom("Fonet.dll").ImageRuntimeVersion

# Or use ildasm to check:
# ildasm Fonet.dll /text | grep ".ver"

# Check your project target framework:
grep -r "TargetFramework" *.csproj
```

**Migration path:** If the DLL won't load, you can't keep FO.NET. The migration is forced.

---

### Problem 2: XSL-FO Rendering Produces Malformed PDF

**Symptom:** PDF opens but has missing sections, garbled text, or overlapping regions. No exception thrown.

**FO.NET debugging approach:**

```csharp
using Fonet;
using Fonet.Render.Pdf;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

// Typical FO.NET pipeline

// Step 1: Load XML data
var xmlDoc = new XmlDocument();
xmlDoc.Load("data.xml");

// Step 2: Apply XSLT to get XSL-FO
var transform = new XslCompiledTransform();
transform.Load("stylesheet.xsl");
var foString = new StringBuilder();
transform.Transform(xmlDoc, null, new StringWriter(foString));

// Step 3: Pass XSL-FO to FonetDriver
FonetDriver driver = FonetDriver.Make();
using (var outputStream = new FileStream("output.pdf", FileMode.Create))
{
    driver.Render(new StringReader(foString.ToString()), outputStream);
}
```

**Debugging the XSL-FO output directly:**

```bash
# Validate XSL-FO before FO.NET processes it
# Save the xsl-fo string to file and inspect:
# - Check for unclosed elements
# - Validate against XSL-FO schema if available
# - Look for missing namespace declarations (fo: prefix)

# A common issue: incorrect namespace declaration
# Should be: xmlns:fo="http://www.w3.org/1999/XSL/Format"
grep "xmlns:fo" your_xslfo_output.xml
```

**Migration path:** If the XSL-FO renders correctly but FO.NET produces bad PDF, the data and transformation logic is salvageable. The pipeline becomes:

```
[OLD]: XML -> XSLT -> XSL-FO -> FO.NET -> PDF
[NEW]: XML -> XSLT -> HTML -> IronPDF -> PDF
```

The key insight: the XML data and the transformation are often good. The output target changes from XSL-FO to HTML.

---

### Problem 3: Missing Fonts in PDF Output

**Symptom:** FO.NET outputs PDF with fonts substituted or missing.

**FO.NET root cause:** Font configuration in FO.NET is non-trivial and may involve configuration files pointing to system font directories.

**IronPDF approach:** Chromium handles font rendering. For custom fonts:

```csharp
using IronPdf;

// Embed custom fonts via HTML @font-face
string htmlWithFont = @"
<html>
<head>
<style>
@font-face {
    font-family: 'CustomFont';
    src: url('https://your-domain.com/fonts/custom.woff2') format('woff2');
}
body { font-family: 'CustomFont', sans-serif; }
</style>
</head>
<body><p>Custom font text</p></body>
</html>";

var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf(htmlWithFont);
pdf.SaveAs("output_with_fonts.pdf");

// For local font files, use BaseUrl to serve from local file system:
// renderer.RenderingOptions.BaseUrl = new Uri("file:///path/to/fonts/dir/");
```

---

### Problem 4: Multi-Page Documents with Page Numbering

**Symptom:** XSL-FO's `<fo:page-number>` worked; now need equivalent in IronPDF.

**XSL-FO pattern (for context):**

```xml
<!-- XSL-FO page numbering — becomes HTML headers/footers in IronPDF -->
<fo:static-content flow-name="xsl-region-after">
  <fo:block text-align="center">
    Page <fo:page-number/> of <fo:page-number-citation ref-id="last-page"/>
  </fo:block>
</fo:static-content>
```

**IronPDF HTML header/footer approach:**

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// HTML footer with page number placeholder
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center; font-size:10px;'>Page {page} of {total-pages}</div>",
    DrawDividerLine = false
};

string html = "<html><body><h1>Multi-page Document</h1></body></html>";
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("numbered.pdf");
// Docs: https://ironpdf.com/how-to/rendering-options/
```

---

### Problem 5: Complex Table Layout Differences

**Symptom:** XSL-FO tables with `table-header`, `table-footer`, repeated across pages work in FO.NET but the HTML equivalent doesn't repeat table headers.

**Solution in HTML/CSS:**

```html
<!-- HTML table headers repeat on page breaks via print CSS -->
<html>
<head>
<style>
  thead { display: table-header-group; }  /* repeat on each page */
  tfoot { display: table-footer-group; }  /* repeat at bottom */
  @media print {
    table { page-break-inside: auto; }
    tr    { page-break-inside: avoid; page-break-after: auto; }
  }
</style>
</head>
<body>
  <table>
    <thead>
      <tr><th>Column A</th><th>Column B</th></tr>
    </thead>
    <tbody>
      <!-- data rows -->
    </tbody>
  </table>
</body>
</html>
```

IronPDF uses Chromium's print rendering — `display: table-header-group` is respected.

---

## Before You Start

### Find FO.NET References

```bash
# Find Fonet / FO.NET references
rg "FonetDriver|Fonet|using Fonet" --type cs -l

# Find XSL-FO pipeline stages
rg "XslCompiledTransform" --type cs                       # XSLT stage
rg "xmlns:fo|fo:root" -g "*.xsl" -g "*.xslt" -g "*.fo"   # XSL-FO templates
rg "xslfo|foDocument|foStream" --type cs                  # FO processing

# Find FO.NET package references in project files
rg "Fonet" -g "*.csproj" -g "*.fsproj" -l
```

### Inventory XSL-FO Stylesheets

```bash
# List all XSLT stylesheets that produce XSL-FO output
# (look for xmlns:fo in XSLT outputs)
grep -rl "xmlns:fo=\"http://www.w3.org/1999/XSL/Format\"" . --include="*.xsl"
grep -rl "xmlns:fo=" . --include="*.xslt"
```

> **Important:** These XSL-FO stylesheets represent significant layout work. If you're migrating to HTML output, you'll be writing equivalent HTML/CSS templates. Factor this effort into your migration plan — it's not a code swap, it's a template rewrite.

### Uninstall / Install

```bash
# Remove FO.NET reference (either the legacy package or the Standard fork)
dotnet remove package Fonet
dotnet remove package Fonet.Standard

# Install IronPDF
dotnet add package IronPdf

dotnet list package
```

---

## Quick Start Migration (3 Steps)

### Step 1: License

```csharp
// FO.NET: no license setup (Apache 2.0)
// IronPDF: license before first render
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Docs: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace Imports

```csharp
// Before (FO.NET)
// using Fonet;
// using Fonet.Render.Pdf;

// After
using IronPdf;
// Keep: using System.Xml; using System.Xml.Xsl; — for your data transformation
```

### Step 3: Pipeline Restructure

The core change is the XSLT output target: instead of XSL-FO, your XSLT now produces HTML.

```csharp
// Before: XSLT -> XSL-FO -> FO.NET -> PDF
// var xslfo = ApplyXslt(xmlData, "to_fo.xsl");   // XSLT to XSL-FO
// FonetDriver.Make().Render(new StringReader(xslfo), outputStream);  // FO.NET renders

// After: XSLT -> HTML -> IronPDF -> PDF
using System.Xml;
using System.Xml.Xsl;
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Step 1: Transform XML data to HTML (change your XSLT output to HTML)
var transform = new XslCompiledTransform();
transform.Load("to_html.xsl"); // new XSLT targeting HTML output

var xmlDoc = new XmlDocument();
xmlDoc.Load("data.xml");

var htmlWriter = new System.Text.StringBuilder();
transform.Transform(xmlDoc, null, new System.IO.StringWriter(htmlWriter));
string html = htmlWriter.ToString();

// Step 2: IronPDF renders HTML
var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// Docs: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| FO.NET | IronPDF | Notes |
|---|---|---|
| `Fonet` | `IronPdf` | Core namespace |
| `Fonet.Render.Pdf` | `IronPdf` | PDF rendering options |
| XSL-FO markup | HTML + CSS | Input format changes |
| XSLT output method="xml" (FO) | XSLT output method="html" | Stylesheet retarget |

### Core Class Mapping

| FO.NET Class | IronPDF Class | Description |
|---|---|---|
| `FonetDriver` | `ChromePdfRenderer` | Main rendering class |
| `FonetDriver.Make()` | `new ChromePdfRenderer()` | Factory / constructor |
| `driver.Render(reader, stream)` | `renderer.RenderHtmlAsPdf(html)` | Render trigger |
| `PdfRendererOptions` | `RenderingOptions` and `SecuritySettings` | Configuration |
| XSL-FO document (stream) | HTML string | Input type changes |
| Output stream | `PdfDocument.SaveAs()` / `BinaryData` | Output |

### Document Loading

| Operation | FO.NET | IronPDF |
|---|---|---|
| Render XSL-FO stream | `driver.Render(StringReader, FileStream)` | N/A — input changes to HTML |
| Render HTML string | N/A | `renderer.RenderHtmlAsPdf(html)` |
| Render URL | Not supported | `renderer.RenderUrlAsPdf(url)` |
| Load existing PDF | Not supported | `PdfDocument.FromFile(path)` |
| Load from bytes | Not supported | `PdfDocument.FromBinaryData(bytes)` |

### Page Operations

| Operation | FO.NET | IronPDF |
|---|---|---|
| Running headers | `fo:static-content flow-name="xsl-region-before"` | `RenderingOptions.HtmlHeader` |
| Running footers | `fo:static-content flow-name="xsl-region-after"` | `RenderingOptions.HtmlFooter` |
| Page size | `fo:simple-page-master` attributes | `RenderingOptions.PaperSize` |
| Margins | `fo:simple-page-master` attributes | `RenderingOptions.Margin*` |

### Merge / Split

| Operation | FO.NET | IronPDF |
|---|---|---|
| Merge multiple docs | Not supported natively | `PdfDocument.Merge(a, b)` |
| Split | Not supported natively | `pdf.CopyPages(start, end)` |

---

## Four Complete Before/After Migrations

### 1. XML Data to PDF (Core Pipeline)

**Before (XSL-FO pipeline with FonetDriver):**

```csharp
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using Fonet;
using Fonet.Render.Pdf;

class Program
{
    static void Main()
    {
        // Step 1: Load XML data
        var xmlDoc = new XmlDocument();
        xmlDoc.Load("invoice_data.xml");

        // Step 2: XSLT transforms XML -> XSL-FO
        var transform = new XslCompiledTransform();
        transform.Load("invoice_to_fo.xsl"); // XSL-FO stylesheet

        var foOutput = new StringBuilder();
        transform.Transform(xmlDoc, null, new StringWriter(foOutput));
        string xslfo = foOutput.ToString();

        // Step 3: FonetDriver renders XSL-FO to PDF
        FonetDriver driver = FonetDriver.Make();
        driver.Options = new PdfRendererOptions
        {
            Title  = "Invoice",
            Author = "Company",
        };
        using (var outputStream = new FileStream("invoice.pdf", FileMode.Create))
        {
            driver.Render(new StringReader(xslfo), outputStream);
        }
    }
}
```

**After (HTML pipeline with IronPDF):**

```csharp
using System.Xml;
using System.Xml.Xsl;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        // Docs: https://ironpdf.com/how-to/license-keys/

        // Step 1: Load XML data (unchanged)
        var xmlDoc = new XmlDocument();
        xmlDoc.Load("invoice_data.xml");

        // Step 2: XSLT now produces HTML instead of XSL-FO
        // (requires rewriting invoice_to_fo.xsl -> invoice_to_html.xsl)
        var transform = new XslCompiledTransform();
        transform.Load("invoice_to_html.xsl");

        var htmlOutput = new System.Text.StringBuilder();
        transform.Transform(xmlDoc, null, new System.IO.StringWriter(htmlOutput));
        string html = htmlOutput.ToString();

        // Step 3: IronPDF renders HTML to PDF
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
        // Docs: https://ironpdf.com/how-to/rendering-options/

        using var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.MetaData.Title = "Invoice";
        pdf.MetaData.Author = "Company";
        pdf.SaveAs("invoice.pdf");
    }
}
```

---

### 2. Merge PDFs

**Before (FO.NET — no native merge):**

```csharp
// FonetDriver has no built-in merge. The typical workaround is to
// render each XSL-FO source to its own PDF, then merge with a separate
// PDF library. There is no Fonet.Merge or equivalent API in either the
// legacy Fonet package or the Fonet.Standard fork.
//
// var driver = FonetDriver.Make();
// using (var s1 = new FileStream("a.pdf", FileMode.Create))
//     driver.Render(new StringReader(xslfoA), s1);
// using (var s2 = new FileStream("b.pdf", FileMode.Create))
//     driver.Render(new StringReader(xslfoB), s2);
// // Then merge a.pdf + b.pdf with a different library
```

**After (IronPDF):**

```csharp
using IronPdf;

class MergeSample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var docA = PdfDocument.FromFile("doc_a.pdf");
        using var docB = PdfDocument.FromFile("doc_b.pdf");

        using var merged = PdfDocument.Merge(docA, docB);
        merged.SaveAs("merged.pdf");
        // Docs: https://ironpdf.com/how-to/merge-or-split-pdfs/
    }
}
```

---

### 3. Watermark

**Before (FO.NET — watermark via XSL-FO overlay markup):**

```xsl
<!-- XSL-FO watermark via fo:block-container with absolute positioning -->
<!-- FO.NET implements a subset of XSL-FO inherited from a pre-1.0 Apache
     FOP build. Z-index, opacity, and absolute positioning may behave
     inconsistently or be ignored depending on the construct. -->
<fo:block-container position="fixed" top="0" left="0"
    width="21cm" height="29.7cm" z-index="-1">
  <fo:block text-align="center" padding-top="10cm" opacity="0.3"
      font-size="60pt" color="gray">DRAFT</fo:block>
</fo:block-container>
```

**After (IronPDF):**

```csharp
using IronPdf;
using IronPdf.Editing;

class WatermarkSample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        var stamper = new TextStamper
        {
            Text = "DRAFT",
            FontSize = 50,
            Opacity = 30,
            Rotation = 45,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        pdf.ApplyStamp(stamper);
        pdf.SaveAs("watermarked.pdf");
        // Docs: https://ironpdf.com/how-to/stamp-text-image/
    }
}
```

---

### 4. Password Protection

**Before (FO.NET — basic password encryption via PdfRendererOptions):**

```csharp
// FonetDriver supports PDF encryption through PdfRendererOptions:
// setting UserPassword or OwnerPassword triggers encryption. The
// encryption scheme is the legacy 40/128-bit RC4 inherited from
// early Apache FOP — no AES-256, no digital signatures, no granular
// permission flags (print / copy / annotate).
using Fonet;
using Fonet.Render.Pdf;
using System.IO;

FonetDriver driver = FonetDriver.Make();
driver.Options = new PdfRendererOptions
{
    Title         = "Confidential Report",
    Author        = "Company Name",
    UserPassword  = "user123",
    OwnerPassword = "owner456",
};
driver.Render(new StringReader(xslFo),
    new FileStream("secured.pdf", FileMode.Create));
```

**After (IronPDF):**

```csharp
using IronPdf;
using IronPdf.Security;

class SecuritySample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        // Docs: https://ironpdf.com/how-to/pdf-permissions-passwords/
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;

        pdf.SaveAs("secured.pdf");
    }
}
```

---

## Critical Migration Notes

### XSL-FO Is a Different Input Model

This is not a drop-in library swap. FO.NET takes XSL Formatting Objects as input. IronPDF takes HTML. If your pipeline generates XSL-FO via XSLT, you have two options:

**Option A: Retarget XSLT to HTML output**
- Change your XSLT `xsl:output method="xml"` to `method="html"`
- Rewrite layout rules from XSL-FO vocabulary (`fo:block`, `fo:table`, `fo:simple-page-master`) to HTML + CSS equivalents
- Higher up-front effort but results in maintainable HTML templates

**Option B: Add an XSL-FO to HTML transformation step**
- Use a separate XSLT stylesheet that translates XSL-FO into HTML
- Less upfront work but adds complexity and a second layer to debug

Neither option is a configuration change. Budget engineering time for template work.

### FO.NET Feature Parity

FO.NET implements a subset of the XSL-FO specification (it forks an early pre-1.0 Apache FOP). Some XSL-FO features may be silently unsupported and you may not know it (the rendering just drops those features). Compare your migrated HTML output against the original PDF output visually — don't assume 1:1 fidelity in either direction.

### Running Headers and Footers

XSL-FO's `fo:static-content` with `flow-name="xsl-region-before"` / `"xsl-region-after"` is a first-class feature. IronPDF uses `RenderingOptions.HtmlHeader` and `HtmlFooter` with `{page}` and `{total-pages}` placeholders. For complex headers (images, company logos, variable data), embed them in the HTML header fragment.

### Page Master Sequences

XSL-FO's `fo:page-sequence-master` with different masters for first page, odd/even pages has no direct equivalent in IronPDF's HTML model. Use CSS `@page :first`, `@page :left`, `@page :right` selectors — Chromium supports the standard CSS Paged Media selectors that map to these patterns.

---

## Performance Considerations

FO.NET is a synchronous, single-threaded processor. IronPDF is Chromium-based with async support. For high-volume document generation:

```csharp
// Async render — preferred for server contexts
public async Task<byte[]> GenerateAsync(string html)
{
    using var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
    // Docs: https://ironpdf.com/how-to/async/
}
```

For batch document generation (parallel):

```csharp
using IronPdf;

// Singleton renderer — initialize once
private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

// Parallel render
// Docs: https://ironpdf.com/examples/parallel/
var tasks = documents.Select(async doc =>
{
    string html = TransformToHtml(doc.XmlData, doc.XsltPath);
    using var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
    return (doc.Id, pdf.BinaryData);
});

var results = await Task.WhenAll(tasks);
```

Cold start note: IronPDF initializes Chromium on first render. FO.NET's cold start was lighter. Plan a warm-up render in your application startup.

---

## Migration Checklist

### Pre-Migration
- [ ] Confirm FO.NET is truly broken / unsupported on your target runtime
- [ ] Inventory all XSL-FO stylesheets (`grep -rl "xmlns:fo" . --include="*.xsl"`)
- [ ] Inventory all XSLT transformations that feed FO.NET
- [ ] Evaluate Option A (retarget XSLT to HTML) vs. Option B (XSL-FO to HTML XSLT)
- [ ] Estimate template rewrite effort — this is likely the bulk of the migration
- [ ] Confirm IronPDF .NET target compatibility
- [ ] Set up IronPDF license in all environments
- [ ] Test IronPDF on your Linux container base image
- [ ] Review [rendering options](https://ironpdf.com/how-to/rendering-options/) for paper size, margin, header/footer

### Code Migration
- [ ] Rewrite XSL-FO stylesheet(s) as HTML/CSS (or retarget XSLT)
- [ ] Replace `Fonet` / `Fonet.Standard` package reference with `IronPdf`
- [ ] Replace `FonetDriver.Render()` with `renderer.RenderHtmlAsPdf()`
- [ ] Add `IronPdf.License.LicenseKey` configuration
- [ ] Register `ChromePdfRenderer` as singleton in DI
- [ ] Port `fo:static-content` headers/footers to `RenderingOptions.HtmlHeader/Footer`
- [ ] Port page size / margin configuration to `RenderingOptions`
- [ ] Add `using` disposal to all `PdfDocument` instances
- [ ] Replace merge step (if any) with `PdfDocument.Merge()`
- [ ] Add watermark/security where needed

### Testing
- [ ] Visual comparison: FO.NET output vs. IronPDF output for each document type
- [ ] Test repeated table headers on multi-page tables
- [ ] Test running headers and footers with page numbers
- [ ] Test complex tables with merged cells
- [ ] Test documents with images (embedded + URL-referenced)
- [ ] Test custom fonts
- [ ] Test under concurrent load
- [ ] Memory stability test — 30 min sustained render

### Post-Migration
- [ ] Remove `Fonet` / `Fonet.Standard` package references
- [ ] Archive XSL-FO stylesheets (don't delete — they document layout intent)
- [ ] Update Dockerfile/deployment for IronPDF Linux deps
- [ ] Warm up renderer at application startup
- [ ] Document the pipeline change for future maintainers

---

## That's the Migration

FO.NET migrations are the most conceptually complex in this series because the input model changes — not just the library. The combination of an abandoned tool, .NET Core incompatibility, and an input format that most modern .NET developers don't know makes this feel more daunting than it is. The XSL-FO to HTML/CSS translation is well-understood territory; most layout concepts map directly, and the CSS Print specification covers the print-specific features you need.

The best thing to do with your old XSL-FO stylesheets: keep them as reference documentation. They describe the layout intent precisely. Use them to guide your HTML/CSS equivalents, page by page.

**Technical question for comments:** Has anyone automated XSL-FO to HTML/CSS conversion, even partially? An XSLT that translates common XSL-FO constructs to HTML equivalents would save migration time for large stylesheets. Has anyone built or found something like this?
