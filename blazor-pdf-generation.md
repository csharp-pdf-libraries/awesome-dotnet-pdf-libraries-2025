# Blazor PDF Generation: Server, WebAssembly, and MAUI Hybrid Guide

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF

[![Blazor](https://img.shields.io/badge/Blazor-Server%20%7C%20WASM%20%7C%20Hybrid-512BD4)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> Blazor developers need PDF generation for reports, invoices, and exports. This guide covers all Blazor hosting models—Server, WebAssembly, and MAUI Hybrid—with working examples.

---

## Table of Contents

1. [Blazor Hosting Models and PDF](#blazor-hosting-models-and-pdf)
2. [Quick Start](#quick-start)
3. [Blazor Server](#blazor-server)
4. [Blazor WebAssembly](#blazor-webassembly)
5. [Blazor MAUI Hybrid](#blazor-maui-hybrid)
6. [Render Razor Components to PDF](#render-razor-components-to-pdf)
7. [Download PDFs from Blazor](#download-pdfs-from-blazor)
8. [Common Patterns](#common-patterns)

---

## Blazor Hosting Models and PDF

| Hosting Model | PDF Generation Location | Approach |
|--------------|------------------------|----------|
| **Blazor Server** | Server-side | Direct IronPDF usage |
| **Blazor WebAssembly** | Server API | Call API endpoint |
| **Blazor MAUI Hybrid** | Device | Direct IronPDF usage |
| **Blazor United (.NET 8)** | Server | Direct or API |

**Key insight:** WebAssembly runs in browser and cannot use IronPDF directly—you need a server API.

---

## Quick Start

### Install IronPDF

```bash
dotnet add package IronPdf
```

### Basic PDF Generation

```csharp
using IronPdf;

var pdf = ChromePdfRenderer.RenderHtmlAsPdf("<h1>Hello from Blazor!</h1>");
pdf.SaveAs("hello.pdf");
```

---

## Blazor Server

Blazor Server runs on the server, so IronPDF works directly.

### Service Registration

```csharp
// Program.cs
builder.Services.AddScoped<IPdfService, PdfService>();
```

### PDF Service

```csharp
// Services/PdfService.cs
using IronPdf;

public interface IPdfService
{
    byte[] GeneratePdf(string html);
    byte[] GenerateInvoice(InvoiceModel invoice);
}

public class PdfService : IPdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        _renderer = new ChromePdfRenderer();
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.MarginTop = 20;
        _renderer.RenderingOptions.MarginBottom = 20;
    }

    public byte[] GeneratePdf(string html)
    {
        return _renderer.RenderHtmlAsPdf(html).BinaryData;
    }

    public byte[] GenerateInvoice(InvoiceModel invoice)
    {
        string html = BuildInvoiceHtml(invoice);
        return _renderer.RenderHtmlAsPdf(html).BinaryData;
    }

    private string BuildInvoiceHtml(InvoiceModel invoice)
    {
        return $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .header {{ display: flex; justify-content: space-between; }}
                .items {{ width: 100%; border-collapse: collapse; }}
                .items th, .items td {{ border: 1px solid #ddd; padding: 8px; }}
            </style>
        </head>
        <body>
            <div class='header'>
                <h1>Invoice #{invoice.Number}</h1>
                <p>Date: {invoice.Date:yyyy-MM-dd}</p>
            </div>
            <table class='items'>
                <tr><th>Description</th><th>Amount</th></tr>
                {string.Join("", invoice.Items.Select(i => $"<tr><td>{i.Description}</td><td>{i.Amount:C}</td></tr>"))}
            </table>
            <p><strong>Total: {invoice.Total:C}</strong></p>
        </body>
        </html>";
    }
}
```

### Blazor Component

```razor
@* Pages/InvoiceReport.razor *@
@page "/invoice/{InvoiceId:int}"
@inject IPdfService PdfService
@inject IJSRuntime JS

<h3>Invoice #@Invoice?.Number</h3>

<button class="btn btn-primary" @onclick="DownloadPdf">
    Download PDF
</button>

@code {
    [Parameter] public int InvoiceId { get; set; }
    private InvoiceModel Invoice { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Invoice = await LoadInvoice(InvoiceId);
    }

    private async Task DownloadPdf()
    {
        var pdfBytes = PdfService.GenerateInvoice(Invoice);

        // Trigger download via JavaScript
        await JS.InvokeVoidAsync("downloadFile",
            $"invoice-{Invoice.Number}.pdf",
            Convert.ToBase64String(pdfBytes));
    }
}
```

### JavaScript Download Helper

```javascript
// wwwroot/js/download.js
window.downloadFile = (fileName, base64Data) => {
    const link = document.createElement('a');
    link.href = 'data:application/pdf;base64,' + base64Data;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
```

---

## Blazor WebAssembly

WebAssembly runs in the browser—PDF generation must happen server-side.

### Server API Endpoint

```csharp
// Server/Controllers/PdfController.cs
[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    [HttpPost("generate")]
    public IActionResult GeneratePdf([FromBody] PdfRequest request)
    {
        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(request.Html);
        return File(pdf.BinaryData, "application/pdf", request.FileName);
    }

    [HttpGet("invoice/{id}")]
    public async Task<IActionResult> GetInvoicePdf(int id)
    {
        var invoice = await _invoiceService.GetById(id);
        string html = GenerateInvoiceHtml(invoice);

        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
        return File(pdf.BinaryData, "application/pdf", $"invoice-{id}.pdf");
    }
}
```

### Client Service

```csharp
// Client/Services/PdfClientService.cs
public class PdfClientService
{
    private readonly HttpClient _http;

    public PdfClientService(HttpClient http)
    {
        _http = http;
    }

    public async Task<byte[]> GeneratePdf(string html, string fileName)
    {
        var response = await _http.PostAsJsonAsync("api/pdf/generate", new
        {
            Html = html,
            FileName = fileName
        });

        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task DownloadInvoice(int invoiceId)
    {
        var response = await _http.GetAsync($"api/pdf/invoice/{invoiceId}");
        var bytes = await response.Content.ReadAsByteArrayAsync();

        // Trigger download
        await DownloadFileFromBytes($"invoice-{invoiceId}.pdf", bytes);
    }
}
```

### Client Component

```razor
@* Client/Pages/InvoiceDownload.razor *@
@page "/invoice/{InvoiceId:int}"
@inject PdfClientService PdfService
@inject IJSRuntime JS

<h3>Invoice</h3>

<button class="btn btn-primary" @onclick="DownloadPdf" disabled="@IsLoading">
    @if (IsLoading)
    {
        <span class="spinner-border spinner-border-sm"></span>
        <span>Generating...</span>
    }
    else
    {
        <span>Download PDF</span>
    }
</button>

@code {
    [Parameter] public int InvoiceId { get; set; }
    private bool IsLoading { get; set; }

    private async Task DownloadPdf()
    {
        IsLoading = true;
        try
        {
            await PdfService.DownloadInvoice(InvoiceId);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

---

## Blazor MAUI Hybrid

MAUI Hybrid apps run natively—IronPDF works directly on the device.

### PDF Service for MAUI

```csharp
// Services/MauiPdfService.cs
using IronPdf;

public class MauiPdfService
{
    public async Task<string> GenerateAndSavePdf(string html, string fileName)
    {
        // Configure for mobile
        IronPdf.Installation.ChromeGpuMode = IronPdf.Engines.Chrome.ChromeGpuModes.Disabled;

        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

        // Save to app-specific folder
        string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        pdf.SaveAs(filePath);

        return filePath;
    }

    public async Task SharePdf(string filePath)
    {
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Share PDF",
            File = new ShareFile(filePath)
        });
    }
}
```

### MAUI Blazor Component

```razor
@* Components/ReportGenerator.razor *@
@inject MauiPdfService PdfService

<button @onclick="GenerateAndShare">Generate Report</button>

@code {
    private async Task GenerateAndShare()
    {
        string html = @"
        <html>
        <body>
            <h1>Mobile Report</h1>
            <p>Generated on: " + DateTime.Now + @"</p>
        </body>
        </html>";

        string filePath = await PdfService.GenerateAndSavePdf(html, "report.pdf");
        await PdfService.SharePdf(filePath);
    }
}
```

---

## Render Razor Components to PDF

Convert existing Razor components to PDF without duplicating templates.

### Component to HTML Renderer

```csharp
// Services/RazorComponentRenderer.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

public class RazorComponentRenderer
{
    private readonly IServiceProvider _serviceProvider;

    public RazorComponentRenderer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<string> RenderComponentAsync<TComponent>(
        Dictionary<string, object> parameters = null) where TComponent : IComponent
    {
        await using var htmlRenderer = new HtmlRenderer(_serviceProvider, NullLoggerFactory.Instance);

        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameterView = parameters != null
                ? ParameterView.FromDictionary(parameters)
                : ParameterView.Empty;

            var output = await htmlRenderer.RenderComponentAsync<TComponent>(parameterView);
            return output.ToHtmlString();
        });

        return html;
    }
}
```

### Usage

```csharp
// Generate PDF from existing Razor component
var renderer = new RazorComponentRenderer(_serviceProvider);

string html = await renderer.RenderComponentAsync<InvoiceComponent>(new Dictionary<string, object>
{
    { "Invoice", invoiceModel }
});

var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
```

---

## Download PDFs from Blazor

### Using JavaScript Interop

```javascript
// wwwroot/js/blazor-pdf.js
window.blazorPdf = {
    downloadFromBase64: function(fileName, base64) {
        const link = document.createElement('a');
        link.href = 'data:application/pdf;base64,' + base64;
        link.download = fileName;
        link.click();
    },

    downloadFromUrl: function(url, fileName) {
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.click();
    },

    openInNewTab: function(base64) {
        const blob = this.base64ToBlob(base64, 'application/pdf');
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
    },

    base64ToBlob: function(base64, contentType) {
        const byteCharacters = atob(base64);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        return new Blob([byteArray], { type: contentType });
    }
};
```

### Blazor Component Wrapper

```razor
@* Shared/PdfDownloader.razor *@
@inject IJSRuntime JS

@code {
    public async Task DownloadPdf(byte[] pdfBytes, string fileName)
    {
        var base64 = Convert.ToBase64String(pdfBytes);
        await JS.InvokeVoidAsync("blazorPdf.downloadFromBase64", fileName, base64);
    }

    public async Task OpenPdfInNewTab(byte[] pdfBytes)
    {
        var base64 = Convert.ToBase64String(pdfBytes);
        await JS.InvokeVoidAsync("blazorPdf.openInNewTab", base64);
    }
}
```

---

## Common Patterns

### Report with Charts (Blazor Server)

```razor
@page "/sales-report"
@inject IPdfService PdfService
@inject IJSRuntime JS

<div id="chart-container">
    <ApexChart TItem="SalesData" ... />
</div>

<button @onclick="DownloadReport">Download PDF Report</button>

@code {
    private async Task DownloadReport()
    {
        // Wait for chart to render
        await Task.Delay(500);

        // Get chart as image via JavaScript
        var chartImage = await JS.InvokeAsync<string>("getChartAsBase64");

        string html = $@"
        <html>
        <body>
            <h1>Sales Report</h1>
            <img src='data:image/png;base64,{chartImage}' />
            <table>
                <!-- Sales data table -->
            </table>
        </body>
        </html>";

        var pdf = PdfService.GeneratePdf(html);
        await DownloadPdfBytes(pdf, "sales-report.pdf");
    }
}
```

### Multi-Page Report

```csharp
public byte[] GenerateMultiPageReport(List<ReportSection> sections)
{
    var html = new StringBuilder();
    html.Append("<html><head><style>");
    html.Append(".page-break { page-break-before: always; }");
    html.Append("</style></head><body>");

    for (int i = 0; i < sections.Count; i++)
    {
        if (i > 0) html.Append("<div class='page-break'></div>");
        html.Append($"<h1>{sections[i].Title}</h1>");
        html.Append(sections[i].Content);
    }

    html.Append("</body></html>");

    return ChromePdfRenderer.RenderHtmlAsPdf(html.ToString()).BinaryData;
}
```

---

## Recommendations

### For Blazor Server:
- ✅ Use IronPDF directly
- ✅ Simple service injection
- ✅ Full feature access

### For Blazor WebAssembly:
- ✅ Create server API endpoints
- ✅ Use HttpClient to call API
- ✅ Handle loading states

### For Blazor MAUI:
- ✅ Use IronPDF directly
- ✅ Disable GPU mode for mobile
- ✅ Use Share API for distribution

---

## Related Tutorials

- **[ASP.NET Core PDF](asp-net-core-pdf-reports.md)** — MVC/API PDF generation
- **[HTML to PDF](html-to-pdf-csharp.md)** — Comprehensive HTML guide
- **[Cross-Platform PDF](cross-platform-pdf-dotnet.md)** — Deployment guide
- **[Best PDF Libraries](best-pdf-libraries-dotnet-2025.md)** — Full comparison

---

### More Tutorials
- **[Beginner Tutorial](csharp-pdf-tutorial-beginners.md)** — First PDF in 5 minutes
- **[Merge & Split](merge-split-pdf-csharp.md)** — Document combination
- **[Digital Signatures](digital-signatures-pdf-csharp.md)** — Sign documents
- **[IronPDF Guide](ironpdf/)** — Full library documentation

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*
