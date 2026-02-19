# How Do I Migrate from MigraDoc to IronPDF in C#?

## Table of Contents
1. [Why Migrate from MigraDoc](#why-migrate-from-migradoc)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from MigraDoc

### The MigraDoc Challenges

MigraDoc, while powerful for programmatic PDF generation, has fundamental limitations:

1. **No HTML Support**: Must manually construct documents element-by-element—cannot leverage existing HTML/CSS designs
2. **Proprietary Document Model**: Steep learning curve to master `Document`, `Section`, `Paragraph`, `Table`, `Style` APIs
3. **Limited Styling**: Modest styling options compared to CSS—difficult to match modern web designs
4. **Verbose Code**: Creating even simple layouts requires dozens of lines of code
5. **No JavaScript**: Cannot render dynamic content or interactive elements
6. **Charts Are Basic**: Chart functionality is limited compared to web charting libraries
7. **Limited .NET Core Support**: PDFSharp/MigraDoc 6.x has improved but earlier versions had limitations

### The IronPDF Advantage

| Feature | MigraDoc | IronPDF |
|---------|----------|---------|
| Content Definition | Programmatic (Document/Section/Paragraph) | HTML/CSS |
| Learning Curve | Steep (proprietary DOM) | Easy (web skills) |
| Styling | Limited properties | Full CSS3 |
| JavaScript | None | Full Chromium execution |
| Tables | Manual column/row definition | HTML `<table>` with CSS |
| Charts | Basic MigraDoc charts | Any JavaScript charting library |
| Images | Manual sizing/positioning | Standard HTML `<img>` |
| Responsive Layouts | Not supported | Flexbox, Grid |
| Modern Design | Difficult | Natural with CSS |

### Migration Benefits

- **Use Existing Web Skills**: HTML/CSS instead of proprietary document model
- **Unlimited Styling**: Full CSS3 with Flexbox, Grid, animations
- **Modern Charts**: Use Chart.js, D3, Highcharts, or any JS library
- **Simpler Code**: 80% less code for complex layouts
- **Template Engines**: Use Razor, Scriban, or any templating system
- **Consistent Web/PDF Design**: Same HTML for web and PDF

---

## Before You Start

### Prerequisites

1. **.NET Environment**: .NET Framework 4.6.2+ or .NET Core 3.1+ / .NET 5+
2. **NuGet Access**: Ability to install NuGet packages
3. **IronPDF License**: Free trial or purchased license key

### Installation

```bash
# Remove MigraDoc packages
dotnet remove package PdfSharp-MigraDoc
dotnet remove package PdfSharp-MigraDoc-GDI
dotnet remove package PDFsharp.MigraDoc.Standard

# Install IronPDF
dotnet add package IronPdf
```

### License Configuration

```csharp
// Add at application startup (Program.cs or Startup.cs)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Identify MigraDoc Usage

```bash
# Find all MigraDoc references
grep -r "using MigraDoc\|PdfDocumentRenderer\|AddSection\|AddParagraph" --include="*.cs" .
grep -r "AddTable\|AddRow\|AddColumn\|AddCell\|AddImage" --include="*.cs" .
```

---

## Quick Start Migration

### Minimal Change Example

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

public class MigraDocService
{
    public byte[] GenerateReport(ReportData data)
    {
        // Create document
        Document document = new Document();
        document.DefaultPageSetup.PageFormat = PageFormat.A4;

        // Add section
        Section section = document.AddSection();

        // Add title
        Paragraph title = section.AddParagraph(data.Title);
        title.Format.Font.Size = 24;
        title.Format.Font.Bold = true;
        title.Format.SpaceAfter = 20;

        // Add table
        Table table = section.AddTable();
        table.Borders.Width = 0.75;

        Column col1 = table.AddColumn("5cm");
        Column col2 = table.AddColumn("3cm");

        Row headerRow = table.AddRow();
        headerRow.Shading.Color = Colors.LightGray;
        headerRow.Cells[0].AddParagraph("Name");
        headerRow.Cells[1].AddParagraph("Value");

        foreach (var item in data.Items)
        {
            Row row = table.AddRow();
            row.Cells[0].AddParagraph(item.Name);
            row.Cells[1].AddParagraph(item.Value.ToString());
        }

        // Render to PDF
        PdfDocumentRenderer renderer = new PdfDocumentRenderer();
        renderer.Document = document;
        renderer.RenderDocument();

        using (MemoryStream ms = new MemoryStream())
        {
            renderer.PdfDocument.Save(ms);
            return ms.ToArray();
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    }

    public byte[] GenerateReport(ReportData data)
    {
        string html = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 20px; }}
                    h1 {{ font-size: 24px; font-weight: bold; margin-bottom: 20px; }}
                    table {{ border-collapse: collapse; width: 100%; }}
                    th, td {{ border: 1px solid #333; padding: 8px; }}
                    th {{ background-color: #d3d3d3; width: 5cm; }}
                    td:last-child {{ width: 3cm; }}
                </style>
            </head>
            <body>
                <h1>{data.Title}</h1>
                <table>
                    <tr><th>Name</th><th>Value</th></tr>
                    {string.Join("", data.Items.Select(i => $"<tr><td>{i.Name}</td><td>{i.Value}</td></tr>"))}
                </table>
            </body>
            </html>";

        return _renderer.RenderHtmlAsPdf(html).BinaryData;
    }
}
```

---

## Complete API Reference

### Namespace Mappings

| MigraDoc Namespace | IronPDF Equivalent | Notes |
|-------------------|-------------------|-------|
| `MigraDoc.DocumentObjectModel` | `IronPdf` | Main namespace |
| `MigraDoc.DocumentObjectModel.Tables` | Use HTML tables | `<table>` |
| `MigraDoc.DocumentObjectModel.Shapes` | Use HTML/CSS | `<div>`, `<img>` |
| `MigraDoc.DocumentObjectModel.Shapes.Charts` | Use JS charts | Chart.js, etc. |
| `MigraDoc.Rendering` | `IronPdf` | Renderer |
| `PdfSharp.Pdf` | `IronPdf` | PDF operations |

### Class Mappings

| MigraDoc Class | IronPDF Equivalent | Notes |
|---------------|-------------------|-------|
| `Document` | `ChromePdfRenderer` | Use renderer, not document |
| `Section` | HTML `<body>` or `<div>` | Structural container |
| `Paragraph` | HTML `<p>`, `<h1>`, etc. | Text elements |
| `FormattedText` | HTML `<span>`, `<strong>`, etc. | Inline formatting |
| `Table` | HTML `<table>` | With CSS styling |
| `Row` | HTML `<tr>` | Table row |
| `Column` | HTML `<col>` or CSS | Column styling |
| `Cell` | HTML `<td>`, `<th>` | Table cell |
| `Image` | HTML `<img>` | With src attribute |
| `TextFrame` | HTML `<div>` | With CSS positioning |
| `Chart` | JS charting library | Chart.js, D3, etc. |
| `Style` | CSS class or inline style | Full CSS support |
| `HeadersFooters` | `RenderingOptions.HtmlHeader/Footer` | HTML-based |
| `PageSetup` | `RenderingOptions.*` | Page configuration |
| `PdfDocumentRenderer` | `ChromePdfRenderer` | Main renderer |

### Property Mappings

| MigraDoc Property | IronPDF/CSS Equivalent | Notes |
|------------------|------------------------|-------|
| `PageSetup.PageFormat = A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Paper size |
| `PageSetup.Orientation` | `RenderingOptions.PaperOrientation` | Portrait/Landscape |
| `PageSetup.TopMargin` | `RenderingOptions.MarginTop` | In mm |
| `Format.Font.Size = 14` | CSS `font-size: 14pt` | Font sizing |
| `Format.Font.Bold = true` | CSS `font-weight: bold` | Bold text |
| `Format.Font.Italic = true` | CSS `font-style: italic` | Italic text |
| `Format.Font.Color = Colors.Blue` | CSS `color: blue` | Text color |
| `Format.Font.Name = "Arial"` | CSS `font-family: Arial` | Font family |
| `Format.Alignment = Center` | CSS `text-align: center` | Text alignment |
| `Format.SpaceAfter = 10` | CSS `margin-bottom: 10pt` | Spacing |
| `Format.SpaceBefore = 10` | CSS `margin-top: 10pt` | Spacing |
| `Format.LeftIndent = 20` | CSS `margin-left: 20pt` | Indentation |
| `Table.Borders.Width = 1` | CSS `border: 1px solid black` | Table borders |
| `Row.Shading.Color` | CSS `background-color` | Row background |
| `Cell.VerticalAlignment` | CSS `vertical-align` | Cell alignment |
| `Image.Width = "5cm"` | CSS `width: 5cm` | Image sizing |
| `Image.Height = "3cm"` | CSS `height: 3cm` | Image sizing |

### Method Mappings

| MigraDoc Method | IronPDF/HTML Equivalent | Notes |
|----------------|-------------------------|-------|
| `document.AddSection()` | `<div>` or `<section>` | Structural container |
| `section.AddParagraph("text")` | `<p>text</p>` | Paragraph element |
| `section.AddParagraph().AddText("text")` | `<p>text</p>` | Same result |
| `section.AddParagraph().AddFormattedText("text", TextFormat.Bold)` | `<p><strong>text</strong></p>` | Formatted text |
| `section.AddTable()` | `<table>` | Table element |
| `table.AddColumn("5cm")` | `<th style="width:5cm">` or CSS | Column width |
| `table.AddRow()` | `<tr>` | Table row |
| `row.Cells[0].AddParagraph("text")` | `<td>text</td>` | Cell content |
| `section.AddImage(path)` | `<img src="path">` | Image element |
| `section.AddPageBreak()` | `<div style="page-break-after:always">` | Page break |
| `paragraph.AddPageField()` | `{page}` in header/footer | Current page |
| `paragraph.AddNumPagesField()` | `{total-pages}` in header/footer | Total pages |
| `section.Headers.Primary.AddParagraph()` | `RenderingOptions.HtmlHeader` | Header definition |
| `section.Footers.Primary.AddParagraph()` | `RenderingOptions.HtmlFooter` | Footer definition |
| `renderer.RenderDocument()` | `renderer.RenderHtmlAsPdf(html)` | Render PDF |
| `pdfDocument.Save(path)` | `pdf.SaveAs(path)` | Save to file |

---

## Code Migration Examples

### Example 1: Simple Document with Styling

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

Document document = new Document();
Section section = document.AddSection();

Paragraph title = section.AddParagraph("Welcome");
title.Format.Font.Size = 24;
title.Format.Font.Bold = true;
title.Format.Font.Color = Colors.DarkBlue;
title.Format.Alignment = ParagraphAlignment.Center;
title.Format.SpaceAfter = 20;

Paragraph body = section.AddParagraph();
body.AddText("This is regular text. ");
body.AddFormattedText("This is bold.", TextFormat.Bold);
body.AddText(" ");
body.AddFormattedText("This is italic.", TextFormat.Italic);

PdfDocumentRenderer renderer = new PdfDocumentRenderer();
renderer.Document = document;
renderer.RenderDocument();
renderer.PdfDocument.Save("output.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

string html = @"
    <html>
    <head>
        <style>
            h1 {
                font-size: 24pt;
                font-weight: bold;
                color: darkblue;
                text-align: center;
                margin-bottom: 20pt;
            }
        </style>
    </head>
    <body>
        <h1>Welcome</h1>
        <p>
            This is regular text.
            <strong>This is bold.</strong>
            <em>This is italic.</em>
        </p>
    </body>
    </html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: Complex Table with Styling

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

Document document = new Document();
Section section = document.AddSection();

Table table = section.AddTable();
table.Borders.Width = 0.75;
table.Format.Alignment = ParagraphAlignment.Center;

// Define columns
table.AddColumn("4cm");
table.AddColumn("3cm");
table.AddColumn("3cm");

// Header row
Row headerRow = table.AddRow();
headerRow.Shading.Color = new Color(51, 51, 51);
headerRow.Format.Font.Color = Colors.White;
headerRow.Format.Font.Bold = true;
headerRow.Cells[0].AddParagraph("Product");
headerRow.Cells[1].AddParagraph("Quantity");
headerRow.Cells[2].AddParagraph("Price");

// Data rows
string[,] data = { {"Widget", "10", "$25.00"}, {"Gadget", "5", "$50.00"} };
for (int i = 0; i < data.GetLength(0); i++)
{
    Row row = table.AddRow();
    if (i % 2 == 1) row.Shading.Color = Colors.LightGray;
    for (int j = 0; j < 3; j++)
    {
        row.Cells[j].AddParagraph(data[i, j]);
    }
}

// Total row
Row totalRow = table.AddRow();
totalRow.Format.Font.Bold = true;
totalRow.Cells[0].MergeRight = 1;
totalRow.Cells[0].AddParagraph("Total");
totalRow.Cells[2].AddParagraph("$500.00");

PdfDocumentRenderer renderer = new PdfDocumentRenderer();
renderer.Document = document;
renderer.RenderDocument();
renderer.PdfDocument.Save("table.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

string html = @"
    <html>
    <head>
        <style>
            table {
                border-collapse: collapse;
                width: 100%;
                margin: 0 auto;
            }
            th, td {
                border: 1px solid #333;
                padding: 8px;
                text-align: center;
            }
            th {
                background-color: #333;
                color: white;
                font-weight: bold;
            }
            th:nth-child(1) { width: 4cm; }
            th:nth-child(2) { width: 3cm; }
            th:nth-child(3) { width: 3cm; }
            tr:nth-child(even) { background-color: #d3d3d3; }
            .total { font-weight: bold; }
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
                <td>$25.00</td>
            </tr>
            <tr>
                <td>Gadget</td>
                <td>5</td>
                <td>$50.00</td>
            </tr>
            <tr class='total'>
                <td colspan='2'>Total</td>
                <td>$500.00</td>
            </tr>
        </table>
    </body>
    </html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("table.pdf");
```

### Example 3: Headers and Footers with Page Numbers

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

Document document = new Document();
Section section = document.AddSection();

// Header
Paragraph header = section.Headers.Primary.AddParagraph();
header.AddText("Company Report - ");
header.AddDateField("MMMM yyyy");
header.Format.Alignment = ParagraphAlignment.Center;
header.Format.Font.Size = 10;

// Footer with page numbers
Paragraph footer = section.Footers.Primary.AddParagraph();
footer.Format.Alignment = ParagraphAlignment.Center;
footer.Format.Font.Size = 9;
footer.AddText("Page ");
footer.AddPageField();
footer.AddText(" of ");
footer.AddNumPagesField();

// Content
section.AddParagraph("Report content goes here...");

PdfDocumentRenderer renderer = new PdfDocumentRenderer();
renderer.Document = document;
renderer.RenderDocument();
renderer.PdfDocument.Save("report.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = $@"
        <div style='text-align:center; font-size:10pt;'>
            Company Report - {DateTime.Now:MMMM yyyy}
        </div>",
    MaxHeight = 25
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='text-align:center; font-size:9pt;'>
            Page {page} of {total-pages}
        </div>",
    MaxHeight = 25
};

string html = "<html><body><p>Report content goes here...</p></body></html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 4: Images

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;

Document document = new Document();
Section section = document.AddSection();

section.AddParagraph("Company Logo:");

Image image = section.AddImage("logo.png");
image.Width = "5cm";
image.Height = "2cm";
image.LockAspectRatio = true;
image.RelativeVertical = RelativeVertical.Paragraph;
image.RelativeHorizontal = RelativeHorizontal.Margin;

section.AddParagraph("Product Image:");

Image productImage = section.AddImage("product.jpg");
productImage.Width = "10cm";
productImage.WrapFormat.Style = WrapStyle.TopBottom;

PdfDocumentRenderer renderer = new PdfDocumentRenderer();
renderer.Document = document;
renderer.RenderDocument();
renderer.PdfDocument.Save("images.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// For local images, use base64 or file:// URLs
byte[] logoBytes = File.ReadAllBytes("logo.png");
string logoBase64 = Convert.ToBase64String(logoBytes);

byte[] productBytes = File.ReadAllBytes("product.jpg");
string productBase64 = Convert.ToBase64String(productBytes);

string html = $@"
    <html>
    <body>
        <p>Company Logo:</p>
        <img src='data:image/png;base64,{logoBase64}'
             style='width:5cm; max-height:2cm; object-fit:contain;' />

        <p>Product Image:</p>
        <img src='data:image/jpeg;base64,{productBase64}'
             style='width:10cm; display:block;' />
    </body>
    </html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("images.pdf");
```

### Example 5: Page Breaks

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

Document document = new Document();
Section section = document.AddSection();

section.AddParagraph("Content on page 1");

section.AddPageBreak();

section.AddParagraph("Content on page 2");

section.AddPageBreak();

section.AddParagraph("Content on page 3");

PdfDocumentRenderer renderer = new PdfDocumentRenderer();
renderer.Document = document;
renderer.RenderDocument();
renderer.PdfDocument.Save("multipage.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

string html = @"
    <html>
    <head>
        <style>
            .page-break { page-break-after: always; }
        </style>
    </head>
    <body>
        <div class='page-break'>
            <p>Content on page 1</p>
        </div>

        <div class='page-break'>
            <p>Content on page 2</p>
        </div>

        <div>
            <p>Content on page 3</p>
        </div>
    </body>
    </html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("multipage.pdf");
```

### Example 6: Custom Styles

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

Document document = new Document();

// Define custom style
Style style = document.Styles.AddStyle("CustomHeading", "Normal");
style.Font.Size = 18;
style.Font.Bold = true;
style.Font.Color = Colors.DarkRed;
style.ParagraphFormat.SpaceBefore = 10;
style.ParagraphFormat.SpaceAfter = 5;

Style bulletStyle = document.Styles.AddStyle("BulletItem", "Normal");
bulletStyle.ParagraphFormat.LeftIndent = 20;
bulletStyle.ParagraphFormat.FirstLineIndent = -10;

Section section = document.AddSection();

Paragraph heading = section.AddParagraph("Important Notice");
heading.Style = "CustomHeading";

Paragraph bullet1 = section.AddParagraph("• First item");
bullet1.Style = "BulletItem";

Paragraph bullet2 = section.AddParagraph("• Second item");
bullet2.Style = "BulletItem";

PdfDocumentRenderer renderer = new PdfDocumentRenderer();
renderer.Document = document;
renderer.RenderDocument();
renderer.PdfDocument.Save("styles.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

string html = @"
    <html>
    <head>
        <style>
            .custom-heading {
                font-size: 18pt;
                font-weight: bold;
                color: darkred;
                margin-top: 10pt;
                margin-bottom: 5pt;
            }
            ul.bullet-list {
                margin-left: 20pt;
                list-style-type: disc;
            }
            ul.bullet-list li {
                margin-left: -10pt;
            }
        </style>
    </head>
    <body>
        <h2 class='custom-heading'>Important Notice</h2>
        <ul class='bullet-list'>
            <li>First item</li>
            <li>Second item</li>
        </ul>
    </body>
    </html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("styles.pdf");
```

### Example 7: Multiple Sections with Different Layouts

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

Document document = new Document();

// First section - Portrait
Section section1 = document.AddSection();
section1.PageSetup.Orientation = Orientation.Portrait;
section1.PageSetup.PageFormat = PageFormat.A4;
section1.AddParagraph("Portrait section content");

// Second section - Landscape
Section section2 = document.AddSection();
section2.PageSetup.Orientation = Orientation.Landscape;
section2.PageSetup.PageFormat = PageFormat.A4;
section2.AddParagraph("Landscape section content");

PdfDocumentRenderer renderer = new PdfDocumentRenderer();
renderer.Document = document;
renderer.RenderDocument();
renderer.PdfDocument.Save("mixed.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

// Render portrait section
var portraitRenderer = new ChromePdfRenderer();
portraitRenderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
portraitRenderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

var portraitPdf = portraitRenderer.RenderHtmlAsPdf("<p>Portrait section content</p>");

// Render landscape section
var landscapeRenderer = new ChromePdfRenderer();
landscapeRenderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
landscapeRenderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

var landscapePdf = landscapeRenderer.RenderHtmlAsPdf("<p>Landscape section content</p>");

// Merge PDFs
var merged = PdfDocument.Merge(portraitPdf, landscapePdf);
merged.SaveAs("mixed.pdf");
```

### Example 8: Charts (Replacing MigraDoc Charts)

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.Rendering;

Document document = new Document();
Section section = document.AddSection();

Chart chart = section.AddChart(ChartType.Column2D);
chart.Width = "15cm";
chart.Height = "7cm";

Series series = chart.SeriesCollection.AddSeries();
series.Name = "Sales";
series.Add(100);
series.Add(200);
series.Add(150);

chart.XAxis.TickLabels.AddItem().Text = "Q1";
chart.XAxis.TickLabels.AddItem().Text = "Q2";
chart.XAxis.TickLabels.AddItem().Text = "Q3";

PdfDocumentRenderer renderer = new PdfDocumentRenderer();
renderer.Document = document;
renderer.RenderDocument();
renderer.PdfDocument.Save("chart.pdf");
```

**After (IronPDF with Chart.js):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.RenderDelay = 500;  // Wait for chart to render

string html = @"
    <html>
    <head>
        <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
    </head>
    <body>
        <canvas id='myChart' width='600' height='300'></canvas>
        <script>
            const ctx = document.getElementById('myChart').getContext('2d');
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: ['Q1', 'Q2', 'Q3'],
                    datasets: [{
                        label: 'Sales',
                        data: [100, 200, 150],
                        backgroundColor: 'rgba(54, 162, 235, 0.8)'
                    }]
                },
                options: {
                    responsive: false,
                    plugins: {
                        legend: { display: true }
                    }
                }
            });
        </script>
    </body>
    </html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("chart.pdf");
```

### Example 9: Lists

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

Document document = new Document();
Section section = document.AddSection();

// Numbered list (manual)
for (int i = 1; i <= 3; i++)
{
    Paragraph p = section.AddParagraph();
    p.Format.LeftIndent = "1cm";
    p.Format.FirstLineIndent = "-0.5cm";
    p.AddText($"{i}. Item number {i}");
}

// Bullet list (manual)
string[] bullets = { "First bullet", "Second bullet", "Third bullet" };
foreach (string item in bullets)
{
    Paragraph p = section.AddParagraph();
    p.Format.LeftIndent = "1cm";
    p.Format.FirstLineIndent = "-0.5cm";
    p.AddText($"• {item}");
}

PdfDocumentRenderer renderer = new PdfDocumentRenderer();
renderer.Document = document;
renderer.RenderDocument();
renderer.PdfDocument.Save("lists.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

string html = @"
    <html>
    <body>
        <ol>
            <li>Item number 1</li>
            <li>Item number 2</li>
            <li>Item number 3</li>
        </ol>

        <ul>
            <li>First bullet</li>
            <li>Second bullet</li>
            <li>Third bullet</li>
        </ul>
    </body>
    </html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("lists.pdf");
```

### Example 10: Complete Invoice Migration

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

public class MigraDocInvoiceService
{
    public void CreateInvoice(Invoice invoice)
    {
        Document document = new Document();
        Section section = document.AddSection();

        // Header
        Paragraph header = section.AddParagraph("INVOICE");
        header.Format.Font.Size = 28;
        header.Format.Font.Bold = true;
        header.Format.Alignment = ParagraphAlignment.Right;

        section.AddParagraph($"Invoice #: {invoice.Number}").Format.Alignment = ParagraphAlignment.Right;
        section.AddParagraph($"Date: {invoice.Date:yyyy-MM-dd}").Format.Alignment = ParagraphAlignment.Right;

        section.AddParagraph();  // Spacer

        // Customer info
        Paragraph customerHeader = section.AddParagraph("Bill To:");
        customerHeader.Format.Font.Bold = true;
        section.AddParagraph(invoice.CustomerName);
        section.AddParagraph(invoice.CustomerAddress);

        section.AddParagraph();  // Spacer

        // Items table
        Table table = section.AddTable();
        table.Borders.Width = 0.5;

        table.AddColumn("7cm");
        table.AddColumn("2cm");
        table.AddColumn("2.5cm");
        table.AddColumn("2.5cm");

        Row headerRow = table.AddRow();
        headerRow.Shading.Color = new Color(51, 51, 51);
        headerRow.Format.Font.Color = Colors.White;
        headerRow.Format.Font.Bold = true;
        headerRow.Cells[0].AddParagraph("Description");
        headerRow.Cells[1].AddParagraph("Qty");
        headerRow.Cells[2].AddParagraph("Price");
        headerRow.Cells[3].AddParagraph("Total");

        foreach (var item in invoice.Items)
        {
            Row row = table.AddRow();
            row.Cells[0].AddParagraph(item.Description);
            row.Cells[1].AddParagraph(item.Quantity.ToString());
            row.Cells[2].AddParagraph(item.UnitPrice.ToString("C"));
            row.Cells[3].AddParagraph(item.Total.ToString("C"));
        }

        Row totalRow = table.AddRow();
        totalRow.Format.Font.Bold = true;
        totalRow.Cells[0].MergeRight = 2;
        totalRow.Cells[0].AddParagraph("Total").Format.Alignment = ParagraphAlignment.Right;
        totalRow.Cells[3].AddParagraph(invoice.Total.ToString("C"));

        PdfDocumentRenderer renderer = new PdfDocumentRenderer();
        renderer.Document = document;
        renderer.RenderDocument();
        renderer.PdfDocument.Save($"Invoice_{invoice.Number}.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfInvoiceService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfInvoiceService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();
    }

    public void CreateInvoice(Invoice invoice)
    {
        string itemRows = string.Join("", invoice.Items.Select(item => $@"
            <tr>
                <td>{item.Description}</td>
                <td style='text-align:center;'>{item.Quantity}</td>
                <td style='text-align:right;'>{item.UnitPrice:C}</td>
                <td style='text-align:right;'>{item.Total:C}</td>
            </tr>"));

        string html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 40px; }}
                    .header {{ text-align: right; margin-bottom: 30px; }}
                    .invoice-title {{ font-size: 28px; font-weight: bold; }}
                    .customer {{ margin-bottom: 30px; }}
                    .customer strong {{ display: block; margin-bottom: 5px; }}
                    table {{ width: 100%; border-collapse: collapse; }}
                    th {{ background-color: #333; color: white; font-weight: bold; padding: 10px; text-align: left; }}
                    td {{ padding: 10px; border: 1px solid #ddd; }}
                    .total-row {{ font-weight: bold; background-color: #f9f9f9; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <div class='invoice-title'>INVOICE</div>
                    <div>Invoice #: {invoice.Number}</div>
                    <div>Date: {invoice.Date:yyyy-MM-dd}</div>
                </div>

                <div class='customer'>
                    <strong>Bill To:</strong>
                    {invoice.CustomerName}<br/>
                    {invoice.CustomerAddress}
                </div>

                <table>
                    <tr>
                        <th style='width:7cm;'>Description</th>
                        <th style='width:2cm; text-align:center;'>Qty</th>
                        <th style='width:2.5cm; text-align:right;'>Price</th>
                        <th style='width:2.5cm; text-align:right;'>Total</th>
                    </tr>
                    {itemRows}
                    <tr class='total-row'>
                        <td colspan='3' style='text-align:right;'>Total</td>
                        <td style='text-align:right;'>{invoice.Total:C}</td>
                    </tr>
                </table>
            </body>
            </html>";

        _renderer.RenderHtmlAsPdf(html).SaveAs($"Invoice_{invoice.Number}.pdf");
    }
}
```

---

## Advanced Scenarios

### Using Razor Templates

For complex documents, use Razor templating instead of string interpolation:

```csharp
// Install RazorLight or use ASP.NET Core views
using RazorLight;

var engine = new RazorLightEngineBuilder()
    .UseMemoryCachingProvider()
    .Build();

string html = await engine.CompileRenderAsync("Invoice.cshtml", invoice);
var pdf = renderer.RenderHtmlAsPdf(html);
```

Additional templating strategies and complete invoice examples are covered in the [comprehensive walkthrough](https://ironpdf.com/blog/migration-guides/migrate-from-migradoc-to-ironpdf/), which addresses common document generation patterns.

### CSS External Stylesheets

```csharp
// Option 1: BaseUrl for relative paths
renderer.RenderingOptions.BaseUrl = new Uri("file:///C:/MyApp/Styles/");

// Option 2: Embed CSS directly
string css = File.ReadAllText("styles.css");
string html = $"<style>{css}</style>{bodyContent}";
```

### Print-Specific CSS

```csharp
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;

// In HTML:
// @media print { ... }
```

---

## Performance Considerations

### Rendering Time

| Scenario | MigraDoc | IronPDF |
|----------|----------|---------|
| Simple document | 50-100ms | 100-200ms |
| Complex tables | 100-300ms | 150-400ms |
| First render | 100-200ms | 1-3s (Chromium init) |
| Subsequent | 50-200ms | 50-200ms |

### Optimization Tips

1. **Reuse Renderer**: Create once, use for all renders
2. **Warm Up**: Initialize renderer at startup
3. **Disable JavaScript**: If not needed
4. **Optimize Images**: Compress before embedding

```csharp
public class OptimizedPdfService
{
    private readonly ChromePdfRenderer _renderer;
    private static bool _warmedUp;

    public OptimizedPdfService()
    {
        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.EnableJavaScript = false;

        if (!_warmedUp)
        {
            _renderer.RenderHtmlAsPdf("<html></html>");
            _warmedUp = true;
        }
    }
}
```

---

## Troubleshooting

### Issue 1: Fonts Look Different

**Cause**: Different font rendering
**Solution**: Specify fonts explicitly in CSS

```html
<style>
    body { font-family: 'Helvetica Neue', Arial, sans-serif; }
</style>
```

### Issue 2: Tables Breaking Across Pages

**Cause**: CSS page-break not set
**Solution**: Use CSS page-break properties

```html
<style>
    tr { page-break-inside: avoid; }
    thead { display: table-header-group; }
</style>
```

### Issue 3: Images Not Loading

**Cause**: Relative paths not resolved
**Solution**: Use base64 or absolute paths

```csharp
byte[] imageBytes = File.ReadAllBytes(imagePath);
string base64 = Convert.ToBase64String(imageBytes);
string imgSrc = $"data:image/png;base64,{base64}";
```

### Issue 4: Page Size Different

**Cause**: Default margins different
**Solution**: Set explicit margins

```csharp
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;
```

### Issue 5: First Render Slow

**Cause**: Chromium initialization
**Solution**: Warm up at startup

```csharp
// In Program.cs
IronPdf.License.LicenseKey = "YOUR-KEY";
new ChromePdfRenderer().RenderHtmlAsPdf("<html></html>");
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Identify all MigraDoc document structures**
  ```bash
  grep -r "using MigraDoc" --include="*.cs" .
  grep -r "Document\|Section\|AddParagraph" --include="*.cs" .
  grep -r "PdfDocumentRenderer" --include="*.cs" .
  ```
  **Why:** MigraDoc's programmatic document model must be converted to HTML/CSS. Identify all usage patterns to plan the conversion.

- [ ] **Document styles and formatting used**
  ```csharp
  // Find patterns like:
  title.Format.Font.Size = 24;
  title.Format.Font.Bold = true;
  title.Format.Font.Color = Colors.DarkBlue;
  paragraph.Format.SpaceAfter = 10;
  ```
  **Why:** These MigraDoc formatting properties map to CSS. Document them now to ensure consistent styling after migration.

- [ ] **List all tables, images, charts**
  ```bash
  grep -r "AddTable\|AddColumn\|AddRow\|AddCell" --include="*.cs" .
  grep -r "AddImage\|AddChart" --include="*.cs" .
  ```
  **Why:** Tables require conversion to HTML `<table>` elements. Images need base64 encoding or URL paths. Charts can be replaced with modern JavaScript libraries.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove MigraDoc packages and install IronPdf**
  ```bash
  dotnet remove package PdfSharp-MigraDoc
  dotnet remove package PdfSharp-MigraDoc-GDI
  dotnet remove package PDFsharp.MigraDoc.Standard
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch. IronPDF uses HTML/CSS instead of MigraDoc's programmatic document model.

- [ ] **Add license key configuration**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Convert Document/Section/Paragraph to HTML**
  ```csharp
  // Before (MigraDoc)
  Document document = new Document();
  Section section = document.AddSection();
  Paragraph title = section.AddParagraph("Welcome");
  title.Format.Font.Size = 24;
  title.Format.Font.Bold = true;
  title.Format.SpaceAfter = 20;

  // After (IronPDF)
  string html = @"
      <html>
      <head>
          <style>
              h1 { font-size: 24pt; font-weight: bold; margin-bottom: 20pt; }
          </style>
      </head>
      <body>
          <h1>Welcome</h1>
      </body>
      </html>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** HTML is far more intuitive than MigraDoc's verbose programmatic API. Use web skills you already have.

- [ ] **Convert Table structures to HTML tables**
  ```csharp
  // Before (MigraDoc - 20+ lines)
  Table table = section.AddTable();
  table.Borders.Width = 0.75;
  Column col1 = table.AddColumn("5cm");
  Column col2 = table.AddColumn("3cm");
  Row headerRow = table.AddRow();
  headerRow.Shading.Color = Colors.LightGray;
  headerRow.Cells[0].AddParagraph("Name");
  // ... many more lines

  // After (IronPDF - clean HTML)
  string html = @"
      <table style='border-collapse: collapse;'>
          <tr style='background-color: #d3d3d3;'>
              <th style='width:5cm; border:1px solid #333;'>Name</th>
              <th style='width:3cm; border:1px solid #333;'>Value</th>
          </tr>
          <tr>
              <td style='border:1px solid #333;'>Item 1</td>
              <td style='border:1px solid #333;'>100</td>
          </tr>
      </table>";
  ```
  **Why:** HTML tables are dramatically simpler than MigraDoc's column/row/cell API. Use CSS for styling, `:nth-child` for zebra striping.

- [ ] **Replace AddImage with img tags**
  ```csharp
  // Before (MigraDoc)
  Image image = section.AddImage("logo.png");
  image.Width = "5cm";
  image.Height = "2cm";
  image.LockAspectRatio = true;

  // After (IronPDF - use base64 for local files)
  byte[] imageBytes = File.ReadAllBytes("logo.png");
  string base64 = Convert.ToBase64String(imageBytes);
  string html = $@"<img src='data:image/png;base64,{base64}'
                        style='width:5cm; max-height:2cm; object-fit:contain;' />";
  ```
  **Why:** HTML `<img>` tags with CSS give you more control. Use base64 for embedded images or URLs for external images.

- [ ] **Convert Styles to CSS classes**
  ```csharp
  // Before (MigraDoc custom styles)
  Style style = document.Styles.AddStyle("CustomHeading", "Normal");
  style.Font.Size = 18;
  style.Font.Bold = true;
  style.Font.Color = Colors.DarkRed;
  style.ParagraphFormat.SpaceBefore = 10;

  Paragraph heading = section.AddParagraph("Notice");
  heading.Style = "CustomHeading";

  // After (IronPDF CSS classes)
  string html = @"
      <style>
          .custom-heading {
              font-size: 18pt;
              font-weight: bold;
              color: darkred;
              margin-top: 10pt;
          }
      </style>
      <h2 class='custom-heading'>Notice</h2>";
  ```
  **Why:** CSS classes are more powerful than MigraDoc styles. Full CSS3 support including Flexbox, Grid, animations.

- [ ] **Replace Headers/Footers with HtmlHeader/HtmlFooter**
  ```csharp
  // Before (MigraDoc)
  Paragraph footer = section.Footers.Primary.AddParagraph();
  footer.Format.Alignment = ParagraphAlignment.Center;
  footer.AddText("Page ");
  footer.AddPageField();
  footer.AddText(" of ");
  footer.AddNumPagesField();

  // After (IronPDF)
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = @"<div style='text-align:center; font-size:9pt;'>
          Page {page} of {total-pages}
      </div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with special placeholders. More flexible than MigraDoc's limited formatting.

- [ ] **Update page setup to RenderingOptions**
  ```csharp
  // Before (MigraDoc)
  document.DefaultPageSetup.PageFormat = PageFormat.A4;
  section.PageSetup.Orientation = Orientation.Landscape;
  section.PageSetup.TopMargin = "2cm";
  section.PageSetup.BottomMargin = "2cm";

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  renderer.RenderingOptions.MarginTop = 20;  // mm
  renderer.RenderingOptions.MarginBottom = 20;
  ```
  **Why:** Direct property mapping. RenderingOptions provides all page setup controls in one place.

- [ ] **Replace MigraDoc charts with JavaScript charting**
  ```csharp
  // Before (MigraDoc - basic charts)
  Chart chart = section.AddChart(ChartType.Column2D);
  chart.Width = "15cm";
  Series series = chart.SeriesCollection.AddSeries();
  series.Add(100); series.Add(200); series.Add(150);

  // After (IronPDF with Chart.js - modern interactive charts)
  renderer.RenderingOptions.EnableJavaScript = true;
  renderer.RenderingOptions.WaitFor.JavaScript(500);

  string html = @"
      <canvas id='chart'></canvas>
      <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
      <script>
          new Chart(document.getElementById('chart'), {
              type: 'bar',
              data: { labels: ['Q1','Q2','Q3'], datasets: [{
                  label: 'Sales', data: [100, 200, 150]
              }]}
          });
      </script>";
  ```
  **Why:** MigraDoc charts are basic. Chart.js, D3, Highcharts give you modern, beautiful, interactive charts.

- [ ] **Convert page breaks**
  ```csharp
  // Before (MigraDoc)
  section.AddPageBreak();

  // After (IronPDF - CSS)
  string html = @"
      <div style='page-break-after: always;'>
          Content on page 1
      </div>
      <div>
          Content on page 2
      </div>";
  ```
  **Why:** CSS `page-break-after: always` or `page-break-before: always` controls pagination.

- [ ] **Handle mixed page orientations**
  ```csharp
  // Before (MigraDoc - multiple sections)
  Section portrait = document.AddSection();
  portrait.PageSetup.Orientation = Orientation.Portrait;
  Section landscape = document.AddSection();
  landscape.PageSetup.Orientation = Orientation.Landscape;

  // After (IronPDF - render and merge)
  var portraitRenderer = new ChromePdfRenderer();
  portraitRenderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
  var pdf1 = portraitRenderer.RenderHtmlAsPdf(portraitHtml);

  var landscapeRenderer = new ChromePdfRenderer();
  landscapeRenderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  var pdf2 = landscapeRenderer.RenderHtmlAsPdf(landscapeHtml);

  var merged = PdfDocument.Merge(pdf1, pdf2);
  merged.SaveAs("output.pdf");
  ```
  **Why:** For documents with mixed orientations, render each section separately then merge. Gives more control than MigraDoc's section approach.

### Testing

- [ ] **Compare visual output**
  **Why:** HTML/CSS rendering may differ slightly from MigraDoc. Verify key documents look correct.

- [ ] **Verify page breaks work correctly**
  ```css
  /* Add to prevent tables from splitting awkwardly */
  tr { page-break-inside: avoid; }
  thead { display: table-header-group; }
  ```
  **Why:** CSS page-break properties may need tuning. Add `page-break-inside: avoid` to elements that shouldn't split.

- [ ] **Check header/footer rendering**
  **Why:** IronPDF placeholders (`{page}`, `{total-pages}`) replace MigraDoc's `AddPageField()` and `AddNumPagesField()`.

- [ ] **Validate table formatting**
  **Why:** Table borders, spacing, and alignment may need CSS adjustment. Use `border-collapse: collapse` for consistent borders.

- [ ] **Test image rendering**
  **Why:** Base64-encoded images work best. Verify images display at correct sizes with proper aspect ratios.

- [ ] **Benchmark performance**
  ```csharp
  // IronPDF first render is slower (Chromium init), subsequent renders are fast
  // Warm up at startup:
  new ChromePdfRenderer().RenderHtmlAsPdf("<html></html>");
  ```
  **Why:** IronPDF's first render includes Chromium initialization (1-3 seconds). Warm up at startup for consistent performance.

---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/
- **MigraDoc Documentation**: https://docs.pdfsharp.net/MigraDoc/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
