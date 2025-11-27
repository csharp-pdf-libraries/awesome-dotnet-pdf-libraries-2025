// NuGet: Install-Package IronPdf
using IronPdf;
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        var pdf = PdfDocument.FromFile("document.pdf");
        
        // Extract information
        Console.WriteLine($"Page Count: {pdf.PageCount}");
        
        // IronPDF can manipulate and save, then open with default viewer
        pdf.SaveAs("modified.pdf");
        
        // Open with default PDF viewer
        Process.Start(new ProcessStartInfo("modified.pdf") { UseShellExecute = true });
    }
}