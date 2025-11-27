# What Are the Most Common C# Mistakes Developers Make, and How Can I Avoid Them?

Every C# developer, whether new or experienced, can stumble into classic pitfalls lurking in the language and .NET ecosystem. From resource leaks to async nightmares, many of these mistakes are easy to fix—if you know what to look out for. This FAQ highlights the most frequent C# errors, why they matter, and how to sidestep them with practical code samples and best practices.

---

## Why Is Disposing `IDisposable` Objects So Important in C#?

Failing to dispose of `IDisposable` objects, such as file handles or PDF generators like IronPDF, leads to resource leaks and can eventually crash your application. If you see a type marked as `IDisposable`, always use a `using` statement.

```csharp
// Install-Package IronPdf
using IronPdf; // NuGet package
public void CreatePdf(string html)
{
    using var pdf = new PdfDocument();
    pdf.AddHtml(html);
    pdf.SaveAs("output.pdf");
}
```
C# 8 introduced the cleaner `using var` syntax—no extra nesting required. For more on manipulating PDFs, see [How can I access the PDF DOM in C#?](access-pdf-dom-object-csharp.md)

---

## When Should I Catch Exceptions, and Why Should I Avoid Catching All Exceptions?

Catching `Exception` broadly can hide bugs and swallow critical errors that should be addressed. Instead, catch the exceptions you know how to handle.

```csharp
public async Task<string?> FetchDataAsync(HttpClient client, ILogger log)
{
    try
    {
        return await client.GetStringAsync("https://api.example.com");
    }
    catch (HttpRequestException ex)
    {
        log.LogError(ex, "Network issue");
        return null;
    }
    // Only catch what you can handle!
}
```
Letting unexpected exceptions surface during development helps you catch issues before production.

---

## How Should I Properly Use `async`/`await` in My C# Code?

Omitting `await` when additional processing is needed can result in missed exceptions or unexpected behavior. If you manipulate the result, always use `async`/`await`.

```csharp
public async Task<string> GetContentAsync()
{
    var response = await _httpClient.GetStringAsync("https://api.site.com");
    // Additional logic here if needed
    return response;
}
```
If you’re just returning the task without extra logic, returning the `Task` directly is fine. For more on extracting text from PDFs, check [How do I parse and extract text from PDFs in C#?](parse-pdf-extract-text-csharp.md)

---

## Why Should I Avoid `.Result` or `.Wait()` in UI and Web Apps?

Using `.Result` or `.Wait()` on asynchronous tasks, especially in ASP.NET or UI applications, can deadlock your application. Instead, make your methods fully asynchronous:

```csharp
public async Task LoadUserDataAsync()
{
    var data = await FetchDataAsync();
    // Use data after await
}
```
Adopting async all the way up your call stack leads to more responsive and robust applications.

---

## What’s the Safest Way to Modify Collections While Iterating?

Changing a collection while iterating over it causes exceptions. To safely remove items, create a snapshot or use dedicated methods:

```csharp
var numbers = new List<int> {1, 2, 3, 4};
numbers.RemoveAll(n => n % 2 == 0); // Removes even numbers safely
```
Or iterate over a copy:

```csharp
foreach (var n in numbers.Where(x => x < 3).ToList())
    numbers.Remove(n);
```
For more advanced PDF page manipulation, see [How do I add, copy, or delete pages in a PDF using C#?](add-copy-delete-pdf-pages-csharp.md)

---

## How Can I Prevent Null Reference Exceptions in Modern C#?

Null checks are essential, especially with nullable reference types (C# 8+). Enable nullability and check for `null` at the start of your methods:

```csharp
#nullable enable
public void PrintUserName(User? user)
{
    if (user == null)
        throw new ArgumentNullException(nameof(user));
    Console.WriteLine(user.Name?.ToUpper() ?? "Unknown");
}
```
Enable `#nullable` in your files or project settings for compiler assistance.

---

## Is It Safe to Use `==` for String Comparison in C#?

Using `==` for strings can cause subtle bugs due to case sensitivity and culture differences. Prefer `string.Equals` with explicit options:

```csharp
if (string.Equals(input, "admin", StringComparison.OrdinalIgnoreCase))
{
    // Correct, case-insensitive comparison
}
```
This approach is reliable for user input and avoids culture-related issues.

---

## When Is It Acceptable to Use `async void` Methods?

You should virtually never use `async void`—except for event handlers in UI code. For all other async methods, return `Task` or `Task<T>` so that exceptions are observable.

```csharp
public async Task SyncDataAsync()
{
    await _apiClient.SendDataAsync();
}
// Event handler (acceptable)
private async void Button_Click(object sender, EventArgs e)
{
    await SyncDataAsync();
}
```
If it’s not an event handler, avoid `async void`.

---

## Why Should I Avoid String Concatenation in Loops?

Concatenating strings repeatedly in a loop is inefficient, as each operation creates a new string. Use `StringBuilder` for better performance:

```csharp
using System.Text;
var builder = new StringBuilder();
for (int i = 0; i < 1000; i++)
{
    builder.AppendLine($"Row {i}");
}
string result = builder.ToString();
```
This is especially crucial when generating large reports or preparing content for PDF generation.

---

## What Is the Role of `.ConfigureAwait(false)` in Library Code?

When writing library code, use `.ConfigureAwait(false)` to prevent capturing the caller’s synchronization context, avoiding deadlocks in UI or ASP.NET apps.

```csharp
public async Task<string> DownloadDataAsync()
{
    return await _httpClient.GetStringAsync("https://api.site.com")
        .ConfigureAwait(false);
}
```
This ensures your library works reliably in any context.

---

## Why Should I Use `DateTime.UtcNow` Instead of `DateTime.Now`?

Storing or logging dates with `DateTime.Now` embeds local time, which can vary by server location and cause confusion. Use `DateTime.UtcNow` for consistency:

```csharp
var timestamp = DateTime.UtcNow;
```
Convert to local time only for display:

```csharp
var local = TimeZoneInfo.ConvertTimeFromUtc(timestamp, TimeZoneInfo.Local);
```
Consistent timestamps make debugging and analytics much easier.

---

## How Do I Properly Validate User Input in C#?

Never trust user input—validate it as soon as it enters your system. For instance, validating an email:

```csharp
using System.Text.RegularExpressions;
public void Register(string email)
{
    if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        throw new ArgumentException("Invalid email address.");
    // Proceed with registration
}
```
Sanitize and validate at the controller or API entry point to prevent security issues.

---

## Is It Ever Correct to Use `Task.Run()` in ASP.NET to “Make Code Async”?

Avoid using `Task.Run()` in ASP.NET controllers for I/O-bound work—it just wastes threads. Use truly asynchronous methods instead:

```csharp
public async Task<IActionResult> GetData()
{
    var result = await _db.FetchAsync();
    return Ok(result);
}
```
`Task.Run()` is only justified for CPU-intensive tasks that truly need to run in the background.

---

## How Should I Use `CancellationToken` in Async Operations?

Accept and propagate `CancellationToken` in all long-running async operations. This allows users or callers to cancel requests, improving responsiveness.

```csharp
public async Task ProcessFileAsync(string path, CancellationToken cancellationToken)
{
    using var stream = File.OpenRead(path);
    await ReadAllAsync(stream, cancellationToken);
}
```
Pass the token down to all async calls for full support.

---

## Why Is Hardcoding Connection Strings or Secrets a Bad Idea?

Storing sensitive configuration, like connection strings, directly in your code can lead to leaks and makes deployments inflexible. Use configuration files or environment variables instead:

```csharp
using Microsoft.Extensions.Configuration;
public class Repository
{
    private readonly string _connStr;
    public Repository(IConfiguration config)
    {
        _connStr = config.GetConnectionString("MyDb");
    }
}
```
Leverage secure storage like Azure Key Vault for production secrets. For attaching files to PDFs securely, see [How do I add attachments to PDFs in C#?](add-attachments-pdf-csharp.md)

---

## What Does a Safe, Modern PDF Generation Controller in C# Look Like?

Here’s how you pull several best practices together with IronPDF in an ASP.NET Core controller:

```csharp
// Install-Package IronPdf
using IronPdf;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("reports")]
public class ReportsController : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateReport([FromBody] ReportInput input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.HtmlContent))
            return BadRequest("Content required.");

        using var renderer = new HtmlToPdf();
        var pdfDoc = await renderer.RenderHtmlAsPdfAsync(input.HtmlContent, cancellationToken);

        using var stream = new MemoryStream();
        pdfDoc.SaveAs(stream);
        stream.Position = 0;

        return File(stream, "application/pdf", "report.pdf");
    }
}

public class ReportInput
{
    public string HtmlContent { get; set; }
}
```
This controller validates input, manages resources, handles cancellation, and works asynchronously. Learn more about IronPDF at [ironpdf.com](https://ironpdf.com) and other developer tools from [Iron Software](https://ironsoftware.com).

---

## What Are Some Quick Fixes for Common C# Problems?

**Async not running as expected?** Check for `.Result`/`.Wait()` or missing `async` all the way up.  
**Getting “Collection was modified” errors?** Don’t change collections during iteration—use `.RemoveAll()` or iterate over a snapshot.  
**Strange timestamps?** Switch to `DateTime.UtcNow`.  
**Library code hangs in ASP.NET?** Use `.ConfigureAwait(false)`.  
**Uncaught exceptions in async code?** Don’t use `async void` unless required for event handlers.  
**Need to clean up after exceptions?** Wrap disposables in `using`.  
**Unreliable string comparisons?** Use `string.Equals` with a specified comparison.

For more on working with PDF files and their contents, see [How do I access the PDF DOM in C#?](access-pdf-dom-object-csharp.md)

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

[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) leads Iron Software's 50+-person engineering team from Chiang Mai, Thailand. A First-Class Honours Bachelor of Engineering (BEng) graduate from The University of Manchester, Jacob has built PDF libraries achieving 41+ million downloads. [GitHub](https://github.com/jacob-mellor) | [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/)
