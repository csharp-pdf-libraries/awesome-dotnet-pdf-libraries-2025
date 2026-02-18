# How Do I Migrate from PDFiumViewer to IronPDF in C#?

## Why Migrate from PDFiumViewer to IronPDF?

PDFiumViewer is a .NET wrapper for Google's PDFium rendering engine, designed specifically for Windows Forms PDF viewing. While excellent for displaying PDFs, it cannot create, edit, or manipulate them—and its uncertain maintenance status creates risk for production applications.

### Critical PDFiumViewer Limitations

1. **Viewing-Only**: Cannot create PDFs from HTML, images, or programmatically
2. **Windows Forms Only**: Restricted to WinForms applications
3. **No PDF Manipulation**: Cannot merge, split, or modify PDF content
4. **Native Binary Dependencies**: Requires platform-specific PDFium binaries
5. **Uncertain Maintenance**: Limited updates and unclear long-term support
6. **No Text Extraction**: Cannot extract text from PDFs (only render as images)
7. **No HTML to PDF**: Cannot convert web content to PDF
8. **No Headers/Footers**: Cannot add page numbers or repeating content
9. **No Watermarks**: Cannot stamp documents with overlays
10. **No Security Features**: Cannot encrypt or password-protect PDFs

### [IronPDF](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/) Advantages

| Aspect | PDFiumViewer | IronPDF |
|--------|--------------|---------|
| **Primary Focus** | WinForms PDF viewer | Complete PDF solution |
| **PDF Creation** | ✗ | ✓ (HTML, URL, images) |
| **PDF Manipulation** | ✗ | ✓ (merge, split, edit) |
| **HTML to PDF** | ✗ | ✓ (Chromium html to pdf c# engine) |
| **Text Extraction** | ✗ | ✓ |
| **Watermarks** | ✗ | ✓ |
| **Headers/Footers** | ✗ | ✓ |
| **Security** | ✗ | ✓ |
| **Platform Support** | Windows Forms only | Console, Web, Desktop |
| **Framework Support** | .NET Framework | .NET Framework, Core, 5+ |
| **Maintenance** | Uncertain | Active |

For a side-by-side feature analysis, see the [detailed comparison](https://ironsoftware.com/suite/blog/comparison/compare-pdfiumviewer-vs-ironpdf/).

---

## NuGet Package Changes

```bash
# Remove PDFiumViewer packages
dotnet remove package PdfiumViewer
dotnet remove package PdfiumViewer.Native.x86.v8-xfa
dotnet remove package PdfiumViewer.Native.x64.v8-xfa

# Install IronPDF
dotnet add package IronPdf
```

---

## Namespace Changes

```csharp
// PDFiumViewer
using PdfiumViewer;

// IronPDF
using IronPdf;
using IronPdf.Rendering;
using IronPdf.Editing;
```

---

## Complete API Reference

### Core Class Mappings

| PDFiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `PdfDocument` | `PdfDocument` | Same name, different capabilities |
| `PdfViewer` | _(no equivalent)_ | IronPDF is backend-focused |
| `PdfRenderer` | `ChromePdfRenderer` | PDF creation |
| _(not available)_ | `HtmlHeaderFooter` | Headers/footers |

### Document Loading

| PDFiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `PdfDocument.Load(path)` | `PdfDocument.FromFile(path)` | Load from file |
| `PdfDocument.Load(stream)` | `PdfDocument.FromStream(stream)` | Load from stream |
| `PdfDocument.Load(bytes)` | `PdfDocument.FromBinaryData(bytes)` | Load from bytes |

### Document Properties

| PDFiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `document.PageCount` | `document.PageCount` | Same |
| `document.PageSizes` | `document.Pages[i].Width/Height` | Per-page access |
| `document.GetPageSize(index)` | `document.Pages[index].Width/Height` | Direct properties |

### Page Rendering

| PDFiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `document.Render(pageIndex, dpiX, dpiY, forPrinting)` | `pdf.RasterizeToImageFiles(path, dpi)` | Rasterize |
| `document.Render(pageIndex, width, height, dpiX, dpiY, flags)` | DPI parameter | Quality control |
| `PdfRenderFlags` enum | DPI parameter | Simplified |

### Saving Documents

| PDFiumViewer | IronPDF | Notes |
|--------------|---------|-------|
| `document.Save(path)` | `pdf.SaveAs(path)` | Different method name |
| `document.Save(stream)` | `pdf.Stream` | Access stream |
| _(not available)_ | `pdf.BinaryData` | Get bytes |

### Viewer Control (WinForms)

| PDFiumViewer | IronPDF Alternative | Notes |
|--------------|---------------------|-------|
| `PdfViewer` control | Save + external viewer | IronPDF has no UI control |
| `pdfViewer.Document = doc` | `pdf.SaveAs(path)` then view | Backend-focused |
| `pdfViewer.Zoom` | N/A | Use viewer-specific control |

### NEW Features (Not in PDFiumViewer)

| IronPDF Feature | Description |
|-----------------|-------------|
| `ChromePdfRenderer.RenderHtmlAsPdf()` | Create from HTML |
| `ChromePdfRenderer.RenderUrlAsPdf()` | Create from URL |
| `PdfDocument.Merge()` | Combine PDFs |
| `pdf.CopyPages()` | Extract pages |
| `pdf.RemovePages()` | Delete pages |
| `pdf.ApplyWatermark()` | Add watermarks |
| `pdf.AddHtmlHeaders()` | Add headers |
| `pdf.AddHtmlFooters()` | Add footers |
| `pdf.SecuritySettings` | Password protection |
| `pdf.ExtractAllText()` | Extract text |
| `pdf.Form` | Form filling |

---

## Code Migration Examples

### Example 1: Load PDF and Render to Image

**Before (PDFiumViewer):**
```csharp
using PdfiumViewer;
using System.Drawing;
using System.Drawing.Imaging;

public class PdfRenderService
{
    public void RenderPdfToImages(string pdfPath, string outputFolder)
    {
        using (var document = PdfDocument.Load(pdfPath))
        {
            for (int i = 0; i < document.PageCount; i++)
            {
                // Render at 150 DPI
                using (var image = document.Render(i, 150, 150, true))
                {
                    image.Save($"{outputFolder}/page_{i + 1}.png", ImageFormat.Png);
                }
            }
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public class PdfRenderService
{
    public PdfRenderService()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    }

    public void RenderPdfToImages(string pdfPath, string outputFolder)
    {
        var pdf = PdfDocument.FromFile(pdfPath);

        // Render all pages at 150 DPI with c# html to pdf capabilities
        pdf.RasterizeToImageFiles($"{outputFolder}/page_*.png", DPI: 150);
    }
}
```

### Example 2: Windows Forms PDF Viewer

**Before (PDFiumViewer):**
```csharp
using PdfiumViewer;
using System.Windows.Forms;

public partial class MainForm : Form
{
    private PdfViewer pdfViewer;

    public MainForm()
    {
        InitializeComponent();

        pdfViewer = new PdfViewer();
        pdfViewer.Dock = DockStyle.Fill;
        this.Controls.Add(pdfViewer);
    }

    private void OpenPdf(string path)
    {
        var document = PdfDocument.Load(path);
        pdfViewer.Document = document;
    }

    private void btnOpen_Click(object sender, EventArgs e)
    {
        using (var dialog = new OpenFileDialog())
        {
            dialog.Filter = "PDF files (*.pdf)|*.pdf";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OpenPdf(dialog.FileName);
            }
        }
    }
}
```

**After (IronPDF - Backend Processing):**
```csharp
using IronPdf;
using System.Windows.Forms;
using System.Diagnostics;

public partial class MainForm : Form
{
    // IronPDF is backend-focused - use WebBrowser control or external viewer

    private WebBrowser webBrowser;
    private string tempPdfPath;

    public MainForm()
    {
        InitializeComponent();
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        webBrowser = new WebBrowser();
        webBrowser.Dock = DockStyle.Fill;
        this.Controls.Add(webBrowser);
    }

    private void OpenPdf(string path)
    {
        // Option 1: Open in default PDF viewer
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });

        // Option 2: Display in WebBrowser control (if Adobe plugin installed)
        webBrowser.Navigate(path);
    }

    // IronPDF enables features PDFiumViewer couldn't provide:
    private void CreatePdfFromHtml(string html)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        tempPdfPath = System.IO.Path.GetTempFileName() + ".pdf";
        pdf.SaveAs(tempPdfPath);
        OpenPdf(tempPdfPath);
    }
}
```

### Example 3: Get Page Information

**Before (PDFiumViewer):**
```csharp
using PdfiumViewer;

public void GetPageInfo(string pdfPath)
{
    using (var document = PdfDocument.Load(pdfPath))
    {
        Console.WriteLine($"Total pages: {document.PageCount}");

        for (int i = 0; i < document.PageCount; i++)
        {
            var size = document.PageSizes[i];
            Console.WriteLine($"Page {i + 1}: {size.Width} x {size.Height} points");
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void GetPageInfo(string pdfPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    Console.WriteLine($"Total pages: {pdf.PageCount}");

    for (int i = 0; i < pdf.PageCount; i++)
    {
        var page = pdf.Pages[i];
        Console.WriteLine($"Page {i + 1}: {page.Width} x {page.Height} points");
    }
}
```

### Example 4: Extract Text (NEW Feature)

**Before (PDFiumViewer):**
```csharp
// PDFiumViewer CANNOT extract text
// You would need OCR or another library
throw new NotSupportedException("PDFiumViewer cannot extract text from PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;

public string ExtractText(string pdfPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    return pdf.ExtractAllText();
}

public string ExtractTextFromPage(string pdfPath, int pageIndex)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    return pdf.Pages[pageIndex].Text;
}
```

### Example 5: Create PDF from HTML (NEW Feature)

**Before (PDFiumViewer):**
```csharp
// PDFiumViewer CANNOT create PDFs
throw new NotSupportedException("PDFiumViewer cannot create PDFs from HTML");
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CreatePdfFromHtml(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.MarginTop = 20;
    renderer.RenderingOptions.MarginBottom = 20;

    // Add page numbers
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>",
        MaxHeight = 20
    };

    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs(outputPath);
}
```

### Example 6: Merge PDFs (NEW Feature)

**Before (PDFiumViewer):**
```csharp
// PDFiumViewer CANNOT merge PDFs
throw new NotSupportedException("PDFiumViewer cannot merge PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;
using System.Collections.Generic;

public void MergePdfs(List<string> inputPaths, string outputPath)
{
    var pdfs = inputPaths.Select(path => PdfDocument.FromFile(path)).ToList();
    var merged = PdfDocument.Merge(pdfs);
    merged.SaveAs(outputPath);
}
```

### Example 7: Add Watermark (NEW Feature)

**Before (PDFiumViewer):**
```csharp
// PDFiumViewer CANNOT add watermarks
throw new NotSupportedException("PDFiumViewer cannot add watermarks");
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Editing;

public void AddWatermark(string pdfPath, string outputPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    pdf.ApplyWatermark(
        "<div style='color:red; font-size:72px; opacity:0.3; transform:rotate(-45deg);'>CONFIDENTIAL</div>",
        45,
        VerticalAlignment.Middle,
        HorizontalAlignment.Center);

    pdf.SaveAs(outputPath);
}
```

### Example 8: Print PDF

**Before (PDFiumViewer):**
```csharp
using PdfiumViewer;
using System.Drawing.Printing;

public void PrintPdf(string pdfPath, string printerName)
{
    using (var document = PdfDocument.Load(pdfPath))
    {
        using (var printDocument = document.CreatePrintDocument())
        {
            printDocument.PrinterSettings.PrinterName = printerName;
            printDocument.Print();
        }
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

public void PrintPdf(string pdfPath, string printerName)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    pdf.Print(printerName);
}
```

### Example 9: Password Protection (NEW Feature)

**Before (PDFiumViewer):**
```csharp
// PDFiumViewer CANNOT encrypt PDFs
throw new NotSupportedException("PDFiumViewer cannot encrypt PDFs");
```

**After (IronPDF):**
```csharp
using IronPdf;

public void SecurePdf(string pdfPath, string outputPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    pdf.SecuritySettings.OwnerPassword = "admin123";
    pdf.SecuritySettings.UserPassword = "user456";
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;
    pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.NoPrint;

    pdf.SaveAs(outputPath);
}
```

### Example 10: Convert URL to PDF (NEW Feature)

**Before (PDFiumViewer):**
```csharp
// PDFiumViewer CANNOT convert URLs to PDF
throw new NotSupportedException("PDFiumViewer cannot convert URLs to PDF");
```

**After (IronPDF):**
```csharp
using IronPdf;

public void CaptureWebPage(string url, string outputPath)
{
    var renderer = new ChromePdfRenderer();

    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PrintHtmlBackgrounds = true;
    renderer.RenderingOptions.WaitFor.RenderDelay(1000); // Wait for JS

    var pdf = renderer.RenderUrlAsPdf(url);
    pdf.SaveAs(outputPath);
}
```

---

## Viewer Control Migration

PDFiumViewer provides a built-in WinForms viewer control. IronPDF is backend-focused, so you need alternative approaches for viewing:

### Option 1: Default Application

```csharp
// Open in system's default PDF viewer
System.Diagnostics.Process.Start(new ProcessStartInfo(pdfPath)
{
    UseShellExecute = true
});
```

### Option 2: WebBrowser Control

```csharp
// Display in WebBrowser control (requires PDF plugin)
webBrowser.Navigate(pdfPath);
```

### Option 3: Third-Party Viewer

Consider using specialized viewer controls:
- Syncfusion PdfViewer
- DevExpress PDF Viewer
- Telerik PDF Viewer

### Option 4: Web-Based Viewer

For web applications, serve the PDF and let the browser display it:

```csharp
// ASP.NET Core controller action
public IActionResult ViewPdf()
{
    var pdf = PdfDocument.FromFile("document.pdf");
    return File(pdf.BinaryData, "application/pdf");
}
```

---

## Native Dependency Removal

### Before (PDFiumViewer) - Complex Deployment

```
MyApp/
├── bin/
│   ├── MyApp.dll
│   ├── PdfiumViewer.dll
│   ├── x86/
│   │   └── pdfium.dll
│   └── x64/
│       └── pdfium.dll
```

### After (IronPDF) - Clean Deployment

```
MyApp/
├── bin/
│   ├── MyApp.dll
│   └── IronPdf.dll  # Everything included
```

### Remove Native Binary References

```bash
# Delete native PDFium binaries
rm -rf x86/ x64/ runtimes/

# Remove from .csproj native package references
# <PackageReference Include="PdfiumViewer.Native.x86.v8-xfa" />
# <PackageReference Include="PdfiumViewer.Native.x64.v8-xfa" />
```

---

## Common Migration Gotchas

### 1. No Built-in Viewer Control

```csharp
// PDFiumViewer: pdfViewer.Document = document;
// IronPDF: Use external viewer or web-based approach
pdf.SaveAs(tempPath);
Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
```

### 2. Render Method Differences

```csharp
// PDFiumViewer: document.Render(pageIndex, dpiX, dpiY, forPrinting)
// IronPDF: pdf.RasterizeToImageFiles(path, DPI)
pdf.RasterizeToImageFiles("*.png", DPI: 150);
```

### 3. Page Size Access

```csharp
// PDFiumViewer: document.PageSizes[index]
// IronPDF: document.Pages[index].Width, .Height
var page = pdf.Pages[0];
Console.WriteLine($"{page.Width} x {page.Height}");
```

### 4. Document Disposal

```csharp
// PDFiumViewer: Required explicit using
using (var document = PdfDocument.Load(path)) { }

// IronPDF: More forgiving, but using still recommended
var pdf = PdfDocument.FromFile(path);
```

### 5. Print Document

```csharp
// PDFiumViewer: document.CreatePrintDocument()
// IronPDF: pdf.Print(printerName)
pdf.Print("HP LaserJet");
```

---

## Feature Comparison Summary

| Feature | PDFiumViewer | IronPDF |
|---------|--------------|---------|
| Load PDF | ✓ | ✓ |
| Render to Image | ✓ | ✓ |
| Built-in Viewer | ✓ | ✗ |
| Print PDF | ✓ | ✓ |
| Extract Text | ✗ | ✓ |
| Create from HTML | ✗ | ✓ |
| Create from URL | ✗ | ✓ |
| Merge PDFs | ✗ | ✓ |
| Split PDFs | ✗ | ✓ |
| Add Watermarks | ✗ | ✓ |
| Headers/Footers | ✗ | ✓ |
| Form Filling | ✗ | ✓ |
| Password Protection | ✗ | ✓ |
| WinForms Support | ✓ | ✓ |
| ASP.NET Support | ✗ | ✓ |
| .NET Core Support | Limited | ✓ |
| Active Maintenance | Uncertain | ✓ |

---

## Pre-Migration Checklist

- [ ] Identify all PDFiumViewer usage in codebase
- [ ] List WinForms using PdfViewer control
- [ ] Document current rendering DPI settings
- [ ] Check for native binary references
- [ ] Identify print functionality usage
- [ ] Plan viewer control replacement strategy
- [ ] Review .NET Framework version requirements

---

## Post-Migration Checklist

- [ ] Remove PDFiumViewer NuGet packages
- [ ] Remove native PDFium packages
- [ ] Delete native pdfium.dll binaries
- [ ] Add IronPDF NuGet package
- [ ] Set IronPDF license key
- [ ] Replace PdfViewer control with alternative
- [ ] Update rendering calls to RasterizeToImageFiles
- [ ] Test printing functionality
- [ ] Test on target platforms
- [ ] Update documentation

---

## Finding PDFiumViewer References

```bash
# Find PDFiumViewer usage
grep -r "PdfiumViewer\|PdfViewer\|PdfDocument\.Load" --include="*.cs" .

# Find native binary references
grep -r "pdfium\.dll\|Native\.x86\|Native\.x64" --include="*.csproj" .

# Find viewer control usage
grep -r "PdfViewer" --include="*.cs" --include="*.Designer.cs" .
```

---

## Troubleshooting

### "License key required"

```csharp
// Set at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### "No viewer control available"

IronPDF is backend-focused. Use:
- `Process.Start()` to open in default viewer
- WebBrowser control for embedded viewing
- Third-party PDF viewer controls

### "Rendered image quality different"

Match DPI settings:

```csharp
// PDFiumViewer: document.Render(0, 150, 150, true)
// IronPDF equivalent:
pdf.RasterizeToImageFiles("*.png", DPI: 150);
```

### "Print functionality changed"

```csharp
// Simplified in IronPDF
pdf.Print("PrinterName");

// Or with settings
var settings = new PrintSettings { NumberOfCopies = 2 };
pdf.Print(settings);
```

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all PDFiumViewer usages in codebase**
  ```bash
  grep -r "using PDFiumViewer" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package PDFiumViewer
  dotnet add package IronPdf
  ```
  **Why:** Clean package switch to IronPDF.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace PDF generation calls**
  ```csharp
  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf(html);
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF uses ChromePdfRenderer with modern Chromium rendering.

- [ ] **Update page settings**
  ```csharp
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  renderer.RenderingOptions.MarginTop = 20;
  renderer.RenderingOptions.MarginBottom = 20;
  ```
  **Why:** RenderingOptions provides all page configuration in one place.

- [ ] **Configure headers/footers**
  ```csharp
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
  };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine provides modern rendering. Verify key documents.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  var merged = PdfDocument.Merge(pdf1, pdf2);
  pdf.SecuritySettings.UserPassword = "secret";
  pdf.ApplyWatermark("<h1>DRAFT</h1>");
  ```
  **Why:** IronPDF provides many additional features.

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [HTML to PDF Tutorial](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [API Reference](https://ironpdf.com/object-reference/api/)
