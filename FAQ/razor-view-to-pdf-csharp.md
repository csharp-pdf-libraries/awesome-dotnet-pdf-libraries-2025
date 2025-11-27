# How Can I Render Razor Views to PDF in C#?

Rendering Razor views directly to PDF in C# is a powerful way to leverage your existing UI templates for printable documents like invoices, reports, and more. Using IronPDF, you can transform `.cshtml` Razor files into high-fidelity PDFs—complete with models, layouts, partials, and advanced logic—without duplicating templates or learning a new syntax. This FAQ walks you through the process, from setup to troubleshooting, with practical code samples and insights for real-world scenarios.

---

## How Do I Quickly Render a Razor View to PDF in C#?

If you want to jump straight in, you just need IronPDF and its Razor extension. Here's the simplest way to render a Razor view to PDF:

```csharp
using IronPdf;
using IronPdf.Extensions.Razor;
// Install-Package IronPdf
// Install-Package IronPdf.Extensions.Razor

var pdfGenerator = new ChromePdfRenderer();
var pdfDoc = pdfGenerator.RenderRazorToPdf("/Views/Invoice.cshtml", model);
pdfDoc.SaveAs("invoice.pdf");
```
With just a couple of lines, you can turn any Razor view into a PDF. If you've already used Razor in MVC or Razor Pages, the workflow will feel very familiar.

---

## Why Would I Use Razor Views for PDF Generation Instead of HTML Strings or Other Templates?

### What Are the Key Benefits of Razor Views for PDFs?

Using Razor for PDFs means you can:

- **Reuse existing front-end templates:** No need to maintain separate HTML for the web and for PDFs.
- **Leverage strong typing:** Use C# models for compile-time safety and IntelliSense.
- **Use layouts and partials:** Maintain branding and structure with `_Layout.cshtml` and partial views.
- **Write real C# logic:** Full language support—no awkward or unfamiliar templating syntax.
- **Preview in your browser:** Design, style, and debug your PDF as a regular web page first.

If you want even more detail about why developers choose Razor for PDF generation, check out IronPDF’s [Razor Pages documentation](https://ironpdf.com/docs/questions/razor-pdf/).

---

## What Packages Do I Need, and How Should My Project Be Structured?

### Which NuGet Packages Should I Install?

To enable Razor-to-PDF rendering, install the following NuGet packages:

```powershell
Install-Package IronPdf
Install-Package IronPdf.Extensions.Razor
```
Or for the .NET CLI:

```bash
dotnet add package IronPdf
dotnet add package IronPdf.Extensions.Razor
```

### Where Should I Place My Razor PDF Templates?

- Keep your PDF-related Razor files in a `/Views` folder (or your preferred Razor directory).
- Use strong models for clarity and safety.
- Make sure that any static assets (such as images and CSS) are accessible to the PDF renderer. For tips on images, see [How Do I Handle Images in Razor-PDF Workflows?](#how-do-i-handle-images-in-razor-pdf-workflows)

---

## How Do I Generate a PDF from a Strongly-Typed Razor View?

### Can I Use My MVC Models with Razor-to-PDF?

Absolutely! Design your Razor view as you would for the web, then pass your model directly to the renderer. For example, suppose you have an `InvoiceModel`:

```csharp
public class InvoiceModel
{
    public string InvoiceNumber { get; set; }
    public DateTime Date { get; set; }
    public string CustomerName { get; set; }
    public List<LineItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Quantity * i.UnitPrice);
}

public class LineItem
{
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
```

Your Razor view (`Views/Invoice.cshtml`) can use this model just like in MVC.

### How Would I Render and Save the PDF?

```csharp
using IronPdf;
using IronPdf.Extensions.Razor;

var invoice = new InvoiceModel
{
    InvoiceNumber = "INV-2024-001",
    Date = DateTime.UtcNow,
    CustomerName = "Contoso Ltd.",
    Items = new List<LineItem>
    {
        new() { Description = "Service Fee", Quantity = 2, UnitPrice = 150.00m },
        new() { Description = "Consulting", Quantity = 5, UnitPrice = 75.00m }
    }
};

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderRazorToPdf("/Views/Invoice.cshtml", invoice);
pdf.SaveAs("output-invoice.pdf");
```

You can preview the `.cshtml` in your browser for quick visual tweaks before generating PDFs.

---

## How Do I Use Layouts and Sections for Consistent PDF Branding?

### Can Razor Layouts Be Used for PDF Templates?

Yes! Razor layouts (like `_Layout.cshtml` or custom layouts) help you maintain a consistent header, footer, and overall structure. For example, your layout might include a logo, contact details, or standardized footer.

#### Example Layout (`Views/Shared/_PdfLayout.cshtml`):

```html
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: 'Segoe UI', sans-serif; margin: 0; padding: 40px; }
        header { border-bottom: 2px solid #333; padding-bottom: 15px; margin-bottom: 25px; }
        footer { position: fixed; bottom: 10px; width: 90%; text-align: center; color: #888; }
    </style>
    @RenderSection("Styles", required: false)
</head>
<body>
    <header>
        <img src="https://example.com/logo.png" height="45" alt="Logo"/>
    </header>
    @RenderBody()
    <footer>
        &copy; @DateTime.Now.Year My Company | Confidential
    </footer>
</body>
</html>
```

#### Example Usage in a View:

```html
@model ReportModel
@{
    Layout = "_PdfLayout";
}

@section Styles {
    <style>
        .report-table { border-collapse: collapse; width: 100%; }
    </style>
}

<h1>@Model.Title</h1>
<p>Generated: @DateTime.Now</p>
```

**Why use layouts?** If your branding changes, just update one file.

---

## How Can I Reuse Components in PDFs with Partial Views?

### How Do Partial Views Work in Razor-PDF Workflows?

Break complex documents into manageable, reusable pieces. For example, you can have `_CustomerInfo.cshtml` or `_LineItem.cshtml` as partials.

#### Example Partial (`Views/Shared/_CustomerInfo.cshtml`):

```html
@model Customer
<div>
    <strong>@Model.Name</strong><br/>
    @Model.Address<br/>
    @Model.Email
</div>
```

#### Using Partial Views in Your Main Template:

```html
@model OrderModel

<partial name="_CustomerInfo" model="Model.Customer" />

@foreach (var item in Model.Items)
{
    <partial name="_LineItem" model="item" />
}
```

This keeps your templates DRY and easier to maintain.

---

## Can I Use ViewData and ViewBag with Razor-to-PDF?

### How Do I Pass Dynamic Data Beyond the Model?

IronPDF’s Razor extension supports `ViewData` and `ViewBag`, just like regular MVC. This is useful for passing configuration, runtime settings, or branding info alongside your model.

#### Example:

```csharp
using IronPdf;
using IronPdf.Extensions.Razor;

var renderer = new ChromePdfRenderer();
var extraData = new Dictionary<string, object>
{
    { "BrandName", "Acme Inc." },
    { "ReportDate", DateTime.Today }
};

var pdf = renderer.RenderRazorToPdf("/Views/Report.cshtml", model, extraData);
```

Then, in your Razor template:

```html
@{
    var brand = ViewData["BrandName"];
    var date = ViewData["ReportDate"];
}
<h1>@brand Report</h1>
<p>Date: @date</p>
```

This approach is especially handy for injecting values that aren't part of your main model.

---

## Is Full C# Supported for Conditional Logic and Formatting?

### How Do I Use C# Logic and Format Data in PDFs?

Razor syntax supports full C# logic, so you can use `if`, `switch`, loops, and more:

```html
@if (Model.IsPaid)
{
    <div class="status-paid">PAID</div>
}
else
{
    <div class="status-unpaid">Due by @Model.DueDate.ToShortDateString()</div>
}

@switch (Model.Status)
{
    case OrderStatus.Pending:
        <span class="badge badge-warning">Pending</span>
        break;
    case OrderStatus.Shipped:
        <span class="badge badge-info">Shipped</span>
        break;
    case OrderStatus.Completed:
        <span class="badge badge-success">Completed</span>
        break;
}
```

### How Do I Apply Custom Formatting to Data?

You can format dates, currency, and more using C#:

```html
@foreach (var entry in Model.Entries)
{
    <tr>
        <td>@entry.Date.ToString("dd MMM yyyy")</td>
        <td>@entry.Amount.ToString("C")</td>
        <td class="@(entry.Amount < 0 ? "negative" : "positive")">
            @entry.Amount
        </td>
    </tr>
}
```

**Tip:** Since Razor renders the same as your web views, preview in browser to catch formatting issues before rendering to PDF.

---

## How Do I Generate PDFs from ASP.NET Controllers for Downloads?

### How Can I Let Users Download PDFs from My Web App?

Generating PDFs on-demand from controllers is straightforward. Here’s how you might return a PDF as a file download in ASP.NET Core:

```csharp
using IronPdf;
using IronPdf.Extensions.Razor;
using Microsoft.AspNetCore.Mvc;

public class ReportsController : Controller
{
    public IActionResult DownloadReport(int id)
    {
        var reportData = _reportService.GetReport(id);

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderRazorToPdf("/Views/Report.cshtml", reportData);

        return File(pdf.BinaryData, "application/pdf", $"report-{id}.pdf");
    }
}
```

### Can I Generate PDFs Asynchronously?

Yes—IronPDF supports async methods for rendering large or complex documents:

```csharp
var pdf = await renderer.RenderRazorToPdfAsync("/Views/Report.cshtml", reportData);
```

For more on integrating with MVC, see [How do I generate a PDF from an MVC View in C#?](mvc-view-to-pdf-csharp.md).

---

## How Do I Add Page Numbers, Headers, and Footers to PDFs?

### What’s the Best Way to Insert Page Numbers?

You can add page numbers and custom headers or footers using IronPDF’s `HtmlHeaderFooter` feature:

```csharp
using IronPdf;
using IronPdf.Extensions.Razor;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
    DrawDividerLine = true
};
var pdf = renderer.RenderRazorToPdf("/Views/Report.cshtml", model);
pdf.SaveAs("paged-report.pdf");
```

Supported tokens include `{page}` for the current page and `{total-pages}` for the total number of pages.

For advanced pagination, see [How do I control PDF pagination?](https://ironpdf.com/how-to/html-to-pdf-page-breaks/).

---

## How Do I Handle Images in Razor-PDF Workflows?

### What’s the Best Way to Include Images in PDFs?

Images can be linked by:

- **Absolute URLs:**  
  ```html
  <img src="https://mydomain.com/images/logo.png" />
  ```
- **Base64 Encoding:**  
  ```html
  <img src="data:image/png;base64,@Model.EmbeddedLogo" />
  ```
- **Local Files:**  
  ```html
  <img src="file:///C:/Projects/app/wwwroot/logo.png" />
  ```

### How Do I Set a Base URL for Relative Paths?

If your template uses relative URLs, set the `BaseUrl` property:

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.BaseUrl = new Uri("https://myapp.com/");

var pdf = renderer.RenderRazorToPdf("/Views/Report.cshtml", model);
```

**Troubleshooting tips:**  
- Ensure remote images are accessible to the rendering process (watch out for firewalls or permissions).
- For local development, consider serving assets with a local web server, or embed images as base64.

---

## What Advanced Features or Real-World Scenarios Should I Consider?

### Can I Create Multi-Page PDFs and Insert Page Breaks Manually?

Yes! Use CSS for page breaks:

```html
<style>
.page-break { page-break-after: always; }
</style>

<div>Section 1</div>
<div class="page-break"></div>
<div>Section 2</div>
```

### How Do I Customize CSS for Print and PDF Output?

Add print-specific CSS:

```html
<style>
@media print {
    .no-print { display: none; }
    body { background: #fff; }
}
</style>
```

### Can I Embed Charts, SVG, or Dynamic Content?

Export charts as images or SVG and embed with `<img>`. Inline SVG works well in most cases with IronPDF.

### How Do I Secure My PDFs?

IronPDF lets you set passwords and permissions:

```csharp
pdf.Password = "lockitdown";
pdf.UserAccessPermissions = PdfDocument.Permissions.AllowPrinting;
pdf.SaveAs("protected.pdf");
```

### Can I Generate PDFs in Background Jobs or Serverless Functions?

Yes—just ensure Razor templates and assets are accessible. IronPDF works in environments like Hangfire, Azure Functions, and other job schedulers.

For more on advanced document scenarios, see [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md) or [How can I generate PDFs from XAML in .NET MAUI?](xaml-to-pdf-maui-csharp.md).

---

## What Are Common Pitfalls or Troubleshooting Tips for Razor-to-PDF?

- **Case sensitivity:** Path issues crop up on Linux containers.
- **Missing layouts/partials:** A missing file can cause blank PDFs or exceptions.
- **Static assets not displaying:** Double-check URLs, permissions, and use absolute or base64 where needed.
- **CSS rendering differences:** Not all CSS features (like certain fonts or advanced grids) work identically in PDF vs. browser. Test with your actual data.
- **Model errors:** If required properties are missing, Razor will throw an error—just like in MVC.
- **Performance:** Large or complex PDFs may take longer to generate. Use async and consider caching.
- **Licensing:** IronPDF is commercial with a trial. If you need open-source, compare carefully—few alternatives support Razor rendering.

**Debugging Suggestions:**
- Render the Razor as HTML first and preview in your browser.
- Log all exceptions—IronPDF typically gives clear error messages.
- Consult IronPDF’s [documentation and troubleshooting resources](https://ironpdf.com/docs/).

For comparing PDF libraries, check [How do I choose the right C# PDF library?](choose-csharp-pdf-library.md).

---

## How Does Razor-to-PDF Compare to Other HTML-to-PDF Techniques?

If you’re considering alternatives, Razor-to-PDF with IronPDF offers:

- No template duplication (reuse what you already have)
- Full C# logic, models, and strongly typed views
- Integration with layouts, partials, and sections
- Easy browser-based design and fast iteration

If your workflow relies on converting raw HTML or external files, see [How do I convert HTML to PDF in C#?](convert-html-to-pdf-csharp.md).

---

## Where Can I Learn More or Get Support for IronPDF?

You’ll find comprehensive docs and guides at [IronPDF](https://ironpdf.com) and additional developer resources at [Iron Software](https://ironsoftware.com).

If you have specific use cases—like MVC, XAML, or XML to PDF—see these related FAQs:
- [How do I generate a PDF from an MVC View in C#?](mvc-view-to-pdf-csharp.md)
- [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md)
- [How can I generate PDFs from XAML in .NET MAUI?](xaml-to-pdf-maui-csharp.md)
- [How do I convert HTML to PDF in C#?](convert-html-to-pdf-csharp.md)
- [How do I choose the right C# PDF library?](choose-csharp-pdf-library.md)

And for more advanced rendering options, see the [ChromePdfRenderer video tutorial](https://ironpdf.com/blog/videos/how-to-render-webgl-sites-to-pdf-in-csharp-ironpdf/).

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

Jacob Mellor (CTO, Iron Software) has spent 41 years solving document processing challenges. His philosophy: "Developer usability is the most underrated part of API design. You can have the most powerful code in the world, but if developers can't understand and get to 'Hello World' in 5 minutes, you've already lost." Connect on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
