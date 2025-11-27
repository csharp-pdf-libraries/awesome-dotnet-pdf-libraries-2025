# Migration Guide: BCL EasyPDF SDK → IronPDF

## Why Migrate from BCL EasyPDF SDK

BCL EasyPDF SDK's reliance on Windows-only architecture, Microsoft Office automation, and virtual printer drivers creates significant deployment challenges, especially in modern containerized and cloud environments. Developers frequently encounter server failures, DLL dependency issues, and timeout problems that work locally but fail in production due to interactive session requirements and COM interop limitations. IronPDF offers a cross-platform, self-contained solution with native support for Windows, Linux, macOS, and Docker containers, eliminating Office dependencies and providing a modern API for .NET Core, .NET 5+, and .NET Framework applications.

## NuGet Package Changes

```bash
# Remove BCL EasyPDF SDK
# (typically installed via MSI merge module or manual DLL reference)

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| BCL EasyPDF SDK | IronPDF |
|----------------|---------|
| `BCL.easyPDF` | `IronPdf` |
| `BCL.easyPDF.Interop` | `IronPdf` |
| `BCL.easyPDF.Printer` | `IronPdf` (no printer driver needed) |
| `BCL.easyPDF.Office` | `IronPdf` (no Office automation needed) |

## API Mapping Table

| BCL EasyPDF SDK | IronPDF | Notes |
|----------------|---------|-------|
| `Printer printer = new Printer()` | `ChromePdfRenderer renderer = new ChromePdfRenderer()` | No printer driver required |
| `printer.PrintOfficeDocToPDF()` | `PdfDocument.FromWord()` | Direct conversion without Office |
| `printer.RenderHTMLToPDF()` | `renderer.RenderHtmlAsPdf()` | Chrome-based rendering engine |
| `printer.RenderUrlToPDF()` | `renderer.RenderUrlAsPdf()` | URL to PDF conversion |
| `printer.Configuration.TimeOut` | `renderer.RenderingOptions.Timeout` | Reliable timeout handling |
| `printer.Configuration.JobTitle` | `pdf.MetaData.Title` | Metadata after generation |
| `Printer.SetLicenseKey()` | `IronPdf.License.LicenseKey = "key"` | License configuration |
| `printer.BeginPrintToFile()` | `renderer.RenderHtmlAsPdf().SaveAs()` | Simplified async pattern |
| `printer.Configuration.PageOrientation` | `renderer.RenderingOptions.PaperOrientation` | Layout control |
| `printer.Configuration.PageSize` | `renderer.RenderingOptions.PaperSize` | Paper size settings |

## Code Examples

### Example 1: HTML to PDF Conversion

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;
using BCL.easyPDF.Interop;

Printer printer = new Printer();
printer.Configuration.TimeOut = 120;
printer.Configuration.JobTitle = "Invoice";

string htmlContent = "<html><body><h1>Invoice</h1></body></html>";
printer.RenderHTMLToPDF(htmlContent, "invoice.pdf");
printer.Dispose();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.Timeout = 120;

string htmlContent = "<html><body><h1>Invoice</h1></body></html>";
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.MetaData.Title = "Invoice";
pdf.SaveAs("invoice.pdf");
```

### Example 2: Word Document to PDF

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;

Printer printer = new Printer();
printer.Configuration.TimeOut = 300;
printer.Configuration.UseOfficeAutomation = true;

// Requires Microsoft Word installed on server
printer.PrintOfficeDocToPDF("document.docx", "output.pdf");
printer.Dispose();
```

**After (IronPDF):**
```csharp
using IronPdf;

// No Microsoft Office required
var pdf = PdfDocument.FromWord("document.docx");
pdf.SaveAs("output.pdf");
```

### Example 3: URL to PDF with Configuration

**Before (BCL EasyPDF SDK):**
```csharp
using BCL.easyPDF;
using BCL.easyPDF.Interop;

Printer printer = new Printer();
printer.Configuration.PageOrientation = PageOrientation.Landscape;
printer.Configuration.PageSize = PageSize.A4;
printer.Configuration.TimeOut = 180;
printer.Configuration.BackgroundPrinting = false; // Often causes failures

try
{
    printer.RenderUrlToPDF("https://example.com", "webpage.pdf");
}
catch (Exception ex)
{
    // Common timeout and session errors on server
    Console.WriteLine("Conversion failed: " + ex.Message);
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
renderer.RenderingOptions.Timeout = 180;

// Reliable server-side rendering without session issues
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

## Common Gotchas

### 1. No Office Installation Required
BCL EasyPDF requires Microsoft Office installed on the server for document conversion. IronPDF handles Office documents natively without any Office dependencies. Remove all Office automation configuration code.

### 2. No COM Interop or Printer Drivers
BCL EasyPDF uses COM interop and virtual printer drivers which cause DLL errors and require interactive sessions. IronPDF uses a self-contained Chrome rendering engine. Remove all printer configuration and COM reference handling.

### 3. Cross-Platform Support
BCL EasyPDF is Windows-only. IronPDF works on Windows, Linux, macOS, Docker, Azure, and AWS without platform-specific configuration changes.

### 4. Timeout Handling Actually Works
BCL EasyPDF's timeout settings often don't prevent hangs in server environments. IronPDF's timeout mechanisms are reliable and consistently applied across all rendering operations.

### 5. Async/Await Pattern Differences
BCL EasyPDF uses `BeginPrintToFile()` callbacks. IronPDF uses standard .NET async/await patterns:

```csharp
// IronPDF async rendering
var pdf = await renderer.RenderUrlAsPdfAsync("https://example.com");
```

### 6. License Configuration
BCL uses `Printer.SetLicenseKey()` method. IronPDF uses static property:

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 7. Container Deployment
BCL EasyPDF cannot run in containers. For IronPDF Docker deployments, use the Linux package and ensure proper dependencies:

```dockerfile
# Install IronPDF dependencies for Linux containers
RUN apt-get update && apt-get install -y \
    libc6-dev \
    libgdiplus \
    libx11-dev
```

## Additional Resources

- **Full Documentation:** https://ironpdf.com/docs/
- **Tutorials & Examples:** https://ironpdf.com/tutorials/
- **API Reference:** https://ironpdf.com/docs/