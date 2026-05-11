---
title: "PuppeteerSharp vs IronPDF: an unbiased look for .NET teams"
published: false
tags: dotnet, csharp, pdf, comparison
---

Two libraries, one tagline: "Chrome-quality PDFs from .NET." The architectures behind that tagline are very different. PuppeteerSharp (MIT) is the .NET port of Google's Puppeteer — it downloads a Chromium build at runtime and drives it as an external process via the DevTools Protocol. IronPDF embeds a Chromium rendering engine inside the library and exposes a PDF-focused API. Both render with Chromium; the operational shapes diverge sharply once you push them into CI, containers, or sustained load.

This comparison exists because teams often evaluate "Chromium-based PDF generation" as a single category. PuppeteerSharp launches headless Chrome as an external process and controls it via the DevTools Protocol. IronPDF embeds the Chromium rendering engine as a library component. Both produce Chrome-quality PDFs, but PuppeteerSharp's process-based model introduces operational concerns around process lifecycle, resource cleanup, binary management, and Linux dependencies that an in-process engine does not have. This guide focuses on the operational differences, common troubleshooting scenarios, and tradeoffs between process-based and library-based approaches.

**Canonical/source version:** This article is also available at [IronPDF's comparison guides](https://ironpdf.com/).

## Understanding IronPDF

IronPDF embeds a Chromium rendering engine as a .NET library component—no external processes, no browser binaries to manage, no DevTools Protocol complexity. The library runs in-process with your application, handles all resource management internally, and works identically across Windows, Linux, and macOS. You call `ChromePdfRenderer.RenderHtmlAsPdf()`, and IronPDF handles the entire rendering pipeline: HTML parsing, CSS layout, JavaScript execution, and PDF generation. No browser lifecycle management required.

IronPDF supports .NET Core, .NET 5+, and .NET Framework 4.6.2+. Because it's a library, not a process wrapper, it integrates naturally with ASP.NET, Azure Functions, AWS Lambda, and containerized deployments. No X-server requirements on Linux, no Chrome binary downloads in CI pipelines, no process cleanup edge cases. See the [HTML to PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) for implementation patterns.

## Key Limitations of PuppeteerSharp

### Product Status
PuppeteerSharp is actively maintained under the MIT license. It's a .NET port of the Node.js Puppeteer library, developed by the community with sponsorship support, and it tracks upstream Puppeteer changes. The project has steady adoption on GitHub. Its design center is browser automation; PDF export is one capability among many rather than the primary focus.

### Missing Capabilities
**No embedded rendering engine:** PuppeteerSharp requires downloading and managing a Chromium binary separately. On first run, `BrowserFetcher` fetches a Chromium build (typically a few hundred MB) onto the file system. In CI/CD pipelines, this can cause timeouts. In restricted environments (network-isolated servers, read-only file systems), it may fail entirely unless the binary is pre-staged.

**No simplified PDF-focused API:** PuppeteerSharp exposes the full browser automation API (navigate, click, type, screenshot). For teams that only need "HTML to PDF," this is complexity overhead. You must manage browser lifecycle, page objects, navigation events, and resource cleanup manually.

**No built-in PDF manipulation:** PuppeteerSharp generates PDFs but does not merge them, split them, extract text, add watermarks, apply digital signatures, or fill forms. You need additional libraries (IronPDF, iText, PdfSharp) for post-generation operations.

### Technical Considerations
**Headless dependencies on Linux:** In Docker containers, headless Chrome typically needs either `xvfb` (X virtual framebuffer) or the `--no-sandbox` flag plus related arguments. `--no-sandbox` removes a layer of process isolation and should be weighed carefully when untrusted input can reach the browser. "Cannot open display" errors are a common first-time-in-Docker symptom.

**Process lifecycle:** Browser and page instances must be explicitly disposed. Missing `await browser.DisposeAsync()` or skipping `await using` can leave Chrome processes resident, consuming memory and file handles. Long-running services need cleanup discipline.

**Timeout and wait tuning:** Page loads with slow external resources (fonts, images, analytics scripts) can stall PDF generation. PuppeteerSharp exposes explicit timeout and navigation wait strategies; defaults may need tuning for production sites.

**Memory pressure under sustained load:** Each `Page` and `Browser` object holds significant resources. Missed disposal in nested async paths can accumulate over time in long-running applications.

### Support Status
PuppeteerSharp is community-maintained, with GitHub issues as the primary channel. Complex debugging can require some familiarity with the Chrome DevTools Protocol. Paid support is via sponsorship tiers rather than vendor SLAs; production troubleshooting is largely self-service via GitHub discussions and Stack Overflow.

### Architectural Tradeoffs
**Binary management:** Chrome binaries are platform-specific and version-tied. PuppeteerSharp upgrades may require fetching a new Chromium build. In air-gapped or restricted networks, this typically means pre-staging the binary and pinning the revision.

**Thread-safety model:** Browser instances are not designed to be shared freely across threads. High-concurrency PDF workloads usually need either per-request browser instances or a browser pool with explicit synchronization.

**DevTools Protocol surface:** Some error messages reference internal DevTools Protocol details (e.g., `Protocol error (Target.closeTarget): Target closed`) that are easier to read once you've encountered Puppeteer's multi-process architecture.

## Feature Comparison Overview

| Category | PuppeteerSharp | IronPDF |
|----------|----------------|---------|
| **Current Status** | Actively maintained, MIT-licensed, community-driven | Actively maintained, commercial with 30-day trial |
| **HTML Support** | Full Chromium browser automation (navigate, interact, PDF export) | Chromium HTML5/CSS3/JS rendering with a PDF-focused API |
| **Rendering Quality** | Chromium engine | Chromium engine (embedded) |
| **Installation** | NuGet package + Chromium binary download at runtime | NuGet package with embedded engine |
| **Support** | Community support, GitHub issues, sponsorship tiers | Commercial support, live chat |
| **Future Viability** | Community-maintained, tracks upstream Puppeteer | Commercial backing, enterprise SLAs available |

---

## Code Comparison: Basic HTML to PDF

### PuppeteerSharp — Browser Automation Approach

PuppeteerSharp requires explicit browser lifecycle management. Here's a complete example with proper disposal:

```csharp
using PuppeteerSharp;
using System;
using System.IO;
using System.Threading.Tasks;

public class PuppeteerPdfGenerator
{
    public static async Task GenerateBasicPdf()
    {
        // Fetches a Chromium build on first run if one is not already present
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        // Launch headless Chrome as an external process
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" } // Commonly needed inside containers
        });

        // Create a new page (a fresh browser context)
        await using var page = await browser.NewPageAsync();

        // Set content and wait for resources
        await page.SetContentAsync(@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; margin: 40px; }
                    h1 { color: #333; }
                </style>
            </head>
            <body>
                <h1>Sample Report</h1>
                <p>This is a basic HTML to PDF example.</p>
            </body>
            </html>
        ");

        // Wait for fonts to load (required for accurate rendering)
        await page.EvaluateExpressionHandleAsync("document.fonts.ready");

        // Generate PDF
        await page.PdfAsync("puppeteersharp-basic.pdf", new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "20px",
                Bottom = "20px",
                Left = "20px",
                Right = "20px"
            }
        });

        // Browser and page are disposed via await using
    }
}

// Usage
await PuppeteerPdfGenerator.GenerateBasicPdf();
```

**Troubleshooting points in this code:**
- **Binary download can time out:** The `DownloadAsync()` call fetches a Chromium build (typically a few hundred MB). In CI/CD or restricted networks, this can fail. Workarounds: pre-download and bundle Chromium, or use a Docker image with Chromium pre-installed.
- **`--no-sandbox` tradeoff:** Often needed inside containers without configured user namespaces. It removes a sandbox layer; evaluate carefully if untrusted input can reach the browser.
- **Font loading timing:** Without an explicit `document.fonts.ready` wait, PDFs may render with fallback fonts.
- **Process cleanup:** Skipping `await using` or `Dispose()` can leave Chrome processes resident. Each instance holds non-trivial memory and file handles.
- **Stalling on external resources:** Slow CDNs or unavailable images can cause `SetContentAsync()` or navigation to wait at the default timeout — set explicit timeouts.
- **Linux headless setup:** Without `xvfb` or appropriate Chrome flags, the launch typically fails with "cannot open display".

### IronPDF — Embedded Rendering Approach

IronPDF handles all resource management internally. No browser lifecycle, no process cleanup, no binary downloads:

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// Generate PDF from HTML string (no separate browser process, no external binaries)
var renderer = new ChromePdfRenderer();
using var pdf = renderer.RenderHtmlAsPdf(@"
    <!DOCTYPE html>
    <html>
    <head>
        <style>
            body { font-family: Arial, sans-serif; margin: 40px; }
            h1 { color: #333; }
        </style>
    </head>
    <body>
        <h1>Sample Report</h1>
        <p>This is a basic HTML to PDF example.</p>
    </body>
    </html>
");

pdf.SaveAs("ironpdf-basic.pdf");
```

IronPDF's embedded engine handles fonts, resources, and rendering internally. No `--no-sandbox` flag, no X-server setup, no separate process lifecycle. The `using` statement ensures the PDF document is disposed when the scope exits. See [HTML string to PDF guide](https://ironpdf.com/how-to/html-string-to-pdf/) for advanced scenarios.

---

## Code Comparison: Rendering URLs with External Resources

### PuppeteerSharp — Navigation and Wait Strategies

Rendering URLs with PuppeteerSharp requires careful timeout and wait condition configuration:

```csharp
using PuppeteerSharp;
using System;
using System.Threading.Tasks;

public class PuppeteerUrlRenderer
{
    public static async Task RenderUrlWithWait()
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] 
            { 
                "--no-sandbox", 
                "--disable-setuid-sandbox",
                "--disable-dev-shm-usage" // Prevent shared memory issues in containers
            },
            Timeout = 120000 // 2-minute launch timeout
        });

        await using var page = await browser.NewPageAsync();

        // Set longer timeout for navigation
        page.DefaultNavigationTimeout = 60000; // 60 seconds

        try
        {
            // Navigate and wait for network idle (all resources loaded)
            await page.GoToAsync("https://example.com/report", new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }, // Wait for network idle
                Timeout = 60000
            });

            // Wait for specific elements if needed
            await page.WaitForSelectorAsync("body", new WaitForSelectorOptions
            {
                Timeout = 10000
            });

            // Additional wait for fonts and images
            await page.EvaluateExpressionHandleAsync("document.fonts.ready");
            await Task.Delay(500); // Additional buffer for images

            // Generate PDF
            await page.PdfAsync("puppeteersharp-url.pdf", new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                PreferCSSPageSize = true,
                MarginOptions = new MarginOptions
                {
                    Top = "20px",
                    Bottom = "20px",
                    Left = "20px",
                    Right = "20px"
                }
            });
        }
        catch (NavigationException ex)
        {
            Console.WriteLine($"Navigation failed: {ex.Message}");
            // Handle timeout gracefully
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"Operation timed out: {ex.Message}");
            // Handle timeout gracefully
        }
    }
}

await PuppeteerUrlRenderer.RenderUrlWithWait();
```

**Common troubleshooting scenarios:**
- **Navigation timeout:** The default 30-second navigation timeout can be too low for real-world sites with analytics, ads, or slow CDNs. Configure timeouts explicitly.
- **Networkidle0 vs Networkidle2:** `Networkidle0` waits for zero in-flight connections for 500ms; `Networkidle2` waits for ≤2 connections. Pick deliberately — the wrong one renders too early or stalls.
- **Long-running third-party scripts:** Analytics and tracking pixels can keep "network idle" from ever firing. Either intercept and abort those requests, or wait on `DOMContentLoaded` instead.
- **Container shared memory:** `--disable-dev-shm-usage` is commonly needed because `/dev/shm` is small inside many container runtimes.
- **HTTPS certificates:** Self-signed or expired certificates cause navigation failures unless `IgnoreHTTPSErrors = true` is set in launch options.
- **Font loading timing:** Even after network idle, web fonts may still be applying. Wait on `document.fonts.ready` before generating the PDF.

### IronPDF — Simplified URL Rendering

IronPDF handles navigation, waiting, and resource loading internally:

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

// Configure wait conditions if needed
renderer.RenderingOptions.Timeout = 60; // seconds
renderer.RenderingOptions.WaitFor.RenderDelay = 500; // milliseconds after load

// Render URL to PDF (handles navigation, resources, fonts internally)
using var pdf = renderer.RenderUrlAsPdf("https://example.com/report");
pdf.SaveAs("ironpdf-url.pdf");
```

IronPDF's embedded engine manages the full rendering pipeline. No navigation strategies, no process lifecycle, no DevTools Protocol complexity. For advanced wait conditions, see [JavaScript and render delays](https://ironpdf.com/how-to/html-file-to-pdf/).

---

## Code Comparison: Handling Authentication

### PuppeteerSharp — Cookie and Header Management

Authentication with PuppeteerSharp requires manual cookie/header setup:

```csharp
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PuppeteerAuthRenderer
{
    public static async Task RenderWithAuth()
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        await using var page = await browser.NewPageAsync();

        // Set authentication cookie
        await page.SetCookieAsync(new CookieParam
        {
            Name = "auth_token",
            Value = "your-token-here",
            Domain = "example.com",
            Path = "/",
            HttpOnly = true,
            Secure = true
        });

        // Set custom headers (e.g., Bearer token)
        await page.SetExtraHttpHeadersAsync(new Dictionary<string, string>
        {
            { "Authorization", "Bearer your-bearer-token" },
            { "Custom-Header", "custom-value" }
        });

        // Navigate with authentication
        await page.GoToAsync("https://example.com/secure-report", new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
            Timeout = 60000
        });

        await page.PdfAsync("puppeteersharp-auth.pdf", new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true
        });
    }
}

await PuppeteerAuthRenderer.RenderWithAuth();
```

**Troubleshooting considerations:**
- **Cookie domain must match:** Setting cookies with wrong domain silently fails. Must match exact domain from URL.
- **Secure flag issues:** Cookies with `Secure = true` only work over HTTPS. HTTP URLs silently ignore them.
- **Header persistence:** Extra headers only apply to navigation request, not subsequent resource loads. May need to intercept and modify resource requests.
- **Session management complexity:** For complex auth flows (OAuth, multi-step logins), must script full browser interaction including form fills, clicks, and redirects.
- **Cookie expiration:** Must handle cookie expiration and renewal logic in application code.

### IronPDF — Authentication in Rendering Options

IronPDF handles authentication through rendering options:

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();

// Set authentication credentials on the renderer
renderer.RenderingOptions.CustomCookies.Add("auth_token", "your-token-here");
renderer.RenderingOptions.CustomHttpHeaders.Add("Authorization", "Bearer your-bearer-token");

using var pdf = renderer.RenderUrlAsPdf("https://example.com/secure-report");
pdf.SaveAs("ironpdf-auth.pdf");
```

IronPDF's rendering options handle authentication configuration without manual browser scripting. See [HTML to PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) for comprehensive authentication examples.

---

## API Mapping Reference

| Operation | PuppeteerSharp API | IronPDF API |
|-----------|-------------------|-------------|
| **Download Chrome binary** | `BrowserFetcher.DownloadAsync()` | Not required (embedded engine) |
| **Launch browser** | `Puppeteer.LaunchAsync()` | Not required (library-based) |
| **Create page** | `browser.NewPageAsync()` | Not required (handled internally) |
| **Render HTML string** | `page.SetContentAsync()` + `page.PdfAsync()` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| **Render URL** | `page.GoToAsync()` + `page.PdfAsync()` | `ChromePdfRenderer.RenderUrlAsPdf()` |
| **Render HTML file** | `page.GoToAsync("file://...")` + `page.PdfAsync()` | `ChromePdfRenderer.RenderHtmlFileAsPdf()` |
| **Set page size** | `PdfOptions.Format = PaperFormat.A4` | `RenderingOptions.PaperSize = PdfPaperSize.A4` |
| **Set margins** | `PdfOptions.MarginOptions` | `RenderingOptions.MarginTop/Bottom/Left/Right` |
| **Wait for network idle** | `NavigationOptions.WaitUntil = Networkidle0` | `RenderingOptions.WaitFor.RenderDelay` (or `WaitFor.HtmlElementId` / `WaitFor.JavaScript`) |
| **Set authentication** | `page.SetCookieAsync()` / `page.SetExtraHttpHeadersAsync()` | `RenderingOptions.CustomCookies` / `CustomHttpHeaders` |
| **Handle HTTPS errors** | `LaunchOptions.IgnoreHTTPSErrors = true` | Typically handled by the embedded engine — verify against your version |
| **Merge PDFs** | Not supported (requires external library) | `PdfDocument.Merge()` |
| **Extract text** | Not supported (requires external library) | `pdf.ExtractAllText()` |
| **Add watermark/stamp** | Not supported (requires external library) | `pdf.ApplyStamp(stamper)` |
| **Digital signature** | Not supported (requires external library) | `pdf.Sign(signature)` |
| **Dispose resources** | `await browser.DisposeAsync()` + `await page.DisposeAsync()` | `pdf.Dispose()` |

---

## Comprehensive Feature Comparison

| Category | Feature | PuppeteerSharp | IronPDF |
|----------|---------|----------------|---------|
| **Status** | Active development | Yes | Yes |
| | Commercial support | Sponsorship-based community support | Commercial support, live chat |
| | Enterprise SLA | No | Available |
| **Support** | Documentation focus | Browser automation focused | PDF-focused tutorials and how-tos |
| | Primary channel | GitHub issues, community | Vendor support channels |
| | Troubleshooting context | DevTools Protocol familiarity helps | PDF-specific guidance |
| **Content Creation** | HTML rendering | Full Chromium browser (navigate, interact) | Embedded Chromium engine, render-focused |
| | CSS support | CSS3 via Chromium | CSS3 via embedded Chromium |
| | JavaScript execution | Full browser automation | JavaScript rendering with wait strategies |
| | External binary requirement | Yes (Chromium fetched at runtime) | No (embedded engine) |
| | Linux headless setup | xvfb or `--no-sandbox` typically required | Native |
| | Process management | Manual (launch, dispose browser instances) | In-process, managed by the library |
| **PDF Operations** | Merge PDFs | No (requires external library) | Yes |
| | Split PDFs | No (requires external library) | Yes (extract pages) |
| | Add watermarks/stamps | No (requires external library) | Yes |
| | Digital signatures | No (requires external library) | Yes |
| | Form filling | No (requires external library) | Yes |
| | Extract text | No (requires external library) | Yes |
| | Password protection | No (requires external library) | Yes |
| **Operational** | Sandbox flag tradeoff | `--no-sandbox` commonly used in containers | No separate browser process |
| | Resource cleanup | Manual disposal of browser/page objects | Handled by the library |
| | Memory pressure under load | Higher if disposal is missed | Lower (no per-request browser process) |
| **Common Friction** | Chromium download in CI | Can time out on restricted networks | N/A (no external binary) |
| | Navigation timeouts | Can occur with slow external resources | Tunable via `Timeout` and `WaitFor` |
| | Process leaks | Possible if disposal is missed | N/A (library-based) |
| | Linux headless errors | Common in Docker without `xvfb` / flags | N/A |
| **Development** | Learning curve | Moderate (browser automation concepts) | Low (PDF-focused API) |
| | .NET Standard 2.0 | Yes | Yes |
| | .NET Framework support | Recent versions of .NET Framework | .NET Framework 4.6.2+ |
| | Modern .NET (.NET 6+) | Yes | Yes |
| | Linux support | Yes (with headless configuration) | Yes (native) |
| | Docker/containers | Yes (with configuration) | Yes (minimal configuration) |

---

## Common Troubleshooting Scenarios

### Chrome Binary Download Failures

**Symptom:** `BrowserFetcher.DownloadAsync()` times out or fails in CI/CD pipelines.

**PuppeteerSharp workaround:**
```csharp
// Pre-stage Chromium and point BrowserFetcher at the bundled path
// Or use a base image that already includes a compatible browser
var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
{
    Path = "/path/to/bundled/chrome"
});

// Pin to a specific revision so DownloadAsync uses the staged binary
await browserFetcher.DownloadAsync();
```

**IronPDF:** Not applicable (no external binary download).

---

### "Cannot open display" Error in Docker

**Symptom:** `Error: Failed to launch the browser process! ... cannot open display`

**PuppeteerSharp workaround:**
```bash
# Install X virtual framebuffer in Dockerfile
RUN apt-get update && apt-get install -y xvfb

# Run with xvfb-run
xvfb-run dotnet MyApp.dll
```

Or pass `--no-sandbox` (removes a process-sandbox layer — evaluate before using with untrusted input):
```csharp
await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = true,
    Args = new[] 
    { 
        "--no-sandbox", // Disables Chromium's sandbox; common in containers
        "--disable-setuid-sandbox",
        "--disable-dev-shm-usage"
    }
});
```

**IronPDF:** Not applicable (no X-server requirement).

---

### Memory Leaks from Undisposed Browser Instances

**Symptom:** Application memory grows continuously; `OutOfMemoryException` after hours of operation.

**PuppeteerSharp pattern:**
```csharp
// Without disposal: browser and page objects are not released until GC,
// and the underlying Chrome process can stay resident
public async Task GeneratePdfWithoutDisposal()
{
    var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
    var page = await browser.NewPageAsync();
    await page.SetContentAsync("<h1>Test</h1>");
    await page.PdfAsync("output.pdf");
    // No DisposeAsync calls
}

// With await using: browser and page are disposed when the scope exits
public async Task GeneratePdfWithDisposal()
{
    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
    await using var page = await browser.NewPageAsync();
    await page.SetContentAsync("<h1>Test</h1>");
    await page.PdfAsync("output.pdf");
}
```

**IronPDF:** No external browser process to dispose; a `using` statement releases the PDF document:
```csharp
public void GeneratePdf()
{
    IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

    var renderer = new ChromePdfRenderer();
    using var pdf = renderer.RenderHtmlAsPdf("<h1>Test</h1>");
    pdf.SaveAs("output.pdf");
}
```

---

### Navigation Timeout on Pages with Slow External Resources

**Symptom:** `TimeoutException: Navigation timeout of 30000 ms exceeded`

**PuppeteerSharp workaround:**
```csharp
// Block problematic domains (analytics, ads)
await page.SetRequestInterceptionAsync(true);
page.Request += (sender, e) =>
{
    var blockedDomains = new[] { "google-analytics.com", "doubleclick.net", "facebook.com" };
    if (blockedDomains.Any(domain => e.Request.Url.Contains(domain)))
    {
        e.Request.AbortAsync();
    }
    else
    {
        e.Request.ContinueAsync();
    }
};

// Increase timeouts
await page.GoToAsync("https://slow-site.com", new NavigationOptions
{
    WaitUntil = new[] { WaitUntilNavigation.DOMContentLoaded }, // Don't wait for all resources
    Timeout = 90000 // 90 seconds
});
```

**IronPDF:** Built-in timeout handling with configurable delays:
```csharp
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.Timeout = 90; // seconds
renderer.RenderingOptions.WaitFor.RenderDelay = 1000; // milliseconds after load

using var pdf = renderer.RenderUrlAsPdf("https://slow-site.com");
pdf.SaveAs("output.pdf");
```

---

## Installation Comparison

### PuppeteerSharp

```bash
# Install via NuGet
dotnet add package PuppeteerSharp

# Or via Package Manager Console
Install-Package PuppeteerSharp
```

Namespace import and setup:
```csharp
using PuppeteerSharp;

// Fetch a Chromium build (first run only — deployment must include or
// fetch the binary)
var browserFetcher = new BrowserFetcher();
await browserFetcher.DownloadAsync();

// Launch browser
await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = true,
    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
});
```

### IronPDF

```bash
# Install via NuGet
dotnet add package IronPdf

# Or via Package Manager Console
Install-Package IronPdf
```

Namespace import:
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

// No binary downloads, no browser lifecycle
var renderer = new ChromePdfRenderer();
```

---

## Conclusion

PuppeteerSharp is a strong fit if you need full browser automation — scripting clicks, form fills, screenshots, and navigation flows alongside PDF generation. Its process-based model gives you the entire Chromium surface, at the cost of binary management, process lifecycle, Linux headless setup, and explicit resource disposal. For teams already doing browser automation or that need fine-grained browser control, that cost is usually justified.

If your primary need is "convert HTML to PDF," the same model can add operational friction without an offsetting benefit. The common friction points (Chromium download in CI, headless setup on Linux, missed disposal, navigation tuning) are consequences of driving an external browser process. An embedded engine sidesteps those concerns by running in-process: no binary fetch step, no separate process to manage, no Linux X-server flags, and no manual cross-process disposal.

IronPDF takes the library-based approach. It renders HTML with Chromium and adds built-in PDF manipulation (merge, split, watermark, sign, encrypt) plus commercial support. For teams that primarily need reliable HTML-to-PDF without browser automation, that shape tends to be the simpler fit.

Does your application need full browser automation with PDF as a byproduct, or is HTML-to-PDF rendering the primary requirement?

**Further reading:**
- [IronPDF HTML to PDF tutorial with authentication and custom headers](https://ironpdf.com/tutorials/html-to-pdf/)
- [PDF security and encryption in C#](https://ironpdf.com/tutorials/csharp-pdf-security-complete-tutorial/)
