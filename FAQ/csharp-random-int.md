# How Do I Generate Random Integers and Work with Randomness in C#?

Random numbers in C# are essential for games, test data, shuffling, and security-sensitive operations like generating tokens. But not all random numbers are created equal—subtle mistakes can cause bugs, security flaws, or non-random results. Here’s a practical FAQ for developers who want to generate random integers—and do it right.

---

## What’s the Best Way to Generate Random Integers in C#?

The standard approach uses the `Random` class. Create an instance and use `.Next()`:

```csharp
using System; // No extra NuGet needed

var rng = new Random();
int value = rng.Next(1, 101); // 1 to 100 (upper bound is exclusive)
Console.WriteLine($"Random number: {value}");
```

Calling `rng.Next()` with no parameters gives you a value from 0 up to `Int32.MaxValue`. Want a specific range? Pass your minimum (inclusive) and maximum (exclusive) values.

## How Do I Specify Ranges Correctly When Using Random.Next()?

Remember: `.Next(min, max)` includes `min` but **excludes** `max`. So `rng.Next(1, 7)` gives you values 1 through 6—just like a die. If you need an *inclusive* upper bound, add one to your max:

```csharp
int dice = rng.Next(1, 7); // 1 to 6
int inclusive = rng.Next(1, 101); // 1 to 100
```

If you often want inclusive ranges, consider a helper:

```csharp
public static int NextInclusive(Random rng, int min, int maxInc)
{
    return rng.Next(min, maxInc + 1);
}
```

## Can I Generate Unique Random Numbers, Like Lottery Picks?

Yes. To get unique random numbers within a range, use a `HashSet<int>` to avoid duplicates:

```csharp
var random = new Random();
var uniqueNumbers = new HashSet<int>();

while (uniqueNumbers.Count < 6)
{
    uniqueNumbers.Add(random.Next(1, 50)); // 1 to 49 inclusive
}
Console.WriteLine("Lottery: " + string.Join(", ", uniqueNumbers));
```

For more complex document scenarios, see how you can [add, copy, or delete PDF pages in C#](add-copy-delete-pdf-pages-csharp.md).

## Is It Okay to Create a New Random Instance in a Loop?

No—don’t do this! Creating new `Random` objects rapidly (especially in a loop) causes repeated or identical numbers because each instance is seeded from the system clock:

```csharp
// Bad: Will likely repeat numbers
for (int i = 0; i < 5; i++)
{
    var rng = new Random();
    Console.WriteLine(rng.Next(100));
}
```

**Best practice:** Create one `Random` per class or method and reuse it:

```csharp
var rng = new Random();
for (int i = 0; i < 5; i++)
{
    Console.WriteLine(rng.Next(100));
}
```

## How Can I Generate Realistic Test Data with Random Numbers?

Randomness is great for generating sample ages, prices, or test datasets:

```csharp
var rng = new Random();
var ages = new List<int>();
for (int i = 0; i < 10; i++) ages.Add(rng.Next(18, 66));
Console.WriteLine("Test ages: " + string.Join(", ", ages));
```

For more realistic numbers (like prices to two decimals), combine with rounding:

```csharp
var rng = new Random();
decimal price = Math.Round((decimal)(rng.NextDouble() * 1000), 2);
```

Need to round decimals elsewhere? See [how to round to 2 decimal places in C#](csharp-round-to-2-decimal-places.md).

## What If I Need Cryptographically Secure Random Numbers?

For tokens, passwords, or anything security-related, use `RandomNumberGenerator`:

```csharp
using System.Security.Cryptography;

int secureInt = RandomNumberGenerator.GetInt32(1, 101); // 1 to 100
```

To create a secure random byte array (great for tokens):

```csharp
byte[] token = new byte[32];
RandomNumberGenerator.Fill(token);
string base64 = Convert.ToBase64String(token);
Console.WriteLine(base64);
```

## How Do I Seed Random for Reproducible Results in Tests?

Pass a seed integer to the `Random` constructor—this makes your results repeatable:

```csharp
var rng = new Random(1234);
int always33 = rng.Next(100); // Always 33
```

This is crucial for unit tests that involve randomness. If you shuffle data, always use a seeded RNG in your test setup.

## How Should I Handle Randomness in Multi-Threaded Code?

The standard `Random` isn’t thread-safe. For parallel scenarios, use `ThreadLocal<Random>`:

```csharp
using System.Threading;

var threadRng = new ThreadLocal<Random>(() => new Random());

Parallel.For(0, 100, i =>
{
    Console.WriteLine(threadRng.Value.Next(100));
});
```

Or, if you’re on .NET 6+, use the built-in thread-safe `Random.Shared`:

```csharp
int n = Random.Shared.Next(1, 101);
```

If you’re generating PDFs in parallel, see [how to work with MemoryStreams for PDFs in C#](pdf-memorystream-csharp.md).

## How Do I Generate Random Bytes or Doubles?

For bytes:

```csharp
var rng = new Random();
byte[] data = new byte[16];
rng.NextBytes(data);
Console.WriteLine(BitConverter.ToString(data));
```

For doubles in a specific range:

```csharp
double d = rng.NextDouble(); // 0.0 <= d < 1.0
double scaled = 50.0 + (d * 50.0); // 50.0 to just under 100.0
```

## How Can I Shuffle, Sample, or Make Weighted Random Choices?

**Shuffle (Fisher-Yates):**

```csharp
public static void Shuffle<T>(T[] items, Random rng)
{
    for (int i = items.Length - 1; i > 0; i--)
    {
        int j = rng.Next(i + 1);
        (items[i], items[j]) = (items[j], items[i]);
    }
}
```

**Weighted random selection:**

```csharp
public static T WeightedChoice<T>(T[] options, double[] weights, Random rng)
{
    double total = weights.Sum();
    double r = rng.NextDouble() * total;
    double acc = 0;
    for (int i = 0; i < options.Length; i++)
    {
        acc += weights[i];
        if (r < acc) return options[i];
    }
    return options[^1];
}
```

**Sampling without replacement:**

```csharp
var list = new List<string> { "A", "B", "C", "D" };
var selected = list.OrderBy(_ => rng.Next()).Take(2).ToList();
```

## What Are Common Pitfalls with Random Numbers in C#?

- **Exclusive upper bound:** `Next(1, 7)` means 1–6, *not* 1–7.
- **Multiple Random instances:** Avoid creating new `Random()` in quick succession.
- **Thread-safety:** Don’t share a single `Random` across threads.
- **Security:** Never use `Random` for sensitive info; use `RandomNumberGenerator` instead.
- **Non-deterministic tests:** Always seed your RNG for reliable test results.

For more on manipulating PDFs, see [how to add attachments to PDFs in C#](add-attachments-pdf-csharp.md) or [work with the PDF DOM in C#](access-pdf-dom-object-csharp.md).

## How Can I Use Randomness When Generating Dynamic PDFs?

Generating randomized PDFs is easy with [IronPDF](https://ironpdf.com):

```csharp
using IronPdf; // Install-Package IronPdf

var rng = new Random();
var html = "<h1>Invoice</h1><table border='1'>";
for (int i = 1; i <= 5; i++)
{
    int qty = rng.Next(1, 10);
    decimal price = Math.Round((decimal)(rng.NextDouble() * 100), 2);
    html += $"<tr><td>Product {i}</td><td>{qty}</td><td>${price}</td></tr>";
}
html += "</table>";

var renderer = new IronPdf.ChromePdfRenderer();
renderer.RenderHtmlAsPdf(html).SaveAs("Invoice.pdf");
```

If you want to manipulate the structure of the PDF after creation, see [accessing the PDF DOM in C#](access-pdf-dom-object-csharp.md).

## When Should I Use Random vs. RandomNumberGenerator?

- Use `Random` for games, test data, or casual randomness.
- Use `RandomNumberGenerator` for anything security-sensitive (tokens, passwords).

For more on dynamic PDF generation and secure document workflows, check [IronPDF](https://ironpdf.com) and [Iron Software](https://ironsoftware.com).

---

*[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), CTO at [Iron Software](https://ironsoftware.com). Building tools that make developers' lives easier since 2017.*
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

**Jacob Mellor** — Creator of IronPDF. C# enthusiast, Visual Studio lover, and usability-driven API designer. Connect on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
