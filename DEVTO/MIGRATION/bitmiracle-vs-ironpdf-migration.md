---
title: "Migrating from BitMiracle Docotic.Pdf to IronPDF: the honest walkthrough"
published: false
tags: dotnet, csharp, pdf, migration
---

Docotic.Pdf is a genuinely well-engineered library. The API is clean, the NuGet package is lean, and it handles PDF parsing and programmatic construction reliably across platforms. The migration decision rarely comes from Docotic.Pdf breaking — it comes from how its HTML-to-PDF story is packaged.

The most common trigger: someone adds a feature that requires rendering a complex HTML template to PDF. Docotic.Pdf supports HTML rendering, but only via the separate `BitMiracle.Docotic.Pdf.HtmlToPdf` add-on, which exposes an async-only `HtmlConverter` that downloads its own Chromium on first use. Teams that started on Docotic for parsing or programmatic construction often end up dragging the add-on into the dependency tree just to render one invoice template — and once that's in, the licensing and packaging story starts looking like a candidate for consolidation onto a single renderer-first library.

This article covers the troubleshooting patterns and API mapping for migrating from BitMiracle Docotic.Pdf to **IronPDF**. The migration is relatively low-drama because both are managed .NET libraries with no COM dependencies. The complexity concentrates in two places: the different mental model (Docotic's document construction plus a separate add-on for HTML vs IronPDF's unified renderer + document model), and any text extraction or annotation code that relies on Docotic's parse-side API.

---

## Why Migrate (Without Drama)

Neutral triggers teams run into:

1. **HTML-to-PDF requires an add-on** — Docotic.Pdf's HTML rendering lives in the separate `BitMiracle.Docotic.Pdf.HtmlToPdf` package. That add-on uses an async-only `HtmlConverter` and downloads its own Chromium on first use, so the dependency tree grows when you need HTML support.
2. **CSS rendering fidelity** — Programmatic document construction in Docotic doesn't match what a designer produces in HTML/CSS. IronPDF's Chromium renderer closes this gap for HTML-based workflows without a separate add-on.
3. **Template maintenance burden** — Programmatic PDF construction (positioning text objects, drawing borders) requires developer changes for every layout update. HTML templates can be maintained by non-developers.
4. **Font handling complexity** — Docotic requires explicit font loading and glyph management on the canvas side. IronPDF's Chromium renderer handles font resolution as a browser would.
5. **Deployment simplicity** — Both are managed .NET, but IronPDF's all-in-one approach (render + edit + security) reduces the number of libraries and add-on packages in the dependency tree.
6. **Async / parallel patterns** — Both libraries support concurrent use; evaluate under your specific load patterns.
7. **API surface for new developers** — `ChromePdfRenderer.RenderHtmlAsPdf(html)` is a shorter onboarding path than document construction for HTML-heavy workloads.
8. **PDF/A output workflow** — Both support PDF/A; test against your target compliance profile to compare output.
9. **License changes** — Evaluate current Docotic.Pdf pricing at bitmiracle.com for your usage scenario, including any per-add-on costs.
10. **Consolidation** — Teams combining parsing (Docotic) and rendering (HtmlToPdf add-on or something else) sometimes consolidate to one library.

### Comparison Table

| Aspect | BitMiracle Docotic.Pdf | IronPDF |
|---|---|---|
| Focus | Parse, edit, construct, extract; HTML via add-on | HTML-to-PDF, edit, merge, security |
| Pricing | Free with watermark; commercial license | Per-developer or royalty-free |
| API Style | Document object model, explicit construction | High-level renderer + document model |
| Learning Curve | Gradual for parsing; steep for programmatic layout | Gradual for HTML; moderate for parse ops |
| HTML Rendering | Via `BitMiracle.Docotic.Pdf.HtmlToPdf` add-on (Chromium) | Chromium-based, built-in |
| Page Indexing | 0-based | 0-based |
| Thread Safety | Reuse `HtmlConverter`; audit concurrent document access | Renderer reusable; see IronPDF parallel examples |
| Namespace | `BitMiracle.Docotic.Pdf` (HtmlConverter lives here too) | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Effort | Notes |
|---|---|---|
| HTML string to PDF | Low | Both support it; IronPDF is built-in, Docotic uses the HtmlToPdf add-on |
| HTML file to PDF | Low | Same |
| Merge PDFs | Low | Both support; Docotic uses `Append(path)`, IronPDF uses `PdfDocument.Merge` |
| Split/extract pages | Medium | Docotic copies pages page-by-page; IronPDF uses `CopyPages` |
| Text watermark | Medium | Docotic drawing model vs IronPDF stamper |
| Image watermark | Medium | Coordinate model differs |
| Password protection | Low | Both support owner/user passwords |
| Text extraction | Medium-High | Docotic's text model is detailed (per-word bounds); audit IronPDF parity |
| Annotation handling | High | Docotic annotations API is rich; audit IronPDF coverage |
| Form field editing | Medium | Both have form APIs; method signatures differ |
| PDF/A output | Medium | Test against your validator |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Primarily programmatic PDF construction, no HTML | Evaluate whether Docotic's model serves you; migration cost may exceed benefit |
| HTML templates requiring browser-quality CSS | IronPDF's built-in Chromium renderer removes the add-on requirement |
| Heavy text extraction and parse workflows | Audit IronPDF's extraction API against your Docotic usage before committing |
| Mixed HTML + parse requirements | IronPDF can handle both; test extraction output quality |

---

## Before You Start

**Prerequisites:**
- .NET Framework 4.6.2+, .NET Core 3.1+, or .NET 5/6/7/8/9 (IronPDF supports the full modern range)
- IronPDF license key ([license setup](https://ironpdf.com/how-to/license-keys/))
- Baseline PDF outputs captured for visual regression

**Find all Docotic.Pdf references:**

```bash
# Find all files with Docotic imports
rg "using BitMiracle" --type cs -l

# Find all PdfDocument usages (Docotic's main class)
rg "PdfDocument" --type cs

# Find text extraction calls
rg "GetText|GetWords|TextData" --type cs

# Find annotation references
rg "Annotations|PdfAnnotation" --type cs

# Find drawing / graphics operations
rg "PdfCanvas|DrawString|DrawLine|HtmlConverter" --type cs
```

**Remove Docotic, add IronPDF:**

```bash
dotnet remove package BitMiracle.Docotic.Pdf
dotnet remove package BitMiracle.Docotic.Pdf.HtmlToPdf
dotnet add package IronPdf
dotnet restore
```

PowerShell: `Uninstall-Package BitMiracle.Docotic.Pdf` (plus the HtmlToPdf add-on if installed) then `Install-Package IronPdf`.

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

**Before (Docotic.Pdf):**

```csharp
using BitMiracle.Docotic.Pdf;

// Docotic uses a license string set before any API call
PdfInfo.SetLicenseKey("YOUR-LICENSE-KEY");
```

**After (IronPDF):**

```csharp
using IronPdf;

// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 2 — Namespace Imports

**Before:**
```csharp
using BitMiracle.Docotic.Pdf;
// HtmlConverter (from the HtmlToPdf add-on) also lives in BitMiracle.Docotic.Pdf
```

**After:**
```csharp
using IronPdf;
```

### Step 3 — Basic PDF Operation

**Before (Docotic — opening a PDF and extracting text):**

```csharp
using BitMiracle.Docotic.Pdf;

PdfInfo.SetLicenseKey("YOUR-LICENSE-KEY");
using var doc = new PdfDocument("input.pdf");
string text = doc.GetText();
Console.WriteLine(text);
```

**After (IronPDF):**

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
using var pdf = PdfDocument.FromFile("input.pdf");
// https://ironpdf.com/how-to/extract-text-and-images/
string text = pdf.ExtractAllText();
Console.WriteLine(text);
```

---

## API Mapping Tables

### Namespace Mapping

| Docotic.Pdf | IronPDF | Notes |
|---|---|---|
| `BitMiracle.Docotic.Pdf` | `IronPdf` | Core namespace |
| `BitMiracle.Docotic.Pdf` (HtmlConverter from HtmlToPdf add-on) | `IronPdf` (`ChromePdfRenderer`) | HTML rendering is built into IronPDF |
| `BitMiracle.Docotic.Pdf.Layout` | `IronPdf` | Layout via HTML/CSS in IronPDF |

### Core Class Mapping

| Docotic.Pdf Class | IronPDF Class | Description |
|---|---|---|
| `PdfDocument` | `PdfDocument` | Same name; different API |
| `PdfPage` | `PdfPage` | Page object |
| `HtmlConverter` | `ChromePdfRenderer` | HTML-to-PDF entry point |
| `PdfCanvas` | No direct equivalent | IronPDF uses HTML/CSS or stamper model for drawing |
| `PdfInfo.SetLicenseKey()` | `IronPdf.License.LicenseKey` | License setup |

### Document Loading Methods

| Operation | Docotic.Pdf | IronPDF |
|---|---|---|
| Load from file | `new PdfDocument("file.pdf")` | `PdfDocument.FromFile("file.pdf")` |
| Load from stream | `PdfDocument.Load(stream)` | `PdfDocument.FromStream(stream)` |
| Create from HTML | `await converter.CreatePdfFromStringAsync(html)` (add-on) | `renderer.RenderHtmlAsPdf(html)` |
| Load from bytes | `PdfDocument.Load(byteArray)` | `PdfDocument.FromBinaryData(byteArray)` |

### Page Operations

| Operation | Docotic.Pdf | IronPDF |
|---|---|---|
| Page count | `doc.PageCount` | `pdf.PageCount` |
| Get page | `doc.Pages[n]` (0-based) | `pdf.Pages[n]` (0-based) |
| Remove page | `doc.RemovePage(n)` | `pdf.RemovePages(n)` |
| Page size | `page.Width`, `page.Height` | `pdf.Pages[n].Width`, `pdf.Pages[n].Height` |

### Merge/Split

| Operation | Docotic.Pdf | IronPDF |
|---|---|---|
| Merge PDFs | `pdf1.Append("file2.pdf")` (path / Stream / byte[]) | `PdfDocument.Merge(pdf1, pdf2)` |
| Extract pages | Copy pages page-by-page into a new `PdfDocument` | `pdf.CopyPages(start, end)` or `pdf.CopyPages(indices)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (Docotic with the HtmlToPdf add-on):**

```csharp
using System;
using System.Threading.Tasks;
using BitMiracle.Docotic.Pdf;

class Program
{
    static async Task Main()
    {
        PdfInfo.SetLicenseKey("YOUR-LICENSE-KEY");

        // HtmlConverter is async-only and downloads Chromium on first use
        using var converter = await HtmlConverter.CreateAsync();

        var options = new HtmlConversionOptions();
        options.Page.SetSize(PdfPaperSize.A4);
        options.Page.MarginTop = 20;
        options.Page.MarginBottom = 20;

        string html = @"<html>
            <body style='font-family:Arial; padding:40px'>
                <h1 style='color:#2563EB'>Invoice #2071</h1>
                <p>Amount: <strong>$1,200.00</strong></p>
                <p>Due: 2025-12-01</p>
            </body>
        </html>";

        using var pdf = await converter.CreatePdfFromStringAsync(html, options);
        pdf.Save("invoice.pdf");
        Console.WriteLine("Saved invoice.pdf");
    }
}
```

**After (IronPDF — built-in renderer, no add-on):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        // https://ironpdf.com/how-to/html-string-to-pdf/
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;

        // HTML template — can be maintained separately from code
        string html = @"<html>
            <body style='font-family:Arial; padding:40px'>
                <h1 style='color:#2563EB'>Invoice #2071</h1>
                <p>Amount: <strong>$1,200.00</strong></p>
                <p>Due: 2025-12-01</p>
            </body>
        </html>";

        using var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("invoice.pdf");
        Console.WriteLine("Saved invoice.pdf");
    }
}
```

### 2. Merge PDFs

**Before (Docotic.Pdf):**

```csharp
using System;
using BitMiracle.Docotic.Pdf;

class Program
{
    static void Main()
    {
        PdfInfo.SetLicenseKey("YOUR-LICENSE-KEY");

        // PdfDocument.Append accepts a file path, Stream, or byte[] —
        // not another PdfDocument instance.
        using var pdf1 = new PdfDocument("part1.pdf");
        pdf1.Append("part2.pdf");
        pdf1.Append("part3.pdf");
        pdf1.Save("merged.pdf");

        Console.WriteLine("Merged to merged.pdf");
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // https://ironpdf.com/how-to/merge-or-split-pdfs/
        var pdf1 = PdfDocument.FromFile("part1.pdf");
        var pdf2 = PdfDocument.FromFile("part2.pdf");
        var pdf3 = PdfDocument.FromFile("part3.pdf");

        using var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
        merged.SaveAs("merged.pdf");
        Console.WriteLine("Merged to merged.pdf");
    }
}
```

### 3. Watermark

**Before (Docotic.Pdf):**

```csharp
using System;
using BitMiracle.Docotic.Pdf;

class Program
{
    static void Main()
    {
        PdfInfo.SetLicenseKey("YOUR-LICENSE-KEY");

        using var doc = new PdfDocument("input.pdf");

        foreach (var page in doc.Pages)
        {
            var canvas = page.Canvas;

            // Save/restore graphics state around the watermark
            canvas.SaveState();
            canvas.SetTransparency(0.3); // 30% opacity

            canvas.FontSize = 48;
            canvas.FillColor = new PdfRgbColor(200, 0, 0);

            // Rotate around the page center, then draw the watermark text
            canvas.Rotate(45, page.Width / 2, page.Height / 2);
            canvas.DrawString(page.Width / 4, page.Height / 2, "CONFIDENTIAL");

            canvas.RestoreState();
        }

        doc.Save("watermarked.pdf");
        Console.WriteLine("Watermarked PDF saved");
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        // https://ironpdf.com/how-to/stamp-text-image/
        var stamper = new TextStamper
        {
            Text = "CONFIDENTIAL",
            FontSize = 48,
            Opacity = 40,
            Rotation = 45,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // Applies to all pages by default
        pdf.ApplyStamp(stamper);
        pdf.SaveAs("watermarked.pdf");
        Console.WriteLine("Watermarked PDF saved");
    }
}
```

### 4. Password Protection

**Before (Docotic.Pdf):**

```csharp
using System;
using BitMiracle.Docotic.Pdf;

class Program
{
    static void Main()
    {
        PdfInfo.SetLicenseKey("YOUR-LICENSE-KEY");

        using var doc = new PdfDocument("input.pdf");

        // Docotic exposes encryption through a single Encrypt() call
        var permissions = PdfPermissions.Printing |
                          PdfPermissions.ContentExtraction;

        doc.Encrypt(
            ownerPassword: "owner456",
            userPassword: "user123",
            permissions: permissions,
            encryptionAlgorithm: PdfEncryptionAlgorithm.Aes256
        );

        doc.Save("protected.pdf");
        Console.WriteLine("Password protected PDF saved");
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // https://ironpdf.com/how-to/pdf-permissions-passwords/
        using var pdf = PdfDocument.FromFile("input.pdf");

        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting =
            IronPdf.Security.PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserCopyPasteContent = true;

        pdf.SaveAs("protected.pdf");
        Console.WriteLine("Password protected PDF saved");
    }
}
```

---

## Critical Migration Notes

### Text Extraction Differences

Docotic.Pdf has a detailed text model — it exposes per-word bounds via `page.GetWords()`, with `chunk.Bounds.Left` and `chunk.Bounds.Top` for positional data. IronPDF's text extraction returns the text content but with less granularity on positional data. If your code depends on character- or word-level positioning (e.g., table detection, column parsing), test IronPDF's output carefully before committing.

```csharp
// Docotic — granular word data with bounds
using var doc = new PdfDocument("input.pdf");
foreach (var page in doc.Pages)
{
    foreach (var word in page.GetWords())
    {
        Console.WriteLine($"{word.Text} at ({word.Bounds.Left}, {word.Bounds.Top})");
    }
}

// IronPDF — text content
// https://ironpdf.com/how-to/extract-text-and-images/
using var pdf = PdfDocument.FromFile("input.pdf");
string allText = pdf.ExtractAllText();
for (int i = 0; i < pdf.PageCount; i++)
{
    string pageText = pdf.ExtractTextFromPage(i);
    Console.WriteLine($"--- Page {i + 1} ---\n{pageText}");
}
```

If you need positional data after migration, review the current IronPDF text extraction API at https://ironpdf.com/how-to/extract-text-and-images/.

### Annotation API Differences

Docotic.Pdf's annotation model is explicit — you create `PdfAnnotation` objects on `page.Annotations` with coordinates and properties. IronPDF has annotation support, but the API shape differs. Teams with heavy annotation workflows should test annotation round-trips (create, save, reload, verify) before migrating.

See [IronPDF annotations docs](https://ironpdf.com/how-to/annotations/) for current API.

### Page Indexing

Both Docotic.Pdf and IronPDF use 0-based page indexing. This makes the page index migration simpler than moving from 1-based libraries, but still audit all page index references — off-by-one errors are silent.

### Canvas vs Stamper

Docotic's `PdfCanvas` is a drawing context — you position everything with coordinates and explicit graphics state. IronPDF's stamper model is alignment-based with offsets. If you have precise coordinate requirements for stamps or overlays, check the stamper's `IsStampedOnAllPages`, `HorizontalOffset`, and `VerticalOffset` properties. For highly precise positioning, IronPDF's HTML-stamp approach (stamping a rendered HTML element) often gives finer control than the basic stamper API.

---

## Performance Considerations

### Native Dependencies

Docotic.Pdf core is fully managed and has no native interop requirements. Docotic's HtmlToPdf add-on downloads its own Chromium and brings the same browser-process model that IronPDF uses. IronPDF's Chromium renderer requires the standard Linux system libraries (libnss3, libatk-bridge2.0-0, libdrm2, etc.) on Linux deployments. If your existing Docotic workload is parse-only with no HtmlToPdf add-on, the Linux dependency footprint will grow after the migration.

### Renderer Reuse

If you're adding HTML-to-PDF as a new capability during this migration, reuse `ChromePdfRenderer`:

```csharp
// Register in DI container as singleton in ASP.NET Core
services.AddSingleton<ChromePdfRenderer>();

// Or reuse at class level
private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();
```

### Memory Patterns

Docotic.Pdf documents are `IDisposable`. IronPDF `PdfDocument` is also `IDisposable`. The migration shouldn't change disposal discipline if your existing code uses `using` consistently. Audit for any `PdfDocument` instances created without `using` or explicit `.Dispose()` calls.

### Parallel Workloads

If you generate PDFs in parallel, review IronPDF's concurrent rendering behavior at [parallel examples](https://ironpdf.com/examples/parallel/). The Chromium renderer process is separate from the managed layer — concurrent requests share the renderer process differently than Docotic's in-process drawing API.

---

## Migration Checklist

### Pre-Migration
- [ ] Run `rg "using BitMiracle" --type cs -l` to find all affected files
- [ ] Capture baseline PDF outputs for visual regression comparison
- [ ] Identify all `PdfCanvas` drawing operations (will need refactoring to HTML or stamper)
- [ ] Note whether the HtmlToPdf add-on (`HtmlConverter`) is in use — that code maps to `ChromePdfRenderer`
- [ ] Inventory annotation-heavy code — audit IronPDF annotation API parity
- [ ] Check form field editing usage — audit IronPDF form API
- [ ] Note any character-level or per-word text extraction dependencies (`page.GetWords()`)
- [ ] Confirm target .NET version supports IronPDF (4.6.2+, Core 3.1+, .NET 5/6/7/8/9)
- [ ] Test IronPDF output in parallel with Docotic before removing Docotic

### Code Migration
- [ ] Replace `BitMiracle.Docotic.Pdf` (and `.HtmlToPdf` add-on) NuGet with `IronPdf`
- [ ] Replace `PdfInfo.SetLicenseKey()` with `IronPdf.License.LicenseKey = "..."`
- [ ] Replace `using BitMiracle.Docotic.Pdf` with `using IronPdf`
- [ ] Replace `new PdfDocument("file.pdf")` with `PdfDocument.FromFile("file.pdf")`
- [ ] Replace `HtmlConverter.CreateAsync()` + `CreatePdfFromStringAsync` with `new ChromePdfRenderer().RenderHtmlAsPdf(...)`
- [ ] Replace programmatic layout with HTML templates where feasible
- [ ] Replace `PdfCanvas` drawing with `TextStamper`, `ImageStamper`, or HTML
- [ ] Replace Docotic merge (`pdf1.Append("file.pdf")`) with `PdfDocument.Merge(pdf1, pdf2, ...)`
- [ ] Replace `doc.Encrypt(...)` with `pdf.SecuritySettings` properties
- [ ] Replace `doc.GetText()` / `page.GetText()` with `pdf.ExtractAllText()` / `pdf.ExtractTextFromPage(i)`
- [ ] Update annotation handling to IronPDF API — audit coverage

### Testing
- [ ] Visual comparison: programmatic layouts vs HTML-rendered equivalents
- [ ] Text extraction output — compare per-word data if positional data is needed
- [ ] Annotation round-trip: create, save, reload, verify
- [ ] Password protection: open with user password, confirm owner restrictions
- [ ] Merge page count and order verification
- [ ] Test on Linux if target environment — verify Chromium system library requirements
- [ ] Load test concurrent rendering if parallel workloads exist

### Post-Migration
- [ ] Remove `BitMiracle.Docotic.Pdf` and `BitMiracle.Docotic.Pdf.HtmlToPdf` from all project files
- [ ] Update Docker images for Linux deployment with IronPDF Chromium dependencies
- [ ] Update internal documentation on PDF generation approach
- [ ] Archive Docotic license for audit purposes

---

## One Last Thing

The Docotic.Pdf to IronPDF migration is generally straightforward for the common operations — loading, merging, password protection. The complexity concentrates in two areas that deserve careful evaluation before starting: annotation workflows and text extraction. If either is central to your use case, run a spike against the IronPDF API before committing to the migration path.

The real win for most teams isn't the API swap — it's collapsing the parse library plus the HtmlToPdf add-on plus the licensing for both into a single package that renders HTML like a browser. That removes the programmatic layout code no one wants to maintain, and it removes the add-on coordination that nobody enjoys auditing either.

**Question for the comments:** For teams that moved from a parse-focused PDF library (Docotic, iText, etc.) to a renderer-first library — how did you handle the text extraction use cases that didn't port cleanly? Did you keep the parse library alongside the renderer, or find a different approach?
