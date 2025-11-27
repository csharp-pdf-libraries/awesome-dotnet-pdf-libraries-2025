// NuGet: Install-Package PdfiumViewer
using PdfiumViewer;
using System;
using System.Drawing;
using System.Drawing.Imaging;

string pdfPath = "document.pdf";
string outputImage = "page1.png";

// PDFiumViewer excels at rendering PDFs to images
using (var document = PdfDocument.Load(pdfPath))
{
    // Render first page at 300 DPI
    int dpi = 300;
    using (var image = document.Render(0, dpi, dpi, true))
    {
        // Save as PNG
        image.Save(outputImage, ImageFormat.Png);
        Console.WriteLine($"Page rendered to {outputImage}");
    }
    
    // Render all pages
    for (int i = 0; i < document.PageCount; i++)
    {
        using (var pageImage = document.Render(i, 150, 150, true))
        {
            pageImage.Save($"page_{i + 1}.png", ImageFormat.Png);
        }
    }
}