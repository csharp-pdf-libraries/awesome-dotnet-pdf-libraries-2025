---
title: "Migrating from DinkToPdf to IronPDF: what actually changes in your code"
published: false
tags: dotnet, csharp, pdf, migration
---

Here's the situation many teams are in: DinkToPdf works, has always worked, and the codebase depends on it. Then someone looks at the wkhtmltopdf upstream status page and realizes the underlying native library stopped active development in 2023. The NuGet package hasn't had a meaningful release in years. The native `.so` binary for your Linux distro requires workarounds to deploy. And when the CSS on a new template doesn't render quite right, there's nowhere to file a bug.

This is the migration that's overdue rather than urgent. DinkToPdf isn't broken today — it'll break when your OS drops support for an old library, or when a new CSS feature makes it into your templates, or when you upgrade to a .NET version that breaks something in the P/Invoke chain.

This article gives you the checklist and code to move from DinkToPdf to **IronPDF** before the forced upgrade happens on someone else's timeline. Both are HTML-to-PDF tools, which makes the migration more of a renderer swap than an architectural change.

---

## Why Migrate (Without Drama)

Teams using DinkToPdf should be aware of these neutral migration triggers:

1. **wkhtmltopdf upstream deprecation** — The native library DinkToPdf wraps was archived upstream in January 2023, and the entire wkhtmltopdf GitHub organization was marked archived by an administrator in July 2024. Security issues will not receive patches from upstream.
2. **Native binary deployment friction** — `libwkhtmltopdf` must be bundled alongside your app. Docker images require explicit `COPY` or installation. This is a common source of "works locally, fails in CI" issues.
3. **32-bit / 64-bit binary mismatch** — DinkToPdf requires matching native binary architecture. Projects that switch between `x86` and `x64` build configs hit this.
4. **CSS rendering limitations** — wkhtmltopdf uses an older WebKit version. Modern CSS (Flexbox, CSS Grid, CSS variables) may not render correctly. Teams with modern frontend templates hit this regularly.
5. **JavaScript support** — wkhtmltopdf has limited and sometimes unreliable JavaScript execution. Pages that rely on JS for layout will render incorrectly.
6. **ARM64 / Apple Silicon** — wkhtmltopdf native binary availability for ARM64 Linux and macOS is limited. Teams moving to ARM-based cloud instances or local dev on Apple Silicon hit deployment issues.
7. **DinkToPdf package maintenance** — The .NET wrapper hasn't had significant releases in years. Issues accumulate without resolution.
8. **Newer .NET versions** — .NET 7/8 introduced changes that some P/Invoke-based wrappers struggled with. Confirm compatibility against your target framework before upgrading.
9. **Alpine Linux** — `libwkhtmltopdf` doesn't run on Alpine by default due to musl libc. Teams using Alpine Docker images need glibc shims. IronPDF also has Linux dependencies (see the [Linux setup docs](https://ironpdf.com/how-to/linux/)) but is designed for current distributions.
10. **Header/footer JavaScript variables** — DinkToPdf/wkhtmltopdf has special header/footer JS variables (`page`, `topage`). Migrating these requires understanding IronPDF's equivalent approach.

### Comparison Table

| Aspect | DinkToPdf | IronPDF |
|---|---|---|
| Focus | HTML-to-PDF (WebKit via wkhtmltopdf) | HTML-to-PDF (Chromium) |
| Pricing | Free / open source | Per-developer or royalty-free |
| API Style | Document object model passed to converter | High-level renderer + options object |
| Learning Curve | Low if you know wkhtmltopdf settings | Similar learning curve |
| HTML Rendering | WebKit (older, deprecated upstream) | Chromium-based |
| Page Indexing | Via wkhtmltopdf header/footer placeholders (`[page]`, `[toPage]`) | Via header/footer API tokens (`{page}`, `{total-pages}`) |
| Thread Safety | `SynchronizedConverter` serializes calls; can still hit native crashes under load | Thread-safe by design; renderer reusable across threads |
| Namespace | `DinkToPdf` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Effort | Notes |
|---|---|---|
| Basic HTML string to PDF | Low | Near-direct API swap |
| HTML file to PDF | Low | Path handling differs slightly |
| Custom headers/footers | Medium | wkhtmltopdf header vars → IronPDF header API |
| Page margins and paper size | Low | Both have equivalent settings |
| Merge PDFs | Medium | DinkToPdf doesn't merge — adds separate dependency |
| Watermark | Medium | Requires stamper model in IronPDF |
| Password protection | Low | Simple property swap |
| CSS rendering parity | Medium-High | Test all templates — Chromium renders differently from WebKit |
| JavaScript execution | Low (improvement) | Chromium JS support is more reliable than wkhtmltopdf |
| Custom fonts | Medium | Font loading approach differs by deployment OS; test on your target |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| wkhtmltopdf security risk is a compliance concern | Migrate; deprecated upstream means no security patches |
| Modern CSS templates (Flexbox, Grid) not rendering | Chromium renderer resolves most of these issues |
| ARM64 deployment or Apple Silicon dev machines | Migrate; wkhtmltopdf ARM64 support is limited |
| All templates simple and stable, no ARM requirement | Can delay, but plan for eventually-forced migration |

---

## Pre-Migration Checklist

### Inventory DinkToPdf Usage
- [ ] Find all DinkToPdf references: `rg "using DinkToPdf" --type cs -l`
- [ ] Find all `HtmlToPdfDocument` usages: `rg "HtmlToPdfDocument\|IConverter\|SynchronizedConverter" --type cs`
- [ ] Find all `ObjectSettings` configurations: `rg "ObjectSettings\|GlobalSettings\|HtmlContent" --type cs`
- [ ] Find all header/footer configurations: `rg "HeaderSettings\|FooterSettings" --type cs`
- [ ] Find all `PechkinPaperKind` or `PaperKind` references (paper size settings): `rg "PaperKind\|PechkinPaper" --type cs`
- [ ] Count total occurrences to estimate migration effort

### Confirm wkhtmltopdf Status
- [ ] Visit https://wkhtmltopdf.org/status.html — confirm current maintenance status
- [ ] Check your deployment OS for native binary availability
- [ ] Check your Docker base image for `libwkhtmltopdf` compatibility
- [ ] Document any known CSS rendering issues your team has already worked around

### Environment Check
- [ ] Confirm .NET version: `dotnet --version`
- [ ] Confirm deployment OS (Windows/Linux distro/Alpine?)
- [ ] Confirm CPU architecture (`x64`? `arm64`?)
- [ ] Capture baseline PDF outputs (screenshot each template) before migration

### Set Up IronPDF Side-by-Side
- [ ] Add IronPDF without removing DinkToPdf: `dotnet add package IronPdf`
- [ ] Get IronPDF license key ([license setup](https://ironpdf.com/how-to/license-keys/))
- [ ] Generate a test PDF with IronPDF against your most complex template
- [ ] Compare IronPDF output to DinkToPdf output before committing to migrate

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

**Before (DinkToPdf — no license required):**

```csharp
// DinkToPdf registration in ASP.NET Core DI
// No license setup
services.AddSingleton(typeof(IConverter),
    new SynchronizedConverter(new PdfTools()));
```

**After (IronPDF):**

```csharp
using IronPdf;

// In Program.cs / Startup.cs — before any IronPDF call
// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = builder.Configuration["IronPdf:LicenseKey"];

// Optional: register ChromePdfRenderer as singleton for DI
builder.Services.AddSingleton<ChromePdfRenderer>();
```

### Step 2 — Namespace Imports

**Before:**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
```

**After:**
```csharp
using IronPdf;
```

### Step 3 — Basic HTML-to-PDF

**Before (DinkToPdf):**

```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

// Injected via DI or created directly
IConverter converter = new SynchronizedConverter(new PdfTools());

var doc = new HtmlToPdfDocument()
{
    GlobalSettings = { PaperSize = PaperKind.A4, Orientation = Orientation.Portrait },
    Objects = { new ObjectSettings { HtmlContent = "<h1>Hello</h1>" } }
};

byte[] pdfBytes = converter.Convert(doc);
File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("output.pdf");
```

---

## API Mapping Tables

### Namespace Mapping

| DinkToPdf | IronPDF | Notes |
|---|---|---|
| `DinkToPdf` | `IronPdf` | Core namespace |
| `DinkToPdf.Contracts` | Not needed | Interface abstraction not required |
| P/Invoke to `libwkhtmltopdf` | Internal Chromium process | No native binary to deploy manually |

### Core Class Mapping

| DinkToPdf Class | IronPDF Class | Description |
|---|---|---|
| `IConverter` | `ChromePdfRenderer` | Main rendering interface |
| `SynchronizedConverter` | `ChromePdfRenderer` | Thread-safe renderer (IronPDF handles internally) |
| `HtmlToPdfDocument` | `ChromePdfRenderOptions` | Document/render configuration |
| `ObjectSettings` | `ChromePdfRenderOptions` | Per-object (page) settings |

### Document Loading Methods

| Operation | DinkToPdf | IronPDF |
|---|---|---|
| HTML string | `ObjectSettings { HtmlContent = html }` | `renderer.RenderHtmlAsPdf(html)` |
| HTML file | `ObjectSettings { Page = "file:///path" }` | `renderer.RenderHtmlFileAsPdf(path)` |
| URL | `ObjectSettings { Page = "https://..." }` | `renderer.RenderUrlAsPdf(url)` |
| From stream | Not natively | `renderer.RenderHtmlAsPdf(html)` with string |

### Page/Render Options

| Setting | DinkToPdf (GlobalSettings / ObjectSettings) | IronPDF (ChromePdfRenderOptions) |
|---|---|---|
| Paper size | `GlobalSettings.PaperSize = PaperKind.A4` | `options.PaperSize = IronPdf.Rendering.PdfPaperSize.A4` |
| Orientation | `GlobalSettings.Orientation = Orientation.Portrait` | `options.PaperOrientation = PdfPaperOrientation.Portrait` |
| Top margin | `GlobalSettings.Margins.Top = "10mm"` | `options.MarginTop = 10` |
| DPI | `GlobalSettings.DPI = 96` | Chromium uses a fixed device DPI; scale via CSS rather than a DPI setting |
| Enable JS | `ObjectSettings.WebSettings.EnableJavascript = true` | `options.EnableJavaScript = true` (enabled by default) |

### Merge/Split

| Operation | DinkToPdf | IronPDF |
|---|---|---|
| Merge PDFs | Not built-in (need separate lib) | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | Not built-in | `pdf.CopyPage(index)` / `pdf.CopyPages(start, end)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (DinkToPdf):**

```csharp
using System;
using System.IO;
using DinkToPdf;
using DinkToPdf.Contracts;

class Program
{
    static void Main()
    {
        // SynchronizedConverter wraps native libwkhtmltopdf calls
        // Note: libwkhtmltopdf binary must exist alongside the .exe / .dll
        IConverter converter = new SynchronizedConverter(new PdfTools());

        string html = @"<html>
            <head>
                <style>body { font-family: Arial; padding: 40px; }</style>
            </head>
            <body>
                <h1>Invoice #4200</h1>
                <p>Customer: Bob Jones</p>
                <p>Amount: $2,500.00</p>
                <p>Due: 2025-12-01</p>
            </body>
        </html>";

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings
                {
                    Top = 10, Bottom = 10, Left = 10, Right = 10
                },
                Out = "invoice.pdf"
            },
            Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = html,
                    WebSettings = new WebSettings
                    {
                        DefaultEncoding = "utf-8",
                        EnableJavascript = false
                    }
                }
            }
        };

        converter.Convert(doc);
        Console.WriteLine("Saved invoice.pdf via DinkToPdf");
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    // Reuse renderer across calls
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer
    {
        RenderingOptions = new ChromePdfRenderOptions
        {
            PaperSize = IronPdf.Rendering.PdfPaperSize.A4,
            MarginTop = 10,
            MarginBottom = 10,
            MarginLeft = 10,
            MarginRight = 10
        }
    };

    static void Main()
    {
        // https://ironpdf.com/how-to/license-keys/
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        string html = @"<html>
            <head><style>body { font-family: Arial; padding: 40px; }</style></head>
            <body>
                <h1>Invoice #4200</h1>
                <p>Customer: Bob Jones</p>
                <p>Amount: $2,500.00</p>
                <p>Due: 2025-12-01</p>
            </body>
        </html>";

        using var pdf = _renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("invoice.pdf");
        Console.WriteLine("Saved invoice.pdf via IronPDF");
    }
}
```

### 2. Merge PDFs

**Before (DinkToPdf — no native merge; separate tool needed):**

```csharp
using System;
using System.IO;
using DinkToPdf;
using DinkToPdf.Contracts;

// DinkToPdf doesn't merge PDFs — teams typically use PdfSharp or iTextSharp
// This shows the pattern of generating then merging with a separate library
class Program
{
    static void Main()
    {
        IConverter converter = new SynchronizedConverter(new PdfTools());

        // Generate PDFs separately
        var doc1 = CreateDoc("<h1>Section 1</h1>", "part1.pdf");
        var doc2 = CreateDoc("<h1>Section 2</h1>", "part2.pdf");

        converter.Convert(doc1);
        converter.Convert(doc2);

        // Then merge using PdfSharp or another library (two dependencies)
        Console.WriteLine("Merge requires a separate library with DinkToPdf");
    }

    static HtmlToPdfDocument CreateDoc(string html, string outputPath) =>
        new HtmlToPdfDocument
        {
            GlobalSettings = new GlobalSettings { Out = outputPath },
            Objects = { new ObjectSettings { HtmlContent = html } }
        };
}
```

**After (IronPDF — single library):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Generate and merge in one library — no second dependency
        // https://ironpdf.com/how-to/merge-or-split-pdfs/
        using var pdf1 = renderer.RenderHtmlAsPdf("<h1>Section 1</h1>");
        using var pdf2 = renderer.RenderHtmlAsPdf("<h1>Section 2</h1>");

        using var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
        Console.WriteLine($"Merged {merged.PageCount} pages to merged.pdf");
    }
}
```

### 3. Headers and Footers with Page Numbers

**Before (DinkToPdf):**

```csharp
using System;
using DinkToPdf;
using DinkToPdf.Contracts;

class Program
{
    static void Main()
    {
        IConverter converter = new SynchronizedConverter(new PdfTools());

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = new GlobalSettings
            {
                PaperSize = PaperKind.A4,
                Orientation = Orientation.Portrait
            },
            Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = "<h1>Report</h1><p>Content here.</p>",
                    HeaderSettings = new HeaderSettings
                    {
                        FontSize = 9,
                        Right = "Page [page] of [toPage]", // wkhtmltopdf JS vars
                        Line = true,
                        Spacing = 2.812
                    },
                    FooterSettings = new FooterSettings
                    {
                        FontSize = 9,
                        Center = "Company Confidential",
                        Line = true
                    }
                }
            }
        };

        converter.Convert(doc);
        Console.WriteLine("Report with headers/footers generated");
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

        var renderer = new ChromePdfRenderer();

        // IronPDF headers/footers — https://ironpdf.com/how-to/headers-and-footers/
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:right;font-size:9pt'>Page {page} of {total-pages}</div>",
            DrawDividerLine = true
        };

        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;font-size:9pt'>Company Confidential</div>",
            DrawDividerLine = true
        };

        using var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1><p>Content here.</p>");
        pdf.SaveAs("report.pdf");
        Console.WriteLine("Report with headers/footers generated");
    }
}
```

### 4. Password Protection

**Before (DinkToPdf — not natively supported):**

```csharp
using System;
using System.IO;
using DinkToPdf;
using DinkToPdf.Contracts;

// DinkToPdf doesn't support password protection
// Teams encrypt the output PDF bytes using a separate library
class Program
{
    static void Main()
    {
        IConverter converter = new SynchronizedConverter(new PdfTools());

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = new GlobalSettings { Out = "output.pdf" },
            Objects = { new ObjectSettings { HtmlContent = "<h1>Confidential</h1>" } }
        };

        converter.Convert(doc);
        byte[] pdfBytes = File.ReadAllBytes("output.pdf");

        // Encrypt using PdfSharp or iTextSharp — another dependency
        // [separate encryption code here]
        Console.WriteLine("Encryption requires a separate library with DinkToPdf");
    }
}
```

**After (IronPDF — single library):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        using var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential</h1>");

        // Encrypt in same step — no separate library
        // https://ironpdf.com/how-to/pdf-permissions-passwords/
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting =
            IronPdf.Security.PdfPrintSecurity.FullPrintRights;

        pdf.SaveAs("protected.pdf");
        Console.WriteLine("Password protected PDF saved");
    }
}
```

---

## Code Migration Checklist

- [ ] Add `IronPdf` NuGet: `dotnet add package IronPdf`
- [ ] Set license key in config and `Program.cs`
- [ ] Register `ChromePdfRenderer` as singleton in DI container
- [ ] Replace `IConverter`/`SynchronizedConverter` with `ChromePdfRenderer` injection
- [ ] Replace `HtmlToPdfDocument` + `ObjectSettings` with `renderer.RenderHtmlAsPdf(html)`
- [ ] Migrate `GlobalSettings.PaperSize` → `RenderingOptions.PaperSize`
- [ ] Migrate `GlobalSettings.Margins` → `RenderingOptions.Margin*` properties
- [ ] Migrate `GlobalSettings.Orientation` → `RenderingOptions.PaperOrientation`
- [ ] Migrate `HeaderSettings` → `RenderingOptions.HtmlHeader`
- [ ] Migrate `FooterSettings` → `RenderingOptions.HtmlFooter`
- [ ] Replace wkhtmltopdf header variables (`[page]`, `[toPage]`) → IronPDF `{page}`, `{total-pages}` ([headers docs](https://ironpdf.com/how-to/headers-and-footers/))
- [ ] Remove `DinkToPdf` NuGet package
- [ ] Remove `libwkhtmltopdf` native binary from build/deploy artifacts
- [ ] Remove `CustomAssemblyLoadContext` setup (if used for DinkToPdf's assembly loading workaround)
- [ ] Replace any separate merge library with `PdfDocument.Merge()`
- [ ] Replace any separate encryption library with `SecuritySettings` properties

---

## Critical Migration Notes

### Header/Footer Variable Syntax

DinkToPdf uses wkhtmltopdf's special JavaScript variables in header/footer strings:

```
[page]     → current page number
[toPage]   → total page count
[sitepage] → page number within section
```

IronPDF uses different tokens:

```
{page}          → current page
{total-pages}   → total page count
```

Audit all header/footer strings in your codebase:

```bash
rg "\[page\]\|\[toPage\]\|topage\|HeaderSettings\|FooterSettings" --type cs
```

### CustomAssemblyLoadContext Workaround

Many DinkToPdf implementations include a `CustomAssemblyLoadContext` class to force loading of the correct native binary from the application directory. This is a workaround for a DinkToPdf deployment quirk:

```csharp
// Common DinkToPdf setup in Program.cs — remove after migration
var context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(),
    "libwkhtmltopdf.dll")); // or .so on Linux
```

Remove this entirely after migration — IronPDF handles its own native dependencies.

### CSS Rendering Differences

Chromium renders differently from the older WebKit used in wkhtmltopdf. Common differences:

- **Flexbox** — IronPDF/Chromium renders Flexbox correctly; wkhtmltopdf often doesn't.
- **CSS Grid** — Same — Chromium handles it; wkhtmltopdf may not.
- **Font rendering** — Subpixel hinting differs. Test in your target output format.
- **Page break behavior** — `page-break-before`, `page-break-after`, and `page-break-inside` CSS rules may behave differently. Test multi-page documents.

This means: most CSS issues you worked around in DinkToPdf resolve automatically in IronPDF — but CSS that coincidentally worked in wkhtmltopdf may need revisiting.

### Page Indexing

IronPDF uses 0-based page indexing. DinkToPdf doesn't have a page access API (output is bytes only). This only applies if you load and manipulate the output PDF after generation.

---

## Performance Considerations

### No More libwkhtmltopdf Bottleneck

wkhtmltopdf is single-threaded in some configurations. DinkToPdf's `SynchronizedConverter` serializes concurrent PDF generation requests — only one PDF renders at a time. IronPDF's Chromium renderer supports concurrent rendering:

```csharp
// DinkToPdf: requests queue behind SynchronizedConverter
// IronPDF: parallel rendering possible
// https://ironpdf.com/examples/parallel/

var tasks = documents.Select(html => Task.Run(() =>
{
    using var pdf = _renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}));
var results = await Task.WhenAll(tasks);
```

Test concurrent throughput against your DinkToPdf baseline — parallel rendering can significantly reduce wall-clock time for batch jobs.

### Memory Profile

The Chromium renderer has a different memory footprint from wkhtmltopdf. Under sustained concurrent load, measure heap and native memory consumption. Size your pod/instance memory accordingly.

### Startup Latency

First render after creating `ChromePdfRenderer` initializes the Chromium process. Register as a singleton in ASP.NET Core to amortize this:

```csharp
// Program.cs
builder.Services.AddSingleton<ChromePdfRenderer>(sp =>
{
    return new ChromePdfRenderer
    {
        RenderingOptions = new ChromePdfRenderOptions
        {
            PaperSize = IronPdf.Rendering.PdfPaperSize.A4,
            MarginTop = 10,
            MarginBottom = 10
        }
    };
});
```

---

## Testing Checklist

- [ ] Visual comparison: IronPDF output vs DinkToPdf output for each template
- [ ] Multi-page documents: verify page count and content distribution
- [ ] Headers/footers: page number variables render correctly (check `{page}` syntax)
- [ ] CSS rendering: test Flexbox, Grid, and any complex layout in templates
- [ ] Custom fonts: verify fonts load and render correctly on your deployment OS
- [ ] Page size and margins: measure output dimensions against expected values
- [ ] JavaScript-dependent pages: verify Chromium renders correctly (usually better than wkhtmltopdf)
- [ ] Merged PDFs (if using PdfDocument.Merge): verify page order and count
- [ ] Password-protected PDFs: open with user password; confirm owner restrictions
- [ ] Test on deployment OS — Linux requires system libraries for IronPDF
- [ ] Load test: concurrent rendering throughput vs DinkToPdf baseline

---

## Post-Migration Checklist

- [ ] Remove `DinkToPdf` NuGet from all project files: `dotnet remove package DinkToPdf`
- [ ] Delete `libwkhtmltopdf.dll` / `libwkhtmltopdf.so` from repository and build output
- [ ] Remove `CustomAssemblyLoadContext` class from codebase
- [ ] Remove wkhtmltopdf binary copy steps from Dockerfile(s)
- [ ] Update Docker images with IronPDF native library dependencies — check [IronPDF Azure docs](https://ironpdf.com/how-to/azure/) for current dependency list
- [ ] Update CI/CD pipeline — remove any wkhtmltopdf download/install steps
- [ ] Update application documentation — note renderer change (CSS behavior may differ)
- [ ] Monitor production PDF output for the first week — watch for CSS regressions

---

## Wrapping Up

DinkToPdf-to-IronPDF is probably the most like-for-like migration in this batch — both are HTML-to-PDF tools, both are NuGet-packaged, both work in ASP.NET Core. The main variables are CSS rendering differences between WebKit and Chromium (usually an improvement), the header/footer variable syntax change, and the native binary deployment model swap.

The one thing that trips up the most teams: the `CustomAssemblyLoadContext` boilerplate that accumulated in `Program.cs` over the years. Find it, delete it, don't feel nostalgic about it.

**Question for the comments:** If you're still running DinkToPdf in production — what's the main thing keeping you there? Is it the wkhtmltopdf CSS rendering that your templates depend on, the `SynchronizedConverter` simplicity, or something else? Curious what the actual blockers look like.
