// NuGet: Install-Package PdfOxide
using PdfOxide.Core;
using System;

class Program
{
    static void Main()
    {
        // Convert a PDF to clean Markdown (useful for RAG / LLM pipelines).
        using var doc = PdfDocument.Open("input.pdf");

        // Single page (0-based) ...
        string firstPage = doc.ToMarkdown(0);
        Console.WriteLine(firstPage);

        // ... or the whole document at once.
        string fullMarkdown = doc.ToMarkdownAll();
        Console.WriteLine(fullMarkdown);
    }
}
