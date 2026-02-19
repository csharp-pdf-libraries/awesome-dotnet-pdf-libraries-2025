# How Do I Migrate from Apryse (formerly PDFTron) to IronPDF in C#?

## Table of Contents
1. [Why Migrate from Apryse to IronPDF](#why-migrate-from-apryse-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start (5 Minutes)](#quick-start-5-minutes)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Migration Checklist](#migration-checklist)
10. [Additional Resources](#additional-resources)

---

## Why Migrate from Apryse to IronPDF

### Cost Comparison

Apryse (formerly PDFTron) is one of the most expensive PDF SDKs on the market, targeting enterprise customers with premium pricing:

| Aspect | Apryse (PDFTron) | IronPDF |
|--------|-----------------|---------|
| **Starting Price** | $1,500+/developer/year (reported) | $749 one-time (Lite) |
| **License Model** | Annual subscription | Perpetual license |
| **Viewer License** | Separate, additional cost | N/A (use standard viewers) |
| **Server License** | Enterprise pricing required | Included in license tiers |
| **Total 3-Year Cost** | $4,500+ per developer | $749 one-time |

### Complexity vs. Simplicity

These implementation complexities are addressed in detail within the [migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-apryse-to-ironpdf/).

| Feature | Apryse | IronPDF |
|---------|--------|---------|
| **Setup** | Module paths, external binaries | Single NuGet package |
| **Initialization** | `PDFNet.Initialize()` with license | Simple property assignment |
| **HTML Rendering** | External html2pdf module required | Built-in Chromium engine |
| **API Style** | C++ heritage, complex | Modern C# conventions |
| **Dependencies** | Multiple DLLs, platform-specific | Self-contained package |

### When to Migrate

**Migrate to IronPDF if:**
- You primarily need HTML/URL to PDF conversion
- You want simpler API with less boilerplate
- Premium pricing isn't justified for your use case
- You don't need PDFViewCtrl viewer controls
- You prefer one-time licensing over subscriptions

**Stay with Apryse if:**
- You need their native viewer controls (PDFViewCtrl)
- You use XOD or proprietary formats extensively
- You require specific enterprise features (advanced redaction, etc.)
- Your organization already has enterprise licenses

---

## Before You Start

### Prerequisites

- **.NET Framework 4.6.2+** or **.NET Core 3.1+** or **.NET 5/6/7/8/9**
- **Visual Studio 2019+** or **VS Code** with C# extension
- **NuGet Package Manager** access
- **Existing Apryse (PDFTron) codebase** to migrate

### Find All Apryse References

Before migrating, identify all PDFTron usage in your codebase:

```bash
# Find all pdftron using statements
grep -r "using pdftron" --include="*.cs" .

# Find PDFNet initialization
grep -r "PDFNet.Initialize\|PDFNet.SetResourcesPath" --include="*.cs" .

# Find PDFDoc usage
grep -r "new PDFDoc\|PDFDoc\." --include="*.cs" .

# Find HTML2PDF usage
grep -r "HTML2PDF\|InsertFromURL\|InsertFromHtmlString" --include="*.cs" .

# Find ElementReader/Writer usage
grep -r "ElementReader\|ElementWriter\|ElementBuilder" --include="*.cs" .
```

### Breaking Changes to Expect

| Apryse Pattern | Change Required |
|----------------|-----------------|
| `PDFNet.Initialize()` | Replace with `IronPdf.License.LicenseKey` |
| `HTML2PDF` module | Built-in `ChromePdfRenderer` |
| `ElementReader`/`ElementWriter` | IronPDF handles content internally |
| `SDFDoc.SaveOptions` | Simple `SaveAs()` method |
| `PDFViewCtrl` | Use external PDF viewers |
| XOD format | Convert to PDF or images |
| Module path configuration | Not needed |

---

## Quick Start (5 Minutes)

### Step 1: Replace NuGet Packages

```bash
# Remove Apryse/PDFTron packages
dotnet remove package PDFTron.NET.x64
dotnet remove package PDFTron.NET.x86
dotnet remove package pdftron

# Install IronPDF
dotnet add package IronPdf
```

Or via Package Manager Console:
```powershell
Uninstall-Package PDFTron.NET.x64
Install-Package IronPdf
```

### Step 2: Update Namespaces

```csharp
// ❌ Remove these
using pdftron;
using pdftron.PDF;
using pdftron.PDF.Convert;
using pdftron.SDF;
using pdftron.Filters;

// ✅ Add these
using IronPdf;
using IronPdf.Rendering;
```

### Step 3: Remove Initialization Code

**Before (Apryse):**
```csharp
// Complex initialization
PDFNet.Initialize("YOUR_LICENSE_KEY");
PDFNet.SetResourcesPath("path/to/resources");
// Plus module path for HTML2PDF...
```

**After (IronPDF):**
```csharp
// Simple license assignment (optional for development)
IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";
```

### Step 4: Convert Your First PDF

**Before (Apryse):**
```csharp
using (PDFDoc doc = new PDFDoc())
{
    HTML2PDF converter = new HTML2PDF();
    converter.SetModulePath("path/to/html2pdf");
    converter.InsertFromHtmlString("<h1>Hello</h1>");
    if (converter.Convert(doc))
    {
        doc.Save("output.pdf", SDFDoc.SaveOptions.e_linearized);
    }
}
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("output.pdf");
```

---

## Complete API Reference

### Namespace Mapping

| Apryse Namespace | IronPDF Namespace | Purpose |
|------------------|-------------------|---------|
| `pdftron` | `IronPdf` | Core functionality |
| `pdftron.PDF` | `IronPdf` | PDF document classes |
| `pdftron.PDF.Convert` | `IronPdf` | Conversion utilities |
| `pdftron.SDF` | `IronPdf` | Low-level document access |
| `pdftron.Filters` | N/A | IronPDF handles internally |
| `pdftron.Common` | N/A | Exception types |

### Core Class Mapping

| Apryse Class | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `PDFDoc` | `PdfDocument` | Main document class |
| `HTML2PDF` | `ChromePdfRenderer` | HTML to PDF conversion |
| `Page` | Accessed via `PdfDocument` | Page manipulation |
| `ElementReader` | N/A | Use `ExtractAllText()` for text |
| `ElementWriter` | N/A | Use HTML rendering instead |
| `ElementBuilder` | N/A | Use HTML rendering instead |
| `TextExtractor` | `PdfDocument.ExtractAllText()` | Text extraction |
| `Stamper` | `PdfDocument.ApplyWatermark()` | Watermarks and stamps |
| `PDFDraw` | `PdfDocument.ToBitmap()` | Rasterization |
| `Flattener` | `PdfDocument.Flatten()` | Flatten forms |
| `SecurityHandler` | `PdfDocument.SecuritySettings` | Encryption/passwords |
| `PDFNet` | `IronPdf.License` | Licensing and config |

### Complete Method Mapping

#### Initialization and Licensing

| Apryse Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `PDFNet.Initialize(key)` | `License.LicenseKey = key` | One-time setup |
| `PDFNet.SetResourcesPath(path)` | Not needed | Self-contained |
| `PDFNet.Terminate()` | Not needed | Auto cleanup |
| `PDFNet.SetDefaultDiskCachingEnabled()` | Not needed | Automatic |

#### Document Creation and Loading

| Apryse Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `new PDFDoc()` | `new PdfDocument()` | Empty document |
| `new PDFDoc(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `new PDFDoc(buffer)` | `PdfDocument.FromBinaryData(bytes)` | Load from bytes |
| `doc.Save(path, options)` | `pdf.SaveAs(path)` | Save to file |
| `doc.Save(buffer)` | `pdf.BinaryData` | Get as bytes |
| `doc.Close()` | `pdf.Dispose()` | Cleanup (or use `using`) |

#### HTML to PDF Conversion

| Apryse Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `HTML2PDF.Convert(doc)` | `renderer.RenderHtmlAsPdf(html)` | Returns PdfDocument |
| `converter.InsertFromURL(url)` | `renderer.RenderUrlAsPdf(url)` | URL conversion |
| `converter.InsertFromHtmlString(html)` | `renderer.RenderHtmlAsPdf(html)` | HTML string |
| `converter.SetModulePath(path)` | Not needed | Built-in engine |
| `converter.SetPaperSize(width, height)` | `RenderingOptions.PaperSize` | Paper size |
| `converter.SetLandscape(true)` | `RenderingOptions.PaperOrientation` | Orientation |
| `converter.SetMargins(t, b, l, r)` | `RenderingOptions.MarginTop`, etc. | Margins |

#### Page Operations

| Apryse Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `doc.GetPageCount()` | `pdf.PageCount` | Page count |
| `doc.GetPage(index)` | `pdf.Pages[index]` | Access page |
| `doc.PageInsert(pos, page)` | `pdf.InsertPage(index, page)` | Insert page |
| `doc.PageRemove(pos)` | `pdf.RemovePage(index)` | Remove page |
| `doc.AppendPages(doc2, start, end)` | `PdfDocument.Merge(pdfs)` | Merge documents |
| `doc.InsertPages(pos, doc2, start, end)` | `pdf.InsertPdf(doc2, index)` | Insert pages |

#### Text Extraction

| Apryse Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `new TextExtractor()` | Direct method call | No separate class |
| `extractor.Begin(page)` | `pdf.ExtractTextFromPage(index)` | Per-page extraction |
| `extractor.GetAsText()` | `pdf.ExtractAllText()` | Full document text |
| `extractor.GetAsXML()` | N/A | Use text output |
| `extractor.GetWordCount()` | Count words from text | Manual count |

#### Watermarks and Stamps

| Apryse Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `new Stamper(size, pos)` | `pdf.ApplyWatermark(html)` | HTML-based watermark |
| `stamper.StampText(doc, text)` | `pdf.ApplyWatermark("<h1>text</h1>")` | Text stamp |
| `stamper.StampImage(doc, image)` | `pdf.ApplyWatermark("<img src='...'/>")` | Image stamp |
| `stamper.SetOpacity(value)` | Include in HTML CSS | Opacity via CSS |
| `stamper.SetRotation(angle)` | `rotation` parameter | Rotation |
| `stamper.SetPosition(x, y)` | CSS positioning | Position via CSS |

#### Security and Encryption

| Apryse Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `new SecurityHandler()` | `pdf.SecuritySettings` | Security object |
| `handler.ChangeUserPassword(pwd)` | `SecuritySettings.UserPassword` | User password |
| `handler.ChangeMasterPassword(pwd)` | `SecuritySettings.OwnerPassword` | Owner password |
| `handler.SetPermission(perm, value)` | `SecuritySettings.AllowUserPrinting` | Permissions |
| `doc.SetSecurityHandler(handler)` | Set properties directly | No separate handler |
| `handler.GetEncryptionAlgorithmID()` | Automatic | Auto encryption |

#### Image and Rasterization

| Apryse Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `new PDFDraw(dpi)` | `pdf.ToBitmap(dpi)` | Rasterize |
| `draw.Export(page, path, format)` | `pdf.RasterizeToImageFiles(path)` | Export images |
| `draw.GetBitmap(page)` | `pdf.ToBitmap()` | Get bitmap array |
| `draw.SetDPI(dpi)` | DPI parameter | Resolution |
| `draw.SetAntiAliasing(bool)` | Automatic | Built-in |

#### Annotations and Forms

| Apryse Method | IronPDF Method | Notes |
|---------------|----------------|-------|
| `Annot.Create(page, type)` | Use HTML rendering | Create via HTML |
| `page.GetAnnot(index)` | `pdf.Form.Fields` | Access form fields |
| `doc.FlattenAnnotations()` | `pdf.Flatten()` | Flatten annotations |
| `FieldIterator` | `foreach (var field in pdf.Form.Fields)` | Iterate fields |
| `field.SetValue(value)` | `field.Value = value` | Set field value |

---

## Code Migration Examples

### Example 1: HTML String to PDF

**Before (Apryse):**
```csharp
using pdftron;
using pdftron.PDF;

class Program
{
    static void Main()
    {
        PDFNet.Initialize("YOUR_LICENSE_KEY");
        PDFNet.SetResourcesPath("path/to/resources");

        string html = "<html><body><h1>Hello World</h1><p>Content here</p></body></html>";

        using (PDFDoc doc = new PDFDoc())
        {
            HTML2PDF converter = new HTML2PDF();
            converter.SetModulePath("path/to/html2pdf");
            converter.InsertFromHtmlString(html);

            HTML2PDF.WebPageSettings settings = new HTML2PDF.WebPageSettings();
            settings.SetPrintBackground(true);
            settings.SetLoadImages(true);

            if (converter.Convert(doc))
            {
                doc.Save("output.pdf", SDFDoc.SaveOptions.e_linearized);
                Console.WriteLine("PDF created successfully");
            }
            else
            {
                Console.WriteLine($"Conversion failed: {converter.GetLog()}");
            }
        }

        PDFNet.Terminate();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        // Optional: Set license for production
        License.LicenseKey = "YOUR_LICENSE_KEY";

        string html = "<html><body><h1>Hello World</h1><p>Content here</p></body></html>";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PrintHtmlBackgrounds = true;

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");

        Console.WriteLine("PDF created successfully");
    }
}
```

### Example 2: URL to PDF

**Before (Apryse):**
```csharp
using pdftron;
using pdftron.PDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

using (PDFDoc doc = new PDFDoc())
{
    HTML2PDF converter = new HTML2PDF();
    converter.SetModulePath("path/to/html2pdf");

    HTML2PDF.WebPageSettings settings = new HTML2PDF.WebPageSettings();
    settings.SetLoadImages(true);
    settings.SetAllowJavaScript(true);
    settings.SetPrintBackground(true);

    converter.InsertFromURL("https://example.com", settings);

    if (converter.Convert(doc))
    {
        doc.Save("webpage.pdf", SDFDoc.SaveOptions.e_linearized);
    }
}

PDFNet.Terminate();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.EnableJavaScript = true;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Merge Multiple PDFs

**Before (Apryse):**
```csharp
using pdftron;
using pdftron.PDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

using (PDFDoc mainDoc = new PDFDoc())
{
    string[] files = { "doc1.pdf", "doc2.pdf", "doc3.pdf" };

    foreach (string file in files)
    {
        using (PDFDoc doc = new PDFDoc(file))
        {
            mainDoc.AppendPages(doc, 1, doc.GetPageCount());
        }
    }

    mainDoc.Save("merged.pdf", SDFDoc.SaveOptions.e_linearized);
}

PDFNet.Terminate();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("doc1.pdf");
var pdf2 = PdfDocument.FromFile("doc2.pdf");
var pdf3 = PdfDocument.FromFile("doc3.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("merged.pdf");

// Or from a list
var pdfs = new[] { "doc1.pdf", "doc2.pdf", "doc3.pdf" }
    .Select(PdfDocument.FromFile)
    .ToList();
var mergedFromList = PdfDocument.Merge(pdfs);
```

### Example 4: Text Extraction

**Before (Apryse):**
```csharp
using pdftron;
using pdftron.PDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

using (PDFDoc doc = new PDFDoc("document.pdf"))
{
    TextExtractor extractor = new TextExtractor();

    for (int i = 1; i <= doc.GetPageCount(); i++)
    {
        Page page = doc.GetPage(i);
        extractor.Begin(page);

        string pageText = extractor.GetAsText();
        Console.WriteLine($"Page {i}:");
        Console.WriteLine(pageText);

        // Word-by-word iteration
        TextExtractor.Word word;
        for (TextExtractor.Line line = extractor.GetFirstLine(); line.IsValid(); line = line.GetNextLine())
        {
            for (word = line.GetFirstWord(); word.IsValid(); word = word.GetNextWord())
            {
                Console.Write(word.GetString() + " ");
            }
        }
    }
}

PDFNet.Terminate();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Extract all text at once
string allText = pdf.ExtractAllText();
Console.WriteLine(allText);

// Extract from specific page
string page1Text = pdf.ExtractTextFromPage(0); // 0-indexed
Console.WriteLine($"Page 1: {page1Text}");

// Extract from multiple pages
for (int i = 0; i < pdf.PageCount; i++)
{
    string pageText = pdf.ExtractTextFromPage(i);
    Console.WriteLine($"Page {i + 1}:");
    Console.WriteLine(pageText);
}
```

### Example 5: Add Watermark

**Before (Apryse):**
```csharp
using pdftron;
using pdftron.PDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

using (PDFDoc doc = new PDFDoc("document.pdf"))
{
    Stamper stamper = new Stamper(Stamper.SizeType.e_relative_scale, 0.5, 0.5);
    stamper.SetAlignment(Stamper.HorizontalAlignment.e_horizontal_center,
                         Stamper.VerticalAlignment.e_vertical_center);
    stamper.SetOpacity(0.3);
    stamper.SetRotation(45);
    stamper.SetFontColor(new ColorPt(1, 0, 0));
    stamper.SetTextAlignment(Stamper.TextAlignment.e_align_center);

    stamper.StampText(doc, "CONFIDENTIAL",
        new PageSet(1, doc.GetPageCount()));

    doc.Save("watermarked.pdf", SDFDoc.SaveOptions.e_linearized);
}

PDFNet.Terminate();
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

var pdf = PdfDocument.FromFile("document.pdf");

// HTML-based watermark with full styling control
string watermarkHtml = @"
<div style='
    color: red;
    opacity: 0.3;
    font-size: 72px;
    font-weight: bold;
    text-align: center;
'>CONFIDENTIAL</div>";

pdf.ApplyWatermark(watermarkHtml,
    rotation: 45,
    verticalAlignment: VerticalAlignment.Middle,
    horizontalAlignment: HorizontalAlignment.Center);

pdf.SaveAs("watermarked.pdf");
```

### Example 6: Password Protection

**Before (Apryse):**
```csharp
using pdftron;
using pdftron.PDF;
using pdftron.SDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

using (PDFDoc doc = new PDFDoc("document.pdf"))
{
    SecurityHandler handler = new SecurityHandler();
    handler.ChangeUserPassword("user123");
    handler.ChangeMasterPassword("owner456");

    handler.SetPermission(SecurityHandler.Permission.e_print, false);
    handler.SetPermission(SecurityHandler.Permission.e_extract_content, false);
    handler.SetPermission(SecurityHandler.Permission.e_doc_modify, false);

    doc.SetSecurityHandler(handler);
    doc.Save("protected.pdf", SDFDoc.SaveOptions.e_linearized);
}

PDFNet.Terminate();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Set passwords
pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.OwnerPassword = "owner456";

// Set permissions
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserAnnotations = false;

pdf.SaveAs("protected.pdf");
```

### Example 7: Convert PDF to Images

**Before (Apryse):**
```csharp
using pdftron;
using pdftron.PDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

using (PDFDoc doc = new PDFDoc("document.pdf"))
{
    PDFDraw draw = new PDFDraw();
    draw.SetDPI(300);
    draw.SetAntiAliasing(true);
    draw.SetImageSmoothing(true);

    for (int i = 1; i <= doc.GetPageCount(); i++)
    {
        Page page = doc.GetPage(i);
        draw.Export(page, $"page_{i}.png", "PNG");
    }
}

PDFNet.Terminate();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Export all pages as images
pdf.RasterizeToImageFiles("page_*.png", DPI: 300);

// Or get as Bitmap objects
System.Drawing.Bitmap[] images = pdf.ToBitmap(DPI: 300);
for (int i = 0; i < images.Length; i++)
{
    images[i].Save($"page_{i + 1}.png");
}
```

### Example 8: Form Field Manipulation

**Before (Apryse):**
```csharp
using pdftron;
using pdftron.PDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

using (PDFDoc doc = new PDFDoc("form.pdf"))
{
    for (FieldIterator itr = doc.GetFieldIterator(); itr.HasNext(); itr.Next())
    {
        Field field = itr.Current();
        Console.WriteLine($"Field: {field.GetName()}, Value: {field.GetValueAsString()}");

        if (field.GetName() == "firstName")
        {
            field.SetValue("John");
        }
    }

    doc.Save("filled_form.pdf", SDFDoc.SaveOptions.e_linearized);
}

PDFNet.Terminate();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("form.pdf");

// List all form fields
foreach (var field in pdf.Form.Fields)
{
    Console.WriteLine($"Field: {field.Name}, Value: {field.Value}");
}

// Set field values
pdf.Form.GetFieldByName("firstName").Value = "John";

// Or access by index
pdf.Form.Fields[0].Value = "Updated Value";

pdf.SaveAs("filled_form.pdf");
```

### Example 9: Split PDF

**Before (Apryse):**
```csharp
using pdftron;
using pdftron.PDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

using (PDFDoc doc = new PDFDoc("large_document.pdf"))
{
    int pageCount = doc.GetPageCount();

    for (int i = 1; i <= pageCount; i++)
    {
        using (PDFDoc newDoc = new PDFDoc())
        {
            newDoc.InsertPages(0, doc, i, i, PDFDoc.InsertFlag.e_none);
            newDoc.Save($"page_{i}.pdf", SDFDoc.SaveOptions.e_linearized);
        }
    }
}

PDFNet.Terminate();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("large_document.pdf");

// Split into individual pages
for (int i = 0; i < pdf.PageCount; i++)
{
    var singlePage = pdf.CopyPage(i);
    singlePage.SaveAs($"page_{i + 1}.pdf");
}

// Or extract a range of pages
var firstFive = pdf.CopyPages(0, 4);
firstFive.SaveAs("first_5_pages.pdf");
```

### Example 10: Headers and Footers

**Before (Apryse):**
```csharp
using pdftron;
using pdftron.PDF;

PDFNet.Initialize("YOUR_LICENSE_KEY");

using (PDFDoc doc = new PDFDoc())
{
    HTML2PDF converter = new HTML2PDF();
    converter.SetModulePath("path/to/html2pdf");

    // Header/Footer through HTML2PDF settings
    HTML2PDF.WebPageSettings settings = new HTML2PDF.WebPageSettings();
    settings.SetHeader("<div style='text-align:center'>Header</div>");
    settings.SetFooter("<div style='text-align:center'>Page [page] of [topage]</div>");

    converter.InsertFromHtmlString("<h1>Content</h1>", settings);
    converter.Convert(doc);

    doc.Save("with_headers.pdf", SDFDoc.SaveOptions.e_linearized);
}

PDFNet.Terminate();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center; font-size:12px;'>Company Header</div>",
    DrawDividerLine = true,
    MaxHeight = 30
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center; font-size:10px;'>Page {page} of {total-pages}</div>",
    DrawDividerLine = true,
    MaxHeight = 25
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Content</h1>");
pdf.SaveAs("with_headers.pdf");
```

---

## Advanced Scenarios

### ASP.NET Core Integration

**Before (Apryse in ASP.NET):**
```csharp
public class PdfController : Controller
{
    public PdfController()
    {
        PDFNet.Initialize("YOUR_LICENSE_KEY");
    }

    public IActionResult GeneratePdf()
    {
        using (PDFDoc doc = new PDFDoc())
        {
            HTML2PDF converter = new HTML2PDF();
            converter.SetModulePath(_modulePath);
            converter.InsertFromHtmlString("<h1>Report</h1>");

            if (converter.Convert(doc))
            {
                byte[] pdfData;
                doc.Save(out pdfData, SDFDoc.SaveOptions.e_linearized);
                return File(pdfData, "application/pdf", "report.pdf");
            }
        }
        return BadRequest("Conversion failed");
    }
}
```

**After (IronPDF in ASP.NET Core):**
```csharp
[ApiController]
[Route("[controller]")]
public class PdfController : ControllerBase
{
    [HttpGet("generate")]
    public IActionResult GeneratePdf()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1>");

        return File(pdf.BinaryData, "application/pdf", "report.pdf");
    }

    [HttpGet("generate-async")]
    public async Task<IActionResult> GeneratePdfAsync()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = await renderer.RenderHtmlAsPdfAsync("<h1>Report</h1>");

        return File(pdf.Stream, "application/pdf", "report.pdf");
    }
}
```

### Dependency Injection Setup

```csharp
// Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Set license once
    IronPdf.License.LicenseKey = Configuration["IronPdf:LicenseKey"];

    // Register renderer as scoped service
    services.AddScoped<ChromePdfRenderer>();

    // Or create a wrapper service
    services.AddScoped<IPdfService, IronPdfService>();
}

// IronPdfService.cs
public interface IPdfService
{
    PdfDocument GenerateFromHtml(string html);
    PdfDocument GenerateFromUrl(string url);
    Task<PdfDocument> GenerateFromHtmlAsync(string html);
}

public class IronPdfService : IPdfService
{
    private readonly ChromePdfRenderer _renderer;

    public IronPdfService()
    {
        _renderer = new ChromePdfRenderer();
        ConfigureDefaults();
    }

    private void ConfigureDefaults()
    {
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.PrintHtmlBackgrounds = true;
        _renderer.RenderingOptions.MarginTop = 25;
        _renderer.RenderingOptions.MarginBottom = 25;
    }

    public PdfDocument GenerateFromHtml(string html) =>
        _renderer.RenderHtmlAsPdf(html);

    public PdfDocument GenerateFromUrl(string url) =>
        _renderer.RenderUrlAsPdf(url);

    public Task<PdfDocument> GenerateFromHtmlAsync(string html) =>
        _renderer.RenderHtmlAsPdfAsync(html);
}
```

### Error Handling Migration

**Before (Apryse):**
```csharp
try
{
    PDFNet.Initialize("KEY");
    using (PDFDoc doc = new PDFDoc())
    {
        HTML2PDF converter = new HTML2PDF();
        converter.InsertFromHtmlString(html);

        if (!converter.Convert(doc))
        {
            string log = converter.GetLog();
            throw new Exception($"Conversion failed: {log}");
        }

        doc.Save("output.pdf", SDFDoc.SaveOptions.e_linearized);
    }
}
catch (PDFNetException e)
{
    Console.WriteLine($"PDFNet Error: {e.Message}");
}
finally
{
    PDFNet.Terminate();
}
```

**After (IronPDF):**
```csharp
try
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
catch (IronPdf.Exceptions.IronPdfLicenseException ex)
{
    Console.WriteLine($"License error: {ex.Message}");
}
catch (IronPdf.Exceptions.IronPdfRenderingException ex)
{
    Console.WriteLine($"Rendering error: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"File error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
// No Terminate() needed
```

### Docker Deployment

**Before (Apryse):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# Apryse requires platform-specific native libraries
COPY ./pdftron_libs /app/pdftron_libs
ENV PDFNet_ResourcesPath=/app/pdftron_libs
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

**After (IronPDF):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install Chromium dependencies
RUN apt-get update && apt-get install -y \
    libc6 libgdiplus libx11-6 libxcomposite1 \
    libxdamage1 libxrandr2 libxss1 libxtst6 \
    libnss3 libatk-bridge2.0-0 libgtk-3-0 \
    libgbm1 libasound2 fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

Or use the official IronPDF Docker image:
```dockerfile
FROM ironsoftwareofficial/ironpdfengine:latest
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

---

## Performance Considerations

### Apryse vs IronPDF Performance

| Metric | Apryse | IronPDF |
|--------|--------|---------|
| **Cold start** | Fast (native code) | ~2s (Chromium init) |
| **Subsequent renders** | Fast | Fast |
| **Complex HTML** | Variable (html2pdf module) | Excellent (Chromium) |
| **CSS support** | Limited | Full CSS3 |
| **JavaScript** | Limited | Full support |
| **Memory** | Lower baseline | Higher (Chromium) |

### Optimizing IronPDF Performance

```csharp
// 1. Reuse renderer instance
private static readonly ChromePdfRenderer SharedRenderer = new ChromePdfRenderer();

public PdfDocument GeneratePdf(string html)
{
    return SharedRenderer.RenderHtmlAsPdf(html);
}

// 2. Parallel generation
public async Task<List<PdfDocument>> GenerateBatch(List<string> htmlDocs)
{
    var tasks = htmlDocs.Select(html =>
        Task.Run(() => SharedRenderer.RenderHtmlAsPdf(html)));

    var results = await Task.WhenAll(tasks);
    return results.ToList();
}

// 3. Disable unnecessary features
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = false; // If not needed
renderer.RenderingOptions.WaitFor.RenderDelay(0);   // No delay
renderer.RenderingOptions.Timeout = 30000;          // 30s max

// 4. Proper disposal
using (var pdf = renderer.RenderHtmlAsPdf(html))
{
    pdf.SaveAs("output.pdf");
}
```

---

## Troubleshooting Guide

### Issue 1: Module path errors

**Symptom:** Looking for html2pdf module path

**Solution:** IronPDF doesn't need module paths—remove all path configuration:
```csharp
// ❌ Remove this
converter.SetModulePath("path/to/html2pdf");

// ✅ Just use the renderer
var renderer = new ChromePdfRenderer();
```

### Issue 2: PDFNet.Initialize() not found

**Symptom:** Method not found error

**Solution:** Replace with IronPDF license setup:
```csharp
// ❌ Remove this
PDFNet.Initialize("KEY");
PDFNet.SetResourcesPath("path");

// ✅ Use this (optional for development)
IronPdf.License.LicenseKey = "YOUR-KEY";
```

### Issue 3: SDFDoc.SaveOptions not available

**Symptom:** Save options enum not found

**Solution:** IronPDF uses simpler save methods:
```csharp
// ❌ Remove this
doc.Save("file.pdf", SDFDoc.SaveOptions.e_linearized);

// ✅ Use this
pdf.SaveAs("file.pdf");
```

### Issue 4: ElementReader/Writer not available

**Symptom:** Low-level element APIs not found

**Solution:** Use HTML rendering instead:
```csharp
// ❌ Apryse low-level approach
ElementReader reader = new ElementReader();
ElementWriter writer = new ElementWriter();
// Complex element manipulation...

// ✅ IronPDF HTML approach
string html = "<div>Your content here</div>";
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 5: Different rendering output

**Symptom:** PDF looks different from Apryse output

**Solution:** Configure IronPDF to match:
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.ViewPortWidth = 1280;
renderer.RenderingOptions.WaitFor.RenderDelay(500);
```

### Issue 6: PDFViewCtrl replacement

**Symptom:** Need embedded PDF viewer

**Solution:** IronPDF doesn't include viewer controls. Options:
- Use PDF.js for web viewers
- Use system PDF viewers
- Consider third-party viewer components

### Issue 7: XOD format not supported

**Symptom:** Need XOD output

**Solution:** Use standard formats:
```csharp
// Convert to images instead
pdf.RasterizeToImageFiles("page_*.png", DPI: 150);

// Or keep as PDF
pdf.SaveAs("output.pdf");
```

### Issue 8: License validation failing

**Symptom:** License not recognized

**Solution:**
```csharp
// Set license at app startup
IronPdf.License.LicenseKey = "YOUR-KEY";

// Verify license
bool isLicensed = IronPdf.License.IsLicensed;
Console.WriteLine($"Licensed: {isLicensed}");
```

---

## Migration Checklist

### Pre-Migration Checklist

- [ ] **Inventory all Apryse/PDFTron usage in codebase**
  ```bash
  grep -r "using pdftron" --include="*.cs" .
  grep -r "PDFDoc\|ElementReader\|Stamper" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **List all features used (HTML2PDF, ElementReader, Stamper, etc.)**
  **Why:** Understanding the features in use helps in mapping them to IronPDF equivalents.

- [ ] **Identify any PDFViewCtrl viewer usage (requires alternative)**
  **Why:** IronPDF does not provide a viewer control; alternatives may be needed.

- [ ] **Check for XOD or proprietary format usage**
  **Why:** IronPDF focuses on standard PDF formats; proprietary formats need alternatives.

- [ ] **Review current license and renewal costs**
  **Why:** Compare with IronPDF's pricing to evaluate cost benefits.

- [ ] **Obtain IronPDF license key for production**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Set up test environment with IronPDF**
  **Why:** Ensure a smooth transition by testing in a controlled environment.

### During Migration Checklist

- [ ] **Remove all PDFTron NuGet packages**
  ```bash
  dotnet remove package pdftron
  ```
  **Why:** Clean removal of the old library to avoid conflicts.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project for PDF functionalities.

- [ ] **Replace `pdftron` namespaces with `IronPdf`**
  ```csharp
  // Before (pdftron)
  using pdftron.PDF;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Update namespaces to reflect the new library.

- [ ] **Remove `PDFNet.Initialize()` and `Terminate()` calls**
  ```csharp
  // Before (pdftron)
  PDFNet.Initialize();

  // After (IronPDF)
  // No initialization required
  ```
  **Why:** IronPDF does not require explicit initialization or termination.

- [ ] **Replace `HTML2PDF` with `ChromePdfRenderer`**
  ```csharp
  // Before (pdftron)
  var html2pdf = new HTML2PDF();
  html2pdf.InsertFromHtmlString(html);
  html2pdf.Save("output.pdf");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer for modern HTML rendering.

- [ ] **Update `PDFDoc` to `PdfDocument`**
  ```csharp
  // Before (pdftron)
  var doc = new PDFDoc("input.pdf");

  // After (IronPDF)
  var doc = PdfDocument.FromFile("input.pdf");
  ```
  **Why:** Use IronPDF's PdfDocument for handling PDF files.

- [ ] **Replace `ElementReader`/`Writer` with HTML rendering**
  ```csharp
  // Before (pdftron)
  var reader = new ElementReader();
  reader.Begin(doc);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** IronPDF simplifies PDF creation with HTML rendering.

- [ ] **Update `TextExtractor` to `ExtractAllText()`**
  ```csharp
  // Before (pdftron)
  var extractor = new TextExtractor();
  extractor.Begin(doc);

  // After (IronPDF)
  var text = doc.ExtractAllText();
  ```
  **Why:** IronPDF provides a straightforward method to extract text.

- [ ] **Replace `Stamper` with `ApplyWatermark()`**
  ```csharp
  // Before (pdftron)
  var stamper = new Stamper(Stamper.SizeType.e_relative_scale, 0.5, 0.5);
  stamper.SetTextAlignment(Stamper.TextAlignment.e_align_center);
  stamper.StampText(doc, "DRAFT");

  // After (IronPDF)
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");
  ```
  **Why:** IronPDF uses HTML/CSS for flexible watermarking.

- [ ] **Update security handler to `SecuritySettings`**
  ```csharp
  // Before (pdftron)
  doc.InitSecurityHandler();
  doc.SetUserPassword("password");

  // After (IronPDF)
  pdf.SecuritySettings.UserPassword = "password";
  ```
  **Why:** IronPDF simplifies security settings with a dedicated property.

- [ ] **Replace `PDFDraw` with `ToBitmap()` or `RasterizeToImageFiles()`**
  ```csharp
  // Before (pdftron)
  var pdfdraw = new PDFDraw();
  pdfdraw.Export(doc.GetPage(1), "output.png");

  // After (IronPDF)
  var image = pdf.RasterizeToImageFiles(1, "output.png");
  ```
  **Why:** IronPDF provides easy methods for rasterizing PDFs to images.

- [ ] **Test each converted feature**
  **Why:** Ensure all functionalities work as expected after migration.

### Post-Migration Checklist

- [ ] **Verify PDF output quality matches expectations**
  **Why:** Ensure the new library meets quality standards.

- [ ] **Test all edge cases (large documents, complex CSS)**
  **Why:** Validate performance and rendering accuracy.

- [ ] **Compare performance metrics**
  **Why:** Assess any performance improvements or regressions.

- [ ] **Update Docker configurations if applicable**
  **Why:** Ensure the environment is correctly set up for deployment.

- [ ] **Remove Apryse license and related configurations**
  **Why:** Clean up any old configurations no longer needed.

- [ ] **Document any IronPDF-specific configurations**
  **Why:** Provide clear documentation for future maintenance.

- [ ] **Train team on new API patterns**
  **Why:** Ensure the team is comfortable with the new library.

- [ ] **Update CI/CD pipelines if needed**
  **Why:** Maintain automated processes for building and deploying the application.
---

## Additional Resources

### Official Documentation
- **IronPDF Documentation**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Getting Started Guide**: https://ironpdf.com/docs/

### Tutorials
- **HTML to PDF Tutorial**: https://ironpdf.com/how-to/html-file-to-pdf/
- **URL to PDF Guide**: https://ironpdf.com/tutorials/url-to-pdf/
- **ASP.NET Integration**: https://ironpdf.com/tutorials/aspx-to-pdf/

### Code Examples
- **GitHub Examples**: https://github.com/nicholashew/IronPdf-Examples
- **Code Samples**: https://ironpdf.com/examples/

### Support
- **Technical Support**: https://ironpdf.com/support/
- **Community Forum**: https://forum.ironpdf.com/
- **Stack Overflow**: Search `[ironpdf]` tag

### Apryse References
- **Apryse API Documentation**: https://sdk.apryse.com/api/PDFTronSDK/index.html
- **PDFDoc Class Reference**: https://sdk.apryse.com/api/PDFTronSDK/dotnet/pdftron.PDF.PDFDoc.html
- **Getting Started Guide**: https://docs.apryse.com/core/guides/get-started/dotnetcore

---

*This migration guide covers the transition from Apryse (PDFTron) to IronPDF. For additional assistance, contact IronPDF support or consult the official documentation.*
