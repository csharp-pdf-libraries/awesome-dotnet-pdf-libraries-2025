# How Do I Migrate from MuPDF (.NET Bindings) to IronPDF in C#?

## Why Migrate?

MuPDF is an excellent PDF toolkit, but its dual AGPL/commercial license (Artifex) and rendering-first focus create real friction for .NET developers building commercial HTML-driven applications. The two main .NET wrappers are **MuPDFCore** (community, by arklumpus, AGPL-3.0) and **MuPDF.NET** (Artifex's official C# bindings, mirrored on PyMuPDF). This guide covers migration from either wrapper to IronPDF.

### Critical Issues with MuPDF

1. **AGPL License Trap (or Commercial Contract)**: Both MuPDF itself and the wrappers ship under AGPL-3.0. Distributing a closed-source product on AGPL code requires either:
   - Open-sourcing your entire application under AGPL, or
   - Purchasing a commercial license from Artifex (pricing is quote-based, not published).

2. **No HTML-to-PDF Engine**: MuPDF and its .NET wrappers do not include an HTML/CSS renderer. Generating PDFs from HTML requires shelling out to a separate browser engine (Chromium, wkhtmltopdf) and only loading the result with MuPDF.

3. **Native Dependencies**: Both wrappers ship `MuPDFCore.NativeAssets` / `MuPDF.NativeAssets` packages with per-RID binaries (win-x64, linux-x64, osx-x64, etc.). Cross-platform deployment works, but Docker images need the matching native package and base image (glibc vs musl, etc.).

4. **API Surface Skewed Toward Reading**: While MuPDF.NET *can* merge, redact, sign, and add watermarks (it mirrors PyMuPDF), most of the API and tooling is shaped around viewing and structured-text extraction rather than HTML-driven document generation. There is no equivalent of `ChromePdfRenderer.RenderHtmlAsPdf`.

5. **C Interop Surface**: Wrappers expose context/document lifetimes that map onto the underlying C library, so:
   - Disposal order matters
   - Native crashes can take down the whole process
   - Marshalling cost is non-trivial for large documents

These challenges, as detailed in the [full migration reference](https://ironpdf.com/blog/migration-guides/migrate-from-mupdf-to-ironpdf/), demonstrate why many teams transition to fully-managed solutions with comprehensive PDF capabilities.

### Benefits of IronPDF

| Aspect | MuPDF (.NET wrappers) | IronPDF |
|--------|-----------------------|---------|
| License | AGPL-3.0 or quote-based commercial (Artifex) | Commercial with published pricing |
| Primary Focus | Rendering, structured text, low-level manipulation | HTML-driven PDF generation + manipulation |
| HTML to PDF | Not supported (no HTML renderer) | Full Chromium engine |
| PDF Creation | Page-level only (insert text/images/vectors) | HTML, URL, images |
| Merge / Split / Reorder | Supported in MuPDF.NET (`InsertPdf`, `CopyPage`, `Select`) | Supported (`Merge`, `CopyPages`, `RemovePages`) |
| Dependencies | Native MuPDF library + RID-specific NativeAssets | Managed wrapper + bundled Chromium |
| Platform Support | Win/Linux/macOS via NativeAssets | Win/Linux/macOS/Docker |
| Async Support | Mostly synchronous; some async in MuPDFCore (e.g., `GetStructuredTextPageAsync`) | Async APIs across renderer and document |
| .NET Integration | P/Invoke wrappers around C library | .NET wrapper + native Chromium |

---

## NuGet Package Changes

```bash
# Remove MuPDF packages (whichever wrapper you used)
dotnet remove package MuPDF.NET
dotnet remove package MuPDFCore
dotnet remove package MuPDFCore.NativeAssets.Linux
dotnet remove package MuPDFCore.NativeAssets.MacOS
dotnet remove package MuPDFCore.NativeAssets.Windows
dotnet remove package MuPDF.NativeAssets

# Install IronPDF
dotnet add package IronPdf
```

**Also remove native MuPDF binaries** from your deployment:
- Delete `mupdf.dll`, `libmupdf.so`, `libmupdf.dylib`
- Remove platform-specific runtime folders
- Update Docker files to drop MuPDF installation steps

---

## Namespace Changes

| MuPDF wrapper | IronPDF |
|---------------|---------|
| `using MuPDFCore;` (arklumpus wrapper) | `using IronPdf;` |
| `using MuPDFCore.StructuredText;` | `using IronPdf;` |
| `using MuPDF.NET;` (Artifex wrapper) | `using IronPdf;` |
| | `using IronPdf.Rendering;` (for enums) |

---

## Complete API Mapping

> The MuPDF column lists the **MuPDF.NET** (Artifex) API by default and notes the **MuPDFCore** (arklumpus) equivalent where it differs.

### Document Loading

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `new Document(path)` (MuPDF.NET) / `new MuPDFDocument(ctx, path)` (MuPDFCore) | `PdfDocument.FromFile(path)` | Load from file |
| `new Document("pdf", stream)` (MuPDF.NET) | `PdfDocument.FromStream(stream)` | Load from stream |
| `new Document(...)` from byte[] | `PdfDocument.FromBinaryData(bytes)` | Load from bytes |
| `doc.Close()` / `Dispose()` | `pdf.Dispose()` | Cleanup |

### Page Access

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `doc.PageCount` (MuPDF.NET) / `document.Pages.Count` (MuPDFCore) | `pdf.PageCount` | Page count |
| `doc[index]` (MuPDF.NET) / `document.Pages[index]` (MuPDFCore) | `pdf.Pages[index]` | Access page |
| `doc.LoadPage(index)` | `pdf.Pages[index]` | Load specific page |
| `page.Rect.Width` | `page.Width` | Page width |
| `page.Rect.Height` | `page.Height` | Page height |

### Rendering to Images

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `page.GetPixmap(matrix)` (MuPDF.NET) / `page.Render(...)` (MuPDFCore) | `pdf.RasterizeToImageFiles(path, dpi)` | Render page to image |
| `pixmap.WritePng(path)` (MuPDF.NET) / `MuPDFDocument.SaveImage(...)` (MuPDFCore) | `pdf.RasterizeToImageFiles("*.png")` | Save as PNG |
| `pixmap.WriteImage(path, "jpg")` | `pdf.RasterizeToImageFiles("*.jpg")` | Save as JPEG |

### Text Extraction

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `page.GetText()` (MuPDF.NET) / iterate `MuPDFStructuredTextPage` (MuPDFCore) | `pdf.Pages[i].Text` | Page text |
| Loop over `doc[i].GetText()` | `pdf.ExtractAllText()` | All text |
| `page.GetText("html")` / `"json"` | `pdf.ExtractTextFromPage(index)` | Per-page text |

### PDF Creation (Not in MuPDF)

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| _(no HTML engine)_ | `ChromePdfRenderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| _(no HTML engine)_ | `ChromePdfRenderer.RenderUrlAsPdf(url)` | URL to PDF |
| _(no HTML engine)_ | `ChromePdfRenderer.RenderHtmlFileAsPdf(path)` | HTML file to PDF |
| `page.InsertImage(rect, fileName: ...)` (per page) | `ImageToPdfConverter.ImageToPdf(imagePaths)` | Images to PDF |

### PDF Manipulation

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `docA.InsertPdf(docB)` (MuPDF.NET) | `PdfDocument.Merge(pdf1, pdf2)` | Merge PDFs |
| `doc.Select(new[]{0,1,2})` | `pdf.CopyPages(start, end)` | Extract / keep page range |
| `docA.InsertPdf(docB, fromPage, toPage, startAt)` | `pdf.InsertPdf(otherPdf, index)` | Insert pages at offset |
| `doc.DeletePage(i)` / `doc.DeletePages(...)` | `pdf.RemovePages(indices)` | Remove pages |
| `page.SetRotation(90)` | `pdf.RotatePages(angle)` | Rotate pages |

### Document Properties

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `doc.MetaData` | `pdf.MetaData` | Document metadata |
| `doc.SetMetadata(...)` | `pdf.MetaData.Title = "..."` | Set title |
| `doc.SetMetadata(...)` | `pdf.MetaData.Author = "..."` | Set author |

### Security

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `doc.Save(path, encryption: ..., ownerPw: ..., userPw: ...)` | `pdf.SecuritySettings.UserPassword = "pass"` | Set password |
| `doc.Save(...)` permissions flags | `pdf.SecuritySettings` | Configure permissions |
| `widget.Sign(pkcs12Signer)` (MuPDF.NET signing API) | `pdf.SignWithFile(certPath, password)` | Digital signatures |

### Output

| MuPDF | IronPDF | Notes |
|-------|---------|-------|
| `doc.Save(path)` | `pdf.SaveAs(path)` | Save to file |
| `doc.Write()` returns byte[] | `pdf.BinaryData` | Get byte array |
| Wrap `doc.Write()` in MemoryStream | `pdf.Stream` | Get stream |

---

## Code Migration Examples

### Example 1: Loading and Rendering PDF Pages

**Before (MuPDFCore):**
```csharp
using MuPDFCore;
using System.IO;

public class MuPdfRenderer
{
    public void RenderPdfToImages(string pdfPath, string outputFolder)
    {
        using (var context = new MuPDFContext())
        using (var document = new MuPDFDocument(context, pdfPath))
        {
            for (int i = 0; i < document.Pages.Count; i++)
            {
                // Render each page to PNG at scale 150/72.
                var outputPath = Path.Combine(outputFolder, $"page_{i + 1}.png");
                document.SaveImage(
                    pageNumber: i,
                    zoom: 150.0 / 72.0,
                    pixelFormat: PixelFormats.RGBA,
                    fileName: outputPath,
                    fileType: RasterOutputFileTypes.PNG);
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

**Before (MuPDF.NET):**
```csharp
using MuPDF.NET;
using System.Text;

public class MuPdfTextExtractor
{
    public string ExtractText(string pdfPath)
    {
        var sb = new StringBuilder();

        Document doc = new Document(pdfPath);

        for (int i = 0; i < doc.PageCount; i++)
        {
            string text = doc[i].GetText();
            sb.AppendLine($"--- Page {i + 1} ---");
            sb.AppendLine(text);
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

### Example 3: PDF Creation from HTML (Not Possible in MuPDF)

**Before (MuPDF):**
```csharp
// Neither MuPDF.NET nor MuPDFCore ships an HTML/CSS engine.
// In practice teams render HTML upstream (Chromium, wkhtmltopdf,
// PrinceXML, etc.) and only load the resulting PDF with MuPDF.
throw new NotSupportedException(
    "MuPDF has no HTML-to-PDF renderer; render HTML out-of-process first.");
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

### Example 4: Merging PDFs

**Before (MuPDF.NET):**
```csharp
using MuPDF.NET;

// MuPDF.NET supports merging via Document.InsertPdf, mirroring
// PyMuPDF's insert_pdf().
public class MuPdfMerger
{
    public void MergePdfs(string[] inputPaths, string outputPath)
    {
        Document outputDoc = new Document();

        foreach (var inputPath in inputPaths)
        {
            Document inputDoc = new Document(inputPath);
            outputDoc.InsertPdf(inputDoc);
        }

        outputDoc.Save(outputPath);
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

**Before (MuPDF.NET):**
```csharp
using MuPDF.NET;
using System;

public class MuPdfPageInfo
{
    public void PrintPageInfo(string pdfPath)
    {
        Document doc = new Document(pdfPath);
        Console.WriteLine($"Total pages: {doc.PageCount}");

        for (int i = 0; i < doc.PageCount; i++)
        {
            var rect = doc[i].Rect;
            Console.WriteLine($"Page {i + 1}: {rect.Width}x{rect.Height}");
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

**Before (MuPDFCore):**
```csharp
using MuPDFCore;
using System;
using System.IO;

public class MuPdfPageRenderer
{
    public byte[] RenderPageToBytes(string pdfPath, int pageIndex, int dpi)
    {
        using (var context = new MuPDFContext())
        using (var document = new MuPDFDocument(context, pdfPath))
        {
            if (pageIndex >= document.Pages.Count)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));

            // SaveImage writes directly to disk; for an in-memory byte[]
            // route through a temp file or use the WriteImage stream overload.
            string tempPath = Path.GetTempFileName() + ".png";
            document.SaveImage(
                pageNumber: pageIndex,
                zoom: dpi / 72.0,
                pixelFormat: PixelFormats.RGBA,
                fileName: tempPath,
                fileType: RasterOutputFileTypes.PNG);

            byte[] bytes = File.ReadAllBytes(tempPath);
            File.Delete(tempPath);
            return bytes;
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

### Example 7: Adding Watermarks

**Before (MuPDF.NET):**
```csharp
using MuPDF.NET;

// MuPDF.NET can stamp watermarks by inserting an image or text on
// each page, but there is no HTML/CSS watermark API; you build the
// image yourself and place it.
public class MuPdfWatermarker
{
    public void AddWatermark(string inputPath, string outputPath, string stampImagePath)
    {
        Document doc = new Document(inputPath);
        for (int i = 0; i < doc.PageCount; i++)
        {
            var page = doc[i];
            page.InsertImage(page.Rect, fileName: stampImagePath, overlay: true);
        }
        doc.Save(outputPath);
    }
}
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
// MuPDF has no first-class HTML header/footer with token substitution
// (page number, total pages, etc.). You can manually call
// page.InsertText(...) for each page, but there is no template engine
// for {page}/{total-pages} the way Chromium-based renderers expose.
throw new NotSupportedException(
    "MuPDF has no HTML header/footer template; emit text per page manually.");
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

### Example 9: Password Protection

**Before (MuPDF.NET):**
```csharp
using MuPDF.NET;

// MuPDF.NET can write encrypted PDFs by passing encryption flags and
// owner/user passwords to Save(); there is no document-level
// SecuritySettings object the way IronPDF exposes.
public class MuPdfSecurer
{
    public void ProtectPdf(string inputPath, string outputPath, string password)
    {
        Document doc = new Document(inputPath);
        // Encryption / permission flag values come from MuPDF constants.
        doc.Save(
            outputPath,
            encryption: 4,        // PDF_ENCRYPT_AES_256
            ownerPW: password,
            userPW: password,
            permissions: 0);      // strip all user permissions
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfSecurer
{
    public void ProtectPdf(string inputPath, string outputPath, string password)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
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

**Before (MuPDF.NET):**
```csharp
using MuPDF.NET;

// MuPDF.NET can read, extract, render, merge, and encrypt — but
// there is no HTML-to-PDF entry point, so document creation has to
// come from existing pages or images.
public class MuPdfWorkflow
{
    public void ProcessDocument(string inputPath, string previewPath)
    {
        Document doc = new Document(inputPath);

        // Text extraction
        string text = doc[0].GetText();

        // Render preview to PNG via Pixmap
        var pix = doc[0].GetPixmap();
        pix.WritePng(previewPath);

        // No HTML entry point: HTML-driven generation must happen
        // outside MuPDF and the resulting PDF then loaded here.
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

**Problem:** MuPDFCore requires an explicit `MuPDFContext`; MuPDF.NET hides it but still needs `Close()` / `Dispose()`.

```csharp
// MuPDFCore pattern:
using (var context = new MuPDFContext())
using (var document = new MuPDFDocument(context, path))
{
    // work with document
}

// MuPDF.NET pattern:
Document doc = new Document(path);
try { /* work */ } finally { doc.Close(); }

// IronPDF pattern (simpler):
var pdf = PdfDocument.FromFile(path);
// work with pdf
// dispose when needed or let GC handle
```

### Issue 2: Pixmap to Image Conversion

**Problem:** MuPDF renders to a `Pixmap` (or via `MuPDFDocument.SaveImage`), IronPDF returns standard .NET images.

```csharp
// MuPDF.NET:
var pixmap = doc[0].GetPixmap();
pixmap.WritePng("page0.png");

// MuPDFCore:
document.SaveImage(0, 2.0, PixelFormats.RGBA, "page0.png", RasterOutputFileTypes.PNG);

// IronPDF:
var images = pdf.ToBitmap(dpi);   // Returns System.Drawing.Bitmap[]
```

### Issue 3: Page Iteration Pattern

**Problem:** Different page access patterns

```csharp
// MuPDF.NET:
for (int i = 0; i < doc.PageCount; i++)
{
    var page = doc[i];
    // page lifetime is tied to the document
}

// MuPDFCore:
for (int i = 0; i < document.Pages.Count; i++)
{
    var page = document.Pages[i]; // page disposed with document
}

// IronPDF:
foreach (var page in pdf.Pages)
{
    // No manual disposal needed per page
}
```

### Issue 4: Missing HTML-to-PDF Path

**Problem:** MuPDF has no HTML/CSS rendering engine.

```csharp
// If you were combining MuPDF with another library for creation:
// Before: Use Chromium / wkhtmltopdf / PrinceXML → Load with MuPDF
// After: Use IronPDF's ChromePdfRenderer end-to-end
```

### Issue 5: DPI and Scale Differences

**Problem:** MuPDF takes a zoom/scale factor (1.0 = 72 DPI). IronPDF takes DPI directly.

```csharp
// MuPDFCore:
//   zoom 1.0 == 72 DPI
//   zoom 2.0 == 144 DPI
//   formula: zoom = dpi / 72.0

// IronPDF uses direct DPI:
pdf.RasterizeToImageFiles("output*.png", DPI: 150);
```

### Issue 6: Text Extraction Coordinates

**Problem:** MuPDF gives you a structured text tree (blocks → lines → characters with bounding boxes). IronPDF returns plain text by default.

```csharp
// MuPDF.NET:
//   doc[i].GetText("dict")    // structured dictionary
//   doc[i].GetText("words")   // list of (rect, word) tuples
//
// MuPDFCore:
//   document.GetStructuredTextPage(i) → MuPDFStructuredTextPage
//
// IronPDF:
//   pdf.ExtractTextFromPage(i)   // per-page plain text
//   pdf.Pages[i].Text            // page text
```

---

## Performance Comparison

The numbers below are illustrative — benchmark on your own documents and machine before relying on them. The qualitative ordering is more reliable than the absolute milliseconds.

| Operation | MuPDF | IronPDF |
|-----------|-------|---------|
| Load PDF (10 pages) | Fast | Fast |
| Render page to image | Fast (native) | Fast (Chromium-backed) |
| Extract text (all pages) | Fast | Fast |
| Create from HTML | Not supported | Single call, includes Chromium spin-up |
| Merge 2 PDFs | Single `InsertPdf` call | Single `Merge` call |
| Add watermark | Image stamp per page | Single `ApplyWatermark` call |
| First operation (init) | Native library load | Chromium warm-up (heavier) |
| Subsequent operations | Fast | Fast |

**Note:** MuPDF tends to be quicker for raw rendering and text extraction. IronPDF wins where the input is HTML/CSS, because MuPDF has no HTML engine at all.

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all MuPDF usages**
  ```bash
  grep -r "MuPDF\|MuPDFCore\|MuPDFDocument\|MuPDF.NET" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage. Both MuPDFCore (`MuPDFDocument`) and MuPDF.NET (`Document`) classes need to be found.

- [ ] **Document all rendering operations (DPI, zoom factors)**
  ```csharp
  // Before (MuPDFCore)
  double zoom = 1.5; // == 108 DPI
  document.SaveImage(0, zoom, PixelFormats.RGBA, "out.png", RasterOutputFileTypes.PNG);

  // After (IronPDF rasterization)
  pdf.RasterizeToImageFiles("out_*.png", DPI: 108);
  ```
  **Why:** MuPDF expresses scale as a zoom factor (1.0 = 72 DPI). Convert to DPI for IronPDF.

- [ ] **Identify any PDF creation needs (currently external)**
  **Why:** IronPDF can handle PDF creation internally, reducing dependencies on external tools.

- [ ] **List text extraction requirements**
  ```csharp
  // Before (MuPDF.NET)
  Document doc = new Document("document.pdf");
  string text = doc[0].GetText();

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("document.pdf");
  var text = pdf.ExtractAllText();
  ```
  **Why:** IronPDF returns whole-document plain text in one call instead of per-page iteration.

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
  dotnet remove package MuPDF.NET     # or MuPDFCore + native asset packages
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
  // Before (MuPDFCore)
  using MuPDFCore;
  // Before (MuPDF.NET)
  using MuPDF.NET;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Update namespaces to reflect the new library.

- [ ] **Replace `Document` / `MuPDFDocument` with `PdfDocument.FromFile()`**
  ```csharp
  // Before (MuPDF.NET)
  Document doc = new Document("document.pdf");
  // Before (MuPDFCore)
  var ctx = new MuPDFContext();
  var mupdfDoc = new MuPDFDocument(ctx, "document.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("document.pdf");
  ```
  **Why:** IronPDF's `PdfDocument` is the unified entry point.

- [ ] **Replace pixmap rendering with `RasterizeToImageFiles()`**
  ```csharp
  // Before (MuPDF.NET)
  var pixmap = doc[0].GetPixmap();
  pixmap.WritePng("page0.png");

  // After (IronPDF)
  pdf.RasterizeToImageFiles("page_*.png", DPI: 150);
  ```
  **Why:** IronPDF rasterizes the whole document in one call.

- [ ] **Replace page iteration patterns**
  ```csharp
  // Before (MuPDF.NET)
  for (int i = 0; i < doc.PageCount; i++) { var p = doc[i]; }

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

- [ ] **Consider adopting IronPDF features that are easier here than in MuPDF**
  ```csharp
  // Merge PDFs
  var merged = PdfDocument.Merge(pdf1, pdf2);

  // Password protection
  pdf.SecuritySettings.UserPassword = "secret";

  // HTML watermarks (no equivalent in MuPDF — MuPDF stamps images/text only)
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");

  // Digital signatures
  pdf.SignWithFile("cert.pfx", "password");
  ```
  **Why:** Many of these *are* possible in MuPDF.NET, but IronPDF's HTML-driven entry points (watermarks, headers, content) cut out the manual coordinate / page-stamping work.
---

## Licensing Comparison

| Aspect | MuPDF AGPL | MuPDF Commercial (Artifex) | IronPDF |
|--------|------------|----------------------------|---------|
| Open-source apps | Free (must remain AGPL-compatible) | Not required | Requires license |
| Proprietary apps | Must open-source under AGPL | Required | Requires license |
| SaaS applications | AGPL "network use" trigger applies | Required | Requires license |
| Pricing | Free | Contact Artifex (quote-based) | Published pricing |
| Source disclosure | Required (AGPL §13) | Not required | Not required |
| Derivative works | Must be AGPL | Per commercial agreement | Per IronPDF EULA |

---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [HTML to PDF Tutorial](https://ironpdf.com/tutorials/html-to-pdf/)
- [PDF Image Rendering Guide](https://ironpdf.com/how-to/rasterize-pdf-to-image/)
- [MuPDF AGPL License Text](https://www.gnu.org/licenses/agpl-3.0.html)
