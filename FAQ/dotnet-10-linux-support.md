# How Do I Run .NET 10 on Linux? (A Practical Developer FAQ)

Running .NET 10 on Linux has never been smoother or more productive. With true LTS, full C# 14 support, and official packages for major distros, Linux is now a first-class .NET development and deployment platform. This FAQ covers installation, cross-platform tips, common pitfalls, and real-world code, so you can hit the ground running.

---

## Why Should I Use .NET 10 on Linux Instead of Windows?

Linux has become a powerhouse for .NET development. With .NET 10, you get long-term support, leading-edge performance, and a modern development experience using C# 14. Linux often matches or outpaces Windows in cloud and container scenarios and makes it easy to deploy microservices, APIs, or even some desktop apps. If you’re curious about the latest C# features, see our [C# 14 tutorial for .NET 10](dotnet-10-csharp-14-tutorial.md).

---

## Which Linux Distros Are Supported for .NET 10?

.NET 10 officially supports several major distributions:

- **Official Microsoft packages:** Ubuntu (22.04, 24.04), Debian (11, 12), RHEL (8.10+), Fedora (39+), openSUSE Leap (15.5+), Alpine Linux (3.18+), CentOS Stream 9.
- **Community-supported (may need manual setup):** Arch, Manjaro, Linux Mint, Pop!_OS.

If your distro isn’t listed, you can almost always use the Snap package or manual install script. For rolling releases like Arch, check the [AUR packages](https://aur.archlinux.org/packages?O=0&K=dotnet-sdk-10.0).

---

## How Do I Install .NET 10 on My Linux Distribution?

Here’s a quick rundown for the main distros. For most, Microsoft’s official repo gives you the most up-to-date packages.

### What’s the Best Way to Install on Ubuntu or Debian?

The recommended way is via Microsoft’s own package feed:

```bash
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```
Or, for a simpler (though sometimes outdated) method:
```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```
After installation, check your version:
```bash
dotnet --version
```

#### Can I Use .NET Libraries Like IronPDF on Linux?

Absolutely! Here’s an example using [IronPDF](https://ironpdf.com):

```csharp
// Install-Package IronPdf (via NuGet)
using IronPdf;

var pdfGen = new HtmlToPdf();
pdfGen.RenderHtmlAsPdf("<h2>Hello from Linux & .NET 10!</h2>")
      .SaveAs("linux-hello.pdf");
Console.WriteLine("PDF created: linux-hello.pdf");
```
If you’re keen on PDF/A compliance, see our [PDF/A in C# guide](pdf-a-compliance-csharp.md).

---

### How Do I Install .NET 10 on Red Hat or Fedora?

For **RHEL** or **Fedora**, use DNF:

```bash
sudo dnf install dotnet-sdk-10.0
```
Updates are quick, and the integration is robust since Microsoft and Red Hat collaborate closely.

#### How Can I Start a Minimal ASP.NET Core API on Linux?

Try this:

```bash
dotnet new webapi -n LinuxApiDemo
cd LinuxApiDemo
dotnet run
```
Access your new API at [http://localhost:5000/weatherforecast](http://localhost:5000/weatherforecast).

---

### What’s the Easiest Way to Install on Alpine Linux?

Alpine is great for containers:

```bash
apk add dotnet10-sdk
```
Or, in Docker:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/YourApp.dll"]
```
Alpine-based images are tiny—ideal for production.

---

### How Do I Install .NET 10 on Arch, Manjaro, Mint, or Pop!_OS?

On **Arch/Manjaro**:  
Use your favorite AUR helper:
```bash
yay -S dotnet-sdk-10.0
```
On **Mint/Pop!_OS**:  
Follow Ubuntu instructions. If needed, enable universe/multiverse repositories.

---

### What If My Distro Isn’t Listed? Can I Use Snap, Manual Scripts, or Docker?

Yes! Universal methods include:

- **Snap:**  
  ```bash
  sudo snap install dotnet-sdk --classic --channel=10.0
  ```
- **Manual script:**  
  ```bash
  wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh
  chmod +x dotnet-install.sh
  ./dotnet-install.sh --channel 10.0
  ```
- **Docker:**  
  Use Microsoft’s official images:
  ```dockerfile
  FROM mcr.microsoft.com/dotnet/sdk:10.0
  ```

These approaches work on almost any Linux system.

---

## What’s New in .NET 10 for Linux Developers?

.NET 10 brings:

- **Long-Term Support:** Maintained through 2028.
- **Performance improvements:** Faster JIT, less memory, especially for containers.
- **Modern C# 14:** Pattern matching, improved records, and more ([see our full C# 14 tutorial](dotnet-10-csharp-14-tutorial.md)).
- **Post-Quantum Cryptography:** New secure algorithms for future-proofing.
- **Direct C# script execution:** Run `.cs` files directly—perfect for quick scripts.

### How Do I Run a C# Script Directly on Linux?

Just write your code and run:
```bash
echo 'System.Console.WriteLine("Hello from C# script!");' > demo.cs
dotnet run demo.cs
```

### Can I Use the New Cryptography Primitives?

Yes, new APIs are supported. Example (API may change):

```csharp
// Install-Package System.Security.Cryptography.PostQuantum
using System.Security.Cryptography.PostQuantum;

var keyPair = MLKEM.GenerateKeyPair();
var encrypted = MLKEM.Encrypt("SecretData", keyPair.PublicKey);
var decrypted = MLKEM.Decrypt(encrypted, keyPair.PrivateKey);
Console.WriteLine($"Decrypted: {decrypted}");
```

---

## How Should I Set Up My Linux Dev Environment for .NET 10?

You’ve got options:

- **VS Code + C# Dev Kit:**  
  ```bash
  sudo snap install code --classic
  ```
  Install the “C# Dev Kit” extension for rich editing and debugging.
- **Rider IDE:**  
  [Rider](https://www.jetbrains.com/rider/) is excellent for large projects.
- **Terminal-only:**  
  The CLI is powerful. Tools like [neovim](https://neovim.io/) pair well with `dotnet new`, `dotnet build`, and `dotnet run`.

For a comparison of .NET UI frameworks, see [Avalonia vs MAUI for .NET 10](avalonia-vs-maui-dotnet-10.md).

---

## How Do I Build and Publish Apps for Windows, Linux, and macOS from Linux?

Cross-platform publishing is easy:

```bash
# Windows x64
dotnet publish -r win-x64 --self-contained -c Release -o out/win

# Linux x64
dotnet publish -r linux-x64 --self-contained -c Release -o out/linux

# macOS ARM64
dotnet publish -r osx-arm64 --self-contained -c Release -o out/mac
```
You get native executables for each OS.

### Can I Use Docker for Consistent Builds?

Yes! Here’s a two-stage Docker build:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -r linux-x64 --self-contained -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./YourAppExecutable"]
```

For building Blazor apps, check out our [Blazor on .NET 10 FAQ](dotnet-10-blazor.md).

---

## Is .NET 10 Faster on Linux or Windows?

On many workloads, Linux matches or even beats Windows.  
- **Web APIs:** Lower overhead and better container density.
- **Networking:** Kestrel leverages Linux-native features.
- **Memory:** Often lower footprint in Linux containers.

### How Do I Benchmark Performance on Linux?

Quick benchmark with [wrk](https://github.com/wg/wrk):

```csharp
// Install-Package Microsoft.AspNetCore.App
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var app = WebApplication.Create();
app.MapGet("/", () => "Hi from Linux!");
app.Run();
```
Start your app, then:
```bash
wrk -t4 -c100 -d30s http://localhost:5000/
```

---

## Can I Build Native Desktop Apps with .NET 10 on Linux?

Linux GUI with .NET is progressing:

- **Avalonia:** Cross-platform and works well now.
- **MAUI:** Linux support is in the works—see our [Avalonia vs MAUI comparison](avalonia-vs-maui-dotnet-10.md).
- **Uno Platform:** Another cross-platform option.

### How Do I Create an Avalonia App?

```bash
dotnet new avalonia.app -n LinuxGuiApp
cd LinuxGuiApp
dotnet run
```
For more insights, see our guide to [cross-platform GUIs in .NET 10](avalonia-vs-maui-dotnet-10.md).

---

## What Are Common Pitfalls When Using .NET 10 on Linux?

### Why Does `dotnet: command not found` Happen?

.NET isn’t installed, or it’s not on your `$PATH`.  
Check:
```bash
ls /usr/share/dotnet/
export PATH=$PATH:/usr/share/dotnet
```
Or for manual installs:
```bash
export PATH=$PATH:$HOME/.dotnet
```

### How Do I Fix SSL Certificate Errors?

Install system certificates:
```bash
# Ubuntu/Debian
sudo apt-get install ca-certificates

# Alpine
apk add ca-certificates
update-ca-certificates
```

### What If I Get Errors About Missing Dependencies?

Some .NET libraries (like [IronPDF](https://ironpdf.com)) require extra Linux libraries, such as ICU:

```bash
# Ubuntu/Debian
sudo apt-get install libicu-dev

# Alpine
apk add icu-libs
```
If you’re seeing odd globalization or rendering errors, this is often the fix.

### How Do I Handle File Permissions on Published Apps?

For published executables:
```bash
chmod +x ./YourAppExecutable
```

### Why Can’t My App Bind to Ports Below 1024?

Only root can do this by default. Either use a higher port (e.g., 8080) or run:
```bash
sudo setcap 'cap_net_bind_service=+ep' ./YourAppExecutable
```

---

## Where Can I Find Quick Reference Commands and Docker Images?

**Install Commands Table:**

| Distro           | Command                                        |
|------------------|------------------------------------------------|
| Ubuntu/Debian    | `sudo apt install dotnet-sdk-10.0`             |
| RHEL/Fedora      | `sudo dnf install dotnet-sdk-10.0`             |
| Alpine           | `apk add dotnet10-sdk`                         |
| Any (Snap)       | `sudo snap install dotnet-sdk --channel=10.0`  |
| Any (Script)     | `./dotnet-install.sh --channel 10.0`           |

**Docker Image Quick Picks:**

| Image                   | Use Case            |
|-------------------------|---------------------|
| sdk:10.0                | Build/test          |
| aspnet:10.0             | Web apps            |
| runtime:10.0            | CLI/console apps    |
| sdk:10.0-alpine         | Smallest build      |

---

## Where Can I Get More Help or Contribute?

The .NET on Linux community is active and growing.  
For .NET document processing, check out [IronPDF](https://ironpdf.com) and [Iron Software](https://ironsoftware.com).  
If you’re interested in common C# pitfalls, don’t miss our [guide on common C# developer mistakes](common-csharp-developer-mistakes.md).

---

*[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) is CTO at Iron Software, where he leads a team of 50+ engineers building .NET document processing tools. He originally created IronPDF.*
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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/) serves as CTO at [Iron Software](https://ironsoftware.com/about-us/authors/jacobmellor/), creator of IronPDF and leader of a globally distributed engineering team. Long-time supporter of NuGet and the .NET community. With 25+ years of commercial development experience, he continues to push the boundaries of .NET PDF technology from Chiang Mai, Thailand.
