# Migration Guide: Scryber.Core → IronPDF

## Why Migrate to IronPDF?

IronPDF offers enterprise-grade commercial support, extensive documentation, and a larger community compared to Scryber.Core. The library provides more flexible licensing options without LGPL restrictions, making it ideal for commercial applications. IronPDF also delivers superior HTML-to-PDF rendering capabilities with modern CSS3 and JavaScript support.

## NuGet Package Changes

```bash
# Remove Scryber.Core
dotnet remove package Scryber.Core

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Scryber.Core | IronPDF |
|--------------|---------|
| `Scryber.Components` | `IronPdf` |
| `Scryber.Components.Pdf` | `IronPdf` |
| `Scryber.PDF` | `IronPdf` |
| `Scryber.PDF.Layout` | `IronPdf` |
| `Scryber.Styles` | CSS styles in HTML |

## API Mapping

| Scryber.Core API | IronPDF API |
|------------------|-------------|
| `Document.ParseDocument()` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `Document.SaveAsPDF()` | `PdfDocument.SaveAs()` |
| `PDFLayoutDocument.LayoutComplete` | N/A (automatic) |
| `PDFDocument.GenerationData` | `PdfDocument.MetaData` |
| `PDFPage` | `PdfDocument.CopyPage()` / `PdfDocument.CopyPages()` |
| `PDFTextReader` | `PdfDocument.ExtractAllText()` |
| `Document.AppendTraceLog()` | Standard .NET logging |
| `PDFStyle` | CSS in HTML string |

## Code Examples

### Example 1: Basic HTML to PDF

**Before (Scryber.Core):**
```csharp
using Scryber.Components;
using Scryber.Components.Pdf;

var html = "<html><body><h1>Hello World</h1></body></html>";
using (var doc = Document.ParseDocument(html))
{
    doc.SaveAsPDF("output.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var html = "<html><body><h1>Hello World</h1></body></html>";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF

**Before (Scryber.Core):**
```csharp
using Scryber.Components;
using System.Net.Http;

var client = new HttpClient();
var html = await client.GetStringAsync("https://example.com");
using (var doc = Document.ParseDocument(html))
{
    doc.SaveAsPDF("webpage.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: PDF with Metadata

**Before (Scryber.Core):**
```csharp
using Scryber.Components;
using Scryber.PDF;

var html = "<html><body><p>Document with metadata</p></body></html>";
using (var doc = Document.ParseDocument(html))
{
    doc.Info.Title = "My Document";
    doc.Info.Author = "John Doe";
    doc.SaveAsPDF("metadata.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var html = "<html><body><p>Document with metadata</p></body></html>";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.MetaData.Title = "My Document";
pdf.MetaData.Author = "John Doe";
pdf.SaveAs("metadata.pdf");
```

## Common Gotchas

### 1. **Styling Approach**
- **Scryber.Core**: Uses programmatic styles or embedded style definitions
- **IronPDF**: Relies on standard CSS in HTML; ensure all styles are included in your HTML string or linked stylesheets

### 2. **Page Layout Control**
- **Scryber.Core**: Uses `PDFLayoutDocument` for layout calculations
- **IronPDF**: Use `ChromePdfRenderOptions` to control margins, orientation, and paper size:
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 40;
```

### 3. **Template Processing**
- **Scryber.Core**: Built-in template engine with data binding
- **IronPDF**: Use your preferred template engine (Razor, Handlebars, etc.) to generate HTML before rendering

### 4. **Document Assembly**
- **Scryber.Core**: Build documents programmatically with components
- **IronPDF**: Generate complete HTML first, or merge multiple PDFs:
```csharp
var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
```

### 5. **Licensing**
- **Scryber.Core**: LGPL requires source disclosure in some scenarios
- **IronPDF**: Commercial license required for production use; free for development

### 6. **Asynchronous Operations**
- **Scryber.Core**: Primarily synchronous API
- **IronPDF**: Supports async methods:
```csharp
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### 7. **Font Handling**
- **Scryber.Core**: May require explicit font registration
- **IronPDF**: Automatically uses system fonts; custom fonts can be embedded via CSS `@font-face`

## Additional Resources

- **Official Documentation**: https://ironpdf.com/docs/
- **Tutorials & Examples**: https://ironpdf.com/tutorials/
- **API Reference**: Available in the documentation link above