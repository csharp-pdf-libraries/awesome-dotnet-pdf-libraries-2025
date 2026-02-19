# How Do I Migrate from Aspose.PDF for .NET to IronPDF in C#?

## Table of Contents
1. [Why Migrate from Aspose.PDF to IronPDF](#why-migrate-from-asposepdf-to-ironpdf)
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

## Why Migrate from Aspose.PDF to IronPDF

### Cost Comparison

Aspose.PDF uses a traditional enterprise licensing model with annual renewals:

| Aspect | Aspose.PDF | IronPDF |
|--------|-----------|---------|
| **Starting Price** | $1,199/developer/year | $749 one-time (Lite) |
| **License Model** | Annual subscription + renewal | Perpetual license |
| **OEM License** | $5,997+ additional | Included in higher tiers |
| **Support** | Extra cost tiers | Included |
| **Total 3-Year Cost** | $3,597+ per developer | $749 one-time |

### HTML Rendering Engine Comparison

Additional implementation examples are available in the [full documentation](https://ironpdf.com/blog/migration-guides/migrate-from-aspose-pdf-to-ironpdf/).

| Feature | Aspose.PDF (Flying Saucer) | IronPDF (Chromium) |
|---------|---------------------------|-------------------|
| **CSS3 Support** | Limited (older CSS) | Full CSS3 |
| **Flexbox/Grid** | Not supported | Full support |
| **JavaScript** | Very limited | Full support |
| **Web Fonts** | Partial | Complete |
| **Modern HTML5** | Limited | Complete |
| **Rendering Quality** | Variable | Pixel-perfect |

### Performance Comparison

Users have reported significant performance differences:

| Metric | Aspose.PDF | IronPDF |
|--------|-----------|---------|
| **HTML Rendering** | Documented slowdowns (30x slower in some cases) | Optimized Chromium engine |
| **Large Documents** | Memory issues reported | Efficient streaming |
| **Linux Performance** | High CPU, memory leaks reported | Stable |
| **Batch Processing** | Variable | Consistent |

### When to Migrate

**Migrate to IronPDF if:**
- You need modern HTML/CSS rendering
- Performance is critical for your application
- You want to reduce licensing costs
- You're experiencing issues with Aspose.PDF's HTML engine
- You need reliable cross-platform performance

**Stay with Aspose.PDF if:**
- You use specific Aspose.PDF features not available in IronPDF
- Your documents are built programmatically (not HTML-based)
- You have deep integration with other Aspose products
- Migration cost exceeds licensing savings

---

## Before You Start

### Prerequisites

- **.NET Framework 4.6.2+** or **.NET Core 3.1+** or **.NET 5/6/7/8/9**
- **Visual Studio 2019+** or **VS Code** with C# extension
- **NuGet Package Manager** access
- **Existing Aspose.PDF codebase** to migrate

### Find All Aspose.PDF References

Before migrating, identify all Aspose.PDF usage in your codebase:

```bash
# Find all Aspose.Pdf using statements
grep -r "using Aspose.Pdf" --include="*.cs" .

# Find Document usage
grep -r "new Document\|Document\." --include="*.cs" .

# Find HtmlLoadOptions usage
grep -r "HtmlLoadOptions\|HtmlFragment" --include="*.cs" .

# Find Facades usage
grep -r "PdfFileEditor\|PdfFileMend\|PdfFileStamp" --include="*.cs" .

# Find TextAbsorber usage
grep -r "TextAbsorber\|TextFragmentAbsorber" --include="*.cs" .
```

### Breaking Changes to Expect

| Aspose.PDF Pattern | Change Required |
|--------------------|-----------------|
| `new Document()` + `Pages.Add()` | Use HTML rendering instead |
| `HtmlLoadOptions` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `TextFragment` + manual positioning | CSS-based positioning |
| `PdfFileEditor.Concatenate()` | `PdfDocument.Merge()` |
| `TextFragmentAbsorber` | `pdf.ExtractAllText()` |
| `ImageStamp` | HTML-based watermarks |
| `.lic` file licensing | Code-based license key |

---

## Quick Start (5 Minutes)

### Step 1: Replace NuGet Packages

```bash
# Remove Aspose.PDF
dotnet remove package Aspose.PDF

# Install IronPDF
dotnet add package IronPdf
```

Or via Package Manager Console:
```powershell
Uninstall-Package Aspose.PDF
Install-Package IronPdf
```

### Step 2: Update Namespaces

```csharp
// ❌ Remove these
using Aspose.Pdf;
using Aspose.Pdf.Text;
using Aspose.Pdf.Facades;
using Aspose.Pdf.Generator;

// ✅ Add these
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
```

### Step 3: Update License Configuration

**Before (Aspose.PDF):**
```csharp
var license = new Aspose.Pdf.License();
license.SetLicense("Aspose.Pdf.lic");
```

**After (IronPDF):**
```csharp
IronPdf.License.LicenseKey = "YOUR-IRONPDF-LICENSE-KEY";
```

### Step 4: Convert Your First PDF

**Before (Aspose.PDF):**
```csharp
string html = "<h1>Hello World</h1>";
using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(html)))
{
    var doc = new Document(stream, new HtmlLoadOptions());
    doc.Save("output.pdf");
}
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

---

## Complete API Reference

### Namespace Mapping

| Aspose.PDF Namespace | IronPDF Namespace | Purpose |
|----------------------|-------------------|---------|
| `Aspose.Pdf` | `IronPdf` | Core PDF classes |
| `Aspose.Pdf.Text` | `IronPdf` | Text operations |
| `Aspose.Pdf.Facades` | `IronPdf` | High-level operations |
| `Aspose.Pdf.Generator` | `IronPdf` | PDF generation |
| `Aspose.Pdf.Annotations` | `IronPdf` | Annotations |
| `Aspose.Pdf.Forms` | `IronPdf.Forms` | Form handling |

### Core Class Mapping

| Aspose.PDF Class | IronPDF Equivalent | Notes |
|------------------|-------------------|-------|
| `Document` | `PdfDocument` | Main document class |
| `Page` | Accessed via `PdfDocument.Pages` | Page operations |
| `HtmlLoadOptions` | `ChromePdfRenderer` | HTML to PDF |
| `TextFragment` | Use HTML rendering | Text content |
| `TextFragmentAbsorber` | `PdfDocument.ExtractAllText()` | Text extraction |
| `PdfFileEditor` | `PdfDocument.Merge()` | Merge/split operations |
| `ImageStamp` | `PdfDocument.ApplyWatermark()` | Watermarks |
| `TextStamp` | `PdfDocument.ApplyWatermark()` | Text stamps |
| `WatermarkArtifact` | `PdfDocument.ApplyWatermark()` | Watermarks |
| `License` | `IronPdf.License` | Licensing |

### Complete Method Mapping

#### Document Operations

| Aspose.PDF Method | IronPDF Method | Notes |
|-------------------|----------------|-------|
| `new Document()` | `new PdfDocument()` | Empty document |
| `new Document(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `new Document(stream)` | `PdfDocument.FromBinaryData(bytes)` | Load from bytes |
| `doc.Save(path)` | `pdf.SaveAs(path)` | Save to file |
| `doc.Save(stream)` | `pdf.BinaryData` or `pdf.Stream` | Get bytes/stream |
| `doc.Pages.Count` | `pdf.PageCount` | Page count |
| `doc.Pages.Add()` | Use HTML rendering | Add pages |
| `doc.Pages.Delete(index)` | `pdf.RemovePage(index)` | Remove page |

#### HTML to PDF Conversion

| Aspose.PDF Method | IronPDF Method | Notes |
|-------------------|----------------|-------|
| `new HtmlLoadOptions()` | `new ChromePdfRenderer()` | HTML renderer |
| `new Document(stream, htmlOptions)` | `renderer.RenderHtmlAsPdf(html)` | HTML string |
| `new Document(path, htmlOptions)` | `renderer.RenderHtmlFileAsPdf(path)` | HTML file |
| `options.PageInfo.Width` | `RenderingOptions.PaperSize` | Page size |
| `options.PageInfo.Height` | `RenderingOptions.PaperSize` | Page size |
| `options.PageInfo.Margin` | `RenderingOptions.MarginTop`, etc. | Margins |
| `options.BasePath` | `renderer.RenderHtmlAsPdf(html, baseUrl)` | Base URL |

#### Text Extraction

| Aspose.PDF Method | IronPDF Method | Notes |
|-------------------|----------------|-------|
| `new TextAbsorber()` | Direct method call | No separate class |
| `page.Accept(absorber)` | `pdf.ExtractTextFromPage(index)` | Per-page |
| `absorber.Text` | `pdf.ExtractAllText()` | All text |
| `new TextFragmentAbsorber(pattern)` | Use regex on extracted text | Pattern matching |
| `absorber.TextFragments` | N/A | Use text string |

#### Merge and Split

| Aspose.PDF Method | IronPDF Method | Notes |
|-------------------|----------------|-------|
| `new PdfFileEditor()` | Direct method call | No separate class |
| `editor.Concatenate(files, output)` | `PdfDocument.Merge(pdfs)` | Merge files |
| `editor.Append(input, portFile, output)` | `pdf.AppendPdf(pdf2)` | Append |
| `editor.Extract(input, start, end, output)` | `pdf.CopyPages(start, end)` | Extract pages |
| `editor.SplitToPages(input)` | Loop with `CopyPage(i)` | Split to pages |
| `editor.Insert(input, pos, portFile)` | `pdf.InsertPdf(pdf2, index)` | Insert pages |

#### Watermarks and Stamps

| Aspose.PDF Method | IronPDF Method | Notes |
|-------------------|----------------|-------|
| `new TextStamp(text)` | `pdf.ApplyWatermark(html)` | Text watermark |
| `new ImageStamp(imagePath)` | `pdf.ApplyWatermark("<img src='...'/>")` | Image watermark |
| `new WatermarkArtifact()` | `pdf.ApplyWatermark(html)` | Watermark artifact |
| `stamp.XIndent`, `stamp.YIndent` | CSS positioning | Position |
| `stamp.Opacity` | CSS `opacity` | Transparency |
| `stamp.Rotate` | `rotation` parameter | Rotation |
| `stamp.Background` | `isBackground` parameter | Layer order |
| `page.AddStamp(stamp)` | `pdf.ApplyWatermark(html)` | Apply to page |

#### Security and Encryption

| Aspose.PDF Method | IronPDF Property | Notes |
|-------------------|------------------|-------|
| `doc.Encrypt(userPwd, ownerPwd, perms)` | `pdf.SecuritySettings` | Set security |
| N/A | `SecuritySettings.UserPassword` | User password |
| N/A | `SecuritySettings.OwnerPassword` | Owner password |
| `DocumentPrivilege.Print` | `SecuritySettings.AllowUserPrinting` | Print permission |
| `DocumentPrivilege.Copy` | `SecuritySettings.AllowUserCopyPasteContent` | Copy permission |
| `DocumentPrivilege.ModifyContent` | `SecuritySettings.AllowUserEdits` | Edit permission |

#### Forms

| Aspose.PDF Method | IronPDF Method | Notes |
|-------------------|----------------|-------|
| `doc.Form.Fields` | `pdf.Form.Fields` | Access fields |
| `field.Value` | `field.Value` | Get/set value |
| `field.Name` | `field.Name` | Field name |
| `doc.Form.FlattenAllFields()` | `pdf.Form.Flatten()` | Flatten form |
| `doc.Form[fieldName]` | `pdf.Form.GetFieldByName(name)` | Get by name |

#### Rendering Options

| Aspose.PDF Option | IronPDF Option | Notes |
|-------------------|----------------|-------|
| `options.PageInfo.Width/Height` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Paper size |
| `options.PageInfo.Margin` | `RenderingOptions.MarginTop`, etc. | Margins (mm) |
| `options.PageInfo.IsLandscape` | `RenderingOptions.PaperOrientation` | Orientation |
| N/A | `RenderingOptions.PrintHtmlBackgrounds = true` | Backgrounds |
| N/A | `RenderingOptions.EnableJavaScript = true` | JavaScript |
| N/A | `RenderingOptions.CssMediaType = Print` | CSS media |
| N/A | `RenderingOptions.WaitFor.RenderDelay(ms)` | Wait time |

---

## Code Migration Examples

### Example 1: HTML String to PDF

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        // Set license
        var license = new License();
        license.SetLicense("Aspose.Pdf.lic");

        string htmlContent = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; }
                    h1 { color: navy; }
                </style>
            </head>
            <body>
                <h1>Hello World</h1>
                <p>This is a PDF from HTML string.</p>
            </body>
            </html>";

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent)))
        {
            var options = new HtmlLoadOptions();
            options.PageInfo.Width = 612;
            options.PageInfo.Height = 792;
            options.PageInfo.Margin = new MarginInfo(20, 20, 20, 20);

            var document = new Document(stream, options);
            document.Save("output.pdf");
        }

        Console.WriteLine("PDF created successfully");
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
        // Set license (optional for development)
        License.LicenseKey = "YOUR-LICENSE-KEY";

        string htmlContent = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; }
                    h1 { color: navy; }
                </style>
            </head>
            <body>
                <h1>Hello World</h1>
                <p>This is a PDF from HTML string.</p>
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.MarginLeft = 20;
        renderer.RenderingOptions.MarginRight = 20;

        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("output.pdf");

        Console.WriteLine("PDF created successfully");
    }
}
```

### Example 2: HTML File to PDF

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;

var options = new HtmlLoadOptions();
options.BasePath = "C:/html_files/";
options.HtmlMediaType = HtmlMediaType.Print;

var document = new Document("input.html", options);
document.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 3: Merge Multiple PDFs

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf.Facades;

var editor = new PdfFileEditor();
string[] inputFiles = { "doc1.pdf", "doc2.pdf", "doc3.pdf" };
editor.Concatenate(inputFiles, "merged.pdf");

// Or with Document class
var document1 = new Document("doc1.pdf");
var document2 = new Document("doc2.pdf");

foreach (Page page in document2.Pages)
{
    document1.Pages.Add(page);
}

document1.Save("merged.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

// Simple merge
var pdf1 = PdfDocument.FromFile("doc1.pdf");
var pdf2 = PdfDocument.FromFile("doc2.pdf");
var pdf3 = PdfDocument.FromFile("doc3.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("merged.pdf");

// Or from a list
var files = new[] { "doc1.pdf", "doc2.pdf", "doc3.pdf" };
var pdfs = files.Select(PdfDocument.FromFile).ToList();
var mergedFromList = PdfDocument.Merge(pdfs);
mergedFromList.SaveAs("merged.pdf");
```

### Example 4: Text Extraction

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;
using Aspose.Pdf.Text;

var document = new Document("document.pdf");
var absorber = new TextAbsorber();

// Extract from all pages
foreach (Page page in document.Pages)
{
    page.Accept(absorber);
}

string extractedText = absorber.Text;
Console.WriteLine(extractedText);

// Or use TextFragmentAbsorber for specific patterns
var fragmentAbsorber = new TextFragmentAbsorber("search term");
document.Pages[1].Accept(fragmentAbsorber);

foreach (TextFragment fragment in fragmentAbsorber.TextFragments)
{
    Console.WriteLine($"Found: {fragment.Text} at {fragment.Position}");
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Text.RegularExpressions;

var pdf = PdfDocument.FromFile("document.pdf");

// Extract all text
string allText = pdf.ExtractAllText();
Console.WriteLine(allText);

// Extract from specific page
string page1Text = pdf.ExtractTextFromPage(0);

// Search for patterns
var matches = Regex.Matches(allText, "search term");
foreach (Match match in matches)
{
    Console.WriteLine($"Found: {match.Value} at position {match.Index}");
}
```

### Example 5: Add Watermark

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;
using Aspose.Pdf.Text;

var document = new Document("document.pdf");

// Text stamp
var textStamp = new TextStamp("CONFIDENTIAL");
textStamp.Background = true;
textStamp.XIndent = 100;
textStamp.YIndent = 100;
textStamp.Rotate = Rotation.on45;
textStamp.Opacity = 0.5;
textStamp.TextState.Font = FontRepository.FindFont("Arial");
textStamp.TextState.FontSize = 72;
textStamp.TextState.ForegroundColor = Color.Red;

foreach (Page page in document.Pages)
{
    page.AddStamp(textStamp);
}

document.Save("watermarked.pdf");

// Or using WatermarkArtifact
var artifact = new WatermarkArtifact();
artifact.SetTextAndState("DRAFT", new TextState
{
    Font = FontRepository.FindFont("Arial"),
    FontSize = 72,
    ForegroundColor = Color.Blue
});
artifact.ArtifactHorizontalAlignment = HorizontalAlignment.Center;
artifact.ArtifactVerticalAlignment = VerticalAlignment.Center;
artifact.Rotation = 45;
artifact.Opacity = 0.3;
artifact.IsBackground = true;

document.Pages[1].Artifacts.Add(artifact);
document.Save("watermarked.pdf");
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
    opacity: 0.5;
    font-family: Arial;
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

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;

var document = new Document("document.pdf");

// Set permissions
var privileges = DocumentPrivilege.ForbidAll;
privileges.AllowPrint = true;
privileges.AllowCopy = false;

document.Encrypt("userPassword", "ownerPassword", privileges, CryptoAlgorithm.AESx256);
document.Save("protected.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Set passwords
pdf.SecuritySettings.UserPassword = "userPassword";
pdf.SecuritySettings.OwnerPassword = "ownerPassword";

// Set permissions
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserAnnotations = false;

pdf.SaveAs("protected.pdf");
```

### Example 7: Form Field Manipulation

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;
using Aspose.Pdf.Forms;

var document = new Document("form.pdf");

// List all fields
foreach (Field field in document.Form.Fields)
{
    Console.WriteLine($"Field: {field.Name}, Type: {field.GetType().Name}");

    if (field is TextBoxField textField)
    {
        Console.WriteLine($"Value: {textField.Value}");
    }
}

// Set field values
if (document.Form["firstName"] is TextBoxField firstName)
{
    firstName.Value = "John";
}

if (document.Form["lastName"] is TextBoxField lastName)
{
    lastName.Value = "Doe";
}

// Flatten form
document.Flatten();
document.Save("filled_form.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("form.pdf");

// List all fields
foreach (var field in pdf.Form.Fields)
{
    Console.WriteLine($"Field: {field.Name}, Type: {field.Type}");
    Console.WriteLine($"Value: {field.Value}");
}

// Set field values
pdf.Form.GetFieldByName("firstName").Value = "John";
pdf.Form.GetFieldByName("lastName").Value = "Doe";

// Flatten form
pdf.Form.Flatten();
pdf.SaveAs("filled_form.pdf");
```

### Example 8: Split PDF

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;
using Aspose.Pdf.Facades;

// Using PdfFileEditor
var editor = new PdfFileEditor();
editor.SplitToPages("large_document.pdf", "page_{0}.pdf");

// Or using Document class
var document = new Document("large_document.pdf");
for (int i = 1; i <= document.Pages.Count; i++)
{
    var newDoc = new Document();
    newDoc.Pages.Add(document.Pages[i]);
    newDoc.Save($"page_{i}.pdf");
}
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

// Or extract a range
var firstFive = pdf.CopyPages(0, 4);
firstFive.SaveAs("first_5_pages.pdf");
```

### Example 9: PDF to Image Conversion

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;
using Aspose.Pdf.Devices;

var document = new Document("document.pdf");

for (int i = 1; i <= document.Pages.Count; i++)
{
    var resolution = new Resolution(300);
    var pngDevice = new PngDevice(resolution);

    using (var stream = new FileStream($"page_{i}.png", FileMode.Create))
    {
        pngDevice.Process(document.Pages[i], stream);
    }
}
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

### Example 10: Headers and Footers

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;
using Aspose.Pdf.Text;

var document = new Document();
var page = document.Pages.Add();

// Add header
var header = new HeaderFooter();
page.Header = header;
header.Margin.Top = 20;
var headerText = new TextFragment("Company Header");
headerText.TextState.Font = FontRepository.FindFont("Arial");
headerText.TextState.FontSize = 12;
header.Paragraphs.Add(headerText);

// Add footer with page numbers
var footer = new HeaderFooter();
page.Footer = footer;
var footerText = new TextFragment("Page $p of $P");
footer.Paragraphs.Add(footerText);

// Add content
page.Paragraphs.Add(new TextFragment("Content here"));

document.Save("with_headers.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='text-align:center; font-family:Arial; font-size:12px;'>
            Company Header
        </div>",
    DrawDividerLine = true,
    MaxHeight = 30
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='text-align:center; font-family:Arial; font-size:10px;'>
            Page {page} of {total-pages}
        </div>",
    DrawDividerLine = true,
    MaxHeight = 25
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Content here</h1>");
pdf.SaveAs("with_headers.pdf");
```

---

## Advanced Scenarios

### ASP.NET Core Integration

**Before (Aspose.PDF in ASP.NET):**
```csharp
public class PdfController : Controller
{
    public PdfController()
    {
        var license = new Aspose.Pdf.License();
        license.SetLicense("Aspose.Pdf.lic");
    }

    public IActionResult GeneratePdf()
    {
        string html = "<h1>Report</h1>";

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(html)))
        {
            var doc = new Document(stream, new HtmlLoadOptions());

            using (var outputStream = new MemoryStream())
            {
                doc.Save(outputStream);
                return File(outputStream.ToArray(), "application/pdf", "report.pdf");
            }
        }
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

**Before (Aspose.PDF):**
```csharp
try
{
    var license = new License();
    license.SetLicense("Aspose.Pdf.lic");

    var doc = new Document(htmlStream, new HtmlLoadOptions());
    doc.Save("output.pdf");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"License error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
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
```

### Docker Deployment

**Before (Aspose.PDF):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# Aspose.PDF may require additional fonts
RUN apt-get update && apt-get install -y fonts-liberation
WORKDIR /app
COPY Aspose.Pdf.lic /app/
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

---

## Performance Considerations

### Aspose.PDF vs IronPDF Performance

| Metric | Aspose.PDF | IronPDF |
|--------|-----------|---------|
| **HTML Rendering** | Documented slowdowns (30x in some cases) | Optimized Chromium engine |
| **Complex CSS** | Poor (Flying Saucer limitations) | Excellent (full CSS3) |
| **JavaScript** | Very limited | Full support |
| **Memory Usage** | Higher (document model overhead) | Efficient |
| **Linux** | Issues reported (CPU, memory leaks) | Stable |
| **Cold start** | Moderate | ~2s (Chromium init) |

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

### Issue 1: HtmlLoadOptions not found

**Symptom:** Class not available in IronPDF

**Solution:** Use `ChromePdfRenderer` instead:
```csharp
// ❌ Remove this
var doc = new Document(stream, new HtmlLoadOptions());

// ✅ Use this
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlString);
```

### Issue 2: TextFragment/TextFragmentAbsorber not found

**Symptom:** Text classes not available

**Solution:** Use direct text extraction:
```csharp
// ❌ Remove this
var absorber = new TextFragmentAbsorber();
page.Accept(absorber);
string text = absorber.Text;

// ✅ Use this
var pdf = PdfDocument.FromFile("doc.pdf");
string text = pdf.ExtractAllText();
```

### Issue 3: PdfFileEditor.Concatenate not available

**Symptom:** Facades classes not found

**Solution:** Use `PdfDocument.Merge()`:
```csharp
// ❌ Remove this
var editor = new PdfFileEditor();
editor.Concatenate(files, output);

// ✅ Use this
var pdfs = files.Select(PdfDocument.FromFile).ToList();
var merged = PdfDocument.Merge(pdfs);
merged.SaveAs(output);
```

### Issue 4: License file (.lic) not working

**Symptom:** Looking for license file

**Solution:** Use code-based licensing:
```csharp
// ❌ Remove this
var license = new License();
license.SetLicense("Aspose.Pdf.lic");

// ✅ Use this
IronPdf.License.LicenseKey = "YOUR-IRONPDF-KEY";
```

### Issue 5: HTML renders differently

**Symptom:** CSS not applied correctly

**Solution:** IronPDF's Chromium engine actually has better CSS support:
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
renderer.RenderingOptions.EnableJavaScript = true;
```

### Issue 6: ImageStamp/TextStamp not available

**Symptom:** Stamp classes not found

**Solution:** Use HTML-based watermarks:
```csharp
// ❌ Remove this
var stamp = new TextStamp("CONFIDENTIAL");
page.AddStamp(stamp);

// ✅ Use this
pdf.ApplyWatermark("<h1 style='color:red; opacity:0.5'>CONFIDENTIAL</h1>",
    rotation: 45);
```

### Issue 7: Page manipulation differences

**Symptom:** Page operations don't match

**Solution:**
```csharp
// Aspose: 1-indexed
document.Pages[1]  // First page

// IronPDF: 0-indexed
pdf.Pages[0]  // First page
```

### Issue 8: Font rendering issues

**Symptom:** Fonts look different

**Solution:** Use web fonts in HTML:
```csharp
var html = @"
<style>
    @import url('https://fonts.googleapis.com/css2?family=Roboto');
    body { font-family: 'Roboto', sans-serif; }
</style>
<body>Content</body>";
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Aspose.PDF usage in codebase**
  ```bash
  # Find all Aspose.PDF usages
  grep -r "Aspose\.Pdf\|HtmlLoadOptions\|TextFragmentAbsorber\|PdfFileEditor" --include="*.cs" .

  # Count occurrences
  grep -r "using Aspose.Pdf" --include="*.cs" . | wc -l
  ```
  **Why:** Understanding usage scope helps estimate migration effort and identify critical paths.

- [ ] **List all features used**
  ```csharp
  // Common Aspose.PDF features and their IronPDF equivalents:
  // HtmlLoadOptions + Document    → ChromePdfRenderer.RenderHtmlAsPdf()
  // TextFragmentAbsorber          → pdf.ExtractAllText()
  // PdfFileEditor.Concatenate()   → PdfDocument.Merge()
  // ImageStamp, TextStamp         → pdf.ApplyWatermark()
  // Document.Form.Fields          → pdf.Form.Fields
  // Document.Sign()               → pdf.Sign()
  // PdfContentEditor              → pdf.ApplyStamp()
  ```
  **Why:** Different Aspose.PDF features have specific IronPDF equivalents.

- [ ] **Calculate current Aspose.PDF licensing costs**
  ```csharp
  // Aspose.PDF pricing tiers (as of 2024):
  // Developer Small Business: $999/developer
  // Developer OEM: $4,997/developer
  // Site Small Business: $4,995/site
  // Site OEM: $23,980/site
  // Plus mandatory annual 40% renewal fees
  ```
  **Why:** Document cost savings from migration (Aspose is typically $999-$11,999/developer).

### Package Changes

- [ ] **Remove Aspose.PDF NuGet package**
  ```bash
  dotnet remove package Aspose.PDF
  dotnet remove package Aspose.PDF.Drawing  # If used
  ```
  **Why:** Clean removal avoids conflicts and licensing issues.

- [ ] **Install IronPDF**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Single package replacement with simpler licensing.

- [ ] **Update namespaces**
  ```csharp
  // Before
  using Aspose.Pdf;
  using Aspose.Pdf.Text;
  using Aspose.Pdf.Facades;

  // After
  using IronPdf;
  ```
  **Why:** Complete namespace replacement for clean migration.

### Code Updates

- [ ] **Replace HtmlLoadOptions with ChromePdfRenderer**
  ```csharp
  // Before (Aspose.PDF)
  var options = new HtmlLoadOptions();
  options.PageInfo.Width = 612;
  options.PageInfo.Height = 792;
  using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(html)))
  {
      var doc = new Document(stream, options);
      doc.Save("output.pdf");
  }

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF's Chromium engine provides superior HTML/CSS rendering.

- [ ] **Replace Document with PdfDocument**
  ```csharp
  // Before (Aspose.PDF) - loading existing PDF
  var doc = new Document("input.pdf");
  doc.Pages.Add();
  doc.Save("output.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("input.pdf");
  var blankPage = new ChromePdfRenderer().RenderHtmlAsPdf("<div style='height:100vh'></div>");
  pdf.AppendPdf(blankPage);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Similar class names but different instantiation patterns.

- [ ] **Replace TextFragmentAbsorber with ExtractAllText**
  ```csharp
  // Before (Aspose.PDF)
  var doc = new Document("input.pdf");
  var absorber = new TextFragmentAbsorber();
  doc.Pages.Accept(absorber);
  var textCollection = absorber.TextFragments;
  var sb = new StringBuilder();
  foreach (TextFragment fragment in textCollection)
  {
      sb.Append(fragment.Text);
  }
  string allText = sb.ToString();

  // After (IronPDF) - one line!
  var pdf = PdfDocument.FromFile("input.pdf");
  string allText = pdf.ExtractAllText();

  // Or per page
  foreach (var page in pdf.Pages)
  {
      string pageText = page.Text;
  }
  ```
  **Why:** IronPDF simplifies text extraction significantly.

- [ ] **Replace PdfFileEditor with PdfDocument.Merge**
  ```csharp
  // Before (Aspose.PDF)
  var editor = new PdfFileEditor();
  editor.Concatenate("doc1.pdf", "doc2.pdf", "merged.pdf");

  // Or with streams
  var editor = new PdfFileEditor();
  editor.Concatenate(
      new[] { stream1, stream2, stream3 },
      outputStream
  );

  // After (IronPDF)
  var merged = PdfDocument.Merge(
      PdfDocument.FromFile("doc1.pdf"),
      PdfDocument.FromFile("doc2.pdf")
  );
  merged.SaveAs("merged.pdf");

  // Or with multiple files
  var pdfs = pdfPaths.Select(PdfDocument.FromFile).ToList();
  var merged = PdfDocument.Merge(pdfs);
  ```
  **Why:** IronPDF merge is simpler and more intuitive.

- [ ] **Replace stamp classes with ApplyWatermark**
  ```csharp
  // Before (Aspose.PDF)
  var doc = new Document("input.pdf");
  var textStamp = new TextStamp("CONFIDENTIAL");
  textStamp.Background = false;
  textStamp.XIndent = 200;
  textStamp.YIndent = 400;
  textStamp.Rotate = Rotation.on45;
  textStamp.Opacity = 0.5;
  textStamp.TextState.Font = FontRepository.FindFont("Arial");
  textStamp.TextState.FontSize = 72;
  textStamp.TextState.ForegroundColor = Aspose.Pdf.Color.Red;
  foreach (Page page in doc.Pages)
  {
      page.AddStamp(textStamp);
  }
  doc.Save("watermarked.pdf");

  // After (IronPDF) - HTML watermark with full CSS
  var pdf = PdfDocument.FromFile("input.pdf");
  pdf.ApplyWatermark(@"
      <div style='
          font-family: Arial;
          font-size: 72px;
          color: rgba(255, 0, 0, 0.5);
          transform: rotate(-45deg);
      '>
          CONFIDENTIAL
      </div>");
  pdf.SaveAs("watermarked.pdf");
  ```
  **Why:** HTML-based watermarks are more flexible.

- [ ] **Update license activation code**
  ```csharp
  // Before (Aspose.PDF)
  var license = new Aspose.Pdf.License();
  license.SetLicense("Aspose.Pdf.lic");
  // Or
  license.SetLicense(licenseStream);

  // After (IronPDF)
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  // Or from environment
  IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");
  ```
  **Why:** Different license activation pattern.

- [ ] **Update page indexing (1-based to 0-based)**
  ```csharp
  // Before (Aspose.PDF) - 1-based indexing
  var doc = new Document("input.pdf");
  var firstPage = doc.Pages[1];  // First page
  var thirdPage = doc.Pages[3];  // Third page

  // After (IronPDF) - 0-based indexing
  var pdf = PdfDocument.FromFile("input.pdf");
  var firstPage = pdf.Pages[0];  // First page
  var thirdPage = pdf.Pages[2];  // Third page
  ```
  **Why:** Aspose uses 1-based indexing, IronPDF uses 0-based.

- [ ] **Replace form handling**
  ```csharp
  // Before (Aspose.PDF)
  var doc = new Document("form.pdf");
  var field = doc.Form["fieldName"] as TextBoxField;
  field.Value = "New Value";
  doc.Save("filled.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("form.pdf");
  pdf.Form.GetFieldByName("fieldName").Value = "New Value";
  pdf.SaveAs("filled.pdf");
  ```
  **Why:** Similar API with minor syntax differences.

### Testing

- [ ] **Verify HTML rendering quality**
  ```csharp
  // Test CSS features that Aspose.PDF struggled with
  var html = @"
  <style>
      .grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; }
      .flex { display: flex; justify-content: space-between; }
      .modern { backdrop-filter: blur(5px); }
  </style>
  <div class='grid'>
      <div>Column 1</div>
      <div>Column 2</div>
      <div>Column 3</div>
  </div>";

  var pdf = renderer.RenderHtmlAsPdf(html);
  // CSS Grid and Flexbox now render correctly!
  ```
  **Why:** IronPDF's Chromium engine should produce better results—verify the improvement.

- [ ] **Test edge cases**
  ```csharp
  // Test large documents
  var largePdf = PdfDocument.FromFile("large_document.pdf");
  Console.WriteLine($"Pages: {largePdf.PageCount}");
  Console.WriteLine($"Text length: {largePdf.ExtractAllText().Length}");

  // Test complex CSS
  var complexHtml = File.ReadAllText("complex_template.html");
  var pdf = renderer.RenderHtmlAsPdf(complexHtml);
  pdf.SaveAs("complex_test.pdf");
  ```
  **Why:** Ensure complex documents migrate correctly.

- [ ] **Compare performance metrics**
  ```csharp
  var sw = Stopwatch.StartNew();
  for (int i = 0; i < 100; i++)
  {
      var pdf = renderer.RenderHtmlAsPdf(html);
  }
  Console.WriteLine($"Average render time: {sw.ElapsedMilliseconds / 100}ms");

  // Note: First render includes Chromium startup (1-3s)
  // Subsequent renders are much faster
  ```
  **Why:** Document performance characteristics for capacity planning.

### Post-Migration

- [ ] **Remove Aspose.PDF license files**
  ```bash
  # Delete Aspose license files
  rm -f Aspose.Pdf.lic
  rm -f Aspose.Total.lic

  # Search for any remaining references
  grep -r "Aspose" --include="*.lic" --include="*.xml" .
  ```
  **Why:** Clean up removes licensing concerns and confusion.

- [ ] **Update Docker configurations**
  ```dockerfile
  # Before (Aspose.PDF may need fonts)
  FROM mcr.microsoft.com/dotnet/aspnet:8.0
  RUN apt-get update && apt-get install -y fonts-liberation

  # After (IronPDF)
  FROM mcr.microsoft.com/dotnet/aspnet:8.0
  # IronPDF auto-configures for Linux - minimal setup needed
  ENV IRONPDF_ENGINE_LINUX_ENABLE_SANDBOX=false
  ```
  **Why:** IronPDF has different runtime requirements.

- [ ] **Update CI/CD pipelines**
  ```yaml
  # GitHub Actions example
  - name: Restore dependencies
    run: dotnet restore

  - name: Set IronPDF License
    env:
      IRONPDF_LICENSE_KEY: ${{ secrets.IRONPDF_LICENSE_KEY }}
    run: echo "License configured"

  - name: Build
    run: dotnet build --no-restore
  ```
  **Why:** Ensure build process works with new package.
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

### Aspose.PDF References
- **Aspose.PDF API Reference**: https://reference.aspose.com/pdf/net/
- **TextFragment Class**: https://reference.aspose.com/pdf/net/aspose.pdf.text/textfragment/
- **HtmlLoadOptions Class**: https://reference.aspose.com/pdf/net/aspose.pdf/htmlloadoptions/
- **PdfFileEditor Class**: https://reference.aspose.com/pdf/net/aspose.pdf.facades/pdffileeditor/

---

*This migration guide covers the transition from Aspose.PDF for .NET to IronPDF. For additional assistance, contact IronPDF support or consult the official documentation.*
