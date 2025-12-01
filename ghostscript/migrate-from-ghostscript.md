# How Do I Migrate from Ghostscript to IronPDF in C#?

## Table of Contents
1. [Why Migrate from Ghostscript to IronPDF](#why-migrate-from-ghostscript-to-ironpdf)
2. [Migration Complexity Assessment](#migration-complexity-assessment)
3. [Before You Start](#before-you-start)
4. [Quick Start Migration](#quick-start-migration)
5. [Complete API Reference](#complete-api-reference)
6. [Command-Line Switch Mapping](#command-line-switch-mapping)
7. [Code Migration Examples](#code-migration-examples)
8. [Advanced Scenarios](#advanced-scenarios)
9. [Performance Considerations](#performance-considerations)
10. [Troubleshooting](#troubleshooting)
11. [Migration Checklist](#migration-checklist)

---

## Why Migrate from Ghostscript to IronPDF

### The Ghostscript Problems

Ghostscript is a venerable PostScript/PDF interpreter with decades of history, but its use in modern .NET applications presents significant challenges:

1. **AGPL License Restrictions**: Ghostscript's AGPL license requires you to release your source code if you distribute software that uses it—unless you purchase an expensive commercial license from Artifex.

2. **Command-Line Interface**: Ghostscript is fundamentally a command-line tool. Using it from C# requires spawning processes, passing string arguments, and parsing output—a fragile and error-prone approach.

3. **External Binary Dependency**: You must install Ghostscript separately, manage PATH variables, and ensure version compatibility across deployment environments.

4. **PostScript-Centric Design**: Ghostscript was built for PostScript interpretation. PDF support was added later, and HTML-to-PDF requires external tools like wkhtmltopdf.

5. **No Native HTML-to-PDF**: Ghostscript cannot convert HTML to PDF directly. You need a multi-step pipeline with external tools.

6. **Complex Switch Syntax**: Operations are controlled via cryptic command-line switches like `-dNOPAUSE -dBATCH -sDEVICE=pdfwrite -sOutputFile=...`

7. **Error Handling**: Errors come through stderr as text strings, requiring parsing rather than structured exception handling.

8. **Process Management Overhead**: Each operation spawns a separate process, adding overhead and complexity for error handling, timeouts, and resource cleanup.

9. **Platform-Specific Binaries**: Different DLLs for 32-bit vs 64-bit (`gsdll32.dll` vs `gsdll64.dll`), requiring careful deployment configuration.

### IronPDF Advantages

| Aspect | Ghostscript | IronPDF |
|--------|-------------|---------|
| License | AGPL (viral) or expensive commercial | Commercial with clear terms |
| Integration | Command-line process spawning | Native .NET library |
| API Design | String-based switches | Typed, IntelliSense-enabled API |
| Error Handling | Parse stderr text | .NET exceptions |
| HTML-to-PDF | Not supported (need external tools) | Built-in Chromium engine |
| Dependencies | External binary installation | Self-contained NuGet package |
| Deployment | Configure PATH, copy DLLs | Just add NuGet reference |
| Thread Safety | Process isolation only | Thread-safe by design |
| Modern .NET | Limited support | Full .NET 6/7/8 support |
| Async Support | Process-based | Native async/await |

---

## Migration Complexity Assessment

### Estimated Effort by Feature

| Feature | Migration Complexity | Notes |
|---------|---------------------|-------|
| PDF to Images | Low | Direct API mapping |
| Merge PDFs | Low | Simpler with IronPDF |
| Compress PDF | Low | Built-in options |
| PostScript to PDF | Medium | Convert PS → PDF first |
| PDF Optimization | Low | Different approach |
| Encryption | Medium | Different API |
| PDF/A Conversion | Low | Built-in support |
| Custom Switches | Medium-High | Research equivalent features |

### Paradigm Shift

The fundamental shift is from **command-line process execution** to **typed .NET API calls**:

```
Ghostscript:  "Pass these string switches to external process"
IronPDF:      "Call these methods on .NET objects"
```

---

## Before You Start

### Prerequisites

1. **.NET Version**: IronPDF supports .NET Framework 4.6.2+ and .NET Core 2.0+ / .NET 5+
2. **License Key**: Obtain your IronPDF license key from [ironpdf.com](https://ironpdf.com)
3. **Backup**: Create a branch for migration work

### Identify All Ghostscript Usage

```bash
# Find all Ghostscript.NET references
grep -r "Ghostscript\.NET\|GhostscriptProcessor\|GhostscriptRasterizer\|gsdll" --include="*.cs" .

# Find direct process calls to Ghostscript
grep -r "gswin64c\|gswin32c\|gs\|ProcessStartInfo.*ghost" --include="*.cs" .

# Find package references
grep -r "Ghostscript" --include="*.csproj" .
```

### NuGet Package Changes

```bash
# Remove Ghostscript.NET
dotnet remove package Ghostscript.NET

# Install IronPDF
dotnet add package IronPdf
```

### Remove Ghostscript Dependencies

After migration:
- Uninstall Ghostscript from servers
- Remove `gsdll32.dll` / `gsdll64.dll` from deployments
- Remove PATH configuration for Ghostscript
- Remove any `GhostscriptVersionInfo` references

---

## Quick Start Migration

### Minimal Migration Example

**Before (Ghostscript.NET):**
```csharp
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using System.Collections.Generic;

// Find Ghostscript DLL
GhostscriptVersionInfo gvi = new GhostscriptVersionInfo("gsdll64.dll");

// Merge PDFs using command-line switches
using (GhostscriptProcessor processor = new GhostscriptProcessor(gvi))
{
    List<string> switches = new List<string>
    {
        "-dNOPAUSE",
        "-dBATCH",
        "-dSAFER",
        "-sDEVICE=pdfwrite",
        "-sOutputFile=merged.pdf",
        "file1.pdf",
        "file2.pdf"
    };

    processor.Process(switches.ToArray());
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf1 = PdfDocument.FromFile("file1.pdf");
var pdf2 = PdfDocument.FromFile("file2.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

**Key Differences:**
- No external DLL to locate
- No command-line switches to memorize
- Type-safe API with IntelliSense
- Proper exception handling

---

## Complete API Reference

### Namespace Mapping

| Ghostscript.NET | IronPDF |
|-----------------|---------|
| `Ghostscript.NET` | `IronPdf` |
| `Ghostscript.NET.Processor` | `IronPdf` |
| `Ghostscript.NET.Rasterizer` | `IronPdf` |
| N/A (command-line) | `IronPdf.Rendering` |

### Core Class Mapping

| Ghostscript.NET | IronPDF | Description |
|-----------------|---------|-------------|
| `GhostscriptProcessor` | Various `PdfDocument` methods | PDF processing |
| `GhostscriptRasterizer` | `PdfDocument.RasterizeToImageFiles()` | PDF to images |
| `GhostscriptVersionInfo` | N/A (not needed) | DLL location |
| `GhostscriptStdIO` | N/A (use exceptions) | I/O handling |
| Process + command-line | `ChromePdfRenderer` | HTML to PDF |

### Common Operations Mapping

| Ghostscript Operation | IronPDF Equivalent | Notes |
|-----------------------|-------------------|-------|
| `processor.Process(switches)` | Various methods | Switch-dependent |
| `rasterizer.Open(file)` | `PdfDocument.FromFile(file)` | Load PDF |
| `rasterizer.GetPage(dpi, page)` | `pdf.ToBitmap(dpi)` | Rasterize |
| `rasterizer.PageCount` | `pdf.PageCount` | Get page count |
| `-sDEVICE=pdfwrite` merge | `PdfDocument.Merge()` | Merge PDFs |
| `-sDEVICE=png16m` | `pdf.RasterizeToImageFiles()` | Export images |
| `-dPDFSETTINGS=/screen` | `pdf.CompressImages()` | Compress |
| `-sOwnerPassword=` | `pdf.SecuritySettings.OwnerPassword` | Encryption |

---

## Command-Line Switch Mapping

### Common Ghostscript Switches to IronPDF

| Ghostscript Switch | IronPDF Equivalent | Description |
|-------------------|-------------------|-------------|
| `-dNOPAUSE` | N/A (not needed) | Don't pause between pages |
| `-dBATCH` | N/A (not needed) | Exit after processing |
| `-dSAFER` | N/A (default) | Safe file access |
| `-sDEVICE=pdfwrite` | Various PDF methods | Output PDF |
| `-sDEVICE=png16m` | `RasterizeToImageFiles("*.png")` | PNG output |
| `-sDEVICE=jpeg` | `RasterizeToImageFiles("*.jpg")` | JPEG output |
| `-sDEVICE=tiff24nc` | `RasterizeToImageFiles("*.tiff")` | TIFF output |
| `-sOutputFile=X` | `SaveAs("X")` | Output filename |
| `-r300` | DPI parameter in methods | Resolution |
| `-dPDFSETTINGS=/screen` | `CompressImages(quality: 50)` | Low quality |
| `-dPDFSETTINGS=/ebook` | `CompressImages(quality: 75)` | Medium quality |
| `-dPDFSETTINGS=/prepress` | N/A (default high quality) | High quality |
| `-dFirstPage=N` | `CopyPages(N-1, ...)` | Start page (1-indexed) |
| `-dLastPage=N` | `CopyPages(..., N-1)` | End page (1-indexed) |
| `-sOwnerPassword=X` | `SecuritySettings.OwnerPassword` | Owner password |
| `-sUserPassword=X` | `SecuritySettings.UserPassword` | User password |
| `-dCompatibilityLevel=1.4` | `RenderingOptions.PdfVersion` | PDF version |
| `-dFastWebView` | N/A | Linearize PDF |
| `-dDetectDuplicateImages` | Automatic | Duplicate detection |
| `-dCompressFonts` | Automatic | Font compression |

### Resolution/DPI Mapping

| Ghostscript | IronPDF |
|-------------|---------|
| `-r72` | DPI: 72 |
| `-r150` | DPI: 150 |
| `-r300` | DPI: 300 (default) |
| `-r600` | DPI: 600 |

---

## Code Migration Examples

### Example 1: PDF to Images

**Before (Ghostscript.NET):**
```csharp
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using System.Drawing;
using System.Drawing.Imaging;

class Program
{
    static void Main()
    {
        // Must locate Ghostscript DLL
        GhostscriptVersionInfo gvi = new GhostscriptVersionInfo("gsdll64.dll");

        using (GhostscriptRasterizer rasterizer = new GhostscriptRasterizer())
        {
            rasterizer.Open("document.pdf", gvi, false);

            int dpi = 300;

            for (int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
            {
                // GetPage returns System.Drawing.Image
                Image img = rasterizer.GetPage(dpi, dpi, pageNumber);
                img.Save($"page_{pageNumber}.png", ImageFormat.Png);
                img.Dispose();
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");

        // Single line to export all pages
        pdf.RasterizeToImageFiles("page_*.png", DPI: 300);

        // Or get images in memory
        var images = pdf.ToBitmap(300);
        for (int i = 0; i < images.Length; i++)
        {
            images[i].Save($"page_{i + 1}.png");
        }
    }
}
```

### Example 2: Merge Multiple PDFs

**Before (Ghostscript.NET):**
```csharp
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        GhostscriptVersionInfo gvi = new GhostscriptVersionInfo("gsdll64.dll");

        using (GhostscriptProcessor processor = new GhostscriptProcessor(gvi))
        {
            List<string> switches = new List<string>
            {
                "-dNOPAUSE",
                "-dBATCH",
                "-dSAFER",
                "-sDEVICE=pdfwrite",
                "-sOutputFile=merged_book.pdf",
                "chapter1.pdf",
                "chapter2.pdf",
                "chapter3.pdf",
                "appendix.pdf"
            };

            try
            {
                processor.Process(switches.ToArray());
                Console.WriteLine("Merge completed successfully");
            }
            catch (Exception ex)
            {
                // Error handling is difficult - parse stderr
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var files = new[] { "chapter1.pdf", "chapter2.pdf", "chapter3.pdf", "appendix.pdf" };

        // Load all PDFs
        var pdfs = files.Select(f => PdfDocument.FromFile(f)).ToList();

        // Merge
        var merged = PdfDocument.Merge(pdfs);
        merged.SaveAs("merged_book.pdf");

        // Cleanup
        foreach (var pdf in pdfs) pdf.Dispose();

        Console.WriteLine("Merge completed successfully");
    }
}
```

### Example 3: Compress PDF

**Before (Ghostscript):**
```csharp
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        GhostscriptVersionInfo gvi = new GhostscriptVersionInfo("gsdll64.dll");

        using (GhostscriptProcessor processor = new GhostscriptProcessor(gvi))
        {
            List<string> switches = new List<string>
            {
                "-dNOPAUSE",
                "-dBATCH",
                "-dSAFER",
                "-sDEVICE=pdfwrite",
                "-dCompatibilityLevel=1.4",
                "-dPDFSETTINGS=/ebook",  // /screen, /ebook, /printer, /prepress
                "-dColorImageDownsampleType=/Bicubic",
                "-dColorImageResolution=150",
                "-dGrayImageDownsampleType=/Bicubic",
                "-dGrayImageResolution=150",
                "-sOutputFile=compressed.pdf",
                "large_document.pdf"
            };

            processor.Process(switches.ToArray());
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("large_document.pdf");

        // Compress images
        pdf.CompressImages(quality: 75);  // 0-100, lower = smaller

        pdf.SaveAs("compressed.pdf");
    }
}
```

### Example 4: Extract Page Range

**Before (Ghostscript):**
```csharp
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        GhostscriptVersionInfo gvi = new GhostscriptVersionInfo("gsdll64.dll");

        // Extract pages 5-10 from a PDF
        using (GhostscriptProcessor processor = new GhostscriptProcessor(gvi))
        {
            List<string> switches = new List<string>
            {
                "-dNOPAUSE",
                "-dBATCH",
                "-dSAFER",
                "-sDEVICE=pdfwrite",
                "-dFirstPage=5",
                "-dLastPage=10",
                "-sOutputFile=pages_5_to_10.pdf",
                "document.pdf"
            };

            processor.Process(switches.ToArray());
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");

        // Extract pages 5-10 (0-indexed: 4-9)
        var extracted = pdf.CopyPages(4, 5, 6, 7, 8, 9);
        extracted.SaveAs("pages_5_to_10.pdf");

        // Or use a range
        var pages = Enumerable.Range(4, 6).ToArray();  // [4, 5, 6, 7, 8, 9]
        var extracted2 = pdf.CopyPages(pages);
        extracted2.SaveAs("pages_5_to_10_v2.pdf");
    }
}
```

### Example 5: Password Protection

**Before (Ghostscript):**
```csharp
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        GhostscriptVersionInfo gvi = new GhostscriptVersionInfo("gsdll64.dll");

        using (GhostscriptProcessor processor = new GhostscriptProcessor(gvi))
        {
            List<string> switches = new List<string>
            {
                "-dNOPAUSE",
                "-dBATCH",
                "-dSAFER",
                "-sDEVICE=pdfwrite",
                "-sOwnerPassword=owner123",
                "-sUserPassword=user456",
                "-dEncryptionR=3",
                "-dKeyLength=128",
                "-dPermissions=-300",  // Restrict printing/copying
                "-sOutputFile=protected.pdf",
                "document.pdf"
            };

            processor.Process(switches.ToArray());
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");

        // Set passwords
        pdf.SecuritySettings.OwnerPassword = "owner123";
        pdf.SecuritySettings.UserPassword = "user456";

        // Set permissions (readable property names!)
        pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;
        pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
        pdf.SecuritySettings.AllowUserAnnotations = false;
        pdf.SecuritySettings.AllowUserFormData = false;

        pdf.SaveAs("protected.pdf");
    }
}
```

### Example 6: Convert PostScript to PDF

**Before (Ghostscript):**
```csharp
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        GhostscriptVersionInfo gvi = new GhostscriptVersionInfo("gsdll64.dll");

        using (GhostscriptProcessor processor = new GhostscriptProcessor(gvi))
        {
            List<string> switches = new List<string>
            {
                "-dNOPAUSE",
                "-dBATCH",
                "-dSAFER",
                "-sDEVICE=pdfwrite",
                "-sOutputFile=from_postscript.pdf",
                "document.ps"
            };

            processor.Process(switches.ToArray());
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        // IronPDF doesn't support PostScript directly
        // Option 1: Keep Ghostscript just for PS conversion, use IronPDF for everything else
        // Option 2: Convert PS to PDF offline, then use IronPDF

        // For most use cases, generate PDFs from HTML instead:
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Content that was in PostScript</h1>");
        pdf.SaveAs("from_html.pdf");
    }
}
```

### Example 7: HTML to PDF (Ghostscript Can't Do This!)

**Before (Ghostscript + wkhtmltopdf):**
```csharp
using System.Diagnostics;

class Program
{
    static void Main()
    {
        // Ghostscript cannot convert HTML to PDF
        // Need external tool like wkhtmltopdf first

        // Step 1: Use wkhtmltopdf (external process)
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "wkhtmltopdf",
                Arguments = "report.html temp_output.pdf",
                UseShellExecute = false,
                RedirectStandardError = true
            }
        };
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine("wkhtmltopdf failed!");
            return;
        }

        // Step 2: Optionally process with Ghostscript
        // (e.g., add compression, encryption)
        // ... more command-line processing ...
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // From HTML file
        var pdf = renderer.RenderHtmlFileAsPdf("report.html");
        pdf.SaveAs("from_file.pdf");

        // From HTML string
        var pdf2 = renderer.RenderHtmlAsPdf("<h1>Dynamic Report</h1><p>Generated content...</p>");
        pdf2.SaveAs("from_string.pdf");

        // From URL
        var pdf3 = renderer.RenderUrlAsPdf("https://www.example.com");
        pdf3.SaveAs("from_url.pdf");
    }
}
```

### Example 8: Batch Processing

**Before (Ghostscript):**
```csharp
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using System.Collections.Generic;
using System.IO;

class Program
{
    static void Main()
    {
        GhostscriptVersionInfo gvi = new GhostscriptVersionInfo("gsdll64.dll");

        string[] pdfFiles = Directory.GetFiles("input", "*.pdf");

        foreach (string file in pdfFiles)
        {
            // Must create new processor for each file (or reuse carefully)
            using (GhostscriptProcessor processor = new GhostscriptProcessor(gvi))
            {
                string outputFile = Path.Combine("output",
                    Path.GetFileNameWithoutExtension(file) + "_compressed.pdf");

                List<string> switches = new List<string>
                {
                    "-dNOPAUSE",
                    "-dBATCH",
                    "-dSAFER",
                    "-sDEVICE=pdfwrite",
                    "-dPDFSETTINGS=/ebook",
                    $"-sOutputFile={outputFile}",
                    file
                };

                try
                {
                    processor.Process(switches.ToArray());
                    Console.WriteLine($"Processed: {file}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed: {file} - {ex.Message}");
                }
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        string[] pdfFiles = Directory.GetFiles("input", "*.pdf");

        // Process in parallel - IronPDF is thread-safe
        await Parallel.ForEachAsync(pdfFiles, async (file, ct) =>
        {
            try
            {
                var pdf = PdfDocument.FromFile(file);
                pdf.CompressImages(75);

                string outputFile = Path.Combine("output",
                    Path.GetFileNameWithoutExtension(file) + "_compressed.pdf");
                pdf.SaveAs(outputFile);

                Console.WriteLine($"Processed: {file}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed: {file} - {ex.Message}");
            }
        });
    }
}
```

### Example 9: Add Text to Existing PDF

**Before (Ghostscript) - Very Complex:**
```csharp
// Ghostscript cannot easily add text to existing PDFs
// Would require:
// 1. Create PostScript file with text overlay
// 2. Merge with original PDF using pdfwrite
// This is extremely complex and error-prone
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("document.pdf");

        // Add text using HTML stamping
        var stamper = new HtmlStamper()
        {
            Html = "<div style='color:red; font-size:20px;'>APPROVED</div>",
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        pdf.ApplyStamp(stamper);
        pdf.SaveAs("with_stamp.pdf");
    }
}
```

### Example 10: PDF/A Conversion

**Before (Ghostscript):**
```csharp
using Ghostscript.NET;
using Ghostscript.NET.Processor;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        GhostscriptVersionInfo gvi = new GhostscriptVersionInfo("gsdll64.dll");

        using (GhostscriptProcessor processor = new GhostscriptProcessor(gvi))
        {
            // PDF/A-1b conversion (complex!)
            List<string> switches = new List<string>
            {
                "-dNOPAUSE",
                "-dBATCH",
                "-dSAFER",
                "-sDEVICE=pdfwrite",
                "-dPDFA=1",
                "-dPDFACompatibilityPolicy=1",
                "-sColorConversionStrategy=UseDeviceIndependentColor",
                "-sProcessColorModel=DeviceCMYK",
                $"-sOutputFile=document_pdfa.pdf",
                "PDFA_def.ps",  // Need this file!
                "document.pdf"
            };

            processor.Process(switches.ToArray());
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        // Generate as PDF/A directly
        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PdfA = true;

        var pdf = renderer.RenderHtmlAsPdf("<h1>PDF/A Compliant Document</h1>");
        pdf.SaveAs("document_pdfa.pdf");
    }
}
```

---

## Advanced Scenarios

### Removing External Process Dependency

If you're calling Ghostscript via `Process.Start()`:

**Before:**
```csharp
var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "gswin64c.exe",
        Arguments = "-dNOPAUSE -dBATCH -sDEVICE=pdfwrite -sOutputFile=output.pdf input.pdf",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    }
};
process.Start();
string stderr = process.StandardError.ReadToEnd();
process.WaitForExit();

if (process.ExitCode != 0)
{
    throw new Exception($"Ghostscript error: {stderr}");
}
```

**After:**
```csharp
using IronPdf;

// No external process, no stderr parsing, no exit codes
var pdf = PdfDocument.FromFile("input.pdf");
pdf.SaveAs("output.pdf");
```

### Handling Ghostscript Custom Workflows

For complex Ghostscript workflows with many switches:

```csharp
// Analyze your Ghostscript switches and map to IronPDF features:

// -dPDFSETTINGS=/ebook → pdf.CompressImages(75)
// -dFirstPage=5 -dLastPage=10 → pdf.CopyPages(4, 5, 6, 7, 8, 9)
// -sOwnerPassword=X → pdf.SecuritySettings.OwnerPassword = "X"
// -sDEVICE=png16m -r300 → pdf.RasterizeToImageFiles("*.png", 300)

// If a switch has no equivalent, consider:
// 1. Is it actually needed? (Many Ghostscript switches have no effect in IronPDF)
// 2. Is there an alternative approach?
// 3. Contact IronPDF support for guidance
```

---

## Performance Considerations

### Process vs Library

| Aspect | Ghostscript (Process) | IronPDF (Library) |
|--------|----------------------|-------------------|
| Startup Overhead | Process creation ~50-200ms | Near zero |
| Memory | Separate process memory | In-process, managed |
| Parallelism | Multiple processes | Thread-safe |
| Error Handling | Parse stderr text | Exceptions |
| Cleanup | Process termination | Dispose() |

### Memory Management

```csharp
// Always dispose PDF documents when done
using var pdf = PdfDocument.FromFile("large.pdf");
// ... work with pdf ...
// Automatic disposal at end of scope

// Or explicit disposal
var pdf2 = PdfDocument.FromFile("another.pdf");
try
{
    // ... work with pdf2 ...
}
finally
{
    pdf2.Dispose();
}
```

### Batch Processing Tips

```csharp
// Reuse renderer for multiple conversions
var renderer = new ChromePdfRenderer();

// Disable features you don't need
renderer.RenderingOptions.EnableJavaScript = false;

foreach (var html in htmlDocuments)
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{count++}.pdf");
}
```

---

## Troubleshooting

### Issue 1: Missing Ghostscript DLL Errors

**Problem:** Code looking for `gsdll64.dll` or `GhostscriptVersionInfo`.

**Solution:** Remove all Ghostscript references:
```csharp
// Remove this:
GhostscriptVersionInfo gvi = new GhostscriptVersionInfo("gsdll64.dll");

// IronPDF doesn't need external DLLs
```

### Issue 2: Command-Line Switches Don't Work

**Problem:** Trying to pass Ghostscript switches to IronPDF.

**Solution:** Use IronPDF's API instead of switches:
```csharp
// Wrong - no switches in IronPDF
// processor.Process(new[] { "-dPDFSETTINGS=/ebook", ... });

// Right - use properties and methods
pdf.CompressImages(75);
```

### Issue 3: PostScript Files

**Problem:** Need to process PostScript (.ps) files.

**Solution:** Options:
1. Keep Ghostscript just for PS→PDF conversion
2. Convert PS files offline before processing
3. Generate content from HTML instead (usually preferable)

### Issue 4: Page Numbering Differences

**Problem:** Page numbering doesn't match.

**Solution:** Ghostscript uses 1-indexed pages; IronPDF uses 0-indexed:
```csharp
// Ghostscript: -dFirstPage=5 (means page 5)
// IronPDF: pdf.CopyPages(4) (index 4 = page 5)
```

### Issue 5: Error Handling Changes

**Problem:** Used to parse stderr for errors.

**Solution:** Use try-catch for exceptions:
```csharp
try
{
    var pdf = PdfDocument.FromFile("document.pdf");
    // ... operations ...
}
catch (IronPdf.Exceptions.PdfException ex)
{
    Console.WriteLine($"PDF Error: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"File not found: {ex.FileName}");
}
```

### Issue 6: Image Quality Differences

**Problem:** Rasterized images look different.

**Solution:** Adjust DPI and check color settings:
```csharp
// Ghostscript default was often 72 DPI
// IronPDF default is higher

// Match Ghostscript 72 DPI output:
pdf.RasterizeToImageFiles("*.png", DPI: 72);

// Or use higher quality:
pdf.RasterizeToImageFiles("*.png", DPI: 300);
```

### Issue 7: AGPL License Concerns

**Problem:** Current Ghostscript use may violate AGPL.

**Solution:** Migrating to IronPDF eliminates this concern—IronPDF has a commercial license without source code disclosure requirements.

### Issue 8: Platform-Specific DLLs

**Problem:** Had different DLLs for 32-bit vs 64-bit.

**Solution:** IronPDF is AnyCPU compatible:
```csharp
// Remove this platform-specific code:
string dllPath = Environment.Is64BitProcess ? "gsdll64.dll" : "gsdll32.dll";

// IronPDF automatically handles platform differences
```

---

## Migration Checklist

```markdown
## Migration Checklist

### Pre-Migration

- [ ] **Inventory all library usages in codebase**
  ```bash
  grep -r "using Ghostscript.NET" --include="*.cs" .
  grep -r "GhostscriptProcessor\|Process" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  ```csharp
  // Find patterns like:
  var settings = new GhostscriptSettings {
      Device = "pdfwrite",
      OutputFile = "output.pdf"
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package Ghostscript.NET
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
  // Before (Ghostscript)
  var processor = new GhostscriptProcessor();
  processor.StartProcessing("input.ps", "output.pdf");

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf("<html><body>Your content here</body></html>");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering for accurate HTML/CSS support.

- [ ] **Convert URL rendering**
  ```csharp
  // Before (Ghostscript with external tool)
  Process.Start("wkhtmltopdf", "https://example.com output.pdf");

  // After (IronPDF)
  var pdf = renderer.RenderUrlAsPdf("https://example.com");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Direct URL rendering with full JavaScript support.

- [ ] **Update page settings**
  ```csharp
  // Before (Ghostscript)
  var settings = new GhostscriptSettings {
      PageSize = "A4",
      Orientation = "Landscape"
  };

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Convert header/footer configuration**
  ```csharp
  // Before (Ghostscript with external tool)
  // Headers and footers might be managed externally

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
      MaxHeight = 25
  };
  renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>{html-title}</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders: {page}, {total-pages}, {date}, {time}, {html-title}.

- [ ] **Enable JavaScript if needed**
  ```csharp
  // Before (Ghostscript with external tool)
  // JavaScript handling might be managed externally

  // After (IronPDF)
  renderer.RenderingOptions.EnableJavaScript = true;
  renderer.RenderingOptions.WaitFor.JavaScript(2000);
  ```
  **Why:** IronPDF's Chromium engine provides reliable JavaScript execution with configurable wait times.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Test JavaScript-heavy pages**
  **Why:** Dynamic content should now render more reliably with modern Chromium.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  // Merge PDFs
  var merged = PdfDocument.Merge(pdf1, pdf2);

  // Password protection
  pdf.SecuritySettings.UserPassword = "secret";

  // Watermarks
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");

  // Digital signatures
  var signature = new PdfSignature("cert.pfx", "password");
  pdf.Sign(signature);
  ```
  **Why:** IronPDF provides many features that may not have been available in the old library.
```
## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Ghostscript usage in codebase**
  ```bash
  grep -r "using Ghostscript.NET" --include="*.cs" .
  grep -r "GhostscriptProcessor\|GhostscriptRasterizer" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current command-line switches used**
  ```csharp
  // Example command-line switches:
  var switches = "-dNOPAUSE -dBATCH -sDEVICE=pdfwrite -sOutputFile=output.pdf";
  ```
  **Why:** These switches map to IronPDF's API calls. Document them now to ensure consistent output after migration.

- [ ] **Identify any PostScript processing (needs special handling)**
  **Why:** PostScript files require specific handling that may not directly map to IronPDF.

- [ ] **Review AGPL license compliance status**
  **Why:** Ensure compliance with Ghostscript's AGPL license to avoid legal issues.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Create migration branch in version control**
  **Why:** Safely make changes without affecting the main codebase.

- [ ] **Set up test environment**
  **Why:** Ensure you have a controlled environment to validate the migration.

### Code Migration

- [ ] **Remove Ghostscript.NET NuGet package**
  ```bash
  dotnet remove package Ghostscript.NET
  ```
  **Why:** Clean removal of the old package to prevent conflicts.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add the new package for PDF operations.

- [ ] **Remove external Ghostscript binary dependencies**
  **Why:** Eliminate the need for external binaries, simplifying deployment.

- [ ] **Remove `GhostscriptVersionInfo` and DLL references**
  ```csharp
  // Before (Ghostscript)
  var versionInfo = new GhostscriptVersionInfo("gsdll32.dll");

  // After (IronPDF)
  // No external DLL references needed
  ```
  **Why:** IronPDF does not require external DLLs, simplifying the setup.

- [ ] **Convert `GhostscriptProcessor.Process()` to IronPDF methods**
  ```csharp
  // Before (Ghostscript)
  processor.StartProcessing(switches, null);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF provides a more straightforward API for PDF generation.

- [ ] **Convert `GhostscriptRasterizer` to `pdf.RasterizeToImageFiles()`**
  ```csharp
  // Before (Ghostscript)
  var rasterizer = new GhostscriptRasterizer();
  rasterizer.Open(pdfPath);

  // After (IronPDF)
  var pdf = PdfDocument.FromFile(pdfPath);
  var images = pdf.RasterizeToImageFiles(300);
  ```
  **Why:** IronPDF simplifies rasterization with built-in methods.

- [ ] **Replace command-line switches with API calls**
  ```csharp
  // Before (Ghostscript)
  var switches = "-dNOPAUSE -dBATCH -sDEVICE=pdfwrite";

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** IronPDF's API is more intuitive and less error-prone than command-line switches.

- [ ] **Update error handling from stderr parsing to exceptions**
  ```csharp
  // Before (Ghostscript)
  if (stderr.Contains("Error")) { /* handle error */ }

  // After (IronPDF)
  try {
      var pdf = renderer.RenderHtmlAsPdf(html);
  } catch (Exception ex) {
      // Handle exception
  }
  ```
  **Why:** Structured exception handling is more reliable and easier to maintain.

- [ ] **Convert 1-indexed page numbers to 0-indexed**
  ```csharp
  // Before (Ghostscript)
  int pageIndex = 1;

  // After (IronPDF)
  int pageIndex = 0;
  ```
  **Why:** IronPDF uses 0-indexed pages, aligning with .NET conventions.

### Feature Mapping

- [ ] **Map merge operations to `PdfDocument.Merge()`**
  ```csharp
  // Before (Ghostscript)
  // Custom merge logic

  // After (IronPDF)
  var mergedPdf = PdfDocument.Merge(pdf1, pdf2);
  ```
  **Why:** IronPDF provides a built-in method for merging PDFs.

- [ ] **Map compression switches to `CompressImages()`**
  ```csharp
  // Before (Ghostscript)
  var switches = "-dPDFSETTINGS=/ebook";

  // After (IronPDF)
  pdf.CompressImages();
  ```
  **Why:** Simplifies image compression with a direct API call.

- [ ] **Map encryption switches to `SecuritySettings`**
  ```csharp
  // Before (Ghostscript)
  var switches = "-sOwnerPassword=secret";

  // After (IronPDF)
  pdf.SecuritySettings.OwnerPassword = "secret";
  ```
  **Why:** IronPDF provides a straightforward way to manage security settings.

- [ ] **Map rasterization to `RasterizeToImageFiles()` or `ToBitmap()`**
  ```csharp
  // Before (Ghostscript)
  var rasterizer = new GhostscriptRasterizer();

  // After (IronPDF)
  var images = pdf.RasterizeToImageFiles(300);
  ```
  **Why:** IronPDF offers flexible rasterization options.

- [ ] **Identify PostScript requirements (if any)**
  **Why:** Ensure any PostScript-specific needs are addressed separately.

### Testing

- [ ] **Test PDF to image conversion**
  **Why:** Verify that rasterization works as expected with IronPDF.

- [ ] **Test PDF merging**
  **Why:** Ensure that PDFs are combined correctly using IronPDF.

- [ ] **Test page extraction**
  **Why:** Validate that page extraction functions as intended.

- [ ] **Test compression quality**
  **Why:** Check that image compression meets quality expectations.

- [ ] **Test password protection**
  **Why:** Confirm that security settings are applied correctly.

- [ ] **Verify output quality matches expectations**
  **Why:** Ensure that the generated PDFs meet the required standards.

- [ ] **Performance benchmark critical paths**
  **Why:** Identify any performance improvements or regressions.

### Deployment

- [ ] **Remove Ghostscript from servers**
  **Why:** Clean up server environments by removing unnecessary dependencies.

- [ ] **Remove PATH configuration**
  **Why:** Simplify server configuration by eliminating PATH modifications.

- [ ] **Remove `gsdll*.dll` files from deployments**
  **Why:** Ensure that no Ghostscript DLLs are included in deployments.

- [ ] **Verify application works without Ghostscript installed**
  **Why:** Confirm that the application functions correctly using only IronPDF.

### Post-Migration

- [ ] **Remove Ghostscript license (if commercial)**
  **Why:** Avoid unnecessary licensing costs by discontinuing Ghostscript use.

- [ ] **Update documentation**
  **Why:** Provide accurate information for future maintenance and development.

- [ ] **Train team on IronPDF API**
  **Why:** Ensure team members are familiar with the new library's capabilities.

- [ ] **Monitor production for any issues**
  **Why:** Quickly identify and resolve any problems that arise post-migration.
```
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [IronPDF Code Examples](https://ironpdf.com/examples/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
