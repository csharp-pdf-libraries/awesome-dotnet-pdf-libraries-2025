# Migration Guide: MigraDoc → IronPDF

## Why Migrate to IronPDF?

IronPDF eliminates MigraDoc's complexity by allowing you to generate PDFs directly from HTML, CSS, and JavaScript—no need to learn a proprietary document model. This approach provides unlimited styling flexibility, matches modern web designs perfectly, and leverages your existing web development skills. The library offers superior rendering quality and significantly reduces development time.

## NuGet Package Changes

```xml
<!-- Remove MigraDoc -->
<PackageReference Include="PdfSharp-MigraDoc" Version="6.x.x" Remove="true" />

<!-- Add IronPDF -->
<PackageReference Include="IronPdf" Version="2024.x.x" />
```

## Namespace Mapping

| MigraDoc | IronPDF |
|----------|---------|
| `MigraDoc.DocumentObjectModel` | `IronPdf` |
| `MigraDoc.Rendering` | `IronPdf` |
| `PdfSharp.Pdf` | `IronPdf` |
| `MigraDoc.DocumentObjectModel.Tables` | N/A (Use HTML tables) |
| `MigraDoc.DocumentObjectModel.Shapes` | N/A (Use HTML/CSS) |

## API Mapping

| MigraDoc API | IronPDF API | Notes |
|--------------|-------------|-------|
| `Document doc = new Document()` | `ChromePdfRenderer renderer = new ChromePdfRenderer()` | No document object needed |
| `Section section = doc.AddSection()` | N/A | Use HTML structure |
| `Paragraph p = section.AddParagraph()` | N/A | Use `<p>` tags |
| `Table table = section.AddTable()` | N/A | Use `<table>` tags |
| `PdfDocumentRenderer.Save()` | `renderer.RenderHtmlAsPdf(html).SaveAs()` | Direct HTML rendering |
| `doc.DefaultPageSetup.PageFormat` | `renderer.RenderingOptions.PaperSize` | Page configuration |
| `Style style = doc.Styles["Heading1"]` | N/A | Use CSS styles |
| `AddImage()` | N/A | Use `<img>` tags or `data:` URIs |

## Code Examples

### Example 1: Simple Document with Text

**Before (MigraDoc):**

```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

Document document = new Document();
Section section = document.AddSection();

Paragraph paragraph = section.AddParagraph("Hello World");
paragraph.Format.Font.Size = 14;
paragraph.Format.Font.Bold = true;
paragraph.Format.Font.Color = Colors.Blue;

section.AddParagraph("This is a simple PDF document created with MigraDoc.");

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
    <h1 style='font-size: 14pt; font-weight: bold; color: blue;'>Hello World</h1>
    <p>This is a simple PDF document created with IronPDF.</p>
";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: Table Generation

**Before (MigraDoc):**

```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

Document document = new Document();
Section section = document.AddSection();

Table table = section.AddTable();
table.Borders.Width = 0.75;

Column column1 = table.AddColumn("3cm");
Column column2 = table.AddColumn("5cm");

Row headerRow = table.AddRow();
headerRow.Shading.Color = Colors.LightGray;
headerRow.Cells[0].AddParagraph("Name");
headerRow.Cells[1].AddParagraph("Email");

Row dataRow = table.AddRow();
dataRow.Cells[0].AddParagraph("John Doe");
dataRow.Cells[1].AddParagraph("john@example.com");

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
    <table style='border-collapse: collapse; width: 100%;'>
        <thead>
            <tr style='background-color: #d3d3d3;'>
                <th style='border: 1px solid black; padding: 8px; width: 3cm;'>Name</th>
                <th style='border: 1px solid black; padding: 8px; width: 5cm;'>Email</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td style='border: 1px solid black; padding: 8px;'>John Doe</td>
                <td style='border: 1px solid black; padding: 8px;'>john@example.com</td>
            </tr>
        </tbody>
    </table>
";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("table.pdf");
```

### Example 3: Headers, Footers, and Page Numbers

**Before (MigraDoc):**

```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

Document document = new Document();
Section section = document.AddSection();

// Header
Paragraph header = section.Headers.Primary.AddParagraph();
header.AddText("Company Report");
header.Format.Alignment = ParagraphAlignment.Center;

// Footer with page numbers
Paragraph footer = section.Footers.Primary.AddParagraph();
footer.AddText("Page ");
footer.AddPageField();
footer.AddText(" of ");
footer.AddNumPagesField();
footer.Format.Alignment = ParagraphAlignment.Right;

// Content
section.AddParagraph("Document content goes here.");

PdfDocumentRenderer renderer = new PdfDocumentRenderer();
renderer.Document = document;
renderer.RenderDocument();
renderer.PdfDocument.Save("document.pdf");
```

**After (IronPDF):**

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

string html = @"
    <div style='text-align: center; margin-bottom: 20px;'>
        <h2>Company Report</h2>
    </div>
    <p>Document content goes here.</p>
";

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align: center;'>Company Report</div>"
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align: right;'>Page {page} of {total-pages}</div>"
};

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("document.pdf");
```

## Common Gotchas

### 1. **CSS Must Be Inline or Embedded**
IronPDF requires CSS to be inline or in `<style>` tags within the HTML. External CSS files need special handling with absolute paths or base URLs.

```csharp
renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;
```

### 2. **Page Breaks**
MigraDoc's `section.AddPageBreak()` becomes CSS page breaks:

```html
<div style="page-break-after: always;"></div>
```

### 3. **Font Embedding**
MigraDoc automatically embeds fonts. With IronPDF, use standard web fonts or custom fonts via CSS `@font-face`:

```csharp
renderer.RenderingOptions.UseGpu = false; // For better font rendering compatibility
```

### 4. **Measurements**
MigraDoc uses `Unit` objects (cm, mm). IronPDF uses CSS units (px, pt, cm, mm, in). Be consistent with units in your CSS.

### 5. **Dynamic Content**
Instead of programmatically building documents, use string interpolation or templating engines (Razor, Scriban) to generate HTML:

```csharp
string html = $"<h1>{customerName}</h1><p>Order total: {total:C}</p>";
```

### 6. **Performance with Large Documents**
IronPDF renders via Chromium engine. For very large documents, consider:

```csharp
renderer.RenderingOptions.EnableJavaScript = false; // If JS not needed
renderer.RenderingOptions.Timeout = 120; // Increase timeout
```

### 7. **Licensing**
IronPDF requires a license key for production use:

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

## Additional Resources

- **Documentation:** https://ironpdf.com/docs/
- **Tutorials:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/docs/questions/html-to-pdf/
- **API Reference:** https://ironpdf.com/object-reference/api/