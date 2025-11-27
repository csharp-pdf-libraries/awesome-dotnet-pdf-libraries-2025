// NuGet: Install-Package PDFreactor.Native.Windows.x64
using RealObjects.PDFreactor;
using System.IO;

class Program
{
    static void Main()
    {
        PDFreactor pdfReactor = new PDFreactor();
        
        string html = "<html><body><h1>Hello World</h1></body></html>";
        
        Configuration config = new Configuration();
        config.Document = html;
        
        Result result = pdfReactor.Convert(config);
        
        File.WriteAllBytes("output.pdf", result.Document);
    }
}