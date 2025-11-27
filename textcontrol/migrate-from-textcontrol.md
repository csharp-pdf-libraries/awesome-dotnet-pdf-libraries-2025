# Migration Guide: TX Text Control → IronPDF

## Why Migrate from TX Text Control to IronPDF

TX Text Control's annual licensing costs $3,398+ per developer with mandatory 40% annual renewals, making it 4.5x more expensive than IronPDF for equivalent functionality. Known rendering bugs affect Intel Iris Xe Graphics (11th gen processors) requiring registry workarounds, while TX Text Control's core word processor architecture treats PDF generation as a secondary feature with documented quality issues. IronPDF provides enterprise-grade PDF generation with superior HTML/CSS rendering, straightforward licensing, and dedicated PDF-first architecture without the bloat of document editing UI components.

## NuGet Package Changes

```bash
# Remove TX Text Control
dotnet remove package TXTextControl.TextControl

# Add IronPDF
dotnet add package IronPdf
```

## Namespace Mapping

| TX Text Control | IronPDF |
|----------------|---------|
| `TXTextControl` | `IronPdf` |
| `TXTextControl.DocumentServer` | `IronPdf` |
| `TXTextControl.SaveSettings` | `IronPdf.Rendering` |
| `TXTextControl.LoadSettings` | N/A (IronPDF renders directly) |

## API Mapping Table

| TX Text Control API | IronPDF Equivalent | Notes |
|---------------------|-------------------|-------|
| `ServerTextControl.Create()` | `ChromePdfRenderer` constructor | IronPDF uses Chrome engine |
| `ServerTextControl.Load()` | `ChromePdfRenderer.RenderHtmlAsPdf()` | Direct HTML rendering |
| `ServerTextControl.Save()` | `PdfDocument.SaveAs()` | Simpler API |
| `SaveSettings.PDFAConformance` | `PdfPrintOptions.PdfAFormat` | PDF/A compliance |
| `DocumentServer.MailMerge.Merge()` | String interpolation + `RenderHtmlAsPdf()` | Use HTML templates |
| `StreamType.AdobePDF` | Default output format | PDF is primary format |
| `LoadSaveSettingsBase` | `PdfPrintOptions` | Cleaner options object |

## Code Examples

### Example 1: Basic HTML to PDF Conversion

**Before (TX Text Control):**
```csharp
using TXTextControl;
using TXTextControl.DocumentServer;

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    tx.Load("<html><body><h1>Invoice</h1></body></html>", StreamType.HTMLFormat);
    tx.Save("output.pdf", StreamType.AdobePDF);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<html><body><h1>Invoice</h1></body></html>");
pdf.SaveAs("output.pdf");
```

### Example 2: URL to PDF with Custom Settings

**Before (TX Text Control):**
```csharp
using TXTextControl;
using TXTextControl.DocumentServer;

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    LoadSettings loadSettings = new LoadSettings();
    loadSettings.ApplicationFieldFormat = ApplicationFieldFormat.MSWord;
    
    tx.Load("https://example.com/invoice", StreamType.HTMLFormat, loadSettings);
    
    SaveSettings saveSettings = new SaveSettings();
    saveSettings.PDFAConformance = PDFAConformance.PDFa1b;
    
    tx.Save("output.pdf", StreamType.AdobePDF, saveSettings);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.PdfAFormat = PdfAFormat.PdfA1B;

var pdf = renderer.RenderUrlAsPdf("https://example.com/invoice");
pdf.SaveAs("output.pdf");
```

### Example 3: Advanced PDF Generation with Headers/Footers

**Before (TX Text Control):**
```csharp
using TXTextControl;
using TXTextControl.DocumentServer;

using (ServerTextControl tx = new ServerTextControl())
{
    tx.Create();
    tx.Load(htmlContent, StreamType.HTMLFormat);
    
    // Header/footer requires complex DocumentTarget manipulation
    tx.Selection.DocumentTarget = DocumentTarget.HeadersAndFooters;
    tx.Text = "Page {page} of {numpages}";
    tx.Selection.DocumentTarget = DocumentTarget.Document;
    
    SaveSettings settings = new SaveSettings();
    settings.CreatorApplication = "My App";
    
    tx.Save("output.pdf", StreamType.AdobePDF, settings);
}
```

**After (IronPDF):**
```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align:right'>Page {page} of {total-pages}</div>"
};
renderer.RenderingOptions.MetaData.Author = "My App";

var pdf = renderer.RenderHtmlAsPdf(htmlContent);
pdf.SaveAs("output.pdf");
```

## Common Gotchas

| Issue | Solution |
|-------|----------|
| **ServerTextControl context required** | IronPDF doesn't need initialization context. Create `ChromePdfRenderer` directly. |
| **StreamType.HTMLFormat vs direct HTML** | IronPDF renders HTML/CSS natively. No format conversion needed. |
| **DOCX-based templating** | Replace with HTML templates. Use Razor, Liquid, or string interpolation for dynamic content. |
| **DocumentTarget selection model** | IronPDF uses declarative `RenderingOptions` properties instead of imperative selection API. |
| **License key activation** | Set `IronPdf.License.LicenseKey = "YOUR-KEY"` at application startup, not per-instance. |
| **Missing LoadSettings** | IronPDF doesn't require load settings. HTML/CSS/JavaScript are rendered with Chrome engine directly. |
| **PDF/A compliance properties** | Set via `RenderingOptions.PdfAFormat` enum, not `SaveSettings` object. |
| **Intel Iris Xe workarounds** | IronPDF uses Chromium rendering engine. No hardware acceleration bugs or registry hacks needed. |

## Additional Resources

- **Documentation**: https://ironpdf.com/docs/
- **Tutorials**: https://ironpdf.com/tutorials/
- **Licensing**: IronPDF offers perpetual licenses with optional annual support (not mandatory renewals)