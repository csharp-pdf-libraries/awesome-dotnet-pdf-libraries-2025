# How Do Base URLs Work in C# HTML-to-PDF Conversion, and Why Are My Assets Missing?

When converting HTML to PDF in C#, missing images and broken CSS are common headaches—usually caused by relative file paths not resolving as expected. Setting the right **base URL** is the secret to making your PDFs look just as polished as your web pages. This FAQ covers practical techniques, code patterns, and troubleshooting tips for working with base URLs in C# PDF generation, especially using IronPDF.

---

## Why Do Images, CSS, or Fonts Disappear When I Convert HTML to PDF in C#?

When you convert HTML to PDF, the PDF renderer can't automatically figure out where your relative asset paths (like `images/logo.png` or `css/site.css`) are located. Unlike a web browser, which knows the context of your HTML, PDF libraries need you to tell them where to find these assets.

If you don't provide this context, your PDF will likely be missing images, styles, and even fonts, resulting in a barebones document. This is a common issue for developers migrating from browser-based rendering to server-side PDF generation.

For a deeper guide to HTML-to-PDF basics, check out [Convert Html To Pdf Csharp](convert-html-to-pdf-csharp.md).

---

## What Exactly Is a Base URL in the Context of HTML-to-PDF Conversion?

A **base URL** is a reference point that tells the PDF renderer where to look for any relative paths in your HTML. Think of it as the "starting directory" for all your images, stylesheets, and scripts that don't use absolute URLs.

For example, if your HTML contains:

```html
<img src="assets/photo.jpg">
```

And you set your base URL to `https://example.com/static/`, the renderer will resolve the image as:

```
https://example.com/static/assets/photo.jpg
```

This applies to all relative assets—images, CSS, JavaScript, and even fonts.

---

## How Do I Set a Base URL Using IronPDF in C#?

IronPDF makes setting a base URL straightforward. Here’s a simple example:

```csharp
using IronPdf; // Install-Package IronPdf

var htmlContent = "<img src='images/logo.png'><link rel='stylesheet' href='css/styles.css'>";
var renderer = new ChromePdfRenderer();

var pdf = renderer.RenderHtmlAsPdf(htmlContent, baseUrl: "https://cdn.yoursite.com/");
pdf.SaveAs("output.pdf");
```

This tells IronPDF to resolve all relative asset paths against `https://cdn.yoursite.com/`. As a result, `images/logo.png` becomes `https://cdn.yoursite.com/images/logo.png`.

For more advanced scenarios—like complex layouts or multi-page documents—see [Advanced Html To Pdf Csharp](advanced-html-to-pdf-csharp.md).

---

## What Types of Paths Does a PDF Renderer Encounter, and How Are They Resolved?

PDF libraries process several types of paths:

- **Relative paths** (e.g., `images/photo.jpg`): These are resolved using the base URL you provide.
- **Absolute URLs** (e.g., `https://cdn.example.com/logo.png`): These always work, regardless of the base URL.
- **Root-relative paths** (e.g., `/images/banner.jpg`): These resolve from the domain root, not from your base URL, and can be a common source of confusion.

If you have a mix of these, only the relative paths will use your specified base URL.

---

## What Are the Most Common Patterns for Setting Base URLs in Real Projects?

### How Can I Reference Static Assets from a CDN in My PDFs?

Using a CDN is the gold standard for serving static files in production. Here’s a recommended approach:

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<link rel='stylesheet' href='css/invoice.css'>
<img src='images/logo.png'>
";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html, baseUrl: "https://cdn.example.com/");
pdf.SaveAs("invoice.pdf");
```

With this, all `css/invoice.css` and `images/logo.png` requests will resolve via your CDN, ensuring fast and reliable asset delivery.

---

### How Do I Reference Local Files When Running a Desktop or Server App?

If your assets are on the local file system (not the web), set the base URL to the absolute folder path:

```csharp
using IronPdf; // Install-Package IronPdf
using System.IO;

var html = @"
<link rel='stylesheet' href='styles/report.css'>
<img src='charts/chart1.png'>
";
string assetDirectory = @"C:\MyApp\Assets\";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html, baseUrl: assetDirectory);
pdf.SaveAs("report.pdf");
```

- Always use absolute paths (e.g., `C:\...` on Windows or `/var/www/...` on Linux).
- Relative paths will not resolve correctly.

---

### What’s the Right Way to Handle Base URLs in ASP.NET Core or MVC Projects?

In ASP.NET Core, static assets are typically in the `wwwroot` folder. You can inject the environment into your PDF service and use the absolute web root path:

```csharp
using IronPdf; // Install-Package IronPdf
using Microsoft.AspNetCore.Hosting;

public class PdfGenerator
{
    private readonly IWebHostEnvironment _env;
    public PdfGenerator(IWebHostEnvironment env) => _env = env;

    public void CreatePdf()
    {
        var html = "<img src='images/logo.png'>";
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html, baseUrl: _env.WebRootPath);
        pdf.SaveAs("webroot-output.pdf");
    }
}
```

This adapts automatically to development, staging, and production environments. If you need to manage authentication or cookies when fetching resources, see [Cookies Html To Pdf Csharp](cookies-html-to-pdf-csharp.md).

---

### What Happens If My HTML Uses a Mix of Absolute and Relative URLs?

Absolute URLs (like `https://cdn.partner.com/logo.png`) are not affected by the base URL—they remain unchanged. Only relative paths (e.g., `logo.png`) are resolved using your base URL.

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<img src='logo.png'> <!-- Uses base URL -->
<img src='https://cdn.partner.com/banner.jpg'> <!-- Absolute URL -->
";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html, baseUrl: "https://myassets.com/images/");
pdf.SaveAs("mixed-assets.pdf");
```

This is especially useful for including both local branding and remote resources.

---

### Can I Embed Images or Fonts Directly into My HTML Using Base64 Data URIs?

Yes! If you can’t access external assets (due to security, firewalls, etc.), you can embed small assets as base64-encoded data URIs:

```csharp
using IronPdf; // Install-Package IronPdf
using System.IO;

var imageBytes = File.ReadAllBytes("logo.png");
var base64Image = Convert.ToBase64String(imageBytes);

var html = $"<img src='data:image/png;base64,{base64Image}' />";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("self-contained.pdf");
```

Use this approach for small images or logos—avoid it for large files or videos.

For more on SVG and base64 usage, explore [Svg To Pdf Csharp](svg-to-pdf-csharp.md).

---

### How Should I Set Base URLs for Headers and Footers in My PDF?

Headers and footers are rendered as separate HTML documents and need their own base URLs:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<img src='header-logo.png'>",
    BaseUrl = "https://cdn.example.com/images/"
};
var html = "<p>Main content here</p>";
var pdf = renderer.RenderHtmlAsPdf(html, baseUrl: "https://example.com/");
pdf.SaveAs("header-footer-assets.pdf");
```

Remember: Setting the base URL on your main content does **not** affect headers or footers.

---

### Can I Inject Global CSS Into All PDFs Without Altering the HTML?

Absolutely—use the `CustomCssUrl` option to apply a stylesheet globally:

```csharp
using IronPdf; // Install-Package IronPdf

var html = "<p>This HTML has no CSS classes!</p>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.CustomCssUrl = "https://cdn.example.com/styles/global.css";
var pdf = renderer.RenderHtmlAsPdf(html, baseUrl: "https://example.com/");
pdf.SaveAs("with-global-css.pdf");
```

This is perfect for applying a consistent look across many documents.

---

### Does the Base URL Apply to All Asset Types (CSS, JS, Images, Video)?

Yes—any relative path (not just images) will be resolved using the base URL:

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<link rel='stylesheet' href='css/main.css'>
<script src='js/app.js'></script>
<img src='images/photo.jpg'>
<video src='videos/demo.mp4'></video>
";
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html, baseUrl: "https://cdn.example.com/");
pdf.SaveAs("all-types.pdf");
```

Note: While video and audio tags will resolve, PDF itself does not support playback—these are usually rendered as placeholders.

---

## How Can I Troubleshoot Missing Assets and Common Path Issues?

### How Can I See What Path Is Being Resolved?

Print out the full resolved path to verify:

```csharp
var resolvedPath = Path.Combine(baseUrl, "images/logo.png");
Console.WriteLine(resolvedPath);
```

Or for a web URL:

```csharp
Console.WriteLine($"{baseUrl}images/logo.png");
```

---

### What If My Images Still Don’t Show Up—What Should I Check?

- **Test Resolved URLs in a Browser:** If you paste the full resolved asset URL in your browser and get a 404, the PDF renderer won’t find it either.
- **Check File Permissions:** On servers, ensure that the application has read access to all asset files.
- **Try Absolute URLs:** Temporarily switch all asset references to absolute URLs to check if there’s a base URL resolution issue.

---

### Are There Special Considerations for Root-Relative and Case-Sensitive Paths?

- **Root-Relative Paths** (e.g., `/images/logo.png`) resolve from the domain root. If your base URL is `https://cdn.example.com/assets/`, `/images/logo.png` becomes `https://cdn.example.com/images/logo.png`—not inside `/assets/`.
- **Case Sensitivity:** Windows is case-insensitive; Linux is not. Make sure your file and path casing matches exactly.

---

### How Do I Handle Environments With No File or Network Access?

If you can't access the filesystem or external URLs, embed small assets as base64. For larger assets or advanced requirements, see [Itextsharp Terrible Html To Pdf Alternatives](itextsharp-terrible-html-to-pdf-alternatives.md) for alternative approaches.

---

## What’s a Quick Reference for Setting Base URLs in Different Scenarios?

| Situation               | Example Base URL                        |
|-------------------------|-----------------------------------------|
| CDN assets              | `https://cdn.example.com/`              |
| Local files (Windows)   | `C:\Project\Assets\`                    |
| Local files (Linux)     | `/var/www/assets/`                      |
| ASP.NET Core (wwwroot)  | Use `env.WebRootPath`                   |
| Header/Footer assets    | Set via `HtmlHeaderFooter.BaseUrl`      |
| No file/network access  | Base64-encode assets in HTML            |

---

## Where Can I Learn More About Advanced HTML to PDF Asset Handling in C#?

For deeper dives into advanced rendering, custom pagination, SVG handling, and cookies, check these related guides:

- [Advanced Html To Pdf Csharp](advanced-html-to-pdf-csharp.md) for edge cases and complex layouts
- [Convert Html To Pdf Csharp](convert-html-to-pdf-csharp.md) for a beginner-to-expert walkthrough
- [Cookies Html To Pdf Csharp](cookies-html-to-pdf-csharp.md) for authenticated asset access
- [Svg To Pdf Csharp](svg-to-pdf-csharp.md) for vector graphics support
- [Itextsharp Terrible Html To Pdf Alternatives](itextsharp-terrible-html-to-pdf-alternatives.md) for migration tips and alternatives

And, of course, the [IronPDF documentation](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/) and [Iron Software](https://ironsoftware.com) resources are excellent places to keep up to date.

---

## What Are the Key Takeaways for Base URLs in C# HTML-to-PDF Conversion?

- **Always specify an absolute base URL** (web URL or full directory path).
- **Headers and footers** require their own base URLs.
- **Test asset URLs in your browser** to confirm they’re accessible.
- **Use base64 for embedding small assets** when external access isn’t possible.
- **Absolute URLs in HTML override the base URL**.

For the biggest time saver: **Set the base URL for every conversion.** It solves 99% of missing asset problems in PDF generation with C#.

Happy coding—and happy PDF-ing!

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
