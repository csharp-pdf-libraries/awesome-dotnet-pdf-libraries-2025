# How Do I Migrate from PDF Duo .NET to IronPDF in C#?

## Why Migrate?

PDF Duo .NET is an obscure, poorly documented library with unclear maintenance status. Migrating to IronPDF provides a stable, well-documented, and actively maintained solution for PDF generation.

### Critical Issues with PDF Duo .NET

1. **Unclear Provenance**: Unknown developer, unclear company backing
   - No visible GitHub repository or source code
   - Limited NuGet download statistics
   - Uncertain licensing terms

2. **Missing Documentation**: Nearly impossible to find reliable information
   - No official API reference
   - Sparse community examples
   - No official tutorials or guides

3. **Abandoned or Inactive**: Signs of neglect
   - Sporadic or no updates
   - Support forums inactive (posts from 2019)
   - No response to issues or questions

4. **Limited Features**: Basic functionality only
   - Simple HTML to PDF
   - Basic PDF merging
   - No advanced features (forms, security, watermarks)

5. **Unknown Rendering Engine**: Unclear what's under the hood
   - CSS/JavaScript support unknown
   - Rendering quality unpredictable
   - Modern web features uncertain

6. **Support Risk**: No recourse when things break
   - No professional support
   - No community to help
   - Risk of complete abandonment

### Benefits of IronPDF

| Aspect | PDF Duo .NET | IronPDF |
|--------|-------------|---------|
| Maintenance | Unknown/Inactive | Active, regular updates |
| Documentation | Sparse/Missing | Comprehensive |
| Support | None | Professional support team |
| Community | ~0 users | 41M+ NuGet downloads |
| Rendering | Unknown engine | Modern Chromium |
| Features | Basic | Full-featured |
| Stability | Unknown | Production-proven |
| Licensing | Unclear | Transparent |

The [migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-pdf-duo-to-ironpdf/) addresses the specific challenges of migrating from obscure libraries to well-documented solutions.

---

## NuGet Package Changes

```bash
# Remove PDF Duo .NET (if you can find the correct package name)
dotnet remove package PDFDuo.NET
dotnet remove package PDFDuo
dotnet remove package PDF-Duo

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

| PDF Duo .NET | IronPDF |
|--------------|---------|
| `using PDFDuo;` | `using IronPdf;` |
| `using PDFDuo.Document;` | `using IronPdf;` |
| `using PDFDuo.Rendering;` | `using IronPdf.Rendering;` |
| `using PDFDuo.Settings;` | `using IronPdf;` |

---

## Complete API Mapping

### HTML to PDF Conversion

| PDF Duo .NET | IronPDF | Notes |
|--------------|---------|-------|
| `new HtmlToPdfConverter()` | `new ChromePdfRenderer()` | Main renderer |
| `converter.ConvertHtmlString(html, path)` | `renderer.RenderHtmlAsPdf(html).SaveAs(path)` | HTML string |
| `converter.ConvertUrl(url, path)` | `renderer.RenderUrlAsPdf(url).SaveAs(path)` | URL conversion |
| `converter.ConvertFile(htmlPath, pdfPath)` | `renderer.RenderHtmlFileAsPdf(htmlPath).SaveAs(pdfPath)` | HTML file |
| `PDFConverter.FromHtml(html)` | `renderer.RenderHtmlAsPdf(html)` | Static method |
| `PDFConverter.FromUrl(url)` | `renderer.RenderUrlAsPdf(url)` | Static method |

### Page Configuration

| PDF Duo .NET | IronPDF | Notes |
|--------------|---------|-------|
| `settings.PageSize = PageSize.A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Paper size |
| `settings.PageSize = PageSize.Letter` | `RenderingOptions.PaperSize = PdfPaperSize.Letter` | US Letter |
| `settings.Orientation = Landscape` | `RenderingOptions.PaperOrientation = Landscape` | Orientation |
| `settings.Margins = new Margins(t, r, b, l)` | Individual margin properties | See below |

### Margins

| PDF Duo .NET | IronPDF | Notes |
|--------------|---------|-------|
| `new Margins(top, right, bottom, left)` | Individual properties | No margins object |
| `margins.Top` | `RenderingOptions.MarginTop` | Top margin (mm) |
| `margins.Right` | `RenderingOptions.MarginRight` | Right margin (mm) |
| `margins.Bottom` | `RenderingOptions.MarginBottom` | Bottom margin (mm) |
| `margins.Left` | `RenderingOptions.MarginLeft` | Left margin (mm) |

### Document Operations

| PDF Duo .NET | IronPDF | Notes |
|--------------|---------|-------|
| `PDFDocument.Load(path)` | `PdfDocument.FromFile(path)` | Load PDF |
| `document.Save(path)` | `pdf.SaveAs(path)` | Save PDF |
| `document.ToBytes()` | `pdf.BinaryData` | Get byte array |
| `PDFDocument.Merge(docs)` | `PdfDocument.Merge(pdfs)` | Merge PDFs |
| `new PdfMerger()` | `PdfDocument.Merge()` | Static method |
| `merger.AddFile(path)` | _(load separately)_ | Load then merge |

### Features Not in PDF Duo (New with IronPDF)

| Feature | IronPDF |
|---------|---------|
| Headers/Footers | `RenderingOptions.HtmlHeader`, `HtmlFooter` |
| Page numbers | `{page}`, `{total-pages}` placeholders |
| Watermarks | `pdf.ApplyWatermark(html)` |
| Password protection | `pdf.SecuritySettings` |
| Form filling | `pdf.Form.Fields` |
| Digital signatures | `pdf.SignWithFile()` |
| Text extraction | `pdf.ExtractAllText()` |
| PDF to Image | `pdf.RasterizeToImageFiles()` |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (PDF Duo .NET):**
```csharp
using PDFDuo;

public class PdfDuoService
{
    public void CreatePdf(string html, string outputPath)
    {
        var converter = new HtmlToPdfConverter();
        converter.ConvertHtmlString(html, outputPath);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();
    }

    public void CreatePdf(string html, string outputPath)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs(outputPath);
    }
}
```

### Example 2: URL to PDF with Configuration

**Before (PDF Duo .NET):**
```csharp
using PDFDuo;

var settings = new PDFSettings
{
    PageSize = PageSize.A4,
    Margins = new Margins(20, 20, 20, 20),
    Orientation = Orientation.Portrait
};

var converter = new HtmlToPdfConverter(settings);
converter.ConvertUrl("https://example.com", "webpage.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: PDF Merging

**Before (PDF Duo .NET):**
```csharp
using PDFDuo;

var merger = new PdfMerger();
merger.AddFile("document1.pdf");
merger.AddFile("document2.pdf");
merger.AddFile("document3.pdf");
merger.Merge("merged.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Linq;

var paths = new[] { "document1.pdf", "document2.pdf", "document3.pdf" };
var pdfs = paths.Select(PdfDocument.FromFile).ToList();
var merged = PdfDocument.Merge(pdfs);
merged.SaveAs("merged.pdf");
```

### Example 4: Adding Headers and Footers (New Feature)

**Before (PDF Duo .NET):**
```csharp
// PDF Duo .NET doesn't support headers/footers
// No equivalent functionality
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center; font-size:10px;'>Company Report</div>",
    MaxHeight = 25
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center; font-size:10px;'>Page {page} of {total-pages}</div>",
    MaxHeight = 25
};

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 5: Adding Watermarks (New Feature)

**Before (PDF Duo .NET):**
```csharp
// PDF Duo .NET doesn't support watermarks
// No equivalent functionality
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

var pdf = PdfDocument.FromFile("document.pdf");

pdf.ApplyWatermark(
    "<h1 style='color:red; opacity:0.5; font-size:72px;'>CONFIDENTIAL</h1>",
    45,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);

pdf.SaveAs("watermarked.pdf");
```

### Example 6: Password Protection (New Feature)

**Before (PDF Duo .NET):**
```csharp
// PDF Duo .NET doesn't support password protection
// No equivalent functionality
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

pdf.SecuritySettings.OwnerPassword = "admin123";
pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;

pdf.SaveAs("protected.pdf");
```

### Example 7: Text Extraction (New Feature)

**Before (PDF Duo .NET):**
```csharp
// PDF Duo .NET doesn't support text extraction
// No equivalent functionality
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
string allText = pdf.ExtractAllText();
Console.WriteLine(allText);

// Per-page extraction
for (int i = 0; i < pdf.PageCount; i++)
{
    string pageText = pdf.ExtractTextFromPage(i);
    Console.WriteLine($"Page {i + 1}: {pageText}");
}
```

### Example 8: PDF to Images (New Feature)

**Before (PDF Duo .NET):**
```csharp
// PDF Duo .NET doesn't support PDF to image conversion
// No equivalent functionality
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Render all pages as images
pdf.RasterizeToImageFiles("page_*.png", DPI: 150);

// Or get as bitmaps
var bitmaps = pdf.ToBitmap(150);
```

### Example 9: Form Filling (New Feature)

**Before (PDF Duo .NET):**
```csharp
// PDF Duo .NET doesn't support form filling
// No equivalent functionality
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("form.pdf");

// Fill form fields
pdf.Form.FindFormField("Name").Value = "John Doe";
pdf.Form.FindFormField("Email").Value = "john@example.com";
pdf.Form.FindFormField("Date").Value = DateTime.Now.ToString("yyyy-MM-dd");

// Optionally flatten (make non-editable)
pdf.Form.Flatten();

pdf.SaveAs("filled_form.pdf");
```

### Example 10: Complete Migration Example

**Before (PDF Duo .NET):**
```csharp
using PDFDuo;

public class PdfDuoService
{
    public void GenerateReport(string html, string outputPath)
    {
        var settings = new PDFSettings
        {
            PageSize = PageSize.A4,
            Margins = new Margins(20, 20, 20, 20)
        };

        var converter = new HtmlToPdfConverter(settings);
        converter.ConvertHtmlString(html, outputPath);

        // That's it - no headers, footers, watermarks, or security
        // PDF Duo .NET has very limited functionality
    }

    public void MergePdfs(string[] files, string outputPath)
    {
        var merger = new PdfMerger();
        foreach (var file in files)
        {
            merger.AddFile(file);
        }
        merger.Merge(outputPath);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;
using System.Linq;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();

        // Configure renderer once, reuse for all renders
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.MarginTop = 20;
        _renderer.RenderingOptions.MarginBottom = 20;
        _renderer.RenderingOptions.MarginLeft = 20;
        _renderer.RenderingOptions.MarginRight = 20;

        // Add professional headers and footers
        _renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;'>Company Report</div>",
            MaxHeight = 25
        };

        _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
            MaxHeight = 25
        };
    }

    public void GenerateReport(string html, string outputPath)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);

        // Add watermark for draft versions
        pdf.ApplyWatermark(
            "<div style='color:gray; opacity:0.3;'>DRAFT</div>",
            45,
            VerticalAlignment.Middle,
            HorizontalAlignment.Center);

        // Add security
        pdf.SecuritySettings.OwnerPassword = "admin";
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

        pdf.SaveAs(outputPath);
    }

    public void MergePdfs(string[] files, string outputPath)
    {
        var pdfs = files.Select(PdfDocument.FromFile).ToList();
        var merged = PdfDocument.Merge(pdfs);
        merged.SaveAs(outputPath);
    }

    public string ExtractText(string pdfPath)
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        return pdf.ExtractAllText();
    }
}
```

---

## Common Migration Issues

### Issue 1: Margins Object vs Individual Properties

**Problem:** PDF Duo uses a single Margins object, IronPDF uses individual properties

```csharp
// PDF Duo:
new Margins(top: 20, right: 15, bottom: 20, left: 15)

// IronPDF:
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginRight = 15;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 15;
```

### Issue 2: Save Methods

**Problem:** Different method names for saving

```csharp
// PDF Duo:
document.Save("output.pdf");

// IronPDF:
pdf.SaveAs("output.pdf");
```

### Issue 3: Loading PDFs

**Problem:** Different method names for loading

```csharp
// PDF Duo:
PDFDocument.Load("document.pdf")

// IronPDF:
PdfDocument.FromFile("document.pdf")
```

### Issue 4: Settings Object vs Properties

**Problem:** PDF Duo uses settings objects passed to constructor

```csharp
// PDF Duo:
var settings = new PDFSettings { PageSize = PageSize.A4 };
var converter = new HtmlToPdfConverter(settings);

// IronPDF:
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
```

### Issue 5: Missing Features to Add

PDF Duo .NET likely lacked many features that IronPDF provides. Consider adding:
- Headers and footers with page numbers
- Watermarks for confidential documents
- Password protection
- Text extraction for indexing
- PDF to image for previews

---

## Feature Comparison

| Feature | PDF Duo .NET | IronPDF |
|---------|-------------|---------|
| HTML to PDF | Basic | Full CSS3, JavaScript |
| URL to PDF | Basic | Full with auth support |
| PDF Merging | Yes | Yes |
| Headers/Footers | No | Full HTML support |
| Page Numbers | No | Built-in placeholders |
| Watermarks | No | HTML-based |
| Password Protection | No | Full security options |
| Form Filling | No | Yes |
| Digital Signatures | No | Yes |
| Text Extraction | No | Yes |
| PDF to Images | No | Yes |
| Async Support | Unknown | Full async/await |
| .NET Core/5+ | Unknown | Full support |

---

## Migration Checklist

### Pre-Migration

- [ ] **Find all PDF Duo references**
  ```bash
  grep -r "PDFDuo\|HtmlToPdfConverter\|PdfMerger" --include="*.cs" .
  ```
  **Why:** Identify all instances of PDF Duo to ensure complete migration to IronPDF.

- [ ] **Document current settings (page size, margins, etc.)**
  ```csharp
  // Example pattern to document:
  var settings = new PdfSettings {
      PageSize = PageSizes.A4,
      Margins = new Margins(10, 10, 10, 10)
  };
  ```
  **Why:** These settings will map to IronPDF's RenderingOptions. Document them to ensure consistent output.

- [ ] **List all PDF operations used**
  ```csharp
  // Example operations:
  converter.LoadHtml(html);
  merger.Merge(pdfs);
  ```
  **Why:** Understanding all operations helps ensure a smooth transition to IronPDF equivalents.

- [ ] **Identify opportunities for new features (headers, watermarks, security)**
  **Why:** IronPDF offers advanced features like headers, watermarks, and security settings that may enhance your application.

### Migration Steps

- [ ] **Install IronPDF NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project to replace PDF Duo functionality.

- [ ] **Remove PDF Duo NuGet package**
  ```bash
  dotnet remove package PDFDuo
  ```
  **Why:** Remove outdated library to avoid conflicts and ensure clean migration.

- [ ] **Add IronPDF license key configuration**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Update namespace imports**
  ```csharp
  // Before (PDF Duo)
  using PDFDuo;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Update namespaces to use IronPDF classes and methods.

- [ ] **Replace `HtmlToPdfConverter` with `ChromePdfRenderer`**
  ```csharp
  // Before (PDF Duo)
  var converter = new HtmlToPdfConverter();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** Use IronPDF's ChromePdfRenderer for modern HTML/CSS rendering.

- [ ] **Convert Margins object to individual properties**
  ```csharp
  // Before (PDF Duo)
  converter.Margins = new Margins(10, 10, 10, 10);

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 10;
  renderer.RenderingOptions.MarginBottom = 10;
  renderer.RenderingOptions.MarginLeft = 10;
  renderer.RenderingOptions.MarginRight = 10;
  ```
  **Why:** IronPDF uses individual properties for margin settings.

- [ ] **Replace `Save()` with `SaveAs()`**
  ```csharp
  // Before (PDF Duo)
  converter.Save("output.pdf");

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses SaveAs() for saving PDF files.

- [ ] **Replace `Load()` with `FromFile()`**
  ```csharp
  // Before (PDF Duo)
  var pdf = converter.Load("input.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("input.pdf");
  ```
  **Why:** Use IronPDF's FromFile() to load existing PDFs.

- [ ] **Add headers/footers for professional output**
  ```csharp
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
  **Why:** Enhance PDF documents with headers and footers using IronPDF's HTML capabilities.

- [ ] **Consider adding watermarks for sensitive documents**
  ```csharp
  // After (IronPDF)
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>CONFIDENTIAL</h1>");
  ```
  **Why:** Watermarks can indicate document status or confidentiality.

- [ ] **Add password protection if needed**
  ```csharp
  // After (IronPDF)
  pdf.SecuritySettings.UserPassword = "securepassword";
  ```
  **Why:** Enhance document security by adding password protection.

### Post-Migration

- [ ] **Run regression tests comparing PDF output**
  **Why:** Ensure that the migration to IronPDF maintains or improves document quality.

- [ ] **Verify page sizes and margins match**
  **Why:** Confirm that the visual layout of documents remains consistent after migration.

- [ ] **Test with complex HTML/CSS**
  **Why:** IronPDF's modern rendering engine should handle complex web content effectively.

- [ ] **Add new features (headers, footers, security)**
  **Why:** Leverage IronPDF's advanced features to enhance document functionality and appearance.

- [ ] **Update documentation**
  **Why:** Ensure all team members and stakeholders understand the new PDF generation process and features.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [HTML to PDF Tutorial](https://ironpdf.com/tutorials/html-to-pdf/)
- [PDF Merging Guide](https://ironpdf.com/how-to/merge-pdfs/)
- [Headers and Footers Guide](https://ironpdf.com/how-to/headers-and-footers/)
