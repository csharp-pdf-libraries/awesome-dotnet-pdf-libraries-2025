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
                Content = "<h1>Document with Header and Footer</h1><p>Main content goes here.</p>",
                Chrome = new Chrome()
                {
                    DisplayHeaderFooter = true,
                    HeaderTemplate = "<div style='font-size:10px; text-align:center; width:100%;'>Custom Header</div>",
                    FooterTemplate = "<div style='font-size:10px; text-align:center; width:100%;'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></div>"
                }
            }
        });

        using (var fileStream = File.Create("document_with_headers.pdf"))
        {
            report.Content.CopyTo(fileStream);
        }

        Console.WriteLine("PDF with headers and footers created successfully!");
    }
}