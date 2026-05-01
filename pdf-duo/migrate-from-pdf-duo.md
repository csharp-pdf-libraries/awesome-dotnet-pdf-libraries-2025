# How Do I Migrate from PDF Duo .NET to IronPDF in C#?

## Why Migrate?

PDF Duo .NET (DuoDimension Software) is a niche HTML-to-PDF component whose last public release (v2.4) shipped in **December 2010**. It targets .NET Framework 1.1 / 2.0 / 3.0 / 3.5 only and is distributed as a DLL download from duodimension.com — **there is no NuGet package**. Migrating to IronPDF gives you a modern, Chromium-based engine that actually runs on .NET Framework 4.6.2+, .NET 6/7/8/9/10, Linux, macOS, and Docker.

### Critical Issues with PDF Duo .NET

1. **Effectively Abandoned**: Last release v2.4 dated December 10, 2010 (per the vendor's own download listing). No new versions have appeared in roughly 15 years.

2. **No NuGet Distribution**: Installation is "download a zip from duodimension.com and reference PDFDuo.dll." There is no `dotnet add package` story, no SemVer, no transitive dependency resolution.

3. **Pre-modern .NET Targeting**: Vendor lists supported runtimes as .NET Framework 1.1 through 3.5 and Windows XP / Vista / 7 / 2000 / 2003. There is no documented support for .NET Framework 4.x, .NET Core, .NET 5+, Linux, or 64-bit-clean modern builds.

4. **Sparse Documentation**: One vendor product page, a couple of sample snippets, and a 2010 press release. No API reference site, no GitHub repo, no Stack Overflow tag of any size.

5. **Narrow Feature Set**: The documented surface is essentially `DuoDimension.HtmlToPdf` with `OpenHTML(...)` and `SavePDF(...)`. There is **no documented native API** for watermarking, password protection / encryption, digital signatures, form filling, text extraction, PDF/A, or PDF merging — teams that needed merge typically paired PDF Duo with iTextSharp 4.x.

6. **Unknown Rendering Engine**: The vendor describes it as a self-contained component with no Office or Acrobat dependency, but does not publish what HTML/CSS engine it uses. CSS3, modern JavaScript, web fonts, and flex/grid layout are not claimed.

### Benefits of IronPDF

| Aspect | PDF Duo .NET | IronPDF |
|--------|-------------|---------|
| Last release | v2.4, December 2010 | Active, regular releases |
| Distribution | DLL download from duodimension.com (no NuGet) | NuGet `IronPdf` |
| Runtime support | .NET Framework 1.1 - 3.5, Windows only | .NET FX 4.6.2+, .NET 6/7/8/9/10, Linux, macOS, Docker |
| Documentation | One vendor product page | Comprehensive docs + API reference |
| Support | None visible | Professional support team |
| Community | Negligible | 41M+ NuGet downloads (Iron Software stack) |
| Rendering | Engine not disclosed | Modern Chromium |
| Features | HTML-to-PDF only | HTML-to-PDF, merge, security, signatures, OCR, forms, watermark |
| Licensing | Per vendor pricing page (verify with sales) | Commercial, perpetual or subscription |

The [migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-pdf-duo-to-ironpdf/) addresses the specific challenges of migrating from obscure libraries to well-documented solutions.

---

## Package / Reference Changes

PDF Duo .NET is **not on NuGet**. Removal is a manual step:

```bash
# In Visual Studio: References -> remove PDFDuo.dll (and any associated PDF Duo files
# such as PDFDuoNET.dll). Delete the binary from your /lib folder if you vendored it.

# Then install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

The vendor's documented root namespace is `DuoDimension`. Sub-namespaces below are not part of any documented public surface — most code using PDF Duo .NET only ever imports `DuoDimension`.

| PDF Duo .NET | IronPDF |
|--------------|---------|
| `using DuoDimension;` | `using IronPdf;` |
| _(no documented sub-namespaces)_ | `using IronPdf.Rendering;` |

---

## Complete API Mapping

### HTML to PDF Conversion

The documented PDF Duo surface is `DuoDimension.HtmlToPdf` with the methods `OpenHTML(...)` (accepts file path, URL, or HTML string written to disk) and `SavePDF(path)`.

| PDF Duo .NET | IronPDF | Notes |
|--------------|---------|-------|
| `new DuoDimension.HtmlToPdf()` | `new ChromePdfRenderer()` | Main renderer |
| `conv.OpenHTML(htmlFile); conv.SavePDF(path);` | `renderer.RenderHtmlFileAsPdf(htmlFile).SaveAs(path)` | HTML file on disk |
| `conv.OpenHTML(url); conv.SavePDF(path);` | `renderer.RenderUrlAsPdf(url).SaveAs(path)` | URL conversion |
| _(write string to temp .html, then `OpenHTML`)_ | `renderer.RenderHtmlAsPdf(htmlString).SaveAs(path)` | Direct HTML string — no native PDF Duo equivalent |
| _(no in-memory byte API)_ | `pdf.BinaryData` | IronPDF returns bytes without round-tripping a file |

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

PDF Duo .NET is an HTML-to-PDF converter only. The vendor product page does not document a `Load`, `ToBytes`, or `Merge` API on `DuoDimension.HtmlToPdf`. The rows below show what teams typically reached for in adjacent libraries when using PDF Duo, and the IronPDF call that consolidates that work.

| Need | PDF Duo .NET | IronPDF | Notes |
|------|-------------|---------|-------|
| Load existing PDF | not native — use iTextSharp 4.x or similar | `PdfDocument.FromFile(path)` | Single dependency in IronPDF |
| Save PDF | `conv.SavePDF(path)` | `pdf.SaveAs(path)` | Both write to a file path |
| Get PDF bytes | not native | `pdf.BinaryData` | PDF Duo writes to disk only |
| Merge PDFs | not native | `PdfDocument.Merge(pdf1, pdf2)` or `Merge(IEnumerable<PdfDocument>)` | See merging example below |

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
using DuoDimension;
using System.IO;

public class PdfDuoService
{
    public void CreatePdf(string html, string outputPath)
    {
        // PDF Duo's OpenHTML accepts a file path, URL, or stream — there is no
        // documented "convert HTML string to PDF" call, so write to a temp file first.
        var tempHtml = Path.GetTempFileName() + ".html";
        File.WriteAllText(tempHtml, html);

        var conv = new HtmlToPdf();
        conv.OpenHTML(tempHtml);
        conv.SavePDF(outputPath);
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

### Example 2: URL to PDF

**Before (PDF Duo .NET):**
```csharp
using DuoDimension;

// PDF Duo's HtmlToPdf does not document a settings/options object covering
// page size, margins, and orientation in one struct. OpenHTML(url) + SavePDF(path)
// is the public surface; deeper layout customization is not part of the documented API.
var conv = new HtmlToPdf();
conv.OpenHTML("https://example.com");
conv.SavePDF("webpage.pdf");
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
// PDF Duo .NET has no native merge API. Teams using PDF Duo for HTML-to-PDF in
// the 2010 era typically combined it with iTextSharp 4.x for downstream merging:
using DuoDimension;
// using iTextSharp.text.pdf;  // separate dependency

// 1) Render each HTML source to its own PDF
var conv = new HtmlToPdf();
conv.OpenHTML("page1.html"); conv.SavePDF("document1.pdf");
conv = new HtmlToPdf();
conv.OpenHTML("page2.html"); conv.SavePDF("document2.pdf");

// 2) Merge with a *different* library. Pseudocode:
//    var reader1 = new PdfReader("document1.pdf");
//    var reader2 = new PdfReader("document2.pdf");
//    PdfCopy.Append(...) // not part of PDF Duo
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
using DuoDimension;
using System.IO;

public class PdfDuoService
{
    public void GenerateReport(string html, string outputPath)
    {
        // No settings object, no margins/page-size options on HtmlToPdf.
        // No headers/footers, watermarks, or security in the documented API.
        var tempHtml = Path.GetTempFileName() + ".html";
        File.WriteAllText(tempHtml, html);

        var conv = new HtmlToPdf();
        conv.OpenHTML(tempHtml);
        conv.SavePDF(outputPath);
    }

    public void MergePdfs(string[] files, string outputPath)
    {
        // PDF Duo .NET has no merge API. A second library is required.
        throw new System.NotSupportedException(
            "PDF Duo .NET does not provide PDF merging. Combine with another library or migrate.");
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

### Issue 1: No Page-Layout Options on PDF Duo's HtmlToPdf

**Problem:** `DuoDimension.HtmlToPdf` does not document a settings object covering paper size, orientation, or margins; IronPDF exposes them on `RenderingOptions`.

```csharp
// PDF Duo: no documented per-instance settings — OpenHTML / SavePDF only.

// IronPDF:
renderer.RenderingOptions.PaperSize  = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop  = 20;
renderer.RenderingOptions.MarginRight = 15;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 15;
```

### Issue 2: Save Methods

**Problem:** Different method names for saving.

```csharp
// PDF Duo:
conv.SavePDF("output.pdf");

// IronPDF:
pdf.SaveAs("output.pdf");
```

### Issue 3: Loading Existing PDFs

**Problem:** PDF Duo .NET only writes PDFs — there is no documented "load existing PDF" call. To operate on an existing PDF you must move to IronPDF (or another library).

```csharp
// PDF Duo: no equivalent.

// IronPDF:
var pdf = PdfDocument.FromFile("document.pdf");
```

### Issue 4: HTML String → Temp File vs Direct Render

**Problem:** PDF Duo's `OpenHTML` does not document a "render this HTML string" call; teams write the markup to a temp file first. IronPDF renders strings directly.

```csharp
// PDF Duo:
File.WriteAllText("temp.html", html);
var conv = new DuoDimension.HtmlToPdf();
conv.OpenHTML("temp.html");
conv.SavePDF("output.pdf");

// IronPDF:
var renderer = new ChromePdfRenderer();
renderer.RenderHtmlAsPdf(html).SaveAs("output.pdf");
```

### Issue 5: Missing Features to Add

PDF Duo .NET's documented API is HTML-to-PDF only. After migrating, consider adding:
- Headers and footers with page numbers
- Watermarks for confidential documents
- Password protection
- Text extraction for indexing
- PDF to image for previews
- Native PDF merge

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
  grep -r "DuoDimension\|HtmlToPdf\b\|OpenHTML\|SavePDF" --include="*.cs" .
  ```
  **Why:** Identify all instances of `DuoDimension.HtmlToPdf` to ensure complete migration to IronPDF.

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

- [ ] **Remove PDF Duo DLL reference**
  ```text
  PDF Duo .NET is not on NuGet — remove the project reference to PDFDuo.dll
  in Visual Studio (References -> right-click -> Remove) and delete the
  vendored binary from your /lib folder.
  ```
  **Why:** Eliminates a 2010-era native dependency and avoids type/method-name conflicts during the cutover.

- [ ] **Add IronPDF license key configuration**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Update namespace imports**
  ```csharp
  // Before (PDF Duo)
  using DuoDimension;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Update namespaces to use IronPDF classes and methods.

- [ ] **Replace `DuoDimension.HtmlToPdf` with `ChromePdfRenderer`**
  ```csharp
  // Before (PDF Duo)
  var conv = new DuoDimension.HtmlToPdf();
  conv.OpenHTML(htmlPath);
  conv.SavePDF(pdfPath);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  renderer.RenderHtmlFileAsPdf(htmlPath).SaveAs(pdfPath);
  ```
  **Why:** Use IronPDF's ChromePdfRenderer for modern HTML/CSS rendering and to avoid the temp-file detour PDF Duo requires for HTML strings.

- [ ] **Set page-layout options that PDF Duo did not expose**
  ```csharp
  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.MarginTop = 10;
  renderer.RenderingOptions.MarginBottom = 10;
  renderer.RenderingOptions.MarginLeft = 10;
  renderer.RenderingOptions.MarginRight = 10;
  ```
  **Why:** PDF Duo does not document a settings object for paper size / margins. With IronPDF, set them explicitly in `RenderingOptions`.

- [ ] **Replace `SavePDF()` with `SaveAs()`**
  ```csharp
  // Before (PDF Duo)
  conv.SavePDF("output.pdf");

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses `SaveAs()` for saving PDF files.

- [ ] **Replace any in-memory load with `PdfDocument.FromFile()`**
  ```csharp
  // PDF Duo .NET cannot load an existing PDF — it only writes them.
  // After (IronPDF)
  var pdf = PdfDocument.FromFile("input.pdf");
  ```
  **Why:** IronPDF lets you operate on existing PDFs (merge, watermark, sign, extract); PDF Duo only produces them.

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
