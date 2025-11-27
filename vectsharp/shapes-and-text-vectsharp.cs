// NuGet: Install-Package VectSharp.PDF
using VectSharp;
using VectSharp.PDF;
using System;

class Program
{
    static void Main()
    {
        Document doc = new Document();
        Page page = new Page(595, 842);
        Graphics graphics = page.Graphics;
        
        // Draw rectangle
        graphics.FillRectangle(50, 50, 200, 100, Colour.FromRgb(0, 0, 255));
        
        // Draw circle
        GraphicsPath circle = new GraphicsPath();
        circle.Arc(400, 100, 50, 0, 2 * Math.PI);
        graphics.FillPath(circle, Colour.FromRgb(255, 0, 0));
        
        // Add text
        graphics.FillText(50, 200, "VectSharp Graphics", 
            new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 20));
        
        doc.Pages.Add(page);
        doc.SaveAsPDF("shapes.pdf");
    }
}