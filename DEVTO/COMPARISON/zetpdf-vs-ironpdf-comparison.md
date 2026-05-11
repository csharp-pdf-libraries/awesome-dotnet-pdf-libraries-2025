---
title: "ZetPDF vs IronPDF: what the docs do not tell you"
published: false
tags: dotnet, csharp, pdf, comparison
canonical_url: https://ironsoftware.com/csharp/pdf/blog/compare-to-competitors/
---

## Understanding IronPDF

[IronPDF](https://ironpdf.com) is designed for headless PDF generation in server environments. Install via `Install-Package IronPdf`, no UI dependencies required. The `ChromePdfRenderer` class converts HTML to PDF using an embedded Chromium engine, with async APIs that suit ASP.NET Core, Azure Functions, AWS Lambda, and Docker containers.

Operationally, IronPDF supports `async/await`, parallel batch processing, and stateless reuse of a single renderer instance. It is designed for scenarios where PDFs are generated programmatically and streamed to clients rather than displayed in desktop controls.

## What ZetPDF Actually Is

ZetPDF ([zetpdf.com](https://zetpdf.com)) is a commercial .NET PDF SDK that closely resembles [PDFsharp](https://www.pdfsharp.net/) in API shape — close enough that the PDFsharp community has [questioned whether it is a closed fork](https://forum.pdfsharp.net/viewtopic.php?f=2&t=3841). The vendor markets it as a proprietary 100%-managed-code engine. Practical points relevant to a migration decision:

- **Distribution**: ZetPDF is **not on NuGet**. Installation is a ZIP download from `https://zetpdf.com/download/`, then a manual DLL reference.
- **Activity**: The project shows no public release activity since 2021.
- **API shape**: The published feature list ([zetpdf.com/net-pdf-sdk](https://zetpdf.com/net-pdf-sdk/)) covers low-level PDF operations — page model, `XGraphics` drawing, AES256 encryption, form fields, annotations, text extraction. **No native HTML-to-PDF or URL-to-PDF converter** is documented.
- **Pricing**: Perpetual licensing — $299 single-project, $449 multi-project, $599 OEM (vendor site at time of writing; verify current pricing on the ZetPDF site).

The architectural consequence: like PDFsharp, ZetPDF is a coordinate-based drawing API. You position every element with `XPoint(x, y)`. There is no HTML layout engine, no CSS, no JavaScript runtime.

## Feature Comparison Overview

| Aspect | ZetPDF | IronPDF |
|--------|--------|---------|
| **Distribution** | ZIP download (no NuGet) | NuGet (`IronPdf`) |
| **API style** | Coordinate-based `XGraphics` drawing | HTML/CSS via Chromium |
| **HTML-to-PDF** | Not documented in feature list | Yes (core feature) |
| **URL-to-PDF** | Not documented in feature list | Yes |
| **Last public release activity** | 2021 (per vendor / third-party listings) | Active, frequent releases |
| **Lineage** | Resembles PDFsharp closely | Chromium-based |
| **Pricing model** | Perpetual ($299 / $449 / $599) | Subscription / custom |

---

## Code Comparison: Single PDF Generation

### ZetPDF — coordinate-based drawing

ZetPDF does not expose a native HTML-to-PDF entry point, so generating a document means drawing text and shapes against an `XGraphics` surface, page by page:

```csharp
// ZetPDF ships as a DLL via SDK ZIP from https://zetpdf.com/download/
using ZetPDF.Document;
using ZetPDF.Drawing;

var document = new PdfDocument();
var page = document.AddPage();
var gfx = XGraphics.FromPdfPage(page);
var titleFont = new XFont("Verdana", 20, XFontStyle.Bold);

gfx.DrawString("Performance Test Document", titleFont,
    XBrushes.Black, new XPoint(40, 60));
// Each line, header, and table cell needs its own coordinates.

document.Save("output.pdf");
```

Notes on the architecture this implies:

1. Layout is manual — text wrapping, page breaks, alignment, and tables are all the developer's responsibility.
2. There is no HTML/CSS pipeline, so styling existing web templates is out of scope.
3. Concurrency is bounded by your own code; the drawing API itself is synchronous.

### IronPDF — headless HTML rendering

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

public class IronPdfGenerator
{
    public async Task<byte[]> GeneratePdfFromHtmlAsync(string html)
    {
        var stopwatch = Stopwatch.StartNew();

        var renderer = new ChromePdfRenderer();
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

        stopwatch.Stop();
        Console.WriteLine($"Generation time: {stopwatch.ElapsedMilliseconds}ms");

        return pdf.BinaryData;
    }
}

// Usage
var generator = new IronPdfGenerator();
string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial; padding: 20px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { border: 1px solid #ddd; padding: 8px; }
    </style>
</head>
<body>
    <h1>Performance Test Document</h1>
    <table>
        <tr><th>Metric</th><th>Value</th></tr>
        <tr><td>Throughput</td><td>High</td></tr>
    </table>
</body>
</html>";

byte[] pdf = await generator.GeneratePdfFromHtmlAsync(html);
```

The Chromium-based renderer is asynchronous and reusable across requests. Learn more about [Chrome rendering engine architecture](https://ironpdf.com/how-to/ironpdf-2021-chrome-rendering-engine-eap/).

---

## Code Comparison: Batch Processing

### ZetPDF — sequential drawing

With a coordinate-based API, batch generation is essentially a loop. There is no async surface and no inherent parallelism — you can run loops in parallel threads, but each document still pays the per-call drawing cost:

```csharp
using ZetPDF.Document;
using ZetPDF.Drawing;
using System.Collections.Generic;

public class ZetPdfBatchProcessor
{
    public List<byte[]> ProcessBatch(List<string> bodyTexts)
    {
        var results = new List<byte[]>();
        var font = new XFont("Verdana", 12);

        foreach (var body in bodyTexts)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            gfx.DrawString(body, font, XBrushes.Black, new XPoint(40, 60));

            using var ms = new System.IO.MemoryStream();
            document.Save(ms, false);
            results.Add(ms.ToArray());
        }

        return results;
    }
}
```

### IronPDF — parallel async processing

```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class IronPdfBatchProcessor
{
    public async Task<List<byte[]>> ProcessBatchAsync(List<string> htmlDocuments)
    {
        var stopwatch = Stopwatch.StartNew();
        var renderer = new ChromePdfRenderer();

        var tasks = htmlDocuments.Select(async html =>
        {
            using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
            return pdf.BinaryData;
        });

        var results = (await Task.WhenAll(tasks)).ToList();

        stopwatch.Stop();
        Console.WriteLine($"Batch time: {stopwatch.Elapsed.TotalSeconds:F1}s");

        return results;
    }
}

// Usage
var processor = new IronPdfBatchProcessor();
var htmlDocs = Enumerable.Range(1, 100)
    .Select(i => $@"
        <!DOCTYPE html>
        <html><body>
            <h1>Document #{i}</h1>
            <p>Content for document {i}</p>
        </body></html>")
    .ToList();

var pdfs = await processor.ProcessBatchAsync(htmlDocs);
```

`ChromePdfRenderer` is safe to reuse, and `Task.WhenAll` scales with CPU cores. See [PDF generation settings](https://ironpdf.com/examples/pdf-generation-settings/) for tuning options.

---

## Server Deployment

### ZetPDF in ASP.NET Core

ZetPDF's 100%-managed-code design does not impose UI-thread or STAThread requirements — the drawing API is plain managed code and runs inside ASP.NET request handlers without special apartment threading. The friction in a server context is the API surface itself:

```csharp
using Microsoft.AspNetCore.Mvc;
using ZetPDF.Document;
using ZetPDF.Drawing;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    [HttpPost]
    public IActionResult GeneratePdf([FromBody] string bodyText)
    {
        var document = new PdfDocument();
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        gfx.DrawString(bodyText, new XFont("Verdana", 12),
            XBrushes.Black, new XPoint(40, 60));

        using var ms = new MemoryStream();
        document.Save(ms, false);
        return File(ms.ToArray(), "application/pdf", "document.pdf");
    }
}
```

The deployment-level questions to verify against current ZetPDF documentation:

1. **Server licensing**: Confirm whether the perpetual license covers server-side / per-server deployment for your scenario.
2. **Cross-platform**: Verify .NET version support and whether the DLL runs on Linux containers (typical PDFsharp-derived stacks run on .NET, but exact support matrix varies).
3. **No HTML pipeline**: If your input is HTML or web templates, you will need a separate renderer in front of ZetPDF.

### IronPDF in ASP.NET Core

```csharp
using Microsoft.AspNetCore.Mvc;
using IronPdf;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    private readonly ChromePdfRenderer _renderer;

    public PdfController()
    {
        // Renderer instances are reusable across requests
        _renderer = new ChromePdfRenderer();
    }

    [HttpPost]
    public async Task<IActionResult> GeneratePdf([FromBody] string html)
    {
        using var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return File(pdf.BinaryData, "application/pdf", "document.pdf");
    }
}
```

IronPDF targets server environments natively: async APIs, container-friendly footprint, and a documented [Azure / AWS / Docker deployment story](https://ironpdf.com/docs/).

---

## API Mapping Reference

| ZetPDF | IronPDF | Notes |
|--------|---------|-------|
| `new PdfDocument()` | `new ChromePdfRenderer()` | Create renderer |
| `document.AddPage()` | Automatic | Pages flow from HTML |
| `XGraphics.FromPdfPage(page)` | N/A | Use HTML/CSS instead |
| `gfx.DrawString(...)` | `<p>`, `<h1>`, etc. | HTML text elements |
| `gfx.DrawImage(...)` | `<img>` tag | HTML images |
| `gfx.DrawLine(...)` | CSS borders / `<hr>` | HTML structure |
| `gfx.DrawRectangle(...)` | CSS `border` + `<div>` | HTML boxes |
| `new XFont(...)` | CSS `font-family` | Web fonts supported |
| `XBrushes.Black` | CSS `color` | Full color support |
| `document.Save(...)` | `pdf.SaveAs(...)` | Save to file |
| `PdfReader.Open(...)` | `PdfDocument.FromFile(...)` | Load existing PDF |

---

## Comprehensive Feature Comparison

| Feature Category | ZetPDF | IronPDF |
|------------------|--------|---------|
| **Distribution** |
| NuGet package | Not available | `IronPdf` |
| Installation | SDK ZIP download | `dotnet add package IronPdf` |
| Last public release activity | 2021 | Active |
| **Content Creation** |
| HTML to PDF | Not documented in feature list | Yes (core feature) |
| URL to PDF | Not documented in feature list | Yes |
| Coordinate drawing (`XGraphics`) | Yes (primary surface) | N/A |
| Automatic layout / page breaks | Manual | Yes |
| CSS / JavaScript | Not applicable | Full CSS3 / modern JS |
| **PDF Operations** |
| Create PDFs | Yes (manual layout) | Yes |
| Merge PDFs | Manual page-loop (no one-line helper) | `PdfDocument.Merge(...)` |
| Text extraction | Yes | Yes |
| Annotations | Yes | Yes |
| AES256 encryption | Yes (document-level settings) | Yes (`SecuritySettings`) |
| Form fields | Yes | Yes |
| Digital signatures | Verify in current vendor docs | Yes |
| **Architecture** |
| Engine | Managed-code, PDFsharp-style | Chromium-based |
| Async/await surface | No | Yes |
| Concurrency model | Caller-managed | Parallel-friendly |
| Container / cloud deployment | Possible (verify supported runtimes) | Documented and supported |

---

## Installation Comparison

**ZetPDF:**

```bash
# Download SDK ZIP from https://zetpdf.com/download/
# Extract and reference ZetPDF.dll from your csproj manually.
```

```csharp
using ZetPDF.Document;
using ZetPDF.Drawing;

var document = new PdfDocument();
var page = document.AddPage();
var gfx = XGraphics.FromPdfPage(page);
gfx.DrawString("Hello, ZetPDF",
    new XFont("Verdana", 20, XFontStyle.Bold),
    XBrushes.Black, new XPoint(40, 100));
document.Save("hello.pdf");
```

**IronPDF:**

```bash
dotnet add package IronPdf
```

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(@"
    <html><body>
        <h1>Generated PDF</h1>
        <p>Content here</p>
    </body></html>");
pdf.SaveAs("output.pdf");
```

---

## Conclusion

ZetPDF and IronPDF solve adjacent but distinct problems. ZetPDF is a PDFsharp-style commercial SDK: a coordinate-based, managed-code engine for programmatically constructing PDFs from primitives — pages, fonts, brushes, and `XGraphics` calls. IronPDF is a Chromium-based HTML-to-PDF generation engine, optimised for headless server use with async APIs and parallel batch rendering.

Teams stay on ZetPDF when:

- The application already builds PDFs from low-level drawing primitives and the perpetual licensing model fits.
- HTML / web-template input is not part of the requirement.
- Existing PDFsharp-style code is the starting point and a lateral move is the least disruptive option.

Migration from ZetPDF to IronPDF tends to make sense when:

- The input is HTML, a URL, or a templated web view rather than a coordinate plan.
- Server-side throughput, async APIs, or containerised deployment are first-class requirements.
- You want NuGet-based distribution and an actively maintained release cadence.
- You need batch processing that scales horizontally.

The architectural difference is the deciding factor: coordinate drawing vs. an HTML rendering pipeline. Once the input format is HTML, the Chromium-based pipeline removes the manual layout work that a PDFsharp-style API requires. The [ChromePdfRenderer API](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderer.html/) is designed for stateless operation in ASP.NET Core, Azure Functions, AWS Lambda, and Docker containers.

Are you currently using ZetPDF or evaluating it for a new project? If you have data on its current maintenance cadence or server-side usage, share it in the comments.

**Related Resources:**

- [HTML String to PDF Conversion Guide](https://ironpdf.com/how-to/html-string-to-pdf/)
- [IronPDF Performance Tuning Documentation](https://ironpdf.com/examples/pdf-generation-settings/)
- [ZetPDF to IronPDF migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-zetpdf-to-ironpdf/)
