---
title: "Moving off PDFFilePrint: practical IronPDF migration notes"
published: false
tags: dotnet, csharp, pdf, migration
---

PDFFilePrint is a tiny MIT-licensed NuGet wrapper around PdfiumViewer that exists for one job: silently send an existing PDF or XPS file to a printer. The latest release is 1.0.3 from February 2020, it targets .NET Framework 4.6.1+ on Windows only, and there's been no public maintenance since. The trigger for migrating is rarely "PDFFilePrint is broken" — it usually shows up as "we need to print PDFs from .NET 6/7/8 on Linux", or "we now need to generate the PDF as well, not just print it", or "our deployment can't carry the PdfiumViewer Win32 native binaries anymore."

This article covers the period right after you've decided to consolidate onto IronPDF for both PDF generation and printing. The patterns below assume your existing PDFFilePrint usage is centred on `new FilePrint(path, null).Print()` plus a handful of `Properties.Settings.Default` keys in `app.config`.

---

## Diagnosing the migration trigger

Before changing code, confirm which constraint is actually pushing you off PDFFilePrint. The remediation differs by cause.

**Symptom: target framework can't restore the package**

```bash
# PDFFilePrint 1.0.3 targets net461 — confirm your project TFM
dotnet list package | grep -i pdffileprint

# Check the package's supported frameworks
# https://www.nuget.org/packages/PDFFilePrint/ — Supported frameworks section
```

**Symptom: package installs but the PdfiumViewer native binaries don't load**

```bash
# PDFFilePrint depends on PdfiumViewer's x86 / x64 native Pdfium DLLs
dotnet list package --include-transitive | grep -i pdfium

# On non-Windows hosts these won't load — PdfiumViewer is Win32-only
```

**Symptom: you've outgrown print-only and need PDF generation too**

```bash
# Inventory call sites — most PDFFilePrint codebases are a single helper
grep -r "using PDFFilePrint\|new FilePrint" --include="*.cs" .

# Inventory the app.config keys it reads
grep -r "PrinterName\|PaperName\|Copies\|PrintToFile\|DefaultPrintToDirectory" \
    --include="*.config" .
```

Once you've identified the constraint, the migration path below covers all three cases.

---

## Why migrate (without drama)

PDFFilePrint is fine at the one thing it does. The reasons teams move off it are usually about scope and platform, not quality:

1. **Print-only surface area** — the public API is essentially the `FilePrint` class with a `Print()` method. There is no create, edit, merge, watermark, or extract.
2. **No release since February 2020** — version 1.0.3 is the latest on NuGet; there is no public source repository linked from the package page.
3. **Windows-only by construction** — PdfiumViewer ships Win32 native binaries (`PdfiumViewer.Native.x86.v8-xfa`, `PdfiumViewer.Native.x86_64.v8-xfa`); Linux, macOS, and Docker hosts can't run it.
4. **.NET Framework 4.6.1+ target only** — no .NET Core or modern .NET TFM published.
5. **app.config-driven configuration** — `PrinterName`, `PaperName`, `Copies`, `PrintToFile`, and `DefaultPrintToDirectory` are read from `Properties.Settings.Default` rather than a strongly-typed options object passed per call.
6. **No first-class page range, duplex, or quality knobs** — those settings have to be applied at the printer driver or via the underlying PdfiumViewer plumbing.
7. **Pairs with another library for any non-printing task** — generating a PDF from HTML, fetching a URL, merging files, or watermarking all require a second tool.
8. **Synchronous only** — `FilePrint.Print()` blocks until the spooler accepts the job; async wrappers have to be hand-rolled with `Task.Run`.

### Comparison table

| Aspect | PDFFilePrint | IronPDF |
|---|---|---|
| Focus | Silent printing of existing PDF / XPS files | Full PDF library + renderer + printer |
| Last release | 1.0.3 (Feb 2020) | Active, ongoing releases |
| Licensing | MIT (open source) | Commercial — see [ironsoftware.com](https://ironpdf.com/) |
| API style | Single `FilePrint` class, `app.config` keys | Strongly-typed `PrinterSettings` per call |
| Learning curve | Trivial — one method | Medium |
| HTML / URL rendering | Not supported | Chromium-based |
| Page indexing | N/A (no page-level API) | 0-based |
| Thread safety | Sequential per `FilePrint` instance | Renderer instance reuse, async overloads |
| Namespace | `PDFFilePrint` | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | PDFFilePrint approach | Effort to migrate |
|---|---|---|
| Silent print existing PDF | `new FilePrint(path, null).Print()` | Low — direct mapping to `pdf.Print()` |
| Named printer | `Properties.Settings.Default.PrinterName` | Low — `pdf.Print(printerName)` or `PrinterSettings` |
| Multiple copies | `Properties.Settings.Default.Copies` | Low — `PrinterSettings.Copies` |
| Paper size | `Properties.Settings.Default.PaperName` | Low — `PrinterSettings.DefaultPageSettings.PaperSize` |
| Print-to-file | `PrintToFile` + `DefaultPrintToDirectory` | Low — `PrinterSettings.PrintToFile` + `PrintFileName` |
| Page range | Not supported (driver default) | Low — `PrinterSettings.FromPage` / `ToPage` |
| Duplex | Not supported (driver default) | Low — `PrinterSettings.Duplex` |
| HTML-to-PDF then print | Requires a second renderer | Low — `ChromePdfRenderer` + `pdf.Print()` |
| URL-to-PDF then print | Requires a second renderer | Low — `RenderUrlAsPdf` + `pdf.Print()` |
| Merge then print | Requires a second library | Low — `PdfDocument.Merge` + `pdf.Print()` |
| Watermark then print | Requires a second library | Low — `pdf.ApplyWatermark` + `pdf.Print()` |
| Linux / macOS / Docker | Not possible | Resolved (CUPS on non-Windows) |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| You only print existing PDFs and you're staying on Windows .NET Framework | PDFFilePrint still works — migration is optional |
| You need to run on .NET 6/7/8/9/10 or non-Windows | IronPDF is a direct fit; PDFFilePrint can't follow you |
| You now need to generate PDFs as well as print them | Consolidate on IronPDF — one library does both |
| You want strongly-typed printer settings per call | IronPDF — `app.config` mutation is no longer required |

---

## Before you start

### Prerequisites

- Target framework decided (IronPDF supports .NET Framework 4.6.2+ and modern .NET)
- Inventory of `app.config` keys PDFFilePrint currently reads
- Inventory of printer names used across environments
- NuGet access
- IronPDF trial or production license key

### Find PDFFilePrint references in your codebase

```bash
# Find all PDFFilePrint usages
grep -r "using PDFFilePrint\|new FilePrint" --include="*.cs" .

# Find related app.config keys
grep -r "PrinterName\|PaperName\|Copies\|PrintToFile\|DefaultPrintToDirectory" \
    --include="*.config" --include="*.settings" .

# Find NuGet references
grep -r -i "pdffileprint" --include="*.csproj" .
```

### Remove PDFFilePrint, install IronPDF

```bash
dotnet remove package PDFFilePrint
dotnet add package IronPdf
dotnet restore
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (PDFFilePrint):**
```csharp
// PDFFilePrint is MIT-licensed and needs no key at runtime.
// Configuration is read from app.config / Settings.Default.
using PDFFilePrint;
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// License guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
using PDFFilePrint;
// Settings live in Properties.Settings.Default (app.config-backed)
```

**After:**
```csharp
using IronPdf;
using System.Drawing.Printing; // PrinterSettings, Duplex, PrintRange
```

### Step 3: Basic silent print

**Before (PDFFilePrint):**
```csharp
using PDFFilePrint;

public void PrintPdf(string pdfPath, string printerName)
{
    // PDFFilePrint reads PrinterName from app.config — to override
    // at runtime, mutate Settings.Default before instantiating FilePrint.
    Properties.Settings.Default.PrinterName = printerName;

    var fileprint = new FilePrint(pdfPath, null);
    fileprint.Print();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("document.pdf");
pdf.Print(); // Silent print to the default printer

// Or target a named printer:
// pdf.Print("HP LaserJet Pro");
// Guide: https://ironpdf.com/docs/questions/print-pdf/
```

---

## Troubleshooting: common migration failures

### Problem: "Print silently produced no output on Linux"

PDFFilePrint never ran on Linux, so this only matters once you've moved to IronPDF on a non-Windows host. Linux printing requires CUPS.

```bash
# Confirm CUPS is installed and a printer is registered
lpstat -p -d

# In a container — install the printing stack:
# RUN apt-get update && apt-get install -y cups cups-client
```

### Problem: "Print dialog opens unexpectedly"

`pdf.Print()` is silent by default. The dialog overload is opt-in.

```csharp
pdf.Print();         // silent
pdf.Print(true);     // shows the OS print dialog
```

### Problem: "Native Pdfium binaries still loaded after removing PDFFilePrint"

PdfiumViewer pulled in `PdfiumViewer.Native.x86.v8-xfa` and `PdfiumViewer.Native.x86_64.v8-xfa` as transitive dependencies. After removing PDFFilePrint, scan for orphaned references:

```bash
dotnet list package --include-transitive | grep -i "pdfium\|pdffileprint"
```

If anything PDFFilePrint-related survives, it's coming from another package. Clean the `bin/` and `obj/` directories before re-running `dotnet restore`.

### Problem: "app.config keys are still being read"

PDFFilePrint relied on `Properties.Settings.Default.PrinterName`, `Copies`, `PaperName`, `PrintToFile`, and `DefaultPrintToDirectory`. After migration these keys aren't used by IronPDF, but they may still be referenced by other parts of the project.

```bash
grep -r "Settings.Default" --include="*.cs" .
```

Either remove the keys from `app.config` once nothing reads them, or migrate the read sites to `PrinterSettings` properties as below.

### Problem: "Docker image grew after switching to IronPDF"

IronPDF bundles a Chromium renderer for HTML-to-PDF — PDFFilePrint had no rendering engine. If image size matters:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y \
    libglib2.0-0 libnss3 libatk1.0-0 libatk-bridge2.0-0 libx11-6 \
    cups cups-client \
    --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*
# Linux deployment notes: https://ironpdf.com/how-to/linux/
```

---

## API mapping tables

### Namespace mapping

| PDFFilePrint | IronPDF | Notes |
|---|---|---|
| `PDFFilePrint` | `IronPdf` | Core |
| `Properties.Settings` (app.config) | `System.Drawing.Printing.PrinterSettings` | Strongly-typed per call |
| _(no rendering)_ | `IronPdf.Rendering` | HTML / URL render options |
| _(no editing)_ | `IronPdf.Editing` | Watermarks, stamps |
| _(no security)_ | `IronPdf.Security` | Passwords, permissions |

### Core class mapping

| PDFFilePrint API | IronPDF API | Description |
|---|---|---|
| `new FilePrint(pdfPath, null)` | `PdfDocument.FromFile(pdfPath)` | Load existing PDF |
| `fileprint.Print()` | `pdf.Print()` | Silent print to default printer |
| `Properties.Settings.Default.PrinterName` | `pdf.Print(printerName)` or `PrinterSettings.PrinterName` | Named printer |
| `new FilePrint(pdfPath, xpsOutputPath)` | `PrinterSettings.PrintToFile` + `PrintFileName` | Print-to-file |
| _(not available)_ | `ChromePdfRenderer` | HTML / URL to PDF |
| _(not available)_ | `PdfDocument.Merge` | Combine PDFs |
| _(not available)_ | `pdf.ApplyWatermark` | Watermarks |
| _(not available)_ | `pdf.SecuritySettings` | Passwords, permissions |

### Printer settings mapping (`System.Drawing.Printing`)

| PDFFilePrint setting (app.config) | `PrinterSettings` property | Type |
|---|---|---|
| `PrinterName` | `PrinterName` | `string` |
| `Copies` | `Copies` | `short` |
| `PaperName` | `DefaultPageSettings.PaperSize` | `PaperSize` |
| `PrintToFile` + `DefaultPrintToDirectory` | `PrintToFile` + `PrintFileName` | `bool` + `string` |
| _(driver default)_ | `FromPage`, `ToPage`, `PrintRange` | `int`, `PrintRange` |
| _(driver default)_ | `Duplex` | `Duplex` enum |
| _(driver default)_ | `Collate` | `bool` |
| _(driver default)_ | `DefaultPageSettings.Color` | `bool` |
| _(not available)_ | `pdf.Print(int dpi)` overload | `int` |

### New capabilities (not in PDFFilePrint)

| IronPDF feature | Description |
|---|---|
| `ChromePdfRenderer.RenderHtmlAsPdf()` | Create PDF from HTML |
| `ChromePdfRenderer.RenderUrlAsPdf()` | Create PDF from URL |
| `PdfDocument.Merge()` | Combine multiple PDFs |
| `pdf.CopyPages()` | Extract specific pages |
| `pdf.ApplyWatermark()` | Add watermarks |
| `pdf.SecuritySettings` | Password protection |
| `pdf.ExtractAllText()` | Extract text content |
| `pdf.RasterizeToImageFiles()` | Convert pages to images |
| `pdf.SignWithDigitalSignature()` | Digital signatures |

---

## Five complete before/after migrations

### 1. Silent print with multiple copies

**Before (PDFFilePrint):**
```csharp
using PDFFilePrint;

public void PrintSilent(string pdfPath, int copies)
{
    // PDFFilePrint is always silent. Copies live in app.config.
    Properties.Settings.Default.Copies = copies;
    Properties.Settings.Default.PrinterName = "Default Printer";

    var fileprint = new FilePrint(pdfPath, null);
    fileprint.Print();
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Drawing.Printing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

public void PrintSilent(string pdfPath, int copies)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    var settings = new PrinterSettings
    {
        Copies = (short)copies
    };

    pdf.GetPrintDocument(settings).Print();
}
```

---

### 2. Print specific page range

**Before (PDFFilePrint):**
```csharp
using PDFFilePrint;

public void PrintPageRange(string pdfPath, string printerName, int startPage, int endPage)
{
    // PDFFilePrint has no first-class page-range API. Common workarounds:
    // pre-split the PDF and feed the smaller file to FilePrint, or rely on
    // the printer driver's "current page" behaviour.
    Properties.Settings.Default.PrinterName = printerName;
    var fileprint = new FilePrint(pdfPath, null);
    fileprint.Print();
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Drawing.Printing;

public void PrintPageRange(string pdfPath, string printerName, int startPage, int endPage)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    var settings = new PrinterSettings
    {
        PrinterName = printerName,
        FromPage = startPage,
        ToPage = endPage,
        PrintRange = PrintRange.SomePages
    };

    pdf.GetPrintDocument(settings).Print();
}
```

---

### 3. Duplex (double-sided) printing

**Before (PDFFilePrint):**
```csharp
using PDFFilePrint;

public void PrintDuplex(string pdfPath, string printerName)
{
    // PDFFilePrint has no Duplex setting. Duplex is configured on the
    // printer driver itself, then PDFFilePrint targets that printer.
    Properties.Settings.Default.PrinterName = printerName;
    var fileprint = new FilePrint(pdfPath, null);
    fileprint.Print();
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Drawing.Printing;

public void PrintDuplex(string pdfPath, string printerName)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    var settings = new PrinterSettings
    {
        PrinterName = printerName,
        Duplex = Duplex.Vertical // Long edge binding
    };

    pdf.GetPrintDocument(settings).Print();
}
```

---

### 4. Batch print a folder of PDFs

**Before (PDFFilePrint):**
```csharp
using PDFFilePrint;
using System;
using System.IO;

public void BatchPrint(string folderPath, string printerName)
{
    Properties.Settings.Default.PrinterName = printerName;

    foreach (var pdfFile in Directory.GetFiles(folderPath, "*.pdf"))
    {
        try
        {
            var fileprint = new FilePrint(pdfFile, null);
            fileprint.Print();
        }
        catch (Exception ex)
        {
            // Errors bubble up from PdfiumViewer as plain exceptions.
            Console.WriteLine($"Failed to print {pdfFile}: {ex.Message}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;
using System.Drawing.Printing;
using System.IO;

public void BatchPrint(string folderPath, string printerName)
{
    var settings = new PrinterSettings { PrinterName = printerName };

    foreach (var pdfFile in Directory.GetFiles(folderPath, "*.pdf"))
    {
        try
        {
            var pdf = PdfDocument.FromFile(pdfFile);
            pdf.GetPrintDocument(settings).Print();
        }
        catch (IronPdf.Exceptions.IronPdfException ex)
        {
            Console.WriteLine($"Failed to print {pdfFile}: {ex.Message}");
        }
    }
}
```

---

### 5. Render then print (consolidating two libraries into one)

**Before (PDFFilePrint + a separate renderer):**
```csharp
using PDFFilePrint;
using System.IO;

public void CreateAndPrint(string html, string outputPath, string printerName)
{
    // PDFFilePrint cannot create PDFs. Render the HTML with a separate
    // library (PuppeteerSharp, wkhtmltopdf, etc.) to produce outputPath,
    // then hand the resulting file to PDFFilePrint for printing.

    // ... renderer-specific code writes outputPath ...

    Properties.Settings.Default.PrinterName = printerName;
    var fileprint = new FilePrint(outputPath, null);
    fileprint.Print();
}
```

**After (IronPDF — single library):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

public void CreateAndPrint(string html, string printerName)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Print directly without staging to disk
    pdf.Print(printerName);
}
// Rendering guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Critical migration notes

### Settings move from app.config to per-call objects

The biggest behavioural change. PDFFilePrint reads `Properties.Settings.Default` keys; IronPDF wants a `PrinterSettings` instance per call. If callers used to mutate `Settings.Default` to influence behaviour, they now pass a configured `PrinterSettings` to `GetPrintDocument`.

```csharp
// PDFFilePrint pattern (do not port forward):
Properties.Settings.Default.PrinterName = "HP LaserJet Pro";
new FilePrint(path, null).Print();

// IronPDF pattern:
var settings = new PrinterSettings { PrinterName = "HP LaserJet Pro" };
PdfDocument.FromFile(path).GetPrintDocument(settings).Print();
```

### Page range, duplex, and quality are now first-class

PDFFilePrint had no API for these — they had to be set on the printer driver. IronPDF exposes them via `PrinterSettings` and overloads:

```csharp
settings.PrintRange = PrintRange.SomePages;
settings.FromPage = 2;
settings.ToPage = 5;
settings.Duplex = Duplex.Vertical;
settings.Collate = true;

// DPI-only convenience overload
pdf.Print(dpi: 300);
```

### Error model

PDFFilePrint surfaces failures as plain exceptions bubbled up from PdfiumViewer. IronPDF raises `IronPdf.Exceptions.IronPdfException` for library-specific errors; printer-stack failures still come through `System.Drawing.Printing`. Validate the printer exists and the file is on disk before printing:

```csharp
try
{
    if (!PrinterSettings.InstalledPrinters.Cast<string>()
        .Any(p => p.Equals(printerName, StringComparison.OrdinalIgnoreCase)))
    {
        throw new ArgumentException($"Printer not found: {printerName}");
    }

    var pdf = PdfDocument.FromFile(pdfPath);
    var settings = new PrinterSettings { PrinterName = printerName };
    pdf.GetPrintDocument(settings).Print();
}
catch (IronPdf.Exceptions.IronPdfException ex)
{
    _logger.LogError(ex, "PDF print failed: {msg}", ex.Message);
    throw;
}
```

---

## Performance considerations

### Renderer reuse (when generating before printing)

If you're now using IronPDF for HTML-to-PDF as well as printing, instantiate `ChromePdfRenderer` once per batch — not per render:

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

foreach (var html in documentQueue)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.Print(); // sends to default printer
}
```

### Async wrappers

PDFFilePrint had no async story — you wrapped `Print()` in `Task.Run`. IronPDF has async overloads for rendering, and printing can still be wrapped in `Task.Run` when you need to keep the calling thread responsive:

```csharp
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
await Task.Run(() => pdf.Print(printerName));
// Async guide: https://ironpdf.com/how-to/async/
```

### Cross-platform printing

PDFFilePrint cannot run on Linux or macOS. IronPDF can — printing on non-Windows hosts goes through CUPS, so a CUPS-registered printer must exist on the target host.

---

## Migration checklist

### Pre-migration

- [ ] Locate every `using PDFFilePrint;` and `new FilePrint(...)` call site
- [ ] Document the current `app.config` keys (`PrinterName`, `PaperName`, `Copies`, `PrintToFile`, `DefaultPrintToDirectory`)
- [ ] Identify printer names used across each environment
- [ ] Identify any code that mutates `Settings.Default` at runtime
- [ ] Decide target framework (IronPDF supports .NET Framework 4.6.2+ and modern .NET)
- [ ] Obtain an IronPDF trial or production license key
- [ ] Note any external libraries that currently pair with PDFFilePrint for PDF generation — those can likely be retired

### Code migration

- [ ] Remove the `PDFFilePrint` NuGet package
- [ ] Remove the PdfiumViewer transitive references if nothing else uses them
- [ ] Add the `IronPdf` NuGet package
- [ ] Replace `using PDFFilePrint;` with `using IronPdf;` and `using System.Drawing.Printing;`
- [ ] Set `IronPdf.License.LicenseKey` at application startup
- [ ] Replace `new FilePrint(path, null).Print()` with `PdfDocument.FromFile(path).Print()` or the `GetPrintDocument(settings)` form
- [ ] Move printer name, copies, paper size, and print-to-file flags from `app.config` into per-call `PrinterSettings`
- [ ] Add explicit `PrintRange`, `Duplex`, and `Collate` where the team previously relied on driver defaults

### Testing

- [ ] Build succeeds on the chosen target framework
- [ ] Silent print to the default printer
- [ ] Named-printer print with `PrinterSettings.PrinterName`
- [ ] Multi-copy and collate
- [ ] Page range (`FromPage` / `ToPage` / `PrintRange.SomePages`)
- [ ] Duplex (`Duplex.Vertical` / `Duplex.Horizontal`)
- [ ] Print-to-file (`PrintToFile` + `PrintFileName`)
- [ ] Batch print across a representative folder of PDFs
- [ ] Cross-platform print test if Linux / macOS is in scope

### Post-migration

- [ ] Remove PDFFilePrint-specific keys from `app.config` once nothing reads them
- [ ] Remove any paired PDF-generation library that IronPDF now subsumes
- [ ] Update deployment documentation (CUPS prerequisite on non-Windows)
- [ ] Monitor the first production print runs for spool, paper-size, or duplex regressions

---

## One Last Thing

PDFFilePrint occupies a narrow slice — silent printing of existing PDF files on Windows .NET Framework — and within that slice it works. The friction is everything around it: platform reach, configuration model, and the fact that you usually need a second library to produce the PDF in the first place. IronPDF collapses that pair down to one dependency and gives you strongly-typed `PrinterSettings` for the print path itself.

If you've completed this migration, what was the biggest behavioural difference you hit between PDFFilePrint's app.config-driven flow and IronPDF's per-call `PrinterSettings` model? Edge cases from real codebases help calibrate expectations for teams making the same jump.
