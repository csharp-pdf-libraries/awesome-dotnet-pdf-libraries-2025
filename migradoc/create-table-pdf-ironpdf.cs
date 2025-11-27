// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        string htmlTable = @"
            <table border='1'>
                <tr><th>Name</th><th>Age</th></tr>
                <tr><td>John</td><td>30</td></tr>
            </table>";
        
        var pdf = renderer.RenderHtmlAsPdf(htmlTable);
        pdf.SaveAs("table.pdf");
    }
}