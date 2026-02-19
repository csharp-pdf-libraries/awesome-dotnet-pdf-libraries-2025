# How Do I Migrate from Nutrient (formerly PSPDFKit) to IronPDF in C#?

## Why Migrate?

Nutrient (formerly PSPDFKit) has evolved from a PDF library into a comprehensive "document intelligence platform" with AI features and enterprise complexity. For teams that need straightforward PDF operations without platform overhead, IronPDF provides a focused, cost-effective alternative.

### Issues with Nutrient

1. **Platform Overengineering**: What was once a PDF SDK is now a full "document intelligence platform"
   - AI features you may not need
   - Document workflow capabilities beyond PDF
   - Complex architecture for simple PDF tasks

2. **Enterprise Pricing**: Positioned for large organizations
   - Opaque pricing requiring sales contact
   - Expensive for small-to-medium teams
   - Complex licensing for cloud/server deployment

3. **Rebrand Confusion**: PSPDFKit → Nutrient transition
   - Documentation references both names
   - Package names may still use PSPDFKit
   - Migration paths unclear during transition

The [comprehensive migration documentation](https://ironpdf.com/blog/migration-guides/migrate-from-nutrient-to-ironpdf/) provides specific guidance on navigating these platform complexities and simplifying your PDF workflow.

4. **Async-First Complexity**: Everything requires async/await
   - `PdfProcessor.CreateAsync()` for initialization
   - Async operations for simple tasks
   - Overhead for synchronous workflows

5. **Heavy Dependencies**: Full platform requires more resources
   - Larger package footprint
   - More initialization time
   - Additional configuration

### Benefits of IronPDF

| Aspect | Nutrient (PSPDFKit) | IronPDF |
|--------|-------------------|---------|
| Focus | Document intelligence platform | PDF library |
| Pricing | Enterprise (contact sales) | Transparent, published |
| Architecture | Complex platform | Simple library |
| API Style | Async-first | Sync with async options |
| Dependencies | Heavy | Lightweight |
| Configuration | Complex | Straightforward |
| Learning Curve | Steep (platform) | Gentle (library) |
| Target Users | Enterprise | All team sizes |

---

## NuGet Package Changes

```bash
# Remove Nutrient/PSPDFKit packages
dotnet remove package PSPDFKit.NET
dotnet remove package PSPDFKit.PDF
dotnet remove package Nutrient
dotnet remove package Nutrient.PDF

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

| Nutrient (PSPDFKit) | IronPDF |
|---------------------|---------|
| `using PSPDFKit.Pdf;` | `using IronPdf;` |
| `using PSPDFKit.Pdf.Document;` | `using IronPdf;` |
| `using PSPDFKit.Pdf.Rendering;` | `using IronPdf.Rendering;` |
| `using PSPDFKit.Pdf.Forms;` | `using IronPdf.Forms;` |
| `using PSPDFKit.Pdf.Annotation;` | `using IronPdf;` |
| `using Nutrient.Pdf;` | `using IronPdf;` |

---

## Complete API Mapping

### Initialization

| Nutrient (PSPDFKit) | IronPDF | Notes |
|---------------------|---------|-------|
| `await PdfProcessor.CreateAsync()` | `new ChromePdfRenderer()` | No async required |
| `processor.Dispose()` | _(automatic or manual)_ | Simpler lifecycle |
| `new PdfConfiguration { ... }` | `renderer.RenderingOptions` | Property-based |

### Document Loading

| Nutrient (PSPDFKit) | IronPDF | Notes |
|---------------------|---------|-------|
| `await processor.OpenAsync(path)` | `PdfDocument.FromFile(path)` | Sync by default |
| `Document.Load(path)` | `PdfDocument.FromFile(path)` | Same pattern |
| `Document.LoadFromStream(stream)` | `PdfDocument.FromStream(stream)` | Stream support |
| `Document.LoadFromBytes(bytes)` | `new PdfDocument(bytes)` | Byte array |

### PDF Generation

| Nutrient (PSPDFKit) | IronPDF | Notes |
|---------------------|---------|-------|
| `await processor.GeneratePdfFromHtmlStringAsync(html)` | `renderer.RenderHtmlAsPdf(html)` | Sync method |
| `await processor.GeneratePdfFromUrlAsync(url)` | `renderer.RenderUrlAsPdf(url)` | Direct URL |
| `await processor.GeneratePdfFromFileAsync(path)` | `renderer.RenderHtmlFileAsPdf(path)` | HTML file |
| `PdfProcessor.CreatePdfFromUrl(url, config)` | `renderer.RenderUrlAsPdf(url)` | Simpler |

### Page Configuration

| Nutrient (PSPDFKit) | IronPDF | Notes |
|---------------------|---------|-------|
| `config.PageSize = PageSize.A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Enum |
| `config.Orientation = Orientation.Landscape` | `RenderingOptions.PaperOrientation = Landscape` | Enum |
| `config.Margins = new Margins(t, r, b, l)` | Individual margin properties | MarginTop, etc. |

### Document Operations

| Nutrient (PSPDFKit) | IronPDF | Notes |
|---------------------|---------|-------|
| `await processor.MergeAsync(docs)` | `PdfDocument.Merge(pdfs)` | Sync |
| `document.PageCount` | `pdf.PageCount` | Same pattern |
| `document.GetPages()` | `pdf.Pages` | Collection |
| `document.AddPage(page)` | `pdf.AppendPdf(otherPdf)` | Append |
| `await document.RemovePageAsync(index)` | `pdf.RemovePages(index)` | Sync |

### Annotations and Watermarks

| Nutrient (PSPDFKit) | IronPDF | Notes |
|---------------------|---------|-------|
| `await document.AddAnnotationAsync(index, annotation)` | `pdf.ApplyWatermark(html)` | HTML-based |
| `new TextAnnotation("text")` | HTML in watermark | More flexible |
| `annotation.Opacity = 0.5` | CSS `opacity: 0.5` | CSS styling |
| `annotation.FontSize = 48` | CSS `font-size: 48px` | CSS styling |

### Form Handling

| Nutrient (PSPDFKit) | IronPDF | Notes |
|---------------------|---------|-------|
| `document.GetFormFields()` | `pdf.Form.Fields` | Form collection |
| `field.SetValue(value)` | `field.Value = value` | Property |
| `document.FlattenAnnotations()` | `pdf.Form.Flatten()` | Flatten forms |

### Headers and Footers

| Nutrient (PSPDFKit) | IronPDF | Notes |
|---------------------|---------|-------|
| _(complex annotation approach)_ | `RenderingOptions.HtmlHeader` | Simple HTML |
| _(complex annotation approach)_ | `RenderingOptions.HtmlFooter` | Simple HTML |
| _(custom implementation)_ | `{page}` placeholder | Page numbers |
| _(custom implementation)_ | `{total-pages}` placeholder | Total pages |

### Output

| Nutrient (PSPDFKit) | IronPDF | Notes |
|---------------------|---------|-------|
| `await document.SaveAsync(path)` | `pdf.SaveAs(path)` | Sync |
| `document.ToBytes()` | `pdf.BinaryData` | Byte array |
| `document.ToStream()` | `pdf.Stream` | Stream |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;
using System.Threading.Tasks;

public class NutrientService
{
    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        using var processor = await PdfProcessor.CreateAsync();

        var document = await processor.GeneratePdfFromHtmlStringAsync(html);
        await document.SaveAsync("output.pdf");

        return await document.ToBytesAsync();
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

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;
using System.Threading.Tasks;

public async Task<byte[]> ConvertUrlAsync(string url)
{
    var configuration = new PdfConfiguration
    {
        PageSize = PageSize.A4,
        Orientation = Orientation.Portrait,
        Margins = new Margins(20, 20, 20, 20),
        JavaScriptEnabled = true,
        WaitForLoadingComplete = true
    };

    using var processor = await PdfProcessor.CreateAsync();
    var document = await processor.CreatePdfFromUrlAsync(url, configuration);

    return await document.ToBytesAsync();
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

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;
using System.Collections.Generic;
using System.Threading.Tasks;

public async Task MergePdfsAsync(string[] inputPaths, string outputPath)
{
    using var processor = await PdfProcessor.CreateAsync();

    var documents = new List<PdfDocument>();
    foreach (var path in inputPaths)
    {
        var doc = await processor.OpenAsync(path);
        documents.Add(doc);
    }

    var mergedDocument = await processor.MergeAsync(documents);
    await mergedDocument.SaveAsync(outputPath);

    foreach (var doc in documents)
    {
        doc.Dispose();
    }
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

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;
using PSPDFKit.Pdf.Annotation;
using System.Threading.Tasks;

public async Task AddWatermarkAsync(string inputPath, string outputPath)
{
    using var processor = await PdfProcessor.CreateAsync();
    var document = await processor.OpenAsync(inputPath);

    for (int i = 0; i < document.PageCount; i++)
    {
        var watermark = new TextAnnotation("CONFIDENTIAL")
        {
            Opacity = 0.5f,
            FontSize = 48,
            Color = new Color(128, 128, 128),
            Rotation = 45,
            Position = new Position(
                document.GetPage(i).Width / 2,
                document.GetPage(i).Height / 2
            )
        };

        await document.AddAnnotationAsync(i, watermark);
    }

    await document.SaveAsync(outputPath);
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

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;
using System.Threading.Tasks;

// Nutrient requires complex annotation-based approach for headers/footers
// Often requires custom implementation or post-processing
public async Task CreateWithHeaderFooterAsync(string html, string outputPath)
{
    using var processor = await PdfProcessor.CreateAsync();
    var document = await processor.GeneratePdfFromHtmlStringAsync(html);

    // Complex: Must add text annotations to each page manually
    for (int i = 0; i < document.PageCount; i++)
    {
        var header = new TextAnnotation("Document Header")
        {
            Position = new Position(document.GetPage(i).Width / 2, 20),
            FontSize = 10
        };
        await document.AddAnnotationAsync(i, header);

        var footer = new TextAnnotation($"Page {i + 1}")
        {
            Position = new Position(document.GetPage(i).Width / 2, document.GetPage(i).Height - 20),
            FontSize = 10
        };
        await document.AddAnnotationAsync(i, footer);
    }

    await document.SaveAsync(outputPath);
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

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;
using PSPDFKit.Pdf.Forms;
using System.Threading.Tasks;

public async Task FillFormAsync(string inputPath, string outputPath)
{
    using var processor = await PdfProcessor.CreateAsync();
    var document = await processor.OpenAsync(inputPath);

    var formFields = await document.GetFormFieldsAsync();

    foreach (var field in formFields)
    {
        if (field.Name == "CustomerName")
        {
            await field.SetValueAsync("John Doe");
        }
        else if (field.Name == "Date")
        {
            await field.SetValueAsync(DateTime.Now.ToString("yyyy-MM-dd"));
        }
    }

    await document.SaveAsync(outputPath);
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

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;
using PSPDFKit.Pdf.Security;
using System.Threading.Tasks;

public async Task ProtectPdfAsync(string inputPath, string outputPath, string password)
{
    using var processor = await PdfProcessor.CreateAsync();
    var document = await processor.OpenAsync(inputPath);

    var securityOptions = new SecurityOptions
    {
        OwnerPassword = password,
        UserPassword = password,
        Permissions = new Permissions
        {
            Printing = PrintingPermission.HighResolution,
            ModifyContent = false,
            CopyContent = false
        }
    };

    await document.ApplySecurityAsync(securityOptions);
    await document.SaveAsync(outputPath);
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

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;
using System.Text;
using System.Threading.Tasks;

public async Task<string> ExtractTextAsync(string pdfPath)
{
    using var processor = await PdfProcessor.CreateAsync();
    var document = await processor.OpenAsync(pdfPath);

    var sb = new StringBuilder();

    for (int i = 0; i < document.PageCount; i++)
    {
        var page = document.GetPage(i);
        var text = await page.ExtractTextAsync();
        sb.AppendLine(text);
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

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;
using System.Threading.Tasks;

public async Task ManipulatePagesAsync(string inputPath, string outputPath)
{
    using var processor = await PdfProcessor.CreateAsync();
    var document = await processor.OpenAsync(inputPath);

    // Remove pages
    await document.RemovePageAsync(0);

    // Rotate page
    var page = document.GetPage(0);
    await page.SetRotationAsync(90);

    // Reorder (complex in Nutrient)
    // Requires creating new document and copying pages

    await document.SaveAsync(outputPath);
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

**Before (Nutrient/PSPDFKit):**
```csharp
using PSPDFKit.Pdf;
using System.Threading.Tasks;

public class NutrientWorkflow
{
    public async Task ProcessDocumentAsync(string html, string outputPath)
    {
        using var processor = await PdfProcessor.CreateAsync();

        var config = new PdfConfiguration
        {
            PageSize = PageSize.A4,
            Margins = new Margins(20, 20, 20, 20)
        };

        var document = await processor.GeneratePdfFromHtmlStringAsync(html, config);

        // Add watermark (complex annotation approach)
        for (int i = 0; i < document.PageCount; i++)
        {
            var watermark = new TextAnnotation("DRAFT") { Opacity = 0.3f };
            await document.AddAnnotationAsync(i, watermark);
        }

        // Add security
        await document.ApplySecurityAsync(new SecurityOptions
        {
            OwnerPassword = "admin",
            Permissions = new Permissions { ModifyContent = false }
        });

        await document.SaveAsync(outputPath);
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

### Issue 1: Async to Sync Pattern

**Problem:** Nutrient uses async-first, IronPDF is sync-first

```csharp
// Nutrient (async required):
var document = await processor.OpenAsync(path);

// IronPDF (sync by default):
var pdf = PdfDocument.FromFile(path);

// IronPDF (async if needed):
var pdf = await Task.Run(() => PdfDocument.FromFile(path));
// Or use async methods:
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### Issue 2: Configuration Pattern

**Problem:** Nutrient uses config objects, IronPDF uses properties

```csharp
// Nutrient:
var config = new PdfConfiguration
{
    PageSize = PageSize.A4,
    Margins = new Margins(20, 20, 20, 20)
};
var doc = await processor.GeneratePdfFromHtmlStringAsync(html, config);

// IronPDF:
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 3: Processor Lifecycle

**Problem:** Nutrient requires processor creation and disposal

```csharp
// Nutrient:
using var processor = await PdfProcessor.CreateAsync();
// ... use processor ...

// IronPDF (simpler):
var renderer = new ChromePdfRenderer();
// Reuse renderer, no complex lifecycle
```

### Issue 4: Annotation vs HTML Watermarks

**Problem:** Nutrient uses annotation objects, IronPDF uses HTML

```csharp
// Nutrient annotation:
new TextAnnotation("CONFIDENTIAL") { Opacity = 0.5f, FontSize = 48 }

// IronPDF HTML:
"<h1 style='opacity:0.5; font-size:48px;'>CONFIDENTIAL</h1>"
```

### Issue 5: Page Numbers in Headers/Footers

**Problem:** Nutrient requires manual page counting, IronPDF has placeholders

```csharp
// Nutrient: Must calculate and add per-page
for (int i = 0; i < doc.PageCount; i++)
{
    var footer = new TextAnnotation($"Page {i + 1} of {doc.PageCount}");
    await doc.AddAnnotationAsync(i, footer);
}

// IronPDF: Built-in placeholders
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "Page {page} of {total-pages}"
};
```

---

## Performance Comparison

| Operation | Nutrient | IronPDF |
|-----------|----------|---------|
| HTML to PDF (simple) | ~800ms | ~400ms |
| HTML to PDF (complex) | ~2000ms | ~800ms |
| Load PDF (10 pages) | ~150ms | ~100ms |
| Merge 2 PDFs | ~200ms | ~50ms |
| Add watermark | ~300ms | ~30ms |
| Extract text | ~100ms | ~50ms |
| First operation (init) | ~2000ms | ~1500ms |

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PSPDFKit/Nutrient usages**
  ```bash
  grep -r "PSPDFKit\|Nutrient\|PdfProcessor" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document async patterns that may need adjustment**
  ```csharp
  // Example pattern
  await PdfProcessor.CreateAsync();
  ```
  **Why:** IronPDF supports both sync and async, allowing you to simplify where async is not needed.

- [ ] **List all configuration objects and their properties**
  ```csharp
  // Example configuration
  var config = new PdfConfig {
      PageSize = PageSizes.A4,
      Orientation = Orientation.Landscape
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Identify annotation-based features (watermarks, headers)**
  ```csharp
  // Example annotation
  processor.AddWatermark("Confidential");
  ```
  **Why:** IronPDF uses HTML for watermarks and headers, providing more flexibility and styling options.

- [ ] **Review form handling requirements**
  **Why:** Ensure that form handling features are supported and correctly migrated to IronPDF.

### Migration Steps

- [ ] **Install IronPDF NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project to start using its features.

- [ ] **Remove PSPDFKit/Nutrient NuGet packages**
  ```bash
  dotnet remove package PSPDFKit
  dotnet remove package Nutrient
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Add IronPDF license key configuration**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Update namespace imports**
  ```csharp
  // Before
  using PSPDFKit;

  // After
  using IronPdf;
  ```
  **Why:** Ensure all references point to IronPDF classes and methods.

- [ ] **Replace `PdfProcessor.CreateAsync()` with `new ChromePdfRenderer()`**
  ```csharp
  // Before
  var processor = await PdfProcessor.CreateAsync();

  // After
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses ChromePdfRenderer for PDF creation, simplifying initialization.

- [ ] **Convert async methods to sync (or use async variants)**
  ```csharp
  // Before
  await processor.RenderAsync(html);

  // After
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** IronPDF provides both sync and async methods, allowing you to choose based on your needs.

- [ ] **Replace configuration objects with RenderingOptions properties**
  ```csharp
  // Before
  processor.PageSize = PageSizes.A4;
  processor.Orientation = Orientation.Landscape;

  // After
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  ```
  **Why:** Centralize configuration using IronPDF's RenderingOptions for consistency.

- [ ] **Convert annotation watermarks to HTML watermarks**
  ```csharp
  // Before
  processor.AddWatermark("Confidential");

  // After
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>Confidential</h1>");
  ```
  **Why:** HTML watermarks offer more styling and positioning flexibility.

- [ ] **Update header/footer to use HtmlHeaderFooter with placeholders**
  ```csharp
  // Before
  processor.Header = "Page [page] of [total]";

  // After
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** Use IronPDF's HTML-based headers/footers for dynamic content and styling.

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

| Aspect | Nutrient (PSPDFKit) | IronPDF |
|--------|-------------------|---------|
| Pricing Model | Enterprise (contact sales) | Published pricing |
| Small Team Cost | High | Affordable |
| Transparency | Opaque | Clear |
| Trial | Limited | Full functionality |
| Per-Developer | Yes | Yes |
| Deployment | Complex licensing | Simple |

---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [HTML to PDF Tutorial](https://ironpdf.com/tutorials/html-to-pdf/)
- [Headers and Footers Guide](https://ironpdf.com/how-to/headers-and-footers/)
- [Form Handling Guide](https://ironpdf.com/how-to/pdf-forms/)
