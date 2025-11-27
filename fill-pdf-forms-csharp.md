# Fill PDF Forms in C#: AcroForms, XFA, and Flattening Guide

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF

[![.NET](https://img.shields.io/badge/.NET-6%2B-512BD4)](https://dotnet.microsoft.com/)
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> PDF forms enable data collection without manual data entry. This guide covers filling AcroForms, handling XFA forms, and flattening for distribution.

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Library Comparison](#library-comparison)
3. [Form Types Explained](#form-types-explained)
4. [Working with AcroForms](#working-with-acroforms)
5. [Reading Form Data](#reading-form-data)
6. [Flattening Forms](#flattening-forms)
7. [Creating Forms](#creating-forms)
8. [Common Use Cases](#common-use-cases)

---

## Quick Start

### Fill Form Fields with IronPDF

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("application-form.pdf");

// Fill text fields
pdf.Form.FindFormField("FirstName").Value = "John";
pdf.Form.FindFormField("LastName").Value = "Smith";
pdf.Form.FindFormField("Email").Value = "john.smith@example.com";

// Fill checkbox
pdf.Form.FindFormField("AgreeToTerms").Value = "Yes";

pdf.SaveAs("filled-application.pdf");
```

---

## Library Comparison

### Form Filling Capabilities

| Library | Fill AcroForms | Read Forms | Flatten | Create Forms | XFA Support |
|---------|---------------|-----------|---------|--------------|-------------|
| **IronPDF** | ✅ Simple | ✅ | ✅ | ✅ | ⚠️ Convert |
| iText7 | ✅ | ✅ | ✅ | ✅ | ✅ |
| Aspose.PDF | ✅ | ✅ | ✅ | ✅ | ✅ |
| PDFSharp | ⚠️ Limited | ⚠️ | ⚠️ | ❌ | ❌ |
| PuppeteerSharp | ❌ | ❌ | ❌ | ❌ | ❌ |
| QuestPDF | ❌ | ❌ | ❌ | ❌ | ❌ |

### Code Complexity

**IronPDF — 3 lines:**
```csharp
var pdf = PdfDocument.FromFile("form.pdf");
pdf.Form.FindFormField("Name").Value = "John";
pdf.SaveAs("filled.pdf");
```

**iText7 — 15+ lines:**
```csharp
using var reader = new PdfReader("form.pdf");
using var writer = new PdfWriter("filled.pdf");
using var pdfDoc = new PdfDocument(reader, writer);
var form = PdfAcroForm.GetAcroForm(pdfDoc, true);
form.GetField("Name").SetValue("John");
pdfDoc.Close();
```

---

## Form Types Explained

### AcroForms (Standard)

- Most common PDF form type
- Created in Adobe Acrobat, LibreOffice, etc.
- Static fields embedded in PDF
- Widely supported

### XFA Forms (Legacy)

- XML Forms Architecture
- Dynamic, data-driven forms
- Created in Adobe LiveCycle Designer
- **Deprecated by Adobe** — Being phased out
- Limited support in modern readers

### HTML Forms (IronPDF Approach)

- Generate form PDFs from HTML
- Full CSS styling
- Modern alternative to AcroForms

---

## Working with AcroForms

### List All Form Fields

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("form.pdf");

Console.WriteLine("Form Fields:");
foreach (var field in pdf.Form.FormFields)
{
    Console.WriteLine($"  Name: {field.Name}");
    Console.WriteLine($"  Type: {field.Type}");
    Console.WriteLine($"  Value: {field.Value}");
    Console.WriteLine($"  ReadOnly: {field.IsReadOnly}");
    Console.WriteLine();
}
```

### Fill Different Field Types

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("comprehensive-form.pdf");

// Text field
pdf.Form.FindFormField("FullName").Value = "Jane Doe";

// Multi-line text
pdf.Form.FindFormField("Address").Value = "123 Main St\nApt 4B\nNew York, NY 10001";

// Checkbox
pdf.Form.FindFormField("Subscribe").Value = "Yes";

// Radio button
pdf.Form.FindFormField("PaymentMethod").Value = "CreditCard";

// Dropdown/ComboBox
pdf.Form.FindFormField("Country").Value = "United States";

// Date field
pdf.Form.FindFormField("BirthDate").Value = "1990-01-15";

pdf.SaveAs("filled-form.pdf");
```

### Fill by Field Index

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("form.pdf");

// Fill by index when field names are unknown
pdf.Form.FormFields[0].Value = "First field value";
pdf.Form.FormFields[1].Value = "Second field value";

pdf.SaveAs("filled.pdf");
```

### Conditional Filling

```csharp
using IronPdf;

public void FillFormFromData(string formPath, Dictionary<string, string> data)
{
    var pdf = PdfDocument.FromFile(formPath);

    foreach (var field in pdf.Form.FormFields)
    {
        if (data.TryGetValue(field.Name, out string value))
        {
            field.Value = value;
        }
        else
        {
            Console.WriteLine($"Warning: No data for field '{field.Name}'");
        }
    }

    pdf.SaveAs(formPath.Replace(".pdf", "-filled.pdf"));
}

// Usage
var formData = new Dictionary<string, string>
{
    { "FirstName", "John" },
    { "LastName", "Doe" },
    { "Email", "john@example.com" }
};

FillFormFromData("application.pdf", formData);
```

---

## Reading Form Data

### Extract All Field Values

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("submitted-form.pdf");

var formData = new Dictionary<string, string>();

foreach (var field in pdf.Form.FormFields)
{
    formData[field.Name] = field.Value;
}

// Export to JSON
string json = System.Text.Json.JsonSerializer.Serialize(formData, new JsonSerializerOptions
{
    WriteIndented = true
});
File.WriteAllText("form-data.json", json);
```

### Process Submitted Forms

```csharp
using IronPdf;

public class FormSubmission
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public bool AgreeToTerms { get; set; }
}

public FormSubmission ExtractFormData(byte[] pdfBytes)
{
    var pdf = new PdfDocument(pdfBytes);

    return new FormSubmission
    {
        FirstName = pdf.Form.FindFormField("FirstName")?.Value,
        LastName = pdf.Form.FindFormField("LastName")?.Value,
        Email = pdf.Form.FindFormField("Email")?.Value,
        AgreeToTerms = pdf.Form.FindFormField("AgreeToTerms")?.Value == "Yes"
    };
}
```

---

## Flattening Forms

Flattening converts form fields to static content, preventing further editing.

### Flatten All Fields

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("filled-form.pdf");

// Flatten - converts fields to static text
pdf.Form.Flatten();

pdf.SaveAs("flattened-form.pdf");
```

### Flatten for Distribution

```csharp
using IronPdf;

public void PrepareForDistribution(string formPath, Dictionary<string, string> data)
{
    var pdf = PdfDocument.FromFile(formPath);

    // Fill all fields
    foreach (var kvp in data)
    {
        var field = pdf.Form.FindFormField(kvp.Key);
        if (field != null)
        {
            field.Value = kvp.Value;
        }
    }

    // Flatten to prevent editing
    pdf.Form.Flatten();

    // Add watermark
    pdf.ApplyWatermark("<p style='opacity:0.3;font-size:10pt;'>Final Copy - Do Not Edit</p>",
        VerticalAlignment.Bottom, HorizontalAlignment.Center);

    pdf.SaveAs(formPath.Replace(".pdf", "-final.pdf"));
}
```

### Selective Flattening

```csharp
using IronPdf;

var pdf = PdfDocument.FromFile("form.pdf");

// Only flatten specific fields
var fieldsToFlatten = new[] { "Signature", "Date", "ApproverName" };

foreach (var fieldName in fieldsToFlatten)
{
    var field = pdf.Form.FindFormField(fieldName);
    if (field != null)
    {
        field.IsReadOnly = true;  // Lock the field
    }
}

pdf.SaveAs("partially-locked.pdf");
```

---

## Creating Forms

### HTML Form to PDF

```csharp
using IronPdf;

var renderer = new ChromePdfRenderer();

string formHtml = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; padding: 40px; }
        .form-group { margin-bottom: 15px; }
        label { display: block; margin-bottom: 5px; font-weight: bold; }
        input[type='text'], select, textarea {
            width: 100%;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }
        .checkbox-group { display: flex; align-items: center; gap: 10px; }
    </style>
</head>
<body>
    <h1>Registration Form</h1>

    <div class='form-group'>
        <label for='name'>Full Name</label>
        <input type='text' id='name' name='FullName' />
    </div>

    <div class='form-group'>
        <label for='email'>Email Address</label>
        <input type='text' id='email' name='Email' />
    </div>

    <div class='form-group'>
        <label for='country'>Country</label>
        <select id='country' name='Country'>
            <option value=''>Select...</option>
            <option value='US'>United States</option>
            <option value='UK'>United Kingdom</option>
            <option value='CA'>Canada</option>
        </select>
    </div>

    <div class='form-group'>
        <label for='comments'>Comments</label>
        <textarea id='comments' name='Comments' rows='4'></textarea>
    </div>

    <div class='form-group checkbox-group'>
        <input type='checkbox' id='terms' name='AgreeToTerms' />
        <label for='terms'>I agree to the terms and conditions</label>
    </div>
</body>
</html>";

var pdf = renderer.RenderHtmlAsPdf(formHtml);
pdf.SaveAs("registration-form.pdf");
```

---

## Common Use Cases

### Tax Form Filling

```csharp
using IronPdf;

public void FillTaxForm(string formPath, TaxData data)
{
    var pdf = PdfDocument.FromFile(formPath);

    // Personal information
    pdf.Form.FindFormField("FirstName").Value = data.FirstName;
    pdf.Form.FindFormField("LastName").Value = data.LastName;
    pdf.Form.FindFormField("SSN").Value = data.SSN;

    // Income
    pdf.Form.FindFormField("WagesLine1").Value = data.Wages.ToString("F2");
    pdf.Form.FindFormField("InterestLine2").Value = data.Interest.ToString("F2");
    pdf.Form.FindFormField("DividendsLine3").Value = data.Dividends.ToString("F2");

    // Calculate total
    decimal total = data.Wages + data.Interest + data.Dividends;
    pdf.Form.FindFormField("TotalIncome").Value = total.ToString("F2");

    // Sign and date
    pdf.Form.FindFormField("SignatureDate").Value = DateTime.Now.ToString("MM/dd/yyyy");

    pdf.SaveAs($"tax-return-{data.LastName}-{DateTime.Now.Year}.pdf");
}
```

### Application Processing

```csharp
using IronPdf;

public class ApplicationProcessor
{
    public void ProcessApplication(byte[] submittedPdf, string applicantId)
    {
        var pdf = new PdfDocument(submittedPdf);

        // Extract submitted data
        var data = new Dictionary<string, string>();
        foreach (var field in pdf.Form.FormFields)
        {
            data[field.Name] = field.Value;
        }

        // Add internal tracking fields
        pdf.Form.FindFormField("ApplicationID").Value = applicantId;
        pdf.Form.FindFormField("ReceivedDate").Value = DateTime.Now.ToString("yyyy-MM-dd");
        pdf.Form.FindFormField("Status").Value = "Pending Review";

        // Save with tracking info
        pdf.SaveAs($"applications/{applicantId}.pdf");

        // Log to database
        SaveToDatabase(applicantId, data);
    }
}
```

### Contract Generation

```csharp
using IronPdf;

public void GenerateContract(string templatePath, ContractData contract)
{
    var pdf = PdfDocument.FromFile(templatePath);

    // Party information
    pdf.Form.FindFormField("PartyAName").Value = contract.PartyA.Name;
    pdf.Form.FindFormField("PartyAAddress").Value = contract.PartyA.Address;
    pdf.Form.FindFormField("PartyBName").Value = contract.PartyB.Name;
    pdf.Form.FindFormField("PartyBAddress").Value = contract.PartyB.Address;

    // Contract details
    pdf.Form.FindFormField("ContractAmount").Value = contract.Amount.ToString("C");
    pdf.Form.FindFormField("StartDate").Value = contract.StartDate.ToString("MMMM dd, yyyy");
    pdf.Form.FindFormField("EndDate").Value = contract.EndDate.ToString("MMMM dd, yyyy");
    pdf.Form.FindFormField("Terms").Value = contract.TermsDescription;

    // Leave signature fields empty for signing
    // pdf.Form.FindFormField("PartyASignature") - left blank
    // pdf.Form.FindFormField("PartyBSignature") - left blank

    pdf.SaveAs($"contracts/contract-{contract.ContractNumber}.pdf");
}
```

---

## Recommendations

### Choose IronPDF for Forms When:
- ✅ Simple API for filling
- ✅ Combined with HTML-to-PDF generation
- ✅ Need flattening for distribution
- ✅ Cross-platform deployment

### Choose iText7 When:
- XFA form support is critical
- Very complex form manipulation needed
- Already using iText ecosystem

### Cannot Fill Forms:
- ❌ PuppeteerSharp — No form support
- ❌ QuestPDF — No form support
- ❌ wkhtmltopdf — No form support

---

## Related Tutorials

- **[Digital Signatures](digital-signatures-pdf-csharp.md)** — Sign filled forms
- **[HTML to PDF](html-to-pdf-csharp.md)** — Create form PDFs from HTML
- **[Extract Text](extract-text-from-pdf-csharp.md)** — Extract form data
- **[Best PDF Libraries](best-pdf-libraries-dotnet-2025.md)** — Full comparison

---

### More Tutorials
- **[PDF Redaction](pdf-redaction-csharp.md)** — Redact form fields
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web form processing
- **[Cross-Platform](cross-platform-pdf-dotnet.md)** — Deploy form services
- **[IronPDF Guide](ironpdf/)** — Full form API
- **[iText Guide](itext-itextsharp/)** — Advanced form operations

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*
