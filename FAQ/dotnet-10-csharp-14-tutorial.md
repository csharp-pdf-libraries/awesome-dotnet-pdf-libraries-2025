# What Are the Must-Know Features and Pitfalls of .NET 10 and C# 14?

.NET 10 and C# 14 have landed with a focus on making developers’ lives easier—introducing features that streamline scripting, enhance language expressiveness, and boost performance out of the box. Whether you want to write C# scripts with zero project boilerplate, add extension properties to legacy types, or squeeze more speed from your APIs, this FAQ covers the most important updates, gotchas, and real-world code samples. Read on for practical answers to the questions every .NET developer is asking.

---

## How Can I Use C# 14 as a Script Without a Project File?

.NET 10 allows you to execute single `.cs` files directly from the command line, bringing C# much closer to the scripting experience of languages like Python or Bash.

You can now run a file like this:

```csharp
// hello.cs
Console.WriteLine("Hello, .NET 10!");
```

To execute:

```bash
dotnet run hello.cs
```

### Can I Pass Arguments to My Script?

Absolutely. The `args` array works as you expect:

```csharp
// greet.cs
if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run greet.cs [name]");
    return;
}
Console.WriteLine($"Hi, {args[0]}!");
```

### What About NuGet Packages in Single-File Scripts?

You can reference NuGet packages with the `#r "nuget: ..."` directive, which is great for quick experiments. For example, generating a PDF with [IronPDF](https://ironpdf.com):

```csharp
#r "nuget: IronPdf, 2024.6.0"

using IronPdf;

var doc = PdfDocument.FromHtml("<h2>Hello PDF!</h2>");
doc.SaveAs("output.pdf");
```

Keep in mind, not all packages support this mode seamlessly. If you hit issues or need to structure more complex scripts, consider creating a standard project. For more on working with PDF files and related tasks, see [How can I extract images from PDFs in C#?](extract-images-from-pdf-csharp.md) and [How do I convert a PDF to a MemoryStream in C#?](pdf-to-memorystream-csharp.md).

---

## What’s New with Extension Members in C# 14?

C# 14 expands extension methods to include **properties, events, and even operators**, letting you add richer APIs to types you don’t own.

### How Do Extension Properties Work?

You can declare extension properties directly on types, so the syntax feels native:

```csharp
public implicit extension StringExtras for string
{
    public bool IsNullOrEmpty => string.IsNullOrEmpty(this);
    public int WordCount => this?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
}

// Usage:
string text = "IronPDF makes PDFs easy";
Console.WriteLine(text.WordCount);      // 4
Console.WriteLine(text.IsNullOrEmpty);  // False
```

### Can I Add Events or Operators to Existing Types?

Yes! You can even add events to collections:

```csharp
public implicit extension ListNotify<T> for List<T>
{
    public event Action? OnCleared;
    public void ClearAndNotify()
    {
        this.Clear();
        OnCleared?.Invoke();
    }
}

var items = new List<int> { 1, 2, 3 };
items.OnCleared += () => Console.WriteLine("List cleared!");
items.ClearAndNotify();
```

### Why Should I Use Extension Members?

- **Cleaner code:** Attach logic where it belongs, not in scattered static classes.
- **Expressiveness:** Properties and events feel natural, making your code more readable.

For even more ways to write modern, idiomatic code, check out [What C# patterns should every .NET developer know?](csharp-patterns-dotnet-developers.md).

---

## What Does the `field` Keyword Do in C# 14?

C# 14 introduces the `field` keyword, which replaces the old backing-field pattern in property accessors.

### How Does `field` Simplify Properties?

Instead of declaring a private field, you can reference `field` directly inside your property:

```csharp
public string Email
{
    get => field;
    set => field = value?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException();
}
```

This eliminates repetitive boilerplate and keeps your class definitions clean.

### Are There Limitations to Using `field`?

- You can only use `field` inside property getters and setters.
- You can't reference `field` in constructors or other class methods.

If you need more complex logic, you may still need explicit field declarations.

---

## How Does Null-Conditional Assignment Improve My Code?

You can now use `?.` and `?[]` on the left-hand side of assignments, drastically reducing manual null checks.

### What Does Null-Conditional Assignment Look Like?

```csharp
Customer? customer = FindCustomer();
customer?.Address?.City = "Chicago";
```

This is equivalent to:

```csharp
if (customer?.Address != null)
{
    customer.Address.City = "Chicago";
}
```

### Are Compound Assignments Supported?

Yes! You can do things like:

```csharp
Order? order = GetOrder();
order?.Total += 50;

// Arrays work too
string[]? names = null;
names?[0] = "Test"; // Safe—no exception thrown if names is null
```

**Tip:** If the left chain is null, the assignment is silently skipped.

---

## How Has `nameof` Improved for Generics?

You can now use `nameof` with unbound generic types, making reflection and diagnostics cleaner.

```csharp
var listTypeName = nameof(List<>);        // "List"
var dictTypeName = nameof(Dictionary<,>); // "Dictionary"
```

This helps when logging or generating attribute/type names dynamically.

---

## What’s Changed with Span Conversions in C# 14?

Arrays now convert implicitly to `Span<T>` and `ReadOnlySpan<T>`, making fast memory access easier and code cleaner.

### How Do I Use Implicit Span Conversions?

```csharp
using System;
using System.IO;

byte[] data = File.ReadAllBytes("file.pdf");
ReadOnlySpan<byte> fileSpan = data;

// Pass to a method expecting a span
ProcessData(fileSpan);

void ProcessData(ReadOnlySpan<byte> span)
{
    Console.WriteLine($"Length: {span.Length}");
}
```

You can also slice spans, use them in assignments, and pass them between APIs with zero allocations. For more on high-performance .NET UI frameworks that leverage spans and memory efficiency, see [Avalonia vs MAUI in .NET 10: Which Should You Choose?](avalonia-vs-maui-dotnet-10.md).

---

## What Are the Most Important Runtime Upgrades in .NET 10?

Even without changing any code, .NET 10 delivers:

- **Smarter JIT inlining and devirtualization** for faster method calls.
- **Improved stack allocation and loop optimizations** (great for tasks like PDF rendering with [IronPDF](https://ironpdf.com)).
- **AVX10.2 SIMD support** if you do heavy math/image work.
- **NativeAOT**: Publish native executables more easily.

Run your performance benchmarks—you'll often see big wins automatically.

---

## Which Library Upgrades Should Developers Know?

.NET 10 brings several improvements across cryptography, JSON handling, and collections.

### What’s New in Cryptography?

- **Post-quantum algorithms**: ML-KEM, ML-DSA, SLH-DSA, preparing you for future security needs.
- **Stronger defaults** in JSON serialization and HTTPS.

### What’s Improved in JSON Serialization?

- **Stricter parsing**: Block ambiguous or duplicate properties with new options.
- **PipeReader support**: Efficiently stream large JSON files.

```csharp
using System.Text.Json;

var options = new JsonSerializerOptions
{
    AllowDuplicateProperties = false,
    StrictMode = true
};
```

For more on handling images and PDFs, see [How do I extract images from PDFs with C#?](extract-images-from-pdf-csharp.md).

### What About Collections?

- **OrderedDictionary<TKey, TValue>**: Predictable iteration order with dictionary performance.
- **General performance gains** in all collection types.

---

## How Has ASP.NET Core 10 Improved API Development?

ASP.NET Core 10 makes it much easier to build APIs quickly and reliably.

### How Does Minimal API Validation Work?

Validation is now almost automatic:

```csharp
app.MapPost("/register", ([FromBody] User user) =>
{
    return Results.Created($"/users/{user.Id}", user);
}).WithValidation();
```

### What About OpenAPI and YAML Support?

You can now generate OpenAPI specs in YAML and target the latest 3.1 version:

```csharp
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiVersion.v3_1;
    options.OutputFormat = OpenApiFormat.Yaml;
});
```

### Can I Use Server-Sent Events (SSE) Easily?

Yes—real-time push is straightforward:

```csharp
app.MapGet("/events", async (HttpContext ctx) =>
{
    ctx.Response.Headers.ContentType = "text/event-stream";
    for (int i = 0; i < 3; i++)
    {
        await ctx.Response.WriteAsync($"data: event {i}\n\n");
        await ctx.Response.Body.FlushAsync();
        await Task.Delay(1000);
    }
});
```

If you're considering UI frameworks for cloud or cross-platform scenarios, see [Should I use Blazor in .NET 10?](dotnet-10-blazor.md).

---

## What’s New in Entity Framework Core 10?

Entity Framework Core 10 adds major productivity and expressiveness boosts.

### How Do Complex Types Work?

You can now embed value objects directly, with columns auto-prefixed in the DB schema:

```csharp
[ComplexType]
public class Address
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
}

public class Customer
{
    public int Id { get; set; }
    public Address Billing { get; set; } = new();
}
```

### Are Left and Right Joins Supported in LINQ?

Yes—LINQ now supports more natural join expressions:

```csharp
var query = from c in context.Customers
            join o in context.Orders on c.Id equals o.CustomerId into orders
            from o in orders.DefaultIfEmpty()
            select new { c.Name, OrderId = o?.Id };
```

### What Are Named Filters?

Chain and toggle multiple query filters for scenarios like soft deletes or multi-tenant apps:

```csharp
modelBuilder.Entity<Order>()
    .HasQueryFilter(o => !o.IsDeleted, name: "SoftDelete")
    .HasQueryFilter(o => o.TenantId == tenantId, name: "Tenant");

var visibleOrders = context.Orders.ToList();
var withDeleted = context.Orders.IgnoreQueryFilters("SoftDelete").ToList();
```

---

## Can You Share Real-World Code Examples Using .NET 10 and C# 14?

### PDF Generation Script with [IronPDF](https://ironpdf.com)

```csharp
#r "nuget: IronPdf, 2024.6.0"
using IronPdf;

var html = "<h1>Instant Report</h1><p>Generated on " + DateTime.Now + "</p>";
var pdfDoc = PdfDocument.FromHtml(html);
pdfDoc.SaveAs("quick-report.pdf");
Console.WriteLine("PDF created!");
```

### Extension Members for Money Handling

```csharp
public implicit extension MoneyHelpers for decimal
{
    public string AsCurrency(string culture = "en-US") =>
        string.Format(new System.Globalization.CultureInfo(culture), "{0:C}", this);

    public bool IsPositive => this > 0;
}

decimal price = 29.99m;
Console.WriteLine(price.AsCurrency());
Console.WriteLine(price.IsPositive);
```

### Minimal API with Validation and SSE

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var app = WebApplication.CreateBuilder().Build();

app.MapPost("/user", ([FromBody] User user) =>
{
    return Results.Created($"/user/{user.Id}", user);
}).WithValidation();

app.MapGet("/progress", async (HttpContext ctx) =>
{
    ctx.Response.Headers.ContentType = "text/event-stream";
    for (int i = 0; i <= 100; i += 25)
    {
        await ctx.Response.WriteAsync($"data: Progress {i}%\n\n");
        await ctx.Response.Body.FlushAsync();
        await Task.Delay(400);
    }
});

app.Run();

record User(string Name, int Id);
```

---

## How Do I Migrate an Existing Project to .NET 10?

1. **Install the .NET 10 SDK**  
   Check your version:  
   ```bash
   dotnet --version
   ```
   Download from the official [.NET website](https://dotnet.microsoft.com/) if needed.

2. **Update Your Project File**  
   Change your `.csproj`:
   ```xml
   <PropertyGroup>
     <TargetFramework>net10.0</TargetFramework>
   </PropertyGroup>
   ```

3. **Restore and Build**  
   Update NuGet packages and build:
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Enable C# 14 Syntax**  
   This is automatic, but you can add:
   ```xml
   <LangVersion>14.0</LangVersion>
   ```

5. **Run All Tests**  
   Ensure your app behaves as expected after the upgrade, especially if you rely on third-party libraries like [IronPDF](https://ironpdf.com).

---

## What Pitfalls Should I Watch Out for When Using .NET 10 and C# 14?

- **NuGet with File-Based Apps:**  
  Some packages may not work in script mode; fallback to a project if you hit errors.

- **Extension Member Name Conflicts:**  
  If you introduce an extension property that shares a name with an existing member, you may get ambiguous references.

- **`field` Keyword Scope:**  
  Only use `field` inside property accessors.

- **Null-Conditional Assignment Gotchas:**  
  If the left chain is always null (or you typo a property), your assignment will silently do nothing.

- **JSON StrictMode:**  
  Enabling strict JSON parsing can break compatibility with poorly formed JSON from third parties.

- **EF Core Named Filter Order:**  
  The order of named query filters can impact results—test combinations thoroughly.

- **NativeAOT Compatibility:**  
  Not all libraries (especially those using reflection or dynamic code) support NativeAOT. Test carefully if you’re targeting native publishing, particularly for document/image processing.

---

## Where Can I Learn More About Related .NET 10 and C# Features?

- [Avalonia vs MAUI in .NET 10: Which Should You Choose?](avalonia-vs-maui-dotnet-10.md)
- [What C# patterns should every .NET developer know?](csharp-patterns-dotnet-developers.md)
- [Should I use Blazor in .NET 10?](dotnet-10-blazor.md)
- [How can I extract images from PDFs in C#?](extract-images-from-pdf-csharp.md)
- [How do I convert a PDF to a MemoryStream in C#?](pdf-to-memorystream-csharp.md)

For more about .NET document generation, see [IronPDF](https://ironpdf.com) and [Iron Software](https://ironsoftware.com).

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

[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) is the Head of Technology and Engineering of Iron Software, where he leads a 50+ person team building .NET libraries with over 41+ million NuGet downloads. With 41 years of coding experience, Jacob drives technical innovation in document processing. Based in Chiang Mai, Thailand, connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
