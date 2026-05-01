// PDFreactor .NET wrapper is a Web Service client (no NuGet package).
// Reference PDFreactor.dll from <PDFreactor-install>/clients/netstandard2/bin/
// and run the PDFreactor Web Service (Java/Jetty) locally or remotely.
// Docs: https://www.pdfreactor.com/product/doc_html/manual-ws.html
using RealObjects.PDFreactor.Webservice.Client;
using System.IO;

class Program
{
    static void Main()
    {
        PDFreactor pdfReactor = new PDFreactor();

        string html = "<html><body><h1>Document with Headers</h1><p>Content here</p></body></html>";

        Configuration config = new Configuration();
        config.Document = html;
        // PDFreactor's CSS Paged Media is its core strength: headers/footers via @page rules.
        config.UserStyleSheets = new System.Collections.Generic.List<Resource>
        {
            new Resource
            {
                Content = "@page { @top-center { content: 'Header Text'; } @bottom-center { content: 'Page ' counter(page); } }"
            }
        };

        Result result = pdfReactor.Convert(config);

        File.WriteAllBytes("document.pdf", result.Document);
    }
}