// NuGet: Install-Package PdfiumViewer
//        Install-Package PdfiumViewer.Native.x86.v8-xfa
//        Install-Package PdfiumViewer.Native.x86_64.v8-xfa
// Repo:  https://github.com/pvginkel/PdfiumViewer (archived 2019-08-02)
using PdfiumViewer;
using System;
using System.Drawing;
using System.Drawing.Imaging;

string pdfPath = "document.pdf";
string outputImage = "page1.png";

// Rendering pages to System.Drawing.Image is what PdfiumViewer is built
// for. Note the dependency on System.Drawing.Common — Windows-only on
// modern .NET (6+).
using (var document = PdfDocument.Load(pdfPath))
{
    // PdfDocument.Render(int page, float dpiX, float dpiY, bool forPrinting)
    int dpi = 300;
    using (var image = document.Render(0, dpi, dpi, true))
    {
        image.Save(outputImage, ImageFormat.Png);
        Console.WriteLine($"Page rendered to {outputImage}");
    }

    for (int i = 0; i < document.PageCount; i++)
    {
        using (var pageImage = document.Render(i, 150, 150, true))
        {
            pageImage.Save($"page_{i + 1}.png", ImageFormat.Png);
        }
    }
}
