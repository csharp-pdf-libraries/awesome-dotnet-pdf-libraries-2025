---
title: "ExpertPdf vs IronPDF: the decision guide for .NET teams"
published: false
tags: dotnet, csharp, pdf, comparison
---

Throughput requirements scale faster than most invoice pipelines. A .NET team can integrate an HTML-to-PDF converter, ship it, and watch sequential processing stay comfortable at low volume — then watch the same workflow stretch from minutes to hours as customer counts grow. The cost is rarely a single error; it is linear scaling colliding with a roadmap that needs better.

The question for .NET teams is not "which library is fastest" but "what performance characteristics matter for my workload?" Throughput for batch jobs differs from latency for real-time API responses. Memory footprint matters differently in serverless functions than in long-running services. Understanding these characteristics drives practical tool selection.

## Understanding IronPDF

IronPDF wraps a modern Chromium rendering engine optimized for server-side PDF generation at scale. The library handles concurrent rendering requests through an internal process pool that manages browser lifecycle. For batch operations, the same `ChromePdfRenderer` instance can process multiple documents sequentially, amortizing initialization overhead.

Memory management uses stream-based operations where possible, allowing PDFs to be generated directly to response streams without intermediate file system storage. The [HTML to PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) covers async/await patterns for high-concurrency scenarios and resource pooling strategies for sustained throughput.

## Key Limitations of ExpertPdf

### Product Status
ExpertPdf is published by Outside Software Inc. and shipped from html-to-pdf.net. The suite is actively maintained — the latest release on nuget.org is v20.1.0 (April 2025) — though release notes typically describe "bug fixes and performance improvements" rather than headline features.

The `.NetCore` packages target .NET Standard 2.0 / .NET Framework 4.6.1, which means they run on .NET 5 through .NET 9, but without native multi-target builds or trimming-friendly assemblies. Platform support is Windows-only.

### Missing Capabilities
**Cross-platform deployment**: ExpertPdf requires Windows for operation. This limits deployment options for teams using Linux containers, Azure App Service Linux plans, or AWS Lambda on non-Windows runtimes.

**Component fragmentation**: ExpertPdf sells separate NuGet packages for different operations (HtmlToPdf, PdfCreator, MergePdf, SplitPdf, PdfSecurity, PdfToImage). Teams needing multiple operations must license each component separately and coordinate versioning across packages.

### Technical Considerations
**Rendering engine**: ExpertPdf historically uses a Trident (IE) engine with a "WebKit2" engine added in v12.2; modern Chromium is not the default. CSS3 features such as Flexbox, Grid, and CSS variables render best on the WebKit2 engine — verify your target HTML before migrating large estates.

**Memory characteristics**: With HTML-to-PDF architectures, complex pages with many images or JavaScript can show higher memory consumption. Memory profiling is recommended for workloads with large pages or high concurrency.

**Thread safety**: Verify documentation for thread safety guarantees. Some .NET PDF libraries require locking or separate instances per thread, which can impact throughput in multi-threaded scenarios.

### Support Status
Commercial library with vendor support. Response times and support tier details vary by licensing arrangement — verify directly with the vendor.

### Architecture Considerations
**Modular licensing**: Organizations track licenses for each component separately. If a workflow requires HTML conversion, merging, splitting, and security operations, that means multiple license keys and package versions to coordinate.

**Windows dependency**: As a Windows-only library, ExpertPdf requires Windows Server or Windows containers in cloud deployments, adding infrastructure cost beyond the library license itself.

## Feature Comparison Overview

| Category | ExpertPdf | IronPDF |
|----------|-----------|---------|
| **Current Status** | v20.1.0 (Apr 2025) | Continuously updated |
| **Rendering Engine** | Trident (IE) + WebKit2 | Chromium |
| **CSS Support** | Best on WebKit2; older engines partial | Full CSS3 (Flexbox, Grid) |
| **Installation** | Multiple packages | Single NuGet |
| **Platform** | Windows-only | Cross-platform |
| **Async Support** | Limited | Full async/await |

---

## Code Comparison: Performance-Critical Operations

### ExpertPdf — Simple HTML to PDF (Baseline)

```csharp
using ExpertPdf.HtmlToPdf;
using System;
using System.Diagnostics;

namespace PerformanceBaseline
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Initialize converter
                PdfConverter pdfConverter = new PdfConverter();

                // Configure basic options
                pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
                pdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;

                // Simple HTML content
                string html = @"
                    <html>
                    <body>
                        <h1>Performance Test Document</h1>
                        <p>This is a simple test paragraph.</p>
                    </body>
                    </html>";

                // Convert to PDF
                byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(html);
                System.IO.File.WriteAllBytes("simple.pdf", pdfBytes);

                stopwatch.Stop();
                Console.WriteLine($"Conversion time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"PDF size: {pdfBytes.Length / 1024}KB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
```

**Performance baseline characteristics:**
- **Cold start**: First conversion includes renderer initialization overhead
- **Memory footprint**: Monitor working set for simple documents as baseline
- **Throughput**: Single-threaded sequential processing as reference point

**Typical measurements to capture:**
- Initial conversion time (includes startup)
- Subsequent conversion time (warm cache)
- Memory delta per conversion
- CPU utilization during conversion
- Process count and handle usage

### IronPDF — Simple HTML to PDF (Baseline)

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.IO;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var stopwatch = Stopwatch.StartNew();

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(@"
    <html>
    <body>
        <h1>Performance Test Document</h1>
        <p>This is a simple test paragraph.</p>
    </body>
    </html>");
pdf.SaveAs("simple.pdf");

stopwatch.Stop();
Console.WriteLine($"Conversion time: {stopwatch.ElapsedMilliseconds}ms");
Console.WriteLine($"PDF size: {new FileInfo("simple.pdf").Length / 1024}KB");
```

For async operations with better concurrency characteristics, see the [HTML String to PDF guide](https://ironpdf.com/how-to/html-string-to-pdf/) which covers `RenderHtmlAsPdfAsync` patterns.

---

### ExpertPdf — Batch Processing Pattern

```csharp
using ExpertPdf.HtmlToPdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BatchProcessing
{
    class Program
    {
        static void Main(string[] args)
        {
            int documentCount = 100;
            var stopwatch = Stopwatch.StartNew();
            long totalBytes = 0;

            try
            {
                // Single converter instance for batch
                PdfConverter pdfConverter = new PdfConverter();
                pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.Letter;

                List<string> htmlTemplates = GenerateHtmlBatch(documentCount);

                for (int i = 0; i < htmlTemplates.Count; i++)
                {
                    byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(htmlTemplates[i]);
                    System.IO.File.WriteAllBytes($"batch_{i}.pdf", pdfBytes);
                    totalBytes += pdfBytes.Length;
                }

                stopwatch.Stop();

                Console.WriteLine($"Total documents: {documentCount}");
                Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"Average time per doc: {stopwatch.ElapsedMilliseconds / documentCount}ms");
                Console.WriteLine($"Throughput: {documentCount / (stopwatch.ElapsedMilliseconds / 1000.0):F2} docs/sec");
                Console.WriteLine($"Total output: {totalBytes / 1024 / 1024}MB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Batch processing failed: {ex.Message}");
            }
        }

        static List<string> GenerateHtmlBatch(int count)
        {
            var templates = new List<string>();
            for (int i = 0; i < count; i++)
            {
                templates.Add($@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial; margin: 40px; }}
                            .invoice {{ border: 1px solid #ddd; padding: 20px; }}
                        </style>
                    </head>
                    <body>
                        <div class='invoice'>
                            <h1>Invoice #{i + 1}</h1>
                            <p>Date: {DateTime.Now.ToString("yyyy-MM-dd")}</p>
                            <p>Customer: Customer {i + 1}</p>
                            <p>Amount: ${(i + 1) * 100:N2}</p>
                        </div>
                    </body>
                    </html>");
            }
            return templates;
        }
    }
}
```

**Batch performance considerations:**
- **Renderer reuse**: Confirm in vendor docs whether a single converter instance is safe for sequential reuse
- **Memory accumulation**: Monitor whether working set grows linearly or stabilizes
- **GC pressure**: Check GC collection frequency during long-running batches
- **File I/O overhead**: Disk writes can become a bottleneck for high-throughput scenarios

### IronPDF — Batch Processing Pattern

```csharp
using IronPdf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var documentCount = 100;
var stopwatch = Stopwatch.StartNew();

var renderer = new ChromePdfRenderer();
var htmlTemplates = Enumerable.Range(1, documentCount)
    .Select(i => $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial; margin: 40px; }}
                .invoice {{ border: 1px solid #ddd; padding: 20px; }}
            </style>
        </head>
        <body>
            <div class='invoice'>
                <h1>Invoice #{i}</h1>
                <p>Date: {DateTime.Now:yyyy-MM-dd}</p>
                <p>Customer: Customer {i}</p>
                <p>Amount: ${i * 100:N2}</p>
            </div>
        </body>
        </html>")
    .ToList();

for (int i = 0; i < htmlTemplates.Count; i++)
{
    var pdf = renderer.RenderHtmlAsPdf(htmlTemplates[i]);
    pdf.SaveAs($"batch_{i}.pdf");
}

stopwatch.Stop();
Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
Console.WriteLine($"Throughput: {documentCount / (stopwatch.ElapsedMilliseconds / 1000.0):F2} docs/sec");
```

The [HTML to PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) includes optimization guidance for batch processing, including render caching and memory management strategies.

---

### ExpertPdf — Complex HTML with Images

```csharp
using ExpertPdf.HtmlToPdf;
using System;
using System.Diagnostics;

namespace ComplexHtmlTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            long peakMemory = 0;

            try
            {
                PdfConverter pdfConverter = new PdfConverter();

                // Fit width to page
                pdfConverter.PdfDocumentOptions.FitWidth = true;

                // Complex HTML with external images
                string complexHtml = @"
                    <html>
                    <head>
                        <style>
                            body { font-family: 'Segoe UI', Arial; margin: 0; }
                            .header { background: #2c3e50; color: white; padding: 30px; }
                            .product-grid {
                                display: grid;
                                grid-template-columns: repeat(3, 1fr);
                                gap: 20px;
                                padding: 20px;
                            }
                            .product-card {
                                border: 1px solid #ddd;
                                border-radius: 8px;
                                padding: 15px;
                                box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                            }
                            .product-card img {
                                width: 100%;
                                height: 200px;
                                object-fit: cover;
                                border-radius: 4px;
                            }
                        </style>
                    </head>
                    <body>
                        <div class='header'>
                            <h1>Product Catalog</h1>
                        </div>
                        <div class='product-grid'>";

                // Add 12 product cards with images
                for (int i = 1; i <= 12; i++)
                {
                    complexHtml += $@"
                        <div class='product-card'>
                            <img src='https://picsum.photos/300/200?random={i}' alt='Product {i}'>
                            <h3>Product {i}</h3>
                            <p>Price: ${(i * 50):N2}</p>
                        </div>";
                }

                complexHtml += @"
                        </div>
                    </body>
                    </html>";

                // Set navigation timeout for image loading (seconds)
                pdfConverter.NavigationTimeout = 60;

                // Measure memory before conversion
                long memBefore = GC.GetTotalMemory(true);

                byte[] pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(complexHtml);

                // Measure memory after conversion
                long memAfter = GC.GetTotalMemory(false);
                peakMemory = memAfter - memBefore;

                System.IO.File.WriteAllBytes("complex_catalog.pdf", pdfBytes);

                stopwatch.Stop();

                Console.WriteLine($"Conversion time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"PDF size: {pdfBytes.Length / 1024 / 1024:F2}MB");
                Console.WriteLine($"Memory delta: {peakMemory / 1024 / 1024:F2}MB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Complex conversion failed: {ex.Message}");
            }
        }
    }
}
```

**Complex document performance factors:**
- **Image loading latency**: Network requests for external images add conversion time
- **Memory per image**: Each embedded image increases working set
- **CSS complexity**: Grid layouts, flexbox, and transforms impact rendering time. Grid and Flexbox support depends on which engine is in use (Trident vs WebKit2) — verify against your target HTML
- **Page count**: Multi-page documents from complex HTML show linear time/memory scaling

### IronPDF — Complex HTML with Images

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var stopwatch = Stopwatch.StartNew();

var renderer = new ChromePdfRenderer();
renderer.RenderingOptions.Timeout = 60; // seconds

var products = Enumerable.Range(1, 12)
    .Select(i => $@"
        <div class='product-card'>
            <img src='https://picsum.photos/300/200?random={i}' alt='Product {i}'>
            <h3>Product {i}</h3>
            <p>Price: ${i * 50:N2}</p>
        </div>")
    .ToList();

string complexHtml = $@"
    <html>
    <head>
        <style>
            body {{ font-family: 'Segoe UI', Arial; margin: 0; }}
            .header {{ background: #2c3e50; color: white; padding: 30px; }}
            .product-grid {{
                display: grid;
                grid-template-columns: repeat(3, 1fr);
                gap: 20px;
                padding: 20px;
            }}
            .product-card {{
                border: 1px solid #ddd;
                border-radius: 8px;
                padding: 15px;
            }}
            .product-card img {{
                width: 100%;
                height: 200px;
                object-fit: cover;
            }}
        </style>
    </head>
    <body>
        <div class='header'><h1>Product Catalog</h1></div>
        <div class='product-grid'>{string.Join("", products)}</div>
    </body>
    </html>";

var pdf = renderer.RenderHtmlAsPdf(complexHtml);
pdf.SaveAs("complex_catalog.pdf");

stopwatch.Stop();
Console.WriteLine($"Conversion time: {stopwatch.ElapsedMilliseconds}ms");
Console.WriteLine($"PDF size: {new FileInfo("complex_catalog.pdf").Length / 1024 / 1024:F2}MB");
```

---

### ExpertPdf — Concurrent Processing

```csharp
using ExpertPdf.HtmlToPdf;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ConcurrentProcessing
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int concurrencyLevel = 4;
            int documentsPerThread = 10;
            var stopwatch = Stopwatch.StartNew();

            var results = new ConcurrentBag<TimeSpan>();

            try
            {
                var tasks = new Task[concurrencyLevel];

                for (int t = 0; t < concurrencyLevel; t++)
                {
                    int threadId = t;
                    tasks[t] = Task.Run(() => ProcessBatch(threadId, documentsPerThread, results));
                }

                await Task.WhenAll(tasks);

                stopwatch.Stop();

                int totalDocs = concurrencyLevel * documentsPerThread;
                double avgTime = results.Select(r => r.TotalMilliseconds).Average();

                Console.WriteLine($"Total documents: {totalDocs}");
                Console.WriteLine($"Concurrency level: {concurrencyLevel}");
                Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"Average doc time: {avgTime:F2}ms");
                Console.WriteLine($"Throughput: {totalDocs / (stopwatch.ElapsedMilliseconds / 1000.0):F2} docs/sec");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Concurrent processing failed: {ex.Message}");
            }
        }

        static void ProcessBatch(int threadId, int count, ConcurrentBag<TimeSpan> results)
        {
            // Separate converter instance per thread — confirm thread safety guarantees in vendor docs
            PdfConverter converter = new PdfConverter();

            for (int i = 0; i < count; i++)
            {
                var docWatch = Stopwatch.StartNew();

                string html = $@"
                    <html>
                    <body>
                        <h1>Thread {threadId}, Doc {i}</h1>
                        <p>Timestamp: {DateTime.Now}</p>
                    </body>
                    </html>";

                byte[] pdf = converter.GetPdfBytesFromHtmlString(html);
                System.IO.File.WriteAllBytes($"thread{threadId}_doc{i}.pdf", pdf);

                docWatch.Stop();
                results.Add(docWatch.Elapsed);
            }
        }
    }
}
```

**Concurrent processing considerations:**
- **Thread safety**: Confirm whether a single converter instance handles concurrent calls or requires separate instances per thread
- **Resource contention**: Monitor CPU and memory as concurrency levels increase
- **Optimal thread count**: Test different concurrency levels to find your throughput peak
- **Synchronization overhead**: Locking mechanisms (if required) can limit scalability

### IronPDF — Concurrent Processing

```csharp
using IronPdf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";

var concurrencyLevel = 4;
var documentsPerThread = 10;
var stopwatch = Stopwatch.StartNew();

var renderer = new ChromePdfRenderer(); // Thread-safe, can be shared

var tasks = Enumerable.Range(0, concurrencyLevel)
    .Select(threadId => Task.Run(async () =>
    {
        for (int i = 0; i < documentsPerThread; i++)
        {
            var html = $@"
                <html>
                <body>
                    <h1>Thread {threadId}, Doc {i}</h1>
                    <p>Timestamp: {DateTime.Now}</p>
                </body>
                </html>";

            var pdf = await renderer.RenderHtmlAsPdfAsync(html);
            pdf.SaveAs($"thread{threadId}_doc{i}.pdf");
        }
    }))
    .ToArray();

await Task.WhenAll(tasks);

stopwatch.Stop();
var totalDocs = concurrencyLevel * documentsPerThread;
Console.WriteLine($"Total time: {stopwatch.ElapsedMilliseconds}ms");
Console.WriteLine($"Throughput: {totalDocs / (stopwatch.ElapsedMilliseconds / 1000.0):F2} docs/sec");
```

IronPDF's `ChromePdfRenderer` is designed to be shared across concurrent calls without locking or separate instances.

---

## API Mapping Reference

| Operation | ExpertPdf | IronPDF |
|-----------|-----------|---------|
| Initialize | `new PdfConverter()` | `new ChromePdfRenderer()` |
| HTML string to PDF | `GetPdfBytesFromHtmlString(html)` | `RenderHtmlAsPdf(html)` |
| HTML URL to PDF | `GetPdfBytesFromUrl(url)` | `RenderUrlAsPdf(url)` |
| HTML file to PDF | `GetPdfBytesFromHtmlFile(path)` | `RenderHtmlFileAsPdf(path)` |
| Async conversion | Limited | `RenderHtmlAsPdfAsync(html)` |
| Set timeout | `NavigationTimeout` (seconds) | `RenderingOptions.Timeout` (seconds) |
| Page size | `PdfPageSize.A4` | `PaperSize = PdfPaperSize.A4` |
| Page orientation | `PdfPageOrientation.Portrait` | `PaperOrientation = PdfPaperOrientation.Portrait` |
| Margins | `PdfDocumentOptions.MarginTop/...` | `MarginTop/Bottom/Left/Right` |
| Headers/footers | `PdfHeaderOptions` / `PdfFooterOptions` | `HtmlHeaderFooter` / `TextHeaderFooter` |
| Page number tokens | `&p;` / `&P;` | `{page}` / `{total-pages}` |

---

## Comprehensive Feature Comparison

### Status
| Feature | ExpertPdf | IronPDF |
|---------|-----------|---------|
| Active development | v20.1.0 (Apr 2025) | Continuously updated |
| .NET 8 support | Via .NET Standard 2.0 (Windows) | Native, cross-platform |
| .NET 9 support | Via .NET Standard 2.0 (Windows) | Native, cross-platform |
| .NET 10 support | Verify with vendor | Native, cross-platform |
| .NET Framework 4.6.1+ | Yes | Yes |
| Linux support | Not supported | Yes |
| macOS support | Not supported | Yes |

### Support
| Feature | ExpertPdf | IronPDF |
|---------|-----------|---------|
| Documentation | Reference docs on html-to-pdf.net | Comprehensive docs and tutorials |
| Code examples | Reference samples | 100+ examples |
| Community forum | Verify availability | Active forum |
| Technical support | Commercial vendor support | Engineering support |
| Response time | Verify SLA with vendor | Published support tiers |

### Content Creation
| Feature | ExpertPdf | IronPDF |
|---------|-----------|---------|
| HTML5 support | Best on WebKit2 engine | Chromium |
| CSS3 support | Best on WebKit2 engine | Full CSS3 |
| JavaScript execution | Yes (engine-dependent) | Yes |
| Web fonts | Yes | Google Fonts, custom |
| SVG rendering | Yes | Native |
| Canvas rendering | Engine-dependent | Yes |
| Responsive layouts | Engine-dependent | Media queries |
| Print CSS | Yes | @media print |

### PDF Operations
| Feature | ExpertPdf | IronPDF |
|---------|-----------|---------|
| Merge PDFs | Separate `ExpertPdf.MergePdf` | Built-in |
| Split PDFs | Separate `ExpertPdf.SplitPdf` | Built-in |
| Extract pages | Yes | Yes |
| Rotate pages | Yes | Yes |
| Extract text | Separate component | Built-in |
| Extract images | Separate component | Built-in |
| Form filling | Yes | AcroForm |
| Headers/footers | Yes (token-based) | HTML-based and text-based |
| Page numbers | Token-based | Dynamic |

### Security
| Feature | ExpertPdf | IronPDF |
|---------|-----------|---------|
| Password encryption | Yes (separate `ExpertPdf.PdfSecurity`) | 128/256-bit AES |
| User permissions | Yes | Granular |
| Digital signatures | Separate component | Built-in X.509 |
| Redaction | Verify with vendor | Permanent |
| Metadata | Yes | Yes |

### Performance
| Metric | ExpertPdf | IronPDF |
|--------|-----------|---------|
| Cold start time | Benchmark in your environment | Benchmark in your environment |
| Warm conversion | Benchmark in your environment | Benchmark in your environment |
| Memory footprint | Varies by document | Stream-based processing |
| Thread safety | Verify with vendor | Thread-safe renderer |
| Concurrent throughput | Test with your workload | Linear scaling to cores |
| Async support | Limited | Full async/await |

### Development
| Feature | ExpertPdf | IronPDF |
|---------|-----------|---------|
| NuGet installation | Multiple packages | One package |
| External dependencies | Verify requirements | Bundled native binaries |
| Component licensing | Separate per component | All-inclusive |
| Error messages | Standard exceptions | Detailed diagnostics |
| Testing support | Unit testable | Unit testable |

---

## Installation Comparison

### ExpertPdf Installation
```bash
# HTML to PDF (pick the variant for your target framework)
Install-Package ExpertPdf.HtmlToPdf.NetCore   # .NET Core / 5-9
Install-Package ExpertPdfHtmlToPdf            # .NET Framework

# Additional components require separate packages
Install-Package ExpertPdf.MergePdf
Install-Package ExpertPdf.SplitPdf
Install-Package ExpertPdf.PdfSecurity
Install-Package ExpertPdf.PdfToImage
Install-Package ExpertPdf.PdfCreator
```

```csharp
using ExpertPdf.HtmlToPdf;
using ExpertPdf.PdfCreator; // If using creator features
```

### IronPDF Installation
```bash
Install-Package IronPdf
```

```csharp
using IronPdf;
using IronPdf.Rendering;
```

---

## When Migration Becomes Mandatory

Performance and platform requirements drive migration more often than feature gaps. Teams encounter forcing functions around ExpertPdf when:

**Throughput requirements exceed single-threaded capacity**: If your batch jobs require processing thousands of PDFs per hour and single-threaded sequential processing cannot meet that target, you need concurrent execution. ExpertPdf's thread safety model may require architectural changes to scale horizontally — confirm the vendor's guidance.

**Licensing surface compounds**: When your workflow requires HTML conversion, merging, splitting, text extraction, and security operations, licensing each ExpertPdf component separately creates budget and version-coordination complexity.

**Cross-platform deployment**: Teams standardizing on Linux containers or Azure App Service Linux plans cannot use ExpertPdf. The infrastructure cost of maintaining Windows-specific environments for PDF generation often exceeds the licensing savings.

**Real-time latency requirements**: If you generate PDFs in API request paths where users wait for responses, cold start times and conversion latency directly affect user experience. Profiling your specific documents under load reveals whether optimization is possible or migration is necessary.

## IronPDF's Technical Approach

IronPDF optimizes for server-side throughput through several architectural choices:

**Thread-safe by design**: The `ChromePdfRenderer` class handles concurrent calls from multiple threads without locking or separate instances, enabling scaling with available cores.

**Stream-based operations**: PDFs can be generated directly to `MemoryStream` or network streams without intermediate file system I/O:
```csharp
var pdf = renderer.RenderHtmlAsPdf(html);
await pdf.SaveAsAsync(Response.Body); // ASP.NET Core
```

**Async/await support**: All rendering methods have async counterparts (`RenderHtmlAsPdfAsync`) for non-blocking operation in high-concurrency scenarios.

**Resource pooling**: The library manages internal process pools to amortize startup costs across multiple conversions.

For detailed performance optimization guidance, see the [HTML to PDF tutorial](https://ironpdf.com/tutorials/html-to-pdf/) which covers caching strategies, memory management, and load testing methodologies.

---

**What performance metrics matter most for your PDF workflows — throughput, latency, memory footprint, or concurrent capacity? Share your benchmarking experiences in the comments.**

**Learn more:**
- [HTML to PDF conversion guide](https://ironpdf.com/tutorials/html-to-pdf/)
- [Merge PDFs efficiently](https://ironpdf.com/how-to/merge-or-split-pdfs/)
