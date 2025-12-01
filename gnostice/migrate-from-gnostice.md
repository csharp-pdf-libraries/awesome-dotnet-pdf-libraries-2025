# How Do I Migrate from Gnostice to IronPDF in C#?

## Table of Contents
1. [Why Migrate from Gnostice to IronPDF](#why-migrate-from-gnostice-to-ironpdf)
2. [Migration Complexity Assessment](#migration-complexity-assessment)
3. [Before You Start](#before-you-start)
4. [Quick Start Migration](#quick-start-migration)
5. [Complete API Reference](#complete-api-reference)
6. [Code Migration Examples](#code-migration-examples)
7. [Advanced Scenarios](#advanced-scenarios)
8. [Performance Considerations](#performance-considerations)
9. [Troubleshooting](#troubleshooting)
10. [Migration Checklist](#migration-checklist)

---

## Why Migrate from Gnostice to IronPDF

### The Gnostice Problems

Gnostice Document Studio .NET and PDFOne have well-documented limitations that affect production applications:

1. **No External CSS Support**: Gnostice's documentation explicitly states it doesn't support external CSS stylesheets—a fundamental requirement for modern web-to-PDF conversion.

2. **No JavaScript Execution**: Dynamic content requiring JavaScript cannot be rendered, making it impossible to convert modern web applications accurately.

3. **Platform Fragmentation**: Separate products for WinForms, WPF, ASP.NET, Xamarin—each with different feature sets and APIs. You may need multiple licenses and codebases.

4. **Memory Leaks and Stability**: User forums and Stack Overflow report persistent memory leaks, JPEG Error #53, and StackOverflow exceptions when processing images.

5. **No Right-to-Left Unicode**: Arabic, Hebrew, and other RTL languages are explicitly unsupported—a dealbreaker for international applications.

6. **Limited Digital Signature Support**: While newer versions claim support, it has been historically missing or unreliable.

7. **Complex Product Line**: Document Studio .NET vs PDFOne vs separate viewer controls creates confusion about which product provides which features.

8. **Coordinate-Based API**: Many operations require manual X/Y positioning rather than modern layout approaches.

### IronPDF Advantages

| Aspect | Gnostice | IronPDF |
|--------|----------|---------|
| External CSS | Not supported | Full support |
| JavaScript Execution | Not supported | Full Chromium engine |
| RTL Languages | Not supported | Full Unicode support |
| Digital Signatures | Limited/Missing | Full X509 support |
| Platform | Fragmented products | Single unified library |
| Memory Stability | Reported issues | Stable, well-managed |
| HTML-to-PDF | Basic, internal engine | Chrome-quality rendering |
| Learning Curve | Complex API | Simple, intuitive API |
| Modern CSS (Flexbox, Grid) | Not supported | Full CSS3 support |

---

## Migration Complexity Assessment

### Estimated Effort by Feature

| Feature | Migration Complexity | Notes |
|---------|---------------------|-------|
| Load/Save PDFs | Very Low | Direct mapping |
| Merge PDFs | Very Low | Direct mapping |
| Split PDFs | Low | Similar approach |
| Text Extraction | Low | Method name change |
| Watermarks | Low | Simpler with IronPDF |
| Headers/Footers | Low | HTML-based approach |
| HTML to PDF | Low | Better with IronPDF |
| Encryption | Medium | Different API structure |
| Form Fields | Medium | Property access differences |
| Viewer Controls | High | IronPDF focuses on generation |
| Digital Signatures | Low | Now supported (wasn't in Gnostice) |

### Features You Gain

When migrating to IronPDF, these previously impossible features become available:
- External CSS stylesheets
- JavaScript execution
- RTL language support
- CSS Grid and Flexbox
- Digital signatures
- Better memory management
- Cross-platform support

---

## Before You Start

### Prerequisites

1. **.NET Version**: IronPDF supports .NET Framework 4.6.2+ and .NET Core 2.0+ / .NET 5+
2. **License Key**: Obtain your IronPDF license key from [ironpdf.com](https://ironpdf.com)
3. **Backup**: Create a branch for migration work

### Identify All Gnostice Usage

```bash
# Find all Gnostice references
grep -r "Gnostice\|PDFOne\|PDFDocument\|PDFPage\|DocExporter" --include="*.cs" .

# Find package references
grep -r "Gnostice\|PDFOne" --include="*.csproj" .
```

### NuGet Package Changes

```bash
# Remove Gnostice packages
dotnet remove package PDFOne.NET
dotnet remove package Gnostice.DocumentStudio.NET
dotnet remove package Gnostice.PDFOne.NET
dotnet remove package Gnostice.XtremeDocumentStudio.NET

# Install IronPDF
dotnet add package IronPdf
```

### License Key Setup

**Gnostice:**
```csharp
// Gnostice license often set via config or property
PDFOne.License.LicenseKey = "YOUR-GNOSTICE-LICENSE";
```

**IronPDF:**
```csharp
// Set once at application startup
IronPdf.License.LicenseKey = "YOUR-IRONPDF-LICENSE-KEY";

// Or in appsettings.json:
// { "IronPdf.License.LicenseKey": "YOUR-LICENSE-KEY" }
```

---

## Quick Start Migration

### Minimal Migration Example

**Before (Gnostice PDFOne):**
```csharp
using Gnostice.PDFOne;

// Load existing PDF
PDFDocument doc = new PDFDocument();
doc.Load("input.pdf");

// Add watermark to all pages
PDFFont font = new PDFFont(PDFStandardFont.Helvetica, 48);
foreach (PDFPage page in doc.Pages)
{
    PDFTextElement watermark = new PDFTextElement();
    watermark.Text = "CONFIDENTIAL";
    watermark.Font = font;
    watermark.RotationAngle = 45;
    watermark.Draw(page, 200, 400);
}

// Save
doc.Save("output.pdf");
doc.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Load existing PDF
var pdf = PdfDocument.FromFile("input.pdf");

// Add watermark with HTML styling - much simpler!
pdf.ApplyWatermark(
    "<div style='font-size:48px; transform:rotate(-45deg); opacity:0.3;'>CONFIDENTIAL</div>",
    opacity: 30,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center
);

// Save
pdf.SaveAs("output.pdf");
```

**Key Differences:**
- No coordinate calculations needed
- HTML/CSS for styling instead of font objects
- Automatic page application
- Simpler, cleaner code

---

## Complete API Reference

### Namespace Mapping

| Gnostice | IronPDF |
|----------|---------|
| `Gnostice.PDFOne` | `IronPdf` |
| `Gnostice.PDFOne.Document` | `IronPdf` |
| `Gnostice.PDFOne.Graphics` | `IronPdf.Editing` |
| `Gnostice.Documents` | `IronPdf` |
| `Gnostice.Documents.PDF` | `IronPdf` |
| `Gnostice.Documents.Controls` | N/A (use third-party viewer) |

### Core Class Mapping

| Gnostice | IronPDF | Description |
|----------|---------|-------------|
| `PDFDocument` | `PdfDocument` | Main PDF document class |
| `PDFPage` | `PdfDocument.Pages[i]` | Page representation |
| `PDFFont` | CSS styling | Font specification |
| `PDFTextElement` | HTML content | Text content |
| `PDFImageElement` | HTML `<img>` tags | Image content |
| `DocExporter` | `ChromePdfRenderer` | HTML/URL to PDF conversion |
| `DocumentManager` | `PdfDocument` static methods | Document loading |

### Document Operations

| Gnostice | IronPDF | Notes |
|----------|---------|-------|
| `new PDFDocument()` | `new PdfDocument()` | Create new document |
| `doc.Load(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `doc.Load(path, password)` | `PdfDocument.FromFile(path, password)` | Password-protected |
| `doc.Save(path)` | `pdf.SaveAs(path)` | Save to file |
| `doc.SaveToStream(stream)` | `pdf.Stream` or `pdf.BinaryData` | Get as stream/bytes |
| `doc.Close()` | `pdf.Dispose()` | Release resources |

### Page Operations

| Gnostice | IronPDF | Notes |
|----------|---------|-------|
| `doc.Pages.Count` | `pdf.PageCount` | Page count |
| `doc.Pages[index]` | `pdf.Pages[index]` | Access page |
| `doc.Pages.Add()` | Render HTML or merge | Add page |
| `doc.Pages.Insert(index)` | `pdf.Pages.Insert(index, page)` | Insert page |
| `doc.Pages.RemoveAt(index)` | `pdf.Pages.RemoveAt(index)` | Remove page |
| `page.Width` | `pdf.Pages[i].Width` | Page width |
| `page.Height` | `pdf.Pages[i].Height` | Page height |
| `page.Rotate` | `pdf.Pages[i].Rotation` | Page rotation |

### Merge and Split Operations

| Gnostice | IronPDF | Notes |
|----------|---------|-------|
| `doc1.Append(doc2)` | `PdfDocument.Merge(pdf1, pdf2)` | Merge documents |
| `doc.AppendDocument(path)` | Load + Merge | Append from file |
| `doc.DeletePages(start, count)` | `pdf.RemovePages(indices)` | Remove pages |
| `doc.ExtractPages(start, count)` | `pdf.CopyPages(indices)` | Extract pages |

### Text Operations

| Gnostice | IronPDF | Notes |
|----------|---------|-------|
| `doc.WriteText(text, x, y)` | Use HTML stamping | Add text at position |
| `page.Draw(textElement, x, y)` | Use HTML stamping | Draw text element |
| `doc.GetPageText(pageIndex)` | `pdf.ExtractTextFromPage(index)` | Extract text |
| `doc.Search(text)` | `pdf.ExtractAllText().Contains(text)` | Search text |

### Watermark Operations

| Gnostice | IronPDF | Notes |
|----------|---------|-------|
| `page.WriteWatermarkText(...)` | `pdf.ApplyWatermark(html)` | Text watermark |
| `page.Draw(imageElement, ...)` | `pdf.ApplyStamp(stamper)` | Image watermark |

### Header/Footer Operations

| Gnostice | IronPDF | Notes |
|----------|---------|-------|
| `doc.AddHeaderText(...)` | `renderer.RenderingOptions.HtmlHeader` | Header text |
| `doc.AddFooterText(...)` | `renderer.RenderingOptions.HtmlFooter` | Footer text |
| `doc.AddHeaderImage(...)` | Include in HTML header | Header image |
| `doc.AddFooterImage(...)` | Include in HTML footer | Footer image |
| Page number placeholders | `{page}` and `{total-pages}` | Page numbers |

### Encryption and Security

| Gnostice | IronPDF | Notes |
|----------|---------|-------|
| `doc.SetEncryption(...)` | `pdf.SecuritySettings` | Security config |
| `doc.SetUserPassword(pwd)` | `pdf.SecuritySettings.UserPassword` | Open password |
| `doc.SetOwnerPassword(pwd)` | `pdf.SecuritySettings.OwnerPassword` | Owner password |
| Various permission flags | `AllowUser*` properties | Permissions |

### HTML to PDF Conversion

| Gnostice | IronPDF | Notes |
|----------|---------|-------|
| `DocExporter.Export(doc, path, PDF)` | `renderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| `DocumentManager.LoadDocument(html)` | `renderer.RenderHtmlAsPdf(html)` | From HTML string |
| `DocumentManager.LoadURL(url)` | `renderer.RenderUrlAsPdf(url)` | From URL |
| N/A (no JavaScript) | `RenderingOptions.EnableJavaScript` | JS execution |
| N/A (no external CSS) | Automatic | External CSS |

### Form Field Operations

| Gnostice | IronPDF | Notes |
|----------|---------|-------|
| `doc.GetFormFields()` | `pdf.Form.Fields` | Form fields collection |
| `field.Name` | `pdf.Form.Fields[i].Name` | Field name |
| `field.Value` | `pdf.Form.Fields[i].Value` | Get/set value |
| `doc.FlattenFormFields()` | `pdf.Form.Flatten()` | Flatten forms |

### Metadata Operations

| Gnostice | IronPDF | Notes |
|----------|---------|-------|
| `doc.DocumentInfo.Title` | `pdf.MetaData.Title` | Document title |
| `doc.DocumentInfo.Author` | `pdf.MetaData.Author` | Author |
| `doc.DocumentInfo.Subject` | `pdf.MetaData.Subject` | Subject |
| `doc.DocumentInfo.Keywords` | `pdf.MetaData.Keywords` | Keywords |

---

## Code Migration Examples

### Example 1: HTML to PDF Conversion

**Before (Gnostice):**
```csharp
using Gnostice.Documents;
using Gnostice.Documents.PDF;

class Program
{
    static void Main()
    {
        // Limited - no external CSS, no JavaScript
        DocExporter exporter = new DocExporter();
        exporter.Preferences.PDFExportPreferences.ConvertLinksToBookmarks = true;
        exporter.Preferences.PDFExportPreferences.PageSize = PDFPageSize.A4;

        // Note: External CSS won't work!
        Document doc = DocumentManager.LoadDocument("report.html");
        exporter.Export(doc, "report.pdf", DocumentFormat.PDF);
        doc.Close();
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

        // Full CSS3, JavaScript, external resources - all work!
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.RenderDelay = 500;  // Wait for JS

        var pdf = renderer.RenderHtmlFileAsPdf("report.html");
        pdf.SaveAs("report.pdf");
    }
}
```

### Example 2: Merge Multiple PDFs

**Before (Gnostice PDFOne):**
```csharp
using Gnostice.PDFOne;
using Gnostice.PDFOne.Document;

class Program
{
    static void Main()
    {
        PDFDocument doc1 = new PDFDocument();
        doc1.Load("chapter1.pdf");

        PDFDocument doc2 = new PDFDocument();
        doc2.Load("chapter2.pdf");

        PDFDocument doc3 = new PDFDocument();
        doc3.Load("chapter3.pdf");

        // Create merged document
        PDFDocument mergedDoc = new PDFDocument();
        mergedDoc.Open();

        mergedDoc.Append(doc1);
        mergedDoc.Append(doc2);
        mergedDoc.Append(doc3);

        mergedDoc.Save("complete-book.pdf");

        // Must close all documents
        doc1.Close();
        doc2.Close();
        doc3.Close();
        mergedDoc.Close();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var files = new[] { "chapter1.pdf", "chapter2.pdf", "chapter3.pdf" };

        // Load all at once
        var pdfs = files.Select(f => PdfDocument.FromFile(f)).ToList();

        // Single merge call
        var merged = PdfDocument.Merge(pdfs);
        merged.SaveAs("complete-book.pdf");

        // Cleanup
        foreach (var pdf in pdfs) pdf.Dispose();
    }
}
```

### Example 3: Add Watermark to All Pages

**Before (Gnostice PDFOne):**
```csharp
using Gnostice.PDFOne;
using Gnostice.PDFOne.Graphics;
using System.Drawing;

class Program
{
    static void Main()
    {
        PDFDocument doc = new PDFDocument();
        doc.Load("contract.pdf");

        PDFFont font = new PDFFont(PDFStandardFont.Helvetica, 48);

        foreach (PDFPage page in doc.Pages)
        {
            // Calculate center position manually
            double centerX = page.Width / 2 - 100;  // Approximate
            double centerY = page.Height / 2;

            PDFTextElement watermark = new PDFTextElement();
            watermark.Text = "CONFIDENTIAL";
            watermark.Font = font;
            watermark.Color = Color.FromArgb(128, 255, 0, 0);  // Semi-transparent red
            watermark.RotationAngle = -45;

            watermark.Draw(page, centerX, centerY);
        }

        doc.Save("contract-watermarked.pdf");
        doc.Close();
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

        var pdf = PdfDocument.FromFile("contract.pdf");

        // HTML watermark - no coordinate calculations needed!
        pdf.ApplyWatermark(
            "<div style='color:red; font-size:48px; font-weight:bold; " +
            "transform:rotate(-45deg); opacity:0.3;'>CONFIDENTIAL</div>",
            opacity: 30,
            VerticalAlignment.Middle,
            HorizontalAlignment.Center
        );

        pdf.SaveAs("contract-watermarked.pdf");
    }
}
```

### Example 4: Extract Text from PDF

**Before (Gnostice PDFOne):**
```csharp
using Gnostice.PDFOne;
using Gnostice.PDFOne.Document;
using System;
using System.Text;

class Program
{
    static void Main()
    {
        PDFDocument doc = new PDFDocument();
        doc.Load("document.pdf");

        StringBuilder allText = new StringBuilder();

        for (int i = 1; i <= doc.Pages.Count; i++)  // Note: Often 1-indexed
        {
            string pageText = doc.GetPageText(i);
            allText.AppendLine($"--- Page {i} ---");
            allText.AppendLine(pageText);
        }

        Console.WriteLine(allText.ToString());
        doc.Close();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");

        // Extract all at once
        string allText = pdf.ExtractAllText();
        Console.WriteLine(allText);

        // Or page by page (0-indexed)
        for (int i = 0; i < pdf.PageCount; i++)
        {
            string pageText = pdf.ExtractTextFromPage(i);
            Console.WriteLine($"--- Page {i + 1} ---");
            Console.WriteLine(pageText);
        }
    }
}
```

### Example 5: Password Protection

**Before (Gnostice PDFOne):**
```csharp
using Gnostice.PDFOne;

class Program
{
    static void Main()
    {
        PDFDocument doc = new PDFDocument();
        doc.Load("document.pdf");

        // Set encryption (API varies by version)
        doc.SetEncryption(
            PDFEncryptionMethod.AES256,
            "user123",      // User password
            "owner456",     // Owner password
            PDFPermissions.None | PDFPermissions.PrintDocument
        );

        doc.Save("protected.pdf");
        doc.Close();
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

        var pdf = PdfDocument.FromFile("document.pdf");

        // Security settings with readable property names
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
        pdf.SecuritySettings.AllowUserAnnotations = false;

        pdf.SaveAs("protected.pdf");
    }
}
```

### Example 6: Digital Signatures (Gnostice Doesn't Support!)

**Before (Gnostice):**
```csharp
// Digital signatures were NOT supported in Gnostice PDFOne!
// This was a documented limitation.
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Signing;
using System.Security.Cryptography.X509Certificates;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("contract.pdf");

        // Load certificate
        var cert = new X509Certificate2("certificate.pfx", "cert-password");

        // Create and apply signature
        var signature = new PdfSignature(cert)
        {
            SigningContact = "legal@company.com",
            SigningLocation = "New York, NY",
            SigningReason = "Contract Approval"
        };

        pdf.Sign(signature);
        pdf.SaveAs("signed-contract.pdf");
    }
}
```

### Example 7: Headers and Footers with Page Numbers

**Before (Gnostice PDFOne):**
```csharp
using Gnostice.PDFOne;

class Program
{
    static void Main()
    {
        PDFDocument doc = new PDFDocument();
        doc.Load("document.pdf");

        // Add header to all pages
        doc.AddHeaderText("Company Report",
            new PDFFont(PDFStandardFont.Helvetica, 10),
            PDFHorizontalAlignment.Center,
            10);  // margin

        // Add footer with page numbers
        // Using placeholders (if supported by version)
        doc.AddFooterText("Page %n of %N",
            new PDFFont(PDFStandardFont.Helvetica, 10),
            PDFHorizontalAlignment.Right,
            10);

        doc.Save("with-headers.pdf");
        doc.Close();
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

        // Full HTML headers and footers
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:center; font-size:10px;'>" +
                          "Company Report</div>",
            DrawDividerLine = true
        };

        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:right; font-size:10px;'>" +
                          "Page {page} of {total-pages}</div>",
            DrawDividerLine = true
        };

        var pdf = renderer.RenderHtmlAsPdf("<h1>Report Content</h1><p>...</p>");
        pdf.SaveAs("with-headers.pdf");

        // Or add to existing PDF
        var existing = PdfDocument.FromFile("document.pdf");
        existing.AddHtmlHeaders(new HtmlHeaderFooter()
        {
            HtmlFragment = "<div>Header</div>"
        });
        existing.AddHtmlFooters(new HtmlHeaderFooter()
        {
            HtmlFragment = "<div>Page {page} of {total-pages}</div>"
        });
        existing.SaveAs("existing-with-headers.pdf");
    }
}
```

### Example 8: RTL Language Support (Gnostice Doesn't Support!)

**Before (Gnostice):**
```csharp
// Right-to-left languages (Arabic, Hebrew) were NOT supported!
// This was a documented limitation in Gnostice.
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

        // Arabic content - fully supported via Chromium
        string arabicHtml = @"
            <!DOCTYPE html>
            <html dir='rtl' lang='ar'>
            <head>
                <meta charset='UTF-8'>
                <style>
                    body {
                        font-family: 'Arial', 'Tahoma', sans-serif;
                        direction: rtl;
                        text-align: right;
                    }
                </style>
            </head>
            <body>
                <h1>مرحبا بك في IronPDF</h1>
                <p>دعم كامل للغة العربية والعبرية وجميع اللغات من اليمين إلى اليسار.</p>
                <p>هذا لم يكن ممكنا مع Gnostice!</p>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(arabicHtml);
        pdf.SaveAs("arabic-document.pdf");
    }
}
```

### Example 9: Form Field Manipulation

**Before (Gnostice PDFOne):**
```csharp
using Gnostice.PDFOne;
using Gnostice.PDFOne.Forms;

class Program
{
    static void Main()
    {
        PDFDocument doc = new PDFDocument();
        doc.Load("form.pdf");

        // Get form fields
        var fields = doc.GetFormFields();

        foreach (var field in fields)
        {
            Console.WriteLine($"Field: {field.Name}, Value: {field.Value}");

            if (field.Name == "FirstName")
            {
                field.Value = "John";
            }
        }

        doc.FlattenFormFields();
        doc.Save("filled-form.pdf");
        doc.Close();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("form.pdf");

        // List all fields
        foreach (var field in pdf.Form.Fields)
        {
            Console.WriteLine($"Field: {field.Name}, Type: {field.Type}");
        }

        // Fill fields by name
        pdf.Form.Fields["FirstName"].Value = "John";
        pdf.Form.Fields["LastName"].Value = "Doe";
        pdf.Form.Fields["Email"].Value = "john@example.com";

        // Optionally flatten
        pdf.Form.Flatten();

        pdf.SaveAs("filled-form.pdf");
    }
}
```

### Example 10: Split PDF into Pages

**Before (Gnostice PDFOne):**
```csharp
using Gnostice.PDFOne;
using Gnostice.PDFOne.Document;

class Program
{
    static void Main()
    {
        PDFDocument sourceDoc = new PDFDocument();
        sourceDoc.Load("multipage.pdf");

        int pageCount = sourceDoc.Pages.Count;

        for (int i = 1; i <= pageCount; i++)
        {
            PDFDocument singlePage = new PDFDocument();
            singlePage.Open();

            // Clone page from source
            singlePage.Append(sourceDoc, i, i);  // Start and end page

            singlePage.Save($"page_{i}.pdf");
            singlePage.Close();
        }

        sourceDoc.Close();
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

        var pdf = PdfDocument.FromFile("multipage.pdf");

        // Split into single pages (0-indexed)
        for (int i = 0; i < pdf.PageCount; i++)
        {
            var singlePage = pdf.CopyPage(i);
            singlePage.SaveAs($"page_{i + 1}.pdf");
        }
    }
}
```

---

## Advanced Scenarios

### Handling Viewer Control Migration

Gnostice provides viewer controls; IronPDF focuses on generation. For viewing:

```csharp
// Option 1: Embed as base64 in HTML
var pdf = PdfDocument.FromFile("document.pdf");
string base64 = Convert.ToBase64String(pdf.BinaryData);
string embedHtml = $@"
    <embed src='data:application/pdf;base64,{base64}'
           type='application/pdf'
           width='100%' height='600px' />";

// Option 2: Use PDF.js (JavaScript viewer)
// Option 3: Use a dedicated viewer component (PSPDFKit, etc.)
```

### Batch Processing with Memory Stability

One of Gnostice's main issues was memory leaks. IronPDF handles this better:

```csharp
using IronPdf;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var htmlFiles = Directory.GetFiles("input", "*.html");

        // Process in parallel without memory issues
        await Parallel.ForEachAsync(htmlFiles, async (file, ct) =>
        {
            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlFileAsPdf(file);
            pdf.SaveAs(Path.ChangeExtension(file, ".pdf"));
            pdf.Dispose();  // Explicit cleanup if desired
        });
    }
}
```

### Modern CSS Features (Not Available in Gnostice)

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Flexbox layout - Gnostice couldn't render this!
        string flexboxHtml = @"
            <style>
                .container {
                    display: flex;
                    justify-content: space-between;
                    gap: 20px;
                }
                .card {
                    flex: 1;
                    padding: 20px;
                    border: 1px solid #ddd;
                    border-radius: 8px;
                }
            </style>
            <div class='container'>
                <div class='card'>Card 1</div>
                <div class='card'>Card 2</div>
                <div class='card'>Card 3</div>
            </div>";

        // CSS Grid - Also not supported by Gnostice!
        string gridHtml = @"
            <style>
                .grid {
                    display: grid;
                    grid-template-columns: repeat(3, 1fr);
                    gap: 15px;
                }
            </style>
            <div class='grid'>
                <div>Item 1</div>
                <div>Item 2</div>
                <div>Item 3</div>
            </div>";

        var pdf = renderer.RenderHtmlAsPdf(flexboxHtml + gridHtml);
        pdf.SaveAs("modern-layout.pdf");
    }
}
```

---

## Performance Considerations

### Memory Management

**Gnostice Issue:** Memory leaks reported in user forums.

**IronPDF Solution:**
```csharp
// Use 'using' statements
using var pdf = PdfDocument.FromFile("document.pdf");
// Automatically disposed

// Or explicit disposal for long-running processes
var pdf2 = renderer.RenderHtmlAsPdf(html);
try
{
    pdf2.SaveAs("output.pdf");
}
finally
{
    pdf2.Dispose();
}
```

### Renderer Reuse

```csharp
// Reuse renderer for batch operations
var renderer = new ChromePdfRenderer();

// Disable unnecessary features for speed
renderer.RenderingOptions.EnableJavaScript = false;
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

foreach (var html in htmlDocuments)
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{count++}.pdf");
}
```

---

## Troubleshooting

### Issue 1: PDFDocument Class Conflict

**Problem:** Both libraries have `PDFDocument` class.

**Solution:** Use fully qualified names during transition:
```csharp
// During migration
IronPdf.PdfDocument ironPdf = IronPdf.PdfDocument.FromFile("input.pdf");
Gnostice.PDFOne.PDFDocument gnosticePdf = new Gnostice.PDFOne.PDFDocument();
```

### Issue 2: Page Indexing Differences

**Problem:** Gnostice often uses 1-indexed pages; IronPDF uses 0-indexed.

**Solution:**
```csharp
// Gnostice: for (int i = 1; i <= pageCount; i++)
// IronPDF:  for (int i = 0; i < pdf.PageCount; i++)

// When converting page references:
int gnosticePage = 5;
int ironPdfIndex = gnosticePage - 1;  // = 4
```

### Issue 3: Coordinate System Changes

**Problem:** Gnostice uses coordinate-based drawing; IronPDF uses HTML.

**Solution:** Convert coordinate positioning to CSS:
```csharp
// Gnostice: element.Draw(page, 100, 200);  // x=100, y=200

// IronPDF: Use absolute positioning
var stamper = new HtmlStamper()
{
    Html = "<div style='position:absolute; left:100px; top:200px;'>Text</div>"
};
```

### Issue 4: Font Handling

**Problem:** Gnostice uses `PDFFont` objects; IronPDF uses CSS.

**Solution:**
```csharp
// Gnostice
PDFFont font = new PDFFont(PDFStandardFont.Helvetica, 12);

// IronPDF - use CSS
var html = "<span style='font-family: Helvetica, Arial, sans-serif; font-size: 12pt;'>Text</span>";
```

### Issue 5: License Not Working

**Problem:** License key not activating.

**Solution:**
```csharp
// Set at very start of application
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Verify
Console.WriteLine($"Licensed: {IronPdf.License.IsValidLicense(IronPdf.License.LicenseKey)}");
```

### Issue 6: External CSS Now Works

**Problem:** CSS that didn't work in Gnostice now renders correctly.

**Solution:** This is a good thing! IronPDF's Chromium engine handles all CSS properly. You may need to adjust layouts that were designed to work around Gnostice's limitations.

### Issue 7: JavaScript Content Now Renders

**Problem:** Content that was missing in Gnostice (JS-generated) now appears.

**Solution:** Enable JavaScript and add appropriate delays:
```csharp
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.RenderDelay = 1000;  // Wait for JS
```

### Issue 8: DocExporter Not Found

**Problem:** `DocExporter` class doesn't exist in IronPDF.

**Solution:** Use `ChromePdfRenderer`:
```csharp
// Gnostice
DocExporter exporter = new DocExporter();
exporter.Export(doc, "output.pdf", DocumentFormat.PDF);

// IronPDF
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Gnostice usage in codebase**
  ```bash
  grep -r "using Gnostice" --include="*.cs" .
  grep -r "PDFDocument\|DocExporter" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Identify viewer control dependencies (need alternative)**
  **Why:** IronPDF does not provide viewer controls; consider alternatives like PDF.js for web applications.

- [ ] **Note features that weren't working (CSS, JS, RTL) - they'll work now!**
  **Why:** IronPDF supports external CSS, JavaScript execution, and RTL languages, improving document rendering accuracy.

- [ ] **Document memory issues for comparison testing**
  **Why:** Track current memory usage to compare with IronPDF's more stable memory management.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Create migration branch in version control**
  **Why:** Isolate changes to safely test and review migration progress.

- [ ] **Set up test environment**
  **Why:** Ensure a controlled environment to validate the migration without affecting production.

### Code Migration

- [ ] **Remove Gnostice NuGet packages**
  ```bash
  dotnet remove package Gnostice
  ```
  **Why:** Clean removal of old dependencies to avoid conflicts.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add the new library to enable PDF generation with IronPDF.

- [ ] **Update namespace imports**
  ```csharp
  // Before (Gnostice)
  using Gnostice.PDFOne;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Update namespaces to reference IronPDF's classes and methods.

- [ ] **Replace license key setup**
  ```csharp
  // Before (Gnostice)
  Gnostice.LicenseManager.SetLicenseKey("YOUR-OLD-LICENSE-KEY");

  // After (IronPDF)
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations with IronPDF.

- [ ] **Convert `PDFDocument` to `PdfDocument`**
  ```csharp
  // Before (Gnostice)
  var doc = new PDFDocument();

  // After (IronPDF)
  var doc = PdfDocument.FromFile("existing.pdf");
  ```
  **Why:** Use IronPDF's PdfDocument class for document manipulation.

- [ ] **Convert `DocExporter` to `ChromePdfRenderer`**
  ```csharp
  // Before (Gnostice)
  var exporter = new DocExporter();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses ChromePdfRenderer for rendering HTML and URLs.

- [ ] **Update page indexing (1-indexed → 0-indexed if applicable)**
  **Why:** Ensure correct page references, as IronPDF may use zero-based indexing.

- [ ] **Replace coordinate-based drawing with HTML stamping**
  ```csharp
  // Before (Gnostice)
  doc.DrawText("Hello", x, y);

  // After (IronPDF)
  doc.ApplyWatermark("<div style='position:absolute; left:10px; top:10px;'>Hello</div>");
  ```
  **Why:** Use HTML for flexible and modern layout capabilities.

- [ ] **Update `PDFFont` to CSS styling**
  ```csharp
  // Before (Gnostice)
  var font = new PDFFont("Arial", 12);

  // After (IronPDF)
  var html = "<span style='font-family: Arial; font-size: 12px;'>Text</span>";
  ```
  **Why:** CSS styling offers more control and compatibility with web standards.

- [ ] **Convert encryption to `SecuritySettings`**
  ```csharp
  // Before (Gnostice)
  doc.SetEncryption("userPassword", "ownerPassword");

  // After (IronPDF)
  doc.SecuritySettings.UserPassword = "userPassword";
  doc.SecuritySettings.OwnerPassword = "ownerPassword";
  ```
  **Why:** IronPDF provides a straightforward API for setting document security.

### Feature Testing

- [ ] **Test HTML to PDF conversion**
  **Why:** Ensure HTML content renders correctly with IronPDF's Chromium engine.

- [ ] **Verify external CSS now works**
  **Why:** IronPDF supports external stylesheets, improving document styling.

- [ ] **Test JavaScript-dependent content**
  **Why:** Dynamic content should render accurately with IronPDF's JavaScript support.

- [ ] **Test RTL languages (if needed)**
  **Why:** IronPDF supports RTL languages, expanding internationalization capabilities.

- [ ] **Test digital signatures (now available!)**
  **Why:** Verify the ability to sign documents with IronPDF's digital signature support.

- [ ] **Test PDF merging**
  ```csharp
  // Example
  var merged = PdfDocument.Merge(pdf1, pdf2);
  ```
  **Why:** Combine multiple PDFs into one document using IronPDF's merging capabilities.

- [ ] **Test watermarking**
  ```csharp
  // Example
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");
  ```
  **Why:** Add watermarks for document status or branding.

- [ ] **Test form filling**
  **Why:** Ensure form fields are correctly populated with IronPDF.

- [ ] **Compare memory usage**
  **Why:** Validate improved memory management with IronPDF in production scenarios.

### Post-Migration

- [ ] **Remove Gnostice license**
  **Why:** Clean up old license references to avoid confusion.

- [ ] **Update documentation**
  **Why:** Ensure all documentation reflects the new IronPDF usage and capabilities.

- [ ] **Remove workarounds for Gnostice limitations**
  **Why:** Simplify codebase by removing unnecessary workarounds.

- [ ] **Train team on IronPDF API**
  **Why:** Ensure team members are familiar with IronPDF's features and API.

- [ ] **Monitor production for any issues**
  **Why:** Early detection of issues ensures a smooth transition to IronPDF in production.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [IronPDF Code Examples](https://ironpdf.com/examples/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
