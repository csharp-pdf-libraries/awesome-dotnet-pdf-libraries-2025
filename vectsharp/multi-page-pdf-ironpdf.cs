// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        
        string html = @"
            <h1>Page 1</h1>
            <p>First page content</p>
            <div style='page-break-after: always;'></div>
            <h1>Page 2</h1>
            <p>Second page content</p>
        ";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("multipage.pdf");
    }
}