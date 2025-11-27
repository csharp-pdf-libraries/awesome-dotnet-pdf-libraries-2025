// NuGet: Install-Package IronPdf
using IronPdf;
using IronPdf.Rendering;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        string html = @"
            <div style='width: 200px; height: 100px; background-color: blue; margin: 50px;'></div>
            <div style='width: 100px; height: 100px; background-color: red; 
                        border-radius: 50%; margin-left: 350px; margin-top: -50px;'></div>
            <h2 style='margin-left: 50px;'>IronPDF Graphics</h2>
        ";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("shapes.pdf");
    }
}