# Migration Guide: Gnostice (Document Studio .NET, PDFOne) → IronPDF

## Why Migrate from Gnostice to IronPDF

Gnostice Document Studio and PDFOne suffer from extensive documented limitations including no external CSS, JavaScript, digital signatures, or right-to-left Unicode support, along with persistent memory leaks and stability issues reported across Stack Overflow and user forums. The platform is fragmented across separate products for different frameworks (.NET, Java, VCL) with inconsistent feature sets between WinForms, WPF, ASP.NET, and Xamarin implementations. IronPDF provides a unified, stable API with comprehensive HTML-to-PDF rendering, full CSS3/JavaScript support, digital signatures, and cross-platform .NET compatibility without the architectural fragmentation.

## NuGet Package Changes

```bash
# Remove Gnostice packages
dotnet remove package Gnostice.DocumentStudio.NET
dotnet remove package Gnostice.PDFOne.NET

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Gnostice | IronPDF |
|----------|---------|
| `Gnostice.Documents` | `IronPdf` |
| `Gnostice.Documents.PDF` | `IronPdf` |
| `Gnostice.PDFOne` | `IronPdf` |
| `Gnostice.Documents.Controls` | `IronPdf.AspNet` (for web controls) |
| `Gnostice.Documents.Spreadsheet` | `IronPdf` (HTML rendering) |
| `Gnostice.Documents.Word` | `IronPdf` (HTML rendering) |

## API Mapping Table

| Gnostice API | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `DocumentManager.LoadDocument()` | `PdfDocument.FromFile()` | Load existing PDF |
| `DocExporter.Export()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | HTML to PDF conversion |
| `PDFDocument.Create()` | `new ChromePdfRenderer()` | Create new PDF |
| `PDFDocument.Save()` | `PdfDocument.SaveAs()` | Save PDF to disk |
| `PDFDocument.AddPage()` | `PdfDocument.AppendPdf()` | Add pages |
| `PDFDocument.SetEncryption()` | `PdfDocument.Password()` | Set password protection |
| `PDFDocument.Merge()` | `PdfDocument.Merge()` | Merge multiple PDFs |
| `PDFTextExtractor.ExtractText()` | `PdfDocument.ExtractAllText()` | Extract text content |
| `PDFDocument.AddWatermark()` | `PdfDocument.ApplyWatermark()` | Add watermark |
| `PDFDocument.Sign()` | `PdfDocument.Sign()` | Digital signatures (IronPDF supports, Gnostice doesn't) |
| `PDFDocument.AddBookmark()` | `ChromePdfRenderer.RenderingOptions.CreatePdfFormsFromHtml` | Bookmarks/TOC generation |
| `DocViewer.LoadDocument()` | Render to HTML or use third-party viewer | IronPDF focuses on generation |

## Code Migration Examples

### Example 1: HTML to PDF Conversion

**Before (Gnostice):**
```csharp
using Gnostice.Documents;
using Gnostice.Documents.PDF;

// Limited HTML support, no external CSS or JavaScript
DocExporter exporter = new DocExporter();
exporter.Preferences.PDFExportPreferences.ConvertLinksToBookmarks = true;

Document doc = DocumentManager.LoadDocument("input.html");
exporter.Export(doc, "output.pdf", DocumentFormat.PDF);
doc.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;

// Full CSS3, JavaScript, and external resource support
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;

PdfDocument pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Custom Settings

**Before (Gnostice):**
```csharp
using Gnostice.Documents;
using Gnostice.Documents.PDF;

// No JavaScript execution, limited CSS support
DocExporter exporter = new DocExporter();
PDFExportPreferences pdfPrefs = exporter.Preferences.PDFExportPreferences;
pdfPrefs.PageSize = PDFPageSize.A4;
pdfPrefs.PageOrientation = PDFPageOrientation.Portrait;

Document doc = DocumentManager.LoadURL("https://example.com");
exporter.Export(doc, "webpage.pdf", DocumentFormat.PDF);
doc.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;

// Full JavaScript execution, dynamic content support
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Portrait;
renderer.RenderingOptions.WaitFor.RenderDelay(500); // Wait for JavaScript

PdfDocument pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Merge PDFs with Password Protection

**Before (Gnostice):**
```csharp
using Gnostice.PDFOne;

// Basic merge functionality, encryption not supported in all versions
PDFDocument pdf1 = new PDFDocument();
pdf1.Load("document1.pdf");

PDFDocument pdf2 = new PDFDocument();
pdf2.Load("document2.pdf");

pdf1.Append(pdf2);
// SetEncryption method often missing or limited
pdf1.Save("merged.pdf");

pdf1.Close();
pdf2.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;

// Robust merge with full encryption support
var pdfs = new List<PdfDocument>
{
    PdfDocument.FromFile("document1.pdf"),
    PdfDocument.FromFile("document2.pdf")
};

PdfDocument merged = PdfDocument.Merge(pdfs);
merged.Password = "secure_password"; // Set password protection
merged.SecuritySettings.AllowUserCopyPasteContent = false;
merged.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;

merged.SaveAs("merged.pdf");
```

## Common Migration Gotchas

### 1. HTML Rendering Differences
**Issue:** Gnostice has documented limitations with external CSS, JavaScript, and complex layouts. IronPDF uses Chromium rendering engine.

**Solution:** Your HTML will likely render MORE accurately with IronPDF. Test complex pages as previously broken features (CSS Grid, Flexbox, external stylesheets) now work correctly.

```csharp
// IronPDF handles external resources automatically
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true; // JavaScript works (unlike Gnostice)
renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Screen;
```

### 2. Document Viewer Components
**Issue:** Gnostice provides WinForms/WPF/ASP.NET viewer controls. IronPDF focuses on PDF generation.

**Solution:** Use IronPDF for generation and integrate a dedicated viewer library (PDF.js, PSPDFKit) for display, or render to embedded base64.

```csharp
// Generate PDF and embed in web page
var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1>");
string base64 = Convert.ToBase64String(pdf.BinaryData);
string embedHtml = $"<embed src='data:application/pdf;base64,{base64}' type='application/pdf' />";
```

### 3. Memory Management
**Issue:** Gnostice users report memory leaks requiring manual disposal. IronPDF has improved memory handling.

**Solution:** Still use `using` statements, but IronPDF handles cleanup more reliably.

```csharp
// Proper disposal pattern
using (var renderer = new ChromePdfRenderer())
{
    using (var pdf = renderer.RenderHtmlAsPdf("<h1>Content</h1>"))
    {
        pdf.SaveAs("output.pdf");
    }
} // Automatic cleanup, no memory leaks
```

### 4. Platform Targeting
**Issue:** Gnostice has separate assemblies for .NET Framework, .NET Core, different UI frameworks.

**Solution:** IronPDF has unified cross-platform support. Single package works across .NET Framework 4.6.2+ and .NET 6/7/8+.

```csharp
// Same code works on Windows, Linux, macOS, Azure, AWS, Docker
#if NET6_0_OR_GREATER
    IronPdf.Installation.ChromeGpuMode = IronPdf.Engines.Chrome.ChromeGpuModes.Disabled; // Linux headless
#endif
```

### 5. Digital Signatures
**Issue:** Gnostice documents explicitly state "NO digital signatures" support.

**Solution:** IronPDF fully supports digital signatures with X509 certificates.

```csharp
using IronPdf;
using IronPdf.Signing;
using System.Security.Cryptography.X509Certificates;

var pdf = PdfDocument.FromFile("document.pdf");
var cert = new X509Certificate2("certificate.pfx", "password");

var signature = new PdfSignature(cert)
{
    SigningContact = "admin@example.com",
    SigningLocation = "New York, USA",
    SigningReason = "Document Approval"
};

pdf.Sign(signature);
pdf.SaveAs("signed.pdf");
```

### 6. Right-to-Left Language Support
**Issue:** Gnostice explicitly lists "NO right-to-left Unicode" in limitations.

**Solution:** IronPDF fully supports RTL languages (Arabic, Hebrew) through Chromium engine.

```csharp
// RTL languages now work correctly
string arabicHtml = @"
<html>
<head>
    <style>body { direction: rtl; font-family: 'Arial'; }</style>
</head>
<body>
    <h1>مرحبا بك في IronPDF</h1>
    <p>دعم كامل للغة العربية والعبرية</p>
</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(arabicHtml);
pdf.SaveAs("arabic.pdf");
```

## Additional Resources

- **Official Documentation:** https://ironpdf.com/docs/
- **Tutorials and Examples:** https://ironpdf.com/tutorials/
- **API Reference:** https://ironpdf.com/docs/
- **Support:** Contact IronPDF support team for migration assistance

---

**Migration Checklist:**
- [ ] Replace NuGet packages
- [ ] Update namespace imports
- [ ] Convert HTML rendering calls to `ChromePdfRenderer`
- [ ] Update file loading to `PdfDocument.FromFile()`
- [ ] Test previously unsupported features (CSS, JavaScript, signatures)
- [ ] Remove workarounds for Gnostice limitations
- [ ] Update platform-specific code to unified IronPDF API
- [ ] Test memory usage (should improve significantly)