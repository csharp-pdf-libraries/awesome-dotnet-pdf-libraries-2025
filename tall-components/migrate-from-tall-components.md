# How Do I Migrate from Tall Components (TallPDF, PDFKit) to IronPDF in C#?

## ⚠️ Tall Components is Closed to New Customers

**TallComponents was acquired by Apryse on May 27, 2025** ([press release](https://apryse.com/blog/apryse-acquires-tallcomponents)). The vendor's site now redirects to `apryse.com/brands/tallcomponents`, where Apryse states plainly: "We are no longer offering new licenses for TallComponents products" and points new buyers to the **iText SDK** (also owned by Apryse). Existing customers continue to receive support, and the NuGet packages remain published — `TallComponents.PDFKit5` was last refreshed in April 2026 — but no road-map development is happening on the TallComponents brand. Migrating now avoids a forced cutover later.

### Why You Should Plan a Migration

1. **Closed to new licenses**: post-acquisition, the only forward path Apryse offers is iText
2. **No road-map**: existing-customer support only — no new features, framework targets, or HTML5 engine
3. **XHTML-only HTML pipeline**: `XhtmlParagraph` parses **XHTML 1.0 Strict / XHTML 1.1 + CSS 2.1**, not modern HTML5/CSS3/JavaScript. That is unusable for most current marketing/dashboard HTML
4. **Legacy package shape**: PDFKit 4.x targets .NET Framework 2.0; PDFKit5 targets .NET Standard 2.0 — fine, but no .NET 8+ TFM is published
5. **Strategic dead end**: development energy at Apryse is concentrated on Apryse SDK (PDFTron) and iText, not TallComponents

### The Tall Components Problem

Modern HTML, the typical input for PDF generation today, only round-trips through TallComponents if it is XHTML-compliant:

```csharp
// TallPDF (NuGet: TallComponents.TallPDF5)
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Paragraphs;

Document document = new Document();
Section section = document.Sections.Add();

// XhtmlParagraph supports XHTML 1.0/1.1 + CSS 2.1 only.
XhtmlParagraph xhtml = new XhtmlParagraph();
xhtml.Text = "<html><body><h1>Hello World</h1></body></html>";
section.Paragraphs.Add(xhtml);

using (var fs = new FileStream("output.pdf", FileMode.Create))
{
    document.Write(fs);
}
```

IronPDF offers a modern, HTML-based approach:

```csharp
// IronPDF: Simple and modern
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1 style='font-size:24px'>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

The full transition from TallPDF and PDFKit to IronPDF—including XML-to-HTML equivalents—is documented in the [complete migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-tall-components-to-ironpdf/).

---

## Quick Start: Tall Components to IronPDF

### Step 1: Replace NuGet Packages

```bash
# Remove TallComponents packages (use whichever your project actually
# references — naming on nuget.org is TallComponents.PDFKit / PDFKit5 for
# the manipulation library, and TallComponents.TallPDF / TallPDF5 / TallPDF6
# for the layout-oriented sibling).
dotnet remove package TallComponents.PDFKit5
dotnet remove package TallComponents.TallPDF5
dotnet remove package TallComponents.PDFRasterizer4

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before — real namespaces (no "TallComponents.PDF.Kit"; that's not a thing)
using TallComponents.PDF;                        // Document, Page, Pages
using TallComponents.PDF.Shapes;                 // TextShape, ImageShape (PDFKit)
using TallComponents.PDF.Security;               // PasswordSecurity
using TallComponents.PDF.Layout;                 // Document, Section (TallPDF)
using TallComponents.PDF.Layout.Paragraphs;      // TextParagraph, XhtmlParagraph

// After
using IronPdf;
```

### Step 3: Initialize License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## API Mapping Reference

| TallComponents | IronPDF | Notes |
|-----------------|---------|-------|
| `Document` (TallPDF: layout) | `ChromePdfRenderer` | Create renderer |
| `Document` (PDFKit: manipulate) | `PdfDocument` | Load/edit existing PDFs |
| `Section` | Automatic | Sections from HTML structure |
| `TextParagraph` | HTML text elements | `<p>`, `<h1>`, `<div>`, etc. |
| `ImageParagraph` | `<img>` tag | HTML images |
| `TableParagraph` | HTML `<table>` | Standard HTML tables |
| `XhtmlParagraph` (XHTML 1.x + CSS 2.1) | `RenderHtmlAsPdf()` (HTML5/CSS3 via Chromium) | Modern HTML works |
| `Font` | CSS `font-family` | Web fonts supported |
| `document.Write(stream)` | `pdf.SaveAs()` / `pdf.BinaryData` / `pdf.Stream` | File or stream output |
| `page.Overlay.Add(shape)` | `pdf.ApplyStamp(stamper)` | Watermark / overlay |
| `outputDoc.Pages.Add(page.Clone())` | `PdfDocument.Merge(...)` / `pdf.AppendPdf(...)` | Merge PDFs |
| `Document.Security = new PasswordSecurity { ... }` | `pdf.SecuritySettings` | PDF security |
| `PageLayout` | `RenderingOptions` | Page settings |

---

## Code Examples

### Example 1: Basic Document Creation

**Tall Components:**
```csharp
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Paragraphs;

Document document = new Document();
Section section = document.Sections.Add();

TextParagraph paragraph = new TextParagraph();
paragraph.Text = "Hello World";
section.Paragraphs.Add(paragraph);

using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
{
    document.Write(fs);
}
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: Formatted Document with Images

**Tall Components:**
```csharp
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Paragraphs;
using TallComponents.PDF.Layout.Shapes;

Document document = new Document();
Section section = document.Sections.Add();

// Title
TextParagraph title = new TextParagraph();
title.Text = "Report Title";
title.Font = new Font("Arial", 24);
title.FontColor = new RgbColor(0, 0, 128);
section.Paragraphs.Add(title);

// Image
ImageParagraph imagePara = new ImageParagraph();
imagePara.Image = new FileImage("logo.png");
imagePara.Width = 200;
imagePara.Height = 100;
section.Paragraphs.Add(imagePara);

// Body text
TextParagraph body = new TextParagraph();
body.Text = "This is the report introduction paragraph with important information.";
body.Font = new Font("Arial", 12);
section.Paragraphs.Add(body);

using (FileStream fs = new FileStream("report.pdf", FileMode.Create))
{
    document.Write(fs);
}
```

**IronPDF:**
```csharp
using IronPdf;

var html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; }
        h1 { color: navy; font-size: 24px; }
        .logo { width: 200px; height: 100px; margin: 20px 0; }
        p { font-size: 12px; line-height: 1.6; }
    </style>
</head>
<body>
    <h1>Report Title</h1>
    <img src='logo.png' class='logo' alt='Logo'>
    <p>This is the report introduction paragraph with important information.</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.BaseUrl = new Uri("file:///C:/path/to/images/");
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 3: Tables

**Tall Components:**
```csharp
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Paragraphs;
using TallComponents.PDF.Layout.Tables;

Document document = new Document();
Section section = document.Sections.Add();

// Create table
TableParagraph table = new TableParagraph();
table.Columns.Add(new Column(150));
table.Columns.Add(new Column(100));
table.Columns.Add(new Column(100));
table.Columns.Add(new Column(100));

// Header row
Row headerRow = table.Rows.Add();
headerRow.BackgroundColor = new RgbColor(240, 240, 240);
headerRow[0].Add(new TextParagraph { Text = "Product", Font = new Font("Arial", 12, FontStyle.Bold) });
headerRow[1].Add(new TextParagraph { Text = "Qty", Font = new Font("Arial", 12, FontStyle.Bold) });
headerRow[2].Add(new TextParagraph { Text = "Price", Font = new Font("Arial", 12, FontStyle.Bold) });
headerRow[3].Add(new TextParagraph { Text = "Total", Font = new Font("Arial", 12, FontStyle.Bold) });

// Data rows
var products = GetProducts();
foreach (var product in products)
{
    Row row = table.Rows.Add();
    row[0].Add(new TextParagraph { Text = product.Name });
    row[1].Add(new TextParagraph { Text = product.Qty.ToString() });
    row[2].Add(new TextParagraph { Text = product.Price.ToString("C") });
    row[3].Add(new TextParagraph { Text = product.Total.ToString("C") });
}

section.Paragraphs.Add(table);
document.Write("table.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var products = GetProducts();

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{
            background: #f0f0f0;
            padding: 12px;
            text-align: left;
            border: 1px solid #ccc;
            font-weight: bold;
        }}
        td {{
            padding: 10px 12px;
            border: 1px solid #ddd;
        }}
        tr:nth-child(even) {{ background: #f9f9f9; }}
        .number {{ text-align: right; }}
    </style>
</head>
<body>
    <table>
        <thead>
            <tr>
                <th>Product</th>
                <th>Qty</th>
                <th>Price</th>
                <th>Total</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", products.Select(p => $@"
            <tr>
                <td>{p.Name}</td>
                <td class='number'>{p.Qty}</td>
                <td class='number'>{p.Price:C}</td>
                <td class='number'>{p.Total:C}</td>
            </tr>"))}
        </tbody>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("table.pdf");
```

### Example 4: Page Layout and Margins

**Tall Components:**
```csharp
using TallComponents.PDF.Layout;

Document document = new Document();
Section section = document.Sections.Add();

// Page settings
section.PageSize = new PageSize(PageFormat.A4);
section.PageOrientation = PageOrientation.Portrait;
section.Margin.Top = 72;     // 1 inch in points
section.Margin.Bottom = 72;
section.Margin.Left = 54;    // 0.75 inch
section.Margin.Right = 54;

// Add content...
section.Paragraphs.Add(new TextParagraph { Text = "Content here" });

document.Write("layout.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Page settings
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
renderer.RenderingOptions.MarginTop = 25;      // mm
renderer.RenderingOptions.MarginBottom = 25;
renderer.RenderingOptions.MarginLeft = 19;
renderer.RenderingOptions.MarginRight = 19;

var pdf = renderer.RenderHtmlAsPdf("<p>Content here</p>");
pdf.SaveAs("layout.pdf");
```

### Example 5: Headers and Footers

**Tall Components:**
```csharp
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Paragraphs;

Document document = new Document();
Section section = document.Sections.Add();

// Header
Header header = section.Header;
header.Paragraphs.Add(new TextParagraph
{
    Text = "Company Report - Confidential",
    Font = new Font("Arial", 10),
    FontColor = new RgbColor(128, 128, 128)
});

// Footer with page numbers
Footer footer = section.Footer;
footer.Paragraphs.Add(new TextParagraph
{
    Text = "Page #p# of #P#",  // Special placeholders
    Font = new Font("Arial", 10),
    FontColor = new RgbColor(128, 128, 128),
    HorizontalAlignment = HorizontalAlignment.Right
});

// Content
section.Paragraphs.Add(new TextParagraph { Text = "Report content..." });

document.Write("report.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// HTML Header
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; font-size: 10pt; color: gray; border-bottom: 1px solid #ccc; padding-bottom: 5px;'>
            Company Report - Confidential
        </div>",
    MaxHeight = 30
};

// HTML Footer with page numbers
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; font-size: 10pt; color: gray; text-align: right; border-top: 1px solid #ccc; padding-top: 5px;'>
            Page {page} of {total-pages}
        </div>",
    MaxHeight = 30
};

renderer.RenderingOptions.MarginTop = 40;
renderer.RenderingOptions.MarginBottom = 40;

var pdf = renderer.RenderHtmlAsPdf("<p>Report content...</p>");
pdf.SaveAs("report.pdf");
```

### Example 6: Merging PDFs

**Tall Components (PDFKit.NET 5.0):**
```csharp
using TallComponents.PDF;

// PDFKit has no dedicated Merger class — you append pages by cloning
// them into a target Document. Source: PDFKit.NET 5.0 "Merge PDF files
// in C# .NET" code sample on tallcomponents.com.
Document outputDoc = new Document();

using (FileStream fs1 = new FileStream("file1.pdf", FileMode.Open, FileAccess.Read))
using (FileStream fs2 = new FileStream("file2.pdf", FileMode.Open, FileAccess.Read))
{
    Document doc1 = new Document(fs1);
    foreach (Page page in doc1.Pages)
        outputDoc.Pages.Add(page.Clone()); // clone is required across documents

    Document doc2 = new Document(fs2);
    outputDoc.Pages.AddRange(doc2.Pages.CloneToArray());
}

using (FileStream fs = new FileStream("merged.pdf", FileMode.Create))
{
    outputDoc.Write(fs);
}
```

**IronPDF:**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("file1.pdf");
var pdf2 = PdfDocument.FromFile("file2.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");

// Or append to existing
pdf1.AppendPdf(pdf2);
pdf1.SaveAs("merged.pdf");
```

### Example 7: Security and Password Protection

**Tall Components (PDFKit.NET):**
```csharp
using TallComponents.PDF;
using TallComponents.PDF.Security;

// Load or create a Document, then assign a PasswordSecurity instance.
// Security is a property that takes a Security object (PasswordSecurity
// or CertificateSecurity) — it is not a flat bag of fields on Document.
using (FileStream fs = new FileStream("input.pdf", FileMode.Open, FileAccess.Read))
{
    Document document = new Document(fs);

    PasswordSecurity security = new PasswordSecurity();
    security.OwnerPassword = "owner456";
    security.UserPassword = "user123";
    security.AllowPrint = false;
    security.AllowCopy = false;
    // AES-256 is supported in PDFKit.NET 5.0 (KeyLength.Aes256).
    security.KeyLength = KeyLength.Aes256;
    document.Security = security;

    using (FileStream output = new FileStream("protected.pdf", FileMode.Create))
    {
        document.Write(output);
    }
}
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);

// Security settings
pdf.SecuritySettings.OwnerPassword = "owner456";
pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserAnnotations = false;

pdf.SaveAs("protected.pdf");
```

### Example 8: URL to PDF (limited in TallComponents)

**Tall Components:** `XhtmlParagraph.Path` accepts an HTTP URL, but the parser only handles XHTML 1.0/1.1 + CSS 2.1. Modern dashboards rendered as HTML5 + JavaScript will not render correctly; there is no headless-browser engine in the box.

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Full JavaScript support for dynamic pages
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.RenderDelay(3000);

var pdf = renderer.RenderUrlAsPdf("https://example.com/dashboard");
pdf.SaveAs("dashboard.pdf");
```

### Example 9: Digital Signatures

**Tall Components (PDFKit.NET):**
```csharp
using TallComponents.PDF;
using TallComponents.PDF.Signing;
using System.Security.Cryptography.X509Certificates;

using (FileStream fs = new FileStream("unsigned.pdf", FileMode.Open, FileAccess.Read))
{
    Document document = new Document(fs);

    X509Certificate2 cert = new X509Certificate2("certificate.pfx", "password");
    SignatureField field = new SignatureField("Signature1");
    field.Sign(cert);
    document.Fields.Add(field);

    using (FileStream output = new FileStream("signed.pdf", FileMode.Create))
    {
        document.Write(output);
    }
}
```

**IronPDF:**
```csharp
using IronPdf;
using IronPdf.Signing;

var pdf = PdfDocument.FromFile("unsigned.pdf");

// Sign with certificate
var signature = new PdfSignature("certificate.pfx", "password")
{
    SigningContact = "support@company.com",
    SigningLocation = "New York",
    SigningReason = "Document Approval"
};

pdf.Sign(signature);
pdf.SaveAs("signed.pdf");
```

---

## Feature Comparison

| Feature | TallComponents | IronPDF |
|---------|-----------------|---------|
| **Status** | Closed to new licenses (Apryse acquired 2025-05-27) | Active |
| **Support** | Existing customers only | Full |
| **Updates** | Maintenance only (PDFKit5 last refresh 2026-04) | Regular feature releases |
| **Content Creation** | | |
| HTML to PDF | XHTML 1.0/1.1 + CSS 2.1 only (`XhtmlParagraph`) | Full HTML5/CSS3 via Chromium |
| URL to PDF | Yes for XHTML URLs (`XhtmlParagraph.Path`) | Yes |
| CSS Support | CSS 2.1 | Full CSS3 |
| JavaScript | No | Full ES2024 |
| **PDF Operations** | | |
| Merge PDFs | Yes (`Pages.Add(page.Clone())`) | Yes |
| Split PDFs | Yes | Yes |
| Watermarks | `page.Overlay.Add(TextShape)` | `pdf.ApplyStamp(...)` |
| Headers/Footers | Layout DOM | HTML/CSS |
| **Security** | | |
| Password Protection | Yes (`PasswordSecurity`) | Yes |
| Digital Signatures | Yes (`SignatureField.Sign`) | Yes |
| Encryption | RC4 / AES-128 / AES-256 | AES-128 / AES-256 |
| PDF/A | PDF/A-1, PDF/A-2, PDF/A-3 | Yes |
| **Platform** | | |
| Target Frameworks | PDFKit5: .NET Standard 2.0; PDFKit (4.x): .NET Framework 2.0 | .NET Framework 4.6.2+, .NET 6/7/8/9 |
| **Development** | | |
| Documentation | Frozen at acquisition; new docs route to Apryse / iText | Maintained |
| Community | Quiet (no new sales) | Active |

---

## Why "the same engine in 2026" is the real risk

The TallComponents PDF engine is unchanged from its pre-acquisition state. Apryse is not investing road-map work into it — they are routing development into Apryse SDK and iText. Any rendering edge case that was a workaround in 2024 will still be a workaround in 2027. IronPDF, by contrast, ships a Chromium-based renderer that is updated alongside the IronPDF release cadence, so HTML/CSS/JS that works in a current browser works in IronPDF.

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit all Tall Components usage**
  ```bash
  grep -r "using TallComponents" --include="*.cs" .
  grep -rE "XhtmlParagraph|TextParagraph|PasswordSecurity|page\.Overlay" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document XML templates and layouts**
  ```csharp
  // Before (.)
  var xmlTemplate = new XmlDocument();
  xmlTemplate.LoadXml("<Document><Section>...</Section></Document>");
  ```
  **Why:** These XML structures will be replaced by HTML, so documenting them helps in conversion.

- [ ] **Identify security settings in use**
  ```csharp
  // Before (.)
  document.Security.OwnerPassword = "owner";
  document.Security.UserPassword = "user";
  ```
  **Why:** Security settings need to be mapped to IronPDF's SecuritySettings.

- [ ] **Note merge/split workflows**
  ```csharp
  // Before (.)
  PdfKit.Merger.Merge(doc1, doc2);
  ```
  **Why:** Understanding current workflows helps in implementing IronPDF's PdfDocument.Merge().

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Remove TallComponents packages**
  ```bash
  dotnet remove package TallComponents.PDFKit5     # or TallComponents.PDFKit (4.x)
  dotnet remove package TallComponents.TallPDF5    # if you used the layout sibling
  dotnet remove package TallComponents.PDFRasterizer4
  ```
  **Why:** Clean package switch to IronPDF. The real package IDs on nuget.org are `TallComponents.PDFKit5` / `PDFKit`, `TallComponents.TallPDF5` / `TallPDF6`, etc. — there is no package literally named `TallComponents.PDF`.

- [ ] **Install IronPdf package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Install IronPDF to access its features.

- [ ] **Convert XML layouts to HTML**
  ```csharp
  // Before (.)
  var xmlTemplate = new XmlDocument();
  xmlTemplate.LoadXml("<Document><Section>...</Section></Document>");

  // After (IronPDF)
  string htmlTemplate = "<html><body><div>...</div></body></html>";
  ```
  **Why:** IronPDF uses HTML for document layouts, providing more flexibility and modern styling.

- [ ] **Replace Section/Paragraph model with HTML**
  ```csharp
  // Before (.)
  var section = new Section();
  section.Paragraphs.Add(new TextParagraph("Hello World"));

  // After (IronPDF)
  string htmlContent = "<div><p>Hello World</p></div>";
  ```
  **Why:** HTML structure naturally replaces sections and paragraphs.

- [ ] **Update table code to HTML tables**
  ```csharp
  // Before (.)
  var table = new TableParagraph();
  table.AddRow(new TableRow(new TableCell("Cell1"), new TableCell("Cell2")));

  // After (IronPDF)
  string htmlTable = "<table><tr><td>Cell1</td><td>Cell2</td></tr></table>";
  ```
  **Why:** HTML tables are more versatile and supported by IronPDF.

- [ ] **Convert headers/footers to HTML**
  ```csharp
  // Before (.)
  document.Header = "Header Text";
  document.Footer = "Footer Text";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Header Text</div>",
      MaxHeight = 25
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Footer Text</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders for dynamic content.

- [ ] **Update security settings**
  ```csharp
  // Before (.)
  document.Security.OwnerPassword = "owner";
  document.Security.UserPassword = "user";

  // After (IronPDF)
  pdf.SecuritySettings.OwnerPassword = "owner";
  pdf.SecuritySettings.UserPassword = "user";
  ```
  **Why:** IronPDF's SecuritySettings provides similar functionality for securing PDFs.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Testing

- [ ] **Compare visual output**
  **Why:** Ensure the new PDFs match or exceed the quality of the old ones.

- [ ] **Test all document templates**
  **Why:** Confirm that all templates render correctly with IronPDF.

- [ ] **Verify PDF merging works**
  ```csharp
  // After (IronPDF)
  var mergedPdf = PdfDocument.Merge(pdf1, pdf2);
  ```
  **Why:** Ensure merging functionality is correctly implemented with IronPDF.

- [ ] **Test digital signatures**
  ```csharp
  // After (IronPDF)
  var signature = new PdfSignature("cert.pfx", "password");
  pdf.Sign(signature);
  ```
  **Why:** Verify that digital signatures are applied correctly.

- [ ] **Confirm security settings apply**
  **Why:** Ensure that all security settings are correctly enforced in the new PDFs.

### Documentation

- [ ] **Update developer documentation**
  **Why:** Provide updated guidance on using IronPDF.

- [ ] **Remove Tall Components references**
  **Why:** Clean up documentation to reflect the new library usage.

- [ ] **Document new IronPDF patterns**
  **Why:** Help developers understand and utilize IronPDF's features effectively.
---

## Migration Timeline Recommendation

Since TallComponents is closed to new licenses and on maintenance-only support, plan the move at your own pace but do not stall:

1. **Week 1**: Audit codebase and identify all usage
2. **Week 2**: Convert XHTML templates to HTML5
3. **Week 3**: Update security, signing, and merge code paths
4. **Week 4**: Testing and deployment

The longer you stay on TallComponents, the further your PDF stack drifts from current .NET targets and current HTML.

---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [PDF Security Guide](https://ironpdf.com/how-to/pdf-security/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
