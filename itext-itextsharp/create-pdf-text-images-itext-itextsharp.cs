// NuGet: Install-Package itext7
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;

class Program
{
    static void Main()
    {
        string outputPath = "document.pdf";
        
        using (PdfWriter writer = new PdfWriter(outputPath))
        using (PdfDocument pdf = new PdfDocument(writer))
        using (Document document = new Document(pdf))
        {
            document.Add(new Paragraph("Sample PDF Document"));
            document.Add(new Paragraph("This document contains text and an image."));
            
            Image img = new Image(ImageDataFactory.Create("image.jpg"));
            img.SetWidth(200);
            document.Add(img);
        }
    }
}