// NuGet: Install-Package GemBox.Pdf
using GemBox.Pdf;
using GemBox.Pdf.Content;

class Program
{
    static void Main()
    {
        ComponentInfo.SetLicense("FREE-LIMITED-KEY");
        
        using (var document = new PdfDocument())
        {
            var page = document.Pages.Add();
            var formattedText = new PdfFormattedText()
            {
                Text = "Hello World",
                FontSize = 24
            };
            
            page.Content.DrawText(formattedText, new PdfPoint(100, 700));
            document.Save("output.pdf");
        }
    }
}