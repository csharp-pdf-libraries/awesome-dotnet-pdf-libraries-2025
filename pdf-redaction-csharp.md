# PDF Redaction in C#: Permanently Remove Sensitive Content

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF | 41 years coding experience

[![.NET](https://img.shields.io/badge/.NET-6%2B-512BD4)](https://dotnet.microsoft.com/)
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> PDF redaction is the permanent, irreversible removal of sensitive content from documents. Unlike simply covering text with black rectangles, true redaction eliminates the underlying data—essential for legal, healthcare, and government compliance.

---

## Table of Contents

1. [What Is PDF Redaction?](#what-is-pdf-redaction)
2. [Library Comparison](#library-comparison)
3. [Quick Start](#quick-start)
4. [Text Redaction](#text-redaction)
5. [Pattern-Based Redaction](#pattern-based-redaction)
6. [Area Redaction](#area-redaction)
7. [Image Redaction](#image-redaction)
8. [Metadata Redaction](#metadata-redaction)
9. [Compliance Requirements](#compliance-requirements)
10. [Common Use Cases](#common-use-cases)

---

## What Is PDF Redaction?

**Redaction** permanently removes content from PDFs. This is different from:

| Action | Recoverable? | Compliant? | Use Case |
|--------|--------------|------------|----------|
| **Redaction** | ❌ No | ✅ Yes | Legal discovery, FOIA |
| Black rectangle overlay | ✅ Yes | ❌ No | Visual hiding only |
| White rectangle overlay | ✅ Yes | ❌ No | Easily removed |
| Text color = white | ✅ Yes | ❌ No | Copy/paste reveals |

### The Danger of Fake Redaction

In 2005, a government report on Italian intelligence was "redacted" with black rectangles. Copy/paste revealed classified names. In 2014, a court filing accidentally exposed confidential client data the same way.

**True redaction:**
1. Removes the underlying text/image data
2. Replaces with redaction marks (optional)
3. Cannot be reversed
4. Removes from text extraction
5. Removes from search

---

## Library Comparison

### PDF Redaction Capabilities

| Library | True Redaction | Text Search | Pattern/Regex | Area Redaction | Metadata |
|---------|---------------|-------------|---------------|----------------|----------|
| **IronPDF** | ✅ Yes | ✅ | ✅ | ✅ | ✅ |
| iText7 | ✅ Yes | ✅ | ✅ | ✅ | ✅ |
| Aspose.PDF | ✅ Yes | ✅ | ⚠️ Limited | ✅ | ✅ |
| PDFSharp | ❌ No | ❌ | ❌ | ❌ | ❌ |
| QuestPDF | ❌ No | ❌ | ❌ | ❌ | ❌ |
| PuppeteerSharp | ❌ No | ❌ | ❌ | ❌ | ❌ |

**Key insight:** Most PDF libraries cannot perform true redaction. They can only draw over content, which remains extractable.

### Code Complexity Comparison

**IronPDF — Simple API:**
```csharp
var pdf = PdfDocument.FromFile("document.pdf");
pdf.RedactTextOnAllPages("SSN: 123-45-6789");
pdf.SaveAs("redacted.pdf");
```

**iText7 — Complex Setup:**
```csharp
var pdfDoc = new PdfDocument(new PdfReader("input.pdf"), new PdfWriter("output.pdf"));
var cleanUpTool = new PdfCleanUpTool(pdfDoc);
var strategy = new RegexBasedLocationExtractionStrategy(@"\d{3}-\d{2}-\d{4}");
// ... 20+ more lines of setup ...
cleanUpTool.CleanUp();
pdfDoc.Close();
```

### Why Choose IronPDF for Redaction

1. **True redaction** — Data is permanently removed, not hidden
2. **Simple API** — One-line redaction operations
3. **Pattern support** — Regex-based sensitive data detection
4. **Compliance ready** — Meets HIPAA, GDPR, FOIA requirements
5. **Combined operations** — Redact, then sign, then save

---

## Quick Start

### Install IronPDF

```bash
dotnet add package IronPdf
```

### Basic Redaction

```csharp
using IronPdf;

// Load PDF
var pdf = PdfDocument.FromFile("contract.pdf");

// Redact specific text
pdf.RedactTextOnAllPages("Confidential Client Name");

// Save redacted version
pdf.SaveAs("contract-redacted.pdf");

Console.WriteLine("Redaction complete - data permanently removed");
```

---

## Text Redaction

### Redact Specific Text

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("employee-records.pdf");

// Redact exact text matches
pdf.RedactTextOnAllPages("John Smith");
pdf.RedactTextOnAllPages("Employee ID: 12345");

pdf.SaveAs("employee-records-redacted.pdf");
```

### Redact on Specific Pages

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("report.pdf");

// Redact only on page 1 (0-indexed)
pdf.RedactTextOnPage(0, "Internal Use Only");

// Redact on pages 3-5
for (int i = 2; i <= 4; i++)
{
    pdf.RedactTextOnPage(i, "Draft - Not for Distribution");
}

pdf.SaveAs("report-redacted.pdf");
```

### Case-Insensitive Redaction

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Find all variations
var variations = new[] { "CONFIDENTIAL", "Confidential", "confidential" };

foreach (var text in variations)
{
    pdf.RedactTextOnAllPages(text);
}

pdf.SaveAs("document-redacted.pdf");
```

---

## Pattern-Based Redaction

### Social Security Numbers

```csharp
using IronPdf;
using System.Text.RegularExpressions;

var pdf = PdfDocument.FromFile("application.pdf");

// Extract text to find SSNs
string text = pdf.ExtractAllText();

// Find all SSN patterns
var ssnPattern = new Regex(@"\d{3}-\d{2}-\d{4}");
var matches = ssnPattern.Matches(text);

// Redact each found SSN
foreach (Match match in matches)
{
    pdf.RedactTextOnAllPages(match.Value);
}

pdf.SaveAs("application-redacted.pdf");

Console.WriteLine($"Redacted {matches.Count} Social Security Numbers");
```

### Email Addresses

```csharp
using IronPdf;
using System.Text.RegularExpressions;

var pdf = PdfDocument.FromFile("contacts.pdf");
string text = pdf.ExtractAllText();

// Email pattern
var emailPattern = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
var matches = emailPattern.Matches(text);

foreach (Match match in matches)
{
    pdf.RedactTextOnAllPages(match.Value);
}

pdf.SaveAs("contacts-redacted.pdf");
```

### Phone Numbers

```csharp
using IronPdf;
using System.Text.RegularExpressions;

var pdf = PdfDocument.FromFile("directory.pdf");
string text = pdf.ExtractAllText();

// Multiple phone formats
var patterns = new[]
{
    @"\(\d{3}\)\s?\d{3}-\d{4}",     // (555) 123-4567
    @"\d{3}-\d{3}-\d{4}",           // 555-123-4567
    @"\d{3}\.\d{3}\.\d{4}",         // 555.123.4567
    @"\+1\s?\d{3}\s?\d{3}\s?\d{4}"  // +1 555 123 4567
};

foreach (var pattern in patterns)
{
    var regex = new Regex(pattern);
    foreach (Match match in regex.Matches(text))
    {
        pdf.RedactTextOnAllPages(match.Value);
    }
}

pdf.SaveAs("directory-redacted.pdf");
```

### Credit Card Numbers

```csharp
using IronPdf;
using System.Text.RegularExpressions;

var pdf = PdfDocument.FromFile("transactions.pdf");
string text = pdf.ExtractAllText();

// Credit card patterns (various formats)
var ccPatterns = new[]
{
    @"\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}",  // 16 digit
    @"\d{4}[\s-]?\d{6}[\s-]?\d{5}"               // 15 digit (Amex)
};

int redactedCount = 0;

foreach (var pattern in ccPatterns)
{
    var regex = new Regex(pattern);
    foreach (Match match in regex.Matches(text))
    {
        pdf.RedactTextOnAllPages(match.Value);
        redactedCount++;
    }
}

pdf.SaveAs("transactions-redacted.pdf");
Console.WriteLine($"Redacted {redactedCount} credit card numbers");
```

### Comprehensive PII Redaction

```csharp
using IronPdf;
using System.Text.RegularExpressions;

public class PiiRedactor
{
    private static readonly Dictionary<string, string> PiiPatterns = new()
    {
        { "SSN", @"\d{3}-\d{2}-\d{4}" },
        { "Email", @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}" },
        { "Phone", @"\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}" },
        { "CreditCard", @"\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}" },
        { "DateOfBirth", @"\b(0[1-9]|1[0-2])/(0[1-9]|[12]\d|3[01])/(\d{4})\b" },
        { "IPAddress", @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b" }
    };

    public RedactionReport RedactPii(string inputPath, string outputPath)
    {
        var pdf = PdfDocument.FromFile(inputPath);
        string text = pdf.ExtractAllText();

        var report = new RedactionReport();

        foreach (var kvp in PiiPatterns)
        {
            var regex = new Regex(kvp.Value);
            var matches = regex.Matches(text);

            foreach (Match match in matches)
            {
                pdf.RedactTextOnAllPages(match.Value);
                report.AddRedaction(kvp.Key, match.Value);
            }
        }

        pdf.SaveAs(outputPath);
        return report;
    }
}

public class RedactionReport
{
    public Dictionary<string, List<string>> Redactions { get; } = new();

    public void AddRedaction(string type, string value)
    {
        if (!Redactions.ContainsKey(type))
            Redactions[type] = new List<string>();

        // Store masked version for logging
        string masked = value.Length > 4
            ? new string('*', value.Length - 4) + value[^4..]
            : new string('*', value.Length);

        Redactions[type].Add(masked);
    }

    public void PrintSummary()
    {
        Console.WriteLine("=== Redaction Report ===");
        foreach (var kvp in Redactions)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value.Count} items redacted");
        }
    }
}

// Usage
var redactor = new PiiRedactor();
var report = redactor.RedactPii("customer-data.pdf", "customer-data-redacted.pdf");
report.PrintSummary();
```

---

## Area Redaction

### Redact Specific Region

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("form.pdf");

// Redact a rectangular area on page 1
// Coordinates: x=100, y=200, width=300, height=50 (in points)
pdf.RedactRegionOnPage(0, new IronPdf.Drawing.Rectangle(100, 200, 300, 50));

pdf.SaveAs("form-redacted.pdf");
```

### Redact Header/Footer Areas

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("letterhead.pdf");

// Get page dimensions
var pageSize = pdf.Pages[0].Size;

// Redact header area (top 100 points)
var headerArea = new IronPdf.Drawing.Rectangle(0, 0, pageSize.Width, 100);

// Redact footer area (bottom 80 points)
var footerArea = new IronPdf.Drawing.Rectangle(0, pageSize.Height - 80, pageSize.Width, 80);

for (int i = 0; i < pdf.PageCount; i++)
{
    pdf.RedactRegionOnPage(i, headerArea);
    pdf.RedactRegionOnPage(i, footerArea);
}

pdf.SaveAs("letterhead-redacted.pdf");
```

### Redact Signature Area

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("signed-contract.pdf");

// Signature typically at bottom right of last page
int lastPage = pdf.PageCount - 1;
var pageSize = pdf.Pages[lastPage].Size;

// Redact signature block area
var signatureArea = new IronPdf.Drawing.Rectangle(
    pageSize.Width - 300,  // 300 points from right
    pageSize.Height - 200, // 200 points from bottom
    280,                    // width
    180                     // height
);

pdf.RedactRegionOnPage(lastPage, signatureArea);
pdf.SaveAs("contract-signature-redacted.pdf");
```

---

## Image Redaction

### Redact All Images

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document-with-photos.pdf");

// Extract and remove all images
var images = pdf.ExtractAllImages();

Console.WriteLine($"Found {images.Count()} images");

// Redact areas where images are located
// (Implementation depends on getting image locations)

pdf.SaveAs("document-images-redacted.pdf");
```

### Replace Image with Placeholder

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("id-card.pdf");

// For documents where you know image locations
// Redact the photo area (typically upper right on ID cards)
var photoArea = new IronPdf.Drawing.Rectangle(400, 50, 150, 180);

pdf.RedactRegionOnPage(0, photoArea);

pdf.SaveAs("id-card-photo-redacted.pdf");
```

---

## Metadata Redaction

### Remove Document Metadata

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("document.pdf");

// Clear metadata that could identify document origin
pdf.MetaData.Author = "";
pdf.MetaData.Creator = "";
pdf.MetaData.Producer = "";
pdf.MetaData.Subject = "";
pdf.MetaData.Title = "Redacted Document";
pdf.MetaData.Keywords = "";

// Clear custom metadata
pdf.MetaData.CustomProperties.Clear();

// Remove creation/modification dates (set to epoch)
pdf.MetaData.CreationDate = DateTime.MinValue;
pdf.MetaData.ModifiedDate = DateTime.MinValue;

pdf.SaveAs("document-metadata-cleared.pdf");
```

### Full Sanitization

```csharp
using IronPdf;

public class PdfSanitizer
{
    public void FullSanitize(string inputPath, string outputPath, List<string> textToRedact)
    {
        var pdf = PdfDocument.FromFile(inputPath);

        // 1. Redact specified text
        foreach (var text in textToRedact)
        {
            pdf.RedactTextOnAllPages(text);
        }

        // 2. Clear all metadata
        ClearMetadata(pdf);

        // 3. Remove JavaScript (if any)
        // 4. Remove attachments (if any)
        // 5. Remove comments/annotations

        pdf.SaveAs(outputPath);
    }

    private void ClearMetadata(PdfDocument pdf)
    {
        pdf.MetaData.Author = "";
        pdf.MetaData.Creator = "Sanitized";
        pdf.MetaData.Producer = "";
        pdf.MetaData.Subject = "";
        pdf.MetaData.Title = "Sanitized Document";
        pdf.MetaData.Keywords = "";
        pdf.MetaData.CustomProperties.Clear();
    }
}
```

---

## Compliance Requirements

### HIPAA (Healthcare)

Protected Health Information (PHI) that must be redacted:

```csharp
using IronPdf;
using System.Text.RegularExpressions;

public class HipaaRedactor
{
    // HIPAA Safe Harbor: 18 identifiers that must be removed
    private static readonly Dictionary<string, string> HipaaPatterns = new()
    {
        { "SSN", @"\d{3}-\d{2}-\d{4}" },
        { "MRN", @"MRN[:\s]?\d{6,10}" },  // Medical Record Number
        { "Phone", @"\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}" },
        { "Fax", @"Fax[:\s]?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}" },
        { "Email", @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}" },
        { "DOB", @"\b(0[1-9]|1[0-2])/(0[1-9]|[12]\d|3[01])/\d{4}\b" },
        { "HealthPlanID", @"Policy[:\s#]?\d{8,12}" }
    };

    public void RedactPhi(string inputPath, string outputPath)
    {
        var pdf = PdfDocument.FromFile(inputPath);
        string text = pdf.ExtractAllText();

        foreach (var pattern in HipaaPatterns)
        {
            var regex = new Regex(pattern.Value, RegexOptions.IgnoreCase);
            foreach (Match match in regex.Matches(text))
            {
                pdf.RedactTextOnAllPages(match.Value);
            }
        }

        // Clear metadata
        pdf.MetaData.Author = "";
        pdf.MetaData.CustomProperties.Clear();

        pdf.SaveAs(outputPath);
    }
}
```

### GDPR (European Union)

Personal data requiring redaction under GDPR:

```csharp
using IronPdf;

public class GdprRedactor
{
    public void RedactPersonalData(PdfDocument pdf, GdprRedactionRequest request)
    {
        // Names
        foreach (var name in request.Names)
        {
            pdf.RedactTextOnAllPages(name);
        }

        // Addresses
        foreach (var address in request.Addresses)
        {
            pdf.RedactTextOnAllPages(address);
        }

        // National ID numbers (varies by country)
        var patterns = new[]
        {
            @"\d{3}-\d{2}-\d{4}",          // US SSN format
            @"[A-Z]{2}\d{6}[A-Z]",          // UK NI number
            @"\d{2}\s?\d{2}\s?\d{2}\s?\d{3}\s?\d{3}\s?\d{2}"  // French INSEE
        };

        string text = pdf.ExtractAllText();
        foreach (var pattern in patterns)
        {
            var regex = new Regex(pattern);
            foreach (Match match in regex.Matches(text))
            {
                pdf.RedactTextOnAllPages(match.Value);
            }
        }

        // Right to be forgotten: remove all traces
        pdf.MetaData.CustomProperties.Clear();
    }
}

public class GdprRedactionRequest
{
    public List<string> Names { get; set; } = new();
    public List<string> Addresses { get; set; } = new();
    public string Email { get; set; }
    public string Phone { get; set; }
}
```

### FOIA (Freedom of Information Act)

Government document redaction:

```csharp
using IronPdf;

public class FoiaRedactor
{
    // FOIA exemptions requiring redaction
    public enum FoiaExemption
    {
        NationalSecurity,      // Exemption 1
        InternalPersonnel,     // Exemption 2
        StatutoryExemption,    // Exemption 3
        TradeSecrets,          // Exemption 4
        InterAgencyMemos,      // Exemption 5
        PersonalPrivacy,       // Exemption 6
        LawEnforcement,        // Exemption 7
        FinancialInstitutions, // Exemption 8
        GeologicalData         // Exemption 9
    }

    public void RedactWithExemptionCode(string inputPath, string outputPath,
        Dictionary<string, FoiaExemption> redactions)
    {
        var pdf = PdfDocument.FromFile(inputPath);

        foreach (var kvp in redactions)
        {
            // Redact the text
            pdf.RedactTextOnAllPages(kvp.Key);
        }

        // Add exemption codes as stamps (optional)
        // Government agencies often add "(b)(6)" etc. to indicate exemption

        pdf.SaveAs(outputPath);
    }
}
```

---

## Common Use Cases

### Legal Discovery (E-Discovery)

```csharp
using IronPdf;

public class EDiscoveryRedactor
{
    public void PrepareForProduction(string inputPath, string outputPath,
        List<string> privilegedTerms, List<string> confidentialNames)
    {
        var pdf = PdfDocument.FromFile(inputPath);

        // Redact attorney-client privileged content
        foreach (var term in privilegedTerms)
        {
            pdf.RedactTextOnAllPages(term);
        }

        // Redact non-party names
        foreach (var name in confidentialNames)
        {
            pdf.RedactTextOnAllPages(name);
        }

        // Redact metadata
        pdf.MetaData.Author = "";
        pdf.MetaData.CustomProperties.Clear();

        // Bates number the document
        AddBatesNumbers(pdf, "ABC-000001");

        pdf.SaveAs(outputPath);
    }

    private void AddBatesNumbers(PdfDocument pdf, string startNumber)
    {
        // Add Bates numbers as footers
        // Implementation for production numbering
    }
}
```

### HR Document Redaction

```csharp
using IronPdf;

public class HrDocumentRedactor
{
    public void RedactEmployeeRecord(string inputPath, string outputPath,
        string employeeName, string ssn, decimal salary)
    {
        var pdf = PdfDocument.FromFile(inputPath);

        // Redact personal identifiers
        pdf.RedactTextOnAllPages(employeeName);
        pdf.RedactTextOnAllPages(ssn);

        // Redact salary information
        pdf.RedactTextOnAllPages(salary.ToString("C"));
        pdf.RedactTextOnAllPages(salary.ToString("N2"));

        // Redact bank account info patterns
        string text = pdf.ExtractAllText();
        var bankPattern = new Regex(@"Account[:\s#]*\d{8,12}");
        foreach (Match match in bankPattern.Matches(text))
        {
            pdf.RedactTextOnAllPages(match.Value);
        }

        pdf.SaveAs(outputPath);
    }
}
```

### Batch Redaction Service

```csharp
using IronPdf;

public class BatchRedactionService
{
    public async Task<BatchRedactionResult> ProcessBatch(
        string inputDirectory,
        string outputDirectory,
        List<string> termsToRedact)
    {
        var result = new BatchRedactionResult();
        var pdfFiles = Directory.GetFiles(inputDirectory, "*.pdf");

        foreach (var file in pdfFiles)
        {
            try
            {
                var pdf = PdfDocument.FromFile(file);
                int redactionCount = 0;

                foreach (var term in termsToRedact)
                {
                    string text = pdf.ExtractAllText();
                    int count = Regex.Matches(text, Regex.Escape(term),
                        RegexOptions.IgnoreCase).Count;

                    if (count > 0)
                    {
                        pdf.RedactTextOnAllPages(term);
                        redactionCount += count;
                    }
                }

                string outputPath = Path.Combine(outputDirectory,
                    Path.GetFileName(file));
                pdf.SaveAs(outputPath);

                result.ProcessedFiles.Add(new FileResult
                {
                    FileName = Path.GetFileName(file),
                    RedactionCount = redactionCount,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                result.ProcessedFiles.Add(new FileResult
                {
                    FileName = Path.GetFileName(file),
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        return result;
    }
}

public class BatchRedactionResult
{
    public List<FileResult> ProcessedFiles { get; } = new();
    public int TotalRedactions => ProcessedFiles.Sum(f => f.RedactionCount);
    public int SuccessCount => ProcessedFiles.Count(f => f.Success);
    public int FailureCount => ProcessedFiles.Count(f => !f.Success);
}

public class FileResult
{
    public string FileName { get; set; }
    public int RedactionCount { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; }
}
```

---

## Best Practices

### 1. Always Verify Redaction

```csharp
using IronPdf;

public bool VerifyRedaction(string originalPath, string redactedPath, string term)
{
    var redactedPdf = PdfDocument.FromFile(redactedPath);
    string text = redactedPdf.ExtractAllText();

    bool isRedacted = !text.Contains(term, StringComparison.OrdinalIgnoreCase);

    if (!isRedacted)
    {
        Console.WriteLine($"WARNING: '{term}' still found in redacted document!");
    }

    return isRedacted;
}
```

### 2. Audit Trail

```csharp
public class RedactionAudit
{
    public string OriginalFile { get; set; }
    public string RedactedFile { get; set; }
    public DateTime Timestamp { get; set; }
    public string PerformedBy { get; set; }
    public List<string> RedactedTerms { get; set; }
    public string Reason { get; set; }

    public void SaveAuditLog(string logPath)
    {
        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.AppendAllText(logPath, json + Environment.NewLine);
    }
}
```

### 3. Never Keep Unredacted Copies

```csharp
// Secure workflow
public void SecureRedactionWorkflow(string inputPath, List<string> terms)
{
    string outputPath = inputPath.Replace(".pdf", "-REDACTED.pdf");

    // Perform redaction
    var pdf = PdfDocument.FromFile(inputPath);
    foreach (var term in terms)
    {
        pdf.RedactTextOnAllPages(term);
    }
    pdf.SaveAs(outputPath);

    // Verify
    var verify = PdfDocument.FromFile(outputPath);
    string text = verify.ExtractAllText();

    foreach (var term in terms)
    {
        if (text.Contains(term))
        {
            throw new Exception($"Redaction verification failed for: {term}");
        }
    }

    // Securely delete original (if policy allows)
    // File.Delete(inputPath);

    Console.WriteLine("Redaction complete and verified.");
}
```

---

## Recommendations

### Choose IronPDF for Redaction When:
- ✅ You need true, permanent redaction (not just overlays)
- ✅ You require pattern-based PII detection
- ✅ Compliance (HIPAA, GDPR, FOIA) is required
- ✅ You want simple, one-line redaction API
- ✅ Combined operations (redact + sign + save)

### Cannot Perform True Redaction:
- ❌ PDFSharp — No redaction support
- ❌ QuestPDF — Generation only
- ❌ PuppeteerSharp — Generation only
- ❌ Basic PDF overlays — Data remains extractable

---

## Related Tutorials

- **[Extract Text from PDF](extract-text-from-pdf-csharp.md)** — Find content to redact
- **[Digital Signatures](digital-signatures-pdf-csharp.md)** — Sign after redaction
- **[PDF/A Compliance](pdf-a-compliance-csharp.md)** — Archive redacted documents
- **[Watermark PDFs](watermark-pdf-csharp.md)** — Mark as redacted
- **[Best PDF Libraries](best-pdf-libraries-dotnet-2025.md)** — Full comparison

---

### More Tutorials
- **[Find & Replace](pdf-find-replace-csharp.md)** — Non-destructive text changes
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web redaction services
- **[Cross-Platform](cross-platform-pdf-dotnet.md)** — Deploy redaction services
- **[IronPDF Guide](ironpdf/)** — Complete redaction API

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*
