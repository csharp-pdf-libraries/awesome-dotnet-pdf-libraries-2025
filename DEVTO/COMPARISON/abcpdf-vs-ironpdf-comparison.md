---
title: "ABCpdf vs IronPDF: a .NET developer honest take"
published: false
tags: dotnet, csharp, pdf, comparison
canonical_url: https://ironsoftware.com/csharp/pdf/blog/compare-to-competitors/
---
## Understanding IronPDF

[IronPDF](https://ironpdf.com) embeds Chromium for HTML-to-PDF conversion in a single package. Install `IronPdf` NuGet, no separate engine downloads needed. The `ChromePdfRenderer` class provides HTML rendering with automatic Chromium version updates. All features (HTML conversion, PDF manipulation, security) included in one assembly.

Architecture: unified package means no engine selection decisions, no compatibility matrix between base library and engine versions, and simplified deployment (one DLL set). The Chromium engine stays current through IronPDF releases—no separate Chrome 65 vs 117 vs 123 engine choices.

## Understanding ABCpdf

### Product Status
ABCpdf is actively maintained by WebSupergoo. Version 13.4.4 was released in April 2026. The product supports .NET Framework 4.6.2+, .NET Core 3.1+, and .NET 5-9. Two main editions are offered: Standard ($329) and Professional ($479), with Professional Redistribution / Enterprise at $4,790 and a Group License at $33,530. Almost two decades of development history. Windows support is comprehensive; Linux support was added in v13 (2026) and varies by HTML engine choice. macOS is not officially supported.

### Edition Differences (Standard vs Professional)
Standard Edition typically excludes PDF rendering/display capabilities, transparency flattening, PDF/A compliance features, and native 64-bit versions — these capabilities are gated to Professional Edition. Teams needing PDF viewing or PDF/A should plan around the Professional tier.

The ABCWebKit engine is based on a wkhtmltopdf-style WebKit fork and may have limited support for newer CSS features such as Grid and Flexbox compared to a current Chromium build — verify against your target version. For modern HTML/CSS, ABCChrome is generally the recommended engine.

### Engine Selection Model
ABCpdf exposes multiple HTML engines: ABCChrome (versions 65/86/117/123), ABCGecko (Gecko/Firefox-based), ABCWebKit, and `MSHtml`. Each engine has separate NuGet packages, different platform support, and varying HTML/CSS compatibility. Deployment planning typically involves mapping engine choice to server OS, .NET version, and required HTML5 features.

Linux deployment requires the platform-specific runtime packages (`ABCpdf.ABCChrome123.Linux` or `ABCpdf.ABCChrome117.Linux`), separate from Windows ABCChrome packages. Cross-platform projects therefore need conditional package references.

### Support
Commercial support is provided by WebSupergoo. Documentation is comprehensive, covering the HTML engines and low-level PDF APIs. Per-server and per-developer licensing models are available.

### Packaging Surface Area
The multi-package model increases deployment surface area: base ABCpdf package + engine-specific package + platform-specific variants. Version compatibility between the base library and engine packages typically must be verified (for example, ABCpdf 13.4.4 + ABCChrome123 vs ABCChrome117).

Per WebSupergoo's own documentation, Azure deployments may require specific instance types — "simpler default Azure instances do not have the features required for functionality like HTML import."

## Feature Comparison Overview

| Aspect | ABCpdf | IronPDF |
|--------|--------|---------|
| **Current Status** | Active (v13.4.4, April 2026) | Active (regular updates) |
| **HTML Support** | Multiple engines (Chrome/Gecko/WebKit/MSHtml) | Single Chromium engine |
| **Rendering Quality** | Engine-dependent | Chromium-based |
| **Installation** | Multi-package (base + engine) | Single package |
| **Support** | Commercial (WebSupergoo) | Commercial (Iron Software) |
| **History** | Active (~20-year history) | Active |

---

## Feature Checklist: Installation & Deployment

| Requirement | ABCpdf | IronPDF |
|-------------|--------|---------|
| **NuGet Installation** | Multiple packages | Single package |
| Base Library Package | `ABCpdf` | `IronPdf` |
| Engine Package Required | Yes (separate for each engine) | No (embedded) |
| Chrome Rendering | `ABCChrome123` or `ABCChrome117` | Included |
| Linux Support Package | `ABCpdf.ABCChrome123.Linux` (separate) | Same package |
| Windows Support | All packages | Same package |
| .NET Framework 4.6.2+ | ✅ Yes | ✅ Yes |
| .NET Core 3.1+ | ✅ Yes | ✅ Yes |
| .NET 5/6/7/8/9 | ✅ Yes | ✅ Yes |
| .NET Standard 2.0 | ✅ Yes | ✅ Yes |
| Docker Deployment | ✅ Yes (engine-dependent) | ✅ Yes |
| Azure App Service | ⚠️ Requires specific instances | ✅ Yes |
| AWS Lambda | Verify per engine | ✅ Yes |
| Xcopy Deployment | ✅ Yes (multi-DLL) | ✅ Yes |

**Deployment complexity:**
- ABCpdf: Choose engine → Install base + engine package → Verify platform compatibility → Deploy multiple DLLs
- IronPDF: Install package → Deploy

---

## Feature Checklist: HTML Rendering Capabilities

| Feature | ABCpdf (ABCChrome) | ABCpdf (ABCGecko) | ABCpdf (ABCWebKit) | IronPDF |
|---------|-------------------|-------------------|-------------------|---------|
| **HTML5 Support** | ✅ Full | ✅ Full | ⚠️ Limited (older WebKit) | ✅ Full |
| **CSS3 Grid** | ✅ Yes | ✅ Yes | ⚠️ Verify per version | ✅ Yes |
| **CSS3 Flexbox** | ✅ Yes | ✅ Yes | ⚠️ Partial | ✅ Yes |
| **Modern JavaScript (ES6+)** | ✅ Yes | ✅ Yes | ⚠️ Limited | ✅ Yes |
| **SVG** | ✅ Full | ✅ Full | ⚠️ Basic | ✅ Full |
| **Web Fonts** | ✅ Yes | ✅ Yes | ⚠️ Verify | ✅ Yes |
| **Canvas** | ✅ Yes | ✅ Yes | ⚠️ Limited | ✅ Yes |
| **@media print** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| **Background Images** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| **Custom Fonts (TTF)** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |

**Engine selection note:** ABCWebKit is based on an older WebKit fork; for modern HTML/CSS, ABCChrome is generally the better fit. Verify against your specific version and target features.

---

## Feature Checklist: API & Development Experience

| Feature | ABCpdf | IronPDF |
|---------|--------|---------|
| **API Style** | Low-level + high-level methods | Fluent high-level API |
| **Async Support** | Synchronous API (no native async render methods) | ✅ Native async (`RenderHtmlAsPdfAsync`) |
| **Sync Support** | ✅ Yes | ✅ Yes |
| **Engine Selection API** | Manual (different classes per engine) | Automatic (single renderer) |
| **Error Handling** | Try-catch with error codes | Try-catch with typed exceptions |
| **Documentation Quality** | ✅ Comprehensive | ✅ Comprehensive |
| **Code Examples** | ✅ Extensive | ✅ Extensive |
| **Thread Safety** | ✅ Yes | ✅ Yes |
| **Reusable Renderer** | ✅ Yes (Doc object) | ✅ Yes (ChromePdfRenderer) |
| **Memory Management** | Manual disposal | Automatic with using statements |
| **Progress Tracking** | ✅ Yes (events) | ⚠️ Limited |
| **Custom Headers/Auth** | ✅ Yes | ✅ Yes |

---

## Feature Checklist: PDF Operations

| Operation | ABCpdf Standard | ABCpdf Professional | IronPDF |
|-----------|----------------|---------------------|---------|
| **Create PDF from HTML** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Create PDF from URL** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Merge PDFs** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Split PDFs** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Add Pages** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Remove Pages** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Rotate Pages** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Extract Text** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Extract Images** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Add Watermarks** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Add Headers/Footers** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Bookmarks** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Annotations** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Form Creation** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Form Filling** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Display/Render PDF** | ❌ No | ✅ Yes | ❌ No (generation only) |
| **Transparency Flattening** | ❌ No | ✅ Yes | ✅ Yes |

**Edition requirement:** PDF display features require Professional Edition.

---

## Feature Checklist: Security & Compliance

| Feature | ABCpdf Standard | ABCpdf Professional | IronPDF |
|---------|----------------|---------------------|---------|
| **Password Protection** | ✅ Yes | ✅ Yes | ✅ Yes |
| **AES Encryption** | ✅ Yes (128/256-bit) | ✅ Yes | ✅ Yes (256-bit) |
| **Digital Signatures** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Certificate Signing** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Permissions (Print/Copy)** | ✅ Yes | ✅ Yes | ✅ Yes |
| **PDF/A Compliance** | ❌ No | ✅ Yes | ✅ Yes |
| **PDF/UA Support** | ✅ Yes | ✅ Yes | ✅ Yes |
| **PDF 2.0 Standard** | ✅ Yes | ✅ Yes | ✅ Yes |
| **Redaction** | ✅ Yes | ✅ Yes | ✅ Yes |
| **LTV (Long Term Validation)** | ✅ Yes | ✅ Yes | Verify in docs |

**Compliance note:** PDF/A features require Professional Edition in ABCpdf.

---

## Feature Checklist: Platform & Deployment

| Platform | ABCpdf (ABCChrome) | ABCpdf (ABCGecko) | ABCpdf (ABCWebKit) | IronPDF |
|----------|-------------------|-------------------|-------------------|---------|
| **Windows** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| **Linux** | ✅ Yes (separate package) | ✅ Yes | ⚠️ Verify | ✅ Yes |
| **macOS** | ⚠️ Verify | ⚠️ Verify | ⚠️ Verify | ✅ Yes |
| **Docker** | ✅ Yes | ✅ Yes | ⚠️ Verify | ✅ Yes |
| **Azure App Service** | ⚠️ Specific instances | ⚠️ Specific instances | ⚠️ Verify | ✅ Yes |
| **AWS Lambda** | ⚠️ Verify | ⚠️ Verify | ⚠️ Verify | ✅ Yes |
| **IIS** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |
| **Kestrel** | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |

**Platform notes:** 
- ABCpdf Linux deployment requires platform-specific engine packages
- Azure requires "specific instance types" per documentation for HTML functionality
- Verify cross-platform support per chosen engine

---

## HTML to PDF Code Comparison

### ABCpdf — Engine Selection Required

```csharp
using WebSupergoo.ABCpdf13;
using WebSupergoo.ABCpdf13.Objects;

public class ABCpdfConverter
{
    public byte[] ConvertHtmlToPdf(string html)
    {
        Doc doc = new Doc();
        // Select an HTML engine. ABCpdf supports Chrome123 (default in v13),
        // Chrome117/Chrome86/Chrome65, Gecko, WebKit, and MSHtml. Each engine
        // is shipped via its own NuGet package.
        doc.HtmlOptions.Engine = EngineType.Chrome123;

        doc.AddImageHtml(html);
        byte[] data = doc.GetData();
        doc.Clear();
        return data;
    }

    public byte[] ConvertUrlToPdf(string url)
    {
        Doc doc = new Doc();
        doc.HtmlOptions.Engine = EngineType.Chrome123;

        // Configure page size and margins
        doc.MediaBox.String = "A4";
        doc.Rect.Inset(20, 20);

        doc.AddImageUrl(url);
        byte[] data = doc.GetData();
        doc.Clear();
        return data;
    }
}

// Usage
var converter = new ABCpdfConverter();

// HTML string
string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial; }
        .grid { display: grid; grid-template-columns: 1fr 1fr; }
    </style>
</head>
<body>
    <div class='grid'>
        <div>Column 1</div>
        <div>Column 2</div>
    </div>
</body>
</html>";

byte[] pdf1 = converter.ConvertHtmlToPdf(html);

// URL
byte[] pdf2 = converter.ConvertUrlToPdf("https://example.com");
```

**Configuration points:**
- Choose HTML engine (Chrome/Gecko/WebKit)
- Ensure corresponding NuGet package installed
- Handle different engine APIs if switching
- Manage multiple DLL dependencies

### IronPDF — Unified API

```csharp
using IronPdf;
using System.Threading.Tasks;

public class IronPdfConverter
{
    static IronPdfConverter()
    {
        IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
    }

    public async Task<byte[]> ConvertHtmlToPdfAsync(string html)
    {
        var renderer = new ChromePdfRenderer();

        using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
        return pdf.BinaryData;
    }

    public async Task<byte[]> ConvertUrlToPdfAsync(string url)
    {
        var renderer = new ChromePdfRenderer();

        // Configure options
        renderer.RenderingOptions.PaperSize =
            IronPdf.Rendering.PdfPaperSize.A4;
        renderer.RenderingOptions.MarginTop = 20;
        renderer.RenderingOptions.MarginBottom = 20;

        using var pdf = await renderer.RenderUrlAsPdfAsync(url);
        return pdf.BinaryData;
    }
}

// Usage
var converter = new IronPdfConverter();

// HTML string
string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial; }
        .grid { display: grid; grid-template-columns: 1fr 1fr; }
    </style>
</head>
<body>
    <div class='grid'>
        <div>Column 1</div>
        <div>Column 2</div>
    </div>
</body>
</html>";

byte[] pdf1 = await converter.ConvertHtmlToPdfAsync(html);

// URL
byte[] pdf2 = await converter.ConvertUrlToPdfAsync("https://example.com");
```

**Simplified flow:**
- No engine selection needed
- Single package contains everything
- Same API regardless of platform
- Chromium always up-to-date

Learn more about [HTML string to PDF conversion](https://ironpdf.com/how-to/html-string-to-pdf/).

---

## Licensing & Edition Checklist

| Consideration | ABCpdf | IronPDF |
|---------------|--------|---------|
| **Edition Tiers** | Standard / Professional | Lite / Professional / Enterprise |
| **Standard Edition Features** | HTML to PDF, PDF manipulation | All features included |
| **Professional Features** | + PDF rendering, PDF/A, transparency | Same as Standard |
| **PDF Display/Viewing** | Professional Edition only | Not included (generation only) |
| **Per-Developer Licensing** | ✅ Available | ✅ Available |
| **Per-Server Licensing** | ✅ Available | ✅ Available |
| **Redistribution** | Verify terms | ✅ Included in deployment |
| **Development License** | ✅ Includes dev/staging/prod | ✅ Includes dev/staging/prod |
| **Trial Period** | ✅ 30-day | ✅ 30-day trial key |
| **Support Included** | ✅ Commercial support | ✅ Commercial support |

**Key difference:** ABCpdf Standard excludes PDF rendering features. IronPDF includes all features in base tier.

---

## Deployment Checklist

### ABCpdf Deployment Steps

- [ ] Choose HTML rendering engine (ABCChrome/ABCGecko/ABCWebKit)
- [ ] Install base `ABCpdf` NuGet package
- [ ] Install engine-specific package (e.g., `ABCChrome123`)
- [ ] If Linux deployment, install platform-specific package (e.g., `ABCpdf.ABCChrome123.Linux`)
- [ ] Verify .NET version compatibility with chosen engine
- [ ] Test HTML/CSS features work with chosen engine
- [ ] Deploy base DLL + engine DLLs to server
- [ ] Configure Azure instance type if using Azure (per documentation requirements)
- [ ] Verify licensing covers chosen edition (Standard vs Professional)
- [ ] Set up engine-specific configuration in code

### IronPDF Deployment Steps

- [ ] Install `IronPdf` NuGet package
- [ ] Deploy to target server
- [ ] Done

---

## Migration Checklist: ABCpdf to IronPDF

### Pre-Migration Assessment

- [ ] Inventory which ABCpdf HTML engine is used (Chrome/Gecko/WebKit)
- [ ] List all PDF operations used (create/merge/split/secure)
- [ ] Identify Standard vs Professional Edition features in use
- [ ] Note deployment platforms (Windows/Linux/both)
- [ ] Check for low-level PDF object manipulation (ObjectSoup)
- [ ] Document custom rendering configurations

### Code Migration

- [ ] Replace `using WebSupergoo.ABCpdf13;` with `using IronPdf;`
- [ ] Replace `Doc` class with `ChromePdfRenderer`
- [ ] Update HTML rendering: `doc.AddImageHtml()` → `renderer.RenderHtmlAsPdf()`
- [ ] Update URL rendering: `doc.AddImageUrl()` → `renderer.RenderUrlAsPdf()`
- [ ] Migrate engine selection logic (remove—single engine in IronPDF)
- [ ] Convert `doc.HtmlOptions` to `renderer.RenderingOptions`
- [ ] Update disposal patterns (both use `using` statements)
- [ ] Remove engine-specific NuGet package references

### Configuration Migration

```csharp
// Before (ABCpdf)
Doc doc = new Doc();
doc.HtmlOptions.Engine = EngineType.Chrome123;
doc.HtmlOptions.Timeout = 60000; // milliseconds
doc.MediaBox.String = "A4";
doc.Rect.Inset(20, 20); // margins
doc.AddImageHtml(html);
byte[] pdf = doc.GetData();
doc.Clear();

// After (IronPDF)
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.Timeout = 60; // seconds
renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
renderer.RenderingOptions.MarginTop = 20;
renderer.RenderingOptions.MarginLeft = 20;
renderer.RenderingOptions.MarginRight = 20;
renderer.RenderingOptions.MarginBottom = 20;
using var pdfDoc = renderer.RenderHtmlAsPdf(html);
byte[] pdfBytes = pdfDoc.BinaryData;
```

### Testing Checklist

- [ ] Visual comparison of PDF output quality
- [ ] Font rendering verification
- [ ] Image quality comparison
- [ ] JavaScript execution validation
- [ ] Header/footer positioning
- [ ] Page numbering accuracy
- [ ] CSS Grid/Flexbox layouts
- [ ] Form field creation
- [ ] Security features (encryption, signatures)
- [ ] Performance benchmarking
- [ ] Cross-platform deployment test

---

## Decision Matrix

### Choose ABCpdf If:

✅ You need PDF rendering/display features (Professional Edition)  
✅ You want flexibility to choose HTML engine (Chrome/Gecko/WebKit)  
✅ Low-level PDF object manipulation is required  
✅ Existing investment in ABCpdf knowledge/code  
✅ 20-year product stability is important  
✅ Specific WebSupergoo support relationship exists  

### Choose IronPDF If:

✅ You want simplified deployment (single package)  
✅ You need guaranteed modern HTML/CSS support without engine selection  
✅ Cross-platform deployment should be seamless  
✅ You prefer not managing multiple package versions  
✅ PDF generation (not rendering/display) is primary use case  
✅ Automatic Chromium updates are desired  
✅ Simpler licensing structure (all features in base tier) is preferred  

---

## Comprehensive Feature Matrix

| Feature Category | ABCpdf Standard | ABCpdf Professional | IronPDF |
|------------------|----------------|---------------------|---------|
| **HTML Rendering** |
| HTML5/CSS3 Support | ✅ (engine-dependent) | ✅ (engine-dependent) | ✅ Yes |
| Multiple Engine Options | ✅ Chrome/Gecko/WebKit | ✅ Chrome/Gecko/WebKit | Single Chromium |
| Modern JavaScript | ✅ (Chrome/Gecko) | ✅ (Chrome/Gecko) | ✅ Yes |
| Async HTML Rendering | Synchronous API | Synchronous API | ✅ Native async |
| **PDF Operations** |
| Create from HTML | ✅ Yes | ✅ Yes | ✅ Yes |
| Create from URL | ✅ Yes | ✅ Yes | ✅ Yes |
| Merge PDFs | ✅ Yes | ✅ Yes | ✅ Yes |
| Split PDFs | ✅ Yes | ✅ Yes | ✅ Yes |
| Extract Text | ✅ Yes | ✅ Yes | ✅ Yes |
| Extract Images | ✅ Yes | ✅ Yes | ✅ Yes |
| Display/Render PDF | ❌ No | ✅ Yes | ❌ No |
| **Security** |
| Encryption | ✅ Yes | ✅ Yes | ✅ Yes |
| Digital Signatures | ✅ Yes | ✅ Yes | ✅ Yes |
| PDF/A Compliance | ❌ No | ✅ Yes | ✅ Yes |
| Permissions | ✅ Yes | ✅ Yes | ✅ Yes |
| **Deployment** |
| Single Package Install | ❌ No (base + engine) | ❌ No (base + engine) | ✅ Yes |
| Windows Support | ✅ Yes | ✅ Yes | ✅ Yes |
| Linux Support | ✅ Yes (separate packages) | ✅ Yes (separate packages) | ✅ Yes |
| Docker Support | ✅ Yes | ✅ Yes | ✅ Yes |
| Azure App Service | ⚠️ Specific instances | ⚠️ Specific instances | ✅ Yes |
| **Development** |
| .NET 5–9 Support | ✅ Yes | ✅ Yes | ✅ Yes |
| Async/Await API | Synchronous API | Synchronous API | ✅ Native async |
| Thread-Safe | ✅ Yes | ✅ Yes | ✅ Yes |
| Low-Level PDF API | ✅ ObjectSoup | ✅ ObjectSoup | ⚠️ Limited |

---

## Installation Commands

**ABCpdf with ABCChrome:**
```bash
Install-Package ABCpdf
Install-Package ABCChrome123
# For Linux deployment, also install the Linux runtime package, e.g.:
Install-Package ABCpdf.ABCChrome123.Linux
```

**IronPDF:**
```bash
Install-Package IronPdf
```

---

## Conclusion

ABCpdf is a mature, comprehensive PDF library with almost 20 years of development history. The multi-engine approach (Chrome/Gecko/WebKit) provides flexibility at the cost of deployment complexity. Professional Edition adds PDF rendering and PDF/A compliance. The ObjectSoup API gives low-level PDF object access for advanced scenarios.

Teams choose ABCpdf when:
- PDF rendering/display features are required (Professional Edition)
- Existing ABCpdf investment and expertise exist
- Low-level PDF object manipulation is needed
- Multiple HTML engine options provide value
- WebSupergoo support relationship is established

Migration from ABCpdf to IronPDF makes sense when:
- Deployment complexity (multiple packages) becomes overhead
- Engine selection decisions add friction
- Cross-platform deployment should be seamless
- PDF rendering features aren't needed (generation only)
- Simplified licensing structure is preferred (all features in base tier)
- Automatic Chromium updates are desired

IronPDF provides unified packaging with embedded Chromium. The [ChromePdfRenderer API](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderer.html/) eliminates engine selection decisions—Chromium is always current. All features included in base tier—no Standard vs Professional split.

Both libraries are actively maintained commercial products with strong support. The choice depends on: deployment complexity tolerance, need for multiple HTML engines vs unified approach, PDF rendering requirements, and preference for packaging models (multi-package flexibility vs single-package simplicity).

**Which packaging model better fits your deployment workflows: multi-package with engine selection, or unified package with embedded engine?**

**Related Resources:**
- [IronPDF HTML to PDF Conversion Guide](https://ironpdf.com/how-to/html-string-to-pdf/)
- [ChromePdfRenderer API Documentation](https://ironpdf.com/object-reference/api/IronPdf.ChromePdfRenderer.html/)
