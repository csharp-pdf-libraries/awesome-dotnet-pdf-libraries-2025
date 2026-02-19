# How Do I Migrate from Sumatra PDF to IronPDF in C#?

## Why Migrate from Sumatra PDF?

Sumatra PDF is a **desktop PDF viewer application**, not a development library. If you're using Sumatra PDF in your .NET application, you're likely:

1. Launching it as an external process to display PDFs
2. Using it for printing PDFs via command-line
3. Relying on it as a dependency your users must install

### Key Problems with Sumatra PDF Integration

| Problem | Impact |
|---------|--------|
| **Not a Library** | Cannot programmatically create or edit PDFs |
| **External Process** | Requires spawning separate processes |
| **GPL License** | Restrictive for commercial software |
| **User Dependency** | Users must install Sumatra separately |
| **No API** | Limited to command-line arguments |
| **View-Only** | Cannot create, edit, or manipulate PDFs |
| **No Web Support** | Desktop-only application |

### What IronPDF Offers Instead

| Capability | Sumatra PDF | IronPDF |
|------------|-------------|---------|
| Create PDFs | No | Yes |
| Edit PDFs | No | Yes |
| HTML to PDF | No | Yes |
| Merge/Split | No | Yes |
| Watermarks | No | Yes |
| Digital Signatures | No | Yes |
| Form Filling | No | Yes |
| Text Extraction | No | Yes |
| .NET Integration | None | Native |
| Web Applications | No | Yes |
| Commercial License | GPL | Yes |

---

## Quick Start: Replacing Sumatra PDF

### Step 1: Install IronPDF

```bash
dotnet add package IronPdf
```

### Step 2: Remove Sumatra Dependencies

```csharp
// Remove this pattern
Process.Start("SumatraPDF.exe", "document.pdf");

// Replace with IronPDF
using IronPdf;
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

---

## Common Sumatra PDF Usage Patterns → IronPDF

### Pattern 1: Opening PDF for User

**Sumatra (External Process):**
```csharp
using System.Diagnostics;

// Requires Sumatra installed on user's machine
Process.Start(new ProcessStartInfo
{
    FileName = "SumatraPDF.exe",
    Arguments = "\"report.pdf\"",
    UseShellExecute = true
});
```

**IronPDF (Generate and Open):**
```csharp
using IronPdf;
using System.Diagnostics;

// Create PDF programmatically
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Report</h1><p>Content here</p>");
pdf.SaveAs("report.pdf");

// Open with default PDF viewer (system-associated)
Process.Start(new ProcessStartInfo
{
    FileName = "report.pdf",
    UseShellExecute = true
});
```

### Pattern 2: Printing PDF

**Sumatra (Command-Line Print):**
```csharp
using System.Diagnostics;

// Print using Sumatra's command line
Process.Start(new ProcessStartInfo
{
    FileName = "SumatraPDF.exe",
    Arguments = "-print-to-default \"document.pdf\"",
    CreateNoWindow = true
});
```

**IronPDF (Native Print):**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Print to default printer
pdf.Print();

// Print with options
pdf.Print(new PrintOptions
{
    PrinterName = "HP LaserJet",
    NumberOfCopies = 2,
    DPI = 300
});
```

### Pattern 3: Creating PDF (Not Possible with Sumatra)

**Sumatra:** Not possible - Sumatra is view-only

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// From HTML string
var pdf = renderer.RenderHtmlAsPdf(@"
    <html>
    <head><style>body { font-family: Arial; }</style></head>
    <body>
        <h1>Invoice #12345</h1>
        <p>Thank you for your purchase.</p>
    </body>
    </html>");

pdf.SaveAs("invoice.pdf");
```

### Pattern 4: Converting URL to PDF (Not Possible with Sumatra)

**Sumatra:** Not possible

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.JavaScript(3000);

var pdf = renderer.RenderUrlAsPdf("https://example.com/report");
pdf.SaveAs("webpage.pdf");
```

### Pattern 5: Merging PDFs (Not Possible with Sumatra)

**Sumatra:** Not possible

**IronPDF:**
```csharp
using IronPdf;

var pdf1 = PdfDocument.FromFile("chapter1.pdf");
var pdf2 = PdfDocument.FromFile("chapter2.pdf");
var pdf3 = PdfDocument.FromFile("chapter3.pdf");

var book = PdfDocument.Merge(pdf1, pdf2, pdf3);
book.SaveAs("complete_book.pdf");
```

### Pattern 6: Adding Watermarks (Not Possible with Sumatra)

**Sumatra:** Not possible

**IronPDF:**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

pdf.ApplyWatermark(@"
    <div style='
        font-size: 60pt;
        color: rgba(255, 0, 0, 0.3);
        transform: rotate(-45deg);
    '>
        CONFIDENTIAL
    </div>");

pdf.SaveAs("watermarked.pdf");
```

### Pattern 7: Extracting Text (Not Possible with Sumatra)

**Sumatra:** Not possible programmatically

**IronPDF:**
```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
string allText = pdf.ExtractAllText();

// Or per page
for (int i = 0; i < pdf.PageCount; i++)
{
    string pageText = pdf.ExtractTextFromPage(i);
    Console.WriteLine($"Page {i + 1}: {pageText}");
}
```

### Pattern 8: Password Protection (Not Possible with Sumatra)

**Sumatra:** View-only, cannot add protection

**IronPDF:**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Sensitive Data</h1>");

pdf.SecuritySettings.OwnerPassword = "owner123";
pdf.SecuritySettings.UserPassword = "user456";
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;

pdf.SaveAs("protected.pdf");
```

---

## Complete Feature Comparison

| Feature | Sumatra PDF | IronPDF |
|---------|-------------|---------|
| **Creation** | | |
| HTML to PDF | No | Yes |
| URL to PDF | No | Yes |
| Text to PDF | No | Yes |
| Image to PDF | No | Yes |
| **Manipulation** | | |
| Merge PDFs | No | Yes |
| Split PDFs | No | Yes |
| Rotate Pages | No | Yes |
| Delete Pages | No | Yes |
| Reorder Pages | No | Yes |
| **Content** | | |
| Add Watermarks | No | Yes |
| Add Headers/Footers | No | Yes |
| Stamp Text | No | Yes |
| Stamp Images | No | Yes |
| **Security** | | |
| Password Protection | No | Yes |
| Digital Signatures | No | Yes |
| Encryption | No | Yes |
| Permission Settings | No | Yes |
| **Extraction** | | |
| Extract Text | No | Yes |
| Extract Images | No | Yes |
| **Forms** | | |
| Fill Forms | No | Yes |
| Create Forms | No | Yes |
| Read Form Data | No | Yes |
| **Viewing** | | |
| Display PDF | Yes (External) | Via Browser/Viewer |
| Print PDF | Yes (CLI) | Native API |
| **Platform** | | |
| Windows | Yes | Yes |
| Linux | No | Yes |
| macOS | No | Yes |
| Web Apps | No | Yes |
| Azure/AWS | No | Yes |

---

## Migration Scenarios

### Scenario 1: Desktop Application Showing PDFs

**Old Approach:** Launch Sumatra as external process
```csharp
Process.Start("SumatraPDF.exe", pdfPath);
```

**New Approach:** Use IronPDF's built-in viewer or system viewer
```csharp
using IronPdf;
using IronPdf.Viewing;

// Option 1: IronPDF's built-in viewer (WinForms/WPF)
var pdf = PdfDocument.FromFile("document.pdf");
var viewer = new PdfViewer();
viewer.LoadPdf(pdf);

// Option 2: Create PDF and open with system viewer
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(htmlContent);
var tempPath = Path.GetTempFileName() + ".pdf";
pdf.SaveAs(tempPath);
Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
```

### Scenario 2: Web Application PDF Generation

**Old Approach:** Not possible with Sumatra

**New Approach:**
```csharp
// ASP.NET Core Controller
public IActionResult GenerateReport()
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(GetReportHtml());

    return File(pdf.BinaryData, "application/pdf", "report.pdf");
}
```

### Scenario 3: Batch PDF Processing

**Old Approach:** Not possible with Sumatra

**New Approach:**
```csharp
var htmlFiles = Directory.GetFiles("input", "*.html");
var renderer = new ChromePdfRenderer();

foreach (var htmlFile in htmlFiles)
{
    var pdf = renderer.RenderHtmlFileAsPdf(htmlFile);
    var outputPath = Path.ChangeExtension(htmlFile, ".pdf");
    pdf.SaveAs(outputPath);
}
```

---

## Common Migration Issues

### Issue 1: User Must Install Sumatra

**Problem:** Your app requires users to install Sumatra.

**Solution:** IronPDF is bundled with your app - no external dependencies.

### Issue 2: GPL License Restrictions

**Problem:** Sumatra's GPL license may conflict with proprietary software.

**Solution:** IronPDF has commercial licensing compatible with proprietary software.

### Issue 3: Cannot Create PDFs

**Problem:** Sumatra can only view, not create PDFs.

**Solution:** IronPDF creates PDFs from HTML, URLs, images, and more.

### Issue 4: Process Management Overhead

**Problem:** Managing external Sumatra processes is complex.

**Solution:** IronPDF is an in-process library with simple API calls.

---

## Migration Checklist

```markdown
## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Sumatra PDF usages in codebase**
  ```bash
  grep -r "SumatraPDF" --include="*.cs" .
  grep -r "Process.Start" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage from Sumatra PDF to IronPDF.

- [ ] **Document current PDF handling configurations**
  ```csharp
  // Find patterns like:
  var process = new ProcessStartInfo {
      FileName = "SumatraPDF.exe",
      Arguments = "-print-to-default file.pdf"
  };
  ```
  **Why:** These configurations map to IronPDF's capabilities. Document them now to ensure consistent behavior after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove Sumatra PDF process handling and install IronPdf**
  ```bash
  // Before (Sumatra PDF)
  // No package to remove, but remove any process handling code

  // After (IronPDF)
  dotnet add package IronPdf
  ```
  **Why:** Transition from external process handling to native .NET PDF handling with IronPDF.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace PDF viewing/printing calls**
  ```csharp
  // Before (Sumatra PDF)
  var process = new ProcessStartInfo {
      FileName = "SumatraPDF.exe",
      Arguments = "-print-to-default file.pdf"
  };
  Process.Start(process);

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("file.pdf");
  pdf.Print();
  ```
  **Why:** IronPDF allows direct PDF printing without external processes, improving integration and reliability.

- [ ] **Implement PDF creation and editing**
  ```csharp
  // Before (Sumatra PDF)
  // Not possible

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** IronPDF enables creating and editing PDFs directly within your application.

- [ ] **Enable advanced PDF features**
  ```csharp
  // Before (Sumatra PDF)
  // Not possible

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("file.pdf");
  pdf.SecuritySettings.UserPassword = "secret";
  pdf.ApplyWatermark("<h1 style='color:red; opacity:0.3;'>CONFIDENTIAL</h1>");
  ```
  **Why:** IronPDF provides advanced features like security settings and watermarks, enhancing document management capabilities.

### Post-Migration

- [ ] **Run all tests**
  **Why:** Verify all PDF operations work correctly after migration.

- [ ] **Compare PDF output quality**
  **Why:** Ensure the output meets expectations with IronPDF's rendering engine.

- [ ] **Test new PDF capabilities**
  **Why:** Leverage IronPDF's features like merging, splitting, and digital signatures to enhance your application's functionality.

- [ ] **Add new capabilities (optional)**
  ```csharp
  // Features now available:
  // Merge PDFs
  var merged = PdfDocument.Merge(pdf1, pdf2);

  // Digital signatures
  var signature = new PdfSignature("cert.pfx", "password");
  pdf.Sign(signature);
  ```
  **Why:** IronPDF provides many features that may not have been available with Sumatra PDF.
```
## Migration Checklist

### Pre-Migration

- [ ] **Identify all Sumatra process launches**
  ```csharp
  // Before (Sumatra)
  Process.Start("SumatraPDF.exe", "file.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("file.pdf");
  ```
  **Why:** Replace external process launches with direct PDF handling in code.

- [ ] **Document print workflows**
  ```csharp
  // Before (Sumatra)
  Process.Start("SumatraPDF.exe", "-print-to-default file.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("file.pdf");
  pdf.Print();
  ```
  **Why:** Integrate printing directly into the application without external dependencies.

- [ ] **Note any Sumatra command-line arguments used**
  **Why:** Ensure all functionalities are covered when migrating to IronPDF.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Updates

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add IronPDF to your project to enable PDF generation and manipulation.

- [ ] **Remove Sumatra process code**
  ```csharp
  // Before (Sumatra)
  Process.Start("SumatraPDF.exe", "file.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("file.pdf");
  ```
  **Why:** Eliminate dependency on external processes for PDF handling.

- [ ] **Implement PDF generation with IronPDF**
  ```csharp
  // Before (Sumatra)
  // No direct PDF generation capability

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
  pdf.SaveAs("output.pdf");
  ```
  **Why:** Use IronPDF to create PDFs directly from HTML content.

- [ ] **Update print functionality**
  ```csharp
  // Before (Sumatra)
  Process.Start("SumatraPDF.exe", "-print-to-default file.pdf");

  // After (IronPDF)
  var pdf = PdfDocument.FromFile("file.pdf");
  pdf.Print();
  ```
  **Why:** Directly print PDFs from within the application using IronPDF.

- [ ] **Add license initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** Ensure the application is licensed for IronPDF usage.

### Testing

- [ ] **Test PDF generation quality**
  **Why:** Verify that PDFs generated with IronPDF meet quality expectations.

- [ ] **Verify print functionality**
  **Why:** Ensure that the new print implementation works correctly across all scenarios.

- [ ] **Test on all target platforms**
  **Why:** Confirm that the application behaves consistently across different environments.

- [ ] **Verify no Sumatra dependency remains**
  **Why:** Ensure that all references to Sumatra are removed and the application is fully migrated to IronPDF.

### Cleanup

- [ ] **Remove Sumatra from installers**
  **Why:** Eliminate unnecessary dependencies from the installation process.

- [ ] **Update documentation**
  **Why:** Reflect the changes in the documentation to guide users and developers.

- [ ] **Remove Sumatra from system requirements**
  **Why:** Simplify system requirements by removing the need for Sumatra PDF.
```
---

## Additional Resources

- [IronPDF Documentation](https://ironpdf.com/docs/)
- [IronPDF Tutorials](https://ironpdf.com/tutorials/)
- [HTML to PDF Guide](https://ironpdf.com/how-to/html-file-to-pdf/)
- [IronPDF Examples](https://ironpdf.com/examples/)
- [Sumatra PDF to IronPDF Transition Guide](https://ironpdf.com/blog/migration-guides/migrate-from-sumatra-pdf-to-ironpdf/)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
