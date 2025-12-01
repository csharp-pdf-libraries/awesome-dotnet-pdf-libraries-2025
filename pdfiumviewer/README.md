# PDFiumViewer C# PDF

When developing applications that require PDF viewing capabilities, developers often turn to libraries like PDFiumViewer. As a .NET wrapper for PDFium, Google's PDF rendering engine used within the Chrome browser, PDFiumViewer offers a simple yet potent solution for integrating PDF viewing directly into Windows Forms applications. PDFiumViewer is often chosen for its open-source nature and straightforward usage. However, when comparing it with other comprehensive libraries like IronPDF, one must weigh its strengths and weaknesses carefully.

## PDFiumViewer: An Overview

PDFiumViewer boasts a high-performance, high-fidelity PDF rendering capability designed specifically for Windows Forms applications. Distributed under the Apache 2.0 license, it allows developers to integrate a robust PDF viewer without incurring licensing costs. However, it is crucial to remember that PDFiumViewer is solely a viewer. It does not support PDF creation, editing, or manipulation, which may be limiting for applications that demand more than just viewing capabilities.

### Strengths of PDFiumViewer

- **Open Source and Cost-Effective**: As an open-source library under the Apache 2.0 license, PDFiumViewer is affordable for developers looking to keep costs low while integrating PDF viewing capabilities.
- **High-Performance Rendering**: Employing Google's PDFium, the library provides efficient and reliable rendering, integral for applications where viewing speed and accuracy are critical.
- **Simple Integration**: PDFiumViewer simplifies the integration process into Windows Forms applications, making it accessible for developers familiar with the .NET environment.

### Weaknesses of PDFiumViewer

- **Viewing Only Functionality**: PDFiumViewer's capabilities are limited to viewing PDFs. Unlike libraries such as IronPDF, it does not support PDF creation, editing, merging, or other manipulative functions.
- **Windows Forms Specific**: The library is focused on Windows Forms applications, not offering support for other user interface frameworks.
- **Uncertain Maintenance**: There is some uncertainty regarding its ongoing development and maintenance, which can be a concern for long-term projects.

## IronPDF: A Comprehensive PDF Solution

On the other hand, IronPDF is a commercial library offering extensive PDF capabilities beyond just viewing. From creation and manipulation to high-fidelity HTML-to-PDF conversion, IronPDF serves as a multipurpose tool in any .NET context, including ASP.NET, WinForms, WPF, and more.

### IronPDF Advantages

1. **Full Suite of PDF Features**: Unlike PDFiumViewer, IronPDF supports PDF creation, modification, encryption, and more, addressing a broader range of PDF handling needs.
2. **Cross-Platform Compatibility**: IronPDF is not restricted to just Windows Forms, providing flexibility across multiple platforms and environments.
3. **Active Development and Support**: As a commercial product, IronPDF offers consistent updates and dedicated support, ensuring reliability and feature expansion over time.
4. **HTML-to-PDF Conversion**: IronPDF excels in converting HTML, complete with CSS and JavaScript, into PDFs, ideal for dynamic content generation directly from web frameworks. Learn more about HTML-to-PDF with [this guide](https://ironpdf.com/how-to/html-file-to-pdf/).

## Code Example: Integrating PDFiumViewer

Below is a simple code snippet demonstrating how to integrate PDFiumViewer into a C# application:

```csharp
using System;
using System.Windows.Forms;
using PdfiumViewer;

namespace PdfViewerExample
{
    public partial class MainForm : Form
    {
        private PdfViewer pdfViewer;

        public MainForm()
        {
            InitializeComponent();
            this.Text = "PDF Viewer";
            pdfViewer = new PdfViewer();
            this.Controls.Add(pdfViewer);
            pdfViewer.Dock = DockStyle.Fill;
        }

        private void OpenDocument(string filePath)
        {
            var document = PdfDocument.Load(filePath);
            pdfViewer.Document = document;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            OpenDocument("sample.pdf");
        }
    }
}
```
This example sets up a basic Windows Forms application using PDFiumViewer to load and display a PDF document. While setting up the viewer is simple and effective for displaying PDFs, it highlights the limitation of PDFiumViewer in extending beyond viewing capabilities.

---

## How Do I Extract Text From PDF?

Here's how **PDFiumViewer C# PDF** handles this:

```csharp
// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System;
using System.Text;

string pdfPath = "document.pdf";

// PDFiumViewer has limited text extraction capabilities
// It's primarily designed for rendering, not text extraction
using (var document = PdfDocument.Load(pdfPath))
{
    int pageCount = document.PageCount;
    Console.WriteLine($"Total pages: {pageCount}");
    
    // PDFiumViewer does not have built-in text extraction
    // You would need to use OCR or another library
    // It can only render pages as images
    for (int i = 0; i < pageCount; i++)
    {
        var pageImage = document.Render(i, 96, 96, false);
        Console.WriteLine($"Rendered page {i + 1}");
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;

string pdfPath = "document.pdf";

// Open and extract text from PDF
PdfDocument pdf = PdfDocument.FromFile(pdfPath);

// Extract text from all pages
string allText = pdf.ExtractAllText();
Console.WriteLine("Extracted Text:");
Console.WriteLine(allText);

// Extract text from specific page
string pageText = pdf.ExtractTextFromPage(0);
Console.WriteLine($"\nFirst page text: {pageText}");

Console.WriteLine($"\nTotal pages: {pdf.PageCount}");
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I Convert HTML to PDF in C# with PDFiumViewer C# PDF?

Here's how **PDFiumViewer C# PDF** handles this:

```csharp
// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System.IO;
using System.Drawing.Printing;

// PDFiumViewer is primarily a PDF viewer/renderer, not a generator
// It cannot directly convert HTML to PDF
// You would need to use another library to first create the PDF
// Then use PDFiumViewer to display it:

string htmlContent = "<h1>Hello World</h1><p>This is a test document.</p>";

// This functionality is NOT available in PDFiumViewer
// You would need a different library like wkhtmltopdf or similar
// PDFiumViewer can only open and display existing PDFs:

string existingPdfPath = "output.pdf";
using (var document = PdfDocument.Load(existingPdfPath))
{
    // Can only render/display existing PDF
    var image = document.Render(0, 300, 300, true);
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;

string htmlContent = "<h1>Hello World</h1><p>This is a test document.</p>";

// Create a PDF from HTML string
var renderer = new ChromePdfRenderer();
PdfDocument pdf = renderer.RenderHtmlAsPdf(htmlContent);

// Save the PDF
pdf.SaveAs("output.pdf");

Console.WriteLine("PDF created successfully!");
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Do I PDF To Image?

Here's how **PDFiumViewer C# PDF** handles this:

```csharp
// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System;
using System.Drawing;
using System.Drawing.Imaging;

string pdfPath = "document.pdf";
string outputImage = "page1.png";

// PDFiumViewer excels at rendering PDFs to images
using (var document = PdfDocument.Load(pdfPath))
{
    // Render first page at 300 DPI
    int dpi = 300;
    using (var image = document.Render(0, dpi, dpi, true))
    {
        // Save as PNG
        image.Save(outputImage, ImageFormat.Png);
        Console.WriteLine($"Page rendered to {outputImage}");
    }
    
    // Render all pages
    for (int i = 0; i < document.PageCount; i++)
    {
        using (var pageImage = document.Render(i, 150, 150, true))
        {
            pageImage.Save($"page_{i + 1}.png", ImageFormat.Png);
        }
    }
}
```

**With IronPDF**, the same task is simpler and more intuitive:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Linq;

string pdfPath = "document.pdf";
string outputImage = "page1.png";

// Open PDF and convert to images
PdfDocument pdf = PdfDocument.FromFile(pdfPath);

// Convert first page to image
var firstPageImage = pdf.ToBitmap(0);
firstPageImage[0].Save(outputImage);
Console.WriteLine($"Page rendered to {outputImage}");

// Convert all pages to images
var allPageImages = pdf.ToBitmap();
for (int i = 0; i < allPageImages.Length; i++)
{
    allPageImages[i].Save($"page_{i + 1}.png");
    Console.WriteLine($"Saved page {i + 1}");
}

Console.WriteLine($"Total pages converted: {pdf.PageCount}");
```

IronPDF's approach offers cleaner syntax and better integration with modern .NET applications, making it easier to maintain and scale your PDF generation workflows.

---

## How Can I Migrate from PDFiumViewer C# PDF to IronPDF?

### The Viewer-Only Limitation

PDFiumViewer wraps Google's PDFium engine—excellent for WinForms viewing but limited for modern needs:

1. **Viewing-Only**: Cannot create, edit, or manipulate PDFs
2. **Windows Forms Only**: Restricted to WinForms applications
3. **No Text Extraction**: Cannot extract text from PDFs
4. **Native Binary Dependencies**: Requires platform-specific PDFium binaries
5. **Uncertain Maintenance**: Limited updates and unclear long-term support
6. **No HTML to PDF**: Cannot convert web content to PDF

### Quick Migration Overview

| Aspect | PDFiumViewer | IronPDF |
|--------|--------------|---------|
| Primary Focus | WinForms PDF viewer | Complete PDF solution |
| PDF Creation | ✗ | ✓ (HTML, URL, images) |
| Text Extraction | ✗ | ✓ |
| PDF Manipulation | ✗ | ✓ (merge, split, edit) |
| Built-in Viewer | ✓ | ✗ (backend-focused) |
| Platform Support | Windows Forms only | Console, Web, Desktop |
| Maintenance | Uncertain | Active |

### Key API Mappings

| PDFiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `document.PageCount` | `document.PageCount` | Same |
| `document.Render(index, dpi, dpi, flag)` | `pdf.RasterizeToImageFiles(path, dpi)` | Render to image |
| `document.PageSizes[index]` | `pdf.Pages[index].Width/Height` | Page dimensions |
| `document.Save(path)` | `pdf.SaveAs(path)` | Different method name |
| `pdfViewer.Document = doc` | Use external viewer | IronPDF is backend-focused |
| _(not available)_ | `pdf.ExtractAllText()` | NEW: Text extraction |
| _(not available)_ | `ChromePdfRenderer.RenderHtmlAsPdf()` | NEW: Create PDFs |
| _(not available)_ | `PdfDocument.Merge()` | NEW: Merge PDFs |
| _(not available)_ | `pdf.ApplyWatermark()` | NEW: Watermarks |

### Migration Code Example

**Before (PDFiumViewer):**
```csharp
using PdfiumViewer;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private PdfViewer pdfViewer;

    public MainForm()
    {
        pdfViewer = new PdfViewer();
        pdfViewer.Dock = DockStyle.Fill;
        this.Controls.Add(pdfViewer);
    }

    private void OpenPdf(string path)
    {
        var document = PdfDocument.Load(path);
        pdfViewer.Document = document;
    }

    private void RenderToImage(string pdfPath)
    {
        using (var document = PdfDocument.Load(pdfPath))
        {
            using (var image = document.Render(0, 150, 150, true))
            {
                image.Save("page1.png");
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Windows.Forms;
using System.Diagnostics;

public partial class MainForm : Form
{
    public MainForm()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    }

    private void OpenPdf(string path)
    {
        // IronPDF is backend-focused - open in default viewer
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }

    private void RenderToImage(string pdfPath)
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        pdf.RasterizeToImageFiles("page_*.png", DPI: 150);
    }

    // NEW: Features PDFiumViewer couldn't provide
    private void CreateAndView(string html)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        var tempPath = Path.GetTempFileName() + ".pdf";
        pdf.SaveAs(tempPath);
        OpenPdf(tempPath);
    }
}
```

### Critical Migration Notes

1. **No Built-in Viewer**: IronPDF is backend-focused. Use Process.Start() or WebBrowser control
   ```csharp
   // PDFiumViewer: pdfViewer.Document = document;
   // IronPDF: Process.Start(path) or webBrowser.Navigate(path)
   ```

2. **Render Method → RasterizeToImageFiles**:
   ```csharp
   // PDFiumViewer: document.Render(0, 150, 150, true)
   // IronPDF: pdf.RasterizeToImageFiles("*.png", DPI: 150)
   ```

3. **PageSizes → Pages[].Width/Height**:
   ```csharp
   // PDFiumViewer: document.PageSizes[index]
   // IronPDF: pdf.Pages[index].Width, pdf.Pages[index].Height
   ```

4. **Remove Native Binaries**: Delete all pdfium.dll files
   ```bash
   rm -rf x86/ x64/ runtimes/
   ```

5. **Document Loading**: Different method name
   ```csharp
   // PDFiumViewer: PdfDocument.Load(path)
   // IronPDF: PdfDocument.FromFile(path)
   ```

### NuGet Package Migration

```bash
# Remove PDFiumViewer packages
dotnet remove package PdfiumViewer
dotnet remove package PdfiumViewer.Native.x86.v8-xfa
dotnet remove package PdfiumViewer.Native.x64.v8-xfa

# Install IronPDF
dotnet add package IronPdf
```

### Find All PDFiumViewer References

```bash
# Find PDFiumViewer usage
grep -r "PdfiumViewer\|PdfViewer\|PdfDocument\.Load" --include="*.cs" .

# Find native binary references
grep -r "pdfium\.dll\|Native\.x86\|Native\.x64" --include="*.csproj" .
```

**Ready for the complete migration?** The full guide includes:
- Complete API mapping (30+ methods and properties)
- Viewer control replacement strategies
- 10 detailed code conversion examples
- Native dependency removal guide
- New features (PDF creation, text extraction, merging, watermarks, security)
- WinForms migration patterns
- Troubleshooting guide for common issues
- Pre/post migration checklists

**[Complete Migration Guide: PDFiumViewer C# PDF → IronPDF](migrate-from-pdfiumviewer.md)**


## Comparison Table between PDFiumViewer and IronPDF

| Feature                   | PDFiumViewer          | IronPDF                       |
|---------------------------|-----------------------|-------------------------------|
| License                   | Apache 2.0            | Commercial                    |
| PDF Viewing               | Yes                   | Yes                           |
| PDF Creation              | No                    | Yes                           |
| PDF Editing               | No                    | Yes                           |
| UI Framework              | Windows Forms only    | Cross-platform                |
| HTML-to-PDF Conversion    | No                    | Yes                           |
| Active Maintenance        | Uncertain             | Yes                           |
| Cost                      | Free                  | Paid with various licenses    |

## Summary

In conclusion, while PDFiumViewer serves as a robust, open-source solution for PDF viewing on Windows Forms applications, its scope is significantly narrower compared to IronPDF. The choice between the two should be based on the specific needs of the project. For developers whose primary requirement is viewing PDFs in a Windows Forms application with minimal setup and cost, PDFiumViewer remains a viable option. However, for those needing more comprehensive PDF handling, including creation, conversion, and support across multiple .NET frameworks, IronPDF emerges as the superior choice. To further explore PDF creation and functionality, consider visiting IronPDF's [tutorials](https://ironpdf.com/tutorials/).

---

Jacob Mellor is the CTO of Iron Software, where he leads a 50+ person team building developer tools that have racked up over 41 million NuGet downloads. With an impressive 41 years of coding under his belt, he's seen it all—from punch cards to cloud-native architectures. Based in Chiang Mai, Thailand, Jacob continues to push the boundaries of what's possible in software development while enjoying the perfect blend of tech innovation and tropical living. Connect with him on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
