# Generate PDF Reports in ASP.NET Core: Complete Guide

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4)](https://dotnet.microsoft.com/apps/aspnet)
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> PDF generation in ASP.NET Core seems simple until you discover that most libraries produce unstable output, fail accessibility audits, or require cloud services that add latency. This guide shows you the production-ready approach.

---

## Table of Contents

1. [The ASP.NET Core PDF Challenge](#the-aspnet-core-pdf-challenge)
2. [Quick Start](#quick-start)
3. [Rendering Razor Views to PDF](#rendering-razor-views-to-pdf)
4. [Controller Actions](#controller-actions)
5. [Minimal API Endpoints](#minimal-api-endpoints)
6. [Background PDF Generation](#background-pdf-generation)
7. [Headers, Footers, and Page Numbers](#headers-footers-and-page-numbers)
8. [Styling for PDF Output](#styling-for-pdf-output)
9. [Common Use Cases](#common-use-cases)
10. [Production Considerations](#production-considerations)
11. [Deployment](#deployment)

---

## The ASP.NET Core PDF Challenge

Here's what I've learned after supporting thousands of ASP.NET Core developers:

**The naive approach fails:**
1. Team picks a "simple" PDF library
2. Basic PDFs work in development
3. Production reveals: missing fonts, broken layouts, accessibility failures
4. Compliance audit fails on Section 508/EU standards
5. Team scrambles to replace library

**The cloud approach adds friction:**
1. Team uses cloud PDF API
2. Works, but 2-5 second latency per PDF
3. Customer data flowing through third-party servers
4. Compliance team raises concerns
5. Costs accumulate month over month

**The right approach:** Local Chromium rendering with IronPDF—fast, compliant, data stays local.

---

## Quick Start

### 1. Install IronPDF

```bash
dotnet add package IronPdf
```

### 2. Generate Your First PDF

```csharp
using IronPdf;

var pdf = ChromePdfRenderer.RenderHtmlAsPdf("<h1>Hello from ASP.NET Core</h1>");
pdf.SaveAs("hello.pdf");
```

### 3. Return PDF from Controller

```csharp
[HttpGet("invoice/{id}")]
public IActionResult GetInvoice(int id)
{
    var html = $"<h1>Invoice #{id}</h1><p>Thank you for your purchase!</p>";
    var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

    return File(pdf.BinaryData, "application/pdf", $"invoice-{id}.pdf");
}
```

That's it. The PDF is generated with full Chromium rendering and returned as a download.

---

## Rendering Razor Views to PDF

The real power comes from reusing your Razor views and templates.

### Setup: IRazorViewToStringRenderer

First, create a service to render Razor views to HTML strings:

```csharp
// Services/IRazorViewToStringRenderer.cs
public interface IRazorViewToStringRenderer
{
    Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);
}

// Services/RazorViewToStringRenderer.cs
public class RazorViewToStringRenderer : IRazorViewToStringRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    public RazorViewToStringRenderer(
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
    {
        var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        using var writer = new StringWriter();

        var viewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: false);

        if (!viewResult.Success)
        {
            throw new InvalidOperationException($"Could not find view: {viewName}");
        }

        var viewDictionary = new ViewDataDictionary<TModel>(
            new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
            writer,
            new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);
        return writer.ToString();
    }
}
```

### Register the Service

```csharp
// Program.cs
builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
```

### Create a Razor View Template

```cshtml
@* Views/Pdf/InvoiceTemplate.cshtml *@
@model InvoiceViewModel

<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; margin: 40px; }
        .header { display: flex; justify-content: space-between; margin-bottom: 40px; }
        .logo { height: 60px; }
        .invoice-details { text-align: right; }
        .items { width: 100%; border-collapse: collapse; margin: 20px 0; }
        .items th, .items td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        .items th { background-color: #f5f5f5; }
        .total { text-align: right; font-size: 1.2em; font-weight: bold; }
        .footer { margin-top: 40px; text-align: center; color: #666; font-size: 0.9em; }
    </style>
</head>
<body>
    <div class="header">
        <img src="https://yourcompany.com/logo.png" class="logo" alt="Company Logo" />
        <div class="invoice-details">
            <h1>Invoice #@Model.InvoiceNumber</h1>
            <p>Date: @Model.Date.ToString("MMMM dd, yyyy")</p>
            <p>Due: @Model.DueDate.ToString("MMMM dd, yyyy")</p>
        </div>
    </div>

    <div class="customer">
        <h3>Bill To:</h3>
        <p>@Model.CustomerName</p>
        <p>@Model.CustomerAddress</p>
        <p>@Model.CustomerEmail</p>
    </div>

    <table class="items">
        <thead>
            <tr>
                <th>Description</th>
                <th>Quantity</th>
                <th>Unit Price</th>
                <th>Amount</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in Model.Items)
            {
                <tr>
                    <td>@item.Description</td>
                    <td>@item.Quantity</td>
                    <td>@item.UnitPrice.ToString("C")</td>
                    <td>@item.Total.ToString("C")</td>
                </tr>
            }
        </tbody>
    </table>

    <div class="total">
        <p>Subtotal: @Model.Subtotal.ToString("C")</p>
        <p>Tax (@Model.TaxRate%): @Model.TaxAmount.ToString("C")</p>
        <p>Total: @Model.Total.ToString("C")</p>
    </div>

    <div class="footer">
        <p>Thank you for your business!</p>
        <p>Questions? Contact billing@yourcompany.com</p>
    </div>
</body>
</html>
```

### Use in Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IRazorViewToStringRenderer _renderer;
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(
        IRazorViewToStringRenderer renderer,
        IInvoiceService invoiceService)
    {
        _renderer = renderer;
        _invoiceService = invoiceService;
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadInvoicePdf(int id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null) return NotFound();

        var viewModel = new InvoiceViewModel
        {
            InvoiceNumber = invoice.Number,
            Date = invoice.CreatedAt,
            DueDate = invoice.DueDate,
            CustomerName = invoice.Customer.Name,
            CustomerAddress = invoice.Customer.Address,
            CustomerEmail = invoice.Customer.Email,
            Items = invoice.Items.Select(i => new InvoiceItemViewModel
            {
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Total = i.Quantity * i.UnitPrice
            }).ToList(),
            Subtotal = invoice.Subtotal,
            TaxRate = invoice.TaxRate,
            TaxAmount = invoice.TaxAmount,
            Total = invoice.Total
        };

        var html = await _renderer.RenderViewToStringAsync("Pdf/InvoiceTemplate", viewModel);

        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

        return File(pdf.BinaryData, "application/pdf", $"invoice-{invoice.Number}.pdf");
    }
}
```

---

## Controller Actions

### Download PDF

```csharp
[HttpGet("report")]
public IActionResult DownloadReport()
{
    var pdf = ChromePdfRenderer.RenderHtmlAsPdf(GetReportHtml());

    // Force download
    return File(pdf.BinaryData, "application/pdf", "report.pdf");
}
```

### View PDF in Browser

```csharp
[HttpGet("preview")]
public IActionResult PreviewReport()
{
    var pdf = ChromePdfRenderer.RenderHtmlAsPdf(GetReportHtml());

    // Display inline (no download dialog)
    Response.Headers.Add("Content-Disposition", "inline; filename=report.pdf");
    return File(pdf.BinaryData, "application/pdf");
}
```

### Stream Large PDFs

```csharp
[HttpGet("large-report")]
public IActionResult StreamLargeReport()
{
    var pdf = ChromePdfRenderer.RenderHtmlAsPdf(GetLargeReportHtml());

    var stream = pdf.Stream;
    return new FileStreamResult(stream, "application/pdf")
    {
        FileDownloadName = "large-report.pdf"
    };
}
```

---

## Minimal API Endpoints

For .NET 6+ minimal APIs:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/pdf/invoice/{id}", async (int id) =>
{
    var html = $"<h1>Invoice #{id}</h1><p>Generated: {DateTime.Now}</p>";
    var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

    return Results.File(pdf.BinaryData, "application/pdf", $"invoice-{id}.pdf");
});

app.MapPost("/pdf/from-html", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var html = await reader.ReadToEndAsync();

    var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

    return Results.File(pdf.BinaryData, "application/pdf", "document.pdf");
});

app.Run();
```

---

## Background PDF Generation

For long-running or batch PDF generation, use background services:

### With IHostedService

```csharp
public class PdfGenerationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Channel<PdfRequest> _channel;

    public PdfGenerationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _channel = Channel.CreateUnbounded<PdfRequest>();
    }

    public async Task QueuePdfRequest(PdfRequest request)
    {
        await _channel.Writer.WriteAsync(request);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var renderer = new ChromePdfRenderer();

        await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                var pdf = renderer.RenderHtmlAsPdf(request.Html);
                pdf.SaveAs(request.OutputPath);

                // Notify completion
                await NotifyCompletion(request.Id, request.OutputPath);
            }
            catch (Exception ex)
            {
                await NotifyFailure(request.Id, ex.Message);
            }
        }
    }
}
```

### With Hangfire

```csharp
public class PdfJobService
{
    [Queue("pdf-generation")]
    public void GeneratePdf(string html, string outputPath)
    {
        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
        pdf.SaveAs(outputPath);
    }
}

// Queue the job
BackgroundJob.Enqueue<PdfJobService>(x => x.GeneratePdf(html, outputPath));
```

---

## Headers, Footers, and Page Numbers

### HTML Headers and Footers

```csharp
var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; font-size: 10px; display: flex; justify-content: space-between;'>
            <span>Confidential</span>
            <span>Your Company Name</span>
            <span>{date}</span>
        </div>",
    DrawDividerLine = true,
    MaxHeight = 25
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='width: 100%; text-align: center; font-size: 9px; color: #666;'>
            Page {page} of {total-pages}
        </div>",
    DrawDividerLine = true,
    MaxHeight = 20
};

var pdf = renderer.RenderHtmlAsPdf(html);
```

### Available Placeholders

| Placeholder | Output |
|-------------|--------|
| `{page}` | Current page number |
| `{total-pages}` | Total page count |
| `{date}` | Current date |
| `{time}` | Current time |
| `{html-title}` | Document title |
| `{url}` | Source URL |

---

## Styling for PDF Output

### Use Print Media Queries

```css
/* Regular screen styles */
.sidebar { display: block; }
.navigation { display: flex; }

/* PDF/Print specific styles */
@media print {
    .sidebar { display: none; }
    .navigation { display: none; }
    .content { width: 100%; }

    /* Force page breaks */
    .chapter { page-break-before: always; }

    /* Prevent breaks inside elements */
    .data-row { page-break-inside: avoid; }
    table { page-break-inside: avoid; }
}
```

### Configure Print Media Type

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
```

### Common Styling Tips

```css
/* Ensure backgrounds print */
* { -webkit-print-color-adjust: exact !important; }

/* Control page margins */
@page {
    size: A4;
    margin: 20mm;
}

/* First page different margin (for cover) */
@page :first {
    margin-top: 0;
}

/* Force tables to not break across pages */
table {
    page-break-inside: avoid;
}

/* Keep headers with content */
h2, h3 {
    page-break-after: avoid;
}
```

---

## Common Use Cases

### Invoice Generation

```csharp
public async Task<byte[]> GenerateInvoicePdf(Invoice invoice)
{
    var html = await _renderer.RenderViewToStringAsync("Pdf/Invoice", new InvoiceViewModel(invoice));

    var pdfRenderer = new ChromePdfRenderer();
    pdfRenderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    pdfRenderer.RenderingOptions.MarginTop = 20;
    pdfRenderer.RenderingOptions.MarginBottom = 30;

    pdfRenderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = $"<div style='text-align:center;font-size:9px;'>Invoice #{invoice.Number} | Page {{page}}</div>"
    };

    return pdfRenderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Dashboard Reports

```csharp
public async Task<byte[]> GenerateDashboardPdf(DashboardData data)
{
    var html = await _renderer.RenderViewToStringAsync("Pdf/Dashboard", data);

    var pdfRenderer = new ChromePdfRenderer();
    pdfRenderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    pdfRenderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

    // Wait for JavaScript charts to render
    pdfRenderer.RenderingOptions.WaitFor.JavaScript(2000);

    return pdfRenderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Batch Statement Generation

```csharp
public async Task GenerateMonthlyStatements(IEnumerable<Customer> customers)
{
    var renderer = new ChromePdfRenderer();

    foreach (var customer in customers)
    {
        var html = await _renderer.RenderViewToStringAsync("Pdf/Statement", customer);
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs($"statements/{customer.Id}-{DateTime.Now:yyyy-MM}.pdf");
    }
}
```

---

## Production Considerations

### Accessibility Compliance

For Section 508 and PDF/UA compliance:

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PdfA = true;  // Creates tagged PDF

// Ensure HTML has proper structure
var accessibleHtml = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <title>Monthly Report</title>
</head>
<body>
    <main>
        <h1>Monthly Report</h1>
        <p>Content here...</p>
        <img src='chart.png' alt='Sales chart showing 20% growth' />
    </main>
</body>
</html>";
```

### Error Handling

```csharp
[HttpGet("{id}/pdf")]
public async Task<IActionResult> GetPdf(int id)
{
    try
    {
        var html = await GenerateHtmlAsync(id);
        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
        return File(pdf.BinaryData, "application/pdf");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "PDF generation failed for {Id}", id);
        return StatusCode(500, "PDF generation failed. Please try again.");
    }
}
```

### Caching Generated PDFs

```csharp
public class PdfCacheService
{
    private readonly IDistributedCache _cache;

    public async Task<byte[]> GetOrGeneratePdf(string key, Func<Task<byte[]>> generator)
    {
        var cached = await _cache.GetAsync(key);
        if (cached != null) return cached;

        var pdf = await generator();

        await _cache.SetAsync(key, pdf, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        });

        return pdf;
    }
}
```

---

## Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install IronPDF Linux dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus libc6-dev libx11-xcb1 libxcomposite1 \
    libxcursor1 libxdamage1 libxi6 libxtst6 libnss3 \
    libcups2 libxss1 libxrandr2 libasound2 libatk1.0-0 \
    libatk-bridge2.0-0 libpangocairo-1.0-0 libgtk-3-0 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

### Azure App Service

IronPDF works on Azure App Service (Windows and Linux). For Linux, ensure you're using a P1v2 or higher plan for sufficient memory.

### AWS Lambda

```csharp
// Configure for Lambda
Installation.LinuxAndDockerDependenciesAutoConfig = true;
```

---

## Conclusion

ASP.NET Core PDF generation is straightforward with the right library:

1. **Use local Chromium rendering** for speed and compliance
2. **Leverage Razor views** for maintainable templates
3. **Apply print media queries** for PDF-specific styling
4. **Configure headers/footers** for professional output
5. **Enable accessibility** for Section 508/PDF/UA compliance

IronPDF handles all of this with a simple API that integrates naturally with ASP.NET Core.

---

## Related Tutorials

- **[Blazor Guide](blazor-pdf-generation.md)** — Blazor Server/WASM/Hybrid
- **[HTML to PDF](html-to-pdf-csharp.md)** — Complete HTML conversion
- **[Cross-Platform](cross-platform-pdf-dotnet.md)** — Azure/Docker/AWS deployment
- **[Best PDF Libraries 2025](best-pdf-libraries-dotnet-2025.md)** — Library comparison

### PDF Operations
- **[Merge & Split PDFs](merge-split-pdf-csharp.md)** — Document combination
- **[Digital Signatures](digital-signatures-pdf-csharp.md)** — Sign documents
- **[Watermarks](watermark-pdf-csharp.md)** — Branding and protection

### Library Guides
- **[IronPDF Guide](ironpdf/)** — Recommended for ASP.NET Core
- **[QuestPDF Guide](questpdf/)** — Code-first alternative

### Resources
- **[IronPDF ASP.NET Core Guide](https://ironpdf.com/how-to/aspnet-core-pdf/)** — Detailed documentation
- **[Razor to PDF Tutorial](https://ironpdf.com/how-to/razor-to-pdf/)** — Complete Razor integration
- **[Code Examples](https://ironpdf.com/examples/)** — 100+ working samples

### ❓ Related FAQs
- **[CSHTML to PDF (ASP.NET Core MVC)](FAQ/cshtml-to-pdf-aspnet-core-mvc.md)** — Server-side Razor rendering
- **[Razor View to PDF](FAQ/razor-view-to-pdf-csharp.md)** — MVC view conversion
- **[MVC View to PDF](FAQ/mvc-view-to-pdf-csharp.md)** — Controller integration
- **[Generate PDF Reports](FAQ/generate-pdf-reports-csharp.md)** — Report generation patterns
- **[Async PDF Generation](FAQ/async-pdf-generation-csharp.md)** — Non-blocking operations
- **[PDF Headers & Footers](FAQ/pdf-headers-footers-csharp.md)** — Page headers and footers
- **[IronPDF Azure Deployment](FAQ/ironpdf-azure-deployment-csharp.md)** — Cloud deployment guide

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared with 167 FAQ articles.*
