// NuGet: Install-Package MuPDF.NET
// MuPDF (and its MuPDF.NET / MuPDFCore wrappers) is a renderer/toolkit;
// it does NOT include an HTML-to-PDF engine. Artifex recommends rendering
// HTML upstream (browser, wkhtmltopdf, etc.) and using MuPDF only to
// view/manipulate the resulting PDF.
using MuPDF.NET;
using System;

class Program
{
    static void Main()
    {
        string html = "<html><body><h1>Hello World</h1></body></html>";

        // Not natively supported by MuPDF or its .NET wrappers.
        throw new NotSupportedException(
            "MuPDF does not support direct HTML to PDF conversion. " +
            "Render HTML with a browser engine first, then load the PDF here.");
    }
}
