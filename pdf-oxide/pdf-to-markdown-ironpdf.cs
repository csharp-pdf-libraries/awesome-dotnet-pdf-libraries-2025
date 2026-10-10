// NuGet: Install-Package IronPdf
// IronPDF does not produce Markdown directly. The closest equivalent is
// plain-text extraction (or HTML export of a rendered page). For structured
// Markdown output, PdfOxide's ToMarkdown()/ToMarkdownAll() is purpose-built.
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var pdf = PdfDocument.FromFile("input.pdf");
        string text = pdf.ExtractAllText(); // plain text, not Markdown
        Console.WriteLine(text);
    }
}
