# How Do I Migrate from ABCpdf to IronPDF in C#?

> **Migration Complexity:** Medium
> **Estimated Time:** 2-4 hours for typical projects
> **Last Updated:** December 2024

## Table of Contents

- [Why Migrate to IronPDF](#why-migrate-to-ironpdf)
- [Before You Start](#before-you-start)
- [Quick Start (5 Minutes)](#quick-start-5-minutes)
- [Complete API Reference](#complete-api-reference)
- [Code Migration Examples](#code-migration-examples)
- [Advanced Scenarios](#advanced-scenarios)
- [Performance Considerations](#performance-considerations)
- [Troubleshooting Guide](#troubleshooting-guide)
- [Migration Checklist](#migration-checklist)
- [Additional Resources](#additional-resources)

---

## Why Migrate to IronPDF

ABCpdf from WebSupergoo has been a capable PDF library for .NET developers, but several factors make IronPDF an attractive alternative for modern development teams.

### The Case for Leaving ABCpdf

**Licensing Complexity**: ABCpdf uses a tiered licensing model that can be confusing to navigate, as detailed in the [migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-abcpdf-to-ironpdf/). Pricing starts at $349 but escalates based on features, server deployments, and use cases. Many developers report the licensing maze as a significant administrative burden.

**Windows-First Architecture**: While ABCpdf has added cross-platform support, its historical Windows-centric design occasionally surfaces in workflows. Developers targeting Linux containers or macOS development environments may encounter friction.

**Documentation Style**: ABCpdf's documentation, while comprehensive, follows an older style that can feel dated compared to modern API documentation standards. New users often struggle to find the exact examples they need.

**Engine Configuration Overhead**: ABCpdf requires explicit engine selection (Gecko, Chrome, etc.) and manual resource management with `Clear()` calls. This adds boilerplate code to every PDF operation.

### What IronPDF Provides

| ABCpdf Limitation | IronPDF Solution |
|-------------------|------------------|
| Complex tiered licensing | Simple, transparent pricing |
| Requires engine selection | Chrome engine by default |
| Manual `Clear()` cleanup | IDisposable with `using` statements |
| Windows-first design | Native cross-platform support |
| Dated documentation | Modern docs with extensive examples |
| Registry-based licensing | Simple code-based license key |

---

## Before You Start

### Prerequisites

- .NET Framework 4.6.2+ or .NET Core 3.1+ / .NET 5-9
- Visual Studio 2019+ or JetBrains Rider
- NuGet Package Manager access
- IronPDF license key (free trial available at [ironpdf.com](https://ironpdf.com))

### Find All ABCpdf References

Run this command in your solution directory to locate all files using ABCpdf:

```bash
grep -r "using WebSupergoo" --include="*.cs" .
grep -r "ABCpdf" --include="*.csproj" .
```

### Breaking Changes to Anticipate

| Category | ABCpdf Behavior | IronPDF Behavior | Migration Action |
|----------|-----------------|------------------|------------------|
| Object Model | `Doc` class is central | `ChromePdfRenderer` + `PdfDocument` | Separate rendering from document |
| Resource Cleanup | Manual `doc.Clear()` | IDisposable pattern | Use `using` statements |
| Engine Selection | `doc.HtmlOptions.Engine = EngineType.Chrome` | Built-in Chrome | Remove engine config |
| Page Coordinates | Point-based with `doc.Rect` | CSS-based margins | Use CSS or RenderingOptions |
| Save to Memory | `doc.GetData()` | `pdf.BinaryData` | Property access |
| Multi-page HTML | Loop with `doc.AddImageHtml()` | Automatic pagination | Simplify code |

---

## Quick Start (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove ABCpdf
dotnet remove package ABCpdf

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Set Your License Key (Program.cs or Startup)

```csharp
// Add this at application startup, before any IronPDF operations
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 3: Global Find & Replace

| Find | Replace With |
|------|--------------|
| `using WebSupergoo.ABCpdf13;` | `using IronPdf;` |
| `using WebSupergoo.ABCpdf13.Objects;` | `using IronPdf;` |
| `using WebSupergoo.ABCpdf12;` | `using IronPdf;` |
| `using WebSupergoo.ABCpdf11;` | `using IronPdf;` |

### Step 4: Verify Basic Operation

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;

Doc doc = new Doc();
doc.HtmlOptions.Engine = EngineType.Chrome;
doc.AddImageHtml("<h1>Hello World</h1>");
doc.Save("output.pdf");
doc.Clear();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

---

## Complete API Reference

### Namespace Mapping

| ABCpdf Namespace | IronPDF Namespace | Purpose |
|------------------|-------------------|---------|
| `WebSupergoo.ABCpdf13` | `IronPdf` | Core PDF functionality |
| `WebSupergoo.ABCpdf13.Objects` | `IronPdf` | PDF objects and types |
| `WebSupergoo.ABCpdf13.Atoms` | `IronPdf` | Low-level PDF atoms |
| `WebSupergoo.ABCpdf12` | `IronPdf` | Version 12 compatibility |
| `WebSupergoo.ABCpdf11` | `IronPdf` | Version 11 compatibility |
| `WebSupergoo.ABCpdf10` | `IronPdf` | Version 10 compatibility |

### Document Creation Methods

| ABCpdf Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `new Doc()` | `new ChromePdfRenderer()` | Renderer creates PDFs |
| `doc.AddImageUrl(url)` | `renderer.RenderUrlAsPdf(url)` | URL to PDF |
| `doc.AddImageHtml(html)` | `renderer.RenderHtmlAsPdf(html)` | HTML string to PDF |
| `doc.AddImageFile(path)` | `renderer.RenderHtmlFileAsPdf(path)` | HTML file to PDF |
| `doc.Read(path)` | `PdfDocument.FromFile(path)` | Load existing PDF |
| `doc.Save(path)` | `pdf.SaveAs(path)` | Save to file |
| `doc.GetData()` | `pdf.BinaryData` | Get as byte array |
| `doc.Clear()` | Use `using` statement | Automatic disposal |

### Page Manipulation Methods

| ABCpdf Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `doc.AddPage()` | Automatic from HTML | Pages created automatically |
| `doc.Page = n` | `pdf.Pages[n-1]` | ABCpdf is 1-indexed, IronPDF is 0-indexed |
| `doc.PageCount` | `pdf.PageCount` | Same usage |
| `doc.Delete(pageId)` | `pdf.RemovePages(index)` | Remove pages |
| `doc.Append(otherDoc)` | `PdfDocument.Merge(pdf1, pdf2)` | Static merge method |
| `doc.RemapPages(int[])` | `pdf.CopyPages(indices)` | Reorder pages |
| `doc.Rect.String = "A4"` | `RenderingOptions.PaperSize` | Page size |
| `doc.Rect.Inset(x, y)` | `RenderingOptions.MarginTop/Bottom/Left/Right` | Margins |

### Content & Text Methods

| ABCpdf Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `doc.AddText(text)` | Use HTML/CSS | IronPDF uses HTML approach |
| `doc.AddImage(image)` | `ImageStamper` or HTML `<img>` | Stamping or HTML |
| `doc.AddBookmark(text, id)` | `pdf.BookMarks.AddBookMarkAtStart()` | Bookmark API |
| `doc.GetText(separators)` | `pdf.ExtractAllText()` | Extract text |
| `doc.FitText(text)` | CSS styling | Use CSS for text fitting |
| `doc.MeasureText(text)` | Not needed | CSS handles layout |

### Watermark & Stamping Methods

| ABCpdf Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `doc.AddText()` on each page | `pdf.ApplyWatermark(html)` | Built-in watermark |
| `doc.AddImage()` on each page | `new ImageStamper(image)` | Image stamping |
| `doc.Rect` positioning | `Stamper` alignment properties | Position control |
| Manual page loop | `pdf.ApplyStamp(stamper)` | Applies to all pages |

### Security & Encryption Methods

| ABCpdf Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `doc.Encryption.Type` | `pdf.SecuritySettings` | Security configuration |
| `doc.Encryption.Password` | `pdf.SecuritySettings.OwnerPassword` | Owner password |
| `doc.Encryption.CanPrint` | `pdf.SecuritySettings.AllowUserPrinting` | Print permission |
| `doc.Encryption.CanCopy` | `pdf.SecuritySettings.AllowUserCopyPasteContent` | Copy permission |
| `doc.Encryption.CanEdit` | `pdf.SecuritySettings.AllowUserEdits` | Edit permission |
| `doc.SetInfo("Title", value)` | `pdf.MetaData.Title` | Document metadata |
| `doc.SetInfo("Author", value)` | `pdf.MetaData.Author` | Author metadata |

### Configuration Options Mapping

| ABCpdf Setting | IronPDF Equivalent | Default |
|----------------|-------------------|---------|
| `doc.HtmlOptions.Engine = EngineType.Chrome` | Built-in Chrome | Chrome |
| `doc.HtmlOptions.PageCacheEnabled` | Not needed | N/A |
| `doc.Rect.String = "A4"` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | A4 |
| `doc.Rect.String = "Letter"` | `RenderingOptions.PaperSize = PdfPaperSize.Letter` | Letter |
| `doc.HtmlOptions.BrowserWidth` | `RenderingOptions.ViewPortWidth` | 1280 |
| `doc.HtmlOptions.Timeout` | `RenderingOptions.Timeout` | 60000ms |
| `doc.HtmlOptions.UseScript` | `RenderingOptions.EnableJavaScript` | true |
| `doc.HtmlOptions.RetryCount` | Handle with try/catch | N/A |

---

## Code Migration Examples

### Example 1: HTML String to PDF

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;

public byte[] CreatePdfFromHtml(string html)
{
    Doc doc = new Doc();
    doc.HtmlOptions.Engine = EngineType.Chrome;
    doc.Rect.Inset(20, 20);
    doc.AddImageHtml(html);
    byte[] data = doc.GetData();
    doc.Clear();
    return data;
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreatePdfFromHtml(string html)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.MarginTop = 20;
    renderer.RenderingOptions.MarginBottom = 20;
    renderer.RenderingOptions.MarginLeft = 20;
    renderer.RenderingOptions.MarginRight = 20;

    using var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 2: URL to PDF

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;

public void ConvertUrlToPdf(string url, string outputPath)
{
    Doc doc = new Doc();
    doc.HtmlOptions.Engine = EngineType.Chrome;
    doc.HtmlOptions.PageCacheEnabled = false;
    doc.AddImageUrl(url);
    doc.Save(outputPath);
    doc.Clear();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void ConvertUrlToPdf(string url, string outputPath)
{
    var renderer = new ChromePdfRenderer();
    using var pdf = renderer.RenderUrlAsPdf(url);
    pdf.SaveAs(outputPath);
}
```

### Example 3: Merge Multiple PDFs

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;

public void MergePdfs(string[] inputFiles, string outputPath)
{
    Doc doc = new Doc();
    doc.Read(inputFiles[0]);

    for (int i = 1; i < inputFiles.Length; i++)
    {
        Doc tempDoc = new Doc();
        tempDoc.Read(inputFiles[i]);
        doc.Append(tempDoc);
        tempDoc.Clear();
    }

    doc.Save(outputPath);
    doc.Clear();
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Linq;

public void MergePdfs(string[] inputFiles, string outputPath)
{
    var pdfs = inputFiles.Select(PdfDocument.FromFile).ToList();
    using var merged = PdfDocument.Merge(pdfs);
    merged.SaveAs(outputPath);

    foreach (var pdf in pdfs)
        pdf.Dispose();
}
```

### Example 4: Add Headers and Footers

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;

public void AddHeaderFooter(string inputPath, string outputPath)
{
    Doc doc = new Doc();
    doc.Read(inputPath);

    for (int i = 1; i <= doc.PageCount; i++)
    {
        doc.Page = i;
        doc.Rect.SetRect(0, 780, 612, 20);
        doc.AddText($"Page {i} of {doc.PageCount}");
    }

    doc.Save(outputPath);
    doc.Clear();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreatePdfWithHeaderFooter(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.TextHeader = new TextHeaderFooter
    {
        CenterText = "My Document",
        FontSize = 12
    };

    renderer.RenderingOptions.TextFooter = new TextHeaderFooter
    {
        CenterText = "Page {page} of {total-pages}",
        FontSize = 10
    };

    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 5: Password Protection

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;

public void ProtectPdf(string inputPath, string outputPath, string password)
{
    Doc doc = new Doc();
    doc.Read(inputPath);

    doc.Encryption.Type = EncryptionType.AES128;
    doc.Encryption.Password = password;
    doc.Encryption.CanPrint = true;
    doc.Encryption.CanCopy = false;

    doc.Save(outputPath);
    doc.Clear();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void ProtectPdf(string inputPath, string outputPath, string password)
{
    using var pdf = PdfDocument.FromFile(inputPath);

    pdf.SecuritySettings.OwnerPassword = password;
    pdf.SecuritySettings.UserPassword = password;
    pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;

    pdf.SaveAs(outputPath);
}
```

### Example 6: Extract Text from PDF

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;

public string ExtractText(string pdfPath)
{
    Doc doc = new Doc();
    doc.Read(pdfPath);

    string text = "";
    for (int i = 1; i <= doc.PageCount; i++)
    {
        doc.Page = i;
        text += doc.GetText("Text") + "\n";
    }

    doc.Clear();
    return text;
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public string ExtractText(string pdfPath)
{
    using var pdf = PdfDocument.FromFile(pdfPath);
    return pdf.ExtractAllText();
}
```

### Example 7: Add Watermark

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;

public void AddWatermark(string inputPath, string outputPath, string watermarkText)
{
    Doc doc = new Doc();
    doc.Read(inputPath);

    for (int i = 1; i <= doc.PageCount; i++)
    {
        doc.Page = i;
        doc.Color.String = "200 200 200";
        doc.Font = doc.AddFont("Arial");
        doc.FontSize = 72;
        doc.Rect.SetRect(100, 300, 400, 100);
        doc.AddText(watermarkText);
    }

    doc.Save(outputPath);
    doc.Clear();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void AddWatermark(string inputPath, string outputPath, string watermarkText)
{
    using var pdf = PdfDocument.FromFile(inputPath);

    pdf.ApplyWatermark($"<h1 style='color:lightgray;font-size:72px;'>{watermarkText}</h1>",
        rotation: 45,
        opacity: 50);

    pdf.SaveAs(outputPath);
}
```

### Example 8: Custom Page Size

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;

public void CreateCustomSizePdf(string html, string outputPath)
{
    Doc doc = new Doc();
    doc.HtmlOptions.Engine = EngineType.Chrome;
    doc.MediaBox.String = "0 0 400 600"; // Custom size in points
    doc.AddImageHtml(html);
    doc.Save(outputPath);
    doc.Clear();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreateCustomSizePdf(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.SetCustomPaperSizeInInches(5.5, 8.5);
    // Or use: renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(140, 216);

    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 9: HTML File with External CSS

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;

public void ConvertHtmlFile(string htmlPath, string outputPath)
{
    Doc doc = new Doc();
    doc.HtmlOptions.Engine = EngineType.Chrome;
    doc.HtmlOptions.AddLinks = true;

    int id = doc.AddImageUrl("file://" + htmlPath);
    while (doc.Chainable(id))
    {
        doc.Page = doc.AddPage();
        id = doc.AddImageToChain(id);
    }

    doc.Save(outputPath);
    doc.Clear();
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.IO;

public void ConvertHtmlFile(string htmlPath, string outputPath)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

    // BaseUrl helps resolve relative CSS/image paths
    var baseUri = new Uri(Path.GetDirectoryName(htmlPath) + "/");

    using var pdf = renderer.RenderHtmlFileAsPdf(htmlPath);
    pdf.SaveAs(outputPath);
}
```

### Example 10: Batch Processing Multiple Files

**Before (ABCpdf):**
```csharp
using WebSupergoo.ABCpdf13;

public void BatchConvert(string[] htmlFiles, string outputDir)
{
    foreach (var htmlFile in htmlFiles)
    {
        Doc doc = new Doc();
        doc.HtmlOptions.Engine = EngineType.Chrome;
        doc.AddImageUrl("file://" + htmlFile);

        string outputPath = Path.Combine(outputDir,
            Path.GetFileNameWithoutExtension(htmlFile) + ".pdf");
        doc.Save(outputPath);
        doc.Clear();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.IO;

public void BatchConvert(string[] htmlFiles, string outputDir)
{
    var renderer = new ChromePdfRenderer();

    foreach (var htmlFile in htmlFiles)
    {
        using var pdf = renderer.RenderHtmlFileAsPdf(htmlFile);

        string outputPath = Path.Combine(outputDir,
            Path.GetFileNameWithoutExtension(htmlFile) + ".pdf");
        pdf.SaveAs(outputPath);
    }
}
```

---

## Advanced Scenarios

### Scenario 1: ASP.NET Core Web Application

**ABCpdf Pattern:**
```csharp
[HttpPost]
public IActionResult GeneratePdf([FromBody] ReportRequest request)
{
    Doc doc = new Doc();
    doc.HtmlOptions.Engine = EngineType.Chrome;
    doc.AddImageHtml(request.Html);
    byte[] pdfBytes = doc.GetData();
    doc.Clear();

    return File(pdfBytes, "application/pdf", "report.pdf");
}
```

**IronPDF Pattern:**
```csharp
[HttpPost]
public IActionResult GeneratePdf([FromBody] ReportRequest request)
{
    var renderer = new ChromePdfRenderer();
    using var pdf = renderer.RenderHtmlAsPdf(request.Html);

    return File(pdf.BinaryData, "application/pdf", "report.pdf");
}
```

### Scenario 2: Async PDF Generation

**ABCpdf** doesn't have native async support. **IronPDF** does:

```csharp
using IronPdf;

public async Task<byte[]> GeneratePdfAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

### Scenario 3: Dependency Injection Setup

**Register IronPDF in Startup.cs / Program.cs:**
```csharp
// Program.cs (.NET 6+)
builder.Services.AddSingleton<ChromePdfRenderer>();

// Or create a service wrapper
public interface IPdfService
{
    Task<byte[]> GeneratePdfAsync(string html);
}

public class IronPdfService : IPdfService
{
    private readonly ChromePdfRenderer _renderer;

    public IronPdfService()
    {
        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    }

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        using var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}

// Register: builder.Services.AddSingleton<IPdfService, IronPdfService>();
```

### Scenario 4: Error Handling Pattern

**ABCpdf Error Handling:**
```csharp
try
{
    Doc doc = new Doc();
    doc.AddImageUrl(url);
    doc.Save(path);
    doc.Clear();
}
catch (Exception ex)
{
    // Generic exception handling
}
```

**IronPDF Error Handling:**
```csharp
try
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.Timeout = 30000; // 30 seconds

    using var pdf = renderer.RenderUrlAsPdf(url);
    pdf.SaveAs(path);
}
catch (IronPdf.Exceptions.IronPdfProductException ex)
{
    // Handle IronPDF-specific errors (licensing, rendering failures)
    Console.WriteLine($"IronPDF Error: {ex.Message}");
}
catch (TimeoutException ex)
{
    // Handle timeout
    Console.WriteLine($"Rendering timed out: {ex.Message}");
}
```

---

## Performance Considerations

### Memory Usage Patterns

| Scenario | ABCpdf | IronPDF | Notes |
|----------|--------|---------|-------|
| Single 10-page PDF | ~80 MB | ~50 MB | IronPDF more efficient |
| Batch 100 PDFs | High (manual cleanup) | ~100 MB | Use `using` statements |
| Large HTML (5MB+) | Variable | ~150 MB | Both need chunking for huge docs |

### Optimization Tips

**1. Reuse the Renderer for Batch Operations:**
```csharp
// Good: Single renderer instance
var renderer = new ChromePdfRenderer();
foreach (var html in htmlList)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{i}.pdf");
}

// Bad: New renderer each time (slower startup)
foreach (var html in htmlList)
{
    var renderer = new ChromePdfRenderer(); // Overhead!
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{i}.pdf");
}
```

**2. Use Async Methods in Web Applications:**
```csharp
public async Task<IActionResult> GenerateReportAsync()
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return File(pdf.BinaryData, "application/pdf");
}
```

**3. Dispose PDFs Promptly:**
```csharp
// Always use 'using' to ensure disposal
using var pdf = renderer.RenderHtmlAsPdf(html);
return pdf.BinaryData; // pdf disposed after this scope
```

**4. Compress Images in Output:**
```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.CompressImages(90); // Reduce image quality to 90%
pdf.SaveAs("compressed.pdf");
```

---

## Troubleshooting Guide

### Issue 1: PDF Renders Blank

**Symptom:** Output PDF has empty pages
**Cause:** JavaScript content not fully loaded before rendering
**Solution:**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.WaitFor.RenderDelay(2000); // Wait 2 seconds
// Or wait for specific element:
renderer.RenderingOptions.WaitFor.HtmlElementById("content-loaded");
```

### Issue 2: External CSS/Images Not Loading

**Symptom:** Styles missing, images show as broken
**Cause:** Relative paths not resolved
**Solution:**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.BaseUrl = new Uri("https://yourdomain.com/assets/");
// Or for local files:
renderer.RenderingOptions.BaseUrl = new Uri("file:///C:/projects/assets/");
```

### Issue 3: Fonts Rendering Incorrectly

**Symptom:** Text appears in wrong font or as squares
**Cause:** Custom fonts not embedded
**Solution:**
```csharp
// Use web fonts in your HTML
string html = @"
<style>
    @import url('https://fonts.googleapis.com/css2?family=Roboto&display=swap');
    body { font-family: 'Roboto', sans-serif; }
</style>
<body>Content with custom font</body>";
```

### Issue 4: Page Breaks in Wrong Places

**Symptom:** Content split awkwardly across pages
**Cause:** Missing CSS page break rules
**Solution:**
```csharp
string html = @"
<style>
    .no-break { page-break-inside: avoid; }
    .page-break { page-break-before: always; }
</style>
<div class='no-break'>Keep this together</div>
<div class='page-break'>Start new page</div>";
```

### Issue 5: License Key Not Working

**Symptom:** Watermark on PDF or license exception
**Cause:** License key not set before PDF operations
**Solution:**
```csharp
// Set BEFORE any IronPDF operations (Program.cs, Global.asax, Startup)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Verify license is valid
bool isLicensed = IronPdf.License.IsLicensed;
```

### Issue 6: Out of Memory on Large Documents

**Symptom:** OutOfMemoryException during rendering
**Cause:** Very large HTML or many images
**Solution:**
```csharp
// 1. Use streaming for very large docs
// 2. Compress images in source HTML
// 3. Split into smaller PDFs and merge

// Process in chunks
var renderer = new ChromePdfRenderer();
List<PdfDocument> chunks = new List<PdfDocument>();

foreach (var htmlChunk in SplitHtml(largeHtml))
{
    chunks.Add(renderer.RenderHtmlAsPdf(htmlChunk));
}

using var final = PdfDocument.Merge(chunks);
final.SaveAs("large-document.pdf");
```

### Issue 7: Slow First PDF Generation

**Symptom:** First PDF takes 5-10 seconds, subsequent ones are fast
**Cause:** Chrome engine initialization
**Solution:**
```csharp
// "Warm up" at application startup
public void ConfigureServices(IServiceCollection services)
{
    // Initialize renderer early
    var warmupRenderer = new ChromePdfRenderer();
    warmupRenderer.RenderHtmlAsPdf("<p>warmup</p>");
}
```

### Issue 8: Headers/Footers Not Appearing

**Symptom:** TextHeader/TextFooter not visible
**Cause:** Margins too small for header/footer space
**Solution:**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.MarginTop = 40; // mm - leave room for header
renderer.RenderingOptions.MarginBottom = 40; // mm - leave room for footer

renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    CenterText = "Header Text",
    FontSize = 12
};
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit all ABCpdf usage**
  ```bash
  grep -r "WebSupergoo" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current PDF output requirements**
  **Why:** Ensure that all existing PDF features and requirements are captured for accurate migration.

- [ ] **Create test cases with sample PDF outputs for comparison**
  **Why:** Baseline comparisons are essential to verify that IronPDF meets or exceeds current output quality.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Backup codebase**
  **Why:** Safeguard against data loss during migration.

### During Migration

- [ ] **Remove ABCpdf NuGet package**
  ```bash
  dotnet remove package ABCpdf
  ```
  **Why:** Clean removal of old package to prevent conflicts.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add new library for PDF operations.

- [ ] **Add license key to application startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Update all `using` statements**
  ```csharp
  // Before (ABCpdf)
  using WebSupergoo.ABCpdf;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure code references the correct library.

- [ ] **Convert `Doc` instantiation to `ChromePdfRenderer`**
  ```csharp
  // Before (ABCpdf)
  var doc = new Doc();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses ChromePdfRenderer for rendering operations.

- [ ] **Replace `doc.Clear()` with `using` statements**
  ```csharp
  // Before (ABCpdf)
  doc.Clear();

  // After (IronPDF)
  using (var pdf = renderer.RenderHtmlAsPdf(html))
  {
      pdf.SaveAs("output.pdf");
  }
  ```
  **Why:** IronPDF supports IDisposable pattern for automatic resource management.

- [ ] **Update method calls per API mapping**
  ```csharp
  // Before (ABCpdf)
  doc.AddImageHtml(html);
  doc.Save("output.pdf");

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Align with IronPDF's API for rendering operations.

- [ ] **Convert coordinate-based layouts to CSS**
  ```csharp
  // Before (ABCpdf)
  doc.Rect.Inset(20, 20);

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  renderer.RenderingOptions.MarginLeft = 20;
  renderer.RenderingOptions.MarginRight = 20;
  ```
  **Why:** Use CSS-based styling for modern layout management.

### Post-Migration

- [ ] **Run all existing PDF tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Visual comparison of PDF outputs (ABCpdf vs IronPDF)**
  **Why:** Ensure output quality meets or exceeds previous standards.

- [ ] **Test all PDF workflows in staging**
  **Why:** Confirm that all business processes function correctly with IronPDF.

- [ ] **Performance benchmark comparison**
  **Why:** Evaluate any performance changes due to the new library.

- [ ] **Remove ABCpdf license configuration**
  **Why:** Clean up any obsolete configurations related to the old library.

- [ ] **Update CI/CD pipeline dependencies**
  **Why:** Ensure build and deployment processes use the correct library.

- [ ] **Document any IronPDF-specific patterns for team**
  **Why:** Provide guidance for future development and maintenance.
---

## Additional Resources

- **IronPDF Documentation**: [https://ironpdf.com/docs/](https://ironpdf.com/docs/)
- **IronPDF Tutorials**: [https://ironpdf.com/tutorials/](https://ironpdf.com/tutorials/)
- **HTML to PDF Guide**: [https://ironpdf.com/how-to/html-file-to-pdf/](https://ironpdf.com/how-to/html-file-to-pdf/)
- **API Reference**: [https://ironpdf.com/object-reference/api/](https://ironpdf.com/object-reference/api/)
- **NuGet Package**: [https://www.nuget.org/packages/IronPdf/](https://www.nuget.org/packages/IronPdf/)

---

*This migration guide was created to help developers transition from ABCpdf to IronPDF efficiently.*
