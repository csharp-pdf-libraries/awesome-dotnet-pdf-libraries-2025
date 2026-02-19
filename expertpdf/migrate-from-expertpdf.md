# How Do I Migrate from ExpertPdf to IronPDF in C#?

## Table of Contents

1. [Why Migrate from ExpertPdf to IronPDF](#why-migrate-from-expertpdf-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Examples](#code-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from ExpertPdf to IronPDF

### The ExpertPdf Problems

1. **Documentation Frozen Since 2018**: ExpertPdf's documentation hasn't been updated in over 6 years. Finding current information, examples, and best practices is increasingly difficult.

2. **Outdated Chrome Version**: ExpertPdf relies on a legacy version of Chrome for rendering. Modern CSS3 features (Flexbox, Grid, CSS Variables) may not render correctly, and security patches are not applied.

3. **Premium Pricing for Legacy Tech**: At $550-$1,200 per license, ExpertPdf charges premium prices while delivering outdated rendering technology.

4. **Fragmented Product Suite**: ExpertPdf sells separate packages for different functions:
   - HtmlToPdf Converter
   - PDF Merger
   - PDF Security
   - PDF Splitter
   - PDF to Image

   Each requires separate licensing.

5. **Limited Modern .NET Support**: While ExpertPdf has .NET Core packages, they lag behind modern .NET versions and practices.

### ExpertPdf vs IronPDF Comparison

| Aspect | ExpertPdf | IronPDF |
|--------|-----------|---------|
| **Documentation** | Frozen since 2018 | Continuously updated |
| **Rendering Engine** | Legacy Chrome | Latest Chromium |
| **CSS Support** | Limited CSS3 | Full CSS3 (Flexbox, Grid) |
| **Price** | $550-$1,200 | Competitive pricing |
| **Update Frequency** | Infrequent | Monthly releases |
| **Product Model** | Fragmented (5+ DLLs) | All-in-one library |
| **Modern .NET** | Limited | .NET 6/7/8/9+ native |
| **Async Support** | Limited | Full async/await |
| **Security Updates** | Infrequent | Regular patches |

### Key Migration Benefits

More information can be found in the [extensive documentation](https://ironpdf.com/blog/migration-guides/migrate-from-expertpdf-to-ironpdf/).

1. **Modern Rendering**: Latest Chromium engine for pixel-perfect output
2. **All-in-One Package**: PDF generation, merging, security, extraction in one NuGet
3. **Active Development**: Monthly updates with new features and security patches
4. **Better Documentation**: Comprehensive tutorials and examples
5. **True Cross-Platform**: Windows, Linux, macOS, Docker support
6. **Modern .NET**: Native support for .NET 6/7/8/9

---

## Before You Start

### 1. Inventory Your ExpertPdf Usage

Identify all ExpertPdf components in use:

```bash
# Find all ExpertPdf references
grep -r "ExpertPdf\|PdfConverter\|PDFMerge\|PdfSecurityManager" --include="*.cs" .

# Check NuGet packages
dotnet list package | grep -i "ExpertPdf"
```

**Common ExpertPdf packages:**
- `ExpertPdf.HtmlToPdf` - HTML to PDF conversion
- `ExpertPdf.PDFMerge` - PDF merging
- `ExpertPdf.PDFSecurity` - Encryption and passwords
- `ExpertPdf.PDFSplit` - PDF splitting
- `ExpertPdf.PdfToImage` - PDF to image conversion

### 2. Document Current Functionality

Create a checklist of ExpertPdf features you use:

- [ ] HTML to PDF (`PdfConverter`)
- [ ] URL to PDF (`GetPdfBytesFromUrl`)
- [ ] PDF merging (`PDFMerge`)
- [ ] PDF security (`PdfSecurityOptions`)
- [ ] Headers and footers (`PdfHeaderOptions`, `PdfFooterOptions`)
- [ ] Page numbering (`&p;` and `&P;` tokens)
- [ ] Custom page sizes
- [ ] PDF splitting
- [ ] PDF to image conversion

### 3. Set Up IronPDF

```bash
# Remove all ExpertPdf packages
dotnet remove package ExpertPdf.HtmlToPdf
dotnet remove package ExpertPdf.PDFMerge
dotnet remove package ExpertPdf.PDFSecurity
dotnet remove package ExpertPdf.PDFSplit
dotnet remove package ExpertPdf.PdfToImage

# Install IronPDF (includes all features)
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

ExpertPdf uses `PdfConverter` with direct-to-file methods. IronPDF uses `ChromePdfRenderer` returning `PdfDocument` objects.

**ExpertPdf pattern:**
```csharp
PdfConverter pdfConverter = new PdfConverter();
pdfConverter.LicenseKey = "LICENSE";
pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
pdfConverter.SavePdfFromUrlToFile(url, "output.pdf");
```

**IronPDF pattern:**
```csharp
IronPdf.License.LicenseKey = "LICENSE";
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
var pdf = renderer.RenderUrlAsPdf(url);
pdf.SaveAs("output.pdf");
```

### Minimal Migration Example

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

public class PdfService
{
    public void GenerateReport(string html)
    {
        PdfConverter pdfConverter = new PdfConverter();
        pdfConverter.LicenseKey = "EXPERTPDF-LICENSE";
        pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
        pdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;
        pdfConverter.PdfDocumentOptions.MarginTop = 20;
        pdfConverter.PdfDocumentOptions.MarginBottom = 20;

        byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(html);
        File.WriteAllBytes("report.pdf", pdfBytes);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    public PdfService()
    {
        IronPdf.License.LicenseKey = "IRONPDF-LICENSE";
    }

    public void GenerateReport(string html)
    {
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report.pdf");
    }
}
```

---

## Complete API Reference

### Namespace Mapping

| ExpertPdf Namespace | IronPDF Equivalent |
|--------------------|-------------------|
| `ExpertPdf.HtmlToPdf` | `IronPdf` |
| `ExpertPdf.PDFMerge` | `IronPdf` |
| `ExpertPdf.PDFSecurity` | `IronPdf` |
| `ExpertPdf.PDFSplit` | `IronPdf` |

### Core Class Mapping

| ExpertPdf Class | IronPDF Equivalent | Notes |
|----------------|-------------------|-------|
| `PdfConverter` | `ChromePdfRenderer` | Main conversion class |
| `PdfDocumentOptions` | `ChromePdfRenderOptions` | Via `RenderingOptions` |
| `PdfSecurityOptions` | `PdfDocument.SecuritySettings` | |
| `PdfHeaderOptions` | `HtmlHeaderFooter` | Configurable HTML |
| `PdfFooterOptions` | `HtmlHeaderFooter` | Configurable HTML |
| `PDFMerge` | `PdfDocument.Merge()` | Static method |
| `PdfSecurityManager` | `PdfDocument.SecuritySettings` | |
| `ImgConverter` | `PdfDocument.RasterizeToImageFiles()` | |

### Method Mapping

| ExpertPdf Method | IronPDF Method | Notes |
|-----------------|----------------|-------|
| `pdfConverter.GetPdfBytesFromHtmlString(html)` | `renderer.RenderHtmlAsPdf(html).BinaryData` | |
| `pdfConverter.GetPdfBytesFromUrl(url)` | `renderer.RenderUrlAsPdf(url).BinaryData` | |
| `pdfConverter.GetPdfBytesFromHtmlFile(path)` | `renderer.RenderHtmlFileAsPdf(path).BinaryData` | |
| `pdfConverter.SavePdfFromUrlToFile(url, path)` | `renderer.RenderUrlAsPdf(url).SaveAs(path)` | Two-step |
| `pdfConverter.SavePdfFromHtmlStringToFile(html, path)` | `renderer.RenderHtmlAsPdf(html).SaveAs(path)` | Two-step |
| `pdfMerge.AppendPDFFile(path)` | `PdfDocument.Merge()` | Static merge |
| `pdfMerge.SaveMergedPDFToFile(path)` | `merged.SaveAs(path)` | |
| `pdfMerge.AppendImageFile(path)` | Render image as HTML | |

### Options Mapping

| ExpertPdf Option | IronPDF RenderingOptions | Notes |
|-----------------|-------------------------|-------|
| `PdfDocumentOptions.PdfPageSize = PdfPageSize.A4` | `PaperSize = PdfPaperSize.A4` | |
| `PdfDocumentOptions.PdfPageSize = PdfPageSize.Letter` | `PaperSize = PdfPaperSize.Letter` | |
| `PdfDocumentOptions.PdfPageOrientation = Portrait` | `PaperOrientation = PdfPaperOrientation.Portrait` | |
| `PdfDocumentOptions.PdfPageOrientation = Landscape` | `PaperOrientation = PdfPaperOrientation.Landscape` | |
| `PdfDocumentOptions.MarginTop` | `MarginTop` | Same property name |
| `PdfDocumentOptions.MarginBottom` | `MarginBottom` | |
| `PdfDocumentOptions.MarginLeft` | `MarginLeft` | |
| `PdfDocumentOptions.MarginRight` | `MarginRight` | |
| `PdfDocumentOptions.ShowHeader = true` | Set `HtmlHeader` property | Use HtmlHeaderFooter |
| `PdfDocumentOptions.ShowFooter = true` | Set `HtmlFooter` property | Use HtmlHeaderFooter |
| `PdfDocumentOptions.PdfCompressionLevel` | `CompressImages` | Different approach |
| `PdfDocumentOptions.LiveUrlsEnabled` | Enabled by default | Links work automatically |
| `PdfDocumentOptions.EmbedFonts` | `CustomCssUrl` with @font-face | Or system fonts |

### Header/Footer Placeholder Mapping

| ExpertPdf Token | IronPDF Placeholder | Notes |
|----------------|---------------------|-------|
| `&p;` | `{page}` | Current page number |
| `&P;` | `{total-pages}` | Total page count |
| `&d;` | `{date}` | Current date |
| `&t;` | `{time}` | Current time |
| `&u;` | `{url}` | Source URL |

### Security Mapping

| ExpertPdf Security | IronPDF Equivalent |
|-------------------|-------------------|
| `PdfSecurityOptions.UserPassword` | `pdf.SecuritySettings.UserPassword` |
| `PdfSecurityOptions.OwnerPassword` | `pdf.SecuritySettings.OwnerPassword` |
| `PdfSecurityOptions.CanPrint` | `pdf.SecuritySettings.AllowUserPrinting` |
| `PdfSecurityOptions.CanCopyContent` | `pdf.SecuritySettings.AllowUserCopyPasteContent` |
| `PdfSecurityOptions.CanEditContent` | `pdf.SecuritySettings.AllowUserEdits` |
| `PdfSecurityOptions.CanFillFormFields` | `pdf.SecuritySettings.AllowUserFormData` |

---

## Code Examples

### Example 1: Basic HTML to PDF

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

PdfConverter pdfConverter = new PdfConverter();
pdfConverter.LicenseKey = "YOUR_LICENSE_KEY";

string html = "<html><body><h1>Hello World</h1><p>Generated with ExpertPdf</p></body></html>";
byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(html);

File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
string html = "<html><body><h1>Hello World</h1><p>Generated with IronPDF</p></body></html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Custom Options

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

PdfConverter pdfConverter = new PdfConverter();
pdfConverter.LicenseKey = "YOUR_LICENSE_KEY";
pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
pdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Landscape;
pdfConverter.PdfDocumentOptions.MarginLeft = 15;
pdfConverter.PdfDocumentOptions.MarginRight = 15;
pdfConverter.PdfDocumentOptions.MarginTop = 20;
pdfConverter.PdfDocumentOptions.MarginBottom = 20;

pdfConverter.SavePdfFromUrlToFile("https://example.com", "webpage.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginLeft = 15;
renderer.RenderingOptions.MarginRight = 15;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Headers and Footers

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

PdfConverter pdfConverter = new PdfConverter();
pdfConverter.LicenseKey = "YOUR_LICENSE_KEY";

// Enable and configure header
pdfConverter.PdfDocumentOptions.ShowHeader = true;
pdfConverter.PdfHeaderOptions.HeaderText = "Company Report";
pdfConverter.PdfHeaderOptions.HeaderTextAlignment = HorizontalTextAlign.Center;
pdfConverter.PdfHeaderOptions.HeaderTextColor = System.Drawing.Color.Navy;
pdfConverter.PdfHeaderOptions.HeaderTextFontSize = 12;
pdfConverter.PdfHeaderOptions.DrawHeaderLine = true;

// Enable and configure footer with page numbers
pdfConverter.PdfDocumentOptions.ShowFooter = true;
pdfConverter.PdfFooterOptions.FooterText = "Page &p; of &P;";
pdfConverter.PdfFooterOptions.FooterTextAlignment = HorizontalTextAlign.Right;
pdfConverter.PdfFooterOptions.DrawFooterLine = true;

byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(html);
File.WriteAllBytes("report.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();

// Configure header with HTML
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = @"<div style='text-align:center; color:navy; font-size:12pt; font-weight:bold;'>
                        Company Report
                     </div>",
    DrawDividerLine = true
};

// Configure footer with page numbers
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = @"<div style='text-align:right; font-size:10pt;'>
                        Page {page} of {total-pages}
                     </div>",
    DrawDividerLine = true
};

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 4: Merging PDFs

**Before (ExpertPdf - requires separate PDFMerge package):**
```csharp
using ExpertPdf.PDFMerge;

PdfDocumentOptions options = new PdfDocumentOptions();
options.PdfCompressionLevel = PDFCompressionLevel.Normal;
options.PdfPageSize = PdfPageSize.A4;

PDFMerge pdfMerge = new PDFMerge(options);
pdfMerge.AppendPDFFile("document1.pdf");
pdfMerge.AppendPDFFile("document2.pdf");
pdfMerge.AppendPDFFile("document3.pdf");

pdfMerge.SaveMergedPDFToFile("merged.pdf");
```

**After (IronPDF - included in main package):**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("document1.pdf");
var pdf2 = PdfDocument.FromFile("document2.pdf");
var pdf3 = PdfDocument.FromFile("document3.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("merged.pdf");
```

### Example 5: PDF Security and Encryption

**Before (ExpertPdf - requires separate PDFSecurity package):**
```csharp
using ExpertPdf.HtmlToPdf;

PdfConverter pdfConverter = new PdfConverter();
pdfConverter.LicenseKey = "YOUR_LICENSE_KEY";

// Set security options
pdfConverter.PdfSecurityOptions.UserPassword = "userpass";
pdfConverter.PdfSecurityOptions.OwnerPassword = "ownerpass";
pdfConverter.PdfSecurityOptions.CanPrint = true;
pdfConverter.PdfSecurityOptions.CanCopyContent = false;
pdfConverter.PdfSecurityOptions.CanEditContent = false;
pdfConverter.PdfSecurityOptions.CanFillFormFields = true;

byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(html);
File.WriteAllBytes("secure.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);

// Set security options
pdf.SecuritySettings.UserPassword = "userpass";
pdf.SecuritySettings.OwnerPassword = "ownerpass";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserFormData = true;

pdf.SaveAs("secure.pdf");
```

### Example 6: HTML File to PDF

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

PdfConverter pdfConverter = new PdfConverter();
pdfConverter.LicenseKey = "YOUR_LICENSE_KEY";
pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;

// Set base URL for relative resources
pdfConverter.PdfDocumentOptions.BaseUrl = "file:///C:/WebProject/";

byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlFile("C:/WebProject/template.html");
File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

// Base URL for relative resources
renderer.RenderingOptions.BaseUrl = new Uri("file:///C:/WebProject/");

var pdf = renderer.RenderHtmlFileAsPdf("C:/WebProject/template.html");
pdf.SaveAs("output.pdf");
```

### Example 7: Custom Page Size

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

PdfConverter pdfConverter = new PdfConverter();
pdfConverter.LicenseKey = "YOUR_LICENSE_KEY";

// Custom page size in points (72 points = 1 inch)
pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.Custom;
pdfConverter.PdfDocumentOptions.CustomPdfPageWidth = 432;  // 6 inches
pdfConverter.PdfDocumentOptions.CustomPdfPageHeight = 288; // 4 inches

byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(html);
File.WriteAllBytes("custom-size.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();

// Custom page size in millimeters (6" x 4" = 152.4mm x 101.6mm)
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(152.4, 101.6);

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("custom-size.pdf");
```

### Example 8: JavaScript Execution Delay

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

PdfConverter pdfConverter = new PdfConverter();
pdfConverter.LicenseKey = "YOUR_LICENSE_KEY";

// Wait for JavaScript execution
pdfConverter.NavigationTimeout = 60;  // seconds
pdfConverter.ConversionDelay = 5;     // seconds after page load

byte[] pdfBytes = pdfConverter.GetPdfBytesFromUrl("https://example.com/dynamic-page");
File.WriteAllBytes("dynamic.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();

// Wait for JavaScript execution
renderer.RenderingOptions.Timeout = 60000;        // milliseconds
renderer.RenderingOptions.RenderDelay = 5000;     // milliseconds after page load

// Or wait for specific JavaScript condition
renderer.RenderingOptions.WaitFor.JavaScript("window.chartReady === true");

var pdf = renderer.RenderUrlAsPdf("https://example.com/dynamic-page");
pdf.SaveAs("dynamic.pdf");
```

### Example 9: Return Bytes for Web Response

**Before (ExpertPdf):**
```csharp
using ExpertPdf.HtmlToPdf;

public byte[] GeneratePdfBytes(string html)
{
    PdfConverter pdfConverter = new PdfConverter();
    pdfConverter.LicenseKey = "YOUR_LICENSE_KEY";
    pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;

    return pdfConverter.GetPdfBytesFromHtmlString(html);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] GeneratePdfBytes(string html)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 10: PDF to Image Conversion

**Before (ExpertPdf - requires separate PdfToImage package):**
```csharp
using ExpertPdf.PdfToImage;

PdfToImageConverter converter = new PdfToImageConverter();
converter.LicenseKey = "YOUR_LICENSE_KEY";

// Convert all pages to images
converter.SavePdfPagesToImages("document.pdf", "output_page", ImageFormat.Png);
```

**After (IronPDF - included):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Convert all pages to images
pdf.RasterizeToImageFiles("output_page_*.png");

// Or get specific page as image
var pageImage = pdf.PageToBitmap(0);
pageImage.Save("first_page.png");
```

---

## Advanced Scenarios

### Migrating Multiple ExpertPdf Packages

If you're using multiple ExpertPdf packages, IronPDF consolidates everything:

```csharp
// BEFORE: Multiple ExpertPdf packages
using ExpertPdf.HtmlToPdf;      // HTML conversion
using ExpertPdf.PDFMerge;       // Merging
using ExpertPdf.PDFSecurity;    // Encryption
using ExpertPdf.PDFSplit;       // Splitting
using ExpertPdf.PdfToImage;     // Image conversion

// AFTER: Single IronPDF package
using IronPdf;
// All functionality in one namespace!
```

### Thread-Safe Service Pattern

**Before (ExpertPdf):**
```csharp
public class PdfService
{
    public byte[] Generate(string html)
    {
        // Create new instance each time
        PdfConverter pdfConverter = new PdfConverter();
        pdfConverter.LicenseKey = "LICENSE";
        return pdfConverter.GetPdfBytesFromHtmlString(html);
    }
}
```

**After (IronPDF - renderer can be reused):**
```csharp
public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
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

### Async Operations

ExpertPdf is primarily synchronous. IronPDF provides async methods:

```csharp
public async Task<byte[]> GeneratePdfAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}

public async Task<byte[]> CaptureUrlAsync(string url)
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderUrlAsPdfAsync(url);
    return pdf.BinaryData;
}
```

---

## Performance Considerations

### Memory Management

**ExpertPdf:**
```csharp
PdfConverter pdfConverter = new PdfConverter();
byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(html);
// No explicit dispose needed
```

**IronPDF:**
```csharp
using var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// Automatic cleanup with using
```

### Renderer Reuse

IronPDF renderers can be reused for better performance:

```csharp
public class OptimizedPdfService
{
    private readonly ChromePdfRenderer _renderer;

    public OptimizedPdfService()
    {
        IronPdf.License.LicenseKey = "YOUR_LICENSE";
        _renderer = new ChromePdfRenderer();
        // Configure once
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

### Issue 1: License Key Location Changed

**Problem:** `pdfConverter.LicenseKey` no longer works.

**Solution:** Set license globally at startup:

```csharp
// ExpertPdf (per-instance)
pdfConverter.LicenseKey = "LICENSE";

// IronPDF (global, once at startup)
IronPdf.License.LicenseKey = "LICENSE";
```

### Issue 2: GetPdfBytes* Returns Object Instead of Bytes

**Problem:** IronPDF methods return `PdfDocument`, not `byte[]`.

**Solution:** Access `BinaryData` property:

```csharp
// ExpertPdf
byte[] bytes = pdfConverter.GetPdfBytesFromHtmlString(html);

// IronPDF
var pdf = renderer.RenderHtmlAsPdf(html);
byte[] bytes = pdf.BinaryData;
```

### Issue 3: Header/Footer Placeholders Different

**Problem:** `&p;` and `&P;` don't work in IronPDF.

**Solution:** Use IronPDF placeholders:

```csharp
// ExpertPdf: "Page &p; of &P;"
// IronPDF:
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "Page {page} of {total-pages}"
};
```

### Issue 4: ShowHeader/ShowFooter Booleans Gone

**Problem:** `ShowHeader = true` doesn't exist.

**Solution:** In IronPDF, headers appear when you set the `HtmlHeader` property:

```csharp
// ExpertPdf
pdfConverter.PdfDocumentOptions.ShowHeader = true;
pdfConverter.PdfHeaderOptions.HeaderText = "Header";

// IronPDF - just set the header content
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div>Header</div>"
};
// No header appears if HtmlHeader is not set
```

### Issue 5: PDF Merging Requires Different Package

**Problem:** `PDFMerge` class is missing.

**Solution:** IronPDF includes merging in the main package:

```csharp
// ExpertPdf (separate package)
using ExpertPdf.PDFMerge;
PDFMerge merger = new PDFMerge();
merger.AppendPDFFile("file1.pdf");
merger.AppendPDFFile("file2.pdf");
merger.SaveMergedPDFToFile("output.pdf");

// IronPDF (same package)
var pdf1 = PdfDocument.FromFile("file1.pdf");
var pdf2 = PdfDocument.FromFile("file2.pdf");
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("output.pdf");
```

### Issue 6: Custom Page Size Units Different

**Problem:** ExpertPdf uses points, IronPDF uses millimeters.

**Solution:** Convert units: `points / 72 * 25.4 = millimeters`

```csharp
// ExpertPdf: 432 x 288 points
pdfConverter.PdfDocumentOptions.CustomPdfPageWidth = 432;
pdfConverter.PdfDocumentOptions.CustomPdfPageHeight = 288;

// IronPDF: Convert to mm (432/72*25.4 = 152.4, 288/72*25.4 = 101.6)
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(152.4, 101.6);
```

### Issue 7: BaseUrl Not Working

**Problem:** Relative resources (images, CSS) not loading.

**Solution:** Set `BaseUrl` as a `Uri` object:

```csharp
// ExpertPdf
pdfConverter.PdfDocumentOptions.BaseUrl = "file:///C:/Project/";

// IronPDF
renderer.RenderingOptions.BaseUrl = new Uri("file:///C:/Project/");
```

### Issue 8: Rendering Looks Different

**Problem:** PDF output looks different after migration.

**Solution:** IronPDF uses modern Chromium which renders CSS3 more accurately. Your content may actually render better. If you need to match legacy output exactly, adjust CSS:

```css
/* Force specific rendering if needed */
* {
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
}
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all ExpertPdf packages in use**
  ```bash
  grep -r "using ExpertPdf" --include="*.cs" .
  grep -r "PdfConverter\|PdfMerge" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document features being used**
  ```csharp
  // Find patterns like:
  var converter = new PdfConverter();
  converter.Options = new PdfDocumentOptions {
      PdfPageSize = PdfPageSize.A4,
      ShowHeader = true
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Note any custom page sizes or options**
  ```csharp
  // Example:
  converter.Options.CustomPageSize = new SizeF(500, 700);
  ```
  **Why:** Custom sizes need to be converted from points to millimeters in IronPDF.

- [ ] **Backup existing codebase**
  **Why:** Ensure you can revert changes if needed during migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Set up test environment**
  **Why:** Test changes in a safe environment before deploying to production.

### Package Migration

- [ ] **Remove `ExpertPdf.HtmlToPdf` package**
  ```bash
  dotnet remove package ExpertPdf.HtmlToPdf
  ```
  **Why:** Clean removal of old package to prevent conflicts.

- [ ] **Remove `ExpertPdf.PDFMerge` package (if used)**
  ```bash
  dotnet remove package ExpertPdf.PDFMerge
  ```
  **Why:** IronPDF handles merging internally, simplifying dependencies.

- [ ] **Remove `ExpertPdf.PDFSecurity` package (if used)**
  ```bash
  dotnet remove package ExpertPdf.PDFSecurity
  ```
  **Why:** IronPDF includes security features natively.

- [ ] **Remove `ExpertPdf.PDFSplit` package (if used)**
  ```bash
  dotnet remove package ExpertPdf.PDFSplit
  ```
  **Why:** IronPDF can handle PDF splitting without additional packages.

- [ ] **Remove `ExpertPdf.PdfToImage` package (if used)**
  ```bash
  dotnet remove package ExpertPdf.PdfToImage
  ```
  **Why:** IronPDF can convert PDFs to images directly.

- [ ] **Install `IronPdf` package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to access its modern PDF functionalities.

- [ ] **Configure license key at startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Code Migration

- [ ] **Replace `using ExpertPdf.*` with `using IronPdf`**
  ```csharp
  // Before (ExpertPdf)
  using ExpertPdf.HtmlToPdf;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Update namespaces to use IronPDF's classes and methods.

- [ ] **Replace `PdfConverter` with `ChromePdfRenderer`**
  ```csharp
  // Before (ExpertPdf)
  var converter = new PdfConverter();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** ChromePdfRenderer provides modern rendering capabilities.

- [ ] **Convert `GetPdfBytesFrom*` to `Render*AsPdf().BinaryData`**
  ```csharp
  // Before (ExpertPdf)
  var pdfBytes = converter.GetPdfBytesFromHtmlString(html);

  // After (IronPDF)
  var pdfBytes = renderer.RenderHtmlAsPdf(html).BinaryData;
  ```
  **Why:** IronPDF provides direct access to PDF binary data.

- [ ] **Convert `SavePdfFrom*ToFile` to `Render*AsPdf().SaveAs()`**
  ```csharp
  // Before (ExpertPdf)
  converter.SavePdfFromUrlToFile(url, "output.pdf");

  // After (IronPDF)
  renderer.RenderUrlAsPdf(url).SaveAs("output.pdf");
  ```
  **Why:** Simplifies the process by using IronPDF's direct save method.

- [ ] **Update header/footer configuration to `HtmlHeaderFooter`**
  ```csharp
  // Before (ExpertPdf)
  converter.Options.ShowHeader = true;
  converter.HeaderText = "Page &p; of &P;";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with flexible styling and placeholders.

- [ ] **Convert page numbering placeholders (`&p;` → `{page}`)**
  ```csharp
  // Before (ExpertPdf)
  converter.HeaderText = "Page &p; of &P;";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** IronPDF uses modern placeholders for dynamic content.

- [ ] **Move license key to global configuration**
  ```csharp
  // Before (ExpertPdf)
  // License key setup in multiple places

  // After (IronPDF)
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** Centralize license management for easier maintenance.

- [ ] **Update security settings syntax**
  ```csharp
  // Before (ExpertPdf)
  converter.SecurityOptions.UserPassword = "password";

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SecuritySettings.UserPassword = "password";
  ```
  **Why:** IronPDF provides a straightforward security settings API.

- [ ] **Convert custom page sizes (points → mm)**
  ```csharp
  // Before (ExpertPdf)
  converter.Options.CustomPageSize = new SizeF(500, 700);

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = new PdfPaperSize(176, 247); // Convert points to mm
  ```
  **Why:** IronPDF uses millimeters for page size settings.

- [ ] **Replace PDFMerge with `PdfDocument.Merge()`**
  ```csharp
  // Before (ExpertPdf)
  var merger = new PdfMerge();
  merger.AppendPDFFile("file1.pdf");
  merger.AppendPDFFile("file2.pdf");
  merger.SaveMergedPDFToFile("merged.pdf");

  // After (IronPDF)
  var pdf1 = PdfDocument.FromFile("file1.pdf");
  var pdf2 = PdfDocument.FromFile("file2.pdf");
  var merged = PdfDocument.Merge(pdf1, pdf2);
  merged.SaveAs("merged.pdf");
  ```
  **Why:** IronPDF provides a simple static method for merging PDFs.

### Testing

- [ ] **Visual comparison of generated PDFs**
  **Why:** Ensure the output quality is consistent with expectations.

- [ ] **Verify headers/footers and page numbers**
  **Why:** Confirm that dynamic content renders correctly.

- [ ] **Test security/encryption**
  **Why:** Validate that password protection and permissions are applied as expected.

- [ ] **Validate merging operations**
  **Why:** Ensure merged documents are correctly combined and functional.

- [ ] **Check custom page sizes**
  **Why:** Verify that custom sizes are applied correctly in the output.

- [ ] **Performance benchmarking**
  **Why:** Assess any performance improvements or regressions.

- [ ] **Cross-platform testing**
  **Why:** Ensure compatibility across different operating systems.

### Post-Migration

- [ ] **Remove ExpertPdf license files**
  **Why:** Clean up legacy files no longer needed.

- [ ] **Update documentation**
  **Why:** Reflect changes in codebase and new library usage.

- [ ] **Train team on IronPDF API**
  **Why:** Ensure all team members are familiar with the new library.

- [ ] **Monitor production deployments**
  **Why:** Watch for any issues that may arise in a live environment.

- [ ] **Archive ExpertPdf code for reference**
  **Why:** Keep a record of the old implementation for future reference if needed.
---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **IronPDF Support**: https://ironpdf.com/support/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
