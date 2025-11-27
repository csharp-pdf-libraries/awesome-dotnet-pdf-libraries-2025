# Migration Guide: TuesPechkin → IronPDF

## Why Migrate

TuesPechkin wraps the legacy wkhtmltopdf library, requiring complex thread management with `ThreadSafeConverter` that still crashes under high load. It inherits all wkhtmltopdf security vulnerabilities (CVEs) and rendering limitations. IronPDF provides a modern, thread-safe API with active maintenance, superior HTML/CSS rendering, and enterprise-grade reliability without manual thread coordination.

## NuGet Package Changes

```bash
# Remove TuesPechkin
dotnet remove package TuesPechkin

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| TuesPechkin | IronPDF |
|-------------|---------|
| `TuesPechkin` | `IronPdf` |
| `TuesPechkin.Wkhtmltox.PdfConvert` | `IronPdf.ChromePdfRenderer` |
| `TuesPechkin.Wkhtmltox.PdfSettings` | `IronPdf.ChromePdfRenderOptions` |

## API Mapping

| TuesPechkin | IronPDF |
|-------------|---------|
| `ThreadSafeConverter` | No equivalent needed (inherently thread-safe) |
| `Convert(HtmlToPdfDocument)` | `RenderHtmlAsPdf(html)` |
| `ConvertHtmlToPdf(string)` | `RenderHtmlAsPdf(html)` |
| `ObjectSettings.PageUrl` | `RenderUrlAsPdf(url)` |
| `GlobalSettings.Orientation` | `ChromePdfRenderOptions.PaperOrientation` |
| `GlobalSettings.PaperSize` | `ChromePdfRenderOptions.PaperSize` |
| `HeaderSettings` | `ChromePdfRenderOptions.TextHeader` |
| `FooterSettings` | `ChromePdfRenderOptions.TextFooter` |

## Code Examples

### Example 1: Basic HTML to PDF

**Before (TuesPechkin):**
```csharp
using TuesPechkin;

var converter = new ThreadSafeConverter(
    new RemotingToolset<PdfToolset>(
        new Win64EmbeddedDeployment(
            new TempFolderDeployment())));

var html = "<h1>Hello World</h1>";
var document = new HtmlToPdfDocument
{
    GlobalSettings = { PaperSize = PaperKind.A4 },
    Objects = { new ObjectSettings { HtmlText = html } }
};

byte[] pdf = converter.Convert(document);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var html = "<h1>Hello World</h1>";

PdfDocument pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Settings

**Before (TuesPechkin):**
```csharp
using TuesPechkin;

var converter = new ThreadSafeConverter(
    new RemotingToolset<PdfToolset>(
        new Win64EmbeddedDeployment(
            new TempFolderDeployment())));

var document = new HtmlToPdfDocument
{
    GlobalSettings = 
    {
        PaperSize = PaperKind.A4,
        Orientation = GlobalSettings.PdfOrientation.Landscape,
        Margins = new MarginSettings { Unit = Unit.Millimeters, Top = 10, Bottom = 10 }
    },
    Objects = 
    {
        new ObjectSettings 
        { 
            PageUrl = "https://example.com",
            WebSettings = { EnableJavascript = true }
        }
    }
};

byte[] pdf = converter.Convert(document);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.EnableJavaScript = true;

PdfDocument pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 3: Headers and Footers

**Before (TuesPechkin):**
```csharp
using TuesPechkin;

var converter = new ThreadSafeConverter(
    new RemotingToolset<PdfToolset>(
        new Win64EmbeddedDeployment(
            new TempFolderDeployment())));

var document = new HtmlToPdfDocument
{
    GlobalSettings = { PaperSize = PaperKind.A4 },
    Objects = 
    {
        new ObjectSettings 
        { 
            HtmlText = "<h1>Content</h1>",
            HeaderSettings = 
            {
                HtmlUrl = "header.html",
                Line = true
            },
            FooterSettings = 
            {
                RightText = "Page [page] of [topage]",
                Line = true
            }
        }
    }
};

byte[] pdf = converter.Convert(document);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.TextHeader = new TextHeaderFooter
{
    DrawDividerLine = true,
    CenterText = "My Header"
};
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    DrawDividerLine = true,
    RightText = "Page {page} of {total-pages}"
};

PdfDocument pdf = renderer.RenderHtmlAsPdf("<h1>Content</h1>");
pdf.SaveAs("output.pdf");
```

## Common Gotchas

| Issue | Solution |
|-------|----------|
| **No more ThreadSafeConverter** | Remove all thread management code - IronPDF handles this automatically |
| **Different paper size enums** | Use `PdfPaperSize` instead of `PaperKind` |
| **Page number placeholders** | Change `[page]` to `{page}`, `[topage]` to `{total-pages}` |
| **Deployment differences** | No manual deployment needed - IronPDF manages Chrome rendering engine internally |
| **Settings organization** | TuesPechkin splits settings into Global/Object - IronPDF uses `ChromePdfRenderOptions` for all settings |
| **Return types** | TuesPechkin returns `byte[]` - IronPDF returns `PdfDocument` object with additional methods |
| **JavaScript execution** | JavaScript enabled by default in IronPDF, configure with `EnableJavaScript` property if needed |

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)