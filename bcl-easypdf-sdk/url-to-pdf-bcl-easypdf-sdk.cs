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
        var pdf = new PDFDocument();
        var htmlConverter = new HTMLConverter();
        htmlConverter.ConvertURL("https://example.com", pdf);
        pdf.Save("webpage.pdf");
        pdf.Close();
    }
}