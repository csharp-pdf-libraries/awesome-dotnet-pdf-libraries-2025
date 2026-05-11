---
title: "XFINIUM.PDF vs IronPDF: a .NET developer honest take"
published: false
tags: dotnet, csharp, pdf, comparison
canonical_url: https://ironsoftware.com/csharp/pdf/blog/compare-to-competitors/
---

## Understanding IronPDF

[IronPDF](https://ironpdf.com) takes HTML/CSS as input and renders it to PDF using Chromium. You write `<div>`, `<table>`, and CSS layouts—IronPDF handles coordinate math, pagination, font rendering, and page breaks automatically. Install via `Install-Package IronPdf`, pass HTML strings to `ChromePdfRenderer.RenderHtmlAsPdf()`. The rendering model matches web browsers: what you see in Chrome is what appears in the PDF.

This approach suits scenarios where content structure is defined in templates: invoices with variable line items, reports from database queries, multi-page documents with headers/footers, or any workflow where separating layout (HTML/CSS) from logic (C# code) simplifies maintenance.

## Where XFINIUM.PDF Fits

### Product Status
XFINIUM.PDF is a commercial .NET PDF library distributed via NuGet. The product line typically targets .NET Framework, .NET Standard, .NET Core, and modern .NET, with platform-specific packages such as `Xfinium.Pdf.NetStandard` and `Xfinium.Pdf.NetCore`. Cross-platform deployment (Windows, Linux, macOS) and additional targets like Xamarin and Unity are documented per platform — verify the exact package that matches your runtime.

### Rendering Model
XFINIUM.PDF does not ship a native HTML-to-PDF engine. The library exposes PDF primitives: draw text at (x, y), draw lines and rectangles, place images at coordinates, create form fields. There is a separate flow-document API for content-flow layout, and the vendor publishes a sample HTML-to-PDF converter that walks XHTML with `XmlReader` and emits formatted content for a limited tag set (`p`, `font`, `b`, `i`, `u`, `ul`, `li`); CSS and JavaScript are not interpreted.

### Layout Considerations
Because layout is coordinate-driven (with the flow API as an alternative for simpler stacks), teams implement page-break logic, table cell math, and text wrapping themselves. This trade-off is well-suited to fixed-format outputs (certificates, labels, form overlays) but can become repetitive for variable, content-heavy reports.

### Support and Docs
Commercial support is available from the vendor. Documentation covers API reference and programmatic examples; HTML-template examples are limited because the in-box API is not HTML-first.

### Architecture
XFINIUM.PDF maps closely to the PDF specification — your code constructs PDF objects directly. That gives precise control for simple, fixed layouts. Multi-column reports, responsive tables, and nested content typically require more manual position math than a browser-style renderer.

## Feature Comparison Overview

| Aspect | XFINIUM.PDF | IronPDF |
|--------|-------------|---------|
| **Current Status** | Actively maintained commercial product | Actively maintained, ships with Chromium |
| **HTML Support** | No native HTML engine (vendor sample XHTML walker only) | Full HTML5/CSS3 rendering |
| **Rendering Quality** | Pixel-level control via coordinate primitives | Browser-quality (Chromium engine) |
| **Installation** | Single NuGet package (platform-specific) | Single NuGet package |
| **Support** | Commercial (vendor) | Commercial (Iron Software) |
| **Future Viability** | Active development | Active development |

---

## Creating an Invoice: Coordinate Math vs HTML Template

### XFINIUM.PDF — Manual Coordinate Layout

```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;
using System.IO;

public class XfiniumInvoiceGenerator
{
    public byte[] CreateInvoice(string companyName, string[] lineItems)
    {
        // Create new PDF document
        var document = new PdfFixedDocument();
        var page = document.Pages.Add();

        // Fonts and brush
        var titleFont = new PdfStandardFont(PdfStandardFontFace.HelveticaBold, 18);
        var bodyFont = new PdfStandardFont(PdfStandardFontFace.Helvetica, 12);
        var brush = new PdfBrush(new PdfRgbColor(0, 0, 0));

        // Coordinate-based positioning: track Y as content is added
        double currentY = 50;
        page.Graphics.DrawString(companyName, titleFont, brush, 50, currentY);
        currentY += 30;

        // Invoice header
        page.Graphics.DrawString("INVOICE", titleFont, brush, 50, currentY);
        currentY += 40;

        // Table header
        page.Graphics.DrawString("Item", bodyFont, brush, 50, currentY);
        page.Graphics.DrawString("Amount", bodyFont, brush, 400, currentY);
        currentY += 5;

        // Horizontal rule under the header
        page.Graphics.DrawLine(
            new PdfPen(new PdfRgbColor(0, 0, 0), 1),
            50, currentY, 500, currentY);
        currentY += 15;

        // Line items
        foreach (var item in lineItems)
        {
            // Add a new page when the current Y approaches the page bottom
            if (currentY > 700)
            {
                page = document.Pages.Add();
                currentY = 50;
            }

            page.Graphics.DrawString(item, bodyFont, brush, 50, currentY);
            currentY += 20;
        }

        // Total line
        currentY += 10;
        page.Graphics.DrawLine(
            new PdfPen(new PdfRgbColor(0, 0, 0), 1), 
            50, currentY, 500, currentY);
        currentY += 15;
        page.Graphics.DrawString("Total", bodyFont, brush, 50, currentY);
        
        // Save to memory stream
        using (var stream = new MemoryStream())
        {
            document.Save(stream);
            return stream.ToArray();
        }
    }
}

// Usage
var generator = new XfiniumInvoiceGenerator();
byte[] pdf = generator.CreateInvoice(
    "Acme Corporation",
    new[] { "Item 1: $100", "Item 2: $200", "Item 3: $150" }
);
// Layout changes generally mean recalculating X/Y coordinates
```

**Things to watch for with coordinate-based layout:**
1. **Y-coordinate drift**: forgetting to increment `currentY` can cause overlapping text
2. **Page overflow**: content beyond the page height requires explicit pagination
3. **Column alignment**: table columns rely on matching X positions across rows
4. **Font metrics**: text width varies per font, so right-aligning often needs measurement
5. **Word wrapping**: long strings need manual splitting unless the flow API is used
6. **Edit churn**: moving an element typically shifts every following Y position

### IronPDF — HTML Template Approach

```csharp
using IronPdf;
using System.Threading.Tasks;

public class IronPdfInvoiceGenerator
{
    public async Task<byte[]> CreateInvoiceAsync(
        string companyName, 
        string[] lineItems)
    {
        var renderer = new ChromePdfRenderer();
        
        // HTML template with CSS - no coordinate math
        string html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 20px; }}
        h1 {{ text-align: center; color: #333; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 10px; text-align: left; }}
        th {{ background-color: #f2f2f2; }}
        .total-row {{ font-weight: bold; background-color: #f9f9f9; }}
    </style>
</head>
<body>
    <h1>{companyName}</h1>
    <h2>INVOICE</h2>
    <table>
        <thead>
            <tr>
                <th>Item</th>
                <th>Amount</th>
            </tr>
        </thead>
        <tbody>
            {string.Join("", lineItems.Select(item => 
                $"<tr><td>{item}</td></tr>"))}
        </tbody>
        <tfoot>
            <tr class='total-row'>
                <td>Total</td>
                <td>$450</td>
            </tr>
        </tfoot>
    </table>
</body>
</html>";
        
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}

// Usage
var generator = new IronPdfInvoiceGenerator();
byte[] pdf = await generator.CreateInvoiceAsync(
    "Acme Corporation",
    new[] { "Item 1: $100", "Item 2: $200", "Item 3: $150" }
);
// Layout changes = CSS edits, no coordinate recalculation
```

IronPDF handles pagination, table layout, text wrapping, and alignment automatically via HTML/CSS. Learn more about [HTML string to PDF conversion](https://ironpdf.com/how-to/html-string-to-pdf/).

---

## Adding Images: Aspect Ratio Math vs \<img\> Tag

### XFINIUM.PDF — Manual Image Positioning and Scaling

```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;
using System.IO;

public class XfiniumImageGenerator
{
    public byte[] AddCompanyLogo(string logoPath)
    {
        var document = new PdfFixedDocument();
        var page = document.Pages.Add();
        
        // Load the image (e.g. PdfJpegImage / PdfPngImage from a FileStream)
        using FileStream imageStream = File.OpenRead(logoPath);
        var pdfImage = new PdfJpegImage(imageStream);

        // Compute target size while preserving aspect ratio
        double originalWidth = 800;
        double originalHeight = 600;
        double aspectRatio = originalWidth / originalHeight;

        double targetWidth = 200;
        double targetHeight = targetWidth / aspectRatio;

        // Margins in PDF user-space units
        double xPosition = 50;
        double yPosition = 50;

        // Fit-to-page guard
        if (yPosition + targetHeight > 750)
        {
            double maxHeight = 750 - yPosition;
            targetHeight = maxHeight;
            targetWidth = targetHeight * aspectRatio;
        }

        page.Graphics.DrawImage(pdfImage, xPosition, yPosition, targetWidth, targetHeight);
        
        using (var stream = new MemoryStream())
        {
            document.Save(stream);
            return stream.ToArray();
        }
    }
}

// Usage
var generator = new XfiniumImageGenerator();
byte[] pdf = generator.AddCompanyLogo(@"C:\logos\company.png");
```

**Things to watch for with manual image placement:**
1. **Aspect ratio**: math slips can stretch or squash the image
2. **DPI**: PDF units (points) and source pixel dimensions need conversion
3. **Overflow**: nothing checks the page bottom automatically — that guard is yours
4. **Format support**: confirm the specific image class for your format (`PdfJpegImage`, `PdfPngImage`, etc.)
5. **Memory loading**: large images are typically loaded fully before placement
6. **Centering**: horizontal centering depends on knowing page width

### IronPDF — HTML \<img\> Tag

```csharp
using IronPdf;
using System.Threading.Tasks;

public class IronPdfImageGenerator
{
    public async Task<byte[]> AddCompanyLogoAsync(string logoPath)
    {
        var renderer = new ChromePdfRenderer();
        
        string html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ text-align: center; padding: 20px; }}
        img {{ max-width: 400px; height: auto; }}
    </style>
</head>
<body>
    <h1>Company Report</h1>
    <img src='file:///{logoPath.Replace("\\", "/")}' alt='Logo' />
    <p>Annual financial summary...</p>
</body>
</html>";
        
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}

// Usage
var generator = new IronPdfImageGenerator();
byte[] pdf = await generator.AddCompanyLogoAsync(@"C:\logos\company.png");
// Aspect ratio, centering, and sizing are handled by CSS
```

IronPDF uses standard HTML `<img>` tags with CSS for sizing. No manual aspect ratio calculations needed. See [pixel-perfect HTML to PDF guide](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/).

---

## Multi-Page Reports: Pagination Logic vs Automatic Flow

### XFINIUM.PDF — Manual Page Break Detection

```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;
using System.Collections.Generic;
using System.IO;

public class XfiniumReportGenerator
{
    private const double PageHeight = 750; // PDF page height in points
    private const double TopMargin = 50;
    private const double BottomMargin = 50;
    private const double LineHeight = 20;
    
    public byte[] CreateReport(List<string> reportLines)
    {
        var document = new PdfFixedDocument();
        var page = document.Pages.Add();
        
        var bodyFont = new PdfStandardFont(PdfStandardFontFace.Helvetica, 12);
        var brush = new PdfBrush(new PdfRgbColor(0, 0, 0));
        
        double currentY = TopMargin;
        
        foreach (var line in reportLines)
        {
            // Page-break check: would this line exceed the printable area?
            if (currentY + LineHeight > PageHeight - BottomMargin)
            {
                page = document.Pages.Add();
                currentY = TopMargin;

                // Repeat the running header on the new page
                page.Graphics.DrawString(
                    "Annual Report (continued)",
                    bodyFont,
                    brush,
                    50,
                    currentY);
                currentY += 30;
            }

            page.Graphics.DrawString(line, bodyFont, brush, 50, currentY);
            currentY += LineHeight;
        }
        
        using (var stream = new MemoryStream())
        {
            document.Save(stream);
            return stream.ToArray();
        }
    }
}

// Usage
var generator = new XfiniumReportGenerator();
var lines = new List<string>();
for (int i = 1; i <= 200; i++)
{
    lines.Add($"Report line {i}: Lorem ipsum dolor sit amet...");
}
byte[] pdf = generator.CreateReport(lines);
// Page-break logic is explicit for each block of content
```

**Things to watch for with manual pagination:**
1. **Page-break timing**: text can clip if `LineHeight` does not match real font metrics
2. **Running headers**: re-draw them on each new page yourself
3. **Variable-height content**: tables or images with dynamic heights complicate the math
4. **Multi-column layouts**: each column needs its own Y tracker
5. **Orphan/widow control**: typically implemented in user code
6. **Page numbering**: track and render "Page X of Y" manually

### IronPDF — Automatic Pagination with CSS

```csharp
using IronPdf;
using System.Collections.Generic;
using System.Threading.Tasks;

public class IronPdfReportGenerator
{
    public async Task<byte[]> CreateReportAsync(List<string> reportLines)
    {
        var renderer = new ChromePdfRenderer();
        
        // CSS handles page breaks automatically
        string html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        @page {{ size: A4; margin: 20mm; }}
        body {{ font-family: Arial, sans-serif; line-height: 1.5; }}
        h1 {{ page-break-after: avoid; }}
        .report-line {{ page-break-inside: avoid; }}
        @media print {{
            .page-header {{ 
                position: running(header); 
                text-align: center; 
                border-bottom: 1px solid #ccc; 
            }}
            @page {{ @top-center {{ content: element(header); }} }}
        }}
    </style>
</head>
<body>
    <div class='page-header'>Annual Report</div>
    <h1>Executive Summary</h1>
    {string.Join("", reportLines.Select(line => 
        $"<div class='report-line'>{line}</div>"))}
</body>
</html>";
        
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}

// Usage
var generator = new IronPdfReportGenerator();
var lines = new List<string>();
for (int i = 1; i <= 200; i++)
{
    lines.Add($"Report line {i}: Lorem ipsum dolor sit amet...");
}
byte[] pdf = await generator.CreateReportAsync(lines);
// CSS page-break-* rules handle pagination automatically
```

IronPDF uses CSS `@page` rules for page breaks, headers, and footers. Content flows across pages automatically. See [PDF generation settings](https://ironpdf.com/examples/pdf-generation-settings/).

---

## API Mapping Reference

| XFINIUM.PDF Concept | IronPDF Equivalent |
|---------------------|-------------------|
| `PdfFixedDocument` | `ChromePdfRenderer.RenderHtmlAsPdf()` returns `PdfDocument` |
| `document.Pages.Add()` | Automatic via HTML content flow |
| `Graphics.DrawString(text, font, brush, x, y)` | HTML text elements with CSS positioning |
| `Graphics.DrawLine(pen, x1, y1, x2, y2)` | HTML `<hr>` or CSS `border` |
| `Graphics.DrawRectangle(pen, x, y, width, height)` | HTML `<div>` with CSS `border` |
| `Graphics.DrawImage(image, x, y, w, h)` | HTML `<img>` tag |
| `PdfStandardFont` | CSS `font-family` |
| Manual Y-coordinate tracking | CSS layout (Grid, Flexbox, tables) |
| Manual page break logic | CSS `page-break-before/after/inside` |
| `document.Save(stream)` | `pdf.SaveAs(path)` or `pdf.BinaryData` |
| Form field creation | HTML form elements + `pdf.Form` API |
| Barcode drawing (coordinate-based) | HTML + CSS or embedded image |
| Low-level PDF object manipulation | Not exposed (Chromium handles PDF structure) |

---

## Comprehensive Feature Comparison

| Feature Category | XFINIUM.PDF | IronPDF |
|------------------|-------------|---------|
| **Status** |
| Maintenance Status | Actively maintained | Actively maintained |
| Release Cadence | Vendor-driven | Continuous (updated with Chromium) |
| Cross-Platform | Windows/Linux/macOS (plus Xamarin, Unity per platform) | Windows/Linux/macOS |
| **Support** |
| Official Support | Commercial (vendor) | Commercial (Iron Software) |
| Documentation | API reference + examples | API reference + HTML guides |
| **Content Creation** |
| HTML to PDF | No—programmatic only | Yes (core feature) |
| Coordinate-based drawing | Yes | No (uses HTML/CSS layout) |
| Text positioning | Manual (x, y) | Automatic via HTML |
| Tables | Manual cell drawing | HTML `<table>` |
| Images | Manual placement/scaling | HTML `<img>` with CSS |
| Multi-column layouts | Manual positioning | CSS Grid/Flexbox |
| Page breaks | Manual detection | CSS `page-break-*` |
| Headers/Footers | Manual on each page | CSS `@page` or RenderingOptions |
| **PDF Operations** |
| Create PDFs | Yes (programmatic) | Yes (from HTML) |
| Merge PDFs | Yes | Yes |
| Split PDFs | Yes | Yes |
| Extract text | Yes | Yes |
| Form filling | Yes | Yes |
| Digital signatures | Yes | Yes |
| Redaction | Yes | Yes |
| **Security** |
| Encryption | Yes (AES 256) | Yes (AES 256) |
| Permissions | Yes | Yes |
| Digital certificates | Yes | Yes |
| **Tradeoffs** |
| Learning curve | Coordinate-based; PDF spec familiarity helps | HTML/CSS skills transfer directly |
| Layout iteration | Recalculate positions | Edit CSS |
| HTML input | Not part of the core API (vendor sample XHTML walker only) | Native (Chromium) |
| Pagination | Implemented in user code | Automatic via CSS |
| **Development** |
| API Style | Low-level PDF primitives | High-level HTML rendering |
| Learning Curve | Steep (PDF spec knowledge) | Shallow (HTML/CSS) |
| Code Maintenance | High (coordinate coupling) | Low (template separation) |
| Layout Control | Pixel-perfect | CSS-based |
| Dynamic Content | Requires recalculation | Automatic reflow |

---

## Installation Comparison

**XFINIUM.PDF:**
```bash
Install-Package Xfinium.Pdf.NetCore
# or Xfinium.Pdf.NetStandard
```
```csharp
using Xfinium.Pdf;
using Xfinium.Pdf.Graphics;

var document = new PdfFixedDocument();
var page = document.Pages.Add();

var font = new PdfStandardFont(PdfStandardFontFace.Helvetica, 12);
var brush = new PdfBrush(new PdfRgbColor(0, 0, 0));

page.Graphics.DrawString("Hello World", font, brush, 50, 50);

using (var stream = new System.IO.MemoryStream())
{
    document.Save(stream);
    // Use stream.ToArray() for bytes
}
```

**IronPDF:**
```bash
Install-Package IronPdf
```
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

---

## Conclusion

XFINIUM.PDF serves a specific niche: teams needing low-level PDF control for barcode labels, form overlays, certificate generation with fixed layouts, or PDF manipulation without HTML involvement. The coordinate-based API gives pixel-perfect control at the cost of manual layout implementation.

Teams choose XFINIUM.PDF when:
- PDF content is purely programmatic (generated graphics, barcodes, stamps)
- Layout is fixed and simple (certificates, labels, form templates)
- HTML parsing overhead is undesirable
- Low-level PDF object access is required
- The team has PDF specification expertise

Migration from XFINIUM.PDF to IronPDF makes sense when:
- Invoice/report templates become complex and coordinate math becomes unwieldy
- Business users want to modify layouts without touching code
- Multi-page documents with variable content require pagination logic
- Responsive tables and dynamic layouts replace fixed-position elements
- Maintenance cost of coordinate recalculation exceeds template editing

IronPDF uses HTML/CSS templating instead of coordinate math. The [ChromePdfRenderer API](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderer.html/) accepts HTML strings, handles layout automatically, and generates PDFs without manual positioning. For document-oriented workflows (invoices, reports, letters), template-based generation reduces maintenance overhead.

The fundamental difference: XFINIUM.PDF is a PDF construction library where you control every primitive. IronPDF is an HTML rendering engine where the browser handles layout. Choose based on whether your PDFs are "designed documents" (use IronPDF with templates) or "generated outputs" (consider XFINIUM.PDF for programmatic control).

**If you're using XFINIUM.PDF, what percentage of your code is coordinate calculation versus business logic?** Have complex layouts pushed you toward template-based approaches?

**Related Resources:**
- [HTML to PDF Conversion Guide](https://ironpdf.com/how-to/html-string-to-pdf/)
- [PDF Generation Settings Documentation](https://ironpdf.com/examples/pdf-generation-settings/)
