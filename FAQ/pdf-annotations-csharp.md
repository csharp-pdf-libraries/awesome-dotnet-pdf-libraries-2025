# How Can I Add, Read, and Manage PDF Annotations in C# with IronPDF?

If you need to review, comment, or automate document feedback in C#, programmatically working with PDF annotations is a must. IronPDF makes this process straightforward—whether you're adding sticky notes, exporting reviewer comments, or building collaborative workflows. Here’s how to get productive with PDF annotations using IronPDF, with code you can copy and adapt right away.

---

## Why Would I Add Annotations to PDFs in C#?

Annotations let you automate document reviews, highlight issues, or guide users—saving hours of manual markup. Common use cases include contract review, automated QA checks, e-signature flows, and exporting comments for project management. If you’re creating or manipulating PDFs in your .NET apps, annotation support is vital for feedback and collaboration.

If you're interested in other PDF generation workflows (like from XML or XAML), see [Xml To Pdf Csharp](xml-to-pdf-csharp.md) and [Xaml To Pdf Maui Csharp](xaml-to-pdf-maui-csharp.md).

---

## How Do I Add a PDF Annotation in C# with IronPDF?

Here’s the simplest way to add a sticky note to a PDF:

```csharp
using IronPdf; // Install-Package IronPdf

var doc = PdfDocument.FromFile("input.pdf");

var note = new PdfAnnotation(0, 100, 650)
{
    Title = "Review Needed",
    Contents = "Verify figures in this table.",
    Icon = PdfAnnotationIcon.Comment,
    Opacity = 0.8,
    Printable = false
};

doc.Annotations.Add(note);
doc.SaveAs("output-annotated.pdf");
```

- The `0` is the page index (first page).
- Coordinates `(100, 650)` are in PDF points from the bottom-left.  
- The note will be clickable in most PDF viewers.

For more on creating PDFs from HTML before annotating, check out [Url To Pdf Csharp](url-to-pdf-csharp.md).

---

## What Kinds of Annotations Can I Add with IronPDF?

IronPDF supports several annotation types:

- **Text Notes (Sticky Notes):** Popup comments with icons.
- **Links:** Clickable areas for navigation using links or stamps.
- **Stamps:** "Approved", "Draft", or custom stamps via the stamping API.

Direct text highlights and underlines aren’t natively supported, but you can simulate them using colored rectangles. For icon choices like check, info, or insert, use the `PdfAnnotationIcon` enum.

Looking to style your PDFs even further (including web fonts and icons)? See [Web Fonts Icons Pdf Csharp](web-fonts-icons-pdf-csharp.md).

---

## How Can I Customize Annotation Appearance and Behavior?

You can adjust most aspects of annotations:

- **Icons:** Choose from `Comment`, `Help`, `Check`, `Insert`, etc.
- **Colors and Opacity:** Use the `Opacity` property or drawing APIs for highlights.
- **Print visibility:** Set `Printable = false` to hide notes on printed copies.
- **Open/Closed State:** `Open = true` makes the note expanded by default.
- **ReadOnly:** Prevents editing in most viewers.

Example:

```csharp
using IronPdf; // Install-Package IronPdf

var pdf = PdfDocument.FromFile("report.pdf");
var critical = new PdfAnnotation(1, 200, 500)
{
    Title = "Urgent",
    Contents = "Missing April data.",
    Icon = PdfAnnotationIcon.Note,
    Opacity = 0.7,
    Printable = false,
    Open = true,
    ReadOnly = true
};
pdf.Annotations.Add(critical);
pdf.SaveAs("report-reviewed.pdf");
```

---

## How Do I Read or Extract Annotations from a PDF?

Extracting annotations is simple, which is great for audits or review reports:

```csharp
using IronPdf; // Install-Package IronPdf

var doc = PdfDocument.FromFile("feedback.pdf");
foreach (var ann in doc.Annotations)
{
    Console.WriteLine($"Page {ann.PageIndex + 1}: {ann.Title} - {ann.Contents}");
}
```

You can also export all annotation data to JSON for integration with external systems:

```csharp
using IronPdf;
using System.Text.Json;
using System.IO;

var pdf = PdfDocument.FromFile("notes.pdf");
var data = pdf.Annotations.Select(a => new { a.PageIndex, a.Title, a.Contents }).ToList();
var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText("annotations.json", json);
```

---

## Can I Edit or Remove Annotations Programmatically?

Absolutely! You can update, filter, or delete annotations as needed.

- **Edit:**
    ```csharp
    foreach (var a in pdf.Annotations)
        if (a.Title == "Review Needed")
            a.Contents += "\nChecked by QA.";
    ```

- **Remove by index:**
    ```csharp
    pdf.Annotations.RemoveAt(0);
    ```

- **Remove all:**
    ```csharp
    pdf.Annotations.Clear();
    ```

- **Remove by condition:**
    ```csharp
    foreach (var a in pdf.Annotations.Where(a => a.Title == "Draft").ToList())
        pdf.Annotations.Remove(a);
    ```

Always use `.ToList()` when removing in a loop to avoid collection modification errors.

---

## What Are Common Pitfalls When Working with PDF Annotations?

### Why Are My Annotation Positions Incorrect?

PDFs use a bottom-left origin and measure in points (not pixels). Double-check your (x, y) values and test visually.

### Why Don’t My Annotations Show in All Viewers?

Some lightweight PDF viewers ignore certain annotation types. Set `Printable = true` and test in multiple viewers for compatibility.

### Why Do I Get Errors Removing Annotations in a Loop?

Modifying collections while iterating throws exceptions. Always snapshot with `.ToList()` before removing items in a loop.

### Why Aren’t My Annotation Edits Saving?

Ensure you call `pdf.SaveAs("filename.pdf")` after changes, and check your filter logic.

---

## Can I Add Annotations When Creating PDFs from HTML or Other Sources?

Yes! After generating a PDF using IronPDF—such as with [ChromePdfRenderer](https://ironpdf.com/blog/videos/how-to-render-html-string-to-pdf-in-csharp-ironpdf/)—you can add annotations immediately:

```csharp
using IronPdf; // Install-Package IronPdf

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Release Notes</h1>");
pdf.Annotations.Add(new PdfAnnotation(0, 100, 550)
{
    Title = "PM",
    Contents = "Highlight major changes.",
    Icon = PdfAnnotationIcon.Exclamation
});
pdf.SaveAs("release-notes-annotated.pdf");
```

For more PDF creation scenarios, see [Url To Pdf Csharp](url-to-pdf-csharp.md).

---

## How Can I Simulate Highlights or Underlines in PDFs?

While IronPDF doesn’t natively support text highlights, you can approximate this using the drawing API:

```csharp
using IronPdf; // Install-Package IronPdf

var pdf = PdfDocument.FromFile("doc.pdf");
var page = pdf.Pages[0];
page.AddRectangle(120, 700, 200, 20, IronPdf.Drawing.PdfColor.Yellow, opacity: 0.5);
pdf.SaveAs("highlighted.pdf");
```

---

## Where Can I Learn More or Troubleshoot Issues with IronPDF?

- For installation help, see [How To Install Dotnet 10](how-to-install-dotnet-10.md).
- Explore the [IronPDF documentation](https://ironpdf.com/) or [Iron Software’s tools](https://ironsoftware.com/).
- For XAML, XML, or web font rendering, see our related FAQs: [Xml To Pdf Csharp](xml-to-pdf-csharp.md), [Xaml To Pdf Maui Csharp](xaml-to-pdf-maui-csharp.md), [Web Fonts Icons Pdf Csharp](web-fonts-icons-pdf-csharp.md).

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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/), CTO of Iron Software, brings 41 years of coding expertise to PDF technology. As creator of IronPDF (10+ million+ downloads), he leads innovation in HTML-to-PDF conversion and document processing. Created first .NET components in 2005. Connect: [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/)
