# How Do I Install .NET 10 on Windows, macOS, and Linux (and Start Coding Right Away)?

Getting started with .NET 10 is easier than ever—but with new features, multiple platforms, and some common pitfalls, you might have questions. This FAQ covers everything: installation instructions for Windows, macOS, and Linux, how to verify your setup, manage multiple versions, and even create your first .NET 10 app using modern C# features and [IronPDF](https://ironpdf.com). Let's get your environment ready for modern cross-platform .NET development.

---

## Why Should Developers Care About .NET 10?

.NET 10 comes packed with features that matter for both hobbyists and professionals. The biggest highlights include:

- **Long-Term Support (LTS)** through November 2028, so you get security updates and bug fixes for years.
- **C# 14** brings powerful new language features—like enhanced pattern matching, primary constructors, and more.
- **Performance improvements** for faster builds and leaner runtime.
- **Enhanced ASP.NET Core, MAUI, and Blazor**—making both web and cross-platform UI development smoother.
- **Better cloud-native support** for containers and microservices.
- **Widespread library support:** Tools like [IronPDF](https://ironpdf.com) and frameworks from [Iron Software](https://ironsoftware.com) are all-in.

If you’re building apps for desktop, web, or cloud, .NET 10 is an upgrade worth making. Need a deeper dive? See [2025 Html To Pdf Solutions Dotnet Comparison](2025-html-to-pdf-solutions-dotnet-comparison.md) for how .NET 10 stacks up with real-world libraries.

---

## How Do I Install .NET 10 on Windows?

You’ve got several reliable options for installing .NET 10 on Windows. Here’s how to pick the right one for your workflow.

### What’s the Fastest Way to Install .NET 10 on Windows?

The quickest method is using WinGet. Open PowerShell or CMD and enter:

```powershell
winget install Microsoft.DotNet.SDK.10
```

This sets up everything for you—including environment variables—so you can start coding right away. It’s also the easiest way to stay updated.

### Can I Use an Installer Instead?

Absolutely. Download the installer from [the official .NET 10 download page](https://dotnet.microsoft.com/download/dotnet/10.0). Pick the Windows x64 or Arm64 version, run the `.exe`, and follow the prompts. This gives you more control, especially on enterprise machines.

### Does Visual Studio 2026 Include .NET 10?

Yes! If you install [Visual Studio 2026](https://visualstudio.microsoft.com/) and select the ".NET desktop development" or ".NET web development" workloads, the .NET 10 SDK comes with it. No separate install needed.

### How Can I Verify My Installation on Windows?

After installing, confirm everything with:

```powershell
dotnet --version
```

You should see an output like `10.0.100`. If Windows doesn’t recognize the command, check the troubleshooting section below.

---

## How Can I Install .NET 10 on macOS?

Developing on macOS? You have a few convenient options, from Homebrew to manual installers.

### What’s the Recommended macOS Install Method?

If you have Homebrew (and most Mac devs do), just run:

```bash
brew install dotnet-sdk
```

This grabs the latest stable SDK, sets up your PATH, and makes future upgrades a breeze (`brew upgrade dotnet-sdk`).

### Is There an Official macOS Installer?

Yes. Download the `.pkg` installer for either Arm64 (Apple Silicon) or x64 (Intel) from [the official .NET 10 download page](https://dotnet.microsoft.com/download/dotnet/10.0). Run the installer as you would for any other app.

**Tip:** Choose Arm64 for native performance on M1-M4 chips—don’t rely on Rosetta unless you must.

### How Do I Automate .NET 10 Installs on macOS?

If you’re setting up CI, Docker, or want full scriptability, use Microsoft’s install script:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0
```

Afterwards, be sure to add .NET to your PATH:

```bash
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.zshrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.zshrc
source ~/.zshrc
```

### How Do I Check That .NET 10 Is Installed on macOS?

Just run:

```bash
dotnet --version
```

If you see a version like `10.0.100`, you’re all set.

---

## What’s the Process for Installing .NET 10 on Linux?

.NET 10 is well-supported across all major Linux distributions. Here’s how to get set up.

### How Do I Install .NET 10 on Ubuntu or Debian?

Add Microsoft’s package repository and install:

```bash
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0
```

### What About Fedora or RHEL?

Use DNF:

```bash
sudo dnf install dotnet-sdk-10.0
```

### How Can I Install on Alpine Linux?

Alpine users can run:

```bash
apk add dotnet10-sdk
```

### Is There a Linux Method That Works Everywhere?

Yes—use the universal install script, perfect for containers or multi-distro setups:

```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Add to PATH (bash)
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
source ~/.bashrc
```

### Can I Use Snap on Linux?

Definitely. For any Linux flavor:

```bash
sudo snap install dotnet-sdk --classic --channel=10.0
```

### How Do I Confirm .NET 10 Works on Linux?

Run:

```bash
dotnet --version
```

If you see a 10.x version, you’re ready to develop.

---

## How Do I Verify and Explore My .NET 10 Install?

After installing, it’s smart to double-check your environment.

Run:

```bash
dotnet --version
dotnet --info
```

`dotnet --info` gives you details about installed SDKs, runtimes, OS, architecture, and install paths. Example output might include:

```
.NET SDKs installed:
  8.0.404 [C:\Program Files\dotnet\sdk]
  10.0.100 [C:\Program Files\dotnet\sdk]
```

If you see your new version, you’re good to go.

---

## Should I Install the .NET 10 SDK or Just the Runtime?

This trips up a lot of devs. Here’s the difference:

- **SDK:** For anyone writing, building, or debugging .NET code. Includes the runtime and all development tools.
- **Runtime:** For running pre-built .NET applications only. Smaller, but you can’t compile or debug.

**Short answer:** If you plan to code, always get the SDK.

### How Can I Install Just the Runtime?

- **Windows:**  
  ```powershell
  winget install Microsoft.DotNet.Runtime.10
  ```
- **Ubuntu:**  
  ```bash
  sudo apt-get install dotnet-runtime-10.0
  ```
- **macOS:**  
  ```bash
  brew install --cask dotnet
  ```

---

## Can I Have Multiple .NET Versions Installed Side-by-Side?

Yes! .NET supports parallel installations, so you can keep legacy projects running while working with .NET 10.

### How Do I List All Installed .NET Versions?

Use:

```bash
dotnet --list-sdks
dotnet --list-runtimes
```

### How Do I Force a Project to Use a Specific .NET SDK?

Create a `global.json` file in your project or repo root:

```json
{
  "sdk": {
    "version": "10.0.100"
  }
}
```

With this, your build tools will always use .NET 10 for that project—great for teams or CI/CD. For more on targeting frameworks, see [How To Develop Aspnet Applications Dotnet 10](how-to-develop-aspnet-applications-dotnet-10.md).

### How Do I Target a Specific .NET Framework in My Project?

In your `.csproj` file:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

You can target older frameworks (like `net8.0`) in the same environment if needed.

---

## How Do I Upgrade or Uninstall .NET 10?

### What’s the Upgrade Path?

- **Windows:**  
  ```powershell
  winget upgrade Microsoft.DotNet.SDK.10
  ```
- **macOS:**  
  ```bash
  brew upgrade dotnet-sdk
  ```
- **Linux (Ubuntu):**  
  ```bash
  sudo apt-get update && sudo apt-get upgrade dotnet-sdk-10.0
  ```
- **Fedora:**  
  ```bash
  sudo dnf update dotnet-sdk-10.0
  ```

### How Can I Uninstall .NET 10?

- **Windows:** Use "Add/Remove Programs" or:
  ```powershell
  winget uninstall Microsoft.DotNet.SDK.10
  ```
- **macOS:**
  ```bash
  sudo rm -rf /usr/local/share/dotnet
  ```
- **Linux (Ubuntu):**
  ```bash
  sudo apt-get remove dotnet-sdk-10.0
  ```
  **Fedora:**
  ```bash
  sudo dnf remove dotnet-sdk-10.0
  ```

---

## Which IDE Should I Use for .NET 10 Development?

.NET 10 is compatible with all major editors. Here’s what’s popular:

| IDE                  | Platform  | Highlights                       |
|----------------------|-----------|-----------------------------------|
| Visual Studio 2026   | Windows   | Feature-rich, best for enterprise |
| VS Code + C# Dev Kit | All       | Lightweight, cross-platform       |
| JetBrains Rider      | All       | Powerful, cross-platform          |

- **VS Code** (with [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)): A great cross-platform, lightweight IDE. It’ll even prompt you to install missing SDKs.
- **JetBrains Rider:** For those who love JetBrains workflows.
- **Visual Studio:** The best option for Windows desktop and enterprise work, especially for WinForms or WPF.

For more on modern UI stacks, check out [Avalonia Vs Maui Dotnet 10](avalonia-vs-maui-dotnet-10.md).

---

## How Do I Create My First .NET 10 Project?

Let’s do a fast Hello World console app and a minimal Web API—both showing off .NET 10/C# 14 features.

### How Do I Write a Modern Console App in .NET 10?

Create a new project:

```bash
dotnet new console -n HelloWorld10
cd HelloWorld10
```

Replace `Program.cs` with:

```csharp
using System;

// C# 14: Primary constructors and pattern matching
public class Greeter(string name)
{
    public void Greet() => Console.WriteLine($"Hello, {name}!");
}

class Program
{
    static void Main(string[] args)
    {
        var who = args.Length > 0 ? args[0] : "Developer";
        var greeter = new Greeter(who);
        greeter.Greet();
    }
}
```

Run with:

```bash
dotnet run
dotnet run -- "Jacob"
```

For a beginner-friendly walkthrough, see [Html To Pdf Csharp Beginners](html-to-pdf-csharp-beginners.md).

### How Do I Create a Minimal Web API in .NET 10?

Start a new API project:

```bash
dotnet new webapi -n MyApi10
cd MyApi10
```

Replace `Program.cs` with:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var app = WebApplication.Create(args);

app.MapGet("/", () => Results.Ok("Hello from .NET 10 Minimal API!"));
app.MapGet("/greet/{name}", (string name) => Results.Ok($"Hello, {name}!"));

app.Run();
```

Run your API:

```bash
dotnet run
```

Navigate to [http://localhost:5000/greet/Hashnode](http://localhost:5000/greet/Hashnode) to see it in action.

---

## How Do I Work with PDFs in .NET 10 Using IronPDF?

[IronPDF](https://ironpdf.com) makes PDF generation and manipulation simple in .NET 10, with support across Windows, macOS, and Linux.

### Can You Show a Simple Example of HTML to PDF in .NET 10?

First, install IronPDF from NuGet:

```bash
dotnet add package IronPdf
```

Then use this example:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        string html = "<h1>Hello, .NET 10!</h1><p>PDF generated using IronPDF.</p>";

        var pdfRenderer = new ChromePdfRenderer();
        using var pdfDoc = pdfRenderer.RenderHtmlAsPdf(html);

        var savePath = "hello-dotnet10.pdf";
        pdfDoc.SaveAs(savePath);

        Console.WriteLine($"PDF saved at {savePath}");
    }
}
```

IronPDF offers a simple API, robust cross-platform support, and is part of a suite of document tools from [Iron Software](https://ironsoftware.com). For more .NET/C# PDF options, compare solutions at [2025 Html To Pdf Solutions Dotnet Comparison](2025-html-to-pdf-solutions-dotnet-comparison.md).

---

## What Are Common .NET 10 Installation Problems and Solutions?

Almost everyone hits a snag eventually. Here are the top issues and fixes:

### Why Does Windows Say "dotnet is not recognized"?

Your PATH variable doesn’t include the .NET install folder.

- **Windows:**  
  ```powershell
  $env:PATH += ";C:\Program Files\dotnet"
  ```
- **macOS/Linux:**  
  ```bash
  export PATH=$PATH:/usr/local/share/dotnet
  ```

### Why Is My Project Using the Wrong .NET Version?

Check for a `global.json` in your project—it might be forcing an old SDK. Update or remove it as needed. Use `dotnet --list-sdks` to verify what’s installed.

### What Do I Do About "Permission Denied" Errors on Linux/macOS?

Don’t use `sudo` unless you’re installing system-wide. For user installs, make sure your user owns the `.dotnet` directory.

### My IDE Doesn’t Detect .NET 10—What’s Up?

Restart your IDE or terminal, especially after modifying your PATH. In VS Code, try “.NET: Select SDK Version” from the command palette.

### Why Do I See Build Errors After Upgrading?

Some NuGet packages may not support .NET 10 yet. Check for package updates, or temporarily target `net8.0` in your `.csproj` if needed. For more tips, see [Csharp Foreach With Index](csharp-foreach-with-index.md) for iterating with new C# features.

---

## What’s a Quick Reference for .NET 10 Installation Commands?

Here’s a handy summary:

| Platform | Recommended Method | Command |
|----------|-------------------|---------|
| **Windows** | WinGet | `winget install Microsoft.DotNet.SDK.10` |
| **macOS**   | Homebrew | `brew install dotnet-sdk` |
| **Ubuntu**  | apt | `sudo apt install dotnet-sdk-10.0` |
| **Fedora**  | dnf | `sudo dnf install dotnet-sdk-10.0` |
| **Any**     | Script | `./dotnet-install.sh --channel 10.0` |
| **Any**     | Snap | `sudo snap install dotnet-sdk --classic --channel=10.0` |
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

**Jacob Mellor** — Creator of IronPDF, CTO at Iron Software. 41 years coding, 50++ person team, 41+ million NuGet downloads. Believes the best APIs don't need a manual. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
