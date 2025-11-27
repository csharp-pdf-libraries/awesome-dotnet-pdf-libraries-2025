# Migration Guide: wkhtmltopdf → IronPDF

## Why Migrate

wkhtmltopdf suffers from a critical SSRF vulnerability (CVE-2022-35583, severity 9.8) that remains unpatched due to project abandonment. The project has received no meaningful updates since 2016-2017, relies on Qt WebKit from 2015, and lacks modern web standards support (CSS Grid, flexbox, ES6+). IronPDF provides active security maintenance, modern rendering with Chromium, and comprehensive .NET integration.

## NuGet Package Changes

```bash
# Remove wkhtmltopdf wrapper (if using)
dotnet remove package WkHtmlToPdf-DotNet
# or
dotnet remove package TuesPechkin

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| wkhtmltopdf (C# Wrapper) | IronPDF |
|--------------------------|---------|
| `WkHtmlToPdfDotNet` | `IronPdf` |
| `TuesPechkin` | `IronPdf` |
| N/A (CLI tool) | `IronPdf.ChromePdfRenderer` |

## API Mapping Table

| wkhtmltopdf Concept | IronPDF Equivalent |
|---------------------|-------------------|
| `wkhtmltopdf [options] input.html output.pdf` | `ChromePdfRenderer.RenderHtmlFileAsPdf()` |
| `--page-size A4` | `PdfPrintOptions.PaperSize = PaperSize.A4` |
| `--orientation Landscape` | `PdfPrintOptions.PaperOrientation = PdfPaperOrientation.Landscape` |
| `--margin-top 10mm` | `PdfPrintOptions.MarginTop = 10` |
| `--header-html header.html` | `PdfPrintOptions.Header = new HtmlHeaderFooter()` |
| `--footer-center "Page [page]"` | `PdfPrintOptions.Footer = new TextHeaderFooter()` |
| `--enable-javascript` | Enabled by default |
| `--print-media-type` | `PdfPrintOptions.CssMediaType = CssMediaType.Print` |
| `--dpi 300` | `PdfPrintOptions.Dpi = 300` |
| `--enable-local-file-access` | `ChromePdfRenderOptions.EnableJavaScript = true` |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (wkhtmltopdf CLI):**
```bash
wkhtmltopdf input.html output.pdf
```

**Before (C# wrapper):**
```csharp
using WkHtmlToPdfDotNet;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = { Out = "output.pdf" },
    Objects = { new ObjectSettings { Page = "input.html" } }
};
converter.Convert(doc);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 2: HTML String with Custom Page Settings

**Before (wkhtmltopdf CLI):**
```bash
echo "<h1>Hello World</h1>" | wkhtmltopdf --page-size A4 --orientation Landscape - output.pdf
```

**Before (C# wrapper):**
```csharp
using WkHtmlToPdfDotNet;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = 
    { 
        Out = "output.pdf",
        PaperSize = PaperKind.A4,
        Orientation = Orientation.Landscape
    },
    Objects = 
    { 
        new ObjectSettings 
        { 
            HtmlContent = "<h1>Hello World</h1>" 
        } 
    }
};
converter.Convert(doc);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 3: Headers, Footers, and Margins

**Before (wkhtmltopdf CLI):**
```bash
wkhtmltopdf \
  --margin-top 20mm \
  --margin-bottom 20mm \
  --header-center "My Report" \
  --footer-center "Page [page] of [toPage]" \
  input.html output.pdf
```

**Before (C# wrapper):**
```csharp
using WkHtmlToPdfDotNet;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    GlobalSettings = 
    { 
        Out = "output.pdf",
        Margins = new MarginSettings { Top = 20, Bottom = 20 }
    },
    Objects = 
    { 
        new ObjectSettings 
        { 
            Page = "input.html",
            HeaderSettings = { Center = "My Report" },
            FooterSettings = { Center = "Page [page] of [toPage]" }
        } 
    }
};
converter.Convert(doc);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;

renderer.RenderingOptions.Header = new TextHeaderFooter
{
    CenterText = "My Report",
    DrawDividerLine = true
};

renderer.RenderingOptions.Footer = new TextHeaderFooter
{
    CenterText = "{page} of {total-pages}",
    DrawDividerLine = true
};

var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

## Common Gotchas

### 1. **Page Number Placeholders**
- **wkhtmltopdf:** `[page]`, `[toPage]`
- **IronPDF:** `{page}`, `{total-pages}`

### 2. **Margins Are in Millimeters by Default**
```csharp
// IronPDF uses millimeters
renderer.RenderingOptions.MarginTop = 20; // 20mm
```

### 3. **Licensing Required for Production**
IronPDF requires a license key for production use (free for development):
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 4. **Modern CSS Works Out of the Box**
IronPDF uses Chromium, so CSS Grid, Flexbox, and modern JavaScript work without configuration—no need for polyfills or workarounds.

### 5. **Async Operations Available**
Unlike wkhtmltopdf wrappers, IronPDF supports async patterns:
```csharp
var pdf = await renderer.RenderHtmlAsPdfAsync(htmlContent);
```

### 6. **Path Resolution**
IronPDF resolves relative paths differently. Use absolute paths or set:
```csharp
renderer.RenderingOptions.BaseUrl = new Uri("file:///path/to/directory/");
```

### 7. **JavaScript Execution**
JavaScript is enabled by default. If you need to wait for JS completion:
```csharp
renderer.RenderingOptions.RenderDelay = 500; // milliseconds
renderer.RenderingOptions.WaitFor.RenderDelay(500);
```

## Additional Resources

- **Documentation:** https://ironpdf.com/docs/
- **Tutorials:** https://ironpdf.com/tutorials/
- **API Reference:** https://ironpdf.com/docs/api/