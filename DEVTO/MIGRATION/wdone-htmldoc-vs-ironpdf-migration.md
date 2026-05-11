# Moving off HTMLDOC: practical IronPDF migration notes

You're upgrading your stack to .NET 8 and the PDF generation pipeline just became a problem. HTMLDOC is a command-line tool from the early 2000s — it does what it does, but it's not a .NET library, and the shell-out wrapper your team wrote years ago is getting less tenable as the surrounding infrastructure modernises. The ASP.NET app targeting .NET 8, the Docker images, the deployment pipeline — they've all moved forward. The PDF generator hasn't.

This is a practical guide to replacing an HTMLDOC-based pipeline with IronPDF. Even if you end up with a different library, the audit steps and migration pattern here transfer.

---

## Why migrate (without drama)

Eight neutral triggers for this particular migration — check which apply:

1. **.NET version upgrade** — .NET 8+ targets and the HTMLDOC subprocess wrapper starts feeling like technical debt. The architecture mismatch becomes visible.
2. **Modern CSS support** — HTMLDOC's HTML engine predates CSS Grid, Flexbox, and most CSS3. Templates designed with modern HTML/CSS won't render correctly.
3. **Subprocess management overhead** — error handling, process cleanup, stderr capture, timeout management, retry logic for a CLI tool is non-trivial code. In-process rendering eliminates this class of problem.
4. **Docker image complexity** — installing HTMLDOC in container images means managing OS-level package installs, which differ by base image and create maintenance overhead.
5. **Async PDF generation** — spawning synchronous processes fits awkwardly with async/await patterns. An in-process library integrates cleanly.
6. **No PDF manipulation** — HTMLDOC generates PDFs but doesn't merge, split, watermark, or encrypt them. Teams add secondary libraries.
7. **Output quality on complex documents** — HTMLDOC's renderer has known limitations with tables, page breaks, and multi-column layouts.
8. **CI/CD portability** — HTMLDOC binary availability varies by OS and image. A NuGet package is more portable across build environments.

### Comparison table

| Aspect | HTMLDOC | IronPDF |
|---|---|---|
| Focus | HTML-to-PDF/PS via CLI | HTML-to-PDF + full PDF manipulation |
| Pricing | Open source (LGPL) | Commercial license — verify at ironsoftware.com |
| API Style | CLI flags via subprocess | In-process .NET library |
| Learning Curve | Low for basic use; shell-out overhead | Medium — larger API surface |
| HTML Rendering | Custom engine, HTML 4.01 era | Chromium-based |
| Page Indexing | N/A (generation only) | 0-based |
| Thread Safety | Process isolation (naturally parallel) | Renderer instance reuse — see async docs |
| Namespace | N/A (CLI) | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | HTMLDOC approach | Effort to migrate |
|---|---|---|
| HTML string to PDF | Write to temp file, shell out | Low |
| HTML file to PDF | Shell out with file arg | Low |
| URL to PDF | Shell out with URL arg | Low |
| Custom margins | CLI flags `--top`, `--bottom`, etc. | Low |
| Headers / footers | CLI flags `--header`, `--footer` | Medium |
| Table of contents | CLI `--toc` flag | Medium — verify IronPDF equivalent |
| Merge PDFs | Not HTMLDOC feature — external lib | Low (native in IronPDF) |
| Watermark | Not HTMLDOC feature — external lib | Low |
| Password protection | Not HTMLDOC feature — external lib | Low |
| Modern CSS support | Not supported | Low to benefit — automatic |
| Async integration | Subprocess: awkward | Low — native async API |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| Open source + simple HTML only + no modern CSS needed | HTMLDOC may still serve; evaluate output quality |
| Modern CSS templates (Grid, Flexbox, CSS3) | Switch to Chromium-based renderer — IronPDF, PuppeteerSharp, or others |
| Air-gapped with no commercial license approval | HTMLDOC, wkhtmltopdf, or PuppeteerSharp (all open source) |
| .NET 8+ target + Docker + CI/CD portability | NuGet-distributed library eliminates binary management |

---

## Before you start

### Prerequisites

- .NET 6+ target framework
- NuGet access for IronPDF
- Your existing HTML templates for render comparison (critical — HTMLDOC output will differ from Chromium output)
- HTMLDOC binary still installed in dev environment (for side-by-side comparison)

### Find HTMLDOC references in your codebase

```bash
# Find files that shell out to htmldoc
rg -l "htmldoc\|HTMLDOC" --type cs -i

# Find Process.Start calls near PDF generation
rg "Process\.Start" --type cs -n

# Find temp file patterns (HTML written to disk for HTMLDOC)
rg "Path\.GetTempFileName\|GetTempPath" --type cs -n

# Find stdout/stderr capture patterns
rg "StandardOutput\|StandardError" --type cs -n

# Find wrapper class if one exists
rg "class.*Pdf\|class.*Html.*Pdf" --type cs -n -i
```

### Remove HTMLDOC dependency, install IronPDF

HTMLDOC is a binary, not a NuGet package. Removal steps depend on how it's installed:

```bash
# Install IronPDF via NuGet
dotnet add package IronPdf
dotnet restore

# Remove from Docker image (update your Dockerfile):
# Remove lines like:
#   RUN apt-get install -y htmldoc
#   COPY htmldoc /usr/local/bin/

# Remove from CI/CD pipeline:
# Find and remove htmldoc install steps in Azure DevOps / GitHub Actions
rg "htmldoc" .github/ azure-pipelines.yml Dockerfile -l 2>/dev/null
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (HTMLDOC — no license, subprocess call):**
```csharp
using System;
using System.Diagnostics;

// Typical HTMLDOC wrapper pattern
static string GetHtmlDocPath() => "/usr/bin/htmldoc"; // or from config
// No license key — open source CLI tool
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR_IRONPDF_LICENSE_KEY";
// Guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
using System.Diagnostics; // for Process
using System.IO;          // for temp file handling
using System.Text;        // for output capture
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering; // for ChromePdfRenderOptions
```

### Step 3: Basic HTML-to-PDF

**Before (typical HTMLDOC shell-out pattern):**
```csharp
using System;
using System.Diagnostics;
using System.IO;

class HtmlToPdfExample
{
    static byte[] ConvertWithHtmlDoc(string html)
    {
        var tempHtml = Path.GetTempFileName() + ".html";
        var tempPdf  = Path.GetTempFileName() + ".pdf";

        File.WriteAllText(tempHtml, html);

        var psi = new ProcessStartInfo
        {
            FileName = "htmldoc",
            Arguments = $"--webpage -t pdf --outfile \"{tempPdf}\" \"{tempHtml}\"",
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var proc = Process.Start(psi)!;
        proc.WaitForExit(30_000); // 30s timeout

        if (proc.ExitCode != 0)
            throw new Exception($"htmldoc failed: {proc.StandardError.ReadToEnd()}");

        return File.ReadAllBytes(tempPdf);
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API mapping tables

### Namespace mapping

| HTMLDOC | IronPDF | Notes |
|---|---|---|
| CLI tool / `System.Diagnostics.Process` | `IronPdf` | Replaces subprocess pattern |
| Temp file I/O | N/A | HTML passed as string or file path |
| CLI flags | `ChromePdfRenderOptions` properties | Strongly typed options object |

### Core class mapping

| HTMLDOC pattern | IronPDF class | Description |
|---|---|---|
| `Process.Start("htmldoc ...")` | `ChromePdfRenderer` | Render entry point |
| CLI flags | `ChromePdfRenderOptions` | Render configuration |
| `File.ReadAllBytes(output)` | `PdfDocument` | Represents output PDF |
| N/A | `PdfDocument.Merge()` | Merge (not available in HTMLDOC) |

### Document loading methods

| Operation | HTMLDOC | IronPDF |
|---|---|---|
| HTML string | Write to file, pass file arg | `renderer.RenderHtmlAsPdf(html)` |
| HTML file | `--outfile output.pdf input.html` | `renderer.RenderHtmlFileAsPdf(path)` |
| URL | `htmldoc --webpage url` | `renderer.RenderUrlAsPdf(url)` |
| Existing PDF | N/A (not HTMLDOC feature) | `PdfDocument.FromFile(path)` |

### Page operations

| Operation | HTMLDOC | IronPDF |
|---|---|---|
| Paper size | `--size A4` | `ChromePdfRenderOptions.PaperSize` |
| Margins | `--top 1cm --bottom 1cm` | `ChromePdfRenderOptions.Margin*` |
| Orientation | `--landscape` | `ChromePdfRenderOptions.PaperOrientation` |
| DPI | `--jpeg` / `--resolution` | `ChromePdfRenderOptions.DPI` |

### Merge/split operations

| Operation | HTMLDOC | IronPDF |
|---|---|---|
| Merge | External library (not HTMLDOC) | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | External library | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

### 1. HTML to PDF

**Before (HTMLDOC subprocess wrapper — realistic production pattern):**
```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

class HtmlToPdfService
{
    private readonly string _htmlDocPath;

    public HtmlToPdfService(string htmlDocPath = "htmldoc")
        => _htmlDocPath = htmlDocPath;

    public byte[] Render(string html, string margins = "1cm")
    {
        var tempHtml = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.html");
        var tempPdf  = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

        try
        {
            File.WriteAllText(tempHtml, html, System.Text.Encoding.UTF8);

            var args = $"--webpage -t pdf --outfile \"{tempPdf}\" " +
                       $"--top {margins} --bottom {margins} " +
                       $"--left {margins} --right {margins} \"{tempHtml}\"";

            var psi = new ProcessStartInfo(_htmlDocPath, args)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = psi };
            proc.Start();
            if (!proc.WaitForExit(60_000))
            {
                proc.Kill();
                throw new TimeoutException("htmldoc timed out after 60 seconds");
            }

            if (proc.ExitCode != 0)
                throw new InvalidOperationException(
                    $"htmldoc exit {proc.ExitCode}: {proc.StandardError.ReadToEnd()}"
                );

            return File.ReadAllBytes(tempPdf);
        }
        finally
        {
            if (File.Exists(tempHtml)) File.Delete(tempHtml);
            if (File.Exists(tempPdf))  File.Delete(tempPdf);
        }
    }
}
```

**After (IronPDF — replaces the entire service):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.MarginTop    = 10; // mm
renderer.RenderingOptions.MarginBottom = 10;
renderer.RenderingOptions.MarginLeft   = 10;
renderer.RenderingOptions.MarginRight  = 10;

var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Invoice</h1></body></html>");
pdf.SaveAs("invoice.pdf");
// Rendering options: https://ironpdf.com/how-to/rendering-options/
```

---

### 2. Merge PDFs

**Before (not HTMLDOC; secondary library pattern teams typically have alongside it):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.IO;

class MergePdfsExample
{
    static void Main()
    {
        // HTMLDOC has no merge feature
        // Teams add PdfSharp or iTextSharp alongside it
        using var outputDoc = new PdfDocument();

        foreach (string path in new[] { "section1.pdf", "section2.pdf", "section3.pdf" })
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"PDF not found: {path}");

            using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            foreach (PdfPage page in input.Pages)
                outputDoc.AddPage(page);
        }

        outputDoc.Save("merged.pdf");
        Console.WriteLine("Merged to: merged.pdf");
    }
}
```

**After (IronPDF native):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var merged = PdfDocument.Merge(
    PdfDocument.FromFile("section1.pdf"),
    PdfDocument.FromFile("section2.pdf"),
    PdfDocument.FromFile("section3.pdf")
);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (not HTMLDOC; secondary library, iTextSharp pattern):**
```csharp
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;

class WatermarkExample
{
    static void Main()
    {
        // HTMLDOC cannot watermark PDFs
        // Common pattern: generate via HTMLDOC, then stamp via iTextSharp
        using var reader  = new PdfReader("generated.pdf");
        using var fs      = new FileStream("watermarked.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs);

        var font = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);

        for (int page = 1; page <= reader.NumberOfPages; page++)
        {
            var cb = stamper.GetOverContent(page);
            cb.SaveState();
            cb.BeginText();
            cb.SetFontAndSize(font, 60);
            cb.SetColorFill(new BaseColor(200, 200, 200));
            cb.ShowTextAligned(Element.ALIGN_CENTER, "DRAFT", 297, 420, 45);
            cb.EndText();
            cb.RestoreState();
        }

        Console.WriteLine("Watermarked: watermarked.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf = PdfDocument.FromFile("generated.pdf");
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

**Before (not HTMLDOC; secondary library):**
```csharp
using iTextSharp.text.pdf;
using System.IO;
using System.Text;

class SecurityExample
{
    static void Main()
    {
        // HTMLDOC has no PDF encryption
        // Secondary library required
        byte[] userPass  = Encoding.ASCII.GetBytes("viewonly");
        byte[] ownerPass = Encoding.ASCII.GetBytes("adminaccess");

        using var reader  = new PdfReader("generated.pdf");
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

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Security;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential Report</h1>");

pdf.SecuritySettings.UserPassword  = "viewonly";
pdf.SecuritySettings.OwnerPassword = "adminaccess";
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Render output will differ

HTMLDOC uses an HTML 4.01-era rendering engine. IronPDF uses Chromium. The output will not be byte-for-byte identical, and for simple HTML it likely won't be visually identical either. Plan for a visual review of all templates. For most teams this is an improvement, but don't assume — verify.

### Subprocess error handling → exceptions

Your current error handling catches process exit codes and stderr. IronPDF throws exceptions on failure. Replace accordingly:

```csharp
// Replace: if (proc.ExitCode != 0) throw ...
// With:
try
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
catch (IronPdf.Exceptions.IronPdfException ex)
{
    // Log and handle
}
```

### Temp file cleanup — gone

Your HTMLDOC wrapper likely manages temp file creation and cleanup. IronPDF operates in-process — no temp files to manage. Remove that logic.

### Docker image changes

```dockerfile
# Remove from Dockerfile:
# RUN apt-get install -y htmldoc

# IronPDF installs its Chromium binaries via NuGet
# Verify IronPDF Docker setup at: https://ironpdf.com/how-to/azure/
```

---

## Performance considerations

### No process startup overhead

HTMLDOC incurs process spawn overhead per call. IronPDF's in-process renderer avoids this — relevant for high-frequency generation:

```csharp
// Process spawn cost: ~50-200ms per call (system dependent)
// In-process renderer: startup amortized across calls when reusing instance
var renderer = new ChromePdfRenderer(); // instantiate once
for (int i = 0; i < 1000; i++)
{
    using var pdf = renderer.RenderHtmlAsPdf(GetHtml(i));
    pdf.SaveAs($"output_{i}.pdf");
}
```

### Concurrent rendering

HTMLDOC achieves parallelism naturally (separate processes). IronPDF requires separate renderer instances per thread:

```csharp
Parallel.ForEach(htmlItems, new ParallelOptions { MaxDegreeOfParallelism = 4 }, html =>
{
    var renderer = new ChromePdfRenderer(); // one per thread
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"{Guid.NewGuid()}.pdf");
});
```

### Memory footprint

A Chromium-based in-process renderer has a higher baseline memory footprint than spawning a lightweight CLI tool. Profile this in your environment before sizing containers.

---

## Migration checklist

### Pre-migration

- [ ] Find all HTMLDOC shell-out code: `rg "htmldoc\|Process.Start" --type cs -i`
- [ ] Inventory HTML templates for render comparison (critical — output will differ)
- [ ] Identify secondary PDF libraries (PdfSharp, iTextSharp) added to supplement HTMLDOC
- [ ] Check Docker images for HTMLDOC installs
- [ ] Check CI/CD pipelines for HTMLDOC binary install steps
- [ ] Verify IronPDF .NET target framework compatibility
- [ ] Confirm commercial license procurement process
- [ ] Set up IronPDF trial license in dev environment

### Code migration

- [ ] Install `IronPdf` NuGet package
- [ ] Replace subprocess wrapper class with `ChromePdfRenderer`
- [ ] Replace `using System.Diagnostics` (for Process) with `using IronPdf`
- [ ] Replace HTML-to-PDF calls
- [ ] Replace URL-to-PDF calls
- [ ] Replace temp file write/read/delete pattern
- [ ] Replace exit code error handling with try/catch
- [ ] Replace secondary library merge/watermark/security calls with IronPDF natives
- [ ] Add IronPDF license key to config

### Testing

- [ ] Render each HTML template and visually compare HTMLDOC vs IronPDF output
- [ ] Pay close attention to: tables, column layouts, page breaks, fonts
- [ ] Verify CSS3 features now render correctly (Flexbox, Grid, etc.)
- [ ] Test merge, watermark, and security operations
- [ ] Verify password-protected PDFs open correctly with correct/wrong credentials
- [ ] Load test: concurrent rendering at expected peak volume

### Post-migration

- [ ] Remove HTMLDOC from Docker images
- [ ] Remove HTMLDOC binary install from CI/CD pipelines
- [ ] Remove secondary PDF libraries if no longer needed
- [ ] Monitor memory baseline — Chromium renderer has higher footprint
- [ ] Record before/after render times and bundle sizes

---

## One Last Thing

The subprocess-to-in-process architectural shift is the most significant change here, not the API surface. Error handling, logging, retry logic, and Docker image management all change character when you go from a CLI tool to a library.

**What were your before/after bundle size or render time differences?** If your team ran this migration and measured, the numbers would be genuinely useful for others making the same call — drop them in the comments.

---

*Tags: `dotnet`, `csharp`, `pdf`, `migration`*
