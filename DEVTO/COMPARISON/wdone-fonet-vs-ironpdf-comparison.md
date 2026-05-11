---
title: "FO.NET vs IronPDF: an unbiased look for .NET teams"
published: false
tags: dotnet, csharp, pdf, comparison
canonical_url: https://ironsoftware.com/suite/blog/comparison/
---
# FO.NET vs IronPDF: an unbiased look for .NET teams

Picture a scenario common in enterprise .NET environments: a report generation system built in 2012 using FO.NET suddenly needs updates for compliance reporting, but the original library has been dormant on CodePlex for nearly a decade, the maintainer unreachable, and .NET 6 deployment throws compatibility warnings. This isn't hypothetical—teams encounter this situation when legacy XSL-FO pipelines collide with modern infrastructure requirements. FO.NET was designed for precise XML-based page layout, targeting applications where data arrives as XML and must render with millimeter-level positioning accuracy.

IronPDF approaches document generation from the opposite direction: it assumes your layout is expressed in HTML/CSS (the universal design language of the web) and renders PDFs using a Chromium engine. This architectural choice reflects the reality of 2026 .NET development, where most teams already use HTML for web UIs and prefer reusing those skills for PDF generation rather than learning XSL-FO's specialized syntax.

## Understanding IronPDF

IronPDF is a .NET library built around HTML-to-PDF conversion, leveraging Chromium's rendering engine to produce PDFs that match browser output. Teams adopt it when they need to convert web pages, Razor views, or dynamically generated HTML into distributable documents. The library handles modern CSS (Grid, Flexbox), JavaScript execution, and web fonts, ensuring that complex layouts render correctly without manual coordinate positioning.

Beyond rendering, IronPDF provides full PDF lifecycle management: merging documents, extracting text, filling forms, applying digital signatures, encrypting files, and manipulating pages. The API is designed for developers already familiar with HTML/CSS, eliminating the learning curve associated with XSL-FO's formatting object model.

## Key Limitations of FO.NET

### Product Status
FO.NET is effectively discontinued. The original CodePlex project is defunct. Several GitHub forks exist (nholik, hahmed, leekelleher) but show minimal activity post-2018. No official maintainer, no roadmap, no security patches. Teams using FO.NET are maintaining legacy code with no upstream support.

### Missing Capabilities
No .NET Core or .NET 5+ support in any known fork. XSL-FO spec is frozen (W3C Recommendation from 2006); no modern CSS features. Cannot render HTML directly—requires XSLT transformation to XSL-FO first. No built-in support for interactive PDF forms, digital signatures, or modern encryption standards (AES-256).

### Technical Issues
XSL-FO learning curve is steep: developers must learn formatting objects (`fo:block`, `fo:table`, `fo:page-sequence`) instead of reusing HTML/CSS knowledge. XSLT transformations add a pre-processing step. Font embedding is manual and error-prone. Limited Unicode support depending on font configuration. Performance degrades with large documents or complex tables.

### Support Status
No commercial support available. Community support via GitHub forks is sporadic. Documentation is primarily the W3C XSL-FO spec plus Apache FOP guides (Java-focused). Troubleshooting requires forum archaeology or source code diving.

### Architecture Problems
Two-stage pipeline (XML → XSL-FO → PDF) adds failure points. XSLT debugging is notoriously difficult. Cannot leverage modern web design tools (browser DevTools, CSS frameworks). Tight coupling to XML data sources—JSON or object models require conversion. No browser-based preview; must generate PDF to see results.

## Feature Comparison Overview

| Aspect | FO.NET | IronPDF |
|--------|--------|---------|
| **Current Status** | Discontinued/unmaintained forks | Active commercial development |
| **HTML Support** | No (XSL-FO only) | Full HTML5/CSS3/JavaScript |
| **Rendering Quality** | Coordinate-based precision | Chromium pixel-perfect |
| **Installation** | Manual DLL or unofficial NuGet | Official NuGet, self-contained |
| **Support** | None | Commercial SLA available |
| **Future Viability** | Legacy maintenance only | Continuous updates |

## Troubleshooting Scenario: Page Break Issues

### FO.NET — XSL-FO Page Break Control

```xml
<!-- XSL-FO document structure (verify syntax in W3C spec) -->
<fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
  <fo:layout-master-set>
    <fo:simple-page-master master-name="A4">
      <fo:region-body margin="1in"/>
    </fo:simple-page-master>
  </fo:layout-master-set>

  <fo:page-sequence master-reference="A4">
    <fo:flow flow-name="xsl-region-body">

      <fo:block font-size="18pt" space-after="12pt">
        Section 1: Introduction
      </fo:block>
      <fo:block>Content for section 1...</fo:block>

      <!-- Force page break -->
      <fo:block break-before="page"/>

      <fo:block font-size="18pt" space-after="12pt">
        Section 2: Details
      </fo:block>
      <fo:block>Content for section 2...</fo:block>

      <!-- Keep content together on same page -->
      <fo:block keep-together="always">
        <fo:block font-weight="bold">Critical Data:</fo:block>
        <fo:table>
          <fo:table-column column-width="3in"/>
          <fo:table-column column-width="2in"/>
          <fo:table-body>
            <fo:table-row>
              <fo:table-cell><fo:block>Metric A</fo:block></fo:table-cell>
              <fo:table-cell><fo:block>Value 1</fo:block></fo:table-cell>
            </fo:table-row>
          </fo:table-body>
        </fo:table>
      </fo:block>

    </fo:flow>
  </fo:page-sequence>
</fo:root>
```

```csharp
// C# code to process XSL-FO (conceptual - verify API in specific FO.NET fork)
// Note: Exact implementation varies by fork and may not work on modern .NET
using System;
using System.IO;
using System.Xml;

namespace FoNetExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Typical workflow (verify against fork documentation):
            // 1. Load XSL-FO document
            // 2. Create FO processor
            // 3. Render to PDF

            // WARNING: This is conceptual code
            // FO.NET forks have inconsistent APIs
            // Refer to specific fork documentation

            try
            {
                var foDocument = new XmlDocument();
                foDocument.Load("report.fo");

                // FO.NET processor creation (API varies by fork)
                // Some forks use: FoNet.Render.Pdf.PdfRenderer
                // Documentation is sparse - expect trial and error

                Console.WriteLine("Check fork-specific documentation for exact API");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
```

**Common troubleshooting issues:**

- **Page break ignored**: XSL-FO `break-before` may not trigger if preceding content doesn't fill the page; use `fo:block` with explicit height to force breaks
- **Content split unexpectedly**: `keep-together="always"` can cause overflow errors if content exceeds page height; no automatic graceful degradation
- **Table rendering issues**: Column widths must be explicitly set; auto-sizing doesn't work reliably
- **Font embedding failures**: Fonts must be registered manually; missing fonts cause silent substitution or rendering errors
- **Namespace errors**: XSL-FO namespace must match exactly: `http://www.w3.org/1999/XSL/Format`
- **No preview workflow**: Must generate full PDF to see layout; no incremental rendering

### IronPDF — CSS-Based Page Control

```csharp
using IronPdf;
using System;

namespace IronPdfPageBreakExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var htmlWithPageBreaks = @"
                <html>
                <head>
                    <style>
                        body { font-family: Arial, sans-serif; }
                        h1 { font-size: 18pt; margin-bottom: 12pt; }
                        .page-break { page-break-before: always; }
                        .keep-together { page-break-inside: avoid; }
                        table { border-collapse: collapse; width: 100%; }
                        td { border: 1px solid #ccc; padding: 8px; }
                    </style>
                </head>
                <body>
                    <h1>Section 1: Introduction</h1>
                    <p>Content for section 1...</p>

                    <div class='page-break'></div>

                    <h1>Section 2: Details</h1>
                    <p>Content for section 2...</p>

                    <div class='keep-together'>
                        <strong>Critical Data:</strong>
                        <table>
                            <tr><td>Metric A</td><td>Value 1</td></tr>
                            <tr><td>Metric B</td><td>Value 2</td></tr>
                        </table>
                    </div>
                </body>
                </html>";

            var renderer = new ChromePdfRenderer();
            renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
            renderer.RenderingOptions.MarginTop = 72; // 1 inch = 72 points
            renderer.RenderingOptions.MarginBottom = 72;

            var pdf = renderer.RenderHtmlAsPdf(htmlWithPageBreaks);
            pdf.SaveAs("SectionedReport.pdf");

            Console.WriteLine("PDF created with controlled page breaks");
            pdf.Dispose();
        }
    }
}
```

CSS `page-break-before` and `page-break-inside` work as expected in browsers, making layout predictable. For complex page break scenarios—widow/orphan control, running headers—see [IronPDF's HTML to PDF guide](https://ironsoftware.com/suite/blog/using-ironsuite/html-to-pdf-ironpdf-tutorial/).

## Troubleshooting Scenario: Font Rendering Problems

### FO.NET — Manual Font Configuration

```xml
<!-- XSL-FO font configuration (verify exact syntax in fork docs) -->
<fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
  <fo:layout-master-set>
    <fo:simple-page-master master-name="main">
      <fo:region-body margin="1in"/>
    </fo:simple-page-master>
  </fo:layout-master-set>

  <fo:page-sequence master-reference="main">
    <fo:flow flow-name="xsl-region-body">

      <fo:block font-family="Arial" font-size="12pt">
        Standard Arial text
      </fo:block>

      <fo:block font-family="CustomFont" font-size="14pt">
        Custom font text (requires font file registration)
      </fo:block>

      <fo:block font-family="Arial" font-weight="bold">
        Bold text
      </fo:block>

    </fo:flow>
  </fo:page-sequence>
</fo:root>
```

**Common font troubleshooting:**

- **Font not found**: FO.NET doesn't auto-discover system fonts; must register font files in configuration (exact method varies by fork)
- **Fallback substitution**: If specified font missing, FO.NET silently uses default sans-serif; no warning or error
- **Bold/italic failures**: Font variations (Arial Bold, Arial Italic) must be registered separately as distinct font families
- **Unicode characters missing**: Font must contain glyphs for characters used; limited Unicode support depending on font
- **Embedding issues**: PDF may not embed fonts correctly, causing rendering problems on different systems
- **Configuration file complexity**: Font registration typically requires XML config file with exact paths to TTF files

### IronPDF — Automatic Font Handling

```csharp
using IronPdf;
using System;

namespace IronPdfFontExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var htmlWithFonts = @"
                <html>
                <head>
                    <style>
                        body { font-family: Arial, sans-serif; }
                        .custom-font { font-family: 'Roboto', Arial, sans-serif; }
                        .bold-text { font-weight: bold; }
                    </style>
                    <link href='https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&display=swap'
                          rel='stylesheet'>
                </head>
                <body>
                    <p>Standard Arial text</p>
                    <p class='custom-font'>Custom Roboto font text</p>
                    <p class='bold-text'>Bold text</p>
                    <p>Unicode test: こんにちは 你好 مرحبا</p>
                </body>
                </html>";

            var renderer = new ChromePdfRenderer();
            renderer.RenderingOptions.PrintHtmlBackgrounds = true;

            var pdf = renderer.RenderHtmlAsPdf(htmlWithFonts);
            pdf.SaveAs("FontTestReport.pdf");

            Console.WriteLine("PDF with various fonts created");
            pdf.Dispose();
        }
    }
}
```

IronPDF's Chromium engine handles fonts like a browser: system fonts work automatically, web fonts load from CDNs, and Unicode support is comprehensive. No manual configuration required.

## Troubleshooting Scenario: Dynamic Table Generation

### FO.NET — XSL-FO Table Construction

```xml
<!-- XSL-FO table with data binding (typically generated via XSLT) -->
<fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
  <fo:layout-master-set>
    <fo:simple-page-master master-name="table-page">
      <fo:region-body margin="0.5in"/>
    </fo:simple-page-master>
  </fo:layout-master-set>

  <fo:page-sequence master-reference="table-page">
    <fo:flow flow-name="xsl-region-body">

      <fo:table table-layout="fixed" width="100%">
        <fo:table-column column-width="proportional-column-width(2)"/>
        <fo:table-column column-width="proportional-column-width(1)"/>
        <fo:table-column column-width="proportional-column-width(1)"/>

        <fo:table-header>
          <fo:table-row background-color="#cccccc">
            <fo:table-cell border="1pt solid black" padding="4pt">
              <fo:block font-weight="bold">Product</fo:block>
            </fo:table-cell>
            <fo:table-cell border="1pt solid black" padding="4pt">
              <fo:block font-weight="bold">Quantity</fo:block>
            </fo:table-cell>
            <fo:table-cell border="1pt solid black" padding="4pt">
              <fo:block font-weight="bold">Price</fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-header>

        <fo:table-body>
          <!-- Data rows - typically generated via XSLT loop -->
          <fo:table-row>
            <fo:table-cell border="1pt solid black" padding="4pt">
              <fo:block>Item A</fo:block>
            </fo:table-cell>
            <fo:table-cell border="1pt solid black" padding="4pt">
              <fo:block>100</fo:block>
            </fo:table-cell>
            <fo:table-cell border="1pt solid black" padding="4pt">
              <fo:block>$50.00</fo:block>
            </fo:table-cell>
          </fo:table-row>
        </fo:table-body>
      </fo:table>

    </fo:flow>
  </fo:page-sequence>
</fo:root>
```

**Common table troubleshooting:**

- **Column width calculation errors**: `proportional-column-width()` doesn't always distribute space as expected; fixed widths more reliable
- **Border rendering gaps**: Borders may not align correctly at cell intersections; workaround involves careful padding/border settings
- **Header repetition on page breaks**: `fo:table-header` should repeat on each page, but implementation varies by fork
- **Cell content overflow**: Long text in cells doesn't wrap automatically; must use `wrap-option="wrap"` and test thoroughly
- **Performance degradation**: Large tables (1000+ rows) can cause severe slowdowns; XSL-FO processors aren't optimized for data tables
- **No auto-sizing**: Column widths must be calculated manually; no "fit content to column" feature

### IronPDF — HTML Table Rendering

```csharp
using IronPdf;
using System;
using System.Text;

namespace IronPdfTableExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var tableHtml = GenerateTableHtml(1000); // Generate large table

            var renderer = new ChromePdfRenderer();
            renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

            var pdf = renderer.RenderHtmlAsPdf(tableHtml);
            pdf.SaveAs("LargeDataTable.pdf");

            Console.WriteLine("Large table PDF created");
            pdf.Dispose();
        }

        static string GenerateTableHtml(int rowCount)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<html><head><style>");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; }");
            sb.AppendLine("th { background-color: #ccc; padding: 8px; border: 1px solid black; }");
            sb.AppendLine("td { padding: 8px; border: 1px solid black; }");
            sb.AppendLine("thead { display: table-header-group; }"); // Repeat header
            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr><th>Product</th><th>Quantity</th><th>Price</th></tr></thead>");
            sb.AppendLine("<tbody>");

            for (int i = 0; i < rowCount; i++)
            {
                sb.AppendLine($"<tr><td>Item {i}</td><td>{i * 10}</td><td>${i * 5.50:F2}</td></tr>");
            }

            sb.AppendLine("</tbody></table></body></html>");
            return sb.ToString();
        }
    }
}
```

HTML tables handle automatic column sizing, text wrapping, and header repetition without manual intervention. The `thead` element automatically repeats on page breaks.

## API Mapping Reference

| FO.NET Operation | XSL-FO Syntax | IronPDF Equivalent | Notes |
|------------------|---------------|-------------------|-------|
| Page setup | `<fo:simple-page-master>` | `RenderingOptions.PaperSize` | XSL-FO: XML config; IronPDF: property |
| Margins | `<fo:region-body margin="">` | `RenderingOptions.MarginTop/Bottom/etc` | FO.NET: layout master; IronPDF: options |
| Font selection | `font-family="Arial"` | CSS `font-family: Arial;` | XSL-FO: attribute; IronPDF: CSS |
| Bold text | `font-weight="bold"` | CSS `font-weight: bold;` | Same concept, different syntax |
| Page breaks | `break-before="page"` | CSS `page-break-before: always;` | XSL-FO: attribute; IronPDF: CSS |
| Keep content together | `keep-together="always"` | CSS `page-break-inside: avoid;` | Different property names |
| Tables | `<fo:table>` structure | `<table>` HTML element | XSL-FO: verbose XML; IronPDF: standard HTML |
| Table columns | `<fo:table-column>` | CSS `width` on `<th>`/`<td>` | FO.NET: explicit; IronPDF: auto-sizing |
| Headers/footers | `<fo:static-content>` | `RenderingOptions.TextHeader/Footer` | FO.NET: XML; IronPDF: API |
| Images | `<fo:external-graphic>` | `<img>` HTML tag | XSL-FO: element; IronPDF: standard tag |
| Hyperlinks | `<fo:basic-link>` | `<a href="">` HTML | FO.NET: FO object; IronPDF: HTML |
| Unicode support | Font-dependent | Built-in | FO.NET: manual; IronPDF: automatic |

## Comprehensive Feature Comparison

| Feature | FO.NET | IronPDF |
|---------|--------|---------|
| **Status** | | |
| Active Development | No (discontinued) | Yes |
| Last Update | 2017-2018 (varies by fork) | Regular updates |
| Official Maintainer | None | Iron Software |
| **Support** | | |
| Community Forums | Dead GitHub repos | Active support |
| Commercial SLA | No | Available |
| Documentation | W3C spec + Apache FOP | Comprehensive |
| Code Samples | Sparse | Extensive |
| **Content Creation** | | |
| Layout System | XSL-FO (XML) | HTML/CSS |
| Modern CSS Support | No | Full CSS3 |
| JavaScript Support | No | Yes |
| HTML Input | No | Yes |
| XML Input | Yes (via XSLT) | Via HTML conversion |
| **PDF Operations** | | |
| Generate PDF | Yes | Yes |
| Merge PDFs | No | Yes |
| Split PDFs | No | Yes |
| Extract Text | No | Yes |
| Edit Existing PDFs | No | Yes |
| Form Filling | Limited | Yes |
| Digital Signatures | No | Yes |
| **Security** | | |
| Encryption | Basic | AES 128/256-bit |
| Password Protection | Basic | User/Owner passwords |
| Permissions | Limited | Full control |
| **Known Issues** | | |
| .NET Core support | No | Yes |
| .NET 5+ support | No | Yes |
| Font embedding | Manual | Automatic |
| Large document performance | Poor | Optimized |
| Table auto-sizing | No | Yes |
| Unicode rendering | Limited | Full |
| **Development** | | |
| .NET Framework | 4.0-4.6 (fork-dependent) | 4.6.2+ |
| .NET Core | No | 3.1+ |
| .NET 5+ | No | Yes |
| Linux | Unknown | Yes |
| Docker | Problematic | Yes |

## Known FO.NET Issues

Based on historical reports and fork documentation gaps:

1. **Memory leaks**: Long-running processes may accumulate memory; no official fix
2. **Threading issues**: Not thread-safe; requires external synchronization
3. **Image format limitations**: Limited support beyond JPEG/PNG; TIFF/BMP problematic
4. **Complex table crashes**: Tables with merged cells or nested tables cause errors
5. **SVG rendering**: Minimal or no SVG support depending on fork

These issues vary by fork and may not be documented. Verify behavior through testing.

## Installation Comparison

### FO.NET

```bash
# No official NuGet package
# Manual DLL reference or fork-specific package
# Example for unofficial fork package:
# dotnet add package Fonet --version X.X.X
```

```csharp
// Namespace varies by fork
// Commonly: using Fonet
// or: using FO.NET
// Verify in fork documentation
```

### IronPDF

```bash
dotnet add package IronPdf
```

```csharp
using IronPdf;
```

## Conclusion

FO.NET served a specific need in the .NET ecosystem circa 2006-2012: teams with XML data pipelines who required precise, declarative page layout via XSL-FO standards. The library's dormancy reflects broader industry shifts—HTML/CSS emerged as the dominant layout language, XML processing moved to JSON/REST APIs, and .NET Framework-only libraries became incompatible with cloud-native architectures.

For existing FO.NET codebases, the migration decision hinges on pain points: if you're encountering .NET Core incompatibility, font rendering issues, or need features like digital signatures, the technical debt of maintaining FO.NET outweighs refactoring costs. IronPDF's HTML-based approach means most teams already possess the skills (HTML/CSS) to rebuild layouts, eliminating XSL-FO's learning curve.

Migration becomes mandatory when: deployment targets shift to .NET 5+; compliance requires modern encryption/signatures; or vendor-lock to unmaintained code becomes a security audit flag. In these cases, IronPDF's Chromium engine and active development provide a supported path forward.

Have you encountered XSL-FO codebases in legacy projects? What triggered the decision to migrate or maintain?

**Learn IronPDF's HTML-to-PDF workflow**: [Complete HTML to PDF tutorial](https://ironsoftware.com/suite/blog/using-ironsuite/html-to-pdf-ironpdf-tutorial/)
**Explore IronPDF's full API capabilities**: [C# PDF generation guide](https://ironsoftware.com/suite/blog/using-ironsuite/csharp-create-pdf-ironpdf-tutorial/)
