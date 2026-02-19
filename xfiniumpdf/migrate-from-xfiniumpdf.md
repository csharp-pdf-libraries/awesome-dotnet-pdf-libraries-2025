# How Do I Migrate from XFINIUM.PDF to IronPDF in C#?

## Why Migrate from XFINIUM.PDF?

XFINIUM.PDF is a low-level PDF library that relies on coordinate-based graphics programming, forcing developers to manually position every element on the page. This approach becomes a maintenance nightmare as requirements change. Key reasons to migrate:

1. **No HTML Support**: Cannot convert HTML/CSS to PDF - only low-level drawing primitives
2. **Coordinate-Based API**: Manual positioning with pixel coordinates like `DrawString("text", font, brush, 50, 100)`
3. **Manual Font Management**: Must create and manage font objects explicitly
4. **No CSS Styling**: No support for modern web styling - must handle colors, fonts, layouts manually
5. **No JavaScript Rendering**: Static content only - cannot render dynamic web content
6. **Complex Text Layout**: Manual text measurement and wrapping calculations required
7. **Limited Documentation**: Smaller community compared to mainstream solutions

### The Core Problem: Graphics API vs HTML

XFINIUM.PDF forces you to think like a graphics programmer, not a document designer:

```csharp
// XFINIUM.PDF: Position every element manually
page.Graphics.DrawString("Invoice", titleFont, titleBrush, new XPoint(50, 50));
page.Graphics.DrawString("Customer:", labelFont, brush, new XPoint(50, 80));
page.Graphics.DrawString(customer.Name, valueFont, brush, new XPoint(120, 80));
page.Graphics.DrawLine(pen, 50, 100, 550, 100);
// ... hundreds of lines for a simple document
```

IronPDF uses familiar HTML/CSS:

```csharp
// IronPDF: Declarative HTML
var html = @"<h1>Invoice</h1><p><b>Customer:</b> " + customer.Name + "</p><hr>";
var pdf = renderer.RenderHtmlAsPdf(html);
```

---

## Quick Start: XFINIUM.PDF to IronPDF

### Step 1: Replace NuGet Package

```bash
# Remove XFINIUM.PDF
dotnet remove package Xfinium.Pdf

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;
using Xfinium.Pdf.Content;
using Xfinium.Pdf.Forms;

// After
using IronPdf;
```

### Step 3: Initialize License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## API Mapping Reference

| XFINIUM.PDF | IronPDF | Notes |
|-------------|---------|-------|
| `PdfFixedDocument` | `ChromePdfRenderer` | Create renderer, not document |
| `PdfPage` | Automatic | Pages created from HTML content |
| `page.Graphics.DrawString()` | HTML text elements | `<p>`, `<h1>`, `<span>`, etc. |
| `page.Graphics.DrawImage()` | `<img>` tag | HTML images |
| `page.Graphics.DrawLine()` | CSS `border` or `<hr>` | HTML/CSS lines |
| `page.Graphics.DrawRectangle()` | CSS `border` on `<div>` | HTML boxes |
| `PdfUnicodeTrueTypeFont` | CSS `font-family` | No font objects needed |
| `PdfRgbColor` | CSS `color` | Standard CSS colors |
| `PdfBrush` | CSS properties | Background, color, etc. |
| `PdfPen` | CSS `border` | Line styling |
| `PdfHtmlTextElement` | `RenderHtmlAsPdf()` | Full HTML support |
| `document.Save(stream)` | `pdf.SaveAs()` or `pdf.BinaryData` | Multiple output options |
| `PdfStringAppearanceOptions` | CSS styling | Use CSS for appearance |
| `PdfStringLayoutOptions` | CSS layout | Flexbox, Grid, etc. |

---

## Code Examples

### Example 1: Basic Document Creation

**XFINIUM.PDF:**
```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;

PdfFixedDocument document = new PdfFixedDocument();
PdfPage page = document.Pages.Add();

// Create font
PdfUnicodeTrueTypeFont font = new PdfUnicodeTrueTypeFont("Arial", 12, true);
PdfBrush brush = new PdfBrush(new PdfRgbColor(0, 0, 0));

// Draw text at exact coordinates
page.Graphics.DrawString("Hello World", font, brush, 50, 50);

using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
{
    document.Save(fs);
}
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: Styled Invoice Document

**XFINIUM.PDF:**
```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;

PdfFixedDocument document = new PdfFixedDocument();
PdfPage page = document.Pages.Add();
PdfGraphics graphics = page.Graphics;

// Fonts - must create each one
PdfUnicodeTrueTypeFont titleFont = new PdfUnicodeTrueTypeFont("Arial", 24, true);
PdfUnicodeTrueTypeFont headerFont = new PdfUnicodeTrueTypeFont("Arial", 14, true);
PdfUnicodeTrueTypeFont bodyFont = new PdfUnicodeTrueTypeFont("Arial", 12, false);

// Colors and brushes
PdfBrush titleBrush = new PdfBrush(new PdfRgbColor(0, 0, 128));
PdfBrush blackBrush = new PdfBrush(new PdfRgbColor(0, 0, 0));
PdfBrush grayBrush = new PdfBrush(new PdfRgbColor(128, 128, 128));
PdfPen linePen = new PdfPen(new PdfRgbColor(200, 200, 200), 1);

// Manual positioning for everything
double y = 50;

graphics.DrawString("INVOICE", titleFont, titleBrush, 50, y);
y += 40;

graphics.DrawLine(linePen, 50, y, 550, y);
y += 20;

graphics.DrawString("Invoice #:", headerFont, blackBrush, 50, y);
graphics.DrawString("INV-2024-001", bodyFont, blackBrush, 150, y);
y += 20;

graphics.DrawString("Date:", headerFont, blackBrush, 50, y);
graphics.DrawString(DateTime.Now.ToString("yyyy-MM-dd"), bodyFont, blackBrush, 150, y);
y += 20;

graphics.DrawString("Customer:", headerFont, blackBrush, 50, y);
graphics.DrawString("Acme Corporation", bodyFont, blackBrush, 150, y);
y += 40;

// Table headers
graphics.DrawString("Item", headerFont, blackBrush, 50, y);
graphics.DrawString("Qty", headerFont, blackBrush, 300, y);
graphics.DrawString("Price", headerFont, blackBrush, 400, y);
graphics.DrawString("Total", headerFont, blackBrush, 500, y);
y += 25;

// Table data
graphics.DrawString("Widget A", bodyFont, blackBrush, 50, y);
graphics.DrawString("10", bodyFont, blackBrush, 300, y);
graphics.DrawString("$25.00", bodyFont, blackBrush, 400, y);
graphics.DrawString("$250.00", bodyFont, blackBrush, 500, y);
y += 20;

graphics.DrawString("Widget B", bodyFont, blackBrush, 50, y);
graphics.DrawString("5", bodyFont, blackBrush, 300, y);
graphics.DrawString("$40.00", bodyFont, blackBrush, 400, y);
graphics.DrawString("$200.00", bodyFont, blackBrush, 500, y);
y += 30;

// Total
graphics.DrawLine(linePen, 400, y, 550, y);
y += 10;
graphics.DrawString("Grand Total:", headerFont, blackBrush, 400, y);
graphics.DrawString("$450.00", headerFont, titleBrush, 500, y);

using (FileStream fs = new FileStream("invoice.pdf", FileMode.Create))
{
    document.Save(fs);
}
```

**IronPDF:**
```csharp
using IronPdf;

var invoice = new
{
    Number = "INV-2024-001",
    Date = DateTime.Now,
    Customer = "Acme Corporation",
    Items = new[]
    {
        new { Name = "Widget A", Qty = 10, Price = 25.00m },
        new { Name = "Widget B", Qty = 5, Price = 40.00m }
    }
};

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 50px; }}
        h1 {{ color: navy; margin-bottom: 20px; }}
        .info {{ margin-bottom: 30px; }}
        .info p {{ margin: 5px 0; }}
        .info strong {{ display: inline-block; width: 100px; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th {{ background: #f5f5f5; text-align: left; padding: 10px; border-bottom: 2px solid #ddd; }}
        td {{ padding: 10px; border-bottom: 1px solid #eee; }}
        .total-row {{ font-weight: bold; }}
        .total-row td {{ border-top: 2px solid #333; padding-top: 15px; }}
        .amount {{ text-align: right; }}
        .grand-total {{ color: navy; font-size: 18px; }}
    </style>
</head>
<body>
    <h1>INVOICE</h1>

    <div class='info'>
        <p><strong>Invoice #:</strong> {invoice.Number}</p>
        <p><strong>Date:</strong> {invoice.Date:yyyy-MM-dd}</p>
        <p><strong>Customer:</strong> {invoice.Customer}</p>
    </div>

    <table>
        <tr>
            <th>Item</th>
            <th>Qty</th>
            <th class='amount'>Price</th>
            <th class='amount'>Total</th>
        </tr>
        {string.Join("", invoice.Items.Select(i => $@"
        <tr>
            <td>{i.Name}</td>
            <td>{i.Qty}</td>
            <td class='amount'>{i.Price:C}</td>
            <td class='amount'>{i.Qty * i.Price:C}</td>
        </tr>"))}
        <tr class='total-row'>
            <td colspan='3'>Grand Total:</td>
            <td class='amount grand-total'>{invoice.Items.Sum(i => i.Qty * i.Price):C}</td>
        </tr>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

### Example 3: Tables with Data

**XFINIUM.PDF:**
```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;

PdfFixedDocument document = new PdfFixedDocument();
PdfPage page = document.Pages.Add();
PdfGraphics graphics = page.Graphics;

PdfUnicodeTrueTypeFont headerFont = new PdfUnicodeTrueTypeFont("Arial", 12, true);
PdfUnicodeTrueTypeFont cellFont = new PdfUnicodeTrueTypeFont("Arial", 10, false);
PdfBrush textBrush = new PdfBrush(PdfRgbColor.Black);
PdfBrush headerBg = new PdfBrush(new PdfRgbColor(240, 240, 240));
PdfPen borderPen = new PdfPen(PdfRgbColor.Black, 0.5);

double x = 50, y = 50;
double[] colWidths = { 150, 100, 100, 100 };
double rowHeight = 25;

// Draw header row
graphics.DrawRectangle(borderPen, headerBg, x, y, colWidths.Sum(), rowHeight);

double cellX = x;
string[] headers = { "Product", "Quantity", "Unit Price", "Total" };
for (int i = 0; i < headers.Length; i++)
{
    graphics.DrawString(headers[i], headerFont, textBrush, cellX + 5, y + 8);
    graphics.DrawLine(borderPen, cellX, y, cellX, y + rowHeight);
    cellX += colWidths[i];
}
graphics.DrawLine(borderPen, cellX, y, cellX, y + rowHeight);

y += rowHeight;

// Draw data rows
var data = GetProducts();
foreach (var item in data)
{
    graphics.DrawRectangle(borderPen, x, y, colWidths.Sum(), rowHeight);

    cellX = x;
    graphics.DrawString(item.Name, cellFont, textBrush, cellX + 5, y + 8);
    cellX += colWidths[0];

    graphics.DrawString(item.Quantity.ToString(), cellFont, textBrush, cellX + 5, y + 8);
    cellX += colWidths[1];

    graphics.DrawString(item.UnitPrice.ToString("C"), cellFont, textBrush, cellX + 5, y + 8);
    cellX += colWidths[2];

    graphics.DrawString(item.Total.ToString("C"), cellFont, textBrush, cellX + 5, y + 8);

    y += rowHeight;

    // Check for page break
    if (y > 750)
    {
        page = document.Pages.Add();
        graphics = page.Graphics;
        y = 50;
    }
}

document.Save("table.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var data = GetProducts();

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 40px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{
            background: #f0f0f0;
            padding: 10px;
            text-align: left;
            border: 1px solid #333;
            font-weight: bold;
        }}
        td {{
            padding: 10px;
            border: 1px solid #ccc;
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
                <th>Quantity</th>
                <th>Unit Price</th>
                <th>Total</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", data.Select(item => $@"
            <tr>
                <td>{item.Name}</td>
                <td class='number'>{item.Quantity}</td>
                <td class='number'>{item.UnitPrice:C}</td>
                <td class='number'>{item.Total:C}</td>
            </tr>"))}
        </tbody>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("table.pdf");
```

### Example 4: Headers and Footers

**XFINIUM.PDF:**
```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;

PdfFixedDocument document = new PdfFixedDocument();
PdfUnicodeTrueTypeFont headerFont = new PdfUnicodeTrueTypeFont("Arial", 10, false);
PdfBrush brush = new PdfBrush(new PdfRgbColor(128, 128, 128));
PdfPen linePen = new PdfPen(new PdfRgbColor(200, 200, 200), 1);

int totalPages = 5;
for (int i = 0; i < totalPages; i++)
{
    PdfPage page = document.Pages.Add();
    PdfGraphics graphics = page.Graphics;

    // Header
    graphics.DrawString("Company Name - Confidential Report", headerFont, brush, 50, 30);
    graphics.DrawLine(linePen, 50, 45, 550, 45);

    // Content area
    // ... draw content between y=50 and y=750 ...

    // Footer
    graphics.DrawLine(linePen, 50, 780, 550, 780);
    graphics.DrawString($"Page {i + 1} of {totalPages}", headerFont, brush, 500, 790);
    graphics.DrawString(DateTime.Now.ToString("yyyy-MM-dd"), headerFont, brush, 50, 790);
}

document.Save("report.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// HTML Header with styling
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; font-size: 10pt; color: gray; border-bottom: 1px solid #ccc; padding-bottom: 5px;'>
            <span>Company Name - Confidential Report</span>
        </div>",
    MaxHeight = 30
};

// HTML Footer with page numbers
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; font-size: 10pt; color: gray; border-top: 1px solid #ccc; padding-top: 5px;'>
            <span style='float: left;'>{date}</span>
            <span style='float: right;'>Page {page} of {total-pages}</span>
        </div>",
    MaxHeight = 30
};

// Margins to accommodate header/footer
renderer.RenderingOptions.MarginTop = 40;
renderer.RenderingOptions.MarginBottom = 40;

var pdf = renderer.RenderHtmlAsPdf(reportHtml);
pdf.SaveAs("report.pdf");
```

### Example 5: Adding Images

**XFINIUM.PDF:**
```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;

PdfFixedDocument document = new PdfFixedDocument();
PdfPage page = document.Pages.Add();
PdfGraphics graphics = page.Graphics;

// Load image
PdfPngImage image = new PdfPngImage("logo.png");

// Draw at specific position and size
graphics.DrawImage(image, 50, 50, 200, 100);

// Draw text below image
PdfUnicodeTrueTypeFont font = new PdfUnicodeTrueTypeFont("Arial", 14, true);
PdfBrush brush = new PdfBrush(PdfRgbColor.Black);
graphics.DrawString("Company Letterhead", font, brush, 50, 170);

document.Save("letterhead.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        .letterhead {
            text-align: center;
            padding: 20px;
        }
        .logo {
            width: 200px;
            height: 100px;
            object-fit: contain;
        }
        h2 {
            margin-top: 20px;
            font-family: Arial, sans-serif;
        }
    </style>
</head>
<body>
    <div class='letterhead'>
        <img src='logo.png' class='logo' alt='Company Logo'>
        <h2>Company Letterhead</h2>
    </div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.BaseUrl = new Uri("file:///C:/path/to/images/");
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("letterhead.pdf");
```

### Example 6: PDF Merging and Manipulation

**XFINIUM.PDF:**
```csharp
using Xfinium.Pdf;

// Load existing PDFs
PdfFixedDocument doc1 = new PdfFixedDocument("cover.pdf");
PdfFixedDocument doc2 = new PdfFixedDocument("content.pdf");
PdfFixedDocument doc3 = new PdfFixedDocument("appendix.pdf");

// Create merged document
PdfFixedDocument merged = new PdfFixedDocument();

// Copy pages manually
foreach (PdfPage page in doc1.Pages)
{
    merged.Pages.Add(page);
}
foreach (PdfPage page in doc2.Pages)
{
    merged.Pages.Add(page);
}
foreach (PdfPage page in doc3.Pages)
{
    merged.Pages.Add(page);
}

merged.Save("book.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var cover = PdfDocument.FromFile("cover.pdf");
var content = PdfDocument.FromFile("content.pdf");
var appendix = PdfDocument.FromFile("appendix.pdf");

// Simple one-line merge
var book = PdfDocument.Merge(cover, content, appendix);
book.SaveAs("book.pdf");

// Or append to existing document
cover.AppendPdf(content);
cover.AppendPdf(appendix);
cover.SaveAs("book.pdf");
```

### Example 7: Security and Encryption

**XFINIUM.PDF:**
```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Security;

PdfFixedDocument document = new PdfFixedDocument();
// ... create content ...

// Set security
document.Security.UserPassword = "user123";
document.Security.OwnerPassword = "owner456";
document.Security.AllowPrinting = false;
document.Security.AllowContentCopying = false;
document.Security.EncryptionAlgorithm = PdfEncryptionAlgorithm.Aes256;

document.Save("protected.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);

// Set security settings
pdf.SecuritySettings.OwnerPassword = "owner456";
pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserAnnotations = false;

pdf.SaveAs("protected.pdf");
```

### Example 8: URL to PDF (Not Possible with XFINIUM.PDF)

**XFINIUM.PDF:** Not supported - requires external HTML parsing library

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Render live web page with full JavaScript support
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.RenderDelay(3000);

var pdf = renderer.RenderUrlAsPdf("https://example.com/dashboard");
pdf.SaveAs("dashboard.pdf");
```

---

## Feature Comparison

| Feature | XFINIUM.PDF | IronPDF |
|---------|-------------|---------|
| **Content Creation** | | |
| HTML to PDF | Limited (PdfHtmlTextElement) | Full Chromium rendering |
| URL to PDF | No | Yes |
| CSS Support | No | Full CSS3 |
| JavaScript | No | Full ES2024 |
| Flexbox/Grid | No | Yes |
| Web Fonts | No | Yes |
| SVG Support | Limited | Full |
| **Layout** | | |
| Automatic Layout | No | Yes |
| Automatic Page Breaks | No | Yes |
| Manual Positioning | Required | Optional (CSS positioning) |
| Tables | Manual drawing | HTML `<table>` |
| **PDF Operations** | | |
| Merge PDFs | Yes | Yes |
| Split PDFs | Yes | Yes |
| Watermarks | Manual drawing | Built-in |
| Headers/Footers | Manual each page | Automatic |
| **Security** | | |
| Password Protection | Yes | Yes |
| Digital Signatures | Yes | Yes |
| Encryption | Yes | Yes |
| PDF/A | Limited | Yes |
| **Development** | | |
| Learning Curve | High (coordinate system) | Low (HTML/CSS) |
| Code Verbosity | Very High | Low |
| Maintenance | Difficult | Easy |
| Cross-Platform | Yes | Yes |

---

## Common Migration Issues

### Issue 1: Coordinate-Based Layout

**XFINIUM.PDF:** Everything requires exact X,Y coordinates.

**Solution:** Use HTML/CSS flow layout. For absolute positioning when needed, use CSS:
```css
.positioned-element {
    position: absolute;
    top: 100px;
    left: 50px;
}
```

### Issue 2: Font Object Management

**XFINIUM.PDF:** Create `PdfUnicodeTrueTypeFont` objects for each font.

**Solution:** Use CSS font-family - fonts are handled automatically:
```html
<style>
    body { font-family: Arial, sans-serif; }
    h1 { font-family: 'Times New Roman', serif; font-size: 24px; }
</style>
```

### Issue 3: Color Handling

**XFINIUM.PDF:** Create `PdfRgbColor` and `PdfBrush` objects.

**Solution:** Use standard CSS colors:
```css
.header { color: navy; background-color: #f5f5f5; }
.warning { color: rgb(255, 0, 0); }
.info { color: rgba(0, 0, 255, 0.8); }
```

### Issue 4: Manual Page Breaks

**XFINIUM.PDF:** Track Y position and create new pages manually.

**Solution:** Automatic page breaks, or use CSS for control:
```css
.section { page-break-after: always; }
.keep-together { page-break-inside: avoid; }
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit all XFINIUM.PDF drawing code**
  ```bash
  grep -r "using Xfinium.Pdf" --include="*.cs" .
  grep -r "Graphics.DrawString\|Graphics.DrawImage\|Graphics.DrawLine" --include="*.cs" .
  ```
  **Why:** Identify all drawing code to ensure complete migration coverage.

- [ ] **Document coordinate-based layouts**
  ```csharp
  // Find patterns like:
  page.Graphics.DrawString("text", font, brush, new XPoint(x, y));
  ```
  **Why:** These need conversion to HTML/CSS for IronPDF.

- [ ] **Note font and color usage**
  ```csharp
  // Find patterns like:
  var font = new PdfUnicodeTrueTypeFont("Arial", 12);
  var color = new PdfRgbColor(255, 0, 0);
  ```
  **Why:** Map these to CSS `font-family` and `color`.

- [ ] **Identify merged PDF workflows**
  ```csharp
  // Find patterns like:
  var document = new PdfFixedDocument();
  document.Pages.Add(new PdfPage());
  ```
  **Why:** IronPDF uses `PdfDocument.Merge()` for combining PDFs.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Remove Xfinium.Pdf package**
  ```bash
  dotnet remove package Xfinium.Pdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Install IronPdf package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project for PDF generation.

- [ ] **Convert DrawString() calls to HTML text**
  ```csharp
  // Before (XFINIUM.PDF)
  page.Graphics.DrawString("Invoice", titleFont, titleBrush, new XPoint(50, 50));

  // After (IronPDF)
  var html = "<h1>Invoice</h1>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** Use HTML for text rendering, allowing for easier styling and layout management.

- [ ] **Convert DrawImage() to `<img>` tags**
  ```csharp
  // Before (XFINIUM.PDF)
  page.Graphics.DrawImage(image, new XPoint(100, 150));

  // After (IronPDF)
  var html = "<img src='image.png' style='position:absolute; left:100px; top:150px;'>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** Use HTML `<img>` tags for image rendering with CSS for positioning.

- [ ] **Convert DrawRectangle/DrawLine to CSS borders**
  ```csharp
  // Before (XFINIUM.PDF)
  page.Graphics.DrawLine(pen, 50, 100, 550, 100);

  // After (IronPDF)
  var html = "<div style='border-top:1px solid black; width:500px; position:absolute; left:50px; top:100px;'></div>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** Use CSS for lines and rectangles, simplifying layout and styling.

- [ ] **Replace font objects with CSS font-family**
  ```csharp
  // Before (XFINIUM.PDF)
  var font = new PdfUnicodeTrueTypeFont("Arial", 12);

  // After (IronPDF)
  var html = "<p style='font-family:Arial; font-size:12px;'>Text</p>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** CSS `font-family` provides a more flexible and familiar way to manage fonts.

- [ ] **Replace color/brush objects with CSS colors**
  ```csharp
  // Before (XFINIUM.PDF)
  var color = new PdfRgbColor(255, 0, 0);

  // After (IronPDF)
  var html = "<p style='color:red;'>Text</p>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** CSS colors are easier to manage and integrate with HTML content.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Testing

- [ ] **Compare visual output**
  **Why:** Ensure the visual appearance matches expectations after migration.

- [ ] **Verify text rendering**
  **Why:** Confirm that all text is rendered correctly with the new HTML/CSS approach.

- [ ] **Check image positioning**
  **Why:** Ensure images are positioned correctly using CSS.

- [ ] **Test page breaks**
  **Why:** Verify that page breaks occur as expected with the new HTML content.

- [ ] **Verify PDF security**
  **Why:** Ensure any security settings are correctly applied in the new PDFs.

- [ ] **Test on all target platforms**
  **Why:** Confirm that PDFs render correctly across all intended platforms and devices.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [CSS Styling for PDFs](https://ironpdf.com/how-to/css/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [XFINIUM.PDF to IronPDF Migration Guide](https://ironpdf.com/blog/migration-guides/migrate-from-xfinium-pdf-to-ironpdf/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
