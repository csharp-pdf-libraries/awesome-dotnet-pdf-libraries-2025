---
title: "Moving off Spire.PDF: practical IronPDF migration notes"
published: false
tags: dotnet, csharp, pdf, migration
---

The .NET upgrade is what forces the conversation. Spire.PDF has been working fine on .NET Framework, but the team is moving to .NET 8, and the NuGet page shows a version that claims compatibility — except there's a regression with a specific API path you're using, or the behavior changed between major versions, or the thread-safety characteristics shifted in a way that doesn't surface until load testing. Whatever the specific trigger, the upgrade timeline becomes the opportunity to evaluate whether this dependency is the right one for the next five years.

This article covers migrating from Spire.PDF to IronPDF. You'll have before/after code for HTML-to-PDF, merge, watermark, and password protection by the end. The comparison tables and checklist are useful regardless of the library you choose as a replacement.

---

## Why Migrate (Without Drama)

Teams migrating from Spire.PDF to IronPDF typically encounter one or more of these:

1. **.NET upgrade friction** — Spire.PDF version requirements for newer .NET versions may force API changes even when the underlying need hasn't changed.
2. **HTML rendering fidelity** — Spire.PDF's default `LoadFromHTML` path uses a legacy IE/QtWebKit engine; modern CSS (Flexbox, Grid, custom properties) often doesn't render as expected. An opt-in `ChromeHtmlConverter` exists since v10.7.21 but requires a system-installed Chrome binary.
3. **Community edition limits** — FreeSpire.PDF caps loading/creating at 10 pages and only converts the first 3 pages when targeting image/Word/HTML/XPS.
4. **Drawing API maintenance overhead** — programmatic PDF creation via a drawing API (`PdfPageBase.Canvas.DrawString`, etc.) is verbose and requires manual layout calculation.
5. **Licensing model** — Spire.PDF is sold per-developer/per-deployment, with OEM tier required for any public-facing/SaaS/Docker deployment. See [e-iceblue's pricing page](https://www.e-iceblue.com/Buy/Spire.PDF.html) for current terms.
6. **Cross-dependency accumulation** — complex workflows may require `Spire.PDF` + `Spire.Doc` + `Spire.XLS`, adding multiple packages (and licenses).
7. **Image-based text output** — on the legacy HTML path, Spire.PDF can render text as bitmap images, producing PDFs that aren't searchable, selectable, or accessible to screen readers.
8. **PDF/A and compliance** — PDF/A support varies by edition.
9. **Missing features** — digital signatures, advanced annotations, or specific PDF standards may vary by edition.
10. **API verbosity** — the drawing API for layout can be significantly more verbose than HTML template rendering for report-style documents.

### Comparison Table

| Aspect | Spire.PDF | IronPDF |
|---|---|---|
| Focus | PDF creation, editing, conversion, text extraction | HTML-to-PDF + PDF manipulation |
| Pricing | Free edition (page-limited) + paid (per-developer/per-deployment) | Commercial license (per-developer/per-project) |
| API Style | Drawing API + document model; verbose for layout | HTML renderer + manipulation objects |
| Learning Curve | Medium-High for layout code; Low for simple ops | Low for web devs; HTML/CSS is the input |
| HTML Rendering | Legacy IE/QtWebKit by default; opt-in `ChromeHtmlConverter` (system Chrome required) since v10.7.21 | Bundled Chromium |
| Page Indexing | 0-based | 0-based |
| Thread Safety | Concurrent rendering behavior varies between major releases | Designed for concurrent renderer instances; see [parallel rendering guide](https://ironpdf.com/examples/parallel/) |
| Namespace | `Spire.Pdf`, `Spire.Pdf.Graphics` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Spire.PDF | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | `PdfDocument.LoadFromHTML()` / `SaveToFile` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low–Medium |
| Drawing text/shapes | `page.Canvas.DrawString()` etc. | HTML/CSS template | High (rewrite) |
| Save to file | `doc.SaveToFile(path)` | `pdf.SaveAs(path)` | Low |
| Save to stream | `doc.SaveToStream(stream)` | `pdf.Stream` | Low |
| Custom page size | `PdfPageSettings.Size` | `RenderingOptions.PaperSize` | Low |
| Headers/footers | Drawing API per page | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Merge PDFs | `PdfDocument.InsertPageRange()` / `AppendPage()` | `PdfDocument.Merge()` | Low–Medium |
| Watermark | Drawing layer per page | `TextStamper` / `ImageStamper` | Low |
| Password protection | `PdfSecurity.OwnerPassword` / `UserPassword` | `pdf.SecuritySettings` | Low |
| Text extraction | `PdfPageBase.ExtractText()` | `pdf.ExtractAllText()` | Low |
| Word/Excel to PDF | Cross-product (Spire.Doc/XLS) | N/A — HTML input path | High |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Primary use case is HTML-to-PDF from web templates | Switch — Chromium rendering handles modern CSS |
| Extensive use of Spire's drawing API for complex layouts | Evaluate migration cost — drawing API → HTML rewrite is significant |
| Word/Excel-to-PDF conversion is the primary need | Spire may be better suited; IronPDF is HTML-first |
| .NET 8 migration blocked by Spire compatibility issue | Switch resolves the blocker; assess feature parity first |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All Spire.PDF References

```bash
# Find Spire.PDF API usage
rg -l "Spire\.Pdf|PdfDocument|PdfPageBase|LoadFromHTML" --type cs
rg "Spire\.Pdf|PdfDocument\b" --type cs -n

# Find drawing API usage (more complex migration)
rg "Canvas\.Draw|DrawString|DrawRectangle|DrawImage" --type cs -n

# Find Spire packages in project files
grep -r "Spire\.PDF\|FreeSpire\.PDF\|Spire\.Doc\|Spire\.XLS" *.csproj **/*.csproj 2>/dev/null

# Distinguish HTML-to-PDF from drawing API usage
rg "LoadFromHTML|LoadFromUrl" --type cs -n
```

### Uninstall / Install

```bash
# Remove Spire packages
dotnet remove package Spire.PDF      # or FreeSpire.PDF
dotnet remove package Spire.Doc      # if present, only for cross-format conversion
dotnet remove package Spire.XLS      # if present

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
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Security;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
```

### Step 3 — Basic HTML to PDF

**Before (Spire.PDF):**
```csharp
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System;

class Program
{
    static void Main()
    {
        var doc = new PdfDocument();
        var setting = new PdfPageSettings();
        setting.Size = PdfPageSize.A4;
        var layout = new PdfHtmlLayoutFormat();
        layout.IsWaiting = false;

        // HTML-string overload: LoadFromHTML(string, bool autoDetectPageBreak,
        //                                    PdfPageSettings, PdfHtmlLayoutFormat)
        doc.LoadFromHTML(
            "<html><body><h1>Hello</h1></body></html>",
            true,
            setting,
            layout);

        doc.SaveToFile("output.pdf");
        doc.Close();
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

| Spire.PDF | IronPDF | Notes |
|---|---|---|
| `Spire.Pdf` | `IronPdf` | Core namespace |
| `Spire.Pdf.Graphics` | `IronPdf.Rendering` | Drawing API replaced by HTML/CSS; rendering options live on `ChromePdfRenderer` |
| `Spire.Pdf.HtmlConverter` | `IronPdf` | `ChromePdfRenderer` |
| `Spire.Pdf.Security` | `IronPdf` | Security on `pdf.SecuritySettings` |

### Core Class Mapping

| Spire.PDF Class | IronPDF Class | Description |
|---|---|---|
| `PdfDocument` (Spire) | `ChromePdfRenderer` | HTML-to-PDF renderer |
| `PdfDocument` (Spire) | `PdfDocument` (IronPDF) | PDF manipulation object |
| `PdfPageBase` / `PdfPage` | N/A — no drawing API | Drawing replaced by HTML/CSS |
| `PdfSecurity` | `pdf.SecuritySettings` | Password + permission settings |

### Document Loading Methods

| Operation | Spire.PDF | IronPDF |
|---|---|---|
| HTML string | `doc.LoadFromHTML(html, autoDetectPageBreak, settings, layout)` | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | `doc.LoadFromHTML(url, enableJS, enableHyperlinks, autoDetectPageBreak)` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML file | Read file contents, then `LoadFromHTML(string, ...)` | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Load existing PDF | `new PdfDocument(); doc.LoadFromFile(path)` | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | Spire.PDF | IronPDF |
|---|---|---|
| Page count | `doc.Pages.Count` | `pdf.PageCount` |
| Remove page | `doc.Pages.RemoveAt(index)` | `pdf.RemovePages(index)` |
| Extract text | `doc.Pages[i].ExtractText()` | `pdf.ExtractAllText()` (or `pdf.Pages[i].Text`) |
| Rotate | `page.Rotation = PdfPageRotateAngle.*` | `pdf.RotateAllPages(PdfRotation.Clockwise90)` |

### Merge / Split Operations

| Operation | Spire.PDF | IronPDF |
|---|---|---|
| Merge | `doc1.InsertPageRange(doc2, 0, doc2.Pages.Count - 1)` | `PdfDocument.Merge(doc1, doc2)` |
| Split | `doc.Split(outputPaths, pageRanges)` | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (Spire.PDF):**
```csharp
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System;

class HtmlToPdfBefore
{
    static void Main()
    {
        var doc = new PdfDocument();
        var setting = new PdfPageSettings();
        setting.Size = PdfPageSize.A4;
        var layout = new PdfHtmlLayoutFormat();
        layout.IsWaiting = false;

        string html = "<html><body><h1>Q3 Invoice #4421</h1><p>Total: $8,400</p></body></html>";

        // Legacy IE/QtWebKit engine on the default path produces image-based text
        doc.LoadFromHTML(html, true, setting, layout);

        doc.SaveToFile("invoice.pdf");
        doc.Close();
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
    <head>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; }
        h1 { font-size: 22px; }
        .total { font-weight: bold; font-size: 16px; margin-top: 20px; }
    </style>
    </head>
    <body>
        <h1>Q3 Invoice #4421</h1>
        <p>Total: $8,400</p>
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

**Before (Spire.PDF):**
```csharp
using Spire.Pdf;
using System;

class MergeBefore
{
    static void Main()
    {
        var pdf1 = new PdfDocument();
        pdf1.LoadFromFile("section1.pdf");

        var pdf2 = new PdfDocument();
        pdf2.LoadFromFile("section2.pdf");

        // Insert all pages from pdf2 into pdf1
        pdf1.InsertPageRange(pdf2, 0, pdf2.Pages.Count - 1);

        pdf1.SaveToFile("merged.pdf");
        pdf1.Close();
        pdf2.Close();
        Console.WriteLine("Saved merged.pdf");
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

// Render sections concurrently
var results = await Task.WhenAll(
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 1: Overview</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 2: Detail</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("merged.pdf");

Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (Spire.PDF drawing API watermark):**
```csharp
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System;
using System.Drawing;

class WatermarkBefore
{
    static void Main()
    {
        var doc = new PdfDocument();
        doc.LoadFromFile("source.pdf");

        // Spire watermark via drawing API on each page
        foreach (PdfPageBase page in doc.Pages)
        {
            var font = new PdfFont(PdfFontFamily.Helvetica, 60f);
            var brush = new PdfSolidBrush(Color.FromArgb(30, Color.Gray));

            var state = page.Canvas.Save();
            page.Canvas.TranslateTransform(
                (float)(page.ActualSize.Width / 2),
                (float)(page.ActualSize.Height / 2));
            page.Canvas.RotateTransform(-45f);
            page.Canvas.DrawString("DRAFT", font, brush, 0, 0,
                new PdfStringFormat(PdfTextAlignment.Center));
            page.Canvas.Restore(state);
        }

        doc.SaveToFile("watermarked.pdf");
        doc.Close();
        Console.WriteLine("Drawing API watermark applied");
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
    FontColor = "#808080",
    Opacity = 15,                          // 0-100 integer scale
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

**Before (Spire.PDF):**
```csharp
using Spire.Pdf;
using Spire.Pdf.Security;
using System;

class PasswordBefore
{
    static void Main()
    {
        var doc = new PdfDocument();
        doc.LoadFromFile("source.pdf");

        // Spire encryption + passwords
        doc.Security.Encrypt("open123", "admin456", PdfPermissionsFlags.Print, PdfEncryptionKeySize.Key128Bit);

        doc.SaveToFile("secured.pdf");
        doc.Close();
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
    "<html><body><h1>Protected Document</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### The Drawing API Gap

If your codebase uses Spire.PDF's drawing API (`Canvas.DrawString`, `DrawRectangle`, `DrawImage`, `PdfFont`, `PdfBrush`, etc.) to construct document layouts, that code has no direct IronPDF equivalent. IronPDF is HTML-first — the drawing operations need to be rewritten as HTML/CSS.

Audit how much of your codebase is drawing API vs. HTML conversion:

```bash
# Drawing API usage (higher migration cost)
rg "Canvas\.Draw|DrawString|DrawLine|DrawRectangle|DrawImage" --type cs -n | wc -l

# HTML/URL conversion usage (lower migration cost)
rg "LoadFromHTML" --type cs -n | wc -l
```

If the drawing API count dominates, estimate the HTML template rewrite effort before committing to a timeline. A complex multi-column report layout done in drawing API code may take several hours to recreate in HTML/CSS.

### Free Edition Page Limit

FreeSpire.PDF silently truncates output beyond 10 pages on load/create operations, and beyond 3 pages when converting to image/Word/HTML/XPS. After migrating, verify that previously-truncated PDFs now render fully.

### API Versioning

Spire.PDF's API surface has changed between major versions — especially the HTML-to-PDF path. Spire 10.7.21 (mid-2024) added a `ChromeHtmlConverter` opt-in that uses a system-installed Chrome binary; the original `LoadFromHTML` API still defaults to the legacy IE/QtWebKit engine. If your code relies on a specific engine's behavior, confirm the equivalent IronPDF API is stable across your target version range before migrating.

### Merge API Difference

Spire's merge pattern (`InsertPageRange` / `AppendPage`) is append-page-by-page. IronPDF's `PdfDocument.Merge()` takes complete document objects:

```csharp
// Spire pattern — page-range insert:
// pdf1.InsertPageRange(pdf2, 0, pdf2.Pages.Count - 1);

// IronPDF — document-level merge:
var merged = PdfDocument.Merge(doc1, doc2);
// https://ironpdf.com/how-to/merge-or-split-pdfs/
```

### Page Indexing

Both Spire.PDF and IronPDF use 0-based page indexing. This should be a non-issue if you're migrating page operations, but audit explicitly.

---

## Performance Considerations

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/examples/parallel/
var htmlJobs = reportDataList.Select(data => BuildHtml(data)).ToArray();

var pdfs = await Task.WhenAll(htmlJobs.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Generated {pdfs.Length} PDFs");
foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

// Spire: doc.Close() required
// IronPDF: 'using' handles disposal

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// Spire pattern: doc.SaveToStream(stream)
// IronPDF: copy pdf.Stream to your stream
using var ms = new MemoryStream();
pdf.Stream.CopyTo(ms);
return ms.ToArray();
// pdf disposed at end of 'using' block
```

### Document Close Pattern

Spire.PDF requires explicit `doc.Close()` for resource cleanup. With IronPDF's `using` pattern, this is handled automatically:

```csharp
// Spire — explicit close required:
// var doc = new PdfDocument();
// try { ... } finally { doc.Close(); }

// IronPDF — using handles it:
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("output.pdf");
// Disposed automatically
```

---

## Migration Checklist

### Pre-Migration
- [ ] Count HTML-to-PDF vs drawing API usage (`rg "Canvas\.Draw" --type cs | wc -l`)
- [ ] Verify Spire.PDF edition (Free vs paid — check page limit constraints)
- [ ] Identify all Spire cross-product dependencies (Spire.Doc, Spire.XLS)
- [ ] Document current .NET version issue that triggered the evaluation
- [ ] Measure baseline render times for HTML-to-PDF operations
- [ ] Obtain IronPDF license key
- [ ] Verify IronPDF target framework compatibility

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove Spire packages (Spire.PDF, FreeSpire.PDF, Spire.Doc, Spire.XLS as applicable)
- [ ] Add license key at application startup
- [ ] Replace `LoadFromHTML()` with `ChromePdfRenderer.RenderHtmlAsPdfAsync()`
- [ ] Replace URL conversion overload of `LoadFromHTML()` with `RenderUrlAsPdfAsync()`
- [ ] Replace `doc.SaveToFile()` with `pdf.SaveAs()`
- [ ] Replace `doc.SaveToStream()` with `pdf.Stream.CopyTo()` or `pdf.BinaryData`
- [ ] Replace Spire merge (`InsertPageRange` / `AppendPage` pattern) with `PdfDocument.Merge()`
- [ ] Replace drawing API watermark with `TextStamper` / `ImageStamper`
- [ ] Replace `doc.Security.*` with `pdf.SecuritySettings.*`

### Testing
- [ ] Render each HTML template and compare visual output
- [ ] Verify previously-truncated Free edition PDFs now render all pages
- [ ] Test merge output page count and order
- [ ] Test password protection — verify opens with correct credentials
- [ ] Benchmark render time vs Spire baseline
- [ ] Test in target .NET version (the original upgrade trigger)
- [ ] Verify disposal — check for resource leaks with Stopwatch + GC.Collect pattern

### Post-Migration
- [ ] Remove Spire NuGet packages
- [ ] Remove Spire license files from deployment configs if present
- [ ] Remove cross-product Spire packages no longer needed
- [ ] Archive drawing API code before deletion for reference

---

## Done Migrating? Here's What's Next

The .NET version trigger that started this evaluation tends to resolve cleanly once the HTML-to-PDF path is migrated. The drawing API code is the variable — measure how much of your PDF generation uses it before committing to a timeline.

For the drawing API sections that need rewriting as HTML, a useful benchmark: compare the line count of the drawing API code with the HTML/CSS equivalent. In most report scenarios, the HTML version is significantly shorter.

**Discussion question:** After migrating, what were your before/after bundle size or render time differences — and did the change in rendering engine (Spire's own vs Chromium) produce any visual output differences you had to address in your templates?
