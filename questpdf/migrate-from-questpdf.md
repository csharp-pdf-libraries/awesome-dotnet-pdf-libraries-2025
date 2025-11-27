# Migration Guide: QuestPDF → IronPDF

## Why Migrate from QuestPDF to IronPDF

IronPDF provides native HTML-to-PDF rendering that QuestPDF completely lacks, eliminating the need to manually reconstruct documents in C# code. It includes comprehensive PDF manipulation features (merge, split, edit, secure) that QuestPDF cannot perform. IronPDF uses a straightforward commercial license without revenue audits or compliance thresholds.

## NuGet Package Changes

```bash
# Remove QuestPDF
dotnet remove package QuestPDF

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| QuestPDF | IronPDF |
|----------|---------|
| `QuestPDF.Fluent` | `IronPdf` |
| `QuestPDF.Helpers` | `IronPdf` |
| `QuestPDF.Infrastructure` | `IronPdf` |
| N/A (no HTML support) | `IronPdf.Rendering` |

## API Mapping

| QuestPDF | IronPDF | Notes |
|----------|---------|-------|
| `Document.Create()` | `ChromePdfRenderer` | IronPDF uses HTML/CSS instead of fluent API |
| `.Page()` | `RenderHtmlAsPdf()` | Direct HTML rendering |
| `.Text()` | Standard HTML tags | Use `<p>`, `<h1>`, etc. |
| `.Image()` | `<img>` tag | Standard HTML image tags |
| `.Table()` | `<table>` tag | Standard HTML tables |
| `.Column()` / `.Row()` | CSS Flexbox/Grid | Use CSS layout |
| `.PageSize()` | `RenderingOptions.PaperSize` | Configure paper dimensions |
| `.GeneratePdf()` | `.SaveAs()` | Output to file |
| N/A | `PdfDocument.Merge()` | Merge multiple PDFs |
| N/A | `PdfDocument.FromFile()` | Load existing PDFs |

## Code Examples

### Example 1: Basic Invoice Generation

**Before (QuestPDF):**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.Content().Column(column =>
        {
            column.Item().Text("Invoice #12345").FontSize(24).Bold();
            column.Item().Text("Date: 2024-01-15").FontSize(12);
            column.Item().PaddingVertical(10);
            column.Item().Text("Customer: Acme Corp");
            column.Item().Text("Total: $1,250.00").FontSize(16).Bold();
        });
    });
}).GeneratePdf("invoice.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial; margin: 2cm; }
        h1 { font-size: 24px; }
        .total { font-size: 16px; font-weight: bold; }
    </style>
</head>
<body>
    <h1>Invoice #12345</h1>
    <p>Date: 2024-01-15</p>
    <p>Customer: Acme Corp</p>
    <p class='total'>Total: $1,250.00</p>
</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

### Example 2: Table with Data

**Before (QuestPDF):**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Content().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Text("Product").Bold();
                header.Cell().Text("Quantity").Bold();
                header.Cell().Text("Price").Bold();
            });

            table.Cell().Text("Widget A");
            table.Cell().Text("10");
            table.Cell().Text("$100");

            table.Cell().Text("Widget B");
            table.Cell().Text("5");
            table.Cell().Text("$75");
        });
    });
}).GeneratePdf("report.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        table { width: 100%; border-collapse: collapse; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; font-weight: bold; }
    </style>
</head>
<body>
    <table>
        <thead>
            <tr>
                <th>Product</th>
                <th>Quantity</th>
                <th>Price</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>Widget A</td>
                <td>10</td>
                <td>$100</td>
            </tr>
            <tr>
                <td>Widget B</td>
                <td>5</td>
                <td>$75</td>
            </tr>
        </tbody>
    </table>
</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 3: Custom Page Size and Headers

**Before (QuestPDF):**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.Letter);
        page.Margin(1, Unit.Inch);
        
        page.Header().Text("Company Report").FontSize(18).Bold();
        
        page.Content().PaddingVertical(10).Column(column =>
        {
            column.Item().Text("Report content here...");
            column.Item().Text("More details...");
        });
        
        page.Footer().AlignCenter().Text(text =>
        {
            text.Span("Page ");
            text.CurrentPageNumber();
        });
    });
}).GeneratePdf("report.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;

var renderer = new ChromePdfRenderer();

// Configure page settings
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
renderer.RenderingOptions.MarginTop = 25;
renderer.RenderingOptions.MarginBottom = 25;
renderer.RenderingOptions.MarginLeft = 25;
renderer.RenderingOptions.MarginRight = 25;

// Set headers and footers
renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    CenterText = "Company Report",
    FontSize = 18
};

renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Page {page}",
    FontSize = 10
};

string html = @"
<!DOCTYPE html>
<html>
<body>
    <p>Report content here...</p>
    <p>More details...</p>
</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

## Common Gotchas

### 1. **Paradigm Shift: Fluent API → HTML/CSS**
QuestPDF uses a C# fluent API for layout. IronPDF requires thinking in HTML/CSS terms. Use your existing web development knowledge or HTML templating engines like Razor.

### 2. **No Direct Layout Translation**
QuestPDF's `.Column()`, `.Row()`, and layout helpers don't have direct equivalents. Use standard CSS layout techniques (Flexbox, Grid, tables) instead.

### 3. **Dynamic Content Generation**
When migrating dynamic content that used C# loops in QuestPDF, use a templating engine (Razor Pages, string interpolation) to generate HTML dynamically before passing to IronPDF.

### 4. **Image Handling**
QuestPDF's `.Image()` method required byte arrays or streams. IronPDF uses standard `<img>` tags with file paths, URLs, or base64-encoded data URIs: `<img src="data:image/png;base64,...">`

### 5. **Font Management**
QuestPDF required explicit font registration. IronPDF uses standard web fonts (Google Fonts, system fonts, or embedded fonts via CSS `@font-face`).

### 6. **Page Breaks**
QuestPDF's `.PageBreak()` becomes CSS: `<div style="page-break-after: always;"></div>` or `page-break-before: always;`

### 7. **PDF Manipulation is Now Possible**
IronPDF can merge, split, and edit existing PDFs—features QuestPDF lacked entirely. Use `PdfDocument.Merge()`, `PdfDocument.FromFile()`, and other manipulation methods.

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/docs/
- **PDF Manipulation Examples**: https://ironpdf.com/tutorials/