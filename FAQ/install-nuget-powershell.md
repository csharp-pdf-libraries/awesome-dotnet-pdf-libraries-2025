# How Can I Manage NuGet Packages Using PowerShell for .NET Projects?

If you're tired of wrestling with NuGet inside Visual Studio, switching between GUIs, CLI, and mysterious scripts, you're not alone. Managing NuGet via PowerShell unlocks scriptable, reproducible, and cross-platform workflows—crucial for modern .NET development, CI/CD pipelines, and Docker environments. This FAQ walks you through the essentials, troubleshooting tips, and practical scripts to streamline your .NET package management—no IDE required.

## Why Should I Use PowerShell to Manage NuGet Packages?

PowerShell lets you manage NuGet packages from any environment—workstations, build servers, Docker containers, or even Linux/macOS. This approach is highly scriptable, making installs, upgrades, and rollbacks fully automatable and reliable—critical for CI, deployment, and reproducible builds. If you're moving to .NET 6 or later, make sure your environment is set up (see [How To Install Dotnet 10](how-to-install-dotnet-10.md)).

## What’s the Difference Between the NuGet Provider and the NuGet Module in PowerShell?

The distinction can be confusing at first:
- **NuGet Provider**: This is the PowerShell "engine" that enables communication with NuGet feeds (like NuGet.org or private sources).
- **NuGet Module**: This adds PowerShell commands such as `Install-Package` and `Update-Package` for actual package management.

You'll need both: the provider to connect, the module to control packages.

## How Do I Set Up NuGet in PowerShell?

Getting started requires two steps—installing both the provider and the module.

### How Do I Install the NuGet Provider?

Run this in your PowerShell session to add the NuGet provider system-wide:

```powershell
Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force
```
If you lack admin rights, install for just the current user:
```powershell
Install-PackageProvider -Name NuGet -Scope CurrentUser
```

### How Do I Install the NuGet PowerShell Module?

Next, add the NuGet module:

```powershell
Install-Module -Name NuGet -Force
```
If prompted about trusting the PowerShell Gallery, confirm with `Y`. Verify installation:
```powershell
Get-Module -ListAvailable -Name NuGet
```

## How Do I Install, Update, or Remove NuGet Packages from PowerShell?

PowerShell makes it easy to manage dependencies, even for advanced scenarios.

### How Can I Install Packages by Name or Version?

To install a package (e.g., IronPdf):

```powershell
Install-Package IronPdf
```
For a specific version:
```powershell
Install-Package IronPdf -RequiredVersion 2024.1.6
```
Check installed packages:
```powershell
Get-Package IronPdf
```
Pinning versions helps you create reproducible builds (see [2025 Html To Pdf Solutions Dotnet Comparison](2025-html-to-pdf-solutions-dotnet-comparison.md) for why this is important).

### How Do I Update or Uninstall NuGet Packages?

To update to the latest version:
```powershell
Update-Package IronPdf
```
To remove a package:
```powershell
Uninstall-Package IronPdf
```
Update all packages (with caution):
```powershell
Update-Package
```
You can script common tasks for popular libraries such as Newtonsoft.Json:
```powershell
Install-Package Newtonsoft.Json -RequiredVersion 13.0.3
Update-Package Newtonsoft.Json
Uninstall-Package Newtonsoft.Json
```
For related tasks like accessing or manipulating PDF files, see [Access Pdf Dom Object Csharp](access-pdf-dom-object-csharp.md) and [Add Copy Delete Pdf Pages Csharp](add-copy-delete-pdf-pages-csharp.md).

## How Can I Work With Custom Package Sources or Private Feeds?

You’re not limited to NuGet.org—PowerShell supports private/internal feeds.

- **List sources**:
    ```powershell
    Get-PackageSource
    ```
- **Add a new source**:
    ```powershell
    Register-PackageSource -Name MyPrivateFeed -Location https://myfeed.example.com/nuget -ProviderName NuGet
    ```
- **Remove a source**:
    ```powershell
    Unregister-PackageSource -Name MyPrivateFeed
    ```
- **Install from a specific source**:
    ```powershell
    Install-Package MySecretLibrary -Source MyPrivateFeed
    ```

## How Do I Use NuGet in CI/CD Pipelines or Build Scripts?

PowerShell scripts make automation simple for servers, Docker, or cloud builds.

```powershell
Install-PackageProvider -Name NuGet -Force
Install-Module -Name NuGet -Force
Install-Package IronPdf -Source https://api.nuget.org/v3/index.json
Install-Package Newtonsoft.Json
dotnet restore ./MyProject.sln
dotnet build ./MyProject.csproj -c Release
dotnet pack ./MyProject.csproj -o ./artifacts
```
This guarantees every environment gets the exact dependencies, a must-have for reliable builds. For converting web pages or ASPX to PDF in CI, see [Convert Aspx To Pdf Csharp](convert-aspx-to-pdf-csharp.md).

## Can I Use PowerShell to Generate PDFs in .NET With IronPDF?

Absolutely! After installing IronPDF via PowerShell, you can create PDFs from HTML in your C# code:

```powershell
Install-Package IronPdf
```
```csharp
using IronPdf; // NuGet: Install-Package IronPdf

var pdfRenderer = new ChromePdfRenderer();
string html = "<h1>Hello, PDF from PowerShell!</h1>";
var doc = pdfRenderer.RenderHtmlAsPdf(html);
doc.SaveAs("output.pdf");
```
For a full walkthrough, check out this [PDF generation video](https://ironpdf.com/blog/videos/how-to-generate-pdf-files-in-dotnet-core-using-ironpdf/).

## How Can I Install NuGet Packages Offline or in Air-Gapped Environments?

First, download packages on a connected machine:
```powershell
Save-Package IronPdf -Path C:\NuGetPackages
```
Copy that folder to your offline server, then install locally:
```powershell
Install-Package IronPdf -Source C:\NuGetPackages
```
This approach is invaluable for secure or disconnected deployments.

## Does NuGet Work the Same in PowerShell Core and Windows PowerShell?

Mostly, yes! PowerShell Core (7+) is cross-platform, so you can use these commands on Windows, Linux, or macOS. Just use `pwsh` instead of `powershell` on non-Windows systems. Very old modules may not be supported, but NuGet and IronPDF work fine.

## What Are Common NuGet PowerShell Issues and How Do I Fix Them?

- **Dependency errors**: Install modules with `-AllowClobber -Force -Scope CurrentUser`.
- **TLS errors on older Windows**: Force TLS 1.2:
    ```powershell
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    ```
- **Permission problems**: Use `-Scope CurrentUser` when needed.
- **Module not found**: Try `Import-Module NuGet`.
- **Source problems**: Verify feeds with `Get-PackageSource` and ensure URLs are correct.

If you need to manipulate PDF files after installation, see [Access Pdf Dom Object Csharp](access-pdf-dom-object-csharp.md).

## What Are the Most Useful PowerShell NuGet Commands?

Here's a quick reference:

| Task                      | Command                                          |
|---------------------------|--------------------------------------------------|
| Install provider          | `Install-PackageProvider -Name NuGet -Force`     |
| Install module            | `Install-Module -Name NuGet -Force`              |
| Install package           | `Install-Package PackageName`                    |
| Install specific version  | `Install-Package PackageName -RequiredVersion X` |
| Update package            | `Update-Package PackageName`                     |
| Uninstall package         | `Uninstall-Package PackageName`                  |
| List installed            | `Get-Package`                                    |
| Add/remove source         | `Register/Unregister-PackageSource ...`           |
| Save for offline          | `Save-Package PackageName -Path Folder`          |

For more insight, see [2025 Html To Pdf Solutions Dotnet Comparison](2025-html-to-pdf-solutions-dotnet-comparison.md).

---

*Questions? [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), CTO at Iron Software, is always happy to help. Drop a comment below or check out [IronPDF](https://ironpdf.com) for more tutorials.*
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
