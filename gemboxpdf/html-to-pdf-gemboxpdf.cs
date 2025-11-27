// NuGet: Install-Package GemBox.Pdf
using GemBox.Pdf;
using GemBox.Pdf.Content;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");
        
        var document = PdfDocument.Load("input.html");
        document.Save("output.pdf");
    }
}