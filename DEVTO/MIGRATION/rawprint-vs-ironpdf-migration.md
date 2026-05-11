---
title: "RawPrint .NET to IronPDF: less config, same output"
published: false
tags: dotnet, csharp, pdf, migration
---

Setting up raw printing in .NET sits in a quieter corner of the ecosystem. RawPrint (frogmorecs/RawPrint, NuGet package `RawPrint`, v0.5.0, last released September 2019 and now unlisted/legacy on nuget.org) is a thin P/Invoke wrapper over `winspool.Drv` that ships a byte stream straight to the Windows print spooler with the RAW datatype. It does exactly one thing — push bytes the printer firmware already understands (ESC/POS, ZPL, PCL, PostScript, or an already-rendered PDF for printers with native PDF firmware). It does not generate PDFs and never claimed to.

That clarity matters. RawPrint and IronPDF solve different problems. The honest framing for most teams is "complement," not "replace": IronPDF generates and manipulates PDFs; RawPrint pushes raw bytes to a Windows spooler. If your current pipeline produces a PDF with one library and then hands it to RawPrint, IronPDF can collapse the create half into a single in-process step — and on Windows it can also handle the print half via `pdf.Print()`. If your RawPrint usage is genuinely raw (ESC/POS to a thermal receipt printer, ZPL to a Zebra label printer), keep RawPrint where it is.

This article is for the first case: teams whose requirements have shifted from "send pre-formed bytes to a named printer" toward "generate a PDF file (and optionally print it)."

---

## Why Migrate (Without Drama)

Teams collapsing a create-then-RawPrint workflow into IronPDF usually have at least a few of these triggers:

1. **Windows-only dependency** — RawPrint wraps `winspool.Drv`; cross-platform or Linux deployment is not in scope.
2. **Package status** — the `RawPrint` NuGet package is unlisted/legacy and was last released in September 2019.
3. **Shifted requirements** — what started as "print this document" has become "generate a PDF file and archive/email/display it."
4. **No HTML input** — RawPrint sends pre-formed bytes; HTML-to-PDF requires a separate rendering pipeline.
5. **No PDF manipulation** — merge, watermark, security, and page extraction are not in RawPrint's scope.
6. **Printer availability dependency** — RawPrint needs a named printer to resolve; development and test environments must have matching printer setup.
7. **Spooler-side failure modes** — Win32 printer API errors typically surface through the spooler rather than as exceptions you can catch directly.
8. **Docker incompatibility** — the Windows print spooler is not available in Linux containers.
9. **No intermediate PDF artifact** — raw print jobs go straight to the printer; there is no PDF for review or archival unless you produce one separately.
10. **Driver-side variation** — PCL/PostScript output can behave differently across printer firmware; a PDF is model-independent for downstream consumers.

### Comparison Table

| Aspect | RawPrint | IronPDF |
|---|---|---|
| Focus | Send raw bytes to Windows printers | PDF generation and manipulation |
| Pricing | Free (MIT) — package unlisted/legacy on nuget.org | Commercial license — see ironsoftware.com |
| API Style | Win32 wrapper; printer name + file/stream | .NET objects; HTML, URL, or existing PDF input |
| Learning Curve | Low for raw printing | Low for .NET developers |
| HTML Rendering | Not in scope | Embedded Chromium |
| Page Indexing | N/A | 0-based |
| Cross-Platform | Windows only | Windows, Linux, macOS, Docker |
| Namespace | `RawPrint` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | RawPrint | IronPDF Equivalent | Complexity |
|---|---|---|---|
| Push bytes to a Windows spooler | `IPrinter.PrintRawFile` / `PrintRawStream` | Not in scope — IronPDF does not stream RAW bytes | N/A |
| Generate PDF from HTML | Not in scope | `ChromePdfRenderer.RenderHtmlAsPdf()` | Low |
| Generate PDF from URL | Not in scope | `ChromePdfRenderer.RenderUrlAsPdf()` | Low |
| Print a generated PDF (Windows) | Possible if printer accepts PDF natively | `pdf.Print()` | Low |
| Merge PDFs | Not in scope | `PdfDocument.Merge()` | Low |
| Watermark | Not in scope | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Not in scope | `pdf.SecuritySettings` | Low |
| Cross-platform support | No — Windows only | Yes — Linux/macOS/Docker | Low (inherit) |
| Save PDF to file/stream | Not in scope | `pdf.SaveAs()` / `pdf.BinaryData` | Low |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Still need to push ESC/POS, ZPL, or PCL bytes to a printer | Keep RawPrint — IronPDF is not a substitute for RAW byte channels |
| Need PDF files for email, archive, or web display | Switch to IronPDF — RawPrint does not produce PDFs |
| Need to create a PDF and then print it on Windows | Collapse into IronPDF: `RenderHtmlAsPdf()` + `pdf.Print()` |
| Moving from Windows to Linux/Docker deployment | Switch — RawPrint relies on the Windows spooler |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)
- Clarity on whether RAW byte channels are still needed or whether PDF generation replaces them

### Find All RawPrint References

```bash
# Find RawPrint usage — real public API is PrintRawFile / PrintRawStream
rg "using RawPrint" --type cs
rg "PrintRawFile|PrintRawStream|IPrinter" --type cs -n

# Find any hand-rolled Win32 printer P/Invoke that lived alongside RawPrint
rg "DllImport.*winspool|DOCINFO|StartDocPrinter|WritePrinter" --type cs -n

# Find printer-name configuration
rg "printerName|PrinterName|GetDefaultPrinter" --type cs -n

# Check project files
grep -r "RawPrint" *.csproj **/*.csproj 2>/dev/null
```

### Uninstall / Install

```bash
# Remove RawPrint (only if you no longer need RAW byte channels)
dotnet remove package RawPrint

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 2 — Namespace Swap

**Before:**
```csharp
using RawPrint;
using System.IO;
```

**After:**
```csharp
using IronPdf;
```

### Step 3 — Replace "create elsewhere, then RawPrint" with IronPDF

**Before (other library produces PDF; RawPrint pushes it to the spooler):**
```csharp
using RawPrint;
using System.IO;

class Program
{
    static void Main()
    {
        // RawPrint cannot create PDFs. The bytes have to come from somewhere else.
        byte[] pdfBytes = SomeOtherLibrary.CreatePdf(reportData);
        File.WriteAllBytes("temp.pdf", pdfBytes);

        IPrinter printer = new Printer();
        printer.PrintRawFile("HP LaserJet", "temp.pdf", false);
    }
}
```

**After (IronPDF generates the PDF; print is one extra line on Windows):**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Document</h1><p>Content here.</p></body></html>"
);

// Keep the PDF as a file (for email, archive, API):
pdf.SaveAs("document.pdf");

// Or print it on Windows via the OS print system:
// pdf.Print();

Console.WriteLine($"Generated document.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

Note: IronPDF's `pdf.Print()` hands the document to the operating system's print system. It does not stream RAW bytes to the spooler. If your printer requires the RAW datatype (ESC/POS, ZPL, certain PCL workflows), that is RawPrint's lane and IronPDF cannot stand in for it — keep RawPrint for that channel.

---

## API Mapping Tables

### Namespace Mapping

| RawPrint | IronPDF | Notes |
|---|---|---|
| `RawPrint` | `IronPdf` | Core namespace |
| `System.Runtime.InteropServices` (hand-rolled P/Invoke alongside RawPrint) | N/A — removed | No Win32 P/Invoke needed for PDF generation |
| N/A | `IronPdf.Editing` | Watermark / stamp |

### Core Class Mapping

| RawPrint Concept | IronPDF Class | Description |
|---|---|---|
| `IPrinter` / `Printer` | N/A — different purpose | RawPrint pushes bytes; IronPDF generates PDFs |
| Byte array (pre-formed document) | `ChromePdfRenderer` | Generates PDF from HTML |
| Printer name string | N/A (for generation); `pdf.Print()` uses OS printers | IronPDF outputs to file, stream, or OS spooler |
| N/A | `PdfDocument` | PDF object — save, merge, manipulate |

### Document Loading and Output Methods

| Operation | RawPrint | IronPDF |
|---|---|---|
| Generate PDF from HTML | Not in scope | `renderer.RenderHtmlAsPdf(html)` |
| Generate PDF from URL | Not in scope | `renderer.RenderUrlAsPdf(url)` |
| Load existing PDF | N/A (path strings only) | `PdfDocument.FromFile(path)` |
| Print existing PDF (Windows) | `printer.PrintRawFile(name, path, paused)` | `PdfDocument.FromFile(path).Print()` |
| Save PDF bytes | N/A | `pdf.BinaryData` |

### Page Operations

| Operation | RawPrint | IronPDF |
|---|---|---|
| Page count | N/A | `pdf.PageCount` |
| Remove page | N/A | `pdf.RemovePages(index)` |
| Extract text | N/A | `pdf.ExtractAllText()` |

### Merge / Split Operations

| Operation | RawPrint | IronPDF |
|---|---|---|
| Merge | Not in scope | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not in scope | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF (the half RawPrint never owned)

**Before (RawPrint pushing pre-formed bytes; PDF generation happens elsewhere):**
```csharp
using RawPrint;
using System;
using System.IO;

class PrintWorkflowBefore
{
    static void Main()
    {
        // RawPrint's real API is PrintRawFile / PrintRawStream on IPrinter.
        // It cannot produce PDF bytes — that step has to happen in another library
        // (or a CLI tool like wkhtmltopdf shelled out from C#).
        var html = "<html><body><h1>Delivery Note #12345</h1></body></html>";

        byte[] pdfBytes = SomeOtherLibrary.GeneratePdf(html);
        File.WriteAllBytes("temp.pdf", pdfBytes);

        IPrinter printer = new Printer();
        printer.PrintRawFile("DefaultPrinter", "temp.pdf", false);
    }
}
```

**After (IronPDF generates the PDF in-process; print is optional):**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, sans-serif; padding: 20px; }
        .header { font-size: 20px; font-weight: bold; border-bottom: 2px solid #333; }
        .detail { margin-top: 20px; font-size: 12px; }
    </style>
    </head>
    <body>
        <div class='header'>Delivery Note #12345</div>
        <div class='detail'>
            <p>Customer: Acme Corp</p>
            <p>Items: Widget x10, Gadget x5</p>
        </div>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("delivery-note.pdf");

// Optional: print on Windows via the OS print system
// pdf.Print();

Console.WriteLine($"Generated delivery-note.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (RawPrint cannot merge; merge happened before the print job):**
```csharp
using System;

class MergeBefore
{
    static void Main()
    {
        // RawPrint scope is "push bytes to a Windows spooler" — merge is out of scope.
        // Pipelines that needed both used a separate PDF library for merge, then
        // handed the merged file to RawPrint:
        //
        //   var pdf1 = GenerateSomehow("section1.html");
        //   var pdf2 = GenerateSomehow("section2.html");
        //   var merged = SomePdfLib.Merge(pdf1, pdf2);
        //   new Printer().PrintRawFile("Printer", "merged.pdf", false);

        Console.WriteLine("Merge happened in a separate library before the print step");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

var pdf1 = renderer.RenderHtmlAsPdf("<html><body><h1>Section 1: Summary</h1></body></html>");
var pdf2 = renderer.RenderHtmlAsPdf("<html><body><h1>Section 2: Details</h1></body></html>");

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("full-document.pdf");

Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (RawPrint cannot watermark; mark must be in the source document):**
```csharp
using System;

class WatermarkBefore
{
    static void Main()
    {
        // RawPrint just ships bytes. The mark has to exist in the document already —
        // typically as CSS in the source HTML before the PDF is rendered by some
        // other library.
        var html = @"
            <html>
            <head>
            <style>
                body::after {
                    content: 'COPY';
                    position: fixed; top: 50%; left: 50%;
                    transform: translate(-50%, -50%) rotate(-45deg);
                    font-size: 80px; color: rgba(200,0,0,0.15);
                    pointer-events: none;
                }
            </style>
            </head>
            <body><h1>Document</h1></body>
            </html>";

        Console.WriteLine("Pre-print watermarks live in the source HTML, before bytes go to the spooler");
    }
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Editing;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Document</h1></body></html>"
);

// Post-render watermark — applied consistently across all pages
// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "COPY",
    FontColor = IronSoftware.Drawing.Color.Red,
    Opacity = 15,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("copy-watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (RawPrint scope does not include PDF encryption):**
```csharp
using System;

class PasswordBefore
{
    static void Main()
    {
        // Password protection is a PDF file feature. RawPrint pushes bytes to the
        // Windows spooler; PDF encryption is applied to the file by whichever library
        // produced it, not by the spooler call.

        Console.WriteLine("Password protection is a PDF file feature — applied before the print step, if at all");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Secured Report</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured-report.pdf");
Console.WriteLine("Saved secured-report.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Troubleshooting Common Migration Issues

### "System.Runtime.InteropServices.COMException" After Removing RawPrint

**Symptom:** After swapping the create-then-print pipeline to IronPDF, COMException or P/Invoke errors appear in unrelated code paths.

**Cause:** Hand-rolled Win32 printer P/Invoke declarations sometimes lived alongside RawPrint, not inside it. Those declarations may still be in your code base after the RawPrint package is removed.

**Resolution:**
```bash
# Find remaining Win32 printer P/Invoke declarations
rg "DllImport.*winspool|OpenPrinter|ClosePrinter" --type cs -n

# Find any leftover RawPrint references
rg "using RawPrint|PrintRawFile|PrintRawStream|IPrinter" --type cs -n

# Identify and remove or isolate from the PDF generation path
```

### "IronPDF renders on Windows but throws on Linux"

**Symptom:** IronPDF renders PDFs correctly on Windows (the environment RawPrint was happy in) but fails in a Linux Docker container.

**Root cause:** Common environment differences between Windows dev and Linux CI.

**Resolution:**
```dockerfile
# IronPDF on Linux typically needs:
# - A 64-bit base image (Debian-derived works well; Alpine is not supported out of the box)
# - System libraries Chromium depends on

FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Common Linux dependencies for Chromium-based rendering
RUN apt-get update && apt-get install -y \
    libnss3 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libgbm1 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

See [IronPDF on Linux](https://ironpdf.com/how-to/installation/) for the current supported list.

### "Font appears as fallback / square boxes in PDF"

**Symptom:** PDFs generated by IronPDF show font fallbacks where custom fonts were specified.

**Cause:** Custom fonts available on the Windows host are not present in the Linux container image.

**Resolution:**
```csharp
using IronPdf;

// Option 1: Web-safe fonts that ship with most environments
var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, Helvetica, sans-serif; }
    </style>
    </head>
    <body>...</body>
    </html>";

// Option 2: Pull fonts from a CDN at render time
var htmlWithCdnFont = @"
    <html>
    <head>
    <link href='https://fonts.googleapis.com/css2?family=Open+Sans' rel='stylesheet'>
    <style>body { font-family: 'Open Sans', sans-serif; }</style>
    </head>
    <body>...</body>
    </html>";

// Option 3: Embed fonts as base64 data URIs for offline/airgapped environments
// var fontBase64 = Convert.ToBase64String(File.ReadAllBytes("CustomFont.woff2"));
// then use: @font-face { src: url(data:font/woff2;base64,...) }

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlWithCdnFont);
```

### "Leftover printer-helper classes after removing RawPrint"

**Symptom:** After removing the RawPrint package, there are large blocks of Win32 P/Invoke declarations and helper classes that were sitting alongside RawPrint and are not obvious to delete safely.

**Resolution:**
```bash
# Find Win32 printer API declarations
rg "DllImport.*winspool|DllImport.*kernel32.*printer|struct DOCINFO" --type cs -n

# Find any class-level printer helper classes
rg "class.*Printer|class.*Print|class.*Raw" --type cs -n

# These can be safely deleted once no call sites reference them
rg "PrintRawFile|PrintRawStream|IPrinter" --type cs -n
# If count is 0, the helpers are safe to delete
```

### "PDF looks different from the original print output"

**Symptom:** The IronPDF-generated PDF does not match what was previously printed via the create-then-RawPrint pipeline.

**Root cause:** The original print output was generated by a separate system (often SAP, an ERP, or a report server) and sent through the spooler as PCL/PostScript or as a PDF that library produced. IronPDF generates PDFs from HTML; the visual output depends on the HTML template, not on reverse-engineering the original printer data.

**Resolution:** This is a template-design problem, not a bug. You need an HTML template that produces the same visual layout as the original document. Start by screenshotting the original print output and recreating the layout in HTML/CSS.

---

## Critical Migration Notes

### This Is a Workflow Change, Not Just a Library Swap

RawPrint and IronPDF solve different problems. The migration is not just swapping one method call for another — it is replacing a "generate-document-elsewhere, send-bytes-to-printer" workflow with a "generate-PDF-as-file-or-stream" workflow. That change has implications for:

- **Where documents are generated** — IronPDF generates in-process; RawPrint received pre-formed bytes from somewhere else.
- **Output destination** — IronPDF outputs a file or stream; `pdf.Print()` on Windows hands the document to the OS spooler, not RAW bytes to a named printer.
- **Windows-only assumption** — RawPrint runs on Windows only; if you are moving to Linux, the print step has to go through whatever Linux-side print infrastructure you have (CUPS, network MFP queues, etc.).

If you still need raw byte channels (ESC/POS to a thermal receipt printer, ZPL to a Zebra label printer, hand-built PCL/PostScript), keep RawPrint where it is. IronPDF and RawPrint can coexist in the same application — use IronPDF for the PDF-generation half and RawPrint for the RAW byte half.

### P/Invoke Cleanup

Hand-rolled Win32 printer P/Invoke declarations sometimes lived alongside RawPrint rather than inside it. After migration, those can usually be removed:

```bash
# Find P/Invoke declarations that may no longer have call sites
rg "DllImport.*winspool|AddPrinter|StartDocPrinter|WritePrinter|EndDocPrinter|ClosePrinter" --type cs -n

# Verify each has no remaining call sites before deleting
```

### Cross-Platform Benefit

IronPDF runs on Windows, Linux, and macOS. Moving PDF generation to IronPDF enables Linux/Docker deployment that was not in scope for RawPrint's Windows-spooler-only design.

---

## Performance Considerations

### Renderer Reuse for High Volume

```csharp
using IronPdf;
using System.Collections.Generic;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// For high-volume document generation, reuse the renderer instance
var renderer = new ChromePdfRenderer();

var htmlDocuments = new List<string>
{
    "<html><body><h1>Delivery Note #001</h1></body></html>",
    "<html><body><h1>Delivery Note #002</h1></body></html>",
};

for (int i = 0; i < htmlDocuments.Count; i++)
{
    using var pdf = renderer.RenderHtmlAsPdf(htmlDocuments[i]);
    pdf.SaveAs($"note_{i:D3}.pdf");
}
```

### MemoryStream for In-Memory Processing

```csharp
using IronPdf;
using System.IO;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// If the downstream workflow needs bytes (email attachment, API response, S3 upload):
var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Hello</h1></body></html>");

var bytes = pdf.BinaryData;
// Use bytes: attach to email, upload to S3, return from API endpoint
```

### Parallel Generation

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// https://ironpdf.com/examples/parallel/
var htmlJobs = Enumerable.Range(1, 10)
    .Select(i => $"<html><body><h1>Delivery Note #{i:D4}</h1></body></html>")
    .ToArray();

var pdfs = await Task.WhenAll(htmlJobs.Select(async html =>
{
    var r = new ChromePdfRenderer();
    return await r.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Generated {pdfs.Length} PDFs");
foreach (var pdf in pdfs) pdf.Dispose();
```

---

## Migration Checklist

### Pre-Migration
- [ ] Identify all RawPrint usage (`rg "using RawPrint|PrintRawFile|PrintRawStream" --type cs`)
- [ ] Classify each call site: PDF-on-an-office-printer (candidate for IronPDF) vs. ESC/POS/ZPL/PCL bytes (keep RawPrint)
- [ ] Find hand-rolled Win32 P/Invoke printer declarations that sat alongside RawPrint
- [ ] Identify how PDFs were generated before being handed to RawPrint (external library? CLI? a report server?)
- [ ] List all document types that need HTML templates created
- [ ] Obtain an IronPDF license key
- [ ] Confirm IronPDF supports your target .NET version

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove the RawPrint package only if no call sites still need RAW byte channels
- [ ] Add license key at application startup
- [ ] Create HTML templates for each document type
- [ ] Replace external PDF generation calls with `ChromePdfRenderer`
- [ ] Remove hand-rolled Win32 P/Invoke printer declarations (after verifying no call sites)
- [ ] Remove printer-name configuration code that no longer applies
- [ ] Add merge, watermark, and security features as needed (all now in one library)
- [ ] Handle cross-platform concerns (remove Windows-only assumptions where they no longer apply)

### Testing
- [ ] Render each HTML template and compare to original print output (visual reference)
- [ ] Verify fonts render correctly in the target deployment environment
- [ ] Test in Linux/Docker if applicable
- [ ] Test merge, watermark, and security features if used
- [ ] Verify error handling — IronPDF raises exceptions you can catch, rather than spooler-side failures
- [ ] Confirm PDF output opens correctly in target viewers (not just prints correctly)

### Post-Migration
- [ ] Remove RawPrint NuGet package (only if no RAW byte channels remain)
- [ ] Remove Win32 printer P/Invoke class files no longer in use
- [ ] Remove printer-name environment variables from deployment config that no longer apply
- [ ] If RAW byte channels are still needed, keep RawPrint alongside IronPDF — they coexist cleanly

---

## Where to Go From Here

The footprint reduction is clearest when the workflow was "another library produces a PDF, RawPrint pushes it to a Windows printer." Both halves collapse into IronPDF on Windows: `RenderHtmlAsPdf()` for the create half, `pdf.Print()` for the print half, no spooler-name juggling and no hand-rolled Win32 P/Invoke for the create side.

The interesting benchmark in this migration is not render time — it is the comparison between the complexity of the old end-to-end pipeline (generate-document-elsewhere + configure-printer + send-bytes) versus the new single-step pipeline (render-HTML-to-PDF, optionally print).

**Discussion question:** After collapsing the pipeline, what surprised you most — the disappearance of the spooler-name configuration, the change in error surfaces (exceptions vs. spooler failures), or something on the deployment side?
