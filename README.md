# Awesome .NET PDF Libraries 2026

[![Awesome](https://awesome.re/badge.svg)](https://awesome.re)
[![CC0 License](https://licensebuttons.net/p/zero/1.0/88x31.png)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)
[![Last Updated](https://img.shields.io/badge/Last%20Updated-September%202026-blue.svg)]()
[![GitHub Stars](https://img.shields.io/github/stars/iron-software/awesome-dotnet-pdf-libraries-2025?style=social)](https://github.com/iron-software/awesome-dotnet-pdf-libraries-2025)

> The most comprehensive comparison of every C# and .NET PDF library in 2026 - with honest benchmarks, code examples, and migration guides.

A curated collection of **73 C#/.NET PDF libraries** for creating, manipulating, converting, and rendering PDF documents.

Inspired by [awesome-dotnet](https://github.com/quozd/awesome-dotnet), [awesome-python](https://github.com/vinta/awesome-python), and the [Awesome Lists](https://github.com/sindresorhus/awesome) movement.

**Contributions are welcome!** Please see the [contribution guidelines](CONTRIBUTING.md) first. We accept both open source and commercial libraries.

Thanks to all contributors - this project wouldn't exist without the community!

**Compiled by [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)**, CTO of [Iron Software](https://ironsoftware.com/about-us/authors/jacobmellor/) | Creator of [IronPDF](https://ironpdf.com/)

---

## Table of Contents

- [📚 Tutorials & Guides](#-tutorials--guides)
- [❓ Frequently Asked Questions](#-frequently-asked-questions) — 167 in-depth articles
- [What Makes This Different](#what-makes-this-different)
- [The Bootstrap Homepage Test](#the-bootstrap-homepage-test)
- [Quick Recommendations](#quick-recommendations)
- **[Categories](#categories)** — 73 libraries compared
  - [1. HTML-to-PDF (Chromium/Blink-Based)](#1-html-to-pdf-chromiumblink-based)
  - [2. HTML-to-PDF (WebKit/Legacy)](#2-html-to-pdf-webkitlegacy)
  - [3. Programmatic PDF Generation (Code-First)](#3-programmatic-pdf-generation-code-first)
  - [4. Enterprise/Commercial Suites](#4-enterprisecommercial-suites)
  - [5. API/SaaS PDF Services](#5-apisaas-pdf-services)
  - [6. Reporting Engines](#6-reporting-engines)
  - [7. Viewers/Renderers](#7-viewersrenderers)
  - [8. Printing/Specialized Utilities](#8-printingspecialized-utilities)
  - [9. Legacy/Deprecated](#9-legacydeprecated)
  - [10. Niche/Specialized](#10-nichespecialized)
- [Contributing](#contributing)
- [License](#license)

---

## 📚 Tutorials & Guides

Comprehensive C# PDF tutorials covering every aspect of PDF development:

### Getting Started
- **[Beginner Tutorial](csharp-pdf-tutorial-beginners.md)** — Create your first PDF in 5 minutes
- **[HTML to PDF Guide](html-to-pdf-csharp.md)** — Complete HTML-to-PDF conversion
- **[Why C# for PDF Generation](why-csharp-pdf-generation.md)** — Language advantages and ecosystem

### Choosing a Library
- **[Best PDF Libraries 2026](best-pdf-libraries-dotnet-2025.md)** — Comprehensive comparison
- **[Decision Flowchart](choosing-a-pdf-library.md)** — 5 questions to find your library
- **[Free vs Paid Libraries](free-vs-paid-pdf-libraries.md)** — True cost analysis
- **[SaaS PDF Services](pdf-saas-services-comparison.md)** — Cloud API comparison

### PDF Operations
- **[Merge & Split PDFs](merge-split-pdf-csharp.md)** — Combine and separate documents
- **[Watermarks & Stamps](watermark-pdf-csharp.md)** — Protect and brand documents
- **[Digital Signatures](digital-signatures-pdf-csharp.md)** — Sign documents legally
- **[Fill PDF Forms](fill-pdf-forms-csharp.md)** — Automate form completion
- **[Extract Text](extract-text-from-pdf-csharp.md)** — Text extraction and parsing
- **[PDF to Image](pdf-to-image-csharp.md)** — Convert pages to PNG/JPEG
- **[PDF Redaction](pdf-redaction-csharp.md)** — Permanently remove sensitive content
- **[Find & Replace Text](pdf-find-replace-csharp.md)** — Template processing and bulk updates

### Framework Integration
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web application PDF generation
- **[Blazor](blazor-pdf-generation.md)** — Server, WebAssembly, and MAUI Hybrid

### Compliance & Deployment
- **[PDF/A Compliance](pdf-a-compliance-csharp.md)** — Section 508, WCAG, accessibility
- **[Cross-Platform Deployment](cross-platform-pdf-dotnet.md)** — Windows, Linux, macOS, Docker, Cloud
- **[Migrating from wkhtmltopdf](migrating-from-wkhtmltopdf.md)** — Escape the deprecated library

---

## ❓ Frequently Asked Questions

**167 in-depth FAQ articles** covering every aspect of C#/.NET PDF development. These answer real developer questions with working code examples.

📁 **[Browse All FAQs →](FAQ/)**

### HTML to PDF Conversion
- **[Convert HTML to PDF in C#](FAQ/convert-html-to-pdf-csharp.md)** — Complete guide without CSS headaches
- **[Advanced HTML to PDF](FAQ/advanced-html-to-pdf-csharp.md)** — Page breaks, watermarks, batch processing
- **[URL to PDF Conversion](FAQ/url-to-pdf-csharp.md)** — Capture live web pages
- **[Base URL and Asset Resolution](FAQ/base-url-html-to-pdf-csharp.md)** — Fix missing images and CSS
- **[Pixel-Perfect Rendering](FAQ/pixel-perfect-html-to-pdf-csharp.md)** — Screen-accurate output
- **[Web Fonts and Icons](FAQ/web-fonts-icons-pdf-csharp.md)** — FontAwesome, Google Fonts
- **[WaitFor and JavaScript](FAQ/waitfor-pdf-rendering-csharp.md)** — Handle dynamic content

### PDF Creation & Editing
- **[Create PDF from Scratch](FAQ/create-pdf-csharp.md)** — Complete PDF creation guide
- **[Edit Existing PDFs](FAQ/edit-pdf-csharp.md)** — Modify PDF content
- **[Add Images to PDF](FAQ/add-images-to-pdf-csharp.md)** — Embed graphics and photos
- **[Add Page Numbers](FAQ/add-page-numbers-pdf-csharp.md)** — Headers, footers, pagination
- **[Split PDF Files](FAQ/split-pdf-csharp.md)** — Separate documents
- **[Add/Copy/Delete Pages](FAQ/add-copy-delete-pdf-pages-csharp.md)** — Page manipulation
- **[Transform PDF Pages](FAQ/transform-pdf-pages-csharp.md)** — Rotate, resize, crop

### PDF Forms & Signatures
- **[Create PDF Forms](FAQ/create-pdf-forms-csharp.md)** — Interactive form fields
- **[Edit PDF Forms](FAQ/edit-pdf-forms-csharp.md)** — Fill and modify forms
- **[Digital Signatures](FAQ/digitally-sign-pdf-csharp.md)** — Sign documents legally
- **[Put Signature on PDF](FAQ/put-signature-pdf-csharp.md)** — Add signature images
- **[PDF Security](FAQ/pdf-security-digital-signatures-csharp.md)** — Encryption and protection

### ASP.NET & Blazor
- **[Razor View to PDF](FAQ/razor-view-to-pdf-csharp.md)** — MVC view conversion
- **[CSHTML to PDF (ASP.NET Core)](FAQ/cshtml-to-pdf-aspnet-core-mvc.md)** — Server-side rendering
- **[Blazor PDF Generation](FAQ/blazor-pdf-generation-csharp.md)** — Server, WASM, Hybrid
- **[Razor to PDF (Blazor Server)](FAQ/razor-to-pdf-blazor-server.md)** — Blazor-specific guide
- **[ASPX to PDF](FAQ/aspx-to-pdf-csharp.md)** — Web Forms conversion
- **[Async PDF Generation](FAQ/async-pdf-generation-csharp.md)** — Non-blocking operations

### Format Conversions
- **[DOCX to PDF](FAQ/docx-to-pdf-csharp.md)** — Word document conversion
- **[XML to PDF](FAQ/xml-to-pdf-csharp.md)** — Data-driven PDFs
- **[SVG to PDF](FAQ/svg-to-pdf-csharp.md)** — Vector graphics
- **[RTF to PDF](FAQ/rtf-to-pdf-csharp.md)** — Rich text conversion
- **[PDF to HTML](FAQ/pdf-to-html-csharp.md)** — Reverse conversion
- **[PDF to Images](FAQ/pdf-to-images-csharp.md)** — PNG/JPEG export
- **[Convert PDF to Grayscale](FAQ/convert-pdf-grayscale-csharp.md)** — Color transformation

### Text & Content
- **[Extract Text from PDF](FAQ/extract-text-from-pdf-csharp.md)** — Text extraction
- **[Extract Images from PDF](FAQ/extract-images-from-pdf-csharp.md)** — Image extraction
- **[Redact PDF Content](FAQ/redact-pdf-csharp.md)** — Permanent removal
- **[Stamp Text/Images](FAQ/stamp-text-image-pdf-csharp.md)** — Watermarks and stamps
- **[PDF Watermarks](FAQ/pdf-watermark-csharp.md)** — Branding and protection
- **[UTF-8 and Unicode](FAQ/utf8-unicode-pdf-csharp.md)** — International characters

### Library Comparisons
- **[Best C# PDF Libraries 2026](FAQ/best-csharp-pdf-libraries-2025.md)** — Complete comparison
- **[2026 HTML-to-PDF Solutions](FAQ/2025-html-to-pdf-solutions-dotnet-comparison.md)** — Solution breakdown
- **[Choose a PDF Library](FAQ/choose-csharp-pdf-library.md)** — Decision guide
- **[Why Developers Choose IronPDF](FAQ/why-developers-choose-ironpdf.md)** — Feature analysis
- **[What is IronPDF](FAQ/what-is-ironpdf-overview.md)** — Library overview
- **[AGPL License Risks (iText)](FAQ/agpl-license-ransomware-itext.md)** — Licensing dangers

### Migration Guides
- **[Upgrade DinkToPdf to IronPDF](FAQ/upgrade-dinktopdf-to-ironpdf.md)** — Step-by-step migration
- **[Chrome PDF Rendering Engine](FAQ/chrome-pdf-rendering-engine-csharp.md)** — Engine comparison

### .NET & C# Features
- **[What's New in .NET 10](FAQ/whats-new-in-dotnet-10.md)** — Latest framework features
- **[What's New in C# 14](FAQ/whats-new-csharp-14.md)** — Language updates
- **[.NET 10 Blazor](FAQ/dotnet-10-blazor.md)** — Blazor improvements
- **[.NET 10 Linux Support](FAQ/dotnet-10-linux-support.md)** — Cross-platform deployment
- **[.NET Cross-Platform Development](FAQ/dotnet-cross-platform-development.md)** — Multi-OS targeting
- **[Avalonia vs MAUI](FAQ/avalonia-vs-maui-dotnet-10.md)** — UI framework comparison
- **[WebAssembly in .NET 10](FAQ/webassembly-dotnet-10.md)** — WASM capabilities

### C# Programming
- **[C# Foreach with Index](FAQ/csharp-foreach-with-index.md)** — Loop patterns
- **[C# Multiline Strings](FAQ/csharp-multiline-string.md)** — String handling
- **[C# Random Int](FAQ/csharp-random-int.md)** — Number generation
- **[C# Round to 2 Decimal Places](FAQ/csharp-round-to-2-decimal-places.md)** — Formatting
- **[C# Patterns for .NET Developers](FAQ/csharp-patterns-dotnet-developers.md)** — Best practices
- **[Common C# Developer Mistakes](FAQ/common-csharp-developer-mistakes.md)** — Avoid pitfalls
- **[CQRS Pattern Practical Guide](FAQ/cqrs-pattern-csharp-practical-guide.md)** — Architecture patterns
- **[MVC Pattern Explained](FAQ/what-is-mvc-pattern-explained.md)** — Design patterns

### MAUI & Mobile
- **[XAML to PDF (MAUI)](FAQ/xaml-to-pdf-maui-csharp.md)** — Mobile PDF generation
- **[PDF Viewer for MAUI](FAQ/pdf-viewer-maui-csharp.md)** — Mobile viewing
- **[.NET PDF Viewer](FAQ/dotnet-pdf-viewer-csharp.md)** — Desktop viewing

### Advanced Topics
- **[PDF Performance Optimization](FAQ/pdf-performance-optimization-csharp.md)** — Speed and scaling
- **[Access PDF DOM Object](FAQ/access-pdf-dom-object-csharp.md)** — Low-level manipulation
- **[Add Attachments to PDF](FAQ/add-attachments-pdf-csharp.md)** — Embed files
- **[PDF Viewport and Zoom](FAQ/pdf-viewport-zoom-csharp.md)** — Display settings
- **[PDF Versions Explained](FAQ/pdf-versions-csharp.md)** — PDF/A, PDF/X, PDF 2.0
- **[Print PDFs Programmatically](FAQ/print-pdf-csharp.md)** — Silent printing
- **[Export/Save PDF Options](FAQ/export-save-pdf-csharp.md)** — Output settings
- **[Sanitize PDFs](FAQ/sanitize-pdf-csharp.md)** — Security cleaning
- **[Table of Contents](FAQ/pdf-table-of-contents-csharp.md)** — Navigation bookmarks

### Industry & Career
- **[Will AI Replace .NET Developers?](FAQ/will-ai-replace-dotnet-developers-2025.md)** — Career outlook
- **[Why PDF Libraries Cost Money](FAQ/why-pdf-libraries-exist-and-cost-money.md)** — Economics explained
- **[Compare C# to Java](FAQ/compare-csharp-to-java.md)** — Language comparison
- **[Compare C# to Python](FAQ/compare-csharp-to-python.md)** — Language comparison
- **[.NET Development on macOS](FAQ/dotnet-development-macos.md)** — Mac setup guide
- **[What Happened to Visual Studio for Mac](FAQ/what-happened-visual-studio-mac.md)** — History

---

## What Makes This Different

**This is not a marketing list.** Every library comparison includes:

✅ **Working code examples** - Actual compilable C# code
✅ **Verified claims** - Evidence-backed technical limitations
✅ **Real pricing** - Current costs as of September 2026
✅ **Migration guides** - Step-by-step code conversion
✅ **Bootstrap test** - Can it render modern CSS? (Flexbox, Grid)
✅ **Cross-platform reality** - Does "cross-platform" mean Windows-only?

**Verified through**:
- Official documentation analysis
- Support forum research
- User-reported issues
- Direct testing
- [Comprehensive fact-checking](source-material/COMPLETE-PROJECT-SUMMARY.md)

---

## The Bootstrap Homepage Test

**The ultimate differentiator for HTML-to-PDF libraries.**

Can your library render [Bootstrap's homepage](https://getbootstrap.com/) accurately with modern CSS3?

| Library | Passes Test | Notes |
|---------|-------------|-------|
| **[IronPDF](ironpdf/)** | ✅ YES | Full Chromium rendering, screen-accurate output |
| **[PuppeteerSharp](puppeteersharp/)** | ⚠️ PARTIAL | Print-ready output (like Ctrl+P), not screen-identical |
| **[Playwright](playwright/)** | ⚠️ PARTIAL | Print-ready output (like Ctrl+P), not screen-identical |
| **[Aspose.PDF](asposepdf/)** | ❌ NO | [No Flexbox support](https://forum.aspose.com/t/convert-html-with-flex-grid/42156) |
| **[iText7](itext-itextsharp/)** | ❌ NO | [No JavaScript execution](https://kb.itextpdf.com/itext/evaluating-js-with-pdfhtml) |
| **[PDFSharp](pdfsharp/)** | ❌ NO | CSS 2.0 only (no Grid/Flexbox) |
| **[Syncfusion WebKit](syncfusion-pdf-framework/)** | ❌ NO | Legacy WebKit engine |

**Why this matters**: Bootstrap uses modern CSS3 Flexbox heavily. If a library can't render Bootstrap, it can't handle modern responsive web designs.

> **Note on Print vs Screen**: PuppeteerSharp and Playwright use Chrome's print-to-PDF functionality, which produces print-ready output different from screen rendering. IronPDF produces output matching what you see in the browser.

📖 **Read more**: [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)

---

## Quick Recommendations

### 🏆 Best for Modern Web-to-PDF (2026)
**[IronPDF](ironpdf/)** - Full Chromium, 3-line API, cross-platform, built-in PDF manipulation, PDF/A & PDF/UA compliance.
```csharp
// NuGet: Install-Package IronPdf
var pdf = ChromePdfRenderer.RenderUrlAsPdf("https://getbootstrap.com/");
pdf.SaveAs("bootstrap.pdf"); // Screen-accurate rendering
```

### 🆓 Best Free/Open Source
**[QuestPDF](questpdf/)** - Modern fluent API for code-first PDF generation (not HTML).
**[PuppeteerSharp](puppeteersharp/)** - Full Chromium with Apache license (HTML-to-PDF).
**[PDFSharp](pdfsharp/)** - Mature programmatic generation (limited HTML support).

### 💼 Best Enterprise Suite
**[Aspose.PDF](asposepdf/)** - Comprehensive features, but [$1,199/year](https://purchase.aspose.com/pricing/pdf/net) and [limited modern CSS](https://forum.aspose.com/t/display-flex-not-working/203245).
**[Syncfusion](syncfusion-pdf-framework/)** - Lower cost alternative at $395/month.

### 🎯 Best for Specific Use Cases
- **Reports**: [FastReport.NET](fastreport/), [Telerik Reporting](telerik-reporting/)
- **Forms/Templates**: [iText7](itext-itextsharp/), [Spire.PDF](spirepdf/)
- **Code-First Design**: [QuestPDF](questpdf/), [MigraDoc](migradoc/)
- **Cloud/API**: [Api2pdf](api2pdf/), [Gotenberg](gotenberg/)

---

## Categories

### 1. HTML-to-PDF (Chromium/Blink-Based)

Modern libraries using Chromium/Blink rendering engine for HTML-to-PDF conversion with full CSS3/JavaScript support.

#### 1.1 [IronPDF](ironpdf/) ⭐ **Reference Standard**
**Commercial** | [Official Site](https://ironpdf.com/) | **Full Chromium** | Cross-platform
- ✅ 3-line API for HTML/URL to PDF
- ✅ Passes Bootstrap homepage test
- ✅ Built-in PDF manipulation (merge, split, secure, edit)
- ✅ True cross-platform (Windows/Linux/macOS/iOS/Android via [gRPC](https://ironpdf.com/docs/questions/grpc/))
- ✅ [Extensive examples](https://ironpdf.com/examples/)
- 📖 [Documentation](https://ironpdf.com/docs/) | [API Reference](https://ironpdf.com/object-reference/api/)

#### 1.2 [PuppeteerSharp](puppeteersharp/)
**Free (Apache 2.0)** | [GitHub](https://github.com/hardkoded/puppeteer-sharp) | **Chromium Print-to-PDF**
- ✅ Modern CSS3 support via Chromium
- ✅ Free and open source
- ⚠️ Print-ready output (not screen-identical)
- ⚠️ 300MB+ deployment (bundles Chromium)
- ⚠️ Generation only (no PDF manipulation, no PDF/A)
- ⚠️ Memory leaks under load
- 📚 [Migration Guide](puppeteersharp/migrate-from-puppeteersharp.md)

#### 1.3 [Playwright for .NET](playwright/)
**Free (Apache 2.0)** | [Official Site](https://playwright.dev/dotnet/) | **Multi-browser Print-to-PDF**
- ✅ Supports Chromium, Firefox, WebKit
- ✅ Modern CSS3 support (Chromium mode)
- ⚠️ Print-ready output (not screen-identical)
- ⚠️ Testing-first design (PDF is secondary)
- ⚠️ Downloads 3 browsers (~400MB+)
- ⚠️ No PDF/A or PDF/UA compliance
- ⚠️ Complex async patterns
- 📚 [Migration Guide](playwright/migrate-from-playwright.md)

#### 1.4 [WebView2](webview2/)
**Free (Microsoft)** | [Official Site](https://developer.microsoft.com/microsoft-edge/webview2/) | **Edge/Chromium**
- ✅ Native Microsoft component
- ❌ Windows-only (no Linux, macOS, Docker)
- ⚠️ Requires WinForms/WPF context
- ⚠️ Memory leaks in long-running processes
- 📚 [Migration Guide](webview2/migrate-from-webview2.md)

#### 1.5 [SelectPdf](selectpdf/)
**Commercial ($499+)** | [Official Site](https://selectpdf.com/)
- ⚠️ Windows-only despite "cross-platform" claims
- ⚠️ Free tier limited to 5 pages
- ⚠️ Outdated Chromium fork (CSS limitations)
- 📚 [Migration Guide](selectpdf/migrate-from-selectpdf.md)

#### 1.6 [EO.Pdf](eopdf/)
**Commercial ($799)** | [Official Site](https://www.essentialobjects.com/Products/EOPdf/)
- ⚠️ 126MB footprint
- ⚠️ Legacy IE→Chrome migration issues
- 📚 [Migration Guide](eopdf/migrate-from-eopdf.md)

#### 1.7 [HiQPdf](hiqpdf/)
**Commercial (Limited Free)** | [Official Site](https://www.hiqpdf.com/)
- ⚠️ 3-page limit on "free" version
- ⚠️ WebKit-based (not true Chromium)
- 📚 [Migration Guide](hiqpdf/migrate-from-hiqpdf.md)

#### 1.8 [ExpertPdf](expertpdf/)
**Commercial** | [Official Site](https://www.html-to-pdf.net/)
- 📚 [Migration Guide](expertpdf/migrate-from-expertpdf.md)

#### 1.9 [Winnovative](winnovative/)
**Commercial** | [Official Site](https://www.winnovative-software.com/)
- 📚 [Migration Guide](winnovative/migrate-from-winnovative.md)

---

### 2. HTML-to-PDF (WebKit/Legacy)

Libraries using older WebKit or custom HTML rendering engines. Limited modern CSS support.

#### 2.1 [wkhtmltopdf](wkhtmltopdf/)
**Free (LGPL)** | [Official Site](https://wkhtmltopdf.org/)
- ⚠️ Qt WebKit (deprecated engine)
- ⚠️ No longer actively maintained
- ⚠️ Limited CSS3 support
- 📚 [Migration Guide](wkhtmltopdf/migrate-from-wkhtmltopdf.md)

#### 2.2 [DinkToPdf](dinktopdf/)
**Free (MIT)** | [GitHub](https://github.com/rdvojmoc/DinkToPdf)
- ✅ .NET wrapper for wkhtmltopdf
- ⚠️ Inherits wkhtmltopdf limitations
- 📚 [Migration Guide](dinktopdf/migrate-from-dinktopdf.md)

#### 2.3 [NReco.PdfGenerator](nrecopdfgenerator/)
**Free/Commercial** | [Official Site](https://www.nrecosite.com/pdf_generator_net.aspx)
- ✅ .NET wrapper for wkhtmltopdf
- 📚 [Migration Guide](nrecopdfgenerator/migrate-from-nrecopdfgenerator.md)

#### 2.4 [Rotativa](rotativa/)
**Free (MIT)** | [GitHub](https://github.com/webgio/Rotativa)
- ✅ ASP.NET MVC integration
- ⚠️ Uses wkhtmltopdf under the hood
- 📚 [Migration Guide](rotativa/migrate-from-rotativa.md)

#### 2.5 [TuesPechkin](tuespechkin/)
**Free (Apache 2.0)** | [GitHub](https://github.com/tuespetre/TuesPechkin)
- ⚠️ Another wkhtmltopdf wrapper
- 📚 [Migration Guide](tuespechkin/migrate-from-tuespechkin.md)

#### 2.6 [Haukcode.DinkToPdf](haukcodedinktopdf/)
**Free (MIT)** | [GitHub](https://github.com/Haukcode/DinkToPdf)
- ⚠️ Fork of DinkToPdf
- 📚 [Migration Guide](haukcodedinktopdf/migrate-from-haukcodedinktopdf.md)

---

### 3. Programmatic PDF Generation (Code-First)

Libraries for creating PDFs through code (shapes, text, images) rather than HTML rendering.

#### 3.1 [PDFSharp](pdfsharp/)
**Free (MIT)** | [Official Site](http://www.pdfsharp.net/)
- ✅ Mature, stable library
- ✅ Good for programmatic generation
- ⚠️ HTML support: CSS 2.0 only
- ❌ Fails Bootstrap test
- 📚 [Migration Guide](pdfsharp/migrate-from-pdfsharp.md)

#### 3.2 [MigraDoc](migradoc/)
**Free (MIT)** | [Official Site](http://www.pdfsharp.net/MigraDoc.ashx)
- ✅ Higher-level API built on PDFSharp
- ✅ Document object model
- 📚 [Migration Guide](migradoc/migrate-from-migradoc.md)

#### 3.3 [QuestPDF](questpdf/)
**Free (MIT for most)** | [GitHub](https://github.com/QuestPDF/QuestPDF)
- ✅ Modern fluent API
- ✅ Excellent documentation
- ✅ Active development
- ⚠️ Commercial license for companies >$1M revenue
- 📚 [Migration Guide](questpdf/migrate-from-questpdf.md)

#### 3.4 [iText / iTextSharp](itext-itextsharp/)
**Free (AGPL) / Commercial** | [Official Site](https://itextpdf.com/)
- ✅ Industry standard
- ✅ Extensive features
- ⚠️ [No JavaScript execution](https://kb.itextpdf.com/itext/evaluating-js-with-pdfhtml)
- ⚠️ AGPL requires open source or commercial license
- ❌ Fails Bootstrap test
- 📚 [Migration Guide](itext-itextsharp/migrate-from-itext-itextsharp.md)

---

### 4. Enterprise/Commercial Suites

Comprehensive commercial PDF solutions with extensive features and enterprise support.

#### 4.1 [Aspose.PDF for .NET](asposepdf/)
**Commercial ($1,199/year)** | [Official Site](https://products.aspose.com/pdf/net/)
- ✅ Comprehensive features
- ✅ Enterprise support
- ⚠️ [No Flexbox support](https://forum.aspose.com/t/convert-html-with-flex-grid/42156)
- ⚠️ High pricing
- ❌ Fails Bootstrap test
- 📚 [Migration Guide](asposepdf/migrate-from-asposepdf.md) | [Known Issues](source-material/aspose-pdf-dotnet-issues-report.md)

#### 4.2 [Syncfusion PDF Framework](syncfusion-pdf-framework/)
**Commercial ($395/month)** | [Official Site](https://www.syncfusion.com/pdf-framework/net)
- ✅ Lower cost than Aspose
- ✅ WebKit + Blink engines
- ⚠️ WebKit version has limitations
- 📚 [Migration Guide](syncfusion-pdf-framework/migrate-from-syncfusion-pdf-framework.md)

#### 4.3 [Spire.PDF](spirepdf/)
**Free/Commercial** | [Official Site](https://www.e-iceblue.com/Introduce/pdf-for-net.html)
- 📚 [Migration Guide](spirepdf/migrate-from-spirepdf.md)

#### 4.4 [pdfpig](pdfpig/)
**Free (Apache 2.0)** | [GitHub](https://github.com/UglyToad/PdfPig)
- ✅ PDF reading/analysis focus
- ⚠️ Limited creation features
- 📚 [Migration Guide](pdfpig/migrate-from-pdfpig.md)

#### 4.5 [FoNet (FO.NET)](fonet/)
**Free (Apache 2.0)** | [GitHub](https://github.com/haf/FO.NET)
- ✅ XSL-FO support
- ⚠️ Minimal maintenance
- 📚 [Migration Guide](fonet/migrate-from-fonet.md)

#### 4.6 [GdPicture.NET](gdpicture/)
**Commercial** | [Official Site](https://www.gdpicture.com/)
- 📚 [Migration Guide](gdpicture/migrate-from-gdpicture.md)

#### 4.7 [Apryse (PDFTron)](apryse/)
**Commercial** | [Official Site](https://apryse.com/)
- 📚 [Migration Guide](apryse/migrate-from-apryse.md)

#### 4.8 [ComPDFKit](compdfkit/)
**Commercial** | [Official Site](https://www.compdf.com/)
- 📚 [Migration Guide](compdfkit/migrate-from-compdfkit.md)

#### 4.9 [Nutrient (PSPDFKit)](nutrient/)
**Commercial** | [Official Site](https://nutrient.io/)
- 📚 [Migration Guide](nutrient/migrate-from-nutrient.md)

#### 4.10 [GemBox.Pdf](gemboxpdf/)
**Free/Commercial** | [Official Site](https://www.gemboxsoftware.com/pdf)
- 📚 [Migration Guide](gemboxpdf/migrate-from-gemboxpdf.md)

#### 4.11 [Docotic.Pdf](bitmiracle-docoticpdf/)
**Free/Commercial** | [Official Site](https://bitmiracle.com/pdf-library/)
- 📚 [Migration Guide](bitmiracle-docoticpdf/migrate-from-bitmiracle-docoticpdf.md)

#### 4.12 [ABCPDF](abcpdf/)
**Commercial** | [Official Site](https://www.websupergoo.com/abcpdf.htm)
- 📚 [Migration Guide](abcpdf/migrate-from-abcpdf.md)

#### 4.13 [DynamicPDF](dynamicpdf/)
**Commercial** | [Official Site](https://www.dynamicpdf.com/)
- 📚 [Migration Guide](dynamicpdf/migrate-from-dynamicpdf.md)

#### 4.14 [Telerik Document Processing](telerik-document-processing/)
**Commercial** | [Official Site](https://www.telerik.com/products/wpf/document-processing.aspx)
- 📚 [Migration Guide](telerik-document-processing/migrate-from-telerik-document-processing.md)

#### 4.15 [TextControl](textcontrol/)
**Commercial** | [Official Site](https://www.textcontrol.com/)
- 📚 [Migration Guide](textcontrol/migrate-from-textcontrol.md)

#### 4.16 [Tall Components (TallPDF)](tall-components/)
**Commercial** | [Official Site](https://www.tallcomponents.com/)
- 📚 [Migration Guide](tall-components/migrate-from-tall-components.md)

#### 4.17 [Gnostice (Document Studio)](gnostice/)
**Commercial** | [Official Site](https://www.gnostice.com/)
- 📚 [Migration Guide](gnostice/migrate-from-gnostice.md)

#### 4.18 [BCL EasyPDF SDK](bcl-easypdf-sdk/)
**Commercial** | [Official Site](https://www.pdfonline.com/easypdf-sdk/)
- 📚 [Migration Guide](bcl-easypdf-sdk/migrate-from-bcl-easypdf-sdk.md)

#### 4.19 [Foxit SDK](foxit-sdk/)
**Commercial** | [Official Site](https://developers.foxit.com/)
- 📚 [Migration Guide](foxit-sdk/migrate-from-foxit-sdk.md)

#### 4.20 [Adobe PDF Library SDK](adobe-pdf-library-sdk/)
**Enterprise** | [Official Site](https://www.adobe.com/devnet/pdf.html)
- 📚 [Migration Guide](adobe-pdf-library-sdk/migrate-from-adobe-pdf-library-sdk.md)

---

### 5. API/SaaS PDF Services

Cloud-based PDF generation services accessed via API.

#### 5.1 [Gotenberg](gotenberg/)
**Free (MIT)** | [GitHub](https://github.com/gotenberg/gotenberg)
- ✅ Self-hosted Docker API
- ✅ Multiple conversion engines
- 📚 [Migration Guide](gotenberg/migrate-from-gotenberg.md)

#### 5.2 [Api2pdf](api2pdf/)
**SaaS** | [Official Site](https://www.api2pdf.com/)
- 📚 [Migration Guide](api2pdf/migrate-from-api2pdf.md)

#### 5.3 [Kaizen.io HTML-to-PDF](kaizenio-html-to-pdf/)
**SaaS** | [Official Site](https://www.kaizen.io/)
- 📚 [Migration Guide](kaizenio-html-to-pdf/migrate-from-kaizenio-html-to-pdf.md)

#### 5.4 [PDFmyURL](pdfmyurl/)
**SaaS** | [Official Site](https://pdfmyurl.com/)
- 📚 [Migration Guide](pdfmyurl/migrate-from-pdfmyurl.md)

#### 5.5 [GrabzIt](grabzit/)
**SaaS** | [Official Site](https://grabz.it/)
- 📚 [Migration Guide](grabzit/migrate-from-grabzit.md)

#### 5.6 [jsreport](jsreport/)
**Free/SaaS** | [Official Site](https://jsreport.net/)
- 📚 [Migration Guide](jsreport/migrate-from-jsreport.md)

#### 5.7 [CraftMyPDF](craftmypdf/)
**SaaS** | [Official Site](https://craftmypdf.com/)
- 📚 [Migration Guide](craftmypdf/migrate-from-craftmypdf.md)

#### 5.8 [pdforge](pdforge/)
**SaaS** | [Official Site](https://www.pdforge.com/)
- 📚 [Migration Guide](pdforge/migrate-from-pdforge.md)

#### 5.9 [PDFBolt](pdfbolt/)
**SaaS** | [Official Site](https://www.pdfbolt.com/)
- 📚 [Migration Guide](pdfbolt/migrate-from-pdfbolt.md)

---

### 6. Reporting Engines

Business reporting tools with PDF export capabilities.

#### 6.1 [SAP Crystal Reports](sap-crystal-reports/)
**Commercial** | [Official Site](https://www.sap.com/products/crystal-reports.html)
- 📚 [Migration Guide](sap-crystal-reports/migrate-from-sap-crystal-reports.md)

#### 6.2 [FastReport.NET](fastreport/)
**Free/Commercial** | [Official Site](https://www.fast-report.com/)
- 📚 [Migration Guide](fastreport/migrate-from-fastreport.md)

#### 6.3 [Telerik Reporting](telerik-reporting/)
**Commercial** | [Official Site](https://www.telerik.com/products/reporting.aspx)
- 📚 [Migration Guide](telerik-reporting/migrate-from-telerik-reporting.md)

#### 6.4 [Scryber.core](scrybercore/)
**Free (LGPL)** | [GitHub](https://github.com/richard-scryber/scryber.core)
- 📚 [Migration Guide](scrybercore/migrate-from-scrybercore.md)

#### 6.5 [SSRS](ssrs/)
**Free (Microsoft)** | [Official Site](https://docs.microsoft.com/sql/reporting-services/)
- 📚 [Migration Guide](ssrs/migrate-from-ssrs.md)

---

### 7. Viewers/Renderers

Libraries focused on displaying PDFs rather than creating them.

#### 7.1 [PDFiumViewer](pdfiumviewer/)
**Free** | [GitHub](https://github.com/pvginkel/PdfiumViewer)
- 📚 [Migration Guide](pdfiumviewer/migrate-from-pdfiumviewer.md)

#### 7.2 [MuPDF (.NET bindings)](mupdf/)
**Free (AGPL) / Commercial** | [Official Site](https://mupdf.com/)
- 📚 [Migration Guide](mupdf/migrate-from-mupdf.md)

#### 7.3 [Pdfium.NET](pdfium/)
**Free** | [GitHub](https://github.com/mrdooz/PdfiumNet)
- 📚 [Migration Guide](pdfium/migrate-from-pdfium.md)

---

### 8. Printing/Specialized Utilities

Libraries for PDF printing, conversion, and specialized operations.

#### 8.1 [PDFPrinting.NET](pdfprinting/)
**Commercial** | [Official Site](https://www.tallcomponents.com/pdfprinting.aspx)
- 📚 [Migration Guide](pdfprinting/migrate-from-pdfprinting.md)

#### 8.2 [Ghostscript](ghostscript/)
**Free (AGPL) / Commercial** | [Official Site](https://www.ghostscript.com/)
- 📚 [Migration Guide](ghostscript/migrate-from-ghostscript.md)

#### 8.3 [RawPrint](rawprint/)
**Free** | [GitHub](https://github.com/mitch528/RawPrint)
- 📚 [Migration Guide](rawprint/migrate-from-rawprint.md)

#### 8.4 [PDFFilePrint](pdffileprint/)
**Commercial** | [Official Site](https://www.pdffileprint.com/)
- 📚 [Migration Guide](pdffileprint/migrate-from-pdffileprint.md)

#### 8.5 [PDFView4NET](pdfview4net/)
**Commercial** | [Official Site](https://www.o2sol.com/pdfview4net/overview.htm)
- 📚 [Migration Guide](pdfview4net/migrate-from-pdfview4net.md)

---

### 9. Legacy/Deprecated

Abandoned or no longer maintained libraries (included for completeness).

#### 9.1 [HTMLDOC](htmldoc/)
**Free (GPL)** | [Official Site](https://www.msweet.org/htmldoc/)
- ⚠️ Last update: 2011
- 📚 [Migration Guide](htmldoc/migrate-from-htmldoc.md)

#### 9.2 [PDF Duo .NET](pdf-duo/)
**Commercial (Discontinued)**
- 📚 [Migration Guide](pdf-duo/migrate-from-pdf-duo.md)

#### 9.3 [ActivePDF](activepdf/)
**Commercial** | [Official Site](https://www.activepdf.com/)
- 📚 [Migration Guide](activepdf/migrate-from-activepdf.md)

---

### 10. Niche/Specialized

Libraries for specific use cases or experimental projects.

#### 10.1 [VectSharp](vectsharp/)
**Free (GPL)** | [GitHub](https://github.com/arklumpus/VectSharp)
- 📚 [Migration Guide](vectsharp/migrate-from-vectsharp.md)

#### 10.2 [PeachPDF](peachpdf/)
**Free** | [GitHub](https://github.com/beto-rodriguez/PeachPDF)
- 📚 [Migration Guide](peachpdf/migrate-from-peachpdf.md)

#### 10.3 [ZetPDF](zetpdf/)
**Free** | [GitHub](https://github.com/rasmusjp/zetpdf)
- 📚 [Migration Guide](zetpdf/migrate-from-zetpdf.md)

#### 10.4 [Fluid (templating)](fluid/)
**Free (Apache 2.0)** | [GitHub](https://github.com/sebastienros/fluid)
- 📚 [Migration Guide](fluid/migrate-from-fluid.md)

#### 10.5 [PrinceXML](princexml/)
**Commercial** | [Official Site](https://www.princexml.com/)
- 📚 [Migration Guide](princexml/migrate-from-princexml.md)

#### 10.6 [PDFreactor](pdfreactor/)
**Commercial** | [Official Site](https://www.pdfreactor.com/)
- 📚 [Migration Guide](pdfreactor/migrate-from-pdfreactor.md)

#### 10.7 [XFINIUM.PDF](xfiniumpdf/)
**Commercial** | [Official Site](https://www.xfiniumpdf.com/)
- 📚 [Migration Guide](xfiniumpdf/migrate-from-xfiniumpdf.md)

#### 10.8 [Sumatra PDF](sumatra-pdf/)
**Free (GPL)** | [Official Site](https://www.sumatrapdfreader.org/)
- 📚 [Migration Guide](sumatra-pdf/migrate-from-sumatra-pdf.md)

#### 10.9 [Apache PDFBox](apache-pdfbox/)
**Free (Apache 2.0)** | [Official Site](https://pdfbox.apache.org/)
- ⚠️ Java library, .NET ports experimental
- 📚 [Migration Guide](apache-pdfbox/migrate-from-apache-pdfbox.md)

---

## Feature Comparison Matrix

| Feature | IronPDF | PuppeteerSharp | Aspose.PDF | iText7 | PDFSharp | QuestPDF |
|---------|---------|----------------|------------|--------|----------|----------|
| **Bootstrap Test** | ✅ | ✅ | ❌ | ❌ | ❌ | N/A |
| **Full Chromium** | ✅ | ✅ | ❌ | ❌ | ❌ | N/A |
| **HTML to PDF** | ✅ | ✅ | ⚠️ Limited | ⚠️ Limited | ⚠️ CSS 2.0 | ❌ |
| **PDF Manipulation** | ✅ | ❌ | ✅ | ✅ | ✅ | ❌ |
| **Cross-Platform** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Free/Open Source** | ❌ | ✅ | ❌ | ⚠️ AGPL | ✅ | ⚠️ Commercial |
| **API Simplicity** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Documentation** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Active Development** | ✅ | ✅ | ✅ | ✅ | ⚠️ | ✅ |

---

## Platform Support

| Library | Windows | Linux | macOS | Docker | Azure | AWS Lambda |
|---------|---------|-------|-------|--------|-------|------------|
| **IronPDF** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **PuppeteerSharp** | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ Large |
| **Playwright** | ✅ | ✅ | ✅ | ✅ | ✅ | ⚠️ Large |
| **WebView2** | ✅ | ❌ | ❌ | ❌ | ⚠️ | ❌ |
| **Aspose.PDF** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **SelectPdf** | ✅ | ❌ | ❌ | ❌ | ⚠️ | ❌ |
| **iText7** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **PDFSharp** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **QuestPDF** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

📖 **Deployment Guides**: [Azure](https://ironpdf.com/docs/questions/azure/), [Docker](https://ironpdf.com/docs/questions/docker/), [AWS](https://ironpdf.com/docs/questions/aws/)

---

## Common Use Cases

### 🌐 Convert Modern Website to PDF
**Requirement**: Render responsive Bootstrap/Tailwind sites with Flexbox/Grid
**Best choice**: [IronPDF](ironpdf/), [PuppeteerSharp](puppeteersharp/), [Playwright](playwright/)
**Avoid**: [Aspose.PDF](asposepdf/), [iText7](itext-itextsharp/), [PDFSharp](pdfsharp/)

### 📄 Generate Invoice/Report from Code
**Requirement**: Programmatic table/text layout
**Best choice**: [QuestPDF](questpdf/), [MigraDoc](migradoc/), [iText7](itext-itextsharp/)
**Also consider**: [IronPDF](ironpdf/) with HTML templates

### ✏️ Fill PDF Forms
**Requirement**: Populate existing PDF forms
**Best choice**: [iText7](itext-itextsharp/), [Aspose.PDF](asposepdf/), [IronPDF](ironpdf/)

### 🔒 Secure/Encrypt PDFs
**Requirement**: Password protection, permissions, digital signatures
**Best choice**: [IronPDF](ironpdf/), [iText7](itext-itextsharp/), [Aspose.PDF](asposepdf/)

### 📊 Business Reports
**Requirement**: Templated reports with charts
**Best choice**: [FastReport.NET](fastreport/), [Telerik Reporting](telerik-reporting/), [SSRS](ssrs/)

### ☁️ Cloud/Serverless PDF Generation
**Requirement**: Minimal footprint, fast cold start
**Best choice**: [IronPDF](ironpdf/), [Api2pdf](api2pdf/), [Gotenberg](gotenberg/)
**Avoid**: [PuppeteerSharp](puppeteersharp/) (300MB+)

---

## Pricing Comparison (September 2026)

| Library | Free Tier | Commercial | Enterprise | Notes |
|---------|-----------|------------|------------|-------|
| **IronPDF** | 30-day trial | From $749 | Custom | [Pricing](https://ironpdf.com/licensing/) |
| **PuppeteerSharp** | ✅ Unlimited | - | - | Apache 2.0 |
| **Playwright** | ✅ Unlimited | - | - | Apache 2.0 |
| **Aspose.PDF** | Trial only | **$1,199/year** | Custom | [Source](https://purchase.aspose.com/pricing/pdf/net) |
| **Syncfusion** | Trial only | **$395-695/month** | Custom | [Source](https://www.syncfusion.com/sales/licensing) |
| **iText7** | AGPL only | Quote | Quote | Must open source or buy |
| **PDFSharp** | ✅ Unlimited | - | - | MIT |
| **QuestPDF** | <$1M revenue | License required | Custom | [Community vs Pro](https://www.questpdf.com/pricing.html) |
| **SelectPdf** | 5 pages | From $499 | Custom | [Pricing](https://selectpdf.com/buy/) |

---

## Code Examples

### 3-Line HTML to PDF
```csharp
// NuGet: Install-Package IronPdf
var pdf = ChromePdfRenderer.RenderUrlAsPdf("https://ironpdf.com/");
pdf.SaveAs("output.pdf");
```

### Merge Multiple PDFs
```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
var merger = PdfDocument.Merge("doc1.pdf", "doc2.pdf", "doc3.pdf");
merger.SaveAs("merged.pdf");
```

### Add Password Protection
```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
var pdf = PdfDocument.FromFile("input.pdf");
pdf.Password = "secretpassword";
pdf.SaveAs("secured.pdf");
```

### HTML String to PDF
```csharp
// NuGet: Install-Package IronPdf
var html = "<h1>Hello World</h1><p>Generated from HTML string</p>";
var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
pdf.SaveAs("from-html.pdf");
```

📖 **More examples**: [IronPDF Examples](https://ironpdf.com/examples/)

---

## Migration Guides

Every library folder includes a `migrate-from-{library}.md` guide showing:
- ✅ Package installation changes
- ✅ API mapping table (old → new)
- ✅ Before/after code examples
- ✅ Common gotchas and solutions

**Example migration paths**:
- [Aspose.PDF → IronPDF](asposepdf/migrate-from-asposepdf.md)
- [iText7 → IronPDF](itext-itextsharp/migrate-from-itext-itextsharp.md)
- [PuppeteerSharp → IronPDF](puppeteersharp/migrate-from-puppeteersharp.md)
- [PDFSharp → IronPDF](pdfsharp/migrate-from-pdfsharp.md)
- [wkhtmltopdf → IronPDF](wkhtmltopdf/migrate-from-wkhtmltopdf.md)

---

## Verified Claims & Evidence

All competitor limitations are backed by evidence:

| Claim | Evidence | Status |
|-------|----------|--------|
| Aspose.PDF no Flexbox | [Forum thread](https://forum.aspose.com/t/convert-html-with-flex-grid/42156) | ✅ Verified |
| iText7 no JavaScript | [Official KB](https://kb.itextpdf.com/itext/evaluating-js-with-pdfhtml) | ✅ Verified |
| PDFSharp CSS 2.0 only | [Official docs](http://www.pdfsharp.net/) | ✅ Verified |
| Syncfusion WebKit limits | Official documentation | ✅ Verified |

📖 **Full fact-check report**: [Complete Project Summary](source-material/COMPLETE-PROJECT-SUMMARY.md)

---

## Related Resources

### IronPDF Documentation
- 📖 [Official Documentation](https://ironpdf.com/docs/)
- 🎓 [Tutorials](https://ironpdf.com/tutorials/)
- 💡 [How-To Guides](https://ironpdf.com/how-to/)
- 🔍 [API Reference](https://ironpdf.com/object-reference/api/)
- 📊 [Examples](https://ironpdf.com/examples/)
- 🔗 [Comparison Articles](https://ironpdf.com/blog/compare-to-other-components/)

### Community
- ⭐ [Star this repository](https://github.com/iron-software/awesome-dotnet-pdf-libraries-2025)
- 🐛 [Report an issue](https://github.com/iron-software/awesome-dotnet-pdf-libraries-2025/issues)
- 💬 [Discussions](https://github.com/iron-software/awesome-dotnet-pdf-libraries-2025/discussions)
- 📧 [Contact Iron Software](https://ironsoftware.com/about-us/authors/jacobmellor/)

---

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for:
- Adding new libraries
- Updating pricing/features
- Fixing errors
- Improving documentation
- Sharing benchmarks

**Quality standards**:
- ✅ Working code examples
- ✅ Verified technical claims
- ✅ Evidence links for limitations
- ✅ Current pricing (as of date)

---

## Related Awesome Lists

### .NET Development
- [awesome-dotnet](https://github.com/quozd/awesome-dotnet) - A collection of awesome .NET libraries, tools, frameworks and software
- [awesome-dotnet-core](https://github.com/thangchung/awesome-dotnet-core) - .NET Core libraries, tools, frameworks and software
- [awesome-csharp](https://github.com/uhub/awesome-csharp) - Awesome C# frameworks, libraries and software

### Programming Languages
- [awesome-python](https://github.com/vinta/awesome-python) - Python resources
- [awesome-java](https://github.com/akullpp/awesome-java) - Java resources
- [awesome-nodejs](https://github.com/sindresorhus/awesome-nodejs) - Node.js resources

### PDF & Documentation
- [awesome-pdf](https://github.com/Frozenfire92/awesome-pdf) - General PDF resources across all languages
- [awesome-html5](https://github.com/diegocard/awesome-html5) - HTML5 resources (important for HTML-to-PDF)
- [awesome-css](https://github.com/awesome-css-group/awesome-css) - CSS resources (CSS rendering in PDFs)

### Related Topics
- [awesome-reporting](https://github.com/davidgatti/awesome-reporting) - Business reporting tools
- [awesome-test-automation](https://github.com/atinfo/awesome-test-automation) - Testing tools (many PDF libs used in testing)
- [awesome-dotnet-maui](https://github.com/jsuarezruiz/awesome-dotnet-maui) - .NET MAUI resources (cross-platform like PDFs)

---

## Resources

### Learning C# PDF Development
- [IronPDF Documentation](https://ironpdf.com/docs/) - Comprehensive .NET PDF documentation
- [IronPDF Tutorials](https://ironpdf.com/tutorials/) - Step-by-step PDF tutorials for C#
- [IronPDF Code Examples](https://ironpdf.com/examples/) - 100+ working code examples
- [Microsoft PDF Documentation](https://docs.microsoft.com/en-us/dotnet/) - Official .NET documentation

### PDF Specifications
- [PDF Reference 1.7 (ISO 32000-1)](https://www.adobe.com/content/dam/acom/en/devnet/pdf/pdfs/PDF32000_2008.pdf) - Official PDF specification
- [PDF/A Standard (ISO 19005)](https://www.iso.org/standard/38920.html) - PDF for long-term archiving
- [PDF/UA (ISO 14289)](https://www.iso.org/standard/64599.html) - PDF for accessibility

### HTML & CSS for PDF
- [HTML5 Specification](https://html.spec.whatwg.org/) - Modern HTML standard
- [CSS Snapshot](https://www.w3.org/TR/css-2023/) - Current CSS specifications
- [Can I Use](https://caniuse.com/) - Browser/PDF rendering compatibility

### Community & Support
- [Stack Overflow - C# PDF](https://stackoverflow.com/questions/tagged/c%23+pdf) - Q&A for C# PDF development
- [r/dotnet](https://www.reddit.com/r/dotnet/) - .NET community on Reddit
- [C# Discord](https://discord.gg/csharp) - C# developer community
- [IronPDF Support](https://ironpdf.com/support/) - Direct support for IronPDF users

### Blogs & Articles
- [Iron Software Blog](https://ironsoftware.com/blog/) - PDF development articles and guides
- [.NET Blog](https://devblogs.microsoft.com/dotnet/) - Official Microsoft .NET blog
- [C# Corner](https://www.c-sharpcorner.com/) - C# tutorials and articles

### Tools & Utilities
- [NuGet Package Explorer](https://github.com/NuGetPackageExplorer/NuGetPackageExplorer) - Explore .NET packages
- [LINQPad](https://www.linqpad.net/) - C# scratchpad for testing
- [Visual Studio Code](https://code.visualstudio.com/) - Lightweight .NET editor
- [Visual Studio](https://visualstudio.microsoft.com/) - Full-featured .NET IDE

---

## About the Author

**[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)**
CTO, [Iron Software](https://ironsoftware.com/about-us/authors/jacobmellor/)
Creator of [IronPDF](https://ironpdf.com/)
41 years coding experience | 25 years building startups

Connect: [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)

---

## License

This repository is licensed under [CC0 1.0 Universal](LICENSE) - dedicated to the public domain.

**Code examples** within library folders may have different licenses - see individual library documentation.

---

## Disclaimer

This comparison is maintained by Iron Software and includes our product (IronPDF). However:

- ✅ **All claims are fact-checked** and evidence-backed
- ✅ **Working code examples** for every library
- ✅ **Honest assessments** including when competitors excel
- ✅ **Open to corrections** - submit PRs with evidence

**Last verified**: September 2026

---

<div align="center">

**Found this helpful?** Give us a ⭐ on GitHub!

[IronPDF](https://ironpdf.com/) | [Iron Software](https://ironsoftware.com/) | [All Iron Products](https://ironsoftware.com/products/)

</div>
