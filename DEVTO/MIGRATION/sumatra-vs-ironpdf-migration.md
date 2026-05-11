---
title: "Migrating from Sumatra PDF to IronPDF: no fuss, no fluff"
published: false
tags: dotnet, csharp, pdf, migration
---

The on-premises deployment works fine. Sumatra PDF is on the file path, the process invocation is stable, and the PDFs print correctly. Then the team moves toward containers — and Sumatra PDF is a Windows GUI application. There is no Linux binary, no Docker image, no server mode. A process that shells out to a `.exe` on a Windows path simply does not exist in a Linux container, and the deployment model collapses entirely.

This article covers migrating workflows that rely on Sumatra PDF (typically for rendering or printing) to IronPDF for PDF generation. The troubleshooting section covers the deployment patterns that commonly break, and the checklist applies whether or not you choose IronPDF as the replacement.

---

## Why Migrate (Without Drama)

Teams moving from Sumatra PDF to IronPDF typically have workflows that need to run outside Windows:

1. **Windows-only constraint** — Sumatra PDF has no Linux binary; Docker on Linux is impossible with this dependency.
2. **Process invocation overhead** — each print or render operation spawns a new process, adding latency and requiring error-handling around exit codes.
3. **Binary management** — bundling `SumatraPDF.exe` with the application adds to the deployment artifact and requires explicit version management.
4. **Printing vs generation gap** — Sumatra PDF prints existing PDFs; it doesn't generate PDFs from HTML. HTML-to-PDF requires a separate pipeline stage.
5. **Cloud deployment** — Azure App Service, AWS Lambda, and similar platforms don't have Windows printer contexts; Sumatra PDF's printing use case doesn't apply.
6. **Silent failure modes** — Sumatra PDF CLI errors surface via process exit codes; error messages go to stderr and require explicit capture.
7. **No PDF manipulation API** — Sumatra PDF doesn't expose merge, watermark, security, or text extraction via its CLI.
8. **Version pinning** — application behavior depends on the bundled binary version; updates require explicit deployment of a new binary.
9. **Licensing** — Sumatra PDF is AGPLv3 (with some BSD-licensed files); the strong copyleft has implications for closed-source applications that bundle or link to it.
10. **Audit trail** — shelling out to a binary makes rendering auditing harder than in-process library calls.

### Comparison Table

| Aspect | Sumatra PDF | IronPDF |
|---|---|---|
| Focus | PDF viewer / printer / image renderer | PDF generation + manipulation |
| Pricing | Open source (AGPLv3 — strong copyleft) | Commercial license |
| API Style | CLI invocation via `Process.Start()` | Native .NET library; no external process |
| Learning Curve | Low for basic printing; CLI flags for options | Low for .NET devs |
| HTML Rendering | Not applicable — renders existing PDFs | Embedded Chromium |
| Page Indexing | 1-based in CLI flags | 0-based |
| Thread Safety | Process-level isolation | In-process; use one renderer per thread for parallel workloads |
| Namespace | None — CLI binary | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Sumatra PDF | IronPDF Equivalent | Complexity |
|---|---|---|---|
| Print PDF to printer | `SumatraPDF.exe -print-to PrinterName` | `pdf.Print(300, 300, "PrinterName")` | Low |
| Print to default printer | `-print-to-default` | `pdf.Print()` | Low |
| Render PDF page to image | External tool | `pdf.RasterizeToImageFiles()` | Medium |
| Generate PDF from HTML | Not applicable | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low (new capability) |
| Generate PDF from URL | Not applicable | `renderer.RenderUrlAsPdfAsync()` | Low (new capability) |
| Merge PDFs | Not applicable | `PdfDocument.Merge()` | Medium (new capability) |
| Watermark | Not applicable | `pdf.ApplyWatermark(html)` | Medium |
| Password protection | Not applicable | `pdf.SecuritySettings` | Low |
| Cross-platform | Windows only | Linux/macOS/Windows | Low (inherit) |
| Process spawn overhead | Per operation | In-process | Low (eliminate) |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Printing PDFs to physical printers on Windows | IronPDF exposes `pdf.Print()` and `pdf.Print(dpiX, dpiY, "PrinterName")` (Windows) |
| Generating PDFs from HTML/URLs (new requirement) | Switch — IronPDF is designed for this |
| Moving to Linux/Docker containers | Switch — eliminates Windows-only binary dependency |
| Rendering PDF pages to images | IronPDF's `RasterizeToImageFiles()` covers this in-process |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)
- Clarification: is raw printing still needed, or has the requirement shifted to PDF file generation?

### Find All Sumatra PDF References

```bash
# Find SumatraPDF process invocations
rg -l "SumatraPDF\|sumatrapdf\|SumatraPDF\.exe" --type cs
rg "SumatraPDF\|sumatrapdf" --type cs -n

# Find Process.Start() calls that may invoke Sumatra
rg "Process\.Start.*[Ss]umatra\|StartInfo.*[Ss]umatra" --type cs -n

# Find binary references in deployment scripts
grep -r "SumatraPDF\|sumatra" Dockerfile* .github/**/*.yml deploy/**/*.ps1 2>/dev/null

# Find bundled binary in project
find . -name "SumatraPDF.exe" -o -name "sumatrapdf.exe" 2>/dev/null
```

### Uninstall / Install

```bash
# No NuGet package to remove — Sumatra PDF is a bundled binary
# Remove the binary from the project:
# rm -f Tools/SumatraPDF.exe
# rm -f wwwroot/SumatraPDF.exe  # if web-accessible (not recommended)

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

Remove from COPY steps in Dockerfile:

```dockerfile
# Remove:
# COPY Tools/SumatraPDF.exe /app/Tools/
# ENV SUMATRA_PATH=/app/Tools/SumatraPDF.exe

# IronPDF — no binary copy step needed
```

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

```csharp
using IronPdf;

// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
    ?? throw new InvalidOperationException("IRONPDF_LICENSE_KEY not set");
```

### Step 2 — Namespace Import

**Before (no namespace — just Process.Start):**
```csharp
using System.Diagnostics;
using System.IO;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic PDF Generation

**Before (Sumatra PDF printing existing PDF via CLI):**
```csharp
using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main()
    {
        // Sumatra PDF: print an existing PDF to a named printer
        // This requires a pre-generated PDF — Sumatra doesn't generate from HTML

        var pdfPath = "existing-document.pdf";
        var printerName = "HP LaserJet Office";

        var psi = new ProcessStartInfo
        {
            FileName = @"C:\Tools\SumatraPDF.exe",   // must be on this path
            Arguments = $"-print-to \"{printerName}\" \"{pdfPath}\"",
            UseShellExecute = false,
            RedirectStandardError = true,
        };

        using var process = Process.Start(psi)!;
        process.WaitForExit();
        if (process.ExitCode != 0)
            Console.Error.WriteLine("SumatraPDF error: " + process.StandardError.ReadToEnd());
    }
}
```

**After (IronPDF generates PDF; print separately if needed):**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Generate the PDF — IronPDF replaces the generation step (if any) upstream of Sumatra
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Document</h1><p>Content.</p></body></html>"
);
pdf.SaveAs("generated.pdf");

Console.WriteLine($"Generated generated.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/

// If raw printing to a physical printer is still needed on Windows,
// IronPDF exposes pdf.Print() (default printer) and
// pdf.Print(dpiX, dpiY, "PrinterName") for a named printer.
```

---

## API Mapping Tables

### Namespace Mapping

| Sumatra PDF | IronPDF | Notes |
|---|---|---|
| None — `System.Diagnostics` for Process.Start | `IronPdf` | Core namespace |
| None | `IronPdf.Rendering` | Rendering config |

### Core Class Mapping

| Sumatra PDF Pattern | IronPDF Class | Description |
|---|---|---|
| `SumatraPDF.exe -print-to` | N/A — different purpose | IronPDF generates PDFs; use OS print API to print |
| `Process.Start()` wrapper | `ChromePdfRenderer` | In-process rendering; no binary invocation |
| Exit code error handling | try/catch `IronPdfException` | Exception-based errors |
| N/A | `PdfDocument` | PDF object for manipulation |

### Document Loading Methods

| Operation | Sumatra PDF | IronPDF |
|---|---|---|
| Generate from HTML | Not applicable | `renderer.RenderHtmlAsPdfAsync(html)` |
| Render from URL | Not applicable | `renderer.RenderUrlAsPdfAsync(url)` |
| Load existing PDF | Via file path to CLI | `PdfDocument.FromFile(path)` |
| Save to file | Output via CLI | `pdf.SaveAs(path)` |

### Page Operations

| Operation | Sumatra PDF | IronPDF |
|---|---|---|
| Page count | Not exposed via CLI | `pdf.PageCount` |
| Remove page | Not applicable | `pdf.RemovePages(index)` |
| Extract text | Not applicable | `pdf.ExtractAllText()` |
| Rotate | Via print settings flag | `pdf.RotateAllPages(PageRotation.Clockwise90)` |

### Merge / Split Operations

| Operation | Sumatra PDF | IronPDF |
|---|---|---|
| Merge | Not applicable | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not applicable | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF (Sumatra gap → new IronPDF capability)

**Before (Sumatra PDF — requires pre-generated PDF; HTML generation was done separately):**
```csharp
using System;
using System.Diagnostics;
using System.IO;

class HtmlToPdfBefore
{
    static void Main()
    {
        // Sumatra PDF cannot generate from HTML.
        // A typical pattern before this migration:
        // 1. Generate HTML string from data
        // 2. Use another tool to convert HTML → PDF (wkhtmltopdf, Chrome CLI, etc.)
        // 3. Pass the resulting PDF to Sumatra for printing

        var html = "<html><body><h1>Invoice #1042</h1><p>Total: $2,800</p></body></html>";

        // Step 2: Write HTML to temp file and invoke another tool (illustrative)
        var tmpHtml = Path.GetTempFileName() + ".html";
        File.WriteAllText(tmpHtml, html);

        // This needed a SECOND binary alongside Sumatra PDF
        var psi = new ProcessStartInfo
        {
            FileName = @"C:\Tools\wkhtmltopdf.exe",   // or Chrome, Prince, etc.
            Arguments = $"\"{tmpHtml}\" invoice.pdf",
            UseShellExecute = false,
        };
        using var proc = Process.Start(psi)!;
        proc.WaitForExit();
        File.Delete(tmpHtml);

        Console.WriteLine("Needed TWO external binaries for HTML→PDF→print workflow");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Single in-process step replaces the two-binary pipeline
var html = @"
    <html>
    <head>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; }
        h1 { font-size: 22px; }
        .amount { font-weight: bold; font-size: 18px; margin-top: 20px; }
    </style>
    </head>
    <body>
        <h1>Invoice #1042</h1>
        <p>Date: 2024-09-30 | Customer: Acme Corp</p>
        <div class='amount'>Total Due: $2,800.00</div>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("invoice.pdf");

Console.WriteLine($"Generated invoice.pdf ({pdf.PageCount} page(s)) — in-process, no external binary");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (Sumatra PDF — not applicable):**
```csharp
using System;
// Sumatra PDF has no merge capability.
// PDF merge was done before Sumatra received the file, using a secondary library.

class MergeBefore
{
    static void Main()
    {
        // Pre-Sumatra pipeline (illustrative):
        // 1. Generate PDF A from some source
        // 2. Generate PDF B from another source
        // 3. Merge via secondary library (iTextSharp, PDFSharp, etc.)
        // 4. Pass merged PDF to SumatraPDF.exe for printing

        Console.WriteLine("Merge was done before Sumatra — separate library required");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();

var results = await Task.WhenAll(
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Part A: Summary</h1></body></html>"),
    renderer.RenderHtmlAsPdfAsync("<html><body><h1>Part B: Detail</h1></body></html>")
);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("merged-document.pdf");

Console.WriteLine($"Merged: {merged.PageCount} pages — no external binary required");
```

---

### 3. Watermark

**Before (Sumatra PDF — not applicable):**
```csharp
using System;
// Sumatra PDF cannot add watermarks.
// Watermarks had to be embedded in the source PDF before passing to Sumatra.
// Typically required a secondary library.

class WatermarkBefore
{
    static void Main()
    {
        // Pre-Sumatra: secondary library applied watermark to PDF bytes
        // var watermarked = SomePdfLib.AddTextWatermark(pdfBytes, "DRAFT");
        // File.WriteAllBytes("watermarked.pdf", watermarked);
        // Then SumatraPDF.exe printed watermarked.pdf

        Console.WriteLine("Watermark required separate library before Sumatra print step");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Document</h1></body></html>"
);

// https://ironpdf.com/how-to/custom-watermark/
pdf.ApplyWatermark(@"
    <div style='
        font-size: 60pt;
        color: rgba(128, 128, 128, 0.3);
        transform: rotate(-45deg);
        text-align: center;
    '>
        DRAFT
    </div>");

pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (Sumatra PDF — not applicable for generation):**
```csharp
using System;
// Sumatra PDF can open password-protected PDFs (with password prompt)
// but cannot set passwords on PDFs.
// Password protection had to be applied before Sumatra received the file.

class PasswordBefore
{
    static void Main()
    {
        // Pre-Sumatra: secondary library encrypted the PDF
        // var secured = SomePdfLib.SetPassword(pdfBytes, "open123", "admin456");
        // File.WriteAllBytes("secured.pdf", secured);
        // Sumatra then received secured.pdf for display/printing

        Console.WriteLine("Password protection required a secondary library before Sumatra");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Confidential Document</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("secured.pdf");
Console.WriteLine("Saved secured.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Troubleshooting Common Migration Issues

### "Process.Start paths need updating throughout the codebase"

**Symptom:** After removing the Sumatra invocation pattern, many references to binary paths remain scattered across the codebase.

**Resolution:**
```bash
# Find all Sumatra binary path references
rg "SumatraPDF\.exe\|SumatraPDF\b\|sumatra" --type cs -n

# Find Process.Start patterns that invoked Sumatra
rg "Process\.Start.*sumatra\|ProcessStartInfo.*sumatra" --type cs -in

# Find environment variables pointing to the binary
grep -r "SUMATRA\|SumatraPDF" .env* appsettings*.json *.yml 2>/dev/null
```

If Sumatra binary paths were in `appsettings.json` or environment variables, clean those up once all invocation sites are migrated.

### "The workflow needed Sumatra for printing, not generation — what replaces it?"

**Symptom:** The migration rationale assumed a generation use case, but the actual use was `SumatraPDF.exe -print-to` for physical printing.

**Cause:** IronPDF is a PDF generation library, not a printer API.

**Resolution:** On Windows, IronPDF exposes a native print API that covers the common cases that previously required Sumatra:

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Option A: Print to the system default printer
pdf.Print();

// Option B: Print to a named printer at a specified DPI
pdf.Print(300, 300, "HP LaserJet");

// Option C: For copies, paper size, duplex, etc., configure the
// System.Drawing.Printing.PrintDocument returned by GetPrintDocument().
```

### "Font rendering differs from Sumatra's viewer output"

**Symptom:** PDFs generated by IronPDF look different from what Sumatra displayed.

**Cause:** Sumatra PDF and IronPDF use different rendering engines. Sumatra displays existing PDFs using its renderer. IronPDF generates PDFs from HTML via Chromium. These are different pipelines.

**Resolution:** If the source of truth is an HTML template, render that in Chrome browser with `@media print` styles enabled (DevTools → Rendering → Emulate CSS media → print) to preview IronPDF output before rendering. The Chromium rendering in IronPDF matches Chrome browser rendering closely:

```csharp
// Ensure @media print styles are applied:
// https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/
var renderer = new ChromePdfRenderer();
// Chromium renders @media print as Chrome browser does
var pdf = await renderer.RenderHtmlAsPdfAsync(htmlWithPrintMediaStyles);
```

### "Docker build succeeds but PDF generation fails at runtime"

**Symptom:** After removing Sumatra (Windows binary) and adding IronPDF, Docker builds succeed but rendering throws at runtime.

**Cause:** IronPDF uses embedded Chromium; on minimal Linux base images, Chromium may require system libraries that aren't present.

**Resolution:**
```dockerfile
# IronPDF on Linux typically requires certain system libraries
# Use a non-minimal base image or install dependencies
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Common Chromium dependencies for IronPDF on Linux:
RUN apt-get update && apt-get install -y \
    libnss3 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libgbm1 \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

# IronPDF: no SumatraPDF.exe needed
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

### "Exit code error handling needs to become try/catch"

**Symptom:** Existing error handling checks `process.ExitCode != 0` for Sumatra failures; this pattern is gone.

**Resolution:**
```csharp
// Before (Sumatra exit code pattern):
// using var proc = Process.Start(psi)!;
// proc.WaitForExit();
// if (proc.ExitCode != 0)
//     throw new Exception($"SumatraPDF failed: exit {proc.ExitCode}");

// After (IronPDF exception pattern):
try
{
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    pdf.SaveAs(outputPath);
}
catch (IronPdf.Exceptions.IronPdfException ex)
{
    Console.Error.WriteLine($"PDF generation failed: {ex.Message}");
    throw;
}
```

---

## Critical Migration Notes

### This Migration Is a Workflow Replacement, Not a Library Swap

Sumatra PDF and IronPDF solve fundamentally different problems:
- Sumatra PDF: views and prints existing PDF files (Windows-only CLI application)
- IronPDF: generates and manipulates PDF files (cross-platform .NET library)

The migration isn't a direct API replacement — it's replacing a Windows-only binary invocation workflow with an in-process library. The scope depends on whether your use case was printing existing PDFs (partial migration, IronPDF handles generation; printing still needs OS API) or generating PDFs from HTML (full replacement).

### Process Cleanup

After removing Sumatra invocations, audit for leftover process-related infrastructure:

```bash
# Find process exit code handling patterns that can be removed
rg "WaitForExit\|ExitCode\|RedirectStandardError" --type cs -n

# Find binary path config that can be removed
grep -r "SUMATRA_PATH\|SumatraPath\|sumatrapdf" appsettings*.json *.env 2>/dev/null

# Find bundled binary references
find . -name "SumatraPDF.exe" 2>/dev/null
```

### AGPLv3 Licensing Clarification

Sumatra PDF is licensed under AGPLv3 (with some BSD-licensed files). The strong copyleft means bundling or linking the binary into a closed-source product imposes obligations most commercial vendors avoid — IronPDF's commercial license sidesteps that concern entirely.

---

## Performance Considerations

### In-Process vs Binary Invocation

Each Sumatra PDF operation spawned a new process. IronPDF renders in-process. The process spawn overhead is non-trivial, especially if Sumatra was invoked per-document in a batch:

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Warm-up (first render initializes Chromium)
var renderer = new ChromePdfRenderer();
using var _ = await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");

// Batch generation — compare against Sumatra process-per-document baseline
var sw = Stopwatch.StartNew();
for (int i = 0; i < 10; i++)
{
    using var pdf = await renderer.RenderHtmlAsPdfAsync($"<html><body><h1>Document {i}</h1></body></html>");
    pdf.SaveAs($"doc-{i}.pdf");
}
sw.Stop();
Console.WriteLine($"10 documents in {sw.Elapsed.TotalMilliseconds:F0}ms (in-process, no process spawn)");
```

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

// https://ironpdf.com/examples/parallel/
var htmlJobs = Enumerable.Range(1, 10)
    .Select(i => $"<html><body><h1>Document {i}</h1></body></html>")
    .ToArray();

var pdfs = await Task.WhenAll(htmlJobs.Select(async html =>
{
    var r = new ChromePdfRenderer();
    return await r.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Generated {pdfs.Length} PDFs in parallel — see https://ironpdf.com/how-to/async/");
foreach (var pdf in pdfs) pdf.Dispose();
```

---

## Migration Checklist

### Pre-Migration
- [ ] Identify all Sumatra PDF invocations (`rg "SumatraPDF\|sumatrapdf" --type cs`)
- [ ] Determine primary use case: printing existing PDFs, or rendering/generation
- [ ] Identify how PDFs were generated before being passed to Sumatra (this is what IronPDF replaces)
- [ ] Find bundled SumatraPDF.exe in project files (`find . -name "SumatraPDF.exe"`)
- [ ] Find Sumatra references in Dockerfiles and CI configs
- [ ] Confirm AGPLv3 implications for the current bundling/deployment model
- [ ] Obtain IronPDF license key
- [ ] Confirm IronPDF .NET version compatibility (.NET 6/7/8/9)

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Add license key at application startup
- [ ] Replace HTML generation → file → Sumatra pipeline with `ChromePdfRenderer`
- [ ] Remove all `Process.Start("SumatraPDF.exe")` patterns
- [ ] Replace exit code error handling with try/catch blocks
- [ ] Remove binary path configuration (appsettings, env vars)
- [ ] Add merge capability via `PdfDocument.Merge()` (was secondary library)
- [ ] Add watermark via `TextStamper` (was secondary library or not supported)
- [ ] Add password protection via `pdf.SecuritySettings` (was secondary library)
- [ ] Handle physical printing separately if still required

### Testing
- [ ] Confirm PDF generation in Linux/Docker (the original deployment blocker)
- [ ] Compare PDF output against Sumatra baseline where applicable
- [ ] Test Docker image builds without SumatraPDF.exe copy step
- [ ] Benchmark generation time vs Sumatra-based pipeline (including process spawn)
- [ ] Confirm error handling — exceptions instead of exit codes
- [ ] Test merge, watermark, and security if implemented
- [ ] Test font rendering in deployment environment

### Post-Migration
- [ ] Remove SumatraPDF.exe from project and deployment artifacts
- [ ] Remove SUMATRA_PATH and related environment variables
- [ ] Remove COPY SumatraPDF.exe from Dockerfiles
- [ ] Remove secondary PDF libraries used for merge/watermark/security

---

## That's the Migration

The deployment blocker — Windows binary in a Linux container — is resolved structurally. That's the cleanest migration outcome: the underlying problem goes away, not just the symptom.

The remaining open question is usually physical printer output. If that's still needed, `System.Drawing.Printing` handles it on Windows; IronPDF focuses on the file generation side. Whether they need to coexist or the printer requirement has faded depends on your specific workflow.

**Discussion question:** Based on your own migration from a binary-invocation PDF workflow, what would you add to this checklist — particularly around Docker environment setup or replacing the generation pipeline that fed into Sumatra?
