# How Do I Migrate from PrinceXML to IronPDF in C#?

## Why Migrate from PrinceXML?

### The External Process Problem

PrinceXML operates as a **separate command-line executable** that creates significant architectural challenges for .NET applications:

1. **Process Management Overhead**: Must spawn, monitor, and terminate external processes
2. **No Native .NET Integration**: Communicate via stdin/stdout or temporary files
3. **Deployment Complexity**: Requires Prince installation on every server
4. **Licensing Per Server**: Each deployment needs a separate license
5. **Error Handling Difficulty**: Parse text output for error detection
6. **No Async/Await**: Blocking calls or complex async wrappers required
7. **Path Dependencies**: Must locate Prince executable on PATH or absolute path

### CSS Paged Media Limitations

While PrinceXML's CSS Paged Media support is powerful, it creates vendor lock-in:

```css
/* Prince-specific CSS that won't work elsewhere */
@page {
    size: A4;
    margin: 2cm;
    @top-center {
        content: "Document Title";
    }
    @bottom-right {
        content: counter(page);
    }
}

/* Prince-specific extensions */
prince-pdf-page-label: "Chapter " counter(chapter);
prince-pdf-destination: attr(id);
```

### Quick Migration Comparison

| Aspect | PrinceXML | IronPDF |
|--------|-----------|---------|
| Architecture | External Process | Native .NET Library |
| Integration | Command-line | Direct API |
| Deployment | Install on every server | Single NuGet package |
| Error Handling | Parse text output | .NET exceptions |
| Async Support | Manual wrappers | Native async/await |
| PDF Manipulation | Generation only | Full manipulation |
| Licensing | Per server | Per developer |
| Updates | Manual reinstall | NuGet update |
| Debugging | Difficult | Full debugger support |

---

## Quick Start: PrinceXML to IronPDF in 5 Minutes

### Step 1: Install IronPDF

```bash
# Install IronPDF
dotnet add package IronPdf

# Remove Prince wrapper if using one
dotnet remove package PrinceXMLWrapper
```

### Step 2: Replace Process Code

**PrinceXML:**
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
    throw new Exception(process.StandardError.ReadToEnd());
}
```

**IronPDF:**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Step 3: Migrate CSS Paged Media

**PrinceXML CSS:**
```css
@page {
    size: A4;
    margin: 2cm;
}
```

**IronPDF C# (equivalent):**
```csharp
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 56;    // ~2cm
renderer.RenderingOptions.MarginBottom = 56;
renderer.RenderingOptions.MarginLeft = 56;
renderer.RenderingOptions.MarginRight = 56;
```

Process elimination patterns and external dependency removal strategies are documented in the [complete migration documentation](https://ironpdf.com/blog/migration-guides/migrate-from-princexml-to-ironpdf/).

---

## Complete API Reference

### Command-Line to Method Mapping

| Prince Command | IronPDF Equivalent |
|---------------|-------------------|
| `prince input.html -o output.pdf` | `renderer.RenderHtmlFileAsPdf("input.html").SaveAs("output.pdf")` |
| `prince --style=custom.css input.html` | Include CSS in HTML or use `RenderingOptions` |
| `prince --javascript` | `renderer.RenderingOptions.EnableJavaScript = true` |
| `prince --no-javascript` | `renderer.RenderingOptions.EnableJavaScript = false` |
| `prince --page-size=Letter` | `renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter` |
| `prince --page-margin=1in` | `renderer.RenderingOptions.MarginTop = 72` (72 points = 1 inch) |
| `prince --pdf-lang=en` | `renderer.RenderingOptions.PdfTitle = "..."; // metadata` |
| `prince --encrypt` | `pdf.SecuritySettings.OwnerPassword = "..."` |
| `prince --user-password=pw` | `pdf.SecuritySettings.UserPassword = "pw"` |
| `prince --disallow-print` | `pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint` |
| `prince --disallow-copy` | `pdf.SecuritySettings.AllowUserCopyPasteContent = false` |
| `prince --baseurl=http://...` | `renderer.RenderingOptions.BaseUrl = new Uri("http://...")` |
| `prince --media=print` | `renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print` |
| `prince --media=screen` | `renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Screen` |

### CSS @page to RenderingOptions Mapping

| CSS @page Property | IronPDF Equivalent |
|-------------------|-------------------|
| `size: A4` | `PaperSize = PdfPaperSize.A4` |
| `size: Letter` | `PaperSize = PdfPaperSize.Letter` |
| `size: A4 landscape` | `PaperSize = PdfPaperSize.A4` + `PaperOrientation = Landscape` |
| `margin: 2cm` | `MarginTop/Bottom/Left/Right = 56` |
| `margin-top: 1in` | `MarginTop = 72` |
| `@top-center { content: "..." }` | `HtmlHeader` with centered div |
| `@bottom-right { content: counter(page) }` | `HtmlFooter` with `{page}` placeholder |

### Page Size Conversions

| Size | Points | Millimeters |
|------|--------|-------------|
| Letter | 612 x 792 | 216 x 279 |
| A4 | 595 x 842 | 210 x 297 |
| Legal | 612 x 1008 | 216 x 356 |
| A3 | 842 x 1191 | 297 x 420 |
| 1 inch | 72 | 25.4 |
| 1 cm | 28.35 | 10 |

---

## Code Examples

### Example 1: Basic HTML File to PDF

**PrinceXML:**
```csharp
using System.Diagnostics;
using System.IO;

public class PrinceConverter
{
    private readonly string _princePath;

    public PrinceConverter(string princePath = "prince")
    {
        _princePath = princePath;
    }

    public void ConvertHtmlToPdf(string htmlPath, string outputPath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _princePath,
                Arguments = $"\"{htmlPath}\" -o \"{outputPath}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Prince conversion failed: {error}");
        }
    }
}

// Usage
var converter = new PrinceConverter(@"C:\Program Files\Prince\engine\bin\prince.exe");
converter.ConvertHtmlToPdf("report.html", "report.pdf");
```

**IronPDF:**
```csharp
using IronPdf;

public class PdfConverter
{
    public PdfConverter()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    }

    public void ConvertHtmlToPdf(string htmlPath, string outputPath)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlFileAsPdf(htmlPath);
        pdf.SaveAs(outputPath);
    }
}

// Usage
var converter = new PdfConverter();
converter.ConvertHtmlToPdf("report.html", "report.pdf");
```

### Example 2: HTML String to PDF

**PrinceXML:**
```csharp
using System.Diagnostics;
using System.IO;

public byte[] ConvertHtmlStringToPdf(string htmlContent)
{
    // Prince requires file input - must create temp file
    string tempHtmlPath = Path.GetTempFileName() + ".html";
    string tempPdfPath = Path.GetTempFileName() + ".pdf";

    try
    {
        // Write HTML to temp file
        File.WriteAllText(tempHtmlPath, htmlContent);

        // Run Prince
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "prince",
                Arguments = $"\"{tempHtmlPath}\" -o \"{tempPdfPath}\"",
                UseShellExecute = false,
                RedirectStandardError = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception(process.StandardError.ReadToEnd());
        }

        // Read PDF bytes
        return File.ReadAllBytes(tempPdfPath);
    }
    finally
    {
        // Clean up temp files
        if (File.Exists(tempHtmlPath)) File.Delete(tempHtmlPath);
        if (File.Exists(tempPdfPath)) File.Delete(tempPdfPath);
    }
}
```

**IronPDF:**
```csharp
using IronPdf;

public byte[] ConvertHtmlStringToPdf(string htmlContent)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(htmlContent);
    return pdf.BinaryData;
}

// Or save directly
public void ConvertHtmlStringToFile(string htmlContent, string outputPath)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(htmlContent);
    pdf.SaveAs(outputPath);
}
```

### Example 3: URL to PDF with JavaScript

**PrinceXML:**
```csharp
using System.Diagnostics;

public void ConvertUrlToPdf(string url, string outputPath)
{
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "prince",
            Arguments = $"--javascript \"{url}\" -o \"{outputPath}\"",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        }
    };

    process.Start();

    // Prince has limited JavaScript support
    // May timeout on complex SPAs
    process.WaitForExit(30000); // 30 second timeout

    if (!process.HasExited)
    {
        process.Kill();
        throw new TimeoutException("Prince conversion timed out");
    }

    if (process.ExitCode != 0)
    {
        throw new Exception(process.StandardError.ReadToEnd());
    }
}
```

**IronPDF:**
```csharp
using IronPdf;

public void ConvertUrlToPdf(string url, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    // Full JavaScript support (ES2024)
    renderer.RenderingOptions.EnableJavaScript = true;

    // Wait for JavaScript to complete
    renderer.RenderingOptions.WaitFor.JavaScript(5000);

    // Or wait for specific element
    renderer.RenderingOptions.WaitFor.HtmlElementById("content-loaded", 10000);

    // Or wait for network idle
    renderer.RenderingOptions.WaitFor.NetworkIdle0(5000);

    var pdf = renderer.RenderUrlAsPdf(url);
    pdf.SaveAs(outputPath);
}

// Async version
public async Task ConvertUrlToPdfAsync(string url, string outputPath)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.EnableJavaScript = true;
    renderer.RenderingOptions.WaitFor.JavaScript(5000);

    var pdf = await renderer.RenderUrlAsPdfAsync(url);
    pdf.SaveAs(outputPath);
}
```

### Example 4: CSS Paged Media Headers/Footers Migration

**PrinceXML CSS:**
```css
@page {
    size: A4;
    margin: 2cm 2cm 3cm 2cm;

    @top-left {
        content: "Company Name";
        font-size: 10pt;
        color: #666;
    }

    @top-right {
        content: string(chapter-title);
        font-size: 10pt;
        font-style: italic;
    }

    @bottom-center {
        content: "Page " counter(page) " of " counter(pages);
        font-size: 9pt;
    }

    @bottom-right {
        content: "Generated: " prince-script(today);
        font-size: 8pt;
        color: #999;
    }
}

h1 { string-set: chapter-title content(); }
```

**IronPDF C#:**
```csharp
using IronPdf;

public PdfDocument ConvertWithHeadersFooters(string html)
{
    var renderer = new ChromePdfRenderer();

    // Page settings
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.MarginTop = 80;     // Extra space for header
    renderer.RenderingOptions.MarginBottom = 100; // Extra space for footer
    renderer.RenderingOptions.MarginLeft = 56;
    renderer.RenderingOptions.MarginRight = 56;

    // Header with full HTML/CSS support
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = @"
            <div style='width: 100%; font-size: 10pt; display: flex; justify-content: space-between;'>
                <span style='color: #666;'>Company Name</span>
                <span style='font-style: italic;'>{html-title}</span>
            </div>",
        MaxHeight = 30
    };

    // Footer with page numbers and date
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = @"
            <div style='width: 100%; font-size: 9pt; display: flex; justify-content: space-between;'>
                <span></span>
                <span>Page {page} of {total-pages}</span>
                <span style='font-size: 8pt; color: #999;'>Generated: {date}</span>
            </div>",
        MaxHeight = 40
    };

    return renderer.RenderHtmlAsPdf(html);
}
```

### Example 5: Page Size and Orientation

**PrinceXML:**
```csharp
var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "prince",
        Arguments = "--page-size=Letter --page-margin=\"0.5in 1in\" " +
                   "--pdf-page-layout=two-column-left input.html -o output.pdf",
        UseShellExecute = false
    }
};
process.Start();
process.WaitForExit();
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Page size
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

// Margins (in points: 72 points = 1 inch)
renderer.RenderingOptions.MarginTop = 36;     // 0.5 inch
renderer.RenderingOptions.MarginBottom = 36;
renderer.RenderingOptions.MarginLeft = 72;    // 1 inch
renderer.RenderingOptions.MarginRight = 72;

// Custom page size
renderer.RenderingOptions.SetCustomPaperSizeInInches(8.5, 14); // Legal size

var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 6: Password Protection and Encryption

**PrinceXML:**
```csharp
var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "prince",
        Arguments = "--encrypt " +
                   "--user-password=user123 " +
                   "--owner-password=owner456 " +
                   "--disallow-print " +
                   "--disallow-copy " +
                   "--disallow-modify " +
                   "input.html -o protected.pdf",
        UseShellExecute = false
    }
};
process.Start();
process.WaitForExit();
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlFileAsPdf("input.html");

// Set passwords
pdf.SecuritySettings.OwnerPassword = "owner456";
pdf.SecuritySettings.UserPassword = "user123";

// Set permissions
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserAnnotations = false;

pdf.SaveAs("protected.pdf");
```

### Example 7: PDF Metadata

**PrinceXML:**
```csharp
var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "prince",
        Arguments = "--pdf-title=\"My Document\" " +
                   "--pdf-author=\"John Doe\" " +
                   "--pdf-subject=\"Annual Report\" " +
                   "--pdf-keywords=\"report, annual, 2024\" " +
                   "--pdf-creator=\"My Application\" " +
                   "input.html -o output.pdf",
        UseShellExecute = false
    }
};
process.Start();
process.WaitForExit();
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Set metadata in rendering options
renderer.RenderingOptions.Title = "My Document";

var pdf = renderer.RenderHtmlFileAsPdf("input.html");

// Or set metadata on the document
pdf.MetaData.Title = "My Document";
pdf.MetaData.Author = "John Doe";
pdf.MetaData.Subject = "Annual Report";
pdf.MetaData.Keywords = "report, annual, 2024";
pdf.MetaData.Creator = "My Application";
pdf.MetaData.Producer = "IronPDF";

pdf.SaveAs("output.pdf");
```

### Example 8: Merge Multiple PDFs

**PrinceXML:**
```csharp
// Prince cannot merge PDFs - must use another tool
// Typically requires a separate library like PDFSharp or iTextSharp

using System.Diagnostics;
using System.IO;

public void MergePdfsWithPrince(string[] htmlFiles, string outputPath)
{
    // Convert each HTML to PDF
    var tempPdfs = new List<string>();

    foreach (var html in htmlFiles)
    {
        string tempPdf = Path.GetTempFileName() + ".pdf";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "prince",
                Arguments = $"\"{html}\" -o \"{tempPdf}\"",
                UseShellExecute = false
            }
        };
        process.Start();
        process.WaitForExit();
        tempPdfs.Add(tempPdf);
    }

    // Would need another library to merge the PDFs
    // Prince doesn't support merging
    throw new NotSupportedException("Prince cannot merge PDFs");
}
```

**IronPDF:**
```csharp
using IronPdf;
using System.Linq;

public void MergePdfs(string[] htmlFiles, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    // Convert all HTML files to PDFs
    var pdfs = htmlFiles.Select(html => renderer.RenderHtmlFileAsPdf(html)).ToList();

    // Merge all PDFs
    var merged = PdfDocument.Merge(pdfs);

    merged.SaveAs(outputPath);
}

// Or merge existing PDFs
public void MergeExistingPdfs(string[] pdfPaths, string outputPath)
{
    var pdfs = pdfPaths.Select(path => PdfDocument.FromFile(path)).ToList();
    var merged = PdfDocument.Merge(pdfs);
    merged.SaveAs(outputPath);
}
```

### Example 9: Watermarks

**PrinceXML:**
```css
/* CSS-based watermark approach */
@page {
    @prince-overlay {
        content: "DRAFT";
        font-size: 72pt;
        color: rgba(255, 0, 0, 0.2);
        transform: rotate(-45deg);
    }
}
```

**IronPDF:**
```csharp
using IronPdf;

public void AddWatermark(string inputPath, string outputPath)
{
    var pdf = PdfDocument.FromFile(inputPath);

    // HTML-based watermark with full CSS support
    pdf.ApplyWatermark(@"
        <div style='
            font-size: 72pt;
            color: rgba(255, 0, 0, 0.2);
            transform: rotate(-45deg);
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%) rotate(-45deg);
        '>
            DRAFT
        </div>");

    pdf.SaveAs(outputPath);
}

// Or apply during rendering
public PdfDocument CreateWithWatermark(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    pdf.ApplyWatermark("<div style='font-size: 48pt; color: rgba(0,0,0,0.1);'>CONFIDENTIAL</div>");

    return pdf;
}
```

### Example 10: Async Conversion

**PrinceXML:**
```csharp
// Prince is synchronous - must wrap in Task
public async Task<byte[]> ConvertAsync(string html)
{
    return await Task.Run(() =>
    {
        string tempHtml = Path.GetTempFileName() + ".html";
        string tempPdf = Path.GetTempFileName() + ".pdf";

        try
        {
            File.WriteAllText(tempHtml, html);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "prince",
                    Arguments = $"\"{tempHtml}\" -o \"{tempPdf}\"",
                    UseShellExecute = false
                }
            };

            process.Start();
            process.WaitForExit();

            return File.ReadAllBytes(tempPdf);
        }
        finally
        {
            if (File.Exists(tempHtml)) File.Delete(tempHtml);
            if (File.Exists(tempPdf)) File.Delete(tempPdf);
        }
    });
}
```

**IronPDF:**
```csharp
using IronPdf;

// Native async support
public async Task<byte[]> ConvertAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}

// Async URL conversion
public async Task<PdfDocument> ConvertUrlAsync(string url)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.EnableJavaScript = true;
    renderer.RenderingOptions.WaitFor.JavaScript(5000);

    return await renderer.RenderUrlAsPdfAsync(url);
}

// Parallel conversions
public async Task<List<PdfDocument>> ConvertManyAsync(List<string> htmlList)
{
    var renderer = new ChromePdfRenderer();

    var tasks = htmlList.Select(html =>
        renderer.RenderHtmlAsPdfAsync(html));

    var results = await Task.WhenAll(tasks);
    return results.ToList();
}
```

### Example 11: Digital Signatures

**PrinceXML:**
```csharp
// Prince cannot add digital signatures
// Must use another tool after PDF generation
throw new NotSupportedException("Prince does not support digital signatures");
```

**IronPDF:**
```csharp
using IronPdf;
using IronPdf.Signing;

public void SignPdf(string inputPath, string certPath, string certPassword, string outputPath)
{
    var pdf = PdfDocument.FromFile(inputPath);

    var signature = new PdfSignature(certPath, certPassword)
    {
        SigningReason = "Document Approval",
        SigningLocation = "New York",
        SigningContact = "approver@example.com"
    };

    pdf.Sign(signature);
    pdf.SaveAs(outputPath);
}
```

### Example 12: PDF/A Compliance

**PrinceXML:**
```csharp
var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "prince",
        Arguments = "--pdf-profile=PDF/A-1b input.html -o output.pdf",
        UseShellExecute = false
    }
};
process.Start();
process.WaitForExit();
```

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlFileAsPdf("input.html");

// Save as PDF/A-3b for archiving
pdf.SaveAsPdfA("output.pdf", PdfAVersions.PdfA3b);
```

---

## CSS Paged Media Migration Patterns

### Pattern 1: @page Size and Margins

**Prince CSS:**
```css
@page {
    size: A4 portrait;
    margin: 2cm 1.5cm;
}
```

**IronPDF:**
```csharp
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
renderer.RenderingOptions.MarginTop = 56;     // 2cm
renderer.RenderingOptions.MarginBottom = 56;
renderer.RenderingOptions.MarginLeft = 42;    // 1.5cm
renderer.RenderingOptions.MarginRight = 42;
```

### Pattern 2: Named Pages

**Prince CSS:**
```css
@page cover {
    margin: 0;
}
@page content {
    margin: 2cm;
}

.cover { page: cover; }
.content { page: content; }
```

**IronPDF:**
```csharp
// Render sections separately, then merge
var coverRenderer = new ChromePdfRenderer();
coverRenderer.RenderingOptions.MarginTop = 0;
coverRenderer.RenderingOptions.MarginBottom = 0;
coverRenderer.RenderingOptions.MarginLeft = 0;
coverRenderer.RenderingOptions.MarginRight = 0;

var contentRenderer = new ChromePdfRenderer();
contentRenderer.RenderingOptions.MarginTop = 56;
contentRenderer.RenderingOptions.MarginBottom = 56;
contentRenderer.RenderingOptions.MarginLeft = 56;
contentRenderer.RenderingOptions.MarginRight = 56;

var cover = coverRenderer.RenderHtmlAsPdf(coverHtml);
var content = contentRenderer.RenderHtmlAsPdf(contentHtml);

var merged = PdfDocument.Merge(cover, content);
merged.SaveAs("document.pdf");
```

### Pattern 3: Page Counters

**Prince CSS:**
```css
@page {
    @bottom-center {
        content: "Page " counter(page) " of " counter(pages);
    }
}
```

**IronPDF:**
```csharp
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='text-align: center; font-size: 10pt;'>
            Page {page} of {total-pages}
        </div>",
    MaxHeight = 25
};
```

### Pattern 4: Running Headers

**Prince CSS:**
```css
h1 { string-set: chapter-title content(); }

@page {
    @top-right {
        content: string(chapter-title);
    }
}
```

**IronPDF:**
```csharp
// Use {html-title} from <title> tag
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='text-align: right; font-style: italic;'>
            {html-title}
        </div>",
    MaxHeight = 25
};

// Or use URL for context
// {url} placeholder shows current URL
```

---

## Feature Comparison Table

| Feature | PrinceXML | IronPDF |
|---------|-----------|---------|
| **Architecture** | | |
| Native .NET | No | Yes |
| External Process | Required | No |
| Async Support | Manual wrapping | Native async/await |
| In-Process | No | Yes |
| **Rendering** | | |
| CSS Paged Media | Full support | Via RenderingOptions |
| CSS Grid | Yes | Yes |
| Flexbox | Yes | Yes |
| JavaScript | Limited | Full ES2024 |
| SVG | Yes | Yes |
| Web Fonts | Yes | Yes |
| **PDF Features** | | |
| Generation | Yes | Yes |
| Merge | No | Yes |
| Split | No | Yes |
| Edit | No | Yes |
| Watermarks | CSS only | HTML/CSS + API |
| Digital Signatures | No | Yes |
| PDF/A | Yes | Yes |
| Encryption | Yes | Yes |
| Forms | No | Yes |
| **Deployment** | | |
| NuGet Package | No | Yes |
| Server Install | Required | No |
| Docker Support | Complex | Simple |
| Cloud Functions | Difficult | Easy |
| **Licensing** | | |
| Model | Per server | Per developer |
| Pricing | $495+ | Competitive |

---

## Common Migration Issues

### Issue 1: CSS @page Not Working

**Symptom:** Prince's @page CSS rules don't apply in IronPDF.

**Cause:** IronPDF uses Chromium, which has limited @page support.

**Solution:** Convert CSS rules to RenderingOptions:
```csharp
// Instead of @page { size: A4; margin: 2cm; }
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 56;
renderer.RenderingOptions.MarginBottom = 56;
renderer.RenderingOptions.MarginLeft = 56;
renderer.RenderingOptions.MarginRight = 56;
```

### Issue 2: Page Margin Boxes Missing

**Symptom:** @top-center, @bottom-right margin box content missing.

**Cause:** CSS margin boxes are Prince-specific.

**Solution:** Use HtmlHeader/HtmlFooter:
```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div>Your header content</div>",
    MaxHeight = 40
};
```

### Issue 3: string-set/content Not Working

**Symptom:** Running headers from `string-set` CSS property don't work.

**Cause:** `string-set` is Prince-specific.

**Solution:** Use `{html-title}` placeholder from `<title>` tag:
```html
<title>Chapter 1: Introduction</title>
```
```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div>{html-title}</div>"
};
```

### Issue 4: counter(pages) Incorrect

**Symptom:** Total page count incorrect during rendering.

**Cause:** Different counter implementations.

**Solution:** Use `{total-pages}` placeholder:
```csharp
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div>Page {page} of {total-pages}</div>"
};
```

### Issue 5: prince-pdf-* Properties

**Symptom:** Prince-specific PDF properties not applying.

**Cause:** These are Prince extensions not in standard CSS.

**Solution:** Use IronPDF's SecuritySettings and MetaData:
```csharp
pdf.MetaData.Title = "Document Title";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
```

---

## Pre-Migration Checklist

### Assessment
- [ ] Identify all Prince command-line invocations
- [ ] Document CSS @page rules used
- [ ] List Prince-specific CSS properties
- [ ] Note any Prince JavaScript functions
- [ ] Identify PDF features used (encryption, metadata)

### Search for Prince Usage

```bash
# Find Prince process calls
grep -r "prince" --include="*.cs" .
grep -r "Process.Start" --include="*.cs" . | grep -i prince
grep -r "@page" --include="*.css" .
grep -r "prince-" --include="*.css" .
grep -r "string-set" --include="*.css" .
```

### Environment Preparation
- [ ] Obtain IronPDF license key
- [ ] Install IronPdf NuGet package
- [ ] Review CSS for Prince-specific rules
- [ ] Set up test environment

---

## Post-Migration Checklist

### Code Updates
- [ ] Remove Prince process management code
- [ ] Replace Process.Start with ChromePdfRenderer
- [ ] Convert command-line arguments to RenderingOptions
- [ ] Migrate @page CSS to RenderingOptions
- [ ] Replace margin boxes with HtmlHeader/HtmlFooter
- [ ] Convert counters to {page}/{total-pages}
- [ ] Remove temporary file handling
- [ ] Add proper exception handling
- [ ] Implement async/await where appropriate

### Testing
- [ ] Test HTML file conversion
- [ ] Test HTML string conversion
- [ ] Test URL conversion
- [ ] Verify page sizes match
- [ ] Verify margins match
- [ ] Test headers and footers
- [ ] Verify page numbers
- [ ] Test encryption/security
- [ ] Test metadata
- [ ] Verify PDF/A compliance
- [ ] Performance benchmark

### Cleanup
- [ ] Remove Prince installation from servers
- [ ] Remove Prince wrapper packages
- [ ] Delete temp file handling code
- [ ] Remove Prince-specific CSS
- [ ] Update deployment scripts
- [ ] Archive Prince license info

---

## Performance Comparison

| Operation | PrinceXML | IronPDF | Notes |
|-----------|-----------|---------|-------|
| Simple HTML | ~400ms | ~300ms | IronPDF in-process |
| Complex CSS | ~600ms | ~400ms | No process overhead |
| JavaScript pages | Limited | ~500ms | Full JS support |
| Large documents | ~1500ms | ~1000ms | Better memory |
| Concurrent (10) | ~4000ms | ~1500ms | Thread pool |
| Startup overhead | ~200ms | ~50ms | No process spawn |

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PrinceXML usages in codebase**
  ```bash
  grep -r "using PrinceXML" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package PrinceXML
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace PDF generation calls**
  ```csharp
  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering.

- [ ] **Update page settings**
  ```csharp
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Configure headers/footers**
  ```csharp
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine provides modern rendering. Verify key documents.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  var merged = PdfDocument.Merge(pdf1, pdf2);
  pdf.SecuritySettings.UserPassword = "secret";
  pdf.ApplyWatermark("<h1>DRAFT</h1>");
  ```
  **Why:** IronPDF provides many additional features.

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [HTML to PDF Tutorial](https://ironpdf.com/tutorials/html-to-pdf/)
- [Headers and Footers Guide](https://ironpdf.com/how-to/headers-and-footers/)
- [IronPDF NuGet Package](https://www.nuget.org/packages/IronPdf)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
