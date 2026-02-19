# How Do I Migrate from Pdfium.NET to IronPDF in C#?

## Why Migrate from Pdfium.NET to IronPDF?

Pdfium.NET is a .NET wrapper around Google's PDFium library—excellent for PDF rendering but severely limited for modern application needs. While it excels at displaying PDFs, it cannot create, edit, or manipulate them.

### Critical Pdfium.NET Limitations

1. **Rendering-Only**: Cannot create PDFs from HTML, images, or programmatically
2. **No PDF Manipulation**: Cannot merge, split, or modify PDF content
3. **Native Binary Dependencies**: Requires platform-specific PDFium binaries
4. **Deployment Complexity**: Must bundle and manage native DLLs per platform
5. **Limited Text Extraction**: Basic text extraction without formatting
6. **No HTML to PDF**: Cannot convert web content to PDF
7. **No Headers/Footers**: Cannot add page numbers or repeating content
8. **No Watermarks**: Cannot stamp documents with overlays
9. **No Form Support**: Cannot fill or read PDF forms
10. **No Security Features**: Cannot encrypt or password-protect PDFs

### IronPDF Advantages

| Aspect | Pdfium.NET | IronPDF |
|--------|------------|---------|
| **Primary Focus** | Rendering/viewing | Complete PDF solution |
| **PDF Creation** | ✗ | ✓ (HTML, URL, images) |
| **PDF Manipulation** | ✗ | ✓ (merge, split, edit) |
| **HTML to PDF** | ✗ | ✓ (Chromium engine) |
| **Watermarks** | ✗ | ✓ |
| **Headers/Footers** | ✗ | ✓ |
| **Form Filling** | ✗ | ✓ |
| **Security** | ✗ | ✓ |
| **Native Dependencies** | Required | None (fully managed) |
| **Cross-Platform** | Complex setup | Automatic |

---

## NuGet Package Changes

```bash
# Remove Pdfium packages
dotnet remove package Pdfium.NET
dotnet remove package Pdfium.Net.SDK
dotnet remove package PdfiumViewer

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// Pdfium.NET
using Pdfium;
using Pdfium.Net;
using PdfiumViewer;

// IronPDF
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
```

---

## Complete API Reference

### Core Class Mappings

| Pdfium.NET | IronPDF | Notes |
|------------|---------|-------|
| `PdfDocument` | `PdfDocument` | Same name, different capabilities |
| `PdfPage` | `PdfPage` | Similar interface |
| `PdfPageCollection` | `PdfPageCollection` | Similar interface |
| _(not available)_ | `ChromePdfRenderer` | PDF creation |
| _(not available)_ | `HtmlHeaderFooter` | Headers/footers |

### Document Loading

| Pdfium.NET | IronPDF | Notes |
|------------|---------|-------|
| `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `PdfDocument.Load(stream)` | `PdfDocument.FromStream(stream)` | Load from stream |
| `PdfDocument.Load(bytes)` | `PdfDocument.FromBinaryData(bytes)` | Load from bytes |
| `new PdfDocument(path)` | `PdfDocument.FromFile(path)` | Constructor pattern |

### Document Properties

| Pdfium.NET | IronPDF | Notes |
|------------|---------|-------|
| `document.PageCount` | `document.PageCount` | Same |
| `document.Pages` | `document.Pages` | Similar collection |
| `document.Pages[index]` | `document.Pages[index]` | Zero-based |
| `document.GetPageSize(index)` | `document.Pages[index].Width/Height` | Direct properties |

### Page Rendering

| Pdfium.NET | IronPDF | Notes |
|------------|---------|-------|
| `page.Render(width, height)` | `pdf.RasterizeToImageFiles(path, dpi)` | Rasterize |
| `page.Render(width, height, flags)` | DPI parameter | Quality control |
| `document.Render(index, width, height)` | `pdf.RasterizeToImageFiles()` | Batch render |
| `page.RenderToScale(scale)` | DPI: `72 * scale` | Scale to DPI |

### Text Extraction

| Pdfium.NET | IronPDF | Notes |
|------------|---------|-------|
| `document.GetPdfText(pageIndex)` | `document.Pages[index].Text` | Per-page |
| _(manual loop)_ | `document.ExtractAllText()` | All pages |
| `page.GetTextBounds()` | `page.Text` | Simplified |

### Saving Documents

| Pdfium.NET | IronPDF | Notes |
|------------|---------|-------|
| `document.Save(path)` | `document.SaveAs(path)` | Different method name |
| `document.Save(stream)` | `document.Stream` | Access stream |
| _(not available)_ | `document.BinaryData` | Get bytes |

### NEW Features (Not in Pdfium.NET)

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

**Before (Pdfium.NET):**
```csharp
using Pdfium;
using System.Drawing;

public class PdfRenderService
{
    public void RenderPdfToImages(string pdfPath, string outputFolder)
    {
        using (var document = PdfDocument.Load(pdfPath))
        {
            for (int i = 0; i < document.PageCount; i++)
            {
                using (var page = document.Pages[i])
                {
                    // Render at specific size
                    int width = (int)(page.Width * 2); // 2x scale
                    int height = (int)(page.Height * 2);

                    using (var bitmap = page.Render(width, height, PdfRenderFlags.Annotations))
                    {
                        bitmap.Save($"{outputFolder}/page_{i + 1}.png");
                    }
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

**Before (Pdfium.NET):**
```csharp
using Pdfium;
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

**Before (Pdfium.NET):**
```csharp
using Pdfium;

public void GetPageInfo(string pdfPath)
{
    using (var document = PdfDocument.Load(pdfPath))
    {
        Console.WriteLine($"Total pages: {document.PageCount}");

        for (int i = 0; i < document.PageCount; i++)
        {
            using (var page = document.Pages[i])
            {
                double width = page.Width;
                double height = page.Height;
                Console.WriteLine($"Page {i + 1}: {width} x {height} points");
            }
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

**Before (Pdfium.NET):**
```csharp
// Pdfium.NET CANNOT create PDFs
// You need a separate library to generate PDFs
throw new NotSupportedException("Pdfium.NET cannot create PDFs from HTML");
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

**Before (Pdfium.NET):**
```csharp
// Pdfium.NET CANNOT convert URLs to PDF
throw new NotSupportedException("Pdfium.NET cannot convert URLs to PDF");
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

**Before (Pdfium.NET):**
```csharp
// Pdfium.NET CANNOT merge PDFs
// Must use separate library like iTextSharp or PdfSharp
throw new NotSupportedException("Pdfium.NET cannot merge PDFs");
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

**Before (Pdfium.NET):**
```csharp
// Pdfium.NET CANNOT add watermarks
throw new NotSupportedException("Pdfium.NET cannot add watermarks");
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

**Before (Pdfium.NET):**
```csharp
// Pdfium.NET CANNOT add headers/footers
throw new NotSupportedException("Pdfium.NET cannot add headers/footers");
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

**Before (Pdfium.NET):**
```csharp
// Pdfium.NET CANNOT encrypt PDFs
throw new NotSupportedException("Pdfium.NET cannot encrypt PDFs");
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

**Before (Pdfium.NET):**
```csharp
// Pdfium.NET CANNOT fill PDF forms
throw new NotSupportedException("Pdfium.NET cannot fill PDF forms");
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

### Pdfium.NET Scale to IronPDF DPI

```csharp
// Pdfium.NET uses scale factor (1.0 = 72 DPI)
// IronPDF uses DPI directly

// Conversion formula:
// IronPDF DPI = 72 * Pdfium scale factor

// Examples:
// Pdfium scale 1.0 → IronPDF DPI 72
// Pdfium scale 2.0 → IronPDF DPI 144
// Pdfium scale 3.0 → IronPDF DPI 216
// Pdfium scale 4.0 → IronPDF DPI 300

// Migration helper:
public int ScaleToDpi(float scale) => (int)(72 * scale);
```

The [detailed migration reference](https://ironpdf.com/blog/migration-guides/migrate-from-pdfium-to-ironpdf/) provides DPI conversion tables and rendering quality comparison guidelines.

---

## Native Dependency Removal

### Before (Pdfium.NET) - Complex Deployment

```
MyApp/
├── bin/
│   ├── MyApp.dll
│   ├── Pdfium.NET.dll
│   ├── x86/
│   │   └── pdfium.dll
│   └── x64/
│       └── pdfium.dll
├── runtimes/
│   ├── win-x86/native/
│   │   └── pdfium.dll
│   └── win-x64/native/
│       └── pdfium.dll
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
// Pdfium.NET: Manual size specification
page.Render(1024, 768);

// IronPDF: DPI-based rendering
pdf.RasterizeToImageFiles("*.png", DPI: 150);
```

### 2. Disposal Pattern Changes

```csharp
// Pdfium.NET: Required explicit disposal
using (var document = PdfDocument.Load(path))
using (var page = document.Pages[0])
using (var bitmap = page.Render(1024, 768))
{
    bitmap.Save("output.png");
}

// IronPDF: Simplified
var pdf = PdfDocument.FromFile(path);
pdf.RasterizeToImageFiles("output.png");
```

### 3. Page Access

```csharp
// Pdfium.NET: Creates page objects that need disposal
using (var page = document.Pages[0]) { }

// IronPDF: Direct property access
var page = pdf.Pages[0];
```

### 4. Platform-Specific Code Removal

```csharp
// Pdfium.NET: Required platform detection
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
// Pdfium.NET
page.Render(width, height, PdfRenderFlags.Annotations | PdfRenderFlags.ForPrinting);

// IronPDF: Configuration happens at render time
// Annotations and print quality are default behaviors
```

---

## Feature Comparison Summary

| Feature | Pdfium.NET | IronPDF |
|---------|------------|---------|
| Load PDF | ✓ | ✓ |
| Render to Image | ✓ | ✓ |
| Extract Text | ✓ (basic) | ✓ (advanced) |
| Page Info | ✓ | ✓ |
| Create from HTML | ✗ | ✓ |
| Create from URL | ✗ | ✓ |
| Merge PDFs | ✗ | ✓ |
| Split PDFs | ✗ | ✓ |
| Add Watermarks | ✗ | ✓ |
| Headers/Footers | ✗ | ✓ |
| Form Filling | ✗ | ✓ |
| Digital Signatures | ✗ | ✓ |
| Password Protection | ✗ | ✓ |
| Native Dependencies | Required | None |
| Cross-Platform | Complex | Automatic |
| Memory Management | Manual disposal | Simplified |

---

## Pre-Migration Checklist

- [ ] Identify all Pdfium.NET usage in codebase
- [ ] Document current rendering dimensions/scales used
- [ ] List native binary locations in project
- [ ] Check for platform-specific loading code
- [ ] Identify PDF creation needs (currently using separate tools?)
- [ ] Review disposal patterns for conversion
- [ ] Check build scripts for native binary copying

---

## Post-Migration Checklist

- [ ] Remove Pdfium NuGet packages
- [ ] Delete native pdfium.dll binaries
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

## Finding Pdfium.NET References

```bash
# Find Pdfium usage
grep -r "Pdfium\|PdfDocument\.Load\|\.Render\(" --include="*.cs" .

# Find native binary references
grep -r "pdfium\.dll\|pdfium\.so\|pdfium\.dylib" --include="*.csproj" --include="*.config" .

# Find platform-specific code
grep -r "#if.*64\|WIN32\|WIN64\|LINUX\|OSX" --include="*.cs" .
```

---

## Troubleshooting

### "License key required"

```csharp
// Set at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### "Rendered image quality different"

Convert Pdfium scale to equivalent DPI:

```csharp
// If Pdfium used scale 2.0, use DPI 144
pdf.RasterizeToImageFiles("*.png", DPI: 144);
```

### "Page dimensions different"

Both use points (1/72 inch). Dimensions should be identical—verify you're accessing the correct properties.

### "Text extraction returns different results"

IronPDF uses more advanced text extraction. Results may be more accurate but formatted differently.

---

## Performance Notes

| Operation | Pdfium.NET | IronPDF |
|-----------|------------|---------|
| First PDF load | Fast (native) | Moderate (managed) |
| Page rendering | Very fast | Fast |
| Text extraction | Basic | Comprehensive |
| Memory usage | Lower | Higher (more features) |
| Startup time | Fast | Moderate (Chromium init) |

IronPDF trades some raw rendering performance for vastly expanded capabilities.

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Pdfium.NET usages in codebase**
  ```bash
  grep -r "using Pdfium.NET" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package Pdfium.NET
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
