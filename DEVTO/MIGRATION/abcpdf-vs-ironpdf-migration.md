---
title: "Migrating from ABCpdf to IronPDF: a practical .NET migration guide"
published: false
description: "A practical migration guide for .NET teams moving from ABCpdf to IronPDF. Covers API mapping, before/after code, effort estimation, and a printable checklist."
tags: dotnet, csharp, pdf, migration
---

Your ABCpdf `Doc` object just swallowed a 47-page HTML invoice and spat out a blank PDF—again. The bundled Chromium engine you pinned eighteen months ago is now several Chromium majors behind upstream, and the CSS grid you shipped last sprint renders as a single column. You could pin a newer ABCChrome runtime, patch the HTML, or file another ticket. Or you could migrate.

This guide walks you through replacing ABCpdf with [IronPDF](https://ironpdf.com) in a .NET codebase. Eighty percent of what follows is general migration knowledge—dependency auditing, API surface mapping, testing strategy—that applies even if you pick a different target library. The remaining twenty percent shows where IronPDF specifically reduces friction. Copy-paste the code, run the checklist, and ship.

## Why Migrate (Without Drama)

Nobody migrates a working PDF pipeline for fun. Here are the concrete triggers that push teams to evaluate alternatives:

1. **HTML engine version churn.** ABCpdf bundles multiple Chromium forks ("ABCChrome") and exposes them through `EngineType.Chrome123`, `Chrome117`, `Chrome86`, and `Chrome65`. The default in v13 is Chrome123, but pinning to an older engine to keep behavior stable means you fall behind on CSS features supported by modern browsers.
2. **Native runtime deployment.** ABCpdf requires platform-specific native binaries alongside the managed assembly. On Linux this means pulling the `ABCpdf.ABCChrome117.Linux` or `ABCpdf.ABCChrome123.Linux` runtime packages. Docker builds, Azure App Services, and CI agents all need these resolved correctly.
3. **License file mechanics.** ABCpdf uses license keys that historically lived in the registry or in `XSettings`-style installation calls. Cloud-first deployments—especially ephemeral containers—fight with this model.
4. **API verbosity for common tasks.** Generating a PDF from HTML in ABCpdf requires creating a `Doc`, calling `Doc.AddImageHtml()`, sometimes chaining additional pages, then calling `doc.Save()` and `doc.Clear()`. That is a lot of ceremony for "turn this HTML into a file."
5. **Page coordinate system.** ABCpdf uses a bottom-left origin with point-based coordinates. If your team thinks in pixels or CSS units, every measurement needs manual conversion.
6. **Thread safety caveats.** ABCpdf `Doc` objects are not designed to be shared across threads. High-throughput services need careful pooling or per-request instantiation—easy to get wrong under load.
7. **Cross-platform gaps.** Linux support arrived in v13, but some features (XPS, WPF rendering) remain Windows-only. macOS is not supported at all. If your deployment matrix spans operating systems, you carry conditional code.
8. **Font embedding complexity.** Getting CJK or custom web fonts to render correctly often requires manual font installation on the server, plus explicit configuration in ABCpdf's rendering options.
9. **No built-in Razor / template integration.** If your .NET app uses Razor views, you must pre-render them to HTML strings yourself before passing to ABCpdf.
10. **Upgrade friction between major versions.** Namespace and assembly names change between major versions (e.g., `WebSupergoo.ABCpdf12` → `WebSupergoo.ABCpdf13`). Every major upgrade is a mini-migration anyway.

### Comparison Table

| Aspect | ABCpdf | IronPDF |
|---|---|---|
| **Focus** | Full PDF manipulation with multiple HTML engines | HTML-first PDF generation + manipulation |
| **Pricing** | Tiered: Standard $329 (32-bit), Professional $479, Enterprise $4,790, Group $33,530 | Per-developer, perpetual licenses available |
| **API Style** | Imperative `Doc` object, manual page chaining | Fluent renderer, one-call HTML-to-PDF |
| **Learning Curve** | Moderate—coordinate system and chaining take time | Low—HTML/CSS developers productive in minutes |
| **HTML Rendering** | Multiple Chromium versions (Chrome123/117/86/65), Gecko, WebKit, MSHtml | Built-in Chromium rendering engine |
| **Page Indexing** | 1-based | 0-based |
| **Thread Safety** | `Doc` not designed for shared use; manual pooling required | `ChromePdfRenderer` reusable across requests |
| **Namespace** | `WebSupergoo.ABCpdf13` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Complexity | Notes |
|---|---|---|
| HTML → PDF | Low | Direct replacement; IronPDF handles pagination automatically |
| URL → PDF | Low | `RenderUrlAsPdf` replaces `AddImageUrl` |
| Merge | Low | `PdfDocument.Merge()` is a one-liner |
| Split / extract pages | Low | Page range copy via `CopyPages` |
| Watermark | Medium | ABCpdf uses `Doc.AddText` at coordinates; IronPDF uses `ApplyWatermark` with HTML |
| Password / encryption | Low | Property-based in both; IronPDF uses `SecuritySettings` |
| Digital signatures | Medium | Different API surface; consult the IronPDF signing docs |
| Form filling | Medium | ABCpdf uses field atoms; IronPDF uses a `FormField` collection |
| PDF/A compliance | Medium-High | ABCpdf has long-standing PDF/A export; IronPDF supports PDF/A export as well |
| Text extraction | Low | Both offer text extraction; API shape differs |
| Accessibility tagging | High | ABCpdf 13 has sophisticated auto-tagging; evaluate IronPDF coverage against your requirements |

### Decision Matrix

| Scenario | Recommendation |
|---|---|
| HTML-heavy generation, minimal low-level manipulation | Migrate—IronPDF's one-call renderer saves significant code |
| Heavy PDF/A and accessibility tagging workflows | Evaluate carefully—ABCpdf 13's tagging is mature |
| Docker / Linux-first deployment | Migrate—IronPDF's single NuGet, no separate native runtime package |
| Existing large ABCpdf codebase with coordinate-based drawing | Phased migration—wrap new features in IronPDF, legacy in ABCpdf |

---

## Before You Start

### Prerequisites

- .NET 6+ (or .NET Framework 4.6.2+)
- A trial or licensed [IronPDF key](https://ironpdf.com/get-started/license-keys/)
- Access to your CI pipeline configuration

### Find ABCpdf References

Use `rg` (ripgrep) to locate every ABCpdf touchpoint:

```bash
# Find all ABCpdf using statements and references
rg -l "WebSupergoo|ABCpdf|XSettings|Doc\.Add" --glob "*.cs"

# Count occurrences for effort estimation
rg -c "new Doc\(\)" --glob "*.cs"

# Find project file references
rg -l "ABCpdf" --glob "*.csproj"
```

If you don't have `rg`, the `grep` equivalent:

```bash
grep -rl "WebSupergoo\|ABCpdf\|XSettings" --include="*.cs" .
```

In PowerShell, `Get-ChildItem -Recurse -Filter *.cs | Select-String -Pattern "WebSupergoo|ABCpdf"` achieves the same result.

### Swap NuGet Packages

```bash
# Remove ABCpdf
dotnet remove package ABCpdf

# Install IronPDF
dotnet add package IronPdf
```

---

## Quick Start Migration (3 Steps)

### Step 1: License Configuration

**Before (ABCpdf):**

```csharp
using WebSupergoo.ABCpdf13;

class Program
{
    static void Main()
    {
        // ABCpdf license — installed via XSettings or registry
        XSettings.InstallLicense("YOUR-ABCPDF-KEY");
        Console.WriteLine("ABCpdf license installed.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        // IronPDF license — one line, works in containers
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        Console.WriteLine($"IronPDF licensed: {License.IsLicensed}");
    }
}
```

### Step 2: Namespace Imports

**Before:**

```csharp
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;
```

**After:**

```csharp
using IronPdf;
using IronPdf.Editing;    // for watermarks, headers, footers
using IronPdf.Rendering;  // for render options
```

### Step 3: Basic HTML-to-PDF

**Before (ABCpdf — verbose `Doc` lifecycle):**

```csharp
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;
using System;

class Program
{
    static void Main()
    {
        XSettings.InstallLicense("YOUR-ABCPDF-KEY");

        Doc doc = new Doc();
        doc.HtmlOptions.Engine = EngineType.Chrome123;
        doc.Rect.Inset(20, 20);

        doc.AddImageHtml("<h1>Invoice</h1><p>Hello from ABCpdf</p>");
        doc.Save("invoice.pdf");
        doc.Clear();
    }
}
```

**After (IronPDF — leaner):**

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Screen;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;

        var pdf = renderer.RenderHtmlAsPdf("<h1>Invoice</h1><p>Hello from IronPDF</p>");
        pdf.SaveAs("invoice.pdf");
    }
}
```

The manual `Doc.Clear()` and the engine selection step are gone. IronPDF paginates multi-page HTML automatically.

---

## API Mapping Tables

### Namespace Mapping

| ABCpdf Namespace | IronPDF Namespace | Purpose |
|---|---|---|
| `WebSupergoo.ABCpdf13` | `IronPdf` | Core classes |
| `WebSupergoo.ABCpdf13.Objects` | `IronPdf.Editing` | Document manipulation |
| `WebSupergoo.ABCpdf13.Atoms` | `IronPdf.Rendering` | Render options, low-level types |

### Core Class Mapping

| ABCpdf Class | IronPDF Class | Description |
|---|---|---|
| `Doc` | `ChromePdfRenderer` + `PdfDocument` | ABCpdf combines rendering and document in one object; IronPDF separates them |
| `XSettings` | `IronPdf.License` | Licensing configuration |
| `HtmlOptions` / `EngineType` | `ChromePdfRenderOptions` | HTML rendering configuration |
| `Rect` | `RenderingOptions.MarginTop` etc. | Page area / margin control |

### Document Loading Methods

| Operation | ABCpdf | IronPDF |
|---|---|---|
| Load from file | `new Doc(); doc.Read("file.pdf")` | `PdfDocument.FromFile("file.pdf")` |
| Load from bytes | `doc.Read(byteArray)` | `PdfDocument.FromBinaryData(byteArray)` |
| Load from stream | `doc.Read(stream)` | `PdfDocument.FromStream(stream)` |
| Load password-protected | `new Doc("file.pdf", "pass")` | `PdfDocument.FromFile("file.pdf", "pass")` |

### Page Operations

| Operation | ABCpdf | IronPDF |
|---|---|---|
| Get page count | `doc.PageCount` | `pdf.PageCount` |
| Navigate to page | `doc.Page = n` (1-based) | `pdf.Pages[n]` (0-based) |
| Add blank page | `doc.AddPage()` | Render HTML with empty content, or copy from template |
| Delete page | `doc.Delete(pageId)` | `pdf.RemovePages(index)` |

### Merge / Split Operations

| Operation | ABCpdf | IronPDF |
|---|---|---|
| Merge documents | `doc.Append(otherDoc)` | `PdfDocument.Merge(pdf1, pdf2)` |
| Extract page range | Manual page copy loop | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (ABCpdf):**

```csharp
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;
using System;

class HtmlToPdfAbcpdf
{
    static void Main()
    {
        XSettings.InstallLicense("YOUR-ABCPDF-KEY");

        Doc doc = new Doc();
        doc.HtmlOptions.Engine = EngineType.Chrome123;
        doc.HtmlOptions.BrowserWidth = 1280;
        doc.Rect.Inset(30, 30);

        string html = @"
            <html>
            <head><style>
                body { font-family: Arial; }
                .header { background: #2c3e50; color: white; padding: 20px; }
                table { width: 100%; border-collapse: collapse; }
                td, th { border: 1px solid #ddd; padding: 8px; }
            </style></head>
            <body>
                <div class='header'><h1>Monthly Report</h1></div>
                <table>
                    <tr><th>Metric</th><th>Value</th></tr>
                    <tr><td>Revenue</td><td>$142,000</td></tr>
                    <tr><td>Users</td><td>8,421</td></tr>
                </table>
            </body>
            </html>";

        doc.AddImageHtml(html);
        doc.Save("report_abcpdf.pdf");
        doc.Clear();
        Console.WriteLine("Saved with ABCpdf.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class HtmlToPdfIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Screen;
        renderer.RenderingOptions.PrintHtmlBackgrounds = true;
        renderer.RenderingOptions.MarginTop = 30;
        renderer.RenderingOptions.MarginBottom = 30;
        renderer.RenderingOptions.ViewPortWidth = 1280;

        string html = @"
            <html>
            <head><style>
                body { font-family: Arial; }
                .header { background: #2c3e50; color: white; padding: 20px; }
                table { width: 100%; border-collapse: collapse; }
                td, th { border: 1px solid #ddd; padding: 8px; }
            </style></head>
            <body>
                <div class='header'><h1>Monthly Report</h1></div>
                <table>
                    <tr><th>Metric</th><th>Value</th></tr>
                    <tr><td>Revenue</td><td>$142,000</td></tr>
                    <tr><td>Users</td><td>8,421</td></tr>
                </table>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report_ironpdf.pdf");
        Console.WriteLine("Saved with IronPDF.");
    }
}
```

No engine selection. No `Clear()` cleanup. The Chrome engine handles multi-page content automatically. See the [IronPDF HTML-to-PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) for advanced rendering options.

### 2. Merge PDFs

**Before (ABCpdf):**

```csharp
using WebSupergoo.ABCpdf13;
using System;

class MergeAbcpdf
{
    static void Main()
    {
        XSettings.InstallLicense("YOUR-ABCPDF-KEY");

        Doc doc1 = new Doc();
        doc1.Read("part1.pdf");

        Doc doc2 = new Doc();
        doc2.Read("part2.pdf");

        // Append doc2 into doc1
        doc1.Append(doc2);

        doc1.Save("merged_abcpdf.pdf");
        doc1.Clear();
        doc2.Clear();
        Console.WriteLine("Merged with ABCpdf.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class MergeIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf1 = PdfDocument.FromFile("part1.pdf");
        var pdf2 = PdfDocument.FromFile("part2.pdf");

        // Static merge — returns a new PdfDocument
        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged_ironpdf.pdf");
        Console.WriteLine("Merged with IronPDF.");
    }
}
```

`PdfDocument.Merge()` accepts `params PdfDocument[]`, so you can pass any number of documents in one call. See the [IronPDF merge documentation](https://ironpdf.com/examples/merge-pdfs/) for additional patterns.

### 3. Watermark

**Before (ABCpdf):**

```csharp
using WebSupergoo.ABCpdf13;
using System;

class WatermarkAbcpdf
{
    static void Main()
    {
        XSettings.InstallLicense("YOUR-ABCPDF-KEY");

        Doc doc = new Doc();
        doc.Read("input.pdf");

        // Apply text watermark to each page
        for (int i = 1; i <= doc.PageCount; i++)
        {
            doc.Page = i;
            doc.Color.String = "200 200 200";       // light gray
            doc.Font = doc.AddFont("Helvetica");
            doc.FontSize = 48;
            doc.Rect.SetRect(100, 300, 400, 100);
            doc.AddText("CONFIDENTIAL");
        }

        doc.Save("watermarked_abcpdf.pdf");
        doc.Clear();
        Console.WriteLine("Watermarked with ABCpdf.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class WatermarkIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        // HTML-based watermark — use any CSS you want
        pdf.ApplyWatermark(
            "<h1 style='color:lightgray;font-size:48px;opacity:0.5;'>CONFIDENTIAL</h1>",
            rotation: 45,
            hyperlink: null
        );

        pdf.SaveAs("watermarked_ironpdf.pdf");
        Console.WriteLine("Watermarked with IronPDF.");
    }
}
```

The coordinate math and per-page loop collapse into a single `ApplyWatermark` call using HTML + CSS. See the [IronPDF watermark documentation](https://ironpdf.com/how-to/watermark/) for alignment options via `VerticalAlignment` and `HorizontalAlignment` enums in `IronPdf.Editing`.

### 4. Password Protection

**Before (ABCpdf):**

```csharp
using WebSupergoo.ABCpdf13;
using System;

class PasswordAbcpdf
{
    static void Main()
    {
        XSettings.InstallLicense("YOUR-ABCPDF-KEY");

        Doc doc = new Doc();
        doc.Read("input.pdf");

        // Configure encryption and passwords
        doc.Encryption.Type = EncryptionType.AES128;
        doc.Encryption.Password = "user456";
        doc.Encryption.CanPrint = false;
        doc.Encryption.CanCopy = false;
        doc.Encryption.CanEdit = false;

        doc.Save("protected_abcpdf.pdf");
        doc.Clear();
        Console.WriteLine("Protected with ABCpdf.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class PasswordIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        // Security settings — named properties, no magic numbers
        pdf.SecuritySettings.OwnerPassword = "owner123";
        pdf.SecuritySettings.UserPassword = "user456";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.NoPrint;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;
        pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;

        pdf.SaveAs("protected_ironpdf.pdf");
        Console.WriteLine("Protected with IronPDF.");
    }
}
```

IronPDF uses typed enums instead of integer flags. See the [IronPDF security documentation](https://ironpdf.com/how-to/pdf-permissions-passwords/) for the full `SecuritySettings` surface.

---

## Critical Migration Notes

### Page Indexing Differences

ABCpdf uses 1-based page numbering (`doc.Page = 1` is the first page). IronPDF uses 0-based indexing (`pdf.Pages[0]` is the first page). Every loop, every page reference, every index needs adjusting. A quick `rg "doc\.Page\s*=" --glob "*.cs"` will find them all.

### Doc Object vs. Separate Renderer + Document

ABCpdf's `Doc` is both the renderer and the document. IronPDF separates these concerns: `ChromePdfRenderer` handles creation, `PdfDocument` handles the result. If your ABCpdf code passes `doc` around for both rendering and manipulation, you will need to refactor call sites to accept the appropriate type.

### Unit Conversion

ABCpdf works in PDF points (1 point = 1/72 inch) with a bottom-left origin. IronPDF margins are specified in millimeters by default. If you have hard-coded coordinate values, convert them: 1 point ≈ 0.353 mm.

### Disposal Patterns

ABCpdf historically required an explicit `doc.Clear()` to release resources. IronPDF's `PdfDocument` is `IDisposable`, so a standard `using` block handles cleanup—drop the `Clear()` call and wrap with `using` when migrating each site.

---

## Performance Considerations

### Renderer Reuse

In ABCpdf, you create a new `Doc` per render. In IronPDF, `ChromePdfRenderer` is lightweight and intended for reuse—create one instance and reuse it across requests:

```csharp
// Good — reuse the renderer
private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

public PdfDocument GeneratePdf(string html)
{
    return _renderer.RenderHtmlAsPdf(html);
}
```

### Disposal

Always dispose `PdfDocument` instances when done. In high-throughput scenarios, this matters:

```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// pdf is disposed here
```

### Edge Cases Worth Flagging

- **Large HTML tables (1000+ rows):** Both libraries handle these, but consider paginating in HTML with `page-break-before: always` for predictable output.
- **JavaScript-dependent rendering:** IronPDF's Chrome engine executes JS by default. If your HTML relies on JS to build the DOM, leave `renderer.RenderingOptions.EnableJavaScript = true` and consider adding a `RenderDelay` if content loads asynchronously.
- **Concurrent rendering:** IronPDF's `ChromePdfRenderer` is designed for reuse across requests. For batch jobs, `Parallel.ForEach` with the same renderer instance works. ABCpdf typically required explicit `Doc`-per-thread isolation.

---

## Migration Checklist

### Pre-Migration (8 items)

- [ ] Run `rg` scan to inventory all ABCpdf usage across the codebase
- [ ] Catalog which ABCpdf features you actually use (HTML-to-PDF, merge, forms, etc.)
- [ ] Obtain an IronPDF trial key from [ironpdf.com/get-started/license-keys/](https://ironpdf.com/get-started/license-keys/)
- [ ] Create a migration branch in source control
- [ ] Identify integration tests that exercise PDF generation paths
- [ ] Document any ABCpdf coordinate-based drawing code that needs rethinking
- [ ] Check for ABCpdf license files or registry entries in deployment scripts
- [ ] Review Docker / CI configurations for native runtime package copy steps

### Code Migration (10 items)

- [ ] Replace NuGet package: remove `ABCpdf`, add `IronPdf`
- [ ] Update all `using` directives from `WebSupergoo.ABCpdf*` to `IronPdf`
- [ ] Replace `XSettings.InstallLicense()` with `IronPdf.License.LicenseKey = "..."`
- [ ] Replace `Doc` + `AddImageHtml` with `ChromePdfRenderer.RenderHtmlAsPdf()`
- [ ] Replace `Doc.Append()` with `PdfDocument.Merge()`
- [ ] Replace coordinate-based watermarks with `ApplyWatermark()` HTML
- [ ] Replace encryption integer flags with `SecuritySettings` properties
- [ ] Convert all 1-based page indices to 0-based
- [ ] Convert point-based margins to millimeters
- [ ] Replace `doc.Clear()` calls with `using` blocks

### Testing (7 items)

- [ ] Run existing PDF output tests and compare visually
- [ ] Verify multi-page HTML renders without manual chaining
- [ ] Test password-protected PDF open/save round-trip
- [ ] Test merge with 3+ documents
- [ ] Validate watermark positioning on single-page and multi-page docs
- [ ] Load-test concurrent rendering in staging
- [ ] Verify Docker/Linux deployment produces identical output

### Post-Migration (4 items)

- [ ] Remove ABCpdf native runtime packages from deployment artifacts
- [ ] Remove ABCpdf license registry entries or file deployment steps
- [ ] Update project documentation and onboarding guides
- [ ] Monitor production error rates for PDF-related exceptions for two weeks

---

## Conclusion

The hardest part of this migration isn't the API mapping—it is the coordinate-system shift and the engine-version cleanup. If your codebase is mostly HTML-to-PDF with straightforward merge and security, expect a one-to-two day migration for a mid-size project. If you have heavy coordinate-based drawing or accessibility tagging, plan for a phased approach.

**Worth knowing even without IronPDF:** pinning an HTML engine to a specific Chromium major (ABCpdf's `Chrome117`, `Chrome86`, etc.) is a common source of "renders fine in the browser, renders wrong in the PDF" bugs. Any modern HTML-to-PDF library that tracks a single, current Chromium internally—IronPDF, Puppeteer Sharp, Playwright—eliminates that class of bug entirely.

Have you hit an edge case migrating from ABCpdf that isn't covered here—maybe around ABCpdf's `ContentStreamScanner`, custom atom manipulation, or 3D PDF rendering? Drop it in the comments. Those are the patterns that are hardest to Google and most useful to document.
