# How Do I Migrate from VectSharp to IronPDF in C#?

## Why Migrate from VectSharp?

VectSharp is a scientific visualization and vector graphics library designed for creating diagrams, charts, and technical illustrations. It's **not designed for document generation** - it's a drawing library that happens to output PDF. Key reasons to migrate:

1. **Scientific Focus Only**: Designed for data visualization and plotting, not documents
2. **No HTML Support**: Cannot convert HTML/CSS to PDF - requires manual vector drawing
3. **Coordinate-Based API**: Must position every element with exact X,Y coordinates
4. **No CSS Styling**: No support for web styling - all styling is programmatic
5. **No JavaScript**: Cannot render dynamic web content
6. **No Text Layout**: No automatic text wrapping, pagination, or flow layout
7. **Graphics-First Paradigm**: Designed for diagrams, not reports or invoices

### The Core Problem: Graphics Library vs Document Generator

VectSharp is built for scientists creating figures and plots, not for developers generating business documents:

```csharp
// VectSharp: Manual vector drawing for every element
Page page = new Page(595, 842);
Graphics graphics = page.Graphics;
graphics.FillRectangle(50, 50, 200, 100, Colour.FromRgb(0, 0, 255));
graphics.FillText(60, 70, "Invoice", new Font(new FontFamily("Arial"), 20), Colours.White);
// ... continue drawing every single element manually
```

IronPDF uses HTML - the universal document format:

```csharp
// IronPDF: Declarative HTML for document creation
var html = "<h1>Invoice</h1><p>Customer: Acme Corp</p>";
var pdf = renderer.RenderHtmlAsPdf(html);
```

This paradigm shift from vector drawing to HTML is explored in depth throughout the [VectSharp to IronPDF migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-vectsharp-to-ironpdf/).

---

## Quick Start: VectSharp to IronPDF

### Step 1: Replace NuGet Packages

```bash
# Remove VectSharp packages
dotnet remove package VectSharp
dotnet remove package VectSharp.PDF

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using VectSharp;
using VectSharp.PDF;

// After
using IronPdf;
```

### Step 3: Initialize License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## API Mapping Reference

| VectSharp | IronPDF | Notes |
|-----------|---------|-------|
| `Document` | `ChromePdfRenderer` | Create renderer |
| `Page` | Automatic | Pages created from HTML |
| `Graphics` | HTML/CSS | Declarative markup |
| `graphics.FillRectangle()` | CSS `background-color` on `<div>` | HTML boxes |
| `graphics.StrokeRectangle()` | CSS `border` on `<div>` | Borders |
| `graphics.FillText()` | HTML text elements | `<p>`, `<h1>`, `<span>` |
| `graphics.StrokePath()` | SVG or CSS borders | Vector paths |
| `GraphicsPath` | SVG `<path>` element | Complex shapes |
| `Colour.FromRgb()` | CSS color values | `rgb()`, `#hex`, named |
| `Font` / `FontFamily` | CSS `font-family` | Web fonts supported |
| `doc.SaveAsPDF()` | `pdf.SaveAs()` | Save to file |
| Manual page sizing | `RenderingOptions.PaperSize` | Or CSS `@page` |

---

## Code Examples

### Example 1: Basic Document Creation

**VectSharp:**
```csharp
using VectSharp;
using VectSharp.PDF;

Document doc = new Document();
Page page = new Page(595, 842); // A4 size in points
Graphics graphics = page.Graphics;

graphics.FillRectangle(50, 50, 200, 100, Colour.FromRgb(0, 0, 255));
graphics.FillText(60, 70, "Hello from VectSharp",
    new Font(new FontFamily("Arial"), 20), Colour.FromRgb(255, 255, 255));

doc.Pages.Add(page);
doc.SaveAsPDF("output.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        .box {
            width: 200px;
            height: 100px;
            background-color: blue;
            padding: 20px;
            margin: 50px;
        }
        h1 {
            color: white;
            font-family: Arial, sans-serif;
            font-size: 20px;
            margin: 0;
        }
    </style>
</head>
<body>
    <div class='box'>
        <h1>Hello from IronPDF</h1>
    </div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: Multi-Page Document

**VectSharp:**
```csharp
using VectSharp;
using VectSharp.PDF;

Document doc = new Document();

for (int i = 1; i <= 3; i++)
{
    Page page = new Page(595, 842);
    Graphics graphics = page.Graphics;

    graphics.FillText(50, 50, $"Page {i}",
        new Font(new FontFamily("Arial"), 24), Colours.Black);

    doc.Pages.Add(page);
}

doc.SaveAsPDF("multipage.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        .page { page-break-after: always; padding: 50px; }
        .page:last-child { page-break-after: auto; }
        h1 { font-size: 24px; }
    </style>
</head>
<body>
    <div class='page'><h1>Page 1</h1></div>
    <div class='page'><h1>Page 2</h1></div>
    <div class='page'><h1>Page 3</h1></div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("multipage.pdf");
```

### Example 3: Scientific Chart (VectSharp's Specialty)

**VectSharp:**
```csharp
using VectSharp;
using VectSharp.PDF;

Document doc = new Document();
Page page = new Page(800, 600);
Graphics graphics = page.Graphics;

// Draw chart axes
graphics.StrokePath(new GraphicsPath().MoveTo(50, 550).LineTo(50, 50), Colours.Black, 2);
graphics.StrokePath(new GraphicsPath().MoveTo(50, 550).LineTo(750, 550), Colours.Black, 2);

// Draw data points
double[] data = { 100, 250, 180, 320, 280, 450 };
double barWidth = 80;
for (int i = 0; i < data.Length; i++)
{
    double x = 80 + i * 110;
    double height = data[i];
    graphics.FillRectangle(x, 550 - height, barWidth, height, Colour.FromRgb(0, 102, 204));
}

// Labels
for (int i = 0; i < data.Length; i++)
{
    double x = 80 + i * 110 + barWidth / 2 - 10;
    graphics.FillText(x, 565, $"Q{i + 1}",
        new Font(new FontFamily("Arial"), 12), Colours.Black);
}

graphics.FillText(350, 30, "Quarterly Sales",
    new Font(new FontFamily("Arial"), 24), Colours.Black);

doc.Pages.Add(page);
doc.SaveAsPDF("chart.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

// IronPDF can render any JavaScript chart library!
var html = @"
<!DOCTYPE html>
<html>
<head>
    <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; }
        h1 { text-align: center; }
        .chart-container { width: 700px; height: 400px; margin: 0 auto; }
    </style>
</head>
<body>
    <h1>Quarterly Sales</h1>
    <div class='chart-container'>
        <canvas id='chart'></canvas>
    </div>
    <script>
        new Chart(document.getElementById('chart'), {
            type: 'bar',
            data: {
                labels: ['Q1', 'Q2', 'Q3', 'Q4', 'Q5', 'Q6'],
                datasets: [{
                    label: 'Sales',
                    data: [100, 250, 180, 320, 280, 450],
                    backgroundColor: 'rgba(0, 102, 204, 0.8)'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: { legend: { display: false } }
            }
        });
    </script>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.JavaScript(2000);
renderer.RenderingOptions.PaperSize = PdfPaperSize.Custom;
renderer.RenderingOptions.SetCustomPaperSizeinPixelsOrPoints(800, 600);

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("chart.pdf");
```

### Example 4: Custom Page Size with Shapes

**VectSharp:**
```csharp
using VectSharp;
using VectSharp.PDF;

Document doc = new Document();
Page page = new Page(800, 600);
Graphics graphics = page.Graphics;

// Draw a circle
GraphicsPath circlePath = new GraphicsPath();
circlePath.Arc(400, 300, 100, 0, 2 * Math.PI);
graphics.FillPath(circlePath, Colour.FromRgb(255, 0, 0));

// Add text inside circle
graphics.FillText(360, 295, "Circle",
    new Font(new FontFamily("Arial"), 16), Colours.White);

doc.Pages.Add(page);
doc.SaveAsPDF("custom.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

var html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { margin: 0; }
        .container {
            width: 800px;
            height: 600px;
            display: flex;
            justify-content: center;
            align-items: center;
        }
        .circle {
            width: 200px;
            height: 200px;
            background-color: red;
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
        }
        .circle span {
            color: white;
            font-family: Arial, sans-serif;
            font-size: 16px;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='circle'>
            <span>Circle</span>
        </div>
    </div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.Custom;
renderer.RenderingOptions.SetCustomPaperSizeinPixelsOrPoints(800, 600);

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("custom.pdf");
```

### Example 5: SVG Graphics in IronPDF (Best of Both Worlds)

**VectSharp:**
```csharp
using VectSharp;
using VectSharp.PDF;

Document doc = new Document();
Page page = new Page(600, 400);
Graphics graphics = page.Graphics;

// Complex path drawing
GraphicsPath path = new GraphicsPath();
path.MoveTo(300, 50);
path.LineTo(400, 150);
path.LineTo(350, 250);
path.LineTo(250, 250);
path.LineTo(200, 150);
path.Close();

graphics.FillPath(path, Colour.FromRgb(0, 128, 0));
graphics.StrokePath(path, Colour.FromRgb(0, 100, 0), 3);

doc.Pages.Add(page);
doc.SaveAsPDF("polygon.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

// Use inline SVG for vector graphics
var html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { margin: 0; padding: 20px; }
    </style>
</head>
<body>
    <svg width='600' height='400' viewBox='0 0 600 400'>
        <polygon
            points='300,50 400,150 350,250 250,250 200,150'
            fill='rgb(0, 128, 0)'
            stroke='rgb(0, 100, 0)'
            stroke-width='3'/>
    </svg>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.Custom;
renderer.RenderingOptions.SetCustomPaperSizeinPixelsOrPoints(600, 400);

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("polygon.pdf");
```

### Example 6: Business Report (Why IronPDF is Better)

**VectSharp (Extremely Verbose):**
```csharp
using VectSharp;
using VectSharp.PDF;

Document doc = new Document();
Page page = new Page(595, 842);
Graphics graphics = page.Graphics;

Font titleFont = new Font(new FontFamily("Arial"), 28);
Font headerFont = new Font(new FontFamily("Arial"), 14);
Font bodyFont = new Font(new FontFamily("Arial"), 11);

double y = 50;

// Title
graphics.FillText(50, y, "Monthly Sales Report", titleFont, Colours.Black);
y += 50;

// Subtitle
graphics.FillText(50, y, $"Generated: {DateTime.Now:MMMM yyyy}", bodyFont, Colours.Gray);
y += 40;

// Table header
graphics.FillRectangle(50, y, 495, 25, Colour.FromRgb(240, 240, 240));
graphics.StrokeRectangle(50, y, 495, 25, Colours.Black, 1);
graphics.FillText(55, y + 7, "Product", headerFont, Colours.Black);
graphics.FillText(200, y + 7, "Q1", headerFont, Colours.Black);
graphics.FillText(280, y + 7, "Q2", headerFont, Colours.Black);
graphics.FillText(360, y + 7, "Q3", headerFont, Colours.Black);
graphics.FillText(440, y + 7, "Q4", headerFont, Colours.Black);
y += 25;

// Table rows
string[,] data = {
    { "Widget A", "$12,000", "$14,500", "$11,800", "$16,200" },
    { "Widget B", "$8,500", "$9,200", "$10,100", "$11,000" },
    { "Widget C", "$5,200", "$4,800", "$6,100", "$7,300" }
};

for (int row = 0; row < 3; row++)
{
    graphics.StrokeRectangle(50, y, 495, 22, Colours.LightGray, 1);
    graphics.FillText(55, y + 5, data[row, 0], bodyFont, Colours.Black);
    graphics.FillText(200, y + 5, data[row, 1], bodyFont, Colours.Black);
    graphics.FillText(280, y + 5, data[row, 2], bodyFont, Colours.Black);
    graphics.FillText(360, y + 5, data[row, 3], bodyFont, Colours.Black);
    graphics.FillText(440, y + 5, data[row, 4], bodyFont, Colours.Black);
    y += 22;
}

doc.Pages.Add(page);
doc.SaveAsPDF("report.pdf");
```

**IronPDF (Clean and Maintainable):**
```csharp
using IronPdf;

var products = new[]
{
    new { Name = "Widget A", Q1 = 12000, Q2 = 14500, Q3 = 11800, Q4 = 16200 },
    new { Name = "Widget B", Q1 = 8500, Q2 = 9200, Q3 = 10100, Q4 = 11000 },
    new { Name = "Widget C", Q1 = 5200, Q2 = 4800, Q3 = 6100, Q4 = 7300 }
};

var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 50px; }}
        h1 {{ margin-bottom: 5px; }}
        .subtitle {{ color: gray; margin-bottom: 30px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th {{ background: #f0f0f0; padding: 10px; text-align: left; border: 1px solid #ccc; }}
        td {{ padding: 8px 10px; border: 1px solid #eee; }}
        .number {{ text-align: right; }}
    </style>
</head>
<body>
    <h1>Monthly Sales Report</h1>
    <p class='subtitle'>Generated: {DateTime.Now:MMMM yyyy}</p>

    <table>
        <thead>
            <tr>
                <th>Product</th>
                <th>Q1</th>
                <th>Q2</th>
                <th>Q3</th>
                <th>Q4</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", products.Select(p => $@"
            <tr>
                <td>{p.Name}</td>
                <td class='number'>{p.Q1:C0}</td>
                <td class='number'>{p.Q2:C0}</td>
                <td class='number'>{p.Q3:C0}</td>
                <td class='number'>{p.Q4:C0}</td>
            </tr>"))}
        </tbody>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 7: Headers and Footers

**VectSharp (Manual on Each Page):**
```csharp
using VectSharp;
using VectSharp.PDF;

Document doc = new Document();
Font headerFont = new Font(new FontFamily("Arial"), 10);

int totalPages = 5;
for (int i = 0; i < totalPages; i++)
{
    Page page = new Page(595, 842);
    Graphics graphics = page.Graphics;

    // Header
    graphics.FillText(50, 30, "Company Report - Confidential", headerFont, Colours.Gray);
    graphics.StrokePath(new GraphicsPath().MoveTo(50, 45).LineTo(545, 45), Colours.LightGray, 1);

    // Content area (y = 60 to 780)
    // ... draw content ...

    // Footer
    graphics.StrokePath(new GraphicsPath().MoveTo(50, 795).LineTo(545, 795), Colours.LightGray, 1);
    graphics.FillText(50, 810, DateTime.Now.ToString("yyyy-MM-dd"), headerFont, Colours.Gray);
    graphics.FillText(480, 810, $"Page {i + 1} of {totalPages}", headerFont, Colours.Gray);

    doc.Pages.Add(page);
}

doc.SaveAsPDF("report.pdf");
```

**IronPDF (Automatic):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; font-size: 10pt; color: gray; border-bottom: 1px solid #ccc; padding-bottom: 5px;'>
            Company Report - Confidential
        </div>",
    MaxHeight = 30
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; font-size: 10pt; color: gray; border-top: 1px solid #ccc; padding-top: 5px;'>
            <span style='float: left;'>{date}</span>
            <span style='float: right;'>Page {page} of {total-pages}</span>
        </div>",
    MaxHeight = 30
};

renderer.RenderingOptions.MarginTop = 50;
renderer.RenderingOptions.MarginBottom = 50;

var pdf = renderer.RenderHtmlAsPdf(reportHtml);
pdf.SaveAs("report.pdf");
```

### Example 8: URL to PDF (Not Possible with VectSharp)

**VectSharp:** Not possible - VectSharp cannot render HTML or web pages

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Render any web page with full JavaScript support
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.RenderDelay(3000);

var pdf = renderer.RenderUrlAsPdf("https://example.com/dashboard");
pdf.SaveAs("dashboard.pdf");
```

---

## Feature Comparison

| Feature | VectSharp | IronPDF |
|---------|-----------|---------|
| **Primary Purpose** | Scientific visualization | Document generation |
| **Content Creation** | | |
| HTML to PDF | No | Full Chromium |
| URL to PDF | No | Yes |
| CSS Support | No | Full CSS3 |
| JavaScript | No | Full ES2024 |
| Chart.js/D3.js | No | Yes |
| **Layout** | | |
| Automatic Layout | No | Yes |
| Automatic Page Breaks | No | Yes |
| Manual Positioning | Required | Optional |
| Tables | Manual drawing | HTML `<table>` |
| Text Wrapping | Manual | Automatic |
| **Graphics** | | |
| Vector Shapes | Native | SVG in HTML |
| Paths | GraphicsPath | SVG `<path>` |
| Images | Yes | `<img>` tag |
| Scientific Plots | Specialty | Via JS libraries |
| **PDF Operations** | | |
| Merge PDFs | No | Yes |
| Split PDFs | No | Yes |
| Watermarks | Manual | Built-in |
| Headers/Footers | Manual per page | Automatic |
| Password Protection | No | Yes |
| Digital Signatures | No | Yes |
| **Development** | | |
| Learning Curve | High (coordinates) | Low (HTML/CSS) |
| Code Verbosity | Very High | Low |
| Maintenance | Complex | Easy |
| Cross-Platform | Yes | Yes |

---

## Migration Strategies

### Strategy 1: Convert Drawing Code to HTML/CSS

Replace coordinate-based drawing with HTML elements:

```csharp
// VectSharp
graphics.FillRectangle(100, 50, 300, 80, Colour.FromRgb(0, 102, 204));
graphics.FillText(110, 80, "Header", font, Colours.White);

// IronPDF HTML equivalent
<div style="
    position: absolute;
    left: 100px;
    top: 50px;
    width: 300px;
    height: 80px;
    background: rgb(0, 102, 204);
    color: white;
    padding: 10px;
">Header</div>
```

### Strategy 2: Use SVG for Vector Graphics

For complex shapes, use inline SVG:

```csharp
// VectSharp path
GraphicsPath path = new GraphicsPath();
path.MoveTo(100, 100);
path.LineTo(200, 50);
path.LineTo(300, 100);
path.Close();
graphics.FillPath(path, Colours.Blue);

// IronPDF SVG equivalent
<svg><polygon points="100,100 200,50 300,100" fill="blue"/></svg>
```

### Strategy 3: Use JavaScript Chart Libraries

For scientific visualizations, leverage powerful JS libraries:

```csharp
// Instead of manual VectSharp chart drawing, use Chart.js, D3.js, or Plotly
var html = @"
<script src='https://cdn.plot.ly/plotly-latest.min.js'></script>
<div id='chart'></div>
<script>
    Plotly.newPlot('chart', [{
        x: [1, 2, 3, 4],
        y: [10, 15, 13, 17],
        type: 'scatter'
    }]);
</script>";
```

---

## Common Migration Issues

### Issue 1: Coordinate System Differences

**VectSharp:** Uses points from top-left origin.

**Solution:** IronPDF uses CSS positioning:
```css
.element {
    position: absolute;
    top: 50px;
    left: 100px;
}
```

### Issue 2: Font Objects

**VectSharp:** Creates `Font` and `FontFamily` objects.

**Solution:** Use CSS font-family:
```html
<style>
    body { font-family: Arial, sans-serif; font-size: 12pt; }
</style>
```

### Issue 3: Color Handling

**VectSharp:** Uses `Colour.FromRgb()`.

**Solution:** Use CSS colors:
```css
.header { color: rgb(0, 102, 204); background-color: #f0f0f0; }
```

### Issue 4: Graphics Paths

**VectSharp:** Complex `GraphicsPath` API.

**Solution:** Use SVG for vector graphics:
```html
<svg>
    <path d="M 100 100 L 200 50 L 300 100 Z" fill="blue"/>
</svg>
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Identify all VectSharp drawing operations**
  ```bash
  grep -r "using VectSharp" --include="*.cs" .
  grep -r "Graphics\|FillRectangle\|FillText" --include="*.cs" .
  ```
  **Why:** Identify all drawing operations to ensure complete migration coverage.

- [ ] **Document page sizes and layouts**
  ```csharp
  // Find patterns like:
  Page page = new Page(595, 842);
  ```
  **Why:** These dimensions map to IronPDF's RenderingOptions.PaperSize. Document them now to ensure consistent output after migration.

- [ ] **Note color schemes and fonts**
  ```csharp
  // Patterns to document:
  Colour.FromRgb(0, 0, 255);
  new Font(new FontFamily("Arial"), 20);
  ```
  **Why:** These settings map to CSS styles in IronPDF. Document them to maintain visual consistency.

- [ ] **Identify complex vector graphics**
  ```csharp
  // Look for:
  GraphicsPath path = new GraphicsPath();
  ```
  **Why:** Complex paths will need conversion to SVG or CSS in IronPDF.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Remove VectSharp packages**
  ```bash
  dotnet remove package VectSharp
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Install IronPdf package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project for PDF generation.

- [ ] **Convert FillRectangle to CSS boxes**
  ```csharp
  // Before (VectSharp)
  graphics.FillRectangle(50, 50, 200, 100, Colour.FromRgb(0, 0, 255));

  // After (IronPDF)
  var html = "<div style='width:200px; height:100px; background-color:rgb(0,0,255);'></div>";
  ```
  **Why:** Use HTML/CSS for layout and styling in IronPDF.

- [ ] **Convert FillText to HTML text**
  ```csharp
  // Before (VectSharp)
  graphics.FillText(60, 70, "Invoice", new Font(new FontFamily("Arial"), 20), Colours.White);

  // After (IronPDF)
  var html = "<p style='font-family:Arial; font-size:20px; color:white;'>Invoice</p>";
  ```
  **Why:** Use HTML text elements for better text layout and styling.

- [ ] **Convert GraphicsPath to SVG**
  ```csharp
  // Before (VectSharp)
  GraphicsPath path = new GraphicsPath();
  // Define path...

  // After (IronPDF)
  var svg = "<svg><path d='...'/></svg>";
  ```
  **Why:** SVG provides a scalable and flexible way to define vector graphics in HTML.

- [ ] **Replace manual page management with CSS**
  ```csharp
  // Before (VectSharp)
  Page page = new Page(595, 842);

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** Use IronPDF's RenderingOptions for automatic page management.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Testing

- [ ] **Compare visual output**
  **Why:** Ensure the new PDF output matches the original design.

- [ ] **Verify colors match**
  **Why:** Confirm that color schemes are consistent with the original.

- [ ] **Check positioning accuracy**
  **Why:** Ensure elements are positioned correctly in the new PDF.

- [ ] **Test page breaks**
  **Why:** Verify that content flows correctly across pages.

- [ ] **Verify vector graphics render correctly**
  **Why:** Ensure that complex graphics are accurately represented in the new format.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [SVG in PDFs](https://ironpdf.com/how-to/svg-to-pdf/)
- [JavaScript Charts in PDFs](https://ironpdf.com/how-to/javascript-charts/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
