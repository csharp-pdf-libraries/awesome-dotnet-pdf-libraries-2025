# How Do I Migrate from Foxit PDF SDK to IronPDF in C#?

## Table of Contents
1. [Why Migrate to IronPDF](#why-migrate-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Examples](#code-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate to IronPDF

### The Foxit SDK Challenges

Foxit PDF SDK is a powerful enterprise-level library, but it comes with significant complexity:

1. **Complex Licensing System**: Multiple products, SKUs, and license types (per-developer, per-server, OEM, etc.) make it difficult to choose the right option
2. **Enterprise Pricing**: Pricing is tailored for large organizations and can be prohibitive for smaller teams
3. **Manual Installation**: Requires manual DLL references or private NuGet feeds—no simple public NuGet package
4. **Verbose API**: Library initialization, error code checking, and explicit resource cleanup add boilerplate
5. **Separate HTML Conversion Add-on**: HTML to PDF conversion requires an additional add-on purchase
6. **Complex Configuration**: Settings require detailed object configuration (e.g., `HTML2PDFSettingData`)
7. **C++ Heritage**: API patterns reflect C++ origins, feeling less natural in modern C#

### Benefits of IronPDF

| Aspect | Foxit SDK | IronPDF |
|--------|-----------|---------|
| Installation | Manual DLLs/private feeds | Simple NuGet package |
| Licensing | Complex, enterprise-focused | Transparent, all sizes |
| Initialization | `Library.Initialize(sn, key)` | Set license key once |
| Error Handling | ErrorCode enums | Standard .NET exceptions |
| HTML to PDF | Separate add-on | Built-in Chromium |
| API Style | C++ heritage, verbose | Modern .NET patterns |
| Resource Cleanup | Manual `Close()`/`Release()` | IDisposable/automatic |
| Documentation | Enterprise docs | Public tutorials |

### Cost-Benefit Analysis

Moving from Foxit SDK to IronPDF offers:
- **Reduced complexity**: Simpler API, less boilerplate
- **Faster development**: Intuitive methods, less configuration
- **Modern .NET**: async/await, LINQ compatibility, standard patterns
- **HTML-first approach**: Use web skills for PDF generation
- **All-inclusive features**: No separate add-ons for HTML conversion

---

## Before You Start

### Prerequisites

1. **.NET Environment**: IronPDF supports .NET Framework 4.6.2+, .NET Core 3.1+, .NET 5/6/7/8/9+
2. **NuGet Access**: Ensure you can install packages from NuGet
3. **License Key**: Obtain your IronPDF license key for production use

### Backup Your Project

```bash
# Create a backup branch
git checkout -b pre-ironpdf-migration
git add .
git commit -m "Backup before Foxit SDK to IronPDF migration"
```

### Identify All Foxit SDK Usage

```bash
# Find all Foxit SDK references
grep -r "foxit\|PDFDoc\|PDFPage\|Library.Initialize\|Library.Release" --include="*.cs" --include="*.csproj" .

# Find Foxit DLL references
find . -name "*.csproj" | xargs grep -l "Foxit\|fsdk"
```

### Document Current Functionality

Before migration, catalog:
- Which Foxit features you use (HTML conversion, annotations, forms, security)
- License key locations and initialization code
- Custom configurations and settings
- Error handling patterns

---

## Quick Start Migration

### Step 1: Update NuGet Packages

```bash
# Foxit SDK typically requires manual removal of DLL references
# Check your .csproj for Foxit references and remove them

# Install IronPDF
dotnet add package IronPdf
```

**If you have Foxit references in .csproj:**
```xml
<!-- Remove these manually -->
<Reference Include="fsdk_dotnet">
    <HintPath>..\libs\Foxit\fsdk_dotnet.dll</HintPath>
</Reference>
```

### Step 2: Update Namespaces

```csharp
// Before
using foxit;
using foxit.common;
using foxit.common.fxcrt;
using foxit.pdf;
using foxit.pdf.annots;
using foxit.addon.conversion;

// After
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
```

### Step 3: Initialize IronPDF

```csharp
// Before (Foxit)
string sn = "YOUR_SERIAL_NUMBER";
string key = "YOUR_LICENSE_KEY";
ErrorCode error_code = Library.Initialize(sn, key);
if (error_code != ErrorCode.e_ErrSuccess)
{
    throw new Exception("Failed to initialize Foxit SDK");
}
// ... your code ...
Library.Release();

// After (IronPDF)
IronPdf.License.LicenseKey = "YOUR-IRONPDF-LICENSE-KEY";
// That's it! No Release() needed
```

### Step 4: Basic Conversion Pattern

```csharp
// Before (Foxit)
Library.Initialize(sn, key);
HTML2PDFSettingData settings = new HTML2PDFSettingData();
settings.page_width = 612.0f;
settings.page_height = 792.0f;
using (HTML2PDF html2pdf = new HTML2PDF(settings))
{
    html2pdf.Convert(htmlContent, "output.pdf");
}
Library.Release();

// After (IronPDF)
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

---

## Complete API Reference

### Namespace Mapping

| Foxit Namespace | IronPDF Equivalent | Notes |
|-----------------|-------------------|-------|
| `foxit` | `IronPdf` | Main namespace |
| `foxit.common` | `IronPdf` | Common types |
| `foxit.common.fxcrt` | N/A | Low-level (not needed) |
| `foxit.pdf` | `IronPdf` | PDF document operations |
| `foxit.pdf.annots` | `IronPdf.Editing` | Annotations |
| `foxit.pdf.actions` | `IronPdf` | Actions (links, etc.) |
| `foxit.pdf.graphics` | `IronPdf.Drawing` | Graphics operations |
| `foxit.addon.conversion` | `IronPdf.Rendering` | HTML/image conversion |

### Core Class Mapping

| Foxit SDK Class | IronPDF Equivalent | Notes |
|-----------------|-------------------|-------|
| `Library` | N/A | IronPDF auto-manages |
| `PDFDoc` | `PdfDocument` | Main document class |
| `PDFPage` | `PdfDocument.Pages[i]` | Page access |
| `HTML2PDF` | `ChromePdfRenderer` | HTML conversion |
| `Convert` | `ChromePdfRenderer` | Conversions |
| `TextPage` | `pdf.ExtractTextFromPage(i)` | Text extraction |
| `Watermark` | `TextStamper` / `ImageStamper` | Watermarks |
| `Security` | `SecuritySettings` | PDF security |
| `Annotation` | Various stampers | Annotations |
| `Form` | `pdf.Form` | Form fields |
| `Bookmark` | `pdf.BookMarks` | Bookmarks/outlines |
| `Metadata` | `pdf.MetaData` | Document metadata |

### Library Initialization

| Foxit Method | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `Library.Initialize(sn, key)` | `IronPdf.License.LicenseKey = "key"` | One-time setup |
| `Library.Release()` | N/A | Not needed |
| ErrorCode checks | Try/catch | Standard exceptions |

### PDFDoc Methods

| Foxit PDFDoc | IronPDF PdfDocument | Notes |
|--------------|---------------------|-------|
| `new PDFDoc(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `doc.LoadW(password)` | `PdfDocument.FromFile(path, password)` | Password protected |
| `doc.Load("")` | N/A | Load is automatic |
| `doc.GetPageCount()` | `pdf.PageCount` | Page count property |
| `doc.GetPage(index)` | `pdf.Pages[index]` | Get page by index |
| `doc.CreatePage()` | HTML rendering creates pages | Or merge existing |
| `doc.SaveAs(path, flags)` | `pdf.SaveAs(path)` | Save document |
| `doc.Close()` | `pdf.Dispose()` or using statement | Cleanup |
| `doc.InsertDocument()` | `PdfDocument.Merge()` | Merge documents |
| `doc.RemovePage(index)` | `pdf.RemovePages(indices)` | Remove pages |
| `doc.GetMetadata()` | `pdf.MetaData` | Property access |
| `doc.SetMetadata()` | `pdf.MetaData.Title = "..."` | Set metadata |

### PDFPage Methods

| Foxit PDFPage | IronPDF Equivalent | Notes |
|---------------|-------------------|-------|
| `page.GetWidth()` | `pdf.Pages[i].Width` | Page width |
| `page.GetHeight()` | `pdf.Pages[i].Height` | Page height |
| `page.GetRotation()` | `pdf.Pages[i].Rotation` | Page rotation |
| `page.StartParse()` | N/A | Auto-parsed |
| `page.StartEditing()` | Use stampers | Edit via stampers |
| `page.EndEditing()` | N/A | Auto-finalized |
| `page.DrawText()` | `TextStamper` | Draw text |
| `page.AddAnnot()` | Various stampers | Add annotations |
| `new TextPage(page)` | `pdf.ExtractTextFromPage(i)` | Text extraction |

### HTML2PDF / Conversion

| Foxit HTML2PDF | IronPDF Equivalent | Notes |
|----------------|-------------------|-------|
| `new HTML2PDFSettingData()` | `new ChromePdfRenderer()` | Create renderer |
| `settings.page_width` | `RenderingOptions.PaperSize` | Standard sizes |
| `settings.page_height` | `RenderingOptions.SetCustomPaperSize()` | Custom size |
| `settings.page_mode` | N/A | Multi-page by default |
| `html2pdf.Convert(html, path)` | `renderer.RenderHtmlAsPdf(html)` | HTML string |
| `html2pdf.ConvertFromURL(url, path)` | `renderer.RenderUrlAsPdf(url)` | URL conversion |
| `html2pdf.ConvertFromFile(path, out)` | `renderer.RenderHtmlFileAsPdf(path)` | HTML file |

### RenderingOptions (Settings)

| Foxit Setting | IronPDF RenderingOptions | Notes |
|---------------|-------------------------|-------|
| `page_width` (points) | `PaperSize` or `SetCustomPaperSize(w, h)` | Standard/custom |
| `page_height` (points) | As above | In mm for custom |
| `page_margin_top/bottom/left/right` | `MarginTop/Bottom/Left/Right` | In mm |
| `scale_mode` | `Zoom` | Zoom percentage |
| `page_orientation` | `PaperOrientation` | Portrait/Landscape |
| JavaScript timeout | `Timeout` | In seconds |
| Media type | `CssMediaType` | Print/Screen |

### Security Settings

| Foxit Security | IronPDF SecuritySettings | Notes |
|----------------|-------------------------|-------|
| `doc.Encrypt(password)` | `pdf.SecuritySettings.OwnerPassword` | Owner password |
| User password | `pdf.SecuritySettings.UserPassword` | User password |
| Print permission | `AllowUserPrinting` | Print rights |
| Copy permission | `AllowUserCopyPasteContent` | Copy rights |
| Modify permission | `AllowUserEdits` | Edit rights |
| `EncryptionLevel` | Automatic (AES-256) | Encryption level |

### Watermark

| Foxit Watermark | IronPDF Equivalent | Notes |
|-----------------|-------------------|-------|
| `new Watermark(doc, text, font, size, color)` | `new TextStamper()` | Text watermark |
| `WatermarkSettings.position` | `VerticalAlignment` + `HorizontalAlignment` | Position |
| `WatermarkSettings.rotation` | `Rotation` | Rotation angle |
| `WatermarkSettings.opacity` | `Opacity` | 0-100 percentage |
| `watermark.InsertToAllPages()` | `pdf.ApplyStamp(stamper)` | Apply to all |
| `watermark.InsertToPage(index)` | `pdf.ApplyStamp(stamper, indices)` | Specific pages |

### Text Extraction

| Foxit Text | IronPDF Equivalent | Notes |
|------------|-------------------|-------|
| `new TextPage(page, flags)` | `pdf.ExtractTextFromPage(i)` | Extract from page |
| `textPage.GetText()` | `pdf.ExtractAllText()` | All text |
| `textPage.GetCharCount()` | `text.Length` | Character count |
| `textPage.GetCharBox()` | N/A | Use OCR for positions |

### Form Fields

| Foxit Form | IronPDF Form | Notes |
|------------|--------------|-------|
| `doc.GetForm()` | `pdf.Form` | Form access |
| `form.GetFieldCount()` | `pdf.Form.Fields.Count` | Field count |
| `form.GetField(index)` | `pdf.Form.GetFieldByName(name)` | Get field |
| `field.GetValue()` | `field.Value` | Get value |
| `field.SetValue(value)` | `field.Value = value` | Set value |
| `form.FlattenFields()` | `pdf.Form.Flatten()` | Flatten forms |

---

## Code Examples

### Example 1: Basic HTML to PDF

**Before (Foxit SDK):**
```csharp
using foxit;
using foxit.common;
using foxit.addon.conversion;

class Program
{
    static void Main()
    {
        string sn = "YOUR_SN";
        string key = "YOUR_KEY";

        ErrorCode err = Library.Initialize(sn, key);
        if (err != ErrorCode.e_ErrSuccess)
        {
            Console.WriteLine("Failed to initialize library");
            return;
        }

        try
        {
            HTML2PDFSettingData settings = new HTML2PDFSettingData();
            settings.page_width = 612.0f;   // Letter width in points
            settings.page_height = 792.0f;  // Letter height in points
            settings.page_mode = HTML2PDFPageMode.e_HTML2PDFPageModeSinglePage;
            settings.page_margin_top = 72.0f;
            settings.page_margin_bottom = 72.0f;

            using (HTML2PDF html2pdf = new HTML2PDF(settings))
            {
                string html = "<html><body><h1>Hello World</h1><p>Generated with Foxit SDK</p></body></html>";
                html2pdf.Convert(html, "output.pdf");
            }

            Console.WriteLine("PDF created successfully");
        }
        finally
        {
            Library.Release();
        }
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
        renderer.RenderingOptions.MarginTop = 25.4;    // 1 inch in mm
        renderer.RenderingOptions.MarginBottom = 25.4;

        string html = "<html><body><h1>Hello World</h1><p>Generated with IronPDF</p></body></html>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");

        Console.WriteLine("PDF created successfully");
    }
}
```

### Example 2: Load PDF and Extract Text

**Before (Foxit SDK):**
```csharp
using foxit;
using foxit.common;
using foxit.pdf;
using System.Text;

class Program
{
    static void Main()
    {
        Library.Initialize("sn", "key");

        try
        {
            using (PDFDoc doc = new PDFDoc("input.pdf"))
            {
                ErrorCode err = doc.LoadW("");
                if (err != ErrorCode.e_ErrSuccess)
                {
                    Console.WriteLine("Failed to load document");
                    return;
                }

                StringBuilder allText = new StringBuilder();
                int pageCount = doc.GetPageCount();

                for (int i = 0; i < pageCount; i++)
                {
                    using (PDFPage page = doc.GetPage(i))
                    {
                        page.StartParse((int)PDFPage.ParseFlags.e_ParsePageNormal, null, false);

                        using (TextPage textPage = new TextPage(page,
                            TextPage.ParsingFlag.e_ParseTextNormal))
                        {
                            allText.AppendLine($"--- Page {i + 1} ---");
                            allText.AppendLine(textPage.GetText(0, textPage.GetCharCount()));
                        }
                    }
                }

                Console.WriteLine(allText.ToString());
            }
        }
        finally
        {
            Library.Release();
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Text;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        // Option 1: Extract all text at once
        string allText = pdf.ExtractAllText();
        Console.WriteLine(allText);

        // Option 2: Extract page by page
        var pageText = new StringBuilder();
        for (int i = 0; i < pdf.PageCount; i++)
        {
            pageText.AppendLine($"--- Page {i + 1} ---");
            pageText.AppendLine(pdf.ExtractTextFromPage(i));
        }
        Console.WriteLine(pageText.ToString());
    }
}
```

### Example 3: Merge PDFs

**Before (Foxit SDK):**
```csharp
using foxit;
using foxit.common;
using foxit.pdf;

class Program
{
    static void Main()
    {
        Library.Initialize("sn", "key");

        try
        {
            using (PDFDoc doc1 = new PDFDoc("document1.pdf"))
            using (PDFDoc doc2 = new PDFDoc("document2.pdf"))
            {
                doc1.LoadW("");
                doc2.LoadW("");

                // Get page count of second document
                int doc2PageCount = doc2.GetPageCount();

                // Create range for all pages in doc2
                Range[] ranges = new Range[] { new Range(0, doc2PageCount - 1) };
                RangeArray rangeArray = new RangeArray(ranges);

                // Insert doc2 pages at the end of doc1
                doc1.InsertDocument(doc1.GetPageCount(), doc2, rangeArray, 0);

                // Save merged document
                doc1.SaveAs("merged.pdf", (int)PDFDoc.SaveFlags.e_SaveFlagNoOriginal);
            }
        }
        finally
        {
            Library.Release();
        }
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf1 = PdfDocument.FromFile("document1.pdf");
        var pdf2 = PdfDocument.FromFile("document2.pdf");

        // Merge PDFs - one line!
        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");

        // Or merge multiple at once
        var pdf3 = PdfDocument.FromFile("document3.pdf");
        var mergedAll = PdfDocument.Merge(pdf1, pdf2, pdf3);
        mergedAll.SaveAs("merged_all.pdf");
    }
}
```

### Example 4: Add Watermark

**Before (Foxit SDK):**
```csharp
using foxit;
using foxit.common;
using foxit.pdf;

class Program
{
    static void Main()
    {
        Library.Initialize("sn", "key");

        try
        {
            using (PDFDoc doc = new PDFDoc("input.pdf"))
            {
                doc.LoadW("");

                // Create watermark settings
                WatermarkSettings settings = new WatermarkSettings();
                settings.flags = Watermark.e_WatermarkFlagASPageContents;
                settings.position = Watermark.Position.e_PosCenter;
                settings.offset_x = 0;
                settings.offset_y = 0;
                settings.scale_x = 1.0f;
                settings.scale_y = 1.0f;
                settings.rotation = -45.0f;
                settings.opacity = 0.3f;

                // Create font
                using (Font font = new Font(Font.StandardID.e_StdIDHelvetica))
                {
                    // Create watermark
                    Watermark watermark = new Watermark(doc, "CONFIDENTIAL", font, 72.0f, 0xFFFF0000);
                    watermark.SetSettings(settings);

                    // Apply to all pages
                    for (int i = 0; i < doc.GetPageCount(); i++)
                    {
                        watermark.InsertToPage(i);
                    }
                }

                doc.SaveAs("watermarked.pdf", (int)PDFDoc.SaveFlags.e_SaveFlagNoOriginal);
            }
        }
        finally
        {
            Library.Release();
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        // Option 1: HTML-based watermark
        pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>CONFIDENTIAL</h1>",
            rotation: -45,
            opacity: 30);

        // Option 2: TextStamper for more control
        var watermark = new TextStamper()
        {
            Text = "CONFIDENTIAL",
            FontSize = 72,
            FontFamily = "Helvetica",
            FontColor = Color.Red,
            Rotation = -45,
            Opacity = 30,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        pdf.ApplyStamp(watermark);

        pdf.SaveAs("watermarked.pdf");
    }
}
```

### Example 5: URL to PDF with Headers/Footers

**Before (Foxit SDK):**
```csharp
using foxit;
using foxit.addon.conversion;

class Program
{
    static void Main()
    {
        Library.Initialize("sn", "key");

        try
        {
            HTML2PDFSettingData settings = new HTML2PDFSettingData();
            settings.page_width = 595.0f;  // A4
            settings.page_height = 842.0f;
            settings.page_margin_top = 100.0f;
            settings.page_margin_bottom = 100.0f;

            // Foxit SDK has limited header/footer support
            // Often requires post-processing or additional code

            using (HTML2PDF html2pdf = new HTML2PDF(settings))
            {
                html2pdf.ConvertFromURL("https://www.example.com", "webpage.pdf");
            }
        }
        finally
        {
            Library.Release();
        }
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PrintHtmlBackgrounds = true;
        renderer.RenderingOptions.WaitFor.RenderDelay(3000);  // Wait for JS

        // Built-in header/footer support
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:center; font-size:12pt;'>Company Report</div>",
            DrawDividerLine = true
        };

        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:right; font-size:10pt;'>Page {page} of {total-pages}</div>",
            DrawDividerLine = true
        };

        var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

### Example 6: PDF Security and Encryption

**Before (Foxit SDK):**
```csharp
using foxit;
using foxit.pdf;

class Program
{
    static void Main()
    {
        Library.Initialize("sn", "key");

        try
        {
            using (PDFDoc doc = new PDFDoc("input.pdf"))
            {
                doc.LoadW("");

                // Set up security handler
                StdSecurityHandler securityHandler = new StdSecurityHandler();

                // Set passwords
                securityHandler.Initialize(
                    StdSecurityHandler.EncryptAlgorithm.e_CipherAES,
                    "user_password",
                    "owner_password",
                    PDFDoc.Permission.e_PermPrint |
                    PDFDoc.Permission.e_PermModify,
                    128);

                doc.SetSecurityHandler(securityHandler);

                doc.SaveAs("encrypted.pdf", (int)PDFDoc.SaveFlags.e_SaveFlagNoOriginal);
            }
        }
        finally
        {
            Library.Release();
        }
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        // Set passwords
        pdf.SecuritySettings.OwnerPassword = "owner_password";
        pdf.SecuritySettings.UserPassword = "user_password";

        // Set permissions
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.EditAll;
        pdf.SecuritySettings.AllowUserCopyPasteContent = true;
        pdf.SecuritySettings.AllowUserAnnotations = true;

        pdf.SaveAs("encrypted.pdf");
    }
}
```

### Example 7: Form Field Manipulation

**Before (Foxit SDK):**
```csharp
using foxit;
using foxit.pdf;
using foxit.pdf.interform;

class Program
{
    static void Main()
    {
        Library.Initialize("sn", "key");

        try
        {
            using (PDFDoc doc = new PDFDoc("form.pdf"))
            {
                doc.LoadW("");

                Form form = new Form(doc);
                int fieldCount = form.GetFieldCount();

                for (int i = 0; i < fieldCount; i++)
                {
                    Field field = form.GetField(i);
                    string fieldName = field.GetName();
                    string fieldValue = field.GetValue();

                    Console.WriteLine($"Field: {fieldName}, Value: {fieldValue}");

                    // Set new value
                    if (fieldName == "name")
                    {
                        field.SetValue("John Doe");
                    }
                }

                doc.SaveAs("filled_form.pdf", (int)PDFDoc.SaveFlags.e_SaveFlagNoOriginal);
            }
        }
        finally
        {
            Library.Release();
        }
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("form.pdf");

        // List all form fields
        foreach (var field in pdf.Form.Fields)
        {
            Console.WriteLine($"Field: {field.Name}, Value: {field.Value}");
        }

        // Set field value by name
        var nameField = pdf.Form.GetFieldByName("name");
        if (nameField != null)
        {
            nameField.Value = "John Doe";
        }

        // Or fill multiple fields at once
        pdf.Form.GetFieldByName("email").Value = "john@example.com";
        pdf.Form.GetFieldByName("phone").Value = "555-1234";

        pdf.SaveAs("filled_form.pdf");
    }
}
```

### Example 8: Set Document Metadata

**Before (Foxit SDK):**
```csharp
using foxit;
using foxit.pdf;

class Program
{
    static void Main()
    {
        Library.Initialize("sn", "key");

        try
        {
            using (PDFDoc doc = new PDFDoc("input.pdf"))
            {
                doc.LoadW("");

                Metadata metadata = doc.GetMetadata();

                metadata.SetValue("Title", "My Document");
                metadata.SetValue("Author", "John Doe");
                metadata.SetValue("Subject", "Important Report");
                metadata.SetValue("Keywords", "report, 2024, important");
                metadata.SetValue("Creator", "My Application");

                doc.SaveAs("with_metadata.pdf", (int)PDFDoc.SaveFlags.e_SaveFlagNoOriginal);
            }
        }
        finally
        {
            Library.Release();
        }
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        // Set metadata properties directly
        pdf.MetaData.Title = "My Document";
        pdf.MetaData.Author = "John Doe";
        pdf.MetaData.Subject = "Important Report";
        pdf.MetaData.Keywords = "report, 2024, important";
        pdf.MetaData.Creator = "My Application";

        pdf.SaveAs("with_metadata.pdf");
    }
}
```

---

## Advanced Scenarios

### Batch Processing

```csharp
using IronPdf;
using System.Threading.Tasks;

public class BatchProcessor
{
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    public async Task ProcessBatchAsync(List<string> htmlFiles, string outputFolder)
    {
        await Parallel.ForEachAsync(htmlFiles, async (file, ct) =>
        {
            string html = await File.ReadAllTextAsync(file);
            var pdf = _renderer.RenderHtmlAsPdf(html);

            string outputPath = Path.Combine(outputFolder,
                Path.GetFileNameWithoutExtension(file) + ".pdf");
            await Task.Run(() => pdf.SaveAs(outputPath));
        });
    }
}
```

### PDF Service with Dependency Injection

```csharp
using IronPdf;
using Microsoft.Extensions.DependencyInjection;

public interface IPdfService
{
    byte[] GeneratePdf(string html);
    string ExtractText(byte[] pdfData);
}

public class PdfService : IPdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        _renderer = new ChromePdfRenderer();
    }

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }

    public string ExtractText(byte[] pdfData)
    {
        using var ms = new MemoryStream(pdfData);
        var pdf = PdfDocument.FromStream(ms);
        return pdf.ExtractAllText();
    }
}

// Registration
services.AddSingleton<IPdfService, PdfService>();
```

---

## Performance Considerations

### Reuse ChromePdfRenderer

```csharp
// GOOD - Reuse renderer (thread-safe)
public class PdfService
{
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    public byte[] Generate(string html) => _renderer.RenderHtmlAsPdf(html).BinaryData;
}

// BAD - Creates new instance each time
public byte[] GenerateBad(string html)
{
    var renderer = new ChromePdfRenderer();  // Wasteful
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Dispose PdfDocument Objects

```csharp
// GOOD - Using statement for automatic cleanup
using (var pdf = PdfDocument.FromFile("large.pdf"))
{
    string text = pdf.ExtractAllText();
    // pdf is disposed automatically
}

// Or explicit dispose
var pdf = PdfDocument.FromFile("large.pdf");
try
{
    // Use pdf
}
finally
{
    pdf.Dispose();
}
```

### Unit Conversion Helper

Foxit uses points; IronPDF uses millimeters:

```csharp
public static class UnitConverter
{
    public static double PointsToMm(double points) => points * 0.352778;
    public static double MmToPoints(double mm) => mm / 0.352778;
    public static double InchesToMm(double inches) => inches * 25.4;
}

// Usage
renderer.RenderingOptions.MarginTop = UnitConverter.PointsToMm(72); // 72 points = ~25.4mm = 1 inch
```

---

## Troubleshooting

### Issue 1: Library.Initialize() Not Found

**Problem**: `Library.Initialize()` doesn't exist in IronPDF.

**Solution**: IronPDF uses a different initialization pattern:

```csharp
// Foxit
Library.Initialize(sn, key);

// IronPDF - just set license key once at startup
IronPdf.License.LicenseKey = "YOUR-KEY";
```

### Issue 2: ErrorCode Handling

**Problem**: Code checks `ErrorCode.e_ErrSuccess` but IronPDF doesn't have this.

**Solution**: Use standard .NET exception handling:

```csharp
// Foxit
ErrorCode err = doc.LoadW("");
if (err != ErrorCode.e_ErrSuccess) { /* handle error */ }

// IronPDF
try
{
    var pdf = PdfDocument.FromFile("input.pdf");
}
catch (IOException ex)
{
    Console.WriteLine($"Failed to load PDF: {ex.Message}");
}
```

### Issue 3: Page Index Out of Range

**Problem**: Getting errors when accessing pages.

**Solution**: Both use zero-based indexing, but check PageCount first:

```csharp
var pdf = PdfDocument.FromFile("input.pdf");
if (pdf.PageCount > 0)
{
    var text = pdf.ExtractTextFromPage(0);  // First page
}
```

### Issue 4: PDFDoc.Close() Not Found

**Problem**: `doc.Close()` method doesn't exist.

**Solution**: Use `Dispose()` or `using` statement:

```csharp
// Foxit
doc.Close();
Library.Release();

// IronPDF
using (var pdf = PdfDocument.FromFile("input.pdf"))
{
    // Use pdf
}  // Automatically disposed
```

### Issue 5: HTML2PDFSettingData Not Found

**Problem**: Can't find settings class for HTML conversion.

**Solution**: Use `RenderingOptions`:

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
renderer.RenderingOptions.MarginTop = 25;
renderer.RenderingOptions.MarginBottom = 25;
```

### Issue 6: Watermark Not Appearing

**Problem**: Watermark code runs but nothing appears.

**Solution**: Ensure you save after applying:

```csharp
var pdf = PdfDocument.FromFile("input.pdf");
pdf.ApplyWatermark("<h1>DRAFT</h1>", rotation: 45, opacity: 50);
pdf.SaveAs("output.pdf");  // Must save!
```

### Issue 7: Font Differences

**Problem**: PDF looks different from Foxit output.

**Solution**: Use web fonts or ensure system fonts are available:

```csharp
string html = @"
    <html>
    <head>
        <style>
            @import url('https://fonts.googleapis.com/css2?family=Roboto&display=swap');
            body { font-family: 'Roboto', sans-serif; }
        </style>
    </head>
    <body>Content</body>
    </html>";
```

### Issue 8: Memory Issues with Large PDFs

**Problem**: Memory errors when processing large documents.

**Solution**: Use streaming and dispose properly:

```csharp
// Process in chunks if needed
using (var pdf = PdfDocument.FromFile("large.pdf"))
{
    for (int i = 0; i < pdf.PageCount; i++)
    {
        string text = pdf.ExtractTextFromPage(i);
        ProcessText(text);
    }
}  // Memory released here
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Foxit SDK features used**
  ```bash
  grep -r "using Foxit" --include="*.cs" .
  grep -r "PDFDoc\|HTML2PDF" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document license key locations**
  ```csharp
  // Look for patterns like:
  Library.Initialize("serial-number", "license-key");
  ```
  **Why:** Ensure all license key usages are identified for removal and replacement with IronPDF.

- [ ] **Note all `Library.Initialize()` and `Library.Release()` calls**
  ```csharp
  // Before (Foxit)
  Library.Initialize("sn", "key");
  // ... PDF operations ...
  Library.Release();
  ```
  **Why:** IronPDF does not require explicit initialization or release, simplifying resource management.

- [ ] **List custom settings (page sizes, margins, etc.)**
  ```csharp
  // Before (Foxit)
  var settings = new HTML2PDFSettingData {
      PageSize = PageSizes.A4,
      Margins = new Margins(10, 10, 10, 10)
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Identify error handling patterns**
  ```csharp
  // Before (Foxit)
  if (errorCode != ErrorCode.Success) {
      // Handle error
  }
  ```
  **Why:** IronPDF uses standard .NET exceptions, which simplifies error handling.

- [ ] **Backup project to version control**
  **Why:** Ensure you have a safe rollback point before making significant changes.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Package Migration

- [ ] **Remove Foxit SDK DLL references**
  **Why:** Clean up project dependencies by removing unused DLLs.

- [ ] **Remove any private NuGet feed configurations**
  **Why:** Simplifies package management by using public NuGet sources.

- [ ] **Install `IronPdf` NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project for PDF generation capabilities.

- [ ] **Update namespace imports**
  ```csharp
  // Before (Foxit)
  using Foxit;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure all code references the correct library namespaces.

- [ ] **Set IronPDF license key at startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Code Migration

- [ ] **Remove `Library.Initialize()` and `Library.Release()` calls**
  ```csharp
  // Before (Foxit)
  Library.Initialize("sn", "key");
  // ... PDF operations ...
  Library.Release();
  ```
  **Why:** IronPDF does not require explicit initialization or release, simplifying resource management.

- [ ] **Replace `ErrorCode` checks with try/catch**
  ```csharp
  // Before (Foxit)
  if (errorCode != ErrorCode.Success) {
      // Handle error
  }

  // After (IronPDF)
  try {
      // PDF operations
  } catch (Exception ex) {
      // Handle exception
  }
  ```
  **Why:** IronPDF uses standard .NET exceptions, which simplifies error handling.

- [ ] **Replace `PDFDoc` with `PdfDocument`**
  ```csharp
  // Before (Foxit)
  var doc = new PDFDoc();

  // After (IronPDF)
  var doc = new PdfDocument();
  ```
  **Why:** Use IronPDF's PdfDocument for document manipulation.

- [ ] **Replace `HTML2PDF` with `ChromePdfRenderer`**
  ```csharp
  // Before (Foxit)
  var converter = new HTML2PDF();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses ChromePdfRenderer for modern HTML/CSS rendering.

- [ ] **Update page access from `GetPage(i)` to `Pages[i]`**
  ```csharp
  // Before (Foxit)
  var page = doc.GetPage(i);

  // After (IronPDF)
  var page = doc.Pages[i];
  ```
  **Why:** IronPDF provides a more intuitive collection-based page access.

- [ ] **Replace `SaveAs(path, flags)` with `SaveAs(path)`**
  ```csharp
  // Before (Foxit)
  doc.SaveAs("output.pdf", SaveFlags.Default);

  // After (IronPDF)
  doc.SaveAs("output.pdf");
  ```
  **Why:** Simplified saving method in IronPDF.

- [ ] **Replace `Close()` with `Dispose()` or using statements**
  ```csharp
  // Before (Foxit)
  doc.Close();

  // After (IronPDF)
  doc.Dispose();
  // or
  using (var doc = new PdfDocument()) {
      // PDF operations
  }
  ```
  **Why:** IronPDF supports IDisposable for automatic resource management.

- [ ] **Update watermark code to use `TextStamper` or `ApplyWatermark()`**
  ```csharp
  // Before (Foxit)
  var stamper = new TextStamper();

  // After (IronPDF)
  doc.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");
  ```
  **Why:** IronPDF provides built-in watermarking capabilities.

- [ ] **Convert units from points to millimeters**
  ```csharp
  // Before (Foxit)
  var margin = new Margins(72, 72, 72, 72); // Points

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 25.4; // Millimeters
  renderer.RenderingOptions.MarginBottom = 25.4;
  renderer.RenderingOptions.MarginLeft = 25.4;
  renderer.RenderingOptions.MarginRight = 25.4;
  ```
  **Why:** IronPDF uses millimeters for more intuitive unit handling.

### Testing

- [ ] **Verify HTML to PDF output matches expectations**
  **Why:** Ensure the new rendering engine produces the desired output.

- [ ] **Test PDF loading and text extraction**
  **Why:** Verify that existing PDFs can be loaded and manipulated correctly.

- [ ] **Verify merge functionality**
  ```csharp
  // Example
  var merged = PdfDocument.Merge(doc1, doc2);
  ```
  **Why:** Ensure PDFs can be combined as expected.

- [ ] **Check watermark appearance**
  **Why:** Confirm that watermarks are applied correctly and visibly.

- [ ] **Test security/encryption features**
  ```csharp
  // Example
  doc.SecuritySettings.UserPassword = "secret";
  ```
  **Why:** Ensure PDFs are secured as intended.

- [ ] **Validate form field operations**
  **Why:** Verify that form fields are accessible and functional.

- [ ] **Performance testing**
  **Why:** Ensure the application performs well with the new library.

### Post-Migration

- [ ] **Delete Foxit SDK DLLs**
  **Why:** Remove unnecessary files to clean up the project.

- [ ] **Remove Foxit-related configuration files**
  **Why:** Clean up configuration files that are no longer needed.

- [ ] **Update documentation**
  **Why:** Ensure all documentation reflects the new library usage.

- [ ] **Clean up unused helper code**
  **Why:** Remove any redundant code that was specific to the old library.
---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Foxit SDK Documentation**: https://developers.foxit.com/developer-hub/document/developer-guide-pdf-sdk-net/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
