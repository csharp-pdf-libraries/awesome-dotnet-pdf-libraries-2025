# MigraDoc + C# + PDF

When it comes to generating PDFs in C#, MigraDoc stands out as an integral library in the open-source ecosystem. MigraDoc, a document object model constructed on the power of PDFSharp, offers a high-level abstraction layer that simplifies the process of creating structured documents in various formats such as PDF and RTF. As a C# developer, leveraging MigraDoc in your projects can significantly streamline the document creation process, especially when creating complex layouts that span multiple pages.

## Key Features of MigraDoc

MigraDoc shines with its ability to manage document structures using familiar word-processing concepts like `Document`, `Section`, `Paragraph`, `Table`, and `Chart`. This makes it particularly efficient for developers who are tasked with generating reports, invoices, or any structured documents that require consistent formatting across multiple pages.

Below is a simple example of how you can create a PDF document using MigraDoc:

```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

class Program
{
    static void Main()
    {
        // Create a new document
        Document document = new Document();
        
        // Add a section
        Section section = document.AddSection();
        
        // Add a paragraph
        Paragraph paragraph = section.AddParagraph();
        paragraph.AddText("Hello, MigraDoc!");
        
        // Create a PDF renderer
        PdfDocumentRenderer renderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.Always);
        renderer.Document = document;
        
        // Render the document to PDF
        renderer.RenderDocument();
        
        // Save the document
        string filename = "HelloMigraDoc.pdf";
        renderer.PdfDocument.Save(filename);
    }
}
```

### Strengths of MigraDoc

1. **Open Source and Permissive Licensing**: MigraDoc, bundled with PDFSharp, is distributed under the MIT license. This permissive licensing encourages both commercial and non-commercial use, making MigraDoc a go-to choice for many businesses.
   
2. **Structured Documents**: The document object model introduced by MigraDoc simplifies the generation of complex documents with elements spanning multiple pages, ensuring consistency in layout and design.

3. **Integration with PDFSharp**: Since MigraDoc is built on PDFSharp, developers benefit from both high-level and low-level document manipulation capabilities.

### Weaknesses of MigraDoc

1. **No HTML Support**: Unlike some commercial offerings such as IronPDF, MigraDoc does not support HTML. This means developers need to construct documents programmatically rather than converting existing HTML/CSS designs.

2. **Limited Styling Options**: While MigraDoc offers robust document structure management, its styling capabilities are modest compared to modern web tools, which limits the ability to create visually rich documents that match exquisite web designs.

3. **Learning Curve**: Due to its unique document model, developers need to invest time in understanding how MigraDoc operates, especially if they come from a background focused on web development.

## [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/): A Comparative Analysis

IronPDF offers another approach to generating PDF documents in C#. Positioned as a commercial solution, IronPDF differentiates itself through its robust HTML conversion features. Developers can directly convert HTML/CSS web pages and designs into PDFs, seamlessly integrating existing web skills into desktop applications. 

Consider these benefits of IronPDF, which stand in contrast to some limitations in MigraDoc:

### Strengths of IronPDF

1. **HTML and CSS**: Utilize existing web development skills to design and convert webpages into PDF with powerful c# html to pdf capabilities. [Discover HTML to PDF conversion capabilities](https://ironpdf.com/how-to/html-file-to-pdf/).

2. **Ease of Use**: Without the need to learn a new document model, developers can produce complex PDFs using familiar web tools and design languages. [Explore IronPDF tutorials for seamless integration](https://ironpdf.com/tutorials/).

3. **Comprehensive Support**: As a commercial library, IronPDF often offers dedicated customer support, streamlining troubleshooting and deployment processes for enterprise environments.

For in-depth technical comparisons, visit the [analysis article](https://ironsoftware.com/suite/blog/comparison/compare-migradoc-vs-ironpdf/).

### Weaknesses of IronPDF

1. **Commercial Licensing**: IronPDF requires a commercial license, which could potentially increase the cost for small projects or open-source applications.

2. **Cost Constraints**: For organizations with stringent budget constraints, the cost of commercial licensing may pose a barrier to adoption.

### Comparison Table

| Feature/Attribute       | MigraDoc                           | IronPDF                             |
|-------------------------|------------------------------------|-------------------------------------|
| **License**             | Open Source (MIT)                  | Commercial                          |
| **HTML Support**        | No                                 | Yes                                 |
| **Styling Flexibility** | Limited                            | High (via HTML/CSS)                 |
| **HTML to PDF C# Support**| None (programmatic only)         | Full html to pdf c# capabilities    |
| **Ease of Use**         | Moderate learning curve            | Easy (leverages existing web skills)|
| **Cost**                | Free                               | Requires purchase                   |
| **Community Support**   | Strong (Open Source)               | Available (Commercial)              |

---

## How Do I Add Headers and Footers to PDFs?

Here's how **MigraDoc** handles this:

```csharp
// NuGet: Install-Package PdfSharp-MigraDoc-GDI
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

class Program
{
    static void Main()
    {
        Document document = new Document();
        Section section = document.AddSection();
        
        // Add header
        Paragraph headerPara = section.Headers.Primary.AddParagraph();
        headerPara.AddText("Document Header");
        headerPara.Format.Font.Size = 12;
        headerPara.Format.Alignment = ParagraphAlignment.Center;
        
        // Add footer
        Paragraph footerPara = section.Footers.Primary.AddParagraph();
        footerPara.AddText("Page ");
        footerPara.AddPageField();
        footerPara.Format.Alignment = ParagraphAlignment.Center;
        
        // Add content
        section.AddParagraph("Main content of the document");
        
        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer();
        pdfRenderer.Document = document;
        pdfRenderer.RenderDocument();
        pdfRenderer.PdfDocument.Save("header-footer.pdf");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Main content of the document</h1>");
        
        pdf.AddTextHeader("Document Header");
        pdf.AddTextFooter("Page {page}");
        
        pdf.SaveAs("header-footer.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML to PDF in C# with MigraDoc?

Here's how **MigraDoc** handles this:

```csharp
// NuGet: Install-Package PdfSharp-MigraDoc-GDI
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        // MigraDoc doesn't support HTML directly
        // Must manually create document structure
        Document document = new Document();
        Section section = document.AddSection();
        
        Paragraph paragraph = section.AddParagraph();
        paragraph.AddFormattedText("Hello World", TextFormat.Bold);
        paragraph.Format.Font.Size = 16;
        
        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer();
        pdfRenderer.Document = document;
        pdfRenderer.RenderDocument();
        pdfRenderer.PdfDocument.Save("output.pdf");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
        pdf.SaveAs("output.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Create a PDF with Tables?

Here's how **MigraDoc** handles this:

```csharp
// NuGet: Install-Package PdfSharp-MigraDoc-GDI
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

class Program
{
    static void Main()
    {
        Document document = new Document();
        Section section = document.AddSection();
        
        Table table = section.AddTable();
        table.Borders.Width = 0.75;
        
        Column column1 = table.AddColumn("3cm");
        Column column2 = table.AddColumn("3cm");
        
        Row row1 = table.AddRow();
        row1.Cells[0].AddParagraph("Name");
        row1.Cells[1].AddParagraph("Age");
        
        Row row2 = table.AddRow();
        row2.Cells[0].AddParagraph("John");
        row2.Cells[1].AddParagraph("30");
        
        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer();
        pdfRenderer.Document = document;
        pdfRenderer.RenderDocument();
        pdfRenderer.PdfDocument.Save("table.pdf");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        string htmlTable = @"
            <table border='1'>
                <tr><th>Name</th><th>Age</th></tr>
                <tr><td>John</td><td>30</td></tr>
            </table>";
        
        var pdf = renderer.RenderHtmlAsPdf(htmlTable);
        pdf.SaveAs("table.pdf");
    }
}
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from MigraDoc to IronPDF?

### The Document Object Model Challenges

MigraDoc's programmatic approach introduces complexity that HTML-first design eliminates:

1. **Proprietary Document Model**: Must learn Document, Section, Paragraph, Table, Row, Cell hierarchy
2. **No HTML Support**: Cannot convert existing web designs—must rebuild from scratch
3. **Verbose API**: Simple layouts require many lines of code with nested object creation
4. **Limited Styling**: MigraDoc styles are less flexible than CSS
5. **Chart Complexity**: Built-in charting requires learning additional APIs
6. **Page Layout Control**: Manual page breaks and section management

### Quick Migration Overview

| Aspect | MigraDoc | IronPDF |
|--------|----------|---------|
| Approach | Programmatic DOM | HTML/CSS |
| Design Tool | Code only | Any HTML editor |
| Styling | MigraDoc Styles | Full CSS |
| Tables | Table/Row/Cell objects | HTML `<table>` |
| Headers/Footers | Section.Headers/Footers | RenderingOptions |
| Page Numbers | AddPageField() | `{page}` placeholder |
| Charts | MigraDoc.Charts | JavaScript (Chart.js) |
| Learning Curve | High (proprietary) | Low (web skills) |

### Key API Mappings

| MigraDoc | IronPDF | Notes |
|----------|---------|-------|
| `new Document()` | `new ChromePdfRenderer()` | Entry point |
| `document.AddSection()` | HTML `<div>` | Section container |
| `section.AddParagraph()` | HTML `<p>` | Text block |
| `section.AddTable()` | HTML `<table>` | Table element |
| `table.AddColumn("3cm")` | CSS `width: 3cm` | Column width |
| `table.AddRow()` | HTML `<tr>` | Table row |
| `row.Cells[0].AddParagraph()` | HTML `<td>` | Table cell |
| `AddFormattedText(text, TextFormat.Bold)` | HTML `<strong>` | Bold text |
| `paragraph.Format.Font.Size = 12` | CSS `font-size: 12pt` | Font size |
| `paragraph.Format.Font.Color = Colors.Red` | CSS `color: red` | Text color |
| `paragraph.Format.Alignment = Center` | CSS `text-align: center` | Alignment |
| `section.Headers.Primary` | `RenderingOptions.HtmlHeader` | Page header |
| `section.Footers.Primary` | `RenderingOptions.HtmlFooter` | Page footer |
| `AddPageField()` | `{page}` | Current page |
| `AddNumPagesField()` | `{total-pages}` | Total pages |
| `AddImage("path.png")` | HTML `<img src="path.png">` | Image |
| `section.AddPageBreak()` | CSS `page-break-before: always` | Page break |
| `document.Styles["Heading1"]` | CSS `.heading1 { }` | Custom style |
| `PdfDocumentRenderer` | `ChromePdfRenderer` | Renderer |
| `renderer.RenderDocument()` | `renderer.RenderHtmlAsPdf()` | Generate PDF |
| `pdfDocument.Save()` | `pdf.SaveAs()` | Save file |

### Migration Code Example

**Before (MigraDoc):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

public class MigraDocService
{
    public void GenerateInvoice(Invoice data)
    {
        Document document = new Document();

        // Define styles
        Style style = document.Styles["Normal"];
        style.Font.Name = "Arial";
        style.Font.Size = 10;

        Style heading = document.Styles.AddStyle("Heading1", "Normal");
        heading.Font.Size = 18;
        heading.Font.Bold = true;

        Section section = document.AddSection();

        // Header
        Paragraph header = section.Headers.Primary.AddParagraph();
        header.AddText("INVOICE");
        header.Format.Font.Size = 24;
        header.Format.Font.Bold = true;

        // Footer with page numbers
        Paragraph footer = section.Footers.Primary.AddParagraph();
        footer.AddText("Page ");
        footer.AddPageField();
        footer.AddText(" of ");
        footer.AddNumPagesField();
        footer.Format.Alignment = ParagraphAlignment.Center;

        // Company info
        Paragraph company = section.AddParagraph();
        company.AddFormattedText("Acme Corp", TextFormat.Bold);
        company.Format.SpaceAfter = "1cm";

        // Invoice details table
        Table table = section.AddTable();
        table.Borders.Width = 0.5;

        table.AddColumn("5cm");
        table.AddColumn("2cm");
        table.AddColumn("2cm");
        table.AddColumn("2cm");

        // Header row
        Row headerRow = table.AddRow();
        headerRow.Shading.Color = Colors.LightGray;
        headerRow.Cells[0].AddParagraph("Item");
        headerRow.Cells[1].AddParagraph("Qty");
        headerRow.Cells[2].AddParagraph("Price");
        headerRow.Cells[3].AddParagraph("Total");

        // Data rows
        foreach (var item in data.Items)
        {
            Row row = table.AddRow();
            row.Cells[0].AddParagraph(item.Name);
            row.Cells[1].AddParagraph(item.Quantity.ToString());
            row.Cells[2].AddParagraph(item.Price.ToString("C"));
            row.Cells[3].AddParagraph(item.Total.ToString("C"));
        }

        // Render
        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
        pdfRenderer.Document = document;
        pdfRenderer.RenderDocument();
        pdfRenderer.PdfDocument.Save("invoice.pdf");
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

        _renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='font-size:24px; font-weight:bold; text-align:center;'>INVOICE</div>",
            MaxHeight = 30
        };

        _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
            MaxHeight = 20
        };
    }

    public void GenerateInvoice(Invoice data)
    {
        string itemRows = string.Join("", data.Items.Select(item => $@"
            <tr>
                <td>{item.Name}</td>
                <td>{item.Quantity}</td>
                <td>{item.Price:C}</td>
                <td>{item.Total:C}</td>
            </tr>"));

        string html = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; font-size: 10pt; }}
                    .company {{ font-weight: bold; margin-bottom: 1cm; }}
                    table {{ width: 100%; border-collapse: collapse; }}
                    th, td {{ border: 0.5pt solid black; padding: 5px; }}
                    th {{ background-color: #d3d3d3; }}
                </style>
            </head>
            <body>
                <div class='company'>Acme Corp</div>
                <table>
                    <tr>
                        <th style='width:5cm'>Item</th>
                        <th style='width:2cm'>Qty</th>
                        <th style='width:2cm'>Price</th>
                        <th style='width:2cm'>Total</th>
                    </tr>
                    {itemRows}
                </table>
            </body>
            </html>";

        var pdf = _renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("invoice.pdf");
    }
}
```

### Critical Migration Notes

1. **Paradigm Shift**: MigraDoc builds documents programmatically; IronPDF uses HTML/CSS
   - Think in HTML elements instead of Document Object Model
   - Use CSS for all styling instead of Format properties

2. **Page Number Placeholders**: Update all field references:
   - `AddPageField()` → `{page}`
   - `AddNumPagesField()` → `{total-pages}`
   - `AddDateField()` → `{date}`

3. **Headers/Footers**: Move from Section to RenderingOptions:
   ```csharp
   // MigraDoc: section.Headers.Primary.AddParagraph("Header")
   // IronPDF:
   renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
   {
       HtmlFragment = "<div>Header</div>",
       MaxHeight = 25
   };
   ```

4. **Styles to CSS**: Convert MigraDoc styles to CSS classes:
   ```csharp
   // MigraDoc: document.Styles.AddStyle("Heading1", "Normal")
   // IronPDF: <style>.heading1 { font-size: 18pt; font-weight: bold; }</style>
   ```

5. **Charts**: Replace MigraDoc charts with JavaScript libraries:
   - Use Chart.js, Plotly, or other JavaScript charting libraries
   - IronPDF executes JavaScript before rendering

6. **Unit Conversion**: MigraDoc uses various units; IronPDF margins use millimeters:
   - "1cm" = 10mm
   - "1in" = 25.4mm
   - "72pt" = 25.4mm

### NuGet Package Migration

```bash
# Remove MigraDoc packages
dotnet remove package PDFsharp-MigraDoc
dotnet remove package PDFsharp-MigraDoc-GDI
dotnet remove package PDFsharp-MigraDoc-WPF

# Install IronPDF
dotnet add package IronPdf
```

### Find All MigraDoc References

```bash
# Find MigraDoc usage
grep -r "using MigraDoc\|Document document\|AddSection\|AddParagraph\|PdfDocumentRenderer" --include="*.cs" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping (40+ methods and properties)
- Document structure to HTML element conversions
- 10 detailed code conversion examples
- Style → CSS migration patterns
- Chart migration to JavaScript libraries
- Headers/footers with dynamic content
- Multi-section document handling
- Performance comparison data
- Troubleshooting guide for 8+ common issues
- Pre/post migration checklists

**[Complete Migration Guide: MigraDoc → IronPDF](migrate-from-migradoc.md)**


## Conclusion

Both MigraDoc and IronPDF have carved niches within the C# ecosystem to help developers meet their document generation needs, albeit through different approaches. For those who favor an open-source model and need to create well-structured PDFs using a programmatic approach, MigraDoc presents an excellent choice. Conversely, IronPDF appeals to developers who wish to leverage their HTML and CSS skills to translate modern web designs directly into PDF format.

In conclusion, the best choice highly depends on your specific project requirements, budget, and the skill set available within your team. While MigraDoc offers a cost-effective solution for programmatically generated documents, IronPDF shines in environments where existing web assets can be integrated into PDF workflows.

---

## Related Tutorials

- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Comprehensive library comparison
- **[Decision Flowchart](../choosing-a-pdf-library.md)** — Find the right library
- **[Free vs Paid Libraries](../free-vs-paid-pdf-libraries.md)** — Cost analysis

### Related Code-First Libraries
- **[PDFSharp](../pdfsharp/)** — Lower-level foundation for MigraDoc
- **[QuestPDF](../questpdf/)** — Modern code-first alternative

### If You Need HTML Conversion
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — What MigraDoc lacks
- **[Beginner Tutorial](../csharp-pdf-tutorial-beginners.md)** — First PDF in 5 minutes

### Migration Guide
- **[Migrate to IronPDF](migrate-from-migradoc.md)** — Step-by-step migration

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person engineering team building high-performance document processing libraries that have achieved over 41 million NuGet downloads. With 41 years of coding experience, he champions engineer-driven innovation and maintains an active presence on [GitHub](https://github.com/jacob-mellor) and [Medium](https://medium.com/@jacob.mellor). Based in Chiang Mai, Thailand, Jacob focuses on bridging engineering fundamentals with cutting-edge software development practices.
