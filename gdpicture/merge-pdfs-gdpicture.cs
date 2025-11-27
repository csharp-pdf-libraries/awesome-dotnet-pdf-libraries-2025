// NuGet: Install-Package GdPicture.NET
using GdPicture14;
using System;

class Program
{
    static void Main()
    {
        using (GdPicturePDF pdf1 = new GdPicturePDF())
        using (GdPicturePDF pdf2 = new GdPicturePDF())
        {
            pdf1.LoadFromFile("document1.pdf", false);
            pdf2.LoadFromFile("document2.pdf", false);
            
            pdf1.MergePages(pdf2);
            pdf1.SaveToFile("merged.pdf");
        }
    }
}