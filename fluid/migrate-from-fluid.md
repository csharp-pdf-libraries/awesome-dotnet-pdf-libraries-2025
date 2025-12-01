# How Do I Migrate from Fluid (Templating) to IronPDF in C#?

## Table of Contents
1. [Why Migrate to IronPDF](#why-migrate-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start Migration](#quick-start-migration)
4. [Complete API Reference](#complete-api-reference)
5. [Code Examples](#code-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting](#troubleshooting)
9. [Migration Checklist](#migration-checklist)

---

## Why Migrate to IronPDF

### The Fluid + External PDF Library Challenges

Fluid is an excellent Liquid-based templating engine, but using it for PDF generation introduces significant complexity:

1. **Two-Library Dependency**: Fluid only generates HTML—you need a separate PDF library (wkhtmltopdf, PuppeteerSharp, etc.) to create PDFs, doubling your dependencies
2. **Integration Complexity**: Coordinating two libraries means managing two sets of configurations, error handling, and updates
3. **Liquid Syntax Learning Curve**: Developers must learn Liquid templating syntax (`{{ }}`, `{% %}`) when C# already has powerful string handling
4. **Limited PDF Control**: Your PDF output quality depends on whichever PDF library you choose to pair with Fluid
5. **Debugging Challenges**: Errors can occur at either the templating or PDF generation stage, making troubleshooting harder
6. **Thread Safety Concerns**: `TemplateContext` is not thread-safe and requires careful management in concurrent applications

### Benefits of IronPDF

| Aspect | Fluid + PDF Library | IronPDF |
|--------|-------------------|---------|
| Dependencies | 2+ packages (Fluid + PDF library) | Single package |
| Templating | Liquid syntax (`{{ }}`) | C# string interpolation or Razor |
| PDF Generation | External library required | Built-in Chromium engine |
| CSS Support | Depends on PDF library | Full CSS3 with Flexbox/Grid |
| JavaScript | Depends on PDF library | Full JavaScript support |
| Thread Safety | TemplateContext not thread-safe | ChromePdfRenderer is thread-safe |
| Learning Curve | Liquid + PDF library API | HTML/CSS (web standards) |
| Error Handling | Two error sources | Single error source |

### Why IronPDF is Better for PDF Generation

IronPDF provides an **all-in-one solution** that eliminates the need for multiple dependencies:

- **Direct PDF Output**: No intermediate HTML file management
- **Chromium Rendering**: Industry-standard rendering engine
- **Full Web Technologies**: CSS3, Flexbox, Grid, JavaScript all work out of the box
- **Better Debugging**: Single point of failure for easier troubleshooting
- **Professional Features**: Headers, footers, watermarks, security—all built-in

---

## Before You Start

### Prerequisites

1. **.NET Environment**: IronPDF supports .NET Framework 4.6.2+, .NET Core 3.1+, .NET 5/6/7/8/9+
2. **NuGet Access**: Ensure you can install packages from NuGet
3. **License Key**: Obtain your IronPDF license key for production use

### Backup Your Project

```bash
# Create a backup branch
git checkout -b pre-ironpdf-migration
git add .
git commit -m "Backup before Fluid to IronPDF migration"
```

### Identify All Fluid Usage

```bash
# Find all Fluid references
grep -r "FluidParser\|FluidTemplate\|TemplateContext\|using Fluid" --include="*.cs" --include="*.csproj" .

# Find Liquid template files
find . -name "*.liquid" -o -name "*.html" | xargs grep -l "{{"
```

### Document Your Templates

Before migration, catalog all templates:
- Template file locations (`.liquid`, `.html`)
- Variables used in each template
- Loops and conditionals
- External PDF library configuration

---

## Quick Start Migration

### Step 1: Update NuGet Packages

```bash
# Remove Fluid and external PDF library
dotnet remove package Fluid.Core
dotnet remove package WkHtmlToPdf-DotNet  # or whatever PDF library you used
dotnet remove package PuppeteerSharp       # if used

# Install IronPDF (all-in-one solution)
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using Fluid;
using Fluid.Values;
using SomeExternalPdfLibrary;

// After
using IronPdf;
using IronPdf.Rendering;  // For RenderingOptions
```

### Step 3: Initialize IronPDF

```csharp
// Set license key at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Optional: Configure Chrome rendering
IronPdf.Installation.ChromeGpuMode = IronPdf.Engines.Chrome.ChromeGpuModes.Disabled;
```

### Step 4: Basic Conversion Pattern

```csharp
// Before (Fluid + external PDF library)
private static readonly FluidParser _parser = new FluidParser();

public async Task<byte[]> GeneratePdfAsync(string templateSource, object model)
{
    if (_parser.TryParse(templateSource, out var template, out var error))
    {
        var context = new TemplateContext(model);
        string html = await template.RenderAsync(context);

        // External PDF library
        var pdfGenerator = new SomePdfLibrary();
        return await pdfGenerator.GeneratePdfAsync(html);
    }
    throw new Exception(error);
}

// After (IronPDF - all-in-one)
public byte[] GeneratePdf(object model)
{
    // Build HTML with C# - no external templating needed
    string html = BuildHtmlFromModel(model);

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

---

## Complete API Reference

### Namespace Mapping

| Fluid Namespace | IronPDF Equivalent | Notes |
|-----------------|-------------------|-------|
| `Fluid` | `IronPdf` | Main namespace |
| `Fluid.Values` | N/A | Use native C# types |
| `Fluid.Ast` | N/A | Not required |
| `Fluid.Parser` | N/A | Not required |
| External PDF namespace | `IronPdf` | Built-in |

### Core Class Mapping

| Fluid Class | IronPDF Equivalent | Notes |
|-------------|-------------------|-------|
| `FluidParser` | N/A | Not needed—use C# strings |
| `FluidTemplate` | N/A | Not needed |
| `IFluidTemplate` | N/A | Not needed |
| `TemplateContext` | C# objects/strings | Pass data directly |
| `TemplateOptions` | `RenderingOptions` | PDF output configuration |
| `FluidValue` | Native C# types | No conversion needed |
| External PDF class | `ChromePdfRenderer` | Main rendering class |

### FluidParser to IronPDF Mapping

| Fluid FluidParser | IronPDF Equivalent | Notes |
|-------------------|-------------------|-------|
| `new FluidParser()` | `new ChromePdfRenderer()` | Create renderer instead |
| `parser.TryParse(source, out template, out error)` | N/A | Not needed—HTML is just a string |
| `parser.Parse(source)` | N/A | Not needed |
| `FluidParserOptions.AllowFunctions` | C# methods | Use C# instead |
| `FluidParserOptions.AllowParentheses` | C# expressions | Use C# instead |

### TemplateContext to IronPDF Mapping

| Fluid TemplateContext | IronPDF Equivalent | Notes |
|-----------------------|-------------------|-------|
| `new TemplateContext()` | N/A | Not needed |
| `new TemplateContext(model)` | String interpolation | Embed data directly in HTML |
| `context.SetValue("key", value)` | Variable in C# | Use C# variables |
| `context.GetValue("key")` | Variable access | Use C# variables |
| `context.Model` | Model object | Pass to HTML builder |
| `context.CultureInfo` | `Thread.CurrentThread.CurrentCulture` | Standard .NET |
| `context.TimeZone` | `TimeZoneInfo` | Standard .NET |

### IFluidTemplate Methods

| Fluid IFluidTemplate | IronPDF Equivalent | Notes |
|----------------------|-------------------|-------|
| `template.Render(context)` | `renderer.RenderHtmlAsPdf(html)` | Direct PDF rendering |
| `template.RenderAsync(context)` | `renderer.RenderHtmlAsPdf(html)` | Sync in IronPDF |
| `template.Render(context, TextEncoder)` | `renderer.RenderHtmlAsPdf(html)` | HTML encoding in string |

### TemplateOptions to RenderingOptions

| Fluid TemplateOptions | IronPDF RenderingOptions | Notes |
|-----------------------|-------------------------|-------|
| `options.MemberAccessStrategy` | N/A | Not needed (C# access) |
| `options.Undefined` | Error handling | Try/catch around render |
| `options.FileProvider` | `RenderingOptions.BaseUrl` | For file references |
| `options.CultureInfo` | Standard .NET culture | Applied during HTML build |
| `options.TimeZone` | Standard .NET timezone | Applied during HTML build |
| `options.Now` | `DateTime.Now` | C# standard |
| `options.MaxSteps` | N/A | Not needed |
| `options.MaxRecursion` | N/A | Not needed |

### RenderingOptions (PDF-Specific)

| IronPDF RenderingOptions | Purpose | Notes |
|-------------------------|---------|-------|
| `PaperSize` | Page dimensions | A4, Letter, custom |
| `PaperOrientation` | Portrait/Landscape | Page orientation |
| `MarginTop/Bottom/Left/Right` | Page margins | In millimeters |
| `PrintHtmlBackgrounds` | Background colors/images | Default: false |
| `HtmlHeader` | Page header | HTML-based |
| `HtmlFooter` | Page footer with {page}/{total-pages} | HTML-based |
| `Timeout` | Render timeout | In seconds |
| `CssMediaType` | Print vs Screen CSS | Print or Screen |
| `BaseUrl` | Base for relative paths | For images, CSS |

### Liquid Syntax to C# Mapping

| Liquid Syntax | C# Equivalent | Notes |
|---------------|--------------|-------|
| `{{ variable }}` | `$"{variable}"` | String interpolation |
| `{{ object.property }}` | `$"{obj.Property}"` | Property access |
| `{% if condition %}` | `if (condition)` | C# conditional |
| `{% unless condition %}` | `if (!condition)` | Negated conditional |
| `{% for item in collection %}` | `foreach (var item in collection)` | Loop |
| `{% assign x = value %}` | `var x = value;` | Variable assignment |
| `{% capture %}...{% endcapture %}` | `var s = BuildString();` | String building |
| `{{ value \| filter }}` | Method call | `Filter(value)` |
| `{% include 'partial' %}` | Concatenate HTML | Or use partial methods |

### Common Liquid Filters to C# Methods

| Liquid Filter | C# Equivalent | Example |
|---------------|--------------|---------|
| `{{ x \| upcase }}` | `x.ToUpper()` | String method |
| `{{ x \| downcase }}` | `x.ToLower()` | String method |
| `{{ x \| capitalize }}` | Custom method | First letter upper |
| `{{ x \| size }}` | `x.Length` or `x.Count()` | Collection size |
| `{{ x \| first }}` | `x.First()` | LINQ |
| `{{ x \| last }}` | `x.Last()` | LINQ |
| `{{ x \| join: ', ' }}` | `string.Join(", ", x)` | String method |
| `{{ x \| split: ',' }}` | `x.Split(',')` | String method |
| `{{ x \| date: '%Y-%m-%d' }}` | `x.ToString("yyyy-MM-dd")` | Date formatting |
| `{{ x \| number_with_delimiter }}` | `x.ToString("N0")` | Number formatting |
| `{{ x \| money }}` | `x.ToString("C")` | Currency formatting |
| `{{ x \| truncate: 50 }}` | `x.Substring(0, Math.Min(50, x.Length))` | String truncation |
| `{{ x \| escape }}` | `WebUtility.HtmlEncode(x)` | HTML encoding |
| `{{ x \| default: 'N/A' }}` | `x ?? "N/A"` | Null coalescing |

---

## Code Examples

### Example 1: Simple Variable Substitution

**Before (Fluid + external PDF):**
```csharp
using Fluid;
using SomeExternalPdfLibrary;

private static readonly FluidParser _parser = new FluidParser();

public async Task<byte[]> GenerateWelcomePdf(string name)
{
    string source = "<html><body><h1>Hello {{ name }}!</h1><p>Welcome to our service.</p></body></html>";

    if (_parser.TryParse(source, out var template, out var error))
    {
        var context = new TemplateContext();
        context.SetValue("name", name);

        string html = await template.RenderAsync(context);

        // Need separate PDF library
        var pdfGenerator = new PdfGenerator();
        return await pdfGenerator.GenerateAsync(html);
    }

    throw new Exception($"Template parsing failed: {error}");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] GenerateWelcomePdf(string name)
{
    // Direct string interpolation - no parsing needed
    string html = $"<html><body><h1>Hello {name}!</h1><p>Welcome to our service.</p></body></html>";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 2: Looping Through Collections

**Before (Fluid + external PDF):**
```csharp
using Fluid;
using SomeExternalPdfLibrary;

private static readonly FluidParser _parser = new FluidParser();

public async Task<byte[]> GenerateProductListPdf(List<Product> products)
{
    string source = @"
        <html>
        <head><style>table { width: 100%; border-collapse: collapse; } th, td { border: 1px solid #ddd; padding: 8px; }</style></head>
        <body>
            <h1>Product Catalog</h1>
            <table>
                <tr><th>Name</th><th>Price</th><th>Stock</th></tr>
                {% for product in products %}
                    <tr>
                        <td>{{ product.Name }}</td>
                        <td>${{ product.Price | number_with_precision: 2 }}</td>
                        <td>{{ product.Stock }}</td>
                    </tr>
                {% endfor %}
            </table>
        </body>
        </html>";

    if (_parser.TryParse(source, out var template, out var error))
    {
        var context = new TemplateContext();
        context.SetValue("products", products);

        string html = await template.RenderAsync(context);

        var pdfGenerator = new PdfGenerator();
        return await pdfGenerator.GenerateAsync(html);
    }

    throw new Exception(error);
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Text;

public byte[] GenerateProductListPdf(List<Product> products)
{
    var html = new StringBuilder();
    html.Append(@"
        <html>
        <head><style>
            table { width: 100%; border-collapse: collapse; }
            th { background: #4CAF50; color: white; }
            th, td { border: 1px solid #ddd; padding: 8px; }
            tr:nth-child(even) { background: #f2f2f2; }
        </style></head>
        <body>
            <h1>Product Catalog</h1>
            <table>
                <tr><th>Name</th><th>Price</th><th>Stock</th></tr>");

    foreach (var product in products)
    {
        html.Append($@"
            <tr>
                <td>{product.Name}</td>
                <td>${product.Price:F2}</td>
                <td>{product.Stock}</td>
            </tr>");
    }

    html.Append("</table></body></html>");

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html.ToString());
    return pdf.BinaryData;
}
```

### Example 3: Conditional Rendering

**Before (Fluid + external PDF):**
```csharp
using Fluid;
using SomeExternalPdfLibrary;

private static readonly FluidParser _parser = new FluidParser();

public async Task<byte[]> GenerateMembershipCard(User user)
{
    string source = @"
        <html>
        <head>
            <style>
                .premium { background: gold; color: black; }
                .standard { background: silver; color: black; }
                .card { padding: 20px; border-radius: 10px; }
            </style>
        </head>
        <body>
            <div class='card {% if user.IsPremium %}premium{% else %}standard{% endif %}'>
                <h1>{{ user.Name }}</h1>
                {% if user.IsPremium %}
                    <p>Premium Member since {{ user.MemberSince | date: '%B %Y' }}</p>
                    <p>Benefits: Priority Support, Exclusive Content</p>
                {% else %}
                    <p>Standard Member</p>
                    <p><a href='#'>Upgrade to Premium</a></p>
                {% endif %}
            </div>
        </body>
        </html>";

    if (_parser.TryParse(source, out var template, out var error))
    {
        var context = new TemplateContext();
        context.SetValue("user", user);

        string html = await template.RenderAsync(context);

        var pdfGenerator = new PdfGenerator();
        return await pdfGenerator.GenerateAsync(html);
    }

    throw new Exception(error);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] GenerateMembershipCard(User user)
{
    string cardClass = user.IsPremium ? "premium" : "standard";
    string membershipContent = user.IsPremium
        ? $@"<p>Premium Member since {user.MemberSince:MMMM yyyy}</p>
             <p>Benefits: Priority Support, Exclusive Content</p>"
        : @"<p>Standard Member</p>
            <p><a href='#'>Upgrade to Premium</a></p>";

    string html = $@"
        <html>
        <head>
            <style>
                .premium {{ background: gold; color: black; }}
                .standard {{ background: silver; color: black; }}
                .card {{ padding: 20px; border-radius: 10px; }}
            </style>
        </head>
        <body>
            <div class='card {cardClass}'>
                <h1>{user.Name}</h1>
                {membershipContent}
            </div>
        </body>
        </html>";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Example 4: Invoice with Headers and Footers

**Before (Fluid + external PDF):**
```csharp
using Fluid;
using SomeExternalPdfLibrary;

private static readonly FluidParser _parser = new FluidParser();

public async Task<byte[]> GenerateInvoicePdf(Invoice invoice)
{
    string source = @"
        <html>
        <body>
            <h1>Invoice #{{ invoice.Number }}</h1>
            <p>Date: {{ invoice.Date | date: '%Y-%m-%d' }}</p>
            <p>Customer: {{ invoice.CustomerName }}</p>

            <table>
                <tr><th>Item</th><th>Qty</th><th>Price</th><th>Total</th></tr>
                {% for item in invoice.Items %}
                    <tr>
                        <td>{{ item.Description }}</td>
                        <td>{{ item.Quantity }}</td>
                        <td>${{ item.UnitPrice }}</td>
                        <td>${{ item.Total }}</td>
                    </tr>
                {% endfor %}
            </table>

            <p><strong>Total: ${{ invoice.Total }}</strong></p>
        </body>
        </html>";

    if (_parser.TryParse(source, out var template, out var error))
    {
        var context = new TemplateContext();
        context.SetValue("invoice", invoice);

        string html = await template.RenderAsync(context);

        // External PDF library - headers/footers might be limited
        var pdfGenerator = new PdfGenerator();
        return await pdfGenerator.GenerateAsync(html);
    }

    throw new Exception(error);
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Text;

public byte[] GenerateInvoicePdf(Invoice invoice)
{
    var html = new StringBuilder();
    html.Append($@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; padding: 20px; }}
                table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                th {{ background: #333; color: white; padding: 10px; }}
                td {{ border: 1px solid #ddd; padding: 8px; }}
                .total {{ font-size: 1.2em; font-weight: bold; text-align: right; }}
            </style>
        </head>
        <body>
            <h1>Invoice #{invoice.Number}</h1>
            <p>Date: {invoice.Date:yyyy-MM-dd}</p>
            <p>Customer: {invoice.CustomerName}</p>

            <table>
                <tr><th>Item</th><th>Qty</th><th>Price</th><th>Total</th></tr>");

    foreach (var item in invoice.Items)
    {
        html.Append($@"
            <tr>
                <td>{item.Description}</td>
                <td>{item.Quantity}</td>
                <td>${item.UnitPrice:F2}</td>
                <td>${item.Total:F2}</td>
            </tr>");
    }

    html.Append($@"
            </table>
            <p class='total'>Total: ${invoice.Total:F2}</p>
        </body>
        </html>");

    var renderer = new ChromePdfRenderer();

    // Professional headers and footers - built into IronPDF!
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
    {
        HtmlFragment = $"<div style='text-align:center; font-size:10pt;'>Invoice #{invoice.Number}</div>",
        DrawDividerLine = true
    };

    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
    {
        HtmlFragment = "<div style='text-align:right; font-size:9pt;'>Page {page} of {total-pages}</div>",
        DrawDividerLine = true
    };

    var pdf = renderer.RenderHtmlAsPdf(html.ToString());
    return pdf.BinaryData;
}
```

### Example 5: Nested Objects and Complex Data

**Before (Fluid + external PDF):**
```csharp
using Fluid;
using SomeExternalPdfLibrary;

private static readonly FluidParser _parser = new FluidParser();

public async Task<byte[]> GenerateOrderSummaryPdf(Order order)
{
    string source = @"
        <html>
        <body>
            <h1>Order #{{ order.Id }}</h1>

            <h2>Shipping Address</h2>
            <p>{{ order.ShippingAddress.Name }}</p>
            <p>{{ order.ShippingAddress.Street }}</p>
            <p>{{ order.ShippingAddress.City }}, {{ order.ShippingAddress.State }} {{ order.ShippingAddress.Zip }}</p>

            <h2>Items</h2>
            {% for item in order.Items %}
                <div class='item'>
                    <h3>{{ item.Product.Name }}</h3>
                    <p>Category: {{ item.Product.Category }}</p>
                    <p>Quantity: {{ item.Quantity }} x ${{ item.Product.Price }}</p>
                    {% if item.Product.OnSale %}
                        <p class='sale'>SALE!</p>
                    {% endif %}
                </div>
            {% endfor %}
        </body>
        </html>";

    if (_parser.TryParse(source, out var template, out var error))
    {
        var options = new TemplateOptions();
        options.MemberAccessStrategy.Register<Order>();
        options.MemberAccessStrategy.Register<Address>();
        options.MemberAccessStrategy.Register<OrderItem>();
        options.MemberAccessStrategy.Register<Product>();

        var context = new TemplateContext(order, options);
        string html = await template.RenderAsync(context);

        var pdfGenerator = new PdfGenerator();
        return await pdfGenerator.GenerateAsync(html);
    }

    throw new Exception(error);
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Text;

public byte[] GenerateOrderSummaryPdf(Order order)
{
    var html = new StringBuilder();
    html.Append($@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; padding: 20px; }}
                .item {{ border: 1px solid #eee; padding: 15px; margin: 10px 0; }}
                .sale {{ color: red; font-weight: bold; }}
            </style>
        </head>
        <body>
            <h1>Order #{order.Id}</h1>

            <h2>Shipping Address</h2>
            <p>{order.ShippingAddress.Name}</p>
            <p>{order.ShippingAddress.Street}</p>
            <p>{order.ShippingAddress.City}, {order.ShippingAddress.State} {order.ShippingAddress.Zip}</p>

            <h2>Items</h2>");

    // No MemberAccessStrategy registration needed!
    foreach (var item in order.Items)
    {
        html.Append($@"
            <div class='item'>
                <h3>{item.Product.Name}</h3>
                <p>Category: {item.Product.Category}</p>
                <p>Quantity: {item.Quantity} x ${item.Product.Price:F2}</p>");

        if (item.Product.OnSale)
        {
            html.Append("<p class='sale'>SALE!</p>");
        }

        html.Append("</div>");
    }

    html.Append("</body></html>");

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html.ToString());
    return pdf.BinaryData;
}
```

### Example 6: Including Partial Templates

**Before (Fluid + external PDF):**
```csharp
using Fluid;
using SomeExternalPdfLibrary;

private static readonly FluidParser _parser = new FluidParser();

public async Task<byte[]> GenerateReportWithPartials(ReportData data)
{
    // Main template includes partials
    string source = @"
        <html>
        <body>
            {% include 'header' %}
            <h1>{{ title }}</h1>
            {% include 'data_table' %}
            {% include 'footer' %}
        </body>
        </html>";

    var options = new TemplateOptions();
    options.FileProvider = new PhysicalFileProvider(templatesPath);

    if (_parser.TryParse(source, out var template, out var error))
    {
        var context = new TemplateContext(data, options);
        string html = await template.RenderAsync(context);

        var pdfGenerator = new PdfGenerator();
        return await pdfGenerator.GenerateAsync(html);
    }

    throw new Exception(error);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public byte[] GenerateReportWithPartials(ReportData data)
{
    // Use methods to create reusable HTML components
    string html = $@"
        <html>
        <body>
            {BuildHeader(data.CompanyName)}
            <h1>{data.Title}</h1>
            {BuildDataTable(data.Items)}
            {BuildFooter()}
        </body>
        </html>";

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}

private string BuildHeader(string companyName)
{
    return $@"<header style='border-bottom: 2px solid #333; padding-bottom: 10px;'>
        <h2>{companyName}</h2>
    </header>";
}

private string BuildDataTable(List<DataItem> items)
{
    var sb = new StringBuilder("<table><tr><th>Name</th><th>Value</th></tr>");
    foreach (var item in items)
    {
        sb.Append($"<tr><td>{item.Name}</td><td>{item.Value}</td></tr>");
    }
    sb.Append("</table>");
    return sb.ToString();
}

private string BuildFooter()
{
    return $"<footer style='border-top: 1px solid #ccc; margin-top: 20px; padding-top: 10px;'>Generated on {DateTime.Now:yyyy-MM-dd}</footer>";
}
```

### Example 7: Using Razor for Complex Templates

For complex templates, you can combine IronPDF with Razor instead of Fluid:

```csharp
using IronPdf;
using RazorLight;
using System.Threading.Tasks;

public class PdfService
{
    private readonly RazorLightEngine _razorEngine;
    private readonly ChromePdfRenderer _renderer;

    public PdfService()
    {
        _razorEngine = new RazorLightEngineBuilder()
            .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates"))
            .UseMemoryCachingProvider()
            .Build();

        _renderer = new ChromePdfRenderer();
    }

    public async Task<byte[]> GeneratePdfFromRazorAsync<T>(string templateName, T model)
    {
        // Razor replaces Liquid with familiar C# syntax
        // @Model.Property instead of {{ property }}
        // @foreach instead of {% for %}
        string html = await _razorEngine.CompileRenderAsync(templateName, model);
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}

// Template (Templates/Invoice.cshtml):
// @model InvoiceModel
// <html>
// <body>
//     <h1>Invoice #@Model.Number</h1>
//     @foreach (var item in Model.Items)
//     {
//         <p>@item.Description - $@item.Price</p>
//     }
// </body>
// </html>
```

### Example 8: PDF Security Features

**Before (Fluid + external PDF - often limited security):**
```csharp
// External PDF libraries often have limited security features
var pdfGenerator = new PdfGenerator();
var pdf = await pdfGenerator.GenerateAsync(html);
// Security options depend entirely on the external library
```

**After (IronPDF - comprehensive security built-in):**
```csharp
using IronPdf;

public byte[] GenerateSecurePdf(string html, string ownerPassword, string userPassword)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Set document metadata
    pdf.MetaData.Title = "Confidential Report";
    pdf.MetaData.Author = "Company Name";

    // Password protection
    pdf.SecuritySettings.OwnerPassword = ownerPassword;
    pdf.SecuritySettings.UserPassword = userPassword;

    // Restrict permissions
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;
    pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.NoPrint;
    pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;
    pdf.SecuritySettings.AllowUserAnnotations = false;

    return pdf.BinaryData;
}
```

### Example 9: Merging Multiple PDFs

```csharp
using IronPdf;
using System.Collections.Generic;

public byte[] GenerateCombinedReport(List<ReportSection> sections)
{
    var renderer = new ChromePdfRenderer();
    var pdfs = new List<PdfDocument>();

    foreach (var section in sections)
    {
        string html = BuildSectionHtml(section);
        pdfs.Add(renderer.RenderHtmlAsPdf(html));
    }

    // Merge all sections into one PDF
    var combined = PdfDocument.Merge(pdfs);
    return combined.BinaryData;
}
```

### Example 10: Adding Watermarks

```csharp
using IronPdf;

public byte[] GenerateWatermarkedPdf(string html, bool isDraft)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    if (isDraft)
    {
        pdf.ApplyWatermark("<h1 style='color:red; opacity:0.5; font-size:72pt;'>DRAFT</h1>",
            rotation: 45,
            opacity: 50);
    }

    return pdf.BinaryData;
}
```

---

## Advanced Scenarios

### Thread-Safe PDF Generation Service

```csharp
using IronPdf;
using System.Collections.Concurrent;

public class ThreadSafePdfService
{
    // ChromePdfRenderer is thread-safe - can be shared
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    public byte[] GeneratePdf(object model)
    {
        // Each call creates its own HTML string - no shared state issues
        string html = BuildHtml(model);
        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }

    // Compare to Fluid where TemplateContext is NOT thread-safe:
    // Each call would need new FluidParser and TemplateContext instances
}
```

### Batch PDF Generation

```csharp
using IronPdf;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class BatchPdfGenerator
{
    private readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    public async Task GenerateBatchAsync(List<Invoice> invoices, string outputFolder)
    {
        await Parallel.ForEachAsync(invoices, async (invoice, ct) =>
        {
            string html = BuildInvoiceHtml(invoice);
            var pdf = _renderer.RenderHtmlAsPdf(html);
            await Task.Run(() => pdf.SaveAs(Path.Combine(outputFolder, $"invoice_{invoice.Id}.pdf")));
        });
    }
}
```

### HTML Template Files (Like Liquid Files)

If you prefer storing templates in files like Liquid:

```csharp
using IronPdf;
using System.Text.RegularExpressions;

public class FileBasedTemplateService
{
    private readonly string _templateFolder;
    private readonly ChromePdfRenderer _renderer;

    public FileBasedTemplateService(string templateFolder)
    {
        _templateFolder = templateFolder;
        _renderer = new ChromePdfRenderer();
    }

    public byte[] RenderTemplate(string templateName, Dictionary<string, string> variables)
    {
        string templatePath = Path.Combine(_templateFolder, $"{templateName}.html");
        string template = File.ReadAllText(templatePath);

        // Simple variable replacement (similar to Liquid {{ variable }})
        string html = Regex.Replace(template, @"\{\{\s*(\w+)\s*\}\}", match =>
        {
            string key = match.Groups[1].Value;
            return variables.TryGetValue(key, out var value) ? value : match.Value;
        });

        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

---

## Performance Considerations

### Reuse ChromePdfRenderer

```csharp
// GOOD - Reuse the renderer (it's thread-safe)
public class PdfService
{
    private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

    public byte[] Generate(string html) => _renderer.RenderHtmlAsPdf(html).BinaryData;
}

// BAD - Creates new renderer each time (wasteful)
public byte[] GenerateBad(string html)
{
    var renderer = new ChromePdfRenderer();  // Don't do this repeatedly
    return renderer.RenderHtmlAsPdf(html).BinaryData;
}
```

### Cache Static HTML Components

```csharp
public class CachedPdfService
{
    private static readonly string _headerHtml;
    private static readonly string _footerHtml;
    private static readonly string _stylesHtml;
    private readonly ChromePdfRenderer _renderer;

    static CachedPdfService()
    {
        // Cache static parts at startup
        _headerHtml = File.ReadAllText("templates/header.html");
        _footerHtml = File.ReadAllText("templates/footer.html");
        _stylesHtml = File.ReadAllText("templates/styles.css");
    }

    public byte[] GeneratePdf(object model)
    {
        // Only build the dynamic parts
        string content = BuildContent(model);
        string html = $"<html><head><style>{_stylesHtml}</style></head><body>{_headerHtml}{content}{_footerHtml}</body></html>";

        var pdf = _renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### StringBuilder for Large Documents

```csharp
// For large documents with many items
public byte[] GenerateLargeReport(List<DataItem> items)
{
    // Use StringBuilder for better performance with many concatenations
    var html = new StringBuilder(50000);  // Pre-allocate if you know approximate size

    html.Append("<html><body><table>");

    foreach (var item in items)
    {
        html.Append($"<tr><td>{item.Name}</td><td>{item.Value}</td></tr>");
    }

    html.Append("</table></body></html>");

    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html.ToString());
    return pdf.BinaryData;
}
```

---

## Troubleshooting

### Issue 1: Missing CSS Styles

**Problem**: PDF doesn't look like the HTML preview in browser.

**Solution**: Enable print backgrounds and set CSS media type:

```csharp
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;
```

### Issue 2: Images Not Loading

**Problem**: Images appear broken in the PDF.

**Solution**: Use absolute paths or Base64 encoding:

```csharp
// Option 1: Set base URL
renderer.RenderingOptions.BaseUrl = new Uri("file:///C:/MyProject/assets/");

// Option 2: Use Base64
byte[] imageBytes = File.ReadAllBytes("logo.png");
string base64 = Convert.ToBase64String(imageBytes);
string imgTag = $"<img src='data:image/png;base64,{base64}' />";
```

### Issue 3: Page Breaks in Wrong Places

**Problem**: Content breaks awkwardly between pages.

**Solution**: Use CSS page break properties:

```css
/* Prevent break inside element */
.no-break { page-break-inside: avoid; }

/* Force break before element */
.new-page { page-break-before: always; }

/* Force break after element */
.section-end { page-break-after: always; }
```

### Issue 4: Liquid Syntax Not Working

**Problem**: `{{ variable }}` appears literally in PDF.

**Solution**: IronPDF doesn't process Liquid syntax. Replace with C# string interpolation:

```csharp
// WRONG - Liquid syntax won't work
string html = "<h1>Hello {{ name }}</h1>";

// CORRECT - Use C# string interpolation
string html = $"<h1>Hello {name}</h1>";
```

### Issue 5: Performance Slower Than Expected

**Problem**: PDF generation is slow.

**Solution**: Reuse renderer and optimize HTML:

```csharp
// Reuse renderer
private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

// Reduce render timeout for simple documents
renderer.RenderingOptions.Timeout = 30;

// Disable features you don't need
renderer.RenderingOptions.EnableJavaScript = false;  // If no JS needed
```

### Issue 6: Headers/Footers Not Appearing

**Problem**: `HtmlHeader` and `HtmlFooter` don't show up.

**Solution**: Ensure proper configuration:

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='width:100%; text-align:center;'>Header</div>",
    MaxHeight = 30,  // Set explicit height in mm
    DrawDividerLine = true
};
```

### Issue 7: Encoding Issues with Special Characters

**Problem**: Special characters appear garbled.

**Solution**: Ensure UTF-8 encoding:

```csharp
string html = $@"
    <html>
    <head>
        <meta charset='UTF-8'>
    </head>
    <body>
        <p>{WebUtility.HtmlEncode(textWithSpecialChars)}</p>
    </body>
    </html>";
```

### Issue 8: Large Files Taking Too Long

**Problem**: Large HTML documents are slow to render.

**Solution**: Split into multiple PDFs and merge:

```csharp
var pdfs = new List<PdfDocument>();
foreach (var chunk in data.Chunk(100))  // Process in chunks
{
    string chunkHtml = BuildChunkHtml(chunk);
    pdfs.Add(renderer.RenderHtmlAsPdf(chunkHtml));
}
var merged = PdfDocument.Merge(pdfs);
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Fluid templates (.liquid, .html with `{{ }}`)**
  ```bash
  grep -r "{{" --include="*.liquid" --include="*.html" .
  ```
  **Why:** Identify all template usages to ensure complete migration coverage.

- [ ] **Document all variables used in each template**
  ```csharp
  // Example variable usage in Fluid
  {{ variableName }}
  ```
  **Why:** These variables will be converted to C# string interpolation. Document them to ensure consistent output after migration.

- [ ] **List external PDF library and its configuration**
  ```csharp
  // Example configuration
  var pdfConfig = new ExternalPdfLibrary.Config {
      Setting1 = value1,
      Setting2 = value2
  };
  ```
  **Why:** Understanding current configurations helps map them to IronPDF's RenderingOptions.

- [ ] **Note any custom Liquid filters or tags**
  ```csharp
  // Example custom filter
  {% custom_filter %}
  ```
  **Why:** Custom filters will need to be converted to equivalent C# methods.

- [ ] **Backup project to version control**
  **Why:** Ensure you have a rollback point in case of migration issues.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Package Migration

- [ ] **Remove `Fluid.Core` package**
  ```bash
  dotnet remove package Fluid.Core
  ```
  **Why:** Remove unused dependencies to clean up the project.

- [ ] **Remove external PDF library package**
  ```bash
  dotnet remove package ExternalPdfLibrary
  ```
  **Why:** Transition to a single PDF solution with IronPDF.

- [ ] **Install `IronPdf` package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add the new PDF library to handle all PDF generation tasks.

- [ ] **Update namespace imports**
  ```csharp
  // Before (.)
  using ExternalPdfLibrary;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure the code references the correct PDF library.

- [ ] **Set IronPDF license key at startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Code Migration

- [ ] **Convert `{{ variable }}` to `$"{variable}"`**
  ```csharp
  // Before (.)
  {{ variable }}

  // After (IronPDF)
  $"{variable}"
  ```
  **Why:** Use C# string interpolation for better integration and performance.

- [ ] **Convert `{% for item in collection %}` to `foreach`**
  ```csharp
  // Before (.)
  {% for item in collection %}
  {{ item }}
  {% endfor %}

  // After (IronPDF)
  foreach (var item in collection)
  {
      Console.WriteLine($"{item}");
  }
  ```
  **Why:** Utilize C# loops for better performance and readability.

- [ ] **Convert `{% if condition %}` to `if` statements**
  ```csharp
  // Before (.)
  {% if condition %}
  {{ trueValue }}
  {% endif %}

  // After (IronPDF)
  if (condition)
  {
      Console.WriteLine($"{trueValue}");
  }
  ```
  **Why:** Use C# conditional logic for better integration and performance.

- [ ] **Convert Liquid filters to C# methods**
  ```csharp
  // Before (.)
  {{ value | custom_filter }}

  // After (IronPDF)
  CustomFilterMethod(value);
  ```
  **Why:** Replace Liquid filters with C# methods for better integration.

- [ ] **Replace `FluidParser` with `ChromePdfRenderer`**
  ```csharp
  // Before (.)
  var parser = new FluidParser();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** Use IronPDF's renderer for direct PDF generation from HTML.

- [ ] **Replace `TemplateContext.SetValue()` with direct variables**
  ```csharp
  // Before (.)
  context.SetValue("key", value);

  // After (IronPDF)
  var key = value;
  ```
  **Why:** Simplify variable management using direct C# assignments.

- [ ] **Remove external PDF library calls**
  ```csharp
  // Before (.)
  var pdf = externalLibrary.GeneratePdf(html);

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(html);
  ```
  **Why:** Transition to using IronPDF for all PDF generation tasks.

- [ ] **Configure `RenderingOptions` for PDF settings**
  ```csharp
  // Before (.)
  externalLibrary.PageSize = PageSizes.A4;

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** Use IronPDF's RenderingOptions for comprehensive PDF settings.

- [ ] **Add headers/footers using `HtmlHeaderFooter`**
  ```csharp
  // Before (.)
  externalLibrary.Header = "Header Text";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Header Text</div>",
      MaxHeight = 25
  };
  ```
  **Why:** Use IronPDF's HTML-based headers/footers for more flexibility.

### Testing

- [ ] **Verify PDF output matches expectations**
  **Why:** Ensure the new PDFs meet quality and content standards.

- [ ] **Test all template variations**
  **Why:** Confirm all templates render correctly with IronPDF.

- [ ] **Check images and styling**
  **Why:** Verify that all images and styles are rendered as expected.

- [ ] **Validate page breaks**
  **Why:** Ensure page breaks occur correctly in the new PDFs.

- [ ] **Test with various data sizes**
  **Why:** Confirm performance and correctness with different data inputs.

- [ ] **Performance testing vs Fluid + external library**
  **Why:** Compare performance to ensure improvements or parity.

- [ ] **Test thread safety in concurrent scenarios**
  **Why:** Ensure IronPDF operates correctly in multi-threaded environments.

### Post-Migration

- [ ] **Delete `.liquid` template files (if no longer needed)**
  **Why:** Clean up the project by removing obsolete files.

- [ ] **Remove Fluid-related helper code**
  **Why:** Eliminate unnecessary code to simplify maintenance.

- [ ] **Update documentation**
  **Why:** Ensure all documentation reflects the new PDF generation process.

- [ ] **Clean up unused dependencies**
  **Why:** Reduce project bloat and potential security vulnerabilities.
---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Razor Templates with IronPDF**: Use RazorLight NuGet package for complex templates

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
