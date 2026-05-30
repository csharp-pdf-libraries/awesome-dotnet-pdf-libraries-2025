// NuGet: Install-Package PdfOxide
using PdfOxide.Core;
using System;
using System.Text;

class Program
{
    static void Main()
    {
        // PdfOxide is powered by a pure-Rust core via P/Invoke.
        using var doc = PdfDocument.Open("input.pdf");

        var text = new StringBuilder();
        for (int page = 0; page < doc.PageCount; page++)
        {
            text.AppendLine(doc.ExtractText(page)); // 0-based page index
        }

        Console.WriteLine(text.ToString());
    }
}
