// NuGet: Install-Package Ghostscript.NET
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

class GhostscriptExample
{
    static void Main()
    {
        string inputPdf = "input.pdf";
        string outputPath = "output";
        
        GhostscriptVersionInfo gvi = new GhostscriptVersionInfo("gsdll64.dll");
        
        using (GhostscriptRasterizer rasterizer = new GhostscriptRasterizer())
        {
            rasterizer.Open(inputPdf, gvi, false);
            
            for (int pageNumber = 1; pageNumber <= rasterizer.PageCount; pageNumber++)
            {
                Image img = rasterizer.GetPage(300, pageNumber);
                img.Save($"{outputPath}_page{pageNumber}.png", ImageFormat.Png);
                img.Dispose();
            }
        }
    }
}