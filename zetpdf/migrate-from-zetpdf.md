# How Do I Migrate from ZetPDF to IronPDF in C#?

## Why Migrate from ZetPDF?

ZetPDF is a fork of PDFSharp with similar limitations. Key reasons to migrate:

1. **No HTML Support**: Cannot convert HTML/URLs to PDF - only low-level graphics drawing
2. **Coordinate-Based API**: Complex manual positioning of every element
3. **No CSS Support**: No styling system - manual font and color management
4. **No JavaScript**: Cannot render dynamic web content
5. **Limited Features**: No watermarks, headers/footers, or merge operations
6. **Manual Page Breaks**: Must calculate and manage page overflow manually
7. **Text Measurement Required**: Manual calculation for text wrapping

### The Fundamental Problem

ZetPDF/PDFSharp forces you to position every element with exact coordinates:

```csharp
// ZetPDF: Manual positioning nightmare
graphics.DrawString("Name:", font, brush, new XPoint(50, 100));
graphics.DrawString("John Doe", font, brush, new XPoint(100, 100));
graphics.DrawString("Address:", font, brush, new XPoint(50, 120));
graphics.DrawString("123 Main St", font, brush, new XPoint(100, 120));
// ... hundreds of lines for a simple form
```

IronPDF uses HTML/CSS - the layout engine handles everything:

```csharp
// IronPDF: Simple HTML
var html = @"
<p><strong>Name:</strong> John Doe</p>
<p><strong>Address:</strong> 123 Main St</p>";
var pdf = renderer.RenderHtmlAsPdf(html);
```

---

## Quick Start: ZetPDF to IronPDF

### Step 1: Replace NuGet Package

```bash
# Remove ZetPDF
dotnet remove package ZetPDF

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using ZetPdf;
using ZetPdf.Drawing;
using ZetPdf.Fonts;

// After
using IronPdf;
```

### Step 3: Migrate to HTML-Based Rendering

```csharp
// OLD: Complex coordinate-based drawing
var document = new PdfDocument();
var page = document.AddPage();
var graphics = XGraphics.FromPdfPage(page);
graphics.DrawString("Hello", font, brush, 50, 50);

// NEW: Simple HTML
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("output.pdf");
```

---

## API Mapping Reference

| ZetPDF | IronPDF | Notes |
|--------|---------|-------|
| `new PdfDocument()` | `new ChromePdfRenderer()` | Create renderer |
| `document.AddPage()` | Automatic | Pages created from HTML |
| `XGraphics.FromPdfPage(page)` | N/A | Use HTML/CSS instead |
| `graphics.DrawString()` | HTML text elements | `<p>`, `<h1>`, etc. |
| `graphics.DrawImage()` | `<img>` tag | HTML images |
| `graphics.DrawLine()` | CSS borders | Or `<hr>` |
| `graphics.DrawRectangle()` | CSS `border` + `div` | HTML boxes |
| `new XFont()` | CSS `font-family` | Web fonts supported |
| `XBrushes.Black` | CSS `color` | Full color support |
| `document.Save()` | `pdf.SaveAs()` | Save to file |
| `PdfReader.Open()` | `PdfDocument.FromFile()` | Load existing PDF |

---

## Code Examples

### Example 1: Basic Text Document

**ZetPDF:**
```csharp
using ZetPdf;
using ZetPdf.Drawing;

var document = new PdfDocument();
var page = document.AddPage();
page.Width = XUnit.FromMillimeter(210);
page.Height = XUnit.FromMillimeter(297);

var graphics = XGraphics.FromPdfPage(page);
var titleFont = new XFont("Arial", 24, XFontStyle.Bold);
var bodyFont = new XFont("Arial", 12);

graphics.DrawString("Company Report", titleFont, XBrushes.Navy,
    new XPoint(50, 50));
graphics.DrawString("This is the introduction paragraph.", bodyFont, XBrushes.Black,
    new XPoint(50, 80));
graphics.DrawString("Generated: " + DateTime.Now.ToString(), bodyFont, XBrushes.Gray,
    new XPoint(50, 100));

document.Save("report.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var html = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 50px; }}
        h1 {{ color: navy; }}
        .date {{ color: gray; }}
    </style>
</head>
<body>
    <h1>Company Report</h1>
    <p>This is the introduction paragraph.</p>
    <p class='date'>Generated: {DateTime.Now}</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 2: Table Data

**ZetPDF (extremely complex):**
```csharp
using ZetPdf;
using ZetPdf.Drawing;

var document = new PdfDocument();
var page = document.AddPage();
var graphics = XGraphics.FromPdfPage(page);
var font = new XFont("Arial", 10);

// Manual table drawing - nightmare!
double x = 50, y = 50;
double colWidth = 100;
double rowHeight = 20;

// Headers
graphics.DrawRectangle(XBrushes.LightGray, x, y, colWidth * 3, rowHeight);
graphics.DrawString("Name", font, XBrushes.Black, x + 5, y + 14);
graphics.DrawString("Quantity", font, XBrushes.Black, x + colWidth + 5, y + 14);
graphics.DrawString("Price", font, XBrushes.Black, x + colWidth * 2 + 5, y + 14);

// Data rows
y += rowHeight;
foreach (var item in items)
{
    graphics.DrawRectangle(XPens.Black, x, y, colWidth * 3, rowHeight);
    graphics.DrawString(item.Name, font, XBrushes.Black, x + 5, y + 14);
    graphics.DrawString(item.Quantity.ToString(), font, XBrushes.Black, x + colWidth + 5, y + 14);
    graphics.DrawString(item.Price.ToString("C"), font, XBrushes.Black, x + colWidth * 2 + 5, y + 14);
    y += rowHeight;
}

document.Save("table.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var rows = items.Select(i => $@"
    <tr>
        <td>{i.Name}</td>
        <td>{i.Quantity}</td>
        <td>{i.Price:C}</td>
    </tr>");

var html = $@"
<html>
<head>
    <style>
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid black; padding: 8px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <table>
        <tr>
            <th>Name</th>
            <th>Quantity</th>
            <th>Price</th>
        </tr>
        {string.Join("", rows)}
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("table.pdf");
```

### Example 3: Multi-Page Document

**ZetPDF:**
```csharp
using ZetPdf;
using ZetPdf.Drawing;

var document = new PdfDocument();
var font = new XFont("Arial", 12);

// Must manually create each page and track position
double y = 50;
double pageHeight = 800;
PdfPage currentPage = document.AddPage();
XGraphics graphics = XGraphics.FromPdfPage(currentPage);

foreach (var paragraph in paragraphs)
{
    // Manual page break detection
    if (y + 50 > pageHeight)
    {
        currentPage = document.AddPage();
        graphics = XGraphics.FromPdfPage(currentPage);
        y = 50;
    }

    graphics.DrawString(paragraph, font, XBrushes.Black, new XPoint(50, y));
    y += 20;
}

document.Save("multipage.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

// Automatic page breaks!
var html = @"
<html>
<body>
" + string.Join("", paragraphs.Select(p => $"<p>{p}</p>")) + @"
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("multipage.pdf");

// Or control page breaks explicitly
var htmlWithBreaks = @"
<div>Page 1 content</div>
<div style='page-break-after: always;'></div>
<div>Page 2 content</div>";
```

### Example 4: Adding Images

**ZetPDF:**
```csharp
using ZetPdf;
using ZetPdf.Drawing;

var document = new PdfDocument();
var page = document.AddPage();
var graphics = XGraphics.FromPdfPage(page);

// Load image
var image = XImage.FromFile("logo.png");

// Draw at specific position
graphics.DrawImage(image, 50, 50, 200, 100);

document.Save("with_image.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var html = @"
<html>
<body>
    <img src='logo.png' style='width: 200px; height: 100px;'>
    <p>Company document with logo</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.BaseUrl = new Uri("file:///C:/path/to/images/");
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("with_image.pdf");
```

### Example 5: Modifying Existing PDFs

**ZetPDF:**
```csharp
using ZetPdf;
using ZetPdf.Drawing;
using ZetPdf.IO;

var document = PdfReader.Open("existing.pdf", PdfDocumentOpenMode.Modify);
var page = document.Pages[0];
var graphics = XGraphics.FromPdfPage(page);

// Add watermark
var font = new XFont("Arial", 72, XFontStyle.Bold);
graphics.RotateTransform(-45);
graphics.DrawString("DRAFT", font, new XSolidBrush(XColor.FromArgb(50, 255, 0, 0)),
    new XPoint(-100, 500));

document.Save("watermarked.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("existing.pdf");

// HTML watermark with full CSS!
pdf.ApplyWatermark(@"
    <div style='
        font-size: 72px;
        font-weight: bold;
        color: rgba(255, 0, 0, 0.2);
        transform: rotate(-45deg);
    '>
        DRAFT
    </div>");

pdf.SaveAs("watermarked.pdf");
```

### Example 6: Merging PDFs (Not Available in ZetPDF)

**ZetPDF:** Limited/no native support

**IronPDF:**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("doc1.pdf");
var pdf2 = PdfDocument.FromFile("doc2.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

---

## Feature Comparison

| Feature | ZetPDF | IronPDF |
|---------|--------|---------|
| HTML to PDF | NO | Yes |
| URL to PDF | NO | Yes |
| CSS Support | NO | Full CSS3 |
| JavaScript | NO | Full ES2024 |
| Automatic Layout | NO | Yes |
| Auto Page Breaks | NO | Yes |
| Tables | Manual drawing | HTML `<table>` |
| Images | Manual placement | `<img>` tag |
| Headers/Footers | Manual | HTML/CSS |
| Watermarks | Manual code | Built-in |
| Merge PDFs | Limited | Yes |
| Split PDFs | Limited | Yes |
| Digital Signatures | NO | Yes |
| PDF/A | NO | Yes |
| Cross-Platform | Yes | Yes |

---

## Migration Checklist

### Pre-Migration

- [ ] **Identify all coordinate-based drawing code**
  ```csharp
  // Before (ZetPDF)
  graphics.DrawString("Name:", font, brush, new XPoint(50, 100));
  graphics.DrawString("John Doe", font, brush, new XPoint(100, 100));

  // After (IronPDF)
  var html = "<p><strong>Name:</strong> John Doe</p>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** IronPDF uses HTML/CSS for layout, eliminating manual coordinate positioning.

- [ ] **Document font and style usage**
  ```csharp
  // Before (ZetPDF)
  var font = new XFont("Arial", 12, XFontStyle.Bold);

  // After (IronPDF)
  var html = "<p style='font-family: Arial; font-size: 12px; font-weight: bold;'>Text</p>";
  ```
  **Why:** Transitioning to CSS styling ensures consistent and flexible design in PDFs.

- [ ] **Map layout to HTML structure**
  ```csharp
  // Before (ZetPDF)
  graphics.DrawRectangle(pen, x, y, width, height);

  // After (IronPDF)
  var html = "<div style='border: 1px solid black; width: 100px; height: 50px;'></div>";
  ```
  **Why:** HTML structure simplifies layout management and supports responsive design.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Replace NuGet package**
  ```bash
  dotnet remove package ZetPDF
  dotnet add package IronPdf
  ```
  **Why:** Ensure the project uses IronPDF for all PDF functionalities.

- [ ] **Update namespaces**
  ```csharp
  // Before (ZetPDF)
  using ZetPDF;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Correct namespaces are necessary for accessing IronPDF classes and methods.

- [ ] **Convert graphics drawing to HTML**
  ```csharp
  // Before (ZetPDF)
  graphics.DrawLine(pen, x1, y1, x2, y2);

  // After (IronPDF)
  var html = "<hr style='border: 1px solid black;'>";
  ```
  **Why:** HTML/CSS provides a more intuitive and flexible way to create visual elements.

- [ ] **Replace XFont with CSS**
  ```csharp
  // Before (ZetPDF)
  var font = new XFont("Times New Roman", 14);

  // After (IronPDF)
  var html = "<p style='font-family: Times New Roman; font-size: 14px;'>Text</p>";
  ```
  **Why:** CSS offers comprehensive styling options that are easier to manage and modify.

- [ ] **Convert tables to HTML tables**
  ```csharp
  // Before (ZetPDF)
  graphics.DrawString("Header", font, brush, new XPoint(x, y));

  // After (IronPDF)
  var html = "<table><tr><th>Header</th></tr></table>";
  ```
  **Why:** HTML tables simplify data presentation and support complex layouts.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations to enable full functionality.

### Testing

- [ ] **Compare visual output**
  **Why:** Ensure the new PDF output matches or improves upon the old design.

- [ ] **Verify fonts match**
  **Why:** Confirm that the CSS-based fonts render as expected compared to the original.

- [ ] **Test page breaks**
  **Why:** IronPDF automatically handles page breaks, but verification ensures content flows correctly.

- [ ] **Verify images render**
  ```csharp
  // Before (ZetPDF)
  graphics.DrawImage(image, x, y);

  // After (IronPDF)
  var html = "<img src='image.png' style='position: absolute; left: 50px; top: 100px;'>";
  ```
  **Why:** Confirm images are correctly positioned and displayed in the new PDF.

- [ ] **Test existing PDF modifications**
  ```csharp
  // Before (ZetPDF)
  var document = PdfReader.Open("existing.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("existing.pdf");
  ```
  **Why:** Ensure that modifications to existing PDFs are seamless and maintain integrity.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
