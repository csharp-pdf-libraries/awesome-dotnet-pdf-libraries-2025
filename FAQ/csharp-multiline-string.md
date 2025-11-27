# How Do I Work with Multiline Strings in C#: Raw, Verbatim, Interpolated, and Real-World Tips?

Dealing with multiline strings in C# can quickly become messy—think SQL queries, JSON blobs, HTML templates, or even file paths. But C# offers powerful ways to keep your strings readable and maintainable, especially with features like verbatim, interpolated, and raw string literals. In this FAQ, I’ll walk you through how and when to use each approach, show practical examples, and flag common pitfalls—so your code can be as clean as the output you want.

---

## What Are the Main Ways to Define Multiline Strings in C#?

C# supports several approaches for multiline strings, each with its own strengths. Let’s break down the three most common:

### How Do I Use Regular Strings in C#?

A regular string is just what you get with double quotes. It’s simple, but you’ll quickly run into trouble with escape characters for newlines, tabs, or file paths.

```csharp
string greeting = "Hello,\nWorld!";
string filePath = "C:\\Users\\Dev\\Documents\\report.txt";
Console.WriteLine(greeting);
// Output:
// Hello,
// World!
```

Regular strings are fine for short snippets, but as soon as you need complex formatting or lots of backslashes, things can get hard to read.

### When Should I Use Verbatim Strings (`@""`)?

Verbatim strings start with an `@` and treat everything literally—except for doubled double quotes, which become a single `"` in the output. They’re great for Windows file paths, SQL queries, and any multiline content where you want your code to match the output.

```csharp
string path = @"C:\Projects\MyApp\config.json";
string query = @"
SELECT Name, Email
FROM Users
WHERE IsActive = 1
";
```

**Tip:** For double quotes inside verbatim strings, just double them:

```csharp
string json = @"{ ""name"": ""Alice"", ""role"": ""admin"" }";
```

### What’s Special About Raw String Literals (`"""..."""`) in C# 11+?

Raw string literals (introduced in C# 11) use triple quotes (`"""`). They let you write exactly what you want—no escaping backslashes, quotes, or line breaks. Great for JSON, HTML, XML, or anything with lots of symbols.

```csharp
string json = """
{
  "user": "Bob",
  "permissions": ["read", "write"]
}
""";
```

- No escapes needed, even for quotes or backslashes.
- Opening and closing triple quotes must be on their own lines.
- Indentation is handled based on the position of the closing quotes.

If you work with HTML-to-PDF generation, check out [How do I convert HTML strings to PDF in C#?](html-string-to-pdf-csharp.md) for more on this style.

---

## How Can I Insert Variables into Multiline Strings?

Most templates need dynamic content. Here’s how to keep your strings both readable and flexible.

### How Does Interpolation Work with Verbatim Strings?

Add a `$` before your verbatim string (`$@""`) to enable variable interpolation:

```csharp
string username = "Charlie";
int userId = 42;
string sql = $@"
SELECT *
FROM Users
WHERE Id = {userId} AND Name = '{username}'
";
```

- Use `{expression}` to insert values.
- Always parameterize SQL in real apps—don’t interpolate user input directly!

### How Can I Interpolate Variables in Raw String Literals?

Raw string literals can also be interpolated by prefixing with `$`:

```csharp
string product = "Laptop";
decimal price = 1199.99m;
string html = $"""
<p>Product: {product}</p>
<p>Price: ${price:F2}</p>
""";
```

This is super handy for JSON, HTML, or any template-heavy scenario.

### What About Curly Braces in Interpolated Raw Strings?

When you need curly braces as part of your output (think CSS or template syntax), use double curly braces and double `$`:

```csharp
string highlight = "#ffd700";
string css = $$"""
.highlight {
  background-color: {{highlight}};
}
""";
```

- `{{variable}}` will be replaced by the variable’s value.
- Single `{` or `}` are literal curly braces.

For more on dynamic HTML and PDF workflows, see [How do I convert HTML to PDF in C#?](html-to-pdf-csharp.md).

---

## What Are Some Practical Use Cases for Multiline Strings in C#?

Let’s see how these string types show up in real code bases.

### How Do I Write Clean, Safe SQL Queries?

Poorly formatted SQL is hard to read and easy to break. Verbatim strings make queries readable, and parameterization keeps them safe.

```csharp
using System.Data.SqlClient;

string sql = @"
SELECT Name, Email
FROM Users
WHERE Status = @status
  AND Role = @role
";
using (var cmd = new SqlCommand(sql, connection))
{
    cmd.Parameters.AddWithValue("@status", "Active");
    cmd.Parameters.AddWithValue("@role", "Admin");
    // Execute command...
}
```

Never interpolate user input directly—always use parameters!

### How Can I Build JSON or Config Templates Easily?

Raw string literals ($"""...""") are perfect for JSON or config data:

```csharp
string apiKey = "abc123";
string config = $"""
{
  "apiKey": "{apiKey}",
  "timeout": 30
}
""";
```

No escaping needed. Your code looks just like your output.

### How Do I Use Multiline HTML Templates for PDF Generation?

If you’re generating PDFs from HTML (especially with [IronPDF](https://ironpdf.com)), multiline strings keep your templates readable and easy to maintain.

```csharp
using IronPdf; // Install-Package IronPdf

string customer = "Grace";
decimal total = 987.65m;
string html = $@"
<html>
  <body>
    <h1>Invoice</h1>
    <p>Customer: {customer}</p>
    <p>Total: ${total:F2}</p>
  </body>
</html>
";

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf(html);
pdf.SaveAs("Invoice.pdf");
```

For more on manipulating PDFs and their DOM, take a look at [How do I access the PDF DOM object in C#?](access-pdf-dom-object-csharp.md).

### What About CSS, JavaScript, or Other Curly-Brace Formats?

Raw string literals with interpolation are a lifesaver for CSS or JS snippets:

```csharp
string color = "#4caf50";
string css = $$"""
.button {
  background-color: {{color}};
  border-radius: 5px;
}
""";
```

No more escape headaches or ugly concatenation.

### How Do I Deal with File Paths Without Escaping Everything?

Verbatim strings (`@""`) are your go-to for Windows paths:

```csharp
string logPath = @"C:\Logs\app.log";
```

If you’re building the path dynamically, combine interpolation and verbatim:

```csharp
string baseDir = @"C:\Data";
string file = "output.txt";
string fullPath = $@"{baseDir}\{file}";
```

If you’re dealing with attachments or file operations in PDFs, see [How do I add attachments to PDFs in C#?](add-attachments-pdf-csharp.md).

### How Can I Use Large Templates Without Cluttering My Code?

For really long templates (like multi-page HTML emails or big PDF layouts), embed them as resources:

1. **Add your file** (e.g., `Template.html`) to your project.
2. **Set Build Action** to “Embedded Resource.”
3. **Load it at runtime:**

```csharp
using System.Reflection;
using System.IO;

var assembly = Assembly.GetExecutingAssembly();
var resource = "MyAppName.Template.html";

using (var stream = assembly.GetManifestResourceStream(resource))
using (var reader = new StreamReader(stream))
{
    string template = reader.ReadToEnd();
    // Replace placeholders as needed
    string output = template.Replace("{{UserName}}", "Maxine");
}
```

For smaller, dynamic templates, stick to inline multiline strings.

---

## How Do I Handle Line Breaks, Whitespace, or Split Long Strings?

### How Can I Flatten a Multiline String into a Single Line?

If you need to log or process a multiline block as one line, just replace line breaks:

```csharp
string multiline = @"
First line
Second line
Third line
";
string singleLine = multiline.Replace(Environment.NewLine, " ");
Console.WriteLine(singleLine);
// Output: First line Second line Third line
```

For a more robust way (removing all whitespace lines):

```csharp
string normalized = string.Join(" ",
    multiline.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
);
```

### How Do I Split Long Strings Across Code Lines for Readability?

If your string is long but should output as a single line, concatenate:

```csharp
string message = "This is a really long error message that " +
                 "needs to wrap in code, but not in output.";
Console.WriteLine(message);
```

Alternatively, use verbatim or raw strings to control visible line breaks.

---

## What Common Pitfalls Should I Watch Out For Using Multiline Strings in C#?

- **Using regular strings for Windows paths:** You’ll get unexpected results with `\n`, `\t`, etc. Use `@`.
- **Forgetting to double up quotes in verbatim strings:** `@"He said, "Hi""` is invalid. Use `@"He said, ""Hi"""`.
- **Wrong resource name for embedded files:** Use `assembly.GetManifestResourceNames()` to check actual names.
- **Mixing `$@""` vs `@$""`:** Both work, but `$@""` is more common.
- **Interpolating user input into SQL:** Always parameterize to prevent SQL injection.
- **Indentation issues in raw string literals:** Closing triple quotes determine what's stripped. Watch your alignment!
- **Trying to use raw strings on older C#:** Raw string literals require C# 11 or higher.

For more language comparisons, see [How does C# compare to Python for text processing?](compare-csharp-to-python.md).

---

## When Should I Use Each String Syntax in C#? (Quick Reference)

| Syntax             | When to Use                             | Example                                                |
|--------------------|-----------------------------------------|--------------------------------------------------------|
| `@"..."`           | Verbatim (newlines, paths, SQL)         | File paths, readable SQL queries                       |
| `$@"...{var}..."`  | Verbatim + interpolation                | Dynamic SQL or inline HTML templates                   |
| `"""..."""`        | Raw (C# 11+, no escapes)                | JSON, HTML, big XML blocks                             |
| `$"""...{var}..."""` | Raw + interpolation                   | CSS, JavaScript, dynamic HTML                          |
| `"a" + "b"`        | Concatenation (single line output)      | Wrapping long strings in code, single-line output      |

**General rule:**  
- Use `@` for paths, SQL, or multiline code.
- Use `"""` when you want code to look like output (JSON, HTML).
- Combine `$` for variable interpolation.
- Use embedded resources for really large templates.

If you’re working with HTML to PDF, IronPDF from [Iron Software](https://ironsoftware.com) is a top pick—see more at [IronPDF](https://ironpdf.com).

---

## What’s the Bottom Line for Multiline Strings in C#?

Leveraging C#’s multiline string features makes your code easier to read, debug, and maintain.  
- Use verbatim strings for most file paths, SQL, and any multiline text.
- Switch to raw string literals for escape-heavy templates (like HTML/JSON) if you’re on C# 11 or newer.
- Add `$` for variable interpolation—but always parameterize queries.
- Store giant templates as embedded resources to keep code clean.

For more real-world, dynamic PDF and HTML scenarios, check out the related guides on [HTML string to PDF](html-string-to-pdf-csharp.md), [PDF DOM access](access-pdf-dom-object-csharp.md), and [adding attachments to PDFs](add-attachments-pdf-csharp.md).

---

*This FAQ was written by [Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/), CTO at Iron Software. Got a question that's not covered here? Drop it in the comments.*
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

[Jacob Mellor](https://ironsoftware.com/about-us/authors/jacobmellor/) is the Founding Engineer of Iron Software, where he leads a 50+ person team building .NET libraries with over 41+ million NuGet downloads. With 41 years of coding experience, Jacob focuses on developer experience and cross-platform solutions. Based in Chiang Mai, Thailand, connect with Jacob on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
