# Migration Guide: PDFreactor → IronPDF

## Why Migrate to IronPDF

IronPDF is a native .NET library that eliminates the complexity of Java integration and separate service architecture required by PDFreactor. It offers seamless deployment within .NET applications without external dependencies, reducing infrastructure overhead and simplifying maintenance. IronPDF provides a pure C# API that integrates naturally with modern .NET projects while delivering high-performance HTML-to-PDF conversion.

## NuGet Package Changes

```xml
<!-- Remove PDFreactor -->
<PackageReference Include="PDFreactor" Version="11.x.x" />

<!-- Add IronPDF -->
<PackageReference Include="IronPdf" Version="2024.x.x" />
```

## Namespace Mapping

| PDFreactor | IronPDF |
|------------|---------|
| `com.realobjects.pdfreactor` | `IronPdf` |
| `com.realobjects.pdfreactor.webservice` | `IronPdf` |
| `com.realobjects.pdfreactor.Configuration` | `IronPdf.ChromePdfRenderer` |

## API Mapping

| PDFreactor | IronPDF |
|------------|---------|
| `PDFreactor.convert()` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `PDFreactor.convertAsBinary()` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `Configuration.setDocument()` | `ChromePdfRenderer.RenderHtmlAsPdf(html)` |
| `Configuration.setDocumentUrl()` | `ChromePdfRenderer.RenderUrlAsPdf(url)` |
| `Configuration.setAuthor()` | `PdfDocument.MetaData.Author` |
| `Configuration.setTitle()` | `PdfDocument.MetaData.Title` |
| `Configuration.setBaseUrl()` | `ChromePdfRenderer.RenderingOptions.HtmlHeader.BaseUrl` |
| `Configuration.setJavaScriptEnabled()` | `ChromePdfRenderer.RenderingOptions.EnableJavaScript` |
| `Configuration.setPageWidth()` | `ChromePdfRenderer.RenderingOptions.PaperSize` |
| `Configuration.setPageHeight()` | `ChromePdfRenderer.RenderingOptions.PaperSize` |
| `Configuration.setPageOrientation()` | `ChromePdfRenderer.RenderingOptions.PaperOrientation` |
| `Configuration.setMargins()` | `ChromePdfRenderer.RenderingOptions.MarginTop/Bottom/Left/Right` |
| `Configuration.setEncryption()` | `PdfDocument.Password` / `PdfDocument.SecuritySettings` |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (PDFreactor):**
```java
PDFreactor pdfReactor = new PDFreactor();
Configuration config = new Configuration();
config.setDocument("<html><body><h1>Hello World</h1></body></html>");

Result result = pdfReactor.convert(config);
byte[] pdf = result.getDocument();
FileOutputStream fos = new FileOutputStream("output.pdf");
fos.write(pdf);
fos.close();
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Hello World</h1></body></html>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Custom Settings

**Before (PDFreactor):**
```java
PDFreactor pdfReactor = new PDFreactor();
Configuration config = new Configuration();
config.setDocumentUrl("https://example.com");
config.setJavaScriptEnabled(true);
config.setPageWidth("210mm");
config.setPageHeight("297mm");
config.setPageOrientation(PageOrientation.PORTRAIT);

Result result = pdfReactor.convert(config);
byte[] pdf = result.getDocument();
File file = new File("output.pdf");
FileUtils.writeByteArrayToFile(file, pdf);
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

### Example 3: PDF with Metadata and Security

**Before (PDFreactor):**
```java
PDFreactor pdfReactor = new PDFreactor();
Configuration config = new Configuration();
config.setDocument("<html><body><h1>Confidential Report</h1></body></html>");
config.setTitle("Report");
config.setAuthor("John Doe");
config.setEncryption(Encryption.TYPE_128);
config.setUserPassword("user123");
config.setOwnerPassword("owner123");

Result result = pdfReactor.convert(config);
byte[] pdf = result.getDocument();
Files.write(Paths.get("output.pdf"), pdf);
```

**After (IronPDF):**
```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Confidential Report</h1></body></html>");

pdf.MetaData.Title = "Report";
pdf.MetaData.Author = "John Doe";
pdf.SecuritySettings.UserPassword = "user123";
pdf.SecuritySettings.OwnerPassword = "owner123";

pdf.SaveAs("output.pdf");
```

## Common Gotchas

- **Initialization**: IronPDF doesn't require service initialization or connection setup like PDFreactor's web service architecture
- **Byte Arrays**: IronPDF returns `PdfDocument` objects instead of raw byte arrays; use `.BinaryData` property if you need byte arrays
- **Page Size**: IronPDF uses enum values (`PdfPaperSize.A4`) instead of string dimensions; custom sizes use `CustomPaperSize` property
- **Margins**: IronPDF margins are set individually (`MarginTop`, `MarginBottom`, etc.) in millimeters by default, not as a single object
- **JavaScript Execution**: IronPDF enables JavaScript by default; PDFreactor requires explicit enablement
- **Async Operations**: IronPDF supports native async/await patterns with `RenderHtmlAsPdfAsync()`; PDFreactor uses Java callback patterns
- **License Configuration**: IronPDF licenses are set via `License.LicenseKey = "YOUR-KEY"` at application startup, not per-conversion
- **Error Handling**: IronPDF throws .NET exceptions; no need to check Result objects for error states

## Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)