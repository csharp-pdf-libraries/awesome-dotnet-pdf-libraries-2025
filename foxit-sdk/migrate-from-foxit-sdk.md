# Migration Guide: Foxit SDK → IronPDF

## Why Migrate to IronPDF

IronPDF offers a streamlined alternative to Foxit SDK with straightforward licensing, simple integration requiring minimal setup, and pricing suitable for projects of all sizes. The library provides intuitive APIs for PDF generation and manipulation without the complexity of enterprise-focused configuration. IronPDF's .NET-native design ensures seamless integration with modern C# applications.

## NuGet Package Changes

```bash
# Remove Foxit SDK
# Foxit SDK typically requires manual DLL references or private NuGet feeds

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Foxit SDK | IronPDF |
|-----------|---------|
| `foxit` | `IronPdf` |
| `foxit.pdf` | `IronPdf` |
| `foxit.common` | `IronPdf` |
| `foxit.common.fxcrt` | `IronPdf.Rendering` |
| `foxit.addon.conversion` | `IronPdf.Rendering` |

## API Mapping

| Foxit SDK | IronPDF | Notes |
|-----------|---------|-------|
| `PDFDoc.Load()` | `PdfDocument.FromFile()` | Load existing PDF |
| `PDFDoc.StartCreate()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Create new PDF from HTML |
| `PDFPage.GetText()` | `PdfDocument.ExtractAllText()` | Extract text content |
| `PDFDoc.SaveAs()` | `PdfDocument.SaveAs()` | Save PDF document |
| `PDFDoc.GetPageCount()` | `PdfDocument.PageCount` | Get page count |
| `ConvertToPDF.FromWord()` | Not needed - use HTML rendering | Convert Office docs |
| `PDFPage.AddWatermark()` | `PdfDocument.ApplyWatermark()` | Add watermarks |
| `PDFDoc.GetMetadata()` | `PdfDocument.MetaData` | Access metadata |
| `PDFDoc.Encrypt()` | `PdfDocument.SecuritySettings` | PDF encryption |
| `ImageConvert.ToDocument()` | `ImageToPdfConverter.ImageToPdf()` | Convert images to PDF |

## Code Examples

### Example 1: Creating a PDF from HTML

**Before (Foxit SDK):**
```csharp
using foxit;
using foxit.common;
using foxit.pdf;
using foxit.addon.conversion;

// Initialize library
Library.Initialize("license_sn", "license_key");

// Create conversion settings
HTML2PDFSettingData settings = new HTML2PDFSettingData();
settings.page_width = 612;
settings.page_height = 792;
settings.page_mode = HTML2PDFSettingData.PageMode.e_PageModeMultiPages;

// Convert HTML to PDF
Convert html2pdf = new Convert(Convert.Type.e_HTML2PDF);
html2pdf.StartConvertFromHTMLFile("<html><body>Hello World</body></html>", 
    "output.pdf", settings);

Library.Release();
```

**After (IronPDF):**
```csharp
using IronPdf;

// Set license key
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Create PDF from HTML
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<html><body>Hello World</body></html>");
pdf.SaveAs("output.pdf");
```

### Example 2: Loading and Extracting Text

**Before (Foxit SDK):**
```csharp
using foxit;
using foxit.pdf;
using foxit.common;

Library.Initialize("license_sn", "license_key");

PDFDoc doc = new PDFDoc("input.pdf");
ErrorCode errorCode = doc.Load(null);

if (errorCode == ErrorCode.e_ErrSuccess)
{
    StringBuilder allText = new StringBuilder();
    for (int i = 0; i < doc.GetPageCount(); i++)
    {
        PDFPage page = doc.GetPage(i);
        TextPage textPage = new TextPage(page, TextPage.ParsingFlag.e_ParseTextNormal);
        allText.Append(textPage.GetText());
    }
    Console.WriteLine(allText.ToString());
}

doc.Close();
Library.Release();
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf = PdfDocument.FromFile("input.pdf");
string allText = pdf.ExtractAllText();
Console.WriteLine(allText);
```

### Example 3: Merging PDFs and Adding Watermark

**Before (Foxit SDK):**
```csharp
using foxit;
using foxit.pdf;
using foxit.common;

Library.Initialize("license_sn", "license_key");

PDFDoc doc1 = new PDFDoc("document1.pdf");
doc1.Load(null);

PDFDoc doc2 = new PDFDoc("document2.pdf");
doc2.Load(null);

// Insert pages from doc2 into doc1
doc1.InsertDocument(doc1.GetPageCount(), doc2, 
    new RangeArray(new Range(0, doc2.GetPageCount() - 1)));

// Add watermark
for (int i = 0; i < doc1.GetPageCount(); i++)
{
    PDFPage page = doc1.GetPage(i);
    Watermark watermark = new Watermark(page, "CONFIDENTIAL", 
        new PointF(300, 400), new PointF(0, 0), 45, 0.5f);
    watermark.SetFontColor(0xFF0000);
}

doc1.SaveAs("merged_with_watermark.pdf", 
    PDFDoc.FileVersion.e_Ver17);

doc1.Close();
doc2.Close();
Library.Release();
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Linq;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var pdf1 = PdfDocument.FromFile("document1.pdf");
var pdf2 = PdfDocument.FromFile("document2.pdf");

// Merge PDFs
var merged = PdfDocument.Merge(pdf1, pdf2);

// Add watermark
merged.ApplyWatermark("<h1 style='color:red'>CONFIDENTIAL</h1>", 
    rotation: 45, 
    opacity: 50);

merged.SaveAs("merged_with_watermark.pdf");
```

## Common Gotchas

### 1. License Initialization
- **Foxit**: Requires `Library.Initialize()` and `Library.Release()` calls
- **IronPDF**: Simply set `IronPdf.License.LicenseKey` once at application startup

### 2. Error Handling
- **Foxit**: Returns `ErrorCode` enums that must be checked explicitly
- **IronPDF**: Uses standard .NET exceptions - wrap operations in try-catch blocks

### 3. Page Indexing
- **Foxit**: Zero-based page indexing with manual page object retrieval
- **IronPDF**: Zero-based indexing with direct property access via `PdfDocument.PageCount`

### 4. Resource Management
- **Foxit**: Requires explicit calls to `Close()` and `Release()` methods
- **IronPDF**: Implements `IDisposable` - use `using` statements for automatic cleanup

### 5. HTML to PDF Conversion
- **Foxit**: Requires separate conversion addon and complex settings configuration
- **IronPDF**: Built-in `ChromePdfRenderer` with sensible defaults and optional configuration

### 6. Output Path Handling
- **Foxit**: Some methods require output paths as parameters during conversion
- **IronPDF**: Generate in-memory first, then save with `SaveAs()` for more flexibility

### 7. Font Embedding
- **Foxit**: Requires manual font configuration and embedding setup
- **IronPDF**: Automatically handles font embedding from HTML/CSS

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)