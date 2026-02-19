# How Do I Migrate from RawPrint to IronPDF in C#?

## Why Migrate from RawPrint?

RawPrint is a low-level printing utility that sends raw bytes to printer spoolers. It's NOT a PDF library - it just pushes data to printers. Key limitations:

1. **No PDF Creation**: Cannot create or generate PDFs
2. **Windows-Only**: Relies on Windows printing subsystem
3. **Low-Level API**: Manual printer handle management
4. **No Document Processing**: Just byte transmission
5. **Limited Control**: Minimal print job configuration
6. **No Cross-Platform**: Tied to Windows spooler

### What RawPrint Does vs. What You Need

| Task | RawPrint | IronPDF |
|------|----------|---------|
| Create PDF from HTML | NO | Yes |
| Create PDF from URL | NO | Yes |
| Edit/Modify PDFs | NO | Yes |
| Merge/Split PDFs | NO | Yes |
| Print Existing PDF | Yes (raw bytes) | Yes (high-level API) |
| Print Control | Basic | Full options |
| Cross-Platform | Windows only | Yes |

---

## Quick Start: RawPrint to IronPDF

### Step 1: Replace NuGet Package

```bash
# Remove RawPrint
dotnet remove package RawPrint

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Initialize License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 3: Replace Printing Code

**RawPrint:**
```csharp
using RawPrint;

byte[] pdfBytes = File.ReadAllBytes("document.pdf");
Printer.SendBytesToPrinter("HP LaserJet", pdfBytes, pdfBytes.Length);
```

**IronPDF:**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
pdf.Print("HP LaserJet");
```

---

## API Mapping Reference

| RawPrint | IronPDF | Notes |
|----------|---------|-------|
| `Printer.SendBytesToPrinter()` | `pdf.Print()` | High-level printing |
| `Printer.OpenPrinter()` | N/A | Not needed |
| `Printer.ClosePrinter()` | N/A | Automatic |
| `Printer.StartDocPrinter()` | N/A | Automatic |
| `Printer.WritePrinter()` | N/A | Automatic |
| `Printer.EndDocPrinter()` | N/A | Automatic |
| N/A | `ChromePdfRenderer` | Create PDFs |
| N/A | `PdfDocument.Merge()` | Merge PDFs |
| N/A | `pdf.ApplyWatermark()` | Add watermarks |

---

## Code Examples

### Example 1: Print Existing PDF

**RawPrint:**
```csharp
using RawPrint;
using System.IO;

byte[] pdfBytes = File.ReadAllBytes("document.pdf");
bool success = Printer.SendBytesToPrinter(
    "Brother HL-L2340D",
    pdfBytes,
    pdfBytes.Length
);

if (!success)
{
    throw new Exception("Print failed");
}
```

**IronPDF:**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Simple print
pdf.Print();

// Or specify printer
pdf.Print("Brother HL-L2340D");
```

### Example 2: Create and Print PDF (RawPrint Can't Do This)

**RawPrint:**
```csharp
// IMPOSSIBLE - RawPrint cannot create PDFs
// Would need another library just to create the PDF first
```

**IronPDF:**
```csharp
using IronPdf;

// Create PDF from HTML
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(@"
    <h1>Invoice #12345</h1>
    <p>Customer: John Doe</p>
    <p>Amount: $150.00</p>
");

// Print directly
pdf.Print("HP LaserJet");

// Or save first
pdf.SaveAs("invoice.pdf");
pdf.Print();
```

### Example 3: Print with Settings

**RawPrint:**
```csharp
using RawPrint;
using System;

// Manual printer handle management
IntPtr printerHandle = IntPtr.Zero;
try
{
    Printer.OpenPrinter("HP LaserJet", out printerHandle);
    Printer.StartDocPrinter(printerHandle, 1, "Document");

    byte[] pdfBytes = File.ReadAllBytes("report.pdf");
    Printer.WritePrinter(printerHandle, pdfBytes, pdfBytes.Length);

    Printer.EndDocPrinter(printerHandle);
}
finally
{
    if (printerHandle != IntPtr.Zero)
        Printer.ClosePrinter(printerHandle);
}
```

**IronPDF:**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("report.pdf");

// Print with full configuration
pdf.Print(new PrintOptions
{
    PrinterName = "HP LaserJet",
    NumberOfCopies = 2,
    DPI = 300,
    GrayScale = false
});
```

### Example 4: Silent/Background Printing

**RawPrint:**
```csharp
using RawPrint;

// RawPrint is inherently silent but has no dialog option
byte[] data = File.ReadAllBytes("document.pdf");
Printer.SendBytesToPrinter("Microsoft Print to PDF", data, data.Length);
```

**IronPDF:**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Silent print (no dialog)
pdf.Print("Microsoft Print to PDF");

// Or with dialog
pdf.PrintWithDialog();
```

### Example 5: Print Generated Report

**RawPrint:**
```csharp
// Requires external library to create PDF, then:
using RawPrint;

// Assume you used something else to create pdfBytes
byte[] pdfBytes = SomeOtherLibrary.CreatePdf(data);
Printer.SendBytesToPrinter("Printer Name", pdfBytes, pdfBytes.Length);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Set page options
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

// Headers and footers
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Monthly Report</div>",
    MaxHeight = 25
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>",
    MaxHeight = 25
};

// Generate and print
var pdf = renderer.RenderHtmlAsPdf(reportHtml);
pdf.Print("HP LaserJet Pro");
```

### Example 6: Batch Printing

**RawPrint:**
```csharp
using RawPrint;

foreach (var filePath in pdfFiles)
{
    byte[] bytes = File.ReadAllBytes(filePath);
    Printer.SendBytesToPrinter("Printer", bytes, bytes.Length);
}
```

**IronPDF:**
```csharp
using IronPdf;

// Option 1: Print each file
foreach (var filePath in pdfFiles)
{
    var pdf = PdfDocument.FromFile(filePath);
    pdf.Print("Printer");
}

// Option 2: Merge and print once
var pdfs = pdfFiles.Select(f => PdfDocument.FromFile(f)).ToList();
var merged = PdfDocument.Merge(pdfs);
merged.Print("Printer");
```

---

## Feature Comparison

| Feature | RawPrint | IronPDF |
|---------|----------|---------|
| **PDF Creation** | | |
| HTML to PDF | NO | Yes |
| URL to PDF | NO | Yes |
| Create from scratch | NO | Yes |
| **PDF Manipulation** | | |
| Merge PDFs | NO | Yes |
| Split PDFs | NO | Yes |
| Add Watermarks | NO | Yes |
| Edit Existing | NO | Yes |
| **Printing** | | |
| Print PDF | Yes (raw) | Yes (high-level) |
| Print Dialog | NO | Yes |
| Multiple Copies | Limited | Yes |
| DPI Control | NO | Yes |
| Duplex | NO | Yes |
| **Platform** | | |
| Windows | Yes | Yes |
| Linux | NO | Yes |
| macOS | NO | Yes |
| Docker | NO | Yes |
| **Other** | | |
| Security | NO | Yes |
| Digital Signatures | NO | Yes |
| PDF/A | NO | Yes |

The [comprehensive migration reference](https://ironpdf.com/blog/migration-guides/migrate-from-rawprint-to-ironpdf/) provides integration patterns for transitioning from low-level byte transmission to high-level PDF creation and printing workflows.

---

## Common Migration Scenarios

### Scenario 1: Print Reports

**Before:** Create PDF elsewhere, then use RawPrint
```csharp
// Step 1: Use some library to create PDF
byte[] pdf = CreatePdfSomehow(reportData);
// Step 2: RawPrint
Printer.SendBytesToPrinter("Printer", pdf, pdf.Length);
```

**After:** All-in-one with IronPDF
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(reportHtml);
pdf.Print("Printer");
```

### Scenario 2: Print Queue Processing

**Before:**
```csharp
while (queue.TryDequeue(out var job))
{
    var bytes = File.ReadAllBytes(job.PdfPath);
    Printer.SendBytesToPrinter(job.PrinterName, bytes, bytes.Length);
}
```

**After:**
```csharp
while (queue.TryDequeue(out var job))
{
    var pdf = PdfDocument.FromFile(job.PdfPath);
    pdf.Print(new PrintOptions
    {
        PrinterName = job.PrinterName,
        NumberOfCopies = job.Copies
    });
}
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Identify all RawPrint usage**
  ```bash
  grep -r "using RawPrint" --include="*.cs" .
  grep -r "Printer\|SendBytesToPrinter" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document printer names used**
  ```csharp
  // Find patterns like:
  string printerName = "MyPrinter";
  ```
  **Why:** Ensure all printers are correctly configured for IronPDF's high-level printing.

- [ ] **Note any external PDF creation code**
  ```csharp
  // Look for external PDF creation logic:
  var pdfBytes = ExternalLibrary.CreatePdf();
  ```
  **Why:** Identify areas where IronPDF can replace or enhance PDF creation.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Remove RawPrint package**
  ```bash
  dotnet remove package RawPrint
  ```
  **Why:** Remove dependency on low-level printing utility.

- [ ] **Install IronPdf package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF for advanced PDF creation and printing capabilities.

- [ ] **Replace raw printing with pdf.Print()**
  ```csharp
  // Before (RawPrint)
  Printer.SendBytesToPrinter(printerName, pdfBytes);

  // After (IronPDF)
  var pdf = PdfDocument.FromBinary(pdfBytes);
  pdf.Print(printerName);
  ```
  **Why:** Use IronPDF's high-level API for reliable and configurable printing.

- [ ] **Consolidate PDF creation and printing**
  ```csharp
  // Before (RawPrint with external PDF creation)
  var pdfBytes = ExternalLibrary.CreatePdf();
  Printer.SendBytesToPrinter(printerName, pdfBytes);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(htmlContent);
  pdf.Print(printerName);
  ```
  **Why:** Streamline PDF creation and printing with IronPDF's integrated tools.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Remove manual handle management**
  ```csharp
  // Before (RawPrint)
  Printer.OpenPrinter(printerName, out printerHandle);
  Printer.StartDocPrinter(printerHandle, ...);
  Printer.WritePrinter(printerHandle, ...);
  Printer.EndDocPrinter(printerHandle);
  Printer.ClosePrinter(printerHandle);

  // After (IronPDF)
  pdf.Print(printerName);
  ```
  **Why:** Simplify code by removing manual printer handle management with IronPDF's automatic handling.

### Testing

- [ ] **Test printing to target printers**
  **Why:** Verify that all printers are correctly configured and functioning with IronPDF.

- [ ] **Verify print quality**
  **Why:** Ensure that the print output meets quality expectations with IronPDF's rendering.

- [ ] **Test multiple copies**
  ```csharp
  // Example with IronPDF
  pdf.Print(printerName, new PrintOptions { Copies = 3 });
  ```
  **Why:** Confirm that multiple copies are handled correctly.

- [ ] **Test silent printing**
  ```csharp
  // Example with IronPDF
  pdf.Print(printerName, new PrintOptions { Silent = true });
  ```
  **Why:** Ensure that silent printing works as expected without user intervention.

- [ ] **Cross-platform if needed**
  **Why:** Verify that the application functions correctly on all required platforms with IronPDF's cross-platform support.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Printing Guide](https://ironpdf.com/how-to/csharp-print-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
