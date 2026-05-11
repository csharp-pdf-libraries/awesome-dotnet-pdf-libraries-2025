---
title: "Migrating from Adobe PDF Library SDK to IronPDF: a working migration in an afternoon"
published: false
description: "A story-driven migration guide for .NET teams moving from the Adobe PDF Library (APDFL/Datalogics) to IronPDF. Covers the paradigm shift from low-level PDF primitives to HTML-first generation."
tags: dotnet, csharp, pdf, migration
---

We were four months into a compliance overhaul when the invoice came. Not the Datalogics renewal itself—we expected that—but the line item for the Forms Extension add-on that nobody remembered purchasing, attached to a server that had been decommissioned in Q2. The Adobe PDF Library SDK had been part of our stack for six years, and somewhere in those six years, the gap between what we needed (turn HTML into PDFs, stamp them, encrypt them, ship them) and what we were paying for (a low-level PDF construction toolkit built on Acrobat source code) had become a canyon.

This is not a story about the Adobe PDF Library being bad software. It is, by any measure, the most comprehensive low-level PDF SDK on the market—it literally shares source code with Acrobat. This is a story about what happens when a team that writes HTML all day is forced to think in PDF content streams, page description operators, and coordinate geometry just to generate a report.

This guide walks through the migration to [IronPDF](https://ironpdf.com). Eighty percent of the content is transferable: dependency auditing, API surface mapping, testing strategies. The remaining twenty percent is IronPDF-specific. Whether you end up using IronPDF, Puppeteer Sharp, or something else entirely, the framework applies.

## Why Migrate (Without Drama)

The Adobe PDF Library is the nuclear reactor of PDF SDKs. It will do anything. The question is whether you need fission when all you want is a light bulb. Here are the triggers that push teams to evaluate:

1. **Paradigm mismatch.** Your team writes HTML and CSS. APDFL thinks in content streams, coordinate pairs, and font resource dictionaries. Every "simple" task requires understanding PDF internals.
2. **Licensing complexity.** APDFL is licensed through Adobe via Datalogics on a per-deployment, OEM, or ISV basis. The process involves sales conversations, not a "buy now" button. For smaller teams or startups, the procurement overhead is a real cost.
3. **No native HTML-to-PDF.** APDFL is not an HTML renderer. If you need HTML-to-PDF, you either pair it with another tool (headless Chrome, wkhtmltopdf) or use an APDFL add-on—adding another dependency and another license.
4. **NuGet package sprawl.** A working APDFL installation requires the core SDK, ICU data, platform runtime packages, and optionally the Forms Extension. Getting the right combination of packages for your target platform takes trial and error.
5. **Learning curve.** APDFL's API mirrors the PDF specification. That is an advantage for PDF specialists and a barrier for everyone else. The difference between a `PDEElement`, a `PDEContainer`, and a `PDEContent` is not obvious to a C# developer who just wants to add a watermark.
6. **Native dependency management.** APDFL includes significant native C++ code. Platform-specific binaries must be present at runtime. Docker builds need the right base image and runtime packages.
7. **Forms Extension is a separate purchase.** If you work with PDF forms—especially XFA forms—the Forms Extension is an add-on with its own license and NuGet package.
8. **COS-level debugging.** When something goes wrong with APDFL, the error messages reference COS objects and PDF internal structures. Debugging requires a PDF specification alongside your C# debugger.
9. **.NET version support cadence.** Datalogics dropped .NET 6 support in early 2025, moving to .NET 8 as the minimum. Teams on .NET 6 LTS face forced upgrades.
10. **Overkill for generation workflows.** If 90% of your PDF work is "take this HTML → produce a PDF → stamp it → encrypt it → email it," you are paying for—and maintaining dependencies on—a full PDF manipulation SDK when a focused HTML-to-PDF library would do.

### Comparison Table

| Aspect | Adobe PDF Library SDK (Datalogics) | IronPDF |
|---|---|---|
| **Focus** | Full PDF specification manipulation, rendering, compliance | HTML-first PDF generation + manipulation |
| **Pricing** | Enterprise license via Adobe/Datalogics, per-deployment | Per-developer, perpetual licenses available |
| **API Style** | Low-level: COS objects, PDE elements, content streams | High-level: HTML → PDF, property-based manipulation |
| **Learning Curve** | High—requires PDF specification knowledge | Low—HTML/CSS developers productive quickly |
| **HTML Rendering** | Not native; requires external tool or add-on | Built-in Chrome rendering engine |
| **Page Indexing** | 0-based | 0-based |
| **Thread Safety** | Library-level initialization required | `ChromePdfRenderer` stateless, safe to share |
| **Namespace** | `Datalogics.APDFL` | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Complexity | Notes |
|---|---|---|
| HTML → PDF | Low | APDFL doesn't do this natively; IronPDF is purpose-built for it |
| Programmatic PDF construction | High | APDFL's PDE-level construction has no direct IronPDF equivalent; rethink as HTML templates |
| Merge | Low | Both offer merge; API shape differs significantly |
| Split / extract pages | Low | Different API, same concept |
| Watermark | Medium | APDFL uses PDE layer drawing; IronPDF uses HTML overlay |
| Password / encryption | Medium | APDFL uses `PDDocSetPermissions`; IronPDF uses `SecuritySettings` |
| Form filling (AcroForms) | Medium | Both support AcroForms; IronPDF uses `FormField` collection |
| XFA forms | High / N/A | APDFL + Forms Extension supports dynamic XFA; IronPDF does not handle XFA—verify in docs |
| PDF/A compliance | Medium-High | APDFL has deep PDF/A support; IronPDF supports PDF/A export—verify version coverage |
| PDF/X, PDF/UA | High | APDFL's strength; IronPDF support varies—verify in docs |
| Text extraction | Low | Both offer text extraction with different APIs |
| Image extraction / rendering | Medium | APDFL renders pages to images natively; verify IronPDF rasterization support |

### Decision Matrix

| Scenario | Recommendation |
|---|---|
| Primarily HTML-to-PDF generation with basic manipulation | Migrate—IronPDF eliminates the paradigm mismatch |
| Deep PDF/A, PDF/X, or PDF/UA compliance workflows | Evaluate carefully—APDFL's compliance coverage is the industry standard |
| XFA forms processing | Stay or find a dedicated XFA tool—IronPDF does not handle XFA |
| Programmatic PDF construction (drawing, coordinate layout) | Rethink as HTML templates in IronPDF, or keep APDFL for those specific workflows |

---

## Before You Start

### Prerequisites

- .NET 6+ (or .NET Framework 4.6.2+)
- A trial or licensed [IronPDF key](https://ironpdf.com/get-started/license-keys/)
- A clear inventory of APDFL features you actually use vs. features you are paying for

### Find APDFL References

```bash
# Find all APDFL using statements
rg -l "Datalogics\.APDFL|using Datalogics" --glob "*.cs"

# Find PDE/COS API usage (indicates deep PDF manipulation)
rg -c "PDEElement|PDEContent|PDEText|CosObj|CosDict" --glob "*.cs"

# Find project references
rg -l "Datalogics" --glob "*.csproj"
```

If you don't have `rg`:

```bash
grep -rl "Datalogics\|APDFL\|PDEElement" --include="*.cs" .
```

In PowerShell, `Get-ChildItem -Recurse -Filter *.cs | Select-String "Datalogics|APDFL|PDEElement"` works similarly.

**Key insight:** If the `rg -c "PDEElement|CosObj"` count is high, you have significant low-level PDF construction code. This code cannot be mechanically translated—it needs to be rethought as HTML templates. If the count is low or zero, your codebase is mostly using APDFL for tasks that IronPDF handles directly, and migration will be straightforward.

### Swap NuGet Packages

```bash
# Remove APDFL packages
dotnet remove package Datalogics.APDFL
dotnet remove package Datalogics.APDFL.ICU.Data
dotnet remove package Datalogics.APDFL.Runtime.Windows  # verify package name
# Remove any other Datalogics.* packages

# Install IronPDF
dotnet add package IronPdf
```

---

## Quick Start Migration (3 Steps)

### Step 1: License Configuration

**Before (Adobe PDF Library SDK):**

```csharp
using Datalogics.APDFL.Document; // verify namespace
using Datalogics.APDFL.Library;  // verify namespace
using System;

class Program
{
    static void Main()
    {
        // APDFL requires library initialization before any operations
        // The library object manages the lifecycle
        using var lib = new Library(); // verify — initializes APDFL runtime

        // License is typically configured via license file or
        // environment settings during Library initialization
        // Exact mechanism varies — verify in Datalogics documentation

        Console.WriteLine("APDFL Library initialized.");

        // All APDFL operations must happen within the Library scope
        // ...
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
        // No library initialization ceremony
        IronPdf.License.LicenseKey = "IRONPDF-YOUR-KEY-HERE";

        Console.WriteLine($"IronPDF licensed: {License.IsLicensed}");
        // Start using IronPDF immediately — no scope restrictions
    }
}
```

The `Library` initialization object and its lifecycle scope go away entirely.

### Step 2: Namespace Imports

**Before:**

```csharp
using Datalogics.APDFL.Library;       // verify — core library init
using Datalogics.APDFL.Document;      // verify — Document class
using Datalogics.APDFL.Document.Page; // verify — Page operations
using Datalogics.APDFL.EditModel;     // verify — PDE layer
using Datalogics.APDFL.Rendering;     // verify — rendering
```

**After:**

```csharp
using IronPdf;
using IronPdf.Editing;    // watermarks, headers, footers
using IronPdf.Rendering;  // render options
```

### Step 3: Basic PDF Creation

**Before (APDFL — programmatic construction, ~25 lines):**

```csharp
using Datalogics.APDFL.Library;   // verify
using Datalogics.APDFL.Document;  // verify
using Datalogics.APDFL.EditModel; // verify
using System;

class Program
{
    static void Main()
    {
        using var lib = new Library(); // verify — required initialization

        // Create a new document
        using var doc = new Document(); // verify class name

        // Add a page with specific dimensions (points: 612x792 = Letter)
        var pageRect = new Rect(0, 0, 612, 792); // verify Rect class
        using var page = doc.CreatePage(
            Document.BeforeFirstPage, // verify constant
            pageRect
        );

        // To add text, you work with PDE elements
        // This is the PDF specification way of doing things:
        // 1. Get or create a font
        // 2. Create a text element
        // 3. Set the text run
        // 4. Add to page content
        var font = new Font("Helvetica"); // verify constructor
        var textElement = new TextRun("Monthly Report", font, 24); // verify — approximate API
        // ... position with matrix transforms, add to page content ...
        // The actual APDFL API is more verbose — verify in docs

        doc.Save(SaveFlags.Full, "report_apdfl.pdf"); // verify save flags
        Console.WriteLine("Saved with APDFL.");
    }
}
```

**After (IronPDF — HTML-first approach, 12 lines):**

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "IRONPDF-YOUR-KEY-HERE";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.Letter;

        // Your "PDF construction" is now HTML
        var pdf = renderer.RenderHtmlAsPdf(@"
            <h1 style='font-family:Helvetica;font-size:24pt;'>Monthly Report</h1>
            <p>Generated on " + DateTime.Now.ToString("yyyy-MM-dd") + @"</p>");

        pdf.SaveAs("report_ironpdf.pdf");
        Console.WriteLine("Saved with IronPDF.");
    }
}
```

The paradigm shift here is fundamental. APDFL constructs PDFs from the inside out—fonts, text runs, content streams, page objects. IronPDF starts from the outside—HTML that your team already writes—and converts it to PDF. The [IronPDF HTML-to-PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) covers rendering options in depth.

---

## API Mapping Tables

### Namespace Mapping

| APDFL Namespace | IronPDF Namespace | Purpose |
|---|---|---|
| `Datalogics.APDFL.Library` | N/A (no init needed) | Library lifecycle management |
| `Datalogics.APDFL.Document` | `IronPdf` | Document creation and manipulation |
| `Datalogics.APDFL.EditModel` | `IronPdf.Editing` | Content modification |

### Core Class Mapping

| APDFL Class | IronPDF Class | Description |
|---|---|---|
| `Library` | N/A | APDFL requires global init; IronPDF does not |
| `Document` | `PdfDocument` | The PDF document object |
| `Page` | `pdf.Pages[n]` | Page access (both 0-based—verify APDFL) |
| `Font` / `PDEFont` | CSS `font-family` in HTML | Font handling—IronPDF uses web fonts via CSS |

### Document Loading Methods

| Operation | APDFL | IronPDF |
|---|---|---|
| Open from file | `new Document("file.pdf")` | `PdfDocument.FromFile("file.pdf")` |
| Open with password | `new Document("file.pdf", "password")` | `PdfDocument.FromFile("file.pdf", "pass")` |
| Create new empty | `new Document()` | `renderer.RenderHtmlAsPdf("<html>...</html>")` |
| Save | `doc.Save(SaveFlags.Full, "output.pdf")` | `pdf.SaveAs("output.pdf")` |

### Page Operations

| Operation | APDFL | IronPDF |
|---|---|---|
| Get page count | `doc.NumPages` | `pdf.PageCount` |
| Get specific page | `doc.GetPage(index)` (0-based, verify) | `pdf.Pages[index]` (0-based) |
| Create page | `doc.CreatePage(position, rect)` | Render HTML content (pages auto-created) |
| Delete page | `doc.DeletePage(index)` | `pdf.RemovePage(index)` |

### Merge / Split Operations

| Operation | APDFL | IronPDF |
|---|---|---|
| Merge documents | `doc.InsertPages(...)` | `PdfDocument.Merge(pdf1, pdf2)` |
| Extract pages | `doc.DeletePage` loop or page copy | `pdf.CopyPages(start, end)` |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (APDFL — no native HTML support, external tool needed):**

```csharp
// APDFL does not natively convert HTML to PDF.
// Common pattern: use a headless browser to generate HTML → PDF,
// then optionally post-process with APDFL.

// Option A: wkhtmltopdf (external process)
using System;
using System.Diagnostics;

class HtmlToPdfApdfl
{
    static void Main()
    {
        // Step 1: Convert HTML using external tool
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "wkhtmltopdf",  // must be installed on server
                Arguments = "--page-size Letter " +
                            "--margin-top 20 --margin-bottom 20 " +
                            "input.html report_apdfl.pdf",
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine("wkhtmltopdf failed: " + process.ExitCode);
            return;
        }

        // Step 2 (optional): Post-process with APDFL for compliance, etc.
        // using var lib = new Library();
        // using var doc = new Document("report_apdfl.pdf");
        // ... add metadata, encrypt, etc. ...

        Console.WriteLine("Generated with wkhtmltopdf + (optional) APDFL post-processing.");
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
        IronPdf.License.LicenseKey = "IRONPDF-YOUR-KEY-HERE";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.Letter;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.PrintHtmlBackgrounds = true;
        renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Screen;

        string html = @"
            <html>
            <head><style>
                body { font-family: 'Segoe UI', sans-serif; margin: 0; }
                .header { background: linear-gradient(135deg, #667eea, #764ba2);
                           color: white; padding: 40px; }
                .content { padding: 30px; }
                table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                th { background: #f8f9fa; text-align: left; }
                td, th { padding: 12px; border-bottom: 1px solid #e9ecef; }
            </style></head>
            <body>
                <div class='header'>
                    <h1>Quarterly Compliance Report</h1>
                    <p>Generated: " + DateTime.Now.ToString("MMMM dd, yyyy") + @"</p>
                </div>
                <div class='content'>
                    <table>
                        <tr><th>Control</th><th>Status</th><th>Last Audit</th></tr>
                        <tr><td>SOC 2 Type II</td><td>Compliant</td><td>2025-12-15</td></tr>
                        <tr><td>PCI DSS</td><td>Compliant</td><td>2025-11-20</td></tr>
                        <tr><td>HIPAA</td><td>Review Needed</td><td>2025-10-01</td></tr>
                    </table>
                </div>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("compliance_report.pdf");
        Console.WriteLine("Saved with IronPDF.");
    }
}
```

The external process dependency disappears. No `wkhtmltopdf` installation, no `Process.Start`, no exit-code checking. CSS gradients, web fonts, and modern layouts render natively. See the [IronPDF HTML-to-PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/).

### 2. Merge PDFs

**Before (APDFL):**

```csharp
using Datalogics.APDFL.Library;  // verify
using Datalogics.APDFL.Document; // verify
using System;

class MergeApdfl
{
    static void Main()
    {
        using var lib = new Library(); // verify — required

        using var doc1 = new Document("part1.pdf"); // verify
        using var doc2 = new Document("part2.pdf"); // verify

        // Insert all pages from doc2 into doc1
        // APDFL uses InsertPages — verify exact method signature
        doc1.InsertPages(
            doc1.NumPages - 1,        // after last page of doc1, verify
            doc2,                      // source document
            0,                         // start page in doc2
            doc2.NumPages,             // number of pages to insert, verify
            PageInsertFlags.All        // verify flag name
        );

        doc1.Save(SaveFlags.Full, "merged_apdfl.pdf"); // verify
        Console.WriteLine("Merged with APDFL.");
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
        IronPdf.License.LicenseKey = "IRONPDF-YOUR-KEY-HERE";

        var pdf1 = PdfDocument.FromFile("part1.pdf");
        var pdf2 = PdfDocument.FromFile("part2.pdf");

        var merged = PdfDocument.Merge(pdf1, pdf2);
        merged.SaveAs("merged.pdf");
        Console.WriteLine("Merged with IronPDF.");
    }
}
```

No page-index arithmetic. No insert flags. See the [IronPDF merge documentation](https://ironpdf.com/examples/merge-pdfs/).

### 3. Watermark

**Before (APDFL — PDE-level text drawing):**

```csharp
using Datalogics.APDFL.Library;   // verify
using Datalogics.APDFL.Document;  // verify
using Datalogics.APDFL.EditModel; // verify
using System;

class WatermarkApdfl
{
    static void Main()
    {
        using var lib = new Library(); // verify

        using var doc = new Document("input.pdf"); // verify

        // For each page, add a watermark using PDE text elements
        for (int i = 0; i < doc.NumPages; i++)
        {
            using var page = doc.GetPage(i); // verify
            var content = page.Content;       // verify — gets PDE content

            // Create font, text run, graphics state, transformation matrix
            // This is PDF specification-level work:
            var font = new Font("Helvetica"); // verify
            var gs = new GraphicState();       // verify
            gs.FillColor = new Color(0.8, 0.8, 0.8); // verify — light gray

            // Create text element at page center with rotation
            // Actual APDFL API involves matrix transforms — verify exact calls
            var textElement = new TextRun("CONFIDENTIAL", font, gs, 48, new Matrix()); // verify — approximate
            // ... position via transformation matrix, add to content ...

            content.AddElement(textElement); // verify
            page.UpdateContent();            // verify
        }

        doc.Save(SaveFlags.Full, "watermarked_apdfl.pdf"); // verify
        Console.WriteLine("Watermarked with APDFL.");
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
        IronPdf.License.LicenseKey = "IRONPDF-YOUR-KEY-HERE";

        var pdf = PdfDocument.FromFile("input.pdf");

        pdf.ApplyWatermark(
            "<h1 style='color:rgba(200,200,200,0.5);font-size:48px;font-family:Helvetica;'>CONFIDENTIAL</h1>",
            rotation: 45
        );

        pdf.SaveAs("watermarked.pdf");
        Console.WriteLine("Watermarked with IronPDF.");
    }
}
```

Fonts, graphic states, transformation matrices, page content loops—all replaced by one CSS-styled HTML string.

### 4. Password Protection

**Before (APDFL):**

```csharp
using Datalogics.APDFL.Library;  // verify
using Datalogics.APDFL.Document; // verify
using System;

class PasswordApdfl
{
    static void Main()
    {
        using var lib = new Library(); // verify

        using var doc = new Document("input.pdf"); // verify

        // Set permissions and passwords
        // APDFL uses PDDocSetPermissions or similar — verify exact API
        var securityParams = new SecurityParams(); // verify class name
        securityParams.OwnerPassword = "owner123"; // verify
        securityParams.UserPassword = "user456";   // verify
        securityParams.Permissions = PermissionFlags.None; // verify — restrict all

        // Apply security — verify method name
        // doc.SetSecurity(securityParams);
        // OR: doc.Save with encryption options — verify

        doc.Save(SaveFlags.Full, "protected_apdfl.pdf"); // verify
        Console.WriteLine("Protected with APDFL.");
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
        IronPdf.License.LicenseKey = "IRONPDF-YOUR-KEY-HERE";

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

Named properties with typed enums replace the security params ceremony. See [IronPDF security documentation](https://ironpdf.com/how-to/pdf-permissions-passwords/).

---

## Critical Migration Notes

### The Paradigm Shift: Construction vs. Conversion

This is the most important thing to understand about this migration. APDFL constructs PDFs by assembling low-level elements—it is a PDF assembler. IronPDF converts web content to PDFs—it is a PDF compiler, where HTML is the source language.

If your codebase has significant `PDEElement`, `PDEText`, `CosObj`, or content stream code, you cannot simply translate it to IronPDF API calls. You need to express the same visual output as HTML/CSS and let IronPDF's Chrome engine render it. This is often less code, but it is different code.

### Library Initialization Scope

APDFL requires a `Library` object to be alive for the duration of all PDF operations. Forgetting to keep it in scope or disposing it early causes crashes. IronPDF has no such requirement—call any API at any time after setting the license key.

### Page Indexing

Both APDFL and IronPDF use 0-based page indexing. This is one less thing to change.

### Save Flags

APDFL's `Save` method takes `SaveFlags` that control incremental vs. full saves. IronPDF's `SaveAs` always performs a full save. If you relied on incremental saves for performance with large documents, test the IronPDF `SaveAs` performance for your file sizes.

### Font Handling

In APDFL, you explicitly create `Font` objects and embed them. In IronPDF, fonts come from CSS. If you use custom fonts, ensure they are installed on the server or loaded via `@font-face` in your HTML. IronPDF's Chrome engine handles font embedding automatically during render.

---

## Performance Considerations

### Eliminating the External Process

If your APDFL setup used `wkhtmltopdf` or headless Chrome as a subprocess for HTML-to-PDF, migrating to IronPDF eliminates that process boundary. The rendering engine runs in-process, which removes:

- Process startup/teardown overhead per render
- Temp file I/O between the subprocess and your .NET code
- Port or file-handle management for the external process

### Renderer Reuse

```csharp
// Create once, reuse across all requests
private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

public PdfDocument RenderReport(string html)
{
    return _renderer.RenderHtmlAsPdf(html);
}
```

### Disposal

Always dispose `PdfDocument` when finished:

```csharp
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Edge Cases Worth Flagging

- **PDF/A output:** APDFL has deep PDF/A-1a, 1b, 2a, 2b, 3a, 3b support. IronPDF supports PDF/A export—verify which conformance levels are covered for your requirements.
- **XFA forms:** If you use APDFL's Forms Extension for dynamic XFA, there is no IronPDF equivalent. AcroForms (static forms) are supported. Consider flattening XFA to AcroForms before migration, or maintaining APDFL for that specific workflow.
- **Print-quality rendering:** APDFL can render PDFs to TIFF at 300+ DPI for print workflows. If you use this, verify IronPDF's rasterization capabilities meet your DPI requirements.
- **COS-level manipulation:** If your code directly manipulates the COS object tree (the raw PDF internal structure), this cannot be ported. Evaluate whether the same outcome can be achieved through IronPDF's higher-level API or HTML-based generation.

---

## Migration Checklist

### Pre-Migration (8 items)

- [ ] Run `rg` to count PDE/COS-level API usage vs. higher-level document operations
- [ ] Classify codebase: what percentage is HTML-to-PDF vs. low-level PDF construction?
- [ ] Identify XFA form dependencies (requires Forms Extension—no IronPDF equivalent)
- [ ] Obtain IronPDF trial key from [ironpdf.com/get-started/license-keys/](https://ironpdf.com/get-started/license-keys/)
- [ ] Create a migration branch
- [ ] Document any `wkhtmltopdf` or headless Chrome dependencies used alongside APDFL
- [ ] List PDF/A, PDF/X, or PDF/UA compliance requirements
- [ ] Inventory all Datalogics NuGet packages and native runtime dependencies

### Code Migration (10 items)

- [ ] Remove all Datalogics NuGet packages
- [ ] Add `IronPdf` NuGet package
- [ ] Remove `Library` initialization objects and their lifecycle scope
- [ ] Replace all `Datalogics.APDFL.*` namespace imports with `IronPdf`
- [ ] Replace `IronPdf.License.LicenseKey = "..."` for licensing
- [ ] Convert PDE-level PDF construction to HTML templates + `ChromePdfRenderer`
- [ ] Replace external HTML-to-PDF tool calls with `RenderHtmlAsPdf()`
- [ ] Replace `InsertPages` merge pattern with `PdfDocument.Merge()`
- [ ] Replace PDE watermark drawing with `ApplyWatermark()` HTML
- [ ] Replace security params with `SecuritySettings` properties

### Testing (7 items)

- [ ] Compare PDF output visually for your top 5 most complex templates
- [ ] Verify PDF/A conformance output if required (use a PDF validator like veraPDF)
- [ ] Test AcroForm filling if applicable
- [ ] Test password-protected PDF round-trip
- [ ] Test merge with documents from different sources
- [ ] Load-test rendering throughput vs. the APDFL + external tool pipeline
- [ ] Validate on target deployment platform (especially Linux/Docker)

### Post-Migration (4 items)

- [ ] Uninstall `wkhtmltopdf` or headless Chrome from servers if no longer needed
- [ ] Remove Datalogics native runtime packages from Docker images
- [ ] Update architecture documentation—the "PDF generation" box just got simpler
- [ ] Monitor production for two weeks, comparing PDF quality and error rates

---

## Wrapping Up

Six years of APDFL taught our team a lot about the PDF specification. That knowledge doesn't go away—it makes you better at debugging any PDF library's output. But the daily reality of constructing compliance reports in content streams when we could express the same thing in HTML and CSS was a cost we no longer needed to pay.

If your situation is similar—if most of your PDF work is generation from web content with some manipulation bolted on—the migration to an HTML-first library saves real engineering hours. If your situation involves deep PDF specification work (XFA, PDF/UA tagging, COS-level surgery), APDFL earns its complexity.

**Worth knowing even without IronPDF:** if you are currently shelling out to `wkhtmltopdf` alongside APDFL, you are running a deprecated, WebKit-based renderer that has known security vulnerabilities and stopped receiving updates. Regardless of where you migrate, replacing that subprocess should be a priority. Puppeteer Sharp and Playwright are both solid alternatives if you want to stay open-source.

Here is my question for the comments: if you are using the Adobe PDF Library's PDE layer for programmatic PDF construction—not HTML conversion, but actually building pages from graphic elements—what does the output look like? Is it CAD-style technical drawings, barcode label sheets, or something else entirely? I am curious what use cases genuinely need that level of control.

---

