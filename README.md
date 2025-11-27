# Awesome .NET PDF Libraries 2025

[![Awesome](https://awesome.re/badge.svg)](https://awesome.re)
[![CC0 License](https://licensebuttons.net/p/zero/1.0/88x31.png)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)
[![Last Updated](https://img.shields.io/badge/Last%20Updated-November%202025-blue.svg)]()
[![GitHub Stars](https://img.shields.io/github/stars/iron-software/awesome-dotnet-pdf-libraries-2025?style=social)](https://github.com/iron-software/awesome-dotnet-pdf-libraries-2025)

> The most comprehensive comparison of every C# and .NET PDF library in 2025 - with honest benchmarks, code examples, and migration guides.

A curated collection of **73 C#/.NET PDF libraries** for creating, manipulating, converting, and rendering PDF documents.

Inspired by [awesome-dotnet](https://github.com/quozd/awesome-dotnet), [awesome-python](https://github.com/vinta/awesome-python), and the [Awesome Lists](https://github.com/sindresorhus/awesome) movement.

**Contributions are welcome!** Please see the [contribution guidelines](CONTRIBUTING.md) first. We accept both open source and commercial libraries.

Thanks to all contributors - this project wouldn't exist without the community!

**Compiled by [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)**, CTO of [Iron Software](https://ironsoftware.com/about-us/authors/jacobmellor/) | Creator of [IronPDF](https://ironpdf.com/)

---

## Table of Contents

- [What Makes This Different](#what-makes-this-different)
- [The Bootstrap Homepage Test](#the-bootstrap-homepage-test)
- [Quick Recommendations](#quick-recommendations)
- **[Categories](#categories)**
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
- **[Best PDF Libraries 2025](best-pdf-libraries-dotnet-2025.md)** — Comprehensive comparison
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

## What Makes This Different

**This is not a marketing list.** Every library comparison includes:

✅ **Working code examples** - Actual compilable C# code
✅ **Verified claims** - Evidence-backed technical limitations
✅ **Real pricing** - Current costs as of November 2025
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

Can your library render [Bootstrap's homepage](https://getbootstrap.com/) pixel-perfect, identical to Chrome?

| Library | Passes Test | Reason |
|---------|-------------|--------|
| **[IronPDF](ironpdf/)** | ✅ YES | Full Chromium rendering engine |
| **[PuppeteerSharp](puppeteersharp/)** | ✅ YES | Real Chromium browser automation |
| **[Playwright](playwright/)** | ✅ YES | Microsoft's Chromium automation |
| **[Aspose.PDF](asposepdf/)** | ❌ NO | [No Flexbox support](https://forum.aspose.com/t/convert-html-with-flex-grid/42156) |
| **[iText7](itext-itextsharp/)** | ❌ NO | [No JavaScript execution](https://kb.itextpdf.com/itext/evaluating-js-with-pdfhtml) |
| **[PDFSharp](pdfsharp/)** | ❌ NO | CSS 2.0 only (no Grid/Flexbox) |
| **[Syncfusion WebKit](syncfusion-pdf/)** | ❌ NO | Legacy WebKit engine |

**Why this matters**: Bootstrap uses modern CSS3 Flexbox heavily. If a library can't render Bootstrap, it can't handle modern responsive web designs.

📖 **Read more**: [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)

---

## Quick Recommendations

### 🏆 Best for Modern Web-to-PDF (2025)
**[IronPDF](ironpdf/)** - Full Chromium, 3-line API, cross-platform, built-in PDF manipulation.
```csharp
// NuGet: Install-Package IronPdf
var pdf = ChromePdfRenderer.RenderUrlAsPdf("https://getbootstrap.com/");
pdf.SaveAs("bootstrap.pdf"); // Pixel-perfect!
```

### 🆓 Best Free/Open Source
**[QuestPDF](questpdf/)** - Modern fluent API for code-first PDF generation (not HTML).
**[PuppeteerSharp](puppeteersharp/)** - Full Chromium with Apache license (HTML-to-PDF).
**[PDFSharp](pdfsharp/)** - Mature programmatic generation (limited HTML support).

### 💼 Best Enterprise Suite
**[Aspose.PDF](asposepdf/)** - Comprehensive features, but [$1,199/year](https://purchase.aspose.com/pricing/pdf/net) and [limited modern CSS](https://forum.aspose.com/t/display-flex-not-working/203245).
**[Syncfusion](syncfusion-pdf/)** - Lower cost alternative at $395/month.

### 🎯 Best for Specific Use Cases
- **Reports**: [FastReport.NET](fastreport/), [Telerik Reporting](telerik-reporting/)
- **Forms/Templates**: [iText7](itext-itextsharp/), [Spire.PDF](spirepdf/)
- **Code-First Design**: [QuestPDF](questpdf/), [MigraDoc](migradoc/)
- **Cloud/API**: [Api2pdf](api2pdf/), [Gotenberg](gotenberg/)

---

## Categories

### 1. HTML-to-PDF (Chromium/Blink-Based)

Modern libraries using Chromium/Blink rendering engine for pixel-perfect HTML-to-PDF conversion with full CSS3/JavaScript support.

#### 1.1 [IronPDF](ironpdf/) ⭐ **Reference Standard**
**Commercial** | [Official Site](https://ironpdf.com/) | **Full Chromium** | Cross-platform
- ✅ 3-line API for HTML/URL to PDF
- ✅ Passes Bootstrap homepage test
- ✅ Built-in PDF manipulation (merge, split, secure, edit)
- ✅ True cross-platform (Windows/Linux/macOS/iOS/Android via [gRPC](https://ironpdf.com/docs/questions/grpc/))
- ✅ [Extensive examples](https://ironpdf.com/examples/)
- 📖 [Documentation](https://ironpdf.com/docs/) | [API Reference](https://ironpdf.com/object-reference/api/)

#### 1.2 [PuppeteerSharp](puppeteersharp/)
**Free (Apache 2.0)** | [GitHub](https://github.com/hardkoded/puppeteer-sharp) | **Full Chromium**
- ✅ Passes Bootstrap homepage test
- ✅ Free and open source
- ⚠️ 300MB+ deployment (bundles Chromium)
- ⚠️ Generation only (no PDF manipulation)
- ⚠️ Memory leaks under load
- 📚 [Migration Guide](puppeteersharp/migrate-from-puppeteersharp.md)

#### 1.3 [Playwright for .NET](playwright/)
**Free (Apache 2.0)** | [Official Site](https://playwright.dev/dotnet/) | **Multi-browser**
- ✅ Supports Chromium, Firefox, WebKit
- ✅ Passes Bootstrap homepage test (Chromium mode)
- ⚠️ Testing-first design (PDF is secondary)
- ⚠️ Downloads 3 browsers (~400MB+)
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

#### 4.2 [Syncfusion PDF Framework](syncfusion-pdf/)
**Commercial ($395/month)** | [Official Site](https://www.syncfusion.com/pdf-framework/net)
- ✅ Lower cost than Aspose
- ✅ WebKit + Blink engines
- ⚠️ WebKit version has limitations
- 📚 [Migration Guide](syncfusion-pdf/migrate-from-syncfusion-pdf.md)

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

#### 4.11 [Docotic.Pdf](docotic/)
**Free/Commercial** | [Official Site](https://bitmiracle.com/pdf-library/)
- 📚 [Migration Guide](docotic/migrate-from-docotic.md)

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

#### 4.16 [Tall Components (TallPDF)](tallcomponents/)
**Commercial** | [Official Site](https://www.tallcomponents.com/)
- 📚 [Migration Guide](tallcomponents/migrate-from-tallcomponents.md)

#### 4.17 [Gnostice (Document Studio)](gnostice/)
**Commercial** | [Official Site](https://www.gnostice.com/)
- 📚 [Migration Guide](gnostice/migrate-from-gnostice.md)

#### 4.18 [BCL EasyPDF SDK](bcl-easypdf/)
**Commercial** | [Official Site](https://www.pdfonline.com/easypdf-sdk/)
- 📚 [Migration Guide](bcl-easypdf/migrate-from-bcl-easypdf.md)

#### 4.19 [Foxit SDK](foxit/)
**Commercial** | [Official Site](https://developers.foxit.com/)
- 📚 [Migration Guide](foxit/migrate-from-foxit.md)

#### 4.20 [Adobe PDF Library SDK](adobe/)
**Enterprise** | [Official Site](https://www.adobe.com/devnet/pdf.html)
- 📚 [Migration Guide](adobe/migrate-from-adobe.md)

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

#### 5.3 [Kaizen.io HTML-to-PDF](kaizenio/)
**SaaS** | [Official Site](https://www.kaizen.io/)
- 📚 [Migration Guide](kaizenio/migrate-from-kaizenio.md)

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

#### 6.1 [SAP Crystal Reports](crystal-reports/)
**Commercial** | [Official Site](https://www.sap.com/products/crystal-reports.html)
- 📚 [Migration Guide](crystal-reports/migrate-from-crystal-reports.md)

#### 6.2 [FastReport.NET](fastreport/)
**Free/Commercial** | [Official Site](https://www.fast-report.com/)
- 📚 [Migration Guide](fastreport/migrate-from-fastreport.md)

#### 6.3 [Telerik Reporting](telerik-reporting/)
**Commercial** | [Official Site](https://www.telerik.com/products/reporting.aspx)
- 📚 [Migration Guide](telerik-reporting/migrate-from-telerik-reporting.md)

#### 6.4 [Scryber.core](scryber/)
**Free (LGPL)** | [GitHub](https://github.com/richard-scryber/scryber.core)
- 📚 [Migration Guide](scryber/migrate-from-scryber.md)

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

#### 7.3 [Pdfium.NET](pdfiumnet/)
**Free** | [GitHub](https://github.com/mrdooz/PdfiumNet)
- 📚 [Migration Guide](pdfiumnet/migrate-from-pdfiumnet.md)

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

#### 9.2 [PDF Duo .NET](pdfduo/)
**Commercial (Discontinued)**
- 📚 [Migration Guide](pdfduo/migrate-from-pdfduo.md)

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

#### 10.7 [XFINIUM.PDF](xfinium/)
**Commercial** | [Official Site](https://www.xfiniumpdf.com/)
- 📚 [Migration Guide](xfinium/migrate-from-xfinium.md)

#### 10.8 [Sumatra PDF](sumatra/)
**Free (GPL)** | [Official Site](https://www.sumatrapdfreader.org/)
- 📚 [Migration Guide](sumatra/migrate-from-sumatra.md)

#### 10.9 [Apache PDFBox](pdfbox/)
**Free (Apache 2.0)** | [Official Site](https://pdfbox.apache.org/)
- ⚠️ Java library, .NET ports experimental
- 📚 [Migration Guide](pdfbox/migrate-from-pdfbox.md)

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

## Pricing Comparison (November 2025)

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

**Last verified**: November 2025

---

<div align="center">

**Found this helpful?** Give us a ⭐ on GitHub!

[IronPDF](https://ironpdf.com/) | [Iron Software](https://ironsoftware.com/) | [All Iron Products](https://ironsoftware.com/products/)

</div>
