# How Do I Migrate from NReco.PdfGenerator to IronPDF in C#?

## Why Migrate?

NReco.PdfGenerator wraps the deprecated wkhtmltopdf binary, inheriting all its security vulnerabilities and rendering limitations. This guide provides a complete migration path to IronPDF's modern, secure Chromium-based engine.

### Critical Issues with NReco.PdfGenerator

1. **Security Vulnerabilities**: Inherits all wkhtmltopdf CVEs (20+ documented vulnerabilities)
   - CVE-2020-21365: Server-side request forgery (SSRF)
   - CVE-2022-35583: Local file read via HTML injection
   - CVE-2022-35580: Remote code execution potential
   - No patches available—wkhtmltopdf is abandoned since 2020

2. **Watermarked Free Version**: Production use requires paid license with opaque pricing

3. **Deprecated Rendering Engine**: WebKit Qt (circa 2012) with limited CSS3/JS support:
   - No CSS Grid or Flexbox
   - No modern JavaScript (ES6+)
   - Poor web font support
   - No CSS variables or custom properties

4. **External Binary Dependency**: Requires managing wkhtmltopdf binaries per platform

5. **No Active Development**: Wrapper maintenance without underlying engine updates

6. **Limited Async Support**: Synchronous API blocks threads in web applications

### Benefits of IronPDF

| Aspect | NReco.PdfGenerator | IronPDF |
|--------|-------------------|---------|
| Rendering Engine | WebKit Qt (2012) | Chromium (current) |
| Security | 20+ CVEs, no patches | Active security updates |
| CSS Support | CSS2.1, limited CSS3 | Full CSS3, Grid, Flexbox |
| JavaScript | Basic ES5 | Full ES6+, async/await |
| Dependencies | External wkhtmltopdf binary | Self-contained |
| Async Support | Synchronous only | Full async/await |
| Web Fonts | Limited | Full Google Fonts, @font-face |
| Licensing | Opaque pricing, contact sales | Transparent pricing |
| Free Trial | Watermarked | Full functionality |

---

## NuGet Package Changes

```bash
# Remove NReco.PdfGenerator
dotnet remove package NReco.PdfGenerator

# Install IronPDF
dotnet add package IronPdf
```

**Also remove wkhtmltopdf binaries** from your deployment:
- Delete `wkhtmltopdf.exe`, `wkhtmltox.dll` from project
- Remove any wkhtmltopdf installation scripts
- Delete platform-specific binary folders

---

## Namespace Changes

| NReco.PdfGenerator | IronPDF |
|-------------------|---------|
| `using NReco.PdfGenerator;` | `using IronPdf;` |
| | `using IronPdf.Rendering;` (for enums) |

---

## Complete API Mapping

### Core Classes

| NReco.PdfGenerator | IronPDF | Notes |
|-------------------|---------|-------|
| `HtmlToPdfConverter` | `ChromePdfRenderer` | Main renderer |
| `PageMargins` | Individual margin properties | MarginTop, MarginBottom, etc. |
| `PageOrientation` | `PdfPaperOrientation` | Enum |
| `PageSize` | `PdfPaperSize` | Enum |

### Rendering Methods

| NReco.PdfGenerator | IronPDF | Notes |
|-------------------|---------|-------|
| `GeneratePdf(html)` | `RenderHtmlAsPdf(html)` | Returns PdfDocument |
| `GeneratePdf(html, coverHtml)` | `RenderHtmlAsPdf()` + `Merge()` | Multi-step |
| `GeneratePdfFromFile(url, output)` | `RenderUrlAsPdf(url)` | Direct URL support |
| `GeneratePdfFromFile(htmlPath, output)` | `RenderHtmlFileAsPdf(path)` | File path |
| _(async not supported)_ | `RenderHtmlAsPdfAsync(html)` | Async version |
| _(async not supported)_ | `RenderUrlAsPdfAsync(url)` | Async version |

### Page Configuration

| NReco.PdfGenerator | IronPDF | Notes |
|-------------------|---------|-------|
| `PageWidth = 210` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Use enum or SetCustomPaperSize |
| `PageHeight = 297` | `RenderingOptions.SetCustomPaperSizeinMilimeters(w, h)` | Custom size |
| `Orientation = PageOrientation.Landscape` | `RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape` | Landscape |
| `Size = PageSize.A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Paper size enum |

### Margins

| NReco.PdfGenerator | IronPDF | Notes |
|-------------------|---------|-------|
| `Margins.Top = 10` | `RenderingOptions.MarginTop = 10` | In millimeters |
| `Margins.Bottom = 10` | `RenderingOptions.MarginBottom = 10` | In millimeters |
| `Margins.Left = 10` | `RenderingOptions.MarginLeft = 10` | In millimeters |
| `Margins.Right = 10` | `RenderingOptions.MarginRight = 10` | In millimeters |
| `new PageMargins { ... }` | Individual properties | No margins object |

### Rendering Options

| NReco.PdfGenerator | IronPDF | Notes |
|-------------------|---------|-------|
| `Zoom = 1.5f` | `RenderingOptions.Zoom = 150` | Percentage (100 = 100%) |
| `Quiet = true` | _(default behavior)_ | IronPDF is quiet by default |
| `LogReceived += handler` | `Logger.EnableDebugging = true` | Debug logging |
| `CustomWkHtmlArgs = "--disable-smart-shrinking"` | `RenderingOptions.FitToPaperMode` | Built-in options |
| `CustomWkHtmlPageArgs = "--no-background"` | `RenderingOptions.PrintHtmlBackgrounds = false` | Native property |

### Headers and Footers

| NReco.PdfGenerator | IronPDF | Notes |
|-------------------|---------|-------|
| `PageHeaderHtml = "<div>..."` | `RenderingOptions.HtmlHeader` | HtmlHeaderFooter object |
| `PageFooterHtml = "<div>..."` | `RenderingOptions.HtmlFooter` | HtmlHeaderFooter object |
| `HeaderSpacing = 10` | `HtmlHeader.MaxHeight = 25` | Height in mm |
| `FooterSpacing = 10` | `HtmlFooter.MaxHeight = 25` | Height in mm |

### Placeholder Variables

| NReco.PdfGenerator (wkhtmltopdf) | IronPDF | Notes |
|--------------------------------|---------|-------|
| `[page]` | `{page}` | Current page number |
| `[topage]` | `{total-pages}` | Total page count |
| `[date]` | `{date}` | Current date |
| `[time]` | `{time}` | Current time |
| `[title]` | `{html-title}` | Document title |
| `[webpage]` | `{url}` | Source URL |

### Output Handling

| NReco.PdfGenerator | IronPDF | Notes |
|-------------------|---------|-------|
| `byte[] pdfBytes = GeneratePdf(html)` | `PdfDocument pdf = RenderHtmlAsPdf(html)` | Returns object |
| `File.WriteAllBytes(path, bytes)` | `pdf.SaveAs(path)` | Direct save |
| `return pdfBytes` | `return pdf.BinaryData` | Get byte array |
| `new MemoryStream(pdfBytes)` | `pdf.Stream` | Get stream |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (NReco.PdfGenerator):**
```csharp
using NReco.PdfGenerator;
using System.IO;

public class PdfService
{
    public byte[] GeneratePdf(string html)
    {
        var converter = new HtmlToPdfConverter();
        return converter.GeneratePdf(html);
    }

    public void SavePdf(string html, string outputPath)
    {
        var pdfBytes = GeneratePdf(html);
        File.WriteAllBytes(outputPath, pdfBytes);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
        _renderer = new ChromePdfRenderer();
    }

    public byte[] GeneratePdf(string html)
    {
        return _renderer.RenderHtmlAsPdf(html).BinaryData;
    }

    public void SavePdf(string html, string outputPath)
    {
        var pdf = _renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs(outputPath);
    }
}
```

### Example 2: Page Configuration with Margins

**Before (NReco.PdfGenerator):**
```csharp
using NReco.PdfGenerator;

var converter = new HtmlToPdfConverter
{
    Orientation = PageOrientation.Landscape,
    Size = PageSize.A4,
    Margins = new PageMargins
    {
        Top = 20,
        Bottom = 20,
        Left = 15,
        Right = 15
    },
    Zoom = 0.9f
};

var pdfBytes = converter.GeneratePdf(html);
File.WriteAllBytes("report.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 15;
renderer.RenderingOptions.MarginRight = 15;
renderer.RenderingOptions.Zoom = 90; // Percentage

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 3: Headers and Footers with Page Numbers

**Before (NReco.PdfGenerator):**
```csharp
using NReco.PdfGenerator;

var converter = new HtmlToPdfConverter
{
    PageHeaderHtml = @"
        <div style='width:100%; text-align:center; font-size:10px;'>
            Company Report - Confidential
        </div>",
    PageFooterHtml = @"
        <div style='width:100%; text-align:center; font-size:10px;'>
            Page [page] of [topage]
        </div>",
    HeaderSpacing = 5,
    FooterSpacing = 5
};

var pdfBytes = converter.GeneratePdf(html);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width:100%; text-align:center; font-size:10px;'>
            Company Report - Confidential
        </div>",
    MaxHeight = 20
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width:100%; text-align:center; font-size:10px;'>
            Page {page} of {total-pages}
        </div>",
    MaxHeight = 20
};

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 4: URL to PDF Conversion

**Before (NReco.PdfGenerator):**
```csharp
using NReco.PdfGenerator;
using System.Net;

var converter = new HtmlToPdfConverter
{
    CustomWkHtmlArgs = "--javascript-delay 2000"
};

// NReco doesn't have direct URL support, must download first
using (var client = new WebClient())
{
    var html = client.DownloadString("https://example.com/report");
    var pdfBytes = converter.GeneratePdf(html);
    File.WriteAllBytes("webpage.pdf", pdfBytes);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.WaitFor.RenderDelay(2000); // Wait for JS

var pdf = renderer.RenderUrlAsPdf("https://example.com/report");
pdf.SaveAs("webpage.pdf");
```

### Example 5: Async Rendering (New in IronPDF)

**Before (NReco.PdfGenerator):**
```csharp
// NReco doesn't support async - blocks the thread
public ActionResult GenerateReport()
{
    var converter = new HtmlToPdfConverter();
    var html = BuildReportHtml();
    var pdfBytes = converter.GeneratePdf(html); // Blocking!

    return File(pdfBytes, "application/pdf", "report.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public async Task<IActionResult> GenerateReport()
{
    var renderer = new ChromePdfRenderer();
    var html = await BuildReportHtmlAsync();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html); // Non-blocking!

    return File(pdf.BinaryData, "application/pdf", "report.pdf");
}
```

### Example 6: Custom wkhtmltopdf Arguments Migration

**Before (NReco.PdfGenerator):**
```csharp
var converter = new HtmlToPdfConverter
{
    CustomWkHtmlArgs = "--disable-smart-shrinking --no-stop-slow-scripts",
    CustomWkHtmlPageArgs = "--no-background --disable-external-links"
};
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();

// --disable-smart-shrinking equivalent
renderer.RenderingOptions.FitToPaperMode = FitToPaperModes.Zoom;

// --no-stop-slow-scripts equivalent
renderer.RenderingOptions.Timeout = 120; // seconds

// --no-background equivalent
renderer.RenderingOptions.PrintHtmlBackgrounds = false;

// --disable-external-links handled automatically
// IronPDF maintains links but renders them properly
```

### Example 7: Error Handling

**Before (NReco.PdfGenerator):**
```csharp
using NReco.PdfGenerator;

var converter = new HtmlToPdfConverter();
converter.LogReceived += (sender, args) =>
    Console.WriteLine($"wkhtmltopdf: {args.Data}");

try
{
    var pdfBytes = converter.GeneratePdf(html);
}
catch (Exception ex)
{
    // Generic exception with wkhtmltopdf stderr
    Console.WriteLine($"Error: {ex.Message}");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

try
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
catch (IronPdf.Exceptions.IronPdfProductException ex)
{
    // Specific exception with detailed message
    Console.WriteLine($"PDF Error: {ex.Message}");
}
catch (IronPdf.Exceptions.IronPdfLicensingException ex)
{
    Console.WriteLine($"License Error: {ex.Message}");
}
```

### Example 8: Custom Paper Size

**Before (NReco.PdfGenerator):**
```csharp
var converter = new HtmlToPdfConverter
{
    PageWidth = 100,  // mm
    PageHeight = 150  // mm
};
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.SetCustomPaperSizeinMilimeters(100, 150);

// Or use SetCustomPaperSizeInInches, SetCustomPaperSizeInPixelsOrPoints
```

### Example 9: JavaScript Execution Wait

**Before (NReco.PdfGenerator):**
```csharp
var converter = new HtmlToPdfConverter
{
    CustomWkHtmlArgs = "--javascript-delay 5000 --run-script 'window.status=\"ready\"' --window-status ready"
};
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();

// Simple delay
renderer.RenderingOptions.WaitFor.RenderDelay(5000);

// Or wait for JavaScript condition
renderer.RenderingOptions.WaitFor.JavaScript("window.dataLoaded === true", 30000);

// Or wait for specific HTML element
renderer.RenderingOptions.WaitFor.HtmlElementById("chart-container", 10000);
```

### Example 10: Print Media Emulation

**Before (NReco.PdfGenerator):**
```csharp
var converter = new HtmlToPdfConverter
{
    CustomWkHtmlArgs = "--print-media-type"  // Or "--no-print-media-type"
};
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
// Or PdfCssMediaType.Screen for screen styles
```

---

## Security Migration: Removing wkhtmltopdf Vulnerabilities

### Delete wkhtmltopdf Artifacts

```bash
# Remove wkhtmltopdf binaries
rm -rf wkhtmltopdf/
rm -f wkhtmltopdf.exe
rm -f wkhtmltox.dll
rm -f libwkhtmltox.so
rm -f libwkhtmltox.dylib

# Remove NReco extraction paths
rm -rf App_Data/wkhtmltopdf/

# Remove any installation scripts
rm -f install-wkhtmltopdf.sh
rm -f install-wkhtmltopdf.ps1
```

### Update Docker Files

**Before:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y wkhtmltopdf
COPY . /app
```

**After:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# No wkhtmltopdf installation needed!
# IronPDF includes its own Chromium binaries
COPY . /app
```

### Remove Binary Management Code

**Delete code like:**
```csharp
// DELETE: NReco binary configuration
NReco.PdfGenerator.HtmlToPdfConverter.WkHtmlToPdfExeName = "wkhtmltopdf.exe";
NReco.PdfGenerator.HtmlToPdfConverter.WkHtmlToPdfPath = @"C:\Tools\wkhtmltopdf\bin";

// DELETE: Binary extraction
ExtractWkHtmlToPdfBinary();
EnsureWkHtmlToPdfInstalled();
```

---

## Common Migration Issues

### Issue 1: Zoom Value Differences

**Problem:** NReco uses float (0.0-2.0), IronPDF uses percentage (0-200)

```csharp
// NReco: Zoom = 0.75f means 75%
// IronPDF: Zoom = 75 means 75%

// Migration formula:
int ironPdfZoom = (int)(nrecoZoom * 100);
```

### Issue 2: Return Type Change

**Problem:** NReco returns `byte[]`, IronPDF returns `PdfDocument`

```csharp
// If your interface expects byte[]:
public interface IPdfGenerator
{
    byte[] Generate(string html);
}

// IronPDF implementation:
public byte[] Generate(string html)
{
    return _renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Issue 3: Placeholder Syntax

**Problem:** Different placeholder format

```csharp
// Find and replace in your HTML templates:
// [page] → {page}
// [topage] → {total-pages}
// [date] → {date}
// [time] → {time}
// [title] → {html-title}

string migratedHtml = html
    .Replace("[page]", "{page}")
    .Replace("[topage]", "{total-pages}")
    .Replace("[date]", "{date}")
    .Replace("[time]", "{time}")
    .Replace("[title]", "{html-title}");
```

### Issue 4: Custom wkhtmltopdf Arguments

**Problem:** No direct equivalent for custom command-line args

```csharp
// Map common arguments to IronPDF properties:
// --dpi → RenderingOptions.PrintDpi
// --image-quality → (handled automatically, better quality)
// --enable-local-file-access → (default enabled)
// --disable-javascript → RenderingOptions.EnableJavaScript = false
// --javascript-delay → RenderingOptions.WaitFor.RenderDelay()
```

### Issue 5: Margins Object vs Individual Properties

**Problem:** NReco uses `PageMargins` object, IronPDF uses individual properties

```csharp
// NReco:
converter.Margins = new PageMargins { Top = 10, Bottom = 10, Left = 5, Right = 5 };

// IronPDF:
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.MarginLeft = 5;
renderer.RenderingOptions.MarginRight = 5;
```

### Issue 6: Event Handler Migration

**Problem:** NReco's `LogReceived` event vs IronPDF logging

```csharp
// NReco:
converter.LogReceived += (s, e) => Debug.WriteLine(e.Data);

// IronPDF:
IronPdf.Logging.Logger.EnableDebugging = true;
IronPdf.Logging.Logger.LogFilePath = "ironpdf.log";
IronPdf.Logging.Logger.LoggingMode = IronPdf.Logging.Logger.LoggingModes.All;
```

### Issue 7: Thread Safety

**Problem:** NReco creates new converter per call, IronPDF can reuse renderer

```csharp
// NReco pattern (creates new each time):
public byte[] Generate(string html)
{
    var converter = new HtmlToPdfConverter();
    return converter.GeneratePdf(html);
}

// IronPDF pattern (reuse renderer, thread-safe):
private readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

public byte[] Generate(string html)
{
    return _renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Issue 8: Base URL for Relative Resources

**Problem:** NReco uses file:// URLs, IronPDF has BaseUrlOrPath

```csharp
// NReco (save temp file with resources):
File.WriteAllText("temp/index.html", html);
converter.GeneratePdfFromFile("temp/index.html", "output.pdf");

// IronPDF (set base path directly):
renderer.RenderingOptions.BaseUrl = new Uri("file:///C:/templates/");
var pdf = renderer.RenderHtmlAsPdf(html);
```

---

## Performance Comparison

| Operation | NReco.PdfGenerator | IronPDF |
|-----------|-------------------|---------|
| Simple HTML (1 page) | ~800ms | ~400ms |
| Complex HTML (10 pages) | ~3500ms | ~1200ms |
| URL Rendering | ~2000ms + download | ~800ms |
| With JavaScript | ~3000ms | ~600ms |
| Memory Usage | Higher (process spawn) | Lower (in-process) |
| First Render | ~2000ms (binary init) | ~1500ms (Chromium init) |
| Subsequent Renders | ~500ms | ~200ms |

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all `NReco.PdfGenerator` usages**
  ```bash
  grep -r "NReco.PdfGenerator\|HtmlToPdfConverter" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document all `CustomWkHtmlArgs` and `CustomWkHtmlPageArgs` values**
  ```csharp
  // Before (.)
  var converter = new HtmlToPdfConverter();
  converter.CustomWkHtmlArgs = "--zoom 1.5";

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.Zoom = 150;
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **List all header/footer HTML templates with placeholders**
  ```csharp
  // Before (.)
  converter.PageHeaderHtml = "<div>Page [page] of [toPage]</div>";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders: {page}, {total-pages}, {date}, {time}, {html-title}.

- [ ] **Identify async requirements (web controllers, services)**
  **Why:** IronPDF supports async operations, which can improve performance in web applications.

- [ ] **Review zoom and margin settings**
  ```csharp
  // Before (.)
  converter.Zoom = 1.5;
  converter.PageMargins = new PageMargins() { Top = 10, Bottom = 10 };

  // After (IronPDF)
  renderer.RenderingOptions.Zoom = 150;
  renderer.RenderingOptions.MarginTop = 10;
  renderer.RenderingOptions.MarginBottom = 10;
  ```
  **Why:** Ensure these settings are accurately translated to IronPDF's RenderingOptions for consistent document layout.

- [ ] **Backup existing PDF outputs for comparison**
  **Why:** Ensure you have a reference to verify the output quality and layout post-migration.

### Migration Steps

- [ ] **Install IronPDF NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add the new library to your project to begin the migration.

- [ ] **Remove NReco.PdfGenerator NuGet package**
  ```bash
  dotnet remove package NReco.PdfGenerator
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Add IronPDF license key configuration**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Update namespace imports**
  ```csharp
  // Before (.)
  using NReco.PdfGenerator;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Update references to use IronPDF's classes and methods.

- [ ] **Replace `HtmlToPdfConverter` with `ChromePdfRenderer`**
  ```csharp
  // Before (.)
  var converter = new HtmlToPdfConverter();
  converter.GeneratePdf(htmlContent, null, "output.pdf");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(htmlContent);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering for accurate HTML/CSS support.

- [ ] **Convert `PageMargins` to individual margin properties**
  ```csharp
  // Before (.)
  converter.PageMargins = new PageMargins() { Top = 10, Bottom = 10 };

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 10;
  renderer.RenderingOptions.MarginBottom = 10;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Update zoom values (float to percentage)**
  ```csharp
  // Before (.)
  converter.Zoom = 1.5;

  // After (IronPDF)
  renderer.RenderingOptions.Zoom = 150;
  ```
  **Why:** IronPDF uses percentage values for zoom, providing more intuitive scaling.

- [ ] **Update placeholder syntax in templates**
  ```csharp
  // Before (.)
  converter.PageHeaderHtml = "<div>Page [page] of [toPage]</div>";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** Ensure placeholders are correctly replaced to maintain document integrity.

- [ ] **Convert sync calls to async where beneficial**
  ```csharp
  // Before (.)
  converter.GeneratePdf(htmlContent, null, "output.pdf");

  // After (IronPDF)
  var pdf = await renderer.RenderHtmlAsPdfAsync(htmlContent);
  await pdf.SaveAsAsync("output.pdf");
  ```
  **Why:** Asynchronous operations can improve performance in web applications by freeing up threads.

- [ ] **Remove wkhtmltopdf binary management code**
  **Why:** IronPDF is self-contained and does not require external binaries, simplifying deployment.

### Post-Migration

- [ ] **Remove wkhtmltopdf binaries from project/deployment**
  **Why:** Clean up unnecessary files, reducing project size and potential security risks.

- [ ] **Update Docker files to remove wkhtmltopdf installation**
  **Why:** Simplify Docker images by removing unnecessary dependencies.

- [ ] **Run regression tests comparing PDF output**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Verify header/footer placeholders render correctly**
  **Why:** Ensure document headers and footers are accurate and formatted as expected.

- [ ] **Test on all target platforms (Windows, Linux, macOS)**
  **Why:** Ensure consistent behavior and output across all environments.

- [ ] **Update CI/CD pipeline to remove wkhtmltopdf steps**
  **Why:** Streamline the build process by removing obsolete steps.

- [ ] **Update security scanning to confirm CVE removal**
  **Why:** Ensure that migrating to IronPDF has mitigated previous security vulnerabilities.
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [HTML to PDF Tutorial](https://ironpdf.com/tutorials/html-to-pdf/)
- [Migrating from wkhtmltopdf](https://ironpdf.com/blog/compare-to-other-components/migrating-from-wkhtmltopdf/)
- [wkhtmltopdf CVE List](https://nvd.nist.gov/vuln/search/results?query=wkhtmltopdf)
