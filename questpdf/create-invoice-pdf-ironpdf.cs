// NuGet: Install-Package IronPdf
using IronPdf;

class Program
{
    static void Main()
    {
        var htmlContent = @"
            <h1>INVOICE</h1>
            <p>Invoice #: 12345</p>
            <br/>
            <p>Customer: John Doe</p>
            <p><strong>Total: $100.00</strong></p>
        ";
        
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf(htmlContent);
        pdf.SaveAs("invoice.pdf");
    }
}