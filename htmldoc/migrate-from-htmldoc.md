# How Do I Migrate from HTMLDOC to IronPDF in C#?

## Table of Contents
1. [Why Migrate from HTMLDOC](#why-migrate-from-htmldoc)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate from HTMLDOC

### The HTMLDOC Reality

HTMLDOC is legacy technology from the late 1990s/early 2000s with fundamental limitations:

1. **Prehistoric Web Standards**: Built before CSS became integral to web design—no support for CSS3, HTML5, Flexbox, or Grid
2. **No JavaScript Support**: Cannot execute JavaScript, making dynamic content impossible
3. **GPL License Concerns**: Viral GPL license requires any incorporating software to also be GPL—problematic for commercial products
4. **Command-Line Only**: No native .NET library—requires process spawning, temp files, and output parsing
5. **Deprecated Rendering**: Simple HTML parser struggles with modern web layouts
6. **No Async Support**: Synchronous process execution blocks threads
7. **Limited Font Support**: Basic font handling with manual embedding required
8. **Platform Dependencies**: Requires HTMLDOC binary installed on target system

### The IronPDF Advantage

| Feature | HTMLDOC | IronPDF |
|---------|---------|---------|
| Rendering Engine | Custom HTML parser (1990s) | Modern Chromium |
| HTML/CSS Support | HTML 3.2, minimal CSS | HTML5, CSS3, Flexbox, Grid |
| JavaScript | None | Full execution |
| .NET Integration | None (command-line) | Native library |
| Async Support | No | Full async/await |
| License | GPL (viral) | Commercial (permissive) |
| Maintenance | Minimal updates | Active development |
| Support | Community only | Professional support |
| Deployment | Install binary | NuGet package |

### Migration Benefits

- **Modern Rendering**: Chromium engine handles any modern web content
- **Native Integration**: No process spawning, temp files, or shell escaping
- **Thread Safety**: Safe for multi-threaded server environments
- **Async Patterns**: Non-blocking PDF generation
- **Commercial License**: Deploy in proprietary software
- **Active Support**: Regular updates and security patches

The complete process for each of these improvements is documented in the [step-by-step migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-htmldoc-to-ironpdf/).

---

## Before You Start

### Prerequisites

1. **.NET Environment**: .NET Framework 4.6.2+ or .NET Core 3.1+ / .NET 5+
2. **NuGet Access**: Ability to install NuGet packages
3. **IronPDF License**: Free trial or purchased license key

### Installation

```bash
# Install IronPDF
dotnet add package IronPdf
```

### License Configuration

```csharp
// Add at application startup (Program.cs or Global.asax)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Identify HTMLDOC Usage

Since HTMLDOC is a command-line tool, search for process execution patterns:

```bash
# Find HTMLDOC usage patterns
grep -r "htmldoc\|HTMLDOC\|ProcessStartInfo" --include="*.cs" .
grep -r "Process\.Start\|CreateNoWindow" --include="*.cs" .
```

---

## Quick Start Migration

### Minimal Change Example

**Before (HTMLDOC via Process):**
```csharp
using System.Diagnostics;
using System.IO;

public class HtmlDocPdfService
{
    public byte[] GeneratePdf(string htmlContent)
    {
        // Write to temp file (HTMLDOC requires file input)
        string tempHtml = Path.GetTempFileName() + ".html";
        string tempPdf = Path.GetTempFileName() + ".pdf";

        try
        {
            File.WriteAllText(tempHtml, htmlContent);

            var startInfo = new ProcessStartInfo
            {
                FileName = "htmldoc",
                Arguments = $"--webpage --size A4 -f \"{tempPdf}\" \"{tempHtml}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    throw new Exception($"HTMLDOC failed: {error}");
                }
            }

            return File.ReadAllBytes(tempPdf);
        }
        finally
        {
            if (File.Exists(tempHtml)) File.Delete(tempHtml);
            if (File.Exists(tempPdf)) File.Delete(tempPdf);
        }
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
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    }

    public byte[] GeneratePdf(string htmlContent)
    {
        // No temp files, no process spawning, no cleanup
        var pdf = _renderer.RenderHtmlAsPdf(htmlContent);
        return pdf.BinaryData;
    }
}
```

---

## Complete API Reference

### Command-Line Flag Mappings

#### Document Type Flags

| HTMLDOC Flag | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `--webpage` | Default behavior | IronPDF treats all input as web content |
| `--book` | N/A | Use multiple renders + merge for chapters |
| `--continuous` | Default | IronPDF flows content continuously |
| `-f output.pdf` | `pdf.SaveAs("output.pdf")` | Or use `pdf.BinaryData` |

#### Page Size and Orientation

| HTMLDOC Flag | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `--size A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Standard paper sizes |
| `--size Letter` | `RenderingOptions.PaperSize = PdfPaperSize.Letter` | 8.5x11 inches |
| `--size Legal` | `RenderingOptions.PaperSize = PdfPaperSize.Legal` | 8.5x14 inches |
| `--size 8.5x11in` | `RenderingOptions.SetCustomPaperSize()` | Custom dimensions |
| `--landscape` | `RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape` | Horizontal |
| `--portrait` | `RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait` | Vertical (default) |

#### Margins

| HTMLDOC Flag | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `--top 20mm` | `RenderingOptions.MarginTop = 20` | IronPDF uses mm |
| `--bottom 20mm` | `RenderingOptions.MarginBottom = 20` | IronPDF uses mm |
| `--left 20mm` | `RenderingOptions.MarginLeft = 20` | IronPDF uses mm |
| `--right 20mm` | `RenderingOptions.MarginRight = 20` | IronPDF uses mm |
| `--top 1in` | `RenderingOptions.MarginTop = 25` | Convert: 1in = 25.4mm |

#### Headers and Footers

| HTMLDOC Flag | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `--header "..."` | `RenderingOptions.TextHeader` or `HtmlHeader` | Text or HTML headers |
| `--footer "..."` | `RenderingOptions.TextFooter` or `HtmlFooter` | Text or HTML footers |
| `--logo image.png` | Include in `HtmlHeader.HtmlFragment` | Use `<img>` tag |
| `$PAGE` | `{page}` | Current page placeholder |
| `$PAGES` | `{total-pages}` | Total pages placeholder |
| `$DATE` | `{date}` | Current date placeholder |
| `$TIME` | `{time}` | Current time placeholder |
| `$TITLE` | `{html-title}` | Document title placeholder |

#### PDF Options

| HTMLDOC Flag | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `--format pdf` | Default | IronPDF generates PDF by default |
| `--format pdf14` | `RenderingOptions.PdfVersion = PdfVersion.Pdf14` | PDF 1.4 |
| `--encryption` | `pdf.SecuritySettings.MakeDocumentReadOnly(password)` | Password protection |
| `--user-password xxx` | `pdf.SecuritySettings.UserPassword` | User password |
| `--owner-password xxx` | `pdf.SecuritySettings.OwnerPassword` | Owner password |
| `--embedfonts` | Default behavior | IronPDF embeds fonts by default |
| `--duplex` | N/A | Printer setting, not PDF content |
| `--links` | Default | Links preserved automatically |
| `--no-links` | N/A | Links always preserved |

#### Typography

| HTMLDOC Flag | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `--fontsize 12` | Use CSS `font-size` | Control via HTML/CSS |
| `--fontspacing 1.2` | Use CSS `line-height` | Control via HTML/CSS |
| `--bodyfont helvetica` | Use CSS `font-family` | Control via HTML/CSS |
| `--headfootfont` | Use CSS in header/footer HTML | Full CSS control |

#### Table of Contents

| HTMLDOC Flag | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `--toclevels 3` | `RenderingOptions.CreateTableOfContents()` | Auto-generate TOC |
| `--toctitle "Contents"` | Configure in HTML | Custom TOC title |
| `--no-toc` | Default | No TOC unless requested |

#### Miscellaneous

| HTMLDOC Flag | IronPDF Equivalent | Notes |
|--------------|-------------------|-------|
| `--verbose` | Logging configuration | Use .NET logging |
| `--quiet` | Default | No console output |
| `--compression` | Default | PDF compression automatic |
| `--no-compression` | N/A | Always compresses |
| `--jpeg` | N/A | Image handling automatic |
| `--grayscale` | Post-process with IronPDF | Image manipulation |

---

## Code Migration Examples

### Example 1: Basic HTML String to PDF

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;
using System.IO;

public byte[] ConvertHtmlToPdf(string html)
{
    string tempHtml = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.html");
    string tempPdf = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

    try
    {
        File.WriteAllText(tempHtml, html);

        var psi = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = $"--webpage -f \"{tempPdf}\" \"{tempHtml}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var p = Process.Start(psi))
        {
            p.WaitForExit(30000); // 30 second timeout
            if (!p.HasExited) p.Kill();
        }

        return File.ReadAllBytes(tempPdf);
    }
    finally
    {
        SafeDelete(tempHtml);
        SafeDelete(tempPdf);
    }
}

private void SafeDelete(string path)
{
    try { if (File.Exists(path)) File.Delete(path); } catch { }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertHtmlToPdf(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}

// Async version
public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

### Example 2: URL to PDF with Options

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;
using System.Net;
using System.IO;

public byte[] ConvertUrlToPdf(string url)
{
    // HTMLDOC has limited URL support, often need to download first
    string tempHtml = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.html");
    string tempPdf = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

    try
    {
        // Download HTML first (HTMLDOC URL handling is unreliable)
        using (var client = new WebClient())
        {
            client.DownloadFile(url, tempHtml);
        }

        var psi = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = $"--webpage --size A4 --landscape " +
                       $"--left 15mm --right 15mm --top 10mm --bottom 10mm " +
                       $"-f \"{tempPdf}\" \"{tempHtml}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var p = Process.Start(psi))
        {
            p.WaitForExit();
        }

        return File.ReadAllBytes(tempPdf);
    }
    finally
    {
        SafeDelete(tempHtml);
        SafeDelete(tempPdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertUrlToPdf(string url)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
    renderer.RenderingOptions.MarginLeft = 15;
    renderer.RenderingOptions.MarginRight = 15;
    renderer.RenderingOptions.MarginTop = 10;
    renderer.RenderingOptions.MarginBottom = 10;

    // Direct URL rendering with JavaScript support
    var pdf = renderer.RenderUrlAsPdf(url);
    return pdf.BinaryData;
}
```

### Example 3: Headers and Footers with Page Numbers

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;
using System.IO;

public byte[] CreateReportPdf(string html, string reportTitle)
{
    string tempHtml = Path.GetTempFileName() + ".html";
    string tempPdf = Path.GetTempFileName() + ".pdf";

    try
    {
        File.WriteAllText(tempHtml, html);

        // HTMLDOC header/footer syntax is cryptic
        // Format: left/center/right with special codes
        // $PAGE = page number, $PAGES = total pages
        var psi = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = $"--webpage --size A4 " +
                       $"--header \".{reportTitle}.\" " +    // center = title
                       $"--footer \"$DATE..$PAGE of $PAGES\" " + // left=date, right=page
                       $"-f \"{tempPdf}\" \"{tempHtml}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var p = Process.Start(psi))
        {
            p.WaitForExit();
        }

        return File.ReadAllBytes(tempPdf);
    }
    finally
    {
        SafeDelete(tempHtml);
        SafeDelete(tempPdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateReportPdf(string html, string reportTitle)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

    // HTML headers with full styling control
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = $@"
            <div style='width:100%; text-align:center; font-size:12px; color:#333;'>
                <strong>{reportTitle}</strong>
            </div>",
        MaxHeight = 20
    };

    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = @"
            <div style='width:100%; font-size:10px; color:#666;'>
                <span style='float:left;'>{date}</span>
                <span style='float:right;'>Page {page} of {total-pages}</span>
            </div>",
        MaxHeight = 20
    };

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 4: Multi-File Book Generation

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

public byte[] CreateBook(List<string> chapterFiles, string coverHtml)
{
    string tempCover = Path.GetTempFileName() + ".html";
    string tempPdf = Path.GetTempFileName() + ".pdf";

    try
    {
        File.WriteAllText(tempCover, coverHtml);

        // Build file list
        var allFiles = new List<string> { tempCover };
        allFiles.AddRange(chapterFiles);
        string fileArgs = string.Join(" ", allFiles.Select(f => $"\"{f}\""));

        var psi = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = $"--book --toclevels 2 --toctitle \"Table of Contents\" " +
                       $"--size A4 -f \"{tempPdf}\" {fileArgs}",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var p = Process.Start(psi))
        {
            p.WaitForExit();
        }

        return File.ReadAllBytes(tempPdf);
    }
    finally
    {
        SafeDelete(tempCover);
        SafeDelete(tempPdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;

public byte[] CreateBook(List<string> chapterFiles, string coverHtml)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

    // Render cover
    var coverPdf = renderer.RenderHtmlAsPdf(coverHtml);

    // Render each chapter
    var chapterPdfs = chapterFiles
        .Select(file => renderer.RenderHtmlFileAsPdf(file))
        .ToList();

    // Merge all PDFs
    var allPdfs = new List<PdfDocument> { coverPdf };
    allPdfs.AddRange(chapterPdfs);

    var book = PdfDocument.Merge(allPdfs);

    // Add table of contents (IronPDF auto-generates from headings)
    // Or create custom TOC as HTML and insert at position 1

    return book.BinaryData;
}
```

### Example 5: Password-Protected PDF

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;
using System.IO;

public byte[] CreateSecurePdf(string html, string userPassword, string ownerPassword)
{
    string tempHtml = Path.GetTempFileName() + ".html";
    string tempPdf = Path.GetTempFileName() + ".pdf";

    try
    {
        File.WriteAllText(tempHtml, html);

        var psi = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = $"--webpage --encryption " +
                       $"--user-password \"{userPassword}\" " +
                       $"--owner-password \"{ownerPassword}\" " +
                       $"-f \"{tempPdf}\" \"{tempHtml}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var p = Process.Start(psi))
        {
            p.WaitForExit();
        }

        return File.ReadAllBytes(tempPdf);
    }
    finally
    {
        SafeDelete(tempHtml);
        SafeDelete(tempPdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateSecurePdf(string html, string userPassword, string ownerPassword)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Apply security settings
    pdf.SecuritySettings.UserPassword = userPassword;
    pdf.SecuritySettings.OwnerPassword = ownerPassword;

    // Optional: Set permissions
    pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;
    pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

    return pdf.BinaryData;
}
```

### Example 6: Batch Processing

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

public void BatchConvert(List<string> htmlFiles, string outputDir)
{
    foreach (var htmlFile in htmlFiles)
    {
        string outputFile = Path.Combine(outputDir,
            Path.GetFileNameWithoutExtension(htmlFile) + ".pdf");

        var psi = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = $"--webpage -f \"{outputFile}\" \"{htmlFile}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var p = Process.Start(psi))
        {
            p.WaitForExit();
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;
using System.Threading.Tasks;

public async Task BatchConvertAsync(List<string> htmlFiles, string outputDir)
{
    var renderer = new ChromePdfRenderer();

    // Process in parallel for speed
    var tasks = htmlFiles.Select(async htmlFile =>
    {
        string outputFile = Path.Combine(outputDir,
            Path.GetFileNameWithoutExtension(htmlFile) + ".pdf");

        var pdf = await renderer.RenderHtmlFileAsPdfAsync(htmlFile);
        await pdf.SaveAsAsync(outputFile);
    });

    await Task.WhenAll(tasks);
}

// Synchronous version
public void BatchConvert(List<string> htmlFiles, string outputDir)
{
    var renderer = new ChromePdfRenderer();

    Parallel.ForEach(htmlFiles, htmlFile =>
    {
        string outputFile = Path.Combine(outputDir,
            Path.GetFileNameWithoutExtension(htmlFile) + ".pdf");

        var pdf = renderer.RenderHtmlFileAsPdf(htmlFile);
        pdf.SaveAs(outputFile);
    });
}
```

### Example 7: Logo in Header

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;
using System.IO;

public byte[] CreateBrandedPdf(string html, string logoPath)
{
    string tempHtml = Path.GetTempFileName() + ".html";
    string tempPdf = Path.GetTempFileName() + ".pdf";

    try
    {
        File.WriteAllText(tempHtml, html);

        // HTMLDOC --logo with header containing 'l'
        var psi = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = $"--webpage " +
                       $"--logo \"{logoPath}\" " +
                       $"--header \"l..\" " +  // 'l' = logo on left
                       $"-f \"{tempPdf}\" \"{tempHtml}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var p = Process.Start(psi))
        {
            p.WaitForExit();
        }

        return File.ReadAllBytes(tempPdf);
    }
    finally
    {
        SafeDelete(tempHtml);
        SafeDelete(tempPdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateBrandedPdf(string html, string logoPath)
{
    var renderer = new ChromePdfRenderer();

    // Convert logo to base64 for embedding
    byte[] logoBytes = File.ReadAllBytes(logoPath);
    string logoBase64 = Convert.ToBase64String(logoBytes);
    string logoMime = logoPath.EndsWith(".png") ? "image/png" : "image/jpeg";

    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = $@"
            <div style='width:100%;'>
                <img src='data:{logoMime};base64,{logoBase64}'
                     style='height:30px; float:left;' />
            </div>",
        MaxHeight = 40
    };

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 8: Custom Page Size

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;
using System.IO;

public byte[] CreateCustomSizePdf(string html, double widthInches, double heightInches)
{
    string tempHtml = Path.GetTempFileName() + ".html";
    string tempPdf = Path.GetTempFileName() + ".pdf";

    try
    {
        File.WriteAllText(tempHtml, html);

        var psi = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = $"--webpage " +
                       $"--size {widthInches}x{heightInches}in " +
                       $"-f \"{tempPdf}\" \"{tempHtml}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var p = Process.Start(psi))
        {
            p.WaitForExit();
        }

        return File.ReadAllBytes(tempPdf);
    }
    finally
    {
        SafeDelete(tempHtml);
        SafeDelete(tempPdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] CreateCustomSizePdf(string html, double widthInches, double heightInches)
{
    var renderer = new ChromePdfRenderer();

    // Convert inches to millimeters (IronPDF uses mm)
    double widthMm = widthInches * 25.4;
    double heightMm = heightInches * 25.4;

    renderer.RenderingOptions.SetCustomPaperSizeInMillimeters(widthMm, heightMm);

    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 9: Error Handling Comparison

**Before (HTMLDOC):**
```csharp
using System.Diagnostics;
using System.IO;

public byte[] ConvertWithErrorHandling(string html)
{
    string tempHtml = Path.GetTempFileName() + ".html";
    string tempPdf = Path.GetTempFileName() + ".pdf";

    try
    {
        File.WriteAllText(tempHtml, html);

        var psi = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = $"--webpage -f \"{tempPdf}\" \"{tempHtml}\"",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using (var p = Process.Start(psi))
        {
            string stderr = p.StandardError.ReadToEnd();
            string stdout = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                throw new Exception($"HTMLDOC failed with exit code {p.ExitCode}: {stderr}");
            }

            // Check if output file exists and has content
            if (!File.Exists(tempPdf))
            {
                throw new Exception("HTMLDOC did not produce output file");
            }

            var result = File.ReadAllBytes(tempPdf);
            if (result.Length == 0)
            {
                throw new Exception("HTMLDOC produced empty PDF");
            }

            return result;
        }
    }
    catch (FileNotFoundException)
    {
        throw new Exception("HTMLDOC executable not found. Ensure it is installed and in PATH.");
    }
    catch (System.ComponentModel.Win32Exception ex)
    {
        throw new Exception($"Failed to start HTMLDOC: {ex.Message}");
    }
    finally
    {
        SafeDelete(tempHtml);
        SafeDelete(tempPdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] ConvertWithErrorHandling(string html)
{
    try
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);

        if (pdf.PageCount == 0)
        {
            throw new Exception("PDF generation produced no pages");
        }

        return pdf.BinaryData;
    }
    catch (IronPdf.Exceptions.IronPdfLicensingException ex)
    {
        throw new Exception("IronPDF license error: " + ex.Message);
    }
    catch (Exception ex)
    {
        throw new Exception("PDF generation failed: " + ex.Message, ex);
    }
}
```

### Example 10: Service Class Refactoring

**Before (HTMLDOC):**
```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

public class HtmlDocPdfService : IDisposable
{
    private readonly string _htmlDocPath;
    private readonly SemaphoreSlim _semaphore;
    private readonly string _tempDir;

    public HtmlDocPdfService(string htmlDocPath = "htmldoc", int maxConcurrent = 4)
    {
        _htmlDocPath = htmlDocPath;
        _semaphore = new SemaphoreSlim(maxConcurrent);
        _tempDir = Path.Combine(Path.GetTempPath(), "htmldoc_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempDir);
    }

    public byte[] GeneratePdf(string html, PdfOptions options)
    {
        _semaphore.Wait();

        string tempHtml = Path.Combine(_tempDir, $"{Guid.NewGuid()}.html");
        string tempPdf = Path.Combine(_tempDir, $"{Guid.NewGuid()}.pdf");

        try
        {
            File.WriteAllText(tempHtml, html);

            var args = $"--webpage --size {options.PageSize} " +
                      $"--top {options.MarginTop}mm --bottom {options.MarginBottom}mm " +
                      $"--left {options.MarginLeft}mm --right {options.MarginRight}mm ";

            if (options.Landscape) args += "--landscape ";
            if (!string.IsNullOrEmpty(options.Header))
                args += $"--header \"{options.Header}\" ";
            if (!string.IsNullOrEmpty(options.Footer))
                args += $"--footer \"{options.Footer}\" ";

            args += $"-f \"{tempPdf}\" \"{tempHtml}\"";

            var psi = new ProcessStartInfo
            {
                FileName = _htmlDocPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var p = Process.Start(psi))
            {
                p.WaitForExit(60000);
                if (!p.HasExited)
                {
                    p.Kill();
                    throw new TimeoutException("HTMLDOC timed out");
                }
            }

            return File.ReadAllBytes(tempPdf);
        }
        finally
        {
            SafeDelete(tempHtml);
            SafeDelete(tempPdf);
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private void SafeDelete(string path)
    {
        try { File.Delete(path); } catch { }
    }
}

public class PdfOptions
{
    public string PageSize { get; set; } = "A4";
    public bool Landscape { get; set; }
    public int MarginTop { get; set; } = 20;
    public int MarginBottom { get; set; } = 20;
    public int MarginLeft { get; set; } = 20;
    public int MarginRight { get; set; } = 20;
    public string Header { get; set; }
    public string Footer { get; set; }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService(string licenseKey)
    {
        IronPdf.License.LicenseKey = licenseKey;
        _renderer = new ChromePdfRenderer();
    }

    public byte[] GeneratePdf(string html, PdfOptions options)
    {
        // Configure renderer based on options
        _renderer.RenderingOptions.PaperSize = GetPaperSize(options.PageSize);
        _renderer.RenderingOptions.PaperOrientation = options.Landscape
            ? PdfPaperOrientation.Landscape
            : PdfPaperOrientation.Portrait;

        _renderer.RenderingOptions.MarginTop = options.MarginTop;
        _renderer.RenderingOptions.MarginBottom = options.MarginBottom;
        _renderer.RenderingOptions.MarginLeft = options.MarginLeft;
        _renderer.RenderingOptions.MarginRight = options.MarginRight;

        if (!string.IsNullOrEmpty(options.Header))
        {
            _renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
            {
                HtmlFragment = options.Header,
                MaxHeight = 25
            };
        }

        if (!string.IsNullOrEmpty(options.Footer))
        {
            _renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
            {
                HtmlFragment = options.Footer,
                MaxHeight = 25
            };
        }

        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }

    public async Task<byte[]> GeneratePdfAsync(string html, PdfOptions options)
    {
        // Apply same options...
        var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }

    private PdfPaperSize GetPaperSize(string size)
    {
        return size?.ToUpperInvariant() switch
        {
            "A4" => PdfPaperSize.A4,
            "LETTER" => PdfPaperSize.Letter,
            "LEGAL" => PdfPaperSize.Legal,
            "A3" => PdfPaperSize.A3,
            "A5" => PdfPaperSize.A5,
            _ => PdfPaperSize.A4
        };
    }
}

public class PdfOptions
{
    public string PageSize { get; set; } = "A4";
    public bool Landscape { get; set; }
    public int MarginTop { get; set; } = 20;
    public int MarginBottom { get; set; } = 20;
    public int MarginLeft { get; set; } = 20;
    public int MarginRight { get; set; } = 20;
    public string Header { get; set; }  // Now accepts HTML!
    public string Footer { get; set; }  // Now accepts HTML!
}
```

---

## Advanced Scenarios

### Eliminating Temp File Management

HTMLDOC requires file system operations. IronPDF eliminates this entirely:

```csharp
// HTMLDOC: Required temp file management
// 1. Create temp directory
// 2. Write HTML to temp file
// 3. Run HTMLDOC process
// 4. Read output PDF
// 5. Clean up temp files
// 6. Handle cleanup failures

// IronPDF: Memory-only operation
public byte[] GeneratePdf(string html)
{
    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Removing Process Management Code

**Delete all HTMLDOC process infrastructure:**

```csharp
// DELETE: Process spawning code
// DELETE: Process timeout handling
// DELETE: Exit code checking
// DELETE: stderr/stdout capture
// DELETE: Process kill on timeout
// DELETE: PATH environment checks
// DELETE: HTMLDOC installation verification
```

### Converting Shell Escaping to Native API

**Before (shell escaping nightmare):**
```csharp
// Dangerous: shell injection possible if html contains quotes
string args = $"--webpage -f \"{outputPath}\" \"{inputPath}\"";

// Even worse with dynamic options
string header = userProvidedHeader.Replace("\"", "\\\"");
string args = $"--header \"{header}\" ...";
```

**After (native API, no escaping needed):**
```csharp
// Safe: native .NET strings, no shell interpretation
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = userProvidedHeader  // Any content is safe
};
```

### Parallel Processing Without Process Pool

```csharp
// IronPDF handles concurrency internally
public async Task<List<byte[]>> GenerateManyPdfs(List<string> htmlContents)
{
    var renderer = new ChromePdfRenderer();

    var tasks = htmlContents.Select(async html =>
    {
        var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    });

    return (await Task.WhenAll(tasks)).ToList();
}
```

---

## Performance Considerations

### Startup Time

| Aspect | HTMLDOC | IronPDF |
|--------|---------|---------|
| First render | 50-200ms (process start) | 1-3s (Chromium init) |
| Subsequent renders | 50-200ms each | 50-200ms |
| Parallel renders | Process overhead × N | Shared engine |

### Memory Usage

| Aspect | HTMLDOC | IronPDF |
|--------|---------|---------|
| Temp files | Required (disk I/O) | None |
| Per-render memory | New process each time | Shared memory pool |
| Cleanup | Manual | Automatic |

### Optimization Tips

1. **Reuse Renderer**: Create `ChromePdfRenderer` once, reuse for all conversions
2. **Parallel Processing**: Use `Task.WhenAll` for batch operations
3. **Async Methods**: Use `RenderHtmlAsPdfAsync()` in web applications
4. **First-render warm-up**: Optionally render a simple PDF at startup

```csharp
public class OptimizedPdfService
{
    private readonly ChromePdfRenderer _renderer;
    private bool _warmedUp;

    public OptimizedPdfService()
    {
        _renderer = new ChromePdfRenderer();
    }

    public async Task WarmUpAsync()
    {
        if (!_warmedUp)
        {
            await _renderer.RenderHtmlAsPdfAsync("<html></html>");
            _warmedUp = true;
        }
    }

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        await WarmUpAsync();
        var pdf = await _renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }
}
```

---

## Troubleshooting

### Issue 1: "HTMLDOC executable not found"

**Cause**: HTMLDOC was not installed or not in PATH
**IronPDF Solution**: No external dependency—just install NuGet package

```csharp
// Remove all HTMLDOC installation checks
// IronPDF includes all dependencies in the NuGet package
```

### Issue 2: Modern CSS Not Rendering

**Cause**: HTMLDOC uses 1990s HTML parser
**IronPDF Solution**: Full CSS3 support with Chromium

```csharp
// Flexbox, Grid, CSS3 all work automatically
string html = @"
<div style='display: flex; justify-content: space-between;'>
    <div>Left</div>
    <div>Right</div>
</div>";

var pdf = renderer.RenderHtmlAsPdf(html);  // Just works!
```

### Issue 3: JavaScript Not Executing

**Cause**: HTMLDOC has no JavaScript support
**IronPDF Solution**: Full JavaScript execution

```csharp
renderer.RenderingOptions.EnableJavaScript = true;  // Default is true
renderer.RenderingOptions.RenderDelay = 500;  // Wait for JS to complete

var pdf = renderer.RenderUrlAsPdf("https://spa-app.example.com");
```

### Issue 4: Character Encoding Issues

**Cause**: HTMLDOC charset handling is inconsistent
**IronPDF Solution**: UTF-8 by default

```csharp
// IronPDF handles encoding automatically
string html = "<html><head><meta charset='UTF-8'></head><body>中文 العربية</body></html>";
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 5: Process Timeout Errors

**Cause**: HTMLDOC process hung or took too long
**IronPDF Solution**: Built-in timeout handling

```csharp
// Remove process timeout code
// IronPDF handles timeouts internally

renderer.RenderingOptions.Timeout = 60000;  // 60 seconds (optional)
```

### Issue 6: Temp File Cleanup Failures

**Cause**: File locked or permissions issue
**IronPDF Solution**: No temp files used

```csharp
// DELETE all temp file cleanup code
// IronPDF operates entirely in memory
```

### Issue 7: GPL License Concerns

**Cause**: HTMLDOC GPL license infects incorporating software
**IronPDF Solution**: Commercial license with clear terms

```csharp
// IronPDF commercial license allows:
// - Proprietary software integration
// - No source code disclosure
// - Commercial distribution
```

### Issue 8: Concurrent Process Limits

**Cause**: Too many HTMLDOC processes overwhelm system
**IronPDF Solution**: Single shared Chromium engine

```csharp
// Remove semaphore/process pool code
// IronPDF manages concurrency internally

var tasks = htmlList.Select(html =>
    renderer.RenderHtmlAsPdfAsync(html));
await Task.WhenAll(tasks);  // Safe parallel execution
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Identify all HTMLDOC usage**
  ```bash
  grep -r "htmldoc" --include="*.cs" .
  grep -r "ProcessStartInfo" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **List all HTMLDOC command-line flags used**
  ```bash
  // Example command-line flags
  htmldoc --webpage --header $PAGE --footer $DATE
  ```
  **Why:** These flags map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Document current temp file patterns**
  ```csharp
  // Example temp file creation
  var tempFile = Path.GetTempFileName();
  ```
  **Why:** IronPDF does not require temp files, simplifying code and reducing I/O operations.

- [ ] **Note any process pool/semaphore code**
  ```csharp
  // Example semaphore usage
  var semaphore = new SemaphoreSlim(1, 1);
  ```
  **Why:** IronPDF's native .NET integration eliminates the need for process management.

- [ ] **Check GPL license compliance needs**
  **Why:** IronPDF's commercial license is more permissive and suitable for proprietary software.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Plan testing for modern CSS/JS content**
  **Why:** IronPDF supports modern web standards, allowing for more comprehensive testing of HTML5/CSS3/JavaScript content.

### Code Changes

- [ ] **Install IronPDF NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Install the IronPDF package to replace HTMLDOC functionality.

- [ ] **Add license key configuration**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace `ProcessStartInfo` with `ChromePdfRenderer`**
  ```csharp
  // Before (HTMLDOC)
  var process = new ProcessStartInfo("htmldoc", "--webpage input.html -o output.pdf");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(File.ReadAllText("input.html"));
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF provides a direct .NET API, eliminating the need for external process management.

- [ ] **Convert command-line flags to `RenderingOptions`**
  ```csharp
  // Before (HTMLDOC)
  var args = "--header $PAGE --footer $DATE";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page}</div>"
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>{date}</div>"
  };
  ```
  **Why:** IronPDF's RenderingOptions provide a more flexible and powerful way to configure PDF output.

- [ ] **Update placeholder syntax ($PAGE → {page}, $PAGES → {total-pages})**
  ```csharp
  // Before (HTMLDOC)
  var header = "$PAGE of $PAGES";

  // After (IronPDF)
  var header = "<div style='text-align:center;'>Page {page} of {total-pages}</div>";
  ```
  **Why:** IronPDF uses HTML with placeholders for dynamic content in headers and footers.

- [ ] **Remove temp file creation code**
  ```csharp
  // Before (HTMLDOC)
  var tempFile = Path.GetTempFileName();

  // After (IronPDF)
  // No temp file needed
  ```
  **Why:** IronPDF processes data in-memory, reducing the need for temporary storage.

- [ ] **Remove temp file cleanup code**
  ```csharp
  // Before (HTMLDOC)
  File.Delete(tempFile);

  // After (IronPDF)
  // No temp file to delete
  ```
  **Why:** Simplifies code by removing unnecessary file operations.

- [ ] **Replace exit code checks with try/catch**
  ```csharp
  // Before (HTMLDOC)
  if (process.ExitCode != 0) { /* handle error */ }

  // After (IronPDF)
  try
  {
      var pdf = renderer.RenderHtmlAsPdf(html);
  }
  catch (Exception ex)
  {
      // handle error
  }
  ```
  **Why:** IronPDF uses exceptions for error handling, providing more detailed error information.

- [ ] **Remove process timeout handling**
  ```csharp
  // Before (HTMLDOC)
  process.WaitForExit(5000);

  // After (IronPDF)
  // No process timeout needed
  ```
  **Why:** IronPDF's synchronous and asynchronous methods handle execution without external timeouts.

- [ ] **Update header/footer from cryptic format to HTML**
  ```csharp
  // Before (HTMLDOC)
  var header = "$PAGE";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page}</div>"
  };
  ```
  **Why:** HTML headers/footers allow for richer formatting and dynamic content.

- [ ] **Convert unit specifications (add explicit mm if needed)**
  ```csharp
  // Before (HTMLDOC)
  var margin = "20";

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 20; // in mm
  ```
  **Why:** IronPDF uses explicit units for clarity and precision in layout settings.

### Process Changes

- [ ] **Delete HTMLDOC executable management**
  **Why:** IronPDF is managed via NuGet, removing the need for executable management.

- [ ] **Remove HTMLDOC installation scripts**
  **Why:** Simplifies deployment by eliminating the need for external installation scripts.

- [ ] **Delete PATH configuration**
  **Why:** No need to configure system PATH for IronPDF, as it is a .NET library.

- [ ] **Remove semaphore/process pool code**
  **Why:** IronPDF's native .NET integration eliminates the need for process management.

- [ ] **Delete temp directory management**
  **Why:** IronPDF processes data in-memory, reducing the need for temporary storage.

### Post-Migration

- [ ] **Remove HTMLDOC executable from deployment**
  **Why:** Simplifies deployment by removing unnecessary binaries.

- [ ] **Update deployment scripts**
  **Why:** Ensure deployment scripts reflect the new IronPDF dependency.

- [ ] **Test with modern HTML5/CSS3 content**
  **Why:** Verify IronPDF's rendering capabilities with modern web standards.

- [ ] **Test JavaScript-dependent pages**
  **Why:** Ensure dynamic content renders correctly with IronPDF's JavaScript support.

- [ ] **Verify parallel processing works**
  **Why:** IronPDF's thread-safe design should support concurrent PDF generation.

- [ ] **Test error handling**
  **Why:** Ensure robust error handling with IronPDF's exception-based model.

- [ ] **Performance benchmark comparison**
  **Why:** Compare performance metrics to evaluate improvements with IronPDF.

- [ ] **Document any CSS improvements possible now**
  **Why:** Leverage IronPDF's support for modern CSS to enhance document styling.

### Files to Delete

- [ ] **HTMLDOC wrapper classes**
  **Why:** No longer needed with IronPDF's native .NET API.

- [ ] **Process management utilities**
  **Why:** IronPDF eliminates the need for external process management.

- [ ] **Temp file helper methods**
  **Why:** Simplifies code by removing unnecessary file operations.

- [ ] **Installation check code**
  **Why:** IronPDF is managed via NuGet, removing the need for installation checks.

- [ ] **PATH verification code**
  **Why:** No need to verify system PATH for IronPDF, as it is a .NET library.
---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/
- **HTMLDOC Documentation**: https://www.msweet.org/htmldoc/htmldoc.html
- **HTMLDOC GitHub**: https://github.com/michaelrsweet/htmldoc

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
