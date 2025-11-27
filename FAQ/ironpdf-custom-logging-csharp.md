# How Can I Implement Custom Logging with IronPDF in C#?

IronPDF supports customizable logging so you can capture, route, and format library logs in a way that best fits your application and production needs. Whether you want to track issues in development, integrate with Serilog or Application Insights, or push structured logs to a database, IronPDF makes it straightforward to use your own logger.

---

## Why Should I Use Custom Logging with IronPDF?

Out of the box, IronPDF writes logs to the console, which is fine for basic scenarios. But when you need to debug production issues, correlate logs across distributed systems, or comply with observability requirements, you’ll want more control. With custom logging, you decide where IronPDF logs go—file, database, cloud, or any .NET logging framework. This flexibility is especially valuable if you’re running IronPDF in environments like Azure (see [IronPDF Azure deployment best practices](ironpdf-azure-deployment-csharp.md)) or if you’re integrating with existing logging pipelines.

---

## How Do I Plug In My Own Logger with IronPDF?

To send IronPDF logs to your own logger, set the logging mode to `Custom` and assign your logger instance. Here’s a quick example:

```csharp
// NuGet: Install-Package IronPdf
using IronPdf;
using Microsoft.Extensions.Logging; // NuGet: Microsoft.Extensions.Logging.Abstractions

IronSoftware.Logger.LoggingMode = IronSoftware.Logger.LoggingModes.Custom;
IronSoftware.Logger.CustomLogger = new MyCustomLogger();

var renderer = new ChromePdfRenderer();
var pdf = renderer.RenderHtmlAsPdf("<h1>Hello, custom logging!</h1>");
```

Make sure you both set `LoggingMode` and assign `CustomLogger`—missing either step means your logger won’t be used.

---

## What Does a Simple Custom ILogger Implementation Look Like?

A basic logger implements `Microsoft.Extensions.Logging.ILogger`. Here’s a minimal example you can build on:

```csharp
using Microsoft.Extensions.Logging;

public class SimpleConsoleLogger : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception exception, Func<TState, Exception, string> formatter)
    {
        Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
        if (exception != null)
            Console.WriteLine($"Exception: {exception}");
    }
}
```

All logs from IronPDF will now pass through your logger, where you can format, filter, or route them as needed.

---

## How Can I Use Serilog or Other Logging Frameworks with IronPDF?

IronPDF works seamlessly with major .NET loggers. For Serilog, wrap your Serilog logger with an adapter:

```csharp
// NuGet: Serilog, Serilog.Sinks.Console, Serilog.Sinks.File
using Serilog;
using Microsoft.Extensions.Logging;

public class SerilogLoggerAdapter : ILogger
{
    private readonly Serilog.ILogger _serilog;

    public SerilogLoggerAdapter(Serilog.ILogger serilog) => _serilog = serilog;
    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => _serilog.IsEnabled(ConvertLevel(logLevel));

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception exception, Func<TState, Exception, string> formatter)
    {
        var msg = formatter(state, exception);
        switch (logLevel)
        {
            case LogLevel.Information: _serilog.Information(msg); break;
            case LogLevel.Warning: _serilog.Warning(msg); break;
            case LogLevel.Error: _serilog.Error(exception, msg); break;
            default: _serilog.Debug(msg); break;
        }
    }

    private Serilog.Events.LogEventLevel ConvertLevel(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.Information => Serilog.Events.LogEventLevel.Information,
            LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            LogLevel.Error => Serilog.Events.LogEventLevel.Error,
            _ => Serilog.Events.LogEventLevel.Debug
        };
}

// Usage
var serilogLogger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
IronSoftware.Logger.CustomLogger = new SerilogLoggerAdapter(serilogLogger);
```

Now IronPDF logs will flow through your Serilog sinks—file, console, cloud, etc. If you’re deploying to cloud platforms, see [IronPDF Azure deployment tips](ironpdf-azure-deployment-csharp.md).

---

## Can I Send IronPDF Logs to Application Insights?

Absolutely. To integrate with Azure Application Insights, use the TelemetryClient in your custom logger:

```csharp
// NuGet: Microsoft.ApplicationInsights
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

public class AppInsightsLogger : ILogger
{
    private readonly TelemetryClient _telemetry;
    public AppInsightsLogger(TelemetryClient telemetry) => _telemetry = telemetry;
    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception exception, Func<TState, Exception, string> formatter)
    {
        var msg = formatter(state, exception);
        _telemetry.TrackTrace(msg);
        if (exception != null) _telemetry.TrackException(exception);
    }
}

// Usage
var telemetry = new TelemetryClient();
IronSoftware.Logger.CustomLogger = new AppInsightsLogger(telemetry);
```

For more on cloud deployment considerations, visit [IronPDF Azure deployment advice](ironpdf-azure-deployment-csharp.md).

---

## How Do I Filter or Enrich IronPDF Log Output?

You can filter logs by level or add context like request/user IDs. Here’s how to only log warnings and above:

```csharp
using Microsoft.Extensions.Logging;

public class WarningLogger : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        Console.WriteLine($"[{logLevel}] {formatter(state, exception)}");
    }
}
```

To add context information, wrap your logger and prepend data to each log entry, such as a request ID for web applications.

---

## Is It Possible to Log IronPDF Events to a Database?

Yes—write a custom logger that inserts records into your database. Here’s a basic SQL Server example:

```csharp
// NuGet: Microsoft.Data.SqlClient
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

public class SqlLogger : ILogger
{
    private readonly string _connStr;
    public SqlLogger(string connStr) => _connStr = connStr;
    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception exception, Func<TState, Exception, string> formatter)
    {
        using var conn = new SqlConnection(_connStr);
        conn.Open();
        var cmd = new SqlCommand("INSERT INTO IronPdfLogs (Level, Message) VALUES (@level, @msg)", conn);
        cmd.Parameters.AddWithValue("@level", logLevel.ToString());
        cmd.Parameters.AddWithValue("@msg", formatter(state, exception));
        cmd.ExecuteNonQuery();
    }
}
```

For high-volume scenarios, consider batching log writes to avoid performance issues.

---

## What Are Common Pitfalls with Custom Logging in IronPDF?

- **Forgetting LoggingMode.Custom**: Both `LoggingMode` and `CustomLogger` must be set.
- **Swallowing exceptions**: If your logger throws, you might lose logs—wrap file/database/network writes in try-catch.
- **Performance impact**: Avoid slow/busy loggers in production, especially for [PDF generation](https://ironpdf.com/java/how-to/java-fill-pdf-form-tutorial/) scenarios.
- **Not filtering**: Don’t log everything at all levels in production—set minimum log levels.
- **Global logger**: IronPDF uses a static logger, so coordinate if running in multi-tenant apps.

---

## How Do I Disable All Logging in IronPDF?

If you need maximum performance and no log output, simply set:

```csharp
IronSoftware.Logger.LoggingMode = IronSoftware.Logger.LoggingModes.None;
```

Logging is now completely turned off. Don’t forget to turn it back on for troubleshooting!

---

## Where Can I Learn More About IronPDF Logging and Related Features?

You can find more about HTML to PDF conversion in [How do I convert HTML to PDF with IronPDF in C#?](html-to-pdf-csharp-ironpdf.md), license key setup in [How do I use my IronPDF license key in C#?](ironpdf-license-key-csharp.md), or image import in [How can I convert images to PDF in C#?](image-to-pdf-csharp.md). For installation, see [How to install .NET 10](how-to-install-dotnet-10.md). Visit [IronPDF](https://ironpdf.com) or [Iron Software](https://ironsoftware.com) for official docs and examples. For advanced rendering, check out [ChromePdfRenderer](https://ironpdf.com/blog/videos/how-to-render-webgl-sites-to-pdf-in-csharp-ironpdf/).

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

Jacob Mellor (CTO, Iron Software) has spent 41 years solving document processing challenges. His philosophy: "You name it I'll learn it. I always do my best work in new programming languages when I don't know what isn't possible." Connect on [LinkedIn](https://www.linkedin.com/in/jacob-mellor-iron-software/).
