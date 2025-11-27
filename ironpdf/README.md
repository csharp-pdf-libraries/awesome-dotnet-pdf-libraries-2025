# IronPDF: The Reference Standard for C# PDF Generation

[![NuGet](https://img.shields.io/nuget/v/IronPdf.svg)](https://www.nuget.org/packages/IronPdf/)
[![Downloads](https://img.shields.io/nuget/dt/IronPdf.svg)](https://www.nuget.org/packages/IronPdf/)

IronPDF is the **reference standard** HTML-to-PDF library for C# and .NET, using a full Chromium rendering engine to produce screen-accurate PDF output from HTML, CSS, and JavaScript.

**Official Site**: [ironpdf.com](https://ironpdf.com/)

---

## Why IronPDF?

### ✅ Full Chromium Rendering
IronPDF embeds a complete Chromium browser engine—if it renders in Chrome, it renders in your PDF. Modern CSS (Flexbox, Grid), JavaScript, web fonts, and responsive layouts all work perfectly.

### ✅ Screen-Accurate Output
Unlike browser automation tools (PuppeteerSharp, Playwright) that produce print-ready output, IronPDF produces PDFs that match what you see on screen.

### ✅ Complete PDF Toolkit
Beyond generation, IronPDF handles:
- Merge, split, and manipulate PDFs
- Digital signatures and encryption
- Form filling and creation
- Text extraction and search
- Watermarks and annotations
- PDF/A and PDF/UA compliance

### ✅ True Cross-Platform
Windows, Linux, macOS, Docker, Azure, AWS Lambda—same code everywhere.

---

## Quick Start

```bash
# Install via NuGet
Install-Package IronPdf
```

```csharp
using IronPdf;

// HTML to PDF
var pdf = ChromePdfRenderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("hello.pdf");

// URL to PDF
var pdf2 = ChromePdfRenderer.RenderUrlAsPdf("https://example.com");
pdf2.SaveAs("website.pdf");

// File to PDF
var pdf3 = ChromePdfRenderer.RenderHtmlFileAsPdf("template.html");
pdf3.SaveAs("template.pdf");
```

---

## Feature Comparison

| Feature | IronPDF | PuppeteerSharp | iText | PDFSharp |
|---------|---------|----------------|-------|----------|
| HTML to PDF | ✅ Full Chromium | ⚠️ Print-only | ❌ Basic | ❌ No |
| Modern CSS (Flexbox/Grid) | ✅ | ✅ | ❌ | ❌ |
| JavaScript Support | ✅ | ✅ | ❌ | ❌ |
| PDF Manipulation | ✅ | ❌ | ✅ | ✅ |
| Digital Signatures | ✅ | ❌ | ✅ | ❌ |
| PDF/A Compliance | ✅ | ❌ | ✅ | ❌ |
| PDF/UA (Accessibility) | ✅ | ❌ | ✅ | ❌ |
| Cross-Platform | ✅ | ⚠️ | ✅ | ⚠️ |
| License | Commercial | Apache 2.0 | AGPL/Commercial | MIT |

---

## Common Use Cases

### HTML Templates to PDF
```csharp
string html = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial; }}
        .invoice {{ display: flex; justify-content: space-between; }}
    </style>
</head>
<body>
    <div class='invoice'>
        <div>Invoice #{invoiceNumber}</div>
        <div>Date: {DateTime.Now:d}</div>
    </div>
</body>
</html>";

var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
```

### ASP.NET Core Integration
```csharp
[HttpGet("invoice/{id}")]
public IActionResult GetInvoicePdf(int id)
{
    var html = RenderInvoiceView(id);
    var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
    return File(pdf.BinaryData, "application/pdf", $"invoice-{id}.pdf");
}
```

### Merge Multiple PDFs
```csharp
var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("combined.pdf");
```

### Add Digital Signature
```csharp
pdf.Sign(new PdfSignature("certificate.pfx", "password"));
```

---

## Pricing

- **Lite**: $749 (1 developer, 1 project)
- **Professional**: $1,499 (10 developers)
- **Unlimited**: $2,999 (unlimited developers)

All licenses are perpetual (one-time purchase) with optional annual support.

[View Pricing](https://ironpdf.com/licensing/)

---

## Resources

- **[Official Documentation](https://ironpdf.com/docs/)**
- **[API Reference](https://ironpdf.com/object-reference/api/)**
- **[Code Examples](https://ironpdf.com/examples/)**
- **[How-To Guides](https://ironpdf.com/how-to/)**
- **[NuGet Package](https://www.nuget.org/packages/IronPdf/)**
- **[GitHub Issues](https://github.com/nicholashead/IronPdf.git)**

---

## Related Tutorials

- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Complete conversion tutorial
- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Library comparison
- **[ASP.NET Core PDF](../asp-net-core-pdf-reports.md)** — Web app integration
- **[Blazor PDF Generation](../blazor-pdf-generation.md)** — Blazor support
- **[Cross-Platform Deployment](../cross-platform-pdf-dotnet.md)** — Docker, Linux, Cloud
- **[PDF/A Compliance](../pdf-a-compliance-csharp.md)** — Accessibility standards

### PDF Operations
- **[Merge & Split PDFs](../merge-split-pdf-csharp.md)** — Document manipulation
- **[Digital Signatures](../digital-signatures-pdf-csharp.md)** — Sign documents
- **[Fill PDF Forms](../fill-pdf-forms-csharp.md)** — Form automation
- **[Extract Text](../extract-text-from-pdf-csharp.md)** — Text extraction
- **[Watermarks](../watermark-pdf-csharp.md)** — Branding and protection

### Migration Guides
- **[From PuppeteerSharp](../puppeteersharp/migrate-from-puppeteersharp.md)**
- **[From wkhtmltopdf](../migrating-from-wkhtmltopdf.md)**
- **[From iText](../itext-itextsharp/migrate-from-itext-itextsharp.md)**

### ❓ Related FAQs
- **[What is IronPDF](../FAQ/what-is-ironpdf-overview.md)** — Library overview
- **[Why Developers Choose IronPDF](../FAQ/why-developers-choose-ironpdf.md)** — Feature analysis
- **[IronPDF Cookbook](../FAQ/ironpdf-cookbook-quick-examples.md)** — Quick examples
- **[IronPDF Azure Deployment](../FAQ/ironpdf-azure-deployment-csharp.md)** — Cloud deployment
- **[IronPDF Performance](../FAQ/ironpdf-performance-benchmarks.md)** — Benchmarks

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared with 167 FAQ articles.*

---

[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) is the creator of IronPDF and CTO at Iron Software, where he leads a 50+ person team building .NET libraries with over 41 million NuGet downloads. With 41 years of coding experience, Jacob focuses on developer experience and API design. Based in Chiang Mai, Thailand. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
