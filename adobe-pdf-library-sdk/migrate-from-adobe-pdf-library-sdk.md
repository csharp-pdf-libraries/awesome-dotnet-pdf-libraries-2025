# How Do I Migrate from Adobe PDF Library SDK to IronPDF in C#?

> **Migration Complexity:** High
> **Estimated Time:** 4-8 hours for typical projects
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

Adobe PDF Library SDK (via Datalogics) is an enterprise-grade PDF engine with Adobe heritage. However, several factors make IronPDF a compelling alternative for most development teams.

### The Case for Leaving Adobe PDF Library SDK

**Prohibitive Licensing Costs**: Adobe PDF Library SDK is priced at enterprise levels, often reaching tens of thousands of dollars annually. This pricing model makes it impractical for small to mid-sized businesses, startups, or individual developers.

**Complex Native SDK Integration**: The SDK is built on native C++ code requiring platform-specific binaries, careful memory management, and explicit initialization/termination patterns. This adds significant development overhead.

**Overkill for Most Projects**: The SDK provides the full Adobe PDF engine—powerful but excessive for projects that primarily need HTML-to-PDF conversion, basic manipulation, or document generation.

**Low-Level API Design**: Creating PDFs requires constructing pages, content streams, text runs, and fonts programmatically. Simple tasks like "render this HTML" become complex multi-step operations.

**Library Lifecycle Management**: Every operation requires wrapping code in `Library.Initialize()` / `Library.Terminate()` blocks, with careful COM object disposal.

### What IronPDF Provides

The transition from Adobe's complex enterprise SDK to IronPDF's streamlined approach, as outlined in the [detailed walkthrough](https://ironpdf.com/blog/migration-guides/migrate-from-adobe-pdf-library-sdk-to-ironpdf/), eliminates many common integration challenges.

| Adobe PDF Library Limitation | IronPDF Solution |
|------------------------------|------------------|
| Enterprise pricing ($10K-$50K+/year) | Affordable per-developer licensing |
| Native SDK, platform-specific builds | Pure .NET, cross-platform NuGet |
| Manual page/content construction | HTML/CSS rendering engine |
| Explicit Library.Initialize/Terminate | Automatic initialization |
| Complex coordinate-based layout | Standard web layout model |
| Manual font embedding | Automatic font handling |

---

## Before You Start

### Prerequisites

- .NET Framework 4.6.2+ or .NET Core 3.1+ / .NET 5-9
- Visual Studio 2019+ or JetBrains Rider
- NuGet Package Manager access
- IronPDF license key (free trial available at [ironpdf.com](https://ironpdf.com))

### Find All Adobe PDF Library References

Run these commands in your solution directory:

```bash
grep -r "using Datalogics" --include="*.cs" .
grep -r "Adobe.PDF.Library" --include="*.csproj" .
grep -r "Library.Initialize\|Library.Terminate" --include="*.cs" .
```

### Breaking Changes to Anticipate

| Category | Adobe PDF Library | IronPDF | Migration Action |
|----------|-------------------|---------|------------------|
| Initialization | `Library.Initialize()` / `Terminate()` | Automatic | Remove lifecycle code |
| Document Creation | `new Document()` with page construction | `ChromePdfRenderer` | Use HTML rendering |
| Coordinate System | PostScript points, bottom-left origin | CSS-based layout | Use HTML/CSS |
| Font Handling | Manual `Font` creation and embedding | Automatic | Remove font code |
| Memory Management | Manual disposal of COM objects | Standard IDisposable | Use `using` statements |
| Page Construction | `CreatePage()`, `AddContent()` | Automatic from HTML | Simplify significantly |

---

## Quick Start (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove Adobe PDF Library
dotnet remove package Adobe.PDF.Library.LM.NET

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Set Your License Key

```csharp
// Replace Adobe's Library.LicenseKey with IronPDF license
// Add this at application startup, before any IronPDF operations
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 3: Global Find & Replace

| Find | Replace With |
|------|--------------|
| `using Datalogics.PDFL;` | `using IronPdf;` |
| `using Datalogics.PDFL.Document;` | `using IronPdf;` |
| `using Datalogics.PDFL.Page;` | `using IronPdf;` |
| `using Datalogics.PDFL.Content;` | `using IronPdf;` |

### Step 4: Verify Basic Operation

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

Library.LicenseKey = "ADOBE-KEY";
Library.Initialize();
try
{
    using (Document doc = new Document())
    {
        Rect pageRect = new Rect(0, 0, 612, 792);
        using (Page page = doc.CreatePage(Document.BeforeFirstPage, pageRect))
        {
            Content content = page.Content;
            Font font = new Font("Arial", FontCreateFlags.Embedded);
            Text text = new Text();
            text.AddRun(new TextRun("Hello World", font, 24, new Point(100, 700)));
            content.AddElement(text);
            page.UpdateContent();
        }
        doc.Save(SaveFlags.Full, "output.pdf");
    }
}
finally
{
    Library.Terminate();
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

| Adobe PDF Library Namespace | IronPDF Namespace | Purpose |
|-----------------------------|-------------------|---------|
| `Datalogics.PDFL` | `IronPdf` | Core functionality |
| `Datalogics.PDFL.Document` | `IronPdf` | Document operations |
| `Datalogics.PDFL.Page` | `IronPdf` | Page operations |
| `Datalogics.PDFL.Content` | `IronPdf.Editing` | Content manipulation |
| `Datalogics.PDFL.Text` | `IronPdf` | Text operations |
| `Datalogics.PDFL.Image` | `IronPdf.Editing` | Image operations |
| `Datalogics.PDFL.Font` | Not needed | Automatic font handling |
| `Datalogics.PDFL.Color` | Use HTML/CSS | Color via CSS |

### Library Lifecycle Methods

| Adobe Method | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `Library.Initialize()` | Not needed | Automatic |
| `Library.Terminate()` | Not needed | Automatic |
| `Library.LicenseKey = "KEY"` | `IronPdf.License.LicenseKey = "KEY"` | Set once at startup |
| `using (Library lib = new Library())` | Not needed | No wrapper required |

### Document Creation Methods

| Adobe Method | IronPDF Method | Notes |
|--------------|----------------|-------|
| `new Document()` | `new ChromePdfRenderer()` | Renderer for HTML |
| `new Document(path)` | `PdfDocument.FromFile(path)` | Load existing PDF |
| `doc.CreatePage(index, rect)` | Automatic from HTML | Pages auto-created |
| `doc.Save(flags, path)` | `pdf.SaveAs(path)` | Save to file |
| `doc.NumPages` | `pdf.PageCount` | Page count |
| `doc.GetPage(index)` | `pdf.Pages[index]` | Access page |
| `doc.InsertPages(...)` | `PdfDocument.Merge()` | Merge documents |
| `doc.DeletePages(...)` | `pdf.RemovePages(index)` | Remove pages |

### Page Operations

| Adobe Method | IronPDF Method | Notes |
|--------------|----------------|-------|
| `page.Content` | Use HTML | Content via HTML |
| `page.UpdateContent()` | Not needed | Automatic |
| `page.Rotate(degrees)` | `pdf.Pages[i].Rotation` | Per-page rotation |
| `page.MediaBox` | `RenderingOptions.PaperSize` | Page dimensions |
| `page.CropBox` | Use CSS margins | Crop via CSS |
| `page.AddAnnotation(...)` | `pdf.Annotations` | Annotation API |

### Content Creation

| Adobe Method | IronPDF Method | Notes |
|--------------|----------------|-------|
| `new Text()` | Use HTML `<p>`, `<h1>`, etc. | HTML tags |
| `text.AddRun(textRun)` | Use HTML | Text via HTML |
| `new TextRun(text, font, size, point)` | CSS styling | Style via CSS |
| `new Font(name, flags)` | CSS `font-family` | Fonts via CSS |
| `new Image(path)` | HTML `<img>` tag | Images via HTML |
| `content.AddElement(...)` | HTML content | Build with HTML |

### Watermark & Stamping

| Adobe Method | IronPDF Method | Notes |
|--------------|----------------|-------|
| `new Watermark(doc, textParams, wmParams)` | `pdf.ApplyWatermark(html)` | HTML watermark |
| `WatermarkParams.Opacity` | CSS `opacity` | Opacity via CSS |
| `WatermarkParams.Rotation` | `rotation` parameter | Rotation angle |
| Manual page iteration | Automatic all pages | Applied globally |

### Security & Encryption

| Adobe Method | IronPDF Method | Notes |
|--------------|----------------|-------|
| `new EncryptionHandler(user, owner, perms)` | `pdf.SecuritySettings` | Security config |
| `doc.SetEncryptionHandler(handler)` | Set properties | Apply settings |
| `PermissionFlags.PrintDoc` | `AllowUserPrinting` | Print permission |
| `PermissionFlags.EditDoc` | `AllowUserEdits` | Edit permission |
| `PermissionFlags.CopyContent` | `AllowUserCopyPasteContent` | Copy permission |

### Text Extraction

| Adobe Method | IronPDF Method | Notes |
|--------------|----------------|-------|
| `new WordFinder(doc, config)` | `pdf.ExtractAllText()` | Simple extraction |
| `wordFinder.GetWordList()` | `pdf.Pages[i].Text` | Per-page text |
| Complex word/character iteration | Single method call | Much simpler |

### Configuration Options

| Adobe Setting | IronPDF Equivalent | Default |
|---------------|-------------------|---------|
| `Rect(0, 0, 612, 792)` Letter | `PaperSize = PdfPaperSize.Letter` | A4 |
| `Rect(0, 0, 595, 842)` A4 | `PaperSize = PdfPaperSize.A4` | A4 |
| `SaveFlags.Full` | Default save behavior | Full save |
| `SaveFlags.Incremental` | Not applicable | Full save |
| `FontCreateFlags.Embedded` | Automatic | Fonts embedded |

---

## Code Migration Examples

### Example 1: HTML to PDF (Most Common)

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

public void CreatePdfFromContent()
{
    Library.LicenseKey = "ADOBE-KEY";
    Library.Initialize();
    try
    {
        using (Document doc = new Document())
        {
            Rect pageRect = new Rect(0, 0, 612, 792);
            using (Page page = doc.CreatePage(Document.BeforeFirstPage, pageRect))
            {
                Content content = page.Content;

                // Create and configure font
                Font titleFont = new Font("Arial", FontCreateFlags.Embedded);
                Font bodyFont = new Font("Times New Roman", FontCreateFlags.Embedded);

                // Add title
                Text title = new Text();
                title.AddRun(new TextRun("Document Title", titleFont, 24,
                    new Point(72, 720)));
                content.AddElement(title);

                // Add paragraph
                Text body = new Text();
                body.AddRun(new TextRun("This is the body text of the document.",
                    bodyFont, 12, new Point(72, 680)));
                content.AddElement(body);

                page.UpdateContent();
            }
            doc.Save(SaveFlags.Full, "output.pdf");
        }
    }
    finally
    {
        Library.Terminate();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreatePdfFromContent()
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;

    string html = @"
        <html>
        <head>
            <style>
                body { font-family: 'Times New Roman', serif; margin: 1in; }
                h1 { font-family: Arial, sans-serif; font-size: 24pt; }
                p { font-size: 12pt; }
            </style>
        </head>
        <body>
            <h1>Document Title</h1>
            <p>This is the body text of the document.</p>
        </body>
        </html>";

    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
```

### Example 2: Merge Multiple PDFs

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

public void MergePdfs(string[] inputFiles, string outputPath)
{
    Library.Initialize();
    try
    {
        using (Document mergedDoc = new Document(inputFiles[0]))
        {
            for (int i = 1; i < inputFiles.Length; i++)
            {
                using (Document doc = new Document(inputFiles[i]))
                {
                    mergedDoc.InsertPages(
                        Document.LastPage,
                        doc,
                        0,
                        Document.AllPages,
                        PageInsertFlags.None);
                }
            }
            mergedDoc.Save(SaveFlags.Full, outputPath);
        }
    }
    finally
    {
        Library.Terminate();
    }
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

### Example 3: Add Watermark

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

public void AddWatermark(string inputPath, string outputPath)
{
    Library.Initialize();
    try
    {
        using (Document doc = new Document(inputPath))
        {
            WatermarkParams wmParams = new WatermarkParams();
            wmParams.Opacity = 0.5;
            wmParams.Rotation = 45.0;
            wmParams.VerticalAlignment = WatermarkVerticalAlignment.Center;
            wmParams.HorizontalAlignment = WatermarkHorizontalAlignment.Center;
            wmParams.Scale = -1; // Auto scale

            WatermarkTextParams textParams = new WatermarkTextParams();
            textParams.Text = "CONFIDENTIAL";
            textParams.Color = new Datalogics.PDFL.Color(0.8, 0.8, 0.8);
            textParams.FontName = "Helvetica";
            textParams.FontSize = 72;

            Watermark watermark = new Watermark(doc, textParams, wmParams);

            doc.Save(SaveFlags.Full, outputPath);
        }
    }
    finally
    {
        Library.Terminate();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void AddWatermark(string inputPath, string outputPath)
{
    using var pdf = PdfDocument.FromFile(inputPath);

    pdf.ApplyWatermark(
        "<h1 style='color:lightgray;font-size:72px;font-family:Helvetica;'>CONFIDENTIAL</h1>",
        rotation: 45,
        opacity: 50);

    pdf.SaveAs(outputPath);
}
```

### Example 4: Password Protection

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

public void ProtectPdf(string inputPath, string outputPath, string password)
{
    Library.Initialize();
    try
    {
        using (Document doc = new Document(inputPath))
        {
            PermissionFlags permissions =
                PermissionFlags.PrintDoc |
                PermissionFlags.PrintFidelity;

            EncryptionHandler encHandler = new EncryptionHandler(
                password,      // User password
                password,      // Owner password
                permissions,
                EncryptionMethod.AES256);

            doc.SetEncryptionHandler(encHandler);
            doc.Save(SaveFlags.Full | SaveFlags.Encrypted, outputPath);
        }
    }
    finally
    {
        Library.Terminate();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void ProtectPdf(string inputPath, string outputPath, string password)
{
    using var pdf = PdfDocument.FromFile(inputPath);

    pdf.SecuritySettings.UserPassword = password;
    pdf.SecuritySettings.OwnerPassword = password;
    pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;
    pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

    pdf.SaveAs(outputPath);
}
```

### Example 5: Extract Text

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

public string ExtractText(string pdfPath)
{
    string extractedText = "";

    Library.Initialize();
    try
    {
        using (Document doc = new Document(pdfPath))
        {
            WordFinderConfig config = new WordFinderConfig();
            config.IgnoreCharGaps = true;

            for (int i = 0; i < doc.NumPages; i++)
            {
                using (WordFinder wordFinder = new WordFinder(doc, i, config))
                {
                    IList<Word> words = wordFinder.GetWordList();
                    foreach (Word word in words)
                    {
                        extractedText += word.Text + " ";
                    }
                    extractedText += "\n";
                }
            }
        }
    }
    finally
    {
        Library.Terminate();
    }

    return extractedText;
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

### Example 6: Add Headers and Footers

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

public void AddHeaderFooter(string inputPath, string outputPath)
{
    Library.Initialize();
    try
    {
        using (Document doc = new Document(inputPath))
        {
            Font font = new Font("Helvetica", FontCreateFlags.None);

            for (int i = 0; i < doc.NumPages; i++)
            {
                using (Page page = doc.GetPage(i))
                {
                    Content content = page.Content;

                    // Add header
                    Text header = new Text();
                    header.AddRun(new TextRun("Document Header",
                        font, 10, new Point(72, page.MediaBox.Top - 36)));
                    content.AddElement(header);

                    // Add footer with page number
                    Text footer = new Text();
                    footer.AddRun(new TextRun($"Page {i + 1} of {doc.NumPages}",
                        font, 10, new Point(72, 36)));
                    content.AddElement(footer);

                    page.UpdateContent();
                }
            }
            doc.Save(SaveFlags.Full, outputPath);
        }
    }
    finally
    {
        Library.Terminate();
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
        CenterText = "Document Header",
        FontSize = 10,
        FontFamily = "Helvetica"
    };

    renderer.RenderingOptions.TextFooter = new TextHeaderFooter
    {
        CenterText = "Page {page} of {total-pages}",
        FontSize = 10,
        FontFamily = "Helvetica"
    };

    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 7: URL to PDF

**Before (Adobe PDF Library SDK):**
```csharp
// Adobe PDF Library doesn't have built-in URL rendering
// Would require external HTML renderer + conversion
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

### Example 8: Custom Page Size

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

public void CreateCustomSizePdf()
{
    Library.Initialize();
    try
    {
        using (Document doc = new Document())
        {
            // Custom size in points (5.5" x 8.5")
            Rect pageRect = new Rect(0, 0, 396, 612);
            using (Page page = doc.CreatePage(Document.BeforeFirstPage, pageRect))
            {
                // Add content...
                page.UpdateContent();
            }
            doc.Save(SaveFlags.Full, "custom.pdf");
        }
    }
    finally
    {
        Library.Terminate();
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

    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 9: Add Image to PDF

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

public void AddImageToPdf(string imagePath)
{
    Library.Initialize();
    try
    {
        using (Document doc = new Document())
        {
            Rect pageRect = new Rect(0, 0, 612, 792);
            using (Page page = doc.CreatePage(Document.BeforeFirstPage, pageRect))
            {
                Content content = page.Content;

                Datalogics.PDFL.Image image = new Datalogics.PDFL.Image(imagePath);
                Matrix imageMatrix = new Matrix();
                imageMatrix.Scale(300, 200); // Width, height in points
                imageMatrix.Translate(72, 500); // Position

                content.AddElement(image, imageMatrix);
                page.UpdateContent();
            }
            doc.Save(SaveFlags.Full, "with-image.pdf");
        }
    }
    finally
    {
        Library.Terminate();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void AddImageToPdf(string imagePath, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    string html = $@"
        <html>
        <body>
            <img src='{imagePath}' style='width:300px; height:200px;' />
        </body>
        </html>";

    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 10: Batch Processing

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

public void BatchProcess(string[] files, string outputDir)
{
    Library.Initialize();
    try
    {
        foreach (var file in files)
        {
            using (Document doc = new Document())
            {
                // Complex page/content construction for each file...
                string outputPath = Path.Combine(outputDir,
                    Path.GetFileNameWithoutExtension(file) + ".pdf");
                doc.Save(SaveFlags.Full, outputPath);
            }
        }
    }
    finally
    {
        Library.Terminate();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.IO;

public void BatchProcess(string[] htmlFiles, string outputDir)
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

### Scenario 1: ASP.NET Core Integration

**Adobe Pattern:**
```csharp
// Adobe requires static Library.Initialize() which doesn't work well with DI
public class AdobePdfService
{
    public byte[] Generate(string content)
    {
        Library.Initialize();
        try
        {
            // Complex document construction...
            return bytes;
        }
        finally
        {
            Library.Terminate();
        }
    }
}
```

**IronPDF Pattern:**
```csharp
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

// Register in Program.cs:
builder.Services.AddSingleton<IPdfService, IronPdfService>();
```

### Scenario 2: Async PDF Generation

Adobe PDF Library doesn't support async operations. **IronPDF** does:

```csharp
using IronPdf;

public async Task<byte[]> GeneratePdfAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

### Scenario 3: Docker Deployment

**Adobe PDF Library** requires platform-specific native binaries and complex Docker configuration.

**IronPDF** works seamlessly:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# IronPDF works out of the box
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### Scenario 4: Error Handling

**Adobe Error Handling:**
```csharp
Library.Initialize();
try
{
    using (Document doc = new Document(path))
    {
        // Operations...
    }
}
catch (ApplicationException ex)
{
    // Adobe-specific exception
}
finally
{
    Library.Terminate(); // Must always terminate
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
```

---

## Performance Considerations

### Memory Usage Comparison

| Scenario | Adobe PDF Library | IronPDF | Notes |
|----------|-------------------|---------|-------|
| Simple PDF | ~100 MB | ~50 MB | Adobe loads full engine |
| Complex document | ~200 MB | ~80 MB | IronPDF more efficient |
| Batch (100 PDFs) | High (native memory) | ~100 MB | IronPDF better managed |

### Optimization Tips

**1. No Library Lifecycle Overhead:**
```csharp
// IronPDF: No Initialize/Terminate needed
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

**3. Reuse Renderer Instance:**
```csharp
// Good: Reuse renderer
var renderer = new ChromePdfRenderer();
// Use for multiple renders
```

---

## Troubleshooting Guide

### Issue 1: "Library Not Initialized" Equivalent

**Symptom:** Looking for Initialize/Terminate pattern
**Cause:** Adobe pattern not needed
**Solution:**
```csharp
// Adobe
Library.Initialize();
try { /* code */ }
finally { Library.Terminate(); }

// IronPDF - just use directly
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 2: Coordinate-Based Positioning Not Working

**Symptom:** Code uses `Point(x, y)` positioning
**Cause:** IronPDF uses HTML/CSS layout
**Solution:**
```csharp
// Adobe: Point-based
new TextRun("Hello", font, 12, new Point(100, 700));

// IronPDF: CSS-based
string html = "<p style='position:absolute; left:100px; top:92px;'>Hello</p>";
```

### Issue 3: Font Not Found

**Symptom:** Missing font errors
**Cause:** Adobe requires manual font embedding
**Solution:**
```csharp
// IronPDF handles fonts automatically
// Use web fonts if needed:
string html = @"
<style>
    @import url('https://fonts.googleapis.com/css2?family=Roboto&display=swap');
    body { font-family: 'Roboto', sans-serif; }
</style>";
```

### Issue 4: Page Size Different

**Symptom:** Output dimensions wrong
**Cause:** Adobe uses points, IronPDF uses standard sizes
**Solution:**
```csharp
// Adobe: Points
Rect(0, 0, 612, 792) // Letter

// IronPDF: Enum or custom
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
// Or custom:
renderer.RenderingOptions.SetCustomPaperSizeInInches(8.5, 11);
```

### Issue 5: License Key Not Working

**Symptom:** Watermark or license error
**Cause:** License not set at startup
**Solution:**
```csharp
// Set BEFORE any IronPDF operations
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Verify
bool isLicensed = IronPdf.License.IsLicensed;
```

### Issue 6: Native Library Dependencies Missing

**Symptom:** Adobe DLL loading errors after removal
**Cause:** Old references remain
**Solution:**
- Remove all Adobe PDF Library DLLs from project
- Clean and rebuild solution
- Remove references from .csproj

### Issue 7: Complex Content Construction

**Symptom:** Complex Text/Content/Element code
**Cause:** Low-level API translation
**Solution:**
```csharp
// Adobe: Many lines of content construction
Content content = page.Content;
Text text = new Text();
text.AddRun(new TextRun("Hello", font, 12, point));
content.AddElement(text);
page.UpdateContent();

// IronPDF: HTML
renderer.RenderHtmlAsPdf("<p>Hello</p>");
```

### Issue 8: SaveFlags Not Available

**Symptom:** Looking for SaveFlags equivalents
**Cause:** Different save model
**Solution:**
```csharp
// Adobe
doc.Save(SaveFlags.Full | SaveFlags.Incremental, path);

// IronPDF - full save is default
pdf.SaveAs(path);
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit Adobe PDF Library usage**
  ```bash
  grep -r "Datalogics.PDFL" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Identify all PDF operations in codebase**
  ```bash
  grep -r "Document\|Page\|Content" --include="*.cs" .
  ```
  **Why:** Ensure all PDF-related operations are accounted for during migration.

- [ ] **Document current coordinate-based layouts**
  ```csharp
  // Before (.)
  page.Graphics.DrawText("Hello World", font, brush, x, y);

  // After (IronPDF)
  var html = "<div style='position:absolute; left:x; top:y;'>Hello World</div>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** Transition from coordinate-based layouts to HTML/CSS for easier maintenance and flexibility.

- [ ] **Map content construction to HTML equivalents**
  **Why:** Simplifies PDF generation by leveraging HTML/CSS, which is more intuitive and flexible than manual content construction.

- [ ] **Obtain IronPDF trial license**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Backup codebase**
  **Why:** Ensure you have a recovery point before making significant changes.

### During Migration

- [ ] **Remove Adobe.PDF.Library NuGet package**
  ```bash
  dotnet remove package Adobe.PDF.Library
  ```
  **Why:** Clean removal of the old library to prevent conflicts.

- [ ] **Remove manual DLL references**
  **Why:** Eliminate dependencies on platform-specific binaries for a cleaner setup.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add the new library to enable PDF functionalities.

- [ ] **Set license key at application startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Remove `Library.Initialize()` / `Terminate()` blocks**
  ```csharp
  // Before (.)
  Library.Initialize();
  // PDF operations
  Library.Terminate();

  // After (IronPDF)
  // PDF operations
  ```
  **Why:** IronPDF handles initialization automatically, simplifying code.

- [ ] **Convert document creation to HTML rendering**
  ```csharp
  // Before (.)
  var doc = new Document();
  var page = doc.CreatePage();
  page.Graphics.DrawText("Hello World", font, brush, x, y);

  // After (IronPDF)
  var html = "<div>Hello World</div>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** HTML rendering is more straightforward and modern.

- [ ] **Replace coordinate-based content with HTML/CSS**
  ```csharp
  // Before (.)
  page.Graphics.DrawRectangle(pen, x, y, width, height);

  // After (IronPDF)
  var html = "<div style='width:widthpx; height:heightpx; border:1px solid black;'></div>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** HTML/CSS provides a more flexible and powerful layout model.

- [ ] **Update font handling (remove manual embedding)**
  ```csharp
  // Before (.)
  var font = new Font("Arial", 12, FontStyle.Regular);

  // After (IronPDF)
  var html = "<div style='font-family: Arial; font-size: 12px;'>Text</div>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** IronPDF automatically handles fonts, simplifying code.

- [ ] **Update encryption/security settings**
  ```csharp
  // Before (.)
  doc.Security.Encrypt("userPassword", "ownerPassword", permissions);

  // After (IronPDF)
  pdf.SecuritySettings.UserPassword = "userPassword";
  pdf.SecuritySettings.OwnerPassword = "ownerPassword";
  pdf.SecuritySettings.Permissions = PdfPermissions.AllowPrint;
  ```
  **Why:** IronPDF provides a straightforward API for security settings.

### Post-Migration

- [ ] **Run all existing tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF outputs visually**
  **Why:** Ensure the new PDFs meet quality expectations and maintain consistency.

- [ ] **Test all PDF workflows**
  **Why:** Confirm all PDF-related functionalities work as intended.

- [ ] **Verify licensing works correctly**
  **Why:** Ensure the application runs without license-related issues.

- [ ] **Performance benchmark**
  **Why:** Identify any performance changes due to the new library.

- [ ] **Remove Adobe licensing configuration**
  **Why:** Clean up any legacy configurations no longer needed.

- [ ] **Update CI/CD pipeline**
  **Why:** Ensure the build process includes the new library and configurations.

- [ ] **Document new patterns for team**
  **Why:** Facilitate team understanding and adoption of new practices.
---

## Additional Resources

- **IronPDF Documentation**: [https://ironpdf.com/docs/](https://ironpdf.com/docs/)
- **IronPDF Tutorials**: [https://ironpdf.com/tutorials/](https://ironpdf.com/tutorials/)
- **HTML to PDF Guide**: [https://ironpdf.com/how-to/html-file-to-pdf/](https://ironpdf.com/how-to/html-file-to-pdf/)
- **API Reference**: [https://ironpdf.com/object-reference/api/](https://ironpdf.com/object-reference/api/)
- **NuGet Package**: [https://www.nuget.org/packages/IronPdf/](https://www.nuget.org/packages/IronPdf/)

---

*This migration guide was created to help developers transition from Adobe PDF Library SDK to IronPDF efficiently.*
