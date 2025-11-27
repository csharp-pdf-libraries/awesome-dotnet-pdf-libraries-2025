# How Can I Draw Text, Images, and Graphics on PDFs in C#?

Absolutely! If you need to dynamically add stamps, watermarks, logos, or custom graphics to PDFs from your C# applications, you’re in the right place. This FAQ covers everything from stamping “PAID” overlays to dropping in barcodes, and even drawing lines or shapes with .NET—using the IronPdf library. Let’s walk through the practical steps, common gotchas, and performance tips for editing PDFs programmatically.

---

## Why Would I Want to Draw on PDFs Using C#?

Automating PDF editing saves time and reduces manual errors, especially when you need to process documents at scale. By drawing directly on PDFs in C#, you can:

- Automatically stamp invoices with “PAID,” “DRAFT,” or custom labels.
- Overlay your company’s logo or watermark all pages of a report or contract.
- Add dynamic annotations like approval timestamps, user notes, or digital signatures.
- Apply branding and security marks to sensitive documents.

If you’re interested in more advanced drawing (like lines or rectangles), see [How do I draw lines or rectangles on a PDF in C#?](draw-line-rectangle-pdf-csharp.md).

---

## How Do I Set Up My C# Project to Edit PDFs?

To get started, you’ll need the IronPdf library—which is developer-focused, easy to set up, and handles PDF quirks for you.

**Install via NuGet:**
```powershell
Install-Package IronPdf
```

**Essential namespaces:**
```csharp
using IronPdf;                // PDF manipulation
using System.Drawing;         // Images, colors, and graphics
using System.IO;              // File and stream handling
using System.Net.Http;        // Fetching images from the web
using System.Threading.Tasks; // For async and batch operations
```

You’ll find [IronPDF](https://ironpdf.com) well-supported and regularly updated by [Iron Software](https://ironsoftware.com).

---

## How Can I Add Text to a PDF, Like Stamps or Watermarks?

Drawing text is straightforward, whether you need a bold “PAID” stamp or subtle annotation.

**Basic example:**
```csharp
using IronPdf;

// Open your PDF
var doc = PdfDocument.FromFile("invoice.pdf");

// Place "PAID" on the first page at coordinates (300, 400)
doc.DrawText("PAID", 300, 400, 0);

doc.SaveAs("invoice-with-stamp.pdf");
```
**PDF coordinate system:**  
- X=0 is the left edge, increases to the right.
- Y=0 is at the bottom, increases upwards.
- Pages use zero-based indices.

**Tip:** For multi-size PDFs, get dimensions dynamically:
```csharp
var dimensions = doc.Pages[0].Size;
int pageWidth = (int)dimensions.Width;
int pageHeight = (int)dimensions.Height;
```

For details on drawing rectangles and lines, see [How do I draw lines and rectangles on a PDF in C#?](draw-lines-rectangles-pdf-csharp.md)

---

### How Do I Control Font, Color, and Text Rotation?

You have full control over text styling—font family, size, color, and even rotation.

**Styled text example:**
```csharp
using IronPdf;
using System.Drawing; // For Color

doc.DrawText(
    "CONFIDENTIAL",
    250, 420, 0,
    font: StandardFonts.HelveticaBold,
    fontSize: 36,
    color: Color.Red,
    rotation: 45
);
```
Available fonts include Helvetica, Courier, TimesRoman, and others (see `StandardFonts`). Use `Color.FromArgb(alpha, r, g, b)` for transparency.

**Multiline text:** Just use `\n`:
```csharp
doc.DrawText("APPROVED\n2024-06-15", 100, 650, 0, fontSize: 18);
```

---

### How Do I Draw Text on Every Page or Add Page Numbers?

Loop through each page to add a watermark, stamp, or page number.

**Watermark every page:**
```csharp
for (int page = 0; page < doc.PageCount; page++)
{
    doc.DrawText(
        "DRAFT",
        200, 400, page,
        font: StandardFonts.HelveticaBold,
        fontSize: 72,
        color: Color.FromArgb(80, 255, 0, 0), // Semi-transparent red
        rotation: 45
    );
}
```

**Add page numbers:**
```csharp
for (int page = 0; page < doc.PageCount; page++)
{
    doc.DrawText(
        $"Page {page + 1} of {doc.PageCount}",
        50, 40, page,
        font: StandardFonts.Courier,
        fontSize: 10
    );
}
```

Need to extract text from PDFs instead? Check [How do I extract text from a PDF in C#?](extract-text-from-pdf-csharp.md).

---

### Can I Add Annotations, Signatures, or Timestamps to PDFs?

Yes, you can dynamically add user names, dates, or electronic signatures for approval trails.

**Example:**
```csharp
string user = Environment.UserName;
string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

doc.DrawText(
    $"Signed by: {user}\n{dateTime}",
    400, 50, 0,
    font: StandardFonts.Courier,
    fontSize: 10,
    color: Color.Gray
);
```

For complex annotation workflows, combining text and graphics can be very effective.

---

## How Do I Add Images, Logos, or Watermarks to a PDF?

Whether it’s a logo, a signature, or a barcode image, here’s how to overlay graphics:

**Overlay a logo:**
```csharp
using IronPdf;
using System.Drawing;

var doc = PdfDocument.FromFile("report.pdf");
using var logo = new Bitmap("company-logo.png");

// Position logo at top-right (adjust X, Y as needed)
doc.DrawBitmap(logo, 450, 700, 0);

doc.SaveAs("report-branded.pdf");
```
PNGs preserve transparency for crisp overlays.

---

### How Can I Resize Images or Add Transparency?

Control the size of inserted images and their transparency easily.

**Resize an image:**
```csharp
// Place a 120x60pt logo at (450, 700)
doc.DrawBitmap(logo, 450, 700, 0, width: 120, height: 60);
```
**Transparent PNGs:**
```csharp
using var watermark = new Bitmap("transparent-watermark.png");
doc.DrawBitmap(watermark, 180, 350, 0, width: 300, height: 150);
```
Alpha channels in PNG/TIFF are supported, but JPEGs lack transparency.

---

### Can I Add Images from URLs or Base64 Strings?

Absolutely—simply download or decode the image and draw it.

**From a URL:**
```csharp
using var client = new HttpClient();
var imageBytes = await client.GetByteArrayAsync("https://mycdn.com/logo.png");

using var ms = new MemoryStream(imageBytes);
using var bitmap = new Bitmap(ms);

doc.DrawBitmap(bitmap, 100, 600, 0);
```

**From Base64:**
```csharp
var imageData = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAA..."); // Truncated
using var ms = new MemoryStream(imageData);
using var bitmap = new Bitmap(ms);

doc.DrawBitmap(bitmap, 300, 600, 0);
```

---

### How Should I Handle Large Images for Performance?

Large images can make PDFs heavy. Resize before drawing:

```csharp
using var original = new Bitmap("big-photo.jpg");
int targetWidth = 400, targetHeight = 300;

using var resized = new Bitmap(original, new Size(targetWidth, targetHeight));
doc.DrawBitmap(resized, 100, 400, 0);
```
For higher-quality scaling, you can use `Graphics.DrawImage` as well.

---

## How Can I Draw Custom Graphics, Lines, or Shapes on a PDF?

IronPdf doesn’t directly support drawing lines or shapes on the PDF canvas, but you can easily create graphics as images in memory and overlay them.

**Example: Draw a custom “VERIFIED” stamp:**
```csharp
using var canvas = new Bitmap(200, 100);
using var g = Graphics.FromImage(canvas);

g.Clear(Color.Transparent);
g.DrawEllipse(Pens.Blue, 10, 10, 180, 80);
g.DrawString("VERIFIED", new Font("Arial", 24, FontStyle.Bold), Brushes.Green, 30, 35);

// Overlay on your PDF
doc.DrawBitmap(canvas, 220, 500, 0);
```
For drawing lines and rectangles natively, check out [How do I draw lines or rectangles on a PDF in C#?](draw-line-rectangle-pdf-csharp.md) and [How do I draw multiple lines and rectangles in a PDF using C#?](draw-lines-rectangles-pdf-csharp.md).

---

## How Do I Add Barcodes or QR Codes to PDFs?

Barcodes and QR codes aren’t built into IronPdf, but you can generate them as images (using a library like IronBarcode) and place them onto your PDFs.

**Barcode example:**
```csharp
using IronBarCode; // Install-Package IronBarCode

var barcode = BarcodeWriter.CreateBarcode("12345", BarcodeEncoding.Code128);
using var barcodeImage = barcode.ToBitmap();

doc.DrawBitmap(barcodeImage, 50, 100, 0, width: 200, height: 60);
```
**QR code example:**
```csharp
var qr = BarcodeWriter.CreateBarcode("https://ironpdf.com", BarcodeEncoding.QRCode);
using var qrImage = qr.ToBitmap();

doc.DrawBitmap(qrImage, 400, 600, 0, width: 100, height: 100);
```

---

## Can I Edit Password-Protected PDFs in C#?

Yes! Just provide the password when loading the PDF.

```csharp
using IronPdf;

var doc = PdfDocument.FromFile("protected.pdf", "yourpassword");
doc.DrawText("APPROVED", 300, 500, 0, font: StandardFonts.HelveticaBold, fontSize: 36);
doc.SaveAs("protected-stamped.pdf");
```

---

## What Are Some Performance Tips for Batch PDF Processing?

Batch stamping or watermarking dozens (or hundreds) of PDFs? Here’s how to keep things efficient:

- **Text drawing:** 10–30ms per operation.
- **Images:** 50–150ms per operation (depends on image size).
- **Batch jobs:** Use parallelism if possible.

**Batch process every PDF in a folder:**
```csharp
using System.IO;
using System.Threading.Tasks;

var files = Directory.GetFiles("invoices", "*.pdf");

Parallel.ForEach(files, file =>
{
    var doc = PdfDocument.FromFile(file);
    doc.DrawText("DRAFT", 300, 400, 0,
        font: StandardFonts.HelveticaBold,
        fontSize: 48,
        color: Color.FromArgb(120, 255, 0, 0),
        rotation: 45);
    doc.SaveAs($"stamped/{Path.GetFileName(file)}");
});
```
Don’t over-parallelize if your system is limited on RAM or CPU.

---

## What Are Common Mistakes or Troubleshooting Tips?

Here are a few things to watch out for:

- **Coordinate confusion:** PDF origin is bottom-left, not top-left like Windows Forms.
- **Blurry images:** Upscaling small images makes them fuzzy; use higher-res sources and scale down.
- **Lost transparency:** Only PNG/TIFF images preserve alpha channels.
- **Fonts not rendering:** Stick to built-in `StandardFonts` for broad compatibility.
- **File save errors:** Check for file locks or open instances.
- **Password issues:** Double-check your entered password and ensure the PDF isn’t corrupted.
- **Resource leaks:** Use `using` blocks to ensure images and PDFs are disposed.
- **Odd page sizes:** Use `doc.Pages[index].Size` to get dimensions for accurate placement.

For more on viewing PDFs directly in .NET MAUI apps, check [How do I view PDFs in MAUI using C# and .NET?](pdf-viewer-maui-csharp-dotnet.md).

---

## Where Can I Find More Info or Advanced PDF Libraries for C#?

For deeper scenarios—like extracting text, advanced layout, or custom PDF viewers—explore these resources:

- [Best C# PDF libraries for 2025](best-csharp-pdf-libraries-2025.md)
- [Extracting text from PDFs](extract-text-from-pdf-csharp.md)
- [Drawing lines and rectangles in C# PDFs](draw-line-rectangle-pdf-csharp.md)

You’ll also find robust docs and community support at [IronPDF](https://ironpdf.com) and [Iron Software](https://ironsoftware.com).

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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/) is CTO at Iron Software, leading a team of 50+ engineers building .NET libraries downloaded over 41+ million times. With 41 years of coding experience, Jacob is passionate about developer experience and API design. Based in Chiang Mai, Thailand.
