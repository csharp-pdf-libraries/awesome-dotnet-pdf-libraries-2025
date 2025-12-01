# How Do I Migrate from DynamicPDF to IronPDF in C#?

## Table of Contents

1. [Why Migrate from DynamicPDF to IronPDF](#why-migrate-from-dynamicpdf-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Examples](#code-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from DynamicPDF to IronPDF

### The Product Fragmentation Problem

DynamicPDF is sold as **separate products with separate licenses**:

1. **DynamicPDF Generator**: Create PDFs from scratch
2. **DynamicPDF Merger**: Merge, split, and manipulate existing PDFs
3. **DynamicPDF Core Suite**: Combined Generator and Merger
4. **DynamicPDF ReportWriter**: Report generation
5. **DynamicPDF HTML Converter**: HTML to PDF conversion (separate add-on)
6. **DynamicPDF Print Manager**: Print PDFs programmatically

**Each product requires a separate license.** A complete PDF solution can cost 3-5x what you'd expect.

### DynamicPDF vs IronPDF Comparison

| Aspect | DynamicPDF | IronPDF |
|--------|------------|---------|
| **Product Model** | Fragmented (5+ products) | All-in-one library |
| **Licensing** | Multiple licenses required | Single license |
| **HTML to PDF** | Separate add-on purchase | Built-in, Chromium-based |
| **CSS Support** | Limited (requires add-on) | Full CSS3 with Flexbox/Grid |
| **JavaScript** | Limited support | Full ES6+ support |
| **API Style** | Coordinate-based positioning | HTML/CSS + manipulation API |
| **Learning Curve** | Steep (multiple APIs) | Gentle (web technologies) |
| **Modern .NET** | .NET Standard 2.0 | .NET 6/7/8/9+ native |
| **Documentation** | Spread across products | Unified documentation |
| **Pricing Clarity** | Complex tiers | Transparent pricing |

### Key Migration Benefits

1. **Single Package**: One NuGet package replaces 3-5 DynamicPDF packages
2. **Modern Rendering**: Chromium engine vs legacy rendering
3. **Web Technologies**: Use HTML/CSS instead of coordinate-based positioning
4. **Simpler API**: Less code, more readable, easier maintenance
5. **No Add-On Purchases**: HTML, merging, security all included
6. **Better Performance**: Optimized for modern .NET runtimes

---

## Before You Start

### 1. Inventory Your DynamicPDF Usage

Identify which DynamicPDF products you're using:

```bash
# Find all DynamicPDF references
grep -r "ceTe.DynamicPDF\|DynamicPDF" --include="*.cs" --include="*.csproj" .

# Check NuGet packages
dotnet list package | grep -i dynamic
```

**Common packages to look for:**
- `ceTe.DynamicPDF.CoreSuite.NET`
- `ceTe.DynamicPDF.Generator.NET`
- `ceTe.DynamicPDF.Merger.NET`
- `ceTe.DynamicPDF.HtmlConverter.NET`

### 2. Document Current Functionality

Create a checklist of features you're using:

- [ ] PDF generation from scratch
- [ ] HTML to PDF conversion
- [ ] PDF merging/splitting
- [ ] Form filling
- [ ] Text extraction
- [ ] Digital signatures
- [ ] Encryption/passwords
- [ ] Watermarks
- [ ] Barcodes
- [ ] Tables (Table2)
- [ ] Headers/footers
- [ ] Page numbering

### 3. Set Up IronPDF

```bash
# Remove DynamicPDF packages
dotnet remove package ceTe.DynamicPDF.CoreSuite.NET
dotnet remove package ceTe.DynamicPDF.Generator.NET
dotnet remove package ceTe.DynamicPDF.Merger.NET
dotnet remove package ceTe.DynamicPDF.HtmlConverter.NET

# Install IronPDF
dotnet add package IronPdf
```

### 4. Configure License

```csharp
// At application startup (Program.cs or Startup.cs)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## Quick Start Migration

### The Paradigm Shift

DynamicPDF uses **coordinate-based positioning** where you place elements at specific X,Y coordinates on a page. IronPDF uses **HTML/CSS rendering** where you design with web technologies.

**DynamicPDF approach:**
```csharp
Document document = new Document();
Page page = new Page(PageSize.Letter);
Label label = new Label("Hello", 100, 200, 300, 50, Font.Helvetica, 12);
page.Elements.Add(label);
document.Pages.Add(page);
document.Draw("output.pdf");
```

**IronPDF approach:**
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1 style='margin-left:100px'>Hello</h1>");
pdf.SaveAs("output.pdf");
```

### Minimal Migration Example

**Before (DynamicPDF):**
```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements;

public byte[] GenerateInvoice(Invoice invoice)
{
    Document document = new Document();
    Page page = new Page(PageSize.A4);

    // Header
    Label title = new Label("INVOICE", 0, 0, 595, 30, Font.HelveticaBold, 24);
    title.Align = TextAlign.Center;
    page.Elements.Add(title);

    // Customer info
    Label customer = new Label($"Bill To: {invoice.CustomerName}", 40, 60, 300, 20);
    page.Elements.Add(customer);

    // Add page and generate
    document.Pages.Add(page);
    return document.Draw();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] GenerateInvoice(Invoice invoice)
{
    var html = $@"
        <html>
        <head>
            <style>
                body {{ font-family: Helvetica, sans-serif; padding: 40px; }}
                h1 {{ text-align: center; font-size: 24pt; }}
                .customer {{ margin-top: 30px; }}
            </style>
        </head>
        <body>
            <h1>INVOICE</h1>
            <div class='customer'>Bill To: {invoice.CustomerName}</div>
        </body>
        </html>";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

---

## Complete API Reference

### Namespace Mapping

| DynamicPDF Namespace | IronPDF Equivalent |
|---------------------|-------------------|
| `ceTe.DynamicPDF` | `IronPdf` |
| `ceTe.DynamicPDF.PageElements` | HTML elements |
| `ceTe.DynamicPDF.Merger` | `IronPdf` |
| `ceTe.DynamicPDF.Text` | HTML/CSS |
| `ceTe.DynamicPDF.Conversion` | `IronPdf` |
| `ceTe.DynamicPDF.PageElements.BarCoding` | HTML with barcode fonts or images |
| `ceTe.DynamicPDF.PageElements.Charting` | HTML chart libraries (Chart.js) |
| `ceTe.DynamicPDF.Cryptography` | `IronPdf` (SecuritySettings) |
| `ceTe.DynamicPDF.Forms` | `IronPdf` (Form property) |

### Core Class Mapping

| DynamicPDF Class | IronPDF Equivalent | Notes |
|-----------------|-------------------|-------|
| `Document` | `ChromePdfRenderer` | For generating new PDFs |
| `Document` | `PdfDocument` | For manipulating existing PDFs |
| `Page` | HTML `<div>` with page-break | Or multiple renders |
| `PageSize.Letter` | `PdfPaperSize.Letter` | Via RenderingOptions |
| `PageSize.A4` | `PdfPaperSize.A4` | Via RenderingOptions |
| `MergeDocument` | `PdfDocument.Merge()` | Static merge method |
| `PdfDocument` (Merger) | `PdfDocument.FromFile()` | Load existing PDFs |
| `HtmlConverter` | `ChromePdfRenderer` | Built-in, no add-on |
| `Template` | `HtmlHeaderFooter` | For headers/footers |

### Page Elements to HTML Mapping

| DynamicPDF PageElement | IronPDF/HTML Equivalent |
|-----------------------|------------------------|
| `Label` | `<p>`, `<span>`, `<div>` |
| `TextArea` | `<div>`, `<p>` with CSS |
| `Image` | `<img>` tag |
| `Rectangle` | `<div>` with CSS border |
| `Line` | `<hr>` or CSS border |
| `Circle` | `<div>` with CSS border-radius |
| `Table2` | HTML `<table>` |
| `PageNumberingLabel` | `{page}` / `{total-pages}` placeholders |
| `QrCode` | QR code image or library |
| `Barcode` | Barcode font or image |
| `Link` | `<a href="...">` |
| `Bookmark` | IronPDF bookmark API |
| `FormattedTextArea` | HTML with CSS formatting |

### Document Operations Mapping

| DynamicPDF Operation | IronPDF Equivalent |
|---------------------|-------------------|
| `document.Draw("file.pdf")` | `pdf.SaveAs("file.pdf")` |
| `document.Draw()` → byte[] | `pdf.BinaryData` |
| `document.Pages.Add(page)` | HTML content or `PdfDocument.InsertPdf()` |
| `mergeDoc.Append("file.pdf")` | `PdfDocument.Merge(pdf1, pdf2)` |
| `mergeDoc.InsertDocument()` | `pdf.InsertPdf(other, index)` |
| `pdfDoc.GetPage(index)` | `pdf.CopyPage(index)` |
| `pdfDoc.Pages[i].GetText()` | `pdf.ExtractTextFromPage(i)` |
| `document.Security = new Aes256Security()` | `pdf.SecuritySettings` |

### Rendering Options Mapping

| DynamicPDF Setting | IronPDF RenderingOptions |
|-------------------|-------------------------|
| `PageSize.Letter` | `PaperSize = PdfPaperSize.Letter` |
| `PageSize.A4` | `PaperSize = PdfPaperSize.A4` |
| `PageOrientation.Landscape` | `PaperOrientation = PdfPaperOrientation.Landscape` |
| `new Margins(top, left, bottom, right)` | `MarginTop`, `MarginLeft`, `MarginBottom`, `MarginRight` |
| Document title metadata | `Title` property on PdfDocument |
| Document author metadata | `MetaData.Author` |

### Security Mapping

| DynamicPDF Security | IronPDF Equivalent |
|--------------------|-------------------|
| `Aes128Security` | `pdf.SecuritySettings` with AES |
| `Aes256Security` | `pdf.SecuritySettings` with AES-256 |
| `RC4128Security` | `pdf.SecuritySettings` |
| `UserPassword` | `SecuritySettings.UserPassword` |
| `OwnerPassword` | `SecuritySettings.OwnerPassword` |
| `AllowPrint` | `SecuritySettings.AllowUserPrinting` |
| `AllowCopy` | `SecuritySettings.AllowUserCopyPasteContent` |
| `AllowEdit` | `SecuritySettings.AllowUserEdits` |
| `Certificate` (signing) | `pdf.SignWithFile()` |

### Form Field Mapping

| DynamicPDF Form | IronPDF Equivalent |
|----------------|-------------------|
| `form.Fields["name"].Value` | `pdf.Form.GetFieldByName("name").Value` |
| `TextField` | `pdf.Form.GetFieldByName()` |
| `CheckBox` | `pdf.Form.GetFieldByName()` |
| `RadioButton` | `pdf.Form.GetFieldByName()` |
| `FormFieldList` | `pdf.Form.Fields` |
| `form.Fields.Count` | `pdf.Form.Fields.Count()` |
| Flatten form | `pdf.Form.Flatten()` |

---

## Code Examples

### Example 1: Basic PDF Generation

**Before (DynamicPDF):**
```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements;

Document document = new Document();
Page page = new Page(PageSize.Letter);

Label title = new Label("Welcome to DynamicPDF", 0, 0, 612, 40, Font.HelveticaBold, 24);
title.Align = TextAlign.Center;
page.Elements.Add(title);

TextArea body = new TextArea("This is a sample document created with DynamicPDF.",
    40, 80, 532, 200, Font.Helvetica, 12);
page.Elements.Add(body);

document.Pages.Add(page);
document.Draw("welcome.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var html = @"
<html>
<head>
    <style>
        body { font-family: Helvetica, sans-serif; margin: 40px; }
        h1 { text-align: center; font-size: 24pt; }
        p { font-size: 12pt; line-height: 1.5; }
    </style>
</head>
<body>
    <h1>Welcome to IronPDF</h1>
    <p>This is a sample document created with IronPDF.</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("welcome.pdf");
```

### Example 2: HTML to PDF Conversion

**Before (DynamicPDF - requires separate HtmlConverter add-on):**
```csharp
using ceTe.DynamicPDF.Conversion;

// Convert from HTML string
string html = "<html><body><h1>Hello World</h1></body></html>";
HtmlConverter converter = new HtmlConverter(html);
converter.Convert("output.pdf");

// Convert from URL
HtmlConverter urlConverter = new HtmlConverter(new Uri("https://example.com"));
urlConverter.Convert("webpage.pdf");

// Convert from file
HtmlConverter fileConverter = new HtmlConverter("template.html", true);
fileConverter.Convert("fromfile.pdf");
```

**After (IronPDF - built-in, no add-on):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Convert from HTML string
var pdf1 = renderer.RenderHtmlAsPdf("<html><body><h1>Hello World</h1></body></html>");
pdf1.SaveAs("output.pdf");

// Convert from URL
var pdf2 = renderer.RenderUrlAsPdf("https://example.com");
pdf2.SaveAs("webpage.pdf");

// Convert from file
var pdf3 = renderer.RenderHtmlFileAsPdf("template.html");
pdf3.SaveAs("fromfile.pdf");
```

### Example 3: Merging PDFs

**Before (DynamicPDF - requires Merger product):**
```csharp
using ceTe.DynamicPDF.Merger;

MergeDocument mergeDoc = new MergeDocument("document1.pdf");
mergeDoc.Append("document2.pdf");
mergeDoc.Append("document3.pdf");

// Insert at specific position
PdfDocument insertDoc = new PdfDocument("insert.pdf");
mergeDoc.InsertDocument(insertDoc, 1);

mergeDoc.Draw("merged.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

// Simple merge
var pdf1 = PdfDocument.FromFile("document1.pdf");
var pdf2 = PdfDocument.FromFile("document2.pdf");
var pdf3 = PdfDocument.FromFile("document3.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);

// Insert at specific position
var insertDoc = PdfDocument.FromFile("insert.pdf");
merged.InsertPdf(insertDoc, 1);

merged.SaveAs("merged.pdf");
```

### Example 4: Tables

**Before (DynamicPDF Table2):**
```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements;

Document document = new Document();
Page page = new Page(PageSize.Letter);

Table2 table = new Table2(0, 0, 500, 300);
table.CellDefault.Padding.Value = 5;
table.CellDefault.Border.Value = new BorderInfo(LineStyle.Solid, 1);

// Add columns
Column2 col1 = table.Columns.Add(150);
Column2 col2 = table.Columns.Add(100);
Column2 col3 = table.Columns.Add(100);

// Add header row
Row2 headerRow = table.Rows.Add(30);
headerRow.Cells.Add("Product");
headerRow.Cells.Add("Quantity");
headerRow.Cells.Add("Price");

// Add data rows
Row2 dataRow = table.Rows.Add(25);
dataRow.Cells.Add("Widget");
dataRow.Cells.Add("10");
dataRow.Cells.Add("$99.99");

page.Elements.Add(table);
document.Pages.Add(page);
document.Draw("table.pdf");
```

**After (IronPDF with HTML):**
```csharp
using IronPdf;

var html = @"
<html>
<head>
    <style>
        table { border-collapse: collapse; width: 500px; }
        th, td { border: 1px solid black; padding: 5px; }
        th { background-color: #f0f0f0; }
    </style>
</head>
<body>
    <table>
        <tr>
            <th>Product</th>
            <th>Quantity</th>
            <th>Price</th>
        </tr>
        <tr>
            <td>Widget</td>
            <td>10</td>
            <td>$99.99</td>
        </tr>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("table.pdf");
```

### Example 5: Headers and Footers

**Before (DynamicPDF Template):**
```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements;

Document document = new Document();

// Create template for header/footer
Template template = new Template();
Label header = new Label("Company Report", 0, 0, 612, 20, Font.HelveticaBold, 12);
header.Align = TextAlign.Center;
template.Elements.Add(header);

PageNumberingLabel pageNum = new PageNumberingLabel("Page %%CP%% of %%TP%%",
    0, 750, 612, 20, Font.Helvetica, 10);
pageNum.Align = TextAlign.Center;
template.Elements.Add(pageNum);

// Apply template to document
document.Template = template;

// Add pages
Page page = new Page(PageSize.Letter);
page.Elements.Add(new Label("Content here...", 40, 50, 400, 100));
document.Pages.Add(page);

document.Draw("report.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Configure headers and footers
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:center; font-weight:bold;'>Company Report</div>",
    DrawDividerLine = true
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
    DrawDividerLine = true
};

var html = "<html><body><p>Content here...</p></body></html>";
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 6: Watermarks

**Before (DynamicPDF):**
```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements;
using ceTe.DynamicPDF.Merger;

// Text watermark using Template
PdfDocument pdfDoc = new PdfDocument("original.pdf");
Document document = new Document(pdfDoc);

Template watermarkTemplate = new Template();
Label watermark = new Label("CONFIDENTIAL", 0, 400, 612, 50, Font.HelveticaBold, 48);
watermark.Align = TextAlign.Center;
watermark.TextColor = new RgbColor(200, 200, 200);
watermarkTemplate.Elements.Add(watermark);

document.Template = watermarkTemplate;
document.Draw("watermarked.pdf");

// Image watermark
ImageWatermark imgWatermark = new ImageWatermark("logo.png");
imgWatermark.Opacity = 0.3f;
// Apply to pages...
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("original.pdf");

// Text watermark using HTML
pdf.ApplyWatermark(
    "<h1 style='color:rgba(200,200,200,0.5); font-size:48pt;'>CONFIDENTIAL</h1>",
    rotation: 45,
    opacity: 50
);

pdf.SaveAs("watermarked.pdf");

// Or use stamp for more control
var stamper = new HtmlStamper()
{
    Html = "<img src='logo.png' style='opacity:0.3; width:200px;' />",
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};
pdf.ApplyStamp(stamper);
```

### Example 7: Text Extraction

**Before (DynamicPDF):**
```csharp
using ceTe.DynamicPDF.Merger;

PdfDocument pdfDoc = new PdfDocument("document.pdf");

// Extract all text
string allText = pdfDoc.GetText();

// Extract from specific page
string pageText = pdfDoc.Pages[0].GetText();

// Extract from rectangle area
string areaText = pdfDoc.Pages[0].GetText(100, 100, 400, 200);

Console.WriteLine(allText);
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Extract all text
string allText = pdf.ExtractAllText();

// Extract from specific page (0-indexed)
string pageText = pdf.ExtractTextFromPage(0);

// Extract from all pages individually
for (int i = 0; i < pdf.PageCount; i++)
{
    string text = pdf.ExtractTextFromPage(i);
    Console.WriteLine($"Page {i + 1}: {text}");
}

Console.WriteLine(allText);
```

### Example 8: Security and Encryption

**Before (DynamicPDF):**
```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.Cryptography;

Document document = new Document();
Page page = new Page();
page.Elements.Add(new Label("Confidential Document", 0, 0, 400, 30));
document.Pages.Add(page);

// Add AES-256 encryption
Aes256Security security = new Aes256Security("userPass", "ownerPass");
security.AllowPrint = true;
security.AllowCopy = false;
security.AllowEdit = false;
security.AllowFormFilling = true;
document.Security = security;

document.Draw("secure.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential Document</h1>");

// Configure security
pdf.SecuritySettings.UserPassword = "userPass";
pdf.SecuritySettings.OwnerPassword = "ownerPass";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserFormData = true;

pdf.SaveAs("secure.pdf");
```

### Example 9: Digital Signatures

**Before (DynamicPDF):**
```csharp
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements.Forms;

Document document = new Document();
Page page = new Page();

// Add signature field
Signature signatureField = new Signature("sig1", 100, 500, 200, 50);
signatureField.Sign("certificate.pfx", "password");
page.Elements.Add(signatureField);

document.Pages.Add(page);
document.Draw("signed.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Signing;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Contract Document</h1><p>Sign below:</p>");

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

### Example 10: Form Filling

**Before (DynamicPDF):**
```csharp
using ceTe.DynamicPDF.Merger;

PdfDocument pdfDoc = new PdfDocument("form.pdf");

// Fill form fields
pdfDoc.Form.Fields["firstName"].Value = "John";
pdfDoc.Form.Fields["lastName"].Value = "Doe";
pdfDoc.Form.Fields["email"].Value = "john@example.com";

// Get checkbox/radio values
if (pdfDoc.Form.Fields["agree"] is CheckBoxField checkBox)
{
    checkBox.Value = "Yes";
}

// Flatten form (make non-editable)
pdfDoc.Form.Output = FormOutput.Flatten;

pdfDoc.Draw("filled-form.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("form.pdf");

// Fill form fields
pdf.Form.GetFieldByName("firstName").Value = "John";
pdf.Form.GetFieldByName("lastName").Value = "Doe";
pdf.Form.GetFieldByName("email").Value = "john@example.com";

// Handle checkbox
var agreeField = pdf.Form.GetFieldByName("agree");
if (agreeField != null)
{
    agreeField.Value = "Yes";
}

// Flatten form (make non-editable)
pdf.Form.Flatten();

pdf.SaveAs("filled-form.pdf");
```

---

## Advanced Scenarios

### Multi-Page Documents with Page Breaks

**DynamicPDF** requires manually creating and adding pages:

```csharp
Document document = new Document();
for (int i = 0; i < 5; i++)
{
    Page page = new Page();
    page.Elements.Add(new Label($"Page {i + 1}", 0, 0, 200, 30));
    document.Pages.Add(page);
}
document.Draw("multipage.pdf");
```

**IronPDF** uses CSS page breaks:

```csharp
var html = @"
<html>
<head>
    <style>
        .page { page-break-after: always; min-height: 100vh; }
        .page:last-child { page-break-after: avoid; }
    </style>
</head>
<body>
    <div class='page'><h1>Page 1</h1></div>
    <div class='page'><h1>Page 2</h1></div>
    <div class='page'><h1>Page 3</h1></div>
    <div class='page'><h1>Page 4</h1></div>
    <div class='page'><h1>Page 5</h1></div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("multipage.pdf");
```

### Batch Processing

**DynamicPDF:**
```csharp
foreach (var data in invoiceDataList)
{
    Document doc = new Document();
    // Create invoice...
    doc.Draw($"invoice_{data.Id}.pdf");
}
```

**IronPDF (with parallel processing):**
```csharp
var renderer = new ChromePdfRenderer();

Parallel.ForEach(invoiceDataList, data =>
{
    var html = GenerateInvoiceHtml(data);
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"invoice_{data.Id}.pdf");
});
```

### Converting DynamicPDF Templates to IronPDF

If you have DynamicPDF `Template` objects, convert them to HTML templates:

**DynamicPDF Template pattern:**
```csharp
Template template = new Template();
template.Elements.Add(new Label("Header", 0, 0, 612, 30));
template.Elements.Add(new Image("logo.png", 500, 0, 80, 30));
template.Elements.Add(new Line(0, 35, 612, 35, LineStyle.Solid, 1));
// Apply to all pages via document.Template
```

**IronPDF equivalent:**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = @"
        <div style='display:flex; justify-content:space-between; align-items:center; border-bottom:1px solid black; padding-bottom:5px;'>
            <span>Header</span>
            <img src='logo.png' style='height:30px;' />
        </div>"
};
```

### Reusing DynamicPDF Coordinate Logic

If you must preserve exact positioning, convert coordinates to CSS:

```csharp
// DynamicPDF: Label at (100, 200) with width 300, height 50
Label label = new Label("Text", 100, 200, 300, 50);

// IronPDF CSS equivalent (using absolute positioning)
var html = @"
<div style='position:absolute; left:100px; top:200px; width:300px; height:50px;'>
    Text
</div>";
```

---

## Performance Considerations

### Memory Management

**DynamicPDF:**
```csharp
using (Document doc = new Document())
{
    // Generate PDF
    doc.Draw("output.pdf");
} // Document disposed
```

**IronPDF:**
```csharp
using var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// All resources disposed
```

### Caching for Repeated Operations

IronPDF's Chromium engine benefits from warm-up:

```csharp
public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        _renderer = new ChromePdfRenderer();
        // Configure once
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    }

    public byte[] GeneratePdf(string html)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Async Operations

IronPDF supports async for better throughput:

```csharp
public async Task<byte[]> GeneratePdfAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

---

## Troubleshooting

### Issue 1: Coordinate Positioning Not Working

**Problem:** Elements don't appear where expected after migration.

**Solution:** Convert DynamicPDF coordinates to CSS positioning:

```csharp
// DynamicPDF coordinates (points from top-left)
// Label at X=100, Y=200

// IronPDF solution: Use CSS
var html = @"
<html>
<head>
    <style>
        body { position: relative; }
        .positioned {
            position: absolute;
            left: 100pt; /* or 100px */
            top: 200pt;
        }
    </style>
</head>
<body>
    <div class='positioned'>Your text here</div>
</body>
</html>";
```

### Issue 2: Table2 Overflow Not Working

**Problem:** DynamicPDF Table2 `GetOverflowRows()` has no direct equivalent.

**Solution:** Use CSS for automatic table overflow:

```csharp
var html = @"
<style>
    table { width: 100%; border-collapse: collapse; }
    /* Tables automatically flow across pages in IronPDF */
    tr { page-break-inside: avoid; }
</style>
<table>
    <!-- Large table content -->
</table>";
```

### Issue 3: Font Not Rendering Correctly

**Problem:** DynamicPDF `Font.Helvetica` doesn't have exact IronPDF equivalent.

**Solution:** Use CSS font-family with fallbacks:

```csharp
var html = @"
<style>
    body {
        font-family: Helvetica, Arial, sans-serif;
    }

    /* Or embed custom font */
    @font-face {
        font-family: 'MyFont';
        src: url('fonts/myfont.ttf');
    }
</style>";
```

### Issue 4: Page Numbering Format Differs

**Problem:** DynamicPDF uses `%%CP%%` / `%%TP%%`, IronPDF uses `{page}` / `{total-pages}`.

**Solution:** Update placeholder syntax:

```csharp
// DynamicPDF: "Page %%CP%% of %%TP%%"
// IronPDF:
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "<span>Page {page} of {total-pages}</span>"
};
```

### Issue 5: Multiple Products Now in Single Package

**Problem:** Code references multiple DynamicPDF namespaces.

**Solution:** Replace all with single `using IronPdf`:

```csharp
// Before (multiple DynamicPDF products)
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.PageElements;
using ceTe.DynamicPDF.Merger;
using ceTe.DynamicPDF.Conversion;
using ceTe.DynamicPDF.Cryptography;

// After (single package)
using IronPdf;
using IronPdf.Rendering; // Only if needed for specific options
```

### Issue 6: Security Enum Values Different

**Problem:** DynamicPDF security permissions don't map directly.

**Solution:** Use IronPDF's granular security settings:

```csharp
// DynamicPDF: security.AllowPrint = true
// IronPDF:
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
// or PdfPrintSecurity.NoPrint, PdfPrintSecurity.PrintLowQuality

// DynamicPDF: security.AllowEdit = false
// IronPDF:
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
// or PdfEditSecurity.EditAll, PdfEditSecurity.EditAllExceptExtract
```

### Issue 7: Form Field Names Case Sensitive

**Problem:** Form field lookup fails after migration.

**Solution:** Check exact field names:

```csharp
// List all field names
foreach (var field in pdf.Form.Fields)
{
    Console.WriteLine($"Field: {field.Name}, Type: {field.GetType().Name}");
}

// Use exact name (case-sensitive)
var field = pdf.Form.GetFieldByName("FirstName"); // not "firstname"
```

### Issue 8: HtmlConverter Async Behavior

**Problem:** DynamicPDF HtmlConverter was synchronous, code expects blocking behavior.

**Solution:** IronPDF is synchronous by default, async available:

```csharp
// Synchronous (default)
var pdf = renderer.RenderHtmlAsPdf(html);

// Async if needed
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all DynamicPDF packages in use**
  ```bash
  grep -r "using ceTe.DynamicPDF" --include="*.cs" .
  grep -r "Document\|Page\|Label\|TextArea\|Table2\|MergeDocument\|HtmlConverter" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **List all DynamicPDF features being used**
  ```csharp
  // Example features to document:
  var document = new Document();
  var page = new Page();
  var label = new Label();
  ```
  **Why:** Ensure all features are accounted for and mapped to IronPDF equivalents.

- [ ] **Document coordinate-based layouts that need conversion**
  ```csharp
  // Example coordinate-based layout:
  var label = new Label("Text", 0, 0, 100, 20);
  ```
  **Why:** Coordinate-based layouts will be converted to HTML/CSS for IronPDF.

- [ ] **Backup existing codebase**
  **Why:** Ensure you have a restore point in case of migration issues.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Set up test environment**
  **Why:** A separate environment ensures safe testing without affecting production.

### Package Migration

- [ ] **Remove `ceTe.DynamicPDF.CoreSuite.NET`**
  ```bash
  dotnet remove package ceTe.DynamicPDF.CoreSuite.NET
  ```
  **Why:** Remove unnecessary packages to avoid conflicts and reduce bloat.

- [ ] **Remove `ceTe.DynamicPDF.Generator.NET`**
  ```bash
  dotnet remove package ceTe.DynamicPDF.Generator.NET
  ```
  **Why:** Transition to IronPDF's all-in-one package.

- [ ] **Remove `ceTe.DynamicPDF.Merger.NET`**
  ```bash
  dotnet remove package ceTe.DynamicPDF.Merger.NET
  ```
  **Why:** IronPDF handles merging natively without separate packages.

- [ ] **Remove `ceTe.DynamicPDF.HtmlConverter.NET`**
  ```bash
  dotnet remove package ceTe.DynamicPDF.HtmlConverter.NET
  ```
  **Why:** IronPDF includes HTML conversion capabilities.

- [ ] **Remove any other DynamicPDF packages**
  ```bash
  dotnet remove package ceTe.DynamicPDF.*
  ```
  **Why:** Ensure all DynamicPDF dependencies are removed.

- [ ] **Install `IronPdf` package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add the IronPDF package to start using its features.

- [ ] **Configure license key at startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Code Migration

- [ ] **Replace `using ceTe.DynamicPDF.*` with `using IronPdf`**
  ```csharp
  // Before (DynamicPDF)
  using ceTe.DynamicPDF.Generator;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Update namespaces to use IronPDF's API.

- [ ] **Convert `Document` + `Page` to HTML templates**
  ```csharp
  // Before (DynamicPDF)
  var document = new Document();
  var page = new Page(PageSize.Letter, PageOrientation.Portrait);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf("<html><body>Your content here</body></html>");
  ```
  **Why:** IronPDF uses HTML/CSS for layout instead of coordinate-based positioning.

- [ ] **Convert `Label`, `TextArea` to HTML elements**
  ```csharp
  // Before (DynamicPDF)
  var label = new Label("Hello World", 0, 0, 100, 20);

  // After (IronPDF)
  var htmlContent = "<div style='position:absolute; top:0; left:0; width:100px; height:20px;'>Hello World</div>";
  ```
  **Why:** HTML elements provide more flexibility and styling options.

- [ ] **Convert `Table2` to HTML tables**
  ```csharp
  // Before (DynamicPDF)
  var table = new Table2(0, 0, 500, 200);

  // After (IronPDF)
  var htmlTable = "<table style='width:500px;'><tr><td>Cell</td></tr></table>";
  ```
  **Why:** HTML tables are more intuitive and support CSS styling.

- [ ] **Convert `MergeDocument` to `PdfDocument.Merge()`**
  ```csharp
  // Before (DynamicPDF)
  var mergeDoc = new MergeDocument("file1.pdf");
  mergeDoc.Append("file2.pdf");

  // After (IronPDF)
  var pdf1 = PdfDocument.FromFile("file1.pdf");
  var pdf2 = PdfDocument.FromFile("file2.pdf");
  var merged = PdfDocument.Merge(pdf1, pdf2);
  ```
  **Why:** IronPDF provides a straightforward API for merging documents.

- [ ] **Convert `HtmlConverter` to `ChromePdfRenderer`**
  ```csharp
  // Before (DynamicPDF)
  var htmlConverter = new HtmlConverter();
  htmlConverter.Convert("https://example.com");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderUrlAsPdf("https://example.com");
  ```
  **Why:** IronPDF's Chromium-based rendering supports modern web standards.

- [ ] **Update security settings**
  ```csharp
  // Before (DynamicPDF)
  document.Security = new DocumentSecurity("ownerPassword", "userPassword");

  // After (IronPDF)
  pdf.SecuritySettings.OwnerPassword = "ownerPassword";
  pdf.SecuritySettings.UserPassword = "userPassword";
  ```
  **Why:** IronPDF provides comprehensive security settings.

- [ ] **Update form field access**
  ```csharp
  // Before (DynamicPDF)
  var formField = document.Form.Fields["fieldName"];

  // After (IronPDF)
  var formField = pdf.Form.Fields["fieldName"];
  ```
  **Why:** Ensure form fields are accessed correctly with IronPDF.

- [ ] **Convert page numbering placeholders**
  ```csharp
  // Before (DynamicPDF)
  page.Elements.Add(new PageNumberingLabel("Page %%CP%% of %%TP%%", 0, 0, 100, 20));

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders.

- [ ] **Convert headers/footers to `HtmlHeaderFooter`**
  ```csharp
  // Before (DynamicPDF)
  page.Elements.Add(new Label("Header", 0, 0, 100, 20));

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Header</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF's HTML headers/footers offer more customization and styling.

### Testing

- [ ] **Visual comparison of generated PDFs**
  **Why:** Ensure the new PDFs match the old ones in appearance and layout.

- [ ] **Verify text positioning and layout**
  **Why:** Confirm that text is correctly positioned using HTML/CSS.

- [ ] **Test table rendering and overflow**
  **Why:** Ensure tables render correctly and handle overflow gracefully.

- [ ] **Verify headers/footers on all pages**
  **Why:** Check that headers and footers appear as expected on every page.

- [ ] **Test form filling functionality**
  **Why:** Ensure form fields are accessible and functional in the new PDFs.

- [ ] **Verify security/encryption**
  **Why:** Confirm that security settings are applied correctly.

- [ ] **Test digital signatures**
  **Why:** Ensure digital signatures are correctly applied and validated.

- [ ] **Performance benchmarking**
  **Why:** Compare performance to ensure the migration does not degrade application speed.

- [ ] **Memory usage verification**
  **Why:** Check for any memory leaks or excessive usage post-migration.

### Post-Migration

- [ ] **Remove unused DynamicPDF license files**
  **Why:** Clean up unnecessary files to avoid confusion.

- [ ] **Update documentation**
  **Why:** Ensure all documentation reflects the new IronPDF usage.

- [ ] **Train team on IronPDF API**
  **Why:** Ensure the team is familiar with the new library and its features.

- [ ] **Monitor production for issues**
  **Why:** Watch for any unexpected behavior or issues in the live environment.

- [ ] **Archive DynamicPDF-related code for reference**
  **Why:** Keep a record of old code for future reference or rollback if needed.
---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **IronPDF Support**: https://ironpdf.com/support/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
