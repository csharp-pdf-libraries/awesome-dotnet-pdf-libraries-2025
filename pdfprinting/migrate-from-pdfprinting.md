# How Do I Migrate from PDFPrinting.NET to IronPDF in C#?

## Why Migrate from PDFPrinting.NET?

PDFPrinting.NET (Terminalworks; NuGet package `PdfPrintingNet`, current 5.4.2 as of March 2026) is a specialized library focused on silent PDF printing within Windows environments. It also offers PDF viewing, basic editing, and PDF-to-image rasterization as separately licensed components. What it does not do is generate PDFs from HTML, URLs, or any other source — its API has no `HtmlToPdfConverter` or `WebPageToPdfConverter` class. When your application needs to evolve beyond printing existing PDFs into creating or rendering them, you need a different tool.

### The Printing-Centric Limitation

PDFPrinting.NET concentrates on a narrow set of operations:

1. **Print-centric**: Creates no new PDF content — it only prints, views, edits, and rasterizes pre-existing PDFs
2. **Windows-only printing**: Print path is tied to Windows printing infrastructure
3. **No HTML/URL-to-PDF**: There is no `HtmlToPdfConverter` and no `WebPageToPdfConverter` class — these features simply do not exist
4. **Limited manipulation**: Has basic merge/split/extract via `PdfPrintDocument` but no watermarking or modern content authoring
5. **No text extraction API surface comparable to a full PDF SDK**
6. **No form filling/flattening**
7. **No JavaScript or modern CSS rendering** — rasterization is for printing existing documents

### Why Choose [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/)?

IronPDF provides complete PDF lifecycle management plus printing:

- **Full PDF Generation**: Create PDFs from HTML, URLs, images, and more
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **PDF Manipulation**: Merge, split, rotate, and modify PDFs
- **Security Features**: Passwords, encryption, digital signatures
- **Text Extraction**: Read and search PDF content
- **Form Handling**: Fill and flatten PDF forms
- **Printing**: All of PDFPrinting.NET's capabilities plus more
- **Modern Rendering**: Chromium-based engine with full CSS3/JS support

---

## Quick Migration Overview

| Aspect | PDFPrinting.NET | IronPDF |
|--------|-----------------|---------|
| Primary Focus | Silent PDF printing | Full PDF lifecycle |
| PDF Creation | Not supported | Comprehensive html to pdf c# |
| HTML to PDF | Not supported | Full Chromium engine |
| PDF Manipulation | Not supported | Merge, split, rotate |
| Text Extraction | Not supported | Full support |
| Platform Support | Windows only | Cross-platform |
| Printer Integration | Windows Print API | Cross-platform printing |
| Silent Printing | Yes | Yes |
| Print Settings | Basic | Comprehensive |
| License | Commercial (Site $299+, Redistributable $699+, Full $2,249) | Commercial |

Explore performance benchmarks and pricing in the [comprehensive guide](https://ironsoftware.com/suite/blog/comparison/compare-pdfprinting-vs-ironpdf/).

---

## NuGet Package Changes

```bash
# Remove PDFPrinting.NET (the actual NuGet ID is PdfPrintingNet)
dotnet remove package PdfPrintingNet

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// Before: PDFPrinting.NET
// Newer API:
using PdfPrintingNet;
// Older code may use:
using TerminalWorks.PDFPrinting;

// After: IronPDF
using IronPdf;
using IronPdf.Printing;
```

---

## API Mapping Reference

### Core Classes

| PDFPrinting.NET | IronPDF | Notes |
|-----------------|---------|-------|
| `PdfPrint` (newer) / `PDFPrinter` (legacy) | `PdfDocument` + `Print()` | Core printing entry point |
| `PdfPrintDocument` | `PdfDocument` | Document representation |
| _(no HTML-to-PDF class exists)_ | `ChromePdfRenderer.RenderHtmlAsPdf` | IronPDF only |
| _(no URL-to-PDF class exists)_ | `ChromePdfRenderer.RenderUrlAsPdf` | IronPDF only |
| Print settings properties | `PrintSettings` | Print configuration |

### Printing Methods

| PDFPrinting.NET | IronPDF | Notes |
|-----------------|---------|-------|
| `pdfPrint.Print(filePath)` | `pdf.Print()` | Print to default printer |
| `pdfPrint.PrinterName = "..."; pdfPrint.Print(filePath)` | `pdf.Print(printerName)` | Specify printer |
| `pdfPrintDocument` instance | `pdf.GetPrintDocument()` | Get PrintDocument |
| Property: page scaling | `printSettings.PrinterResolution` | Scaling options |
| Property: copies | `printSettings.NumberOfCopies = n` | Copy count |
| Property: duplex | `printSettings.DuplexMode` | Duplex printing |
| Property: collate | `printSettings.Collate` | Collation |
| Property: color | `printSettings.GrayscaleOutput` | Color settings |
| Property: paper source | `printSettings.PaperTray` | Paper source |

### PDF Generation (NEW in IronPDF)

| Feature | IronPDF Method | Notes |
|---------|----------------|-------|
| HTML to PDF | `renderer.RenderHtmlAsPdf(html)` | Full HTML/CSS/JS |
| URL to PDF | `renderer.RenderUrlAsPdf(url)` | Web page capture |
| File to PDF | `renderer.RenderHtmlFileAsPdf(path)` | HTML file |
| Image to PDF | `ImageToPdfConverter.ImageToPdf(paths)` | Images |

### PDF Manipulation (NEW in IronPDF)

| Feature | IronPDF Method | Notes |
|---------|----------------|-------|
| Load PDF | `PdfDocument.FromFile(path)` | Load existing |
| Merge PDFs | `PdfDocument.Merge(pdfs)` | Combine |
| Split PDF | `pdf.CopyPages(start, end)` | Extract pages |
| Add Watermark | `pdf.ApplyWatermark(html)` | Overlay |
| Add Password | `pdf.SecuritySettings.UserPassword` | Security |
| Extract Text | `pdf.ExtractAllText()` | Read content |

---

## Code Migration Examples

### Example 1: Basic Silent Printing

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

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        string filePath = "document.pdf";

        var pdf = PdfDocument.FromFile(filePath);
        pdf.Print();

        Console.WriteLine("PDF printed successfully.");
    }
}
```

### Example 2: Print to Specific Printer

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

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");
        pdf.Print("HP LaserJet Pro");

        Console.WriteLine("PDF sent to HP LaserJet Pro.");
    }
}
```

### Example 3: Print with Custom Settings

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
        pdfPrint.PageScaling = PageScaling.FitToPrintableArea;
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

class Program
{
    static void Main()
    {
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
    }
}
```

### Example 4: Access PrintDocument for Advanced Control

**Before (PDFPrinting.NET):**
```csharp
using PdfPrintingNet;
using System.Drawing.Printing;
using System;

class Program
{
    static void Main()
    {
        // PdfPrintDocument inherits System.Drawing.Printing.PrintDocument
        var printDoc = new PdfPrintDocument("license-owner", "license-key", "document.pdf");

        printDoc.PrinterSettings.PrinterName = "Network Printer";
        printDoc.PrinterSettings.Copies = 2;
        printDoc.PrinterSettings.FromPage = 1;
        printDoc.PrinterSettings.ToPage = 5;

        printDoc.Print();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Drawing.Printing;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");
        var printDoc = pdf.GetPrintDocument();

        // Same System.Drawing.Printing.PrintDocument access
        printDoc.PrinterSettings.PrinterName = "Network Printer";
        printDoc.PrinterSettings.Copies = 2;
        printDoc.PrinterSettings.FromPage = 1;
        printDoc.PrinterSettings.ToPage = 5;

        printDoc.Print();
    }
}
```

### Example 5: Print Page Range

**Before (PDFPrinting.NET):**
```csharp
using PdfPrintingNet;
using System;

class Program
{
    static void Main()
    {
        var pdfPrint = new PdfPrint("license-owner", "license-key");
        pdfPrint.FromPage = 2;
        pdfPrint.ToPage = 5;

        pdfPrint.Print("document.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");
        var printDoc = pdf.GetPrintDocument();

        printDoc.PrinterSettings.FromPage = 2;
        printDoc.PrinterSettings.ToPage = 5;
        printDoc.PrinterSettings.PrintRange = System.Drawing.Printing.PrintRange.SomePages;

        printDoc.Print();
    }
}
```

### Example 6: Create PDF Then Print (NEW Capability)

**Before (PDFPrinting.NET):**
```csharp
// NOT POSSIBLE
// PDFPrinting.NET cannot create PDFs - only print existing ones
// You would need to use another library to create the PDF first
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Create PDF from HTML using c# html to pdf
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
            </html>
        ");

        // Print immediately
        pdf.Print("Invoice Printer");

        // Or save and print later
        pdf.SaveAs("invoice.pdf");

        Console.WriteLine("PDF created and printed.");
    }
}
```

### Example 7: Print from URL (NEW Capability)

**Before (PDFPrinting.NET):**
```csharp
// NOT POSSIBLE
// PDFPrinting.NET cannot convert URLs to PDF
// Would need a separate library to capture and convert first
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Configure rendering
        renderer.RenderingOptions.RenderDelay = 2000;  // Wait for JS
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

        // Capture web page as PDF
        var pdf = renderer.RenderUrlAsPdf("https://example.com/report");

        // Print directly
        pdf.Print();

        Console.WriteLine("Web page captured and printed.");
    }
}
```

### Example 8: Batch Printing Multiple PDFs

**Before (PDFPrinting.NET):**
```csharp
using PdfPrintingNet;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var pdfPrint = new PdfPrint("license-owner", "license-key");
        pdfPrint.PrinterName = "Office Printer";

        var files = new List<string>
        {
            "document1.pdf",
            "document2.pdf",
            "document3.pdf"
        };

        foreach (var file in files)
        {
            pdfPrint.Print(file);
            Console.WriteLine($"Printed: {file}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var files = new List<string>
        {
            "document1.pdf",
            "document2.pdf",
            "document3.pdf"
        };

        foreach (var file in files)
        {
            var pdf = PdfDocument.FromFile(file);
            pdf.Print("Office Printer");
            Console.WriteLine($"Printed: {file}");
        }

        // Or merge then print as one job
        var merged = PdfDocument.Merge(files);
        merged.Print("Office Printer");
        Console.WriteLine("All documents printed as single job.");
    }
}
```

### Example 9: Merge and Print (NEW Capability)

**Before (PDFPrinting.NET):**
```csharp
// NOT POSSIBLE
// PDFPrinting.NET cannot merge PDFs
// Would need separate library like iTextSharp or PdfSharp
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Load multiple PDFs
        var cover = PdfDocument.FromFile("cover.pdf");
        var content = PdfDocument.FromFile("content.pdf");
        var appendix = PdfDocument.FromFile("appendix.pdf");

        // Merge into one document
        var combined = PdfDocument.Merge(cover, content, appendix);

        // Print the combined document
        combined.Print("Office Printer");

        // Or save merged version
        combined.SaveAs("complete-report.pdf");

        Console.WriteLine("Documents merged and printed.");
    }
}
```

### Example 10: Print with Watermark (NEW Capability)

**Before (PDFPrinting.NET):**
```csharp
// NOT POSSIBLE
// PDFPrinting.NET cannot modify PDFs before printing
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("draft-report.pdf");

        // Add watermark before printing
        pdf.ApplyWatermark(
            "<h2 style='color:red; opacity:0.3; font-size:72px;'>DRAFT</h2>",
            rotation: 45,
            IronPdf.Editing.VerticalAlignment.Middle,
            IronPdf.Editing.HorizontalAlignment.Center
        );

        // Print with watermark
        pdf.Print();

        Console.WriteLine("Draft watermark added and printed.");
    }
}
```

---

## Print Settings Migration

### Common Print Settings

```csharp
// PDFPrinting.NET settings → IronPDF equivalents

// Copies
// Before: pdfPrint.Copies = 3;
// After:
var settings = new PrintSettings { NumberOfCopies = 3 };

// Duplex
// Before: pdfPrint.Duplex = true;
// After:
settings.DuplexMode = System.Drawing.Printing.Duplex.Vertical;

// Collate
// Before: pdfPrint.Collate = true;
// After:
settings.Collate = true;

// Grayscale
// Before: pdfPrint.PrintInColor = false;
// After:
settings.GrayscaleOutput = true;

// DPI/Resolution
// Before: pdfPrint.PrintQuality = PrintQuality.High;
// After:
settings.Dpi = 300;

// Printer name
// Before: pdfPrint.PrinterName = "MyPrinter";
// After:
settings.PrinterName = "MyPrinter";

// Use the settings
pdf.Print(settings);
```

### Paper Handling

```csharp
// PDFPrinting.NET → IronPDF paper settings

var printDoc = pdf.GetPrintDocument();

// Paper size
printDoc.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("A4", 827, 1169);

// Paper source/tray
printDoc.DefaultPageSettings.PaperSource = printDoc.PrinterSettings.PaperSources[0];

// Orientation
printDoc.DefaultPageSettings.Landscape = true;

// Margins
printDoc.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(50, 50, 50, 50);

printDoc.Print();
```

---

## Features Not Available in PDFPrinting.NET

IronPDF provides many capabilities beyond printing:

### PDF Generation

```csharp
// Create from HTML
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");

// Create from URL
var webPdf = renderer.RenderUrlAsPdf("https://example.com");

// Create from HTML file
var filePdf = renderer.RenderHtmlFileAsPdf("template.html");
```

### Headers and Footers

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Company Report</div>",
    MaxHeight = 25
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>",
    MaxHeight = 20
};
```

### PDF Manipulation

```csharp
// Merge
var merged = PdfDocument.Merge(pdf1, pdf2);

// Split
var chapter = pdf.CopyPages(0, 9);

// Append
pdf.AppendPdf(otherPdf);

// Remove page
pdf.RemovePage(5);

// Rotate
pdf.RotatePage(0, PdfPageRotation.Rotate90);
```

### Text Extraction

```csharp
// Extract all text
string text = pdf.ExtractAllText();

// Page-specific
string pageText = pdf.ExtractTextFromPage(0);
```

### Security

```csharp
// Password protection
pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.OwnerPassword = "owner456";
pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
```

### Digital Signatures

```csharp
var signature = new PdfSignature("certificate.pfx", "password")
{
    SigningContact = "support@company.com",
    SigningReason = "Document Approval"
};
pdf.Sign(signature);
```

### Form Filling

```csharp
pdf.Form.GetFieldByName("CustomerName").Value = "John Doe";
pdf.Form.GetFieldByName("Date").Value = DateTime.Now.ToShortDateString();
pdf.Form.Flatten();
```

Cross-platform printing patterns and silent printing implementations are explored further in the [detailed reference](https://ironpdf.com/blog/migration-guides/migrate-from-pdfprinting-to-ironpdf/).

---

## Cross-Platform Printing

PDFPrinting.NET is Windows-only. IronPDF works cross-platform:

### Windows
```csharp
// Works same as before
pdf.Print("HP LaserJet");
```

### Linux
```csharp
// Requires CUPS (Common Unix Printing System)
// Install: apt-get install cups

// Print using CUPS printer name
pdf.Print("HP_LaserJet");  // Note: CUPS uses underscores instead of spaces
```

### macOS
```csharp
// Uses macOS printing system
pdf.Print("HP LaserJet");
```

### List Available Printers (All Platforms)

```csharp
using System.Drawing.Printing;

foreach (string printer in PrinterSettings.InstalledPrinters)
{
    Console.WriteLine(printer);
}
```

---

## Common Migration Issues

### 1. License Configuration

```csharp
// PDFPrinting.NET: License typically via config file or at instantiation

// IronPDF: Set at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Set once in Program.cs or Startup.cs
```

### 2. File Loading Pattern

```csharp
// PDFPrinting.NET: Pass path directly to Print()
printer.Print("document.pdf");

// IronPDF: Load first, then operate
var pdf = PdfDocument.FromFile("document.pdf");
pdf.Print();
```

### 3. Printer Name Exact Match

```csharp
// Both libraries require exact printer name match
// Query available printers to ensure correct name:
foreach (string printer in PrinterSettings.InstalledPrinters)
{
    Console.WriteLine($"'{printer}'");  // Note exact name including spaces
}
```

### 4. Resource Disposal

```csharp
// PDFPrinting.NET: Usually stateless

// IronPDF: PdfDocument holds resources
using (var pdf = PdfDocument.FromFile("document.pdf"))
{
    pdf.Print();
}  // Properly disposed
```

### 5. Async Operations

```csharp
// PDFPrinting.NET: Sync only

// IronPDF: Async support available
var pdf = await PdfDocument.FromFileAsync("document.pdf");
await pdf.SaveAsAsync("output.pdf");
```

---

## Server Deployment

### PDFPrinting.NET
- Windows only
- Requires Windows Print Spooler service
- Needs access to printer drivers

### IronPDF
- Cross-platform (Windows, Linux, macOS)
- First run downloads Chromium (~150MB one-time)
- Linux requires additional dependencies:

```bash
# Ubuntu/Debian
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

# For printing on Linux
apt-get install -y cups
```

---

## Pre-Migration Checklist

- [ ] Inventory all PDFPrinting.NET usage in your codebase
- [ ] Document all printer names currently used
- [ ] Note all print settings configurations
- [ ] Identify if cross-platform support is needed
- [ ] Plan IronPDF license key storage
- [ ] Test with IronPDF trial license first

## Post-Migration Checklist

- [ ] Remove PdfPrintingNet NuGet package
- [ ] Update all namespace imports (`PdfPrintingNet` / legacy `TerminalWorks.PDFPrinting` → `IronPdf`)
- [ ] Set IronPDF license key at application startup
- [ ] Convert `PdfPrint.Print(path)` calls to `PdfDocument.FromFile(path).Print()` pattern
- [ ] Update print settings to IronPDF equivalents
- [ ] Verify printer names work correctly
- [ ] Test printing on all target platforms
- [ ] Consider adding PDF generation for dynamic documents
- [ ] Install Linux dependencies if deploying to Linux

---

## Find All PDFPrinting.NET References

```bash
# Find PDFPrinting.NET usage (newer + legacy namespaces)
grep -rE "PdfPrintingNet|TerminalWorks\.PDFPrinting|PdfPrint\b|PdfPrintDocument|PDFPrinter" --include="*.cs" .

# Find printer-related code
grep -r "\.Print(\|PrinterName" --include="*.cs" .

# Find package references
grep -r "PdfPrintingNet" --include="*.csproj" .
```

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PDFPrinting.NET usages in codebase**
  ```bash
  grep -rE "using PdfPrintingNet|using TerminalWorks\.PDFPrinting" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package PdfPrintingNet
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

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **Printing Guide**: https://ironpdf.com/how-to/pdf-printing-print/
- **Getting Started**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Tutorials**: https://ironpdf.com/tutorials/
- **Support**: https://ironpdf.com/support/
