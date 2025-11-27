# C# PDF Tutorial for Beginners: Create Your First PDF

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF

[![Beginner Friendly](https://img.shields.io/badge/Level-Beginner-brightgreen)]()
[![.NET](https://img.shields.io/badge/.NET-6%2B-512BD4)](https://dotnet.microsoft.com/)
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> Your first PDF in C# in under 5 minutes. This beginner-friendly tutorial walks you through everything step-by-step with working examples you can copy and run.

---

## Table of Contents

1. [What You'll Learn](#what-youll-learn)
2. [Prerequisites](#prerequisites)
3. [Step 1: Create a Project](#step-1-create-a-project)
4. [Step 2: Install IronPDF](#step-2-install-ironpdf)
5. [Step 3: Your First PDF](#step-3-your-first-pdf)
6. [Step 4: Add Styling](#step-4-add-styling)
7. [Step 5: Save and Open](#step-5-save-and-open)
8. [Next Steps](#next-steps)
9. [Common Beginner Questions](#common-beginner-questions)

---

## What You'll Learn

By the end of this tutorial, you'll be able to:

- ✅ Create a PDF from HTML
- ✅ Add styling with CSS
- ✅ Include images
- ✅ Save and view your PDF
- ✅ Understand the basics of PDF generation in C#

**Time required:** 5-10 minutes

---

## Prerequisites

You need:

1. **Visual Studio 2022** (free Community edition is fine)
   - Download: https://visualstudio.microsoft.com/

2. **.NET 6, 7, or 8 SDK** (included with Visual Studio)

3. **Basic C# knowledge** (variables, methods)

That's it! No previous PDF experience needed.

---

## Step 1: Create a Project

### Using Visual Studio

1. Open Visual Studio
2. Click **Create a new project**
3. Select **Console App** (not Console App .NET Framework)
4. Click **Next**
5. Name it `MyFirstPdf`
6. Click **Next**
7. Select **.NET 8.0** (or your installed version)
8. Click **Create**

### Using Command Line

```bash
dotnet new console -n MyFirstPdf
cd MyFirstPdf
```

---

## Step 2: Install IronPDF

### Using Visual Studio

1. Right-click your project in **Solution Explorer**
2. Click **Manage NuGet Packages**
3. Click **Browse** tab
4. Search for `IronPdf`
5. Click **IronPdf** by Iron Software
6. Click **Install**
7. Click **Accept** on the license dialog

### Using Package Manager Console

```powershell
Install-Package IronPdf
```

### Using Command Line

```bash
dotnet add package IronPdf
```

---

## Step 3: Your First PDF

Replace all the code in `Program.cs` with:

```csharp
using IronPdf;

// Create a PDF from HTML
var pdf = ChromePdfRenderer.RenderHtmlAsPdf("<h1>Hello, World!</h1>");

// Save it
pdf.SaveAs("hello.pdf");

Console.WriteLine("PDF created successfully!");
```

### Run It

Press **F5** in Visual Studio, or run:

```bash
dotnet run
```

### Find Your PDF

Look in:
- `bin/Debug/net8.0/hello.pdf` (Visual Studio)
- Or the same folder where you ran `dotnet run`

**Congratulations!** You just created your first PDF! 🎉

---

## Step 4: Add Styling

Let's make something that looks professional. Replace your code with:

```csharp
using IronPdf;

// HTML with CSS styling
string html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {
            font-family: Arial, sans-serif;
            padding: 40px;
            color: #333;
        }
        .header {
            background-color: #2c3e50;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 8px;
        }
        .content {
            margin-top: 30px;
            line-height: 1.6;
        }
        .highlight {
            background-color: #f39c12;
            padding: 5px 10px;
            border-radius: 4px;
            color: white;
        }
        .footer {
            margin-top: 50px;
            text-align: center;
            color: #999;
            font-size: 12px;
        }
    </style>
</head>
<body>
    <div class='header'>
        <h1>My First Styled PDF</h1>
        <p>Created with C# and IronPDF</p>
    </div>

    <div class='content'>
        <h2>Welcome!</h2>
        <p>This PDF was generated from <span class='highlight'>HTML and CSS</span>.</p>
        <p>You can use any HTML you know to create beautiful documents:</p>
        <ul>
            <li>Headings and paragraphs</li>
            <li>Lists (like this one!)</li>
            <li>Tables</li>
            <li>Images</li>
            <li>CSS styling</li>
        </ul>
    </div>

    <div class='footer'>
        <p>Generated on " + DateTime.Now.ToString("MMMM dd, yyyy") + @"</p>
    </div>
</body>
</html>";

// Create the PDF
var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
pdf.SaveAs("styled-document.pdf");

Console.WriteLine("Styled PDF created!");
```

Run it and open `styled-document.pdf`. Much better!

---

## Step 5: Save and Open

Let's automatically open the PDF after creating it:

```csharp
using IronPdf;
using System.Diagnostics;

// Create a PDF
var pdf = ChromePdfRenderer.RenderHtmlAsPdf(@"
    <h1 style='color: blue;'>Auto-Open PDF</h1>
    <p>This PDF will open automatically!</p>
");

// Save it
string filePath = Path.Combine(Directory.GetCurrentDirectory(), "auto-open.pdf");
pdf.SaveAs(filePath);

// Open it in the default PDF viewer
Process.Start(new ProcessStartInfo
{
    FileName = filePath,
    UseShellExecute = true
});

Console.WriteLine("PDF created and opened!");
```

---

## Understanding What Happened

Let's break down the key concepts:

### 1. Import the Library
```csharp
using IronPdf;
```
This gives you access to IronPDF's classes.

### 2. Create a Renderer
```csharp
ChromePdfRenderer.RenderHtmlAsPdf(html)
```
This uses a real Chrome browser engine to convert HTML to PDF. That's why CSS works perfectly!

### 3. Save the Result
```csharp
pdf.SaveAs("filename.pdf")
```
Saves the PDF to a file. You can also get the bytes with `pdf.BinaryData`.

---

## More Examples

### Create an Invoice

```csharp
using IronPdf;

string invoiceHtml = @"
<html>
<head>
    <style>
        body { font-family: Arial; padding: 30px; }
        .invoice-header { display: flex; justify-content: space-between; }
        .company { font-size: 24px; font-weight: bold; color: #2c3e50; }
        .invoice-number { color: #666; }
        table { width: 100%; margin-top: 30px; border-collapse: collapse; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background-color: #f5f5f5; }
        .total { font-size: 20px; font-weight: bold; text-align: right; margin-top: 20px; }
    </style>
</head>
<body>
    <div class='invoice-header'>
        <div class='company'>ACME Corporation</div>
        <div class='invoice-number'>Invoice #1234</div>
    </div>

    <p><strong>Bill To:</strong> John Smith</p>
    <p>123 Main Street, New York, NY 10001</p>

    <table>
        <tr>
            <th>Description</th>
            <th>Qty</th>
            <th>Price</th>
            <th>Total</th>
        </tr>
        <tr>
            <td>Widget A</td>
            <td>2</td>
            <td>$25.00</td>
            <td>$50.00</td>
        </tr>
        <tr>
            <td>Widget B</td>
            <td>1</td>
            <td>$75.00</td>
            <td>$75.00</td>
        </tr>
        <tr>
            <td>Service Fee</td>
            <td>1</td>
            <td>$15.00</td>
            <td>$15.00</td>
        </tr>
    </table>

    <div class='total'>Total: $140.00</div>
</body>
</html>";

var pdf = ChromePdfRenderer.RenderHtmlAsPdf(invoiceHtml);
pdf.SaveAs("invoice.pdf");

Console.WriteLine("Invoice created!");
```

### Convert a Website to PDF

```csharp
using IronPdf;

// Convert any website to PDF!
var pdf = ChromePdfRenderer.RenderUrlAsPdf("https://en.wikipedia.org/wiki/C_Sharp_(programming_language)");
pdf.SaveAs("wikipedia-csharp.pdf");

Console.WriteLine("Website converted to PDF!");
```

### Add Headers and Page Numbers

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

// Add header
renderer.RenderingOptions.HtmlHeader = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align: center;'>My Document Header</div>"
};

// Add footer with page numbers
renderer.RenderingOptions.HtmlFooter = new HtmlHeaderFooter()
{
    HtmlFragment = "<div style='text-align: center;'>Page {page} of {total-pages}</div>"
};

var pdf = renderer.RenderHtmlAsPdf("<h1>Document with Headers</h1><p>Content goes here...</p>");
pdf.SaveAs("with-headers.pdf");
```

---

## Next Steps

Now that you've created your first PDF, explore these tutorials:

### Essential Operations
- **[HTML to PDF](html-to-pdf-csharp.md)** — Comprehensive HTML conversion guide
- **[Merge PDFs](merge-split-pdf-csharp.md)** — Combine multiple PDFs
- **[Add Watermarks](watermark-pdf-csharp.md)** — Protect your documents

### Framework-Specific
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web application PDFs
- **[Blazor](blazor-pdf-generation.md)** — Blazor Server/WASM

### Advanced
- **[Digital Signatures](digital-signatures-pdf-csharp.md)** — Sign documents
- **[Form Filling](fill-pdf-forms-csharp.md)** — Fill PDF forms
- **[PDF/A Compliance](pdf-a-compliance-csharp.md)** — Accessibility

---

## Common Beginner Questions

### Q: Why IronPDF instead of other libraries?

**A:** IronPDF uses a real Chrome browser engine, so any HTML that works in Chrome works in your PDF. Many other libraries can't render modern CSS (Flexbox, Grid) or JavaScript.

### Q: Is IronPDF free?

**A:** IronPDF has a free trial. For production use, licenses start at $749 (one-time). Compare this to developer hours—most teams save money.

### Q: Can I use this in a web app?

**A:** Yes! IronPDF works in ASP.NET Core, ASP.NET MVC, Web API, Blazor Server, and more. See our [ASP.NET Core guide](asp-net-core-pdf-reports.md).

### Q: Does it work on Linux/Mac?

**A:** Yes! IronPDF is fully cross-platform. See the [deployment guide](cross-platform-pdf-dotnet.md).

### Q: How do I add images?

**A:** Use standard HTML `<img>` tags:
```csharp
string html = "<img src='https://example.com/logo.png' />";
// Or local file:
string html = "<img src='file:///C:/images/logo.png' />";
// Or base64:
string html = "<img src='data:image/png;base64,...' />";
```

### Q: My CSS isn't working. Why?

**A:** Make sure your CSS is valid. IronPDF renders exactly like Chrome. Try your HTML in Chrome first to debug styling issues.

---

## Summary

You learned how to:

1. ✅ Create a new C# project
2. ✅ Install IronPDF via NuGet
3. ✅ Generate PDFs from HTML strings
4. ✅ Apply CSS styling
5. ✅ Save and open PDFs

**The key insight:** If you know HTML and CSS, you know how to create PDFs. IronPDF handles the conversion.

---

## Resources

- **[IronPDF Documentation](https://ironpdf.com/docs/)** — Complete reference
- **[Code Examples](https://ironpdf.com/examples/)** — 100+ samples
- **[Best PDF Libraries](best-pdf-libraries-dotnet-2025.md)** — Library comparison
- **[Free Trial](https://ironpdf.com/trial/)** — Try IronPDF

---

### More Tutorials
- **[HTML to PDF Guide](html-to-pdf-csharp.md)** — Advanced HTML conversion
- **[Decision Flowchart](choosing-a-pdf-library.md)** — Choose your library
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web applications
- **[Blazor Guide](blazor-pdf-generation.md)** — Blazor integration
- **[IronPDF Guide](ironpdf/)** — Complete documentation
- **[QuestPDF Guide](questpdf/)** — Code-first alternative

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*
