# How Do I Implement CQRS with MediatR in C# for Real-World Document and PDF Workflows?

CQRS (Command Query Responsibility Segregation) is a powerful architectural pattern for separating read and write operations—especially when your app’s reporting, document management, or PDF generation requirements start to grow. In this FAQ, you’ll learn how to actually implement CQRS using MediatR in C#, where it shines (and where it doesn’t), and how to apply it to practical scenarios like PDF creation. We’ll walk through hands-on code, share best practices, and flag key pitfalls.

For related deep-dives, see [How do I create PDFs in C#?](create-pdf-csharp-complete-guide.md), [How can I access the PDF DOM in C#?](access-pdf-dom-object-csharp.md), and [How do I add attachments to PDFs in C#?](add-attachments-pdf-csharp.md).

---

## When Should I Use CQRS in a C# Project? What Are Its Real Advantages?

CQRS is great when your application needs to handle heavy or complex operations differently for reads and writes. For example, if your reporting dashboard is slow because it’s loading complex data structures, or if your PDF export processes are tightly coupled with your API logic, CQRS can help you scale and streamline these paths independently.

**Use CQRS when:**
- You need to optimize reads (e.g. dashboards, reports) and writes (e.g. data entry, PDF generation) separately.
- Your write logic is complex—think multi-step workflows, validation, or eventing.
- Your read requirements involve denormalized or highly-optimized models.

**Skip CQRS for:**
- Simple CRUD applications with similar logic for both reads and writes.
- Low-traffic apps where scaling isn’t a concern.
- Projects where your team is new to message-based or event-driven thinking—there’s a learning curve.

Want more tips on CQRS fit? See the [complete CQRS pattern guide](https://ironpdf covers event sourcing, MediatR pipelines, and more advanced scenarios.).

---

## How Does CQRS Work with MediatR in C#? What’s the High-Level Approach?

With MediatR, you treat each command (write) and query (read) as a distinct message. Handler classes process these messages, keeping your controllers thin and your business logic organized.

**Commands** represent actions that change state (like generating a PDF or creating a record).
**Queries** fetch data (like retrieving a list of invoices).

For example:
- A “Generate Invoice PDF” request is a command—it triggers business logic and produces a new file.
- A “List Invoices” query is a read operation, returning DTOs for display.

This separation helps you scale each independently and keep your codebase much more maintainable.

---

## How Do I Set Up CQRS and MediatR in an ASP.NET Core Project?

You’ll need to install the MediatR NuGet package. For PDF workflows, you’ll also want IronPDF.

```bash
dotnet add package MediatR
dotnet add package IronPdf // For advanced PDF generation
```

**Register MediatR and services:**

```csharp
using IronPdf; // NuGet: IronPdf
using MediatR;

// In your Program.cs or equivalent setup file
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register ChromePdfRenderer as a singleton for best performance
builder.Services.AddSingleton<ChromePdfRenderer>();

// Register your repositories
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

var app = builder.Build();
```

### Why Use Singleton for PDF Rendering Engines?

PDF engines like `ChromePdfRenderer` can be resource-intensive to initialize. Registering them as a singleton means you only pay the startup cost once, and they’re thread-safe for concurrent requests.

### What Kind of Models Should I Use in CQRS?

- **Domain Models:** Used for commands (writes), including validation and business logic.
- **DTOs/View Models:** Used for queries (reads), flattened and ready for serialization.

For advanced PDF features, check out [How do I access the PDF DOM in C#?](access-pdf-dom-object-csharp.md) and [How do I render WebGL sites to PDF in C#?](render-webgl-pdf-csharp.md).

---

## How Do I Write and Handle Commands in CQRS with MediatR?

Let’s walk through creating an invoice as a typical command.

**Define the command:**

```csharp
using MediatR;

public record CreateInvoiceCommand(string Customer, decimal Amount) : IRequest<int>;
```

Here, `record` keeps it immutable, and `IRequest<int>` means the handler returns the new invoice ID.

**Implement the handler:**

```csharp
using MediatR;

public class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, int>
{
    private readonly IInvoiceRepository _repo;

    public CreateInvoiceHandler(IInvoiceRepository repo) => _repo = repo;

    public async Task<int> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var invoice = new Invoice
        {
            Customer = command.Customer,
            Amount = command.Amount,
            CreatedUtc = DateTime.UtcNow
        };

        await _repo.AddAsync(invoice, cancellationToken);
        return invoice.Id;
    }
}
```

**Dispatch in your controller:**

```csharp
using Microsoft.AspNetCore.Mvc;
using MediatR;

[ApiController]
[Route("invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    // ... other endpoints
}
```

Notice that controllers remain slim—just dispatching commands, not containing business logic.

---

## How Do I Implement Queries for Fast Reads in CQRS?

Queries should be optimized for speed and return only what you need. Here’s how to set them up:

**Define a query and DTO:**

```csharp
using MediatR;
using System.Collections.Generic;

public record GetInvoicesQuery(int Page = 1, int PageSize = 50) : IRequest<IReadOnlyList<InvoiceDto>>;

public record InvoiceDto(int Id, string Customer, decimal Amount, DateTime CreatedUtc);
```

**Write the query handler:**

```csharp
using MediatR;

public class GetInvoicesHandler : IRequestHandler<GetInvoicesQuery, IReadOnlyList<InvoiceDto>>
{
    private readonly IInvoiceRepository _repo;

    public GetInvoicesHandler(IInvoiceRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<InvoiceDto>> Handle(GetInvoicesQuery query, CancellationToken cancellationToken)
    {
        var invoices = await _repo.GetPagedAsync(query.Page, query.PageSize, cancellationToken);

        return invoices.Select(i => new InvoiceDto(
            i.Id,
            i.Customer,
            i.Amount,
            i.CreatedUtc
        )).ToList();
    }
}
```

**Controller endpoint:**

```csharp
[HttpGet]
public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int size = 50)
{
    var query = new GetInvoicesQuery(page, size);
    var invoices = await _mediator.Send(query);
    return Ok(invoices);
}
```

**Why use DTOs for queries?**
DTOs keep your domain logic clean and allow you to tailor your output for the UI or API consumers. If you ever switch to a dedicated read store (like Elasticsearch), you only update the handler.

For more on customizing PDF outputs, check [How can I add attachments to PDFs in C#?](add-attachments-pdf-csharp.md).

---

## How Can I Use CQRS and MediatR for PDF Generation in C#?

PDF generation is a classic “command” use case, perfect for CQRS. Let’s create a PDF from invoice data using IronPDF:

**1. Define the generate PDF command:**

```csharp
using MediatR;

public record GenerateInvoicePdfCommand(int InvoiceId) : IRequest<byte[]>;
```

**2. Write the command handler using IronPDF:**

```csharp
using MediatR;
using IronPdf; // NuGet: IronPdf

public class GenerateInvoicePdfHandler : IRequestHandler<GenerateInvoicePdfCommand, byte[]>
{
    private readonly IInvoiceRepository _repo;
    private readonly ChromePdfRenderer _renderer;

    public GenerateInvoicePdfHandler(IInvoiceRepository repo, ChromePdfRenderer renderer)
    {
        _repo = repo;
        _renderer = renderer;
    }

    public async Task<byte[]> Handle(GenerateInvoicePdfCommand command, CancellationToken cancellationToken)
    {
        var invoice = await _repo.GetByIdAsync(command.InvoiceId, cancellationToken) 
                      ?? throw new Exception("Invoice not found");

        var htmlContent = $@"
            <h1>Invoice #{invoice.Id}</h1>
            <p>Customer: {invoice.Customer}</p>
            <p>Amount: {invoice.Amount:C}</p>
            <p>Date: {invoice.CreatedUtc:yyyy-MM-dd}</p>";

        var pdfDoc = _renderer.RenderHtmlAsPdf(htmlContent);

        return pdfDoc.BinaryData;
    }
}
```

**3. Return the generated PDF from your controller:**

```csharp
[HttpPost("{id}/pdf")]
public async Task<IActionResult> GeneratePdf(int id)
{
    var pdfBytes = await _mediator.Send(new GenerateInvoicePdfCommand(id));
    return File(pdfBytes, "application/pdf", $"invoice-{id}.pdf");
}
```

**Bonus: How Do I Store Generated PDFs in Blob Storage?**

Extend your handler to upload the PDF and return a download URL:

```csharp
public class GenerateInvoicePdfHandler : IRequestHandler<GenerateInvoicePdfCommand, string>
{
    // ...dependencies

    public async Task<string> Handle(GenerateInvoicePdfCommand command, CancellationToken cancellationToken)
    {
        // ...generate PDF as above

        var fileName = $"invoices/invoice-{invoice.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var url = await _storage.UploadAsync(fileName, pdfDoc.BinaryData, cancellationToken);

        return url;
    }
}
```

For full PDF creation options, see [How do I create PDFs in C#?](create-pdf-csharp-complete-guide.md).

---

## What Are Common CQRS Pitfalls in C# and How Can I Avoid Them?

**1. Over-architecting Simple Scenarios**

If your app is basic CRUD, CQRS can add unneeded complexity. Only split reads and writes where it’s worth the effort.

**2. Missing Validation in Commands**

Business validation belongs in handlers, not controllers. Use MediatR pipeline behaviors or FluentValidation for reusable validation.

**Validation example:**

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

**3. Fat Controllers, Thin Handlers**

If you’re moving business logic into controllers, you’re missing the CQRS point. Handlers should do the heavy lifting.

**4. Eventual Consistency Surprises**

If you scale out with separate read/write databases, you may face delays seeing new data. Stick with a single database or synchronize updates if consistency is critical.

**5. Handler Discovery Issues**

If MediatR isn’t finding your handlers, use `RegisterServicesFromAssembly` and double-check your handler namespaces and types.

**6. Performance Trouble With Large Aggregates**

Don’t fetch heavy domain objects for simple reads. Keep query handlers lean by projecting only needed fields.

**7. Not Using Cancellation Tokens**

Pass the `CancellationToken` all the way—especially important for long-running operations like PDF rendering or exports.

---

## Can You Summarize the CQRS Pattern in C#?

Here’s a quick reference for CQRS with MediatR and IronPDF:

| Concept        | Example Implementation                            | Notes                        |
|----------------|---------------------------------------------------|------------------------------|
| Command        | `record CreateInvoiceCommand(...) : IRequest<int>` | Changes state, returns value |
| Query          | `record GetInvoicesQuery(...) : IRequest<List<Dto>>` | Data fetch, no side effects  |
| Handler        | `IRequestHandler<TReq, TRes>`                     | Logic for each message       |
| Registration   | `AddMediatR(cfg => cfg.RegisterServices...)`      | In DI setup                  |
| Controller Use | `await _mediator.Send(command)`                   | Controllers just dispatch    |
| Validation     | Pipeline behaviors / FluentValidation             | Centralized, reusable        |
| PDF Renderer   | `ChromePdfRenderer` (IronPDF)                     | Advanced HTML to PDF         |

Keep handlers focused, controllers slim, and DTOs optimized for reads. For more on IronPDF, visit [IronPDF’s documentation](https://ironpdf.com/blog/using-ironpdf/dotnet-core-generate-pdf/).

---

## When Should I Choose CQRS (and When Should I Avoid It) in C#?

CQRS is best for applications with complex business domains, heavy reporting, or lots of document processing—especially when you need to scale reads and writes independently. It’s ideal for workflows like PDF generation, exporting data, or building live dashboards. However, for simple CRUD apps or small projects, CQRS can be unnecessary overhead.

For more advanced scenarios, read the [complete CQRS pattern guide](https://ironpdf covers event sourcing, MediatR pipelines, and more advanced scenarios.).

If you want to learn more about working with PDFs in .NET, see [How do I create PDFs in C#?](create-pdf-csharp-complete-guide.md), [How can I work with the PDF DOM in C#?](access-pdf-dom-object-csharp.md), or [How do I add attachments to PDFs in C#?](add-attachments-pdf-csharp.md). You can also explore [IronPDF for macOS C#](ironpdf-macos-csharp.md) if you’re developing on a Mac.

For more on IronPDF and .NET document solutions, check out [IronPDF](https://ironpdf.com) and [Iron Software](https://ironsoftware.com).

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

[Jacob Mellor](https://www.linkedin.com/in/jacob-mellor-iron-software/), CTO of Iron Software, brings 41 years of coding expertise to PDF technology. As creator of IronPDF (10+ million+ downloads), he leads innovation in HTML-to-PDF conversion and document processing. First software business opened in London in 1999. Connect: [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/)
