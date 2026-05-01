// No NuGet package — easyPDF SDK is MSI-installed; reference
// BCL.easyPDF.PDFConverter.dll (.NET Framework) or
// BCL.easyPDF.PDFConverter.NetCore.dll (.NET Core).
// Vendor: now part of Apryse (acquired March 2020).
using BCL.easyPDF;
using System;

class Program
{
    static void Main()
    {
        var pdf1 = new PDFDocument("document1.pdf");
        var pdf2 = new PDFDocument("document2.pdf");
        pdf1.Append(pdf2);
        pdf1.Save("merged.pdf");
        pdf1.Close();
        pdf2.Close();
    }
}