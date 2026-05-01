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

        // jsreport's chrome-pdf recipe renders the Template.Content HTML.
        // To render an external URL, point the Chrome recipe's Url option
        // at the page (Content can be left as a placeholder).
        var report = await rs.RenderAsync(new RenderRequest()
        {
            Template = new Template()
            {
                Recipe = Recipe.ChromePdf,
                Engine = Engine.None,
                Content = " ",
                Chrome = new Chrome()
                {
                    Url = "https://example.com",
                    WaitForNetworkIddle = true
                }
            }
        });

        using (var fileStream = File.Create("webpage.pdf"))
        {
            report.Content.CopyTo(fileStream);
        }

        Console.WriteLine("Webpage PDF created successfully!");
    }
}