# Migrating from PdfiumViewer to IronPDF: no fuss, no fluff

PdfiumViewer worked on-prem for years. The developer machine, the test server, the production Windows VM — all fine. Then the team moved the PDF generation service to a Linux Docker container on Kubernetes and the first deploy failed with a `DllNotFoundException` on `pdfiumviewer.dll`. It's a Windows-targeting library. You're on Linux now. The migration decision practically makes itself.

This article covers the PdfiumViewer to IronPDF migration with a troubleshooting focus on the deployment constraint that most commonly triggers it. The patterns apply if you're running on Linux, in Docker, or in a cloud environment where Windows-only native libraries fail.

---

## Diagnosing the deployment constraint

Before writing migration code, confirm the exact failure mode. Linux/Docker failures with PdfiumViewer are predictable:

**Symptom: `DllNotFoundException` for `pdfium.dll`**

```bash
# Confirm the library is Windows-targeting
docker run --rm your-image dotnet --info | grep OS

# The error will reference pdfium.dll — a Windows PE binary
# This won't load on Linux regardless of configuration
```

**Symptom: `PlatformNotSupportedException` at startup**

```bash
# PdfiumViewer or its dependencies may explicitly check OS
docker run --rm your-image dotnet run 2>&1 | grep -i "platform\|not.*supported\|windows"
```

**Symptom: Process crashes with native exception**

```bash
# Native exception from trying to load a Windows DLL on Linux
docker run --rm your-image dotnet run 2>&1 | grep -i "segfault\|abort\|SIGSEGV\|native"
```

Once confirmed as a Windows-binary dependency issue, there's no configuration path — the library must be replaced for Linux/Docker deployment.

---

## Why migrate (without drama)

Nine reasons this migration makes sense:

1. **Windows-only limitation** — PdfiumViewer targets Windows Forms and native Windows DLLs. Linux, macOS, and container environments can't use it.
2. **Repository archived** — PdfiumViewer appears unmaintained. Verify current status; maintenance has likely stopped.
3. **.NET Framework target** — limited or no .NET 6+ support means incompatibility as the stack modernizes.
4. **PDF viewer vs server-side generator** — PdfiumViewer is designed as a desktop viewer control. Server-side PDF generation is not its primary design target.
5. **HTML-to-PDF not supported** — PdfiumViewer renders existing PDFs to images; it doesn't generate PDFs from HTML.
6. **Native binary management** — Windows-specific `pdfium.dll` must be bundled and maintained separately from NuGet.
7. **No manipulation features** — merge, watermark, password protection require additional libraries.
8. **Cloud deployment incompatible** — Azure App Service on Linux, AWS Lambda, GCP Cloud Run: all fail with Windows-only native libraries.
9. **CI/CD on Linux agents** — build agents running Linux (GitHub Actions ubuntu runners, GitLab CI) can't run PdfiumViewer-dependent tests.

### Comparison table

| Aspect | PdfiumViewer | IronPDF |
|---|---|---|
| Focus | PDF viewer control + page rendering | HTML-to-PDF + PDF manipulation |
| Pricing | Open source (MIT-ish) | Commercial — verify at ironsoftware.com |
| API Style | Windows Forms control / rendering | In-process .NET library |
| Learning Curve | Low for viewing; not designed for server | Medium |
| HTML Rendering | Not supported | Chromium-based |
| Page Indexing | 0-based | 0-based |
| Thread Safety | Windows Forms threading model | Renderer instance reuse |
| Namespace | `PdfiumViewer` | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | PdfiumViewer approach | Effort to migrate |
|---|---|---|
| PDF page to bitmap image | PdfiumViewer native | High — different feature set |
| Display PDF in WinForms UI | PdfiumViewer native | High — N/A in IronPDF |
| HTML to PDF generation | Not supported | Low (native in IronPDF) |
| Open/read existing PDF | Via PdfDocument | Low |
| Page count / metadata | `doc.PageCount` | Low |
| Text extraction | Limited | Medium — verify IronPDF |
| Merge PDFs | Not a feature | Low (native in IronPDF) |
| Watermark | Not a feature | Low |
| Password protection | Not a feature | Low |
| Linux/Docker deployment | Not possible | Resolved |
| Cross-platform .NET 8 | Not supported | Resolved |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| WinForms PDF viewer replacement | IronPDF is not a viewer — evaluate pdf.js, DevExpress, Syncfusion |
| Server-side PDF generation on Linux | IronPDF direct fit |
| PDF-to-image rendering on Linux | Pdfium with Linux binaries (separate), or IronPDF if HTML-to-PDF also needed |
| Both viewing and server-side generation | Split: IronPDF for server; separate viewer component for client |

---

## Before you start

### Prerequisites

- .NET 6+ target with Linux or cross-platform deployment
- Confirm all PdfiumViewer usage is server-side (not WinForms UI)
- NuGet access

### Find PdfiumViewer references in your codebase

```bash
# Find all PdfiumViewer usage
rg -l "PdfiumViewer\|PdfDocument.*Pdfium\|PdfRenderer" --type cs

# Find using statements
rg "using PdfiumViewer" --type cs -n

# Find class instantiation
rg "PdfiumViewer\.PdfDocument\|PdfRenderer\|PdfiumViewer" --type cs -n

# Find native binary references
find . -name "pdfium.dll" 2>/dev/null
rg "pdfium\.dll\|pdfium\.so" . -n 2>/dev/null

# Find NuGet references
grep -r -i "pdfiumviewer" **/*.csproj *.csproj 2>/dev/null
```

### Remove PdfiumViewer, install IronPDF

```bash
# Remove PdfiumViewer
dotnet remove package PdfiumViewer

# Install IronPDF
dotnet add package IronPdf
dotnet restore
```

Remove native binaries from project:

```bash
# Remove pdfium.dll from project directories
find . -name "pdfium.dll" -exec echo "Remove: {}" \;
# After confirming, delete them

# Remove from Docker COPY commands
rg "pdfium\.dll\|PdfiumViewer" Dockerfile* -n
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (PdfiumViewer — no license key, open source):**
```csharp
using PdfiumViewer;
// No license initialization needed — open source library
// Native pdfium.dll must be present in application directory
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR_IRONPDF_LICENSE_KEY";
// License guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
using PdfiumViewer;
using System.Drawing; // for bitmap rendering — Windows Forms dependency
using System.Drawing.Imaging;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
using IronPdf.Security;
```

### Step 3: Basic PDF operation

**Before (PdfiumViewer — page rendering to image):**
```csharp
using PdfiumViewer;
using System;
using System.Drawing;
using System.Drawing.Imaging;

class BasicPdfiumViewerExample
{
    static void Main()
    {
        // PdfiumViewer: open existing PDF and render page to image
        // This is the primary use case — Windows only
        using var document = PdfDocument.Load("input.pdf");

        int pageCount = document.PageCount;
        Console.WriteLine($"Pages: {pageCount}");

        // Render page 0 to bitmap (Windows-only System.Drawing)
        using var image = document.Render(
            page: 0,
            width: 1240,
            height: 1754,
            dpiX: 150,
            dpiY: 150,
            forPrinting: false
        );

        image.Save("page1.png", ImageFormat.Png);
        Console.WriteLine("Rendered page 1 to page1.png");
    }
}
```

**After (IronPDF — HTML-to-PDF; page-to-image is different scope):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

// If replacing server-side PDF generation:
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/

// If you need to open and read an existing PDF:
// var existing = PdfDocument.FromFile("input.pdf");
// Console.WriteLine($"Pages: {existing.PageCount}");
```

---

## Troubleshooting: Linux/Docker-specific failures

### Problem: "IronPDF fails in Docker on Linux"

IronPDF's Chromium renderer requires specific Linux libraries. Verify your Docker base image has them:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# IronPDF Chromium dependencies on Debian/Ubuntu base images
RUN apt-get update && apt-get install -y \
    libglib2.0-0 \
    libnss3 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libx11-6 \
    libxcomposite1 \
    libxdamage1 \
    libxext6 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libxkbcommon0 \
    libpango-1.0-0 \
    libcairo2 \
    --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*

# Verify current IronPDF Docker requirements:
# https://ironpdf.com/how-to/azure/
```

### Problem: "Render succeeds but font is wrong"

```bash
# Check available fonts in container
docker run --rm your-image fc-list | grep -i arial

# Install fonts if missing:
RUN apt-get install -y fonts-liberation fonts-dejavu fontconfig \
    && fc-cache -fv
```

### Problem: "PdfiumViewer PDF-to-image code has no equivalent in IronPDF"

This is a genuine feature gap, not a configuration issue:

```csharp
// PdfiumViewer: render PDF page to bitmap — Windows only
// using var doc = PdfDocument.Load("input.pdf");
// using var image = doc.Render(0, 1240, 1754, 150, 150, false);
// image.Save("page.png");

// IronPDF equivalent: not a primary feature
// For cross-platform PDF-to-image on Linux, alternatives include:
// 1. docnet (open-source, cross-platform Pdfium wrapper):
//    https://github.com/GowenGit/docnet
// 2. Ghostscript command-line:
//    ghostscript -dBATCH -dNOPAUSE -sDEVICE=png16m -r150 input.pdf
// 3. ImageMagick with Ghostscript:
//    convert -density 150 input.pdf output-%03d.png
```

### Problem: "Azure deployment fails with IronPDF"

Azure App Service on Linux has specific configurations for Chromium-based rendering:

```csharp
// In ASP.NET startup for Azure:
IronPdf.License.LicenseKey = "YOUR_KEY";

// Verify Azure-specific setup at:
// https://ironpdf.com/how-to/azure/
```

---

## API mapping tables

### Namespace mapping

| PdfiumViewer | IronPDF | Notes |
|---|---|---|
| `PdfiumViewer` | `IronPdf` | Core |
| `System.Drawing` (WinForms) | N/A | Windows-only dep removed |
| N/A | `IronPdf.Editing` | Manipulation |

### Core class mapping

| PdfiumViewer class | IronPDF class | Description |
|---|---|---|
| `PdfiumViewer.PdfDocument` | `PdfDocument` | PDF document (different API) |
| `PdfRenderer` (WinForms control) | N/A | Viewer control — no equivalent |
| N/A | `ChromePdfRenderer` | HTML-to-PDF generator |
| N/A | `PdfDocument` static | Merge, split, load |

### Document loading methods

| Operation | PdfiumViewer | IronPDF |
|---|---|---|
| Open existing PDF | `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` |
| Open from stream | `PdfDocument.Load(stream, ...)` | `PdfDocument.FromStream(stream)` |
| HTML to PDF | Not supported | `renderer.RenderHtmlAsPdf(html)` |
| URL to PDF | Not supported | `renderer.RenderUrlAsPdf(url)` |

### Page operations

| Operation | PdfiumViewer | IronPDF |
|---|---|---|
| Page count | `doc.PageCount` | `pdf.PageCount` |
| Render page to image | `doc.Render(page, w, h, ...)` | Not primary feature |
| Page size | `doc.PageSizes[n]` | `pdf.Pages[n].Width/Height` |
| Access page | N/A (render-focused) | `pdf.Pages[n]` |

### Merge/split operations

| Operation | PdfiumViewer | IronPDF |
|---|---|---|
| Merge | Not a feature | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | Not a feature | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

### 1. HTML to PDF (PdfiumViewer didn't do this — secondary tool pattern)

**Before (wkhtmltopdf subprocess, common addition alongside PdfiumViewer):**
```csharp
using System;
using System.Diagnostics;
using System.IO;

class HtmlToPdfWithSecondary
{
    static void Main()
    {
        // PdfiumViewer can't generate PDFs from HTML
        // Teams add wkhtmltopdf or another tool for this
        string html    = "<html><body><h1>Invoice</h1></body></html>";
        string tmpHtml = Path.GetTempFileName() + ".html";
        string tmpPdf  = Path.GetTempFileName() + ".pdf";

        File.WriteAllText(tmpHtml, html);

        var psi = new ProcessStartInfo("wkhtmltopdf", $"\"{tmpHtml}\" \"{tmpPdf}\"")
        {
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi)!;
        if (!proc.WaitForExit(30_000)) { proc.Kill(); throw new TimeoutException(); }
        if (proc.ExitCode != 0)
            throw new Exception(proc.StandardError.ReadToEnd());

        byte[] pdfBytes = File.ReadAllBytes(tmpPdf);
        File.Delete(tmpHtml);
        File.Delete(tmpPdf);
        Console.WriteLine($"Generated {pdfBytes.Length} bytes");
    }
}
```

**After (IronPDF — cross-platform, no subprocess):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Invoice</h1><p>Amount: $500</p></body></html>"
);
pdf.SaveAs("invoice.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (PdfiumViewer — not a feature; PDFSharp secondary library):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;

class MergePdfsExample
{
    static void Main()
    {
        // PdfiumViewer has no merge — secondary library needed
        using var output = new PdfDocument();

        foreach (string path in new[] { "section1.pdf", "section2.pdf" })
        {
            using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            foreach (PdfPage page in input.Pages)
                output.AddPage(page);
        }

        output.Save("merged.pdf");
        Console.WriteLine("Merged to: merged.pdf");
    }
}
```

**After (IronPDF native — cross-platform):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var merged = PdfDocument.Merge(
    PdfDocument.FromFile("section1.pdf"),
    PdfDocument.FromFile("section2.pdf")
);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (PdfiumViewer — not a feature; iTextSharp pattern):**
```csharp
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;

class WatermarkExample
{
    static void Main()
    {
        using var reader  = new PdfReader("input.pdf");
        using var fs      = new FileStream("watermarked.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs);

        var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
        for (int i = 1; i <= reader.NumberOfPages; i++)
        {
            var cb = stamper.GetOverContent(i);
            cb.BeginText();
            cb.SetFontAndSize(baseFont, 60);
            cb.SetColorFill(new BaseColor(180, 180, 180));
            cb.ShowTextAligned(Element.ALIGN_CENTER, "DRAFT", 300, 420, 45);
            cb.EndText();
        }
        Console.WriteLine("Watermarked.");
    }
}
```

**After (IronPDF — cross-platform):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf = PdfDocument.FromFile("input.pdf");
var stamper = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronSoftware.Drawing.Color.LightGray,
    FontSize = 60,
    Opacity = 30,
    Rotation = 45,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};
pdf.ApplyStamp(stamper);
pdf.SaveAs("watermarked.pdf");
// Guide: https://ironpdf.com/how-to/custom-watermark/
```

---

### 4. Password protection

**Before (PdfiumViewer — not a feature; secondary library):**
```csharp
using iTextSharp.text.pdf;
using System.IO;
using System.Text;

class SecurityExample
{
    static void Main()
    {
        byte[] userPass  = Encoding.ASCII.GetBytes("viewonly");
        byte[] ownerPass = Encoding.ASCII.GetBytes("admin");

        using var reader  = new PdfReader("input.pdf");
        using var fs      = new FileStream("secured.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs, '\0', false);
        stamper.SetEncryption(
            userPass, ownerPass,
            PdfWriter.ALLOW_PRINTING,
            PdfWriter.ENCRYPTION_AES_128
        );
    }
}
```

**After (IronPDF — cross-platform):**
```csharp
using IronPdf;
using IronPdf.Security;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf = PdfDocument.FromFile("input.pdf");
pdf.SecuritySettings.UserPassword  = "viewonly";
pdf.SecuritySettings.OwnerPassword = "admin";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### WinForms viewer is out of scope

PdfiumViewer as a Windows Forms control (`PdfRenderer`) has no IronPDF equivalent. IronPDF is a server-side generation and manipulation library. If you have a WinForms application that uses PdfiumViewer for display, those view components need a separate replacement:

- **Browser-based PDF viewing:** pdf.js (open source)
- **WinForms PDF viewer:** DevExpress, Syncfusion, Telerik, or commercial alternatives
- **WebView2 + pdf.js:** embedded Chromium in WinForms

### Page indexing

Both PdfiumViewer and IronPDF use 0-based page indexing. Minimal change here, but verify your specific PdfiumViewer wrapper's convention:

```csharp
// PdfiumViewer: doc.Render(page: 0, ...) — 0-based
// IronPDF: pdf.Pages[0] — 0-based
// Consistent between them
```

### `System.Drawing` removal

PdfiumViewer's image rendering returns `System.Drawing.Image` objects — Windows-only. Those types should be removed from your server-side code entirely:

```bash
# Find remaining System.Drawing usage after migration
rg "System\.Drawing\|Bitmap\|ImageFormat" --type cs -n
# Remove any that remain from PdfiumViewer's rendering pipeline
```

---

## Performance considerations

### Chromium on Linux vs Windows

Chromium-based rendering performs similarly across platforms. Run the benchmark scaffold from the Pdfium article against your actual templates in both environments if parity is important.

### Renderer reuse

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

foreach (var html in templates)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{Guid.NewGuid()}.pdf");
}
```

### Docker build optimization

```dockerfile
# Cache apt-get layer separately from app layer
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

RUN apt-get update && apt-get install -y \
    libglib2.0-0 libnss3 libatk1.0-0 libatk-bridge2.0-0 \
    libx11-6 libxcomposite1 libxdamage1 libxext6 libxfixes3 \
    libxrandr2 libgbm1 libxkbcommon0 libpango-1.0-0 libcairo2 \
    fonts-liberation fontconfig \
    --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*
# This layer is cached unless base image changes
```

### Edge cases

- **Alpine Linux:** Alpine uses musl libc; Chromium requires glibc. Verify IronPDF's Alpine support or switch to a glibc-based base image.
- **ARM64:** Verify IronPDF provides ARM64 Chromium binaries for your target architecture.
- **Memory:** Chromium in-process has a higher memory footprint than PdfiumViewer's native Windows rendering. Profile under load before sizing Kubernetes pods.

---

## Migration checklist

### Pre-migration

- [ ] Confirm failure mode: `DllNotFoundException`, `PlatformNotSupportedException`, or similar
- [ ] Inventory all PdfiumViewer usage: `rg "PdfiumViewer" --type cs`
- [ ] Separate viewer (WinForms) usage from server-side usage
- [ ] Identify secondary libraries (wkhtmltopdf, iTextSharp, PDFSharp) added alongside
- [ ] Verify IronPDF .NET 8 + Linux compatibility
- [ ] Confirm commercial license requirements
- [ ] Set up IronPDF trial license in dev environment
- [ ] Plan WinForms viewer replacement separately if applicable

### Code migration

- [ ] Remove `PdfiumViewer` NuGet package
- [ ] Remove `pdfium.dll` native binary from project and Docker
- [ ] Remove secondary libraries (if not needed independently)
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `using PdfiumViewer` with `using IronPdf`
- [ ] Replace `PdfDocument.Load()` with `PdfDocument.FromFile()`
- [ ] Replace HTML-to-PDF secondary tool with `ChromePdfRenderer`
- [ ] Replace merge, watermark, security operations
- [ ] Remove `System.Drawing` dependencies from server-side code
- [ ] Add IronPDF license key to config

### Testing

- [ ] Build succeeds on Linux / .NET 8
- [ ] Docker image builds and starts without native library errors
- [ ] Render each HTML template and compare output
- [ ] Test merge, watermark, security
- [ ] Test in target cloud environment (Azure, AWS, GCP)
- [ ] Load test at expected peak concurrency
- [ ] Verify `fc-list` shows expected fonts in container

### Post-migration

- [ ] Remove `pdfium.dll` from all deployment artifacts
- [ ] Update Docker image to remove Windows-specific steps
- [ ] Update deployment documentation with Linux requirements
- [ ] Monitor memory usage in first week of production traffic

---

## Conclusion

The PdfiumViewer migration is one of the cleaner cases because the constraint is definitive — it's a Windows library and you need Linux. The ambiguity is whether the WinForms viewer component needs a parallel replacement or if it's only server-side code that's moving.

**What would you add to this migration checklist based on your own PdfiumViewer integration?** Specifically interested in teams who had PdfiumViewer in both a WinForms desktop application and a shared server-side library — that split tends to create the most coordination overhead.

---

*Tags: `dotnet`, `csharp`, `pdf`, `migration`*
