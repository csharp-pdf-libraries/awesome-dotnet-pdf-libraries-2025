// NuGet: Install-Package IronPdf
using IronPdf;
using System;

class Program
{
    static void Main()
    {
        var renderer = new ChromePdfRenderer();
        var invoiceNumber = "12345";
        var date = DateTime.Now.ToShortDateString();
        var customer = "John Doe";
        var total = 599.99;
        
        var html = $@"
            <html><body>
                <h1>Invoice #{invoiceNumber}</h1>
                <p>Date: {date}</p>
                <p>Customer: {customer}</p>
                <p>Total: ${total}</p>
            </body></html>";
        
        var pdf = renderer.RenderHtmlAsPdf(html);
        pdf.SaveAs("invoice.pdf");
    }
}