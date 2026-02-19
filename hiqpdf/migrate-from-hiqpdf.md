# How Do I Migrate from HiQPdf to IronPDF in C#?

## Table of Contents

1. [Why Migrate from HiQPdf to IronPDF](#why-migrate-from-hiqpdf-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Comparison](#performance-comparison)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from HiQPdf to IronPDF

### The HiQPdf Limitations

HiQPdf is a commercial HTML-to-PDF library with several concerning limitations:

1. **Restrictive "Free" Version**: The free version imposes a 3-page limit with intrusive watermarks—essentially unusable for production
2. **Older WebKit Engine**: Uses an older WebKit-based rendering engine that struggles with modern JavaScript frameworks
3. **Unclear .NET Core Support**: Documentation doesn't explicitly clarify .NET Core / .NET 5+ support, requiring separate NuGet packages
4. **Fragmented Packages**: Multiple NuGet packages for different platforms (HiQPdf, HiQPdf.NetCore, HiQPdf.Client)
5. **Complex API**: Requires verbose configuration through `Document`, `Header`, `Footer` property chains
6. **Limited JavaScript Support**: WebKit engine has challenges with React, Angular, Vue and modern JS frameworks

### What IronPDF Offers Instead

| Aspect | HiQPdf | IronPDF |
|--------|--------|---------|
| Rendering Engine | WebKit-based (older) | Modern Chromium |
| Free Tier | 3-page limit + watermark | 30-day full trial |
| Modern JS Support | Limited | Full (React, Angular, Vue) |
| .NET Core/5+ Support | Multiple packages needed | Single unified package |
| API Design | Complex property chains | Clean fluent API |
| CSS3 Support | Partial | Full support |
| Documentation | Fragmented | Comprehensive |
| NuGet Package | Multiple variants | Single package |

---

## Before You Start

### Prerequisites

1. **.NET Version**: IronPDF supports .NET Framework 4.6.2+ and .NET Core 3.1+ / .NET 5+
2. **License Key**: Obtain from [IronPDF website](https://ironpdf.com/licensing/)
3. **Remove HiQPdf**: Plan to remove all HiQPdf NuGet package variants

### Identify HiQPdf Usage

Find all HiQPdf usage in your codebase:

```bash
# Find HiQPdf namespace usage
grep -r "using HiQPdf\|HtmlToPdf\|PdfDocument" --include="*.cs" .

# Find HiQPdf document configuration
grep -r "BrowserWidth\|TriggerMode\|PageOrientation\|ConvertHtmlToMemory" --include="*.cs" .

# Find header/footer usage
grep -r "\.Header\.\|\.Footer\.\|HtmlToPdfVariableElement" --include="*.cs" .

# Find NuGet references
grep -r "HiQPdf" --include="*.csproj" .
```

### Dependency Audit

Check for HiQPdf package variants:

```bash
grep -r "HiQPdf\|hiqpdf" --include="*.csproj" .
```

Common package names:
- `HiQPdf`
- `HiQPdf.Free`
- `HiQPdf.NetCore`
- `HiQPdf.NetCore.x64`
- `HiQPdf.Client`

---

## Quick Start Migration

### Step 1: Install IronPDF

```bash
# Remove all HiQPdf variants
dotnet remove package HiQPdf
dotnet remove package HiQPdf.Free
dotnet remove package HiQPdf.NetCore
dotnet remove package HiQPdf.NetCore.x64
dotnet remove package HiQPdf.Client

# Install IronPDF (single package for all platforms)
dotnet add package IronPdf
```

### Step 2: Update Code

**Before (HiQPdf):**
```csharp
using HiQPdf;

public class PdfService
{
    public byte[] GeneratePdf(string html)
    {
        HtmlToPdf htmlToPdfConverter = new HtmlToPdf();

        // Browser settings
        htmlToPdfConverter.BrowserWidth = 1024;

        // Page settings
        htmlToPdfConverter.Document.PageSize = PdfPageSize.A4;
        htmlToPdfConverter.Document.PageOrientation = PdfPageOrientation.Portrait;
        htmlToPdfConverter.Document.Margins.Left = 10;
        htmlToPdfConverter.Document.Margins.Right = 10;

        // Convert to memory
        byte[] pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, null);
        return pdfBuffer;
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    public byte[] GeneratePdf(string html)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Viewport settings
        renderer.RenderingOptions.ViewPortWidth = 1024;

        // Page settings
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        renderer.RenderingOptions.MarginLeft = 10;
        renderer.RenderingOptions.MarginRight = 10;

        // Convert and return
        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Step 3: Update License Configuration

**Before (HiQPdf):**
```csharp
// License in constructor or property
HtmlToPdf converter = new HtmlToPdf();
converter.SerialNumber = "HIQPDF-SERIAL-NUMBER";
```

**After (IronPDF):**
```csharp
// Set globally at application startup (Program.cs / Startup.cs)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Or in appsettings.json
// { "IronPdf": { "LicenseKey": "YOUR-KEY" } }
```

---

## Complete API Reference

### Main Class Mapping

| HiQPdf Class | IronPDF Class | Notes |
|-------------|--------------|-------|
| `HtmlToPdf` | `ChromePdfRenderer` | Main converter class |
| `PdfDocument` | `PdfDocument` | Same name, different namespace |
| `PdfPage` | `pdf.Pages[i]` | Access via indexer |
| `PdfDocumentControl` | `RenderingOptions` | Configuration |
| `PdfHeader` / `PdfDocumentHeader` | `HtmlHeaderFooter` | Header configuration |
| `PdfFooter` / `PdfDocumentFooter` | `HtmlHeaderFooter` | Footer configuration |
| `HtmlToPdfVariableElement` | `HtmlHeaderFooter.HtmlFragment` | HTML in headers/footers |

### Conversion Method Mapping

| HiQPdf Method | IronPDF Method | Notes |
|--------------|----------------|-------|
| `ConvertHtmlToMemory(html, baseUrl)` | `RenderHtmlAsPdf(html, baseUrl)` | Returns `PdfDocument` |
| `ConvertUrlToMemory(url)` | `RenderUrlAsPdf(url)` | Returns `PdfDocument` |
| `ConvertHtmlToFile(html, baseUrl, path)` | `RenderHtmlAsPdf(html).SaveAs(path)` | Chain methods |
| `ConvertUrlToFile(url, path)` | `RenderUrlAsPdf(url).SaveAs(path)` | Chain methods |
| `ConvertHtmlToPdfDocument(html, baseUrl)` | `RenderHtmlAsPdf(html)` | Returns `PdfDocument` |

### HtmlToPdf Property Mapping

| HiQPdf Property | IronPDF Property | Notes |
|----------------|------------------|-------|
| `BrowserWidth` | `RenderingOptions.ViewPortWidth` | In pixels |
| `BrowserHeight` | `RenderingOptions.ViewPortHeight` | In pixels |
| `TriggerMode` | `RenderingOptions.WaitFor` | Wait conditions |
| `WaitBeforeConvert` | `RenderingOptions.RenderDelay` | In milliseconds |
| `TrimToBrowserWidth` | N/A | IronPDF handles automatically |
| `Document.PageSize` | `RenderingOptions.PaperSize` | Use enum |
| `Document.PageOrientation` | `RenderingOptions.PaperOrientation` | `Portrait`/`Landscape` |
| `Document.Margins.Top` | `RenderingOptions.MarginTop` | In mm (not points!) |
| `Document.Margins.Bottom` | `RenderingOptions.MarginBottom` | In mm |
| `Document.Margins.Left` | `RenderingOptions.MarginLeft` | In mm |
| `Document.Margins.Right` | `RenderingOptions.MarginRight` | In mm |
| `Document.FitPageWidth` | `RenderingOptions.FitToPaperMode` | Scaling options |
| `Document.FitPageHeight` | `RenderingOptions.FitToPaperMode` | Scaling options |
| `Document.ResizePageWidth` | N/A | IronPDF auto-handles |
| `SerialNumber` | `IronPdf.License.LicenseKey` | Set globally |

### TriggerMode Mapping

| HiQPdf TriggerMode | IronPDF Equivalent | Notes |
|-------------------|-------------------|-------|
| `TriggerMode.Auto` | Default behavior | No special setting |
| `TriggerMode.WaitTime` | `RenderingOptions.RenderDelay = ms` | Wait fixed time |
| `TriggerMode.Manual` | `RenderingOptions.WaitFor.JavaScript(expr)` | Wait for JS condition |
| `WaitBeforeConvert` | `RenderingOptions.RenderDelay` | Milliseconds |

### Header/Footer Mapping

| HiQPdf Property | IronPDF Property | Notes |
|----------------|------------------|-------|
| `Document.Header.Enabled` | Set header to `null` if disabled | Or don't set |
| `Document.Header.Height` | `HtmlHeaderFooter` CSS height | In HTML/CSS |
| `Document.Header.HtmlSource` | `HtmlHeaderFooter.HtmlFragment` | HTML string |
| `Document.Footer.Enabled` | Set footer to `null` if disabled | Or don't set |
| `Document.Footer.Height` | `HtmlHeaderFooter` CSS height | In HTML/CSS |
| `Document.Footer.HtmlSource` | `HtmlHeaderFooter.HtmlFragment` | HTML string |
| `Document.Footer.DisplayOnFirstPage` | `HtmlHeaderFooter.FirstPageNumber` | Skip pages |
| `{CrtPage}` placeholder | `{page}` | Current page |
| `{PageCount}` placeholder | `{total-pages}` | Total pages |
| `{DocTitle}` placeholder | `{title}` | Document title |
| `{DocSubject}` placeholder | N/A | Use custom HTML |
| `{DocAuthor}` placeholder | N/A | Use custom HTML |
| `{CrtPageURL}` placeholder | `{url}` | Current URL |
| `{Date}` placeholder | `{date}` | Current date |
| `{Time}` placeholder | `{time}` | Current time |

### PdfDocument Method Mapping

| HiQPdf Method | IronPDF Method | Notes |
|--------------|----------------|-------|
| `PdfDocument.FromFile(path)` | `PdfDocument.FromFile(path)` | Same |
| `document.AddDocument(other)` | `PdfDocument.Merge(doc1, doc2)` | Static method |
| `document.Save(path)` | `pdf.SaveAs(path)` | Different name |
| `document.WriteToFile(path)` | `pdf.SaveAs(path)` | Same result |
| `document.WriteToMemory()` | `pdf.BinaryData` | Property |
| `document.Pages.Count` | `pdf.PageCount` | Property |
| `document.Pages[i]` | `pdf.Pages[i]` | Same syntax |
| `document.RemovePage(index)` | `pdf.RemovePage(index)` | Same |
| `document.InsertPage(index, page)` | `pdf.InsertPdf(index, other)` | Slightly different |
| `page.Rotate(degrees)` | `pdf.RotateAllPages(rotation)` | Or per-page |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (HiQPdf):**
```csharp
using HiQPdf;

public byte[] ConvertHtmlToPdf(string html)
{
    HtmlToPdf htmlToPdfConverter = new HtmlToPdf();
    byte[] pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, null);
    return pdfBuffer;
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertHtmlToPdf(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 2: URL to PDF with Wait Time

**Before (HiQPdf):**
```csharp
using HiQPdf;

public byte[] ConvertUrlToPdf(string url)
{
    HtmlToPdf htmlToPdfConverter = new HtmlToPdf();

    // Wait for JavaScript to complete
    htmlToPdfConverter.TriggerMode = ConversionTriggeringMode.WaitTime;
    htmlToPdfConverter.WaitBeforeConvert = 3000;  // 3 seconds

    byte[] pdfBuffer = htmlToPdfConverter.ConvertUrlToMemory(url);
    return pdfBuffer;
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertUrlToPdf(string url)
{
    var renderer = new ChromePdfRenderer();

    // Wait for JavaScript to complete
    renderer.RenderingOptions.RenderDelay = 3000;  // 3 seconds

    var pdf = renderer.RenderUrlAsPdf(url);
    return pdf.BinaryData;
}
```

### Example 3: Custom Page Size and Margins

**Before (HiQPdf):**
```csharp
using HiQPdf;

public byte[] CreateLandscapePdf(string html)
{
    HtmlToPdf htmlToPdfConverter = new HtmlToPdf();

    // Page settings
    htmlToPdfConverter.Document.PageSize = PdfPageSize.Letter;
    htmlToPdfConverter.Document.PageOrientation = PdfPageOrientation.Landscape;

    // Margins in points (72 points = 1 inch)
    htmlToPdfConverter.Document.Margins.Top = 36;     // 0.5 inch
    htmlToPdfConverter.Document.Margins.Bottom = 36;
    htmlToPdfConverter.Document.Margins.Left = 72;    // 1 inch
    htmlToPdfConverter.Document.Margins.Right = 72;

    byte[] pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, null);
    return pdfBuffer;
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateLandscapePdf(string html)
{
    var renderer = new ChromePdfRenderer();

    // Page settings
    renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

    // Margins in millimeters (25.4mm = 1 inch)
    renderer.RenderingOptions.MarginTop = 12.7;       // 0.5 inch
    renderer.RenderingOptions.MarginBottom = 12.7;
    renderer.RenderingOptions.MarginLeft = 25.4;      // 1 inch
    renderer.RenderingOptions.MarginRight = 25.4;

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 4: Headers and Footers with Page Numbers

**Before (HiQPdf):**
```csharp
using HiQPdf;

public byte[] CreatePdfWithHeaderFooter(string html)
{
    HtmlToPdf htmlToPdfConverter = new HtmlToPdf();

    // Configure header
    htmlToPdfConverter.Document.Header.Enabled = true;
    htmlToPdfConverter.Document.Header.Height = 50;
    HtmlToPdfVariableElement headerElement = new HtmlToPdfVariableElement(
        "<div style='text-align:center;'>Company Report</div>", null);
    htmlToPdfConverter.Document.Header.AddElement(headerElement);

    // Configure footer with page numbers
    htmlToPdfConverter.Document.Footer.Enabled = true;
    htmlToPdfConverter.Document.Footer.Height = 30;
    htmlToPdfConverter.Document.Footer.DisplayOnFirstPage = false;
    HtmlToPdfVariableElement footerElement = new HtmlToPdfVariableElement(
        "<div style='text-align:center;'>Page {CrtPage} of {PageCount}</div>", null);
    htmlToPdfConverter.Document.Footer.AddElement(footerElement);

    byte[] pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, null);
    return pdfBuffer;
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreatePdfWithHeaderFooter(string html)
{
    var renderer = new ChromePdfRenderer();

    // Configure header
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
    {
        HtmlFragment = "<div style='text-align:center; height:50px;'>Company Report</div>",
        DrawDividerLine = false
    };

    // Configure footer with page numbers
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
    {
        HtmlFragment = "<div style='text-align:center; height:30px;'>Page {page} of {total-pages}</div>",
        DrawDividerLine = false
    };

    // Skip footer on first page
    renderer.RenderingOptions.FirstPageNumber = 0;  // First page is 0

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 5: Browser Width and Scaling

**Before (HiQPdf):**
```csharp
using HiQPdf;

public byte[] CreateWidePdf(string html)
{
    HtmlToPdf htmlToPdfConverter = new HtmlToPdf();

    // Browser viewport
    htmlToPdfConverter.BrowserWidth = 1920;
    htmlToPdfConverter.TrimToBrowserWidth = true;

    // Scaling options
    htmlToPdfConverter.Document.FitPageWidth = true;
    htmlToPdfConverter.Document.ResizePageWidth = false;

    byte[] pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, null);
    return pdfBuffer;
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateWidePdf(string html)
{
    var renderer = new ChromePdfRenderer();

    // Browser viewport
    renderer.RenderingOptions.ViewPortWidth = 1920;

    // Scaling - IronPDF auto-fits by default
    // Use FitToPaperMode for specific behavior
    renderer.RenderingOptions.FitToPaperMode = FitToPaperModes.Zoom;
    renderer.RenderingOptions.Zoom = 75;  // Scale to 75%

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 6: Merging PDF Documents

**Before (HiQPdf):**
```csharp
using HiQPdf;

public byte[] MergePdfs(string[] filePaths)
{
    PdfDocument mergedDocument = PdfDocument.FromFile(filePaths[0]);

    for (int i = 1; i < filePaths.Length; i++)
    {
        PdfDocument docToAdd = PdfDocument.FromFile(filePaths[i]);
        mergedDocument.AddDocument(docToAdd);
        docToAdd.Close();
    }

    byte[] result = mergedDocument.WriteToMemory();
    mergedDocument.Close();
    return result;
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] MergePdfs(string[] filePaths)
{
    var pdfs = filePaths.Select(path => PdfDocument.FromFile(path)).ToList();
    var merged = PdfDocument.Merge(pdfs);

    // Cleanup source documents
    foreach (var pdf in pdfs)
        pdf.Dispose();

    return merged.BinaryData;
}
```

### Example 7: Base URL for Relative Resources

**Before (HiQPdf):**
```csharp
using HiQPdf;

public byte[] ConvertWithBaseUrl(string html, string baseUrl)
{
    HtmlToPdf htmlToPdfConverter = new HtmlToPdf();

    // baseUrl resolves relative paths for images, CSS, etc.
    byte[] pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, baseUrl);
    return pdfBuffer;
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertWithBaseUrl(string html, string baseUrl)
{
    var renderer = new ChromePdfRenderer();

    // Pass baseUrl as second parameter
    var pdf = renderer.RenderHtmlAsPdf(html, baseUrl);
    return pdf.BinaryData;
}
```

### Example 8: Manual Trigger (Wait for JS Condition)

**Before (HiQPdf):**
```csharp
using HiQPdf;

public byte[] ConvertWithManualTrigger(string html)
{
    HtmlToPdf htmlToPdfConverter = new HtmlToPdf();

    // Wait for manual trigger
    htmlToPdfConverter.TriggerMode = ConversionTriggeringMode.Manual;
    // JavaScript calls: hiqPdfConverter.startConversion()

    byte[] pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, null);
    return pdfBuffer;
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertWithManualTrigger(string html)
{
    var renderer = new ChromePdfRenderer();

    // Wait for JavaScript condition
    renderer.RenderingOptions.WaitFor.JavaScript("window.readyToPrint === true");

    // Or wait for specific element to exist
    // renderer.RenderingOptions.WaitFor.HtmlElementById("content-loaded");

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 9: Page Extraction

**Before (HiQPdf):**
```csharp
using HiQPdf;

public byte[] ExtractPages(string pdfPath, int startPage, int endPage)
{
    PdfDocument document = PdfDocument.FromFile(pdfPath);
    PdfDocument newDocument = new PdfDocument();

    for (int i = startPage; i <= endPage; i++)
    {
        PdfPage page = document.Pages[i];
        newDocument.AddPage(page);
    }

    byte[] result = newDocument.WriteToMemory();
    document.Close();
    newDocument.Close();
    return result;
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ExtractPages(string pdfPath, int startPage, int endPage)
{
    var document = PdfDocument.FromFile(pdfPath);

    // Create array of page indices to copy (0-indexed in IronPDF)
    var pageIndices = Enumerable.Range(startPage, endPage - startPage + 1).ToArray();

    var newDocument = document.CopyPages(pageIndices);
    return newDocument.BinaryData;
}
```

### Example 10: Saving to File

**Before (HiQPdf):**
```csharp
using HiQPdf;

public void SavePdfToFile(string html, string outputPath)
{
    HtmlToPdf htmlToPdfConverter = new HtmlToPdf();

    // Option 1: Convert directly to file
    htmlToPdfConverter.ConvertHtmlToFile(html, null, outputPath);

    // Option 2: Convert to memory then save
    byte[] pdfBuffer = htmlToPdfConverter.ConvertHtmlToMemory(html, null);
    System.IO.File.WriteAllBytes(outputPath, pdfBuffer);

    // Option 3: Via PdfDocument
    PdfDocument document = htmlToPdfConverter.ConvertHtmlToPdfDocument(html, null);
    document.WriteToFile(outputPath);
    document.Close();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void SavePdfToFile(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    // Render and save in one chain
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);

    // Or get bytes if needed elsewhere
    // byte[] bytes = pdf.BinaryData;
    // File.WriteAllBytes(outputPath, bytes);
}
```

---

## Advanced Scenarios

### ASP.NET MVC Migration

**Before (HiQPdf in MVC):**
```csharp
using HiQPdf;
using System.Web.Mvc;

public class ReportController : Controller
{
    public ActionResult GeneratePdf()
    {
        HtmlToPdf converter = new HtmlToPdf();
        converter.SerialNumber = ConfigurationManager.AppSettings["HiQPdfLicense"];

        string html = RenderViewToString("ReportView", model);
        byte[] pdfData = converter.ConvertHtmlToMemory(html, Request.Url.GetLeftPart(UriPartial.Authority));

        return File(pdfData, "application/pdf", "report.pdf");
    }
}
```

**After (IronPDF in MVC):**
```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;

public class ReportController : Controller
{
    public IActionResult GeneratePdf()
    {
        var renderer = new ChromePdfRenderer();

        string html = RenderViewToString("ReportView", model);
        var pdf = renderer.RenderHtmlAsPdf(html, $"{Request.Scheme}://{Request.Host}");

        return File(pdf.BinaryData, "application/pdf", "report.pdf");
    }
}

// Startup.cs / Program.cs
IronPdf.License.LicenseKey = builder.Configuration["IronPdf:LicenseKey"];
```

### Unit Conversion Helper

HiQPdf uses points (72 per inch), IronPDF uses millimeters:

```csharp
public static class UnitConverter
{
    // 1 inch = 72 points = 25.4mm
    public static double PointsToMillimeters(double points) => points * 25.4 / 72;
    public static double MillimetersToPoints(double mm) => mm * 72 / 25.4;

    public static double InchesToMillimeters(double inches) => inches * 25.4;
    public static double InchesToPoints(double inches) => inches * 72;

    // Convert HiQPdf margin (points) to IronPDF (mm)
    public static double HiQPdfMarginToIronPdf(double hiQPdfPoints)
    {
        return PointsToMillimeters(hiQPdfPoints);
    }
}

// Usage:
// HiQPdf: htmlToPdfConverter.Document.Margins.Top = 72; // 1 inch in points
// IronPDF: renderer.RenderingOptions.MarginTop = 25.4;  // 1 inch in mm
```

### Handling HtmlToPdfVariableElement

**Before (HiQPdf custom elements in header/footer):**
```csharp
using HiQPdf;

public void ConfigureComplexHeader(HtmlToPdf converter)
{
    converter.Document.Header.Enabled = true;
    converter.Document.Header.Height = 100;

    // Add text element
    PdfText titleText = new PdfText(10, 10, "Report Title", new PdfFont("Arial", 18, true, false));
    converter.Document.Header.AddElement(titleText);

    // Add HTML element
    HtmlToPdfVariableElement htmlElement = new HtmlToPdfVariableElement(
        0, 40, converter.Document.Header.Width, 60,
        "<div style='border-bottom:1px solid #ccc;'>Page {CrtPage} of {PageCount}</div>",
        null);
    converter.Document.Header.AddElement(htmlElement);

    // Add image
    PdfImage logoImage = new PdfImage(converter.Document.Header.Width - 100, 10, 80, 30, "logo.png");
    converter.Document.Header.AddElement(logoImage);
}
```

**After (IronPDF - use HTML for everything):**
```csharp
using IronPdf;

public void ConfigureComplexHeader(ChromePdfRenderer renderer)
{
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
    {
        HtmlFragment = @"
            <div style='width:100%; height:100px; position:relative;'>
                <span style='font-family:Arial; font-size:18px; font-weight:bold;
                       position:absolute; left:10px; top:10px;'>Report Title</span>
                <img src='logo.png' style='position:absolute; right:10px; top:10px;
                       width:80px; height:30px;'>
                <div style='position:absolute; bottom:0; width:100%;
                       border-bottom:1px solid #ccc; text-align:center;'>
                    Page {page} of {total-pages}
                </div>
            </div>",
        MaxHeight = 100,  // Same as Height in HiQPdf
        DrawDividerLine = false
    };
}
```

---

## Performance Comparison

### Rendering Speed

| Operation | HiQPdf | IronPDF | Notes |
|-----------|--------|---------|-------|
| Simple HTML | 100-300ms | 150-400ms | Similar |
| Complex HTML | 500-1500ms | 300-1000ms | IronPDF often faster |
| JavaScript-heavy | 2-5s | 1-3s | Chromium handles better |
| Large documents | Varies | Consistent | IronPDF more predictable |

### Modern JavaScript Support

| Framework | HiQPdf | IronPDF |
|-----------|--------|---------|
| React | Partial | Full |
| Angular | Limited | Full |
| Vue.js | Limited | Full |
| Chart.js | Partial | Full |
| D3.js | Limited | Full |

---

## Troubleshooting

### Issue 1: "3-page limit exceeded" or watermarks appearing

**Cause**: Using HiQPdf.Free package or no license

**Solution**: IronPDF offers 30-day full trial. Set license key:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Issue 2: "Margins look different"

**Cause**: HiQPdf uses points, IronPDF uses millimeters

**Solution**: Convert units:
```csharp
// HiQPdf: 72 points = 1 inch
// IronPDF: 25.4mm = 1 inch
double ironPdfMargin = hiQPdfPoints * 25.4 / 72;
```

### Issue 3: "Page numbers show {CrtPage} literally"

**Cause**: Different placeholder syntax

**Solution**: Use IronPDF placeholders:
```csharp
// HiQPdf: {CrtPage} of {PageCount}
// IronPDF: {page} of {total-pages}
```

### Issue 4: "ConvertHtmlToMemory not found"

**Cause**: Method doesn't exist in IronPDF

**Solution**: Use `RenderHtmlAsPdf()`:
```csharp
var pdf = renderer.RenderHtmlAsPdf(html);
byte[] bytes = pdf.BinaryData;  // Get byte array
```

### Issue 5: "TriggerMode not working"

**Cause**: Different API for wait conditions

**Solution**: Use `RenderingOptions.RenderDelay` or `WaitFor`:
```csharp
// Fixed delay
renderer.RenderingOptions.RenderDelay = 3000;

// Wait for condition
renderer.RenderingOptions.WaitFor.JavaScript("window.loaded === true");
```

### Issue 6: "Header/Footer elements not positioning correctly"

**Cause**: HiQPdf uses x,y coordinates; IronPDF uses HTML/CSS

**Solution**: Use CSS positioning in HtmlHeaderFooter:
```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='position:absolute; right:10px;'>Logo here</div>"
};
```

### Issue 7: "BrowserWidth setting not taking effect"

**Cause**: Property renamed in IronPDF

**Solution**: Use `ViewPortWidth`:
```csharp
// HiQPdf: htmlToPdf.BrowserWidth = 1024;
// IronPDF:
renderer.RenderingOptions.ViewPortWidth = 1024;
```

### Issue 8: "SerialNumber property not found"

**Cause**: Different license configuration

**Solution**: Set license globally:
```csharp
// At application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## Migration Checklist

```markdown
## Migration Checklist

### Pre-Migration

- [ ] **Inventory all library usages in codebase**
  ```bash
  grep -r "using HiQPdf" --include="*.cs" .
  grep -r "PdfDocument\|HtmlToPdf" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  ```csharp
  // Find patterns like:
  var pdf = new HtmlToPdf();
  pdf.Document.PageSize = PdfPageSize.A4;
  pdf.Document.Margins = new PdfMargins(10, 10, 10, 10);
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package HiQPdf
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
  // Before (HiQPdf)
  var pdf = new HtmlToPdf();
  pdf.ConvertHtmlToFile(html, "output.pdf");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdfDocument = renderer.RenderHtmlAsPdf(html);
  pdfDocument.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering for accurate HTML/CSS support.

- [ ] **Convert URL rendering**
  ```csharp
  // Before (HiQPdf)
  pdf.ConvertUrlToFile("https://example.com", "output.pdf");

  // After (IronPDF)
  var pdfDocument = renderer.RenderUrlAsPdf("https://example.com");
  pdfDocument.SaveAs("output.pdf");
  ```
  **Why:** Direct URL rendering with full JavaScript support.

- [ ] **Update page settings**
  ```csharp
  // Before (HiQPdf)
  pdf.Document.PageSize = PdfPageSize.A4;
  pdf.Document.PageOrientation = PdfPageOrientation.Landscape;
  pdf.Document.Margins = new PdfMargins(20, 20, 20, 20);

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  renderer.RenderingOptions.MarginLeft = 20;
  renderer.RenderingOptions.MarginRight = 20;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Convert header/footer configuration**
  ```csharp
  // Before (HiQPdf)
  pdf.Document.Header.Text = "Page [page] of [total]";
  pdf.Document.Footer.Text = "Document Title";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>{html-title}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders: {page}, {total-pages}, {date}, {time}, {html-title}.

- [ ] **Enable JavaScript if needed**
  ```csharp
  // Before (HiQPdf)
  pdf.Document.JavaScriptEnabled = true;
  pdf.Document.JavaScriptDelay = 2000;

  // After (IronPDF)
  renderer.RenderingOptions.EnableJavaScript = true;
  renderer.RenderingOptions.WaitFor.JavaScript(2000);
  ```
  **Why:** IronPDF's Chromium engine provides reliable JavaScript execution with configurable wait times.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Test JavaScript-heavy pages**
  **Why:** Dynamic content should now render more reliably with modern Chromium.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  // Merge PDFs
  var merged = PdfDocument.Merge(pdf1, pdf2);

  // Password protection
  pdfDocument.SecuritySettings.UserPassword = "secret";

  // Watermarks
  pdfDocument.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");

  // Digital signatures
  var signature = new PdfSignature("cert.pfx", "password");
  pdfDocument.Sign(signature);
  ```
  **Why:** IronPDF provides many features that may not have been available in the old library.
```
## Migration Checklist

### Pre-Migration

- [ ] **Inventory all HiQPdf usage locations**
  ```bash
  grep -r "using HiQPdf" --include="*.cs" .
  grep -r "HtmlToPdf\|ConvertHtmlToMemory\|ConvertUrlToMemory" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current page sizes, margins, and settings**
  ```csharp
  // Find patterns like:
  var pdf = new HtmlToPdf();
  pdf.Document.PageSize = PdfPageSize.A4;
  pdf.Document.Margins = new PdfMargins(10, 10, 10, 10);
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Identify header/footer configurations**
  ```csharp
  // Find patterns like:
  pdf.Document.Header = new PdfHeaderFooter();
  pdf.Document.Footer = new PdfHeaderFooter();
  ```
  **Why:** IronPDF uses HtmlHeaderFooter with HTML placeholders, requiring a different setup.

- [ ] **Note any TriggerMode settings**
  ```csharp
  // Find patterns like:
  pdf.TriggerMode = ConversionTriggerMode.Auto;
  ```
  **Why:** IronPDF uses RenderDelay or WaitFor for similar functionality, requiring adjustments.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Test IronPDF in development environment**
  **Why:** Ensure compatibility and identify any potential issues early in the migration process.

### Code Migration

- [ ] **Remove HiQPdf NuGet packages (all variants)**
  ```bash
  dotnet remove package HiQPdf
  dotnet remove package HiQPdf.NetCore
  dotnet remove package HiQPdf.Client
  ```
  **Why:** Clean removal of old packages to prevent conflicts.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add the new library to enable PDF generation with IronPDF.

- [ ] **Update namespace imports**
  ```csharp
  // Before (HiQPdf)
  using HiQPdf;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure code references the correct library.

- [ ] **Replace `HtmlToPdf` with `ChromePdfRenderer`**
  ```csharp
  // Before (HiQPdf)
  var pdf = new HtmlToPdf();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses ChromePdfRenderer for modern HTML/CSS rendering.

- [ ] **Convert `ConvertHtmlToMemory()` to `RenderHtmlAsPdf()`**
  ```csharp
  // Before (HiQPdf)
  var pdfBytes = pdf.ConvertHtmlToMemory(html);

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(html);
  var pdfBytes = pdf.BinaryData;
  ```
  **Why:** IronPDF provides a direct method for rendering HTML to PDF.

- [ ] **Convert `ConvertUrlToMemory()` to `RenderUrlAsPdf()`**
  ```csharp
  // Before (HiQPdf)
  var pdfBytes = pdf.ConvertUrlToMemory(url);

  // After (IronPDF)
  var pdf = renderer.RenderUrlAsPdf(url);
  var pdfBytes = pdf.BinaryData;
  ```
  **Why:** IronPDF supports direct URL rendering with full JavaScript support.

- [ ] **Update margin values (points → millimeters)**
  ```csharp
  // Before (HiQPdf)
  pdf.Document.Margins = new PdfMargins(10, 10, 10, 10);

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 10;
  renderer.RenderingOptions.MarginBottom = 10;
  renderer.RenderingOptions.MarginLeft = 10;
  renderer.RenderingOptions.MarginRight = 10;
  ```
  **Why:** IronPDF uses millimeters for margin settings.

- [ ] **Update header/footer placeholders**
  ```csharp
  // Before (HiQPdf)
  pdf.Document.Header.Text = "Page [page] of [total]";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders.

- [ ] **Replace `HtmlToPdfVariableElement` with `HtmlHeaderFooter`**
  ```csharp
  // Before (HiQPdf)
  var header = new HtmlToPdfVariableElement("Header text");

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div>Header text</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HtmlHeaderFooter for dynamic content in headers/footers.

- [ ] **Update `TriggerMode` to `RenderDelay` or `WaitFor`**
  ```csharp
  // Before (HiQPdf)
  pdf.TriggerMode = ConversionTriggerMode.Manual;

  // After (IronPDF)
  renderer.RenderingOptions.WaitFor.JavaScript(2000);
  ```
  **Why:** IronPDF provides configurable wait times for JavaScript execution.

- [ ] **Add license key initialization at startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Configuration Migration

- [ ] **Remove HiQPdf license/serial number from config**
  **Why:** Clean up configuration files to remove unused settings.

- [ ] **Add IronPDF license key to configuration**
  **Why:** Ensure the application is properly licensed for production use.

- [ ] **Update any deployment scripts**
  **Why:** Ensure deployment scripts reflect changes in package dependencies and configuration.

### Testing

- [ ] **Test HTML to PDF conversion**
  **Why:** Verify that HTML content is rendered correctly with IronPDF.

- [ ] **Test URL to PDF conversion**
  **Why:** Ensure URLs are converted to PDFs accurately with full JavaScript support.

- [ ] **Verify page sizes and margins**
  **Why:** Confirm that page layouts remain consistent after migration.

- [ ] **Verify header/footer rendering**
  **Why:** Ensure headers and footers appear as expected with new HTML configuration.

- [ ] **Verify page number placeholders**
  **Why:** Check that page numbers are displayed correctly using IronPDF's placeholders.

- [ ] **Test JavaScript-heavy pages**
  **Why:** Ensure dynamic content is rendered accurately with IronPDF's modern engine.

- [ ] **Test with production HTML templates**
  **Why:** Validate that all production templates render correctly with IronPDF.

- [ ] **Performance test under load**
  **Why:** Ensure the application performs well under expected usage conditions.

### Post-Migration

- [ ] **Remove all HiQPdf references from documentation**
  **Why:** Update documentation to reflect the new library usage.

- [ ] **Update any CI/CD configurations**
  **Why:** Ensure continuous integration and deployment pipelines are using the correct packages.

- [ ] **Monitor for any rendering differences**
  **Why:** Identify and address any discrepancies in PDF output quality.

- [ ] **Verify license is working in production**
  **Why:** Confirm that the application is properly licensed and functioning in a live environment.
```
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [Headers and Footers Guide](https://ironpdf.com/how-to/headers-and-footers/)
- [Migrating from HiQPdf to IronPDF](https://ironpdf.com/blog/migration-guides/migrate-from-hiqpdf-to-ironpdf/)

---

## Summary

Migrating from HiQPdf to IronPDF provides several advantages:

1. **Modern Rendering**: Chromium engine vs older WebKit—better JavaScript/CSS support
2. **Simple Licensing**: Clear trial period vs 3-page limit with watermarks
3. **Unified Package**: Single NuGet package vs multiple platform-specific packages
4. **Cleaner API**: Fluent methods vs complex property chains
5. **Better Documentation**: Comprehensive guides vs fragmented docs

Key migration steps:

1. **Package Change**: Replace all HiQPdf variants with single IronPdf package
2. **Class Replacement**: `HtmlToPdf` → `ChromePdfRenderer`
3. **Method Replacement**: `ConvertHtmlToMemory()` → `RenderHtmlAsPdf().BinaryData`
4. **Unit Conversion**: Points → Millimeters for margins
5. **Placeholder Update**: `{CrtPage}` → `{page}`, `{PageCount}` → `{total-pages}`
6. **License Configuration**: Set globally at startup instead of per-converter

The migration results in cleaner, more maintainable code with better support for modern web technologies.
