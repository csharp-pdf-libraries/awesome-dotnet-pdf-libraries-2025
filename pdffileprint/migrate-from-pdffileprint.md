# How Do I Migrate from PDFFilePrint to IronPDF in C#?

## Why Migrate from PDFFilePrint to IronPDF?

[PDFFilePrint](https://www.nuget.org/packages/PDFFilePrint/) is a small open-source NuGet wrapper (MIT, by Christian Andersen) around [PdfiumViewer](https://github.com/pvginkel/PdfiumViewer) and Google's Pdfium. It exists for one purpose: silently print an existing PDF or XPS file to a printer driver (including a "print to file" driver). The latest release is **1.0.3, published 2020-02-10**, targeting **.NET Framework 4.6.1+** on Windows. While useful for batch printing, it creates significant architectural limitations once your needs grow beyond "print this PDF":

### Critical PDFFilePrint Limitations

1. **Printing-Only**: Cannot create, edit, merge, or manipulate PDFs — the public surface is essentially a `FilePrint` class with a `Print()` method.
2. **Config-File-Driven**: Settings (`PrinterName`, `PaperName`, `Copies`, `PrintToFile`, `DefaultPrintToDirectory`) are read from `app.config` / `Settings.Default` rather than passed as a strongly-typed options object.
3. **Windows-Only**: Built on PdfiumViewer's Win32 native binaries (`PdfiumViewer.Native.x86.v8-xfa`, `PdfiumViewer.Native.x86_64.v8-xfa`) and the Windows printing subsystem.
4. **.NET Framework Only**: Targets net461; no .NET Core / .NET 6+ TFM published.
5. **Effectively Unmaintained**: No release since February 2020; no public source repository linked from NuGet.
6. **Limited Print Knobs**: Duplex, page range, orientation, and color must be configured at the printer-driver / app.config level — there is no rich API for them.
7. **No PDF Generation**: Can't create PDFs — only prints existing ones. To go from HTML or a URL to paper you must pair PDFFilePrint with a separate renderer.

### IronPDF Advantages

| Aspect | PDFFilePrint | IronPDF |
|--------|--------------|---------|
| **Type** | Pdfium print wrapper | Full PDF library + renderer |
| **Last release** | 1.0.3 (Feb 2020) | Active, 2026 releases |
| **PDF Printing** | Yes (PrintDocument under the hood) | Yes (`PdfDocument.Print` / `GetPrintDocument`) |
| **PDF Creation** | No | Yes (HTML, URL, images) |
| **PDF Manipulation** | No | Yes (merge, split, edit) |
| **Cross-Platform** | Windows only (net461) | Windows, Linux, macOS, Docker |
| **Config Surface** | app.config keys | Strongly-typed `PrinterSettings` |
| **IntelliSense** | Minimal | Full |
| **Async Support** | No | Built-in `Async` overloads |
| **NuGet Package** | `PDFFilePrint` 1.0.3 | `IronPdf` |

Additional process simplification patterns and native API integration examples are available in the [complete walkthrough](https://ironpdf.com/blog/migration-guides/migrate-from-pdffileprint-to-ironpdf/).

---

## NuGet Package Changes

```bash
# Remove PDFFilePrint
dotnet remove package PDFFilePrint

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// PDFFilePrint
using PDFFilePrint;

// IronPDF — printing flows through System.Drawing.Printing
using IronPdf;
using System.Drawing.Printing;
```

---

## Complete API Reference

### Class / Method Mapping

| PDFFilePrint API | IronPDF API | Notes |
|---------------------|-------------|-------|
| `new FilePrint(pdfPath, null)` | `PdfDocument.FromFile(pdfPath)` | Load PDF |
| `fileprint.Print()` | `pdf.Print()` / `pdf.GetPrintDocument(settings).Print()` | Silent print |
| `Properties.Settings.Default.PrinterName` | `PrinterSettings.PrinterName` | Printer selection |
| `Properties.Settings.Default.Copies` | `PrinterSettings.Copies` | Copy count |
| `Properties.Settings.Default.PaperName` | `PrinterSettings.DefaultPageSettings.PaperSize` | Paper size |
| `Properties.Settings.Default.PrintToFile` + `DefaultPrintToDirectory` | `PrinterSettings.PrintToFile` + `PrintFileName` | Print-to-file |
| _(driver default)_ | `PrinterSettings.FromPage` / `ToPage` | Page range |
| _(driver default)_ | `PrinterSettings.Duplex` | Double-sided |
| _(driver default)_ | `PrinterSettings.Collate` | Collation |
| _(not available)_ | `pdf.Print(int dpi)` overload | Print quality |

### Core Print Operations

| PDFFilePrint Pattern | IronPDF Method | Notes |
|---------------------|----------------|-------|
| `new FilePrint(path, null).Print()` | `pdf.Print()` | Default printer |
| Configured via `app.config` | `var ps = new PrinterSettings { ... }` | Strongly typed |
| Errors surface as exceptions from PdfiumViewer | `try`/`catch IronPdfException` | Native errors |
| Sequential `foreach` only | `foreach` or `Parallel.ForEach` | Native iteration |

### PrinterSettings Mapping (System.Drawing.Printing)

| PDFFilePrint Setting | .NET `PrinterSettings` Property | Type |
|-------------------|-------------------------------|------|
| `PrinterName` (config) | `PrinterName` | `string` |
| `Copies` (config) | `Copies` | `short` |
| _(no silent flag — always silent)_ | `pdf.Print(showDialog: false)` | `bool` |
| _(driver default)_ | `FromPage`, `ToPage` | `int` |
| `PaperName` (config) | `DefaultPageSettings.PaperSize` | `PaperSize` |
| _(driver default)_ | `Duplex` | `Duplex` enum |
| _(driver default)_ | `Collate` | `bool` |
| _(driver default)_ | `DefaultPageSettings.Color` | `bool` |
| _(not available)_ | `pdf.Print(dpi)` overload | `int` |

### New Capabilities (Not in PDFFilePrint)

| IronPDF Feature | Description |
|-----------------|-------------|
| `ChromePdfRenderer.RenderHtmlAsPdf()` | Create PDF from HTML |
| `ChromePdfRenderer.RenderUrlAsPdf()` | Create PDF from URL |
| `PdfDocument.Merge()` | Combine multiple PDFs |
| `pdf.CopyPages()` | Extract specific pages |
| `pdf.ApplyWatermark()` | Add watermarks |
| `pdf.SecuritySettings` | Password protection |
| `pdf.ExtractAllText()` | Extract text content |
| `pdf.RasterizeToImageFiles()` | Convert to images |
| `pdf.SignWithDigitalSignature()` | Digital signatures |

---

## Code Migration Examples

### Example 1: Basic PDF Printing

**Before (PDFFilePrint):**
```csharp
using PDFFilePrint;

public class PrintService
{
    public void PrintPdf(string pdfPath, string printerName)
    {
        // PDFFilePrint reads the printer name from app.config
        // (Properties.Settings.Default.PrinterName). To override at runtime
        // you have to mutate Settings.Default before instantiating FilePrint.
        Properties.Settings.Default.PrinterName = printerName;

        var fileprint = new FilePrint(pdfPath, null);
        fileprint.Print();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PrintService
{
    public PrintService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    }

    public void PrintPdf(string pdfPath, string printerName)
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        pdf.Print(printerName);
    }
}
```

### Example 2: Silent Printing with Multiple Copies

**Before (PDFFilePrint):**
```csharp
using PDFFilePrint;

public void PrintSilent(string pdfPath, int copies)
{
    // PDFFilePrint is always silent (no UI). Copies live in app.config.
    Properties.Settings.Default.Copies = copies;
    Properties.Settings.Default.PrinterName = "Default Printer";

    var fileprint = new FilePrint(pdfPath, null);
    fileprint.Print();
    // No timeout / cancellation hooks — Print() blocks until PdfiumViewer
    // hands the job to the spooler.
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Drawing.Printing;

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

### Example 3: Print Specific Page Range

**Before (PDFFilePrint):**
```csharp
using PDFFilePrint;

public void PrintPageRange(string pdfPath, string printerName, int startPage, int endPage)
{
    // PDFFilePrint has no first-class page-range API. Workarounds: pre-split
    // the PDF with another tool and feed the smaller file to FilePrint, or
    // rely on the printer driver's "current page" behaviour.
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

### Example 4: Duplex (Double-Sided) Printing

**Before (PDFFilePrint):**
```csharp
using PDFFilePrint;

public void PrintDuplex(string pdfPath, string printerName)
{
    // PDFFilePrint has no Duplex setting. You configure duplex on the printer
    // driver itself (or via the underlying PdfiumViewer PrintDocument), then
    // tell FilePrint which printer to target.
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

### Example 5: Batch Printing Multiple Files

**Before (PDFFilePrint):**
```csharp
using PDFFilePrint;
using System.IO;

public void BatchPrint(string folderPath, string printerName)
{
    Properties.Settings.Default.PrinterName = printerName;

    var pdfFiles = Directory.GetFiles(folderPath, "*.pdf");
    foreach (var pdfFile in pdfFiles)
    {
        try
        {
            var fileprint = new FilePrint(pdfFile, null);
            fileprint.Print();
        }
        catch (Exception ex)
        {
            // Errors surface from PdfiumViewer as plain exceptions —
            // no exit-code abstraction.
            Console.WriteLine($"Failed to print {pdfFile}: {ex.Message}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Drawing.Printing;
using System.IO;

public void BatchPrint(string folderPath, string printerName)
{
    var pdfFiles = Directory.GetFiles(folderPath, "*.pdf");

    var settings = new PrinterSettings { PrinterName = printerName };

    foreach (var pdfFile in pdfFiles)
    {
        try
        {
            var pdf = PdfDocument.FromFile(pdfFile);
            pdf.GetPrintDocument(settings).Print();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to print {pdfFile}: {ex.Message}");
        }
    }
}
```

### Example 6: Print with Async Pattern

**Before (PDFFilePrint):**
```csharp
using PDFFilePrint;
using System.Threading.Tasks;

public Task PrintAsync(string pdfPath, string printerName)
{
    // FilePrint.Print() is synchronous — wrap it in Task.Run if you want
    // to keep the calling thread responsive.
    return Task.Run(() =>
    {
        Properties.Settings.Default.PrinterName = printerName;
        var fileprint = new FilePrint(pdfPath, null);
        fileprint.Print();
    });
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Threading.Tasks;

public Task PrintAsync(string pdfPath, string printerName)
{
    return Task.Run(() =>
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        pdf.Print(printerName);
    });
}
```

### Example 7: Create PDF Then Print (NEW Feature)

**Before (PDFFilePrint):**
```csharp
// PDFFilePrint CANNOT create PDFs. The FilePrint class only consumes
// existing PDF / XPS files. You need a separate renderer (e.g. IronPDF,
// PuppeteerSharp, wkhtmltopdf) to generate the PDF first.
throw new NotSupportedException("PDFFilePrint cannot create PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreateAndPrint(string html, string printerName)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Print directly without saving to disk
    pdf.Print(printerName);
}
```

### Example 8: Print URL Content Directly (NEW Feature)

**Before (PDFFilePrint):**
```csharp
// PDFFilePrint CANNOT fetch or render URLs. You must download/convert the
// URL to a PDF with another tool, then feed the resulting file to FilePrint.
throw new NotSupportedException("PDFFilePrint cannot convert URLs");
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Drawing.Printing;

public void PrintWebPage(string url, string printerName)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderUrlAsPdf(url);

    var settings = new PrinterSettings { PrinterName = printerName };
    pdf.GetPrintDocument(settings).Print();
}
```

### Example 9: Merge PDFs Then Print (NEW Feature)

**Before (PDFFilePrint):**
```csharp
// PDFFilePrint CANNOT merge PDFs. Merge with another tool first (e.g. IronPDF,
// PdfSharp), then hand the merged file to FilePrint.
throw new NotSupportedException("PDFFilePrint cannot merge PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;
using System.Linq;

public void MergeAndPrint(List<string> pdfPaths, string printerName)
{
    var pdfs = pdfPaths.Select(path => PdfDocument.FromFile(path)).ToList();
    var merged = PdfDocument.Merge(pdfs);

    merged.Print(printerName);
}
```

### Example 10: Print with Watermark (NEW Feature)

**Before (PDFFilePrint):**
```csharp
// PDFFilePrint CANNOT add watermarks. Apply a watermark with another library
// (IronPDF, iText, PdfSharp) and then print the watermarked file with FilePrint.
throw new NotSupportedException("PDFFilePrint cannot add watermarks");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

public void PrintWithWatermark(string pdfPath, string printerName, string watermarkText)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    pdf.ApplyWatermark(
        $"<div style='color:red; font-size:48px; opacity:0.3;'>{watermarkText}</div>",
        rotation: 45,
        verticalAlignment: VerticalAlignment.Middle,
        horizontalAlignment: HorizontalAlignment.Center);

    pdf.Print(printerName);
}
```

---

## Print Settings Reference

### Complete PrinterSettings Properties

```csharp
using System.Drawing.Printing;

var settings = new PrinterSettings
{
    // Printer selection
    PrinterName = "HP LaserJet Pro",

    // Copy settings
    Copies = 3,
    Collate = true,

    // Page range
    PrintRange = PrintRange.SomePages,
    FromPage = 1,
    ToPage = 10,

    // Duplex
    Duplex = Duplex.Vertical
};

// Page-level options (paper size, orientation, color, margins) live on
// DefaultPageSettings and are passed alongside PrinterSettings.
settings.DefaultPageSettings.Landscape = false;
settings.DefaultPageSettings.Color = true;

// Hand the assembled settings to IronPDF and print:
var pdf = PdfDocument.FromFile("document.pdf");
pdf.GetPrintDocument(settings).Print();

// IronPDF also exposes a DPI-only convenience overload:
// pdf.Print(dpi: 300);
```

---

## Printer Discovery

### List Available Printers

```csharp
using System.Drawing.Printing;

public IEnumerable<string> GetAvailablePrinters()
{
    foreach (string printer in PrinterSettings.InstalledPrinters)
    {
        yield return printer;
    }
}

public string GetDefaultPrinter()
{
    var settings = new PrinterSettings();
    return settings.PrinterName;
}

public bool PrinterExists(string printerName)
{
    return PrinterSettings.InstalledPrinters
        .Cast<string>()
        .Any(p => p.Equals(printerName, StringComparison.OrdinalIgnoreCase));
}
```

---

## Error Handling Migration

### Before (PDFFilePrint)

```csharp
using PDFFilePrint;

public void PrintWithErrorHandling(string pdfPath, string printerName)
{
    try
    {
        Properties.Settings.Default.PrinterName = printerName;
        var fileprint = new FilePrint(pdfPath, null);
        fileprint.Print();
    }
    catch (Exception ex)
    {
        // PDFFilePrint surfaces failures as plain exceptions bubbled up from
        // PdfiumViewer / the spooler. There is no granular error type — you
        // typically log the message and the inner exception.
        throw new Exception($"Print failed: {ex.Message}", ex);
    }
}
```

### After (IronPDF)

```csharp
using IronPdf;
using System.Drawing.Printing;
using System.IO;

public void PrintWithErrorHandling(string pdfPath, string printerName)
{
    // Validate printer exists
    if (!PrinterSettings.InstalledPrinters.Cast<string>()
        .Any(p => p.Equals(printerName, StringComparison.OrdinalIgnoreCase)))
    {
        throw new ArgumentException($"Printer not found: {printerName}");
    }

    // Validate file exists
    if (!File.Exists(pdfPath))
    {
        throw new FileNotFoundException("PDF file not found", pdfPath);
    }

    try
    {
        var pdf = PdfDocument.FromFile(pdfPath);

        var settings = new PrinterSettings { PrinterName = printerName };
        pdf.GetPrintDocument(settings).Print();
    }
    catch (IronPdf.Exceptions.IronPdfException ex)
    {
        throw new Exception($"PDF print error: {ex.Message}", ex);
    }
}
```

---

## Deployment Migration

### Before (PDFFilePrint)

1. Add `PDFFilePrint` 1.0.3 NuGet package (pulls PdfiumViewer + native Pdfium binaries).
2. Ship the matching `app.config` keys (`PrinterName`, `PaperName`, `Copies`, `PrintToFile`, `DefaultPrintToDirectory`).
3. Constrain deployment to **Windows .NET Framework 4.6.1+** — no .NET Core / Linux / macOS support.

### After (IronPDF)

1. Add NuGet package — dependencies resolved automatically across .NET Framework 4.6.2+ and .NET 6/7/8/9/10.
2. Set license key at startup.
3. Deploy — works on Windows, Linux (CUPS), macOS, and Docker.

```csharp
// Program.cs or Startup.cs
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## Common Migration Gotchas

### 1. Settings Live in app.config (PDFFilePrint) vs an Object (IronPDF)

```csharp
// PDFFilePrint: mutate Properties.Settings.Default before instantiating
Properties.Settings.Default.PrinterName = "HP LaserJet Pro";
new FilePrint(path, null).Print();

// IronPDF: pass a PrinterSettings instance per call
pdf.GetPrintDocument(new PrinterSettings { PrinterName = "HP LaserJet Pro" }).Print();
```

### 2. No Built-in Page Range or Duplex on PDFFilePrint

```csharp
// PDFFilePrint: rely on the printer driver default; no API knob.
// IronPDF: System.Drawing.Printing primitives.
settings.PrintRange = PrintRange.SomePages;
settings.FromPage = 2;
settings.ToPage = 5;
settings.Duplex = Duplex.Vertical;
```

### 3. Async / Cancellation

```csharp
// PDFFilePrint: synchronous only — wrap in Task.Run for fire-and-forget.
// IronPDF: prints synchronously by default; cancellation via Task + CTS:
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await Task.Run(() => pdf.Print(), cts.Token);
```

### 4. Path Handling

```csharp
// PDFFilePrint: forwards the path to PdfiumViewer; spaces are fine, no quoting.
// IronPDF: also accepts raw paths.
pdf.Print(); // file path was already passed to PdfDocument.FromFile
```

### 5. Default Printer

```csharp
// PDFFilePrint: leave PrinterName empty/null in app.config to use the default.
// IronPDF: omit printer name or pass null.
pdf.Print(); // Uses default printer
```

---

## Feature Comparison Summary

| Feature | PDFFilePrint | IronPDF |
|---------|--------------|---------|
| Basic printing | Yes | Yes |
| Silent printing | Yes (always silent) | Yes |
| Multiple copies | Yes (via config) | Yes |
| Page range | Driver default only | Yes |
| Duplex | Driver default only | Yes |
| Landscape | Driver default only | Yes |
| Create from HTML | No | Yes |
| Create from URL | No | Yes |
| Merge PDFs | No | Yes |
| Split PDFs | No | Yes |
| Add watermarks | No | Yes |
| Extract text | No | Yes |
| Password protection | No | Yes |
| Digital signatures | No | Yes |
| Cross-platform | No (Windows / net461) | Yes |
| Native .NET API | Yes (small surface) | Yes |
| NuGet package | Yes (`PDFFilePrint` 1.0.3, MIT) | Yes (`IronPdf`) |
| Active maintenance | No (last release 2020) | Yes |

---

## Pre-Migration Checklist

- [ ] Locate all `using PDFFilePrint;` and `new FilePrint(...)` call sites in the codebase
- [ ] Document current `app.config` settings (`PrinterName`, `PaperName`, `Copies`, `PrintToFile`, `DefaultPrintToDirectory`)
- [ ] Identify printer names used across environments
- [ ] List any external scripts that currently mutate `Settings.Default`
- [ ] Check for custom error handling around `FilePrint.Print()`
- [ ] Identify any PDF generation needs (currently fulfilled with separate tools)
- [ ] Review .NET Framework targeting — IronPDF supports Framework + .NET 6/7/8/9/10

---

## Post-Migration Checklist

- [ ] Remove the `PDFFilePrint` NuGet reference
- [ ] Remove PDFFilePrint-specific keys from `app.config`
- [ ] Add `IronPdf` NuGet package
- [ ] Set IronPDF license key in startup code
- [ ] Replace `new FilePrint(...).Print()` calls with `PdfDocument.FromFile(...).Print(...)` (or `GetPrintDocument`)
- [ ] Move printer/copy/page settings into `System.Drawing.Printing.PrinterSettings`
- [ ] Test printing on all target printers
- [ ] Test cross-platform if applicable (Linux requires CUPS)
- [ ] Update documentation

---

## Finding PDFFilePrint References

```bash
# Find usages of the PDFFilePrint API
grep -r "using PDFFilePrint\|new FilePrint" --include="*.cs" .

# Find related app.config keys
grep -r "PrinterName\|PaperName\|PrintToFile\|DefaultPrintToDirectory" \
    --include="*.config" --include="*.settings" .
```

---

## Troubleshooting

### "Printer not found"

```csharp
// Validate printer exists before printing
var printers = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
if (!printers.Contains(printerName))
{
    throw new Exception($"Printer '{printerName}' not found. Available: {string.Join(", ", printers)}");
}
```

### "License key required"

```csharp
// Set at application startup, before any IronPDF calls
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### "Print dialog appears when it shouldn't"

```csharp
// pdf.Print() is silent by default. The print-dialog overload requires
// passing true explicitly: pdf.Print(true). Make sure you aren't doing that.
pdf.Print();
```

### "Cross-platform printing issues"

Printing on Linux/macOS requires CUPS. Test thoroughly on non-Windows platforms — note that PDFFilePrint cannot run there at all (net461 + Win32 native binaries).

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PDFFilePrint usages in codebase**
  ```bash
  grep -r "using PDFFilePrint" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** PDFFilePrint settings (PrinterName, PaperName, Copies, PrintToFile, DefaultPrintToDirectory) live in app.config and map to IronPDF's `PrinterSettings` properties. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package PDFFilePrint
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace PDF generation calls**
  ```csharp
  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering — something PDFFilePrint cannot do at all.

- [ ] **Update page settings**
  ```csharp
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Configure headers/footers**
  ```csharp
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine provides modern rendering. Verify key documents.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  var merged = PdfDocument.Merge(pdf1, pdf2);
  pdf.SecuritySettings.UserPassword = "secret";
  pdf.ApplyWatermark("<h1>DRAFT</h1>");
  ```
  **Why:** IronPDF provides many additional features that PDFFilePrint never offered.

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Printing Guide](https://ironpdf.com/docs/questions/print-pdf/)
- [HTML to PDF Tutorial](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [API Reference](https://ironpdf.com/object-reference/api/)
- [PDFFilePrint NuGet page](https://www.nuget.org/packages/PDFFilePrint/)
