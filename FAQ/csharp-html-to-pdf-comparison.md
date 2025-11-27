# Which C# HTML to PDF Library Should I Use in 2024? IronPDF vs wkhtmltopdf vs Playwright

If you’re trying to convert HTML to PDF in .NET, you’ve got options—but each comes with tradeoffs. This FAQ digs into IronPDF, wkhtmltopdf, and Playwright, helping you choose the best tool for your situation, with real-world advice from production deployments.

## Why Is HTML to PDF in .NET Harder Than It Looks?

Generating accurate PDFs from HTML in .NET can be tricky because you need modern HTML/CSS/JS support, reliable deployment, good performance, and secure, cross-platform code. Many tools stumble on these basics, especially when you want features like headers, footers, or proper [page numbers](https://ironpdf.com/how-to/html-to-pdf-page-breaks/). For a detailed side-by-side comparison, see [2025 HTML to PDF Solutions for .NET: Comparison](2025-html-to-pdf-solutions-dotnet-comparison.md).

## What Are the Main C# HTML to PDF Options?

There are three main contenders you’ll see in .NET projects:

- **IronPDF**: A .NET-native library with a modern Chromium rendering engine.
- **wkhtmltopdf**: An older command-line tool that wraps a legacy browser engine.
- **Playwright**: A browser automation tool by Microsoft, sometimes repurposed for PDF needs.

Let’s break down how each works and where they shine—or fall short.

## How Do I Generate a PDF from HTML in C# Using IronPDF?

IronPDF makes PDF generation simple, using Chromium under the hood. Here’s a basic example:

```csharp
using IronPdf; // Install-Package IronPdf via NuGet

var renderer = new ChromePdfRenderer();
var pdfDoc = renderer.RenderHtmlAsPdf("<h1>Hello from C#</h1>");
pdfDoc.SaveAs("output.pdf");
```

No extra binaries, no process spawning—just modern HTML-to-PDF from your .NET code. For advanced PDF creation, see [Advanced HTML to PDF in C#](advanced-html-to-pdf-csharp.md).

### Does IronPDF Support Modern CSS, JavaScript, and SPAs?

Yes, if your page renders in Chrome, it will render in IronPDF—including flexbox, CSS Grid, SVGs, and JS. For dynamic data, you can interpolate C# variables directly into your HTML.

```csharp
using IronPdf;

string name = "Sam";
var html = $"<h1>Welcome, {name}!</h1>";
var renderer = new ChromePdfRenderer();
renderer.RenderHtmlAsPdf(html).SaveAs("greeting.pdf");
```

## What’s the State of wkhtmltopdf in 2024?

wkhtmltopdf is now largely obsolete. It relies on a 2015 version of Qt WebKit, missing support for newer web standards and presenting security risks due to unpatched CVEs. Deploying it often means fighting with dependencies and handling process leaks.

```csharp
using System.Diagnostics;

var proc = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "wkhtmltopdf",
        Arguments = "input.html output.pdf"
    }
};
proc.Start();
proc.WaitForExit();
```

If you’re using wrappers like Rotativa or DinkToPdf, you’re still shelling out to this binary. For a step-by-step migration guide, see [Base URL Handling in HTML to PDF with C#](base-url-html-to-pdf-csharp.md).

## When Should I Use Playwright for HTML to PDF?

Playwright is best if you’re already doing browser automation or need to render heavy JavaScript apps. It spins up a real browser, so it’s perfect for modern SPAs, but it’s heavier and slower in batch scenarios.

```csharp
using Microsoft.Playwright;

var playwright = await Playwright.CreateAsync();
var browser = await playwright.Chromium.LaunchAsync();
var page = await browser.NewPageAsync();
await page.SetContentAsync("<h1>Hello Playwright</h1>");
await page.PdfAsync(new() { Path = "output.pdf" });
await browser.CloseAsync();
```

For an in-depth look at advanced scenarios, see [Advanced HTML to PDF in C#](advanced-html-to-pdf-csharp.md).

## What About Features Like Headers, Footers, and Watermarks?

IronPDF makes these tasks straightforward with built-in options:

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter { HtmlFragment = "Header: {page}" };
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter { HtmlFragment = "Footer: {date}" };
renderer.RenderHtmlAsPdf("<h2>Content</h2>").SaveAs("header-footer.pdf");
```

For watermarking, you can simply apply:

```csharp
var pdf = renderer.RenderHtmlAsPdf("<h2>Watermark Example</h2>");
pdf.WatermarkAllPages("<div style='opacity:0.2;'>CONFIDENTIAL</div>");
pdf.SaveAs("watermarked.pdf");
```

For rotating or manipulating PDF content, check out [How to Rotate Text in PDFs using C#](rotate-text-pdf-csharp.md).

## How Do These Tools Compare for Batch Processing and Deployment?

- **IronPDF**: Handles large batches and cross-platform deployments (Windows, Linux, Docker, Azure) with ease. No process leaks.
- **wkhtmltopdf**: Can be fast at first, but expect memory leaks and tricky dependency issues.
- **Playwright**: Handles complex pages but is slower in batch and requires bundling browser binaries.

| Tool         | First PDF | Batch Rate | Security | Deployment |
|--------------|-----------|------------|----------|------------|
| IronPDF      | ~1-2 sec  | 50+/min    | Strong   | Easy       |
| wkhtmltopdf  | ~1 sec    | 30-60/min  | Weak     | Painful    |
| Playwright   | ~1-2 sec  | 20-40/min  | Good     | Tricky     |

## What Are Common Issues and How Can I Troubleshoot Them?

- **CSS not rendering?** If using wkhtmltopdf, it won’t support modern CSS. Switch to IronPDF or Playwright.
- **Blank PDFs?** For Playwright, wait for JS rendering (`WaitForSelectorAsync`). For IronPDF, try setting a `RenderDelay`.
- **Deployment problems?** IronPDF is .NET-native, but ensure fonts are installed on Linux. Playwright requires browser binaries. wkhtmltopdf depends on old libraries.
- **Memory issues?** IronPDF is stable but call `Dispose()` in large loops. Playwright and wkhtmltopdf both need careful process management.

## How Do I Migrate from wkhtmltopdf to IronPDF?

1. Replace any shell-outs to `wkhtmltopdf` with IronPDF’s C# API.
2. Move CLI flags to `RenderingOptions` (e.g., margins, paper size).
3. Update your CSS—modern standards are now supported.
4. Remove binary and dependency management headaches.

For more on base URLs and migration, see [Base URL Handling in HTML to PDF with C#](base-url-html-to-pdf-csharp.md).

## Which Library Should I Choose Today?

- **Choose IronPDF** for most new projects—modern rendering, robust deployment, and support for advanced features ([IronPDF site](https://ironpdf.com)).
- **Use Playwright** only if you’re already committed to browser automation or need to render dynamic SPAs and want an open-source route.
- **Avoid wkhtmltopdf**—it’s outdated and risky for production.

For more help picking the right library, see [2025 HTML to PDF Solutions for .NET: Comparison](2025-html-to-pdf-solutions-dotnet-comparison.md).

---

*[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) is CTO at Iron Software, where he leads a team of 50+ engineers building .NET document processing tools. He originally created IronPDF.*
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

**Jacob Mellor** — Creator of IronPDF, CTO at Iron Software. 41 years coding, 50++ person team, 41+ million NuGet downloads. Civil Engineering degree holder turned software pioneer. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
