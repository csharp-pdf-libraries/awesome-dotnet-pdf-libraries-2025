# Migration Guide: jsreport → IronPDF

## Why Migrate from jsreport to IronPDF

IronPDF eliminates the complexity of running a separate Node.js server and learning JavaScript templating systems by providing a pure C# solution for PDF generation. With IronPDF, you can work directly with HTML strings, Razor views, or URLs without leaving the .NET ecosystem. This results in simpler deployment, better performance, and seamless integration with your existing C# codebase.

## NuGet Package Changes

**Remove:**
```xml
<PackageReference Include="jsreport.Binary" Version="3.x.x" />
<PackageReference Include="jsreport.Local" Version="3.x.x" />
<PackageReference Include="jsreport.Types" Version="3.x.x" />
```

**Add:**
```xml
<PackageReference Include="IronPdf" Version="2024.x.x" />
```

## Namespace Mapping

| jsreport | IronPDF |
|----------|---------|
| `jsreport.Local` | `IronPdf` |
| `jsreport.Types` | `IronPdf` |
| `jsreport.Binary` | _(not needed)_ |

## API Mapping

| jsreport API | IronPDF API | Notes |
|--------------|-------------|-------|
| `new LocalReporting()` | `new ChromePdfRenderer()` | Main renderer instance |
| `.RenderAsync(report)` | `.RenderHtmlAsPdf(html)` | HTML to PDF conversion |
| `Report` object with `Template` | Direct HTML string | No template wrapper needed |
| `Engine = Engine.Chrome` | _(default)_ | Chrome engine is default |
| `Recipe.ChromePdf` | _(not needed)_ | Built-in rendering |
| `Header` / `Footer` templates | `RenderingOptions.HtmlHeader` / `HtmlFooter` | Direct HTML assignment |
| `Margin` settings | `RenderingOptions.MarginTop/Bottom/Left/Right` | Individual margin properties |

## Before/After Code Examples

### Example 1: Basic HTML to PDF

**Before (jsreport):**
```csharp
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;

var rs = new LocalReporting().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();

var report = await rs.RenderAsync(new RenderRequest
{
    Template = new Template
    {
        Recipe = Recipe.ChromePdf,
        Engine = Engine.None,
        Content = "<h1>Hello World</h1><p>This is a PDF document.</p>"
    }
});

using (var fs = new FileStream("output.pdf", FileMode.Create))
{
    report.Content.CopyTo(fs);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>This is a PDF document.</p>");
pdf.SaveAs("output.pdf");
```

### Example 2: PDF with Header and Footer

**Before (jsreport):**
```csharp
var rs = new LocalReporting().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();

var report = await rs.RenderAsync(new RenderRequest
{
    Template = new Template
    {
        Recipe = Recipe.ChromePdf,
        Engine = Engine.None,
        Content = "<h1>Report Content</h1>",
        Chrome = new Chrome
        {
            HeaderTemplate = "<div style='text-align:center'>Header Text</div>",
            FooterTemplate = "<div style='text-align:center'>Page {#pageNum}/{#numPages}</div>",
            DisplayHeaderFooter = true,
            MarginTop = "1cm",
            MarginBottom = "1cm"
        }
    }
});

using (var fs = new FileStream("report.pdf", FileMode.Create))
{
    report.Content.CopyTo(fs);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Header Text</div>"
};
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>"
};
renderer.RenderingOptions.MarginTop = 10; // millimeters
renderer.RenderingOptions.MarginBottom = 10;

var pdf = renderer.RenderHtmlAsPdf("<h1>Report Content</h1>");
pdf.SaveAs("report.pdf");
```

### Example 3: URL to PDF with Custom Settings

**Before (jsreport):**
```csharp
var rs = new LocalReporting().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();

var report = await rs.RenderAsync(new RenderRequest
{
    Template = new Template
    {
        Recipe = Recipe.ChromePdf,
        Engine = Engine.None,
        Content = "<html><body></body></html>",
        Chrome = new Chrome
        {
            Url = "https://example.com",
            WaitForNetworkIdle = true,
            MediaType = MediaType.Print,
            Format = "A4",
            Landscape = true
        }
    }
});

using (var fs = new FileStream("webpage.pdf", FileMode.Create))
{
    report.Content.CopyTo(fs);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
renderer.RenderingOptions.WaitFor.RenderDelay(500); // Wait for network

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

## Common Gotchas

### 1. **No Server Required**
- **jsreport:** Requires running jsreport server process (either local or remote)
- **IronPDF:** Runs in-process, no separate server needed. Simply instantiate and use.

### 2. **Margin Units**
- **jsreport:** Uses CSS units (cm, in, px) as strings
- **IronPDF:** Uses millimeters as doubles/integers by default

### 3. **Page Numbering Syntax**
- **jsreport:** `{#pageNum}` and `{#numPages}`
- **IronPDF:** `{page}` and `{total-pages}`

### 4. **Async vs Sync**
- **jsreport:** Most operations are async (`RenderAsync`)
- **IronPDF:** Most operations are synchronous by default, but async versions available (`RenderHtmlAsPdfAsync`)

### 5. **Template Engines Not Needed**
- **jsreport:** Often uses Handlebars or other JS templating engines
- **IronPDF:** Use C# string interpolation, Razor views, or any .NET templating directly

### 6. **License Key Configuration**
- **IronPDF:** Requires license key for production use:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 7. **Binary Dependencies**
- **jsreport:** Requires downloading and managing jsreport binaries
- **IronPDF:** Automatically handles Chrome dependencies, no manual setup required

## Additional Resources

- **Official Documentation:** https://ironpdf.com/docs/
- **Tutorials and Examples:** https://ironpdf.com/tutorials/
- **API Reference:** https://ironpdf.com/object-reference/api/