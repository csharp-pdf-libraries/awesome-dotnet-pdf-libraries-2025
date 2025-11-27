# C# HTML to PDF: The Complete Developer Guide (2025)

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF | 41 years coding experience

[![.NET](https://img.shields.io/badge/.NET-6%2B-512BD4)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/badge/NuGet-IronPdf-blue)](https://www.nuget.org/packages/IronPdf/)
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> Converting HTML to PDF in C# is one of the most common document generation tasks in .NET development. This comprehensive guide covers everything from basic conversions to enterprise-scale implementations, drawing on my 25+ years of experience building document processing solutions.

---

## Table of Contents

1. [Why HTML to PDF Matters](#why-html-to-pdf-matters)
2. [The Rendering Engine Problem](#the-rendering-engine-problem)
3. [Quick Start: 3 Lines of Code](#quick-start-3-lines-of-code)
4. [Understanding HTML-to-PDF Approaches](#understanding-html-to-pdf-approaches)
5. [The Chromium Advantage](#the-chromium-advantage)
6. [Complete Code Examples](#complete-code-examples)
7. [CSS Rendering: The Bootstrap Test](#css-rendering-the-bootstrap-test)
8. [JavaScript Execution](#javascript-execution)
9. [Headers, Footers, and Page Numbers](#headers-footers-and-page-numbers)
10. [Cross-Platform Deployment](#cross-platform-deployment)
11. [Performance Optimization](#performance-optimization)
12. [Common Pitfalls and Solutions](#common-pitfalls-and-solutions)
13. [Enterprise Considerations](#enterprise-considerations)
14. [Library Comparison](#library-comparison)
15. [Conclusion](#conclusion)

---

## Why HTML to PDF Matters

In my 41 years of coding—starting at age 6 on a BBC Micro—I've watched document generation evolve from printer control codes to PostScript to today's HTML-first approach. The shift to HTML-to-PDF represents one of the most significant productivity gains in enterprise software development.

### The Business Case

Every modern application eventually needs to generate documents:

- **Invoices and receipts** — e-commerce, SaaS billing, financial systems
- **Reports and dashboards** — analytics, business intelligence, compliance
- **Contracts and legal documents** — HR systems, CRM, legal tech
- **Certificates and credentials** — education, training, certifications
- **Customer communications** — statements, confirmations, notifications

The question isn't *whether* you'll need PDF generation, but *how* you'll implement it.

### Why HTML?

Here's a truth I've learned building tools used by NASA, Tesla, and Fortune 500 companies: **developers already know HTML and CSS**. This isn't a trivial observation—it's the foundation of modern document generation.

Consider the alternatives:

| Approach | Learning Curve | Design Flexibility | Maintenance |
|----------|---------------|-------------------|-------------|
| **HTML/CSS** | Near zero (existing skills) | Unlimited | Easy (web standards) |
| Programmatic (coordinates) | High (manual positioning) | Limited | Difficult |
| Template languages | Medium (new syntax) | Moderate | Variable |
| WYSIWYG designers | Low (drag-drop) | Limited | Vendor lock-in |

When your designers create mockups in Figma, those designs export to HTML/CSS. When your front-end team builds React components, those render to HTML. **HTML-to-PDF lets you leverage existing work instead of recreating it.**

### The Real-World Impact

At Iron Software, we've helped over 50,000 developers generate billions of PDFs. The patterns are consistent:

1. Teams start with HTML they already have
2. They need pixel-perfect rendering of modern CSS
3. They need to scale from development to production seamlessly
4. They need the PDF to look identical across all platforms

This guide addresses all four requirements.

---

## The Rendering Engine Problem

Before we write any code, you need to understand the fundamental technical challenge: **HTML rendering engines are complex beasts, and most PDF libraries don't actually render HTML—they parse it.**

### The Difference Between Parsing and Rendering

Consider this simple HTML:

```html
<div style="display: flex; justify-content: space-between;">
  <div>Left</div>
  <div>Right</div>
</div>
```

A **parser** reads this HTML and creates a document structure. But without a full CSS layout engine, it doesn't know that:
- `display: flex` creates a flex container
- `justify-content: space-between` places items at opposite ends
- The available width determines the actual positions

A **renderer** (like Chrome) executes the CSS specification, calculates layouts, handles fonts, applies styles, and produces a visual result.

**This is why most "HTML-to-PDF" libraries produce disappointing results with modern CSS.** They parse HTML tags but don't render modern layouts.

### What Actually Renders HTML?

Only a handful of engines actually render HTML like a browser:

| Engine | Used By | CSS3 Support | JavaScript |
|--------|---------|--------------|------------|
| **Chromium/Blink** | Chrome, Edge, IronPDF, Puppeteer | ✅ Full | ✅ Full |
| WebKit | Safari, old wkhtmltopdf | ⚠️ Partial | ⚠️ Limited |
| Flying Saucer | Aspose, iText pdfHTML | ❌ CSS 2.1 only | ❌ None |
| Custom | PDFSharp, QuestPDF | ❌ Minimal | ❌ None |

**If a library doesn't use Chromium, it cannot render modern websites correctly.** This isn't a limitation that can be patched—it's architectural.

---

## Quick Start: 3 Lines of Code

Let's start with working code. Install IronPDF via NuGet:

```bash
Install-Package IronPdf
```

Or via .NET CLI:

```bash
dotnet add package IronPdf
```

Now convert HTML to PDF in three lines:

```csharp
using IronPdf;

// Create a PDF from HTML string
var pdf = ChromePdfRenderer.RenderHtmlAsPdf("<h1>Hello, World!</h1>");
pdf.SaveAs("hello.pdf");
```

That's it. The PDF is created with full Chromium rendering.

### From a URL

```csharp
using IronPdf;

// Render any website as PDF
var pdf = ChromePdfRenderer.RenderUrlAsPdf("https://getbootstrap.com/");
pdf.SaveAs("bootstrap.pdf");
```

### From an HTML File

```csharp
using IronPdf;

// Render a local HTML file
var pdf = ChromePdfRenderer.RenderHtmlFileAsPdf("invoice-template.html");
pdf.SaveAs("invoice.pdf");
```

### Why This Matters

I've spent years obsessing over API design. As I often say: *"If it doesn't show up cleanly in IntelliSense inside Visual Studio, it's not done yet."*

Compare this to other libraries that require 15-50+ lines of boilerplate, configuration objects, stream management, and resource disposal. The IronPDF approach embodies the philosophy that **tools should feel like a seamless part of the C# language itself**.

---

## Understanding HTML-to-PDF Approaches

Not all HTML-to-PDF solutions are created equal. Here's a taxonomy:

### 1. Chromium-Based (Browser Rendering)

These libraries use a real browser engine to render HTML:

- **IronPDF** — Embedded Chromium with managed .NET API
- **PuppeteerSharp** — Browser automation (requires external Chrome)
- **Playwright** — Microsoft's browser automation (multi-browser)

**Pros:** Perfect rendering, full JavaScript, modern CSS
**Cons:** Larger deployment footprint (Chromium binaries)

### 2. WebKit-Based (Legacy)

These use older WebKit engines:

- **wkhtmltopdf** — Qt WebKit (abandoned, CVEs)
- **DinkToPdf** — wkhtmltopdf wrapper
- **NReco** — wkhtmltopdf wrapper

**Pros:** Smaller footprint, fast for simple documents
**Cons:** No CSS Grid, poor Flexbox, no modern JavaScript, security vulnerabilities

### 3. Custom HTML Parsers

These parse HTML but don't render it:

- **Aspose.PDF** — Custom engine
- **iText pdfHTML** — Flying Saucer engine
- **PDFSharp + HtmlRenderer** — Basic CSS 2.0

**Pros:** No browser dependency
**Cons:** Fail on modern CSS, no JavaScript execution

### 4. Code-First (No HTML)

These generate PDFs programmatically without HTML:

- **QuestPDF** — Fluent C# API
- **PDFSharp/MigraDoc** — Coordinate-based drawing

**Pros:** Full control, no rendering complexity
**Cons:** Steep learning curve, no design reuse from web

### Which Approach for Your Project?

| If You Need... | Use |
|---------------|-----|
| Modern CSS (Flexbox, Grid, Bootstrap) | Chromium-based |
| Legacy HTML (tables, basic CSS) | WebKit or custom parser may work |
| Full JavaScript execution | Chromium-based only |
| Smallest deployment footprint | Code-first (QuestPDF) |
| Fastest development time | HTML with existing templates |

---

## The Chromium Advantage

Let me be direct: **if your HTML uses any CSS from the last decade, you need Chromium rendering.**

### What Chromium Handles That Others Don't

1. **CSS Flexbox** — The foundation of modern responsive design
2. **CSS Grid** — Two-dimensional layouts
3. **CSS Variables** — Custom properties
4. **Media Queries** — `@media print` stylesheets
5. **Web Fonts** — Google Fonts, custom typefaces
6. **JavaScript** — Dynamic content, frameworks
7. **Canvas/SVG** — Complex graphics
8. **Shadow DOM** — Web components

### IronPDF's Chromium Integration

When I architected IronPDF, the decision to embed Chromium wasn't made lightly. It adds complexity to the package. But it solves the fundamental problem: **identical rendering to Chrome**.

Key differentiators:

- **Bundled Chromium** — No separate browser installation needed
- **Managed lifecycle** — Automatic browser process management
- **Thread-safe** — Safe for concurrent PDF generation
- **Cross-platform** — Same behavior on Windows, Linux, macOS
- **Optimized memory** — Shared browser instance for batch operations

Compare to PuppeteerSharp, which requires you to:
1. Download Chromium separately (300MB+)
2. Manage browser processes manually
3. Handle cleanup and resource disposal
4. Deal with memory leaks under load

---

## Complete Code Examples

### Basic HTML String to PDF

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        // Your HTML content
        string html = @"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body { font-family: 'Segoe UI', Arial, sans-serif; }
                .header { background: #2c3e50; color: white; padding: 20px; }
                .content { padding: 20px; }
                .footer { background: #ecf0f1; padding: 10px; text-align: center; }
            </style>
        </head>
        <body>
            <div class='header'>
                <h1>Invoice #12345</h1>
            </div>
            <div class='content'>
                <p>Thank you for your purchase!</p>
                <table>
                    <tr><td>Item</td><td>$99.00</td></tr>
                    <tr><td>Tax</td><td>$8.91</td></tr>
                    <tr><td><strong>Total</strong></td><td><strong>$107.91</strong></td></tr>
                </table>
            </div>
            <div class='footer'>
                <p>Questions? Contact support@example.com</p>
            </div>
        </body>
        </html>";

        // Generate PDF
        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("invoice.pdf");

        Console.WriteLine("PDF created successfully!");
    }
}
```

### Modern Flexbox Layout

```csharp
using IronPdf;

// This layout ONLY works with Chromium rendering
string modernHtml = @"
<div style='display: flex; gap: 20px; padding: 20px;'>
    <div style='flex: 1; background: #3498db; color: white; padding: 20px; border-radius: 8px;'>
        <h2>Column 1</h2>
        <p>This uses CSS Flexbox with gap property.</p>
    </div>
    <div style='flex: 2; background: #2ecc71; color: white; padding: 20px; border-radius: 8px;'>
        <h2>Column 2</h2>
        <p>This column is twice as wide (flex: 2).</p>
    </div>
    <div style='flex: 1; background: #e74c3c; color: white; padding: 20px; border-radius: 8px;'>
        <h2>Column 3</h2>
        <p>Modern CSS that competitors can't render.</p>
    </div>
</div>";

var pdf = ChromePdfRenderer.RenderHtmlAsPdf(modernHtml);
pdf.SaveAs("flexbox-demo.pdf");
```

### CSS Grid Layout

```csharp
using IronPdf;

string gridHtml = @"
<style>
    .grid-container {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        grid-template-rows: auto;
        grid-template-areas:
            'header header header'
            'sidebar main main'
            'footer footer footer';
        gap: 10px;
        padding: 20px;
    }
    .header { grid-area: header; background: #34495e; color: white; padding: 20px; }
    .sidebar { grid-area: sidebar; background: #95a5a6; padding: 20px; }
    .main { grid-area: main; background: #bdc3c7; padding: 20px; min-height: 200px; }
    .footer { grid-area: footer; background: #7f8c8d; color: white; padding: 20px; }
</style>

<div class='grid-container'>
    <div class='header'><h1>Dashboard Header</h1></div>
    <div class='sidebar'><h3>Navigation</h3><ul><li>Home</li><li>Reports</li><li>Settings</li></ul></div>
    <div class='main'><h2>Main Content Area</h2><p>CSS Grid layout rendered perfectly.</p></div>
    <div class='footer'><p>© 2025 Your Company</p></div>
</div>";

var pdf = ChromePdfRenderer.RenderHtmlAsPdf(gridHtml);
pdf.SaveAs("grid-demo.pdf");
```

### External CSS and Web Fonts

```csharp
using IronPdf;

string htmlWithFonts = @"
<!DOCTYPE html>
<html>
<head>
    <link href='https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&display=swap' rel='stylesheet'>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
    <style>
        body { font-family: 'Roboto', sans-serif; }
    </style>
</head>
<body>
    <div class='container py-4'>
        <div class='row'>
            <div class='col-md-6'>
                <div class='card'>
                    <div class='card-header bg-primary text-white'>
                        Bootstrap Card
                    </div>
                    <div class='card-body'>
                        <p>This uses Bootstrap 5 with Google Fonts.</p>
                        <button class='btn btn-success'>Styled Button</button>
                    </div>
                </div>
            </div>
            <div class='col-md-6'>
                <div class='alert alert-info'>
                    All Bootstrap classes render correctly with Chromium.
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

var pdf = ChromePdfRenderer.RenderHtmlAsPdf(htmlWithFonts);
pdf.SaveAs("bootstrap-demo.pdf");
```

### ASP.NET Core Razor Views

```csharp
using IronPdf;
using Microsoft.AspNetCore.Mvc;

public class ReportsController : Controller
{
    public IActionResult DownloadInvoice(int invoiceId)
    {
        // Get your data
        var invoice = _invoiceService.GetById(invoiceId);

        // Render Razor view to HTML
        var html = RenderViewToString("InvoiceTemplate", invoice);

        // Convert to PDF
        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

        // Return as file download
        return File(pdf.BinaryData, "application/pdf", $"invoice-{invoiceId}.pdf");
    }

    private string RenderViewToString(string viewName, object model)
    {
        // Implementation using IRazorViewEngine
        // See: https://ironpdf.com/how-to/razor-to-pdf/
    }
}
```

### Batch Processing

```csharp
using IronPdf;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BatchProcessor
{
    public async Task GenerateReportsAsync(List<ReportData> reports)
    {
        // Create renderer once for efficiency
        var renderer = new ChromePdfRenderer();

        var tasks = reports.Select(async report =>
        {
            string html = GenerateReportHtml(report);
            var pdf = await Task.Run(() => renderer.RenderHtmlAsPdf(html));
            pdf.SaveAs($"reports/{report.Id}.pdf");
        });

        await Task.WhenAll(tasks);
    }

    private string GenerateReportHtml(ReportData report)
    {
        return $@"
        <html>
        <body>
            <h1>Report: {report.Title}</h1>
            <p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm}</p>
            <div>{report.Content}</div>
        </body>
        </html>";
    }
}
```

---

## CSS Rendering: The Bootstrap Test

I use the "Bootstrap Homepage Test" as the definitive measure of a library's HTML rendering capability. It's simple: can your library render https://getbootstrap.com/ identically to Chrome?

### Why Bootstrap?

Bootstrap uses:
- CSS Flexbox throughout
- CSS Grid for some layouts
- CSS custom properties (variables)
- Complex media queries
- Modern JavaScript
- SVG graphics
- Web fonts

**If a library can render Bootstrap, it can render anything.**

### Test Results (November 2025)

| Library | Bootstrap Test | Reason |
|---------|---------------|--------|
| **IronPDF** | ✅ PASS | Full Chromium |
| PuppeteerSharp | ✅ PASS | Full Chromium |
| Playwright | ✅ PASS | Full Chromium |
| Aspose.PDF | ❌ FAIL | [No Flexbox](https://forum.aspose.com/t/display-flex-not-working/203245) |
| iText pdfHTML | ❌ FAIL | [No JavaScript](https://kb.itextpdf.com/itext/evaluating-js-with-pdfhtml) |
| PDFSharp | ❌ FAIL | CSS 2.0 only |
| wkhtmltopdf | ❌ FAIL | Abandoned WebKit |
| SelectPdf | ⚠️ PARTIAL | Outdated Chromium |

### Run the Test Yourself

```csharp
using IronPdf;

// The Bootstrap Test
var pdf = ChromePdfRenderer.RenderUrlAsPdf("https://getbootstrap.com/");
pdf.SaveAs("bootstrap-test.pdf");

// Open both the website and PDF - they should be identical
```

---

## JavaScript Execution

Modern web pages rely on JavaScript for content rendering. Single-page applications (SPAs) built with React, Vue, or Angular won't render without JavaScript execution.

### How IronPDF Handles JavaScript

IronPDF's embedded Chromium executes JavaScript just like Chrome:

```csharp
using IronPdf;

string dynamicHtml = @"
<div id='content'>Loading...</div>
<script>
    // Simulated dynamic content
    setTimeout(() => {
        document.getElementById('content').innerHTML = `
            <h1>Dynamic Content</h1>
            <p>This was rendered by JavaScript at ${new Date().toLocaleString()}</p>
        `;
    }, 100);
</script>";

var renderer = new ChromePdfRenderer();

// Wait for JavaScript to complete
renderer.RenderingOptions.WaitFor.JavaScript(500); // Wait 500ms

var pdf = renderer.RenderHtmlAsPdf(dynamicHtml);
pdf.SaveAs("dynamic-content.pdf");
```

### Waiting for Specific Conditions

```csharp
var renderer = new ChromePdfRenderer();

// Wait for a specific element to appear
renderer.RenderingOptions.WaitFor.HtmlElementById("chart-loaded", 5000);

// Or wait for a custom JavaScript condition
renderer.RenderingOptions.WaitFor.JavaScript("window.chartReady === true", 10000);

var pdf = renderer.RenderUrlAsPdf("https://example.com/dashboard");
pdf.SaveAs("dashboard.pdf");
```

### React/Vue/Angular Applications

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// For SPA frameworks, wait for hydration
renderer.RenderingOptions.WaitFor.JavaScript(2000);

// Optionally wait for network idle
renderer.RenderingOptions.WaitFor.NetworkIdle(1000);

var pdf = renderer.RenderUrlAsPdf("https://your-react-app.com/report");
pdf.SaveAs("spa-report.pdf");
```

---

## Headers, Footers, and Page Numbers

Professional documents need headers, footers, and page numbering. IronPDF provides multiple approaches:

### HTML Headers and Footers

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Header with company logo
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = @"
        <div style='width: 100%; text-align: center; font-size: 12px; color: #666;'>
            <img src='https://your-company.com/logo.png' style='height: 30px;'>
            <span style='margin-left: 20px;'>Confidential Document</span>
        </div>",
    DrawDividerLine = true
};

// Footer with page numbers
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = @"
        <div style='width: 100%; text-align: center; font-size: 10px; color: #999;'>
            Page {page} of {total-pages} | Generated: {date} {time}
        </div>",
    DrawDividerLine = true
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Document Content</h1><p>Your content here...</p>");
pdf.SaveAs("with-headers.pdf");
```

### Available Placeholders

| Placeholder | Output |
|------------|--------|
| `{page}` | Current page number |
| `{total-pages}` | Total number of pages |
| `{date}` | Current date |
| `{time}` | Current time |
| `{html-title}` | Document's `<title>` element |
| `{pdf-title}` | PDF metadata title |
| `{url}` | Source URL (if applicable) |

### Text-Based Headers (Simpler Approach)

```csharp
var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.TextHeader = new TextHeaderFooter()
{
    CenterText = "Company Name",
    RightText = "{date}",
    FontSize = 12,
    FontFamily = "Arial"
};

renderer.RenderingOptions.TextFooter = new TextHeaderFooter()
{
    LeftText = "Confidential",
    CenterText = "Page {page} of {total-pages}",
    RightText = "v1.0",
    FontSize = 10
};
```

### First Page Different

```csharp
var renderer = new ChromePdfRenderer();

// First page header (cover page style)
renderer.RenderingOptions.FirstPageNumber = 0;  // Start counting from 0
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div id='header'></div>",  // Empty for first page
    DrawDividerLine = false
};

// Use CSS @page rules for first page
string html = @"
<style>
    @page :first {
        margin-top: 0;
    }
    @page {
        margin-top: 60px;
    }
</style>
<div class='cover'>
    <h1>Annual Report 2025</h1>
</div>
<div class='content'>
    <h2>Executive Summary</h2>
    <p>...</p>
</div>";
```

---

## Cross-Platform Deployment

One of IronPDF's core strengths is genuine cross-platform support. The same code runs on:

- Windows (x86, x64, ARM64)
- Linux (Ubuntu, Debian, CentOS, Alpine, Amazon Linux)
- macOS (Intel and Apple Silicon)
- Docker containers
- Azure App Service, Azure Functions
- AWS Lambda, AWS ECS
- iOS and Android (via gRPC)

### Docker Deployment

```dockerfile
# Use Microsoft's .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime image with dependencies
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install IronPDF dependencies for Linux
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libx11-xcb1 \
    libxcomposite1 \
    libxcursor1 \
    libxdamage1 \
    libxi6 \
    libxtst6 \
    libnss3 \
    libcups2 \
    libxss1 \
    libxrandr2 \
    libasound2 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libpangocairo-1.0-0 \
    libgtk-3-0 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

### Azure Functions

```csharp
using IronPdf;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

public class PdfFunction
{
    [Function("GeneratePdf")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        // IronPDF works in Azure Functions
        var html = await new StreamReader(req.Body).ReadToEndAsync();
        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/pdf");
        response.Headers.Add("Content-Disposition", "attachment; filename=document.pdf");
        await response.Body.WriteAsync(pdf.BinaryData);

        return response;
    }
}
```

### AWS Lambda

```csharp
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using IronPdf;

public class PdfHandler
{
    public APIGatewayProxyResponse GeneratePdf(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Configure for Lambda environment
        Installation.LinuxAndDockerDependenciesAutoConfig = true;

        var html = request.Body;
        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/pdf" },
                { "Content-Disposition", "attachment; filename=document.pdf" }
            },
            Body = Convert.ToBase64String(pdf.BinaryData),
            IsBase64Encoded = true
        };
    }
}
```

---

## Performance Optimization

When you're generating thousands of PDFs, performance matters. Here are optimization strategies:

### Reuse the Renderer

```csharp
// DON'T create a new renderer for each PDF
// DO reuse a single instance

public class PdfService
{
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        _renderer = new ChromePdfRenderer();
        ConfigureRenderer(_renderer);
    }

    private void ConfigureRenderer(ChromePdfRenderer renderer)
    {
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;
    }

    public PdfDocument GeneratePdf(string html)
    {
        return _renderer.RenderHtmlAsPdf(html);
    }
}
```

### Parallel Processing

```csharp
using System.Collections.Concurrent;

public class ParallelPdfGenerator
{
    public async Task<Dictionary<string, byte[]>> GenerateMultiplePdfsAsync(
        Dictionary<string, string> htmlDocuments,
        int maxDegreeOfParallelism = 4)
    {
        var results = new ConcurrentDictionary<string, byte[]>();

        await Parallel.ForEachAsync(
            htmlDocuments,
            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
            async (kvp, cancellationToken) =>
            {
                var pdf = ChromePdfRenderer.RenderHtmlAsPdf(kvp.Value);
                results[kvp.Key] = pdf.BinaryData;
            });

        return new Dictionary<string, byte[]>(results);
    }
}
```

### Disable Unnecessary Features

```csharp
var renderer = new ChromePdfRenderer();

// Disable features you don't need
renderer.RenderingOptions.EnableJavaScript = false;  // If no JS needed
renderer.RenderingOptions.RenderDelay = 0;            // No artificial delay

// Use print media type for cleaner output
renderer.RenderingOptions.CssMediaType = PdfCssMediaType.Print;
```

### Memory Management for Large Batches

```csharp
public void GenerateLargeBatch(IEnumerable<ReportData> reports)
{
    foreach (var batch in reports.Chunk(100))
    {
        using var renderer = new ChromePdfRenderer();

        foreach (var report in batch)
        {
            using var pdf = renderer.RenderHtmlAsPdf(GenerateHtml(report));
            pdf.SaveAs($"output/{report.Id}.pdf");
            // PDF is disposed after save, freeing memory
        }

        // Force garbage collection between batches if needed
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
```

---

## Common Pitfalls and Solutions

After helping thousands of developers, I've seen patterns in common issues:

### 1. Missing Fonts

**Problem:** PDFs show incorrect fonts or squares
**Solution:** Embed fonts or use web fonts

```csharp
// Use web fonts that are always available
string html = @"
<link href='https://fonts.googleapis.com/css2?family=Inter:wght@400;700&display=swap' rel='stylesheet'>
<style>
    body { font-family: 'Inter', sans-serif; }
</style>";

// Or embed fonts in base64
string html = $@"
<style>
    @font-face {{
        font-family: 'CustomFont';
        src: url('data:font/woff2;base64,{Convert.ToBase64String(fontBytes)}');
    }}
</style>";
```

### 2. Images Not Loading

**Problem:** Images appear broken in PDF
**Solution:** Use absolute URLs or base64 encoding

```csharp
// Convert relative paths to absolute
string html = html.Replace("src=\"/images/", $"src=\"{baseUrl}/images/");

// Or embed images as base64
byte[] imageBytes = File.ReadAllBytes("logo.png");
string base64 = Convert.ToBase64String(imageBytes);
string imgTag = $"<img src='data:image/png;base64,{base64}' />";
```

### 3. Page Breaks in Wrong Places

**Problem:** Content breaks awkwardly between pages
**Solution:** Use CSS print properties

```csharp
string html = @"
<style>
    .section { page-break-inside: avoid; }
    .chapter { page-break-before: always; }
    .keep-together { break-inside: avoid; }
    table { page-break-inside: avoid; }
    tr { page-break-inside: avoid; }
</style>";
```

### 4. Background Colors Not Printing

**Problem:** Background colors/images don't appear
**Solution:** Enable print backgrounds

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
```

### 5. Content Cut Off

**Problem:** Content extends beyond page margins
**Solution:** Configure margins and paper size

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 25;      // mm
renderer.RenderingOptions.MarginBottom = 25;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;

// Or use custom size
renderer.RenderingOptions.SetCustomPaperSize(210, 297);  // A4 in mm
```

### 6. JavaScript Not Executing

**Problem:** Dynamic content not rendered
**Solution:** Add render delay or wait conditions

```csharp
var renderer = new ChromePdfRenderer();

// Wait for JavaScript to complete
renderer.RenderingOptions.WaitFor.JavaScript(1000);

// Or wait for specific element
renderer.RenderingOptions.WaitFor.HtmlElementById("content-loaded");

// Or wait for network requests to complete
renderer.RenderingOptions.WaitFor.NetworkIdle(500);
```

---

## Enterprise Considerations

When deploying HTML-to-PDF in production, consider:

### Licensing

Different libraries have different licensing models:

| Library | Model | Risk |
|---------|-------|------|
| IronPDF | Perpetual or subscription | Clear commercial use |
| iText | AGPL or commercial | Must open-source or pay |
| Aspose | Subscription only | Ongoing cost |
| wkhtmltopdf | LGPL | Security vulnerabilities |

I always recommend understanding licensing before integration. Switching libraries later is painful.

### Security

- **wkhtmltopdf** has CVE-2022-35583 (SSRF, 9.8 severity) — UNPATCHED
- Never render untrusted HTML without sandboxing
- Consider input validation for URLs
- Use Content Security Policy headers in templates

### Scalability

For high-volume generation:

1. **Use a dedicated PDF service** — Separate from your main application
2. **Queue jobs** — Use message queues for asynchronous generation
3. **Cache templates** — Parse HTML templates once
4. **Monitor memory** — Chromium can be memory-intensive

### Support

When evaluating libraries, consider support options:

- Does the vendor respond quickly?
- Is there documentation for your use case?
- What's the community size (Stack Overflow answers)?

At Iron Software, we pride ourselves on response times. When you're stuck at 2 AM with a production issue, support matters.

---

## Library Comparison

### Quick Reference

| Feature | IronPDF | PuppeteerSharp | Aspose.PDF | iText7 |
|---------|---------|----------------|------------|--------|
| **Chromium rendering** | ✅ | ✅ | ❌ | ❌ |
| **Modern CSS (Flexbox/Grid)** | ✅ | ✅ | ❌ | ❌ |
| **JavaScript execution** | ✅ | ✅ | ❌ | ❌ |
| **PDF manipulation** | ✅ | ❌ | ✅ | ✅ |
| **Cross-platform** | ✅ | ✅ | ✅ | ✅ |
| **Deployment footprint** | ~100MB | ~300MB | ~50MB | ~30MB |
| **Learning curve** | Low | Medium | Medium | High |
| **Documentation quality** | Excellent | Good | Good | Good |
| **Free tier** | Trial | Yes | Trial | AGPL |
| **Commercial license** | From $749 | Free | $1,199/yr | Quote |

### When to Choose Each

**Choose IronPDF when:**
- You need modern CSS support (Flexbox, Grid, Bootstrap)
- You want the simplest API (3 lines of code)
- You need PDF manipulation (merge, split, secure)
- You're deploying to Azure, AWS, or Docker
- You want professional support

**Choose PuppeteerSharp when:**
- Budget is zero and you can accept limitations
- You're comfortable managing browser processes
- You only need generation, not manipulation
- You can handle 300MB+ deployment size

**Choose Aspose.PDF when:**
- Your HTML is simple (no modern CSS)
- You need extensive PDF manipulation
- Budget is not a constraint
- You're already in the Aspose ecosystem

**Choose iText when:**
- You're open-sourcing your application (AGPL)
- You need PDF form filling specifically
- Your HTML is simple or you'll use programmatic generation

---

## Conclusion

HTML-to-PDF conversion in C# has evolved significantly. The key insight is simple: **use Chromium-based rendering if your HTML uses any modern CSS**.

Here's my recommended decision tree:

1. **Does your HTML use Flexbox, Grid, or Bootstrap?** → You need Chromium (IronPDF, PuppeteerSharp)
2. **Do you need PDF manipulation (merge, split, secure)?** → IronPDF includes this; PuppeteerSharp doesn't
3. **Is deployment simplicity important?** → IronPDF bundles everything; PuppeteerSharp requires browser management
4. **Is budget zero?** → PuppeteerSharp is Apache-licensed (but has limitations)

For most production applications, I recommend starting with IronPDF:

```csharp
using IronPdf;

var pdf = ChromePdfRenderer.RenderHtmlAsPdf(yourHtml);
pdf.SaveAs("output.pdf");
```

That's the level of simplicity I've aimed for in 41 years of writing code. Tools should work for developers, not against them.

---

## Resources

- **[IronPDF Documentation](https://ironpdf.com/docs/)** — Complete API reference and guides
- **[Code Examples](https://ironpdf.com/examples/)** — 100+ working examples
- **[Tutorials](https://ironpdf.com/tutorials/)** — Step-by-step guides
- **[API Reference](https://ironpdf.com/object-reference/api/)** — Full class documentation
- **[Stack Overflow](https://stackoverflow.com/questions/tagged/ironpdf)** — Community Q&A

---

## About the Author

**[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** is the Chief Technology Officer at [Iron Software](https://ironsoftware.com/) and creator of [IronPDF](https://ironpdf.com/). With 41 years of coding experience starting at age 6, Jacob has built document processing solutions used by NASA, Tesla, and Fortune 500 companies. He holds a First-Class Honours degree in Civil Engineering from the University of Manchester and has spent 25+ years in commercial enterprise development.

*"Developer usability is the most underrated part of API design. You can have the most powerful code in the world, but if developers can't understand and get to 'Hello World' in 5 minutes, you've already lost."* — Jacob Mellor

Connect with Jacob:
- [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/)
- [Iron Software Author Page](https://ironsoftware.com/about-us/authors/jacobmellor/)
- [PDF Association](https://pdfa.org/people/jacob-mellor/)

---

## Related Tutorials

### Getting Started
- **[Beginner Tutorial](csharp-pdf-tutorial-beginners.md)** — Create your first PDF in 5 minutes
- **[Why C# for PDF Generation](why-csharp-pdf-generation.md)** — Language advantages for PDF development

### Choosing a Library
- **[Best PDF Libraries 2025](best-pdf-libraries-dotnet-2025.md)** — Comprehensive library comparison
- **[Decision Flowchart](choosing-a-pdf-library.md)** — 5 questions to find your library
- **[Free vs Paid Libraries](free-vs-paid-pdf-libraries.md)** — True cost analysis

### PDF Operations
- **[Merge & Split PDFs](merge-split-pdf-csharp.md)** — Combine generated PDFs
- **[Watermarks & Stamps](watermark-pdf-csharp.md)** — Add branding to HTML-generated PDFs
- **[Digital Signatures](digital-signatures-pdf-csharp.md)** — Sign generated documents
- **[PDF to Image](pdf-to-image-csharp.md)** — Convert PDFs to thumbnails

### Framework Integration
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web application PDF generation
- **[Blazor](blazor-pdf-generation.md)** — Server, WebAssembly, and MAUI Hybrid

### Compliance & Deployment
- **[PDF/A Compliance](pdf-a-compliance-csharp.md)** — Accessibility standards
- **[Cross-Platform Deployment](cross-platform-pdf-dotnet.md)** — Windows, Linux, Docker, Cloud

### Library-Specific Guides
- **[IronPDF](ironpdf/)** — Chromium-based rendering (recommended)
- **[PuppeteerSharp](puppeteersharp/)** — Browser automation approach
- **[Playwright](playwright/)** — Multi-browser testing framework
- **[wkhtmltopdf Migration](migrating-from-wkhtmltopdf.md)** — Escape the deprecated library

### ❓ Related FAQs
- **[Convert HTML to PDF](FAQ/convert-html-to-pdf-csharp.md)** — Complete FAQ with troubleshooting
- **[Advanced HTML to PDF](FAQ/advanced-html-to-pdf-csharp.md)** — Page breaks, batch processing
- **[URL to PDF](FAQ/url-to-pdf-csharp.md)** — Capture live web pages
- **[Base URL Resolution](FAQ/base-url-html-to-pdf-csharp.md)** — Fix missing images/CSS
- **[Pixel-Perfect Rendering](FAQ/pixel-perfect-html-to-pdf-csharp.md)** — Screen-accurate output
- **[Web Fonts & Icons](FAQ/web-fonts-icons-pdf-csharp.md)** — FontAwesome, Google Fonts
- **[WaitFor & JavaScript](FAQ/waitfor-pdf-rendering-csharp.md)** — Handle dynamic content
- **[Responsive CSS](FAQ/html-to-pdf-responsive-css-csharp.md)** — Media queries in PDF

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — the most comprehensive comparison of 73 C#/.NET PDF libraries with 167 FAQ articles.*
