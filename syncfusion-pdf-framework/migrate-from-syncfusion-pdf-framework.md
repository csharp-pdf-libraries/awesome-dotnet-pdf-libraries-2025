# How Do I Migrate from Syncfusion PDF Framework to IronPDF in C#?

Syncfusion PDF Framework is a comprehensive PDF library that's part of the Essential Studio suite. While powerful, its bundled licensing model and coordinate-based API can be limiting. This guide provides an exhaustive migration path to IronPDF, covering the complete Syncfusion PDF API surface area.

## Table of Contents

1. [Why Migrate from Syncfusion?](#why-migrate-from-syncfusion)
2. [Installation and Licensing](#installation-and-licensing)
3. [Complete API Mappings](#complete-api-mappings)
4. [Document Creation and Management](#document-creation-and-management)
5. [Text and Graphics Operations](#text-and-graphics-operations)
6. [Tables and Data Grids](#tables-and-data-grids)
7. [Working with Existing PDFs](#working-with-existing-pdfs)
8. [Page Operations](#page-operations)
9. [Headers, Footers, and Page Numbers](#headers-footers-and-page-numbers)
10. [Security and Encryption](#security-and-encryption)
11. [Forms and Annotations](#forms-and-annotations)
12. [Text Extraction](#text-extraction)
13. [Merging and Splitting](#merging-and-splitting)
14. [Watermarks and Stamps](#watermarks-and-stamps)
15. [Bookmarks and Navigation](#bookmarks-and-navigation)
16. [Attachments](#attachments)
17. [Digital Signatures](#digital-signatures)
18. [Compression and Optimization](#compression-and-optimization)
19. [Migration Checklist](#migration-checklist)

---

## Why Migrate from Syncfusion?

### The Bundle Licensing Problem

Syncfusion's licensing model creates significant challenges:

1. **Suite-Only Purchase**: Cannot buy PDF library standalone—must purchase entire Essential Studio
2. **Community License Restrictions**: Free tier requires BOTH <$1M revenue AND <5 developers
3. **Complex Deployment Licensing**: Different licenses for web, desktop, server deployments
4. **Annual Renewal Required**: Subscription model with yearly costs
5. **Per-Developer Pricing**: Costs scale linearly with team size
6. **Suite Bloat**: Includes 1000+ components you may not need

### IronPDF Advantages

| Aspect | Syncfusion PDF | IronPDF |
|--------|----------------|---------|
| Purchase Model | Suite bundle only | Standalone |
| Licensing | Complex tiers | Simple per-developer |
| Community Limit | <$1M AND <5 devs | Free trial, then license |
| Deployment | Multiple license types | One license covers all |
| API Style | Coordinate-based graphics | HTML/CSS-first |
| HTML Support | Requires BlinkBinaries | Native Chromium |
| CSS Support | Limited | Full CSS3/flexbox/grid |
| Dependencies | Multiple packages | Single NuGet |

---

## Installation and Licensing

### Remove Syncfusion Packages

```bash
# Remove all Syncfusion PDF packages
dotnet remove package Syncfusion.Pdf.Net.Core
dotnet remove package Syncfusion.HtmlToPdfConverter.Net.Windows
dotnet remove package Syncfusion.Pdf.Imaging.Net.Core
dotnet remove package Syncfusion.Pdf.OCR.Net.Core

# Remove license registration
# Delete: Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_KEY");
```

### Install IronPDF

```bash
dotnet add package IronPdf
```

### License Configuration

**Syncfusion:**
```csharp
// Must register before any Syncfusion calls
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR-SYNCFUSION-KEY");
```

**IronPDF:**
```csharp
// One-time at startup
IronPdf.License.LicenseKey = "YOUR-IRONPDF-KEY";
```

---

## Complete API Mappings

### Core Document Classes

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `PdfDocument` | `ChromePdfRenderer` / `PdfDocument` | Create or load PDFs |
| `PdfPage` | N/A (HTML generates pages) | Pages from HTML automatically |
| `PdfLoadedDocument` | `PdfDocument.FromFile()` | Load existing PDF |
| `PdfLoadedPage` | `pdf.Pages[index]` | Access page |
| `PdfDocumentBase` | `PdfDocument` | Base document |
| `PdfPageBase` | N/A | Use HTML/CSS |

### Graphics and Drawing

| Syncfusion PdfGraphics | IronPDF | Notes |
|-----------------------|---------|-------|
| `graphics.DrawString()` | HTML text elements | `<p>`, `<h1>`, `<span>` |
| `graphics.DrawLine()` | CSS border or `<hr>` | HTML/CSS |
| `graphics.DrawRectangle()` | `<div>` with CSS | CSS borders |
| `graphics.DrawEllipse()` | CSS border-radius | 50% for circle |
| `graphics.DrawImage()` | `<img>` tag | HTML images |
| `graphics.DrawPath()` | SVG `<path>` | SVG graphics |
| `graphics.DrawArc()` | SVG arc | SVG |
| `graphics.DrawBezier()` | SVG bezier | SVG |
| `graphics.DrawPolygon()` | SVG `<polygon>` | SVG |
| `graphics.DrawPie()` | SVG or CSS | SVG |
| `graphics.SetClip()` | CSS `clip-path` | CSS clipping |
| `graphics.RotateTransform()` | CSS `transform: rotate()` | CSS transforms |
| `graphics.TranslateTransform()` | CSS `transform: translate()` | CSS transforms |
| `graphics.ScaleTransform()` | CSS `transform: scale()` | CSS transforms |
| `graphics.Save()` / `Restore()` | N/A | CSS handles state |

### Fonts and Text

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `PdfStandardFont` | CSS `font-family` | System fonts |
| `PdfTrueTypeFont` | CSS `@font-face` | Custom fonts |
| `PdfCjkStandardFont` | CSS with CJK fonts | Asian fonts |
| `PdfFontFamily.Helvetica` | `font-family: Helvetica` | CSS |
| `PdfFontFamily.TimesRoman` | `font-family: 'Times New Roman'` | CSS |
| `PdfFontFamily.Courier` | `font-family: 'Courier New'` | CSS |
| `PdfFontStyle.Bold` | `font-weight: bold` | CSS |
| `PdfFontStyle.Italic` | `font-style: italic` | CSS |
| `PdfFontStyle.Underline` | `text-decoration: underline` | CSS |
| `PdfFontStyle.Strikeout` | `text-decoration: line-through` | CSS |
| `font.MeasureString()` | N/A (automatic) | CSS handles |
| `PdfTextElement` | HTML text elements | `<p>`, `<div>` |
| `PdfTextWebLink` | `<a href="">` | HTML links |

### Colors and Brushes

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `PdfBrushes.Black` | `color: black` | CSS color |
| `PdfBrushes.Red` | `color: red` | CSS color |
| `PdfSolidBrush` | CSS `color` / `background-color` | CSS |
| `PdfLinearGradientBrush` | CSS `linear-gradient()` | CSS gradient |
| `PdfRadialGradientBrush` | CSS `radial-gradient()` | CSS gradient |
| `PdfColor` | CSS color values | hex, rgb, rgba |
| `PdfPen` | CSS `border` | CSS borders |
| `PdfPens.Black` | `border-color: black` | CSS |

### Page Settings

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `document.PageSettings.Size` | `RenderingOptions.PaperSize` | Paper size |
| `PdfPageSize.A4` | `PdfPaperSize.A4` | A4 paper |
| `PdfPageSize.Letter` | `PdfPaperSize.Letter` | Letter paper |
| `document.PageSettings.Orientation` | `RenderingOptions.PaperOrientation` | Orientation |
| `PdfPageOrientation.Landscape` | `PdfPaperOrientation.Landscape` | Landscape |
| `document.PageSettings.Margins` | `RenderingOptions.MarginTop/Bottom/Left/Right` | Margins |
| `page.Size.Width` | `RenderingOptions.PaperSize` | Page width |
| `page.Size.Height` | `RenderingOptions.PaperSize` | Page height |

### Tables (PdfGrid)

| Syncfusion PdfGrid | IronPDF | Notes |
|-------------------|---------|-------|
| `new PdfGrid()` | HTML `<table>` | HTML tables |
| `grid.DataSource = data` | Build HTML from data | Templating |
| `grid.Columns.Add()` | `<th>` elements | Table headers |
| `grid.Rows.Add()` | `<tr>` elements | Table rows |
| `PdfGridCell` | `<td>` elements | Table cells |
| `grid.Headers` | `<thead>` | Table header |
| `PdfGridCellStyle` | CSS styles | Cell styling |
| `grid.Draw(page, point)` | Automatic in HTML | No manual position |
| `PdfGridLayoutResult` | N/A | Automatic layout |
| `PdfLightTable` | HTML `<table>` | Simpler table |

### HTML Conversion

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `HtmlToPdfConverter` | `ChromePdfRenderer` | Main converter |
| `converter.Convert(url)` | `renderer.RenderUrlAsPdf(url)` | URL to PDF |
| `converter.Convert(html, baseUrl)` | `renderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| `BlinkConverterSettings` | `ChromePdfRenderOptions` | Settings |
| `settings.ViewPortSize` | `RenderingOptions.ViewPortWidth` | Viewport |
| `settings.PdfPageSize` | `RenderingOptions.PaperSize` | Paper size |
| `settings.Margin` | `RenderingOptions.Margin*` | Margins |
| `settings.EnableJavaScript` | `RenderingOptions.EnableJavaScript` | JS execution |

### Security and Encryption

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `document.Security.UserPassword` | `pdf.SecuritySettings.UserPassword` | User password |
| `document.Security.OwnerPassword` | `pdf.SecuritySettings.OwnerPassword` | Owner password |
| `document.Security.Permissions` | `pdf.SecuritySettings.Allow*` | Permissions |
| `PdfPermissionsFlags.Print` | `AllowUserPrinting` | Print permission |
| `PdfPermissionsFlags.CopyContent` | `AllowUserCopyPasteContent` | Copy permission |
| `PdfPermissionsFlags.EditContent` | `AllowUserEdits` | Edit permission |
| `PdfPermissionsFlags.EditAnnotations` | `AllowUserAnnotations` | Annotation permission |
| `PdfPermissionsFlags.FillFields` | `AllowUserFormData` | Form permission |
| `PdfEncryptionAlgorithm.AES` | 256-bit encryption | Encryption |
| `document.Security.KeySize` | Automatic | Key size |

### Metadata

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `document.DocumentInformation.Title` | `pdf.MetaData.Title` | Title |
| `document.DocumentInformation.Author` | `pdf.MetaData.Author` | Author |
| `document.DocumentInformation.Subject` | `pdf.MetaData.Subject` | Subject |
| `document.DocumentInformation.Keywords` | `pdf.MetaData.Keywords` | Keywords |
| `document.DocumentInformation.Creator` | `pdf.MetaData.Creator` | Creator |
| `document.DocumentInformation.Producer` | `pdf.MetaData.Producer` | Producer |
| `document.DocumentInformation.CreationDate` | `pdf.MetaData.CreationDate` | Date |

### Bookmarks

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `document.Bookmarks.Add()` | `pdf.BookMarks.AddBookMarkAtStart()` | Add bookmark |
| `PdfBookmark` | `PdfBookmark` | Bookmark object |
| `bookmark.Title` | `bookmark.Text` | Bookmark text |
| `bookmark.Destination` | `bookmark.PageIndex` | Target page |
| `bookmark.TextStyle` | N/A | Styling |
| `bookmark.Color` | N/A | Color |

### Annotations

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `PdfTextMarkupAnnotation` | `pdf.ApplyStamp()` | Markup |
| `PdfPopupAnnotation` | N/A | Popup notes |
| `PdfLineAnnotation` | Use PDF editing | Line |
| `PdfRubberStampAnnotation` | `pdf.ApplyStamp()` | Stamps |
| `PdfFreeTextAnnotation` | `TextStamper` | Free text |
| `PdfInkAnnotation` | N/A | Ink |
| `page.Annotations.Add()` | `pdf.ApplyStamp()` | Add annotation |

### Forms

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `PdfForm` | `pdf.Form` | Form object |
| `PdfTextBoxField` | `pdf.Form.FindFormField()` | Text field |
| `PdfCheckBoxField` | `pdf.Form.FindFormField()` | Checkbox |
| `PdfRadioButtonListField` | `pdf.Form.FindFormField()` | Radio button |
| `PdfComboBoxField` | `pdf.Form.FindFormField()` | Dropdown |
| `PdfListBoxField` | `pdf.Form.FindFormField()` | List box |
| `PdfButtonField` | `pdf.Form.FindFormField()` | Button |
| `PdfSignatureField` | Digital signatures | Signature |
| `form.Fields[name].Text` | `field.Value` | Get/set value |
| `form.Flatten()` | `pdf.Form.Flatten()` | Flatten form |

### Text Extraction

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `PdfTextExtractor` | `pdf.ExtractAllText()` | Extract text |
| `extractor.ExtractText()` | `pdf.ExtractAllText()` | All text |
| `extractor.ExtractText(page)` | `pdf.Pages[i].Text` | Page text |
| `PdfTextExtractOptions` | N/A | Options |
| `TextLineCollection` | String result | Text lines |

### Image Operations

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `PdfBitmap` | HTML `<img>` | Images |
| `graphics.DrawImage()` | `<img>` tag | Draw image |
| `PdfImage.FromFile()` | `<img src="">` | Load image |
| `PdfImage.FromStream()` | Base64 in `<img>` | Stream image |
| `image.Width` / `Height` | CSS width/height | Dimensions |
| `PdfImageInfo` | N/A | Image info |
| `page.ExtractImages()` | `pdf.ExtractAllImages()` | Extract images |

### Attachments

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `document.Attachments.Add()` | `pdf.Attachments.Add()` | Add attachment |
| `PdfAttachment` | Attachment object | Attachment |
| `attachment.FileName` | `attachment.Name` | File name |
| `attachment.Data` | `attachment.GetBytes()` | File data |
| `attachment.Description` | N/A | Description |

### Digital Signatures

| Syncfusion | IronPDF | Notes |
|------------|---------|-------|
| `PdfSignature` | `pdf.SignWithFile()` | Sign PDF |
| `PdfCertificate` | Certificate file | PFX certificate |
| `signature.Certificate` | Certificate path | Certificate |
| `signature.Reason` | Signature options | Reason |
| `signature.Location` | Signature options | Location |
| `signature.ContactInfo` | Signature options | Contact |

---

## Document Creation and Management

### Creating a New Document

**Syncfusion:**
```csharp
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;

// Create document
PdfDocument document = new PdfDocument();

// Set page settings
document.PageSettings.Size = PdfPageSize.A4;
document.PageSettings.Orientation = PdfPageOrientation.Portrait;
document.PageSettings.Margins.All = 40;

// Add page
PdfPage page = document.Pages.Add();

// Draw content
PdfGraphics graphics = page.Graphics;
PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);
graphics.DrawString("Hello World", font, PdfBrushes.Black, new PointF(0, 0));

// Save
using (FileStream stream = new FileStream("output.pdf", FileMode.Create))
{
    document.Save(stream);
}
document.Close(true);
```

**IronPDF:**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
renderer.RenderingOptions.MarginTop = 40;
renderer.RenderingOptions.MarginBottom = 40;
renderer.RenderingOptions.MarginLeft = 40;
renderer.RenderingOptions.MarginRight = 40;

var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

---

## Text and Graphics Operations

### Drawing Text with Formatting

**Syncfusion:**
```csharp
PdfPage page = document.Pages.Add();
PdfGraphics graphics = page.Graphics;

// Different fonts
PdfFont titleFont = new PdfStandardFont(PdfFontFamily.Helvetica, 24, PdfFontStyle.Bold);
PdfFont bodyFont = new PdfStandardFont(PdfFontFamily.TimesRoman, 12);
PdfFont italicFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Italic);

// Draw text at positions
float y = 20;
graphics.DrawString("Document Title", titleFont, PdfBrushes.DarkBlue, new PointF(20, y));
y += 40;
graphics.DrawString("This is body text in Times Roman.", bodyFont, PdfBrushes.Black, new PointF(20, y));
y += 20;
graphics.DrawString("This is italic text.", italicFont, PdfBrushes.Gray, new PointF(20, y));

// Draw with text element for wrapping
PdfTextElement textElement = new PdfTextElement("This is a long paragraph that needs to wrap...", bodyFont);
textElement.Brush = PdfBrushes.Black;
PdfLayoutResult result = textElement.Draw(page, new RectangleF(20, y + 30, 400, 200));
```

**IronPDF:**
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(@"
<!DOCTYPE html>
<html>
<head>
<style>
    body { font-family: Arial, sans-serif; margin: 20px; }
    h1 { font-size: 24pt; font-weight: bold; color: darkblue; }
    .body-text { font-family: 'Times New Roman', serif; font-size: 12pt; }
    .italic { font-style: italic; color: gray; }
    .paragraph { width: 400px; }
</style>
</head>
<body>
    <h1>Document Title</h1>
    <p class='body-text'>This is body text in Times Roman.</p>
    <p class='body-text italic'>This is italic text.</p>
    <p class='body-text paragraph'>This is a long paragraph that needs to wrap automatically without any manual calculations needed.</p>
</body>
</html>");

pdf.SaveAs("output.pdf");
```

### Drawing Shapes

**Syncfusion:**
```csharp
PdfGraphics graphics = page.Graphics;

// Rectangle
graphics.DrawRectangle(PdfPens.Black, PdfBrushes.LightBlue, new RectangleF(50, 50, 200, 100));

// Circle (ellipse with equal dimensions)
graphics.DrawEllipse(PdfPens.Red, PdfBrushes.LightPink, new RectangleF(300, 50, 100, 100));

// Line
graphics.DrawLine(PdfPens.Green, new PointF(50, 200), new PointF(450, 200));

// Polygon
PointF[] points = { new PointF(100, 250), new PointF(150, 300), new PointF(50, 300) };
graphics.DrawPolygon(PdfPens.Blue, PdfBrushes.Yellow, points);
```

**IronPDF:**
```csharp
var pdf = renderer.RenderHtmlAsPdf(@"
<html>
<head>
<style>
    .rectangle {
        width: 200px; height: 100px;
        background: lightblue;
        border: 1px solid black;
        position: absolute; top: 50px; left: 50px;
    }
    .circle {
        width: 100px; height: 100px;
        background: lightpink;
        border: 1px solid red;
        border-radius: 50%;
        position: absolute; top: 50px; left: 300px;
    }
    .line {
        width: 400px;
        border-top: 1px solid green;
        position: absolute; top: 200px; left: 50px;
    }
</style>
</head>
<body>
    <div class='rectangle'></div>
    <div class='circle'></div>
    <div class='line'></div>
    <svg style='position:absolute; top:250px; left:50px;'>
        <polygon points='50,0 100,50 0,50' fill='yellow' stroke='blue'/>
    </svg>
</body>
</html>");
```

---

## Tables and Data Grids

### Creating Data Tables

**Syncfusion:**
```csharp
using Syncfusion.Pdf.Grid;

PdfPage page = document.Pages.Add();

// Create grid
PdfGrid grid = new PdfGrid();

// Add columns
grid.Columns.Add(4);
grid.Headers.Add(1);

// Header row
PdfGridRow header = grid.Headers[0];
header.Cells[0].Value = "Product";
header.Cells[1].Value = "Quantity";
header.Cells[2].Value = "Price";
header.Cells[3].Value = "Total";

// Style header
PdfGridCellStyle headerStyle = new PdfGridCellStyle();
headerStyle.BackgroundBrush = new PdfSolidBrush(new PdfColor(68, 114, 196));
headerStyle.TextBrush = PdfBrushes.White;
headerStyle.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold);
foreach (PdfGridCell cell in header.Cells)
{
    cell.Style = headerStyle;
}

// Data rows
PdfGridRow row1 = grid.Rows.Add();
row1.Cells[0].Value = "Widget A";
row1.Cells[1].Value = "10";
row1.Cells[2].Value = "$5.00";
row1.Cells[3].Value = "$50.00";

PdfGridRow row2 = grid.Rows.Add();
row2.Cells[0].Value = "Widget B";
row2.Cells[1].Value = "5";
row2.Cells[2].Value = "$10.00";
row2.Cells[3].Value = "$50.00";

// Cell styling
PdfGridCellStyle cellStyle = new PdfGridCellStyle();
cellStyle.CellPadding = new PdfPaddings(5, 5, 5, 5);
grid.Style.CellPadding = cellStyle.CellPadding;

// Draw grid
grid.Draw(page, new PointF(50, 50));
```

**IronPDF:**
```csharp
var pdf = renderer.RenderHtmlAsPdf(@"
<!DOCTYPE html>
<html>
<head>
<style>
    table {
        border-collapse: collapse;
        width: 100%;
        margin: 50px;
    }
    th {
        background-color: #4472C4;
        color: white;
        font-weight: bold;
        padding: 10px;
        text-align: left;
    }
    td {
        border: 1px solid #ddd;
        padding: 10px;
    }
    tr:nth-child(even) {
        background-color: #f9f9f9;
    }
</style>
</head>
<body>
    <table>
        <thead>
            <tr>
                <th>Product</th>
                <th>Quantity</th>
                <th>Price</th>
                <th>Total</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>Widget A</td>
                <td>10</td>
                <td>$5.00</td>
                <td>$50.00</td>
            </tr>
            <tr>
                <td>Widget B</td>
                <td>5</td>
                <td>$10.00</td>
                <td>$50.00</td>
            </tr>
        </tbody>
    </table>
</body>
</html>");
```

### Dynamic Table from Data

**Syncfusion:**
```csharp
// Create grid from DataTable
PdfGrid grid = new PdfGrid();
grid.DataSource = GetDataTable();  // Returns DataTable

// Style all at once
grid.Style.Font = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
grid.Style.CellPadding = new PdfPaddings(5, 5, 5, 5);

// Apply header style
PdfGridRow headerRow = grid.Headers[0];
headerRow.Style.BackgroundBrush = new PdfSolidBrush(new PdfColor(68, 114, 196));
headerRow.Style.TextBrush = PdfBrushes.White;

grid.Draw(page, new PointF(0, 0));
```

**IronPDF:**
```csharp
// Build HTML from data
var data = GetData();  // Your data source
var htmlBuilder = new StringBuilder();
htmlBuilder.Append(@"<table style='border-collapse:collapse; width:100%;'>
    <tr style='background:#4472C4; color:white;'>
        <th>Name</th><th>Email</th><th>Amount</th>
    </tr>");

foreach (var item in data)
{
    htmlBuilder.Append($@"<tr>
        <td style='border:1px solid #ddd; padding:8px;'>{item.Name}</td>
        <td style='border:1px solid #ddd; padding:8px;'>{item.Email}</td>
        <td style='border:1px solid #ddd; padding:8px;'>{item.Amount:C}</td>
    </tr>");
}
htmlBuilder.Append("</table>");

var pdf = renderer.RenderHtmlAsPdf(htmlBuilder.ToString());
```

---

## Working with Existing PDFs

### Loading and Modifying

**Syncfusion:**
```csharp
using Syncfusion.Pdf.Parsing;

// Load PDF
FileStream stream = new FileStream("existing.pdf", FileMode.Open);
PdfLoadedDocument loadedDoc = new PdfLoadedDocument(stream);

// Access first page
PdfLoadedPage page = loadedDoc.Pages[0] as PdfLoadedPage;

// Add content
PdfGraphics graphics = page.Graphics;
PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
graphics.DrawString("Added Text", font, PdfBrushes.Red, new PointF(100, 100));

// Save
FileStream output = new FileStream("modified.pdf", FileMode.Create);
loadedDoc.Save(output);
loadedDoc.Close(true);
stream.Close();
output.Close();
```

**IronPDF:**
```csharp
// Load PDF
var pdf = PdfDocument.FromFile("existing.pdf");

// Add text stamp
pdf.ApplyStamp(new TextStamper
{
    Text = "Added Text",
    FontSize = 12,
    FontFamily = "Helvetica",
    FontColor = IronSoftware.Drawing.Color.Red,
    HorizontalOffset = 100,
    VerticalOffset = 100
}, 0);  // Page index

pdf.SaveAs("modified.pdf");
```

---

## Headers, Footers, and Page Numbers

### Document Headers and Footers

**Syncfusion:**
```csharp
// Create header template
PdfPageTemplateElement header = new PdfPageTemplateElement(new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 50));
PdfFont headerFont = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
header.Graphics.DrawString("Company Report", headerFont, PdfBrushes.Gray, new PointF(0, 15));
header.Graphics.DrawLine(PdfPens.Gray, new PointF(0, 45), new PointF(header.Width, 45));

// Create footer template with page numbers
PdfPageTemplateElement footer = new PdfPageTemplateElement(new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 50));
PdfPageNumberField pageNumber = new PdfPageNumberField();
PdfPageCountField pageCount = new PdfPageCountField();
PdfCompositeField compositeField = new PdfCompositeField(headerFont, PdfBrushes.Gray, "Page {0} of {1}", pageNumber, pageCount);
compositeField.Draw(footer.Graphics, new PointF((footer.Width - 80) / 2, 15));

// Apply templates
document.Template.Top = header;
document.Template.Bottom = footer;
```

**IronPDF:**
```csharp
var renderer = new ChromePdfRenderer();

// Text headers/footers with placeholders
renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    CenterText = "Company Report",
    FontSize = 12,
    FontFamily = "Helvetica",
    DrawDividerLine = true
};

renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Page {page} of {total-pages}",
    FontSize = 10
};

// OR HTML headers/footers for more control
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"<div style='text-align:center; font-size:12pt; border-bottom:1px solid gray; padding-bottom:10px;'>Company Report</div>",
    MaxHeight = 50
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"<div style='text-align:center; font-size:10pt;'>Page {page} of {total-pages}</div>",
    MaxHeight = 30
};

var pdf = renderer.RenderHtmlAsPdf(htmlContent);
```

---

## Security and Encryption

### Password Protection

**Syncfusion:**
```csharp
PdfDocument document = new PdfDocument();
// Add content...

// Set security
document.Security.UserPassword = "user123";
document.Security.OwnerPassword = "owner456";
document.Security.Permissions = PdfPermissionsFlags.Print | PdfPermissionsFlags.CopyContent;
document.Security.KeySize = PdfEncryptionKeySize.Key256Bit;
document.Security.Algorithm = PdfEncryptionAlgorithm.AES;

document.Save("secured.pdf");
```

**IronPDF:**
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);

// Set security
pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.OwnerPassword = "owner456";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = true;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserAnnotations = false;
pdf.SecuritySettings.AllowUserFormData = true;

pdf.SaveAs("secured.pdf");
```

---

## Forms and Annotations

### Working with PDF Forms

**Syncfusion:**
```csharp
// Load PDF with form
PdfLoadedDocument loadedDoc = new PdfLoadedDocument("form.pdf");
PdfLoadedForm form = loadedDoc.Form;

// Fill fields
(form.Fields["name"] as PdfLoadedTextBoxField).Text = "John Doe";
(form.Fields["email"] as PdfLoadedTextBoxField).Text = "john@example.com";
(form.Fields["agree"] as PdfLoadedCheckBoxField).Checked = true;

// Flatten form
form.Flatten = true;

loadedDoc.Save("filled-form.pdf");
```

**IronPDF:**
```csharp
var pdf = PdfDocument.FromFile("form.pdf");

// Fill fields
var nameField = pdf.Form.FindFormField("name");
nameField.Value = "John Doe";

var emailField = pdf.Form.FindFormField("email");
emailField.Value = "john@example.com";

// Flatten form
pdf.Form.Flatten();

pdf.SaveAs("filled-form.pdf");
```

---

## Text Extraction

### Extracting Text Content

**Syncfusion:**
```csharp
PdfLoadedDocument loadedDoc = new PdfLoadedDocument("document.pdf");

// Extract all text
string allText = "";
foreach (PdfPageBase page in loadedDoc.Pages)
{
    PdfTextExtractor extractor = new PdfTextExtractor(page);
    allText += extractor.ExtractText();
}

// Extract from specific page
PdfTextExtractor pageExtractor = new PdfTextExtractor(loadedDoc.Pages[0]);
string pageText = pageExtractor.ExtractText();

loadedDoc.Close(true);
```

**IronPDF:**
```csharp
var pdf = PdfDocument.FromFile("document.pdf");

// Extract all text
string allText = pdf.ExtractAllText();

// Extract from specific page
string pageText = pdf.Pages[0].Text;
```

---

## Merging and Splitting

### Merging PDFs

**Syncfusion:**
```csharp
// Load documents
FileStream stream1 = new FileStream("doc1.pdf", FileMode.Open);
FileStream stream2 = new FileStream("doc2.pdf", FileMode.Open);
PdfLoadedDocument doc1 = new PdfLoadedDocument(stream1);
PdfLoadedDocument doc2 = new PdfLoadedDocument(stream2);

// Create merged document
PdfDocument merged = new PdfDocument();
merged.ImportPageRange(doc1, 0, doc1.Pages.Count - 1);
merged.ImportPageRange(doc2, 0, doc2.Pages.Count - 1);

// Save
FileStream output = new FileStream("merged.pdf", FileMode.Create);
merged.Save(output);

// Cleanup
merged.Close(true);
doc1.Close(true);
doc2.Close(true);
stream1.Close();
stream2.Close();
output.Close();
```

**IronPDF:**
```csharp
var pdf1 = PdfDocument.FromFile("doc1.pdf");
var pdf2 = PdfDocument.FromFile("doc2.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

### Splitting PDFs

**Syncfusion:**
```csharp
PdfLoadedDocument loadedDoc = new PdfLoadedDocument("large.pdf");

// Extract pages 1-5
PdfDocument split1 = new PdfDocument();
split1.ImportPageRange(loadedDoc, 0, 4);
split1.Save("pages1-5.pdf");

// Extract pages 6-10
PdfDocument split2 = new PdfDocument();
split2.ImportPageRange(loadedDoc, 5, 9);
split2.Save("pages6-10.pdf");

split1.Close(true);
split2.Close(true);
loadedDoc.Close(true);
```

**IronPDF:**
```csharp
var pdf = PdfDocument.FromFile("large.pdf");

// Extract pages 1-5
var split1 = pdf.CopyPages(0, 4);
split1.SaveAs("pages1-5.pdf");

// Extract pages 6-10
var split2 = pdf.CopyPages(5, 9);
split2.SaveAs("pages6-10.pdf");
```

---

## Watermarks and Stamps

### Adding Watermarks

**Syncfusion:**
```csharp
PdfLoadedDocument loadedDoc = new PdfLoadedDocument("document.pdf");

foreach (PdfPageBase page in loadedDoc.Pages)
{
    PdfGraphics graphics = page.Graphics;
    PdfGraphicsState state = graphics.Save();

    // Move to center and rotate
    graphics.TranslateTransform(page.Size.Width / 2, page.Size.Height / 2);
    graphics.RotateTransform(-45);

    // Draw watermark
    PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 72);
    PdfBrush brush = new PdfSolidBrush(new PdfColor(200, 200, 200, 100));
    SizeF size = font.MeasureString("CONFIDENTIAL");
    graphics.DrawString("CONFIDENTIAL", font, brush, new PointF(-size.Width / 2, -size.Height / 2));

    graphics.Restore(state);
}

loadedDoc.Save("watermarked.pdf");
loadedDoc.Close(true);
```

**IronPDF:**
```csharp
var pdf = PdfDocument.FromFile("document.pdf");

pdf.ApplyWatermark(
    "<div style='color:rgba(200,200,200,0.4); font-size:72pt; transform:rotate(-45deg);'>CONFIDENTIAL</div>",
    opacity: 100
);

pdf.SaveAs("watermarked.pdf");

// Or use text stamper
pdf.ApplyStamp(new TextStamper
{
    Text = "CONFIDENTIAL",
    FontSize = 72,
    Opacity = 25,
    Rotation = -45,
    FontColor = IronSoftware.Drawing.Color.Gray,
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Middle
});
```

---

## Digital Signatures

### Signing PDFs

**Syncfusion:**
```csharp
PdfLoadedDocument loadedDoc = new PdfLoadedDocument("document.pdf");
PdfCertificate certificate = new PdfCertificate("certificate.pfx", "password");

PdfSignature signature = new PdfSignature(loadedDoc, loadedDoc.Pages[0], certificate, "Signature");
signature.Reason = "I approve this document";
signature.Location = "New York";
signature.ContactInfo = "john@example.com";
signature.Bounds = new RectangleF(100, 100, 200, 100);

loadedDoc.Save("signed.pdf");
loadedDoc.Close(true);
```

**IronPDF:**
```csharp
var pdf = PdfDocument.FromFile("document.pdf");

pdf.SignWithFile(
    "certificate.pfx",
    "password",
    null,  // Optional signing permissions
    IronPdf.Signing.SignaturePermissions.NoChangesAllowed
);

pdf.SaveAs("signed.pdf");
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Syncfusion PDF usage**
  ```bash
  # Find all Syncfusion PDF usages
  grep -r "Syncfusion\.Pdf\|PdfDocument\|PdfGrid\|HtmlToPdfConverter" --include="*.cs" .
  ```
  **Why:** Understand the scope of migration and identify complex areas.

- [ ] **Document licensing costs**
  ```csharp
  // Syncfusion license tiers (as of 2024):
  // Community: Free (<$1M revenue + <5 developers)
  // Team: ~$995/developer
  // Enterprise: Contact sales
  // Plus annual renewal requirements
  ```
  **Why:** Syncfusion requires per-developer licenses with complex tier structures.

### Package Changes

- [ ] **Remove Syncfusion packages**
  ```bash
  dotnet remove package Syncfusion.Pdf.Net.Core
  dotnet remove package Syncfusion.HtmlToPdfConverter.Net.Windows
  dotnet remove package Syncfusion.Pdf.Imaging.Net.Core
  dotnet remove package Syncfusion.Compression.Net.Core
  dotnet remove package Syncfusion.Licensing
  ```
  **Why:** Clean removal avoids conflicts and licensing issues.

- [ ] **Install IronPDF**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Single package replaces multiple Syncfusion packages.

### Code Updates

- [ ] **Update license registration**
  ```csharp
  // Before (Syncfusion)
  Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR-SYNCFUSION-KEY");

  // After (IronPDF)
  IronPdf.License.LicenseKey = "YOUR-IRONPDF-KEY";
  ```
  **Why:** Different license activation pattern.

- [ ] **Replace HtmlToPdfConverter with ChromePdfRenderer**
  ```csharp
  // Before (Syncfusion)
  var converter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
  var settings = new WebKitConverterSettings();
  settings.WebKitPath = @"C:\QtBinaries\";
  converter.ConverterSettings = settings;
  var doc = converter.Convert(htmlString, baseUrl);
  doc.Save("output.pdf");
  doc.Close();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(htmlString);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF's Chromium engine provides superior CSS/JS support.

- [ ] **Replace PdfGrid with HTML tables**
  ```csharp
  // Before (Syncfusion)
  var grid = new PdfGrid();
  grid.Columns.Add(3);
  var header = grid.Headers.Add(1);
  header.Cells[0].Value = "Product";
  header.Cells[1].Value = "Qty";
  header.Cells[2].Value = "Price";
  foreach (var item in items)
  {
      var row = grid.Rows.Add();
      row.Cells[0].Value = item.Name;
      row.Cells[1].Value = item.Quantity.ToString();
      row.Cells[2].Value = item.Price.ToString("C");
  }
  grid.Draw(page, new PointF(0, 0));

  // After (IronPDF)
  var rows = items.Select(i => $"<tr><td>{i.Name}</td><td>{i.Quantity}</td><td>{i.Price:C}</td></tr>");
  var html = $@"
  <table style='border-collapse: collapse; width: 100%;'>
      <tr><th>Product</th><th>Qty</th><th>Price</th></tr>
      {string.Join("", rows)}
  </table>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** HTML tables are simpler and more flexible.

- [ ] **Replace PdfLoadedDocument with PdfDocument.FromFile**
  ```csharp
  // Before (Syncfusion)
  var doc = new PdfLoadedDocument("input.pdf");
  var page = doc.Pages[0];
  doc.Save("output.pdf");
  doc.Close();

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("input.pdf");
  var page = pdf.Pages[0];
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Simpler API for loading existing PDFs.

- [ ] **Update merge operations**
  ```csharp
  // Before (Syncfusion)
  var doc1 = new PdfLoadedDocument("doc1.pdf");
  var doc2 = new PdfLoadedDocument("doc2.pdf");
  var merger = new PdfDocumentBase.Merge(doc1, doc2);
  doc1.Save("merged.pdf");

  // After (IronPDF)
  var merged = PdfDocument.Merge(
      PdfDocument.FromFile("doc1.pdf"),
      PdfDocument.FromFile("doc2.pdf")
  );
  merged.SaveAs("merged.pdf");
  ```
  **Why:** IronPDF merge is simpler.

- [ ] **Update security settings**
  ```csharp
  // Before (Syncfusion)
  var security = doc.Security;
  security.OwnerPassword = "owner";
  security.UserPassword = "user";
  security.Permissions = PdfPermissionsFlags.Print;
  security.Algorithm = PdfEncryptionAlgorithm.AES;

  // After (IronPDF)
  pdf.SecuritySettings.OwnerPassword = "owner";
  pdf.SecuritySettings.UserPassword = "user";
  pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
  ```
  **Why:** Cleaner security API.

### Testing

- [ ] **Visual comparison**
  ```csharp
  var testFiles = new[] { "invoice.html", "report.html" };
  foreach (var file in testFiles)
  {
      var pdf = renderer.RenderHtmlFileAsPdf(file);
      pdf.SaveAs($"test_{Path.GetFileNameWithoutExtension(file)}.pdf");
  }
  // Compare visually - expect improved CSS rendering
  ```
  **Why:** Ensure layouts match or improve.

### Post-Migration

- [ ] **Remove Syncfusion references**
  ```bash
  grep -r "Syncfusion" --include="*.cs" --include="*.csproj" .
  # Remove any remaining references
  ```
  **Why:** Clean up removes licensing concerns.

- [ ] **Update build configurations**
  **Why:** Ensure build settings are optimized for IronPDF.

- [ ] **Update deployment documentation**
  **Why:** Reflect changes in PDF generation approach and licensing.

- [ ] **Calculate licensing cost savings**
  **Why:** Highlight financial benefits of migration.

- [ ] **Train team on HTML/CSS approach**
  **Why:** Ensure developers are comfortable with new PDF generation techniques using IronPDF.
---

## Quick Reference Card

```csharp
// ========== SYNCFUSION ==========
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("KEY");

PdfDocument doc = new PdfDocument();
PdfPage page = doc.Pages.Add();
page.Graphics.DrawString("Hello", font, brush, new PointF(10, 10));
doc.Save(stream);
doc.Close(true);

// ========== IRONPDF ==========
using IronPdf;
IronPdf.License.LicenseKey = "KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<p style='margin:10px;'>Hello</p>");
pdf.SaveAs("output.pdf");
```

---

## Summary

Migrating from Syncfusion PDF Framework to IronPDF offers:

1. **Standalone Licensing**: No suite bundle required
2. **Simplified API**: HTML/CSS instead of coordinates
3. **Modern CSS Support**: Flexbox, grid, CSS3
4. **Cleaner Code**: Less boilerplate, automatic resource management
5. **Better HTML Conversion**: Native Chromium engine
6. **Flexible Licensing**: Per-developer, not complex deployment tiers

The migration primarily involves converting coordinate-based graphics to HTML/CSS and updating loading/saving patterns. Most Syncfusion features have direct IronPDF equivalents.

For support, contact IronPDF or consult the [documentation](https://ironpdf.com/docs/).
