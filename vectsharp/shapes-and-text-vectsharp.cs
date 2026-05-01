// NuGet: Install-Package VectSharp.PDF
using System;
using VectSharp;
using VectSharp.PDF;

class Program
{
    static void Main()
    {
        Document doc = new Document();
        Page page = new Page(595, 842);
        Graphics graphics = page.Graphics;

        // Draw filled rectangle (blue)
        graphics.FillRectangle(50, 50, 200, 100, Colour.FromRgb(0, 0, 255));

        // Draw filled circle via GraphicsPath.Arc
        GraphicsPath circle = new GraphicsPath();
        circle.Arc(400, 100, 50, 0, 2 * Math.PI);
        graphics.FillPath(circle, Colour.FromRgb(255, 0, 0));

        // FillText requires the colour parameter.
        Font font = new Font(
            FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica),
            20);
        graphics.FillText(50, 200, "VectSharp Graphics", font, Colours.Black);

        doc.Pages.Add(page);
        doc.SaveAsPDF("shapes.pdf");
    }
}