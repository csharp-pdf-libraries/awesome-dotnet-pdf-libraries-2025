# How Do I Migrate from Winnovative to IronPDF in C#?

## Why Migrate from Winnovative?

### The Outdated Rendering Engine Problem

Winnovative relies on a **WebKit engine from 2016** that creates serious problems for modern web applications:

1. **No CSS Grid Support**: Bootstrap 5, Tailwind CSS, and modern layouts break completely
2. **Buggy Flexbox Implementation**: Inconsistent rendering compared to modern browsers
3. **ES5 JavaScript Only**: Modern ES6+ JavaScript (arrow functions, async/await, classes) fails silently
4. **Stagnant Development**: Despite "Winnovative" suggesting innovation, minimal updates in recent years
5. **Font Rendering Issues**: Web fonts and custom typography often render incorrectly
6. **Security Concerns**: 2016-era WebKit lacks years of security patches

### Real-World Impact

```html
<!-- This modern CSS breaks in Winnovative -->
<div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px;">
  <div>Column 1</div>
  <div>Column 2</div>
  <div>Column 3</div>
</div>

<!-- Modern JavaScript fails silently -->
<script>
const items = data.map(item => item.name); // Arrow functions: FAIL
const result = await fetchData(); // Async/await: FAIL
class Report { } // Classes: FAIL
</script>
```

### Quick Migration Comparison

| Aspect | Winnovative | IronPDF |
|--------|-------------|---------|
| Rendering Engine | WebKit (2016) | Chromium (Current) |
| CSS Grid | Not Supported | Full Support |
| Flexbox | Buggy | Full Support |
| JavaScript | ES5 only | ES2024 |
| Bootstrap 5 | Broken | Full Support |
| Tailwind CSS | Not Supported | Full Support |
| React/Vue SSR | Problematic | Works Perfectly |
| Web Fonts | Unreliable | Full Support |
| Updates | Infrequent | Monthly |
| Price | $750-$1,600 | Competitive |

For a complete walkthrough with working code, the [Winnovative to IronPDF guide](https://ironpdf.com/blog/migration-guides/migrate-from-winnovative-to-ironpdf/) covers every API pattern and common migration scenario.

---

## Quick Start: Winnovative to IronPDF in 5 Minutes

### Step 1: Replace NuGet Package

```bash
# Remove Winnovative
dotnet remove package Winnovative.WebKitHtmlToPdf
dotnet remove package Winnovative.HtmlToPdf
dotnet remove package Winnovative.WebToPdfConverter

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Update Namespaces

```csharp
// Before
using Winnovative;
using Winnovative.WebKit;

// After
using IronPdf;
```

### Step 3: Convert Basic Code

**Winnovative:**
```csharp
var converter = new HtmlToPdfConverter();
converter.LicenseKey = "your-winnovative-key";
byte[] pdfBytes = converter.ConvertUrl("https://example.com");
File.WriteAllBytes("output.pdf", pdfBytes);
```

**IronPDF:**
```csharp
IronPdf.License.LicenseKey = "YOUR-IRONPDF-KEY";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("output.pdf");
```

---

## Complete API Reference

### Core Classes Mapping

| Winnovative Class | IronPDF Equivalent | Notes |
|-------------------|-------------------|-------|
| `HtmlToPdfConverter` | `ChromePdfRenderer` | Main conversion class |
| `PdfDocument` | `PdfDocument` | PDF manipulation |
| `PdfPage` | `PdfDocument.Pages[]` | Page access |
| `PdfDocumentOptions` | `RenderingOptions` | Configuration |
| `PdfHeaderOptions` | `HtmlHeaderFooter` | Headers |
| `PdfFooterOptions` | `HtmlHeaderFooter` | Footers |
| `TextElement` | HTML in `HtmlFragment` | Text positioning |
| `ImageElement` | HTML `<img>` | Image placement |
| `PdfSecurityOptions` | `SecuritySettings` | Security |

### Method Mapping

| Winnovative Method | IronPDF Method |
|-------------------|----------------|
| `ConvertUrl(url)` | `RenderUrlAsPdf(url)` |
| `ConvertUrlToFile(url, path)` | `RenderUrlAsPdf(url).SaveAs(path)` |
| `ConvertHtml(html, baseUrl)` | `RenderHtmlAsPdf(html)` |
| `ConvertHtmlToFile(html, path)` | `RenderHtmlAsPdf(html).SaveAs(path)` |
| `ConvertHtmlFile(path)` | `RenderHtmlFileAsPdf(path)` |
| `MergePdf(streams)` | `PdfDocument.Merge(pdfs)` |
| `AppendPdf(pdf)` | `pdf1.AppendPdf(pdf2)` |
| `AddElement(element)` | HTML-based headers/footers |

### Options Mapping

| Winnovative Option | IronPDF Option |
|-------------------|----------------|
| `PdfPageSize.A4` | `PaperSize = PdfPaperSize.A4` |
| `PdfPageSize.Letter` | `PaperSize = PdfPaperSize.Letter` |
| `PdfPageOrientation.Portrait` | `PaperOrientation = PdfPaperOrientation.Portrait` |
| `PdfPageOrientation.Landscape` | `PaperOrientation = PdfPaperOrientation.Landscape` |
| `TopMargin = 20` | `MarginTop = 20` |
| `BottomMargin = 20` | `MarginBottom = 20` |
| `LeftMargin = 15` | `MarginLeft = 15` |
| `RightMargin = 15` | `MarginRight = 15` |
| `ShowHeader = true` | Set `HtmlHeader` property |
| `ShowFooter = true` | Set `HtmlFooter` property |
| `LiveUrlsEnabled = true` | Links work by default |
| `JavaScriptEnabled = true` | `EnableJavaScript = true` |
| `InternalLinksEnabled = true` | Links work by default |
| `EmbedFonts = true` | Fonts embedded by default |

---

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Winnovative:**
```csharp
using Winnovative;

public byte[] ConvertHtml(string html)
{
    HtmlToPdfConverter converter = new HtmlToPdfConverter();
    converter.LicenseKey = "your-license-key";

    // Set page options
    converter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
    converter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;

    // Set margins
    converter.PdfDocumentOptions.TopMargin = 20;
    converter.PdfDocumentOptions.BottomMargin = 20;
    converter.PdfDocumentOptions.LeftMargin = 15;
    converter.PdfDocumentOptions.RightMargin = 15;

    // Convert
    return converter.ConvertHtml(html, "https://example.com");
}
```

**IronPDF:**
```csharp
using IronPdf;

public PdfDocument ConvertHtml(string html)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    // Set page options
    renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
    renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

    // Set margins
    renderer.RenderingOptions.MarginTop = 20;
    renderer.RenderingOptions.MarginBottom = 20;
    renderer.RenderingOptions.MarginLeft = 15;
    renderer.RenderingOptions.MarginRight = 15;

    // Convert
    return renderer.RenderHtmlAsPdf(html);
}
```

### Example 2: URL to PDF with JavaScript Wait

**Winnovative:**
```csharp
using Winnovative;

public void ConvertUrl(string url, string outputPath)
{
    HtmlToPdfConverter converter = new HtmlToPdfConverter();
    converter.LicenseKey = "your-license-key";

    // Enable JavaScript
    converter.PdfDocumentOptions.JavaScriptEnabled = true;

    // Wait for JavaScript (limited support)
    converter.ConversionDelay = 3000; // 3 seconds

    // Convert URL
    byte[] pdfBytes = converter.ConvertUrl(url);

    // Save
    File.WriteAllBytes(outputPath, pdfBytes);
}
```

**IronPDF:**
```csharp
using IronPdf;

public void ConvertUrl(string url, string outputPath)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    // Enable JavaScript (on by default)
    renderer.RenderingOptions.EnableJavaScript = true;

    // Wait for JavaScript completion
    renderer.RenderingOptions.WaitFor.JavaScript(5000); // Wait up to 5 seconds

    // Or wait for specific element
    renderer.RenderingOptions.WaitFor.HtmlElementById("content-loaded", 10000);

    // Or wait for network idle
    renderer.RenderingOptions.WaitFor.NetworkIdle0(5000);

    // Convert URL
    var pdf = renderer.RenderUrlAsPdf(url);

    // Save
    pdf.SaveAs(outputPath);
}
```

### Example 3: Headers and Footers

**Winnovative:**
```csharp
using Winnovative;
using System.Drawing;

public byte[] ConvertWithHeaderFooter(string html)
{
    HtmlToPdfConverter converter = new HtmlToPdfConverter();
    converter.LicenseKey = "your-license-key";

    // Configure header
    converter.PdfDocumentOptions.ShowHeader = true;
    converter.PdfHeaderOptions.HeaderHeight = 50;

    // Add header text element
    TextElement headerText = new TextElement(0, 15, "Company Report",
        new Font("Arial", 12, FontStyle.Bold));
    headerText.ForeColor = Color.Navy;
    converter.PdfHeaderOptions.AddElement(headerText);

    // Add header line
    LineElement headerLine = new LineElement(0, 45,
        converter.PdfDocumentOptions.PdfPageSize.Width, 45);
    headerLine.ForeColor = Color.Navy;
    converter.PdfHeaderOptions.AddElement(headerLine);

    // Configure footer
    converter.PdfDocumentOptions.ShowFooter = true;
    converter.PdfFooterOptions.FooterHeight = 40;

    // Add page numbers
    TextElement pageNumber = new TextElement(0, 10, "Page &p; of &P;",
        new Font("Arial", 9));
    pageNumber.TextAlign = HorizontalTextAlign.Center;
    converter.PdfFooterOptions.AddElement(pageNumber);

    // Add date
    TextElement dateText = new TextElement(0, 10, "&d;", new Font("Arial", 9));
    dateText.TextAlign = HorizontalTextAlign.Right;
    converter.PdfFooterOptions.AddElement(dateText);

    return converter.ConvertHtml(html, "");
}
```

**IronPDF:**
```csharp
using IronPdf;

public PdfDocument ConvertWithHeaderFooter(string html)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();

    // Configure header with full HTML/CSS support
    renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
    {
        HtmlFragment = @"
            <div style='width: 100%; font-family: Arial, sans-serif;'>
                <div style='color: navy; font-size: 14px; font-weight: bold;'>
                    Company Report
                </div>
                <hr style='border: 1px solid navy; margin-top: 10px;'>
            </div>",
        DrawDividerLine = false,
        MaxHeight = 50
    };

    // Configure footer with page numbers and date
    renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
    {
        HtmlFragment = @"
            <div style='width: 100%; font-family: Arial; font-size: 9px; display: flex; justify-content: space-between;'>
                <span></span>
                <span>Page {page} of {total-pages}</span>
                <span>{date}</span>
            </div>",
        MaxHeight = 40
    };

    return renderer.RenderHtmlAsPdf(html);
}
```

### Example 4: Merge Multiple PDFs

**Winnovative:**
```csharp
using Winnovative;

public void MergePdfs(string[] inputPaths, string outputPath)
{
    HtmlToPdfConverter converter = new HtmlToPdfConverter();
    converter.LicenseKey = "your-license-key";

    // Load first document
    PdfDocument mergedDocument = new PdfDocument();

    foreach (string path in inputPaths)
    {
        // Load each PDF
        byte[] pdfBytes = File.ReadAllBytes(path);
        PdfDocument doc = new PdfDocument(pdfBytes);

        // Append pages
        mergedDocument.AppendDocument(doc);
    }

    // Save merged document
    mergedDocument.Save(outputPath);
}
```

**IronPDF:**
```csharp
using IronPdf;
using System.Collections.Generic;
using System.Linq;

public void MergePdfs(string[] inputPaths, string outputPath)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    // Load all PDFs
    var pdfs = inputPaths.Select(path => PdfDocument.FromFile(path)).ToList();

    // Merge all at once
    var merged = PdfDocument.Merge(pdfs);

    // Save
    merged.SaveAs(outputPath);

    // Alternative: Append one by one
    var basePdf = PdfDocument.FromFile(inputPaths[0]);
    for (int i = 1; i < inputPaths.Length; i++)
    {
        var appendPdf = PdfDocument.FromFile(inputPaths[i]);
        basePdf.AppendPdf(appendPdf);
    }
    basePdf.SaveAs(outputPath);
}
```

### Example 5: Add Watermark

**Winnovative:**
```csharp
using Winnovative;
using System.Drawing;

public void AddWatermark(string inputPath, string outputPath)
{
    // Load PDF
    PdfDocument document = new PdfDocument(inputPath);

    // Create watermark text
    foreach (PdfPage page in document.Pages)
    {
        // Create text element for watermark
        TextElement watermark = new TextElement(0, 0, "CONFIDENTIAL",
            new Font("Arial", 72, FontStyle.Bold));
        watermark.ForeColor = Color.FromArgb(50, 255, 0, 0); // Semi-transparent red
        watermark.TextAlign = HorizontalTextAlign.Center;

        // Calculate center position
        float x = (page.Size.Width - 400) / 2;
        float y = (page.Size.Height - 72) / 2;

        // Add to page (complex positioning required)
        page.AddElement(watermark, x, y);
    }

    document.Save(outputPath);
}
```

**IronPDF:**
```csharp
using IronPdf;

public void AddWatermark(string inputPath, string outputPath)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    // Load PDF
    var pdf = PdfDocument.FromFile(inputPath);

    // Add HTML watermark (full CSS support!)
    pdf.ApplyWatermark(@"
        <div style='
            font-size: 72px;
            font-weight: bold;
            color: rgba(255, 0, 0, 0.2);
            transform: rotate(-45deg);
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%) rotate(-45deg);
        '>
            CONFIDENTIAL
        </div>");

    pdf.SaveAs(outputPath);
}

// Alternative: Image watermark
public void AddImageWatermark(string inputPath, string watermarkPath, string outputPath)
{
    var pdf = PdfDocument.FromFile(inputPath);

    pdf.ApplyWatermark($@"
        <img src='{watermarkPath}'
             style='opacity: 0.3; position: absolute; top: 50%; left: 50%;
                    transform: translate(-50%, -50%);'>");

    pdf.SaveAs(outputPath);
}
```

### Example 6: Password Protection and Security

**Winnovative:**
```csharp
using Winnovative;

public void ProtectPdf(string inputPath, string outputPath)
{
    // Load PDF
    PdfDocument document = new PdfDocument(inputPath);

    // Set security options
    document.Security.UserPassword = "user123";
    document.Security.OwnerPassword = "owner456";

    // Set permissions
    document.Security.CanPrint = true;
    document.Security.CanEditContent = false;
    document.Security.CanCopyContent = false;
    document.Security.CanEditAnnotations = false;
    document.Security.CanFillFormFields = true;

    // Set encryption
    document.Security.EncryptionKeySize = PdfEncryptionKeySize.EncryptKey128Bit;

    document.Save(outputPath);
}
```

**IronPDF:**
```csharp
using IronPdf;

public void ProtectPdf(string inputPath, string outputPath)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    // Load PDF
    var pdf = PdfDocument.FromFile(inputPath);

    // Set passwords
    pdf.SecuritySettings.OwnerPassword = "owner456";
    pdf.SecuritySettings.UserPassword = "user123";

    // Set permissions
    pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
    pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;
    pdf.SecuritySettings.AllowUserAnnotations = false;
    pdf.SecuritySettings.AllowUserFormData = true;

    // Save (encryption applied automatically)
    pdf.SaveAs(outputPath);
}

// Alternative: Create protected PDF from HTML
public void CreateProtectedPdf(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Apply protection
    pdf.SecuritySettings.OwnerPassword = "owner456";
    pdf.SecuritySettings.UserPassword = "user123";
    pdf.SecuritySettings.AllowUserCopyPasteContent = false;

    pdf.SaveAs(outputPath);
}
```

### Example 7: Extract Text from PDF

**Winnovative:**
```csharp
using Winnovative;

public string ExtractText(string pdfPath)
{
    // Load PDF
    PdfDocument document = new PdfDocument(pdfPath);

    StringBuilder allText = new StringBuilder();

    // Extract text from each page
    foreach (PdfPage page in document.Pages)
    {
        string pageText = page.ExtractText();
        allText.AppendLine(pageText);
    }

    return allText.ToString();
}
```

**IronPDF:**
```csharp
using IronPdf;

public string ExtractText(string pdfPath)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    // Load PDF
    var pdf = PdfDocument.FromFile(pdfPath);

    // Extract all text at once
    string allText = pdf.ExtractAllText();

    return allText;
}

// Extract text from specific pages
public string ExtractTextFromPages(string pdfPath, int startPage, int endPage)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    StringBuilder text = new StringBuilder();
    for (int i = startPage; i <= endPage && i < pdf.PageCount; i++)
    {
        text.AppendLine(pdf.ExtractTextFromPage(i));
    }

    return text.ToString();
}
```

### Example 8: Split PDF into Separate Pages

**Winnovative:**
```csharp
using Winnovative;

public void SplitPdf(string inputPath, string outputFolder)
{
    // Load PDF
    PdfDocument document = new PdfDocument(inputPath);

    for (int i = 0; i < document.Pages.Count; i++)
    {
        // Create new document for each page
        PdfDocument newDoc = new PdfDocument();

        // Copy page
        PdfPage page = document.Pages[i];
        newDoc.AddPage(page);

        // Save
        string outputPath = Path.Combine(outputFolder, $"page_{i + 1}.pdf");
        newDoc.Save(outputPath);
    }
}
```

**IronPDF:**
```csharp
using IronPdf;

public void SplitPdf(string inputPath, string outputFolder)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    // Load PDF
    var pdf = PdfDocument.FromFile(inputPath);

    // Split into individual pages
    for (int i = 0; i < pdf.PageCount; i++)
    {
        // Copy specific page
        var singlePage = pdf.CopyPage(i);

        // Save
        string outputPath = Path.Combine(outputFolder, $"page_{i + 1}.pdf");
        singlePage.SaveAs(outputPath);
    }
}

// Alternative: Split into ranges
public void SplitPdfByRange(string inputPath, int[] pageNumbers, string outputPath)
{
    var pdf = PdfDocument.FromFile(inputPath);
    var subset = pdf.CopyPages(pageNumbers);
    subset.SaveAs(outputPath);
}
```

### Example 9: Add Images to PDF

**Winnovative:**
```csharp
using Winnovative;
using System.Drawing;

public byte[] CreatePdfWithImage(string html, string imagePath)
{
    HtmlToPdfConverter converter = new HtmlToPdfConverter();
    converter.LicenseKey = "your-license-key";

    // Convert HTML first
    byte[] pdfBytes = converter.ConvertHtml(html, "");

    // Load resulting PDF
    PdfDocument document = new PdfDocument(pdfBytes);

    // Get first page
    PdfPage page = document.Pages[0];

    // Load image
    Image img = Image.FromFile(imagePath);

    // Create image element
    ImageElement imageElement = new ImageElement(10, 10, 100, 100, img);

    // Add to page
    page.AddElement(imageElement);

    // Get bytes
    return document.Save();
}
```

**IronPDF:**
```csharp
using IronPdf;

public PdfDocument CreatePdfWithImage(string html, string imagePath)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    // Simply include image in HTML (much simpler!)
    string htmlWithImage = $@"
        {html}
        <img src='{imagePath}' style='width: 100px; height: 100px; position: absolute; top: 10px; left: 10px;'>";

    var renderer = new ChromePdfRenderer();
    return renderer.RenderHtmlAsPdf(htmlWithImage);
}

// Alternative: Stamp image on existing PDF
public void StampImageOnPdf(string pdfPath, string imagePath, string outputPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    // Apply image as stamp/watermark
    pdf.ApplyStamp(new ImageStamper(imagePath)
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalOffset = 10,
        VerticalOffset = 10,
        Width = 100,
        Height = 100,
        Opacity = 100
    });

    pdf.SaveAs(outputPath);
}
```

### Example 10: Fill PDF Forms

**Winnovative:**
```csharp
using Winnovative;

public void FillPdfForm(string inputPath, string outputPath)
{
    // Load PDF with forms
    PdfDocument document = new PdfDocument(inputPath);

    // Get form fields
    PdfFormFields formFields = document.Form.Fields;

    // Fill text fields
    PdfFormField nameField = formFields["name"];
    if (nameField != null)
    {
        nameField.Value = "John Doe";
    }

    PdfFormField emailField = formFields["email"];
    if (emailField != null)
    {
        emailField.Value = "john@example.com";
    }

    // Check checkbox
    PdfFormField agreeField = formFields["agree"];
    if (agreeField != null && agreeField is PdfCheckBoxField)
    {
        ((PdfCheckBoxField)agreeField).Checked = true;
    }

    document.Save(outputPath);
}
```

**IronPDF:**
```csharp
using IronPdf;

public void FillPdfForm(string inputPath, string outputPath)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    // Load PDF with forms
    var pdf = PdfDocument.FromFile(inputPath);

    // Get form field collection
    var form = pdf.Form;

    // Fill text fields by name
    form.GetFieldByName("name").Value = "John Doe";
    form.GetFieldByName("email").Value = "john@example.com";

    // Check checkbox
    var agreeField = form.GetFieldByName("agree");
    if (agreeField != null)
    {
        agreeField.Value = "Yes"; // or "true" depending on form setup
    }

    // Save filled form
    pdf.SaveAs(outputPath);
}

// List all form fields
public void ListFormFields(string pdfPath)
{
    var pdf = PdfDocument.FromFile(pdfPath);

    foreach (var field in pdf.Form.Fields)
    {
        Console.WriteLine($"Field: {field.Name}, Type: {field.Type}, Value: {field.Value}");
    }
}
```

### Example 11: Digital Signatures

**Winnovative:**
```csharp
using Winnovative;
using System.Security.Cryptography.X509Certificates;

public void SignPdf(string inputPath, string outputPath, string certPath, string certPassword)
{
    // Load PDF
    PdfDocument document = new PdfDocument(inputPath);

    // Load certificate
    X509Certificate2 certificate = new X509Certificate2(certPath, certPassword);

    // Create signature
    PdfDigitalSignature signature = new PdfDigitalSignature(document);
    signature.SignatureCertificate = certificate;
    signature.SignatureReason = "Document approval";
    signature.SignatureLocation = "New York";
    signature.ContactInfo = "contact@example.com";

    // Apply signature
    signature.Sign();

    document.Save(outputPath);
}
```

**IronPDF:**
```csharp
using IronPdf;
using IronPdf.Signing;

public void SignPdf(string inputPath, string outputPath, string certPath, string certPassword)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    // Load PDF
    var pdf = PdfDocument.FromFile(inputPath);

    // Create signature with certificate
    var signature = new PdfSignature(certPath, certPassword)
    {
        SigningReason = "Document approval",
        SigningLocation = "New York",
        SigningContact = "contact@example.com"
    };

    // Sign and save
    pdf.Sign(signature);
    pdf.SaveAs(outputPath);
}

// Create visible signature with image
public void SignPdfWithVisibleSignature(string inputPath, string outputPath,
    string certPath, string certPassword, string signatureImagePath)
{
    var pdf = PdfDocument.FromFile(inputPath);

    var signature = new PdfSignature(certPath, certPassword)
    {
        SigningReason = "Approved",
        SigningLocation = "New York"
    };

    // Add visible signature image
    pdf.Sign(signature);

    // Add signature image as stamp
    pdf.ApplyStamp(new ImageStamper(signatureImagePath)
    {
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Bottom,
        Width = 150,
        Height = 50
    }, 0); // First page only

    pdf.SaveAs(outputPath);
}
```

### Example 12: Convert to PDF/A for Archiving

**Winnovative:**
```csharp
using Winnovative;

public void ConvertToPdfA(string inputPath, string outputPath)
{
    // Load PDF
    PdfDocument document = new PdfDocument(inputPath);

    // Set PDF/A compliance (limited support)
    document.Conformance = PdfConformance.PdfA1b;

    // Save
    document.Save(outputPath);
}
```

**IronPDF:**
```csharp
using IronPdf;

public void ConvertToPdfA(string inputPath, string outputPath)
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    // Load PDF
    var pdf = PdfDocument.FromFile(inputPath);

    // Convert to PDF/A-3b
    pdf.SaveAsPdfA(outputPath, PdfAVersions.PdfA3b);
}

// Create PDF/A directly from HTML
public void CreatePdfA(string html, string outputPath)
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf(html);

    // Save as PDF/A
    pdf.SaveAsPdfA(outputPath, PdfAVersions.PdfA3b);
}
```

---

## Feature Comparison Table

| Feature | Winnovative | IronPDF |
|---------|-------------|---------|
| **Rendering** | | |
| CSS Grid | Not Supported | Full Support |
| Flexbox | Buggy | Full Support |
| CSS3 Animations | Not Supported | Supported |
| Web Fonts | Unreliable | Full Support |
| SVG | Basic | Full Support |
| Canvas | Limited | Full Support |
| **JavaScript** | | |
| ES6+ | Not Supported | Full ES2024 |
| Async/Await | Not Supported | Supported |
| Arrow Functions | Not Supported | Supported |
| Classes | Not Supported | Supported |
| Modules | Not Supported | Supported |
| **Frameworks** | | |
| Bootstrap 5 | Broken | Full Support |
| Tailwind CSS | Not Supported | Full Support |
| React SSR | Problematic | Works |
| Vue SSR | Problematic | Works |
| Angular | Problematic | Works |
| **PDF Features** | | |
| Merge PDFs | Yes | Yes |
| Split PDFs | Yes | Yes |
| Watermarks | Basic | HTML/CSS |
| Headers/Footers | Text-based | Full HTML |
| Forms | Yes | Yes |
| Digital Signatures | Yes | Yes |
| PDF/A | Limited | Full Support |
| Encryption | Yes | Yes |
| **Platform** | | |
| Windows | Yes | Yes |
| Linux | No | Yes |
| macOS | No | Yes |
| Docker | No | Yes |
| Azure Functions | No | Yes |
| AWS Lambda | No | Yes |
| **Development** | | |
| Async Support | Limited | Full async/await |
| .NET Core | Yes | Yes |
| .NET 6/7/8 | Yes | Yes |
| .NET Framework | Yes | Yes |

---

## Common Migration Patterns

### Pattern 1: Remove CSS Workarounds

**Old Winnovative code with CSS hacks:**
```html
<!-- Winnovative-specific hacks no longer needed -->
<style>
/* Flexbox workaround - Winnovative */
.container {
    display: -webkit-flex; /* Needed for Winnovative */
    display: flex;
    -webkit-flex-direction: row; /* Winnovative prefix */
    flex-direction: row;
}

/* Grid fallback - Winnovative doesn't support grid */
.grid-layout {
    /* Fallback to floats */
    overflow: hidden;
}
.grid-layout > div {
    float: left;
    width: 33.33%;
}
</style>
```

**Clean IronPDF code:**
```html
<!-- Modern CSS works perfectly in IronPDF -->
<style>
.container {
    display: flex;
    flex-direction: row;
    gap: 20px;
}

.grid-layout {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 20px;
}
</style>
```

### Pattern 2: Use Modern JavaScript

**Old Winnovative-compatible code:**
```javascript
// ES5 code for Winnovative
var items = data.map(function(item) {
    return item.name;
});

function fetchData(callback) {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function() {
        if (xhr.readyState === 4) {
            callback(JSON.parse(xhr.responseText));
        }
    };
    xhr.open('GET', '/api/data');
    xhr.send();
}
```

**Modern IronPDF code:**
```javascript
// Modern ES2024 works in IronPDF
const items = data.map(item => item.name);

async function fetchData() {
    const response = await fetch('/api/data');
    return response.json();
}

// Classes, destructuring, template literals all work
class Report {
    constructor({ title, data }) {
        this.title = title;
        this.data = data;
    }

    render() {
        return `<h1>${this.title}</h1>`;
    }
}
```

### Pattern 3: Convert Text Headers to HTML

**Winnovative text-based headers:**
```csharp
// Complex text positioning
converter.PdfHeaderOptions.HeaderHeight = 60;
TextElement title = new TextElement(10, 10, "Report Title", new Font("Arial", 14));
TextElement subtitle = new TextElement(10, 30, "Generated: " + DateTime.Now, new Font("Arial", 10));
LineElement line = new LineElement(0, 55, pageWidth, 55);
converter.PdfHeaderOptions.AddElement(title);
converter.PdfHeaderOptions.AddElement(subtitle);
converter.PdfHeaderOptions.AddElement(line);
```

**IronPDF HTML headers:**
```csharp
// Simple HTML with full CSS
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"
        <div style='font-family: Arial; padding: 10px; border-bottom: 2px solid #333;'>
            <div style='font-size: 14px; font-weight: bold;'>Report Title</div>
            <div style='font-size: 10px; color: #666;'>Generated: " + DateTime.Now.ToString("g") + @"</div>
        </div>",
    MaxHeight = 60
};
```

---

## Troubleshooting Common Migration Issues

### Issue 1: CSS Layouts Look Different

**Symptom:** Layouts that looked "okay" in Winnovative now look different in IronPDF.

**Cause:** Winnovative's 2016 WebKit had rendering bugs that developers worked around. IronPDF renders correctly.

**Solution:** Remove Winnovative-specific CSS hacks and use standard CSS:
```csharp
// Clean up legacy CSS
string cleanedHtml = html
    .Replace("-webkit-flex", "flex")
    .Replace("display: -webkit-box", "display: flex");
```

### Issue 2: JavaScript Not Executing

**Symptom:** Dynamic content not appearing in PDF.

**Cause:** Need to configure JavaScript wait options.

**Solution:**
```csharp
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.JavaScript(5000);
// Or wait for specific element
renderer.RenderingOptions.WaitFor.HtmlElementById("content-ready", 10000);
```

### Issue 3: Base URL Not Working

**Symptom:** Relative URLs for images/CSS not resolving.

**Cause:** IronPDF needs explicit base URL configuration.

**Solution:**
```csharp
renderer.RenderingOptions.BaseUrl = new Uri("https://example.com/");
```

### Issue 4: Different Page Breaks

**Symptom:** Content breaking at different points than Winnovative.

**Cause:** Different rendering engines handle page breaks differently.

**Solution:**
```css
/* Control page breaks explicitly */
.no-break {
    page-break-inside: avoid;
}
.page-break-before {
    page-break-before: always;
}
.page-break-after {
    page-break-after: always;
}
```

### Issue 5: Fonts Look Different

**Symptom:** Text appears in different fonts than expected.

**Cause:** IronPDF uses system fonts; web fonts need explicit loading.

**Solution:**
```html
<style>
@import url('https://fonts.googleapis.com/css2?family=Roboto&display=swap');

body {
    font-family: 'Roboto', Arial, sans-serif;
}
</style>
```

---

## Pre-Migration Checklist

### Assessment
- [ ] Audit all `HtmlToPdfConverter` usage in codebase
- [ ] List all Winnovative-specific features used
- [ ] Identify CSS workarounds that can be removed
- [ ] Document JavaScript compatibility requirements
- [ ] Note any custom fonts or resources used

### Search for Winnovative Usage

```bash
# Find all Winnovative references
grep -r "Winnovative" --include="*.cs" .
grep -r "HtmlToPdfConverter" --include="*.cs" .
grep -r "PdfDocumentOptions" --include="*.cs" .
grep -r "ConvertUrl\|ConvertHtml" --include="*.cs" .
```

### Environment Preparation
- [ ] Obtain IronPDF license key
- [ ] Verify .NET version compatibility
- [ ] Review IronPDF documentation
- [ ] Set up test environment

---

## Post-Migration Checklist

### Code Updates
- [ ] Remove Winnovative NuGet packages
- [ ] Install IronPdf NuGet package
- [ ] Update all namespace imports
- [ ] Convert all conversion methods
- [ ] Update page size/orientation settings
- [ ] Convert margin configurations
- [ ] Migrate header/footer code to HTML
- [ ] Update security settings
- [ ] Convert form handling code
- [ ] Update merge/split operations

### Testing
- [ ] Test basic HTML to PDF conversion
- [ ] Test URL to PDF conversion
- [ ] Verify CSS Grid layouts render correctly
- [ ] Verify Flexbox layouts render correctly
- [ ] Test JavaScript-heavy pages
- [ ] Verify Bootstrap 5 compatibility
- [ ] Test header/footer rendering
- [ ] Verify page breaks
- [ ] Test PDF merging
- [ ] Test PDF splitting
- [ ] Verify watermarks
- [ ] Test password protection
- [ ] Test digital signatures
- [ ] Verify PDF/A compliance
- [ ] Test form filling
- [ ] Performance benchmark comparison

### Cleanup
- [ ] Remove Winnovative CSS workarounds
- [ ] Update ES5 JavaScript to modern syntax
- [ ] Remove webkit prefixes from CSS
- [ ] Remove float-based grid fallbacks
- [ ] Update documentation
- [ ] Update build scripts
- [ ] Archive Winnovative license info

---

## Performance Comparison

| Operation | Winnovative | IronPDF | Notes |
|-----------|-------------|---------|-------|
| Simple HTML | ~500ms | ~300ms | IronPDF faster |
| Complex CSS | ~800ms | ~400ms | Modern CSS optimized |
| JavaScript-heavy | Often fails | ~600ms | IronPDF handles JS |
| Large documents | ~2000ms | ~1200ms | Better memory management |
| PDF merge (10 docs) | ~400ms | ~200ms | Optimized merging |
| Text extraction | ~300ms | ~150ms | Faster parsing |

---

---

## Migration Checklist

### Pre-Migration

- [ ] **Inventory all Winnovative usages in codebase**
  ```bash
  grep -r "using Winnovative" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current configurations**
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

### Code Changes

- [ ] **Remove old package and install IronPdf**
  ```bash
  dotnet remove package Winnovative
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
- [IronPDF API Reference](https://ironpdf.com/object-reference/api/)
- [HTML to PDF Tutorial](https://ironpdf.com/tutorials/html-to-pdf/)
- [IronPDF Examples](https://ironpdf.com/examples/)
- [IronPDF NuGet Package](https://www.nuget.org/packages/IronPdf)

---

*This migration guide is part of the [Awesome .NET PDF Libraries](../README.md) collection.*
