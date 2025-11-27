# Migration Guide: GdPicture.NET (Nutrient) → IronPDF

## Why Migrate to IronPDF

IronPDF offers a focused, streamlined solution for developers who need PDF generation and manipulation without the complexity and cost of a full document imaging suite. With a simpler API and straightforward pricing, IronPDF eliminates the overhead of features like OCR and barcode scanning when you only need core PDF functionality. Teams can reduce licensing costs while gaining a more intuitive developer experience designed specifically for .NET PDF workflows.

## NuGet Package Changes

```bash
# Remove GdPicture.NET
dotnet remove package GdPicture.NET.14

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| GdPicture.NET | IronPDF |
|---------------|---------|
| `GdPicture14` | `IronPdf` |
| `GdPicture14.PDF` | `IronPdf` |
| `GdPictureDocumentConverter` | `IronPdf.ChromePdfRenderer` |

## API Mapping

| GdPicture.NET | IronPDF | Notes |
|---------------|---------|-------|
| `GdPicturePDF` | `PdfDocument` | Main PDF document class |
| `GdPicturePDF.LoadFromFile()` | `PdfDocument.FromFile()` | Load existing PDF |
| `GdPicturePDF.SaveToFile()` | `PdfDocument.SaveAs()` | Save PDF to disk |
| `GdPictureDocumentConverter.RenderHTML()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | HTML to PDF conversion |
| `GdPicturePDF.NewPDF()` | `new PdfDocument()` | Create new PDF |
| `GdPicturePDF.AddImageFromFile()` | `ImageToPdfConverter.ImageToPdf()` | Add images to PDF |
| `GdPicturePDF.MergePDF()` | `PdfDocument.Merge()` | Merge multiple PDFs |
| `GdPicturePDF.GetPageCount()` | `PdfDocument.PageCount` | Get number of pages |
| `GdPicturePDF.EncryptPDF()` | `PdfDocument.Password` | Password protection |
| `GdPicturePDF.GetText()` | `PdfDocument.ExtractAllText()` | Extract text content |

## Code Examples

### Example 1: HTML to PDF Conversion

**Before (GdPicture.NET):**
```csharp
using GdPicture14;

GdPictureDocumentConverter converter = new GdPictureDocumentConverter();
GdPictureStatus status = converter.LoadFromURL("https://example.com");

if (status == GdPictureStatus.OK)
{
    status = converter.SaveAsPDF("output.pdf");
    converter.Release();
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 2: Merge Multiple PDFs

**Before (GdPicture.NET):**
```csharp
using GdPicture14;

using (GdPicturePDF pdf1 = new GdPicturePDF())
using (GdPicturePDF pdf2 = new GdPicturePDF())
{
    pdf1.LoadFromFile("document1.pdf", false);
    pdf2.LoadFromFile("document2.pdf", false);
    
    pdf1.MergePDF(pdf2);
    pdf1.SaveToFile("merged.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("document1.pdf");
var pdf2 = PdfDocument.FromFile("document2.pdf");

var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");
```

### Example 3: Password Protect PDF

**Before (GdPicture.NET):**
```csharp
using GdPicture14;

using (GdPicturePDF pdf = new GdPicturePDF())
{
    pdf.LoadFromFile("input.pdf", false);
    pdf.SetEncryption(
        PdfEncryptionAlgorithm.PdfEncryptionAES128bits,
        "userpassword",
        "ownerpassword",
        PdfPermissions.PdfPermissionFullQualityPrint
    );
    pdf.SaveToFile("protected.pdf");
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");

var securitySettings = new PdfPrintSecurity
{
    OwnerPassword = "ownerpassword",
    UserPassword = "userpassword",
    AllowUserAnnotations = false,
    AllowUserCopyPasteContent = false,
    AllowUserFormData = false,
    AllowUserPrinting = PdfPrintSecurity.PdfPrintPermission.FullPrintRights
};

pdf.SecuritySettings = securitySettings;
pdf.SaveAs("protected.pdf");
```

## Common Gotchas

### 1. License Key Activation
- **GdPicture.NET**: License key set via `LicenseManager.RegisterKEY()`
- **IronPDF**: License key set via `IronPdf.License.LicenseKey = "YOUR-KEY"` or placed in `appsettings.json`

### 2. Disposal Patterns
- **GdPicture.NET**: Requires explicit `.Release()` or `.Dispose()` calls
- **IronPDF**: Implements standard .NET `IDisposable` pattern; use `using` statements

### 3. Status Checking
- **GdPicture.NET**: Returns `GdPictureStatus` enum that must be checked after operations
- **IronPDF**: Throws exceptions on errors; use try-catch blocks for error handling

### 4. HTML Rendering Engine
- **GdPicture.NET**: Uses internal rendering engine
- **IronPDF**: Uses Chromium-based renderer with better CSS3/JavaScript support

### 5. Page Indexing
- **GdPicture.NET**: Pages are typically 1-indexed
- **IronPDF**: Pages are 0-indexed when accessing page collections

### 6. Thread Safety
- **GdPicture.NET**: Generally not thread-safe; requires careful instance management
- **IronPDF**: Thread-safe by design; can render multiple PDFs concurrently

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [IronPDF API Reference](https://ironpdf.com/docs/api/)
- [Code Examples](https://ironpdf.com/examples/)