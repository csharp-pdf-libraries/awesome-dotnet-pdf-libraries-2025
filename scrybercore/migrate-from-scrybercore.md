# How Do I Migrate from Scryber.Core to IronPDF in C#?

## Why Migrate from Scryber.Core?

Scryber.Core (maintained by Richard Hewitson — `richard-scryber` on GitHub, current version 9.3.1) is an open-source PDF library that parses XHTML+CSS templates with its own layout engine — not a browser. While it supports CSS styling, Handlebars-style `{{ }}` data binding, and SVG, several limitations drive developers to migrate:

1. **LGPL v3 License**: Source-disclosure obligations apply if you modify Scryber itself; static linking in closed-source apps is permitted but the terms still constrain how you ship modifications.
2. **XHTML-strict Input**: Templates must be valid XML — most real-world HTML needs to be cleaned up before Scryber will parse it.
3. **Non-browser Renderer**: A custom layout engine, not Chromium — modern CSS (Grid, Flexbox subset, container queries) and any JavaScript are not executed.
4. **Smaller Community**: ~226 GitHub stars, ~36 forks; fewer worked examples than mainstream libraries.
5. **No JavaScript Execution**: Static rendering only — charts, SPAs, and dynamic widgets must be pre-rendered.
6. **Manual URL Fetch**: No native URL-to-PDF; you fetch HTML yourself with `HttpClient` and feed it in.

### Quick Comparison

| Aspect | Scryber.Core | IronPDF |
|--------|--------------|---------|
| License | LGPL v3 | Commercial |
| Rendering Engine | Custom XHTML/CSS layout engine | Chromium |
| CSS Support | Subset (no Grid, partial Flexbox) | Full CSS3 |
| JavaScript | No | Full (Chromium V8) |
| Template Binding | Handlebars-style `{{ }}` + `<template>` | Standard (Razor, RazorLight, etc.) |
| Input Strictness | Requires well-formed XHTML | Accepts real-world HTML |
| Async Support | `PDFAsync` helper for MVC; sync core | Full async API |
| Documentation | Read-the-docs site + samples | Extensive |

---

## Quick Start: Scryber.Core to IronPDF

### Step 1: Replace NuGet Package

```bash
# Remove Scryber.Core
dotnet remove package Scryber.Core

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using Scryber.Components;     // Document, Page, etc.
using Scryber.Drawing;        // Units, colours
using Scryber.PDF;            // RenderOptions / OutputCompressionType
using Scryber.Styles;         // Style elements (older XML template usage)

// After
using IronPdf;
```

### Step 3: Initialize License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## API Mapping Reference

| Scryber.Core | IronPDF | Notes |
|--------------|---------|-------|
| `Document.ParseDocument(reader, "", ParseSourceType.DynamicContent)` | `renderer.RenderHtmlAsPdf(html)` | Parse in-memory HTML |
| `Document.ParseDocument(path)` | `renderer.RenderHtmlFileAsPdf(path)` | File-based template |
| `doc.SaveAsPDF(stream)` | `pdf.SaveAs(path)` / `pdf.BinaryData` | Output |
| `doc.Info.Title` / `doc.Info.Author` | `pdf.MetaData.Title` / `pdf.MetaData.Author` | Metadata |
| `doc.RenderOptions` | `renderer.RenderingOptions` | Output-side options |
| `@page` CSS in template | `renderer.RenderingOptions.PaperSize` / margins | Page layout |
| Handlebars `{{value}}` + `<template>` | Razor / string interpolation | Templating |

---

## Code Examples

### Example 1: Basic HTML to PDF

**Scryber.Core:**
```csharp
using Scryber.Components;
using System.IO;

var html = @"<html xmlns='http://www.w3.org/1999/xhtml'>
    <body>
        <h1>Hello World</h1>
        <p>Generated with Scryber</p>
    </body>
</html>";

using (var reader = new StringReader(html))
using (var doc = Document.ParseDocument(reader, string.Empty, ParseSourceType.DynamicContent))
using (var stream = new FileStream("output.pdf", FileMode.Create))
{
    doc.SaveAsPDF(stream);
}
```

**IronPDF:**
```csharp
using IronPdf;

var html = @"
<html>
<body>
    <h1>Hello World</h1>
    <p>Generated with IronPDF</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: Template with Data Binding

**Scryber.Core (XHTML + Handlebars-style binding):**
```csharp
using Scryber.Components;
using System.IO;

// Scryber 6+ favours XHTML with {{ }} binding via doc.Params
var template = @"<html xmlns='http://www.w3.org/1999/xhtml'>
    <body>
        <h1>{{model.Title}}</h1>
        <p>Customer: {{model.CustomerName}}</p>
        <p>Amount: {{model.Amount}}</p>
    </body>
</html>";

var model = new { Title = "Invoice", CustomerName = "John Doe", Amount = "$150.00" };

using (var reader = new StringReader(template))
using (var doc = Document.ParseDocument(reader, string.Empty, ParseSourceType.DynamicContent))
using (var stream = new FileStream("invoice.pdf", FileMode.Create))
{
    doc.Params["model"] = model;
    doc.SaveAsPDF(stream);
}
```

**IronPDF (use any template engine):**
```csharp
using IronPdf;

// Use standard C# string interpolation
var model = new { Title = "Invoice", CustomerName = "John Doe", Amount = "$150.00" };

var html = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        h1 {{ color: #333; border-bottom: 2px solid #007bff; }}
        .amount {{ font-size: 24px; color: green; }}
    </style>
</head>
<body>
    <h1>{model.Title}</h1>
    <p><strong>Customer:</strong> {model.CustomerName}</p>
    <p class='amount'><strong>Amount:</strong> {model.Amount}</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

### Example 3: URL to PDF

**Scryber.Core:**
```csharp
using Scryber.Components;
using System.IO;
using System.Net.Http;

// Scryber has no native URL-to-PDF — fetch the HTML yourself.
// Note: most live HTML is not valid XHTML; expect to clean it
// (e.g., HtmlAgilityPack) before Scryber will parse it.
var client = new HttpClient();
var html = await client.GetStringAsync("https://example.com");

using (var reader = new StringReader(html))
using (var doc = Document.ParseDocument(reader, string.Empty, ParseSourceType.DynamicContent))
using (var stream = new FileStream("webpage.pdf", FileMode.Create))
{
    doc.SaveAsPDF(stream);
}
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

### Example 4: Page Settings and Margins

**Scryber.Core:**
```csharp
// Page size and margins are declared in the template via @page CSS.
var template = @"<html xmlns='http://www.w3.org/1999/xhtml'>
    <head>
        <style>
            @page { size: A4 portrait; margin: 20pt; }
        </style>
    </head>
    <body>
        <h1>Report</h1>
    </body>
</html>";
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Page settings
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

// Margins in mm
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;

var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1>");
pdf.SaveAs("report.pdf");
```

### Example 5: Headers and Footers

**Scryber.Core:**
```csharp
// Headers and footers live in the XHTML template via @page margin boxes
// or explicit <header> / <footer> sections; Scryber resolves page-number
// expressions at layout time.
var template = @"<html xmlns='http://www.w3.org/1999/xhtml'>
    <head>
        <style>
            @page { @top-center { content: 'Company Report'; }
                    @bottom-center { content: 'Page ' counter(page) ' of ' counter(pages); } }
        </style>
    </head>
    <body>
        <h1>Content Here</h1>
    </body>
</html>";
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// HTML header with full CSS support
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; text-align: center; font-size: 12pt; border-bottom: 1px solid #ccc;'>
            Company Report
        </div>",
    MaxHeight = 30
};

// HTML footer with page numbers
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; text-align: center; font-size: 10pt;'>
            Page {page} of {total-pages}
        </div>",
    MaxHeight = 25
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Content Here</h1>");
pdf.SaveAs("report.pdf");
```

### Example 6: Merging PDFs

**Scryber.Core:**
```csharp
// Scryber has no PDF-to-PDF merge API — its mandate is generating
// new PDFs from XHTML. To combine existing PDFs you need a separate
// library (PdfSharp, iText, etc.).
```

**IronPDF:**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("chapter1.pdf");
var pdf2 = PdfDocument.FromFile("chapter2.pdf");
var pdf3 = PdfDocument.FromFile("chapter3.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("complete_book.pdf");
```

### Example 7: Security and Metadata

**Scryber.Core:**
```csharp
using (var doc = Document.ParseDocument(reader, string.Empty, ParseSourceType.DynamicContent))
using (var stream = new FileStream("output.pdf", FileMode.Create))
{
    doc.Info.Title = "My Document";
    doc.Info.Author = "John Doe";
    // Encryption / password protection is not part of the public API.
    doc.SaveAsPDF(stream);
}
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential</h1>");

// Metadata
pdf.MetaData.Title = "My Document";
pdf.MetaData.Author = "John Doe";
pdf.MetaData.Subject = "Annual Report";
pdf.MetaData.Keywords = "report, annual, confidential";

// Security
pdf.SecuritySettings.OwnerPassword = "owner123";
pdf.SecuritySettings.UserPassword = "user456";
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;

pdf.SaveAs("protected.pdf");
```

---

## Feature Comparison

| Feature | Scryber.Core | IronPDF |
|---------|--------------|---------|
| HTML to PDF | XHTML+CSS via custom layout engine | Full Chromium |
| URL to PDF | Manual fetch only | Native `RenderUrlAsPdf` |
| CSS Grid | Not supported | Full support |
| Flexbox | Partial | Full support |
| JavaScript | Not executed | Full (V8) |
| Data Binding | `{{ }}` Handlebars + `doc.Params` | Razor / RazorLight / interpolation |
| Headers/Footers | `@page` margin boxes / template sections | HtmlHeaderFooter with `{page}` `{total-pages}` |
| Merge PDFs | Not supported | Built-in `PdfDocument.Merge` |
| Split PDFs | Not supported | Yes |
| Watermarks | Via overlay HTML in template | Native `ApplyStamp` / `ApplyWatermark` |
| Digital Signatures | Not supported | Yes |
| PDF/A | Not supported | Yes (PDF/A-1b, PDF/A-3) |
| Password Protection | Not supported | Full (user/owner + permissions) |
| Async Support | `PDFAsync` MVC helper; sync core | Full async surface |
| Cross-Platform | Yes (.NET Std 2.0/2.1, .NET 8/9/10) | Yes |

---

## Template Migration Patterns

### Migrating Scryber Binding to Standard Templates

**Scryber Binding (XHTML + Handlebars):**
```xml
<p>{{model.Name}}</p>
<p>Total: {{model.Total}}</p>
<template data-bind='{{model.Items}}'>
    <p>{{.Name}}: {{.Price}}</p>
</template>
```

**IronPDF with C# String Interpolation:**
```csharp
var items = model.Items.Select(i => $"<li>{i.Name}: {i.Price:C}</li>");

var html = $@"
<p>{model.Name}</p>
<p>Total: {model.Total:C}</p>
<ul>
    {string.Join("", items)}
</ul>";
```

**IronPDF with Razor (recommended for complex templates):**
```csharp
// Use RazorLight or similar
var engine = new RazorLightEngineBuilder()
    .UseMemoryCachingProvider()
    .Build();

var html = await engine.CompileRenderStringAsync("template", template, model);
var pdf = renderer.RenderHtmlAsPdf(html);
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit all Scryber templates**
  ```bash
  grep -rE "ParseDocument|Scryber\.Components" --include="*.cs" --include="*.html" --include="*.xhtml" .
  ```
  **Why:** Identify every call site and template so they can all be re-rendered through IronPDF.

- [ ] **Document data binding patterns used**
  ```xml
  <!-- Before (Scryber XHTML) -->
  <p>{{model.Name}}</p>
  <template data-bind='{{model.Items}}'><p>{{.Name}}</p></template>
  ```
  **Why:** Scryber's `{{ }}` + `<template>` constructs need to be re-expressed in Razor or string interpolation.

- [ ] **Identify custom styles**
  ```html
  <!-- Before (Scryber) -->
  <style>
      @page { size: A4; margin: 20pt; }
      h1 { font-size: 24pt; }
  </style>
  ```
  **Why:** `@page` rules and unit-suffixed sizes need to be mapped to IronPDF's RenderingOptions or kept as standard CSS.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Replace NuGet package**
  ```bash
  dotnet remove package Scryber.Core
  dotnet add package IronPdf
  ```
  **Why:** Transition from Scryber to IronPDF for enhanced rendering capabilities.

- [ ] **Update namespaces**
  ```csharp
  // Before (Scryber)
  using Scryber.Components;
  using Scryber.PDF;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** IronPDF consolidates the Scryber.Components / Scryber.PDF / Scryber.Drawing tree under a single `IronPdf` namespace.

- [ ] **Convert XHTML templates to plain HTML**
  ```html
  <!-- Before (Scryber — must be valid XHTML with namespace) -->
  <html xmlns="http://www.w3.org/1999/xhtml">
      <body><p>{{model.Data}}</p></body>
  </html>

  <!-- After (IronPDF — any well-formed HTML5) -->
  <html><body><p>@Model.Data</p></body></html>
  ```
  **Why:** IronPDF parses with Chromium, so the strict XHTML namespace requirement and well-formedness rules go away.

- [ ] **Replace `{{ }}` binding with standard templating**
  ```csharp
  // Before (Scryber)
  doc.Params["model"] = model; // template uses {{model.Data}}

  // After (IronPDF)
  var html = $"<p>{model.Data}</p>"; // or Razor / RazorLight
  ```
  **Why:** Razor and string interpolation are first-class .NET tooling — debuggable and refactor-safe.

- [ ] **Move `@page` CSS to RenderingOptions**
  ```csharp
  // Before (Scryber — in template CSS)
  // @page { size: A4 portrait; margin: 20pt; }

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
  renderer.RenderingOptions.MarginTop = 20;
  ```
  **Why:** RenderingOptions is the single API surface for page geometry in IronPDF.

- [ ] **Convert template-bound headers/footers to HtmlHeaderFooter**
  ```csharp
  // Before (Scryber — @page margin boxes in CSS)
  // @page { @bottom-center { content: 'Page ' counter(page); } }

  // After (IronPDF)
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF's `{page}` / `{total-pages}` placeholders replace Scryber's CSS counter expressions.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations to enable full functionality.

### Testing

- [ ] **Test all document templates**
  **Why:** Ensure all templates render correctly with IronPDF's HTML-based approach.

- [ ] **Verify styling matches**
  **Why:** Confirm that the visual appearance is consistent post-migration, leveraging full CSS support.

- [ ] **Test data binding**
  **Why:** Validate that data is correctly bound and displayed using the new templating system.

- [ ] **Verify page breaks**
  **Why:** Ensure pagination is handled correctly with IronPDF's rendering engine.

- [ ] **Test headers/footers**
  **Why:** Confirm that dynamic content in headers/footers is rendered as expected.

- [ ] **Performance comparison**
  **Why:** Evaluate performance improvements with IronPDF's modern rendering engine.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [Headers and Footers Guide](https://ironpdf.com/how-to/headers-and-footers/)
- [Scryber.Core to IronPDF Migration Reference](https://ironpdf.com/blog/migration-guides/migrate-from-scryber-core-to-ironpdf/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
