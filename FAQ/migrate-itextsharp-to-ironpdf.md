# How Do I Migrate from iTextSharp to IronPDF in .NET?

Migrating from iTextSharp to IronPDF can make your .NET PDF workflows much simpler, modernize your codebase, and remove licensing headaches. This FAQ covers why you’d switch, key differences, migration strategies, and tips to make the transition smooth. If you’ve considered moving from other PDF tools, see our guides on [migrating from wkhtmltopdf](migrate-wkhtmltopdf-to-ironpdf.md), [Telerik](migrate-telerik-to-ironpdf.md), or [Syncfusion](migrate-syncfusion-to-ironpdf.md) as well.

---

## Why Should I Move from iTextSharp to IronPDF?

Most developers switch because iTextSharp's AGPL license can force you to open-source your entire project unless you pay for a pricey commercial license. IronPDF is commercially licensed, affordable, and lets you keep your code closed-source. On top of that, IronPDF’s [Chromium-based HTML-to-PDF](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/) support delivers modern CSS and JavaScript rendering—something iTextSharp simply can’t match.

You also get a much more concise API, fewer dependencies, and built-in features like watermarking (see [watermark example](https://ironpdf.com/java/blog/using-ironpdf-for-java/java-watermark-pdf-tutorial/)), digital signatures, and form filling without jumping through hoops.

---

## What Are the Key Differences Between iTextSharp and IronPDF?

IronPDF focuses on high-level, practical tasks and ease of use:

| Feature                     | iTextSharp                    | IronPDF                                       |
|-----------------------------|-------------------------------|-----------------------------------------------|
| HTML to PDF                 | Outdated, add-on required     | Chromium-based, pixel-perfect                 |
| Modern CSS/JS Support       | ❌                            | ✅                                            |
| API Complexity              | Verbose, low-level            | Simple, high-level                            |
| Licensing                   | AGPL or $$$                   | Commercial, affordable                        |
| Advanced PDF Hacking        | Powerful                      | Limited (for most business needs, not needed) |
| Text Extraction             | Verbose                       | One-liner                                     |
| Printing                    | Needs 3rd party tools         | Built-in                                      |

For more practical examples, check the [IronPDF Cookbook](ironpdf-cookbook-quick-examples.md).

---

## What’s the Best Approach to Migrate My Code?

### How Do I Audit My iTextSharp Usage?

Start by listing how you're using iTextSharp: HTML-to-PDF, text extraction, form filling, merging, etc. If most of your work is HTML-to-PDF, migration will be fast—otherwise, identify any advanced, low-level manipulations you might need to handle differently.

### How Do I Migrate HTML-to-PDF Logic?

Replace your iTextSharp HTML rendering (which needs paid add-ons for decent results) with IronPDF’s `ChromePdfRenderer`. For example:

```csharp
using IronPdf; // Install-Package IronPdf via NuGet

var html = "<h1>Invoice</h1><p>Total: $500</p>";
var renderer = new ChromePdfRenderer();
var pdfDoc = renderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("invoice.pdf");
```
IronPDF supports modern CSS, JavaScript, and complex layouts natively.

If you’re migrating from other engines, see our guides for [wkhtmltopdf](migrate-wkhtmltopdf-to-ironpdf.md), [Telerik](migrate-telerik-to-ironpdf.md), or [Syncfusion](migrate-syncfusion-to-ironpdf.md).

### How Do I Extract Text from PDFs More Easily?

Instead of looping over pages with listeners, use IronPDF’s one-liners:

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
var text = pdf.ExtractAllText();
Console.WriteLine(text);
```
For extracting from just a single page:

```csharp
var singlePageText = pdf.ExtractTextFromPage(0); // Page index starts at 0
```
Learn more in [how to extract images from PDFs](extract-images-from-pdf-csharp.md)—the same approach applies for images.

### How Do I Fill PDF Forms with IronPDF?

Form filling is much simpler:

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("form.pdf");
pdf.Form.SetFieldValue("name", "Alice");
pdf.Form.SetFieldValue("email", "alice@example.com");
pdf.SaveAs("filled.pdf");
```
Flattening (making fields non-editable) is also easy:

```csharp
pdf.Form.Flatten();
pdf.SaveAs("filled_flat.pdf");
```

### How Can I Merge or Combine Multiple PDFs?

IronPDF lets you merge PDFs in a single method call:

```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("doc1.pdf");
var pdf2 = PdfDocument.FromFile("doc2.pdf");
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```
For merging a list:

```csharp
using System.Collections.Generic;
using IronPdf;

var files = new List<string> { "a.pdf", "b.pdf", "c.pdf" };
var pdfs = files.ConvertAll(PdfDocument.FromFile);
var mergedDoc = PdfDocument.Merge(pdfs.ToArray());
mergedDoc.SaveAs("all_combined.pdf");
```

### How Do I Add Digital Signatures?

IronPDF streamlines digital signing:

```csharp
using IronPdf;
using IronPdf.Signing;

var pdf = PdfDocument.FromFile("doc.pdf");
var signature = new PdfSignature("cert.pfx", "password")
{
    SigningContact = "legal@company.com",
    SigningReason = "Approval"
};
pdf.Sign(signature);
pdf.SaveAs("signed.pdf");
```
Batch signing is just as straightforward.

### How Do I Add Watermarks?

Adding a watermark is a single call using HTML:

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("doc.pdf");
pdf.ApplyWatermark("<h2 style='color:rgba(200,0,0,0.2);'>CONFIDENTIAL</h2>", rotation: 30);
pdf.SaveAs("watermarked.pdf");
```

---

## What If I Need Advanced, Low-Level PDF Features?

IronPDF is designed for most business needs—HTML-to-PDF, merging, forms, signatures, etc. If you need very deep PDF manipulation (custom objects, raw byte tweaks, OpenType embedding), you may need to keep iTextSharp for those edge cases. A hybrid approach is possible: use IronPDF for 90% of tasks, and iTextSharp only where necessary.

For more on advanced scenarios, see our [IronPDF Cookbook](ironpdf-cookbook-quick-examples.md).

---

## What Should I Watch Out for When Migrating?

- **Layout Differences:** IronPDF uses HTML/CSS for layout; if you relied on low-level PDF positioning before, you may need to refactor templates.
- **Missing Features:** Test if you need obscure PDF features (like custom layers).
- **Deployment:** IronPDF requires Chromium; make sure your server environment supports it (especially on Linux/Docker).
- **Learning Curve:** The API is much simpler, but plan for a brief adjustment period.

---

## How Does IronPDF Licensing Compare to iTextSharp?

IronPDF starts at $749/developer (see [IronPDF pricing](https://ironpdf.com)), and lets you keep your code closed-source. iTextSharp’s AGPL requires you to open-source your app unless you buy a commercial license (starting at $1,800 per developer). IronPDF’s licensing is more straightforward and affordable for most teams.

---

## How Can I Get Top Performance with IronPDF?

The first PDF render spins up Chromium and takes a couple of seconds, but subsequent renders are much faster. For batch jobs, reuse the `ChromePdfRenderer` instance:

```csharp
using IronPdf;

public class PdfService
{
    private static readonly ChromePdfRenderer Renderer = new ChromePdfRenderer();

    public PdfDocument GenerateFromHtml(string html)
    {
        return Renderer.RenderHtmlAsPdf(html);
    }
}
```

---

## Where Can I Learn More or Get Help?

Check out the [IronPDF documentation](https://ironpdf.com), our [IronPDF Cookbook](ironpdf-cookbook-quick-examples.md), or reach out for support at [Iron Software](https://ironsoftware.com). For migration guides from other libraries, see [migrate wkhtmltopdf to IronPDF](migrate-wkhtmltopdf-to-ironpdf.md), [migrate Telerik to IronPDF](migrate-telerik-to-ironpdf.md), and [migrate Syncfusion to IronPDF](migrate-syncfusion-to-ironpdf.md).

---

*Questions? [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), CTO at Iron Software, is always happy to help. Drop a comment below or check out [IronPDF](https://ironpdf.com) for more tutorials.*
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
