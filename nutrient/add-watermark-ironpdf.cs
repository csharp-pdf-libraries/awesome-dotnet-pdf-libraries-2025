// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Editing;

class Program
{
    static void Main()
    {
        var pdf = PdfDocument.FromFile("document.pdf");
        
        pdf.ApplyWatermark("<h1 style='color:gray;opacity:0.5;'>CONFIDENTIAL</h1>",
            50,
            VerticalAlignment.Middle,
            HorizontalAlignment.Center);
        
        pdf.SaveAs("watermarked.pdf");
    }
}