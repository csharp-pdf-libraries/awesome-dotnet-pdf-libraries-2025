// No NuGet package — easyPDF SDK is MSI-installed; reference
// BCL.easyPDF.PDFConverter.dll (.NET Framework) or
// BCL.easyPDF.PDFConverter.NetCore.dll (.NET Core).
// Vendor: now part of Apryse (acquired March 2020). Docs:
// https://www.pdfonline.com/easypdf/sdk/usermanual/
using BCL.easyPDF;
using System;

class Program
{
    static void Main()
    {
        var pdf = new PDFDocument();
        var htmlConverter = new HTMLConverter();
        htmlConverter.ConvertHTML("<h1>Hello World</h1>", pdf);
        pdf.Save("output.pdf");
        pdf.Close();
    }
}