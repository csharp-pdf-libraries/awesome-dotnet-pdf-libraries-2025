# How Do I Migrate from Apache PDFBox (.NET Ports) to IronPDF in C#?

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

Apache PDFBox is a well-respected Java library, but its unofficial .NET ports present significant challenges for .NET developers.

### The Case for Leaving Apache PDFBox .NET Ports

**Unofficial Port Status**: PDFBox is fundamentally a Java library. All .NET versions are community-driven ports that lack official support from the Apache project. These ports frequently lag behind Java releases and may miss critical features or security updates.

**Java-First API Design**: The ported API retains Java conventions (`camelCase` methods, `File` objects, explicit `close()` calls) that feel foreign in .NET code, a complexity addressed comprehensively in the [step-by-step guide](https://ironpdf.com/blog/migration-guides/migrate-from-apache-pdfbox-to-ironpdf/). This cognitive overhead affects development velocity and code quality.

**No HTML Rendering**: PDFBox is designed for PDF manipulation, not HTML-to-PDF conversion. Creating PDFs requires manual page construction with precise coordinate positioning—a tedious and error-prone process.

**Limited Community Support**: The .NET ecosystem around PDFBox ports is sparse. Finding help, examples, or best practices for .NET-specific issues is difficult.

**Potential JVM Dependencies**: Some ports may require Java runtime components, adding complexity to deployment and environment management.

### What IronPDF Provides

| PDFBox .NET Port Limitation | IronPDF Solution |
|-----------------------------|------------------|
| Unofficial, community-driven | Native .NET, professionally supported |
| Java-style API conventions | Idiomatic C# with modern patterns |
| No HTML rendering capability | Full Chromium-based HTML/CSS/JS rendering |
| Manual coordinate positioning | CSS-based layout |
| Explicit close() calls | IDisposable with `using` statements |
| Sparse .NET community | Active .NET community, 10M+ downloads |

---

## Before You Start

### Prerequisites

- .NET Framework 4.6.2+ or .NET Core 3.1+ / .NET 5-9
- Visual Studio 2019+ or JetBrains Rider
- NuGet Package Manager access
- IronPDF license key (free trial available at [ironpdf.com](https://ironpdf.com))

### Find All PDFBox References

Run these commands in your solution directory:

```bash
grep -r "apache.pdfbox\|PdfBox\|PDDocument\|PDFTextStripper" --include="*.cs" .
grep -r "PdfBox\|Apache.PdfBox" --include="*.csproj" .
```

### Breaking Changes to Anticipate

| Category | PDFBox .NET Port | IronPDF | Migration Action |
|----------|------------------|---------|------------------|
| Object Model | `PDDocument`, `PDPage` | `PdfDocument`, `ChromePdfRenderer` | Different class hierarchy |
| PDF Creation | Manual page/content streams | HTML rendering | Rewrite creation logic |
| Method Style | `camelCase()` (Java style) | `PascalCase()` (.NET style) | Update method names |
| Resource Cleanup | `document.close()` | `using` statements | Change disposal pattern |
| File Access | Java `File` objects | Standard .NET strings/streams | Use .NET types |
| Text Extraction | `PDFTextStripper` class | `pdf.ExtractAllText()` | Simpler API |

---

## Quick Start (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove PDFBox .NET port packages
dotnet remove package PdfBox
dotnet remove package PDFBoxNet
dotnet remove package Apache.PdfBox

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Set Your License Key

```csharp
// Add this at application startup, before any IronPDF operations
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 3: Global Find & Replace

| Find | Replace With |
|------|--------------|
| `using org.apache.pdfbox.pdmodel;` | `using IronPdf;` |
| `using org.apache.pdfbox.text;` | `using IronPdf;` |
| `using org.apache.pdfbox.multipdf;` | `using IronPdf;` |
| `using PdfBoxDotNet.Pdmodel;` | `using IronPdf;` |
| `using Apache.Pdfbox.PdModel;` | `using IronPdf;` |

### Step 4: Verify Basic Operation

**Before (PDFBox .NET Port):**
```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.text;

PDDocument document = PDDocument.load("input.pdf");
PDFTextStripper stripper = new PDFTextStripper();
string text = stripper.getText(document);
Console.WriteLine(text);
document.close();
```

**After (IronPDF):**
```csharp
using IronPdf;

using var pdf = PdfDocument.FromFile("input.pdf");
string text = pdf.ExtractAllText();
Console.WriteLine(text);
```

---

## Complete API Reference

### Namespace Mapping

| PDFBox .NET Port Namespace | IronPDF Namespace | Purpose |
|---------------------------|-------------------|---------|
| `org.apache.pdfbox.pdmodel` | `IronPdf` | Core document operations |
| `org.apache.pdfbox.text` | `IronPdf` | Text extraction |
| `org.apache.pdfbox.multipdf` | `IronPdf` | Merge/split operations |
| `org.apache.pdfbox.rendering` | `IronPdf` | PDF to image |
| `org.apache.pdfbox.pdmodel.encryption` | `IronPdf` | Security/encryption |
| `org.apache.pdfbox.pdmodel.font` | Not needed | Automatic font handling |
| `org.apache.pdfbox.pdmodel.graphics` | Use HTML/CSS | Graphics via HTML |

### Document Operations

| PDFBox Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `PDDocument.load(path)` | `PdfDocument.FromFile(path)` | Load PDF |
| `PDDocument.load(stream)` | `PdfDocument.FromStream(stream)` | Load from stream |
| `new PDDocument()` | `new ChromePdfRenderer()` | For creating PDFs |
| `document.save(path)` | `pdf.SaveAs(path)` | Save PDF |
| `document.close()` | `using` statement or `Dispose()` | Cleanup |
| `document.getNumberOfPages()` | `pdf.PageCount` | Page count |
| `document.getPage(index)` | `pdf.Pages[index]` | Access page |
| `document.addPage(page)` | Automatic from HTML | Pages auto-created |
| `document.removePage(index)` | `pdf.RemovePages(index)` | Remove pages |

### Text Extraction

| PDFBox Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `new PDFTextStripper()` | Not needed | No stripper object |
| `stripper.getText(document)` | `pdf.ExtractAllText()` | Full document |
| `stripper.setStartPage(n)` | `pdf.Pages[n].Text` | Per-page extraction |
| `stripper.setEndPage(n)` | Loop through pages | Page range |
| `stripper.setSortByPosition(true)` | Automatic | Built-in sorting |

### Merge & Split Operations

| PDFBox Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `new PDFMergerUtility()` | Not needed | Static merge method |
| `merger.addSource(file)` | Load with `FromFile()` | Load documents first |
| `merger.mergeDocuments()` | `PdfDocument.Merge(pdfs)` | Static merge |
| `new Splitter()` | Not needed | Direct page manipulation |
| `splitter.split(document)` | `pdf.CopyPages(indices)` | Copy specific pages |

### PDF Creation (HTML-based)

| PDFBox Pattern | IronPDF Method | Notes |
|----------------|----------------|-------|
| Manual `PDPage` creation | `renderer.RenderHtmlAsPdf(html)` | HTML rendering |
| `PDPageContentStream` | Use HTML/CSS | Content via HTML |
| `contentStream.beginText()` | HTML `<p>`, `<span>` | Text via HTML |
| `contentStream.drawString()` | HTML content | Text via HTML |
| `contentStream.setFont()` | CSS `font-family` | Fonts via CSS |
| Manual coordinate positioning | CSS positioning | Layout via CSS |

### Security & Encryption

| PDFBox Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `StandardProtectionPolicy` | `pdf.SecuritySettings` | Security config |
| `policy.setUserPassword()` | `pdf.SecuritySettings.UserPassword` | User password |
| `policy.setOwnerPassword()` | `pdf.SecuritySettings.OwnerPassword` | Owner password |
| `policy.setPermissions()` | `pdf.SecuritySettings.AllowUserXxx` | Permissions |
| `document.protect(policy)` | Set properties, then save | Apply on save |

### PDF to Image

| PDFBox Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `new PDFRenderer(document)` | Not needed | Direct method |
| `renderer.renderImage(pageIndex)` | `pdf.ToBitmap(dpi)` | All pages |
| `renderer.renderImageWithDPI(idx, dpi)` | `pdf.ToBitmap(dpi)` | With DPI |
| Manual image saving | `bitmap.Save(path)` | Standard .NET |

### Configuration Options

| PDFBox Setting | IronPDF Equivalent | Default |
|----------------|-------------------|---------|
| `PDPage.LETTER` | `PdfPaperSize.Letter` | A4 |
| `PDPage.A4` | `PdfPaperSize.A4` | A4 |
| Manual page dimensions | `RenderingOptions.PaperSize` | A4 |
| Font embedding | Automatic | Embedded |

---

## Code Migration Examples

### Example 1: Extract Text from PDF

**Before (PDFBox .NET Port):**
```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.text;
using System.IO;

public string ExtractText(string pdfPath)
{
    PDDocument document = null;
    try
    {
        document = PDDocument.load(new File(pdfPath));
        PDFTextStripper stripper = new PDFTextStripper();
        stripper.setSortByPosition(true);
        stripper.setStartPage(1);
        stripper.setEndPage(document.getNumberOfPages());
        return stripper.getText(document);
    }
    finally
    {
        if (document != null)
        {
            document.close();
        }
    }
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

### Example 2: Create PDF (Manual vs HTML)

**Before (PDFBox .NET Port):**
```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdmodel.font;
using org.apache.pdfbox.pdmodel.edit;

public void CreatePdf(string outputPath)
{
    PDDocument document = new PDDocument();
    try
    {
        PDPage page = new PDPage();
        document.addPage(page);

        PDPageContentStream contentStream = new PDPageContentStream(document, page);
        PDFont font = PDType1Font.HELVETICA_BOLD;

        contentStream.beginText();
        contentStream.setFont(font, 24);
        contentStream.moveTextPositionByAmount(72, 700);
        contentStream.drawString("Hello World");
        contentStream.endText();

        contentStream.beginText();
        contentStream.setFont(PDType1Font.HELVETICA, 12);
        contentStream.moveTextPositionByAmount(72, 650);
        contentStream.drawString("This is a paragraph of text.");
        contentStream.endText();

        contentStream.close();
        document.save(outputPath);
    }
    finally
    {
        document.close();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreatePdf(string outputPath)
{
    var renderer = new ChromePdfRenderer();

    string html = @"
        <html>
        <head>
            <style>
                body { font-family: Helvetica, Arial, sans-serif; margin: 1in; }
                h1 { font-size: 24pt; font-weight: bold; }
                p { font-size: 12pt; }
            </style>
        </head>
        <body>
            <h1>Hello World</h1>
            <p>This is a paragraph of text.</p>
        </body>
        </html>";

    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 3: Merge Multiple PDFs

**Before (PDFBox .NET Port):**
```csharp
using org.apache.pdfbox.multipdf;
using org.apache.pdfbox.pdmodel;
using System.IO;
using System.Collections.Generic;

public void MergePdfs(string[] inputFiles, string outputPath)
{
    PDFMergerUtility merger = new PDFMergerUtility();

    foreach (string file in inputFiles)
    {
        merger.addSource(new File(file));
    }

    merger.setDestinationFileName(outputPath);
    merger.mergeDocuments(MemoryUsageSetting.setupMainMemoryOnly());
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

### Example 4: Split PDF

**Before (PDFBox .NET Port):**
```csharp
using org.apache.pdfbox.multipdf;
using org.apache.pdfbox.pdmodel;
using System.Collections.Generic;

public void SplitPdf(string inputPath, string outputDir)
{
    PDDocument document = PDDocument.load(new File(inputPath));
    try
    {
        Splitter splitter = new Splitter();
        List<PDDocument> pages = splitter.split(document);

        int pageNum = 1;
        foreach (PDDocument page in pages)
        {
            page.save($"{outputDir}/page_{pageNum}.pdf");
            page.close();
            pageNum++;
        }
    }
    finally
    {
        document.close();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.IO;

public void SplitPdf(string inputPath, string outputDir)
{
    using var pdf = PdfDocument.FromFile(inputPath);

    for (int i = 0; i < pdf.PageCount; i++)
    {
        using var singlePage = pdf.CopyPage(i);
        singlePage.SaveAs(Path.Combine(outputDir, $"page_{i + 1}.pdf"));
    }
}
```

### Example 5: Add Password Protection

**Before (PDFBox .NET Port):**
```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdmodel.encryption;

public void ProtectPdf(string inputPath, string outputPath, string password)
{
    PDDocument document = PDDocument.load(new File(inputPath));
    try
    {
        AccessPermission ap = new AccessPermission();
        ap.setCanPrint(true);
        ap.setCanExtractContent(false);

        StandardProtectionPolicy spp = new StandardProtectionPolicy(password, password, ap);
        spp.setEncryptionKeyLength(128);

        document.protect(spp);
        document.save(outputPath);
    }
    finally
    {
        document.close();
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

    pdf.SaveAs(outputPath);
}
```

### Example 6: PDF to Images

**Before (PDFBox .NET Port):**
```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.rendering;
using java.awt.image;
using javax.imageio;
using System.IO;

public void ConvertToImages(string pdfPath, string outputDir)
{
    PDDocument document = PDDocument.load(new File(pdfPath));
    try
    {
        PDFRenderer renderer = new PDFRenderer(document);

        for (int i = 0; i < document.getNumberOfPages(); i++)
        {
            BufferedImage image = renderer.renderImageWithDPI(i, 300);
            ImageIO.write(image, "PNG",
                new File($"{outputDir}/page_{i + 1}.png"));
        }
    }
    finally
    {
        document.close();
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

### Example 7: URL to PDF (Not Available in PDFBox)

**Before (PDFBox .NET Port):**
```csharp
// PDFBox does not support URL to PDF conversion
// Would require external HTML renderer + manual PDF construction
// This is NOT possible with PDFBox alone
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

### Example 8: Add Watermark

**Before (PDFBox .NET Port):**
```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdmodel.edit;
using org.apache.pdfbox.pdmodel.font;

public void AddWatermark(string inputPath, string outputPath, string watermarkText)
{
    PDDocument document = PDDocument.load(new File(inputPath));
    try
    {
        PDFont font = PDType1Font.HELVETICA_BOLD;

        for (int i = 0; i < document.getNumberOfPages(); i++)
        {
            PDPage page = document.getPage(i);
            PDPageContentStream cs = new PDPageContentStream(
                document, page, PDPageContentStream.AppendMode.APPEND, true, true);

            cs.beginText();
            cs.setFont(font, 72);
            cs.setNonStrokingColor(200, 200, 200);
            cs.setTextMatrix(Matrix.getRotateInstance(Math.toRadians(45), 200, 400));
            cs.showText(watermarkText);
            cs.endText();
            cs.close();
        }

        document.save(outputPath);
    }
    finally
    {
        document.close();
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

### Example 9: Headers and Footers

**Before (PDFBox .NET Port):**
```csharp
// PDFBox requires manual positioning on each page
// Complex coordinate calculations required
// No built-in header/footer support
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreatePdfWithHeaderFooter(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.TextHeader = new TextHeaderFooter
    {
        CenterText = "Document Title",
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

### Example 10: Batch Processing

**Before (PDFBox .NET Port):**
```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.text;
using System.Collections.Generic;

public Dictionary<string, string> BatchExtractText(string[] pdfFiles)
{
    var results = new Dictionary<string, string>();

    foreach (string file in pdfFiles)
    {
        PDDocument document = null;
        try
        {
            document = PDDocument.load(new File(file));
            PDFTextStripper stripper = new PDFTextStripper();
            results[file] = stripper.getText(document);
        }
        finally
        {
            if (document != null)
                document.close();
        }
    }

    return results;
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;

public Dictionary<string, string> BatchExtractText(string[] pdfFiles)
{
    var results = new Dictionary<string, string>();

    foreach (string file in pdfFiles)
    {
        using var pdf = PdfDocument.FromFile(file);
        results[file] = pdf.ExtractAllText();
    }

    return results;
}
```

---

## Advanced Scenarios

### Scenario 1: ASP.NET Core Web Application

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

PDFBox ports don't support async. **IronPDF** does:

```csharp
using IronPdf;

public async Task<byte[]> GeneratePdfAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

### Scenario 3: Dependency Injection

```csharp
public interface IPdfService
{
    Task<byte[]> GeneratePdfAsync(string html);
    string ExtractText(string pdfPath);
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

    public string ExtractText(string pdfPath)
    {
        using var pdf = PdfDocument.FromFile(pdfPath);
        return pdf.ExtractAllText();
    }
}
```

### Scenario 4: Error Handling

**PDFBox Pattern:**
```csharp
try
{
    document = PDDocument.load(file);
    // operations
}
catch (java.io.IOException ex)
{
    // Java-style exception
}
finally
{
    document?.close();
}
```

**IronPDF Pattern:**
```csharp
try
{
    using var pdf = PdfDocument.FromFile(path);
    // operations
}
catch (IronPdf.Exceptions.IronPdfProductException ex)
{
    Console.WriteLine($"PDF Error: {ex.Message}");
}
```

---

## Performance Considerations

### Memory Usage

| Scenario | PDFBox .NET Port | IronPDF | Notes |
|----------|------------------|---------|-------|
| Text extraction | ~80 MB | ~50 MB | IronPDF more efficient |
| PDF creation | ~100 MB | ~60 MB | HTML rendering optimized |
| Batch (100 PDFs) | High (manual cleanup) | ~100 MB | Use `using` statements |

### Optimization Tips

**1. Use `using` Statements:**
```csharp
// Automatic cleanup
using var pdf = PdfDocument.FromFile(path);
```

**2. Reuse Renderer for Batch:**
```csharp
var renderer = new ChromePdfRenderer();
foreach (var html in htmlList)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{i}.pdf");
}
```

**3. Use Async in Web Apps:**
```csharp
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

---

## Troubleshooting Guide

### Issue 1: Java-Style Method Names Not Found

**Symptom:** `getText()`, `getNumberOfPages()` not recognized
**Cause:** IronPDF uses .NET conventions
**Solution:**
```csharp
// PDFBox: stripper.getText(document)
// IronPDF: pdf.ExtractAllText()

// PDFBox: document.getNumberOfPages()
// IronPDF: pdf.PageCount
```

### Issue 2: File Object Not Recognized

**Symptom:** Java `File` class not available
**Cause:** IronPDF uses standard .NET strings
**Solution:**
```csharp
// PDFBox: PDDocument.load(new File(path))
// IronPDF: PdfDocument.FromFile(path)
```

### Issue 3: No close() Method

**Symptom:** Looking for `close()` method
**Cause:** IronPDF uses IDisposable
**Solution:**
```csharp
// PDFBox
document.close();

// IronPDF
using var pdf = PdfDocument.FromFile(path);
// Automatic disposal
```

### Issue 4: Coordinate Positioning Not Working

**Symptom:** Manual `moveTextPositionByAmount()` not available
**Cause:** IronPDF uses HTML/CSS
**Solution:**
```csharp
// Use CSS positioning
string html = "<p style='position:absolute; left:72px; top:700px;'>Text</p>";
```

### Issue 5: No PDFTextStripper Equivalent

**Symptom:** Looking for stripper configuration
**Cause:** Simpler API
**Solution:**
```csharp
// IronPDF: Just call ExtractAllText()
string text = pdf.ExtractAllText();

// Per-page:
string pageText = pdf.Pages[0].Text;
```

### Issue 6: License Key Not Working

**Symptom:** Watermark or license error
**Cause:** License not set
**Solution:**
```csharp
IronPdf.License.LicenseKey = "YOUR-KEY";
```

### Issue 7: PDFMergerUtility Not Found

**Symptom:** No merger utility class
**Cause:** Simpler static method
**Solution:**
```csharp
// IronPDF uses static Merge
var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
```

### Issue 8: No ContentStream for Drawing

**Symptom:** Can't find content stream API
**Cause:** IronPDF uses HTML
**Solution:**
```csharp
// Use HTML for all content
var pdf = renderer.RenderHtmlAsPdf("<h1>Title</h1><p>Content</p>");
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit PDFBox usage**
  ```bash
  grep -r "pdfbox\|PDDocument" --include="*.cs" .
  ```
  **Why:** Identify all PDFBox usages to ensure complete migration coverage to IronPDF.

- [ ] **List all PDF operations**
  ```csharp
  // Before (.)
  PDDocument document = new PDDocument();
  // Various PDF operations

  // After (IronPDF)
  var pdf = new ChromePdfRenderer();
  // Equivalent PDF operations using IronPDF
  ```
  **Why:** Understanding current operations helps map them to IronPDF's capabilities.

- [ ] **Identify manual coordinate positioning code**
  ```csharp
  // Before (.)
  contentStream.beginText();
  contentStream.newLineAtOffset(100, 700);
  contentStream.showText("Hello World");

  // After (IronPDF)
  var html = "<div style='position:absolute; left:100px; top:700px;'>Hello World</div>";
  var pdf = new ChromePdfRenderer().RenderHtmlAsPdf(html);
  ```
  **Why:** IronPDF allows using HTML/CSS for positioning, simplifying layout management.

- [ ] **Note any Java-style patterns**
  ```csharp
  // Before (.)
  File file = new File("path/to/file.pdf");

  // After (IronPDF)
  string filePath = "path/to/file.pdf";
  ```
  **Why:** Transitioning to idiomatic C# improves code readability and maintainability.

- [ ] **Obtain IronPDF trial license**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Backup codebase**
  **Why:** Ensure you have a restore point in case of migration issues.

### During Migration

- [ ] **Remove PDFBox NuGet packages**
  ```bash
  dotnet remove package PDFBox
  ```
  **Why:** Clean removal of old dependencies to avoid conflicts.

- [ ] **Install IronPdf package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project to begin using its features.

- [ ] **Set license key at startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations with IronPDF.

- [ ] **Update all namespace imports**
  ```csharp
  // Before (.)
  using org.apache.pdfbox.pdmodel;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure all references point to IronPDF classes and methods.

- [ ] **Replace `close()` with `using` statements**
  ```csharp
  // Before (.)
  PDDocument document = new PDDocument();
  // Operations
  document.close();

  // After (IronPDF)
  using (var pdf = PdfDocument.FromFile("path/to/file.pdf"))
  {
      // Operations
  }
  ```
  **Why:** Use `IDisposable` pattern for automatic resource management in C#.

- [ ] **Convert manual PDF creation to HTML**
  ```csharp
  // Before (.)
  PDPage page = new PDPage();
  PDDocument document = new PDDocument();
  document.addPage(page);

  // After (IronPDF)
  var html = "<html><body><h1>Hello World</h1></body></html>";
  var pdf = new ChromePdfRenderer().RenderHtmlAsPdf(html);
  ```
  **Why:** HTML-based PDF creation is more intuitive and less error-prone.

- [ ] **Update method names to .NET conventions**
  ```csharp
  // Before (.)
  document.save("output.pdf");

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Align with .NET naming conventions for consistency and clarity.

- [ ] **Replace `File` objects with string paths**
  ```csharp
  // Before (.)
  File file = new File("path/to/file.pdf");

  // After (IronPDF)
  string filePath = "path/to/file.pdf";
  ```
  **Why:** Use native .NET types for file handling.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF outputs**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Test text extraction accuracy**
  **Why:** Ensure text extraction is as expected with IronPDF.

- [ ] **Verify licensing**
  **Why:** Confirm that the IronPDF license is correctly applied and functioning.

- [ ] **Performance benchmark**
  **Why:** Assess performance to ensure IronPDF meets application requirements.

- [ ] **Update CI/CD pipeline**
  **Why:** Ensure build and deployment processes accommodate IronPDF.

- [ ] **Document new patterns for team**
  **Why:** Share knowledge of new patterns and practices with the development team for consistency.
---

## Additional Resources

- **IronPDF Documentation**: [https://ironpdf.com/docs/](https://ironpdf.com/docs/)
- **IronPDF Tutorials**: [https://ironpdf.com/tutorials/](https://ironpdf.com/tutorials/)
- **HTML to PDF Guide**: [https://ironpdf.com/how-to/html-file-to-pdf/](https://ironpdf.com/how-to/html-file-to-pdf/)
- **API Reference**: [https://ironpdf.com/object-reference/api/](https://ironpdf.com/object-reference/api/)
- **NuGet Package**: [https://www.nuget.org/packages/IronPdf/](https://www.nuget.org/packages/IronPdf/)

---

*This migration guide was created to help developers transition from Apache PDFBox .NET ports to IronPDF efficiently.*
