# Dropping MigraDoc for IronPDF: a .NET migration that fits in an afternoon

The pipeline worked fine on your dev machine. Then it went into the CI container and started failing with font-related errors on every build. MigraDoc's renderer depends on GDI+ internals that behave differently on Linux — in Alpine-based containers particularly, the `libgdiplus` dependency either isn't present, isn't the right version, or produces rendering artefacts that don't show up locally. You can fix it, but fixing it means adding OS-level dependencies to your Docker image and validating them across environments. That's an afternoon of work that has nothing to do with PDF generation.

This article covers the migration from MigraDoc to IronPDF, with a troubleshooting-first structure: the problems that commonly surface during migration, how to diagnose them, and the code changes that resolve them.

---

## The CI/CD problem — diagnosing first

Before touching migration code, understand exactly what's breaking in your pipeline. The failure mode shapes the fix.

### Common MigraDoc CI/CD failures

**Symptom: `TypeInitializationException` or `DllNotFoundException` referencing `libgdi+`**

```bash
# Check if libgdiplus is present in your container
docker run --rm your-image ldconfig -p | grep gdi

# If missing, the quick fix for Debian/Ubuntu base images:
RUN apt-get update && apt-get install -y libgdiplus
```

This is a temporary fix. It adds an OS-level dependency to your image and must be validated each time the base image updates.

**Symptom: Fonts render differently in CI than locally**

MigraDoc relies on font resolution that can differ between Windows (dev) and Linux (CI). Check:

```bash
# List available fonts in your container
docker run --rm your-image fc-list

# If font-config not installed:
RUN apt-get install -y fontconfig

# Install specific fonts if needed (e.g., liberation fonts)
RUN apt-get install -y fonts-liberation
```

**Symptom: Works in .NET 6 but breaks in .NET 7+**

`System.Drawing.Common` was restricted to Windows in .NET 6 (with a warning) and throws `PlatformNotSupportedException` on Linux in .NET 7+ by default. MigraDoc versions that depend on `System.Drawing.Common` will fail:

```bash
# Check if your MigraDoc version has this dependency
dotnet list package --include-transitive | grep -i drawing
```

If it does, you need either a newer MigraDoc version or a different library.

---

## Why migrate (without drama)

Beyond the CI/CD failure, here are 8 additional reasons teams make this switch:

1. **`System.Drawing.Common` deprecation on Linux** — breaking change in .NET 7+ that affects MigraDoc versions with this dependency.
2. **Programmatic vs HTML-first** — MigraDoc requires building documents in code. Teams with HTML templates don't have a natural migration path within MigraDoc.
3. **CSS/modern layout not supported** — MigraDoc has its own style system, not CSS. Web-first teams find the mental model mismatch expensive.
4. **No HTML renderer** — converting existing HTML templates requires either converting them to MigraDoc's DOM or adding a second library.
5. **Limited manipulation features** — MigraDoc's strength is document generation; merging, watermarking, and encrypting existing PDFs requires PDFsharp directly or additional libraries.
6. **Licensing trajectory** — verify current licensing status; ensure commercial use terms haven't changed.
7. **Debug difficulty** — visual debugging of layout issues is harder via a DOM API than via HTML preview in a browser.
8. **Limited async support** — verify current async API surface in your MigraDoc version.

### Comparison table

| Aspect | MigraDoc | IronPDF |
|---|---|---|
| Focus | Programmatic document construction | HTML-to-PDF + PDF manipulation |
| Pricing | Open source (MIT) | Commercial — verify at ironsoftware.com |
| API Style | Document DOM (Sections, Paragraphs, Tables) | HTML renderer + document model |
| Learning Curve | Medium-High (proprietary document model) | Medium |
| HTML Rendering | None natively — DOM-only | Chromium-based |
| Page Indexing | N/A (generation only) | 0-based |
| Thread Safety | Verify in MigraDoc docs | Renderer instance reuse — see async docs |
| Namespace | `MigraDoc.DocumentObjectModel` | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | MigraDoc approach | Effort to migrate |
|---|---|---|
| Basic document generation | Document DOM → render | High — rewrite as HTML template |
| Tables | `Table` class with `Row`/`Cell` | High — rewrite as HTML `<table>` |
| Paragraph styles | `ParagraphFormat` / `Style` | Medium — convert to CSS |
| Images | `Image` element | Medium — `<img>` tag in HTML |
| Page headers / footers | `HeaderFooter` class | Medium — verify IronPDF API |
| Merge PDFs | PDFsharp required | Low (native in IronPDF) |
| Watermark | PDFsharp required | Low |
| Password protection | PDFsharp required | Low |
| Custom page size | `PageSetup` | Low — `ChromePdfRenderOptions.PaperSize` |
| Bookmarks / outline | `BookmarkField` | Medium — verify IronPDF equivalent |
| PDF/A output | Verify in MigraDoc/PDFsharp | Low in IronPDF |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| CI/CD failing due to `System.Drawing.Common` on .NET 7+ | Urgent migration signal; evaluate IronPDF or PuppeteerSharp |
| HTML templates already exist for the document types | IronPDF fits naturally; minimal logic rewrite |
| Existing document DOM code is extensive and tested | Budget higher effort; or fix MigraDoc deps and stay |
| Open source budget constraint | MigraDoc is MIT; IronPDF is commercial — evaluate wkhtmltopdf or PuppeteerSharp |

---

## Before you start

### Prerequisites

- .NET 6+ target
- HTML templating approach decided (inline HTML, Razor, or other)
- Dev environment with MigraDoc currently working (for visual comparison)

### Find MigraDoc references in your codebase

```bash
# Find all MigraDoc usage
rg -l "MigraDoc\|PdfDocumentRenderer" --type cs

# Find Document DOM construction
rg "new Document\(\)\|AddSection\|AddParagraph\|AddTable" --type cs -n

# Find rendering calls
rg "PdfDocumentRenderer" --type cs -n

# Find using statements
rg "using MigraDoc" --type cs -n
```

### Remove MigraDoc, install IronPDF

```bash
# Remove MigraDoc packages
dotnet remove package MigraDoc.DocumentObjectModel
dotnet remove package MigraDoc.Rendering

# Also remove PDFsharp if it was only used to support MigraDoc
# dotnet remove package PdfSharp  # only if no direct usage elsewhere

# Install IronPDF
dotnet add package IronPdf

# Restore
dotnet restore
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (MigraDoc — no license key, open source):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
// No license initialization needed — open source
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR_IRONPDF_LICENSE_KEY";
// Guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
using IronPdf.Security;
```

### Step 3: Basic document generation

**Before (MigraDoc document DOM):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

var doc = new Document();
var section = doc.AddSection();
var paragraph = section.AddParagraph("Hello World");
paragraph.Format.Font.Size = 18;
paragraph.Format.Font.Bold = true;

var renderer = new PdfDocumentRenderer(unicode: true);
renderer.Document = doc;
renderer.RenderDocument();
renderer.PdfDocument.Save("output.pdf");
```

**After (IronPDF — HTML template replaces DOM):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Troubleshooting: common migration failures

### Problem: "My MigraDoc tables don't convert cleanly to HTML"

MigraDoc tables use an explicit column/row/cell model with precise width control. HTML tables are more flexible but require CSS for the same level of control.

**MigraDoc table (before):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

class TableExample
{
    static void Main()
    {
        var doc = new Document();
        var section = doc.AddSection();

        var table = section.AddTable();
        table.Borders.Width = 0.75;

        // Define columns with explicit widths
        table.AddColumn("3cm");
        table.AddColumn("5cm");
        table.AddColumn("3cm");

        // Header row
        var row = table.AddRow();
        row.Shading.Color = MigraDoc.DocumentObjectModel.Colors.LightGray;
        row.Cells[0].AddParagraph("Product");
        row.Cells[1].AddParagraph("Description");
        row.Cells[2].AddParagraph("Price");

        // Data row
        row = table.AddRow();
        row.Cells[0].AddParagraph("Widget A");
        row.Cells[1].AddParagraph("Standard widget");
        row.Cells[2].AddParagraph("$50.00");

        var renderer = new PdfDocumentRenderer(unicode: true);
        renderer.Document = doc;
        renderer.RenderDocument();
        renderer.PdfDocument.Save("table.pdf");
    }
}
```

**After (HTML/CSS table — equivalent layout):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

string html = @"
<html><head><style>
  table { border-collapse: collapse; width: 100%; }
  th, td { border: 1px solid #999; padding: 6px 10px; }
  th { background: #d3d3d3; font-weight: bold; }
  .col-product { width: 27%; }
  .col-desc    { width: 46%; }
  .col-price   { width: 27%; }
</style></head><body>
  <table>
    <tr>
      <th class='col-product'>Product</th>
      <th class='col-desc'>Description</th>
      <th class='col-price'>Price</th>
    </tr>
    <tr>
      <td>Widget A</td>
      <td>Standard widget</td>
      <td>$50.00</td>
    </tr>
  </table>
</body></html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("table.pdf");
```

---

### Problem: "Styles and formatting aren't matching"

MigraDoc uses named styles with `ParagraphFormat`. The migration path is CSS:

| MigraDoc concept | HTML/CSS equivalent |
|---|---|
| `Style.Font.Size = 12` | `font-size: 12pt` |
| `Style.Font.Bold = true` | `font-weight: bold` |
| `Style.Font.Color = Colors.Red` | `color: red` |
| `ParagraphFormat.SpaceBefore` | `margin-top` |
| `ParagraphFormat.Alignment = Center` | `text-align: center` |
| `Section.PageSetup.LeftMargin` | `ChromePdfRenderOptions.MarginLeft` |

---

### Problem: "Headers and footers aren't carrying over"

MigraDoc uses `HeaderFooter` objects attached to sections. IronPDF uses `HtmlHeaderFooter`:

```csharp
// MigraDoc header pattern:
// section.Headers.Primary.AddParagraph("Page Header");

// IronPDF equivalent:
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:right;font-size:9pt;'>Page Header</div>",
    DrawDividerLine = false
};
// Headers/footers guide: https://ironpdf.com/how-to/headers-and-footers/
```

---

### Problem: "CI build passes but output looks different from local"

Font availability is the most common cause. Verify:

```bash
# In your Dockerfile — add fonts before build
RUN apt-get install -y fonts-liberation fonts-dejavu

# Verify fonts are accessible inside the container
docker run --rm your-image fc-list | grep -i liberation
```

IronPDF bundles its own Chromium renderer, which has more predictable cross-platform behavior, but system fonts still need to be present for custom font usage.

---

## API mapping tables

### Namespace mapping

| MigraDoc | IronPDF | Notes |
|---|---|---|
| `MigraDoc.DocumentObjectModel` | `IronPdf` | Core |
| `MigraDoc.Rendering` | `IronPdf` | Rendering entry point |
| `PdfSharp.Pdf` | `IronPdf.Editing` / `IronPdf.Security` | Manipulation ops |

### Core class mapping

| MigraDoc class | IronPDF class | Description |
|---|---|---|
| `Document` | N/A (replaced by HTML string) | Document root → HTML template |
| `PdfDocumentRenderer` | `ChromePdfRenderer` | Rendering entry point |
| `Section` | N/A | HTML document structure |
| `PdfDocument` (PDFsharp) | `PdfDocument` | Existing PDF operations |

### Document loading methods

| Operation | MigraDoc | IronPDF |
|---|---|---|
| HTML string | Not supported | `renderer.RenderHtmlAsPdf(html)` |
| File-based | N/A | `renderer.RenderHtmlFileAsPdf(path)` |
| Existing PDF | `PdfReader.Open()` (PDFsharp) | `PdfDocument.FromFile(path)` |
| URL | Not supported | `renderer.RenderUrlAsPdf(url)` |

### Page operations

| Operation | MigraDoc | IronPDF |
|---|---|---|
| Page size | `Section.PageSetup.PageFormat` | `ChromePdfRenderOptions.PaperSize` |
| Margins | `Section.PageSetup.LeftMargin` etc. | `ChromePdfRenderOptions.Margin*` |
| Orientation | `Section.PageSetup.Orientation` | `ChromePdfRenderOptions.PaperOrientation` |
| Page count | N/A (generation only) | `pdf.PageCount` |

### Merge/split operations

| Operation | MigraDoc/PDFsharp | IronPDF |
|---|---|---|
| Merge | `PdfDocument` + `ImportPage()` | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | `PdfDocument` page manipulation | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

### 1. HTML to PDF (Document generation)

**Before (MigraDoc document DOM — full working example):**
```csharp
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;

class DocumentGenerationExample
{
    static void Main()
    {
        // Build document structure in code
        var doc = new Document();
        doc.Info.Title = "Invoice #1234";

        // Define styles
        var style = doc.Styles["Normal"];
        style.Font.Name = "Arial";
        style.Font.Size = 10;

        var section = doc.AddSection();
        section.PageSetup.TopMargin = "2cm";
        section.PageSetup.BottomMargin = "2cm";

        // Title
        var title = section.AddParagraph("Invoice #1234");
        title.Format.Font.Size = 18;
        title.Format.Font.Bold = true;
        title.Format.SpaceAfter = "0.5cm";

        // Content paragraph
        var content = section.AddParagraph("Amount Due: $500.00");
        content.Format.Font.Size = 12;

        // Render
        var pdfRenderer = new PdfDocumentRenderer(unicode: true)
        {
            Document = doc
        };
        pdfRenderer.RenderDocument();
        pdfRenderer.PdfDocument.Save("invoice.pdf");
        Console.WriteLine("Saved: invoice.pdf");
    }
}
```

**After (IronPDF — HTML template replaces DOM):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

string html = @"
<html><head><style>
  body { font-family: Arial, sans-serif; font-size: 10pt;
         margin: 2cm 2cm 2cm 2cm; }
  h1   { font-size: 18pt; margin-bottom: 0.5cm; }
  .amount { font-size: 12pt; }
</style></head><body>
  <h1>Invoice #1234</h1>
  <p class='amount'>Amount Due: $500.00</p>
</body></html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
// Rendering options: https://ironpdf.com/how-to/rendering-options/
```

---

### 2. Merge PDFs

**Before (PDFsharp — typically used alongside MigraDoc):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;

class MergePdfsExample
{
    static void Main()
    {
        using var outputDoc = new PdfDocument();

        foreach (string path in new[] { "section1.pdf", "section2.pdf" })
        {
            using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            foreach (PdfPage page in input.Pages)
                outputDoc.AddPage(page);
        }

        outputDoc.Save("merged.pdf");
        Console.WriteLine("Merged to: merged.pdf");
    }
}
```

**After (IronPDF native merge):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var merged = PdfDocument.Merge(
    PdfDocument.FromFile("section1.pdf"),
    PdfDocument.FromFile("section2.pdf")
);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (PDFsharp — no native watermark in MigraDoc; PDFsharp content stream):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using System.IO;
using System;

class WatermarkExample
{
    static void Main()
    {
        using var document = PdfReader.Open("input.pdf", PdfDocumentOpenMode.Modify);

        foreach (PdfPage page in document.Pages)
        {
            using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
            var font = new XFont("Arial", 60, XFontStyle.Bold);
            gfx.TranslateTransform(page.Width / 2, page.Height / 2);
            gfx.RotateTransform(-45);
            gfx.DrawString(
                "CONFIDENTIAL",
                font,
                new XBrush[] { XBrushes.LightGray }[0],
                new XPoint(0, 0)
            );
        }

        document.Save("watermarked.pdf");
        Console.WriteLine("Watermarked.");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf = PdfDocument.FromFile("input.pdf");
var stamper = new TextStamper
{
    Text = "CONFIDENTIAL",
    FontColor = IronSoftware.Drawing.Color.LightGray,
    FontSize = 60,
    Opacity = 35,
    Rotation = 45,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};
pdf.ApplyStamp(stamper);
pdf.SaveAs("watermarked.pdf");
// Guide: https://ironpdf.com/how-to/custom-watermark/
```

---

### 4. Password protection

**Before (PDFsharp encryption — no native security in MigraDoc):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using System;

class SecurityExample
{
    static void Main()
    {
        using var document = PdfReader.Open("input.pdf", PdfDocumentOpenMode.Modify);

        var security = document.SecuritySettings;
        security.UserPassword  = "readpass";
        security.OwnerPassword = "adminpass";
        security.PermitPrint   = true;
        security.PermitModifyDocument = false;

        document.Save("secured.pdf");
        Console.WriteLine("Encrypted: secured.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Security;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf = PdfDocument.FromFile("input.pdf");
pdf.SecuritySettings.UserPassword  = "readpass";
pdf.SecuritySettings.OwnerPassword = "adminpass";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserAnnotations = false;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Document model → HTML template migration

This is the non-trivial part. MigraDoc's document DOM doesn't have a 1:1 mapping to HTML — it's a conceptual rewrite. Strategy:

1. List every document type your system generates
2. Create an HTML/CSS template for each (browser-preview it first)
3. Wire up your data model to populate the template (Razor, Handlebars.NET, string interpolation, etc.)
4. Render via IronPDF

The HTML approach is typically more maintainable long-term, but the initial conversion takes time proportional to template complexity.

### Page indexing

IronPDF uses 0-based page indexing. MigraDoc doesn't expose a post-render page model, but PDFsharp (used for manipulation) uses 0-based as well — verify your specific version.

### Font handling

MigraDoc lets you specify fonts by name from the system font list. IronPDF's Chromium renderer does the same via CSS `font-family`. Verify that fonts your templates reference are installed in your deployment environment.

---

## Performance considerations

### Renderer reuse for batch

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

foreach (var data in documentDataSet)
{
    string html = BuildHtml(data); // your template engine
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"doc_{data.Id}.pdf");
}
```

### Async for web

```csharp
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
// Async guide: https://ironpdf.com/how-to/async/
return File(pdf.Stream, "application/pdf", "document.pdf");
```

### Container footprint

IronPDF bundles Chromium binaries — the package is larger than MigraDoc + PDFsharp. Factor this into image size planning. The upside: no OS-level dependency on `libgdiplus`.

---

## Migration checklist

### Pre-migration

- [ ] Find all MigraDoc usages: `rg "MigraDoc\|PdfDocumentRenderer" --type cs`
- [ ] List every document type generated — these become HTML templates
- [ ] Identify CI/CD failure mode precisely (font? GDI+? version?)
- [ ] Check PDFsharp usage — is it used independently beyond MigraDoc support?
- [ ] Verify IronPDF .NET target framework compatibility
- [ ] Confirm commercial license requirements
- [ ] Set up IronPDF trial license in dev environment
- [ ] Choose HTML template approach (Razor, Handlebars.NET, string formatting)

### Code migration

- [ ] Remove `MigraDoc.DocumentObjectModel` and `MigraDoc.Rendering` NuGet packages
- [ ] Remove `PdfSharp` if only used to support MigraDoc
- [ ] Add `IronPdf` NuGet package
- [ ] Create HTML templates for each document type
- [ ] Replace `PdfDocumentRenderer` with `ChromePdfRenderer`
- [ ] Replace merge pattern with `PdfDocument.Merge()`
- [ ] Replace PDFsharp watermark with `pdf.ApplyStamp()`
- [ ] Replace PDFsharp security with `pdf.SecuritySettings`
- [ ] Add IronPDF license key to config
- [ ] Update Docker image — remove `libgdiplus` install step

### Testing

- [ ] Render each document type and visually compare against MigraDoc output
- [ ] Pay attention to: table column widths, font rendering, spacing
- [ ] Test headers and footers on multi-page documents
- [ ] Test merge and split with representative document sets
- [ ] Test password protection (correct and incorrect credentials)
- [ ] Run CI/CD pipeline and verify no font or GDI errors
- [ ] Load test concurrent rendering at expected peak

### Post-migration

- [ ] Remove `libgdiplus` from Dockerfile if present
- [ ] Remove MigraDoc/PDFsharp from Docker base image
- [ ] Update deployment documentation
- [ ] Monitor memory baseline (Chromium vs GDI rendering have different profiles)

---

## Wrapping Up

The CI/CD failures are usually the immediate trigger, but the real migration work is the document DOM-to-HTML-template conversion. Teams that have complex multi-section reports with precise column widths and custom styles should budget a day or two per document type. Teams with simple invoices or letters can typically move faster.

**What version of MigraDoc are you migrating from, and did anything break unexpectedly during the HTML template conversion?** Particularly interested in teams who had MigraDoc-generated PDFs with complex table layouts or inline images — those tend to surface the most CSS calibration work.

---

*Tags: `dotnet`, `csharp`, `pdf`, `migration`*
