# Migration Guide: GemBox.Pdf → IronPDF

## Why Migrate to IronPDF

IronPDF eliminates the restrictive 20-paragraph limit that makes GemBox.Pdf's free version unusable for real-world applications, especially when working with tables where each cell counts toward this limit. With native HTML-to-PDF rendering using Chrome engine, IronPDF allows you to create complex PDFs from HTML/CSS rather than constructing documents programmatically. IronPDF also provides a more comprehensive feature set including form filling, digital signatures, and advanced text extraction capabilities.

## NuGet Package Changes

```bash
# Remove GemBox.Pdf
dotnet remove package GemBox.Pdf

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| GemBox.Pdf | IronPDF |
|------------|---------|
| `GemBox.Pdf` | `IronPdf` |
| `GemBox.Pdf.Content` | `IronPdf.Rendering` |
| `GemBox.Pdf.Forms` | `IronPdf.Forms` |
| `GemBox.Pdf.Security` | `IronPdf.Editing` |

## API Mapping

| GemBox.Pdf | IronPDF | Notes |
|------------|---------|-------|
| `PdfDocument` | `PdfDocument` | Core document class |
| `PdfDocument.Load()` | `PdfDocument.FromFile()` | Load existing PDF |
| `PdfDocument.Save()` | `PdfDocument.SaveAs()` | Save document |
| `PdfPage` | `PdfPage` | Page representation |
| `PdfPage.Content.Elements.AddText()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Use HTML instead |
| `PdfLoadOptions` | `ChromePdfRenderOptions` | Rendering configuration |
| `PdfSaveOptions` | `PdfPrintOptions` | Output configuration |
| `PdfFormField` | `FormField` | Form field manipulation |
| `TextContent.ToString()` | `PdfDocument.ExtractAllText()` | Text extraction |

## Code Examples

### Example 1: Creating a PDF Document

**Before (GemBox.Pdf):**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content;

ComponentInfo.SetLicense("FREE-LIMITED-KEY");

using (var document = new PdfDocument())
{
    var page = document.Pages.Add();
    page.Content.Elements.AddText("Hello World!", 
        new PdfPoint(100, 700));
    
    document.Save("output.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World!</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: Loading and Extracting Text

**Before (GemBox.Pdf):**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content;

ComponentInfo.SetLicense("FREE-LIMITED-KEY");

using (var document = PdfDocument.Load("input.pdf"))
{
    var page = document.Pages[0];
    var text = page.Content.GetText().ToString();
    Console.WriteLine(text);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");
string text = pdf.ExtractAllText();
Console.WriteLine(text);
```

### Example 3: Creating Multi-Page Document with Tables

**Before (GemBox.Pdf):**
```csharp
using GemBox.Pdf;
using GemBox.Pdf.Content;

ComponentInfo.SetLicense("FREE-LIMITED-KEY");

using (var document = new PdfDocument())
{
    var page = document.Pages.Add();
    var elements = page.Content.Elements;
    
    // Manually position each cell (tedious)
    elements.AddText("Header 1", new PdfPoint(50, 750));
    elements.AddText("Header 2", new PdfPoint(200, 750));
    elements.AddText("Data 1", new PdfPoint(50, 730));
    elements.AddText("Data 2", new PdfPoint(200, 730));
    // Limited to 20 paragraphs in free version!
    
    document.Save("table.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var html = @"
<table border='1'>
    <thead>
        <tr>
            <th>Header 1</th>
            <th>Header 2</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Data 1</td>
            <td>Data 2</td>
        </tr>
        <!-- Add hundreds of rows without limitations -->
    </tbody>
</table>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("table.pdf");
```

## Common Gotchas

### 1. **HTML vs Programmatic Construction**
IronPDF uses HTML rendering rather than programmatic element placement. Instead of calculating coordinates, write HTML/CSS. This is more intuitive but requires thinking in web terms.

### 2. **License Configuration**
```csharp
// Set license before any IronPDF operations
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 3. **Renderer Instantiation**
Always create a `ChromePdfRenderer` instance for HTML-to-PDF conversion. Reuse it for multiple conversions for better performance.

### 4. **File Paths**
IronPDF's `FromFile()` and `SaveAs()` use different method names than GemBox's `Load()` and `Save()`.

### 5. **Page Size Configuration**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
```

### 6. **Headers and Footers**
In IronPDF, headers/footers are set via rendering options with HTML templates:
```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<h3>Page Header</h3>"
};
```

### 7. **URL Rendering**
IronPDF can render URLs directly:
```csharp
var pdf = renderer.RenderUrlAsPdf("https://example.com");
```

## Additional Resources

- **Official Documentation:** https://ironpdf.com/docs/
- **Tutorials:** https://ironpdf.com/tutorials/
- **API Reference:** Full API documentation available in the docs section
- **Support:** Contact IronPDF support for migration assistance