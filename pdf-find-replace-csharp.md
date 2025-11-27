# PDF Text Find and Replace in C#: Complete Guide

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF | 41 years coding experience

[![.NET](https://img.shields.io/badge/.NET-6%2B-512BD4)](https://dotnet.microsoft.com/)
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> Find and replace text in PDFs is essential for template processing, document personalization, and bulk updates. This guide covers techniques from simple text replacement to complex pattern-based transformations.

---

## Table of Contents

1. [The Challenge of PDF Text Replacement](#the-challenge-of-pdf-text-replacement)
2. [Library Comparison](#library-comparison)
3. [Quick Start](#quick-start)
4. [Simple Text Replacement](#simple-text-replacement)
5. [Template Processing](#template-processing)
6. [Pattern-Based Replacement](#pattern-based-replacement)
7. [Bulk Document Processing](#bulk-document-processing)
8. [Advanced Techniques](#advanced-techniques)
9. [Common Use Cases](#common-use-cases)

---

## The Challenge of PDF Text Replacement

PDF is not a document format—it's a **page description language**. Text in PDFs is stored as positioned glyphs, not editable strings.

### Why PDF Text Replacement Is Hard

```
Traditional document: "Hello World"
PDF internal format:
  - Glyph "H" at position (72, 720)
  - Glyph "e" at position (80, 720)
  - Glyph "l" at position (86, 720)
  - ... and so on
```

This means:
- Text can be fragmented across multiple internal objects
- Font substitution affects glyph widths
- Replacing text may break layout
- Not all PDF libraries can do true replacement

### Approaches to Find/Replace

| Approach | Pros | Cons |
|----------|------|------|
| **Re-render from HTML** | Perfect quality, full control | Need original HTML |
| **Text layer replacement** | Works on existing PDFs | Limited formatting control |
| **Overlay/stamp** | Simple, reliable | Covers, doesn't replace |
| **Full reflow** | True editing | Complex, expensive |

---

## Library Comparison

### Text Find/Replace Capabilities

| Library | Simple Replace | Regex | Preserve Formatting | Templates |
|---------|---------------|-------|---------------------|-----------|
| **IronPDF** | ✅ Yes | ✅ | ✅ | ✅ |
| iText7 | ✅ Complex | ⚠️ | ⚠️ | ⚠️ |
| Aspose.PDF | ✅ Yes | ✅ | ⚠️ | ✅ |
| PDFSharp | ❌ No | ❌ | ❌ | ❌ |
| QuestPDF | ❌ No* | ❌ | ❌ | ✅ |
| PuppeteerSharp | ❌ No* | ❌ | ❌ | ✅ |

*Generation-only libraries: Use template approach instead

### The IronPDF Advantage

**Why IronPDF excels at find/replace:**

1. **Simple API** — One line for basic replacement
2. **Preserves formatting** — Font, size, color maintained
3. **HTML re-rendering** — Perfect for templates
4. **Regex support** — Pattern-based replacement
5. **Batch processing** — Handle thousands of documents

---

## Quick Start

### Install IronPDF

```bash
dotnet add package IronPdf
```

### Basic Find and Replace

```csharp
using IronPdf;

// Load existing PDF
var pdf = PdfDocument.FromFile("contract.pdf");

// Find and replace text
pdf.ReplaceText("OLD COMPANY NAME", "NEW COMPANY NAME");

// Save
pdf.SaveAs("contract-updated.pdf");
```

### Template Approach (Recommended for New Documents)

```csharp
using IronPdf;

// Create template with placeholders
string template = @"
<html>
<body>
    <h1>Invoice for {{CustomerName}}</h1>
    <p>Amount Due: {{Amount}}</p>
    <p>Due Date: {{DueDate}}</p>
</body>
</html>";

// Replace placeholders
string html = template
    .Replace("{{CustomerName}}", "Acme Corp")
    .Replace("{{Amount}}", "$1,500.00")
    .Replace("{{DueDate}}", "December 15, 2025");

// Generate PDF
var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
pdf.SaveAs("invoice.pdf");
```

---

## Simple Text Replacement

### Replace on All Pages

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Replace all occurrences across all pages
pdf.ReplaceText("Draft", "Final");
pdf.ReplaceText("CONFIDENTIAL", "PUBLIC");

pdf.SaveAs("document-updated.pdf");
```

### Replace on Specific Page

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("report.pdf");

// Replace only on first page (cover page)
pdf.ReplaceTextOnPage(0, "2024", "2025");

// Replace on pages 3-5
for (int i = 2; i < 5; i++)
{
    pdf.ReplaceTextOnPage(i, "Q3", "Q4");
}

pdf.SaveAs("report-updated.pdf");
```

### Case-Sensitive vs Insensitive

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Case-sensitive (default)
pdf.ReplaceText("URGENT", "Priority");

// For case-insensitive, replace all variants
var variants = new[] { "urgent", "Urgent", "URGENT" };
foreach (var v in variants)
{
    pdf.ReplaceText(v, "Priority");
}

pdf.SaveAs("document-updated.pdf");
```

### Multiple Replacements

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("template.pdf");

var replacements = new Dictionary<string, string>
{
    { "[DATE]", DateTime.Now.ToString("MMMM dd, yyyy") },
    { "[CLIENT_NAME]", "John Smith" },
    { "[PROJECT]", "Website Redesign" },
    { "[AMOUNT]", "$15,000.00" },
    { "[COMPANY]", "TechCorp Inc." }
};

foreach (var kvp in replacements)
{
    pdf.ReplaceText(kvp.Key, kvp.Value);
}

pdf.SaveAs("completed-document.pdf");
```

---

## Template Processing

### HTML Template System

The most reliable approach for find/replace: start with HTML templates.

```csharp
using IronPdf;

public class PdfTemplateEngine
{
    public byte[] GenerateFromTemplate(string templatePath, Dictionary<string, string> data)
    {
        // Load HTML template
        string html = File.ReadAllText(templatePath);

        // Replace all placeholders
        foreach (var kvp in data)
        {
            html = html.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }

        // Render to PDF
        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}

// Usage
var engine = new PdfTemplateEngine();

var data = new Dictionary<string, string>
{
    { "CustomerName", "Jane Doe" },
    { "InvoiceNumber", "INV-2025-001" },
    { "Total", "$2,500.00" },
    { "DueDate", "December 31, 2025" }
};

var pdfBytes = engine.GenerateFromTemplate("templates/invoice.html", data);
File.WriteAllBytes("invoice-001.pdf", pdfBytes);
```

### Template with Loops

```csharp
using IronPdf;
using System.Text;

public class Invoice
{
    public string CustomerName { get; set; }
    public string InvoiceNumber { get; set; }
    public List<LineItem> Items { get; set; } = new();
    public decimal Tax { get; set; }
}

public class LineItem
{
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;
}

public byte[] GenerateInvoice(Invoice invoice)
{
    var itemsHtml = new StringBuilder();
    foreach (var item in invoice.Items)
    {
        itemsHtml.AppendLine($@"
            <tr>
                <td>{item.Description}</td>
                <td>{item.Quantity}</td>
                <td>{item.UnitPrice:C}</td>
                <td>{item.Total:C}</td>
            </tr>");
    }

    decimal subtotal = invoice.Items.Sum(i => i.Total);
    decimal total = subtotal + invoice.Tax;

    string html = $@"
    <html>
    <head>
        <style>
            body {{ font-family: Arial; }}
            table {{ width: 100%; border-collapse: collapse; }}
            th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
            .total {{ font-weight: bold; text-align: right; }}
        </style>
    </head>
    <body>
        <h1>Invoice {invoice.InvoiceNumber}</h1>
        <p>Customer: {invoice.CustomerName}</p>

        <table>
            <tr>
                <th>Description</th>
                <th>Qty</th>
                <th>Unit Price</th>
                <th>Total</th>
            </tr>
            {itemsHtml}
        </table>

        <p class='total'>Subtotal: {subtotal:C}</p>
        <p class='total'>Tax: {invoice.Tax:C}</p>
        <p class='total'>Total: {total:C}</p>
    </body>
    </html>";

    var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
    return pdf.BinaryData;
}
```

### Razor View Templates (ASP.NET)

```csharp
// ASP.NET Core approach using Razor views
public class RazorPdfService
{
    private readonly IViewRenderService _viewRenderer;

    public RazorPdfService(IViewRenderService viewRenderer)
    {
        _viewRenderer = viewRenderer;
    }

    public async Task<byte[]> GenerateFromRazorView<T>(string viewName, T model)
    {
        // Render Razor view to HTML string
        string html = await _viewRenderer.RenderViewToStringAsync(viewName, model);

        // Convert HTML to PDF
        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}

// Usage in controller
public class ReportController : Controller
{
    private readonly RazorPdfService _pdfService;

    public async Task<IActionResult> DownloadReport(int id)
    {
        var report = await _reportService.GetReport(id);
        var pdfBytes = await _pdfService.GenerateFromRazorView("Reports/MonthlyReport", report);

        return File(pdfBytes, "application/pdf", $"report-{id}.pdf");
    }
}
```

---

## Pattern-Based Replacement

### Regex Find and Replace

```csharp
using IronPdf;
using System.Text.RegularExpressions;

var pdf = PdfDocument.FromFile("document.pdf");

// Extract text to find patterns
string text = pdf.ExtractAllText();

// Find all dates in MM/DD/YYYY format
var datePattern = new Regex(@"(\d{2})/(\d{2})/(\d{4})");

// Replace with YYYY-MM-DD format
foreach (Match match in datePattern.Matches(text))
{
    string oldDate = match.Value;
    string newDate = $"{match.Groups[3].Value}-{match.Groups[1].Value}-{match.Groups[2].Value}";
    pdf.ReplaceText(oldDate, newDate);
}

pdf.SaveAs("document-dates-fixed.pdf");
```

### Phone Number Formatting

```csharp
using IronPdf;
using System.Text.RegularExpressions;

var pdf = PdfDocument.FromFile("contacts.pdf");
string text = pdf.ExtractAllText();

// Find unformatted phone numbers
var phonePattern = new Regex(@"(\d{3})(\d{3})(\d{4})");

foreach (Match match in phonePattern.Matches(text))
{
    string raw = match.Value;
    string formatted = $"({match.Groups[1].Value}) {match.Groups[2].Value}-{match.Groups[3].Value}";
    pdf.ReplaceText(raw, formatted);
}

pdf.SaveAs("contacts-formatted.pdf");
```

### Currency Conversion

```csharp
using IronPdf;
using System.Text.RegularExpressions;

var pdf = PdfDocument.FromFile("price-list.pdf");
string text = pdf.ExtractAllText();

decimal exchangeRate = 0.85m; // USD to EUR

// Find USD amounts
var usdPattern = new Regex(@"\$(\d+(?:,\d{3})*(?:\.\d{2})?)");

foreach (Match match in usdPattern.Matches(text))
{
    string usdString = match.Value;
    decimal usdAmount = decimal.Parse(match.Groups[1].Value.Replace(",", ""));
    decimal eurAmount = usdAmount * exchangeRate;

    string eurString = $"€{eurAmount:N2}";
    pdf.ReplaceText(usdString, eurString);
}

pdf.SaveAs("price-list-eur.pdf");
```

---

## Bulk Document Processing

### Process Directory of PDFs

```csharp
using IronPdf;

public class BulkPdfProcessor
{
    public void ProcessDirectory(string inputDir, string outputDir,
        Dictionary<string, string> replacements)
    {
        var pdfFiles = Directory.GetFiles(inputDir, "*.pdf");

        foreach (var file in pdfFiles)
        {
            var pdf = PdfDocument.FromFile(file);

            foreach (var kvp in replacements)
            {
                pdf.ReplaceText(kvp.Key, kvp.Value);
            }

            string outputPath = Path.Combine(outputDir, Path.GetFileName(file));
            pdf.SaveAs(outputPath);

            Console.WriteLine($"Processed: {Path.GetFileName(file)}");
        }
    }
}

// Usage: Update company name across all contracts
var processor = new BulkPdfProcessor();
processor.ProcessDirectory(
    "contracts/",
    "contracts-updated/",
    new Dictionary<string, string>
    {
        { "Old Company LLC", "New Company Inc." },
        { "123 Old Street", "456 New Avenue" }
    }
);
```

### Parallel Processing

```csharp
using IronPdf;

public async Task ProcessBulkParallel(string[] files, Dictionary<string, string> replacements)
{
    await Parallel.ForEachAsync(files, async (file, ct) =>
    {
        var pdf = PdfDocument.FromFile(file);

        foreach (var kvp in replacements)
        {
            pdf.ReplaceText(kvp.Key, kvp.Value);
        }

        string outputPath = file.Replace(".pdf", "-updated.pdf");
        pdf.SaveAs(outputPath);

        Console.WriteLine($"Completed: {Path.GetFileName(file)}");
    });
}
```

### Mail Merge Style Processing

```csharp
using IronPdf;

public class MailMergeProcessor
{
    public void GeneratePersonalizedDocuments(
        string templatePath,
        List<Dictionary<string, string>> recipients,
        string outputDir)
    {
        string templateHtml = File.ReadAllText(templatePath);

        for (int i = 0; i < recipients.Count; i++)
        {
            string personalizedHtml = templateHtml;

            foreach (var kvp in recipients[i])
            {
                personalizedHtml = personalizedHtml.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
            }

            var pdf = ChromePdfRenderer.RenderHtmlAsPdf(personalizedHtml);

            string fileName = recipients[i].ContainsKey("Name")
                ? $"{recipients[i]["Name"]}.pdf"
                : $"document-{i + 1}.pdf";

            pdf.SaveAs(Path.Combine(outputDir, fileName));
        }
    }
}

// Usage
var processor = new MailMergeProcessor();

var recipients = new List<Dictionary<string, string>>
{
    new() { { "Name", "John Smith" }, { "Email", "john@example.com" }, { "Amount", "$500" } },
    new() { { "Name", "Jane Doe" }, { "Email", "jane@example.com" }, { "Amount", "$750" } },
    new() { { "Name", "Bob Wilson" }, { "Email", "bob@example.com" }, { "Amount", "$600" } }
};

processor.GeneratePersonalizedDocuments("templates/letter.html", recipients, "output/letters/");
```

---

## Advanced Techniques

### Conditional Replacement

```csharp
using IronPdf;

public class ConditionalReplacer
{
    public void ReplaceConditionally(PdfDocument pdf, string search,
        Func<string, bool> condition, string replacement)
    {
        string text = pdf.ExtractAllText();

        // Find all occurrences
        int index = 0;
        while ((index = text.IndexOf(search, index)) != -1)
        {
            // Get surrounding context
            int contextStart = Math.Max(0, index - 50);
            int contextEnd = Math.Min(text.Length, index + search.Length + 50);
            string context = text.Substring(contextStart, contextEnd - contextStart);

            if (condition(context))
            {
                pdf.ReplaceText(search, replacement);
            }

            index += search.Length;
        }
    }
}

// Usage: Only replace "Draft" if it's in a header
var replacer = new ConditionalReplacer();
var pdf = PdfDocument.FromFile("document.pdf");

replacer.ReplaceConditionally(pdf, "Draft",
    context => context.Contains("Status:") || context.Contains("Version:"),
    "Final");

pdf.SaveAs("document-final.pdf");
```

### Replacement with Logging

```csharp
using IronPdf;

public class AuditedReplacer
{
    public List<ReplacementLog> Replacements { get; } = new();

    public void ReplaceWithAudit(PdfDocument pdf, string oldText, string newText)
    {
        string before = pdf.ExtractAllText();
        int countBefore = CountOccurrences(before, oldText);

        pdf.ReplaceText(oldText, newText);

        string after = pdf.ExtractAllText();
        int countAfter = CountOccurrences(after, oldText);

        Replacements.Add(new ReplacementLog
        {
            OldText = oldText,
            NewText = newText,
            OccurrencesReplaced = countBefore - countAfter,
            Timestamp = DateTime.UtcNow
        });
    }

    private int CountOccurrences(string text, string search)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(search, index)) != -1)
        {
            count++;
            index += search.Length;
        }
        return count;
    }

    public void SaveAuditLog(string path)
    {
        var json = JsonSerializer.Serialize(Replacements, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(path, json);
    }
}

public class ReplacementLog
{
    public string OldText { get; set; }
    public string NewText { get; set; }
    public int OccurrencesReplaced { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Dynamic Content Insertion

```csharp
using IronPdf;

public class DynamicContentInserter
{
    public byte[] InsertDynamicContent(string templatePath, DocumentData data)
    {
        string html = File.ReadAllText(templatePath);

        // Simple replacements
        html = html.Replace("{{Title}}", data.Title);
        html = html.Replace("{{Author}}", data.Author);
        html = html.Replace("{{Date}}", data.Date.ToString("MMMM dd, yyyy"));

        // Conditional content
        if (data.IncludeDisclaimer)
        {
            html = html.Replace("{{Disclaimer}}", GetDisclaimerHtml());
        }
        else
        {
            html = html.Replace("{{Disclaimer}}", "");
        }

        // Dynamic table
        html = html.Replace("{{DataTable}}", BuildTableHtml(data.TableRows));

        // Dynamic chart (embedded as image)
        string chartBase64 = GenerateChartImage(data.ChartData);
        html = html.Replace("{{Chart}}", $"<img src='data:image/png;base64,{chartBase64}' />");

        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }

    private string BuildTableHtml(List<TableRow> rows)
    {
        var sb = new StringBuilder("<table>");
        foreach (var row in rows)
        {
            sb.Append("<tr>");
            foreach (var cell in row.Cells)
            {
                sb.Append($"<td>{cell}</td>");
            }
            sb.Append("</tr>");
        }
        sb.Append("</table>");
        return sb.ToString();
    }

    private string GetDisclaimerHtml() => "<p class='disclaimer'>Standard disclaimer text...</p>";
    private string GenerateChartImage(object data) => "..."; // Chart generation logic
}
```

---

## Common Use Cases

### Contract Template Processing

```csharp
using IronPdf;

public class ContractGenerator
{
    private readonly string _templatePath;

    public ContractGenerator(string templatePath)
    {
        _templatePath = templatePath;
    }

    public byte[] GenerateContract(ContractData contract)
    {
        string html = File.ReadAllText(_templatePath);

        // Party information
        html = html.Replace("{{PartyA_Name}}", contract.PartyA.Name);
        html = html.Replace("{{PartyA_Address}}", contract.PartyA.Address);
        html = html.Replace("{{PartyB_Name}}", contract.PartyB.Name);
        html = html.Replace("{{PartyB_Address}}", contract.PartyB.Address);

        // Contract details
        html = html.Replace("{{EffectiveDate}}", contract.EffectiveDate.ToString("MMMM dd, yyyy"));
        html = html.Replace("{{TermLength}}", contract.TermMonths.ToString());
        html = html.Replace("{{ContractValue}}", contract.Value.ToString("C"));

        // Terms and conditions
        html = html.Replace("{{PaymentTerms}}", contract.PaymentTerms);
        html = html.Replace("{{Jurisdiction}}", contract.Jurisdiction);

        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);

        return pdf.BinaryData;
    }
}

public class ContractData
{
    public Party PartyA { get; set; }
    public Party PartyB { get; set; }
    public DateTime EffectiveDate { get; set; }
    public int TermMonths { get; set; }
    public decimal Value { get; set; }
    public string PaymentTerms { get; set; }
    public string Jurisdiction { get; set; }
}

public class Party
{
    public string Name { get; set; }
    public string Address { get; set; }
}
```

### Certificate Generation

```csharp
using IronPdf;

public class CertificateGenerator
{
    public byte[] GenerateCertificate(string recipientName, string courseName,
        DateTime completionDate, string instructorName)
    {
        string html = $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: 'Times New Roman', serif;
                    text-align: center;
                    padding: 50px;
                    background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
                }}
                .certificate {{
                    border: 3px double #gold;
                    padding: 40px;
                    background: white;
                }}
                h1 {{ color: #2c3e50; font-size: 36px; }}
                .recipient {{ font-size: 28px; color: #e74c3c; margin: 30px 0; }}
                .course {{ font-size: 20px; margin: 20px 0; }}
                .signature {{ margin-top: 50px; }}
            </style>
        </head>
        <body>
            <div class='certificate'>
                <h1>Certificate of Completion</h1>
                <p>This is to certify that</p>
                <p class='recipient'><strong>{recipientName}</strong></p>
                <p>has successfully completed the course</p>
                <p class='course'><strong>{courseName}</strong></p>
                <p>on {completionDate:MMMM dd, yyyy}</p>
                <div class='signature'>
                    <p>_________________________</p>
                    <p>{instructorName}</p>
                    <p>Instructor</p>
                </div>
            </div>
        </body>
        </html>";

        var renderer = new ChromePdfRenderer();
        renderer.RenderingOptions.PaperSize = PdfPaperSize.A4;
        renderer.RenderingOptions.PaperOrientation = PdfPaperOrientation.Landscape;

        var pdf = renderer.RenderHtmlAsPdf(html);
        return pdf.BinaryData;
    }
}
```

### Invoice Number Update

```csharp
using IronPdf;

public class InvoiceUpdater
{
    public void UpdateInvoiceStatus(string pdfPath, string newStatus)
    {
        var pdf = PdfDocument.FromFile(pdfPath);

        // Update status
        var statuses = new[] { "Draft", "Pending", "Sent", "Paid", "Overdue" };
        foreach (var status in statuses)
        {
            pdf.ReplaceText($"Status: {status}", $"Status: {newStatus}");
        }

        // Update modification date
        pdf.ReplaceText(
            pdf.ExtractAllText().Contains("Modified:") ? "Modified:" : "",
            $"Modified: {DateTime.Now:yyyy-MM-dd HH:mm}"
        );

        pdf.SaveAs(pdfPath);
    }
}
```

---

## Best Practices

### 1. Use HTML Templates for New Documents

```csharp
// ✅ Recommended: Template approach
string template = File.ReadAllText("template.html");
string html = template.Replace("{{Name}}", name);
var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

// ⚠️ Less reliable: Editing existing PDFs
var pdf = PdfDocument.FromFile("existing.pdf");
pdf.ReplaceText("Placeholder", name);
```

### 2. Verify Replacements

```csharp
public bool VerifyReplacement(string pdfPath, string oldText)
{
    var pdf = PdfDocument.FromFile(pdfPath);
    string text = pdf.ExtractAllText();

    bool success = !text.Contains(oldText);

    if (!success)
    {
        Console.WriteLine($"WARNING: '{oldText}' still found in document");
    }

    return success;
}
```

### 3. Handle Special Characters

```csharp
public string SanitizeForPdf(string text)
{
    // HTML encode for template approach
    return System.Web.HttpUtility.HtmlEncode(text);
}

public string SanitizeForDirectReplacement(string text)
{
    // Remove characters that might cause issues
    return text
        .Replace("\r\n", " ")
        .Replace("\n", " ")
        .Replace("\t", " ");
}
```

---

## Recommendations

### Choose IronPDF for Find/Replace When:
- ✅ You need reliable text replacement in existing PDFs
- ✅ HTML template approach fits your workflow
- ✅ Pattern-based replacement is required
- ✅ Bulk processing of many documents
- ✅ Combined operations (replace + watermark + sign)

### Use HTML Templates When:
- ✅ Creating new documents from data
- ✅ Complex layouts with tables, images
- ✅ Perfect formatting control needed
- ✅ Razor views or other templating engines available

### Cannot Do Text Replacement:
- ❌ PDFSharp — No text replacement API
- ❌ QuestPDF — Generation only (use templates)
- ❌ PuppeteerSharp — Generation only (use templates)

---

## Related Tutorials

- **[HTML to PDF](html-to-pdf-csharp.md)** — Template-based generation
- **[PDF Redaction](pdf-redaction-csharp.md)** — Permanently remove text
- **[Extract Text](extract-text-from-pdf-csharp.md)** — Find content to replace
- **[Watermark PDFs](watermark-pdf-csharp.md)** — Add stamps after replacement
- **[Best PDF Libraries](best-pdf-libraries-dotnet-2025.md)** — Full comparison

---

### More Tutorials
- **[Beginner Tutorial](csharp-pdf-tutorial-beginners.md)** — First PDF in 5 minutes
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web template processing
- **[Cross-Platform](cross-platform-pdf-dotnet.md)** — Deploy template services
- **[IronPDF Guide](ironpdf/)** — Complete API documentation

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*
