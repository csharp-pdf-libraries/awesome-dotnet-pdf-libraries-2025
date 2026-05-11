---
title: "Spire.PDF vs IronPDF: the practical breakdown for .NET"
published: false
tags: dotnet, csharp, pdf, comparison
---

A common onboarding question on .NET projects with a legacy PDF stack: "Can we just generate these from our existing HTML templates?" If the answer involves rewriting templates into a library-specific drawing API, the integration cost balloons before any feature work begins. The 2026 question isn't "does the library create PDFs?" — it's "does it work with the content you already have?"

Spire.PDF (commercial, from E-iceblue) is a PDF library that covers creating PDFs programmatically, editing existing documents, and converting HTML/images/text to PDF. There is also a separate **Free Spire.PDF (Community Edition)** SKU with a 10-page output cap. Historically the HTML-to-PDF path has not been Chromium-based — older Spire versions render via the Windows Internet Explorer/Edge Legacy WebBrowser control or an optional QT/WebKit plugin, and as of Spire.PDF 10.7.21 there is an opt-in `ChromeHtmlConverter` (`Spire.Additions.Chrome`) that shells out to a locally installed Google Chrome. This comparison walks through what Spire.PDF does well, where IronPDF's bundled-Chromium HTML-first approach differs, and which integration model is a better fit per scenario.

## Understanding IronPDF

IronPDF is purpose-built for HTML-to-PDF conversion using Google Chrome's rendering engine, with additional PDF manipulation capabilities (merge, split, sign, secure, extract). The [Chrome-based rendering approach](https://ironpdf.com/tutorials/html-to-pdf/) means your HTML renders pixel-perfectly as PDF—if it looks right in Chrome DevTools, it looks right in the PDF. IronPDF supports .NET 10, 9, 8, 7, 6, Core, Framework, and runs on Windows, Linux, macOS, Docker, and cloud platforms.

The library's workflow optimizes for a specific scenario: you already have HTML (web views, templates, generated content), and you need it as PDF. No learning a new document layout API; no rewriting content. Write HTML, call `RenderHtmlAsPdf()`, get PDF. For teams building web applications, this workflow reduces PDF integration to a rendering step, not a development project.

## Key Architectural Differences

### Editions and Licensing
Spire.PDF ships as two distinct SKUs: the commercial **Spire.PDF** product (paid tiers from per-developer through site/OEM) and **Free Spire.PDF (Community Edition)**, which has a documented 10-page cap on loading/creating PDFs and a 3-page cap when converting to image/Word/HTML/XPS. Unlicensed/evaluation runs of the commercial edition typically include an evaluation notice on output. Commercial licensing has multiple tiers with deployment scope restrictions (e.g., OEM tier is required for public-facing/SaaS/Docker scenarios) — verify the current tier list and price at the vendor's site before purchase.

### HTML Engine
The default `PdfDocument.LoadFromHTML(...)` path historically uses the system Internet Explorer/Edge Legacy WebBrowser control on the non-plugin code path, with an optional QtWebKit plugin available as a separate download. Spire.PDF 10.7.21 added `ChromeHtmlConverter` under `Spire.Additions.Chrome` as an opt-in path that requires a system-installed Google Chrome. Modern CSS features such as Flexbox, CSS Grid, and custom properties typically render with limited fidelity on the legacy engines; JavaScript execution on those paths is partial. The Chrome converter improves fidelity but adds an external Chrome dependency.

### Drawing API
Creating PDFs from scratch uses Spire-specific objects (`PdfDocument`, `PdfPageBase`, `PdfFont`, `PdfBrush`, `PdfTable`, etc.). The drawing API is low-level and precise but requires learning a PDF-specific surface rather than reusing HTML/CSS.

### Footprint
Spire.PDF distributes as multiple assemblies (e.g., `Spire.Pdf.dll`, `Spire.License.dll`); installed footprint is typically in the 20–30 MB range. IronPDF bundles its Chromium runtime, which is heavier on disk but eliminates the system-Chrome dependency.

## Feature Comparison Overview

| Aspect | Spire.PDF | IronPDF |
|--------|-----------|---------|
| **Current Status** | Active (commercial, E-iceblue) | Active (commercial, Iron Software) |
| **HTML Engine** | IE/Edge Legacy or QtWebKit (default); opt-in `ChromeHtmlConverter` since v10.7.21 (requires system Chrome) | Bundled Chromium |
| **Rendering Model** | Proprietary PDF engine + chosen HTML path | Chrome-rendering pipeline |
| **Installation** | Multiple assemblies (~20-30 MB) | Single NuGet with bundled native runtime |
| **Free SKU** | Free Spire.PDF: 10-page load/create cap, 3-page cap for image/Word/HTML/XPS conversion | 30-day commercial trial |

---

## Code Comparison Checklist

### ☑ **Task: Convert HTML String to PDF**

#### Spire.PDF Implementation

```csharp
// NuGet: Install-Package Spire.PDF
using Spire.Pdf;
using Spire.Pdf.Graphics;

public class SpirePdfHtmlConverter
{
    public void ConvertHtmlToPdf(string html, string outputPath)
    {
        PdfDocument doc = new PdfDocument();

        PdfPageSettings settings = new PdfPageSettings();
        settings.Size = PdfPageSize.A4;

        PdfHtmlLayoutFormat layout = new PdfHtmlLayoutFormat();
        layout.IsWaiting = false;

        // HTML-string overload (4 args):
        //   LoadFromHTML(string html, bool autoDetectPageBreak,
        //                PdfPageSettings setting, PdfHtmlLayoutFormat layout)
        // The 4-bool form (url, enableJS, enableHyperlinks, autoDetectPageBreak)
        // is the URL overload, not the string overload.
        doc.LoadFromHTML(html, true, settings, layout);

        doc.SaveToFile(outputPath, FileFormat.PDF);
        doc.Close();
    }
}
```

**Spire.PDF HTML conversion notes:**
- HTML string is supported directly via the 4-arg `LoadFromHTML` overload
- Modern CSS (Flexbox, Grid, custom properties) renders with limited fidelity on the legacy IE/QtWebKit paths; the opt-in `ChromeHtmlConverter` improves fidelity but requires Chrome installed
- JavaScript execution on the legacy paths is partial; `PdfHtmlLayoutFormat.IsWaiting` provides limited wait control
- Web fonts via `@font-face` behavior varies by engine — verify against your version
- Free Spire.PDF caps loading/creating at 10 pages and conversion to image/Word/HTML/XPS at 3 pages

#### IronPDF Implementation

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

public void ConvertHtmlToPdf(string html, string outputPath)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

**IronPDF HTML conversion notes:**
- HTML string input is direct (no temp file)
- Bundled Chromium — no system Chrome dependency
- Full CSS Grid / Flexbox support via the Chromium engine
- JavaScript execution supported (toggle via `RenderingOptions.EnableJavaScript`)
- Web fonts work via `@font-face`
- Auto page sizing from HTML; explicit size via `PaperSize`

[View IronPDF HTML rendering documentation](https://ironpdf.com/tutorials/html-to-pdf/)

---

### ☑ **Task: Create PDF from Scratch with Text and Tables**

#### Spire.PDF Implementation

```csharp
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Tables;
using System.Data;
using System.Drawing;

public void CreatePdfFromScratch(string outputPath)
{
    // Create new PDF document
    PdfDocument doc = new PdfDocument();
    
    // Add page
    PdfPageBase page = doc.Pages.Add(PdfPageSize.A4);
    
    // Create font
    PdfFont font = new PdfFont(PdfFontFamily.Helvetica, 12f);
    PdfFont titleFont = new PdfFont(PdfFontFamily.Helvetica, 20f);
    
    // Create brush for text
    PdfBrush brush = PdfBrushes.Black;
    
    // Draw title
    page.Canvas.DrawString("Invoice Report", titleFont, brush, 
        new PointF(50, 50));
    
    // Create table (Spire-specific table API)
    PdfTable table = new PdfTable();
    table.Style.CellPadding = 2;
    table.Style.BorderPen = new PdfPen(Color.Black, 0.5f);
    table.Style.DefaultStyle.Font = font;
    
    // Create data source
    DataTable dataTable = new DataTable();
    dataTable.Columns.Add("Item");
    dataTable.Columns.Add("Quantity");
    dataTable.Columns.Add("Price");
    dataTable.Rows.Add("Widget A", "10", "$50.00");
    dataTable.Rows.Add("Widget B", "5", "$75.00");
    dataTable.Rows.Add("Widget C", "20", "$100.00");
    
    // Set table data source
    table.DataSource = dataTable;
    
    // Draw table at specific position
    PdfLayoutResult result = table.Draw(page, new PointF(50, 100));
    
    // Add more content after table
    float yPos = result.Bounds.Bottom + 20;
    page.Canvas.DrawString("Total: $225.00", font, brush, 
        new PointF(50, yPos));
    
    // Save document
    doc.SaveToFile(outputPath, FileFormat.PDF);
    doc.Close();
}
```

**Spire.PDF drawing API notes:**
- Low-level control over PDF structure and precise positioning
- Table creation from `DataTable`
- Requires learning Spire-specific types (`PdfTable`, `PdfFont`, `PdfBrush`, etc.) — HTML/CSS knowledge does not transfer
- Manual layout calculations for stacking content
- Free Spire.PDF caps loading/creating at 10 pages — verify free-edition limits against the current vendor docs before relying on them

#### IronPDF Implementation (HTML-based)

```csharp
using IronPdf;

public void CreatePdfFromScratch(string outputPath)
{
    string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Helvetica, Arial, sans-serif; margin: 50px; }
        h1 { font-size: 20pt; margin-bottom: 20px; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { border: 1px solid black; padding: 8px; text-align: left; }
        .total { margin-top: 20px; font-size: 12pt; }
    </style>
</head>
<body>
    <h1>Invoice Report</h1>
    <table>
        <tr><th>Item</th><th>Quantity</th><th>Price</th></tr>
        <tr><td>Widget A</td><td>10</td><td>$50.00</td></tr>
        <tr><td>Widget B</td><td>5</td><td>$75.00</td></tr>
        <tr><td>Widget C</td><td>20</td><td>$100.00</td></tr>
    </table>
    <div class='total'>Total: $225.00</div>
</body>
</html>";
    
    var renderer = new ChromePdfRenderer();
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

**IronPDF HTML creation notes:**
- Familiar HTML/CSS (no new API surface to learn)
- Automatic layout — no manual positioning
- No table count cap in trial or licensed runs
- Styling via CSS (easier to maintain or theme)
- Shorter, more readable code
- Less low-level drawing control than Spire's canvas API (see [IronPDF editing features](https://ironpdf.com/docs/))

---

### ☑ **Task: Edit Existing PDF (Add Watermark)**

#### Spire.PDF Implementation

```csharp
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Drawing;

public void AddWatermark(string inputPath, string outputPath, string watermarkText)
{
    // Load existing PDF
    PdfDocument doc = new PdfDocument(inputPath);
    
    // Configure watermark font and brush
    PdfFont font = new PdfFont(PdfFontFamily.Helvetica, 72f);
    PdfBrush brush = new PdfBrush(Color.FromArgb(100, 200, 200, 200)); // Semi-transparent
    
    // Iterate through pages
    foreach (PdfPageBase page in doc.Pages)
    {
        // Calculate position (center of page, rotated)
        SizeF pageSize = page.Size;
        float x = (pageSize.Width - font.MeasureString(watermarkText).Width) / 2;
        float y = (pageSize.Height - font.MeasureString(watermarkText).Height) / 2;
        
        // Save graphics state
        page.Canvas.Save();
        
        // Rotate and draw watermark
        page.Canvas.TranslateTransform(pageSize.Width / 2, pageSize.Height / 2);
        page.Canvas.RotateTransform(-45); // Diagonal
        page.Canvas.TranslateTransform(-pageSize.Width / 2, -pageSize.Height / 2);
        
        page.Canvas.DrawString(watermarkText, font, brush, 
            new PointF(x, y), new PdfStringFormat());
        
        // Restore graphics state
        page.Canvas.Restore();
    }
    
    // Save modified PDF
    doc.SaveToFile(outputPath, FileFormat.PDF);
    doc.Close();
}
```

**Spire.PDF watermark notes:**
- Precise control over watermark positioning, rotation, and transparency
- Position and transform math is manual (translate/rotate/translate-back)
- Text-only path shown above — image watermarks use the image-drawing API
- Verify watermark behavior against your edition (commercial vs Free Spire.PDF) and version

#### IronPDF Implementation

```csharp
using IronPdf;

public void AddWatermark(string inputPath, string outputPath, string watermarkText)
{
    using var pdf = PdfDocument.FromFile(inputPath);
    
    // Apply HTML-based watermark
    string watermarkHtml = $@"
        <div style='
            font-size: 72px; 
            color: rgba(200, 200, 200, 0.4); 
            font-weight: bold;
            font-family: Helvetica;'>
            {watermarkText}
        </div>";
    
    pdf.ApplyWatermark(watermarkHtml, rotation: 45, opacity: 40);
    pdf.SaveAs(outputPath);
}
```

**IronPDF watermark notes:**
- Concise API
- HTML-based watermarks (full CSS styling)
- Supports text and images
- Centering and rotation handled by the API
- Applied across all pages by default

[View IronPDF watermark guide](https://ironpdf.com/how-to/custom-watermark/)

---

## API Mapping Reference

| Spire.PDF API | IronPDF Equivalent |
|--------------|-------------------|
| `PdfDocument.LoadFromHTML(string, bool, PdfPageSettings, PdfHtmlLayoutFormat)` | `ChromePdfRenderer.RenderHtmlAsPdf(html)` |
| `PdfDocument.LoadFromHTML(url, bool, bool, bool)` (URL overload) | `ChromePdfRenderer.RenderUrlAsPdf(url)` |
| `new PdfDocument()` | `new ChromePdfRenderer()` + `RenderHtmlAsPdf(...)` |
| `doc.SaveToFile(path, FileFormat.PDF)` | `pdf.SaveAs(path)` |
| `doc.Pages.Add()` | HTML defines pages automatically |
| `page.Canvas.DrawString()` | HTML text elements |
| `PdfTable` API | HTML `<table>` elements |
| `PdfFont`, `PdfBrush` | CSS styling |
| Manual positioning | CSS layout (Grid, Flexbox) |
| `doc.Close()` | `using` / `Dispose()` |
| `Spire.Additions.Chrome.ChromeHtmlConverter` | `ChromePdfRenderer` (Chromium bundled) |

## Comprehensive Feature Comparison

| Category | Feature | Spire.PDF | IronPDF |
|----------|---------|-----------|---------|
| **Status** | Active development | Yes | Yes |
| | .NET 10 support | Yes (.NET Framework 4.0+, .NET 6/8/9/10) | Yes (.NET Framework 4.6.2+, .NET 6/7/8/9) |
| | Free SKU | Free Spire.PDF: 10-page load/create cap, 3-page conversion cap | 30-day commercial trial |
| **Support** | Documentation | E-iceblue knowledgebase (API-focused) | Iron Software docs and tutorials (task-focused) |
| | Channels | Email and vendor forums | Vendor support channels |
| **Content Creation** | HTML to PDF engine | IE/Edge Legacy or QtWebKit by default; opt-in `ChromeHtmlConverter` since v10.7.21 | Bundled Chromium |
| | CSS3 support | Limited on the legacy engines; improves with `ChromeHtmlConverter` | Full (Chromium) |
| | JavaScript during HTML render | Limited on legacy engines | Yes |
| | Drawing API | Yes (low-level canvas) | Limited (HTML-first) |
| | Fonts | Manual embedding or system fonts | `@font-face` via Chromium |
| **PDF Operations** | Merge PDFs | Yes | Yes |
| | Split PDFs | Yes | Yes |
| | Watermarks | Canvas draw + transform | HTML/image stamp API |
| | Digital signatures | Yes | Yes |
| | Forms | Create and fill | Create and fill |
| **Security** | Encryption | Yes | Yes (AES-256) |
| | Permissions | Yes | Yes (granular) |
| **HTML conversion** | HTML string input | Yes (4-arg `LoadFromHTML` overload) | Yes (`RenderHtmlAsPdf`) |
| | System Chrome required | Only when using `ChromeHtmlConverter` | No (bundled) |
| | Evaluation watermark | Yes (unlicensed commercial edition) | Yes (after trial) |
| **Development** | Learning curve | Higher when using drawing API | Lower if you already write HTML/CSS |
| | Web developer ergonomics | PDF-specific API | HTML-first |

---

## Installation Comparison

### Spire.PDF Installation

```bash
# Install Spire.PDF
Install-Package Spire.PDF

# Requires Spire.Pdf.dll and Spire.License.dll
# Multiple assemblies (~20-30MB)

# Namespace imports:
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Tables; // If using tables
using Spire.Pdf.Fields; // If using forms

# License activation required for production (removes watermark)
# License key applied in code:
// Spire.License.LicenseProvider.SetLicenseKey("YOUR-LICENSE-KEY");
```

**Licensing notes:**
- Free Spire.PDF (Community Edition): 10-page load/create cap; 3-page cap when converting to image/Word/HTML/XPS
- Unlicensed/evaluation runs of the commercial edition typically include an evaluation notice on output
- Multiple commercial tiers (Developer Small Business, Developer OEM, Site Small Business, Site OEM); OEM tier is required for public-facing/SaaS/Docker deployments per the vendor's tier rules
- Verify the current price list at e-iceblue.com

### IronPDF Installation

```bash
# Install IronPDF
Install-Package IronPdf

# Single NuGet package with automatic native handling

# Namespace import:
using IronPdf;

# License key applied at startup:
// IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

**Licensing notes:**
- 30-day full-feature commercial trial
- Per-developer / per-project / per-organization tiers (Lite, Plus, Professional, Unlimited)
- Royalty-free redistribution under most tiers — verify your tier's terms at ironpdf.com/licensing

---

## Decision Checklist

### Choose Spire.PDF if:

- [ ] You need a low-level PDF drawing API (precise positioning, canvas graphics)
- [ ] You're building PDF templates from scratch without HTML as the source format
- [ ] Browser-grade HTML/CSS fidelity is not a hard requirement, or you're willing to depend on system-installed Chrome via `ChromeHtmlConverter`
- [ ] Your usage fits within Free Spire.PDF caps (10-page load/create; 3-page conversion to image/Word/HTML/XPS), or you have a commercial license
- [ ] You prefer a smaller installed footprint (no bundled browser runtime)
- [ ] Your team is comfortable working in Spire's drawing-API surface

### Choose IronPDF if:

- [ ] You have HTML templates (web views, email templates, generated reports)
- [ ] You need browser-grade HTML-to-PDF fidelity without managing a system Chrome install
- [ ] CSS Grid, Flexbox, custom properties, or other modern CSS are part of your templates
- [ ] JavaScript execution during PDF generation is required
- [ ] Web fonts via `@font-face` are used in templates
- [ ] Your team writes HTML/CSS and prefers not to learn a PDF-specific drawing API
- [ ] Cross-platform deployment (Linux, macOS, containers) is in scope

---

## Conclusion

Spire.PDF and IronPDF target different PDF-generation models. Spire.PDF is a comprehensive PDF library with a low-level drawing API and editing surface — useful when building PDFs programmatically. Its default HTML-to-PDF path historically uses IE/Edge Legacy or QtWebKit, so modern CSS features may render with limited fidelity; the opt-in `ChromeHtmlConverter` since v10.7.21 closes much of that gap at the cost of a system Chrome dependency.

[IronPDF's Chromium-based HTML rendering](https://ironpdf.com/tutorials/html-to-pdf/) targets teams that already have HTML content — web views, email templates, generated reports. The workflow is straightforward: write HTML, call `RenderHtmlAsPdf`, manipulate the result if needed. Chromium is bundled, so there is no separate Chrome to install. If you're building PDFs from scratch with bespoke vector graphics, evaluate whether HTML and CSS can express your layout first.

For .NET teams in 2026, the practical split is: do you have HTML to convert, or PDFs to draw? If you have HTML, IronPDF's pipeline aligns naturally. If you're drawing PDFs at the canvas level and HTML can't express the layout, Spire.PDF's drawing API is the lower-level option.

**What's your team's biggest PDF generation pain point: layout accuracy or API complexity?**

Additional IronPDF resources: [Security and metadata controls](https://ironpdf.com/examples/security-and-metadata/), [Comprehensive editing guide](https://ironpdf.com/docs/).

---

*Canonical/source version on [Iron Software blog](https://ironpdf.com/blog/).*
