# Migration Guide: Ghostscript → IronPDF

## Why Migrate to IronPDF?

IronPDF offers a native .NET library with a permissive commercial license, eliminating the AGPL licensing concerns of Ghostscript. Unlike Ghostscript's command-line interface requiring complex process management, IronPDF provides a clean, object-oriented API designed specifically for .NET applications. This results in simpler code, better error handling, and improved maintainability without sacrificing PDF processing capabilities.

## NuGet Package Changes

```diff
- Install-Package Ghostscript.NET
+ Install-Package IronPdf
```

## Namespace Mapping

| Ghostscript | IronPDF |
|-------------|---------|
| `Ghostscript.NET` | `IronPdf` |
| `Ghostscript.NET.Rasterizer` | `IronPdf` |
| `Ghostscript.NET.Processor` | `IronPdf` |
| N/A (command-line args) | `IronPdf.ChromePdfRenderer` |

## API Mapping

| Ghostscript Operation | IronPDF Equivalent |
|-----------------------|-------------------|
| `GhostscriptProcessor.Process()` | `PdfDocument.FromFile()` |
| `GhostscriptRasterizer.Open()` | `PdfDocument.RasterizeToImageFiles()` |
| Command-line PDF generation | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `ps2pdf` conversion | `PdfDocument` operations |
| Manual DPI/resolution settings | `RenderingOptions.CreatePdfOptions` |
| Process output parsing | Native exception handling |

## Code Examples

### Example 1: Converting HTML to PDF

**Before (Ghostscript):**
```csharp
using Ghostscript.NET.Processor;
using System.Diagnostics;

// First generate PostScript, then convert to PDF
var htmlToPsProcess = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "wkhtmltopdf",
        Arguments = "input.html output.ps",
        UseShellExecute = false,
        RedirectStandardOutput = true
    }
};
htmlToPsProcess.Start();
htmlToPsProcess.WaitForExit();

// Then use Ghostscript to convert PS to PDF
var processor = new GhostscriptProcessor();
var args = new[]
{
    "-dNOPAUSE", "-dBATCH", "-sDEVICE=pdfwrite",
    "-sOutputFile=output.pdf", "output.ps"
};
processor.Process(args);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlFileAsPdf("input.html");
pdf.SaveAs("output.pdf");
```

### Example 2: Merging PDF Files

**Before (Ghostscript):**
```csharp
using Ghostscript.NET.Processor;

var processor = new GhostscriptProcessor();
var args = new[]
{
    "-dNOPAUSE",
    "-dBATCH",
    "-sDEVICE=pdfwrite",
    "-sOutputFile=merged.pdf",
    "file1.pdf",
    "file2.pdf",
    "file3.pdf"
};

try
{
    processor.Process(args);
}
catch (Exception ex)
{
    // Parse error output from console
    Console.WriteLine($"Ghostscript error: {ex.Message}");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("file1.pdf");
var pdf2 = PdfDocument.FromFile("file2.pdf");
var pdf3 = PdfDocument.FromFile("file3.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("merged.pdf");
```

### Example 3: Converting PDF to Images

**Before (Ghostscript):**
```csharp
using Ghostscript.NET.Rasterizer;

var rasterizer = new GhostscriptRasterizer();
rasterizer.Open("input.pdf");

for (int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
{
    var img = rasterizer.GetPage(300, 300, pageNumber);
    img.Save($"page_{pageNumber}.png", ImageFormat.Png);
}

rasterizer.Close();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");
pdf.RasterizeToImageFiles("page_*.png", 300);

// Or get images directly in memory
var images = pdf.ToBitmap(300);
for (int i = 0; i < images.Count; i++)
{
    images[i].Save($"page_{i + 1}.png");
}
```

## Common Gotchas

| Issue | Solution |
|-------|----------|
| **License Key Required** | IronPDF requires a license key for production use. Set it with `IronPdf.License.LicenseKey = "YOUR-KEY";` at application startup. |
| **Process Dependencies** | No need to install Ghostscript binaries or manage PATH variables—IronPDF is self-contained. |
| **Command-Line Arguments** | Ghostscript command-line flags have no direct equivalent. Use IronPDF's fluent API and `RenderingOptions` instead. |
| **File Locking** | Unlike Ghostscript's process-based approach, IronPDF may keep files open. Always dispose `PdfDocument` objects or use `using` statements. |
| **PostScript Support** | IronPDF doesn't natively support PostScript input. Convert PS files to PDF using other tools first, or generate PDFs directly from HTML. |
| **Error Messages** | Ghostscript writes errors to stderr; IronPDF throws structured .NET exceptions with detailed messages. |
| **Async Operations** | IronPDF supports async/await patterns for better scalability: `await renderer.RenderHtmlAsPdfAsync()`. |
| **Memory Usage** | IronPDF loads PDFs into memory. For large files, use streaming methods like `SaveAs()` instead of keeping entire documents in memory. |

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials & Examples**: https://ironpdf.com/tutorials/
- **API Reference**: https://ironpdf.com/docs/
- **Support**: Contact IronPDF support for migration assistance