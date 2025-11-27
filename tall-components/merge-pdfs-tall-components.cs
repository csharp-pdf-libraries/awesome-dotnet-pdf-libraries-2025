// NuGet: Install-Package TallComponents.PDF.Kit
using TallComponents.PDF.Kit;
using System.IO;

class Program
{
    static void Main()
    {
        // Create output document
        using (Document outputDoc = new Document())
        {
            // Load first PDF
            using (FileStream fs1 = new FileStream("document1.pdf", FileMode.Open))
            using (Document doc1 = new Document(fs1))
            {
                foreach (Page page in doc1.Pages)
                {
                    outputDoc.Pages.Add(page.Clone());
                }
            }
            
            // Load second PDF
            using (FileStream fs2 = new FileStream("document2.pdf", FileMode.Open))
            using (Document doc2 = new Document(fs2))
            {
                foreach (Page page in doc2.Pages)
                {
                    outputDoc.Pages.Add(page.Clone());
                }
            }
            
            // Save merged document
            using (FileStream output = new FileStream("merged.pdf", FileMode.Create))
            {
                outputDoc.Write(output);
            }
        }
    }
}