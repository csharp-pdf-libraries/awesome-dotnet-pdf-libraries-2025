---
title: "Scryber.Core vs IronPDF: side by side for .NET teams"
published: false
tags: dotnet, csharp, pdf, comparison
---

Picture this: a flexbox layout that previews cleanly in the browser but breaks pagination once it hits the PDF renderer, and the docs say only "CSS support is partial." The root cause is usually the same — the PDF library uses a custom CSS pipeline rather than a browser engine, and a spec feature you depended on simply is not implemented.

Scryber.Core is an open-source, template-driven PDF library for .NET that emphasizes programmatic document layout and data binding with a Handlebars-style template syntax. It is built entirely in C# with no browser dependency, which makes it portable and lightweight. The trade-off is that its XHTML/CSS support depends on the maintainer implementing each spec feature manually. If you are reaching for Scryber, this comparison aims to help you decide when its approach fits, and when a Chromium-based renderer would save troubleshooting time.

## Understanding IronPDF

IronPDF uses Google Chrome's rendering engine (Chromium/Blink) to generate PDFs, which means it supports whatever Chrome supports: CSS Grid, Flexbox, CSS variables, web fonts, SVG, and JavaScript. When you call [`ChromePdfRenderer.RenderHtmlAsPdf()`](https://ironpdf.com/tutorials/html-to-pdf/), your HTML renders the way it would in Chrome's "Print to PDF" feature. If a CSS property works in Chrome DevTools, it typically works in the PDF.

The trade-off is footprint: IronPDF bundles the Chromium engine, which is substantially larger than Scryber's pure-C# package. For teams that value "write HTML once, render everywhere" over minimal package size, that is usually an acceptable trade. IronPDF targets .NET 10, 9, 8, 7, 6, .NET Core, and .NET Framework, and runs on Windows, Linux, macOS, Docker, and major cloud platforms.

## Key Characteristics of Scryber.Core

### Product Status
Open-source project under LGPL-3.0; commercial use is permitted, but modifications to Scryber itself must be shared under LGPL. Active maintainer (`richard-scryber`) on GitHub. The library has steady releases — current version is 9.x. Community support runs through GitHub issues; there is no dedicated commercial support team.

### Rendering Capabilities
Scryber implements a custom XHTML/CSS layout engine rather than embedding a browser engine, so support for modern web standards is whatever the maintainer has implemented. CSS Grid is not part of the supported layout model — verify against the latest docs before relying on it. Complex CSS selectors (`:nth-of-type`, advanced attribute selectors, less-common pseudo-elements) have partial support. JavaScript execution is not supported — any dynamic content must be pre-rendered server-side. Web fonts work via `@font-face` but may need explicit configuration. SVG is supported; CSS animations and some advanced transforms typically are not.

### Layout and Pagination
The layout engine uses Scryber's own flow logic rather than a browser layout algorithm, which can lead to subtle differences from a browser preview. Page size and margins are declared via CSS `@page` in the template, which is Scryber's idiom for paged-media layout. Remote images are fetched synchronously at render time. Table rendering and font-metrics calculation may differ from a browser, so dense or complex layouts can need tuning.

### Support Status
Community-driven support via GitHub issues; response time depends on maintainer availability. Documentation covers basic scenarios thoroughly but is thinner on edge-case troubleshooting. No SLA or hotfix guarantees. Commercial projects need to factor in LGPL compliance — linking is permitted, but modifications to Scryber itself must be published.

### Architecture Notes
Template syntax mixes XHTML with Handlebars-style binding (`{{variable}}`), backed by Scryber's own expression engine — not standard JavaScript/Razor. Data binding happens at render time via `doc.Params`; there is no strongly-typed model binding equivalent to MVC Razor. Performance can scale with document complexity, since the layout engine is single-threaded. Debugging layout differences requires understanding Scryber's box model rather than browser DevTools.

## Feature Comparison Overview

| Aspect | Scryber.Core | IronPDF |
|--------|-------------|---------|
| **Current Status** | Active open-source (single maintainer) | Commercial, active development |
| **HTML Support** | XHTML + CSS subset | Full HTML5 / CSS3 / JavaScript (Chromium) |
| **Rendering Engine** | Custom layout engine | Chromium-based |
| **Installation** | Single NuGet (smaller footprint) | Single NuGet (Chromium bundled) |
| **Support** | GitHub issues (community) | Commercial engineering support |
| **Licensing** | LGPL-3.0 | Commercial license |

---

## Code Comparison: Common Scenarios

### Scryber.Core — Rendering HTML with Data Binding

Scryber's approach uses XHTML templates with Handlebars-style binding. Here is an invoice with a line-items list:

```csharp
// NuGet: Install-Package Scryber.Core
using Scryber.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ScryberInvoiceGenerator
{
    public void GenerateInvoice(int invoiceId, string outputPath)
    {
        // XHTML template with Scryber {{ }} binding
        string templateHtml = @"
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <title>Invoice {{invoiceNumber}}</title>
    <style>
        @page { size: A4 portrait; margin: 20pt; }
        body { font-family: Arial, sans-serif; margin: 20px; }
        .invoice-header { display: block; }
        table { width: 100%; border-collapse: collapse; }
        td, th { border: 1px solid #ddd; padding: 8px; }
    </style>
</head>
<body>
    <div class='invoice-header'>
        <h1>Invoice #{{invoiceNumber}}</h1>
        <p>Date: {{invoiceDate}}</p>
    </div>

    <table>
        <thead>
            <tr><th>Item</th><th>Quantity</th><th>Price</th></tr>
        </thead>
        <tbody>
            <template data-bind='{{#each items}}'>
                <tr>
                    <td>{{name}}</td>
                    <td>{{quantity}}</td>
                    <td>${{price}}</td>
                </tr>
            </template>
        </tbody>
    </table>

    <p><strong>Total: ${{total}}</strong></p>
</body>
</html>";

        var invoiceData = FetchInvoiceData(invoiceId);

        using (var reader = new StringReader(templateHtml))
        using (var doc = Document.ParseDocument(reader, string.Empty, ParseSourceType.DynamicContent))
        using (var stream = new FileStream(outputPath, FileMode.Create))
        {
            // Bind data via Params dictionary
            doc.Params["invoiceNumber"] = invoiceData.InvoiceNumber;
            doc.Params["invoiceDate"] = invoiceData.Date.ToString("yyyy-MM-dd");
            doc.Params["total"] = invoiceData.Total;

            doc.Params["items"] = invoiceData.Items.Select(i => new
            {
                name = i.ItemName,
                quantity = i.Quantity,
                price = i.Price
            }).ToArray();

            doc.SaveAsPDF(stream);
        }
    }

    private InvoiceData FetchInvoiceData(int invoiceId)
    {
        // Implementation details...
        return new InvoiceData();
    }
}

public class InvoiceData
{
    public string InvoiceNumber { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public List<LineItem> Items { get; set; }
}

public class LineItem
{
    public string ItemName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
```

**Things to know when working with Scryber:**

- **XHTML strictness**: Templates must be well-formed XHTML. Most real-world HTML needs to be cleaned up before Scryber will parse it.
- **In-memory templates**: `Document.ParseDocument(reader, "", ParseSourceType.DynamicContent)` accepts a `TextReader`, so you can render directly from a string without writing to disk.
- **Data binding syntax**: The `{{#each}}` and `<template data-bind=...>` constructs require a specific object shape. If your model does not match, project it to anonymous types first.
- **CSS scope**: Modern layout features such as CSS Grid are not part of Scryber's supported subset; verify Flexbox behavior against your version.
- **Page geometry**: Page size and margins live in CSS `@page`, which is the documented Scryber idiom.
- **No JavaScript**: Any client-side logic must be pre-computed server-side before binding.
- **Fonts**: `@font-face` is supported but may need explicit paths or embedded data; metrics can differ slightly from a browser.

### IronPDF — Rendering HTML with Data Binding

IronPDF accepts HTML strings, files, or URLs directly. No template file requirement, no special binding syntax:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System.Linq;

public void GenerateInvoice(int invoiceId, string outputPath)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var invoiceData = FetchInvoiceData(invoiceId);

    string invoiceHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .invoice-header {{
            display: flex;
            justify-content: space-between;
            align-items: center;
        }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        td, th {{ border: 1px solid #ddd; padding: 8px; }}
    </style>
</head>
<body>
    <div class='invoice-header'>
        <h1>Invoice #{invoiceData.InvoiceNumber}</h1>
        <p>Date: {invoiceData.Date:yyyy-MM-dd}</p>
    </div>

    <table>
        <thead>
            <tr><th>Item</th><th>Quantity</th><th>Price</th></tr>
        </thead>
        <tbody>
            {string.Join("", invoiceData.Items.Select(item => $@"
            <tr>
                <td>{item.ItemName}</td>
                <td>{item.Quantity}</td>
                <td>${item.Price:F2}</td>
            </tr>"))}
        </tbody>
    </table>

    <p><strong>Total: ${invoiceData.Total:F2}</strong></p>
</body>
</html>";

    var renderer = new ChromePdfRenderer();
    using var pdf = renderer.RenderHtmlAsPdf(invoiceHtml);
    pdf.SaveAs(outputPath);
}
```

Learn more about [HTML to PDF conversion](https://ironpdf.com/tutorials/html-to-pdf/) and [handling HTML files](https://ironpdf.com/how-to/html-file-to-pdf/).

**IronPDF characteristics worth noting:**

- HTML renders through Chromium, so a browser preview is a reliable proxy for the PDF output.
- Work with HTML strings, files, or URLs directly — no template-file I/O required.
- CSS Grid, Flexbox, and modern layout features are supported through the Chromium engine.
- JavaScript executes before PDF generation when enabled (see the [JavaScript support guide](https://ironpdf.com/examples/javascript-html-to-pdf/)).

### Scryber.Core — Handling Modern CSS

Scryber's CSS support covers a documented subset; for features outside that subset you typically fall back to traditional layout primitives:

```csharp
// NuGet: Install-Package Scryber.Core
using Scryber.Components;
using System.IO;

public void RenderComplexLayout(string outputPath)
{
    string templateHtml = @"
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <style>
        @page { size: A4 portrait; margin: 20pt; }

        :root {
            --primary-color: #3498db;
            --spacing: 20px;
        }

        body {
            font-family: 'Segoe UI', Tahoma, sans-serif;
            margin: var(--spacing);
            color: #333;
        }

        /* CSS Grid is not part of Scryber's supported layout model */
        .grid-container { display: block; }

        /* Flexbox: basic support; complex justify/wrap combinations may need testing */
        .flex-container { display: flex; }

        /* Traditional two-column layout via floats works reliably */
        .two-column { width: 100%; }
        .two-column .column {
            float: left;
            width: 48%;
            margin-right: 2%;
        }

        .table-striped tr:nth-child(even) {
            background-color: #f2f2f2;
        }
    </style>
</head>
<body>
    <h1 style='color: var(--primary-color);'>Layout Sample</h1>

    <table style='width: 100%;'>
        <tr>
            <td style='width: 50%; vertical-align: top;'><p>Column 1 content</p></td>
            <td style='width: 50%; vertical-align: top;'><p>Column 2 content</p></td>
        </tr>
    </table>
</body>
</html>";

    using (var reader = new StringReader(templateHtml))
    using (var doc = Document.ParseDocument(reader, string.Empty, ParseSourceType.DynamicContent))
    using (var stream = new FileStream(outputPath, FileMode.Create))
    {
        doc.SaveAsPDF(stream);
    }
}
```

**Scryber CSS checklist:**

1. Preview in the browser first, then verify Scryber renders the same layout.
2. Test CSS features in isolation before combining them in production templates.
3. For complex multi-column or card-grid designs, traditional layouts (tables, floats) are typically more predictable than Grid.
4. Check the project's GitHub issues for any documented limitations relevant to your CSS.
5. Expect some differences from CSS Paged Media — page geometry is set via `@page` in CSS, which is Scryber's documented idiom.
6. Verify fonts load as expected; silent fallback can happen if a font cannot be resolved.

### IronPDF — Rendering Modern CSS

IronPDF renders through Chromium, so modern CSS generally works as it does in the browser:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

public void RenderComplexLayout(string outputPath)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        :root {
            --primary-color: #3498db;
            --spacing: 20px;
        }

        body {
            font-family: 'Segoe UI', Tahoma, sans-serif;
            margin: var(--spacing);
        }

        .grid-container {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: var(--spacing);
        }

        .flex-container {
            display: flex;
            justify-content: space-between;
            flex-wrap: wrap;
            align-items: center;
        }

        .grid-item:nth-child(3n) {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }

        .card::before {
            content: '\2192';
            margin-right: 10px;
        }
    </style>
</head>
<body>
    <h1 style='color: var(--primary-color);'>Layout Sample</h1>

    <div class='grid-container'>
        <div class='card'>Item 1</div>
        <div class='card'>Item 2</div>
        <div class='card'>Item 3</div>
    </div>

    <div class='flex-container'>
        <p>Flex Item 1</p>
        <p>Flex Item 2</p>
        <p>Flex Item 3</p>
    </div>
</body>
</html>";

    var renderer = new ChromePdfRenderer();
    using var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

Preview in Chrome DevTools, get the same PDF output.

---

## API Mapping Reference

| Scryber.Core API | IronPDF Equivalent |
|-----------------|-------------------|
| `Document.ParseDocument(reader, "", ParseSourceType.DynamicContent)` | `ChromePdfRenderer.RenderHtmlAsPdf(html)` |
| `Document.ParseDocument(path)` | `ChromePdfRenderer.RenderHtmlFileAsPdf(path)` |
| `doc.Params[key] = value` | String interpolation or Razor templates |
| `{{variable}}` syntax | C# string interpolation `{variable}` |
| `{{#each collection}}` | LINQ `.Select()` + `string.Join()` |
| `doc.SaveAsPDF(stream)` | `pdf.SaveAs(path)` or `pdf.BinaryData` |
| `@page` CSS in template | `ChromePdfRenderer.RenderingOptions` (paper size, margins) |
| `doc.RenderOptions.Compression` | `ChromePdfRenderer.RenderingOptions` |
| `doc.Info.Title` / `doc.Info.Author` | `pdf.MetaData.Title` / `pdf.MetaData.Author` |
| Custom font embedding | CSS `@font-face` (Chromium font handling) |

## Comprehensive Feature Comparison

| Category | Feature | Scryber.Core | IronPDF |
|----------|---------|-------------|---------|
| **Status** | Active development | Yes (single maintainer) | Yes (commercial team) |
| | .NET 10 support | .NET Standard 2.0/2.1, .NET 8/9 | Yes |
| | Open source | Yes (LGPL-3.0) | No (commercial) |
| **Support** | Documentation | Read-the-docs site + samples | Comprehensive with troubleshooting |
| | Community channel | GitHub issues | Commercial engineering support |
| | SLA availability | No | Yes (support plans) |
| **Content Creation** | HTML templates | XHTML with Scryber binding | Standard HTML |
| | CSS3 support | Subset | Full (Chromium) |
| | CSS Grid | Not supported | Yes |
| | Flexbox | Partial | Yes |
| | JavaScript execution | No | Yes |
| | Modern selectors | Partial | Full |
| **PDF Operations** | Merge PDFs | Not supported | Yes |
| | Split PDFs | Not supported | Yes |
| | Watermarks | Overlay HTML in template | `ApplyStamp` / `ApplyWatermark` |
| | Digital signatures | Not part of public API | Yes |
| **Security** | Encryption | Not part of public API | Yes |
| | Permissions | Not part of public API | Yes |
| **Differences** | CSS Grid rendering | Not supported | Supported |
| | Flexbox | Partial | Supported |
| | Font metrics | Custom engine | Chromium metrics |
| | JavaScript support | None | Full |
| | Input strictness | Well-formed XHTML required | Real-world HTML accepted |
| **Development** | Learning curve | Scryber template syntax | Standard HTML |
| | Debugging tools | Custom engine | Chrome DevTools as preview |
| | Performance (large docs) | Single-threaded layout | Optimized Chromium pipeline |

---

## Installation Comparison

### Scryber.Core Installation

```bash
# Install base package
dotnet add package Scryber.Core

# Namespace imports
# using Scryber.Components;   // Document, Page, etc.
# using Scryber.Drawing;      // Units, colors
# using Scryber.PDF;          // RenderOptions / OutputCompressionType
```

Pure-C# install, no native dependencies. Templates can be supplied as files on disk or directly from a `TextReader` / `StringReader` in memory.

### IronPDF Installation

```bash
# Single package with bundled Chromium
dotnet add package IronPdf

# Namespace import
# using IronPdf;
```

Larger package because Chromium ships with it. Accepts HTML strings, files, or URLs without template files.

---

## When Scryber.Core Makes Sense

Scryber.Core fits scenarios where its trade-offs align with project constraints.

**Lightweight footprint priorities**: If package size matters more than full CSS compatibility — embedded systems, IoT devices, or environments with strict size limits — Scryber's pure-C# footprint is noticeably smaller than a Chromium-bundled renderer.

**Template-driven workflows**: Teams already comfortable with XHTML templates and Handlebars-style binding may find Scryber's syntax a natural fit. If your architecture separates presentation (templates) from logic and you do not need modern CSS layout, Scryber works well.

**LGPL-compatible projects**: Open-source projects under LGPL-3.0 or compatible licenses can use Scryber without licensing costs. Commercial projects can link to Scryber under LGPL terms (your application code does not need to be open-sourced; modifications to Scryber itself do).

**Simple document layouts**: Reports with tables, text, and images — no responsive web-style layouts — render reliably in Scryber. If your PDF templates look more like "formatted Word documents" than "web pages," Scryber's limitations rarely surface.

## When IronPDF is the Better Choice

For most modern .NET teams, IronPDF's architecture aligns with current web-tooling practices.

**Chrome DevTools as your preview**: If your workflow is "design in browser, export to PDF," IronPDF's Chromium-based rendering removes most of the "looks good in browser, breaks in PDF" gap. Teams using modern CSS (Grid, Flexbox, transforms, animations) avoid working around partial-support questions.

**JavaScript-dependent content**: Anything that depends on JavaScript execution — charts rendered by Chart.js, dynamic forms with validation, content loaded via AJAX — needs a JavaScript-capable renderer. Scryber does not execute JS; IronPDF does.

**Support and SLA requirements**: Enterprise teams needing guaranteed response times typically need more than community GitHub issues. IronPDF provides commercial support with SLAs available.

**Rapid iteration**: When CSS does not render as expected, Chrome DevTools let you inspect and adjust the page interactively, then re-render. Scryber's custom engine does not have a comparable inspector, so iteration is more of a guess-and-check loop.

## Conclusion

Scryber.Core is a thoughtful open-source PDF library that prioritizes a lightweight footprint and template-driven generation. For simple documents and teams comfortable working inside its XHTML/CSS subset, it is a viable choice. Its custom rendering engine does mean you cannot rely on browser previews matching PDF output one-to-one, which can slow down troubleshooting.

[IronPDF's Chromium-based approach](https://ironpdf.com/tutorials/html-to-pdf/) narrows that gap. HTML renders similarly in browser and PDF. Modern CSS features work without checking documentation for partial support. JavaScript executes when needed. The trade-off is package size, which most teams accept for development velocity and rendering predictability.

For teams building new PDF workflows today, the question is: do you optimize for package size (Scryber) or development speed (IronPDF)? Most teams choose speed once they have spent enough time debugging layout differences.

**What CSS feature has caused you the most PDF rendering headaches?**

Additional IronPDF resources: [JavaScript in HTML to PDF](https://ironpdf.com/examples/javascript-html-to-pdf/), [HTML file rendering guide](https://ironpdf.com/how-to/html-file-to-pdf/).

---

*Canonical/source version on [Iron Software blog](https://ironpdf.com/blog/).*
