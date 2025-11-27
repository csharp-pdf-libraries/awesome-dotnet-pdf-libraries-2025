# How Can I Stamp Text, Images, and QR Codes on PDFs in C# with IronPDF?

Need to overlay "CONFIDENTIAL" across a contract, brand your PDFs with a logo, or digitally sign files in C#? IronPDF makes stamping text, images, barcodes, and even HTML onto existing PDFs straightforward and powerful. This FAQ covers the essentials—from simple watermarks to advanced dynamic stamping—so you can confidently automate document workflows in C#. 

Whether you're handling HR paperwork, legal docs, or generating invoices in bulk, this guide will show you how to put overlays exactly where you want them, with flexible options for appearance and page targeting.

---

## What Is PDF Stamping and Why Would I Use It?

PDF stamping is the process of overlaying content—like text, images, watermarks, barcodes, or HTML—on top of existing PDF documents. It's commonly used to:

- Add [watermarks](https://ironpdf.com/java/blog/using-ironpdf-for-java/java-watermark-pdf-tutorial/) such as "SAMPLE" or "APPROVED"
- Insert [digital signatures](https://ironpdf.com/nodejs/examples/digitally-sign-a-pdf/) or scanned signature images
- Stamp logos or company branding on every page
- Embed QR codes or barcodes for tracking and verification
- Mark documents clearly as "COPY" or "ORIGINAL"

If your application generates or manages documents—think HR systems, legal tech, or finance apps—stamping ensures branding, legal compliance, and traceability. IronPDF abstracts away the complexity of direct PDF editing, letting you work with simple C# objects and methods.

## Which Stamper Types Does IronPDF Offer for PDF Overlays?

IronPDF provides four dedicated stamper classes, each designed for a different overlay type:

| Stamper         | Typical Usage                                  |
|-----------------|------------------------------------------------|
| TextStamper     | Watermarks, labels, page numbers               |
| ImageStamper    | Logos, signatures, approval stamps             |
| HtmlStamper     | Custom layouts, disclaimers, styled elements   |
| BarcodeStamper  | QR codes, barcodes, document tracking          |

Each stamper lets you set alignment, transparency, rotation, and target specific pages. You can combine multiple stampers for complex overlays.

## How Do I Add a Text Watermark to a PDF in C#?

The most common use case is stamping a watermark like "CONFIDENTIAL" diagonally across every page. Here's how to do it with IronPDF:

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;
using System.Drawing;

var doc = PdfDocument.FromFile("report.pdf");

var watermark = new TextStamper
{
    Text = "CONFIDENTIAL",
    FontFamily = "Arial",
    FontSize = 60,
    FontColor = Color.Red,
    Opacity = 30, // 30% opacity for subtlety
    Rotation = -45,
    IsBold = true,
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Middle
};

doc.ApplyStamp(watermark);
doc.SaveAs("report-watermarked.pdf");
```

**Tips:**  
- Adjust `Opacity` for a lighter or heavier watermark.
- Use `Rotation` for diagonal effects.
- For text rotation specifics, see [How can I rotate text in a PDF using C#?](rotate-text-pdf-csharp.md)

## How Can I Stamp Images—Like Logos or Signatures—on PDFs?

To overlay images such as company logos or signature scans, use `ImageStamper`. It's easy to scale and position images as needed:

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;

var pdf = PdfDocument.FromFile("invoice.pdf");

var logoStamp = new ImageStamper("logo.png")
{
    Scale = 24, // Scale down to 24% of original size
    HorizontalAlignment = HorizontalAlignment.Left,
    VerticalAlignment = VerticalAlignment.Top,
    HorizontalOffset = new Length(12), // 12px from the left
    VerticalOffset = new Length(12),   // 12px from the top
    Opacity = 80 // Slightly transparent
};

pdf.ApplyStamp(logoStamp);
pdf.SaveAs("invoice-branded.pdf");
```

**Real-world tips:**
- Use the `Scale` property to prevent large images from covering content.
- Combine multiple images (e.g., logo and signature) by applying several stamps.

For more on extracting images from PDFs, see [How can I convert a PDF to a bitmap image in C#?](pdf-to-image-bitmap-csharp.md).

## How Do I Overlay HTML Content as a PDF Stamp?

When you need rich layouts—like a styled "PAID" box with CSS, links, or branding—`HtmlStamper` is your best friend. You can render nearly any HTML and CSS directly as a stamp.

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;

var doc = PdfDocument.FromFile("receipt.pdf");

var htmlStamp = new HtmlStamper
{
    Html = @"
        <div style='
            background: #f0fff0;
            border: 2px solid #28a745;
            color: #28a745;
            font-family: Arial, sans-serif;
            font-size: 24px;
            font-weight: bold;
            border-radius: 10px;
            padding: 10px 28px;
            box-shadow: 1px 2px 3px rgba(0,0,0,.10);
            text-align: center;'>
            PAID<br><small>Thanks for your business!</small>
        </div>",
    HorizontalAlignment = HorizontalAlignment.Right,
    VerticalAlignment = VerticalAlignment.Top,
    HorizontalOffset = new Length(-38),
    VerticalOffset = new Length(36)
};

doc.ApplyStamp(htmlStamp);
doc.SaveAs("receipt-stamped.pdf");
```

**Why use HTML?**
- Supports dynamic content and custom styling.
- Embed images, hyperlinks, or tables easily.
- Great for legal disclaimers or branded headers.

## Can I Add QR Codes or Barcodes as PDF Stamps?

Absolutely. IronPDF’s `BarcodeStamper` lets you insert QR codes or classic barcodes with full control over position and size.

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;

var doc = PdfDocument.FromFile("shipping-label.pdf");

var qrCode = new BarcodeStamper("https://example.com/track/12345")
{
    BarcodeType = BarcodeType.QRCode,
    Width = 80,
    Height = 80,
    HorizontalAlignment = HorizontalAlignment.Right,
    VerticalAlignment = VerticalAlignment.Bottom,
    HorizontalOffset = new Length(-18),
    VerticalOffset = new Length(-18)
};

doc.ApplyStamp(qrCode);
doc.SaveAs("shipping-label-with-qr.pdf");
```

**Tricks:**
- Use different `BarcodeType` values for various formats (QR, Code128, DataMatrix).
- QR codes can store URLs, serials, or any data.
- For extracting barcode data from PDFs, you'll likely need specialized libraries.

## How Do I Precisely Place Stamps on the PDF Page?

You can combine alignment and offset properties to put stamps exactly where you want them:

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;
using System.Drawing;

var doc = PdfDocument.FromFile("handbook.pdf");

// Add a header
var header = new TextStamper
{
    Text = "Internal Use Only",
    FontColor = Color.OrangeRed,
    FontSize = 14,
    HorizontalAlignment = HorizontalAlignment.Left,
    VerticalAlignment = VerticalAlignment.Top,
    HorizontalOffset = new Length(22),
    VerticalOffset = new Length(14)
};

// Add a footer
var footer = new TextStamper
{
    Text = "© 2024 ExampleCorp",
    FontSize = 10,
    HorizontalAlignment = HorizontalAlignment.Right,
    VerticalAlignment = VerticalAlignment.Bottom,
    HorizontalOffset = new Length(-18),
    VerticalOffset = new Length(-10)
};

doc.ApplyStamp(header);
doc.ApplyStamp(footer);
doc.SaveAs("handbook-stamped.pdf");
```

**Tip:** Positive offsets push the stamp further from the anchor; negative offsets pull it inward.

## Can I Stamp Only Certain Pages Instead of the Whole PDF?

Yes! You can target specific pages by passing a page index or array to `ApplyStamp`. For example:

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;
using System.Linq;

var doc = PdfDocument.FromFile("summary.pdf");

var draftStamp = new TextStamper
{
    Text = "DRAFT",
    FontSize = 60,
    FontColor = System.Drawing.Color.Gray,
    Opacity = 20,
    Rotation = -45,
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Middle
};

// Stamp only the first page (page 0)
doc.ApplyStamp(draftStamp, 0);

// Or, stamp first and last page
doc.ApplyStamp(draftStamp, new[] { 0, doc.PageCount - 1 });

// Or, stamp every odd-numbered page
var oddPages = Enumerable.Range(0, doc.PageCount).Where(i => i % 2 == 1).ToArray();
doc.ApplyStamp(draftStamp, oddPages);

doc.SaveAs("summary-draft.pdf");
```

For advanced per-page logic, you can iterate and apply different stamps dynamically.

## How Can I Combine Multiple Stamps—Logos, Watermarks, QR Codes—on One PDF?

You can layer as many stamps as you like by calling `ApplyStamp` multiple times, optionally with different page targets or stampers.

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;
using System.Drawing;

var doc = PdfDocument.FromFile("agreement.pdf");

var logo = new ImageStamper("logo.png")
{
    Scale = 22,
    HorizontalAlignment = HorizontalAlignment.Left,
    VerticalAlignment = VerticalAlignment.Top,
    HorizontalOffset = new Length(18),
    VerticalOffset = new Length(18)
};

var watermark = new TextStamper
{
    Text = "CONFIDENTIAL",
    FontFamily = "Arial Black",
    FontSize = 75,
    FontColor = Color.FromArgb(90, 0, 0, 0),
    Opacity = 14,
    Rotation = -32,
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Middle
};

var qr = new BarcodeStamper("https://verify.com/contract/789")
{
    BarcodeType = BarcodeType.QRCode,
    Width = 80,
    Height = 80,
    HorizontalAlignment = HorizontalAlignment.Right,
    VerticalAlignment = VerticalAlignment.Bottom,
    HorizontalOffset = new Length(-22),
    VerticalOffset = new Length(-22)
};

doc.ApplyStamp(logo);
doc.ApplyStamp(watermark);
doc.ApplyStamp(qr);
doc.SaveAs("agreement-branded.pdf");
```

For combining signatures and more, see [How do I parse and extract text from PDFs in C#?](parse-pdf-extract-text-csharp.md).

## How Do I Make Watermarks Subtle or Bold Using Opacity?

Control watermark visibility using the `Opacity` property. Values range from `0` (invisible) to `100` (fully solid).

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;

var doc = PdfDocument.FromFile("draft.pdf");

var faint = new TextStamper
{
    Text = "SAMPLE",
    Opacity = 8,
    FontSize = 100,
    Rotation = -40
};

var strong = new TextStamper
{
    Text = "VOID",
    Opacity = 70,
    FontSize = 100,
    FontColor = System.Drawing.Color.Red,
    Rotation = -40
};

doc.ApplyStamp(faint);
doc.ApplyStamp(strong);
doc.SaveAs("draft-watermarked.pdf");
```

**Range reminder:**  
- Under 15: almost invisible  
- 30–50: typical watermark  
- Over 70: bold and prominent

## How Can I Stamp Digital Signatures (Image + Text) Easily?

A reusable method lets you add a signature image and signer info to the last page:

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;
using System;
using System.Drawing;

public void AddSignature(string pdfPath, string imagePath, string signer, DateTime signDate)
{
    var doc = PdfDocument.FromFile(pdfPath);
    int lastPage = doc.PageCount - 1;

    var signatureImg = new ImageStamper(imagePath)
    {
        Scale = 38,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalOffset = new Length(88),
        VerticalOffset = new Length(66)
    };

    var signatureText = new HtmlStamper
    {
        Html = $@"<div style='font-family:Arial;font-size:11px;'>
                    <hr style='width:138px;'/>
                    <strong>{signer}</strong><br/>
                    Date: {signDate:yyyy-MM-dd}
                  </div>",
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalOffset = new Length(88),
        VerticalOffset = new Length(28)
    };

    doc.ApplyStamp(signatureImg, lastPage);
    doc.ApplyStamp(signatureText, lastPage);
    doc.SaveAs(pdfPath.Replace(".pdf", "-signed.pdf"));
}
```

For cryptographic signing, explore the [digital signatures](https://ironpdf.com/nodejs/examples/digitally-sign-a-pdf/) feature in IronPDF.

## How Can I Clearly Mark Copies vs. Originals in PDFs?

Use a clear "COPY" or "ORIGINAL" HTML stamp with color coding:

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;

public void StampCopyOrOriginal(PdfDocument doc, bool isOriginal)
{
    var color = isOriginal ? "green" : "red";
    var label = isOriginal ? "ORIGINAL" : "COPY";

    var stamp = new HtmlStamper
    {
        Html = $@"<div style='border:3px solid {color}; color:{color};
                   padding:6px 18px; font-size:24px; font-family:Arial;
                   font-weight:bold; border-radius:5px; transform:rotate(-13deg);'>
                   {label}
                 </div>",
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalOffset = new Length(-58),
        VerticalOffset = new Length(48)
    };

    doc.ApplyStamp(stamp);
}
```

## How Do I Watermark Entire Folders of PDFs Automatically?

Batch processing is easy—loop through files in a directory and apply a watermark to each:

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;
using System.IO;

public void BatchWatermark(string inputDir, string outputDir, string watermark)
{
    Directory.CreateDirectory(outputDir);

    var stamp = new TextStamper
    {
        Text = watermark,
        FontSize = 62,
        Opacity = 20,
        Rotation = -38,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Middle
    };

    foreach (var file in Directory.GetFiles(inputDir, "*.pdf"))
    {
        using var doc = PdfDocument.FromFile(file);
        doc.ApplyStamp(stamp);
        var outPath = Path.Combine(outputDir, Path.GetFileName(file));
        doc.SaveAs(outPath);
        Console.WriteLine($"Stamped: {file} -> {outPath}");
    }
}
```

**Tip:** Integrate this into an automated workflow or CI/CD pipeline for efficiency.

## Can I Add Dynamic Stamps for Each Page (Like Page Numbers or User Info)?

Definitely. With `HtmlStamper`, you can use placeholders like `{page}` and `{total-pages}` for page-specific data, and string interpolation for user info:

```csharp
// Install-Package IronPdf
using IronPdf;
using IronPdf.Stamps;

var doc = PdfDocument.FromFile("guide.pdf");
var user = "Jane Developer";

var footer = new HtmlStamper
{
    Html = $@"<div style='font-size:10px; text-align:center; color:#555;'>
                 Page {{page}} of {{total-pages}} &mdash; Prepared for: {user}
              </div>",
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Bottom,
    VerticalOffset = new Length(-8)
};

doc.ApplyStamp(footer);
doc.SaveAs("guide-personalized.pdf");
```

For more dynamic PDF manipulation, check out [How can I parse and extract text from PDFs in C#?](parse-pdf-extract-text-csharp.md).

## What Properties Can I Use to Customize PDF Stamps in IronPDF?

Here’s a quick cheat sheet:

| Stamper Type    | Key Properties                                  |
|-----------------|-------------------------------------------------|
| TextStamper     | `Text`, `FontFamily`, `FontSize`, `FontColor`, `IsBold`, `IsItalic`, `Opacity`, `Rotation` |
| ImageStamper    | Image path, `Scale`, `Opacity`                  |
| HtmlStamper     | `Html`, `Opacity`, `Rotation`                   |
| BarcodeStamper  | Value, `BarcodeType`, `Width`, `Height`, `Opacity` |

Other key props:  
- `HorizontalAlignment` (Left, Center, Right)
- `VerticalAlignment` (Top, Middle, Bottom)
- `HorizontalOffset` and `VerticalOffset` for fine-tuning position
- `Opacity` (0–100)
- `Rotation` (degrees; negative for counter-clockwise)

Explore the official [IronPDF documentation](https://ironpdf.com) for more.

## What Are Common Issues When Stamping PDFs and How Do I Fix Them?

Here are some typical pitfalls:

- **Fonts not displaying correctly:** Ensure fonts are installed on your server or embed them using CSS with `@font-face` for HTML stamps.
- **Images misaligned or blurry:** Adjust the `Scale` property and double-check offsets.
- **Stamps missing from pages:** Confirm page indices (zero-based) and your logic when targeting specific pages.
- **Unreadable barcodes:** Increase `Width` and `Height`, and avoid overlaying on busy backgrounds (reduce opacity).
- **Watermark visibility issues:** Tweak `Opacity` for desired prominence.
- **Large PDFs after stamping:** Compress images and keep HTML/CSS simple to prevent file bloat.
- **Save errors or exceptions:** Dispose streams properly and verify output directory permissions.

For more on permissions, see [How do I secure PDFs with permissions and passwords in C#?](pdf-permissions-passwords-csharp.md).

## Where Can I Learn More About IronPDF or Get Help?

For more in-depth coverage of IronPDF’s capabilities, see the official [IronPDF website](https://ironpdf.com) and [Iron Software](https://ironsoftware.com). You may also enjoy reading about the company’s journey in [How did IronPDF grow from startup to 50 engineers?](ironpdf-journey-startup-to-50-engineers.md).

---

*[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), CTO at [Iron Software](https://ironsoftware.com). Building tools that make developers' lives easier since 2017.*
---

## Related Resources

### 📚 Tutorials & Guides
- **[HTML to PDF Guide](../html-to-pdf-csharp.md)** — Complete conversion tutorial
- **[Best PDF Libraries 2025](../best-pdf-libraries-dotnet-2025.md)** — Library comparison
- **[Beginner Tutorial](../csharp-pdf-tutorial-beginners.md)** — First PDF in 5 minutes
- **[Decision Flowchart](../choosing-a-pdf-library.md)** — Find the right library

### 🔧 PDF Operations
- **[Merge & Split PDFs](../merge-split-pdf-csharp.md)** — Combine documents
- **[Digital Signatures](../digital-signatures-pdf-csharp.md)** — Sign legally
- **[Extract Text](../extract-text-from-pdf-csharp.md)** — Text extraction
- **[Fill PDF Forms](../fill-pdf-forms-csharp.md)** — Form automation

### 🚀 Framework Integration
- **[ASP.NET Core PDF](../asp-net-core-pdf-reports.md)** — Web app integration
- **[Blazor PDF Generation](../blazor-pdf-generation.md)** — Blazor support
- **[Cross-Platform Deployment](../cross-platform-pdf-dotnet.md)** — Docker, Linux, Cloud

### 📖 Library Documentation
- **[IronPDF](../ironpdf/)** — Full Chromium rendering
- **[QuestPDF](../questpdf/)** — Code-first generation
- **[PDFSharp](../pdfsharp/)** — Open source option

---

*Part of the [Awesome .NET PDF Libraries 2025](../README.md) collection — 73 C#/.NET PDF libraries compared with 167 FAQ articles.*


---

Written by [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), Chief Technology Officer at Iron Software. Jacob started coding at age 6 and created IronPDF to solve real PDF challenges. Based in Chiang Mai, Thailand. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
