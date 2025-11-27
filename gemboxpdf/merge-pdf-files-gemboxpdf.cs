// NuGet: Install-Package GemBox.Pdf
using GemBox.Pdf;
using System.Linq;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");
        
        using (var document = new PdfDocument())
        {
            var source1 = PdfDocument.Load("document1.pdf");
            var source2 = PdfDocument.Load("document2.pdf");
            
            document.Pages.AddClone(source1.Pages);
            document.Pages.AddClone(source2.Pages);
            
            document.Save("merged.pdf");
        }
    }
}