# How Do I Migrate from Scryber.Core to IronPDF in C#?

## Why Migrate from Scryber.Core?

Scryber.Core is an open-source PDF library that uses XML/HTML templates with a custom parsing engine. While it offers data binding capabilities, several limitations drive developers to migrate:

1. **LGPL License Concerns**: Requires source disclosure in some commercial scenarios
2. **Custom Template Syntax**: Proprietary binding syntax requires learning curve
3. **Limited CSS Support**: Not a full browser-based renderer
4. **Smaller Community**: Less documentation and community examples
5. **No JavaScript Execution**: Static rendering only
6. **Complex Configuration**: XML-heavy configuration approach

### Quick Comparison

| Aspect | Scryber.Core | IronPDF |
|--------|--------------|---------|
| License | LGPL (restrictive) | Commercial |
| Rendering Engine | Custom | Chromium |
| CSS Support | Limited | Full CSS3 |
| JavaScript | No | Full ES2024 |
| Template Binding | Proprietary XML | Standard (Razor, etc.) |
| Learning Curve | Custom syntax | Standard HTML/CSS |
| Async Support | Limited | Full |
| Documentation | Basic | Extensive |

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
using Scryber.Components;
using Scryber.Components.Pdf;
using Scryber.PDF;
using Scryber.Styles;

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
| `Document.ParseDocument(html)` | `renderer.RenderHtmlAsPdf(html)` | HTML rendering |
| `Document.ParseTemplate(path)` | `renderer.RenderHtmlFileAsPdf(path)` | File rendering |
| `doc.SaveAsPDF(path)` | `pdf.SaveAs(path)` | Save to file |
| `doc.SaveAsPDF(stream)` | `pdf.Stream` or `pdf.BinaryData` | Get stream/bytes |
| `doc.Info.Title` | `pdf.MetaData.Title` | Metadata |
| `doc.Info.Author` | `pdf.MetaData.Author` | Metadata |
| `PDFPage` | `pdf.Pages[i]` | Page access |
| `PDFLayoutDocument` | `RenderingOptions` | Layout control |
| `PDFStyle` | CSS in HTML | Styling |
| Data binding (`{{value}}`) | Razor/String interpolation | Templating |

---

## Code Examples

### Example 1: Basic HTML to PDF

**Scryber.Core:**
```csharp
using Scryber.Components;

var html = @"<html xmlns='http://www.w3.org/1999/xhtml'>
    <body>
        <h1>Hello World</h1>
        <p>Generated with Scryber</p>
    </body>
</html>";

using (var doc = Document.ParseDocument(new System.IO.StringReader(html), ParseSourceType.DynamicContent))
{
    doc.SaveAsPDF("output.pdf");
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

**Scryber.Core (proprietary binding):**
```csharp
using Scryber.Components;

// Scryber XML template with binding
var template = @"<?xml version='1.0' encoding='utf-8' ?>
<pdf:Document xmlns:pdf='http://www.scryber.co.uk/schemas/core/release/v1/Scryber.Components.xsd'>
    <Pages>
        <pdf:Page>
            <Content>
                <pdf:H1 text='{{model.Title}}' />
                <pdf:Para text='Customer: {{model.CustomerName}}' />
                <pdf:Para text='Amount: {{model.Amount}}' />
            </Content>
        </pdf:Page>
    </Pages>
</pdf:Document>";

var model = new { Title = "Invoice", CustomerName = "John Doe", Amount = "$150.00" };

using (var doc = Document.ParseDocument(new System.IO.StringReader(template), ParseSourceType.DynamicContent))
{
    doc.Params["model"] = model;
    doc.SaveAsPDF("invoice.pdf");
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
using System.Net.Http;

// Must fetch HTML manually
var client = new HttpClient();
var html = await client.GetStringAsync("https://example.com");

// Parse and save
using (var doc = Document.ParseDocument(new System.IO.StringReader(html), ParseSourceType.DynamicContent))
{
    doc.SaveAsPDF("webpage.pdf");
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
using Scryber.Components;
using Scryber.Styles;

var template = @"<?xml version='1.0' encoding='utf-8' ?>
<pdf:Document xmlns:pdf='http://www.scryber.co.uk/schemas/core/release/v1/Scryber.Components.xsd'>
    <Styles>
        <pdf:Style applied-type='pdf:Page'>
            <pdf:Size paper-size='A4' orientation='Portrait' />
            <pdf:Margins all='20pt' />
        </pdf:Style>
    </Styles>
    <Pages>
        <pdf:Page>
            <Content>
                <pdf:H1 text='Report' />
            </Content>
        </pdf:Page>
    </Pages>
</pdf:Document>";
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
// XML-based header/footer definition required
var template = @"<?xml version='1.0' encoding='utf-8' ?>
<pdf:Document xmlns:pdf='http://www.scryber.co.uk/schemas/core/release/v1/Scryber.Components.xsd'>
    <Pages>
        <pdf:Section>
            <Header>
                <pdf:Para text='Company Report' />
            </Header>
            <Footer>
                <pdf:Para text='Page {{pagenum}} of {{pagetotal}}' />
            </Footer>
            <Content>
                <pdf:H1 text='Content Here' />
            </Content>
        </pdf:Section>
    </Pages>
</pdf:Document>";
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
// Limited built-in merge support
// Often requires external library
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
using (var doc = Document.ParseDocument(reader, ParseSourceType.DynamicContent))
{
    doc.Info.Title = "My Document";
    doc.Info.Author = "John Doe";
    // Limited security options
    doc.SaveAsPDF("output.pdf");
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
| HTML to PDF | Basic | Full Chromium |
| URL to PDF | Manual fetch | Native support |
| CSS Grid | Limited | Full support |
| Flexbox | Limited | Full support |
| JavaScript | No | Full ES2024 |
| Data Binding | Proprietary XML | Use Razor/Handlebars |
| Headers/Footers | XML-based | HTML/CSS |
| Merge PDFs | Limited | Built-in |
| Split PDFs | No | Yes |
| Watermarks | Basic | Full HTML |
| Digital Signatures | No | Yes |
| PDF/A | No | Yes |
| Password Protection | Basic | Full |
| Async Support | Limited | Full |
| Cross-Platform | Yes | Yes |

---

## Template Migration Patterns

### Migrating Proprietary Binding to Standard Templates

**Scryber Binding:**
```xml
<pdf:Para text='{{model.Name}}' />
<pdf:Para text='Total: {{model.Total:C}}' />
<pdf:ForEach on='{{model.Items}}'>
    <pdf:Para text='{{.Name}}: {{.Price}}' />
</pdf:ForEach>
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
  grep -r "<scryber:Document" --include="*.xml" .
  ```
  **Why:** Identify all Scryber templates to ensure they are converted to HTML for IronPDF.

- [ ] **Document data binding patterns used**
  ```xml
  <!-- Before (Scryber) -->
  <scryber:TextField Value="{{dataBinding}}" />

  <!-- Document these patterns for conversion -->
  ```
  **Why:** Scryber uses proprietary data binding that needs to be converted to standard templating like Razor.

- [ ] **Identify custom styles**
  ```xml
  <!-- Before (Scryber) -->
  <scryber:Style>
      <scryber:Font Size="12pt" />
  </scryber:Style>
  ```
  **Why:** Custom styles need to be translated into CSS for IronPDF's HTML-based rendering.

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
  using Scryber;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure code references the correct library for PDF operations.

- [ ] **Convert XML templates to HTML**
  ```html
  <!-- Before (Scryber) -->
  <scryber:Document>
      <scryber:TextField Value="{{data}}" />
  </scryber:Document>

  <!-- After (IronPDF) -->
  <html>
      <body>
          <p>@Model.Data</p>
      </body>
  </html>
  ```
  **Why:** IronPDF uses standard HTML, allowing for more flexible and powerful templating.

- [ ] **Replace proprietary binding with standard templating**
  ```csharp
  // Before (Scryber)
  var template = "<scryber:TextField Value=\"{{data}}\" />";

  // After (IronPDF)
  var template = "<p>@Model.Data</p>";
  ```
  **Why:** Use familiar templating engines like Razor for better maintainability and readability.

- [ ] **Update page settings to RenderingOptions**
  ```csharp
  // Before (Scryber)
  var doc = new PDFDocument();
  doc.PageSize = PageSize.A4;

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** Centralize page settings using IronPDF's RenderingOptions for consistency.

- [ ] **Convert headers/footers to HTML**
  ```csharp
  // Before (Scryber)
  doc.Header = "Page [page] of [total]";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** Utilize HTML for dynamic headers/footers with placeholders for enhanced flexibility.

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
