// NuGet: Install-Package GdPicture.NET
using GdPicture14;
using System;

class Program
{
    static void Main()
    {
        using (GdPictureDocumentConverter converter = new GdPictureDocumentConverter())
        {
            string htmlContent = "<html><body><h1>Hello World</h1></body></html>";
            GdPictureStatus status = converter.LoadFromHTMLString(htmlContent);
            
            if (status == GdPictureStatus.OK)
            {
                converter.SaveAsPDF("output.pdf");
            }
        }
    }
}