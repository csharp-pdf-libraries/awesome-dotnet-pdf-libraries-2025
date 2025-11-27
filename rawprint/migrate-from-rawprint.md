# Migration Guide: RawPrint → IronPDF

## Why Migrate from RawPrint to IronPDF

RawPrint is a low-level printing utility that sends raw bytes directly to printer spoolers, requiring manual PDF generation and platform-specific implementations. IronPDF provides a comprehensive, cross-platform solution for creating, manipulating, and printing PDFs with high-level APIs that handle rendering, formatting, and printer communication automatically. Migrating to IronPDF eliminates the need for raw printer commands and provides a modern, maintainable approach to PDF generation and printing.

## NuGet Package Changes

```bash
# Remove RawPrint
dotnet remove package RawPrint

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| RawPrint | IronPDF |
|----------|---------|
| `RawPrint` | `IronPdf` |
| N/A | `IronPdf.Rendering` |
| N/A | `IronPdf.Configuration` |

## API Mapping

| RawPrint API | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `Printer.SendBytesToPrinter()` | `PdfDocument.Print()` | High-level printing with dialog |
| `Printer.OpenPrinter()` | `ChromePdfRenderer` | Create PDF first, then print |
| `Printer.ClosePrinter()` | Auto-managed | No manual cleanup needed |
| N/A | `PdfDocument.RenderHtmlAsPdf()` | Create PDF from HTML |
| N/A | `PdfDocument.PrintWithoutDialog()` | Silent printing |
| N/A | `PdfDocument.SaveAs()` | Save to file before/instead of printing |

## Code Examples

### Example 1: Basic Printing

**Before (RawPrint):**
```csharp
using RawPrint;
using System.IO;

// Read PDF bytes
byte[] pdfBytes = File.ReadAllBytes("document.pdf");

// Send raw bytes to printer
bool success = Printer.SendBytesToPrinter(
    "Brother HL-L2340D series",
    pdfBytes,
    pdfBytes.Length
);
```

**After (IronPDF):**
```csharp
using IronPdf;

// Load existing PDF
var pdf = PdfDocument.FromFile("document.pdf");

// Print with dialog
pdf.Print();

// Or print silently to specific printer
pdf.PrintWithoutDialog("Brother HL-L2340D series");
```

### Example 2: Creating and Printing PDF from HTML

**Before (RawPrint):**
```csharp
using RawPrint;
using System.IO;

// Need external library to create PDF first
// (RawPrint doesn't create PDFs)
byte[] pdfBytes = SomeExternalLibrary.CreatePdfFromHtml(htmlContent);

// Send to printer
Printer.SendBytesToPrinter(
    "Microsoft Print to PDF",
    pdfBytes,
    pdfBytes.Length
);
```

**After (IronPDF):**
```csharp
using IronPdf;

string htmlContent = "<h1>Invoice</h1><p>Amount: $100.00</p>";

// Create PDF from HTML
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);

// Print directly
pdf.Print();

// Or save and print
pdf.SaveAs("invoice.pdf");
pdf.PrintWithoutDialog("Microsoft Print to PDF");
```

### Example 3: Advanced Printing with Configuration

**Before (RawPrint):**
```csharp
using RawPrint;
using System;
using System.IO;

// Manual printer handle management
IntPtr printerHandle = IntPtr.Zero;

try
{
    Printer.OpenPrinter("HP LaserJet", out printerHandle);
    
    byte[] pdfBytes = File.ReadAllBytes("report.pdf");
    
    // Low-level print job control
    Printer.StartDocPrinter(printerHandle, 1, "Report");
    Printer.WritePrinter(printerHandle, pdfBytes, pdfBytes.Length);
    Printer.EndDocPrinter(printerHandle);
}
finally
{
    if (printerHandle != IntPtr.Zero)
        Printer.ClosePrinter(printerHandle);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

// Load PDF
var pdf = PdfDocument.FromFile("report.pdf");

// Configure print settings
var printSettings = new IronPdf.Printing.PrintSettings
{
    PrinterName = "HP LaserJet",
    NumberOfCopies = 2,
    PaperOrientation = IronPdf.Printing.PaperOrientation.Portrait,
    DuplexMode = IronPdf.Printing.DuplexMode.TwoSidedLongEdge
};

// Print with settings (no manual cleanup needed)
pdf.Print(printSettings);
```

## Common Gotchas

### 1. **License Required for Production**
IronPDF requires a license for commercial use. Set your license key before use:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 2. **Different Architecture Support**
- RawPrint works directly with Windows printer spooler
- IronPDF is cross-platform (Windows, Linux, macOS) but may require Docker for Linux
- Ensure the correct runtime package is installed

### 3. **PDF Creation vs. Raw Printing**
- RawPrint assumes you already have PDF bytes
- IronPDF focuses on creating PDFs first, then printing
- If you only need to print existing PDFs, use `PdfDocument.FromFile()` or `PdfDocument.FromStream()`

### 4. **Printer Name Format**
- Both libraries use similar printer names, but IronPDF may be more forgiving with partial matches
- Test printer names with `System.Drawing.Printing.PrinterSettings.InstalledPrinters`

### 5. **Silent Printing Permissions**
- `PrintWithoutDialog()` may require elevated permissions depending on system configuration
- Test in your target environment early

### 6. **Async Operations**
- IronPDF operations are synchronous by default
- For web applications, consider running print operations in background tasks to avoid blocking

## Documentation & Resources

- **Official Documentation:** https://ironpdf.com/docs/
- **Tutorials & Examples:** https://ironpdf.com/tutorials/
- **API Reference:** https://ironpdf.com/docs/api/
- **Printing Guide:** https://ironpdf.com/tutorials/csharp-print-pdf/