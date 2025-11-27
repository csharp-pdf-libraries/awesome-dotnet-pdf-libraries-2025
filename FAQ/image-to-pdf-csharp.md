# How Do I Convert Images to PDF in C#? (JPG, PNG, TIFF, SVG & More)

Turning images into PDF files is a common need for C# developers, whether you’re managing scanned receipts, building photo albums, or archiving technical diagrams. Historically, this has been a source of frustration—with older libraries demanding manual sizing, tricky resource management, and pages of boilerplate. Enter IronPDF: a modern, developer-friendly solution that makes image to PDF conversion simple, efficient, and powerful.

This FAQ will walk you through everything from quick one-liners for basic conversions to advanced workflows: batch processing, custom layouts, handling multi-page TIFFs, adding searchable metadata, and even working with massive image files. Plus, you’ll find troubleshooting tips, code samples, and links to [related image/PDF tasks](add-images-to-pdf-csharp.md) you might need in your .NET projects.

---

## Why Would I Want to Convert Images to PDF in C#?

Image-to-PDF workflows are everywhere in modern applications—think expense reporting (scanned receipts), document archiving, creating technical manuals, or generating printable reports from screenshots and diagrams. But, as many developers have discovered, most C# PDF libraries (like iTextSharp) make the process much harder than it should be, often requiring manual calculation of page sizes, tricky stream management, and lots of cleanup.

IronPDF flips this on its head with an API that lets you convert images to PDFs in a single, clean line. No more aspect ratio headaches or resource juggling—just quick, reliable results.

---

## What Do I Need to Get Started With Image-to-PDF Conversion in C#?

First things first: you’ll need the IronPDF library. It’s available via NuGet, so installation is straightforward:

```powershell
Install-Package IronPdf
```
Or with the .NET CLI:
```bash
dotnet add package IronPdf
```

IronPDF supports virtually all common image formats—JPEG, PNG, BMP, GIF, SVG, TIFF, and WEBP—without the need for extra dependencies. For more details on IronPDF’s rendering engine, check out the deep dive on [ChromePdfRenderer](https://ironpdf.com/blog/videos/how-to-render-html-string-to-pdf-in-csharp-ironpdf/).

---

## How Do I Convert a Single Image to a PDF in C#?

If you’re looking for the simplest case—turning one image (like a scanned contract) into a PDF—the process couldn’t be easier:

```csharp
using IronPdf;
// Install-Package IronPdf

var pdf = ImageToPdfConverter.ImageToPdf("invoice.jpg");
pdf.SaveAs("invoice.pdf");
```

**Here’s what happens under the hood:**
- The PDF page is sized to match your image—no cropping, no stretching, just a pixel-perfect fit.
- The aspect ratio is maintained automatically.
- No margins or manual adjustments needed.

This approach works with any supported image format and is perfect for quick, no-fuss conversions.

---

## How Can I Combine Multiple Images Into a Single Multi-Page PDF?

If you’ve got a stack of images (for example, scanned pages of a contract) and want them all in one PDF, IronPDF makes batching just as easy:

```csharp
using IronPdf;
// Install-Package IronPdf

var imageFiles = new[] { "page1.png", "page2.png", "page3.png" };
var pdf = ImageToPdfConverter.ImageToPdf(imageFiles);
pdf.SaveAs("all-pages.pdf");
```

Each image becomes a new page in the PDF, ordered as you specify them in the array.

### Can I Batch Convert All Images in a Folder?

Absolutely! If you have a directory filled with images from a scanner, you can programmatically gather them and convert everything in one go:

```csharp
using System.IO;
using IronPdf;
// Install-Package IronPdf

var images = Directory.GetFiles("scans/", "*.jpg");
var pdf = ImageToPdfConverter.ImageToPdf(images);
pdf.SaveAs("batch-scan.pdf");
```

This approach is super helpful for automating bulk document processing.

### What If My Folder Has Mixed Image Formats (JPG, PNG, TIFF)?

No problem. IronPDF can handle arrays of images with mixed file types seamlessly. Just filter for supported extensions:

```csharp
using System.IO;
using IronPdf;
// Install-Package IronPdf

var validExts = new[] { ".jpg", ".jpeg", ".png", ".tiff" };
var files = Directory
    .EnumerateFiles("mixed/", "*.*")
    .Where(f => validExts.Contains(Path.GetExtension(f).ToLower()))
    .ToArray();

var pdf = ImageToPdfConverter.ImageToPdf(files);
pdf.SaveAs("mixed-images.pdf");
```

For adding images to existing PDFs instead of creating new ones, see [How do I add images to PDFs in C#?](add-images-to-pdf-csharp.md)

---

## Which Image Formats Can I Convert to PDF Using IronPDF?

IronPDF supports nearly every relevant image format for PDF conversion:
- **JPEG/JPG**: Perfect for photos and scanned documents.
- **PNG**: Great for screenshots and diagrams, including transparency.
- **BMP**: For legacy bitmap images.
- **GIF**: Only the first frame is used—ideal for static documents.
- **SVG**: Scalable vector graphics render at print quality.
- **TIFF**: Supports multi-page TIFFs (details below).
- **WEBP**: Efficient for modern, compressed images.

No need to worry about installing separate image codecs—IronPDF handles it all by default.

### How Do I Convert an SVG Image to PDF and Preserve Vector Quality?

SVG images remain crisp at any size when converted with IronPDF. Here’s how:

```csharp
using IronPdf;
// Install-Package IronPdf

var pdf = ImageToPdfConverter.ImageToPdf("diagram.svg");
pdf.SaveAs("diagram.pdf");
```

SVG is ideal for technical drawings or company logos you might need to print or embed.

### Can IronPDF Handle Multi-Page TIFF Scans?

Yes! If your scanner creates a multi-page TIFF file (essentially a single file containing multiple images), IronPDF splits each frame into a separate PDF page automatically:

```csharp
using IronPdf;
// Install-Package IronPdf

var pdf = ImageToPdfConverter.ImageToPdf("multipage-scan.tiff");
pdf.SaveAs("scan-document.pdf");
```

No manual frame extraction needed—just point and convert.

For the reverse (PDF to image/bitmap), see [How can I convert PDFs to images/bitmaps in C#?](pdf-to-image-bitmap-csharp.md)

---

## How Does IronPDF Position Images and Set Page Sizes By Default?

IronPDF’s default logic is developer-friendly:
- Each PDF page matches the size of its source image.
- The image is centered and scaled to fit, without distortion.
- No cropping or unwanted margins.

This avoids the classic “image stretched or squashed” problem. If you want more control—like adding margins, backgrounds, or combining multiple images on one page—you can use IronPDF’s robust HTML-to-PDF rendering.

---

## How Do I Create Custom PDF Layouts With Multiple Images, Captions, or CSS Styling?

If you need more than a simple one-image-per-page layout—say, a catalog, album, or annotated report—IronPDF lets you use HTML and CSS for total control. This leverages the [ChromePdfRenderer](https://ironpdf.com/blog/videos/how-to-render-html-string-to-pdf-in-csharp-ironpdf/) under the hood.

Here’s an example with a styled image, caption, and date:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();

string html = $@"
<html>
  <body style='margin: 0; background: #f5f5f5;'>
    <div style='padding: 24px; border: 1px solid #bbb; background: #fff; text-align: center;'>
      <img src='blueprint.svg' style='width: 80%; max-width: 600px; height: auto; object-fit: contain;' />
      <h2>Blueprint Overview</h2>
      <p style='color: #888;'>Generated: {DateTime.Now:yyyy-MM-dd}</p>
    </div>
  </body>
</html>
";

renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginBottom = 20;

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("blueprint-report.pdf");
```

You can use pure HTML/CSS to:
- Set custom backgrounds or branding
- Add captions, headers, or footers
- Place multiple images per page (think photo grid, catalog, or comparison chart)

For more advanced HTML-to-PDF techniques (like advanced CSS, page breaks, or scripting), see [How do I use advanced HTML-to-PDF features in C#?](advanced-html-to-pdf-csharp.md)

### How Can I Arrange Multiple Images on a Single PDF Page?

To build a grid, just use HTML tables or flex layouts:

```csharp
using IronPdf;
// Install-Package IronPdf

string html = @"
<html>
  <body>
    <table>
      <tr>
        <td><img src='imgA.jpg' width='200'></td>
        <td><img src='imgB.jpg' width='200'></td>
      </tr>
      <tr>
        <td><img src='imgC.jpg' width='200'></td>
        <td><img src='imgD.jpg' width='200'></td>
      </tr>
    </table>
  </body>
</html>
";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("photo-grid.pdf");
```

This is perfect for photo albums or any scenario where you want multiple images on each page.

---

## How Do I Add Metadata (Title, Author, Keywords) to My PDFs?

Metadata helps organize large archives and makes PDFs easier to search. With IronPDF, you can set PDF properties right after conversion:

```csharp
using IronPdf;
// Install-Package IronPdf

var pdf = ImageToPdfConverter.ImageToPdf("report-scan.png");
pdf.MetaData.Title = "Q1 Financial Report";
pdf.MetaData.Author = "Accounting Team";
pdf.MetaData.Keywords = "finance, Q1, report";
pdf.MetaData.CreationDate = DateTime.UtcNow;
pdf.SaveAs("Q1-report.pdf");
```

PDF viewers (like Adobe Reader) will display this metadata, and document management systems can use it to index your files.

### Can I Set Metadata in Batch Processing?

Yes! Here’s how to dynamically assign metadata based on filenames during batch conversion:

```csharp
using System.IO;
using IronPdf;
// Install-Package IronPdf

var files = Directory.GetFiles("invoices/");
foreach (var file in files)
{
    var pdf = ImageToPdfConverter.ImageToPdf(file);
    var name = Path.GetFileNameWithoutExtension(file);

    pdf.MetaData.Title = name;
    pdf.MetaData.Keywords = "invoice, scanned";
    pdf.SaveAs($"processed/{name}.pdf");
}
```

For even more control over PDF content (like merging PDFs), see [How do I add images to PDFs in C#?](add-images-to-pdf-csharp.md)

---

## How Do I Handle Large Images or Massive Batches Without Running Out of Memory?

High-res images (like 12,000x12,000 pixel scans) can quickly blow up your memory usage, especially when batch processing hundreds of files. The best practice is to process images one by one and avoid holding too much in memory at once.

```csharp
using System.IO;
using IronPdf;
// Install-Package IronPdf

var images = Directory.GetFiles("large-scans/");
foreach (var img in images)
{
    var pdf = ImageToPdfConverter.ImageToPdf(img);
    pdf.SaveAs($"output/{Path.GetFileNameWithoutExtension(img)}.pdf");
    // Move to the next image after saving
}
```

### Should I Downscale Huge Images Before Converting to PDF?

If your images are overkill for print (e.g., 100+ megapixels), consider resizing them first to save disk space and speed up PDF creation. Here’s a sample approach:

```csharp
using System.Drawing;
using IronPdf;
// Install-Package IronPdf

string source = "satellite.tif";
string downscaled = "satellite-small.jpg";

using (var original = Image.FromFile(source))
{
    int limit = 4000;
    double scale = Math.Min((double)limit / original.Width, (double)limit / original.Height);
    int w = (int)(original.Width * scale);
    int h = (int)(original.Height * scale);

    using (var resized = new Bitmap(original, new Size(w, h)))
    {
        resized.Save(downscaled, System.Drawing.Imaging.ImageFormat.Jpeg);
    }
}

var pdf = ImageToPdfConverter.ImageToPdf(downscaled);
pdf.SaveAs("satellite.pdf");
```

---

## How Do I Make My Image-Based PDFs Searchable With OCR?

By default, image-to-PDF conversion creates image-only PDFs—these aren’t searchable. If you need to select, search, or index text from scans, you’ll want to add OCR (Optical Character Recognition).

### How Can I Add OCR to Create Searchable PDFs?

IronOCR, a separate package from Iron Software, integrates well with IronPDF. Here’s how to use it:

```csharp
// Install-Package IronOcr

using IronOcr;

var ocr = new IronTesseract();
using (var input = new OcrInput())
{
    input.AddImage("receipt.jpg");
    var result = ocr.Read(input);
    result.SaveAsSearchablePdf("searchable-receipt.pdf");
}
```

Now your output PDF will include selectable, searchable text—a must for legal, finance, and research archives.

---

## How Do I Add Watermarks, Headers, or Page Numbers to My Image-to-PDF Output?

With IronPDF’s HTML rendering, you can add overlays like company logos, watermarks, or even dynamic [page numbers](https://ironpdf.com/how-to/html-to-pdf-page-breaks/) using CSS.

### What’s the Best Way to Watermark Every Page in a PDF?

Here’s an example using HTML/CSS to overlay a watermark:

```csharp
using IronPdf;
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();

string html = @"
<html>
  <body style='margin:0;'>
    <div style='position:relative;'>
      <img src='report.jpg' style='width:100%; display:block;'/>
      <div style='position:absolute; top:40%; left:30%; font-size:48px; color:rgba(200,0,0,0.3); transform:rotate(-25deg); pointer-events:none;'>
        CONFIDENTIAL
      </div>
    </div>
  </body>
</html>
";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("confidential-report.pdf");
```

You can layer backgrounds, overlays, headers, and footers as needed—just use standard HTML/CSS. For more on watermarking, see [Watermark examples](https://ironpdf.com/how-to/html-string-to-pdf/).

---

## What Are Common Pitfalls When Converting Images to PDF, and How Do I Solve Them?

Here’s a quick troubleshooting guide for typical issues:

- **High memory usage:** Don’t load all images into memory at once—process one at a time, and downscale huge images.
- **PDFs not searchable:** By default, image-only PDFs can’t be searched. Use [IronOCR](https://ironsoftware.com/csharp/ocr/) for searchable results.
- **Stretched or distorted images:** IronPDF preserves aspect ratio by default. If using HTML, make sure to set `object-fit: contain;` on `<img>` tags.
- **Transparency or background issues:** PNG transparency may default to black; use HTML rendering to set a background color via CSS.
- **Multi-page TIFF only shows first page:** Update to the latest IronPDF—multi-page TIFFs are supported out of the box.
- **Access denied/file lock errors:** Ensure files aren’t open elsewhere, and close streams properly (IronPDF handles this unless you’ve opened files manually).

If you run into more obscure problems, check out the [IronPDF documentation](https://ironpdf.com/docs/) or consult their support forums.

---

## Can IronPDF Integrate With Advanced PDF Features or Other Libraries?

Absolutely—IronPDF is highly flexible. For example, you can:
- Combine image-to-PDF with [custom logging](ironpdf-custom-logging-csharp.md)
- Migrate from Puppeteer or Playwright workflows (see [how to migrate headless browser code](migrate-puppeteer-playwright-to-ironpdf.md))
- Mix image and HTML content for complex documents (see [advanced HTML to PDF scenarios](advanced-html-to-pdf-csharp.md))
- Convert PDFs back to images or bitmaps as needed ([see guide](pdf-to-image-bitmap-csharp.md))

IronPDF is built to play nicely with other tools in the Iron Software ecosystem and beyond.

---

## Where Can I Learn More or Get Support?

For more examples, recipes, and deep-dive documentation, visit the [IronPDF website](https://ironpdf.com). If you’re interested in other powerful developer tools, explore the full range at [Iron Software](https://ironsoftware.com).

If you’re tackling unusual scenarios or have tips to share, the developer community is always evolving—don’t hesitate to reach out!

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

**Jacob Mellor** — Creator of IronPDF. C# enthusiast, Visual Studio lover, and usability-driven API designer. Connect on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
