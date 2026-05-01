// NuGet: Install-Package GdPicture
// Nutrient .NET SDK is built on GdPicture.NET. Merge is exposed through
// GdPictureDocumentConverter.CombineToPDF (the modern path) or the older
// GdPicturePDF.MergeDocuments overloads.
using GdPicture14;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        IEnumerable<string> sourceFiles = new List<string>
        {
            "document1.pdf",
            "document2.pdf"
        };

        using var converter = new GdPictureDocumentConverter();
        converter.CombineToPDF(sourceFiles, "merged.pdf", PdfConformance.PDF);
    }
}
