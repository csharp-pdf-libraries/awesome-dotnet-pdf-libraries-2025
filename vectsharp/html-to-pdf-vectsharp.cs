// NuGet: Install-Package VectSharp.PDF
using VectSharp;
using VectSharp.PDF;
using VectSharp.SVG;
using System.IO;

class Program
{
    static void Main()
    {
        // VectSharp doesn't directly support HTML to PDF
        // It requires manual creation of graphics objects
        Document doc = new Document();
        Page page = new Page(595, 842); // A4 size
        Graphics graphics = page.Graphics;
        
        graphics.FillText(100, 100, "Hello from VectSharp", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 24));
        
        doc.Pages.Add(page);
        doc.SaveAsPDF("output.pdf");
    }
}