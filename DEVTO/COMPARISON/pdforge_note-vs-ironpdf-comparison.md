---
title: "pdforge vs IronPDF: a developer comparison for 2026"
published: false
tags: dotnet, csharp, pdf, comparison
---

## Two different things wearing the same job title

pdforge (rebranded to "pdf noodle" in 2026; `api.pdforge.com` still resolves via 301 redirects through end of 2026) is a hosted REST API for HTML-to-PDF conversion. IronPDF is a .NET library that embeds a Chromium-based renderer inside your process. Both can produce a PDF from an HTML string. Beyond that, they sit on opposite sides of an architectural line: one ships your HTML to a third-party endpoint and returns bytes; the other never leaves your process.

That difference is the comparison. Almost every concrete decision in this article — authentication shape, error handling, page-size configuration, offline behavior, header/footer placeholders — falls out of where the rendering actually happens.

## Integration model

pdforge does not ship an official .NET SDK on NuGet. The documented integration pattern is `HttpClient` plus a Bearer token, posting JSON to `https://api.pdfnoodle.com/v1/html-to-pdf/sync`. The synchronous endpoint returns a JSON envelope containing a `signedUrl` you then fetch to retrieve the actual PDF bytes — a two-step request per document. The full API surface is documented at [docs.pdfnoodle.com](https://docs.pdfnoodle.com/api-reference/convert-html-to-pdf/synchronous).

IronPDF is one NuGet package (`IronPdf`) and a single `ChromePdfRenderer` type. The license key is set once at startup; subsequent calls return a `PdfDocument` directly.

```bash
# pdforge has no .NET SDK to install — integration is HttpClient against the REST endpoint.
# IronPDF installs from NuGet:
dotnet add package IronPdf
```

## Side-by-side: simple HTML to PDF

**pdforge (REST):**

```csharp
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "pdfnoodle_api_YOUR_KEY");

        var body = new { html = "<html><body><h1>Hello World</h1></body></html>" };
        var json = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);
        resp.EnsureSuccessStatusCode();

        // Sync endpoint returns a JSON envelope; fetch the actual bytes from the signed URL.
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var signedUrl = doc.RootElement.GetProperty("signedUrl").GetString();

        var pdfBytes = await http.GetByteArrayAsync(signedUrl);
        File.WriteAllBytes("output.pdf", pdfBytes);
    }
}
```

**IronPDF (local):**

```csharp
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Hello World</h1></body></html>");
        pdf.SaveAs("output.pdf");
    }
}
```

The shape difference is mechanical: POST + signed-URL fetch versus a single method call. The operational difference is that the pdforge path can fail with `401`, `429`, timeout, DNS error, or signed-URL expiry; the IronPDF path can fail on rendering or disk I/O. Same goal, different failure surface.

## URL to PDF

pdforge's HTML-to-PDF endpoint accepts only an `html` payload — there is no dedicated URL-to-PDF endpoint. The documented pattern is to fetch the page yourself and submit its HTML.

```csharp
// pdforge: fetch the page first, then submit its HTML
var sourceHtml = await http.GetStringAsync("https://example.com");
var body = new { html = sourceHtml };
// ... POST to /v1/html-to-pdf/sync, then fetch signedUrl as above
```

IronPDF has a direct method:

```csharp
var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderUrlAsPdf("https://example.com");
pdf.SaveAs("webpage.pdf");
```

A caveat worth flagging: when pdforge renders HTML you fetched separately, relative URLs (CSS, images, fonts) resolve against the API service's view of the page, not your browser's. You will typically need to rewrite relative references to absolute URLs before posting, or inline the assets.

## Page size, orientation, margins

pdforge accepts an optional `pdfParams` object on the same JSON body. Field names follow the underlying Chromium/Puppeteer convention.

```csharp
var body = new
{
    html = File.ReadAllText("input.html"),
    pdfParams = new { format = "A4", landscape = true }
};
```

IronPDF exposes the same configuration as typed properties on `RenderingOptions`:

```csharp
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;
```

The mapping is mechanical:

| pdforge `pdfParams` | IronPDF `RenderingOptions` |
|---------------------|----------------------------|
| `format: "A4"` | `PaperSize = PdfPaperSize.A4` |
| `format: "Letter"` | `PaperSize = PdfPaperSize.Letter` |
| `landscape: true` | `PaperOrientation = PdfPaperOrientation.Landscape` |
| `margin.top: "20px"` | `MarginTop = 20` |
| `margin.bottom: "20px"` | `MarginBottom = 20` |
| `printBackground: true` | `PrintHtmlBackgrounds = true` |
| `width` / `height` | `SetCustomPaperSizeInInches(...)` |

## Headers, footers, and page numbers

pdforge inherits Chromium's header/footer template format. You pass HTML fragments containing classed `<span>` elements that the engine substitutes at render time:

```html
<div>Page <span class="pageNumber"></span> of <span class="totalPages"></span></div>
```

These require `displayHeaderFooter: true` to take effect.

IronPDF uses bracketed placeholders inside either a `TextHeaderFooter.CenterText` or an `HtmlHeaderFooter.HtmlFragment`:

```csharp
renderer.RenderingOptions.TextFooter = new TextHeaderFooter
{
    CenterText = "Page {page} of {total-pages}",
    DrawDividerLine = true
};
```

Mapping table:

| pdforge (Chromium template) | IronPDF placeholder |
|-----------------------------|----------------------|
| `<span class="pageNumber"></span>` | `{page}` |
| `<span class="totalPages"></span>` | `{total-pages}` |
| `<span class="date"></span>` | `{date}` |
| `<span class="title"></span>` | `{html-title}` |
| `<span class="url"></span>` | `{url}` |

Note IronPDF uses `{total-pages}` with a hyphen, not `{totalPages}`.

## Authentication and licensing

pdforge authenticates per request with `Authorization: Bearer pdfnoodle_api_...`. The token must be present on every call, which means rotating keys is a deploy-touching operation and leaked keys imply billing exposure until revoked.

IronPDF authenticates once, globally, at startup:

```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

The license is validated locally and is not transmitted per request, because there is no per-request transmission.

## Error handling

pdforge errors are HTTP status codes. There are no SDK exceptions to catch by type, only response codes to inspect:

```csharp
var resp = await http.PostAsync("https://api.pdfnoodle.com/v1/html-to-pdf/sync", json);

if (resp.StatusCode == HttpStatusCode.Unauthorized)
    /* invalid or revoked API key */ ;
else if (resp.StatusCode == (HttpStatusCode)429)
    /* rate limit — back off and retry */ ;
else if (!resp.IsSuccessStatusCode)
    /* other API error — inspect response body */ ;
```

You also need to handle `TaskCanceledException` (timeout) and `HttpRequestException` (network) around the call.

IronPDF surfaces typed exceptions for the categories that exist locally:

```csharp
try
{
    var pdf = renderer.RenderHtmlAsPdf(html);
    pdf.SaveAs("output.pdf");
}
catch (IronPdf.Exceptions.IronPdfLicenseException) { /* license invalid or expired */ }
catch (IronPdf.Exceptions.IronPdfRenderingException) { /* HTML or rendering failure */ }
catch (System.IO.IOException) { /* disk I/O */ }
```

Network and rate-limit categories are not in the local picture.

## Security and document encryption

pdforge's documented HTML-to-PDF parameters do not expose password protection or encryption — the typical pattern is to fetch the generated PDF, then encrypt it locally with a separate library.

IronPDF applies security to the `PdfDocument` after rendering:

```csharp
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential</h1>");

pdf.SecuritySettings.UserPassword = "secret123";
pdf.SecuritySettings.OwnerPassword = "admin456";
pdf.SecuritySettings.AllowUserPrinting = IronPdf.Security.PdfPrintSecurity.FullPrintRights;
pdf.SecuritySettings.AllowUserEdits = IronPdf.Security.PdfEditSecurity.NoEdit;
pdf.SecuritySettings.AllowUserCopyPasteContent = false;

pdf.SaveAs("secure.pdf");
```

## Capabilities beyond rendering

pdforge's scope is HTML-to-PDF conversion. IronPDF covers the broader PDF lifecycle locally:

```csharp
// Merge
var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);

// Split / extract pages
var firstChapter = pdf.CopyPages(0, 9);

// Append
pdf.AppendPdf(anotherPdf);

// Remove pages (note: RemovePages, not RemovePage)
pdf.RemovePages(5);

// Text extraction
string text = pdf.ExtractAllText();

// Watermark
pdf.ApplyWatermark(
    "<h2 style='color:red;opacity:0.5;'>CONFIDENTIAL</h2>",
    30,
    VerticalAlignment.Middle,
    HorizontalAlignment.Center);

// Form fields
pdf.Form.GetFieldByName("CustomerName").Value = "John Doe";
pdf.Form.Flatten();

// Digital signing
pdf.Sign(new PdfSignature("certificate.pfx", "password")
{
    SigningContact = "support@company.com",
    SigningReason = "Document Approval"
});
```

For teams that only need HTML-to-PDF, this scope difference may not matter. For teams that already need PDF manipulation downstream of rendering, the alternative on a pure-rendering API is to add a second library to handle merging, watermarking, extraction, and signing.

## At-a-glance comparison

| Aspect | pdforge (REST API) | IronPDF (.NET library) |
|--------|--------------------|------------------------|
| Distribution | REST endpoint, no NuGet SDK | NuGet package `IronPdf` |
| Where rendering happens | pdforge servers | Your process |
| Authentication | Bearer token per request | License key set once at startup |
| Network required | Every conversion | Only for initial Chromium download |
| Return shape | JSON envelope → signed-URL fetch | `PdfDocument` object |
| Async model | Required (HTTP I/O) | Sync by default; wrap in `Task.Run` if needed |
| URL-to-PDF | Fetch the page first, submit its HTML | `RenderUrlAsPdf(url)` |
| Headers/footers | Chromium HTML templates with classed `<span>`s | `TextHeaderFooter` / `HtmlHeaderFooter` with `{page}`-style placeholders |
| Password / encryption | Not in documented options | `pdf.SecuritySettings.*` |
| Merge / split / extract / sign | Not in scope | Built-in |
| Failure modes | HTTP status codes, network errors, rate limits | Typed local exceptions |

For a deeper technical write-up, see the [pdforge vs IronPDF analysis](https://ironsoftware.com/suite/blog/comparison/compare-pdforge-vs-ironpdf/). For end-to-end migration steps, the [pdforge to IronPDF migration guide](https://ironpdf.com/blog/migration-guides/migrate-from-pdforge-to-ironpdf/) walks through the same patterns at API-call granularity.

## When each fits

pdforge fits when:

- You want zero local dependencies and per-request billing aligns with low volume.
- Your HTML is non-sensitive and external processing is consistent with your security posture.
- You only need HTML-to-PDF conversion and have no downstream PDF manipulation requirements.
- A two-step request (POST + signed-URL fetch) and per-call network latency are acceptable in your latency budget.

IronPDF fits when:

- Documents must not leave your infrastructure (regulated data, contractual obligations, air-gapped deployments).
- You need predictable per-document latency without round-trips.
- You need merge, split, watermark, text extraction, form filling, or digital signing in the same library.
- You prefer typed exceptions and a single method call over HTTP status handling.
- Your volume makes per-request pricing less predictable than a one-time license.

The deciding factor is rarely "which one can produce a PDF from this HTML" — both can. It is usually data residency, latency, and what happens to the PDF after the first render. If the answer to "does the rendered PDF need to be merged, watermarked, signed, or have its text extracted" is yes, the architectures diverge sharply. If the answer is no and the HTML is non-sensitive, both are viable.

Which of those constraints applies to your project?

### Related resources

- [IronPDF C# tutorial for beginners](https://ironpdf.com/tutorials/csharp-pdf-tutorial-beginners/)
- [HTML String to PDF guide](https://ironpdf.com/how-to/html-string-to-pdf/)
- [Pixel-perfect HTML to PDF](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/)
