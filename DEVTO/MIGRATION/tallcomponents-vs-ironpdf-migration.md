---
title: "Dropping TallComponents for IronPDF: a .NET migration that fits in an afternoon"
published: false
tags: dotnet, csharp, pdf, migration
---

TallComponents was acquired by Apryse on May 27, 2025, and the brand is now closed to new licenses — Apryse points new buyers at the iText SDK and keeps TallComponents in maintenance mode for existing customers. If your codebase still uses `TallComponents.PDFKit5` or `TallComponents.TallPDF5`, the engine you depend on is frozen at its pre-acquisition state, and the XHTML 1.0/1.1 + CSS 2.1 pipeline is increasingly out of step with the modern HTML5 pages teams actually want to render.

This article covers migrating from TallComponents (PDFKit.NET 5.x and TallPDF.NET 5.x) to IronPDF. You'll have working before/after code for the core operations, a troubleshooting section for the common CI/container issues, and a complete checklist.

---

## Why Migrate (Without Drama)

Teams using TallComponents in 2026 commonly hit a mix of these:

1. **Closed to new licenses** — post-Apryse acquisition, Apryse [no longer sells TallComponents](https://apryse.com/blog/apryse-acquires-tallcomponents) and routes new buyers to the iText SDK. The PDFKit5 and TallPDF5 NuGet packages remain published, but the road map has stopped.
2. **XHTML-only HTML pipeline** — TallPDF's `XhtmlParagraph` parses XHTML 1.0 Strict / XHTML 1.1 + CSS 2.1 only. Modern HTML5 dashboards, flexbox/grid layouts, and JavaScript-rendered content don't survive that engine.
3. **Legacy target frameworks** — PDFKit5 targets .NET Standard 2.0, and the older 4.x line targets .NET Framework 2.0. No first-party .NET 8+ TFM is published.
4. **HTML input gap in PDFKit** — PDFKit.NET on its own has no HTML pipeline at all. Teams that want HTML rendering have to add the layout-oriented TallPDF.NET sibling, which is the package that owns `XhtmlParagraph`.
5. **Secondary tool accumulation** — when XHTML isn't enough, the common workaround is to invoke wkhtmltopdf or Chrome headless as a CLI step and then re-open the result in PDFKit. That's two libraries and a process boundary for one job.
6. **CI/CD environment differences** — native dependencies and Windows-only assumptions that work on developer machines surface as `FileNotFoundException` or `DllNotFoundException` on Linux runners.
7. **Strategic dead end** — Apryse is concentrating development on Apryse SDK (formerly PDFTron) and iText, not TallComponents.

### Comparison Table

| Aspect | TallComponents (PDFKit5 / TallPDF5) | IronPDF |
|---|---|---|
| Status | Closed to new licenses since 2025-05-27 (Apryse) | Active, regular releases |
| Focus | Document model + XHTML 1.x layout (TallPDF) and content-stream manipulation (PDFKit) | HTML5/CSS3-to-PDF + PdfDocument manipulation |
| HTML Rendering | XHTML 1.0/1.1 + CSS 2.1 via `XhtmlParagraph` (TallPDF only) | Embedded Chromium, full HTML5/CSS3/JS |
| API Style | `Document` / `Section` / `Paragraph` (TallPDF), `Document` / `Page` / `Overlay` (PDFKit) | `ChromePdfRenderer` + `PdfDocument` |
| Target Frameworks | .NET Standard 2.0 (PDFKit5), .NET Framework 2.0 (PDFKit 4.x) | .NET Framework 4.6.2+, .NET 6/7/8/9 |
| Page Indexing | 0-based (`document.Pages[0]`) | 0-based |
| Namespace | `TallComponents.PDF.*` / `TallComponents.PDF.Layout.*` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | TallComponents | IronPDF Equivalent | Complexity |
|---|---|---|---|
| HTML string to PDF | `XhtmlParagraph` (XHTML 1.x, TallPDF) | `ChromePdfRenderer.RenderHtmlAsPdfAsync()` | Low |
| URL to PDF | `XhtmlParagraph.Path` (XHTML only) | `renderer.RenderUrlAsPdfAsync()` | Low |
| Load existing PDF | `new Document(stream)` (PDFKit) | `PdfDocument.FromFile(path)` | Low |
| Save to file | `document.Write(stream)` | `pdf.SaveAs(path)` | Low |
| Form field manipulation | `Document.Fields` collection | IronPDF form API | Medium |
| Merge PDFs | `Pages.Add(page.Clone())` (PDFKit) | `PdfDocument.Merge()` / `pdf.AppendPdf()` | Low |
| Watermark | `page.Overlay.Add(TextShape)` (PDFKit) | `TextStamper` / `ImageStamper` | Low |
| Password protection | `Document.Security = new PasswordSecurity { ... }` | `pdf.SecuritySettings` | Low |
| Text extraction | PDFKit text-extraction APIs | `pdf.ExtractAllText()` | Medium |
| Digital signatures | `SignatureField.Sign(cert)` (PDFKit) | `PdfSignature` + `pdf.Sign(signature)` | Medium |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| New license needed and Apryse won't sell one | Migrate — TallComponents is closed to new customers |
| HTML5/CSS3 dashboards or JavaScript-rendered content | Migrate — `XhtmlParagraph` is XHTML 1.x + CSS 2.1 only |
| CI/CD fails on Linux runners due to legacy native dependencies | Migrate — IronPDF supports .NET 6/7/8/9 on Linux containers |
| Heavy form-field manipulation as the core use case | Map the specific form operations to IronPDF's form API as a focused subtask |
| .NET 8+ TFM required | Migrate — no first-party .NET 8 TFM for PDFKit5 |

---

## Before You Start

### Prerequisites

- .NET 6/7/8/9
- IronPDF license key — [get a trial](https://ironpdf.com/how-to/license-keys/)

### Find All TallComponents References

```bash
# Find TallComponents API usage across PDFKit and TallPDF
rg -l "TallComponents|XhtmlParagraph|PasswordSecurity" --type cs
rg "TallComponents|XhtmlParagraph|TextParagraph" --type cs -n

# Find document load and overlay patterns
rg "new Document\(|page\.Overlay\.Add|document\.Fields\[" --type cs -n

# Find TallComponents references in project files
grep -r "TallComponents" *.csproj **/*.csproj 2>/dev/null

# Check for any bundled native files
find . -name "*.dll" -path "*/TallComponents/*" 2>/dev/null
```

### Uninstall / Install

```bash
# Remove TallComponents packages — use whichever your project references.
# Real package IDs on nuget.org are TallComponents.PDFKit5 (5.x) /
# TallComponents.PDFKit (4.x) for the manipulation library, and
# TallComponents.TallPDF5 / TallPDF6 for the layout-oriented sibling.
dotnet remove package TallComponents.PDFKit5
dotnet remove package TallComponents.TallPDF5
dotnet remove package TallComponents.PDFRasterizer4

# Install IronPDF
dotnet add package IronPdf

dotnet restore
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

**Before (real TallComponents namespaces):**
```csharp
using TallComponents.PDF;                        // Document, Page, Pages (PDFKit)
using TallComponents.PDF.Shapes;                 // TextShape, ImageShape (PDFKit)
using TallComponents.PDF.Security;               // PasswordSecurity
using TallComponents.PDF.Layout;                 // Document, Section (TallPDF)
using TallComponents.PDF.Layout.Paragraphs;      // TextParagraph, XhtmlParagraph
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3 — Basic PDF Generation

**Before (TallPDF.NET 5.x, XHTML-based generation):**
```csharp
// NuGet: Install-Package TallComponents.TallPDF5
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Paragraphs;
using System.IO;

class Program
{
    static void Main()
    {
        var document = new Document();
        var section = document.Sections.Add();

        // XhtmlParagraph parses XHTML 1.0 Strict / XHTML 1.1 + CSS 2.1.
        var xhtml = new XhtmlParagraph();
        xhtml.Text = "<html><body><h1>Hello World</h1></body></html>";
        section.Paragraphs.Add(xhtml);

        using (var fs = new FileStream("output.pdf", FileMode.Create))
        {
            document.Write(fs);
        }
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
    "<html><body><h1>Hello World</h1></body></html>"
);
pdf.SaveAs("output.pdf");

Console.WriteLine($"Saved output.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API Mapping Tables

### Namespace Mapping

| TallComponents | IronPDF | Notes |
|---|---|---|
| `TallComponents.PDF` | `IronPdf` | Core PDFKit namespace |
| `TallComponents.PDF.Shapes` | `IronPdf.Editing` | Watermark / stamp shapes |
| `TallComponents.PDF.Security` | `IronPdf` (`pdf.SecuritySettings`) | Password / permissions |
| `TallComponents.PDF.Layout` | `IronPdf` (HTML/CSS instead) | TallPDF document model |
| `TallComponents.PDF.Layout.Paragraphs` | HTML elements | `<p>`, `<h1>`, `<img>`, `<table>` |
| `TallComponents.PDF.Signing` | `IronPdf.Signing` | Digital signatures |

### Core Class Mapping

| TallComponents Class | IronPDF Class | Description |
|---|---|---|
| `Document` (TallPDF layout) | `ChromePdfRenderer` | PDF generation from HTML |
| `Document` (PDFKit manipulation) | `PdfDocument` | Existing PDF manipulation |
| `Section`, `TextParagraph`, `ImageParagraph` | HTML elements | Document structure via HTML |
| `XhtmlParagraph` | `renderer.RenderHtmlAsPdfAsync(html)` | Full HTML5/CSS3 via Chromium |
| `TextShape` (page overlay) | `TextStamper` | Text watermark / stamp |
| `PasswordSecurity` | `pdf.SecuritySettings` | Encryption + permissions |
| `SignatureField` | `PdfSignature` | Digital signing |

### Document Loading Methods

| Operation | TallComponents | IronPDF |
|---|---|---|
| Generate from HTML | `XhtmlParagraph` (XHTML 1.x only) | `renderer.RenderHtmlAsPdfAsync(html)` |
| Load existing PDF | `new Document(stream)` | `PdfDocument.FromFile(path)` |
| Save to file | `document.Write(fileStream)` | `pdf.SaveAs(path)` |
| Save to stream | `document.Write(stream)` | `pdf.Stream` / `pdf.BinaryData` |

### Page Operations

| Operation | TallComponents | IronPDF |
|---|---|---|
| Page count | `document.Pages.Count` | `pdf.PageCount` |
| Remove page | `document.Pages.RemoveAt(index)` | `pdf.RemovePages(index)` |
| Extract text | PDFKit text extraction APIs | `pdf.ExtractAllText()` |
| Append pages | `Pages.Add(page.Clone())` | `pdf.AppendPdf(otherPdf)` |

### Merge / Split Operations

| Operation | TallComponents | IronPDF |
|---|---|---|
| Merge | `outputDoc.Pages.Add(page.Clone())` per page | `PdfDocument.Merge(doc1, doc2)` |
| Split | `outputDoc.Pages.Add(srcDoc.Pages[i].Clone())` | [Guide](https://ironpdf.com/how-to/merge-or-split-pdfs/) |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (TallPDF.NET 5.x via `XhtmlParagraph` — XHTML 1.0/1.1 + CSS 2.1 only):**
```csharp
// NuGet: Install-Package TallComponents.TallPDF5
using TallComponents.PDF.Layout;
using TallComponents.PDF.Layout.Paragraphs;
using System.IO;

class HtmlToPdfBefore
{
    static void Main()
    {
        var document = new Document();
        var section = document.Sections.Add();

        // XhtmlParagraph is the XHTML pipeline. Modern HTML5 features
        // (flexbox, grid, web fonts, JavaScript) won't render here.
        var xhtml = new XhtmlParagraph();
        xhtml.Text = "<html><body><h1>Document</h1></body></html>";
        section.Paragraphs.Add(xhtml);

        using (var fs = new FileStream("doc.pdf", FileMode.Create))
        {
            document.Write(fs);
        }
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// Chromium-based renderer: HTML5, CSS3, web fonts, JavaScript all work.
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = await renderer.RenderHtmlAsPdfAsync(
    "<html><body><h1>Document</h1></body></html>"
);
pdf.SaveAs("doc.pdf");

Console.WriteLine($"Generated doc.pdf ({pdf.PageCount} page(s))");
// https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (PDFKit.NET 5.0 — no dedicated `Merger` class; pages are cloned into a target):**
```csharp
// NuGet: Install-Package TallComponents.PDFKit5
using TallComponents.PDF;
using System.IO;

class MergeBefore
{
    static void Main()
    {
        var outputDoc = new Document();

        using (var fs1 = new FileStream("section1.pdf", FileMode.Open, FileAccess.Read))
        {
            var doc1 = new Document(fs1);
            foreach (Page page in doc1.Pages)
            {
                // Clone is required when moving pages between documents
                outputDoc.Pages.Add(page.Clone());
            }
        }

        using (var fs2 = new FileStream("section2.pdf", FileMode.Open, FileAccess.Read))
        {
            var doc2 = new Document(fs2);
            outputDoc.Pages.AddRange(doc2.Pages.CloneToArray());
        }

        using (var output = new FileStream("merged.pdf", FileMode.Create))
        {
            outputDoc.Write(output);
        }
    }
}
```

**After:**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/how-to/merge-or-split-pdfs/
var pdf1 = PdfDocument.FromFile("section1.pdf");
var pdf2 = PdfDocument.FromFile("section2.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
Console.WriteLine($"Merged: {merged.PageCount} pages");
```

---

### 3. Watermark

**Before (PDFKit.NET — `TextShape` added to each page's overlay):**
```csharp
// NuGet: Install-Package TallComponents.PDFKit5
using TallComponents.PDF;
using TallComponents.PDF.Shapes;
using System.IO;
using System.Drawing;

class WatermarkBefore
{
    static void Main()
    {
        using (var fs = new FileStream("input.pdf", FileMode.Open, FileAccess.Read))
        {
            var document = new Document(fs);

            foreach (Page page in document.Pages)
            {
                var watermark = new TextShape();
                watermark.Text = "CONFIDENTIAL";
                watermark.Font = new Font("Arial", 60);
                watermark.Pen = new Pen(Color.FromArgb(128, 255, 0, 0));
                watermark.X = 200;
                watermark.Y = 400;
                // TextShape has no Rotate property in the PDFKit API.
                // Diagonal rotation is applied via a transform.
                watermark.Transform = new RotateTransform(45);

                page.Overlay.Add(watermark);
            }

            using (var output = new FileStream("watermarked.pdf", FileMode.Create))
            {
                document.Write(output);
            }
        }
    }
}
```

**After:**
```csharp
using IronPdf;
using IronPdf.Editing;
using System;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

var pdf = PdfDocument.FromFile("input.pdf");

// https://ironpdf.com/how-to/custom-watermark/
// Opacity is an integer 0–100, not a 0–1 decimal.
var watermark = new TextStamper
{
    Text = "CONFIDENTIAL",
    FontSize = 60,
    Opacity = 50,
    Rotation = 45,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center,
};

pdf.ApplyStamp(watermark);
pdf.SaveAs("watermarked.pdf");
Console.WriteLine("Watermark applied — https://ironpdf.com/examples/pdf-watermarking/");
```

---

### 4. Password Protection

**Before (PDFKit.NET — `PasswordSecurity` instance assigned to `Document.Security`):**
```csharp
// NuGet: Install-Package TallComponents.PDFKit5
using TallComponents.PDF;
using TallComponents.PDF.Security;
using System.IO;

class PasswordBefore
{
    static void Main()
    {
        using (var fs = new FileStream("input.pdf", FileMode.Open, FileAccess.Read))
        {
            var document = new Document(fs);

            // Security is a property that takes a Security object
            // (PasswordSecurity or CertificateSecurity).
            var security = new PasswordSecurity();
            security.OwnerPassword = "admin456";
            security.UserPassword = "open123";
            security.AllowPrint = false;
            security.AllowCopy = false;
            security.KeyLength = KeyLength.Aes256;
            document.Security = security;

            using (var output = new FileStream("protected.pdf", FileMode.Create))
            {
                document.Write(output);
            }
        }
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
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;

pdf.SaveAs("protected.pdf");
Console.WriteLine("Saved protected.pdf — https://ironpdf.com/examples/encryption-and-decryption/");
```

---

## Troubleshooting Common Migration Issues

### CI Pipeline Fails While Local Build Passes

**Symptom:** `FileNotFoundException` or `DllNotFoundException` for TallComponents-related files in CI, even though local builds succeed.

**Diagnosis:**
```bash
# Check for TallComponents native DLLs that must be copied to output
find . -name "*.dll" -path "*/TallComponents/*" 2>/dev/null

# Check CI runner OS — Linux runners can't load Windows-only DLLs
cat .github/workflows/*.yml | grep "runs-on"

# Check if native binary copy is configured in project
grep -r "CopyToOutput\|PreserveNewest\|TallComponents" *.csproj **/*.csproj 2>/dev/null
```

**Root cause pattern:** Legacy TallComponents components targeting older .NET Framework TFMs can have Windows-only dependencies. If your CI runner is Linux-based, platform mismatch is a typical failure cause.

**Resolution:** After migrating to IronPDF, add any needed Chromium system dependencies to the CI container:

```yaml
# GitHub Actions — add IronPDF system dependencies for Linux
- name: Install system dependencies
  run: |
    sudo apt-get update
    sudo apt-get install -y libnss3 libatk-bridge2.0-0 libdrm2 libgbm1 libxkbcommon0
```

### Container Image Missing Fonts

**Symptom:** PDF renders in CI but fonts appear as boxes or fallback glyphs in output.

**Cause:** Custom fonts present on developer machines aren't in the Docker base image.

**Resolution:**
```dockerfile
# Install base font packages
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y \
    fonts-liberation \
    fonts-noto-core \
    # Add specific font packages your PDFs require
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

Alternatively, embed fonts in HTML via Google Fonts CDN or base64 data URIs:

```csharp
var html = @"
    <html>
    <head>
    <link href='https://fonts.googleapis.com/css2?family=Open+Sans' rel='stylesheet'>
    <style>body { font-family: 'Open Sans', sans-serif; }</style>
    </head>
    <body><h1>Document</h1></body>
    </html>";

var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
```

### "PDF opens fine locally but is corrupted after container deploy"

**Symptom:** PDF file saves without error but is unreadable or shows corruption when opened.

**Diagnosis:**
```bash
# Confirm the output file is a valid PDF
head -c 4 output.pdf | xxd  # should show %PDF

# Check file size is non-trivial
ls -lh output.pdf

# If using MemoryStream, check that it's not being disposed before SaveAs
```

**Common root cause:** A `MemoryStream` being disposed before the PDF bytes are read. With IronPDF, use `pdf.BinaryData` or `pdf.SaveAs()` before the stream is disposed:

```csharp
using IronPdf;
using System.IO;

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// Safe — SaveAs doesn't depend on a MemoryStream
pdf.SaveAs("output.pdf");

// Also safe — BinaryData is a byte array
var bytes = pdf.BinaryData;
// pdf disposed at end of 'using' block
```

### License Key Not Found in CI

**Symptom:** `IronPdf.Exceptions.IronPdfLicenseException` in CI; works locally.

**Resolution:**
```yaml
# GitHub Actions — set license as secret
- name: Run tests
  env:
    IRONPDF_LICENSE_KEY: ${{ secrets.IRONPDF_LICENSE_KEY }}
  run: dotnet test
```

```csharp
// In startup code — fail early with clear message
IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY")
    ?? throw new InvalidOperationException(
        "IRONPDF_LICENSE_KEY environment variable not set. Add it to CI secrets.");
```

---

## Critical Migration Notes

### TallComponents Is Not a Chromium-Class HTML Renderer

PDFKit.NET on its own has no HTML pipeline. TallPDF.NET adds `XhtmlParagraph`, but that is an XHTML 1.0/1.1 + CSS 2.1 parser — not a browser engine. If your team was using PDFKit to manipulate PDFs that were generated by a separate tool (wkhtmltopdf, Chrome CLI, etc.), the migration replaces both tools with IronPDF.

Audit your pipeline:

```bash
# Find all PDF generation tools in the codebase
rg "wkhtmltopdf|chrome.*headless|princexml|PhantomJS" --type cs -n

# These can be removed after IronPDF handles generation
```

### Page Indexing

Both TallComponents PDFKit and IronPDF use 0-based page indexing, so direct page-index references usually port over without off-by-one adjustments. Worth confirming in your specific code paths anyway.

### Stream Handling Difference

TallComponents writes via `Document.Write(stream)`. IronPDF exposes `pdf.Stream` (a `MemoryStream`), `pdf.BinaryData`, and `pdf.SaveAs(path)`. Map the pattern:

```csharp
// TallComponents pattern:
// document.Write(outputStream);

// IronPDF patterns:
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);

// Pattern 1: Copy stream
pdf.Stream.CopyTo(existingOutputStream);

// Pattern 2: Get bytes
byte[] bytes = pdf.BinaryData;

// Pattern 3: Save to file
pdf.SaveAs("output.pdf");
```

---

## Performance Considerations

### Parallel Rendering

```csharp
using IronPdf;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");

// https://ironpdf.com/examples/parallel/
var pdfs = await Task.WhenAll(htmlJobs.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync(html);
}));

Console.WriteLine($"Rendered {pdfs.Length} PDFs concurrently");
foreach (var pdf in pdfs) pdf.Dispose();
// See: https://ironpdf.com/how-to/async/
```

### Disposal Pattern

```csharp
using IronPdf;

// Always use 'using' on PdfDocument
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
pdf.SaveAs(outputPath);
// disposed automatically
```

---

## Migration Checklist

### Pre-Migration
- [ ] Find all TallComponents usage (`rg "TallComponents|XhtmlParagraph|PasswordSecurity" --type cs`)
- [ ] Identify which TallComponents package(s) are referenced (PDFKit5, TallPDF5, PDFRasterizer4)
- [ ] Identify CI/CD failure points (platform mismatch, DLL issues)
- [ ] Identify secondary tools used for HTML generation (CLI tools, other libs)
- [ ] Document PDF manipulation features in use (form fields, signatures, etc.)
- [ ] Obtain IronPDF license key
- [ ] Set IRONPDF_LICENSE_KEY in CI secrets

### Code Migration
- [ ] Install IronPDF (`dotnet add package IronPdf`)
- [ ] Remove TallComponents NuGet packages (`TallComponents.PDFKit5`, `TallComponents.TallPDF5`, etc.)
- [ ] Add license key at application startup
- [ ] Replace `XhtmlParagraph` and secondary HTML-to-PDF tools with `ChromePdfRenderer`
- [ ] Replace `new Document(stream)` + `document.Write(stream)` with `PdfDocument.FromFile()` + `pdf.SaveAs()`
- [ ] Replace `Pages.Add(page.Clone())` merge pattern with `PdfDocument.Merge()`
- [ ] Replace `TextShape` + `page.Overlay.Add(...)` watermarks with `TextStamper`
- [ ] Replace `PasswordSecurity` assigned to `Document.Security` with `pdf.SecuritySettings`
- [ ] Remove CLI tool invocation code (Process.Start wkhtmltopdf, etc.)
- [ ] Map any form-field operations to IronPDF's form API

### Testing
- [ ] Render each PDF type and compare visual output
- [ ] Confirm the CI pipeline runs on Linux without native-binary issues
- [ ] Test container deployment
- [ ] Confirm fonts render correctly in container image
- [ ] Test merge, watermark, and security features
- [ ] Test password protection
- [ ] Confirm no stream-disposal issues

### Post-Migration
- [ ] Remove TallComponents NuGet packages
- [ ] Remove secondary HTML-to-PDF CLI tools from deployment artifacts
- [ ] Confirm IronPDF license key is in CI secrets
- [ ] Update Dockerfile with any needed system library installs

---

## Final Thoughts

The Apryse acquisition cleared up the strategic question for anyone still on TallComponents: there is no road map past the current PDFKit5 / TallPDF5 line. The HTML pipeline isn't catching up, the .NET 8+ TFM isn't coming, and new licenses aren't on offer. Migrating now means you control the timing rather than reacting to a forced cutover later.

The CI/CD and modern-HTML stories tend to be the most immediate wins — once `XhtmlParagraph` and any CLI workarounds are gone, the Linux containers usually stop fighting you.

The form-field manipulation use case deserves the most careful look before committing — form APIs differ meaningfully between PDF libraries, so map your specific operations to IronPDF's form API as a focused subtask rather than assuming a one-to-one swap.

**Discussion question:** What version of TallComponents were you migrating from, and did anything break unexpectedly during the switch — particularly around the form field API or specific PDF manipulation features?
