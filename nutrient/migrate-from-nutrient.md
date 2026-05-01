# How Do I Migrate from Nutrient (formerly PSPDFKit) to IronPDF in C#?

## Why Migrate?

Nutrient (formerly PSPDFKit; rebranded 2024-10-23) has evolved from a single PDF SDK into a multi-product family — Document Engine (Docker microservice), Web SDK, mobile SDKs, and a server-side **.NET SDK that is actually GdPicture.NET** (Nutrient acquired ORPALIS/GdPicture and folded it in as their .NET line). For teams that need straightforward PDF operations without the multi-product surface area, IronPDF provides a single focused .NET package.

### Issues with Nutrient

1. **Multiple overlapping products**: One brand, several SKUs
   - Document Engine (server / Docker) vs .NET SDK (in-process GdPicture)
   - PSPDFKit-branded mobile/Web packages still on NuGet alongside `Nutrient.*` packages
   - Choosing the right product line is a non-trivial up-front decision

2. **Sales-led pricing**: No published price list
   - The .NET SDK pricing page (`nutrient.io/sdk/pricing`) routes to "Contact Us"
   - Quotes typically per-developer + deployment tier
   - Hard to compare against published-pricing alternatives without a sales call

3. **Rebrand and acquisition trail**: PSPDFKit → Nutrient + GdPicture acquisition
   - Documentation, blog posts, and NuGet packages reference all three names
   - The .NET SDK still ships as the `GdPicture` NuGet package, namespace `GdPicture14`
   - Class names (`GdPicturePDF`, `GdPictureDocumentConverter`) do not match the "Nutrient" or "PSPDFKit" branding

The [comprehensive migration documentation](https://ironpdf.com/blog/migration-guides/migrate-from-nutrient-to-ironpdf/) covers package choice and API surface differences in more detail.

4. **External browser dependency for HTML-to-PDF**: Chrome or Edge must be installed
   - The .NET SDK invokes a system Chrome/Edge for HTML rendering (or a portable path you set with `SetWebBrowserPath`)
   - IronPDF ships its own embedded Chromium and does not rely on a system browser

5. **Procedural PDF API**: Many low-level primitives instead of one composed call
   - Watermarks compose `SetFillAlpha`, `DrawTextBox`, OCG layer markers
   - Header/footer, security, etc. require multiple calls per operation

### Benefits of IronPDF

| Aspect | Nutrient .NET SDK (GdPicture) | IronPDF |
|--------|-------------------------------|---------|
| Focus | Imaging + PDF + OCR + barcode toolkit | PDF library |
| Pricing | Sales-led (contact for quote) | Transparent, published |
| NuGet package | `GdPicture` | `IronPdf` |
| Root namespace | `GdPicture14` | `IronPdf` |
| API Style | Mostly synchronous, procedural | Sync with async options |
| HTML rendering | Requires system Chrome/Edge | Embedded Chromium |
| Cross-platform | Windows / Linux / macOS, .NET Framework + .NET 6/7/8/10 | Windows / Linux / macOS, .NET Framework + .NET 6+ |
| Learning Curve | Broad surface (imaging, scanning, OCR, PDF) | PDF-focused |

---

## NuGet Package Changes

```bash
# Remove the Nutrient .NET SDK (GdPicture) and any legacy PSPDFKit-branded packages
dotnet remove package GdPicture
dotnet remove package GdPicture.WPF
dotnet remove package GdPicture.WinForms

# Install IronPDF
dotnet add package IronPdf
```

The current Nutrient .NET SDK ships under the `GdPicture` NuGet ID (latest 14.x, owner ORPALIS — Nutrient's subsidiary). The older PSPDFKit-branded `PSPDFKit.NET` package targeted .NET Standard 2.0 and is now sunset; the mobile `PSPDFKit.dotnet.*` packages have been republished as `Nutrient.dotnet.*`. For server-side .NET PDF work, `GdPicture` is the package Nutrient currently directs new customers to.

---

## Namespace Changes

| Nutrient .NET SDK (GdPicture) | IronPDF |
|--------------------------------|---------|
| `using GdPicture14;` | `using IronPdf;` |
| `GdPicturePDF` (PDF object model) | `PdfDocument` |
| `GdPictureDocumentConverter` (loaders/exporters) | `ChromePdfRenderer` |
| `GdPictureImaging` (raster utilities) | `IronSoftware.Drawing` |
| `PdfHorizontalAlignment` / `PdfVerticalAlignment` | `IronPdf.Editing.HorizontalAlignment` / `VerticalAlignment` |

---

## Complete API Mapping

### Initialization

| Nutrient .NET SDK (GdPicture) | IronPDF | Notes |
|-------------------------------|---------|-------|
| `new GdPicturePDF()` | `new PdfDocument(...)` / `PdfDocument.FromFile(...)` | Both sync |
| `new GdPictureDocumentConverter()` | `new ChromePdfRenderer()` | Used for HTML/Office conversion |
| `pdf.Dispose()` (IDisposable) | `pdf.Dispose()` (IDisposable) | Both implement `IDisposable` |

### Document Loading

| Nutrient .NET SDK (GdPicture) | IronPDF | Notes |
|-------------------------------|---------|-------|
| `pdf.LoadFromFile(path)` | `PdfDocument.FromFile(path)` | Sync |
| `pdf.LoadFromStream(stream)` | `PdfDocument.FromStream(stream)` | Stream |
| `pdf.LoadFromByteArray(bytes)` | `new PdfDocument(bytes)` / `PdfDocument.FromBinaryData(bytes)` | Byte array |

### PDF Generation (HTML)

| Nutrient .NET SDK (GdPicture) | IronPDF | Notes |
|-------------------------------|---------|-------|
| `converter.LoadFromFile(htmlPath, DocumentFormat.DocumentFormatHTML)` then `converter.SaveAsPDF(out)` | `renderer.RenderHtmlFileAsPdf(htmlPath)` | Two-step vs one |
| `converter.LoadFromHttp(url)` then `SaveAsPDF` | `renderer.RenderUrlAsPdf(url)` | URL → PDF |
| HTML string: write to temp file then `LoadFromFile` | `renderer.RenderHtmlAsPdf(html)` | Direct string in IronPDF |
| Requires system Chrome/Edge or `SetWebBrowserPath` | Embedded Chromium | No external browser |

### Page Configuration

| Nutrient .NET SDK (GdPicture) | IronPDF | Notes |
|-------------------------------|---------|-------|
| `converter.HtmlPageWidth` / `HtmlPageHeight` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Named sizes vs raw width/height |
| `pdf.SetMediaBox(...)` (per page) | `RenderingOptions.PaperOrientation` | Orientation enum |
| Margin properties on converter | `RenderingOptions.MarginTop` etc. | Individual margin properties |

### Document Operations

| Nutrient .NET SDK (GdPicture) | IronPDF | Notes |
|-------------------------------|---------|-------|
| `pdf.MergeDocuments(GdPicturePDF[])` or `converter.CombineToPDF(paths, out, conformance)` | `PdfDocument.Merge(pdfs)` | Multiple overloads on both |
| `pdf.GetPageCount()` | `pdf.PageCount` | Method vs property |
| `pdf.SelectPage(i)` | `pdf.Pages[i]` | 1-based vs 0-based |
| `pdf.ClonePage(srcPdf, pageIndex)` | `pdf.AppendPdf(otherPdf)` | Page-level vs document-level |
| `pdf.DeletePage(index)` | `pdf.RemovePages(index)` | — |

### Annotations and Watermarks

| Nutrient .NET SDK (GdPicture) | IronPDF | Notes |
|-------------------------------|---------|-------|
| `pdf.SetFillAlpha(alpha)` + `pdf.DrawTextBox(...)` | `pdf.ApplyWatermark(html, rotation, vAlign, hAlign)` | One call vs several |
| `pdf.AddStandardFont(...)` then draw | HTML/CSS in watermark string | CSS styling |
| `pdf.AddStampAnnotation(...)` | `pdf.ApplyStamp(...)` | Stamp objects |
| Alpha is 0–255 (byte) | Watermark `Opacity` is 0–100 (int) | Different scales |

### Form Handling

| Nutrient .NET SDK (GdPicture) | IronPDF | Notes |
|-------------------------------|---------|-------|
| `pdf.GetFormFieldsCount()` + `pdf.GetFormFieldName(i)` | `pdf.Form.Fields` | Index-based vs collection |
| `pdf.SetFormFieldValue(name, value)` | `pdf.Form.FindFormField(name).Value = value` | Method vs property |
| `pdf.FlattenFormFields()` | `pdf.Form.Flatten()` | Flatten forms |

### Headers and Footers

| Nutrient .NET SDK (GdPicture) | IronPDF | Notes |
|-------------------------------|---------|-------|
| Manual `DrawTextBox` per page | `RenderingOptions.HtmlHeader = new HtmlHeaderFooter { ... }` | One-shot HTML |
| Manual page-number drawing in a loop | `{page}` / `{total-pages}` placeholders | Built-in tokens |

### Output

| Nutrient .NET SDK (GdPicture) | IronPDF | Notes |
|-------------------------------|---------|-------|
| `pdf.SaveToFile(path)` | `pdf.SaveAs(path)` | Sync |
| `pdf.SaveToByteArray(out bytes)` | `pdf.BinaryData` | Byte array |
| `pdf.SaveToStream(stream)` | `pdf.Stream` / `pdf.SaveAs(stream)` | Stream |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (Nutrient .NET SDK / GdPicture):**
```csharp
using GdPicture14;
using System.IO;

public class NutrientService
{
    public byte[] GeneratePdf(string html)
    {
        // GdPicture's HTML loader takes a file, so we stage the string first.
        File.WriteAllText("input.html", html);

        using var converter = new GdPictureDocumentConverter();
        converter.LoadFromFile("input.html", DocumentFormat.DocumentFormatHTML);
        converter.SaveAsPDF("output.pdf");

        return File.ReadAllBytes("output.pdf");
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

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
        return pdf.BinaryData;
    }
}
```

### Example 2: URL to PDF with Configuration

**Before (Nutrient .NET SDK / GdPicture):**
```csharp
using GdPicture14;
using System.IO;

public byte[] ConvertUrl(string url)
{
    using var converter = new GdPictureDocumentConverter();

    // Configuration is set via properties on the converter; HTML page dimensions
    // are in inches/points depending on measurement unit.
    converter.HtmlPageWidth  = 8.27f;   // A4 width  (inches)
    converter.HtmlPageHeight = 11.69f;  // A4 height (inches)
    converter.HtmlMarginTop = 0.78f;
    converter.HtmlMarginBottom = 0.78f;
    converter.HtmlMarginLeft = 0.78f;
    converter.HtmlMarginRight = 0.78f;

    converter.LoadFromHttp(url);
    converter.SaveAsPDF("output.pdf");

    return File.ReadAllBytes("output.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertUrl(string url)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
    renderer.RenderingOptions.MarginTop = 20;
    renderer.RenderingOptions.MarginBottom = 20;
    renderer.RenderingOptions.MarginLeft = 20;
    renderer.RenderingOptions.MarginRight = 20;
    renderer.RenderingOptions.EnableJavaScript = true;
    renderer.RenderingOptions.WaitFor.RenderDelay(2000);

    var pdf = renderer.RenderUrlAsPdf(url);
    return pdf.BinaryData;
}
```

### Example 3: Merging PDFs

**Before (Nutrient .NET SDK / GdPicture):**
```csharp
using GdPicture14;
using System.Collections.Generic;

public void MergePdfs(string[] inputPaths, string outputPath)
{
    using var converter = new GdPictureDocumentConverter();
    converter.CombineToPDF((IEnumerable<string>)inputPaths, outputPath, PdfConformance.PDF);
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Linq;

public void MergePdfs(string[] inputPaths, string outputPath)
{
    var pdfs = inputPaths.Select(PdfDocument.FromFile).ToList();
    var merged = PdfDocument.Merge(pdfs);
    merged.SaveAs(outputPath);
}
```

### Example 4: Adding Watermarks

**Before (Nutrient .NET SDK / GdPicture):**
```csharp
using GdPicture14;

public void AddWatermark(string inputPath, string outputPath)
{
    using var pdf = new GdPicturePDF();
    pdf.LoadFromFile(inputPath);
    pdf.SetMeasurementUnit(PdfMeasurementUnit.PdfMeasurementUnitPoint);

    var fontResName = pdf.AddStandardFont(PdfStandardFont.PdfStandardFontHelveticaBold);
    int pageCount = pdf.GetPageCount();

    for (int i = 1; i <= pageCount; i++)
    {
        pdf.SelectPage(i);
        pdf.SetFillAlpha(128);          // 0=transparent, 255=opaque
        pdf.SetTextSize(48);
        pdf.SetOriginRotationInDegrees(45);
        pdf.DrawTextBox(fontResName,
            100, 300, 500, 400,
            "CONFIDENTIAL",
            PdfHorizontalAlignment.PdfHorizontalAlignmentCenter,
            PdfVerticalAlignment.PdfVerticalAlignmentMiddle);
    }

    pdf.SaveToFile(outputPath);
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

public void AddWatermark(string inputPath, string outputPath)
{
    var pdf = PdfDocument.FromFile(inputPath);

    pdf.ApplyWatermark(
        "<h1 style='color:gray; opacity:0.5; font-size:48px;'>CONFIDENTIAL</h1>",
        45, // rotation
        VerticalAlignment.Middle,
        HorizontalAlignment.Center);

    pdf.SaveAs(outputPath);
}
```

### Example 5: Headers and Footers

**Before (Nutrient .NET SDK / GdPicture):**
```csharp
using GdPicture14;

// GdPicture has no first-class header/footer model — you draw on each page yourself.
public void CreateWithHeaderFooter(string inputPdf, string outputPath)
{
    using var pdf = new GdPicturePDF();
    pdf.LoadFromFile(inputPdf);
    pdf.SetMeasurementUnit(PdfMeasurementUnit.PdfMeasurementUnitPoint);

    var font = pdf.AddStandardFont(PdfStandardFont.PdfStandardFontHelvetica);
    int pageCount = pdf.GetPageCount();

    for (int i = 1; i <= pageCount; i++)
    {
        pdf.SelectPage(i);
        pdf.SetTextSize(10);

        float w = pdf.GetPageWidth();
        float h = pdf.GetPageHeight();

        pdf.DrawText(font, w / 2 - 40, h - 20, "Document Header");
        pdf.DrawText(font, w / 2 - 30, 20, $"Page {i} of {pageCount}");
    }

    pdf.SaveToFile(outputPath);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreateWithHeaderFooter(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center; font-size:10px;'>Document Header</div>",
        MaxHeight = 25
    };

    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center; font-size:10px;'>Page {page} of {total-pages}</div>",
        MaxHeight = 25
    };

    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 6: Form Field Handling

**Before (Nutrient .NET SDK / GdPicture):**
```csharp
using GdPicture14;
using System;

public void FillForm(string inputPath, string outputPath)
{
    using var pdf = new GdPicturePDF();
    pdf.LoadFromFile(inputPath);

    int count = pdf.GetFormFieldsCount();
    for (int i = 0; i < count; i++)
    {
        string name = pdf.GetFormFieldName(i);
        if (name == "CustomerName")
            pdf.SetFormFieldValue(name, "John Doe");
        else if (name == "Date")
            pdf.SetFormFieldValue(name, DateTime.Now.ToString("yyyy-MM-dd"));
    }

    pdf.SaveToFile(outputPath);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void FillForm(string inputPath, string outputPath)
{
    var pdf = PdfDocument.FromFile(inputPath);

    pdf.Form.FindFormField("CustomerName").Value = "John Doe";
    pdf.Form.FindFormField("Date").Value = DateTime.Now.ToString("yyyy-MM-dd");

    pdf.SaveAs(outputPath);
}
```

### Example 7: Password Protection

**Before (Nutrient .NET SDK / GdPicture):**
```csharp
using GdPicture14;

public void ProtectPdf(string inputPath, string outputPath, string password)
{
    using var pdf = new GdPicturePDF();
    pdf.LoadFromFile(inputPath);

    // SaveToFile overload that applies AES-256 encryption with permissions.
    pdf.SaveToFile(outputPath,
        password,                       // user password
        password,                       // owner password
        PdfEncryption.PdfEncryptionAES256,
        false,                          // disallow content copy
        false,                          // disallow content edit
        true,                           // allow printing
        false);                         // disallow form filling
}
```

**After (IronPDF):**
```csharp
using IronPdf;

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
```

### Example 8: Text Extraction

**Before (Nutrient .NET SDK / GdPicture):**
```csharp
using GdPicture14;
using System.Text;

public string ExtractText(string pdfPath)
{
    using var pdf = new GdPicturePDF();
    pdf.LoadFromFile(pdfPath);

    var sb = new StringBuilder();
    int pageCount = pdf.GetPageCount();
    for (int i = 1; i <= pageCount; i++)
    {
        pdf.SelectPage(i);
        sb.AppendLine(pdf.GetPageText());
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
```

### Example 9: Page Manipulation

**Before (Nutrient .NET SDK / GdPicture):**
```csharp
using GdPicture14;

public void ManipulatePages(string inputPath, string outputPath)
{
    using var pdf = new GdPicturePDF();
    pdf.LoadFromFile(inputPath);

    // Remove first page (1-based)
    pdf.SelectPage(1);
    pdf.DeletePage();

    // Rotate (now-)first page 90 degrees
    pdf.SelectPage(1);
    pdf.RotatePage(PdfRotation.PdfRotation90);

    // Reorder pages: GdPicture supports MovePage/SwapPages, otherwise
    // build a new document and ClonePage in the desired order.

    pdf.SaveToFile(outputPath);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void ManipulatePages(string inputPath, string outputPath)
{
    var pdf = PdfDocument.FromFile(inputPath);

    // Remove page
    pdf.RemovePages(0);

    // Rotate page
    pdf.Pages[0].Rotation = PdfPageRotation.Rotate90;

    // Reorder pages
    var reordered = pdf.CopyPages(2, 0, 1); // Pages 3, 1, 2

    reordered.SaveAs(outputPath);
}
```

### Example 10: Complete Workflow

**Before (Nutrient .NET SDK / GdPicture):**
```csharp
using GdPicture14;
using System.IO;

public class NutrientWorkflow
{
    public void ProcessDocument(string html, string outputPath)
    {
        File.WriteAllText("input.html", html);

        // 1. HTML -> PDF
        using (var converter = new GdPictureDocumentConverter())
        {
            converter.LoadFromFile("input.html", DocumentFormat.DocumentFormatHTML);
            converter.SaveAsPDF("temp.pdf");
        }

        // 2. Add a "DRAFT" watermark and save with encryption
        using var pdf = new GdPicturePDF();
        pdf.LoadFromFile("temp.pdf");
        pdf.SetMeasurementUnit(PdfMeasurementUnit.PdfMeasurementUnitPoint);
        var font = pdf.AddStandardFont(PdfStandardFont.PdfStandardFontHelveticaBold);

        int pageCount = pdf.GetPageCount();
        for (int i = 1; i <= pageCount; i++)
        {
            pdf.SelectPage(i);
            pdf.SetFillAlpha(76);  // ~30%
            pdf.SetTextSize(72);
            pdf.SetOriginRotationInDegrees(45);
            pdf.DrawTextBox(font, 100, 300, 500, 400, "DRAFT",
                PdfHorizontalAlignment.PdfHorizontalAlignmentCenter,
                PdfVerticalAlignment.PdfVerticalAlignmentMiddle);
        }

        pdf.SaveToFile(outputPath, "", "admin",
            PdfEncryption.PdfEncryptionAES256,
            true, false, true, true);  // disallow content edit
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

public class PdfWorkflow
{
    private readonly ChromePdfRenderer _renderer;

    public PdfWorkflow()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();

        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.MarginTop = 20;
        _renderer.RenderingOptions.MarginBottom = 20;
        _renderer.RenderingOptions.MarginLeft = 20;
        _renderer.RenderingOptions.MarginRight = 20;

        _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>",
            MaxHeight = 20
        };
    }

    public void ProcessDocument(string html, string outputPath)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);

        // Add watermark (simple HTML)
        pdf.ApplyWatermark(
            "<div style='color:gray; opacity:0.3; font-size:72px;'>DRAFT</div>",
            45,
            VerticalAlignment.Middle,
            HorizontalAlignment.Center);

        // Add security
        pdf.SecuritySettings.OwnerPassword = "admin";
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

        pdf.SaveAs(outputPath);
    }
}
```

---

## Common Migration Issues

### Issue 1: HTML Input Surface

**Problem:** GdPicture's HTML loader is file/URL-based; IronPDF accepts strings directly

```csharp
// Nutrient .NET SDK (string -> file -> PDF):
File.WriteAllText("input.html", html);
converter.LoadFromFile("input.html", DocumentFormat.DocumentFormatHTML);
converter.SaveAsPDF("output.pdf");

// IronPDF (string -> PDF):
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Issue 2: External Browser Dependency

**Problem:** GdPicture's HTML rendering requires Chrome or Edge installed on the host. IronPDF ships embedded Chromium.

```csharp
// Nutrient .NET SDK: optionally point to a portable Chrome
converter.SetWebBrowserPath(@"C:\Tools\chrome\chrome.exe");

// IronPDF: nothing to configure
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 3: Procedural Watermarks vs HTML Watermarks

**Problem:** GdPicture composes watermarks from primitive draw calls; IronPDF takes a single HTML/CSS string.

```csharp
// Nutrient .NET SDK:
pdf.SetFillAlpha(128);
pdf.SetTextSize(48);
pdf.SetOriginRotationInDegrees(45);
pdf.DrawTextBox(font, 100, 300, 500, 400, "CONFIDENTIAL",
    PdfHorizontalAlignment.PdfHorizontalAlignmentCenter,
    PdfVerticalAlignment.PdfVerticalAlignmentMiddle);

// IronPDF:
pdf.ApplyWatermark("<h1 style='opacity:0.5; font-size:48px;'>CONFIDENTIAL</h1>",
    45, VerticalAlignment.Middle, HorizontalAlignment.Center);
```

### Issue 4: Headers/Footers and Page Numbers

**Problem:** GdPicture has no first-class header/footer model; you draw on each page in a loop. IronPDF exposes `HtmlHeader`/`HtmlFooter` with `{page}` / `{total-pages}` tokens.

```csharp
// Nutrient .NET SDK: per-page DrawText loop
for (int i = 1; i <= pdf.GetPageCount(); i++)
{
    pdf.SelectPage(i);
    pdf.DrawText(font, x, y, $"Page {i} of {pdf.GetPageCount()}");
}

// IronPDF: built-in placeholders
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "Page {page} of {total-pages}"
};
```

### Issue 5: Page Indexing

**Problem:** GdPicture is **1-based** (`SelectPage(1)` is the first page). IronPDF `pdf.Pages` and `pdf.RemovePages(int)` are **0-based**. Off-by-one errors are the most common porting bug.

```csharp
// Nutrient .NET SDK (1-based):
pdf.SelectPage(1);
pdf.DeletePage();

// IronPDF (0-based):
pdf.RemovePages(0);
```

---

## Performance Comparison

Hard numbers depend heavily on host hardware, the host's installed Chrome/Edge version (for GdPicture's HTML path), and document complexity. Both libraries are competitive on raw PDF operations; the biggest practical difference is HTML rendering, where IronPDF's embedded Chromium starts faster than spawning a host browser. Benchmark on your own workload before publishing numbers.

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all GdPicture / PSPDFKit / Nutrient usages**
  ```bash
  grep -rE "GdPicture14|GdPicturePDF|GdPictureDocumentConverter|PSPDFKit|Nutrient" --include="*.cs" .
  ```
  **Why:** Identify all usages so the package and namespace removal is complete.

- [ ] **Note 1-based vs 0-based page indexing in your code**
  ```csharp
  // GdPicture is 1-based: SelectPage(1) targets the first page.
  ```
  **Why:** IronPDF is 0-based — every page-index call needs review.

- [ ] **List configuration set on `GdPictureDocumentConverter`**
  ```csharp
  converter.HtmlPageWidth = 8.27f;
  converter.HtmlPageHeight = 11.69f;
  converter.HtmlMarginTop = 0.78f;
  ```
  **Why:** These map to IronPDF's `RenderingOptions.PaperSize` and per-side margin properties.

- [ ] **Identify procedural drawing for watermarks and headers**
  ```csharp
  pdf.SetFillAlpha(128);
  pdf.DrawTextBox(font, ...);
  ```
  **Why:** IronPDF replaces these with `pdf.ApplyWatermark(html, ...)` and `RenderingOptions.HtmlHeader/HtmlFooter`.

- [ ] **Review form-field code that uses index-based access**
  **Why:** IronPDF exposes a name-keyed `pdf.Form.FindFormField(name)` you can use directly.

### Migration Steps

- [ ] **Install IronPDF NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project to start using its features.

- [ ] **Remove the Nutrient .NET SDK NuGet package**
  ```bash
  dotnet remove package GdPicture
  dotnet remove package GdPicture.WPF
  dotnet remove package GdPicture.WinForms
  ```
  **Why:** The Nutrient .NET SDK ships as `GdPicture` on NuGet (owner ORPALIS, a Nutrient subsidiary). Older PSPDFKit-branded `PSPDFKit.NET` packages are sunset; remove those if present too.

- [ ] **Add IronPDF license key configuration**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Update namespace imports**
  ```csharp
  // Before
  using GdPicture14;

  // After
  using IronPdf;
  ```
  **Why:** Ensure all references point to IronPDF classes and methods.

- [ ] **Replace `GdPictureDocumentConverter` with `ChromePdfRenderer` for HTML / URL input**
  ```csharp
  // Before
  using var converter = new GdPictureDocumentConverter();
  converter.LoadFromFile("input.html", DocumentFormat.DocumentFormatHTML);
  converter.SaveAsPDF("output.pdf");

  // After
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);   // string input directly
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF accepts an HTML string directly and uses an embedded Chromium, removing the system-browser dependency.

- [ ] **Replace `GdPicturePDF` with `PdfDocument`**
  ```csharp
  // Before
  using var pdf = new GdPicturePDF();
  pdf.LoadFromFile("input.pdf");

  // After
  var pdf = PdfDocument.FromFile("input.pdf");
  ```
  **Why:** IronPDF's `PdfDocument` is the primary PDF object model.

- [ ] **Replace converter properties with `RenderingOptions`**
  ```csharp
  // Before
  converter.HtmlPageWidth = 8.27f; // A4 inches
  converter.HtmlMarginTop = 0.78f;

  // After
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.MarginTop = 20; // millimetres
  ```
  **Why:** IronPDF uses named paper sizes and a single `RenderingOptions` bag.

- [ ] **Replace procedural watermark draw calls with `ApplyWatermark`**
  ```csharp
  // Before
  pdf.SetFillAlpha(76);
  pdf.DrawTextBox(font, ..., "DRAFT", ...);

  // After
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>",
      45, VerticalAlignment.Middle, HorizontalAlignment.Center);
  ```
  **Why:** HTML watermarks are styled with CSS instead of composed from primitives.

- [ ] **Replace per-page DrawText loops with `HtmlHeaderFooter` placeholders**
  ```csharp
  // Before: per-page DrawText "Page i of N"

  // After
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** Built-in `{page}` / `{total-pages}` tokens replace manual page counting.

### Post-Migration

- [ ] **Remove async/await where no longer needed**
  **Why:** Simplify code by using synchronous methods where appropriate.

- [ ] **Run regression tests comparing PDF output**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Verify headers/footers with page numbers**
  **Why:** Ensure dynamic content like page numbers renders correctly.

- [ ] **Test form filling functionality**
  **Why:** Confirm that form handling features work as expected with IronPDF.

- [ ] **Confirm security settings work correctly**
  ```csharp
  // Example
  pdf.SecuritySettings.UserPassword = "secret";
  ```
  **Why:** Ensure that PDFs are secured as required.

- [ ] **Update CI/CD pipeline**
  **Why:** Reflect changes in dependencies and build processes in your CI/CD setup.
---

## Pricing Comparison

| Aspect | Nutrient .NET SDK (GdPicture) | IronPDF |
|--------|-------------------------------|---------|
| Pricing Model | Sales-led — `nutrient.io/sdk/pricing` routes to Contact Us | Published per-developer pricing on `ironpdf.com` |
| Trial | Free trial; trial-mode watermark and reminders on output | Free trial / development key |
| Per-Developer | Yes | Yes |
| Deployment Tiers | Negotiated with sales | Listed on pricing page |

---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [HTML to PDF Tutorial](https://ironpdf.com/tutorials/html-to-pdf/)
- [Headers and Footers Guide](https://ironpdf.com/how-to/headers-and-footers/)
- [Form Handling Guide](https://ironpdf.com/how-to/pdf-forms/)
