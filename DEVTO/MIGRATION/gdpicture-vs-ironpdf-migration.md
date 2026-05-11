---
title: "Migrating from GdPicture.NET SDK to IronPDF: from install to ship"
published: false
tags: dotnet, csharp, pdf, migration
---

There's a particular kind of technical debt that accumulates quietly: the imaging SDK you licensed four years ago because you needed PDF thumbnail generation. Then someone added barcode scanning to the same integration. Then OCR. Then PDF signing. Now you have a GdPicture.NET license that touches seven different workflows, the API has surface area in dozens of files, and when renewal time comes, the conversation is less "should we renew" and more "can we even untangle this."

This guide is for the narrower scenario: you want to replace GdPicture.NET's PDF generation and manipulation features specifically, using [IronPDF](https://ironpdf.com/). If your GdPicture usage is purely imaging (TIFF processing, scanning, barcode), this article is less relevant. If your primary pain is HTML-to-PDF, merge, watermark, and document security — read on.

---

## Why Migrate (Without Drama)

Teams running GdPicture.NET for PDF work specifically — not imaging — commonly run into a subset of friction points:

1. **Module licensing complexity** — GdPicture.NET ships as a broad document imaging suite (OCR, barcode, scanning, image processing). PDF-only projects pay for surface area they don't use.
2. **API breadth vs. task specificity** — a platform built for imaging and PDF alike has a larger learning surface than a focused PDF library.
3. **HTML rendering capability** — GdPicture.NET's converter renders HTML through its own engine; teams that need full modern CSS3 / JavaScript fidelity may prefer a Chromium-based renderer.
4. **Status code error handling** — every operation returns a `GdPictureStatus` enum that has to be checked manually; modern .NET codebases typically expect exceptions.
5. **Page indexing model** — GdPicture is 1-indexed on `SelectPage(i)`-style operations, which mixes awkwardly with 0-indexed collections elsewhere in .NET.
6. **Resource management** — handles like `GdPicturePDF` require explicit `Dispose()` (or `using` blocks) and the SDK has its own release semantics for image handles.
7. **Versioned namespace** — the `GdPicture14` namespace embeds a version number, so major upgrades ripple through every `using` directive.
8. **Rebrand fragmentation** — the recent Nutrient rebrand creates documentation drift between gdpicture.com and nutrient.io while content settles.
9. **Container/Linux deployment** — large document SDKs tend to carry heavier native dependency chains than a focused PDF library.
10. **Team familiarity** — engineers hired specifically for PDF work generally onboard faster on a focused library than on a multi-modal imaging SDK.

### Comparison Table

| Aspect | GdPicture.NET SDK | IronPDF |
|---|---|---|
| Focus | Imaging, scanning, OCR, barcode, PDF — broad platform | HTML-to-PDF rendering + document manipulation |
| Pricing | Modular commercial license | Commercial license |
| API Style | Status codes, manual handle management | Fluent, renderer-centric, exception-based |
| Learning Curve | High overall; Medium for PDF-specific paths | Medium — focused surface area |
| HTML Rendering | Built-in converter (`GdPictureDocumentConverter`) | Primary design goal; Chromium-based |
| Page Indexing | 1-based on `SelectPage(i)` and similar APIs | 0-based on `Pages[i]` |
| Disposal | Explicit `Dispose()` / `using` required | `IDisposable` on `PdfDocument` — idiomatic `using` |
| Namespace | `GdPicture14` (version embedded) | `IronPdf` (stable) |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Effort | Notes |
|---|---|---|
| HTML string to PDF | Low | `LoadFromHTMLString` + `SaveAsPDF` maps cleanly to `ChromePdfRenderer.RenderHtmlAsPdf` |
| URL to PDF | Low | `LoadFromURL` maps to `RenderUrlAsPdf` |
| PDF merge | Low | `MergePages` maps to `PdfDocument.Merge` |
| PDF split | Low | `ClonePage` / `ExtractPages` map to `CopyPage` / `CopyPages` |
| Text extraction | Low | `GetPageText` maps to `ExtractTextFromPage`; mind page indexing |
| Image extraction | Medium | GdPicture imaging surface is broader than IronPDF's PDF-image extraction |
| OCR | N/A | Not in scope — pair with [IronOCR](https://ironsoftware.com/csharp/ocr/) if needed |
| Barcode | N/A | Not in scope — pair with [IronBarcode](https://ironsoftware.com/csharp/barcode/) if needed |
| Watermarking | Low | `DrawText`-loop pattern replaced by single `ApplyWatermark` call |
| Password protection | Low | Encryption args on `SaveToFile` move to `pdf.SecuritySettings.*` |
| Digital signatures | Medium | See [IronPDF signing](https://ironpdf.com/how-to/sign-pdf-with-digital-signature/) |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| PDF generation only (HTML/URL → PDF, merge, security) | IronPDF migration is well-scoped |
| PDF generation + OCR/barcode in same pipeline | Split: IronPDF for PDF, pair with IronOCR / IronBarcode |
| Scanning/TIFF workflows interleaved with PDF | Retain GdPicture for imaging; IronPDF won't replace those features |
| Greenfield .NET 8 service, PDF-only | IronPDF is a reasonable choice; evaluate against requirements |

---

## Before You Start

### Prerequisites

- .NET 6, 7, or 8 project (IronPDF also supports .NET Framework 4.6.2+ and .NET Core 2.0+)
- NuGet access
- IronPDF license key ([license key setup](https://ironpdf.com/how-to/license-keys/))
- Clarity on which GdPicture modules are actually in use

### Identify GdPicture Usage

```bash
# Find all GdPicture namespace imports
rg "using GdPicture" --type cs

# Find all GdPicture type references in code
rg "GdPicturePDF|GdPictureDocumentConverter|GdPictureStatus|LicenseManager\.RegisterKEY" --type cs

# Check project files for GdPicture packages
rg "GdPicture" **/*.csproj
```

### Remove GdPicture, Add IronPDF

```bash
# Remove the GdPicture package (your project may also reference companion
# packages like GdPicture.NET.API; remove each as applicable)
dotnet remove package GdPicture.NET

# Add IronPDF
dotnet add package IronPdf

dotnet restore
```

---

## Quick Start Migration (3 Steps)

### Step 1: License Configuration

**Before (GdPicture.NET)**
```csharp
using GdPicture14;

// Static call — must run before any GdPicture operation
LicenseManager.RegisterKEY("YOUR-LICENSE-KEY");
```

**After (IronPDF)**
```csharp
using IronPdf;

// Set once at application startup
// See: https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Alternative: via environment variable IronPdf__LicenseKey
// or appsettings.json under "IronPdf" : { "LicenseKey": "..." }
```

### Step 2: Namespace Imports

**Before**
```csharp
using GdPicture14;           // core PDF and converter types
using System.Drawing;        // used for Color when drawing/text-coloring
```

**After**
```csharp
using IronPdf;
using IronPdf.Rendering;     // for ChromePdfRenderOptions
using IronPdf.Editing;       // for Stamper types
```

### Step 3: Basic Conversion

**Before (GdPicture)**
```csharp
using GdPicture14;

LicenseManager.RegisterKEY("YOUR-LICENSE-KEY");

using (var converter = new GdPictureDocumentConverter())
{
    string html = "<html><body><h1>Hello</h1></body></html>";
    GdPictureStatus status = converter.LoadFromHTMLString(html);

    if (status == GdPictureStatus.OK)
    {
        converter.SaveAsPDF("output.pdf");
    }
}
```

**After (IronPDF)**
```csharp
using IronPdf;

// See: https://ironpdf.com/how-to/html-string-to-pdf/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1><p>Migrated from GdPicture.</p>");
pdf.SaveAs("output.pdf");
```

---

## API Mapping Tables

### Namespace Mapping

| GdPicture Namespace | IronPDF Namespace | Purpose |
|---|---|---|
| `GdPicture14` | `IronPdf` | Core types |
| `GdPicture14` (rendering options on converter) | `IronPdf.Rendering` | Rendering options |
| `GdPicture14` (encryption args on `SaveToFile`) | `IronPdf.Security` | Document security |

### Core Class Mapping

| GdPicture Class | IronPDF Class | Description |
|---|---|---|
| `GdPicturePDF` | `PdfDocument` | Main document object |
| `GdPictureDocumentConverter` | `ChromePdfRenderer` | HTML / URL to PDF rendering |
| `pdf.SelectPage(i)` + `pdf.*` | `PdfDocument.Pages[i]` | Page access |
| Encryption args on `SaveToFile` | `PdfDocument.SecuritySettings` | Encryption |
| `LicenseManager` | `IronPdf.License` | License management |
| `GdPictureStatus` | Exceptions | Error handling |

### Document Loading Methods

| Operation | GdPicture | IronPDF |
|---|---|---|
| Load from file | `pdf.LoadFromFile(path, false)` | `PdfDocument.FromFile(path)` |
| Load from stream | `pdf.LoadFromStream(stream)` | `PdfDocument.FromStream(stream)` |
| Load from bytes | Load via stream | `PdfDocument.FromBinaryData(bytes)` |
| Render from HTML string | `converter.LoadFromHTMLString(html)` + `SaveAsPDF` | `renderer.RenderHtmlAsPdf(html)` |
| Render from HTML file | `converter.LoadFromHTMLFile(path)` + `SaveAsPDF` | `renderer.RenderHtmlFileAsPdf(path)` |
| Render from URL | `converter.LoadFromURL(url)` + `SaveAsPDF` | `renderer.RenderUrlAsPdf(url)` |

### Page Operations

| Operation | GdPicture | IronPDF |
|---|---|---|
| Page count | `pdf.GetPageCount()` | `pdf.PageCount` |
| Select/get page | `pdf.SelectPage(i)` (1-based) | `pdf.Pages[i]` (0-based) |
| Page width | `pdf.GetPageWidth()` | `page.Width` |
| Page rotation | `pdf.RotatePage(angle)` | `page.Rotation` |
| Remove page | `pdf.RemovePage(pageNo)` | `pdf.RemovePages(index)` |

### Merge/Split Operations

| Operation | GdPicture | IronPDF |
|---|---|---|
| Merge documents | `pdf1.MergePages(pdf2)` | `PdfDocument.Merge(doc1, doc2)` |
| Clone single page | `pdf.ClonePage(source, i)` | `pdf.CopyPage(index)` |
| Extract pages | `pdf.ExtractPages(start, end)` | `pdf.CopyPages(indices)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (GdPicture)**
```csharp
using GdPicture14;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("YOUR-LICENSE-KEY");

        string html = "<html><body><h1>Report</h1><p>Quarterly summary.</p></body></html>";

        using (var converter = new GdPictureDocumentConverter())
        {
            GdPictureStatus status = converter.LoadFromHTMLString(html);

            if (status == GdPictureStatus.OK)
            {
                status = converter.SaveAsPDF("output.pdf");
                if (status != GdPictureStatus.OK)
                {
                    Console.Error.WriteLine($"Save error: {status}");
                }
            }
            else
            {
                Console.Error.WriteLine($"Load error: {status}");
            }
        }
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

string html = "<html><body><h1>Report</h1><p>Quarterly summary.</p></body></html>";
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
Console.WriteLine($"Generated {pdf.PageCount} page(s)");
```

---

### 2. Merge PDFs

**Before (GdPicture)**
```csharp
using GdPicture14;

class MergeExample
{
    static void Main()
    {
        LicenseManager.RegisterKEY("YOUR-LICENSE-KEY");

        using (var pdf1 = new GdPicturePDF())
        using (var pdf2 = new GdPicturePDF())
        {
            pdf1.LoadFromFile("file1.pdf", false);
            pdf2.LoadFromFile("file2.pdf", false);

            pdf1.MergePages(pdf2);
            pdf1.SaveToFile("merged.pdf");
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
Console.WriteLine($"Merged: {merged.PageCount} pages total");
```

---

### 3. Watermark

**Before (GdPicture)**
```csharp
using GdPicture14;
using System.Drawing;

class WatermarkExample
{
    static void Main()
    {
        LicenseManager.RegisterKEY("YOUR-LICENSE-KEY");

        using (var pdf = new GdPicturePDF())
        {
            pdf.LoadFromFile("input.pdf", false);

            for (int i = 1; i <= pdf.GetPageCount(); i++)
            {
                pdf.SelectPage(i);
                pdf.SetTextColor(Color.Red);
                pdf.SetTextSize(48);
                pdf.DrawText("CONFIDENTIAL", 200, 400);
            }

            pdf.SaveToFile("watermarked.pdf");
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

pdf.ApplyWatermark(
    "<h1 style='color:red;'>CONFIDENTIAL</h1>",
    50,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);

pdf.SaveAs("watermarked.pdf");
```

---

### 4. Password Protection

**Before (GdPicture)**
```csharp
using GdPicture14;

class SecurityExample
{
    static void Main()
    {
        LicenseManager.RegisterKEY("YOUR-LICENSE-KEY");

        using (var pdf = new GdPicturePDF())
        {
            pdf.LoadFromFile("input.pdf", false);

            // Encryption is configured at save time via SaveToFile overload:
            // path, encryption, userPwd, ownerPwd,
            // canPrint, canCopy, canModify, canAddNotes,
            // canFillForms, canExtract, canAssemble, canPrintHQ
            pdf.SaveToFile(
                "protected.pdf",
                PdfEncryption.PdfEncryption256BitAES,
                "user123",
                "owner456",
                true,   // canPrint
                false,  // canCopy
                false,  // canModify
                false,  // canAddNotes
                true,   // canFillForms
                false,  // canExtract
                false,  // canAssemble
                true);  // canPrintHQ
        }
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
pdf.SecuritySettings.AllowUserEdits            = PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserAnnotations      = false;
pdf.SecuritySettings.AllowUserFormData         = true;

pdf.SaveAs("protected.pdf");
```

---

## Critical Migration Notes

### Page Indexing: 1-Based vs 0-Based

GdPicture.NET uses **1-based** page indexing in many operations (e.g., `SelectPage(1)` for the first page). IronPDF uses **0-based** indexing. This is a common source of off-by-one bugs during migration.

```csharp
// GdPicture: pdf.SelectPage(1) selects the first page
// IronPDF equivalent:
var firstPage = pdf.Pages[0];                          // index 0 = first page
var lastPage  = pdf.Pages[pdf.PageCount - 1];
```

### Status Codes vs Exceptions

GdPicture returns `GdPictureStatus` codes from many operations; the caller checks `== GdPictureStatus.OK` after each step. IronPDF throws exceptions instead.

```csharp
// IronPDF: always use try/catch for file I/O and rendering
try
{
    using var pdf = PdfDocument.FromFile("maybe-missing.pdf");
}
catch (System.IO.FileNotFoundException ex)
{
    Console.Error.WriteLine($"File not found: {ex.Message}");
}
catch (IronPdf.Exceptions.PdfException ex)
{
    Console.Error.WriteLine($"PDF error: {ex.Message}");
}
```

### Disposal Patterns

GdPicture requires explicit `Dispose()` on its objects (typically via `using` on `GdPicturePDF` / `GdPictureDocumentConverter`). IronPDF's `PdfDocument` implements `IDisposable` the same way — use `using` blocks consistently.

```csharp
// IronPDF: using block ensures disposal
using var pdf = PdfDocument.FromFile("input.pdf");
// pdf is disposed when block exits, even on exception
```

---

## Performance Considerations

### Renderer Instance Management

`ChromePdfRenderer` carries initialization overhead. For batch processing, construct once and reuse:

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

// Process batch
foreach (var task in renderQueue)
{
    using var pdf = renderer.RenderHtmlAsPdf(task.Html);
    pdf.SaveAs(task.OutputPath);
}
```

### Async Support

For web API or async service contexts, use the async rendering path to avoid blocking thread pool threads:

```csharp
var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(htmlContent);
await File.WriteAllBytesAsync(outputPath, pdf.BinaryData);
```

### Memory Pressure from Large Documents

For pipelines processing many large PDFs, dispose each `PdfDocument` promptly. Holding many open simultaneously increases GC pressure.

```csharp
// Process files sequentially, dispose immediately
foreach (var filePath in pdfPaths)
{
    using var pdf = PdfDocument.FromFile(filePath);
    int pages = pdf.PageCount;
    // ... extract/transform ...
    // disposed here
}
```

---

## Migration Checklist

### Pre-Migration
- [ ] Identify all GdPicture modules currently licensed and in use
- [ ] Separate PDF operations from imaging/OCR/barcode operations
- [ ] Confirm IronPDF covers all PDF operations needed (not imaging)
- [ ] Verify IronPDF .NET version compatibility for your targets
- [ ] Obtain and test IronPDF license key
- [ ] Review the [license key setup guide](https://ironpdf.com/how-to/license-keys/)
- [ ] Create migration branch
- [ ] Baseline integration tests for PDF output quality

### Code Migration
- [ ] Remove GdPicture NuGet packages
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `using GdPicture14` with `using IronPdf`
- [ ] Initialize `IronPdf.License.LicenseKey` at startup (in place of `LicenseManager.RegisterKEY`)
- [ ] Migrate `GdPictureDocumentConverter` HTML/URL flows to `ChromePdfRenderer`
- [ ] Migrate `MergePages` calls to `PdfDocument.Merge` — check for 1-based vs 0-based index bugs
- [ ] Migrate watermark loops (`SelectPage` + `DrawText`) to `ApplyWatermark`
- [ ] Migrate `SaveToFile` encryption args to `pdf.SecuritySettings.*`
- [ ] Replace `GdPictureStatus` checks with try/catch
- [ ] Replace `GdPicturePDF` instances with `using var pdf = PdfDocument.From...`

### Testing
- [ ] Render representative HTML templates; visual comparison
- [ ] Test page counts and content accuracy post-merge
- [ ] Test password protection roundtrip (lock + open)
- [ ] Verify watermark placement matches requirements
- [ ] Test page index references for off-by-one errors
- [ ] Test under concurrency (renderer reuse pattern)
- [ ] Verify no GdPicture license calls remain in codebase

### Post-Migration
- [ ] Remove GdPicture license keys from configuration
- [ ] Simplify deployment (remove GdPicture native deps as applicable)
- [ ] Update container/Docker images
- [ ] Pin IronPDF version in all project files

---

## Final Thoughts

The GdPicture.NET migration story has a useful structural property: GdPicture's breadth (imaging, OCR, barcode) is also the natural migration boundary. Teams that used GdPicture primarily for PDF generation find the migration scope well-contained. Teams that used it for imaging and PDF interleaved need to be more deliberate about what stays and what moves.

A question worth raising in the comments: **when migrating from an imaging platform that included PDF as a module, what's been your experience keeping a multi-SDK setup long-term vs. consolidating to specialized tools per task?** Trade-offs in dependency management, licensing cost, and team expertise are all real considerations.
