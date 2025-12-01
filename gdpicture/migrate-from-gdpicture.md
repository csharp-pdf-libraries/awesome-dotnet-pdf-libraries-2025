# How Do I Migrate from GdPicture.NET to IronPDF in C#?

## Table of Contents
1. [Why Migrate from GdPicture.NET to IronPDF](#why-migrate-from-gdpicturenet-to-ironpdf)
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

## Why Migrate from GdPicture.NET to IronPDF

### The GdPicture.NET Problems

GdPicture.NET (now rebranded as Nutrient) is a comprehensive document imaging SDK with several challenges for PDF-focused development:

1. **Overkill for PDF-Only Projects**: GdPicture is a full document imaging suite including OCR, barcode, scanning, and image processing. If you only need PDF functionality, you're paying for features you'll never use.

2. **Complex Licensing**: Multiple product tiers (GdPicture.NET 14, GdPicture.API, Ultimate, Professional) with confusing SKU combinations and annual subscription requirements.

3. **Enterprise Pricing**: License costs start at $2,999 for the PDF plugin alone, scaling to $10,000+ for the Ultimate edition. Per-developer licensing adds significant overhead.

4. **Steep Learning Curve**: The API is designed around document imaging concepts, not modern .NET patterns. Methods like `LicenseManager.RegisterKEY()`, `GdPictureStatus` enum checking, and 1-indexed pages feel dated.

5. **Status Code Pattern**: Every operation returns a `GdPictureStatus` enum that must be checked—no exceptions thrown on errors, making error handling verbose.

6. **Manual Resource Management**: Requires explicit `Dispose()` or `Release()` calls. The SDK doesn't follow standard .NET disposal patterns cleanly.

7. **Version Lock-in**: The namespace `GdPicture14` includes the version number, making major version upgrades require namespace changes throughout your codebase.

8. **Rebranding Confusion**: The recent rebrand to "Nutrient" creates documentation fragmentation between gdpicture.com and nutrient.io.

### IronPDF Advantages

| Aspect | GdPicture.NET | IronPDF |
|--------|---------------|---------|
| Focus | Document imaging suite (overkill for PDF) | PDF-specific library |
| Pricing | $2,999-$10,000+ enterprise tier | Competitive, scales with business |
| API Style | Status codes, manual management | Exceptions, IDisposable, modern .NET |
| Learning Curve | Steep (imaging SDK concepts) | Simple (HTML/CSS familiar) |
| HTML Rendering | Basic, internal engine | Latest Chromium with CSS3/JS |
| Page Indexing | 1-indexed | 0-indexed (standard .NET) |
| Thread Safety | Manual synchronization required | Thread-safe by design |
| Namespace | Version-specific (`GdPicture14`) | Stable (`IronPdf`) |

---

## Migration Complexity Assessment

### Estimated Effort by Feature

| Feature | Migration Complexity | Notes |
|---------|---------------------|-------|
| HTML to PDF | Low | Direct method mapping |
| URL to PDF | Low | Direct method mapping |
| Merge PDFs | Low | Similar API |
| Split PDFs | Low | Similar API |
| Watermarks | Low | Different approach (HTML-based) |
| Text Extraction | Low | Property vs method |
| Password Protection | Medium | Different parameter structure |
| Form Fields | Medium | API differences |
| Digital Signatures | Medium-High | Different certificate handling |
| OCR | High | IronOCR is separate product |
| Barcode Recognition | N/A | Not supported in IronPDF |
| Image Processing | N/A | Not supported in IronPDF |

### Migration Decision Matrix

| Your Situation | Recommendation |
|----------------|----------------|
| PDF-only operations | Migrate—significant simplification and cost savings |
| Heavy OCR usage | Consider IronOCR as companion product |
| Barcode/scanning | Keep GdPicture for those features, use IronPDF for PDF |
| Full document imaging | Evaluate if you actually use all features |

---

## Before You Start

### Prerequisites

1. **.NET Version**: IronPDF supports .NET Framework 4.6.2+ and .NET Core 2.0+ / .NET 5+
2. **License Key**: Obtain your IronPDF license key from [ironpdf.com](https://ironpdf.com)
3. **Backup**: Create a branch for migration work

### Identify All GdPicture Usage

```bash
# Find all GdPicture references in your codebase
grep -r "GdPicture14\|GdPicturePDF\|GdPictureDocumentConverter\|GdPictureStatus\|LicenseManager\.RegisterKEY" --include="*.cs" .

# Find all GdPicture package references
grep -r "GdPicture" --include="*.csproj" .
```

### NuGet Package Changes

```bash
# Remove GdPicture packages
dotnet remove package GdPicture.NET.14
dotnet remove package GdPicture.NET.14.API
dotnet remove package GdPicture
dotnet remove package GdPicture.API

# Install IronPDF
dotnet add package IronPdf
```

### License Key Setup

**GdPicture.NET:**
```csharp
// Must be called before any GdPicture operations
LicenseManager.RegisterKEY("YOUR-GDPICTURE-LICENSE-KEY");
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

**Before (GdPicture.NET):**
```csharp
using GdPicture14;

// License registration
LicenseManager.RegisterKEY("LICENSE-KEY");

// HTML to PDF
using (GdPictureDocumentConverter converter = new GdPictureDocumentConverter())
{
    GdPictureStatus status = converter.LoadFromHTMLString("<h1>Hello World</h1>");

    if (status == GdPictureStatus.OK)
    {
        status = converter.SaveAsPDF("output.pdf");

        if (status != GdPictureStatus.OK)
        {
            Console.WriteLine($"Error: {status}");
        }
    }
    else
    {
        Console.WriteLine($"Load error: {status}");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

// License (set once at startup)
IronPdf.License.LicenseKey = "LICENSE-KEY";

// HTML to PDF - clean, exception-based
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

**Key Differences:**
- No status checking—exceptions on errors
- No explicit disposal required for renderer
- Modern fluent API
- Chromium-based rendering for better HTML/CSS support

---

## Complete API Reference

### Namespace Mapping

| GdPicture.NET | IronPDF |
|---------------|---------|
| `GdPicture14` | `IronPdf` |
| `GdPicture14.PDF` | `IronPdf` |
| `GdPicture14.Imaging` | N/A (not needed) |

### Core Class Mapping

| GdPicture.NET | IronPDF | Description |
|---------------|---------|-------------|
| `GdPicturePDF` | `PdfDocument` | Main PDF document class |
| `GdPictureDocumentConverter` | `ChromePdfRenderer` | HTML/URL to PDF conversion |
| `GdPictureImaging` | N/A | Image processing (not in IronPDF) |
| `GdPictureOCR` | `IronOcr.IronTesseract` | OCR (separate product) |
| `LicenseManager` | `IronPdf.License` | License management |
| `GdPictureStatus` | Exceptions | Error handling |

### Document Loading Methods

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf.LoadFromFile(path, loadInMemory)` | `PdfDocument.FromFile(path)` | Load from file |
| `pdf.LoadFromFile(path, password, loadInMemory)` | `PdfDocument.FromFile(path, password)` | Password-protected |
| `pdf.LoadFromStream(stream)` | `PdfDocument.FromStream(stream)` | Load from stream |
| `pdf.LoadFromStream(stream, password)` | `PdfDocument.FromStream(stream, password)` | With password |
| `converter.LoadFromHTMLString(html)` | `renderer.RenderHtmlAsPdf(html)` | HTML string |
| `converter.LoadFromHTMLFile(path)` | `renderer.RenderHtmlFileAsPdf(path)` | HTML file |
| `converter.LoadFromURL(url)` | `renderer.RenderUrlAsPdf(url)` | URL |

### Document Saving Methods

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf.SaveToFile(path)` | `pdf.SaveAs(path)` | Save to file |
| `pdf.SaveToStream(stream)` | `pdf.Stream` or `pdf.BinaryData` | Get as stream/bytes |
| `converter.SaveAsPDF(path)` | `pdf.SaveAs(path)` | Save converted PDF |
| `converter.SaveAsPDF(stream, conformance)` | `pdf.SaveAs(path)` | With conformance |

### Page Operations

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf.GetPageCount()` | `pdf.PageCount` | Get page count |
| `pdf.SelectPage(pageNo)` | `pdf.Pages[index]` | Select page (1-indexed vs 0-indexed) |
| `pdf.NewPage(width, height)` | `pdf.Pages.Add()` | Add new page |
| `pdf.InsertPage(position)` | `pdf.Pages.Insert(index, page)` | Insert page |
| `pdf.RemovePage(pageNo)` | `pdf.Pages.RemoveAt(index)` | Remove page |
| `pdf.GetPageWidth()` | `pdf.Pages[i].Width` | Page width |
| `pdf.GetPageHeight()` | `pdf.Pages[i].Height` | Page height |
| `pdf.RotatePage(angle)` | `pdf.Pages[i].Rotation` | Rotate page |

### Merge and Split Operations

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf1.MergePages(pdf2)` | `PdfDocument.Merge(pdf1, pdf2)` | Merge PDFs |
| `pdf1.MergePDF(pdf2)` | `PdfDocument.Merge(pdf1, pdf2)` | Same as above |
| `pdf.ClonePage(pageNo)` | `pdf.CopyPage(index)` | Copy page |
| `pdf.ExtractPages(start, end)` | `pdf.CopyPages(indices)` | Extract pages |

### Text Operations

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf.GetPageText(pageNo)` | `pdf.ExtractTextFromPage(index)` | Extract text from page |
| `pdf.GetText()` | `pdf.ExtractAllText()` | Extract all text |
| `pdf.DrawText(fontRes, x, y, text)` | Use HTML stamping | Add text |
| `pdf.SetTextSize(size)` | CSS styling | Set text size |
| `pdf.SetTextColor(color)` | CSS styling | Set text color |
| `pdf.SetFillColor(r, g, b)` | CSS styling | Set fill color |
| `pdf.AddStandardFont(fontType)` | CSS font-family | Add font |

### Watermark Operations

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf.DrawText(...)` loop | `pdf.ApplyWatermark(html)` | Text watermark |
| `pdf.AddImageFromFile(...)` | `pdf.ApplyStamp(stamper)` | Image watermark |

### Security and Encryption

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf.SaveToFile(path, encryption, userPwd, ownerPwd, ...)` | `pdf.SecuritySettings` | Set encryption |
| `pdf.SetPassword(password)` | `pdf.Password = "..."` | Open password |
| `pdf.IsEncrypted()` | `pdf.SecuritySettings.OwnerPassword != null` | Check encryption |
| `pdf.GetEncryptionScheme()` | N/A (automatic) | Get encryption type |
| `PdfEncryption.PdfEncryption128BitRC4` | 128-bit AES default | Encryption algorithm |
| `PdfEncryption.PdfEncryption256BitAES` | `pdf.SecuritySettings` | AES 256 |

### Metadata Operations

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf.GetTitle()` | `pdf.MetaData.Title` | Get title |
| `pdf.SetTitle(title)` | `pdf.MetaData.Title = title` | Set title |
| `pdf.GetAuthor()` | `pdf.MetaData.Author` | Get author |
| `pdf.SetAuthor(author)` | `pdf.MetaData.Author = author` | Set author |
| `pdf.GetSubject()` | `pdf.MetaData.Subject` | Get subject |
| `pdf.SetSubject(subject)` | `pdf.MetaData.Subject = subject` | Set subject |
| `pdf.GetKeywords()` | `pdf.MetaData.Keywords` | Get keywords |
| `pdf.SetKeywords(keywords)` | `pdf.MetaData.Keywords = keywords` | Set keywords |
| `pdf.GetCreator()` | `pdf.MetaData.Creator` | Get creator |
| `pdf.GetProducer()` | `pdf.MetaData.Producer` | Get producer |

### Image Operations

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf.AddImageFromFile(path)` | Include in HTML | Add image |
| `pdf.RenderPageToGdPictureImage(pageNo)` | `pdf.RasterizeToImageFiles()` | Render to image |
| `pdf.RenderPageToFile(path, format, dpi)` | `pdf.RasterizeToImageFiles(path, format, dpi)` | Save as image |
| `pdf.GetImageCount()` | N/A | Count images |
| `pdf.ExtractImageToFile(...)` | `pdf.ExtractAllRawImagesFromPdf()` | Extract images |

### Form Field Operations

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf.GetFormFieldsCount()` | `pdf.Form.Fields.Count` | Count fields |
| `pdf.GetFormFieldTitle(idx)` | `pdf.Form.Fields[i].Name` | Field name |
| `pdf.GetFormFieldValue(idx)` | `pdf.Form.Fields[i].Value` | Get value |
| `pdf.SetFormFieldValue(idx, value)` | `pdf.Form.Fields[i].Value = value` | Set value |
| `pdf.FlattenFormFields()` | `pdf.Form.Flatten()` | Flatten forms |

### Annotation Operations

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf.GetAnnotationCount(pageNo)` | `pdf.Pages[i].Annotations.Count` | Count annotations |
| `pdf.AddTextAnnot(...)` | `pdf.Pages[i].Annotations.Add(...)` | Add annotation |
| `pdf.FlattenAnnotations()` | `pdf.Annotations.Flatten()` | Flatten annotations |

### PDF/A Conversion

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `pdf.ConvertToPDFA(conformance)` | Rendering options | PDF/A conversion |
| `pdf.GetPDFAConformance()` | N/A | Check conformance |
| `PdfConformance.PDF_A_1b` | `PdfACompliance.PDFA_1b` | PDF/A-1b |
| `PdfConformance.PDF_A_2b` | `PdfACompliance.PDFA_2b` | PDF/A-2b |

---

## Code Migration Examples

### Example 1: HTML to PDF Conversion

**Before (GdPicture.NET):**
```csharp
using GdPicture14;
using System;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        using (GdPictureDocumentConverter converter = new GdPictureDocumentConverter())
        {
            string html = @"
                <html>
                <head>
                    <style>
                        body { font-family: Arial; }
                        h1 { color: #333; }
                    </style>
                </head>
                <body>
                    <h1>Sales Report</h1>
                    <p>Generated on: " + DateTime.Now + @"</p>
                </body>
                </html>";

            GdPictureStatus status = converter.LoadFromHTMLString(html);

            if (status == GdPictureStatus.OK)
            {
                // Set page size
                converter.HtmlSetPageWidth(8.5f);
                converter.HtmlSetPageHeight(11.0f);
                converter.HtmlSetMargins(0.5f, 0.5f, 0.5f, 0.5f);

                status = converter.SaveAsPDF("report.pdf");

                if (status != GdPictureStatus.OK)
                {
                    Console.WriteLine($"Save error: {status}");
                }
            }
            else
            {
                Console.WriteLine($"Load error: {status}");
            }
        }
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
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Configure page settings
        renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
        renderer.RenderingOptions.MarginTop = 12.7;      // 0.5 inches in mm
        renderer.RenderingOptions.MarginBottom = 12.7;
        renderer.RenderingOptions.MarginLeft = 12.7;
        renderer.RenderingOptions.MarginRight = 12.7;

        string html = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial; }}
                    h1 {{ color: #333; }}
                </style>
            </head>
            <body>
                <h1>Sales Report</h1>
                <p>Generated on: {DateTime.Now}</p>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report.pdf");
    }
}
```

### Example 2: URL to PDF with Custom Options

**Before (GdPicture.NET):**
```csharp
using GdPicture14;
using System;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        using (GdPictureDocumentConverter converter = new GdPictureDocumentConverter())
        {
            // Set rendering options
            converter.HtmlSetPageOrientation(PdfPageOrientation.PdfPageOrientationLandscape);
            converter.HtmlEnableJavascript(true);
            converter.HtmlSetPageWidth(11.0f);  // Landscape
            converter.HtmlSetPageHeight(8.5f);

            GdPictureStatus status = converter.LoadFromURL("https://www.example.com");

            if (status == GdPictureStatus.OK)
            {
                status = converter.SaveAsPDF("webpage.pdf");

                if (status != GdPictureStatus.OK)
                {
                    Console.WriteLine($"Error: {status}");
                }
            }
            else
            {
                Console.WriteLine($"Load error: {status}");
            }
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
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Configure rendering
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.RenderDelay = 500;  // Wait for JS

        var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

### Example 3: Merge Multiple PDFs

**Before (GdPicture.NET):**
```csharp
using GdPicture14;
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        List<string> files = new List<string>
        {
            "chapter1.pdf",
            "chapter2.pdf",
            "chapter3.pdf",
            "appendix.pdf"
        };

        using (GdPicturePDF masterPdf = new GdPicturePDF())
        {
            // Load first file
            GdPictureStatus status = masterPdf.LoadFromFile(files[0], false);

            if (status != GdPictureStatus.OK)
            {
                Console.WriteLine($"Failed to load {files[0]}: {status}");
                return;
            }

            // Merge remaining files
            for (int i = 1; i < files.Count; i++)
            {
                using (GdPicturePDF pdf = new GdPicturePDF())
                {
                    status = pdf.LoadFromFile(files[i], false);

                    if (status == GdPictureStatus.OK)
                    {
                        status = masterPdf.MergePages(pdf);

                        if (status != GdPictureStatus.OK)
                        {
                            Console.WriteLine($"Failed to merge {files[i]}: {status}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to load {files[i]}: {status}");
                    }
                }
            }

            // Save merged document
            status = masterPdf.SaveToFile("complete-book.pdf");

            if (status != GdPictureStatus.OK)
            {
                Console.WriteLine($"Failed to save: {status}");
            }
        }
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
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var files = new List<string>
        {
            "chapter1.pdf",
            "chapter2.pdf",
            "chapter3.pdf",
            "appendix.pdf"
        };

        // Load all PDFs
        var pdfs = files.Select(f => PdfDocument.FromFile(f)).ToList();

        // Merge all at once
        var merged = PdfDocument.Merge(pdfs);
        merged.SaveAs("complete-book.pdf");

        // Dispose all documents
        foreach (var pdf in pdfs)
        {
            pdf.Dispose();
        }
    }
}
```

### Example 4: Add Watermark to All Pages

**Before (GdPicture.NET):**
```csharp
using GdPicture14;
using System.Drawing;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        using (GdPicturePDF pdf = new GdPicturePDF())
        {
            GdPictureStatus status = pdf.LoadFromFile("contract.pdf", false);

            if (status != GdPictureStatus.OK)
            {
                return;
            }

            // Add standard font
            string fontRes = pdf.AddStandardFont(PdfStandardFont.PdfStandardFontHelveticaBold);

            int pageCount = pdf.GetPageCount();

            for (int i = 1; i <= pageCount; i++)  // 1-indexed!
            {
                pdf.SelectPage(i);

                // Get page dimensions
                float width = pdf.GetPageWidth();
                float height = pdf.GetPageHeight();

                // Set text properties
                pdf.SetOrigin(PdfOrigin.PdfOriginTopLeft);
                pdf.SetMeasurementUnit(PdfMeasurementUnit.PdfMeasurementUnitPoint);
                pdf.SetFillColor(255, 0, 0);  // Red
                pdf.SetTextSize(72);

                // Calculate center position
                float x = width / 2 - 150;
                float y = height / 2;

                // Save graphics state for rotation
                pdf.SaveGraphicsState();
                pdf.RotateCoordinates(x, y, -45);  // Diagonal

                // Draw watermark text
                status = pdf.DrawText(fontRes, x, y, "CONFIDENTIAL");

                pdf.RestoreGraphicsState();
            }

            pdf.SaveToFile("contract-watermarked.pdf");
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
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var pdf = PdfDocument.FromFile("contract.pdf");

        // Apply watermark with HTML - much simpler!
        pdf.ApplyWatermark(
            "<div style='color:red; font-size:72px; font-weight:bold; " +
            "transform:rotate(-45deg); opacity:0.3;'>CONFIDENTIAL</div>",
            opacity: 30,
            VerticalAlignment.Middle,
            HorizontalAlignment.Center
        );

        pdf.SaveAs("contract-watermarked.pdf");
    }
}
```

### Example 5: Password Protection and Security

**Before (GdPicture.NET):**
```csharp
using GdPicture14;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        using (GdPicturePDF pdf = new GdPicturePDF())
        {
            GdPictureStatus status = pdf.LoadFromFile("document.pdf", false);

            if (status != GdPictureStatus.OK) return;

            // Save with encryption
            // Parameters: path, encryption, userPwd, ownerPwd,
            //            canPrint, canCopy, canModify, canAddNotes,
            //            canFillForms, canExtract, canAssemble, canPrintHQ
            status = pdf.SaveToFile(
                "protected.pdf",
                PdfEncryption.PdfEncryption256BitAES,
                "user123",      // User password (to open)
                "owner456",     // Owner password (full access)
                true,           // Can print
                false,          // Cannot copy
                false,          // Cannot modify
                false,          // Cannot add notes
                true,           // Can fill forms
                false,          // Cannot extract
                false,          // Cannot assemble
                true            // Can print high quality
            );

            if (status != GdPictureStatus.OK)
            {
                Console.WriteLine($"Error: {status}");
            }
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
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");

        // Configure security settings
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.UserPassword = "user123";

        // Set permissions
        pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
        pdf.SecuritySettings.AllowUserAnnotations = false;
        pdf.SecuritySettings.AllowUserFormData = true;

        pdf.SaveAs("protected.pdf");
    }
}
```

### Example 6: Extract Text from PDF

**Before (GdPicture.NET):**
```csharp
using GdPicture14;
using System;
using System.Text;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        using (GdPicturePDF pdf = new GdPicturePDF())
        {
            GdPictureStatus status = pdf.LoadFromFile("document.pdf", false);

            if (status != GdPictureStatus.OK)
            {
                Console.WriteLine($"Load error: {status}");
                return;
            }

            StringBuilder allText = new StringBuilder();
            int pageCount = pdf.GetPageCount();

            for (int i = 1; i <= pageCount; i++)  // 1-indexed
            {
                pdf.SelectPage(i);
                string pageText = pdf.GetPageText();

                allText.AppendLine($"--- Page {i} ---");
                allText.AppendLine(pageText);
                allText.AppendLine();
            }

            Console.WriteLine(allText.ToString());
        }
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
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");

        // Extract all text at once
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

### Example 7: Split PDF into Single Pages

**Before (GdPicture.NET):**
```csharp
using GdPicture14;
using System;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        using (GdPicturePDF sourcePdf = new GdPicturePDF())
        {
            GdPictureStatus status = sourcePdf.LoadFromFile("multipage.pdf", false);

            if (status != GdPictureStatus.OK) return;

            int pageCount = sourcePdf.GetPageCount();

            for (int i = 1; i <= pageCount; i++)  // 1-indexed
            {
                using (GdPicturePDF newPdf = new GdPicturePDF())
                {
                    // Create new document
                    newPdf.NewPDF();

                    // Clone page from source
                    status = newPdf.ClonePage(sourcePdf, i);

                    if (status == GdPictureStatus.OK)
                    {
                        newPdf.SaveToFile($"page_{i}.pdf");
                    }
                }
            }
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
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var pdf = PdfDocument.FromFile("multipage.pdf");

        // Split into single pages (0-indexed)
        for (int i = 0; i < pdf.PageCount; i++)
        {
            var singlePage = pdf.CopyPage(i);
            singlePage.SaveAs($"page_{i + 1}.pdf");
        }

        // Or use CopyPages for ranges
        var firstThree = pdf.CopyPages(0, 1, 2);
        firstThree.SaveAs("first-three-pages.pdf");
    }
}
```

### Example 8: PDF to Image Conversion

**Before (GdPicture.NET):**
```csharp
using GdPicture14;
using System;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        using (GdPicturePDF pdf = new GdPicturePDF())
        using (GdPictureImaging imaging = new GdPictureImaging())
        {
            GdPictureStatus status = pdf.LoadFromFile("document.pdf", false);

            if (status != GdPictureStatus.OK) return;

            int pageCount = pdf.GetPageCount();

            for (int i = 1; i <= pageCount; i++)
            {
                pdf.SelectPage(i);

                // Render page to image
                int imageId = pdf.RenderPageToGdPictureImage(200, true);  // 200 DPI

                if (imageId != 0)
                {
                    // Save as PNG
                    status = imaging.SaveAsPNG(imageId, $"page_{i}.png");
                    imaging.ReleaseGdPictureImage(imageId);

                    if (status != GdPictureStatus.OK)
                    {
                        Console.WriteLine($"Save error for page {i}: {status}");
                    }
                }
            }
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
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");

        // Render all pages to images at once
        pdf.RasterizeToImageFiles("page_*.png", DPI: 200);

        // Or get as byte arrays
        var images = pdf.ToBitmap(200);
        for (int i = 0; i < images.Length; i++)
        {
            images[i].Save($"page_{i + 1}.png");
        }
    }
}
```

### Example 9: Headers and Footers

**Before (GdPicture.NET):**
```csharp
using GdPicture14;
using System;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        using (GdPicturePDF pdf = new GdPicturePDF())
        {
            GdPictureStatus status = pdf.LoadFromFile("document.pdf", false);
            if (status != GdPictureStatus.OK) return;

            string fontRes = pdf.AddStandardFont(PdfStandardFont.PdfStandardFontHelvetica);
            int pageCount = pdf.GetPageCount();

            for (int i = 1; i <= pageCount; i++)
            {
                pdf.SelectPage(i);
                float width = pdf.GetPageWidth();
                float height = pdf.GetPageHeight();

                pdf.SetOrigin(PdfOrigin.PdfOriginTopLeft);
                pdf.SetMeasurementUnit(PdfMeasurementUnit.PdfMeasurementUnitPoint);
                pdf.SetFillColor(0, 0, 0);
                pdf.SetTextSize(10);

                // Header
                pdf.DrawText(fontRes, 50, 30, "Company Name - Confidential");

                // Footer with page number
                string footer = $"Page {i} of {pageCount}";
                pdf.DrawText(fontRes, width - 100, height - 20, footer);
            }

            pdf.SaveToFile("with-headers.pdf");
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
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Configure headers and footers with HTML
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:center; font-size:10px;'>" +
                          "Company Name - Confidential</div>",
            DrawDividerLine = true
        };

        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:right; font-size:10px;'>" +
                          "Page {page} of {total-pages}</div>",
            DrawDividerLine = true
        };

        var pdf = renderer.RenderHtmlAsPdf("<h1>Document Content</h1><p>...</p>");
        pdf.SaveAs("with-headers.pdf");

        // Or add to existing PDF
        var existingPdf = PdfDocument.FromFile("document.pdf");
        existingPdf.AddHtmlHeaders(
            new HtmlHeaderFooter()
            {
                HtmlFragment = "<div>Header Text</div>"
            }
        );
        existingPdf.AddHtmlFooters(
            new HtmlHeaderFooter()
            {
                HtmlFragment = "<div>Page {page} of {total-pages}</div>"
            }
        );
        existingPdf.SaveAs("existing-with-headers.pdf");
    }
}
```

### Example 10: Fill PDF Forms

**Before (GdPicture.NET):**
```csharp
using GdPicture14;
using System;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        using (GdPicturePDF pdf = new GdPicturePDF())
        {
            GdPictureStatus status = pdf.LoadFromFile("form.pdf", false);
            if (status != GdPictureStatus.OK) return;

            int fieldCount = pdf.GetFormFieldsCount();

            for (int i = 0; i < fieldCount; i++)
            {
                string fieldTitle = pdf.GetFormFieldTitle(i);

                // Set values based on field name
                switch (fieldTitle)
                {
                    case "FirstName":
                        pdf.SetFormFieldValue(i, "John");
                        break;
                    case "LastName":
                        pdf.SetFormFieldValue(i, "Doe");
                        break;
                    case "Email":
                        pdf.SetFormFieldValue(i, "john.doe@example.com");
                        break;
                    case "AgreeToTerms":
                        pdf.SetFormFieldValue(i, "Yes");
                        break;
                }
            }

            // Optionally flatten the form
            pdf.FlattenFormFields();

            pdf.SaveToFile("filled-form.pdf");
        }
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
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var pdf = PdfDocument.FromFile("form.pdf");

        // Fill form fields by name
        pdf.Form.Fields["FirstName"].Value = "John";
        pdf.Form.Fields["LastName"].Value = "Doe";
        pdf.Form.Fields["Email"].Value = "john.doe@example.com";
        pdf.Form.Fields["AgreeToTerms"].Value = "Yes";

        // Or iterate through all fields
        foreach (var field in pdf.Form.Fields)
        {
            Console.WriteLine($"Field: {field.Name}, Type: {field.Type}");
        }

        // Optionally flatten
        pdf.Form.Flatten();

        pdf.SaveAs("filled-form.pdf");
    }
}
```

---

## Advanced Scenarios

### Thread-Safe PDF Generation

**GdPicture.NET Issue:**
```csharp
// GdPicture requires careful instance management
// Generally NOT thread-safe without synchronization
```

**IronPDF Solution:**
```csharp
using IronPdf;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            int reportId = i;
            tasks.Add(Task.Run(() =>
            {
                // Each task gets its own renderer - thread-safe
                var renderer = new ChromePdfRenderer();
                var pdf = renderer.RenderHtmlAsPdf($"<h1>Report {reportId}</h1>");
                pdf.SaveAs($"report_{reportId}.pdf");
            }));
        }

        await Task.WhenAll(tasks);
    }
}
```

### Batch Processing with Progress Reporting

**Before (GdPicture.NET):**
```csharp
using GdPicture14;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        string[] htmlFiles = Directory.GetFiles("input", "*.html");
        int processed = 0;

        foreach (string file in htmlFiles)
        {
            using (GdPictureDocumentConverter converter = new GdPictureDocumentConverter())
            {
                GdPictureStatus status = converter.LoadFromHTMLFile(file);

                if (status == GdPictureStatus.OK)
                {
                    string outputPath = Path.Combine("output",
                        Path.GetFileNameWithoutExtension(file) + ".pdf");

                    status = converter.SaveAsPDF(outputPath);

                    if (status == GdPictureStatus.OK)
                    {
                        processed++;
                    }
                }
            }

            Console.WriteLine($"Progress: {processed}/{htmlFiles.Length}");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        var htmlFiles = Directory.GetFiles("input", "*.html");
        var progress = new Progress<int>(p =>
            Console.WriteLine($"Progress: {p}/{htmlFiles.Length}"));

        int processed = 0;
        var renderer = new ChromePdfRenderer();

        await Task.Run(() =>
        {
            foreach (var file in htmlFiles)
            {
                string outputPath = Path.Combine("output",
                    Path.GetFileNameWithoutExtension(file) + ".pdf");

                var pdf = renderer.RenderHtmlFileAsPdf(file);
                pdf.SaveAs(outputPath);

                ((IProgress<int>)progress).Report(++processed);
            }
        });

        Console.WriteLine($"Completed: {processed} files");
    }
}
```

### Error Handling Pattern Migration

**GdPicture.NET (Status Code Pattern):**
```csharp
using GdPicture14;
using System;

class Program
{
    static void Main()
    {
        LicenseManager.RegisterKEY("LICENSE-KEY");

        using (GdPicturePDF pdf = new GdPicturePDF())
        {
            GdPictureStatus status = pdf.LoadFromFile("document.pdf", false);

            if (status != GdPictureStatus.OK)
            {
                switch (status)
                {
                    case GdPictureStatus.CannotOpenFile:
                        Console.WriteLine("File not found or access denied");
                        break;
                    case GdPictureStatus.InvalidPassword:
                        Console.WriteLine("Document is encrypted, password required");
                        break;
                    case GdPictureStatus.InvalidPDFFile:
                        Console.WriteLine("File is not a valid PDF");
                        break;
                    default:
                        Console.WriteLine($"Unknown error: {status}");
                        break;
                }
                return;
            }

            // Process document...
        }
    }
}
```

**IronPDF (Exception Pattern):**
```csharp
using IronPdf;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "LICENSE-KEY";

        try
        {
            var pdf = PdfDocument.FromFile("document.pdf");
            // Process document...
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("File not found");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Access denied");
        }
        catch (IronPdf.Exceptions.PdfException ex) when (ex.Message.Contains("password"))
        {
            Console.WriteLine("Document is encrypted, password required");
        }
        catch (IronPdf.Exceptions.PdfException ex)
        {
            Console.WriteLine($"PDF error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
```

---

## Performance Considerations

### Memory Management

**GdPicture.NET:**
```csharp
// Must explicitly release resources
using (GdPicturePDF pdf = new GdPicturePDF())
{
    // ...
}  // Dispose called

// Or manually
GdPicturePDF pdf = new GdPicturePDF();
// ...
pdf.Dispose();  // or pdf.CloseDocument();
```

**IronPDF:**
```csharp
// Standard .NET patterns work correctly
using var pdf = PdfDocument.FromFile("document.pdf");
// Automatically disposed at end of scope

// For long-lived applications
var pdf = PdfDocument.FromFile("document.pdf");
// ... process ...
pdf.Dispose();  // Or use 'using' statement
```

### Large Document Handling

**GdPicture.NET:**
```csharp
// LoadFromFile with false = don't load into memory
pdf.LoadFromFile("large.pdf", false);
```

**IronPDF:**
```csharp
// Stream-based loading for large files
using var stream = File.OpenRead("large.pdf");
var pdf = PdfDocument.FromStream(stream);
```

### Rendering Performance Tips

```csharp
// For batch operations, reuse the renderer
var renderer = new ChromePdfRenderer();

// Disable features you don't need
renderer.RenderingOptions.EnableJavaScript = false;
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
renderer.RenderingOptions.Timeout = 30;  // Fail fast

foreach (var html in htmlDocuments)
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{count++}.pdf");
}
```

---

## Troubleshooting

### Issue 1: Page Indexing Errors

**Problem:** Off-by-one errors when accessing pages.

**GdPicture.NET:** Pages are 1-indexed
**IronPDF:** Pages are 0-indexed

```csharp
// GdPicture: for (int i = 1; i <= pageCount; i++)
// IronPDF:   for (int i = 0; i < pdf.PageCount; i++)

// Converting GdPicture page number to IronPDF index
int gdPicturePage = 5;
int ironPdfIndex = gdPicturePage - 1;  // = 4
```

### Issue 2: Status Code to Exception Mapping

**Problem:** Need to handle errors that were previously status codes.

```csharp
// GdPicture status codes map roughly to these exceptions:
// GdPictureStatus.CannotOpenFile -> FileNotFoundException
// GdPictureStatus.InvalidPassword -> IronPdf.Exceptions.PdfException
// GdPictureStatus.InvalidPDFFile -> IronPdf.Exceptions.PdfException
// GdPictureStatus.OutOfMemory -> OutOfMemoryException
// GdPictureStatus.GenericError -> Exception

// Wrap in try-catch and handle appropriately
```

### Issue 3: Coordinate System Differences

**Problem:** Text positioning doesn't match.

**GdPicture.NET:** Uses SetOrigin and SetMeasurementUnit
**IronPDF:** Uses HTML/CSS positioning

```csharp
// GdPicture: Absolute point positioning
pdf.SetOrigin(PdfOrigin.PdfOriginTopLeft);
pdf.SetMeasurementUnit(PdfMeasurementUnit.PdfMeasurementUnitPoint);
pdf.DrawText(fontRes, 72, 72, "Text");  // 1 inch from edges

// IronPDF: CSS positioning
var stamper = new HtmlStamper()
{
    Html = "<div style='position:absolute; top:1in; left:1in;'>Text</div>"
};
pdf.ApplyStamp(stamper);
```

### Issue 4: Font Handling

**Problem:** Custom fonts not rendering correctly.

```csharp
// GdPicture: Explicit font resource management
string fontRes = pdf.AddStandardFont(PdfStandardFont.PdfStandardFontHelvetica);
string customFont = pdf.AddTrueTypeFontU("path/to/font.ttf", true, true);
pdf.DrawText(fontRes, x, y, "Text");

// IronPDF: CSS font-family
var html = @"
    <style>
        @font-face {
            font-family: 'CustomFont';
            src: url('path/to/font.ttf');
        }
        body { font-family: 'CustomFont', Arial, sans-serif; }
    </style>
    <body>Text</body>";
```

### Issue 5: Unit Conversion

**GdPicture.NET uses points by default. IronPDF uses millimeters for margins.**

```csharp
// Conversion formulas:
// points to mm: points * 0.352778
// mm to points: mm * 2.83465
// inches to mm: inches * 25.4
// mm to inches: mm / 25.4

// GdPicture: 72 points margin (1 inch)
// IronPDF equivalent:
renderer.RenderingOptions.MarginTop = 25.4;  // 1 inch in mm
```

### Issue 6: Color Value Differences

**Problem:** Colors don't match between libraries.

```csharp
// GdPicture: RGB as separate values
pdf.SetFillColor(255, 0, 0);  // Red

// IronPDF: CSS colors
var html = "<div style='color: rgb(255, 0, 0);'>Red Text</div>";
// Or hex: style='color: #FF0000;'
// Or named: style='color: red;'
```

### Issue 7: LicenseManager Migration

**Problem:** License not activating.

```csharp
// GdPicture: Must call before any operations
LicenseManager.RegisterKEY("KEY");

// IronPDF: Set property once at startup
// In Program.cs or Application startup:
IronPdf.License.LicenseKey = "KEY";

// Or in appsettings.json:
// { "IronPdf.License.LicenseKey": "KEY" }

// Check license status
Console.WriteLine($"Licensed: {IronPdf.License.IsValidLicense(IronPdf.License.LicenseKey)}");
```

### Issue 8: Missing OCR/Barcode Features

**Problem:** Need OCR or barcode features that were in GdPicture.

**Solution:** Use companion products:
```bash
# For OCR
dotnet add package IronOcr

# For Barcode
dotnet add package IronBarcode
```

```csharp
// IronOCR example
using IronOcr;

var ocr = new IronTesseract();
using var input = new OcrInput("scanned.pdf");
var result = ocr.Read(input);
Console.WriteLine(result.Text);

// IronBarcode example
using IronBarCode;

var results = BarcodeReader.Read("document.pdf");
foreach (var barcode in results)
{
    Console.WriteLine(barcode.Value);
}
```

---

## Migration Checklist

```markdown
## Migration Checklist

### Pre-Migration

- [ ] **Inventory all library usages in codebase**
  ```bash
  grep -r "using GdPicture14" --include="*.cs" .
  grep -r "LicenseManager.RegisterKEY\|GdPictureStatus" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  ```csharp
  // Find patterns like:
  var status = LicenseManager.RegisterKEY("YOUR-LICENSE-KEY");
  ```
  **Why:** These configurations map to IronPDF's license initialization. Document them now to ensure consistent setup after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package GdPicture.NET
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Add license key initialization**
  ```csharp
  // Before (GdPicture.NET)
  var status = LicenseManager.RegisterKEY("YOUR-LICENSE-KEY");

  // After (IronPDF)
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace PDF generation calls**
  ```csharp
  // Before (GdPicture.NET)
  var pdf = new GdPicturePDF();
  pdf.NewPDF();
  pdf.AddImageFromFile("image.jpg", 0, 0, 100, 100);
  pdf.SaveToFile("output.pdf");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf("<img src='image.jpg' style='width:100px;height:100px;'/>");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering for accurate HTML/CSS support.

- [ ] **Convert URL rendering**
  ```csharp
  // Before (GdPicture.NET)
  var pdf = new GdPicturePDF();
  pdf.LoadFromURL("https://example.com");
  pdf.SaveToFile("output.pdf");

  // After (IronPDF)
  var pdf = renderer.RenderUrlAsPdf("https://example.com");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Direct URL rendering with full JavaScript support.

- [ ] **Update page settings**
  ```csharp
  // Before (GdPicture.NET)
  pdf.SetPageDimensions(595, 842); // A4 size
  pdf.SetPageOrientation(GdPicturePDF.Orientation.Landscape);
  pdf.SetMargins(20, 20, 20, 20);

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  renderer.RenderingOptions.MarginLeft = 20;
  renderer.RenderingOptions.MarginRight = 20;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Convert header/footer configuration**
  ```csharp
  // Before (GdPicture.NET)
  pdf.SetHeaderText("Page [page] of [total]");
  pdf.SetFooterText("Document Title");

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>{html-title}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders: {page}, {total-pages}, {date}, {time}, {html-title}.

- [ ] **Enable JavaScript if needed**
  ```csharp
  // Before (GdPicture.NET)
  pdf.EnableJavaScript(true);
  pdf.SetJavaScriptDelay(2000);

  // After (IronPDF)
  renderer.RenderingOptions.EnableJavaScript = true;
  renderer.RenderingOptions.WaitFor.JavaScript(2000);
  ```
  **Why:** IronPDF's Chromium engine provides reliable JavaScript execution with configurable wait times.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Test JavaScript-heavy pages**
  **Why:** Dynamic content should now render more reliably with modern Chromium.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  // Merge PDFs
  var merged = PdfDocument.Merge(pdf1, pdf2);

  // Password protection
  pdf.SecuritySettings.UserPassword = "secret";

  // Watermarks
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");

  // Digital signatures
  var signature = new PdfSignature("cert.pfx", "password");
  pdf.Sign(signature);
  ```
  **Why:** IronPDF provides many features that may not have been available in the old library.
```
## Migration Checklist

### Pre-Migration

- [ ] **Inventory all GdPicture.NET usage in codebase**
  ```bash
  grep -r "using GdPicture14" --include="*.cs" .
  grep -r "GdPicturePDF\|GdPictureDocumentConverter" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Identify which GdPicture features are actually used**
  **Why:** Determine if any features beyond PDF generation are in use, which may require additional libraries.

- [ ] **Determine if OCR/barcode features are needed (consider IronOCR/IronBarcode)**
  **Why:** IronPDF focuses on PDF rendering. For OCR or barcode, IronOCR/IronBarcode may be required.

- [ ] **Review current GdPicture licensing and compare to IronPDF pricing**
  **Why:** Ensure cost-effectiveness and budget alignment with IronPDF's pricing model.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Create migration branch in version control**
  **Why:** Isolate changes and facilitate rollback if needed.

- [ ] **Set up test environment**
  **Why:** Ensure a controlled environment to verify migration success without affecting production.

### Code Migration

- [ ] **Remove GdPicture NuGet packages**
  ```bash
  dotnet remove package GdPicture.NET
  ```
  **Why:** Clean package removal to prevent conflicts.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to the project for PDF operations.

- [ ] **Update namespace imports (`GdPicture14` → `IronPdf`)**
  ```csharp
  // Before (GdPicture.NET)
  using GdPicture14;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure code references the correct library.

- [ ] **Replace `LicenseManager.RegisterKEY()` with `IronPdf.License.LicenseKey`**
  ```csharp
  // Before (GdPicture.NET)
  LicenseManager.RegisterKEY("YOUR-GDPICTURE-KEY");

  // After (IronPDF)
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** IronPDF requires setting the license key for activation.

- [ ] **Convert status code checks to try-catch blocks**
  ```csharp
  // Before (GdPicture.NET)
  GdPictureStatus status = pdf.LoadFromFile("file.pdf");
  if (status != GdPictureStatus.OK) { /* handle error */ }

  // After (IronPDF)
  try
  {
      var pdf = PdfDocument.FromFile("file.pdf");
  }
  catch (Exception ex)
  {
      // handle error
  }
  ```
  **Why:** IronPDF uses exceptions for error handling, simplifying code.

- [ ] **Update page indexing (1-indexed → 0-indexed)**
  ```csharp
  // Before (GdPicture.NET)
  pdf.SelectPage(1);

  // After (IronPDF)
  pdf.Pages[0];
  ```
  **Why:** IronPDF uses 0-based indexing, aligning with .NET standards.

- [ ] **Replace `GdPicturePDF` with `PdfDocument`**
  ```csharp
  // Before (GdPicture.NET)
  GdPicturePDF pdf = new GdPicturePDF();

  // After (IronPDF)
  PdfDocument pdf = new PdfDocument();
  ```
  **Why:** Transition to IronPDF's PdfDocument for PDF manipulation.

- [ ] **Replace `GdPictureDocumentConverter` with `ChromePdfRenderer`**
  ```csharp
  // Before (GdPicture.NET)
  GdPictureDocumentConverter converter = new GdPictureDocumentConverter();

  // After (IronPDF)
  ChromePdfRenderer renderer = new ChromePdfRenderer();
  ```
  **Why:** Use IronPDF's ChromePdfRenderer for HTML to PDF conversion.

- [ ] **Update method calls per API mapping table**
  **Why:** Ensure all PDF operations are updated to use IronPDF's API.

- [ ] **Convert coordinate-based text to HTML stamping**
  ```csharp
  // Before (GdPicture.NET)
  pdf.DrawText(100, 100, "Sample Text");

  // After (IronPDF)
  pdf.ApplyWatermark("<div style='position:absolute; top:100px; left:100px;'>Sample Text</div>");
  ```
  **Why:** IronPDF uses HTML/CSS for text placement, providing more flexibility.

- [ ] **Update unit conversions (points → millimeters where needed)**
  **Why:** IronPDF uses millimeters for measurements, aligning with PDF standards.

### Testing

- [ ] **Unit test all PDF generation paths**
  **Why:** Verify functionality and correctness of PDF generation.

- [ ] **Verify HTML rendering quality matches or exceeds**
  **Why:** Ensure rendering quality meets expectations with IronPDF's Chromium engine.

- [ ] **Test all security/encryption scenarios**
  **Why:** Validate that PDF security features work as intended.

- [ ] **Verify form filling functionality**
  **Why:** Ensure form fields are correctly handled in PDFs.

- [ ] **Test merge/split operations**
  **Why:** Confirm that PDF merging and splitting work correctly.

- [ ] **Validate watermark appearance**
  **Why:** Ensure watermarks are applied as expected.

- [ ] **Test header/footer rendering**
  **Why:** Verify that headers and footers are correctly rendered.

- [ ] **Performance benchmark critical paths**
  **Why:** Ensure performance is acceptable in key areas.

- [ ] **Memory usage profiling**
  **Why:** Identify and address any memory usage issues.

### Post-Migration

- [ ] **Remove GdPicture license files/keys**
  **Why:** Clean up old licensing information to prevent conflicts.

- [ ] **Update documentation**
  **Why:** Ensure all documentation reflects the new library usage.

- [ ] **Train team on IronPDF API patterns**
  **Why:** Ensure all team members are familiar with the new API.

- [ ] **Monitor production for any issues**
  **Why:** Quickly identify and resolve any post-migration issues.

- [ ] **Archive GdPicture-related code/documentation**
  **Why:** Preserve historical information for reference if needed.
```
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [IronPDF Code Examples](https://ironpdf.com/examples/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronOCR for OCR Features](https://ironsoftware.com/csharp/ocr/)
- [IronBarcode for Barcode Features](https://ironsoftware.com/csharp/barcode/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
