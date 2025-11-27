// NuGet: Install-Package TallComponents.PDF.Kit
using TallComponents.PDF.Kit;
using System.IO;

class Program
{
    static void Main()
    {
        // Create a new document
        using (Document document = new Document())
        {
            string html = "<html><body><h1>Hello World</h1><p>This is a PDF from HTML.</p></body></html>";
            
            // Create HTML fragment
            Fragment fragment = Fragment.FromText(html);
            
            // Add to document
            Section section = document.Sections.Add();
            section.Fragments.Add(fragment);
            
            // Save to file
            using (FileStream fs = new FileStream("output.pdf", FileMode.Create))
            {
                document.Write(fs);
            }
        }
    }
}