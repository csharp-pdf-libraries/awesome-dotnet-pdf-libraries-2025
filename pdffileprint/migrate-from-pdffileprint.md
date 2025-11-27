# Migration Guide: PDFFilePrint → IronPDF

## Why Migrate to IronPDF

PDFFilePrint is limited to printing PDF files and typically requires command-line execution with Windows-specific dependencies. IronPDF provides a comprehensive .NET library that not only handles PDF printing but also enables PDF creation, manipulation, conversion, and advanced features like digital signatures and form filling. Migrating to IronPDF gives you a unified, cross-platform solution with better integration into modern .NET applications.

## NuGet Package Changes

```bash
# Remove PDFFilePrint (if installed via package manager)
# No official NuGet package exists for PDFFilePrint

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| PDFFilePrint | IronPDF |
|--------------|---------|
| N/A (Command-line tool) | `IronPdf` |
| N/A | `IronPdf.Rendering` |

## API Mapping

| PDFFilePrint | IronPDF | Notes |
|--------------|---------|-------|
| `PDFFilePrint.exe <file> <printer>` | `PdfDocument.Load().Print()` | Direct printing from code |
| Command-line arguments | `PrintSettings` class | Programmatic configuration |
| `/silent` flag | `PrintSettings.ShowPrintDialog = false` | Silent printing |
| `/printer:<name>` | `PrintSettings.PrinterName` | Printer selection |
| `/copies:<n>` | `PrintSettings.NumberOfCopies` | Copy control |
| `/orientation:landscape` | `PrintSettings.PaperOrientation` | Page orientation |

## Code Examples

### Example 1: Basic PDF Printing

**Before (PDFFilePrint - Command-line execution):**
```csharp
using System.Diagnostics;

var pdfPath = @"C:\documents\invoice.pdf";
var printerName = "HP LaserJet Pro";

var startInfo = new ProcessStartInfo
{
    FileName = @"C:\tools\PDFFilePrint.exe",
    Arguments = $"\"{pdfPath}\" \"{printerName}\"",
    UseShellExecute = false,
    CreateNoWindow = true
};

Process.Start(startInfo);
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdfPath = @"C:\documents\invoice.pdf";
var printerName = "HP LaserJet Pro";

var pdf = PdfDocument.FromFile(pdfPath);
pdf.Print(printerName);
```

### Example 2: Silent Printing with Multiple Copies

**Before (PDFFilePrint):**
```csharp
using System.Diagnostics;

var pdfPath = @"C:\reports\monthly-report.pdf";
var args = $"/silent /printer:\"Default Printer\" /copies:3 \"{pdfPath}\"";

var startInfo = new ProcessStartInfo
{
    FileName = @"C:\tools\PDFFilePrint.exe",
    Arguments = args,
    UseShellExecute = false,
    CreateNoWindow = true,
    RedirectStandardOutput = true
};

var process = Process.Start(startInfo);
process.WaitForExit();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdfPath = @"C:\reports\monthly-report.pdf";

var pdf = PdfDocument.FromFile(pdfPath);
var printSettings = new PrintSettings
{
    ShowPrintDialog = false,
    NumberOfCopies = 3
};

pdf.Print(printSettings);
```

### Example 3: Landscape Printing with Custom Settings

**Before (PDFFilePrint):**
```csharp
using System.Diagnostics;
using System.IO;

var pdfPath = @"C:\charts\sales-chart.pdf";

if (File.Exists(pdfPath))
{
    var args = $"/silent /orientation:landscape /printer:\"Color Printer\" \"{pdfPath}\"";
    
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = @"C:\tools\PDFFilePrint.exe",
            Arguments = args,
            UseShellExecute = false
        }
    };
    
    process.Start();
    process.WaitForExit();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdfPath = @"C:\charts\sales-chart.pdf";

var pdf = PdfDocument.FromFile(pdfPath);
var printSettings = new PrintSettings
{
    ShowPrintDialog = false,
    PaperOrientation = PdfPrintOrientation.Landscape,
    PrinterName = "Color Printer"
};

pdf.Print(printSettings);
```

## Common Gotchas

1. **Licensing Required**: IronPDF requires a license key for commercial use. Set it with `IronPdf.License.LicenseKey = "YOUR-KEY"` at application startup.

2. **Print Dialog Behavior**: By default, IronPDF shows a print dialog. Set `ShowPrintDialog = false` in `PrintSettings` for silent printing like PDFFilePrint's `/silent` flag.

3. **Printer Name Validation**: IronPDF will throw an exception if the specified printer doesn't exist. Use `PrinterSettings.InstalledPrinters` to validate printer names before printing.

4. **Cross-Platform Considerations**: Unlike PDFFilePrint (Windows-only), IronPDF supports Linux and macOS, but printing functionality may have platform-specific limitations. Test thoroughly on your target platform.

5. **Asynchronous Operations**: IronPDF's `Print()` method is synchronous. For long-running print jobs, consider wrapping calls in `Task.Run()` to avoid blocking the UI thread.

6. **PDF Generation Bonus**: IronPDF can create PDFs from HTML, URLs, and other sources—eliminating the need for separate PDF generation tools you may have used alongside PDFFilePrint.

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **Printing Guide**: https://ironpdf.com/docs/questions/print-pdf/