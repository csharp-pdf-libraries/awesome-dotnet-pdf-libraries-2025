# How Can I Add Digital and Handwritten Signatures to PDFs in C#?

Adding digital or visual signatures to PDFs in C# is easier than you might think—and can dramatically streamline contract approvals, document workflows, and compliance. In this FAQ, you'll learn how to create cryptographic signatures, overlay handwritten signature images, set up fillable signature fields, and handle advanced requirements like batch signing, certificate management, and regulatory compliance. All examples use [IronPDF](https://ironpdf.com), a popular C# PDF library built for developers.

---

## What Are the Main Types of PDF Signatures I Can Use in C#?

PDF signatures generally fall into three categories, and you can use them individually or in combination depending on your needs:

1. **Digital (Cryptographic) Signatures:**  
   These are invisible but securely bind your identity to the PDF using a digital certificate. They provide the strongest legal proof and tamper detection.

2. **Visible Signature Images:**  
   A scanned or drawn image of your signature, placed visually on the page—just like pen on paper.

3. **Interactive Signature Fields:**  
   Blank fields that recipients can fill in later with their own digital signature, using Adobe Reader or similar tools.

Here’s a practical C# snippet using IronPDF for all three:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using IronPdf.Forms;
using System.Drawing;

// Load an existing PDF
var pdf = PdfDocument.FromFile("document.pdf");

// 1. Digitally sign (invisible)
pdf.SignWithFile("mycert.pfx", "certPassword");

// 2. Add a visible signature image
using (var signImg = new Bitmap("signature.png"))
{
    pdf.DrawBitmap(signImg, 400, 120, 0, 180, 60);
}

// 3. Insert a signature field for others to sign
var sigField = new SignatureFormField
{
    Name = "recipient_signature",
    PageIndex = 0,
    X = 100,
    Y = 180,
    Width = 220,
    Height = 60
};
pdf.Form.Fields.Add(sigField);

pdf.SaveAs("signed-and-ready.pdf");
```

If you're interested in generating PDFs from sources like XML or XAML before signing, check out [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md) and [How can I generate PDF from XAML in .NET MAUI?](xaml-to-pdf-maui-csharp.md).

---

## How Do I Digitally Sign a PDF with a Certificate in C#?

Digitally signing a PDF attaches cryptographic proof of authorship and ensures tamper detection. Here’s how to do it quickly with a PFX certificate:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf

var pdf = PdfDocument.FromFile("contract.pdf");
pdf.SignWithFile("signing-cert.pfx", "password123");
pdf.SaveAs("contract-signed.pdf");
```

### What Happens When I Digitally Sign a PDF?

IronPDF creates a secure hash of your document, encrypts it with your private key, and embeds it in the PDF. If anyone changes the PDF after signing, the signature becomes invalid.

### Where Can I Get a Certificate for PDF Signing?

To digitally sign PDFs, you need a certificate (PFX file) and its password. There are two main options:

#### Should I Buy a Certificate Authority (CA)-Signed Certificate?

- **Best for:** External contracts, customer-facing documents, legal and compliance needs.
- **Vendors:** DigiCert, GlobalSign, Sectigo, etc.
- **Pros:** Trusted by Adobe and most major PDF viewers.
- **Cons:** Involves identity verification and yearly costs.

#### Is a Self-Signed Certificate Sufficient?

- **Best for:** Internal use, testing, development, or low-risk documents.
- **Pros:** Free and instant to generate.
- **Cons:** Not trusted by recipients outside your organization (Adobe will show a warning).

##### How Do I Create a Self-Signed Certificate in Windows?

Use PowerShell to create your own certificate:

```powershell
$cert = New-SelfSignedCertificate -Subject "CN=MyApp" -Type CodeSigningCert
Export-PfxCertificate -Cert $cert -FilePath mycert.pfx -Password (ConvertTo-SecureString "password123" -AsPlainText -Force)
```

For production, always prefer a CA-issued cert. For more about PDF generation and signing from files, see [How do I convert an HTML file to PDF in C#?](html-file-to-pdf-csharp.md).

---

## When Should I Use Self-Signed Versus CA-Issued Certificates?

**Use self-signed certificates** for internal documents, prototypes, or development scenarios—just remember recipients will see "untrusted" warnings unless they manually trust the certificate.

**Use CA-signed certificates** whenever you send signed PDFs to clients, partners, or for legal record-keeping. It's the only way to guarantee trust in mainstream PDF viewers without extra steps.

---

## How Can I Add a Handwritten Signature Image to a PDF?

Stamping a signature image onto a PDF is perfect when you want a visible, familiar "ink" look, even if you don’t need full cryptographic proof.

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using System.Drawing;

var pdf = PdfDocument.FromFile("report.pdf");
using (var handSig = new Bitmap("my-signature.png"))
{
    pdf.DrawBitmap(handSig, x: 350, y: 80, pageIndex: 0, width: 160, height: 60);
}
pdf.SaveAs("report-with-visual-signature.pdf");
```

### When Is a Visual Signature Image Enough?

- For low-risk approvals and internal records
- Personal projects or informal agreements
- When stakeholders care about appearance, not strict legal validity

For more robust authenticity, combine with a digital signature as shown below.

---

### How Do I Precisely Position a Signature Image on a PDF?

PDF coordinates are measured in points (1 point = 1/72 inch), with (0, 0) at the bottom-left. To place your signature in the bottom-right, calculate like this:

```csharp
int pageW = 612;   // Letter size width in points
int pageH = 792;   // Letter size height in points
int sigW = 160;
int sigH = 60;

int x = pageW - sigW - 25; // 25pt margin
int y = 30; // 30pt from bottom

using (var sigBmp = new Bitmap("sig.png"))
{
    pdf.DrawBitmap(sigBmp, x, y, 0, sigW, sigH);
}
```

Open your PDF in a viewer, move your mouse to check coordinates, and adjust as needed for perfect placement.

---

## Can I Combine Digital and Visual Signatures in One PDF?

Yes! Adding both a visible signature image and a cryptographic digital signature provides both the look and the security.

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using System.Drawing;

var pdf = PdfDocument.FromFile("agreement.pdf");

using (var visSig = new Bitmap("ceo-sign.png"))
{
    pdf.DrawBitmap(visSig, 380, 90, 0, 180, 70);
}

pdf.SignWithFile("ceo-cert.pfx", "ceoPassword");

pdf.SaveAs("agreement-fully-signed.pdf");
```

This way, your PDF looks signed and is also legally protected.

---

## How Do I Add a Signature Field That Others Can Fill in Later?

If you need your client or manager to sign the document themselves, add an interactive signature field:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using IronPdf.Forms;

var pdf = PdfDocument.FromFile("blank-form.pdf");

var sigPlaceholder = new SignatureFormField
{
    Name = "customer_signature",
    PageIndex = 0,
    X = 110,
    Y = 140,
    Width = 200,
    Height = 60
};

pdf.Form.Fields.Add(sigPlaceholder);

pdf.SaveAs("blank-form-with-sig-field.pdf");
```

When opened in Adobe Reader or compatible apps, the recipient will see a clickable signature slot.

---

## How Do I Sign a PDF Using Windows Certificate Store in C#?

If your organization manages certificates centrally or you use hardware tokens, you can sign using a certificate from the Windows Certificate Store:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using System.Security.Cryptography.X509Certificates;

var pdf = PdfDocument.FromFile("secure-doc.pdf");

var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
store.Open(OpenFlags.ReadOnly);

var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "MyCompany", false);
if (certs.Count == 0)
    throw new Exception("No matching certificate found.");

var cert = certs[0];
pdf.SignWithCertificate(cert);

pdf.SaveAs("store-signed.pdf");
```

This avoids distributing sensitive PFX files and lets your IT team rotate keys easily.

---

## How Can I Add Signature Metadata Like Reason, Location, or Contact?

You can add rich metadata to your signature, making it clearer who signed, why, and how to contact them:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using IronPdf.Signing;

var pdf = PdfDocument.FromFile("offer.pdf");

var sig = new PdfSignature("hr-cert.pfx", "securePass")
{
    SigningReason = "Offer accepted",
    SigningLocation = "Berlin HQ",
    SigningContactInfo = "hr@example.com"
};

pdf.Sign(sig);

pdf.SaveAs("offer-signed.pdf");
```

Open the signed PDF in Adobe, and you'll see these details in the signature panel.

---

## How Do I Handle Multiple Signatures or Multi-Party Documents?

For agreements with multiple signers, simply add each signature sequentially:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using System.Drawing;

var pdf = PdfDocument.FromFile("partnership.pdf");

// CEO's signature
using (var ceo = new Bitmap("ceo-sign.png"))
{
    pdf.DrawBitmap(ceo, 120, 160, 0, 170, 60);
    pdf.SignWithFile("ceo-cert.pfx", "ceoPass");
}

// CFO's signature, added incrementally
using (var cfo = new Bitmap("cfo-sign.png"))
{
    pdf.DrawBitmap(cfo, 350, 160, 0, 170, 60);
    pdf.SignWithFile("cfo-cert.pfx", "cfoPass");
}

pdf.SaveAs("fully-signed-partnership.pdf");
```

**Tip:** Always use the latest PDF (with all previous signatures) before adding a new signature to avoid overwriting.

---

## How Can I Batch Sign Multiple PDFs Automatically?

When you need to process hundreds of documents, loop through files and apply your signature logic:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using System.IO;

string certPath = "company.pfx";
string certPass = Environment.GetEnvironmentVariable("PDF_CERT_PASS");

var inputDir = "to-sign";
var outputDir = "signed-files";
Directory.CreateDirectory(outputDir);

foreach (var file in Directory.GetFiles(inputDir, "*.pdf"))
{
    var pdf = PdfDocument.FromFile(file);

    pdf.SignWithFile(certPath, certPass);

    using (var sigImg = new System.Drawing.Bitmap("company-sig.png"))
    {
        pdf.DrawBitmap(sigImg, 400, 90, 0, 160, 55);
    }

    pdf.SaveAs(Path.Combine(outputDir, Path.GetFileName(file)));
}
```

Combine this with a web app or scheduler for seamless automation. For more PDF automation ideas, see [How do I convert HTML files to PDFs in C#?](html-file-to-pdf-csharp.md).

---

## How Can I Add a Timestamp Authority for Long-Term Signature Validity?

Adding a trusted timestamp ensures your signature remains verifiable even if your certificate later expires:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using IronPdf.Signing;

var pdf = PdfDocument.FromFile("legal-record.pdf");

var sig = new PdfSignature("legal.pfx", "password")
{
    TimestampUrl = "http://timestamp.digicert.com"
};

pdf.Sign(sig);

pdf.SaveAs("legal-record-timestamped.pdf");
```

Most Certificate Authorities provide a timestamp URL—use one that matches your compliance requirements.

---

## How Do I Restrict Permissions on a Signed PDF?

You can use signature permissions to lock down what others can do after signing:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using IronPdf.Signing;

var pdf = PdfDocument.FromFile("nda.pdf");

var sig = new PdfSignature("nda-cert.pfx", "secure123")
{
    Permissions = SignaturePermissions.NoChangesAllowed
};

pdf.Sign(sig);

pdf.SaveAs("nda-locked.pdf");
```

Other permission options allow form filling or annotations. Locked PDFs will show changes as tampering if someone alters them post-signing.

---

## How Do I Verify PDF Signatures in C#?

[IronPDF](https://ironpdf.com) specializes in creating and adding signatures, but not verifying them. For programmatic verification, consider using [iTextSharp](https://github.com/itext/itextsharp):

- Open your PDF in Adobe Reader to check signatures manually.
- For code-based verification, see the iTextSharp documentation.

If you need to extract images from signed PDFs, see [How do I convert PDF to images in C#?](pdf-to-images-csharp.md).

---

## What Are Security Best Practices for PDF Signing in .NET?

- **Never hardcode certificate passwords** in your source or config files.
- **Use environment variables or secrets managers** like Azure Key Vault or AWS Secrets Manager to manage sensitive info.
- **Set strong access controls** on certificate files (PFX).
- **Monitor certificate expiry** and rotate certificates before they lapse.
- **Log all signing operations** for auditing.
- **Validate input/output directories** and check for file permission issues in automation scripts.

Here’s a secure example:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf

string certPass = Environment.GetEnvironmentVariable("PDF_CERT_PASS");
if (string.IsNullOrWhiteSpace(certPass))
    throw new Exception("Certificate password not set.");

var pdf = PdfDocument.FromFile("sensitive.pdf");
pdf.SignWithFile("secure-cert.pfx", certPass);
pdf.SaveAs("sensitive-signed.pdf");
```

---

## Can I Remove a Signature from a PDF, and Should I?

Technically, you can remove a signature, but it immediately invalidates the document's authenticity. Only do this if you have explicit authority and understand the implications.

```csharp
using IronPdf; // NuGet: Install-Package IronPdf

var pdf = PdfDocument.FromFile("signed-example.pdf", ownerPassword);
// Saving the PDF after removing a signature will mark it as tampered/insecure.
```

PDF signatures are designed to be tamper-evident for a reason.

---

## Can I Sign PDFs That I Generate On-the-Fly in C#?

Absolutely—you can generate PDFs dynamically (e.g., from HTML, XML, or XAML) and sign them in the same process. Here’s an HTML example:

```csharp
using IronPdf; // NuGet: Install-Package IronPdf
using System.Drawing;

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Invoice #4567</h1>");

pdf.SignWithFile("invoice-cert.pfx", "password456");

using (var sigImg = new Bitmap("company-sig.png"))
{
    pdf.DrawBitmap(sigImg, 390, 80, 0, 150, 50);
}

pdf.SaveAs("invoice-4567-signed.pdf");
```

If you're working with web fonts or icons in your PDFs, see [How do I use web fonts and icon fonts in PDFs?](web-fonts-icons-pdf-csharp.md). For XML or XAML, see [XML to PDF in C#](xml-to-pdf-csharp.md) and [XAML to PDF in MAUI](xaml-to-pdf-maui-csharp.md).

---

## Is IronPDF E-Signature Compliant with ESIGN, eIDAS, and UETA?

[IronPDF's digital signing capabilities](https://ironpdf.com/nodejs/examples/digitally-sign-a-pdf/) meet the technical requirements of major e-signature laws, including:

- **ESIGN Act (USA)**
- **eIDAS (EU)**
- **UETA (Uniform Electronic Transactions Act)**

However, legal compliance also requires proper consent, identity verification, and audit trails. Consult your compliance team for industry-specific rules.

---

## What Are Common Pitfalls When Signing PDFs in C#?

### Why Does Adobe Reader Say "Signature Not Trusted"?

- Your certificate may be self-signed or not in Adobe's trusted list. For external documents, always use a CA-issued cert.

### I’m Getting “Password Incorrect” Errors—Why?

- Double-check the certificate password, and ensure the PFX isn’t expired or corrupted. Try opening it in Windows Certificate Manager.

### My Signature Image Isn’t Showing Up!

- Check coordinates, page index, and image format (PNG, JPEG, BMP are safe bets).

### Why Do My Multiple Signatures Overwrite Each Other?

- Always load the newest version of the PDF before adding a new signature. Never start with the original unsigned document.

### My Batch Signing Script is Failing—What Should I Check?

- Make sure output directories exist, and check for file locks or permission issues.
- Use `Directory.CreateDirectory()` defensively.

### Can I Verify Signatures Programmatically with IronPDF?

- No, verification requires a library like [iTextSharp](https://github.com/itext/itextsharp) or manual checking in Adobe Reader.

If you're running into edge cases, let the [IronPDF](https://ironpdf.com) community know!

---

## Where Can I Learn More or Get Help with PDF Signing in C#?

For comprehensive documentation and advanced topics, visit [IronPDF](https://ironpdf.com) and [Iron Software](https://ironsoftware.com). If you’re working with hybrid document flows (like XML, XAML, or web fonts), these related FAQs may help:

- [How do I convert XML to PDF in C#?](xml-to-pdf-csharp.md)
- [How can I generate PDF from XAML in .NET MAUI?](xaml-to-pdf-maui-csharp.md)
- [How do I use web fonts and icon fonts in PDFs?](web-fonts-icons-pdf-csharp.md)
- [How do I convert an HTML file to PDF in C#?](html-file-to-pdf-csharp.md)
- [How do I convert PDF to images in C#?](pdf-to-images-csharp.md)

---

*[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) created IronPDF to solve PDF generation headaches in .NET. He's now CTO at Iron Software, leading a team focused on developer tools.*
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

Written by [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), Chief Technology Officer at Iron Software. Jacob started coding at age 6 and created IronPDF to solve real PDF challenges. Based in Chiang Mai, Thailand. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
