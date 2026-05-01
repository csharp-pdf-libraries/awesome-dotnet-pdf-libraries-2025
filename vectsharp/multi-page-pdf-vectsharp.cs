// NuGet: Install-Package VectSharp.PDF
using VectSharp;
using VectSharp.PDF;

class Program
{
    static void Main()
    {
        Document doc = new Document();

        FontFamily helvetica =
            FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica);
        Font titleFont = new Font(helvetica, 24);
        Font bodyFont = new Font(helvetica, 14);

        // Page 1
        Page page1 = new Page(595, 842);
        Graphics g1 = page1.Graphics;
        g1.FillText(50, 50, "Page 1", titleFont, Colours.Black);
        g1.FillText(50, 100, "First page content", bodyFont, Colours.Black);
        doc.Pages.Add(page1);

        // Page 2
        Page page2 = new Page(595, 842);
        Graphics g2 = page2.Graphics;
        g2.FillText(50, 50, "Page 2", titleFont, Colours.Black);
        g2.FillText(50, 100, "Second page content", bodyFont, Colours.Black);
        doc.Pages.Add(page2);

        doc.SaveAsPDF("multipage.pdf");
    }
}