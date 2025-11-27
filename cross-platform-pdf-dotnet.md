# Cross-Platform PDF Generation in .NET: Windows, Linux, Docker, and Cloud

**By [Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/)** — CTO of Iron Software, Creator of IronPDF

[![.NET](https://img.shields.io/badge/.NET-6%20%7C%207%20%7C%208%20%7C%209-512BD4)](https://dotnet.microsoft.com/)
[![Platforms](https://img.shields.io/badge/Platforms-Windows%20%7C%20Linux%20%7C%20macOS%20%7C%20Docker-blue)]()
[![Last Updated](https://img.shields.io/badge/Updated-November%202025-green)]()

> "Cross-platform" is the most abused claim in PDF library marketing. This guide tells you what actually works—and what fails—on Windows, Linux, macOS, Docker, Azure, and AWS.

---

## Table of Contents

1. [The Cross-Platform Reality](#the-cross-platform-reality)
2. [Platform Support Matrix](#platform-support-matrix)
3. [Windows Deployment](#windows-deployment)
4. [Linux Deployment](#linux-deployment)
5. [Docker Containers](#docker-containers)
6. [macOS Deployment](#macos-deployment)
7. [Azure Deployment](#azure-deployment)
8. [AWS Deployment](#aws-deployment)
9. [.NET MAUI and Blazor](#net-maui-and-blazor)
10. [Common Cross-Platform Issues](#common-cross-platform-issues)

---

## The Cross-Platform Reality

After building IronPDF to work genuinely cross-platform across 10+ Linux distributions, Windows variants, and cloud environments, I can tell you: **most "cross-platform" claims are lies.**

### What Libraries Actually Support

| Library | Windows | Linux | macOS | Docker | Azure | AWS Lambda |
|---------|---------|-------|-------|--------|-------|------------|
| **IronPDF** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| PuppeteerSharp | ✅ | ✅ | ✅ | ⚠️ Size | ⚠️ Cold start | ⚠️ Size |
| Playwright | ✅ | ✅ | ✅ | ⚠️ Size | ⚠️ Cold start | ⚠️ Size |
| **SelectPdf** | ✅ | ❌ | ❌ | ❌ | ⚠️ Windows only | ❌ |
| **WebView2** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| PDFSharp | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| QuestPDF | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| wkhtmltopdf | ✅ | ⚠️ | ⚠️ | ⚠️ | ⚠️ | ⚠️ |

**Key insight:** If you need HTML-to-PDF with modern CSS, your real options are IronPDF, PuppeteerSharp, or Playwright.

---

## Platform Support Matrix

### IronPDF Specific Support

| Platform | Version | Architecture | Status |
|----------|---------|--------------|--------|
| Windows 10/11 | All | x64, x86, ARM64 | ✅ Full |
| Windows Server | 2016+ | x64 | ✅ Full |
| Ubuntu | 20.04, 22.04, 24.04 | x64, ARM64 | ✅ Full |
| Debian | 10, 11, 12 | x64 | ✅ Full |
| CentOS | 7, 8 Stream | x64 | ✅ Full |
| RHEL | 8, 9 | x64 | ✅ Full |
| Alpine | 3.17+ | x64 | ✅ Full |
| Amazon Linux | 2, 2023 | x64 | ✅ Full |
| macOS | 12+ (Monterey) | Intel, Apple Silicon | ✅ Full |
| Docker | All base images | x64, ARM64 | ✅ Full |
| Azure App Service | Windows, Linux | - | ✅ Full |
| Azure Functions | v4 | - | ✅ Full |
| AWS Lambda | .NET 6/8 | x64, ARM64 | ✅ Full |
| AWS ECS/Fargate | - | x64 | ✅ Full |

---

## Windows Deployment

Windows deployment is the most straightforward.

### Installation

```bash
dotnet add package IronPdf
```

### Basic Usage

```csharp
using IronPdf;

var pdf = ChromePdfRenderer.RenderHtmlAsPdf("<h1>Hello Windows</h1>");
pdf.SaveAs("output.pdf");
```

### Windows Server Considerations

For Windows Server without desktop experience:

```csharp
// IronPDF handles headless rendering automatically
// No additional configuration needed
```

### IIS Deployment

```xml
<!-- web.config -->
<configuration>
  <system.webServer>
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength="52428800" />
      </requestFiltering>
    </security>
  </system.webServer>
</configuration>
```

Ensure app pool identity has write access to temp directory.

---

## Linux Deployment

Linux requires installing dependencies for Chromium.

### Ubuntu/Debian

```bash
# Install dependencies
apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libx11-xcb1 \
    libxcomposite1 \
    libxcursor1 \
    libxdamage1 \
    libxi6 \
    libxtst6 \
    libnss3 \
    libcups2 \
    libxss1 \
    libxrandr2 \
    libasound2 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libpangocairo-1.0-0 \
    libgtk-3-0 \
    libgbm1 \
    fonts-liberation
```

### CentOS/RHEL

```bash
# Install EPEL first
yum install -y epel-release

# Install dependencies
yum install -y \
    libgdiplus \
    libX11-xcb \
    libXcomposite \
    libXcursor \
    libXdamage \
    libXi \
    libXtst \
    nss \
    cups-libs \
    libXScrnSaver \
    libXrandr \
    alsa-lib \
    atk \
    at-spi2-atk \
    pango \
    gtk3 \
    libgbm \
    liberation-fonts
```

### Alpine Linux

```bash
# Alpine uses musl, needs extra packages
apk add --no-cache \
    libgdiplus \
    chromium \
    nss \
    freetype \
    harfbuzz \
    ca-certificates \
    ttf-freefont
```

### Automatic Dependency Configuration

```csharp
// Enable automatic Linux dependency detection
Installation.LinuxAndDockerDependenciesAutoConfig = true;
```

---

## Docker Containers

### Recommended Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install Chromium dependencies
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    libx11-xcb1 \
    libxcomposite1 \
    libxcursor1 \
    libxdamage1 \
    libxi6 \
    libxtst6 \
    libnss3 \
    libcups2 \
    libxss1 \
    libxrandr2 \
    libasound2 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libpangocairo-1.0-0 \
    libgtk-3-0 \
    libgbm1 \
    fonts-liberation \
    fonts-noto-cjk \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

### Alpine-Based (Smaller Image)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

RUN apk add --no-cache \
    libgdiplus \
    icu-libs \
    chromium \
    nss \
    freetype \
    harfbuzz \
    ca-certificates \
    ttf-freefont

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

### Docker Compose

```yaml
version: '3.8'
services:
  web:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    volumes:
      - pdf-output:/app/output

volumes:
  pdf-output:
```

### Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: pdf-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: pdf-service
  template:
    metadata:
      labels:
        app: pdf-service
    spec:
      containers:
      - name: pdf-service
        image: your-registry/pdf-service:latest
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "2Gi"
            cpu: "2000m"
        securityContext:
          allowPrivilegeEscalation: false
```

---

## macOS Deployment

### Intel Macs

```bash
dotnet add package IronPdf
```

Works out of the box.

### Apple Silicon (M1/M2/M3)

```bash
dotnet add package IronPdf
```

IronPDF includes native ARM64 binaries. No Rosetta required.

### Development Setup

```csharp
using IronPdf;

// Works identically on macOS
var pdf = ChromePdfRenderer.RenderHtmlAsPdf("<h1>Hello macOS</h1>");
pdf.SaveAs("output.pdf");
```

---

## Azure Deployment

### Azure App Service (Linux)

```yaml
# azure-pipelines.yml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: DotNetCoreCLI@2
    inputs:
      command: 'publish'
      publishWebProjects: true
      arguments: '--configuration Release --output $(Build.ArtifactStagingDirectory)'

  - task: AzureWebApp@1
    inputs:
      azureSubscription: 'Your-Subscription'
      appType: 'webAppLinux'
      appName: 'your-app-name'
      package: '$(Build.ArtifactStagingDirectory)/**/*.zip'
```

**Recommended tier:** P1v2 or higher (sufficient memory for Chromium)

### Azure App Service (Windows)

Works out of the box. No special configuration needed.

### Azure Functions

```csharp
public class PdfFunction
{
    [FunctionName("GeneratePdf")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string html = await new StreamReader(req.Body).ReadToEndAsync();

        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

        return new FileContentResult(pdf.BinaryData, "application/pdf")
        {
            FileDownloadName = "document.pdf"
        };
    }
}
```

**Configuration:**
- Use Consumption Plan or Premium Plan
- Set `FUNCTIONS_WORKER_RUNTIME` to `dotnet-isolated`
- Ensure sufficient memory (256MB+ recommended)

### Azure Container Apps

```yaml
# containerapp.yaml
properties:
  configuration:
    ingress:
      external: true
      targetPort: 80
  template:
    containers:
      - image: your-registry.azurecr.io/pdf-service:latest
        name: pdf-service
        resources:
          cpu: 1
          memory: 2Gi
```

---

## AWS Deployment

### AWS Lambda

```csharp
public class Function
{
    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Enable Linux dependencies
        Installation.LinuxAndDockerDependenciesAutoConfig = true;

        var html = request.Body;
        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/pdf" },
                { "Content-Disposition", "attachment; filename=document.pdf" }
            },
            Body = Convert.ToBase64String(pdf.BinaryData),
            IsBase64Encoded = true
        };
    }
}
```

**Configuration:**
- Memory: 512MB minimum, 1024MB recommended
- Timeout: 30 seconds minimum
- Architecture: x86_64 or arm64

### AWS Lambda Container Image

```dockerfile
FROM public.ecr.aws/lambda/dotnet:8

# Install dependencies
RUN yum install -y \
    libgdiplus \
    libX11-xcb \
    nss \
    cups-libs \
    libXScrnSaver \
    alsa-lib \
    atk \
    gtk3 \
    && yum clean all

COPY bin/Release/net8.0/linux-x64/publish/ ${LAMBDA_TASK_ROOT}

CMD ["YourAssembly::YourNamespace.Function::FunctionHandler"]
```

### AWS ECS/Fargate

```json
{
  "family": "pdf-service",
  "containerDefinitions": [
    {
      "name": "pdf-service",
      "image": "your-account.dkr.ecr.region.amazonaws.com/pdf-service:latest",
      "memory": 2048,
      "cpu": 1024,
      "essential": true,
      "portMappings": [
        {
          "containerPort": 80,
          "protocol": "tcp"
        }
      ]
    }
  ],
  "requiresCompatibilities": ["FARGATE"],
  "networkMode": "awsvpc",
  "cpu": "1024",
  "memory": "2048"
}
```

---

## .NET MAUI and Blazor

### Blazor Server

```csharp
// Works directly - server-side rendering
@inject IJSRuntime JS

<button @onclick="GeneratePdf">Download PDF</button>

@code {
    private async Task GeneratePdf()
    {
        var html = "<h1>Blazor PDF</h1>";
        var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);

        await JS.InvokeVoidAsync("downloadFile", "document.pdf", pdf.BinaryData);
    }
}
```

### Blazor WebAssembly

Blazor WASM runs in the browser, so PDF generation must happen server-side:

```csharp
// Client
var pdfBytes = await Http.GetByteArrayAsync($"api/pdf/generate?html={encodedHtml}");

// Server API
[HttpGet("generate")]
public IActionResult Generate(string html)
{
    var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
    return File(pdf.BinaryData, "application/pdf");
}
```

### .NET MAUI

For MAUI apps, use IronPDF's remote/gRPC mode:

```csharp
// Configure remote rendering for mobile
IronPdf.Installation.ChromeGpuMode = IronPdf.Engines.Chrome.ChromeGpuModes.Disabled;

var pdf = ChromePdfRenderer.RenderHtmlAsPdf(html);
```

---

## Common Cross-Platform Issues

### Issue 1: Missing Fonts

**Symptom:** Fonts render as boxes or wrong typeface

**Solution:** Install font packages:

```bash
# Ubuntu/Debian
apt-get install -y fonts-liberation fonts-noto-cjk

# Alpine
apk add --no-cache ttf-freefont font-noto-cjk

# Or embed fonts via CSS
<link href="https://fonts.googleapis.com/css2?family=Roboto" rel="stylesheet">
```

### Issue 2: Sandbox Errors in Docker

**Symptom:** `Running as root without --no-sandbox is not supported`

**Solution:** Run container as non-root or disable sandbox:

```dockerfile
RUN groupadd -r appuser && useradd -r -g appuser appuser
USER appuser
```

### Issue 3: Out of Memory

**Symptom:** Container crashes or PDF generation fails

**Solution:** Increase memory limits:

```yaml
# Docker Compose
deploy:
  resources:
    limits:
      memory: 2G
```

### Issue 4: Slow Cold Starts

**Symptom:** First PDF takes 5-10 seconds

**Solution:**
- Keep warm instances in serverless
- Use provisioned concurrency (Lambda)
- Consider always-on containers for high volume

### Issue 5: ARM64 Compatibility

**Symptom:** Binary not found on Apple Silicon or AWS Graviton

**Solution:** IronPDF includes ARM64 binaries. Ensure you're using latest version:

```bash
dotnet add package IronPdf --version latest
```

---

## Conclusion

True cross-platform PDF generation requires:

1. **Native binaries for each platform** — IronPDF bundles these
2. **Dependency management** — Install Chromium deps on Linux
3. **Memory allocation** — Chromium needs 512MB+
4. **Proper containerization** — Use multi-stage builds

IronPDF is designed from the ground up for cross-platform deployment, with equal first-class support for Windows, Linux, macOS, and cloud environments.

---

## Related Tutorials

- **[Best PDF Libraries 2025](best-pdf-libraries-dotnet-2025.md)** — Library comparison
- **[HTML to PDF](html-to-pdf-csharp.md)** — Complete conversion guide
- **[ASP.NET Core](asp-net-core-pdf-reports.md)** — Web deployment
- **[Blazor Guide](blazor-pdf-generation.md)** — Blazor deployment

### Library Deployment Guides
- **[IronPDF Guide](ironpdf/)** — Cross-platform leader
- **[PuppeteerSharp Guide](puppeteersharp/)** — Browser automation
- **[Playwright Guide](playwright/)** — Microsoft automation

### Resources
- **[Azure Deployment Guide](https://ironpdf.com/docs/questions/azure/)** — Detailed Azure setup
- **[Docker Guide](https://ironpdf.com/docs/questions/docker/)** — Container best practices
- **[AWS Guide](https://ironpdf.com/docs/questions/aws/)** — Lambda and ECS deployment

---

*Part of the [Awesome .NET PDF Libraries 2025](README.md) collection — 73 C#/.NET PDF libraries compared.*
