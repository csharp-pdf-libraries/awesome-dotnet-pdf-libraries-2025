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

ActivePDF Toolkit has been a powerful PDF manipulation library, but its acquisition by PDFTron (now Apryse) has folded it into a much larger product portfolio, creating uncertainty for developers who picked ActivePDF specifically for its server-side automation focus.

### The Case for Leaving ActivePDF

**Brand and roadmap uncertainty**: PDFTron acquired ActivePDF on June 23, 2020, and PDFTron itself rebranded to Apryse in February 2023. ActivePDF still ships under its own name (Toolkit, WebGrabber, DocConverter, Server, Meridian) but is now one of several brands inside Apryse, alongside the flagship Apryse SDK.

**Stateful, COM-derived API surface**: ActivePDF Toolkit's design comes from its long history as a COM/native component. Calls like `OpenOutputFile`/`CloseOutputFile`, integer return codes (0 for success, non-zero for errors), and the explicit `MergeFile(file, startPage, endPage)` pattern are accurate to the docs but don't fit modern .NET idioms (exceptions, `using`, async).

**Native dependency footprint**: Starting with Toolkit 10, the native libraries are no longer auto-copied to the system folder, so the constructor frequently needs an explicit `CoreLibPath` pointing at the installed directory (e.g. `new Toolkit(CoreLibPath: @"C:\Program Files\ActivePDF\Toolkit\bin")`). This complicates Docker, CI, and any deployment without an installer.

**HTML-to-PDF lives in a separate product**: Toolkit itself is a PDF manipulation library — it does not render HTML. To go from HTML or a URL to a PDF you need WebGrabber (`APWebGrabber.WebGrabber.ConvertToPDF`), which is sold and licensed separately.

### What IronPDF Provides

| ActivePDF Limitation | IronPDF Solution |
|----------------------|------------------|
| Multiple SKUs (Toolkit + WebGrabber + DocConverter) | Single `IronPdf` NuGet package |
| Apryse umbrella, several brands | Focused PDF library, single roadmap |
| Native `CoreLibPath` configuration | NuGet handles native runtimes |
| Stateful Open/Close pattern | Fluent, functional API |
| Integer return codes | Standard .NET exceptions |
| .NET Framework heritage | .NET Framework 4.6.2 to .NET 9 |

---

## Before You Start

### Prerequisites

- .NET Framework 4.6.2+ or .NET Core 3.1+ / .NET 5-9
- Visual Studio 2019+ or JetBrains Rider
- NuGet Package Manager access
- IronPDF license key (free trial available at [ironpdf.com](https://ironpdf.com))

### Find All ActivePDF References

Run these commands in your solution directory to locate all files using ActivePDF. The real namespaces are `APToolkitNET` (Toolkit) and `APWebGrabber` (WebGrabber); a few legacy projects also reference `ActivePDF.Toolkit` via the COM interop assembly:

```bash
grep -r "using APToolkitNET" --include="*.cs" .
grep -r "using APWebGrabber" --include="*.cs" .
grep -r "ActivePDF" --include="*.csproj" .
```

### Breaking Changes to Anticipate

| Category | ActivePDF Behavior | IronPDF Behavior | Migration Action |
|----------|-------------------|------------------|------------------|
| Product split | Toolkit (manipulation) + WebGrabber (HTML rendering) sold separately | Single `IronPdf` package | Collapse both into one library |
| Object Model | `APToolkitNET.Toolkit` for PDFs, `APWebGrabber.WebGrabber` for HTML | `ChromePdfRenderer` + `PdfDocument` | Separate concerns |
| File Operations | `OpenOutputFile()`/`CloseOutputFile()` | Direct `SaveAs()` | Remove open/close calls |
| Native runtime | `CoreLibPath` argument since v10 | NuGet ships natives | Remove path configuration |
| Page Creation | `NewPage()` method | Automatic from HTML | Remove page creation calls |
| Return Values | Integer error codes (0 = success) | Exceptions | Use try/catch |
| Licensing | Per-server / per-core ActivePDF keys | Code-based IronPDF key | Add `License.LicenseKey` |

---

## Quick Start (5 Minutes)

### Step 1: Update NuGet Packages

The package on nuget.org is `ActivePDF.Toolkit` (current version 11.4.4, published December 2025). HTML rendering ships separately as `ActivePDF.WebGrabber`.

```bash
# Remove ActivePDF packages
dotnet remove package ActivePDF.Toolkit
dotnet remove package ActivePDF.WebGrabber

# Install IronPDF
dotnet add package IronPdf
```

Or via Package Manager Console:
```powershell
Uninstall-Package ActivePDF.Toolkit
Uninstall-Package ActivePDF.WebGrabber
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
| `using APToolkitNET;` | `using IronPdf;` |
| `using APWebGrabber;` | `using IronPdf;` |
| `APToolkitNET.Toolkit` | `ChromePdfRenderer` (rendering) / `PdfDocument` (manipulation) |
| `APWebGrabber.WebGrabber` | `ChromePdfRenderer` |

### Step 4: Verify Basic Operation

**Before (ActivePDF WebGrabber — HTML to PDF lives here, not in Toolkit):**
```csharp
using APWebGrabber;
using System.IO;

var wg = new WebGrabber();
File.WriteAllText("input.html", "<h1>Hello World</h1>");
wg.URL = "input.html";
wg.OutputDirectory = Directory.GetCurrentDirectory();
wg.OutputFilename = "output.pdf";
wg.ConvertToPDF(); // returns 0 on success
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
| `APToolkitNET` | `IronPdf` | PDF manipulation (merge, forms, security, text) |
| `APWebGrabber` | `IronPdf` | HTML-to-PDF / URL-to-PDF rendering |
| `APToolkitNET` (Rasterizer call) | `IronPdf` | PDF to image (`pdf.ToBitmap`) |

### Document Creation Methods

| ActivePDF Method | IronPDF Method | Notes |
|------------------|----------------|-------|
| `new APToolkitNET.Toolkit()` | `new ChromePdfRenderer()` / `new PdfDocument(...)` | IronPDF separates rendering from manipulation |
| `new Toolkit(CoreLibPath: path)` | `new ChromePdfRenderer()` | NuGet ships natives — no `CoreLibPath` |
| `toolkit.OpenOutputFile(path)` | No equivalent needed | Just call `SaveAs` at end |
| `toolkit.CloseOutputFile()` | No equivalent needed | `using` handles cleanup |
| `webGrabber.URL = html; webGrabber.ConvertToPDF()` | `renderer.RenderHtmlAsPdf(html)` | WebGrabber, not Toolkit |
| `webGrabber.URL = url; webGrabber.ConvertToPDF()` | `renderer.RenderUrlAsPdf(url)` | WebGrabber, not Toolkit |
| `toolkit.NewPage()` | Automatic from HTML | Pages auto-created |
| `toolkit.SaveOutputToFile()` / output filename | `pdf.SaveAs(path)` | Save to file |

### File Operations Methods

| ActivePDF Method | IronPDF Method | Notes |
|------------------|----------------|-------|
| `toolkit.OpenInputFile(path)` | `PdfDocument.FromFile(path)` | Load existing PDF |
| `toolkit.MergeFile(path, startPage, endPage)` | `PdfDocument.Merge(pdfs)` | ActivePDF merges into the open output file in-place; IronPDF returns a new merged document |
| `toolkit.NumPages` (property) | `pdf.PageCount` | Page count |
| `toolkit.SetCustomDocInfo(...)` | `pdf.MetaData.Title = ...` etc. | Document metadata |

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

**Before (ActivePDF WebGrabber):**
```csharp
using APWebGrabber;
using System.IO;

public void CreatePdfFromHtml(string html, string outputPath)
{
    var wg = new WebGrabber();

    string tempHtml = Path.Combine(Path.GetTempPath(), "input.html");
    File.WriteAllText(tempHtml, html);

    wg.URL = tempHtml;
    wg.PageSize = APWebGrabber.Enums.PageSize.Letter;
    wg.OutputDirectory = Path.GetDirectoryName(outputPath);
    wg.OutputFilename = Path.GetFileName(outputPath);

    wg.ConvertToPDF(); // 0 on success
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

**Before (ActivePDF WebGrabber):**
```csharp
using APWebGrabber;
using System.IO;

public void ConvertUrlToPdf(string url, string outputPath)
{
    var wg = new WebGrabber();
    wg.URL = url;
    wg.OutputDirectory = Path.GetDirectoryName(outputPath);
    wg.OutputFilename = Path.GetFileName(outputPath);

    int result = wg.ConvertToPDF();
    Console.WriteLine(result == 0 ? "PDF created successfully" : $"Error code: {result}");
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

**Before (ActivePDF Toolkit):**
```csharp
using APToolkitNET;

public void MergePdfs(string[] inputFiles, string outputPath)
{
    using (Toolkit toolkit = new Toolkit())
    {
        if (toolkit.OpenOutputFile(outputPath) == 0)
        {
            foreach (string file in inputFiles)
            {
                // MergeFile(FileName, StartPage, EndPage); -1 means "to end"
                toolkit.MergeFile(file, 1, -1);
            }
            toolkit.CloseOutputFile();
        }
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

**Before (ActivePDF WebGrabber):**
```csharp
using APWebGrabber;
using System.IO;

public void CreatePdfWithHeaderFooter(string html, string outputPath)
{
    var wg = new WebGrabber();
    string tempHtml = Path.Combine(Path.GetTempPath(), "input.html");
    File.WriteAllText(tempHtml, html);

    wg.URL = tempHtml;
    wg.HeaderText = "My Document";
    wg.FooterText = "Page [page] of [pages]";
    wg.OutputDirectory = Path.GetDirectoryName(outputPath);
    wg.OutputFilename = Path.GetFileName(outputPath);
    wg.ConvertToPDF();
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

**Before (ActivePDF Toolkit):**
```csharp
using APToolkitNET;

public void ProtectPdf(string inputPath, string outputPath, string password)
{
    using (Toolkit toolkit = new Toolkit())
    {
        if (toolkit.OpenOutputFile(outputPath) == 0
            && toolkit.OpenInputFile(inputPath) == 0)
        {
            toolkit.SetEncryption(password, password, 128, 0);
            toolkit.CopyForm(0, 0);
            toolkit.CloseInputFile();
            toolkit.CloseOutputFile();
        }
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

**Before (ActivePDF Toolkit):**
```csharp
using APToolkitNET;
using System.Text;

public string ExtractText(string pdfPath)
{
    var sb = new StringBuilder();

    using (Toolkit toolkit = new Toolkit())
    {
        if (toolkit.OpenInputFile(pdfPath) == 0)
        {
            int pageCount = toolkit.NumPages;
            for (int i = 1; i <= pageCount; i++)
            {
                sb.AppendLine(toolkit.GetPageText(i, 0));
            }
            toolkit.CloseInputFile();
        }
    }

    return sb.ToString();
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

**Before (ActivePDF Toolkit — drawn as page text per page):**
```csharp
using APToolkitNET;

public void AddWatermark(string inputPath, string outputPath, string watermarkText)
{
    using (Toolkit toolkit = new Toolkit())
    {
        if (toolkit.OpenOutputFile(outputPath) == 0
            && toolkit.OpenInputFile(inputPath) == 0)
        {
            int pageCount = toolkit.NumPages;
            for (int i = 1; i <= pageCount; i++)
            {
                toolkit.CopyForm(i, 0);
                toolkit.SetFont("Helvetica", 72);
                toolkit.SetTextColor(200, 200, 200);
                toolkit.PrintText(150, 400, watermarkText);
            }
            toolkit.CloseInputFile();
            toolkit.CloseOutputFile();
        }
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

**Before (ActivePDF WebGrabber):**
```csharp
using APWebGrabber;
using System.IO;

public void CreateCustomSizePdf(string html, string outputPath)
{
    var wg = new WebGrabber();
    string tempHtml = Path.Combine(Path.GetTempPath(), "input.html");
    File.WriteAllText(tempHtml, html);

    wg.URL = tempHtml;
    wg.PageWidth = 396;   // points
    wg.PageHeight = 612;  // points (half letter)
    wg.OutputDirectory = Path.GetDirectoryName(outputPath);
    wg.OutputFilename = Path.GetFileName(outputPath);
    wg.ConvertToPDF();
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

**Before (ActivePDF Toolkit Ultimate / Rasterizer SKU):**
```csharp
// Requires ActivePDF.Toolkit.Ultimate (rasterization is not in the base Toolkit).
using APToolkitNET;

public void ConvertToImages(string pdfPath, string outputDir)
{
    using (Toolkit toolkit = new Toolkit())
    {
        if (toolkit.OpenInputFile(pdfPath) == 0)
        {
            int pageCount = toolkit.NumPages;
            for (int i = 1; i <= pageCount; i++)
            {
                toolkit.RenderPage(i, $"{outputDir}/page_{i}.png", 300);
            }
            toolkit.CloseInputFile();
        }
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

**Before (ActivePDF WebGrabber):**
```csharp
using APWebGrabber;
using System.IO;

public void BatchConvert(string[] htmlFiles, string outputDir)
{
    var wg = new WebGrabber();
    wg.OutputDirectory = outputDir;

    foreach (var htmlFile in htmlFiles)
    {
        wg.URL = htmlFile;
        wg.OutputFilename = Path.GetFileNameWithoutExtension(htmlFile) + ".pdf";
        wg.ConvertToPDF();
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

**ActivePDF Pattern (WebGrabber):**
```csharp
[HttpPost]
public IActionResult GeneratePdf([FromBody] ReportRequest request)
{
    var wg = new APWebGrabber.WebGrabber();
    string tempHtml = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".html");
    System.IO.File.WriteAllText(tempHtml, request.Html);

    wg.URL = tempHtml;
    wg.OutputDirectory = Path.GetTempPath();
    wg.OutputFilename = "temp.pdf";

    if (wg.ConvertToPDF() == 0)
    {
        byte[] bytes = System.IO.File.ReadAllBytes(Path.Combine(Path.GetTempPath(), "temp.pdf"));
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
using APToolkitNET;

using (var toolkit = new Toolkit())
{
    int result = toolkit.OpenOutputFile(path);

    if (result != 0)
    {
        // Error - look up the code in "Toolkit Return Results and Error Codes"
        Console.WriteLine($"Error code: {result}");
    }
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

### Issue 1: "Cannot Find APToolkitNET.dll" or "Core library not loaded"

**Symptom:** FileNotFoundException, or `Core library not loaded Parameter name: Toolkit` after removing ActivePDF
**Cause:** Old DLL/native references remain. From Toolkit 10 onward the native libraries live next to the assembly, not in the system folder.
**Solution:**
```xml
<!-- Remove from .csproj -->
<Reference Include="APToolkitNET">
    <HintPath>..\..\Libs\APToolkitNET.dll</HintPath>
</Reference>
<!-- Also delete any APToolkitNET.* and ap*.dll native files in bin/ -->
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
**Cause:** ActivePDF WebGrabber uses points, IronPDF uses mm or enums
**Solution:**
```csharp
// ActivePDF WebGrabber: Points (612x792 = Letter)
wg.PageWidth = 612;
wg.PageHeight = 792;

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
  grep -rE "APToolkitNET|APWebGrabber|ActivePDF" --include="*.cs" .
  ```
  **Why:** Identify all instances of ActivePDF Toolkit and WebGrabber usage to ensure complete migration coverage.

- [ ] **List all PDF operations in your codebase**
  ```csharp
  // Look for patterns like:
  var toolkit = new APToolkitNET.Toolkit();
  toolkit.OpenInputFile("input.pdf");
  toolkit.OpenOutputFile("output.pdf");
  toolkit.CopyForm(0, 0);
  toolkit.MergeFile("extra.pdf", 1, -1);
  toolkit.CloseOutputFile();

  var wg = new APWebGrabber.WebGrabber();
  wg.URL = "https://example.com";
  wg.ConvertToPDF();
  ```
  **Why:** Understanding all PDF operations helps map them to IronPDF equivalents. Remember Toolkit and WebGrabber are separate SKUs in ActivePDF; both collapse into IronPDF.

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

- [ ] **Remove ActivePDF package references**
  ```bash
  dotnet remove package ActivePDF.Toolkit
  dotnet remove package ActivePDF.WebGrabber
  ```
  **Why:** Clean removal of the old packages to prevent conflicts.

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
  // Before
  using APToolkitNET;
  using APWebGrabber;

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
  // Before (WebGrabber)
  wg.PageWidth = 595;   // A4 width in points
  wg.PageHeight = 842;  // A4 height in points

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** IronPDF uses enums for standard page sizes, simplifying configuration.

- [ ] **Convert method calls per API mapping**
  ```csharp
  // Before (APToolkitNET)
  toolkit.MergeFile("input.pdf", 1, -1);

  // After (IronPDF)
  var merged = PdfDocument.Merge(PdfDocument.FromFile("input.pdf"));
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
