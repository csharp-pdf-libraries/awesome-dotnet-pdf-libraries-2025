# How Do I Migrate from PDFSharp to IronPDF in C#?

## Why Migrate from PDFSharp to IronPDF

PDFSharp requires manual positioning of every element using GDI+ style coordinates, making document generation tedious and error-prone. IronPDF supports native HTML-to-PDF conversion with modern CSS3 (including flexbox and grid), allowing you to leverage web technologies instead of calculating X,Y positions. This dramatically reduces development time and makes PDF generation maintainable through standard HTML/CSS skills.

### The Coordinate Calculation Problem

PDFSharp's GDI+ approach means you must:
- Calculate exact X,Y positions for every element
- Manually track content height for page overflow
- Handle line wrapping and text measurement yourself
- Draw tables cell by cell with border calculations
- Manage multi-page documents with manual page breaks

### Key Migration Benefits with [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/)

| Aspect | PDFSharp | IronPDF |
|--------|----------|---------|
| Document Creation | Coordinate-based drawing | HTML/CSS html to pdf c# templates |
| Layout System | Manual X,Y positioning | CSS Flow/Flexbox/Grid |
| Page Breaks | Manual calculation | Automatic + CSS control |
| Tables | Draw cells individually | HTML `<table>` |
| Styling | Code-based fonts/colors | CSS stylesheets |
| Maintenance | Difficult to modify | Edit HTML/CSS |
| Learning Curve | GDI+ knowledge required | Web skills transfer |
| License | MIT (Free) | Commercial |

For a side-by-side feature analysis, see the [detailed comparison](https://ironsoftware.com/suite/blog/comparison/compare-pdfsharp-vs-ironpdf/).

---

## NuGet Package Changes

```bash
# Remove PDFSharp
dotnet remove package PdfSharp
dotnet remove package PdfSharp-wpf
dotnet remove package PdfSharp.Charting

# Add IronPDF
dotnet add package IronPdf
```

---

## Namespace Mapping

| PDFSharp | IronPDF |
|----------|---------|
| `PdfSharp.Pdf` | `IronPdf` |
| `PdfSharp.Drawing` | Not needed (use HTML/CSS) |
| `PdfSharp.Pdf.IO` | `IronPdf` |
| `PdfSharp.Charting` | HTML/CSS or charting libraries |

---

## API Mapping

| PDFSharp API | IronPDF API | Notes |
|--------------|-------------|-------|
| `new PdfDocument()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Create from HTML |
| `document.AddPage()` | Automatic | Pages created from HTML content |
| `XGraphics.FromPdfPage()` | Not needed | Use HTML elements |
| `XGraphics.DrawString()` | HTML `<p>`, `<h1>`, etc. | Position with CSS |
| `XGraphics.DrawImage()` | HTML `<img>` tag | Position with CSS |
| `XGraphics.DrawLine()` | CSS borders or SVG | `<hr>` or CSS |
| `XGraphics.DrawRectangle()` | CSS or SVG | `<div>` with borders |
| `XGraphics.DrawEllipse()` | SVG `<ellipse>` | Or CSS border-radius |
| `XFont` | CSS `font-family`, `font-size` | Standard CSS |
| `XBrush`, `XPen` | CSS colors/borders | `color`, `background-color` |
| `XRect` | CSS positioning | `margin`, `padding`, `width`, `height` |
| `document.Save()` | `pdf.SaveAs()` | Similar functionality |
| `PdfReader.Open()` | `PdfDocument.FromFile()` | Open existing PDF |
| `document.Pages.Count` | `pdf.PageCount` | Page count |
| `document.Version` | `pdf.MetaData` | Document properties |

---

## Code Examples

### Example 1: Simple Document with Text

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;

PdfDocument document = new PdfDocument();
document.Info.Title = "My Document";

PdfPage page = document.AddPage();
XGraphics gfx = XGraphics.FromPdfPage(page);
XFont font = new XFont("Arial", 20);

gfx.DrawString("Hello, World!", font, XBrushes.Black,
    new XRect(50, 50, page.Width, page.Height),
    XStringFormats.TopLeft);

document.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(@"
    <html>
    <head><title>My Document</title></head>
    <body>
        <h1>Hello, World!</h1>
    </body>
    </html>");

pdf.SaveAs("output.pdf");
```

### Example 2: Styled Document with Multiple Elements

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;

PdfDocument document = new PdfDocument();
PdfPage page = document.AddPage();
XGraphics gfx = XGraphics.FromPdfPage(page);

// Title
XFont titleFont = new XFont("Arial", 24, XFontStyle.Bold);
gfx.DrawString("Invoice", titleFont, XBrushes.Black,
    new XRect(50, 50, page.Width, page.Height),
    XStringFormats.TopLeft);

// Body text
XFont bodyFont = new XFont("Arial", 12);
gfx.DrawString("Customer: John Doe", bodyFont, XBrushes.Black,
    new XRect(50, 100, page.Width, page.Height),
    XStringFormats.TopLeft);

gfx.DrawString("Total: $150.00", bodyFont, XBrushes.Black,
    new XRect(50, 130, page.Width, page.Height),
    XStringFormats.TopLeft);

// Draw a line
XPen pen = new XPen(XColors.Black, 1);
gfx.DrawLine(pen, 50, 90, 300, 90);

document.Save("invoice.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

string html = @"
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; padding: 50px; }
        h1 { font-size: 24px; font-weight: bold; border-bottom: 1px solid black; padding-bottom: 10px; }
        p { font-size: 12px; margin: 10px 0; }
    </style>
</head>
<body>
    <h1>Invoice</h1>
    <p>Customer: John Doe</p>
    <p>Total: $150.00</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

### Example 3: Document with Images

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;

PdfDocument document = new PdfDocument();
PdfPage page = document.AddPage();
XGraphics gfx = XGraphics.FromPdfPage(page);

XFont font = new XFont("Arial", 16);
gfx.DrawString("Company Report", font, XBrushes.Black,
    new XRect(50, 50, page.Width, page.Height),
    XStringFormats.TopLeft);

XImage image = XImage.FromFile("logo.png");
gfx.DrawImage(image, 50, 100, 150, 75);

document.Save("report.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

string html = @"
<html>
<head>
    <style>
        h1 { font-size: 16px; }
        img { width: 150px; height: 75px; }
    </style>
</head>
<body>
    <h1>Company Report</h1>
    <img src='logo.png' alt='Logo' />
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 4: Tables

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;

PdfDocument document = new PdfDocument();
PdfPage page = document.AddPage();
XGraphics gfx = XGraphics.FromPdfPage(page);

XFont headerFont = new XFont("Arial", 12, XFontStyle.Bold);
XFont cellFont = new XFont("Arial", 10);
XPen pen = new XPen(XColors.Black, 1);

// Draw table manually - tedious!
double startX = 50, startY = 50;
double colWidth = 100, rowHeight = 25;

// Header row
gfx.DrawRectangle(pen, XBrushes.LightGray, startX, startY, colWidth * 3, rowHeight);
gfx.DrawString("Product", headerFont, XBrushes.Black, startX + 5, startY + 17);
gfx.DrawString("Qty", headerFont, XBrushes.Black, startX + colWidth + 5, startY + 17);
gfx.DrawString("Price", headerFont, XBrushes.Black, startX + colWidth * 2 + 5, startY + 17);

// Data rows - repeat for each row
double y = startY + rowHeight;
gfx.DrawRectangle(pen, startX, y, colWidth * 3, rowHeight);
gfx.DrawString("Widget", cellFont, XBrushes.Black, startX + 5, y + 15);
gfx.DrawString("10", cellFont, XBrushes.Black, startX + colWidth + 5, y + 15);
gfx.DrawString("$9.99", cellFont, XBrushes.Black, startX + colWidth * 2 + 5, y + 15);

// Draw column separators
gfx.DrawLine(pen, startX + colWidth, startY, startX + colWidth, y + rowHeight);
gfx.DrawLine(pen, startX + colWidth * 2, startY, startX + colWidth * 2, y + rowHeight);

document.Save("table.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

string html = @"
<html>
<head>
    <style>
        table { border-collapse: collapse; width: 300px; }
        th { background-color: lightgray; font-weight: bold; padding: 5px; border: 1px solid black; }
        td { padding: 5px; border: 1px solid black; }
    </style>
</head>
<body>
    <table>
        <tr>
            <th>Product</th>
            <th>Qty</th>
            <th>Price</th>
        </tr>
        <tr>
            <td>Widget</td>
            <td>10</td>
            <td>$9.99</td>
        </tr>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("table.pdf");
```

### Example 5: Multi-Page Document with Page Breaks

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;

PdfDocument document = new PdfDocument();

// Must manually create pages and track content position
for (int i = 1; i <= 3; i++)
{
    PdfPage page = document.AddPage();
    XGraphics gfx = XGraphics.FromPdfPage(page);
    XFont font = new XFont("Arial", 16);

    gfx.DrawString($"Page {i} of 3", font, XBrushes.Black,
        new XRect(50, 50, page.Width, page.Height),
        XStringFormats.TopLeft);
}

document.Save("multipage.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

string html = @"
<html>
<head>
    <style>
        .page { page-break-after: always; padding: 50px; }
        .page:last-child { page-break-after: auto; }
    </style>
</head>
<body>
    <div class='page'><h1>Page 1</h1><p>Content for page 1...</p></div>
    <div class='page'><h1>Page 2</h1><p>Content for page 2...</p></div>
    <div class='page'><h1>Page 3</h1><p>Content for page 3...</p></div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("multipage.pdf");
```

### Example 6: Headers and Footers

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Drawing;

PdfDocument document = new PdfDocument();
PdfPage page = document.AddPage();
XGraphics gfx = XGraphics.FromPdfPage(page);

XFont headerFont = new XFont("Arial", 10);
XFont bodyFont = new XFont("Arial", 12);

// Header - draw at top
gfx.DrawString("Company Name", headerFont, XBrushes.Gray,
    new XRect(50, 20, page.Width - 100, 20),
    XStringFormats.TopLeft);

// Must draw line manually
XPen pen = new XPen(XColors.Gray, 0.5);
gfx.DrawLine(pen, 50, 35, page.Width - 50, 35);

// Body content
gfx.DrawString("Document content here...", bodyFont, XBrushes.Black,
    new XRect(50, 60, page.Width - 100, page.Height),
    XStringFormats.TopLeft);

// Footer - calculate position from bottom
gfx.DrawLine(pen, 50, page.Height - 40, page.Width - 50, page.Height - 40);
gfx.DrawString("Page 1", headerFont, XBrushes.Gray,
    new XRect(50, page.Height - 35, page.Width - 100, 20),
    XStringFormats.TopLeft);

document.Save("header-footer.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align: center; font-size: 10px; color: gray; border-bottom: 1px solid gray;'>Company Name</div>"
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align: center; font-size: 10px; color: gray; border-top: 1px solid gray;'>Page {page} of {total-pages}</div>"
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Document Content</h1><p>Your content here...</p>");
pdf.SaveAs("header-footer.pdf");
```

### Example 7: Opening and Modifying Existing PDFs

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;

// Open existing PDF
PdfDocument document = PdfReader.Open("existing.pdf", PdfDocumentOpenMode.Modify);
PdfPage page = document.Pages[0];

// Get graphics object for the page
XGraphics gfx = XGraphics.FromPdfPage(page);
XFont font = new XFont("Arial", 20, XFontStyle.Bold);

// Add watermark at calculated position
gfx.DrawString("CONFIDENTIAL", font, XBrushes.Red,
    new XPoint(200, 400));

document.Save("modified.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("existing.pdf");

var textStamper = new TextStamper
{
    Text = "CONFIDENTIAL",
    FontSize = 20,
    FontFamily = "Arial",
    IsBold = true,
    FontColor = IronSoftware.Drawing.Color.Red,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};

pdf.ApplyStamp(textStamper);
pdf.SaveAs("modified.pdf");
```

### Example 8: Merging PDFs

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

PdfDocument outputDocument = new PdfDocument();

// Open first document
PdfDocument inputDocument1 = PdfReader.Open("doc1.pdf", PdfDocumentOpenMode.Import);
foreach (PdfPage page in inputDocument1.Pages)
{
    outputDocument.AddPage(page);
}

// Open second document
PdfDocument inputDocument2 = PdfReader.Open("doc2.pdf", PdfDocumentOpenMode.Import);
foreach (PdfPage page in inputDocument2.Pages)
{
    outputDocument.AddPage(page);
}

outputDocument.Save("merged.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf1 = PdfDocument.FromFile("doc1.pdf");
var pdf2 = PdfDocument.FromFile("doc2.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

### Example 9: Splitting PDFs

**Before (PDFSharp):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

PdfDocument inputDocument = PdfReader.Open("source.pdf", PdfDocumentOpenMode.Import);

for (int i = 0; i < inputDocument.Pages.Count; i++)
{
    PdfDocument outputDocument = new PdfDocument();
    outputDocument.AddPage(inputDocument.Pages[i]);
    outputDocument.Save($"page_{i + 1}.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("source.pdf");

for (int i = 0; i < pdf.PageCount; i++)
{
    var singlePage = pdf.CopyPage(i);
    singlePage.SaveAs($"page_{i + 1}.pdf");
}
```

### Example 10: URL to PDF

**Before (PDFSharp):**
```csharp
// PDFSharp does NOT support URL to PDF conversion
// You would need a separate library like a headless browser
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://www.example.com");
pdf.SaveAs("website.pdf");
```

---

## Common Gotchas

### 1. Coordinate System vs. Flow Layout
- **PDFSharp:** You must calculate exact X,Y coordinates for positioning
- **IronPDF:** Uses HTML flow layout; elements position automatically. Use CSS for precise control.

### 2. Page Breaks
- **PDFSharp:** Manually create new pages and track content overflow
- **IronPDF:** Automatic page breaks. Control with CSS: `page-break-before`, `page-break-after`, `page-break-inside: avoid`

### 3. Fonts
- **PDFSharp:** Must instantiate `XFont` objects and embed fonts manually
- **IronPDF:** Use standard CSS `@font-face` or system fonts via `font-family`

### 4. File Paths for Images
- **PDFSharp:** Requires full file system paths
- **IronPDF:** Supports relative paths, URLs, or base64-encoded images in `src` attributes

### 5. Tables and Layouts
- **PDFSharp:** Must calculate cell positions and draw borders manually
- **IronPDF:** Use HTML `<table>` or CSS Grid/Flexbox for complex layouts

### 6. License Requirements
- **PDFSharp:** Open source (MIT license)
- **IronPDF:** Commercial product; requires license for production use (free trial available)

### 7. Drawing Operations
- **PDFSharp:** Direct graphics operations (`DrawLine`, `DrawRectangle`)
- **IronPDF:** Use CSS borders, SVG, or HTML5 Canvas for graphics

### 8. Text Measurement
- **PDFSharp:** `gfx.MeasureString()` to calculate text dimensions
- **IronPDF:** Not needed - browser engine handles text flow automatically

### 9. Document Properties
- **PDFSharp:** `document.Info.Title`, `document.Info.Author`, etc.
- **IronPDF:** `pdf.MetaData.Title`, `pdf.MetaData.Author`, etc.

### 10. Performance Considerations
- **PDFSharp:** Lightweight, fast for simple documents
- **IronPDF:** Uses Chromium engine - more features but larger footprint

---

## Find All PDFSharp References

```bash
# Find all PDFSharp usages in your codebase
grep -r "PdfSharp\|XGraphics\|XFont\|XBrush" --include="*.cs" .
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PDFSharp usages in codebase**
  ```bash
  # Find all PDFSharp usages
  grep -r "PdfSharp\|XGraphics\|XFont\|XBrush\|XPen" --include="*.cs" .
  ```
  **Why:** Understanding your current usage patterns helps plan the migration scope and identify complex areas.

- [ ] **Identify document types being generated**
  ```csharp
  // Document types to identify:
  // - Reports (text + tables) → HTML templates
  // - Invoices (structured data) → HTML with CSS Grid
  // - Certificates (graphics-heavy) → HTML + SVG
  // - Forms (fillable) → IronPDF form handling
  ```
  **Why:** Different document types may require different IronPDF approaches (HTML templates vs. direct manipulation).

- [ ] **Note any custom graphics or drawing operations**
  ```csharp
  // PDFSharp drawing operations to convert:
  gfx.DrawLine()      // → CSS border or <hr> or SVG <line>
  gfx.DrawRectangle() // → <div> with CSS border
  gfx.DrawEllipse()   // → CSS border-radius: 50% or SVG <ellipse>
  gfx.DrawString()    // → HTML text elements (<p>, <span>, <h1>)
  ```
  **Why:** PDFSharp's `XGraphics` drawing translates to HTML/CSS or SVG in IronPDF.

### Package Changes

- [ ] **Remove PdfSharp NuGet packages**
  ```bash
  dotnet remove package PdfSharp
  dotnet remove package PdfSharp-wpf
  dotnet remove package PdfSharp.Charting
  ```
  **Why:** Avoid conflicts and reduce package bloat.

- [ ] **Add IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Single package provides complete PDF functionality.

- [ ] **Add license key initialization**
  ```csharp
  // Program.cs or Startup.cs
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

  // Or from environment variable (recommended for production)
  IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");
  ```
  **Why:** Required for production use; add early in application startup.

### Code Updates

- [ ] **Replace namespace imports**
  ```csharp
  // Before
  using PdfSharp.Pdf;
  using PdfSharp.Drawing;
  using PdfSharp.Pdf.IO;

  // After
  using IronPdf;
  ```
  **Why:** IronPDF has a simpler namespace structure.

- [ ] **Convert coordinate-based layouts to HTML/CSS**
  ```csharp
  // Before (PDFSharp) - manual positioning nightmare
  gfx.DrawString("Invoice", titleFont, XBrushes.Black, new XPoint(50, 50));
  gfx.DrawString("Customer: John", bodyFont, XBrushes.Black, new XPoint(50, 80));

  // After (IronPDF) - let CSS handle layout
  var html = @"
  <div style='padding: 50px;'>
      <h1>Invoice</h1>
      <p>Customer: John</p>
  </div>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** HTML layout engines handle positioning, text wrapping, and page breaks automatically.

- [ ] **Replace XFont with CSS font properties**
  ```csharp
  // Before (PDFSharp)
  var titleFont = new XFont("Arial", 24, XFontStyle.Bold);
  var bodyFont = new XFont("Times New Roman", 12);

  // After (IronPDF) - CSS in HTML
  var html = @"
  <style>
      h1 { font-family: Arial, sans-serif; font-size: 24px; font-weight: bold; }
      p { font-family: 'Times New Roman', serif; font-size: 12px; }
  </style>";
  ```
  **Why:** CSS fonts are more flexible and support web fonts.

- [ ] **Replace XBrush/XPen with CSS colors/borders**
  ```csharp
  // Before (PDFSharp)
  gfx.DrawRectangle(new XPen(XColors.Black, 2), XBrushes.LightGray, rect);

  // After (IronPDF) - CSS
  var html = @"<div style='border: 2px solid black; background-color: lightgray;'>Content</div>";
  ```
  **Why:** CSS provides more styling options with less code.

- [ ] **Convert table drawing code to HTML tables**
  ```csharp
  // Before (PDFSharp) - 50+ lines of manual cell drawing
  gfx.DrawRectangle(pen, x, y, cellWidth, cellHeight);
  gfx.DrawString(text, font, brush, x + 5, y + 15);
  // ... repeat for every cell

  // After (IronPDF) - clean HTML
  var html = @"
  <table style='border-collapse: collapse; width: 100%;'>
      <tr><th>Product</th><th>Price</th></tr>
      <tr><td>Widget</td><td>$9.99</td></tr>
  </table>";
  ```
  **Why:** HTML tables handle cell sizing, borders, and pagination automatically.

- [ ] **Update page break handling**
  ```csharp
  // Before (PDFSharp) - manual page creation
  if (currentY > pageHeight - margin) {
      page = document.AddPage();
      gfx = XGraphics.FromPdfPage(page);
      currentY = margin;
  }

  // After (IronPDF) - automatic with CSS control
  var html = @"
  <div>Page 1 content</div>
  <div style='page-break-before: always;'>Page 2 content</div>";
  // Or let content flow naturally with automatic page breaks
  ```
  **Why:** CSS page-break properties replace manual page creation.

- [ ] **Convert header/footer code**
  ```csharp
  // Before (PDFSharp) - draw on every page manually
  foreach (var page in document.Pages) {
      var gfx = XGraphics.FromPdfPage(page);
      gfx.DrawString("Header", font, brush, new XPoint(50, 30));
      gfx.DrawString($"Page {pageNum}", font, brush, new XPoint(50, page.Height - 30));
  }

  // After (IronPDF) - declarative
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter {
      HtmlFragment = "<div style='text-align:center;'>Header</div>"
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** IronPDF's HTML headers/footers with placeholders are simpler and more flexible.

### Testing

- [ ] **Visual comparison of generated PDFs**
  ```csharp
  // Generate test PDFs with both libraries for comparison
  // Key areas to verify:
  // - Font rendering and sizes
  // - Table alignment and borders
  // - Image quality and positioning
  // - Page margins and layout
  // - Color accuracy
  ```
  **Why:** Ensure output quality matches or exceeds PDFSharp output.

- [ ] **Test multi-page documents**
  ```csharp
  // Test with long content that spans multiple pages
  var longContent = string.Join("", Enumerable.Repeat("<p>Paragraph content...</p>", 100));
  var pdf = renderer.RenderHtmlAsPdf($"<body>{longContent}</body>");
  Console.WriteLine($"Generated {pdf.PageCount} pages");
  ```
  **Why:** Verify automatic pagination works correctly.

- [ ] **Verify font rendering**
  ```csharp
  // Test with various fonts
  var html = @"
  <p style='font-family: Arial;'>Arial text</p>
  <p style='font-family: Times New Roman;'>Times New Roman text</p>
  <p style='font-family: Courier New;'>Monospace text</p>";
  ```
  **Why:** Ensure fonts display correctly across platforms.

### Deployment

- [ ] **Configure license for production**
  ```csharp
  // appsettings.json
  {
      "IronPdf": {
          "LicenseKey": "YOUR-LICENSE-KEY"
      }
  }

  // Configuration in DI
  services.AddSingleton(sp => {
      IronPdf.License.LicenseKey = configuration["IronPdf:LicenseKey"];
      return new ChromePdfRenderer();
  });
  ```
  **Why:** Ensure license is configured correctly in all environments.

- [ ] **Update CI/CD pipelines**
  ```yaml
  # GitHub Actions example
  - name: Restore dependencies
    run: dotnet restore

  - name: Set IronPDF License
    run: echo "IRONPDF_LICENSE_KEY=${{ secrets.IRONPDF_LICENSE_KEY }}" >> $GITHUB_ENV
  ```
  **Why:** Ensure builds work with new package dependencies.
---

## Additional Resources

- **IronPDF Documentation:** https://ironpdf.com/docs/
- **Tutorials and Examples:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference:** https://ironpdf.com/object-reference/api/
