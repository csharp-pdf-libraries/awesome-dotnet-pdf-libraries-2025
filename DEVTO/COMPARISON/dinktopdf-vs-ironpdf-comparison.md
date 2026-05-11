---
title: "DinkToPdf vs IronPDF: what the docs do not tell you"
published: false
tags: dotnet, csharp, pdf, comparison
canonical_url: https://ironsoftware.com/csharp/pdf/blog/compare-to-competitors/
---

A reporting API hits Kubernetes and the throughput numbers come in lower than the load test target. The renderer is DinkToPdf, a .NET wrapper around the native `wkhtmltopdf` binary, and the underlying engine is a fork of WebKit from circa 2015. Two facts then surface from upstream: the `wkhtmltopdf/wkhtmltopdf` repository was archived on January 2, 2023, and the entire `wkhtmltopdf` GitHub organization was marked archived by an administrator on July 10, 2024. DinkToPdf itself, on the original `rdvojmoc/DinkToPdf` repository, has had no significant commits since around 2019, and its NuGet package v1.0.8 is dated April 18, 2017.

This comparison looks at architecture, rendering capability, and long-term viability when a wrapper around an archived native binary meets a modern in-process renderer.

## Understanding IronPDF

[IronPDF](https://ironpdf.com) embeds a current Chromium engine (the same technology powering Google Chrome) directly into the .NET library. Install via `Install-Package IronPdf` — no external binaries, no command-line tools. The `ChromePdfRenderer` converts HTML using the same rendering pipeline as Chrome, supporting modern CSS3, HTML5, and JavaScript. The library is designed for multi-threaded server environments with in-process rendering.

For performance-conscious teams, this means no process spawning overhead, a modern rendering engine, and consistent behavior with the browser developers already test against.

## Key Limitations of DinkToPdf

### Product Status

DinkToPdf is a thin P/Invoke wrapper over `libwkhtmltox`. The NuGet package v1.0.8 is dated April 18, 2017. The underlying `wkhtmltopdf/wkhtmltopdf` repository was archived on January 2, 2023, and the upstream organization was archived on July 10, 2024. The last `wkhtmltopdf` stable release is v0.12.6 (June 2020). A community fork lives on as `Haukcode.WkHtmlToPdfDotNet` (renamed from `Haukcode.DinkToPdf`), latest 1.5.95 published October 22, 2024 under LGPL-3.0-or-later, but it still binds to the same archived native binary.

`wkhtmltopdf` is built on a WebKit fork from around 2015 (originally derived from a much older Qt WebKit lineage). New web standards added since are not part of that engine.

### Missing Capabilities

**Modern CSS**: Flexbox support is limited and frequently requires legacy `-webkit-` prefixes; CSS Grid is not supported in this engine. Modern CSS3 properties (`gap`, modern `transform`, modern `box-shadow`) are inconsistent or absent — verify against your version. HTML5 elements such as `<canvas>` and `<video>` are limited.

**JavaScript execution**: JavaScript support is based on the older JavaScriptCore engine bundled in the WebKit fork. Modern JavaScript features (ES6+, async/await, recent web APIs) are not part of that engine, and dynamic content rendering can be unreliable.

**No update path**: With the upstream archived, security issues in the bundled WebKit will not be patched, and new web standards will not be added.

### Technical Characteristics

**Process model**: DinkToPdf uses P/Invoke into a native `libwkhtmltox` binary. Documented behavior in the DinkToPdf README is that `BasicConverter` is single-threaded only, and `SynchronizedConverter` serializes calls through a blocking queue. The native side can still crash under concurrent load — the recommendation is to register the converter as a singleton.

**Native binary deployment**: The native `libwkhtmltox` binaries are platform-specific (Windows/Linux/macOS, x86/x64/ARM). On Linux, the binary typically requires `libssl`, `libfontconfig`, and X11-related libraries. Native library management across environments is error-prone.

**IIS hosting**: The DinkToPdf README explicitly states the library was not tested with IIS. Community reports describe crashes and hung processes under IIS. The recommended hosting model is Kestrel.

### Support Status

DinkToPdf is community-maintained at `rdvojmoc/DinkToPdf`, with no significant commits since around 2019. The underlying `wkhtmltopdf` project is archived, so security issues in the bundled WebKit will not be fixed there. No first-party commercial support is offered for either layer.

### Architecture

The stack has multiple layers: .NET wrapper → P/Invoke → native executable → Qt libraries → WebKit engine. Debugging spans several technology layers. Cross-platform deployment requires managing the native binary per OS, and container images need the matching native dependencies.

## Feature Comparison Overview

| Aspect | DinkToPdf (wkhtmltopdf) | IronPDF |
|--------|------------------------|---------|
| **Current Status** | Wrapper has no significant commits since ~2019; engine archived 2023, org archived 2024 | Actively maintained |
| **HTML Support** | WebKit fork (circa 2015) | Modern Chromium |
| **Rendering Quality** | Older CSS/HTML coverage | Browser-quality (Chromium) |
| **Installation** | NuGet + native binaries | Single NuGet package |
| **Support** | Community (wrapper only) | Commercial |
| **Future Viability** | Upstream archived | Active development |

For developers who want extended migration patterns and real-world implementation examples, the [detailed guide](https://ironpdf.com/blog/migration-guides/migrate-from-dinktopdf-to-ironpdf/) offers more code samples and troubleshooting techniques.

---

## Performance Analysis 1: Concurrent Request Throughput

### DinkToPdf — Process and Queue Model

```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class DinkToPdfPerformance
{
    private readonly IConverter _converter;

    public DinkToPdfPerformance()
    {
        // Recommended in the DinkToPdf README:
        // register as a singleton to avoid native-side crashes
        _converter = new SynchronizedConverter(new PdfTools());
    }

    public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
    {
        var sw = Stopwatch.StartNew();

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4
            },
            Objects = {
                new ObjectSettings() {
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
        };

        // Convert() invokes the native wkhtmltopdf engine via P/Invoke
        byte[] pdf = _converter.Convert(doc);

        sw.Stop();
        return pdf;
    }

    public async Task<double> MeasureConcurrentThroughputAsync(
        int concurrentRequests,
        string html)
    {
        var sw = Stopwatch.StartNew();

        // SynchronizedConverter serializes work through a BlockingCollection,
        // so conversions execute sequentially even when callers await in parallel.
        var tasks = new Task<byte[]>[concurrentRequests];
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks[i] = ConvertHtmlToPdfAsync(html);
        }

        await Task.WhenAll(tasks);
        sw.Stop();

        double throughput = concurrentRequests / sw.Elapsed.TotalSeconds;
        return throughput;
    }
}
```

**Architectural characteristics:**
1. P/Invoke into a native `libwkhtmltox` binary
2. `SynchronizedConverter` serializes calls through a blocking queue (per the README)
3. Older WebKit fork in the engine
4. Singleton converter is the documented usage pattern
5. Each rendered document keeps the Qt/WebKit runtime resident
6. Heavy load is gated by the serialized queue

### IronPDF — In-Process Renderer

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class IronPdfPerformance
{
    public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var sw = Stopwatch.StartNew();

        var renderer = new ChromePdfRenderer();

        // In-process rendering using an embedded Chromium engine
        var pdf = await renderer.RenderHtmlAsPdfAsync(html);

        sw.Stop();
        return pdf.BinaryData;
    }

    public async Task<double> MeasureConcurrentThroughputAsync(
        int concurrentRequests,
        string html)
    {
        var sw = Stopwatch.StartNew();

        var tasks = new Task<byte[]>[concurrentRequests];
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks[i] = ConvertHtmlToPdfAsync(html);
        }

        await Task.WhenAll(tasks);
        sw.Stop();

        double throughput = concurrentRequests / sw.Elapsed.TotalSeconds;
        return throughput;
    }
}
```

IronPDF renders in-process and is documented as thread-safe — multiple renderers can run concurrently. See [performance settings](https://ironpdf.com/examples/pdf-generation-settings/).

---

## Performance Analysis 2: Modern CSS Rendering

### DinkToPdf — WebKit Fork Coverage

```csharp
using DinkToPdf;
using DinkToPdf.Contracts;
using System;

public class DinkToPdfModernCss
{
    public byte[] RenderModernLayout(string html)
    {
        var modernHtml = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        .container {
            display: flex;
            justify-content: space-between;
            gap: 20px;
        }

        .grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
        }

        .card {
            transform: rotate(5deg);
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            border-radius: 8px;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='card'>Card 1</div>
        <div class='card'>Card 2</div>
    </div>
</body>
</html>";

        var converter = new SynchronizedConverter(new PdfTools());

        var doc = new HtmlToPdfDocument()
        {
            Objects = {
                new ObjectSettings() {
                    HtmlContent = modernHtml
                }
            }
        };

        byte[] pdf = converter.Convert(doc);
        return pdf;
    }
}
```

**Rendering characteristics in this engine:**
1. Flexbox support is limited and often needs legacy `-webkit-` prefixes
2. CSS Grid is not supported in this engine — verify against your version
3. Modern `transform` and `filter` coverage is inconsistent
4. The `gap` shorthand is typically ignored
5. Vendor prefixes are commonly required for properties Chrome treats as standard
6. Output may not match a modern browser preview

### IronPDF — Chromium Rendering

```csharp
using IronPdf;
using System.Threading.Tasks;

public class IronPdfModernCss
{
    public async Task<byte[]> RenderModernLayoutAsync(string html)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var modernHtml = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        .container {
            display: flex;
            justify-content: space-between;
            gap: 20px;
        }

        .grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 15px;
        }

        .card {
            transform: rotate(5deg);
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            border-radius: 8px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='card'>Card 1</div>
        <div class='card'>Card 2</div>
    </div>
    <div class='grid'>
        <div>Item 1</div>
        <div>Item 2</div>
        <div>Item 3</div>
    </div>
</body>
</html>";

        var renderer = new ChromePdfRenderer();
        var pdf = await renderer.RenderHtmlAsPdfAsync(modernHtml);

        return pdf.BinaryData;
    }
}
```

IronPDF uses a current Chromium build, so flexbox, grid, gradients, transforms, and CSS variables behave the same as in the browser. See [pixel-perfect rendering](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/).

---

## Performance Analysis 3: Server Deployment

### DinkToPdf — Native Dependencies

```csharp
// Deployment shape for DinkToPdf:
// - .NET application
// - DinkToPdf.dll (the wrapper)
// - libwkhtmltox native library (per OS / architecture)
// - Qt runtime dependencies
// - X11-related libraries on headless Linux

public class DinkToPdfDeployment
{
    // Typical Docker layout (Linux):
    // FROM mcr.microsoft.com/dotnet/aspnet:8.0
    //
    // RUN apt-get update && apt-get install -y \
    //     libssl1.1 \
    //     libfontconfig1 \
    //     libx11-6 \
    //     libxext6 \
    //     libxrender1 \
    //     xvfb \
    //     && rm -rf /var/lib/apt/lists/*
    //
    // COPY wkhtmltox/bin/libwkhtmltox.so /app/
    // ENV LD_LIBRARY_PATH=/app
    //
    // ENTRYPOINT ["xvfb-run", "-a", "dotnet", "YourApp.dll"]
}
```

**Deployment considerations:**
1. A platform-specific native binary must ship with the app (Windows/Linux/macOS, x64/ARM64)
2. Qt and SSL/X11 dependencies must match the binary
3. Docker images grow by the size of the headless display dependencies
4. The DinkToPdf README does not recommend IIS
5. The serialized queue limits per-instance concurrency
6. Native crashes are harder to diagnose than managed exceptions

### IronPDF — Managed Deployment

```csharp
// Deployment shape for IronPDF:
// - .NET application
// - IronPdf NuGet package (Chromium downloaded at runtime)

public class IronPdfDeployment
{
    // Typical Docker layout:
    // FROM mcr.microsoft.com/dotnet/aspnet:8.0
    // COPY . /app
    // ENTRYPOINT ["dotnet", "YourApp.dll"]
    //
    // No additional native library to ship manually,
    // and no X11 server is required.

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

        var renderer = new ChromePdfRenderer();
        return (await renderer.RenderHtmlAsPdfAsync(html)).BinaryData;
    }
}
```

IronPDF deploys like any other .NET library and works under both IIS and Kestrel. Kubernetes and Docker setups follow standard .NET patterns.

---

## API Mapping Reference

| DinkToPdf / wkhtmltopdf | IronPDF |
|------------------------|---------|
| `HtmlToPdfDocument` | `ChromePdfRenderer` |
| `GlobalSettings` | `RenderingOptions` |
| `ObjectSettings` | Per-render configuration |
| `SynchronizedConverter` | Thread-safe renderer by design |
| Native process via P/Invoke | In-process rendering |
| WebKit fork (circa 2015) | Chromium (current) |
| Native binary deployment | NuGet package |
| Serialized blocking queue | Multi-threaded rendering |
| X11 on Linux | No X server required |
| Not recommended for IIS | IIS supported |

---

## Comprehensive Feature Comparison

| Feature Category | DinkToPdf (wkhtmltopdf) | IronPDF |
|------------------|------------------------|---------|
| **Status** | | |
| Wrapper Maintenance | No significant commits since ~2019 (v1.0.8 NuGet, April 2017) | Active |
| Engine Status | Archived 2023; org archived 2024 | Active |
| Last Engine Release | wkhtmltopdf 0.12.6 (June 2020) | Regular updates |
| **Rendering** | | |
| Engine | WebKit fork (circa 2015) | Chromium (current) |
| Flexbox | Limited; legacy prefixes typical | Full support |
| CSS Grid | Not supported in this engine | Full support |
| Modern CSS3 | Limited coverage | Full support |
| JavaScript | Older JavaScriptCore (pre-ES6) | Modern Chromium V8 |
| **Architecture** | | |
| Process model | Native P/Invoke | In-process |
| Concurrency model | Serialized queue (`SynchronizedConverter`) | Thread-safe by design |
| Multi-threading | Single queue | Multi-core |
| **Deployment** | | |
| Installation | NuGet + native binaries | NuGet package |
| Dependencies | Qt + X11 on Linux | None to ship manually |
| IIS Support | Not recommended (per README) | Yes |
| Docker | Requires native deps and headless display | Standard |
| Cross-platform | Manual binaries per OS | Managed |
| **Support** | | |
| Type | Community (wrapper only) | Commercial |
| Engine Support | None (upstream archived) | Active |
| Security Updates | Not provided by upstream | Yes |

---

## Installation Comparison

**DinkToPdf:**

```bash
Install-Package DinkToPdf

# Then download the native binaries manually:
# Windows: wkhtmltox-0.12.6-1.msvc2015-win64.exe
# Linux:   wkhtmltox_0.12.6-1.focal_amd64.deb
# Place libwkhtmltox.dll/.so under the application root.

# Register the converter as a singleton (per the README).
# services.AddSingleton(typeof(IConverter),
#     new SynchronizedConverter(new PdfTools()));
```

```csharp
using DinkToPdf;
using DinkToPdf.Contracts;

var converter = new SynchronizedConverter(new PdfTools());
var doc = new HtmlToPdfDocument()
{
    Objects = { new ObjectSettings() { HtmlContent = html } }
};
byte[] pdf = converter.Convert(doc);
```

**IronPDF:**

```bash
Install-Package IronPdf
# Everything included; no additional binaries to manage.
```

```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var renderer = new ChromePdfRenderer();
using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
byte[] bytes = pdf.BinaryData;
```

---

## Conclusion

DinkToPdf was a reasonable choice when `wkhtmltopdf` was actively maintained and CSS workloads were simpler. For .NET projects started before 2020 with basic HTML layouts, the wrapper provided straightforward HTML-to-PDF conversion, the open-source licensing fit projects with strict constraints, and the wider `wkhtmltopdf` ecosystem offered community knowledge.

The upstream situation has changed: `wkhtmltopdf` was archived in January 2023, the upstream organization was archived in July 2024, and the bundled WebKit fork dates from around 2015. New CSS standards added since are not part of that engine, and security issues there will not be patched upstream. DinkToPdf itself has had no significant commits since around 2019, and its NuGet package v1.0.8 still dates from April 2017.

Architectural points worth comparing: a native P/Invoke pipeline and a serialized converter queue versus an in-process renderer; an older WebKit fork versus current Chromium; native binary deployment with X11 dependencies versus a managed NuGet package; IIS not recommended for one and supported by the other.

Migration to IronPDF becomes worth considering when:

- Modern CSS layouts (flexbox, grid) are required for design consistency
- Concurrent throughput requirements exceed what a serialized queue can deliver
- A security review flags unpatched dependencies upstream
- Deployment complexity (native binaries, X11 dependencies) adds operational risk
- IIS hosting is required
- Rendering needs to match Chrome for testing workflows

Teams still using DinkToPdf can plan a migration path. The wrapper may keep working, but the underlying engine will not improve and will not receive security patches upstream. For new projects, picking DinkToPdf means picking an archived engine on day one.

DinkToPdf served its purpose when `wkhtmltopdf` was maintained. IronPDF offers modern rendering aligned with current web standards. Evaluate based on whether circa-2015 web compatibility is sufficient or modern CSS and ongoing security updates are mandatory.

**Are you still running wkhtmltopdf/DinkToPdf?** What does your migration plan look like given the archived upstream?

**Related Resources:**

- [IronPDF HTML Conversion Guide](https://ironpdf.com/how-to/html-string-to-pdf/)
- [Modern CSS Rendering](https://ironpdf.com/how-to/pixel-perfect-html-to-pdf/)
