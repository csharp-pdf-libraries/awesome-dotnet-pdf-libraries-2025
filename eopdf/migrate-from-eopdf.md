# How Do I Migrate from EO.Pdf to IronPDF in C#?

## Table of Contents

1. [Why Migrate from EO.Pdf to IronPDF](#why-migrate-from-eopdf-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Examples](#code-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from EO.Pdf to IronPDF

### The EO.Pdf Problems

1. **Massive 126MB Package Size**: EO.Pdf bundles its own Chromium engine, resulting in a 126MB deployment footprint. This inflates Docker images, slows CI/CD pipelines, and increases infrastructure costs.

2. **Legacy Architecture Baggage**: EO.Pdf was originally built on Internet Explorer's rendering engine before migrating to Chromium. This legacy introduces:
   - Compatibility issues from the IE era
   - Technical debt in the API design
   - Inconsistent behavior between versions

3. **Windows-Centric Design**: Despite marketing as "cross-platform," EO.Pdf's Linux and macOS support is limited. Many developers report issues with non-Windows deployments.

4. **$799 Per License**: At $799 per developer license, EO.Pdf is expensive compared to alternatives offering similar or better functionality.

5. **Static Global Options**: EO.Pdf uses static `HtmlToPdf.Options` for configuration, which is not thread-safe and problematic in multi-tenant web applications.

### EO.Pdf vs IronPDF Comparison

| Aspect | EO.Pdf | IronPDF |
|--------|--------|---------|
| **Package Size** | 126MB | Optimized (~50MB) |
| **Legacy Issues** | IE migration baggage | Clean, modern codebase |
| **Platform Support** | Windows-focused | True cross-platform |
| **Configuration** | Static/global | Instance-based, thread-safe |
| **Price** | $799/developer | Competitive pricing |
| **API Design** | Mixed (HtmlToPdf + ACM) | Unified, consistent |
| **Documentation** | Limited | Comprehensive tutorials |
| **Modern .NET** | .NET Standard | .NET 6/7/8/9+ native |
| **Async Support** | Limited | Full async/await |

### Key Migration Benefits

For comprehensive coverage, consult the [comprehensive documentation](https://ironpdf.com/blog/migration-guides/migrate-from-eo-pdf-to-ironpdf/).

1. **50% Smaller Footprint**: IronPDF's optimized Chromium packaging
2. **True Cross-Platform**: Works identically on Windows, Linux, macOS, Docker
3. **Thread-Safe Configuration**: Instance-based renderer options
4. **Modern API**: Consistent, intuitive method names
5. **Better Documentation**: Extensive tutorials and examples
6. **Active Development**: Regular updates and security patches

---

## Before You Start

### 1. Inventory Your EO.Pdf Usage

Identify all EO.Pdf patterns in your codebase:

```bash
# Find all EO.Pdf references
grep -r "EO.Pdf\|HtmlToPdf\|AcmRender\|PdfDocument" --include="*.cs" .

# Check NuGet packages
dotnet list package | grep -i "EO.Pdf"
```

**Common EO.Pdf namespaces:**
- `EO.Pdf` - Core HTML to PDF
- `EO.Pdf.Acm` - Advanced Content Model (ACM)
- `EO.Pdf.Contents` - Low-level content manipulation
- `EO.Pdf.Drawing` - Graphics operations

### 2. Document Current Functionality

Create a checklist of EO.Pdf features you use:

- [ ] HTML to PDF conversion (`HtmlToPdf.ConvertHtml`)
- [ ] URL to PDF conversion (`HtmlToPdf.ConvertUrl`)
- [ ] ACM rendering (`AcmRender`, `AcmText`, `AcmBlock`)
- [ ] PDF merging (`PdfDocument.Merge`)
- [ ] Page manipulation
- [ ] Headers and footers
- [ ] Security/encryption
- [ ] Form fields
- [ ] Watermarks
- [ ] Printing (`PdfDocument.Print`)

### 3. Set Up IronPDF

```bash
# Remove EO.Pdf
dotnet remove package EO.Pdf

# Install IronPDF
dotnet add package IronPdf
```

### 4. Configure License

```csharp
// At application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## Quick Start Migration

### The Core Pattern Change

EO.Pdf uses static methods with global options. IronPDF uses instance-based renderers with local options.

**EO.Pdf pattern (static, not thread-safe):**
```csharp
// Global options affect ALL conversions
HtmlToPdf.Options.PageSize = PdfPageSizes.A4;
HtmlToPdf.Options.OutputArea = new RectangleF(0.5f, 0.5f, 7.5f, 10.5f);
HtmlToPdf.ConvertHtml(html, "output.pdf");
```

**IronPDF pattern (instance-based, thread-safe):**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 12.7;  // mm
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Minimal Migration Example

**Before (EO.Pdf):**
```csharp
using EO.Pdf;

public class PdfService
{
    public void GenerateReport(string html)
    {
        HtmlToPdf.Options.PageSize = PdfPageSizes.A4;
        HtmlToPdf.Options.OutputArea = new RectangleF(0.5f, 0.5f, 7.5f, 10.5f);
        HtmlToPdf.ConvertHtml(html, "report.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    public void GenerateReport(string html)
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 12.7;
        renderer.RenderingOptions.MarginBottom = 12.7;
        renderer.RenderingOptions.MarginLeft = 12.7;
        renderer.RenderingOptions.MarginRight = 12.7;

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report.pdf");
    }
}
```

---

## Complete API Reference

### Namespace Mapping

| EO.Pdf Namespace | IronPDF Equivalent |
|-----------------|-------------------|
| `EO.Pdf` | `IronPdf` |
| `EO.Pdf.Acm` | HTML/CSS (no ACM needed) |
| `EO.Pdf.Contents` | `IronPdf.Editing` |
| `EO.Pdf.Drawing` | HTML/CSS or `IronPdf.Editing` |

### Core Class Mapping

| EO.Pdf Class | IronPDF Equivalent | Notes |
|-------------|-------------------|-------|
| `HtmlToPdf` | `ChromePdfRenderer` | Instance-based |
| `PdfDocument` | `PdfDocument` | Similar but different methods |
| `PdfPage` | `PdfDocument.Pages[i]` | Via page collection |
| `HtmlToPdfOptions` | `ChromePdfRenderOptions` | Via `RenderingOptions` |
| `AcmRender` | Not needed | Use HTML/CSS instead |
| `AcmText` | HTML `<span>`, `<p>` | |
| `AcmBlock` | HTML `<div>` | |
| `AcmImage` | HTML `<img>` | |
| `AcmTable` | HTML `<table>` | |

### Method Mapping

| EO.Pdf Method | IronPDF Method | Notes |
|--------------|----------------|-------|
| `HtmlToPdf.ConvertHtml(html, path)` | `renderer.RenderHtmlAsPdf(html)` then `SaveAs(path)` | Two-step in IronPDF |
| `HtmlToPdf.ConvertHtml(html, stream)` | `pdf.Stream` or `pdf.BinaryData` | Access data after render |
| `HtmlToPdf.ConvertHtml(html, pdfDoc)` | `renderer.RenderHtmlAsPdf(html)` | Returns PdfDocument |
| `HtmlToPdf.ConvertUrl(url, path)` | `renderer.RenderUrlAsPdf(url)` then `SaveAs(path)` | |
| `HtmlToPdf.ConvertUrl(url, stream)` | `pdf.BinaryData` or `pdf.Stream` | |
| `PdfDocument.Save(path)` | `pdf.SaveAs(path)` | |
| `PdfDocument.Save(stream)` | `pdf.Stream` | |
| `PdfDocument.Merge(docs)` | `PdfDocument.Merge(docs)` | Static method |
| `PdfDocument.Append(doc)` | `pdf.AppendPdf(other)` or `PdfDocument.Merge()` | |
| `PdfDocument.Print()` | `pdf.Print()` | Similar functionality |
| `new PdfDocument(path)` | `PdfDocument.FromFile(path)` | Static factory |
| `new PdfDocument(stream)` | `PdfDocument.FromStream(stream)` | |

### Options Mapping

| EO.Pdf Option | IronPDF RenderingOptions | Notes |
|--------------|-------------------------|-------|
| `Options.PageSize = PdfPageSizes.A4` | `PaperSize = PdfPaperSize.A4` | |
| `Options.PageSize = PdfPageSizes.Letter` | `PaperSize = PdfPaperSize.Letter` | |
| `Options.OutputArea` (RectangleF) | `MarginTop`, `MarginBottom`, etc. | Individual properties |
| `Options.BaseUrl` | `BaseUrl` | Same concept |
| `Options.AutoFitX` | `FitToPaperMode` | |
| `Options.NoCacheImages` | Not needed | Chrome handles caching |
| `Options.NoLink` | CSS can disable links | |
| `Options.RepeatTableHeader` | Automatic in HTML tables | |
| `Options.RepeatTableFooter` | Automatic in HTML tables | |

### Page Size Constants

| EO.Pdf | IronPDF |
|--------|---------|
| `PdfPageSizes.A0` - `A10` | `PdfPaperSize.A0` - `A10` |
| `PdfPageSizes.Letter` | `PdfPaperSize.Letter` |
| `PdfPageSizes.Legal` | `PdfPaperSize.Legal` |
| `PdfPageSizes.Tabloid` | `PdfPaperSize.Tabloid` |
| `PdfPageSizes.B0` - `B10` | `PdfPaperSize.B0` - `B10` |
| Custom via `new PdfPageSize(w, h)` | `SetCustomPaperSizeInMillimeters(w, h)` |

### Security Mapping

| EO.Pdf Security | IronPDF Equivalent |
|----------------|-------------------|
| `PdfDocumentSecurity.UserPassword` | `pdf.SecuritySettings.UserPassword` |
| `PdfDocumentSecurity.OwnerPassword` | `pdf.SecuritySettings.OwnerPassword` |
| `PdfDocumentSecurity.AllowPrint` | `pdf.SecuritySettings.AllowUserPrinting` |
| `PdfDocumentSecurity.AllowCopy` | `pdf.SecuritySettings.AllowUserCopyPasteContent` |
| `PdfDocumentSecurity.AllowModify` | `pdf.SecuritySettings.AllowUserEdits` |

---

## Code Examples

### Example 1: Basic HTML to PDF

**Before (EO.Pdf):**
```csharp
using EO.Pdf;

string html = "<html><body><h1>Hello World</h1><p>Generated with EO.Pdf</p></body></html>";
HtmlToPdf.ConvertHtml(html, "output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

string html = "<html><body><h1>Hello World</h1><p>Generated with IronPDF</p></body></html>";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Options

**Before (EO.Pdf):**
```csharp
using EO.Pdf;
using System.Drawing;

HtmlToPdf.Options.PageSize = PdfPageSizes.A4;
HtmlToPdf.Options.OutputArea = new RectangleF(0.5f, 0.5f, 7.5f, 10.5f);  // inches
HtmlToPdf.Options.BaseUrl = "https://example.com";
HtmlToPdf.ConvertUrl("https://example.com/report", "report.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 12.7;    // 0.5 inch = 12.7mm
renderer.RenderingOptions.MarginBottom = 12.7;
renderer.RenderingOptions.MarginLeft = 12.7;
renderer.RenderingOptions.MarginRight = 12.7;
renderer.RenderingOptions.BaseUrl = new Uri("https://example.com");

var pdf = renderer.RenderUrlAsPdf("https://example.com/report");
pdf.SaveAs("report.pdf");
```

### Example 3: Merging PDFs

**Before (EO.Pdf):**
```csharp
using EO.Pdf;

PdfDocument doc1 = new PdfDocument("file1.pdf");
PdfDocument doc2 = new PdfDocument("file2.pdf");
PdfDocument doc3 = new PdfDocument("file3.pdf");

doc1.Append(doc2);
doc1.Append(doc3);
doc1.Save("merged.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var doc1 = PdfDocument.FromFile("file1.pdf");
var doc2 = PdfDocument.FromFile("file2.pdf");
var doc3 = PdfDocument.FromFile("file3.pdf");

var merged = PdfDocument.Merge(doc1, doc2, doc3);
merged.SaveAs("merged.pdf");
```

### Example 4: ACM Content to HTML

EO.Pdf's ACM (Advanced Content Model) requires manual positioning. IronPDF uses HTML/CSS.

**Before (EO.Pdf ACM):**
```csharp
using EO.Pdf;
using EO.Pdf.Acm;

PdfDocument doc = new PdfDocument();
PdfPage page = doc.Pages.Add();
AcmRender render = new AcmRender(page);

AcmBlock container = new AcmBlock();

AcmText header = new AcmText("Company Report");
header.Style.FontSize = 24;
header.Style.FontBold = true;
container.Children.Add(header);

AcmText paragraph = new AcmText("This is the report content...");
paragraph.Style.FontSize = 12;
container.Children.Add(paragraph);

AcmImage logo = new AcmImage("logo.png");
logo.Style.Width = new AcmUnit(100, AcmUnitType.Point);
container.Children.Add(logo);

render.Render(container);
doc.Save("report.pdf");
```

**After (IronPDF - just use HTML):**
```csharp
using IronPdf;

var html = @"
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; padding: 20px; }
        h1 { font-size: 24pt; font-weight: bold; }
        p { font-size: 12pt; }
        img { width: 100px; }
    </style>
</head>
<body>
    <h1>Company Report</h1>
    <p>This is the report content...</p>
    <img src='logo.png' />
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 5: Headers and Footers

**Before (EO.Pdf via AfterRenderPage event):**
```csharp
using EO.Pdf;
using EO.Pdf.Acm;

HtmlToPdf.Options.AfterRenderPage += (sender, e) =>
{
    // Header
    AcmText header = new AcmText("Company Name - Confidential");
    header.Style.FontSize = 10;
    AcmRender headerRender = new AcmRender(e.Page, 0, 0, e.Page.Size.Width, 30);
    headerRender.Render(header);

    // Footer with page number
    AcmText footer = new AcmText($"Page {e.PageIndex + 1}");
    footer.Style.FontSize = 10;
    AcmRender footerRender = new AcmRender(e.Page, 0, e.Page.Size.Height - 30, e.Page.Size.Width, 30);
    footerRender.Render(footer);
};

HtmlToPdf.ConvertHtml(html, "document.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:center; font-size:10pt;'>Company Name - Confidential</div>",
    DrawDividerLine = true
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:center; font-size:10pt;'>Page {page} of {total-pages}</div>",
    DrawDividerLine = true
};

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("document.pdf");
```

### Example 6: Security and Encryption

**Before (EO.Pdf):**
```csharp
using EO.Pdf;

PdfDocument doc = new PdfDocument();
// ... add content ...

doc.Security.UserPassword = "userpass";
doc.Security.OwnerPassword = "ownerpass";
doc.Security.AllowPrint = true;
doc.Security.AllowCopy = false;
doc.Security.AllowModify = false;

doc.Save("secure.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);

pdf.SecuritySettings.UserPassword = "userpass";
pdf.SecuritySettings.OwnerPassword = "ownerpass";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

pdf.SaveAs("secure.pdf");
```

### Example 7: Converting to Stream/Bytes

**Before (EO.Pdf):**
```csharp
using EO.Pdf;
using System.IO;

// To MemoryStream
using (MemoryStream stream = new MemoryStream())
{
    HtmlToPdf.ConvertHtml(html, stream);
    byte[] pdfBytes = stream.ToArray();
    // Use bytes...
}

// Or to PdfDocument then stream
PdfDocument doc = new PdfDocument();
HtmlToPdf.ConvertHtml(html, doc);
using (MemoryStream stream = new MemoryStream())
{
    doc.Save(stream);
    // Use stream...
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);

// Get bytes directly
byte[] pdfBytes = pdf.BinaryData;

// Or get stream
using Stream stream = pdf.Stream;
```

### Example 8: Watermarks

**Before (EO.Pdf via ACM):**
```csharp
using EO.Pdf;
using EO.Pdf.Acm;

PdfDocument doc = new PdfDocument("original.pdf");
foreach (PdfPage page in doc.Pages)
{
    AcmText watermark = new AcmText("CONFIDENTIAL");
    watermark.Style.FontSize = 72;
    watermark.Style.ForegroundColor = Color.FromArgb(100, 200, 200, 200);
    watermark.Style.HorizontalAlign = AcmHorizontalAlign.Center;

    AcmRender render = new AcmRender(page, 0, page.Size.Height / 2 - 50,
        page.Size.Width, 100);
    render.Render(watermark);
}
doc.Save("watermarked.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("original.pdf");

pdf.ApplyWatermark(
    "<h1 style='color:rgba(200,200,200,0.5); font-size:72pt;'>CONFIDENTIAL</h1>",
    rotation: 45,
    opacity: 50
);

pdf.SaveAs("watermarked.pdf");
```

### Example 9: Custom Paper Size

**Before (EO.Pdf):**
```csharp
using EO.Pdf;
using System.Drawing;

// Custom size in points (1 inch = 72 points)
PdfPageSize customSize = new PdfPageSize(432, 288);  // 6"x4" (432x288 points)
HtmlToPdf.Options.PageSize = customSize;
HtmlToPdf.ConvertHtml(html, "custom.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
// Custom size in millimeters (6"x4" = 152.4mm x 101.6mm)
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(152.4, 101.6);

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("custom.pdf");
```

### Example 10: Printing PDFs

**Before (EO.Pdf):**
```csharp
using EO.Pdf;
using System.Drawing.Printing;

PdfDocument doc = new PdfDocument("document.pdf");

PrinterSettings settings = new PrinterSettings();
settings.PrinterName = "HP LaserJet";
settings.Copies = 2;

doc.Print(settings);
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Print with default printer
pdf.Print();

// Or specify printer
pdf.Print("HP LaserJet");

// Or with custom settings
var printerSettings = new System.Drawing.Printing.PrinterSettings()
{
    PrinterName = "HP LaserJet",
    Copies = 2
};
pdf.Print(printerSettings);
```

---

## Advanced Scenarios

### Migrating ACM-Heavy Applications

If your application heavily uses EO.Pdf's ACM (Advanced Content Model), migrate to HTML/CSS:

**ACM Pattern → HTML Pattern:**

| ACM Element | HTML/CSS Equivalent |
|-------------|-------------------|
| `AcmBlock` | `<div>` |
| `AcmText` | `<span>`, `<p>`, `<h1>-<h6>` |
| `AcmImage` | `<img>` |
| `AcmTable` | `<table>` |
| `AcmTableRow` | `<tr>` |
| `AcmTableCell` | `<td>`, `<th>` |
| `AcmBreak` | `<br>` or CSS `page-break-after` |
| `AcmStyle.FontSize` | CSS `font-size` |
| `AcmStyle.FontBold` | CSS `font-weight: bold` |
| `AcmStyle.ForegroundColor` | CSS `color` |
| `AcmStyle.BackgroundColor` | CSS `background-color` |
| `AcmUnit(x, AcmUnitType.Point)` | CSS `xpt` |
| `AcmUnit(x, AcmUnitType.Inch)` | CSS `xin` |
| `AcmUnit(x, AcmUnitType.Percent)` | CSS `x%` |

**Example Migration:**

```csharp
// EO.Pdf ACM
AcmBlock block = new AcmBlock();
block.Style.Width = new AcmUnit(100, AcmUnitType.Percent);
block.Style.BackgroundColor = Color.LightGray;
block.Style.Padding = new AcmPadding(10, AcmUnitType.Point);

AcmText title = new AcmText("Report Title");
title.Style.FontSize = 24;
title.Style.FontBold = true;
title.Style.HorizontalAlign = AcmHorizontalAlign.Center;
block.Children.Add(title);
```

```csharp
// IronPDF HTML/CSS
var html = @"
<div style='width:100%; background-color:lightgray; padding:10pt;'>
    <h1 style='font-size:24pt; font-weight:bold; text-align:center;'>
        Report Title
    </h1>
</div>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Thread-Safe Service Pattern

EO.Pdf's static options are problematic. Here's the IronPDF pattern:

**Before (EO.Pdf - not thread-safe):**
```csharp
public class PdfService
{
    public byte[] GenerateReport()
    {
        // DANGER: These options affect all threads!
        HtmlToPdf.Options.PageSize = PdfPageSizes.A4;
        HtmlToPdf.Options.BaseUrl = "https://mysite.com";

        using var stream = new MemoryStream();
        HtmlToPdf.ConvertHtml(html, stream);
        return stream.ToArray();
    }
}
```

**After (IronPDF - thread-safe):**
```csharp
public class PdfService
{
    public byte[] GenerateReport()
    {
        // Each call gets its own renderer with isolated options
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.BaseUrl = new Uri("https://mysite.com");

        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Async Operations

EO.Pdf has limited async support. IronPDF provides full async:

```csharp
public async Task<byte[]> GenerateReportAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}

public async Task<byte[]> CaptureWebPageAsync(string url)
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderUrlAsPdfAsync(url);
    return pdf.BinaryData;
}
```

---

## Performance Considerations

### Memory Management

**EO.Pdf pattern:**
```csharp
PdfDocument doc = new PdfDocument("file.pdf");
// ... work with doc ...
doc.Dispose();  // Manual dispose
```

**IronPDF pattern:**
```csharp
using var pdf = PdfDocument.FromFile("file.pdf");
// ... work with pdf ...
// Automatic dispose via using
```

### Reusing Renderers

IronPDF renderers can be reused for better performance:

```csharp
public class PdfGeneratorService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfGeneratorService()
    {
        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    }

    public byte[] Generate(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Batch Processing

```csharp
var renderer = new ChromePdfRenderer();

// Process multiple documents efficiently
Parallel.ForEach(documents, doc =>
{
    var pdf = renderer.RenderHtmlAsPdf(doc.Html);
    pdf.SaveAs($"output_{doc.Id}.pdf");
});
```

---

## Troubleshooting

### Issue 1: Static Options Still Being Used

**Problem:** Code still references `HtmlToPdf.Options`.

**Solution:** Replace all static option references with instance options:

```csharp
// Find and replace pattern:
// HtmlToPdf.Options.X = value;
// becomes:
// renderer.RenderingOptions.X = value;
```

### Issue 2: Margin Units Different

**Problem:** EO.Pdf uses inches in `OutputArea`, IronPDF uses millimeters.

**Solution:** Convert: `inches × 25.4 = millimeters`

```csharp
// EO.Pdf: 0.5 inch margins
HtmlToPdf.Options.OutputArea = new RectangleF(0.5f, 0.5f, 7.5f, 10.5f);

// IronPDF: Convert to mm
renderer.RenderingOptions.MarginTop = 12.7;    // 0.5 * 25.4
renderer.RenderingOptions.MarginBottom = 12.7;
renderer.RenderingOptions.MarginLeft = 12.7;
renderer.RenderingOptions.MarginRight = 12.7;
```

### Issue 3: ACM Code Doesn't Translate

**Problem:** Complex ACM layouts don't have direct IronPDF equivalents.

**Solution:** Rebuild using HTML/CSS. ACM is essentially a layout engine—HTML/CSS is a better one:

```csharp
// Instead of ACM coordinate positioning, use CSS flexbox/grid
var html = @"
<style>
    .container { display: flex; flex-direction: column; }
    .header { font-size: 24pt; text-align: center; }
    .content { flex: 1; padding: 20px; }
    .footer { text-align: center; font-size: 10pt; }
</style>
<div class='container'>
    <div class='header'>Title</div>
    <div class='content'>Content here...</div>
    <div class='footer'>Page footer</div>
</div>";
```

### Issue 4: ConvertHtml Returns Void vs Object

**Problem:** EO.Pdf's `ConvertHtml` saves directly; IronPDF returns a `PdfDocument`.

**Solution:** Two-step process in IronPDF:

```csharp
// EO.Pdf
HtmlToPdf.ConvertHtml(html, "output.pdf");

// IronPDF
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Issue 5: AfterRenderPage Event Missing

**Problem:** EO.Pdf's `AfterRenderPage` event for custom content on each page.

**Solution:** Use IronPDF's `HtmlHeaderFooter` or `HtmlStamper`:

```csharp
// For headers/footers (appears on all pages)
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div>Header content</div>"
};

// For watermarks/stamps (on all or specific pages)
var stamper = new HtmlStamper()
{
    Html = "<div>Stamp content</div>",
    VerticalAlignment = VerticalAlignment.Middle
};
pdf.ApplyStamp(stamper);
```

### Issue 6: PdfDocument Constructor vs FromFile

**Problem:** EO.Pdf uses `new PdfDocument(path)`, IronPDF uses static factory.

**Solution:**

```csharp
// EO.Pdf
var doc = new PdfDocument("file.pdf");

// IronPDF
var doc = PdfDocument.FromFile("file.pdf");
// Or from bytes:
var doc = PdfDocument.FromBinaryData(bytes);
// Or from stream:
var doc = PdfDocument.FromStream(stream);
```

### Issue 7: Package Size Still Large

**Problem:** Deployment is still large after migration.

**Solution:** IronPDF is optimized but still includes Chromium. For minimal deployments:
- Use trimming if your target framework supports it
- Consider platform-specific packages
- Check IronPDF documentation for size optimization tips

### Issue 8: Linux Deployment Issues

**Problem:** Application worked on Windows but fails on Linux.

**Solution:** IronPDF has true cross-platform support:

```bash
# Ensure required dependencies on Linux (Debian/Ubuntu)
apt-get install -y libgdiplus libc6-dev

# Or use IronPDF's Docker image
FROM ironpdf/ironpdf-dotnet:latest
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all EO.Pdf usage**
  ```bash
  grep -r "using EO.Pdf" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document all ACM patterns in use**
  ```csharp
  // Look for patterns like:
  var acmContent = new AcmContent();
  acmContent.Add(new AcmText("Sample Text"));
  ```
  **Why:** ACM patterns will need to be converted to HTML/CSS for IronPDF.

- [ ] **List static option configurations**
  ```csharp
  // Example:
  HtmlToPdf.Options.PageSize = PdfPageSizes.A4;
  HtmlToPdf.Options.OutputArea = new RectangleF(0, 0, 8.5f, 11f);
  ```
  **Why:** These configurations map to IronPDF's instance-based RenderingOptions.

- [ ] **Backup existing codebase**
  **Why:** Ensure you have a recovery point before making changes.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Test environment setup**
  **Why:** Ensure all dependencies and configurations are ready for migration.

### Package Migration

- [ ] **Remove `EO.Pdf` NuGet package**
  ```bash
  dotnet remove package EO.Pdf
  ```
  **Why:** Clean removal of the old library to prevent conflicts.

- [ ] **Install `IronPdf` NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add the new library to the project.

- [ ] **Configure license key at startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Verify package restore**
  ```bash
  dotnet restore
  ```
  **Why:** Ensure all packages are correctly installed and ready to use.

### Code Migration

- [ ] **Replace `using EO.Pdf` with `using IronPdf`**
  ```csharp
  // Before
  using EO.Pdf;

  // After
  using IronPdf;
  ```
  **Why:** Update namespaces to use IronPDF.

- [ ] **Convert `HtmlToPdf.ConvertHtml()` to `renderer.RenderHtmlAsPdf()`**
  ```csharp
  // Before
  HtmlToPdf.ConvertHtml(html, "output.pdf");

  // After
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer for HTML to PDF conversion.

- [ ] **Convert `HtmlToPdf.ConvertUrl()` to `renderer.RenderUrlAsPdf()`**
  ```csharp
  // Before
  HtmlToPdf.ConvertUrl("https://example.com", "output.pdf");

  // After
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderUrlAsPdf("https://example.com");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF provides direct URL rendering with full JavaScript support.

- [ ] **Replace static `HtmlToPdf.Options` with instance `RenderingOptions`**
  ```csharp
  // Before
  HtmlToPdf.Options.PageSize = PdfPageSizes.A4;

  // After
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** IronPDF uses instance-based options for thread safety.

- [ ] **Convert margin values (inches → mm)**
  ```csharp
  // Before
  HtmlToPdf.Options.OutputArea = new RectangleF(0, 0, 8.5f, 11f);

  // After
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.MarginTop = 25.4; // 1 inch in mm
  renderer.RenderingOptions.MarginBottom = 25.4;
  renderer.RenderingOptions.MarginLeft = 25.4;
  renderer.RenderingOptions.MarginRight = 25.4;
  ```
  **Why:** IronPDF uses millimeters for margin settings.

- [ ] **Migrate ACM code to HTML/CSS**
  ```csharp
  // Before
  var acmContent = new AcmContent();
  acmContent.Add(new AcmText("Sample Text"));

  // After
  var html = "<div>Sample Text</div>";
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** IronPDF uses HTML/CSS for content rendering.

- [ ] **Replace `PdfDocument` constructors with `FromFile()`/`FromStream()`**
  ```csharp
  // Before
  var pdfDoc = new PdfDocument("file.pdf");

  // After
  var pdfDoc = PdfDocument.FromFile("file.pdf");
  ```
  **Why:** IronPDF uses static factory methods for loading PDFs.

- [ ] **Update merge operations**
  ```csharp
  // Before
  PdfDocument.Merge(doc1, doc2);

  // After
  var mergedPdf = PdfDocument.Merge(doc1, doc2);
  ```
  **Why:** IronPDF provides a static method for merging PDFs.

- [ ] **Migrate `AfterRenderPage` to `HtmlHeaderFooter`**
  ```csharp
  // Before
  HtmlToPdf.Options.AfterRenderPage = (page) => { /* custom logic */ };

  // After
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders.

- [ ] **Update security settings**
  ```csharp
  // Before
  pdfDoc.Security.UserPassword = "password";

  // After
  var pdf = PdfDocument.FromFile("file.pdf");
  pdf.SecuritySettings.UserPassword = "password";
  ```
  **Why:** IronPDF provides detailed security settings for PDFs.

### Testing

- [ ] **Visual comparison of generated PDFs**
  **Why:** Verify that the visual output matches expectations.

- [ ] **Verify headers/footers**
  **Why:** Ensure headers and footers render correctly with IronPDF.

- [ ] **Test security/encryption**
  **Why:** Confirm that password protection and permissions are correctly applied.

- [ ] **Validate merging operations**
  **Why:** Ensure PDF merging works as expected with IronPDF.

- [ ] **Check watermarks**
  **Why:** Verify that watermarks are applied correctly.

- [ ] **Performance benchmarking**
  **Why:** Compare performance between EO.Pdf and IronPDF.

- [ ] **Cross-platform testing (Windows, Linux, macOS)**
  **Why:** Ensure consistent behavior across different operating systems.

### Post-Migration

- [ ] **Remove EO.Pdf license files**
  **Why:** Clean up any remaining files related to the old library.

- [ ] **Update documentation**
  **Why:** Ensure all documentation reflects the new IronPDF usage.

- [ ] **Train team on IronPDF API**
  **Why:** Ensure the development team is familiar with the new library.

- [ ] **Monitor production deployments**
  **Why:** Watch for any issues that may arise after migration.

- [ ] **Archive EO.Pdf code for reference**
  **Why:** Keep a record of the old implementation for historical reference.
---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **IronPDF Support**: https://ironpdf.com/support/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
