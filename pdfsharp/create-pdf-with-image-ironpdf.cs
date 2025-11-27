// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        // Create PDF from HTML with image
        var renderer = new ChromePdfRenderer();
        
        string html = @"
            <h1>Image in PDF</h1>
            <img src='image.jpg' style='width:200px; height:200px;' />
            <p>Easy image embedding with HTML</p>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
        
        // Alternative: Add image to existing PDF
        var existingPdf = new ChromePdfRenderer().RenderHtmlAsPdf("<h1>Document</h1>");
        var imageStamper = new IronPdf.Editing.ImageStamper(new Uri("image.jpg"))
        {
            VerticalAlignment = IronPdf.Editing.VerticalAlignment.Top
        };
        existingPdf.ApplyStamp(imageStamper);
    }
}