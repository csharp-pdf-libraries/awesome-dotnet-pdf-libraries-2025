# Migration Guide: Aspose.PDF for .NET → IronPDF

## Why Migrate from Aspose.PDF to IronPDF

IronPDF offers a compelling alternative to Aspose.PDF with significantly lower costs (starting at $749 vs $1,199+ annually), modern Chromium-based HTML rendering that handles CSS3 and JavaScript perfectly, and superior performance without the documented slowdowns reported in Aspose.PDF forums. The API is more intuitive and .NET-focused, eliminating the legacy Java engine dependencies that plague Aspose.PDF's HTML-to-PDF conversion.

## NuGet Package Changes

```bash
# Remove Aspose.PDF
dotnet remove package Aspose.PDF

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Aspose.PDF | IronPDF |
|------------|---------|
| `Aspose.Pdf` | `IronPdf` |
| `Aspose.Pdf.Generator` | `IronPdf` |
| `Aspose.Pdf.Text` | `IronPdf` |
| `Aspose.Pdf.Facades` | `IronPdf` |

## API Mapping Table

| Aspose.PDF API | IronPDF API | Notes |
|----------------|-------------|-------|
| `new Document()` | `PdfDocument.FromFile()` | Load existing PDF |
| `HtmlLoadOptions` | `ChromePdfRenderer` | HTML to PDF conversion |
| `Document.Save()` | `PdfDocument.SaveAs()` | Save PDF to file |
| `PdfFileEditor.Concatenate()` | `PdfDocument.Merge()` | Merge PDFs |
| `TextFragmentAbsorber` | `PdfDocument.ExtractAllText()` | Extract text |
| `Document.Pages` | `PdfDocument.Pages` | Access pages |
| `PdfFileSignature` | `PdfDocument.Sign()` | Digital signatures |
| `ImageStamp` | `ChromePdfRenderer.RenderHtmlAsPdf()` with headers | Add watermarks/images |
| `Page.AddImage()` | Use HTML/CSS for images | Chromium rendering |

## Code Examples

### Example 1: HTML to PDF Conversion

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;

var options = new HtmlLoadOptions();
Document pdfDocument = new Document("input.html", options);
pdfDocument.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 2: Generate PDF from HTML String

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;
using System.IO;

string htmlContent = "<h1>Hello World</h1><p>This is a test.</p>";
Document doc = new Document(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(htmlContent)), 
    new HtmlLoadOptions());
doc.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

string htmlContent = "<h1>Hello World</h1><p>This is a test.</p>";
var renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

### Example 3: Merge Multiple PDFs

**Before (Aspose.PDF):**
```csharp
using Aspose.Pdf;
using Aspose.Pdf.Facades;

PdfFileEditor pdfEditor = new PdfFileEditor();
string[] inputFiles = { "file1.pdf", "file2.pdf", "file3.pdf" };
pdfEditor.Concatenate(inputFiles, "merged.pdf");
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

## Common Gotchas

### 1. **HTML Rendering Engine Differences**
- **Aspose.PDF** uses the Flying Saucer engine (Java-based, limited CSS support)
- **IronPDF** uses Chromium (full CSS3, JavaScript, modern web standards)
- **Fix:** Your HTML/CSS will likely render better in IronPDF without changes, but test thoroughly

### 2. **License Activation**
- **Aspose.PDF:** Uses `License.SetLicense()` with .lic file
- **IronPDF:** Set `IronPdf.License.LicenseKey = "YOUR-KEY"` once at startup

```csharp
// IronPDF licensing
IronPdf.License.LicenseKey = "IRONPDF-MYLICENSE-KEY-123456";
```

### 3. **Page Size and Margins**
- **Aspose.PDF:** Manual page size configuration via `PageInfo`
- **IronPDF:** CSS-based using `@page` rules or `RenderingOptions`

```csharp
// IronPDF page settings
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 40;
renderer.RenderingOptions.MarginBottom = 40;
```

### 4. **Text Extraction**
- **Aspose.PDF:** Requires `TextFragmentAbsorber` and loops
- **IronPDF:** Simple one-liner

```csharp
// IronPDF text extraction
var pdf = PdfDocument.FromFile("document.pdf");
string text = pdf.ExtractAllText();
```

### 5. **Asynchronous Operations**
- IronPDF supports async rendering for better performance:

```csharp
var renderer = new ChromePdfRenderer();
PdfDocument pdf = await renderer.RenderHtmlAsPdfAsync(htmlContent);
```

### 6. **Custom Headers and Footers**
- **Aspose.PDF:** Complex template system
- **IronPDF:** Simple HTML-based headers/footers

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:right'>{page} of {total-pages}</div>"
};
```

## Additional Resources

- **Official Documentation:** https://ironpdf.com/docs/
- **Tutorials & Examples:** https://ironpdf.com/tutorials/
- **API Reference:** https://ironpdf.com/object-reference/api/

---

**Need help with migration?** Contact IronPDF support or consult the comprehensive documentation at the links above.