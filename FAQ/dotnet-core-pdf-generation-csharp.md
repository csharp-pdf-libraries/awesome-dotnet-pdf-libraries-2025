# How Can I Reliably Generate PDFs in .NET Core? Real-World Patterns, Libraries, and Code That Works

If you need to create PDFs in .NET Core—whether it's for invoices, reports, receipts, or compliance documents—you'll quickly realize that .NET Core doesn't provide built-in PDF generation. You're left to choose from a range of third-party libraries, each with its quirks. As a developer who's tried them all, I'll walk you through what actually works, why it works, and how to avoid common pitfalls. This FAQ gives you the practical patterns, code, and honest advice you need to ship PDF features that actually work.

---

## Why Is PDF Generation Tricky in .NET Core?

Most business applications eventually require PDF output. Whether it's for downloadable invoices, official receipts, or client reports, PDFs are the standard. However, .NET Core doesn't provide native support for PDF creation. Instead, developers must pick a PDF library, learn its API, and figure out how to integrate it into their workflow—often while dealing with formatting issues, deployment quirks, and support tickets when output doesn't look right. That's why library selection and setup matter so much for .NET developers.

---

## Which PDF Libraries Are Available for .NET Core, and What Are Their Strengths?

There are several popular PDF libraries in the .NET ecosystem, each with distinct advantages and trade-offs.

### When Should I Use iTextSharp or iText7?

iTextSharp (and its successor, iText7) excels at manipulating existing PDFs—think merging, splitting, annotating, or encrypting files. It's powerful for advanced PDF operations. However, generating PDFs from HTML is cumbersome with iTextSharp, and the HTML-to-PDF functionality is either an add-on or requires a commercial license. You'll end up writing lots of code to position elements, and the API isn't the friendliest for HTML-based workflows.

**Typical use:** Only when you need to modify or process existing PDFs, not for HTML-to-PDF conversions.

### Is wkhtmltopdf Still a Good Solution?

Wrappers like DinkToPdf and NReco.PdfGenerator bundle the old wkhtmltopdf command-line tool, which converts [HTML to PDF](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/) using an ancient version of WebKit. While it can handle simple HTML to PDF conversions, the project is no longer maintained, doesn't support modern CSS or JavaScript, and is challenging to deploy cross-platform (especially in Docker). Security updates are also lacking.

**Advice:** If you already have it working, fine—but migrating to a maintained library is a safer long-term bet.

### What Is PDFSharp Best For?

PDFSharp is lightweight and open source, letting you generate PDFs by specifying X/Y positions for every element. It doesn't support HTML at all. If your documents are simple—like barcodes, shipping labels, or forms with fixed layouts—it works. For anything visually complex or with dynamic layouts, it's tedious.

**Recommendation:** Use it only for simple, strictly coded layouts where full control of positioning is required.

### Should I Consider QuestPDF?

QuestPDF offers a modern, fluent C# API for building complex layouts. It's more intuitive than PDFSharp but still relies on code-driven layout instead of HTML templates. Designers can't easily contribute, and HTML-to-PDF isn't possible.

**Best fit:** Dynamic reports with layouts driven entirely by data, managed by developers.

### Why Choose IronPDF for Modern HTML-to-PDF Needs?

IronPDF wraps the Chromium rendering engine (the same one in Chrome), providing [pixel-perfect HTML/CSS/JavaScript to PDF conversion](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/). It offers a .NET-friendly API, works across Windows, Linux, macOS, and Docker, doesn't require external processes, and supports async operations.

**Pros:**
- Modern HTML/CSS/JS rendering—if it looks right in Chrome, it looks right in your PDF.
- Simple [NuGet install](https://www.nuget.org/packages/IronPdf/), no native setup.
- Great for production workloads, good documentation, and responsive support.
- [Async PDF generation](async-pdf-generation-csharp.md) support.

**Cons:**
- Requires a commercial license for production (trial adds a [watermark](https://ironpdf.com/java/blog/using-ironpdf-for-java/java-watermark-pdf-tutorial/)).
- Doesn't match iTextSharp for ultra-advanced manipulation features.

**Use it for:** Any scenario where you want your PDFs to mirror your HTML output, especially in production systems.

---

## How Do I Set Up IronPDF in My .NET Core Project?

Getting started with IronPDF is straightforward.

### What Are the Steps to Install IronPDF via NuGet?

IronPDF supports .NET Core 3.1 to .NET 10 (and beyond, thanks to .NET Standard 2.0 targeting). Add it to your project via NuGet Package Manager:

```powershell
Install-Package IronPdf
```

Or, from the command line:

```bash
dotnet add package IronPdf
```

No need to install Chromium or worry about native dependencies—the NuGet package handles it.

### How Do I Generate a Basic PDF Using IronPDF?

Here's a simple example to create a PDF with some HTML content:

```csharp
using IronPdf; // Install-Package IronPdf

var pdfRenderer = new ChromePdfRenderer();
string html = "<h1>Welcome to IronPDF!</h1><p>This is your first PDF.</p>";
var pdfDoc = pdfRenderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("example.pdf");
```

You'll get a file named `example.pdf` that matches the HTML output as seen in Chrome.

### Can I Generate Dynamic Invoices or Documents?

Absolutely. IronPDF works well with dynamic data—just build your HTML template using variables and render it:

```csharp
using IronPdf; // Install-Package IronPdf

int invoiceId = 1011;
string client = "Globex Inc.";
decimal totalAmount = 789.00m;

string htmlTemplate = $@"
<html>
<head>
  <style>body {{ font-family: Arial; }}</style>
</head>
<body>
  <h2>Invoice #{invoiceId}</h2>
  <p>Client: {client}</p>
  <p>Total Due: ${totalAmount:N2}</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlTemplate);
pdf.SaveAs($"invoice_{invoiceId}.pdf");
```

You can use any CSS, images, or even JavaScript charts in your templates.

---

## Why Use HTML Templates For PDF Generation?

### What Are the Benefits of HTML over Code-Driven Layout?

Designing documents with HTML/CSS means:
- Designers can help with layout and styling.
- You can preview and tweak in your browser.
- Modern web technologies—Google Fonts, Flexbox, charts—are available.
- It's less error-prone than specifying coordinates in code.

Typically, you:
1. Build the layout in HTML/CSS.
2. Insert dynamic values using interpolation or a view engine.
3. Render the result to PDF using IronPDF.

This approach is much easier to maintain and update.

### How Can I Inject Dynamic Data or Use Templates?

There are several templating strategies:
- Simple string interpolation for small documents.
- Razor views for complex layouts (especially in ASP.NET Core).
- Third-party engines like Handlebars or Mustache.

For [Blazor PDF generation](blazor-pdf-generation-csharp.md), similar strategies apply—just generate HTML from your Blazor component and convert it to PDF.

### Can You Show a Dynamic Invoice Example?

Sure! Here's how to generate an invoice with multiple line items:

```csharp
using IronPdf; // Install-Package IronPdf

var invoice = new
{
    Number = 2024,
    Client = "Acme Corp",
    Items = new[] { ("Design", 500m), ("Hosting", 150m), ("Support", 125m) }
};

var itemRows = string.Join("", invoice.Items.Select(i =>
    $"<tr><td>{i.Item1}</td><td style='text-align:right;'>${i.Item2:N2}</td></tr>"
));

var html = $@"
<html>
<head>
    <style>
        body {{ font-family: Arial; margin: 40px; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th, td {{ border-bottom: 1px solid #eee; padding: 8px; }}
        .total {{ font-weight: bold; }}
    </style>
</head>
<body>
    <h1>Invoice #{invoice.Number}</h1>
    <p>Client: {invoice.Client}</p>
    <table>
        <tr><th>Service</th><th>Price</th></tr>
        {itemRows}
        <tr><td class='total'>Total</td><td class='total' style='text-align:right;'>${invoice.Items.Sum(i => i.Item2):N2}</td></tr>
    </table>
</body>
</html>
";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs($"invoice_{invoice.Number}.pdf");
```

---

## How Can I Convert URLs, Razor Views, or ASPX Pages to PDF?

### Is It Possible to Convert a Live Web Page to PDF?

Yes, IronPDF can render any accessible URL to PDF:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com/dashboard");
pdf.SaveAs("dashboard.pdf");
```

This is useful for archiving dashboards, reports, or any page with dynamic content.

### How Do I Handle JavaScript or Dynamic Content That Loads After the Page?

For pages that load data with JavaScript, you might need to wait before rendering. IronPDF provides mechanisms for this:

**Fixed delay:**
```csharp
renderer.RenderingOptions.WaitFor.RenderDelay(3000); // Wait 3 seconds
var pdf = renderer.RenderUrlAsPdf("https://someapp.com/interactive-report");
```

**Wait for a specific element:**
On the page, add a marker when content is ready:
```js
document.body.insertAdjacentHTML('beforeend', '<div id="ready"></div>');
```
Then, in C#:
```csharp
renderer.RenderingOptions.WaitFor.HtmlElement("#ready");
var pdf = renderer.RenderUrlAsPdf("https://someapp.com/interactive-report");
```

### How Can I Render Razor Views or ASPX Pages to PDF?

For Razor views in ASP.NET Core:
1. Render the Razor view to a string.
2. Send that HTML to IronPDF.

**Example in a controller:**
```csharp
using IronPdf; // Install-Package IronPdf

public async Task<IActionResult> DownloadInvoice(int id)
{
    var invoice = GetInvoiceById(id);
    var html = await RenderViewToStringAsync("InvoiceTemplate", invoice); // custom helper
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    return File(pdf.BinaryData, "application/pdf", $"invoice-{id}.pdf");
}
```
For full details, see the [ASPX to PDF in C# FAQ](aspx-to-pdf-csharp.md) and [Razor-to-PDF guide](https://ironpdf.com/how-to/cshtml-to-pdf-razor-headlessly/).

---

## How Do I Deploy IronPDF in Docker, Linux, and the Cloud?

### What Should I Know About Running IronPDF in Containers or Linux?

IronPDF works cross-platform, but Chromium requires a few extra native libraries on Linux. Here's a Dockerfile that covers most scenarios:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    fonts-dejavu \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApp/MyApp.csproj", "MyApp/"]
RUN dotnet restore "MyApp/MyApp.csproj"
COPY . .
WORKDIR "/src/MyApp"
RUN dotnet build "MyApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

If you see font issues, add more fonts to your container. For more on deploying or integrating with Blazor, see the [Blazor PDF Generation in C# FAQ](blazor-pdf-generation-csharp.md).

### How Does IronPDF Work With Cloud Platforms Like Azure or AWS?

- **Azure App Service (Linux):** Works out of the box.
- **AWS ECS, GCP, Kubernetes:** Use the Dockerfile above.
- **Azure Functions or AWS Lambda:** Prefer container-based deployments for the best results.

Check the [IronPDF docs](https://ironpdf.com/blog/using-ironpdf/dotnet-core-generate-pdf/) for more deployment recipes and troubleshooting.

---

## How Efficient Is IronPDF, and Does It Support Async and Batch Processing?

### What Performance Should I Expect?

IronPDF typically generates a PDF in 100–300ms for standard documents. Heavier content (lots of images, charts, or JS) may take up to a second. This is considerably faster and more consistent than browser automation tools.

### Can IronPDF Generate PDFs Asynchronously or in Bulk?

Yes, IronPDF supports async APIs, making it easy to process documents in parallel. This is essential when generating hundreds or thousands of PDFs.

**Example:**
```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var reports = FetchReports(); // returns List<Report>
var tasks = reports.Select(async report =>
{
    string html = BuildHtml(report);
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    await pdf.SaveAsAsync($"report_{report.Id}.pdf");
    return pdf;
});
var pdfs = await Task.WhenAll(tasks);
```
For more advanced async patterns, see the [Async PDF Generation in C# FAQ](async-pdf-generation-csharp.md).

---

## What Common Issues Should I Watch Out For?

Here are a few "gotchas" and how to address them:

- **Missing fonts:** If PDFs look wrong on Linux, install the needed fonts in your container.
- **Broken images:** Use absolute URLs or embed images as data URIs. Ensure resources are accessible from the server.
- **JavaScript not loading:** Use `WaitFor.RenderDelay(ms)` or `WaitFor.HtmlElement(selector)` to wait for content.
- **Large PDFs:** Optimize HTML, compress images, or batch processing.
- **Deployment issues:** Most failures result from missing native libraries—double-check your Dockerfile or dependencies.
- **Licensing:** IronPDF requires a purchased license in production; the trial version adds a watermark.

To preview and debug output, save the HTML sent to IronPDF and open it in Chrome—if it looks off there, it will in your PDF too.

If you need to sanitize output, check the [Sanitize PDF in C# FAQ](sanitize-pdf-csharp.md).

---

## What Are the Most Useful Code Patterns for PDF Generation in .NET Core?

Here's a quick summary of the patterns you'll use most:

| Task                | Code Example                                            | Notes                                       |
|---------------------|--------------------------------------------------------|---------------------------------------------|
| Install IronPDF     | `dotnet add package IronPdf`                           | NuGet install                               |
| HTML to PDF         | `renderer.RenderHtmlAsPdf(html)`                       | Core use case                               |
| URL to PDF          | `renderer.RenderUrlAsPdf(url)`                         | Render live web pages                       |
| Razor to PDF        | Render to string, then `RenderHtmlAsPdf()`             | See [ASPX to PDF](aspx-to-pdf-csharp.md)    |
| Async generation    | `await renderer.RenderHtmlAsPdfAsync(html)`            | For batching/parallelism                    |
| Wait for JS         | `RenderingOptions.WaitFor.RenderDelay(ms)`             | Wait for dynamic content                    |
| Wait for element    | `RenderingOptions.WaitFor.HtmlElement("#id")`          | For SPA dashboards, etc.                    |
| Save PDF            | `pdf.SaveAs("file.pdf")`                               | Save to disk                                |
| Save PDF (async)    | `await pdf.SaveAsAsync("file.pdf")`                    | Async save                                  |

For advanced viewing and annotation, check the [Dotnet PDF Viewer in C# FAQ](dotnet-pdf-viewer-csharp.md).

---

## What’s the Bottom Line and Where Can I Learn More?

To summarize:
- Use HTML or Razor templates for easy, maintainable document generation.
- IronPDF produces browser-accurate PDFs with minimal fuss.
- Deploying in Docker/Linux is straightforward if you add needed fonts and libraries.
- Async and batch workflows are supported out of the box.

For comprehensive guides and updates, visit [IronPDF](https://ironpdf.com) and [Iron Software](https://ironsoftware.com). If you have unique use cases or run into issues, the documentation and community are very responsive.
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

**Jacob Mellor** — Creator of IronPDF. C# enthusiast, Visual Studio lover, and usability-driven API designer. Connect on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
