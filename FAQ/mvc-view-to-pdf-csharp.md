# How Can I Render Razor Views to PDF in ASP.NET MVC and Core with IronPDF?

Need to generate polished, branded PDFs directly from your existing Razor views in an ASP.NET MVC or Core project? IronPDF lets you turn any `.cshtml` Razor page into a high-fidelity PDF—no extra templates, no copy-pasting HTML, and no third-party services required. This FAQ walks you through everything you need to know: from basic setup to advanced customization, with code-first answers and practical tips.

---

## Why Would I Want to Render Razor Views Directly to PDF?

Many business apps require downloadable PDFs—think invoices, contracts, reports, or certificates. Manually duplicating Razor markup into separate PDF templates is tedious and error-prone, often leading to inconsistencies. By converting the same Razor views you use for web pages into PDFs, you keep your codebase DRY:

- Design your HTML/CSS once, use it for both browser and PDF.
- Leverage the full power of Razor: layouts, partials, model binding, and more.
- Enjoy pixel-perfect output thanks to IronPDF’s Chromium rendering engine, which supports modern CSS and JavaScript features.

For alternatives to Razor, like converting XML or XAML to PDF, see [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md) and [How do I export XAML to PDF in .NET MAUI?](xaml-to-pdf-maui-csharp.md).

---

## What Packages and Setup Are Required to Render Razor Views to PDF?

To get started, you’ll need the IronPDF core library and the appropriate IronPDF MVC extension for your framework:

| Your Framework      | NuGet Package Extension             |
|---------------------|------------------------------------|
| ASP.NET MVC (.NET Framework)  | `IronPdf.Extensions.Mvc.Framework` |
| ASP.NET Core / .NET 5+        | `IronPdf.Extensions.Mvc.Core`      |

Install both the core IronPdf package and the relevant extension:

```bash
# For classic .NET Framework MVC
Install-Package IronPdf
Install-Package IronPdf.Extensions.Mvc.Framework

# For ASP.NET Core / .NET 5/6/7/8+
Install-Package IronPdf
Install-Package IronPdf.Extensions.Mvc.Core
```

You can find setup guides and troubleshooting tips in the [IronPDF documentation](https://ironpdf.com).

---

## How Do I Render a Razor View to PDF in a Controller Action?

IronPDF makes it straightforward to convert a Razor view to PDF. Here’s a minimal example for a controller action that exports a view as a downloadable PDF:

```csharp
using IronPdf; // Install via NuGet
using IronPdf.Extensions.Mvc.Framework; // Or .Core for .NET Core

public class ReportsController : Controller
{
    public ActionResult ExportMonthlyReport()
    {
        var renderer = new ChromePdfRenderer();
        var dataModel = GetMonthlyReport(); // Fetch your data here

        var pdf = renderer.RenderView(
            HttpContext,
            "~/Views/Reports/Monthly.cshtml",
            dataModel);

        return File(pdf.BinaryData, "application/pdf", "MonthlyReport.pdf");
    }
}
```

- `ChromePdfRenderer` uses a real Chromium browser engine for accurate rendering.
- `RenderView()` accepts the HTTP context, Razor view path, and your model.
- The result is a `PdfDocument` with easy access to the PDF bytes.

Pro tip: You can use both absolute (`~`) and relative view paths.

If you’re interested in converting Razor views to PDF in a more general-purpose way, check out our in-depth guide: [How do I render Razor views to PDF in C#?](razor-view-to-pdf-csharp.md)

---

## Can I Pass Models to My Razor View When Generating PDFs?

Absolutely. You can pass strongly-typed models to your Razor view just as you would for normal HTML rendering. Here’s a quick example for a sales report:

**Model Classes:**
```csharp
public class SalesRow
{
    public string Product { get; set; }
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public class SalesReport
{
    public string Title { get; set; }
    public DateTime Generated { get; set; }
    public List<SalesRow> Items { get; set; }
}
```

**Controller Action:**
```csharp
using IronPdf;
using IronPdf.Extensions.Mvc.Framework;

public ActionResult DownloadSalesPdf()
{
    var model = new SalesReport
    {
        Title = "June 2024 Sales",
        Generated = DateTime.UtcNow,
        Items = salesService.GetJuneSales()
    };

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderView(HttpContext, "~/Views/Reports/Sales.cshtml", model);

    return File(pdf.BinaryData, "application/pdf", "Sales-June-2024.pdf");
}
```

Your Razor view (`Sales.cshtml`) can then use the model to dynamically render tables, charts, or other content. If it works in your browser, it’ll work in your PDF.

For more advanced templating scenarios (think XML-based), see [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md).

---

## How Do I Apply CSS and Maintain Brand Styling in PDF Output?

### Which CSS Features Are Supported When Rendering Razor to PDF?

IronPDF’s Chromium renderer supports the majority of modern CSS—flexbox, grid, media queries, and web fonts. Here’s what works best:

- Inline `<style>` blocks are most reliable for PDF output.
- Linked external stylesheets are supported, but the URLs must be accessible from your server environment (not just your dev machine).
- For consistent results, copy critical CSS into your Razor layout or view.

**Example:**
```html
<link rel="stylesheet" href="https://cdn.yourbrand.com/css/pdf.css" />
```
Or, for maximum reliability:
```html
<style>
    body { font-family: 'Segoe UI', Arial, sans-serif; }
    h1 { color: #1a237e; }
    /* More styles */
</style>
```

### Can I Use Shared Layouts and Partials with Razor-to-PDF?

Yes! Razor layouts and partials work exactly as they do for web pages, so you can keep headers, footers, and branding consistent across all PDFs. For example:

```csharp
@{
    Layout = "~/Views/Shared/_PdfLayout.cshtml";
}
```

Your `_PdfLayout.cshtml` can contain `<style>` blocks, logos, and navigation. For tips on drawing custom shapes or annotations, check [How do I draw lines and rectangles in a PDF with C#?](draw-line-rectangle-pdf-csharp.md).

### Is It Possible to Show or Hide Content Based on PDF Export?

Definitely! You can use Razor conditionals or CSS to control what appears only in PDF exports:

```csharp
@if (Request.Url.AbsolutePath.Contains("Export"))
{
    <div>This appears only in exported PDFs.</div>
}
```
Or with CSS:
```css
@media print { .no-print { display: none; } }
```

---

## How Do I Add Headers, Footers, and Page Numbers to My PDFs?

IronPDF makes it easy to inject HTML headers and footers onto every page. This is perfect for including page numbers, legal notes, or branding.

**Example: Adding Custom Header and Footer**
```csharp
using IronPdf;
using IronPdf.Extensions.Mvc.Framework;

public ActionResult ExportWithHeaders()
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center; font-size:12px;'>Confidential Report</div>",
        DrawDividerLine = true
    };
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:right; font-size:10px;'>Page {page} of {total-pages}</div>"
    };

    var pdf = renderer.RenderView(HttpContext, "~/Views/Reports/Annual.cshtml", GetReportData());
    return File(pdf.BinaryData, "application/pdf", "AnnualReport.pdf");
}
```

**Available placeholders:**

- `{page}`: current page number
- `{total-pages}`: total page count
- `{date}` and `{time}`: timestamp info

For more on customizing page numbering, see [How do I add page numbers to PDFs in C#?](https://ironpdf.com/blog/compare-to-other-components/questpdf-add-page-number-to-pdf/).

---

## How Can I Set Page Size, Orientation, and Margins for My PDF?

You can easily control these settings using `RenderingOptions`:

```csharp
using IronPdf;
using IronPdf.Extensions.Mvc.Framework;

public ActionResult ExportLandscapeA4()
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
    renderer.RenderingOptions.MarginTop = 15; // millimeters
    renderer.RenderingOptions.MarginBottom = 15;
    renderer.RenderingOptions.MarginLeft = 10;
    renderer.RenderingOptions.MarginRight = 10;

    var pdf = renderer.RenderView(HttpContext, "~/Views/Reports/WideView.cshtml", FetchModel());
    return File(pdf.BinaryData, "application/pdf", "WideView.pdf");
}
```

You can also set DPI and other advanced rendering options for sharper image output or QR codes.

---

## How Do I Serve PDFs for Download, Inline Preview, or Streaming?

By default, returning a `File()` result with a filename triggers a download in most browsers. If you’d prefer to display the PDF inline within the browser tab, adjust the `Content-Disposition` header:

```csharp
using IronPdf;
using IronPdf.Extensions.Mvc.Framework;

public ActionResult PreviewPdf()
{
    var pdf = RenderPdfDocument();
    Response.Headers.Add("Content-Disposition", "inline; filename=Preview.pdf");
    return File(pdf.BinaryData, "application/pdf");
}
```

For large PDFs or custom streaming scenarios, you can write the bytes directly to the response stream. For including file attachments in your PDFs, check [How do I add attachments to PDFs in C#?](add-attachments-pdf-csharp.md).

---

## What Does a Real-World Razor-to-PDF Invoice Generator Look Like?

Here’s a full end-to-end example for generating an invoice PDF from a model-driven Razor view.

**Model:**
```csharp
public class InvoiceLine
{
    public string Item { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
}

public class InvoiceModel
{
    public string InvoiceNo { get; set; }
    public string Company { get; set; }
    public DateTime DateIssued { get; set; }
    public string Customer { get; set; }
    public string Address { get; set; }
    public List<InvoiceLine> Lines { get; set; }
    public decimal Total => Lines.Sum(l => l.Total);
}
```

**Controller:**
```csharp
using IronPdf;
using IronPdf.Extensions.Mvc.Framework;

public class InvoiceController : Controller
{
    private readonly IInvoiceService _service;

    public InvoiceController(IInvoiceService service) => _service = service;

    public ActionResult Download(int id)
    {
        var invoice = _service.FindInvoice(id);
        if (invoice == null) return HttpNotFound();

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
        {
            HtmlFragment = "<div style='text-align:center; font-size:9px;'>Thank you for your business!</div>"
        };

        var pdf = renderer.RenderView(HttpContext, "~/Views/Invoice/View.cshtml", invoice);
        return File(pdf.BinaryData, "application/pdf", $"Invoice-{invoice.InvoiceNo}.pdf");
    }
}
```

**Razor View:**
```html
@model InvoiceModel
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial; padding: 28px; }
        .header { display: flex; justify-content: space-between; }
    </style>
</head>
<body>
    <div class="header">
        <div>
            <h2>INVOICE</h2>
            <p>@Model.Company</p>
        </div>
        <div>
            <p><strong>No:</strong> @Model.InvoiceNo</p>
            <p><strong>Date:</strong> @Model.DateIssued.ToShortDateString()</p>
        </div>
    </div>
    <h4>Billed To:</h4>
    <p>@Model.Customer<br/>@Model.Address</p>
    <!-- Table of line items -->
</body>
</html>
```

For custom drawing and annotations, see [How do I draw lines and rectangles in a PDF with C#?](draw-line-rectangle-pdf-csharp.md).

---

## How Do I Handle Images, Fonts, and Resource Paths in PDF Rendering?

### What Image Sources Are Supported?

- **Absolute URLs:**  
  ```html
  <img src="https://yourcdn.com/logo.png" />
  ```
- **Base64 Embedded:**  
  ```html
  <img src="data:image/png;base64,@Model.LogoBytesBase64" />
  ```
- **Local Files (rarely needed):**  
  ```html
  <img src="file:///C:/App/Content/logo.png" />
  ```

Remember: The PDF renderer runs server-side, so all resources must be accessible from the server environment.

### How About Fonts?

- Standard web fonts (Arial, Times) always work.
- Custom fonts should be referenced via `@font-face` or CDN.
- Don’t assume system fonts are present—embed or link them explicitly.

### How Do I Set a Base URL for Relative Paths?

If your HTML uses relative paths for images, CSS, or JS, set `BaseUrl` to help the renderer resolve them:

```csharp
renderer.RenderingOptions.BaseUrl = new Uri(Request.Url.GetLeftPart(UriPartial.Authority));
```

---

## What Advanced Features Are Available: Multilingual, Watermarks, Security?

### Can I Generate Multilingual or RTL PDFs?

Yes, IronPDF supports Unicode and right-to-left scripts as long as your CSS specifies a compatible font.

### How Do I Add a Watermark to My PDF?

Add a watermark with just a few lines:

```csharp
renderer.RenderingOptions.Watermark = new IronPdf.Watermark
{
    Text = "CONFIDENTIAL",
    FontSize = 60,
    Opacity = 0.07,
    Rotate = -25
};
```
For more, see [Watermarks](https://ironpdf.com/how-to/pdf-memory-stream/).

### Can I Password-Protect or Restrict My PDFs?

You can set user/owner passwords and restrict copying or editing:

```csharp
pdf.SecuritySettings.UserPassword = "userpass";
pdf.SecuritySettings.OwnerPassword = "adminpass";
pdf.SecuritySettings.AllowUserEditing = false;
pdf.SecuritySettings.AllowUserCopying = false;
```

---

## What Are Common Pitfalls and How Do I Troubleshoot Razor-to-PDF Issues?

- **CSS Not Applied:** Ensure styles are either inline or linked via absolute URLs accessible to the server.
- **Images Missing:** Use absolute URLs or base64; verify file permissions and network access.
- **Blank PDFs:** Render the view in the browser first to catch Razor errors.
- **JavaScript Not Running:** Most scripts work, but avoid relying on client-side rendering frameworks unless server-pre-rendered.
- **Slow Output:** Optimize large tables/images and cache static content where possible.
- **Watermarks or Licensing Issues:** IronPDF requires a license for production use; trial mode adds watermarks.
- **Running on Docker/Linux:** Make sure Chromium dependencies are present—see [IronPDF’s deployment docs](https://ironpdf.com).

If your workflow involves XAML or XML sources, check [How do I export XAML to PDF in .NET MAUI?](xaml-to-pdf-maui-csharp.md) and [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md).

---

## Where Can I Learn More or Get Help?

- For more examples, visit [IronPDF’s official documentation](https://ironpdf.com).
- To see what else Iron Software is building, check out [Iron Software](https://ironsoftware.com).
- Need to attach files to your generated PDFs? See [How do I add attachments to PDFs in C#?](add-attachments-pdf-csharp.md).
- Have a unique use-case, or hit a snag that’s not covered here? Drop your question in the comments.

---

*This FAQ was written by [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), CTO at Iron Software. Got a question that's not covered here? Drop it in the comments.*
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

**Jacob Mellor** — Creator of IronPDF, CTO at Iron Software. 41 years coding, 50++ person team, 41+ million NuGet downloads. Created first .NET components in 2005. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
