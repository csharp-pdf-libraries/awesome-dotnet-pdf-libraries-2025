---
title: "Replacing PDFPrinting.NET with IronPDF: what breaks, what doesn't"
published: false
tags: dotnet, csharp, pdf, migration
---

"We need to also generate the PDF, not just print it." That is the conversation that drives most migrations away from PDFPrinting.NET. The Terminalworks `PdfPrintingNet` package (current 5.4.2) is a focused silent-print library for Windows — it prints, views, edits, and rasterizes existing PDFs, but it does not author them. There is no `HtmlToPdfConverter`, no `WebPageToPdfConverter`, no way to turn an HTML invoice into a PDF before the print spooler sees it. So teams end up bolting a second library onto the pipeline, and at some point that arrangement becomes the thing that has to go.

This article is about swapping `PdfPrintingNet` for IronPDF: keeping the silent-print workflow intact while consolidating PDF generation into the same package. You'll have working migration code for the most common operations by the end, plus the gotchas worth knowing before you ship.

---

## Why Migrate (Without Drama)

Teams evaluating alternatives to PDFPrinting.NET commonly hit these triggers. Not every item applies to every project — this is a general list.

1. **No PDF generation** — `PdfPrintingNet` only operates on existing PDFs; it cannot create them from HTML, URLs, or images.
2. **No HTML or URL conversion** — there is no `HtmlToPdfConverter` and no `WebPageToPdfConverter` in the API surface.
3. **Windows-only print path** — the print pipeline depends on Windows printing infrastructure, which complicates Docker/Linux deployment.
4. **Limited PDF manipulation** — basic merge/split/extract via `PdfPrintDocument`, but no watermarking, no form filling, no signing.
5. **Secondary-library sprawl** — teams typically pair `PdfPrintingNet` with a generation library (PdfSharp, iTextSharp, others) and a manipulation library, which means two or three NuGet dependencies and two or three different APIs.
6. **No JavaScript or modern CSS rendering** — rasterization is for printing existing documents, not for capturing dynamic web content.
7. **Sparse manipulation API** — text extraction, form handling, and digital signatures sit outside the package's intended scope.

### Comparison Table

| Aspect | PDFPrinting.NET | IronPDF |
|---|---|---|
| Primary focus | Silent PDF printing for existing PDFs | Full PDF lifecycle (create + manipulate + print) |
| PDF creation from HTML/URL | Not supported | Supported (Chromium engine) |
| PDF manipulation | Basic merge/split/extract only | Merge, split, rotate, watermark, sign |
| Text extraction | Not in core API | Supported |
| Platform support | Windows only | Windows, Linux, macOS |
| Silent printing | Yes (its core feature) | Yes |
| Print settings | Property-based (`PdfPrint`) | `PrintSettings` object + `PrintDocument` access |
| Pricing | Site $299+, Redistributable $699+, Full $2,249 | Commercial — see [ironsoftware.com](https://ironpdf.com/licensing/) |
| Namespace | `PdfPrintingNet` (legacy: `TerminalWorks.PDFPrinting`) | `IronPdf`, `IronPdf.Printing` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | PDFPrinting.NET | IronPDF Equivalent | Complexity |
|---|---|---|---|
| Silent print existing PDF | `new PdfPrint(...).Print(path)` | `PdfDocument.FromFile(path).Print()` | Low |
| Print to specific printer | `pdfPrint.PrinterName = "..."` | `pdf.Print(printerName)` | Low |
| Copies, duplex, collate | Properties on `PdfPrint` | `PrintSettings` properties | Low |
| Print page range | `FromPage` / `ToPage` on `PdfPrint` | `PrintDocument.PrinterSettings` | Low |
| Advanced print control | `PdfPrintDocument` (inherits `PrintDocument`) | `pdf.GetPrintDocument()` | Low |
| Create PDF from HTML | Not available — second library required | `ChromePdfRenderer.RenderHtmlAsPdf()` | Low (new capability) |
| Create PDF from URL | Not available | `ChromePdfRenderer.RenderUrlAsPdf()` | Low (new capability) |
| Merge PDFs | Limited via `PdfPrintDocument` | `PdfDocument.Merge()` | Low |
| Watermark | Not available natively | `ApplyWatermark()` / `ApplyStamp()` | Low (new capability) |
| Password protection | Not available natively | `pdf.SecuritySettings` | Low (new capability) |
| Cross-platform print | Windows only | Windows/Linux (CUPS)/macOS | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Pure Windows silent-print workflow with no generation needs | Either tool works; IronPDF gives you headroom for the next requirement |
| Currently combining PdfPrintingNet with PdfSharp/iTextSharp to generate then print | Switch — one library, one license, one API |
| Linux/Docker deployment required | Switch — PdfPrintingNet is Windows-only; IronPDF supports Linux via CUPS |
| Need to add HTML/URL-to-PDF, watermarking, or security | Switch — these are not in PdfPrintingNet's scope |
| Heavy print-spooler customization that depends on `PdfPrintDocument` directly | Easy port — IronPDF exposes the same `System.Drawing.Printing.PrintDocument` via `pdf.GetPrintDocument()` |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9 (verify against your target with the [IronPDF compatibility docs](https://ironpdf.com/docs/))
- NuGet access
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)
- For Linux deployments: CUPS and the Chromium native dependencies (covered below)

### Find All PDFPrinting.NET References

```bash
# Find PDFPrinting.NET usage (newer + legacy namespaces)
grep -rE "PdfPrintingNet|TerminalWorks\.PDFPrinting|PdfPrint\b|PdfPrintDocument|PDFPrinter" --include="*.cs" .

# Find printer-related code that may need review
grep -rE "\.Print\(|PrinterName" --include="*.cs" .

# Find package references
grep -r "PdfPrintingNet" --include="*.csproj" .
```

### Uninstall / Install

```bash
# Remove PDFPrinting.NET (NuGet package id is PdfPrintingNet)
dotnet remove package PdfPrintingNet

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// Set once at application startup — https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 2 — Namespace Swap

**Before:**
```csharp
// Newer PDFPrinting.NET API:
using PdfPrintingNet;
// Older code may use:
using TerminalWorks.PDFPrinting;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Printing;
```

### Step 3 — Basic Silent Print

**Before (PDFPrinting.NET):**
```csharp
using PdfPrintingNet;
using System;

class Program
{
    static void Main()
    {
        string filePath = "document.pdf";

        var pdfPrint = new PdfPrint("license-owner", "license-key");
        var status = pdfPrint.Print(filePath);

        Console.WriteLine($"PDF printed: {status}");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("document.pdf");
pdf.Print();

Console.WriteLine("PDF printed successfully.");
```

The shape is similar — the main difference is that IronPDF separates "load the document" from "print it," which gives you a `PdfDocument` you can also save, manipulate, or inspect before printing.

---

## API Mapping Tables

### Namespace Mapping

| PDFPrinting.NET | IronPDF | Notes |
|---|---|---|
| `PdfPrintingNet` (newer) | `IronPdf` | Core namespace |
| `TerminalWorks.PDFPrinting` (legacy) | `IronPdf` | Older PDFPrinting.NET code |
| _(no separate namespace)_ | `IronPdf.Printing` | `PrintSettings` and related types |

### Core Class Mapping

| PDFPrinting.NET | IronPDF | Description |
|---|---|---|
| `PdfPrint` (newer) / `PDFPrinter` (legacy) | `PdfDocument` + `Print()` | Core print entry point |
| `PdfPrintDocument` | `PdfDocument` + `GetPrintDocument()` | Document representation |
| _(no HTML-to-PDF class)_ | `ChromePdfRenderer.RenderHtmlAsPdf` | New capability |
| _(no URL-to-PDF class)_ | `ChromePdfRenderer.RenderUrlAsPdf` | New capability |
| Properties on `PdfPrint` | `PrintSettings` | Print configuration object |

### Printing Methods

| Operation | PDFPrinting.NET | IronPDF |
|---|---|---|
| Print to default printer | `pdfPrint.Print(filePath)` | `pdf.Print()` |
| Print to named printer | `pdfPrint.PrinterName = "..."; pdfPrint.Print(filePath)` | `pdf.Print(printerName)` |
| Get underlying `PrintDocument` | `new PdfPrintDocument(...)` | `pdf.GetPrintDocument()` |
| Copies | `pdfPrint.Copies = n` | `printSettings.NumberOfCopies = n` |
| Duplex | `pdfPrint.Duplex = true` | `printSettings.DuplexMode` |
| Collate | `pdfPrint.Collate = true` | `printSettings.Collate = true` |
| Color/grayscale | `pdfPrint.PrintInColor = false` | `printSettings.GrayscaleOutput = true` |
| Page range | `pdfPrint.FromPage` / `ToPage` | `PrintDocument.PrinterSettings.FromPage/ToPage` + `PrintRange.SomePages` |

### PDF Generation (New in IronPDF)

| Feature | IronPDF Method | Notes |
|---|---|---|
| HTML string to PDF | `renderer.RenderHtmlAsPdf(html)` | Full HTML/CSS/JS via Chromium |
| URL to PDF | `renderer.RenderUrlAsPdf(url)` | Captures rendered web page |
| HTML file to PDF | `renderer.RenderHtmlFileAsPdf(path)` | Local HTML file |
| Image to PDF | `ImageToPdfConverter.ImageToPdf(paths)` | Multiple images, one PDF |

### PDF Manipulation (New in IronPDF)

| Feature | IronPDF Method |
|---|---|
| Load PDF | `PdfDocument.FromFile(path)` |
| Merge | `PdfDocument.Merge(pdf1, pdf2, ...)` |
| Split / extract pages | `pdf.CopyPages(start, end)` |
| Watermark | `pdf.ApplyWatermark(html, ...)` |
| Password | `pdf.SecuritySettings.UserPassword = "..."` |
| Extract text | `pdf.ExtractAllText()` |

---

## Four Complete Before/After Migrations

### 1. Print to a Specific Printer

**Before (PDFPrinting.NET):**
```csharp
using PdfPrintingNet;
using System;

class Program
{
    static void Main()
    {
        var pdfPrint = new PdfPrint("license-owner", "license-key");
        pdfPrint.PrinterName = "HP LaserJet Pro";
        pdfPrint.Print("document.pdf");

        Console.WriteLine("PDF sent to HP LaserJet Pro.");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("document.pdf");
pdf.Print("HP LaserJet Pro");

Console.WriteLine("PDF sent to HP LaserJet Pro.");
// https://ironpdf.com/how-to/pdf-printing-print/
```

---

### 2. Print with Custom Settings (Copies, Duplex, Grayscale)

**Before (PDFPrinting.NET):**
```csharp
using PdfPrintingNet;
using System;

class Program
{
    static void Main()
    {
        var pdfPrint = new PdfPrint("license-owner", "license-key");
        pdfPrint.PrinterName = "Office Printer";
        pdfPrint.Copies = 3;
        pdfPrint.Duplex = true;
        pdfPrint.Collate = true;
        pdfPrint.PrintInColor = false;

        pdfPrint.Print("report.pdf");

        Console.WriteLine("Printed 3 grayscale duplex copies.");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Printing;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("report.pdf");

var printSettings = new PrintSettings
{
    PrinterName = "Office Printer",
    NumberOfCopies = 3,
    DuplexMode = System.Drawing.Printing.Duplex.Vertical,
    Collate = true,
    GrayscaleOutput = true
};

pdf.Print(printSettings);

Console.WriteLine("Printed 3 grayscale duplex copies.");
```

---

### 3. Generate a PDF from HTML, Then Print (Replaces a Two-Library Setup)

This is the migration that usually triggers the switch: when a team has been pairing `PdfPrintingNet` with a separate generation library, IronPDF collapses both jobs into one package.

**Before (PDFPrinting.NET — generation is not supported):**
```csharp
// PdfPrintingNet has no HtmlToPdfConverter — teams typically pair it
// with a separate library (e.g. PdfSharp) to generate the PDF first,
// then hand the file to PdfPrint for silent printing.

using PdfPrintingNet;
using System;

class Program
{
    static void Main()
    {
        // Step 1: A second library generates "invoice.pdf" from HTML.
        //         That code lives in another package and is not shown here.

        // Step 2: PDFPrinting.NET prints the file produced in Step 1.
        var pdfPrint = new PdfPrint("license-owner", "license-key");
        pdfPrint.PrinterName = "Invoice Printer";
        pdfPrint.Print("invoice.pdf");
    }
}
```

**After (IronPDF — one package handles both):**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(@"
    <html>
    <head>
        <style>
            body { font-family: Arial; }
            h1 { color: navy; }
        </style>
    </head>
    <body>
        <h1>Invoice #12345</h1>
        <p>Amount Due: $1,234.56</p>
    </body>
    </html>");

// Print immediately
pdf.Print("Invoice Printer");

// Or save and print later
pdf.SaveAs("invoice.pdf");

Console.WriteLine("PDF created and printed.");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 4. Merge Multiple PDFs Into One Print Job

**Before (PDFPrinting.NET):**
```csharp
// PdfPrintDocument can print files sequentially, but merging into a
// single PDF before printing typically requires a secondary library.

using PdfPrintingNet;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var pdfPrint = new PdfPrint("license-owner", "license-key");
        pdfPrint.PrinterName = "Office Printer";

        var files = new List<string> { "cover.pdf", "content.pdf", "appendix.pdf" };

        foreach (var file in files)
        {
            pdfPrint.Print(file);
        }
        // Three separate print jobs — collation and paper handling
        // are at the printer's mercy.
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var cover = PdfDocument.FromFile("cover.pdf");
var content = PdfDocument.FromFile("content.pdf");
var appendix = PdfDocument.FromFile("appendix.pdf");

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var combined = PdfDocument.Merge(cover, content, appendix);
combined.Print("Office Printer");

// Or save the merged version for archival
combined.SaveAs("complete-report.pdf");

Console.WriteLine("Documents merged and printed as one job.");
```

One print job, predictable collation, and a single archived PDF.

---

## Print Settings Reference

```csharp
using IronPdf;
using IronPdf.Printing;

var pdf = PdfDocument.FromFile("document.pdf");

var settings = new PrintSettings
{
    PrinterName = "MyPrinter",
    NumberOfCopies = 3,
    DuplexMode = System.Drawing.Printing.Duplex.Vertical,
    Collate = true,
    GrayscaleOutput = true,
    Dpi = 300
};

pdf.Print(settings);
```

For deeper print control (paper source, margins, orientation), drop into the underlying `PrintDocument`:

```csharp
var printDoc = pdf.GetPrintDocument();

printDoc.PrinterSettings.PrinterName = "Network Printer";
printDoc.PrinterSettings.FromPage = 2;
printDoc.PrinterSettings.ToPage = 5;
printDoc.PrinterSettings.PrintRange = System.Drawing.Printing.PrintRange.SomePages;

printDoc.DefaultPageSettings.Landscape = true;
printDoc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(50, 50, 50, 50);

printDoc.Print();
```

The `System.Drawing.Printing.PrintDocument` surface is identical to what `PdfPrintDocument` exposed, so any code that worked against `PrinterSettings` or `DefaultPageSettings` ports across with no logic changes.

---

## Adding New Capabilities After You Migrate

These are not parts of `PdfPrintingNet` you have to translate — they are features that simply become available after the swap. Pull them in as you need them.

### Headers and Footers

```csharp
var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Company Report</div>"
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>"
};

var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Document Content</h1></body></html>");
pdf.SaveAs("report.pdf");
```

### Watermark Before Printing

```csharp
var pdf = PdfDocument.FromFile("draft-report.pdf");

pdf.ApplyWatermark(
    "<h2 style='color:red; opacity:0.3; font-size:72px;'>DRAFT</h2>",
    rotation: 45,
    IronPdf.Editing.VerticalAlignment.Middle,
    IronPdf.Editing.HorizontalAlignment.Center
);

pdf.Print();
```

### Password Protection

```csharp
var pdf = PdfDocument.FromFile("sensitive.pdf");

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
```

### Text Extraction

```csharp
string allText = pdf.ExtractAllText();
string pageText = pdf.ExtractTextFromPage(0);
```

---

## Cross-Platform Printing

`PdfPrintingNet` is Windows-only. IronPDF prints on all three desktop platforms.

### Windows

```csharp
pdf.Print("HP LaserJet");
```

### Linux

```csharp
// Requires CUPS:
//   apt-get install cups
//
// CUPS printer names typically use underscores instead of spaces:
pdf.Print("HP_LaserJet");
```

### macOS

```csharp
pdf.Print("HP LaserJet");
```

### List Available Printers (Any Platform)

```csharp
using System.Drawing.Printing;

foreach (string printer in PrinterSettings.InstalledPrinters)
{
    Console.WriteLine(printer);
}
```

---

## Server Deployment Notes

### Linux Dependencies

First IronPDF run downloads Chromium (~150MB, one-time). On a clean Debian/Ubuntu base image you'll also need:

```bash
apt-get update && apt-get install -y \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2

# Only if you need actual printing on the Linux host:
apt-get install -y cups
```

### Resource Disposal

`PdfPrintingNet` calls are largely stateless; IronPDF's `PdfDocument` holds resources and is `IDisposable`. Wrap it in a `using` when the document goes out of scope:

```csharp
using (var pdf = PdfDocument.FromFile("document.pdf"))
{
    pdf.Print();
} // disposed
```

### Async Operations

`PdfPrintingNet` is synchronous. IronPDF supports async loading and saving where it matters:

```csharp
var pdf = await PdfDocument.FromFileAsync("document.pdf");
await pdf.SaveAsAsync("output.pdf");
```

---

## Common Migration Issues

### Printer Name Exact Match

Both libraries require the printer name to match exactly. The simplest way to catch a typo is to enumerate installed printers first:

```csharp
foreach (string printer in PrinterSettings.InstalledPrinters)
{
    Console.WriteLine($"'{printer}'");
}
```

Note any leading/trailing spaces and case differences between what's installed and the string you're passing to `Print()`.

### CUPS Names on Linux

On Linux, CUPS replaces spaces with underscores in printer names. If you're moving from a Windows-only deployment to a Linux-or-mixed deployment, audit any hardcoded printer-name strings.

### Page Indexing

IronPDF uses 0-based page indexing for its document API (`CopyPages`, `RemovePages`, etc.). `PdfPrintingNet`'s `FromPage`/`ToPage` properties are 1-based because they map to the underlying `PrinterSettings`. If you port `FromPage = 2; ToPage = 5;` directly into `PrintDocument.PrinterSettings` on IronPDF, those stay 1-based — only the IronPDF document-manipulation calls switch to 0-based.

### Secondary Library Cleanup

The most common finding during this migration: projects added PdfSharp, iTextSharp, or a similar library specifically to fill the gaps in `PdfPrintingNet` (generation, merge, watermark, security). After moving to IronPDF, those secondary packages can usually be removed. Audit your NuGet dependencies once the printing path is green.

---

## Migration Checklist

### Pre-Migration

- [ ] Inventory all `PdfPrintingNet` and legacy `TerminalWorks.PDFPrinting` usages
- [ ] Document all printer names currently in production
- [ ] Note all print settings (copies, duplex, grayscale, page range, paper source)
- [ ] Identify any secondary PDF libraries paired with `PdfPrintingNet`
- [ ] Decide whether cross-platform support is in scope
- [ ] Obtain an IronPDF license key (trial is fine to start)

### Code Migration

- [ ] `dotnet remove package PdfPrintingNet`
- [ ] `dotnet add package IronPdf`
- [ ] Set `IronPdf.License.LicenseKey` at application startup
- [ ] Replace `new PdfPrint(...).Print(path)` with `PdfDocument.FromFile(path).Print()`
- [ ] Replace `PdfPrint` property settings with a `PrintSettings` object
- [ ] Replace `new PdfPrintDocument(...)` with `pdf.GetPrintDocument()`
- [ ] Remove the secondary generation/manipulation library if it was only there to fill `PdfPrintingNet` gaps
- [ ] Wrap `PdfDocument` usage in `using` blocks

### Testing

- [ ] Print to each printer used in production and verify output
- [ ] Verify copies, duplex, collate, and grayscale behavior
- [ ] Verify page-range printing
- [ ] If migrating to Linux: confirm CUPS printer names and the Chromium native dependencies
- [ ] Verify any new generation paths (HTML/URL to PDF) render correctly

### Post-Migration

- [ ] Remove `PdfPrintingNet` from `.csproj` and any references
- [ ] Remove secondary PDF library packages that are no longer needed
- [ ] Update internal docs and runbooks to reference IronPDF
- [ ] Consider adopting the new capabilities (watermark, security, signing) where they remove existing workarounds

---

## Before You Ship

The clearest win in this migration is collapsing a two- or three-library print-and-generate pipeline into one package. The actual silent-print code change is small — `PdfPrint.Print(path)` becomes `PdfDocument.FromFile(path).Print()` and the property bag turns into a `PrintSettings` object — so most of the work is auditing what else can come out of the project once IronPDF is in.

The area worth testing carefully: any code that depends on the `System.Drawing.Printing.PrintDocument` surface. IronPDF exposes that same surface via `pdf.GetPrintDocument()`, but if your old code was constructing `PdfPrintDocument` directly and subscribing to `PrintPage`-style events, walk through the lifecycle in a small spike before porting the whole call site.

**Discussion question:** Which feature was the one that drove the decision — cross-platform, dropping the secondary library, or something else? Particularly interested in cases where the existing print pipeline had grown around `PdfPrintingNet`'s limits.
