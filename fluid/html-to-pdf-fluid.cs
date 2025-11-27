// NuGet: Install-Package Fluid.Core
using Fluid;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var parser = new FluidParser();
        var template = parser.Parse("<html><body><h1>Hello {{name}}!</h1></body></html>");
        var context = new TemplateContext();
        context.SetValue("name", "World");
        var html = await template.RenderAsync(context);
        
        // Fluid only generates HTML - you'd need another library to convert to PDF
        File.WriteAllText("output.html", html);
    }
}