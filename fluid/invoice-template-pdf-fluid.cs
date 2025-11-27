// NuGet: Install-Package Fluid.Core
using Fluid;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var parser = new FluidParser();
        var template = parser.Parse(@"
            <html><body>
                <h1>Invoice #{{invoiceNumber}}</h1>
                <p>Date: {{date}}</p>
                <p>Customer: {{customer}}</p>
                <p>Total: ${{total}}</p>
            </body></html>");
        
        var context = new TemplateContext();
        context.SetValue("invoiceNumber", "12345");
        context.SetValue("date", DateTime.Now.ToShortDateString());
        context.SetValue("customer", "John Doe");
        context.SetValue("total", 599.99);
        
        var html = await template.RenderAsync(context);
        // Fluid outputs HTML - requires additional PDF library
        File.WriteAllText("invoice.html", html);
    }
}