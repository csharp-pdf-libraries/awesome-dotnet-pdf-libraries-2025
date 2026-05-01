# How Do I Migrate from PeachPDF to IronPDF in C#?

## Why Migrate from PeachPDF?

PeachPDF is a relatively new, single-maintainer .NET library (BSD-3-Clause, by `jhaygood86`) that is still pre-1.0 (latest release `0.7.26`, October 2025). It lacks the maturity, features, and support of established solutions. Key reasons to migrate:

1. **Pre-1.0 Feature Set**: PeachPDF has no built-in digital signatures, PDF/A compliance, OCR, form filling, or page-level header/footer API
2. **No JavaScript engine**: PeachPDF is a layout renderer built on PeachPDF.PdfSharpCore, not a browser; scripts and dynamic CSS layout (e.g. complex Grid/Flexbox) do not execute
3. **.NET 8 only**: the package targets `net8.0`; older .NET Framework / .NET 6 projects need to upgrade or pick a different library
4. **Small Community**: a single GitHub maintainer, limited documentation, limited examples
5. **No Enterprise Support**: no commercial support or SLA options

### Quick Comparison

| Aspect | PeachPDF | IronPDF |
|--------|----------|---------|
| Maturity | Pre-1.0 (0.7.26, Oct 2025) | Established (40M+ downloads) |
| HTML Rendering | Layout engine on PdfSharpCore | Full Chromium html to pdf c# |
| CSS Support | HTML+CSS subset, web fonts, @font-face | Full CSS3 |
| JavaScript | Not executed | Full ES2024 |
| Digital Signatures | No | Yes |
| PDF/A Compliance | No | Yes |
| Professional Support | No | Yes |
| Documentation | Limited (single README) | Extensive |

For in-depth technical comparisons, visit the [analysis article](https://ironsoftware.com/suite/blog/comparison/compare-peachpdf-vs-ironpdf/).

---

## Quick Start: PeachPDF to [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/)

### Step 1: Replace NuGet Package

```bash
# Remove PeachPDF
dotnet remove package PeachPDF

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using PeachPDF;

// After
using IronPdf;
```

### Step 3: Initialize License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## API Mapping Reference

| PeachPDF | IronPDF | Notes |
|----------|---------|-------|
| `new PdfGenerator()` | `new ChromePdfRenderer()` | Create renderer |
| `await generator.GeneratePdf(html, config)` | `renderer.RenderHtmlAsPdf(html)` | HTML to PDF (PeachPDF is async) |
| `document.Save(stream)` | `pdf.SaveAs(path)` | Save file (PeachPDF only takes a Stream) |
| `document.Save(memStream)` then `memStream.ToArray()` | `pdf.BinaryData` | Get bytes |
| `new PdfGenerateConfig { NetworkAdapter = new HttpClientNetworkAdapter(...) }` | `renderer.RenderUrlAsPdf(url)` | URL to PDF |
| Not supported | `PdfDocument.FromFile(path)` | Load existing PDF (PeachPDF is HTML->PDF only) |
| Not supported | `pdf.AppendPdf(other)` / `PdfDocument.Merge(...)` | Merge PDFs |
| `config.PageSize`, `config.PageOrientation` | `renderer.RenderingOptions.PaperSize`, `.PaperOrientation` | Page setup |
| Embed in source HTML (CSS `position: fixed`) | `RenderingOptions.HtmlHeader` / `HtmlFooter` | Headers/footers |

---

## Code Examples

### Example 1: Basic HTML to PDF

**PeachPDF:**
```csharp
using PeachPDF;
using PeachPDF.PdfSharpCore;

var config = new PdfGenerateConfig { PageSize = PageSize.Letter };
var generator = new PdfGenerator();
var document = await generator.GeneratePdf("<h1>Hello World</h1><p>Sample content</p>", config);

using var stream = File.Create("output.pdf");
document.Save(stream);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>Sample content</p>");
pdf.SaveAs("output.pdf");
```

The [extensive migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-peachpdf-to-ironpdf/) covers strategies for migrating from lesser-known libraries with limited documentation to mature, enterprise-ready solutions.

### Example 2: PDF from URL

**PeachPDF:**
```csharp
// PeachPDF has no RenderUrlAsPdf-style helper; you wire it up via a network adapter.
using PeachPDF;
using PeachPDF.Network;
using PeachPDF.PdfSharpCore;

var http = new HttpClient();
var config = new PdfGenerateConfig
{
    PageSize = PageSize.Letter,
    NetworkAdapter = new HttpClientNetworkAdapter(http, new Uri("https://example.com"))
};

var generator = new PdfGenerator();
var document = await generator.GeneratePdf(null, config); // null tells PeachPDF to fetch from the adapter
using var stream = File.Create("webpage.pdf");
document.Save(stream);
// Note: no JavaScript execution -- SPA pages will not render.
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.JavaScript(3000);

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Load and Modify PDF

**PeachPDF:**
```csharp
// Not supported. PeachPDF is HTML -> PDF only; it does not parse, load,
// or edit existing PDF documents. To round-trip an existing PDF you must
// drop down to PdfSharpCore yourself or migrate to IronPDF.
```

**IronPDF:**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");

// Add new content page
var renderer = new ChromePdfRenderer();
var newPage = renderer.RenderHtmlAsPdf("<h1>New Page</h1>");
pdf.AppendPdf(newPage);

// Add watermark
pdf.ApplyWatermark("<div style='color: red; font-size: 48pt;'>DRAFT</div>");

pdf.SaveAs("modified.pdf");
```

### Example 4: Merge Multiple PDFs

**PeachPDF:**
```csharp
// Not supported as a first-class operation. PeachPDF only emits a freshly
// generated PDF document via PdfGenerator.GeneratePdf(...).Save(stream);
// merging existing PDFs requires using PdfSharpCore directly.
```

**IronPDF:**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("doc1.pdf");
var pdf2 = PdfDocument.FromFile("doc2.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

### Example 5: PDF with Headers and Footers

**PeachPDF:**
```csharp
// PeachPDF v0.7.x has no header/footer API; embed the markup in the
// source HTML using CSS positioning and let the renderer lay it out.
using PeachPDF;
using PeachPDF.PdfSharpCore;

var html = @"
    <html><head><style>
      .header { position: fixed; top: 0; left: 0; right: 0; text-align:center; }
      .footer { position: fixed; bottom: 0; left: 0; right: 0; text-align:center; }
    </style></head>
    <body>
      <div class='header'>My Header</div>
      <h1>Content</h1>
      <div class='footer'>Footer text</div>
    </body></html>";

var generator = new PdfGenerator();
var document = await generator.GeneratePdf(html, new PdfGenerateConfig { PageSize = PageSize.Letter });
using var stream = File.Create("output.pdf");
document.Save(stream);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"<div style='text-align:center; font-size:10pt;'>Company Report</div>",
    MaxHeight = 30
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"<div style='text-align:center; font-size:9pt;'>Page {page} of {total-pages}</div>",
    MaxHeight = 25
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Report Content</h1>");
pdf.SaveAs("report.pdf");
```

### Example 6: Password Protection

**PeachPDF:**
```csharp
// May not be supported
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential</h1>");

pdf.SecuritySettings.OwnerPassword = "owner123";
pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;

pdf.SaveAs("protected.pdf");
```

### Example 7: Digital Signatures

**PeachPDF:**
```csharp
// Not supported
```

**IronPDF:**
```csharp
using IronPdf;
using IronPdf.Signing;

var pdf = PdfDocument.FromFile("document.pdf");

var signature = new PdfSignature("certificate.pfx", "password")
{
    SigningReason = "Document Approval",
    SigningLocation = "New York"
};

pdf.Sign(signature);
pdf.SaveAs("signed.pdf");
```

### Example 8: Async Operations

**PeachPDF:**
```csharp
// PeachPDF's primary API is already async-only.
var generator = new PdfGenerator();
var document = await generator.GeneratePdf(html, new PdfGenerateConfig());
using var stream = File.Create("async_output.pdf");
document.Save(stream);
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync("<h1>Async PDF</h1>");
pdf.SaveAs("async_output.pdf");
```

---

## Feature Comparison

| Feature | PeachPDF | IronPDF |
|---------|----------|---------|
| HTML to PDF | Yes (PdfSharpCore-based layout) | Full Chromium |
| URL to PDF | Via `HttpClientNetworkAdapter` (no JS) | Yes |
| CSS Grid/Flexbox | Partial / not guaranteed | Yes |
| JavaScript | Not executed | Full ES2024 |
| Merge PDFs | No (HTML->PDF only) | Yes |
| Split PDFs | No | Yes |
| Watermarks | Manual via HTML | Full HTML |
| Headers/Footers | Manual via HTML/CSS | Full HTML |
| Digital Signatures | No | Yes |
| PDF/A | No | Yes |
| Form Filling | No | Yes |
| Text Extraction | No | Yes |
| Image Extraction | No | Yes |
| Async Support | Async-only API | Yes |
| Cross-Platform | .NET 8 (Windows/Linux/macOS) | Yes |

---

## Common Migration Issues

### Issue 1: Different API Pattern

**Problem:** PeachPDF separates `PdfGenerator` (engine) from `PdfGenerateConfig` (settings) and is async; IronPDF uses one `ChromePdfRenderer` that returns a `PdfDocument` synchronously.

**Solution:**
```csharp
// PeachPDF pattern (async, stream-out)
var config = new PdfGenerateConfig { PageSize = PageSize.Letter };
var generator = new PdfGenerator();
var document = await generator.GeneratePdf(html, config);
using var fs = File.Create(path);
document.Save(fs);

// IronPDF pattern
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs(path);
```

### Issue 2: No Existing-PDF Operations

**Problem:** PeachPDF is HTML-to-PDF only; it cannot load, merge, split, or edit existing PDFs.

**Solution:** Replace those operations with IronPDF's `PdfDocument.FromFile`, `PdfDocument.Merge`, and `pdf.AppendPdf` once migrated.

### Issue 3: Save Targets a Stream, not a Path

**Problem:** PeachPDF's `document.Save(stream)` only takes a `Stream`; IronPDF's `pdf.SaveAs(path)` takes a path string.

**Solution:** Use `pdf.SaveAs("file.pdf")` directly in IronPDF instead of opening a `FileStream`.

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit PeachPDF usage in codebase**
  ```bash
  grep -r "using PeachPDF" --include="*.cs" .
  grep -r "PdfGenerator\|PdfGenerateConfig\|GeneratePdf" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document custom configurations**
  ```csharp
  // Find patterns like:
  var config = new PdfGenerateConfig {
      PageSize = PageSize.Letter,
      PageOrientation = PageOrientation.Portrait,
      NetworkAdapter = new HttpClientNetworkAdapter(http, baseUri)
  };
  ```
  **Why:** These settings map to IronPDF's `RenderingOptions` (PaperSize, PaperOrientation, etc.). Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Replace NuGet package**
  ```bash
  dotnet remove package PeachPDF
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Update namespaces**
  ```csharp
  // Before (PeachPDF)
  using PeachPDF;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure the code references the correct library.

- [ ] **Convert API calls**
  ```csharp
  // Before (PeachPDF)
  var config = new PdfGenerateConfig { PageSize = PageSize.Letter };
  var generator = new PdfGenerator();
  var document = await generator.GeneratePdf(html, config);
  using var fs = File.Create("output.pdf");
  document.Save(fs);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering for accurate c# html to pdf HTML/CSS support.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Update save methods**
  ```csharp
  // Before (PeachPDF)
  using var fs = File.Create("output.pdf");
  document.Save(fs); // Save takes a Stream

  // After (IronPDF)
  pdf.SaveAs("output.pdf"); // SaveAs takes a path
  ```
  **Why:** Ensure the correct method is used for saving PDFs in IronPDF.

### Testing

- [ ] **Test HTML rendering**
  **Why:** Verify that HTML content renders correctly using IronPDF's Chromium engine.

- [ ] **Verify PDF output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Test merge operations**
  ```csharp
  // Before (PeachPDF): not supported -- you previously had to drop down to
  // PdfSharpCore to merge PDFs.

  // After (IronPDF)
  var merged = PdfDocument.Merge(pdf1, pdf2);
  ```
  **Why:** Ensure PDF merging works correctly with IronPDF's API.

- [ ] **Verify any security settings**
  ```csharp
  // Before (PeachPDF): no password / encryption API.

  // After (IronPDF)
  pdf.SecuritySettings.UserPassword = "password";
  ```
  **Why:** Ensure that security settings are correctly applied in IronPDF.

- [ ] **Performance comparison**
  **Why:** Compare performance between PeachPDF and IronPDF to ensure that the migration does not negatively impact application performance.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
