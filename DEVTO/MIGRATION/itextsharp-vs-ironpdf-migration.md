---
title: "iText / iTextSharp to IronPDF: three steps and you're done"
published: false
tags: dotnet, csharp, pdf, migration
---

The signal was quiet at first. The legacy `iTextSharp` 5.x package sits on NuGet at version 5.5.13.5, frozen on the .NET Framework, receiving only occasional security fixes. The actively maintained successor — the current `itext` package (formerly `itext7`) — has a different API, ships HTML-to-PDF as a separately purchased add-on (`itext.pdfhtml`), and carries the AGPL or a paid commercial license. Teams using either line face the same choice: stay and absorb the licensing and API cost, or move to a differently-licensed renderer.

This article covers the path to IronPDF specifically. It's a code-first walkthrough — the conceptual parts are short, the working snippets are long. If you're evaluating the jump and want to know exactly what changes, this gets into the details.

---

## The fundamental architecture difference

This is worth surfacing early because it changes what "migration" means here.

Both `itext` (current line) and `iTextSharp` (legacy 5.x line) build PDFs by composing objects in code — `Document`, `Paragraph`, `Table`, `Cell`, and friends. HTML-to-PDF in the current line is handled by the `itext.pdfhtml` add-on (a separate purchase) and supports a defined CSS subset; it does not execute JavaScript during rendering. The legacy `iTextSharp` line has `HTMLWorker`, which supports a small subset of HTML 4 and almost no modern CSS.

IronPDF works primarily as an HTML renderer — you give it HTML/CSS, and Chromium renders it to PDF. It also supports PDF manipulation (merge, split, watermark, etc.).

This means the migration path splits into two tracks:

1. **If you're using iText / iTextSharp to generate PDFs from HTML** -> straightforward substitution
2. **If you're using it as a programmatic document builder** -> rewrite the generation logic as HTML templates, then render

Track 2 is more work but often produces better results, since HTML/CSS is a more maintainable way to define document layouts than building them programmatically.

This article covers both tracks.

---

## Why migrate (without drama)

Eight reasons teams make this switch:

1. **iTextSharp 5.x is end-of-life** — the legacy package receives security fixes only; new development should not target it.
2. **AGPL licensing friction** — iText's AGPL license requires that any software distributed with it also be AGPL, unless you purchase a commercial license. Teams with proprietary applications either buy a subscription or move to a differently-licensed library.
3. **pdfHTML is a separate paid add-on** — the current `itext` package does not include HTML-to-PDF; the `itext.pdfhtml` add-on is sold separately on top of the core `itext` package.
4. **Subscription-only commercial licensing** — iText's commercial license is sold as an annual subscription based on PDF processing volume; a perpetual option is not advertised on the public pricing page.
5. **HTML-to-PDF fidelity** — `itext.pdfhtml` supports a defined CSS subset, not a full browser engine, and does not execute JavaScript at render time. The legacy `HTMLWorker` covers even less.
6. **API verbosity** — building a simple table requires substantial boilerplate. HTML/CSS templates with a renderer library often require less code.
7. **Debugging document layout** — debugging a PDF built from programmatic API calls is harder than debugging an HTML template that you can preview in a browser.
8. **Coordinate-system mental shift** — iText uses PDF coordinates (bottom-left origin); web developers joining the team have to context-switch every time they touch the layout code.

### Comparison table

| Aspect | iText / iTextSharp | IronPDF |
|---|---|---|
| Focus | Programmatic PDF builder + manipulation | HTML-to-PDF + PDF manipulation |
| Pricing | AGPL (open source) or commercial subscription | Commercial, perpetual option available |
| API Style | Document object model — programmatic | HTML renderer + document model |
| Learning Curve | High (low-level API, PDF coordinate system) | Medium (web-developer friendly) |
| HTML Rendering | `itext.pdfhtml` add-on (CSS subset, no JS); `HTMLWorker` on legacy 5.x | Chromium-based (full CSS3 + JS) |
| Page Indexing | 1-based | 1-based |
| Thread Safety | Not thread-safe by default | Renderer instance reuse — see async docs |
| Namespace | `iText.Kernel.Pdf` / `iText.Layout` (current); `iTextSharp.text` (legacy) | `IronPdf` |

Note on page indexing: iText uses 1-based page numbers throughout (`GetNumberOfPages()`, `GetPage(n)`, `PdfMerger.Merge(doc, 1, n)`). IronPDF's `PageCount` returns the total page count; specific page-range methods like `CopyPages(startIndex, endIndex)` operate on indices — audit each call site against the IronPDF API docs rather than assuming the iText convention carries over verbatim.

---

## Migration complexity assessment

### Effort by feature

| Feature | iText / iTextSharp approach | Effort to migrate |
|---|---|---|
| HTML string to PDF | `HtmlConverter.ConvertToPdf()` (current) / `HTMLWorker.Parse()` (legacy) | Low |
| Programmatic document building | `Document` + `Paragraph` + `Table` API | High — rewrite as HTML template |
| Merge PDFs | `PdfMerger` (current) / `PdfCopy` (legacy) | Low |
| Split PDF | Manual page copy | Low |
| Watermark / stamp | Canvas overlay + `PdfExtGState` (current) / `PdfStamper` (legacy) | Low |
| Password protection | `WriterProperties.SetStandardEncryption()` (current) / `PdfStamper.SetEncryption()` (legacy) | Low |
| Form filling | `PdfAcroForm` (current) / `AcroFields` (legacy) | Medium |
| Digital signatures | `PdfSigner` (current) / `PdfSignatureAppearance` (legacy) | Medium |
| Reading existing PDF text | `PdfTextExtractor.GetTextFromPage()` | Medium |
| PDF metadata | `pdfDoc.GetDocumentInfo()` (current) / `PdfReader.Info` (legacy) | Low |
| Page rotation | `page.GetRotation()` (current) / `PdfReader.GetPageRotation()` (legacy) | Low |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| Generating PDFs from HTML templates | IronPDF is a natural fit; minimal rewrite |
| Programmatic PDF construction (complex tables, precise positioning) | Evaluate whether HTML/CSS templates can replace it — often yes |
| AGPL constraint is the trigger | Compare commercial license terms (iText annual subscription vs IronPDF perpetual option) |
| Heavy PDF form filling (AcroForms) | Confirm IronPDF form API coverage against your form types |

---

## Before you start

### Prerequisites

- A supported .NET target (IronPDF supports modern .NET; check the [IronPDF compatibility matrix](https://ironpdf.com/docs/) for current ranges)
- Full HTML template inventory if replacing programmatic PDF building
- Browser preview setup for HTML template development

### Find iText / iTextSharp references in your codebase

```bash
# Find all iText and iTextSharp using statements
rg "using iText\b|using iTextSharp" --type cs -n

# Find library references
rg "iTextSharp|iText\." --type cs -n

# Find Document/PdfWriter patterns (core classes)
rg "PdfWriter|PdfReader|PdfStamper|PdfDocument|PdfMerger" --type cs -n

# Find HTML conversion paths
rg "HtmlConverter|HTMLWorker" --type cs -n -i

# Find form filling paths
rg "PdfAcroForm|AcroFields|SetField" --type cs -n
```

### Remove iText / iTextSharp, install IronPDF

```bash
# Remove current iText packages
dotnet remove package itext
dotnet remove package itext.pdfhtml
dotnet remove package itext7
dotnet remove package itext7.pdfhtml

# Remove legacy iTextSharp 5.x if present
dotnet remove package iTextSharp

# Install IronPDF
dotnet add package IronPdf

# Restore
dotnet restore
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (iText — AGPL by default, commercial license loaded via subscription):**
```csharp
using iText.Kernel.Pdf;
using System.IO;

// iText: no license key for AGPL use. Commercial subscription customers
// load a license via the iText licensing add-on (separate package).
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before (current `itext`):**
```csharp
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Html2pdf;       // requires itext.pdfhtml add-on
using System.IO;
```

**Before (legacy `iTextSharp` 5.x):**
```csharp
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using System.IO;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3: Basic HTML-to-PDF

**Before (current iText with pdfHTML add-on):**
```csharp
using iText.Html2pdf;
using System.IO;

class HtmlToPdfExample
{
    static void Main()
    {
        string html = "<h1>Hello World</h1><p>Content here.</p>";
        using var fs = new FileStream("output.pdf", FileMode.Create);
        HtmlConverter.ConvertToPdf(html, fs);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>Content here.</p>");
pdf.SaveAs("output.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API mapping tables

### Namespace mapping

| iText / iTextSharp | IronPDF | Notes |
|---|---|---|
| `iText.Kernel.Pdf` / `iTextSharp.text.pdf` | `IronPdf` | Core PDF operations |
| `iText.Layout` / `iTextSharp.text` | `IronPdf` | Document model |
| `iText.Html2pdf` / `iTextSharp.text.html.simpleparser` | N/A | Replaced by Chromium renderer |

### Core class mapping

| iText / iTextSharp class | IronPDF class | Description |
|---|---|---|
| `PdfWriter` + `PdfDocument` + `Document` | `ChromePdfRenderer` | PDF generation entry point |
| `PdfReader` | `PdfDocument.FromFile()` | Open existing PDF |
| `PdfDocument(reader, writer)` / `PdfStamper` | `PdfDocument` + methods | Modify existing PDF |
| `PdfMerger` / `PdfCopy` | `PdfDocument.Merge()` | Combine PDFs |

### Document loading methods

| Operation | iText (current) | iTextSharp 5.x | IronPDF |
|---|---|---|---|
| HTML string | `HtmlConverter.ConvertToPdf()` | `HTMLWorker.Parse()` | `renderer.RenderHtmlAsPdf(html)` |
| URL | Download HTML + convert | Not native | `renderer.RenderUrlAsPdf(url)` |
| Existing PDF | `new PdfDocument(new PdfReader(path))` | `new PdfReader(path)` | `PdfDocument.FromFile(path)` |
| From bytes | `new PdfReader(new MemoryStream(bytes))` | Similar | `PdfDocument.FromBinaryData(bytes)` |

### Page operations

| Operation | iText / iTextSharp | IronPDF |
|---|---|---|
| Page count | `pdfDoc.GetNumberOfPages()` / `reader.NumberOfPages` | `pdf.PageCount` |
| Paper size | `PageSize.A4` constant | `ChromePdfRenderOptions.PaperSize` |
| Margins | `Document.SetMargins()` | `ChromePdfRenderOptions.Margin*` |
| Page rotation | `page.GetRotation()` / `reader.GetPageRotation(n)` | `pdf.Pages[i].PageRotation` |

### Merge/split operations

| Operation | iText / iTextSharp | IronPDF |
|---|---|---|
| Merge | `PdfMerger.Merge()` / `PdfCopy` + `PdfReader` per file | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | Manual page copy iteration | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

### 1. HTML to PDF

**Before (current iText with `itext.pdfhtml` add-on):**
```csharp
// NuGet: Install-Package itext.pdfhtml
using iText.Html2pdf;
using System.IO;
using System;

class HtmlToPdfMigration
{
    static void Main()
    {
        // pdfHTML supports a defined CSS subset; no JavaScript execution
        string html = @"
            <html><body>
                <h1>Invoice #1234</h1>
                <table border='1' cellpadding='4'>
                    <tr><th>Item</th><th>Price</th></tr>
                    <tr><td>Widget A</td><td>$50.00</td></tr>
                    <tr><td>Widget B</td><td>$75.00</td></tr>
                </table>
                <p><b>Total: $125.00</b></p>
            </body></html>";

        using var fs = new FileStream("invoice.pdf", FileMode.Create);
        HtmlConverter.ConvertToPdf(html, fs);
        Console.WriteLine("Saved: invoice.pdf");
    }
}
```

**After (IronPDF — full CSS3 support, shorter):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

string html = @"
    <html><head><style>
        h1 { color: #333; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ccc; padding: 8px; }
        th { background: #f5f5f5; }
    </style></head><body>
        <h1>Invoice #1234</h1>
        <table>
            <tr><th>Item</th><th>Price</th></tr>
            <tr><td>Widget A</td><td>$50.00</td></tr>
            <tr><td>Widget B</td><td>$75.00</td></tr>
        </table>
        <p><strong>Total: $125.00</strong></p>
    </body></html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

---

### 2. Merge PDFs

**Before (current iText `PdfMerger`):**
```csharp
// NuGet: Install-Package itext
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System;

class MergePdfsExample
{
    static void MergePdfs(string[] inputPaths, string outputPath)
    {
        using var writer = new PdfWriter(outputPath);
        using var pdfDoc = new PdfDocument(writer);
        var merger = new PdfMerger(pdfDoc);

        foreach (string path in inputPaths)
        {
            using var sourcePdf = new PdfDocument(new PdfReader(path));
            merger.Merge(sourcePdf, 1, sourcePdf.GetNumberOfPages());
        }

        Console.WriteLine($"Merged {inputPaths.Length} files to {outputPath}");
    }

    static void Main() => MergePdfs(
        new[] { "part1.pdf", "part2.pdf", "part3.pdf" },
        "merged.pdf"
    );
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var merged = PdfDocument.Merge(
    PdfDocument.FromFile("part1.pdf"),
    PdfDocument.FromFile("part2.pdf"),
    PdfDocument.FromFile("part3.pdf")
);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (current iText — canvas overlay with `PdfExtGState`):**
```csharp
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System;

class WatermarkExample
{
    static void Main()
    {
        using var reader = new PdfReader("input.pdf");
        using var writer = new PdfWriter("watermarked.pdf");
        using var pdfDoc = new PdfDocument(reader, writer);

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
                  .ShowText("CONFIDENTIAL")
                  .EndText()
                  .RestoreState();
        }

        Console.WriteLine("Watermarked: watermarked.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("input.pdf");
var stamper = new TextStamper
{
    Text = "CONFIDENTIAL",
    FontColor = "#808080",
    FontSize = 60,
    Opacity = 30,
    Rotation = 45,
    VerticalAlignment = IronSoftware.Drawing.AnchorType.Middle,
    HorizontalAlignment = IronSoftware.Drawing.AnchorType.Middle
};
pdf.ApplyStamp(stamper);
pdf.SaveAs("watermarked.pdf");
// Guide: https://ironpdf.com/how-to/custom-watermark/
```

---

### 4. Password protection

**Before (current iText — `WriterProperties.SetStandardEncryption`):**
```csharp
using iText.Kernel.Pdf;
using System;
using System.Text;

class SecurityExample
{
    static void Main()
    {
        byte[] userPassword  = Encoding.UTF8.GetBytes("readonlyaccess");
        byte[] ownerPassword = Encoding.UTF8.GetBytes("admincontrol");

        var writerProperties = new WriterProperties()
            .SetStandardEncryption(
                userPassword,
                ownerPassword,
                EncryptionConstants.ALLOW_PRINTING,
                EncryptionConstants.ENCRYPTION_AES_128);

        using var reader = new PdfReader("document.pdf");
        using var writer = new PdfWriter("secured.pdf", writerProperties);
        using var pdfDoc = new PdfDocument(reader, writer);

        Console.WriteLine("Encrypted: secured.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("document.pdf");
pdf.SecuritySettings.UserPassword  = "readonlyaccess";
pdf.SecuritySettings.OwnerPassword = "admincontrol";
pdf.SecuritySettings.AllowUserCopyPasteContent = true;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Audit page-index references

iText uses 1-based page numbers throughout — `GetNumberOfPages()`, `GetPage(n)`, and `PdfMerger.Merge(doc, 1, n)` all start at 1. IronPDF's API surface is different: `PageCount` is the total count, and specific page-range methods like `CopyPages(startIndex, endIndex)` and indexing into `pdf.Pages` follow IronPDF's own conventions. Don't assume the iText convention carries over verbatim — audit each call site against the [IronPDF page-manipulation docs](https://ironpdf.com/how-to/copy-pages-from-multiple-pdfs/) when porting loops.

### Document builder → HTML template migration

If your codebase builds PDFs programmatically using `Document`, `Paragraph`, `Table` / `PdfPTable`, and related classes, migrating to IronPDF means rewriting those as HTML templates. This isn't a mechanical substitution — it's a design change.

A practical approach:

1. For each PDF type your system generates, create an HTML template (Razor, Liquid, Handlebars — any templating engine works)
2. Render the HTML template to a string
3. Pass the string to IronPDF's renderer

The HTML template approach is typically more maintainable and easier to iterate on, but it does require the initial rewrite investment.

### AGPL licensing

If AGPL was the migration trigger, confirm IronPDF's commercial license terms cover your distribution model before switching — this is a legal/business decision separate from the technical migration. iText's commercial license is sold as an annual subscription based on PDF volume; IronPDF offers a perpetual option in addition to subscription.

### iText form filling (AcroForms)

If your code uses `PdfAcroForm` (current iText) or `AcroFields` (legacy `iTextSharp`) to fill PDF forms, test IronPDF's [form-filling API](https://ironpdf.com/how-to/form-filling-and-form-creation/) against your specific form types before committing to the migration. Forms are one of the more variable corners of the PDF spec, and behavior differs across renderers.

---

## Performance considerations

### Renderer reuse

```csharp
// Reuse renderer for batch generation
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

foreach (var template in GetDocumentTemplates())
{
    var html = RenderTemplate(template); // your template engine
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"doc_{template.Id}.pdf");
}
```

### Async for web applications

```csharp
// Async pattern for ASP.NET controllers
[HttpGet("generate")]
public async Task<IActionResult> GeneratePdf()
{
    var renderer = new ChromePdfRenderer();
    var html = await _templateEngine.RenderAsync(model);
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);

    return File(pdf.Stream, "application/pdf", "document.pdf");
    // Async guide: https://ironpdf.com/how-to/async/
}
```

### Memory disposal

```csharp
// PdfDocument implements IDisposable
using var pdf = PdfDocument.FromFile("input.pdf");
// ... operations ...
pdf.SaveAs("output.pdf");
// Disposed at end of block
```

### Edge cases worth flagging

- **Complex table layouts** — iText's programmatic table API gives precise control over cell sizing. HTML/CSS tables in a renderer work differently. Test your most complex table layouts explicitly.
- **Exact positioning** — iText supports absolute PDF coordinates (bottom-left origin). In HTML, absolute positioning is CSS-based (top-left origin). The mental model differs.
- **Font embedding** — both libraries embed fonts differently. Check font rendering in output PDFs, especially with custom fonts.

---

## Migration checklist

### Pre-migration

- [ ] Classify all PDF generation code: HTML-based vs programmatic document builder
- [ ] Find all iText / iTextSharp usages: `rg "iText|PdfWriter|PdfStamper|PdfMerger|PdfCopy" --type cs`
- [ ] Identify form-filling (`PdfAcroForm` / `AcroFields`) usage and test IronPDF form API coverage against your forms
- [ ] Identify digital signature usage and test IronPDF signing API against your workflow
- [ ] Check AGPL licensing impact on your project (legal/business, not technical)
- [ ] Inventory HTML templates (if any) for render comparison
- [ ] Confirm IronPDF .NET version compatibility for your target framework
- [ ] Set up an IronPDF trial license in dev environment

### Code migration

- [ ] Remove `itext`, `itext.pdfhtml`, `itext7`, `iTextSharp` NuGet packages
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `using iText.*` / `using iTextSharp.*` with `using IronPdf` (and `using IronPdf.Editing` for stamping)
- [ ] Replace HTML-to-PDF (`HtmlConverter` / `HTMLWorker`) with `ChromePdfRenderer`
- [ ] Replace `PdfMerger` / `PdfCopy` merge pattern with `PdfDocument.Merge()`
- [ ] Replace canvas-overlay watermark / `PdfStamper` watermark with `pdf.ApplyStamp()`
- [ ] Replace `WriterProperties.SetStandardEncryption()` / `PdfStamper.SetEncryption()` with `pdf.SecuritySettings`
- [ ] Audit page-index references (both libraries use 1-based, but mixing collection-style and method-style indexing is a common bug source)
- [ ] Rewrite programmatic document builders as HTML templates (scope this separately)
- [ ] Add IronPDF license key to config

### Testing

- [ ] Render each HTML template and compare output
- [ ] Test merge on representative document sets
- [ ] Test watermark on single-page and multi-page documents
- [ ] Test password protection: confirm correct/incorrect credential behavior
- [ ] Test form filling if applicable
- [ ] Test digital signatures if applicable
- [ ] Load test concurrent rendering at expected peak

### Post-migration

- [ ] Remove iText license file if a commercial subscription was in place
- [ ] Remove iText / iTextSharp from dependency list
- [ ] Update architecture documentation
- [ ] Monitor memory baseline (Chromium renderer vs iText have different profiles)

---

## That's the migration

The hardest part of an iText or iTextSharp migration isn't usually the API mapping — it's the cases where someone spent significant effort building precise document layouts programmatically that now need to be reimplemented as HTML templates. The 1:1 conceptual mapping doesn't exist. That's the honest assessment.

For teams that are primarily HTML-to-PDF (using `HtmlConverter` or `HTMLWorker`), the migration is genuinely straightforward. For teams with complex programmatic document construction, budget more time.

**Which feature was hardest to replicate in your migration, and why?** Particularly interested in teams who had complex table layouts or form-filling requirements — those tend to be where the depth of the feature difference shows up.
