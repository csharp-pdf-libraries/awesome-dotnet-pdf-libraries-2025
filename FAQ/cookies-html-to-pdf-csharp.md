# How Do I Handle Cookies for Authenticated HTML-to-PDF Conversion in C# with IronPDF?

Converting HTML—especially authenticated web pages—into PDFs in C# is a powerful workflow. But if you're getting login screens or generic content in your PDFs instead of the private dashboards and personalized data you expect, cookies are probably the missing link. This FAQ covers how to securely and reliably send cookies with IronPDF, handle complex authentication, troubleshoot common issues, and master edge cases for robust HTML-to-PDF automation.

---

## Why Do Cookies Matter When Converting Web Pages to PDF?

Whenever you use IronPDF (or any similar tool) to turn a live URL or [HTML into a PDF](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/), it's like launching a new, clean browser. If that process doesn't have the right cookies, it can't access protected or personalized content—just like a blank Chrome profile wouldn't be logged in.

### What Problems Do Missing Cookies Cause in PDF Generation?

If you don't supply the correct cookies:

- **Authenticated pages** will redirect you to a login screen.
- **User dashboards** won't show personalized info—just a sign-in prompt.
- **CSRF-protected forms** might display errors or load incompletely.
- **Dynamic content** intended for a logged-in user will appear generic.
- **APIs or resource endpoints** could return "401 Unauthorized" or empty data.

Essentially, any site using cookies for session, authentication, or customization will require you to pass those cookies when rendering PDFs.

For a broader look at HTML-to-PDF workflows, see [Convert Html To Pdf Csharp](convert-html-to-pdf-csharp.md).

---

## How Can I Pass Custom Cookies to IronPDF in C#?

IronPDF makes it straightforward to supply custom cookies for rendering. You just populate a dictionary with your cookie names and values, and IronPDF appends them to each request—exactly as your browser would.

```csharp
using IronPdf; // Install-Package IronPdf

var pdfGenerator = new ChromePdfRenderer();

// Define your authentication/session cookies here
pdfGenerator.RenderingOptions.CustomCookies = new Dictionary<string, string>
{
    { ".AspNetCore.Session", "example_session_token" },
    { "auth_token", "user_jwt_here" }
};

var resultPdf = pdfGenerator.RenderUrlAsPdf("https://yourdomain.com/dashboard");
resultPdf.SaveAs("dashboard.pdf");
```

**Tip:**  
Every HTTP request made during the render—including for images, CSS, or AJAX—will include these cookies. For more advanced HTML and PDF scenarios, check out [Advanced Html To Pdf Csharp](advanced-html-to-pdf-csharp.md).

---

## What If I Need to Send Multiple Cookies? Does the Order Matter?

No worries—you can add as many cookies as you need. The order of items in the dictionary is irrelevant; what matters is that you include every cookie expected by your application.

```csharp
using IronPdf; // Install-Package IronPdf

var pdfRenderer = new ChromePdfRenderer();

pdfRenderer.RenderingOptions.CustomCookies = new Dictionary<string, string>
{
    { "sessionId", "sess_ABC123" },
    { "csrf", "token_value" },
    { "theme", "dark" }
};

var pdf = pdfRenderer.RenderUrlAsPdf("https://example.com/profile");
pdf.SaveAs("profile.pdf");
```

Cookies can cover anything from authentication to UI preferences. Just make sure you spell the names and provide current values.

---

## How Do I Handle ASP.NET or ASP.NET Core Authentication Cookies?

ASP.NET apps typically use cookies with names like `.AspNetCore.Identity.Application`, `.AspNetCore.Cookies`, or `ASP.NET_SessionId`. These are critical for authenticated web apps.

### How Can I Find and Use the Right Cookies?

To identify which cookies to use, log into your web app in a browser, open DevTools (Network > Cookies), and copy the relevant cookie names and values.

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.CustomCookies = new Dictionary<string, string>
{
    { ".AspNetCore.Identity.Application", "real_auth_token" },
    { "ASP.NET_SessionId", "session_id" }
};

var pdf = renderer.RenderUrlAsPdf("https://myapp.com/admin/reports");
pdf.SaveAs("admin-report.pdf");
```

If you're generating the PDF inside your ASP.NET app, see the next section for automating cookie transfer.

For more about working with different authentication flows, see [Advanced Html To Pdf Csharp](advanced-html-to-pdf-csharp.md).

---

## Can I Automatically Use the User's Cookies from ASP.NET Core?

Absolutely! If you're inside an ASP.NET Core controller and want the PDF to reflect the current user's session, just copy cookies from the incoming request.

```csharp
using IronPdf;
using Microsoft.AspNetCore.Http;
// Install-Package IronPdf

public class PdfController : Controller
{
    public IActionResult DownloadDashboard()
    {
        var pdfRenderer = new ChromePdfRenderer();

        var cookiesToSend = new Dictionary<string, string>();
        foreach (var cookie in Request.Cookies)
        {
            cookiesToSend[cookie.Key] = cookie.Value;
        }

        pdfRenderer.RenderingOptions.CustomCookies = cookiesToSend;
        var pdf = pdfRenderer.RenderUrlAsPdf($"{Request.Scheme}://{Request.Host}/dashboard");
        return File(pdf.BinaryData, "application/pdf", "dashboard.pdf");
    }
}
```

This guarantees the PDF matches the logged-in user's view—hugely useful for personalized dashboards and multi-user reporting.

---

## How Do Request Contexts Affect Cookie Handling in IronPDF?

IronPDF uses "request contexts" to control how browser sessions and cookies persist between renders.

- **Isolated** (default): Starts with a fresh browser state every time (no previous cookies).
- **Global**: Shares cookies/session data across multiple renders (great for multi-step flows, like login then render).
- **Auto**: Lets IronPDF decide (usually defaults to Isolated).

### When Should I Choose Each Context?

- Use **Global** when you need to log in or maintain session state across multiple renders.
- Stick with **Isolated** for single, stateless renders where you don't want cookies to persist.

```csharp
using IronPdf; // Install-Package IronPdf

var pdfRenderer = new ChromePdfRenderer();
pdfRenderer.RenderingOptions.RequestContext = RequestContexts.Global; // or Isolated
```

For complex workflows (like logging in before rendering a report), Global context is a must. More on this in [Advanced Html To Pdf Csharp](advanced-html-to-pdf-csharp.md).

---

## How Can I Automate Logging In Before PDF Rendering?

You can script a login sequence by rendering the login page first (with credentials or cookies), then rendering the protected content with the same session.

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.RequestContext = RequestContexts.Global;

renderer.RenderUrlAsPdf("https://example.com/login"); // Optionally automate form submission as needed

var pdf = renderer.RenderUrlAsPdf("https://example.com/protected/dashboard");
pdf.SaveAs("protected-dashboard.pdf");
```

This lets IronPDF handle authentication cookies, redirects, and session management for you. For more tips on controlling workflows and session state, check [Convert Html To Pdf Csharp](convert-html-to-pdf-csharp.md).

---

## How Do I Pass JWT Bearer Tokens for Authenticated PDF Conversion?

Many APIs and SPAs use JWTs for authentication—either in cookies or HTTP headers.

### What If My JWT Is in a Cookie?

Just include it in your cookie dictionary:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.CustomCookies = new Dictionary<string, string>
{
    { "jwt", "eyJhbGciOiJIUzI1NiIsIn..." },
    { "refresh_token", "refresh_token_here" }
};

var pdf = renderer.RenderUrlAsPdf("https://api.example.com/reports/view");
pdf.SaveAs("jwt-cookie-report.pdf");
```

### What If the JWT Is Required in an HTTP Header?

Use the `CustomHttpHeaders` option:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.CustomHttpHeaders = new Dictionary<string, string>
{
    { "Authorization", "Bearer eyJhbGciOiJIUzI1NiIsIn..." }
};

var pdf = renderer.RenderUrlAsPdf("https://api.example.com/reports/view");
pdf.SaveAs("jwt-header-report.pdf");
```

**Caution:**  
JWTs often expire quickly. Always use valid, current tokens or you'll get unauthorized errors.

For more about PDF security and authentication, see [Understanding Pdf Security Net 10 Ironpdf](understanding-pdf-security-net-10-ironpdf.md).

---

## How Can I Debug When My PDF Shows a Login Page Instead of Content?

If your PDF isn't displaying the intended authenticated content, debugging is essential.

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.EnableJavaScript = true;
renderer.RenderingOptions.WaitFor.RenderDelay = 3000;

renderer.RenderingOptions.CustomCookies = new Dictionary<string, string>
{
    { "debug_session", "test_value" }
};

var pdf = renderer.RenderUrlAsPdf("https://secure.example.com/protected");

// Extract and inspect PDF text
var text = pdf.ExtractAllText();
Console.WriteLine(text.Substring(0, Math.Min(500, text.Length)));
```

Look for "Please log in" or similar clues. If you see login prompts, double-check:

- Cookie names and values (are they typo-free and current?)
- Cookie domain/path (must match the rendered URL)
- Whether cookies are expired or malformed
- Secure/HttpOnly settings

See also [Base Url Html To Pdf Csharp](base-url-html-to-pdf-csharp.md) if your app relies on relative URLs.

---

## How Should I Handle Secure and HttpOnly Cookies in IronPDF?

"Secure" cookies are only sent over HTTPS, and "HttpOnly" cookies can't be accessed by client-side scripts. Here's how IronPDF deals with them:

- **Secure cookies**: Only included for HTTPS requests. Make sure your URL starts with `https://`.
- **HttpOnly cookies**: Can be set programmatically in IronPDF, but you can't fetch them via JavaScript—use server-side code or browser dev tools.

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.CustomCookies = new Dictionary<string, string>
{
    { "secure_session", "secure_val" },     // Only for HTTPS
    { "httponly_auth", "httponly_val" }     // Settable in code
};

var pdf = renderer.RenderUrlAsPdf("https://secure.example.com/secret");
pdf.SaveAs("secure-content.pdf");
```

Don't attempt to read HttpOnly cookies from JavaScript—they're intentionally hidden for security.

---

## What If My Local HTML References Cookie-Protected Resources?

If your HTML points to images, CSS, or scripts hosted on secure servers or CDNs that require cookies, simply include those cookies in your CustomCookies dictionary. Every HTTP request IronPDF makes (including for those resources) will carry the cookies.

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();

renderer.RenderingOptions.CustomCookies = new Dictionary<string, string>
{
    { "cdn_auth", "cdn_token_value" }
};

string html = @"
<html>
  <body>
    <h1>My Report</h1>
    <img src='https://cdn.example.com/protected/chart.png' />
    <link rel='stylesheet' href='https://cdn.example.com/protected/styles.css' />
  </body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("report-with-assets.pdf");
```

If you need to control asset locations, learn more in [Base Url Html To Pdf Csharp](base-url-html-to-pdf-csharp.md).

---

## What Are Some Common Mistakes and Troubleshooting Tips for Cookies?

Even experienced devs can run into cookie headaches. Here's a checklist for troubleshooting:

- **Incorrect cookie names**: Watch out for case sensitivity, dots, dashes, and underscores.
- **Expired or invalid tokens**: Ensure your session/auth tokens are fresh.
- **Domain mismatches**: Cookie domain must match the URL you’re rendering (including subdomains).
- **HttpOnly cookies**: Retrieve these from server-side code, not JavaScript.
- **Secure cookies over HTTP**: Only send Secure cookies with HTTPS.
- **JavaScript-based authentication**: Set `EnableJavaScript = true` and provide enough `RenderDelay` for scripts to run.
- **Missing resources**: If images or styles don’t load, they might need cookies too.

**My debugging workflow:**
1. Extract text from the PDF to see what rendered (`pdf.ExtractAllText()`).
2. Compare with your browser’s output (using DevTools > Network > Cookies).
3. Try a public page first to rule out other rendering issues.
4. Use detailed logging if IronPDF supports it.

For advanced debugging (like WebGL rendering), check [Render Webgl Pdf Csharp](render-webgl-pdf-csharp.md).

---

## Quick Reference: How Do I Perform Common Cookie Tasks in IronPDF?

- **Add a single cookie**  
  ```csharp
  renderer.RenderingOptions.CustomCookies = new Dictionary<string, string> { { "key", "value" } };
  ```
- **Persist cookies globally**  
  ```csharp
  renderer.RenderingOptions.RequestContext = RequestContexts.Global;
  ```
- **Isolate session per render**  
  ```csharp
  renderer.RenderingOptions.RequestContext = RequestContexts.Isolated;
  ```
- **Copy all cookies from ASP.NET Core**  
  Loop through `Request.Cookies` and add to your dictionary.
- **Add JWT via HTTP header**  
  ```csharp
  renderer.RenderingOptions.CustomHttpHeaders = new Dictionary<string, string> { { "Authorization", "Bearer token" } };
  ```

For deeper dives and more edge cases, see [Advanced Html To Pdf Csharp](advanced-html-to-pdf-csharp.md).

---

## Where Can I Learn More About Cookies, Security, and IronPDF Features?

- See the official [IronPDF documentation](https://ironpdf.com/) for exhaustive API details.
- Explore [Iron Software](https://ironsoftware.com/) for other .NET document tools.
- For security and access control, read [Understanding Pdf Security Net 10 Ironpdf](understanding-pdf-security-net-10-ironpdf.md).
- For advanced HTML conversion, see [Advanced Html To Pdf Csharp](advanced-html-to-pdf-csharp.md).
- Handling base URLs and asset loading? [Base Url Html To Pdf Csharp](base-url-html-to-pdf-csharp.md) is your friend.
- For a full beginner-to-advanced guide, check [Convert Html To Pdf Csharp](convert-html-to-pdf-csharp.md).

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

Written by [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), Chief Technology Officer at Iron Software. Jacob started coding at age 6 and created IronPDF to solve real PDF challenges. Based in Chiang Mai, Thailand. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
