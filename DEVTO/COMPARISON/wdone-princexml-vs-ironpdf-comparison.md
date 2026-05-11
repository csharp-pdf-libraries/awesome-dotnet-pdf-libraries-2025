---
title: "PrinceXML vs IronPDF: a technical breakdown for 2026"
published: false
tags: dotnet, csharp, pdf, comparison
---
# PrinceXML vs IronPDF: a technical breakdown for 2026

*Canonical/source version on Iron Software blog: https://ironsoftware.com/suite/blog*

## When print publishing requirements meet .NET development

A compliance department needed to generate annual reports with strict print specifications: CMYK color profiles, PDF/X-1a compliance for commercial printing, running headers with page numbers, footnotes at page bottoms. The team evaluated PrinceXML—industry standard for print publishing. The CSS Paged Media support checked all boxes: `@page` rules, `running()` elements, CMYK profiles. Then deployment started: install Java or standalone binary, manage external process lifecycle, handle CLI output parsing, configure PATH variables, troubleshoot cross-platform execution. The print features worked perfectly; the .NET integration required custom infrastructure. The question became: do print publishing requirements justify external process complexity?

PrinceXML is a commercial HTML-to-PDF converter specialized for print publishing workflows. The tool uses CSS Paged Media specifications—features like `@page` margins, `running()` elements for headers/footers, `footnote` placement, page breaks, widow/orphan control. This makes PrinceXML the choice for documents destined for commercial printing: books, magazines, catalogs, compliance reports requiring PDF/X or PDF/A. The architecture is command-line executable; .NET integration means spawning processes, capturing output, managing file I/O. For teams with print publishing requirements and existing CLI tooling, PrinceXML's specialization fits. For teams needing embedded PDF generation without external processes, the architectural mismatch creates friction.

## Understanding IronPDF

IronPDF takes a different approach: embedded Chromium engine for HTML-to-PDF within .NET applications. The library uses browser CSS (CSS3, Flexbox, Grid) rather than CSS Paged Media specifications. This means familiar web development patterns (style components with standard CSS, test in browser) translate directly to PDFs. The tradeoff: advanced print publishing features (CMYK profiles, PDF/X compliance, complex footnoting) aren't built-in. For business documents, invoices, reports, web-to-PDF workflows, Chromium's web-focused rendering provides standard compliance. For commercial printing, print-specific features require evaluation.

IronPDF's workflow is: write HTML/CSS as if building a webpage, call `RenderHtmlAsPdf()`, get PDF. Installation is single NuGet package with Chromium managed internally—no external executables, PATH configuration, or process management. The architectural question: embedded generation (IronPDF's approach) versus specialized print tools (PrinceXML's domain).

## Key Limitations of PrinceXML

### **Product Status**

Active commercial product by YesLogic, version 15.1 (2024 release), mature tool (20+ years). Commercial license per-server with runtime royalty-free deployment. Strong track record in publishing industry (O'Reilly Media, W3C, nature.com). Documentation comprehensive for CSS Paged Media. For print publishing requirements, PrinceXML is industry-proven. For .NET-native embedding, external process architecture creates integration work.

### **Missing Capabilities**

No native .NET library—command-line executable only. .NET wrapper available (PrinceXMLWrapper package) but manages CLI process, not true library integration. No in-process rendering—every PDF generation spawns external process. No object model for PDFs—output is file-based only. Limited post-generation manipulation (no merge, split, edit APIs). No direct database integration—must generate HTML first. No async/await patterns—process spawning is synchronous with async wrappers.

### **Technical Issues**

External process overhead on every PDF generation (~100-500ms process spawn). File-based I/O required—can't render directly to memory streams without wrappers. PATH configuration needed for CLI access or explicit path in code. Cross-platform differences (Windows/Linux executable paths). License validation happens per-process (licensing overhead). Error handling via exit codes and stderr parsing. No renderer reuse—fresh process per document. Resource cleanup complexity if processes leak.

### **Support Status**

Commercial support from YesLogic via email and forums. Documentation excellent for CSS Paged Media, adequate for CLI usage. .NET integration examples minimal—community-driven patterns. License includes email support. For print publishing questions, support is strong. For .NET integration patterns, support relies on CLI documentation and community knowledge. No SLA unless enterprise agreement.

### **Architecture Problems**

Command-line architecture creates .NET integration friction. Process management (spawn, monitor, cleanup) becomes application responsibility. File-based I/O patterns (write HTML, invoke CLI, read PDF) add latency and disk I/O. No connection pooling or renderer reuse—each document is fresh process. Deployment complexity (install executable separate from NuGet packages). Container images require PrinceXML installation in base image. Version management (application references specific CLI version, must keep in sync).

---

## Feature Comparison Overview

| Aspect | PrinceXML | IronPDF |
|--------|-----------|----------|
| **Current Status** | Active (commercial CLI) | Active (commercial library) |
| **CSS Support** | Paged Media specialist | CSS3/Flexbox/Grid (web) |
| **Rendering Quality** | Print publishing optimized | Browser-grade web-focused |
| **Installation** | CLI executable | Single NuGet |
| **Support** | Commercial (email/forums) | Commercial (multiple channels) |
| **Future Viability** | Print publishing stable | Web-to-PDF evolving |

---

## Decision Flowcharts & Checklists

### ☑ When PrinceXML is Mandatory

Use PrinceXML when these apply:

**Print Publishing Requirements (non-negotiable):**
- ☐ PDFs destined for commercial printing presses
- ☐ CMYK color profiles required (not RGB)
- ☐ PDF/X-1a, PDF/X-3, or PDF/X-4 compliance mandated
- ☐ Complex footnoting with automatic page flow
- ☐ Running headers/footers with content from document flow
- ☐ Widow/orphan control, column balancing, page break optimization
- ☐ CSS Paged Media specifications already in use

**Workflow Indicators (strong fit):**
- ☐ Design team works in CSS Paged Media (existing expertise)
- ☐ Multi-language stack (PrinceXML CLI works from any language)
- ☐ Existing CLI-based tooling infrastructure
- ☐ Publishing industry workflows (books, magazines, catalogs)
- ☐ Compliance requires PDF/A-3 with specific conformance levels

If **3+ print publishing requirements** checked: PrinceXML's specialization justifies integration complexity.

---

### ☑ When PrinceXML Creates Friction

Avoid PrinceXML when these apply:

**Architecture Mismatches (red flags):**
- ☐ .NET-only environment with no CLI tooling experience
- ☐ Need embedded in-process rendering (no external executables)
- ☐ High-volume generation (process spawn overhead matters)
- ☐ Real-time generation (low latency critical)
- ☐ Complex post-generation manipulation (merge/split/edit)
- ☐ Container-based deployment with minimal base images
- ☐ Serverless architecture (Lambda, Azure Functions)

**Skill Set Mismatches:**
- ☐ Team knows web CSS, not CSS Paged Media
- ☐ No experience managing external process lifecycles
- ☐ Standard web-to-PDF workflow (invoices, reports, receipts)
- ☐ Design-to-PDF iteration needs browser preview
- ☐ CSS Grid/Flexbox layouts (not Paged Media constructs)

If **4+ red flags** checked: PrinceXML's CLI architecture outweighs print features. Consider IronPDF for web-focused rendering or other print-specific tools if CMYK/PDF-X truly needed.

---

### Deployment Complexity Checklist

#### PrinceXML Deployment (Estimate: 4-8 hours initial setup)

**Installation Phase:**
- ☐ Download PrinceXML binary for target OS (Windows/Linux/macOS)
- ☐ Install standalone executable (or extract portable version)
- ☐ Configure system PATH or store explicit path in configuration
- ☐ Verify CLI works: `prince --version`
- ☐ Configure license file (per-server licensing)
- ☐ Test CLI with sample HTML: `prince input.html -o output.pdf`

**Integration Phase:**
- ☐ Install .NET wrapper if needed (community packages available)
- ☐ Write process management code (spawn, monitor, cleanup)
- ☐ Implement file I/O handling (HTML to temp files, PDF from output)
- ☐ Add error handling (parse stderr, check exit codes)
- ☐ Implement async wrappers if needed (CLI is synchronous)
- ☐ Add resource cleanup (delete temp files, kill leaked processes)

**Deployment Phase:**
- ☐ Add PrinceXML to Docker base image (if containerized)
- ☐ Configure PATH in container environment variables
- ☐ Copy license file to deployment location
- ☐ Test cross-platform execution (Windows → Linux differences)
- ☐ Monitor process lifetimes (leaked processes = memory leak)
- ☐ Add health checks

**Maintenance:**
- ☐ Synchronize PrinceXML version with application deployments
- ☐ Update license files on renewal
- ☐ Monitor process execution times and memory
- ☐ Handle CLI version differences across environments

**Total setup time:** 4-8 hours for initial integration + ongoing maintenance

---

#### IronPDF Deployment (Estimate: 15-30 minutes initial setup)

**Installation Phase:**
- ☐ Install NuGet package: `dotnet add package IronPdf`
- ☐ Verify installation in project

**Integration Phase:**
- ☐ Write rendering code: `new ChromePdfRenderer().RenderHtmlAsPdf(html)`
- ☐ No process management needed
- ☐ No file I/O required (in-memory operation)
- ☐ Standard exception handling (.NET exceptions)

**Deployment Phase:**
- ☐ Deploy application (Chromium bundled automatically)
- ☐ No PATH configuration needed
- ☐ No external executables to manage
- ☐ Standard .NET deployment patterns apply

**Maintenance:**
- ☐ Update NuGet package when needed
- ☐ Standard .NET versioning

**Total setup time:** 15-30 minutes for initial integration

---

## Code Comparisons

### CSS Paged Media vs Standard CSS

#### PrinceXML — CSS Paged Media Approach

```csharp
// Using PrinceXML .NET wrapper (community package)
// First install PrinceXML executable separately

using System;
using System.Diagnostics;
using System.IO;

public class PrinceXmlGenerator
{
    private readonly string _princeExePath = @"C:\Program Files\Prince\engine\bin\prince.exe";
    
    public void GenerateWithPagedMedia()
    {
        // HTML with CSS Paged Media features
        var html = @"
            <html>
            <head>
                <style>
                    @page {
                        size: A4;
                        margin: 2.5cm 2cm 3cm 2cm;
                        
                        /* Running headers/footers */
                        @top-center {
                            content: 'Annual Report 2026';
                        }
                        @bottom-right {
                            content: 'Page ' counter(page) ' of ' counter(pages);
                        }
                    }
                    
                    /* Running header content from document */
                    h1 { 
                        string-set: chapter content();
                    }
                    @page :right {
                        @top-right {
                            content: string(chapter);
                        }
                    }
                    
                    /* Footnote support */
                    .footnote {
                        float: footnote;
                        footnote-style-position: inside;
                    }
                    
                    /* Widow/orphan control */
                    p {
                        widows: 2;
                        orphans: 2;
                    }
                    
                    /* Page breaks */
                    .chapter {
                        page-break-before: always;
                    }
                </style>
            </head>
            <body>
                <h1>Financial Overview</h1>
                <p>This section discusses revenue<span class='footnote'>Based on GAAP accounting</span>.</p>
                
                <div class='chapter'>
                    <h1>Operations Report</h1>
                    <p>Content here...</p>
                </div>
            </body>
            </html>";
        
        // Write HTML to temp file
        string htmlPath = Path.GetTempFileName() + ".html";
        File.WriteAllText(htmlPath, html);
        
        // Output PDF path
        string pdfPath = "report.pdf";
        
        // Invoke Prince CLI
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _princeExePath,
                Arguments = $"\"{htmlPath}\" -o \"{pdfPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string errors = process.StandardError.ReadToEnd();
        process.WaitForExit();
        
        // Check exit code
        if (process.ExitCode != 0)
        {
            throw new Exception($"PrinceXML failed: {errors}");
        }
        
        // Cleanup temp file
        File.Delete(htmlPath);
    }
}
```

**Technical strengths observed:**

- **CSS Paged Media**: Running headers pull content from document flow
- **Page counter**: `counter(pages)` provides total page count
- **Footnotes**: `.footnote` elements automatically flow to page bottom
- **Widow/orphan control**: Paragraph fragmentation rules respected
- **Print-specific**: Features designed for paginated output

**Integration complexity:**

- External process management required
- File-based I/O (temp files for HTML input)
- Error handling via exit codes and stderr
- Process cleanup critical
- PATH configuration needed

#### IronPDF — Standard Web CSS Approach

```csharp
// Install-Package IronPdf

using IronPdf;

public class IronPdfGenerator
{
    public void GenerateWithWebCss()
    {
        var html = @"
            <html>
            <head>
                <style>
                    body {
                        font-family: Arial, sans-serif;
                        margin: 2.5cm 2cm 3cm 2cm;
                    }
                    
                    /* Standard CSS for layout */
                    .header {
                        text-align: center;
                        font-weight: bold;
                    }
                    
                    /* Footnotes styled as list */
                    .footnotes {
                        border-top: 1px solid #ccc;
                        margin-top: 2em;
                        padding-top: 1em;
                        font-size: 0.9em;
                    }
                    
                    /* Page breaks via CSS */
                    .chapter {
                        page-break-before: always;
                    }
                </style>
            </head>
            <body>
                <div class='header'>Annual Report 2026</div>
                
                <h1>Financial Overview</h1>
                <p>This section discusses revenue<sup>1</sup>.</p>
                
                <div class='footnotes'>
                    <p>1. Based on GAAP accounting</p>
                </div>
                
                <div class='chapter'>
                    <h1>Operations Report</h1>
                    <p>Content here...</p>
                </div>
            </body>
            </html>";
        
        var renderer = new ChromePdfRenderer();
        
        // Optional: Configure rendering options
        renderer.RenderingOptions.MarginTop = 25;
        renderer.RenderingOptions.MarginBottom = 30;
        renderer.RenderingOptions.MarginLeft = 20;
        renderer.RenderingOptions.MarginRight = 20;
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("report.pdf");
    }
}
```

**Difference noted:** IronPDF uses web-standard CSS (no CSS Paged Media). Footnotes, running headers, page counters require manual HTML structure. For web-familiar teams, CSS is standard; for print publishing workflows, Paged Media automation is absent.

---

### CMYK Color Profiles and PDF/X Compliance

#### PrinceXML — Print Publishing Features

```csharp
using System.Diagnostics;

public class PrinceXmlPrintPublishing
{
    private readonly string _princeExePath = @"C:\Program Files\Prince\engine\bin\prince.exe";
    
    public void GeneratePdfX1a()
    {
        var html = @"
            <html>
            <head>
                <style>
                    /* CMYK color specification */
                    .cmyk-red {
                        color: cmyk(0%, 100%, 100%, 0%);
                    }
                    
                    body {
                        background: cmyk(0%, 0%, 0%, 0%);
                    }
                    
                    @page {
                        size: A4;
                        bleed: 3mm;
                        marks: crop cross;
                    }
                </style>
            </head>
            <body>
                <h1 class='cmyk-red'>Print-Ready Document</h1>
            </body>
            </html>";
        
        System.IO.File.WriteAllText("input.html", html);
        
        // Generate PDF/X-1a compliant PDF
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _princeExePath,
                Arguments = "input.html -o output.pdf --pdf-profile=PDF/X-1a:2003 --pdf-output-intent=Coated_FOGRA39.icc",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        process.WaitForExit();
        
        // Result: PDF/X-1a compliant with CMYK colors, bleed marks, crop marks
    }
}
```

**Print publishing features:**

- **CMYK colors**: Direct CMYK specification in CSS
- **PDF/X compliance**: --pdf-profile flag generates standards-compliant output
- **Color profiles**: ICC profiles for commercial printing
- **Bleed and marks**: Crop marks, bleed zones for print production
- **Industry standard**: Output matches printing press requirements

#### IronPDF — RGB-Focused Rendering

```csharp
using IronPdf;

public class IronPdfStandardRendering
{
    public void GenerateStandardPdf()
    {
        var html = @"
            <html>
            <head>
                <style>
                    /* RGB color (web standard) */
                    .red {
                        color: rgb(255, 0, 0);
                    }
                    
                    body {
                        background: white;
                    }
                </style>
            </head>
            <body>
                <h1 class='red'>Business Document</h1>
            </body>
            </html>";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
        
        // Result: Standard RGB PDF (suitable for screen viewing, digital distribution)
        // For CMYK/PDF-X: verify commercial printing requirements in IronPDF docs
    }
}
```

**Difference:** IronPDF produces standard RGB PDFs (web-focused). For CMYK/PDF-X commercial printing requirements, verify capabilities in docs or consider print-specialized tools.

---

## API Mapping Reference

| PrinceXML CLI Argument | IronPDF Equivalent |
|------------------------|-------------------|
| `prince input.html -o output.pdf` | `renderer.RenderHtmlAsPdf(html)` |
| `--pdf-profile=PDF/X-1a` | Verify PDF/X support in docs |
| `--pdf-output-intent=profile.icc` | Verify ICC profile support in docs |
| `--no-compress` | `pdf.CompressionEnabled = false` |
| `--encrypt` | `pdf.Password` property |
| `--pdf-title='Title'` | `pdf.MetaData.Title` |
| CMYK color support | Verify in docs for CMYK |
| CSS Paged Media features | No direct equivalent (manual HTML structure) |
| Running headers via string-set | No equivalent—requires HTML templating |
| Footnote float | No equivalent—manual footnote positioning |
| Bleed and crop marks | Verify in docs |

---

## Comprehensive Feature Comparison

### Status & Support

| Feature | PrinceXML | IronPDF |
|---------|-----------|---------|
| **Product Status** | Active (commercial) | Active (commercial) |
| **Architecture** | CLI executable | Embedded library |
| **License Model** | Per-server commercial | Per-developer commercial |
| **Support** | Email + forums | Multiple channels |
| **.NET Integration** | External process (wrapper available) | Native library |

### CSS Support

| Feature | PrinceXML | IronPDF |
|---------|-----------|---------|
| **CSS Standard** | CSS Paged Media specialist | CSS3/Flexbox/Grid (web) |
| **@page Rules** | Full support | Basic support |
| **Running Headers** | Yes (string-set) | No (manual HTML) |
| **Footnotes** | Automatic flow | Manual positioning |
| **Page Counters** | counter(pages) built-in | Verify in docs |
| **Widow/Orphan** | Full control | Limited |

### Print Publishing

| Feature | PrinceXML | IronPDF |
|---------|-----------|---------|
| **CMYK Colors** | Native support | Verify in docs |
| **PDF/X Compliance** | PDF/X-1a/3/4 | Verify in docs |
| **PDF/A Compliance** | PDF/A-1/2/3 | PDF/A supported |
| **ICC Profiles** | Full support | Verify in docs |
| **Bleed/Crop Marks** | Built-in | Verify in docs |

### Content Creation

| Feature | PrinceXML | IronPDF |
|---------|-----------|---------|
| **HTML to PDF** | Yes (CLI-based) | Yes (embedded) |
| **CSS Rendering** | Print-focused | Web-focused |
| **JavaScript** | Limited | Full (Chromium) |
| **Web Fonts** | Yes | Yes |

### PDF Operations

| Feature | PrinceXML | IronPDF |
|---------|-----------|---------|
| **Create PDFs** | Yes | Yes |
| **Merge PDFs** | No (CLI single output) | Yes |
| **Split PDFs** | No | Yes |
| **Text Extraction** | No | Yes |
| **Watermarks** | Via HTML/CSS | API-based |
| **Encryption** | CLI flags | API properties |

### Deployment

| Feature | PrinceXML | IronPDF |
|---------|-----------|---------|
| **Install Complexity** | Moderate (CLI + license) | Low (single NuGet) |
| **Process Management** | Required (external process) | Not needed (in-process) |
| **Container Deploy** | Custom (install CLI in image) | Standard (.NET image) |
| **Startup Time** | Process spawn per PDF | Renderer reuse |

---

## When Teams Consider PrinceXML Migration

**Print publishing requirements** justify PrinceXML's architecture. When documents go to commercial printing presses, CMYK color accuracy, PDF/X compliance, and professional print features (bleed, crop marks, footnote flow) are non-negotiable. PrinceXML delivers these features through CSS Paged Media specifications. For teams in publishing workflows—books, magazines, catalogs, compliance documents for printing—the CLI architecture becomes acceptable operational overhead for print quality output.

**CSS Paged Media expertise** already exists in design teams. Organizations with print designers familiar with `@page` rules, `running()` elements, and footnote float already have the skill set. Migrating to web-standard CSS (Flexbox, Grid) means retraining and rewriting templates. When existing CSS Paged Media templates work well, keeping PrinceXML preserves that investment.

**Multi-language stack** benefits from CLI architecture. PrinceXML's command-line interface works from any language—Python, Ruby, Java, .NET, Node.js. For organizations with polyglot architectures where PDF generation is shared service, CLI tools provide language-neutral integration. The process management complexity is common across all languages.

**Architectural mismatch** drives migration evaluation for .NET-native teams. External process management (spawn, monitor, cleanup), file-based I/O (temp files for HTML), PATH configuration, and cross-platform executable differences create friction. For .NET shops building web applications or APIs where PDF generation is embedded feature (invoices, reports, receipts), PrinceXML's CLI architecture adds operational complexity for features (CMYK, PDF/X) they don't need.

**Web-focused workflows** don't benefit from print specialization. Teams designing templates in browsers, testing with web CSS (Flexbox, Grid, modern selectors), and generating documents for digital distribution (email, download, web view) find CSS Paged Media irrelevant. Running headers, footnote flow, widow/orphan control matter for print; for screen-focused PDFs, these features add learning curve without benefit.

---

## Installation Comparison

### PrinceXML

```bash
# Download from https://www.princexml.com/download/
# Install standalone executable (Windows/Linux/macOS)

# Windows
prince.exe input.html -o output.pdf

# Linux  
/usr/local/bin/prince input.html -o output.pdf
```

```csharp
// .NET integration via process spawning
using System.Diagnostics;

var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = @"C:\Program Files\Prince\engine\bin\prince.exe",
        Arguments = "input.html -o output.pdf"
    }
};
process.Start();
process.WaitForExit();
```

**Container Deployment:**
```dockerfile
# Install PrinceXML in Docker image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN apt-get update && apt-get install -y wget
RUN wget https://www.princexml.com/download/prince_15.1-1_ubuntu22.04_amd64.deb
RUN dpkg -i prince_15.1-1_ubuntu22.04_amd64.deb
ENV PATH="/usr/local/bin:$PATH"
```

### IronPDF

```bash
dotnet add package IronPdf
```

```csharp
using IronPdf;
```

**Container Deployment:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# No additional configuration needed
```

---

## Conclusion

PrinceXML delivers on its print publishing promise: CSS Paged Media specialist with CMYK support, PDF/X compliance, running headers/footers, automatic footnotes, and commercial printing features. The command-line architecture serves multi-language environments and teams with existing print workflows. For documents destined for printing presses—books, magazines, catalogs, compliance reports—PrinceXML's specialization justifies integration complexity. YesLogic's 20+ year track record and publishing industry adoption (O'Reilly, W3C) demonstrate production stability.

Migration from PrinceXML becomes relevant when: (1) print publishing features (CMYK, PDF/X, Paged Media) aren't needed for digital-only documents, (2) CLI architecture creates friction in .NET-native environments expecting embedded libraries, (3) team expertise is web CSS (Flexbox/Grid) not CSS Paged Media, or (4) operational complexity (process management, file I/O, PATH config) outweighs print feature benefits. The evaluation centers on print requirements versus .NET integration simplicity.

IronPDF addresses embedded HTML-to-PDF generation: Chromium engine with web-standard CSS, in-process rendering, and .NET-native integration. Missing features: CSS Paged Media automation, CMYK/PDF-X for commercial printing, print-specific pagination controls. For business documents (invoices, reports, receipts), web-to-PDF workflows, and teams designing in browsers with standard CSS, IronPDF's architecture eliminates external process complexity. For print publishing, PrinceXML's specialization remains relevant.

**What drives your evaluation—print publishing requirements (CMYK, PDF/X, Paged Media), embedded .NET generation with web CSS, or something else? Do your documents go to printing presses or digital distribution?**

*For HTML-to-PDF web workflows, see [rendering guide](https://ironsoftware.com/csharp/pdf/how-to/html-to-pdf/). For CSS media type handling, review the [print media documentation](https://ironpdf.com/how-to/css-media-types/).*
