# Migration Guide: Apache PDFBox (.NET ports) → IronPDF

## Why Migrate from Apache PDFBox .NET Ports

Apache PDFBox is a Java library with unofficial .NET ports that often lag behind, suffer from inconsistent quality, and lack native .NET design patterns. These ports have limited community support in the .NET ecosystem and may introduce compatibility issues. IronPDF provides a native .NET solution with modern Chromium rendering, professional support (23-second average response time), and a proven track record with 10M+ downloads trusted by NASA, Tesla, and Fortune 500 companies.

## NuGet Package Changes

```bash
# Remove PDFBox .NET port packages
dotnet remove package PdfBox
dotnet remove package PDFBoxNet
dotnet remove package Apache.PdfBox

# Install IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| Apache PDFBox (.NET ports) | IronPDF |
|---------------------------|---------|
| `org.apache.pdfbox.pdmodel` | `IronPdf` |
| `org.apache.pdfbox.text` | `IronPdf` |
| `org.apache.pdfbox.multipdf` | `IronPdf` |
| `org.apache.pdfbox.rendering` | `IronPdf` |
| `org.apache.pdfbox.pdmodel.encryption` | `IronPdf` |

## API Mapping

| Apache PDFBox (.NET ports) | IronPDF |
|---------------------------|---------|
| `PDDocument.Load()` | `PdfDocument.FromFile()` |
| `PDFTextStripper.GetText()` | `pdfDocument.ExtractAllText()` |
| `PDFMergerUtility.MergeDocuments()` | `PdfDocument.Merge()` |
| `PDDocument.Save()` | `pdfDocument.SaveAs()` |
| `PDPage` / `PDPageContentStream` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| `PDFRenderer.RenderImage()` | `pdfDocument.RasterizeToImageFiles()` |
| `StandardProtectionPolicy` | `pdfDocument.Password` / `pdfDocument.SecuritySettings` |
| `Splitter.Split()` | `pdfDocument.CopyPages()` / `pdfDocument.CopyPage()` |

## Code Examples

### Example 1: Creating a PDF from HTML

**Before (Apache PDFBox .NET port):**
```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.pdmodel.font;
using org.apache.pdfbox.pdmodel.edit;

// PDFBox doesn't support HTML rendering - manual creation required
PDDocument document = new PDDocument();
PDPage page = new PDPage();
document.addPage(page);

PDPageContentStream contentStream = new PDPageContentStream(document, page);
PDFont font = PDType1Font.HELVETICA_BOLD;
contentStream.beginText();
contentStream.setFont(font, 12);
contentStream.moveTextPositionByAmount(100, 700);
contentStream.drawString("Hello World");
contentStream.endText();
contentStream.close();

document.save("output.pdf");
document.close();
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
```

### Example 2: Extracting Text from PDF

**Before (Apache PDFBox .NET port):**
```csharp
using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.text;
using System.IO;

PDDocument document = null;
try
{
    document = PDDocument.load(new File("input.pdf"));
    PDFTextStripper stripper = new PDFTextStripper();
    stripper.setStartPage(1);
    stripper.setEndPage(document.getNumberOfPages());
    string text = stripper.getText(document);
    Console.WriteLine(text);
}
finally
{
    if (document != null)
    {
        document.close();
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("input.pdf");
string text = pdf.ExtractAllText();
Console.WriteLine(text);
```

### Example 3: Merging Multiple PDFs

**Before (Apache PDFBox .NET port):**
```csharp
using org.apache.pdfbox.multipdf;
using org.apache.pdfbox.pdmodel;
using System.IO;
using System.Collections.Generic;

PDFMergerUtility merger = new PDFMergerUtility();
List<File> files = new List<File>
{
    new File("document1.pdf"),
    new File("document2.pdf"),
    new File("document3.pdf")
};

foreach (File file in files)
{
    merger.addSource(file);
}

merger.setDestinationFileName("merged.pdf");
merger.mergeDocuments();
```

**After (IronPDF):**
```csharp
using IronPdf;

var pdfs = new[] { "document1.pdf", "document2.pdf", "document3.pdf" }
    .Select(PdfDocument.FromFile)
    .ToList();

var merged = PdfDocument.Merge(pdfs);
merged.SaveAs("merged.pdf");
```

## Common Gotchas

| Issue | Solution |
|-------|----------|
| **Java-style syntax in .NET ports** | IronPDF uses idiomatic C# with properties, LINQ support, and async/await patterns |
| **No HTML/CSS support in PDFBox** | IronPDF uses Chromium rendering engine for full HTML5, CSS3, and JavaScript support |
| **Manual coordinate positioning** | Use HTML/CSS for layout instead of manual positioning - IronPDF handles rendering automatically |
| **Exception handling differences** | PDFBox throws Java-style exceptions; IronPDF uses standard .NET exception patterns |
| **File handling** | PDFBox requires Java `File` objects; IronPDF accepts standard .NET strings, streams, and paths |
| **Memory management** | PDFBox requires explicit `close()` calls; IronPDF uses `IDisposable` pattern with `using` statements |
| **Font handling** | PDFBox requires manual font loading; IronPDF automatically handles system and web fonts |
| **Cross-platform compatibility** | .NET ports may have JVM dependencies; IronPDF is pure .NET with native cross-platform support |

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **API Reference**: Comprehensive IntelliSense support with detailed XML documentation
- **Support**: Professional support team with 23-second average response time

## Next Steps

1. Install IronPDF via NuGet
2. Review the [Getting Started Guide](https://ironpdf.com/docs/)
3. Explore [code examples](https://ironpdf.com/tutorials/) for your specific use cases
4. Test your migration in a development environment
5. Contact support for migration assistance if needed