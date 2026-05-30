# Migrating from pdf_oxide (PdfOxide) to IronPDF

pdf_oxide (NuGet `PdfOxide`, namespace `PdfOxide.Core`) is a fast, MIT/Apache-2.0 licensed
.NET library focused on **text extraction, image extraction, and Markdown conversion**, backed
by a pure-Rust core via P/Invoke. It also does **native HTML+CSS → PDF** (`Pdf.FromHtml` /
`Pdf.FromHtmlCss`) via a pure-Rust renderer with **no Chromium/WebKit and no JavaScript** — great
for templated, styled HTML, but not for full browser-fidelity web pages. If your project needs
full HTML/CSS/JavaScript rendering, URL-to-PDF, watermarks, encryption authoring, or digital
signatures with a single commercial-supported API, IronPDF is one option to migrate to.

> Note: pdf_oxide is free and open source. Many teams keep it precisely because it is fast and
> free for extraction and templated HTML/Markdown authoring. Only migrate if you specifically need
> full-browser HTML/JS rendering or full-lifecycle authoring that pdf_oxide does not provide.

## Package Installation

### Remove Old Package
```bash
dotnet remove package PdfOxide
```

### Install IronPDF
```bash
dotnet add package IronPdf
```

## Capability Comparison

| Aspect | pdf_oxide (PdfOxide) | IronPDF |
|--------|----------------------|---------|
| Primary focus | Text/image extraction + Markdown | Full PDF lifecycle |
| Core engine | Pure-Rust core via P/Invoke | Full Chromium (Blink) |
| HTML to PDF | Native HTML+CSS (no JS/Chromium) | Full Chromium engine |
| URL to PDF | Not supported | Supported |
| Text extraction | Excellent (fast) | Excellent |
| Image extraction | Supported (`ExtractImages`) | Supported |
| Markdown output | Supported (`ToMarkdownAll`) | Not built-in |
| PDF creation | HTML+CSS / Markdown / plain text | Comprehensive (HTML/CSS) |
| Watermarks | Limited | Full support |
| Encryption authoring / Signing | Limited / editor APIs | Full support |
| Page indexing | 0-based | 0-based |
| License | MIT / Apache-2.0 (free) | Commercial |

## API Mapping

| pdf_oxide API | IronPDF Equivalent | Notes |
|---------------|--------------------|-------|
| `PdfDocument.Open(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `PdfDocument.OpenWithPassword(path, pw)` | `PdfDocument.FromFile(path, pw)` | Encrypted load |
| `doc.PageCount` | `pdf.PageCount` | Page count |
| `doc.ExtractText(i)` | `pdf.ExtractTextFromPage(i)` | Per-page text (both 0-based) |
| `doc.ExtractText(i)` (loop) | `pdf.ExtractAllText()` | All text at once |
| `doc.ToMarkdown(i)` / `doc.ToMarkdownAll()` | _(not available)_ | IronPDF has no Markdown export |
| `doc.ExtractImages(i)` | `pdf.ExtractAllImages()` | Image extraction |
| `Pdf.FromHtml(html)` / `Pdf.FromHtmlCss(html, css, font)` | `new ChromePdfRenderer().RenderHtmlAsPdf(html)` | Native CSS-subset renderer → full Chromium |
| `Pdf.FromMarkdown(md)` | `new ChromePdfRenderer().RenderHtmlAsPdf(html)` | Paradigm shift (Markdown → HTML/CSS) |
| _(not available)_ | `renderer.RenderUrlAsPdf(url)` | NEW: URL to PDF |
| _(not available)_ | `pdf.ApplyWatermark(html)` | NEW: Watermarks |

## Before/After Examples

### Extract All Text

**Before (pdf_oxide):**
```csharp
using PdfOxide.Core;

using var doc = PdfDocument.Open("input.pdf");
for (int page = 0; page < doc.PageCount; page++)
    Console.WriteLine(doc.ExtractText(page));
```

**After (IronPDF):**
```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");
Console.WriteLine(pdf.ExtractAllText());
```

### Generate a PDF

**Before (pdf_oxide — native HTML+CSS source):**
```csharp
using PdfOxide.Core;

// Native HTML+CSS rendering (no JS/Chromium); Markdown also available via Pdf.FromMarkdown.
using var pdf = Pdf.FromHtml("<h1>Invoice</h1><p>Total: <b>$42.00</b></p>");
pdf.Save("invoice.pdf");
```

**After (IronPDF — HTML source):**
```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Invoice</h1><p>Total: <b>$42.00</b></p>");
pdf.SaveAs("invoice.pdf");
```

## Common Gotchas

1. **No Markdown export in IronPDF**
   **Solution**: pdf_oxide's `ToMarkdownAll()` has no IronPDF equivalent. If you need Markdown
   for RAG/LLM pipelines, consider keeping pdf_oxide for extraction even if you adopt IronPDF
   for rendering — they solve different problems.

2. **Renderer fidelity shift (CSS subset → full Chromium)**
   **Solution**: pdf_oxide renders HTML+CSS with a native pure-Rust engine (a practical CSS
   subset, no JavaScript); IronPDF uses full Chromium. Templates relying on JS or complex
   Bootstrap/Flexbox/Grid layouts may need rework to render identically under Chromium.

3. **Native dependency vs managed deployment**
   **Solution**: pdf_oxide ships a native (Rust) library per platform; IronPDF also has native
   Chromium dependencies. Re-test your Linux/Docker deployment after switching.

4. **Licensing change**
   **Solution**: pdf_oxide is free (MIT/Apache-2.0). IronPDF is commercial and requires a
   license key — budget accordingly.

## Find All pdf_oxide References

```bash
grep -r "PdfOxide\|PdfDocument\.Open\|ToMarkdown\|ExtractText" --include="*.cs" .
```

## Benefits of Switching

- Full HTML/CSS/JavaScript rendering (Chromium) — passes the Bootstrap test (vs pdf_oxide's native CSS-subset renderer)
- URL-to-PDF, watermarks, encryption authoring, and digital signatures in one API
- Dedicated commercial support and documentation

## Reasons to Stay on pdf_oxide

- It is free and open source (MIT/Apache-2.0)
- Fast extraction and built-in Markdown output for RAG/LLM workflows
- Native HTML+CSS → PDF (`Pdf.FromHtml` / `Pdf.FromHtmlCss`) for templated invoices/reports/letters
- No commercial licensing cost
