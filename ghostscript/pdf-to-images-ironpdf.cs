// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class IronPdfExample
{
    static void Main()
    {
        var pdf = PdfDocument.FromFile("input.pdf");
        
        var images = pdf.ToBitmap();
        
        for (int i = 0; i < images.Length; i++)
        {
            images[i].Save($"output_page{i + 1}.png");
        }
    }
}