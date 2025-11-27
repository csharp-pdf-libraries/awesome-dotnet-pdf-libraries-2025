// NuGet: Install-Package VectSharp.PDF
using VectSharp;
using VectSharp.PDF;
using System;

class Program
{
    static void Main()
    {
        Document doc = new Document();
        
        // Page 1
        Page page1 = new Page(595, 842);
        Graphics g1 = page1.Graphics;
        g1.FillText(50, 50, "Page 1", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 24));
        g1.FillText(50, 100, "First page content", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 14));
        doc.Pages.Add(page1);
        
        // Page 2
        Page page2 = new Page(595, 842);
        Graphics g2 = page2.Graphics;
        g2.FillText(50, 50, "Page 2", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 24));
        g2.FillText(50, 100, "Second page content", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 14));
        doc.Pages.Add(page2);
        
        doc.SaveAsPDF("multipage.pdf");
    }
}