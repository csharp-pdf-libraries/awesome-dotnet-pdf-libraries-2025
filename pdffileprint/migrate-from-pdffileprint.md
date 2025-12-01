# How Do I Migrate from PDFFilePrint to IronPDF in C#?

## Why Migrate from PDFFilePrint to IronPDF?

PDFFilePrint is a command-line utility for printing PDF files. While useful for simple batch printing, it creates significant architectural limitations:

### Critical PDFFilePrint Limitations

1. **Printing-Only**: Cannot create, edit, merge, or manipulate PDFs
2. **Command-Line Dependency**: Requires external executable, Process.Start() calls
3. **Windows-Only**: Relies on Windows printing subsystem
4. **No .NET Integration**: No native API, no NuGet package, no IntelliSense
5. **External Process Management**: Must handle process lifecycle, exit codes, errors
6. **Limited Error Handling**: Parsing stdout/stderr for error detection
7. **Deployment Complexity**: Must bundle PDFFilePrint.exe with application
8. **No PDF Generation**: Can't create PDFs—only print existing ones

### IronPDF Advantages

| Aspect | PDFFilePrint | IronPDF |
|--------|--------------|---------|
| **Type** | Command-line utility | Native .NET library |
| **Integration** | Process.Start() | Direct API calls |
| **PDF Printing** | ✓ | ✓ |
| **PDF Creation** | ✗ | ✓ (HTML, URL, images) |
| **PDF Manipulation** | ✗ | ✓ (merge, split, edit) |
| **Cross-Platform** | Windows only | Windows, Linux, macOS |
| **Error Handling** | Parse stdout/stderr | Native exceptions |
| **IntelliSense** | ✗ | ✓ |
| **Async Support** | Manual | Built-in |
| **NuGet Package** | ✗ | ✓ |

---

## NuGet Package Changes

```bash
# PDFFilePrint has no NuGet package
# Remove the bundled PDFFilePrint.exe from your deployment

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// PDFFilePrint - No namespaces, just Process execution
using System.Diagnostics;

// IronPDF
using IronPdf;
using IronPdf.Printing;
```

---

## Complete API Reference

### Command-Line to API Mapping

| PDFFilePrint Command | IronPDF API | Notes |
|---------------------|-------------|-------|
| `PDFFilePrint.exe "file.pdf" "Printer"` | `pdf.Print("Printer")` | Basic printing |
| `-printer "Name"` | `PrintSettings.PrinterName = "Name"` | Printer selection |
| `-copies N` | `PrintSettings.NumberOfCopies = N` | Copy count |
| `-silent` | `PrintSettings.ShowPrintDialog = false` | Silent mode |
| `-pages "1-5"` | `PrintSettings.FromPage`, `PrintSettings.ToPage` | Page range |
| `-orientation landscape` | `PrintSettings.PaperOrientation = Landscape` | Orientation |
| `-duplex` | `PrintSettings.Duplex = Duplex.Vertical` | Double-sided |
| `-collate` | `PrintSettings.Collate = true` | Collation |
| `-fit` | Print scaling options | Fit to page |

### Core Print Operations

| PDFFilePrint Pattern | IronPDF Method | Notes |
|---------------------|----------------|-------|
| `Process.Start("PDFFilePrint.exe", args)` | `pdf.Print()` | Direct print |
| Parse exit code | Exception handling | Native errors |
| Parse stdout for status | Method returns | Direct feedback |
| Batch script loops | `foreach` loop or `Parallel.ForEach` | Native iteration |

### Print Settings Mapping

| PDFFilePrint Flag | IronPDF PrintSettings Property | Type |
|-------------------|-------------------------------|------|
| `-printer` | `PrinterName` | `string` |
| `-copies` | `NumberOfCopies` | `int` |
| `-silent` | `ShowPrintDialog` | `bool` (false = silent) |
| `-pages "1-5"` | `FromPage`, `ToPage` | `int` |
| `-orientation` | `PaperOrientation` | `PdfPrintOrientation` |
| `-duplex` | `Duplex` | `Duplex` enum |
| `-collate` | `Collate` | `bool` |
| `-color` | `PrintInColor` | `bool` |
| _(not available)_ | `DPI` | `int` (print quality) |

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
using System.Diagnostics;

public class PrintService
{
    private readonly string _pdfFilePrintPath = @"C:\tools\PDFFilePrint.exe";

    public void PrintPdf(string pdfPath, string printerName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _pdfFilePrintPath,
            Arguments = $"-printer \"{printerName}\" \"{pdfPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using (var process = Process.Start(startInfo))
        {
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                throw new Exception($"Print failed: {error}");
            }
        }
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
using System.Diagnostics;

public void PrintSilent(string pdfPath, int copies)
{
    var args = $"-silent -copies {copies} -printer \"Default Printer\" \"{pdfPath}\"";

    var startInfo = new ProcessStartInfo
    {
        FileName = @"C:\tools\PDFFilePrint.exe",
        Arguments = args,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };

    using (var process = new Process { StartInfo = startInfo })
    {
        process.Start();
        process.WaitForExit(30000); // 30 second timeout

        if (!process.HasExited)
        {
            process.Kill();
            throw new TimeoutException("Print job timed out");
        }

        if (process.ExitCode != 0)
        {
            throw new Exception($"Print failed with exit code {process.ExitCode}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void PrintSilent(string pdfPath, int copies)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    var settings = new PrintSettings
    {
        ShowPrintDialog = false,
        NumberOfCopies = copies
    };

    pdf.Print(settings);
}
```

### Example 3: Print Specific Page Range

**Before (PDFFilePrint):**
```csharp
using System.Diagnostics;

public void PrintPageRange(string pdfPath, string printerName, int startPage, int endPage)
{
    // PDFFilePrint page range syntax varies by version
    var args = $"-silent -printer \"{printerName}\" -pages \"{startPage}-{endPage}\" \"{pdfPath}\"";

    var startInfo = new ProcessStartInfo
    {
        FileName = @"C:\tools\PDFFilePrint.exe",
        Arguments = args,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using (var process = Process.Start(startInfo))
    {
        process.WaitForExit();

        // No reliable way to know if specific pages printed successfully
        if (process.ExitCode != 0)
        {
            throw new Exception("Print operation failed");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void PrintPageRange(string pdfPath, string printerName, int startPage, int endPage)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    var settings = new PrintSettings
    {
        ShowPrintDialog = false,
        PrinterName = printerName,
        FromPage = startPage,
        ToPage = endPage
    };

    pdf.Print(settings);
}
```

### Example 4: Duplex (Double-Sided) Printing

**Before (PDFFilePrint):**
```csharp
using System.Diagnostics;

public void PrintDuplex(string pdfPath, string printerName)
{
    // Duplex support depends on PDFFilePrint version and printer drivers
    var args = $"-silent -duplex -printer \"{printerName}\" \"{pdfPath}\"";

    var startInfo = new ProcessStartInfo
    {
        FileName = @"C:\tools\PDFFilePrint.exe",
        Arguments = args,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using (var process = Process.Start(startInfo))
    {
        process.WaitForExit();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Drawing.Printing;

public void PrintDuplex(string pdfPath, string printerName)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    var settings = new PrintSettings
    {
        ShowPrintDialog = false,
        PrinterName = printerName,
        Duplex = Duplex.Vertical // Long edge binding
    };

    pdf.Print(settings);
}
```

### Example 5: Batch Printing Multiple Files

**Before (PDFFilePrint):**
```csharp
using System.Diagnostics;
using System.IO;

public void BatchPrint(string folderPath, string printerName)
{
    var pdfFiles = Directory.GetFiles(folderPath, "*.pdf");

    foreach (var pdfFile in pdfFiles)
    {
        var args = $"-silent -printer \"{printerName}\" \"{pdfFile}\"";

        var startInfo = new ProcessStartInfo
        {
            FileName = @"C:\tools\PDFFilePrint.exe",
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(startInfo))
        {
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine($"Failed to print: {pdfFile}");
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.IO;

public void BatchPrint(string folderPath, string printerName)
{
    var pdfFiles = Directory.GetFiles(folderPath, "*.pdf");

    var settings = new PrintSettings
    {
        ShowPrintDialog = false,
        PrinterName = printerName
    };

    foreach (var pdfFile in pdfFiles)
    {
        try
        {
            var pdf = PdfDocument.FromFile(pdfFile);
            pdf.Print(settings);
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
using System.Diagnostics;
using System.Threading.Tasks;

public async Task PrintAsync(string pdfPath, string printerName)
{
    await Task.Run(() =>
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = @"C:\tools\PDFFilePrint.exe",
            Arguments = $"-silent -printer \"{printerName}\" \"{pdfPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(startInfo))
        {
            process.WaitForExit();
        }
    });
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Threading.Tasks;

public async Task PrintAsync(string pdfPath, string printerName)
{
    await Task.Run(() =>
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        pdf.Print(printerName);
    });
}
```

### Example 7: Create PDF Then Print (NEW Feature)

**Before (PDFFilePrint):**
```csharp
// PDFFilePrint CANNOT create PDFs
// You need a separate tool to generate the PDF first
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
// PDFFilePrint CANNOT convert URLs to PDF
// You need to download/convert the URL first with another tool
throw new NotSupportedException("PDFFilePrint cannot convert URLs");
```

**After (IronPDF):**
```csharp
using IronPdf;

public void PrintWebPage(string url, string printerName)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderUrlAsPdf(url);

    var settings = new PrintSettings
    {
        ShowPrintDialog = false,
        PrinterName = printerName
    };

    pdf.Print(settings);
}
```

### Example 9: Merge PDFs Then Print (NEW Feature)

**Before (PDFFilePrint):**
```csharp
// PDFFilePrint CANNOT merge PDFs
// Must use another tool to merge, then print the result
throw new NotSupportedException("PDFFilePrint cannot merge PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;
using System.IO;

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
// PDFFilePrint CANNOT add watermarks
// Must use another tool to watermark, then print
throw new NotSupportedException("PDFFilePrint cannot add watermarks");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

public void PrintWithWatermark(string pdfPath, string printerName, string watermarkText)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    // Add watermark
    pdf.ApplyWatermark(
        $"<div style='color:red; font-size:48px; opacity:0.3;'>{watermarkText}</div>",
        45,
        VerticalAlignment.Middle,
        HorizontalAlignment.Center);

    pdf.Print(printerName);
}
```

---

## Print Settings Reference

### Complete PrintSettings Properties

```csharp
var settings = new PrintSettings
{
    // Printer selection
    PrinterName = "HP LaserJet Pro",

    // Dialog control
    ShowPrintDialog = false,

    // Copy settings
    NumberOfCopies = 3,
    Collate = true,

    // Page range
    FromPage = 1,
    ToPage = 10,

    // Orientation
    PaperOrientation = PdfPrintOrientation.Portrait,

    // Color
    PrintInColor = true,

    // Duplex
    Duplex = Duplex.Vertical,

    // Quality
    DPI = 300
};
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
using System.Diagnostics;

public void PrintWithErrorHandling(string pdfPath, string printerName)
{
    try
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = @"C:\tools\PDFFilePrint.exe",
            Arguments = $"-silent -printer \"{printerName}\" \"{pdfPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        using (var process = Process.Start(startInfo))
        {
            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                // Parse error message from stderr
                throw new Exception($"Print failed (exit code {process.ExitCode}): {stderr}");
            }

            // Parse stdout to confirm success
            if (!stdout.Contains("Success") && !string.IsNullOrEmpty(stdout))
            {
                Console.WriteLine($"Warning: {stdout}");
            }
        }
    }
    catch (System.ComponentModel.Win32Exception ex)
    {
        throw new Exception("PDFFilePrint.exe not found", ex);
    }
    catch (InvalidOperationException ex)
    {
        throw new Exception("Failed to start PDFFilePrint process", ex);
    }
}
```

### After (IronPDF)

```csharp
using IronPdf;
using System.Drawing.Printing;

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

        var settings = new PrintSettings
        {
            ShowPrintDialog = false,
            PrinterName = printerName
        };

        pdf.Print(settings);
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

1. Bundle `PDFFilePrint.exe` with application
2. Set correct path or add to PATH
3. Ensure Windows dependencies are installed
4. Handle different versions across machines

### After (IronPDF)

1. Add NuGet package - dependencies resolved automatically
2. Set license key at startup
3. Deploy - works on Windows, Linux, macOS

```csharp
// Program.cs or Startup.cs
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## Common Migration Gotchas

### 1. Silent Mode Flag

```csharp
// PDFFilePrint: -silent flag
// IronPDF: ShowPrintDialog = false (inverted logic)
var settings = new PrintSettings { ShowPrintDialog = false };
```

### 2. Exit Code vs Exceptions

```csharp
// PDFFilePrint: Check process.ExitCode
if (process.ExitCode != 0) { ... }

// IronPDF: Use try-catch
try { pdf.Print(); }
catch (Exception ex) { ... }
```

### 3. Process Timeout Handling

```csharp
// PDFFilePrint: Manual timeout
process.WaitForExit(30000);
if (!process.HasExited) process.Kill();

// IronPDF: Prints synchronously, no timeout needed
// For long jobs, wrap in Task with cancellation:
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await Task.Run(() => pdf.Print(), cts.Token);
```

### 4. Path Handling

```csharp
// PDFFilePrint: Quote paths with spaces
Arguments = $"\"{pathWithSpaces}\"";

// IronPDF: No quoting needed
pdf.Print(pathWithSpaces);
```

### 5. Default Printer

```csharp
// PDFFilePrint: Use -printer "Default Printer" or omit
// IronPDF: Omit printer name or use null
pdf.Print(); // Uses default printer
```

---

## Feature Comparison Summary

| Feature | PDFFilePrint | IronPDF |
|---------|--------------|---------|
| Basic printing | ✓ | ✓ |
| Silent printing | ✓ | ✓ |
| Multiple copies | ✓ | ✓ |
| Page range | ✓ | ✓ |
| Duplex | Varies | ✓ |
| Landscape | ✓ | ✓ |
| Create from HTML | ✗ | ✓ |
| Create from URL | ✗ | ✓ |
| Merge PDFs | ✗ | ✓ |
| Split PDFs | ✗ | ✓ |
| Add watermarks | ✗ | ✓ |
| Extract text | ✗ | ✓ |
| Password protection | ✗ | ✓ |
| Digital signatures | ✗ | ✓ |
| Cross-platform | ✗ | ✓ |
| Native .NET API | ✗ | ✓ |
| NuGet package | ✗ | ✓ |
| IntelliSense | ✗ | ✓ |

---

## Pre-Migration Checklist

- [ ] Locate all PDFFilePrint.exe calls in codebase
- [ ] Document current command-line arguments used
- [ ] Identify printer names used across environments
- [ ] List any batch scripts using PDFFilePrint
- [ ] Check for custom error handling around Process.Start
- [ ] Identify any PDF generation needs (currently using separate tools)
- [ ] Review deployment process for PDFFilePrint.exe bundling

---

## Post-Migration Checklist

- [ ] Remove PDFFilePrint.exe from source control
- [ ] Remove PDFFilePrint.exe from deployment packages
- [ ] Update build scripts to remove PDFFilePrint copying
- [ ] Add IronPDF NuGet package
- [ ] Set IronPDF license key in startup code
- [ ] Replace Process.Start calls with IronPDF API
- [ ] Replace exit code checks with exception handling
- [ ] Test printing on all target printers
- [ ] Test cross-platform if applicable
- [ ] Update documentation

---

## Finding PDFFilePrint References

```bash
# Find command-line execution patterns
grep -r "PDFFilePrint\|ProcessStartInfo.*pdf\|Process.Start.*print" --include="*.cs" .

# Find batch scripts
find . -name "*.bat" -o -name "*.cmd" -o -name "*.ps1" | xargs grep -l "PDFFilePrint"

# Find configuration references
grep -r "PDFFilePrint" --include="*.json" --include="*.config" --include="*.xml" .
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
// Ensure ShowPrintDialog is explicitly false
var settings = new PrintSettings { ShowPrintDialog = false };
pdf.Print(settings);
```

### "Cross-platform printing issues"

Printing on Linux/macOS requires CUPS. Test thoroughly on non-Windows platforms.

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
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

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
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering.

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
  **Why:** IronPDF provides many additional features.

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Printing Guide](https://ironpdf.com/docs/questions/print-pdf/)
- [HTML to PDF Tutorial](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [API Reference](https://ironpdf.com/object-reference/api/)
