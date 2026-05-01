// NuGet: Install-Package GdPicture
// Nutrient .NET SDK is built on GdPicture.NET (Nutrient acquired ORPALIS/GdPicture and
// rebranded from PSPDFKit to Nutrient on 2024-10-23). Requires Chrome or Edge installed
// on the host for HTML rendering. Pricing is sales-led; contact nutrient.io/sdk/pricing.
using GdPicture14;

class Program
{
    static void Main()
    {
        // Write the HTML to a file so the converter can ingest it.
        var htmlContent = "<html><body><h1>Hello World</h1></body></html>";
        System.IO.File.WriteAllText("input.html", htmlContent);

        using var converter = new GdPictureDocumentConverter();
        converter.LoadFromFile("input.html", DocumentFormat.DocumentFormatHTML);
        converter.SaveAsPDF("output.pdf");
    }
}
