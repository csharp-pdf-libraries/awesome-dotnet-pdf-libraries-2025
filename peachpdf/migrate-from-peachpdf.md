# How Do I Migrate from PeachPDF to IronPDF in C#?

## Why Migrate from PeachPDF?

PeachPDF is a relatively new, lesser-known PDF library that lacks the maturity, features, and support of established solutions. Key reasons to migrate:

1. **Limited Feature Set**: PeachPDF lacks advanced features like digital signatures, PDF/A compliance, and sophisticated text extraction
2. **Small Community**: Limited documentation, examples, and community support
3. **Uncertain Future**: New libraries without established track records carry adoption risk
4. **Basic HTML Support**: Limited CSS and JavaScript rendering capabilities
5. **No Enterprise Support**: No professional support or SLA options

### Quick Comparison

| Aspect | PeachPDF | IronPDF |
|--------|----------|---------|
| Maturity | New | Established (40M+ downloads) |
| HTML Rendering | Basic | Full Chromium html to pdf c# |
| CSS Support | Limited | Full CSS3 |
| JavaScript | Basic | Full ES2024 |
| Digital Signatures | No | Yes |
| PDF/A Compliance | No | Yes |
| Professional Support | No | Yes |
| Documentation | Limited | Extensive |

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
| `PdfDocument.Create()` | `new ChromePdfRenderer()` | Create renderer |
| `document.AddHtmlContent(html)` | `renderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| `document.Save(path)` | `pdf.SaveAs(path)` | Save file |
| `document.ToByteArray()` | `pdf.BinaryData` | Get bytes |
| `PdfReader.LoadFromFile(path)` | `PdfDocument.FromFile(path)` | Load PDF |
| `document.AddPage()` | `pdf.AddPdfPages(newPdf)` | Add pages |
| `document.SetMetadata()` | `pdf.MetaData` | Set properties |
| `document.MergeWith(other)` | `PdfDocument.Merge(pdfs)` | Merge PDFs |

---

## Code Examples

### Example 1: Basic HTML to PDF

**PeachPDF:**
```csharp
using PeachPDF;

var document = PdfDocument.Create();
document.AddHtmlContent("<h1>Hello World</h1><p>Sample content</p>");
document.Save("output.pdf");
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
// Limited or no URL support
var document = PdfDocument.Create();
// Would need to fetch HTML manually
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
using PeachPDF;

var document = PdfReader.LoadFromFile("input.pdf");
document.AddPage();
document.Save("modified.pdf");
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
using PeachPDF;

var doc1 = PdfReader.LoadFromFile("doc1.pdf");
var doc2 = PdfReader.LoadFromFile("doc2.pdf");
doc1.MergeWith(doc2);
doc1.Save("merged.pdf");
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
// Limited header/footer support
var document = PdfDocument.Create();
document.AddHtmlContent("<div>Header</div><h1>Content</h1>");
document.Save("output.pdf");
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
// Synchronous only
var document = PdfDocument.Create();
document.AddHtmlContent(html);
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
| HTML to PDF | Basic | Full Chromium |
| URL to PDF | Limited | Yes |
| CSS Grid/Flexbox | No | Yes |
| JavaScript | Limited | Full ES2024 |
| Merge PDFs | Yes | Yes |
| Split PDFs | Limited | Yes |
| Watermarks | Limited | Full HTML |
| Headers/Footers | Basic | Full HTML |
| Digital Signatures | No | Yes |
| PDF/A | No | Yes |
| Form Filling | Limited | Yes |
| Text Extraction | Basic | Yes |
| Image Extraction | No | Yes |
| Async Support | Limited | Yes |
| Cross-Platform | Unknown | Yes |

---

## Common Migration Issues

### Issue 1: Different API Pattern

**Problem:** PeachPDF uses builder pattern, IronPDF uses separate renderer.

**Solution:**
```csharp
// PeachPDF pattern
var document = PdfDocument.Create();
document.AddHtmlContent(html);
document.Save(path);

// IronPDF pattern
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs(path);
```

### Issue 2: Page Addition Differences

**Problem:** PeachPDF adds empty pages, IronPDF adds content pages.

**Solution:**
```csharp
// Create a blank page in IronPDF
var blankPage = renderer.RenderHtmlAsPdf("<div style='height: 100vh;'></div>");
pdf.AppendPdf(blankPage);
```

### Issue 3: Save vs SaveAs

**Problem:** Method naming differs.

**Solution:** Use `SaveAs()` in IronPDF instead of `Save()`.

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit PeachPDF usage in codebase**
  ```bash
  grep -r "using PeachPDF" --include="*.cs" .
  grep -r "PdfDocument\|AddHtmlContent" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document custom configurations**
  ```csharp
  // Find patterns like:
  var config = new PeachPdfOptions {
      CustomSetting1 = value1,
      CustomSetting2 = value2
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

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
  var document = PdfDocument.Create();
  document.AddHtmlContent(html);
  document.Save("output.pdf");

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
  document.Save("output.pdf");

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Ensure the correct method is used for saving PDFs in IronPDF.

### Testing

- [ ] **Test HTML rendering**
  **Why:** Verify that HTML content renders correctly using IronPDF's Chromium engine.

- [ ] **Verify PDF output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Test merge operations**
  ```csharp
  // Before (PeachPDF)
  document.MergeWith(otherDocument);

  // After (IronPDF)
  var merged = PdfDocument.Merge(pdf1, pdf2);
  ```
  **Why:** Ensure PDF merging works correctly with IronPDF's API.

- [ ] **Verify any security settings**
  ```csharp
  // Before (PeachPDF)
  document.SetPassword("password");

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
