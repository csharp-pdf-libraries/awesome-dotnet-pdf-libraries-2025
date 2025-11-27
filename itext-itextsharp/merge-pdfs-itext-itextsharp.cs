// NuGet: Install-Package itext7
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System.IO;

class Program
{
    static void Main()
    {
        string outputPath = "merged.pdf";
        string[] inputFiles = { "document1.pdf", "document2.pdf", "document3.pdf" };
        
        using (PdfWriter writer = new PdfWriter(outputPath))
        using (PdfDocument pdfDoc = new PdfDocument(writer))
        {
            PdfMerger merger = new PdfMerger(pdfDoc);
            
            foreach (string file in inputFiles)
            {
                using (PdfDocument sourcePdf = new PdfDocument(new PdfReader(file)))
                {
                    merger.Merge(sourcePdf, 1, sourcePdf.GetNumberOfPages());
                }
            }
        }
    }
}