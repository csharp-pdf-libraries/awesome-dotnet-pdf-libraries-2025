---
title: "Migrating from PrinceXML to IronPDF: no fuss, no fluff"
published: false
tags: dotnet, csharp, pdf, migration
---

The deployment works perfectly in the on-premises environment and breaks as soon as you try to containerize it. PrinceXML is a native binary — it's not a .NET library, it's an executable that you shell out to or call via an HTTP API. In a Docker context, that means either bundling the binary in your image (licensing implications, Linux ABI compatibility, container size) or running PrinceXML as a sidecar service (orchestration complexity, network hop on every render, single point of failure). Neither option is frictionless.

This article covers migrating from PrinceXML to IronPDF. You'll have benchmark-comparable before/after code for HTML-to-PDF, merge, watermark, and password protection, plus a concurrency section for the teams who hit this during a scaling event.

---

## Why Migrate (Without Drama)

Teams evaluating alternatives to PrinceXML commonly encounter these conditions:

1. **Docker/container deployment** — PrinceXML is a native binary; Docker images require bundling the binary or running a sidecar process.
2. **Cloud platform restrictions** — serverless environments (Azure Functions Consumption, AWS Lambda) often can't execute arbitrary native binaries.
3. **Licensing per server** — PrinceXML's per-server license model requires tracking and paying per deployment node; horizontal scaling has direct cost implications.
4. **Process invocation overhead** — shelling out to PrinceXML via `Process.Start()` per document adds process spawn latency and error-handling complexity.
5. **Sidecar service management** — running PrinceXML as an HTTP service introduces health checks, failover, and inter-service error handling.
6. **CSS Paged Media specificity** — PrinceXML's strict CSS Paged Media compliance is ideal for print-typographic work but requires specific HTML preparation for web-derived content.
7. **Missing manipulation features** — PrinceXML generates PDFs but doesn't provide merge, split, watermark, or security APIs out of the box.
8. **Binary version management** — updating PrinceXML requires updating binaries in every environment, which is different from a NuGet version bump.
9. **Linux ABI compatibility** — the PrinceXML binary must match your Linux distribution and container base image.
10. **Concurrent request throughput** — a single PrinceXML process handles one render at a time; concurrency requires multiple processes or a queuing wrapper.

### Comparison Table

| Aspect | PrinceXML | IronPDF |
|---|---|---|
| Focus | CSS Paged Media HTML/XML-to-PDF | HTML-to-PDF + PDF manipulation |
| Pricing | Per-server commercial license (Desktop / Server / OEM tiers) | Per-developer commercial license |
| API Style | Native binary / HTTP API / process invocation | Native .NET library; no external process |
| Learning Curve | Medium; CSS Paged Media concepts required for complex output | Low for .NET developers |
| HTML Rendering | CSS Paged Media W3C compliant | Chromium-based |
| Page Indexing | N/A — output only | 0-based |
| Thread Safety | Process-level isolation; one render per process | Async rendering; one renderer per parallel task |
| Namespace | `PrinceXML.Wrapper` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | PrinceXML | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | Via file / stdin / HTTP API | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | `prince https://...` or HTTP API | `ChromePdfRenderer.RenderUrlAsPdfAsync()` | Low |
| Save to file | `--output` flag / file write | `pdf.SaveAs(path)` | Low |
| Save to bytes | stdout / HTTP response body | `pdf.BinaryData` | Low |
| Custom page size | CSS `@page { size: A4; }` | `RenderingOptions.PaperSize` | Low |
| Headers/footers | CSS Paged Media `@page` running elements | `RenderingOptions.HtmlHeader/Footer` | Medium |
| CSS Paged Media features | Native — full spec | Chromium subset of CSS print spec | High |
| Merge PDFs | Not native — secondary library required | `PdfDocument.Merge()` | Medium |
| Watermark | Not native | `TextStamper` / `ImageStamper` | Medium |
| Password protection | Via `--encrypt` flag | `pdf.SecuritySettings` | Medium |
| Concurrent rendering | Multiple processes | `Task.WhenAll` + async render methods | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Print-exact typographic output (books, legal, complex footnotes) | PrinceXML may be better suited — evaluate CSS Paged Media needs carefully |
| Docker/cloud deployment required | Switch — eliminates native binary dependency |
| Web-derived HTML reports and dashboards | Switch — Chromium rendering matches browser output |
| Horizontal scaling with per-server licensing concern | Switch — IronPDF's NuGet model is container-friendly |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All PrinceXML References

```bash
# Find PrinceXML invocation patterns
rg -l "Prince|princexml|prince\.exe|PrinceXML" --type cs
rg "Prince|princexml" --type cs -n

# Find Process.Start calls that might invoke Prince
rg "Process\.Start.*prince|StartInfo.*prince" --type cs -n

# Find prince in Docker files and CI
grep -r "prince\|princexml" Dockerfile* docker-compose.yml .github/**/*.yml 2>/dev/null

# Find project references
grep -r "Prince\|PrinceXML" *.csproj **/*.csproj 2>/dev/null
```

### Uninstall / Install

```bash
# Remove the official PrinceXML .NET wrapper if you were using it
dotnet remove package PrinceXMLWrapper

# Install IronPDF
dotnet add package IronPdf

dotnet restore
```

Also remove PrinceXML from Docker images:

```dockerfile
# Remove:
# RUN apt-get install -y prince
# COPY prince-license.dat /etc/prince/license/
# RUN chmod ...

# IronPDF: no native binary install step needed
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

### Step 2 — Namespace Swap

**Before:**
```csharp
using PrinceXML.Wrapper;       // official PrinceXMLWrapper NuGet package
using System.Diagnostics;      // for Process.Start() approach
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic Conversion

**Before (PrinceXML via Process.Start — common pattern):**
```csharp
using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main()
    {
        var htmlPath = Path.GetTempFileName() + ".html";
        var pdfPath = "output.pdf";

        File.WriteAllText(htmlPath,
            "<html><body><h1>Hello</h1></body></html>");

        var psi = new ProcessStartInfo
        {
            FileName = "prince",  // must be on PATH; varies by install
            Arguments = $"\"{htmlPath}\" -o \"{pdfPath}\"",
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi)!;
        var errors = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception($"Prince failed: {errors}");

        Console.WriteLine("Saved output.pdf");
        File.Delete(htmlPath);
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

// No native binary required — renders locally via .NET
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync("<html><body><h1>Hello</h1></body></html>");
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## Benchmark Reference Patterns

> Benchmark structures only — no performance claims. Run these against your actual templates in your environment. PrinceXML's render time includes process spawn or HTTP round-trip; IronPDF renders in-process. Measure both under realistic load.

### Single-Render Timing

```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var html = @"
    <html>
    <head><style>@page { size: A4; margin: 2cm; } body { font-family: Arial; }</style></head>
    <body>
        <h1>Benchmark Document</h1>
        <table>
            <tr><th>Column A</th><th>Column B</th><th>Column C</th></tr>
            <tr><td>Value 1</td><td>Value 2</td><td>Value 3</td></tr>
        </table>
    </body>
    </html>";

async Task<(double avg, double p95)> BenchmarkIronPdf(int runs = 25)
{
    var renderer = new ChromePdfRenderer();
    // Warm-up
    using var _ = await renderer.RenderHtmlAsPdfAsync(html);

    var times = new List<double>();
    for (int i = 0; i < runs; i++)
    {
        var sw = Stopwatch.StartNew();
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        sw.Stop();
        times.Add(sw.Elapsed.TotalMilliseconds);
    }

    times.Sort();
    return (times.Average(), times[(int)(times.Count * 0.95)]);
}

var (avg, p95) = await BenchmarkIronPdf(25);
Console.WriteLine($"IronPDF — Avg: {avg:F1}ms | P95: {p95:F1}ms");
Console.WriteLine("Compare against Prince CLI wall time including process spawn");
```

### Concurrent Throughput

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Prince: concurrent renders require multiple processes (no shared state)
// IronPDF: multiple async renders via Task.WhenAll
// https://ironpdf.com/examples/parallel/

async Task<double> BenchmarkConcurrency(int concurrency)
{
    var htmlJobs = Enumerable.Range(1, concurrency)
        .Select(i => $"<html><body><h1>Document {i}</h1></body></html>")
        .ToArray();

    var sw = Stopwatch.StartNew();
    var tasks = htmlJobs.Select(async html =>
    {
        var renderer = new ChromePdfRenderer();
        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        // In production: save or return bytes here
        return pdf.PageCount;
    });

    await Task.WhenAll(tasks);
    sw.Stop();
    Console.WriteLine($"Concurrency {concurrency}: {sw.Elapsed.TotalMilliseconds:F0}ms total");
    return sw.Elapsed.TotalMilliseconds;
}

await BenchmarkConcurrency(5);
await BenchmarkConcurrency(10);
await BenchmarkConcurrency(20);
// See: https://ironpdf.com/how-to/async/
```

### Process-Spawn Overhead Comparison

```csharp
// PrinceXML via Process.Start:
// Total time = process spawn + binary load + render + process exit
// Measure with Stopwatch.StartNew() wrapping the full Process.Start() block

// IronPDF in-process:
// Total time = render (after warm-up)
// No process spawn, no binary I/O

// For fair comparison, warm up IronPDF renderer before measurement
// and measure PrinceXML including the process lifecycle
```

---

## API Mapping Tables

### Namespace Mapping

| PrinceXML | IronPDF | Notes |
|---|---|---|
| `PrinceXML.*` / `System.Diagnostics` | `IronPdf` | Core namespace |
| CLI flags / HTTP params | `IronPdf.Rendering.ChromePdfRenderOptions` | Rendering configuration |
| N/A | `IronPdf.Editing` | Watermark / stamp |

### Core Class Mapping

| PrinceXML Concept | IronPDF Class | Description |
|---|---|---|
| `prince` CLI / HTTP API / wrapper class | `ChromePdfRenderer` | No external process required |
| CLI `--output` / HTTP response | `PdfDocument` | PDF object with manipulation methods |
| CLI flags / config | `ChromePdfRenderOptions` | Page size, margins, print options |
| N/A | `PdfDocument.Merge()` | Local merge of multiple PDFs |

### Document Loading Methods

| Operation | PrinceXML | IronPDF |
|---|---|---|
| HTML file | `prince input.html -o output.pdf` | `renderer.RenderHtmlFileAsPdfAsync(path)` |
| HTML string | Temp file + `prince` / HTTP POST | `renderer.RenderHtmlAsPdfAsync(html)` |
| URL | `prince https://...` | `renderer.RenderUrlAsPdfAsync(url)` |
| Load existing PDF | N/A | `PdfDocument.FromFile(path)` |

### Page Operations

| Operation | PrinceXML | IronPDF |
|---|---|---|
| Page count | N/A — output only | `pdf.PageCount` |
| Remove page | N/A | `pdf.RemovePages(index)` |
| Extract text | N/A | `pdf.ExtractAllText()` |
| Rotate | N/A | `pdf.RotateAllPages(PdfRotation.Clockwise90)` |

### Merge / Split Operations

| Operation | PrinceXML | IronPDF |
|---|---|---|
| Merge | Not native — requires secondary library | `PdfDocument.Merge(doc1, doc2)` |
| Split | Not native | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (PrinceXML via Process.Start):**
```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

class HtmlToPdfBefore
{
    static async Task Main()
    {
        var html = @"
            <html>
            <head>
            <style>
                @page { size: A4; margin: 2cm; }
                body { font-family: 'Arial', sans-serif; }
                table { width: 100%; border-collapse: collapse; }
                td, th { border: 1px solid #ccc; padding: 6px; }
            </style>
            </head>
            <body>
                <h1>Quarterly Report Q3 2024</h1>
                <table>
                    <tr><th>Region</th><th>Revenue</th></tr>
                    <tr><td>APAC</td><td>$2.4M</td></tr>
                </table>
            </body>
            </html>";

        var tmpHtml = Path.GetTempFileName() + ".html";
        await File.WriteAllTextAsync(tmpHtml, html);

        var psi = new ProcessStartInfo
        {
            FileName = "prince",
            Arguments = $"\"{tmpHtml}\" -o report.pdf",
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi)!;
        var err = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0) throw new Exception($"Prince error: {err}");

        File.Delete(tmpHtml);
        Console.WriteLine("Saved report.pdf");
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var html = @"
    <html>
    <head>
    <style>
        @page { size: A4; margin: 2cm; }
        body { font-family: Arial, sans-serif; }
        table { width: 100%; border-collapse: collapse; }
        td, th { border: 1px solid #ccc; padding: 6px; }
    </style>
    </head>
    <body>
        <h1>Quarterly Report Q3 2024</h1>
        <table>
            <tr><th>Region</th><th>Revenue</th></tr>
            <tr><td>APAC</td><td>$2.4M</td></tr>
        </table>
    </body>
    </html>";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs("report.pdf");

Console.WriteLine($"Saved report.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (PrinceXML — no native merge; common pattern):**
```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

class MergeBefore
{
    static async Task Main()
    {
        // PrinceXML can concatenate multiple HTML inputs on the CLI:
        // prince section1.html section2.html -o merged.pdf
        // But for dynamically generated PDFs, teams typically generate each
        // and then use a secondary library to merge the PDF files.

        var htmlSections = new[]
        {
            ("<html><body><h1>Section 1: Overview</h1></body></html>", "sec1.pdf"),
            ("<html><body><h1>Section 2: Detail</h1></body></html>", "sec2.pdf"),
        };

        foreach (var (html, outFile) in htmlSections)
        {
            var tmpHtml = Path.GetTempFileName() + ".html";
            await File.WriteAllTextAsync(tmpHtml, html);

            var psi = new ProcessStartInfo
            {
                FileName = "prince",
                Arguments = $"\"{tmpHtml}\" -o \"{outFile}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            using var p = Process.Start(psi)!;
            await p.WaitForExitAsync();
            File.Delete(tmpHtml);
        }

        // Now merge sec1.pdf + sec2.pdf using secondary library
        Console.WriteLine("PrinceXML merge requires secondary library for programmatic use");
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

// Render concurrently
var t1 = renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 1</h1></body></html>");
var t2 = renderer.RenderHtmlAsPdfAsync("<html><body><h1>Section 2</h1></body></html>");
var results = await Task.WhenAll(t1, t2);

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var merged = PdfDocument.Merge(results[0], results[1]);
merged.SaveAs("merged-report.pdf");

Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (PrinceXML — CSS Paged Media approach or secondary library):**
```csharp
using System;
// PrinceXML: watermark via CSS @page or secondary post-process library

class WatermarkBefore
{
    static void Main()
    {
        // Option A: CSS Paged Media watermark — works in PrinceXML
        var html = @"
            <html>
            <head>
            <style>
                @page {
                    background: url('watermark.png') center/contain no-repeat;
                    /* OR use generated content with opacity */
                }
                body { font-family: Arial; }
            </style>
            </head>
            <body><h1>Confidential Report</h1></body>
            </html>";

        // Pass to prince CLI or HTTP API; full CSS Paged Media syntax
        // is documented at princexml.com/doc/.
        Console.WriteLine("PrinceXML watermark applied via CSS @page background");
    }
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Editing;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Confidential Report</h1></body></html>"
);

// https://ironpdf.com/how-to/custom-watermark/
var watermark = new TextStamper
{
    Text = "CONFIDENTIAL",
    FontColor = IronSoftware.Drawing.Color.Red,
    Opacity = 15, // 0–100 scale
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("confidential.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (PrinceXML `--encrypt` with user/owner passwords):**
```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

class PasswordBefore
{
    static async Task Main()
    {
        var tmpHtml = Path.GetTempFileName() + ".html";
        await File.WriteAllTextAsync(tmpHtml, "<html><body><h1>Protected</h1></body></html>");

        var psi = new ProcessStartInfo
        {
            FileName = "prince",
            Arguments = $"\"{tmpHtml}\" -o protected.pdf --encrypt" +
                        " --user-password=open123 --owner-password=admin456",
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi)!;
        await process.WaitForExitAsync();
        File.Delete(tmpHtml);
        Console.WriteLine("Saved protected.pdf via PrinceXML --encrypt");
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
    "<html><body><h1>Protected Document</h1></body></html>"
);

// https://ironpdf.com/how-to/pdf-permissions-passwords/
pdf.SecuritySettings.UserPassword = "open123";
pdf.SecuritySettings.OwnerPassword = "admin456";

pdf.SaveAs("protected.pdf");
Console.WriteLine("Saved protected.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Critical Migration Notes

### CSS Paged Media Gap

PrinceXML implements W3C CSS Paged Media closely. If your templates use `running()`, `string()`, `@footnote`, named page flows, or Prince-specific extensions (`-prince-*` properties), those won't transfer to IronPDF's Chromium renderer.

Before migrating, audit your CSS for:

```bash
# Find Prince-specific CSS properties and CSS Paged Media features
rg "\-prince\-|@footnote|running\(|@page.*string\(" --type css
rg "\-prince\-|@footnote|running\(" --type html
```

For headers/footers, migrate from CSS Paged Media `@page` running elements to IronPDF's `HtmlHeader/Footer` API:

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// https://ironpdf.com/how-to/headers-and-footers/
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='font-size:10px; text-align:right'>{page} of {total-pages}</div>",
};

// Replace: @page { @top-right { content: counter(page); } }
```

### Process Exit Code Handling

PrinceXML via `Process.Start()` returns exit codes; your existing code likely checks `process.ExitCode`. Replace with try/catch for IronPDF exceptions:

```csharp
// Remove this pattern:
// if (process.ExitCode != 0) throw new Exception($"Prince error: {stderr}");

// Replace with:
try
{
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    pdf.SaveAs(outputPath);
}
catch (IronPdf.Exceptions.IronPdfException ex)
{
    Console.Error.WriteLine($"Render failed: {ex.Message}");
    throw;
}
```

### Temp File Cleanup

PrinceXML Process.Start() workflows often write HTML to temp files. With IronPDF, you pass HTML strings directly — remove the temp file write/delete pattern.

```bash
# Find temp file patterns to remove
rg "GetTempFileName|GetTempPath|\.html.*temp|temp.*\.html" --type cs -n
```

### Docker Image Update

```dockerfile
# Remove Prince installation:
# RUN apt-get install -y wget
# RUN wget https://www.princexml.com/download/prince_XX.X_linux.tar.gz
# RUN tar xf prince_*.tar.gz && cd prince_* && ./install.sh
# COPY prince-license.dat /usr/lib/prince/license/

# IronPDF: only NuGet restore needed — no native binary install
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out
```

---

## Performance Considerations

### In-Process vs Process Spawn

PrinceXML via CLI spawns a new process per render. IronPDF renders in-process. The practical impact depends on document complexity and hardware, but process spawn overhead is non-trivial at high throughput. For accurate comparison, benchmark both under your realistic load pattern.

### Renderer Warm-Up

```csharp
using IronPdf;

// First render includes Chromium initialization overhead
// Warm up during application startup for consistent request latency
var renderer = new ChromePdfRenderer();
using var _ = await renderer.RenderHtmlAsPdfAsync("<html><body>warmup</body></html>");
// Renderer is now ready for production traffic
```

### Parallel Rendering Pattern

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/examples/parallel/
var jobs = Enumerable.Range(1, 10)
    .Select(i => $"<html><body><h1>Document {i}</h1></body></html>")
    .ToArray();

var pdfs = await Task.WhenAll(jobs.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Rendered {pdfs.Length} documents in parallel");
foreach (var pdf in pdfs) pdf.Dispose();
```

### Memory Management

```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();

// Always use 'using' on PdfDocument
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// MemoryStream pattern for in-memory processing
// https://ironpdf.com/how-to/async/
var bytes = pdf.BinaryData;
// pdf disposed at end of 'using' block
```

---

## Migration Checklist

### Pre-Migration
- [ ] Identify all PrinceXML invocations (`rg "prince|Prince|princexml" --type cs`)
- [ ] Audit CSS for Prince-specific properties (`rg "\-prince\-" --type css`)
- [ ] Identify CSS Paged Media features in use (running elements, footnotes, named flows)
- [ ] Document current render times including process spawn overhead
- [ ] Identify secondary libraries used for merge/security/watermark
- [ ] Obtain IronPDF license key
- [ ] Confirm IronPDF .NET version compatibility for your target framework

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove PrinceXML NuGet wrapper package reference (if used)
- [ ] Add license key at application startup
- [ ] Replace `Process.Start("prince")` calls with `ChromePdfRenderer`
- [ ] Replace `prince https://...` with `RenderUrlAsPdfAsync()`
- [ ] Replace temp file HTML pattern with direct HTML string rendering
- [ ] Convert exit code error handling to try/catch blocks
- [ ] Migrate `@page` headers/footers to `RenderingOptions.HtmlHeader/Footer`
- [ ] Replace CSS Paged Media watermark with `TextStamper`
- [ ] Replace `--encrypt` flags with `pdf.SecuritySettings`

### Testing
- [ ] Render each HTML template and compare output
- [ ] Check CSS Paged Media features — page size, margins, basic `@page`
- [ ] Confirm headers/footers render on all pages correctly
- [ ] Test password protection — open the file with both user and owner credentials
- [ ] Benchmark render time vs PrinceXML baseline (including process spawn)
- [ ] Confirm Docker image builds without PrinceXML binary install
- [ ] Test in cloud environment (Azure, AWS) if applicable

### Post-Migration
- [ ] Remove PrinceXML binary from Docker images
- [ ] Remove PrinceXML license file from deployment configs
- [ ] Update CI/CD pipelines — remove `prince install` steps
- [ ] Remove secondary PDF manipulation libraries now handled by IronPDF

---

## Conclusion

The clearest operational win in this migration is the Dockerfile simplification — removing a native binary install, license file management, and sidecar service orchestration. The biggest technical risk is CSS Paged Media feature parity, and the CSS audit (above) is the first thing to run before writing any migration code.

If the CSS audit comes back clean (basic `@page` sizing, standard print properties only), this migration is low-risk. If it surfaces `running()` elements, footnotes, or `-prince-*` properties, those sections need rewriting before the migration is comparable output.

**Discussion question:** Based on your own migration, what would you add to the checklist above — particularly around CSS Paged Media feature gaps or Docker deployment patterns that weren't covered here?
