# How Do I Migrate from GemBox.Pdf to IronPDF in C#?

## Table of Contents
1. [Why Migrate from GemBox.Pdf to IronPDF](#why-migrate-from-gemboxpdf-to-ironpdf)
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

## Why Migrate from GemBox.Pdf to IronPDF

### The GemBox.Pdf Problems

GemBox.Pdf is a capable .NET PDF component, but it has significant limitations that affect real-world development:

1. **20 Paragraph Limit in Free Version**: The free version restricts you to 20 paragraphs, and table cells count toward this limit. A simple 10-row, 5-column table uses 50 "paragraphs," making the free version unusable for even basic documents.

2. **No HTML-to-PDF Conversion**: GemBox.Pdf requires programmatic document construction. You must calculate coordinates and manually position every element—there's no simple "render this HTML" capability.

3. **Coordinate-Based Layout**: Unlike HTML/CSS where layout flows naturally, GemBox.Pdf requires you to calculate exact X/Y positions for every text element, image, and shape.

4. **Limited Feature Set**: Compared to comprehensive PDF libraries, GemBox.Pdf focuses on basic operations—reading, writing, merging, splitting—without advanced features like form filling, digital signatures, or watermarking in the same intuitive way.

5. **Programmatic Only**: Every design change requires code changes. Want to tweak spacing? Recalculate coordinates. Want a different font size? Adjust all Y positions below it.

6. **Table Cell Counting**: The paragraph limit counts table cells, not just visible paragraphs. This makes the free version practically worthless for business documents with tables.

7. **Learning Curve for Design**: Developers must think in coordinates rather than document flow, making simple tasks like "add a paragraph" surprisingly complex.

For best practices, consult the [complete guide](https://ironpdf.com/blog/migration-guides/migrate-from-gembox-pdf-to-ironpdf/).

### IronPDF Advantages

| Aspect | GemBox.Pdf | IronPDF |
|--------|------------|---------|
| Free Version Limits | 20 paragraphs (includes table cells) | Watermark only, no content limits |
| HTML-to-PDF | Not supported | Full Chromium engine |
| Layout Approach | Coordinate-based, manual | HTML/CSS flow layout |
| Tables | Count toward paragraph limit | Unlimited, use HTML tables |
| Modern CSS | Not applicable | Flexbox, Grid, CSS3 animations |
| JavaScript Support | Not applicable | Full JavaScript execution |
| Design Changes | Recalculate coordinates | Edit HTML/CSS |
| Learning Curve | PDF coordinate system | HTML/CSS (web familiar) |

---

## Migration Complexity Assessment

### Estimated Effort by Feature

| Feature | Migration Complexity | Notes |
|---------|---------------------|-------|
| Load/Save PDFs | Very Low | Direct method mapping |
| Merge PDFs | Very Low | Direct method mapping |
| Split PDFs | Low | Page index handling |
| Text Extraction | Very Low | Direct method mapping |
| Add Text | Medium | Coordinate → HTML |
| Tables | Low | Manual → HTML tables |
| Images | Low | Coordinate → HTML |
| Watermarks | Low | Different API |
| Password Protection | Medium | Different structure |
| Form Fields | Medium | API differences |
| Digital Signatures | Medium | Different approach |

### Paradigm Shift

The biggest change is moving from **coordinate-based layout** to **HTML/CSS layout**:

```
GemBox.Pdf:  "Draw text at position (100, 700)"
IronPDF:     "Render this HTML with CSS styling"
```

This is generally easier, but requires thinking about PDFs differently.

---

## Before You Start

### Prerequisites

1. **.NET Version**: IronPDF supports .NET Framework 4.6.2+ and .NET Core 2.0+ / .NET 5+
2. **License Key**: Obtain your IronPDF license key from [ironpdf.com](https://ironpdf.com)
3. **Backup**: Create a branch for migration work
4. **HTML/CSS Knowledge**: Basic familiarity helpful but not required

### Identify All GemBox.Pdf Usage

```bash
# Find all GemBox.Pdf references
grep -r "GemBox\.Pdf\|PdfDocument\|PdfPage\|PdfFormattedText\|ComponentInfo\.SetLicense" --include="*.cs" .

# Find package references
grep -r "GemBox\.Pdf" --include="*.csproj" .
```

### NuGet Package Changes

```bash
# Remove GemBox.Pdf
dotnet remove package GemBox.Pdf

# Install IronPDF
dotnet add package IronPdf
```

### License Key Setup

**GemBox.Pdf:**
```csharp
// Must call before any GemBox operations
ComponentInfo.SetLicense("FREE-LIMITED-KEY");
// Or for professional:
ComponentInfo.SetLicense("YOUR-PROFESSIONAL-LICENSE");
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

**Before (GemBox.Pdf):**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content;

ComponentInfo.SetLicense("FREE-LIMITED-KEY");

using (var document = new PdfDocument())
{
    var page = document.Pages.Add();
    var formattedText = new PdfFormattedText();
    formattedText.AppendLine("Hello World!");
    formattedText.FontSize = 24;

    page.Content.DrawText(formattedText, new PdfPoint(100, 700));
    document.Save("output.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1 style='font-size:24px;'>Hello World!</h1>");
pdf.SaveAs("output.pdf");
```

**Key Differences:**
- No coordinate calculations needed
- HTML/CSS instead of programmatic layout
- No paragraph limits
- Simpler, more readable code

---

## Complete API Reference

### Namespace Mapping

| GemBox.Pdf | IronPDF |
|------------|---------|
| `GemBox.Pdf` | `IronPdf` |
| `GemBox.Pdf.Content` | `IronPdf` (content is HTML) |
| `GemBox.Pdf.Security` | `IronPdf` (SecuritySettings) |
| `GemBox.Pdf.Forms` | `IronPdf.Forms` |

### Core Class Mapping

| GemBox.Pdf | IronPDF | Description |
|------------|---------|-------------|
| `PdfDocument` | `PdfDocument` | Main PDF document class |
| `PdfPage` | `PdfDocument.Pages[i]` | Page representation |
| `PdfContent` | N/A (use HTML) | Page content |
| `PdfFormattedText` | N/A (use HTML) | Formatted text |
| `PdfPoint` | N/A (use CSS positioning) | Coordinate positioning |
| `ComponentInfo.SetLicense()` | `IronPdf.License.LicenseKey` | License management |

### Document Operations

| GemBox.Pdf | IronPDF | Notes |
|------------|---------|-------|
| `new PdfDocument()` | `new PdfDocument()` | Create new document |
| `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `PdfDocument.Load(stream)` | `PdfDocument.FromStream(stream)` | Load from stream |
| `document.Save(path)` | `pdf.SaveAs(path)` | Save to file |
| `document.Save(stream)` | `pdf.Stream` or `pdf.BinaryData` | Get as stream/bytes |

### Page Operations

| GemBox.Pdf | IronPDF | Notes |
|------------|---------|-------|
| `document.Pages.Add()` | Create via HTML rendering | Add new page |
| `document.Pages.Count` | `pdf.PageCount` | Page count |
| `document.Pages[index]` | `pdf.Pages[index]` | Access page (both 0-indexed) |
| `document.Pages.AddClone(pages)` | `PdfDocument.Merge()` | Clone pages |
| `document.Pages.Remove(page)` | `pdf.Pages.RemoveAt(index)` | Remove page |
| `page.Size` | `pdf.Pages[i].Width/Height` | Page dimensions |
| `page.Rotate` | `pdf.Pages[i].Rotation` | Page rotation |

### Text and Content Operations

| GemBox.Pdf | IronPDF | Notes |
|------------|---------|-------|
| `new PdfFormattedText()` | HTML string | Text content |
| `formattedText.Append(text)` | Include in HTML | Add text |
| `formattedText.AppendLine(text)` | Include in HTML | Add text with newline |
| `formattedText.FontSize = 12` | CSS `font-size: 12pt` | Font size |
| `formattedText.Font = ...` | CSS `font-family: ...` | Font family |
| `formattedText.Color = ...` | CSS `color: ...` | Text color |
| `page.Content.DrawText(text, point)` | `renderer.RenderHtmlAsPdf(html)` | Render text |
| `page.Content.DrawImage(image, x, y)` | Include in HTML | Add image |
| `page.Content.GetText()` | `pdf.ExtractTextFromPage(i)` | Extract text |

### Merge and Split Operations

| GemBox.Pdf | IronPDF | Notes |
|------------|---------|-------|
| `document.Pages.AddClone(source.Pages)` | `PdfDocument.Merge(pdf1, pdf2)` | Merge documents |
| Multiple `AddClone` calls | `PdfDocument.Merge(listOfPdfs)` | Merge many |
| Loop through pages | `pdf.CopyPages(indices)` | Split/extract pages |
| `page.Clone()` | `pdf.CopyPage(index)` | Copy single page |

### Security and Encryption

| GemBox.Pdf | IronPDF | Notes |
|------------|---------|-------|
| `document.SaveOptions.SetPasswordEncryption()` | `pdf.SecuritySettings` | Security config |
| `encryption.DocumentOpenPassword` | `pdf.SecuritySettings.UserPassword` | Open password |
| `encryption.PermissionsPassword` | `pdf.SecuritySettings.OwnerPassword` | Owner password |
| `encryption.Permissions` | Various `AllowUser*` properties | Permissions |
| `PdfEncryptionLevel.AES_256` | Default AES encryption | Encryption level |

### Form Field Operations

| GemBox.Pdf | IronPDF | Notes |
|------------|---------|-------|
| `document.Form.Fields` | `pdf.Form.Fields` | Form fields collection |
| `field.Value` | `pdf.Form.Fields[name].Value` | Get/set value |
| `field.Name` | `pdf.Form.Fields[i].Name` | Field name |
| Form field iteration | `foreach (var field in pdf.Form.Fields)` | Iterate fields |

### Metadata Operations

| GemBox.Pdf | IronPDF | Notes |
|------------|---------|-------|
| `document.Info.Title` | `pdf.MetaData.Title` | Document title |
| `document.Info.Author` | `pdf.MetaData.Author` | Author |
| `document.Info.Subject` | `pdf.MetaData.Subject` | Subject |
| `document.Info.Keywords` | `pdf.MetaData.Keywords` | Keywords |
| `document.Info.Creator` | `pdf.MetaData.Creator` | Creator application |

---

## Code Migration Examples

### Example 1: Simple Document Creation

**Before (GemBox.Pdf):**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        using (var document = new PdfDocument())
        {
            var page = document.Pages.Add();

            // Title
            var title = new PdfFormattedText();
            title.AppendLine("Invoice #12345");
            title.FontSize = 24;
            page.Content.DrawText(title, new PdfPoint(50, 750));

            // Date - calculate Y position manually
            var date = new PdfFormattedText();
            date.Append("Date: 2024-01-15");
            date.FontSize = 12;
            page.Content.DrawText(date, new PdfPoint(50, 720));

            // Body text - more Y position calculations
            var body = new PdfFormattedText();
            body.Append("Thank you for your business.");
            body.FontSize = 12;
            page.Content.DrawText(body, new PdfPoint(50, 680));

            document.Save("invoice.pdf");
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

        var html = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; padding: 50px; }
                    h1 { font-size: 24px; margin-bottom: 10px; }
                    .date { font-size: 12px; margin-bottom: 40px; }
                    p { font-size: 12px; }
                </style>
            </head>
            <body>
                <h1>Invoice #12345</h1>
                <div class='date'>Date: 2024-01-15</div>
                <p>Thank you for your business.</p>
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("invoice.pdf");
    }
}
```

### Example 2: Creating Tables (The Biggest Improvement!)

**Before (GemBox.Pdf) - Each cell counts toward 20-paragraph limit:**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        var products = new List<(string Name, decimal Price, int Qty)>
        {
            ("Widget A", 19.99m, 5),
            ("Widget B", 29.99m, 3),
            ("Widget C", 9.99m, 10),
            // Can only add a few more rows before hitting 20-paragraph limit!
        };

        using (var document = new PdfDocument())
        {
            var page = document.Pages.Add();
            double y = 700;
            double[] xPositions = { 50, 200, 300, 400 };

            // Headers (4 paragraphs)
            var headers = new[] { "Product", "Price", "Qty", "Total" };
            for (int i = 0; i < headers.Length; i++)
            {
                var text = new PdfFormattedText { Text = headers[i], FontSize = 12 };
                page.Content.DrawText(text, new PdfPoint(xPositions[i], y));
            }
            y -= 20;

            // Data rows (4 paragraphs per row!)
            foreach (var product in products)
            {
                var cells = new[] {
                    product.Name,
                    product.Price.ToString("C"),
                    product.Qty.ToString(),
                    (product.Price * product.Qty).ToString("C")
                };

                for (int i = 0; i < cells.Length; i++)
                {
                    var text = new PdfFormattedText { Text = cells[i], FontSize = 10 };
                    page.Content.DrawText(text, new PdfPoint(xPositions[i], y));
                }
                y -= 15;
            }

            document.Save("products.pdf");
        }
    }
}
```

**After (IronPDF) - No limits, proper HTML tables:**
```csharp
using IronPdf;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var products = new List<(string Name, decimal Price, int Qty)>
        {
            ("Widget A", 19.99m, 5),
            ("Widget B", 29.99m, 3),
            ("Widget C", 9.99m, 10),
            // Add hundreds or thousands of rows - no limit!
        };

        var rows = string.Join("\n", products.Select(p =>
            $"<tr><td>{p.Name}</td><td>{p.Price:C}</td><td>{p.Qty}</td><td>{(p.Price * p.Qty):C}</td></tr>"));

        var html = $@"
            <html>
            <head>
                <style>
                    table {{ border-collapse: collapse; width: 100%; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                    th {{ background-color: #4CAF50; color: white; }}
                    tr:nth-child(even) {{ background-color: #f2f2f2; }}
                </style>
            </head>
            <body>
                <table>
                    <thead>
                        <tr>
                            <th>Product</th>
                            <th>Price</th>
                            <th>Qty</th>
                            <th>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("products.pdf");
    }
}
```

### Example 3: Merge Multiple PDFs

**Before (GemBox.Pdf):**
```csharp
using GemBox.Pdf;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        using (var document = new PdfDocument())
        {
            // Load each source and clone pages
            using (var source1 = PdfDocument.Load("chapter1.pdf"))
            using (var source2 = PdfDocument.Load("chapter2.pdf"))
            using (var source3 = PdfDocument.Load("chapter3.pdf"))
            {
                document.Pages.AddClone(source1.Pages);
                document.Pages.AddClone(source2.Pages);
                document.Pages.AddClone(source3.Pages);
            }

            document.Save("merged.pdf");
        }
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdfs = new List<PdfDocument>
        {
            PdfDocument.FromFile("chapter1.pdf"),
            PdfDocument.FromFile("chapter2.pdf"),
            PdfDocument.FromFile("chapter3.pdf")
        };

        var merged = PdfDocument.Merge(pdfs);
        merged.SaveAs("merged.pdf");

        // Cleanup
        foreach (var pdf in pdfs)
        {
            pdf.Dispose();
        }
    }
}
```

### Example 4: Extract Text from PDF

**Before (GemBox.Pdf):**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content;
using System;
using System.Text;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");

        using (var document = PdfDocument.Load("document.pdf"))
        {
            var allText = new StringBuilder();

            foreach (var page in document.Pages)
            {
                var textContent = page.Content.GetText();
                allText.AppendLine(textContent.ToString());
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");

        // Extract all text at once
        string allText = pdf.ExtractAllText();
        Console.WriteLine(allText);

        // Or page by page
        for (int i = 0; i < pdf.PageCount; i++)
        {
            string pageText = pdf.ExtractTextFromPage(i);
            Console.WriteLine($"--- Page {i + 1} ---\n{pageText}");
        }
    }
}
```

### Example 5: Password Protection

**Before (GemBox.Pdf):**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Security;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("YOUR-LICENSE-KEY");

        using (var document = PdfDocument.Load("document.pdf"))
        {
            // Configure encryption
            var encryption = document.SaveOptions.SetPasswordEncryption();
            encryption.DocumentOpenPassword = "user123";
            encryption.PermissionsPassword = "owner456";
            encryption.Permissions = PdfUserAccessPermissions.None |
                                     PdfUserAccessPermissions.Print;
            encryption.EncryptionLevel = PdfEncryptionLevel.AES_256;

            document.Save("protected.pdf");
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

        var pdf = PdfDocument.FromFile("document.pdf");

        // Configure security
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

        pdf.SaveAs("protected.pdf");
    }
}
```

### Example 6: Add Watermark

**Before (GemBox.Pdf):**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("YOUR-LICENSE-KEY");

        using (var document = PdfDocument.Load("document.pdf"))
        {
            foreach (var page in document.Pages)
            {
                var text = new PdfFormattedText();
                text.Append("CONFIDENTIAL");
                text.FontSize = 48;
                text.Color = PdfColor.FromRgb(255, 0, 0);

                // Calculate center position manually
                var pageSize = page.Size;
                double x = pageSize.Width / 2 - 100;  // Approximate
                double y = pageSize.Height / 2;

                // GemBox requires manual rotation handling
                page.Content.DrawText(text, new PdfPoint(x, y));
            }

            document.Save("watermarked.pdf");
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

        var pdf = PdfDocument.FromFile("document.pdf");

        // Simple HTML watermark with rotation
        pdf.ApplyWatermark(
            "<div style='color:red; font-size:48px; transform:rotate(-45deg); opacity:0.3;'>" +
            "CONFIDENTIAL</div>",
            opacity: 30,
            VerticalAlignment.Middle,
            HorizontalAlignment.Center
        );

        pdf.SaveAs("watermarked.pdf");
    }
}
```

### Example 7: Headers and Footers

**Before (GemBox.Pdf) - Manual positioning on each page:**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("YOUR-LICENSE-KEY");

        using (var document = PdfDocument.Load("document.pdf"))
        {
            int pageNumber = 1;
            int totalPages = document.Pages.Count;

            foreach (var page in document.Pages)
            {
                var pageSize = page.Size;

                // Header
                var header = new PdfFormattedText { Text = "Company Report", FontSize = 10 };
                page.Content.DrawText(header, new PdfPoint(50, pageSize.Height - 30));

                // Footer with page numbers
                var footer = new PdfFormattedText
                {
                    Text = $"Page {pageNumber} of {totalPages}",
                    FontSize = 10
                };
                page.Content.DrawText(footer, new PdfPoint(pageSize.Width - 100, 20));

                pageNumber++;
            }

            document.Save("with-headers.pdf");
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

        // Configure headers and footers with HTML
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:center; font-size:10px;'>Company Report</div>",
            DrawDividerLine = true
        };

        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:right; font-size:10px;'>Page {page} of {total-pages}</div>",
            DrawDividerLine = true
        };

        var pdf = renderer.RenderHtmlAsPdf("<h1>Report Content</h1><p>...</p>");
        pdf.SaveAs("with-headers.pdf");

        // Or add to existing PDF
        var existingPdf = PdfDocument.FromFile("document.pdf");
        existingPdf.AddHtmlFooters(new HtmlHeaderFooter()
        {
            HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
        });
        existingPdf.SaveAs("existing-with-footers.pdf");
    }
}
```

### Example 8: Form Field Manipulation

**Before (GemBox.Pdf):**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Forms;
using System;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("YOUR-LICENSE-KEY");

        using (var document = PdfDocument.Load("form.pdf"))
        {
            foreach (var field in document.Form.Fields)
            {
                Console.WriteLine($"Field: {field.Name}, Type: {field.FieldType}");

                if (field.Name == "FirstName" && field is PdfTextField textField)
                {
                    textField.Value = "John";
                }
                else if (field.Name == "LastName" && field is PdfTextField lastField)
                {
                    lastField.Value = "Doe";
                }
            }

            document.Save("filled-form.pdf");
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("form.pdf");

        // Iterate through fields
        foreach (var field in pdf.Form.Fields)
        {
            Console.WriteLine($"Field: {field.Name}, Type: {field.Type}");
        }

        // Fill by name directly
        pdf.Form.Fields["FirstName"].Value = "John";
        pdf.Form.Fields["LastName"].Value = "Doe";

        pdf.SaveAs("filled-form.pdf");
    }
}
```

### Example 9: Split PDF into Pages

**Before (GemBox.Pdf):**
```csharp
using GemBox.Pdf;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("YOUR-LICENSE-KEY");

        using (var source = PdfDocument.Load("multipage.pdf"))
        {
            for (int i = 0; i < source.Pages.Count; i++)
            {
                using (var newDoc = new PdfDocument())
                {
                    newDoc.Pages.AddClone(source.Pages[i]);
                    newDoc.Save($"page_{i + 1}.pdf");
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
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("multipage.pdf");

        for (int i = 0; i < pdf.PageCount; i++)
        {
            var singlePage = pdf.CopyPage(i);
            singlePage.SaveAs($"page_{i + 1}.pdf");
        }
    }
}
```

### Example 10: URL to PDF

**GemBox.Pdf - Not Supported!**
```csharp
// GemBox.Pdf does not support URL-to-PDF conversion
// You would need to:
// 1. Download HTML using HttpClient
// 2. Parse and manually reconstruct as PdfFormattedText elements
// 3. Handle CSS, images, etc. manually
// This is extremely complex and error-prone
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
        renderer.RenderingOptions.EnableJavaScript = true;
        renderer.RenderingOptions.RenderDelay = 500;  // Wait for JS

        var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

---

## Advanced Scenarios

### Dynamic Report Generation

**Before (GemBox.Pdf) - Complex coordinate calculations:**
```csharp
// Extremely tedious - must calculate every Y position
// Risk of elements overlapping or going off page
// No automatic page breaks
```

**After (IronPDF) - Natural HTML flow:**
```csharp
using IronPdf;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var data = GetReportData();  // Your data

        var html = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial; margin: 40px; }}
                    .header {{ background: #333; color: white; padding: 20px; }}
                    .summary {{ display: flex; gap: 20px; margin: 20px 0; }}
                    .card {{ border: 1px solid #ddd; padding: 15px; flex: 1; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; }}
                    th {{ background: #f5f5f5; }}
                    @media print {{
                        .page-break {{ page-break-before: always; }}
                    }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Monthly Report</h1>
                    <p>Generated: {DateTime.Now:yyyy-MM-dd}</p>
                </div>

                <div class='summary'>
                    <div class='card'>Total Sales: ${data.TotalSales:N2}</div>
                    <div class='card'>Orders: {data.OrderCount}</div>
                    <div class='card'>Customers: {data.CustomerCount}</div>
                </div>

                <table>
                    <thead>
                        <tr><th>Product</th><th>Units</th><th>Revenue</th></tr>
                    </thead>
                    <tbody>
                        {string.Join("", data.Products.Select(p =>
                            $"<tr><td>{p.Name}</td><td>{p.Units}</td><td>${p.Revenue:N2}</td></tr>"))}
                    </tbody>
                </table>
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report.pdf");
    }
}
```

### Batch Processing

```csharp
using IronPdf;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var htmlFiles = Directory.GetFiles("templates", "*.html");
        var renderer = new ChromePdfRenderer();

        // Process in parallel - IronPDF is thread-safe
        await Parallel.ForEachAsync(htmlFiles, async (file, ct) =>
        {
            var html = await File.ReadAllTextAsync(file, ct);
            var pdf = renderer.RenderHtmlAsPdf(html);
            var outputPath = Path.ChangeExtension(file, ".pdf");
            pdf.SaveAs(outputPath);
        });
    }
}
```

---

## Performance Considerations

### Memory Management

**GemBox.Pdf:**
```csharp
using (var document = new PdfDocument())
{
    // ...
}  // Dispose called
```

**IronPDF:**
```csharp
using var pdf = PdfDocument.FromFile("document.pdf");
// Automatically disposed at end of scope
```

### Renderer Reuse

```csharp
// Good - reuse renderer for multiple conversions
var renderer = new ChromePdfRenderer();

foreach (var html in htmlDocuments)
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{count++}.pdf");
}
```

### Disable Unnecessary Features

```csharp
var renderer = new ChromePdfRenderer();

// Speed up rendering by disabling unused features
renderer.RenderingOptions.EnableJavaScript = false;
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
renderer.RenderingOptions.Timeout = 30;
```

---

## Troubleshooting

### Issue 1: Missing Coordinate-Based API

**Problem:** Need precise element placement like GemBox.Pdf's `DrawText(text, point)`.

**Solution:** Use CSS absolute positioning:
```csharp
var html = @"
    <div style='position:absolute; left:100px; top:700px; font-size:24px;'>
        Precisely positioned text
    </div>";
```

### Issue 2: 20-Paragraph Limit Errors in Old Code

**Problem:** Migrated code no longer has paragraph limits.

**Solution:** This is fixed automatically! IronPDF has no content limits.

### Issue 3: PdfPoint Not Found

**Problem:** `PdfPoint` class doesn't exist in IronPDF.

**Solution:** Use CSS positioning instead:
```csharp
// GemBox: new PdfPoint(100, 700)
// IronPDF: style='position:absolute; left:100pt; top:700pt;'

// Or let HTML flow naturally without coordinates
```

### Issue 4: PdfFormattedText Not Found

**Problem:** `PdfFormattedText` class doesn't exist in IronPDF.

**Solution:** Use HTML with CSS styling:
```csharp
// GemBox
var text = new PdfFormattedText { Text = "Hello", FontSize = 24 };

// IronPDF
var html = "<span style='font-size:24px;'>Hello</span>";
```

### Issue 5: ComponentInfo.SetLicense Not Found

**Problem:** `ComponentInfo` class doesn't exist in IronPDF.

**Solution:** Use `IronPdf.License.LicenseKey`:
```csharp
// GemBox
ComponentInfo.SetLicense("FREE-LIMITED-KEY");

// IronPDF
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Issue 6: Save Method Not Found

**Problem:** `document.Save()` method doesn't exist.

**Solution:** Use `SaveAs()`:
```csharp
// GemBox
document.Save("output.pdf");

// IronPDF
pdf.SaveAs("output.pdf");
```

### Issue 7: Document Loading Differences

**Problem:** `PdfDocument.Load()` not found.

**Solution:** Use `PdfDocument.FromFile()` or `FromStream()`:
```csharp
// GemBox
var doc = PdfDocument.Load("input.pdf");

// IronPDF
var pdf = PdfDocument.FromFile("input.pdf");
```

### Issue 8: Page Content Differences

**Problem:** `page.Content.DrawText()` not available.

**Solution:** Create content via HTML rendering:
```csharp
// For new documents - render HTML
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Content</h1>");

// For existing documents - use stampers
var stamper = new TextStamper() { Text = "Added Text" };
pdf.ApplyStamp(stamper);
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all GemBox.Pdf usage in codebase**
  ```bash
  grep -r "using GemBox.Pdf" --include="*.cs" .
  grep -r "ComponentInfo\|PdfDocument" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Identify coordinate-based layouts that need HTML conversion**
  ```csharp
  // Before (GemBox.Pdf)
  var text = new PdfFormattedText();
  text.Append("Hello, world!", new PdfFont("Arial", 12), PdfBrushes.Black);

  // After (IronPDF)
  var html = "<p style='font-family: Arial; font-size: 12px;'>Hello, world!</p>";
  ```
  **Why:** HTML/CSS provides a more flexible and intuitive layout system.

- [ ] **Evaluate current paragraph limits affecting your code**
  **Why:** IronPDF does not have paragraph limits, allowing for more complex documents without restrictions.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Create migration branch in version control**
  **Why:** Isolate migration work to avoid disrupting main development.

- [ ] **Set up test environment**
  **Why:** Ensure you can test changes without affecting production systems.

### Code Migration

- [ ] **Remove GemBox.Pdf NuGet package**
  ```bash
  dotnet remove package GemBox.Pdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project for PDF operations.

- [ ] **Update namespace imports**
  ```csharp
  // Before (GemBox.Pdf)
  using GemBox.Pdf;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure code references the correct library.

- [ ] **Replace `ComponentInfo.SetLicense()` with `IronPdf.License.LicenseKey`**
  ```csharp
  // Before (GemBox.Pdf)
  ComponentInfo.SetLicense("YOUR-LICENSE-KEY");

  // After (IronPDF)
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Convert `PdfDocument.Load()` to `PdfDocument.FromFile()`**
  ```csharp
  // Before (GemBox.Pdf)
  var document = PdfDocument.Load("input.pdf");

  // After (IronPDF)
  var document = PdfDocument.FromFile("input.pdf");
  ```
  **Why:** IronPDF uses `FromFile()` for loading existing PDFs.

- [ ] **Convert `document.Save()` to `pdf.SaveAs()`**
  ```csharp
  // Before (GemBox.Pdf)
  document.Save("output.pdf");

  // After (IronPDF)
  document.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses `SaveAs()` for saving PDFs.

- [ ] **Replace coordinate-based text with HTML content**
  ```csharp
  // Before (GemBox.Pdf)
  var text = new PdfFormattedText();
  text.Append("Hello, world!", new PdfFont("Arial", 12), PdfBrushes.Black);

  // After (IronPDF)
  var html = "<p style='font-family: Arial; font-size: 12px;'>Hello, world!</p>";
  var pdf = new ChromePdfRenderer().RenderHtmlAsPdf(html);
  ```
  **Why:** HTML/CSS provides a more flexible and intuitive layout system.

- [ ] **Convert `PdfFormattedText` to HTML with CSS styling**
  ```csharp
  // Before (GemBox.Pdf)
  var text = new PdfFormattedText();
  text.Append("Styled Text", new PdfFont("Arial", 16), PdfBrushes.Blue);

  // After (IronPDF)
  var html = "<p style='font-family: Arial; font-size: 16px; color: blue;'>Styled Text</p>";
  ```
  **Why:** HTML/CSS allows for more complex styling and layout options.

- [ ] **Update merge operations to use `PdfDocument.Merge()`**
  ```csharp
  // Before (GemBox.Pdf)
  var mergedDocument = new PdfDocument();
  mergedDocument.Pages.AddClone(document1.Pages[0]);
  mergedDocument.Pages.AddClone(document2.Pages[0]);

  // After (IronPDF)
  var mergedDocument = PdfDocument.Merge(document1, document2);
  ```
  **Why:** IronPDF provides a simpler API for merging PDFs.

- [ ] **Convert security settings to `SecuritySettings` properties**
  ```csharp
  // Before (GemBox.Pdf)
  document.Security.UserPassword = "password";

  // After (IronPDF)
  document.SecuritySettings.UserPassword = "password";
  ```
  **Why:** IronPDF uses `SecuritySettings` for managing PDF security.

### Testing

- [ ] **Verify all documents generate correctly**
  **Why:** Ensure that the migration did not introduce any errors.

- [ ] **Validate document appearance matches expectations**
  **Why:** IronPDF's rendering engine may produce different results; verify key documents.

- [ ] **Test table generation (previously limited by 20-paragraph rule)**
  **Why:** Ensure tables render correctly without paragraph limits.

- [ ] **Verify text extraction works correctly**
  **Why:** Ensure that text can be extracted as expected from generated PDFs.

- [ ] **Test merge and split operations**
  **Why:** Ensure that PDF documents can be combined and divided as needed.

- [ ] **Validate security/encryption functionality**
  **Why:** Ensure that password protection and permissions are correctly applied.

- [ ] **Test form field operations**
  **Why:** Ensure that form fields are correctly handled in PDFs.

- [ ] **Performance benchmark critical paths**
  **Why:** Ensure that the migration does not negatively impact performance.

### Post-Migration

- [ ] **Remove GemBox.Pdf license keys**
  **Why:** Clean up any old license keys that are no longer needed.

- [ ] **Update documentation**
  **Why:** Ensure that all documentation reflects the new PDF library usage.

- [ ] **Train team on HTML/CSS approach for PDFs**
  **Why:** Ensure team members understand the new workflow and capabilities.

- [ ] **Monitor production for any issues**
  **Why:** Quickly identify and resolve any issues that arise post-migration.

- [ ] **Enjoy unlimited content without paragraph limits!**
  **Why:** IronPDF allows for more complex and feature-rich PDFs without restrictions.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [IronPDF Code Examples](https://ironpdf.com/examples/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [CSS for Print Media](https://developer.mozilla.org/en-US/docs/Web/CSS/@media)

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
