# How Do I Migrate from ComPDFKit to IronPDF in C#?

## Table of Contents
1. [Why Migrate to IronPDF](#why-migrate-to-ironpdf)
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

## Why Migrate to IronPDF

### Key Advantages

| Aspect | ComPDFKit | IronPDF |
|--------|-----------|---------|
| **HTML-to-PDF** | Requires manual HTML parsing | Native Chromium rendering |
| **Market Maturity** | Newer entrant | 10+ years, battle-tested |
| **Community Size** | Smaller, limited Stack Overflow | Large, active community |
| **Documentation** | Some gaps | Extensive tutorials & guides |
| **Downloads** | Growing | 10+ million NuGet downloads |
| **API Style** | C++ influenced, verbose | Modern .NET fluent API |
| **Memory Management** | Manual `Release()` calls | Automatic GC handling |

### Feature Comparison

The [complete migration reference](https://ironpdf.com/blog/migration-guides/migrate-from-compdfkit-to-ironpdf/) offers deeper insights into these processes.

| Feature | ComPDFKit | IronPDF |
|---------|-----------|---------|
| HTML to PDF | Basic/Manual | ✅ Native Chromium |
| URL to PDF | Manual implementation | ✅ Built-in |
| Create PDF from scratch | ✅ | ✅ |
| PDF editing | ✅ | ✅ |
| Text extraction | ✅ | ✅ |
| Merge/Split | ✅ | ✅ |
| Digital signatures | ✅ | ✅ |
| Annotations | ✅ | ✅ |
| Form filling | ✅ | ✅ |
| PDF/A compliance | ✅ | ✅ |
| Watermarks | ✅ | ✅ |
| Cross-platform | Windows, Linux, macOS | Windows, Linux, macOS |
| .NET Core/.NET 5+ | ✅ | ✅ |

### Migration Benefits

1. **Superior HTML Rendering**: IronPDF's Chromium engine handles modern CSS3, JavaScript, and responsive layouts
2. **Mature Ecosystem**: 10+ years of refinement, extensive documentation, and proven stability
3. **Simpler API**: Less boilerplate code, no manual memory management with `Release()` calls
4. **Better .NET Integration**: Native async/await, LINQ support, fluent interfaces
5. **Extensive Resources**: Thousands of Stack Overflow answers and community examples
6. **Professional Support**: Enterprise-grade support and regular updates

---

## Before You Start

### Prerequisites

- **.NET Version**: IronPDF supports .NET Framework 4.6.2+, .NET Core 3.1+, .NET 5/6/7/8/9
- **NuGet Access**: Ensure you can install packages from nuget.org
- **License Key**: Obtain from [IronPDF website](https://ironpdf.com/) (free trial available)

### Find All ComPDFKit References

```bash
# Find all ComPDFKit usages in your codebase
grep -r "using ComPDFKit" --include="*.cs" .
grep -r "CPDFDocument\|CPDFPage\|CPDFAnnotation" --include="*.cs" .

# Find NuGet package references
grep -r "ComPDFKit" --include="*.csproj" .
grep -r "ComPDFKit" --include="packages.config" .
```

### Breaking Changes Overview

| Change | ComPDFKit | IronPDF | Impact |
|--------|-----------|---------|--------|
| **Document loading** | `CPDFDocument.InitWithFilePath()` | `PdfDocument.FromFile()` | Method name change |
| **Saving** | `document.WriteToFilePath()` | `pdf.SaveAs()` | Method name change |
| **Memory cleanup** | `document.Release()` required | Automatic (GC) | Remove manual cleanup |
| **Page access** | `document.PageAtIndex(i)` | `pdf.Pages[i]` | Array-style access |
| **Page indexing** | 0-based | 0-based | No change needed |
| **HTML rendering** | Manual implementation | `RenderHtmlAsPdf()` | Major simplification |
| **Text extraction** | `textPage.GetText()` | `pdf.ExtractAllText()` | Simplified API |
| **Page count** | `document.PageCount` property | `pdf.PageCount` | Same pattern |

---

## Quick Start (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove ComPDFKit packages
dotnet remove package ComPDFKit.NetCore
dotnet remove package ComPDFKit.NetFramework

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Using Statements

```csharp
// Before
using ComPDFKit.PDFDocument;
using ComPDFKit.PDFPage;
using ComPDFKit.PDFAnnotation;
using ComPDFKit.Import;

// After
using IronPdf;
```

### Step 3: Apply License Key

```csharp
// Add at application startup (Program.cs or Global.asax)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 4: Basic Code Migration

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;

var document = CPDFDocument.InitWithFilePath("input.pdf");
int pageCount = document.PageCount;
Console.WriteLine($"Pages: {pageCount}");
document.WriteToFilePath("output.pdf");
document.Release();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");
int pageCount = pdf.PageCount;
Console.WriteLine($"Pages: {pageCount}");
pdf.SaveAs("output.pdf");
// No Release() needed - GC handles cleanup
```

---

## Complete API Reference

### Namespace Mapping

| ComPDFKit Namespace | IronPDF Namespace | Notes |
|--------------------|-------------------|-------|
| `ComPDFKit.PDFDocument` | `IronPdf` | Main document operations |
| `ComPDFKit.PDFPage` | `IronPdf` | Page-level operations |
| `ComPDFKit.PDFAnnotation` | `IronPdf` | Annotations |
| `ComPDFKit.Import` | `IronPdf` | Import/conversion |
| `ComPDFKit.Conversion` | `IronPdf` | Format conversion |
| `ComPDFKit.Tools` | `IronPdf` | Utility functions |

### Document Operations

| Task | ComPDFKit | IronPDF |
|------|-----------|---------|
| Create empty document | `CPDFDocument.CreateDocument()` | `new PdfDocument()` |
| Load from file | `CPDFDocument.InitWithFilePath(path)` | `PdfDocument.FromFile(path)` |
| Load from stream | `CPDFDocument.InitWithStream(stream)` | `PdfDocument.FromStream(stream)` |
| Load from bytes | Via stream | `PdfDocument.FromBinaryData(bytes)` |
| Save to file | `document.WriteToFilePath(path)` | `pdf.SaveAs(path)` |
| Save to stream | `document.WriteToStream(stream)` | `pdf.Stream` |
| Save to bytes | Via stream | `pdf.BinaryData` |
| Get page count | `document.PageCount` | `pdf.PageCount` |
| Release/Dispose | `document.Release()` | Not required |
| Check if modified | `document.IsModified()` | Via document state |

### HTML to PDF Conversion

| Task | ComPDFKit | IronPDF |
|------|-----------|---------|
| HTML string to PDF | Manual implementation required | `renderer.RenderHtmlAsPdf(html)` |
| HTML file to PDF | Manual implementation required | `renderer.RenderHtmlFileAsPdf(path)` |
| URL to PDF | Manual implementation required | `renderer.RenderUrlAsPdf(url)` |
| Set page size | Via page creation parameters | `renderer.RenderingOptions.PaperSize` |
| Set margins | Via editor configuration | `renderer.RenderingOptions.MarginTop` etc. |
| Set orientation | Via page creation | `renderer.RenderingOptions.PaperOrientation` |
| Wait for JavaScript | N/A | `renderer.RenderingOptions.WaitFor.JavaScript(ms)` |

### Page Operations

| Task | ComPDFKit | IronPDF |
|------|-----------|---------|
| Access page | `document.PageAtIndex(index)` | `pdf.Pages[index]` |
| Insert page | `document.InsertPage(index, width, height, "")` | Via merge/manipulation |
| Delete page | `document.RemovePage(index)` | `pdf.RemovePages(index)` |
| Get page size | `page.GetPageSize()` | `page.Width` / `page.Height` |
| Rotate page | `page.SetRotation(angle)` | `pdf.Pages[i].Rotation = ...` |
| Copy pages | Manual page copying | `pdf.CopyPages(start, end)` |
| Extract pages | `document.ExtractPages(range)` | `pdf.CopyPages(indices)` |

### Text Operations

| Task | ComPDFKit | IronPDF |
|------|-----------|---------|
| Get text page | `page.GetTextPage()` | N/A (direct extraction) |
| Extract all text | Loop through pages with `GetText()` | `pdf.ExtractAllText()` |
| Extract page text | `textPage.GetText(start, count)` | `pdf.ExtractTextFromPage(i)` |
| Count characters | `textPage.CountChars()` | Via extracted text length |
| Search text | `textPage.FindText(query)` | Via extracted text |

### Merge and Split

| Task | ComPDFKit | IronPDF |
|------|-----------|---------|
| Merge documents | `doc1.ImportPagesAtIndex(doc2, range, index)` | `PdfDocument.Merge(pdf1, pdf2)` |
| Merge multiple | Loop with ImportPages | `PdfDocument.Merge(pdfList)` |
| Split document | Extract pages to new document | `pdf.CopyPages(start, end)` |
| Extract pages | `document.ExtractPages(range)` | `pdf.CopyPages(indices)` |

### Annotations

| Task | ComPDFKit | IronPDF |
|------|-----------|---------|
| Get annotations | `page.GetAnnotationList()` | Via PDF structure |
| Add annotation | `page.CreateAnnot(type)` | Via PDF editing |
| Delete annotation | `annotation.RemoveAnnot()` | Via PDF manipulation |
| Flatten annotations | `document.FlattenAllAnnotations()` | Via rendering |
| Import XFDF | `document.ImportAnnotations(xfdf)` | Via custom handling |
| Export XFDF | `document.ExportAnnotations(path)` | Via custom handling |

### Form Operations

| Task | ComPDFKit | IronPDF |
|------|-----------|---------|
| Get form | `document.GetForm()` | `pdf.Form` |
| Get all fields | `form.GetFieldCount()` + loop | `pdf.Form.Fields` |
| Get field by name | Loop through fields | `pdf.Form.GetFieldByName(name)` |
| Set field value | `field.SetValue(value)` | `pdf.Form.SetFieldValue(name, value)` |
| Flatten form | `document.FlattenForm()` | `pdf.Form.Flatten()` |
| Create form field | `page.CreateWidget(type, rect)` | Via form builder |

### Security and Encryption

| Task | ComPDFKit | IronPDF |
|------|-----------|---------|
| Set password | `document.SetPassword(password)` | `pdf.SecuritySettings.UserPassword = "..."` |
| Set owner password | Via encryption settings | `pdf.SecuritySettings.OwnerPassword = "..."` |
| Set permissions | `document.SetPermissions(flags)` | `pdf.SecuritySettings.AllowUser...` |
| Remove password | `document.RemovePassword()` | `pdf.SecuritySettings.RemovePasswordsAndEncryption()` |
| Check if encrypted | `document.IsEncrypted()` | Via security settings |

### Digital Signatures

| Task | ComPDFKit | IronPDF |
|------|-----------|---------|
| Sign document | `document.SignDocument(certificate)` | `pdf.Sign(signature)` |
| Add signature field | `page.CreateSignatureWidget(rect)` | Via signature creation |
| Verify signature | `signature.VerifySignature()` | Via signature object |

### Images and Watermarks

| Task | ComPDFKit | IronPDF |
|------|-----------|---------|
| Add image | Via editor `InsertImage()` | HTML `<img>` tag |
| Extract images | `page.GetImages()` | `pdf.ExtractAllImages()` |
| Add watermark | Via editor with transparency | `pdf.ApplyWatermark(html)` |
| Add stamp | `page.CreateStampAnnot()` | `pdf.ApplyStamp(stamp)` |
| PDF to image | `page.RenderPageBitmap(dpi)` | `pdf.RasterizeToImageFiles()` |

### Metadata

| Task | ComPDFKit | IronPDF |
|------|-----------|---------|
| Get/Set title | `document.GetTitle()` / `SetTitle()` | `pdf.MetaData.Title` |
| Get/Set author | `document.GetAuthor()` / `SetAuthor()` | `pdf.MetaData.Author` |
| Get/Set subject | `document.GetSubject()` / `SetSubject()` | `pdf.MetaData.Subject` |
| Get/Set keywords | `document.GetKeywords()` / `SetKeywords()` | `pdf.MetaData.Keywords` |
| Get creation date | `document.GetCreationDate()` | `pdf.MetaData.CreationDate` |

---

## Code Migration Examples

### Example 1: HTML to PDF

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;

class Program
{
    static void Main()
    {
        // ComPDFKit doesn't have native HTML-to-PDF
        // You would need to manually render HTML or use a separate library

        var document = CPDFDocument.CreateDocument();
        var page = document.InsertPage(0, 595, 842, "");

        var editor = page.GetEditor();
        editor.BeginEdit(CPDFEditType.EditText);

        // Manually add text elements
        var textArea = editor.CreateTextArea(
            new System.Drawing.RectangleF(50, 50, 500, 700),
            "Hello World");
        textArea.SetFontSize(24);

        editor.EndEdit();

        document.WriteToFilePath("output.pdf");
        document.Release();
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
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(@"
            <html>
            <body>
                <h1>Hello World</h1>
                <p>This is paragraph text.</p>
            </body>
            </html>");

        pdf.SaveAs("output.pdf");
    }
}
```

### Example 2: URL to PDF

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;
using System.Net.Http;

class Program
{
    static async Task Main()
    {
        // ComPDFKit requires manual URL fetching and HTML parsing
        using var client = new HttpClient();
        string html = await client.GetStringAsync("https://example.com");

        // Then manually convert HTML content...
        // This is complex and doesn't render properly
        var document = CPDFDocument.CreateDocument();
        // Manual HTML parsing required...

        document.WriteToFilePath("webpage.pdf");
        document.Release();
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
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.WaitFor.JavaScript(3000);
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

### Example 3: Text Extraction

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;
using System.Text;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.InitWithFilePath("document.pdf");
        var allText = new StringBuilder();

        for (int i = 0; i < document.PageCount; i++)
        {
            var page = document.PageAtIndex(i);
            var textPage = page.GetTextPage();

            int charCount = textPage.CountChars();
            if (charCount > 0)
            {
                string pageText = textPage.GetText(0, charCount);
                allText.AppendLine($"--- Page {i + 1} ---");
                allText.AppendLine(pageText);
            }

            textPage.Release();
            page.Release();
        }

        Console.WriteLine(allText.ToString());
        document.Release();
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
        var pdf = PdfDocument.FromFile("document.pdf");

        // Get all text at once
        string allText = pdf.ExtractAllText();
        Console.WriteLine(allText);

        // Or page by page
        for (int i = 0; i < pdf.PageCount; i++)
        {
            string pageText = pdf.ExtractTextFromPage(i);
            Console.WriteLine($"--- Page {i + 1} ---");
            Console.WriteLine(pageText);
        }
    }
}
```

### Example 4: Merge PDFs

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;

class Program
{
    static void Main()
    {
        var document1 = CPDFDocument.InitWithFilePath("file1.pdf");
        var document2 = CPDFDocument.InitWithFilePath("file2.pdf");
        var document3 = CPDFDocument.InitWithFilePath("file3.pdf");

        // Import all pages from document2
        string range2 = $"0-{document2.PageCount - 1}";
        document1.ImportPagesAtIndex(document2, range2, document1.PageCount);

        // Import all pages from document3
        string range3 = $"0-{document3.PageCount - 1}";
        document1.ImportPagesAtIndex(document3, range3, document1.PageCount);

        document1.WriteToFilePath("merged.pdf");

        document1.Release();
        document2.Release();
        document3.Release();
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
        var pdf1 = PdfDocument.FromFile("file1.pdf");
        var pdf2 = PdfDocument.FromFile("file2.pdf");
        var pdf3 = PdfDocument.FromFile("file3.pdf");

        var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
        merged.SaveAs("merged.pdf");
    }
}
```

### Example 5: Split PDF

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.InitWithFilePath("large.pdf");

        // Extract first 5 pages
        var part1 = CPDFDocument.CreateDocument();
        for (int i = 0; i < 5 && i < document.PageCount; i++)
        {
            var sourcePage = document.PageAtIndex(i);
            // Complex page copying logic...
            sourcePage.Release();
        }
        part1.WriteToFilePath("part1.pdf");
        part1.Release();

        // Extract pages 5-9
        var part2 = CPDFDocument.CreateDocument();
        for (int i = 5; i < 10 && i < document.PageCount; i++)
        {
            var sourcePage = document.PageAtIndex(i);
            // Complex page copying logic...
            sourcePage.Release();
        }
        part2.WriteToFilePath("part2.pdf");
        part2.Release();

        document.Release();
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
        var pdf = PdfDocument.FromFile("large.pdf");

        // Extract first 5 pages
        var part1 = pdf.CopyPages(0, 4);
        part1.SaveAs("part1.pdf");

        // Extract pages 5-9
        var part2 = pdf.CopyPages(5, 9);
        part2.SaveAs("part2.pdf");

        // Or specific non-consecutive pages
        var specific = pdf.CopyPages(new[] { 0, 2, 4, 6 });
        specific.SaveAs("specific.pdf");
    }
}
```

### Example 6: Add Watermark

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;
using System.Drawing;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.InitWithFilePath("document.pdf");

        for (int i = 0; i < document.PageCount; i++)
        {
            var page = document.PageAtIndex(i);
            var editor = page.GetEditor();

            editor.BeginEdit(CPDFEditType.EditText);

            // Create text with transparency
            var textArea = editor.CreateTextArea(
                new RectangleF(100, 300, 400, 100),
                "CONFIDENTIAL");
            textArea.SetFontSize(48);
            textArea.SetTransparency(0.3f);
            textArea.SetColor(Color.Red);
            textArea.SetRotation(45);

            editor.EndEdit();
            page.Release();
        }

        document.WriteToFilePath("watermarked.pdf");
        document.Release();
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
        var pdf = PdfDocument.FromFile("document.pdf");

        string watermarkHtml = @"
            <div style='
                font-size: 48px;
                color: rgba(255, 0, 0, 0.3);
                transform: rotate(-45deg);
                text-align: center;
                width: 100%;
                position: absolute;
                top: 40%;
            '>CONFIDENTIAL</div>";

        pdf.ApplyWatermark(watermarkHtml);
        pdf.SaveAs("watermarked.pdf");
    }
}
```

### Example 7: Form Filling

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.InitWithFilePath("form.pdf");
        var form = document.GetForm();

        int fieldCount = form.GetFieldCount();
        for (int i = 0; i < fieldCount; i++)
        {
            var field = form.GetField(i);
            string fieldName = field.GetFieldName();

            if (fieldName == "CustomerName")
            {
                field.SetValue("John Doe");
            }
            else if (fieldName == "Email")
            {
                field.SetValue("john@example.com");
            }
            else if (fieldName == "AgreeToTerms")
            {
                field.SetCheckBoxValue(true);
            }
        }

        // Flatten if needed
        document.FlattenForm();

        document.WriteToFilePath("filled-form.pdf");
        document.Release();
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
        var pdf = PdfDocument.FromFile("form.pdf");

        // Set values directly by field name
        pdf.Form.SetFieldValue("CustomerName", "John Doe");
        pdf.Form.SetFieldValue("Email", "john@example.com");
        pdf.Form.SetFieldValue("AgreeToTerms", "Yes");

        // List all fields
        foreach (var field in pdf.Form.Fields)
        {
            Console.WriteLine($"{field.Name}: {field.Value}");
        }

        // Flatten if needed
        pdf.Form.Flatten();

        pdf.SaveAs("filled-form.pdf");
    }
}
```

### Example 8: Encryption

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.InitWithFilePath("document.pdf");

        // Set encryption
        var encryptInfo = new CPDFPermissionsInfo
        {
            AllowsCopying = true,
            AllowsPrinting = true,
            AllowsDocumentChanges = false
        };

        document.SetPassword("owner123", "user123");
        document.SetPermissions(encryptInfo);
        document.WriteToFilePath("encrypted.pdf");
        document.Release();

        // Open encrypted document
        var encrypted = CPDFDocument.InitWithFilePath("encrypted.pdf");
        encrypted.UnlockWithPassword("user123");
        Console.WriteLine($"Pages: {encrypted.PageCount}");
        encrypted.Release();
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
        var pdf = PdfDocument.FromFile("document.pdf");

        // Set encryption
        pdf.SecuritySettings.OwnerPassword = "owner123";
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.AllowUserCopyPasteContent = true;
        pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

        pdf.SaveAs("encrypted.pdf");

        // Open encrypted document
        var encrypted = PdfDocument.FromFile("encrypted.pdf", "user123");
        Console.WriteLine($"Pages: {encrypted.PageCount}");
    }
}
```

### Example 9: PDF to Images

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;
using System.Drawing;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.InitWithFilePath("document.pdf");

        for (int i = 0; i < document.PageCount; i++)
        {
            var page = document.PageAtIndex(i);
            var size = page.GetPageSize();

            // Render at 300 DPI
            float scale = 300f / 72f;
            int width = (int)(size.Width * scale);
            int height = (int)(size.Height * scale);

            using (var bitmap = new Bitmap(width, height))
            {
                page.RenderPageBitmap(bitmap, width, height);
                bitmap.Save($"page-{i + 1}.png");
            }

            page.Release();
        }

        document.Release();
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
        var pdf = PdfDocument.FromFile("document.pdf");

        // Convert all pages at once
        pdf.RasterizeToImageFiles("page-*.png", 300); // 300 DPI

        // Or get as byte arrays
        var images = pdf.ToPngImages(300);
        for (int i = 0; i < images.Length; i++)
        {
            System.IO.File.WriteAllBytes($"page-{i + 1}.png", images[i]);
        }
    }
}
```

### Example 10: Digital Signature

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;
using ComPDFKit.DigitalSign;

class Program
{
    static void Main()
    {
        var document = CPDFDocument.InitWithFilePath("contract.pdf");

        // Create signature field
        var page = document.PageAtIndex(0);
        var signField = page.CreateSignatureWidget(
            new System.Drawing.RectangleF(50, 50, 200, 50));

        // Load certificate
        var signer = new CPDFSigner();
        signer.InitWithP12Certificate("certificate.pfx", "password");

        // Sign
        signer.SetReason("Contract Approval");
        signer.SetLocation("New York");
        signer.SignDocument(document, signField);

        document.WriteToFilePath("signed.pdf");
        document.Release();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Signing;

class Program
{
    static void Main()
    {
        var pdf = PdfDocument.FromFile("contract.pdf");

        var signature = new PdfSignature("certificate.pfx", "password")
        {
            SigningContact = "legal@example.com",
            SigningLocation = "New York",
            SigningReason = "Contract Approval"
        };

        pdf.Sign(signature);
        pdf.SaveAs("signed.pdf");
    }
}
```

---

## Advanced Scenarios

### ASP.NET Core Integration

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    [HttpGet("generate")]
    public IActionResult GenerateReport()
    {
        var document = CPDFDocument.CreateDocument();
        var page = document.InsertPage(0, 595, 842, "");

        // Add content manually...
        var editor = page.GetEditor();
        editor.BeginEdit(CPDFEditType.EditText);
        // Complex manual text placement...
        editor.EndEdit();

        using var stream = new MemoryStream();
        document.WriteToStream(stream);
        stream.Position = 0;

        document.Release();

        return File(stream.ToArray(), "application/pdf", "report.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    [HttpGet("generate")]
    public IActionResult GenerateReport()
    {
        string html = @"
            <html>
            <body>
                <h1>Monthly Report</h1>
                <p>Generated on: " + DateTime.Now + @"</p>
                <table>
                    <tr><th>Item</th><th>Value</th></tr>
                    <tr><td>Sales</td><td>$10,000</td></tr>
                </table>
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);

        return File(pdf.BinaryData, "application/pdf", "report.pdf");
    }
}
```

### Dependency Injection

**Before (ComPDFKit):**
```csharp
// No standard DI pattern for ComPDFKit
public class PdfService
{
    public byte[] CreatePdf(string content)
    {
        var document = CPDFDocument.CreateDocument();
        // Manual setup...

        using var stream = new MemoryStream();
        document.WriteToStream(stream);
        document.Release();

        return stream.ToArray();
    }
}
```

**After (IronPDF):**
```csharp
// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<ChromePdfRenderer>();
}

// Service
public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService(ChromePdfRenderer renderer)
    {
        _renderer = renderer;
    }

    public byte[] CreatePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Async Operations

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;

public class AsyncPdfGenerator
{
    // ComPDFKit is primarily synchronous
    public byte[] GeneratePdf(string content)
    {
        var document = CPDFDocument.CreateDocument();
        // Synchronous operations only...
        using var stream = new MemoryStream();
        document.WriteToStream(stream);
        document.Release();
        return stream.ToArray();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class AsyncPdfGenerator
{
    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }

    public async Task<byte[]> RenderUrlAsync(string url)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = await renderer.RenderUrlAsPdfAsync(url);
        return pdf.BinaryData;
    }
}
```

### Batch Processing

**Before (ComPDFKit):**
```csharp
using ComPDFKit.PDFDocument;

public class BatchProcessor
{
    public void ProcessFiles(string[] paths)
    {
        foreach (var path in paths)
        {
            var document = CPDFDocument.InitWithFilePath(path);

            // Process...
            var text = new StringBuilder();
            for (int i = 0; i < document.PageCount; i++)
            {
                var page = document.PageAtIndex(i);
                var textPage = page.GetTextPage();
                text.AppendLine(textPage.GetText(0, textPage.CountChars()));
                textPage.Release();
                page.Release();
            }

            // Add watermark manually...

            document.WriteToFilePath(path.Replace(".pdf", "-processed.pdf"));
            document.Release();
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Threading.Tasks;

public class BatchProcessor
{
    public void ProcessFiles(string[] paths)
    {
        // Process in parallel
        Parallel.ForEach(paths, path =>
        {
            var pdf = PdfDocument.FromFile(path);

            // Extract text
            string text = pdf.ExtractAllText();

            // Add watermark
            pdf.ApplyWatermark("<div style='opacity:0.3'>PROCESSED</div>");

            pdf.SaveAs(path.Replace(".pdf", "-processed.pdf"));
        });
    }
}
```

### Docker Deployment

**Before (ComPDFKit):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# ComPDFKit requires specific native dependencies
# Platform-specific configuration required...

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApp.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

**After (IronPDF):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# IronPDF Linux dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libx11-dev \
    libnss3 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libasound2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApp.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

---

## Performance Considerations

### Rendering Performance

| Scenario | ComPDFKit | IronPDF | Notes |
|----------|-----------|---------|-------|
| Simple documents | Fast | Fast | Similar performance |
| HTML rendering | N/A (no native support) | Fast | IronPDF advantage |
| Complex layouts | Manual implementation | Chromium handles | IronPDF easier |
| Large documents | Good | Good | Both handle well |
| Batch processing | Sequential only | Parallel support | IronPDF advantage |

### Memory Optimization

**ComPDFKit:**
```csharp
// Must manually release resources
var document = CPDFDocument.InitWithFilePath("large.pdf");
try
{
    // Process...
}
finally
{
    document.Release(); // Critical - memory leak if missed
}
```

**IronPDF:**
```csharp
// Automatic memory management
var pdf = PdfDocument.FromFile("large.pdf");
// Process...
// GC handles cleanup automatically

// Or explicit disposal if needed:
using (var pdf = PdfDocument.FromFile("large.pdf"))
{
    // Process...
}
```

---

## Troubleshooting Guide

### Issue 1: Manual Release() Not Called

**Error:** Memory leaks in ComPDFKit application

**Solution:** IronPDF handles memory automatically:

```csharp
// Before (ComPDFKit) - must remember Release()
var document = CPDFDocument.InitWithFilePath("file.pdf");
// If you forget document.Release(), memory leaks!

// After (IronPDF) - automatic cleanup
var pdf = PdfDocument.FromFile("file.pdf");
// No Release() needed
```

### Issue 2: No HTML-to-PDF Support

**Error:** ComPDFKit doesn't render HTML natively

**Solution:** IronPDF has built-in Chromium:

```csharp
// ComPDFKit: Manual text placement required
// IronPDF: Native HTML support
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
```

### Issue 3: Page Index Access

**Problem:** Different page access patterns

**Solution:** Use array indexer:

```csharp
// Before (ComPDFKit)
var page = document.PageAtIndex(0);

// After (IronPDF)
var page = pdf.Pages[0];
```

### Issue 4: Text Extraction Verbosity

**Problem:** ComPDFKit requires multiple steps for text extraction

**Solution:** IronPDF one-liner:

```csharp
// Before (ComPDFKit)
var page = document.PageAtIndex(0);
var textPage = page.GetTextPage();
string text = textPage.GetText(0, textPage.CountChars());
textPage.Release();
page.Release();

// After (IronPDF)
string text = pdf.ExtractAllText();
```

### Issue 5: Merge Complexity

**Problem:** ComPDFKit uses page range strings

**Solution:** IronPDF uses simple merge:

```csharp
// Before (ComPDFKit)
document1.ImportPagesAtIndex(document2, "0-9", document1.PageCount);

// After (IronPDF)
var merged = PdfDocument.Merge(pdf1, pdf2);
```

### Issue 6: License Configuration

**Problem:** Different licensing approaches

**Solution:**
```csharp
// Before (ComPDFKit)
CPDFDocument.VerifyLicense("license-key", "secret");

// After (IronPDF)
IronPdf.License.LicenseKey = "YOUR-IRONPDF-KEY";
```

### Issue 7: Form Field Access

**Problem:** ComPDFKit requires field iteration

**Solution:** IronPDF direct field access:

```csharp
// Before (ComPDFKit)
var form = document.GetForm();
for (int i = 0; i < form.GetFieldCount(); i++)
{
    var field = form.GetField(i);
    if (field.GetFieldName() == "Name")
        field.SetValue("John");
}

// After (IronPDF)
pdf.Form.SetFieldValue("Name", "John");
```

### Issue 8: Linux Deployment

**Error:** Missing native libraries

**Solution:** Install dependencies:

```bash
apt-get install -y libgdiplus libc6-dev libx11-dev libnss3 \
    libatk-bridge2.0-0 libdrm2 libxkbcommon0 libxcomposite1 \
    libxdamage1 libxfixes3 libxrandr2 libgbm1 libasound2
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all ComPDFKit usages in codebase**
  ```bash
  grep -r "using ComPDFKit" --include="*.cs" .
  grep -r "InitWithFilePath\|WriteToFilePath" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Note all `Release()` calls that need removal**
  ```csharp
  // Before (ComPDFKit)
  pdfDocument.Release();

  // After (IronPDF)
  // No equivalent needed, IronPDF handles memory automatically
  ```
  **Why:** IronPDF uses automatic garbage collection, eliminating the need for manual release calls.

- [ ] **Document current PDF workflows**
  ```csharp
  // Example workflow documentation
  var pdf = new PdfDocument();
  pdf.AddPage();
  pdf.Save("workflow.pdf");
  ```
  **Why:** Understanding current workflows helps ensure all functionalities are preserved post-migration.

- [ ] **Identify HTML rendering needs**
  ```csharp
  // Before (ComPDFKit)
  htmlRenderer.Render(htmlContent);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(htmlContent);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF provides superior HTML rendering with its Chromium engine.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Review IronPDF system requirements**
  **Why:** Ensure your environment meets the requirements for IronPDF to avoid runtime issues.

- [ ] **Set up test environment**
  **Why:** A controlled environment is essential for testing the migration without affecting production.

### During Migration

- [ ] **Remove ComPDFKit NuGet packages**
  ```bash
  dotnet remove package ComPDFKit
  ```
  **Why:** Clean removal of the old library to prevent conflicts.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project to start using its features.

- [ ] **Update using statements**
  ```csharp
  // Before (ComPDFKit)
  using ComPDFKit;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure your code references the correct namespaces for IronPDF.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Convert `InitWithFilePath()` to `FromFile()`**
  ```csharp
  // Before (ComPDFKit)
  var pdf = new PdfDocument();
  pdf.InitWithFilePath("document.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("document.pdf");
  ```
  **Why:** Simplifies loading PDFs with IronPDF's fluent API.

- [ ] **Convert `WriteToFilePath()` to `SaveAs()`**
  ```csharp
  // Before (ComPDFKit)
  pdf.WriteToFilePath("output.pdf");

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF's SaveAs method is straightforward and intuitive.

- [ ] **Remove all `Release()` calls**
  ```csharp
  // Before (ComPDFKit)
  pdfDocument.Release();

  // After (IronPDF)
  // No equivalent needed, IronPDF handles memory automatically
  ```
  **Why:** IronPDF uses automatic garbage collection, eliminating the need for manual release calls.

- [ ] **Convert manual text creation to HTML**
  ```csharp
  // Before (ComPDFKit)
  pdf.AddText("Hello, World!");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf("<p>Hello, World!</p>");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF leverages HTML for text rendering, providing more flexibility and styling options.

- [ ] **Update text extraction calls**
  ```csharp
  // Before (ComPDFKit)
  var text = pdf.ExtractText();

  // After (IronPDF)
  var text = pdf.ExtractAllText();
  ```
  **Why:** IronPDF provides straightforward methods for text extraction.

- [ ] **Update merge operations**
  ```csharp
  // Before (ComPDFKit)
  pdf.MergeWith(otherPdf);

  // After (IronPDF)
  var mergedPdf = PdfDocument.Merge(pdf, otherPdf);
  ```
  **Why:** IronPDF offers a simple merge method to combine PDFs.

- [ ] **Update form filling code**
  ```csharp
  // Before (ComPDFKit)
  pdf.FillFormField("fieldName", "value");

  // After (IronPDF)
  pdf.Form.FillField("fieldName", "value");
  ```
  **Why:** IronPDF provides a clear API for form manipulation.

- [ ] **Update security settings**
  ```csharp
  // Before (ComPDFKit)
  pdf.SetPassword("password");

  // After (IronPDF)
  pdf.SecuritySettings.UserPassword = "password";
  ```
  **Why:** IronPDF's security settings are comprehensive and easy to use.

### Post-Migration

- [ ] **Run all unit tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Test HTML rendering**
  **Why:** Ensure all HTML content is rendered as expected with IronPDF's engine.

- [ ] **Verify text extraction accuracy**
  **Why:** Confirm that text extraction works as intended with IronPDF.

- [ ] **Test form functionality**
  **Why:** Ensure all form fields are correctly filled and saved.

- [ ] **Performance test batch operations**
  **Why:** Validate that IronPDF handles large-scale operations efficiently.

- [ ] **Test in all target environments**
  **Why:** Ensure compatibility across all environments where the application is deployed.

- [ ] **Update CI/CD pipelines**
  **Why:** Integrate IronPDF into your build and deployment processes.

- [ ] **Update Docker configurations**
  **Why:** Ensure Docker images are configured correctly to support IronPDF.

- [ ] **Remove ComPDFKit license files**
  **Why:** Clean up any old license files that are no longer needed.

- [ ] **Update documentation**
  **Why:** Provide updated information on PDF generation and manipulation using IronPDF.
---

## Additional Resources

### IronPDF Documentation
- **Getting Started**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF**: https://ironpdf.com/how-to/html-file-to-pdf/

### ComPDFKit Reference (for migration)
- **Documentation**: https://www.compdf.com/documentation
- **GitHub Samples**: https://github.com/ComPDFKit/PDF-SDK-Windows
- **.NET Core SDK**: https://github.com/ComPDFKit/compdfkit-pdf-sdk-netcore

### Code Examples
- **IronPDF Examples**: https://ironpdf.com/examples/
- **Stack Overflow**: Search `[ironpdf]` tag

### Support
- **IronPDF Support**: https://ironpdf.com/support/
- **License Information**: https://ironpdf.com/licensing/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
