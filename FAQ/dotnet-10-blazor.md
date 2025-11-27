# What’s New in .NET 10 Blazor and How Can Developers Take Advantage?

.NET 10 brings major upgrades to Blazor, including dramatically smaller bundle sizes, easy persistent state, passwordless authentication, better form validation, and much more. If you’re building modern C# web apps, these features can save you time and deliver a better user experience. Here’s a practical FAQ covering the most important changes and how to use them today.

---

## How Much Smaller Are Blazor Bundle Sizes in .NET 10?

Blazor’s core JavaScript bundle is now just 43 KB (down from 183 KB)—a 76% reduction. This is automatic: once you upgrade, users get faster page loads and improved caching right away. No config or code tweaks needed.

If you’re building document-heavy apps (e.g., PDFs, reports), this smaller bundle pairs especially well with [IronPDF](https://ironpdf.com), letting your UI load quickly while complex processing happens server-side. For other ecosystem updates, see [Dotnet 10 Linux Support](dotnet-10-linux-support.md).

---

## How Does Persistent State Work in Blazor Server?

.NET 10 Blazor Server can now persist state across disconnects or server restarts—no more losing a user’s session due to flaky WiFi.

Just decorate your stateful property with `[PersistentState]`:

```csharp
// NuGet: Microsoft.AspNetCore.Components.Server
using Microsoft.AspNetCore.Components;

[PersistentState]
public string Theme { get; set; } = "Light";
```

This attribute tells Blazor to automatically save and restore that property after reconnects. Complex objects work too—just mark them with `[PersistentState]`. 

### Can I Control State Persistence from JavaScript?

Yes! .NET 10 lets you pause and resume the Blazor “circuit” from JavaScript, handy for advanced scenarios:

```javascript
// Pause Blazor
Blazor.pause();
// Resume Blazor
Blazor.resume();
```

This is useful for debugging or freezing the UI during critical actions.

---

## How Do I Use Passkey (Passwordless) Authentication in Blazor?

.NET 10 supports FIDO2/WebAuthn passkeys out of the box, making it easy to enable fingerprint, Face ID, or hardware key logins.

In your setup, add passkey support to Identity:

```csharp
// NuGet: Microsoft.AspNetCore.Identity
using Microsoft.AspNetCore.Identity;

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddPasskeys();
```

Then, create a UI that triggers passkey sign-in:

```csharp
private async Task SignInWithPasskey()
{
    var result = await AuthenticationService.RequestPasskeySignInAsync();
    // Handle result
}
```

Remember: Passkey authentication requires HTTPS and up-to-date Identity setup. For more on secure authentication, check out [Dotnet 10 Csharp 14 Tutorial](dotnet-10-csharp-14-tutorial.md).

---

## How Has Form Validation Improved for Nested Models?

Blazor now directly supports complex and nested validation, so you don’t need messy custom logic for deeply structured forms.

In `Program.cs`, register types for validation:

```csharp
// NuGet: Microsoft.Extensions.DependencyInjection
builder.Services.AddValidatableTypes();
```

Then use `[ValidatableType]` and `[ValidateComplexType]` in your models:

```csharp
[ValidatableType]
public class Order {
    [Required] public string CustomerName { get; set; }
    [ValidateComplexType] public Address ShippingAddress { get; set; } = new();
}
```

Now, Blazor validates every nested property automatically. This is great for real-world forms with complex data.

---

## How Do I Handle 404 Pages in .NET 10 Blazor?

Handling “not found” routes is now a single line:

```csharp
NavigationManager.NotFound();
```

You can also fully customize your 404 page by editing `NotFound.razor`:

```razor
@page "/not-found"
<h1>404: Page Not Found</h1>
```

Centralizing your 404 UI keeps things clean and consistent.

---

## What’s New with JavaScript Interop in Blazor?

No more boilerplate JS wrappers: .NET 10 lets you get/set JavaScript properties directly from C#.

```csharp
// NuGet: Microsoft.JSInterop
using Microsoft.JSInterop;

await JS.SetValueAsync("document.title", "New Title");
string currentTitle = await JS.GetValueAsync<string>("document.title");
```

You can also create and manage JS object references simply:

```csharp
var jsObj = await JS.InvokeAsync<IJSObjectReference>("SomeFactory.create");
await jsObj.DisposeAsync();
```

For advanced integration patterns, see [Iron Software’s developer blog](https://ironsoftware.com).

---

## What Are the Major QuickGrid Improvements in .NET 10?

Blazor’s open-source grid now supports:

- **Conditional row styling:** Apply CSS classes based on row data.
- **Programmatic column control:** Hide/show columns directly from code.

Example:

```razor
<QuickGrid Items="@accounts" RowClass="GetRowClass">
    <PropertyColumn Property="@(a => a.Name)" Title="Name" />
</QuickGrid>
@code {
    private string GetRowClass(Account a) => a.Balance < 0 ? "negative" : "";
}
```

For more about document tables and PDF grids, see [Pdf Headers Footers Csharp](pdf-headers-footers-csharp.md).

---

## How Has NavLink Active State Handling Improved?

NavLink now correctly matches routes even when there are query strings or fragments:

```razor
<NavLink href="/dashboard" Match="NavLinkMatch.All">Dashboard</NavLink>
```

This means `/dashboard`, `/dashboard?tab=x`, and `/dashboard#section` all set the NavLink as active—solving a long-standing annoyance.

---

## What’s New in Blazor’s Reconnection and Preloading Features?

The new `ReconnectModal` component is customizable and CSP-friendly—no inline styles or scripts required. You can easily style or extend it for better UX during connection hiccups.

Blazor WebAssembly also adds improved preloading support for faster startup:

```html
<link rel="preload" href="_framework/blazor.webassembly.js" as="script" />
```

---

## How Do I Upgrade My Project to .NET 10 Blazor?

1. **Update the .csproj file:**  
   ```xml
   <TargetFramework>net10.0</TargetFramework>
   ```
2. **Install the .NET 10 SDK:**  
   ```bash
   dotnet --version
   ```
3. **Restore and build:**  
   ```bash
   dotnet restore
   dotnet build
   ```
4. **Test your app, especially if using JS interop or third-party libraries.**  
5. **Update dependencies:** Tools like [IronPDF](https://ironpdf.com) or grids may need new versions.

For cross-platform upgrade tips, see [Avalonia Vs Maui Dotnet 10](avalonia-vs-maui-dotnet-10.md).

---

## What Pitfalls Should I Watch Out For When Upgrading?

- **Build errors?** Update all NuGet dependencies.
- **Persistent state not restoring?** Ensure `[PersistentState]` is on all relevant properties.
- **Passkey sign-in not working?** Use HTTPS and double-check `.AddPasskeys()`.
- **Custom 404 not showing?** Review your `NotFound.razor` route and logic.
- **JS interop fails?** Validate property paths and JS runtime availability.

For font management in PDFs, see [Manage Fonts Pdf Csharp](manage-fonts-pdf-csharp.md).

---

## Where Can I Learn More About .NET 10, C# 14, and Blazor?

For deeper tutorials and ecosystem insights, see [Dotnet 10 Csharp 14 Tutorial](dotnet-10-csharp-14-tutorial.md), or visit [Iron Software](https://ironsoftware.com) and [IronPDF](https://ironpdf.com).

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

**Jacob Mellor** — Creator of IronPDF, CTO at Iron Software. 41 years coding, 50++ person team, 41+ million NuGet downloads. Civil Engineering degree holder turned software pioneer. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
