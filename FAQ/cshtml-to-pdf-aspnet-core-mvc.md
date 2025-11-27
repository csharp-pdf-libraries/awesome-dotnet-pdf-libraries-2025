# How Can I Export Razor (CSHTML) Views to PDF in ASP.NET Core MVC?

Yes, you can generate PDF files directly from your Razor (CSHTML) views in ASP.NET Core MVC—without maintaining separate HTML templates or fighting with unreliable PDF libraries. This FAQ walks you through the practical, modern workflow for converting your MVC Razor views to well-formatted PDFs using IronPDF, with tips, code samples, and real-world solutions to common gotchas.

---

## Why Should I Use Razor Views As PDF Templates?

Razor views (CSHTML files) make excellent PDF templates for several reasons. First, you avoid the hassle of duplicating markup—no need to manage both PDF-specific HTML and your web pages. With Razor, your data models, layouts, and logic are unified and benefit from full IntelliSense, type safety, and refactoring support in Visual Studio.

Unlike string-based HTML templates (Handlebars, Mustache, etc.), Razor is tightly integrated with MVC, enabling you to leverage partials, layouts, and shared styling for consistent branding. This also allows designers and developers to collaborate on a single source of truth for both web and PDF output.

If your PDFs are generated from background processes or microservices outside MVC, you might consider alternative rendering approaches. But for web apps, Razor remains the most maintainable route.

For a comparison of .NET PDF solutions, see [2025 HTML to PDF Solutions: .NET Comparison](2025-html-to-pdf-solutions-dotnet-comparison.md).

---

## How Do I Render a Razor View to an HTML String in .NET 6/7/8?

To convert a Razor view into an HTML string (which you can then turn into a PDF), you’ll need a reusable service that renders views outside of the normal HTTP pipeline. Here’s how you can do it:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.IO;
using System.Threading.Tasks;

public class RazorViewToStringService
{
    private readonly ICompositeViewEngine _engine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _services;

    public RazorViewToStringService(
        ICompositeViewEngine engine,
        ITempDataProvider tempDataProvider,
        IServiceProvider services)
    {
        _engine = engine;
        _tempDataProvider = tempDataProvider;
        _services = services;
    }

    public async Task<string> RenderToStringAsync(
        ControllerContext context,
        string viewPath,
        object model)
    {
        var viewResult = _engine.FindView(context, viewPath, false);
        if (!viewResult.Success)
            throw new FileNotFoundException($"Razor view '{viewPath}' not found.");

        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };
        var tempData = new TempDataDictionary(context.HttpContext, _tempDataProvider);

        using var writer = new StringWriter();
        var viewContext = new ViewContext(
            context,
            viewResult.View,
            viewData,
            tempData,
            writer,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return writer.ToString();
    }
}
```

**Why use this pattern?**  
- It supports both async and sync view rendering.
- You can inject it into any controller or service.
- It keeps your rendering logic encapsulated and testable.

For details on rendering regular MVC views directly to PDF, see [How do I output an MVC view as a PDF in C#?](mvc-view-to-pdf-csharp.md)

---

## How Do I Convert HTML from Razor to a PDF File Using IronPDF?

Once you have your HTML string, you can use IronPDF’s [ChromePdfRenderer](https://ironpdf.com/blog/videos/how-to-render-an-html-file-to-pdf-in-csharp-ironpdf/) to generate a PDF file. IronPDF is actively maintained, works cross-platform, and faithfully renders modern HTML and CSS.

Here’s a quick example:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf

var pdfRenderer = new ChromePdfRenderer();
var pdfDocument = pdfRenderer.RenderHtmlAsPdf(htmlContent);
pdfDocument.SaveAs("invoice.pdf"); // You can also access pdfDocument.BinaryData to return a stream or byte[]
```

No external processes, no complex setup—just pass your HTML and get a high-fidelity PDF back.

Curious how IronPDF stacks up against other PDF tools? See [2025 HTML to PDF Solutions: .NET Comparison](2025-html-to-pdf-solutions-dotnet-comparison.md).

---

## Can You Show a Full Example: Downloading a PDF Invoice from an MVC Controller?

Absolutely! Here’s a controller that lets users download an invoice as a PDF. It uses the service above to render your Razor view, and IronPDF to generate and stream the PDF:

```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public class InvoicesController : Controller
{
    private readonly RazorViewToStringService _viewService;

    public InvoicesController(RazorViewToStringService viewService)
    {
        _viewService = viewService;
    }

    [HttpGet("invoices/{id}/pdf")]
    public async Task<IActionResult> GetInvoicePdf(int id)
    {
        var invoiceModel = await FetchInvoiceViewModelAsync(id);

        var html = await _viewService.RenderToStringAsync(
            ControllerContext,
            "Invoices/PdfInvoiceTemplate",
            invoiceModel);

        var pdfRenderer = new ChromePdfRenderer();
        var pdf = pdfRenderer.RenderHtmlAsPdf(html);

        return File(pdf.BinaryData, "application/pdf", $"invoice-{id}.pdf");
    }

    private async Task<InvoicePdfViewModel> FetchInvoiceViewModelAsync(int id)
    {
        // Replace with your data access
        return new InvoicePdfViewModel
        {
            InvoiceNumber = $"INV-{id:0000}",
            CustomerName = "Acme Corp",
            CustomerAddress = "123 Main St",
            InvoiceDate = DateTime.UtcNow,
            LineItems = new List<LineItemViewModel>
            {
                new LineItemViewModel { Description = "Consulting", Quantity = 8, Price = 120 },
                new LineItemViewModel { Description = "Support", Quantity = 4, Price = 80 }
            },
            Subtotal = 1280,
            Tax = 128,
            Total = 1408,
            PaymentTerms = "Due in 30 days"
        };
    }
}
```

This pattern means you don’t have to deal with temp files or subprocesses—just C# all the way.

For more on returning PDFs as streams or memory, visit [How do I work with PDF MemoryStreams in C#?](pdf-memorystream-csharp.md)

---

### What Does a Typical Razor PDF View Look Like?

A Razor view for PDF exporting can be as simple or as detailed as you like. Here’s a clean example you might place at `/Views/Invoices/PdfInvoiceTemplate.cshtml`:

```html
@model InvoicePdfViewModel
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>Invoice @Model.InvoiceNumber</title>
    <style>
        @page { size: A4; margin: 20mm; }
        body { font-family: Arial, sans-serif; color: #333; }
        .header { font-size: 22px; font-weight: bold; margin-bottom: 16px; }
        .address { margin-bottom: 18px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { border-bottom: 1px solid #ccc; padding: 7px; }
        th { background: #f0f0f0; }
        .totals { text-align: right; font-weight: bold; }
    </style>
</head>
<body>
    <div class="header">Invoice @Model.InvoiceNumber</div>
    <div class="address">
        <strong>To:</strong> @Model.CustomerName<br />
        @Model.CustomerAddress<br />
        <strong>Date:</strong> @Model.InvoiceDate.ToShortDateString()
    </div>
    <table>
        <thead>
            <tr>
                <th>Description</th>
                <th>Qty</th>
                <th>Price</th>
                <th>Amount</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in Model.LineItems)
            {
                <tr>
                    <td>@item.Description</td>
                    <td>@item.Quantity</td>
                    <td>@item.Price.ToString("C")</td>
                    <td>@(item.Quantity * item.Price).ToString("C")</td>
                </tr>
            }
        </tbody>
    </table>
    <div class="totals">
        Subtotal: @Model.Subtotal.ToString("C")<br />
        Tax: @Model.Tax.ToString("C")<br />
        <span>Total: @Model.Total.ToString("C")</span>
    </div>
    <div>Payment Terms: @Model.PaymentTerms</div>
</body>
</html>
```

**Tip:** Inline styles are far more reliable for PDF rendering than external CSS.

---

## How Can I Use Razor Layouts, Partials, and Shared Styles with PDFs?

Razor’s support for layouts and partial views means you can reuse headers, footers, and complex sections across different PDFs—just like you do for web pages.

### How Do I Add a Consistent Header and Footer to All PDFs?

Create a layout in `/Views/Shared/_PdfLayout.cshtml`:

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <style>
        body { margin: 28px; font-family: Verdana, sans-serif; }
        .pdf-header { text-align: center; border-bottom: 2px solid #333; margin-bottom: 12px; }
        .pdf-footer { text-align: center; color: #555; font-size: 11px; border-top: 1px solid #bbb; margin-top: 24px; }
    </style>
</head>
<body>
    <div class="pdf-header">
        <img src="https://placehold.co/150x50?text=Logo" alt="Logo" /><br />
        <span>Your Company Name – Invoices</span>
    </div>
    @RenderBody()
    <div class="pdf-footer">
        Generated: @DateTime.Now.ToShortDateString()
    </div>
</body>
</html>
```

In your PDF template, specify the layout:

```csharp
@{
    Layout = "_PdfLayout";
}
```

### How Can I Reuse Table Sections Across Multiple PDFs?

Define a partial view like `/Views/Shared/_LineItemsTable.cshtml` and use it in your PDFs:

```html
@model List<LineItemViewModel>
<table>
    <thead>
        <tr>
            <th>Description</th><th>Qty</th><th>Price</th><th>Amount</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model)
        {
            <tr>
                <td>@item.Description</td>
                <td>@item.Quantity</td>
                <td>@item.Price.ToString("C")</td>
                <td>@(item.Quantity * item.Price).ToString("C")</td>
            </tr>
        }
    </tbody>
</table>
```

Reference it with:

```csharp
@await Html.PartialAsync("_LineItemsTable", Model.LineItems)
```

For more on modifying or drawing on PDFs (like adding images, lines, or rectangles), see [How do I add images to a PDF in C#?](add-images-to-pdf-csharp.md) and [How do I draw lines and rectangles in PDFs with C#?](draw-line-rectangle-pdf-csharp.md)

---

## How Do I Handle Page Breaks, Headers, and Footers in Multi-Page PDFs?

### How Do I Control Page Breaks in Razor PDF Output?

Use CSS for print to manage page breaks:

```css
.section { page-break-after: always; }
.no-break { page-break-inside: avoid; }
thead { display: table-header-group; }
tfoot { display: table-footer-group; }
```

- `page-break-after: always;` adds a forced page break after a specific section.
- `page-break-inside: avoid;` prevents tables or blocks from splitting.
- Table header/footer groups ensure headers repeat on each page.

For more, see the official [HTML to PDF page break guide](https://ironpdf.com/how-to/html-to-pdf-page-breaks/).

### How Can I Add Dynamic Page Numbers?

IronPDF lets you insert page numbers via footer options:

```csharp
var renderer = new ChromePdfRenderer();
renderer.PrintOptions.Footer = new SimpleHeaderFooter()
{
    CenterText = "Page {page} of {total-pages}",
    DrawDividerLine = true
};
```

This automatically replaces placeholders with the correct values.

See also [How do I add page numbers to PDFs in C#?](https://ironpdf.com/blog/compare-to-other-components/questpdf-add-page-number-to-pdf/)

---

## What Are the Best Practices for PDF View Models in Razor?

Always pass fully populated, flat view models to your Razor PDF views. Avoid lazy-loading or navigation properties in the view, as these can cause exceptions or incomplete data in the output.

Example view models:

```csharp
public class InvoicePdfViewModel
{
    public string InvoiceNumber { get; set; }
    public string CustomerName { get; set; }
    public string CustomerAddress { get; set; }
    public DateTime InvoiceDate { get; set; }
    public List<LineItemViewModel> LineItems { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string PaymentTerms { get; set; }
}

public class LineItemViewModel
{
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
```

Calculate all totals and fill all properties **before** rendering. Keep your views purely for display—do not fetch data from within Razor.

---

## What Are Common Pitfalls When Exporting Razor Views to PDF?

### Why Doesn’t My PDF Load CSS or JS?

PDF converters often ignore or cannot access external files. Inline or embed all critical CSS right in your Razor view. If you must use external files, use absolute URLs.

### Why Are Images Missing in My PDF?

Relative paths break. Use full URLs or embed images as base64 data URIs. For more techniques, see [How do I add images to a PDF in C#?](add-images-to-pdf-csharp.md).

### Why Can’t My View Be Found?

Make sure the view path and name are correct, including case and folder structure. Use the relative path from the `Views` folder.

### Why Does My PDF Look Different From the Browser?

Even with Chromium-based engines, print rendering and web rendering can differ. Use `@media print` CSS and test your output early.

### Why Are My Large PDFs Slow or Failing?

Paginate large data sets and optimize your CSS/images. IronPDF is robust, but every system has limits.

### Why Does It Work Locally But Not in Docker or Azure?

Check that all dependencies are present in your deployment environment. See IronPDF’s docs for container/cloud deployment tips.

---

## Where Can I Learn More or Find Further Resources?

The [IronPDF documentation](https://ironpdf.com) has in-depth guides, recipes, and troubleshooting. For broader .NET document and data tools (barcodes, OCR, Excel), visit [Iron Software](https://ironsoftware.com).

For advanced scenarios—like batch exports, background PDF generation, or direct MemoryStream access—explore [working with PDF MemoryStreams in C#](pdf-memorystream-csharp.md).

---

*[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), CTO at [Iron Software](https://ironsoftware.com). Building tools that make developers' lives easier since 2017.*
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

**About the Author**: Jacob Mellor is Chief Technology Officer at Iron Software, where PDF tools are used by GE and other Fortune 500 companies. With expertise in C++, JavaScript, C#, Jacob focuses on making PDF generation accessible to all .NET developers. [Author Page](https://ironsoftware.com/about-us/authors/jacobmellor/)
