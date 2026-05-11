---
title: "Gotenberg vs IronPDF: a developer comparison for 2026"
published: false
tags: dotnet, csharp, pdf, comparison
---
# Gotenberg vs IronPDF: a developer comparison for 2026

Canonical/source version on Iron Software blog: [IronPDF Documentation](https://ironsoftware.com/object-reference/api/IronPdf.IPdfRenderOptions.html)

Gotenberg represents a different architectural philosophy: PDF generation as a containerized microservice accessible via HTTP. .NET applications send HTML, URLs, or Office documents to a Gotenberg container and receive PDFs via HTTP response. This approach makes sense in polyglot environments where Python, Node.js, and .NET services all need PDF generation without language-specific SDKs. IronPDF takes the opposite approach—a native .NET library that integrates directly into your application process.

The choice between microservice and library architectures affects deployment complexity, latency characteristics, error handling patterns, and operational monitoring requirements. Teams running Kubernetes clusters may prefer Gotenberg's service model. Teams deploying .NET applications to Azure App Service or AWS Lambda often find IronPDF's in-process design simpler. The comparison below uses a checklist format to evaluate each approach across infrastructure, development workflow, and operational dimensions.

## Understanding IronPDF

IronPDF is a NuGet package that embeds a Chrome rendering engine directly into your .NET application. HTML-to-PDF conversion happens in-process, eliminating network calls and external service dependencies. The library supports .NET Framework 4.6.2+, .NET Core 3.1+, and .NET 6-10 with consistent behavior across Windows and Linux.

Beyond HTML rendering, IronPDF provides PDF manipulation APIs within the same package: merging files, extracting text, filling forms, adding digital signatures. Deployment involves installing a NuGet package—no container orchestration, no service health monitoring, no network configuration. For detailed rendering options, see [HTML to PDF Conversion Tutorial](https://ironsoftware.com/suite/blog/using-ironsuite/html-to-pdf-ironpdf-tutorial/).

## Key Limitations of Gotenberg

### Product Status
Gotenberg version 8.x is actively developed and maintained as an open-source project. The Docker-first design means .NET developers must manage container infrastructure, image updates, and service availability. For teams without existing container orchestration platforms, this introduces operational overhead.

### Missing Capabilities
Gotenberg provides no .NET-native API. All interactions happen via HTTP multipart/form-data requests. Error handling involves parsing HTTP status codes and response bodies. No type-safe APIs, no IntelliSense, no compile-time validation. Teams must implement HTTP client logic, retry mechanisms, and timeout handling manually.

### Technical Issues
The HTTP API creates latency overhead. Each PDF generation requires: serializing HTML/documents to HTTP request, network round-trip to Gotenberg service, Chromium processing time, network round-trip back, deserializing response. For high-throughput scenarios generating thousands of PDFs, network latency accumulates. Synchronous HTTP calls block threads while waiting for Gotenberg responses.

### Support Status
Gotenberg is community-supported open-source software. No SLA guarantees, no commercial support contracts. Issue resolution depends on community responsiveness. Security patches and updates require monitoring GitHub releases and rebuilding container images manually.

### Architecture Problems
Running Gotenberg requires Docker runtime, container orchestration (Docker Compose, Kubernetes, etc.), and network infrastructure for service discovery. Memory consumption is high—the Docker image includes full Chromium and LibreOffice installations (~600-800MB). Horizontal scaling requires load balancer configuration and shared session state management if using webhooks.

## Feature Comparison Overview

| Feature | Gotenberg | IronPDF |
|---------|-----------|---------|
| **Current Status** | Active open-source (v8.x) | Active commercial product |
| **HTML Support** | Chromium via HTTP API | Native Chrome engine |
| **Rendering Quality** | High-quality (Chromium) | Pixel-perfect (Chrome) |
| **Installation** | Docker image + HTTP client | Single NuGet package |
| **Support** | Community (GitHub issues) | 24/7 live chat, email, tickets |
| **Future Viability** | Open-source maintenance model | Commercial vendor support |

---

## Integration Checklist Comparison

### Infrastructure Requirements Checklist

**Gotenberg Requirements:**
- ✅ Docker runtime installed on all environments (dev, staging, prod)
- ✅ Container orchestration platform (Docker Compose, Kubernetes, ECS, etc.)
- ✅ Network configuration for service discovery
- ✅ Health check monitoring endpoints configured
- ✅ Resource limits defined (CPU, memory for Chromium processes)
- ✅ Image update process for security patches
- ✅ Load balancer configuration for horizontal scaling
- ✅ Persistent storage for webhook outputs (if using async mode)
- ✅ Network firewall rules allowing HTTP traffic between services
- ✅ Container security scanning in CI/CD pipeline

**IronPDF Requirements:**
- ✅ NuGet package reference in .csproj
- ✅ License key configuration (for production use)
- ⚠️ libgdiplus package on Linux for certain image operations
- ⚠️ Adequate process memory for Chrome engine (typically 256MB+)

---

### Development Workflow Checklist

**Gotenberg Development Setup:**
- ✅ Docker Desktop installed locally
- ✅ Gotenberg container running: `docker run -p 3000:3000 gotenberg/gotenberg:8`
- ✅ HTTP client library configured (HttpClient, RestSharp, etc.)
- ✅ Multipart/form-data request builder implemented
- ✅ Error response parser implemented
- ✅ Retry logic for transient network failures
- ✅ Timeout configuration for long-running conversions
- ✅ Unit tests mocked with HTTP response fixtures
- ✅ Integration tests with actual Gotenberg container
- ✅ Environment-specific endpoint configuration

**IronPDF Development Setup:**
- ✅ NuGet package installed: `Install-Package IronPdf`
- ✅ Namespace imported: `using IronPdf;`
- ✅ IntelliSense and type safety available immediately
- ✅ Unit tests run in-process (no external dependencies)
- ✅ Debugging with breakpoints in rendering pipeline

---

### Deployment Checklist

**Gotenberg Deployment:**
- ✅ Container image built and pushed to registry
- ✅ Kubernetes manifests or Docker Compose files configured
- ✅ Service discovery configured (Kubernetes Service, Docker network, etc.)
- ✅ Health probes defined (`/health` endpoint)
- ✅ Resource requests and limits specified
- ✅ Horizontal pod autoscaler configured (if using Kubernetes)
- ✅ Network policies defined for security
- ✅ Monitoring and logging infrastructure integrated
- ✅ Alert thresholds configured for service downtime
- ✅ Backup and disaster recovery plan for service unavailability

**IronPDF Deployment:**
- ✅ NuGet package included in build output
- ✅ Platform-specific runtime packages deployed automatically
- ✅ License key configured via environment variable or config file
- ✅ Application published (no separate service deployment)

---

### Code Comparison: Implementation Patterns

#### Gotenberg — HTML to PDF via HTTP API

```csharp
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class GotenbergPdfGenerator
{
    private static readonly HttpClient httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://gotenberg-service:3000"),
        Timeout = TimeSpan.FromMinutes(5) // Long timeout for complex renders
    };

    public static async Task<byte[]> GeneratePdfFromHtml(string htmlContent)
    {
        try
        {
            // Create multipart form data
            using var multipartContent = new MultipartFormDataContent();

            // Add HTML file as form field
            var htmlBytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);
            var htmlStream = new MemoryStream(htmlBytes);
            var htmlFileContent = new StreamContent(htmlStream);
            htmlFileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");

            multipartContent.Add(htmlFileContent, "files", "index.html");

            // Optional: Add custom headers for Gotenberg configuration
            multipartContent.Add(
                new StringContent("1080"),
                "emulatedMediaType",
                "screen"
            );

            // Send request to Gotenberg
            var response = await httpClient.PostAsync(
                "/forms/chromium/convert/html",
                multipartContent
            );

            // Check response status
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Gotenberg error (HTTP {response.StatusCode}): {errorBody}"
                );
            }

            // Read PDF bytes from response
            var pdfBytes = await response.Content.ReadAsByteArrayAsync();

            Console.WriteLine($"Generated PDF: {pdfBytes.Length} bytes");
            return pdfBytes;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error connecting to Gotenberg: {ex.Message}");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request timeout: {ex.Message}");
            throw;
        }
    }

    public static async Task GenerateInvoicePdf()
    {
        var htmlContent = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; padding: 40px; }
                    h1 { font-size: 24px; }
                    table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                    th, td { padding: 10px; border: 1px solid #ddd; text-align: left; }
                    th { background: #f0f0f0; }
                </style>
            </head>
            <body>
                <h1>Invoice #12345</h1>
                <p>Date: " + DateTime.Now.ToString("yyyy-MM-dd") + @"</p>
                <p>Customer: Acme Corporation</p>

                <table>
                    <thead>
                        <tr><th>Item</th><th>Quantity</th><th>Price</th><th>Total</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>Professional Services</td><td>40</td><td>$150</td><td>$6,000</td></tr>
                        <tr><td>Consulting</td><td>10</td><td>$200</td><td>$2,000</td></tr>
                    </tbody>
                </table>
                <p><strong>Total: $8,000</strong></p>
            </body>
            </html>";

        var pdfBytes = await GeneratePdfFromHtml(htmlContent);
        File.WriteAllBytes("invoice_gotenberg.pdf", pdfBytes);
        Console.WriteLine("Invoice created: invoice_gotenberg.pdf");
    }
}
```

**Checklist: Gotenberg HTTP Implementation Concerns**
- ✅ HttpClient instance configured with base address
- ✅ Timeout set appropriately for document complexity
- ✅ Multipart/form-data request constructed manually
- ✅ Content-Type headers specified for HTML
- ✅ HTTP status code error handling implemented
- ✅ Network exception handling (HttpRequestException)
- ✅ Timeout exception handling (TaskCanceledException)
- ⚠️ No type safety—field names are string literals
- ⚠️ No IntelliSense for Gotenberg configuration options
- ⚠️ Response parsing limited to raw bytes
- ⚠️ Async-only API (no synchronous option for legacy code)
- ⚠️ Service availability required for local development
- ⚠️ Network latency affects performance

---

#### IronPDF — HTML to PDF In-Process

```csharp
using IronPdf;
using System;

public class IronPdfGenerator
{
    public static void GenerateInvoicePdf()
    {
        var renderer = new ChromePdfRenderer();

        var htmlContent = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial; padding: 40px; }
                    h1 { font-size: 24px; }
                    table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                    th, td { padding: 10px; border: 1px solid #ddd; text-align: left; }
                    th { background: #f0f0f0; }
                </style>
            </head>
            <body>
                <h1>Invoice #12345</h1>
                <p>Date: " + DateTime.Now.ToString("yyyy-MM-dd") + @"</p>
                <p>Customer: Acme Corporation</p>

                <table>
                    <thead>
                        <tr><th>Item</th><th>Quantity</th><th>Price</th><th>Total</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>Professional Services</td><td>40</td><td>$150</td><td>$6,000</td></tr>
                        <tr><td>Consulting</td><td>10</td><td>$200</td><td>$2,000</td></tr>
                    </tbody>
                </table>
                <p><strong>Total: $8,000</strong></p>
            </body>
            </html>";

        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("invoice_ironpdf.pdf");

        Console.WriteLine("Invoice created: invoice_ironpdf.pdf");
    }
}
```

**Checklist: IronPDF In-Process Implementation Benefits**
- ✅ Type-safe API with IntelliSense support
- ✅ No network configuration or service dependencies
- ✅ Synchronous and async methods available
- ✅ Rich exception context with stack traces
- ✅ Debugging with breakpoints in rendering pipeline
- ✅ No external service availability requirements
- ✅ Zero network latency overhead
- ✅ Integrated error handling and validation
- ✅ Platform-specific optimizations built-in

---

### Operational Monitoring Checklist

**Gotenberg Monitoring Requirements:**
- ✅ Service health endpoint monitoring (`/health`)
- ✅ Container resource usage metrics (CPU, memory)
- ✅ HTTP request latency tracking
- ✅ Error rate monitoring (4xx, 5xx responses)
- ✅ Queue depth monitoring (concurrent request limits)
- ✅ Docker container logs aggregation
- ✅ Chromium crash detection and alerting
- ✅ Network connectivity monitoring between services
- ✅ Load balancer health check configuration
- ✅ Horizontal scaling metrics (pod count, request distribution)
- ⚠️ Requires separate monitoring infrastructure
- ⚠️ Multiple failure points: container, network, service

**IronPDF Monitoring Requirements:**
- ✅ Application-level exception tracking
- ✅ Performance counters for PDF generation timing
- ⚠️ Standard application monitoring (no separate service)
- ⚠️ Memory usage monitoring for Chrome engine
- ✅ Single process to monitor

---

### Cost Analysis Checklist

**Gotenberg Operational Costs:**
- Container orchestration platform costs (EKS, AKS, GKE)
- Load balancer costs for service exposure
- Container registry storage costs
- Increased compute resources for separate service
- DevOps time for container deployment and management
- Monitoring infrastructure costs
- Network egress costs between services

**IronPDF Operational Costs:**
- License cost (one-time or annual)
- No additional infrastructure costs
- No separate service management overhead
- Included in application deployment costs

---

### API Mapping Reference

| Gotenberg HTTP API | IronPDF Equivalent |
|-------------------|-------------------|
| POST `/forms/chromium/convert/html` | `ChromePdfRenderer.RenderHtmlAsPdf()` |
| POST `/forms/chromium/convert/url` | `ChromePdfRenderer.RenderUrlAsPdf()` |
| POST `/forms/libreoffice/convert` | IronPDF does not convert Office docs natively |
| POST `/forms/pdfengines/merge` | `PdfDocument.Merge()` |
| Multipart form-data with file fields | Direct method parameters |
| HTTP headers for configuration | `ChromePdfRenderOptions` properties |
| Webhook for async results | Async/await methods |
| HTTP status codes for errors | Managed exceptions |
| Manual HTTP client management | Automatic resource management |

---

### Comprehensive Feature Comparison

#### Status & Support

| Feature | Gotenberg | IronPDF |
|---------|-----------|---------|
| Current Development Status | Active open-source (v8.x) | Active commercial |
| Support Model | Community (GitHub) | 24/7 commercial support |
| SLA Guarantees | None | Available with license |
| Documentation | GitHub documentation | Comprehensive with examples |
| Version Updates | Manual container rebuilds | NuGet package updates |
| Security Patches | Monitor GitHub releases | Automatic via NuGet |

#### Deployment Architecture

| Feature | Gotenberg | IronPDF |
|---------|-----------|---------|
| Deployment Model | Microservice container | In-process library |
| Infrastructure Required | Docker + orchestration | NuGet package |
| Network Configuration | HTTP endpoint + load balancer | None |
| Service Discovery | Required | N/A |
| Horizontal Scaling | Via container orchestration | Via application instances |
| Health Monitoring | Required | Application-level |

#### Development Experience

| Feature | Gotenberg | IronPDF |
|---------|-----------|---------|
| API Type | HTTP REST with multipart/form-data | Native .NET API |
| Type Safety | None (string-based) | Full IntelliSense |
| Local Development | Requires Docker | Direct integration |
| Debugging | HTTP inspection tools | Visual Studio debugger |
| Error Handling | HTTP status codes | Managed exceptions |
| Async Support | HTTP async | Native async/await |

#### Performance Characteristics

| Feature | Gotenberg | IronPDF |
|---------|-----------|---------|
| Latency Overhead | Network round-trip + processing | In-process only |
| Throughput | Limited by service capacity | Limited by application resources |
| Cold Start | Container startup time | Chrome engine initialization (~500ms) |
| Memory Usage | ~600-800MB per container | ~100-200MB per process |
| Concurrent Requests | Configured queue size | Thread pool limited |

#### Content Creation

| Feature | Gotenberg | IronPDF |
|---------|-----------|---------|
| HTML to PDF | Chromium via HTTP | Native Chrome engine |
| URL to PDF | Via HTTP endpoint | `RenderUrlAsPdf()` |
| Office Documents to PDF | LibreOffice support | Not natively supported |
| Markdown to PDF | Via HTTP endpoint | Render HTML from Markdown |
| JavaScript Execution | Supported | Full Chrome JavaScript |
| CSS3 Support | Complete | Complete |

#### Known Issues

**Gotenberg Environment-Specific Issues:**
- Docker Desktop license requirements for large organizations
- Container startup time affects cold starts in serverless environments
- Network partition can cause cascading failures
- Chromium process crashes require container restart
- Memory leaks in long-running containers (restart required)
- Load balancer configuration complexity for sticky sessions
- Image size (~600-800MB) affects deployment time

**IronPDF Environment-Specific Issues:**
- Initial Chrome engine startup ~500ms per process
- Linux deployments need libgdiplus for certain operations
- Docker containers require memory configuration for Chrome process

---

### Installation Comparison

#### Gotenberg Installation

**Step 1: Install Docker**
Follow Docker installation for your platform: https://docs.docker.com/get-docker/

**Step 2: Run Gotenberg Container**
```bash
# Basic deployment
docker run --rm -p 3000:3000 gotenberg/gotenberg:8

# Production deployment with Docker Compose
# docker-compose.yml
version: '3.8'
services:
  gotenberg:
    image: gotenberg/gotenberg:8
    ports:
      - "3000:3000"
    environment:
      - API_TIMEOUT=30s
      - CHROMIUM_MAX_QUEUE_SIZE=10
    restart: always
```

**Step 3: Configure .NET HTTP Client**
```csharp
using System.Net.Http;

var httpClient = new HttpClient
{
    BaseAddress = new Uri("http://gotenberg-service:3000"),
    Timeout = TimeSpan.FromMinutes(5)
};
```

**Deployment Checklist:**
- ✅ Docker installed on all environments
- ✅ Container orchestration configured
- ✅ Service discovery endpoints set
- ✅ Health checks configured
- ✅ Resource limits defined
- ✅ Monitoring and logging integrated

---

#### IronPDF Installation

```bash
# NuGet Package Manager
Install-Package IronPdf

# .NET CLI
dotnet add package IronPdf
```

**Configuration:**
```csharp
using IronPdf;

// Optional license configuration
IronPdf.License.LicenseKey = "YOUR-LICENSE-KEY";
```

**Deployment Checklist:**
- ✅ NuGet package referenced in project
- ✅ License key configured (production)

---

## Architectural Decision Checklist

### Choose Gotenberg If:
- ✅ You operate a polyglot microservices architecture (Python, Node.js, Java, .NET)
- ✅ You have existing Kubernetes/Docker Swarm infrastructure
- ✅ Your DevOps team has container orchestration expertise
- ✅ You need to convert Office documents (Word, Excel, PowerPoint) to PDF
- ✅ You require service-level isolation for PDF generation
- ✅ Your organization has Docker licensing for commercial use
- ✅ You're comfortable managing separate service dependencies
- ✅ Network latency overhead is acceptable for your use case
- ✅ You have monitoring infrastructure for microservices

### Choose IronPDF If:
- ✅ You're building .NET-centric applications
- ✅ You need type-safe APIs with IntelliSense
- ✅ You want to minimize infrastructure complexity
- ✅ You require low-latency PDF generation
- ✅ You need comprehensive .NET debugging capabilities
- ✅ Your deployment targets don't support Docker (Azure App Service, traditional hosting)
- ✅ You want single-process deployment simplicity
- ✅ You need commercial support with SLAs
- ✅ Network calls are a performance concern
- ✅ You prefer vendor-supported libraries over community projects

---

## Conclusion

Gotenberg and IronPDF represent fundamentally different architectural approaches to PDF generation. Gotenberg's microservice model makes sense in polyglot environments where multiple programming languages need PDF generation without implementing language-specific integrations. The Docker-based deployment provides isolation and enables horizontal scaling through container orchestration. However, this architecture introduces operational complexity: container management, service discovery, network configuration, health monitoring, and multi-stage deployment pipelines.

For .NET-centric applications, Gotenberg requires building and maintaining HTTP client infrastructure that handles multipart/form-data requests, parses response codes, implements retry logic, and manages timeouts. Each PDF generation adds network round-trip latency. Debugging involves HTTP inspection tools rather than stepping through managed code. Type safety disappears at the HTTP boundary—configuration options become string literals susceptible to typos and runtime errors.

IronPDF consolidates PDF generation into a single NuGet package with a type-safe, IntelliSense-supported API. Deployment involves no Docker configuration, no service health monitoring, no network infrastructure. In-process execution eliminates network latency and simplifies debugging with Visual Studio breakpoints. For teams without existing container orchestration platforms, this reduces operational overhead significantly.

The choice depends on architectural context. Polyglot teams with mature Kubernetes infrastructure may find Gotenberg's service-based model aligns with existing patterns. .NET teams prioritizing development velocity and deployment simplicity typically benefit from IronPDF's library-based integration. Both approaches deliver high-quality PDF rendering; the decision hinges on infrastructure philosophy and operational complexity tolerance.

Does your team run production containers for infrastructure services, or do you prefer keeping dependencies within application processes? Share your experience with microservice vs. library architectures for PDF generation.

**Related Resources:**
- [HTML to PDF with IronPDF](https://ironsoftware.com/suite/blog/using-ironsuite/html-to-pdf-ironpdf-tutorial/)
- [IronPDF API Reference](https://ironsoftware.com/object-reference/api/IronPdf.IPdfRenderOptions.html)
