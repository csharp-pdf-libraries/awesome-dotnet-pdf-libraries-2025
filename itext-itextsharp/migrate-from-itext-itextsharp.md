# How Do I Migrate from iText / iTextSharp to IronPDF in C#?

## Table of Contents
1. [Why Migrate from iText/iTextSharp](#why-migrate-from-itexitextsharp)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from iText/iTextSharp

### The AGPL License Trap

iText presents serious legal and business risks for commercial applications:

1. **AGPL Viral License**: If you use iText in a web application, the AGPL requires you to **open-source your ENTIRE application**—not just the PDF code, but your entire codebase
2. **No Perpetual License**: iText has eliminated perpetual licensing, forcing annual subscription renewals
3. **pdfHTML Add-On Cost**: HTML-to-PDF requires the pdfHTML add-on, sold separately at additional cost
4. **Complex Licensing Audits**: Enterprise deployments face licensing complexity and audit risk
5. **Programmatic-Only API**: Requires manual low-level PDF construction with `Paragraph`, `Table`, `Cell`
6. **No Modern Web Rendering**: Even with pdfHTML, complex CSS/JavaScript requires significant effort

### The IronPDF Advantage

| Feature | iText 7 / iTextSharp | IronPDF |
|---------|---------------------|---------|
| License | AGPL (viral) or expensive subscription | Commercial, perpetual option |
| HTML-to-PDF | Separate pdfHTML add-on | Built-in Chromium renderer |
| CSS Support | Basic CSS | Full CSS3, Flexbox, Grid |
| JavaScript | None | Full execution |
| API Paradigm | Programmatic (Paragraph, Table, Cell) | HTML-first with CSS |
| Learning Curve | Steep (PDF coordinate system) | Web developer friendly |
| Open Source Risk | Must open-source web apps | No viral requirements |
| Pricing Model | Subscription only | Perpetual or subscription |

### Migration Benefits

- **Eliminate AGPL Risk**: Keep your proprietary code closed-source
- **Simplify PDF Creation**: Use HTML/CSS instead of programmatic construction
- **Modern Rendering**: Chromium engine handles any modern web content
- **Reduce Cost**: One license includes HTML-to-PDF (no pdfHTML add-on)
- **Perpetual Licensing**: Option for one-time purchase

These advantages are illustrated with working code throughout the [full migration reference](https://ironpdf.com/blog/migration-guides/migrate-from-itext-to-ironpdf/).

---

## Before You Start

### Prerequisites

1. **.NET Environment**: .NET Framework 4.6.2+ or .NET Core 3.1+ / .NET 5+
2. **NuGet Access**: Ability to install NuGet packages
3. **IronPDF License**: Free trial or purchased license key

### Installation

```bash
# Remove iText packages
dotnet remove package itext7
dotnet remove package itext7.pdfhtml
dotnet remove package itextsharp

# Install IronPDF
dotnet add package IronPdf
```

### License Configuration

```csharp
// Add at application startup (Program.cs or Startup.cs)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Identify iText Usage

```bash
# Find all iText references
grep -r "using iText\|using iTextSharp" --include="*.cs" .
grep -r "PdfWriter\|PdfDocument\|Document\|Paragraph\|Table\|Cell" --include="*.cs" .
grep -r "HtmlConverter\|ConverterProperties" --include="*.cs" .
```

---

## Quick Start Migration

### Minimal Change Example

**Before (iText 7):**
```csharp
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

public class ItextPdfService
{
    public byte[] CreateReport(ReportData data)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var writer = new PdfWriter(memoryStream))
            using (var pdfDoc = new PdfDocument(writer))
            using (var document = new Document(pdfDoc))
            {
                // Header
                var header = new Paragraph(data.Title)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(24)
                    .SetBold();
                document.Add(header);

                // Table
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2, 1 }))
                    .UseAllAvailableWidth();

                // Header cells
                table.AddHeaderCell(new Cell().Add(new Paragraph("ID")));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Name")));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Value")));

                // Data rows
                foreach (var item in data.Items)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.Id.ToString())));
                    table.AddCell(new Cell().Add(new Paragraph(item.Name)));
                    table.AddCell(new Cell().Add(new Paragraph(item.Value.ToString("C"))));
                }

                document.Add(table);
            }

            return memoryStream.ToArray();
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
    }

    public byte[] CreateReport(ReportData data)
    {
        // Use HTML/CSS instead of programmatic construction
        string html = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 20px; }}
                    h1 {{ text-align: center; font-size: 24px; }}
                    table {{ width: 100%; border-collapse: collapse; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                    th {{ background-color: #4CAF50; color: white; }}
                    tr:nth-child(even) {{ background-color: #f2f2f2; }}
                </style>
            </head>
            <body>
                <h1>{data.Title}</h1>
                <table>
                    <tr><th>ID</th><th>Name</th><th>Value</th></tr>
                    {string.Join("", data.Items.Select(item => $@"
                        <tr>
                            <td>{item.Id}</td>
                            <td>{item.Name}</td>
                            <td>{item.Value:C}</td>
                        </tr>"))}
                </table>
            </body>
            </html>";

        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

---

## Complete API Reference

### Namespace Mappings

| iText 7 Namespace | iTextSharp Namespace | IronPDF Equivalent |
|-------------------|---------------------|-------------------|
| `iText.Kernel.Pdf` | `iTextSharp.text.pdf` | `IronPdf` |
| `iText.Layout` | `iTextSharp.text` | `IronPdf` |
| `iText.Layout.Element` | `iTextSharp.text` | Use HTML elements |
| `iText.Layout.Properties` | `iTextSharp.text` | Use CSS |
| `iText.Html2Pdf` | N/A | `IronPdf` (built-in) |
| `iText.IO.Image` | `iTextSharp.text` | Use HTML `<img>` |
| `iText.Kernel.Font` | `iTextSharp.text` | Use CSS fonts |
| `iText.Kernel.Colors` | `iTextSharp.text` | Use CSS colors |
| `iText.Kernel.Geom` | `iTextSharp.text` | Use CSS positioning |

### Class Mappings

| iText 7 Class | iTextSharp Class | IronPDF Equivalent |
|---------------|-----------------|-------------------|
| `PdfWriter` | `PdfWriter` | `ChromePdfRenderer` |
| `PdfDocument` | `Document` | `PdfDocument` |
| `Document` | `Document` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `Paragraph` | `Paragraph` | HTML `<p>`, `<h1>`, etc. |
| `Table` | `PdfPTable` | HTML `<table>` |
| `Cell` | `PdfPCell` | HTML `<td>`, `<th>` |
| `Image` | `Image` | HTML `<img>` |
| `List` | `List` | HTML `<ul>`, `<ol>` |
| `ListItem` | `ListItem` | HTML `<li>` |
| `PdfReader` | `PdfReader` | `PdfDocument.FromFile()` |
| `PdfMerger` | N/A | `PdfDocument.Merge()` |
| `PdfTextExtractor` | `PdfTextExtractor` | `pdf.ExtractAllText()` |

### Method Mappings

| Task | iText 7 | iTextSharp | IronPDF |
|------|---------|-----------|---------|
| Create PDF from HTML | `HtmlConverter.ConvertToPdf()` | N/A | `renderer.RenderHtmlAsPdf()` |
| Create PDF from URL | Download HTML + convert | N/A | `renderer.RenderUrlAsPdf()` |
| Create PDF from file | `HtmlConverter.ConvertToPdf(File.ReadAllText())` | N/A | `renderer.RenderHtmlFileAsPdf()` |
| Save to file | `document.Close()` (via stream) | `document.Close()` | `pdf.SaveAs()` |
| Save to bytes | `memoryStream.ToArray()` | `memoryStream.ToArray()` | `pdf.BinaryData` |
| Open existing PDF | `new PdfDocument(new PdfReader(path))` | `new PdfReader(path)` | `PdfDocument.FromFile()` |
| Open from bytes | `new PdfDocument(new PdfReader(new MemoryStream(bytes)))` | Similar | `PdfDocument.FromBytes()` |
| Merge PDFs | `PdfMerger.Merge()` | `PdfCopy` | `PdfDocument.Merge()` |
| Split PDF | Manual page copy | Manual | `pdf.CopyPages()` |
| Add text | `document.Add(new Paragraph())` | `document.Add(new Paragraph())` | Include in HTML |
| Add image | `document.Add(new Image())` | `document.Add(new Image())` | HTML `<img src="">` |
| Add table | `document.Add(new Table())` | `document.Add(new PdfPTable())` | HTML `<table>` |
| Set margins | `new Document(pdf, PageSize.A4).SetMargins()` | `document.SetMargins()` | `RenderingOptions.Margin*` |
| Set page size | `new Document(pdf, PageSize.A4)` | `new Document(PageSize.A4)` | `RenderingOptions.PaperSize` |
| Add header/footer | Event handlers | Event handlers | `RenderingOptions.HtmlHeader/Footer` |
| Set password | `WriterProperties.SetStandardEncryption()` | `writer.SetEncryption()` | `pdf.SecuritySettings.Password` |
| Get page count | `pdfDoc.GetNumberOfPages()` | `reader.NumberOfPages` | `pdf.PageCount` |
| Extract text | `PdfTextExtractor.GetTextFromPage()` | `PdfTextExtractor` | `pdf.ExtractAllText()` |
| Add watermark | Overlay canvas drawing | Similar | `pdf.ApplyWatermark()` |
| Set metadata | `pdfDoc.GetDocumentInfo().SetTitle()` | `document.AddTitle()` | `pdf.MetaData.Title` |

### Property Mappings

| iText 7 Property | IronPDF Equivalent |
|------------------|-------------------|
| `TextAlignment.CENTER` | CSS `text-align: center` |
| `TextAlignment.LEFT` | CSS `text-align: left` |
| `TextAlignment.RIGHT` | CSS `text-align: right` |
| `TextAlignment.JUSTIFIED` | CSS `text-align: justify` |
| `SetFontSize(12)` | CSS `font-size: 12px` |
| `SetBold()` | CSS `font-weight: bold` |
| `SetItalic()` | CSS `font-style: italic` |
| `SetUnderline()` | CSS `text-decoration: underline` |
| `SetBackgroundColor()` | CSS `background-color` |
| `SetBorder()` | CSS `border` |
| `SetPadding()` | CSS `padding` |
| `SetMargin()` | CSS `margin` |
| `SetWidth()` | CSS `width` |
| `SetHeight()` | CSS `height` |

---

## Code Migration Examples

### Example 1: HTML to PDF Conversion

**Before (iText 7 with pdfHTML):**
```csharp
using iText.Html2Pdf;
using iText.Kernel.Pdf;
using System.IO;

public class ItextHtmlService
{
    public byte[] ConvertHtmlToPdf(string html)
    {
        using (var memoryStream = new MemoryStream())
        {
            var properties = new ConverterProperties();
            // Note: pdfHTML is a separate purchase!
            HtmlConverter.ConvertToPdf(html, memoryStream, properties);
            return memoryStream.ToArray();
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    public byte[] ConvertHtmlToPdf(string html)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }

    // Async version for web applications
    public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}
```

### Example 2: URL to PDF

**Before (iText 7):**
```csharp
using iText.Html2Pdf;
using iText.Kernel.Pdf;
using System.IO;
using System.Net.Http;

public class ItextUrlService
{
    public async Task<byte[]> ConvertUrlToPdfAsync(string url)
    {
        // Must download HTML first
        using (var httpClient = new HttpClient())
        {
            string html = await httpClient.GetStringAsync(url);

            using (var memoryStream = new MemoryStream())
            {
                var properties = new ConverterProperties();
                properties.SetBaseUri(url);  // For relative resources
                HtmlConverter.ConvertToPdf(html, memoryStream, properties);
                return memoryStream.ToArray();
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    public byte[] ConvertUrlToPdf(string url)
    {
        var renderer = new ChromePdfRenderer();
        // Direct URL rendering with full JavaScript execution
        var pdf = renderer.RenderUrlAsPdf(url);
        return pdf.BinaryData;
    }
}
```

### Example 3: Create PDF with Tables

**Before (iText 7):**
```csharp
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;

public byte[] CreateTablePdf(List<Product> products)
{
    using (var ms = new MemoryStream())
    {
        using (var writer = new PdfWriter(ms))
        using (var pdf = new PdfDocument(writer))
        using (var document = new Document(pdf))
        {
            // Create table with 4 columns
            var table = new Table(UnitValue.CreatePercentArray(4)).UseAllAvailableWidth();

            // Header row
            Cell header1 = new Cell().Add(new Paragraph("ID"))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER);
            Cell header2 = new Cell().Add(new Paragraph("Name"))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            Cell header3 = new Cell().Add(new Paragraph("Price"))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.RIGHT);
            Cell header4 = new Cell().Add(new Paragraph("Stock"))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER);

            table.AddHeaderCell(header1);
            table.AddHeaderCell(header2);
            table.AddHeaderCell(header3);
            table.AddHeaderCell(header4);

            // Data rows
            foreach (var product in products)
            {
                table.AddCell(new Cell().Add(new Paragraph(product.Id.ToString()))
                    .SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell().Add(new Paragraph(product.Name)));
                table.AddCell(new Cell().Add(new Paragraph(product.Price.ToString("C")))
                    .SetTextAlignment(TextAlignment.RIGHT));
                table.AddCell(new Cell().Add(new Paragraph(product.Stock.ToString()))
                    .SetTextAlignment(TextAlignment.CENTER));
            }

            document.Add(table);
        }
        return ms.ToArray();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateTablePdf(List<Product> products)
{
    string rows = string.Join("", products.Select(p => $@"
        <tr>
            <td style='text-align:center;'>{p.Id}</td>
            <td>{p.Name}</td>
            <td style='text-align:right;'>{p.Price:C}</td>
            <td style='text-align:center;'>{p.Stock}</td>
        </tr>"));

    string html = $@"
        <html>
        <head>
            <style>
                table {{ width: 100%; border-collapse: collapse; font-family: Arial; }}
                th {{ background-color: #d3d3d3; padding: 10px; text-align: left; }}
                td {{ padding: 8px; border-bottom: 1px solid #ddd; }}
                tr:hover {{ background-color: #f5f5f5; }}
            </style>
        </head>
        <body>
            <table>
                <tr>
                    <th style='text-align:center;'>ID</th>
                    <th>Name</th>
                    <th style='text-align:right;'>Price</th>
                    <th style='text-align:center;'>Stock</th>
                </tr>
                {rows}
            </table>
        </body>
        </html>";

    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Example 4: Merge Multiple PDFs

**Before (iText 7):**
```csharp
using iText.Kernel.Pdf;
using iText.Kernel.Utils;

public void MergePdfs(string[] inputFiles, string outputFile)
{
    using (var writer = new PdfWriter(outputFile))
    using (var pdfDoc = new PdfDocument(writer))
    {
        var merger = new PdfMerger(pdfDoc);

        foreach (string file in inputFiles)
        {
            using (var reader = new PdfReader(file))
            using (var sourceDoc = new PdfDocument(reader))
            {
                merger.Merge(sourceDoc, 1, sourceDoc.GetNumberOfPages());
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void MergePdfs(string[] inputFiles, string outputFile)
{
    var pdfs = inputFiles.Select(f => PdfDocument.FromFile(f)).ToList();
    var merged = PdfDocument.Merge(pdfs);
    merged.SaveAs(outputFile);
}

// Or even simpler with a one-liner
public void MergePdfsOneLiner(string[] inputFiles, string outputFile)
{
    PdfDocument.Merge(inputFiles.Select(PdfDocument.FromFile).ToArray())
               .SaveAs(outputFile);
}
```

### Example 5: Add Headers and Footers

**Before (iText 7):**
```csharp
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Events;
using iText.Kernel.Geom;

public class HeaderFooterEventHandler : IEventHandler
{
    public void HandleEvent(Event @event)
    {
        PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
        PdfDocument pdfDoc = docEvent.GetDocument();
        PdfPage page = docEvent.GetPage();
        int pageNumber = pdfDoc.GetPageNumber(page);
        Rectangle pageSize = page.GetPageSize();

        PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);

        // Header
        new Canvas(pdfCanvas, pageSize)
            .ShowTextAligned("Company Report",
                pageSize.GetWidth() / 2,
                pageSize.GetTop() - 30,
                TextAlignment.CENTER);

        // Footer with page number
        new Canvas(pdfCanvas, pageSize)
            .ShowTextAligned($"Page {pageNumber}",
                pageSize.GetWidth() / 2,
                30,
                TextAlignment.CENTER);
    }
}

// Usage
pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, new HeaderFooterEventHandler());
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center; font-size:12px;'>Company Report</div>",
    MaxHeight = 25
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center; font-size:10px;'>Page {page} of {total-pages}</div>",
    MaxHeight = 25
};

var pdf = renderer.RenderHtmlAsPdf(html);
```

### Example 6: Password Protection

**Before (iText 7):**
```csharp
using iText.Kernel.Pdf;

public void CreateProtectedPdf(string html, string outputPath, string password)
{
    var writerProperties = new WriterProperties()
        .SetStandardEncryption(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(password),
            EncryptionConstants.ALLOW_PRINTING,
            EncryptionConstants.ENCRYPTION_AES_256);

    using (var writer = new PdfWriter(outputPath, writerProperties))
    using (var pdfDoc = new PdfDocument(writer))
    using (var document = new Document(pdfDoc))
    {
        // Add content...
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreateProtectedPdf(string html, string outputPath, string password)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    pdf.SecuritySettings.UserPassword = password;
    pdf.SecuritySettings.OwnerPassword = password;
    pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;

    pdf.SaveAs(outputPath);
}
```

### Example 7: Extract Text from PDF

**Before (iText 7):**
```csharp
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

public string ExtractText(string pdfPath)
{
    var sb = new StringBuilder();

    using (var reader = new PdfReader(pdfPath))
    using (var pdfDoc = new PdfDocument(reader))
    {
        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i));
            sb.AppendLine(pageText);
        }
    }

    return sb.ToString();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public string ExtractText(string pdfPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    return pdf.ExtractAllText();
}

// Or extract from specific pages
public string ExtractTextFromPages(string pdfPath, int startPage, int endPage)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    return pdf.ExtractTextFromPages(
        Enumerable.Range(startPage, endPage - startPage + 1));
}
```

### Example 8: Add Watermark

**Before (iText 7):**
```csharp
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Pdf.Extgstate;

public void AddWatermark(string inputPath, string outputPath, string watermarkText)
{
    using (var reader = new PdfReader(inputPath))
    using (var writer = new PdfWriter(outputPath))
    using (var pdfDoc = new PdfDocument(reader, writer))
    {
        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var gs = new PdfExtGState().SetFillOpacity(0.3f);

        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            var page = pdfDoc.GetPage(i);
            var pageSize = page.GetPageSize();
            var canvas = new PdfCanvas(page);

            canvas.SaveState()
                  .SetExtGState(gs)
                  .BeginText()
                  .SetFontAndSize(font, 60)
                  .SetTextMatrix(
                      (float)Math.Cos(Math.PI / 4), (float)Math.Sin(Math.PI / 4),
                      -(float)Math.Sin(Math.PI / 4), (float)Math.Cos(Math.PI / 4),
                      pageSize.GetWidth() / 4, pageSize.GetHeight() / 2)
                  .ShowText(watermarkText)
                  .EndText()
                  .RestoreState();
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void AddWatermark(string inputPath, string outputPath, string watermarkText)
{
    var pdf = PdfDocument.FromFile(inputPath);

    pdf.ApplyWatermark(
        $"<div style='font-size:60px; color:gray; opacity:0.3; " +
        $"transform:rotate(-45deg);'>{watermarkText}</div>",
        opacity: 30,
        rotation: -45);

    pdf.SaveAs(outputPath);
}
```

### Example 9: Set Document Metadata

**Before (iText 7):**
```csharp
using iText.Kernel.Pdf;

public void SetMetadata(string pdfPath)
{
    using (var reader = new PdfReader(pdfPath))
    using (var writer = new PdfWriter(pdfPath + ".tmp"))
    using (var pdfDoc = new PdfDocument(reader, writer))
    {
        var info = pdfDoc.GetDocumentInfo();
        info.SetTitle("Annual Report 2025");
        info.SetAuthor("Jane Smith");
        info.SetSubject("Financial Performance");
        info.SetKeywords("finance, annual, report");
        info.SetCreator("MyApp v2.0");
    }

    File.Move(pdfPath + ".tmp", pdfPath, true);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void SetMetadata(string pdfPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    pdf.MetaData.Title = "Annual Report 2025";
    pdf.MetaData.Author = "Jane Smith";
    pdf.MetaData.Subject = "Financial Performance";
    pdf.MetaData.Keywords = "finance, annual, report";
    pdf.MetaData.Creator = "MyApp v2.0";

    pdf.SaveAs(pdfPath);
}
```

### Example 10: Complete Invoice Service Migration

**Before (iText 7):**
```csharp
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Colors;

public class ItextInvoiceService
{
    public byte[] GenerateInvoice(Invoice invoice)
    {
        using (var ms = new MemoryStream())
        {
            using (var writer = new PdfWriter(ms))
            using (var pdfDoc = new PdfDocument(writer))
            using (var document = new Document(pdfDoc))
            {
                // Logo
                if (File.Exists("logo.png"))
                {
                    var logo = new Image(ImageDataFactory.Create("logo.png"))
                        .SetWidth(100);
                    document.Add(logo);
                }

                // Title
                document.Add(new Paragraph("INVOICE")
                    .SetFontSize(28)
                    .SetBold()
                    .SetTextAlignment(TextAlignment.RIGHT));

                // Invoice details
                document.Add(new Paragraph($"Invoice #: {invoice.Number}")
                    .SetTextAlignment(TextAlignment.RIGHT));
                document.Add(new Paragraph($"Date: {invoice.Date:yyyy-MM-dd}")
                    .SetTextAlignment(TextAlignment.RIGHT));

                document.Add(new Paragraph("\n"));

                // Customer info
                document.Add(new Paragraph("Bill To:")
                    .SetBold());
                document.Add(new Paragraph(invoice.CustomerName));
                document.Add(new Paragraph(invoice.CustomerAddress));

                document.Add(new Paragraph("\n"));

                // Items table
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 4, 1, 1, 1 }))
                    .UseAllAvailableWidth();

                // Headers
                table.AddHeaderCell(CreateHeaderCell("Description"));
                table.AddHeaderCell(CreateHeaderCell("Qty"));
                table.AddHeaderCell(CreateHeaderCell("Price"));
                table.AddHeaderCell(CreateHeaderCell("Total"));

                // Items
                foreach (var item in invoice.Items)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.Description)));
                    table.AddCell(new Cell().Add(new Paragraph(item.Quantity.ToString()))
                        .SetTextAlignment(TextAlignment.CENTER));
                    table.AddCell(new Cell().Add(new Paragraph(item.UnitPrice.ToString("C")))
                        .SetTextAlignment(TextAlignment.RIGHT));
                    table.AddCell(new Cell().Add(new Paragraph(item.Total.ToString("C")))
                        .SetTextAlignment(TextAlignment.RIGHT));
                }

                // Total row
                table.AddCell(new Cell(1, 3).Add(new Paragraph("Total"))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.RIGHT));
                table.AddCell(new Cell().Add(new Paragraph(invoice.Total.ToString("C")))
                    .SetBold()
                    .SetTextAlignment(TextAlignment.RIGHT));

                document.Add(table);
            }

            return ms.ToArray();
        }
    }

    private Cell CreateHeaderCell(string text)
    {
        return new Cell()
            .Add(new Paragraph(text))
            .SetBackgroundColor(new DeviceRgb(51, 51, 51))
            .SetFontColor(ColorConstants.WHITE)
            .SetBold();
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

    public byte[] GenerateInvoice(Invoice invoice)
    {
        string itemRows = string.Join("", invoice.Items.Select(item => $@"
            <tr>
                <td>{item.Description}</td>
                <td class='center'>{item.Quantity}</td>
                <td class='right'>{item.UnitPrice:C}</td>
                <td class='right'>{item.Total:C}</td>
            </tr>"));

        string html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 40px; }}
                    .header {{ display: flex; justify-content: space-between; }}
                    .logo {{ width: 100px; }}
                    .invoice-title {{ font-size: 28px; font-weight: bold; }}
                    .invoice-details {{ text-align: right; margin-bottom: 30px; }}
                    .customer {{ margin-bottom: 30px; }}
                    .customer strong {{ display: block; margin-bottom: 5px; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    th {{ background-color: #333; color: white; padding: 10px; text-align: left; }}
                    td {{ padding: 10px; border-bottom: 1px solid #ddd; }}
                    .center {{ text-align: center; }}
                    .right {{ text-align: right; }}
                    .total-row {{ font-weight: bold; background-color: #f9f9f9; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <img src='logo.png' class='logo' />
                    <div class='invoice-details'>
                        <div class='invoice-title'>INVOICE</div>
                        <div>Invoice #: {invoice.Number}</div>
                        <div>Date: {invoice.Date:yyyy-MM-dd}</div>
                    </div>
                </div>

                <div class='customer'>
                    <strong>Bill To:</strong>
                    {invoice.CustomerName}<br/>
                    {invoice.CustomerAddress}
                </div>

                <table>
                    <tr>
                        <th>Description</th>
                        <th class='center'>Qty</th>
                        <th class='right'>Price</th>
                        <th class='right'>Total</th>
                    </tr>
                    {itemRows}
                    <tr class='total-row'>
                        <td colspan='3' class='right'>Total</td>
                        <td class='right'>{invoice.Total:C}</td>
                    </tr>
                </table>
            </body>
            </html>";

        return _renderer.RenderHtmlAsPdf(html).BinaryData;
    }
}
```

---

## Advanced Scenarios

### Converting from Programmatic to HTML-First

The key paradigm shift is from **building PDFs element by element** to **designing with HTML/CSS**:

```csharp
// iText approach: Build PDF programmatically
document.Add(new Paragraph("Title").SetBold().SetFontSize(24));
document.Add(new Paragraph("Content here..."));

// IronPDF approach: Design with HTML/CSS
string html = @"
    <style>
        h1 { font-weight: bold; font-size: 24px; }
    </style>
    <h1>Title</h1>
    <p>Content here...</p>";
renderer.RenderHtmlAsPdf(html);
```

### Using Razor Templates

For complex documents, use Razor templates:

```csharp
// Install RazorLight or use ASP.NET Core views
var engine = new RazorLightEngineBuilder()
    .UseMemoryCachingProvider()
    .Build();

string html = await engine.CompileRenderAsync("Invoice.cshtml", invoice);
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Handling Complex Layouts

```csharp
// iText: Complex coordinate-based positioning
// IronPDF: Use CSS Grid or Flexbox

string html = @"
    <style>
        .container { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
        .full-width { grid-column: span 2; }
    </style>
    <div class='container'>
        <div>Left Column</div>
        <div>Right Column</div>
        <div class='full-width'>Full Width Section</div>
    </div>";
```

---

## Performance Considerations

### Startup Time

| Aspect | iText 7 | IronPDF |
|--------|---------|---------|
| First render | Fast | 1-3s (Chromium init) |
| Subsequent renders | Fast | Fast |
| Memory footprint | Lower | Higher (Chromium) |

### Optimization Tips

1. **Reuse Renderer**: Create `ChromePdfRenderer` once at startup
2. **Warm Up**: Render a blank PDF during app initialization
3. **Async Methods**: Use `RenderHtmlAsPdfAsync()` in web apps
4. **Template Caching**: Cache compiled Razor templates

```csharp
public class OptimizedPdfService
{
    private readonly ChromePdfRenderer _renderer;
    private static bool _warmedUp;

    public OptimizedPdfService()
    {
        _renderer = new ChromePdfRenderer();

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

### Issue 1: AGPL License Compliance Error

**Cause**: iText AGPL requires open-sourcing your application
**IronPDF Solution**: Commercial license with no viral requirements

```csharp
// Remove all iText references
// IronPDF commercial license allows proprietary code
```

### Issue 2: pdfHTML Add-On Missing

**Cause**: HTML conversion requires separate pdfHTML purchase
**IronPDF Solution**: HTML-to-PDF built-in

```csharp
// IronPDF includes Chromium-based HTML rendering
var pdf = renderer.RenderHtmlAsPdf(html);  // Just works!
```

### Issue 3: Complex CSS Not Rendering

**Cause**: iText's pdfHTML has limited CSS support
**IronPDF Solution**: Full CSS3 with Chromium

```csharp
// Flexbox, Grid, animations - all work!
string html = "<div style='display:flex; gap:10px;'>...</div>";
```

### Issue 4: JavaScript Not Executing

**Cause**: iText cannot execute JavaScript
**IronPDF Solution**: Full JavaScript execution

```csharp
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.RenderDelay = 500;
```

### Issue 5: PDF Coordinate System Confusion

**Cause**: iText uses PDF coordinates (bottom-left origin)
**IronPDF Solution**: Use standard web coordinates via CSS

```csharp
// Forget PDF coordinates - use CSS!
// position: absolute; top: 100px; left: 50px;
```

### Issue 6: Manual Font Management

**Cause**: iText requires explicit font registration
**IronPDF Solution**: Automatic font handling

```csharp
// IronPDF automatically detects system fonts
// For web fonts, use @font-face in CSS
```

### Issue 7: Memory Issues with Large Documents

**IronPDF Solution**: Process in chunks

```csharp
// For very large documents, render sections separately and merge
var sections = largeData.Chunk(100);
var pdfs = sections.Select(chunk =>
    renderer.RenderHtmlAsPdf(GenerateHtml(chunk)));
var merged = PdfDocument.Merge(pdfs);
```

### Issue 8: Async Pattern Mismatch

**Cause**: iText is mostly synchronous
**IronPDF Solution**: Full async support

```csharp
public async Task<byte[]> GeneratePdfAsync(string html)
{
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit AGPL license compliance status**
  ```csharp
  // Check your project for iText usage
  // If you're shipping proprietary software with iText AGPL,
  // you may be in license violation unless you purchased commercial license

  // IronPDF solution: Commercial license with no viral requirements
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** iText 7's AGPL requires open-sourcing your entire application if distributed—a major legal risk for proprietary software.

- [ ] **Document current iText version (iText 7 vs iTextSharp 5.x)**
  ```bash
  # Check installed version
  dotnet list package | grep -i itext

  # iText 7: Uses iText.Kernel, iText.Layout namespaces
  # iTextSharp 5.x: Uses iTextSharp.text namespace (legacy)
  ```
  **Why:** API differences between versions affect migration strategy.

- [ ] **List all PDF operations used**
  ```csharp
  // Common iText operations and IronPDF equivalents:
  // PdfWriter + Document → ChromePdfRenderer.RenderHtmlAsPdf()
  // HtmlConverter.ConvertToPdf() → renderer.RenderHtmlAsPdf()
  // PdfMerger → PdfDocument.Merge()
  // PdfReader + stamper → PdfDocument.FromFile() + manipulation
  // Paragraph, Table, Cell → HTML <p>, <table>, <td>
  ```
  **Why:** Different operations have different migration paths.

- [ ] **Plan HTML template strategy**
  ```csharp
  // Before: iText programmatic construction
  new Paragraph("Hello")
      .SetFont(PdfFontFactory.CreateFont())
      .SetFontSize(12);

  // After: IronPDF with HTML templates
  // Option 1: Inline HTML
  var html = "<p style='font-size: 12pt;'>Hello</p>";

  // Option 2: Razor templates (recommended for complex documents)
  // Option 3: External HTML files with CSS
  ```
  **Why:** Moving from programmatic construction to HTML templates is the key paradigm shift.

### Package Changes

- [ ] **Remove iText NuGet packages**
  ```bash
  # Remove all iText packages
  dotnet remove package itext7
  dotnet remove package itext7.pdfhtml
  dotnet remove package itext7.bouncy-castle-adapter
  dotnet remove package itextsharp  # If using legacy version
  ```
  **Why:** Clean removal prevents conflicts and clarifies dependencies.

- [ ] **Install IronPDF**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Single package provides complete PDF functionality.

- [ ] **Update namespace imports**
  ```csharp
  // Before
  using iText.Kernel.Pdf;
  using iText.Layout;
  using iText.Layout.Element;
  using iText.Html2pdf;

  // After
  using IronPdf;
  ```
  **Why:** Clean up unused iText references.

### Code Changes

- [ ] **Add license key configuration**
  ```csharp
  // Program.cs or Startup.cs
  public static void Main(string[] args)
  {
      // Set license before any IronPDF operations
      IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

      // Rest of application startup...
  }
  ```
  **Why:** Required for production; best placed in startup code.

- [ ] **Replace PdfWriter/Document pattern**
  ```csharp
  // Before (iText)
  using (var writer = new PdfWriter("output.pdf"))
  using (var pdf = new PdfDocument(writer))
  using (var document = new Document(pdf))
  {
      document.Add(new Paragraph("Hello World"));
  }

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf("<p>Hello World</p>");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF's HTML-first approach eliminates complex object construction.

- [ ] **Convert Paragraph/Table/Cell to HTML**
  ```csharp
  // Before (iText)
  var table = new Table(3);
  table.AddHeaderCell(new Cell().Add(new Paragraph("Product")));
  table.AddHeaderCell(new Cell().Add(new Paragraph("Qty")));
  table.AddHeaderCell(new Cell().Add(new Paragraph("Price")));
  table.AddCell(new Cell().Add(new Paragraph("Widget")));
  table.AddCell(new Cell().Add(new Paragraph("10")));
  table.AddCell(new Cell().Add(new Paragraph("$99.00")));
  document.Add(table);

  // After (IronPDF)
  var html = @"
  <table style='border-collapse: collapse; width: 100%;'>
      <tr><th>Product</th><th>Qty</th><th>Price</th></tr>
      <tr><td>Widget</td><td>10</td><td>$99.00</td></tr>
  </table>";
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** HTML provides the same layout capabilities with simpler syntax.

- [ ] **Replace HtmlConverter.ConvertToPdf()**
  ```csharp
  // Before (iText pdfHTML)
  HtmlConverter.ConvertToPdf(new FileStream("input.html", FileMode.Open),
                             new FileStream("output.pdf", FileMode.Create));

  // After (IronPDF)
  var pdf = renderer.RenderHtmlFileAsPdf("input.html");
  pdf.SaveAs("output.pdf");

  // Or from URL
  var pdf = renderer.RenderUrlAsPdf("https://example.com");
  ```
  **Why:** IronPDF's Chromium engine provides superior CSS/JS support.

- [ ] **Convert event handlers to HtmlHeader/HtmlFooter**
  ```csharp
  // Before (iText) - complex page event handler
  public class HeaderFooterHandler : IEventHandler
  {
      public void HandleEvent(Event @event)
      {
          var docEvent = (PdfDocumentEvent)@event;
          var page = docEvent.GetPage();
          var canvas = new PdfCanvas(page);
          // Complex drawing code for headers/footers...
      }
  }

  // After (IronPDF) - simple declaration
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
  {
      HtmlFragment = "<div style='text-align:center;'>Company Name</div>",
      MaxHeight = 30
  };

  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** Declarative headers/footers are simpler than iText's page event model.

- [ ] **Update password protection code**
  ```csharp
  // Before (iText)
  var writer = new PdfWriter("output.pdf", new WriterProperties()
      .SetStandardEncryption(
          Encoding.UTF8.GetBytes("userpass"),
          Encoding.UTF8.GetBytes("ownerpass"),
          EncryptionConstants.ALLOW_PRINTING,
          EncryptionConstants.ENCRYPTION_AES_128));

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SecuritySettings.UserPassword = "userpass";
  pdf.SecuritySettings.OwnerPassword = "ownerpass";
  pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF's security API is more intuitive.

- [ ] **Migrate text extraction code**
  ```csharp
  // Before (iText)
  using (var reader = new PdfReader("document.pdf"))
  using (var pdfDoc = new PdfDocument(reader))
  {
      for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
      {
          var page = pdfDoc.GetPage(i);
          var text = PdfTextExtractor.GetTextFromPage(page);
      }
  }

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("document.pdf");
  string allText = pdf.ExtractAllText();

  // Or per page
  foreach (var page in pdf.Pages)
  {
      string pageText = page.Text;
  }
  ```
  **Why:** IronPDF provides simpler text extraction.

- [ ] **Update merge operations**
  ```csharp
  // Before (iText)
  var pdf = new PdfDocument(new PdfWriter("merged.pdf"));
  var merger = new PdfMerger(pdf);
  merger.Merge(new PdfDocument(new PdfReader("doc1.pdf")), 1, 10);
  merger.Merge(new PdfDocument(new PdfReader("doc2.pdf")), 1, 5);
  pdf.Close();

  // After (IronPDF)
  var merged = PdfDocument.Merge(
      PdfDocument.FromFile("doc1.pdf"),
      PdfDocument.FromFile("doc2.pdf")
  );
  merged.SaveAs("merged.pdf");
  ```
  **Why:** IronPDF merge is a one-liner.

### Testing

- [ ] **Test all PDF generation paths**
  ```csharp
  // Create a test suite covering all document types
  [Theory]
  [InlineData("invoice")]
  [InlineData("report")]
  [InlineData("certificate")]
  public async Task GeneratePdf_ShouldProduceValidOutput(string documentType)
  {
      var pdf = await _pdfService.GenerateAsync(documentType, testData);
      Assert.True(pdf.PageCount > 0);
      Assert.True(pdf.BinaryData.Length > 0);
  }
  ```
  **Why:** Ensure all document types render correctly.

- [ ] **Verify visual output matches expectations**
  ```csharp
  // Compare PDF output visually
  // Consider using a tool like PDFCompare or visual regression testing
  var newPdf = renderer.RenderHtmlAsPdf(html);
  newPdf.SaveAs("new_output.pdf");

  // Manual review or automated comparison with reference PDFs
  ```
  **Why:** Visual comparison ensures migration doesn't break layouts.

- [ ] **Benchmark performance**
  ```csharp
  var stopwatch = Stopwatch.StartNew();
  for (int i = 0; i < 100; i++)
  {
      var pdf = renderer.RenderHtmlAsPdf(html);
  }
  Console.WriteLine($"Average: {stopwatch.ElapsedMilliseconds / 100}ms per PDF");

  // Note: First render includes Chromium initialization (1-3s)
  // Subsequent renders are much faster
  ```
  **Why:** Understand performance characteristics for capacity planning.

### Post-Migration

- [ ] **Remove iText license files and references**
  ```bash
  # Delete iText license files
  rm -f iText.License.xml
  rm -f itextkey.xml

  # Remove any license-related code
  grep -r "LicenseKey\|iTextLicense" --include="*.cs" .
  ```
  **Why:** Clean up removes potential confusion and license concerns.

- [ ] **Update documentation**
  ```markdown
  # Document the new HTML-first approach for team consistency
  ```
  **Why:** Ensure team members understand the new workflow and benefits.
## PDF Generation (Updated)

We now use IronPDF with HTML templates instead of iText.

### Quick Start
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

### Templates Location
HTML templates are in `/Templates/Pdf/`
```

---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/
- **iText 7 Documentation**: https://kb.itextpdf.com/itext/
- **AGPL License FAQ**: https://www.gnu.org/licenses/agpl-3.0.en.html

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
