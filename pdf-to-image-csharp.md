# Convert PDF to Image in C#: PNG, JPEG, and Thumbnail Guide

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF

[![.NET](https://img.shields.io/badge/.NET-6%2B-512BD4)](https://dotnet.microsoft.com/)
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> Converting PDF pages to images enables thumbnails, previews, and image-based processing. This guide covers rasterization with resolution control and library comparisons.

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Library Comparison](#library-comparison)
3. [PDF to PNG](#pdf-to-png)
4. [PDF to JPEG](#pdf-to-jpeg)
5. [Resolution and Quality](#resolution-and-quality)
6. [Thumbnails](#thumbnails)
7. [Batch Conversion](#batch-conversion)
8. [Common Use Cases](#common-use-cases)

---

## Quick Start

### PDF to PNG with IronPDF

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Convert all pages to PNG images
pdf.RasterizeToImageFiles("output/page_*.png");
```

### Single Page to Image

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Convert first page only
var images = pdf.ToBitmap();
images[0].Save("first-page.png");
```

---

## Library Comparison

### PDF to Image Capabilities

| Library | To PNG | To JPEG | DPI Control | Thumbnails | Quality Control |
|---------|--------|---------|-------------|------------|-----------------|
| **IronPDF** | ✅ Simple | ✅ | ✅ | ✅ | ✅ |
| Aspose.PDF | ✅ | ✅ | ✅ | ✅ | ✅ |
| PDFSharp | ⚠️ Limited | ⚠️ | ⚠️ | ⚠️ | ⚠️ |
| Docotic.Pdf | ✅ | ✅ | ✅ | ✅ | ✅ |
| PuppeteerSharp | ⚠️ Screenshot | ⚠️ | ⚠️ | ⚠️ | ⚠️ |
| QuestPDF | ❌ | ❌ | ❌ | ❌ | ❌ |
| iText7 | ❌ | ❌ | ❌ | ❌ | ❌ |

**Key insight:** iText7 cannot convert PDF to images natively. QuestPDF is generation-only.

### Code Complexity

**IronPDF — 2 lines:**
```csharp
var pdf = PdfDocument.FromFile("doc.pdf");
pdf.RasterizeToImageFiles("page_*.png");
```

**Aspose.PDF — 10+ lines:**
```csharp
var document = new Aspose.Pdf.Document("doc.pdf");
for (int pageCount = 1; pageCount <= document.Pages.Count; pageCount++)
{
    using var imageStream = new FileStream($"page_{pageCount}.png", FileMode.Create);
    var resolution = new Aspose.Pdf.Devices.Resolution(300);
    var pngDevice = new Aspose.Pdf.Devices.PngDevice(resolution);
    pngDevice.Process(document.Pages[pageCount], imageStream);
}
```

---

## PDF to PNG

### All Pages

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("report.pdf");

// Wildcard pattern creates page_1.png, page_2.png, etc.
pdf.RasterizeToImageFiles("output/page_*.png");

Console.WriteLine($"Converted {pdf.PageCount} pages to PNG");
```

### Specific Pages

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Convert pages 1, 3, and 5 (0-indexed)
var pageIndexes = new[] { 0, 2, 4 };

foreach (int i in pageIndexes)
{
    var bitmap = pdf.PageToBitmap(i);
    bitmap.Save($"selected_page_{i + 1}.png");
}
```

### To Memory (Byte Array)

```csharp
using IronPdf;
using System.Drawing.Imaging;

var pdf = PdfDocument.FromFile("document.pdf");

// Get as byte arrays for web/API use
var imageBytes = new List<byte[]>();

foreach (var bitmap in pdf.ToBitmap())
{
    using var ms = new MemoryStream();
    bitmap.Save(ms, ImageFormat.Png);
    imageBytes.Add(ms.ToArray());
}

// Use in web response
// return File(imageBytes[0], "image/png");
```

---

## PDF to JPEG

### Basic JPEG Conversion

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// JPEG output
pdf.RasterizeToImageFiles("output/page_*.jpg");
```

### With Quality Control

```csharp
using IronPdf;
using System.Drawing;
using System.Drawing.Imaging;

var pdf = PdfDocument.FromFile("document.pdf");

// JPEG quality encoder
var jpegEncoder = ImageCodecInfo.GetImageEncoders()
    .First(e => e.MimeType == "image/jpeg");

var encoderParams = new EncoderParameters(1);
encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 85L); // 85% quality

int page = 1;
foreach (var bitmap in pdf.ToBitmap())
{
    bitmap.Save($"page_{page++}.jpg", jpegEncoder, encoderParams);
}
```

---

## Resolution and Quality

### High Resolution (Print Quality)

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// 300 DPI for print quality
pdf.RasterizeToImageFiles("print_quality_*.png", 300);
```

### Low Resolution (Web Thumbnails)

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// 72 DPI for web use - smaller file sizes
pdf.RasterizeToImageFiles("web_*.png", 72);
```

### Custom Resolution

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Different resolutions for different uses
var resolutions = new Dictionary<string, int>
{
    { "thumbnail", 72 },
    { "preview", 150 },
    { "print", 300 },
    { "archive", 600 }
};

foreach (var kvp in resolutions)
{
    pdf.RasterizeToImageFiles($"{kvp.Key}/page_*.png", kvp.Value);
}
```

---

## Thumbnails

### Generate Thumbnails

```csharp
using IronPdf;
using System.Drawing;

var pdf = PdfDocument.FromFile("document.pdf");

// Low DPI for thumbnails
var thumbnails = pdf.ToBitmap(72);

int page = 1;
foreach (var bitmap in thumbnails)
{
    // Resize to thumbnail dimensions
    var thumb = new Bitmap(bitmap, new Size(150, 200));
    thumb.Save($"thumbnails/thumb_{page++}.png");
}
```

### First Page Thumbnail Only

```csharp
using IronPdf;
using System.Drawing;

var pdf = PdfDocument.FromFile("document.pdf");

// Get first page as low-res thumbnail
var firstPage = pdf.PageToBitmap(0, 72);

// Resize maintaining aspect ratio
int targetWidth = 200;
int targetHeight = (int)(firstPage.Height * (targetWidth / (double)firstPage.Width));

var thumbnail = new Bitmap(firstPage, new Size(targetWidth, targetHeight));
thumbnail.Save("document-thumbnail.png");
```

### Thumbnail Grid

```csharp
using IronPdf;
using System.Drawing;

var pdf = PdfDocument.FromFile("presentation.pdf");

const int thumbWidth = 150;
const int thumbHeight = 200;
const int columns = 4;
const int padding = 10;

var thumbnails = pdf.ToBitmap(72);
int rows = (int)Math.Ceiling(thumbnails.Length / (double)columns);

// Create grid image
int gridWidth = columns * (thumbWidth + padding) + padding;
int gridHeight = rows * (thumbHeight + padding) + padding;

using var grid = new Bitmap(gridWidth, gridHeight);
using var graphics = Graphics.FromImage(grid);
graphics.Clear(Color.White);

for (int i = 0; i < thumbnails.Length; i++)
{
    int col = i % columns;
    int row = i / columns;

    var thumb = new Bitmap(thumbnails[i], new Size(thumbWidth, thumbHeight));

    int x = padding + col * (thumbWidth + padding);
    int y = padding + row * (thumbHeight + padding);

    graphics.DrawImage(thumb, x, y);
}

grid.Save("thumbnail-grid.png");
```

---

## Batch Conversion

### Convert All PDFs in Directory

```csharp
using IronPdf;

public void ConvertAllPdfs(string inputDir, string outputDir)
{
    var pdfFiles = Directory.GetFiles(inputDir, "*.pdf");

    foreach (var pdfPath in pdfFiles)
    {
        string baseName = Path.GetFileNameWithoutExtension(pdfPath);
        string outputSubDir = Path.Combine(outputDir, baseName);
        Directory.CreateDirectory(outputSubDir);

        var pdf = PdfDocument.FromFile(pdfPath);
        pdf.RasterizeToImageFiles(Path.Combine(outputSubDir, "page_*.png"));

        Console.WriteLine($"Converted: {baseName} ({pdf.PageCount} pages)");
    }
}

// Usage
ConvertAllPdfs("documents/", "images/");
```

### Parallel Processing

```csharp
using IronPdf;

public async Task ConvertPdfsParallel(string[] pdfPaths, string outputDir)
{
    await Parallel.ForEachAsync(pdfPaths, async (pdfPath, ct) =>
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        string baseName = Path.GetFileNameWithoutExtension(pdfPath);

        pdf.RasterizeToImageFiles(Path.Combine(outputDir, $"{baseName}_*.png"));

        Console.WriteLine($"Completed: {baseName}");
    });
}
```

---

## Common Use Cases

### Document Preview System

```csharp
using IronPdf;
using System.Drawing;

public class DocumentPreviewService
{
    public byte[] GetPreviewImage(string pdfPath, int pageIndex = 0)
    {
        var pdf = PdfDocument.FromFile(pdfPath);

        if (pageIndex >= pdf.PageCount)
            pageIndex = 0;

        var bitmap = pdf.PageToBitmap(pageIndex, 150); // 150 DPI for preview

        using var ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }

    public int GetPageCount(string pdfPath)
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        return pdf.PageCount;
    }
}
```

### E-Commerce Product Catalog

```csharp
using IronPdf;

public void GenerateProductImages(string catalogPdf, string outputDir)
{
    var pdf = PdfDocument.FromFile(catalogPdf);

    // Generate multiple sizes
    var sizes = new[] { 100, 300, 800 };

    for (int i = 0; i < pdf.PageCount; i++)
    {
        foreach (int size in sizes)
        {
            // DPI calculation for target width
            int dpi = (int)(size * 72.0 / 612); // Assuming letter size PDF

            var bitmap = pdf.PageToBitmap(i, Math.Max(dpi, 72));
            bitmap.Save(Path.Combine(outputDir, $"product_{i + 1}_{size}px.png"));
        }
    }
}
```

### PDF Slideshow

```csharp
using IronPdf;
using System.Drawing;

public class PdfSlideshow
{
    private readonly List<Bitmap> _slides;
    private int _currentSlide;

    public PdfSlideshow(string pdfPath)
    {
        var pdf = PdfDocument.FromFile(pdfPath);
        _slides = pdf.ToBitmap(150).ToList();
        _currentSlide = 0;
    }

    public Bitmap GetCurrentSlide() => _slides[_currentSlide];

    public Bitmap NextSlide()
    {
        _currentSlide = (_currentSlide + 1) % _slides.Count;
        return GetCurrentSlide();
    }

    public Bitmap PreviousSlide()
    {
        _currentSlide = (_currentSlide - 1 + _slides.Count) % _slides.Count;
        return GetCurrentSlide();
    }

    public int TotalSlides => _slides.Count;
    public int CurrentIndex => _currentSlide + 1;
}
```

---

## Recommendations

### Choose IronPDF for PDF to Image When:
- ✅ Simple 2-line API
- ✅ Resolution/DPI control needed
- ✅ Combined with other PDF operations
- ✅ Cross-platform deployment

### Choose Docotic.Pdf When:
- Very specific rendering requirements
- High-fidelity color management needed

### Cannot Convert PDF to Image:
- ❌ iText7 — No native rasterization
- ❌ QuestPDF — Generation only
- ❌ PDFSharp — Very limited support

---

## Related Tutorials

- **[Merge PDFs](merge-split-pdf-csharp.md)** — Combine before conversion
- **[Extract Text](extract-text-from-pdf-csharp.md)** — OCR converted images
- **[Watermark PDFs](watermark-pdf-csharp.md)** — Add watermarks before conversion
- **[Best PDF Libraries](best-pdf-libraries-dotnet-2025.md)** — Full comparison

---

### More Tutorials
- **[HTML to PDF](html-to-pdf-csharp.md)** — Generate PDFs to convert
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web image generation
- **[Cross-Platform](cross-platform-pdf-dotnet.md)** — Deploy conversion services
- **[IronPDF Guide](ironpdf/)** — Full rasterization API

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*
