# How Do I Migrate from a PDFium .NET Wrapper to IronPDF in C#?

## Why Migrate from a PDFium wrapper to IronPDF?

There is no official Google-supplied .NET binding for PDFium. Instead, the .NET ecosystem ships several community wrappers around the C++ engine: `PdfiumViewer` (Apache 2.0; archived August 2019, last NuGet release 2.13.0 in November 2017), the maintained `PdfiumViewer.Updated` fork (.NET Core / .NET 6), `PDFiumCore` (Dtronix, .NET Standard 2.1 P/Invoke bindings), and the commercial `Pdfium.Net.SDK` from Patagames. Their APIs differ but they share one trait: PDFium itself is a rendering and parsing engine, not an HTML-to-PDF or document-authoring engine. While the wrappers excel at displaying PDFs, none of them can convert HTML to PDF—because PDFium has no HTML parser.

This guide uses `PdfiumViewer` as the representative wrapper for "before" snippets because its API is the most widely cited; the migration approach is the same for any of the four.

### Critical PDFium Wrapper Limitations

1. **Rendering-First**: PDFium has no HTML parser, so no wrapper can create PDFs from HTML or URLs
2. **Limited Manipulation in Open-Source Wrappers**: PdfiumViewer / PDFiumCore expose viewing and text extraction; merge / split / form-edit is partial in Patagames Pdfium.Net.SDK and largely absent in the free wrappers
3. **Native Binary Dependencies**: Requires platform-specific PDFium binaries
4. **Deployment Complexity**: Must bundle and manage native binaries per platform
5. **Text Extraction**: PdfiumViewer exposes per-page raw text via `GetPdfText`; layout/format information is minimal
6. **No HTML to PDF**: Cannot convert web content to PDF
7. **No Built-in Headers/Footers**: No high-level page-overlay API
8. **No Built-in Watermarks**: No stamping primitive
9. **Forms**: Read-only or none in the free wrappers
10. **Security**: Encryption / permissions not exposed by the free wrappers

### IronPDF Advantages

| Aspect | PDFium wrappers | IronPDF |
|--------|-----------------|---------|
| **Primary Focus** | Rendering/viewing | Complete PDF solution |
| **PDF Creation from HTML** | None | Yes (Chromium engine) |
| **PDF Manipulation** | Limited (varies) | Yes (merge, split, edit) |
| **HTML to PDF** | None | Yes |
| **Watermarks** | Not built-in | Yes |
| **Headers/Footers** | Not built-in | Yes |
| **Form Filling** | Patagames only | Yes |
| **Security** | Patagames only | Yes |
| **Native Dependencies** | Required | Bundled / NuGet-managed |
| **Cross-Platform** | Manual native binary management | Automatic |

---

## NuGet Package Changes

```bash
# Remove whichever PDFium wrapper you used:
#   PdfiumViewer            (Apache 2.0 - archived 2019, .NET Framework only)
#   PdfiumViewer.Updated    (community fork, .NET Core / .NET 6)
#   PDFiumCore              (Dtronix - .NET Standard 2.1 P/Invoke bindings)
#   Pdfium.Net.SDK          (Patagames - commercial, perpetual license)
dotnet remove package PdfiumViewer
dotnet remove package PdfiumViewer.Updated
dotnet remove package PDFiumCore
dotnet remove package Pdfium.Net.SDK

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// PDFium wrappers (use the one you installed)
using PdfiumViewer;             // PdfiumViewer / PdfiumViewer.Updated
using PDFiumCore;               // Dtronix PDFiumCore (P/Invoke bindings)
using Patagames.Pdf;            // Patagames Pdfium.Net.SDK
using Patagames.Pdf.Net;        // Patagames Pdfium.Net.SDK

// IronPDF
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
```

---

## Complete API Reference

### Core Class Mappings

| PdfiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `PdfiumViewer.PdfDocument` | `IronPdf.PdfDocument` | Same simple name in different namespaces |
| `PdfRenderer` (WinForms control) | _(not applicable)_ | PdfiumViewer also ships UI controls; IronPDF is headless |
| _(not available)_ | `ChromePdfRenderer` | HTML / URL → PDF |
| _(not available)_ | `HtmlHeaderFooter` | Headers/footers |

### Document Loading

| PdfiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `PdfDocument.Load(stream)` | `PdfDocument.FromStream(stream)` | Load from stream |
| _(not provided directly; wrap bytes in MemoryStream)_ | `PdfDocument.FromBinaryData(bytes)` | Load from bytes |

### Document Properties

| PdfiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `document.PageCount` | `document.PageCount` | Same |
| `document.PageSizes` (`IList<SizeF>`) | `document.Pages[index].Width / Height` | PdfiumViewer exposes sizes; pages are not first-class objects |
| `document.PageSizes[i].Width` | `document.Pages[i].Width` | Per-page width in points |

### Page Rendering

| PdfiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `document.Render(page, width, height, dpiX, dpiY, flags)` | `pdf.RasterizeToImageFiles(path, DPI)` | Rasterize |
| `document.Render(page, ...)` returns `Image` | DPI parameter | Quality control via DPI |
| (loop calling `document.Render(i, ...)`) | `pdf.RasterizeToImageFiles("page_*.png")` | Batch render across all pages |
| Manual scale math | `DPI = 72 * scale` | Scale-to-DPI conversion |

### Text Extraction

| PdfiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `document.GetPdfText(pageIndex)` | `document.Pages[index].Text` | Per-page |
| _(manual loop)_ | `document.ExtractAllText()` | All pages |

### Saving Documents

| PdfiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `document.Save(stream)` | `document.SaveAs(path)` | PdfiumViewer takes a Stream; IronPDF takes a path |
| _(not available)_ | `document.Stream` | Access raw stream |
| _(not available)_ | `document.BinaryData` | Get bytes |

### NEW Features (Not in PDFium wrappers)

| IronPDF Feature | Description |
|-----------------|-------------|
| `ChromePdfRenderer.RenderHtmlAsPdf()` | Create from HTML |
| `ChromePdfRenderer.RenderUrlAsPdf()` | Create from URL |
| `ChromePdfRenderer.RenderHtmlFileAsPdf()` | Create from HTML file |
| `PdfDocument.Merge()` | Combine PDFs |
| `pdf.CopyPages()` | Extract pages |
| `pdf.RemovePages()` | Delete pages |
| `pdf.InsertPdf()` | Insert PDF at position |
| `pdf.ApplyWatermark()` | Add watermarks |
| `pdf.AddHtmlHeaders()` | Add headers |
| `pdf.AddHtmlFooters()` | Add footers |
| `pdf.SecuritySettings` | Password protection |
| `pdf.SignWithDigitalSignature()` | Digital signatures |
| `pdf.Form` | Form filling |

---

## Code Migration Examples

### Example 1: Load and Render PDF to Image

**Before (PdfiumViewer):**
```csharp
using PdfiumViewer;
using System.Drawing;

public class PdfRenderService
{
    public void RenderPdfToImages(string pdfPath, string outputFolder)
    {
        using (var document = PdfDocument.Load(pdfPath))
        {
            for (int i = 0; i < document.PageCount; i++)
            {
                // PdfiumViewer exposes per-page sizes via document.PageSizes
                var size = document.PageSizes[i];
                int width = (int)(size.Width * 2);   // 2x scale
                int height = (int)(size.Height * 2);

                using (var bitmap = document.Render(i, width, height, 96, 96, PdfRenderFlags.Annotations))
                {
                    bitmap.Save($"{outputFolder}/page_{i + 1}.png");
                }
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfRenderService
{
    public PdfRenderService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    }

    public void RenderPdfToImages(string pdfPath, string outputFolder)
    {
        var pdf = PdfDocument.FromFile(pdfPath);

        // Render all pages at 150 DPI (2x of 72 DPI default)
        pdf.RasterizeToImageFiles($"{outputFolder}/page_*.png", DPI: 150);
    }
}
```

### Example 2: Extract Text from PDF

**Before (PdfiumViewer):**
```csharp
using PdfiumViewer;
using System.Text;

public string ExtractText(string pdfPath)
{
    var sb = new StringBuilder();

    using (var document = PdfDocument.Load(pdfPath))
    {
        for (int i = 0; i < document.PageCount; i++)
        {
            string pageText = document.GetPdfText(i);
            sb.AppendLine(pageText);
        }
    }

    return sb.ToString();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public string ExtractText(string pdfPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    return pdf.ExtractAllText();
}

// Or per-page if needed
public string ExtractTextPerPage(string pdfPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    var sb = new StringBuilder();

    foreach (var page in pdf.Pages)
    {
        sb.AppendLine(page.Text);
    }

    return sb.ToString();
}
```

### Example 3: Get Page Dimensions

**Before (PdfiumViewer):**
```csharp
using PdfiumViewer;

public void GetPageInfo(string pdfPath)
{
    using (var document = PdfDocument.Load(pdfPath))
    {
        Console.WriteLine($"Total pages: {document.PageCount}");

        for (int i = 0; i < document.PageCount; i++)
        {
            var size = document.PageSizes[i];
            Console.WriteLine($"Page {i + 1}: {size.Width} x {size.Height} points");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void GetPageInfo(string pdfPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    Console.WriteLine($"Total pages: {pdf.PageCount}");

    for (int i = 0; i < pdf.PageCount; i++)
    {
        var page = pdf.Pages[i];
        Console.WriteLine($"Page {i + 1}: {page.Width} x {page.Height} points");
    }
}
```

### Example 4: Create PDF from HTML (NEW Feature)

**Before (PDFium wrapper):**
```csharp
// PDFium has no HTML parser - no wrapper can create PDFs from HTML.
// You need a separate library (e.g. wkhtmltopdf, headless Chromium, IronPDF).
throw new NotSupportedException("PDFium cannot create PDFs from HTML");
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreatePdfFromHtml(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    // Configure options
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.MarginTop = 20;
    renderer.RenderingOptions.MarginBottom = 20;

    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 5: Create PDF from URL (NEW Feature)

**Before (PDFium wrapper):**
```csharp
// PDFium has no HTTP client and no HTML parser.
throw new NotSupportedException("PDFium cannot convert URLs to PDF");
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CaptureWebPage(string url, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PrintHtmlBackgrounds = true;
    renderer.RenderingOptions.WaitFor.RenderDelay(1000); // Wait for JS

    var pdf = renderer.RenderUrlAsPdf(url);
    pdf.SaveAs(outputPath);
}
```

### Example 6: Merge PDFs (NEW Feature)

**Before (PDFium wrapper):**
```csharp
// Open-source PDFium wrappers (PdfiumViewer, PDFiumCore) do not expose
// document-merge APIs. Use a separate library (PdfSharp, iText) - or move to IronPDF.
throw new NotSupportedException("PdfiumViewer / PDFiumCore cannot merge PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;

public void MergePdfs(List<string> inputPaths, string outputPath)
{
    var pdfs = inputPaths.Select(path => PdfDocument.FromFile(path)).ToList();
    var merged = PdfDocument.Merge(pdfs);
    merged.SaveAs(outputPath);
}
```

### Example 7: Add Watermark (NEW Feature)

**Before (PDFium wrapper):**
```csharp
// PDFium has no high-level watermark/stamp API.
throw new NotSupportedException("PDFium wrappers cannot add watermarks");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

public void AddWatermark(string pdfPath, string outputPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    pdf.ApplyWatermark(
        "<div style='color:red; font-size:72px; opacity:0.3; transform:rotate(-45deg);'>CONFIDENTIAL</div>",
        45,
        VerticalAlignment.Middle,
        HorizontalAlignment.Center);

    pdf.SaveAs(outputPath);
}
```

### Example 8: Add Headers and Footers (NEW Feature)

**Before (PDFium wrapper):**
```csharp
// PDFium has no headers/footers primitive.
throw new NotSupportedException("PDFium wrappers cannot add headers/footers");
```

**After (IronPDF):**
```csharp
using IronPdf;

public void AddHeadersFooters(string pdfPath, string outputPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    pdf.AddHtmlHeaders(new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center; font-size:12px;'>Company Report</div>",
        MaxHeight = 25
    });

    pdf.AddHtmlFooters(new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center; font-size:10px;'>Page {page} of {total-pages}</div>",
        MaxHeight = 25
    });

    pdf.SaveAs(outputPath);
}
```

### Example 9: Password Protection (NEW Feature)

**Before (PDFium wrapper):**
```csharp
// Free PDFium wrappers do not expose encryption/permissions APIs.
// (Patagames Pdfium.Net.SDK exposes some security features.)
throw new NotSupportedException("PdfiumViewer / PDFiumCore cannot encrypt PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;

public void SecurePdf(string pdfPath, string outputPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    pdf.SecuritySettings.OwnerPassword = "admin123";
    pdf.SecuritySettings.UserPassword = "user456";
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;
    pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.NoPrint;

    pdf.SaveAs(outputPath);
}
```

### Example 10: Fill PDF Form (NEW Feature)

**Before (PDFium wrapper):**
```csharp
// Free PDFium wrappers do not expose form-fill APIs.
throw new NotSupportedException("PdfiumViewer / PDFiumCore cannot fill PDF forms");
```

**After (IronPDF):**
```csharp
using IronPdf;

public void FillForm(string pdfPath, string outputPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    // Fill form fields by name
    pdf.Form.GetFieldByName("FirstName").Value = "John";
    pdf.Form.GetFieldByName("LastName").Value = "Doe";
    pdf.Form.GetFieldByName("Email").Value = "john@example.com";

    pdf.SaveAs(outputPath);
}
```

---

## DPI and Scaling Conversion

### PDFium Scale to IronPDF DPI

```csharp
// PdfiumViewer's Render(...) takes explicit dpiX/dpiY (96 default).
// PDFium's underlying rendering is scale-driven (1.0 = 72 DPI = 1 pt per pixel).
// IronPDF uses DPI directly.

// Conversion formula:
// IronPDF DPI = 72 * PDFium scale factor

// Examples:
// PDFium scale 1.0 → IronPDF DPI 72
// PDFium scale 2.0 → IronPDF DPI 144
// PDFium scale 3.0 → IronPDF DPI 216
// PDFium scale 4.0 → IronPDF DPI 300

// Migration helper:
public int ScaleToDpi(float scale) => (int)(72 * scale);
```

The [detailed migration reference](https://ironpdf.com/blog/migration-guides/migrate-from-pdfium-to-ironpdf/) provides DPI conversion tables and rendering quality comparison guidelines.

---

## Native Dependency Removal

### Before (PDFium wrapper) - Complex Deployment

```
MyApp/
├── bin/
│   ├── MyApp.dll
│   ├── PdfiumViewer.dll      # or PDFiumCore.dll / Patagames.Pdf.dll
│   ├── x86/
│   │   └── pdfium.dll
│   └── x64/
│       └── pdfium.dll
├── runtimes/
│   ├── win-x86/native/
│   │   └── pdfium.dll
│   ├── win-x64/native/
│   │   └── pdfium.dll
│   ├── linux-x64/native/
│   │   └── libpdfium.so
│   └── osx-x64/native/
│       └── libpdfium.dylib
```

### After (IronPDF) - Clean Deployment

```
MyApp/
├── bin/
│   ├── MyApp.dll
│   └── IronPdf.dll  # Everything included
```

### Remove Native Binary References

```bash
# Delete native PDFium binaries
rm -rf x86/ x64/ runtimes/

# Remove from .csproj
# Delete any <Content Include="pdfium.dll" /> entries
# Delete any <None Include="x86/pdfium.dll" /> entries
```

---

## Common Migration Gotchas

### 1. Render Method → RasterizeToImageFiles

```csharp
// PdfiumViewer: per-page Render call with explicit pixel size + dpi
document.Render(0, 1024, 768, 96, 96, PdfRenderFlags.None);

// IronPDF: DPI-based rendering across the whole document
pdf.RasterizeToImageFiles("*.png", DPI: 150);
```

### 2. Disposal Pattern Changes

```csharp
// PdfiumViewer: explicit disposal of document and the rendered Image
using (var document = PdfDocument.Load(path))
using (var bitmap = document.Render(0, 1024, 768, 96, 96, PdfRenderFlags.None))
{
    bitmap.Save("output.png");
}

// IronPDF: Simplified
var pdf = PdfDocument.FromFile(path);
pdf.RasterizeToImageFiles("output.png");
```

### 3. Page Access

```csharp
// PdfiumViewer: pages are NOT first-class disposable objects.
// Use document.PageSizes[i] for size and document.GetPdfText(i) for text.
var size = document.PageSizes[0];

// IronPDF: Direct property access on a Pages collection
var page = pdf.Pages[0];
```

### 4. Platform-Specific Code Removal

```csharp
// PDFium wrapper: Required platform detection / RID-specific binaries
#if WIN64
    // Load x64 pdfium.dll
#else
    // Load x86 pdfium.dll
#endif

// IronPDF: Remove all platform-specific code
// Just use the API directly
```

### 5. PdfRenderFlags → RenderingOptions

```csharp
// PdfiumViewer
document.Render(0, width, height, 96, 96,
    PdfRenderFlags.Annotations | PdfRenderFlags.ForPrinting);

// IronPDF: Configuration happens at render time
// Annotations and print quality are default behaviors
```

---

## Feature Comparison Summary

| Feature | PDFium wrappers | IronPDF |
|---------|-----------------|---------|
| Load PDF | Yes | Yes |
| Render to Image | Yes | Yes |
| Extract Text | Yes (basic, per-page) | Yes (logical / visual order) |
| Page Info | Yes | Yes |
| Create from HTML | None | Yes |
| Create from URL | None | Yes |
| Merge PDFs | Free wrappers: no; Patagames: partial | Yes |
| Split PDFs | Free wrappers: no; Patagames: partial | Yes |
| Add Watermarks | Not built-in | Yes |
| Headers/Footers | Not built-in | Yes |
| Form Filling | Free wrappers: no; Patagames: yes | Yes |
| Digital Signatures | Free wrappers: no; Patagames: yes | Yes |
| Password Protection | Free wrappers: no; Patagames: yes | Yes |
| Native Dependencies | Required | Bundled / NuGet-managed |
| Cross-Platform | Complex (per-RID native binaries) | Automatic |
| Memory Management | Manual disposal | Simplified |

---

## Pre-Migration Checklist

- [ ] Identify which PDFium wrapper is in use (PdfiumViewer / PdfiumViewer.Updated / PDFiumCore / Pdfium.Net.SDK)
- [ ] Document current rendering dimensions / DPI / scales used
- [ ] List native binary locations in project (per RID)
- [ ] Check for platform-specific loading code
- [ ] Identify PDF creation needs (currently using separate tools?)
- [ ] Review disposal patterns for conversion
- [ ] Check build scripts for native binary copying

---

## Post-Migration Checklist

- [ ] Remove the PDFium wrapper NuGet package(s)
- [ ] Delete native pdfium.dll / .so / .dylib binaries
- [ ] Remove platform-specific conditional compilation
- [ ] Remove runtimes folder from project
- [ ] Update .csproj to remove native binary references
- [ ] Add IronPDF NuGet package
- [ ] Set IronPDF license key
- [ ] Convert scale factors to DPI values
- [ ] Simplify disposal patterns
- [ ] Test rendering output quality
- [ ] Test cross-platform deployment

---

## Finding PDFium References

```bash
# Find PDFium wrapper usage
grep -rE "PdfiumViewer|PDFiumCore|Patagames\.Pdf|PdfDocument\.Load|\.Render\(" --include="*.cs" .

# Find native binary references
grep -rE "pdfium\.dll|libpdfium\.(so|dylib)" --include="*.csproj" --include="*.config" .

# Find platform-specific code
grep -rE "#if.*64|WIN32|WIN64|LINUX|OSX" --include="*.cs" .
```

---

## Troubleshooting

### "License key required"

```csharp
// Set at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### "Rendered image quality different"

Convert PDFium scale (or PdfiumViewer's dpiX/dpiY × pixel-size math) to equivalent IronPDF DPI:

```csharp
// If PDFium effectively used scale 2.0, use DPI 144
pdf.RasterizeToImageFiles("*.png", DPI: 144);
```

### "Page dimensions different"

Both use points (1/72 inch). Dimensions should be identical—verify you're accessing the correct properties.

### "Text extraction returns different results"

IronPDF uses more advanced text extraction. Results may be more accurate but formatted differently.

---

## Performance Notes

| Operation | PDFium wrappers | IronPDF |
|-----------|-----------------|---------|
| First PDF load | Fast (native) | Moderate (managed) |
| Page rendering | Very fast | Fast |
| Text extraction | Basic (per page) | Comprehensive (logical / visual order) |
| Memory usage | Lower | Higher (Chromium init) |
| Startup time | Fast | Moderate (Chromium init) |

IronPDF trades some raw rendering performance for vastly expanded capabilities (HTML/URL → PDF, merge, watermark, headers/footers, security, signatures, forms).

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PDFium wrapper usages in codebase**
  ```bash
  grep -rE "using PdfiumViewer|using PDFiumCore|using Patagames\.Pdf" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package PdfiumViewer       # or PDFiumCore / Pdfium.Net.SDK
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace PDF generation calls**
  ```csharp
  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering.

- [ ] **Update page settings**
  ```csharp
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Configure headers/footers**
  ```csharp
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine provides modern rendering. Verify key documents.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  var merged = PdfDocument.Merge(pdf1, pdf2);
  pdf.SecuritySettings.UserPassword = "secret";
  pdf.ApplyWatermark("<h1>DRAFT</h1>");
  ```
  **Why:** IronPDF provides many additional features.

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Tutorial](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [API Reference](https://ironpdf.com/object-reference/api/)
- [Rendering PDFs Guide](https://ironpdf.com/docs/questions/rasterize/)
