# From Kaizen.io to IronPDF: what actually changes in your code

The version pin in your `.csproj` has been sitting at `2.x` for eight months. A security advisory came through, the fix landed in `3.x`, and the upgrade path involves breaking API changes you haven't had time to assess. Meanwhile the rest of the stack has moved to .NET 8 and the old package doesn't play cleanly with the new runtime. You're not looking for a migration out of enthusiasm — you're looking for a path forward.

This article maps the code changes needed to move from Kaizen.io PDF generation to IronPDF. The mapping tables and checklist are useful reference material regardless of which library you end up choosing.

---

## Why migrate (without drama)

Version pinning and upgrade friction are common triggers, but they're rarely the only one. Here are 9 neutral reasons teams evaluate a PDF library switch:

1. **Version lock** — security or compatibility fixes land in a version that requires breaking API changes. The cost of staying starts to exceed the cost of switching.
2. **Runtime incompatibility** — older libraries built for .NET Framework or early .NET Core may not work correctly on .NET 6/8 targets.
3. **Maintenance signal** — slowing commit cadence, unanswered GitHub issues, or no roadmap communication.
4. **NuGet package deprecation** — if the package is marked deprecated or abandoned on NuGet, your supply chain tooling will start flagging it.
5. **Deployment constraints** — libraries with native binary dependencies become friction in Docker, Azure, or CI/CD environments as those environments evolve.
6. **HTML/CSS fidelity** — if the rendering engine doesn't keep pace with CSS standards, templates start to drift visually.
7. **Missing features** — PDF/A compliance, digital signatures, merge/split, or watermarking absent from the current library.
8. **API verbosity** — accumulated boilerplate from a library that wasn't designed for your primary use case.
9. **Commercial support requirements** — teams needing SLA-backed support may need a library with an enterprise support tier.

### Comparison table

| Aspect | Kaizen.io | IronPDF |
|---|---|---|
| Focus | Verify — document/PDF tooling | HTML-to-PDF + full PDF manipulation |
| Pricing | Verify at kaizen.io | Commercial — verify at ironsoftware.com |
| API Style | Verify — library vs HTTP client | In-process .NET library |
| Learning Curve | Verify | Medium |
| HTML Rendering | Verify rendering engine | Chromium-based |
| Page Indexing | Verify | 0-based |
| Thread Safety | Verify | Renderer instance reuse — see async docs |
| Namespace | Verify | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | Kaizen.io approach | Effort to migrate |
|---|---|---|
| HTML string to PDF | Verify | Low (if supported) |
| URL to PDF | Verify | Low |
| HTML file to PDF | Verify | Low |
| Merge PDFs | Verify | Low (native in IronPDF) |
| Watermark | Verify | Low |
| Password protection | Verify | Low |
| Custom margins | Verify | Low |
| Headers / footers | Verify | Medium |
| Async rendering | Verify | Medium |
| Digital signatures | Verify | Medium — verify IronPDF signing API |
| PDF/A compliance | Verify | Low in IronPDF if needed |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| Version upgrade path blocked, .NET 8 required | Evaluate in-process alternatives; IronPDF or open-source options |
| Open source requirement | IronPDF is commercial; evaluate PuppeteerSharp, wkhtmltopdf wrappers |
| Core use case is HTML-to-PDF only, no manipulation | Both may work — verify Kaizen.io's HTML fidelity against your templates |
| Need merge, watermark, encryption in one library | IronPDF consolidates these; avoids adding secondary libraries |

---

## Before you start

### Prerequisites

- .NET 6+ target framework
- NuGet access or offline package cache
- Full inventory of Kaizen.io features in active use (critical before removing it)

### Find Kaizen.io references in your codebase

```bash
# Find all files referencing Kaizen — adjust namespace/class names after verifying docs
rg -l "Kaizen\|kaizen" --type cs -i

# Find using statements
rg "using Kaizen" --type cs -n

# Find class instantiation — replace KaizenClient with actual class name
rg "KaizenClient\|KaizenPdf\|KaizenRenderer" --type cs -n

# Find NuGet package references in project files
grep -r -i "kaizen" **/*.csproj *.csproj 2>/dev/null
```

### Remove Kaizen.io, install IronPDF

```bash
# Remove Kaizen package — verify exact package name on NuGet first
dotnet remove package Kaizen.Pdf   # replace with actual package name

# Install IronPDF
dotnet add package IronPdf

# Restore
dotnet restore
```

On Windows PowerShell via Package Manager Console: `Install-Package IronPdf` and `Uninstall-Package Kaizen.Pdf` (replace with actual package name).

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (Kaizen.io — verify license initialization in docs):**
```csharp
// Kaizen.io license setup — verify exact method and class names in official docs
// Pattern below is illustrative; do not use without verification
using Kaizen.Pdf; // verify namespace

// Verify whether Kaizen requires license initialization and how
// KaizenLicense.SetKey("YOUR_KEY"); // hypothetical — verify before use
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
// Verify actual namespaces from Kaizen.io documentation
using Kaizen.Pdf;        // verify
using Kaizen.Pdf.Options; // verify — may not exist
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;  // for ChromePdfRenderOptions
using IronPdf.Editing;    // for stamping/watermark
using IronPdf.Security;   // for passwords
```

### Step 3: Basic HTML-to-PDF

**Before:**
```csharp
// IMPORTANT: The code below uses placeholder names.
// Verify every class, method, and property name against actual Kaizen.io documentation
// before using this in production.

using System;
// using Kaizen.Pdf; // verify namespace

class BasicConversionExample
{
    static void Main()
    {
        // Verify: class name, constructor, method names
        // var converter = new KaizenPdfConverter(); // hypothetical
        // var result = converter.ConvertFromHtml("<h1>Hello</h1>"); // hypothetical
        // File.WriteAllBytes("output.pdf", result); // hypothetical

        // Replace above with actual Kaizen.io API — verify in docs
        Console.WriteLine("Verify Kaizen.io API before implementing");
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

| Kaizen.io | IronPDF | Notes |
|---|---|---|
| `Kaizen.Pdf` | `IronPdf` | Core namespace |
| Options namespace | `IronPdf.Rendering` | Render configuration |
| N/A | `IronPdf.Editing` | Manipulation — stamp, annotate |

### Core class mapping

| Kaizen.io class | IronPDF class | Description |
|---|---|---|
| Converter class | `ChromePdfRenderer` | PDF generation entry point |
| Options/settings class | `ChromePdfRenderOptions` | Render configuration |
| Document class | `PdfDocument` | Represents a loaded/rendered PDF |
| N/A | `PdfDocument` static methods | Merge, split, load from file |

### Document loading methods

| Operation | Kaizen.io | IronPDF |
|---|---|---|
| HTML string | Verify method name | `renderer.RenderHtmlAsPdf(html)` |
| URL | Verify method name | `renderer.RenderUrlAsPdf(url)` |
| HTML file | Verify method name | `renderer.RenderHtmlFileAsPdf(path)` |
| Existing PDF | Verify method name | `PdfDocument.FromFile(path)` |

### Page operations

| Operation | Kaizen.io | IronPDF |
|---|---|---|
| Page count | Verify | `pdf.PageCount` |
| Paper size | Verify | `ChromePdfRenderOptions.PaperSize` |
| Margins | Verify | `ChromePdfRenderOptions.Margin*` |
| Orientation | Verify | `ChromePdfRenderOptions.PaperOrientation` |

### Merge/split operations

| Operation | Kaizen.io | IronPDF |
|---|---|---|
| Merge | Verify | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | Verify | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

> **Note on "Before" blocks:** Because Kaizen.io's API cannot be verified from public documentation at the time of writing, the Before blocks show structural patterns common to PDF libraries in this category, with explicit verification reminders. Replace every method and class name with the actual Kaizen.io API before using.

### 1. HTML to PDF

**Before (structural pattern — verify all names against Kaizen.io docs):**
```csharp
using System;
using System.IO;
// using Kaizen.Pdf; // uncomment and verify namespace

class HtmlToPdfExample
{
    static void Main()
    {
        // VERIFY: All class names, method signatures, and option properties
        // below are placeholders. Do not ship without consulting Kaizen.io docs.

        // Step 1: Instantiate converter — verify class name
        // var converter = new KaizenConverter(); // hypothetical

        // Step 2: Configure options — verify option class and properties
        // var options = new KaizenOptions
        // {
        //     PaperSize = "A4",              // verify property name and value type
        //     MarginTop = "10mm",            // verify
        //     MarginBottom = "10mm",         // verify
        // };

        // Step 3: Convert — verify method signature
        // byte[] pdfBytes = converter.HtmlToPdf(
        //     "<html><body><h1>Invoice</h1></body></html>",
        //     options
        // );

        // Step 4: Save output
        // File.WriteAllBytes("invoice.pdf", pdfBytes);
        Console.WriteLine("Replace placeholder code with actual Kaizen.io API");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;

var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Invoice #1234</h1><p>Amount: $500</p></body></html>"
);
pdf.SaveAs("invoice.pdf");
// Full guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 2. Merge PDFs

**Before:**
```csharp
using System;
using System.IO;
// If Kaizen.io doesn't support merge natively, teams typically add PdfSharp or similar

// PdfSharp pattern (common workaround — verify if applicable):
// using PdfSharp.Pdf;
// using PdfSharp.Pdf.IO;

class MergePdfsExample
{
    static void Main()
    {
        // VERIFY: Does Kaizen.io support PDF merge natively?
        // If yes — use their API (verify class/method names)
        // If no — a secondary library is typically added

        // Common PdfSharp workaround pattern:
        // using var output = new PdfDocument();
        // foreach (var path in new[] { "part1.pdf", "part2.pdf" })
        // {
        //     using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
        //     foreach (PdfPage page in input.Pages)
        //         output.AddPage(page);
        // }
        // output.Save("merged.pdf");

        Console.WriteLine("Verify Kaizen.io merge API or secondary library before implementing");
    }
}
```

**After (IronPDF native merge):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf1 = PdfDocument.FromFile("part1.pdf");
var pdf2 = PdfDocument.FromFile("part2.pdf");
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before:**
```csharp
using System;
using System.IO;
// using iTextSharp.text;        // common secondary library
// using iTextSharp.text.pdf;

class WatermarkExample
{
    static void Main()
    {
        // VERIFY: Does Kaizen.io support watermarking natively?
        // If not, iTextSharp PdfStamper pattern is common:

        // using var reader = new PdfReader("input.pdf");
        // using var fs = new FileStream("watermarked.pdf", FileMode.Create);
        // using var stamper = new PdfStamper(reader, fs);
        // var font = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
        // var cb = stamper.GetOverContent(1);
        // cb.BeginText();
        // cb.SetFontAndSize(font, 50);
        // cb.SetColorFill(new BaseColor(200, 200, 200));
        // cb.ShowTextAligned(Element.ALIGN_CENTER, "DRAFT", 300, 400, 45);
        // cb.EndText();

        Console.WriteLine("Verify Kaizen.io watermark API before implementing");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

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

**Before:**
```csharp
using System;
using System.IO;
// using iTextSharp.text.pdf; // common secondary library if not native

class SecurityExample
{
    static void Main()
    {
        // VERIFY: Does Kaizen.io support PDF encryption natively?
        // If not, iTextSharp pattern is typical:

        // byte[] userPass = System.Text.Encoding.ASCII.GetBytes("userpass");
        // byte[] ownerPass = System.Text.Encoding.ASCII.GetBytes("ownerpass");
        // using var reader = new PdfReader("input.pdf");
        // using var fs = new FileStream("secured.pdf", FileMode.Create);
        // using var stamper = new PdfStamper(reader, fs, '\0', false);
        // stamper.SetEncryption(userPass, ownerPass,
        //     PdfWriter.ALLOW_PRINTING, PdfWriter.ENCRYPTION_AES_128);

        Console.WriteLine("Verify Kaizen.io security API before implementing");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Security;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf = PdfDocument.FromFile("input.pdf");
pdf.SecuritySettings.UserPassword = "userpass";
pdf.SecuritySettings.OwnerPassword = "ownerpass";
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Page indexing

IronPDF uses 0-based page indexing. Verify Kaizen.io's page indexing convention in its documentation, then audit all page index references in your code:

```csharp
// IronPDF: 0-based
var firstPage = pdf.Pages[0]; // first page
var lastPage  = pdf.Pages[pdf.PageCount - 1]; // last page
```

### Output model difference

Many PDF libraries return `byte[]` from conversion. IronPDF returns a `PdfDocument` object. Update call sites that expect raw bytes:

```csharp
// If your code expected byte[] from Kaizen.io:
// byte[] pdfBytes = converter.Convert(html); // old pattern

// IronPDF returns PdfDocument — save or stream as needed:
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");

// Or to MemoryStream:
// https://ironpdf.com/how-to/pdf-memory-stream/
using var ms = new MemoryStream();
pdf.Stream.CopyTo(ms);
byte[] pdfBytes = ms.ToArray();
```

### Error handling

IronPDF throws exceptions on failure. If Kaizen.io returned status codes or null on error, update your error handling:

```csharp
try
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
catch (IronPdf.Exceptions.IronPdfException ex)
{
    // Log render failure
    Console.Error.WriteLine($"Render failed: {ex.Message}");
}
```

---

## Performance considerations

### Renderer reuse

```csharp
// Instantiate once, reuse for batch work
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

foreach (var html in templates)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"output_{Guid.NewGuid()}.pdf");
}
```

### Concurrent rendering

```csharp
// Separate renderer per thread — don't share across concurrent operations
Parallel.ForEach(htmlBatch, html =>
{
    var renderer = new ChromePdfRenderer();
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"{Guid.NewGuid()}.pdf");
});
// Parallel examples: https://ironpdf.com/examples/parallel/
```

### Disposal

```csharp
// PdfDocument implements IDisposable
using var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
// Auto-disposed at end of block
```

### Edge cases

- **Cold start:** Chromium-based renderers have a startup cost. In serverless or per-request scaling scenarios, the first render will be slower. Pre-warm if latency is critical.
- **Font availability:** System fonts must be present on the render server. Verify font availability in Docker images.
- **Memory:** Profile under production-representative load before sizing containers.

---

## Migration checklist

### Pre-migration

- [ ] Verify Kaizen.io NuGet package name and exact version in use
- [ ] Review Kaizen.io changelog — understand what changed between your pinned version and current
- [ ] Decide: upgrade within Kaizen.io vs switch — document the rationale
- [ ] Inventory all Kaizen.io API calls in use: `rg "Kaizen\|kaizen" --type cs -i`
- [ ] Identify any secondary libraries added to supplement Kaizen.io (merge, watermark, security)
- [ ] Verify IronPDF .NET target framework compatibility
- [ ] Confirm commercial license requirements internally
- [ ] Set up IronPDF trial license in dev environment

### Code migration

- [ ] Remove Kaizen.io NuGet package
- [ ] Add `IronPdf` NuGet package
- [ ] Replace Kaizen.io using directives with `using IronPdf` etc.
- [ ] Replace license initialization
- [ ] Replace HTML-to-PDF calls with `ChromePdfRenderer`
- [ ] Replace URL-to-PDF calls
- [ ] Replace merge operations (native in IronPDF — remove secondary library if used only for this)
- [ ] Replace watermark operations
- [ ] Replace password/security operations
- [ ] Update output handling from `byte[]` to `PdfDocument` model where needed

### Testing

- [ ] Render each HTML template and visually compare output
- [ ] Verify page count matches expected for multi-page documents
- [ ] Test merge with representative document sets
- [ ] Test watermark on single-page and multi-page documents
- [ ] Test password protection with correct and incorrect credentials
- [ ] Test async/concurrent rendering at expected peak load
- [ ] Regression test all PDF-generating endpoints

### Post-migration

- [ ] Remove Kaizen.io license key from config / secrets management
- [ ] Remove secondary libraries no longer needed
- [ ] Update deployment documentation
- [ ] Monitor memory and CPU in first production week

---

## Final Thoughts

The variability in this migration depends almost entirely on how much of Kaizen.io's API surface your code touches and whether there's a secondary library doing the manipulation work. Single-concern codebases (HTML-to-PDF only) migrate in a few hours. Codebases where manipulation features are scattered across multiple libraries take longer to consolidate.

**What edge cases did you hit in this migration that the article didn't cover?** Particularly interested in teams who had Kaizen.io integrated into background job pipelines or report scheduling systems — those setups have extra steps not covered here.

---

*Tags: `dotnet`, `csharp`, `pdf`, `migration`*
