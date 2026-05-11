---
title: "TuesPechkin to IronPDF: an honest migration walkthrough"
published: false
tags: dotnet, csharp, pdf, migration
---

The license renewal arrives and someone finally checks the NuGet page before paying. TuesPechkin's last release was years ago. The wkhtmltopdf binary it wraps hasn't seen an update in a while either. The .NET 6+ support is community-maintained at best. The conversation about finding something with active maintenance gets moved from "someday" to "this sprint."

This article covers migrating from TuesPechkin to IronPDF. You'll have working before/after code and a comprehensive checklist by the end. The codebase audit commands and comparison tables are useful regardless of which replacement you choose.

---

## Why Migrate (Without Drama)

Teams evaluating TuesPechkin replacements typically encounter these conditions:

1. **Maintenance status** — TuesPechkin and wkhtmltopdf are both in low or no active development; .NET 6+ compatibility is not officially maintained.
2. **Windows-only** — the bundled wkhtmltopdf binary is Windows-only; Docker on Linux or any cross-platform deployment fails.
3. **wkhtmltopdf rendering age** — Qt WebKit is a pre-2016 rendering engine; modern CSS (flex, grid, custom properties) doesn't render correctly.
4. **Thread safety constraints** — wkhtmltopdf has COM-like STA affinity requirements; concurrent rendering requires a dedicated thread.
5. **Binary bundling** — the Win64 NuGet includes a ~14MB binary; the package must be updated manually when the binary changes.
6. **AppDomain/STA infrastructure** — typical TuesPechkin production setups use an STA-threaded AppDomain or `ThreadLocal<IConverter>` to manage thread safety.
7. **No native PDF manipulation** — merge, watermark, security, text extraction require additional libraries alongside TuesPechkin.
8. **Deployment artifact size** — the wkhtmltopdf binary increases Docker image or deployment artifact size.
9. **No URL authentication** — cookies, session tokens, and authenticated page rendering are complex with wkhtmltopdf.
10. **JavaScript execution** — wkhtmltopdf executes JavaScript but with an older engine; complex JS-rendered content may not render correctly.

### Comparison Table

| Aspect | TuesPechkin + wkhtmltopdf | IronPDF |
|---|---|---|
| Focus | HTML-to-PDF via wkhtmltopdf wrapper | HTML-to-PDF + PDF manipulation |
| Pricing | Open source — wkhtmltopdf LGPL | Commercial license ([ironpdf.com/licensing](https://ironpdf.com/licensing/)) |
| API Style | `IConverter.Convert(document)` + settings objects | `ChromePdfRenderer` + options |
| Learning Curve | Low for basic use; STA threading adds complexity | Low for .NET devs |
| HTML Rendering | Qt WebKit (pre-2016) | Embedded Chromium |
| Page Indexing | wkhtmltopdf 1-based in some contexts | 0-based |
| Thread Safety | STA thread affinity; one thread per converter | `ChromePdfRenderer` is thread-safe; concurrent rendering supported |
| Namespace | `TuesPechkin` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | TuesPechkin | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | `IConverter.Convert(doc)` | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | `ObjectSettings.Page = url` | `renderer.RenderUrlAsPdfAsync(url)` | Low |
| HTML file to PDF | `ObjectSettings.Page = "file:///..."` | `renderer.RenderHtmlFileAsPdfAsync(path)` | Low |
| Save to bytes | `Convert()` returns byte[] | `pdf.BinaryData` | Low |
| Save to file | `File.WriteAllBytes(path, bytes)` | `pdf.SaveAs(path)` | Low |
| Page size | `GlobalSettings.PaperSize` | `RenderingOptions.PaperSize` | Low |
| Margins | `ObjectSettings.Margins.*` | `RenderingOptions.Margin*` | Low |
| Headers/footers | `HeaderSettings` / `FooterSettings` | `RenderingOptions.HtmlHeader/Footer` | Medium |
| Merge PDFs | Not native | `PdfDocument.Merge()` | Medium |
| Watermark | Not native | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Not native | `pdf.SecuritySettings` | Medium |
| Text extraction | Not native | `pdf.ExtractAllText()` | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| .NET 6+ compatibility is the primary concern | Switch — TuesPechkin has no official .NET 6+ support |
| Docker/Linux deployment needed | Switch — wkhtmltopdf binary is Windows-only |
| Modern CSS (flex, grid) required | Switch — Chromium vs pre-2016 WebKit |
| Basic HTML-to-PDF, Windows-only, wkhtmltopdf working well | Evaluate migration cost; if it's working, timeline is flexible |

---

## Pre-Migration Codebase Audit Checklist

Before touching any code, run these commands and document the results. The output determines scope and risk.

### Find All TuesPechkin References

```bash
# 1. Find all TuesPechkin API usage files
rg -l "TuesPechkin\|IConverter\|HtmlToPdfDocument\|ObjectSettings\|GlobalSettings" --type cs

# 2. Show line-level usage for review
rg "TuesPechkin\|IConverter\|HtmlToPdfDocument" --type cs -n

# 3. Find STA threading workaround code
rg "ApartmentState\.STA\|SetApartmentState\|ThreadLocal.*Converter" --type cs -n

# 4. Find AppDomain usage (common TuesPechkin server pattern)
rg "AppDomain\b\|IsolatedAppDomain" --type cs -n

# 5. Find the converter factory / singleton pattern
rg "ThreadSafeConverter\|MultiplexingConverter\|ThreadLocal<IConverter>" --type cs -n

# 6. Find URL rendering vs HTML string rendering
rg "ObjectSettings.*Page\s*=\|Page\s*=.*http" --type cs -n

# 7. Find secondary libraries added for missing features
grep -r "PdfSharp\|iTextSharp\|PDFMerge\|DinkToPdf\b" *.csproj **/*.csproj 2>/dev/null

# 8. Find TuesPechkin NuGet packages
grep -r "TuesPechkin" *.csproj **/*.csproj 2>/dev/null

# 9. Count usage density
rg "IConverter\.\|HtmlToPdfDocument\|ObjectSettings" --type cs | wc -l

# 10. Check for wkhtmltopdf binary references
find . -name "wkhtmltopdf*.dll" -o -name "wkhtmltopdf*.exe" 2>/dev/null
```

---

## Install / Uninstall Checklist

```bash
# Remove TuesPechkin packages
dotnet remove package TuesPechkin
dotnet remove package TuesPechkin.Wkhtmltopdf.Win64   # or Win32
dotnet remove package TuesPechkin.Wkhtmltopdf.Win32   # if used

# Remove secondary libraries that filled TuesPechkin's gaps (after verifying IronPDF covers them)
# dotnet remove package PdfSharp      # if used only for merge
# dotnet remove package DinkToPdf     # if duplicate of TuesPechkin

# Install IronPDF
dotnet add package IronPdf

dotnet restore

# Verify packages
dotnet list package | grep -i "tuespechkin\|wkhtmltopdf\|ironpdf"
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
    ?? throw new InvalidOperationException("IRONPDF_LICENSE_KEY not set");

// Remove: TuesPechkin has no equivalent license setup — but remove the ThreadSafeConverter singleton
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using TuesPechkin;
using System.Threading;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic HTML to PDF

**Before (TuesPechkin with STA thread):**
```csharp
using TuesPechkin;
using System;
using System.IO;
using System.Threading;

class Program
{
    // ThreadLocal converter — the standard STA workaround
    private static readonly ThreadLocal<IConverter> _converter = new ThreadLocal<IConverter>(
        () => new ThreadSafeConverter(
            new PdfToolset(new Win64EmbeddedDeployment(
                new TempFolderDeployment()))));

    static void Main()
    {
        byte[] pdfBytes = null;
        var thread = new Thread(() =>
        {
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = { PaperSize = PaperKind.A4 },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlText = "<html><body><h1>Hello</h1></body></html>",
                        WebSettings = { DefaultEncoding = "utf-8" },
                    }
                }
            };
            pdfBytes = _converter.Value.Convert(doc);
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        File.WriteAllBytes("output.pdf", pdfBytes);
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

## Code Migration Checklist

Work through each item in order. Check each off before proceeding.

### Converter Singleton Removal

```bash
# Find the converter pattern to remove
rg "ThreadSafeConverter\|MultiplexingConverter\|ThreadLocal<IConverter>" --type cs -n
rg "PdfToolset\|Win64EmbeddedDeployment\|TempFolderDeployment" --type cs -n
```

The `ThreadSafeConverter` / `MultiplexingConverter` singleton is the central workaround for wkhtmltopdf's thread affinity. After migration, the entire pattern is removed:

- [ ] Remove `ThreadLocal<IConverter>` or `MultiplexingConverter` field
- [ ] Remove `PdfToolset` / `Win64EmbeddedDeployment` / `TempFolderDeployment` initialization
- [ ] Remove STA thread wrapper if present (`thread.SetApartmentState(ApartmentState.STA)`)

### Settings Migration

```bash
# Find all GlobalSettings and ObjectSettings usage
rg "GlobalSettings\s*\{?\s*\." --type cs -n
rg "ObjectSettings\s*\{?\s*\." --type cs -n
```

| TuesPechkin Setting | IronPDF Equivalent |
|---|---|
| `GlobalSettings.PaperSize = PaperKind.A4` | `renderer.RenderingOptions.PaperSize = PdfPaperSize.A4` |
| `GlobalSettings.Orientation = PaperOrientation.Landscape` | `renderer.RenderingOptions.PaperOrientation = ...` |
| `ObjectSettings.HtmlText = html` | HTML string input to `RenderHtmlAsPdfAsync()` |
| `ObjectSettings.Page = url` | URL input to `RenderUrlAsPdfAsync()` |
| `ObjectSettings.Margins.Top = 20` | `renderer.RenderingOptions.MarginTop = 20` |
| `WebSettings.DefaultEncoding = "utf-8"` | N/A — UTF-8 is default |
| `LoadSettings.JavascriptDelay = 500` | `renderer.RenderingOptions.WaitFor.JavaScript(500)` |
| `HeaderSettings` / `FooterSettings` | `RenderingOptions.HtmlHeader` / `HtmlFooter` |

- [ ] Map each `GlobalSettings.*` property to `RenderingOptions.*`
- [ ] Map each `ObjectSettings.*` property to `RenderingOptions.*`
- [ ] Replace `ObjectSettings.Page = url` with `RenderUrlAsPdfAsync(url)`
- [ ] Replace `ObjectSettings.HtmlText = html` with `RenderHtmlAsPdfAsync(html)`
- [ ] Remove `WebSettings.DefaultEncoding` (not needed)

### Output Pattern Migration

```bash
# Find all Convert() call sites
rg "\.Convert\(" --type cs -n
# Find byte array usage pattern
rg "Convert\(doc\)\|pdfBytes\|byte\[\].*pdf" --type cs -n
```

- [ ] Replace `converter.Convert(doc)` (returns `byte[]`) with `await renderer.RenderHtmlAsPdfAsync(html)` (returns `PdfDocument`)
- [ ] Replace `File.WriteAllBytes(path, pdfBytes)` with `pdf.SaveAs(path)`
- [ ] Replace `return pdfBytes` (byte array) with `return pdf.BinaryData`
- [ ] Replace stream writing of `pdfBytes` with `pdf.Stream.CopyTo(stream)`
- [ ] Wrap all `PdfDocument` usage in `using` blocks

### Header / Footer Migration

```bash
# Find HeaderSettings / FooterSettings usage
rg "HeaderSettings\|FooterSettings\|HeaderHtmlUrl\|FooterHtmlUrl" --type cs -n
```

```csharp
// Before (TuesPechkin header):
// var doc = new HtmlToPdfDocument { Objects = { new ObjectSettings {
//     HeaderSettings = new HeaderSettings { HtmlUrl = "file:///header.html", Spacing = 5 }
// }}};

// After:
// https://ironpdf.com/how-to/headers-and-footers/
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='font-size:10px; text-align:left; padding:0 20px'>Report Title</div>",
    MaxHeight = 25, // millimeters
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='font-size:9px; text-align:right; padding:0 20px'>Page {page} of {total-pages}</div>",
};
```

- [ ] Replace `HeaderSettings.HtmlUrl` with `RenderingOptions.HtmlHeader.HtmlFragment`
- [ ] Replace `FooterSettings.HtmlUrl` with `RenderingOptions.HtmlFooter.HtmlFragment`
- [ ] Replace page number tokens (`[page]` / `[topage]` in wkhtmltopdf) with `{page}` / `{total-pages}`

---

## API Mapping Tables

### Namespace Mapping

| TuesPechkin | IronPDF | Notes |
|---|---|---|
| `TuesPechkin` | `IronPdf` | Core namespace |
| N/A | `IronPdf.Rendering` | Rendering config |
| N/A | `IronPdf.Editing` | Watermark / stamp |

### Core Class Mapping

| TuesPechkin Class | IronPDF Class | Description |
|---|---|---|
| `IConverter` / `ThreadSafeConverter` | `ChromePdfRenderer` | Primary rendering class |
| `HtmlToPdfDocument` | N/A — HTML string is the input | No document object needed |
| `GlobalSettings` | `ChromePdfRenderOptions` | Page size, margins, global settings |
| `ObjectSettings` | `ChromePdfRenderOptions` + HTML input | Per-document settings |

### Document Loading Methods

| Operation | TuesPechkin | IronPDF |
|---|---|---|
| HTML string | `ObjectSettings.HtmlText = html` | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | `ObjectSettings.Page = url` | `renderer.RenderUrlAsPdfAsync(url)` |
| HTML file | `ObjectSettings.Page = "file:///path"` | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| Get bytes | `converter.Convert(doc)` → `byte[]` | `pdf.BinaryData` |

### Page Operations

| Operation | TuesPechkin | IronPDF |
|---|---|---|
| Page count | Not returned from Convert() | `pdf.PageCount` |
| Remove page | Not applicable | `pdf.RemovePages(index)` |
| Extract text | Not applicable | `pdf.ExtractAllText()` |
| Rotate | Via CSS `@page { rotate }` or wkhtmltopdf setting | `pdf.RotateAllPages(PdfRotation.Clockwise90)` |

### Merge / Split Operations

| Operation | TuesPechkin | IronPDF |
|---|---|---|
| Merge | Not native — secondary lib | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not native | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before (TuesPechkin):**
```csharp
using TuesPechkin;
using System;
using System.IO;
using System.Threading;

class HtmlToPdfBefore
{
    private static readonly ThreadLocal<IConverter> _converter = new(() =>
        new ThreadSafeConverter(
            new PdfToolset(
                new Win64EmbeddedDeployment(new TempFolderDeployment()))));

    static void Main()
    {
        byte[] pdfBytes = null;

        var thread = new Thread(() =>
        {
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    PaperSize = PaperKind.A4,
                    Margins = { Top = 20, Bottom = 20, Left = 25, Right = 25, Unit = Unit.Millimeters },
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlText = "<html><body><h1>Invoice #9901</h1></body></html>",
                        WebSettings = { DefaultEncoding = "utf-8" },
                    }
                }
            };
            pdfBytes = _converter.Value.Convert(doc);
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        File.WriteAllBytes("invoice.pdf", pdfBytes);
        Console.WriteLine($"Saved invoice.pdf ({pdfBytes.Length:N0} bytes)");
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
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 25;
renderer.RenderingOptions.MarginRight = 25;

var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Invoice #9901</h1></body></html>");
pdf.SaveAs("invoice.pdf");
Console.WriteLine($"Saved invoice.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (TuesPechkin — not native; secondary library required):**
```csharp
using TuesPechkin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class MergeBefore
{
    private static readonly ThreadLocal<IConverter> _converter = new(() =>
        new ThreadSafeConverter(
            new PdfToolset(new Win64EmbeddedDeployment(new TempFolderDeployment()))));

    static void Main()
    {
        var htmlSections = new[] {
            "<html><body><h1>Section A</h1></body></html>",
            "<html><body><h1>Section B</h1></body></html>",
        };
        var pdfFiles = new List<string>();

        for (int i = 0; i < htmlSections.Length; i++)
        {
            var html = htmlSections[i];
            var outPath = $"section{i + 1}.pdf";
            byte[] bytes = null;

            var thread = new Thread(() =>
            {
                var doc = new HtmlToPdfDocument
                {
                    Objects = { new ObjectSettings { HtmlText = html } }
                };
                bytes = _converter.Value.Convert(doc);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            File.WriteAllBytes(outPath, bytes);
            pdfFiles.Add(outPath);
        }

        // Merge via secondary library — TuesPechkin has no merge
        Console.WriteLine("Merge requires secondary library — STA thread per render");
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
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section A</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section B</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages — no STA thread needed");
```

---

### 3. Watermark

**Before (TuesPechkin — not native; secondary library required):**
```csharp
using TuesPechkin;
using System;
using System.IO;
using System.Threading;

class WatermarkBefore
{
    static void Main()
    {
        byte[] pdfBytes = null;
        var thread = new Thread(() =>
        {
            var converter = new ThreadSafeConverter(
                new PdfToolset(new Win64EmbeddedDeployment(new TempFolderDeployment())));

            var doc = new HtmlToPdfDocument
            {
                Objects = { new ObjectSettings
                    { HtmlText = "<html><body><h1>Report</h1></body></html>" } }
            };
            pdfBytes = converter.Convert(doc);
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        // Watermark requires secondary library — TuesPechkin can't do this
        // var watermarked = SomePdfLib.AddTextWatermark(pdfBytes, "DRAFT");
        // File.WriteAllBytes("watermarked.pdf", watermarked);

        Console.WriteLine("Watermark requires secondary library with TuesPechkin");
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
    FontColor = IronSoftware.Drawing.Color.Gray,
    Opacity = 15,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (TuesPechkin — not native):**
```csharp
using TuesPechkin;
using System;
using System.IO;
using System.Threading;

class PasswordBefore
{
    static void Main()
    {
        byte[] pdfBytes = null;
        var thread = new Thread(() =>
        {
            var converter = new ThreadSafeConverter(
                new PdfToolset(new Win64EmbeddedDeployment(new TempFolderDeployment())));

            var doc = new HtmlToPdfDocument
            {
                Objects = { new ObjectSettings
                    { HtmlText = "<html><body><h1>Protected Doc</h1></body></html>" } }
            };
            pdfBytes = converter.Convert(doc);
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        // Password protection requires secondary library — TuesPechkin doesn't support it
        // var secured = SomePdfLib.SetPassword(pdfBytes, "open123", "admin456");
        // File.WriteAllBytes("secured.pdf", secured);

        Console.WriteLine("Password protection requires secondary library with TuesPechkin");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Protected Doc</h1></body></html>");

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Testing Checklist

### Visual Output Validation

- [ ] Render each HTML template and compare visually against TuesPechkin reference output
- [ ] Check that modern CSS (flex, grid) now renders correctly (it didn't with Qt WebKit)
- [ ] Verify page size and margin match reference
- [ ] Test headers and footers — check page number token format difference (`[page]` → `{page}`)

### Concurrency Validation

- [ ] Remove STA thread wrapper from at least one call site and verify rendering still works
- [ ] Run concurrent rendering test at 10/50/100 simultaneous requests
- [ ] Verify no ThreadAbortException or lock contention errors
- [ ] Compare throughput: before (serialized through STA thread) vs after (concurrent)

### Feature Validation

- [ ] Test merge output — verify page order is correct
- [ ] Test watermark — verify opacity, position, and text
- [ ] Test password protection — verify PDF opens with user password; denies without
- [ ] Test text extraction if used (`pdf.ExtractAllText()`)
- [ ] Verify URL rendering with any authenticated endpoints

### Platform Validation

- [ ] Test in Docker on Linux (the Windows-only constraint is now removed)
- [ ] Verify Docker image builds without the wkhtmltopdf binary
- [ ] Check Docker image size difference (no ~14MB wkhtmltopdf binary)
- [ ] Verify CI pipeline runs successfully on Linux runner

---

## Critical Migration Notes

### wkhtmltopdf Token Format Changes

wkhtmltopdf and TuesPechkin use `[page]` and `[topage]` as header/footer tokens. IronPDF uses `{page}` and `{total-pages}`. Any header/footer HTML that referenced the old tokens needs updating:

```csharp
// wkhtmltopdf/TuesPechkin footer tokens:
// "Page [page] of [topage]"

// IronPDF footer tokens:
// https://ironpdf.com/how-to/headers-and-footers/
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='font-size:9px; text-align:right; padding: 0 20px'>Page {page} of {total-pages}</div>",
};
```

### STA Thread Boilerplate Count

Count the STA thread wrappers before starting code changes — each one is removed:

```bash
# Each STA thread creation is a before/after pair
rg "SetApartmentState\(ApartmentState\.STA\)" --type cs -n | wc -l
# This number = number of migration sites
```

### Secondary Library Removal

TuesPechkin commonly accumulates secondary libraries for missing features. After IronPDF covers merge, watermark, and security, these can be removed:

```bash
# Find secondary PDF libraries to remove
dotnet list package | grep -i "pdfsharp\|itextsharp\|pdfdocument\|dinkto"
```

### Cross-Platform Docker

Remove the wkhtmltopdf binary copy from Dockerfiles:

```dockerfile
# Remove:
# COPY wkhtmltopdf.exe /app/
# or any Win64 binary copy step

# IronPDF Chromium dependencies (Linux):
RUN apt-get update && apt-get install -y \
    libnss3 libatk-bridge2.0-0 libdrm2 libgbm1 libxkbcommon0 fonts-liberation \
    libxcomposite1 libxdamage1 libxfixes3 libxrandr2 libasound2 \
    && rm -rf /var/lib/apt/lists/*
# Full list: https://ironpdf.com/docs/questions/installing-ironpdf-linux/
```

---

## Performance Considerations

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// TuesPechkin: one at a time per STA thread
// IronPDF: concurrent, no STA constraint
// https://ironpdf.com/examples/parallel/

var htmlJobs = invoiceDataList.Select(data => BuildInvoiceHtml(data)).ToArray();

var pdfs = await Task.WhenAll(htmlJobs.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Rendered {pdfs.Length} invoices concurrently");
foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Disposal Pattern

```csharp
using IronPdf;
using System.IO;

// TuesPechkin: Convert() returned byte[] — no disposal needed
// IronPDF: PdfDocument should be disposed

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// Equivalent of TuesPechkin's byte[] return:
var bytes = pdf.BinaryData;

// Or save directly:
pdf.SaveAs(outputPath);
// pdf disposed at end of 'using' block
```

---

## Post-Migration Checklist

- [ ] Remove `TuesPechkin` NuGet package
- [ ] Remove `TuesPechkin.Wkhtmltopdf.Win64` (and Win32 if present) packages
- [ ] Remove `ThreadSafeConverter` / `MultiplexingConverter` singleton infrastructure
- [ ] Remove STA thread boilerplate from all PDF generation code
- [ ] Remove wkhtmltopdf binary from Docker image copy steps
- [ ] Remove secondary PDF libraries now replaced by IronPDF
- [ ] Add IronPDF license key to CI/CD secrets
- [ ] Update Dockerfile with IronPDF Linux system dependencies
- [ ] Verify deployment artifact size reduced (no wkhtmltopdf binary)
- [ ] Archive wkhtmltopdf-specific header/footer HTML files for reference

---

## The Bottom Line

TuesPechkin's maintenance status is the clearest migration trigger in this space — both the wrapper and the underlying wkhtmltopdf have low or no active development, and .NET 6+ compatibility is not guaranteed. The migration is one of the more direct in this space because both libraries solve the same core problem (HTML-to-PDF), and the API surface maps reasonably closely once the STA thread wrapper is removed.

The immediate wins — Linux/Docker support, modern CSS rendering, concurrent rendering without thread infrastructure — are measurable from day one.

**Discussion question:** What edge cases did you hit that this article didn't cover — particularly around header/footer token migration, URL rendering with authentication, or JavaScript-heavy pages that behaved differently between wkhtmltopdf and Chromium?
