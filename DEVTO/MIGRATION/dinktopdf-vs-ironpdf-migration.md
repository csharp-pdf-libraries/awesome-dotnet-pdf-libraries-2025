---
title: "Haukcode.DinkToPdf to IronPDF: an honest migration walkthrough"
published: false
tags: dotnet, csharp, pdf, migration
---

License renewal season tends to trigger these conversations. The DinkToPdf package has been doing its job, but it's time to evaluate alternatives before signing off on another cycle — and while you're in there, you want to understand exactly what you're trading away and what you might gain. This article gives you a fact-based comparison and working code for both sides, so you can make that call with real information instead of vendor marketing.

Even if DinkToPdf is staying for now, the mapping tables and checklist are reference material worth keeping.

---

## Why migrate (without drama)

Before touching a line of code, it helps to be clear about why you're looking. Here are 9 neutral migration triggers — not all will apply:

1. **Underlying library maintenance status** — `libwkhtmltopdf` is no longer under active development. The DinkToPdf wrapper depends on it entirely. Teams on long-lived projects factor this into renewal decisions.
2. **WebKit vs Chromium rendering** — DinkToPdf renders using a patched WebKit. CSS Grid, Flexbox, and modern CSS features may behave differently than your browser preview. Teams with complex HTML templates encounter fidelity issues.
3. **Native binary dependency** — `libwkhtmltopdf.so` must be present at runtime. Docker images need to include it explicitly; deployment pipelines need to account for it. This is manageable but adds surface area.
4. **Linux-only native behavior on some builds** — Windows development with Linux deployment can surface native library mismatch issues in common configurations.
5. **No PDF manipulation beyond generation** — DinkToPdf generates PDFs; it doesn't merge, split, watermark, or encrypt them. Teams commonly bolt on a second library (PdfSharp, iTextSharp) for these features.
6. **Thread safety characteristics** — `libwkhtmltopdf` has documented thread-safety constraints. Review your concurrent rendering patterns.
7. **JavaScript support gaps** — wkhtmltopdf's WebKit has limited support for modern JS features. Dynamic content generated via JS may not render as expected.
8. **Headers/footers templating** — DinkToPdf supports headers and footers via wkhtmltopdf's HTML template mechanism; behavior can be finicky with complex layouts.
9. **.NET version trajectory** — as your stack moves to .NET 8/9, native binary wrapper compatibility becomes an ongoing maintenance concern.

### Comparison table

| Aspect | Haukcode.DinkToPdf | IronPDF |
|---|---|---|
| Focus | HTML-to-PDF via WebKit native lib | HTML-to-PDF + full PDF manipulation |
| Pricing | Open source (MIT) | Commercial license — see [ironpdf.com](https://ironpdf.com/) |
| API Style | Converter class + settings objects | Renderer + document model |
| Learning Curve | Low for basic use, medium for advanced | Medium — larger API surface |
| HTML Rendering | WebKit (wkhtmltopdf) | Chromium-based |
| Page Indexing | N/A (generation only, no manipulation) | 0-based |
| Thread Safety | libwkhtmltopdf constraints apply | Renderer reuse — see async docs |
| Namespace | `DinkToPdf` | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | DinkToPdf approach | Effort to migrate |
|---|---|---|
| HTML string to PDF | `SynchronizedConverter` + `HtmlToPdfDocument` | Low |
| HTML file to PDF | Via file:// URI in settings | Low |
| URL to PDF | Via `Uri` property in settings | Low |
| Global settings | `GlobalSettings` object | Low |
| Object settings | `ObjectSettings` object | Low |
| Merge PDFs | External library required | Low (native in IronPDF) |
| Watermark | External library required | Low (native in IronPDF) |
| Password protection | External library required | Low (native in IronPDF) |
| Custom margins | `MarginSettings` | Low |
| Headers / footers | `HeaderSettings` / `FooterSettings` | Medium — placeholder syntax differs |
| Concurrent rendering | Thread constraints from libwkhtmltopdf | Medium — review carefully |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| Open source requirement, basic HTML-to-PDF | DinkToPdf remains viable if stack is compatible |
| Modern CSS (Grid, Flexbox) critical to your templates | Test both renderers against your actual HTML |
| Need merge/split/watermark without a second library | IronPDF consolidates this; worth evaluating |
| Air-gapped environment, no commercial license approval | DinkToPdf (open source); or wkhtmltopdf CLI directly |

---

## Before you start

### Prerequisites

- .NET 6+ recommended
- NuGet access
- A dev copy of your codebase with DinkToPdf working
- Your existing PDF generation HTML templates (for render comparison testing)

### Find DinkToPdf references in your codebase

```bash
# Find all files using DinkToPdf
rg -l "DinkToPdf" --type cs

# Find converter instantiation
rg "SynchronizedConverter\|BasicConverter\|IConverter" --type cs -n

# Find settings objects
rg "HtmlToPdfDocument\|GlobalSettings\|ObjectSettings" --type cs -n

# Find using directives
rg "using DinkToPdf" --type cs -n
```

### Remove DinkToPdf, install IronPDF

```bash
# Remove DinkToPdf package
dotnet remove package DinkToPdf

# Install IronPDF
dotnet add package IronPdf

# Restore
dotnet restore
```

> **Note on native binaries:** If you manually copied `libwkhtmltopdf.so` or `.dll` into your project or Docker image, remove those references too. IronPDF ships its own Chromium binaries via NuGet — see the [license setup guide](https://ironpdf.com/how-to/license-keys/) for runtime configuration.

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (DinkToPdf — no license key, open source):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Extensions.DependencyInjection;

// DinkToPdf registered via DI in ASP.NET Core
var services = new ServiceCollection();
services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
// No license key needed — open source library
```

**After (IronPDF — license at startup):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// License configuration guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering; // for ChromePdfRenderOptions
```

### Step 3: Basic HTML-to-PDF

**Before:**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

var converter = new SynchronizedConverter(new PdfTools());

var doc = new HtmlToPdfDocument
{
    GlobalSettings = new GlobalSettings
    {
        ColorMode = ColorMode.Color,
        Orientation = Orientation.Portrait,
        PaperSize = PaperKind.A4,
        Out = "output.pdf"
    },
    Objects = {
        new ObjectSettings
        {
            HtmlContent = "<h1>Hello World</h1>",
            WebSettings = { DefaultEncoding = "utf-8" }
        }
    }
};

converter.Convert(doc);
```

**After:**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// Full guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API mapping tables

### Namespace mapping

| DinkToPdf | IronPDF | Notes |
|---|---|---|
| `DinkToPdf` | `IronPdf` | Core namespace |
| `DinkToPdf.Contracts` | `IronPdf.Rendering` | Settings / options |
| N/A | `IronPdf.Editing` | Manipulation features |

### Core class mapping

| DinkToPdf class | IronPDF class | Description |
|---|---|---|
| `SynchronizedConverter` | `ChromePdfRenderer` | Main conversion entry point |
| `HtmlToPdfDocument` | `ChromePdfRenderOptions` | Document configuration |
| `GlobalSettings` | `ChromePdfRenderOptions` | Global render settings |
| `ObjectSettings` | `ChromePdfRenderOptions` | Per-object / source settings |

### Document loading methods

| Operation | DinkToPdf | IronPDF |
|---|---|---|
| HTML string | `ObjectSettings.HtmlContent` | `renderer.RenderHtmlAsPdf(html)` |
| URL | `ObjectSettings.Page` (URL string) | `renderer.RenderUrlAsPdf(url)` |
| HTML file | `ObjectSettings.Page` (file:// path) | `renderer.RenderHtmlFileAsPdf(path)` |
| Existing PDF | Not supported | `PdfDocument.FromFile(path)` |

### Page operations

| Operation | DinkToPdf | IronPDF |
|---|---|---|
| Page count | Not accessible after render | `pdf.PageCount` |
| Paper size | `GlobalSettings.PaperSize` | `ChromePdfRenderOptions.PaperSize` |
| Orientation | `GlobalSettings.Orientation` | `ChromePdfRenderOptions.PaperOrientation` |
| Margins | `GlobalSettings.Margins` | `ChromePdfRenderOptions.Margin*` properties |

### Merge/split operations

| Operation | DinkToPdf | IronPDF |
|---|---|---|
| Merge | Not supported — external library | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | Not supported — external library | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

### 1. HTML to PDF

**Before (DinkToPdf):**
```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using System;

class HtmlToPdfExample
{
    static void Main()
    {
        var converter = new SynchronizedConverter(new PdfTools());

        var document = new HtmlToPdfDocument
        {
            GlobalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 15, Right = 15 },
                Out = "invoice.pdf",
                DPI = 180
            },
            Objects = {
                new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>",
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = null },
                    HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]" },
                    FooterSettings = { FontSize = 9, Center = "Confidential" }
                }
            }
        };

        converter.Convert(document);
        Console.WriteLine("Saved: invoice.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.MarginLeft = 15;
renderer.RenderingOptions.MarginRight = 15;

var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>"
);
pdf.SaveAs("invoice.pdf");
// Rendering options: https://ironpdf.com/how-to/rendering-options/
```

---

### 2. Merge PDFs

**Before (DinkToPdf — requires secondary library; PdfSharp shown):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;

class MergePdfsExample
{
    static void Main()
    {
        // DinkToPdf generates each PDF separately
        // Merging requires a separate library
        using var output = new PdfDocument();

        foreach (string path in new[] { "part1.pdf", "part2.pdf", "part3.pdf" })
        {
            using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            foreach (var page in input.Pages)
                output.AddPage(page);
        }

        output.Save("merged.pdf");
        Console.WriteLine("Merged 3 files into merged.pdf");
    }
}
```

**After (IronPDF native):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdfs = new[]
{
    PdfDocument.FromFile("part1.pdf"),
    PdfDocument.FromFile("part2.pdf"),
    PdfDocument.FromFile("part3.pdf")
};
var merged = PdfDocument.Merge(pdfs);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (DinkToPdf — no native watermark; iTextSharp workaround typical):**
```csharp
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;

class WatermarkExample
{
    static void Main()
    {
        // Generate PDF via DinkToPdf (omitted), then apply watermark
        using var reader = new PdfReader("generated.pdf");
        using var fs = new FileStream("watermarked.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs);

        for (int i = 1; i <= reader.NumberOfPages; i++)
        {
            var cb = stamper.GetOverContent(i);
            var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
            cb.BeginText();
            cb.SetFontAndSize(baseFont, 50);
            cb.SetColorFill(new BaseColor(211, 211, 211));
            cb.ShowTextAligned(Element.ALIGN_CENTER, "DRAFT", 300, 400, 45);
            cb.EndText();
        }

        Console.WriteLine("Watermarked: watermarked.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("generated.pdf");
var stamper = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronSoftware.Drawing.Color.LightGray,
    FontSize = 50,
    Opacity = 40,
    Rotation = 45,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};
pdf.ApplyStamp(stamper);
pdf.SaveAs("watermarked.pdf");
// Guide: https://ironpdf.com/how-to/custom-watermark/
```

---

### 4. Password protection

**Before (DinkToPdf — not supported natively; iTextSharp workaround):**
```csharp
using iTextSharp.text.pdf;
using System.IO;
using System.Text;

class PasswordExample
{
    static void Main()
    {
        // Generate via DinkToPdf, then encrypt with iTextSharp
        byte[] userPass = Encoding.ASCII.GetBytes("readonly");
        byte[] ownerPass = Encoding.ASCII.GetBytes("adminpass");

        using var reader = new PdfReader("generated.pdf");
        using var fs = new FileStream("secured.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs, '\0', false);
        stamper.SetEncryption(
            userPass,
            ownerPass,
            PdfWriter.ALLOW_PRINTING | PdfWriter.ALLOW_COPY,
            PdfWriter.ENCRYPTION_AES_128
        );
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Security;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("generated.pdf");
pdf.SecuritySettings.UserPassword = "readonly";
pdf.SecuritySettings.OwnerPassword = "adminpass";
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Rendering engine difference

DinkToPdf uses WebKit (via wkhtmltopdf). IronPDF uses Chromium. For most templates this produces visually similar output, but not identical. Teams with pixel-sensitive layouts (invoices, certificates, reports with precise table rendering) should run a visual comparison across all templates before cutting over. Treat this like a browser compatibility test.

### Page indexing

IronPDF uses 0-based page indexing for all manipulation operations. DinkToPdf doesn't expose a page model post-render, but if you're feeding output into other tools that use 1-based indexing, check the consumer side as well.

### DI registration pattern

In ASP.NET Core projects, DinkToPdf is typically registered as `IConverter` via DI. IronPDF doesn't follow the same DI registration pattern. You'll need to update service registration and constructor injection accordingly:

```csharp
// DinkToPdf pattern in Startup.cs or Program.cs
services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

// IronPDF pattern — license once, instantiate renderer as needed
// (or register ChromePdfRenderer as singleton if appropriate for your workload)
IronPdf.License.LicenseKey = "...";
services.AddSingleton<ChromePdfRenderer>();
```

### wkhtmltopdf string options

DinkToPdf exposes some options as strings that match wkhtmltopdf CLI flags. These don't have direct equivalents in IronPDF's API. Map feature by feature, not flag by flag.

---

## Performance considerations

### Renderer instance reuse

```csharp
// Reuse renderer for batch work
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

foreach (var html in templates)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{Guid.NewGuid()}.pdf");
}
```

### Concurrent rendering

Unlike DinkToPdf which wraps a non-thread-safe native library behind `SynchronizedConverter` (serializing requests), IronPDF supports concurrent rendering with separate renderer instances:

```csharp
// Each task gets its own renderer — safe for parallel work
await Task.WhenAll(htmlBatch.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    await pdf.SaveAsAsync($"{Guid.NewGuid()}.pdf");
}));
// Async docs: https://ironpdf.com/how-to/async/
```

### Edge cases

- **Custom fonts:** DinkToPdf on Linux used system fonts. IronPDF's Chromium renderer also uses system fonts. Verify font availability in your target Docker image.
- **Local file references:** `file://` paths work in both libraries, but path resolution can differ by target OS — test on the deployment platform.
- **Large documents:** Memory consumption profile differs between WebKit and Chromium. Monitor RSS in production for the first week.

---

## Migration checklist

### Pre-migration

- [ ] Find all DinkToPdf usages: `rg "DinkToPdf" --type cs`
- [ ] Inventory all HTML templates for render comparison testing
- [ ] Check if secondary PDF library (PdfSharp, iTextSharp) is only there to supplement DinkToPdf — can it be removed?
- [ ] Verify IronPDF target framework compatibility
- [ ] Confirm commercial license requirements with your procurement process
- [ ] Note all `GlobalSettings` and `ObjectSettings` properties currently in use
- [ ] Identify ASP.NET DI registration points for `IConverter`
- [ ] Set up IronPDF trial license for dev environment

### Code migration

- [ ] Remove `DinkToPdf` NuGet package
- [ ] Remove native `libwkhtmltopdf` binaries from Docker images and deployment scripts
- [ ] Install `IronPdf` NuGet package
- [ ] Replace `using DinkToPdf` imports
- [ ] Replace `SynchronizedConverter` with `ChromePdfRenderer`
- [ ] Replace `HtmlToPdfDocument` + `GlobalSettings` + `ObjectSettings` with `ChromePdfRenderOptions`
- [ ] Update ASP.NET DI registration
- [ ] Replace HTML-to-PDF calls with `renderer.RenderHtmlAsPdf()`
- [ ] Remove secondary merge/watermark/encrypt libraries if only used to supplement DinkToPdf
- [ ] Add IronPDF license key to config (appsettings, environment variable)

### Testing

- [ ] Render each HTML template and visually compare DinkToPdf vs IronPDF output
- [ ] Pay particular attention to: tables, Flexbox, custom fonts, page breaks
- [ ] Verify page counts match expectations
- [ ] Test headers and footers on multi-page documents
- [ ] Test merge and split on real document sets
- [ ] Test password protection (open with correct/incorrect credentials)
- [ ] Load test: concurrent rendering at expected peak

### Post-migration

- [ ] Remove `libwkhtmltopdf` from Docker base images
- [ ] Remove DinkToPdf environment variables / config entries
- [ ] Monitor memory usage in first production week
- [ ] Update runbooks to reflect new library and any changed error modes

---

## Where to Go From Here

The main edge cases teams hit in this migration tend to be around render fidelity (WebKit vs Chromium), the DI registration change in ASP.NET projects, and discovering that native binary management was more intertwined with their Docker pipeline than they expected.

**What edge cases did you hit that this article didn't cover?** Particularly interested in teams running DinkToPdf on Alpine Linux or in multi-stage Docker builds — that's where native dependency management tends to get interesting.

