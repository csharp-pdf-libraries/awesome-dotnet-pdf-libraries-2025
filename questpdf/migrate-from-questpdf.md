# How Do I Migrate from QuestPDF to IronPDF in C#?

## Why Migrate from QuestPDF to IronPDF

**QuestPDF is often recommended for HTML-to-PDF conversion, but it doesn't support HTML at all.** Despite being heavily promoted on Reddit and developer forums, QuestPDF uses its own proprietary layout language that requires learning an entirely new DSL instead of leveraging existing web skills. This fundamental limitation makes it the wrong choice for most PDF generation scenarios.

### The Core Problem: No HTML Support

| Feature | QuestPDF | IronPDF |
|---------|----------|---------|
| **HTML-to-PDF** | ❌ **NOT SUPPORTED** | ✅ Full support |
| **CSS Styling** | ❌ **NOT SUPPORTED** | ✅ Full CSS3 |
| **Existing Templates** | ❌ Must rebuild from scratch | ✅ Reuse HTML/CSS assets |
| **Design Tool Compatibility** | ❌ None | ✅ Any web design tool |
| **Learning Curve** | New proprietary DSL | Web skills transfer |
| **Layout Preview** | ❌ Requires IDE plugin | ✅ Preview in any browser |
| **True Open Source** | ❌ Free for devs, paid for clients | ✅ Simple commercial license |

---

## The QuestPDF Licensing Trap

QuestPDF markets itself as "free" and "open source," but this is misleading:

### The Reality

1. **Free Tier Limitations**: QuestPDF's "Community License" is only free if your company has less than $1 million in annual gross revenue
2. **Client Impact**: Your clients (not just you as a developer) may need to purchase licenses if they exceed revenue thresholds
3. **License Auditing**: Unlike a simple per-developer commercial license, QuestPDF's model requires revenue disclosure and compliance tracking
4. **iText-Style Licensing**: This mirrors the problematic iText/iTextSharp licensing model that caused headaches for many organizations

### IronPDF's Simple Licensing

- One license per developer
- No revenue audits
- No client licensing requirements
- Clear, predictable costs
- License once, deploy anywhere

---

## The Proprietary Language Problem

QuestPDF forces you to learn a custom C# fluent API instead of using standard web technologies:

### QuestPDF's Approach (Proprietary DSL)
```csharp
// You must learn QuestPDF's custom fluent API
container.Page(page =>
{
    page.Content().Column(column =>
    {
        column.Item().Text("Invoice").Bold().FontSize(24);
        column.Item().Row(row =>
        {
            row.RelativeItem().Text("Customer:");
            row.RelativeItem().Text("Acme Corp");
        });
    });
});
```

**Problems:**
- Cannot visualize output without building and running code
- Requires QuestPDF Previewer plugin for Visual Studio or JetBrains IDEs
- No syntax highlighting for layout structure in standard editors
- Cannot reuse existing HTML/CSS templates
- Design changes require C# code changes
- Non-developers (designers) cannot contribute to templates

### IronPDF's Approach (Standard HTML/CSS)
```csharp
// Use standard HTML/CSS - preview in any browser
var html = @"
<html>
<head>
    <style>
        h1 { font-size: 24px; font-weight: bold; }
        .row { display: flex; justify-content: space-between; }
    </style>
</head>
<body>
    <h1>Invoice</h1>
    <div class='row'>
        <span>Customer:</span>
        <span>Acme Corp</span>
    </div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
```

**Benefits:**
- Preview in any web browser instantly
- Use existing HTML/CSS skills
- Designers can create and modify templates
- Use any HTML templating engine (Razor, Handlebars, etc.)
- Leverage CSS frameworks (Bootstrap, Tailwind)
- Debug layouts with browser dev tools

---

## No Design Asset Reuse

One of QuestPDF's most significant limitations is that it cannot reuse existing design assets:

| Asset Type | QuestPDF | IronPDF |
|------------|----------|---------|
| **HTML Email Templates** | ❌ Must rebuild | ✅ Direct use |
| **Website Stylesheets** | ❌ Must rebuild | ✅ Direct use |
| **Bootstrap/Tailwind** | ❌ Not supported | ✅ Full support |
| **Figma/Sketch Exports** | ❌ Not supported | ✅ Export to HTML |
| **Existing Reports** | ❌ Must rebuild | ✅ Keep HTML versions |
| **Design System Components** | ❌ Must rebuild | ✅ Reuse directly |

**The Real Cost**: Every design change in QuestPDF requires a C# developer to modify code. With IronPDF, designers can update HTML/CSS templates independently.

---

## The IDE Plugin Requirement

QuestPDF requires installing a special plugin to preview your PDF layouts:

### Without the Plugin
- You must build and run your code to see output
- Trial and error for positioning and styling
- No visual feedback during development
- Slow iteration cycle

### With IronPDF (No Plugin Required)
- Open HTML in any browser to preview
- Use browser dev tools for debugging
- Instant visual feedback
- Fast iteration with hot reload

---

## NuGet Package Changes

```bash
# Remove QuestPDF
dotnet remove package QuestPDF

# Add IronPDF
dotnet add package IronPdf
```

---

## Namespace Mapping

| QuestPDF | IronPDF |
|----------|---------|
| `QuestPDF.Fluent` | `IronPdf` |
| `QuestPDF.Helpers` | Not needed (use CSS) |
| `QuestPDF.Infrastructure` | `IronPdf` |
| N/A (no HTML support) | `IronPdf.Rendering` |

---

## API Mapping

| QuestPDF Concept | IronPDF Equivalent | Notes |
|------------------|-------------------|-------|
| `Document.Create()` | `new ChromePdfRenderer()` | Renderer creation |
| `.Page()` | `RenderHtmlAsPdf()` | Renders HTML to PDF |
| `.Text()` | HTML `<p>`, `<h1>`, `<span>` | Standard HTML tags |
| `.Bold()` | CSS `font-weight: bold` | Standard CSS |
| `.FontSize(24)` | CSS `font-size: 24px` | Standard CSS |
| `.Image()` | HTML `<img src="...">` | Standard HTML |
| `.Table()` | HTML `<table>` | Standard HTML |
| `.Column()` | CSS `display: flex; flex-direction: column` | CSS Flexbox |
| `.Row()` | CSS `display: flex; flex-direction: row` | CSS Flexbox |
| `.PageSize()` | `RenderingOptions.PaperSize` | Paper dimensions |
| `.Margin()` | `RenderingOptions.Margin*` | Page margins |
| `.GeneratePdf()` | `pdf.SaveAs()` | File output |
| `.GeneratePdfStream()` | `pdf.BinaryData` or `pdf.Stream` | Memory output |
| N/A | `PdfDocument.Merge()` | Merge PDFs |
| N/A | `PdfDocument.FromFile()` | Load existing PDFs |
| N/A | `pdf.SecuritySettings` | PDF encryption |
| N/A | `pdf.Sign()` | Digital signatures |

---

## Code Examples

### Example 1: Simple Invoice

**Before (QuestPDF) - Proprietary DSL:**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// Must learn QuestPDF's fluent API
// Cannot preview without IDE plugin or running code
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);

        page.Header().Text("Invoice #12345").FontSize(24).Bold();

        page.Content().Column(column =>
        {
            column.Spacing(10);
            column.Item().Text("Date: 2024-01-15");
            column.Item().Text("Customer: Acme Corp");

            column.Item().PaddingTop(20).Text("Items:").Bold();

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Cell().Text("Description").Bold();
                table.Cell().Text("Qty").Bold();
                table.Cell().Text("Price").Bold();

                table.Cell().Text("Widget A");
                table.Cell().Text("10");
                table.Cell().Text("$100");

                table.Cell().Text("Widget B");
                table.Cell().Text("5");
                table.Cell().Text("$75");
            });

            column.Item().PaddingTop(20).AlignRight().Text("Total: $175.00").FontSize(18).Bold();
        });
    });
}).GeneratePdf("invoice.pdf");
```

**After (IronPDF) - Standard HTML/CSS:**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Standard HTML - preview in any browser!
string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 2cm;
        }
        h1 {
            font-size: 24px;
            font-weight: bold;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }
        th, td {
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
        }
        th {
            background-color: #f2f2f2;
            font-weight: bold;
        }
        .total {
            font-size: 18px;
            font-weight: bold;
            text-align: right;
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <h1>Invoice #12345</h1>
    <p>Date: 2024-01-15</p>
    <p>Customer: Acme Corp</p>

    <table>
        <thead>
            <tr>
                <th>Description</th>
                <th>Qty</th>
                <th>Price</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>Widget A</td>
                <td>10</td>
                <td>$100</td>
            </tr>
            <tr>
                <td>Widget B</td>
                <td>5</td>
                <td>$75</td>
            </tr>
        </tbody>
    </table>

    <p class='total'>Total: $175.00</p>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

### Example 2: Complex Layout with Flexbox

**Before (QuestPDF) - Complex nested DSL:**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.Letter);

        page.Content().Column(mainColumn =>
        {
            // Header row
            mainColumn.Item().Row(header =>
            {
                header.RelativeItem(2).Column(left =>
                {
                    left.Item().Text("COMPANY NAME").Bold().FontSize(20);
                    left.Item().Text("123 Business Street");
                    left.Item().Text("City, State 12345");
                });

                header.RelativeItem(1).AlignRight().Column(right =>
                {
                    right.Item().Text("INVOICE").Bold().FontSize(24);
                    right.Item().Text("Invoice #: INV-001");
                    right.Item().Text("Date: 2024-01-15");
                });
            });

            // Divider
            mainColumn.Item().PaddingVertical(20).LineHorizontal(1);

            // Content columns
            mainColumn.Item().Row(content =>
            {
                content.RelativeItem().Column(col =>
                {
                    col.Item().Text("Bill To:").Bold();
                    col.Item().Text("Customer Name");
                    col.Item().Text("Customer Address");
                });

                content.RelativeItem().Column(col =>
                {
                    col.Item().Text("Ship To:").Bold();
                    col.Item().Text("Shipping Name");
                    col.Item().Text("Shipping Address");
                });
            });
        });
    });
}).GeneratePdf("complex-invoice.pdf");
```

**After (IronPDF) - Clean CSS Flexbox:**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 2cm;
        }
        .header {
            display: flex;
            justify-content: space-between;
        }
        .company-name {
            font-size: 20px;
            font-weight: bold;
        }
        .invoice-title {
            font-size: 24px;
            font-weight: bold;
            text-align: right;
        }
        .divider {
            border-top: 1px solid #333;
            margin: 20px 0;
        }
        .addresses {
            display: flex;
            gap: 40px;
        }
        .address-block {
            flex: 1;
        }
        .address-block h3 {
            font-weight: bold;
            margin-bottom: 5px;
        }
    </style>
</head>
<body>
    <div class='header'>
        <div>
            <div class='company-name'>COMPANY NAME</div>
            <div>123 Business Street</div>
            <div>City, State 12345</div>
        </div>
        <div>
            <div class='invoice-title'>INVOICE</div>
            <div>Invoice #: INV-001</div>
            <div>Date: 2024-01-15</div>
        </div>
    </div>

    <div class='divider'></div>

    <div class='addresses'>
        <div class='address-block'>
            <h3>Bill To:</h3>
            <div>Customer Name</div>
            <div>Customer Address</div>
        </div>
        <div class='address-block'>
            <h3>Ship To:</h3>
            <div>Shipping Name</div>
            <div>Shipping Address</div>
        </div>
    </div>
</body>
</html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("complex-invoice.pdf");
```

### Example 3: Dynamic Data with Templating

**Before (QuestPDF) - C# loops in DSL:**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;

var items = GetOrderItems(); // Returns list of items

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Content().Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                // Header
                table.Cell().Text("Product").Bold();
                table.Cell().Text("Qty").Bold();
                table.Cell().Text("Price").Bold();

                // Data rows
                foreach (var item in items)
                {
                    table.Cell().Text(item.Name);
                    table.Cell().Text(item.Quantity.ToString());
                    table.Cell().Text(item.Price.ToString("C"));
                }
            });
        });
    });
}).GeneratePdf("order.pdf");
```

**After (IronPDF) - Use any templating engine (Razor, Handlebars, etc.):**
```csharp
using IronPdf;
using System.Text;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var items = GetOrderItems();

// Simple string interpolation (or use Razor, Handlebars, etc.)
var rowsHtml = new StringBuilder();
foreach (var item in items)
{
    rowsHtml.AppendLine($@"
        <tr>
            <td>{item.Name}</td>
            <td>{item.Quantity}</td>
            <td>{item.Price:C}</td>
        </tr>");
}

string html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        table {{ width: 100%; border-collapse: collapse; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; }}
        th {{ background-color: #f2f2f2; font-weight: bold; }}
    </style>
</head>
<body>
    <table>
        <thead>
            <tr>
                <th>Product</th>
                <th>Qty</th>
                <th>Price</th>
            </tr>
        </thead>
        <tbody>
            {rowsHtml}
        </tbody>
    </table>
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("order.pdf");
```

### Example 4: Headers and Footers with Page Numbers

**Before (QuestPDF):**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);

        page.Header().Text("Company Report").FontSize(18).Bold();

        page.Content().Text("Report content here...");

        page.Footer().AlignCenter().Text(text =>
        {
            text.Span("Page ");
            text.CurrentPageNumber();
            text.Span(" of ");
            text.TotalPages();
        });
    });
}).GeneratePdf("report.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 35;
renderer.RenderingOptions.MarginBottom = 25;

// HTML header
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='font-size: 18px; font-weight: bold; text-align: center;'>
            Company Report
        </div>",
    DrawDividerLine = true
};

// HTML footer with page numbers
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='text-align: center; font-size: 10px;'>
            Page {page} of {total-pages}
        </div>",
    DrawDividerLine = true
};

string html = "<p>Report content here...</p>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report.pdf");
```

### Example 5: Images

**Before (QuestPDF) - Complex image handling:**
```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Content().Column(column =>
        {
            column.Item().Text("Document with Image");

            // From file - requires loading bytes
            column.Item().Image("logo.png");

            // Or from bytes
            byte[] imageBytes = File.ReadAllBytes("chart.png");
            column.Item().Image(imageBytes);
        });
    });
}).GeneratePdf("document.pdf");
```

**After (IronPDF) - Standard HTML images:**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

string html = @"
<!DOCTYPE html>
<html>
<body>
    <h1>Document with Image</h1>

    <!-- From file -->
    <img src='logo.png' alt='Logo' />

    <!-- From URL -->
    <img src='https://example.com/chart.png' alt='Chart' />

    <!-- From Base64 -->
    <img src='data:image/png;base64,iVBORw0KGgo...' alt='Embedded' />
</body>
</html>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("document.pdf");
```

### Example 6: PDF Manipulation (NOT Possible in QuestPDF)

**QuestPDF:** Cannot merge, split, or edit existing PDFs.

**IronPDF:**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Merge PDFs
var pdf1 = PdfDocument.FromFile("cover.pdf");
var pdf2 = PdfDocument.FromFile("content.pdf");
var pdf3 = PdfDocument.FromFile("appendix.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("complete.pdf");

// Split PDFs
var source = PdfDocument.FromFile("large-document.pdf");
var chapter1 = source.CopyPages(0, 9);  // Pages 1-10
var chapter2 = source.CopyPages(10, 19); // Pages 11-20
chapter1.SaveAs("chapter1.pdf");
chapter2.SaveAs("chapter2.pdf");

// Add watermark
var document = PdfDocument.FromFile("document.pdf");
document.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>DRAFT</h1>");
document.SaveAs("watermarked.pdf");
```

### Example 7: PDF Security (NOT Possible in QuestPDF)

**QuestPDF:** Cannot encrypt or secure PDFs.

**IronPDF:**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential Document</h1>");

// Password protection
pdf.SecuritySettings.OwnerPassword = "admin123";
pdf.SecuritySettings.UserPassword = "user123";

// Restrict permissions
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrinting;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

pdf.SaveAs("secure.pdf");
```

### Example 8: Digital Signatures (NOT Possible in QuestPDF)

**QuestPDF:** Cannot digitally sign PDFs.

**IronPDF:**
```csharp
using IronPdf;
using IronPdf.Signing;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Contract</h1>");

var signature = new PdfSignature("certificate.pfx", "password")
{
    SigningContact = "legal@company.com",
    SigningLocation = "New York, USA",
    SigningReason = "Contract Approval"
};

pdf.Sign(signature);
pdf.SaveAs("signed-contract.pdf");
```

---

## Common Gotchas

### 1. **QuestPDF Has NO HTML Support**
QuestPDF cannot convert HTML to PDF. Period. Every reference to "HTML-to-PDF" with QuestPDF on Reddit or forums is incorrect. If you need HTML-to-PDF, QuestPDF is the wrong choice.

### 2. **Proprietary DSL Learning Curve**
QuestPDF requires learning a completely new layout language. Your existing HTML/CSS knowledge doesn't transfer. With IronPDF, web developers are productive immediately.

### 3. **No Visual Preview Without Plugin**
Without the QuestPDF Previewer plugin for Visual Studio or JetBrains, you must compile and run code to see output. With IronPDF, open your HTML in any browser.

### 4. **Designers Cannot Contribute**
QuestPDF templates require C# code changes. With IronPDF's HTML approach, designers can create and modify templates independently.

### 5. **Revenue-Based Licensing**
QuestPDF's free tier has revenue thresholds. Your clients may unexpectedly need licenses. IronPDF uses simple per-developer licensing with no client impact.

### 6. **Page Breaks**
- **QuestPDF:** `.PageBreak()` method
- **IronPDF:** CSS `page-break-before: always` or `page-break-after: always`

### 7. **No PDF Manipulation**
QuestPDF can only create PDFs. It cannot load, merge, split, edit, secure, or sign existing PDFs. IronPDF does all of this.

### 8. **No URL-to-PDF**
QuestPDF cannot render websites to PDF. IronPDF can:
```csharp
var pdf = renderer.RenderUrlAsPdf("https://example.com");
```

---

## Find All QuestPDF References

```bash
# Find all QuestPDF usages in your codebase
grep -r "QuestPDF\|Document.Create\|\.GeneratePdf" --include="*.cs" .
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Identify all QuestPDF document templates**
  ```bash
  # Find all QuestPDF document definitions
  grep -r "Document.Create\|IDocument\|QuestPDF" --include="*.cs" .
  ```
  **Why:** Understand the scope of conversion work needed.

- [ ] **Document the DSL patterns used**
  ```csharp
  // QuestPDF DSL patterns to identify:
  .Column()     // → CSS flex-direction: column
  .Row()        // → CSS flex-direction: row
  .Table()      // → HTML <table>
  .Text()       // → HTML <p>, <span>, <h1>, etc.
  .Image()      // → HTML <img>
  .Container()  // → HTML <div>
  .Page()       // → Page-level CSS
  ```
  **Why:** Each DSL pattern has a corresponding HTML/CSS equivalent.

### Package Changes

- [ ] **Remove QuestPDF NuGet package**
  ```bash
  dotnet remove package QuestPDF
  ```
  **Why:** Avoid licensing complications and dependency bloat.

- [ ] **Install IronPDF**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Complete PDF solution with no revenue-based licensing.

- [ ] **Add license initialization**
  ```csharp
  // Program.cs - one-time setup
  IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE_KEY");
  ```
  **Why:** Required for production; no revenue calculation needed.

### Template Conversion

- [ ] **Convert QuestPDF DSL to HTML/CSS**
  ```csharp
  // Before (QuestPDF DSL)
  Document.Create(container =>
  {
      container.Page(page =>
      {
          page.Content().Column(column =>
          {
              column.Item().Text("Invoice").Bold().FontSize(24);
              column.Item().Text($"Customer: {customer.Name}");
          });
      });
  }).GeneratePdf("invoice.pdf");

  // After (IronPDF with HTML)
  var html = $@"
  <div style='font-family: Arial; padding: 40px;'>
      <h1 style='font-weight: bold; font-size: 24px;'>Invoice</h1>
      <p>Customer: {customer.Name}</p>
  </div>";

  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("invoice.pdf");
  ```
  **Why:** HTML/CSS is the universal document format with better tooling.

- [ ] **Replace Column/Row with CSS Flexbox**
  ```csharp
  // Before (QuestPDF)
  container.Row(row =>
  {
      row.RelativeItem().Text("Left");
      row.RelativeItem().Text("Center");
      row.RelativeItem().Text("Right");
  });

  // After (IronPDF)
  var html = @"
  <div style='display: flex; justify-content: space-between;'>
      <div>Left</div>
      <div>Center</div>
      <div>Right</div>
  </div>";
  ```
  **Why:** Flexbox provides identical layout capabilities.

- [ ] **Replace Table DSL with HTML tables**
  ```csharp
  // Before (QuestPDF)
  container.Table(table =>
  {
      table.ColumnsDefinition(columns =>
      {
          columns.RelativeColumn();
          columns.ConstantColumn(100);
          columns.RelativeColumn();
      });
      table.Header(header =>
      {
          header.Cell().Text("Product");
          header.Cell().Text("Qty");
          header.Cell().Text("Price");
      });
      foreach (var item in items)
      {
          table.Cell().Text(item.Name);
          table.Cell().Text(item.Quantity.ToString());
          table.Cell().Text(item.Price.ToString("C"));
      }
  });

  // After (IronPDF)
  var rows = items.Select(i => $"<tr><td>{i.Name}</td><td>{i.Quantity}</td><td>{i.Price:C}</td></tr>");
  var html = $@"
  <table style='border-collapse: collapse; width: 100%;'>
      <thead>
          <tr style='background: #f0f0f0;'>
              <th>Product</th>
              <th style='width: 100px;'>Qty</th>
              <th>Price</th>
          </tr>
      </thead>
      <tbody>
          {string.Join("", rows)}
      </tbody>
  </table>";
  ```
  **Why:** HTML tables are more familiar and have better tool support.

- [ ] **Convert styling to CSS**
  ```csharp
  // Before (QuestPDF)
  .Text("Title")
      .FontSize(24)
      .Bold()
      .FontColor(Colors.Blue.Darken2)
      .Italic();

  // After (IronPDF)
  var html = @"<h1 style='font-size: 24px; font-weight: bold; color: #1565C0; font-style: italic;'>Title</h1>";
  ```
  **Why:** CSS is more expressive and familiar to web developers.

- [ ] **Convert page settings to RenderingOptions**
  ```csharp
  // Before (QuestPDF)
  page.Size(PageSizes.A4);
  page.Margin(1, Unit.Centimetre);

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.MarginTop = 10;  // mm
  renderer.RenderingOptions.MarginBottom = 10;
  renderer.RenderingOptions.MarginLeft = 10;
  renderer.RenderingOptions.MarginRight = 10;
  ```
  **Why:** IronPDF page settings are more intuitive.

DSL-to-HTML conversion patterns and licensing model comparisons are thoroughly documented in the [extensive migration walkthrough](https://ironpdf.com/blog/migration-guides/migrate-from-questpdf-to-ironpdf/).

### Testing

- [ ] **Visual comparison of PDF output**
  ```csharp
  // Generate comparison PDFs
  var testCases = new[] { "invoice", "report", "receipt" };
  foreach (var template in testCases)
  {
      var html = LoadTemplate(template);
      var pdf = renderer.RenderHtmlAsPdf(html);
      pdf.SaveAs($"comparison/{template}_ironpdf.pdf");
  }
  ```
  **Why:** Ensure layouts match or exceed QuestPDF quality.

- [ ] **Test multi-page documents**
  ```csharp
  var longContent = string.Join("", Enumerable.Repeat("<p>Paragraph content...</p>", 100));
  var pdf = renderer.RenderHtmlAsPdf($"<body>{longContent}</body>");
  Console.WriteLine($"Generated {pdf.PageCount} pages");
  ```
  **Why:** Verify page breaks work correctly.

### Post-Migration Benefits

- [ ] **Enable PDF merging (new capability)**
  ```csharp
  var cover = renderer.RenderHtmlAsPdf("<h1>Cover</h1>");
  var content = renderer.RenderHtmlAsPdf(reportHtml);
  var existing = PdfDocument.FromFile("appendix.pdf");

  var merged = PdfDocument.Merge(cover, content, existing);
  merged.SaveAs("complete.pdf");
  ```
  **Why:** QuestPDF cannot merge existing PDFs.

- [ ] **Add PDF security (new capability)**
  ```csharp
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SecuritySettings.OwnerPassword = "admin";
  pdf.SecuritySettings.UserPassword = "reader";
  pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
  pdf.SaveAs("protected.pdf");
  ```
  **Why:** QuestPDF has no security features.

- [ ] **Enable URL-to-PDF (new capability)**
  ```csharp
  var renderer = new ChromePdfRenderer();
  renderer.RenderingOptions.EnableJavaScript = true;
  renderer.RenderingOptions.WaitFor.RenderDelay(2000);

  var pdf = renderer.RenderUrlAsPdf("https://example.com/report");
  pdf.SaveAs("webpage.pdf");
  ```
  **Why:** Capture web pages directly—impossible with QuestPDF.
---

## Why IronPDF is the Right Choice

| Consideration | QuestPDF | IronPDF |
|---------------|----------|---------|
| **HTML-to-PDF** | ❌ Not supported | ✅ Primary feature |
| **Learning Curve** | Proprietary DSL | Standard web skills |
| **Template Preview** | Plugin required | Any browser |
| **Design Collaboration** | Developers only | Designers + Developers |
| **Existing Assets** | Must rebuild | Reuse HTML/CSS |
| **PDF Manipulation** | ❌ Not supported | ✅ Full support |
| **Security/Signing** | ❌ Not supported | ✅ Full support |
| **Licensing Model** | Revenue-based | Per-developer |
| **Client Impact** | May need licenses | None |

---

## Additional Resources

- **IronPDF Documentation:** https://ironpdf.com/docs/
- **Tutorials and Examples:** https://ironpdf.com/tutorials/
- **HTML to PDF Guide:** https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference:** https://ironpdf.com/object-reference/api/
