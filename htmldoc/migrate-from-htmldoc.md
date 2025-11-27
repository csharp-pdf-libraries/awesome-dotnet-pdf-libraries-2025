# Migration Guide: HTMLDOC → IronPDF

## Why Migrate from HTMLDOC to IronPDF

HTMLDOC is outdated technology from the early 2000s with minimal modern web standards support and GPL licensing restrictions that can complicate commercial deployments. IronPDF is a modern .NET library with active development, comprehensive HTML5/CSS3 support, and a commercial license suitable for enterprise applications. Migrating enables native .NET integration, better rendering quality, and access to advanced PDF manipulation features.

## NuGet Package Changes

```bash
# Remove HTMLDOC (command-line tool, not a NuGet package)
# No package to uninstall - remove executable and wrapper code

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| HTMLDOC | IronPDF |
|---------|---------|
| N/A (Command-line tool) | `IronPdf` |
| N/A | `IronPdf.Rendering` |
| N/A | `IronPdf.Extensions` |

## API Mapping

| HTMLDOC Command/Flag | IronPDF Equivalent |
|---------------------|-------------------|
| `htmldoc --webpage -f output.pdf input.html` | `ChromePdfRenderer.RenderHtmlFileAsync()` |
| `htmldoc --webpage --header ...` | `ChromePdfRenderer.RenderingOptions.HtmlHeader` |
| `htmldoc --footer ...` | `ChromePdfRenderer.RenderingOptions.HtmlFooter` |
| `htmldoc --size A4` | `ChromePdfRenderer.RenderingOptions.PaperSize = PdfPaperSize.A4` |
| `htmldoc --landscape` | `ChromePdfRenderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape` |
| `htmldoc --left 20mm` | `ChromePdfRenderer.RenderingOptions.MarginLeft = 20` |
| `htmldoc --right 20mm` | `ChromePdfRenderer.RenderingOptions.MarginRight = 20` |
| `htmldoc --top 20mm` | `ChromePdfRenderer.RenderingOptions.MarginTop = 20` |
| `htmldoc --bottom 20mm` | `ChromePdfRenderer.RenderingOptions.MarginBottom = 20` |
| `htmldoc --jpeg` | `ChromePdfRenderer.RenderingOptions.EnableJavaScript = true` |
| N/A (Process execution) | Native .NET async/await methods |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;

var startInfo = new ProcessStartInfo
{
    FileName = "htmldoc",
    Arguments = "--webpage -f output.pdf input.html",
    UseShellExecute = false,
    RedirectStandardOutput = true
};

using (var process = Process.Start(startInfo))
{
    process.WaitForExit();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlFileAsync("input.html");
await pdf.SaveAsAsync("output.pdf");
```

### Example 2: HTML String to PDF with Page Settings

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;
using System.IO;

string htmlContent = "<html><body><h1>Hello World</h1></body></html>";
File.WriteAllText("temp.html", htmlContent);

var startInfo = new ProcessStartInfo
{
    FileName = "htmldoc",
    Arguments = "--webpage --size A4 --landscape --left 20mm --right 20mm -f output.pdf temp.html",
    UseShellExecute = false
};

using (var process = Process.Start(startInfo))
{
    process.WaitForExit();
}

File.Delete("temp.html");
```

**After (IronPDF):**
```csharp
using IronPdf;

string htmlContent = "<html><body><h1>Hello World</h1></body></html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;

var pdf = await renderer.RenderHtmlAsPdfAsync(htmlContent);
await pdf.SaveAsAsync("output.pdf");
```

### Example 3: Adding Headers and Footers

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;

var startInfo = new ProcessStartInfo
{
    FileName = "htmldoc",
    Arguments = "--webpage --header \"...\" --footer \"Page $PAGE of $PAGES\" -f output.pdf input.html",
    UseShellExecute = false
};

using (var process = Process.Start(startInfo))
{
    process.WaitForExit();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Company Report</div>"
};
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center'>Page {page} of {total-pages}</div>"
};

var pdf = await renderer.RenderHtmlFileAsync("input.html");
await pdf.SaveAsAsync("output.pdf");
```

## Common Gotchas

- **No temp files needed**: IronPDF works directly with HTML strings; no need to create temporary HTML files for string content
- **Async patterns**: IronPDF uses modern async/await patterns; update calling code to use `await` or `.Result`
- **Margin units**: IronPDF uses millimeters by default; HTMLDOC flags need explicit unit specification
- **License key required**: Add `IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";` at application startup for production use
- **First render slower**: Initial PDF generation may be slower as IronPDF initializes the Chrome engine; subsequent renders are faster
- **CSS support**: IronPDF supports modern CSS3, Flexbox, and Grid; you may need to update legacy CSS from HTMLDOC era
- **JavaScript execution**: Unlike HTMLDOC, IronPDF can execute JavaScript in HTML (enable with `RenderingOptions.EnableJavaScript`)
- **Error handling**: Replace process exit code checks with standard .NET exception handling using try/catch blocks

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **API Reference**: Full IntelliSense support available in Visual Studio