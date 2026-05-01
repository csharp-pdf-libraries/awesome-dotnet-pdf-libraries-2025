// NuGet: Install-Package GemBox.Pdf
// Free version: 2-page limit (FreeLimitReachedException beyond that).
// Source: https://www.gemboxsoftware.com/pdf/free-version
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

            // PdfFormattedText has no Text property; use Append/AppendLine.
            var formattedText = new PdfFormattedText();
            formattedText.Append("Hello World");

            page.Content.DrawText(formattedText, new PdfPoint(100, 700));
            document.Save("output.pdf");
        }
    }
}