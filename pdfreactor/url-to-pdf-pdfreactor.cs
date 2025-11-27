// NuGet: Install-Package PDFreactor.Native.Windows.x64
using RealObjects.PDFreactor;
using System.IO;

class Program
{
    static void Main()
    {
        PDFreactor pdfReactor = new PDFreactor();
        
        Configuration config = new Configuration();
        config.Document = "https://www.example.com";
        
        Result result = pdfReactor.Convert(config);
        
        File.WriteAllBytes("webpage.pdf", result.Document);
    }
}