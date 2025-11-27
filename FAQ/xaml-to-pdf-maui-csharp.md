# How Can I Convert .NET MAUI XAML Pages to PDF with IronPDF?

If you're developing a .NET MAUI desktop app and need to export your XAML-based pages—complete with layouts, images, and custom styles—to polished PDF files, IronPDF is a powerful solution. This FAQ covers everything you need to get started, from setup and basic rendering to advanced scenarios, workarounds, and troubleshooting. Whether you want quick, offline-ready PDF exports or complex multi-page reports, you'll find practical, code-focused answers here.

---

## What Do I Need to Start Converting MAUI XAML to PDF?

To begin converting your .NET MAUI XAML pages to PDF, you'll need to add a couple of NuGet packages. The process is straightforward:

Install these packages using the .NET CLI:

```bash
dotnet add package IronPdf
dotnet add package IronPdf.Extensions.Maui
```

Or via the NuGet Package Manager in Visual Studio:

```
Install-Package IronPdf
Install-Package IronPdf.Extensions.Maui
```

- `IronPdf` provides the core PDF rendering engine.
- `IronPdf.Extensions.Maui` adds MAUI-specific helpers, letting you convert `ContentPage` objects directly to PDF.

For more PDF conversion scenarios, including HTML, XML, SVG, and URLs, check out our related FAQs:
- [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md)
- [How can I convert a URL to PDF in C#?](url-to-pdf-csharp.md)
- [How do I export SVG to PDF in C#?](svg-to-pdf-csharp.md)

## Which Platforms Support IronPDF with MAUI?

IronPDF's MAUI extension currently supports **desktop platforms only**: Windows, macOS, and Linux. This is because it relies on a headless Chromium rendering engine, which isn't available on iOS or Android. If you need to generate PDFs on mobile, consider generating them server-side and delivering them to the device, or explore other mobile-oriented PDF libraries.

## How Do I Render a ContentPage to PDF in MAUI?

Here's how you can export your XAML page to PDF:

```csharp
using IronPdf; // Install-Package IronPdf
using IronPdf.Extensions.Maui; // Install-Package IronPdf.Extensions.Maui

var renderer = new ChromePdfRenderer(); // Uses Chromium rendering
var pdfDoc = await renderer.RenderContentPageToPdf<MainPage, App>();
pdfDoc.SaveAs("mainpage.pdf");
```

This will save a PDF version of your `MainPage` in your app's working directory. To save elsewhere, just specify the desired path in `SaveAs`.

For more quick PDF code samples, see our [IronPDF quick examples cookbook](ironpdf-cookbook-quick-examples.md).

## Can I Export Any ContentPage, Not Just MainPage?

Absolutely! You can render any `ContentPage`—for example, an `InvoicePage`, `SummaryPage`, or a custom report page:

```csharp
using IronPdf;
using IronPdf.Extensions.Maui;

var renderer = new ChromePdfRenderer();
var pdfInvoice = await renderer.RenderContentPageToPdf<InvoicePage, App>();
pdfInvoice.SaveAs("invoice.pdf");
```

### How Do I Render a Page With Dynamic Data?

If you have a page that isn't created with a default constructor—for example, a detail page constructed with specific data—you can render an existing instance:

```csharp
var customPage = new ReportPage(reportDataModel);
var pdfResult = await renderer.RenderContentPageToPdf(customPage);
pdfResult.SaveAs("custom-report.pdf");
```

- Use the generic method for pages with a parameterless constructor.
- Pass an instance for pages initialized with data or custom properties.

## How Can I Add Headers, Footers, and Custom Styling to My PDFs?

IronPDF allows you to add headers, footers, and custom CSS for your exported PDFs.

### How Do I Add Dynamic Headers and Footers?

You can set HTML content in the header or footer, including [page numbers](https://ironpdf.com/blog/compare-to-other-components/questpdf-add-page-number-to-pdf/), company info, or even images:

```csharp
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter
{
    HtmlFragment = "<h2 style='text-align:center;'>Monthly Sales Report</h2>"
};
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter
{
    HtmlFragment = "<div style='text-align:center;'>Page {page} of {total-pages}</div>"
};
```

HTML in headers/footers can include dynamic data or branding, giving your PDFs a professional look.

### How Do I Control Margins, Paper Size, or Inject Custom CSS?

You have fine-grained control over layout:

```csharp
renderer.RenderingOptions.MarginTop = 50;
renderer.RenderingOptions.MarginBottom = 30;
renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
```

To inject custom CSS:

```csharp
renderer.RenderingOptions.CustomCss = @"
    body { font-family: 'Segoe UI', Arial, sans-serif; }
    .highlight { color: #3498db; font-weight: bold; }
";
```

Or link to a stylesheet:

```csharp
renderer.RenderingOptions.CustomCssUrl = "file:///C:/YourApp/styles/pdf-export.css";
```

These options let you tailor the appearance of your PDF independently of your in-app XAML.

For more on exporting HTML content, see [How do I convert HTML to PDF in C#?](html-to-pdf-csharp.md)

## What Are the Limitations With Data Binding When Exporting to PDF?

Currently, the `RenderContentPageToPdf` method doesn't support XAML data binding out of the box. Any data-bound controls will render empty in the PDF.

### How Can I Ensure Dynamic Data Appears in the PDF?

Set the properties of your controls directly in code, before rendering the page:

```csharp
public partial class ReportPage : ContentPage
{
    public ReportPage(MyReportData data)
    {
        InitializeComponent();
        TitleLabel.Text = data.Title;
        DataGrid.ItemsSource = data.Details;
    }
}
```

Then, export the specific instance:

```csharp
var reportPage = new ReportPage(reportData);
var pdfDoc = await renderer.RenderContentPageToPdf(reportPage);
pdfDoc.SaveAs("report-with-data.pdf");
```

While this requires more code than XAML bindings, it ensures your PDF output contains the correct data.

## How Can Users Trigger PDF Exports Within My MAUI App?

A common scenario is letting users click a button to export a report. Here’s how you can wire up a button in XAML and handle the export in code:

**MainPage.xaml:**
```xml
<Button Text="Export to PDF" Clicked="OnExportPdfClicked" />
```

**MainPage.xaml.cs:**
```csharp
private async void OnExportPdfClicked(object sender, EventArgs e)
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderContentPageToPdf<MainPage, App>();
    var path = Path.Combine(FileSystem.AppDataDirectory, "exported.pdf");
    pdf.SaveAs(path);

    await DisplayAlert("Done", $"PDF saved at {path}", "OK");
}
```

### How Can I Provide a Smooth User Experience During PDF Generation?

- Show a loading indicator for large or complex exports.
- Gracefully handle errors (e.g., file permissions or disk space).
- Optionally, open or share the PDF immediately after saving:

```csharp
await Launcher.OpenAsync(new OpenFileRequest
{
    File = new ReadOnlyFile(path)
});
```

## Will Images, Fonts, and Advanced XAML Features Work in the PDF?

### Do Images Embedded in XAML Render Properly in the PDF?

Yes, images (e.g., `<Image Source="logo.png" />`) will appear in the PDF, as long as they're available at runtime and included in your project as *Content* or *Embedded Resource*.

### Can I Use Custom Fonts in My Exported PDFs?

Definitely. Register fonts in your `MauiProgram.cs`:

```csharp
builder.ConfigureFonts(fonts =>
{
    fonts.AddFont("CustomFont.ttf", "CustomFontAlias");
});
```
Then use the alias in XAML:

```xml
<Label Text="Stylish Text" FontFamily="CustomFontAlias" />
```

### What About Data Grids and Collection Views?

Complex layouts like tables, grids, and lists are supported. Just make sure to programmatically set their `ItemsSource` before exporting.

### How Does IronPDF Handle Multi-Page Content?

If your page’s content exceeds a single page, IronPDF automatically paginates based on the specified paper size and margins. You don’t need to manually split your XAML—just use scrollable layouts like `ScrollView` or `CollectionView`.

## How Do I Handle Large Reports, Multi-Page Documents, and Performance?

### Can I Merge Multiple ContentPages Into a Single PDF?

Yes! You can render multiple pages and merge them:

```csharp
var renderer = new ChromePdfRenderer();

var coverPdf = await renderer.RenderContentPageToPdf<CoverPage, App>();
var dataPdf = await renderer.RenderContentPageToPdf<DataPage, App>();
var summaryPdf = await renderer.RenderContentPageToPdf<SummaryPage, App>();

var mergedPdf = PdfDocument.Merge(coverPdf, dataPdf, summaryPdf);
mergedPdf.SaveAs("complete-report.pdf");
```

This is great for multi-section documents, like contracts or comprehensive reports.

### What Are Some Performance Tips for Large Exports?

- Simple pages render quickly (usually under 1 second).
- Complex or image-heavy pages may take 2-5 seconds.
- Offload PDF generation to a background thread for best UI responsiveness:

```csharp
await Task.Run(async () =>
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderContentPageToPdf<HeavyPage, App>();
    pdf.SaveAs("large-report.pdf");
});
```

Always show a progress indicator if the export takes more than a second or two.

## How Can I Save, Share, or Print PDFs in a MAUI App?

### How Do I Let Users Pick Where to Save the PDF?

Use the `FilePicker` to let users choose the save location:

```csharp
private async void OnSavePdfClicked(object sender, EventArgs e)
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderContentPageToPdf<MainPage, App>();

    var saveResult = await FilePicker.PickSaveFileAsync(new PickOptions
    {
        PickerTitle = "Save PDF As"
    });

    if (saveResult != null)
    {
        using var fileStream = new FileStream(saveResult.FullPath, FileMode.Create);
        pdf.Stream.CopyTo(fileStream);
    }
}
```

### How Can Users Print the PDF Directly?

After saving, launch the default PDF viewer so users can print:

```csharp
var tempFile = Path.Combine(FileSystem.CacheDirectory, "temp-report.pdf");
pdf.SaveAs(tempFile);

await Launcher.OpenAsync(new OpenFileRequest
{
    File = new ReadOnlyFile(tempFile)
});
```

Users can then print from their preferred PDF viewer.

### Can I Automatically Test PDF Generation?

Yes, you can write automated tests to ensure your PDFs are generated as expected:

```csharp
[Test]
public async Task SampleReportPdf_IsGenerated()
{
    var renderer = new ChromePdfRenderer();
    var pdf = await renderer.RenderContentPageToPdf<SampleReportPage, App>();
    Assert.Greater(pdf.PageCount, 0);
    pdf.SaveAs("test-output.pdf");
    Assert.IsTrue(File.Exists("test-output.pdf"));
}
```

## What Are Common Issues and How Do I Troubleshoot Them?

### Why Is Data Missing in My Exported PDF?

- Relying on XAML bindings won’t work—set values in code before exporting.

### Why Don’t My Images Appear?

- Ensure images are included as Content or Embedded Resource, and paths are correct.

### Why Aren’t Custom Fonts Showing Up?

- Double-check font registration and use the font alias, not the filename, in XAML.

### Why Does Saving the PDF Fail?

- Check write permissions, ensure the file isn’t open elsewhere, and use safe directories like `AppData` or let the user pick the location.

### Why Does the PDF Look Different Than My App?

- Some advanced XAML features (animations, specific controls) may not render identically in the PDF. Test output across platforms to spot inconsistencies.

### What If I Need to Use IronPDF Outside MAUI?

IronPDF supports many .NET scenarios, including ASP.NET, WPF, and console apps. For advanced PDF generation from HTML, SVG, or XML, see these related guides:
- [How do I convert HTML to PDF in C#?](html-to-pdf-csharp.md)
- [How do I convert SVG to PDF in C#?](svg-to-pdf-csharp.md)
- [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md)

You can learn more at the [IronPDF documentation](https://ironpdf.com) and [Iron Software](https://ironsoftware.com).

## Where Can I Learn More or Get Help?

If you run into issues or want to share your tips, the IronPDF team is active in the developer community. For more examples, check the [IronPDF Cookbook](ironpdf-cookbook-quick-examples.md), and don’t hesitate to reach out.

---

*About the author: [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) founded IronPDF and now serves as CTO at [Iron Software](https://ironsoftware.com), where he oversees the Iron Suite of .NET libraries.*
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

**About the Author**: Jacob Mellor is Chief Technology Officer at Iron Software, where PDF tools are used by NASA and other Fortune 500 companies. With expertise in WebAssembly, Rust, Python, Jacob focuses on making PDF generation accessible to all .NET developers. [Author Page](https://ironsoftware.com/about-us/authors/jacobmellor/)
