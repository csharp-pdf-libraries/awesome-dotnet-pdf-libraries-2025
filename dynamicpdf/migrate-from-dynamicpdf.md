# Migration Guide: DynamicPDF to IronPDF

## Why Migrate to IronPDF

IronPDF offers a unified, modern API that consolidates PDF generation, manipulation, and rendering into a single package, eliminating product fragmentation. With straightforward licensing, excellent documentation, and active development using current .NET patterns, IronPDF simplifies PDF workflows while providing superior HTML-to-PDF conversion capabilities. The library supports .NET Core, .NET 5+, and .NET Framework with consistent performance across platforms.

## NuGet Package Changes

```bash
# Remove DynamicPDF packages
dotnet remove package ceTe.DynamicPDF.CoreSuite.NET
dotnet remove package ceTe.DynamicPDF.Generator.NET
dotnet remove package ceTe.DynamicPDF.Merger.NET

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| DynamicPDF | IronPDF |
|------------|---------|
| `ceTe.DynamicPDF` | `IronPdf` |
| `ceTe.DynamicPDF.PageElements` | `IronPdf` (integrated) |
| `ceTe.DynamicPDF.Merger` | `IronPdf` |
| `ceTe.DynamicPDF.Text` | `IronPdf` (use HTML/CSS) |

## API Mapping

| DynamicPDF API | IronPDF API | Notes |
|----------------|-------------|-------|
| `Document` | `ChromePdfRenderer` or `PdfDocument` | Use renderer for HTML, PdfDocument for manipulation |
| `Page.Elements.Add()` | HTML/CSS rendering | IronPDF uses HTML for layout |
| `Label` | HTML `<p>`, `<span>`, `<div>` | Style with CSS |
| `TextArea` | HTML elements | Full CSS support |
| `Image` | `<img>` tag or `PdfDocument.AddImage()` | Support for URLs and embedded images |
| `MergeDocument` | `PdfDocument.Merge()` | Simpler merge API |
| `Document.Draw()` | `RenderHtmlAsPdf()` | HTML-based generation |
| `Document.DrawImage()` | Use HTML `<img>` | More flexible with CSS |

## Code Examples

### Example 1: Simple PDF Generation

**Before (DynamicPDF):**
```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements;

Document document = new Document();
Page page = new Page(PageSize.Letter);
Label label = new Label("Hello World!", 0, 0, 504, 100);
page.Elements.Add(label);
document.Pages.Add(page);
document.Draw("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World!</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: Merging PDFs

**Before (DynamicPDF):**
```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.Merger;

MergeDocument document = new MergeDocument("file1.pdf");
document.Append("file2.pdf");
document.Append("file3.pdf");
document.Draw("merged.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("file1.pdf");
var pdf2 = PdfDocument.FromFile("file2.pdf");
var pdf3 = PdfDocument.FromFile("file3.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("merged.pdf");
```

### Example 3: Complex Layout with Text and Images

**Before (DynamicPDF):**
```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements;

Document document = new Document();
Page page = new Page();

Label header = new Label("Invoice Report", 0, 0, 400, 30, Font.Helvetica, 18);
page.Elements.Add(header);

Image logo = new Image("logo.png", 0, 40, 100, 50);
page.Elements.Add(logo);

TextArea description = new TextArea("Invoice details...", 0, 100, 400, 200);
page.Elements.Add(description);

document.Pages.Add(page);
document.Draw("invoice.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

string html = @"
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        h1 { font-size: 18pt; margin: 0; }
        .logo { width: 100px; height: 50px; margin: 10px 0; }
        .description { margin-top: 20px; max-width: 400px; }
    </style>
</head>
<body>
    <h1>Invoice Report</h1>
    <img src='logo.png' class='logo' />
    <div class='description'>Invoice details...</div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

## Common Gotchas

### 1. **HTML-First Approach**
IronPDF uses HTML/CSS for layout instead of programmatic element positioning. This is more flexible but requires thinking in web technologies rather than coordinate-based positioning.

### 2. **Licensing Setup**
Set your license key before creating PDFs:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 3. **Rendering Settings**
Configure rendering options for better control:
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
```

### 4. **File Paths and Resources**
When referencing images or CSS files, use absolute paths or data URIs:
```csharp
// Absolute path
<img src='file:///C:/images/logo.png' />

// Or base URL
renderer.RenderingOptions.BaseUrl = new Uri("C:/myproject/");
```

### 5. **Async Operations**
IronPDF supports async methods for better performance:
```csharp
var pdf = await renderer.RenderHtmlAsPdfAsync("<h1>Hello</h1>");
```

### 6. **Font Handling**
Use standard web fonts or embed custom fonts via CSS:
```css
@font-face {
    font-family: 'CustomFont';
    src: url('fonts/custom.ttf');
}
```

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **Code Examples**: Comprehensive examples for HTML to PDF, URL to PDF, merging, splitting, and more available in the tutorials section