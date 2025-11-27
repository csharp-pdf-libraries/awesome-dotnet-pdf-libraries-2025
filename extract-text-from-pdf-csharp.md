# Extract Text from PDF in C#: Complete Guide with OCR Support

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF

[![.NET](https://img.shields.io/badge/.NET-6%2B-512BD4)](https://dotnet.microsoft.com/)
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> Extracting text from PDFs is essential for search indexing, data migration, and content analysis. This guide covers both text-based and scanned PDFs, with library comparisons.

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Library Comparison](#library-comparison)
3. [Extract All Text](#extract-all-text)
4. [Page-by-Page Extraction](#page-by-page-extraction)
5. [Structured Extraction](#structured-extraction)
6. [OCR for Scanned PDFs](#ocr-for-scanned-pdfs)
7. [Performance Optimization](#performance-optimization)
8. [Common Use Cases](#common-use-cases)

---

## Quick Start

### Extract All Text with IronPDF

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
string allText = pdf.ExtractAllText();
Console.WriteLine(allText);
```

### Extract from Specific Page

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
string pageText = pdf.ExtractTextFromPage(0);  // First page
Console.WriteLine(pageText);
```

---

## Library Comparison

### Text Extraction Capabilities

| Library | Extract Text | By Page | Structured | OCR | Table Detection |
|---------|-------------|---------|-----------|-----|-----------------|
| **IronPDF** | ✅ Simple | ✅ | ✅ | ✅ (IronOCR) | ⚠️ |
| iText7 | ✅ | ✅ | ✅ | ❌ | ⚠️ |
| Aspose.PDF | ✅ | ✅ | ✅ | ❌ | ✅ |
| PDFSharp | ⚠️ Limited | ⚠️ | ❌ | ❌ | ❌ |
| pdfpig | ✅ | ✅ | ✅ | ❌ | ✅ |
| PuppeteerSharp | ❌ | ❌ | ❌ | ❌ | ❌ |
| QuestPDF | ❌ | ❌ | ❌ | ❌ | ❌ |

**Key insight:** Generation-only libraries cannot extract text at all.

### Code Complexity

**IronPDF — 2 lines:**
```csharp
var pdf = PdfDocument.FromFile("document.pdf");
string text = pdf.ExtractAllText();
```

**iText7 — 10+ lines:**
```csharp
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

using var reader = new PdfReader("document.pdf");
using var pdfDoc = new PdfDocument(reader);

var text = new StringBuilder();
for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
{
    text.AppendLine(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
}
```

---

## Extract All Text

### Basic Extraction

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("report.pdf");

// Get all text from entire document
string allText = pdf.ExtractAllText();

// Save to file
File.WriteAllText("extracted-text.txt", allText);
Console.WriteLine($"Extracted {allText.Length} characters");
```

### From URL

```csharp
using IronPdf;

// Download and extract
var pdf = PdfDocument.FromFile(new Uri("https://example.com/document.pdf"));
string text = pdf.ExtractAllText();
```

### From Byte Array

```csharp
using IronPdf;

byte[] pdfBytes = await httpClient.GetByteArrayAsync("https://example.com/doc.pdf");
var pdf = new PdfDocument(pdfBytes);
string text = pdf.ExtractAllText();
```

---

## Page-by-Page Extraction

### Single Page

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("book.pdf");

// Extract first page (0-indexed)
string firstPage = pdf.ExtractTextFromPage(0);

// Extract last page
string lastPage = pdf.ExtractTextFromPage(pdf.PageCount - 1);
```

### Page Range

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("report.pdf");
var pageTexts = new List<string>();

// Extract pages 5-10
for (int i = 4; i < 10 && i < pdf.PageCount; i++)
{
    string pageText = pdf.ExtractTextFromPage(i);
    pageTexts.Add($"=== Page {i + 1} ===\n{pageText}");
}

string combined = string.Join("\n\n", pageTexts);
```

### Process Each Page

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

for (int i = 0; i < pdf.PageCount; i++)
{
    string pageText = pdf.ExtractTextFromPage(i);

    // Process each page
    int wordCount = pageText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
    Console.WriteLine($"Page {i + 1}: {wordCount} words");
}
```

---

## Structured Extraction

### Extract with Layout

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("formatted-document.pdf");

// ExtractAllText preserves some layout information
string text = pdf.ExtractAllText();

// Lines are typically preserved
string[] lines = text.Split('\n');
foreach (string line in lines)
{
    if (!string.IsNullOrWhiteSpace(line))
    {
        Console.WriteLine(line.Trim());
    }
}
```

### Extract Tables (Basic)

```csharp
using IronPdf;
using System.Text.RegularExpressions;

var pdf = PdfDocument.FromFile("report-with-tables.pdf");
string text = pdf.ExtractAllText();

// Basic table detection by whitespace patterns
var lines = text.Split('\n');
var tableRows = new List<string[]>();

foreach (string line in lines)
{
    // Detect tabular data by multiple whitespace separators
    if (Regex.IsMatch(line, @"\s{2,}"))
    {
        var cells = Regex.Split(line.Trim(), @"\s{2,}");
        if (cells.Length > 1)
        {
            tableRows.Add(cells);
        }
    }
}

// Output as CSV
foreach (var row in tableRows)
{
    Console.WriteLine(string.Join(",", row.Select(c => $"\"{c}\"")));
}
```

### Extract Headings

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");
string text = pdf.ExtractAllText();

// Find lines that look like headings (all caps, short, followed by content)
var lines = text.Split('\n');
var headings = new List<string>();

for (int i = 0; i < lines.Length - 1; i++)
{
    string line = lines[i].Trim();

    // Heuristic: Short lines in all caps or title case
    if (line.Length > 0 && line.Length < 100)
    {
        if (line == line.ToUpper() || char.IsUpper(line[0]))
        {
            // Check if followed by content
            string nextLine = lines[i + 1].Trim();
            if (nextLine.Length > line.Length)
            {
                headings.Add(line);
            }
        }
    }
}

Console.WriteLine("Found headings:");
headings.ForEach(h => Console.WriteLine($"  - {h}"));
```

---

## OCR for Scanned PDFs

For scanned documents or image-based PDFs, you need OCR.

### Using IronOCR with IronPDF

```csharp
using IronOcr;
using IronPdf;

// First, convert PDF pages to images
var pdf = PdfDocument.FromFile("scanned-document.pdf");

// Use IronOCR for text recognition
var ocr = new IronTesseract();
var ocrInput = new OcrInput();

// Add PDF directly to OCR input
ocrInput.AddPdf("scanned-document.pdf");

// Perform OCR
var result = ocr.Read(ocrInput);
Console.WriteLine(result.Text);

// Or get text by page
foreach (var page in result.Pages)
{
    Console.WriteLine($"Page {page.PageNumber}: {page.Text}");
}
```

### Detect If OCR Is Needed

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("unknown-document.pdf");
string extractedText = pdf.ExtractAllText();

// If very little text extracted, likely scanned/image-based
bool needsOcr = extractedText.Trim().Length < 100;

if (needsOcr)
{
    Console.WriteLine("This PDF appears to be scanned. OCR required.");
    // Use IronOCR for extraction
}
else
{
    Console.WriteLine("Text-based PDF. Direct extraction successful.");
    Console.WriteLine(extractedText);
}
```

---

## Performance Optimization

### Large Documents

```csharp
using IronPdf;

// For very large documents, process in chunks
var pdf = PdfDocument.FromFile("large-document.pdf");

const int chunkSize = 50;
var allText = new StringBuilder();

for (int start = 0; start < pdf.PageCount; start += chunkSize)
{
    int end = Math.Min(start + chunkSize, pdf.PageCount);

    for (int i = start; i < end; i++)
    {
        allText.AppendLine(pdf.ExtractTextFromPage(i));
    }

    // Process chunk or write to disk
    Console.WriteLine($"Processed pages {start + 1}-{end}");
}
```

### Parallel Extraction

```csharp
using IronPdf;
using System.Collections.Concurrent;

var pdf = PdfDocument.FromFile("document.pdf");
var pageTexts = new ConcurrentDictionary<int, string>();

Parallel.For(0, pdf.PageCount, i =>
{
    string text = pdf.ExtractTextFromPage(i);
    pageTexts[i] = text;
});

// Combine in order
var orderedText = pageTexts
    .OrderBy(kvp => kvp.Key)
    .Select(kvp => kvp.Value);

string fullText = string.Join("\n", orderedText);
```

---

## Common Use Cases

### Search Indexing

```csharp
using IronPdf;

public class PdfIndexEntry
{
    public string FilePath { get; set; }
    public string Content { get; set; }
    public DateTime Indexed { get; set; }
}

public PdfIndexEntry IndexPdf(string filePath)
{
    var pdf = PdfDocument.FromFile(filePath);

    return new PdfIndexEntry
    {
        FilePath = filePath,
        Content = pdf.ExtractAllText(),
        Indexed = DateTime.UtcNow
    };
}

// Index all PDFs in directory
var entries = Directory.GetFiles("documents/", "*.pdf")
    .Select(IndexPdf)
    .ToList();
```

### Invoice Data Extraction

```csharp
using IronPdf;
using System.Text.RegularExpressions;

var pdf = PdfDocument.FromFile("invoice.pdf");
string text = pdf.ExtractAllText();

// Extract common invoice fields
var invoiceNumber = Regex.Match(text, @"Invoice\s*#?\s*:?\s*(\w+)", RegexOptions.IgnoreCase);
var totalAmount = Regex.Match(text, @"Total:?\s*\$?([\d,]+\.?\d*)", RegexOptions.IgnoreCase);
var dateMatch = Regex.Match(text, @"Date:?\s*([\d/\-]+)", RegexOptions.IgnoreCase);

Console.WriteLine($"Invoice: {invoiceNumber.Groups[1].Value}");
Console.WriteLine($"Total: ${totalAmount.Groups[1].Value}");
Console.WriteLine($"Date: {dateMatch.Groups[1].Value}");
```

### Resume Parsing

```csharp
using IronPdf;
using System.Text.RegularExpressions;

var pdf = PdfDocument.FromFile("resume.pdf");
string text = pdf.ExtractAllText();

// Extract email
var email = Regex.Match(text, @"[\w\.-]+@[\w\.-]+\.\w+");

// Extract phone
var phone = Regex.Match(text, @"\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}");

// Extract skills (look for common section)
var skillsSection = Regex.Match(text, @"Skills:?\s*(.*?)(?=Experience|Education|$)",
    RegexOptions.IgnoreCase | RegexOptions.Singleline);

Console.WriteLine($"Email: {email.Value}");
Console.WriteLine($"Phone: {phone.Value}");
Console.WriteLine($"Skills: {skillsSection.Groups[1].Value.Trim()}");
```

---

## Recommendations

### Choose IronPDF for Text Extraction When:
- ✅ You need simple API (2 lines)
- ✅ You're also generating/manipulating PDFs
- ✅ Combined with IronOCR for scanned documents
- ✅ Cross-platform support needed

### Choose pdfpig When:
- Budget is zero
- You need detailed position information
- Table extraction is primary use case

### Choose iText7 When:
- You need very specific text positioning
- Already using iText for other operations

### Cannot Extract Text:
- ❌ PuppeteerSharp — Generation only
- ❌ QuestPDF — Generation only
- ❌ wkhtmltopdf — Generation only

---

## Related Tutorials

- **[HTML to PDF](html-to-pdf-csharp.md)** — Generate searchable PDFs
- **[PDF/A Compliance](pdf-a-compliance-csharp.md)** — Create accessible, searchable PDFs
- **[Merge PDFs](merge-split-pdf-csharp.md)** — Combine documents before extraction
- **[Best PDF Libraries](best-pdf-libraries-dotnet-2025.md)** — Full library comparison

---

### More Tutorials
- **[PDF Redaction](pdf-redaction-csharp.md)** — Remove sensitive extracted data
- **[Find & Replace](pdf-find-replace-csharp.md)** — Modify text in PDFs
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web extraction services
- **[IronPDF Guide](ironpdf/)** — Full extraction API
- **[Aspose.PDF Guide](asposepdf/)** — Alternative extraction

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*
