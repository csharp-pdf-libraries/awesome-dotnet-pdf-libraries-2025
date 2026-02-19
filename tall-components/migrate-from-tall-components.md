# How Do I Migrate from Tall Components (TallPDF, PDFKit) to IronPDF in C#?

## ⚠️ CRITICAL: Tall Components is Discontinued

**Tall Components has been acquired by Apryse and is no longer available.** New licenses cannot be purchased, and existing users are being redirected to iText SDK. If you're still using Tall Components, migration is not optional - it's mandatory.

### Why You Must Migrate Now

1. **Product Discontinued**: No new licenses available since Apryse acquisition
2. **No Support**: No bug fixes, security patches, or updates
3. **Known Rendering Bugs**: Documented issues with blank pages, missing graphics, and font problems
4. **No HTML Support**: Only XML-based document creation - completely unsuitable for modern web workflows
5. **Legacy Architecture**: Built for a different era of .NET development
6. **Vendor Lock-in Risk**: Being pushed toward expensive iText SDK

### The Tall Components Problem

Before discontinuation, Tall Components was already problematic:

```csharp
// Tall Components: Verbose XML-based approach
Document document = new Document();
Section section = document.Sections.Add();
TextParagraph paragraph = new TextParagraph();
paragraph.Text = "Hello World";
paragraph.Font = new Font("Arial", 24);
section.Paragraphs.Add(paragraph);
document.Write("output.pdf");
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
# Remove Tall Components packages
dotnet remove package TallComponents.PDF.Kit
dotnet remove package TallComponents.PDF.Layout
dotnet remove package TallComponents.PDF.Layout.Drawing

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using TallComponents.PDF.Kit;
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Drawing;
using TallComponents.PDF.Layout.Paragraphs;

// After
using IronPdf;
```

### Step 3: Initialize License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## API Mapping Reference

| Tall Components | IronPDF | Notes |
|-----------------|---------|-------|
| `Document` | `ChromePdfRenderer` | Create renderer |
| `Section` | Automatic | Sections from HTML structure |
| `TextParagraph` | HTML text elements | `<p>`, `<h1>`, `<div>`, etc. |
| `ImageParagraph` | `<img>` tag | HTML images |
| `TableParagraph` | HTML `<table>` | Standard HTML tables |
| `Font` | CSS `font-family` | Web fonts supported |
| `document.Write()` | `pdf.SaveAs()` | Save to file |
| `document.Write(stream)` | `pdf.BinaryData` or `pdf.Stream` | Stream output |
| `Page.Canvas` | HTML/CSS rendering | No manual canvas needed |
| `XmlDocument.Generate()` | `RenderHtmlAsPdf()` | HTML replaces XML |
| `PdfKit.Merger.Merge()` | `PdfDocument.Merge()` | Merge PDFs |
| `Document.Security` | `pdf.SecuritySettings` | PDF security |
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

**Tall Components:**
```csharp
using TallComponents.PDF.Kit;

Document doc1 = new Document("file1.pdf");
Document doc2 = new Document("file2.pdf");

PdfKit.Merger merger = new PdfKit.Merger();
merger.Append(doc1);
merger.Append(doc2);

Document merged = merger.Merge();
using (FileStream fs = new FileStream("merged.pdf", FileMode.Create))
{
    merged.Write(fs);
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

**Tall Components:**
```csharp
using TallComponents.PDF.Kit;
using TallComponents.PDF.Kit.Security;

Document document = new Document();
// ... create content ...

// Security settings
document.Security.OwnerPassword = "owner456";
document.Security.UserPassword = "user123";
document.Security.AllowPrinting = false;
document.Security.AllowCopying = false;
document.Security.EncryptionLevel = EncryptionLevel.AES256;

document.Write("protected.pdf");
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

### Example 8: URL to PDF (Not Possible with Tall Components)

**Tall Components:** Not supported - no HTML rendering capability

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

**Tall Components:**
```csharp
using TallComponents.PDF.Kit;
using TallComponents.PDF.Kit.Signing;

Document document = new Document("unsigned.pdf");

// Load certificate
X509Certificate2 cert = new X509Certificate2("certificate.pfx", "password");

// Create signature
SignatureHandler handler = new SignatureHandler(cert);
document.Sign(handler);

document.Write("signed.pdf");
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

| Feature | Tall Components | IronPDF |
|---------|-----------------|---------|
| **Status** | ❌ DISCONTINUED | ✅ Active |
| **Support** | ❌ None | ✅ Full |
| **Updates** | ❌ None | ✅ Regular |
| **Content Creation** | | |
| HTML to PDF | No | Full Chromium |
| URL to PDF | No | Yes |
| CSS Support | No | Full CSS3 |
| JavaScript | No | Full ES2024 |
| XML Templates | Yes | Not needed |
| **PDF Operations** | | |
| Merge PDFs | Yes | Yes |
| Split PDFs | Yes | Yes |
| Watermarks | Manual | Built-in |
| Headers/Footers | XML-based | HTML/CSS |
| **Security** | | |
| Password Protection | Yes | Yes |
| Digital Signatures | Yes | Yes |
| Encryption | Yes | Yes |
| PDF/A | Limited | Yes |
| **Known Issues** | | |
| Blank Pages | ⚠️ Documented bug | None |
| Missing Graphics | ⚠️ Documented bug | None |
| Font Problems | ⚠️ Documented bug | None |
| **Development** | | |
| Learning Curve | High (XML) | Low (HTML) |
| Documentation | Outdated | Extensive |
| Community | None | Active |

---

## Known Tall Components Bugs (Documented)

These issues were never fixed before discontinuation:

1. **Blank Pages Bug**: Random blank pages appearing in generated PDFs
2. **Disappearing Graphics**: Images and shapes not rendering in certain conditions
3. **Missing Text**: Text paragraphs randomly omitted from output
4. **Incorrect Font Rendering**: Wrong fonts or garbled characters
5. **Memory Leaks**: Document objects not properly disposed

IronPDF has none of these issues - it uses a proven Chromium rendering engine.

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit all Tall Components usage**
  ```bash
  grep -r "using TallComponents" --include="*.cs" .
  grep -r "Document\|Section\|TextParagraph" --include="*.cs" .
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
  dotnet remove package TallComponents.PDF
  ```
  **Why:** Clean package switch to IronPDF.

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

- [ ] **Verify no more blank page issues**
  **Why:** IronPDF's rendering should eliminate blank pages, but verification is necessary.

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

Since Tall Components is discontinued with no support:

1. **Week 1**: Audit codebase and identify all usage
2. **Week 2**: Convert document templates to HTML
3. **Week 3**: Update security and merging code
4. **Week 4**: Testing and deployment

**Do not delay** - you're running unsupported software with known rendering bugs.

---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [PDF Security Guide](https://ironpdf.com/how-to/pdf-security/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
