# Migrating from BCL EasyPDF SDK to IronPDF: drop-in replacement guide

---

The support ticket that forces the migration conversation usually has a subject line like: "PDF generation failing on new server." The stack trace points to a missing COM component, or a 32-bit DLL that doesn't load in a 64-bit process, or a Windows-only native dependency that was never going to survive the move to Linux containers.

BCL EasyPDF SDK works well in the environment it was designed for. The problems start when that environment changes — containerized CI/CD, cross-platform deployments, or .NET Core workloads that never anticipated a COM dependency in the PDF path.

This guide walks through the troubleshooting patterns and migration steps for moving from BCL EasyPDF SDK to **IronPDF**. The focus is on the failure modes you'll actually hit, not a theoretical comparison. Even if you don't use IronPDF, the diagnostic approach here — finding all library surface area, capturing baseline output, testing incrementally — applies to any PDF library switch.

---

## Why Migrate (Without Drama)

Teams running BCL EasyPDF SDK in stable Windows environments often have no reason to move. The trigger is usually an infrastructure change, not a library failure. Common migration drivers:

1. **Docker/Linux containerization** — BCL EasyPDF SDK's native Windows dependencies don't translate to Linux containers without significant workaround effort.
2. **.NET Core / .NET 5+ incompatibility** — COM-based components require `[STAThread]` or specific interop configuration that conflicts with modern ASP.NET Core threading models. **Verify current .NET support before assuming.**
3. **32-bit/64-bit mismatch** — Historical BCL SDK versions ship 32-bit native components. 64-bit IIS and modern .NET runtime defaults cause load failures.
4. **NuGet-first dependency management** — BCL EasyPDF distributed via installer, not NuGet, creates friction in modern CI/CD pipelines.
5. **Missing MSI in CI** — Build agents provisioned from images don't have the BCL installer baked in; each provisioning cycle requires manual intervention.
6. **Azure App Service / serverless** — Managed hosting environments often prohibit COM registration or native library installation.
7. **HTML rendering limitations** — BCL's HTML-to-PDF engine may not handle modern CSS. **Verify your specific version's CSS support.**
8. **Support escalation path** — If BCL Technologies support is slow to respond to issues, teams start evaluating alternatives.
9. **License key management** — Machine-bound or per-server licensing creates friction at scale.
10. **Consolidation** — Teams running multiple PDF tools prefer one well-maintained library.

### Comparison Table

| Aspect | BCL EasyPDF SDK | IronPDF |
|---|---|---|
| Focus | HTML/Word to PDF, Windows-centric | HTML-to-PDF, edit, merge, security |
| Pricing | Per-developer or server | Per-developer or royalty-free |
| API Style | COM interop or .NET wrapper over native | Managed .NET, NuGet-native |
| Learning Curve | Simple for basic use; COM quirks for advanced | Gradual; Chromium model helps with CSS |
| HTML Rendering | Proprietary | Chromium-based |
| Page Indexing | Verify in current docs | 0-based |
| Thread Safety | COM STA model; verify concurrent patterns | Renderer reusable; verify thread safety docs |
| Namespace | Varies by version | `IronPdf` |

---

## Migration Complexity Assessment

### Effort by Feature

| Feature | Effort | Notes |
|---|---|---|
| HTML string to PDF | Low–Medium | API shape likely differs; test CSS output carefully |
| HTML file to PDF | Low–Medium | Path and base URL handling may differ |
| Merge PDFs | Medium | BCL merge API varies by version; verify |
| Split PDFs | Medium-High | Verify BCL page extraction support |
| Watermark | Medium | Stamping model differs |
| Password protection | Medium | Both support; property names differ |
| COM interop removal | High | Core architectural change — audit threading model |
| 32-bit to 64-bit | High | Any P/Invoke or unsafe code must be audited |
| Azure/container deploy | High | New dependency surface, test thoroughly |
| Async patterns | Medium | Verify IronPDF async API against your usage |

### Decision Matrix

| Business Scenario | Recommendation |
|---|---|
| Windows-only, stable infrastructure | Evaluate whether migration provides enough benefit |
| Moving to Linux containers or Azure App Service | Migration strongly worth evaluating |
| .NET Core 3.1+ or .NET 5/6/7/8 workloads | Verify BCL compatibility first; IronPDF is modern .NET native |
| CI/CD pipeline without MSI installer support | NuGet-based IronPDF simplifies dependency management |

---

## Before You Start — Troubleshooting the Existing Setup First

Before writing migration code, diagnose what's actually failing. Teams waste days rewriting code when the real issue is a missing runtime dependency.

**Diagnostic step 1 — What is BCL actually doing?**

```bash
# Find all BCL EasyPDF references
rg "EasyPDF\|BCL\|PdfPrint\|PdfDocument" --type cs -l

# Find COM interop usage
rg "CreateObject\|Activator\.CreateInstance\|Marshal\." --type cs

# Find P/Invoke signatures
rg "\[DllImport" --type cs
```

**Diagnostic step 2 — Check process architecture**

```csharp
// Run this in your current app to confirm 32/64-bit context
Console.WriteLine($"Process is 64-bit: {Environment.Is64BitProcess}");
Console.WriteLine($"OS is 64-bit: {Environment.Is64BitOperatingSystem}");
Console.WriteLine($"Runtime: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
```

**Diagnostic step 3 — Confirm COM availability**

```csharp
// If BCL uses COM, this will fail in environments without the component registered
try
{
    // Replace with actual BCL COM ProgID — verify in your docs
    var type = Type.GetTypeFromProgID("BCL.EasyPDF"); // hypothetical — verify
    if (type == null)
        Console.WriteLine("COM type not found — likely unregistered or missing");
    var obj = Activator.CreateInstance(type);
    Console.WriteLine("COM instance created successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"COM instantiation failed: {ex.Message}");
    // This is the error to address before migration
}
```

**Install IronPDF alongside BCL (don't remove BCL yet):**

```bash
# Add IronPDF while BCL is still present
dotnet add package IronPdf

# Restore
dotnet restore
```

Run a parallel test — both libraries generating the same PDF — before removing BCL. This lets you compare outputs visually and catch rendering differences early.

---

## Quick Start Migration (3 Steps)

### Step 1 — License Configuration

**Before (BCL EasyPDF — generic pattern, verify against your version):**

```csharp
// BCL license activation varies by version
// Some versions use a static key, others a license file
// Verify the exact API in your BCL documentation
var license = new BCL.EasyPdf.License(); // verify class name
license.SetKey("YOUR-BCL-KEY");          // verify method name
```

**After (IronPDF):**

```csharp
using IronPdf;

// Set once at application startup
// https://ironpdf.com/how-to/license-keys/
IronPdf.License.LicenseKey = "YOUR-IRONPDF-KEY";
```

### Step 2 — Namespace Imports

**Before (BCL — verify actual namespace):**
```csharp
using BCL.EasyPdf;        // verify
using BCL.EasyPdf.Html;   // verify
```

**After:**
```csharp
using IronPdf;
```

### Step 3 — Basic HTML-to-PDF

**Before (BCL — generic pattern based on known API shape, verify all method names):**

```csharp
// NOTE: BCL API naming varies by version — verify all of the below
// against your installed SDK documentation before using
using BCL.EasyPdf; // verify namespace

class Program
{
    static void Main()
    {
        // BCL license setup — verify exact API
        // ...license code here...

        // BCL HTML to PDF — verify class and method names
        var converter = new HtmlToPdfConverter(); // verify class name
        converter.Html = "<html><body><h1>Hello</h1></body></html>";
        converter.ConvertToPdf("output.pdf"); // verify method name
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-KEY";

        var renderer = new ChromePdfRenderer();
        using var pdf = renderer.RenderHtmlAsPdf(
            "<html><body><h1>Hello</h1></body></html>"
        );
        pdf.SaveAs("output.pdf");
        Console.WriteLine("Done");
    }
}
```

---

## API Mapping Tables

> **Note:** BCL EasyPDF API names below are based on publicly available documentation patterns. Verify every class and method name against your installed version — BCL SDK versions have varied significantly.

### Namespace Mapping

| BCL EasyPDF | IronPDF | Notes |
|---|---|---|
| `BCL.EasyPdf` | `IronPdf` | Core namespace |
| COM ProgID usage | Not applicable | IronPDF is fully managed |
| `BCL.EasyPdf.Html` | `IronPdf` | HTML rendering |

### Core Class Mapping

| BCL EasyPDF | IronPDF Class | Description |
|---|---|---|
| `HtmlToPdfConverter` | `ChromePdfRenderer` | HTML-to-PDF conversion |
| `PdfDocument` | `PdfDocument` | Loaded PDF document |
| `License` class | `IronPdf.License` | License configuration |
| COM object (via Activator) | Not applicable | IronPDF requires no COM |

### Document Loading Methods

| Operation | BCL EasyPDF | IronPDF |
|---|---|---|
| Load from file | Verify in docs | `PdfDocument.FromFile("file.pdf")` |
| Load from stream | Verify in docs | `PdfDocument.FromStream(stream)` |
| HTML string | Verify in docs | `renderer.RenderHtmlAsPdf(html)` |
| HTML file | Verify in docs | `renderer.RenderHtmlFileAsPdf(path)` |

### Page Operations

| Operation | BCL EasyPDF | IronPDF |
|---|---|---|
| Page count | Verify in docs | `pdf.PageCount` |
| Get page | Verify in docs | `pdf.Pages[0]` (0-based) |
| Delete page | Verify in docs | `pdf.RemovePage(0)` — verify |
| Rotate | Verify in docs | See rendering options docs |

### Merge/Split

| Operation | BCL EasyPDF | IronPDF |
|---|---|---|
| Merge PDFs | Verify in docs | `PdfDocument.Merge(pdf1, pdf2)` |
| Split/extract | Verify in docs | Verify IronPDF page extraction API |

---

## Four Complete Before/After Migrations

### 1. HTML to PDF

**Before (BCL EasyPDF — verify all API against your version):**

```csharp
using System;
// NOTE: verify actual BCL namespace and class names in your SDK documentation
// The following is a representative pattern — not guaranteed to compile

class Program
{
    [STAThread] // COM-based BCL may require STA thread model
    static void Main()
    {
        // BCL license — verify exact API
        // LicenseManager.SetKey("KEY"); // hypothetical

        // BCL HTML conversion — verify class and method names
        // var converter = new HtmlToPdfConverter();
        // converter.Html = GetHtml();
        // converter.OutputFile = "invoice.pdf";
        // converter.Convert(); // hypothetical method name

        // The above is pseudocode — use your BCL docs for actual API
        Console.WriteLine("verify-in-docs — replace with actual BCL API");
    }

    static string GetHtml() => @"<html>
        <body style='font-family:Arial'>
            <h1>Invoice #2071</h1>
            <p>Amount: $1,200.00</p>
            <p>Due: 2025-12-01</p>
        </body>
    </html>";
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        // No STA thread requirement — works in any threading context
        IronPdf.License.LicenseKey = "YOUR-KEY";

        var renderer = new ChromePdfRenderer();

        string html = @"<html>
            <body style='font-family:Arial'>
                <h1>Invoice #2071</h1>
                <p>Amount: $1,200.00</p>
                <p>Due: 2025-12-01</p>
            </body>
        </html>";

        using var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("invoice.pdf");
        Console.WriteLine("Saved invoice.pdf");
    }
}
```

### 2. Merge PDFs

**Before (BCL — verify):**

```csharp
using System;
// BCL merge API — verify all below against your SDK version
// This is a representative pseudocode pattern

class Program
{
    static void Main()
    {
        // BCL merge — hypothetical; verify actual class/method
        // var merger = new PdfMerger(); // verify
        // merger.Add("part1.pdf");      // verify
        // merger.Add("part2.pdf");      // verify
        // merger.Save("merged.pdf");    // verify
        Console.WriteLine("verify-in-docs — replace with actual BCL merge API");
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-KEY";

        // https://ironpdf.com/how-to/merge-or-split-pdfs/
        var pdf1 = PdfDocument.FromFile("part1.pdf");
        var pdf2 = PdfDocument.FromFile("part2.pdf");
        var pdf3 = PdfDocument.FromFile("part3.pdf");

        using var merged = PdfDocument.Merge(pdf1, pdf2, pdf3);
        merged.SaveAs("merged.pdf");
        Console.WriteLine("Merged to merged.pdf");
    }
}
```

### 3. Watermark

**Before (BCL — verify):**

```csharp
using System;
// BCL watermark/stamp API — verify in your SDK documentation
// The stamping mechanism varies significantly by BCL SDK version

class Program
{
    static void Main()
    {
        // Hypothetical BCL watermark API — do not use without verifying
        // var doc = PdfDocument.Open("input.pdf");
        // doc.Pages[0].Watermark("CONFIDENTIAL", opacity: 0.4f);
        // doc.Save("watermarked.pdf");
        Console.WriteLine("verify-in-docs — replace with actual BCL watermark API");
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-KEY";

        using var pdf = PdfDocument.FromFile("input.pdf");

        // https://ironpdf.com/how-to/stamp-text-image/
        var stamper = new TextStamper
        {
            Text = "CONFIDENTIAL",
            FontSize = 48,
            Opacity = 40,
            Rotation = 45,
            VerticalAlignment = VerticalAlignment.Middle,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        pdf.ApplyStamp(stamper);
        pdf.SaveAs("watermarked.pdf");
        Console.WriteLine("Watermarked PDF saved");
    }
}
```

### 4. Password Protection

**Before (BCL — verify):**

```csharp
using System;
// BCL password protection — verify API against your version

class Program
{
    static void Main()
    {
        // Hypothetical — verify actual BCL encryption API
        // var security = new PdfSecurity("input.pdf");
        // security.SetUserPassword("user123");
        // security.SetOwnerPassword("owner456");
        // security.Save("protected.pdf");
        Console.WriteLine("verify-in-docs — replace with actual BCL security API");
    }
}
```

**After (IronPDF):**

```csharp
using System;
using IronPdf;

class Program
{
    static void Main()
    {
        IronPdf.License.LicenseKey = "YOUR-KEY";

        // https://ironpdf.com/how-to/pdf-permissions-passwords/
        using var pdf = PdfDocument.FromFile("input.pdf");

        pdf.SecuritySettings.UserPassword = "user123";
        pdf.SecuritySettings.OwnerPassword = "owner456";
        pdf.SecuritySettings.AllowUserPrinting =
            IronPdf.Security.PdfPrintSecurity.FullPrintRights;

        pdf.SaveAs("protected.pdf");
        Console.WriteLine("Password protected PDF saved");
    }
}
```

---

## Critical Migration Notes

### COM Threading Model

If BCL EasyPDF SDK uses COM under the hood, your code may rely on `[STAThread]` or manual STA thread marshaling. IronPDF is fully managed and does not require STA threading. After migration, you can remove `[STAThread]` from entry points and simplify async code that worked around COM STA restrictions.

```csharp
// Before — STA required for COM-based PDF generation
[STAThread]
static void Main()
{
    // BCL PDF call
}

// After — no threading constraint
static void Main()
{
    // IronPDF call
}
```

### 32-bit to 64-bit

If your project was forced to target `x86` due to BCL's 32-bit native components, you may be able to switch to `AnyCPU` or `x64` after migration. Update your project file:

```xml
<!-- Before -->
<PlatformTarget>x86</PlatformTarget>

<!-- After -->
<PlatformTarget>AnyCPU</PlatformTarget>
```

Then rebuild and test — other dependencies may have had hidden x86 assumptions.

### Page Indexing

IronPDF pages are 0-based. If BCL used 1-based indexing, update all page index references. This is the most common silent bug in PDF library migrations — it won't throw, it'll just operate on the wrong page.

### Exception Types

COM-based failures throw `COMException` with HRESULT codes. IronPDF throws .NET exception types. Update your `catch` blocks accordingly:

```csharp
// Before
catch (System.Runtime.InteropServices.COMException comEx)
{
    Log($"COM error 0x{comEx.HResult:X}: {comEx.Message}");
}

// After
catch (Exception ex)
{
    Log($"IronPDF error: {ex.GetType().Name}: {ex.Message}");
}
```

---

## Performance Considerations

### Async Without COM Constraints

BCL's COM-based architecture makes `async`/`await` painful — COM STA objects can't be awaited across threads without explicit marshaling. IronPDF's managed API works cleanly with `async`:

```csharp
// IronPDF async rendering — https://ironpdf.com/how-to/async/
public async Task<byte[]> RenderAsync(string html)
{
    var renderer = new ChromePdfRenderer();
    using var pdf = await renderer.RenderHtmlAsPdfAsync(html);
    return pdf.BinaryData;
}
```

### Renderer Reuse in Web Apps

In ASP.NET Core, register `ChromePdfRenderer` as a singleton or reuse it at the service level. Creating one per request adds startup overhead (Chromium process initialization).

### Linux Container Dependencies

IronPDF on Linux requires native libraries. In Docker:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install IronPDF native dependencies (verify current list in IronPDF docs)
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libx11-6 \
    libc6 \
    libgtk-3-0 \
    # verify current required packages at https://ironpdf.com/how-to/azure/
    && rm -rf /var/lib/apt/lists/*
```

Check [Azure deployment docs](https://ironpdf.com/how-to/azure/) for the current authoritative dependency list.

---

## Migration Checklist

### Pre-Migration
- [ ] Run `rg "EasyPDF\|BCL" --type cs -l` to inventory all affected files
- [ ] Check project platform target (`x86` vs `AnyCPU`) — flag all x86 dependencies
- [ ] Confirm whether COM registration is required in current setup
- [ ] Capture baseline PDF output (screenshot or file hash) for comparison
- [ ] Identify all `[STAThread]` decorations and COM STA workarounds
- [ ] Check .NET target framework version — confirm IronPDF compatibility
- [ ] Review async patterns that work around COM STA limitations
- [ ] Test IronPDF in parallel with BCL before removing BCL

### Code Migration
- [ ] Add `IronPdf` NuGet package
- [ ] Replace license setup with `IronPdf.License.LicenseKey = "..."`
- [ ] Replace `using BCL...` with `using IronPdf`
- [ ] Replace HTML-to-PDF calls with `ChromePdfRenderer.RenderHtmlAsPdf()`
- [ ] Replace merge operations with `PdfDocument.Merge()`
- [ ] Replace watermark/stamp operations with `TextStamper` or `ImageStamper`
- [ ] Replace password operations with `SecuritySettings` properties
- [ ] Remove `[STAThread]` attributes where no longer needed
- [ ] Update `catch (COMException)` to IronPDF exception types
- [ ] Update page index references

### Testing
- [ ] Visual comparison of HTML-to-PDF outputs
- [ ] Multi-page document merge order and page count verification
- [ ] Password-protected PDF opens with correct credentials
- [ ] Test in 64-bit process if switching from x86
- [ ] Test on Linux/container if that's a target environment
- [ ] Load test concurrent PDF generation
- [ ] Confirm CI/CD pipeline builds without MSI installer step

### Post-Migration
- [ ] Remove BCL package references and installer from build pipeline
- [ ] Update Docker images with IronPDF native dependencies
- [ ] Archive BCL license keys (for audit; don't delete)
- [ ] Update internal runbooks documenting PDF generation approach

---

## Done Migrating? Here's What's Next

The hardest part of migrating off BCL EasyPDF SDK is usually not the API mapping — it's the archaeology. Code that grew up around COM interop tends to have `[STAThread]` decorators in unexpected places, x86 platform targets baked deep into project configs, and threading workarounds that no one quite remembers the reason for. The diagnostic steps before writing migration code are worth more than any API reference.

Run the grep commands. Find the COM usage. Confirm your platform target. Then migrate.

**Question for the comments:** Has anyone successfully run BCL EasyPDF SDK in a 64-bit Linux container? If so — what was the wrapper approach, and was it worth maintaining?

---

*Tags: `dotnet` `csharp` `pdf` `migration`*
