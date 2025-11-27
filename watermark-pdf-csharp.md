# Add Watermarks to PDF in C#: Text, Image, and Stamp Guide

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF

[![.NET](https://img.shields.io/badge/.NET-6%2B-512BD4)](https://dotnet.microsoft.com/)
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> Watermarks protect documents, indicate status, and add branding. This guide covers text watermarks, image stamps, and confidential markings with library comparisons.

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Library Comparison](#library-comparison)
3. [Text Watermarks](#text-watermarks)
4. [Image Watermarks](#image-watermarks)
5. [HTML Watermarks](#html-watermarks)
6. [Positioning and Styling](#positioning-and-styling)
7. [Common Use Cases](#common-use-cases)

---

## Quick Start

### Add Text Watermark with IronPDF

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
pdf.ApplyWatermark("<h1 style='color:red;opacity:0.5;'>CONFIDENTIAL</h1>");
pdf.SaveAs("watermarked.pdf");
```

### Add Image Watermark

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
pdf.ApplyWatermark("<img src='logo.png' style='opacity:0.3;width:200px;'>");
pdf.SaveAs("branded.pdf");
```

---

## Library Comparison

### Watermark Capabilities

| Library | Text Watermark | Image Watermark | HTML Watermark | Opacity Control | Per-Page |
|---------|---------------|-----------------|----------------|-----------------|----------|
| **IronPDF** | ✅ Simple | ✅ | ✅ Unique | ✅ | ✅ |
| iText7 | ✅ | ✅ | ❌ | ✅ | ✅ |
| Aspose.PDF | ✅ | ✅ | ❌ | ✅ | ✅ |
| PDFSharp | ✅ | ✅ | ❌ | ⚠️ | ✅ |
| PuppeteerSharp | ❌ | ❌ | ❌ | ❌ | ❌ |
| QuestPDF | ❌ | ❌ | ❌ | ❌ | ❌ |

**Key advantage:** IronPDF uses HTML/CSS for watermarks, enabling unlimited styling options.

### Code Complexity

**IronPDF — 2 lines:**
```csharp
pdf.ApplyWatermark("<h1 style='color:gray;opacity:0.5;'>DRAFT</h1>");
```

**iText7 — 25+ lines:**
```csharp
// Requires PdfCanvas, Rectangle calculations, transparency groups,
// font loading, coordinate positioning, and proper resource disposal
```

---

## Text Watermarks

### Simple Text

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("report.pdf");

// Red confidential stamp
pdf.ApplyWatermark("<h1 style='color:red;'>CONFIDENTIAL</h1>");

pdf.SaveAs("confidential-report.pdf");
```

### Diagonal Text

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Rotated watermark
pdf.ApplyWatermark(@"
    <h1 style='
        color: gray;
        opacity: 0.3;
        font-size: 72pt;
        transform: rotate(-45deg);
    '>DRAFT</h1>
");

pdf.SaveAs("draft-document.pdf");
```

### Multiple Lines

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

pdf.ApplyWatermark(@"
    <div style='text-align:center;opacity:0.4;'>
        <h1 style='color:#333;'>INTERNAL USE ONLY</h1>
        <p style='color:#666;'>Not for distribution</p>
    </div>
");

pdf.SaveAs("internal-document.pdf");
```

---

## Image Watermarks

### Company Logo

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("invoice.pdf");

// Add semi-transparent logo
pdf.ApplyWatermark(@"
    <img src='company-logo.png'
         style='opacity:0.2; width:300px;' />
");

pdf.SaveAs("branded-invoice.pdf");
```

### Background Image

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("certificate.pdf");

// Large background watermark
pdf.ApplyWatermark(@"
    <img src='seal.png'
         style='opacity:0.1; width:80%;' />
", VerticalAlignment.Middle, HorizontalAlignment.Center);

pdf.SaveAs("certified-document.pdf");
```

### Base64 Encoded Image

```csharp
using IronPdf;

byte[] logoBytes = File.ReadAllBytes("logo.png");
string base64Logo = Convert.ToBase64String(logoBytes);

var pdf = PdfDocument.FromFile("document.pdf");

pdf.ApplyWatermark($@"
    <img src='data:image/png;base64,{base64Logo}'
         style='opacity:0.3; width:200px;' />
");

pdf.SaveAs("watermarked.pdf");
```

---

## HTML Watermarks

IronPDF's unique advantage: full HTML/CSS watermarks.

### Complex Styled Watermark

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("contract.pdf");

pdf.ApplyWatermark(@"
    <div style='
        border: 3px solid red;
        padding: 20px;
        background: rgba(255,0,0,0.1);
        border-radius: 10px;
    '>
        <h1 style='color:red; margin:0;'>⚠️ DRAFT</h1>
        <p style='color:#666; margin:5px 0 0 0;'>
            Not legally binding - Review required
        </p>
    </div>
");

pdf.SaveAs("draft-contract.pdf");
```

### Dynamic Watermark with Date

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("report.pdf");

string watermarkHtml = $@"
    <div style='opacity:0.5; text-align:center;'>
        <p style='font-size:10pt; color:#999;'>
            Printed: {DateTime.Now:yyyy-MM-dd HH:mm}
        </p>
        <p style='font-size:8pt; color:#ccc;'>
            This copy expires in 30 days
        </p>
    </div>
";

pdf.ApplyWatermark(watermarkHtml, VerticalAlignment.Bottom, HorizontalAlignment.Center);

pdf.SaveAs("time-stamped.pdf");
```

### QR Code Watermark

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// QR code linking to verification page
string qrCodeUrl = "https://api.qrserver.com/v1/create-qr-code/?size=100x100&data=https://verify.example.com/doc123";

pdf.ApplyWatermark($@"
    <div style='opacity:0.7;'>
        <img src='{qrCodeUrl}' style='width:80px;' />
        <p style='font-size:8pt; color:#333;'>Scan to verify</p>
    </div>
", VerticalAlignment.Bottom, HorizontalAlignment.Right);

pdf.SaveAs("verifiable-document.pdf");
```

---

## Positioning and Styling

### Position Options

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Center (default)
pdf.ApplyWatermark("<h1>CENTER</h1>",
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);

// Top-left corner
pdf.ApplyWatermark("<h1>TOP LEFT</h1>",
    VerticalAlignment.Top,
    HorizontalAlignment.Left);

// Bottom-right corner
pdf.ApplyWatermark("<h1>BOTTOM RIGHT</h1>",
    VerticalAlignment.Bottom,
    HorizontalAlignment.Right);
```

### Opacity Control

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Very subtle (10% opacity)
pdf.ApplyWatermark("<h1 style='opacity:0.1;'>WATERMARK</h1>");

// Medium (30% opacity)
pdf.ApplyWatermark("<h1 style='opacity:0.3;'>WATERMARK</h1>");

// Prominent (60% opacity)
pdf.ApplyWatermark("<h1 style='opacity:0.6;'>WATERMARK</h1>");
```

### Specific Pages Only

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Watermark only first page
var firstPage = pdf.CopyPage(0);
firstPage.ApplyWatermark("<h1>COVER PAGE</h1>");

// Watermark remaining pages differently
for (int i = 1; i < pdf.PageCount; i++)
{
    var page = pdf.CopyPage(i);
    page.ApplyWatermark("<p style='opacity:0.2;'>Page " + (i + 1) + "</p>",
        VerticalAlignment.Bottom, HorizontalAlignment.Right);
}
```

---

## Common Use Cases

### Confidential Documents

```csharp
using IronPdf;

public void ApplyConfidentialWatermark(string inputPath, string outputPath, string classification)
{
    var pdf = PdfDocument.FromFile(inputPath);

    string color = classification switch
    {
        "TOP SECRET" => "red",
        "SECRET" => "orange",
        "CONFIDENTIAL" => "blue",
        _ => "gray"
    };

    pdf.ApplyWatermark($@"
        <div style='text-align:center;'>
            <h1 style='color:{color}; opacity:0.4; font-size:48pt;
                       transform:rotate(-30deg);'>
                {classification}
            </h1>
        </div>
    ");

    pdf.SaveAs(outputPath);
}
```

### Draft Documents

```csharp
using IronPdf;

public void MarkAsDraft(string filePath)
{
    var pdf = PdfDocument.FromFile(filePath);

    pdf.ApplyWatermark(@"
        <div style='transform:rotate(-45deg); opacity:0.15;'>
            <h1 style='font-size:120pt; color:#333; margin:0;'>DRAFT</h1>
        </div>
    ");

    pdf.SaveAs(filePath.Replace(".pdf", "-draft.pdf"));
}
```

### Approved/Rejected Stamps

```csharp
using IronPdf;

public void ApplyApprovalStamp(string filePath, bool approved, string approver)
{
    var pdf = PdfDocument.FromFile(filePath);

    string stampHtml = approved
        ? $@"<div style='border:3px solid green; padding:15px; background:rgba(0,255,0,0.1);'>
               <h2 style='color:green; margin:0;'>✓ APPROVED</h2>
               <p style='margin:5px 0 0 0;'>By: {approver}</p>
               <p style='margin:0; font-size:10pt;'>{DateTime.Now:yyyy-MM-dd}</p>
             </div>"
        : $@"<div style='border:3px solid red; padding:15px; background:rgba(255,0,0,0.1);'>
               <h2 style='color:red; margin:0;'>✗ REJECTED</h2>
               <p style='margin:5px 0 0 0;'>By: {approver}</p>
               <p style='margin:0; font-size:10pt;'>{DateTime.Now:yyyy-MM-dd}</p>
             </div>";

    pdf.ApplyWatermark(stampHtml,
        VerticalAlignment.Top,
        HorizontalAlignment.Right);

    pdf.SaveAs(filePath.Replace(".pdf", $"-{(approved ? "approved" : "rejected")}.pdf"));
}
```

### Copy Protection

```csharp
using IronPdf;

public void ApplyCopyProtection(string filePath, string recipientName)
{
    var pdf = PdfDocument.FromFile(filePath);

    // Add recipient-specific watermark to discourage sharing
    pdf.ApplyWatermark($@"
        <div style='opacity:0.15; transform:rotate(-30deg);'>
            <p style='font-size:14pt; color:#333;'>
                Licensed to: {recipientName}<br/>
                Copy ID: {Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}
            </p>
        </div>
    ");

    // Also add password protection
    pdf.SecuritySettings.OwnerPassword = "admin123";
    pdf.SecuritySettings.UserPassword = "";  // Can view but not edit

    pdf.SaveAs($"protected-{recipientName.Replace(" ", "-")}.pdf");
}
```

---

## Recommendations

### Choose IronPDF for Watermarks When:
- ✅ You want HTML/CSS styling flexibility
- ✅ You need dynamic watermarks with variables
- ✅ Complex layouts (borders, backgrounds, QR codes)
- ✅ You're also doing other PDF operations

### Choose iText7 When:
- You need very precise coordinate positioning
- You're already in the iText ecosystem

### Cannot Watermark:
- ❌ PuppeteerSharp — Generation only
- ❌ QuestPDF — Generation only
- ❌ wkhtmltopdf — Generation only

---

## Related Tutorials

- **[Digital Signatures](digital-signatures-pdf-csharp.md)** — Sign watermarked documents
- **[Merge PDFs](merge-split-pdf-csharp.md)** — Watermark combined documents
- **[PDF/A Compliance](pdf-a-compliance-csharp.md)** — Accessible watermarks
- **[Best PDF Libraries](best-pdf-libraries-dotnet-2025.md)** — Full comparison

---

### More Tutorials
- **[HTML to PDF](html-to-pdf-csharp.md)** — Generate PDFs to watermark
- **[PDF to Image](pdf-to-image-csharp.md)** — Convert watermarked PDFs
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web watermarking
- **[IronPDF Guide](ironpdf/)** — Full watermark API

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*
