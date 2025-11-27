// NuGet: Install-Package IronPdf
using IronPdf;
using System.IO;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        string imageBase64 = Convert.ToBase64String(File.ReadAllBytes("image.jpg"));
        string html = $@"
            <html>
                <body>
                    <h1>Sample PDF Document</h1>
                    <img src='data:image/jpeg;base64,{imageBase64}' width='200' height='150' />
                </body>
            </html>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("output.pdf");
    }
}