# Migration Guide: PDFView4NET to IronPDF

## Why Migrate to IronPDF

IronPDF is a comprehensive PDF generation and manipulation library designed for production environments, unlike PDFView4NET which is primarily a UI viewing component. While PDFView4NET requires WinForms/WPF context for displaying PDFs, IronPDF focuses on creating, editing, and converting PDFs programmatically across all .NET platforms including web applications, services, and console apps. This makes IronPDF ideal for server-side PDF processing, automated document generation, and headless environments where UI components are not needed.

## NuGet Package Changes

```xml
<!-- Remove PDFView4NET -->
<PackageReference Include="O2S.Components.PDFView4NET" Version="*" Remove />

<!-- Add IronPDF -->
<PackageReference Include="IronPdf" Version="2024.*" />
```

## Namespace Mapping

| PDFView4NET | IronPDF |
|-------------|---------|
| `O2S.Components.PDFView4NET` | `IronPdf` |
| `O2S.Components.PDFView4NET.PDFFile` | `IronPdf.PdfDocument` |
| `O2S.Components.PDFView4NET.Printing` | `IronPdf.Printing` |
| `O2S.Components.PDFView4NET.Forms` | `IronPdf.Forms` |
| N/A (viewing only) | `IronPdf.Rendering` |

## API Mapping

| PDFView4NET API | IronPDF API | Notes |
|-----------------|-------------|-------|
| `PDFFile.Open()` | `PdfDocument.FromFile()` | Load existing PDF |
| `PdfViewer.Document` | `PdfDocument` object | Document handling |
| `PDFPage` | `PdfDocument.Pages[index]` | Page access |
| `PDFPrintDocument` | `PdfDocument.Print()` | Printing functionality |
| N/A | `ChromePdfRenderer.RenderHtmlAsPdf()` | HTML to PDF conversion |
| N/A | `PdfDocument.Merge()` | Combine PDFs |
| `PDFFixedFormField` | `PdfDocument.Form.Fields` | Form field access |
| N/A | `PdfDocument.SaveAs()` | Save document |
| `PDFFile.Close()` | `PdfDocument.Dispose()` | Resource cleanup |
| N/A | `ImageToPdfConverter.ImageToPdf()` | Image conversion |

## Code Examples

### Example 1: Loading and Saving a PDF

**Before (PDFView4NET):**
```csharp
using O2S.Components.PDFView4NET;

// PDFView4NET is for viewing only
PDFFile pdfFile = PDFFile.Open("input.pdf");
pdfViewer.Document = pdfFile;
// No direct save/manipulation capability
```

**After (IronPDF):**
```csharp
using IronPdf;

// Load, manipulate, and save PDF
PdfDocument pdf = PdfDocument.FromFile("input.pdf");
// Perform operations
pdf.SaveAs("output.pdf");
```

### Example 2: Creating a PDF from HTML

**Before (PDFView4NET):**
```csharp
// Not supported - PDFView4NET is a viewer only
// Would need a separate library for PDF creation
```

**After (IronPDF):**
```csharp
using IronPdf;

ChromePdfRenderer renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>PDF content here</p>");
pdf.SaveAs("created.pdf");
```

### Example 3: Printing a PDF

**Before (PDFView4NET):**
```csharp
using O2S.Components.PDFView4NET;
using O2S.Components.PDFView4NET.Printing;

PDFFile pdfFile = PDFFile.Open("document.pdf");
PDFPrintDocument printDoc = new PDFPrintDocument(pdfFile);
printDoc.Print();
```

**After (IronPDF):**
```csharp
using IronPdf;

PdfDocument pdf = PdfDocument.FromFile("document.pdf");
pdf.Print();
// Or with options
pdf.Print(new PrintOptions 
{ 
    PrinterName = "Microsoft Print to PDF",
    NumberOfCopies = 2
});
```

## Common Gotchas

| Issue | Solution |
|-------|----------|
| **No UI component** | IronPDF has no built-in viewer. Use third-party PDF viewers or export to browser for display. |
| **Different licensing model** | IronPDF requires a license key for production. Set with `IronPdf.License.LicenseKey = "KEY";` |
| **Thread safety** | Create separate `PdfDocument` instances per thread or use locking mechanisms. |
| **Memory management** | Always dispose `PdfDocument` objects using `using` statements or explicit `.Dispose()` calls. |
| **Rendering engine differences** | IronPDF uses Chromium for HTML rendering, which may produce different output than expected. Test thoroughly. |
| **Font embedding** | Fonts must be available on the server. Install required fonts or embed them explicitly. |
| **Path handling** | Use absolute paths or ensure working directory is correct when loading files. |
| **WinForms dependency removed** | No `System.Windows.Forms` reference needed - IronPDF works in console, web, and service apps. |

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Code Examples**: https://ironpdf.com/examples/

---

**Note**: PDFView4NET and IronPDF serve fundamentally different purposes. If you need PDF *viewing* functionality in a desktop application, you'll need to integrate a separate PDF viewer component alongside IronPDF for creation/manipulation tasks.