# How Do I Generate PDFs in Blazor with C#?

Need to create PDFs from your Blazor apps—whether Blazor Server or WebAssembly? This FAQ covers the practical steps, code samples, and best practices for Blazor PDF generation using IronPDF, with tips on downloads, previews, styling, and more. Let’s answer the top developer questions!

---

## What’s the Difference Between Blazor Server and WebAssembly for PDF Generation?

Blazor Server and Blazor WebAssembly handle PDF generation very differently. In Blazor Server, your C# code runs on the server and can use libraries like [IronPDF](https://ironpdf.com) directly. In Blazor WebAssembly, you’re running inside the browser, so you can’t use native .NET libraries for PDF creation—you’ll need to call a backend API to do the heavy lifting.

**Key points:**
- **Blazor Server:** Use IronPDF directly in your components, synchronously or asynchronously.
- **Blazor WebAssembly:** Set up an API endpoint to generate the PDF server-side, then send the file to the browser.

Not sure which hosting model is best? See [How do I generate PDFs in .NET Core?](dotnet-core-pdf-generation-csharp.md)

---

## How Can I Generate and Download a PDF in Blazor Server?

In Blazor Server, PDF creation is straightforward. Here’s a sample setup:

```csharp
// Install-Package IronPdf
@page "/simple-pdf"
@using IronPdf
@inject IJSRuntime JS

<button @onclick="CreatePdf">Download PDF</button>

@code {
    private async Task CreatePdf()
    {
        var renderer = new ChromePdfRenderer();
        var pdfDoc = renderer.RenderHtmlAsPdf("<h2>Hello from Blazor Server</h2>");
        await JS.InvokeVoidAsync("downloadFile", "sample.pdf", pdfDoc.BinaryData);
    }
}
```

For the download, you’ll need a little JavaScript registered in your project:

```javascript
// wwwroot/js/download.js
window.downloadFile = (filename, bytes) => {
    const blob = new Blob([new Uint8Array(bytes)], { type: 'application/pdf' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
};
```

Don’t forget to reference this JS file in your HTML host page.

---

## How Do I Generate PDFs in Blazor WebAssembly?

Since IronPDF can’t run in the browser, you need a backend API. Here’s the typical approach:

### How Do I Set Up a PDF Generation API Endpoint?

Add an API controller to your ASP.NET Core backend:

```csharp
// Install-Package IronPdf
using IronPdf;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PdfGenController : ControllerBase
{
    [HttpPost("create")]
    public IActionResult Create([FromBody] PdfInput model)
    {
        var renderer = new ChromePdfRenderer();
        var html = $"<h1>{model.Title}</h1><p>{model.Text}</p>";
        var pdf = renderer.RenderHtmlAsPdf(html);
        return File(pdf.BinaryData, "application/pdf", "report.pdf");
    }
}
public class PdfInput
{
    public string Title { get; set; }
    public string Text { get; set; }
}
```

### How Does the Blazor WASM Client Consume the PDF API?

```csharp
@inject HttpClient Http
@inject IJSRuntime JS

<input @bind="title" placeholder="Title" />
<textarea @bind="content" placeholder="Content"></textarea>
<button @onclick="DownloadWasmPdf">Get PDF</button>

@code {
    string title = "Blazor WASM PDF";
    string content = "PDFs generated via API.";

    private async Task DownloadWasmPdf()
    {
        var req = new { Title = title, Text = content };
        var resp = await Http.PostAsJsonAsync("api/pdfgen/create", req);
        if (resp.IsSuccessStatusCode)
        {
            var bytes = await resp.Content.ReadAsByteArrayAsync();
            await JS.InvokeVoidAsync("downloadFile", "wasm-output.pdf", bytes);
        }
    }
}
```

For more on async workflows, check out [How do I generate PDFs asynchronously?](async-pdf-generation-csharp.md)

---

## Can I Render a Razor Component as a PDF?

Absolutely—but you’ll need to render the component to HTML first on the server. Here’s a reusable service pattern:

```csharp
// Install-Package IronPdf
using IronPdf;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

public class BlazorComponentPdfService
{
    private readonly HtmlRenderer _renderer;

    public BlazorComponentPdfService(IServiceProvider services)
    {
        _renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
    }

    public async Task<byte[]> RenderComponentToPdf<TComponent>(Dictionary<string, object> parameters = null)
        where TComponent : IComponent
    {
        var html = await _renderer.Dispatcher.InvokeAsync(async () =>
            (await _renderer.RenderComponentAsync<TComponent>(
                ParameterView.FromDictionary(parameters ?? new()))).ToHtmlString()
        );

        var pdf = new ChromePdfRenderer().RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

Register and inject your service, then call it with your component and parameters.

For working with the PDF DOM or adding advanced manipulation, see [How can I access the PDF DOM in C#?](access-pdf-dom-object-csharp.md)

---

## How Do I Build Invoices, Tables, or Styled PDFs in Blazor?

Design your data models (e.g., Invoice), then use string interpolation to generate HTML with tables, custom CSS, and more. Render that HTML to PDF with IronPDF:

```csharp
// Install-Package IronPdf
using IronPdf;

public byte[] CreateInvoicePdf(Invoice invoice)
{
    var rows = string.Join("", invoice.Items.Select(i => $"<tr><td>{i.Description}</td><td>{i.Quantity}</td><td>{i.UnitPrice:C}</td><td>{i.Total:C}</td></tr>"));
    var html = $@"
        <h1>Invoice #{invoice.Number}</h1>
        <p>Date: {invoice.Date:d}</p>
        <table>
            <tr><th>Item</th><th>Qty</th><th>Unit Price</th><th>Total</th></tr>
            {rows}
            <tr><td colspan='3'>Total</td><td>{invoice.Total:C}</td></tr>
        </table>";
    return new ChromePdfRenderer().RenderHtmlAsPdf(html).BinaryData;
}
```

Need more on PDF features or comparing libraries? See [Which C# HTML to PDF library should I use?](csharp-html-to-pdf-comparison.md)

---

## How Do I Add Headers, Footers, and Branding to PDFs?

Set the `HtmlHeader` and `HtmlFooter` properties on your renderer for logos and page numbers:

```csharp
// Install-Package IronPdf
using IronPdf;

public byte[] AddBranding(string html)
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center;'><img src='https://cdn.com/logo.png' height='20'/> MyBrand</div>",
        DrawDividerLine = true
    };
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
    };
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

For more advanced styling tips, consult the [IronPDF documentation](https://ironpdf.com/blog/videos/how-to-generate-pdf-files-in-dotnet-core-using-ironpdf/).

---

## How Can I Generate Large PDFs Without Freezing the UI?

Move generation to a background thread to keep your Blazor UI responsive:

```csharp
// Install-Package IronPdf
@inject IJSRuntime JS

<button @onclick="CreateLargePdf" disabled="@generating">
    @(generating ? "Working..." : "Download Large PDF")
</button>

@code {
    bool generating = false;
    private async Task CreateLargePdf()
    {
        generating = true;
        StateHasChanged();
        try
        {
            var pdfData = await Task.Run(() =>
            {
                var renderer = new ChromePdfRenderer();
                return renderer.RenderHtmlAsPdf(BuildBigHtml()).BinaryData;
            });
            await JS.InvokeVoidAsync("downloadFile", "large.pdf", pdfData);
        }
        finally
        {
            generating = false;
            StateHasChanged();
        }
    }
}
```

For more on async approaches, revisit [How do I generate PDFs asynchronously?](async-pdf-generation-csharp.md)

---

## How Can I Preview a PDF in the Browser Before Downloading?

Convert the PDF to a base64 data URL and display it in an `<iframe>`:

```csharp
// Install-Package IronPdf
@inject IJSRuntime JS

@if (!string.IsNullOrEmpty(pdfUrl))
{
    <iframe src="@pdfUrl" style="width:100%;height:600px;"></iframe>
    <button @onclick="DownloadPdf">Download PDF</button>
}

@code {
    string pdfUrl;
    byte[] pdfBytes;

    private async Task Preview()
    {
        var pdf = new ChromePdfRenderer().RenderHtmlAsPdf("<h2>Preview</h2>");
        pdfBytes = pdf.BinaryData;
        pdfUrl = $"data:application/pdf;base64,{Convert.ToBase64String(pdfBytes)}";
    }

    private async Task DownloadPdf()
    {
        await JS.InvokeVoidAsync("downloadFile", "preview.pdf", pdfBytes);
    }
}
```

---

## What Are Common Pitfalls When Generating PDFs in Blazor?

- **Blazor WASM can’t generate PDFs directly:** Always route PDF creation through a backend API.
- **Blank PDFs or missing styles:** Use inline CSS and ensure all referenced resources (like images) are accessible via absolute URLs.
- **CORS errors:** Make sure your API allows requests from your Blazor client.
- **Slow UI with large PDFs:** Offload the work to a background thread.
- **Images not appearing:** Check that your server can access the remote image URLs.

---

## Where Can I Learn More About PDF Generation with .NET?

Check the [IronPDF documentation](https://ironpdf.com/blog/videos/how-to-generate-html-to-pdf-with-dotnet-on-azure-pdf/) for deep dives and more advanced samples. Also, explore [Iron Software](https://ironsoftware.com) for other document processing tools.

For more on handling lists in Python, see [How do I find elements in a Python list?](python-find-in-list.md)

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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/), CTO of Iron Software, brings 41 years of coding expertise to PDF technology. As creator of IronPDF (10+ million+ downloads), he leads innovation in HTML-to-PDF conversion and document processing. Believes the best APIs don't need a manual. Connect: [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/)
