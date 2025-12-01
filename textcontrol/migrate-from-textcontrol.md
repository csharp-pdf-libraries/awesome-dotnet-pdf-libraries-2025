# How Do I Migrate from TX Text Control to IronPDF in C#?

## Why Migrate from TX Text Control?

TX Text Control is a document editor component that treats PDF generation as a secondary feature. Key reasons to migrate:

1. **Expensive Licensing**: $3,398+/developer with mandatory 40% annual renewals
2. **PDF as Afterthought**: Core architecture is word processing, not PDF
3. **Hardware Bugs**: Known Intel Iris Xe Graphics rendering issues requiring registry workarounds
4. **Bloated Dependencies**: Includes document editing UI components you may not need
5. **Word-Processor Architecture**: Not optimized for HTML-to-PDF workflows
6. **Complex API**: ServerTextControl context management and selection model

### Cost Comparison

| Aspect | TX Text Control | IronPDF |
|--------|-----------------|---------|
| Base License | $3,398+ | Significantly lower |
| Annual Renewal | 40% mandatory | Optional support |
| Per Developer | Yes | Yes |
| UI Components | Bundled (bloat) | PDF-focused |
| Total 3-Year Cost | $5,750+ | Much lower |

---

## Quick Start: TX Text Control to IronPDF

### Step 1: Replace NuGet Package

```bash
# Remove TX Text Control
dotnet remove package TXTextControl.TextControl
dotnet remove package TXTextControl.DocumentServer

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using TXTextControl;
using TXTextControl.DocumentServer;

// After
using IronPdf;
```

### Step 3: Initialize License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## API Mapping Reference

| TX Text Control | IronPDF | Notes |
|-----------------|---------|-------|
| `ServerTextControl.Create()` | `new ChromePdfRenderer()` | No context management |
| `tx.Load(html, StreamType.HTMLFormat)` | `renderer.RenderHtmlAsPdf(html)` | Direct rendering |
| `tx.Load(url, StreamType.HTMLFormat)` | `renderer.RenderUrlAsPdf(url)` | URL support |
| `tx.Save(path, StreamType.AdobePDF)` | `pdf.SaveAs(path)` | Simple save |
| `SaveSettings.PDFAConformance` | `RenderingOptions.PdfAFormat` | PDF/A |
| `DocumentServer.MailMerge` | HTML templates + Razor | Template merging |
| `DocumentTarget.HeadersAndFooters` | `HtmlHeaderFooter` | Headers/footers |
| `LoadSettings` | `RenderingOptions` | Configuration |
| `StreamType.AdobePDF` | Default output | PDF is primary |

---

## Code Examples

### Example 1: Basic HTML to PDF

**TX Text Control:**
```csharp
using TXTextControl;
using TXTextControl.DocumentServer;

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    tx.Load("<html><body><h1>Invoice</h1></body></html>", StreamType.HTMLFormat);
    tx.Save("output.pdf", StreamType.AdobePDF);
}
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Invoice</h1></body></html>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with PDF/A Compliance

**TX Text Control:**
```csharp
using TXTextControl;
using TXTextControl.DocumentServer;

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();

    LoadSettings loadSettings = new LoadSettings();
    loadSettings.ApplicationFieldFormat = ApplicationFieldFormat.MSWord;
    tx.Load("https://example.com/invoice", StreamType.HTMLFormat, loadSettings);

    SaveSettings saveSettings = new SaveSettings();
    saveSettings.PDFAConformance = PDFAConformance.PDFa1b;
    tx.Save("output.pdf", StreamType.AdobePDF, saveSettings);
}
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// PDF/A compliance - simple property
renderer.RenderingOptions.PdfAFormat = PdfAVersions.PdfA1B;

var pdf = renderer.RenderUrlAsPdf("https://example.com/invoice");
pdf.SaveAs("output.pdf");
```

### Example 3: Headers and Footers

**TX Text Control:**
```csharp
using TXTextControl;
using TXTextControl.DocumentServer;

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    tx.Load(htmlContent, StreamType.HTMLFormat);

    // Complex DocumentTarget manipulation
    tx.Selection.DocumentTarget = DocumentTarget.HeadersAndFooters;
    tx.Selection.HeaderFooterTargetSection = HeaderFooterTargetSection.All;
    tx.Selection.HeaderFooterTargetPosition = HeaderFooterPosition.Header;
    tx.Text = "Company Report";

    tx.Selection.HeaderFooterTargetPosition = HeaderFooterPosition.Footer;
    tx.Text = "Page {page} of {numpages}";

    tx.Selection.DocumentTarget = DocumentTarget.Document;

    SaveSettings settings = new SaveSettings();
    settings.CreatorApplication = "My App";
    tx.Save("output.pdf", StreamType.AdobePDF, settings);
}
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Simple declarative header
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; text-align: center; font-size: 12pt;'>
            Company Report
        </div>",
    MaxHeight = 30
};

// Simple declarative footer
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; text-align: right; font-size: 10pt;'>
            Page {page} of {total-pages}
        </div>",
    MaxHeight = 25
};

var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.MetaData.Creator = "My App";
pdf.SaveAs("output.pdf");
```

### Example 4: Mail Merge Replacement

**TX Text Control:**
```csharp
using TXTextControl;
using TXTextControl.DocumentServer;

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    tx.Load("template.docx", StreamType.InternalUnicodeFormat);

    // Complex mail merge
    MailMerge mailMerge = new MailMerge(tx);
    mailMerge.MergeFields["CustomerName"].Text = "John Doe";
    mailMerge.MergeFields["InvoiceNumber"].Text = "12345";
    mailMerge.MergeFields["Total"].Text = "$1,500.00";

    tx.Save("invoice.pdf", StreamType.AdobePDF);
}
```

**IronPDF (HTML templates - simpler and more flexible):**
```csharp
using IronPdf;

// Use standard C# string interpolation
var data = new { CustomerName = "John Doe", InvoiceNumber = "12345", Total = "$1,500.00" };

var html = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial; padding: 40px; }}
        h1 {{ color: #333; }}
        .total {{ font-size: 24px; color: green; }}
    </style>
</head>
<body>
    <h1>Invoice #{data.InvoiceNumber}</h1>
    <p>Customer: {data.CustomerName}</p>
    <p class='total'>Total: {data.Total}</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

### Example 5: Page Settings

**TX Text Control:**
```csharp
using TXTextControl;
using TXTextControl.DocumentServer;

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    tx.Load(html, StreamType.HTMLFormat);

    // Complex page settings through sections
    foreach (Section section in tx.Sections)
    {
        section.Format.PageSize = PageSize.A4;
        section.Format.PageMargins = new PageMargins(
            1440, 1440, 1440, 1440); // TWIPS
        section.Format.Landscape = true;
    }

    tx.Save("output.pdf", StreamType.AdobePDF);
}
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 25;    // mm
renderer.RenderingOptions.MarginBottom = 25;
renderer.RenderingOptions.MarginLeft = 25;
renderer.RenderingOptions.MarginRight = 25;

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 6: Password Protection

**TX Text Control:**
```csharp
using TXTextControl;
using TXTextControl.DocumentServer;

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    tx.Load(html, StreamType.HTMLFormat);

    SaveSettings saveSettings = new SaveSettings();
    saveSettings.UserPassword = "user123";
    saveSettings.MasterPassword = "owner456";

    tx.Save("protected.pdf", StreamType.AdobePDF, saveSettings);
}
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);

pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.OwnerPassword = "owner456";
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;

pdf.SaveAs("protected.pdf");
```

---

## Feature Comparison

| Feature | TX Text Control | IronPDF |
|---------|-----------------|---------|
| HTML to PDF | Yes (secondary) | Yes (primary) |
| CSS Support | Limited | Full CSS3 |
| JavaScript | Limited | Full ES2024 |
| URL to PDF | Complex setup | Native |
| Headers/Footers | Complex API | Simple HTML |
| Mail Merge | Proprietary | HTML templates |
| PDF/A | Yes | Yes |
| Password Protection | Yes | Yes |
| Digital Signatures | Yes | Yes |
| Merge PDFs | Limited | Yes |
| Split PDFs | Limited | Yes |
| Watermarks | Complex | Simple HTML |
| **Non-PDF Features** | | |
| DOCX Editing | Yes | No |
| Rich Text Editor | Yes | No |
| **Architecture** | | |
| Context Management | Required | Not needed |
| Selection Model | Complex | N/A |
| Cross-Platform | Windows-focused | Yes |

---

## Common Migration Issues

### Issue 1: ServerTextControl Context

**TX Text Control:** Requires `Create()` and `using` block.

**Solution:** IronPDF has no context management:
```csharp
// Just create and use
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 2: StreamType Conversions

**TX Text Control:** Load different formats, convert to PDF.

**Solution:** IronPDF renders HTML directly:
```csharp
// No format conversion needed
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 3: DOCX Templates

**TX Text Control:** Uses DOCX files for templates.

**Solution:** Convert to HTML templates:
```csharp
// Use Razor or string interpolation for templates
var html = $"<h1>Invoice #{invoiceNumber}</h1>";
```

### Issue 4: Intel Iris Xe Graphics Bug

**TX Text Control:** Requires registry workarounds for 11th gen Intel.

**Solution:** IronPDF uses Chromium - no hardware acceleration bugs.

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit TX Text Control usage**
  ```bash
  grep -r "using TXTextControl" --include="*.cs" .
  grep -r "ServerTextControl\|Load\|Save" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document mail merge templates**
  ```csharp
  // Before (TX Text Control)
  var mailMerge = new DocumentServer.MailMerge();
  mailMerge.LoadTemplate("template.docx");

  // After (IronPDF)
  // Use HTML templates with Razor syntax
  ```
  **Why:** Transition to HTML templates for better integration with IronPDF's rendering capabilities.

- [ ] **Note header/footer requirements**
  ```csharp
  // Before (TX Text Control)
  var headersAndFooters = DocumentTarget.HeadersAndFooters;

  // After (IronPDF)
  var header = new HtmlHeaderFooter() { HtmlFragment = "<div>Header Content</div>" };
  var footer = new HtmlHeaderFooter() { HtmlFragment = "<div>Footer Content</div>" };
  ```
  **Why:** IronPDF uses HTML for headers/footers, allowing for more flexible and dynamic content.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Remove TX Text Control packages**
  ```bash
  dotnet remove package TXTextControl
  ```
  **Why:** Clean package removal to switch to IronPDF.

- [ ] **Install IronPdf package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to handle all PDF generation tasks.

- [ ] **Remove ServerTextControl context management**
  ```csharp
  // Before (TX Text Control)
  var server = new ServerTextControl();
  server.Create();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF does not require context management, simplifying the codebase.

- [ ] **Convert StreamType.HTMLFormat to RenderHtmlAsPdf**
  ```csharp
  // Before (TX Text Control)
  tx.Load(html, StreamType.HTMLFormat);

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** Direct HTML rendering with IronPDF's modern Chromium engine.

- [ ] **Convert mail merge to HTML templates**
  ```csharp
  // Before (TX Text Control)
  mailMerge.Merge();

  // After (IronPDF)
  // Use Razor or other templating engines to generate HTML
  ```
  **Why:** HTML templates provide a more flexible and robust solution for document generation.

- [ ] **Update headers/footers to HtmlHeaderFooter**
  ```csharp
  // Before (TX Text Control)
  tx.HeadersAndFooters = headersAndFooters;

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter() { HtmlFragment = "<div>Header Content</div>" };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter() { HtmlFragment = "<div>Footer Content</div>" };
  ```
  **Why:** IronPDF's HTML-based headers/footers allow for dynamic content and styling.

- [ ] **Simplify page settings**
  ```csharp
  // Before (TX Text Control)
  tx.PageSize = PageSizes.A4;
  tx.Orientation = Orientations.Landscape;

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  ```
  **Why:** Centralized page settings in IronPDF's RenderingOptions streamline configuration.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Testing

- [ ] **Test all document templates**
  **Why:** Ensure all templates render correctly with IronPDF.

- [ ] **Verify PDF/A compliance**
  ```csharp
  // Before (TX Text Control)
  tx.SaveSettings.PDFAConformance = PDFAConformance.PDF_A1b;

  // After (IronPDF)
  renderer.RenderingOptions.PdfAFormat = PdfAFormat.PdfA1b;
  ```
  **Why:** Maintain compliance with PDF/A standards for archival purposes.

- [ ] **Test password protection**
  ```csharp
  // After (IronPDF)
  pdf.SecuritySettings.UserPassword = "password";
  ```
  **Why:** Ensure sensitive documents are protected with passwords.

- [ ] **Verify headers/footers**
  **Why:** Confirm that all headers and footers render as expected with IronPDF's HTML capabilities.

- [ ] **Check on Intel 11th gen hardware (no more workarounds!)**
  **Why:** IronPDF's modern rendering engine eliminates hardware-specific issues, ensuring consistent performance.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
