// PdfOxide DOES convert HTML+CSS to PDF — natively, via a pure-Rust
// renderer (no Chromium/Blink/WebKit, no JavaScript execution).
// PdfOxide (NuGet: PdfOxide, namespace PdfOxide.Core) renders templated /
// styled HTML plus a practical CSS subset (fonts, colors, sizes, basic
// layout) — great for invoices, reports, and letters. It is NOT a full
// browser engine, so JS-driven or complex Bootstrap/Flexbox/Grid pages
// will not render with full fidelity. For pixel-accurate modern web pages,
// use a Chromium-based engine such as IronPDF, PuppeteerSharp, or Playwright.

using PdfOxide.Core;

// 1) Simple HTML string -> PDF
using var pdf = Pdf.FromHtml("<h1>Hello World</h1><p>This is a PDF from HTML</p>");
pdf.Save("html.pdf");

// 2) HTML + CSS with a custom font (a TTF font byte[] is required for FromHtmlCss)
byte[] font = System.IO.File.ReadAllBytes("DejaVuSans.ttf");
using var styled = Pdf.FromHtmlCss(
    "<h1>Invoice</h1><p>Total: <b>$42.00</b></p>",
    "h1 { color: #1a73e8 } b { color: #c0392b }",
    font);
styled.Save("invoice.pdf");
