# Migration Guide: Sumatra PDF → IronPDF

## Why Migrate from Sumatra PDF to IronPDF

Sumatra PDF is a lightweight PDF reader designed as a standalone application, not a library for integration into .NET applications. IronPDF is a comprehensive .NET library specifically built for developers to create, read, and manipulate PDFs programmatically within commercial applications. Unlike Sumatra's GPL license restrictions, IronPDF offers commercial licensing that allows seamless integration into proprietary software.

## NuGet Package Changes

```bash
# Remove Sumatra PDF (if using any wrapper)
# Sumatra PDF is not available as an official NuGet package

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Sumatra PDF | IronPDF |
|-------------|---------|
| N/A (Standalone application) | `IronPdf` |
| N/A | `IronPdf.Rendering` |
| N/A | `IronPdf.Engines.Chrome` |

## API Mapping

| Sumatra PDF Functionality | IronPDF Equivalent |
|---------------------------|-------------------|
| Launch viewer programmatically | `ChromePdfRenderer.RenderHtmlAsPdf()` + Display |
| Open PDF file | `PdfDocument.FromFile()` |
| View PDF (external) | `PdfDocument.FromFile()` (manipulation, not viewing) |
| N/A - Cannot create PDFs | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| N/A - Cannot edit PDFs | `PdfDocument` methods (AddPage, RemovePage, etc.) |
| N/A - Cannot convert HTML | `ChromePdfRenderer.RenderUrlAsPdf()` |
| N/A - Cannot merge PDFs | `PdfDocument.Merge()` |

## Code Examples

### Example 1: Creating a PDF from HTML

**Before (Sumatra PDF):**
```csharp
// Not possible with Sumatra PDF
// You would need to:
// 1. Create PDF with another tool
// 2. Launch Sumatra as external process
Process.Start("SumatraPDF.exe", "document.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>Created with IronPDF</p>");
pdf.SaveAs("document.pdf");
```

### Example 2: Converting a URL to PDF

**Before (Sumatra PDF):**
```csharp
// Not possible with Sumatra PDF
// Sumatra can only view existing PDFs
// No programmatic PDF creation capability
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Reading and Manipulating PDF Content

**Before (Sumatra PDF):**
```csharp
// Not possible with Sumatra PDF
// Sumatra is read-only viewer
// No API for programmatic access
Process.Start("SumatraPDF.exe", "input.pdf");
```

**After (IronPDF):**
```csharp
using IronPdf;

// Load existing PDF
var pdf = PdfDocument.FromFile("input.pdf");

// Extract text
string text = pdf.ExtractAllText();

// Add watermark
pdf.ApplyWatermark("<h1>CONFIDENTIAL</h1>", 50, VerticalAlignment.Middle, HorizontalAlignment.Center);

// Save modified PDF
pdf.SaveAs("output.pdf");
```

## Common Gotchas

### 1. **IronPDF is a Library, Not a Viewer**
Sumatra PDF is a viewer application. IronPDF is a development library for creating and manipulating PDFs. If you need to display PDFs in your application, you'll need to implement your own viewer or use a third-party component.

### 2. **License Requirements**
IronPDF requires a license key for commercial use. Add your license key at application startup:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### 3. **Chrome Engine Dependencies**
IronPDF uses a Chrome rendering engine which may require additional setup in certain environments (Docker, Linux, Azure). Ensure all dependencies are installed:
```bash
# Linux example
apt-get install -y libgdiplus libc6-dev
```

### 4. **Process Launching vs. Library Integration**
Migration from Sumatra means moving from external process launching to in-process library calls. Remove all `Process.Start()` calls and replace with IronPDF API methods.

### 5. **GPL License Concerns Eliminated**
Unlike Sumatra's GPL license, IronPDF uses commercial licensing. Ensure you purchase appropriate licenses for your deployment scenario.

## Additional Resources

- **Official Documentation:** https://ironpdf.com/docs/
- **Tutorials & Examples:** https://ironpdf.com/tutorials/
- **API Reference:** https://ironpdf.com/docs/