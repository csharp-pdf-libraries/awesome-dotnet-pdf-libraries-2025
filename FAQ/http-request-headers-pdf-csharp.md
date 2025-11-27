# How Can I Set Custom HTTP Request Headers When Generating PDFs from Web Pages in C#?

If you need to create PDFs from web pages that require authentication, tokens, or other special headers, you’ll quickly run into issues without custom HTTP headers. With IronPDF, you can set these headers easily and ensure your PDF output matches what an authenticated user would see in the browser.

Below, you'll find answers to common developer questions about configuring HTTP request headers for PDF rendering, including practical code samples and troubleshooting advice.

---

## Why Would I Need to Set Custom HTTP Headers When Creating PDFs in C#?

Most modern web apps expect specific headers—such as authentication tokens, cookies, or user-agent strings—incoming with requests. If these aren’t present, you’ll likely get a login page, missing assets, or errors instead of the content you expect in your PDF.

Common scenarios where custom headers are essential include:

- **Authentication:** Passing JWT/Bearer tokens, API keys, or SSO cookies for protected resources.
- **Session Management:** Maintaining user sessions to access dashboards or user-specific data.
- **User-Agent Spoofing:** Some servers block headless browsers unless you mimic a real browser.
- **Proxy/Custom Routing:** Certain networks require headers for proxy authentication.
- **Localization/Negotiation:** Specifying language or desired content types.

For instance, when generating reports from internal dashboards, our team relies on custom headers to ensure we get the correct, secure content. For more on why IronPDF is a top tool for these workflows, see [Why do developers choose IronPDF?](why-developers-choose-ironpdf.md)

---

## How Do I Set HTTP Headers When Using IronPDF’s ChromePdfRenderer?

With IronPDF, you can easily specify headers using the `RenderingOptions.HttpRequestHeaders` property. This applies your headers to every HTTP request—both for the main page and embedded resources like images and CSS.

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HttpRequestHeaders = new Dictionary<string, string>
{
    { "Authorization", "Bearer YOUR_API_TOKEN" },
    { "User-Agent", "MyCustomBot/1.0" },
    { "Accept-Language", "en-US,en;q=0.8" }
};

var pdf = renderer.RenderUrlAsPdf("https://secure.example.com/dashboard");
pdf.SaveAs("dashboard.pdf");
```

This approach works for authentication, cookies, language selection, and more. For broader PDF customization like headers and footers, see [How do I add headers and footers to PDFs in C#?](pdf-headers-footers-csharp.md)

---

## What Are Some Typical Scenarios Where Headers Are Needed?

### How Do I Generate PDFs from Authenticated Dashboards?

If your content is behind SSO or login, you’ll need to pass session cookies or tokens. For example:

```csharp
renderer.RenderingOptions.HttpRequestHeaders = new Dictionary<string, string>
{
    { "Cookie", "sessionid=xyz123; sso_token=tokenvalue" }
};
```

### How Can I Use API Tokens or Bearer Tokens for PDF Generation?

Many APIs require a Bearer token in the `Authorization` header:

```csharp
renderer.RenderingOptions.HttpRequestHeaders = new Dictionary<string, string>
{
    { "Authorization", "Bearer abc.def.ghi" }
};
```

### How Do I Avoid Bot Detection with Custom User-Agents?

If a site blocks bots, mimic a real browser:

```csharp
renderer.RenderingOptions.HttpRequestHeaders = new Dictionary<string, string>
{
    { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0 Safari/537.36" }
};
```

### What About Proxy Authentication or Custom Routing?

Some networks require proxy credentials via headers:

```csharp
renderer.RenderingOptions.HttpRequestHeaders = new Dictionary<string, string>
{
    { "Proxy-Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("user:password")) }
};
```

For more complex PDF workflows—like attaching files—see [How can I add attachments to PDFs in C#?](add-attachments-pdf-csharp.md)

---

## Which Common HTTP Headers Should I Consider for PDF Rendering?

Here’s a handy list of headers with their common uses:

| Header            | Example Value                  | When to Use                |
|-------------------|-------------------------------|----------------------------|
| Authorization     | `Bearer <token>`              | Authentication             |
| Cookie            | `sessionid=abc; token=xyz`    | Session persistence        |
| User-Agent        | Browser UA string              | Bot avoidance              |
| Accept            | `text/html,...`               | Content negotiation        |
| Referer           | `https://myapp.com`           | Some resource restrictions |
| X-API-Key         | `123apikey`                   | API authentication         |
| Accept-Language   | `en-US,en;q=0.9`              | Language preference        |
| Origin            | `https://frontend.com`         | CORS & cross-origin cases  |

Tip: Use your browser’s Network tools to inspect which headers are sent for your use case.

---

## How Can I Debug or Test Which Headers Are Actually Being Sent?

If you’re not sure your headers are working, try rendering a PDF of [httpbin.org/headers](https://httpbin.org/headers). This service echoes all headers it receives:

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HttpRequestHeaders = new Dictionary<string, string>
{
    { "X-Debug-Header", "Test123" },
    { "Authorization", "Bearer testtoken" }
};

var pdf = renderer.RenderUrlAsPdf("https://httpbin.org/headers");
pdf.SaveAs("debug-headers.pdf");
```

Open the PDF and check what the server received.

---

## What’s the Best Way to Handle Different Headers for Different PDFs?

Because headers are set per `ChromePdfRenderer` instance, use separate instances if different header sets are required:

```csharp
using IronPdf;

var renderer1 = new ChromePdfRenderer();
renderer1.RenderingOptions.HttpRequestHeaders = new Dictionary<string, string>
{
    { "Authorization", "Bearer token1" }
};

var renderer2 = new ChromePdfRenderer();
renderer2.RenderingOptions.HttpRequestHeaders = new Dictionary<string, string>
{
    { "Authorization", "Bearer token2" }
};

renderer1.RenderUrlAsPdf("https://service1.example.com").SaveAs("out1.pdf");
renderer2.RenderUrlAsPdf("https://service2.example.com").SaveAs("out2.pdf");
```

---

## Are Headers Applied to Embedded Resources (CSS, JS, Images) Too?

Yes, IronPDF automatically sends the headers you set with every HTTP request during rendering, not just the main page. This prevents missing styles or images due to authentication issues. For more advanced PDF object manipulation, see [How do I access the PDF DOM object in C#?](access-pdf-dom-object-csharp.md)

---

## How Do I Deal with HTTP Basic Authentication?

Legacy APIs may require Basic Auth. Here’s how to encode credentials:

```csharp
using IronPdf;

var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("user:pass"));
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HttpRequestHeaders = new Dictionary<string, string>
{
    { "Authorization", $"Basic {credentials}" }
};

renderer.RenderUrlAsPdf("https://legacy.example.com/protected").SaveAs("legacy.pdf");
```

---

## What Are Common Pitfalls When Setting HTTP Headers in IronPDF?

- **Typos in Header Names:** Headers are case-sensitive and must match exactly.
- **Invalid or Expired Tokens:** Always verify your tokens are valid before use.
- **Missing Headers for Embedded Resources:** Make sure headers cover all assets.
- **Headers Not Set at the Right Time:** Always assign headers before calling `RenderUrlAsPdf`.
- **Login Pages in Output:** Double-check that session cookies or tokens are correct.
- **Server Blocking Requests:** If a server blocks your request, try copying browser headers exactly.

If you need to troubleshoot further, use tools like httpbin, Fiddler, or reach out for help on the IronPDF [documentation](https://ironpdf.com) or through [Iron Software](https://ironsoftware.com).

---

*Have more questions? Find [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) on the Iron Software team page. As CTO, Jacob leads development of IronPDF and the Iron Suite.*
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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/) serves as CTO at [Iron Software](https://ironsoftware.com/about-us/authors/jacobmellor/), creator of IronPDF and leader of a globally distributed engineering team. Started coding at age 6 and never stopped. With 25+ years of commercial development experience, he continues to push the boundaries of .NET PDF technology from Chiang Mai, Thailand.
