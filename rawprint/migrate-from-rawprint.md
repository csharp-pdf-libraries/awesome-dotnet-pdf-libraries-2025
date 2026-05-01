# How Do I Combine RawPrint with IronPDF in C#?

## Why This Is Not a Straight Migration

RawPrint and IronPDF solve different problems and the honest framing is "complement," not "replace." RawPrint (frogmorecs/RawPrint, NuGet `RawPrint` v0.5.0, last release September 2019, now unlisted/legacy on nuget.org) is a thin P/Invoke wrapper over `winspool.Drv` that ships a byte stream straight to the Windows print spooler with the RAW datatype. That is exactly what you want for ESC/POS receipt printers, ZPL/EPL Zebra label printers, and legacy PCL/PostScript jobs where you have already produced the bytes the printer firmware expects. RawPrint does not generate PDFs and never claimed to.

IronPDF generates and manipulates PDFs. If your application currently uses RawPrint to push an existing PDF to a printer, and the PDF was being produced by some other library, IronPDF can replace that other library and (on Windows) also replace the spooler call via `pdf.Print()`. If your RawPrint usage is genuinely raw — ESC/POS to a thermal receipt printer, ZPL to a Zebra — IronPDF is not a drop-in replacement, and you should keep RawPrint for that channel.

### What RawPrint Does vs. What IronPDF Does

| Task | RawPrint | IronPDF |
|------|----------|---------|
| Create PDF from HTML | No | Yes |
| Create PDF from URL | No | Yes |
| Edit/Modify PDFs | No | Yes |
| Merge/Split PDFs | No | Yes |
| Push raw bytes to a Windows spooler | Yes (its only job) | No |
| Send ESC/POS to a receipt printer | Yes | No |
| Send ZPL/EPL to a Zebra label printer | Yes | No |
| Print a finished PDF on Windows | Yes (RAW; printer must accept PDF natively) | Yes (renders, then prints via system spooler) |
| Cross-platform (Linux, macOS, Docker) | No (Windows spooler only) | Yes |

---

## RawPrint's Real API Surface

The package's public API is small. There is one class, `Printer`, implementing `IPrinter`, in the `RawPrint` namespace:

```csharp
namespace RawPrint
{
    public interface IPrinter
    {
        event EventHandler<JobEventArgs> OnJobCreated;
        void PrintRawFile(string printer, string path, bool paused);
        void PrintRawFile(string printer, string path, string documentName, bool paused);
        void PrintRawStream(string printer, Stream stream, string documentName, bool paused);
        void PrintRawStream(string printer, Stream stream, string documentName, bool paused, int pagecount);
    }
}
```

There is no public `SendBytesToPrinter`, `OpenPrinter`, `StartDocPrinter`, `WritePrinter`, or `EndDocPrinter` method on `Printer` — those are the underlying Win32 calls that RawPrint hides. Code samples that assume those are public are referring to hand-rolled P/Invoke helpers, not RawPrint itself.

---

## Quick Start: Replacing the "create PDF then push it" Pattern

If you currently produce a PDF with a different library and then hand it to RawPrint to print, you can collapse the whole pipeline into IronPDF on Windows.

### Step 1: Pick the right packages

```bash
# Keep RawPrint only if you need RAW byte channels (ESC/POS, ZPL, etc.)
# dotnet remove package RawPrint

# Add IronPDF
dotnet add package IronPdf
```

### Step 2: Initialize the license

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 3: Replace the create-then-RawPrint pattern

**Before (other library + RawPrint):**
```csharp
using RawPrint;
using System.IO;

byte[] pdfBytes = SomeOtherLibrary.CreatePdf(reportData);
File.WriteAllBytes("temp.pdf", pdfBytes);

IPrinter printer = new Printer();
printer.PrintRawFile("HP LaserJet", "temp.pdf", false);
```

**After (IronPDF only, on Windows):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(reportHtml);
pdf.Print(); // uses the Windows print system; pass PrintHtmlOptions for finer control
```

If you still need RAW byte channels for receipt or label printers, leave RawPrint where it is and add IronPDF alongside it for the PDF-rendering side of the application.

---

## API Mapping Reference

| RawPrint (real public API) | IronPDF | Notes |
|----------------------------|---------|-------|
| `new Printer()` / `IPrinter` | `new ChromePdfRenderer()` / `PdfDocument` | RawPrint pushes bytes; IronPDF generates PDFs |
| `printer.PrintRawFile(name, path, paused)` | `PdfDocument.FromFile(path).Print()` | IronPDF re-renders to the OS print system; not RAW |
| `printer.PrintRawStream(name, stream, doc, paused)` | `new PdfDocument(stream).Print()` | Same caveat |
| `printer.OnJobCreated` event | n/a | Use IronPDF print options instead |
| n/a | `ChromePdfRenderer.RenderHtmlAsPdf()` | Create PDFs |
| n/a | `PdfDocument.Merge()` | Merge PDFs |
| n/a | `pdf.ApplyWatermark()` | Add watermarks |

Note: IronPDF prints by handing the document to the operating system, not by streaming raw bytes to the spooler. If your printer requires the RAW datatype (ESC/POS, ZPL, certain PCL workflows), that is RawPrint's lane and IronPDF cannot stand in for it.

---

## Code Examples

### Example 1: Print an Existing PDF File

**RawPrint (real API):**
```csharp
using RawPrint;

IPrinter printer = new Printer();
// PrintRawFile is happiest with bytes the printer firmware can interpret directly.
// Most modern enterprise MFPs accept PDF as RAW; cheap home printers do not.
printer.PrintRawFile("Brother HL-L2340D", "document.pdf", false);
```

**IronPDF (Windows):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

pdf.Print();                  // default printer
pdf.Print(); // with PrintHtmlOptions or after setting renderer options as needed
```

### Example 2: Create and Print (RawPrint cannot do the create half)

**RawPrint:**
```csharp
// Not possible with RawPrint alone — you need a separate library to produce the PDF first.
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(@"
    <h1>Invoice #12345</h1>
    <p>Customer: John Doe</p>
    <p>Amount: $150.00</p>
");

pdf.SaveAs("invoice.pdf");
pdf.Print();
```

### Example 3: Send ESC/POS to a Thermal Receipt Printer (RawPrint's strength)

**RawPrint — the right tool:**
```csharp
using RawPrint;
using System.IO;
using System.Text;

// ESC/POS: ESC @ resets, then text, then a partial cut.
byte[] receipt = Encoding.ASCII.GetBytes("\x1B@Hello\nWorld\n\n\n\x1DV\x01");

IPrinter printer = new Printer();
using (var stream = new MemoryStream(receipt))
{
    printer.PrintRawStream("EPSON TM-T20II", stream, "Receipt", false);
}
```

**IronPDF — wrong tool for this job:** IronPDF renders documents to PDF and prints PDFs through the Windows print system. It does not (and should not) speak ESC/POS. Keep RawPrint for this channel.

### Example 4: Send ZPL to a Zebra Label Printer (RawPrint's strength)

**RawPrint:**
```csharp
using RawPrint;
using System.IO;
using System.Text;

string zpl = "^XA^FO50,50^ADN,36,20^FDHello Zebra^FS^XZ";
byte[] data = Encoding.ASCII.GetBytes(zpl);

IPrinter printer = new Printer();
using (var stream = new MemoryStream(data))
{
    printer.PrintRawStream("ZDesigner GK420d", stream, "Label", false);
}
```

IronPDF has nothing to offer here — ZPL is a printer-side language, not a document format.

### Example 5: Batch-Printing a Folder of PDFs

**RawPrint:**
```csharp
using RawPrint;

IPrinter printer = new Printer();
foreach (var filePath in pdfFiles)
{
    printer.PrintRawFile("Printer", filePath, false);
}
```

**IronPDF:**
```csharp
using IronPdf;

foreach (var filePath in pdfFiles)
{
    var pdf = PdfDocument.FromFile(filePath);
    pdf.Print();
}

// Or merge and print once:
var pdfs = pdfFiles.Select(f => PdfDocument.FromFile(f)).ToList();
var merged = PdfDocument.Merge(pdfs);
merged.Print();
```

---

## Feature Comparison

| Feature | RawPrint | IronPDF |
|---------|----------|---------|
| **PDF Creation** | | |
| HTML to PDF | No | Yes |
| URL to PDF | No | Yes |
| Create from scratch | No | Yes |
| **PDF Manipulation** | | |
| Merge PDFs | No | Yes |
| Split PDFs | No | Yes |
| Add Watermarks | No | Yes |
| Edit Existing | No | Yes |
| **Printing** | | |
| Push RAW bytes to spooler | Yes | No |
| ESC/POS, ZPL, raw PCL | Yes | No |
| Print via OS print system | No | Yes |
| **Platform** | | |
| Windows | Yes | Yes |
| Linux | No | Yes |
| macOS | No | Yes |
| Docker | No | Yes |
| **Other** | | |
| Encryption / Security | No | Yes |
| Digital Signatures | No | Yes |
| PDF/A | No | Yes |

The [comprehensive migration reference](https://ironpdf.com/blog/migration-guides/migrate-from-rawprint-to-ironpdf/) walks through the create-then-print pipelines that benefit most from collapsing into IronPDF on Windows.

---

## Common Scenarios

### Scenario 1: PDF Reports, Then Print

**Before:** Build PDF in another library, push with RawPrint
```csharp
byte[] pdf = OtherLibrary.CreatePdf(reportData);
File.WriteAllBytes("temp.pdf", pdf);
new Printer().PrintRawFile("Printer", "temp.pdf", false);
```

**After:** All-in-one with IronPDF
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(reportHtml);
pdf.Print();
```

### Scenario 2: Receipt or Label Printing

Keep RawPrint. IronPDF is not a substitute for ESC/POS or ZPL channels. If the same application also needs to produce PDF reports for archival or email, use IronPDF for that pipeline and RawPrint for the printer-language pipeline side by side.

---

## Migration Checklist

### Pre-Migration

- [ ] **Identify all RawPrint usage**
  ```bash
  grep -r "using RawPrint" --include="*.cs" .
  grep -r "PrintRawFile\|PrintRawStream" --include="*.cs" .
  ```
  **Why:** Real RawPrint usage calls `PrintRawFile` or `PrintRawStream`. Custom hand-rolled P/Invoke wrappers may also use the name "RawPrinterHelper" but are not the package.

- [ ] **Classify each call site**
  - PDF being printed on a normal office printer -> candidate for IronPDF
  - ESC/POS, ZPL, EPL, or other printer-language bytes -> keep RawPrint
  - PCL/PostScript hand-built bytes -> usually keep RawPrint

- [ ] **Note the printer names used**
  **Why:** IronPDF prints through the OS print system; printer names must still resolve.

- [ ] **Note any external PDF creation code**
  ```csharp
  var pdfBytes = ExternalLibrary.CreatePdf();
  ```
  **Why:** This is the half IronPDF replaces.

- [ ] **Obtain an IronPDF license key.** Free trial at https://ironpdf.com/

### Code Updates

- [ ] **Install IronPdf**
  ```bash
  dotnet add package IronPdf
  ```

- [ ] **Replace create-then-RawPrint pipelines**
  ```csharp
  // Before: external PDF creation + RawPrint
  byte[] pdfBytes = ExternalLibrary.CreatePdf();
  File.WriteAllBytes("tmp.pdf", pdfBytes);
  new Printer().PrintRawFile(printerName, "tmp.pdf", false);

  // After: IronPDF on Windows
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(htmlContent);
  pdf.Print();
  ```

- [ ] **Add license initialization**
  ```csharp
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```

- [ ] **Leave RawPrint in place for raw-byte channels** (ESC/POS, ZPL).

### Testing

- [ ] Test printing to each target printer.
- [ ] Verify print quality and page layout for IronPDF-rendered output.
- [ ] If you removed RawPrint, confirm no remaining call sites need RAW bytes.
- [ ] Test on Linux/macOS/Docker for any IronPDF code paths that move off Windows.

---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Printing Guide](https://ironpdf.com/how-to/csharp-print-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [RawPrint repository (frogmorecs)](https://github.com/frogmorecs/RawPrint)
- [RawPrint on nuget.org (unlisted/legacy)](https://www.nuget.org/packages/RawPrint)

---

*This guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
