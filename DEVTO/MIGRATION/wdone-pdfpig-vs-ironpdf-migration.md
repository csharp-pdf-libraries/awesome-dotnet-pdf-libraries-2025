# Dropping PdfPig for IronPDF: a .NET migration that fits in an afternoon

The unit tests pass locally. The Docker build finishes clean. Then the CI pipeline hits the `dotnet test` step and a handful of PDF-parsing tests start intermittently failing with `System.IO.IOException: The process cannot access the file` or a threading assertion in the PdfPig internals. It works on your machine because you run tests serially. The pipeline runs them in parallel. PdfPig's document model isn't designed for the way your CI runner dispatches test jobs.

This article covers the migration from PdfPig to IronPDF, with a troubleshooting focus on the concurrency and deployment patterns that most commonly surface during CI/CD integration. The checklist and mapping tables are useful regardless of which library you choose.

---

## Diagnosing the CI/CD concurrency failure

PdfPig opens a `PdfDocument` via `PdfDocument.Open()` which holds a file handle. In a parallel test environment, multiple test instances accessing the same PDF fixture can step on each other.

**Symptom: `IOException` on file access in parallel tests**

```bash
# Typical error in CI log:
# System.IO.IOException: The process cannot access the file 'test.pdf'
# because it is being used by another process.

# Verify test parallelism settings in your runner:
cat .runsettings 2>/dev/null | grep -i parallel
cat xunit.runner.json 2>/dev/null

# In xUnit — check for missing [Collection] attribute isolating PDF tests:
rg "\[Fact\]\|\[Theory\]" --type cs -n | grep -i pdf
```

**Fix within PdfPig (without migrating):**

```csharp
// Option 1: Open from bytes instead of file — eliminates file handle contention
byte[] pdfBytes = File.ReadAllBytes("test.pdf");
using var doc = PdfDocument.Open(pdfBytes);
// No file handle held — safe for parallel tests

// Option 2: Copy fixture per test — isolation at file level
string tempPath = Path.GetTempFileName() + ".pdf";
File.Copy("test.pdf", tempPath);
using var doc = PdfDocument.Open(tempPath);
// Clean up after test
File.Delete(tempPath);
```

If these fixes resolve your CI failure, the migration may not be necessary. If the issue is structural — PdfPig doesn't support HTML generation and you need both parsing and generation — the migration has a clearer motivation.

---

## Why migrate (without drama)

Nine reasons teams evaluate alternatives to PdfPig:

1. **HTML-to-PDF not supported** — PdfPig is a reader, not a renderer. Teams needing HTML-to-PDF add a second library (wkhtmltopdf, NReco, etc.), which adds maintenance overhead.
2. **Document manipulation limited** — merging, splitting, watermarking, and encrypting PDFs are not PdfPig's scope. Secondary libraries fill these gaps.
3. **Dependency sprawl** — a codebase with PdfPig + wkhtmltopdf + PdfSharp + iTextSharp is handling the same domain across four packages.
4. **Parallel test isolation** — file handle management in concurrent test environments requires explicit workarounds.
5. **Feature convergence** — if your requirements have grown beyond parsing into generation and manipulation, a library covering all three simplifies the stack.
6. **API verbosity for simple reads** — PdfPig's word/letter model is powerful for layout analysis but verbose for simple text extraction tasks.
7. **Cloud/serverless rendering** — adding HTML-to-PDF to a PdfPig-based service requires a second deployment concern (wkhtmltopdf binary, Puppeteer, etc.).
8. **Maintenance overhead** — tracking two or more PDF libraries through version updates is twice the work.
9. **HTML content processing** — teams with HTML templates for reports need generation capabilities PdfPig doesn't provide.

### Comparison table

| Aspect | PdfPig | IronPDF |
|---|---|---|
| Focus | PDF parsing + text extraction | HTML-to-PDF + PDF manipulation + extraction |
| Pricing | Open source (Apache 2.0) | Commercial — verify at ironsoftware.com |
| API Style | Document/page/word object model | HTML renderer + document model |
| Learning Curve | Medium (layout analysis model) | Medium |
| HTML Rendering | Not supported | Chromium-based |
| Page Indexing | 0-based | 0-based |
| Thread Safety | File handle isolation required | Renderer instance reuse |
| Namespace | `UglyToad.PdfPig` | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | PdfPig approach | Effort to migrate |
|---|---|---|
| Text extraction (full doc) | `doc.GetPages()` + words | Low |
| Text extraction (per page) | `page.GetWords()` | Low |
| Word-level layout analysis | PdfPig native strength | High — different model in IronPDF |
| Font inspection | `page.Letters` / `word.Font` | Medium-High — verify IronPDF equivalent |
| HTML to PDF | Not supported | Low (native in IronPDF) |
| Merge PDFs | Not PdfPig scope | Low (native in IronPDF) |
| Watermark | Not PdfPig scope | Low |
| Password protection | Not PdfPig scope | Low |
| Page count / metadata | `doc.NumberOfPages` | Low |
| Annotation reading | PdfPig supports — verify | Medium — verify IronPDF annotations |
| Open source requirement | Apache 2.0 | IronPDF is commercial |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| Primary use: precise word/letter layout analysis | PdfPig is strong here; IronPDF has different focus |
| Need HTML-to-PDF + basic text extraction | IronPDF covers both; eliminates secondary library |
| Open source budget constraint | PdfPig is Apache 2.0; IronPDF is commercial — evaluate DFM, iText 7 |
| Dependency consolidation | IronPDF replaces PdfPig + secondary HTML/manipulation libraries |

---

## Before you start

### Prerequisites

- .NET 6+ target
- PdfPig feature inventory: text extraction vs layout analysis vs parsing
- HTML templates (if HTML-to-PDF is the new requirement)

### Find PdfPig references in your codebase

```bash
# Find all PdfPig usage
rg -l "PdfPig\|UglyToad" --type cs

# Find using statements
rg "using UglyToad\.PdfPig" --type cs -n

# Find document open patterns
rg "PdfDocument\.Open\|\.GetPages\|\.GetWords\|\.GetLetters" --type cs -n

# Find text extraction calls
rg "\.Text\b\|GetWords\(\)\|GetLetters\(\)" --type cs -n | grep -i pdf

# Find NuGet package references
grep -r -i "pdfpig\|uglytoad" **/*.csproj *.csproj 2>/dev/null
```

### Remove PdfPig, install IronPDF

```bash
# Remove PdfPig
dotnet remove package PdfPig

# Also remove secondary libraries if added only to supplement PdfPig:
# dotnet remove package NReco.PdfGenerator  # if added for HTML-to-PDF
# dotnet remove package PdfSharp            # if added for merge/manipulation

# Install IronPDF
dotnet add package IronPdf
dotnet restore
```

On Windows via Package Manager Console: `Install-Package IronPdf` and `Uninstall-Package PdfPig`.

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (PdfPig — open source, no license key):**
```csharp
using UglyToad.PdfPig;
// No license initialization needed — Apache 2.0 open source
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
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;  // for ChromePdfRenderOptions
using IronPdf.Editing;    // for TextStamper
using IronPdf.Security;   // for SecuritySettings
```

### Step 3: Basic text extraction

**Before (PdfPig):**
```csharp
using UglyToad.PdfPig;
using System;

using var doc = PdfDocument.Open("invoice.pdf");
foreach (var page in doc.GetPages())
{
    string pageText = page.Text;
    Console.WriteLine($"Page {page.Number}: {pageText.Length} chars");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf = PdfDocument.FromFile("invoice.pdf");
string allText = pdf.ExtractAllText();
Console.WriteLine($"Total text: {allText.Length} chars");
// Per-page: pdf.Pages[n].Text (0-based)
// Guide: https://ironpdf.com/how-to/extract-text-and-images/
```

---

## Troubleshooting: common migration failures

### Problem: "Word-level layout data isn't available in IronPDF"

PdfPig's word/letter model is its primary strength for layout analysis:

```csharp
// PdfPig: word-level access with bounding boxes, fonts, and positions
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

using var doc = PdfDocument.Open("document.pdf");
foreach (var page in doc.GetPages())
{
    foreach (Word word in page.GetWords())
    {
        Console.WriteLine($"'{word.Text}' at ({word.BoundingBox.Left:F1}, " +
                          $"{word.BoundingBox.Bottom:F1})");
    }
}
```

IronPDF's extraction model is text-focused rather than layout-focused. If bounding box data per word is essential to your application, PdfPig may be the better fit for that specific operation. For teams who need layout analysis AND HTML-to-PDF generation, keeping PdfPig for parsing while using IronPDF for generation is a valid architecture:

```csharp
// Partial migration: PdfPig for layout analysis, IronPDF for generation
// Keep both packages for different responsibilities

// PdfPig: extract structured text with positions
using (var doc = UglyToad.PdfPig.PdfDocument.Open("input.pdf"))
{
    var words = doc.GetPages()
        .SelectMany(p => p.GetWords())
        .Select(w => new { w.Text, w.BoundingBox })
        .ToList();
    // ... process layout data
}

// IronPDF: generate new PDF from HTML
IronPdf.License.LicenseKey = "YOUR_KEY";
var renderer = new ChromePdfRenderer();
var output = renderer.RenderHtmlAsPdf(generatedHtml);
output.SaveAs("output.pdf");
```

### Problem: "PdfPig page numbers are 1-based; IronPDF is 0-based"

```csharp
// PdfPig: pages are 1-based
using var doc = UglyToad.PdfPig.PdfDocument.Open("input.pdf");
var firstPage = doc.GetPage(1);  // page number 1 = first page

// IronPDF: pages are 0-based
var pdf = IronPdf.PdfDocument.FromFile("input.pdf");
var firstPage = pdf.Pages[0];    // index 0 = first page

// Migration pattern — update all page index references:
// PdfPig page N → IronPDF page index N-1
```

### Problem: "IronPDF text extraction output differs from PdfPig"

Different extraction implementations can produce different results on edge cases (column-based layouts, right-to-left text, ligatures):

```csharp
// Run comparison on your document corpus before committing
using var pdfPigDoc = UglyToad.PdfPig.PdfDocument.Open("test.pdf");
string pdfPigText = string.Join("\n",
    pdfPigDoc.GetPages().Select(p => p.Text));

IronPdf.License.LicenseKey = "YOUR_KEY";
var ironPdf = IronPdf.PdfDocument.FromFile("test.pdf");
string ironPdfText = ironPdf.ExtractAllText();

// Compare character counts as a quick signal
Console.WriteLine($"PdfPig:  {pdfPigText.Length} chars");
Console.WriteLine($"IronPDF: {ironPdfText.Length} chars");
// Deep diff on representative documents before migration
```

### Problem: "Encrypted PDF handling differs"

```csharp
// PdfPig: open with password
using var doc = UglyToad.PdfPig.PdfDocument.Open("secured.pdf",
    new ParsingOptions { Password = "yourpassword" });

// IronPDF: FromFile with password
var pdf = IronPdf.PdfDocument.FromFile("secured.pdf", "yourpassword");
```

---

## API mapping tables

### Namespace mapping

| PdfPig | IronPDF | Notes |
|---|---|---|
| `UglyToad.PdfPig` | `IronPdf` | Core document operations |
| `UglyToad.PdfPig.Content` | `IronPdf.Editing` | Content/manipulation |
| `UglyToad.PdfPig.DocumentLayoutAnalysis` | N/A | Layout analysis — no IronPDF equivalent |

### Core class mapping

| PdfPig class | IronPDF class | Description |
|---|---|---|
| `PdfDocument` (PdfPig) | `PdfDocument` (IronPDF) | Document representation — different API |
| `Page` | `pdf.Pages[n]` | Page access |
| `Word` | N/A | Word-level model — not in IronPDF |
| N/A | `ChromePdfRenderer` | HTML-to-PDF generation |

### Document loading methods

| Operation | PdfPig | IronPDF |
|---|---|---|
| Open from file | `PdfDocument.Open(path)` | `PdfDocument.FromFile(path)` |
| Open from bytes | `PdfDocument.Open(bytes)` | `PdfDocument.FromStream(stream)` |
| Open with password | `PdfDocument.Open(path, new ParsingOptions { Password = "..." })` | `PdfDocument.FromFile(path, password)` |
| HTML to PDF | Not supported | `renderer.RenderHtmlAsPdf(html)` |

### Page operations

| Operation | PdfPig | IronPDF |
|---|---|---|
| Page count | `doc.NumberOfPages` | `pdf.PageCount` |
| Access page | `doc.GetPage(n)` (1-based) | `pdf.Pages[n]` (0-based) |
| Extract text | `page.Text` | `pdf.Pages[n].Text` |
| Page dimensions | `page.Width` / `page.Height` | `pdf.Pages[n].Width` / `.Height` |

### Merge/split operations

| Operation | PdfPig | IronPDF |
|---|---|---|
| Merge | Not in scope — PdfPig is read-only | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | Not in scope | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

### 1. Text extraction (PdfPig's primary use case)

**Before (PdfPig — full text extraction per page):**
```csharp
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System;
using System.Text;

class TextExtractionExample
{
    static void Main()
    {
        using var doc = PdfDocument.Open("invoice.pdf");

        Console.WriteLine($"PDF has {doc.NumberOfPages} pages");

        var fullText = new StringBuilder();

        // PdfPig: 1-based page access via GetPages()
        foreach (Page page in doc.GetPages())
        {
            Console.WriteLine($"\n--- Page {page.Number} ---");

            // page.Text: concatenated text content
            fullText.AppendLine(page.Text);
            Console.WriteLine(page.Text);

            // Word-level access with position data:
            int wordCount = 0;
            foreach (Word word in page.GetWords())
            {
                wordCount++;
                // word.Text, word.BoundingBox.Left, word.BoundingBox.Bottom
            }
            Console.WriteLine($"({wordCount} words)");
        }

        System.IO.File.WriteAllText("extracted.txt", fullText.ToString());
        Console.WriteLine("\nExtracted to: extracted.txt");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;
using System.IO;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf = PdfDocument.FromFile("invoice.pdf");
Console.WriteLine($"PDF has {pdf.PageCount} pages");

// Full document text — one call
string allText = pdf.ExtractAllText();
File.WriteAllText("extracted.txt", allText);
Console.WriteLine($"Extracted {allText.Length} chars to: extracted.txt");

// Per-page access (0-based):
for (int i = 0; i < pdf.PageCount; i++)
    Console.WriteLine($"Page {i + 1}: {pdf.Pages[i].Text.Length} chars");

// Guide: https://ironpdf.com/how-to/extract-text-and-images/
```

---

### 2. HTML to PDF (PdfPig doesn't support this — secondary tool migration)

**Before (wkhtmltopdf subprocess, commonly added alongside PdfPig for generation):**
```csharp
using System;
using System.Diagnostics;
using System.IO;

class HtmlToPdfWithSecondaryTool
{
    static void Main()
    {
        // PdfPig reads PDFs; it doesn't generate them
        // Teams add wkhtmltopdf or NReco for HTML-to-PDF generation
        string html    = "<html><body><h1>Report</h1><p>Data here</p></body></html>";
        string tmpHtml = Path.GetTempFileName() + ".html";
        string tmpPdf  = Path.GetTempFileName() + ".pdf";

        File.WriteAllText(tmpHtml, html);

        var psi = new ProcessStartInfo("wkhtmltopdf", $"\"{tmpHtml}\" \"{tmpPdf}\"")
        {
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow  = true
        };

        using var proc = Process.Start(psi)!;
        if (!proc.WaitForExit(30_000))
        {
            proc.Kill();
            throw new TimeoutException("wkhtmltopdf timed out");
        }
        if (proc.ExitCode != 0)
            throw new Exception($"wkhtmltopdf error: {proc.StandardError.ReadToEnd()}");

        byte[] pdfBytes = File.ReadAllBytes(tmpPdf);
        File.Delete(tmpHtml);
        File.Delete(tmpPdf);
        Console.WriteLine($"Generated {pdfBytes.Length} bytes");
    }
}
```

**After (IronPDF — replaces secondary HTML-to-PDF tool):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

var pdf = renderer.RenderHtmlAsPdf(
    "<html><body><h1>Report</h1><p>Data here</p></body></html>"
);
pdf.SaveAs("report.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

### 3. Merge PDFs (PdfPig doesn't support — secondary library migration)

**Before (PdfSharp secondary library, common addition to PdfPig-based projects):**
```csharp
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;

class MergePdfsExample
{
    static void Main()
    {
        // PdfPig can't merge PDFs — read-only library
        // PdfSharp is a common addition
        using var output = new PdfDocument();

        foreach (string path in new[] { "chapter1.pdf", "chapter2.pdf", "chapter3.pdf" })
        {
            using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            foreach (PdfPage page in input.Pages)
                output.AddPage(page);
        }

        output.Save("full_document.pdf");
        Console.WriteLine("Merged to: full_document.pdf");
    }
}
```

**After (IronPDF native — eliminates PdfSharp dependency):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var merged = PdfDocument.Merge(
    PdfDocument.FromFile("chapter1.pdf"),
    PdfDocument.FromFile("chapter2.pdf"),
    PdfDocument.FromFile("chapter3.pdf")
);
merged.SaveAs("full_document.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 4. Watermark (PdfPig doesn't support — secondary library migration)

**Before (iTextSharp secondary library, commonly added alongside PdfPig):**
```csharp
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;

class WatermarkExample
{
    static void Main()
    {
        // PdfPig is read-only — watermarking requires a secondary library
        using var reader  = new PdfReader("document.pdf");
        using var fs      = new FileStream("watermarked.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs);

        var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);

        for (int page = 1; page <= reader.NumberOfPages; page++)
        {
            var cb = stamper.GetOverContent(page);
            cb.SaveState();
            cb.SetGState(new PdfGState { FillOpacity = 0.3f });
            cb.BeginText();
            cb.SetFontAndSize(baseFont, 60);
            cb.SetColorFill(BaseColor.GRAY);
            cb.ShowTextAligned(Element.ALIGN_CENTER, "DRAFT", 300, 420, 45);
            cb.EndText();
            cb.RestoreState();
        }

        Console.WriteLine("Watermarked: watermarked.pdf");
    }
}
```

**After (IronPDF — eliminates iTextSharp dependency):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var pdf = PdfDocument.FromFile("document.pdf");
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

### 5. Password protection (PdfPig doesn't support writing — secondary library)

**Before (iTextSharp encryption alongside PdfPig):**
```csharp
using iTextSharp.text.pdf;
using System.IO;
using System.Text;

class SecurityExample
{
    static void Main()
    {
        // PdfPig can READ encrypted PDFs (with password)
        // but cannot WRITE encrypted PDFs — secondary library required
        byte[] userPass  = Encoding.ASCII.GetBytes("readonly");
        byte[] ownerPass = Encoding.ASCII.GetBytes("admin");

        using var reader  = new PdfReader("document.pdf");
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

var pdf = PdfDocument.FromFile("document.pdf");
pdf.SecuritySettings.UserPassword  = "readonly";
pdf.SecuritySettings.OwnerPassword = "admin";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Page indexing: 1-based → 0-based

This is the most common migration bug. PdfPig uses 1-based page numbering; IronPDF uses 0-based:

```csharp
// PdfPig: 1-based
using var doc = UglyToad.PdfPig.PdfDocument.Open("input.pdf");
var firstPage = doc.GetPage(1);  // 1 = first page
var lastPage  = doc.GetPage(doc.NumberOfPages); // last page

// IronPDF: 0-based
var pdf = IronPdf.PdfDocument.FromFile("input.pdf");
var firstPage = pdf.Pages[0];               // 0 = first page
var lastPage  = pdf.Pages[pdf.PageCount - 1]; // last page
```

Audit every `GetPage(n)` call and subtract 1 for the IronPDF equivalent.

### `NumberOfPages` → `PageCount`

```csharp
// PdfPig
int pages = doc.NumberOfPages;

// IronPDF
int pages = pdf.PageCount;
```

### Text extraction model

PdfPig's `page.Text` and IronPDF's `pdf.Pages[n].Text` both return extracted text, but their implementations differ. PdfPig uses spatial reading order; IronPDF uses the PDF content stream order. For complex multi-column layouts, test both on your representative documents:

```csharp
// Test extraction quality comparison:
var pdfPigText  = string.Join("\n", pdfPigDoc.GetPages().Select(p => p.Text));
var ironPdfText = ironPdfDoc.ExtractAllText();

// Simple quality signal: tokenize and compare unique word sets
var ppWords  = new HashSet<string>(pdfPigText.Split(' ', '\n'));
var ipWords  = new HashSet<string>(ironPdfText.Split(' ', '\n'));
var onlyInPP = ppWords.Except(ipWords).Count();
var onlyInIP = ipWords.Except(ppWords).Count();
Console.WriteLine($"Only in PdfPig: {onlyInPP} | Only in IronPDF: {onlyInIP}");
```

### Word/letter model gap

PdfPig exposes individual `Letter` and `Word` objects with geometric bounding boxes, fonts, and colours. IronPDF's extraction model is text-string focused. If your application logic depends on `word.BoundingBox.Left` or `letter.FontName`, that data is not directly available in IronPDF's extraction API — verify current IronPDF API surface before migrating code that uses this.

---

## Performance considerations

### PdfPig text extraction vs IronPDF

Both extract text from the PDF content stream. PdfPig's approach is fully managed .NET. IronPDF uses its Chromium-based internals for extraction. Profile on your actual document set:

```csharp
using System.Diagnostics;
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_KEY";
string[] testFiles = Directory.GetFiles("test_pdfs", "*.pdf");

var sw = Stopwatch.StartNew();
foreach (string path in testFiles)
{
    var pdf  = PdfDocument.FromFile(path);
    string _ = pdf.ExtractAllText();
}
sw.Stop();
Console.WriteLine($"IronPDF: {sw.ElapsedMilliseconds}ms for {testFiles.Length} docs");
// Run the equivalent PdfPig loop and compare
```

### Dispose PdfDocument after use

```csharp
// PdfPig: PdfDocument implements IDisposable — file handle held until dispose
using var doc = UglyToad.PdfPig.PdfDocument.Open("input.pdf");
// ... use doc ...
// Disposed here — file handle released

// IronPDF: also IDisposable
using var pdf = IronPdf.PdfDocument.FromFile("input.pdf");
// ... use pdf ...
```

### Renderer reuse for batch HTML-to-PDF

```csharp
// One renderer, many renders — efficient for batch work
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;

foreach (var html in templateBatch)
{
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs($"report_{Guid.NewGuid()}.pdf");
}
```

### Async for web

```csharp
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
return File(pdf.Stream, "application/pdf", "report.pdf");
// Async guide: https://ironpdf.com/how-to/async/
```

### Edge cases

- **Parallel test isolation:** If you migrate to IronPDF for text extraction, the file-handle contention issue is resolved — `PdfDocument.FromFile()` reads the file and closes the handle unless the document is held open.
- **Large PDFs:** Profile memory for large documents. PdfPig streams page data lazily; IronPDF loads the document into memory. Test with your largest representative files.
- **Right-to-left text:** Verify extraction accuracy on any RTL documents in your corpus — PdfPig and IronPDF may handle these differently.

---

## Migration checklist

### Pre-migration

- [ ] Inventory all PdfPig features in use: `rg "UglyToad\|PdfPig" --type cs -n`
- [ ] Distinguish word/letter layout analysis from simple text extraction
- [ ] Identify secondary libraries added alongside PdfPig (wkhtmltopdf, PdfSharp, iTextSharp)
- [ ] Verify IronPDF .NET target framework compatibility
- [ ] Confirm commercial license requirements internally
- [ ] Set up IronPDF trial license in dev environment
- [ ] Run side-by-side text extraction comparison on representative PDFs
- [ ] Document page indexing references: `rg "GetPage\|\.Pages\[" --type cs -n`

### Code migration

- [ ] Remove `PdfPig` NuGet package
- [ ] Remove secondary libraries (if supplementing PdfPig only)
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `using UglyToad.PdfPig` with `using IronPdf`
- [ ] Replace `PdfDocument.Open()` with `PdfDocument.FromFile()`
- [ ] Replace `doc.GetPages()` / `page.Text` with `pdf.Pages[n].Text` (0-based)
- [ ] Replace `doc.NumberOfPages` with `pdf.PageCount`
- [ ] Update all page index references: PdfPig page N → IronPDF index N-1
- [ ] Replace HTML-to-PDF secondary tool with `ChromePdfRenderer`
- [ ] Add IronPDF license key to config

### Testing

- [ ] Compare text extraction output on all PDF fixtures
- [ ] Verify page index updates (regression-test page-specific operations)
- [ ] Test encrypted PDF opening with password
- [ ] Test merge, watermark, security (if migrating from secondary libraries)
- [ ] Run parallel test suite — verify CI/CD failure mode is resolved
- [ ] Render HTML templates if HTML-to-PDF is a new requirement
- [ ] Test in Docker/CI environment

### Post-migration

- [ ] Remove secondary libraries no longer needed
- [ ] Update CI/CD pipeline documentation
- [ ] Commit updated `packages.lock.json` if using locked restores
- [ ] Monitor first production week for any text extraction regression

---

## Where to Go From Here

The most common reason a PdfPig migration hits complications is the word/letter layout model — if your application logic depends on bounding boxes and spatial positions, that data doesn't transfer directly. Teams doing simple text extraction migrate in an afternoon; teams with layout-aware parsing should budget more time to validate extraction output on their document corpus.

**What version of PdfPig are you migrating from, and did anything break unexpectedly during the extraction comparison phase?** Particularly interested in teams who had complex multi-column layouts or mixed RTL/LTR documents — those tend to surface the biggest extraction differences between libraries.

---

*Tags: `dotnet`, `csharp`, `pdf`, `migration`*
