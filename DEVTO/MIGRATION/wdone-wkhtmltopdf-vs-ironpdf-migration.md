# Migrating from wkhtmltopdf to IronPDF: no fuss, no fluff

The on-premises server works. The Dockerfile doesn't. wkhtmltopdf requires a native binary on the system path, font packages, and a display server (or `xvfb-run` on headless Linux) to handle its X11 dependency. What installs cleanly on a configured Ubuntu server fails silently in a minimal Docker image because the font paths are wrong, libXrender isn't present, or the binary simply isn't there. Every engineer who has added `wkhtmltopdf` to a Dockerfile has encountered this — usually after a deployment that appeared to succeed.

This article covers migrating from wkhtmltopdf (or DinkToPdf, which wraps the same binary) to IronPDF. You'll have a comprehensive checklist, working before/after code, and the diagnostic commands to clear out every trace of wkhtmltopdf from your pipeline.

---

## Why Migrate (Without Drama)

Teams migrating from wkhtmltopdf to IronPDF commonly encounter:

1. **Project archived** — wkhtmltopdf is no longer maintained as of 2023; no security patches, no .NET 8+ compatibility updates, no bug fixes.
2. **Docker/container complexity** — requires native binary, fonts, and X11 libraries in the container image; minimal images like `mcr.microsoft.com/dotnet/aspnet` don't include these.
3. **Qt WebKit rendering age** — the underlying rendering engine is pre-2016 WebKit; modern CSS (flex, grid, custom properties, CSS variables) doesn't render correctly.
4. **Process invocation overhead** — each PDF generation spawns a new process (or uses DinkToPdf's managed wrapper which still loads a native library).
5. **Thread safety** — the native `libwkhtmltox` has documented single-threaded constraints; concurrent generation requires worker pools or queues.
6. **JavaScript rendering limitations** — wkhtmltopdf executes JavaScript but with an older engine; `--javascript-delay` is a crude timing workaround for JS-rendered content.
7. **Binary version management** — the wkhtmltopdf binary must be present at a known path; version pinning in Docker images adds maintenance.
8. **No PDF manipulation** — wkhtmltopdf generates PDFs; merge, watermark, security, and text extraction require secondary libraries.
9. **Deployment artifact size** — the bundled DinkToPdf approach adds ~14MB of native libraries to the deployment.
10. **DinkToPdf wrapper also unmaintained** — the most common .NET wrapper (DinkToPdf) has low recent activity; verify at github.com/rdvojmoc/DinkToPdf.

### Comparison Table

| Aspect | wkhtmltopdf / DinkToPdf | IronPDF |
|---|---|---|
| Focus | HTML-to-PDF via native binary | HTML-to-PDF + PDF manipulation |
| Pricing | Open source (LGPL) — archived | Commercial — verify at ironsoftware.com |
| API Style | CLI flags / DinkToPdf managed wrapper | `ChromePdfRenderer` + options object |
| Learning Curve | Low for CLI; DinkToPdf adds wrapper layer | Low for .NET devs |
| HTML Rendering | Qt WebKit (pre-2016) — archived | Embedded Chromium |
| Page Indexing | Not exposed via standard output | 0-based |
| Thread Safety | Native library is single-threaded | Verify IronPDF concurrent instance guidance |
| Namespace | `DinkToPdf` (wrapper) | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | wkhtmltopdf / DinkToPdf | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | `--` CLI / `HtmlToPdfDocument` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | wkhtmltopdf URL arg | `renderer.RenderUrlAsPdfAsync()` | Low |
| HTML file to PDF | wkhtmltopdf file arg | `renderer.RenderHtmlFileAsPdfAsync()` | Low |
| Save to bytes | Via stdout / `SynchronizedConverter` | `pdf.BinaryData` | Low |
| Save to file | wkhtmltopdf output arg | `pdf.SaveAs(path)` | Low |
| Page size | `--page-size A4` / `GlobalSettings` | `RenderingOptions.PaperSize` | Low |
| Margins | `--margin-top 20mm` / `ObjectSettings` | `RenderingOptions.Margin*` | Low |
| Headers/footers | `--header-html` / `HeaderSettings` | `RenderingOptions.HtmlHeader/Footer` | Medium |
| JS wait | `--javascript-delay 500` | `RenderingOptions.WaitFor.*` — verify | Medium |
| Merge PDFs | Not native | `PdfDocument.Merge()` | Medium |
| Watermark | Not native | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Not native | `pdf.SecuritySettings` | Low |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Docker/Linux deployment fails on wkhtmltopdf deps | Switch — eliminates native binary dependency entirely |
| Archived/unmaintained status is the concern | Switch — IronPDF is actively maintained |
| Modern CSS (flex, grid) rendering needed | Switch — Chromium vs pre-2016 WebKit |
| wkhtmltopdf working fine, no deployment friction | Evaluate migration cost vs timing of eventual forced migration |

---

## Pre-Migration Codebase Audit Checklist

Run these commands before touching any code.

### Find All wkhtmltopdf References

```bash
# 1. Find Process.Start invocations of wkhtmltopdf
rg -l "wkhtmltopdf\b" --type cs
rg "wkhtmltopdf\|wkhtmltox" --type cs -n

# 2. Find DinkToPdf wrapper usage
rg -l "DinkToPdf\|IConverter\|HtmlToPdfDocument\b\|ObjectSettings\b\|GlobalSettings\b" --type cs
rg "DinkToPdf\|SynchronizedConverter\|BasicConverter" --type cs -n

# 3. Find JavaScript delay settings (manual timing workarounds)
rg "javascript.delay\|JavascriptDelay\|--javascript-delay" --type cs --type yaml -n

# 4. Find header/footer HTML files used with wkhtmltopdf
rg "header.*html\|footer.*html\|HeaderHtmlUrl\|FooterHtmlUrl" --type cs -n
find . -name "header.html" -o -name "footer.html" -o -name "*-header.html" 2>/dev/null

# 5. Find wkhtmltopdf in NuGet
grep -r "DinkToPdf\|wkhtmltopdf" *.csproj **/*.csproj 2>/dev/null

# 6. Find wkhtmltopdf in Dockerfiles
grep -r "wkhtmltopdf\|wkhtmltox\|xvfb" Dockerfile* .dockerignore docker-compose*.yml 2>/dev/null

# 7. Find in CI/CD pipelines
grep -r "wkhtmltopdf" .github/**/*.yml .gitlab-ci.yml Jenkinsfile 2>/dev/null

# 8. Find secondary libraries (merge/security added alongside)
dotnet list package | grep -i "pdfsharp\|itextsharp\|pdfdocument"

# 9. Find wkhtmltopdf binary in project
find . -name "wkhtmltopdf*" -o -name "libwkhtmltox*" 2>/dev/null

# 10. Count total usage density
rg "wkhtmltopdf\|DinkToPdf\|HtmlToPdfDocument\b" --type cs | wc -l
```

---

## Install / Uninstall Checklist

```bash
# Remove DinkToPdf NuGet packages (verify exact names)
dotnet remove package DinkToPdf                    # verify
dotnet remove package DinkToPdf.NetCore            # verify

# Remove wkhtmltopdf binary from project if bundled
# rm -f Tools/wkhtmltopdf
# rm -f Tools/wkhtmltopdf.exe
# rm -f runtimes/linux-x64/native/libwkhtmltox.so  # if bundled via DinkToPdf

# Install IronPDF
dotnet add package IronPdf

dotnet restore

# Verify
dotnet list package | grep -i "dinkto\|wkhtmltopdf\|ironpdf"
```

Remove from Dockerfile — the biggest visible change:

```dockerfile
# REMOVE these lines from Dockerfile:
# RUN apt-get update && apt-get install -y wkhtmltopdf
# RUN apt-get install -y libxrender1 libfontconfig1 xvfb fonts-liberation

# REPLACE WITH IronPDF system dependencies (verify complete list in IronPDF docs):
RUN apt-get update && apt-get install -y \
    libnss3 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libgbm1 \
    libxkbcommon0 \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
    ?? throw new InvalidOperationException("IRONPDF_LICENSE_KEY not set");

// CI/CD: add IRONPDF_LICENSE_KEY to GitHub Secrets / Azure DevOps variables / etc.
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Diagnostics; // for Process.Start approach
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic HTML to PDF

**Before (DinkToPdf wrapper):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using System;

class Program
{
    // DinkToPdf: single global converter — thread-safety requires SynchronizedConverter
    private static readonly IConverter _converter =
        new SynchronizedConverter(new PdfTools());

    static void Main()
    {
        var doc = new HtmlToPdfDocument
        {
            GlobalSettings = { PaperSize = PaperKind.A4 },
            Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = "<html><body><h1>Hello</h1></body></html>",
                    WebSettings = { DefaultEncoding = "utf-8" },
                }
            }
        };

        var bytes = _converter.Convert(doc);
        System.IO.File.WriteAllBytes("output.pdf", bytes);
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

## Settings Migration Checklist

Map each wkhtmltopdf flag / DinkToPdf setting to its IronPDF equivalent.

### Global Settings

```bash
# Find all GlobalSettings usage to map
rg "GlobalSettings\s*\{?\s*\." --type cs -n
rg "PaperSize\|Orientation\|DocumentTitle\|DPI\b" --type cs -n
```

| wkhtmltopdf Flag / DinkToPdf Setting | IronPDF Equivalent |
|---|---|
| `--page-size A4` / `GlobalSettings.PaperSize = PaperKind.A4` | `renderer.RenderingOptions.PaperSize = PdfPaperSize.A4` |
| `--orientation Landscape` / `GlobalSettings.Orientation = Orientation.Landscape` | `renderer.RenderingOptions.PaperOrientation = ...` |
| `--dpi 300` / `GlobalSettings.DPI = 300` | Verify IronPDF DPI setting in rendering options |
| `--title "Report"` | Set via HTML `<title>` tag |
| `--collate` | N/A — IronPDF merge handles ordering |

### Object / Margin Settings

```bash
# Find ObjectSettings usage
rg "ObjectSettings\s*\{?\s*\." --type cs -n
rg "Margins\.\|MarginTop\|MarginBottom\|MarginLeft\|MarginRight" --type cs -n
```

| wkhtmltopdf Flag / DinkToPdf Setting | IronPDF Equivalent |
|---|---|
| `--margin-top 20` / `ObjectSettings.Margins.Top = 20` | `renderer.RenderingOptions.MarginTop = 20` |
| `--margin-bottom 20` | `renderer.RenderingOptions.MarginBottom = 20` |
| `--margin-left 25` | `renderer.RenderingOptions.MarginLeft = 25` |
| `--margin-right 25` | `renderer.RenderingOptions.MarginRight = 25` |
| `--encoding utf-8` / `WebSettings.DefaultEncoding` | UTF-8 is default — not needed |
| `--disable-javascript` | `renderer.RenderingOptions.EnableJavaScript = false` |
| `--javascript-delay 500` | `renderer.RenderingOptions.WaitFor.*` — verify in docs |

- [ ] Map each `GlobalSettings.*` property to `RenderingOptions.*`
- [ ] Map each `ObjectSettings.Margins.*` to `RenderingOptions.Margin*`
- [ ] Remove `WebSettings.DefaultEncoding` — not needed in IronPDF
- [ ] Replace `--javascript-delay` with appropriate `WaitFor` setting

### Header / Footer Migration Checklist

```bash
# Find header/footer HTML files
rg "HeaderHtmlUrl\|FooterHtmlUrl\|HeaderSettings\|FooterSettings" --type cs -n
find . -name "header.html" -o -name "footer.html" 2>/dev/null
```

wkhtmltopdf uses external HTML files for headers/footers with special tokens (`[page]`, `[toPage]`). IronPDF uses `HtmlHeaderFooter` with inline HTML and different tokens:

```csharp
// Before (wkhtmltopdf / DinkToPdf):
// HeaderSettings = new HeaderSettings
// {
//     HtmlUrl = "file:///app/header.html",  // external file
//     Spacing = 5,
// },
// FooterSettings = new FooterSettings
// {
//     HtmlUrl = "file:///app/footer.html",
//     Line = true,
// }
// Tokens in HTML files: [page] [toPage] [date] [sitepage] [sitepages]

// After (IronPDF):
// https://ironpdf.com/how-to/headers-and-footers/
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"<div style='font-family:Arial; font-size:10px; padding:0 25px'>
        <span style='float:left'>Report Title</span>
        <span style='float:right'>{date}</span>
    </div>",
    MaxHeight = 25, // mm — verify unit in current IronPDF docs
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"<div style='font-family:Arial; font-size:9px;
        text-align:right; padding:0 25px; color:#666'>
        Page {page} of {total-pages}
    </div>",
};
// Verify current token names ({page}, {total-pages}, {date}) in IronPDF docs
```

Token mapping:

| wkhtmltopdf Token | IronPDF Token |
|---|---|
| `[page]` | `{page}` |
| `[toPage]` | `{total-pages}` |
| `[date]` | `{date}` |
| `[sitepage]` / `[sitepages]` | Verify in IronPDF docs |

- [ ] Convert external header HTML files to `HtmlHeaderFooter.HtmlFragment` strings
- [ ] Convert external footer HTML files to `HtmlHeaderFooter.HtmlFragment` strings
- [ ] Replace wkhtmltopdf tokens (`[page]`, `[toPage]`) with IronPDF equivalents
- [ ] Remove `--header-html` / `--footer-html` CLI flags or `HeaderSettings.HtmlUrl`

---

## API Mapping Tables

### Namespace Mapping

| wkhtmltopdf / DinkToPdf | IronPDF | Notes |
|---|---|---|
| `DinkToPdf` | `IronPdf` | Core namespace |
| `DinkToPdf.Contracts` | `IronPdf.Rendering` | Options/settings namespace |
| N/A | `IronPdf.Editing` | Watermark / stamp |

### Core Class Mapping

| DinkToPdf Class | IronPDF Class | Description |
|---|---|---|
| `SynchronizedConverter` | `ChromePdfRenderer` | Primary rendering class |
| `HtmlToPdfDocument` | N/A — HTML string is the input | No document object needed |
| `GlobalSettings` | `ChromePdfRenderOptions` | Page-level settings |
| `ObjectSettings` | `ChromePdfRenderOptions` + HTML | Per-content settings + HTML input |

### Document Loading Methods

| Operation | wkhtmltopdf / DinkToPdf | IronPDF |
|---|---|---|
| HTML string | `ObjectSettings.HtmlContent = html` | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | `ObjectSettings.Page = url` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML file | `ObjectSettings.Page = "file:///path"` | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Get bytes | `converter.Convert(doc)` → `byte[]` | `pdf.BinaryData` |

### Page Operations

| Operation | wkhtmltopdf | IronPDF |
|---|---|---|
| Page count | Not returned | `pdf.PageCount` |
| Remove page | Not applicable | `pdf.RemovePage(index)` — verify |
| Extract text | Not applicable | `pdf.ExtractAllText()` |
| Rotate | Via CSS / print settings | Verify in IronPDF docs |

### Merge / Split Operations

| Operation | wkhtmltopdf | IronPDF |
|---|---|---|
| Merge | Not native — secondary lib | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not native | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before (DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.IO;

class HtmlToPdfBefore
{
    private static readonly IConverter _converter =
        new SynchronizedConverter(new PdfTools());

    static void Main()
    {
        var html = @"
            <html>
            <head><style>
            body { font-family: Arial, sans-serif; padding: 40px; }
            table { width: 100%; border-collapse: collapse; }
            th, td { border: 1px solid #ccc; padding: 6px; font-size: 12px; }
            </style></head>
            <body>
                <h1>Invoice #2024-0099</h1>
                <p>Customer: Acme Corp | Due: 2024-12-31</p>
                <table>
                    <tr><th>Item</th><th>Qty</th><th>Price</th></tr>
                    <tr><td>Widget Pro</td><td>3</td><td>$149.00</td></tr>
                </table>
            </body></html>";

        var doc = new HtmlToPdfDocument
        {
            GlobalSettings =
            {
                PaperSize = PaperKind.A4,
                Margins = { Top = 20, Bottom = 20, Left = 25, Right = 25,
                            Unit = Unit.Millimeters },
            },
            Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8" },
                }
            }
        };

        var bytes = _converter.Convert(doc);
        File.WriteAllBytes("invoice.pdf", bytes);
        Console.WriteLine($"Saved invoice.pdf ({bytes.Length:N0} bytes)");
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
    body { font-family: Arial, sans-serif; padding: 40px; }
    table { width: 100%; border-collapse: collapse; }
    th, td { border: 1px solid #ccc; padding: 6px; font-size: 12px; }
    </style></head>
    <body>
        <h1>Invoice #2024-0099</h1>
        <p>Customer: Acme Corp | Due: 2024-12-31</p>
        <table>
            <tr><th>Item</th><th>Qty</th><th>Price</th></tr>
            <tr><td>Widget Pro</td><td>3</td><td>$149.00</td></tr>
        </table>
    </body></html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 25;
renderer.RenderingOptions.MarginRight = 25;

var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");
Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (wkhtmltopdf — not native; secondary library):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.Collections.Generic;
using System.IO;

class MergeBefore
{
    private static readonly IConverter _converter =
        new SynchronizedConverter(new PdfTools());

    static void Main()
    {
        // DinkToPdf: no merge — generate each section, merge externally
        var sections = new[]
        {
            "<html><body><h1>Section 1: Overview</h1></body></html>",
            "<html><body><h1>Section 2: Detail</h1></body></html>",
        };

        var pdfBytes = new List<byte[]>();
        foreach (var html in sections)
        {
            var doc = new HtmlToPdfDocument
            {
                Objects = { new ObjectSettings { HtmlContent = html } }
            };
            pdfBytes.Add(_converter.Convert(doc));
        }

        // Merge via secondary library (PdfSharp, iTextSharp, etc.):
        // var merged = SomePdfLib.Merge(pdfBytes);
        // File.WriteAllBytes("merged.pdf", merged);

        Console.WriteLine("wkhtmltopdf: merge needs secondary library");
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

var results = await Task.WhenAll(
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 1: Overview</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 2: Detail</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages — no secondary library needed");
```

---

### 3. Watermark

**Before (wkhtmltopdf — CSS embed or secondary library):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.IO;

class WatermarkBefore
{
    private static readonly IConverter _converter =
        new SynchronizedConverter(new PdfTools());

    static void Main()
    {
        // Option A: embed watermark in HTML before rendering (CSS fixed position)
        var htmlWithWatermark = @"
            <html><head><style>
            .watermark {
                position: fixed; top: 50%; left: 50%;
                transform: rotate(-45deg) translate(-50%, -50%);
                font-size: 80px; opacity: 0.08; color: #888;
                pointer-events: none; user-select: none;
            }
            </style></head>
            <body>
                <div class='watermark'>DRAFT</div>
                <h1>Report</h1>
            </body></html>";

        var doc = new HtmlToPdfDocument
        {
            Objects = { new ObjectSettings { HtmlContent = htmlWithWatermark } }
        };

        var bytes = _converter.Convert(doc);
        File.WriteAllBytes("watermarked.pdf", bytes);
        // Option B: post-generation via secondary library — omitted
        Console.WriteLine("wkhtmltopdf: CSS embed or secondary library for watermark");
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
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Report</h1></body></html>");

// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronPdf.Imaging.Color.Gray,
    Opacity = 0.15,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (wkhtmltopdf — not native; secondary library):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.IO;

class PasswordBefore
{
    private static readonly IConverter _converter =
        new SynchronizedConverter(new PdfTools());

    static void Main()
    {
        // wkhtmltopdf / DinkToPdf have no native password API
        // Generate PDF first, then apply password via secondary library
        var doc = new HtmlToPdfDocument
        {
            Objects = { new ObjectSettings
                { HtmlContent = "<html><body><h1>Protected</h1></body></html>" } }
        };

        var bytes = _converter.Convert(doc);

        // Apply password via secondary library (required):
        // var secured = SomePdfLib.SetPassword(bytes, "open123", "admin456");
        // File.WriteAllBytes("secured.pdf", secured);

        Console.WriteLine("wkhtmltopdf: password requires secondary library");
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

### wkhtmltopdf Is Archived

The project is archived as of 2023. The implication for production systems: no security patches, no .NET runtime compatibility updates, no rendering bug fixes. This is the clearest migration trigger.

### `SynchronizedConverter` Singleton Removal

DinkToPdf's standard pattern uses a `SynchronizedConverter` as a thread-safe singleton. After migrating, the entire pattern is removed:

```bash
# Find the singleton to remove
rg "SynchronizedConverter\|BasicConverter\|IConverter\b" --type cs -n
rg "new PdfTools\(\)" --type cs -n
```

IronPDF's `ChromePdfRenderer` doesn't have the same single-threaded constraint. Each Task can have its own renderer:

```csharp
// Before: one SynchronizedConverter for all requests (serialized)
// private static readonly IConverter _converter = new SynchronizedConverter(new PdfTools());

// After: one renderer per concurrent task
var pdfs = await Task.WhenAll(jobs.Select(async html =>
{
    var renderer = new ChromePdfRenderer(); // one per task
    return await renderer.RenderHtmlAsPdfAsync(html);
}));
// See: https://ironpdf.com/examples/parallel/
```

### Header/Footer Token Format Change

This is the most common post-migration bug. wkhtmltopdf uses square bracket tokens (`[page]`, `[toPage]`). IronPDF uses curly brace tokens (`{page}`, `{total-pages}`). Any header/footer HTML that referenced wkhtmltopdf tokens needs updating:

```bash
# Find wkhtmltopdf tokens in HTML files
grep -r "\[page\]\|\[toPage\]\|\[date\]\|\[sitepages\]" . --include="*.html" -n
```

### CSS Rendering Differences

Qt WebKit (wkhtmltopdf) and Chromium render CSS differently. After migration, test for:

```bash
# Find CSS that was added as wkhtmltopdf workarounds
grep -r "TODO.*wkhtmltopdf\|HACK.*wkhtml\|-webkit-column\|page-break-before" \
    --include="*.html" --include="*.css" . 2>/dev/null
```

Common differences to verify:
- `@page` CSS margin rules
- `page-break-before: always` behavior
- CSS `position: fixed` in headers/footers
- Font rendering of system fonts

### `--javascript-delay` Replacement

wkhtmltopdf used `--javascript-delay` to wait for JavaScript to execute before rendering. IronPDF has rendering options for JavaScript wait conditions:

```csharp
// wkhtmltopdf: --javascript-delay 1000 (crude timing)
// IronPDF: verify WaitFor options in rendering docs
// See: https://ironpdf.com/how-to/rendering-options/
// The approach is more reliable than a fixed delay
```

---

## Performance Considerations

### Concurrent Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// wkhtmltopdf: SynchronizedConverter serializes all conversions through one thread
// IronPDF: Task.WhenAll with per-task renderer — no serialization overhead
// https://ironpdf.com/examples/parallel/

var htmlJobs = invoiceDataList.Select(BuildInvoiceHtml).ToArray();

var pdfs = await Task.WhenAll(htmlJobs.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Generated {pdfs.Length} PDFs concurrently");
foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Warm-Up

```csharp
using IronPdf;

// Amortize Chromium initialization at application startup
// DinkToPdf also had initialization overhead on first use — this is equivalent
var renderer = new ChromePdfRenderer();
using var _ = await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");
// Ready for production traffic
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

// DinkToPdf: Convert() returned byte[] — no disposal needed
// IronPDF: PdfDocument should be disposed

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
var bytes = pdf.BinaryData; // equivalent to DinkToPdf Convert() byte[] return
pdf.SaveAs(outputPath);     // or save directly
// pdf disposed at end of 'using' block
```

---

## Testing Checklist

- [ ] Render each HTML template and compare visually against wkhtmltopdf reference output
- [ ] Check that modern CSS (flex, grid, custom properties) renders correctly
- [ ] Verify header and footer tokens work correctly (`{page}`, `{total-pages}`)
- [ ] Test page size and margin accuracy
- [ ] Test Docker deployment — verify no native dependency failures
- [ ] Test concurrent rendering at target throughput — no `SynchronizedConverter` bottleneck
- [ ] Verify JS-rendered content renders correctly with appropriate `WaitFor` settings

---

## Post-Migration Checklist

- [ ] Remove `DinkToPdf` and `DinkToPdf.NetCore` NuGet packages
- [ ] Remove `SynchronizedConverter` / `BasicConverter` singleton infrastructure
- [ ] Remove wkhtmltopdf system install from Dockerfile (`apt-get install wkhtmltopdf`)
- [ ] Remove X11/display library installs (`libxrender1`, `xvfb`) from Dockerfile
- [ ] Remove secondary PDF libraries used for merge/security
- [ ] Remove wkhtmltopdf binary from CI/CD artifact lists
- [ ] Add IronPDF license key to CI/CD secrets
- [ ] Add IronPDF Chromium system library installs to Dockerfile

---

## That's the Migration

wkhtmltopdf's archived status removes the "if it's not broken" argument. The project won't receive updates, and as .NET and system libraries evolve, the gap will grow. The Docker complexity is usually the practical trigger that forces the migration timeline sooner rather than later.

The header/footer token format change (`[page]` → `{page}`) is the most common post-migration surprise — flag it before starting the code changes and it's a five-minute find-and-replace. Everything else maps directly.

**Discussion question:** Based on your own wkhtmltopdf migration, what would you add to this checklist — particularly around CSS rendering differences, Docker image setup, or JavaScript-rendered content that required additional wait configuration?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
