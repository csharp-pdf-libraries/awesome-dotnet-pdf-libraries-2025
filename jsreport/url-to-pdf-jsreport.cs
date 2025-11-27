// NuGet: Install-Package jsreport.Binary
// NuGet: Install-Package jsreport.Local
// NuGet: Install-Package jsreport.Types
using jsreport.Binary;
using jsreport.Local;
using jsreport.Types;
using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var rs = new LocalReporting()
            .UseBinary(JsReportBinary.GetBinary())
            .AsUtility()
            .Create();

        var report = await rs.RenderAsync(new RenderRequest()
        {
            Template = new Template()
            {
                Recipe = Recipe.ChromePdf,
                Engine = Engine.None,
                Content = "<html><body><script>window.location='https://example.com';</script></body></html>"
            }
        });

        using (var fileStream = File.Create("webpage.pdf"))
        {
            report.Content.CopyTo(fileStream);
        }

        Console.WriteLine("Webpage PDF created successfully!");
    }
}