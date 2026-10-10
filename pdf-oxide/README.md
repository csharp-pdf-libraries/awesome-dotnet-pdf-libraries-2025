# pdf_oxide (PdfOxide) - C# PDF Library Comparison 2025

> Comprehensive comparison of pdf_oxide (PdfOxide) vs IronPDF for .NET developers

## Overview

- **License**: Open Source (MIT, also available under Apache-2.0)
- **Website / GitHub**: https://github.com/yfedoseev/pdf_oxide (C# binding under `/csharp`)
- **NuGet**: `dotnet add package PdfOxide` (namespace `PdfOxide.Core`) — v0.3.57
- **Use Case**: High-speed PDF text extraction, image extraction, and Markdown conversion (RAG / LLM pipelines, document analysis). Also supports PDF creation from HTML+CSS, Markdown, and plain text.

**pdf_oxide** is a fast, free, and open-source .NET PDF library. It is a thin C# binding over a pure-Rust core, called through P/Invoke against a bundled native library. Its focus is **reading and converting** PDFs — extracting text and images and producing clean Markdown. It also performs **native HTML+CSS → PDF** via `Pdf.FromHtml(...)` and `Pdf.FromHtmlCss(...)`, using a pure-Rust renderer with **no Chromium/Blink/WebKit and no JavaScript execution**. That makes it great for templated, styled HTML (invoices, reports, letters) over a practical CSS subset (fonts, colors, sizes, basic layout), but it will **not** faithfully render full modern web pages that rely on JavaScript or complex Bootstrap/Flexbox/Grid layouts. It can also *create* PDFs from Markdown and plain text via `Pdf.FromMarkdown(...)`.

The author reports a mean of ~0.8 ms per document for extraction on their benchmark suite (verified May 2025 against the project's own benchmarks; independent third-party benchmarks are not yet published).

## Quick Example

```csharp
// NuGet: Install-Package PdfOxide
using PdfOxide.Core;

using var doc = PdfDocument.Open("paper.pdf");
string text = doc.ExtractText(0);       // page 0 (0-based)
string markdown = doc.ToMarkdownAll();  // whole document as Markdown
```

## Strengths

✅ Free and open source under a permissive MIT license (also Apache-2.0) — usable in proprietary apps
✅ Fast text/image extraction via a pure-Rust core (P/Invoke), reported ~0.8 ms/document on the project's benchmarks
✅ Built-in **Markdown** conversion (`ToMarkdown`, `ToMarkdownAll`) — well suited to RAG / LLM ingestion
✅ Image extraction (`ExtractImages`) and region/rectangle text extraction (`ExtractTextInRect`)
✅ Cross-platform: ships native libraries for Linux, macOS, and Windows
✅ **Native HTML+CSS → PDF** via `Pdf.FromHtml(...)` / `Pdf.FromHtmlCss(...)` (pure-Rust, no JS/Chromium)
✅ Can also create PDFs from Markdown / plain text via `Pdf.FromMarkdown(...)`

## Limitations

⚠️ **Native CSS-subset renderer, not a browser engine** — HTML+CSS rendering is supported, but there is no Chromium/Blink/WebKit and no JavaScript execution, so JS-driven or complex Bootstrap/Flexbox/Grid pages won't render with full fidelity (partial on the [Bootstrap Homepage Test](../README.md#the-bootstrap-homepage-test) — it handles styled HTML/CSS but not JS or full Bootstrap fidelity)
⚠️ **Not a full PDF-lifecycle suite** — focused on extraction/conversion plus templated HTML/Markdown authoring rather than full-browser web-page rendering
⚠️ Native P/Invoke dependency — a platform-specific native library must be present at runtime (Linux/macOS/Windows provided)
⚠️ Pre-1.0 (v0.3.57): the public API may still change between minor versions
⚠️ Performance figures come from the project's own benchmarks; no independent third-party benchmark is published yet

## Comparison with IronPDF

| Feature | pdf_oxide (PdfOxide) | IronPDF |
|---------|----------------------|---------|
| **License** | Open Source (MIT / Apache-2.0) | Commercial |
| **NuGet package** | `PdfOxide` (v0.3.57, pre-1.0) | `IronPdf` |
| **Cost** | Free | Paid |
| **Core engine** | Pure-Rust core via P/Invoke | Full Chromium (Blink) |
| **PDF Reading/Text Extraction** | Excellent (fast, ~0.8 ms/doc on project benchmarks) | Excellent |
| **Image Extraction** | Supported (`ExtractImages`) | Supported |
| **Markdown Conversion** | Supported (`ToMarkdown` / `ToMarkdownAll`) | Not built-in (plain text only) |
| **PDF Creation** | HTML+CSS (`Pdf.FromHtml`/`FromHtmlCss`), Markdown, plain text | Comprehensive (HTML/CSS) |
| **HTML to PDF** | Native HTML+CSS (no JS/Chromium) | Supported (full Chromium) |
| **URL to PDF** | Not supported | Supported |
| **Watermarks / Encryption authoring / Signing** | Partial / editor APIs (not a full PDF-lifecycle suite) | Supported |
| **Cross-Platform** | Windows / Linux / macOS (native libs) | Windows / Linux / macOS / Docker / Cloud |
| **Support & Documentation** | Community (GitHub) | Dedicated support |

## Code Comparison

### Extract Text

#### pdf_oxide
```csharp
// NuGet: Install-Package PdfOxide
using PdfOxide.Core;

using var doc = PdfDocument.Open("input.pdf");
for (int page = 0; page < doc.PageCount; page++)
    System.Console.WriteLine(doc.ExtractText(page)); // 0-based
```

#### IronPDF
```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");
System.Console.WriteLine(pdf.ExtractAllText());
```

### HTML to PDF

#### pdf_oxide
```csharp
// PdfOxide renders HTML+CSS to PDF natively (pure-Rust, no JS/Chromium).
using PdfOxide.Core;

using var pdf = Pdf.FromHtml("<h1>Hello World</h1><p>This is a PDF from HTML</p>");
pdf.Save("html.pdf");

// HTML + CSS with a custom font (a TTF font byte[] is required for FromHtmlCss):
byte[] font = System.IO.File.ReadAllBytes("DejaVuSans.ttf");
using var styled = Pdf.FromHtmlCss(
    "<h1>Invoice</h1><p>Total: <b>$42.00</b></p>",
    "h1 { color: #1a73e8 }",
    font);
styled.Save("invoice.pdf");
```

#### IronPDF
```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

## Migration Guide

See [migrate-from-pdf-oxide.md](migrate-from-pdf-oxide.md) for detailed migration instructions.

## Related Libraries

- [PdfPig](../pdfpig/) — open-source PDF reading/extraction (Apache 2.0)
- [Apache PDFBox](../apache-pdfbox/) — extraction-focused, Java-rooted
- [QuestPDF](../questpdf/) — code-first PDF generation (MIT)

## References

- [GitHub Repository](https://github.com/yfedoseev/pdf_oxide)
- [NuGet: PdfOxide](https://www.nuget.org/packages/PdfOxide)
