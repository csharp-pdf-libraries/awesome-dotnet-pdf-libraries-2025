// NuGet: Install-Package TallComponents.PDFKit5
// (real package name is TallComponents.PDFKit5 — current 5.0.216, targets
//  .NET Standard 2.0. The 4.x line ships as TallComponents.PDFKit.)
// Note: TallComponents was acquired by Apryse (May 27, 2025) and no longer
// sells new licenses. Existing customers can still use the package; Apryse
// directs new buyers to the iText SDK.
using TallComponents.PDF;
using System.IO;

class Program
{
    static void Main()
    {
        // Create the target document
        Document outputDoc = new Document();

        // Load first PDF and clone every page into the target
        using (FileStream fs1 = new FileStream("document1.pdf", FileMode.Open, FileAccess.Read))
        {
            Document doc1 = new Document(fs1);
            foreach (Page page in doc1.Pages)
            {
                // Clone is required when moving pages between documents
                outputDoc.Pages.Add(page.Clone());
            }
        }

        // Load second PDF and append the whole page collection
        using (FileStream fs2 = new FileStream("document2.pdf", FileMode.Open, FileAccess.Read))
        {
            Document doc2 = new Document(fs2);
            outputDoc.Pages.AddRange(doc2.Pages.CloneToArray());
        }

        // Save merged document
        using (FileStream output = new FileStream("merged.pdf", FileMode.Create))
        {
            outputDoc.Write(output);
        }
    }
}
