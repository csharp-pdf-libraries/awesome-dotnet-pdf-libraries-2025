# Migrating from jsreport to IronPDF: no fuss, no fluff

The deployment worked fine on-prem. Then the team containerized the application for Kubernetes, and the PDF generation service started failing intermittently. The error logs pointed to jsreport's child process spawning — Node.js inside a Docker container, with the reporting service trying to launch headless Chromium inside that Node.js process. The layered process management, the user permission model in the container, the memory limits — it all compounds. Getting jsreport running reliably in a container environment takes work that has nothing to do with generating PDFs.

That's a common trigger for evaluating alternatives. This article is for .NET teams considering IronPDF as the replacement. If you're not switching, the architecture comparison section still maps out the tradeoffs worth understanding.

---

## Architecture difference — this matters first

jsreport is a reporting server. IronPDF is a .NET library. That difference has practical implications before you write a single line of migration code:

**jsreport architecture:**
- External Node.js process (separate service, or embedded via jsreport.Local)
- Templates stored in jsreport's template engine (Handlebars, etc.)
- .NET communicates via HTTP API or local IPC
- Reports can be managed via a browser-based designer UI
- Supports multiple output formats: PDF, Excel, CSV, HTML, etc.

**IronPDF architecture:**
- In-process .NET library
- Templates are HTML strings or files (rendered via Chromium)
- No external service — runs in your process
- PDF output only (plus manipulation of existing PDFs)
- No built-in template management UI

The migration decision depends on what you're actually using jsreport for:

| jsreport usage | Migration path |
|---|---|
| PDF generation only, HTML templates | Direct replacement with IronPDF |
| Multiple output formats (Excel, CSV, etc.) | IronPDF covers PDF; other formats need separate handling |
| Template management via browser UI | IronPDF has no equivalent — build template management separately |
| Complex Handlebars/EJS logic | Rewrite templates in HTML/Razor; logic moves to .NET |
| On-premises, works fine | No migration trigger — document this and move on |

---

## Why migrate (without drama)

Eight specific reasons teams trigger this migration:

1. **Container deployment complexity** — jsreport's Chromium-in-Node.js-in-Docker requires managing layered process permissions, seccomp profiles, and memory limits that don't apply to an in-process library.
2. **Node.js dependency in a .NET shop** — maintaining a Node.js runtime, managing npm packages, and keeping jsreport updated adds operational overhead for teams without Node.js expertise.
3. **Inter-process communication overhead** — HTTP or IPC round trips to a reporting server add latency per PDF. In-process rendering eliminates the network hop.
4. **Service orchestration** — jsreport as a separate service needs health checks, restart policies, and separate monitoring. An in-process library reduces the service count.
5. **Azure App Service / serverless constraints** — some Azure tiers restrict background process spawning. An in-process renderer avoids this constraint. See [IronPDF Azure documentation](https://ironpdf.com/how-to/azure/).
6. **Template management coupling** — jsreport templates stored in jsreport's data store create a dependency between your template design and the reporting service. Moving to HTML files in your codebase gives you version control integration.
7. **Output format scope** — if PDF is the only output you actually use, maintaining a full reporting platform is overhead.
8. **jsreport licensing tiers** — if you're hitting limits on the community edition for your use case, evaluating alternatives is reasonable before upgrading tiers.

### Comparison table

| Aspect | jsreport | IronPDF |
|---|---|---|
| Focus | Multi-format reporting platform | PDF generation + manipulation |
| Pricing | Community (free) + commercial tiers | Commercial — verify at ironsoftware.com |
| API Style | HTTP REST API + .NET client | In-process .NET library |
| Learning Curve | Medium (requires understanding reporting concepts) | Medium (.NET API) |
| HTML Rendering | Chromium via Puppeteer | Chromium-based |
| Page Indexing | N/A (output only) | 0-based |
| Thread Safety | External service handles isolation | Renderer instance reuse — see async docs |
| Namespace | `jsreport.Client` (HTTP client) | `IronPdf` |

---

## Migration complexity assessment

### Effort by feature

| Feature | jsreport approach | Effort to migrate |
|---|---|---|
| HTML/Handlebars to PDF | API call with template data | Medium — migrate templates to HTML |
| URL to PDF | jsreport recipe | Low |
| Headers and footers | Template recipe options | Medium |
| Watermark | Custom CSS in template / post-processing | Low (native in IronPDF) |
| Merge PDFs | Not a jsreport core feature | Low (native in IronPDF) |
| Password protection | Not a jsreport core feature | Low |
| Excel output | jsreport native feature | N/A — IronPDF doesn't cover this |
| CSV output | jsreport native feature | N/A |
| Template management UI | jsreport studio | Not in IronPDF — build separately |
| Scheduled reports | jsreport scheduling | Not in IronPDF — build separately |
| Container deployment | Complex (see opening) | Simpler — in-process |

### Decision matrix

| Scenario | Recommendation |
|---|---|
| PDF only, container deployment issues | IronPDF is a direct fit — evaluates well |
| Multi-format output (Excel, CSV) needed | IronPDF covers PDF; retain jsreport for other formats, or add libraries |
| Template management UI is business-critical | jsreport's studio has no equivalent in IronPDF |
| Node.js runtime is already in your stack | jsreport overhead is lower — migration ROI is smaller |

---

## Before you start

### Prerequisites

- .NET 6+
- HTML template inventory from jsreport
- Access to jsreport template files for migration
- NuGet access

### Find jsreport references in your codebase

```bash
# Find jsreport client usage
rg -l "jsreport\|JsReport\|IJsReportClient" --type cs -i

# Find HTTP API calls to jsreport
rg "jsreport\|localhost:5488\|api/report" --type cs -n

# Find template rendering calls
rg "ReportingService\|RenderAsync" --type cs -n

# Find NuGet package references
grep -r "jsreport" *.csproj **/*.csproj 2>/dev/null

# Find jsreport config files
find . -name "jsreport.config.json" -o -name "*.jsrep" 2>/dev/null
```

### Remove jsreport client, install IronPDF

```bash
# Remove jsreport .NET client packages
dotnet remove package jsreport.Client
dotnet remove package jsreport.Local    # if using embedded mode
dotnet remove package jsreport.AspNetCore  # if using middleware

# Install IronPDF
dotnet add package IronPdf
dotnet restore
```

Separately: remove jsreport from Docker images, docker-compose files, and service orchestration config.

---

## Quick start migration (3 steps)

### Step 1: License configuration

**Before (jsreport — no .NET license key; authenticated via jsreport server config):**
```csharp
using jsreport.Client;
using jsreport.Types;
using System;

// jsreport authentication is handled at the HTTP level
var rs = new ReportingService("http://localhost:5488");
// Optional: rs.Username = "admin"; rs.Password = "password";
// No in-process license key
```

**After (IronPDF):**
```csharp
using IronPdf;

// One-time setup at application startup
IronPdf.License.LicenseKey = "YOUR_IRONPDF_LICENSE_KEY";
// Guide: https://ironpdf.com/how-to/license-keys/
```

### Step 2: Namespace imports

**Before:**
```csharp
using jsreport.Client;
using jsreport.Types;
using System.Threading.Tasks;
```

**After:**
```csharp
using IronPdf;
using IronPdf.Rendering;
```

### Step 3: Basic PDF generation

**Before (jsreport API call):**
```csharp
using jsreport.Client;
using jsreport.Types;
using System.IO;
using System.Threading.Tasks;

class QuickStartExample
{
    static async Task Main()
    {
        var rs = new ReportingService("http://localhost:5488");

        var report = await rs.RenderAsync(new RenderRequest
        {
            Template = new Template
            {
                Content = "<h1>Hello World</h1>",
                Engine = Engine.None,
                Recipe = Recipe.ChromePdf
            }
        });

        using var ms = new MemoryStream();
        await report.Content.CopyToAsync(ms);
        File.WriteAllBytes("output.pdf", ms.ToArray());
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World</h1>");
pdf.SaveAs("output.pdf");
// Guide: https://ironpdf.com/how-to/html-string-to-pdf/
```

---

## API mapping tables

### Namespace mapping

| jsreport | IronPDF | Notes |
|---|---|---|
| `jsreport.Client` | `IronPdf` | Core |
| `jsreport.Types` | `IronPdf.Rendering` | Configuration types |
| HTTP client pattern | In-process — no HTTP | Architectural change |

### Core class mapping

| jsreport class | IronPDF class | Description |
|---|---|---|
| `ReportingService` | `ChromePdfRenderer` | Entry point for PDF generation |
| `RenderRequest` | `ChromePdfRenderOptions` | Render configuration |
| `Template` | HTML string or file | Template definition |
| `Report.Content` (stream) | `PdfDocument` | PDF output |

### Document loading methods

| Operation | jsreport | IronPDF |
|---|---|---|
| HTML string | `Template.Content` + `Engine.None` | `renderer.RenderHtmlAsPdf(html)` |
| URL | jsreport URL recipe | `renderer.RenderUrlAsPdf(url)` |
| Template file | jsreport template store | `renderer.RenderHtmlFileAsPdf(path)` |
| Existing PDF | Not jsreport's scope | `PdfDocument.FromFile(path)` |

### Page operations

| Operation | jsreport | IronPDF |
|---|---|---|
| Page size | `ChromePdfOptions.PaperSize` | `ChromePdfRenderOptions.PaperSize` |
| Margins | `ChromePdfOptions` properties | `ChromePdfRenderOptions.Margin*` |
| Orientation | jsreport Chrome options | `ChromePdfRenderOptions.PaperOrientation` |
| Headers/footers | `HeaderTemplate` / `FooterTemplate` | `HtmlHeaderFooter` — verify IronPDF API |

### Merge/split operations

| Operation | jsreport | IronPDF |
|---|---|---|
| Merge | Not a core jsreport feature | `PdfDocument.Merge(pdf1, pdf2)` |
| Split | Not a core jsreport feature | `pdf.CopyPages(startIndex, endIndex)` |

---

## Four complete before/after migrations

### 1. HTML to PDF

**Before (jsreport RenderAsync with template data):**
```csharp
using jsreport.Client;
using jsreport.Types;
using System;
using System.IO;
using System.Threading.Tasks;

class HtmlToPdfExample
{
    static async Task Main()
    {
        var rs = new ReportingService("http://localhost:5488");

        // Template with Handlebars data binding
        string template = @"
            <html><body>
                <h1>Invoice #{{invoiceNumber}}</h1>
                <p>Customer: {{customerName}}</p>
                <p>Amount: ${{amount}}</p>
            </body></html>";

        var report = await rs.RenderAsync(new RenderRequest
        {
            Template = new Template
            {
                Content = template,
                Engine = Engine.Handlebars,
                Recipe = Recipe.ChromePdf,
                Chrome = new Chrome { MarginTop = "10mm", MarginBottom = "10mm" }
            },
            Data = new
            {
                invoiceNumber = "1234",
                customerName = "ACME Corp",
                amount = "500.00"
            }
        });

        // Stream to bytes
        using var ms = new MemoryStream();
        await report.Content.CopyToAsync(ms);
        File.WriteAllBytes("invoice.pdf", ms.ToArray());
        Console.WriteLine("Saved: invoice.pdf");
    }
}
```

**After (IronPDF — template logic moves to your .NET code):**
```csharp
using IronPdf;
using System;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

// Template data rendering moves to .NET (Razor, string interpolation, etc.)
var invoiceNumber = "1234";
var customerName = "ACME Corp";
var amount = "500.00";

string html = $@"
    <html><body>
        <h1>Invoice #{invoiceNumber}</h1>
        <p>Customer: {customerName}</p>
        <p>Amount: ${amount}</p>
    </body></html>";

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.MarginTop = 10;
renderer.RenderingOptions.MarginBottom = 10;

var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

---

### 2. Merge PDFs

**Before (jsreport doesn't merge natively — multi-report generation then merge separately):**
```csharp
using jsreport.Client;
using jsreport.Types;
using PdfSharp.Pdf;       // secondary library
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

class MergeReportsExample
{
    static async Task Main()
    {
        var rs = new ReportingService("http://localhost:5488");
        var tempPaths = new List<string>();

        // Generate each section as separate PDF
        foreach (var sectionHtml in new[] { "<h1>Section 1</h1>", "<h1>Section 2</h1>" })
        {
            var report = await rs.RenderAsync(new RenderRequest
            {
                Template = new Template
                {
                    Content = sectionHtml,
                    Engine = Engine.None,
                    Recipe = Recipe.ChromePdf
                }
            });
            var tempPath = Path.GetTempFileName() + ".pdf";
            using var fs = new FileStream(tempPath, FileMode.Create);
            await report.Content.CopyToAsync(fs);
            tempPaths.Add(tempPath);
        }

        // Merge with PdfSharp (secondary library)
        using var outputDoc = new PdfDocument();
        foreach (var path in tempPaths)
        {
            using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
            foreach (PdfPage page in input.Pages)
                outputDoc.AddPage(page);
        }
        outputDoc.Save("merged.pdf");
        foreach (var path in tempPaths) File.Delete(path); // cleanup
    }
}
```

**After (IronPDF native merge):**
```csharp
using IronPdf;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();

var section1 = renderer.RenderHtmlAsPdf("<h1>Section 1</h1>");
var section2 = renderer.RenderHtmlAsPdf("<h1>Section 2</h1>");

var merged = PdfDocument.Merge(section1, section2);
merged.SaveAs("merged.pdf");
// Guide: https://ironpdf.com/how-to/merge-or-split-pdfs/
```

---

### 3. Watermark

**Before (jsreport — watermark typically via CSS in template, or post-process with secondary library):**
```csharp
using jsreport.Client;
using jsreport.Types;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Threading.Tasks;

class WatermarkExample
{
    static async Task Main()
    {
        var rs = new ReportingService("http://localhost:5488");

        // Generate PDF
        var report = await rs.RenderAsync(new RenderRequest
        {
            Template = new Template
            {
                Content = "<h1>Confidential Report</h1>",
                Engine = Engine.None,
                Recipe = Recipe.ChromePdf
            }
        });

        // Write to temp file, apply watermark with secondary library
        var temp = Path.GetTempFileName() + ".pdf";
        using (var fs = new FileStream(temp, FileMode.Create))
            await report.Content.CopyToAsync(fs);

        // Apply watermark via iTextSharp (secondary library)
        using var reader  = new PdfReader(temp);
        using var outFs   = new FileStream("watermarked.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, outFs);
        var font = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, false);
        for (int i = 1; i <= reader.NumberOfPages; i++)
        {
            var cb = stamper.GetOverContent(i);
            cb.BeginText();
            cb.SetFontAndSize(font, 60);
            cb.SetColorFill(new BaseColor(200, 200, 200));
            cb.ShowTextAligned(Element.ALIGN_CENTER, "CONFIDENTIAL", 300, 400, 45);
            cb.EndText();
        }
        File.Delete(temp);
    }
}
```

**After (IronPDF — no secondary library needed):**
```csharp
using IronPdf;
using IronPdf.Editing;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Confidential Report</h1>");

var stamper = new TextStamper
{
    Text = "CONFIDENTIAL",
    FontColor = IronSoftware.Drawing.Color.LightGray,
    FontSize = 60,
    Opacity = 30,
    Rotation = 45,
    VerticalAlignment = VerticalAlignment.Middle,
    HorizontalAlignment = HorizontalAlignment.Center
};
pdf.ApplyStamp(stamper);
pdf.SaveAs("watermarked.pdf");
// Guide: https://ironpdf.com/how-to/custom-watermark/
```

---

### 4. Password protection

**Before (jsreport — not a native feature; secondary library pattern):**
```csharp
using jsreport.Client;
using jsreport.Types;
using iTextSharp.text.pdf;
using System.IO;
using System.Text;
using System.Threading.Tasks;

class SecurityExample
{
    static async Task Main()
    {
        var rs = new ReportingService("http://localhost:5488");

        var report = await rs.RenderAsync(new RenderRequest
        {
            Template = new Template
            {
                Content = "<h1>Private Document</h1>",
                Engine = Engine.None,
                Recipe = Recipe.ChromePdf
            }
        });

        // Stream to bytes, then encrypt with secondary library
        byte[] pdfBytes;
        using (var ms = new MemoryStream())
        {
            await report.Content.CopyToAsync(ms);
            pdfBytes = ms.ToArray();
        }

        using var reader  = new PdfReader(pdfBytes);
        using var fs      = new FileStream("secured.pdf", FileMode.Create);
        using var stamper = new PdfStamper(reader, fs, '\0', false);
        stamper.SetEncryption(
            Encoding.ASCII.GetBytes("userpass"),
            Encoding.ASCII.GetBytes("ownerpass"),
            PdfWriter.ALLOW_PRINTING,
            PdfWriter.ENCRYPTION_AES_128
        );
    }
}
```

**After (IronPDF):**
```csharp
using IronPdf;
using IronPdf.Security;

IronPdf.License.LicenseKey = "YOUR_LICENSE_KEY";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Private Document</h1>");

pdf.SecuritySettings.UserPassword  = "userpass";
pdf.SecuritySettings.OwnerPassword = "ownerpass";
pdf.SecuritySettings.AllowUserPrinting = PdfPrintSecurity.FullPrintRights;
pdf.SaveAs("secured.pdf");
// Guide: https://ironpdf.com/how-to/pdf-permissions-passwords/
```

---

## Critical migration notes

### Template engine migration

jsreport templates use Handlebars, EJS, or similar engines running in Node.js. After migration, that template logic needs a .NET home. Common patterns:

```csharp
// Option 1: String interpolation (simple cases)
string html = $"<h1>Invoice #{invoice.Number}</h1><p>Total: {invoice.Total:C}</p>";

// Option 2: Razor in a separate class library
// Use RazorLight or a similar Razor rendering library to compile .cshtml → string
// Then pass string to ChromePdfRenderer

// Option 3: Handlebars.NET
// NuGet: Handlebars.Net — allows reusing Handlebars template syntax in .NET
var template = Handlebars.Compile(handlebarsTemplate);
string html = template(data);
var pdf = renderer.RenderHtmlAsPdf(html);
```

### jsreport.Local (embedded mode)

If you were using `jsreport.Local` (which embeds Node.js directly in the .NET process), the migration is simpler — the HTTP layer is already internal. The code change is still significant, but the deployment change is minimal.

### Service removal

If jsreport runs as a separate Docker service in your compose or Kubernetes setup:

```yaml
# docker-compose.yml — remove this service entirely:
# services:
#   jsreport:
#     image: jsreport/jsreport
#     ports:
#       - "5488:5488"
```

Update any service dependencies that reference the jsreport container.

### Page indexing

IronPDF uses 0-based page indexing for all document operations. jsreport doesn't expose a page model (it renders and outputs), so this only matters if you added post-processing with page-aware operations.

---

## Performance considerations

### Eliminate HTTP round trip

Every jsreport PDF generation made a network round trip (or at least IPC). IronPDF renders in-process — no network latency:

```csharp
// jsreport: HTTP call → jsreport Node process → Chromium → response
// Typical latency: 100-500ms+ depending on network and template complexity

// IronPDF: in-process → Chromium → return
// Same Chromium rendering, no network overhead
```

### Renderer reuse in ASP.NET

```csharp
// Register as singleton in ASP.NET Core for reuse
builder.Services.AddSingleton<ChromePdfRenderer>(sp =>
{
    var renderer = new ChromePdfRenderer();
    renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
    return renderer;
});
```

### Async rendering

```csharp
// Async pattern for web handlers
var renderer = new ChromePdfRenderer();
var pdf = await renderer.RenderHtmlAsPdfAsync(html);
// Async guide: https://ironpdf.com/how-to/async/

// Return as file download
return File(pdf.Stream, "application/pdf", "report.pdf");
```

### Azure / cloud deployment

IronPDF's in-process Chromium is more predictable in containerized environments than jsreport's Node.js + Chromium stack. Verify configuration for your specific Azure tier — see [IronPDF Azure documentation](https://ironpdf.com/how-to/azure/).

### Edge cases worth flagging

- **Handlebars template complexity** — if your jsreport templates have complex Handlebars helpers or partials, inventory them before migrating. Each helper needs a .NET equivalent.
- **jsreport assets** — images, fonts, and stylesheets referenced as jsreport assets need to be moved to accessible static paths or embedded in the HTML.
- **Scheduling and reporting workflows** — jsreport includes scheduling features. If your system uses them, build those workflows separately in .NET after migration.

---

## Migration checklist

### Pre-migration

- [ ] Find all jsreport client code: `rg "jsreport\|ReportingService" --type cs -i`
- [ ] Inventory all jsreport templates — export from jsreport studio if needed
- [ ] Identify non-PDF outputs (Excel, CSV) — plan separately
- [ ] Identify scheduled reports — plan .NET-based scheduling separately
- [ ] Map Handlebars helpers to .NET equivalents
- [ ] Identify jsreport assets (images, fonts, styles)
- [ ] Verify IronPDF .NET version compatibility
- [ ] Check Azure / cloud tier for Chromium subprocess permissions if relevant
- [ ] Set up IronPDF trial license in dev environment

### Code migration

- [ ] Remove `jsreport.Client`, `jsreport.Local`, `jsreport.AspNetCore` NuGet packages
- [ ] Add `IronPdf` NuGet package
- [ ] Replace `ReportingService` client with `ChromePdfRenderer`
- [ ] Replace `RenderRequest` + `Template` with HTML string generation
- [ ] Migrate Handlebars templates to .NET template engine (Razor, Handlebars.NET, etc.)
- [ ] Replace merge operations (if any secondary library involved)
- [ ] Replace watermark operations
- [ ] Replace password protection
- [ ] Add IronPDF license key to config
- [ ] Register `ChromePdfRenderer` in DI if using ASP.NET

### Testing

- [ ] Render each template and compare output against jsreport reference PDFs
- [ ] Test with production-representative data (edge cases in data binding)
- [ ] Verify merge, watermark, security operations
- [ ] Test async rendering under concurrent load
- [ ] Test in target deployment environment (container, Azure, etc.)
- [ ] Verify no Chromium subprocess permission issues in container

### Post-migration

- [ ] Remove jsreport Docker service from compose / Kubernetes config
- [ ] Remove jsreport port from service discovery / firewall rules
- [ ] Remove Node.js runtime from Docker images if jsreport was the only consumer
- [ ] Update monitoring — replace jsreport health checks with in-process metrics
- [ ] Monitor memory baseline (Chromium in-process vs external service)

---

## Conclusion

The architectural shift from external reporting server to in-process library is the meaningful part of this migration. The API differences are straightforward once you've mapped the template engine and determined how non-PDF outputs will be handled.

Teams that hit friction tend to be the ones with large Handlebars template libraries — that migration is mechanical but takes time. And teams who depended on jsreport Studio for template management need to decide where that workflow goes.

**What would you add to this migration checklist based on your own jsreport integration?** Particularly interested in teams who had complex Handlebars helpers or were running jsreport in Kubernetes — those setups tend to have extra migration steps not covered here.

---

*Tags: `dotnet`, `csharp`, `pdf`, `migration`*
