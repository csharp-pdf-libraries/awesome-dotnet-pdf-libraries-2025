# What Are the Most Practical New Features in C# 14 and How Do I Use Them?

C# 14 is here, launching alongside .NET 10, and it brings a wave of genuinely useful features that make daily development cleaner, faster, and more expressive. Whether you’re keen on collection expressions, primary constructors, advanced pattern matching, or in need of performance improvements with inline arrays, C# 14 has something for you. In this FAQ, I’ll break down the most impactful features, show where they shine (and where they bite), and provide real-world code examples you can copy and adapt right away. If you’re curious about what’s new, how to use it, and what to watch out for, you’re in the right place.

For an overview of the broader platform, see [What’s new in .NET 10?](whats-new-in-dotnet-10.md) and for a guided walk-through, check the [C# 14/.NET 10 tutorial](dotnet-10-csharp-14-tutorial.md).

---

## How Do Unified Collection Expressions Simplify Collection Initialization in C# 14?

C# 14 introduces a universal, bracket-based syntax for initializing collections, which reduces boilerplate and makes your code more readable and maintainable. Whether you’re working with arrays, `List<T>`, immutable collections, or even your own types, you get a single, intuitive syntax.

### How Does the New Syntax Compare to the Old Way of Creating Collections?

Previously, initializing collections meant choosing the right constructor or static method for each type, leading to repetitive code. Here’s how things looked before and after:

#### Before C# 14

```csharp
// Install-Package IronPdf
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

var arr = new int[] { 1, 2, 3 };
var list = new List<int> { 1, 2, 3 };
var span = new Span<int>(new int[] { 1, 2, 3 });
var immutable = ImmutableArray.Create(1, 2, 3);
```

#### With C# 14 Collection Expressions

```csharp
// Install-Package IronPdf
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

var arr = [1, 2, 3];                        // int[]
List<int> list = [1, 2, 3];                 // List<int>
ImmutableArray<int> immutable = [1, 2, 3];  // ImmutableArray<int>
Span<int> span = [1, 2, 3];                 // Span<int>
```

The compiler figures out what you want based on context, so refactoring between collection types is much less painful.

### Can I Use This New Syntax with Custom Collection Types?

Yes! If your class has a suitable constructor or static method that takes an `IEnumerable<T>`, you can use the new collection expression syntax.

```csharp
public class CustomNumbers : List<int>
{
    public CustomNumbers(IEnumerable<int> numbers) : base(numbers) { }
}

CustomNumbers nums = [4, 5, 6]; // Contains 4, 5, 6
```

This makes custom collections feel first-class.

### How Does the Spread Operator Work in Collection Expressions?

The spread operator (`..`) lets you inline and concatenate collections, insert elements from other collections, or even filter while building collections.

```csharp
var first = [1, 2, 3];
var second = [4, 5, 6];
var merged = [..first, ..second, 7]; // [1, 2, 3, 4, 5, 6, 7]

var odds = [1, 3, 5];
var all = [0, ..odds, 6, 8]; // [0, 1, 3, 5, 6, 8]

// Filtering inline
var nums = [1, 2, 3, 4, 5, 6];
var evens = [..nums.Where(n => n % 2 == 0)]; // [2, 4, 6]
```

This is extremely handy for pipeline-style code, such as when assembling dynamic documents for [IronPDF](https://ironpdf.com).

### Is Type Inference Smarter with Collection Expressions?

Absolutely. You can use collection expressions as method parameters or returns, and the type is inferred from context:

```csharp
List<string> GetNames() => ["Alice", "Bob", "Charlie"];

void UseScores(List<int> scores) { /* ... */ }
UseScores([100, 95, 80]);
```

No more `new List<T>()` or unnecessary conversions.

### What About Creating Empty Collections or Filtering as I Go?

Creating empty collections is as simple as `[]`, and you can combine with LINQ for on-the-fly filtering:

```csharp
List<int> empty = [];
var names = ["Anna", "Ben", "Charlie"];
var aNames = [..names.Where(n => n.StartsWith("A"))]; // ["Anna"]
```

### Are There Any Gotchas with Collection Expressions?

Yes, context is important—sometimes the type might not be what you expect (e.g., `int[]` when you wanted a `List<int>`). If there’s ambiguity, explicitly declare the type:

```csharp
List<int> numbers = [];
```

For more on new language features, see [What’s new in C# 14?](whats-new-csharp-14.md).

---

## What Are Primary Constructors and How Do They Change Class Design?

Primary constructors, previously limited to records, are now available for any class or struct. They drastically reduce boilerplate, especially for simple data-holding types or services with dependencies.

### How Do Primary Constructors Reduce Boilerplate in My Classes?

Let’s look at the difference:

#### Traditional Class Constructor

```csharp
public class Invoice
{
    private readonly string _customer;
    private readonly decimal _amount;

    public Invoice(string customer, decimal amount)
    {
        _customer = customer;
        _amount = amount;
    }

    public string GetSummary() => $"{_customer}: {_amount:C}";
}
```

#### Using a Primary Constructor (C# 14)

```csharp
public class Invoice(string customer, decimal amount)
{
    public string GetSummary() => $"{customer}: {amount:C}";
}
```

You can expose properties too:

```csharp
public class Product(string name, decimal price)
{
    public string Name { get; } = name;
    public decimal Price { get; } = price;
}
```

### How Do Primary Constructors Work with Dependency Injection?

Primary constructors make DI in ASP.NET Core and other frameworks much less verbose:

```csharp
public class NotificationService(IEmailSender emailSender, ILogger<NotificationService> logger)
{
    public async Task SendAsync(string recipient, string message)
    {
        logger.LogInformation("Sending to {Recipient}", recipient);
        await emailSender.SendAsync(recipient, message);
    }
}
```

Register with your DI container as usual:

```csharp
// In Program.cs
builder.Services.AddScoped<NotificationService>();
```

For more DI improvements, see [What’s New in .NET 10?](whats-new-in-dotnet-10.md).

### Can I Add Validation or Logic to Primary Constructors?

Yes—you can assign properties with validation inline, or include a constructor body for more logic:

```csharp
public class Order(int orderId, decimal total)
{
    public int OrderId { get; } = orderId > 0 ? orderId : throw new ArgumentException("Order ID must be positive");
    public decimal Total { get; } = total >= 0 ? total : throw new ArgumentException("Total must be non-negative");
}
```

Or with a logic body:

```csharp
public class User(string name, string email)
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public string Email { get; } = email ?? throw new ArgumentNullException(nameof(email));
    // additional logic goes here
}
```

### Are There Any Limitations or Pitfalls with Primary Constructors?

- You can’t reference `this` in parameter default values or initializers—use parameters directly.
- They are best suited for simple, immutable models or services.
- Some serializers or tools may need updates for full primary constructor support.

For hands-on guidance, check out [C# 14/.NET 10 tutorial](dotnet-10-csharp-14-tutorial.md).

---

## How Has Pattern Matching Improved in C# 14?

C# 14 delivers next-level pattern matching for collections, letting you match on contents, structure, or even slices of lists—all with clear, concise syntax.

### What Are List Patterns and How Do I Use Them?

List patterns let you match the structure and content of collections directly in a `switch`:

```csharp
var numbers = [99, 100, 101];

var result = numbers switch
{
    [] => "Empty",
    [var only] => $"One item: {only}",
    [var first, var second] => $"Two: {first}, {second}",
    [var first, .., var last] => $"First: {first}, Last: {last}",
    _ => "Many items"
};
```

### Can I Use Pattern Matching for Real-World Logic Like Grading or Routing?

Yes! Here are practical examples:

#### Grading

```csharp
string Grade(List<int> scores) => scores switch
{
    [] => "No scores",
    [>= 90] => "Solo A",
    [>= 90, >= 90, ..] => "Multiple A's",
    [.., < 60] => "Last one failed",
    _ => "Mixed"
};
```

#### URL Routing

```csharp
string Route(string[] parts) => parts switch
{
    [] => "Home",
    ["products", var id] => $"Product {id}",
    ["about"] => "About",
    _ => "404"
};
```

### What Are Slice Patterns and When Should I Use Them?

Slice patterns let you extract sections of a collection:

```csharp
var data = [10, 20, 30, 40];

var (head, tail) = data switch
{
    [var h, .. var t] => (h, t),
    _ => throw new InvalidOperationException()
};
// head: 10, tail: [20, 30, 40]
```

This is great for things like command parsing:

```csharp
void ParseCommand(string[] args)
{
    var message = args switch
    {
        ["add", .. var rest] => $"Add: {string.Join(", ", rest)}",
        ["remove", var item] => $"Remove: {item}",
        _ => "Unknown"
    };
    Console.WriteLine(message);
}
```

Pattern matching makes your code more expressive and easier to extend. For more advanced examples, see [C# 14/.NET 10 Tutorial](dotnet-10-csharp-14-tutorial.md).

---

## What Are Inline Arrays and Why Should I Care?

Inline arrays provide high-performance, stack-only storage for fixed-size buffers. They’re essential if you want zero heap allocation—for example, when parsing, buffering, or processing data in tight loops.

### How Do I Create and Use Inline Arrays in C# 14?

You define an inline array with the `[InlineArray(n)]` attribute on a struct:

```csharp
using System;
using System.Runtime.CompilerServices;

[InlineArray(16)]
public struct Buffer16<T>
{
    private T _element0;
}

var buffer = new Buffer16<byte>();
buffer[0] = 42;
Console.WriteLine(buffer[0]); // 42
```

No heap allocations are involved—these arrays are stack-allocated.

### When Do Inline Arrays Matter in Real Applications?

Inline arrays are ideal for scenarios like network packet processing, parsing file headers, or high-frequency operations in libraries such as [IronPDF](https://ironpdf.com):

```csharp
[InlineArray(512)]
public struct PacketBuffer
{
    private byte _element0;
}

void HandlePacket(ReadOnlySpan<byte> packet)
{
    var buffer = new PacketBuffer();
    int len = Math.Min(packet.Length, 512);
    for (int i = 0; i < len; i++)
        buffer[i] = packet[i];
    // Process buffer
}
```

They’re not managed by the garbage collector, so you avoid GC pressure in performance-critical code.

### Are There Pitfalls to Using Inline Arrays?

- They are value types; don’t assume reference-type semantics.
- Don’t store them in long-lived heap objects.
- Not all tools and serializers know about them yet.

---

## How Has the `nameof` Operator Improved in C# 14?

The `nameof` keyword, already a favorite for refactoring safety, is now more powerful and usable in more places, including attributes and static contexts.

### How Can I Use `nameof` in Attributes and Static Methods?

You can use `nameof` for error messages in attributes:

```csharp
using System.ComponentModel.DataAnnotations;

public void AddUser(
    [Required(ErrorMessage = $"{nameof(username)} is required")]
    string username,
    [Range(18, 100, ErrorMessage = $"{nameof(age)} must be 18-100")]
    int age)
{
    // ...
}
```

And you can now reference instance members from static methods:

```csharp
public class Transaction
{
    public decimal Amount { get; set; }
    public static string PropName() => nameof(Amount); // Works in static!
}
```

### What Are the Real-World Benefits of the Improved `nameof`?

You get safer logging, error handling, and messaging:

```csharp
void SaveFile(string path)
{
    if (string.IsNullOrWhiteSpace(path))
        throw new ArgumentException($"{nameof(path)} cannot be empty");
}
```

No more fragile string literals—refactoring is reliable.

---

## How Has `params` Been Enhanced to Accept More Collection Types?

C# 14 lets you use `params` with arrays, lists, spans, and other collections, eliminating the need for multiple overloads or conversions.

### How Do I Use `params` with Lists, Spans, or Collection Expressions?

#### Previously: Only Arrays

```csharp
void PrintNumbers(params int[] numbers)
{
    foreach (var n in numbers)
        Console.WriteLine(n);
}

PrintNumbers(1, 2, 3);
```

#### Now: Any Enumerable Collection

```csharp
void PrintNumbers(params IEnumerable<int> numbers)
{
    foreach (var n in numbers)
        Console.WriteLine(n);
}

PrintNumbers([5, 10, 15]);
```

#### Spans Work Too—No Heap Allocation

```csharp
void ShowBytes(params ReadOnlySpan<byte> bytes)
{
    foreach (var b in bytes)
        Console.WriteLine(b);
}

ShowBytes([1, 2, 3, 4]);
```

#### Example: Merging PDFs with IronPDF

```csharp
using IronPdf;

void MergeDocuments(params IEnumerable<PdfDocument> docs)
{
    var merger = new PdfDocumentMerger();
    foreach (var doc in docs)
        merger.AddDocument(doc);
    merger.SaveAs("combined.pdf");
}

MergeDocuments([
    new PdfDocument("intro.pdf"),
    new PdfDocument("content.pdf"),
    new PdfDocument("appendix.pdf")
]);
```

For more on document processing and logging, see [How do I implement custom logging in IronPDF?](ironpdf-custom-logging-csharp.md).

---

## Are Discriminated Unions Available in C# 14, and How Do I Use Them?

Discriminated unions (DUs) are available as an experimental feature in C# 14. They’re great for modeling “exactly one of these types,” similar to F# or Rust enums.

### How Do I Enable and Define a Discriminated Union in C#?

Enable preview features in your `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <LangVersion>preview</LangVersion>
    <EnablePreviewFeatures>true</EnablePreviewFeatures>
  </PropertyGroup>
</Project>
```

Define and use a union:

```csharp
public union Result<T, E>
{
    T Success,
    E Error
}

public Result<int, string> Divide(int a, int b)
{
    if (b == 0)
        return new Result<int, string>.Error("Division by zero");
    return new Result<int, string>.Success(a / b);
}

var result = Divide(10, 2);
var output = result switch
{
    { Success: var value } => $"Result: {value}",
    { Error: var err } => $"Error: {err}"
};
```

### Should I Use Discriminated Unions in Production?

Not yet—they’re still experimental. APIs and syntax could change. For now, they’re perfect for prototypes, tools, or experiments, but hold off for production.

---

## What’s New with Lock Statements and Thread Safety?

C# 14 introduces a dedicated `Lock` type for the `lock` statement, bringing better performance and clarity for thread synchronization.

### How Do I Use the New `Lock` Type?

Instead of locking on an `object`, use the new `Lock` type:

```csharp
private readonly Lock _sync = new();

void Increment()
{
    lock (_sync)
    {
        // Thread-safe logic here
    }
}
```

### Why Use the New Lock Statement?

- Reduces accidental boxing or misuse
- Signals intent more clearly to readers and reviewers
- May offer compiler optimizations

Don’t mix old `object` locks and the new `Lock` type in the same codebase.

---

## Can You Show Before-and-After Refactors Using C# 14 Features?

Here are some real-world refactors showing how C# 14 can simplify your code:

### How Can I Merge Lists Without Boilerplate?

**Before:**

```csharp
var res = new List<int>();
res.AddRange(source1);
res.AddRange(source2);
res.Add(extra);
var array = res.ToArray();
```

**After:**

```csharp
var array = [..source1, ..source2, extra];
```

### How Do Primary Constructors Clean Up Service Classes?

**Before:**

```csharp
public class UserService
{
    private readonly IUserRepo _repo;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepo repo, ILogger<UserService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<User> GetUserAsync(int id)
    {
        _logger.LogInformation("Getting user {Id}", id);
        return await _repo.GetByIdAsync(id);
    }
}
```

**After:**

```csharp
public class UserService(IUserRepo repo, ILogger<UserService> logger)
{
    public async Task<User> GetUserAsync(int id)
    {
        logger.LogInformation("Getting user {Id}", id);
        return await repo.GetByIdAsync(id);
    }
}
```

### How Does Pattern Matching Simplify Collection Analysis?

**Before:**

```csharp
string DescribeScores(List<int> scores)
{
    if (scores.Count == 0)
        return "No scores";
    if (scores.Count == 1)
        return $"Score: {scores[0]}";
    if (scores.All(s => s >= 90))
        return "All A's";
    return $"First: {scores[0]}, Last: {scores[^1]}";
}
```

**After:**

```csharp
string DescribeScores(List<int> scores) => scores switch
{
    [] => "No scores",
    [var only] => $"Score: {only}",
    [>= 90, >= 90, ..] => "All A's",
    [var first, .., var last] => $"First: {first}, Last: {last}"
};
```

---

## What Are Common Pitfalls or Troubleshooting Tips with C# 14?

- **Collection expressions need clear context:** Sometimes type inference surprises you—be explicit if needed.
- **Tooling support:** Not all IDEs or analyzers are updated for C# 14. Check your toolchain.
- **Primary constructor limitations:** Don’t try to reference `this` in parameter expressions.
- **Discriminated unions:** Experimental! Syntax and APIs may change.
- **Inline arrays:** These are value types, not references—don’t misuse them.
- **Lock statements:** Use the new `Lock` type, but don’t mix with old object locks.

For PDF-specific troubleshooting, see [How do I digitally sign PDFs in C#?](digitally-sign-pdf-csharp.md).

---

## Should I Adopt C# 14 Now, and Where Can I Learn More?

If you’re on .NET 10 and your tooling supports C# 14, absolutely start using these features—they make code easier to write, read, and maintain. Features like collection expressions, primary constructors, and advanced pattern matching are immediately valuable in real projects.

If you’re building document processing apps, libraries like [IronPDF](https://ironpdf.com) are already C# 14-ready. The team at [Iron Software](https://ironsoftware.com) is using these features internally to boost performance and clarity.

Curious about more .NET/C# features? Check out these related FAQs:
- [What’s new in C# 14?](whats-new-csharp-14.md)
- [What’s new in .NET 10?](whats-new-in-dotnet-10.md)
- [Hands-on tutorial for .NET 10 and C# 14](dotnet-10-csharp-14-tutorial.md)
- [How do I implement custom logging in IronPDF?](ironpdf-custom-logging-csharp.md)
- [How do I digitally sign PDFs in C#?](digitally-sign-pdf-csharp.md)
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

**Jacob Mellor** — Creator of IronPDF, CTO at Iron Software. 41 years coding, 50++ person team, 41+ million NuGet downloads. Libraries handle billions of PDFs annually. [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/) | [GitHub](https://github.com/jacob-mellor)
