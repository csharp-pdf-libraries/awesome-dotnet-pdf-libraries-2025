# Migration Guide: Fluid (Templating) → IronPDF

## Why Migrate from Fluid to IronPDF

While Fluid is an excellent templating engine for generating HTML content, it requires integration with a separate PDF generation library to create PDF documents. IronPDF provides an all-in-one solution that combines HTML templating capabilities with robust PDF generation, eliminating the need for multiple dependencies. By migrating to IronPDF, you can streamline your codebase, reduce complexity, and leverage a comprehensive PDF library designed specifically for document generation from HTML.

## NuGet Package Changes

```xml
<!-- Remove Fluid -->
<PackageReference Include="Fluid.Core" Version="2.x.x" />

<!-- Add IronPDF -->
<PackageReference Include="IronPdf" Version="2024.x.x" />
```

## Namespace Mapping

| Fluid Namespace | IronPDF Namespace |
|----------------|-------------------|
| `Fluid` | `IronPdf` |
| `Fluid.Values` | `IronPdf` (built-in C# types) |
| `Fluid.Ast` | Not required |
| N/A (PDF library needed) | `IronPdf.Rendering` |

## API Mapping

| Fluid API | IronPDF API | Notes |
|-----------|-------------|-------|
| `FluidParser` | `ChromePdfRenderer` | Parse and render in one |
| `FluidTemplate.Render()` | `RenderHtmlAsPdf()` | Direct HTML to PDF |
| `TemplateContext.SetValue()` | String interpolation or `RenderHtmlAsPdf()` | Use C# string features |
| `TemplateOptions` | `RenderingOptions` | Configure PDF output |
| External PDF library | Built-in PDF generation | No additional library needed |

## Code Examples

### Example 1: Basic Template Rendering

**Before (Fluid + PDF library):**
```csharp
using Fluid;
using SomeExternalPdfLibrary;

var parser = new FluidParser();
var template = parser.Parse("<h1>Hello {{ name }}</h1>");

var context = new TemplateContext();
context.SetValue("name", "John Doe");

string html = template.Render(context);

// Requires additional PDF library
var pdfGenerator = new PdfGenerator();
byte[] pdfBytes = pdfGenerator.GenerateFromHtml(html);
File.WriteAllBytes("output.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
string name = "John Doe";
string html = $"<h1>Hello {name}</h1>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: Looping Through Collections

**Before (Fluid + PDF library):**
```csharp
using Fluid;
using SomeExternalPdfLibrary;

var parser = new FluidParser();
var template = parser.Parse(@"
<ul>
{% for item in items %}
    <li>{{ item.name }}: ${{ item.price }}</li>
{% endfor %}
</ul>
");

var context = new TemplateContext();
var items = new[]
{
    new { name = "Product 1", price = 29.99 },
    new { name = "Product 2", price = 49.99 }
};
context.SetValue("items", items);

string html = template.Render(context);

var pdfGenerator = new PdfGenerator();
byte[] pdfBytes = pdfGenerator.GenerateFromHtml(html);
File.WriteAllBytes("invoice.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Text;

var renderer = new ChromePdfRenderer();
var items = new[]
{
    new { name = "Product 1", price = 29.99 },
    new { name = "Product 2", price = 49.99 }
};

var htmlBuilder = new StringBuilder("<ul>");
foreach (var item in items)
{
    htmlBuilder.Append($"<li>{item.name}: ${item.price}</li>");
}
htmlBuilder.Append("</ul>");

var pdf = renderer.RenderHtmlAsPdf(htmlBuilder.ToString());
pdf.SaveAs("invoice.pdf");
```

### Example 3: Conditional Rendering with Styling

**Before (Fluid + PDF library):**
```csharp
using Fluid;
using SomeExternalPdfLibrary;

var parser = new FluidParser();
var template = parser.Parse(@"
<html>
<head>
    <style>
        .premium { color: gold; }
        .standard { color: silver; }
    </style>
</head>
<body>
    <h1>Welcome {{ user.name }}</h1>
    {% if user.isPremium %}
        <p class='premium'>Premium Member</p>
    {% else %}
        <p class='standard'>Standard Member</p>
    {% endif %}
</body>
</html>
");

var context = new TemplateContext();
context.SetValue("user", new { name = "Jane Smith", isPremium = true });

string html = template.Render(context);

var pdfGenerator = new PdfGenerator();
byte[] pdfBytes = pdfGenerator.GenerateFromHtml(html);
File.WriteAllBytes("welcome.pdf", pdfBytes);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var user = new { name = "Jane Smith", isPremium = true };

string membershipClass = user.isPremium ? "premium" : "standard";
string membershipText = user.isPremium ? "Premium Member" : "Standard Member";

string html = $@"
<html>
<head>
    <style>
        .premium {{ color: gold; }}
        .standard {{ color: silver; }}
    </style>
</head>
<body>
    <h1>Welcome {user.name}</h1>
    <p class='{membershipClass}'>{membershipText}</p>
</body>
</html>
";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("welcome.pdf");
```

## Common Gotchas

1. **No Liquid Syntax**: IronPDF doesn't use Liquid templating syntax. Use C# string interpolation, `StringBuilder`, or your preferred C# templating approach instead of `{{ variable }}` and `{% if %}` tags.

2. **Direct PDF Output**: Unlike Fluid which only produces HTML strings, IronPDF directly generates PDF documents. You don't need to manage an intermediate HTML file or integrate a separate PDF library.

3. **CSS and JavaScript Support**: IronPDF uses a Chrome rendering engine, providing superior CSS3 and JavaScript support compared to most PDF generators. Ensure your HTML/CSS is valid for best results.

4. **Licensing**: IronPDF requires a license for commercial use, while Fluid is open-source. Test thoroughly with the free trial before deploying.

5. **Template Compilation**: Fluid pre-compiles templates for performance. With IronPDF, consider caching generated HTML strings if you're rendering the same template structure repeatedly with different data.

6. **Error Handling**: Fluid's template parsing errors occur at parse-time. With IronPDF, HTML rendering issues may only surface during PDF generation, so implement appropriate error handling around `RenderHtmlAsPdf()` calls.

7. **Complex Templates**: For very complex templating needs, consider using Razor Pages or another C#-native templating engine alongside IronPDF rather than reimplementing Liquid-style logic in raw C#.

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)