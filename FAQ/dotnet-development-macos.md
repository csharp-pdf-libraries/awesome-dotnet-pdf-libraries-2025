# How Do I Develop .NET Applications on macOS Now That Visual Studio for Mac Is Gone?

Wondering how to set up a modern .NET development environment on a Mac in 2025? You're not alone—since Visual Studio for Mac was discontinued, many developers have switched tools and workflows. The good news: .NET is thriving on macOS, and you can build anything from APIs to cross-platform GUI apps with the right setup. Here’s how to get started, what tools to use, and some practical code tips.

---

## What Happened to Visual Studio for Mac, and What Should I Use Instead?

Visual Studio for Mac was officially discontinued as of August 31, 2024. The recommended alternatives are [VS Code with the C# Dev Kit](https://code.visualstudio.com/docs/languages/dotnet) for a lightweight experience, or [JetBrains Rider](https://www.jetbrains.com/rider/) for a full-featured IDE. Both are well-supported and work smoothly on macOS, making .NET development totally viable.

For more strategies on cross-platform workflows, see [Dotnet Cross Platform Development](dotnet-cross-platform-development.md).

---

## What Are the Best .NET IDEs for macOS in 2025?

You now have several solid editor choices, each with its strengths:

- **VS Code + C# Dev Kit**: Fast, free, and great for quick edits, small projects, or scripting. It lacks some deep refactoring and designer features but excels for everyday coding.
- **JetBrains Rider**: A comprehensive IDE (free for personal/non-commercial use) with advanced refactoring, debugging, hot reload, and a robust NuGet workflow. Great for larger projects and teams.
- **Neovim/Vim + OmniSharp**: If you love terminal-based workflows and customization, this option is for you—but expect a steep setup curve.

Generally, start with VS Code for small/medium projects and graduate to Rider for complex or team-based development. For more comparison, check [VS Code vs Rider: The Real-World Showdown](#vs-code-vs-rider-the-real-world-showdown).

---

## How Do I Install .NET on My Mac the Right Way?

For most developers, using [Homebrew](https://brew.sh/) is the easiest way to install and stay updated:

```bash
brew install dotnet-sdk
```

Alternatively, download installers directly from [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download). To confirm it worked, run:

```bash
dotnet --version
```

If you need to switch between SDK versions, use [dotnet-install scripts](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script).

---

## How Do I Set Up VS Code for .NET Development?

First, install these extensions:
- **C# Dev Kit** (Microsoft) for language support, intellisense, and debugging.
- **.NET Install Tool** and (optionally) NuGet Package Manager for managing dependencies.

To start a new project:

```bash
dotnet new webapi -n MyApi
cd MyApi
code .
```

VS Code will restore packages and provide debugging out of the box. For hot reload, use:

```bash
dotnet watch run
```

If you’re looking to render views or export PDFs from MVC projects, see [Mvc View To Pdf Csharp](mvc-view-to-pdf-csharp.md).

---

## Why Should I Consider JetBrains Rider for .NET on macOS?

[Rider](https://www.jetbrains.com/rider/) is ideal when you need deep code analysis, advanced refactoring, or excellent multi-platform debugging. Open any `.sln` or `.csproj`—it just works, with features like hot reload and built-in terminal. Here’s a quick PDF generator example:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new HtmlToPdf();
        renderer.RenderHtmlAsPdf("<h1>Hello, IronPDF on macOS!</h1>")
                .SaveAs("example.pdf");
    }
}
```

IronPDF ([docs](https://ironpdf.com)) works seamlessly on macOS, and Rider’s NuGet UI makes it easy to add packages.

For a comparison of top PDF libraries, see [2025 Html To Pdf Solutions Dotnet Comparison](2025-html-to-pdf-solutions-dotnet-comparison.md).

---

## How Do I Create, Build, and Cross-Compile .NET Projects on Mac?

Create a console app:

```bash
dotnet new console -n HelloMac
cd HelloMac
dotnet run
```

To build for other platforms:

```bash
dotnet publish -r win-x64 --self-contained
dotnet publish -r linux-x64 --self-contained
dotnet publish -r osx-arm64 --self-contained
```

This produces a native binary for each platform—no .NET runtime needed by your users.

For more on multi-platform app strategies, see [Dotnet Cross Platform Development](dotnet-cross-platform-development.md) and [Avalonia Vs Maui Dotnet 10](avalonia-vs-maui-dotnet-10.md).

---

## Can I Develop ASP.NET Core Applications on macOS?

Absolutely! ASP.NET Core runs natively on macOS. Here’s a minimal API example:

```csharp
// NuGet: Microsoft.AspNetCore.App
using Microsoft.AspNetCore.Builder;

var app = WebApplication.CreateBuilder(args).Build();
app.MapGet("/", () => "Hello from ASP.NET Core on macOS!");
app.Run();
```

To trust HTTPS development certificates:

```bash
dotnet dev-certs https --trust
```

Hot reload is just:

```bash
dotnet watch run
```

---

## How Do I Build and Run .NET Apps in Docker on macOS?

Docker is a great way to standardize builds and deployments. Use a Dockerfile like:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

Build and run:

```bash
docker build -t myapp .
docker run -p 8080:80 myapp
```

Make sure your base images support `arm64` if you're on Apple Silicon.

---

## What Are My Options for Building GUI Apps with .NET on a Mac?

You can write cross-platform GUI apps using .NET MAUI or Avalonia:

- **.NET MAUI**: Targets macOS, Windows, iOS, and Android. Requires Xcode for iOS development.
- **Avalonia**: Open-source, XAML-based, and works across macOS, Windows, and Linux.

To start an Avalonia app:

```bash
dotnet new avalonia.app -n MyAvaloniaApp
cd MyAvaloniaApp
dotnet run
```

For a deeper dive into UI options, check [Avalonia Vs Maui Dotnet 10](avalonia-vs-maui-dotnet-10.md).

---

## Are There Any Common Pitfalls When Developing .NET Apps on macOS?

Yes—watch out for:
- **File paths**: Use `Path.Combine` to avoid separator issues.
- **Line endings**: Configure Git with `git config --global core.autocrlf input`.
- **Case sensitivity**: Be consistent with file and folder naming.
- **SDK in PATH**: Ensure Homebrew’s dotnet-sdk is linked and the path is correct.
- **Docker on Apple Silicon**: Use `arm64` images where possible.

---

## Is .NET Performance Good on Apple Silicon Macs?

Yes! .NET runs natively on Apple Silicon (M1, M2, M3, etc.) and offers excellent build and runtime performance. Libraries like [IronPDF](https://ironpdf.com) and other [Iron Software](https://ironsoftware.com) tools are fully supported.

---

## Can I Still Build Native iOS or macOS Apps with .NET?

For native iOS, you’ll still need Xcode. However, .NET MAUI allows you to share business logic across macOS, iOS, Android, and Windows projects using C#. For more on integrating .NET business logic, see [How do I use a foreach loop with an index in C#?](csharp-foreach-with-index.md).

---

## How Do VS Code and Rider Compare for .NET Development on Mac?

| Feature         | VS Code                   | Rider                      |
|-----------------|--------------------------|----------------------------|
| Price           | Free                     | Free (personal use)        |
| Startup Time    | Very fast                | Slower                     |
| Refactoring     | Good (extensions)        | Excellent                  |
| Debugging       | Good                     | Excellent                  |
| Memory Usage    | Low                      | Higher                     |
| UI Designers    | None                     | Some support               |

Start with VS Code for simplicity; move to Rider as your projects grow.
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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/), CTO of Iron Software, brings 41 years of coding expertise to PDF technology. As creator of IronPDF (10+ million+ downloads), he leads innovation in HTML-to-PDF conversion and document processing. Believes the best APIs don't need a manual. Connect: [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/)
