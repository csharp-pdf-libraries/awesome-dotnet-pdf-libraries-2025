---
title: "MuPDF to IronPDF: less config, same output"
published: false
tags: dotnet, csharp, pdf, migration
---

Setting up MuPDF in a .NET project means dealing with native binaries before you write a line of PDF code. You pull in `libmupdf`, figure out the right platform-specific binary for your target environment, add a wrapper package such as `MuPDF.NET` or `MuPDFCore`, and verify the native side loads correctly on both your dev machine and your Docker image. That is setup overhead that has nothing to do with the actual requirement — generating or processing PDFs.

This article covers the migration from MuPDF-based .NET integrations to IronPDF. The mapping tables and checklist stand on their own if you end up choosing a different library.

---

## Why migrate (without drama)

Nine reasons teams look at alternatives to MuPDF-based .NET integrations:

1. **AGPL licensing constraints** — MuPDF and both major wrappers (MuPDF.NET, MuPDFCore) ship under AGPL-3.0. Closed-source distribution requires a commercial license from Artifex on quote-based pricing.
2. **No HTML-to-PDF engine** — MuPDF renders and parses PDFs; it does not include an HTML/CSS renderer. Teams needing HTML-to-PDF add a second tool (Chromium, wkhtmltopdf, PrinceXML) and only load the result with MuPDF.
3. **Native binary management** — both wrappers ship `MuPDF.NativeAssets` / `MuPDFCore.NativeAssets` packages with per-RID binaries (win-x64, linux-x64, osx-x64). Docker images need the matching native package and a compatible base image (glibc vs musl).
4. **API surface skewed toward reading** — although MuPDF.NET can merge, redact, sign, and stamp, most of the API is shaped around viewing and structured-text extraction rather than HTML-driven document generation.
5. **C interop surface** — wrappers expose context and document lifetimes that map onto the underlying C library. Disposal order matters, native crashes can take down the process, and marshalling cost is non-trivial for large documents.
6. **ARM64 deployment** — ARM64 NativeAssets must be present in the wrapper's runtime package set for that platform; coverage is wrapper-dependent.
7. **CI/CD binary distribution** — native binaries in CI/CD pipelines create supply chain and caching complexity.
8. **Maintenance overhead** — tracking security updates for a native C library plus its .NET wrapper is a separate maintenance track from your application code.
9. **Mostly synchronous APIs** — async coverage varies by wrapper; MuPDF.NET is largely synchronous and MuPDFCore exposes a small async surface such as `GetStructuredTextPageAsync`.

### Comparison table

| Aspect | MuPDF (.NET wrappers) | IronPDF |
|---|---|---|
| Focus | Rendering, structured text, low-level manipulation | HTML-to-PDF + PDF manipulation |
| Pricing | AGPL (free) or quote-based commercial via Artifex | Commercial with published pricing |
| API Style | P/Invoke wrappers over a native C library | In-process .NET library with bundled Chromium |
| Learning Curve | High (native interop, context/document lifetimes) | Medium |
| HTML Rendering | Not supported (no HTML renderer) | Chromium-based |
| Page Indexing | 0-based | 0-based |
| Async Support | Mostly synchronous; small async surface in MuPDFCore | Async APIs across renderer and document |
| Namespace | `MuPDF.NET` or `MuPDFCore` | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | MuPDF approach | Effort to migrate |
|---|---|---|
| Render existing PDF to image | MuPDF native strength | Medium (different rendering model) |
| Parse/extract PDF text | MuPDF native | Low-Medium |
| HTML string to PDF | Not native — second tool | Low (native in IronPDF) |
| URL to PDF | Not native — second tool | Low |
| Merge PDFs | `Document.InsertPdf` (MuPDF.NET) | Low |
| Split PDF | `Document.Select` (MuPDF.NET) | Low |
| Watermark | Image/text stamp per page | Low |
| Password protection | Encryption flags on `Save` | Low |
| Page count / metadata | Via wrapper API | Low |
| Native binary management | Required | Eliminated |
| Cross-platform deployment | Complex (per-arch binaries) | Simplified |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| Primary use: render PDF pages to images | MuPDF is well-suited; IronPDF has a different focus |
| Primary use: HTML-to-PDF generation | IronPDF is a direct fit; MuPDF cannot do this natively |
| AGPL constraint is a blocker | Evaluate commercial MuPDF license vs IronPDF license |
| Native binary management is the pain point | IronPDF eliminates native binary management |

---

## Before you start

### Prerequisites

- .NET 6+ target framework
- Full inventory of MuPDF wrapper features in active use
- Note your wrapper package (`MuPDF.NET` or `MuPDFCore`) and version — this shapes the "Before" code

### Find MuPDF references in your codebase

```bash
# Find the wrapper library name first — check .csproj files
grep -r -i "mupdf" **/*.csproj *.csproj 2>/dev/null

# Find all MuPDF usage in C# files
rg -l "MuPDF\.NET|MuPDFCore|MuPDFDocument|MuPDFContext" --type cs

# Find P/Invoke declarations if using direct interop
rg "DllImport.*mupdf|extern.*mupdf" --type cs -n -i

# Find native binary references in build files
rg "libmupdf|mupdf\.dll|mupdf\.so" . --type xml -n 2>/dev/null
find . -name "*.dll" -name "*mupdf*" -o -name "*.so" -name "*mupdf*" 2>/dev/null
```

### Remove MuPDF, install IronPDF

```bash
# Remove whichever wrapper you used
dotnet remove package MuPDF.NET
dotnet remove package MuPDFCore
dotnet remove package MuPDFCore.NativeAssets.Linux
dotnet remove package MuPDFCore.NativeAssets.MacOS
dotnet remove package MuPDFCore.NativeAssets.Windows
dotnet remove package MuPDF.NativeAssets

# Install IronPDF
dotnet add package IronPdf
dotnet restore
```

Also remove native binaries from your project:

```bash
# Find native binaries bundled in project
find . -name "libmupdf*" -o -name "mupdf*.dll" -o -name "mupdf*.so" 2>/dev/null
# Remove them from the repository and any Dockerfile COPY commands that reference them
```

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (MuPDF.NET — no in-process .NET license key; commercial license from Artifex is a separate contract):**
```csharp
// MuPDF open-source: no in-process license key.
// Commercial license from Artifex is a separate agreement.
// Wrapper initialization is implicit — opening a Document loads the native library.
using MuPDF.NET;
// Document doc = new Document("input.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set once at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
// License guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
// Artifex official wrapper:
using MuPDF.NET;

// Or community wrapper:
// using MuPDFCore;
// using MuPDFCore.StructuredText;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
```

### Step 3: Basic PDF operation (text extraction → HTML-to-PDF)

**Before (MuPDF.NET — text extraction):**
```csharp
using MuPDF.NET;
using System;
using System.Text;

class BasicExample
{
    static void Main()
    {
        Document doc = new Document("input.pdf");

        StringBuilder allText = new StringBuilder();
        for (int i = 0; i < doc.PageCount; i++)
        {
            string pageText = doc[i].GetText();
            allText.AppendLine(pageText);
        }

        Console.WriteLine(allText.ToString());
    }
}
```

**After (IronPDF — HTML-to-PDF or text extraction):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// If replacing HTML-to-PDF (MuPDF did not do this — you had a second tool):
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/

// If replacing text extraction:
// var pdf = PdfDocument.FromFile("input.pdf");
// string text = pdf.ExtractAllText();
// Guide: https://ironpdf.com/how-to/extract-text-and-images/
```

---

## API mapping tables

### Namespace mapping

| MuPDF wrapper | IronPDF | Notes |
|---|---|---|
| `MuPDF.NET` / `MuPDFCore` | `IronPdf` | Core namespace |
| `MuPDFCore.StructuredText` | `IronPdf` | Structured text in MuPDFCore lives here |
| N/A | `IronPdf.Rendering` | Render options |
| N/A | `IronPdf.Editing` | Stamp, annotate, manipulate |

### Core class mapping

| MuPDF wrapper class | IronPDF class | Description |
|---|---|---|
| `Document` (MuPDF.NET) / `MuPDFDocument` (MuPDFCore) | `PdfDocument` | PDF document representation |
| `doc[i]` (MuPDF.NET) / `document.Pages[i]` (MuPDFCore) | `pdf.Pages[n]` | Individual page access |
| `Pixmap` / `MuPDFDocument.SaveImage` | `pdf.RasterizeToImageFiles(...)` | Page-to-image rendering |
| N/A | `ChromePdfRenderer` | HTML-to-PDF generation |

### Document loading methods

| Operation | MuPDF | IronPDF |
|---|---|---|
| Open existing PDF | `new Document(path)` / `new MuPDFDocument(ctx, path)` | `PdfDocument.FromFile(path)` |
| Open from stream | `new Document("pdf", stream)` | `PdfDocument.FromStream(stream)` |
| Open from bytes | `new Document(...)` from `byte[]` | `PdfDocument.FromBinaryData(bytes)` |
| HTML to PDF | Not native | `renderer.RenderHtmlAsPdf(html)` |
| URL to PDF | Not native | `renderer.RenderUrlAsPdf(url)` |

### Page operations

| Operation | MuPDF | IronPDF |
|---|---|---|
| Page count | `doc.PageCount` / `document.Pages.Count` | `pdf.PageCount` |
| Access page | `doc[i]` / `document.Pages[i]` | `pdf.Pages[n]` (0-based) |
| Extract text (all) | Loop `doc[i].GetText()` | `pdf.ExtractAllText()` |
| Extract text (page) | `doc[i].GetText()` | `pdf.ExtractTextFromPage(i)` |
| Page size | `page.Rect.Width` / `.Height` | `pdf.Pages[n].Width` / `.Height` |

### Merge/split operations

| Operation | MuPDF | IronPDF |
|---|---|---|
| Merge | `docA.InsertPdf(docB)` (MuPDF.NET) | `PdfDocument.Merge(pdf1, pdf2)` |
| Split / select pages | `doc.Select(new[]{0,1,2})` | `pdf.CopyPages(startIndex, endIndex)` |
| Remove pages | `doc.DeletePage(i)` / `DeletePages(...)` | `pdf.RemovePages(indices)` |

---

## Four complete before/after migrations

> **Note:** "Before" blocks use the MuPDF.NET (Artifex official) API by default and note MuPDFCore equivalents where they differ.

### 1. HTML to PDF

**Before (MuPDF — HTML-to-PDF is not native; teams add a second tool):**
```csharp
using System;
using System.Diagnostics;
using System.IO;

class HtmlToPdfWithSecondTool
{
    static void Main()
    {
        // Neither MuPDF.NET nor MuPDFCore ships an HTML/CSS engine.
        // Common patterns teams use alongside MuPDF:
        // 1. wkhtmltopdf subprocess
        // 2. Puppeteer/PuppeteerSharp
        // 3. Another library entirely

        // Example: wkhtmltopdf subprocess (common workaround)
        string html = "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>";
        string tempHtml = Path.GetTempFileName() + ".html";
        string tempPdf  = Path.GetTempFileName() + ".pdf";

        File.WriteAllText(tempHtml, html);
        var psi = new ProcessStartInfo("wkhtmltopdf", $"\"{tempHtml}\" \"{tempPdf}\"")
        {
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var proc = Process.Start(psi)!;
        proc.WaitForExit(30_000);

        if (proc.ExitCode != 0)
            throw new Exception($"wkhtmltopdf failed: {proc.StandardError.ReadToEnd()}");

        byte[] pdfBytes = File.ReadAllBytes(tempPdf);
        File.Delete(tempHtml);
        File.Delete(tempPdf);
        // Use pdfBytes...
        Console.WriteLine("Generated invoice PDF");
    }
}
```

**After (IronPDF — replaces both MuPDF and the secondary HTML-to-PDF tool):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>"
);
pdf.SaveAs("invoice.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before (MuPDF.NET — `Document.InsertPdf`, mirroring PyMuPDF's `insert_pdf`):**
```csharp
using MuPDF.NET;

class MergePdfsExample
{
    static void Main()
    {
        Document doc1 = new Document("part1.pdf");
        Document doc2 = new Document("part2.pdf");

        // Append every page of doc2 to the end of doc1
        doc1.InsertPdf(doc2);

        doc1.Save("merged.pdf");
    }
}
```

**After (IronPDF native):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var merged = PdfDocument.Merge(
    PdfDocument.FromFile("part1.pdf"),
    PdfDocument.FromFile("part2.pdf")
);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (MuPDF.NET — stamp an image on each page; no HTML/CSS watermark API):**
```csharp
using MuPDF.NET;

class WatermarkExample
{
    static void Main()
    {
        // MuPDF.NET can stamp watermarks by inserting an image or text
        // on each page; you build the image yourself and place it.
        Document doc = new Document("input.pdf");
        for (int i = 0; i < doc.PageCount; i++)
        {
            var page = doc[i];
            page.InsertImage(page.Rect, fileName: "draft-stamp.png", overlay: true);
        }
        doc.Save("watermarked.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("input.pdf");
var stamper = new TextStamper
{
    Text = "DRAFT",
    FontColor = IronSoftware.Drawing.Color.LightGray,
    FontSize = 50,
    Opacity = 40,
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

**Before (MuPDF.NET — encryption flags on `Save`):**
```csharp
using MuPDF.NET;

class SecurityExample
{
    static void Main()
    {
        // MuPDF.NET writes encrypted PDFs by passing encryption flags
        // and owner/user passwords to Save().
        Document doc = new Document("input.pdf");

        doc.Save(
            "secured.pdf",
            encryption: 4,            // PDF_ENCRYPT_AES_256
            ownerPW: "admincontrol",
            userPW: "viewonly",
            permissions: 0);          // strip all user permissions
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("input.pdf");
pdf.SecuritySettings.UserPassword  = "viewonly";
pdf.SecuritySettings.OwnerPassword = "admincontrol";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Native binary cleanup

When migrating away from MuPDF, audit your deployment artifacts:

```bash
# Find native binaries in project
find . -name "*.so"  -path "*mupdf*" 2>/dev/null
find . -name "*.dll" -path "*mupdf*" 2>/dev/null

# Find Docker COPY commands referencing native binaries
rg "COPY.*mupdf|ADD.*mupdf" Dockerfile* -n

# Find runtime directory references
rg "libmupdf|mupdf\." --type cs -n
```

Remove these from your Docker images, CI/CD pipelines, and project directories.

### Page indexing

Both MuPDF wrappers and IronPDF use 0-based page indexing, so page-specific code translates directly:

```csharp
// IronPDF: 0-based throughout
var firstPage = pdf.Pages[0];
var lastPage  = pdf.Pages[pdf.PageCount - 1];
```

### AGPL licensing

If you are migrating in part because of MuPDF's AGPL, confirm IronPDF's commercial license covers your use case. This is a business/legal decision rather than a purely technical one.

### Text extraction feature parity

Text extraction is one of MuPDF's strongest areas — its structured-text APIs return bounding boxes, lines, and characters. IronPDF returns plain text by default. If precise positional extraction is core to your application, test IronPDF against a representative PDF corpus before committing:

```csharp
// IronPDF text extraction
var pdf = PdfDocument.FromFile("document.pdf");
string fullText = pdf.ExtractAllText();

// Per-page extraction:
foreach (var page in pdf.Pages)
{
    string pageText = page.Text;
    Console.WriteLine($"Page {page.PageIndex}: {pageText.Length} chars");
}
// Guide: https://ironpdf.com/how-to/extract-text-and-images/
```

---

## Performance considerations

### No native binary overhead

MuPDF's P/Invoke pattern has native interop overhead per call. IronPDF operates in managed code with its own Chromium process. The performance characteristics are different — profile your specific workload before sizing.

### Renderer reuse

```csharp
// Instantiate once for batch HTML-to-PDF work
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

foreach (var html in htmlTemplates)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{Guid.NewGuid()}.pdf");
}
```

### Parallel rendering

```csharp
// One renderer per thread — do not share across concurrent calls
await Task.WhenAll(htmlBatch.Select(async html =>
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    await pdf.SaveAsAsync($"{Guid.NewGuid()}.pdf");
}));
// Parallel guide: https://ironpdf.com/examples/parallel/
```

### Edge cases

- **ARM64:** IronPDF ships ARM64 binaries via NuGet — no manual binary sourcing required.
- **Alpine Linux:** Chromium on Alpine has historically required specific dependencies; confirm IronPDF's Alpine compatibility in current docs before targeting it.
- **Memory:** Chromium-based rendering has a different memory profile than MuPDF's C renderer. Profile under load before sizing.

---

## Migration checklist

### Pre-migration

- [ ] Identify your specific MuPDF .NET wrapper (`MuPDF.NET` or `MuPDFCore`) and version
- [ ] Audit all features in use: text extraction, rendering, merge, etc.
- [ ] Identify secondary libraries added alongside MuPDF (Chromium, wkhtmltopdf, PrinceXML)
- [ ] Inventory native binaries in project and Docker images: `find . -name "*mupdf*"`
- [ ] Confirm IronPDF .NET target framework compatibility
- [ ] Assess AGPL licensing impact — confirm replacement library licensing meets requirements
- [ ] Set up an IronPDF trial license in dev environment
- [ ] Test IronPDF text extraction against your PDF corpus if that is a key feature

### Code migration

- [ ] Remove MuPDF wrapper NuGet package(s) and matching NativeAssets packages
- [ ] Remove secondary libraries (wkhtmltopdf, etc.) if only used to supplement MuPDF
- [ ] Add `IronPdf` NuGet package
- [ ] Replace wrapper using directives with `using IronPdf` etc.
- [ ] Replace HTML-to-PDF calls (MuPDF did not do this — replace your secondary tool)
- [ ] Replace text extraction calls
- [ ] Replace merge operations
- [ ] Replace watermark operations
- [ ] Replace security/encryption operations
- [ ] Add IronPDF license key to config

### Testing

- [ ] Render each HTML template and verify output
- [ ] Run text extraction and compare output against MuPDF results
- [ ] Test merge with representative document sets
- [ ] Test watermark on multi-page documents
- [ ] Test password protection
- [ ] Confirm no native binary load errors in all environments
- [ ] Load test at expected peak concurrency

### Post-migration

- [ ] Remove native MuPDF binaries from project directories
- [ ] Remove native binary COPY/ADD steps from Dockerfiles
- [ ] Remove MuPDF binary install steps from CI/CD pipelines
- [ ] Update deployment documentation
- [ ] Monitor bundle size change (NuGet-distributed Chromium is larger than MuPDF binaries)

---

## Where to Go From Here

The install footprint complaint that opens most MuPDF-to-managed-library migrations is valid — native binary distribution across multiple architectures is real operational overhead. The tradeoff is that IronPDF's Chromium distribution is larger than MuPDF's native binaries, so container image size may increase. Measure both.

**What were your before/after bundle sizes or render times after migrating from MuPDF?** If your team ran benchmarks across the two approaches, those numbers would be genuinely useful for others making the same assessment.
