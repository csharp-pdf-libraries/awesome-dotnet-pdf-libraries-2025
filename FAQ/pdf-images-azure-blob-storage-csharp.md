# How Can I Embed Azure Blob Storage Images in C# PDFs the Right Way?

Embedding images stored in Azure Blob Storage into PDFs generated with C# can be tricky—especially if you want the PDFs to work offline, look sharp, and avoid broken image links. This FAQ covers the most reliable techniques for pulling images from Azure Blob Storage, embedding them in PDFs using IronPDF, handling large images, optimizing for cloud workflows, and avoiding common pitfalls.

Whether you’re building dynamic reports, automating document workflows, or just want to avoid the usual headache of broken image icons, you’ll find practical, production-ready guidance here.

---

## Why Would I Want to Use Azure Blob Storage Images in My C#-Generated PDFs?

Azure Blob Storage is a popular choice for storing images, logos, charts, and other assets in the cloud because of its scalability, cost-effectiveness, and native integration with Azure services. But when you’re generating PDFs in C#—especially in cloud environments like Azure Functions or containers—you often don’t have access to local files.

To make your PDFs dynamic and cloud-friendly, you’ll want to:
- Fetch images directly from Azure Blob Storage (on demand)
- Embed them into PDFs so they render reliably (even offline)
- Manage authentication, security, and performance
- Save the resulting PDFs back to Blob Storage for sharing or archiving

If you’re looking to extract images from PDFs or convert PDFs to images, see our guides on [Extract Images From PDF in C#](extract-images-from-pdf-csharp.md) and [PDF to Images in C#](pdf-to-images-csharp.md).

---

## What Libraries and Tools Do I Need to Embed Blob Images in PDFs?

To get started, you’ll want the following NuGet packages:
- `IronPdf` – The go-to .NET library for [PDF generation](https://ironpdf.com/blog/videos/how-to-generate-pdf-files-in-dotnet-core-using-ironpdf/)
- `Azure.Storage.Blobs` – For reading/writing blobs in Azure Storage
- `SkiaSharp` (optional) – For resizing and optimizing images before embedding
- .NET 6 or newer

You can quickly add the packages like so:

```bash
dotnet add package IronPdf
dotnet add package Azure.Storage.Blobs
dotnet add package SkiaSharp
```

Or in your source:

```csharp
// Install-Package IronPdf
// Install-Package Azure.Storage.Blobs
// Install-Package SkiaSharp
```

You’ll also need some images in your Azure Blob Storage container. For more on why so many developers choose IronPDF, check out [Why Developers Choose Ironpdf](why-developers-choose-ironpdf.md).

---

## How Do I Embed Azure Blob Storage Images as Base64 in a PDF for Maximum Reliability?

Embedding images as Base64 data URIs is the most foolproof way to ensure your images always render, even if the PDF is opened offline or the Blob Storage service is unreachable.

Here’s a handy async helper method to fetch a blob and convert it to a Base64 string:

```csharp
using IronPdf; // Install-Package IronPdf
using Azure.Storage.Blobs; // Install-Package Azure.Storage.Blobs

public static async Task<string> GetImageAsBase64Async(string connStr, string container, string blobName)
{
    var containerClient = new BlobContainerClient(connStr, container);
    var blobClient = containerClient.GetBlobClient(blobName);
    var resp = await blobClient.DownloadContentAsync();
    var bytes = resp.Value.Content.ToArray();
    var ext = Path.GetExtension(blobName).ToLowerInvariant();
    var mime = ext switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".svg" => "image/svg+xml",
        _ => "application/octet-stream"
    };
    return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
}
```

**How do I use this when generating a PDF?**

```csharp
string connStr = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
string logoDataUri = await GetImageAsBase64Async(connStr, "assets", "logo.png");
string chartDataUri = await GetImageAsBase64Async(connStr, "assets", "stats-chart.jpg");

string html = $@"
  <html>
    <body>
      <img src='{logoDataUri}' style='height:50px;'/>
      <h1>Monthly Analytics</h1>
      <img src='{chartDataUri}' style='width:85%;'/>
    </body>
  </html>";

var pdfRenderer = new ChromePdfRenderer();
var pdfDoc = pdfRenderer.RenderHtmlAsPdf(html);
pdfDoc.SaveAs("monthly-analytics.pdf");
```

With Base64 embedding, the images are part of the PDF file itself, so you never have to worry about broken links.

---

### Can I Embed Multiple Images from Blob Storage in One Go?

Absolutely. Here’s how you can fetch and embed several images efficiently:

```csharp
public static async Task<Dictionary<string, string>> FetchImagesBase64(string connStr, string container, params string[] blobNames)
{
    var client = new BlobContainerClient(connStr, container);
    var results = new Dictionary<string, string>();

    foreach (var blob in blobNames)
    {
        var blobClient = client.GetBlobClient(blob);
        var resp = await blobClient.DownloadContentAsync();
        string ext = Path.GetExtension(blob).ToLowerInvariant();
        string mime = ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
        results[blob] = $"data:{mime};base64,{Convert.ToBase64String(resp.Value.Content.ToArray())}";
    }
    return results;
}

// Usage
var images = await FetchImagesBase64(connStr, "assets", "logo.png", "banner.png", "footer.jpg");

string html = $@"
  <img src='{images["logo.png"]}' style='height:40px;'/>
  <h2>Report</h2>
  <img src='{images["banner.png"]}' style='width:90%'/>
  <footer>
    <img src='{images["footer.jpg"]}' style='width:100%'/>
  </footer>";

var pdf = new ChromePdfRenderer().RenderHtmlAsPdf(html);
pdf.SaveAs("styled-report.pdf");
```

---

## When Should I Use Blob URLs (SAS or Public) Instead of Base64 Embedding?

While Base64 is ultra-reliable, embedding large images this way can make your PDF files huge. If your images are large (multi-megabyte) or you need to keep PDF sizes down, you can embed images by URL—using either public blob URLs or SAS (Shared Access Signature) URLs for private blobs.

**How do I generate a secure SAS URL for a blob?**

```csharp
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

public static string CreateBlobSasUrl(string connStr, string container, string blobName, int minutes = 60)
{
    var containerClient = new BlobContainerClient(connStr, container);
    var blobClient = containerClient.GetBlobClient(blobName);

    var sasBuilder = new BlobSasBuilder
    {
        BlobContainerName = container,
        BlobName = blobName,
        Resource = "b",
        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(minutes)
    };
    sasBuilder.SetPermissions(BlobSasPermissions.Read);

    return blobClient.GenerateSasUri(sasBuilder).ToString();
}
```

**Example usage:**

```csharp
string imageUrl = CreateBlobSasUrl(connStr, "assets", "dashboard.png");

string html = $@"
  <html>
    <body>
      <img src='{imageUrl}' style='width:80%;'/>
      <p>Dashboard Overview</p>
    </body>
  </html>";

var pdf = new ChromePdfRenderer().RenderHtmlAsPdf(html);
pdf.SaveAs("dashboard-overview.pdf");
```

**Remember:** Your server needs to be able to access the image URL at PDF generation time. After that, the image is "baked in" to the PDF.

---

### Should I Use Base64 or URLs for My Images?

- **Base64**: Best for small images, icons, signatures, or when you need PDFs to work offline.
- **SAS/Public URLs**: Great for large images or when minimizing PDF file size is important.

For more about manipulating PDF images, see [Flatten Pdf Images in C#](flatten-pdf-images-csharp.md).

---

## How Can I Batch Process Multiple Blob Images for a PDF?

If you need to embed several images in one PDF, it’s best to batch-fetch the SAS URLs:

```csharp
public static async Task<List<string>> GenerateBatchSasUrls(string connStr, string container, List<string> blobNames, int durationMins = 60)
{
    var containerClient = new BlobContainerClient(connStr, container);
    var result = new List<string>();

    foreach (var blob in blobNames)
    {
        var blobClient = containerClient.GetBlobClient(blob);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = container,
            BlobName = blob,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(durationMins)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        result.Add(blobClient.GenerateSasUri(sasBuilder).ToString());
    }
    return result;
}

// Usage
var blobs = new List<string> { "img1.png", "img2.png", "footer.png" };
var urls = await GenerateBatchSasUrls(connStr, "docs", blobs);

string html = $@"
  <img src='{urls[0]}' />
  <img src='{urls[1]}' />
  <img src='{urls[2]}' />";

var pdf = new ChromePdfRenderer().RenderHtmlAsPdf(html);
pdf.SaveAs("multi-image-report.pdf");
```

---

## How Do I Save My Generated PDF Back to Azure Blob Storage?

Once you’ve created your PDF, saving it to Azure Blob Storage is straightforward:

```csharp
using IronPdf; // Install-Package IronPdf
using Azure.Storage.Blobs;

public static async Task UploadPdfToBlobAsync(PdfDocument pdf, string connStr, string container, string blobName)
{
    var containerClient = new BlobContainerClient(connStr, container);
    await containerClient.CreateIfNotExistsAsync();
    var blobClient = containerClient.GetBlobClient(blobName);

    using var stream = new MemoryStream(pdf.BinaryData);
    await blobClient.UploadAsync(stream, overwrite: true);
    await blobClient.SetHttpHeadersAsync(new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = "application/pdf" });
}

// Usage
var pdf = new ChromePdfRenderer().RenderHtmlAsPdf("<h1>Hello, Azure!</h1>");
await UploadPdfToBlobAsync(pdf, connStr, "pdfs", "hello-azure.pdf");
```

---

## How Can I Load a PDF from Blob Storage to Edit or Stamp It?

To modify an existing PDF stored in Azure Blob Storage (for example, to add a watermark), load it into IronPDF, edit, and re-upload:

```csharp
using IronPdf; // Install-Package IronPdf
using Azure.Storage.Blobs;

public static async Task<PdfDocument> OpenPdfFromBlobAsync(string connStr, string container, string blobName)
{
    var containerClient = new BlobContainerClient(connStr, container);
    var blobClient = containerClient.GetBlobClient(blobName);
    var resp = await blobClient.DownloadContentAsync();
    var bytes = resp.Value.Content.ToArray();
    return new PdfDocument(bytes);
}

// Example: Add "CONFIDENTIAL" stamp
var pdf = await OpenPdfFromBlobAsync(connStr, "pdfs", "confidential.pdf");
pdf.ApplyStamp(new TextStamper
{
    Text = "CONFIDENTIAL",
    Opacity = 25,
    Rotation = -40
});
await UploadPdfToBlobAsync(pdf, connStr, "pdfs", "confidential-stamped.pdf");
```

Want to dig into the PDF structure itself? See [How do I access the PDF DOM object in C#?](access-pdf-dom-object-csharp.md).

---

## What If My Images Are Huge? How Can I Optimize Before Embedding?

Large images can inflate your PDFs and slow down processing. Resize and compress images before embedding using SkiaSharp:

```csharp
using Azure.Storage.Blobs;
using SkiaSharp;

public static async Task<string> GetOptimizedBase64Image(string connStr, string container, string blobName, int maxWidth = 800)
{
    var containerClient = new BlobContainerClient(connStr, container);
    var blobClient = containerClient.GetBlobClient(blobName);
    var resp = await blobClient.DownloadContentAsync();
    var bytes = resp.Value.Content.ToArray();

    using var inputMs = new MemoryStream(bytes);
    using var bitmap = SKBitmap.Decode(inputMs);

    if (bitmap.Width > maxWidth)
    {
        float scale = (float)maxWidth / bitmap.Width;
        int height = (int)(bitmap.Height * scale);
        using var resized = bitmap.Resize(new SKSizeI(maxWidth, height), SKFilterQuality.Medium);
        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);
        return $"data:image/jpeg;base64,{Convert.ToBase64String(data.ToArray())}";
    }
    return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
}
```

Optimizing before embedding results in smaller, more manageable PDF files.

---

## How Do I Build a Complete Azure-Integrated PDF Report Service in C#?

Building a robust, cloud-native PDF report service is as simple as orchestrating these steps:

```csharp
using IronPdf;
using Azure.Storage.Blobs;

public class AzurePdfReportService
{
    private readonly string _connStr;
    private readonly BlobContainerClient _images;
    private readonly BlobContainerClient _pdfs;

    public AzurePdfReportService(string connStr)
    {
        _connStr = connStr;
        _images = new BlobContainerClient(connStr, "images");
        _pdfs = new BlobContainerClient(connStr, "reports");
    }

    public async Task<string> GenerateReportAsync(string reportId, string title, string summary)
    {
        string logo = await GetImage("company-logo.png");
        string chart = await GetImage($"charts/{reportId}-mainchart.png");

        string html = $@"
        <html>
          <body>
            <img src='{logo}' style='height:32px;'/>
            <h1>{title}</h1>
            <img src='{chart}' style='width:90%;'/>
            <p>{summary}</p>
          </body>
        </html>";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);

        string pdfName = $"report-{reportId}-{DateTime.Now:yyyyMMdd-HHmmss}.pdf";
        await SavePdf(pdf, pdfName);
        return pdfName;
    }

    private async Task<string> GetImage(string blobName)
    {
        var blob = _images.GetBlobClient(blobName);
        var resp = await blob.DownloadContentAsync();
        var bytes = resp.Value.Content.ToArray();
        var ext = Path.GetExtension(blobName).ToLowerInvariant();
        var mime = ext == ".png" ? "image/png" : "image/jpeg";
        return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
    }

    private async Task SavePdf(PdfDocument pdf, string blobName)
    {
        await _pdfs.CreateIfNotExistsAsync();
        var blob = _pdfs.GetBlobClient(blobName);

        using var stream = new MemoryStream(pdf.BinaryData);
        await blob.UploadAsync(stream, overwrite: true);
        await blob.SetHttpHeadersAsync(new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = "application/pdf" });
    }
}
```

**Usage:**
```csharp
var service = new AzurePdfReportService(connStr);
string pdfBlobName = await service.GenerateReportAsync(
    reportId: "Q2-2024",
    title: "Quarterly Results",
    summary: "Strong performance across regions."
);
Console.WriteLine($"Report uploaded as: {pdfBlobName}");
```

---

## What Are Common Pitfalls When Embedding Azure Blob Images in PDFs?

Here are some issues developers often run into—and how to fix them:

- **Images Not Appearing:** For images referenced by URL, ensure your server has outbound internet access and the blobs are public or SAS-authenticated. CORS isn’t an issue for [PDF rendering](https://ironpdf.com/nodejs/how-to/nodejs-pdf-to-image/), but expired or invalid SAS tokens are.
- **Large PDF Files:** Base64 encoding big images can balloon file size. Use image compression or direct URLs for large assets.
- **Authentication Problems:** Double-check your connection strings and permissions. For production, consider managed identity instead of hardcoding secrets.
- **Upload Fails:** Make sure the blob container exists (`CreateIfNotExistsAsync`). Always set the blob’s content type to `application/pdf`.
- **Editing PDFs “In Place”:** Azure Blob Storage is object storage; you must download, modify, and overwrite the PDF.
- **Memory or Performance Issues:** Always process large images async and in batches to avoid OOM errors.
- **Case Sensitivity:** Remember that blob names are case-sensitive.

If you encounter unique edge cases, feel free to share solutions in the comments.

---

## Where Can I Learn More or Get Help With PDF and Image Processing in C#?

For further reading, check out these related guides:
- [Extract Images From PDF in C#](extract-images-from-pdf-csharp.md)
- [PDF to Images in C#](pdf-to-images-csharp.md)
- [Flatten Pdf Images in C#](flatten-pdf-images-csharp.md)
- [Why Developers Choose Ironpdf](why-developers-choose-ironpdf.md)
- [How do I access the PDF DOM object in C#?](access-pdf-dom-object-csharp.md)

For more on IronPDF and its full capabilities, visit [IronPDF](https://ironpdf.com) and [Iron Software](https://ironsoftware.com).

---

*This FAQ was written by [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), CTO at Iron Software. Got a question that's not covered here? Drop it in the comments.*
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

[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) is the Head of Technology and Engineering of Iron Software, where he leads a 50+ person team building .NET libraries with over 41+ million NuGet downloads. With 41 years of coding experience, Jacob drives technical innovation in document processing. Based in Chiang Mai, Thailand, connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
