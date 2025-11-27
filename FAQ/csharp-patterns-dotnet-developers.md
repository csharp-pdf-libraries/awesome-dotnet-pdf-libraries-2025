# What Modern C# Patterns Should Every .NET Developer Really Use?

Wondering which C# coding patterns are actually worth mastering in real-world .NET projects? Over the years building large-scale libraries like [IronPDF](https://ironpdf.com), I’ve distilled a list of practical, modern C# and .NET patterns that consistently make code clearer, safer, and easier to maintain. Let’s break down the patterns you’ll use most—and how to apply them effectively.

---

## How Can I Use Pattern Matching to Make My C# Code More Readable?

Pattern matching is a powerful way to express complex logic without tangled if-else blocks or verbose switch statements. It lets you match on types, properties, and even collection shapes, making intent crystal clear.

```csharp
using System;

public record Customer(bool IsVip, int YearsActive, decimal Spend);

public static decimal GetDiscount(Customer customer) =>
    customer switch
    {
        { IsVip: true, YearsActive: > 5, Spend: > 10000m } => 0.35m,
        { IsVip: true, YearsActive: > 5 } => 0.25m,
        { IsVip: true } => 0.15m,
        { YearsActive: > 10 } => 0.10m,
        _ => 0m
    };
```

### Can Pattern Matching Handle Types, Properties, and Collections?

Absolutely! It’s handy for property checks, type checks, and even for analyzing array shapes.

```csharp
object val = "Hello, World!";
if (val is string str && str.Length > 5)
    Console.WriteLine($"Long string: {str}");

var person = new { Age = 25, Country = "US" };
if (person is { Age: >= 18, Country: "US" })
    Console.WriteLine("US adult");

int[] numbers = { 1, 2, 3, 4 };
if (numbers is [1, 2, .. var rest])
    Console.WriteLine($"Rest: {string.Join(",", rest)}");
```

Pattern matching is especially useful when working with business rules or data transformations. For more pattern matching examples, see our [Dotnet 10 Csharp 14 Tutorial](dotnet-10-csharp-14-tutorial.md).

---

## When Should I Use C# Records Instead of Classes?

Use records when you want immutable, value-based data types—like DTOs, API models, or configuration snapshots. Records provide value equality and easy non-destructive mutation.

```csharp
public record Invoice(string Number, DateTime Created, decimal Amount);

var inv1 = new Invoice("INV-001", DateTime.UtcNow, 100m);
var inv2 = inv1 with { Amount = 120m };

Console.WriteLine(inv1 == inv2); // False
```

For entities with identity (like ORM models) or objects with lots of behavior, stick with classes. Records work beautifully with pattern matching and deconstruction.

---

## What Are Init-Only Properties and Why Should I Use Them?

Init-only properties enable properties to be assigned during initialization but stay immutable afterward, keeping your objects safe from unwanted changes.

```csharp
public class UserProfile
{
    public string Username { get; init; }
    public DateTime Registered { get; init; }
    public bool IsAdmin { get; set; }
}

var user = new UserProfile
{
    Username = "devhero",
    Registered = DateTime.UtcNow,
    IsAdmin = false
};
user.IsAdmin = true; // OK
// user.Username = "hacker"; // Compile error
```

This is perfect for IDs, timestamps, or anything that should only be set once.

---

## How Does the Null Object Pattern Help Me Avoid Null Checks?

The null object pattern replaces null references with a harmless default implementation, preventing repeated null checks and making your code safer.

```csharp
public interface INotifier { void Notify(string msg); }

public class NullNotifier : INotifier { public void Notify(string msg) { } }

public class NotificationService
{
    private readonly INotifier _notifier;
    public NotificationService(INotifier? notifier = null)
        => _notifier = notifier ?? new NullNotifier();
    public void Alert(string msg) => _notifier.Notify(msg);
}
```

This keeps your code free from `if (notifier != null)` clutter.

---

## How Do I Use the Options Pattern for Typed Configuration?

The Options pattern is a best practice for injecting settings into your services, especially in ASP.NET Core apps.

```csharp
public class PdfSettings
{
    public string DefaultFont { get; set; }
    public bool EnableCompression { get; set; }
}

// Register in Program.cs
builder.Services.Configure<PdfSettings>(
    builder.Configuration.GetSection("PdfSettings"));

// Inject into your service
public class PdfService
{
    private readonly PdfSettings _settings;
    public PdfService(Microsoft.Extensions.Options.IOptions<PdfSettings> options)
        => _settings = options.Value;
}
```

This approach gives you strong typing and testability. When working with PDF generation, see our [Dotnet Core Pdf Generation Csharp FAQ](dotnet-core-pdf-generation-csharp.md) for more on customizing PDF output.

---

## What Is the Result Pattern and How Does It Improve Error Handling?

The result pattern lets you signal success or failure explicitly, eliminating the need for exceptions-as-flow-control.

```csharp
public class Result<T>
{
    public bool Success { get; }
    public T? Value { get; }
    public string? Error { get; }
    private Result(T value) { Success = true; Value = value; }
    private Result(string error) { Success = false; Error = error; }
    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(string error) => new(error);
}
```

Now, instead of try/catch everywhere, you return a result object and handle errors at the call site cleanly.

---

## When Should I Use the Specification Pattern?

The specification pattern encapsulates complex queries or business rules, keeping them DRY and reusable.

```csharp
public interface ISpec<T> { Expression<Func<T, bool>> ToExpression(); }

public class ActiveUserSpec : ISpec<User>
{
    public Expression<Func<User, bool>> ToExpression()
        => u => u.IsActive && !u.IsBanned;
}

var activeUsers = db.Users.Where(new ActiveUserSpec().ToExpression());
```

This is especially handy for filtering in ORMs like Entity Framework, and prevents logic duplication.

---

## How Do Primary Constructors Make Dependency Injection Simpler in C# 12+?

Primary constructors in C# 12 reduce boilerplate for DI-heavy classes by letting you declare dependencies right in the class header.

```csharp
public class PdfGenerator(IPdfRenderer renderer, ILogger<PdfGenerator> logger)
{
    public void Generate(string html)
    {
        renderer.Render(html);
        logger.LogInformation("PDF created.");
    }
}
```

Check out our [Dotnet 10 Csharp 14 Tutorial](dotnet-10-csharp-14-tutorial.md) for more on C# 12’s new syntax.

---

## What Are Collection Expressions and How Do They Simplify Code?

C# 12 lets you build arrays, lists, and spans with bracket syntax. No more verbose `new List<T> {...}`.

```csharp
int[] nums = [1, 2, 3];
List<string> names = ["Alice", "Bob"];
int[] joined = [..nums, 4, 5];
```

This makes your code more concise and readable, especially for test data and configs.

---

## How Should I Implement the Dispose Pattern for Resource Cleanup?

Implementing `IDisposable` ensures resources like files and streams are released properly.

```csharp
using System;
using System.IO;

public class FileReader : IDisposable
{
    private readonly StreamReader _reader;
    private bool _disposed;
    public FileReader(string path) => _reader = new StreamReader(path);

    public string ReadLine()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(FileReader));
        return _reader.ReadLine();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _reader.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
```

Always use `using` or `using var` for disposable resources. For document viewing and cleanup, see [Dotnet Pdf Viewer Csharp](dotnet-pdf-viewer-csharp.md).

---

## Why Use Async Dispose and When Is It Needed?

If your class manages async resources, implement `IAsyncDisposable` for proper async cleanup.

```csharp
using System.Net.Http;
public class Downloader : IAsyncDisposable
{
    private readonly HttpClient _client = new();
    public async Task<string> DownloadAsync(string url) =>
        await _client.GetStringAsync(url);

    public async ValueTask DisposeAsync()
    {
        await Task.Delay(10); // Simulate async cleanup
        _client.Dispose();
    }
}
```

This is crucial for network streams, file writers, or anything that flushes data asynchronously.

---

## What’s the Factory Pattern and Why Should I Use It?

The factory pattern lets you create objects by hiding their concrete types—great for extensibility and plugin architectures.

```csharp
public enum ExportType { Pdf, Csv }
public interface IExporter { void Export(string data, string path); }
public class PdfExporter : IExporter { public void Export(string d, string p) => Console.WriteLine("PDF!"); }
public class CsvExporter : IExporter { public void Export(string d, string p) => Console.WriteLine("CSV!"); }

public class ExportFactory
{
    public IExporter GetExporter(ExportType type) => type switch
    {
        ExportType.Pdf => new PdfExporter(),
        ExportType.Csv => new CsvExporter(),
        _ => throw new ArgumentException()
    };
}
```

Need robust PDF exporting? Check out [IronPDF](https://ironpdf.com). If you’re migrating, see [Upgrade Dinktopdf To Ironpdf](upgrade-dinktopdf-to-ironpdf.md).

---

## How Do Extension Methods Help Me Write Cleaner C# Code?

Extension methods let you add “extra” methods to existing types—perfect for utilities or LINQ-style helpers.

```csharp
public static class StringHelpers
{
    public static bool IsBlank(this string? str) => string.IsNullOrWhiteSpace(str);
    public static string Truncate(this string str, int max) =>
        str.Length <= max ? str : str[..max] + "...";
}
```

Use them for discoverable helpers and keeping code DRY. For exporting data with extension methods, see [Export Save Pdf Csharp](export-save-pdf-csharp.md).

---

## What Common C# Pitfalls Should I Watch Out For?

- **Overusing inheritance:** Favor composition over complex base classes.
- **Catching `Exception`:** Be specific in your error handling.
- **Using `async void`:** Only for event handlers, never for business logic.
- **Ignoring `CancellationToken`:** Always pass it for async operations.
- **Blocking on async code:** Avoid `.Result` or `.Wait()`; always `await`.
- **Not disposing resources:** Use `using` or call `Dispose()` when needed.
- **Version mismatches:** Some features require C# 12+. Check your project’s settings.

For more troubleshooting tips, see our [Dotnet 10 Csharp 14 Tutorial](dotnet-10-csharp-14-tutorial.md).

---

## Where Can I Learn More About Modern C# Patterns and Features?

Modern C# (9–12+) gives developers a rich set of features—pattern matching, records, primary constructors, and more—that make code more expressive and reliable. Explore the [IronPDF documentation](https://ironpdf.com) and [Iron Software](https://ironsoftware.com) blogs for deep dives and real-world samples.

---

*About the author: [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) founded IronPDF and now serves as CTO at [Iron Software](https://ironsoftware.com), where he oversees the Iron Suite of .NET libraries.*
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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/) serves as CTO at [Iron Software](https://ironsoftware.com/about-us/authors/jacobmellor/), creator of IronPDF and leader of a globally distributed engineering team. Engineering fundamentals meet cutting-edge software development. With 25+ years of commercial development experience, he continues to push the boundaries of .NET PDF technology from Chiang Mai, Thailand.
