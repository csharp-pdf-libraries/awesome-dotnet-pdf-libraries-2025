# Why Should Developers Replace iTextSharp with IronPDF for .NET PDF Generation?

If you’re wondering whether to migrate your .NET projects from iTextSharp to IronPDF, you’re not alone. Many developers face this decision as iTextSharp ages and new requirements for HTML-to-PDF, modern features, and licensing emerge. Below is a practical FAQ for .NET developers considering this move—explaining why, how, and what to expect, with plenty of real code and lessons learned.

## Why Are So Many .NET Applications Still Using iTextSharp?

Many .NET projects rely on iTextSharp, the C# port of Java’s iText, because it was once the default choice for PDF manipulation. However, iTextSharp 5.x hasn’t received updates since 2016—no bug fixes, no new features, and, crucially, no security patches. Despite this, numerous legacy applications still use it because migrating a PDF solution is never “urgent” until it breaks or a compliance officer calls.

So why is this a problem?  
- **Security:** No more patches means vulnerabilities linger.
- **HTML/CSS Limitations:** iTextSharp’s HTML renderer is stuck in the past—modern CSS, JS, and fonts just won’t work.
- **Licensing Risks:** The AGPL license can force you to open-source your code or pay a steep fee.

If you’re stuck on iTextSharp, you’re not alone—but there are better, modern alternatives. For more, see [Is iTextSharp abandoned? Should I upgrade to IronPDF?](itextsharp-abandoned-upgrade-ironpdf.md).

## What Is IronPDF and How Does It Differ from iTextSharp?

IronPDF is a .NET PDF library built on a Chromium engine, which means it renders PDFs just like Google Chrome would render a webpage. This stands in stark contrast to iTextSharp, which attempts to mimic HTML/CSS rendering but falls short with anything built after the year 2000.

Why does this matter?
- **HTML5/CSS3 Support:** IronPDF supports the latest web standards—Bootstrap, Tailwind, Google Fonts, Flexbox, Grid, JavaScript, and more.
- **Modern .NET Compatibility:** Works seamlessly with .NET Core, .NET 5–8+, Linux, Docker, and Azure.
- **Simplicity:** Drop in your HTML and get a visually accurate PDF.
- **Ongoing Development:** IronPDF is actively updated, with a clear, developer-friendly commercial license.

Want to see how easy HTML-to-PDF conversion is? Here’s a minimal example:

```csharp
using IronPdf; // Install via NuGet: Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello, IronPDF!</h1>");
pdf.SaveAs("greeting.pdf");
```

For more HTML-to-PDF scenarios, visit the [HTML to PDF guide](https://ironpdf.com/how-to/html-string-to-pdf/).

## What Were the Main Reasons Developers Migrate from iTextSharp to IronPDF?

### Is AGPL Licensing a Real Problem with iTextSharp?

Absolutely. Many teams discover late that iTextSharp’s AGPL license isn’t “free” for SaaS or closed-source projects. If you don’t open-source your code, you’re required to pay for a commercial license—often over $1,800 per developer per year. Compliance audits can trigger expensive surprises.

IronPDF, by contrast, offers upfront commercial licensing ($749 per developer, perpetual), no surprise audits, and clear terms. For most teams, this means massive cost savings and peace of mind.

For more, see [How do I migrate from iTextSharp to IronPDF?](migrate-itextsharp-to-ironpdf.md).

### Are Security Vulnerabilities a Risk with iTextSharp?

Yes, and it’s only getting worse. iTextSharp 5.x is abandonware. For example, the CVE-2020-15522 XXE vulnerability was never patched in iTextSharp—it was only fixed in iText 7, which is a separate, non-drop-in library.

IronPDF is actively maintained, with monthly security updates and rapid responses to new issues. If your projects need PCI-DSS, SOC2, or HIPAA compliance, you can’t afford to run obsolete software.

### Does IronPDF Really Solve HTML Rendering Issues?

In practice, yes. iTextSharp’s HTML/CSS rendering struggles with anything beyond basic HTML—Bootstrap layouts break, flexbox and grid are ignored, and even Google Fonts require manual embedding. Our team struggled for weeks to get modern invoices looking right with iTextSharp, only to have IronPDF render them perfectly on the first try.

If you rely on contemporary web tech for your PDFs, IronPDF is a game changer. To see why other developers switched, read [Why do developers choose IronPDF?](why-developers-choose-ironpdf.md).

## How Do Features Compare Between iTextSharp and IronPDF?

Here’s a quick feature comparison for common PDF tasks:

| Feature                  | iTextSharp 5.x      | IronPDF                        |
|--------------------------|---------------------|--------------------------------|
| HTML5/CSS3 Support       | No (HTML 3.2 only)  | Yes (Chromium engine)          |
| Flexbox/Grid             | No                  | Yes                            |
| JavaScript               | No                  | Yes                            |
| Bootstrap Compatibility  | Poor                | Excellent                      |
| Google Fonts             | Manual embedding    | Automatic loading              |
| .NET Core/5–8+           | No                  | Full support                   |
| Linux/Docker             | Mono only           | Native/Container support       |
| Security Patches         | None                | Monthly                        |
| Licensing                | AGPL (expensive)    | Perpetual commercial           |
| API Simplicity           | Verbose             | Modern and simple              |

If you want a summary of why developers are making the jump, check [Why do developers choose IronPDF?](why-developers-choose-ironpdf.md).

## How Do Common PDF Tasks Differ in IronPDF and iTextSharp?

### How Do You Convert HTML to PDF?

#### With iTextSharp:
```csharp
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

var html = "<h1>Invoice</h1><p>Total: $500</p>";
var htmlStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));
using var pdfFile = new FileStream("invoice.pdf", FileMode.Create);
var doc = new Document();
var writer = PdfWriter.GetInstance(doc, pdfFile);
doc.Open();
XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, htmlStream, null);
doc.Close();
```
Limited CSS, no JS, and fonts often missing.

#### With IronPDF:
```csharp
using IronPdf; // NuGet

var html = "<h1>Invoice</h1><p>Total: $500</p>";
var renderer = new ChromePdfRenderer();
var pdfDoc = renderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("invoice.pdf");
```
Looks just like your web page—Bootstrap, fonts, and all.

For more Razor-based scenarios, see [How do I generate PDFs from Razor Views in C#?](razor-view-to-pdf-csharp.md).

### Can IronPDF Handle Bootstrap, Flexbox, and Google Fonts?

Yes, IronPDF shines here. Just use your web HTML as-is:

```csharp
using IronPdf;

var html = @"
<link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
<div class='container'>
  <div class='row'>
    <div class='col'>Invoice #12345</div>
    <div class='col'>Date: Feb 2, 2025</div>
    <div class='col'>Total: $1,500</div>
  </div>
</div>";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("styled-invoice.pdf");
```

You get true Bootstrap columns, Google Fonts, and all modern CSS.

### How Do You Add Headers, Footers, or Page Numbers?

With iTextSharp, you subclass event handlers and manually draw text. In IronPDF, just set HTML for headers/footers:

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
};
var pdf = renderer.RenderHtmlAsPdf("<h1>Document</h1>");
pdf.SaveAs("with-footer.pdf");
```

For advanced features, see [How do I add bookmarks to PDFs in C#?](pdf-bookmarks-csharp.md) and [Page Numbers](https://ironpdf.com/blog/compare-to-other-components/questpdf-add-page-number-to-pdf/).

### How Do You Merge or Split PDFs?

IronPDF makes PDF merging trivial:
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("a.pdf");
var pdf2 = PdfDocument.FromFile("b.pdf");
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```
No more manual page handling.

### Can You Extract Text from PDFs Easily?

Yes—one line with IronPDF:
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("sample.pdf");
var text = pdf.ExtractAllText();
Console.WriteLine(text);
```

### Does IronPDF Support Dynamic Templates Like Razor?

Absolutely. You can render Razor views directly to PDF, which is a lifesaver for ASP.NET MVC/Core apps. For a walk-through, see [How do I convert a Razor View to PDF in C#?](razor-view-to-pdf-csharp.md).

## How Difficult Is It to Migrate from iTextSharp to IronPDF?

Most teams overestimate migration pain. Our experience:
- **Proof of concept:** A few hours—most existing HTML “just worked.”
- **Full replacement:** 4–5 days for all PDF code, with much old code deleted.
- **Testing:** A week of QA, but few issues.
- **Total:** Under a month, part-time, for a single developer.

For a step-by-step migration plan, see [How do I migrate from iTextSharp to IronPDF?](migrate-itextsharp-to-ironpdf.md).

## What Are the Costs? How Does IronPDF Compare to iText 7?

| Year      | iText 7 (6 devs) | IronPDF (6 devs) |
|-----------|------------------|------------------|
| Year 1    | $10,800          | $4,494           |
| Year 2+   | $10,800/year     | $0               |
| 3 Years   | $32,400          | $4,494           |

IronPDF’s perpetual licensing model means massive savings and no annual renewal headaches.

## What Pitfalls or Gotchas Should I Watch Out For When Migrating?

### Is Chromium Startup Delay a Problem?

The first PDF generation with IronPDF is slower as Chromium starts up. For high-throughput apps, “warm up” IronPDF at startup:

```csharp
Task.Run(() => {
    var renderer = new ChromePdfRenderer();
    renderer.RenderHtmlAsPdf("<span>Warm up</span>");
});
```

### Will Memory Usage Increase?

IronPDF loads PDFs into memory. For typical documents, this isn’t a problem, but test with large files (1000+ pages).

### Are There Issues Running on Linux or Docker?

You may need to install additional fonts and libraries in your Docker container. Here’s a sample Dockerfile line:

```dockerfile
RUN apt-get update && apt-get install -y fonts-dejavu-core libnss3 libatk1.0-0 libatk-bridge2.0-0 libcups2 libxcomposite1 libxdamage1 libxrandr2 libgbm1 libpango-1.0-0 libpangocairo-1.0-0 libasound2 libxss1 libxtst6
```

### What About Licensing and Watermarks?

You must activate your IronPDF license in production:
```csharp
IronPdf.License.LicenseKey = "YOUR-KEY-HERE";
```
Otherwise, you’ll see [watermarks](https://ironpdf.com/java/how-to/java-create-pdf-tutorial/).

### Can I Migrate Custom Event Handlers (e.g., Watermarks, Headers)?

Yes, but you’ll use IronPDF’s HTML header/footer system. It’s usually easier and more flexible:

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='position:absolute; opacity:0.3; font-size:60px; color:red; transform:rotate(-30deg); left:50%; top:50%; z-index:10;'>CONFIDENTIAL</div>"
};
```

## When Is It Okay to Stick with iTextSharp?

If you only do basic PDF merging or extraction, run on .NET Framework 4.x, and don’t care about modern HTML or compliance, you can delay migration. But if you need HTML-to-PDF, security, or modern compatibility, now is the time to move.

For more perspective, check [Is iTextSharp abandoned? Should I upgrade to IronPDF?](itextsharp-abandoned-upgrade-ironpdf.md).

## Where Can I Learn More or Get Help?

- [IronPDF Documentation](https://ironpdf.com)
- [Iron Software Tools](https://ironsoftware.com)
- [How do I migrate from iTextSharp to IronPDF?](migrate-itextsharp-to-ironpdf.md)
- [Why do developers choose IronPDF?](why-developers-choose-ironpdf.md)
- [How do I add bookmarks to PDFs in C#?](pdf-bookmarks-csharp.md)
- [How do I convert a Razor View to PDF in C#?](razor-view-to-pdf-csharp.md)

---

*Have more questions? Find [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) on the Iron Software team page. As CTO, Jacob leads development of IronPDF and the Iron Suite.*
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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/) serves as CTO at [Iron Software](https://ironsoftware.com/about-us/authors/jacobmellor/), creator of IronPDF and leader of a globally distributed engineering team. Constantly explores how AI tools can push software development forward. With 25+ years of commercial development experience, he continues to push the boundaries of .NET PDF technology from Chiang Mai, Thailand.
