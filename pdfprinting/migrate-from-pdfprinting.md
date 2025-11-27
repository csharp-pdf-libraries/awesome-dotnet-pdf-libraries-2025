# Migration Guide: PDFPrinting.NET → IronPDF

## Why Migrate to IronPDF

IronPDF is a comprehensive PDF library that not only prints PDFs but also creates, edits, and manipulates them programmatically. Unlike PDFPrinting.NET's Windows-only printing capabilities, IronPDF works cross-platform (Windows, Linux, macOS) and provides enterprise-grade features including HTML-to-PDF conversion, digital signatures, form filling, and advanced rendering options. Migrating to IronPDF future-proofs your application with a full-featured PDF toolkit while maintaining printing functionality.

## NuGet Package Changes

```bash
# Remove old package
dotnet remove package PDFPrinting.NET

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| PDFPrinting.NET | IronPDF |
|----------------|---------|
| `PDFPrinting` | `IronPdf` |
| N/A | `IronPdf.Rendering` |
| N/A | `IronPdf.Signing` |

## API Mapping

| PDFPrinting.NET | IronPDF | Notes |
|----------------|---------|-------|
| `PDFPrintDocument` | `PdfDocument` | Core PDF object |
| `Print(string path)` | `PdfDocument.Print()` | Print existing PDF |
| `Print(string path, string printerName)` | `PdfDocument.Print(printerName)` | Print to specific printer |
| N/A | `ChromePdfRenderer.RenderHtmlAsPdf()` | Create PDFs from HTML |
| N/A | `PdfDocument.FromFile()` | Load existing PDF |
| N/A | `PdfDocument.SaveAs()` | Save PDF to file |

## Code Examples

### Example 1: Basic PDF Printing

**Before (PDFPrinting.NET):**
```csharp
using PDFPrinting;

var pdfPrinter = new PDFPrintDocument();
pdfPrinter.Print("document.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
pdf.Print();
```

### Example 2: Print to Specific Printer

**Before (PDFPrinting.NET):**
```csharp
using PDFPrinting;

var pdfPrinter = new PDFPrintDocument();
pdfPrinter.Print("document.pdf", "HP LaserJet Printer");
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
pdf.Print("HP LaserJet Printer");
```

### Example 3: Advanced Printing with Settings

**Before (PDFPrinting.NET):**
```csharp
using PDFPrinting;
using System.Drawing.Printing;

var pdfPrinter = new PDFPrintDocument();
var printDoc = pdfPrinter.GetPrintDocument("document.pdf");
printDoc.PrinterSettings.Copies = 2;
printDoc.Print();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
var printSettings = new IronPdf.Printing.PrintSettings
{
    NumberOfCopies = 2,
    Dpi = 300
};
pdf.Print(printSettings);
```

## Common Gotchas

### 1. **Cross-Platform Printing**
PDFPrinting.NET only works on Windows. IronPDF printing works across platforms, but requires platform-specific printer drivers to be installed. On Linux, ensure CUPS is configured properly.

### 2. **License Requirement**
IronPDF requires a license key for commercial use. Set it before using:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```
Free trial available with watermarked output.

### 3. **Async Operations**
IronPDF supports async operations that PDFPrinting.NET doesn't:
```csharp
var pdf = await PdfDocument.FromFileAsync("document.pdf");
await pdf.SaveAsAsync("output.pdf");
```

### 4. **File Locking**
IronPDF may keep files open longer than PDFPrinting.NET. Use `using` statements or explicitly dispose:
```csharp
using (var pdf = PdfDocument.FromFile("document.pdf"))
{
    pdf.Print();
} // Automatically disposed
```

### 5. **Printer Name Format**
Ensure printer names match exactly (case-sensitive). Query available printers:
```csharp
var printers = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
foreach (string printer in printers)
{
    Console.WriteLine(printer);
}
```

### 6. **Expanded Capabilities**
Unlike PDFPrinting.NET, IronPDF can create PDFs from scratch. Consider generating PDFs programmatically instead of just printing:
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Generated PDF</h1>");
pdf.SaveAs("output.pdf");
```

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **Printing Guide**: https://ironpdf.com/docs/questions/print-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/