# How to Choose a C# PDF Library: Decision Flowchart and Comparison

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF

[![Decision Guide](https://img.shields.io/badge/Guide-Decision%20Flowchart-blue)]()
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> After reviewing 73 C# PDF libraries and helping 50,000+ developers, I've distilled the selection process into a simple flowchart. Answer 5 questions, get your recommendation.

---

## Table of Contents

1. [The Decision Flowchart](#the-decision-flowchart)
2. [Question 1: Do You Have HTML?](#question-1-do-you-have-html)
3. [Question 2: Modern CSS?](#question-2-modern-css)
4. [Question 3: Need Manipulation?](#question-3-need-manipulation)
5. [Question 4: Budget?](#question-4-budget)
6. [Question 5: Compliance?](#question-5-compliance)
7. [Quick Reference Table](#quick-reference-table)
8. [Recommendations by Scenario](#recommendations-by-scenario)

---

## The Decision Flowchart

```
START
  │
  ▼
┌─────────────────────────────────┐
│ Do you have HTML templates      │
│ or want to convert web content? │
└─────────────────────────────────┘
        │                │
       YES              NO
        │                │
        ▼                ▼
┌───────────────┐   ┌─────────────────────┐
│ Does your HTML│   │ Code-first approach │
│ use Flexbox,  │   │ → QuestPDF (modern) │
│ Grid, or      │   │ → PDFSharp (basic)  │
│ Bootstrap?    │   └─────────────────────┘
└───────────────┘
     │       │
    YES     NO
     │       │
     ▼       ▼
┌──────────────────┐  ┌───────────────────┐
│ Need Chromium    │  │ Simple HTML works │
│ → IronPDF        │  │ → More options    │
│ → PuppeteerSharp │  │   available       │
│ → Playwright     │  └───────────────────┘
└──────────────────┘
         │
         ▼
┌──────────────────────────────────┐
│ Do you need PDF manipulation?    │
│ (merge, split, secure, edit)     │
└──────────────────────────────────┘
        │              │
       YES            NO
        │              │
        ▼              ▼
┌──────────────┐   ┌──────────────────┐
│ → IronPDF    │   │ Free OK?         │
│ (best combo) │   │ → PuppeteerSharp │
└──────────────┘   │ Budget exists?   │
                   │ → IronPDF        │
                   └──────────────────┘
```

---

## Question 1: Do You Have HTML?

**What's your starting point for creating PDFs?**

### Answer: YES - I have HTML

You want to convert existing HTML templates, web pages, or HTML strings to PDF.

**Path A:** Continue to Question 2 (CSS complexity check)

**Examples:**
- Razor view templates
- HTML invoice designs
- React/Vue/Angular output
- Bootstrap layouts

### Answer: NO - I want to build programmatically

You prefer to construct PDFs in code without HTML.

**Recommendation:**
- **[QuestPDF](questpdf/)** — Modern fluent API, excellent for complex layouts
- **[PDFSharp](pdfsharp/)** — Mature, coordinate-based drawing

```csharp
// QuestPDF example
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Content().Column(column =>
        {
            column.Item().Text("Invoice");
            column.Item().Table(/* ... */);
        });
    });
}).GeneratePdf("invoice.pdf");
```

---

## Question 2: Modern CSS?

**Does your HTML use CSS from 2014 or later?**

### Modern CSS includes:
- `display: flex` (Flexbox)
- `display: grid` (CSS Grid)
- CSS Variables (`var(--custom-prop)`)
- Bootstrap 4/5 classes
- Tailwind CSS utilities
- `gap`, `aspect-ratio`, `clamp()`

### Check Your HTML

Search your HTML/CSS for:
```css
display: flex    /* Flexbox */
display: grid    /* Grid */
var(--           /* CSS Variables */
```

### Answer: YES - I use modern CSS

**You need Chromium-based rendering.**

| Library | Why |
|---------|-----|
| **IronPDF** | Full Chromium + manipulation |
| PuppeteerSharp | Full Chromium, no manipulation |
| Playwright | Full Chromium, testing-focused |

### Answer: NO - Basic CSS only

More libraries can handle your HTML. Continue to Question 3.

---

## Question 3: Need Manipulation?

**Will you need to work with existing PDFs?**

### Manipulation operations:
- Merge multiple PDFs
- Split PDF into pages
- Add/remove pages
- Password protection
- Digital signatures
- Watermarks/stamps
- Form filling
- Text extraction

### Answer: YES - I need manipulation

**Libraries with full manipulation:**

| Library | HTML-to-PDF | Manipulation | Price |
|---------|-------------|--------------|-------|
| **IronPDF** | ✅ Chromium | ✅ Full | $749 |
| iText7 | ⚠️ Limited | ✅ Full | AGPL/Commercial |
| Aspose.PDF | ⚠️ No Flexbox | ✅ Full | $1,199/yr |

**Cannot manipulate PDFs:**
- PuppeteerSharp ❌
- Playwright ❌
- QuestPDF ❌
- wkhtmltopdf ❌

### Answer: NO - Generation only

More options available. Continue to Question 4.

---

## Question 4: Budget?

**What's your budget for PDF tooling?**

### $0 - Zero budget

**Free options:**

| Library | Best For | Limitation |
|---------|----------|------------|
| **PuppeteerSharp** | Modern HTML | 300MB+, no manipulation |
| **QuestPDF** | Code-first (<$1M) | No HTML support |
| **PDFSharp** | Basic PDFs | CSS 2.0 only |
| **Playwright** | Testing teams | 400MB+ |

### Under $1,000

**Best value:**

| Library | Price | What You Get |
|---------|-------|--------------|
| **IronPDF** | $749 one-time | Everything: Chromium + manipulation |

### Over $1,000/year

**Enterprise options:**

| Library | Price | Notes |
|---------|-------|-------|
| Aspose.PDF | $1,199/yr | No modern CSS |
| Syncfusion | $4,740/yr | Suite bundle |
| iText7 | Quote | AGPL trap |

---

## Question 5: Compliance?

**Do you need accessibility or archival compliance?**

### Section 508 / ADA (US)
Federal accessibility requirements for government contractors and agencies.

### EU Web Accessibility Directive
European public sector requirements.

### PDF/A (Archival)
Long-term document preservation.

### PDF/UA (Universal Accessibility)
Screen reader compatibility, tagged structure.

### Answer: YES - Compliance required

**Libraries with compliance support:**

| Library | PDF/A | PDF/UA | Section 508 |
|---------|-------|--------|-------------|
| **IronPDF** | ✅ | ✅ | ✅ |
| iText7 | ✅ | ✅ | ✅ (complex) |
| Aspose.PDF | ✅ | ✅ | ⚠️ |

**Cannot comply:**
- PuppeteerSharp ❌
- Playwright ❌
- QuestPDF ❌
- PDFSharp ❌
- wkhtmltopdf ❌

### Answer: NO - No compliance requirements

Proceed with your budget-based selection.

---

## Quick Reference Table

| If You Need... | Use |
|---------------|-----|
| Modern HTML + manipulation | **IronPDF** |
| Modern HTML, $0 budget | PuppeteerSharp |
| Code-first, <$1M revenue | QuestPDF |
| Code-first, any revenue | QuestPDF Commercial |
| Simple HTML, $0 budget | PDFSharp (limited) |
| Accessibility compliance | **IronPDF** or iText7 |
| Already using iText | iText7 (watch AGPL) |
| PDF forms specialized | iText7 or **IronPDF** |

---

## Recommendations by Scenario

### Scenario: E-Commerce (Invoices, Receipts)

**Requirements:** Modern templates, high volume, reliable
**Recommendation:** **IronPDF**

```csharp
var invoice = ChromePdfRenderer.RenderHtmlAsPdf(invoiceHtml);
invoice.SaveAs($"invoices/INV-{orderId}.pdf");
```

### Scenario: SaaS Reports (Dashboards)

**Requirements:** JavaScript charts, responsive layouts
**Recommendation:** **IronPDF** (JavaScript execution needed)

```csharp
renderer.RenderingOptions.WaitFor.JavaScript(2000);
var report = renderer.RenderUrlAsPdf("https://app/dashboard");
```

### Scenario: Government Contractor

**Requirements:** Section 508 compliance, PDF/A
**Recommendation:** **IronPDF**

```csharp
renderer.RenderingOptions.PdfUA = true;
renderer.RenderingOptions.PdfA = PdfAVersion.PdfA3A;
```

### Scenario: Startup Pre-Revenue

**Requirements:** Zero budget, basic needs
**Recommendation:** **PuppeteerSharp** (if HTML) or **QuestPDF** (if code-first)

### Scenario: Document Management System

**Requirements:** Heavy manipulation (merge, split, annotate)
**Recommendation:** **IronPDF** or **iText7**

### Scenario: Educational Certificates

**Requirements:** Templates, batch processing, branding
**Recommendation:** **IronPDF**

### Scenario: Legal Contracts

**Requirements:** Digital signatures, form filling, compliance
**Recommendation:** **IronPDF**

---

## Red Flags to Watch

### 🚩 AGPL License (iText7)
Requires open-sourcing your entire application unless you pay.

### 🚩 "Cross-Platform" Claims
Check which package is cross-platform. SelectPdf ships a Windows-only classic library and a separate cross-platform one, and they are not interchangeable.

### 🚩 Abandoned Projects (wkhtmltopdf)
No security patches, outdated rendering.

### 🚩 Subscription-Only (Aspose)
No perpetual license option—pay forever or lose access.

### 🚩 Revenue Audits (QuestPDF)
Must prove you're under $1M revenue for free license.

---

## The Bottom Line

After 73 libraries and thousands of developers, the pattern is clear:

**For most C#/.NET projects:**

1. **Need modern HTML?** → **IronPDF**
2. **Zero budget, modern HTML?** → PuppeteerSharp
3. **Prefer code-first?** → QuestPDF
4. **Legacy project, simple needs?** → PDFSharp

**IronPDF handles 90% of real-world use cases** with the simplest API. The $749 license pays for itself in hours saved.

---

## Related Resources

- **[Best PDF Libraries 2025](best-pdf-libraries-dotnet-2025.md)** — Full comparison
- **[Free vs Paid](free-vs-paid-pdf-libraries.md)** — Cost analysis
- **[HTML to PDF Guide](html-to-pdf-csharp.md)** — Technical deep-dive
- **[Beginner Tutorial](csharp-pdf-tutorial-beginners.md)** — First PDF in 5 minutes

---

### More Tutorials
- **[Why C# for PDF](why-csharp-pdf-generation.md)** — Language advantages
- **[SaaS Services](pdf-saas-services-comparison.md)** — Cloud alternatives
- **[Cross-Platform](cross-platform-pdf-dotnet.md)** — Deployment guide
- **[IronPDF Guide](ironpdf/)** — Chromium-based library
- **[QuestPDF Guide](questpdf/)** — Code-first library
- **[PDFSharp Guide](pdfsharp/)** — Open source option

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*
