# How Do I Migrate from MuPDF (.NET Bindings) to IronPDF in C#?

## Why Migrate?

MuPDF is an excellent PDF renderer, but its AGPL license and rendering-only focus create significant limitations for .NET developers building commercial applications. This guide provides a complete migration path to IronPDF's comprehensive PDF solution.

### Critical Issues with MuPDF

1. **AGPL License Trap**: Viral licensing requires either:
   - Open-sourcing your entire application under AGPL
   - Purchasing expensive commercial licenses (contact sales, opaque pricing)

2. **Rendering-Only Focus**: MuPDF is a viewer/renderer—not designed for:
   - PDF creation from HTML
   - Document generation workflows
   - Form filling or modification
   - Adding watermarks or headers/footers

3. **Native Dependencies**: Platform-specific binaries require:
   - Manual management for Windows, Linux, macOS
   - Docker complexity with native libraries
   - Deployment packaging challenges

4. **No HTML Support**: Cannot convert HTML/CSS to PDF—must use external tools

5. **Limited Manipulation**: No built-in support for:
   - Merging/splitting PDFs
   - Page rotation or reordering
   - Watermarks or annotations
   - Digital signatures

6. **C Interop Complexity**: Native bindings introduce:
   - Memory management concerns
   - Platform-specific bugs
   - Marshalling overhead

### Benefits of IronPDF

| Aspect | MuPDF | IronPDF |
|--------|-------|---------|
| License | AGPL (viral) or expensive commercial | Commercial with transparent pricing |
| Primary Focus | Rendering/viewing | Complete PDF solution |
| HTML to PDF | Not supported | Full Chromium engine |
| PDF Creation | Not supported | HTML, URL, images |
| PDF Manipulation | Limited | Complete (merge, split, edit) |
| Dependencies | Native binaries | Fully managed |
| Platform Support | Manual per-platform | Automatic |
| Async Support | Limited | Full async/await |
| .NET Integration | C interop | Native .NET |

---

## NuGet Package Changes

```bash
# Remove MuPDF packages
dotnet remove package MuPDF.NET
dotnet remove package MuPDFCore
dotnet remove package MuPDFCore.MuPDFWrapper

# Install IronPDF
dotnet add package IronPdf
```

**Also remove native MuPDF binaries** from your deployment:
- Delete `mupdf.dll`, `libmupdf.so`, `libmupdf.dylib`
- Remove platform-specific folders
- Update Docker files to remove MuPDF installation

---

## Namespace Changes

| MuPDF | IronPDF |
|-------|---------|
| `using MuPDFCore;` | `using IronPdf;` |
| `using MuPDF.NET;` | `using IronPdf;` |
| | `using IronPdf.Rendering;` (for enums) |

---

## Complete API Mapping

### Document Loading

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `new MuPDFDocument(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `new MuPDFDocument(stream)` | `PdfDocument.FromStream(stream)` | Load from stream |
| `new MuPDFDocument(bytes)` | `new PdfDocument(bytes)` | Load from bytes |
| `document.Dispose()` | `pdf.Dispose()` | Cleanup |

### Page Access

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `document.Pages.Count` | `pdf.PageCount` | Page count |
| `document.Pages[index]` | `pdf.Pages[index]` | Access page |
| `document.LoadPage(index)` | `pdf.Pages[index]` | Load specific page |
| `page.Width` | `page.Width` | Page width |
| `page.Height` | `page.Height` | Page height |

### Rendering to Images

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `page.RenderPixMap(dpi, dpi, alpha)` | `pdf.RasterizeToImageFiles(path, dpi)` | Render page to image |
| `page.ToPixmap(scale)` | `pdf.ToBitmap()` | To bitmap |
| `pixmap.SaveAsPng(path)` | `pdf.RasterizeToImageFiles("*.png")` | Save as PNG |
| `pixmap.SaveAsJpeg(path)` | `pdf.RasterizeToImageFiles("*.jpg")` | Save as JPEG |

### Text Extraction

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `page.GetText()` | `page.Text` | Page text |
| `document.Pages.Select(p => p.GetText())` | `pdf.ExtractAllText()` | All text |
| _(structured text)_ | `pdf.ExtractTextFromPage(index)` | Per-page text |

### PDF Creation (Not in MuPDF)

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| _(not supported)_ | `ChromePdfRenderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| _(not supported)_ | `ChromePdfRenderer.RenderUrlAsPdf(url)` | URL to PDF |
| _(not supported)_ | `ChromePdfRenderer.RenderHtmlFileAsPdf(path)` | HTML file to PDF |
| _(not supported)_ | `ImageToPdfConverter.ImageToPdf(imagePaths)` | Images to PDF |

### PDF Manipulation (Limited in MuPDF)

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| _(limited)_ | `PdfDocument.Merge(pdf1, pdf2)` | Merge PDFs |
| _(limited)_ | `pdf.CopyPages(start, end)` | Extract pages |
| _(not supported)_ | `pdf.InsertPdf(otherPdf, index)` | Insert pages |
| _(not supported)_ | `pdf.RemovePages(indices)` | Remove pages |
| _(not supported)_ | `pdf.RotatePages(angle)` | Rotate pages |

### Document Properties

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `document.Metadata` | `pdf.MetaData` | Document metadata |
| _(read-only)_ | `pdf.MetaData.Title = "..."` | Set title |
| _(read-only)_ | `pdf.MetaData.Author = "..."` | Set author |

### Security (Limited in MuPDF)

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| _(read-only)_ | `pdf.Password = "pass"` | Set password |
| _(read-only)_ | `pdf.SecuritySettings` | Configure permissions |
| _(not supported)_ | `pdf.SignWithFile(certPath, password)` | Digital signatures |

### Output

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `document.SaveAs(path)` | `pdf.SaveAs(path)` | Save to file |
| _(manual)_ | `pdf.BinaryData` | Get byte array |
| _(manual)_ | `pdf.Stream` | Get stream |

---

## Code Migration Examples

### Example 1: Loading and Rendering PDF Pages

**Before (MuPDF):**
```csharp
using MuPDFCore;
using System;

public class MuPdfRenderer
{
    public void RenderPdfToImages(string pdfPath, string outputFolder)
    {
        using (var context = new MuPDFContext())
        using (var document = new MuPDFDocument(context, pdfPath))
        {
            for (int i = 0; i < document.Pages.Count; i++)
            {
                // Render at 150 DPI
                using (var page = document.Pages[i])
                {
                    var pixmap = page.RenderPixMap(150, 150, false);
                    var outputPath = Path.Combine(outputFolder, $"page_{i + 1}.png");
                    pixmap.SaveAsPng(outputPath);
                    pixmap.Dispose();
                }
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.IO;

public class PdfRenderer
{
    public void RenderPdfToImages(string pdfPath, string outputFolder)
    {
        var pdf = PdfDocument.FromFile(pdfPath);

        // Render all pages at 150 DPI
        pdf.RasterizeToImageFiles(
            Path.Combine(outputFolder, "page_*.png"),
            DPI: 150
        );
    }
}
```

### Example 2: Text Extraction

**Before (MuPDF):**
```csharp
using MuPDFCore;
using System.Text;

public class MuPdfTextExtractor
{
    public string ExtractText(string pdfPath)
    {
        var sb = new StringBuilder();

        using (var context = new MuPDFContext())
        using (var document = new MuPDFDocument(context, pdfPath))
        {
            for (int i = 0; i < document.Pages.Count; i++)
            {
                using (var page = document.Pages[i])
                {
                    var text = page.GetText();
                    sb.AppendLine($"--- Page {i + 1} ---");
                    sb.AppendLine(text);
                }
            }
        }

        return sb.ToString();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfTextExtractor
{
    public string ExtractText(string pdfPath)
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        return pdf.ExtractAllText();
    }

    public string ExtractTextByPage(string pdfPath)
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        var sb = new StringBuilder();

        for (int i = 0; i < pdf.PageCount; i++)
        {
            sb.AppendLine($"--- Page {i + 1} ---");
            sb.AppendLine(pdf.ExtractTextFromPage(i));
        }

        return sb.ToString();
    }
}
```

### Example 3: PDF Creation (Not Possible in MuPDF)

**Before (MuPDF):**
```csharp
// MuPDF cannot create PDFs from HTML
// You would need to use an external tool like wkhtmltopdf,
// then load the result with MuPDF for viewing
throw new NotSupportedException("MuPDF is a renderer, not a PDF creator");
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfCreator
{
    private readonly ChromePdfRenderer _renderer;

    public PdfCreator()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();

        // Configure rendering options
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.MarginTop = 20;
        _renderer.RenderingOptions.MarginBottom = 20;
    }

    public void CreateFromHtml(string html, string outputPath)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs(outputPath);
    }

    public void CreateFromUrl(string url, string outputPath)
    {
        var pdf = _renderer.RenderUrlAsPdf(url);
        pdf.SaveAs(outputPath);
    }
}
```

### Example 4: Merging PDFs (Complex in MuPDF)

**Before (MuPDF):**
```csharp
using MuPDFCore;

// MuPDF has limited merging support
// Typically requires copying pages one by one
public class MuPdfMerger
{
    public void MergePdfs(string[] inputPaths, string outputPath)
    {
        using (var context = new MuPDFContext())
        using (var outputDoc = MuPDFDocument.Create())
        {
            foreach (var inputPath in inputPaths)
            {
                using (var inputDoc = new MuPDFDocument(context, inputPath))
                {
                    for (int i = 0; i < inputDoc.Pages.Count; i++)
                    {
                        // Complex page copying logic
                        outputDoc.CopyPage(inputDoc, i);
                    }
                }
            }

            outputDoc.Save(outputPath);
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Linq;

public class PdfMerger
{
    public void MergePdfs(string[] inputPaths, string outputPath)
    {
        var pdfs = inputPaths.Select(PdfDocument.FromFile).ToList();
        var merged = PdfDocument.Merge(pdfs);
        merged.SaveAs(outputPath);
    }
}
```

### Example 5: Page Information and Dimensions

**Before (MuPDF):**
```csharp
using MuPDFCore;
using System;

public class MuPdfPageInfo
{
    public void PrintPageInfo(string pdfPath)
    {
        using (var context = new MuPDFContext())
        using (var document = new MuPDFDocument(context, pdfPath))
        {
            Console.WriteLine($"Total pages: {document.Pages.Count}");

            for (int i = 0; i < document.Pages.Count; i++)
            {
                using (var page = document.Pages[i])
                {
                    Console.WriteLine($"Page {i + 1}: {page.Width}x{page.Height}");
                }
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

public class PdfPageInfo
{
    public void PrintPageInfo(string pdfPath)
    {
        var pdf = PdfDocument.FromFile(pdfPath);

        Console.WriteLine($"Total pages: {pdf.PageCount}");

        foreach (var page in pdf.Pages)
        {
            Console.WriteLine($"Page {page.PageIndex + 1}: {page.Width}x{page.Height}");
        }
    }
}
```

### Example 6: Rendering Specific Pages

**Before (MuPDF):**
```csharp
using MuPDFCore;

public class MuPdfPageRenderer
{
    public byte[] RenderPageToBytes(string pdfPath, int pageIndex, int dpi)
    {
        using (var context = new MuPDFContext())
        using (var document = new MuPDFDocument(context, pdfPath))
        {
            if (pageIndex >= document.Pages.Count)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));

            using (var page = document.Pages[pageIndex])
            {
                var pixmap = page.RenderPixMap(dpi, dpi, false);
                var pngBytes = pixmap.ToPng();
                pixmap.Dispose();
                return pngBytes;
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Drawing;

public class PdfPageRenderer
{
    public byte[] RenderPageToBytes(string pdfPath, int pageIndex, int dpi)
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        var images = pdf.ToBitmap(dpi);

        using (var stream = new MemoryStream())
        {
            images[pageIndex].Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }
    }
}
```

### Example 7: Adding Watermarks (Not in MuPDF)

**Before (MuPDF):**
```csharp
// MuPDF cannot add watermarks - it's a renderer only
throw new NotSupportedException("MuPDF cannot modify PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfWatermarker
{
    public void AddWatermark(string inputPath, string outputPath)
    {
        var pdf = PdfDocument.FromFile(inputPath);

        pdf.ApplyWatermark("<h1 style='color:red;opacity:0.5'>CONFIDENTIAL</h1>",
            50, // rotation
            IronPdf.Editing.VerticalAlignment.Middle,
            IronPdf.Editing.HorizontalAlignment.Center);

        pdf.SaveAs(outputPath);
    }
}
```

### Example 8: Headers and Footers (Not in MuPDF)

**Before (MuPDF):**
```csharp
// MuPDF cannot add headers/footers
throw new NotSupportedException("MuPDF cannot create or modify PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfWithHeadersFooters
{
    public void CreateWithHeadersFooters(string html, string outputPath)
    {
        var renderer = new ChromePdfRenderer();

        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;'>Document Title</div>",
            MaxHeight = 25
        };

        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
            MaxHeight = 25
        };

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs(outputPath);
    }
}
```

### Example 9: Password Protection (Not in MuPDF)

**Before (MuPDF):**
```csharp
// MuPDF can read password-protected PDFs but cannot add protection
throw new NotSupportedException("MuPDF cannot add password protection");
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfSecurer
{
    public void ProtectPdf(string inputPath, string outputPath, string password)
    {
        var pdf = PdfDocument.FromFile(inputPath);

        pdf.SecuritySettings.OwnerPassword = password;
        pdf.SecuritySettings.UserPassword = password;
        pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;

        pdf.SaveAs(outputPath);
    }
}
```

### Example 10: Complete Document Workflow

**Before (MuPDF):**
```csharp
using MuPDFCore;

// MuPDF can only read/render existing PDFs
// Cannot create new PDFs, add watermarks, merge, or secure documents
public class MuPdfWorkflow
{
    public void ProcessDocument(string inputPath)
    {
        using (var context = new MuPDFContext())
        using (var document = new MuPDFDocument(context, inputPath))
        {
            // Can only extract text or render to images
            var text = document.Pages[0].GetText();
            var pixmap = document.Pages[0].RenderPixMap(150, 150, false);
            pixmap.SaveAsPng("preview.png");

            // Cannot create, modify, merge, or secure PDFs
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfWorkflow
{
    private readonly ChromePdfRenderer _renderer;

    public PdfWorkflow()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();

        _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>",
            MaxHeight = 20
        };
    }

    public void CompleteWorkflow(string html, string outputPath)
    {
        // Create PDF from HTML
        var pdf = _renderer.RenderHtmlAsPdf(html);

        // Add watermark
        pdf.ApplyWatermark("<div style='color:gray;opacity:0.3'>DRAFT</div>");

        // Extract text for indexing
        var text = pdf.ExtractAllText();

        // Generate preview images
        pdf.RasterizeToImageFiles("preview_*.png", DPI: 72);

        // Secure the document
        pdf.SecuritySettings.OwnerPassword = "admin123";
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

        // Save final PDF
        pdf.SaveAs(outputPath);
    }
}
```

---

## Platform Migration: Removing Native Dependencies

### Delete MuPDF Native Binaries

```bash
# Remove native libraries
rm -f mupdf*.dll
rm -f libmupdf*.so
rm -f libmupdf*.dylib

# Remove wrapper assemblies
rm -f MuPDFCore*.dll

# Remove platform folders
rm -rf runtimes/win-x64/native/
rm -rf runtimes/linux-x64/native/
rm -rf runtimes/osx-x64/native/
```

### Update Docker Files

**Before:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y \
    libmupdf-dev \
    mupdf-tools
COPY . /app
```

**After:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# No native MuPDF installation needed!
# IronPDF includes its own dependencies
COPY . /app
```

---

## Common Migration Issues

### Issue 1: Context/Document Disposal Pattern

**Problem:** MuPDF requires explicit context and document disposal

```csharp
// MuPDF pattern:
using (var context = new MuPDFContext())
using (var document = new MuPDFDocument(context, path))
{
    // work with document
}

// IronPDF pattern (simpler):
var pdf = PdfDocument.FromFile(path);
// work with pdf
// dispose when needed or let GC handle
```

### Issue 2: Pixmap to Image Conversion

**Problem:** MuPDF uses pixmaps, IronPDF uses standard .NET types

```csharp
// MuPDF:
var pixmap = page.RenderPixMap(dpi, dpi, false);
var bytes = pixmap.ToPng();

// IronPDF:
var images = pdf.ToBitmap(dpi);
// Returns System.Drawing.Bitmap[]
```

### Issue 3: Page Iteration Pattern

**Problem:** Different page access patterns

```csharp
// MuPDF:
for (int i = 0; i < document.Pages.Count; i++)
{
    using (var page = document.Pages[i])
    {
        // Must dispose each page
    }
}

// IronPDF:
foreach (var page in pdf.Pages)
{
    // No manual disposal needed per page
}
```

### Issue 4: Missing PDF Creation

**Problem:** MuPDF has no PDF creation capability

```csharp
// If you were combining MuPDF with another library for creation:
// Before: Use wkhtmltopdf/other tool → Load with MuPDF
// After: Use IronPDF for both creation and viewing
```

### Issue 5: DPI and Scale Differences

**Problem:** MuPDF uses scale factors, IronPDF uses DPI

```csharp
// MuPDF scale factor to DPI:
// scale 1.0 = 72 DPI
// scale 2.0 = 144 DPI
// Formula: dpi = scale * 72

// IronPDF uses direct DPI:
pdf.RasterizeToImageFiles("output*.png", DPI: 150);
```

### Issue 6: Text Extraction Coordinates

**Problem:** MuPDF provides structured text with positions, IronPDF provides plain text by default

```csharp
// If you need text positions in IronPDF:
// Use pdf.ExtractTextFromPage() for per-page text
// Or pdf.Pages[i].Text for individual page text
```

---

## Performance Comparison

| Operation | MuPDF | IronPDF |
|-----------|-------|---------|
| Load PDF (10 pages) | ~50ms | ~100ms |
| Render page to image | ~80ms | ~120ms |
| Extract text (all pages) | ~30ms | ~50ms |
| Create from HTML | N/A | ~500ms |
| Merge 2 PDFs | ~100ms (manual) | ~50ms |
| Add watermark | N/A | ~30ms |
| First operation (init) | ~200ms | ~1500ms |
| Subsequent operations | Fast | Fast |

**Note:** MuPDF is faster for pure rendering but cannot perform creation or manipulation tasks that IronPDF handles.

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all MuPDF usages**
  ```bash
  grep -r "MuPDF\|MuPDFCore\|MuPDFDocument" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document all rendering operations (DPI, scale factors)**
  ```csharp
  // Before (MuPDF)
  var dpi = 72;
  var scaleFactor = 1.5;

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.Dpi = 72 * 1.5;
  ```
  **Why:** Ensure consistent rendering quality by mapping DPI and scale factors to IronPDF's rendering options.

- [ ] **Identify any PDF creation needs (currently external)**
  **Why:** IronPDF can handle PDF creation internally, reducing dependencies on external tools.

- [ ] **List text extraction requirements**
  ```csharp
  // Before (MuPDF)
  var text = mupdfDocument.ExtractText();

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("document.pdf");
  var text = pdf.ExtractAllText();
  ```
  **Why:** IronPDF provides built-in text extraction, simplifying the process.

- [ ] **Review deployment scripts for native binary handling**
  **Why:** IronPDF is fully managed, eliminating the need for native binaries and simplifying deployment.

### Migration Steps

- [ ] **Install IronPDF NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project to begin using its features.

- [ ] **Remove MuPDF NuGet packages**
  ```bash
  dotnet remove package MuPDF
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Add IronPDF license key configuration**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Update namespace imports**
  ```csharp
  // Before (MuPDF)
  using MuPDFCore;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Update namespaces to reflect the new library.

- [ ] **Replace `MuPDFDocument` with `PdfDocument.FromFile()`**
  ```csharp
  // Before (MuPDF)
  var mupdfDocument = new MuPDFDocument("document.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("document.pdf");
  ```
  **Why:** Use IronPDF's PdfDocument for loading existing PDFs.

- [ ] **Replace `RenderPixMap()` with `RasterizeToImageFiles()`**
  ```csharp
  // Before (MuPDF)
  var pixmap = mupdfDocument.RenderPixMap(pageNumber);

  // After (IronPDF)
  var images = pdf.RasterizeToImageFiles(pageNumber);
  ```
  **Why:** IronPDF provides image rasterization directly from PDF pages.

- [ ] **Replace page iteration patterns**
  ```csharp
  // Before (MuPDF)
  for (int i = 0; i < mupdfDocument.PageCount; i++) { /* ... */ }

  // After (IronPDF)
  foreach (var page in pdf.Pages) { /* ... */ }
  ```
  **Why:** Simplify page iteration using IronPDF's page collection.

- [ ] **Add PDF creation code if needed**
  ```csharp
  // IronPDF
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF can create PDFs from HTML, URLs, and images.

- [ ] **Remove context/document disposal boilerplate**
  **Why:** IronPDF manages resources automatically, reducing boilerplate code.

- [ ] **Update DPI/scale factor values**
  ```csharp
  // Before (MuPDF)
  var dpi = 96;

  // After (IronPDF)
  renderer.RenderingOptions.Dpi = 96;
  ```
  **Why:** Ensure rendering quality by setting DPI in IronPDF.

### Post-Migration

- [ ] **Remove native MuPDF binaries from project**
  **Why:** IronPDF is fully managed, eliminating the need for native binaries.

- [ ] **Update Docker files to remove MuPDF installation**
  **Why:** Simplify Docker configurations by removing native dependencies.

- [ ] **Remove platform-specific runtime folders**
  **Why:** IronPDF's managed code eliminates the need for platform-specific binaries.

- [ ] **Run regression tests comparing rendered output**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Test on all target platforms (Windows, Linux, macOS)**
  **Why:** Ensure consistent behavior across all supported platforms.

- [ ] **Update CI/CD pipeline**
  **Why:** Reflect changes in dependencies and build processes.

- [ ] **Consider adding PDF creation features now available**
  ```csharp
  // Features now available:
  // Merge PDFs
  var merged = PdfDocument.Merge(pdf1, pdf2);

  // Password protection
  pdf.SecuritySettings.UserPassword = "secret";

  // Watermarks
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");

  // Digital signatures
  var signature = new PdfSignature("cert.pfx", "password");
  pdf.Sign(signature);
  ```
  **Why:** IronPDF provides many features that may not have been available in the old library.
---

## Licensing Comparison

| Aspect | MuPDF AGPL | MuPDF Commercial | IronPDF |
|--------|------------|------------------|---------|
| Open-source apps | Free | Not needed | Requires license |
| Proprietary apps | Must open-source | Required | Requires license |
| SaaS applications | Must open-source | Required | Requires license |
| Pricing | Free | Contact sales | Published pricing |
| Source disclosure | Required | Not required | Not required |
| Derivative works | Must be AGPL | Negotiable | No restrictions |

---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [HTML to PDF Tutorial](https://ironpdf.com/tutorials/html-to-pdf/)
- [PDF Image Rendering Guide](https://ironpdf.com/how-to/rasterize-pdf-to-image/)
- [MuPDF AGPL License Text](https://www.gnu.org/licenses/agpl-3.0.html)
