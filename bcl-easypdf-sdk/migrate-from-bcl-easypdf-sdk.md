# How Do I Migrate from BCL EasyPDF SDK to IronPDF in C#?

## Table of Contents
1. [Why Migrate from BCL EasyPDF SDK to IronPDF](#why-migrate-from-bcl-easypdf-sdk-to-ironpdf)
2. [Before You Start](#before-you-start)
3. [Quick Start (5 Minutes)](#quick-start-5-minutes)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Migration Checklist](#migration-checklist)
10. [Additional Resources](#additional-resources)

---

## Why Migrate from BCL EasyPDF SDK to IronPDF

### The Problem with BCL EasyPDF SDK

BCL EasyPDF SDK relies on several problematic technologies that create deployment nightmares:

| Issue | Impact |
|-------|--------|
| **Windows-Only** | Cannot deploy to Linux, macOS, Docker, or cloud containers |
| **Microsoft Office Required** | Must install Office on every server for document conversion |
| **COM Interop** | DLL loading errors, crashes, and version conflicts |
| **Virtual Printer Driver** | Requires interactive user session on servers |
| **Legacy Architecture** | Limited .NET Core/.NET 5+ support |
| **Complex Installation** | MSI installers, GAC registration, registry modifications |

### Common BCL EasyPDF SDK Errors

Developers frequently encounter these issues:

- `bcl.easypdf.interop.easypdfprinter.dll error loading`
- `COM object that has been separated from its underlying RCW cannot be used`
- `Timeout expired waiting for print job to complete`
- `The printer operation failed because the service is not running`
- `Error: Access denied` (interactive session required)
- `Cannot find printer: BCL easyPDF Printer`

### IronPDF Advantages

| Feature | BCL EasyPDF SDK | IronPDF |
|---------|-----------------|---------|
| **Platform** | Windows-only | Windows, Linux, macOS, Docker |
| **Office Dependency** | Required | None |
| **Installation** | Complex MSI + drivers | Simple NuGet package |
| **HTML Rendering** | Basic (Office-based) | Full Chromium (CSS3, JS, Flexbox) |
| **Server Deployment** | Requires interactive session | Runs headless |
| **.NET Support** | Limited .NET Core | Full .NET 5/6/7/8/9 |
| **Async Support** | Callback-based | Native async/await |
| **Container Support** | None | Full Docker/Kubernetes |
| **Licensing** | Per-server | Per-developer |

---

## Before You Start

### Prerequisites

- **.NET Framework 4.6.2+** or **.NET Core 3.1+** or **.NET 5/6/7/8/9**
- **Visual Studio 2019+** or **VS Code** with C# extension
- **NuGet Package Manager** access
- **Existing BCL EasyPDF SDK codebase** to migrate

### Find All BCL EasyPDF SDK References

Before migrating, identify all BCL EasyPDF usage in your codebase:

```bash
# Find all BCL using statements
grep -r "using BCL" --include="*.cs" .

# Find Printer/PDFDocument usage
grep -r "Printer\|PDFDocument\|PDFConverter\|HTMLConverter" --include="*.cs" .

# Find COM interop references
grep -r "easyPDF\|BCL.easyPDF" --include="*.csproj" .

# Find configuration settings
grep -r "PageOrientation\|TimeOut\|PrintOffice" --include="*.cs" .
```

### Breaking Changes to Expect

| BCL EasyPDF Pattern | Change Required |
|---------------------|-----------------|
| `new Printer()` | Use `ChromePdfRenderer` |
| `PrintOfficeDocToPDF()` | Office conversion handled differently |
| `RenderHTMLToPDF()` | `RenderHtmlAsPdf()` |
| COM interop references | Remove entirely |
| Printer driver config | Not needed |
| `BeginPrintToFile()` callbacks | Native async/await |
| Interactive session requirements | Runs headless |

---

## Quick Start (5 Minutes)

### Step 1: Remove BCL EasyPDF SDK

BCL EasyPDF SDK is typically installed via:
- MSI installer
- Manual DLL references
- GAC registration

Remove all references:
1. Uninstall BCL EasyPDF SDK from Programs and Features
2. Remove DLL references from your project
3. Remove COM interop references
4. Clean up GAC entries if present

### Step 2: Install IronPDF

```bash
# Install IronPDF
dotnet add package IronPdf
```

Or via Package Manager Console:
```powershell
Install-Package IronPdf
```

### Step 3: Update Namespaces

```csharp
// ❌ Remove these
using BCL.easyPDF;
using BCL.easyPDF.Interop;
using BCL.easyPDF.PDFConverter;
using BCL.easyPDF.Printer;

// ✅ Add these
using IronPdf;
using IronPdf.Rendering;
```

### Step 4: Convert Your First PDF

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;
using BCL.easyPDF.Interop;

Printer printer = new Printer();
printer.Configuration.TimeOut = 120;

try
{
    printer.RenderHTMLToPDF("<h1>Hello</h1>", "output.pdf");
}
finally
{
    printer.Dispose();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.Timeout = 120000; // milliseconds

var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
pdf.SaveAs("output.pdf");
```

---

## Complete API Reference

### Namespace Mapping

| BCL EasyPDF Namespace | IronPDF Namespace | Purpose |
|-----------------------|-------------------|---------|
| `BCL.easyPDF` | `IronPdf` | Core functionality |
| `BCL.easyPDF.Interop` | `IronPdf` | Interop (not needed) |
| `BCL.easyPDF.PDFConverter` | `IronPdf` | PDF conversion |
| `BCL.easyPDF.Printer` | `IronPdf` | No printer needed |
| `BCL.easyPDF.Office` | N/A | No Office needed |

### Core Class Mapping

| BCL EasyPDF Class | IronPDF Equivalent | Notes |
|-------------------|-------------------|-------|
| `Printer` | `ChromePdfRenderer` | Main conversion class |
| `PDFDocument` | `PdfDocument` | Document manipulation |
| `HTMLConverter` | `ChromePdfRenderer` | HTML conversion |
| `PrinterConfiguration` | `ChromePdfRenderOptions` | Rendering options |
| `PageOrientation` | `PdfPaperOrientation` | Page orientation |
| `PageSize` | `PdfPaperSize` | Paper size |
| `SecurityHandler` | `PdfDocument.SecuritySettings` | Security options |
| `PDFProcessor` | `PdfDocument` | PDF processing |

### Complete Method Mapping

#### PDF Creation

| BCL EasyPDF Method | IronPDF Method | Notes |
|--------------------|----------------|-------|
| `printer.RenderHTMLToPDF(html, path)` | `renderer.RenderHtmlAsPdf(html).SaveAs(path)` | HTML string |
| `printer.RenderUrlToPDF(url, path)` | `renderer.RenderUrlAsPdf(url).SaveAs(path)` | URL |
| `printer.RenderFileToPDF(file, path)` | `renderer.RenderHtmlFileAsPdf(file).SaveAs(path)` | HTML file |
| `htmlConverter.ConvertHTML(html, doc)` | `renderer.RenderHtmlAsPdf(html)` | HTML to PDF |
| `htmlConverter.ConvertURL(url, doc)` | `renderer.RenderUrlAsPdf(url)` | URL to PDF |

#### Office Document Conversion

| BCL EasyPDF Method | IronPDF Method | Notes |
|--------------------|----------------|-------|
| `printer.PrintOfficeDocToPDF(doc, pdf)` | Use IronWord or HTML workflow | No Office needed |
| `printer.PrintWordToPDF()` | Convert to HTML first, then PDF | HTML-based workflow |
| `printer.PrintExcelToPDF()` | Use IronXL or HTML workflow | Consider IronXL |

*Note: For Office document conversion without Office, consider using IronWord for .docx, IronXL for .xlsx, or converting documents to HTML first.*

#### PDF Manipulation

| BCL EasyPDF Method | IronPDF Method | Notes |
|--------------------|----------------|-------|
| `doc.Append(doc2)` | `PdfDocument.Merge(pdf1, pdf2)` | Merge PDFs |
| `doc.ExtractPages(start, end)` | `pdf.CopyPages(start, end)` | Extract pages |
| `doc.DeletePage(index)` | `pdf.RemovePage(index)` | Remove page |
| `doc.RotatePage(index, angle)` | `pdf.RotatePage(index, angle)` | Rotate page |
| `doc.GetPageCount()` | `pdf.PageCount` | Page count |
| `doc.Save(path)` | `pdf.SaveAs(path)` | Save PDF |
| `doc.Close()` | `pdf.Dispose()` or `using` | Cleanup |

#### Configuration Options

| BCL EasyPDF Option | IronPDF Option | Notes |
|--------------------|----------------|-------|
| `config.TimeOut` | `RenderingOptions.Timeout` | Timeout (ms) |
| `config.PageOrientation = Landscape` | `RenderingOptions.PaperOrientation = Landscape` | Orientation |
| `config.PageSize = A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` | Paper size |
| `config.JobTitle` | `pdf.MetaData.Title` | Document title |
| `config.PageWidth` | `RenderingOptions.SetCustomPaperSize()` | Custom width |
| `config.PageHeight` | `RenderingOptions.SetCustomPaperSize()` | Custom height |
| `config.MarginTop/Bottom/Left/Right` | `RenderingOptions.MarginTop`, etc. | Margins |
| `config.BackgroundPrinting` | Always true | Background support |

#### Security and Metadata

| BCL EasyPDF Method | IronPDF Method | Notes |
|--------------------|----------------|-------|
| `doc.SetPassword(pwd)` | `pdf.SecuritySettings.UserPassword` | Password |
| `doc.SetOwnerPassword(pwd)` | `pdf.SecuritySettings.OwnerPassword` | Owner password |
| `doc.SetPrintPermission(bool)` | `pdf.SecuritySettings.AllowUserPrinting` | Print permission |
| `doc.SetCopyPermission(bool)` | `pdf.SecuritySettings.AllowUserCopyPasteContent` | Copy permission |
| `doc.SetTitle(title)` | `pdf.MetaData.Title` | Title |
| `doc.SetAuthor(author)` | `pdf.MetaData.Author` | Author |
| `doc.SetSubject(subject)` | `pdf.MetaData.Subject` | Subject |
| `doc.SetKeywords(keywords)` | `pdf.MetaData.Keywords` | Keywords |

#### Text Extraction

| BCL EasyPDF Method | IronPDF Method | Notes |
|--------------------|----------------|-------|
| `doc.ExtractText()` | `pdf.ExtractAllText()` | All text |
| `doc.ExtractTextFromPage(index)` | `pdf.ExtractTextFromPage(index)` | Per-page |

#### Licensing

| BCL EasyPDF Method | IronPDF Method | Notes |
|--------------------|----------------|-------|
| `Printer.SetLicenseKey(key)` | `IronPdf.License.LicenseKey = key` | Set license |
| License file path | Code-based only | No file needed |

---

## Code Migration Examples

### Example 1: HTML String to PDF

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;
using BCL.easyPDF.Interop;

class Program
{
    static void Main()
    {
        Printer printer = new Printer();

        try
        {
            // Set configuration
            printer.Configuration.TimeOut = 120;
            printer.Configuration.JobTitle = "Invoice";
            printer.Configuration.PageOrientation = PageOrientation.Portrait;
            printer.Configuration.PageSize = PageSize.Letter;

            string htmlContent = @"
                <html>
                <head>
                    <style>
                        body { font-family: Arial; }
                        h1 { color: navy; }
                    </style>
                </head>
                <body>
                    <h1>Invoice #12345</h1>
                    <p>Thank you for your order.</p>
                </body>
                </html>";

            printer.RenderHTMLToPDF(htmlContent, "invoice.pdf");
            Console.WriteLine("PDF created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            printer.Dispose();
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
        // Optional: Set license
        License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();

        // Set configuration
        renderer.RenderingOptions.Timeout = 120000;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;
        renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;

        string htmlContent = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; }
                    h1 { color: navy; }
                </style>
            </head>
            <body>
                <h1>Invoice #12345</h1>
                <p>Thank you for your order.</p>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.MetaData.Title = "Invoice";
        pdf.SaveAs("invoice.pdf");

        Console.WriteLine("PDF created successfully");
    }
}
```

### Example 2: URL to PDF with Landscape Layout

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;
using BCL.easyPDF.Interop;

Printer printer = new Printer();
printer.Configuration.PageOrientation = PageOrientation.Landscape;
printer.Configuration.PageSize = PageSize.A4;
printer.Configuration.TimeOut = 180;
printer.Configuration.BackgroundPrinting = false; // Often causes issues

try
{
    printer.RenderUrlToPDF("https://example.com/report", "report.pdf");
}
catch (Exception ex)
{
    // Common: timeout errors, session errors, printer not found
    Console.WriteLine($"Conversion failed: {ex.Message}");
}
finally
{
    printer.Dispose();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.Timeout = 180000;
renderer.RenderingOptions.PrintHtmlBackgrounds = true; // Always works

var pdf = renderer.RenderUrlAsPdf("https://example.com/report");
pdf.SaveAs("report.pdf");

Console.WriteLine("Report generated successfully");
```

### Example 3: Merge Multiple PDFs

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;

PDFDocument doc1 = new PDFDocument("document1.pdf");
PDFDocument doc2 = new PDFDocument("document2.pdf");
PDFDocument doc3 = new PDFDocument("document3.pdf");

try
{
    doc1.Append(doc2);
    doc1.Append(doc3);
    doc1.Save("merged.pdf");
}
finally
{
    doc1.Close();
    doc2.Close();
    doc3.Close();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("document1.pdf");
var pdf2 = PdfDocument.FromFile("document2.pdf");
var pdf3 = PdfDocument.FromFile("document3.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
merged.SaveAs("merged.pdf");

// Or from a collection
var files = new[] { "document1.pdf", "document2.pdf", "document3.pdf" };
var pdfs = files.Select(PdfDocument.FromFile).ToList();
var mergedFromList = PdfDocument.Merge(pdfs);
```

### Example 4: Password-Protected PDF

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;

PDFDocument doc = new PDFDocument();
Printer printer = new Printer();

try
{
    printer.RenderHTMLToPDF("<h1>Confidential</h1>", doc);

    // Set security
    doc.SetPassword("user123");
    doc.SetOwnerPassword("owner456");
    doc.SetPrintPermission(false);
    doc.SetCopyPermission(false);

    doc.Save("protected.pdf");
}
finally
{
    doc.Close();
    printer.Dispose();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential</h1>");

// Set security
pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.OwnerPassword = "owner456";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

pdf.SaveAs("protected.pdf");
```

### Example 5: Extract and Split Pages

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;

PDFDocument doc = new PDFDocument("large_document.pdf");

try
{
    // Extract first 5 pages
    PDFDocument extracted = doc.ExtractPages(1, 5);
    extracted.Save("first_5_pages.pdf");
    extracted.Close();

    // Split into individual pages
    int pageCount = doc.GetPageCount();
    for (int i = 1; i <= pageCount; i++)
    {
        PDFDocument singlePage = doc.ExtractPages(i, i);
        singlePage.Save($"page_{i}.pdf");
        singlePage.Close();
    }
}
finally
{
    doc.Close();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("large_document.pdf");

// Extract first 5 pages
var firstFive = pdf.CopyPages(0, 4); // 0-indexed
firstFive.SaveAs("first_5_pages.pdf");

// Split into individual pages
for (int i = 0; i < pdf.PageCount; i++)
{
    var singlePage = pdf.CopyPage(i);
    singlePage.SaveAs($"page_{i + 1}.pdf");
}
```

### Example 6: Text Extraction

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;

PDFDocument doc = new PDFDocument("document.pdf");

try
{
    // Extract all text
    string allText = doc.ExtractText();
    Console.WriteLine(allText);

    // Extract from specific page
    string pageText = doc.ExtractTextFromPage(1);
    Console.WriteLine($"Page 1: {pageText}");
}
finally
{
    doc.Close();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Extract all text
string allText = pdf.ExtractAllText();
Console.WriteLine(allText);

// Extract from specific page (0-indexed)
string pageText = pdf.ExtractTextFromPage(0);
Console.WriteLine($"Page 1: {pageText}");

// Extract from multiple pages
for (int i = 0; i < pdf.PageCount; i++)
{
    string text = pdf.ExtractTextFromPage(i);
    Console.WriteLine($"Page {i + 1}: {text}");
}
```

### Example 7: Async PDF Generation

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;
using BCL.easyPDF.Interop;

class Program
{
    static void Main()
    {
        Printer printer = new Printer();

        // BCL uses callback-based async
        printer.BeginPrintToFile(
            "https://example.com",
            "output.pdf",
            OnPrintComplete,
            OnPrintError
        );

        // Wait for completion...
        Console.ReadLine();
        printer.Dispose();
    }

    static void OnPrintComplete(string path)
    {
        Console.WriteLine($"PDF created: {path}");
    }

    static void OnPrintError(Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var renderer = new ChromePdfRenderer();

        // Native async/await
        var pdf = await renderer.RenderUrlAsPdfAsync("https://example.com");
        await pdf.SaveAsAsync("output.pdf");

        Console.WriteLine("PDF created: output.pdf");
    }
}
```

### Example 8: Custom Page Size and Margins

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;

Printer printer = new Printer();
printer.Configuration.PageWidth = 8.5;  // inches
printer.Configuration.PageHeight = 11;
printer.Configuration.MarginTop = 1;
printer.Configuration.MarginBottom = 1;
printer.Configuration.MarginLeft = 0.75;
printer.Configuration.MarginRight = 0.75;

printer.RenderHTMLToPDF(html, "custom.pdf");
printer.Dispose();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Custom paper size (in millimeters)
renderer.RenderingOptions.SetCustomPaperSizeinMilimeters(215.9, 279.4); // 8.5" x 11"

// Or use standard size
renderer.RenderingOptions.PaperSize = PdfPaperSize.Letter;

// Margins in millimeters
renderer.RenderingOptions.MarginTop = 25.4;    // 1 inch
renderer.RenderingOptions.MarginBottom = 25.4;
renderer.RenderingOptions.MarginLeft = 19.05;  // 0.75 inch
renderer.RenderingOptions.MarginRight = 19.05;

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("custom.pdf");
```

### Example 9: PDF Metadata

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;

PDFDocument doc = new PDFDocument();
Printer printer = new Printer();

printer.RenderHTMLToPDF("<h1>Document</h1>", doc);

doc.SetTitle("My Document");
doc.SetAuthor("John Doe");
doc.SetSubject("Important Report");
doc.SetKeywords("report, quarterly, sales");
doc.SetCreator("My Application");

doc.Save("with_metadata.pdf");
doc.Close();
printer.Dispose();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Document</h1>");

pdf.MetaData.Title = "My Document";
pdf.MetaData.Author = "John Doe";
pdf.MetaData.Subject = "Important Report";
pdf.MetaData.Keywords = "report, quarterly, sales";
pdf.MetaData.Creator = "My Application";
pdf.MetaData.Producer = "IronPDF";

pdf.SaveAs("with_metadata.pdf");
```

### Example 10: Headers and Footers

**Before (BCL EasyPDF SDK):**
```csharp
// BCL EasyPDF SDK doesn't have native header/footer support
// Headers/footers must be included in the source HTML
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// HTML header
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='text-align:center; font-size:12px; font-family:Arial;'>
            Company Name - Confidential
        </div>",
    DrawDividerLine = true,
    MaxHeight = 30
};

// HTML footer with page numbers
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='text-align:center; font-size:10px;'>
            Page {page} of {total-pages}
        </div>",
    DrawDividerLine = true,
    MaxHeight = 25
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Content</h1>");
pdf.SaveAs("with_headers.pdf");
```

---

## Advanced Scenarios

### ASP.NET Core Integration

**Before (BCL EasyPDF SDK in ASP.NET):**
```csharp
// BCL EasyPDF SDK doesn't work well in web contexts
// due to interactive session requirements
public class PdfController : Controller
{
    public IActionResult GeneratePdf()
    {
        // Often fails with "printer not found" or timeout errors
        Printer printer = new Printer();
        try
        {
            printer.RenderHTMLToPDF("<h1>Report</h1>", "temp.pdf");
            return File(System.IO.File.ReadAllBytes("temp.pdf"), "application/pdf");
        }
        finally
        {
            printer.Dispose();
        }
    }
}
```

**After (IronPDF in ASP.NET Core):**
```csharp
[ApiController]
[Route("[controller]")]
public class PdfController : ControllerBase
{
    [HttpGet("generate")]
    public IActionResult GeneratePdf()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1>");

        return File(pdf.BinaryData, "application/pdf", "report.pdf");
    }

    [HttpGet("generate-async")]
    public async Task<IActionResult> GeneratePdfAsync()
    {
        var renderer = new ChromePdfRenderer();
        var pdf = await renderer.RenderHtmlAsPdfAsync("<h1>Report</h1>");

        return File(pdf.Stream, "application/pdf", "report.pdf");
    }
}
```

### Docker Deployment

**Before (BCL EasyPDF SDK):**
```dockerfile
# BCL EasyPDF SDK CANNOT run in Docker containers
# It requires:
# - Windows container (not Linux)
# - Microsoft Office installed
# - Virtual printer driver
# - Interactive desktop session
# This is fundamentally incompatible with containerization
```

**After (IronPDF):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install Chromium dependencies
RUN apt-get update && apt-get install -y \
    libc6 libgdiplus libx11-6 libxcomposite1 \
    libxdamage1 libxrandr2 libxss1 libxtst6 \
    libnss3 libatk-bridge2.0-0 libgtk-3-0 \
    libgbm1 libasound2 fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

Or use the official IronPDF Docker image:
```dockerfile
FROM ironsoftwareofficial/ironpdfengine:latest
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### Azure/AWS Deployment

**Before (BCL EasyPDF SDK):**
```csharp
// BCL EasyPDF SDK cannot deploy to:
// - Azure App Service
// - Azure Functions
// - AWS Lambda
// - Any serverless environment
// All require interactive Windows sessions with Office installed
```

**After (IronPDF):**
```csharp
// Azure Functions example
public class PdfFunction
{
    [FunctionName("GeneratePdf")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = await renderer.RenderHtmlAsPdfAsync("<h1>Azure PDF</h1>");

        return new FileContentResult(pdf.BinaryData, "application/pdf")
        {
            FileDownloadName = "document.pdf"
        };
    }
}
```

### Dependency Injection Setup

```csharp
// Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Set license once
    IronPdf.License.LicenseKey = Configuration["IronPdf:LicenseKey"];

    // Register renderer as scoped service
    services.AddScoped<ChromePdfRenderer>();

    // Or create a wrapper service
    services.AddScoped<IPdfService, IronPdfService>();
}

// IronPdfService.cs
public interface IPdfService
{
    PdfDocument GenerateFromHtml(string html);
    Task<PdfDocument> GenerateFromHtmlAsync(string html);
    PdfDocument GenerateFromUrl(string url);
}

public class IronPdfService : IPdfService
{
    private readonly ChromePdfRenderer _renderer;

    public IronPdfService()
    {
        _renderer = new ChromePdfRenderer();
        ConfigureDefaults();
    }

    private void ConfigureDefaults()
    {
        _renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        _renderer.RenderingOptions.PrintHtmlBackgrounds = true;
        _renderer.RenderingOptions.MarginTop = 25;
        _renderer.RenderingOptions.MarginBottom = 25;
    }

    public PdfDocument GenerateFromHtml(string html) =>
        _renderer.RenderHtmlAsPdf(html);

    public Task<PdfDocument> GenerateFromHtmlAsync(string html) =>
        _renderer.RenderHtmlAsPdfAsync(html);

    public PdfDocument GenerateFromUrl(string url) =>
        _renderer.RenderUrlAsPdf(url);
}
```

---

## Performance Considerations

### BCL EasyPDF SDK vs IronPDF Performance

| Metric | BCL EasyPDF SDK | IronPDF |
|--------|-----------------|---------|
| **Simple HTML** | Slow (printer driver overhead) | Fast (direct rendering) |
| **Complex HTML** | Often fails | Full support |
| **Server load** | High (COM, Office) | Low (self-contained) |
| **Memory** | High (Office processes) | Moderate |
| **Startup** | Slow (driver init) | ~2s (Chromium init) |
| **Reliability** | Variable (many failure points) | Consistent |
| **Scalability** | Poor (session limits) | Excellent |

### Optimizing IronPDF Performance

```csharp
// 1. Reuse renderer instance
private static readonly ChromePdfRenderer SharedRenderer = new ChromePdfRenderer();

public PdfDocument GeneratePdf(string html)
{
    return SharedRenderer.RenderHtmlAsPdf(html);
}

// 2. Parallel generation
public async Task<List<PdfDocument>> GenerateBatch(List<string> htmlDocs)
{
    var tasks = htmlDocs.Select(html =>
        Task.Run(() => SharedRenderer.RenderHtmlAsPdf(html)));

    var results = await Task.WhenAll(tasks);
    return results.ToList();
}

// 3. Disable unnecessary features
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = false; // If not needed
renderer.RenderingOptions.WaitFor.RenderDelay(0);   // No delay
renderer.RenderingOptions.Timeout = 30000;          // 30s max

// 4. Proper disposal
using (var pdf = renderer.RenderHtmlAsPdf(html))
{
    pdf.SaveAs("output.pdf");
}
```

---

## Troubleshooting Guide

### Issue 1: "Printer not found" errors

**Symptom:** BCL EasyPDF looking for virtual printer

**Solution:** IronPDF doesn't use printers—remove all printer-related code:
```csharp
// ❌ Remove this
Printer printer = new Printer();
printer.RenderHTMLToPDF(html, path);

// ✅ Use this
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs(path);
```

### Issue 2: COM interop exceptions

**Symptom:** `COMException` or RCW errors

**Solution:** IronPDF doesn't use COM—remove all COM references:
```csharp
// ❌ Remove these references from .csproj
<Reference Include="BCL.easyPDF.Interop" />
<COMReference Include="BCLEasyPDFLib" />

// ✅ Add IronPdf NuGet package
<PackageReference Include="IronPdf" Version="*" />
```

### Issue 3: Office automation errors

**Symptom:** Office-related exceptions

**Solution:** IronPDF doesn't need Office—convert documents to HTML first:
```csharp
// ❌ Remove Office dependencies
printer.PrintOfficeDocToPDF("document.docx", "output.pdf");

// ✅ Convert to HTML workflow, or use IronWord
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlFromDocument);
pdf.SaveAs("output.pdf");
```

### Issue 4: Timeout/hang on server

**Symptom:** PDF generation hangs on web server

**Solution:** IronPDF runs headless without interactive sessions:
```csharp
// IronPDF just works on servers
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.Timeout = 60000; // Reliable timeout
var pdf = renderer.RenderHtmlAsPdf(html);
```

### Issue 5: Missing namespace errors

**Symptom:** `BCL.easyPDF` not found

**Solution:** Update all namespaces:
```csharp
// ❌ Remove
using BCL.easyPDF;
using BCL.easyPDF.Interop;

// ✅ Add
using IronPdf;
```

### Issue 6: Page index differences

**Symptom:** Wrong pages selected

**Solution:** IronPDF uses 0-based indexing:
```csharp
// BCL: 1-based
doc.ExtractPages(1, 5);

// IronPDF: 0-based
pdf.CopyPages(0, 4);
```

### Issue 7: Background not printing

**Symptom:** CSS backgrounds missing

**Solution:** Enable background printing:
```csharp
renderer.RenderingOptions.PrintHtmlBackgrounds = true;
```

### Issue 8: Licensing errors

**Symptom:** License validation failed

**Solution:** Use code-based license:
```csharp
// ❌ Remove BCL license setup
Printer.SetLicenseKey("BCL-KEY");

// ✅ Use IronPDF license
IronPdf.License.LicenseKey = "IRONPDF-KEY";
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Document all BCL EasyPDF SDK usage in codebase**
  ```bash
  grep -r "using BCL.easyPDF" --include="*.cs" .
  grep -r "Printer\|PDFDocument" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **List all features used (HTML, URL, Office conversion)**
  **Why:** Understanding the features in use helps map them to IronPDF equivalents.

- [ ] **Identify server deployment requirements**
  **Why:** IronPDF supports cross-platform deployment, unlike BCL EasyPDF SDK.

- [ ] **Note any COM interop dependencies**
  **Why:** IronPDF does not require COM interop, simplifying deployment.

- [ ] **Review current Office automation usage**
  **Why:** IronPDF does not require Microsoft Office, reducing server dependencies.

- [ ] **Plan for Office document alternatives**
  **Why:** IronPDF can handle many document types natively without Office.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Set up test environment with IronPDF**
  **Why:** Testing in a controlled environment ensures a smooth migration process.

### During Migration

- [ ] **Uninstall BCL EasyPDF SDK**
  ```bash
  dotnet remove package BCL.easyPDF
  ```
  **Why:** Remove old dependencies to prevent conflicts.

- [ ] **Remove COM interop references**
  **Why:** IronPDF does not require COM, simplifying the codebase.

- [ ] **Remove Office automation dependencies**
  **Why:** IronPDF eliminates the need for Office, reducing server requirements.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to access its modern PDF generation features.

- [ ] **Replace `BCL.easyPDF` namespaces with `IronPdf`**
  ```csharp
  // Before (BCL.easyPDF)
  using BCL.easyPDF;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Update namespaces to use IronPDF's API.

- [ ] **Replace `Printer` with `ChromePdfRenderer`**
  ```csharp
  // Before (BCL.easyPDF)
  var printer = new Printer();

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** Use IronPDF's ChromePdfRenderer for modern HTML rendering.

- [ ] **Replace `PDFDocument` with `PdfDocument`**
  ```csharp
  // Before (BCL.easyPDF)
  var doc = new PDFDocument();

  // After (IronPDF)
  var pdf = new PdfDocument();
  ```
  **Why:** Use IronPDF's PdfDocument for PDF manipulation.

- [ ] **Update configuration options**
  ```csharp
  // Before (BCL.easyPDF)
  printer.PageSize = PageSizes.A4;

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** IronPDF's RenderingOptions provides a unified configuration approach.

- [ ] **Update page indexing (1-based to 0-based)**
  **Why:** IronPDF uses zero-based indexing for pages, aligning with .NET conventions.

- [ ] **Replace callback async with async/await**
  ```csharp
  // Before (BCL.easyPDF)
  printer.PrintAsync(callback);

  // After (IronPDF)
  await renderer.RenderHtmlAsPdfAsync(html);
  ```
  **Why:** IronPDF supports modern async/await for better asynchronous programming.

- [ ] **Update license configuration**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Test each converted feature**
  **Why:** Ensure all functionality works as expected after migration.

### Post-Migration

- [ ] **Verify PDF output quality**
  **Why:** IronPDF's Chromium engine may render differently; ensure quality meets expectations.

- [ ] **Test all edge cases**
  **Why:** Validate that all scenarios are handled correctly with IronPDF.

- [ ] **Validate server deployment works**
  **Why:** Ensure IronPDF runs smoothly in the server environment.

- [ ] **Test Docker/container deployment**
  **Why:** IronPDF supports containerized environments, unlike BCL EasyPDF SDK.

- [ ] **Test cloud environment deployment**
  **Why:** Verify that IronPDF works in cloud setups, leveraging its cross-platform capabilities.

- [ ] **Remove BCL EasyPDF installer from deployment**
  **Why:** Clean up deployment by removing unnecessary components.

- [ ] **Remove Office installation from servers**
  **Why:** IronPDF does not require Office, simplifying server maintenance.

- [ ] **Document any IronPDF-specific configurations**
  **Why:** Maintain clear documentation for future reference and troubleshooting.

- [ ] **Train team on new API patterns**
  **Why:** Ensure the development team is familiar with IronPDF's API for efficient use.

- [ ] **Update CI/CD pipelines**
  **Why:** Reflect changes in build and deployment processes to accommodate IronPDF.
---

## Additional Resources

### Official Documentation
- **IronPDF Documentation**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Getting Started Guide**: https://ironpdf.com/docs/

### Tutorials
- **HTML to PDF Tutorial**: https://ironpdf.com/how-to/html-file-to-pdf/
- **URL to PDF Guide**: https://ironpdf.com/tutorials/url-to-pdf/
- **ASP.NET Integration**: https://ironpdf.com/tutorials/aspx-to-pdf/

### Code Examples
- **GitHub Examples**: https://github.com/nicholashew/IronPdf-Examples
- **Code Samples**: https://ironpdf.com/examples/

### Support
- **Technical Support**: https://ironpdf.com/support/
- **Community Forum**: https://forum.ironpdf.com/
- **Stack Overflow**: Search `[ironpdf]` tag

### BCL EasyPDF References
- **BCL Technologies Website**: https://www.bcltechnologies.com/
- **BCL easyPDF SDK**: https://www.bcltechnologies.com/easypdf/sdk/
- **.NET API Reference**: https://www.pdfonline.com/Easypdf/sdk/usermanual/

---

*This migration guide covers the transition from BCL EasyPDF SDK to IronPDF. For additional assistance, contact IronPDF support or consult the official documentation.*
