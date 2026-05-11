---
title: "PDFium vs IronPDF: the decision guide for .NET teams"
published: false
tags: dotnet, csharp, pdf, comparison
---

A .NET team needs PDF thumbnails for a document portal, reaches for a PDFium-based wrapper, and ships locally in an hour. Then the deployment to a Linux container surfaces a "Unable to load DLL 'pdfium.dll'" — and a few hours later, the team also realises the wrapper renders pages but cannot fill the AcroForm fields the next user story depends on. That gap — rendering engine vs. full PDF toolkit — is the real story when comparing PDFium with IronPDF.

PDFium is Google's open-source C++ PDF library, the same engine that ships inside Chromium and Android, released under BSD-3-Clause. There is no official Google-supplied .NET binding. The .NET ecosystem ships several community wrappers around the native engine, including:

- `PdfiumViewer` (Apache 2.0; archived August 2019, last NuGet release 2.13.0 in November 2017)
- `PdfiumViewer.Updated` (maintained community fork targeting .NET Core / .NET 6)
- `PDFiumCore` (Dtronix, .NET Standard 2.1 P/Invoke bindings)
- `Pdfium.Net.SDK` (Patagames, commercial)

The wrappers differ in API surface, platform support, and maintenance cadence, but they share one trait: PDFium itself is a rendering and parsing engine, not an HTML-to-PDF or document-authoring engine. None of the wrappers can convert HTML to PDF on their own, because PDFium has no HTML parser.

## Understanding IronPDF

IronPDF is a commercial PDF generation and manipulation library built on a Chromium rendering engine. The core use case is [creating PDFs from HTML](https://ironpdf.com/tutorials/html-to-pdf/), URLs, or other content with rendering aligned to Chrome Print Preview. Unlike PDFium wrappers that render existing PDFs to images, IronPDF generates new PDFs from web content (HTML5, CSS3, JavaScript), manipulates existing PDFs (merge, split, forms, signatures, encryption), and extracts content (text, images, metadata). It targets .NET Framework 4.6.2+, .NET Core 3.1+, and modern .NET versions through .NET 10, with cross-platform packaging for Windows, Linux, macOS, Docker, and cloud environments — separate native binary packages are not required.

## Where PDFium wrappers fit, and where they do not

### Product status

- Fragmented ecosystem — multiple wrappers, no single canonical package
- Community maintenance — most wrappers are individual or small-team efforts
- Version drift — wrappers update their bundled PDFium binaries independently, so binaries can be incompatible across packages
- API surface differs between wrappers and across major versions

### Missing capabilities (relative to a full PDF toolkit)

- No HTML-to-PDF generation in PDFium itself
- No URL-to-PDF conversion
- Document-edit operations (merge, split, insert) are limited or absent in free wrappers; Patagames Pdfium.Net.SDK exposes more
- Form filling and AcroForm manipulation are limited or absent in free wrappers
- Digital signatures and encryption are not exposed by the free wrappers
- Text extraction may not be present in viewer-only wrappers

### Technical considerations

- Native binary dependency: requires the appropriate `pdfium.dll` / `libpdfium.so` / `libpdfium.dylib` per RID, which adds deployment steps
- P/Invoke marshaling overhead on frequent native calls
- Manual disposal — forgetting `using` statements can cause native handles to leak
- Platform packaging varies — some wrappers are Windows-only, others ship per-RID native binaries you must wire up
- Different wrappers use different PDFium builds and cannot be mixed in one process

### Support

- Free wrappers rely on GitHub issues and community channels
- Documentation depth varies — some wrappers have minimal published docs and rely on IntelliSense / source
- Bug-fix cadence depends on maintainer availability
- Patagames Pdfium.Net.SDK is the commercial option in the family and offers paid support tiers

### Architecture

- Tight coupling to native binaries can complicate containerized and serverless deployments unless RID-specific layouts are explicit
- Async APIs vary by wrapper; several expose synchronous-only operations
- Configurability is typically narrow — wrappers tend to expose a small subset of PDFium's configuration surface

## Feature Comparison Overview

| Aspect | PDFium wrappers | IronPDF |
|--------|----------------|---------|
| **Current status** | Fragmented, community-maintained (free); commercial Patagames option | Active, commercial, single product |
| **HTML support** | None (PDFium has no HTML parser) | HTML5 / CSS3 / JS via Chromium |
| **Rendering quality** | Suited to rendering existing PDFs | Chromium-based output for generated PDFs |
| **Installation** | Wrapper package + per-RID native binaries | Single NuGet, multi-target |
| **Support** | Community for free wrappers; paid for Patagames | Commercial support / SLA options |
| **Future viability** | Depends on individual maintainers / vendor | .NET 10-ready, cross-platform |

---

## Code Comparison

### PDFium (PdfiumViewer) — Render PDF pages to images

```csharp
// NuGet: PdfiumViewer  (representative open-source PDFium wrapper)
using PdfiumViewer;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class Program
{
    static void Main()
    {
        // Ensure the PDFium native binary for your RID is on the load path
        try
        {
            using (var document = PdfDocument.Load("existing-report.pdf"))
            {
                for (int i = 0; i < document.PageCount; i++)
                {
                    // PdfiumViewer exposes per-page sizes via document.PageSizes
                    var size = document.PageSizes[i];
                    int width = (int)(size.Width * 2);   // 2x scale
                    int height = (int)(size.Height * 2);

                    using (var bitmap = document.Render(i, width, height, 96, 96, PdfRenderFlags.Annotations))
                    {
                        bitmap.Save($"page-{i + 1}.png", ImageFormat.Png);
                    }
                }
            }

            Console.WriteLine("PDF rendered to images successfully.");
        }
        catch (DllNotFoundException ex)
        {
            // pdfium.dll not found in output directory
            Console.WriteLine($"ERROR: {ex.Message}");
        }
        catch (BadImageFormatException ex)
        {
            // x86 / x64 mismatch between process and native binary
            Console.WriteLine($"ERROR: {ex.Message}");
        }
    }
}
```

Notes on this approach:

- PDFium is a rendering / parsing engine; generating a PDF from HTML requires a different library
- The native binary must match the process architecture and RID
- Pages are not first-class objects in PdfiumViewer — per-page size lives on `document.PageSizes[i]`
- Sequential per-page calls — batching across pages is the caller's responsibility

### IronPDF — Generate PDF from HTML

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var htmlContent = @"
<html>
<head>
    <style>
        body { font-family: Arial; margin: 40px; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ddd; padding: 8px; }
    </style>
</head>
<body>
    <h1>Monthly Report</h1>
    <table>
        <tr><th>Metric</th><th>Value</th></tr>
        <tr><td>Revenue</td><td>$125,000</td></tr>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("monthly-report.pdf");
```

The [HTML to PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) walks through modern web content rendering (CSS Grid, Flexbox, JavaScript) without manual layout calculation. Platform-specific binaries are bundled by IronPdf and resolved at runtime.

---

### PDFium (PdfiumViewer) — Extract page count and text

```csharp
// NuGet: PdfiumViewer
using PdfiumViewer;
using System;
using System.Text;

class Program
{
    static void Main()
    {
        try
        {
            using (var document = PdfDocument.Load("contract.pdf"))
            {
                int pageCount = document.PageCount;
                Console.WriteLine($"Document has {pageCount} pages.");

                // PdfiumViewer's text extraction is per-page raw text via GetPdfText(int).
                // Layout/format metadata is not exposed.
                var text = new StringBuilder();
                for (int i = 0; i < pageCount; i++)
                {
                    text.AppendLine(document.GetPdfText(i));
                }
                Console.WriteLine(text.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PDF error: {ex.Message}");
        }
    }
}
```

Notes on this approach:

- Text extraction is per-page raw text via `GetPdfText(i)`; layout / format information is minimal
- AcroForm field read/write is not exposed by the open-source wrappers
- Document-info access and modification depend on the wrapper; not all free wrappers expose metadata writes
- Digital signature validation is not part of the free-wrapper surface

### IronPDF — Extract text and metadata

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("contract.pdf");

// Extract all text
string allText = pdf.ExtractAllText();

// Read metadata
var metadata = pdf.MetaData;
Console.WriteLine($"Title: {metadata.Title}");
Console.WriteLine($"Pages: {pdf.PageCount}");

// Modify metadata
metadata.Author = "Legal Team";
metadata.Subject = "Updated Contract";
pdf.SaveAs("contract-updated.pdf");
```

IronPDF provides read/write access to metadata, form fields, and content extraction. The [rendering options documentation](https://ironpdf.com/how-to/rendering-options/) covers watermarking, headers/footers, and custom page sizes.

---

### PDFium (PDFiumCore) — Render pages in parallel

```csharp
// NuGet: PDFiumCore  (Dtronix, .NET Standard 2.1 P/Invoke bindings)
// The exact API surface varies between PDFiumCore releases — verify method names
// against your installed version.
using PDFiumCore;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        try
        {
            // PDFiumCore uses .NET Standard 2.1 — target .NET Core 3.1+ or .NET 5+.
            using (var document = new PdfDocument("large-manual.pdf"))
            {
                int pageCount = document.PageCount;
                var renderTasks = new Task[pageCount];

                for (int i = 0; i < pageCount; i++)
                {
                    int pageIndex = i;  // Capture for closure
                    renderTasks[i] = Task.Run(() =>
                    {
                        using (var page = document.GetPage(pageIndex))
                        {
                            // Page width/height are in points (1/72 inch).
                            double widthPts  = page.Width;
                            double heightPts = page.Height;

                            // Convert to pixels at 150 DPI.
                            int pixelWidth  = (int)(widthPts  * 150.0 / 72.0);
                            int pixelHeight = (int)(heightPts * 150.0 / 72.0);

                            using (var bitmap = new PDFiumBitmap(pixelWidth, pixelHeight, false))
                            {
                                page.Render(bitmap);
                                // Bitmap conversion / save APIs vary by version.
                            }
                        }
                    });
                }

                await Task.WhenAll(renderTasks);
                Console.WriteLine($"Rendered {pageCount} pages.");
            }
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
        }
    }
}
```

Notes on this approach:

- Manual conversion from points to pixels at the target DPI
- Type conversion between `PDFiumBitmap` and `System.Drawing.Bitmap` adds a copy
- Thread safety of `PdfDocument` instances depends on the wrapper — verify against your version's documentation
- Output is rasterized images; rendered images cannot be re-inserted into the source PDF without another library

### IronPDF — Generate multi-page PDF from an HTML file

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

// Render HTML file; page breaks come from CSS print media rules.
var pdf = await Task.Run(() =>
    renderer.RenderHtmlFileAsPdf("product-catalog.html"));

// Add page numbers
pdf.AddPageNumbers("Page {page} of {total-pages}");

pdf.SaveAs("catalog-with-page-numbers.pdf");
```

The [HTML file to PDF guide](https://ironpdf.com/how-to/html-file-to-pdf/) covers external CSS, JavaScript execution timing, and responsive layouts.

---

### PDFium (generic wrapper) — native binary placement

```csharp
using System;
using System.IO;
using System.Reflection;

class Program
{
    static void Main()
    {
        // Most PDFium wrappers load pdfium.dll / libpdfium.so / libpdfium.dylib
        // from a runtimes/<rid>/native folder or from the application directory.
        // Container / cloud deploys typically need the runtimes folder included.

        bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        bool is64Bit = Environment.Is64BitProcess;

        string dllName = isWindows
            ? (is64Bit ? "pdfium_x64.dll" : "pdfium_x86.dll")
            : "libpdfium.so";

        string dllPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "runtimes",
            isWindows ? "win" : "linux",
            dllName);

        if (!File.Exists(dllPath))
        {
            // Without the expected native binary the wrapper cannot initialise.
            Console.WriteLine($"PDFium native binary not found at {dllPath}");
            return;
        }

        Console.WriteLine($"PDFium native binary located at {dllPath}");
    }
}
```

Notes on this approach:

- Native binary discovery is the application's responsibility per RID
- Missing or misplaced binaries surface as `DllNotFoundException` at first use
- Containerized builds need the `runtimes/` folder explicitly included in the publish output
- There is no in-process fallback if the native binary fails to load

### IronPDF — single-package deployment

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Deployed Successfully</h1>");
pdf.SaveAs("deployment-test.pdf");
```

IronPDF bundles platform-specific binaries inside its NuGet package and resolves them at runtime. The same package is used on Windows, Linux, macOS, Docker, Azure Functions, and AWS Lambda; whether each scenario is a fit for your project should still be validated against your environment.

---

## API Mapping Reference

| PDFium wrapper concept | IronPDF equivalent | Notes |
|------------------------|--------------------|-------|
| `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `PdfDocument.Load(stream)` | `PdfDocument.FromStream(stream)` | Load from stream |
| _(wrap bytes in MemoryStream)_ | `PdfDocument.FromBinaryData(bytes)` | Load from bytes |
| `document.Render(page, w, h, dpi, dpi, flags)` | `pdf.RasterizeToImageFiles(path, DPI)` | Rasterize pages |
| Native DLL management | Bundled in IronPdf NuGet | Embedded Chromium / per-RID payload |
| `document.PageCount` | `pdf.PageCount` | Same |
| `document.PageSizes[i].Width/Height` | `pdf.Pages[i].Width/Height` | Per-page size in points |
| `document.GetPdfText(i)` | `pdf.Pages[i].Text` / `pdf.ExtractAllText()` | Per-page or whole-document |
| `document.Save(stream)` | `pdf.SaveAs(path)` / `pdf.Stream` / `pdf.BinaryData` | Save variants |
| _(not available in PDFium)_ | `ChromePdfRenderer.RenderHtmlAsPdf` | Generate from HTML |
| _(not available in PDFium)_ | `ChromePdfRenderer.RenderUrlAsPdf` | Generate from URL |
| _(not available in free wrappers)_ | `PdfDocument.Merge` | Combine PDFs |
| _(not available in free wrappers)_ | `pdf.Form` | AcroForm read/write |
| _(not available in free wrappers)_ | `pdf.SignWithDigitalSignature` | Digital signatures |
| _(not available in free wrappers)_ | `pdf.SecuritySettings` | Password / permissions |

---

## Comprehensive Feature Comparison

| Feature | PDFium wrappers | IronPDF |
|---------|----------------|---------|
| **Status** | | |
| Maintenance | Community-maintained (free); commercial Patagames | Commercial, active |
| Breaking changes | Vary by wrapper / version | Versioned releases with migration notes |
| Platform support | Varies by wrapper | Cross-platform NuGet |
| **Support** | | |
| Documentation | Varies by wrapper | Documentation + API reference |
| Channels | GitHub issues (free); paid tiers for Patagames | Commercial support options |
| Enterprise SLA | Available via Patagames | Available |
| **Content creation** | | |
| HTML to PDF | None | Yes |
| URL to PDF | None | Yes |
| Generate from scratch | None | Yes |
| Merge PDFs | Free wrappers: no; Patagames: partial | Yes |
| Split PDFs | Free wrappers: no; Patagames: partial | Yes |
| **PDF operations** | | |
| Render to bitmap | Yes (core capability) | Yes (via rasterization) |
| Extract text | Per page (free wrappers) | Full document or per page |
| Extract images | SDK-dependent | Yes |
| Form filling | Free wrappers: no; Patagames: yes | Full AcroForm support |
| Digital signatures | Free wrappers: no; Patagames: yes | Yes (X.509) |
| Encryption | Free wrappers: no; Patagames: yes | Yes (AES 128 / 256) |
| Watermarks | Not built-in | Yes |
| Headers/footers | Not built-in | Yes |
| **Security** | | |
| Password protection | Free wrappers: read-only; Patagames: read/write | Read and write |
| Certificate signatures | Free wrappers: no | Yes |
| Redaction | Not exposed | Yes |
| **Operational** | | |
| Native binary deployment | Required (per RID) | Bundled in NuGet |
| x86 / x64 conflicts | Possible if process / binary mismatch | N/A |
| Thread safety | Verify per wrapper | Documented |
| Memory management | Manual disposal of native handles | Managed |
| Docker support | Manual native binary placement | Built-in |
| **Development** | | |
| Async / await | Wrapper-dependent | Async APIs |
| Dependency injection | Wrapper-dependent | Supported |
| Unit testing | Native dependency may complicate mocking | Standard mocking works |
| NuGet packages | Wrapper + per-RID native | Single, multi-target |

---

## Installation Comparison

**PDFium (PdfiumViewer example):**

```powershell
Install-Package PdfiumViewer
# On Linux, obtain libpdfium.so (e.g. from pdfium-binaries) and place under
# runtimes/linux-x64/native/ in the publish output.
```

```csharp
using PdfiumViewer;
// Ensure the native pdfium binary for your RID is on the load path.
```

**IronPDF:**

```powershell
Install-Package IronPdf  # Single package, multi-target
```

```csharp
using IronPdf;
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// No additional configuration required.
```

---

## Conclusion

PDFium wrappers fit a specific niche: rendering existing PDFs to bitmaps for display in viewer controls or thumbnail generation, plus per-page raw text extraction. For teams building custom PDF viewers in WinForms, WPF, or desktop apps — and willing to manage native binaries per RID — wrappers like PdfiumViewer, PdfiumViewer.Updated, PDFiumCore, or the commercial Pdfium.Net.SDK can be a good fit.

Migration to a fuller toolkit tends to make sense when:

- HTML-to-PDF generation is required (reports, invoices, dynamic content)
- PDF manipulation is needed beyond viewing (merge, split, forms, signatures)
- Cross-platform deployment is on the roadmap (Docker, Linux, macOS) and per-RID native binary management is unwelcome
- Commercial support and SLA are business requirements

IronPDF complements PDFium's strength in rendering with comprehensive PDF generation from HTML, URLs, and other sources through a [Chromium rendering engine](https://ironpdf.com/tutorials/html-to-pdf/). The library covers document manipulation (merge, split, forms, encryption, signatures) and text / image extraction, and packages platform-specific binaries inside a single NuGet so consuming projects do not need to place native binaries by hand.

Have you hit "Unable to load DLL 'pdfium.dll'" errors in production? Share your PDFium wrapper troubleshooting stories in the comments.

**Related resources:**

- [IronPDF Rendering Options Guide](https://ironpdf.com/how-to/rendering-options/)
- [HTML to PDF Tutorial](https://ironpdf.com/tutorials/html-to-pdf/)
