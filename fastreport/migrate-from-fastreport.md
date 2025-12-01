# How Do I Migrate from FastReport.NET to IronPDF in C#?

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

### The FastReport.NET Challenges

FastReport.NET is a powerful reporting tool, but it comes with significant limitations for modern PDF generation:

1. **Report Designer Dependency**: Creating complex layouts requires the visual designer or deep knowledge of .frx file structure—not suitable for code-first development
2. **Steep Learning Curve**: FastReport's band-based architecture (DataBand, PageHeaderBand, etc.) requires understanding report-specific concepts
3. **Limited CSS Support**: Web-standard styling isn't natively supported; styling is done through FastReport's proprietary format
4. **Complex Data Binding**: RegisterData() and DataSource connections add boilerplate for simple PDF generation
5. **Fragmented Packages**: Multiple NuGet packages (FastReport.OpenSource, FastReport.OpenSource.Export.PdfSimple, etc.) needed for full functionality
6. **Licensing Complexity**: Open source version has limited features; commercial version required for PDF encryption, digital signing, and font embedding

### Benefits of IronPDF

| Aspect | FastReport.NET | IronPDF |
|--------|----------------|---------|
| Design Approach | Visual designer + .frx files | HTML/CSS (web technologies) |
| Learning Curve | Steep (band-based concepts) | Gentle (HTML/CSS knowledge) |
| Data Binding | RegisterData(), DataBand | String interpolation, Razor, templating engines |
| CSS Support | Limited | Full CSS3 with Flexbox/Grid |
| Package Model | Multiple packages | Single package (all features) |
| Rendering Engine | Custom | Latest Chromium |
| PDF Manipulation | Export-focused | Full manipulation (merge, split, security, forms) |
| Modern .NET | .NET Standard 2.0 | .NET 6/7/8/9+ native |

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
git commit -m "Backup before FastReport.NET to IronPDF migration"
```

### Identify All FastReport Usage

```bash
# Find all FastReport references
grep -r "FastReport\|\.frx\|PDFExport\|PDFSimpleExport\|DataBand\|RegisterData" --include="*.cs" --include="*.csproj" .
```

### Document Your Report Templates

Before migration, catalog all `.frx` files and their purposes:
- Report name and purpose
- Data sources used
- Headers/footers configuration
- Page numbering requirements
- Special formatting or styling

---

## Quick Start Migration

### Step 1: Update NuGet Packages

```bash
# Remove all FastReport packages
dotnet remove package FastReport.OpenSource
dotnet remove package FastReport.OpenSource.Export.PdfSimple
dotnet remove package FastReport.OpenSource.Web
dotnet remove package FastReport.OpenSource.Data.MsSql

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using FastReport;
using FastReport.Export.Pdf;
using FastReport.Export.PdfSimple;
using FastReport.Data;
using FastReport.Utils;

// After
using IronPdf;
using IronPdf.Rendering;
```

### Step 3: Initialize IronPDF

```csharp
// Set license key at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Optional: Configure logging
IronPdf.Logging.Logger.LoggingMode = IronPdf.Logging.Logger.LoggingModes.Custom;
```

### Step 4: Basic Conversion Pattern

```csharp
// Before (FastReport.NET)
using (Report report = new Report())
{
    report.Load("report.frx");
    report.RegisterData(dataSet, "MyData");
    report.Prepare();

    using (var export = new PDFSimpleExport())
    {
        report.Export(export, "output.pdf");
    }
}

// After (IronPDF)
var renderer = new ChromePdfRenderer();
var html = GenerateHtmlFromData(dataSet);  // Your templating logic
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("output.pdf");
```

---

## Complete API Reference

### Namespace Mapping

| FastReport.NET Namespace | IronPDF Equivalent | Notes |
|--------------------------|-------------------|-------|
| `FastReport` | `IronPdf` | Main namespace |
| `FastReport.Utils` | `IronPdf.Rendering` | Rendering utilities |
| `FastReport.Export.Pdf` | `IronPdf` | PDF export (built-in) |
| `FastReport.Export.PdfSimple` | `IronPdf` | Simplified export (built-in) |
| `FastReport.Data` | N/A | Use standard .NET data access |
| `FastReport.Engine` | N/A | Not needed (direct rendering) |
| `FastReport.Matrix` | HTML `<table>` | Use HTML tables |
| `FastReport.Barcode` | IronBarCode (separate) | Or use HTML/SVG barcodes |

### Core Class Mapping

| FastReport.NET Class | IronPDF Equivalent | Notes |
|---------------------|-------------------|-------|
| `Report` | `ChromePdfRenderer` | Main rendering class |
| `PDFExport` | `ChromePdfRenderer` + `SecuritySettings` | Rendering + security |
| `PDFSimpleExport` | `ChromePdfRenderer` | Simplified export |
| `ReportPage` | HTML `<body>` or `<div>` | Page content |
| `TextObject` | HTML `<p>`, `<span>`, `<div>` | Text elements |
| `TableObject` | HTML `<table>` | Tables |
| `DataBand` | Loop in template | Data iteration |
| `PageHeaderBand` | `HtmlHeaderFooter` | Page headers |
| `PageFooterBand` | `HtmlHeaderFooter` | Page footers |
| `HTMLObject` | Direct HTML rendering | HTML content |
| `PictureObject` | HTML `<img>` | Images |
| `ShapeObject` | HTML/SVG/CSS | Shapes and borders |
| `LineObject` | CSS border/HR | Lines |
| `BarcodeObject` | IronBarCode or SVG | Barcodes |
| `CheckBoxObject` | HTML `<input type="checkbox">` | Checkboxes |
| `ZipCodeObject` | CSS-styled elements | Postal codes |

### Report Class Methods

| FastReport Method | IronPDF Equivalent | Notes |
|-------------------|-------------------|-------|
| `report.Load(path)` | Read HTML template file | Or generate HTML in code |
| `report.Load(stream)` | Read HTML from stream | `File.ReadAllText()` or stream |
| `report.RegisterData(data, name)` | Direct data binding in HTML | String interpolation/Razor |
| `report.RegisterData(ds, name, maxNesting)` | Nested loop in templating | Handle hierarchy manually |
| `report.GetDataSource(name)` | N/A | Use .NET collections directly |
| `report.Prepare()` | N/A | Not needed (direct rendering) |
| `report.Export(export, path)` | `pdf.SaveAs(path)` | Save to file |
| `report.Export(export, stream)` | `pdf.Stream` or `pdf.BinaryData` | Get as stream/bytes |
| `report.FindObject(name)` | N/A | Use DOM or template logic |
| `report.Designer` | N/A | Use HTML/CSS editors |
| `report.Refresh()` | Re-render with new data | Create new `PdfDocument` |
| `report.Print()` | `pdf.Print()` | Print to default printer |
| `report.PrintWithDialog()` | `pdf.Print()` with dialog | Configure print dialog |
| `report.Dispose()` | `pdf.Dispose()` (optional) | IDisposable support |

### PDFExport Class Properties

| FastReport PDFExport Property | IronPDF Equivalent | Notes |
|------------------------------|-------------------|-------|
| `Author` | `pdf.MetaData.Author` | Document author |
| `Title` | `pdf.MetaData.Title` | Document title |
| `Subject` | `pdf.MetaData.Subject` | Document subject |
| `Keywords` | `pdf.MetaData.Keywords` | Document keywords |
| `Creator` | `pdf.MetaData.Creator` | Creator application |
| `Producer` | `pdf.MetaData.Producer` | Producer name |
| `OwnerPassword` | `pdf.SecuritySettings.OwnerPassword` | Owner password |
| `UserPassword` | `pdf.SecuritySettings.UserPassword` | User password |
| `AllowPrint` | `pdf.SecuritySettings.AllowUserPrinting` | Print permission |
| `AllowCopy` | `pdf.SecuritySettings.AllowUserCopyPasteContent` | Copy permission |
| `AllowModify` | `pdf.SecuritySettings.AllowUserEdits` | Modify permission |
| `AllowAnnotate` | `pdf.SecuritySettings.AllowUserAnnotations` | Annotate permission |
| `Compressed` | Always compressed | IronPDF compresses by default |
| `Background` | `RenderingOptions.PrintHtmlBackgrounds` | Background rendering |
| `Outline` | `pdf.BookMarks` | PDF outline/bookmarks |
| `Transparency` | Automatic | Handled automatically |
| `HideToolbar` | `pdf.SecuritySettings.AllowUserFormData` | Viewer settings |
| `CenterWindow` | Via JavaScript in HTML | Viewer settings |
| `PdfCompliance` | `RenderingOptions.PdfA` | PDF/A compliance |
| `DigitalSignCertificate` | `pdf.SignWithFile()` | Digital signatures |

### Rendering Options

| FastReport Option | IronPDF Equivalent | Notes |
|-------------------|-------------------|-------|
| Page Size | `RenderingOptions.PaperSize` | Standard sizes or custom |
| Page Orientation | `RenderingOptions.PaperOrientation` | Portrait/Landscape |
| Margins | `RenderingOptions.MarginTop/Bottom/Left/Right` | In mm |
| First Page Number | `RenderingOptions.FirstPageNumber` | Starting page number |
| Zoom | `RenderingOptions.Zoom` | Default zoom level |
| Print Background | `RenderingOptions.PrintHtmlBackgrounds` | Background colors/images |
| Custom Paper Size | `RenderingOptions.SetCustomPaperSize()` | Custom dimensions |

### Page Number Placeholders

| FastReport Placeholder | IronPDF Equivalent | Notes |
|-----------------------|-------------------|-------|
| `[Page]` | `{page}` | Current page number |
| `[TotalPages]` | `{total-pages}` | Total page count |
| `[Page#]` | `{page}` | Alternate syntax |
| `[TotalPages#]` | `{total-pages}` | Alternate syntax |
| `[PageN]` | `{page}` | Variant |
| `[PageNofM]` | `{page} of {total-pages}` | Combined |
| `[Date]` | `{date}` or JavaScript | Current date |
| `[Time]` | `{time}` or JavaScript | Current time |

---

## Code Examples

### Example 1: Simple HTML to PDF

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.PdfSimple;
using System.IO;

class Program
{
    static void Main()
    {
        using (Report report = new Report())
        {
            // Create HTML object
            FastReport.HTMLObject htmlObject = new FastReport.HTMLObject();
            htmlObject.Name = "HtmlContent";
            htmlObject.Width = 500;
            htmlObject.Height = 300;
            htmlObject.Text = "<h1>Hello World</h1><p>This is a PDF document.</p>";

            // Add to page
            ReportPage page = new ReportPage();
            page.Name = "Page1";
            page.ReportTitle = new ReportTitleBand();
            page.ReportTitle.Height = 300;
            page.ReportTitle.Objects.Add(htmlObject);
            report.Pages.Add(page);

            report.Prepare();

            using (var export = new PDFSimpleExport())
            {
                export.Export(report, "output.pdf");
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>This is a PDF document.</p>");
        pdf.SaveAs("output.pdf");
    }
}
```

### Example 2: Report with Data Binding

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.PdfSimple;
using System.Data;

class Program
{
    static void Main()
    {
        // Create sample data
        DataTable products = new DataTable("Products");
        products.Columns.Add("Name", typeof(string));
        products.Columns.Add("Price", typeof(decimal));
        products.Columns.Add("Quantity", typeof(int));

        products.Rows.Add("Widget A", 29.99m, 100);
        products.Rows.Add("Widget B", 49.99m, 50);
        products.Rows.Add("Widget C", 19.99m, 200);

        using (Report report = new Report())
        {
            // Load template with DataBand configuration
            report.Load("products.frx");

            // Register the data source
            report.RegisterData(products, "Products");

            // Enable the data source
            report.GetDataSource("Products").Enabled = true;

            // Prepare the report
            report.Prepare();

            // Export to PDF
            using (var export = new PDFSimpleExport())
            {
                report.Export(export, "products.pdf");
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Text;

class Program
{
    static void Main()
    {
        // Create sample data
        var products = new[]
        {
            new { Name = "Widget A", Price = 29.99m, Quantity = 100 },
            new { Name = "Widget B", Price = 49.99m, Quantity = 50 },
            new { Name = "Widget C", Price = 19.99m, Quantity = 200 }
        };

        // Build HTML with data
        var html = new StringBuilder();
        html.Append(@"
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; padding: 20px; }
                    h1 { color: #333; }
                    table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                    th { background-color: #4CAF50; color: white; padding: 12px; text-align: left; }
                    td { border: 1px solid #ddd; padding: 10px; }
                    tr:nth-child(even) { background-color: #f2f2f2; }
                    .price { text-align: right; }
                    .quantity { text-align: center; }
                </style>
            </head>
            <body>
                <h1>Product Catalog</h1>
                <table>
                    <thead>
                        <tr><th>Product</th><th>Price</th><th>Quantity</th></tr>
                    </thead>
                    <tbody>");

        foreach (var product in products)
        {
            html.Append($@"
                <tr>
                    <td>{product.Name}</td>
                    <td class='price'>${product.Price:F2}</td>
                    <td class='quantity'>{product.Quantity}</td>
                </tr>");
        }

        html.Append(@"
                    </tbody>
                </table>
            </body>
            </html>");

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html.ToString());
        pdf.SaveAs("products.pdf");
    }
}
```

### Example 3: Headers and Footers with Page Numbers

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.PdfSimple;

class Program
{
    static void Main()
    {
        using (Report report = new Report())
        {
            report.Load("template.frx");

            // Access the page
            ReportPage page = report.Pages[0] as ReportPage;

            // Configure page header
            if (page.PageHeader == null)
            {
                page.PageHeader = new PageHeaderBand();
            }
            page.PageHeader.Height = 50;

            TextObject headerText = new TextObject();
            headerText.Name = "HeaderText";
            headerText.Bounds = new RectangleF(0, 0, Units.Millimeters * 190, Units.Millimeters * 10);
            headerText.Text = "Company Report - Confidential";
            headerText.HorzAlign = HorzAlign.Center;
            headerText.Font = new Font("Arial", 12, FontStyle.Bold);
            page.PageHeader.Objects.Add(headerText);

            // Configure page footer
            if (page.PageFooter == null)
            {
                page.PageFooter = new PageFooterBand();
            }
            page.PageFooter.Height = 50;

            TextObject footerText = new TextObject();
            footerText.Name = "FooterText";
            footerText.Bounds = new RectangleF(0, 0, Units.Millimeters * 190, Units.Millimeters * 10);
            footerText.Text = "Page [Page] of [TotalPages]";
            footerText.HorzAlign = HorzAlign.Right;
            page.PageFooter.Objects.Add(footerText);

            report.Prepare();

            using (var export = new PDFSimpleExport())
            {
                report.Export(export, "report.pdf");
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();

        // Configure header
        renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
        {
            HtmlFragment = @"
                <div style='text-align: center; font-family: Arial; font-size: 12pt; font-weight: bold;'>
                    Company Report - Confidential
                </div>",
            DrawDividerLine = true
        };

        // Configure footer with page numbers
        renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
        {
            HtmlFragment = @"
                <div style='text-align: right; font-family: Arial; font-size: 10pt;'>
                    Page {page} of {total-pages}
                </div>",
            DrawDividerLine = true
        };

        var html = @"
            <html>
            <body>
                <h1>Report Content</h1>
                <p>This is the main report content with headers and footers.</p>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report.pdf");
    }
}
```

### Example 4: PDF Security and Encryption

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.Pdf;

class Program
{
    static void Main()
    {
        using (Report report = new Report())
        {
            report.Load("secure_report.frx");
            report.Prepare();

            // Configure PDF export with security
            PDFExport export = new PDFExport();

            // Set document metadata
            export.Title = "Confidential Report";
            export.Author = "Company Name";
            export.Subject = "Financial Data";
            export.Keywords = "finance, report, confidential";

            // Set passwords
            export.OwnerPassword = "owner123";
            export.UserPassword = "user456";

            // Set permissions
            export.AllowPrint = true;
            export.AllowCopy = false;
            export.AllowModify = false;
            export.AllowAnnotate = false;

            report.Export(export, "secure_report.pdf");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(@"
            <h1>Confidential Report</h1>
            <p>This document contains sensitive financial information.</p>");

        // Set document metadata
        pdf.MetaData.Title = "Confidential Report";
        pdf.MetaData.Author = "Company Name";
        pdf.MetaData.Subject = "Financial Data";
        pdf.MetaData.Keywords = "finance, report, confidential";

        // Configure security settings
        pdf.SecuritySettings.OwnerPassword = "owner123";
        pdf.SecuritySettings.UserPassword = "user456";

        // Set permissions
        pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
        pdf.SecuritySettings.AllowUserCopyPasteContent = false;
        pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;
        pdf.SecuritySettings.AllowUserAnnotations = false;

        pdf.SaveAs("secure_report.pdf");
    }
}
```

### Example 5: Multi-Page Report with Page Breaks

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.PdfSimple;

class Program
{
    static void Main()
    {
        using (Report report = new Report())
        {
            // Load multi-page template
            report.Load("multi_page.frx");

            // FastReport handles page breaks through bands and DataBand properties
            // PageBreak can be set in the designer or programmatically
            DataBand dataBand = report.FindObject("Data1") as DataBand;
            if (dataBand != null)
            {
                dataBand.StartNewPage = true;  // Each record on new page
            }

            report.RegisterData(GetReportData(), "ReportData");
            report.Prepare();

            using (var export = new PDFSimpleExport())
            {
                report.Export(export, "multi_page_report.pdf");
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Text;

class Program
{
    static void Main()
    {
        var sections = new[] { "Introduction", "Chapter 1", "Chapter 2", "Conclusion" };

        var html = new StringBuilder();
        html.Append(@"
            <html>
            <head>
                <style>
                    body { font-family: Georgia, serif; margin: 40px; }
                    h1 { color: #2c3e50; page-break-before: always; }
                    h1:first-of-type { page-break-before: avoid; }
                    p { line-height: 1.6; }
                    .no-break { page-break-inside: avoid; }
                </style>
            </head>
            <body>");

        foreach (var section in sections)
        {
            html.Append($@"
                <h1>{section}</h1>
                <p class='no-break'>
                    This is the content for {section}. Lorem ipsum dolor sit amet,
                    consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut
                    labore et dolore magna aliqua.
                </p>");
        }

        html.Append("</body></html>");

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html.ToString());
        pdf.SaveAs("multi_page_report.pdf");
    }
}
```

### Example 6: URL to PDF Conversion

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.PdfSimple;
using System.Net.Http;
using System.IO;

class Program
{
    static async Task Main()
    {
        // FastReport doesn't natively support URL to PDF
        // Must download HTML first
        string htmlContent;
        using (var client = new HttpClient())
        {
            htmlContent = await client.GetStringAsync("https://example.com");
        }

        using (Report report = new Report())
        {
            ReportPage page = new ReportPage();
            page.Name = "Page1";

            ReportTitleBand titleBand = new ReportTitleBand();
            titleBand.Height = 800;  // Large height for web content

            FastReport.HTMLObject htmlObject = new FastReport.HTMLObject();
            htmlObject.Width = 800;
            htmlObject.Height = 1000;
            htmlObject.Text = htmlContent;

            titleBand.Objects.Add(htmlObject);
            page.ReportTitle = titleBand;
            report.Pages.Add(page);

            report.Prepare();

            using (var export = new PDFSimpleExport())
            {
                report.Export(export, "webpage.pdf");
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();

        // Configure for web page rendering
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PrintHtmlBackgrounds = true;
        renderer.RenderingOptions.Timeout = 60;  // Wait for JS to load
        renderer.RenderingOptions.WaitFor.RenderDelay(2000);  // Wait for dynamic content

        var pdf = renderer.RenderUrlAsPdf("https://example.com");
        pdf.SaveAs("webpage.pdf");
    }
}
```

### Example 7: Merging Multiple Reports

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.Pdf;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // FastReport doesn't have built-in merge
        // Export each report separately, then merge with another library
        var pdfPaths = new List<string>();

        foreach (var reportFile in new[] { "report1.frx", "report2.frx", "report3.frx" })
        {
            using (Report report = new Report())
            {
                report.Load(reportFile);
                report.Prepare();

                string tempPath = Path.GetTempFileName() + ".pdf";
                using (var export = new PDFExport())
                {
                    report.Export(export, tempPath);
                }
                pdfPaths.Add(tempPath);
            }
        }

        // Use third-party library to merge
        MergePdfsWithThirdParty(pdfPaths, "merged_report.pdf");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();

        // Generate multiple PDFs
        var pdf1 = renderer.RenderHtmlAsPdf("<h1>Report 1</h1><p>First report content</p>");
        var pdf2 = renderer.RenderHtmlAsPdf("<h1>Report 2</h1><p>Second report content</p>");
        var pdf3 = renderer.RenderHtmlAsPdf("<h1>Report 3</h1><p>Third report content</p>");

        // Merge directly - no third-party library needed
        var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
        merged.SaveAs("merged_report.pdf");

        // Or merge from files
        var fromFile1 = PdfDocument.FromFile("existing1.pdf");
        var fromFile2 = PdfDocument.FromFile("existing2.pdf");
        var mergedFromFiles = PdfDocument.Merge(fromFile1, fromFile2);
        mergedFromFiles.SaveAs("merged_from_files.pdf");
    }
}
```

### Example 8: Nested Data with Master-Detail

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.PdfSimple;
using System.Data;

class Program
{
    static void Main()
    {
        // Create master-detail data structure
        DataSet ds = new DataSet();

        // Orders table (master)
        DataTable orders = new DataTable("Orders");
        orders.Columns.Add("OrderID", typeof(int));
        orders.Columns.Add("CustomerName", typeof(string));
        orders.Columns.Add("OrderDate", typeof(DateTime));
        orders.Rows.Add(1, "John Smith", DateTime.Now);
        orders.Rows.Add(2, "Jane Doe", DateTime.Now.AddDays(-1));
        ds.Tables.Add(orders);

        // OrderDetails table (detail)
        DataTable details = new DataTable("OrderDetails");
        details.Columns.Add("OrderID", typeof(int));
        details.Columns.Add("Product", typeof(string));
        details.Columns.Add("Quantity", typeof(int));
        details.Rows.Add(1, "Widget A", 5);
        details.Rows.Add(1, "Widget B", 3);
        details.Rows.Add(2, "Widget C", 10);
        ds.Tables.Add(details);

        // Create relation
        ds.Relations.Add("OrderDetails", orders.Columns["OrderID"], details.Columns["OrderID"]);

        using (Report report = new Report())
        {
            // Load template with DataBand and nested DataBand
            report.Load("master_detail.frx");
            report.RegisterData(ds, "MyData", 2);  // maxNestingLevel = 2
            report.Prepare();

            using (var export = new PDFSimpleExport())
            {
                report.Export(export, "master_detail.pdf");
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        // Create master-detail data structure
        var orders = new List<Order>
        {
            new Order {
                OrderID = 1,
                CustomerName = "John Smith",
                OrderDate = DateTime.Now,
                Details = new List<OrderDetail>
                {
                    new OrderDetail { Product = "Widget A", Quantity = 5 },
                    new OrderDetail { Product = "Widget B", Quantity = 3 }
                }
            },
            new Order {
                OrderID = 2,
                CustomerName = "Jane Doe",
                OrderDate = DateTime.Now.AddDays(-1),
                Details = new List<OrderDetail>
                {
                    new OrderDetail { Product = "Widget C", Quantity = 10 }
                }
            }
        };

        var html = new StringBuilder();
        html.Append(@"
            <html>
            <head>
                <style>
                    body { font-family: Arial; padding: 20px; }
                    .order { margin-bottom: 30px; border: 1px solid #ccc; padding: 15px; }
                    .order-header { background: #f5f5f5; padding: 10px; margin: -15px -15px 15px -15px; }
                    table { width: 100%; border-collapse: collapse; }
                    th { background: #4CAF50; color: white; padding: 8px; text-align: left; }
                    td { border: 1px solid #ddd; padding: 8px; }
                </style>
            </head>
            <body>
                <h1>Order Report</h1>");

        foreach (var order in orders)
        {
            html.Append($@"
                <div class='order'>
                    <div class='order-header'>
                        <strong>Order #{order.OrderID}</strong> - {order.CustomerName}
                        <br/>Date: {order.OrderDate:yyyy-MM-dd}
                    </div>
                    <table>
                        <tr><th>Product</th><th>Quantity</th></tr>");

            foreach (var detail in order.Details)
            {
                html.Append($"<tr><td>{detail.Product}</td><td>{detail.Quantity}</td></tr>");
            }

            html.Append("</table></div>");
        }

        html.Append("</body></html>");

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html.ToString());
        pdf.SaveAs("master_detail.pdf");
    }
}

class Order
{
    public int OrderID { get; set; }
    public string CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderDetail> Details { get; set; }
}

class OrderDetail
{
    public string Product { get; set; }
    public int Quantity { get; set; }
}
```

### Example 9: PDF with Embedded Images

**Before (FastReport.NET):**
```csharp
using FastReport;
using FastReport.Export.PdfSimple;
using System.Drawing;

class Program
{
    static void Main()
    {
        using (Report report = new Report())
        {
            ReportPage page = new ReportPage();
            page.Name = "Page1";

            ReportTitleBand titleBand = new ReportTitleBand();
            titleBand.Height = 300;

            // Add picture object
            PictureObject picture = new PictureObject();
            picture.Name = "Logo";
            picture.Bounds = new RectangleF(0, 0, 200, 100);
            picture.Image = Image.FromFile("company_logo.png");
            picture.SizeMode = PictureBoxSizeMode.Zoom;

            titleBand.Objects.Add(picture);

            // Add text below image
            TextObject text = new TextObject();
            text.Name = "Title";
            text.Bounds = new RectangleF(0, 110, 400, 30);
            text.Text = "Company Report";
            text.Font = new Font("Arial", 18, FontStyle.Bold);

            titleBand.Objects.Add(text);
            page.ReportTitle = titleBand;
            report.Pages.Add(page);

            report.Prepare();

            using (var export = new PDFSimpleExport())
            {
                report.Export(export, "report_with_image.pdf");
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System;
using System.IO;

class Program
{
    static void Main()
    {
        // Option 1: Reference local image file
        var html = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; padding: 20px; }
                    .logo { max-width: 200px; margin-bottom: 20px; }
                    h1 { font-size: 18pt; font-weight: bold; }
                </style>
            </head>
            <body>
                <img src='company_logo.png' class='logo' />
                <h1>Company Report</h1>
                <p>Report content here...</p>
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.BaseUrl = new Uri(Directory.GetCurrentDirectory());
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report_with_image.pdf");

        // Option 2: Embed image as Base64
        byte[] imageBytes = File.ReadAllBytes("company_logo.png");
        string base64Image = Convert.ToBase64String(imageBytes);

        var htmlWithBase64 = $@"
            <html>
            <body>
                <img src='data:image/png;base64,{base64Image}' style='max-width: 200px;' />
                <h1>Company Report</h1>
            </body>
            </html>";

        var pdf2 = renderer.RenderHtmlAsPdf(htmlWithBase64);
        pdf2.SaveAs("report_with_embedded_image.pdf");
    }
}
```

### Example 10: Interactive Forms (PDF Forms)

**Before (FastReport.NET):**
```csharp
// FastReport.NET has limited support for interactive PDF forms
// Typically requires the commercial version for form field creation
using FastReport;
using FastReport.Export.Pdf;

class Program
{
    static void Main()
    {
        using (Report report = new Report())
        {
            report.Load("form_template.frx");
            report.Prepare();

            PDFExport export = new PDFExport();
            // Form fields would need to be defined in the template
            // Limited programmatic form creation

            report.Export(export, "form.pdf");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        // Create PDF with form fields using HTML
        var html = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; padding: 30px; }
                    .form-group { margin-bottom: 15px; }
                    label { display: block; font-weight: bold; margin-bottom: 5px; }
                    input[type='text'], textarea {
                        width: 100%;
                        padding: 8px;
                        border: 1px solid #ccc;
                        box-sizing: border-box;
                    }
                    textarea { height: 100px; }
                </style>
            </head>
            <body>
                <h1>Application Form</h1>
                <div class='form-group'>
                    <label>Full Name:</label>
                    <input type='text' name='fullName' />
                </div>
                <div class='form-group'>
                    <label>Email:</label>
                    <input type='text' name='email' />
                </div>
                <div class='form-group'>
                    <label>Comments:</label>
                    <textarea name='comments'></textarea>
                </div>
            </body>
            </html>";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.CreatePdfFormsFromHtml = true;

        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("interactive_form.pdf");

        // Or work with existing form fields
        var existingPdf = PdfDocument.FromFile("existing_form.pdf");

        // Get form field by name
        var nameField = existingPdf.Form.GetFieldByName("fullName");
        if (nameField != null)
        {
            nameField.Value = "John Smith";
        }

        existingPdf.SaveAs("filled_form.pdf");
    }
}
```

---

## Advanced Scenarios

### Using Razor Templates for Complex Reports

For complex data-driven reports, use Razor templating:

```csharp
using IronPdf;
using RazorLight;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Set up Razor engine
        var engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Templates"))
            .UseMemoryCachingProvider()
            .Build();

        // Prepare data model
        var model = new ReportModel
        {
            Title = "Sales Report",
            Date = DateTime.Now,
            Items = new List<SalesItem>
            {
                new SalesItem { Product = "Widget A", Quantity = 100, Price = 29.99m },
                new SalesItem { Product = "Widget B", Quantity = 50, Price = 49.99m }
            }
        };

        // Render template to HTML
        string html = await engine.CompileRenderAsync("SalesReport.cshtml", model);

        // Convert to PDF
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("sales_report.pdf");
    }
}
```

### Batch Report Generation

```csharp
using IronPdf;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var customers = GetCustomers();  // Your data source
        var renderer = new ChromePdfRenderer();

        var tasks = new List<Task>();

        foreach (var customer in customers)
        {
            tasks.Add(Task.Run(() =>
            {
                var html = GenerateInvoiceHtml(customer);
                var pdf = renderer.RenderHtmlAsPdf(html);
                pdf.SaveAs($"invoices/invoice_{customer.Id}.pdf");
            }));
        }

        await Task.WhenAll(tasks);
    }
}
```

### Converting Existing .frx Templates

If you have existing FastReport templates, consider these approaches:

1. **Manual Conversion**: Recreate the layout in HTML/CSS
2. **Export to Image**: Export FastReport output as images, then embed in IronPDF
3. **Gradual Migration**: Keep FastReport for complex reports while using IronPDF for new development

```csharp
// Hybrid approach during migration
public class ReportService
{
    private readonly ChromePdfRenderer _ironPdf;

    public byte[] GenerateReport(string reportType, object data)
    {
        return reportType switch
        {
            // New reports use IronPDF
            "invoice" => GenerateInvoiceWithIronPdf(data),
            "statement" => GenerateStatementWithIronPdf(data),

            // Legacy reports still use FastReport (temporarily)
            "legacy_complex_report" => GenerateWithFastReport(data),

            _ => throw new ArgumentException($"Unknown report type: {reportType}")
        };
    }
}
```

---

## Performance Considerations

### Memory Management

```csharp
// IronPDF manages resources efficiently, but dispose when done
using (var pdf = renderer.RenderHtmlAsPdf(html))
{
    pdf.SaveAs("output.pdf");
}  // Automatically disposed

// Or for multiple operations
var pdf = renderer.RenderHtmlAsPdf(html);
try
{
    // Multiple operations
    pdf.ApplyWatermark("<h2>DRAFT</h2>");
    pdf.SaveAs("draft.pdf");
}
finally
{
    pdf.Dispose();
}
```

### Caching Strategies

```csharp
// Cache rendered templates for repeated use
public class PdfCache
{
    private static readonly Dictionary<string, byte[]> _cache = new();

    public byte[] GetOrCreate(string key, Func<byte[]> factory)
    {
        if (!_cache.TryGetValue(key, out var pdf))
        {
            pdf = factory();
            _cache[key] = pdf;
        }
        return pdf;
    }
}
```

### Async Operations

```csharp
// IronPDF supports async for I/O operations
public async Task<byte[]> GeneratePdfAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Async save
    using var ms = new MemoryStream();
    await Task.Run(() => pdf.Stream.CopyTo(ms));
    return ms.ToArray();
}
```

---

## Troubleshooting

### Issue 1: Report Layout Doesn't Match

**Problem**: PDF output looks different from FastReport.

**Solution**: Use CSS to match the layout precisely:

```csharp
var html = @"
    <html>
    <head>
        <style>
            @page {
                size: A4;
                margin: 1cm;
            }
            body {
                font-family: 'Arial', sans-serif;
                font-size: 10pt;
                line-height: 1.4;
            }
            /* Match FastReport's positioning with CSS Grid or absolute positioning */
            .report-container {
                display: grid;
                grid-template-columns: repeat(12, 1fr);
            }
        </style>
    </head>
    <body>...</body>
    </html>";
```

### Issue 2: Data Binding Not Working

**Problem**: Data doesn't appear in the PDF.

**Solution**: Ensure data is embedded in HTML before rendering:

```csharp
// WRONG - placeholders won't be replaced
var html = "<p>Customer: {CustomerName}</p>";

// CORRECT - use string interpolation
var html = $"<p>Customer: {customer.Name}</p>";

// Or use Razor
var html = await razorEngine.CompileRenderAsync("template.cshtml", customer);
```

### Issue 3: Page Breaks in Wrong Places

**Problem**: Content breaks awkwardly between pages.

**Solution**: Use CSS page break properties:

```css
/* Force break before element */
.chapter { page-break-before: always; }

/* Prevent break inside element */
.table-row { page-break-inside: avoid; }

/* Force break after element */
.section { page-break-after: always; }
```

### Issue 4: Images Not Rendering

**Problem**: Images don't appear in the PDF.

**Solution**: Use absolute paths or Base64 encoding:

```csharp
// Set base URL for relative paths
renderer.RenderingOptions.BaseUrl = new Uri("file:///C:/MyProject/");

// Or use Base64
var base64 = Convert.ToBase64String(File.ReadAllBytes("image.png"));
var imgTag = $"<img src='data:image/png;base64,{base64}' />";
```

### Issue 5: Fonts Not Matching

**Problem**: PDF uses different fonts than expected.

**Solution**: Embed fonts or use web-safe fonts:

```html
<style>
    @import url('https://fonts.googleapis.com/css2?family=Roboto&display=swap');
    body { font-family: 'Roboto', Arial, sans-serif; }
</style>
```

### Issue 6: Headers/Footers Missing

**Problem**: Headers and footers don't appear.

**Solution**: Ensure proper HtmlHeaderFooter configuration:

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:center;'>Header</div>",
    DrawDividerLine = true,
    MaxHeight = 30  // Set explicit height in mm
};
```

### Issue 7: PDF Size Too Large

**Problem**: Output PDF is larger than FastReport version.

**Solution**: Optimize images and enable compression:

```csharp
// Compress images before embedding
var optimizedImage = OptimizeImage(originalImage, 150);  // 150 DPI

// IronPDF compresses by default
// For further optimization, use a PDF compression tool post-generation
```

### Issue 8: Performance Slower Than FastReport

**Problem**: PDF generation takes longer.

**Solution**: Use async rendering and caching:

```csharp
// Reuse renderer instances
private static readonly ChromePdfRenderer _renderer = new ChromePdfRenderer();

// Pre-warm the renderer
static MyClass()
{
    _renderer.RenderHtmlAsPdf("<html></html>");  // First render is slowest
}

// Use parallel processing for batch jobs
await Parallel.ForEachAsync(items, async (item, ct) =>
{
    var pdf = _renderer.RenderHtmlAsPdf(GenerateHtml(item));
    await Task.Run(() => pdf.SaveAs($"output_{item.Id}.pdf"));
});
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all `.frx` report files**
  ```bash
  find . -name "*.frx"
  ```
  **Why:** Identify all report templates to ensure complete migration coverage.

- [ ] **Document data sources and bindings for each report**
  ```csharp
  // Before (FastReport)
  report.RegisterData(dataSet, "Data");

  // Document data sources for HTML templating
  ```
  **Why:** Understanding data bindings is crucial for converting to HTML-based data rendering.

- [ ] **List all FastReport NuGet packages in use**
  ```bash
  dotnet list package | grep FastReport
  ```
  **Why:** Ensure all dependencies are removed and replaced with IronPDF.

- [ ] **Backup project to version control**
  ```bash
  git add .
  git commit -m "Backup before migration"
  ```
  **Why:** Safeguard current state before making significant changes.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Package Migration

- [ ] **Remove all FastReport packages**
  ```bash
  dotnet remove package FastReport.OpenSource
  dotnet remove package FastReport.OpenSource.Export.PdfSimple
  ```
  **Why:** Clean removal of old dependencies to avoid conflicts.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to enable modern PDF generation features.

- [ ] **Update using statements in all files**
  ```csharp
  // Before (FastReport)
  using FastReport;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Update namespaces to reflect the new library usage.

- [ ] **Set IronPDF license key at startup**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

### Code Migration

- [ ] **Convert Report.Load() to HTML templating**
  ```csharp
  // Before (FastReport)
  report.Load("report.frx");

  // After (IronPDF)
  var htmlTemplate = "<html><body>Your HTML content here</body></html>";
  ```
  **Why:** Transition to HTML for flexible, web-standard document design.

- [ ] **Replace RegisterData() with data-bound HTML generation**
  ```csharp
  // Before (FastReport)
  report.RegisterData(dataSet, "Data");

  // After (IronPDF)
  var htmlContent = $"<html><body>{GenerateHtmlFromData(dataSet)}</body></html>";
  ```
  **Why:** Use HTML and C# string interpolation for dynamic content.

- [ ] **Convert TextObject/TableObject to HTML elements**
  ```csharp
  // Before (FastReport)
  var textObject = new TextObject();

  // After (IronPDF)
  var htmlElement = "<p>Your text here</p>";
  ```
  **Why:** HTML provides a more flexible and powerful way to define document content.

- [ ] **Replace PDFExport settings with IronPDF equivalents**
  ```csharp
  // Before (FastReport)
  var pdfExport = new PDFExport();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** Use IronPDF's modern rendering options for better PDF output.

- [ ] **Update page number placeholders ([Page] → {page})**
  ```csharp
  // Before (FastReport)
  "[Page]"

  // After (IronPDF)
  "{page}"
  ```
  **Why:** IronPDF uses HTML-based placeholders for dynamic content.

- [ ] **Migrate headers/footers to HtmlHeaderFooter**
  ```csharp
  // Before (FastReport)
  report.PageHeader = "Header text";

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Header text</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers, allowing for more customization.

- [ ] **Convert security settings**
  ```csharp
  // Before (FastReport)
  pdfExport.SecurityOptions = new SecurityOptions();

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf(htmlContent);
  pdf.SecuritySettings.UserPassword = "password";
  ```
  **Why:** IronPDF provides straightforward security settings for PDF protection.

- [ ] **Update merge/combine logic**
  ```csharp
  // Before (FastReport)
  report1.Merge(report2);

  // After (IronPDF)
  var mergedPdf = PdfDocument.Merge(pdf1, pdf2);
  ```
  **Why:** IronPDF offers robust PDF manipulation capabilities.

### Testing

- [ ] **Verify output matches expected layout**
  **Why:** Ensure the new PDFs maintain the intended design and structure.

- [ ] **Test with production data volumes**
  **Why:** Validate performance and correctness with real-world data sizes.

- [ ] **Validate headers, footers, and page numbers**
  **Why:** Confirm dynamic content like page numbers render correctly.

- [ ] **Test security/encryption features**
  **Why:** Ensure PDFs are properly protected as intended.

- [ ] **Check image rendering**
  **Why:** Verify that images are displayed correctly in the new PDFs.

- [ ] **Verify form fields (if applicable)**
  **Why:** Ensure interactive elements function as expected.

- [ ] **Performance testing**
  **Why:** Confirm that PDF generation is efficient and meets performance requirements.

### Post-Migration

- [ ] **Remove unused .frx template files**
  ```bash
  find . -name "*.frx" -delete
  ```
  **Why:** Clean up obsolete files to reduce clutter.

- [ ] **Delete FastReport-related code**
  ```bash
  // Remove all FastReport code references
  ```
  **Why:** Eliminate unnecessary code to streamline the codebase.

- [ ] **Update documentation**
  **Why:** Ensure all documentation reflects the new IronPDF implementation.

- [ ] **Train team on IronPDF patterns**
  **Why:** Equip the team with knowledge to effectively use IronPDF features.

- [ ] **Monitor for issues in production**
  **Why:** Proactively address any issues that arise after deployment.
---

## Additional Resources

- **IronPDF Documentation**: https://ironpdf.com/docs/
- **IronPDF Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF Guide**: https://ironpdf.com/how-to/html-file-to-pdf/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Code Examples**: https://ironpdf.com/examples/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection.*
