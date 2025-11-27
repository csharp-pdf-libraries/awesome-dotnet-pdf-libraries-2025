# How Do I Add and Manage Images in PDFs Using C# and IronPDF?

Adding images to PDFs in C# is a must for most real-world apps—think logos, watermarks, scanned docs, and product galleries. With [IronPDF](https://ironpdf.com), you can bring your HTML/CSS skills straight into .NET PDF generation, making image embedding, stamping, and layout control much simpler than with older libraries. This FAQ covers practical ways to embed, position, and manipulate images in your PDFs using IronPDF.

## Why Should I Embed Images in My PDFs?

Images are essential for branding, signatures, data visualization, and readable layouts. Making sure your images are crisp, correctly embedded (so they never break), and positioned exactly right is crucial for professional results. IronPDF makes this easy by letting you use familiar HTML and CSS for layout, and supports all major image formats.

## What’s the Easiest Way to Add Images to a PDF in C#?

The quickest way is to use HTML `<img>` tags inside your document. IronPDF renders your HTML to PDF, automatically downloading and embedding images.

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
string html = @"
  <html>
    <body>
      <img src='https://example.com/logo.png' style='width:150px;' />
      <h1>Welcome!</h1>
    </body>
  </html>";
renderer.RenderHtmlAsPdf(html).SaveAs("output.pdf");
```
This approach works with external URLs, local files, or even inline base64 images.

*For more on advanced PDF creation, see [Create PDF C# Complete Guide](create-pdf-csharp-complete-guide.md).*

## How Do I Embed Base64 Images for Fully Offline PDFs?

If you want to guarantee your PDF works offline or want all assets self-contained, embed images as base64 data URIs.

```csharp
using System;
using System.IO;
using IronPdf;
// Install-Package IronPdf

string ToBase64(string path) =>
    $"data:image/{Path.GetExtension(path).Trim('.')};base64,{Convert.ToBase64String(File.ReadAllBytes(path))}";

string html = $@"
  <img src='{ToBase64("logo.png")}' style='width:120px;' />
  <p>Offline-ready PDF!</p>";
var renderer = new ChromePdfRenderer();
renderer.RenderHtmlAsPdf(html).SaveAs("self-contained.pdf");
```
Base64 is ideal for logos, icons, and signatures. For large images, be mindful of file size.

## How Can I Stamp Images onto Existing PDFs (e.g., Signatures or Watermarks)?

IronPDF’s `ImageStamper` lets you overlay images onto existing PDFs—perfect for adding approval stamps or signatures.

```csharp
using IronPdf;
using IronPdf.Stamps;
// Install-Package IronPdf

var pdf = PdfDocument.FromFile("original.pdf");
var signature = new ImageStamper("signature.png")
{
    HorizontalAlignment = HorizontalAlignment.Right,
    VerticalAlignment = VerticalAlignment.Bottom,
    Opacity = 80
};
pdf.ApplyStamp(signature);
pdf.SaveAs("signed.pdf");
```
You can apply stamps to specific pages or the entire document. For more on managing PDF pages, check [Add Copy Delete Pdf Pages C#](add-copy-delete-pdf-pages-csharp.md).

## How Do I Precisely Place and Size Images in My PDF?

For HTML-based PDFs, control image layout using CSS—absolute positioning, margins, flexbox, etc. For stamped images, use the stamper’s alignment and offset options.

```csharp
// CSS positioning in HTML
string html = @"
  <div style='position:absolute; top:30px; right:40px;'>
    <img src='logo.png' style='width:120px;' />
  </div>";
```
Or for stamping:
```csharp
var stamp = new ImageStamper("logo.png")
{
    HorizontalAlignment = HorizontalAlignment.Left,
    VerticalAlignment = VerticalAlignment.Top,
    HorizontalOffset = new Length(36, MeasurementUnit.Points),
    VerticalOffset = new Length(36, MeasurementUnit.Points)
};
pdf.ApplyStamp(stamp);
```
You get pixel-level control for both methods.

## Which Image Formats Does IronPDF Support?

IronPDF handles PNG, JPEG, GIF (static), SVG, WebP, and BMP. For logos and icons, SVG gives the sharpest results at any zoom. JPEG is best for photos and scans.

```csharp
using IronPdf;
var renderer = new ChromePdfRenderer();
string html = @"
<div>
  <img src='logo.svg' style='height:40px;' />
  <img src='photo.jpg' style='height:40px;' />
</div>";
renderer.RenderHtmlAsPdf(html).SaveAs("images.pdf");
```
For extracting images, see [Pdf To Images Csharp](pdf-to-images-csharp.md).

## How Do I Use Local Image Files in My PDF?

If you reference images with relative paths in your HTML, set the renderer’s `BaseUrl` so IronPDF can resolve them:

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.BaseUrl = new Uri("C:/MyApp/Assets/");
string html = @"<img src='images/logo.png' />";
renderer.RenderHtmlAsPdf(html).SaveAs("local-images.pdf");
```
Alternatively, use `file:///` URIs, but setting `BaseUrl` is more maintainable.

## Can I Add Images to PDF Headers and Footers?

Absolutely! You can embed images in headers and footers using IronPDF’s HTML header/footer options.

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = @"<img src='logo.png' style='height:28px;' />",
    DrawDividerLine = true
};
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "Page {page} of {total-pages}"
};
```
This is great for consistent branding on every page.

## How Do I Add Images Only on Certain PDF Pages?

You can target specific pages with the stamper by passing page indices:

```csharp
var stamp = new ImageStamper("approved.png") { Opacity = 65 };
pdf.ApplyStamp(stamp, 0); // Only the first page
pdf.ApplyStamp(stamp, new[] { 1, 3 }); // On pages 2 and 4 (zero-based)
```
Zero-based page indices—be careful not to go off-by-one!

## Can I Create PDFs Directly from Images (e.g., Scans, Galleries)?

Yes! IronPDF supports converting images to PDFs—either single images or entire galleries.

```csharp
var pdf = ImageToPdfConverter.ImageToPdf("scan.jpg");
pdf.SaveAs("scan.pdf");

var multiPagePdf = ImageToPdfConverter.ImageToPdf(new[] { "img1.png", "img2.png" });
multiPagePdf.SaveAs("gallery.pdf");
```
For more about merging or manipulating pages, see [Add Copy Delete Pdf Pages Csharp](add-copy-delete-pdf-pages-csharp.md).

## What Are Common Image-Related Issues and How Do I Fix Them?

- **Broken images:** Check your paths and `BaseUrl`. For remote images, confirm URLs are public.
- **Blurry logos:** Prefer SVG or high-res PNGs.
- **Large PDFs:** Compress or resize images before embedding.
- **Header/footer image issues:** Use base64 or absolute paths to avoid missing assets.
- **Animated GIFs:** Only the first frame appears—PDFs don’t support animation.

For attachments, see [Add Attachments Pdf Csharp](add-attachments-pdf-csharp.md). For more advanced scenarios like migrating from Puppeteer, check [Migrate Puppeteer Playwright To Ironpdf](migrate-puppeteer-playwright-to-ironpdf.md).

## Where Can I Learn More About IronPDF and Image Handling?

- [IronPDF official documentation](https://ironpdf.com)
- [IronPDF image docs](https://ironpdf.com/nodejs/how-to/nodejs-pdf-to-image/)
- [Iron Software home](https://ironsoftware.com)
- For full API details and advanced use, see the IronPDF API reference.

---

*[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) created IronPDF to solve PDF generation headaches in .NET. He's now CTO at Iron Software, leading a team focused on developer tools.*
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

**About the Author**: Jacob Mellor is Chief Technology Officer at Iron Software, where PDF tools are used by Qualcomm and other Fortune 500 companies. With expertise in .NET, WebAssembly, C#, Jacob focuses on making PDF generation accessible to all .NET developers. [Author Page](https://ironsoftware.com/about-us/authors/jacobmellor/)
