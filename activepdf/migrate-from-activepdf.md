# How Do I Migrate from ActivePDF to IronPDF in C#?

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

ActivePDF Toolkit has been a powerful PDF manipulation library, but its acquisition by Foxit has created significant uncertainty for developers relying on the platform.

### The Case for Leaving ActivePDF

**Uncertain Product Future**: ActivePDF's acquisition by Foxit raises questions about the product's long-term development trajectory. Developers face potential risks of the toolkit becoming a legacy product with reduced support and innovation.

**Licensing Complications**: The transition period following the acquisition has introduced licensing uncertainties. Machine-locked licensing can complicate deployments, especially in cloud and containerized environments.

**Legacy Codebase Concerns**: ActivePDF's architecture reflects an older design philosophy with stateful toolkit patterns (`OpenOutputFile`, `CloseOutputFile`) that don't align with modern .NET conventions.

**Installation Complexity**: ActivePDF often requires manual DLL references and path configuration (`new Toolkit(@"C:\Program Files\ActivePDF\...")`), unlike modern NuGet-based package management.

**Limited Modern .NET Support**: While ActivePDF supports .NET, its focus has historically been on .NET Framework, with varying support for .NET Core and .NET 5+.

### What IronPDF Provides

| ActivePDF Limitation | IronPDF Solution |
|----------------------|------------------|
| Uncertain future (Foxit acquisition) | Independent company, clear roadmap |
| Machine-locked licensing | Code-based license key |
| Manual DLL installation | Simple NuGet package |
| Stateful Open/Close pattern | Fluent, functional API |
| Legacy .NET Framework focus | .NET Framework 4.6.2 to .NET 9 |
| Complex configuration paths | Zero configuration needed |

---

## Before You Start

### Prerequisites

- .NET Framework 4.6.2+ or .NET Core 3.1+ / .NET 5-9
- Visual Studio 2019+ or JetBrains Rider
- NuGet Package Manager access
- IronPDF license key (free trial available at [ironpdf.com](https://ironpdf.com))

### Find All ActivePDF References

Run these commands in your solution directory to locate all files using ActivePDF:

```bash
grep -r "using ActivePDF" --include="*.cs" .
grep -r "using APToolkitNET" --include="*.cs" .
grep -r "APToolkitNET" --include="*.csproj" .
```

### Breaking Changes to Anticipate

| Category | ActivePDF Behavior | IronPDF Behavior | Migration Action |
|----------|-------------------|------------------|------------------|
| Object Model | Single `Toolkit` object | `ChromePdfRenderer` + `PdfDocument` | Separate concerns |
| File Operations | `OpenOutputFile()`/`CloseOutputFile()` | Direct `SaveAs()` | Remove open/close calls |
| DLL Reference | Manual path reference | NuGet package | Remove path configuration |
| Page Creation | `NewPage()` method | Automatic from HTML | Remove page creation calls |
| Return Values | Integer error codes | Exceptions | Use try/catch |
| Licensing | Machine-locked | Code-based key | Add `License.LicenseKey` |

---

## Quick Start (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove ActivePDF package
dotnet remove package APToolkitNET

# Install IronPDF
dotnet add package IronPdf
```

Or via Package Manager Console:
```powershell
Uninstall-Package APToolkitNET
Install-Package IronPdf
```

### Step 2: Set Your License Key (Program.cs or Startup)

```csharp
// Add this at application startup, before any IronPDF operations
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 3: Global Find & Replace

| Find | Replace With |
|------|--------------|
| `using ActivePDF.Toolkit;` | `using IronPdf;` |
| `using APToolkitNET;` | `using IronPdf;` |
| `using APToolkitNET.PDFObjects;` | `using IronPdf;` |
| `using APToolkitNET.Common;` | `using IronPdf;` |

### Step 4: Verify Basic Operation

**Before (ActivePDF):**
```csharp
using ActivePDF.Toolkit;

Toolkit toolkit = new Toolkit();
if (toolkit.OpenOutputFile("output.pdf") == 0)
{
    toolkit.AddHTML("<h1>Hello World</h1>");
    toolkit.CloseOutputFile();
}
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

| ActivePDF Namespace | IronPDF Namespace | Purpose |
|---------------------|-------------------|---------|
| `ActivePDF.Toolkit` | `IronPdf` | Core functionality |
| `APToolkitNET` | `IronPdf` | Main PDF operations |
| `APToolkitNET.PDFObjects` | `IronPdf` | PDF document objects |
| `APToolkitNET.Common` | `IronPdf` | Common utilities |
| `APToolkitNET.Extractor` | `IronPdf` | Text extraction |
| `APToolkitNET.Rasterizer` | `IronPdf` | PDF to image |

### Document Creation Methods

| ActivePDF Method | IronPDF Method | Notes |
|------------------|----------------|-------|
| `new Toolkit()` | `new ChromePdfRenderer()` | Renderer creates PDFs |
| `new Toolkit(path)` | `new ChromePdfRenderer()` | No path needed |
| `toolkit.OpenOutputFile(path)` | No equivalent needed | Just call SaveAs at end |
| `toolkit.CloseOutputFile()` | No equivalent needed | Automatic cleanup |
| `toolkit.AddHTML(html)` | `renderer.RenderHtmlAsPdf(html)` | Returns PdfDocument |
| `toolkit.AddURL(url)` | `renderer.RenderUrlAsPdf(url)` | URL to PDF |
| `toolkit.NewPage()` | Automatic from HTML | Pages auto-created |
| `toolkit.SaveAs(path)` | `pdf.SaveAs(path)` | Save to file |

### File Operations Methods

| ActivePDF Method | IronPDF Method | Notes |
|------------------|----------------|-------|
| `toolkit.OpenInputFile(path)` | `PdfDocument.FromFile(path)` | Load existing PDF |
| `toolkit.AddPDF(path)` | `PdfDocument.Merge()` | For merging |
| `toolkit.Merge(path)` | `PdfDocument.Merge(pdfs)` | Merge multiple PDFs |
| `toolkit.GetPDFInfo()` | `pdf.MetaData` | Document metadata |
| `toolkit.GetPageCount()` | `pdf.PageCount` | Page count property |

### Page Manipulation Methods

| ActivePDF Method | IronPDF Method | Notes |
|------------------|----------------|-------|
| `toolkit.SetPageSize(w, h)` | `RenderingOptions.PaperSize` | Use enum or custom |
| `toolkit.SetOrientation("Portrait")` | `RenderingOptions.PaperOrientation` | Enum value |
| `toolkit.SetMargins(t, b, l, r)` | `RenderingOptions.MarginTop/Bottom/Left/Right` | Individual properties |
| `toolkit.CopyPage(src, dest)` | `pdf.CopyPages(indices)` | Copy pages |
| `toolkit.DeletePage(index)` | `pdf.RemovePages(index)` | Remove pages |
| `toolkit.RotatePage(degrees)` | `pdf.Pages[i].Rotation` | Per-page rotation |

### Content Methods

| ActivePDF Method | IronPDF Method | Notes |
|------------------|----------------|-------|
| `toolkit.AddText(text, x, y)` | Use HTML/CSS positioning | HTML approach |
| `toolkit.AddImage(path, x, y)` | `ImageStamper` or HTML `<img>` | Stamping |
| `toolkit.AddWatermark(text)` | `pdf.ApplyWatermark(html)` | HTML watermark |
| `toolkit.AddBookmark(title)` | `pdf.BookMarks.AddBookMarkAtStart()` | Bookmark API |
| `toolkit.AddAnnotation(...)` | `pdf.Annotations` | Annotation API |

### Text Extraction Methods

| ActivePDF Method | IronPDF Method | Notes |
|------------------|----------------|-------|
| `toolkit.GetText()` | `pdf.ExtractAllText()` | Full document text |
| `toolkit.GetTextFromPage(page)` | `pdf.Pages[i].Text` | Per-page extraction |
| `extractor.ExtractToFile(path)` | `File.WriteAllText(path, pdf.ExtractAllText())` | Save to file |

### Security Methods

| ActivePDF Method | IronPDF Method | Notes |
|------------------|----------------|-------|
| `toolkit.Encrypt(password)` | `pdf.SecuritySettings.OwnerPassword` | Owner password |
| `toolkit.SetUserPassword(pwd)` | `pdf.SecuritySettings.UserPassword` | User password |
| `toolkit.SetPermissions(flags)` | `pdf.SecuritySettings.AllowUserXxx` | Individual permissions |
| `toolkit.RemoveEncryption()` | Load with password, save without | No direct method |

### Configuration Options Mapping

| ActivePDF Setting | IronPDF Equivalent | Default |
|-------------------|-------------------|---------|
| `toolkit.SetPageSize(612, 792)` | `RenderingOptions.PaperSize = PdfPaperSize.Letter` | A4 |
| `toolkit.SetPageSize(595, 842)` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | A4 |
| `toolkit.SetOrientation("Landscape")` | `RenderingOptions.PaperOrientation = Landscape` | Portrait |
| `toolkit.SetDPI(300)` | `RenderingOptions.Dpi` | 96 |
| `toolkit.SetCompression(true)` | `pdf.CompressImages()` | After rendering |
| `toolkit.EnableJavaScript(true)` | `RenderingOptions.EnableJavaScript` | true |

---

## Code Migration Examples

### Example 1: HTML String to PDF

**Before (ActivePDF):**
```csharp
using ActivePDF.Toolkit;

public void CreatePdfFromHtml(string html, string outputPath)
{
    Toolkit toolkit = new Toolkit();

    if (toolkit.OpenOutputFile(outputPath) == 0)
    {
        toolkit.SetPageSize(612, 792); // Letter size
        toolkit.SetMargins(72, 72, 72, 72); // 1 inch margins
        toolkit.AddHTML(html);
        toolkit.CloseOutputFile();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreatePdfFromHtml(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
    renderer.RenderingOptions.MarginTop = 25;  // mm
    renderer.RenderingOptions.MarginBottom = 25;
    renderer.RenderingOptions.MarginLeft = 25;
    renderer.RenderingOptions.MarginRight = 25;

    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 2: URL to PDF

**Before (ActivePDF):**
```csharp
using ActivePDF.Toolkit;

public void ConvertUrlToPdf(string url, string outputPath)
{
    Toolkit toolkit = new Toolkit();

    if (toolkit.OpenOutputFile(outputPath) == 0)
    {
        toolkit.AddURL(url);
        toolkit.CloseOutputFile();
        Console.WriteLine("PDF created successfully");
    }
    else
    {
        Console.WriteLine("Error creating PDF");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void ConvertUrlToPdf(string url, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    try
    {
        using var pdf = renderer.RenderUrlAsPdf(url);
        pdf.SaveAs(outputPath);
        Console.WriteLine("PDF created successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating PDF: {ex.Message}");
    }
}
```

### Example 3: Merge Multiple PDFs

**Before (ActivePDF):**
```csharp
using ActivePDF.Toolkit;

public void MergePdfs(string[] inputFiles, string outputPath)
{
    Toolkit toolkit = new Toolkit();

    if (toolkit.OpenOutputFile(outputPath) == 0)
    {
        foreach (string file in inputFiles)
        {
            toolkit.AddPDF(file);
        }
        toolkit.CloseOutputFile();
    }
}
```

The [comprehensive documentation](https://ironpdf.com/blog/migration-guides/migrate-from-activepdf-to-ironpdf/) provides additional merge strategies and handles edge cases like memory optimization.

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

**Before (ActivePDF):**
```csharp
using ActivePDF.Toolkit;

public void CreatePdfWithHeaderFooter(string html, string outputPath)
{
    Toolkit toolkit = new Toolkit();

    if (toolkit.OpenOutputFile(outputPath) == 0)
    {
        toolkit.SetHeader("My Document", 12, "Arial");
        toolkit.SetFooter("Page %p of %P", 10, "Arial");
        toolkit.AddHTML(html);
        toolkit.CloseOutputFile();
    }
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
        FontSize = 12,
        FontFamily = "Arial"
    };

    renderer.RenderingOptions.TextFooter = new TextHeaderFooter
    {
        CenterText = "Page {page} of {total-pages}",
        FontSize = 10,
        FontFamily = "Arial"
    };

    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 5: Password Protection

**Before (ActivePDF):**
```csharp
using ActivePDF.Toolkit;

public void ProtectPdf(string inputPath, string outputPath, string password)
{
    Toolkit toolkit = new Toolkit();

    if (toolkit.OpenInputFile(inputPath) == 0)
    {
        toolkit.Encrypt(password);
        toolkit.SetUserPassword(password);
        toolkit.SetPermissions(4); // Print only
        toolkit.SaveAs(outputPath);
        toolkit.CloseInputFile();
    }
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
    pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

    pdf.SaveAs(outputPath);
}
```

### Example 6: Extract Text from PDF

**Before (ActivePDF):**
```csharp
using ActivePDF.Toolkit;

public string ExtractText(string pdfPath)
{
    Toolkit toolkit = new Toolkit();
    string text = "";

    if (toolkit.OpenInputFile(pdfPath) == 0)
    {
        int pageCount = toolkit.GetPageCount();
        for (int i = 1; i <= pageCount; i++)
        {
            text += toolkit.GetTextFromPage(i) + "\n";
        }
        toolkit.CloseInputFile();
    }

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

**Before (ActivePDF):**
```csharp
using ActivePDF.Toolkit;

public void AddWatermark(string inputPath, string outputPath, string watermarkText)
{
    Toolkit toolkit = new Toolkit();

    if (toolkit.OpenInputFile(inputPath) == 0)
    {
        int pageCount = toolkit.GetPageCount();
        for (int i = 1; i <= pageCount; i++)
        {
            toolkit.SetPage(i);
            toolkit.AddWatermark(watermarkText, 45, 0.5f);
        }
        toolkit.SaveAs(outputPath);
        toolkit.CloseInputFile();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void AddWatermark(string inputPath, string outputPath, string watermarkText)
{
    using var pdf = PdfDocument.FromFile(inputPath);

    pdf.ApplyWatermark(
        $"<h1 style='color:lightgray;font-size:72px;'>{watermarkText}</h1>",
        rotation: 45,
        opacity: 50);

    pdf.SaveAs(outputPath);
}
```

### Example 8: Custom Page Size

**Before (ActivePDF):**
```csharp
using ActivePDF.Toolkit;

public void CreateCustomSizePdf(string html, string outputPath)
{
    Toolkit toolkit = new Toolkit();

    if (toolkit.OpenOutputFile(outputPath) == 0)
    {
        toolkit.SetPageSize(396, 612); // Half letter in points
        toolkit.AddHTML(html);
        toolkit.CloseOutputFile();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreateCustomSizePdf(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.SetCustomPaperSizeInInches(5.5, 8.5);
    // Or: renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(140, 216);

    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 9: PDF to Images

**Before (ActivePDF):**
```csharp
using ActivePDF.Toolkit;

public void ConvertToImages(string pdfPath, string outputDir)
{
    Toolkit toolkit = new Toolkit();

    if (toolkit.OpenInputFile(pdfPath) == 0)
    {
        var rasterizer = toolkit.GetRasterizer();
        int pageCount = toolkit.GetPageCount();

        for (int i = 1; i <= pageCount; i++)
        {
            rasterizer.RenderPage(i, $"{outputDir}/page_{i}.png", 300);
        }
        toolkit.CloseInputFile();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.IO;

public void ConvertToImages(string pdfPath, string outputDir)
{
    using var pdf = PdfDocument.FromFile(pdfPath);

    var images = pdf.ToBitmap(300); // 300 DPI

    for (int i = 0; i < images.Length; i++)
    {
        images[i].Save(Path.Combine(outputDir, $"page_{i + 1}.png"));
    }
}
```

### Example 10: Batch Processing

**Before (ActivePDF):**
```csharp
using ActivePDF.Toolkit;

public void BatchConvert(string[] htmlFiles, string outputDir)
{
    Toolkit toolkit = new Toolkit();

    foreach (var htmlFile in htmlFiles)
    {
        string outputPath = Path.Combine(outputDir,
            Path.GetFileNameWithoutExtension(htmlFile) + ".pdf");

        if (toolkit.OpenOutputFile(outputPath) == 0)
        {
            string html = File.ReadAllText(htmlFile);
            toolkit.AddHTML(html);
            toolkit.CloseOutputFile();
        }
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

**ActivePDF Pattern:**
```csharp
[HttpPost]
public IActionResult GeneratePdf([FromBody] ReportRequest request)
{
    Toolkit toolkit = new Toolkit();

    if (toolkit.OpenOutputFile("temp.pdf") == 0)
    {
        toolkit.AddHTML(request.Html);
        toolkit.CloseOutputFile();

        byte[] bytes = System.IO.File.ReadAllBytes("temp.pdf");
        return File(bytes, "application/pdf", "report.pdf");
    }

    return BadRequest("PDF generation failed");
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

ActivePDF doesn't support native async. **IronPDF** does:

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

```csharp
// Program.cs (.NET 6+)
builder.Services.AddSingleton<ChromePdfRenderer>();

// Service wrapper
public interface IPdfService
{
    Task<byte[]> GeneratePdfAsync(string html);
    Task<byte[]> GeneratePdfFromUrlAsync(string url);
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

    public async Task<byte[]> GeneratePdfFromUrlAsync(string url)
    {
        using var pdf = await _renderer.RenderUrlAsPdfAsync(url);
        return pdf.BinaryData;
    }
}
```

### Scenario 4: Error Handling Comparison

**ActivePDF Error Handling:**
```csharp
Toolkit toolkit = new Toolkit();
int result = toolkit.OpenOutputFile(path);

if (result != 0)
{
    // Error - need to look up error code
    Console.WriteLine($"Error code: {result}");
}
```

**IronPDF Error Handling:**
```csharp
try
{
    var renderer = new ChromePdfRenderer();
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(path);
}
catch (IronPdf.Exceptions.IronPdfProductException ex)
{
    Console.WriteLine($"IronPDF Error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"General Error: {ex.Message}");
}
```

---

## Performance Considerations

### Memory Usage Patterns

| Scenario | ActivePDF | IronPDF | Notes |
|----------|-----------|---------|-------|
| Single PDF | ~60 MB | ~50 MB | IronPDF slightly more efficient |
| Batch (100 PDFs) | High (stateful) | ~100 MB | Reuse renderer |
| Large documents | Variable | ~150 MB | Both handle well |

### Optimization Tips

**1. Reuse the Renderer:**
```csharp
// Good: Reuse renderer
var renderer = new ChromePdfRenderer();
foreach (var html in htmlList)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{i}.pdf");
}
```

**2. Use Async in Web Applications:**
```csharp
public async Task<IActionResult> GenerateReport()
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return File(pdf.BinaryData, "application/pdf");
}
```

**3. Dispose PDFs Properly:**
```csharp
// Always use 'using' statements
using var pdf = renderer.RenderHtmlAsPdf(html);
return pdf.BinaryData;
```

**4. Compress Images:**
```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.CompressImages(85); // 85% quality
pdf.SaveAs("compressed.pdf");
```

---

## Troubleshooting Guide

### Issue 1: "Cannot Find APToolkitNET.dll"

**Symptom:** FileNotFoundException after removing ActivePDF
**Cause:** Old DLL references remain
**Solution:**
```xml
<!-- Remove from .csproj -->
<Reference Include="APToolkitNET">
    <HintPath>..\..\Libs\APToolkitNET.dll</HintPath>
</Reference>
```

### Issue 2: Integer Return Values No Longer Work

**Symptom:** Code checked `if (toolkit.OpenOutputFile(path) == 0)`
**Cause:** IronPDF uses exceptions, not return codes
**Solution:**
```csharp
// Before
if (toolkit.OpenOutputFile(path) == 0) { ... }

// After
try
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(path);
}
catch (Exception ex)
{
    // Handle error
}
```

### Issue 3: Page Size Units Different

**Symptom:** PDFs are wrong size after migration
**Cause:** ActivePDF uses points, IronPDF uses mm or enums
**Solution:**
```csharp
// ActivePDF: Points (612x792 = Letter)
toolkit.SetPageSize(612, 792);

// IronPDF: Use enum
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
// Or custom in mm:
renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(215.9, 279.4);
```

### Issue 4: License Key Not Working

**Symptom:** Watermark appears or license exception
**Cause:** License key not set before operations
**Solution:**
```csharp
// Set at app startup, BEFORE any IronPDF operations
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Verify
bool isLicensed = IronPdf.License.IsLicensed;
```

### Issue 5: Missing CloseOutputFile Equivalent

**Symptom:** Looking for close/cleanup method
**Cause:** Different paradigm
**Solution:**
```csharp
// ActivePDF
toolkit.OpenOutputFile(path);
toolkit.AddHTML(html);
toolkit.CloseOutputFile(); // Required!

// IronPDF - no open/close needed
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs(path); // 'using' handles cleanup
```

### Issue 6: PDF Renders Blank

**Symptom:** Empty pages in output
**Cause:** JavaScript not loaded
**Solution:**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.WaitFor.RenderDelay(2000);
// Or wait for element:
renderer.RenderingOptions.WaitFor.HtmlElementById("content-loaded");
```

### Issue 7: Slow First PDF Generation

**Symptom:** First PDF takes 5-10 seconds
**Cause:** Chrome engine initialization
**Solution:**
```csharp
// Warm up at app start
var warmup = new ChromePdfRenderer();
warmup.RenderHtmlAsPdf("<p>warmup</p>");
```

### Issue 8: CSS/Images Not Loading

**Symptom:** Missing styles or broken images
**Cause:** Relative paths not resolved
**Solution:**
```csharp
renderer.RenderingOptions.BaseUrl = new Uri("https://yourdomain.com/assets/");
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit ActivePDF usage**
  ```bash
  grep -r "ActivePDF\|APToolkitNET" --include="*.cs" .
  ```
  **Why:** Identify all instances of ActivePDF usage to ensure complete migration coverage.

- [ ] **List all PDF operations in your codebase**
  ```csharp
  // Look for patterns like:
  var toolkit = new APToolkitNET.Toolkit();
  toolkit.OpenInputFile("input.pdf");
  toolkit.OpenOutputFile("output.pdf");
  toolkit.CopyForm(0, 0);
  toolkit.CloseOutputFile();
  ```
  **Why:** Understanding all PDF operations helps map them to IronPDF equivalents.

- [ ] **Document current output requirements**
  ```csharp
  // Example requirements:
  var pageSize = "A4";
  var orientation = "Portrait";
  var margins = new { Top = 10, Bottom = 10, Left = 10, Right = 10 };
  ```
  **Why:** Ensures that the migrated solution meets all existing output specifications.

- [ ] **Obtain IronPDF trial license**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Backup codebase**
  **Why:** Safeguard against data loss during the migration process.

### During Migration

- [ ] **Remove APToolkitNET package reference**
  ```bash
  dotnet remove package APToolkitNET
  ```
  **Why:** Clean removal of the old package to prevent conflicts.

- [ ] **Remove manual DLL references from .csproj**
  ```xml
  <!-- Before -->
  <Reference Include="APToolkitNET">
    <HintPath>path\to\APToolkitNET.dll</HintPath>
  </Reference>
  ```
  **Why:** IronPDF uses NuGet for package management, eliminating the need for manual DLL references.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Adds IronPDF to the project, enabling modern PDF functionalities.

- [ ] **Set license key at application startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations to unlock full functionality.

- [ ] **Update all `using` statements**
  ```csharp
  // Before (APToolkitNET)
  using APToolkitNET;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure the code references the correct namespaces for IronPDF.

- [ ] **Remove `OpenOutputFile`/`CloseOutputFile` patterns**
  ```csharp
  // Before (APToolkitNET)
  toolkit.OpenOutputFile("output.pdf");
  toolkit.CloseOutputFile();

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses a more modern, fluent API without the need for manual file handling.

- [ ] **Replace integer error checking with try/catch**
  ```csharp
  // Before (APToolkitNET)
  int result = toolkit.SomeMethod();
  if (result != 0) { /* handle error */ }

  // After (IronPDF)
  try
  {
      // IronPDF operations
  }
  catch (Exception ex)
  {
      // handle error
  }
  ```
  **Why:** IronPDF uses exceptions for error handling, aligning with modern .NET practices.

- [ ] **Update page size from points to enums/mm**
  ```csharp
  // Before (APToolkitNET)
  toolkit.SetPageSize(595, 842); // A4 in points

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** IronPDF uses enums for standard page sizes, simplifying configuration.

- [ ] **Convert method calls per API mapping**
  ```csharp
  // Before (APToolkitNET)
  toolkit.CopyForm(0, 0);

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf("<html><body>Content</body></html>");
  ```
  **Why:** Ensures all PDF operations are correctly mapped to IronPDF's API.

### Post-Migration

- [ ] **Run all existing tests**
  **Why:** Verify that all PDF generation and manipulation still work correctly after migration.

- [ ] **Compare PDF outputs visually**
  **Why:** Ensure that the output quality and layout remain consistent with the previous implementation.

- [ ] **Test all PDF workflows in staging**
  **Why:** Validate the entire PDF processing pipeline in a controlled environment before production deployment.

- [ ] **Verify licensing works correctly**
  **Why:** Ensure that the IronPDF license key is correctly applied and the library functions without trial limitations.

- [ ] **Performance benchmark**
  **Why:** Compare performance metrics between ActivePDF and IronPDF to identify any improvements or regressions.

- [ ] **Remove old ActivePDF installation files**
  **Why:** Clean up any residual files from the old library to avoid confusion and free up space.

- [ ] **Update CI/CD dependencies**
  **Why:** Ensure that build and deployment pipelines use IronPDF and are free of ActivePDF dependencies.

- [ ] **Document IronPDF patterns for team**
  **Why:** Provide guidance to the development team on using IronPDF effectively, ensuring consistent and efficient use of the library.
---

## Additional Resources

- **IronPDF Documentation**: [https://ironpdf.com/docs/](https://ironpdf.com/docs/)
- **IronPDF Tutorials**: [https://ironpdf.com/tutorials/](https://ironpdf.com/tutorials/)
- **HTML to PDF Guide**: [https://ironpdf.com/how-to/html-file-to-pdf/](https://ironpdf.com/how-to/html-file-to-pdf/)
- **API Reference**: [https://ironpdf.com/object-reference/api/](https://ironpdf.com/object-reference/api/)
- **NuGet Package**: [https://www.nuget.org/packages/IronPdf/](https://www.nuget.org/packages/IronPdf/)

---

*This migration guide was created to help developers transition from ActivePDF to IronPDF efficiently.*
