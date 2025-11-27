# Migration Guide: CraftMyPDF → IronPDF

## Why Migrate to IronPDF

IronPDF eliminates vendor lock-in by allowing you to generate PDFs directly from HTML, CSS, and JavaScript without template restrictions. Unlike CraftMyPDF's cloud-only architecture, IronPDF runs entirely on-premise, giving you complete control over your data and eliminating recurring API costs. With IronPDF, you pay once and own the license perpetually, avoiding the burden of monthly subscriptions.

## NuGet Package Changes

```bash
# Remove CraftMyPDF
dotnet remove package CraftMyPDF

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| CraftMyPDF | IronPDF |
|------------|---------|
| `CraftMyPdf` | `IronPdf` |
| `CraftMyPdf.Client` | `IronPdf` |
| `CraftMyPdf.Models` | `IronPdf` |

## API Mapping Table

| CraftMyPDF | IronPDF | Notes |
|------------|---------|-------|
| `new PdfClient(apiKey)` | `ChromePdfRenderer` | No API key needed for on-premise |
| `client.CreatePdfFromTemplate()` | `renderer.RenderHtmlAsPdf()` | Use standard HTML instead of templates |
| `template.SetData()` | Direct HTML string manipulation | Pass data directly in HTML |
| `template.AddImage()` | `<img>` tags in HTML | Use standard HTML image tags |
| `client.CreatePdfFromUrl()` | `renderer.RenderUrlAsPdf()` | Direct URL to PDF conversion |
| `SetPageSize()` | `RenderingOptions.PaperSize` | Set before rendering |
| `SetOrientation()` | `RenderingOptions.PaperOrientation` | Portrait or Landscape |

## Before/After Code Examples

### Example 1: Basic PDF Generation from Template/HTML

**Before (CraftMyPDF):**
```csharp
using CraftMyPdf;

var client = new PdfClient("your-api-key");
var template = await client.GetTemplate("template-id");

template.SetData(new {
    name = "John Doe",
    amount = "$1,000"
});

var pdf = await client.CreatePdfFromTemplate(template);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

var html = $@"
    <h1>Invoice</h1>
    <p>Name: John Doe</p>
    <p>Amount: $1,000</p>
";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

### Example 2: PDF from URL

**Before (CraftMyPDF):**
```csharp
using CraftMyPdf;

var client = new PdfClient("your-api-key");

var options = new UrlToPdfOptions {
    Url = "https://example.com",
    PageSize = "A4",
    Orientation = "Portrait"
};

var pdf = await client.CreatePdfFromUrl(options);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 3: Advanced Template with Images and Styling

**Before (CraftMyPDF):**
```csharp
using CraftMyPdf;

var client = new PdfClient("your-api-key");
var template = await client.GetTemplate("invoice-template-id");

template.SetData(new {
    invoiceNumber = "INV-001",
    items = new[] {
        new { name = "Product A", price = 50 },
        new { name = "Product B", price = 75 }
    }
});

template.AddImage("logo", logoBytes);
var pdf = await client.CreatePdfFromTemplate(template);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

var items = new[] {
    new { name = "Product A", price = 50 },
    new { name = "Product B", price = 75 }
};

var itemsHtml = string.Join("", items.Select(i => 
    $"<tr><td>{i.name}</td><td>${i.price}</td></tr>"));

var html = $@"
    <html>
    <head>
        <style>
            table {{ border-collapse: collapse; width: 100%; }}
            td, th {{ border: 1px solid #ddd; padding: 8px; }}
        </style>
    </head>
    <body>
        <img src='data:image/png;base64,{Convert.ToBase64String(logoBytes)}' />
        <h1>Invoice INV-001</h1>
        <table>
            <tr><th>Item</th><th>Price</th></tr>
            {itemsHtml}
        </table>
    </body>
    </html>
";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

## Common Gotchas

### 1. No API Key Required
IronPDF runs on-premise, so remove all API key configurations. Apply your license key once in app startup:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 2. Template Designer Not Needed
You no longer need CraftMyPDF's template designer. Use any HTML editor or generate HTML programmatically. This gives you complete flexibility with standard web technologies.

### 3. Synchronous by Default
IronPDF operations are synchronous. Remove `await` keywords unless using async file I/O separately:
```csharp
// Not: var pdf = await renderer.RenderHtmlAsPdf(html);
var pdf = renderer.RenderHtmlAsPdf(html); // Correct
```

### 4. Image Embedding
Instead of API calls to add images, embed them directly in HTML using:
- Base64 encoding: `data:image/png;base64,...`
- File paths: `file:///C:/path/to/image.png`
- URLs: `https://example.com/image.png`

### 5. CSS and JavaScript Support
IronPDF renders using a full Chrome engine, so complex CSS (Flexbox, Grid) and JavaScript are fully supported. No need to simplify designs.

### 6. Headers and Footers
Set headers/footers using `RenderingOptions`:
```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter() {
    HtmlFragment = "<div style='text-align:right'>{page} of {total-pages}</div>"
};
```

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **Code Examples**: Extensive examples available in the tutorials section
- **Support**: Direct support included with license