// NuGet: Install-Package PDFreactor.Native.Windows.x64
using RealObjects.PDFreactor;
using System.IO;

class Program
{
    static void Main()
    {
        PDFreactor pdfReactor = new PDFreactor();
        
        string html = "<html><body><h1>Document with Headers</h1><p>Content here</p></body></html>";
        
        Configuration config = new Configuration();
        config.Document = html;
        config.AddUserStyleSheet("@page { @top-center { content: 'Header Text'; } @bottom-center { content: 'Page ' counter(page); } }");
        
        Result result = pdfReactor.Convert(config);
        
        File.WriteAllBytes("document.pdf", result.Document);
    }
}