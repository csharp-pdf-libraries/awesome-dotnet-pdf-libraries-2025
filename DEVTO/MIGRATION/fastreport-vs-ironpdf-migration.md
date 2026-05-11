---
title: "Migrating from FastReport to IronPDF: what breaks, what does not"
published: false
tags: dotnet, csharp, pdf, migration
---

Here's a scenario that comes up more often than it should: a .NET application has been generating invoices, purchase orders, and reports via FastReport for years. The reports look exactly right — pixel-perfect, precisely positioned, every field bound to data. Then someone asks: "Can we just take this HTML email template and turn it into a PDF?" And that's where things get complicated.

FastReport is a reporting tool. It excels at data-bound, template-designed documents. IronPDF is an HTML-to-PDF renderer. They solve different problems, but they overlap in the "produce a PDF" outcome enough that teams regularly need to make a choice. This article is for teams maintaining both tools because FastReport does the reports and something else does the HTML rendering — and they want to consolidate.

By the end, you'll have the migration patterns, API mappings, and code samples for the HTML-to-PDF half of that consolidation.

---

## Why Migrate (Without Drama)

These triggers apply specifically to teams trying to reduce the FastReport footprint for HTML-rendering use cases:

1. **HTML input isn't FastReport's model** — FastReport uses `.frx` template files and a designer. Teams needing to render HTML emails, Razor-generated pages, or web templates find FastReport is the wrong tool.
2. **Template designer dependency** — FastReport's report model is built around visual designer tooling for layout changes. HTML templates can be edited in any editor.
3. **Data binding model mismatch** — FastReport binds data to report bands via `RegisterData()`. If your data is already in an HTML template, FastReport adds unnecessary ceremony.
4. **Deployment weight** — FastReport ships as multiple NuGet packages (Core, Export.PdfSimple, Web, etc.). For an HTML-to-PDF use case, a targeted HTML renderer is typically lighter.
5. **License scope** — `FastReport.OpenSource` is MIT-licensed, but its `PDFSimpleExport` rasterizes pages, so output PDFs contain images of text rather than selectable text. Selectable-text PDF, encryption, digital signing, and PDF/A require the commercial `FastReport.Net` package, which is published on FastReport's private NuGet feed rather than nuget.org.
6. **CSS fidelity in HTML exports** — FastReport's `HtmlObject` only handles a limited HTML 4 subset with no JavaScript and no modern CSS. Heavy CSS3 layouts (Flexbox, Grid) typically need a Chromium-based renderer.
7. **Maintenance of .frx files** — Report template files are XML, not particularly version-control-friendly to diff. HTML templates are plain text.
8. **Dynamic content** — For PDFs whose structure changes based on data (not just field values but layout structure), HTML templates can be more flexible than banded report templates.

### Side-by-Side Comparison

| Aspect | FastReport | IronPDF |
|---|---|---|
| Focus | Data-bound report designer + multi-format export | HTML/URL → PDF via Chromium |
| Pricing | Commercial (`FastReport.Net`) or MIT-licensed (`FastReport.OpenSource`, with rasterized PDF export only) | Commercial, free trial available |
| API Style | Report engine: load `.frx`, register data, prepare, export | Renderer: pass HTML, get PDF |
| Learning Curve | Steep — band-based concepts (DataBand, PageHeaderBand) | Gentle — HTML/CSS knowledge transfers directly |
| HTML Rendering | `HtmlObject` (limited HTML 4 subset, no JS, no modern CSS) | Chromium-based, full CSS3 / JS |
| Page Indexing | 0-based | 0-based |
| Packaging | Multiple NuGet packages | Single `IronPdf` package |
| Namespace | `FastReport` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Complexity | Notes |
|---|---|---|
| HTML string → PDF | Low | FastReport has no native HTML-string API — net new |
| URL → PDF | Low | FastReport has no native URL API — net new |
| Data-bound report → PDF | High | Core FastReport use case — don't migrate this |
| Banded layout → HTML template | High | Architectural change, not a library swap |
| Merge PDFs | Medium | FastReport has no built-in merge; IronPDF has native `PdfDocument.Merge` |
| Watermark on existing PDF | Medium | FastReport adds watermarks in the `.frx` template; IronPDF uses a stamper on existing PDFs |
| Password protection | Medium | FastReport `PDFExport` security properties map to IronPDF `SecuritySettings` |
| Dynamic charts | Low | FastReport has built-in chart objects; IronPDF renders JS charts (Chart.js, D3) directly |
| Large multi-hundred-page report | Medium | FastReport is optimized for banded reports at scale; IronPDF handles large HTML documents but rendering cost scales with page complexity |

### Decision Matrix

| Scenario | Recommendation |
|---|---|
| All PDFs are data-bound banded reports | Keep FastReport — IronPDF won't improve this |
| Mix: banded reports + HTML-template PDFs | Keep both; use each for what it's built for |
| HTML-template PDFs only, no banded reports | Migrate fully to IronPDF; retire FastReport |
| Need to export `.frx` reports to PDF programmatically | Stay on FastReport for this workload |

> The most honest migration advice here: if your FastReport `.frx` templates are working and your stakeholders don't want to move to HTML-based templates, don't migrate. FastReport does its job. The migration makes sense when you're already generating HTML and want to convert it, not when you're trying to recreate report templates in a different tool.

---

## The Consolidation Story

Teams often find themselves maintaining two PDF pathways: FastReport for structured reports and a separate library (or hand-rolled solution) for HTML-to-PDF. A common pattern teams I have worked with have hit:

```
Invoice report       → FastReport (.frx template) → PDF
Email confirmation   → HtmlString + ad-hoc lib    → PDF (fragile)
Dynamic dashboard    → Razor view                 → PDF (CSS issues)
Compliance doc       → HTML template              → PDF (no JS)
```

The consolidation case is: replace the ad-hoc HTML-to-PDF paths with IronPDF, and keep FastReport for the banded reports that use `.frx` templates. This reduces the number of "how do we convert X to PDF" conversations without disrupting working report templates.

The scenario you want to avoid: trying to rewrite working FastReport templates as HTML because you want one library. Unless there's a specific reason to do this (maintenance burden on `.frx` files, designer tooling cost), it adds risk for marginal benefit.

---

## Before You Start

### Identify the HTML-to-PDF vs. Report-to-PDF Split

```bash
# Find FastReport report loading — these are your report-PDF paths (keep FastReport)
rg "new Report\(\)|\.Load\(" --type cs

# Find HTML string being passed to FastReport or converted elsewhere — migration targets
rg "HtmlContent|htmlString|HtmlObject" --type cs

# Find Razor view rendering to PDF — migration candidates
rg "RenderToString|ViewResult.*pdf|html.*pdf" --type cs -l
```

### Uninstall / Install (for HTML-to-PDF paths only)

```bash
# If fully replacing FastReport (only if you've confirmed no banded report usage):
dotnet remove package FastReport.OpenSource
dotnet remove package FastReport.OpenSource.Export.PdfSimple

# Install IronPDF for HTML-to-PDF paths
dotnet add package IronPdf

dotnet list package
```

### License

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Docs: https://ironpdf.com/how-to/license-keys/
```

---

## Quick Start Migration (3 Steps)

For the HTML-to-PDF path:

### Step 1: License

```csharp
// IronPDF license
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 2: Namespace Imports

```csharp
// Before (FastReport)
using FastReport;
using FastReport.Export.PdfSimple;

// After (IronPDF)
using IronPdf;
```

### Step 3: Basic HTML → PDF

```csharp
// Before (FastReport — HTML must be wrapped in a band-bound HtmlObject)
using (Report report = new Report())
{
    ReportPage page = new ReportPage();
    report.Pages.Add(page);
    page.ReportTitle = new ReportTitleBand { Height = Units.Millimeters * 100 };

    HtmlObject htmlObject = new HtmlObject();
    htmlObject.Bounds = new RectangleF(0, 0, Units.Millimeters * 190, Units.Millimeters * 100);
    htmlObject.Text = htmlBody;
    page.ReportTitle.Objects.Add(htmlObject);

    report.Prepare();
    var export = new PDFSimpleExport();
    using var fs = new FileStream("output.pdf", FileMode.Create);
    report.Export(export, fs);
}

// After (IronPDF)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf(htmlBody);
pdf.SaveAs("output.pdf");
// Docs: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| FastReport | IronPDF | Notes |
|---|---|---|
| `FastReport` | `IronPdf` | Core namespace |
| `FastReport.Export.Pdf` | `IronPdf` | Commercial PDF export — built into IronPDF |
| `FastReport.Export.PdfSimple` | `IronPdf` | OpenSource rasterized export — replaced by Chromium rendering |
| `FastReport.Utils` | `IronPdf.Rendering` | Rendering utilities |
| `FastReport.Data` | N/A | Use standard .NET data access |

### Core Class Mapping

| FastReport Class | IronPDF Class | Description |
|---|---|---|
| `Report` | `ChromePdfRenderer` | Main rendering class (different model) |
| `PDFExport` / `PDFSimpleExport` | `ChromePdfRenderer` + `SecuritySettings` | Rendering + security |
| `ReportPage` | HTML `<body>` / `<div>` | Page content |
| `TextObject` | HTML `<p>`, `<span>`, `<div>` | Text elements |
| `TableObject` | HTML `<table>` | Tables |
| `DataBand` | Loop in template | Data iteration |
| `PageHeaderBand` / `PageFooterBand` | `HtmlHeaderFooter` | Headers and footers |
| `HtmlObject` | Direct HTML rendering | Native input — no wrapper object needed |
| `PictureObject` | HTML `<img>` | Images |

### Document Loading (HTML path only)

| Operation | FastReport (workaround) | IronPDF |
|---|---|---|
| HTML string → PDF | Wrap in `HtmlObject` on a band | `renderer.RenderHtmlAsPdf(html)` |
| URL → PDF | Download via `HttpClient`, feed to `HtmlObject` | `renderer.RenderUrlAsPdf(url)` |
| Load existing PDF | Not natively supported | `PdfDocument.FromFile(path)` |
| Load from bytes | Not natively supported | `PdfDocument.FromBinaryData(bytes)` |

### Page Operations

| Operation | FastReport | IronPDF |
|---|---|---|
| Page count | `report.PreparedPages.Count` after `Prepare()` | `pdf.PageCount` |
| Page size | Report template setting | `RenderingOptions.PaperSize` |
| Remove page(s) | Not directly exposed at export time | `pdf.RemovePages(index)` |
| Margins | Report template setting | `RenderingOptions.MarginTop/Bottom/Left/Right` (mm) |

### Merge / Split

| Operation | FastReport | IronPDF |
|---|---|---|
| Merge reports | No built-in merge — must export each, then use a third-party tool | `PdfDocument.Merge(a, b)` |
| Split | Not directly exposed | `pdf.CopyPages(start, end)` |

---

## Four Complete Before/After Migrations

### 1. HTML String to PDF

**Before (FastReport — the standard band-and-HtmlObject pattern):**

```csharp
using FastReport;
using FastReport.Export.PdfSimple;
using System.Drawing;
using System.IO;

class Program
{
    static void Main()
    {
        using (Report report = new Report())
        {
            // FastReport renders nothing unless objects live on a band that
            // lives on a ReportPage.
            ReportPage page = new ReportPage();
            report.Pages.Add(page);
            page.ReportTitle = new ReportTitleBand { Height = Units.Millimeters * 100 };

            HtmlObject htmlObject = new HtmlObject();
            htmlObject.Bounds = new RectangleF(0, 0, Units.Millimeters * 190, Units.Millimeters * 100);
            htmlObject.Text = "<html><body><h1>Hello World</h1><p>This is a test PDF</p></body></html>";
            page.ReportTitle.Objects.Add(htmlObject);

            report.Prepare();

            // PDFSimpleExport rasterizes each page — text is not selectable.
            PDFSimpleExport pdfExport = new PDFSimpleExport();
            using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
            {
                report.Export(pdfExport, fs);
            }
        }
    }
}
```

**After (IronPDF — direct HTML input):**

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        // Docs: https://ironpdf.com/how-to/license-keys/

        // HTML template populated by your Razor/Scriban/Handlebars engine
        string html = BuildHtmlTemplate(myData);

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 15;
        renderer.RenderingOptions.MarginBottom = 15;
        // Docs: https://ironpdf.com/how-to/rendering-options/

        using var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report.pdf");
    }

    static string BuildHtmlTemplate(object data)
    {
        // Your existing template engine (Razor, Scriban, Handlebars, etc.)
        return $"<html><body><h1>Report for {data}</h1></body></html>";
    }
}
```

---

### 2. Merge PDFs

**Before (FastReport — no built-in merge):**

```csharp
using FastReport;
using FastReport.Export.PdfSimple;
using System.IO;
using System.Collections.Generic;

class MergeSample
{
    static void Main()
    {
        // FastReport has no built-in merge. Export each report to a temp file
        // then combine them with a separate PDF library.
        var pdfPaths = new List<string>();

        foreach (var reportFile in new[] { "report_a.frx", "report_b.frx" })
        {
            using (Report report = new Report())
            {
                report.Load(reportFile);
                report.Prepare();

                string tempPath = Path.GetTempFileName() + ".pdf";
                using (var export = new PDFSimpleExport())
                {
                    report.Export(export, tempPath);
                }
                pdfPaths.Add(tempPath);
            }
        }

        // Then merge the byte arrays using a separate PDF tool.
        MergePdfsWithThirdParty(pdfPaths, "merged.pdf");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class MergeSample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var docA = PdfDocument.FromFile("doc_a.pdf");
        using var docB = PdfDocument.FromFile("doc_b.pdf");

        using var merged = PdfDocument.Merge(docA, docB);
        merged.SaveAs("merged.pdf");
        // Docs: https://ironpdf.com/how-to/merge-or-split-pdfs/
    }
}
```

---

### 3. Watermark

**Before (FastReport — watermark lives in the .frx template):**

```csharp
using FastReport;
using FastReport.Export.PdfSimple;

class WatermarkSample
{
    static void Main()
    {
        // FastReport watermarks are typically added as a PictureObject or
        // TextObject on the report's background band, designed in the .frx.
        var report = new Report();
        report.Load("template_with_watermark.frx");
        report.Prepare();

        var pdfExport = new PDFSimpleExport();

        using var ms = new System.IO.MemoryStream();
        report.Export(pdfExport, ms);

        System.IO.File.WriteAllBytes("watermarked.pdf", ms.ToArray());
        report.Dispose();
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;
using IronPdf.Editing;

class WatermarkSample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        // Apply text stamp as watermark — no designer required
        var stamper = new TextStamper
        {
            Text = "DRAFT",
            FontSize = 50,
            Opacity = 25,
            Rotation = 45,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        pdf.ApplyStamp(stamper);
        pdf.SaveAs("watermarked.pdf");
        // For HTML watermarks: https://ironpdf.com/how-to/custom-watermark/
        // Docs: https://ironpdf.com/how-to/stamp-text-image/
    }
}
```

---

### 4. Password Protection

**Before (FastReport — PDFExport security properties):**

```csharp
using FastReport;
using FastReport.Export.Pdf;

class SecuritySample
{
    static void Main()
    {
        using (Report report = new Report())
        {
            report.Load("secure_report.frx");
            report.Prepare();

            // PDFExport (commercial) supports passwords and permissions.
            // PDFSimpleExport in the OpenSource edition does not.
            PDFExport export = new PDFExport();

            export.Title = "Confidential Report";
            export.Author = "Company Name";

            export.OwnerPassword = "owner456";
            export.UserPassword = "user123";

            export.AllowPrint = true;
            export.AllowCopy = false;
            export.AllowModify = false;
            export.AllowAnnotate = false;

            report.Export(export, "secured.pdf");
        }
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;
using IronPdf.Security;

class SecuritySample
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        // Docs: https://ironpdf.com/how-to/pdf-permissions-passwords/
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;
        pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;
        pdf.SecuritySettings.AllowUserAnnotations = false;

        pdf.SaveAs("secured.pdf");
    }
}
```

---

## Critical Migration Notes

### Two Different Models, Not a Drop-In Swap
FastReport uses a designer-template model: create a `.frx` file, register data sources, call `Prepare()`, export. IronPDF uses an HTML-in-PDF-out model. These are not interchangeable. The migration only makes sense for the HTML-to-PDF portions of your application.

### FastReport's PDFExport Has Security Features (Commercial Edition)
The commercial `FastReport.Net` package exposes `PDFExport.UserPassword`, `OwnerPassword`, `AllowPrint`, `AllowCopy`, `AllowModify`, and `AllowAnnotate`. These map directly to `IronPdf.PdfDocument.SecuritySettings.*`. The OpenSource edition's `PDFSimpleExport` does not include security features, so teams on OpenSource were typically handling encryption externally already.

### Memory Management
FastReport's `Report` object should be disposed after export. IronPDF's `PdfDocument` is `IDisposable` — use `using`. In server scenarios, treat `ChromePdfRenderer` as a singleton.

### .frx Template Preservation
Do not delete `.frx` templates until you have confirmed that all reports generated from them are either migrated or retired. These templates can be hard to reconstruct.

---

## Performance Considerations

FastReport's performance characteristics are tuned for banded report generation — large datasets, cross-tab aggregations, grouping. IronPDF's performance characteristics are tuned for HTML rendering throughput. They're not directly comparable; profile the specific workload.

```csharp
// IronPDF singleton pattern for server use
public class PdfRenderService
{
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    public byte[] RenderDocument(string html)
    {
        // Async alternative: await _renderer.RenderHtmlAsPdfAsync(html)
        // Docs: https://ironpdf.com/how-to/async/
        using var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

For parallel batch generation:

```csharp
// Parallel render using Task.WhenAll
// Docs: https://ironpdf.com/examples/parallel/
var tasks = htmlDocuments.Select(html => Task.Run(() =>
{
    using var pdf = _renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}));

var results = await Task.WhenAll(tasks);
```

---

## Migration Checklist

### Pre-Migration
- [ ] Separate FastReport usages: report-to-PDF (keep) vs. HTML-to-PDF (migrate)
- [ ] Identify all `.frx` template files — do not retire these until reports are confirmed migrated
- [ ] Map `PDFExport` security property names against your FastReport edition (commercial vs. OpenSource)
- [ ] Confirm IronPDF .NET target compatibility for your project
- [ ] Set up IronPDF license in all environments
- [ ] Review [rendering options](https://ironpdf.com/how-to/rendering-options/) for paper size and margin parity
- [ ] Document which paths are migrating, which are staying on FastReport

### Code Migration
- [ ] Isolate HTML-to-PDF paths into a service or module
- [ ] Add `ChromePdfRenderer` as a singleton/scoped service
- [ ] Replace HTML-to-PDF workarounds with `renderer.RenderHtmlAsPdf()`
- [ ] Replace `PDFExport` security with `SecuritySettings`
- [ ] Replace watermark approach (designer-based → stamper API)
- [ ] Replace merge approach (third-party combine → `PdfDocument.Merge`)
- [ ] Add `using` to all `PdfDocument` instances
- [ ] Update callers expecting `byte[]` to use `pdf.BinaryData`

### Testing
- [ ] Visual regression on all migrated PDF templates
- [ ] Test CSS-heavy templates (often a pain point pre-migration)
- [ ] Test JS-rendered charts (new capability)
- [ ] Test password protection
- [ ] Test merge output
- [ ] Load-test at target concurrency
- [ ] Confirm FastReport paths still work (they should be untouched)

### Post-Migration
- [ ] Remove FastReport packages from projects where no banded reports remain
- [ ] Confirm FastReport is still installed if report-to-PDF paths remain
- [ ] Update deployment documentation
- [ ] Archive FastReport license info even if staying on OpenSource

---

## Done Migrating? Here's What's Next

The FastReport migration story is really a consolidation story: stop solving HTML-to-PDF with workarounds and use a library designed for it. If FastReport's banded reports are working, leave them alone. The migration target is the code that was trying to shoehorn HTML into a report engine, or the parallel PDF library you've been maintaining alongside FastReport.

The pattern worth asking yourself: how many different code paths does your application currently use to produce a PDF? If the answer is more than two, that's a sign that consolidation has value independent of any specific library choice.

**Technical question for comments:** For teams running FastReport alongside an HTML renderer — how do you handle the case where a "report" needs to include a section that's dynamically generated HTML (like a chart from a JS library)? Do you render them separately and merge, or is there a cleaner pattern you've landed on?
