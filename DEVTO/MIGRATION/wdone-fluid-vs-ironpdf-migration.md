# Migrating from Fluid (Templating) to IronPDF: a .NET developer migration notes

Fluid is a Liquid-syntax template engine for .NET — it renders strings from templates and data. What it isn't is a PDF generator. Teams building PDF pipelines around Fluid are typically doing something like: render HTML string via Fluid, then pass that string to a PDF library. If the PDF library they're passing that string to is the problem, this article is for you.

This guide covers migrating the PDF-generation half of a Fluid-based pipeline — replacing whatever is currently converting the Fluid output to PDF with IronPDF. Fluid itself continues to do what it's good at: template rendering.

By the end, you'll have a working migration scaffold, API mappings for common PDF operations, and four before/after code examples showing Fluid HTML output feeding into IronPDF.

---

## Why Migrate the PDF Layer (Without Drama)

Fluid stays in this migration. The trigger is the PDF library feeding off Fluid's output. Teams hit these:

1. **CSS fidelity** — Fluid generates the HTML; the downstream PDF library may not render modern CSS. Chromium-based rendering resolves this without changing templates.
2. **JavaScript in templates** — Fluid-generated HTML may include JS-dependent components (charts, dynamic tables). Most non-Chromium PDF libraries can't run this JS.
3. **@font-face and custom fonts** — Font embedding in the PDF depends on the renderer, not the template engine.
4. **Linux container rendering** — The PDF renderer, not Fluid, is the source of Linux dependency issues.
5. **Async pipeline** — Fluid's rendering is synchronous or easily awaitable. The PDF layer is often the async bottleneck.
6. **Output file size** — PDF compression and optimization are renderer responsibilities.
7. **Watermarking and post-processing** — Adding watermarks, page numbers, or security to the generated PDF requires PDF library features, not template features.
8. **URL asset loading** — Fluid-generated HTML referencing external CSS/JS requires a PDF renderer that can follow those URLs. Verify this behavior in your current library.
9. **Print media CSS** — `@media print` rules in Fluid templates need a renderer that honors them.
10. **Merge multiple rendered outputs** — If you're merging multiple Fluid-rendered HTML documents into one PDF, the merge API lives in the PDF library.

### Side-by-Side Comparison

| Aspect | Fluid (template layer) | IronPDF (render layer) |
|---|---|---|
| Focus | Liquid template → string | HTML/URL → PDF via Chromium |
| Pricing | MIT open source | Commercial — verify current |
| API Style | Parser/render pattern | Renderer with options |
| Learning Curve | Low (Liquid syntax) | Low for HTML-first workflows |
| HTML Rendering | Not applicable — string output only | Chromium, full CSS/JS |
| Page Indexing | N/A | 0-based |
| Thread Safety | Fluid parser is reusable | `ChromePdfRenderer` reusable |
| Namespace | `Fluid` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Complexity | Notes |
|---|---|---|
| HTML string → PDF | Low | Replace downstream PDF library call only |
| Fluid → HTML → PDF pipeline | Low | Fluid stays; only PDF call changes |
| CSS rendering fidelity fix | Low | Switch to IronPDF; template unchanged |
| JS chart rendering | Low | New capability; template may already have charts |
| Multi-template merge | Medium | Render each → merge PDFs |
| Watermark on output | Medium | Post-generation stamp |
| Password protect output | Low | Security settings on rendered PDF |
| Per-page headers/footers | Medium | Move from template to IronPDF HTML header |
| Stream output (no disk) | Low | `pdf.BinaryData` or stream |
| PDF/A compliance | Medium | Verify IronPDF PDF/A options |

### Decision Matrix

| Scenario | Recommendation |
|---|---|
| Fluid templates work; downstream PDF renderer has CSS issues | Replace PDF renderer, keep Fluid |
| Fluid templates are fine; PDF library can't render JS | Replace PDF renderer, keep Fluid |
| Need to merge multiple Fluid-rendered PDFs | Add `PdfDocument.Merge()` to existing pipeline |
| Want watermarks or security on Fluid output | Add IronPDF post-processing step |

---

## Before You Start

### Pre-Migration Checklist

Before touching any code:

- [ ] Identify which PDF library is currently receiving Fluid's HTML output (this is what's being replaced)
- [ ] List all Fluid template files and confirm none have embedded PDF logic
- [ ] Run `rg "\.Render\(" --type cs` to find Fluid render calls — these precede the PDF calls
- [ ] Run `rg "(PdfConverter|HtmlToPdf|PdfDocument)\.(Convert|Render|Get)" --type cs` — these are the PDF calls being replaced
- [ ] Confirm your .NET target is IronPDF-compatible
- [ ] Verify IronPDF license key availability in all environments
- [ ] Check CSS complexity in Fluid templates — any flexbox, grid, custom fonts worth testing first
- [ ] Check if templates use `{% javascript %}` or similar — note which templates will benefit from JS execution

### Find Fluid + PDF Coupling Points

```bash
# Find all Fluid template render calls
rg "\.Render\(|\.RenderAsync\(" --type cs -l

# Find the downstream PDF conversion calls — adjust pattern to your current library
rg "HtmlToPdf\|PdfConvert\|ConvertHtml\|GetPdfBytes" --type cs

# Find places where Fluid output feeds into PDF
rg -A 5 "\.Render\(" --type cs | grep -E "pdf|Pdf|PDF"

# Find any existing IronPDF usage to avoid conflicts
rg "IronPdf\|ChromePdfRenderer" --type cs
```

### Uninstall / Install

```bash
# Remove the old PDF library (replace with your actual package name)
# dotnet remove package [OldPdfLibrary]

# Install IronPDF
dotnet add package IronPdf

# Fluid stays installed
dotnet list package | grep -E "Fluid|IronPdf"
```

---

## Quick Start Migration (3 Steps)

### Step 1: License

```csharp
// Fluid: no license needed (MIT)
// IronPDF: set license before first use
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// Docs: https://ironpdf.com/how-to/license-keys/
// Or via environment variable: IRONPDF_LICENSEKEY
// Or appsettings.json — verify current config options in docs
```

### Step 2: Namespace Imports

```csharp
// Keep Fluid imports
using Fluid;
using Fluid.Values;

// Add IronPDF
using IronPdf;
// Remove old PDF library import
```

### Step 3: Basic Fluid → IronPDF Pipeline

```csharp
// Fluid renders HTML — unchanged
var parser = new FluidParser();
if (!parser.TryParse(liquidTemplate, out var template, out var error))
    throw new Exception($"Template parse error: {error}");

var context = new TemplateContext();
context.SetValue("name", "Alice");
string html = await template.RenderAsync(context);

// Before: old PDF library
// var pdf = OldLibrary.Convert(html); // whatever was here

// After: IronPDF
var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// Docs: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| Old PDF Library | IronPDF | Notes |
|---|---|---|
| (varies by library) | `IronPdf` | Replace old PDF namespace |
| Fluid stays as `Fluid` | `Fluid` (unchanged) | Template layer not affected |
| Old renderer class | `IronPdf.ChromePdfRenderer` | Main renderer |

### Core Class Mapping

| Old PDF Class | IronPDF Class | Description |
|---|---|---|
| Old HTML-to-PDF converter | `ChromePdfRenderer` | Rendering engine |
| Old document object | `IronPdf.PdfDocument` | Document object |
| Old options/settings | `ChromePdfRenderOptions` | Rendering configuration |
| Old security settings | `PdfDocument.SecuritySettings` | Password/permissions |

### Document Loading

| Operation | Old Library | IronPDF |
|---|---|---|
| HTML string → PDF | varies | `renderer.RenderHtmlAsPdf(html)` |
| URL → PDF | varies | `renderer.RenderUrlAsPdf(url)` |
| Load from file | varies | `PdfDocument.FromFile(path)` |
| Load from stream | varies | `PdfDocument.FromStream(stream)` |

### Page Operations

| Operation | Old Library | IronPDF |
|---|---|---|
| Page count | varies | `pdf.PageCount` |
| Get page | varies | `pdf.Pages[i]` |
| Remove page | varies | `pdf.RemovePage(index)` |
| Paper size | varies | `RenderingOptions.PaperSize` |

### Merge / Split

| Operation | Old Library | IronPDF |
|---|---|---|
| Merge two PDFs | varies | `PdfDocument.Merge(a, b)` |
| Split by page range | varies | `pdf.CopyPages(start, end)` |

---

## Four Complete Before/After Migrations

### 1. Fluid HTML → PDF (Core Pipeline)

**Before (Fluid + placeholder for old PDF library):**

```csharp
using Fluid;
// using OldPdfLibrary; // your actual library

class Program
{
    static async Task Main()
    {
        // Fluid template rendering — this part stays
        string liquidTemplate = @"
            <html>
            <body>
                <h1>Invoice #{{ invoice.number }}</h1>
                <p>Customer: {{ invoice.customer }}</p>
                <p>Total: {{ invoice.total | currency }}</p>
                <table>
                    {% for item in invoice.items %}
                    <tr><td>{{ item.name }}</td><td>{{ item.price }}</td></tr>
                    {% endfor %}
                </table>
            </body>
            </html>";

        var parser = new FluidParser();
        parser.TryParse(liquidTemplate, out var template, out _);

        var context = new TemplateContext();
        context.SetValue("invoice", new { number = "INV-001", customer = "Acme Corp",
            total = 500.00m, items = new[] { new { name = "Widget", price = 500.00m } } });

        string html = await template.RenderAsync(context);

        // Old PDF conversion — replace this part
        // var pdfBytes = OldLibrary.ConvertHtml(html); // verify your old API
        // File.WriteAllBytes("invoice.pdf", pdfBytes);

        Console.WriteLine("Generated HTML, PDF conversion was here");
    }
}
```

**After (Fluid + IronPDF):**

```csharp
using Fluid;
using IronPdf;

class Program
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        // Docs: https://ironpdf.com/how-to/license-keys/

        // Fluid template rendering — unchanged from before
        string liquidTemplate = @"
            <html>
            <body>
                <h1>Invoice #{{ invoice.number }}</h1>
                <p>Customer: {{ invoice.customer }}</p>
                <p>Total: {{ invoice.total | currency }}</p>
                <table>
                    {% for item in invoice.items %}
                    <tr><td>{{ item.name }}</td><td>{{ item.price }}</td></tr>
                    {% endfor %}
                </table>
            </body>
            </html>";

        var parser = new FluidParser();
        parser.TryParse(liquidTemplate, out var template, out _);

        var context = new TemplateContext();
        context.SetValue("invoice", new { number = "INV-001", customer = "Acme Corp" });

        string html = await template.RenderAsync(context);

        // IronPDF renders the HTML output from Fluid
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
        // Docs: https://ironpdf.com/how-to/rendering-options/

        using var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("invoice.pdf");
    }
}
```

---

### 2. Merge Multiple Fluid-Rendered PDFs

**Before:**

```csharp
using Fluid;
// using OldPdfLibrary;

class MergeSample
{
    static async Task Main()
    {
        var parser = new FluidParser();
        var templates = new[] { "template_a.liquid", "template_b.liquid" };
        var renderedPdfs = new List<byte[]>();

        foreach (var templatePath in templates)
        {
            var liquidText = await File.ReadAllTextAsync(templatePath);
            parser.TryParse(liquidText, out var template, out _);

            var context = new TemplateContext();
            string html = await template.RenderAsync(context);

            // Old PDF conversion — replace
            // byte[] pdfBytes = OldLibrary.ConvertHtml(html); // verify
            // renderedPdfs.Add(pdfBytes);
        }

        // Old merge — verify old library's merge API
        // byte[] merged = OldLibrary.Merge(renderedPdfs.ToArray()); // verify
        // File.WriteAllBytes("merged.pdf", merged);
    }
}
```

**After:**

```csharp
using Fluid;
using IronPdf;

class MergeSample
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var parser = new FluidParser();
        var renderer = new ChromePdfRenderer(); // reuse across renders

        var pdfs = new List<PdfDocument>();

        foreach (var templatePath in new[] { "template_a.liquid", "template_b.liquid" })
        {
            var liquidText = await File.ReadAllTextAsync(templatePath);
            parser.TryParse(liquidText, out var template, out _);

            var context = new TemplateContext();
            string html = await template.RenderAsync(context);

            // Note: don't using here — we need these for merge below
            pdfs.Add(renderer.RenderHtmlAsPdf(html));
        }

        // Merge all rendered PDFs
        using var merged = PdfDocument.Merge(pdfs);
        merged.SaveAs("merged.pdf");
        // Docs: https://ironpdf.com/how-to/merge-or-split-pdfs/

        // Dispose individual PDFs after merge
        foreach (var pdf in pdfs) pdf.Dispose();
    }
}
```

---

### 3. Watermark on Fluid Output

**Before:**

```csharp
using Fluid;

class WatermarkSample
{
    static async Task Main()
    {
        var parser = new FluidParser();
        parser.TryParse("<html><body><h1>{{ title }}</h1></body></html>", out var template, out _);

        var context = new TemplateContext();
        context.SetValue("title", "Confidential Report");
        string html = await template.RenderAsync(context);

        // Old PDF + watermark — verify old library approach
        // var converter = new OldConverter();
        // converter.WatermarkText = "CONFIDENTIAL"; // verify
        // byte[] pdfBytes = converter.Convert(html); // verify
        // File.WriteAllBytes("watermarked.pdf", pdfBytes);
    }
}
```

**After:**

```csharp
using Fluid;
using IronPdf;
using IronPdf.Editing;

class WatermarkSample
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var parser = new FluidParser();
        parser.TryParse("<html><body><h1>{{ title }}</h1></body></html>", out var template, out _);

        var context = new TemplateContext();
        context.SetValue("title", "Confidential Report");
        string html = await template.RenderAsync(context);

        // Render via IronPDF
        var renderer = new ChromePdfRenderer();
        using var pdf = renderer.RenderHtmlAsPdf(html);

        // Apply watermark as text stamp
        var stamper = new TextStamper
        {
            Text = "CONFIDENTIAL",
            FontSize = 45,
            Opacity = 25,
            Rotation = 45
        };

        pdf.ApplyStamp(stamper);
        pdf.SaveAs("watermarked.pdf");
        // Docs: https://ironpdf.com/how-to/stamp-text-image/
    }
}
```

---

### 4. Password-Protected Fluid Output

**Before:**

```csharp
using Fluid;

class SecuritySample
{
    static async Task Main()
    {
        var parser = new FluidParser();
        parser.TryParse("<html><body><p>{{ content }}</p></body></html>", out var template, out _);

        var context = new TemplateContext();
        context.SetValue("content", "Sensitive document content");
        string html = await template.RenderAsync(context);

        // Old PDF + security — verify old library
        // var converter = new OldConverter();
        // converter.UserPassword = "user123"; // verify
        // converter.OwnerPassword = "owner456"; // verify
        // byte[] pdfBytes = converter.Convert(html); // verify
        // File.WriteAllBytes("secured.pdf", pdfBytes);
    }
}
```

**After:**

```csharp
using Fluid;
using IronPdf;
using IronPdf.Security;

class SecuritySample
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var parser = new FluidParser();
        parser.TryParse("<html><body><p>{{ content }}</p></body></html>", out var template, out _);

        var context = new TemplateContext();
        context.SetValue("content", "Sensitive document content");
        string html = await template.RenderAsync(context);

        var renderer = new ChromePdfRenderer();
        using var pdf = renderer.RenderHtmlAsPdf(html);

        // Docs: https://ironpdf.com/how-to/pdf-permissions-passwords/
        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;

        pdf.SaveAs("secured.pdf");
    }
}
```

---

## Critical Migration Notes

### Fluid is Not Being Replaced

The single most important thing to understand: this migration touches only the PDF-generation layer. Fluid's template parsing, the `FluidParser`, `TemplateContext`, and `template.RenderAsync()` calls are entirely unchanged. If you're making changes to those, that's a separate project.

### HTML Escaping and Fluid

Fluid escapes HTML by default. If your templates contain HTML that you want to render (not display as escaped text), you may need `| raw` filters in your templates. This behavior doesn't change with IronPDF — it's a Fluid concern — but it's worth confirming your templates output valid HTML rather than HTML-escaped text.

```liquid
{# Escaped (default) — shows as text in PDF #}
{{ html_content }}

{# Unescaped — renders as HTML #}
{{ html_content | raw }}
```

### Asset URLs in Fluid Templates

If your Fluid templates include relative URLs to CSS, images, or fonts (e.g., `/assets/report.css`), IronPDF needs to resolve these. For local files, use `BaseUrl` on the rendering options:

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.BaseUrl = new Uri("https://your-domain.com/");
// or for local file: new Uri("file:///path/to/assets/")
```

### Async Pipeline Integration

Both Fluid and IronPDF support async. The clean pattern for ASP.NET:

```csharp
// Fully async Fluid → IronPDF pipeline
public async Task<byte[]> GeneratePdfAsync(string template, object model)
{
    // Fluid render (async)
    var fluidTemplate = _parser.Parse(template);
    var context = new TemplateContext(model);
    string html = await fluidTemplate.RenderAsync(context);

    // IronPDF render (async)
    using var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
    // Async docs: https://ironpdf.com/how-to/async/
}
```

---

## Performance Considerations

### Renderer Lifecycle

```csharp
// Register as singleton in DI
builder.Services.AddSingleton<ChromePdfRenderer>(sp =>
{
    IronPdf.License.LicenseKey = sp.GetRequiredService<IConfiguration>()["IronPdf:LicenseKey"];
    return new ChromePdfRenderer();
});

// Fluid parser can also be singleton
builder.Services.AddSingleton<FluidParser>();
```

### Warm-Up Pattern

```csharp
// IHostedService warm-up
public async Task StartAsync(CancellationToken ct)
{
    using var _ = await _renderer.RenderHtmlAsPdfAsync("<p>warmup</p>");
    _logger.LogInformation("IronPDF renderer initialized");
}
```

### Batch Rendering

If generating many PDFs from different Fluid templates (e.g., batch invoice run), parallel rendering is possible. See [IronPDF parallel examples](https://ironpdf.com/examples/parallel/). Fluid's parser is thread-safe; `ChromePdfRenderer` is reusable.

```csharp
// Parallel batch: Fluid render → IronPDF per item
var tasks = orders.Select(async order =>
{
    var context = new TemplateContext(order);
    string html = await _template.RenderAsync(context);

    using var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
    return (order.Id, pdf.BinaryData);
});

var results = await Task.WhenAll(tasks);
```

---

## Migration Checklist

### Pre-Migration
- [ ] Confirm Fluid stays installed — this is a PDF-layer-only migration
- [ ] Identify current downstream PDF library (`rg "HtmlToPdf\|PdfConvert\|GetPdfBytes" --type cs`)
- [ ] List all Fluid render sites that feed into PDF conversion
- [ ] Check for `| raw` filter usage — confirms HTML is passing through correctly
- [ ] Check for relative URL references in templates — plan `BaseUrl` config
- [ ] Confirm .NET target is IronPDF-compatible
- [ ] Set up IronPDF license in all environments (dev, staging, prod)
- [ ] Test IronPDF Linux deps on your deployment container

### Code Migration
- [ ] Add `dotnet add package IronPdf`
- [ ] Add `using IronPdf;` to PDF generation files
- [ ] Set `IronPdf.License.LicenseKey` before first render
- [ ] Register `ChromePdfRenderer` as singleton in DI
- [ ] Replace old PDF conversion calls with `renderer.RenderHtmlAsPdf(html)`
- [ ] Add `BaseUrl` if templates use relative asset references
- [ ] Replace old merge calls with `PdfDocument.Merge()`
- [ ] Replace old watermark approach with `TextStamper` or HTML stamp
- [ ] Replace old security settings with `SecuritySettings`
- [ ] Add `using` disposal to all `PdfDocument` instances
- [ ] Update async render calls to `RenderHtmlAsPdfAsync()`

### Testing
- [ ] Render each Fluid template through IronPDF and visual-diff output
- [ ] Test templates with CSS flexbox/grid (previously broken?)
- [ ] Test templates with `{{ var | raw }}` — confirm HTML rendered, not escaped
- [ ] Test asset URL resolution if using `BaseUrl`
- [ ] Test merge of multiple Fluid-rendered PDFs
- [ ] Test watermark output
- [ ] Test password-protected output with a PDF reader
- [ ] Test async pipeline under concurrent load
- [ ] Verify memory stability after 30-min load test

### Post-Migration
- [ ] Remove old PDF library NuGet package
- [ ] Remove old PDF library `using` statements
- [ ] Update Dockerfile for IronPDF Linux deps if needed
- [ ] Update application startup to warm up renderer
- [ ] Document the Fluid → IronPDF pipeline for new developers

---

## One Last Thing

The Fluid migration is clean because Fluid and IronPDF don't overlap — Fluid produces HTML strings, IronPDF consumes them. The migration is entirely in the seam between those two operations. If that seam was previously filled by a custom implementation or a library with CSS fidelity issues, the replacement is additive rather than disruptive.

The one area worth extra attention: Fluid's HTML escaping behavior and whether your templates are using `| raw` correctly. A template that looked fine in a browser may have subtly different behavior in a PDF renderer depending on how HTML content flows through Liquid filters.

**Technical question for comments:** For teams using Liquid templates (Fluid or DotLiquid) for PDF generation — how do you handle templates that need different layouts for different paper sizes (A4 vs. Letter vs. Legal)? Separate template files, CSS media queries, or a conditional approach in the template itself?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
