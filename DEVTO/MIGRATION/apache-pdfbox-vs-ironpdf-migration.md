---
title: "Migrating from Apache PDFBox (.NET Ports) to IronPDF: the parts that actually matter"
published: false
description: "A practical migration guide for .NET teams moving from Apache PDFBox .NET ports (Pdfbox-IKVM, PdfBox_DotNet_Version, MASES.NetPDF) to IronPDF. Covers the IKVM/JCOBridge paradigm shift, API mapping, before/after code, and a printable checklist."
tags: dotnet, csharp, pdf, migration
---

Most .NET teams who reach for Apache PDFBox end up on one of three NuGet packages: `Pdfbox-IKVM`, `PdfBox_DotNet_Version`, or `MASES.NetPDF`. None of them are official. Two of the three have been abandoned for years. The third still calls out to a real JVM at runtime via JCOBridge. If you have one of these in your `.csproj` and are wondering whether to keep maintaining a Java-shaped API and a Java-runtime dependency inside a .NET application, this guide is for you.

Apache PDFBox is a solid open-source Java library. The migration question is not about the upstream library's quality — it is about whether the .NET port you happen to be running still belongs in a modern .NET stack. This guide covers the shift to [IronPDF](https://ironpdf.com), but eighty percent of the framework — dependency auditing, API mapping, testing patterns — applies to any .NET-native PDF library.

## Why Migrate (Without Drama)

PDFBox is one of the most widely deployed PDF libraries in the world. Here is why .NET teams specifically move away from its .NET ports:

1. **Unofficial port status.** Apache ships only Java artifacts. Every .NET PDFBox option on NuGet is a community-driven port. `Pdfbox` (built against PDFBox 1.8.2) and `Pdfbox-IKVM` were last published in 2013 and 2017 respectively. `PdfBox_DotNet_Version` was last published in 2019. `MASES.NetPDF` is the only actively maintained option, and it is a JCOBridge wrapper that requires a JVM at runtime alongside the CLR.
2. **No HTML-to-PDF.** PDFBox does not convert HTML in any form. If you need HTML rendering, you are pairing PDFBox with another tool — adding a second dependency on top of the port.
3. **Runtime baggage.** The IKVM-based ports bundle a .NET reimplementation of the JVM. `MASES.NetPDF` calls into a real JVM via JCOBridge. Either way, the deployment carries Java-runtime weight that idiomatic .NET libraries avoid.
4. **IKVM compatibility gaps.** The IKVM-based ports were built against older PDFBox lines (1.8.x or 2.0.x). They do not track upstream PDFBox 3.0.x, and IKVM's coverage of newer Java features is limited.
5. **API verbosity for common operations.** Because the ports expose the Java API directly, the surface keeps Java conventions: `camelCase` methods, `File` objects, explicit `close()` calls. Merging two PDFs requires constructing a `PDFMergerUtility`, adding sources, and calling `mergeDocuments(MemoryUsageSetting...)`.
6. **Manual resource management.** Documents hold native resources and must be explicitly closed. In a port that wraps Java semantics, leaking a `PDDocument` is easier than leaking an `IDisposable` in idiomatic C#.
7. **Text extraction ordering.** PDFBox extracts text in the order it appears in the PDF content stream, which is not necessarily visual order. Reading order requires `PDFTextStripper.setSortByPosition(true)` — a common source of bugs for teams unfamiliar with PDF internals.
8. **No async support.** The Java-shaped API is synchronous. In an async .NET pipeline, blocking on JVM-bridge calls ties up thread pool threads.
9. **Font handling across platforms.** Font resolution flows through the wrapped Java font infrastructure. Font availability and rendering may differ between development machines and Linux containers in production.
10. **Sparse .NET community.** Help, examples, and best practices for .NET-specific issues with these ports are difficult to find. Most search results point at the Java PDFBox documentation, which uses a slightly different API surface than each port exposes.

### Comparison Table

| Aspect | Apache PDFBox (.NET Ports) | IronPDF (.NET) |
|---|---|---|
| **Focus** | PDF manipulation, text extraction, form filling | HTML-first PDF generation + manipulation |
| **Pricing** | Free, Apache License 2.0 | Per-developer, perpetual licenses available |
| **API Style** | Java objects mirroring PDF spec (PDDocument, PDPage) | C# fluent API, HTML-to-PDF, property-based |
| **Learning Curve** | Moderate — Java idioms plus PDF internals | Low — HTML/CSS developers productive quickly |
| **HTML Rendering** | None | Built-in Chrome rendering engine |
| **Page Indexing** | 0-based | 0-based |
| **Thread Safety** | PDDocument not thread-safe | `ChromePdfRenderer` stateless, safe to share |
| **Namespace** | `org.apache.pdfbox.*` (IKVM) or `Org.Apache.Pdfbox.*` (MASES) | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Complexity | Notes |
|---|---|---|
| HTML → PDF | Low | PDFBox cannot do this; IronPDF is purpose-built for it |
| Text extraction | Medium | Both extract text; API shape and sort behavior differ |
| Merge | Low | Both offer merge; IronPDF is a one-liner |
| Split / extract pages | Low | Different API, same concept |
| Watermark | Medium | PDFBox uses PDPageContentStream drawing; IronPDF uses HTML overlay |
| Password / encryption | Medium | PDFBox uses AccessPermission + StandardProtectionPolicy; IronPDF uses SecuritySettings |
| Form filling (AcroForms) | Medium | Both support AcroForms with different API surfaces |
| Digital signatures | Medium-High | Different libraries and API patterns |
| PDF/A validation | High / N/A | PDFBox has the Preflight subproject; IronPDF supports PDF/A export but does not validate existing documents |
| Image extraction | Medium | Both support image extraction with different APIs |
| PDF rendering to image | Medium | PDFBox renders pages to BufferedImage; IronPDF rasterizes via `ToBitmap(dpi)` |

### Decision Matrix

| Scenario | Recommendation |
|---|---|
| .NET app pinned to an abandoned IKVM-based PDFBox port | Migrate — the port no longer tracks upstream and brings IKVM-era constraints |
| PDFBox used only for text extraction | Evaluate — IronPDF extracts text, but also consider [PdfPig](https://github.com/UglyToad/PdfPig) as a lighter .NET-native alternative |
| PDFBox used for PDF/A validation via Preflight | Keep Preflight (or veraPDF) as a separate validator; IronPDF generates PDF/A but does not validate |
| Full .NET stack, needs HTML-to-PDF + manipulation | Migrate — IronPDF covers both in one NuGet package |

---

## Before You Start

### Prerequisites

- .NET 6+ (or .NET Framework 4.6.2+)
- A trial or licensed [IronPDF key](https://ironpdf.com/get-started/license-keys/)
- Inventory of which PDFBox port your project uses: `Pdfbox`, `Pdfbox-IKVM`, `PdfBox_DotNet_Version`, or `MASES.NetPDF`

### Find PDFBox References

```bash
# Find PDFBox usage across the codebase
rg "org\.apache\.pdfbox|Org\.Apache\.Pdfbox|PDDocument|PDFTextStripper|PDFMergerUtility" --glob "*.cs"

# Identify which port the project references
rg "Pdfbox|Pdfbox-IKVM|PdfBox_DotNet_Version|MASES\.NetPDF" --glob "*.csproj"
```

In PowerShell: `Get-ChildItem -Recurse -Filter *.cs | Select-String "PDDocument|pdfbox|PDFTextStripper"`.

### Swap Dependencies

```bash
# Remove whichever PDFBox .NET port your project uses
dotnet remove package Pdfbox            # PDFBox 1.8.2-era IKVM port
dotnet remove package Pdfbox-IKVM       # IKVM wrapper, last published 2017
dotnet remove package PdfBox_DotNet_Version  # last published 2019
dotnet remove package MASES.NetPDF      # JCOBridge wrapper, requires JVM

# Install IronPDF
dotnet add package IronPdf
```

---

## Quick Start Migration (3 Steps)

### Step 1: License Configuration

**Before (PDFBox .NET port — no license needed, but a JVM-shaped runtime is):**

```csharp
// Apache 2.0 — no license key required.
// IKVM-based ports bundle a .NET-side JVM reimplementation;
// MASES.NetPDF calls into a real JVM via JCOBridge.
using org.apache.pdfbox.pdmodel;

class Program
{
    static void Main()
    {
        PDDocument document = new PDDocument();
        Console.WriteLine($"PDFBox ready. Pages: {document.getNumberOfPages()}");
        document.close();
    }
}
```

**After (IronPDF — one line of configuration):**

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        // License key — trial available at ironpdf.com
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        Console.WriteLine($"IronPDF licensed: {License.IsLicensed}");
        // No bundled JVM, no JCOBridge, no Java-shaped API
    }
}
```

### Step 2: Namespace Imports

**Before (PDFBox .NET port — IKVM-style):**

```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdmodel.font;
using org.apache.pdfbox.text;
using org.apache.pdfbox.multipdf;
using java.io;
```

**After (IronPDF):**

```csharp
using IronPdf;
using IronPdf.Editing;    // watermarks, headers, footers
using IronPdf.Rendering;  // render options
```

### Step 3: Basic PDF Creation

**Before (PDFBox .NET port — manual page construction):**

```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdmodel.font;
using System;

class Program
{
    static void Main()
    {
        PDDocument document = new PDDocument();
        try
        {
            PDPage page = new PDPage();
            document.addPage(page);

            PDPageContentStream cs = new PDPageContentStream(document, page);
            cs.beginText();
            cs.setFont(PDType1Font.HELVETICA_BOLD, 24);
            cs.newLineAtOffset(72, 700); // x, y from bottom-left
            cs.showText("Monthly Report");
            cs.endText();
            cs.close();

            document.save("report_pdfbox.pdf");
        }
        finally
        {
            document.close();
        }
        Console.WriteLine("Saved with PDFBox.");
    }
}
```

**After (IronPDF — HTML rendering):**

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.Letter;

        var pdf = renderer.RenderHtmlAsPdf(
            "<h1 style='font-family:Helvetica;font-size:24pt;'>Monthly Report</h1>");

        pdf.SaveAs("report_ironpdf.pdf");
        Console.WriteLine("Saved with IronPDF.");
    }
}
```

No bundled JVM. No coordinate math. The [IronPDF HTML-to-PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) covers rendering options in detail.

---

## API Mapping Tables

These map the Java-shaped APIs exposed by the PDFBox .NET ports to native IronPDF APIs. Because the ports surface Java method names directly through IKVM or JCOBridge, the differences are both naming (camelCase to PascalCase) and conceptual (page construction to HTML rendering).

### Namespace Mapping

| PDFBox .NET Port | IronPDF Namespace | Purpose |
|---|---|---|
| `org.apache.pdfbox.pdmodel` / `Org.Apache.Pdfbox.Pdmodel` | `IronPdf` | Core document operations |
| `org.apache.pdfbox.text` / `Org.Apache.Pdfbox.Text` | `IronPdf` | Text extraction |
| `org.apache.pdfbox.multipdf` / `Org.Apache.Pdfbox.Multipdf` | `IronPdf` | Merge/split utilities |
| `org.apache.pdfbox.pdmodel.font` | CSS `font-family` in HTML | Font handling |
| `org.apache.pdfbox.pdmodel.encryption` | `IronPdf.Security` | Security/encryption |

### Core Class Mapping

| PDFBox Class | IronPDF Class | Description |
|---|---|---|
| `PDDocument` | `PdfDocument` | The PDF document object |
| `PDPage` | `pdf.Pages[n]` | Individual page access |
| `PDPageContentStream` | HTML/CSS via `ChromePdfRenderer` | Content creation — IronPDF uses HTML instead of drawing commands |
| `PDFMergerUtility` | `PdfDocument.Merge()` | Document merging |

### Document Loading Methods

| Operation | PDFBox .NET Port | IronPDF |
|---|---|---|
| Open from file | `PDDocument.load(new File("f.pdf"))` | `PdfDocument.FromFile("f.pdf")` |
| Open with password | `PDDocument.load(file, "pass")` | `PdfDocument.FromFile("f.pdf", "pass")` |
| Create new | `new PDDocument()` | `renderer.RenderHtmlAsPdf(html)` |
| Save | `document.save("out.pdf")` | `pdf.SaveAs("out.pdf")` |

### Page Operations

| Operation | PDFBox .NET Port | IronPDF |
|---|---|---|
| Get page count | `document.getNumberOfPages()` | `pdf.PageCount` |
| Get page | `document.getPage(index)` (0-based) | `pdf.Pages[index]` (0-based) |
| Add page | `document.addPage(new PDPage())` | Render HTML (pages auto-created) |
| Remove page | `document.removePage(index)` | `pdf.RemovePages(index)` |

### Merge / Split Operations

| Operation | PDFBox .NET Port | IronPDF |
|---|---|---|
| Merge | `PDFMergerUtility` → `addSource()` → `mergeDocuments()` | `PdfDocument.Merge(pdf1, pdf2)` |
| Extract pages | `PDDocument.getPage()` + new doc + import page | `pdf.CopyPages(start, end)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (PDFBox has no HTML-to-PDF; common workaround):**

```csharp
// PDFBox cannot convert HTML to PDF.
// Common workaround: shell out to wkhtmltopdf, then optionally
// post-process the result with PDFBox.
using System.Diagnostics;

class HtmlToPdfPdfbox
{
    static void Main()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "wkhtmltopdf",
            Arguments = "--page-size Letter --margin-top 20 --margin-bottom 20 input.html report.pdf"
        };
        var p = Process.Start(psi);
        p.WaitForExit();

        if (p.ExitCode != 0)
        {
            Console.Error.WriteLine($"wkhtmltopdf failed: {p.ExitCode}");
            return;
        }

        // Optional: post-process with PDFBox if needed
        // PDDocument doc = PDDocument.load(new File("report.pdf"));
        // ... manipulate ...
        // doc.save("report_final.pdf");
        // doc.close();

        Console.WriteLine("Generated with wkhtmltopdf + PDFBox.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class HtmlToPdfIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.Letter;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.PrintHtmlBackgrounds = true;

        var pdf = renderer.RenderHtmlAsPdf(@"
            <html><head><style>
                body { font-family: Arial, sans-serif; }
                .title { color: #1a237e; border-bottom: 2px solid #1a237e; padding-bottom: 10px; }
                table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                td, th { padding: 10px; border: 1px solid #e0e0e0; }
                th { background: #f5f5f5; }
            </style></head><body>
                <h1 class='title'>Q4 Sales Report</h1>
                <table>
                    <tr><th>Region</th><th>Revenue</th><th>Growth</th></tr>
                    <tr><td>North</td><td>$1.2M</td><td>+12%</td></tr>
                    <tr><td>South</td><td>$890K</td><td>+8%</td></tr>
                    <tr><td>West</td><td>$2.1M</td><td>+22%</td></tr>
                </table>
            </body></html>");

        pdf.SaveAs("report.pdf");
        Console.WriteLine("Saved with IronPDF.");
    }
}
```

No external process. No second toolchain. See the [IronPDF HTML-to-PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/).

### 2. Merge PDFs

**Before (PDFBox .NET port):**

```csharp
using org.apache.pdfbox.multipdf;
using org.apache.pdfbox.io;
using System;

class MergePdfbox
{
    static void Main()
    {
        PDFMergerUtility merger = new PDFMergerUtility();
        merger.addSource("part1.pdf");
        merger.addSource("part2.pdf");
        merger.setDestinationFileName("merged_pdfbox.pdf");

        // MemoryUsageSetting governs heap vs temp-file buffering
        merger.mergeDocuments(MemoryUsageSetting.setupMainMemoryOnly());
        Console.WriteLine("Merged with PDFBox.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class MergeIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf1 = PdfDocument.FromFile("part1.pdf");
        var pdf2 = PdfDocument.FromFile("part2.pdf");

        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
        Console.WriteLine("Merged with IronPDF.");
    }
}
```

No `MemoryUsageSetting`. No merger utility class. See [IronPDF merge documentation](https://ironpdf.com/examples/merge-pdfs/).

### 3. Watermark

**Before (PDFBox .NET port — content-stream drawing):**

```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdmodel.font;
using org.apache.pdfbox.util;
using java.io;
using System;

class WatermarkPdfbox
{
    static void Main()
    {
        PDDocument doc = PDDocument.load(new File("input.pdf"));
        try
        {
            foreach (PDPage page in doc.getPages())
            {
                PDPageContentStream cs = new PDPageContentStream(
                    doc, page, PDPageContentStream.AppendMode.APPEND, true, true);

                cs.setFont(PDType1Font.HELVETICA, 48);
                cs.setNonStrokingColor(200, 200, 200); // light gray

                // Rotate 45 degrees around page center
                float cx = page.getMediaBox().getWidth() / 2;
                float cy = page.getMediaBox().getHeight() / 2;
                cs.transform(Matrix.getRotateInstance(
                    Math.toRadians(45), cx, cy));

                cs.beginText();
                cs.newLineAtOffset(cx - 100, cy); // approximate centering
                cs.showText("CONFIDENTIAL");
                cs.endText();
                cs.close();
            }

            doc.save("watermarked_pdfbox.pdf");
        }
        finally
        {
            doc.close();
        }
        Console.WriteLine("Watermarked with PDFBox.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class WatermarkIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        pdf.ApplyWatermark(
            "<h1 style='color:rgb(200,200,200);font-size:48px;opacity:0.5;font-family:Helvetica;'>CONFIDENTIAL</h1>",
            rotation: 45
        );

        pdf.SaveAs("watermarked.pdf");
        Console.WriteLine("Watermarked with IronPDF.");
    }
}
```

Content-stream operations, matrix transforms, and per-page loops collapse into one HTML string.

### 4. Password Protection

**Before (PDFBox .NET port):**

```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdmodel.encryption;
using java.io;
using System;

class PasswordPdfbox
{
    static void Main()
    {
        PDDocument doc = PDDocument.load(new File("input.pdf"));
        try
        {
            AccessPermission perms = new AccessPermission();
            perms.setCanPrint(false);
            perms.setCanModify(false);
            perms.setCanExtractContent(false);

            StandardProtectionPolicy policy = new StandardProtectionPolicy(
                "owner123",  // owner password
                "user456",   // user password
                perms
            );
            policy.setEncryptionKeyLength(128);

            doc.protect(policy);
            doc.save("protected_pdfbox.pdf");
        }
        finally
        {
            doc.close();
        }
        Console.WriteLine("Protected with PDFBox.");
    }
}
```

**After (IronPDF):**

```csharp
using IronPdf;

class PasswordIronPdf
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var pdf = PdfDocument.FromFile("input.pdf");

        pdf.SecuritySettings.OwnerPassword = "owner123";
        pdf.SecuritySettings.UserPassword = "user456";
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.NoPrint;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;
        pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;

        pdf.SaveAs("protected.pdf");
        Console.WriteLine("Protected with IronPDF.");
    }
}
```

No `AccessPermission` + `StandardProtectionPolicy` ceremony. See [IronPDF security documentation](https://ironpdf.com/how-to/pdf-permissions-passwords/).

---

## Critical Migration Notes

### The Runtime Shift Is the Migration

Unlike a port-to-port API swap, this migration changes the runtime shape of the dependency. You are eliminating an IKVM-bundled JVM (or a real JVM via JCOBridge) and the Java-style surface that comes with it. Plan for the dependency cleanup, not just the code rewrite — `.csproj` references, transitive Java-class assemblies, and any bootstrap code that initialized the bridge all need to go.

### Page Indexing

Both PDFBox and IronPDF use 0-based page indexing. This is the one thing that does not change.

### Text Extraction Ordering

PDFBox's `PDFTextStripper` extracts text in content-stream order by default, which can produce scrambled output. You must set `setSortByPosition(true)` for visual reading order. IronPDF's `ExtractAllText()` returns text without requiring that toggle; if extraction quality matters for your corpus, compare outputs side-by-side on representative documents during the migration.

### Resource Management

PDFBox's `PDDocument` requires an explicit `close()` call. IronPDF's `PdfDocument` implements `IDisposable`, so a `using` block handles cleanup. The pattern maps naturally — and disposal failures are easier to catch in idiomatic C# than in Java-shaped wrappers.

### Bouncy Castle Dependency

The IKVM-based PDFBox ports pull in Bouncy Castle for encryption. IronPDF handles encryption internally in .NET, so the Bouncy Castle dependency goes away with the port.

---

## Performance Considerations

### Eliminating the Bridge

Calls through IKVM or JCOBridge cross a runtime boundary on every invocation. Direct .NET calls in IronPDF avoid that crossover:

```csharp
// Before: Java-bridged call through the .NET port
// PDDocument doc = PDDocument.load(new File(path));

// After: in-process .NET, no bridge
using var pdf = PdfDocument.FromFile(path);
```

### Renderer Reuse

```csharp
private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();
```

### Disposal

```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Edge Cases Worth Flagging

- **Text extraction quality:** PDFBox's `PDFTextStripper` is well-regarded for extraction quality. Compare IronPDF's output against your representative document corpus. For extraction-heavy workloads, also consider [PdfPig](https://github.com/UglyToad/PdfPig), a .NET-native PDF text extraction library under Apache 2.0.
- **PDF/A validation:** PDFBox's Preflight module validates PDF/A compliance. IronPDF generates PDF/A but does not validate existing documents — if you need validation, keep Preflight or move to [veraPDF](https://verapdf.org/).
- **Large file memory:** PDFBox exposes `MemoryUsageSetting` to choose between heap and temp-file buffering for large documents. IronPDF handles this internally; benchmark with your largest production documents during migration.

---

## Migration Checklist

### Pre-Migration (8 items)

- [ ] Identify which PDFBox .NET port is referenced: `Pdfbox`, `Pdfbox-IKVM`, `PdfBox_DotNet_Version`, or `MASES.NetPDF`
- [ ] List all PDFBox operations in use: merge, text extraction, form fill, encryption, etc.
- [ ] Determine whether PDFBox Preflight (PDF/A validation) is in the dependency tree
- [ ] Obtain an IronPDF trial key from [ironpdf.com/get-started/license-keys/](https://ironpdf.com/get-started/license-keys/)
- [ ] Create a migration branch
- [ ] Document any bootstrap code that initializes IKVM or JCOBridge
- [ ] Identify all call sites that pass `java.io.File` or other bridged Java types
- [ ] Plan removal of IKVM/JCOBridge runtime assets from the deployment

### Code Migration (10 items)

- [ ] Add the `IronPdf` NuGet package to the .NET project
- [ ] Set `IronPdf.License.LicenseKey` in application startup
- [ ] Replace `PDDocument.load(new File(...))` with `PdfDocument.FromFile(...)`
- [ ] Replace `PDFMergerUtility` patterns with `PdfDocument.Merge()`
- [ ] Replace `PDPageContentStream` watermark drawing with `ApplyWatermark()`
- [ ] Replace `AccessPermission` + `StandardProtectionPolicy` with `SecuritySettings`
- [ ] Replace external-tool HTML-to-PDF with `ChromePdfRenderer.RenderHtmlAsPdf()`
- [ ] Add `using` blocks for all `PdfDocument` instances
- [ ] Test text extraction output against the PDFBox baseline
- [ ] Remove the PDFBox port and IKVM/JCOBridge packages from `.csproj`

### Testing (7 items)

- [ ] Compare PDF merge output byte-for-byte or visually
- [ ] Compare text extraction output against PDFBox for 10+ diverse documents
- [ ] Test password protection round-trip (encrypt, save, open, verify permissions)
- [ ] Test watermark positioning on multi-page documents
- [ ] Benchmark native IronPDF calls against the PDFBox-port baseline
- [ ] Validate on Linux containers (the bundled JVM and IronPDF's runtime behave differently)
- [ ] Run integration tests for all existing PDF workflows

### Post-Migration (4 items)

- [ ] Remove IKVM/JCOBridge runtime assemblies from the build output
- [ ] Drop the JDK or JRE from CI agents if it was only there for the port
- [ ] Slim Docker base images that previously included Java tooling
- [ ] Update architecture diagrams — one fewer runtime to think about

---

## Where to Go From Here

The real migration here is not API-to-API. It is runtime-to-runtime. You are removing a Java-shaped surface, a bundled or bridged JVM, and the Java exception types and `File` objects that leak through any .NET PDFBox port. The code translation is the easy part. The dependency simplification is where the savings compound.

**Worth knowing even without IronPDF:** if you are pinned to an IKVM-based PDFBox port for compatibility reasons, IKVM's coverage of newer Java versions is limited. If you stay in .NET but want a free option, [PdfPig](https://github.com/UglyToad/PdfPig) handles text extraction and basic manipulation in pure C# under Apache 2.0 — though it does not do HTML-to-PDF.

What is the most unusual PDFBox-port setup you have inherited? I have seen IKVM bindings, JCOBridge wrappers, a CLI shell-out, and even a scheduled job that polled a shared folder. I would love to hear if there is something even more unusual out there.

---
