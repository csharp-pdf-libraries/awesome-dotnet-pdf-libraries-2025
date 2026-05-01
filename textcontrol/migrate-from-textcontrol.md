# How Do I Migrate from TX Text Control to IronPDF in C#?

## Why Migrate from TX Text Control?

TX Text Control (Text Control GmbH; current version 34.0 SP2, February 2026) is a Word-processing component whose primary strength is DOCX/RTF editing — PDF generation is a secondary export path. Key reasons to migrate:

1. **Expensive Licensing**: A single TX Text Control .NET Server developer license is $4,198 (perpetual, with one year of updates); annual subscription renewals are ~40% of list price (about $1,698/yr/developer) and the renewal window closes 30 days after expiration.
2. **PDF as Afterthought**: Core architecture is word processing, not PDF
3. **Word-Processor Architecture**: Not optimized for HTML-to-PDF workflows
4. **Heavy Deployment**: The ServerTextControl runtime ships via a licensed Windows installer/MSI (or a private NuGet feed for licensed customers); the public `TXTextControl.Web` package is the editor companion, not a self-contained headless server.
5. **STA-Affinity in ASP.NET**: ServerTextControl traces back to a STA COM-style component model and historically requires STA-compatible hosting in ASP.NET pipelines.
6. **Complex API**: ServerTextControl context management, separate `StringStreamType` / `BinaryStreamType` / `StreamType` enums, and a selection-driven document model

### Cost Comparison

| Aspect | TX Text Control .NET Server | IronPDF |
|--------|-----------------------------|---------|
| Single developer | $4,198 perpetual + 1yr support | One-time per-developer license |
| Team of 4 | $8,398 perpetual + 1yr support | Lower per-seat |
| Annual Renewal | ~40% of list (~$1,698/dev/yr) | Optional support |
| Server runtime | Separate runtime/OEM license required for production servers | None |
| UI Components | Bundled DOCX editor surface | PDF-focused

These costs and capabilities are explored in detail in the [comprehensive migration reference](https://ironpdf.com/blog/migration-guides/migrate-from-textcontrol-to-ironpdf/).

---

## Quick Start: TX Text Control to IronPDF

### Step 1: Replace NuGet Package

TX Text Control's headless server runtime is not distributed as a single public NuGet package — the public package is `TXTextControl.Web` (editor) and the actual `ServerTextControl` runtime comes from the licensed Windows installer or the vendor's private NuGet feed. So removal usually means uninstalling those installer-shipped packages plus `TXTextControl.Web` and uninstalling the MSI.

```bash
# Remove TX Text Control packages (names depend on which were installed
# from the licensed installer / private feed / public NuGet)
dotnet remove package TXTextControl.Web

# Then install IronPDF (single self-contained package)
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
| `tx.Load(html, StringStreamType.HTMLFormat)` | `renderer.RenderHtmlAsPdf(html)` | String overload uses `StringStreamType` |
| `tx.Load(bytes, BinaryStreamType.AdobePDF)` | `PdfDocument.FromFile(...)` / `FromBinaryData(...)` | Byte[] overload uses `BinaryStreamType` |
| `tx.Save(path, StreamType.AdobePDF)` | `pdf.SaveAs(path)` | File-path Save uses `StreamType` |
| `tx.Append(bytes, BinaryStreamType.AdobePDF, AppendSettings.StartWithNewSection)` | `PdfDocument.Merge(a, b)` | Concatenate documents |
| `SaveSettings.PDFAConformance` | `RenderingOptions.PdfAFormat` | PDF/A |
| `DocumentServer.MailMerge` | HTML templates + Razor | Template merging |
| `HeaderFooter` + `Sections[i].HeadersAndFooters` | `HtmlHeaderFooter` / `TextHeaderFooter` | Headers/footers |
| `LoadSettings` | `RenderingOptions` | Configuration |
| `StreamType.AdobePDF` | Default output | PDF is primary |

---

## Code Examples

### Example 1: Basic HTML to PDF

**TX Text Control:**
```csharp
using TXTextControl;

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    // String overload uses StringStreamType
    tx.Load("<html><body><h1>Invoice</h1></body></html>", StringStreamType.HTMLFormat);
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

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();

    // ServerTextControl does not fetch URLs directly; in practice you download the
    // HTML yourself (HttpClient) and load it as a string with StringStreamType.HTMLFormat.
    string html = new System.Net.Http.HttpClient().GetStringAsync("https://example.com/invoice").Result;

    LoadSettings loadSettings = new LoadSettings();
    loadSettings.ApplicationFieldFormat = ApplicationFieldFormat.MSWord;
    tx.Load(html, StringStreamType.HTMLFormat, loadSettings);

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

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    tx.Load(htmlContent, StringStreamType.HTMLFormat);

    // HeaderFooter objects are added to a section's HeadersAndFooters collection
    HeaderFooter header = new HeaderFooter(HeaderFooterType.Header);
    header.Text = "Company Report";
    tx.Sections[0].HeadersAndFooters.Add(header);

    HeaderFooter footer = new HeaderFooter(HeaderFooterType.Footer);
    footer.Text = "Page {page} of {numpages}";
    tx.Sections[0].HeadersAndFooters.Add(footer);

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
    // Load a DOCX template (note: TX Text Control's internal "TX" format is StreamType.InternalUnicodeFormat;
    // for DOCX use StreamType.WordprocessingML).
    tx.Load("template.docx", StreamType.WordprocessingML);

    // DocumentServer.MailMerge merges template merge fields with a data source.
    MailMerge mailMerge = new MailMerge();
    var data = new[] { new { CustomerName = "John Doe", InvoiceNumber = "12345", Total = "$1,500.00" } };
    mailMerge.MergeObjects(data, tx);

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

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    tx.Load(html, StringStreamType.HTMLFormat);

    // Page settings are configured per Section.Format (page size in TWIPS).
    foreach (Section section in tx.Sections)
    {
        section.Format.PageSize = new System.Drawing.Size(11906, 16838); // A4 in TWIPS
        section.Format.PageMargins = new PageMargins(
            1440, 1440, 1440, 1440); // TWIPS = 1/1440 inch
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

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    tx.Load(html, StringStreamType.HTMLFormat);

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

### Issue 4: STA-Affinity in ASP.NET

**TX Text Control:** ServerTextControl historically traces back to a STA COM-style component model and is most reliable when hosted on STA-compatible threads (e.g. `[ASPCOMPAT]` in classic Web Forms, or wrapping calls on a dedicated STA thread/queue in ASP.NET Core). Mixing it freely with MTA worker threads can produce occasional thread-affinity errors under load.

**Solution:** IronPDF has no STA requirement and runs unmodified on MTA worker threads, in async controllers, and inside Linux containers under .NET 6/7/8/9.

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

- [ ] **Convert StringStreamType.HTMLFormat to RenderHtmlAsPdf**
  ```csharp
  // Before (TX Text Control)
  tx.Load(html, StringStreamType.HTMLFormat);

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** Direct HTML rendering with IronPDF's modern Chromium engine; no need to juggle `StringStreamType` vs `BinaryStreamType` vs `StreamType`.

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

- [ ] **Verify ASP.NET Core hosting on MTA threads / Linux containers**
  **Why:** Removes the STA-affinity considerations that ServerTextControl inherits from its COM-style threading model.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
