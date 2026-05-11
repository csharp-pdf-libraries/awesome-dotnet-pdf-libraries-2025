---
title: "Dropping ZetPDF for IronPDF: a .NET migration that fits in an afternoon"
published: false
tags: dotnet, csharp, pdf, migration
---

The first ZetPDF symptom that usually prompts a migration is not a runtime failure — it is the realisation that there is no `RenderHtmlAsPdf` method to look for. ZetPDF is a PDFsharp-shaped, coordinate-drawing SDK distributed as a ZIP from [zetpdf.com/download](https://zetpdf.com/download/), with no NuGet listing and no documented HTML-to-PDF or URL-to-PDF support in its [feature list](https://zetpdf.com/net-pdf-sdk/). Every text block, table cell, and image gets placed manually with `XGraphics.DrawString` at exact coordinates. The moment a designer changes a layout, the C# changes too.

This article covers migrating from ZetPDF to IronPDF — moving from coordinate-based drawing to an HTML/CSS-driven Chromium renderer. You will get working before/after code for the patterns that recur in coordinate-based codebases, a troubleshooting section for the issues that surface during the switch, and a complete checklist.

---

## Why Migrate (Without Drama)

Teams evaluating ZetPDF alternatives commonly encounter:

1. **No HTML or URL rendering** — ZetPDF's documented surface covers PDF view/print, annotations, AES256 encryption, form fields, and text extraction. There is no native HTML-to-PDF or URL-to-PDF converter.
2. **Coordinate-based API** — every element is positioned manually via `XGraphics.DrawString(text, font, brush, new XPoint(x, y))`. A simple form takes hundreds of lines.
3. **No CSS, no JavaScript** — styling is per-element via `XFont`/`XBrushes`. Web fonts, gradients, flexbox, and any dynamic content are unavailable.
4. **Manual page breaks** — pagination is hand-rolled: track `y`, compare to page height, call `document.AddPage()`, reset, repeat.
5. **Manual text measurement** — wrapping long strings requires measuring each glyph run yourself.
6. **No NuGet distribution** — ZetPDF is shipped as a ZIP. You add the DLL to your project manually, which breaks `dotnet restore`-driven CI and reproducible builds.
7. **Limited public release activity** — the PDFsharp community has [openly questioned whether ZetPDF is a closed fork](https://forum.pdfsharp.net/viewtopic.php?f=2&t=3841), and AlternativeTo records no public release activity since 2021.
8. **No first-class helpers** — there is no one-line `Merge`, no `TextStamper`-style watermark API, no header/footer helper in the documented surface.
9. **Secondary library accumulation** — features ZetPDF doesn't cover (HTML rendering, watermark, merge) require adding a second PDF library, and now there are two SDKs to license, version, and patch.
10. **Debugging friction** — when the produced PDF is wrong, the bug is usually in coordinate math you wrote, not in the library, which makes diffing against a designer's mockup tedious.

### Comparison Table

| Aspect | ZetPDF | IronPDF |
|---|---|---|
| Focus | Coordinate-based PDF drawing (PDFsharp-shaped) | HTML-to-PDF + PDF manipulation |
| Distribution | ZIP from zetpdf.com (no NuGet) | NuGet (`IronPdf`) |
| Pricing | Commercial — see [zetpdf.com](https://zetpdf.com/) | Commercial — see [ironsoftware.com](https://ironpdf.com/) |
| API Style | `XGraphics.DrawString` at coordinates | `ChromePdfRenderer` + HTML input |
| Learning Curve | Familiar to PDFsharp users; tedious otherwise | Low for .NET devs; HTML/CSS is the input |
| HTML Rendering | Not in documented feature list | Embedded Chromium |
| Page Indexing | Pages indexed via `document.Pages[i]` (0-based) | 0-based |
| Namespace | `ZetPDF`, `ZetPdf.Drawing` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | ZetPDF | IronPDF Equivalent | Complexity |
|---|---|---|---|
| Text on a page | `graphics.DrawString(text, font, brush, point)` | HTML `<p>`/`<h1>` + `RenderHtmlAsPdfAsync()` | Low |
| Tables | Manual rectangles + per-cell `DrawString` | HTML `<table>` + CSS | Low |
| Multi-page document | Manual `AddPage()` + Y-position tracking | Automatic page breaks | Low |
| URL to PDF | Not in documented surface | `renderer.RenderUrlAsPdfAsync()` | Low |
| Save to file | `document.Save(path)` | `pdf.SaveAs(path)` | Low |
| Load existing PDF | `PdfReader.Open(path, ...)` | `PdfDocument.FromFile(path)` | Low |
| Merge PDFs | Manual page loop into output document | `PdfDocument.Merge(...)` | Medium |
| Watermark | Manual rotated `DrawString` | `pdf.ApplyWatermark(html)` | Medium |
| Password protection | AES256 settings on document | `pdf.SecuritySettings` | Low |
| Concurrent rendering | Not addressed in documented surface | `Task.WhenAll` pattern | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Output is HTML-shaped (invoices, reports, dashboards) | Migrate — eliminates hundreds of lines of coordinate math |
| ZetPDF added to get PDFsharp-compatible drawing on a paid license | Evaluate if the proprietary fork is buying anything you cannot get from PDFsharp itself or IronPDF |
| Reproducible CI builds matter | Migrate — IronPDF restores from NuGet; ZetPDF requires a manual DLL drop |
| .NET upgrade blocked or release cadence stalled | Migrate — public release activity has been quiet since 2021 |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All ZetPDF References

```bash
# Find ZetPDF usage
rg -l "ZetPDF|ZetPdf\b" --type cs
rg "ZetPDF|ZetPdf\b" --type cs -n

# Find coordinate-drawing patterns to be rewritten as HTML
rg "XGraphics|DrawString|DrawRectangle|DrawImage|XFont|XBrushes" --type cs -n

# Find the manual DLL reference in csproj (ZetPDF is not on NuGet)
rg -l "ZetPDF\.dll|HintPath.*ZetPDF" *.csproj **/*.csproj

# Find in Dockerfiles and CI pipelines
grep -r "ZetPDF\|ZetPdf" Dockerfile* .github/**/*.yml .gitlab-ci.yml 2>/dev/null

# Count usage
rg "ZetPdf\b|ZetPDF\b" --type cs | wc -l
```

### Uninstall / Install

```bash
# ZetPDF is not on NuGet — remove the manual <Reference Include="ZetPDF" />
# block (with its <HintPath>) from your .csproj by hand.

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
using ZetPDF;
using ZetPdf.Drawing;
using ZetPdf.Fonts;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic PDF Generation

**Before (ZetPDF — coordinate drawing):**
```csharp
using ZetPDF;
using ZetPdf.Drawing;
using System;

class Program
{
    static void Main()
    {
        var document = new PdfDocument();
        var page = document.AddPage();
        var graphics = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 24, XFontStyle.Bold);
        graphics.DrawString("Hello", titleFont, XBrushes.Black, new XPoint(50, 80));

        document.Save("output.pdf");
        Console.WriteLine("Saved output.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Hello</h1></body></html>");
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| ZetPDF | IronPDF | Notes |
|---|---|---|
| `ZetPDF` | `IronPdf` | Core namespace |
| `ZetPdf.Drawing` | N/A — use HTML/CSS | Coordinate drawing replaced by HTML input |
| N/A | `IronPdf.Rendering` | Rendering config |
| N/A | `IronPdf.Editing` | Watermark / stamp |

### Core Class Mapping

| ZetPDF Class | IronPDF Class | Description |
|---|---|---|
| `PdfDocument` (ZetPDF) | `ChromePdfRenderer` | Entry point: in IronPDF, the renderer produces a `PdfDocument` from HTML |
| `PdfPage` + `XGraphics` | HTML body | Pages and drawing surface replaced by HTML markup |
| `XFont` / `XBrushes` | CSS `font-family` / `color` | Typography via CSS, not constructor args |
| `XImage.FromFile` | HTML `<img>` tag | Images placed via markup, optional `BaseUrl` |
| `PdfReader.Open` | `PdfDocument.FromFile` | Open existing PDFs |
| N/A | `TextStamper` / HTML watermark | Native watermark APIs |

### Document Loading Methods

| Operation | ZetPDF | IronPDF |
|---|---|---|
| HTML string | Not in documented surface | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | Not in documented surface | `renderer.RenderUrlAsPdfAsync(url)` |
| Load existing PDF | `PdfReader.Open(path, PdfDocumentOpenMode.Modify)` | `PdfDocument.FromFile(path)` |
| Save | `document.Save(path)` | `pdf.SaveAs(path)` |

### Page Operations

| Operation | ZetPDF | IronPDF |
|---|---|---|
| Page count | `document.PageCount` | `pdf.PageCount` |
| Remove page | `document.Pages.RemoveAt(index)` | `pdf.RemovePages(index)` |
| Extract text | Documented via text-extraction module | `pdf.ExtractAllText()` |
| Rotate | Manual `XGraphics.RotateTransform` per page | `pdf.RotateAllPages(...)` |

### Merge / Split Operations

| Operation | ZetPDF | IronPDF |
|---|---|---|
| Merge | Manual loop: open each input, copy pages into output, save | `PdfDocument.Merge(doc1, doc2)` |
| Split | Manual page-range loop into new documents | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

ZetPDF has no native HTML-to-PDF API, so the "before" side of this migration is the coordinate-drawing code that IronPDF replaces with one HTML string.

**Before (ZetPDF — coordinate drawing):**
```csharp
using ZetPDF;
using ZetPdf.Drawing;
using System;

class HtmlToPdfBefore
{
    static void Main()
    {
        var document = new PdfDocument();
        var page = document.AddPage();
        var graphics = XGraphics.FromPdfPage(page);

        var titleFont = new XFont("Arial", 20, XFontStyle.Bold);
        var bodyFont = new XFont("Arial", 10);

        graphics.DrawString("Invoice #2024-0099", titleFont, XBrushes.Black, new XPoint(40, 50));

        // Hand-drawn 2-column "table" — every cell is a coordinate
        double x = 40, y = 90, colWidth = 200, rowHeight = 20;
        graphics.DrawRectangle(XBrushes.LightGray, x, y, colWidth * 2, rowHeight);
        graphics.DrawString("Item",  bodyFont, XBrushes.Black, x + 6, y + 14);
        graphics.DrawString("Price", bodyFont, XBrushes.Black, x + colWidth + 6, y + 14);

        y += rowHeight;
        graphics.DrawRectangle(XPens.Black, x, y, colWidth * 2, rowHeight);
        graphics.DrawString("Widget",   bodyFont, XBrushes.Black, x + 6, y + 14);
        graphics.DrawString("$149.00", bodyFont, XBrushes.Black, x + colWidth + 6, y + 14);

        document.Save("invoice.pdf");
        Console.WriteLine("Saved invoice.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var html = @"
    <html>
    <head><style>
    body { font-family: Arial; padding: 40px; }
    table { width: 100%; border-collapse: collapse; }
    th, td { border: 1px solid #ccc; padding: 6px; }
    </style></head>
    <body>
        <h1>Invoice #2024-0099</h1>
        <table>
            <tr><th>Item</th><th>Price</th></tr>
            <tr><td>Widget</td><td>$149.00</td></tr>
        </table>
    </body></html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");
Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

ZetPDF has no one-line merge helper — the documented pattern is the PDFsharp idiom of opening each input, copying pages into an output document, and saving.

**Before (ZetPDF — manual page copy):**
```csharp
using ZetPDF;
using ZetPdf.IO;
using System;

class MergeBefore
{
    static void Main()
    {
        var output = new PdfDocument();

        foreach (var path in new[] { "section1.pdf", "section2.pdf" })
        {
            var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            for (int i = 0; i < input.PageCount; i++)
                output.AddPage(input.Pages[i]);
        }

        output.Save("merged.pdf");
        Console.WriteLine("Merged sections");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;
using System.Collections.Generic;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var pdfs = new List<PdfDocument>
{
    PdfDocument.FromFile("section1.pdf"),
    PdfDocument.FromFile("section2.pdf")
};
var merged = PdfDocument.Merge(pdfs);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

ZetPDF has no built-in watermark helper. The PDFsharp-style approach is to open the document, rotate the graphics context, and `DrawString` semi-transparent text. IronPDF accepts an HTML watermark with full CSS.

**Before (ZetPDF — rotated DrawString):**
```csharp
using ZetPDF;
using ZetPdf.Drawing;
using ZetPdf.IO;
using System;

class WatermarkBefore
{
    static void Main()
    {
        var document = PdfReader.Open("document.pdf", PdfDocumentOpenMode.Modify);
        var font = new XFont("Arial", 72, XFontStyle.Bold);
        var brush = new XSolidBrush(XColor.FromArgb(50, 128, 128, 128));

        foreach (var page in document.Pages)
        {
            var graphics = XGraphics.FromPdfPage(page);
            graphics.RotateTransform(-45);
            graphics.DrawString("CONFIDENTIAL", font, brush, new XPoint(-100, 500));
        }

        document.Save("watermarked.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var pdf = PdfDocument.FromFile("document.pdf");

// https://ironpdf.com/how-to/custom-watermark/
pdf.ApplyWatermark(@"
    <div style='
        font-size: 72px;
        font-weight: bold;
        color: rgba(128, 128, 128, 0.2);
        transform: rotate(-45deg);
    '>
        CONFIDENTIAL
    </div>");

pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

ZetPDF exposes AES256 encryption via the document's security settings; IronPDF uses `SecuritySettings` on the rendered document.

**Before (ZetPDF — AES256 on document):**
```csharp
using ZetPDF;
using ZetPdf.Security;
using System;

class PasswordBefore
{
    static void Main()
    {
        var document = new PdfDocument();
        var page = document.AddPage();
        var graphics = XGraphics.FromPdfPage(page);
        graphics.DrawString("Protected", new XFont("Arial", 20), XBrushes.Black, new XPoint(40, 50));

        var security = document.SecuritySettings;
        security.UserPassword  = "open123";
        security.OwnerPassword = "admin456";
        // AES256 is the strongest documented mode in ZetPDF's security settings.

        document.Save("secured.pdf");
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

### "dotnet restore does not pull ZetPDF — CI build fails"

**Symptom:** The build server cannot find `ZetPDF` despite a clean `dotnet restore`. The local Windows dev machine builds fine because the DLL was dropped into a `lib/` folder months ago and committed by hand.

**Diagnosis:** ZetPDF is not distributed via NuGet — `dotnet restore` will never resolve it. Builds rely on a manual `<Reference Include="ZetPDF"><HintPath>...\ZetPDF.dll</HintPath></Reference>` in the `.csproj`, and the file has to be present in the repository or on a shared drive the build agent can reach.

**Resolution:** After migrating to IronPDF the project restores from NuGet like any other package, which removes this entire failure mode. During the migration window itself:

```bash
# Confirm the manual reference path resolves on the build agent
rg "ZetPDF\.dll|HintPath" *.csproj **/*.csproj
# Confirm the DLL is actually committed (or available to the agent)
ls -la lib/ZetPDF*.dll
```

Once IronPDF replaces ZetPDF, delete the `<Reference Include="ZetPDF" />` block and the `lib/ZetPDF.dll` checkin.

### "PDF renders in staging, missing fonts in production"

**Symptom:** PDF generates correctly in one environment but produces missing-glyph boxes or fallback fonts in another.

**Diagnosis:**

```bash
# Compare font packages between environments
# Staging:
dpkg -l | grep fonts

# Production:
dpkg -l | grep fonts

# Add missing font packages to production image:
# apt-get install -y fonts-liberation fonts-noto-core
```

IronPDF renders via Chromium and uses system fonts for `font-family` declarations. If a font requested in CSS isn't available, Chromium falls back to a system default which may render differently. Explicit web font loading is more reliable:

```csharp
// Embed fonts via CSS to avoid system font dependency
var html = @"
    <html>
    <head>
    <link href='https://fonts.googleapis.com/css2?family=Inter:wght@400;700' rel='stylesheet'>
    <style>body { font-family: 'Inter', Arial, sans-serif; }</style>
    </head>
    <body><h1>Report</h1></body>
    </html>";

// Or use base64-embedded font in @font-face
```

### "Output looks different from the ZetPDF reference"

**Symptom:** The IronPDF output is HTML/CSS-rendered. The ZetPDF output was pixel-positioned by coordinate. They will rarely be byte-identical, and the temptation is to spend a week trying to match the old layout exactly.

**Cause pattern:** Coordinate-drawn PDFs encode the developer's manual decisions about line spacing, font metrics, and column widths. Chromium's renderer encodes CSS box-model decisions. The two layout engines do not agree on default leading or margin collapse.

**Resolution:** Treat the migration as a chance to rebuild the layout from a designer's intent rather than from the old C# coordinates. Use `@page` and `@media print` CSS to control margins and headers/footers explicitly:

```csharp
var html = @"
    <html>
    <head><style>
        @page { size: A4; margin: 20mm 15mm; }
        @media print {
            h1 { page-break-after: avoid; }
            tr  { page-break-inside: avoid; }
        }
        body { font-family: 'Inter', Arial, sans-serif; }
    </style></head>
    <body>
        <h1>Quarterly Report</h1>
        <p>Generated content</p>
    </body></html>";
```

### "License key not found in CI pipeline"

**Symptom:** `IronPdf.Exceptions.IronPdfLicenseException` appears in CI but not in local development.

**Resolution:**

```yaml
# GitHub Actions
- name: Generate PDFs
  env:
    IRONPDF_LICENSE_KEY: ${{ secrets.IRONPDF_LICENSE_KEY }}
  run: dotnet test

# Azure DevOps
- task: DotNetCoreCLI@2
  env:
    IRONPDF_LICENSE_KEY: $(IRONPDF_LICENSE_KEY)
  inputs:
    command: test
```

```csharp
// In startup code — fail early with a clear message
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
    ?? throw new InvalidOperationException(
        "IRONPDF_LICENSE_KEY not set. Add it to CI secrets and local .env.");
```

---

## Critical Migration Notes

### Map the Coordinate Drawing Before You Remove It

ZetPDF's documented surface is small, but the codebases that use it are usually large — every page of every report is hundreds of `DrawString` calls. Before writing any HTML, capture every drawing call:

```bash
# Capture all ZetPDF drawing usage for the migration map
rg "graphics\.(DrawString|DrawRectangle|DrawImage|DrawLine)" --type cs -n -A 1 > zetpdf-usage-map.txt
cat zetpdf-usage-map.txt
# Each block becomes a section of HTML
```

Do not remove the ZetPDF code until each block of drawing calls is mapped to its HTML/CSS equivalent and rendered side-by-side against the original output.

### Page Indexing

IronPDF uses 0-based page indexing and ZetPDF's `document.Pages[i]` collection is also 0-based — but if your code was working around that by subtracting one somewhere, audit every page manipulation site before testing.

### Stream / Bytes Output Difference

The output pattern changes from a saved file to a renderer that hands back a `PdfDocument` you can save or stream:

```csharp
// ZetPDF: document.Save(path)
// IronPDF equivalent:
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
var bytes = pdf.BinaryData; // byte[] copy
// OR: pdf.SaveAs(path);
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
var pdfs = await Task.WhenAll(htmlJobs.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
}));

foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Disposal Pattern

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("output.pdf");
// pdf disposed automatically
```

---

## Migration Checklist

### Pre-Migration
- [ ] Find all ZetPDF usage (`rg "ZetPdf|ZetPDF" --type cs`)
- [ ] Capture every `XGraphics.DrawString` / `DrawRectangle` / `DrawImage` call to a usage map
- [ ] Identify the manual `<Reference Include="ZetPDF" />` block in your `.csproj`
- [ ] Identify secondary libraries used alongside ZetPDF (for HTML rendering, merge, etc.)
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility
- [ ] Add `IRONPDF_LICENSE_KEY` to CI secrets

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove the manual `<Reference Include="ZetPDF" />` block and `ZetPDF.dll` checkin
- [ ] Add license key at application startup
- [ ] Replace each block of `DrawString`/`DrawRectangle` with HTML markup + CSS
- [ ] Replace `XFont`/`XBrushes` with CSS `font-family`/`color`
- [ ] Replace manual page-break tracking with HTML `page-break-after` or automatic flow
- [ ] Replace `XImage.FromFile` + `DrawImage` with `<img>` and an optional `BaseUrl`
- [ ] Replace manual merge loop with `PdfDocument.Merge()`
- [ ] Replace rotated-`DrawString` watermark with `pdf.ApplyWatermark(html)` or `TextStamper`
- [ ] Replace ZetPDF security settings with `pdf.SecuritySettings`
- [ ] Wrap all `PdfDocument` usage in `using` blocks

### Testing
- [ ] Verify CI pipeline restores `IronPdf` cleanly from NuGet
- [ ] Test in Docker container — install the Chromium system libraries explicitly
- [ ] Compare PDF output visually against the ZetPDF reference (expect HTML-renderer differences in spacing)
- [ ] Test concurrent rendering at target throughput
- [ ] Test merge, watermark, and security features
- [ ] Verify font rendering in production container image
- [ ] Test license key is correctly injected in CI environment

### Post-Migration
- [ ] Delete `ZetPDF.dll` from `lib/`
- [ ] Remove the `<Reference Include="ZetPDF" />` block from every `.csproj`
- [ ] Remove secondary PDF libraries now replaced by IronPDF
- [ ] Add IronPDF Chromium system library installs to Dockerfile

---

## Final Thoughts

The defining moment in a ZetPDF migration is not the API swap — it is the realisation that you no longer need to write coordinate math at all. Hundreds of `DrawString` calls collapse into a single HTML template that a designer can edit directly. The migration cost lives in mapping the existing coordinate code to HTML structure, not in learning a new SDK.

The other quiet win is that `dotnet restore` becomes the source of truth for your PDF dependency. ZetPDF's ZIP-distribution model worked when build agents were long-lived Windows machines and `lib/` folders survived for years. Modern CI runners are ephemeral, and a single `dotnet add package IronPdf` line in the `.csproj` is materially easier to maintain than a manual DLL drop.

**Discussion question:** What did your ZetPDF codebase look like — mostly coordinate drawing for forms and reports, or were you using it as a thin PDF read/write layer? The shape of the legacy code usually decides whether the migration is a one-afternoon swap or a multi-week layout rebuild.
