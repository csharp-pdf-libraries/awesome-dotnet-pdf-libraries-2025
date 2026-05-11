---
title: "Migrating from GemBox.Pdf to IronPDF: a practical .NET migration guide"
published: false
tags: dotnet, csharp, pdf, migration
---

Before committing to a migration, teams want specifics — not marketing copy. This article is honest about where GemBox.Pdf and IronPDF differ, where comparisons are apples-to-oranges, and what the technical gaps actually look like in production code.

The short version: GemBox.Pdf is a capable, managed-code PDF component with a clean low-level API, but it is a content-stream library — you draw at coordinates, and there is no HTML rendering. IronPDF is primarily a [Chromium-based HTML-to-PDF renderer](https://ironpdf.com/how-to/html-string-to-pdf/) that also does document manipulation. If your workload is HTML-driven reports, the two products are not actually substitutes for one another, and the migration is a paradigm shift rather than a one-for-one API swap.

Let's look at the specifics.

---

## Why Migrate (Without Drama)

Migration triggers tend to be practical, not philosophical:

1. **No HTML-to-PDF in GemBox.Pdf** — `PdfDocument.Load` opens existing PDFs, not HTML. To render HTML with GemBox you have to switch to the separate **GemBox.Document** SKU, which is a different product with its own license. ([Confirmed by GemBox staff on the GemBox forum](https://forum.gemboxsoftware.com/t/loading-pdf-document-from-html/966).)
2. **Two-page ceiling in free mode** — GemBox.Pdf's free tier throws `FreeLimitReachedException` on the third page. Anything beyond a one-page receipt or two-page invoice needs a paid license. ([GemBox free-version docs.](https://www.gemboxsoftware.com/pdf/free-version))
3. **Coordinate-based layout** — every text element is drawn at `(x, y)` in PDF user-space units via `page.Content.DrawText(formattedText, new PdfPoint(x, y))`. There is no flow layout, no automatic page breaks, no CSS.
4. **Multiple SKUs for Office input** — Word-to-PDF needs GemBox.Document, Excel-to-PDF needs GemBox.Spreadsheet, email parsing needs GemBox.Email. The Bundle covers all of them but costs more.
5. **CSS3 / flexbox / grid templates** — modern HTML report templates assume a browser engine. Chromium-based rendering handles them natively; coordinate APIs do not.
6. **JavaScript support** — Chromium-based rendering executes JS in HTML for charting and dynamic content. GemBox.Pdf has no HTML engine to execute it.
7. **Deployment-size trade-off** — pure managed code (GemBox) keeps deployment artifacts small. IronPDF ships Chromium binaries, which is a meaningful container-size delta and worth measuring before committing.
8. **Design changes require code changes** — tweaking spacing in a coordinate-based layout means recalculating Y positions. Editing HTML/CSS is materially faster for iterative report design.
9. **Long-term rendering investment** — Chromium-based engines inherit browser improvements; custom renderers have to track CSS specs themselves.

### Comparison Table

| Aspect | GemBox.Pdf | IronPDF |
|---|---|---|
| HTML-to-PDF | Not supported (separate GemBox.Document SKU) | Full Chromium engine |
| Free Tier | 2-page max (`FreeLimitReachedException`) | Watermark only, no page cap |
| Layout Model | Coordinate-based content streams | HTML/CSS flow layout |
| API Style | Object model; `PdfDocument` + `PdfPage` + `PdfContent` | Renderer-centric + `PdfDocument` |
| Page Indexing | 0-based | 0-based |
| Modern CSS (flexbox, grid) | Not applicable (no HTML engine) | Full browser support |
| JavaScript execution | Not applicable | Supported (V8) |
| Word/Excel input | Separate SKUs (GemBox.Document / Spreadsheet) | Render via HTML pipeline |
| Deployment Size | Smaller (pure managed code) | Larger (Chromium included) |
| Namespace | `GemBox.Pdf` | `IronPdf` |

---

## Rendering Approach: Coordinates vs. HTML

The biggest paradigm shift in this migration is not the API names — it is the layout model. GemBox.Pdf is a content-stream library, IronPDF is a renderer. They solve different problems and the transition rewards thinking about your documents differently.

### What Differs in Practice

| Characteristic | GemBox.Pdf (Content Streams) | IronPDF (Chromium) |
|---|---|---|
| Text positioning | `DrawText(text, new PdfPoint(x, y))` | HTML flow + CSS positioning |
| Tables | Manual cell-by-cell coordinate math | `<table>` with CSS |
| Page breaks | You decide, manually | Automatic from content height |
| Web fonts (@font-face) | Not applicable | Full support |
| SVG | Not applicable in HTML form | Full support |
| Deployment size | Smaller (no Chromium binaries) | Larger (Chromium included) |
| Cold start | Faster (no browser init) | Slower (Chromium init) |

### Benchmarking Approach

If rendering throughput matters in your environment, run benchmarks against your own templates. A reasonable harness for IronPDF:

```csharp
using System.Diagnostics;
using IronPdf;

// IronPDF throughput test - renderer reuse pattern
// See: https://ironpdf.com/examples/parallel/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

string html = File.ReadAllText("template.html"); // your actual template
int iterations = 100;

var renderer = new ChromePdfRenderer();

// Warm up (important for Chromium-based renderers)
using var warmup = renderer.RenderHtmlAsPdf(html);

var sw = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    // In real workload: save or process pdf here
}
sw.Stop();

double msPerDoc = sw.Elapsed.TotalMilliseconds / iterations;
Console.WriteLine($"IronPDF: {msPerDoc:F1} ms/doc over {iterations} iterations");
Console.WriteLine($"Total: {sw.Elapsed.TotalSeconds:F2}s for {iterations} documents");
```

A direct head-to-head against GemBox.Pdf is not apples-to-apples for HTML workloads — GemBox.Pdf can't load HTML at all, so the comparable test on the GemBox side would be a content-stream document built programmatically. If your real workload is HTML rendering, the meaningful benchmark is "IronPDF vs. GemBox.Document," not GemBox.Pdf.

**Methodology notes for your own benchmarks:**
- Use representative HTML/CSS from your actual templates, not synthetic markup.
- Include warm-up passes (especially for Chromium-based renderers).
- Test at realistic concurrency levels, not just sequential loops.
- Measure memory consumption under sustained load, not just wall time.
- Run on the same hardware/VM spec you use in production.

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Effort | Notes |
|---|---|---|
| Load existing PDF | Low | `PdfDocument.Load(path)` → `PdfDocument.FromFile(path)` |
| Save PDF | Low | `document.Save(path)` → `pdf.SaveAs(path)` |
| PDF merge | Low | `Pages.AddClone(source.Pages)` → `PdfDocument.Merge(...)` |
| PDF split | Low | Loop + `AddClone` → `pdf.CopyPage(i)` |
| Text extraction | Low | `page.Content.GetText()` → `pdf.ExtractAllText()` |
| HTML-to-PDF | High | New capability — GemBox.Pdf has none |
| Adding text/tables | Medium | Coordinate code → HTML/CSS |
| Watermarking | Low | Manual `DrawText` → `ApplyStamp` / `ApplyWatermark` |
| Password protection | Medium | `SetPasswordEncryption()` → `SecuritySettings` |
| Form fields | Medium | `document.Form.Fields` → `pdf.Form.Fields` (similar shape) |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| HTML templates with modern CSS | IronPDF — GemBox.Pdf has no HTML engine |
| Minimal deployment footprint, programmatic PDFs | GemBox.Pdf advantage (no Chromium); evaluate the trade-off |
| High throughput PDF manipulation without rendering | Benchmark both; GemBox.Pdf may win on throughput-per-MB |
| Complex web-style reports | IronPDF Chromium rendering |
| Existing GemBox.Pdf codebase hitting `FreeLimitReachedException` | IronPDF removes the per-document page cap |

---

## Before You Start

### Prerequisites

- .NET Framework 4.6.2+ or .NET Core 2.0+ / .NET 5+ for IronPDF
- NuGet access
- An IronPDF [license key](https://ironpdf.com/how-to/license-keys/)

### Find GemBox.Pdf References

```bash
# Find all GemBox using statements
rg "using GemBox" --type cs

# Find all GemBox type references
rg "GemBox\." --type cs -l

# Check project files
rg "GemBox" **/*.csproj
```

### Swap the NuGet Package

```bash
# Remove GemBox.Pdf
dotnet remove package GemBox.Pdf

# Add IronPDF
dotnet add package IronPdf

dotnet restore
```

---

## Quick Start Migration (3 Steps)

### Step 1: License Configuration

**Before (GemBox.Pdf)**
```csharp
using GemBox.Pdf;

// GemBox license initialization - set before first use.
// Free tier uses the literal "FREE-LIMITED-KEY"; the commercial key replaces it.
ComponentInfo.SetLicense("FREE-LIMITED-KEY");
```

**After (IronPDF)**
```csharp
using IronPdf;

// See: https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 2: Namespace Imports

**Before**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content; // PdfFormattedText, PdfPoint, drawing primitives
using GemBox.Pdf.Forms;   // form field types (if you use forms)
using GemBox.Pdf.Security; // encryption types (if you use security)
```

**After**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
```

### Step 3: Basic Content Creation

There is no like-for-like "HTML-to-PDF" mapping on the GemBox.Pdf side, so the closest "before" is the canonical hello-world content stream.

**Before (GemBox.Pdf — coordinate-based)**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content;

ComponentInfo.SetLicense("FREE-LIMITED-KEY");

using (var document = new PdfDocument())
{
    var page = document.Pages.Add();

    var formattedText = new PdfFormattedText();
    formattedText.Append("Hello World");

    page.Content.DrawText(formattedText, new PdfPoint(100, 700));
    document.Save("output.pdf");
}
```

**After (IronPDF — HTML)**
```csharp
using IronPdf;

// See: https://ironpdf.com/how-to/html-string-to-pdf/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

---

## API Mapping Tables

### Namespace Mapping

| GemBox Namespace | IronPDF Namespace | Purpose |
|---|---|---|
| `GemBox.Pdf` | `IronPdf` | Core document types |
| `GemBox.Pdf.Content` | N/A (content authored as HTML) | Coordinate drawing primitives |
| `GemBox.Pdf.Security` | `IronPdf` (`SecuritySettings`) | Encryption / permissions |
| `GemBox.Pdf.Forms` | `IronPdf` | AcroForm fields |

### Core Class Mapping

| GemBox Class | IronPDF Equivalent | Notes |
|---|---|---|
| `PdfDocument` | `PdfDocument` | Main document object |
| `PdfPage` | `PdfDocument.Pages[n]` | Page access (0-based both) |
| `PdfFormattedText` | HTML string | Text styled with CSS |
| `PdfPoint` | CSS positioning | `position:absolute; left:Xpt; top:Ypt;` if you need pixel placement |
| `PdfContent` | `ChromePdfRenderer` | Page rendering happens at HTML render time |
| `ComponentInfo.SetLicense()` | `IronPdf.License.LicenseKey` | License entry point |

### Document Loading and Saving

| Operation | GemBox.Pdf | IronPDF |
|---|---|---|
| Load from file | `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` |
| Load from stream | `PdfDocument.Load(stream)` | `PdfDocument.FromStream(stream)` |
| Save to file | `document.Save(path)` | `pdf.SaveAs(path)` |
| Save to stream/bytes | `document.Save(stream)` | `pdf.Stream` / `pdf.BinaryData` |
| Render from HTML | Not supported (use GemBox.Document SKU) | `renderer.RenderHtmlAsPdf(html)` |

### Page Operations

| Operation | GemBox.Pdf | IronPDF |
|---|---|---|
| Page count | `document.Pages.Count` | `pdf.PageCount` |
| Get page | `document.Pages[index]` | `pdf.Pages[index]` |
| Page width / height | `page.Size.Width` / `page.Size.Height` | `pdf.Pages[i].Width` / `Height` |
| Add page | `document.Pages.Add()` | Render new HTML or merge |
| Remove page | `document.Pages.Remove(page)` | `pdf.Pages.RemoveAt(index)` |

### Merge / Split Operations

| Operation | GemBox.Pdf | IronPDF |
|---|---|---|
| Merge two docs | `document.Pages.AddClone(source.Pages)` | `PdfDocument.Merge(doc1, doc2)` |
| Merge many docs | Multiple `AddClone` calls | `PdfDocument.Merge(listOfPdfs)` |
| Copy single page | `document.Pages.AddClone(otherPage)` | `pdf.CopyPage(index)` |
| Copy page range | Loop with `AddClone` | `pdf.CopyPages(indices)` |

### Security / Encryption

| Operation | GemBox.Pdf | IronPDF |
|---|---|---|
| Enable encryption | `document.SaveOptions.SetPasswordEncryption()` | Set `pdf.SecuritySettings.*` |
| User password | `encryption.DocumentOpenPassword` | `pdf.SecuritySettings.UserPassword` |
| Owner password | `encryption.PermissionsPassword` | `pdf.SecuritySettings.OwnerPassword` |
| Permissions | `encryption.Permissions` (flags) | `AllowUserPrinting`, `AllowUserCopyPasteContent`, etc. |
| Encryption level | `PdfEncryptionLevel.AES_256` | Default AES |

---

## Four Complete Before/After Migrations

### 1. HTML "Rendering"

Strictly speaking, GemBox.Pdf has no HTML pipeline. The honest "before" picture is either a `DocumentModel.Load("input.html")` call against the **GemBox.Document** SKU, or a coordinate-based reconstruction of the report. We'll show the former so the migration is concrete.

**Before (GemBox.Document — note: separate product)**
```csharp
// NuGet: Install-Package GemBox.Document
// HTML-to-PDF on the GemBox stack lives in the Document SKU,
// not GemBox.Pdf. Different package, different license.
using GemBox.Document;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        var document = DocumentModel.Load("input.html");
        document.Save("output.pdf");
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;

// See: https://ironpdf.com/how-to/html-string-to-pdf/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize             = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.PrintHtmlBackgrounds  = true;

string html = @"
    <html>
    <head><style>body { font-family: Arial; }</style></head>
    <body><h1>Report Q4</h1><p>Summary content.</p></body>
    </html>";

using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
Console.WriteLine($"Rendered {pdf.PageCount} page(s)");
```

---

### 2. Merge PDFs

**Before (GemBox.Pdf)**
```csharp
using GemBox.Pdf;

class MergeExample
{
    static void Main()
    {
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        using (var document = new PdfDocument())
        {
            var source1 = PdfDocument.Load("file1.pdf");
            var source2 = PdfDocument.Load("file2.pdf");

            document.Pages.AddClone(source1.Pages);
            document.Pages.AddClone(source2.Pages);

            document.Save("merged.pdf");
        }
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;

// See: https://ironpdf.com/how-to/merge-or-split-pdfs/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

using var doc1 = PdfDocument.FromFile("file1.pdf");
using var doc2 = PdfDocument.FromFile("file2.pdf");

using var merged = PdfDocument.Merge(doc1, doc2);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (GemBox.Pdf — manual positioning)**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content;

class WatermarkExample
{
    static void Main()
    {
        ComponentInfo.SetLicense("YOUR-LICENSE-KEY");

        using (var document = PdfDocument.Load("input.pdf"))
        {
            foreach (var page in document.Pages)
            {
                var text = new PdfFormattedText();
                text.Append("CONFIDENTIAL");

                // Center positioning must be computed manually
                var pageSize = page.Size;
                double x = pageSize.Width / 2 - 100;
                double y = pageSize.Height / 2;

                page.Content.DrawText(text, new PdfPoint(x, y));
            }

            document.Save("watermarked.pdf");
        }
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;
using IronPdf.Editing;

// See: https://ironpdf.com/how-to/custom-watermark/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

using var pdf = PdfDocument.FromFile("input.pdf");

var stamp = new TextStamper
{
    Text                = "CONFIDENTIAL",
    FontSize            = 40,
    Opacity             = 30,         // 0-100 integer scale
    Rotation            = -45,
    VerticalAlignment   = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};

pdf.ApplyStamp(stamp);
pdf.SaveAs("watermarked.pdf");
```

---

### 4. Password Protection

**Before (GemBox.Pdf)**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Security;

class SecurityExample
{
    static void Main()
    {
        ComponentInfo.SetLicense("YOUR-LICENSE-KEY");

        using (var document = PdfDocument.Load("input.pdf"))
        {
            var encryption = document.SaveOptions.SetPasswordEncryption();
            encryption.DocumentOpenPassword = "user123";
            encryption.PermissionsPassword  = "owner456";
            encryption.Permissions          = PdfUserAccessPermissions.Print;
            encryption.EncryptionLevel      = PdfEncryptionLevel.AES_256;

            document.Save("protected.pdf");
        }
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;

// See: https://ironpdf.com/how-to/pdf-permissions-passwords/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

using var pdf = PdfDocument.FromFile("input.pdf");

pdf.SecuritySettings.UserPassword              = "user123";
pdf.SecuritySettings.OwnerPassword             = "owner456";
pdf.SecuritySettings.AllowUserPrinting         = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserAnnotations      = false;

pdf.SaveAs("protected.pdf");
```

---

## Critical Migration Notes

### Page Indexing

Both GemBox.Pdf and IronPDF use **0-based** page indexing — one of the rare points of direct compatibility.

```csharp
// Both: 0 = first page
var first = pdf.Pages[0];
var last  = pdf.Pages[pdf.PageCount - 1];
```

### `FreeLimitReachedException` Goes Away

GemBox.Pdf's free tier throws `FreeLimitReachedException` once a document exceeds two pages. IronPDF's free mode adds a watermark instead, with no per-document page cap. If your codebase had try/catch blocks scoped around `FreeLimitReachedException`, you can retire them after migration.

### Error Handling

Both libraries throw exceptions for errors rather than returning status codes, so the pattern carries over:

```csharp
try
{
    using var pdf = PdfDocument.FromFile("input.pdf");
    // operations
}
catch (IronPdf.Exceptions.PdfException ex)
{
    Console.Error.WriteLine($"PDF error: {ex.Message}");
}
catch (System.IO.IOException ex)
{
    Console.Error.WriteLine($"IO error: {ex.Message}");
}
```

### Deployment Size Delta

GemBox.Pdf's pure managed code keeps deployment artifacts small. IronPDF bundles Chromium binaries, which is a meaningful container-size delta. Measure both before committing in either direction:

```bash
# After migrating, check container image size delta
docker build -t myapp:gembox  . && docker image inspect myapp:gembox  --format '{{.Size}}'
docker build -t myapp:ironpdf . && docker image inspect myapp:ironpdf --format '{{.Size}}'
```

---

## Performance Considerations

### Chromium Cold Start

The first `ChromePdfRenderer` instantiation in a process triggers Chromium initialization. For serverless or short-lived processes, this is measurable overhead. Profile in your deployment environment.

```csharp
// For Lambda / Function-as-a-Service: initialize renderer at cold start
// (static field in execution context), reuse across invocations.
private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();
```

### Parallel Rendering

See [IronPDF parallel rendering examples](https://ironpdf.com/examples/parallel/) for validated patterns. Rule of thumb: one `ChromePdfRenderer` per concurrent task.

```csharp
// Parallel batch - one renderer per task
var tasks = htmlDocuments.Select(html => Task.Run(() =>
{
    var renderer = new ChromePdfRenderer();
    using var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}));

byte[][] results = await Task.WhenAll(tasks);
```

### When GemBox.Pdf May Still Be Faster

For pure document manipulation (merge, split, text extraction, form filling) without rendering, a non-Chromium library has lower overhead. If your workload is 90% manipulation and 10% rendering, benchmark both before deciding.

---

## Migration Checklist

### Pre-Migration
- [ ] List all GemBox.Pdf features currently in use.
- [ ] Identify coordinate-based layouts that need HTML conversion.
- [ ] Evaluate whether the 2-page free-mode ceiling affects you today.
- [ ] Measure current render times and memory usage as baseline.
- [ ] Evaluate Chromium deployment-size impact for your environment.
- [ ] Obtain an IronPDF license key and confirm activation.
- [ ] Review [https://ironpdf.com/how-to/license-keys/](https://ironpdf.com/how-to/license-keys/).
- [ ] Create a migration branch.
- [ ] Set up visual diff tooling for PDF output comparison.

### Code Migration
- [ ] Remove `GemBox.Pdf` (and `GemBox.Document` if you used it for HTML).
- [ ] Add `IronPdf`.
- [ ] Replace `using GemBox.Pdf` with `using IronPdf`.
- [ ] Replace `ComponentInfo.SetLicense(...)` with `IronPdf.License.LicenseKey = ...`.
- [ ] Replace `PdfDocument.Load(path)` with `PdfDocument.FromFile(path)`.
- [ ] Replace `document.Save(...)` with `pdf.SaveAs(...)`.
- [ ] Replace coordinate-based `PdfFormattedText` + `DrawText` with HTML rendering.
- [ ] Replace `Pages.AddClone(...)` chains with `PdfDocument.Merge(...)`.
- [ ] Replace `SetPasswordEncryption()` with `SecuritySettings` properties.
- [ ] Replace `page.Size` reads with `pdf.Pages[i].Width` / `Height`.
- [ ] Add `using` blocks for all `PdfDocument` instances.

### Testing
- [ ] Visual comparison on 10+ representative templates.
- [ ] Verify page counts, layout, fonts, and colors match expectations.
- [ ] Benchmark render throughput against baseline.
- [ ] Test merge with 3+ documents.
- [ ] Test password-protection round-trip.
- [ ] Test watermark visibility and positioning.
- [ ] Measure memory under sustained load.
- [ ] Confirm any code paths that caught `FreeLimitReachedException` are removed.

### Post-Migration
- [ ] Remove GemBox license keys from config.
- [ ] Update Docker base images / deployment scripts for Chromium deps.
- [ ] Pin IronPDF version in project files.
- [ ] Document rendering output comparison results for QA sign-off.

---

## Wrapping Up

GemBox.Pdf is a thoughtfully designed content-stream library with a clean low-level API and a real pure-managed-code advantage. IronPDF's Chromium-based renderer is a fundamentally different architectural choice — HTML/CSS layout, modern web platform features, larger deployment footprint, slower cold start. Neither is categorically better; the right choice depends on whether your real workload is HTML rendering, coordinate-based programmatic PDF, or a mix.

A question worth raising before you commit: **have you actually run your production HTML templates through both engines and compared the output pixel-by-pixel?** Synthetic benchmarks and toy examples often miss the rendering differences that matter for real report templates with custom fonts, flexbox layouts, or SVG charts.

Drop specifics in the comments — particularly if you've found templates that render significantly differently between the two libraries.
