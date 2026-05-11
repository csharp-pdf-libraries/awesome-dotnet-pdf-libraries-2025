---
title: "Migrating from Foxit PDF SDK to IronPDF: the honest walkthrough"
published: false
tags: dotnet, csharp, pdf, migration
---

The licensing email arrives on a Tuesday. Renewal quote attached. Your team's Foxit PDF SDK contract is up, and the number has moved. You open the renewal, scan the per-server clause, and a quiet audit begins: how deeply is this SDK embedded, and what would it take to swap it out?

That's the moment this article is written for.

Whether you renew, migrate, or build a hybrid approach is entirely your call. What follows is the technical map for one specific path: replacing Foxit PDF SDK with [IronPDF](https://ironpdf.com/) in a .NET application. You'll walk away with working before/after code, API mapping tables, and a printable checklist — regardless of which direction you go.

---

## Why Migrate (Without Drama)

Migration decisions rarely come from a single trigger. More often it's an accumulation:

1. **Licensing model friction** — per-server or per-CPU pricing becomes a variable cost at scale.
2. **Native DLL deployment complexity** — Foxit's NuGet package weighs in around 240 MB, and HTML-to-PDF needs the separate HTML2PDF engine binaries from Foxit support, adding CI/CD overhead.
3. **HTML/CSS rendering fidelity** — if your primary workload is HTML-to-PDF, a dedicated Chromium-based renderer can outperform a general-purpose SDK with a bolt-on conversion engine.
4. **API verbosity** — error-code checks, explicit `Library.Initialize`/`Library.Release` lifecycle, and `Close()`/`Release()` cleanup add boilerplate.
5. **C++ heritage** — Foxit's API patterns reflect its C++ origins (lowercase `foxit.*` namespaces, enum-style flags) and can feel less idiomatic in modern C#.
6. **Container/Linux support** — native dependency chains can complicate Docker images.
7. **HTML2PDF engine distribution** — the engine ships separately from the main NuGet package and is obtained through Foxit support/sales rather than NuGet.
8. **Version lock** — tight coupling to a specific SDK version makes .NET upgrades riskier.
9. **Support tier access** — enterprise support costs vary significantly between vendors.
10. **Team velocity** — a steeper learning curve adds onboarding time for new engineers.

### Comparison Table

| Aspect | Foxit PDF SDK | IronPDF |
|---|---|---|
| Focus | Full document lifecycle (view, edit, sign, form, annotate) | HTML-to-PDF rendering + document manipulation |
| Pricing | Commercial license; per-server/CPU tiers | Commercial license; OEM/SaaS options |
| API Style | Object-oriented, C++ heritage, error-code returns | Fluent, renderer-centric, .NET exceptions |
| Learning Curve | Medium-High — broad surface area | Medium — focused on rendering tasks |
| HTML Rendering | Requires separate HTML2PDF engine download | Built-in Chromium |
| Page Indexing | 0-based | 0-based |
| Resource Cleanup | Manual `Close()` / `Library.Release()` | `IDisposable` / `using` |
| Namespace | `foxit.*` (lowercase) | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Effort | Notes |
|---|---|---|
| HTML string to PDF | Low | Near 1:1 conceptual mapping; IronPDF drops the separate engine requirement |
| URL to PDF | Low | Both support URL rendering |
| PDF merge | Low | Both expose merge APIs (`InsertDocument` vs `PdfDocument.Merge`) |
| PDF split | Medium | `pdf.CopyPages(indices)` in IronPDF |
| Text extraction | Medium | `TextPage` vs `pdf.ExtractAllText()` / `ExtractTextFromPage(i)` |
| Form filling | Medium-High | `Form`/`Field` (Foxit) vs `pdf.Form.GetFieldByName(name)` |
| Digital signatures | High | Certificate chain handling differs |
| Annotations | Medium-High | Annotation model differs |
| Watermarking | Low | `Watermark`/`InsertToPage` vs `TextStamper`/`ApplyStamp` |
| Password protection | Low | `StdSecurityHandler` vs `SecuritySettings` |
| PDF/A compliance | Medium | Configurable in IronPDF rendering options |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Primary use case is HTML-to-PDF | IronPDF migration is low-risk |
| Heavy annotation/form workflows | Evaluate carefully; test before committing |
| Signing workflows with HSM | Review [IronPDF signing documentation](https://ironpdf.com/how-to/signing/) before committing |
| Full document viewer needed | IronPDF is not a viewer — Foxit SDK may be the better fit |

---

## Before You Start

### Prerequisites

- .NET 6, 7, or 8 project (IronPDF also supports .NET Framework 4.6.2+ and .NET Core 3.1+)
- NuGet access
- A valid IronPDF license key ([license setup](https://ironpdf.com/how-to/license-keys/))

### Find Foxit References in Your Codebase

```bash
# Find all Foxit using directives (note: lowercase namespaces)
rg "using foxit" --type cs

# Find all Foxit type references
rg "PDFDoc|Library\.Initialize|HTML2PDF" --type cs -l

# Find NuGet package references
rg "Foxit\.SDK\.Dotnet" *.csproj **/*.csproj
```

### Remove Foxit, Add IronPDF

```bash
# Remove Foxit package
dotnet remove package Foxit.SDK.Dotnet

# Add IronPDF
dotnet add package IronPdf

# Restore
dotnet restore
```

If you have older `<Reference Include="fsdk_dotnet">` direct DLL references in your `.csproj`, remove them manually. Also delete any HTML2PDF engine folders that were unpacked separately on disk or in your deployment artifacts.

---

## Quick Start Migration (3 Steps)

### Step 1: License Configuration

**Before (Foxit)**
```csharp
using foxit;
using foxit.common;

string sn  = "YOUR_FOXIT_SERIAL";
string key = "YOUR_FOXIT_KEY";

ErrorCode err = Library.Initialize(sn, key);
if (err != ErrorCode.e_ErrSuccess)
{
    throw new Exception("Failed to initialize Foxit SDK");
}

// ... PDF operations ...

Library.Release();
```

**After (IronPDF)**
```csharp
using IronPdf;

// Set license key before any IronPDF calls
// See: https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Or via appsettings.json / environment variable
// IronPdf:LicenseKey in configuration
```

### Step 2: Namespace Imports

**Before**
```csharp
using foxit;
using foxit.common;
using foxit.common.fxcrt;
using foxit.pdf;
using foxit.pdf.annots;
using foxit.addon.conversion;
```

**After**
```csharp
using IronPdf;
using IronPdf.Rendering;   // for ChromePdfRenderOptions
using IronPdf.Editing;     // for TextStamper, etc.
```

### Step 3: Basic Conversion

**Before (HTML string to PDF — Foxit)**
```csharp
using foxit;
using foxit.common;
using foxit.addon.conversion;

Library.Initialize("sn", "key");
try
{
    HTML2PDFSettingData settings = new HTML2PDFSettingData();
    settings.page_width  = 612.0f;  // Letter
    settings.page_height = 792.0f;
    settings.page_mode   = HTML2PDFPageMode.e_HTML2PDFPageModeSinglePage;

    // engine_path points to the Foxit HTML2PDF engine binaries
    // shipped separately by Foxit support/sales.
    Convert.FromHTML(
        "<html><body><h1>Hello</h1></body></html>",
        @"C:\Foxit\html2pdf_engine",  // engine_path
        "",                            // cookies path
        settings,
        "output.pdf",
        30);                           // timeout (seconds)
}
finally
{
    Library.Release();
}
```

**After (IronPDF)**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf("<h1>Hello, IronPDF</h1>");
pdf.SaveAs("output.pdf");
// See: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| Foxit Namespace | IronPDF Namespace | Purpose |
|---|---|---|
| `foxit` | `IronPdf` | Core types |
| `foxit.common` | `IronPdf` | Shared types, errors |
| `foxit.pdf` | `IronPdf` | Document operations |
| `foxit.pdf.annots` | `IronPdf.Editing` | Annotations / stampers |
| `foxit.pdf.graphics` | `IronPdf.Drawing` | Graphics |
| `foxit.addon.conversion` | `IronPdf.Rendering` | HTML/image conversion |

### Core Class Mapping

| Foxit Class | IronPDF Class | Description |
|---|---|---|
| `Library` (static lifecycle) | n/a | IronPDF auto-manages — set `License.LicenseKey` once |
| `PDFDoc` | `PdfDocument` | Main document object |
| `Convert.FromHTML` (static) | `ChromePdfRenderer.RenderHtmlAsPdf` | HTML-to-PDF rendering |
| `HTML2PDFSettingData` | `ChromePdfRenderOptions` | Rendering settings |
| `PDFPage` | `PdfDocument.Pages[n]` | Page access |
| `Watermark` + `WatermarkSettings` | `TextStamper` / `ImageStamper` | Watermarks |
| `StdSecurityHandler` + `StdEncryptData` | `PdfDocument.SecuritySettings` | Encryption/passwords |
| `TextPage` | `pdf.ExtractTextFromPage(i)` | Text extraction |
| `Form` / `Field` | `pdf.Form` / `GetFieldByName(name)` | Form fields |

### Document Loading Methods

| Operation | Foxit | IronPDF |
|---|---|---|
| Load from file | `new PDFDoc(path)` + `doc.LoadW("")` | `PdfDocument.FromFile(path)` |
| Load password-protected | `new PDFDoc(path)` + `doc.LoadW(password)` | `PdfDocument.FromFile(path, password)` |
| Load from bytes | Stream-based load API | `PdfDocument.FromBinaryData(bytes)` |
| Load from stream | Stream-based load API | `PdfDocument.FromStream(stream)` |
| Render from HTML string | `Convert.FromHTML(html, enginePath, ...)` | `renderer.RenderHtmlAsPdf(html)` |

### Page Operations

| Operation | Foxit | IronPDF |
|---|---|---|
| Page count | `doc.GetPageCount()` | `pdf.PageCount` |
| Get page | `doc.GetPage(index)` | `pdf.Pages[index]` |
| Page size | `page.GetWidth()` / `page.GetHeight()` | `page.Width`, `page.Height` |
| Rotate page | `page.GetRotation()` / set via page object | `page.Rotation` |

### Merge/Split Operations

| Operation | Foxit | IronPDF |
|---|---|---|
| Merge documents | `doc1.InsertDocument(insertAt, doc2, ranges, flags)` | `PdfDocument.Merge(doc1, doc2)` |
| Extract pages | Iterate pages or use range-based copy | `pdf.CopyPages(indices)` |
| Remove pages | `doc.RemovePage(index)` | `pdf.RemovePages(indices)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (Foxit)**
```csharp
using foxit;
using foxit.common;
using foxit.addon.conversion;
using System;

class Program
{
    static void Main(string[] args)
    {
        ErrorCode err = Library.Initialize("YOUR_FOXIT_SERIAL", "YOUR_FOXIT_KEY");
        if (err != ErrorCode.e_ErrSuccess) return;

        try
        {
            HTML2PDFSettingData settings = new HTML2PDFSettingData();
            settings.page_width  = 612.0f;  // Letter width in points
            settings.page_height = 792.0f;  // Letter height in points
            settings.page_mode   = HTML2PDFPageMode.e_HTML2PDFPageModeSinglePage;
            settings.page_margin_top    = 72.0f;
            settings.page_margin_bottom = 72.0f;

            string html = "<html><body><h1>Report</h1><p>Content here.</p></body></html>";

            // engine_path points to the Foxit HTML2PDF engine binaries
            // shipped separately by Foxit support/sales.
            Convert.FromHTML(
                html,
                @"C:\Foxit\html2pdf_engine",
                "",
                settings,
                "output.pdf",
                30);

            Console.WriteLine("PDF created");
        }
        finally
        {
            Library.Release();
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

// Optional: configure rendering
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 10;

string html = "<html><body><h1>Report</h1><p>Content here.</p></body></html>";
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
Console.WriteLine($"Pages: {pdf.PageCount}");
```

---

### 2. Merge PDFs

**Before (Foxit)**
```csharp
using foxit;
using foxit.common;
using foxit.pdf;
using System;

class MergeExample
{
    static void Main(string[] args)
    {
        Library.Initialize("sn", "key");

        try
        {
            using (PDFDoc doc1 = new PDFDoc("file1.pdf"))
            using (PDFDoc doc2 = new PDFDoc("file2.pdf"))
            {
                doc1.LoadW("");
                doc2.LoadW("");

                int doc2PageCount = doc2.GetPageCount();
                Range[] ranges = new Range[] { new Range(0, doc2PageCount - 1) };
                RangeArray rangeArray = new RangeArray(ranges);

                // Insert doc2 pages at the end of doc1
                doc1.InsertDocument(doc1.GetPageCount(), doc2, rangeArray, 0);

                doc1.SaveAs("merged.pdf", (int)PDFDoc.SaveFlags.e_SaveFlagNoOriginal);
            }
        }
        finally
        {
            Library.Release();
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

// Static merge — returns a new PdfDocument
using var merged = PdfDocument.Merge(doc1, doc2);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (Foxit)**
```csharp
using foxit;
using foxit.common;
using foxit.pdf;
using System;

class WatermarkExample
{
    static void Main(string[] args)
    {
        Library.Initialize("sn", "key");

        try
        {
            using (PDFDoc doc = new PDFDoc("input.pdf"))
            {
                doc.LoadW("");

                WatermarkSettings settings = new WatermarkSettings();
                settings.flags    = (int)Watermark.Flags.e_FlagASPageContents;
                settings.position = Position.e_PosCenter;
                settings.rotation = -45.0f;
                settings.opacity  = 30;  // 0-100 in newer SDKs

                WatermarkTextProperties props = new WatermarkTextProperties();
                props.font      = new Font(Font.StandardID.e_StdIDHelvetica);
                props.font_size = 72.0f;
                props.color     = 0xFF0000;  // RGB
                props.alignment = Alignment.e_AlignmentCenter;

                Watermark watermark = new Watermark(doc, "CONFIDENTIAL", props, settings);

                // No InsertToAllPages helper — iterate pages explicitly
                for (int i = 0; i < doc.GetPageCount(); i++)
                {
                    using (PDFPage page = doc.GetPage(i))
                    {
                        watermark.InsertToPage(page);
                    }
                }

                doc.SaveAs("watermarked.pdf", (int)PDFDoc.SaveFlags.e_SaveFlagNoOriginal);
            }
        }
        finally
        {
            Library.Release();
        }
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

// Create a text stamp (Opacity is 0-100, matching Foxit's scale)
var stamp = new TextStamper
{
    Text                = "CONFIDENTIAL",
    FontSize            = 36,
    Opacity             = 40,
    Rotation            = -45,
    VerticalAlignment   = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};

pdf.ApplyStamp(stamp);
pdf.SaveAs("watermarked.pdf");
```

---

### 4. Password Protection

**Before (Foxit)**
```csharp
using foxit;
using foxit.common;
using foxit.pdf;
using System;

class SecurityExample
{
    static void Main(string[] args)
    {
        Library.Initialize("sn", "key");

        try
        {
            using (PDFDoc doc = new PDFDoc("input.pdf"))
            {
                doc.LoadW("");

                using (StdEncryptData encryptData = new StdEncryptData(
                    true,                                    // is_encrypt_metadata
                    (int)(PDFDoc.UserPermissions.e_PermPrint |
                          PDFDoc.UserPermissions.e_PermModify),
                    SecurityHandler.CipherType.e_CipherAES,
                    16))                                     // key length (bytes) → AES-128
                using (StdSecurityHandler securityHandler = new StdSecurityHandler())
                {
                    securityHandler.Initialize(encryptData, "user123", "owner456");
                    doc.SetSecurityHandler(securityHandler);
                }

                doc.SaveAs("protected.pdf", (int)PDFDoc.SaveFlags.e_SaveFlagNoOriginal);
            }
        }
        finally
        {
            Library.Release();
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

// Set user and owner passwords with permissions
pdf.SecuritySettings.UserPassword  = "user123";
pdf.SecuritySettings.OwnerPassword = "owner456";

// Restrict permissions
pdf.SecuritySettings.AllowUserPrinting       = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserAnnotations    = false;

pdf.SaveAs("protected.pdf");
```

---

## Critical Migration Notes

### Page Indexing

Both Foxit and IronPDF use 0-based page indexing for the common page-access patterns shown in this guide (`doc.GetPage(i)` / `pdf.Pages[i]`).

```csharp
// IronPDF: 0-based
var firstPage = pdf.Pages[0];
var lastPage  = pdf.Pages[pdf.PageCount - 1];
```

### Error Handling: Status Codes vs Exceptions

Foxit PDF SDK APIs return `ErrorCode` enum values that must be checked after each call. IronPDF raises standard .NET exceptions on failure.

```csharp
// IronPDF exception pattern
try
{
    using var pdf = PdfDocument.FromFile("input.pdf");
    // operations...
}
catch (IronPdf.Exceptions.IronPdfNativeException ex)
{
    Console.Error.WriteLine($"PDF error: {ex.Message}");
}
catch (System.IO.FileNotFoundException ex)
{
    Console.Error.WriteLine($"File not found: {ex.Message}");
}
```

### Unit Conversion

IronPDF margins and sizes use millimeters (mm) by default. Foxit uses points (1/72 inch) in most APIs. Converting: `mm = points × 0.3528`.

```csharp
// IronPDF margin in mm
renderer.RenderingOptions.MarginTop    = 25.4; // 1 inch
renderer.RenderingOptions.MarginBottom = 25.4;
renderer.RenderingOptions.MarginLeft   = 25.4;
renderer.RenderingOptions.MarginRight  = 25.4;
```

### HTML2PDF Engine Distribution

Foxit's HTML-to-PDF capability ships in a separate engine package obtained through Foxit support/sales, not in the core `Foxit.SDK.Dotnet` NuGet package. The `engine_path` argument to `Convert.FromHTML` points at those engine binaries on disk. IronPDF bundles Chromium in the NuGet package — no separate engine download, no second deployment artifact.

---

## Performance Considerations

### Renderer Reuse

`ChromePdfRenderer` is relatively expensive to initialize. In throughput scenarios, reuse the same instance across requests rather than constructing per-call.

```csharp
// Good: single renderer instance reused
var renderer = new ChromePdfRenderer();

foreach (var htmlItem in htmlQueue)
{
    using var pdf = renderer.RenderHtmlAsPdf(htmlItem);
    pdf.SaveAs(GetOutputPath(htmlItem));
}
```

### Disposal

`PdfDocument` implements `IDisposable`. Always dispose — especially in loops.

```csharp
// Correct disposal pattern
using var pdf = PdfDocument.FromFile("input.pdf");
// ... work with pdf ...
// Disposed at end of using block
```

### Parallel Rendering

For parallel workloads, use one `ChromePdfRenderer` per thread or use the async API. See [IronPDF parallel rendering examples](https://ironpdf.com/examples/parallel/) for validated patterns.

```csharp
// Parallel — one renderer per task
var results = await Task.WhenAll(htmlItems.Select(async html =>
{
    var renderer = new ChromePdfRenderer(); // per-task instance
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}));
```

### Edge Cases to Flag

- **Very large HTML documents** — Chromium-based rendering is memory-intensive for extremely large pages.
- **External resources** — HTML with cross-origin fonts/images may require `WaitFor` or timeout configuration. See [IronPDF rendering options](https://ironpdf.com/how-to/rendering-options/).
- **Custom fonts** — Embed fonts in HTML/CSS rather than relying on system font resolution.

---

## Migration Checklist

### Pre-Migration
- [ ] Inventory all Foxit namespaces in use (`rg "using foxit" --type cs`)
- [ ] List all PDF operations performed (render, merge, split, sign, form, annotate)
- [ ] Identify any operations with no IronPDF equivalent (viewer, complex annotations)
- [ ] Confirm IronPDF supports your target .NET version
- [ ] Obtain IronPDF license key and verify it activates
- [ ] Review [IronPDF license setup](https://ironpdf.com/how-to/license-keys/)
- [ ] Back up codebase / create migration branch
- [ ] Identify integration test coverage for PDF output

### Code Migration
- [ ] Remove `Foxit.SDK.Dotnet` NuGet package and any direct `fsdk_dotnet` `<Reference>` entries
- [ ] Delete any unpacked HTML2PDF engine folders from disk and deployment artifacts
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `using foxit.*` with `using IronPdf` (+ `IronPdf.Editing`, `IronPdf.Rendering` as needed)
- [ ] Set `IronPdf.License.LicenseKey` at application startup
- [ ] Remove all `Library.Initialize(sn, key)` and `Library.Release()` calls
- [ ] Replace `Convert.FromHTML(...)` calls with `ChromePdfRenderer.RenderHtmlAsPdf(...)` (see Quick Start above)
- [ ] Replace `doc1.InsertDocument(...)` merges with `PdfDocument.Merge(...)`
- [ ] Replace `Watermark`/`WatermarkSettings` flow with `TextStamper` + `pdf.ApplyStamp(...)`
- [ ] Replace `StdSecurityHandler`/`StdEncryptData` flow with `pdf.SecuritySettings.*`
- [ ] Update error handling from `ErrorCode` checks to try/catch
- [ ] Update unit conversions (points → mm) for margins/sizes

### Testing
- [ ] Render 10 representative HTML templates; compare output visually
- [ ] Verify page counts match expected values
- [ ] Test merge with 2+ documents; verify page order
- [ ] Test password protection: open with user password, verify restrictions
- [ ] Test watermark visibility and positioning
- [ ] Load-test renderer reuse pattern under concurrency
- [ ] Verify disposal — run with memory profiler under load

### Post-Migration
- [ ] Remove Foxit license keys from config/secrets
- [ ] Update deployment scripts (remove Foxit native DLL + engine staging steps)
- [ ] Update CI/CD pipeline (Docker images, if applicable)
- [ ] Document IronPDF version pinned in your project

---

## Conclusion

The migration from Foxit PDF SDK to IronPDF is straightforward for the core rendering and manipulation operations: HTML-to-PDF, merge, watermark, password protection. The complexity concentrates in features that rely on Foxit's deeper document model — advanced annotation APIs, form workflows, and signing — where the mapping is less direct and warrants careful evaluation before committing.

One open question worth thinking through before you start: **if your application performs both PDF generation (HTML → PDF) and PDF viewing/annotation in-process, does IronPDF fully replace Foxit SDK, or does your architecture need to separate the rendering path from the viewing/annotation path?**

Drop your edge cases in the comments — particularly around form workflows or signing pipelines.
