# Migration Guide: Adobe PDF Library SDK → IronPDF

## Why Migrate from Adobe PDF Library SDK to IronPDF

Adobe PDF Library SDK offers enterprise-grade PDF functionality but comes with prohibitive licensing costs that can reach tens of thousands of dollars annually, making it impractical for most projects. The native C++ SDK requires complex integration and platform-specific builds, adding significant development overhead. IronPDF provides equivalent PDF generation and manipulation capabilities with a simple .NET API, transparent pricing, and straightforward integration that gets you productive in minutes rather than weeks.

## NuGet Package Changes

```xml
<!-- Remove Adobe PDF Library SDK -->
<!-- Adobe PDF Library requires manual installation and licensing -->

<!-- Add IronPDF -->
<PackageReference Include="IronPdf" Version="2024.*" />
```

## Namespace Mapping

| Adobe PDF Library SDK | IronPDF |
|----------------------|---------|
| `Datalogics.PDFL` | `IronPdf` |
| `Datalogics.PDFL.Document` | `IronPdf.ChromePdfRenderer` |
| `Datalogics.PDFL.Page` | `IronPdf.PdfDocument` |
| `Datalogics.PDFL.Content` | `IronPdf.Editing` |
| `Datalogics.PDFL.Text` | `IronPdf.Editing.TextExtractor` |
| `Datalogics.PDFL.Image` | `IronPdf.Editing.ImageExtractor` |

## API Mapping Table

| Adobe PDF Library SDK | IronPDF | Notes |
|----------------------|---------|-------|
| `Library.Initialize()` | `License.LicenseKey = "KEY"` | Simpler initialization |
| `new Document(html)` | `ChromePdfRenderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| `document.CreatePage()` | `PdfDocument.FromHtml()` | Page creation |
| `document.Save()` | `pdf.SaveAs()` | Save PDF |
| `document.GetNumPages()` | `pdf.PageCount` | Get page count |
| `page.GetContent()` | `pdf.ExtractAllText()` | Extract text |
| `document.AddWatermark()` | `pdf.ApplyWatermark()` | Add watermarks |
| `document.Merge()` | `PdfDocument.Merge()` | Merge PDFs |
| `page.Rotate()` | `pdf.RotatePage()` | Rotate pages |
| `document.Encrypt()` | `pdf.Password = "pass"` | Encryption |

## Code Examples

### Example 1: HTML to PDF Conversion

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

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
            text.AddRun(new TextRun("Hello World", font, 12, new Point(100, 700)));
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

### Example 2: Merging PDF Documents

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

Library.Initialize();
try
{
    using (Document doc1 = new Document("file1.pdf"))
    using (Document doc2 = new Document("file2.pdf"))
    {
        doc1.InsertPages(Document.LastPage, doc2, 0, Document.AllPages, 
            PageInsertFlags.None);
        doc1.Save(SaveFlags.Full, "merged.pdf");
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

var pdf1 = PdfDocument.FromFile("file1.pdf");
var pdf2 = PdfDocument.FromFile("file2.pdf");
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

### Example 3: Adding Password Protection

**Before (Adobe PDF Library SDK):**
```csharp
using Datalogics.PDFL;

Library.Initialize();
try
{
    using (Document doc = new Document("input.pdf"))
    {
        EncryptionHandler encHandler = new EncryptionHandler(
            "userpass", "ownerpass", 
            PermissionFlags.PrintDoc | PermissionFlags.PrintFidelity);
        doc.SetEncryptionHandler(encHandler);
        doc.Save(SaveFlags.Full | SaveFlags.Encrypted, "protected.pdf");
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

var pdf = PdfDocument.FromFile("input.pdf");
pdf.Password = "userpass";
pdf.SecuritySettings.OwnerPassword = "ownerpass";
pdf.SecuritySettings.AllowUserPrinting = PrintPermission.FullPrintRights;
pdf.SaveAs("protected.pdf");
```

## Common Gotchas

1. **Library Initialization**: Adobe PDF Library requires explicit `Library.Initialize()` and `Library.Terminate()` calls. IronPDF handles initialization automatically—just set the license key once.

2. **Coordinate Systems**: Adobe uses PostScript points with origin at bottom-left. IronPDF uses HTML/CSS rendering with standard web coordinate systems, making layout more intuitive.

3. **Font Handling**: Adobe requires explicit font embedding and management. IronPDF automatically handles system fonts and web fonts without manual configuration.

4. **Memory Management**: Adobe's native SDK requires careful disposal of COM objects and unmanaged resources. IronPDF is fully managed .NET code with standard `IDisposable` patterns.

5. **Platform Dependencies**: Adobe PDF Library requires platform-specific native binaries. IronPDF is cross-platform and works on Windows, Linux, macOS, Docker, and Azure without additional configuration.

6. **HTML Rendering**: Adobe requires manual content construction using low-level APIs. IronPDF uses a full Chromium engine for HTML rendering, supporting modern CSS, JavaScript, and web standards.

7. **Licensing Model**: Adobe uses seat-based or processor-based licensing with annual maintenance. IronPDF uses simple per-developer or per-deployment licensing with perpetual options.

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **Tutorials & Examples**: https://ironpdf.com/tutorials/
- **API Reference**: Full IntelliSense support included in NuGet package