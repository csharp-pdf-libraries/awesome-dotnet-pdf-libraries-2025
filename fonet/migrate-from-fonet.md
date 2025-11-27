# Migration Guide: FoNet (FO.NET) → IronPDF

## Why Migrate from FoNet to IronPDF

FoNet requires learning XSL-FO, an obsolete XML-based formatting language that lacks modern adoption and has a steep learning curve. IronPDF allows you to generate PDFs directly from HTML and CSS, technologies familiar to most developers, eliminating the need for XSL-FO expertise. Additionally, IronPDF provides superior rendering quality, better documentation, and active support for modern web standards.

## NuGet Package Changes

```xml
<!-- Remove FoNet -->
<PackageReference Include="FO.NET" Version="*" Remove />

<!-- Add IronPDF -->
<PackageReference Include="IronPdf" Version="2024.*" />
```

## Namespace Mapping

| FoNet Namespace | IronPDF Namespace |
|----------------|-------------------|
| `Fonet` | `IronPdf` |
| `Fonet.Pdf` | `IronPdf` |
| `Fonet.Render.Pdf` | `IronPdf` |

## API Mapping

| FoNet API | IronPDF API | Notes |
|-----------|-------------|-------|
| `FonetDriver.Make()` | `ChromePdfRenderer` | Main rendering engine |
| `driver.Render(xslfo, output)` | `renderer.RenderHtmlAsPdf(html)` | Render to PDF |
| XSL-FO XML Document | HTML string or file | Input format change |
| `FonetDriver.ApfMime` | N/A | Direct PDF generation |
| Stream output | `PdfDocument.SaveAs()` | Save to file/stream |
| XSL-FO transformations | HTML/CSS | Use web standards |

## Code Examples

### Example 1: Basic PDF Generation

**Before (FoNet with XSL-FO):**
```csharp
using Fonet;
using System.IO;
using System.Xml;

string xslfo = @"<?xml version='1.0' encoding='utf-8'?>
<fo:root xmlns:fo='http://www.w3.org/1999/XSL/Format'>
  <fo:layout-master-set>
    <fo:simple-page-master master-name='simple' page-height='11in' page-width='8.5in'>
      <fo:region-body margin='1in'/>
    </fo:simple-page-master>
  </fo:layout-master-set>
  <fo:page-sequence master-reference='simple'>
    <fo:flow flow-name='xsl-region-body'>
      <fo:block font-size='18pt' font-weight='bold'>Hello World</fo:block>
      <fo:block>This is my first PDF document.</fo:block>
    </fo:flow>
  </fo:page-sequence>
</fo:root>";

FonetDriver driver = FonetDriver.Make();
using (FileStream output = new FileStream("output.pdf", FileMode.Create))
{
    driver.Render(xslfo, output);
}
```

**After (IronPDF with HTML):**
```csharp
using IronPdf;

string html = @"
<h1 style='font-size: 18pt; font-weight: bold;'>Hello World</h1>
<p>This is my first PDF document.</p>";

ChromePdfRenderer renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: PDF from File

**Before (FoNet):**
```csharp
using Fonet;
using System.IO;
using System.Xml;

// Load XSL-FO file
XmlDocument doc = new XmlDocument();
doc.Load("template.fo");

FonetDriver driver = FonetDriver.Make();
using (FileStream output = new FileStream("invoice.pdf", FileMode.Create))
{
    driver.Render(doc, output);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

ChromePdfRenderer renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlFileAsPdf("template.html");
pdf.SaveAs("invoice.pdf");
```

### Example 3: PDF with Styling and Tables

**Before (FoNet with XSL-FO):**
```csharp
using Fonet;
using System.IO;

string xslfo = @"<?xml version='1.0' encoding='utf-8'?>
<fo:root xmlns:fo='http://www.w3.org/1999/XSL/Format'>
  <fo:layout-master-set>
    <fo:simple-page-master master-name='A4' page-height='297mm' page-width='210mm'>
      <fo:region-body margin='20mm'/>
    </fo:simple-page-master>
  </fo:layout-master-set>
  <fo:page-sequence master-reference='A4'>
    <fo:flow flow-name='xsl-region-body'>
      <fo:table>
        <fo:table-column column-width='50mm'/>
        <fo:table-column column-width='50mm'/>
        <fo:table-body>
          <fo:table-row>
            <fo:table-cell border='1pt solid black' padding='2mm'>
              <fo:block>Product</fo:block>
            </fo:table-cell>
            <fo:table-cell border='1pt solid black' padding='2mm'>
              <fo:block>Price</fo:block>
            </fo:table-cell>
          </fo:table-row>
          <fo:table-row>
            <fo:table-cell border='1pt solid black' padding='2mm'>
              <fo:block>Widget</fo:block>
            </fo:table-cell>
            <fo:table-cell border='1pt solid black' padding='2mm'>
              <fo:block>$10.00</fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>
    </fo:flow>
  </fo:page-sequence>
</fo:root>";

FonetDriver driver = FonetDriver.Make();
using (FileStream output = new FileStream("table.pdf", FileMode.Create))
{
    driver.Render(xslfo, output);
}
```

**After (IronPDF with HTML/CSS):**
```csharp
using IronPdf;

string html = @"
<style>
  table { border-collapse: collapse; margin: 20mm; }
  td { border: 1px solid black; padding: 2mm; }
</style>
<table>
  <tr>
    <td>Product</td>
    <td>Price</td>
  </tr>
  <tr>
    <td>Widget</td>
    <td>$10.00</td>
  </tr>
</table>";

ChromePdfRenderer renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("table.pdf");
```

## Common Gotchas

### 1. **No More XSL-FO Transformations**
FoNet requires XSL-FO XML structure with specific namespaces. IronPDF uses standard HTML/CSS instead. Convert your XSL-FO templates to HTML equivalents.

### 2. **Page Sizing Differences**
FoNet uses `page-height` and `page-width` attributes in XSL-FO. IronPDF uses CSS `@page` rules or `ChromePdfRenderOptions`:
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
```

### 3. **Stream Handling**
FoNet renders directly to an output stream. IronPDF returns a `PdfDocument` object that must be explicitly saved:
```csharp
// IronPDF approach
PdfDocument pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf"); // or use pdf.Stream for in-memory
```

### 4. **Font Handling**
FoNet requires font metrics configuration. IronPDF automatically handles system fonts and supports custom fonts via CSS `@font-face`.

### 5. **Licensing**
IronPDF requires a license key for production use:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 6. **Asynchronous Operations**
IronPDF supports async methods for better performance:
```csharp
PdfDocument pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

## Additional Resources

- **Documentation:** https://ironpdf.com/docs/
- **Tutorials:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/docs/
- **API Reference:** https://ironpdf.com/docs/