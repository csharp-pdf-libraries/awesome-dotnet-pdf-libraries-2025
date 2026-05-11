---
title: "From Syncfusion PDF to IronPDF: what actually changes in your code"
published: false
tags: dotnet, csharp, pdf, migration
---

Version pinning is a quiet tax. You're on Syncfusion.Pdf.Net.Core 21.x because the upgrade to 22.x broke something in your PDF text extraction pipeline, and the fix required touching ten files and a full regression pass. The next major release came while that regression was still open. Now you're two versions behind, the new .NET runtime support you need is in the version you can't upgrade to, and the options are narrowing. Before spending a sprint on a Syncfusion upgrade that might break something else, teams often ask whether the migration cost would be similar if they switched entirely.

This article covers migrating from Syncfusion PDF to IronPDF. You'll have working before/after code for HTML-to-PDF, merge, watermark, and password protection by the end. The comparison tables and checklist are useful regardless of which path you take.

---

## Why Migrate (Without Drama)

Teams evaluating Syncfusion PDF alternatives commonly cite:

1. **Version pinning friction** — Syncfusion's major releases occasionally include breaking changes; skipping versions creates an upgrade debt that compounds.
2. **Bundle-only licensing** — Syncfusion doesn't sell the PDF library as a standalone SKU; the smallest unit is the Document SDK bundle (PDF + Word + Excel + PowerPoint), or the full Essential Studio suite.
3. **Community license conditions** — the free tier requires under $1M annual revenue AND 5 or fewer developers AND 10 or fewer total employees AND a $3M lifetime cap on outside capital. All four conditions, not any.
4. **HTML converter package fragmentation** — HTML-to-PDF lives in a separate package (`Syncfusion.HtmlToPdfConverter.Net.Windows` / `.Linux` / `.Mac` / `.Aws`) with platform-specific Blink binaries.
5. **Drawing API verbosity** — generating layout-heavy PDFs via `PdfPage`, `PdfGraphics`, `PdfFont` requires manual layout calculation and coordinate math.
6. **Native binary management** — the HTML converter pulls in platform-specific native binaries via the matching NuGet package, and the deployment story changes per OS.
7. **Per-developer subscription pricing** — Syncfusion is a per-developer annual subscription (minimum 1-year term), so costs scale linearly with team size.
8. **Bundle bloat** — the Document SDK ships Word/Excel/PowerPoint libraries you may not need; full Essential Studio adds 1000+ UI components on top.
9. **.NET version support lag** — new .NET versions may not be supported immediately in every Syncfusion package.
10. **Multiple transitive packages** — Syncfusion components pull in multiple Syncfusion packages (`Syncfusion.Pdf.Net.Core`, `Syncfusion.Compression.Net.Core`, `Syncfusion.Licensing`, the platform-specific HTML converter); IronPDF is a single NuGet.

### Comparison Table

| Aspect | Syncfusion PDF | IronPDF |
|---|---|---|
| Focus | PDF creation/editing via drawing API + HTML converter | HTML-to-PDF + PDF manipulation |
| Purchase Model | Document SDK bundle or full Essential Studio (no per-library SKU) | Standalone IronPDF license |
| API Style | Drawing API + separate HtmlToPdfConverter | `ChromePdfRenderer` + `PdfDocument` |
| Learning Curve | Medium-High for drawing API; Low for converter | Low for .NET devs; HTML/CSS is the input |
| HTML Rendering | Blink (current default); QtWebKit (legacy) | Embedded Chromium |
| Page Indexing | 0-based | 0-based |
| Namespace | `Syncfusion.Pdf`, `Syncfusion.HtmlConverter` | `IronPdf` |
| Dependencies | Multiple Syncfusion packages + platform-specific HTML converter | Single NuGet |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Syncfusion PDF | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | `HtmlToPdfConverter.Convert(html)` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | `converter.Convert(url)` | `renderer.RenderUrlAsPdfAsync(url)` | Low |
| Save to file | `doc.Save(stream)` | `pdf.SaveAs(path)` | Low |
| Save to stream | `doc.Save(stream)` | `pdf.Stream` / `pdf.BinaryData` | Low |
| Custom page size | `document.PageSettings.Size` | `RenderingOptions.PaperSize` | Low |
| Headers/footers | `PdfPageTemplateElement` or HTML converter options | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Merge PDFs | `PdfDocument.ImportPageRange()` | `PdfDocument.Merge()` | Low |
| Watermark | `PdfGraphics` rotate + draw, or `pdf.ApplyWatermark` | `TextStamper` / `ImageStamper` | Low |
| Password protection | `document.Security.UserPassword` | `pdf.SecuritySettings.UserPassword` | Low |
| Text extraction | `PdfTextExtractor.ExtractText()` | `pdf.ExtractAllText()` | Low |
| Drawing API layouts | `PdfPage.Graphics.DrawString()` etc. | HTML/CSS template | High (rewrite) |
| Digital signatures | `PdfSignature` | `pdf.SignWithFile()` | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Stuck on old version due to breaking changes | Switch resolves the version-pinning constraint entirely |
| Primarily HTML-to-PDF with modern CSS | Switch — both use modern engines, but IronPDF ships Chromium in-package |
| Extensive drawing API use for complex layouts | Evaluate migration cost — drawing API to HTML rewrite scope |
| Need PDF/A, digital signatures, form fields | Both libraries cover these; confirm specific feature parity for your use cases |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All Syncfusion PDF References

```bash
# Find Syncfusion PDF API usage
rg -l "Syncfusion\.Pdf|HtmlToPdfConverter|PdfDocument|PdfPage\b" --type cs
rg "Syncfusion\.Pdf|HtmlToPdfConverter" --type cs -n

# Find drawing API usage (higher migration cost)
rg "PdfPage\b|PdfGraphics|DrawString|DrawLine|PdfFont\b" --type cs -n | wc -l

# Find HTML converter usage (lower migration cost)
rg "HtmlToPdfConverter|\.Convert\(.*html|\.Convert\(.*Uri" --type cs -n

# Find Syncfusion in project files
grep -r "Syncfusion" *.csproj **/*.csproj 2>/dev/null
```

### Uninstall / Install

```bash
# Remove Syncfusion packages (substitute the platform variant you actually used:
# .Net.Windows / .Net.Linux / .Net.Mac / .Net.Aws for the HTML converter).
dotnet remove package Syncfusion.Pdf.Net.Core
dotnet remove package Syncfusion.HtmlToPdfConverter.Net.Windows
dotnet remove package Syncfusion.Compression.Net.Core
dotnet remove package Syncfusion.Licensing

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

// Also remove Syncfusion license setup:
// Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR-LICENSE-KEY");
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf.Security;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
```

### Step 3 — Basic Conversion

**Before (Syncfusion HtmlToPdfConverter):**
```csharp
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // Syncfusion license required first
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR-LICENSE-KEY");

        // HTML converter (separate package from core PDF)
        var converter = new HtmlToPdfConverter();
        var settings = new BlinkConverterSettings();
        settings.PdfPageSize = Syncfusion.Pdf.PdfPageSize.A4;
        converter.ConverterSettings = settings;

        var doc = converter.Convert("<html><body><h1>Hello</h1></body></html>", string.Empty);
        using var fs = new FileStream("output.pdf", FileMode.Create);
        doc.Save(fs);
        doc.Close(true);
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
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Hello</h1></body></html>");
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| Syncfusion | IronPDF | Notes |
|---|---|---|
| `Syncfusion.Pdf` | `IronPdf` | Core namespace |
| `Syncfusion.HtmlConverter` | `IronPdf` (renderer included) | No separate package |
| `Syncfusion.Pdf.Security` | `IronPdf` (security on PdfDocument) | Settings on output object |

### Core Class Mapping

| Syncfusion Class | IronPDF Class | Description |
|---|---|---|
| `HtmlToPdfConverter` | `ChromePdfRenderer` | HTML-to-PDF; no separate package needed |
| `PdfDocument` (Syncfusion) | `PdfDocument` (IronPDF) | Different API surface |
| `PdfLoadedDocument` | `PdfDocument.FromFile()` | Loaded PDF object |
| `document.Security` | `pdf.SecuritySettings` | Password and permission settings |
| `PdfGraphics` watermark | `TextStamper` / `ImageStamper` | Watermark/stamp objects |

### Document Loading Methods

| Operation | Syncfusion | IronPDF |
|---|---|---|
| HTML string | `converter.Convert(html, baseUrl)` | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | `converter.Convert(url)` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML file | `converter.Convert(File.ReadAllText(path), baseUrl)` | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Load existing PDF | `new PdfLoadedDocument(stream)` | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | Syncfusion | IronPDF |
|---|---|---|
| Page count | `doc.Pages.Count` | `pdf.PageCount` |
| Remove page | `doc.Pages.RemoveAt(index)` | `pdf.RemovePages(index)` |
| Extract text | `new PdfTextExtractor(page).ExtractText()` | `pdf.ExtractAllText()` |
| Rotate | `page.Rotation = PdfPageRotateAngle.*` | `pdf.RotateAllPages(PdfRotation.*)` |

### Merge / Split Operations

| Operation | Syncfusion | IronPDF |
|---|---|---|
| Merge | `merged.ImportPageRange(doc, 0, doc.Pages.Count - 1)` | `PdfDocument.Merge(doc1, doc2)` |
| Split | `target.ImportPageRange(source, start, end)` | `pdf.CopyPages(start, end)` — see the [merge/split guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before (Syncfusion HtmlToPdfConverter):**
```csharp
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using System;
using System.IO;

class HtmlToPdfBefore
{
    static void Main()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
            Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY"));

        var html = @"
            <html><head>
            <style>body { font-family: Arial; padding: 40px; }
            table { width:100%; border-collapse:collapse; }
            th, td { border:1px solid #ccc; padding:6px; }</style>
            </head><body>
            <h1>Invoice #9901</h1>
            <table><tr><th>Item</th><th>Amount</th></tr>
            <tr><td>Consulting</td><td>$3,200</td></tr></table>
            </body></html>";

        var converter = new HtmlToPdfConverter();
        var settings = new BlinkConverterSettings();
        settings.PdfPageSize = Syncfusion.Pdf.PdfPageSize.A4;
        converter.ConverterSettings = settings;

        var doc = converter.Convert(html, string.Empty);
        using var fs = new FileStream("invoice.pdf", FileMode.Create);
        doc.Save(fs);
        doc.Close(true);
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
    <html><head>
    <style>body { font-family: Arial; padding: 40px; }
    table { width:100%; border-collapse:collapse; }
    th, td { border:1px solid #ccc; padding:6px; }</style>
    </head><body>
    <h1>Invoice #9901</h1>
    <table><tr><th>Item</th><th>Amount</th></tr>
    <tr><td>Consulting</td><td>$3,200</td></tr></table>
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

**Before (Syncfusion):**
```csharp
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using System;
using System.IO;

class MergeBefore
{
    static void Main()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
            Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY"));

        // Load two existing PDFs and merge with ImportPageRange
        using var stream1 = new FileStream("section1.pdf", FileMode.Open, FileAccess.Read);
        using var stream2 = new FileStream("section2.pdf", FileMode.Open, FileAccess.Read);
        var doc1 = new PdfLoadedDocument(stream1);
        var doc2 = new PdfLoadedDocument(stream2);

        var merged = new PdfDocument();
        merged.ImportPageRange(doc1, 0, doc1.Pages.Count - 1);
        merged.ImportPageRange(doc2, 0, doc2.Pages.Count - 1);

        using var output = new FileStream("merged.pdf", FileMode.Create);
        merged.Save(output);

        merged.Close(true);
        doc1.Close(true);
        doc2.Close(true);
        Console.WriteLine("Saved merged.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();

var results = await Task.WhenAll(
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 1</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 2</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(new List<PdfDocument> { results[0], results[1] });
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (Syncfusion drawing layer watermark):**
```csharp
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Drawing;
using System;

class WatermarkBefore
{
    static void Main()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
            Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY"));

        var doc = new PdfLoadedDocument("source.pdf");

        foreach (PdfPageBase page in doc.Pages)
        {
            var graphics = page.Graphics;
            var state = graphics.Save();

            // Move origin to page center, then rotate
            graphics.TranslateTransform(page.Size.Width / 2, page.Size.Height / 2);
            graphics.RotateTransform(-45);

            var font = new PdfStandardFont(PdfFontFamily.Helvetica, 60);
            var brush = new PdfSolidBrush(new PdfColor(128, 128, 128, 30));
            graphics.DrawString("DRAFT", font, brush, new PointF(0, 0));

            graphics.Restore(state);
        }

        doc.Save("watermarked.pdf");
        doc.Close(true);
        Console.WriteLine("Watermark applied via drawing API");
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
    "<html><body><h1>Document</h1></body></html>"
);

// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronSoftware.Drawing.Color.Gray,
    Opacity = 15, // 0-100 integer scale
    Rotation = -45,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (Syncfusion):**
```csharp
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Security;
using Syncfusion.Drawing;
using System;
using System.IO;

class PasswordBefore
{
    static void Main()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
            Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY"));

        var doc = new PdfDocument();
        var page = doc.Pages.Add();
        var font = new PdfStandardFont(PdfFontFamily.Helvetica, 24);
        page.Graphics.DrawString("Confidential", font, PdfBrushes.Black, new PointF(40, 40));

        doc.Security.Algorithm = PdfEncryptionAlgorithm.AES;
        doc.Security.KeySize = PdfEncryptionKeySize.Key256Bit;
        doc.Security.UserPassword = "open123";
        doc.Security.OwnerPassword = "admin456";

        using var fs = new FileStream("secured.pdf", FileMode.Create);
        doc.Save(fs);
        doc.Close(true);
        Console.WriteLine("Saved secured.pdf");
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
    "<html><body><h1>Confidential Document</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### Syncfusion License Registration Removal

Syncfusion requires license registration at startup: `SyncfusionLicenseProvider.RegisterLicense("key")`. Remove this and replace with IronPDF's key assignment:

```bash
# Find Syncfusion license registration to remove
rg "SyncfusionLicenseProvider|RegisterLicense" --type cs -n
```

### Drawing API Gap

If your codebase uses `PdfPage.Graphics.DrawString`, `PdfGraphics.DrawRectangle`, `PdfFont`, `PdfBrush` — those are the drawing API and have no direct IronPDF equivalent. IronPDF is HTML-first; drawing operations rewrite as HTML/CSS:

```bash
# Audit drawing API usage
rg "DrawString|DrawLine|DrawRectangle|PdfGraphics|PdfFont\b|PdfBrush\b" --type cs -n | wc -l
```

If this count is high, estimate the HTML template rewrite effort before setting a timeline.

### HtmlToPdfConverter Package Separation

Syncfusion's HTML-to-PDF lives in a platform-specific NuGet package (`Syncfusion.HtmlToPdfConverter.Net.Windows` / `.Linux` / `.Mac` / `.Aws`) that pulls in the Blink binaries for that OS. In IronPDF, HTML rendering is in the core package — there's no separate package or platform variant to manage.

### `doc.Close(true)` Pattern

Syncfusion's `PdfDocument.Close(bool)` is explicit resource release. IronPDF uses standard `using`:

```csharp
// Syncfusion:
// doc.Save(stream);
// doc.Close(true);

// IronPDF:
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("out.pdf");
// pdf disposed at end of 'using' block
```

### Page Indexing

Both Syncfusion PDF and IronPDF use 0-based page indexing. This typically requires no changes for existing page manipulation code.

---

## Performance Considerations

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/examples/parallel/
var htmlJobs = documentData.Select(data => BuildHtml(data)).ToArray();

var pdfs = await Task.WhenAll(htmlJobs.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Rendered {pdfs.Length} PDFs in parallel");
foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Renderer Warm-Up

```csharp
using IronPdf;

// Amortize Chromium initialization at application startup
var renderer = new ChromePdfRenderer();
using var _ = await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");
// Renderer ready for production traffic
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// Syncfusion: doc.Save(stream); doc.Close(true);
// IronPDF: pdf.Stream or pdf.BinaryData; disposed by 'using'
var bytes = pdf.BinaryData;
// pdf disposed at end of 'using' block
```

---

## Migration Checklist

### Pre-Migration
- [ ] Count drawing API vs HTML converter usage in codebase
- [ ] Verify Syncfusion community/paid license thresholds against current usage
- [ ] Identify which Syncfusion HTML converter platform package is in use (`Net.Windows` / `Net.Linux` / `Net.Mac` / `Net.Aws`)
- [ ] Document native binary/system library dependencies of the current converter
- [ ] Identify secondary libraries used alongside Syncfusion
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF .NET version compatibility
- [ ] List all Syncfusion-specific config files and license registration calls

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove Syncfusion NuGet packages
- [ ] Remove `SyncfusionLicenseProvider.RegisterLicense()` call
- [ ] Add IronPDF license key at application startup
- [ ] Replace `HtmlToPdfConverter.Convert()` with `ChromePdfRenderer.RenderHtmlAsPdfAsync()`
- [ ] Replace URL conversion with `RenderUrlAsPdfAsync()`
- [ ] Replace `doc.Save(stream)` + `doc.Close(true)` with `pdf.SaveAs(path)` in `using` block
- [ ] Replace `PdfLoadedDocument` + `ImportPageRange` merge pattern with `PdfDocument.Merge()`
- [ ] Replace drawing API watermark with `TextStamper` / `ImageStamper`
- [ ] Replace `doc.Security.*` with `pdf.SecuritySettings.*`

### Testing
- [ ] Render each HTML template and compare visual output
- [ ] Test that modern CSS (flex, grid) renders correctly
- [ ] Verify merge output page count and order
- [ ] Test password protection
- [ ] Test concurrent rendering under load
- [ ] Benchmark render time vs Syncfusion baseline
- [ ] Verify deployment works without Syncfusion platform-specific HTML converter packages

### Post-Migration
- [ ] Remove all Syncfusion NuGet packages
- [ ] Remove Syncfusion license key from app config / secrets
- [ ] Remove native Syncfusion converter binaries from deployment
- [ ] Archive Syncfusion drawing API code for reference before deletion

---

## Conclusion

The version pinning problem — the original trigger in many Syncfusion migrations — is solved structurally: IronPDF versioning is independent of Syncfusion's release cycle. Once migrated, you track one dependency instead of `Syncfusion.Pdf.Net.Core` plus the platform-specific HTML converter plus `Syncfusion.Compression.Net.Core` plus `Syncfusion.Licensing`.

The most variable migration cost is the drawing API. HTML converter usage (most of this article) is a find-and-replace exercise. Drawing API code that constructs layouts with `PdfGraphics.DrawString` requires HTML template rewrites, and those take proportional time.

**Discussion question:** What edge cases did you hit that this article didn't cover — particularly around which Syncfusion HTML converter platform package you were on, or specific drawing API features that needed HTML equivalent replacements?
