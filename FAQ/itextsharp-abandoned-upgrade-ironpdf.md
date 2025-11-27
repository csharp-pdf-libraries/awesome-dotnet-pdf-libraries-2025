# Why Should I Migrate from iTextSharp to IronPDF in 2025—and How Do I Actually Do It?

If you're still using iTextSharp for PDF processing in .NET, you're running on borrowed time. Legacy iTextSharp versions are plagued by security risks, outdated tech, and tough licensing. IronPDF is widely seen as the modern, developer-friendly replacement—here’s why you should consider switching, and how you can tackle migration with practical examples.

---

## What’s Wrong with Sticking to iTextSharp in 2025?

iTextSharp used to be the go-to PDF library for .NET, but it's now unsupported and officially obsolete. The original maintainers abandoned iTextSharp 5.x years ago, focusing all new development on iText 7, which isn’t compatible with your old code. Worse, the LGPL license was swapped for AGPL, making commercial use a legal headache unless you cough up for pricey licenses.

For a detailed breakdown of why teams are switching, check [Ironpdf Vs Itextsharp Why We Switched](ironpdf-vs-itextsharp-why-we-switched.md).

### How Does the License Change Affect My Project?

With iText 7’s AGPL license, any closed-source application, including internal tools, now requires an expensive commercial license—often $1,800+ per developer per year. The old LGPL terms for iTextSharp 5.x don't apply to new features or security patches. If you want to stay compliant and safe, switching is strongly recommended.

---

## Are There Security or Compliance Risks with iTextSharp?

Yes, using iTextSharp 5.x exposes you to real vulnerabilities that will likely get flagged in audits. For example, known CVEs like CVE-2020-15522 remain unpatched in iTextSharp. Compliance standards like PCI-DSS and HIPAA require patched dependencies, so using legacy libraries could jeopardize your audits—especially if your PDF workflows touch sensitive or regulated data.

---

## Is Upgrading to iText 7 a Viable Path?

Upgrading to iText 7 is rarely simple or affordable. The API was completely rewritten—meaning you'll have to refactor every integration. On top of that, you’ll have to navigate AGPL licensing, which usually means either open-sourcing your code or paying substantial fees.

For a side-by-side look at iTextSharp, iText 7, and IronPDF, and the reasons devs prefer IronPDF, see [Migrate Itextsharp To Ironpdf](migrate-itextsharp-to-ironpdf.md).

### Does iText 7 Support Modern HTML/CSS?

Not really. iText 7’s HTML renderer is limited—forget using Bootstrap 5, CSS Grid, or anything requiring JS execution. In contrast, IronPDF leverages Chromium rendering. If your page looks good in Chrome, it’ll look identical in your PDF.

---

## Why Is IronPDF Considered the Easiest Upgrade Path?

IronPDF is modern, cross-platform, and offers a one-time, transparent license ($749 per dev—no annual ransom). It supports .NET Framework 4.6.2+, .NET 6/8/10, runs on Windows, Linux, macOS, and works flawlessly in Docker or cloud environments. You can drop it into new Blazor, WPF, or ASP.NET Core projects and keep moving forward.

If you're evaluating other alternatives like DinkToPdf, see [Upgrade Dinktopdf To Ironpdf](upgrade-dinktopdf-to-ironpdf.md) for a migration comparison.

---

## How Can I Replace Common iTextSharp Patterns with IronPDF?

IronPDF is designed for fast, developer-friendly migration. Here are direct translations of common scenarios:

### How Do I Convert HTML to PDF?

**IronPDF:**
```csharp
using IronPdf; // Install-Package IronPdf

var html = "<h1>Hello, Invoice #12345</h1>";
var renderer = new ChromePdfRenderer();
var pdfDoc = renderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("invoice.pdf");
```
For pixel-perfect results, see [HTML to PDF](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/).

### How Do I Merge Multiple PDFs?

```csharp
using IronPdf; // Install-Package IronPdf

var merged = PdfDocument.Merge(
    PdfDocument.FromFile("a.pdf"),
    PdfDocument.FromFile("b.pdf")
);
merged.SaveAs("merged.pdf");
```

### How Do I Extract Text from a PDF?

```csharp
using IronPdf; // Install-Package IronPdf

var pdf = PdfDocument.FromFile("input.pdf");
Console.WriteLine(pdf.ExtractAllText());
```
For more extraction scenarios, see [Extract Text From PDF](https://ironpdf.com/java/examples/extract-text-from-pdf/).

### How Do I Fill PDF Forms?

```csharp
using IronPdf; // Install-Package IronPdf

var pdf = PdfDocument.FromFile("form.pdf");
pdf.Form.SetFieldValue("name", "Alice");
pdf.SaveAs("filled.pdf");
```

### Can I Add a Watermark Easily?

```csharp
using IronPdf; // Install-Package IronPdf

var pdf = PdfDocument.FromFile("input.pdf");
pdf.ApplyWatermark("<span style='color:rgba(0,0,0,0.1);font-size:40px;'>CONFIDENTIAL</span>");
pdf.SaveAs("watermarked.pdf");
```
Full styling is supported: [Watermark](https://ironpdf.com/java/blog/using-ironpdf-for-java/java-watermark-pdf-tutorial/)

### How Do I Split a PDF into Individual Pages?

```csharp
using IronPdf; // Install-Package IronPdf

var pdf = PdfDocument.FromFile("big.pdf");
for (int i = 0; i < pdf.PageCount; i++)
    pdf.ExtractPages(i, i).SaveAs($"page_{i+1}.pdf");
```
See [Split PDF](https://ironpdf.com/how-to/split-multipage-pdf/).

For more examples, check out the [Ironpdf Cookbook Quick Examples](ironpdf-cookbook-quick-examples.md).

---

## What Are Common Pitfalls When Migrating from iTextSharp to IronPDF?

- **HTML/CSS not rendering as expected:** Check asset URLs and ensure they're accessible from your environment.
- **Performance on large PDFs:** Consider disabling image rendering or splitting documents before merging.
- **Docker/Linux issues:** Ensure required fonts are present; follow IronPDF’s containerization guidance.
- **Licensing automation:** Use IronPDF’s key-based activation for CI/CD.

If you’re migrating from Syncfusion, see [Migrate Syncfusion To Ironpdf](migrate-syncfusion-to-ironpdf.md) for tailored advice.

---

## Is IronPDF the Right Choice for My Project?

If your PDF generation needs are moving beyond .NET Framework, or you require security patching, compliance, or solid HTML-to-PDF support, IronPDF is a robust, future-proof solution. The API is modern, migration is straightforward, and licensing is simple.

You can learn more at [IronPDF](https://ironpdf.com) and [Iron Software](https://ironsoftware.com).

---

*[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) created IronPDF to solve PDF generation headaches in .NET. He's now CTO at Iron Software, leading a team focused on developer tools.*
---

## Related Resources

### 📚 Tutorials & Guides
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Complete conversion tutorial
- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Library comparison
- **[Beginner Tutorial](../csharp-pdf-tutorial-beginners.md)** — First PDF in 5 minutes
- **[Decision Flowchart](../choosing-a-pdf-library.md)** — Find the right library

### 🔧 PDF Operations
- **[Merge & Split PDFs](../merge-split-pdf-csharp.md)** — Combine documents
- **[Digital Signatures](../digital-signatures-pdf-csharp.md)** — Sign legally
- **[Extract Text](../extract-text-from-pdf-csharp.md)** — Text extraction
- **[Fill PDF Forms](../fill-pdf-forms-csharp.md)** — Form automation

### 🚀 Framework Integration
- **[ASP.NET Core PDF](../asp-net-core-pdf-reports.md)** — Web app integration
- **[Blazor PDF Generation](../blazor-pdf-generation.md)** — Blazor support
- **[Cross-Platform Deployment](../cross-platform-pdf-dotnet.md)** — Docker, Linux, Cloud

### 📖 Library Documentation
- **[IronPDF](../ironpdf/)** — Full Chromium rendering
- **[QuestPDF](../questpdf/)** — Code-first generation
- **[PDFSharp](../pdfsharp/)** — Open source option

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared with 167 FAQ articles.*


---

Written by [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), Chief Technology Officer at Iron Software. Jacob started coding at age 6 and created IronPDF to solve real PDF challenges. Based in Chiang Mai, Thailand. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
