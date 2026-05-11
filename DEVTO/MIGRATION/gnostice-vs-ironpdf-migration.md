---
title: "Migrating from Gnostice PDFOne to IronPDF: a working migration in an afternoon"
published: false
tags: dotnet, csharp, pdf, migration
---

Some migrations are smooth. Others surface subtle issues only after the first full test run. This guide focuses on the second kind — the edge cases, the "why does this work differently" moments, and the places where a Gnostice-to-IronPDF migration is more likely to hit friction.

If you're migrating from [Gnostice PDFOne](https://www.gnostice.com) to [IronPDF](https://ironpdf.com/), the baseline operations (HTML to PDF, merge, watermark, passwords) are all achievable. The troubleshooting comes from differences in API model, page indexing, error surfacing, and rendering approach.

> **Context worth knowing:** The legacy `PDFOne.NET` NuGet package is marked deprecated (last release 24.1.60, July 1, 2024). Gnostice now directs new work to the `Gnostice.DocumentStudio.*` package line. If you're still on PDFOne.NET, a migration of some kind is already on your roadmap.

---

## Why Migrate (Without Drama)

The most common triggers for a Gnostice migration are documented limitations rather than vendor critique:

1. **PDFOne.NET deprecation** — the legacy package's last release was 24.1.60 on July 1, 2024, with new work directed to `Gnostice.DocumentStudio.*`.
2. **No external CSS support** — Gnostice's documentation explicitly states external CSS stylesheets are not supported, which limits modern HTML-to-PDF workflows.
3. **No JavaScript execution** — dynamic content that depends on JS cannot be rendered.
4. **No right-to-left language support** — Arabic, Hebrew, and other RTL scripts are documented as unsupported.
5. **Platform fragmentation** — separate WinForms, WPF, ASP.NET, and Xamarin products with different feature sets often mean multiple licenses and parallel codebases.
6. **Memory stability issues** — user forums and Stack Overflow have reported memory leaks, JPEG Error #53, and stack-overflow exceptions during image processing.
7. **Coordinate-based drawing API** — many operations require manual X/Y positioning rather than CSS layout.
8. **Limited modern CSS** — Flexbox and Grid layouts that web teams take for granted are not rendered.

### Comparison Table

| Aspect | Gnostice PDFOne .NET | IronPDF |
|---|---|---|
| Focus | PDF creation, editing, forms, text extraction | HTML-to-PDF rendering + document manipulation |
| Pricing | Commercial license | Commercial license |
| API Style | Object-oriented, coordinate-based | Fluent, HTML-renderer-centric |
| HTML Rendering | Limited; no external CSS or JS | Full Chromium engine |
| Modern CSS (Grid/Flexbox) | Not supported | Supported |
| RTL Languages | Not supported | Full Unicode support |
| Page Indexing | 1-based (typical PDFOne usage) | 0-based |
| Namespace | `Gnostice.PDFOne` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Effort | Notes |
|---|---|---|
| HTML to PDF | Low (IronPDF) | New capability; Gnostice has no external CSS or JS |
| Load existing PDF | Low | Both support file/stream loading |
| PDF merge | Low | `Append` becomes `PdfDocument.Merge` |
| PDF split | Low | API shape differs |
| Text extraction | Low | `GetPageText` becomes `ExtractTextFromPage` |
| Form filling | Medium | Property-access model differs |
| Watermarking | Low | HTML-styled watermark, no manual coordinates |
| Password protection | Low | `SecuritySettings` properties |
| Digital signatures | Low | First-class in IronPDF; limited/late addition in Gnostice |
| Viewer controls | High | IronPDF generates; viewers are out of scope (use PDF.js etc.) |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| HTML-to-PDF primary workload | IronPDF migration is well-scoped |
| Complex form filling workflows | Validate IronPDF form-field coverage against your forms |
| Annotation-heavy documents | Evaluate IronPDF annotation support before committing |
| PDFOne.NET deprecation is the driver | Migration is unavoidable — IronPDF avoids the second Gnostice-to-Gnostice jump |

---

## Before You Start

### Prerequisites

- .NET Framework 4.6.2+ or .NET Core 2.0+ / .NET 5+ project
- IronPDF license key ([ironpdf.com/how-to/license-keys/](https://ironpdf.com/how-to/license-keys/))

### Find Gnostice References

```bash
# Find Gnostice namespace imports
rg "using Gnostice" --type cs

# Find PDFOne type references
rg "Gnostice\.|PDFOne|PDFDocument|DocExporter" --type cs -l

# Find NuGet references
rg -i "gnostice|pdfone" **/*.csproj
```

### Remove Gnostice, Add IronPDF

```bash
# Remove Gnostice packages (real package IDs on nuget.org)
dotnet remove package PDFOne.NET                          # legacy, deprecated
dotnet remove package Gnostice.DocumentStudio.WinForms
dotnet remove package Gnostice.DocumentStudio.WPF
dotnet remove package Gnostice.DocumentStudio.ASP
dotnet remove package Gnostice.DocumentStudio.ASP.Core
dotnet remove package Gnostice.DocumentStudio.Xamarin

# Add IronPDF
dotnet add package IronPdf

dotnet restore
```

---

## Quick Start Migration (3 Steps)

### Step 1: License Configuration

**Before (Gnostice PDFOne)**
```csharp
using Gnostice.PDFOne;

// PDFOne license is set via the static PDFOne class
Gnostice.PDFOne.PDFOne.LicenseKey = "YOUR-GNOSTICE-LICENSE";
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
using Gnostice.PDFOne;
using Gnostice.PDFOne.Graphics;
using Gnostice.PDFOne.Document;
```

**After**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
using IronPdf.Security;
```

### Step 3: Basic Conversion

**Before (Gnostice PDFOne)**
```csharp
using Gnostice.PDFOne;
using Gnostice.PDFOne.Graphics;

class Program
{
    static void Main()
    {
        PDFDocument doc = new PDFDocument();
        doc.Open();

        PDFPage page = doc.Pages.Add();

        // PDFOne has no direct HTML-to-PDF renderer.
        // Content is built up with element objects and coordinates.
        PDFTextElement textElement = new PDFTextElement();
        textElement.Text = "Simple text content";
        textElement.Draw(page, 10, 10);

        doc.Save("output.pdf");
        doc.Close();
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;

// See: https://ironpdf.com/how-to/html-string-to-pdf/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1><p>Migrated from Gnostice.</p>");
pdf.SaveAs("output.pdf");
```

---

## API Mapping Tables

### Namespace Mapping

| Gnostice Namespace | IronPDF Namespace | Purpose |
|---|---|---|
| `Gnostice.PDFOne` | `IronPdf` | Core types |
| `Gnostice.PDFOne.Document` | `IronPdf` | Document operations |
| `Gnostice.PDFOne.Graphics` | `IronPdf.Editing` | Drawing, stamps, watermarks |
| `Gnostice.Documents.PDF` | `IronPdf` | PDF export |
| `Gnostice.Documents.Controls` | N/A | Viewer controls — use a separate viewer |

### Core Class Mapping

| Gnostice Class | IronPDF Class | Description |
|---|---|---|
| `PDFDocument` | `PdfDocument` | Main document object |
| `PDFPage` | `PdfDocument.Pages[i]` | Page representation |
| `PDFFont` | CSS `font-family` / `font-size` | Font specification |
| `PDFTextElement` | HTML content | Text content |
| `DocExporter` | `ChromePdfRenderer` | HTML/URL-to-PDF |
| `DocumentManager` | `PdfDocument` static methods | Document loading |

### Document Loading Methods

| Operation | Gnostice | IronPDF |
|---|---|---|
| Load from file | `doc.Load("file.pdf")` | `PdfDocument.FromFile(path)` |
| Load with password | `doc.Load(path, password)` | `PdfDocument.FromFile(path, password)` |
| Save to file | `doc.Save(path)` | `pdf.SaveAs(path)` |
| Save to stream | `doc.SaveToStream(stream)` | `pdf.Stream` or `pdf.BinaryData` |
| Close/dispose | `doc.Close()` | `pdf.Dispose()` (or `using`) |
| Render from HTML | (not directly supported) | `renderer.RenderHtmlAsPdf(html)` |

### Page Operations

| Operation | Gnostice | IronPDF |
|---|---|---|
| Page count | `doc.Pages.Count` | `pdf.PageCount` |
| Get page | `doc.Pages[index]` (1-based usage) | `pdf.Pages[index]` (0-based) |
| Insert page | `doc.Pages.Insert(index)` | `pdf.Pages.Insert(index, page)` |
| Remove page | `doc.Pages.RemoveAt(index)` | `pdf.Pages.RemoveAt(index)` |
| Page rotation | `page.Rotate` | `page.Rotation` |

### Merge/Split Operations

| Operation | Gnostice | IronPDF |
|---|---|---|
| Merge documents | `doc1.Append(doc2)` | `PdfDocument.Merge(pdf1, pdf2)` |
| Delete pages | `doc.DeletePages(start, count)` | `pdf.RemovePages(indices)` |
| Extract pages | `doc.ExtractPages(start, count)` | `pdf.CopyPages(indices)` |

### Encryption and Security

| Operation | Gnostice | IronPDF |
|---|---|---|
| Encrypt | `doc.SetEncryption(...)` | `pdf.SecuritySettings` |
| User password | `doc.SetUserPassword(pwd)` | `pdf.SecuritySettings.UserPassword` |
| Owner password | `doc.SetOwnerPassword(pwd)` | `pdf.SecuritySettings.OwnerPassword` |
| Permissions | `PDFPermissions` enum flags | `AllowUser*` properties |

---

## Troubleshooting: Common Migration Issues

This section documents patterns that frequently surface during Gnostice-to-IronPDF migrations. Use it as a debugging checklist when something doesn't behave as expected.

---

### Issue 1: Rendering Differences on HTML Templates

**Symptom:** HTML-to-PDF output looks different after migration — fonts, spacing, or layout don't match.

**Why it happens:** Gnostice does not render external CSS or execute JavaScript. If your HTML relied on either, content that was missing or broken before will now render in IronPDF. Pages that were "designed around" Gnostice's limitations may now overflow or lay out differently.

**Diagnosis:**
```bash
# Compare outputs visually using ImageMagick (if available)
convert -density 150 before.pdf before-%03d.png
convert -density 150 after.pdf after-%03d.png
# diff pages visually
```

**Fix options:**
```csharp
// IronPDF rendering options for CSS fidelity
// See: https://ironpdf.com/how-to/rendering-options/
var renderer = new ChromePdfRenderer();

// External CSS now works — link or @import resolves
// External fonts also resolve; ensure HTTPS URLs

renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;

// Wait for JS / web fonts to settle if external resources are slow
renderer.RenderingOptions.WaitFor.RenderDelay(200);
```

---

### Issue 2: Page Index Off-By-One Errors

**Symptom:** Wrong pages are extracted, or a page operation targets the wrong page.

**Why it happens:** Gnostice PDFOne code commonly iterates with `for (int i = 1; i <= doc.Pages.Count; i++)` and passes those values to APIs like `GetPageText(i)`. IronPDF uses 0-based indexing throughout. A direct port of page-number values introduces off-by-one errors.

**Diagnosis:**
```csharp
// Before (Gnostice — typical 1-based loop):
// for (int i = 1; i <= doc.Pages.Count; i++)
// {
//     string text = doc.GetPageText(i);
// }

// After (IronPDF — 0-based):
for (int i = 0; i < pdf.PageCount; i++)
{
    string text = pdf.ExtractTextFromPage(i);
}

// Converting a stored page reference:
int gnosticePage = 5;
int ironPdfIndex = gnosticePage - 1; // = 4
```

**Unit test to catch this:**
```csharp
[Fact]
public void FirstPage_IsIndexZero()
{
    using var pdf = PdfDocument.FromFile("test.pdf");
    Assert.True(pdf.PageCount > 0);

    var first = pdf.Pages[0];
    Assert.True(first.Width > 0);
}
```

---

### Issue 3: No Exception on Corrupted/Missing PDF

**Symptom:** `PdfDocument.FromFile()` throws a different exception type than your Gnostice code was catching, or surfaces a problem in a different place in the call stack.

**Why it happens:** PDFOne tends to surface errors through status returns and library-specific exception types. IronPDF uses standard .NET exceptions. Any `try/catch` ported verbatim will need its catch clauses re-typed.

**Fix:**
```csharp
try
{
    using var pdf = PdfDocument.FromFile("potentially-corrupt.pdf");
    Console.WriteLine($"Loaded {pdf.PageCount} pages");
}
catch (IronPdf.Exceptions.PdfException ex)
{
    Console.Error.WriteLine($"PDF format error: {ex.Message}");
}
catch (System.IO.FileNotFoundException ex)
{
    Console.Error.WriteLine($"File not found: {ex.Message}");
}
catch (UnauthorizedAccessException ex)
{
    Console.Error.WriteLine($"Permission denied: {ex.Message}");
}
```

---

### Issue 4: Password-Protected PDF Won't Open

**Symptom:** `PdfDocument.FromFile()` fails on a password-protected PDF.

**Fix:**
```csharp
// Load a password-protected PDF in IronPDF
// See: https://ironpdf.com/how-to/pdf-permissions-passwords/
try
{
    using var pdf = PdfDocument.FromFile("protected.pdf", "userpassword");
    Console.WriteLine($"Opened: {pdf.PageCount} pages");
}
catch (IronPdf.Exceptions.PdfException)
{
    Console.Error.WriteLine("Incorrect password or corrupted file");
}
```

---

### Issue 5: Watermark Opacity or Positioning Looks Wrong

**Symptom:** Watermark appears in the wrong position or at a different opacity after migration.

**Why it happens:** Gnostice draws watermarks at explicit X/Y coordinates with rotation angles applied to a text element. IronPDF lays watermarks out with alignment enums and percentage opacity. The mental model is different, and a one-to-one numeric port will not produce the same placement.

**Fix:**
```csharp
// IronPDF stamp positioning uses enum-based alignment
// See: https://ironpdf.com/how-to/stamp-text-image/
using IronPdf.Editing;

var stamp = new TextStamper
{
    Text                = "CONFIDENTIAL",
    FontSize            = 36,
    // Opacity in IronPDF is an integer 0–100
    Opacity             = 30,
    Rotation            = -45,
    VerticalAlignment   = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};

// Apply to all pages (default) or specific pages:
pdf.ApplyStamp(stamp);                     // all pages
pdf.ApplyStamp(stamp, new[] { 0, 1 });     // pages 0 and 1 only
```

---

### Issue 6: Merge Result Has Wrong Page Order

**Symptom:** After migrating from `doc1.Append(doc2)` to `PdfDocument.Merge()`, pages appear in an unexpected order.

**Why it happens:** Gnostice's `Append` mutates the receiver in place — pages from the argument are appended onto `doc1`. IronPDF's `Merge` is a static method whose argument order defines the page order in a new document. If your old code built up a merged document by repeatedly appending into one base document, the equivalent IronPDF call needs the documents in that same left-to-right order.

**Fix:**
```csharp
// IronPDF Merge: documents merged left-to-right in argument order
// file1 pages first, then file2 pages
// See: https://ironpdf.com/how-to/merge-or-split-pdfs/

using var doc1 = PdfDocument.FromFile("part1.pdf");
using var doc2 = PdfDocument.FromFile("part2.pdf");

// Result: all doc1 pages, then all doc2 pages
using var merged = PdfDocument.Merge(doc1, doc2);

// If order is wrong, swap arguments:
// using var merged = PdfDocument.Merge(doc2, doc1);

Console.WriteLine($"Page order: {merged.PageCount} total pages");
```

---

### Issue 7: Memory Behavior Under Load

**Symptom:** Memory consumption grows under sustained processing.

**Why it happens:** Gnostice user forums have reported memory-leak patterns under repeated processing, particularly with images. IronPDF generally manages memory well, but `PdfDocument` and `ChromePdfRenderer` still hold unmanaged resources. Missing `using` blocks or explicit `Dispose()` calls re-introduce leaks regardless of the underlying engine.

**Fix:**
```csharp
// Always use 'using' for PdfDocument instances
// Without disposal: native resources accumulate
// var pdf = PdfDocument.FromFile("input.pdf");
// pdf.SaveAs("output.pdf");

using var pdf = PdfDocument.FromFile("input.pdf");
pdf.SaveAs("output.pdf");

// In loops: dispose per-iteration
foreach (var path in filePaths)
{
    using var doc = PdfDocument.FromFile(path);
    int pages = doc.PageCount;
    // doc disposed at end of each iteration
}
```

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (Gnostice)**
```csharp
using Gnostice.Documents;
using Gnostice.Documents.PDF;

class Program
{
    static void Main()
    {
        // No external CSS, no JavaScript
        DocExporter exporter = new DocExporter();
        exporter.Preferences.PDFExportPreferences.PageSize = PDFPageSize.A4;

        // External CSS will not load
        Document doc = DocumentManager.LoadDocument("report.html");
        exporter.Export(doc, "report.pdf", DocumentFormat.PDF);
        doc.Close();
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;

// See: https://ironpdf.com/how-to/html-string-to-pdf/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.RenderDelay = 500; // wait for JS / web fonts

using var pdf = renderer.RenderHtmlFileAsPdf("report.html");
pdf.SaveAs("report.pdf");
Console.WriteLine($"Created: {pdf.PageCount} page(s)");
```

---

### 2. Merge PDFs

**Before (Gnostice PDFOne)**
```csharp
using Gnostice.PDFOne;
using Gnostice.PDFOne.Document;

class MergeExample
{
    static void Main()
    {
        PDFDocument doc1 = new PDFDocument();
        doc1.Load("document1.pdf");

        PDFDocument doc2 = new PDFDocument();
        doc2.Load("document2.pdf");

        PDFDocument mergedDoc = new PDFDocument();
        mergedDoc.Open();

        mergedDoc.Append(doc1);
        mergedDoc.Append(doc2);

        mergedDoc.Save("merged.pdf");

        doc1.Close();
        doc2.Close();
        mergedDoc.Close();
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;

// See: https://ironpdf.com/how-to/merge-or-split-pdfs/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

using var doc1 = PdfDocument.FromFile("document1.pdf");
using var doc2 = PdfDocument.FromFile("document2.pdf");

using var merged = PdfDocument.Merge(doc1, doc2);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (Gnostice PDFOne)**
```csharp
using Gnostice.PDFOne;
using Gnostice.PDFOne.Graphics;
using System.Drawing;

class WatermarkExample
{
    static void Main()
    {
        PDFDocument doc = new PDFDocument();
        doc.Load("input.pdf");

        PDFFont font = new PDFFont(PDFStandardFont.Helvetica, 48);

        foreach (PDFPage page in doc.Pages)
        {
            PDFTextElement watermark = new PDFTextElement();
            watermark.Text = "CONFIDENTIAL";
            watermark.Font = font;
            watermark.Color = Color.FromArgb(128, 255, 0, 0); // semi-transparent red
            watermark.RotationAngle = 45;

            // Manual X/Y coordinates for each draw
            watermark.Draw(page, 200, 400);
        }

        doc.Save("watermarked.pdf");
        doc.Close();
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;
using IronPdf.Editing;

// See: https://ironpdf.com/how-to/stamp-text-image/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

using var pdf = PdfDocument.FromFile("input.pdf");

var stamp = new TextStamper
{
    Text                = "CONFIDENTIAL",
    FontSize            = 48,
    Opacity             = 50,   // integer 0–100
    Rotation            = 45,
    VerticalAlignment   = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};

pdf.ApplyStamp(stamp);
pdf.SaveAs("watermarked.pdf");
```

---

### 4. Password Protection

**Before (Gnostice PDFOne)**
```csharp
using Gnostice.PDFOne;

class SecurityExample
{
    static void Main()
    {
        PDFDocument doc = new PDFDocument();
        doc.Load("input.pdf");

        doc.SetEncryption(
            PDFEncryptionMethod.AES256,
            "user123",
            "owner456",
            PDFPermissions.None | PDFPermissions.PrintDocument
        );

        doc.Save("protected.pdf");
        doc.Close();
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;
using IronPdf.Security;

// See: https://ironpdf.com/how-to/pdf-permissions-passwords/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

using var pdf = PdfDocument.FromFile("input.pdf");

pdf.SecuritySettings.UserPassword  = "user123";
pdf.SecuritySettings.OwnerPassword = "owner456";
pdf.SecuritySettings.AllowUserPrinting         = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;

pdf.SaveAs("protected.pdf");
```

---

## Critical Migration Notes

### Page Indexing

IronPDF is 0-based throughout. Gnostice PDFOne code commonly uses 1-based loops (`for (int i = 1; i <= doc.Pages.Count; i++)`). Any stored page-number value carried over from old code needs adjustment by one.

### Error Surfacing

Gnostice surfaces errors through library-specific exception types and status returns. IronPDF uses standard .NET exceptions (`PdfException`, `FileNotFoundException`, `UnauthorizedAccessException`). Audit any ported `try/catch` blocks.

### Thread Safety

For concurrent workloads, use one `ChromePdfRenderer` per thread. `PdfDocument` instances are not designed for shared cross-thread access.

---

## Performance Considerations

### Renderer Reuse

```csharp
// Single renderer instance across multiple operations in same thread
var renderer = new ChromePdfRenderer();

foreach (var template in templates)
{
    using var pdf = renderer.RenderHtmlAsPdf(template.Html);
    pdf.SaveAs(template.OutputPath);
}
```

### Async for Web Contexts

```csharp
// See: https://ironpdf.com/how-to/async/
var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(htmlContent);
await File.WriteAllBytesAsync(outputPath, pdf.BinaryData);
```

---

## Migration Checklist

### Pre-Migration
- [ ] Inventory all Gnostice usage in codebase (`rg "using Gnostice|PDFOne|DocExporter" --type cs`)
- [ ] Identify any viewer-control dependencies (IronPDF does not provide a viewer)
- [ ] Note features that were documented as unsupported in Gnostice (external CSS, JS, RTL) — they will work in IronPDF
- [ ] Obtain IronPDF license key; confirm activation
- [ ] Review [ironpdf.com/how-to/license-keys/](https://ironpdf.com/how-to/license-keys/)
- [ ] Create migration branch
- [ ] Set up PDF visual diff tooling

### Code Migration
- [ ] Remove `PDFOne.NET` and any `Gnostice.DocumentStudio.*` NuGet packages
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `using Gnostice.PDFOne` with `using IronPdf`
- [ ] Replace `Gnostice.PDFOne.PDFOne.LicenseKey` with `IronPdf.License.LicenseKey`
- [ ] Convert `PDFDocument` to `PdfDocument` and `DocExporter` to `ChromePdfRenderer`
- [ ] Replace coordinate-based drawing with HTML / CSS stamping
- [ ] Update page loops from 1-based to 0-based
- [ ] Replace `SetEncryption(...)` with `SecuritySettings` properties
- [ ] Re-type any `try/catch` to match IronPDF exception types
- [ ] Add `using` blocks for all `PdfDocument` instances

### Testing
- [ ] Run troubleshooting checklist against each migrated feature
- [ ] Visual comparison of rendered output
- [ ] Page index off-by-one test (first and last page)
- [ ] Password protection roundtrip
- [ ] Merge page order verification
- [ ] Watermark placement and opacity
- [ ] Memory check under sustained batch processing
- [ ] Concurrent operation test

### Post-Migration
- [ ] Remove Gnostice license keys from configuration
- [ ] Update deployment scripts
- [ ] Pin IronPDF version
- [ ] Document migration decisions for future team members

---

## Before You Ship

Gnostice migrations tend to surface issues in three places: rendering differences (HTML that finally renders correctly because external CSS and JavaScript now work), page indexing (Gnostice code commonly used 1-based loops), and error handling (different exception types in catch clauses). The troubleshooting section above covers the most common patterns, but production systems always have surprises.

One question worth raising: **with PDFOne.NET deprecated, are you migrating to escape the legacy line entirely, or could the `Gnostice.DocumentStudio.*` packages cover your usage?** If your feature mix is primarily HTML-to-PDF, RTL text, or modern CSS layouts, IronPDF removes documented limitations rather than papering over them. If you only use coordinate-based PDF manipulation and no HTML, the calculus is different — and worth being explicit about before scoping the work.

Drop specific edge cases in the comments — especially anything related to form fields or annotation workflows.
