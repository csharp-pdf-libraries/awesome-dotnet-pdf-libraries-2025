# What Are the Most Common HTML-to-PDF Problems in C#, and How Do I Fix Them?

Converting [HTML to PDF](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/) in C# can seem straightforward—until you run into missing images, broken CSS, fonts that vanish, or mysteriously blank pages. Developers often discover that what works in a browser can fall apart during PDF generation. This FAQ tackles the real-world issues you’ll face and shows you practical, copy-paste solutions, from CSS mishaps to Docker deployment quirks. If you run into something we haven't covered, check out the related guides or drop a comment below.

---

## Why Do My PDF Styles Look Wrong Compared to the Browser?

This is one of the most common headaches: your HTML looks perfect in Chrome, but when rendered as a PDF, the styling is broken or missing entirely. It's usually caused by how the PDF tool handles CSS and resource paths.

### How Can I Fix Broken CSS in My PDF Output?

When CSS doesn't load, it's almost always due to the PDF renderer not knowing where to find your stylesheets. If you use a relative link like `<link rel="stylesheet" href="styles.css">`, it won’t resolve correctly unless you set the base URL.

**Solution:** Tell the renderer where to find your assets by setting the `BaseUrl`.

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<link rel='stylesheet' href='styles.css'>
<h1>Styled PDF Content</h1>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.BaseUrl = new Uri("https://yourdomain.com/");
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("styled-output.pdf");
```

With `BaseUrl`, the renderer maps all relative paths the same way your browser does. For more details and advanced scenarios on base URLs, see [How do I use BaseUrl for HTML to PDF in C#?](base-url-html-to-pdf-csharp.md).

### What If Inline or Modern CSS Still Isn’t Working?

Some legacy libraries (such as wkhtmltopdf or iTextSharp) don’t understand modern CSS features like Flexbox, Grid, or custom properties. If you want pixel-perfect output that matches modern browsers, you need a renderer built on Chromium—like IronPDF.

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<div style='display: flex; justify-content: space-between;'>
    <div>Left Side</div>
    <div>Right Side</div>
</div>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("flexbox-layout.pdf");
```

If you need even more advanced CSS support or want to push rendering capabilities further, check out [Advanced HTML to PDF in C#](advanced-html-to-pdf-csharp.md).

---

## Why Are My Images Missing in the PDF?

Missing images are almost always a path or permission issue. If your PDF is missing logos, photos, or icons, here’s what you can do.

### How Should I Reference Images in HTML for PDF Generation?

Prefer absolute URLs for maximum reliability:

```csharp
var html = "<img src='https://yourdomain.com/images/logo.png'>";
```

If you need to use relative paths, make sure you set the `BaseUrl` so the renderer knows how to resolve them:

```csharp
using IronPdf; // Install-Package IronPdf

var html = "<img src='logo.png'>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.BaseUrl = new Uri("https://yourdomain.com/images/");
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("image-in-pdf.pdf");
```

For more on resource paths and troubleshooting, see [How do I convert HTML to PDF in C#?](convert-html-to-pdf-csharp.md)

### Can I Use Local Files or Embedded Images?

Direct file paths like `<img src='C:\\images\\logo.png'>` rarely work, especially in production or on servers, due to access and path issues. The best approach is to use the `file:///` protocol or, even better, embed the image as a Base64 data URI:

```csharp
using IronPdf;
using System.IO; // For File.ReadAllBytes
// Install-Package IronPdf

var imageBytes = File.ReadAllBytes("logo.png");
var base64 = Convert.ToBase64String(imageBytes);
var html = $"<img src='data:image/png;base64,{base64}'>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("embedded-image.pdf");
```

Embedding images as Base64 ensures they’re always available, regardless of where the code runs.

---

## Why Don’t My Fonts Render Correctly in the PDF?

Fonts may vanish, change, or get replaced by generic ones if not handled properly. This is especially common with Google Fonts, custom fonts, or fonts loaded from a CDN.

### Why Won’t My Web Fonts Load in the PDF?

Web fonts can fail to load if the renderer tries to create the PDF before the font files finish downloading.

**Fix:** Use the `RenderDelay` property to give fonts time to load.

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<link href='https://fonts.googleapis.com/css2?family=Roboto' rel='stylesheet'>
<div style='font-family: Roboto;'>Should use Roboto font</div>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.RenderDelay = 700; // Wait 700ms for fonts
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("web-fonts-loaded.pdf");
```

You can determine the right delay by inspecting font load times in your browser’s network tools.

### How Can I Embed Custom Fonts for Maximum Reliability?

To avoid network dependency, embed your font with a Base64-encoded data URI:

```csharp
using IronPdf;
using System.IO;
// Install-Package IronPdf

var fontBytes = File.ReadAllBytes("MyFont.woff2");
var fontBase64 = Convert.ToBase64String(fontBytes);

var html = $@"
<style>
@font-face {{
    font-family: 'CustomFont';
    src: url(data:font/woff2;base64,{fontBase64}) format('woff2');
}}
body {{ font-family: 'CustomFont'; }}
</style>
<div>PDF with Embedded Font</div>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("custom-font.pdf");
```

This makes your PDFs portable and self-contained—no font errors, even in air-gapped environments.

---

## Why Is My JavaScript-Generated Content Missing in the PDF?

Dynamic content created by JavaScript (or frameworks like React, Vue, or Angular) often doesn’t show up because the PDF is rendered before the scripts finish.

### How Do I Make Sure JavaScript Content Appears in the PDF?

If your HTML relies on JavaScript to update the DOM, use `RenderDelay` to wait for scripts to complete:

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<div id='message'></div>
<script>
    setTimeout(() => {{
        document.getElementById('message').innerText = 'Loaded by JS!';
    }}, 150);
</script>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.RenderDelay = 400; // Wait for JS
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("js-content.pdf");
```

### What If I’m Using a Large SPA (React, Vue, Angular)?

Frameworks often need more time or a signal that rendering is complete. You can wait for a custom JavaScript variable using `WaitFor.Expression`:

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<div id='app'>Loading...</div>
<script>
    setTimeout(() => {{
        document.getElementById('app').innerHTML = '<h1>App Ready</h1>';
        window.appReady = true;
    }}, 800);
</script>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.WaitFor.Expression("window.appReady === true");
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("spa-complete.pdf");
```

This ensures the PDF only renders once your app signals readiness.

For more on handling advanced JavaScript scenarios, see [Advanced HTML to PDF in C#](advanced-html-to-pdf-csharp.md).

---

## Why Are My Tables and Sections Splitting Across Pages?

Odd page breaks and split content are a classic frustration. Fortunately, Chromium-based engines respect standard CSS for pagination.

### How Can I Force or Prevent Page Breaks in My PDF?

To force a break:

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<div>Page 1</div>
<div style='page-break-after: always;'></div>
<div>Page 2</div>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("forced-break.pdf");
```

To stop content from being split, use `page-break-inside: avoid` on containers:

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<style>
.keep-together {{ page-break-inside: avoid; }}
</style>
<div class='keep-together'>
    <h2>Section</h2>
    <p>This section won’t break across pages.</p>
</div>";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("no-split.pdf");
```

For more complex layout control, see [How do I handle page breaks in HTML to PDF?](advanced-html-to-pdf-csharp.md).

---

## What Should I Do When My PDF Output Is Blank?

A blank PDF often signals a major failure—invalid HTML, missing resources, or a rendering crash.

### How Can I Debug Blank or Failed PDF Generation?

Always catch exceptions and log errors for more insight:

```csharp
using IronPdf; // Install-Package IronPdf

try
{
    var renderer = new ChromePdfRenderer();
    var pdf = renderer.RenderHtmlAsPdf("<h1>Testing</h1>");
    pdf.SaveAs("output.pdf");
}
catch (Exception ex)
{
    Console.WriteLine($"PDF rendering failed: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
```

**Common causes of blank PDFs:**
- Broken HTML (unclosed tags)
- Missing `BaseUrl` (so CSS/images fail to load)
- JavaScript errors
- Licensing problems

Always test your HTML in a regular browser first—the same issues usually show up there. For more troubleshooting tips, check out [Convert HTML to PDF in C#](convert-html-to-pdf-csharp.md).

---

## Why Is PDF Generation Slow, and How Can I Speed It Up?

Rendering HTML to PDF can be slow if you reinitialize the rendering engine for every document.

### Should I Reuse the Renderer Instance?

**Yes!** Creating the renderer is expensive; reusing it is much faster.

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();

for (int i = 0; i < 50; i++)
{
    var pdf = renderer.RenderHtmlAsPdf($"<h1>Doc {i}</h1>");
    pdf.SaveAs($"doc-{i}.pdf");
}
```

This approach can make batch PDF generation up to 10x faster.

---

## Why Does PDF Rendering Fail in Docker or Linux?

Running in containers or on Linux systems often requires extra setup due to Chromium’s dependencies.

### What Linux Packages Does IronPDF Need?

Add these to your Dockerfile to avoid initialization errors:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

RUN apt-get update && apt-get install -y \
    libc6-dev \
    libgdiplus \
    libx11-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

Without these, you’ll often get errors about failing to start Chromium. For more on deployment, see [Why do developers choose IronPDF?](why-developers-choose-ironpdf.md).

---

## Why Do CSS Frameworks Like Bootstrap or Tailwind Look Broken in My PDFs?

Bootstrap 4+, Tailwind, and CSS Grid require a modern rendering engine. Older libraries often choke on their CSS and layouts.

### How Do I Get Modern CSS Frameworks to Render Perfectly?

Use Chromium-based rendering and give time for CSS to load:

```csharp
using IronPdf; // Install-Package IronPdf

var html = @"
<link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
<div class='container'>
    <div class='row'>
        <div class='col'>Column 1</div>
        <div class='col'>Column 2</div>
    </div>
</div>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.RenderDelay = 350; // Allow CSS to load
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("bootstrap-output.pdf");
```

For a deeper dive into framework compatibility and advanced rendering, see [Advanced HTML to PDF in C#](advanced-html-to-pdf-csharp.md) and [Why do developers choose IronPDF?](why-developers-choose-ironpdf.md).

---

## How Do I Add Headers, Footers, and Page Numbers Without Overlap?

Headers and footers can end up on top of your main content if you don’t adjust the margins.

### How Can I Add Custom Headers and Footers Correctly?

Always increase the top/bottom margins to make room.

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align: center;'>Custom Header</div>"
};
renderer.RenderingOptions.MarginTop = 50; // Make room for header

renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
};
renderer.RenderingOptions.MarginBottom = 45; // Make room for footer

var pdf = renderer.RenderHtmlAsPdf("<h1>Main Content</h1>");
pdf.SaveAs("header-footer.pdf");
```

Want to add dynamic [page numbers](https://ironpdf.com/blog/compare-to-other-components/questpdf-add-page-number-to-pdf/)? Just use `{page}` and `{total-pages}` in your footer’s HTML.

---

## How Can I Improve Image and Text Quality in My PDFs?

Blurry or pixelated output is often due to low DPI settings.

### What DPI Should I Use for High-Quality PDFs?

You can set the DPI (dots per inch) for your renderer like this:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.Dpi = 300; // Great for print

var pdf = renderer.RenderHtmlAsPdf("<h1>Sharp Content</h1>");
pdf.SaveAs("high-res.pdf");
```

- **96 DPI:** Good for screens (default)
- **150 DPI:** Decent for basic printing
- **300 DPI:** Professional print quality

Keep in mind that higher DPI will increase the file size.

---

## Why Does My App Crash When Rendering Multiple PDFs in Parallel?

Parallel PDF generation can lead to thread-safety issues or memory exhaustion.

### How Should I Handle Concurrent PDF Rendering?

Limit parallelism by using something like a semaphore or dataflow block:

```csharp
using IronPdf;
using System.Threading.Tasks.Dataflow; // Install System.Threading.Tasks.Dataflow via NuGet
// Install-Package IronPdf

var renderer = new ChromePdfRenderer();

var options = new ExecutionDataflowBlockOptions
{
    MaxDegreeOfParallelism = 4 // Adjust based on your system
};

var block = new ActionBlock<string>(async html =>
{
    var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    await pdf.SaveAsAsync($"{Guid.NewGuid()}.pdf");
}, options);

foreach (var htmlDoc in myHtmlDocs)
{
    await block.SendAsync(htmlDoc);
}

block.Complete();
await block.Completion;
```

This keeps your app stable and avoids out-of-memory errors during heavy loads.

---

## What Should I Do If I Get “License Key Required” Errors?

If your code runs fine in development but fails in production, you may be missing a license key.

### How Do I Provide the IronPDF License Key?

Set the license key as early as possible, preferably from an environment variable or configuration:

```csharp
using IronPdf; // Install-Package IronPdf

IronPdf.License.LicenseKey = Environment.GetEnvironmentVariable("IRONPDF_LICENSE");

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Licensed PDF</h1>");
pdf.SaveAs("licensed.pdf");
```

This avoids hardcoding sensitive information and helps prevent licensing issues in different environments. Double-check that your license is valid and covers the intended platform. For migration tips, see [How do I migrate from wkhtmltopdf to IronPDF?](migrate-wkhtmltopdf-to-ironpdf.md).

---

## What Is the Best Way to Troubleshoot HTML-to-PDF Conversion Issues?

Here’s a quick checklist to resolve most conversion problems:

1. **Is your HTML valid?**  
   Paste it in a browser and check for errors.
2. **Are resource paths correct?**  
   Use absolute URLs or set `BaseUrl`.
3. **Is your JavaScript completing before render?**  
   Use `RenderDelay` or `WaitFor.Expression`.
4. **Are fonts loaded?**  
   Set a render delay or embed fonts as Base64.
5. **Are you using a modern renderer?**  
   Chromium-based engines provide the best compatibility.
6. **Deploying to Linux or Docker?**  
   Install required native packages.
7. **Is your license valid and correctly set?**
8. **Are you controlling parallelism?**  
   Limit concurrent renders to avoid memory issues.

For more troubleshooting strategies, check out [Convert HTML to PDF in C#](convert-html-to-pdf-csharp.md) and [IronPDF documentation](https://ironpdf.com).

---

## Where Can I Find More Resources or Get Help?

- For the latest on IronPDF, visit [IronPDF](https://ironpdf.com)
- Explore other developer tools from [Iron Software](https://ironsoftware.com)
- Dive deeper into advanced use cases: [Advanced HTML to PDF in C#](advanced-html-to-pdf-csharp.md)
- Learn why developers choose IronPDF: [Why do developers choose IronPDF?](why-developers-choose-ironpdf.md)
- Tackle base URL issues: [How do I use BaseUrl for HTML to PDF in C#?](base-url-html-to-pdf-csharp.md)
- Migration tips: [How do I migrate from wkhtmltopdf to IronPDF?](migrate-wkhtmltopdf-to-ironpdf.md)

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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/) is CTO at Iron Software, leading a team of 50+ engineers building .NET libraries downloaded over 41+ million times. With 41 years of coding experience, Jacob is passionate about developer experience and API design. Based in Chiang Mai, Thailand.
