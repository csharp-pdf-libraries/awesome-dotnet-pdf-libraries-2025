# Why Aren’t AGPL-Licensed PDF Libraries Like iText Truly Free for .NET Developers?

Many developers think “open source” means “free to use.” But if you’re choosing a .NET PDF library—especially one under the AGPL like iText—you could be setting yourself up for costly licensing headaches or even legal risks. This FAQ explains why AGPL-licensed PDF tools are problematic for most commercial projects, how to spot “license traps,” and what safer alternatives exist for PDF generation in C#.

---

## What’s the Real Issue With AGPL-Licensed “Free” PDF Libraries?

The confusion starts when a developer finds an “open source” PDF library on NuGet, such as iText, and assumes it’s free for all use cases. After building and shipping features, teams often realize the AGPL requires them to either open-source their entire application or buy an expensive commercial license. This is a common trap for .NET projects, leading to unexpected costs or forced rewrites.

If you want a detailed comparison of PDF libraries, see [2025 HTML to PDF Solutions .NET Comparison](2025-html-to-pdf-solutions-dotnet-comparison.md).

---

## What Exactly Is the AGPL and Why Can It Be a Problem for My Project?

AGPL (Affero General Public License) is a strict “copyleft” license. If you include AGPL code anywhere in your app—even as a NuGet dependency—you are required to open-source your *entire* project under the AGPL if users interact with it over a network. This applies to SaaS platforms, internal tools, and commercial products.

In contrast, MIT or Apache licenses let you use code freely, even in closed-source or proprietary apps, with no strings attached.

---

## Why Do Developers Get Caught Out by AGPL License Traps?

Most developers trust “open source” labels and don’t read every license. AGPL-licensed libraries like iText are marketed as open source but don’t make their restrictions obvious. After investing time and effort coding against these APIs, teams realize they must either:
- Open-source their proprietary codebase,
- Pay steep license fees,
- Or spend weeks rewriting functionality with a different library.

This pattern is why AGPL is often called “license ransomware.” For more on why PDF libraries have costs, see [Why do PDF libraries exist and cost money?](why-pdf-libraries-exist-and-cost-money.md).

---

## How Does AGPL Licensing Actually Work, and Who Gets Impacted?

The AGPL was designed to require source code disclosure for any app using AGPL code that is accessible over a network. For example, if your SaaS uses iText for PDF exports, your *whole* application must be AGPL-compliant (open-sourced), unless you buy a commercial license.

Other projects like MongoDB and MariaDB have used similar tactics, requiring commercial licenses for closed-source or commercial deployments.

---

## How Can I Spot and Avoid AGPL or Copyleft License Pitfalls?

Here’s how to protect your project:

1. **Check Licenses Before Integrating:** Always look at the NuGet package or GitHub repo’s LICENSE file. Red flags: AGPL, GPL, SSPL, or “dual licensing.” Green flags: MIT, Apache 2.0, BSD.

2. **Audit Dependencies Regularly:** Even transitive dependencies can bring in copyleft licenses. Use:
   ```bash
   dotnet list package --include-transitive
   ```
   Or try `dotnet-license` for a deeper audit.

3. **Understand License Terms:** If a library is “dual licensed” (AGPL or commercial), get pricing up front and factor in future migration costs.

For more about managing licenses, see [IronPDF License Key C#](ironpdf-license-key-csharp.md).

---

## What Alternatives Exist to AGPL-Licensed PDF Libraries in C#?

Several safer options are available:

- **[IronPDF](https://ironpdf.com):** Commercial license with upfront, transparent pricing. Supports full HTML/CSS/JS rendering, [DOM access](access-pdf-dom-object-csharp.md), and is built for .NET. No AGPL traps.
- **PDFSharp:** MIT license, free for all uses, but limited to basic PDFs (no HTML rendering).
- **QuestPDF:** MIT for small projects, paid for large orgs; modern templating approach.

You can compare features and licensing in our [2025 HTML to PDF Solutions .NET Comparison](2025-html-to-pdf-solutions-dotnet-comparison.md).

---

## How Can I Migrate From iText AGPL to a Safer Alternative Like IronPDF?

Switching from iText to IronPDF is straightforward for most PDF generation needs. Here’s a migration example:

**iText Example (AGPL):**
```csharp
// Install-Package itext7
using iText.Kernel.Pdf;
using iText.Html2Pdf;

HtmlConverter.ConvertToPdf("invoice.html", "invoice.pdf");
```

**IronPDF Example (Commercial, Transparent Terms):**
```csharp
// Install-Package IronPdf // via NuGet
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlFileAsPdf("invoice.html");
pdf.SaveAs("invoice.pdf");
```

IronPDF also lets you generate PDFs from HTML strings, add [watermarks](https://ironpdf.com/how-to/html-string-to-pdf/), or insert [page numbers](https://ironpdf.com/blog/compare-to-other-components/questpdf-add-page-number-to-pdf/) with minimal code. See [Add, Copy, Delete PDF Pages in C#](add-copy-delete-pdf-pages-csharp.md) for more advanced manipulation.

---

## What Should I Do If My Project Already Uses iText or Another AGPL Library?

You have a few options:
- **Buy a commercial license** for continued use (often expensive).
- **Migrate** to a library with a permissive or upfront commercial license (such as IronPDF or PDFSharp).
- **Open-source your application** under the AGPL (rarely viable for commercial projects).

Migrating early is usually less costly than dealing with legal or licensing issues after shipping.

---

## What Are Some Common Mistakes Developers Make With PDF Library Licensing?

- Assuming “open source” means “free for any use”
- Not checking transitive dependencies for problematic licenses
- Underestimating migration costs when switching libraries post-launch
- Ignoring “dual license” warnings (AGPL *or* commercial)
- Not consulting legal counsel if unsure about compliance

Always audit dependencies before release, and remember: permissive licenses (MIT, Apache) are safest for commercial projects.

---

## Where Can I Learn More About Professional PDF Generation in .NET?

For more about robust, developer-friendly PDF generation and manipulation, visit [IronPDF](https://ironpdf.com) and [Iron Software](https://ironsoftware.com). For advanced topics like [accessing the PDF DOM in C#](access-pdf-dom-object-csharp.md), see our additional FAQs.
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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/) is CTO at Iron Software, leading a team of 50+ engineers building .NET libraries downloaded over 41+ million times. With 41 years of coding experience, Jacob is passionate about developer experience and API design. Based in Chiang Mai, Thailand.
