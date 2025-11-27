# Migration Guide: PrinceXML → IronPDF

## Why Migrate

IronPDF is a native .NET library that eliminates the need for separate server processes and complex command-line integrations. Unlike PrinceXML's CSS Paged Media approach, IronPDF uses modern Chromium rendering for consistent HTML-to-PDF conversion directly within your .NET application. This results in simpler deployment, better maintainability, and native integration with .NET ecosystems.

## NuGet Package Changes

```xml
<!-- Remove PrinceXML wrapper (if using one) -->
<!-- <PackageReference Include="Prince.Wrapper" Version="*" /> -->

<!-- Add IronPDF -->
<PackageReference Include="IronPdf" Version="2024.*" />
```

## Namespace Mapping

| PrinceXML | IronPDF |
|-----------|---------|
| N/A (command-line) | `IronPdf` |
| N/A | `IronPdf.Rendering` |
| N/A | `IronPdf.Extensions` |

## API Mapping

| PrinceXML Approach | IronPDF Equivalent |
|--------------------|-------------------|
| `prince input.html -o output.pdf` | `ChromePdfRenderer.RenderHtmlFileAsPdf()` |
| `prince --style=custom.css input.html` | `ChromePdfRenderer.RenderingOptions.CssMediaType` |
| `prince --javascript` | `ChromePdfRenderer.RenderingOptions.EnableJavaScript` |
| `prince --baseurl=http://example.com` | `ChromePdfRenderer.RenderingOptions.HtmlHeader/HtmlFooter` |
| Process.Start() wrapper | Direct method calls |
| File-based input/output | String, file, URL, or stream-based |
| `--page-size` | `ChromePdfRenderer.RenderingOptions.PaperSize` |
| `--page-margin` | `ChromePdfRenderer.RenderingOptions.MarginTop/Bottom/Left/Right` |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (PrinceXML):**
```csharp
using System.Diagnostics;

var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "prince",
        Arguments = "input.html -o output.pdf",
        UseShellExecute = false,
        RedirectStandardError = true
    }
};
process.Start();
process.WaitForExit();

if (process.ExitCode != 0)
{
    var error = process.StandardError.ReadToEnd();
    throw new Exception($"Prince failed: {error}");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 2: HTML String with Custom CSS

**Before (PrinceXML):**
```csharp
using System.Diagnostics;
using System.IO;

// Write HTML to temp file
var tempHtml = Path.GetTempFileName() + ".html";
File.WriteAllText(tempHtml, htmlContent);

var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "prince",
        Arguments = $"--style=custom.css {tempHtml} -o output.pdf",
        UseShellExecute = false
    }
};
process.Start();
process.WaitForExit();

File.Delete(tempHtml);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

### Example 3: Custom Page Settings and Margins

**Before (PrinceXML):**
```csharp
using System.Diagnostics;

var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "prince",
        Arguments = "--page-size=Letter --page-margin=1in input.html -o output.pdf",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    }
};
process.Start();
var output = process.StandardOutput.ReadToEnd();
var error = process.StandardError.ReadToEnd();
process.WaitForExit();
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Rendering;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
renderer.RenderingOptions.MarginTop = 72;    // 1 inch = 72 points
renderer.RenderingOptions.MarginBottom = 72;
renderer.RenderingOptions.MarginLeft = 72;
renderer.RenderingOptions.MarginRight = 72;

var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

## Common Gotchas

### 1. CSS Paged Media Not Supported
PrinceXML's `@page` CSS rules and paged media features are specific to Prince. IronPDF uses Chromium rendering, so use standard CSS and IronPDF's rendering options instead.

**Solution:** Set page properties via `RenderingOptions` rather than CSS:
```csharp
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
```

### 2. No Command-Line Arguments
IronPDF is a library, not a CLI tool. All configuration is done through C# properties.

**Solution:** Replace argument strings with strongly-typed properties:
```csharp
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.RenderDelay = 500; // milliseconds
```

### 3. File Path vs. Content
PrinceXML always works with files. IronPDF supports multiple input types.

**Solution:** Choose the appropriate method:
```csharp
// From file
pdf = renderer.RenderHtmlFileAsPdf("file.html");
// From string
pdf = renderer.RenderHtmlAsPdf("<html>...</html>");
// From URL
pdf = renderer.RenderUrlAsPdf("https://example.com");
```

### 4. Process Management Removed
No need to handle process lifecycle, exit codes, or standard output/error streams.

**Solution:** Use standard .NET exception handling:
```csharp
try
{
    var pdf = renderer.RenderHtmlAsPdf(html);
}
catch (Exception ex)
{
    // Handle rendering errors
}
```

### 5. Licensing
IronPDF requires a license key for production use.

**Solution:** Set the license key at application startup:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

## Additional Resources

- **Documentation:** https://ironpdf.com/docs/
- **Tutorials:** https://ironpdf.com/tutorials/