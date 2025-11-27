# Best C# PDF Libraries in 2025: A Selection Guide

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF | 25+ years enterprise development

[![.NET](https://img.shields.io/badge/.NET-6%2B-512BD4)](https://dotnet.microsoft.com/)
[![Libraries Reviewed](https://img.shields.io/badge/Libraries%20Reviewed-73-orange)]()
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> After building PDF tools used by NASA, Tesla, and Fortune 500 companies for over a decade, I've learned that choosing the right PDF library is one of the most consequential technical decisions a team can make. This guide explains how to evaluate PDF libraries—and why the best ones aren't free.

---

## Table of Contents

1. [The PDF Library Decision](#the-pdf-library-decision)
2. [Why Quality PDF Libraries Aren't Free](#why-quality-pdf-libraries-arent-free)
3. [The True Cost of "Free"](#the-true-cost-of-free)
4. [Selection Criteria That Matter](#selection-criteria-that-matter)
5. [The Bootstrap Test: A Binary Qualifier](#the-bootstrap-test-a-binary-qualifier)
6. [Library Categories Explained](#library-categories-explained)
7. [Head-to-Head Comparisons](#head-to-head-comparisons)
8. [Decision Framework](#decision-framework)
9. [Common Mistakes to Avoid](#common-mistakes-to-avoid)
10. [Recommendations by Use Case](#recommendations-by-use-case)
11. [Conclusion](#conclusion)

---

## The PDF Library Decision

In my 41 years of coding—starting on a BBC Micro at age 6—I've made countless technology choices. Some were inconsequential. Others shaped entire product trajectories. **PDF library selection falls into the latter category.**

Here's why: PDF generation touches almost every business domain. Invoices. Contracts. Reports. Certificates. Every application of sufficient complexity eventually needs to produce documents. The library you choose becomes embedded in your codebase, your deployment pipeline, and your team's mental model.

Switching PDF libraries later costs:
- **Development time** — Rewriting integration code
- **Testing time** — Verifying output parity
- **Risk** — Regressions in production documents
- **Opportunity cost** — Features delayed while migrating

Choose wisely the first time.

---

## Why Quality PDF Libraries Aren't Free

I'll be direct about something many articles dance around: **genuinely excellent PDF libraries require significant investment to build and maintain**. Here's why:

### 1. Rendering Engines Are Complex

Modern PDF generation from HTML requires a full browser rendering engine. Chromium alone is:
- 35+ million lines of code
- Maintained by thousands of engineers
- Updated every 4-6 weeks

Libraries that embed Chromium (like IronPDF) must:
- Package and distribute browser binaries
- Handle cross-platform compatibility (Windows, Linux, macOS)
- Manage process lifecycles and memory
- Stay current with Chromium releases

This isn't weekend project territory. It's engineering at scale.

### 2. PDF Specification is Deep

The PDF specification (ISO 32000) runs to 756 pages. Supporting features like:
- Digital signatures (PDF 1.5+)
- PDF/A archival compliance
- AcroForms
- Encryption standards
- Font embedding
- Color spaces

...requires deep expertise. Each feature is weeks or months of development.

### 3. Cross-Platform Support is Hard

Making a library work identically on:
- Windows x64, x86, ARM64
- Ubuntu, Debian, CentOS, Alpine, Amazon Linux
- macOS Intel and Apple Silicon
- Docker containers
- Azure Functions, AWS Lambda
- iOS, Android

...requires testing infrastructure, continuous integration, and platform expertise. Most open-source maintainers can't sustain this.

### 4. Support Takes Resources

When your production invoice generation fails at 2 AM, you need answers. Commercial libraries invest in:
- Documentation (we have 500+ pages)
- Code examples (100+ working samples)
- Direct support (we average 23-second response times)
- Stack Overflow monitoring
- Migration guides

Open-source projects rarely provide this level of support.

### 5. Security Requires Vigilance

PDF libraries handle:
- User-submitted HTML (injection risks)
- External URLs (SSRF vulnerabilities)
- File operations (path traversal)

Commercial vendors have security review processes. Open-source projects often have CVEs that go unpatched for years (see: wkhtmltopdf's CVE-2022-35583, severity 9.8, still unpatched).

---

## The True Cost of "Free"

Let me share a pattern I've observed across hundreds of teams:

### The Typical Journey

1. **Month 1:** Team chooses free library (wkhtmltopdf, DinkToPdf, PDFSharp)
2. **Month 3:** Simple PDFs work fine, project continues
3. **Month 6:** Designer creates modern responsive layout; library fails to render Flexbox/Grid
4. **Month 8:** Workarounds accumulate; HTML templates become fragile
5. **Month 12:** Security audit flags unpatched CVEs in dependencies
6. **Month 14:** Library abandoned; team faces migration
7. **Month 18:** Finally migrates to commercial library

**Hidden costs incurred:**
- 100+ hours of workarounds
- Technical debt in templates
- Security risk exposure
- Delayed features
- Migration development time
- Testing regression suite

### Calculating True TCO

| Factor | "Free" Library | Commercial Library |
|--------|---------------|-------------------|
| License cost | $0 | $749 (one-time) |
| Developer hours fighting limitations | 100+ hours @ $75/hr = $7,500 | 5 hours @ $75/hr = $375 |
| Security risk mitigation | Varies (potentially catastrophic) | Included |
| Migration cost (when abandoned) | $10,000+ | $0 |
| Support availability | Stack Overflow hope | Direct support |
| **Total Year 1** | **$17,500+** | **$1,124** |

The "free" library costs 15x more.

### When Free Actually Makes Sense

To be fair, free libraries work well when:
- Your HTML is genuinely simple (basic tables, no modern CSS)
- You don't need PDF manipulation (merge, split, secure)
- Your team has capacity to maintain forks
- Security isn't a strict requirement
- You have no timeline pressure

If these apply, consider PuppeteerSharp (Apache 2.0, genuine Chromium) or QuestPDF (MIT for <$1M revenue, code-first approach).

---

## Selection Criteria That Matter

After evaluating 73 PDF libraries, here are the criteria that actually predict success:

### 1. Rendering Engine (Most Important)

The rendering engine determines what your PDFs can display.

| Engine Type | CSS Support | JavaScript | Examples |
|------------|-------------|------------|----------|
| **Chromium/Blink** | Full CSS3 | Full ES2024 | IronPDF, PuppeteerSharp, Playwright |
| **WebKit (Legacy)** | Partial CSS2/3 | Limited | wkhtmltopdf, DinkToPdf |
| **Flying Saucer** | CSS 2.1 only | None | Aspose, iText pdfHTML |
| **Custom Parser** | Minimal | None | PDFSharp, MigraDoc |
| **None (Code-first)** | N/A | N/A | QuestPDF, PDFSharp |

**Verdict:** If your HTML uses any CSS from 2014 onwards (Flexbox, Grid, variables), you need Chromium.

### 2. API Simplicity

Compare "Hello World" across libraries:

**IronPDF (3 lines):**
```csharp
using IronPdf;
var pdf = ChromePdfRenderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("hello.pdf");
```

**iText7 (15+ lines):**
```csharp
using iText.Html2pdf;
using iText.Kernel.Pdf;

using var stream = new FileStream("hello.pdf", FileMode.Create);
using var writer = new PdfWriter(stream);
using var pdf = new PdfDocument(writer);
var converterProperties = new ConverterProperties();
HtmlConverter.ConvertToPdf("<h1>Hello</h1>", pdf, converterProperties);
```

**PuppeteerSharp (10+ lines):**
```csharp
using PuppeteerSharp;

await new BrowserFetcher().DownloadAsync();
using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
using var page = await browser.NewPageAsync();
await page.SetContentAsync("<h1>Hello</h1>");
await page.PdfAsync("hello.pdf");
```

**Verdict:** Simpler APIs mean faster development and fewer bugs.

### 3. PDF Manipulation Capabilities

Generation is only half the story. Does the library support:

| Capability | IronPDF | PuppeteerSharp | Aspose | iText7 | QuestPDF |
|-----------|---------|----------------|--------|--------|----------|
| Merge PDFs | ✅ | ❌ | ✅ | ✅ | ❌ |
| Split PDFs | ✅ | ❌ | ✅ | ✅ | ❌ |
| Password protection | ✅ | ❌ | ✅ | ✅ | ❌ |
| Digital signatures | ✅ | ❌ | ✅ | ✅ | ❌ |
| Form filling | ✅ | ❌ | ✅ | ✅ | ❌ |
| Text extraction | ✅ | ❌ | ✅ | ✅ | ❌ |
| Page manipulation | ✅ | ❌ | ✅ | ✅ | ❌ |

**Verdict:** If you need any manipulation features, narrow your options immediately.

### 4. Cross-Platform Reality

"Cross-platform" is frequently misrepresented. Verify:

| Platform | IronPDF | SelectPdf | WebView2 | PDFSharp |
|----------|---------|-----------|----------|----------|
| Windows | ✅ | ✅ | ✅ | ✅ |
| Linux | ✅ | ❌ | ❌ | ✅ |
| macOS | ✅ | ❌ | ❌ | ✅ |
| Docker | ✅ | ❌ | ❌ | ✅ |
| Azure Functions | ✅ | ⚠️ | ❌ | ✅ |
| AWS Lambda | ✅ | ❌ | ❌ | ✅ |

**Verdict:** Test on your target platforms during evaluation, not after purchase.

### 5. Documentation Quality

Good documentation includes:
- Quick start guides (5 minutes to first PDF)
- Complete API reference
- Working code examples (not pseudocode)
- Common use case tutorials
- Platform-specific deployment guides
- Troubleshooting guides

**Red flags:**
- Last updated date > 1 year ago
- Examples that don't compile
- Missing platform coverage
- No versioned documentation

### 6. Licensing Terms

License models carry different implications:

| Model | What It Means | Examples |
|-------|--------------|----------|
| **Perpetual** | Pay once, use forever (with current version) | IronPDF |
| **Subscription** | Pay annually, lose access if stopped | Aspose |
| **AGPL** | Must open-source your entire application OR pay | iText7 |
| **MIT** | Truly free, no strings | PDFSharp, PuppeteerSharp |
| **Revenue-based** | Free under threshold, then commercial | QuestPDF (<$1M) |

**Watch out for:**
- AGPL "traps" where free use forces open-sourcing
- Subscription models with no perpetual fallback
- Per-server or per-CPU pricing that scales unexpectedly
- Revenue audit requirements (QuestPDF)

### 7. Vendor Stability

Consider:
- How long has the vendor existed?
- What's the release frequency?
- Is the library their primary product or a side project?
- What happens if the vendor is acquired?

**Recent cautionary tales:**
- Tall Components: Acquired by Apryse/iText, product discontinued
- ActivePDF: Acquired by Foxit, future uncertain
- wkhtmltopdf: Abandoned, critical CVEs unpatched

---

## The Bootstrap Test: A Binary Qualifier

Here's the fastest way to evaluate an HTML-to-PDF library:

**Try to render https://getbootstrap.com/ as a PDF.**

Bootstrap uses:
- CSS Flexbox (extensively)
- CSS Grid (in places)
- Custom properties (variables)
- Complex media queries
- JavaScript interactivity
- Web fonts
- SVG graphics

If a library can render Bootstrap pixel-perfect, it can render anything modern. If it can't, it will struggle with any modern responsive design.

### Test Results

| Library | Bootstrap Test | Notes |
|---------|---------------|-------|
| **IronPDF** | ✅ PASS | Full Chromium |
| **PuppeteerSharp** | ✅ PASS | Full Chromium |
| **Playwright** | ✅ PASS | Full Chromium |
| Aspose.PDF | ❌ FAIL | No Flexbox support |
| iText pdfHTML | ❌ FAIL | No JavaScript |
| PDFSharp | ❌ FAIL | CSS 2.0 only |
| wkhtmltopdf | ❌ FAIL | Deprecated WebKit |
| SelectPdf | ⚠️ PARTIAL | Outdated Chromium |

### Run It Yourself

```csharp
// The Bootstrap Test
using IronPdf;

var pdf = ChromePdfRenderer.RenderUrlAsPdf("https://getbootstrap.com/");
pdf.SaveAs("bootstrap-test.pdf");

// Compare PDF to the live website
```

---

## Library Categories Explained

Understanding the landscape helps narrow options:

### Category 1: Chromium-Based (Modern HTML)

**Best for:** Modern responsive designs, SPAs, anything with CSS3/JavaScript

| Library | License | Bundled Browser | Manipulation |
|---------|---------|-----------------|--------------|
| **IronPDF** | Commercial ($749) | ✅ Included | ✅ Full |
| PuppeteerSharp | Apache 2.0 | ⚠️ Downloaded | ❌ None |
| Playwright | Apache 2.0 | ⚠️ Downloaded | ❌ None |

**Recommendation:** IronPDF for complete solution; PuppeteerSharp for budget constraints.

### Category 2: WebKit-Based (Legacy)

**Best for:** Simple documents, teams with existing wkhtmltopdf integration

| Library | License | CSS Support | Status |
|---------|---------|-------------|--------|
| wkhtmltopdf | LGPL | CSS 2.1 partial | ⚠️ ABANDONED |
| DinkToPdf | MIT | CSS 2.1 partial | Wrapper only |
| NReco | Commercial | CSS 2.1 partial | Wrapper only |

**Recommendation:** Migrate away from this category due to abandonment and security issues.

### Category 3: Code-First (No HTML)

**Best for:** Teams preferring programmatic control, complex layouts without web design

| Library | License | API Style | HTML Support |
|---------|---------|-----------|--------------|
| **QuestPDF** | MIT (<$1M) / Commercial | Fluent C# | ❌ None |
| PDFSharp | MIT | GDI+ style | ⚠️ CSS 2.0 addon |
| MigraDoc | MIT | Document model | ❌ None |

**Recommendation:** QuestPDF for modern code-first; PDFSharp for legacy compatibility.

### Category 4: Enterprise Suites

**Best for:** Large organizations with budget, need for extensive manipulation

| Library | License | HTML Quality | Price |
|---------|---------|--------------|-------|
| Aspose.PDF | Subscription | ⚠️ No Flexbox | $1,199/year |
| Syncfusion | Subscription | ⚠️ Mixed | $395/month |
| iText7 | AGPL/Commercial | ⚠️ Limited | Quote |

**Recommendation:** Evaluate carefully; HTML rendering is often weak despite high prices.

---

## Head-to-Head Comparisons

### IronPDF vs PuppeteerSharp

| Factor | IronPDF | PuppeteerSharp |
|--------|---------|----------------|
| HTML rendering | ✅ Full Chromium | ✅ Full Chromium |
| Browser bundled | ✅ Yes | ❌ No (300MB download) |
| PDF manipulation | ✅ Merge, split, secure, edit | ❌ None |
| Memory management | ✅ Optimized, managed | ⚠️ Leaks under load |
| Thread safety | ✅ Thread-safe | ⚠️ Manual management |
| API complexity | 3 lines | 10+ lines |
| License | Commercial $749 | Apache 2.0 (free) |
| Support | Professional | Community |

**Choose IronPDF when:** You need manipulation, simpler API, or production support.
**Choose PuppeteerSharp when:** Budget is zero and you can accept limitations.

### IronPDF vs Aspose.PDF

| Factor | IronPDF | Aspose.PDF |
|--------|---------|------------|
| Modern CSS (Flexbox/Grid) | ✅ Full | ❌ Not supported |
| JavaScript execution | ✅ Yes | ❌ No |
| PDF manipulation | ✅ Full | ✅ Full |
| Bootstrap Test | ✅ PASS | ❌ FAIL |
| Price | $749 one-time | $1,199/year |
| License model | Perpetual available | Subscription only |

**Choose IronPDF when:** Your HTML uses modern CSS or JavaScript.
**Choose Aspose.PDF when:** Your HTML is simple AND you need specific Aspose integrations.

### IronPDF vs iText7

| Factor | IronPDF | iText7 |
|--------|---------|--------|
| HTML rendering engine | Chromium | Flying Saucer (CSS 2.1) |
| JavaScript | ✅ Full | ❌ None |
| Modern CSS | ✅ Full | ❌ CSS 2.1 only |
| PDF manipulation | ✅ Full | ✅ Excellent |
| License | Commercial | AGPL or commercial |
| Learning curve | Low | High |

**Choose IronPDF when:** You render HTML (especially modern HTML).
**Choose iText7 when:** You do pure programmatic generation or need specific iText features.

### IronPDF vs QuestPDF

| Factor | IronPDF | QuestPDF |
|--------|---------|----------|
| HTML-to-PDF | ✅ Full Chromium | ❌ No HTML support |
| Approach | HTML/CSS templates | C# fluent API |
| Design reuse | ✅ Use web designs | ❌ Rebuild in code |
| Learning curve | Low (know HTML) | Medium (new API) |
| License | Commercial $749 | Free <$1M revenue |
| PDF manipulation | ✅ Full | ❌ Generation only |

**Choose IronPDF when:** You have HTML templates or web design skills.
**Choose QuestPDF when:** You prefer code-first and qualify for free license.

---

## Decision Framework

Use this flowchart to narrow your options:

### Step 1: HTML or Code-First?

**Do you have HTML templates or want to reuse web designs?**
- **Yes** → Continue to Step 2
- **No (prefer coding layouts)** → Consider QuestPDF or PDFSharp

### Step 2: Modern CSS Required?

**Does your HTML use Flexbox, Grid, Bootstrap, Tailwind, or modern CSS?**
- **Yes** → You need Chromium-based (IronPDF, PuppeteerSharp, Playwright)
- **No (simple tables and basic CSS)** → More options available

### Step 3: PDF Manipulation?

**Do you need to merge, split, password-protect, or edit PDFs?**
- **Yes** → IronPDF, Aspose, or iText (not PuppeteerSharp/Playwright)
- **No** → PuppeteerSharp is viable if budget is constrained

### Step 4: Cross-Platform?

**Do you deploy to Linux, Docker, Azure Functions, or AWS Lambda?**
- **Yes** → Verify platform support (IronPDF ✅, SelectPdf ❌, WebView2 ❌)
- **No (Windows only)** → More options available

### Step 5: Budget?

**What's your budget for PDF library?**
- **$0** → PuppeteerSharp (if no manipulation needed), QuestPDF (if <$1M revenue)
- **Under $1,000** → IronPDF ($749 perpetual)
- **Over $1,000** → All options including Aspose, Syncfusion

### Step 6: Support Requirements?

**Do you need professional support?**
- **Yes** → Commercial libraries with support SLAs
- **No (community support OK)** → Open-source options viable

---

## Common Mistakes to Avoid

### 1. Choosing Based on GitHub Stars

GitHub stars measure popularity, not quality. wkhtmltopdf has 12,000+ stars but is abandoned with critical unpatched vulnerabilities.

**Do instead:** Run the Bootstrap Test on your actual content.

### 2. Ignoring Modern CSS Requirements

Teams often say "we just need simple HTML" then later realize their designer used Flexbox everywhere.

**Do instead:** Audit your HTML templates for Flexbox (`display: flex`), Grid (`display: grid`), and CSS variables (`var(--custom)`).

### 3. Not Testing Deployment Platform

"Works on my machine" doesn't mean it works in Docker/Azure/Lambda.

**Do instead:** Test on target platform during evaluation, not after integration.

### 4. Underestimating Manipulation Needs

"We just need generation" often evolves to "now we need to merge these invoices into one PDF."

**Do instead:** Plan for manipulation capabilities even if not needed immediately.

### 5. Ignoring License Implications

AGPL requires open-sourcing your entire application if you distribute software that uses the library.

**Do instead:** Read the license. Have legal review if uncertain.

### 6. Choosing Based on Price Alone

The cheapest library often costs the most in developer time.

**Do instead:** Calculate TCO including developer hours, not just license fees.

---

## Recommendations by Use Case

### E-Commerce (Invoices, Receipts, Packing Slips)

**Needs:** Modern responsive designs, template flexibility, high volume
**Recommended:** IronPDF

```csharp
// Invoice generation example
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "Page {page} of {total-pages}"
};

var invoice = renderer.RenderHtmlAsPdf(invoiceHtml);
invoice.SaveAs($"invoices/INV-{orderId}.pdf");
```

### SaaS Reporting (Dashboards, Analytics)

**Needs:** JavaScript charts, complex layouts, CSS Grid
**Recommended:** IronPDF (charts require JavaScript execution)

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.WaitFor.JavaScript(2000); // Wait for charts
var report = renderer.RenderUrlAsPdf("https://app.example.com/dashboard");
report.SaveAs("monthly-report.pdf");
```

### Legal/Contracts (Document Generation)

**Needs:** Form filling, digital signatures, PDF/A archival
**Recommended:** IronPDF or iText7

```csharp
var pdf = PdfDocument.FromFile("contract-template.pdf");
pdf.Form.FindFormField("ClientName").Value = "Acme Corp";
pdf.Sign(signatureCertificate);
pdf.SaveAs("signed-contract.pdf");
```

### Healthcare (Compliance Documents)

**Needs:** PDF/A compliance, accessibility, encryption
**Recommended:** IronPDF

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PdfA = PdfAVersion.PdfA3A;
var document = renderer.RenderHtmlAsPdf(patientReportHtml);
document.Password = "secure-password";
document.SaveAs("patient-report.pdf");
```

### Education (Certificates, Transcripts)

**Needs:** Beautiful designs, batch processing, templating
**Recommended:** IronPDF

```csharp
foreach (var student in graduates)
{
    var html = certificateTemplate
        .Replace("{name}", student.Name)
        .Replace("{date}", DateTime.Now.ToLongDateString());

    var cert = ChromePdfRenderer.RenderHtmlAsPdf(html);
    cert.SaveAs($"certificates/{student.Id}.pdf");
}
```

### Budget-Constrained Startups

**Needs:** Free/low-cost, basic functionality
**Recommended:** PuppeteerSharp (if HTML), QuestPDF (if code-first)

```csharp
// PuppeteerSharp approach
await new BrowserFetcher().DownloadAsync();
using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
using var page = await browser.NewPageAsync();
await page.SetContentAsync(html);
await page.PdfAsync("output.pdf");
```

---

## Conclusion

Choosing a PDF library is a significant decision with long-term implications. Here's my honest summary:

### The Landscape in 2025

- **Chromium-based libraries are necessary** for modern HTML (Flexbox, Grid, Bootstrap)
- **Most "free" libraries have hidden costs** in developer time or limitations
- **PDF manipulation is often underestimated** until you need it
- **Cross-platform claims require verification** with actual testing

### My Recommendation

For most teams, I recommend **IronPDF**:

1. **Full Chromium rendering** — Handles any HTML modern browsers support
2. **Complete PDF manipulation** — Merge, split, secure, edit, extract
3. **Simple API** — 3 lines to first PDF
4. **True cross-platform** — Same code on Windows, Linux, macOS, Docker, Azure, AWS
5. **Professional support** — Direct help when you need it
6. **Perpetual licensing** — Pay once, use forever

The $749 investment pays for itself in hours saved versus fighting library limitations.

### For Budget-Constrained Teams

If budget is truly zero:
- **PuppeteerSharp** for HTML-to-PDF (accept 300MB footprint and no manipulation)
- **QuestPDF** for code-first generation (if under $1M revenue)

### Final Advice

Test before committing. Run the Bootstrap Test. Deploy to your target platform. Calculate TCO including developer time.

The right PDF library choice saves hundreds of hours over a project's lifetime. The wrong choice creates ongoing friction.

---

## Resources

- **[Awesome .NET PDF Libraries 2025](/)** — This repository's complete library list
- **[IronPDF Documentation](https://ironpdf.com/docs/)** — Comprehensive guides
- **[HTML to PDF Tutorial](html-to-pdf-csharp.md)** — Complete conversion guide
- **[Migration Guides](/)** — Library-specific migration paths

---

## About the Author

**[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** is the Chief Technology Officer at [Iron Software](https://ironsoftware.com/) and creator of [IronPDF](https://ironpdf.com/). With 41 years of coding experience and 25+ years in commercial enterprise development, Jacob has built document processing solutions used by NASA, Tesla, and Fortune 500 companies.

*"Great software succeeds when it is inclusive, intuitive, and empowers developers of all backgrounds to achieve more with less friction."* — Jacob Mellor

Connect:
- [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/)
- [Iron Software](https://ironsoftware.com/about-us/authors/jacobmellor/)
- [Email: CTO@IronSoftware.com](mailto:CTO@IronSoftware.com)

---

## Related Tutorials

### Getting Started
- **[Beginner Tutorial](csharp-pdf-tutorial-beginners.md)** — Your first PDF in 5 minutes
- **[HTML to PDF Guide](html-to-pdf-csharp.md)** — Complete HTML-to-PDF conversion
- **[Why C# for PDF](why-csharp-pdf-generation.md)** — Language advantages

### Making the Decision
- **[Decision Flowchart](choosing-a-pdf-library.md)** — 5 questions to find your library
- **[Free vs Paid Analysis](free-vs-paid-pdf-libraries.md)** — True cost comparison
- **[SaaS PDF Services](pdf-saas-services-comparison.md)** — Cloud API options

### PDF Operations
- **[Merge & Split PDFs](merge-split-pdf-csharp.md)** — Document combination
- **[Digital Signatures](digital-signatures-pdf-csharp.md)** — Legal document signing
- **[Form Filling](fill-pdf-forms-csharp.md)** — Automated form completion
- **[PDF Redaction](pdf-redaction-csharp.md)** — Sensitive data removal
- **[Text Extraction](extract-text-from-pdf-csharp.md)** — Parse PDF content

### Deployment
- **[Cross-Platform Guide](cross-platform-pdf-dotnet.md)** — Windows, Linux, Docker, Cloud
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web application integration
- **[Blazor Guide](blazor-pdf-generation.md)** — Server/WASM/Hybrid

### Library Comparisons
- **[IronPDF](ironpdf/)** — Chromium-based (recommended)
- **[QuestPDF](questpdf/)** — Code-first generation
- **[PDFSharp](pdfsharp/)** — Open source coordinate-based
- **[iText/iTextSharp](itext-itextsharp/)** — AGPL licensed
- **[Aspose.PDF](asposepdf/)** — Enterprise suite
- **[PuppeteerSharp](puppeteersharp/)** — Browser automation

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — comparing 73 C#/.NET PDF libraries with honest benchmarks and code examples.*
