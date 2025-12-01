# How Do I Migrate from BitMiracle Docotic.Pdf to IronPDF in C#?

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

| Aspect | Docotic.Pdf | IronPDF |
|--------|-------------|---------|
| **HTML-to-PDF** | Requires separate add-on (HtmlToPdf) | Built-in core feature |
| **Package Structure** | Core + multiple add-ons | Single NuGet package |
| **Licensing Model** | Per-add-on licensing | All features included |
| **API Complexity** | Separate namespaces per add-on | Unified API |
| **HTML Engine** | Chromium (via add-on) | Chromium (built-in) |
| **Community Size** | Smaller | Larger, more resources |
| **Documentation** | Technical reference | Extensive tutorials |

### Feature Comparison

| Feature | Docotic.Pdf | IronPDF |
|---------|-------------|---------|
| Create PDF from scratch | ✅ | ✅ |
| HTML to PDF | ✅ (add-on required) | ✅ (built-in) |
| URL to PDF | ✅ (add-on required) | ✅ (built-in) |
| PDF manipulation | ✅ | ✅ |
| Text extraction | ✅ | ✅ |
| Merge/Split | ✅ | ✅ |
| Digital signatures | ✅ | ✅ |
| Encryption | ✅ | ✅ |
| Form filling | ✅ | ✅ |
| PDF/A compliance | ✅ | ✅ |
| Watermarks | ✅ | ✅ |
| 100% managed code | ✅ | ❌ (Chromium engine) |
| Page layout via code | ✅ | HTML/CSS-based |

### Migration Benefits

1. **Simplified Package Management**: No add-on complexity—IronPDF includes all features in one package
2. **HTML-First Approach**: Native HTML/CSS rendering without extra dependencies
3. **Larger Ecosystem**: More StackOverflow answers, tutorials, and community support
4. **Modern API Design**: Fluent, intuitive methods that feel natural to .NET developers
5. **Consistent Licensing**: One license covers all features
6. **Active Development**: Regular updates with new features and improvements

---

## Before You Start

### Prerequisites

- **.NET Version**: IronPDF supports .NET Framework 4.6.2+, .NET Core 3.1+, .NET 5/6/7/8/9
- **NuGet Access**: Ensure you can install packages from nuget.org
- **License Key**: Obtain from [IronPDF website](https://ironpdf.com/) (free trial available)

### Find All Docotic.Pdf References

```bash
# Find all Docotic.Pdf usages in your codebase
grep -r "using BitMiracle.Docotic" --include="*.cs" .
grep -r "PdfDocument\|PdfPage\|PdfCanvas" --include="*.cs" .

# Find NuGet package references
grep -r "Docotic.Pdf" --include="*.csproj" .
grep -r "Docotic.Pdf" --include="packages.config" .
```

### Breaking Changes Overview

| Change | Docotic.Pdf | IronPDF | Impact |
|--------|-------------|---------|--------|
| **HTML rendering** | Requires HtmlToPdf add-on | Built-in | Remove add-on package |
| **Page indexing** | 0-based (`Pages[0]`) | 0-based (`Pages[0]`) | No change needed |
| **Coordinate system** | Bottom-left origin | HTML/CSS flow | Use CSS for positioning |
| **Canvas drawing** | `PdfCanvas.DrawText()` | HTML markup | Paradigm shift |
| **Text extraction** | `page.GetText()` / `TextData.GetText()` | `pdf.ExtractAllText()` | Method name change |
| **Document loading** | `new PdfDocument(path)` | `PdfDocument.FromFile(path)` | Constructor → static method |
| **Saving** | `document.Save(path)` | `pdf.SaveAs(path)` | Method name change |
| **Disposal** | `IDisposable` pattern | Not required | Simpler resource management |

---

## Quick Start (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove Docotic.Pdf packages
dotnet remove package BitMiracle.Docotic.Pdf
dotnet remove package BitMiracle.Docotic.Pdf.HtmlToPdf
dotnet remove package BitMiracle.Docotic.Pdf.Layout

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Using Statements

```csharp
// Before
using BitMiracle.Docotic.Pdf;
using BitMiracle.Docotic.Pdf.Layout;
// using BitMiracle.Docotic.Pdf.HtmlToPdf; // if using HTML add-on

// After
using IronPdf;
```

### Step 3: Apply License Key

```csharp
// Add at application startup (Program.cs or Global.asax)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 4: Basic Code Migration

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;

using (var pdf = new PdfDocument())
{
    var page = pdf.Pages[0];
    var canvas = page.Canvas;
    canvas.DrawString(50, 50, "Hello World");
    pdf.Save("output.pdf");
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

| Docotic.Pdf Namespace | IronPDF Namespace | Notes |
|-----------------------|-------------------|-------|
| `BitMiracle.Docotic.Pdf` | `IronPdf` | Main namespace |
| `BitMiracle.Docotic.Pdf.Layout` | `IronPdf` | Layout via HTML/CSS |
| `BitMiracle.Docotic.Pdf.HtmlToPdf` | `IronPdf` | Built-in, no add-on |

### Document Operations

| Task | Docotic.Pdf | IronPDF |
|------|-------------|---------|
| Create empty document | `new PdfDocument()` | `new PdfDocument()` |
| Load from file | `new PdfDocument(path)` | `PdfDocument.FromFile(path)` |
| Load from stream | `PdfDocument.Load(stream)` | `PdfDocument.FromStream(stream)` |
| Load from bytes | `PdfDocument.Load(bytes)` | `PdfDocument.FromBinaryData(bytes)` |
| Save to file | `document.Save(path)` | `pdf.SaveAs(path)` |
| Save to stream | `document.Save(stream)` | `pdf.SaveAsStream()` |
| Save to bytes | `document.Save()` returns bytes | `pdf.BinaryData` |
| Get page count | `document.PageCount` | `pdf.PageCount` |
| Close/Dispose | `document.Dispose()` | Not required |

### HTML to PDF Conversion

| Task | Docotic.Pdf (HtmlToPdf Add-on) | IronPDF |
|------|-------------------------------|---------|
| HTML string to PDF | `HtmlConverter.Create(html).ToPdf()` | `renderer.RenderHtmlAsPdf(html)` |
| HTML file to PDF | `HtmlConverter.Create(new Uri(filePath)).ToPdf()` | `renderer.RenderHtmlFileAsPdf(path)` |
| URL to PDF | `HtmlConverter.Create(new Uri(url)).ToPdf()` | `renderer.RenderUrlAsPdf(url)` |
| Set page size | `options.PageSize = PageSize.A4` | `renderer.RenderingOptions.PaperSize = PdfPaperSize.A4` |
| Set margins | `options.PageMargins = new Margins(20)` | `renderer.RenderingOptions.MarginTop = 20` |
| Set orientation | `options.PageOrientation = Orientation.Landscape` | `renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape` |
| JavaScript wait | `options.WaitTime = TimeSpan.FromSeconds(3)` | `renderer.RenderingOptions.WaitFor.JavaScript(3000)` |

### Page Operations

| Task | Docotic.Pdf | IronPDF |
|------|-------------|---------|
| Access page | `document.Pages[index]` | `pdf.Pages[index]` |
| Add page | `document.AddPage()` | Via HTML or `Merge()` |
| Insert page | `document.InsertPage(index)` | Via page manipulation |
| Remove page | `document.RemovePage(index)` | `pdf.RemovePages(index)` |
| Copy pages | `document.CopyPage(index)` | `pdf.CopyPages(start, end)` |
| Get page size | `page.Width` / `page.Height` | `page.Width` / `page.Height` |
| Rotate page | `page.Rotation = PageRotation.Rotate90` | `pdf.Pages[i].Rotation = PdfPageRotation.Rotate90` |

### Text Operations

| Task | Docotic.Pdf | IronPDF |
|------|-------------|---------|
| Extract all text | `document.GetText()` | `pdf.ExtractAllText()` |
| Extract page text | `page.GetText()` | `pdf.Pages[i].Text` |
| Extract text with location | `page.GetWords()` | Via advanced extraction |
| Draw text on canvas | `canvas.DrawString(x, y, text)` | Use HTML with CSS positioning |
| Set font | `canvas.Font = new PdfFont(...)` | CSS `font-family` |
| Set font size | `canvas.FontSize = 12` | CSS `font-size` |

### Merge and Split

| Task | Docotic.Pdf | IronPDF |
|------|-------------|---------|
| Merge documents | `doc1.Append(doc2)` | `PdfDocument.Merge(pdf1, pdf2)` |
| Merge multiple | Loop with `Append()` | `PdfDocument.Merge(pdfList)` |
| Split document | `document.CopyPage(index)` to new doc | `pdf.CopyPages(start, end)` |
| Extract page range | Loop extraction | `pdf.CopyPages(indices)` |

### Form Operations

| Task | Docotic.Pdf | IronPDF |
|------|-------------|---------|
| Access form | `document.Form` | `pdf.Form` |
| Get all fields | `document.Form.Fields` | `pdf.Form.Fields` |
| Get field by name | `document.Form.GetField(name)` | `pdf.Form.GetFieldByName(name)` |
| Set field value | `field.Value = "text"` | `pdf.Form.SetFieldValue(name, value)` |
| Flatten forms | `document.FlattenForm()` | `pdf.Form.Flatten()` |

### Security and Encryption

| Task | Docotic.Pdf | IronPDF |
|------|-------------|---------|
| Set owner password | `document.Encrypt(ownerPassword, userPassword, permissions)` | `pdf.SecuritySettings.OwnerPassword = "pass"` |
| Set user password | Part of `Encrypt()` method | `pdf.SecuritySettings.UserPassword = "pass"` |
| Allow printing | `PdfPermissions.Printing` | `pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights` |
| Allow copying | `PdfPermissions.CopyContent` | `pdf.SecuritySettings.AllowUserCopyPasteContent = true` |
| Remove security | `document.Decrypt()` | `pdf.SecuritySettings.RemovePasswordsAndEncryption()` |
| Check encryption | `document.GetEncryptionInfo()` | `pdf.SecuritySettings` properties |

### Digital Signatures

| Task | Docotic.Pdf | IronPDF |
|------|-------------|---------|
| Sign document | `document.Sign(certificate)` | `pdf.Sign(certificate)` |
| Add signature field | `document.SignatureFields.Add(...)` | `pdf.SignatureFields[name]` |
| Verify signature | `signature.Verify()` | Via signature object |
| Add timestamp | `document.TimestampAndSave()` | Via signing options |

### Images and Graphics

| Task | Docotic.Pdf | IronPDF |
|------|-------------|---------|
| Add image | `canvas.DrawImage(image, x, y, width, height)` | HTML `<img>` tag |
| Extract images | `page.ExtractImages()` | `pdf.ExtractAllImages()` |
| Add watermark | `canvas.DrawString()` with opacity | `pdf.ApplyWatermark(html)` |
| Stamp content | Draw on canvas | `pdf.ApplyStamp(stamp)` |
| PDF to image | `page.Render()` | `pdf.RasterizeToImageFiles()` |

### Metadata

| Task | Docotic.Pdf | IronPDF |
|------|-------------|---------|
| Get/Set title | `document.Metadata.Title` | `pdf.MetaData.Title` |
| Get/Set author | `document.Metadata.Author` | `pdf.MetaData.Author` |
| Get/Set subject | `document.Metadata.Subject` | `pdf.MetaData.Subject` |
| Get/Set keywords | `document.Metadata.Keywords` | `pdf.MetaData.Keywords` |
| Get/Set creator | `document.Metadata.Creator` | `pdf.MetaData.Creator` |

### Annotations and Bookmarks

| Task | Docotic.Pdf | IronPDF |
|------|-------------|---------|
| Add annotation | `page.Annotations.Add(...)` | Via PDF editing |
| Get annotations | `page.Annotations` | Via PDF structure |
| Add bookmark | `document.Outlines.Add(...)` | `pdf.BookMarks.AddBookMarkAtStart(...)` |
| Get bookmarks | `document.Outlines` | `pdf.BookMarks` |

---

## Code Migration Examples

### Example 1: HTML to PDF (Simplest Case)

**Before (Docotic.Pdf with HtmlToPdf Add-on):**
```csharp
using BitMiracle.Docotic.Pdf;
using BitMiracle.Docotic.Pdf.HtmlToPdf;

class Program
{
    static async Task Main()
    {
        // Chromium engine must download on first use
        using var engine = await HtmlEngine.CreateAsync();

        var options = new HtmlConversionOptions();
        options.PageSize = PaperKind.A4;
        options.PageMargins = new PageMargins(20);

        string html = "<html><body><h1>Hello World</h1></body></html>";

        using var pdf = await engine.CreatePdfAsync(html, options);
        pdf.Save("output.pdf");

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
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.MarginLeft = 20;
        renderer.RenderingOptions.MarginRight = 20;

        string html = "<html><body><h1>Hello World</h1></body></html>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");

        Console.WriteLine("PDF created successfully");
    }
}
```

### Example 2: URL to PDF

**Before (Docotic.Pdf with HtmlToPdf Add-on):**
```csharp
using BitMiracle.Docotic.Pdf;
using BitMiracle.Docotic.Pdf.HtmlToPdf;

class Program
{
    static async Task Main()
    {
        using var engine = await HtmlEngine.CreateAsync();

        var options = new HtmlConversionOptions();
        options.PageOrientation = PageOrientation.Landscape;
        options.WaitTime = TimeSpan.FromSeconds(5); // Wait for JS

        using var pdf = await engine.CreatePdfAsync(
            new Uri("https://example.com"), options);
        pdf.Save("webpage.pdf");
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
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
        renderer.RenderingOptions.WaitFor.JavaScript(5000);

        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

### Example 3: Text Extraction

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;

class Program
{
    static void Main()
    {
        using (var pdf = new PdfDocument("document.pdf"))
        {
            // Method 1: Get all text
            string allText = pdf.GetText();
            Console.WriteLine(allText);

            // Method 2: Page by page
            foreach (var page in pdf.Pages)
            {
                string pageText = page.GetText();
                Console.WriteLine($"Page: {pageText}");
            }

            // Method 3: Get words with positions
            foreach (var page in pdf.Pages)
            {
                var words = page.GetWords();
                foreach (var word in words)
                {
                    Console.WriteLine($"{word.Text} at ({word.Bounds.Left}, {word.Bounds.Top})");
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
        var pdf = PdfDocument.FromFile("document.pdf");

        // Method 1: Get all text
        string allText = pdf.ExtractAllText();
        Console.WriteLine(allText);

        // Method 2: Page by page
        for (int i = 0; i < pdf.PageCount; i++)
        {
            string pageText = pdf.ExtractTextFromPage(i);
            Console.WriteLine($"Page {i + 1}: {pageText}");
        }

        // Method 3: Get text from specific pages
        string rangeText = pdf.ExtractTextFromPages(
            Enumerable.Range(0, 3).ToArray()); // First 3 pages
        Console.WriteLine(rangeText);
    }
}
```

### Example 4: Merge PDFs

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;

class Program
{
    static void Main()
    {
        using (var pdf1 = new PdfDocument("document1.pdf"))
        using (var pdf2 = new PdfDocument("document2.pdf"))
        using (var pdf3 = new PdfDocument("document3.pdf"))
        {
            pdf1.Append(pdf2);
            pdf1.Append(pdf3);
            pdf1.Save("merged.pdf");
        }

        Console.WriteLine("PDFs merged successfully");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var pdf1 = PdfDocument.FromFile("document1.pdf");
        var pdf2 = PdfDocument.FromFile("document2.pdf");
        var pdf3 = PdfDocument.FromFile("document3.pdf");

        var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
        merged.SaveAs("merged.pdf");

        Console.WriteLine("PDFs merged successfully");
    }
}
```

### Example 5: Split PDF

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;

class Program
{
    static void Main()
    {
        using (var pdf = new PdfDocument("large-document.pdf"))
        {
            // Extract pages 0-4 (first 5 pages)
            using (var part1 = new PdfDocument())
            {
                for (int i = 0; i < 5 && i < pdf.PageCount; i++)
                {
                    part1.Pages.Add(pdf.Pages[i]);
                }
                part1.Save("part1.pdf");
            }

            // Extract pages 5-9 (next 5 pages)
            using (var part2 = new PdfDocument())
            {
                for (int i = 5; i < 10 && i < pdf.PageCount; i++)
                {
                    part2.Pages.Add(pdf.Pages[i]);
                }
                part2.Save("part2.pdf");
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
        var pdf = PdfDocument.FromFile("large-document.pdf");

        // Extract pages 0-4 (first 5 pages)
        var part1 = pdf.CopyPages(0, 4);
        part1.SaveAs("part1.pdf");

        // Extract pages 5-9 (next 5 pages)
        var part2 = pdf.CopyPages(5, 9);
        part2.SaveAs("part2.pdf");

        // Or extract specific pages
        var specific = pdf.CopyPages(new[] { 0, 2, 4, 6 });
        specific.SaveAs("specific-pages.pdf");
    }
}
```

### Example 6: Watermark

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;

class Program
{
    static void Main()
    {
        using (var pdf = new PdfDocument("document.pdf"))
        {
            foreach (var page in pdf.Pages)
            {
                var canvas = page.Canvas;

                // Set transparency
                canvas.SaveState();
                canvas.SetTransparency(0.3); // 30% opacity

                // Set font and color
                canvas.FontSize = 72;
                canvas.FillColor = new PdfRgbColor(255, 0, 0);

                // Rotate and draw watermark
                canvas.Rotate(45, page.Width / 2, page.Height / 2);
                canvas.DrawString(page.Width / 4, page.Height / 2, "CONFIDENTIAL");

                canvas.RestoreState();
            }

            pdf.Save("watermarked.pdf");
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
        var pdf = PdfDocument.FromFile("document.pdf");

        // Apply HTML watermark
        string watermarkHtml = @"
            <div style='
                font-size: 72px;
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

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;

class Program
{
    static void Main()
    {
        using (var pdf = new PdfDocument("form.pdf"))
        {
            var form = pdf.Form;

            // Get field by name and set value
            var nameField = form.GetField("CustomerName");
            if (nameField != null)
                nameField.Value = "John Doe";

            var emailField = form.GetField("Email");
            if (emailField != null)
                emailField.Value = "john@example.com";

            // Set checkbox
            var agreeField = form.GetField("AgreeToTerms") as PdfCheckBox;
            if (agreeField != null)
                agreeField.Checked = true;

            // Flatten the form (make fields non-editable)
            pdf.FlattenForm();

            pdf.Save("filled-form.pdf");
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
        var pdf = PdfDocument.FromFile("form.pdf");

        // Set field values
        pdf.Form.SetFieldValue("CustomerName", "John Doe");
        pdf.Form.SetFieldValue("Email", "john@example.com");
        pdf.Form.SetFieldValue("AgreeToTerms", "Yes");

        // List all fields
        foreach (var field in pdf.Form.Fields)
        {
            Console.WriteLine($"Field: {field.Name}, Value: {field.Value}");
        }

        // Flatten the form
        pdf.Form.Flatten();

        pdf.SaveAs("filled-form.pdf");
    }
}
```

### Example 8: Encryption and Security

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;

class Program
{
    static void Main()
    {
        using (var pdf = new PdfDocument("document.pdf"))
        {
            // Set encryption with permissions
            var permissions = PdfPermissions.Printing |
                             PdfPermissions.ContentExtraction;

            pdf.Encrypt(
                ownerPassword: "owner123",
                userPassword: "user123",
                permissions: permissions,
                encryptionAlgorithm: PdfEncryptionAlgorithm.Aes256
            );

            pdf.Save("encrypted.pdf");
        }

        // Open encrypted PDF
        using (var encrypted = new PdfDocument("encrypted.pdf", "user123"))
        {
            var info = encrypted.GetEncryptionInfo();
            Console.WriteLine($"Encryption: {info?.EncryptionAlgorithm}");
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
        var pdf = PdfDocument.FromFile("document.pdf");

        // Set security options
        pdf.SecuritySettings.OwnerPassword = "owner123";
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserCopyPasteContent = true;
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

        pdf.SaveAs("encrypted.pdf");

        // Open encrypted PDF
        var encrypted = PdfDocument.FromFile("encrypted.pdf", "user123");
        Console.WriteLine($"Page count: {encrypted.PageCount}");
    }
}
```

### Example 9: Digital Signature

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;
using System.Security.Cryptography.X509Certificates;

class Program
{
    static void Main()
    {
        using (var pdf = new PdfDocument("contract.pdf"))
        {
            // Load certificate
            var certificate = new X509Certificate2("certificate.pfx", "password");

            // Create signature field
            var signatureField = pdf.Pages[0].Annotations.Add(
                new PdfSignatureFieldAnnotation(50, 50, 200, 50));

            // Sign the document
            pdf.Sign(certificate, new PdfSigningOptions
            {
                Field = signatureField,
                Reason = "Contract Approval",
                Location = "New York",
                ContactInfo = "legal@example.com"
            });

            pdf.Save("signed-contract.pdf");
        }
    }
}
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
        var pdf = PdfDocument.FromFile("contract.pdf");

        // Create signature
        var signature = new PdfSignature("certificate.pfx", "password")
        {
            SigningContact = "legal@example.com",
            SigningLocation = "New York",
            SigningReason = "Contract Approval"
        };

        // Apply signature
        pdf.Sign(signature);
        pdf.SaveAs("signed-contract.pdf");
    }
}
```

### Example 10: PDF to Images

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;
using System.Drawing;

class Program
{
    static void Main()
    {
        using (var pdf = new PdfDocument("document.pdf"))
        {
            for (int i = 0; i < pdf.PageCount; i++)
            {
                var page = pdf.Pages[i];

                // Render page to image
                using (var image = page.Render(300)) // 300 DPI
                {
                    image.Save($"page-{i + 1}.png");
                }
            }
        }

        Console.WriteLine("Pages converted to images");
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

        // Convert all pages to images
        pdf.RasterizeToImageFiles("page-*.png", 300); // 300 DPI

        // Or get images as bytes
        var images = pdf.ToPngImages(300);
        for (int i = 0; i < images.Length; i++)
        {
            System.IO.File.WriteAllBytes($"page-{i + 1}.png", images[i]);
        }

        Console.WriteLine("Pages converted to images");
    }
}
```

---

## Advanced Scenarios

### ASP.NET Core Integration

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;
using BitMiracle.Docotic.Pdf.HtmlToPdf;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly HtmlEngine _engine;

    public ReportController()
    {
        // HtmlEngine is expensive to create
        _engine = HtmlEngine.CreateAsync().GetAwaiter().GetResult();
    }

    [HttpGet("generate")]
    public async Task<IActionResult> GenerateReport()
    {
        string html = "<h1>Report</h1><p>Content...</p>";

        var options = new HtmlConversionOptions();
        using var pdf = await _engine.CreatePdfAsync(html, options);

        using var stream = new MemoryStream();
        pdf.Save(stream);
        stream.Position = 0;

        return File(stream.ToArray(), "application/pdf", "report.pdf");
    }

    public void Dispose()
    {
        _engine?.Dispose();
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
        string html = "<h1>Report</h1><p>Content...</p>";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);

        return File(pdf.BinaryData, "application/pdf", "report.pdf");
    }
}
```

### Dependency Injection

**Before (Docotic.Pdf):**
```csharp
// Startup.cs - Complex setup for HtmlEngine
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<HtmlEngine>(sp =>
    {
        return HtmlEngine.CreateAsync().GetAwaiter().GetResult();
    });
}

// Service
public class PdfService
{
    private readonly HtmlEngine _engine;

    public PdfService(HtmlEngine engine)
    {
        _engine = engine;
    }

    public async Task<byte[]> CreatePdfAsync(string html)
    {
        using var pdf = await _engine.CreatePdfAsync(html);
        return pdf.GetBytes();
    }
}
```

**After (IronPDF):**
```csharp
// Startup.cs - Simple setup
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IPdfRenderer, ChromePdfRenderer>();
}

// Interface for testability
public interface IPdfRenderer
{
    PdfDocument RenderHtmlAsPdf(string html);
}

// Service
public class PdfService
{
    private readonly IPdfRenderer _renderer;

    public PdfService(IPdfRenderer renderer)
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

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;
using BitMiracle.Docotic.Pdf.HtmlToPdf;

public class AsyncPdfGenerator
{
    private readonly HtmlEngine _engine;

    public AsyncPdfGenerator()
    {
        _engine = HtmlEngine.CreateAsync().GetAwaiter().GetResult();
    }

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        var options = new HtmlConversionOptions
        {
            WaitTime = TimeSpan.FromSeconds(3)
        };

        using var pdf = await _engine.CreatePdfAsync(html, options);
        return pdf.GetBytes();
    }

    public async Task<byte[]> GenerateFromUrlAsync(Uri url)
    {
        using var pdf = await _engine.CreatePdfAsync(url);
        return pdf.GetBytes();
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
        renderer.RenderingOptions.WaitFor.JavaScript(3000);

        // IronPDF provides async methods
        var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }

    public async Task<byte[]> GenerateFromUrlAsync(string url)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = await renderer.RenderUrlAsPdfAsync(url);
        return pdf.BinaryData;
    }
}
```

### Batch Processing

**Before (Docotic.Pdf):**
```csharp
using BitMiracle.Docotic.Pdf;

public class BatchProcessor
{
    public void ProcessFiles(string[] filePaths)
    {
        foreach (var path in filePaths)
        {
            using (var pdf = new PdfDocument(path))
            {
                // Process each PDF
                string text = pdf.GetText();
                int pages = pdf.PageCount;

                // Add watermark to each page
                foreach (var page in pdf.Pages)
                {
                    var canvas = page.Canvas;
                    canvas.SaveState();
                    canvas.SetTransparency(0.5);
                    canvas.DrawString(50, 50, "PROCESSED");
                    canvas.RestoreState();
                }

                pdf.Save(path.Replace(".pdf", "-processed.pdf"));
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

public class BatchProcessor
{
    public void ProcessFiles(string[] filePaths)
    {
        // Process in parallel for better performance
        Parallel.ForEach(filePaths, path =>
        {
            var pdf = PdfDocument.FromFile(path);

            // Get info
            string text = pdf.ExtractAllText();
            int pages = pdf.PageCount;

            // Add watermark
            pdf.ApplyWatermark("<div style='opacity:0.5'>PROCESSED</div>");

            pdf.SaveAs(path.Replace(".pdf", "-processed.pdf"));
        });
    }
}
```

### Docker Deployment

**Before (Docotic.Pdf with HtmlToPdf):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Chromium dependencies for HtmlToPdf add-on
RUN apt-get update && apt-get install -y \
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
    libpango-1.0-0 \
    libcairo2 \
    libatspi2.0-0 \
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

**After (IronPDF):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# IronPDF Linux dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libx11-dev \
    && rm -rf /var/lib/apt/lists/*

# Chrome dependencies for HTML rendering
RUN apt-get update && apt-get install -y \
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

| Scenario | Docotic.Pdf | IronPDF | Notes |
|----------|-------------|---------|-------|
| Simple HTML | Similar | Similar | Both use Chromium |
| Complex CSS | Similar | Similar | Modern CSS support |
| JavaScript-heavy | Similar | Similar | Chromium handles JS |
| Canvas drawing | Faster | N/A | Use HTML instead |
| Large documents | Similar | Similar | Both handle well |

### Memory Optimization

**Docotic.Pdf:**
```csharp
// Uses IDisposable pattern
using (var pdf = new PdfDocument("large.pdf"))
{
    // Process...
} // Disposed here
```

**IronPDF:**
```csharp
// IronPDF handles memory internally
var pdf = PdfDocument.FromFile("large.pdf");
// Process...
// GC handles cleanup, or dispose explicitly:
pdf.Dispose();
```

### Batch Processing Optimization

```csharp
using IronPdf;
using System.Threading.Tasks;

public class OptimizedProcessor
{
    public async Task ProcessLargeBatchAsync(string[] htmlContents)
    {
        var renderer = new ChromePdfRenderer();

        // Configure for batch processing
        renderer.RenderingOptions.Timeout = 60000;

        var tasks = htmlContents.Select(html =>
            renderer.RenderHtmlAsPdfAsync(html));

        var pdfs = await Task.WhenAll(tasks);

        // Merge all PDFs
        var final = PdfDocument.Merge(pdfs);
        final.SaveAs("combined.pdf");
    }
}
```

---

## Troubleshooting Guide

### Issue 1: Missing HTML Engine

**Error (Docotic.Pdf):**
```
System.IO.FileNotFoundException: Could not load file or assembly 'BitMiracle.Docotic.Pdf.HtmlToPdf'
```

**Solution:** IronPDF includes HTML rendering built-in—no separate add-on needed.

```csharp
// Just use IronPdf namespace
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
```

### Issue 2: Canvas Drawing Migration

**Error:** No direct equivalent for `PdfCanvas.DrawText()`

**Solution:** Use HTML/CSS for positioning:

```csharp
// Before (Docotic.Pdf)
canvas.DrawString(100, 200, "Text at position");

// After (IronPDF) - Use CSS absolute positioning
var html = @"
<div style='position: absolute; left: 100px; top: 200px;'>
    Text at position
</div>";
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 3: Coordinate System Differences

**Problem:** Docotic.Pdf uses bottom-left origin; HTML uses top-left.

**Solution:** Adjust Y coordinates or use CSS:

```csharp
// Docotic.Pdf: y=0 is bottom
// HTML/CSS: top=0 is top

// For precise positioning, use CSS
var html = @"
<div style='
    position: absolute;
    bottom: 50px;  /* Distance from bottom */
    left: 100px;
'>Content</div>";
```

### Issue 4: Font Loading

**Error (Docotic.Pdf):** Required explicit font registration.

**Solution:** IronPDF uses system fonts or CSS web fonts:

```csharp
var html = @"
<style>
    @import url('https://fonts.googleapis.com/css2?family=Roboto');
    body { font-family: 'Roboto', sans-serif; }
</style>
<body>Text with Google Font</body>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 5: License Key Migration

**Problem:** Docotic.Pdf uses license file; IronPDF uses code key.

**Solution:**
```csharp
// Before (Docotic.Pdf)
// License applied via license file or:
BitMiracle.Docotic.LicenseManager.AddLicenseData("LICENSE-KEY");

// After (IronPDF)
IronPdf.License.LicenseKey = "YOUR-IRONPDF-KEY";
```

### Issue 6: Async Pattern Differences

**Problem:** Docotic.Pdf HtmlToPdf uses async heavily; core library is sync.

**Solution:** IronPDF supports both:

```csharp
// Sync
var pdf = renderer.RenderHtmlAsPdf(html);

// Async
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### Issue 7: Text Extraction Differences

**Problem:** Text output format differs between libraries.

**Solution:** Test and adjust parsing:

```csharp
// IronPDF extraction
var text = pdf.ExtractAllText();

// If formatting differs, extract page by page
for (int i = 0; i < pdf.PageCount; i++)
{
    var pageText = pdf.ExtractTextFromPage(i);
    Console.WriteLine($"--- Page {i + 1} ---\n{pageText}");
}
```

### Issue 8: Linux Deployment Issues

**Error:** Missing libraries on Linux.

**Solution:** Install required dependencies:

```bash
# Ubuntu/Debian
apt-get install -y libgdiplus libc6-dev libx11-dev

# For HTML rendering, also install Chrome dependencies:
apt-get install -y libnss3 libatk-bridge2.0-0 libdrm2 libxkbcommon0 \
    libxcomposite1 libxdamage1 libxfixes3 libxrandr2 libgbm1 libasound2
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Docotic.Pdf usages in codebase**
  ```bash
  grep -r "using BitMiracle.Docotic.Pdf" --include="*.cs" .
  grep -r "PdfDocument\|PdfCanvas" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Note which add-ons are used (HtmlToPdf, Layout, etc.)**
  **Why:** Determine which features rely on add-ons to ensure equivalent functionality in IronPDF.

- [ ] **Identify canvas drawing code that needs HTML conversion**
  ```csharp
  // Before (Docotic.Pdf)
  var canvas = pdfPage.Canvas;
  canvas.DrawString("Hello, World!", font, brush, x, y);

  // After (IronPDF)
  var html = "<div style='position:absolute; left:" + x + "px; top:" + y + "px;'>Hello, World!</div>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** IronPDF uses HTML/CSS for layout, which may require converting canvas operations to HTML.

- [ ] **Document current PDF generation workflows**
  ```csharp
  // Before (Docotic.Pdf)
  var pdf = new PdfDocument();
  pdf.AddPage();
  pdf.Save("output.pdf");

  // After (IronPDF)
  var pdf = new ChromePdfRenderer().RenderHtmlAsPdf("<html><body></body></html>");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Understanding current workflows helps map them to IronPDF's capabilities.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Review IronPDF system requirements**
  **Why:** Ensure the target environment supports IronPDF's Chromium-based rendering.

- [ ] **Set up test environment**
  **Why:** Isolate changes and validate functionality without affecting production.

### During Migration

- [ ] **Remove Docotic.Pdf NuGet packages**
  ```bash
  dotnet remove package BitMiracle.Docotic.Pdf
  ```
  **Why:** Remove old dependencies to prevent conflicts.

- [ ] **Remove add-on packages (HtmlToPdf, Layout)**
  ```bash
  dotnet remove package BitMiracle.Docotic.Pdf.HtmlToPdf
  ```
  **Why:** IronPDF includes all features, eliminating the need for separate add-ons.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to enable new PDF functionalities.

- [ ] **Update using statements**
  ```csharp
  // Before (Docotic.Pdf)
  using BitMiracle.Docotic.Pdf;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure code references the correct library.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Convert canvas drawing to HTML/CSS**
  ```csharp
  // Before (Docotic.Pdf)
  var canvas = pdfPage.Canvas;
  canvas.DrawRectangle(pen, x, y, width, height);

  // After (IronPDF)
  var html = $"<div style='position:absolute; left:{x}px; top:{y}px; width:{width}px; height:{height}px; border:1px solid black;'></div>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** IronPDF uses HTML/CSS for layout, which may require converting canvas operations to HTML.

- [ ] **Update document loading methods**
  ```csharp
  // Before (Docotic.Pdf)
  var pdf = new PdfDocument("input.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("input.pdf");
  ```
  **Why:** Use IronPDF's method for loading existing PDFs.

- [ ] **Update save methods**
  ```csharp
  // Before (Docotic.Pdf)
  pdf.Save("output.pdf");

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Ensure PDFs are saved using IronPDF's API.

- [ ] **Convert text extraction calls**
  ```csharp
  // Before (Docotic.Pdf)
  var text = pdf.GetText();

  // After (IronPDF)
  var text = pdf.ExtractAllText();
  ```
  **Why:** Use IronPDF's text extraction methods for consistency.

- [ ] **Update merge/split operations**
  ```csharp
  // Before (Docotic.Pdf)
  var mergedPdf = new PdfDocument();
  mergedPdf.Append(pdf1);
  mergedPdf.Append(pdf2);

  // After (IronPDF)
  var mergedPdf = PdfDocument.Merge(pdf1, pdf2);
  ```
  **Why:** Simplify PDF merging with IronPDF's built-in methods.

- [ ] **Update form filling code**
  ```csharp
  // Before (Docotic.Pdf)
  var field = pdf.GetField("fieldName");
  field.Value = "value";

  // After (IronPDF)
  var form = pdf.Form;
  form["fieldName"].Value = "value";
  ```
  **Why:** Ensure form fields are accessed and modified using IronPDF's API.

- [ ] **Update encryption code**
  ```csharp
  // Before (Docotic.Pdf)
  pdf.Encryption = new PdfEncryption("userPassword", "ownerPassword");

  // After (IronPDF)
  pdf.SecuritySettings.UserPassword = "userPassword";
  pdf.SecuritySettings.OwnerPassword = "ownerPassword";
  ```
  **Why:** Use IronPDF's security settings for encryption.

- [ ] **Remove `using` statements if not needed**
  **Why:** Clean up code by removing unnecessary references.

### Post-Migration

- [ ] **Run all unit tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Verify text extraction accuracy**
  **Why:** Ensure text extraction is accurate with IronPDF's methods.

- [ ] **Test form filling functionality**
  **Why:** Confirm that form fields are correctly filled and saved.

- [ ] **Validate digital signatures**
  **Why:** Ensure digital signatures are correctly applied and validated.

- [ ] **Performance test batch operations**
  **Why:** Confirm that batch operations perform efficiently with IronPDF.

- [ ] **Test in all target environments**
  **Why:** Ensure compatibility across all environments where the application is deployed.

- [ ] **Update CI/CD pipelines**
  **Why:** Reflect changes in build and deployment processes.

- [ ] **Update Docker configurations**
  **Why:** Ensure Docker images are configured to support IronPDF's requirements.

- [ ] **Remove Docotic.Pdf license files**
  **Why:** Clean up old license files that are no longer needed.

- [ ] **Update documentation**
  **Why:** Ensure all documentation reflects the new library and its usage.
---

## Additional Resources

### IronPDF Documentation
- **Getting Started**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF**: https://ironpdf.com/how-to/html-file-to-pdf/

### Docotic.Pdf Reference (for migration)
- **API Documentation**: https://api.docotic.com/pdfdocument
- **Namespace Reference**: https://api.docotic.com/bitmiracle-docotic-pdf
- **GitHub Samples**: https://github.com/BitMiracle/Docotic.Pdf.Samples

### Code Examples
- **IronPDF GitHub**: https://github.com/nickaccessibility/IronPdf-Examples
- **Stack Overflow**: Search `[ironpdf]` tag

### Support
- **IronPDF Support**: https://ironpdf.com/support/
- **License Information**: https://ironpdf.com/licensing/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
