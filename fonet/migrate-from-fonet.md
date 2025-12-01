# How Do I Migrate from FoNet (FO.NET) to IronPDF in C#?

## Table of Contents
1. [Why Migrate to IronPDF](#why-migrate-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [XSL-FO to HTML Conversion Guide](#xsl-fo-to-html-conversion-guide)
6. [Code Examples](#code-examples)
7. [Advanced Scenarios](#advanced-scenarios)
8. [Performance Considerations](#performance-considerations)
9. [Troubleshooting](#troubleshooting)
10. [Migration Checklist](#migration-checklist)

---

## Why Migrate to IronPDF

### The FoNet (FO.NET) Challenges

FoNet is an XSL-FO to PDF renderer that has significant limitations for modern development:

1. **Obsolete Technology**: XSL-FO (Extensible Stylesheet Language Formatting Objects) is a W3C specification from 2001 that has seen no updates since 2006 and is largely considered obsolete
2. **Steep Learning Curve**: XSL-FO requires learning complex XML-based markup with specialized formatting objects (fo:block, fo:table, fo:page-sequence, etc.)
3. **No HTML/CSS Support**: Cannot render HTML or CSS—requires manual conversion from HTML to XSL-FO markup
4. **Abandoned/Unmaintained**: The original CodePlex repository is defunct; GitHub forks are no longer actively maintained
5. **Windows-Only**: FoNet has internal dependencies on System.Drawing that prevent it from working on Linux/macOS
6. **Limited Modern Features**: No JavaScript support, no CSS3, no flexbox/grid, no modern web fonts
7. **No URL Rendering**: Cannot directly render web pages—requires manual HTML-to-XSL-FO conversion

### Benefits of IronPDF

| Aspect | FoNet (FO.NET) | IronPDF |
|--------|---------------|---------|
| Input Format | XSL-FO (obsolete XML) | HTML/CSS (modern web standards) |
| Learning Curve | Steep (XSL-FO expertise) | Gentle (HTML/CSS knowledge) |
| Maintenance | Abandoned | Actively maintained monthly |
| Platform Support | Windows only | True cross-platform |
| CSS Support | None | Full CSS3 (Flexbox, Grid) |
| JavaScript | None | Full JavaScript support |
| URL Rendering | Not supported | Built-in |
| Modern Features | Limited | Headers, footers, watermarks, security |
| Documentation | Outdated | Comprehensive tutorials |

### Why the Switch Makes Sense

FoNet was designed when XSL-FO was expected to become a standard for document formatting. That didn't happen. HTML/CSS became the universal document format:

- **98%+ of developers** know HTML/CSS
- **< 1% of developers** know XSL-FO
- Most XSL-FO resources are from 2005-2010

IronPDF lets you use the skills you already have to create professional PDFs.

---

## Before You Start

### Prerequisites

1. **.NET Environment**: IronPDF supports .NET Framework 4.6.2+, .NET Core 3.1+, .NET 5/6/7/8/9+
2. **NuGet Access**: Ensure you can install packages from NuGet
3. **License Key**: Obtain your IronPDF license key for production use

### Backup Your Project

```bash
# Create a backup branch
git checkout -b pre-ironpdf-migration
git add .
git commit -m "Backup before FoNet to IronPDF migration"
```

### Identify All FoNet Usage

```bash
# Find all FoNet references
grep -r "FonetDriver\|Fonet\|\.fo\"\|xsl-region" --include="*.cs" --include="*.csproj" .

# Find all XSL-FO template files
find . -name "*.fo" -o -name "*.xslfo" -o -name "*xsl-fo*"
```

### Document Your XSL-FO Templates

Before migration, catalog all XSL-FO files:
- Page dimensions and margins
- Fonts used
- Tables and their structures
- Headers and footers (fo:static-content)
- Page numbering patterns
- Image references

---

## Quick Start Migration

### Step 1: Update NuGet Packages

```bash
# Remove FoNet package
dotnet remove package Fonet
dotnet remove package FO.NET

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using Fonet;
using Fonet.Render.Pdf;
using System.Xml;

// After
using IronPdf;
using IronPdf.Rendering;
```

### Step 3: Initialize IronPDF

```csharp
// Set license key at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 4: Basic Conversion Pattern

```csharp
// Before (FoNet with XSL-FO)
FonetDriver driver = FonetDriver.Make();
using (FileStream output = new FileStream("output.pdf", FileMode.Create))
{
    driver.Render(new StringReader(xslFoContent), output);
}

// After (IronPDF with HTML)
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

---

## Complete API Reference

### Namespace Mapping

| FoNet Namespace | IronPDF Equivalent | Notes |
|-----------------|-------------------|-------|
| `Fonet` | `IronPdf` | Main namespace |
| `Fonet.Render.Pdf` | `IronPdf` | PDF rendering |
| `Fonet.Layout` | N/A | Layout handled by CSS |
| `Fonet.Fo` | N/A | Formatting Objects → HTML |
| `Fonet.Datatypes` | N/A | Use C# types |
| `Fonet.Image` | `IronPdf` | Image handling built-in |

### FonetDriver to ChromePdfRenderer

| FonetDriver Method | IronPDF Equivalent | Notes |
|--------------------|-------------------|-------|
| `FonetDriver.Make()` | `new ChromePdfRenderer()` | Create renderer |
| `driver.Render(inputStream, outputStream)` | `renderer.RenderHtmlAsPdf(html)` | Stream-based |
| `driver.Render(xmlReader, outputStream)` | `renderer.RenderHtmlAsPdf(html)` | XML reader not needed |
| `driver.Render(xmlDoc, outputStream)` | `renderer.RenderHtmlAsPdf(html)` | XML doc not needed |
| `driver.Render(inputFile, outputStream)` | `renderer.RenderHtmlFileAsPdf(path)` | File-based |
| `driver.OnError += handler` | Try/catch around render | Error handling |
| `driver.OnWarning += handler` | IronPDF logging | Warning handling |
| `driver.OnInfo += handler` | N/A | Info logging |

### FonetDriver Properties

| FonetDriver Property | IronPDF Equivalent | Notes |
|---------------------|-------------------|-------|
| `driver.CloseOnExit` | Automatic | IronPDF handles resources |
| `driver.Options` | `RenderingOptions` | Configuration |
| `driver.BaseDirectory` | `RenderingOptions.BaseUrl` | Base path for resources |

### RenderingOptions (PDF Configuration)

| FoNet (XSL-FO Attributes) | IronPDF RenderingOptions | Notes |
|--------------------------|-------------------------|-------|
| `page-height` | `PaperSize` or `SetCustomPaperSize()` | Page dimensions |
| `page-width` | `PaperSize` | Standard or custom |
| `margin-top` | `MarginTop` | In millimeters |
| `margin-bottom` | `MarginBottom` | In millimeters |
| `margin-left` | `MarginLeft` | In millimeters |
| `margin-right` | `MarginRight` | In millimeters |
| `reference-orientation` | `PaperOrientation` | Portrait/Landscape |

### XSL-FO Elements to HTML/CSS

| XSL-FO Element | HTML/CSS Equivalent | Notes |
|----------------|-------------------|-------|
| `<fo:root>` | `<html>` | Root element |
| `<fo:layout-master-set>` | CSS `@page` rule | Page setup |
| `<fo:simple-page-master>` | CSS `@page` | Page definition |
| `<fo:page-sequence>` | `<body>` or `<div>` | Page content |
| `<fo:flow>` | `<main>` or `<div>` | Main content area |
| `<fo:static-content>` | `HtmlHeaderFooter` | Headers/footers |
| `<fo:block>` | `<p>`, `<div>`, `<h1>-<h6>` | Block content |
| `<fo:inline>` | `<span>` | Inline content |
| `<fo:table>` | `<table>` | Tables |
| `<fo:table-row>` | `<tr>` | Table rows |
| `<fo:table-cell>` | `<td>`, `<th>` | Table cells |
| `<fo:list-block>` | `<ul>`, `<ol>` | Lists |
| `<fo:list-item>` | `<li>` | List items |
| `<fo:external-graphic>` | `<img>` | Images |
| `<fo:page-number>` | `{page}` placeholder | Page numbers |
| `<fo:page-number-citation>` | `{total-pages}` | Total pages |
| `<fo:leader>` | CSS `flex: 1` or dots pattern | Leader/tab |
| `<fo:footnote>` | HTML footnote pattern | Footnotes |
| `<fo:basic-link>` | `<a href>` | Hyperlinks |

### XSL-FO Properties to CSS

| XSL-FO Property | CSS Equivalent | Example |
|-----------------|---------------|---------|
| `font-family` | `font-family` | Same syntax |
| `font-size` | `font-size` | Same syntax |
| `font-weight` | `font-weight` | `bold`, `normal`, `700` |
| `font-style` | `font-style` | `italic`, `normal` |
| `text-align` | `text-align` | `left`, `center`, `right`, `justify` |
| `color` | `color` | Hex, RGB, names |
| `background-color` | `background-color` | Same syntax |
| `border` | `border` | Same syntax |
| `padding` | `padding` | Same syntax |
| `margin` | `margin` | Same syntax |
| `space-before` | `margin-top` | Before element |
| `space-after` | `margin-bottom` | After element |
| `start-indent` | `margin-left` | Left indent |
| `end-indent` | `margin-right` | Right indent |
| `line-height` | `line-height` | Same syntax |
| `text-indent` | `text-indent` | Same syntax |
| `text-decoration` | `text-decoration` | `underline`, `none` |
| `vertical-align` | `vertical-align` | Same syntax |
| `display-align` | `vertical-align` | In table cells |
| `keep-together` | `page-break-inside: avoid` | Prevent breaks |
| `break-before="page"` | `page-break-before: always` | Force page break |
| `break-after="page"` | `page-break-after: always` | Force page break |
| `width` | `width` | Same syntax |
| `height` | `height` | Same syntax |
| `content-width` | `width` (on img) | Image width |
| `content-height` | `height` (on img) | Image height |

---

## XSL-FO to HTML Conversion Guide

### Page Setup

**XSL-FO:**
```xml
<fo:layout-master-set>
    <fo:simple-page-master master-name="A4"
        page-height="297mm" page-width="210mm"
        margin-top="20mm" margin-bottom="20mm"
        margin-left="25mm" margin-right="25mm">
        <fo:region-body margin-top="30mm" margin-bottom="30mm"/>
        <fo:region-before extent="25mm"/>
        <fo:region-after extent="25mm"/>
    </fo:simple-page-master>
</fo:layout-master-set>
```

**HTML/CSS + IronPDF:**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 25;
renderer.RenderingOptions.MarginRight = 25;

// Headers/footers replace fo:region-before and fo:region-after
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    MaxHeight = 25,  // mm
    HtmlFragment = "<div style='text-align:center;'>Header Content</div>"
};
```

### Headers and Footers

**XSL-FO:**
```xml
<fo:static-content flow-name="xsl-region-before">
    <fo:block text-align="center" font-size="10pt">
        Company Name - Confidential
    </fo:block>
</fo:static-content>

<fo:static-content flow-name="xsl-region-after">
    <fo:block text-align="right" font-size="10pt">
        Page <fo:page-number/> of <fo:page-number-citation ref-id="last-page"/>
    </fo:block>
</fo:static-content>
```

**IronPDF:**
```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:center; font-size:10pt;'>Company Name - Confidential</div>",
    DrawDividerLine = true
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:right; font-size:10pt;'>Page {page} of {total-pages}</div>",
    DrawDividerLine = true
};
```

### Tables

**XSL-FO:**
```xml
<fo:table border="1pt solid black">
    <fo:table-column column-width="50mm"/>
    <fo:table-column column-width="100mm"/>
    <fo:table-header>
        <fo:table-row background-color="#CCCCCC">
            <fo:table-cell padding="3mm">
                <fo:block font-weight="bold">Product</fo:block>
            </fo:table-cell>
            <fo:table-cell padding="3mm">
                <fo:block font-weight="bold">Description</fo:block>
            </fo:table-cell>
        </fo:table-row>
    </fo:table-header>
    <fo:table-body>
        <fo:table-row>
            <fo:table-cell padding="3mm" border="0.5pt solid black">
                <fo:block>Widget A</fo:block>
            </fo:table-cell>
            <fo:table-cell padding="3mm" border="0.5pt solid black">
                <fo:block>A high-quality widget</fo:block>
            </fo:table-cell>
        </fo:table-row>
    </fo:table-body>
</fo:table>
```

**HTML:**
```html
<style>
    table { border: 1px solid black; border-collapse: collapse; }
    th { background-color: #CCCCCC; padding: 3mm; font-weight: bold; }
    td { padding: 3mm; border: 0.5px solid black; }
</style>
<table>
    <thead>
        <tr>
            <th style="width: 50mm;">Product</th>
            <th style="width: 100mm;">Description</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Widget A</td>
            <td>A high-quality widget</td>
        </tr>
    </tbody>
</table>
```

### Lists

**XSL-FO:**
```xml
<fo:list-block>
    <fo:list-item>
        <fo:list-item-label end-indent="label-end()">
            <fo:block>&#x2022;</fo:block>
        </fo:list-item-label>
        <fo:list-item-body start-indent="body-start()">
            <fo:block>First item</fo:block>
        </fo:list-item-body>
    </fo:list-item>
    <fo:list-item>
        <fo:list-item-label end-indent="label-end()">
            <fo:block>&#x2022;</fo:block>
        </fo:list-item-label>
        <fo:list-item-body start-indent="body-start()">
            <fo:block>Second item</fo:block>
        </fo:list-item-body>
    </fo:list-item>
</fo:list-block>
```

**HTML:**
```html
<ul>
    <li>First item</li>
    <li>Second item</li>
</ul>
```

### Images

**XSL-FO:**
```xml
<fo:external-graphic src="url('logo.png')"
    content-width="50mm"
    content-height="auto"
    scaling="uniform"/>
```

**HTML:**
```html
<img src="logo.png" style="width: 50mm; height: auto;" />
```

---

## Code Examples

### Example 1: Simple Document

**Before (FoNet with XSL-FO):**
```csharp
using Fonet;
using System.IO;

public byte[] GenerateSimpleDocument(string title, string content)
{
    string xslFo = $@"<?xml version='1.0' encoding='utf-8'?>
        <fo:root xmlns:fo='http://www.w3.org/1999/XSL/Format'>
            <fo:layout-master-set>
                <fo:simple-page-master master-name='page'
                    page-height='11in' page-width='8.5in'
                    margin-top='1in' margin-bottom='1in'
                    margin-left='1in' margin-right='1in'>
                    <fo:region-body/>
                </fo:simple-page-master>
            </fo:layout-master-set>
            <fo:page-sequence master-reference='page'>
                <fo:flow flow-name='xsl-region-body'>
                    <fo:block font-size='24pt' font-weight='bold'
                        space-after='12pt'>{title}</fo:block>
                    <fo:block font-size='12pt'>{content}</fo:block>
                </fo:flow>
            </fo:page-sequence>
        </fo:root>";

    FonetDriver driver = FonetDriver.Make();
    using (MemoryStream output = new MemoryStream())
    {
        driver.Render(new StringReader(xslFo), output);
        return output.ToArray();
    }
}
```

**After (IronPDF with HTML):**
```csharp
using IronPdf;

public byte[] GenerateSimpleDocument(string title, string content)
{
    string html = $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    margin: 1in;
                }}
                h1 {{
                    font-size: 24pt;
                    font-weight: bold;
                    margin-bottom: 12pt;
                }}
                p {{ font-size: 12pt; }}
            </style>
        </head>
        <body>
            <h1>{title}</h1>
            <p>{content}</p>
        </body>
        </html>";

    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
    renderer.RenderingOptions.MarginTop = 25.4;   // 1 inch in mm
    renderer.RenderingOptions.MarginBottom = 25.4;
    renderer.RenderingOptions.MarginLeft = 25.4;
    renderer.RenderingOptions.MarginRight = 25.4;

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 2: Invoice with Table

**Before (FoNet with XSL-FO):**
```csharp
using Fonet;
using System.IO;
using System.Text;

public byte[] GenerateInvoice(Invoice invoice)
{
    var xslFo = new StringBuilder();
    xslFo.Append(@"<?xml version='1.0' encoding='utf-8'?>
        <fo:root xmlns:fo='http://www.w3.org/1999/XSL/Format'>
            <fo:layout-master-set>
                <fo:simple-page-master master-name='A4'
                    page-height='297mm' page-width='210mm'
                    margin='20mm'>
                    <fo:region-body margin-top='15mm' margin-bottom='15mm'/>
                    <fo:region-before extent='15mm'/>
                    <fo:region-after extent='15mm'/>
                </fo:simple-page-master>
            </fo:layout-master-set>
            <fo:page-sequence master-reference='A4'>
                <fo:static-content flow-name='xsl-region-after'>
                    <fo:block text-align='right' font-size='10pt'>
                        Page <fo:page-number/>
                    </fo:block>
                </fo:static-content>
                <fo:flow flow-name='xsl-region-body'>");

    xslFo.Append($@"
                    <fo:block font-size='18pt' font-weight='bold' space-after='10mm'>
                        Invoice #{invoice.Number}
                    </fo:block>
                    <fo:block space-after='5mm'>Date: {invoice.Date:yyyy-MM-dd}</fo:block>
                    <fo:block space-after='10mm'>Customer: {invoice.CustomerName}</fo:block>

                    <fo:table border='0.5pt solid black' width='100%'>
                        <fo:table-column column-width='40%'/>
                        <fo:table-column column-width='20%'/>
                        <fo:table-column column-width='20%'/>
                        <fo:table-column column-width='20%'/>
                        <fo:table-header>
                            <fo:table-row background-color='#333333' color='white'>
                                <fo:table-cell padding='3mm'><fo:block font-weight='bold'>Item</fo:block></fo:table-cell>
                                <fo:table-cell padding='3mm'><fo:block font-weight='bold'>Qty</fo:block></fo:table-cell>
                                <fo:table-cell padding='3mm'><fo:block font-weight='bold'>Price</fo:block></fo:table-cell>
                                <fo:table-cell padding='3mm'><fo:block font-weight='bold'>Total</fo:block></fo:table-cell>
                            </fo:table-row>
                        </fo:table-header>
                        <fo:table-body>");

    foreach (var item in invoice.Items)
    {
        xslFo.Append($@"
                            <fo:table-row>
                                <fo:table-cell padding='2mm' border='0.5pt solid black'>
                                    <fo:block>{item.Description}</fo:block>
                                </fo:table-cell>
                                <fo:table-cell padding='2mm' border='0.5pt solid black'>
                                    <fo:block text-align='center'>{item.Quantity}</fo:block>
                                </fo:table-cell>
                                <fo:table-cell padding='2mm' border='0.5pt solid black'>
                                    <fo:block text-align='right'>${item.UnitPrice:F2}</fo:block>
                                </fo:table-cell>
                                <fo:table-cell padding='2mm' border='0.5pt solid black'>
                                    <fo:block text-align='right'>${item.Total:F2}</fo:block>
                                </fo:table-cell>
                            </fo:table-row>");
    }

    xslFo.Append($@"
                        </fo:table-body>
                    </fo:table>
                    <fo:block space-before='10mm' text-align='right' font-size='14pt' font-weight='bold'>
                        Total: ${invoice.Total:F2}
                    </fo:block>
                </fo:flow>
            </fo:page-sequence>
        </fo:root>");

    FonetDriver driver = FonetDriver.Make();
    using (MemoryStream output = new MemoryStream())
    {
        driver.Render(new StringReader(xslFo.ToString()), output);
        return output.ToArray();
    }
}
```

**After (IronPDF with HTML):**
```csharp
using IronPdf;
using System.Text;

public byte[] GenerateInvoice(Invoice invoice)
{
    var html = new StringBuilder();
    html.Append($@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; padding: 20mm; }}
                h1 {{ font-size: 18pt; margin-bottom: 10mm; }}
                table {{ width: 100%; border-collapse: collapse; margin: 15mm 0; }}
                th {{ background: #333; color: white; padding: 3mm; font-weight: bold; }}
                td {{ border: 0.5px solid black; padding: 2mm; }}
                .right {{ text-align: right; }}
                .center {{ text-align: center; }}
                .total {{ font-size: 14pt; font-weight: bold; text-align: right; margin-top: 10mm; }}
            </style>
        </head>
        <body>
            <h1>Invoice #{invoice.Number}</h1>
            <p>Date: {invoice.Date:yyyy-MM-dd}</p>
            <p>Customer: {invoice.CustomerName}</p>

            <table>
                <thead>
                    <tr>
                        <th style='width:40%'>Item</th>
                        <th style='width:20%'>Qty</th>
                        <th style='width:20%'>Price</th>
                        <th style='width:20%'>Total</th>
                    </tr>
                </thead>
                <tbody>");

    foreach (var item in invoice.Items)
    {
        html.Append($@"
                    <tr>
                        <td>{item.Description}</td>
                        <td class='center'>{item.Quantity}</td>
                        <td class='right'>${item.UnitPrice:F2}</td>
                        <td class='right'>${item.Total:F2}</td>
                    </tr>");
    }

    html.Append($@"
                </tbody>
            </table>
            <p class='total'>Total: ${invoice.Total:F2}</p>
        </body>
        </html>");

    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.MarginTop = 20;
    renderer.RenderingOptions.MarginBottom = 20;

    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
    {
        HtmlFragment = "<div style='text-align:right; font-size:10pt;'>Page {page}</div>"
    };

    var pdf = renderer.RenderHtmlAsPdf(html.ToString());
    return pdf.BinaryData;
}
```

### Example 3: Multi-Page Report with Headers

**Before (FoNet with XSL-FO):**
```csharp
using Fonet;
using System.IO;

public void GenerateReport(string title, List<Section> sections)
{
    var xslFo = new StringBuilder();
    xslFo.Append($@"<?xml version='1.0' encoding='utf-8'?>
        <fo:root xmlns:fo='http://www.w3.org/1999/XSL/Format'>
            <fo:layout-master-set>
                <fo:simple-page-master master-name='report'
                    page-height='11in' page-width='8.5in'
                    margin='0.75in'>
                    <fo:region-body margin-top='0.5in' margin-bottom='0.5in'/>
                    <fo:region-before extent='0.5in'/>
                    <fo:region-after extent='0.5in'/>
                </fo:simple-page-master>
            </fo:layout-master-set>
            <fo:page-sequence master-reference='report'>
                <fo:static-content flow-name='xsl-region-before'>
                    <fo:block text-align='center' font-weight='bold' border-bottom='1pt solid black'>
                        {title}
                    </fo:block>
                </fo:static-content>
                <fo:static-content flow-name='xsl-region-after'>
                    <fo:block text-align='center' border-top='1pt solid black'>
                        Page <fo:page-number/> of <fo:page-number-citation ref-id='last-page'/>
                    </fo:block>
                </fo:static-content>
                <fo:flow flow-name='xsl-region-body'>");

    foreach (var section in sections)
    {
        xslFo.Append($@"
                    <fo:block font-size='16pt' font-weight='bold' space-before='15pt'
                        space-after='10pt' keep-with-next='always'>
                        {section.Title}
                    </fo:block>
                    <fo:block>{section.Content}</fo:block>");
    }

    xslFo.Append(@"
                    <fo:block id='last-page'/>
                </fo:flow>
            </fo:page-sequence>
        </fo:root>");

    FonetDriver driver = FonetDriver.Make();
    using (FileStream output = new FileStream("report.pdf", FileMode.Create))
    {
        driver.Render(new StringReader(xslFo.ToString()), output);
    }
}
```

**After (IronPDF with HTML):**
```csharp
using IronPdf;
using System.Text;

public void GenerateReport(string title, List<Section> sections)
{
    var html = new StringBuilder();
    html.Append(@"
        <html>
        <head>
            <style>
                body { font-family: Arial, sans-serif; }
                h2 {
                    font-size: 16pt;
                    margin-top: 15pt;
                    margin-bottom: 10pt;
                    page-break-after: avoid;
                }
                p { margin-bottom: 10pt; }
            </style>
        </head>
        <body>");

    foreach (var section in sections)
    {
        html.Append($@"
            <h2>{section.Title}</h2>
            <p>{section.Content}</p>");
    }

    html.Append("</body></html>");

    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
    renderer.RenderingOptions.MarginTop = 19;     // 0.75in
    renderer.RenderingOptions.MarginBottom = 19;
    renderer.RenderingOptions.MarginLeft = 19;
    renderer.RenderingOptions.MarginRight = 19;

    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
    {
        HtmlFragment = $"<div style='text-align:center; font-weight:bold; border-bottom:1px solid black; padding-bottom:5px;'>{title}</div>",
        MaxHeight = 15
    };

    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
    {
        HtmlFragment = "<div style='text-align:center; border-top:1px solid black; padding-top:5px;'>Page {page} of {total-pages}</div>",
        MaxHeight = 15
    };

    var pdf = renderer.RenderHtmlAsPdf(html.ToString());
    pdf.SaveAs("report.pdf");
}
```

### Example 4: Document with Images

**Before (FoNet with XSL-FO):**
```csharp
using Fonet;
using System.IO;

public void GenerateDocumentWithImage(string imagePath)
{
    string xslFo = $@"<?xml version='1.0' encoding='utf-8'?>
        <fo:root xmlns:fo='http://www.w3.org/1999/XSL/Format'>
            <fo:layout-master-set>
                <fo:simple-page-master master-name='page'>
                    <fo:region-body margin='25mm'/>
                </fo:simple-page-master>
            </fo:layout-master-set>
            <fo:page-sequence master-reference='page'>
                <fo:flow flow-name='xsl-region-body'>
                    <fo:block text-align='center'>
                        <fo:external-graphic src='url({imagePath})'
                            content-width='100mm' content-height='auto' scaling='uniform'/>
                    </fo:block>
                    <fo:block space-before='10mm' text-align='center'>
                        Company Logo
                    </fo:block>
                </fo:flow>
            </fo:page-sequence>
        </fo:root>";

    FonetDriver driver = FonetDriver.Make();
    driver.BaseDirectory = new DirectoryInfo(Path.GetDirectoryName(imagePath));
    using (FileStream output = new FileStream("output.pdf", FileMode.Create))
    {
        driver.Render(new StringReader(xslFo), output);
    }
}
```

**After (IronPDF with HTML):**
```csharp
using IronPdf;
using System;
using System.IO;

public void GenerateDocumentWithImage(string imagePath)
{
    // Option 1: Use file path with BaseUrl
    string html = $@"
        <html>
        <body style='text-align: center; margin: 25mm;'>
            <img src='{Path.GetFileName(imagePath)}' style='width: 100mm; height: auto;' />
            <p>Company Logo</p>
        </body>
        </html>";

    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.BaseUrl = new Uri(Path.GetDirectoryName(imagePath));
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");

    // Option 2: Embed image as Base64 (more portable)
    byte[] imageBytes = File.ReadAllBytes(imagePath);
    string base64 = Convert.ToBase64String(imageBytes);
    string extension = Path.GetExtension(imagePath).TrimStart('.');

    string htmlWithBase64 = $@"
        <html>
        <body style='text-align: center; margin: 25mm;'>
            <img src='data:image/{extension};base64,{base64}' style='width: 100mm; height: auto;' />
            <p>Company Logo</p>
        </body>
        </html>";

    var pdf2 = renderer.RenderHtmlAsPdf(htmlWithBase64);
    pdf2.SaveAs("output_embedded.pdf");
}
```

### Example 5: URL to PDF (Not Possible in FoNet)

**Before (FoNet - manual workaround required):**
```csharp
using Fonet;
using System.Net.Http;

public async Task<byte[]> GeneratePdfFromUrl(string url)
{
    // FoNet cannot render URLs directly
    // Must download HTML, convert to XSL-FO manually (complex!)
    using (var client = new HttpClient())
    {
        string html = await client.GetStringAsync(url);
        string xslFo = ConvertHtmlToXslFo(html);  // NOT built-in, extremely complex

        FonetDriver driver = FonetDriver.Make();
        using (MemoryStream output = new MemoryStream())
        {
            driver.Render(new StringReader(xslFo), output);
            return output.ToArray();
        }
    }
}

private string ConvertHtmlToXslFo(string html)
{
    // Would require building a complete HTML-to-XSL-FO converter
    // This is extremely complex and error-prone
    throw new NotImplementedException("HTML to XSL-FO conversion not built into FoNet");
}
```

**After (IronPDF - built-in support):**
```csharp
using IronPdf;

public byte[] GeneratePdfFromUrl(string url)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PrintHtmlBackgrounds = true;
    renderer.RenderingOptions.WaitFor.RenderDelay(2000);  // Wait for JS

    var pdf = renderer.RenderUrlAsPdf(url);
    return pdf.BinaryData;
}
```

### Example 6: PDF Security

**Before (FoNet - limited security):**
```csharp
// FoNet has very limited PDF security options
// Must use post-processing with another library
```

**After (IronPDF - comprehensive security):**
```csharp
using IronPdf;

public byte[] GenerateSecurePdf(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Set metadata
    pdf.MetaData.Title = "Confidential Report";
    pdf.MetaData.Author = "Company Name";

    // Password protection
    pdf.SecuritySettings.OwnerPassword = "owner123";
    pdf.SecuritySettings.UserPassword = "user456";

    // Restrict permissions
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;
    pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.NoPrint;
    pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;

    return pdf.BinaryData;
}
```

---

## Advanced Scenarios

### XSLT Transformation Alternative

If you were using XSLT with FoNet for XML data transformation:

```csharp
using IronPdf;
using System.Xml;
using System.Xml.Xsl;
using System.IO;

public byte[] GeneratePdfFromXml(string xmlData, string xsltPath)
{
    // Transform XML to HTML using XSLT (instead of XSL-FO)
    XslCompiledTransform xslt = new XslCompiledTransform();
    xslt.Load(xsltPath);

    using (StringReader xmlReader = new StringReader(xmlData))
    using (StringWriter htmlWriter = new StringWriter())
    {
        xslt.Transform(XmlReader.Create(xmlReader), null, htmlWriter);
        string html = htmlWriter.ToString();

        // Render HTML to PDF
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Batch PDF Generation

```csharp
using IronPdf;
using System.Threading.Tasks;

public async Task GenerateBatchPdfs(List<Document> documents)
{
    var renderer = new ChromePdfRenderer();  // Thread-safe, reuse

    await Parallel.ForEachAsync(documents, async (doc, ct) =>
    {
        string html = BuildHtmlForDocument(doc);
        var pdf = renderer.RenderHtmlAsPdf(html);
        await Task.Run(() => pdf.SaveAs($"output/{doc.Id}.pdf"));
    });
}
```

---

## Performance Considerations

### Reuse ChromePdfRenderer

```csharp
// GOOD - Reuse the renderer
public class PdfService
{
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    public byte[] Generate(string html) => _renderer.RenderHtmlAsPdf(html).BinaryData;
}

// BAD - Creating new instance each time
public byte[] GenerateBad(string html)
{
    var renderer = new ChromePdfRenderer();  // Wasteful
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Unit Conversion Helper

FoNet XSL-FO uses various units. Here's a helper for IronPDF (which uses mm):

```csharp
public static class UnitConverter
{
    public static double InchesToMm(double inches) => inches * 25.4;
    public static double PointsToMm(double points) => points * 0.352778;
    public static double PicasToMm(double picas) => picas * 4.233;
    public static double CmToMm(double cm) => cm * 10;
}

// Usage
renderer.RenderingOptions.MarginTop = UnitConverter.InchesToMm(1);  // 1 inch
```

---

## Troubleshooting

### Issue 1: XSL-FO Namespace References

**Problem**: Old code references `xmlns:fo='http://www.w3.org/1999/XSL/Format'`.

**Solution**: Remove all XSL-FO namespaces. Convert to HTML:

```csharp
// No namespace needed in HTML
string html = "<html><body><h1>Title</h1></body></html>";
```

### Issue 2: fo:block to HTML Mapping

**Problem**: Not sure what `<fo:block>` should become.

**Solution**: Use appropriate semantic HTML:
- Headings: `<h1>` through `<h6>`
- Paragraphs: `<p>`
- Generic containers: `<div>`
- Inline text: `<span>`

### Issue 3: Page Size Differences

**Problem**: PDF page size looks different.

**Solution**: Map XSL-FO page dimensions correctly:

```csharp
// XSL-FO: page-height='11in' page-width='8.5in' (Letter)
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;

// XSL-FO: page-height='297mm' page-width='210mm' (A4)
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

// Custom size (in mm)
renderer.RenderingOptions.SetCustomPaperSize(210, 297);
```

### Issue 4: Fonts Not Matching

**Problem**: Fonts look different from FoNet output.

**Solution**: Use web fonts or specify system fonts in CSS:

```html
<style>
    @import url('https://fonts.googleapis.com/css2?family=Roboto&display=swap');
    body { font-family: 'Roboto', Arial, sans-serif; }
</style>
```

### Issue 5: Region-Before/After Not Working

**Problem**: Headers/footers from `fo:static-content` not appearing.

**Solution**: Use IronPDF's `HtmlHeaderFooter`:

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div>Header Content</div>",
    MaxHeight = 25  // mm
};
```

### Issue 6: Page Numbers Not Working

**Problem**: `<fo:page-number/>` doesn't work.

**Solution**: Use IronPDF placeholders in headers/footers:
- `{page}` - current page number
- `{total-pages}` - total page count
- `{date}` - current date
- `{time}` - current time

### Issue 7: Keep-Together Not Working

**Problem**: Content breaks across pages unexpectedly.

**Solution**: Use CSS page break properties:

```css
.no-break { page-break-inside: avoid; }
.new-page { page-break-before: always; }
```

### Issue 8: Tables Looking Different

**Problem**: Table styling doesn't match XSL-FO output.

**Solution**: Use CSS border-collapse and explicit widths:

```css
table {
    border-collapse: collapse;
    width: 100%;
}
td, th {
    border: 1px solid black;
    padding: 8px;
}
```

---

## Migration Checklist

```markdown
## Migration Checklist

### Pre-Migration

- [ ] **Inventory all library usages in codebase**
  ```bash
  grep -r "using Fonet" --include="*.cs" .
  grep -r "FonetDriver\|FonetRenderer" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  ```csharp
  // Find patterns like:
  var driver = FonetDriver.Make();
  driver.Options = new FonetOptions {
      PageSize = "A4",
      Orientation = "Portrait"
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package Fonet
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace PDF generation calls**
  ```csharp
  // Before (Fonet)
  var driver = FonetDriver.Make();
  driver.Render("input.fo", "output.pdf");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf("<html><body>Your content here</body></html>");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering for accurate HTML/CSS support.

- [ ] **Convert URL rendering**
  ```csharp
  // Before (Fonet)
  // Not supported directly

  // After (IronPDF)
  var pdf = renderer.RenderUrlAsPdf("https://example.com");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Direct URL rendering with full JavaScript support.

- [ ] **Update page settings**
  ```csharp
  // Before (Fonet)
  driver.Options.PageSize = "A4";
  driver.Options.Orientation = "Landscape";

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Convert header/footer configuration**
  ```csharp
  // Before (Fonet)
  // Headers/footers configured via XSL-FO markup

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>{html-title}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders: {page}, {total-pages}, {date}, {time}, {html-title}.

- [ ] **Enable JavaScript if needed**
  ```csharp
  // Before (Fonet)
  // Not supported

  // After (IronPDF)
  renderer.RenderingOptions.EnableJavaScript = true;
  renderer.RenderingOptions.WaitFor.JavaScript(2000);
  ```
  **Why:** IronPDF's Chromium engine provides reliable JavaScript execution with configurable wait times.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Test JavaScript-heavy pages**
  **Why:** Dynamic content should now render more reliably with modern Chromium.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  // Merge PDFs
  var merged = PdfDocument.Merge(pdf1, pdf2);

  // Password protection
  pdf.SecuritySettings.UserPassword = "secret";

  // Watermarks
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");

  // Digital signatures
  var signature = new PdfSignature("cert.pfx", "password");
  pdf.Sign(signature);
  ```
  **Why:** IronPDF provides many features that may not have been available in the old library.
```
## Migration Checklist

### Pre-Migration

- [ ] **Inventory all `.fo` and `.xslfo` template files**
  **Why:** Identify all template files to ensure complete migration coverage.

- [ ] **Document page dimensions and margins for each template**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **List all fonts used in XSL-FO templates**
  **Why:** Ensure all fonts are available and mapped correctly in HTML/CSS for IronPDF.

- [ ] **Note header/footer configurations (fo:static-content)**
  **Why:** IronPDF uses HtmlHeaderFooter for headers/footers. Document current configurations for accurate migration.

- [ ] **Identify table structures and styling**
  **Why:** Tables will need to be converted to HTML `<table>` elements with corresponding CSS.

- [ ] **Backup project to version control**
  **Why:** Ensure you have a restore point in case of issues during migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Package Migration

- [ ] **Remove `Fonet` or `FO.NET` package**
  ```bash
  dotnet remove package Fonet
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Install `IronPdf` package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add the new library to your project for PDF generation.

- [ ] **Update namespace imports**
  ```csharp
  // Before (Fonet)
  using Fonet;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure the code references the correct library.

- [ ] **Set IronPDF license key at startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Template Conversion

- [ ] **Convert `<fo:block>` to HTML elements (`<p>`, `<div>`, `<h1>`)**
  ```html
  <!-- Before (.fo) -->
  <fo:block>Content</fo:block>

  <!-- After (HTML) -->
  <p>Content</p>
  ```
  **Why:** HTML elements are required for IronPDF rendering.

- [ ] **Convert `<fo:table>` to HTML `<table>`**
  ```html
  <!-- Before (.fo) -->
  <fo:table>
    <fo:table-row>
      <fo:table-cell>Cell</fo:table-cell>
    </fo:table-row>
  </fo:table>

  <!-- After (HTML) -->
  <table>
    <tr>
      <td>Cell</td>
    </tr>
  </table>
  ```
  **Why:** HTML tables are necessary for IronPDF rendering.

- [ ] **Convert `<fo:list-block>` to `<ul>` / `<ol>`**
  ```html
  <!-- Before (.fo) -->
  <fo:list-block>
    <fo:list-item>Item</fo:list-item>
  </fo:list-block>

  <!-- After (HTML) -->
  <ul>
    <li>Item</li>
  </ul>
  ```
  **Why:** HTML lists are required for IronPDF rendering.

- [ ] **Convert `<fo:external-graphic>` to `<img>`**
  ```html
  <!-- Before (.fo) -->
  <fo:external-graphic src="image.png"/>

  <!-- After (HTML) -->
  <img src="image.png"/>
  ```
  **Why:** HTML `<img>` tags are needed for IronPDF rendering.

- [ ] **Convert XSL-FO properties to CSS**
  ```css
  /* Before (.fo) */
  <fo:block font-size="12pt" color="red">Text</fo:block>

  /* After (CSS) */
  <p style="font-size: 12pt; color: red;">Text</p>
  ```
  **Why:** CSS is used for styling in IronPDF.

- [ ] **Map page dimensions to RenderingOptions**
  ```csharp
  // Before (.fo)
  <fo:simple-page-master page-height="29.7cm" page-width="21cm"/>

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** Ensure consistent page size in IronPDF.

- [ ] **Convert `fo:static-content` to HtmlHeaderFooter**
  ```csharp
  // Before (.fo)
  <fo:static-content>Header</fo:static-content>

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div>Header</div>"
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers.

- [ ] **Replace `<fo:page-number/>` with `{page}` placeholder**
  ```html
  <!-- Before (.fo) -->
  <fo:page-number/>

  <!-- After (IronPDF) -->
  {page}
  ```
  **Why:** Use IronPDF placeholders for dynamic content.

### Code Migration

- [ ] **Replace `FonetDriver.Make()` with `new ChromePdfRenderer()`**
  ```csharp
  // Before (Fonet)
  var driver = FonetDriver.Make();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** Use IronPDF's rendering engine.

- [ ] **Replace `driver.Render()` with `renderer.RenderHtmlAsPdf()`**
  ```csharp
  // Before (Fonet)
  driver.Render(foInput, outputStream);

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(htmlInput);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Render HTML directly to PDF with IronPDF.

- [ ] **Update file output from streams to `pdf.SaveAs()`**
  ```csharp
  // Before (Fonet)
  driver.Render(foInput, outputStream);

  // After (IronPDF)
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Simplify PDF saving process with IronPDF.

- [ ] **Replace error event handlers with try/catch**
  ```csharp
  // Before (Fonet)
  driver.OnError += HandleError;

  // After (IronPDF)
  try
  {
      var pdf = renderer.RenderHtmlAsPdf(htmlInput);
  }
  catch (Exception ex)
  {
      // Handle error
  }
  ```
  **Why:** Use standard exception handling for robustness.

- [ ] **Add PDF security if needed**
  ```csharp
  // After (IronPDF)
  pdf.SecuritySettings.UserPassword = "password";
  ```
  **Why:** Enhance document security with IronPDF's features.

### Testing

- [ ] **Compare output appearance to original FoNet PDFs**
  **Why:** Ensure visual consistency between old and new PDFs.

- [ ] **Verify page dimensions and margins**
  **Why:** Confirm layout accuracy after migration.

- [ ] **Check headers and footers**
  **Why:** Ensure headers/footers are correctly rendered.

- [ ] **Validate page numbers**
  **Why:** Confirm dynamic content like page numbers is accurate.

- [ ] **Test table rendering**
  **Why:** Verify tables are displayed correctly in the output PDF.

- [ ] **Verify image loading**
  **Why:** Ensure all images are correctly embedded in the PDF.

- [ ] **Test with various data sizes**
  **Why:** Confirm performance and correctness with different input sizes.

### Post-Migration

- [ ] **Delete `.fo` and `.xslfo` template files**
  **Why:** Remove obsolete files after successful migration.

- [ ] **Remove FoNet-related code and utilities**
  **Why:** Clean up codebase by removing unused dependencies.

- [ ] **Update documentation**
  **Why:** Ensure documentation reflects the new PDF generation process.

- [ ] **Clean up XSLT files if XSL-FO specific**
  **Why:** Remove any XSL-FO specific logic no longer needed.
```
---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **FoNet GitHub (archived)**: https://github.com/prepare/FO.NET

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
